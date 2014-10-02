
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
    internal sealed class ByteWriterBackend : IByteWriterBase
    {
        #region Constants
        private const byte True = 1;
        private const byte False = 0;
        #endregion

        #region Instance Fields
        private ByteKernal _kernal;
        private Encoding _encoding;
        #endregion

        #region Public Properties
        public int Position { get { return _kernal.Position; } set { _kernal.Position = value; } }
        #endregion

        #region Constructor
        public ByteWriterBackend(ByteKernal kernal)
        {
            _kernal = kernal;
            _encoding = Encoding.ASCII;
        }
        #endregion

        #region Write Handling
        public void Write7BitEncodedInt(int value)
        {
            uint num = (uint)value;

            while (num >= 0x80)
            {
                Write((byte)(num | 0x80));
                num = num >> 7;
            }

            Write((byte)num);
        }
        public void Write(bool value)
        {
            Write(value ? True : False);
        }

        public void Write(char value)
        {
            Write((short)value);
        }

        public void Write(string value)
        {
            int length = value == null ? 0 : value.Length;

            if (length == 0)
            {
                Write7BitEncodedInt(0);
                return;
            }

            Encoding encoding = _encoding;
            int byteCount = encoding.GetByteCount(value);
            Write7BitEncodedInt(byteCount);

            _kernal.SetLength(_kernal.Length + byteCount);
            _encoding.GetBytes(value, 0, length, _kernal.Buffer, _kernal.Position);
            _kernal.Position += byteCount;
        }

        public void Write(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        public void Write(byte[] value, int offset, int count)
        {
            int end = _kernal.Position + count;

            if (end > _kernal.Length) _kernal.EnsureWrite(end);

            if (count <= 8)
            {
                while (--count >= 0)
                    _kernal.Buffer[_kernal.Position + count] = value[offset + count];
            }
            else Buffer.BlockCopy(value, offset, _kernal.Buffer, _kernal.Position, count);

            _kernal.Position = end;
        }

        public void Write(char[] value)
        {
            int length = value.Length;

            if (length == 0)
            {
                Write7BitEncodedInt(0);
                return;
            }

            Encoding encoding = _encoding;
            int byteCount = encoding.GetByteCount(value);
            Write7BitEncodedInt(byteCount);

            _kernal.SetLength(_kernal.Length + byteCount);
            _encoding.GetBytes(value, 0, length, _kernal.Buffer, _kernal.Position);
            _kernal.Position += byteCount;
        }

        public void Write(char[] value, int offset, int count)
        {
            if (count == 0)
            {
                Write7BitEncodedInt(0);
                return;
            }

            Encoding encoding = _encoding;
            int byteCount = encoding.GetByteCount(value, offset, count);
            Write7BitEncodedInt(byteCount);

            _kernal.SetLength(_kernal.Length + byteCount);
            _encoding.GetBytes(value, offset, count, _kernal.Buffer, _kernal.Position);
            _kernal.Position += byteCount;
        }

        public void Write(sbyte value)
        {
            Write((byte)value);
        }

        public unsafe void Write(short value)
        {
            int end = _kernal.Position + 2;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((short*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(int value)
        {
            int end = _kernal.Position + 4;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((int*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(long value)
        {
            int end = _kernal.Position + 8;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((long*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(decimal value)
        {
            int end = _kernal.Position + 16;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((decimal*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public void Write(byte value)
        {
            if (_kernal.Position >= _kernal.Length)
            {
                int end = _kernal.Position + 1;
                bool clear = _kernal.Position > _kernal.Length;
                if (end >= _kernal.Capacity && _kernal.EnsureCapacity(end)) clear = false;
                if (clear) Array.Clear(_kernal.Buffer, _kernal.Length, _kernal.Position - _kernal.Length);
                _kernal.Length = end;
            }

            _kernal.Buffer[_kernal.Position++] = value;
        }

        public unsafe void Write(ushort value)
        {
            int end = _kernal.Position + 2;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((ushort*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(uint value)
        {
            int end = _kernal.Position + 4;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((uint*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(ulong value)
        {
            int end = _kernal.Position + 8;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((ulong*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(float value)
        {
            int end = _kernal.Position + 4;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((float*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(double value)
        {
            int end = _kernal.Position + 8;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((double*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(Version value)
        {
            int end = _kernal.Position + 16;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);

            fixed (byte* pBuffer = _kernal.Buffer)
            {
                *((int*)(pBuffer + _kernal.Position)) = value.Major;
                *((int*)(pBuffer + _kernal.Position + 4)) = value.Minor;
                *((int*)(pBuffer + _kernal.Position + 8)) = value.Build;
                *((int*)(pBuffer + _kernal.Position + 12)) = value.Revision;
            }

            _kernal.Position = end;
        }

        public unsafe void Write(DateTime value)
        {
            int end = _kernal.Position + 8;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((double*)(pBuffer + _kernal.Position)) = value.ToOADate();
            _kernal.Position = end;
        }

        public unsafe void Write(TimeSpan value)
        {
            int end = _kernal.Position + 8;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((double*)(pBuffer + _kernal.Position)) = value.TotalMilliseconds;
            _kernal.Position = end;
        }

        public void Write(Guid value)
        {
            Write(value.ToByteArray(), 0, 16);
        }

        public unsafe void Write(TwoOctetUnion value)
        {
            int end = _kernal.Position + TwoOctetUnion.SizeInBytes;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((TwoOctetUnion*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(FourOctetUnion value)
        {
            int end = _kernal.Position + FourOctetUnion.SizeInBytes;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((FourOctetUnion*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }

        public unsafe void Write(EightOctetUnion value)
        {
            int end = _kernal.Position + EightOctetUnion.SizeInBytes;
            if (end > _kernal.Length) _kernal.EnsureWrite(end);
            fixed (byte* pBuffer = _kernal.Buffer) *((EightOctetUnion*)(pBuffer + _kernal.Position)) = value;
            _kernal.Position = end;
        }
        #endregion

        #region Disposal Handling
        void IDisposable.Dispose()
        {
        }
        #endregion
    }
}
