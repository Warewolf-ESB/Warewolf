using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Dev2.DataList.Contract;
using Unlimited.Framework;

namespace Dev2.Studio.Core {
    public class MockSecurityProvider : IFrameworkSecurityContext {

        string[] _allRoles = new string[]{"Business Design Studio Developers"};
        private string _name;
        private string[] _roles = new string[] {"Business Design Studio Developers"};

        public MockSecurityProvider(string name) {
            _name = name;
        }

        public string[] AllRoles {
            get { return _allRoles; }
        }

        public bool IsUserInRole(string[] roles) {
            return Roles.Intersect(_allRoles).Any(); 
        }

        public string[] Roles {
            get { return _roles; }
        }

        public System.Security.Principal.IIdentity UserIdentity {
            get { return new GenericIdentity(_name); }
        }

        public IDev2ConfigurationProvider ConfigProvider { get; set; }
    }
}
