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
            var claim = claimsPrincipal.Claims.First(o => o.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication");
            var userGroups = JsonConvert.DeserializeObject<JObject>(claim.Value);
            return userGroups["UserGroups"]
                .Select(o => o["Name"].Value<string>())
                .ToArray();
        }
    }
}