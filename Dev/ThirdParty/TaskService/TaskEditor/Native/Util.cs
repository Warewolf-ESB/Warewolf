
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
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
	internal static partial class NativeMethods
	{
		public static class Util
		{
			// Methods
			private static int GetEmbeddedNullStringLengthAnsi(string s)
			{
				int index = s.IndexOf('\0');
				if (index > -1)
				{
					string str = s.Substring(0, index);
					string str2 = s.Substring(index + 1);
					return ((GetPInvokeStringLength(str) + GetEmbeddedNullStringLengthAnsi(str2)) + 1);
				}
				return GetPInvokeStringLength(s);
			}

			public static int GetPInvokeStringLength(string s)
			{
				if (string.IsNullOrEmpty(s))
					return 0;
				if (Marshal.SystemDefaultCharSize == 2)
					return s.Length;
				if (s.IndexOf('\0') > -1)
					return GetEmbeddedNullStringLengthAnsi(s);
				return lstrlen(s);
			}

			public static int HIWORD(int n)
			{
				return ((n >> 0x10) & 0xffff);
			}

			public static int HIWORD(IntPtr n)
			{
				return HIWORD((int)((long)n));
			}

			public static int LOWORD(int n)
			{
				return (n & 0xffff);
			}

			public static int LOWORD(IntPtr n)
			{
				return LOWORD((int)((long)n));
			}

			[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
			private static extern int lstrlen(string s);

			public static int MAKELONG(int low, int high)
			{
				return ((high << 0x10) | (low & 0xffff));
			}

			public static IntPtr MAKELPARAM(int low, int high)
			{
				return (IntPtr)((high << 0x10) | (low & 0xffff));
			}

			public static int SignedHIWORD(int n)
			{
				return (short)((n >> 0x10) & 0xffff);
			}

			public static int SignedHIWORD(IntPtr n)
			{
				return SignedHIWORD((int)((long)n));
			}

			public static int SignedLOWORD(int n)
			{
				return (short)(n & 0xffff);
			}

			public static int SignedLOWORD(IntPtr n)
			{
				return SignedLOWORD((int)((long)n));
			}
		}
	}
}
