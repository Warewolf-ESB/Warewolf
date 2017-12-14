/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Warewolf.Resource.Errors;


namespace Dev2.Activities
{
    public class ModalChecker
    {
        public static Boolean IsWaitingForUserInput(Process process)
        {
            if(process == null)
            {
                throw new Exception(ErrorResource.NoProcessFound);
            }
            if (process.HasExited)
            {
                return false;
            }

            var checker = new ModalChecker(process);
            return checker.WaitingForUserInput;
        }

        #region Native Windows Stuff

        const int WS_EX_DLGMODALFRAME = 0x00000001;
        const int GWL_EXSTYLE = -20;
        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
        delegate int EnumWindowsProc(IntPtr hWnd, int lParam);
        [DllImport("user32")]
        static extern int EnumWindows(EnumWindowsProc lpEnumFunc, int lParam);
        [DllImport("user32", CharSet = CharSet.Auto)]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);
        [DllImport("user32")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr handle);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder caption,
            int count);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLength(IntPtr handle);
        #endregion

        readonly Process _process;
        Boolean _waiting;

        ModalChecker(Process process)
        {
            _process = process;
            _waiting = false;
        }

        Boolean WaitingForUserInput
        {
            get
            {
                WindowEnum(_process.MainWindowHandle);
                if (!_waiting)
                {
                    _waiting = ThreadWindows(_process.MainWindowHandle);
                }
                return _waiting;
            }
        }

        static bool ThreadWindows(IntPtr handle)
        {
            if (IsWindowVisible(handle))
            {
                var length = GetWindowTextLength(handle);
                var caption = new StringBuilder(length + 1);
                GetWindowText(handle, caption, caption.Capacity);
                return true;
            }
            return false;
        }

        int WindowEnum(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out IntPtr processId);
            if (processId.ToInt32() != _process.Id)
            {
                return 1;
            }

            var style = GetWindowLong(hWnd, GWL_EXSTYLE);
            if ((style & WS_EX_DLGMODALFRAME) != 0)
            {
                _waiting = true;
                return 0;
            }
            return 1;
        }
    }
}
