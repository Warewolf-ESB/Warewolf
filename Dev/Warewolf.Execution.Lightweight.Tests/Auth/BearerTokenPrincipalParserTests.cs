/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  TST-07 — BearerTokenPrincipalParser tests
 *
 *  Notes on scope:
 *  The parser delegates RS256 signature/issuer/audience/lifetime checks to
 *  Microsoft.IdentityModel via an OIDC ConfigurationManager that fetches signing
 *  keys from the live tenant.  Reaching that path in unit tests would require
 *  network access and a real signing key — which would turn this into an
 *  integration test.  The cases covered here exercise every code branch that
 *  can be reached deterministically without a live network:
 *
 *   • missing Authorization header           → null
 *   • non-Bearer scheme                      → null
 *   • Bearer present but Entra disabled      → null + warning
 *   • Bearer present and Entra enabled but
 *     token is malformed                     → null (caught & swallowed)
 *
 *  Live-tenant validation (valid/expired/wrong aud/iss) is exercised end-to-end
 *  by the integration test project.
 */

using System;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class BearerTokenPrincipalParserTests
{
    private static FakeHttpRequestData NewRequest()
        => new(new TestFunctionContext(), new Uri("https://x.test/secure/hello"));

    private static BearerTokenPrincipalParser NewParser(EntraAuthOptions options)
        => new(options, NullLogger<BearerTokenPrincipalParser>.Instance);

    private static EntraAuthOptions Disabled() => new();
    private static EntraAuthOptions Enabled()  => new()
    {
        TenantId = "00000000-0000-0000-0000-000000000001",
        Audience = "api://test-app",
    };

    [TestMethod]
    public async Task TST07_MissingAuthorizationHeader_ReturnsNull()
    {
        var principal = await NewParser(Enabled()).TryParseAsync(NewRequest(), CancellationToken.None);
        Assert.IsNull(principal);
    }

    [TestMethod]
    public async Task TST07_NonBearerScheme_ReturnsNull()
    {
        var req = NewRequest();
        req.AddHeader("Authorization", "Basic dXNlcjpwYXNz");

        var principal = await NewParser(Enabled()).TryParseAsync(req, CancellationToken.None);

        Assert.IsNull(principal);
    }

    [TestMethod]
    public async Task TST07_OptionsDisabled_ReturnsNull()
    {
        var req = NewRequest();
        req.AddHeader("Authorization", "Bearer dummy.token.value");

        var principal = await NewParser(Disabled()).TryParseAsync(req, CancellationToken.None);

        Assert.IsNull(principal);
    }

    [TestMethod]
    public async Task TST07_MalformedToken_ReturnsNullWithoutThrowing()
    {
        var req = NewRequest();
        req.AddHeader("Authorization", "Bearer not-a-real-jwt");

        var principal = await NewParser(Enabled()).TryParseAsync(req, CancellationToken.None);

        Assert.IsNull(principal);
    }

    [TestMethod]
    public void TST07_EntraOptions_ValidIssuersAndAudiences_AreCorrectlyDerived()
    {
        var options = Enabled();
        CollectionAssert.Contains(
            (System.Collections.ICollection)options.ValidIssuers,
            "https://login.microsoftonline.com/00000000-0000-0000-0000-000000000001/v2.0");
        CollectionAssert.Contains(
            (System.Collections.ICollection)options.ValidAudiences,
            "api://test-app");
        Assert.IsTrue(options.IsEnabled);
    }

    [TestMethod]
    public void TST07_EntraOptions_NotEnabled_WhenTenantIdMissing()
    {
        var options = new EntraAuthOptions { Audience = "api://x" };
        Assert.IsFalse(options.IsEnabled);
    }
}
