using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.ViewModels;

namespace Dev2.Studio.Core
{
    [Export(typeof(IFrameworkSecurityContext))]
    public class FrameworkSecurityProvider : IFrameworkSecurityContext
    {
        static readonly string _authMode = "Dev2StudioSecurityMode";

        static readonly string _ldapServer = "Dev2StudioLDAPEndpoint";

        string[] _allRoles;

        public IDev2ConfigurationProvider _config;

        string _email;

        IIdentity _identity;

        public IPopupController _popUp;

        string[] _roles;

        string _username;

        PrincipalContext _context;

        [Import]
        public IPopupController mb
        {
            get
            {
                return _popUp;
            }
            set
            {
                _popUp = value;
            }
        }

        [Import]
        public IDev2ConfigurationProvider ConfigProvider
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
            }
        }

        public string UserName
        {
            get
            {
                if(_username == null)
                {
                    Init();
                }
                return _username;
            }
        }

        public string EmailAddress
        {
            get
            {
                if(_email == null)
                {
                    Init();
                }
                return _email;
            }
        }

        #region IFrameworkSecurityContext Members

        public IIdentity UserIdentity
        {
            get
            {
                if(_identity == null)
                {
                    Init();
                }
                return _identity;
            }
        }

        public string[] Roles
        {
            get
            {
                if(_roles == null)
                {
                    Init();
                }

                return _roles;
            }
        }

        public string[] AllRoles
        {
            get
            {
                if(_allRoles == null)
                {
                    Init();
                }
                return _allRoles;
            }
        }

        public bool IsUserInRole(string[] roles)
        {
            return Roles != null && Roles.Intersect(roles).Any();
        }

        #endregion

        void Init()
        {
            _identity = WindowsIdentity.GetCurrent();
            _username = _identity.Name;

            string mode = GetSettingFromKey(_authMode);

            if(mode != null)
            {
                InitAuthMode(mode.ToLower());
            }
        }

        void InitAuthMode(string mode)
        {
            if(mode == "ldap")
            {
                try
                {
                    StudioLogger.LogMessage("Using LDAP mode for studio");
                    UseLdapAuth();
                }
                catch(Exception e)
                {
                    mb.Description = "An error occured while contacting the LDAP server " + GetSettingFromKey(_ldapServer) + "\nError : " + e.Message;
                    mb.Header = "Error Loading Studio";
                    mb.Buttons = MessageBoxButton.OK;
                    mb.ImageType = MessageBoxImage.Exclamation;
                    mb.Show();
                    ConfigProvider.OnReadFailure();
                }
            }
            else if(mode == "offline")
            {
                _email = "nobody@localhost.local";

                string[] offlineRoles =
                {
                    "Schema Admins"
                    , "Enterprise Admins"
                    , "Domain Admins"
                    , "Domain Users"
                    , "Windows SBS Remote Web Workplace Users"
                    , "Windows SBS Fax Users"
                    , "Windows SBS Fax Administrators"
                    , "Windows SBS Virtual Private Network Users"
                    , "All Users"
                    , "Windows SBS Administrators"
                    , "Windows SBS SharePoint_OwnersGroup"
                    , "Windows SBS Link Users"
                    , "Windows SBS Admin Tools Group"
                    , "Company Users"
                    , "Business Design Studio Developers"
                };

                // copy all offline roles over
                _allRoles = new String[offlineRoles.Length];
                _roles = new String[offlineRoles.Length];

                Array.Copy(offlineRoles, AllRoles, offlineRoles.Length);
                Array.Copy(offlineRoles, Roles, offlineRoles.Length);
                StudioLogger.LogMessage("Using Offline mode for studio");
            }
            else
            {
                if (mb != null)
                {
                    mb.Description = "Error reading authentication mode";
                    mb.Header = "Error Loading Studio";
                    mb.Buttons = MessageBoxButton.OK;
                    mb.ImageType = MessageBoxImage.Exclamation;
                    mb.Show();
                }
                ConfigProvider.OnReadFailure();
            }
        }

        #region Contact Active Directory and assembly security context for the application

        /// <summary>
        ///     New for PBI 4455.
        ///     2012-09-25, Brendon Page, Modified to read from the string resources instead of the app.config.
        ///     This was done so that the setting is embedded into the exe.
        /// </summary>
        string GetSettingFromKey(string key)
        {
            string result = string.Empty;

            try
            {
                result = ConfigProvider.ReadKey(key);
                //result = System.Configuration.ConfigurationManager.AppSettings[key];
            }
            catch(Exception e)
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
        ///     Refactored out for PBI 4455
        /// </summary>
        void UseLdapAuth()
        {
            _context = new PrincipalContext(ContextType.Domain);

            // Dev2StudioLDAPEndpoint
            string ldapServer = GetSettingFromKey(_ldapServer);

            // OLD:  "LDAP://dev2.local"
            var domain = new DirectoryEntry(ldapServer);

            var search = new DirectorySearcher(domain);
            search.Filter = "(&(objectClass=group))";
            search.SearchScope = SearchScope.Subtree;
            SearchResultCollection searchResults = null;
            try
            {
                searchResults = search.FindAll();
            }
            catch (COMException e)
            {
                
            }

            List<string> adGroups = new List<string>();

            string tmpGrps = string.Empty;

            adGroups.ForEach(g => { tmpGrps += g.ToString(); });

            if(searchResults != null && searchResults.Count > 0)
            {
                foreach(SearchResult searchResult in searchResults)
                {
                    adGroups.Add(searchResult.GetDirectoryEntry().Name.Replace("CN=", ""));
                }
                _allRoles = adGroups.ToArray().Where(c => !c.Contains("@") && !c.Contains("SQL")).ToArray();
            }

            var user = UserPrincipal.FindByIdentity(_context, IdentityType.SamAccountName, UserIdentity.Name);

            if(user == null)
            {
                throw new UnauthorizedAccessException("Access Denied");
            }

            _email = user.EmailAddress;

            var groups = user.GetGroups().ToList();
            adGroups = new List<string>();
            groups.ForEach(c => adGroups.Add(c.Name));
            _roles = adGroups.ToArray();
        }

        #endregion
    }
}