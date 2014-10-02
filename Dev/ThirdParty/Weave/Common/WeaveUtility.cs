
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using System.Management;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace System
{
    public partial class WeaveUtility
    {
        #region Readonly Fields
        private static readonly string[] _emptyStringArray = new string[0];
        public static readonly IDisposable EmptyReferenceDisposable = new EmptyRefDisposable();
        public static readonly IDisposable EmptyValueTypeDisposable = new EmptyValDisposable();
        #endregion

        #region Instance Fields
        private static MethodInfo _grsSingle = typeof(Environment).GetMethod("GetResourceString", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(String) }, null);
        #endregion

        #region Resource Handling
        public static string GetResourceString(string key)
        {
            return _grsSingle.Invoke(null, new object[] { key }) as string;
        }

        public static string GetResourceString(string key, params object[] values)
        {
            string resource = GetResourceString(key);
            return String.Format(CultureInfo.CurrentCulture, resource, values);
        }
        #endregion

        #region Array Handling
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool Equals(byte[] array1, byte[] array2)
        {
            if (ReferenceEquals(array1, array2)) return true;
            int length = array1.Length;
            if (length != array2.Length) return false;
            if (length == 0) return true;

            if (((length >> 1) << 1) != length)
                if (array1[--length] != array2[length])
                    return false;

            fixed (byte* str = array1)
            {
                byte* chPtr = str;

                fixed (byte* str2 = array2)
                {
                    byte* chPtr2 = str2;
                    byte* chPtr3 = chPtr;
                    byte* chPtr4 = chPtr2;

                    while (length >= 10)
                    {
                        if ((((*(((int*)chPtr3)) != *(((int*)chPtr4))) || (*(((int*)(chPtr3 + 2))) != *(((int*)(chPtr4 + 2))))) || ((*(((int*)(chPtr3 + 4))) != *(((int*)(chPtr4 + 4)))) || (*(((int*)(chPtr3 + 6))) != *(((int*)(chPtr4 + 6)))))) || (*(((int*)(chPtr3 + 8))) != *(((int*)(chPtr4 + 8)))))
                            break;

                        chPtr3 += 10;
                        chPtr4 += 10;
                        length -= 10;
                    }

                    while (length > 0)
                    {
                        if (*(((int*)chPtr3)) != *(((int*)chPtr4))) break;
                        chPtr3 += 2;
                        chPtr4 += 2;
                        length -= 2;
                    }

                    return (length <= 0);
                }
            }
        }

        public static byte[] GetAssignedByteArray(int length, byte value)
        {
            byte[] toReturn = new byte[length];
            for (int i = 0; i < toReturn.Length; i++) toReturn[i] = value;
            return toReturn;
        }

        public static T[] CreateArray<T>(int amount) where T : new()
        {
            T[] toReturn = new T[amount];
            for (int i = 0; i < toReturn.Length; i++) toReturn[i] = new T();
            return toReturn;
        }

        public static T[][] CreateArray<T>(int rank1Amount, int rank2Amount)
        {
            T[][] toReturn = new T[rank1Amount][];
            for (int i = 0; i < toReturn.Length; i++) toReturn[i] = new T[rank2Amount];
            return toReturn;
        }
        #endregion

        #region WMI
        public static uint GetVolumeSerial(string strDriveLetter)
        {
            uint serNum = 0;
            uint maxCompLen = 0;
            StringBuilder VolLabel = new StringBuilder(256);
            UInt32 VolFlags = new UInt32();
            StringBuilder FSName = new StringBuilder(256);
            strDriveLetter += ":\\";
            long Ret = NativeMethods.GetVolumeInformation(strDriveLetter, VolLabel, (UInt32)VolLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, (UInt32)FSName.Capacity);
            return serNum;
        }

        public static object[] PollWMI(string key, string query, CollectOutputEventHandler collection)
        {
            List<object> toReturn = new List<object>();
            bool acquired = false;

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    ManagementObjectCollection result = searcher.Get();

                    if (collection != null)
                    {
                        object collectedOutput = null;

                        foreach (ManagementObject output in result)
                        {
                            object rawOutput = output[key];

                            if (rawOutput != null)
                                collectedOutput = collection(rawOutput, collectedOutput);
                        }

                        if (collectedOutput != null)
                        {
                            toReturn.Add(collectedOutput);
                            acquired = true;
                        }
                    }
                    else
                    {
                        foreach (ManagementObject output in result)
                        {
                            object rawOutput = output[key];

                            if (rawOutput != null)
                            {
                                toReturn.Add(rawOutput);
                                acquired = true;
                            }
                        }
                    }
                }
            }
            catch { acquired = false; }

            if (!acquired) toReturn.Clear();
            return toReturn.ToArray();
        }
        #endregion

        #region EmptyRefDisposable
        private sealed class EmptyRefDisposable : IDisposable
        {
            public EmptyRefDisposable() { }
            public void Dispose() { }
        }
        #endregion

        #region EmptyValDisposable
        [StructLayout(LayoutKind.Sequential)]
        private struct EmptyValDisposable : IDisposable
        {
            public void Dispose() { }
        }
        #endregion

        #region DLLImports
        [SuppressUnmanagedCodeSecurity, SuppressUnmanagedCodeSecurity, SecurityCritical]
        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, UInt32 VolumeNameSize, ref UInt32 VolumeSerialNumber, ref UInt32 MaximumComponentLength, ref UInt32 FileSystemFlags, StringBuilder FileSystemNameBuffer, UInt32 FileSystemNameSize);
        }
        #endregion
    }

    public delegate object CollectOutputEventHandler(object rawOutput, object collectedOutput);
}
