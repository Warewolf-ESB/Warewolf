using System;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.ExtMethods;
using Dev2.Providers.Logs;

namespace Dev2.Network
{
    public static class TaskExtensions
    {
        public static T WaitForResult<T>(this Task<T> task)
        {
            try
            {
                task.WaitWithPumping(GlobalConstants.NetworkTimeOut);
                return task.Result;
            }
            catch(Exception e)
            {
                Logger.LogError("TaskExtensions", e);
            }
            return default(T);
        }

        public static void WaitForResult(this Task task)
        {
            task.Wait(100);
        }
    }
}