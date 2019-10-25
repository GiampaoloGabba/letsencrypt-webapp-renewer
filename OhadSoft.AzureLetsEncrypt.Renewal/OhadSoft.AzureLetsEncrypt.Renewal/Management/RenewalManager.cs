﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;
using OhadSoft.AzureLetsEncrypt.Renewal.Configuration;
using OhadSoft.AzureLetsEncrypt.Renewal.Util;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public class RenewalManager : IRenewalManager
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        public const string DefaultWebsiteDomainName = "azurewebsites.net";
        public const string DefaultAcmeBaseUri = "https://acme-v02.api.letsencrypt.org/directory";
        public const string DefaultAuthenticationUri = "https://login.windows.net/";
        public const string DefaultAzureTokenAudienceService = "https://management.core.windows.net/";
        public const string DefaultManagementEndpoint = "https://management.azure.com";
#pragma warning restore S1075 // URIs should not be hardcoded

        private static readonly RNGCryptoServiceProvider s_randomGenerator = new RNGCryptoServiceProvider(); // thread-safe

        public Task Renew(RenewalParameters renewalParams)
        {
            if (renewalParams == null)
            {
                throw new ArgumentNullException(nameof(renewalParams));
            }

            return RenewCore(renewalParams);
        }

        private static async Task RenewCore(RenewalParameters renewalParams)
        {
            Trace.TraceInformation("Generating SSL certificate with parameters: {0}", renewalParams);

            var acmeConfig = GetAcmeConfig(renewalParams);
            var webAppEnvironment = GetWebAppEnvironment(renewalParams);
            var certificateServiceSettings = new CertificateServiceSettings { UseIPBasedSSL = renewalParams.UseIpBasedSsl };
            var azureDnsEnvironment = GetAzureDnsEnvironment(renewalParams);

            if (azureDnsEnvironment != null)
            {
                throw new ConfigurationErrorsException("Azure DNS challenge currently not supported");
            }

            var manager = CertificateManager.CreateKuduWebAppCertificateManager(webAppEnvironment, acmeConfig, certificateServiceSettings, new AuthProviderConfig());
            Trace.TraceInformation("Adding SSL cert for '{0}'...", GetWebAppFullName(renewalParams));

            bool addNewCert = true;
            if (renewalParams.RenewXNumberOfDaysBeforeExpiration > 0)
            {
                var staging = acmeConfig.BaseUri.Contains("staging", StringComparison.OrdinalIgnoreCase);
                var letsEncryptHostNames = await CertificateHelper.GetLetsEncryptHostNames(webAppEnvironment, staging);
                Trace.TraceInformation("Let's Encrypt host names (staging: {0}): {1}", staging, String.Join(", ", letsEncryptHostNames));

                ICollection<string> missingHostNames = acmeConfig.Hostnames.Except(letsEncryptHostNames, StringComparer.OrdinalIgnoreCase).ToArray();
                if (missingHostNames.Count > 0)
                {
                    Trace.TraceInformation(
                        "Detected host name(s) with no associated Let's Encrypt certificates, will add a new certificate: {0}",
                        String.Join(", ", missingHostNames));
                }
                else
                {
                    Trace.TraceInformation("All host names associated with Let's Encrypt certificates, will perform cert renewal");
                    addNewCert = false;
                }
            }

            if (addNewCert)
            {
                await manager.AddCertificate();
            }
            else
            {
                await manager.RenewCertificate(false, renewalParams.RenewXNumberOfDaysBeforeExpiration);
            }

            Trace.TraceInformation("Let's Encrypt SSL certs & bindings renewed for '{0}'", renewalParams.WebApp);
        }

        private static IAzureDnsEnvironment GetAzureDnsEnvironment(RenewalParameters renewalParams)
        {
            var zoneName = renewalParams.AzureDnsZoneName;
            var relativeRecordSetName = renewalParams.AzureDnsRelativeRecordSetName;

            if (zoneName == null || relativeRecordSetName == null)
            {
                Trace.TraceInformation($"Either {nameof(zoneName)} or {nameof(relativeRecordSetName)} are null for {GetWebAppFullName(renewalParams)}, will not use Azure DNS challenge");
                return null;
            }

            Debug.Assert(renewalParams.AzureDnsEnvironmentParams?.SubscriptionId != null, "renewalParams.AzureDnsEnvironmentParams?.SubscriptionId != null");
            Debug.Assert(renewalParams.AzureDnsEnvironmentParams?.ClientId != null, "renewalParams.AzureDnsEnvironmentParams?.ClientId != null");

            return new AzureDnsEnvironment(
                renewalParams.AzureDnsEnvironmentParams.TenantId,
                renewalParams.AzureDnsEnvironmentParams.SubscriptionId.Value,
                renewalParams.AzureDnsEnvironmentParams.ClientId.Value,
                renewalParams.AzureDnsEnvironmentParams.ClientSecret,
                renewalParams.AzureDnsEnvironmentParams.ResourceGroup,
                zoneName,
                relativeRecordSetName);
        }

        private static AzureWebAppEnvironment GetWebAppEnvironment(RenewalParameters renewalParams)
        {
            Debug.Assert(renewalParams.WebAppEnvironmentParams?.SubscriptionId != null, "renewalParams.WebAppEnvironmentParams?.SubscriptionId != null");
            Debug.Assert(renewalParams.WebAppEnvironmentParams?.ClientId != null, "renewalParams.WebAppEnvironmentParams?.ClientId != null");

            return new AzureWebAppEnvironment(
                renewalParams.WebAppEnvironmentParams.TenantId,
                renewalParams.WebAppEnvironmentParams.SubscriptionId.Value,
                renewalParams.WebAppEnvironmentParams.ClientId.Value,
                renewalParams.WebAppEnvironmentParams.ClientSecret,
                renewalParams.WebAppEnvironmentParams.ResourceGroup,
                renewalParams.WebApp,
                renewalParams.ServicePlanResourceGroup,
                renewalParams.SiteSlotName)
            {
                WebRootPath = renewalParams.WebRootPath,
                AzureWebSitesDefaultDomainName = renewalParams.AzureDefaultWebsiteDomainName ?? DefaultWebsiteDomainName,
                AuthenticationEndpoint = renewalParams.AuthenticationUri ?? new Uri(DefaultAuthenticationUri),
                ManagementEndpoint = renewalParams.AzureManagementEndpoint ?? new Uri(DefaultManagementEndpoint),
                TokenAudience = renewalParams.AzureTokenAudience ?? new Uri(DefaultAzureTokenAudienceService),
            };
        }

        private static AcmeConfig GetAcmeConfig(RenewalParameters renewalParams)
        {
            Trace.TraceInformation("Generating secure PFX password for '{0}'...", renewalParams.WebApp);
            var pfxPassData = new byte[32];
            s_randomGenerator.GetBytes(pfxPassData);

            return new AcmeConfig
            {
                Host = renewalParams.Hosts[0],
                AlternateNames = renewalParams.Hosts.Skip(1).ToList(),
                RegistrationEmail = renewalParams.Email,
                RSAKeyLength = renewalParams.RsaKeyLength,
                PFXPassword = Convert.ToBase64String(pfxPassData),
                BaseUri = (renewalParams.AcmeBaseUri ?? new Uri(DefaultAcmeBaseUri)).ToString(),
            };
        }

        private static string GetWebAppFullName(RenewalParameters renewalParams)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}{1}", renewalParams.WebApp, renewalParams.GroupName == null ? String.Empty : $"[{renewalParams.GroupName}]");
        }
    }
}