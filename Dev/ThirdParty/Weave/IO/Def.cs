
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
using System.Runtime.InteropServices;

namespace System
{
    public interface IByteReaderBase : IDisposable
    {
        bool ReadBoolean();
        char ReadChar();
        string ReadString();
        byte[] ReadBytes(int amount);
        int ReadBytes(byte[] buffer, int offset, int count);
        char[] ReadChars(int amount);
        int ReadChars(char[] buffer, int offset, int count);
        sbyte ReadSByte();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        decimal ReadDecimal();
        byte ReadByte();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
        float ReadSingle();
        double ReadDouble();
        Version ReadVersion();
        DateTime ReadDateTime();
        TimeSpan ReadTimeSpan();
        Guid ReadGuid();
        TwoOctetUnion ReadTwoOctet();
        FourOctetUnion ReadFourOctet();
        EightOctetUnion ReadEightOctet();
    }

    public interface IByteWriterBase : IDisposable
    {
        void Write(bool value);
        void Write(char value);
        void Write(string value);
        void Write(byte[] value);
        void Write(byte[] value, int offset, int count);
        void Write(char[] value);
        void Write(char[] value, int offset, int count);
        void Write(sbyte value);
        void Write(short value);
        void Write(int value);
        void Write(long value);
        void Write(decimal value);
        void Write(byte value);
        void Write(ushort value);
        void Write(uint value);
        void Write(ulong value);
        void Write(float value);
        void Write(double value);
        void Write(Version value);
        void Write(DateTime value);
        void Write(TimeSpan value);
        void Write(Guid value);
        void Write(TwoOctetUnion value);
        void Write(FourOctetUnion value);
        void Write(EightOctetUnion value);
    }

    public interface IByteStreamBase : IByteReaderBase, IByteWriterBase
    {

    }

    public abstract class SerializableEntity
    {
        protected SerializableEntity()
        {
        }

        public SerializableEntity(IByteReaderBase reader)
        {
        }

        public void Serialize(IByteWriterBase writer, bool attachHeader)
        {
            Type ourType = GetType();
            object[] ourAttribs = ourType.GetCustomAttributes(typeof(SerializationExclusionAttribute), false);
            if (ourAttribs != null && ourAttribs.Length != 0) return;
            if (attachHeader) writer.Write(Deserializer.GetTypeHeader(ourType));
            Serialize(writer);
        }

        protected abstract void Serialize(IByteWriterBase writer);
    }

    #region SerializationExclusion Attribute
    [global::System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SerializationExclusionAttribute : Attribute
    {
        public SerializationExclusionAttribute()
        {
        }
    }
    #endregion

    public static class __IByteReaderBaseExtensions
    {
        public static byte[] ReadByteArray(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            return reader.ReadBytes(length);
        }

        public static bool[] ReadBooleanArray(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            bool[] result = new bool[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadBoolean();
            return result;
        }

        public static string[] ReadStringArray(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            string[] result = new string[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadString();
            return result;
        }

        public static sbyte[] ReadSByteArray(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            sbyte[] result = new sbyte[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadSByte();
            return result;
        }
        public static short[] ReadInt16Array(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            short[] result = new short[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt16();
            return result;
        }
        public static int[] ReadInt32Array(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            int[] result = new int[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt32();
            return result;
        }
        public static long[] ReadInt64Array(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            long[] result = new long[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt64();
            return result;
        }
        public static decimal[] ReadDecimalArray(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            decimal[] result = new decimal[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadDecimal();
            return result;
        }
        public static ushort[] ReadUInt16Array(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            ushort[] result = new ushort[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt16();
            return result;
        }
        public static uint[] ReadUInt32Array(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            uint[] result = new uint[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt32();
            return result;
        }
        public static ulong[] ReadUInt64Array(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            ulong[] result = new ulong[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt64();
            return result;
        }
        public static float[] ReadSingleArray(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            float[] result = new float[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadSingle();
            return result;
        }
        public static double[] ReadDoubleArray(this IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            double[] result = new double[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadDouble();
            return result;
        }
    }

    public static class __IByteWriterBaseExtensions
    {
        public static void Write(this IByteWriterBase writer, bool[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, bool[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, string[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, string[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, sbyte[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, byte[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, sbyte[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, short[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, short[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, int[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, int[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, long[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, long[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, decimal[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, decimal[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, ushort[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, ushort[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, uint[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, uint[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, ulong[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, ulong[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, float[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, float[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }

        public static void Write(this IByteWriterBase writer, double[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public static void Write(this IByteWriterBase writer, double[] array, int arrayIndex, int amount)
        {
            writer.Write(amount);
            for (int i = 0; i < amount; i++) writer.Write(array[arrayIndex + i]);
        }
    }
}
