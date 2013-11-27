using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Security.Principal;
using System.Configuration;
using Dev2.DataList.Contract;

namespace Dev2 {
    public class IFrameworkSecurityProviderDefaultImpl : IFrameworkSecurityContext {
        PrincipalContext context;

        readonly List<string> _roles;
        readonly List<string> _allRoles;

        public IFrameworkSecurityProviderDefaultImpl(string domainName) {

            UserName = WindowsIdentity.GetCurrent().Name;
            _roles = new List<string>();
            _allRoles = new List<string>();

            BuildPrincipalFromDirectoryServices(domainName);

        }

        public string UserName { get; set; }
        public string EmailAddress { get; set; }

        public IDev2ConfigurationProvider ConfigProvider { get; set; }

        #region IFrameworkSecurityProvider Members

        public IIdentity UserIdentity {
            get {
                return new GenericIdentity(UserName);
            }
        }
        public string[] Roles {
            get {
                return _roles.ToArray();
            }
        }


        public string[] AllRoles {
            get {
                return _allRoles.ToArray();
            }
        }

        public bool IsUserInRole(string[] roles) {
            return Roles.Intersect(roles).Count() > 0;
        }

        private void BuildPrincipalFromDirectoryServices(string domainName) {
            context = new PrincipalContext(ContextType.Domain);

            var domain = new DirectoryEntry(string.Format("LDAP://{0}", domainName));

            var search = new DirectorySearcher(domain);
            search.Filter = "(&(objectClass=group))";
            search.SearchScope = SearchScope.Subtree;
            var searchResults = search.FindAll();

            List<string> adGroups = new List<string>();

            if (searchResults.Count > 0) {
                foreach (SearchResult searchResult in searchResults) {
                    adGroups.Add(searchResult.GetDirectoryEntry().Name.Replace("CN=", ""));
                }

                adGroups.ToArray().Where(c => !c.Contains("@") && !c.Contains("SQL"))
                    .ToList()
                    .ForEach(c => _allRoles.Add(c));
            }

            var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, UserIdentity.Name);

            if (user == null) {
                throw new UnauthorizedAccessException("Access Denied");
            }

            this.EmailAddress = user.EmailAddress;

            var groups = user.GetGroups().ToList();
            groups.ForEach(c => _roles.Add(c.Name));
        }


        #endregion
    }
}
