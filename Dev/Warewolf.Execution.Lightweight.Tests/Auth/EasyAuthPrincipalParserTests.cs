/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Security.Claims;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class EasyAuthPrincipalParserTests
{
    private static FakeHttpRequestData NewRequest()
        => new(new TestFunctionContext(), new Uri("https://x.test/secure/hello"));

    private static string EncodePrincipal(string json)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

    private static EasyAuthPrincipalParser NewParser()
        => new(NullLogger<EasyAuthPrincipalParser>.Instance);

    [TestMethod]
    public async Task TST06_ValidHeader_PopulatesNameRolesAndIdp()
    {
        var json = """
        {
          "auth_typ": "aad",
          "claims": [
            { "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "val": "alice@x.com" },
            { "typ": "roles", "val": "TeamA" },
            { "typ": "roles", "val": "Permission.View" },
            { "typ": "roles", "val": "Permission.Execute" }
          ]
        }
        """;
        var req = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, EncodePrincipal(json));

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNotNull(principal);
        Assert.AreEqual("alice@x.com", principal!.UserName);
        Assert.IsTrue(principal.IsInGroup("TeamA"));
        // Permissions are resolved at request time from secure.config, not from token claims.
        // Permission.View / Permission.Execute roles are surfaced as groups only.
        Assert.IsTrue(principal.IsInGroup("Permission.View"));
        Assert.IsTrue(principal.IsInGroup("Permission.Execute"));
        Assert.AreEqual("aad", principal.FindFirst(AuthConstants.IdentityProvider)?.Value);
    }

    [TestMethod]
    public async Task TST06_MissingHeader_ReturnsNull()
    {
        var principal = await NewParser().TryParseAsync(NewRequest(), CancellationToken.None);
        Assert.IsNull(principal);
    }

    [TestMethod]
    public async Task TST06_MalformedBase64_ReturnsNullAndDoesNotThrow()
    {
        var req = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, "!!!not-base64!!!");

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNull(principal);
    }

    [TestMethod]
    public async Task TST06_EmptyHeaderValue_ReturnsNull()
    {
        var req = NewRequest();
        req.AddHeader(AuthConstants.ClientPrincipalHeader, "");

        var principal = await NewParser().TryParseAsync(req, CancellationToken.None);

        Assert.IsNull(principal);
    }
}
