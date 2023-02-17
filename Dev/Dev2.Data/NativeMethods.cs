using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Data
{
    public class NativeMethods
    {
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GlobalMemoryStatusEx([In, Out] ref MEMORYSTATUSEX lpBuffer);

		[StructLayout(LayoutKind.Sequential)]
		internal struct MEMORYSTATUSEX
		{
			internal uint dwLength;
			internal uint dwMemoryLoad;
			internal ulong ulTotalPhys;
			internal ulong ulAvailPhys;
			internal ulong ulTotalPageFile;
			internal ulong ulAvailPageFile;
			internal ulong ulTotalVirtual;
			internal ulong ulAvailVirtual;
			internal ulong ulAvailExtendedVirtual;
		}
	}
}
