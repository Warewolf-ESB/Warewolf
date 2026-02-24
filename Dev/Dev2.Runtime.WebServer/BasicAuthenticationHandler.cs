#if !NETFRAMEWORK
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Dev2.Runtime.WebServer
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var envUsername = Environment.GetEnvironmentVariable("SERVER_USERNAME");
            var envPassword = Environment.GetEnvironmentVariable("SERVER_PASSWORD");

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                if (authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
                {
                    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                    var username = credentials[0];
                    var password = credentials[1];

                    if (!string.IsNullOrEmpty(envUsername) && !string.IsNullOrEmpty(envPassword) &&
                        username == envUsername && password == envPassword)
                    {
                        const string groupsJson = "{\"UserGroups\": [{\"Name\": \"Administrators\"}, {\"Name\": \"Warewolf Administrators\"}]}";
                        var claims = new[] { 
                            new Claim(ClaimTypes.NameIdentifier, username),
                            new Claim(ClaimTypes.Name, username),
                            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication", groupsJson)
                        };
                        var identity = new ClaimsIdentity(claims, Scheme.Name);
                        var principal = new ClaimsPrincipal(identity);
                        var ticket = new AuthenticationTicket(principal, Scheme.Name);

                        return Task.FromResult(AuthenticateResult.Success(ticket));
                    }
                    else
                    {
                        return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
                    }
                }
                else
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Scheme"));
                }
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"Warewolf\", charset=\"UTF-8\"";
            await base.HandleChallengeAsync(properties);
        }
    }
}
#endif