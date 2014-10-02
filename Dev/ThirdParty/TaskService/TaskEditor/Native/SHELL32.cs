
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
using System.Text;

namespace Microsoft.Win32
{
	[System.Security.SuppressUnmanagedCodeSecurity]
	internal static partial class NativeMethods
	{
		internal const string SHELL32 = "shell32.dll";

		[DllImport(SHELL32, CharSet = CharSet.Unicode, SetLastError = false)]
		public static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, int nFolder, out IntPtr ppidl);

		// Note that the BROWSEINFO object's pszDisplayName only gives you the name of the folder.
		// To get the actual folderToSelect, you need to parse the returned PIDL
		[DllImport(SHELL32, CharSet = CharSet.Unicode)]
		public static extern uint SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPath);

		[DllImport(SHELL32, CharSet = CharSet.Unicode, SetLastError = false)]
		public static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);
	}
}
