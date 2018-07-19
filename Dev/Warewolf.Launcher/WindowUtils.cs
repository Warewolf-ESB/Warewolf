using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Warewolf.Launcher
{
    static class WindowUtils
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

        public static void BringToFront()
        {
            var originalTitle = Console.Title;
            var uniqueTitle = Guid.NewGuid().ToString();
            Console.Title = uniqueTitle;
            Thread.Sleep(50);
            var handle = FindWindowByCaption(IntPtr.Zero, uniqueTitle);

            if (handle != IntPtr.Zero)
            {
                Console.Title = originalTitle;
                SetForegroundWindow(handle);
            }
        }

        public static string PromptForUserInput()
        {
            BringToFront();
            return Console.ReadLine();
        }
    }
}
