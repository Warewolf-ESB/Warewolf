using System;
using System.Collections.Generic;
using System.IO;
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
#if WINDOWS || NETFRAMEWORK
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GlobalMemoryStatusEx([In, Out] ref MEMORYSTATUSEX lpBuffer);
#endif

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

#if WINDOWS || NETFRAMEWORK
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr CreateMemoryResourceNotification(MemoryResourceNotificationType notificationType);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool QueryMemoryResourceNotification(IntPtr resourceNotificationHandle, out int resourceState);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SuppressUnmanagedCodeSecurity]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);
#endif

		enum MemoryResourceNotificationType : int
		{
			LowMemoryResourceNotification = 0,
			HighMemoryResourceNotification = 1,
		}

		private static IntPtr MemoryResourceNotificationHandle;

		public static void RegisterNotification()
		{
#if WINDOWS || NETFRAMEWORK
			MemoryResourceNotificationHandle = CreateMemoryResourceNotification(MemoryResourceNotificationType.LowMemoryResourceNotification);
#endif
		}

		public static bool IsLowMemoryDetected()
		{
			if (IntPtr.Zero == MemoryResourceNotificationHandle || null == MemoryResourceNotificationHandle)
			{
				RegisterNotification();
			}

#if WINDOWS || NETFRAMEWORK
			bool isSuccecced = QueryMemoryResourceNotification(MemoryResourceNotificationHandle, out int memoryStatus);

			if (isSuccecced)
			{
				if (memoryStatus >= 1)
				{
					return true;
				}

			}
#endif
			return false;
		}

		public static void ReleaseResources()
		{
			if (null == MemoryResourceNotificationHandle)
			{
#if WINDOWS || NETFRAMEWORK
				CloseHandle(MemoryResourceNotificationHandle);
#endif
				MemoryResourceNotificationHandle = IntPtr.Zero;
			}
		}

		// Linux fallback for GlobalMemoryStatusEx. Parses /proc/meminfo, which
		// reports values in kB. Returns false if /proc/meminfo isn't present
		// (e.g. macOS) or doesn't contain MemTotal — caller should fall back.
		internal static bool TryReadProcMeminfo(ref MEMORYSTATUSEX status)
		{
			const string path = "/proc/meminfo";
			if (!File.Exists(path))
			{
				return false;
			}

			var values = new Dictionary<string, ulong>(StringComparer.Ordinal);
			try
			{
				foreach (var line in File.ReadAllLines(path))
				{
					var colon = line.IndexOf(':');
					if (colon <= 0)
					{
						continue;
					}
					var key = line.Substring(0, colon);
					var rest = line.Substring(colon + 1).TrimStart();
					var space = rest.IndexOf(' ');
					var numText = space > 0 ? rest.Substring(0, space) : rest;
					if (ulong.TryParse(numText, out var kb))
					{
						// kB → bytes; guard against overflow on absurd values
						values[key] = kb <= ulong.MaxValue / 1024UL ? kb * 1024UL : ulong.MaxValue;
					}
				}
			}
			catch (IOException)
			{
				return false;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}

			if (!values.TryGetValue("MemTotal", out var memTotal) || memTotal == 0UL)
			{
				return false;
			}

			// MemAvailable was added in kernel 3.14. Fall back to the classic
			// free+buffers+cached(+reclaimable slab) estimate on older kernels.
			if (!values.TryGetValue("MemAvailable", out var memAvailable))
			{
				values.TryGetValue("MemFree", out var memFree);
				values.TryGetValue("Buffers", out var buffers);
				values.TryGetValue("Cached", out var cached);
				values.TryGetValue("SReclaimable", out var sreclaim);
				memAvailable = memFree + buffers + cached + sreclaim;
			}
			if (memAvailable > memTotal)
			{
				memAvailable = memTotal;
			}

			values.TryGetValue("SwapTotal", out var swapTotal);
			values.TryGetValue("SwapFree", out var swapFree);
			if (swapFree > swapTotal)
			{
				swapFree = swapTotal;
			}

			var used = memTotal - memAvailable;
			var commitTotal = SaturatingAdd(memTotal, swapTotal);
			var commitAvail = SaturatingAdd(memAvailable, swapFree);

			status.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
			status.dwMemoryLoad = (uint)(used * 100UL / memTotal);
			status.ulTotalPhys = memTotal;
			status.ulAvailPhys = memAvailable;
			status.ulTotalPageFile = commitTotal;
			status.ulAvailPageFile = commitAvail;

			// Prefer the precise per-process VA limit (RLIMIT_AS minus current
			// VmSize). Fall back to the commit-limit approximation if rlimit is
			// unavailable or unlimited.
			if (TryGetLinuxVirtualMemory(out var totalVirt, out var availVirt))
			{
				status.ulTotalVirtual = totalVirt;
				status.ulAvailVirtual = availVirt;
			}
			else
			{
				status.ulTotalVirtual = commitTotal;
				status.ulAvailVirtual = commitAvail;
			}
			status.ulAvailExtendedVirtual = 0UL;
			return true;
		}

		// Linux RLIMIT_AS value; differs from BSD/macOS, but this helper is
		// only invoked after /proc/meminfo has been found, so we're on Linux.
		private const int LinuxRLIMIT_AS = 9;

		[StructLayout(LayoutKind.Sequential)]
		private struct RLimit
		{
			public ulong rlim_cur;
			public ulong rlim_max;
		}

		[DllImport("libc", SetLastError = true)]
		private static extern int getrlimit(int resource, out RLimit rlim);

		private static bool TryGetLinuxVirtualMemory(out ulong totalVirtual, out ulong availVirtual)
		{
			totalVirtual = 0UL;
			availVirtual = 0UL;
			ulong limit;
			try
			{
				if (getrlimit(LinuxRLIMIT_AS, out var rlim) != 0)
				{
					return false;
				}
				limit = rlim.rlim_cur;
			}
			catch (DllNotFoundException)
			{
				return false;
			}
			catch (EntryPointNotFoundException)
			{
				return false;
			}

			// RLIM_INFINITY (~0UL) means no per-process VA cap was set; the
			// commit-limit fallback is more meaningful in that case.
			if (limit == 0UL || limit == ulong.MaxValue)
			{
				return false;
			}

			ulong vmSize = 0UL;
			try
			{
				const string statusPath = "/proc/self/status";
				if (File.Exists(statusPath))
				{
					foreach (var line in File.ReadAllLines(statusPath))
					{
						if (!line.StartsWith("VmSize:", StringComparison.Ordinal))
						{
							continue;
						}
						var rest = line.Substring("VmSize:".Length).TrimStart();
						var space = rest.IndexOf(' ');
						var numText = space > 0 ? rest.Substring(0, space) : rest;
						if (ulong.TryParse(numText, out var kb) && kb <= ulong.MaxValue / 1024UL)
						{
							vmSize = kb * 1024UL;
						}
						break;
					}
				}
			}
			catch (IOException) { /* leave vmSize = 0 */ }
			catch (UnauthorizedAccessException) { /* leave vmSize = 0 */ }

			totalVirtual = limit;
			availVirtual = vmSize >= limit ? 0UL : limit - vmSize;
			return true;
		}

		private static ulong SaturatingAdd(ulong a, ulong b) =>
			a > ulong.MaxValue - b ? ulong.MaxValue : a + b;
	}
}
