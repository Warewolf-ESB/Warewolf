/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dev2.Common;
using Dev2.Services.Security;
using Microsoft.IdentityModel.Tokens;

namespace Dev2.Runtime.WebServer
{
    public static class JwtManager
    {
        public static string GenerateToken(string payload, int expireMinutes = 20)
        {
            try
            {
                var settings = new SecuritySettings();
                var tokenHandler = new JwtSecurityTokenHandler();
                var symmetricKey = Convert.FromBase64String(settings.SecuritySettingsData.SecretKey);
                var now = DateTime.UtcNow;
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Authentication, payload)
                    }),

                    Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(symmetricKey),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var sToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(sToken);
                return token;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(nameof(JwtManager), ex, GlobalConstants.WarewolfError);
                return null;
            }
        }

        public static string ValidateToken(string token)
        {

            var payload = "";
            var simplePrinciple = GetPrincipal(token);

            var identity = simplePrinciple?.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            if (!identity.IsAuthenticated)
                return null;

            var authClaim = identity.FindFirst(ClaimTypes.Authentication);
            payload = authClaim?.Value;
            return payload;
        }

        private static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var settings = new SecuritySettings();
                var symmetricKey = Convert.FromBase64String(settings.SecuritySettingsData.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
                return principal;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(nameof(JwtManager), ex, GlobalConstants.WarewolfError);
                return null;
            }
        }
    }
}