/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  An engine configured with tenantId "common" or "organizations" accepts sign-ins from any
 *  tenant (the shared warewolfserver, which every Web Studio user gets, including personal
 *  Microsoft accounts). Covers EntraTenants, the issuer check EntraBearerTokenValidator uses for
 *  those aliases, and the tid check in EntraTokenValidator.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class EntraTenantsTests
{
    const string OrgTenant = "ca0cc53b-9af4-4067-bcdf-be9c648450d1";
    const string OtherTenant = "11111111-2222-3333-4444-555555555555";
    const string Consumers = EntraTenants.ConsumersTenantId;

    [TestMethod]
    [DataRow("common", true)]
    [DataRow(" Common ", true)]
    [DataRow("organizations", true)]
    [DataRow(OrgTenant, false)]
    [DataRow("consumers", false)]
    [DataRow(null, false)]
    public void IsMultiTenant_OnlyForTheAnyTenantAliases(string? configured, bool expected) =>
        Assert.AreEqual(expected, EntraTenants.IsMultiTenant(configured));

    [TestMethod]
    [DataRow("common", OrgTenant, true)]
    [DataRow("common", Consumers, true)]
    [DataRow("organizations", OtherTenant, true)]
    [DataRow("organizations", Consumers, false)]
    [DataRow(OrgTenant, OrgTenant, true)]
    [DataRow(OrgTenant, Consumers, false)]
    [DataRow("common", null, false)]
    [DataRow("common", "not-a-guid", false)]
    [DataRow(null, OrgTenant, false)]
    public void Accepts_MatchesTheTokensTenant(string? configured, string? tokenTenant, bool expected) =>
        Assert.AreEqual(expected, EntraTenants.Accepts(configured, tokenTenant));

    [TestMethod]
    public void EntraAuthOptions_Common_IsMultiTenantAndUsesTheCommonMetadata()
    {
        var options = new EntraAuthOptions { TenantId = "common", Audience = "api://app" };

        Assert.IsTrue(options.IsEnabled);
        Assert.IsTrue(options.IsMultiTenant);
        StringAssert.Contains(options.MetadataAddress, "/common/v2.0/");
    }

    [TestMethod]
    public void EntraAuthOptions_TenantGuid_KeepsItsFixedIssuers()
    {
        var options = new EntraAuthOptions { TenantId = OrgTenant, Audience = "api://app" };

        Assert.IsFalse(options.IsMultiTenant);
        CollectionAssert.AreEquivalent(
            new[] { $"https://login.microsoftonline.com/{OrgTenant}/v2.0", $"https://sts.windows.net/{OrgTenant}/" },
            options.ValidIssuers.ToArray());
    }

    [TestMethod]
    public void ValidateMultiTenantIssuer_PersonalAccountOnCommon_IsAccepted()
    {
        var issuer = $"https://login.microsoftonline.com/{Consumers}/v2.0";

        Assert.AreEqual(issuer, EntraBearerTokenValidator.ValidateMultiTenantIssuer("common", issuer, Jwt(issuer, Consumers)));
    }

    [TestMethod]
    public void ValidateMultiTenantIssuer_V1IssuerFromAnotherOrganization_IsAccepted()
    {
        var issuer = $"https://sts.windows.net/{OtherTenant}/";

        Assert.AreEqual(issuer, EntraBearerTokenValidator.ValidateMultiTenantIssuer("organizations", issuer, Jwt(issuer, OtherTenant)));
    }

    [TestMethod]
    public void ValidateMultiTenantIssuer_PersonalAccountOnOrganizations_IsRejected()
    {
        var issuer = $"https://login.microsoftonline.com/{Consumers}/v2.0";

        Assert.ThrowsException<SecurityTokenInvalidIssuerException>(() =>
            EntraBearerTokenValidator.ValidateMultiTenantIssuer("organizations", issuer, Jwt(issuer, Consumers)));
    }

    [TestMethod]
    public void ValidateMultiTenantIssuer_IssuerForADifferentTenantThanTid_IsRejected()
    {
        var issuer = $"https://login.microsoftonline.com/{OrgTenant}/v2.0";

        Assert.ThrowsException<SecurityTokenInvalidIssuerException>(() =>
            EntraBearerTokenValidator.ValidateMultiTenantIssuer("common", issuer, Jwt(issuer, OtherTenant)));
    }

    [TestMethod]
    public void ValidateMultiTenantIssuer_NonMicrosoftIssuer_IsRejected()
    {
        const string issuer = "https://evil.example.com/v2.0";

        Assert.ThrowsException<SecurityTokenInvalidIssuerException>(() =>
            EntraBearerTokenValidator.ValidateMultiTenantIssuer("common", issuer, Jwt(issuer, OtherTenant)));
    }

    [TestMethod]
    public void ValidateMultiTenantIssuer_NoTidClaim_IsRejected()
    {
        var issuer = $"https://login.microsoftonline.com/{OrgTenant}/v2.0";

        Assert.ThrowsException<SecurityTokenInvalidIssuerException>(() =>
            EntraBearerTokenValidator.ValidateMultiTenantIssuer("common", issuer, Jwt(issuer, null)));
    }

    [TestMethod]
    public void GetRolesFromToken_CommonTenant_AcceptsAPersonalAccount()
    {
        var token = RawToken($"https://login.microsoftonline.com/{Consumers}/v2.0", Consumers);

        Assert.IsNotNull(EntraTokenValidator.GetRolesFromToken("Bearer " + token, "common", "api://app"));
    }

    [TestMethod]
    public void GetRolesFromToken_OrganizationsTenant_RejectsAPersonalAccount()
    {
        var token = RawToken($"https://login.microsoftonline.com/{Consumers}/v2.0", Consumers);

        Assert.IsNull(EntraTokenValidator.GetRolesFromToken("Bearer " + token, "organizations", "api://app"));
    }

    static JwtSecurityToken Jwt(string issuer, string? tenantId)
    {
        var claims = tenantId is null ? Array.Empty<Claim>() : new[] { new Claim("tid", tenantId) };
        return new JwtSecurityToken(issuer: issuer, audience: "api://app", claims: claims);
    }

    static string RawToken(string issuer, string tenantId)
    {
        var payload = new JObject
        {
            [EntraTokenValidator.IssuerClaim] = issuer,
            [EntraTokenValidator.ExpiryClaim] = DateTimeOffset.UtcNow.AddMinutes(20).ToUnixTimeSeconds(),
            [EntraTokenValidator.TenantIdClaim] = tenantId,
            [EntraTokenValidator.AudienceClaim] = "api://app",
        };
        static string B64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return B64("""{"alg":"RS256","typ":"JWT"}""") + "." + B64(payload.ToString(Formatting.None)) + "." + B64("sig");
    }
}
