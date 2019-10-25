﻿using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class RenewalParameters : IEquatable<RenewalParameters>
    {
        public string WebApp { get; }
        public IReadOnlyList<string> Hosts { get; }
        public string Email { get; }
        public string ServicePlanResourceGroup { get; }
        public string SiteSlotName { get; }
        public string GroupName { get; }
        public AzureEnvironmentParams WebAppEnvironmentParams { get; }
        public AzureEnvironmentParams AzureDnsEnvironmentParams { get; }
        public string AzureDnsZoneName { get; }
        public string AzureDnsRelativeRecordSetName { get; }
        public bool UseIpBasedSsl { get; }
        public int RsaKeyLength { get; }
        public Uri AcmeBaseUri { get; }
        public string WebRootPath { get; }
        public int RenewXNumberOfDaysBeforeExpiration { get; }
        public Uri AuthenticationUri { get; }
        public Uri AzureTokenAudience { get; }
        public Uri AzureManagementEndpoint { get; }
        public string AzureDefaultWebsiteDomainName { get; }

        public RenewalParameters(
            AzureEnvironmentParams webAppEnvironmentParams,
            string webApp,
            IReadOnlyList<string> hosts,
            string email,
            string servicePlanResourceGroup = null,
            string groupName = null,
            string siteSlotName = null,
            AzureEnvironmentParams azureDnsEnvironmentParams = null,
            string azureDnsZoneName = null,
            string azureDnsRelativeRecordSetName = null,
            bool useIpBasedSsl = false,
            int rsaKeyLength = 2048,
            Uri acmeBaseUri = null,
            string webRootPath = null,
            int renewXNumberOfDaysBeforeExpiration = -1,
            Uri authenticationUri = null,
            Uri azureTokenAudience = null,
            Uri azureManagementEndpoint = null,
            string azureDefaultWebsiteDomainName = null)
        {
            WebAppEnvironmentParams = ParamValidator.VerifyNonNull(webAppEnvironmentParams, nameof(webAppEnvironmentParams));
            WebApp = ParamValidator.VerifyString(webApp, nameof(webApp));
            Hosts = ParamValidator.VerifyHosts(hosts, nameof(hosts));
            Email = ParamValidator.VerifyEmail(email, nameof(email));
            ServicePlanResourceGroup = ParamValidator.VerifyOptionalString(servicePlanResourceGroup, nameof(servicePlanResourceGroup));
            GroupName = ParamValidator.VerifyOptionalString(groupName, nameof(groupName));
            SiteSlotName = ParamValidator.VerifyOptionalString(siteSlotName, nameof(siteSlotName));
            AzureDnsEnvironmentParams = azureDnsEnvironmentParams ?? WebAppEnvironmentParams;
            AzureDnsZoneName = ParamValidator.VerifyOptionalString(azureDnsZoneName, nameof(azureDnsZoneName));
            AzureDnsRelativeRecordSetName = ParamValidator.VerifyOptionalString(azureDnsRelativeRecordSetName, nameof(azureDnsRelativeRecordSetName));
            UseIpBasedSsl = useIpBasedSsl;
            RsaKeyLength = ParamValidator.VerifyPositiveInteger(rsaKeyLength, nameof(rsaKeyLength));
            AcmeBaseUri = ParamValidator.VerifyOptionalUri(acmeBaseUri, nameof(acmeBaseUri));
            WebRootPath = ParamValidator.VerifyOptionalString(webRootPath, nameof(webRootPath));
            RenewXNumberOfDaysBeforeExpiration = renewXNumberOfDaysBeforeExpiration;
            AuthenticationUri = ParamValidator.VerifyOptionalUri(authenticationUri, nameof(authenticationUri));
            AzureTokenAudience = ParamValidator.VerifyOptionalUri(azureTokenAudience, nameof(azureTokenAudience));
            AzureManagementEndpoint = ParamValidator.VerifyOptionalUri(azureManagementEndpoint, nameof(azureManagementEndpoint));
            AzureDefaultWebsiteDomainName = ParamValidator.VerifyOptionalHostName(azureDefaultWebsiteDomainName, nameof(azureDefaultWebsiteDomainName));
        }

        public override string ToString()
        {
            return Invariant($"{nameof(WebApp)}: {WebApp}, {nameof(Hosts)}: {String.Join(", ", Hosts)}, {nameof(ServicePlanResourceGroup)}: {ServicePlanResourceGroup}, {nameof(SiteSlotName)}: {SiteSlotName}, {nameof(GroupName)}: {GroupName}, {nameof(WebAppEnvironmentParams)}: {WebAppEnvironmentParams}, {nameof(AzureDnsEnvironmentParams)}: {AzureDnsEnvironmentParams}, {nameof(AzureDnsZoneName)}: {AzureDnsZoneName}, {nameof(AzureDnsRelativeRecordSetName)}: {AzureDnsRelativeRecordSetName}, {nameof(UseIpBasedSsl)}: {UseIpBasedSsl}, {nameof(RsaKeyLength)}: {RsaKeyLength}, {nameof(AcmeBaseUri)}: {AcmeBaseUri}, {nameof(WebRootPath)}: {WebRootPath}, {nameof(RenewXNumberOfDaysBeforeExpiration)}: {RenewXNumberOfDaysBeforeExpiration}, {nameof(AuthenticationUri)}: {AuthenticationUri}, {nameof(AzureTokenAudience)}: {AzureTokenAudience}, {nameof(AzureManagementEndpoint)}: {AzureManagementEndpoint}, {nameof(AzureDefaultWebsiteDomainName)}: {AzureDefaultWebsiteDomainName}");
        }

        public bool Equals(RenewalParameters other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(WebApp, other.WebApp)
                   && Hosts.SequenceEqual(other.Hosts)
                   && string.Equals(Email, other.Email)
                   && string.Equals(ServicePlanResourceGroup, other.ServicePlanResourceGroup)
                   && string.Equals(SiteSlotName, other.SiteSlotName)
                   && string.Equals(GroupName, other.GroupName)
                   && Equals(WebAppEnvironmentParams, other.WebAppEnvironmentParams)
                   && Equals(AzureDnsEnvironmentParams, other.AzureDnsEnvironmentParams)
                   && string.Equals(AzureDnsZoneName, other.AzureDnsZoneName)
                   && string.Equals(AzureDnsRelativeRecordSetName, other.AzureDnsRelativeRecordSetName)
                   && UseIpBasedSsl == other.UseIpBasedSsl
                   && RsaKeyLength == other.RsaKeyLength
                   && Equals(AcmeBaseUri, other.AcmeBaseUri)
                   && string.Equals(WebRootPath, other.WebRootPath)
                   && RenewXNumberOfDaysBeforeExpiration == other.RenewXNumberOfDaysBeforeExpiration
                   && Equals(AuthenticationUri, other.AuthenticationUri)
                   && Equals(AzureTokenAudience, other.AzureTokenAudience)
                   && Equals(AzureManagementEndpoint, other.AzureManagementEndpoint)
                   && string.Equals(AzureDefaultWebsiteDomainName, other.AzureDefaultWebsiteDomainName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is RenewalParameters other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = WebApp != null ? WebApp.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Hosts != null ? Hosts.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Email != null ? Email.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ServicePlanResourceGroup != null ? ServicePlanResourceGroup.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SiteSlotName != null ? SiteSlotName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (GroupName != null ? GroupName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WebAppEnvironmentParams != null ? WebAppEnvironmentParams.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureDnsEnvironmentParams != null ? AzureDnsEnvironmentParams.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureDnsZoneName != null ? AzureDnsZoneName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureDnsRelativeRecordSetName != null ? AzureDnsRelativeRecordSetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ UseIpBasedSsl.GetHashCode();
                hashCode = (hashCode * 397) ^ RsaKeyLength;
                hashCode = (hashCode * 397) ^ (AcmeBaseUri != null ? AcmeBaseUri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WebRootPath != null ? WebRootPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RenewXNumberOfDaysBeforeExpiration;
                hashCode = (hashCode * 397) ^ (AuthenticationUri != null ? AuthenticationUri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureTokenAudience != null ? AzureTokenAudience.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureManagementEndpoint != null ? AzureManagementEndpoint.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureDefaultWebsiteDomainName != null ? AzureDefaultWebsiteDomainName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}