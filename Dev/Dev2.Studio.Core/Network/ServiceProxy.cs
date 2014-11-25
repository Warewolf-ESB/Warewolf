
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
                Dev2Logger.Log.Error("TaskExtensions", e);
            }
            return default(T);
        }

        public static void WaitForResult(this Task task)
        {
            task.Wait(10);
        }
    }
}
