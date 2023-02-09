using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Dev2.Net6.Compatibility
{
    public static class TransactionManagerExtensions
    {
        public static void ConfigureTransactionTimeout(TimeSpan defaultTimeOut, TimeSpan maxTimeOut)
        {
            SetTransactionManagerField("s_cachedMaxTimeout", true);
            SetTransactionManagerField("s_maximumTimeout", maxTimeOut);

            //SetTransactionManagerField("s_defaultTimeout", true);
            SetTransactionManagerField("s_defaultTimeout", defaultTimeOut);
        }

        static private void SetTransactionManagerField(string fieldName, object value)
        {
            var cacheField = typeof(TransactionManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);

            if (cacheField != null)
            {
                cacheField.SetValue(null, value);
            }
        }
    }
}
