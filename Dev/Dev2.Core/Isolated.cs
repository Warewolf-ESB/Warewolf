using System;
using System.Configuration;
using System.IO;

namespace Dev2
{
    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        private AppDomain _domain;
        private readonly T _value;

        public Isolated()
        {
            var appDomainSetup = new AppDomainSetup();
            System.Configuration.Configuration conf = ConfigurationManager.OpenExeConfiguration("D:\\Ambledown\\DLLs\\Ambledown.Neo.Integration.Services.dll");
            if (File.Exists(conf.FilePath))
            {
                appDomainSetup.ConfigurationFile = conf.FilePath;
            }

            
            _domain = AppDomain.CreateDomain("Isolated:" + Guid.NewGuid(),
                null, appDomainSetup);

            Type type = typeof(T);

            _value = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        private void SetAppDomainConfiguration(string assemblyLocation)
        {
            System.Configuration.Configuration conf = ConfigurationManager.OpenExeConfiguration("D:\\Ambledown\\DLLs\\Ambledown.Neo.Integration.Services.dll");
            if (File.Exists(conf.FilePath))
            {
                AppDomain.CurrentDomain.SetupInformation.ConfigurationFile = conf.FilePath;
            }
        }
        public T Value => _value;

        public void Dispose()
        {
            if (_domain != null)
            {
                AppDomain.Unload(_domain);

                _domain = null;
            }
        }
    }
}