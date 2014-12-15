
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
using System.Windows.Threading;

namespace Dev2.Core.Tests
{
    public static class ThreadExecuter
    {
        public static void RunCodeAsSTA(AutoResetEvent are, ThreadStart originalDelegate)
        {
            Thread thread = new Thread(delegate()
            {
                try
                {
                    originalDelegate.Invoke();
                    are.Set();
                    Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    are.Set();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
