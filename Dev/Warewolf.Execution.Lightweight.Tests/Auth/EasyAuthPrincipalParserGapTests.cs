/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Gap tests for EasyAuthPrincipalParser — covering:
 *  - Valid JSON where the 'claims' property is absent
 *  - Claims array elements with empty/null typ — should be skipped
 *  - objectidentifier claim type normalised to NameIdentifier
 *  - auth_typ absent — no idp claim added
 *  - Malformed JSON after valid base64
 *  - Name property
 */

using System;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class EasyAuthPrincipalParserGapTests
{
    private static FakeHttpRequestData NewRequest()
        => new(new TestFunctionContext(), new Uri("https://x.test/secure/hello"));

    private static string Encode(string json)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

    private static EasyAuthPrincipalParser NewParser()
        => new(NullLogger<EasyAuthPrincipalParser>.Instance);

    // ── Name property ─────────────────────────────────────────────────────────

    [TestMethod]
    public void Name_ReturnsEasyAuth()
    {
        Assert.AreEqual("EasyAuth", NewParser().Name);
    }

    // ── auth_typ absent ───────────────────────────────────────────────────────

    [TestMethod]
    public async Task ValidHeader_AuthTypAbsent_ReturnsAuthenticatedPrincipalWithNoIdpClaim()
    {
        var json = """{"claims":[{"typ":"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name","val":"alice@x.com"}]}""";
        var req  = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, Encode(json));

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNotNull(principal);
        Assert.IsTrue(principal!.Identity?.IsAuthenticated);
        Assert.IsNull(principal.FindFirst(AuthConstants.IdentityProvider));
    }

    // ── 'claims' property absent ─────────────────────────────────────────────

    [TestMethod]
    public async Task ValidHeader_ClaimsPropertyAbsent_ReturnsAuthenticatedPrincipalWithIdpOnly()
    {
        // auth_typ is present but 'claims' is missing → only idp claim added
        var json = """{"auth_typ":"aad"}""";
        var req  = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, Encode(json));

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNotNull(principal);
        Assert.IsTrue(principal!.Identity?.IsAuthenticated);
        Assert.AreEqual("aad", principal.FindFirst(AuthConstants.IdentityProvider)?.Value);
    }

    // ── Empty/null typ in claims array ───────────────────────────────────────

    [TestMethod]
    public async Task ValidHeader_ClaimWithEmptyTyp_IsSkipped()
    {
        // A claim entry with empty 'typ' must not be added to the identity
        var json = """
        {
          "auth_typ": "aad",
          "claims": [
            { "typ": "", "val": "should-be-skipped" },
            { "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "val": "alice@x.com" }
          ]
        }
        """;
        var req = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, Encode(json));

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNotNull(principal);
        // Only the name claim should survive, not the empty-typ entry
        Assert.AreEqual("alice@x.com", principal!.UserName);
    }

    // ── objectidentifier normalisation ────────────────────────────────────────

    [TestMethod]
    public async Task ValidHeader_ObjectIdentifierClaim_IsNormalisedToNameIdentifier()
    {
        var json = """
        {
          "auth_typ": "aad",
          "claims": [
            { "typ": "http://schemas.microsoft.com/identity/claims/objectidentifier", "val": "oid-abc" }
          ]
        }
        """;
        var req = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, Encode(json));

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNotNull(principal);
        Assert.AreEqual("oid-abc", principal!.UserId,
            "objectidentifier should be surfaced as UserId via NameIdentifier normalisation");
    }

    // ── Malformed JSON after valid base64 ─────────────────────────────────────

    [TestMethod]
    public async Task MalformedJsonAfterValidBase64_ReturnsNull()
    {
        // Valid base64 but not valid JSON
        var notJson = "this-is-not-json";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(notJson));
        var req     = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, encoded);

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNull(principal);
    }

    // ── Roles claim normalisation ─────────────────────────────────────────────

    [TestMethod]
    public async Task ValidHeader_RolesClaim_IsSurfacedAsGroup()
    {
        var json = """
        {
          "auth_typ": "aad",
          "claims": [
            { "typ": "roles", "val": "TeamA" }
          ]
        }
        """;
        var req = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, Encode(json));

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNotNull(principal);
        Assert.IsTrue(principal!.IsInGroup("TeamA"));
    }
}
