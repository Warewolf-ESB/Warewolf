// Decompiled with JetBrains decompiler
// Type: System.WeaveUtility
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System
{
  public class WeaveUtility
  {
    private static readonly string[] _emptyStringArray = new string[0];
    public static readonly IDisposable EmptyReferenceDisposable = (IDisposable) new WeaveUtility.EmptyRefDisposable();
    public static readonly IDisposable EmptyValueTypeDisposable = (IDisposable) new WeaveUtility.EmptyValDisposable();
    private static MethodInfo _grsSingle = typeof (Environment).GetMethod("GetResourceString", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null, new Type[1]
    {
      typeof (string)
    }, (ParameterModifier[]) null);

    public static string GetResourceString(string key) => WeaveUtility._grsSingle.Invoke((object) null, new object[1]
    {
      (object) key
    }) as string;

    public static string GetResourceString(string key, params object[] values) => string.Format((IFormatProvider) CultureInfo.CurrentCulture, WeaveUtility.GetResourceString(key), values);

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    public static unsafe bool Equals(byte[] array1, byte[] array2)
    {
      if (array1 == array2)
        return true;
      int length = array1.Length;
      if (length != array2.Length)
        return false;
      if (length == 0)
        return true;
      if (length >> 1 << 1 != length && (int) array1[--length] != (int) array2[length])
        return false;
      fixed (byte* numPtr1 = array1)
        fixed (byte* numPtr2 = array2)
        {
          byte* numPtr3 = numPtr1;
          byte* numPtr4;
          for (numPtr4 = numPtr2; length >= 10 && *(int*) numPtr3 == *(int*) numPtr4 && *(int*) (numPtr3 + 2) == *(int*) (numPtr4 + 2) && *(int*) (numPtr3 + 4) == *(int*) (numPtr4 + 4) && *(int*) (numPtr3 + 6) == *(int*) (numPtr4 + 6) && *(int*) (numPtr3 + 8) == *(int*) (numPtr4 + 8); length -= 10)
          {
            numPtr3 += 10;
            numPtr4 += 10;
          }
          for (; length > 0 && *(int*) numPtr3 == *(int*) numPtr4; length -= 2)
          {
            numPtr3 += 2;
            numPtr4 += 2;
          }
          return length <= 0;
        }
    }

    public static byte[] GetAssignedByteArray(int length, byte value)
    {
      byte[] assignedByteArray = new byte[length];
      for (int index = 0; index < assignedByteArray.Length; ++index)
        assignedByteArray[index] = value;
      return assignedByteArray;
    }

    public static T[] CreateArray<T>(int amount) where T : new()
    {
      T[] array = new T[amount];
      for (int index = 0; index < array.Length; ++index)
        array[index] = new T();
      return array;
    }

    public static T[][] CreateArray<T>(int rank1Amount, int rank2Amount)
    {
      T[][] array = new T[rank1Amount][];
      for (int index = 0; index < array.Length; ++index)
        array[index] = new T[rank2Amount];
      return array;
    }

    public static uint GetVolumeSerial(string strDriveLetter)
    {
      uint VolumeSerialNumber = 0;
      uint MaximumComponentLength = 0;
      StringBuilder VolumeNameBuffer = new StringBuilder(256);
      uint FileSystemFlags = 0;
      StringBuilder FileSystemNameBuffer = new StringBuilder(256);
      strDriveLetter += ":\\";
      WeaveUtility.NativeMethods.GetVolumeInformation(strDriveLetter, VolumeNameBuffer, (uint) VolumeNameBuffer.Capacity, ref VolumeSerialNumber, ref MaximumComponentLength, ref FileSystemFlags, FileSystemNameBuffer, (uint) FileSystemNameBuffer.Capacity);
      return VolumeSerialNumber;
    }

    public static object[] PollWMI(string key, string query, CollectOutputEventHandler collection)
    {
      List<object> objectList = new List<object>();
      bool flag = false;
      try
      {
        using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query))
        {
          ManagementObjectCollection objectCollection = managementObjectSearcher.Get();
          if (collection != null)
          {
            object collectedOutput = (object) null;
            foreach (ManagementBaseObject managementBaseObject in objectCollection)
            {
              object rawOutput = managementBaseObject[key];
              if (rawOutput != null)
                collectedOutput = collection(rawOutput, collectedOutput);
            }
            if (collectedOutput != null)
            {
              objectList.Add(collectedOutput);
              flag = true;
            }
          }
          else
          {
            foreach (ManagementBaseObject managementBaseObject in objectCollection)
            {
              object obj = managementBaseObject[key];
              if (obj != null)
              {
                objectList.Add(obj);
                flag = true;
              }
            }
          }
        }
      }
      catch
      {
        flag = false;
      }
      if (!flag)
        objectList.Clear();
      return objectList.ToArray();
    }

    public static FileStream TryOpen(string path) => File.Exists(path) ? File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None) : (FileStream) null;

    public static FileStream Open(string path) => File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);

    public static FileStream OpenOrCreate(string path)
    {
      FileStream fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
      fileStream.Seek(0L, SeekOrigin.End);
      return fileStream;
    }

    public static FileStream Create(string path) => File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);

    public static IByteReaderBase Read(byte[] data) => (IByteReaderBase) new BinaryFileReader((Stream) new MemoryStream(data, true));

    public static IByteReaderBase Read(Stream input, bool closeStream = true) => (IByteReaderBase) new BinaryFileReader(input, closeStream);

    public static IByteReaderBase ReadOpen(string path) => (IByteReaderBase) new BinaryFileReader((Stream) WeaveUtility.Open(path));

    public static IByteWriterBase WriteOpenOrCreate(string path) => (IByteWriterBase) new BinaryFileWriter((Stream) WeaveUtility.OpenOrCreate(path));

    public static IByteWriterBase WriteCreate(string path) => (IByteWriterBase) new BinaryFileWriter((Stream) WeaveUtility.Create(path));

    private sealed class EmptyRefDisposable : IDisposable
    {
      public void Dispose()
      {
      }
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    private struct EmptyValDisposable : IDisposable
    {
      public void Dispose()
      {
      }
    }

    [SuppressUnmanagedCodeSecurity]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private static class NativeMethods
    {
      [DllImport("kernel32.dll")]
      public static extern long GetVolumeInformation(
        string PathName,
        StringBuilder VolumeNameBuffer,
        uint VolumeNameSize,
        ref uint VolumeSerialNumber,
        ref uint MaximumComponentLength,
        ref uint FileSystemFlags,
        StringBuilder FileSystemNameBuffer,
        uint FileSystemNameSize);
    }
  }
}
