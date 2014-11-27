
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Helpers
{
    public static class RetryUtility
    {
        public static void RetryAction(Action action, int numRetries, int retryTimeout)
        {
            if(action == null)
                throw new ArgumentNullException("action"); // slightly safer...

            do
            {
                try { action(); return; }
                catch
                {
                    if(numRetries <= 0) throw;  // improved to avoid silent failure
                    Thread.Sleep(retryTimeout);
                }
            } while(numRetries-- > 0);
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
                    if(onFailureAction != null)
                        onFailureAction();
                    if(numRetries <= 0) throw; // improved to avoid silent failure
                    Thread.Sleep(retryTimeout);
                }
            } while(numRetries-- > 0);
            return retval;
        }
    }
}
