/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for EntraBearerTokenValidator — the shared RS256/issuer/audience/
 *  lifetime validation core used by BOTH BearerTokenPrincipalParser (HTTP) and
 *  ServiceBusWorkflowTriggerFunction (Service Bus message tokens).
 *
 *  Notes on scope (mirrors BearerTokenPrincipalParserTests):
 *  Fetching Entra's live OIDC metadata to validate a real/expired/malformed token
 *  requires network access and turns a unit test into an integration test. The
 *  cases here exercise everything reachable deterministically without a network
 *  call:
 *   - IsEnabled / not-configured guard (throws before any network call is made)
 *   - NormalizeClaims — the claim re-projection logic, fully unit-testable in isolation
 *
 *  Live-tenant validation is exercised end-to-end by the integration test project.
 */

using System.Linq;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class EntraBearerTokenValidatorTests
{
    private static EntraAuthOptions Disabled() => new();
    private static EntraAuthOptions Enabled() => new()
    {
        TenantId = "00000000-0000-0000-0000-000000000001",
        Audience = "api://test-app",
    };

    [TestMethod]
    public void IsEnabled_ReflectsBoundOptions()
    {
        Assert.IsFalse(new EntraBearerTokenValidator(Disabled()).IsEnabled);
        Assert.IsTrue(new EntraBearerTokenValidator(Enabled()).IsEnabled);
    }

    [TestMethod]
    public async Task ValidateAsync_Disabled_ThrowsInvalidOperationException_WithoutNetworkCall()
    {
        var validator = new EntraBearerTokenValidator(Disabled());

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => validator.ValidateAsync("any-token", CancellationToken.None));
    }

    [TestMethod]
    public void Constructor_NullOptions_Throws()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new EntraBearerTokenValidator(null!));
    }

    [TestMethod]
    public void NormalizeClaims_MapsOidToNameIdentifier()
    {
        var source = new[] { new Claim("oid", "user-object-id") };

        var normalized = EntraBearerTokenValidator.NormalizeClaims(source).ToList();

        Assert.AreEqual(1, normalized.Count);
        Assert.AreEqual(ClaimTypes.NameIdentifier, normalized[0].Type);
        Assert.AreEqual("user-object-id", normalized[0].Value);
    }

    [TestMethod]
    public void NormalizeClaims_MapsEntraObjectIdentifierUriToNameIdentifier()
    {
        var source = new[] { new Claim(AuthConstants.ObjectIdentifier, "user-object-id") };

        var normalized = EntraBearerTokenValidator.NormalizeClaims(source).Single();

        Assert.AreEqual(ClaimTypes.NameIdentifier, normalized.Type);
    }

    [TestMethod]
    public void NormalizeClaims_MapsNameToClaimTypesName()
    {
        var source = new[] { new Claim("name", "Alice Example") };

        var normalized = EntraBearerTokenValidator.NormalizeClaims(source).Single();

        Assert.AreEqual(ClaimTypes.Name, normalized.Type);
        Assert.AreEqual("Alice Example", normalized.Value);
    }

    [TestMethod]
    public void NormalizeClaims_PreferredUsernamePassesThroughUnchanged()
    {
        // preferred_username (the UPN) must NOT be collapsed onto ClaimTypes.Name —
        // doing so makes it indistinguishable from the "name" (display name) claim,
        // and WorkflowClaimsPrincipal.UserName silently picks whichever claim happens
        // to come first in the token. secure.config's WindowsGroup rows are keyed on
        // UPN/email, so that ambiguity broke permission matching for real callers
        // (observed against warewolfserver-mcp: create_workflow 400s despite a
        // correctly-configured Contribute row for the caller's email).
        var source = new[] { new Claim(AuthConstants.PreferredUsername, "alice@example.com") };

        var normalized = EntraBearerTokenValidator.NormalizeClaims(source).Single();

        Assert.AreEqual(AuthConstants.PreferredUsername, normalized.Type);
        Assert.AreEqual("alice@example.com", normalized.Value);
    }

    [TestMethod]
    public void NormalizeClaims_NameAndPreferredUsername_RemainDistinctClaims()
    {
        // Regression guard: a token carrying both claims (the normal Entra v2.0 shape)
        // must yield two distinct claim types after normalization, not one collapsed
        // ClaimTypes.Name claim whose value depends on source claim order.
        var source = new[]
        {
            new Claim("name", "Alice Example"),
            new Claim(AuthConstants.PreferredUsername, "alice@example.com"),
        };

        var normalized = EntraBearerTokenValidator.NormalizeClaims(source).ToList();

        Assert.AreEqual(2, normalized.Count);
        Assert.AreEqual("Alice Example", normalized.Single(c => c.Type == ClaimTypes.Name).Value);
        Assert.AreEqual("alice@example.com", normalized.Single(c => c.Type == AuthConstants.PreferredUsername).Value);
    }

    [TestMethod]
    public void NormalizeClaims_MapsRolesClaimToClaimTypesRole()
    {
        var source = new[] { new Claim(AuthConstants.Roles, "Developers") };

        var normalized = EntraBearerTokenValidator.NormalizeClaims(source).Single();

        Assert.AreEqual(ClaimTypes.Role, normalized.Type);
        Assert.AreEqual("Developers", normalized.Value);
    }

    [TestMethod]
    public void NormalizeClaims_PassesThroughUnknownClaimTypesUnchanged()
    {
        var source = new[] { new Claim("jti", "token-id-123"), new Claim(AuthConstants.Scope, "user_impersonation") };

        var normalized = EntraBearerTokenValidator.NormalizeClaims(source).ToList();

        Assert.AreEqual(2, normalized.Count);
        Assert.AreEqual("jti", normalized[0].Type);
        Assert.AreEqual("token-id-123", normalized[0].Value);
        Assert.AreEqual(AuthConstants.Scope, normalized[1].Type);
    }
}
