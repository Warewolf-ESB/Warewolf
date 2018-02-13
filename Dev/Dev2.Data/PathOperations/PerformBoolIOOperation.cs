using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformBoolIOOperation
    {
        public static SafeTokenHandle DoLogOn(IDev2LogonProvider dev2Logon, IActivityIOPath path) => dev2Logon.DoLogon(path);
        public static SafeTokenHandle RequiresAuth(IActivityIOPath path, IDev2LogonProvider dev2LogonProvider) => string.IsNullOrEmpty(path.Username) ? null : DoLogOn(dev2LogonProvider, path);
        public abstract bool ExecuteOperationWithAuth();
        public abstract bool ExecuteOperation();
    }
}
