using System;
using System.Configuration;
using System.Runtime.InteropServices;
using Dev2.Common;
using Microsoft.VisualBasic.Devices;

namespace Dev2.Data.Storage
{

    /// <summary>
    /// Manage the segment count and size from here ;)
    /// </summary>
    public static class StorageSettingManager
    {
        const double _pressureFactor = 0.8;
        const int _minSlabSize = 16777216; // ~ 16MB

        public static Func<ulong> TotalFreeMemory { get; set; }

        public static Func<string> StorageLayerSegments { get; set; }

        public static Func<string> StorageLayerSegmentSize { get; set; }

        // Static constructor to init Funcs ;)
        static StorageSettingManager()
        {
            TotalFreeMemory = () =>
            {
                var computerInfo = new ComputerInfo();

                var result = computerInfo.AvailablePhysicalMemory;

                return result;
            };

            StorageLayerSegments = () => ConfigurationManager.AppSettings["StorageLayerSegments"];

            StorageLayerSegmentSize = () => ConfigurationManager.AppSettings["StorageLayerSegmentSize"];
        }

        /// <summary>
        /// Gets the segment count.
        /// </summary>
        /// <returns></returns>
        public static int GetSegmentCount()
        {

            var tmp = StorageLayerSegments();

            int result;
            int.TryParse(tmp, out result);

            // doggy config or 32 bit os, adjust ;)
            if((result < 1 || !OsBitVersionDetector.Is64BitOperatingSystem()))
            {
                result = GlobalConstants.DefaultStorageSegments;
            }


            return result;
        }

        /// <summary>
        /// Gets the size of the segment.
        /// </summary>
        /// <returns></returns>
        public static int GetSegmentSize()
        {
            var tmp = StorageLayerSegmentSize();

            int result;
            int.TryParse(tmp, out result);

            // doggy config or 32 bit os, adjust ;)
            if(result < 1 || !OsBitVersionDetector.Is64BitOperatingSystem())
            {
                result = GlobalConstants.DefaultStorageSegmentSize;
            }

            // adjust for pressure
            var adjustmentValue = AdjustMemoryForPressure(result);

            if(adjustmentValue > 0)
            {
                return adjustmentValue;
            }


            return result;
        }

        private static int AdjustMemoryForPressure(int requestedSize)
        {
            // Ensure we have the memory available ;)
            double totalSegments = GetSegmentCount() + 1; // account for background worker ;)

            var totalRequiredMemory = totalSegments * requestedSize;

            var totalFreeMemory = TotalFreeMemory();

            // we need to adjust, shoot ;(
            if(totalRequiredMemory >= totalFreeMemory)
            {
                ServerLogger.LogMessage("Memory Pressure...");

                var usableMemeory = totalFreeMemory * _pressureFactor;

                int result = (int)(usableMemeory / totalSegments);

                if(result < _minSlabSize)
                {
                    const string Msg = "Too little memory to start server, at least 16 MB should be free.";
                    var ex = new Exception(Msg);
                    ServerLogger.LogError("StorageSettingManager", ex);
                    throw ex;
                }

                return result;
            }

            return -1;
        }
    }

    /// <summary>
    /// Used to detect 32 or 64 bit OS ;)
    /// FROM : http://1code.codeplex.com/SourceControl/changeset/view/39074#842775
    /// </summary>
    public class OsBitVersionDetector
    {

        #region Is64BitOperatingSystem (IsWow64Process)

        /// <summary>
        /// The function determines whether the current operating system is a 
        /// 64-bit operating system.
        /// </summary>
        /// <returns>
        /// The function returns true if the operating system is 64-bit; 
        /// otherwise, it returns false.
        /// </returns>
        public static bool Is64BitOperatingSystem()
        {
            if(IntPtr.Size == 8) // 64-bit programs run only on Win64
            {
                return true;
            }

            // 32-bit programs run on both 32-bit and 64-bit Windows
            // Detect whether the current process is a 32-bit process 
            // running on a 64-bit system.
            bool flag;
            return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                     IsWow64Process(GetCurrentProcess(), out flag)) && flag);
        }

        /// <summary>
        /// The function determines whether a method exists in the export 
        /// table of a certain module.
        /// </summary>
        /// <param name="moduleName">The name of the module</param>
        /// <param name="methodName">The name of the method</param>
        /// <returns>
        /// The function returns true if the method specified by methodName 
        /// exists in the export table of the module specified by moduleName.
        /// </returns>
        private static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if(moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule,
                                                    [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        #endregion

    }
}
