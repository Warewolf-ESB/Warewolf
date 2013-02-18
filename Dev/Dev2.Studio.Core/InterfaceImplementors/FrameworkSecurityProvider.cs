using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unlimited.Framework;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Security.Principal;
using System.Configuration;
using System.ComponentModel.Composition;
using System.Windows;
using Dev2.Studio.Core.ViewModels;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Threading;
using Dev2.DataList.Contract;

namespace Dev2.Studio.Core {
    [Export(typeof(IFrameworkSecurityContext))]
    public class FrameworkSecurityProvider : IFrameworkSecurityContext {
        PrincipalContext context;

        public IPopUp _popUp;
        [Import]
        public IPopUp mb {
            get {
                return _popUp;
            }
            set {
                _popUp = value;
            }
        }

        public IDev2ConfigurationProvider _config;

        [Import]
        public IDev2ConfigurationProvider ConfigProvider {
            get {
                return _config;
            }
            set {
                _config = value;
            }
        }

        private static readonly string _authMode = "Dev2StudioSecurityMode";
        private static readonly string _ldapServer = "Dev2StudioLDAPEndpoint";

        public FrameworkSecurityProvider() {}

        // Must be called!!!!
        private void Init() {

            _username = WindowsIdentity.GetCurrent().Name;

            string mode = GetSettingFromKey(_authMode);

            if (mode != null) {
                InitAuthMode(mode.ToLower());
            }
        }

        private void InitAuthMode(string mode){
            if (mode == "ldap") {
                try {
                    TraceWriter.WriteTrace("Using LDAP mode for studio");
                    UseLdapAuth();
                }
                catch (Exception e) {
                    mb.Description = "An error occured while contacting the LDAP server " + GetSettingFromKey(_ldapServer) + "\nError : " + e.Message;
                    mb.Header = "Error Loading Studio";
                    mb.Buttons = MessageBoxButton.OK;
                    mb.ImageType = MessageBoxImage.Exclamation;
                    mb.Show();
                    ConfigProvider.OnReadFailure();
                }
            }
            else if (mode == "offline") {
                _email = "nobody@localhost.local";

                string[] offlineRoles = { "Schema Admins" 
                                      ,"Enterprise Admins"
                                      ,"Domain Admins"
                                      ,"Domain Users"
                                      ,"Windows SBS Remote Web Workplace Users"
                                      ,"Windows SBS Fax Users"
                                      ,"Windows SBS Fax Administrators"
                                      ,"Windows SBS Virtual Private Network Users"
                                      ,"All Users"
                                      ,"Windows SBS Administrators"
                                      ,"Windows SBS SharePoint_OwnersGroup"
                                      ,"Windows SBS Link Users"
                                      ,"Windows SBS Admin Tools Group"
                                      ,"Company Users"
                                      ,"Business Design Studio Developers" };

                // copy all offline roles over
                _allRoles = new String[offlineRoles.Length];
                _roles = new String[offlineRoles.Length];

                Array.Copy(offlineRoles, AllRoles, offlineRoles.Length);
                Array.Copy(offlineRoles, Roles, offlineRoles.Length);

                TraceWriter.WriteTrace("Using Offline mode for studio");
            }
            else {
                mb.Description = "Error reading authentication mode";
                mb.Header = "Error Loading Studio";
                mb.Buttons = MessageBoxButton.OK;
                mb.ImageType = MessageBoxImage.Exclamation;
                mb.Show();
                ConfigProvider.OnReadFailure();
            }
        }

        #region Contact Active Directory and assembly security context for the application

        /// <summary>
        /// New for PBI 4455.
        /// 2012-09-25, Brendon Page, Modified to read from the string resources instead of the app.config.
        ///                           This was done so that the setting is embedded into the exe.
        /// </summary>
        private string GetSettingFromKey(string key) {
            string result = string.Empty;

            try
            {
                result = ConfigProvider.ReadKey(key);
                //result = System.Configuration.ConfigurationManager.AppSettings[key];
            }
            catch (Exception e)
            {
                mb.Description = "An error occured while reading the security configuration.\nError : " + e.Message;
                mb.Header = "Error Loading Studio";
                mb.Buttons = MessageBoxButton.OK;
                mb.ImageType = MessageBoxImage.Exclamation;
                mb.Show();
                ConfigProvider.OnReadFailure();
            }

            return result;
        }

        /// <summary>
        /// Refactored out for PBI 4455
        /// </summary>
        private void UseLdapAuth() {
            context = new PrincipalContext(ContextType.Domain);

            // Dev2StudioLDAPEndpoint
            string ldapServer = GetSettingFromKey(_ldapServer);

            // OLD:  "LDAP://dev2.local"
            var domain = new DirectoryEntry(ldapServer);

            var search = new DirectorySearcher(domain);
            search.Filter = "(&(objectClass=group))";
            search.SearchScope = SearchScope.Subtree;
            var searchResults = search.FindAll();

            List<string> adGroups = new List<string>();

            string tmpGrps = string.Empty;

            adGroups.ForEach(g => {
                tmpGrps += g.ToString();

            });

            if (searchResults.Count > 0) {
                foreach (SearchResult searchResult in searchResults) {
                    adGroups.Add(searchResult.GetDirectoryEntry().Name.Replace("CN=", ""));
                }
                _allRoles = adGroups.ToArray().Where(c => !c.Contains("@") && !c.Contains("SQL")).ToArray();
            }

            var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, UserIdentity.Name);

            if (user == null) {
                throw new UnauthorizedAccessException("Access Denied");
            }

            _email = user.EmailAddress;

            var groups = user.GetGroups().ToList();
            adGroups = new List<string>();
            groups.ForEach(c => adGroups.Add(c.Name));
            _roles = adGroups.ToArray();

        }

        #endregion

        private string _username;
        public string UserName {
            get {
                if (_username == null) {
                    Init();
                }
                return _username;
            }
        }

        private string _email;
        public string EmailAddress {
            get {
                if (_email == null) {
                    Init();
                }
                return _email;
            }
        }

        #region IFrameworkSecurityProvider Members

        public IIdentity UserIdentity {
            get {
                if (_username == null) {
                    Init();
                }
                return new GenericIdentity(UserName);
            }
        }

        private string[] _roles;

        public string[] Roles {
            get {
                if (_roles == null) {
                    Init();
                }

                return _roles;
            }
        }

        private string[] _allRoles;

        public string[] AllRoles {
            get {
                if (_allRoles == null) {
                    Init();
                }
                return _allRoles;
            }
        }


        public bool IsUserInRole(string[] roles) {
            return Roles != null && Roles.Intersect(roles).Any();
        }
        #endregion
    }
}
