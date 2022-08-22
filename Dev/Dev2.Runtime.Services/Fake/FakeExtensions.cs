using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime
{
    internal static class FakeExtensions
    {
        public static AppDomain CreateDomain(string friendlyName, System.Security.Policy.Evidence evidence , AppDomainSetup setup)
        {
            return AppDomain.CreateDomain(friendlyName);
        }
    }
}
