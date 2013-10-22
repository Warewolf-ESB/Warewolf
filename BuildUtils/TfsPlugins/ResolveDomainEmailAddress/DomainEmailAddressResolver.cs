using System.DirectoryServices.AccountManagement;

namespace ResolveDomainEmailAddress
{
    public class DomainEmailAddressResolver
    {
        public string GetEmailAddress(string userName)
        {
            var domainContext = new PrincipalContext(ContextType.Domain, "RSAKLFSVRSBSPDC.dev2.local");
            var user = UserPrincipal.FindByIdentity(domainContext, userName);

            if (user != null)
            {
                return user.EmailAddress.Replace("local","co.za");
            }
            return null;
        }
    }
}
