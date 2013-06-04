using Dev2.DataList.Contract;
using System;
using System.ComponentModel.Composition;

namespace Dev2.Studio.Core.InterfaceImplementors {
    [Export(typeof(IDev2ConfigurationProvider))]
    public class Dev2ConfigurationProvider : IDev2ConfigurationProvider  {

        //private static readonly string _authMode = "Dev2StudioSecurityMode";
        //private static readonly string _ldapServer = "Dev2StudioLDAPEndpoint";

        public string ReadKey(string key) {
            return (System.Configuration.ConfigurationManager.AppSettings[key]);
            //string result = string.Empty;
            //if (key == _authMode)
            //{
            //    result = StringResources.Dev2StudioSecurityMode;
            //}
            //else if (key == _ldapServer)
            //{
            //    result = StringResources.Dev2StudioLDAPEndpoint;
            //}

            //return result;
        }

        public void OnReadFailure() {
            Environment.Exit(1);
        }
    }
}
