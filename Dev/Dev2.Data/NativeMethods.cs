using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Threading;

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

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr CreateMemoryResourceNotification(MemoryResourceNotificationType notificationType);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool QueryMemoryResourceNotification(IntPtr resourceNotificationHandle, out int resourceState);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SuppressUnmanagedCodeSecurity]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);

		enum MemoryResourceNotificationType : int
		{
			LowMemoryResourceNotification = 0,
			HighMemoryResourceNotification = 1,
		}

		private static IntPtr MemoryResourceNotificationHandle;

		public static void RegisterNotification()
		{
			MemoryResourceNotificationHandle = CreateMemoryResourceNotification(MemoryResourceNotificationType.LowMemoryResourceNotification);
		}

		public static bool IsLowMemoryDetected()
		{
			if (IntPtr.Zero == MemoryResourceNotificationHandle || null == MemoryResourceNotificationHandle)
			{
				RegisterNotification();
			}

			bool isSuccecced = QueryMemoryResourceNotification(MemoryResourceNotificationHandle, out int memoryStatus);

			if (isSuccecced)
			{
				if (memoryStatus >= 1)
				{
					return true;
				}

			}
			return false;
		}

		public static void ReleaseResources()
		{
			if (null == MemoryResourceNotificationHandle)
			{
				CloseHandle(MemoryResourceNotificationHandle);
				MemoryResourceNotificationHandle = IntPtr.Zero;
			}
		}
	}
}
