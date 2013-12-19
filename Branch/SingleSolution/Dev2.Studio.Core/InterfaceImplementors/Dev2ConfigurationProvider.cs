using System;
using System.ComponentModel.Composition;
using Dev2.Data.Interfaces;

namespace Dev2.InterfaceImplementors {
    [Export(typeof(IDev2ConfigurationProvider))]
    public class Dev2ConfigurationProvider : IDev2ConfigurationProvider  {

        public string ReadKey(string key) {
            return (System.Configuration.ConfigurationManager.AppSettings[key]);
        }

        public void OnReadFailure() {
            Environment.Exit(1);
        }
    }
}
