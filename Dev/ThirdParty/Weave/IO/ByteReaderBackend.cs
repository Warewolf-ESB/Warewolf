
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

namespace System.IO
{
    internal sealed class ByteReaderBackend : IByteReaderBase
    {
        #region Constants
        private const byte True = 1;
        private const byte False = 0;
        #endregion

        #region Readonly Fields
        private static readonly char[] _zeroCharArray = new char[0];
        #endregion

        #region Instance Fields
        private ByteKernal _kernal;
        private Encoding _encoding;
        #endregion

        #region Constructor
        public ByteReaderBackend(ByteKernal kernal)
        {
            _kernal = kernal;
            _encoding = Encoding.ASCII;
        }
        #endregion

        #region Read Handling
        public int Read7BitEncodedInt()
        {
            byte num3;
            int num = 0;
            int num2 = 0;

            do
            {
                if (num2 == 0x23) throw new FormatException(WeaveUtility.GetResourceString("Format_Bad7BitInt32"));
                num3 = ReadByte();
                num |= (num3 & 0x7f) << num2;
                num2 += 7;
            }
            while ((num3 & 0x80) != 0);

            return num;
        }

        public bool ReadBoolean()
        {
            if (!_kernal.EnsureRead(1)) return false;
            return (_kernal.Buffer[_kernal.Position++] != False);
        }

        public char ReadChar()
        {
            return (char)ReadInt16();
        }

        public string ReadString()
        {
            int capacity = Read7BitEncodedInt();
            if (capacity == 0) return String.Empty;
            if (capacity < 0) throw new IOException(WeaveUtility.GetResourceString("IO.IO_InvalidStringLen_Len", new object[] { capacity }));
            if (!_kernal.EnsureRead(capacity)) throw new EndOfStreamException();
            string result = _encoding.GetString(_kernal.Buffer, _kernal.Position, capacity);
            _kernal.Position += capacity;
            return result;
        }

        public byte[] ReadBytes(int amount)
        {
            if (!_kernal.EnsureRead(amount)) return new byte[amount];
            byte[] toReturn = new byte[amount];
            Buffer.BlockCopy(_kernal.Buffer, _kernal.Position, toReturn, 0, amount);
            _kernal.Position += amount;
            return toReturn;
        }

        public int ReadBytes(byte[] buffer, int offset, int count)
        {
            if (!_kernal.EnsureRead(count))
            {
                count = _kernal.Length - _kernal.Position;
                if (count == 0) return 0;
            }

            Buffer.BlockCopy(_kernal.Buffer, _kernal.Position, buffer, offset, count);
            _kernal.Position += count;
            return count;
        }

        public char[] ReadChars(int amount)
        {
            int capacity = Read7BitEncodedInt();
            if (capacity == 0) return _zeroCharArray;
            if (capacity < 0) throw new IOException(WeaveUtility.GetResourceString("IO.IO_InvalidStringLen_Len", new object[] { capacity }));
            if (!_kernal.EnsureRead(capacity)) throw new EndOfStreamException();
            char[] result = _encoding.GetChars(_kernal.Buffer, _kernal.Position, capacity);
            _kernal.Position += capacity;
            return result;
        }

        public int ReadChars(char[] buffer, int offset, int count)
        {
            int capacity = Read7BitEncodedInt();
            if (capacity == 0) return 0;
            if (capacity < 0) throw new IOException(WeaveUtility.GetResourceString("IO.IO_InvalidStringLen_Len", new object[] { capacity }));
            if (!_kernal.EnsureRead(capacity)) throw new EndOfStreamException();
            int result = _encoding.GetChars(_kernal.Buffer, _kernal.Position, capacity, buffer, offset);
            _kernal.Position += capacity;
            return result;
        }

        public sbyte ReadSByte()
        {
            if (!_kernal.EnsureRead(1)) return SByte.MinValue;
            return (sbyte)_kernal.Buffer[_kernal.Position++];
        }

        public unsafe short ReadInt16()
        {
            if (!_kernal.EnsureRead(2)) return 0;
            short toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((short*)(pBuffer + _kernal.Position));
            _kernal.Position += 2;
            return toReturn;
        }

        public unsafe int ReadInt32()
        {
            if (!_kernal.EnsureRead(4)) return 0;
            int toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((int*)(pBuffer + _kernal.Position));
            _kernal.Position += 4;
            return toReturn;
        }

        public unsafe long ReadInt64()
        {
            if (!_kernal.EnsureRead(8)) return 0;
            long toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((long*)(pBuffer + _kernal.Position));
            _kernal.Position += 8;
            return toReturn;
        }

        public unsafe decimal ReadDecimal()
        {
            if (!_kernal.EnsureRead(16)) return 0;
            decimal toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((decimal*)(pBuffer + _kernal.Position));
            _kernal.Position += 16;
            return toReturn;
        }

        public byte ReadByte()
        {
            if (!_kernal.EnsureRead(1)) return Byte.MinValue;
            return _kernal.Buffer[_kernal.Position++];
        }

        public unsafe ushort ReadUInt16()
        {
            if (!_kernal.EnsureRead(2)) return UInt16.MinValue;
            ushort toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((ushort*)(pBuffer + _kernal.Position));
            _kernal.Position += 2;
            return toReturn;
        }

        public unsafe uint ReadUInt32()
        {
            if (!_kernal.EnsureRead(4)) return UInt32.MinValue;
            uint toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((uint*)(pBuffer + _kernal.Position));
            _kernal.Position += 4;
            return toReturn;
        }

        public unsafe ulong ReadUInt64()
        {
            if (!_kernal.EnsureRead(8)) return UInt64.MinValue;
            ulong toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((ulong*)(pBuffer + _kernal.Position));
            _kernal.Position += 8;
            return toReturn;
        }

        public unsafe float ReadSingle()
        {
            if (!_kernal.EnsureRead(4)) return 0.0F;
            float toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((float*)(pBuffer + _kernal.Position));
            _kernal.Position += 4;
            return toReturn;
        }

        public unsafe double ReadDouble()
        {
            if (!_kernal.EnsureRead(8)) return 0.0;
            double toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((double*)(pBuffer + _kernal.Position));
            _kernal.Position += 8;
            return toReturn;
        }

        public unsafe Version ReadVersion()
        {
            if (!_kernal.EnsureRead(16)) return new Version();

            Version toReturn = null;

            fixed (byte* pBuffer = _kernal.Buffer)
            {
                int major = *((int*)(pBuffer + _kernal.Position));
                int minor = *((int*)(pBuffer + _kernal.Position + 4));
                int build = *((int*)(pBuffer + _kernal.Position + 8));
                int revision = *((int*)(pBuffer + _kernal.Position + 12));
                toReturn = new Version(major, minor, build, revision);
            }

            _kernal.Position += 16;
            return toReturn;
        }

        public unsafe DateTime ReadDateTime()
        {
            if (!_kernal.EnsureRead(8)) return DateTime.FromOADate(0.0);
            double toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((double*)(pBuffer + _kernal.Position));
            _kernal.Position += 8;
            return DateTime.FromOADate(toReturn);
        }

        public unsafe TimeSpan ReadTimeSpan()
        {
            if (!_kernal.EnsureRead(8)) return TimeSpan.FromMilliseconds(0.0);
            double toReturn = 0;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((double*)(pBuffer + _kernal.Position));
            _kernal.Position += 8;
            return TimeSpan.FromMilliseconds(toReturn);
        }

        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        public unsafe TwoOctetUnion ReadTwoOctet()
        {
            if (!_kernal.EnsureRead(TwoOctetUnion.SizeInBytes)) return TwoOctetUnion.Empty;
            TwoOctetUnion toReturn = TwoOctetUnion.Empty;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((TwoOctetUnion*)(pBuffer + _kernal.Position));
            _kernal.Position += TwoOctetUnion.SizeInBytes;
            return toReturn;
        }

        public unsafe FourOctetUnion ReadFourOctet()
        {
            if (!_kernal.EnsureRead(FourOctetUnion.SizeInBytes)) return FourOctetUnion.Empty;
            FourOctetUnion toReturn = FourOctetUnion.Empty;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((FourOctetUnion*)(pBuffer + _kernal.Position));
            _kernal.Position += FourOctetUnion.SizeInBytes;
            return toReturn;
        }

        public unsafe EightOctetUnion ReadEightOctet()
        {
            if (!_kernal.EnsureRead(EightOctetUnion.SizeInBytes)) return EightOctetUnion.Empty;
            EightOctetUnion toReturn = EightOctetUnion.Empty;
            fixed (byte* pBuffer = _kernal.Buffer) toReturn = *((EightOctetUnion*)(pBuffer + _kernal.Position));
            _kernal.Position += EightOctetUnion.SizeInBytes;
            return toReturn;
        }
        #endregion

        #region Disposal Handling
        void IDisposable.Dispose()
        {
        }
        #endregion
    }
}
