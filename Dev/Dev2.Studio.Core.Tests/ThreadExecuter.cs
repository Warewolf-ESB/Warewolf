using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
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
