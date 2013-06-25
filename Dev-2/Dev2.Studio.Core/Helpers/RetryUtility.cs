using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dev2.Studio.Core.Helpers
{
    public static class RetryUtility
    {
        public static void RetryAction(Action action, int numRetries, int retryTimeout)
        {
            if (action == null)
                throw new ArgumentNullException("action"); // slightly safer...

            do
            {
                try { action(); return; }
                catch
                {
                    if (numRetries <= 0) throw;  // improved to avoid silent failure
                    else Thread.Sleep(retryTimeout);
                }
            } while (numRetries-- > 0);
        }

        public static T RetryMethod<T>(Func<T> method, int numRetries, int retryTimeout, Action onFailureAction)
        {
            if(method == null) return default(T);
            T retval = default(T);
            do
            {
                try
                {
                    retval = method();
                    return retval;
                }
                catch
                {
                    if (onFailureAction != null)
                        onFailureAction();
                    if (numRetries <= 0) throw; // improved to avoid silent failure
                    Thread.Sleep(retryTimeout);
                }
            } while (numRetries-- > 0);
            return retval;
        }
    }
}
