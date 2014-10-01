
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	internal static class ButtonExtension
	{
		public static void SetElevationRequiredState(this ButtonBase btn, bool required = true)
		{
			if (System.Environment.OSVersion.Version.Major >= 6)
			{
				const uint BCM_SETSHIELD = 0x160C;    //Elevated button
				btn.FlatStyle = FlatStyle.System;
				SendMessage(btn.Handle, BCM_SETSHIELD, IntPtr.Zero, required ? new IntPtr(-1) : IntPtr.Zero);
				btn.Invalidate();
			}
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	}
}
