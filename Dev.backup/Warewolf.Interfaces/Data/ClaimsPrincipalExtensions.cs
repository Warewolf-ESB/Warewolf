/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Warewolf.Data
{
    public static class ClaimsPrincipalExtensions
    {
        public static string[] GetUserGroups(this ClaimsPrincipal claimsPrincipal)
        {
            var claim = claimsPrincipal.Claims.FirstOrDefault(o => o.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication");
            if (claim is null)
            {
                return new string[] { };
            }
            var userGroups = JsonConvert.DeserializeObject<JObject>(claim.Value);
            return userGroups["UserGroups"]
                .Select(o => o["Name"].Value<string>())
                .ToArray();
        }
    }
}