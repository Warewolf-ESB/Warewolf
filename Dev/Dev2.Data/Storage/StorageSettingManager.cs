
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
        const double PressureFactor = 0.8;

        const double LowPhysicalMemoryPressure = 0.3;

        const long LowPhysicalMemoryLimit = 3221225472;

        public static Func<ulong> TotalFreeMemory { get; set; }

        public static Func<ulong> TotalPhysicalMemory { get; set; }

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

            TotalPhysicalMemory = () =>
            {
                var computerInfo = new ComputerInfo();

                var result = computerInfo.TotalPhysicalMemory;

                return result;
            };

            StorageLayerSegments = () => ConfigurationManager.AppSettings["StorageLayerSegments"];

            StorageLayerSegmentSize = () => ConfigurationManager.AppSettings["StorageLayerSegmentSize"];
        }

        public static Func<bool> Is64BitOs = () => OsBitVersionDetector.Is64BitOperatingSystem();

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
            if((result < 1 || !Is64BitOs()))
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
            if(result < 1 || !Is64BitOs())
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

            Dev2Logger.Log.Info("*** Total Free Memory [ " + ((totalFreeMemory / 1024) / 1024) + " MB ]");
            Dev2Logger.Log.Info("*** Total Required Memory [ " + ((totalRequiredMemory / 1024) / 1024) + " MB]");

            // we need to adjust, shoot ;(
            if(totalRequiredMemory >= totalFreeMemory)
            {
                Dev2Logger.Log.Info("Memory Pressure...");

                var totalMemory = TotalPhysicalMemory();

                var factor = PressureFactor;

                // 3 GB is our boundary for stepping down to 1/3 of free ;)
                if(totalMemory <= LowPhysicalMemoryLimit)
                {
                    Dev2Logger.Log.Info("** Low Physical Memory **");
                    factor = LowPhysicalMemoryPressure;
                    totalSegments = 2; // set low number of segments in this case ;)
                    StorageLayerSegments = () => "1";
                }

                var usableMemeory = totalFreeMemory * factor;

                int result = (int)(usableMemeory / totalSegments);

                Dev2Logger.Log.Info("*** Slab Size Is [ " + ((result / 1024) / 1024) + " ]");

                if(result < GlobalConstants.DefaultStorageSegmentSize)
                {
                    const string Msg = "Too little memory to start server, at least 8 MB / slab should be free.";
                    var ex = new Exception(Msg);
                    Dev2Logger.Log.Error("StorageSettingManager", ex);
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
