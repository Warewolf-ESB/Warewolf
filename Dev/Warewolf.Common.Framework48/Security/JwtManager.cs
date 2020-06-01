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
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Services.Security;
using Microsoft.IdentityModel.Tokens;

namespace Warewolf.Security
{
    public class JwtManager : IJwtManager
    {
        private readonly ISecuritySettings _securitySettings;

        public JwtManager(ISecuritySettings securitySettings)
        {
            _securitySettings = securitySettings;
        }

        public string GenerateToken(string payload, int expireMinutes = 20)
        {
            try
            {
                var securitySettingsData = _securitySettings.ReadSettingsFile(new ResourceNameProvider());
                var tokenHandler = new JwtSecurityTokenHandler();
                var symmetricKey = Convert.FromBase64String(securitySettingsData.SecretKey);
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

        public string ValidateToken(string token)
        {
            var simplePrinciple = BuildPrincipal(token);

            var identity = simplePrinciple?.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            if (!identity.IsAuthenticated)
                return null;

            var authClaim = identity.FindFirst(ClaimTypes.Authentication);
            var payload = authClaim?.Value;
            return payload;
        }

        public ClaimsPrincipal BuildPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken is null)
                {
                    return null;
                }

                var securitySettingsData = _securitySettings.ReadSettingsFile(new ResourceNameProvider());
                var symmetricKey = Convert.FromBase64String(securitySettingsData.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(nameof(JwtManager), ex, GlobalConstants.WarewolfError);
                return null;
            }
        }
    }
}
