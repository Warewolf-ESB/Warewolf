
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
using System.Threading.Tasks;

namespace System.Network
{
    public class __BaseNetworkTransparentProxy
    {
        public __BaseNetworkTransparentProxy()
        {
        }

        public virtual IByteReaderBase SendDuplexPacket(Packet packet)
        {
            throw new NotSupportedException();
        }

        public virtual void SendSimplexPacket(Packet packet)
        {
            throw new NotSupportedException();
        }

        public void Write(IByteWriterBase writer, bool[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, string[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, sbyte[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, short[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, int[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, long[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, decimal[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, ushort[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, uint[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, ulong[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, float[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, double[] array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Length);
                for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
            }
        }


        public void Write(IByteWriterBase writer, IList<bool> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<string> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<sbyte> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<short> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<int> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<long> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<decimal> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<ushort> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<uint> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<ulong> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<float> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write(IByteWriterBase writer, IList<double> array)
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) writer.Write(array[i]);
            }
        }

        public void Write<T>(IByteWriterBase writer, IList<T> array) where T : INetworkSerializable
        {
            if (array == null) writer.Write(0);
            else
            {
                writer.Write(array.Count);
                for (int i = 0; i < array.Count; i++) array[i].Serialize(writer);
            }
        }

        public virtual void WriteUnhandled(IByteWriterBase writer, object unhandled)
        {
            throw new NotSupportedException();
        }

        public virtual object ConstructUnhandled(Type type)
        {
            throw new NotSupportedException();
        }

        public virtual object ReadUnhandled(IByteReaderBase reader, Type type)
        {
            throw new NotSupportedException();
        }

        public byte[] ReadByteArray(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            return reader.ReadBytes(length);
        }

        public bool[] ReadBooleanArray(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            bool[] result = new bool[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadBoolean();
            return result;
        }

        public string[] ReadStringArray(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            string[] result = new string[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadString();
            return result;
        }

        public sbyte[] ReadSByteArray(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            sbyte[] result = new sbyte[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadSByte();
            return result;
        }

        public short[] ReadInt16Array(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            short[] result = new short[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt16();
            return result;
        }

        public int[] ReadInt32Array(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            int[] result = new int[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt32();
            return result;
        }

        public long[] ReadInt64Array(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            long[] result = new long[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt64();
            return result;
        }

        public decimal[] ReadDecimalArray(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            decimal[] result = new decimal[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadDecimal();
            return result;
        }

        public ushort[] ReadUInt16Array(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            ushort[] result = new ushort[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt16();
            return result;
        }

        public uint[] ReadUInt32Array(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            uint[] result = new uint[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt32();
            return result;
        }

        public ulong[] ReadUInt64Array(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            ulong[] result = new ulong[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt64();
            return result;
        }

        public float[] ReadSingleArray(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            float[] result = new float[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadSingle();
            return result;
        }

        public double[] ReadDoubleArray(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            double[] result = new double[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadDouble();
            return result;
        }

        public IList<byte> ReadIListOfByte(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            return new List<byte>(reader.ReadBytes(length));
        }

        public IList<bool> ReadIListOfBoolean(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            bool[] result = new bool[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadBoolean();
            return new List<bool>(result);
        }

        public IList<string> ReadIListOfString(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            string[] result = new string[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadString();
            return new List<string>(result);
        }

        public IList<sbyte> ReadIListOfSByte(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            sbyte[] result = new sbyte[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadSByte();
            return new List<sbyte>(result);
        }

        public IList<short> ReadIListOfInt16(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            short[] result = new short[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt16();
            return new List<short>(result);
        }

        public IList<int> ReadIListOfInt32(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            int[] result = new int[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt32();
            return new List<int>(result);
        }

        public IList<long> ReadIListOfInt64(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            long[] result = new long[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadInt64();
            return new List<long>(result);
        }

        public IList<decimal> ReadIListOfDecimal(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            decimal[] result = new decimal[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadDecimal();
            return new List<decimal>(result);
        }

        public IList<ushort> ReadIListOfUInt16(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            ushort[] result = new ushort[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt16();
            return new List<ushort>(result);
        }

        public IList<uint> ReadIListOfUInt32(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            uint[] result = new uint[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt32();
            return new List<uint>(result);
        }

        public IList<ulong> ReadIListOfUInt64(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            ulong[] result = new ulong[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadUInt64();
            return new List<ulong>(result);
        }

        public IList<float> ReadIListOfSingle(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            float[] result = new float[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadSingle();
            return new List<float>(result);
        }

        public IList<double> ReadIListOfDouble(IByteReaderBase reader)
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            double[] result = new double[length];
            for (int i = 0; i < result.Length; i++) result[i] = reader.ReadDouble();
            return new List<double>(result);
        }

        public IList<T> ReadIListOfT<T>(IByteReaderBase reader) where T : INetworkSerializable, new()
        {
            int length = reader.ReadInt32();
            if (length == 0) return null;
            List<T> result = new List<T>();

            for (int i = 0; i < length; i++)
            {
                T t = new T();
                t.Deserialize(reader);
                result.Add(t);
            }

            return result;
        }
    }
}
