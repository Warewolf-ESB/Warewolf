
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
    public sealed class BinaryFileWriter : IByteWriterBase
    {
        #region Constants
        private const byte True = 1;
        private const byte False = 0;
        private const int LargeBufferSize = 0x100;
        #endregion

        #region Instance Fields
        private Stream _stream;
        private Encoder _encoder;
        private Encoding _encoding;
        private byte[] _largeBuffer;
        private byte[] _buffer;
        private int _maxChars;
        private bool _closeStream;
        #endregion

        #region Public Properties
        public Stream BaseStream { get { Flush(); return _stream; } }
        #endregion

        #region Constructors
        public BinaryFileWriter(string path, bool append)
            : this(File.Open(path, append ? FileMode.Open : FileMode.Create, FileAccess.Write, FileShare.None), Encoding.ASCII)
        {
        }

        public BinaryFileWriter(Stream output)
            : this(output, Encoding.ASCII)
        {
        }

        public BinaryFileWriter(Stream output, Encoding encoding, bool closeStream = true)
        {
            if (output == null) throw new ArgumentNullException("output");
            if (encoding == null) throw new ArgumentNullException("encoding");
            if (!output.CanWrite) throw new ArgumentException("Stream is not writable.");

            _stream = output;
            _buffer = new byte[0x10];
            _encoding = encoding;
            _closeStream = closeStream;
        }
        #endregion

        #region [Flush/Seek] Handling
        public void Flush()
        {
            _stream.Flush();
        }

        public long Seek(int offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
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

        public unsafe void Write(char value)
        {
            if (Char.IsSurrogate(value)) throw new ArgumentException();
            if (_encoder == null) _encoder = _encoding.GetEncoder();
            int count = 0;
            fixed (byte* buffer = _buffer) count = _encoder.GetBytes(&value, 1, buffer, 0x10, true);
            _stream.Write(_buffer, 0, count);
        }

        public unsafe void Write(string value)
        {
            int length = value == null ? 0 : value.Length;

            if (length == 0)
            {
                Write7BitEncodedInt(0);
                return;
            }

            int byteCount = _encoding.GetByteCount(value);
            Write7BitEncodedInt(byteCount);

            if (_largeBuffer == null)
            {
                _largeBuffer = new byte[LargeBufferSize];
                _maxChars = LargeBufferSize / _encoding.GetMaxByteCount(1);
            }

            if (byteCount <= LargeBufferSize)
            {
                _encoding.GetBytes(value, 0, length, _largeBuffer, 0);
                _stream.Write(_largeBuffer, 0, byteCount);
            }
            else
            {
                if (_encoder == null) _encoder = _encoding.GetEncoder();
                int charCount;
                int offset = 0;

                for (int i = length; i > 0; i -= charCount)
                {
                    charCount = (i > _maxChars) ? _maxChars : i;

                    fixed (char* str = value)
                    {
                        int consumed;
                        char* chPtr = str;

                        fixed (byte* buffer = _largeBuffer)
                            consumed = _encoder.GetBytes(chPtr + offset, charCount, buffer, LargeBufferSize, charCount == i);

                        _stream.Write(_largeBuffer, 0, consumed);
                        offset += charCount;
                    }
                }
            }
        }

        public void Write(byte[] value)
        {
            _stream.Write(value, 0, value.Length);
        }

        public void Write(byte[] value, int offset, int count)
        {
            _stream.Write(value, offset, count);
        }

        public void Write(char[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            byte[] buffer = _encoding.GetBytes(value, 0, value.Length);
            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Write(char[] value, int offset, int count)
        {
            byte[] buffer = _encoding.GetBytes(value, offset, count);
            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Write(sbyte value)
        {
            _stream.WriteByte((byte)value);
        }

        public unsafe void Write(short value)
        {
            fixed (byte* pBuffer = _buffer) *((short*)pBuffer) = value;
            this._stream.Write(_buffer, 0, 2);
        }

        public unsafe void Write(int value)
        {
            fixed (byte* pBuffer = _buffer) *((int*)pBuffer) = value;
            _stream.Write(_buffer, 0, 4);
        }

        public unsafe void Write(long value)
        {
            fixed (byte* pBuffer = _buffer) *((long*)pBuffer) = value;
            _stream.Write(_buffer, 0, 8);
        }

        public unsafe void Write(decimal value)
        {
            fixed (byte* pBuffer = _buffer) *((decimal*)pBuffer) = value;
            _stream.Write(_buffer, 0, 16);
        }

        public void Write(byte value)
        {
            _stream.WriteByte(value);
        }

        public unsafe void Write(ushort value)
        {
            fixed (byte* pBuffer = _buffer) *((ushort*)pBuffer) = value;
            _stream.Write(_buffer, 0, 2);
        }

        public unsafe void Write(uint value)
        {
            fixed (byte* pBuffer = _buffer) *((uint*)pBuffer) = value;
            _stream.Write(_buffer, 0, 4);
        }

        public unsafe void Write(ulong value)
        {
            fixed (byte* pBuffer = _buffer) *((ulong*)pBuffer) = value;
            _stream.Write(_buffer, 0, 8);
        }

        public unsafe void Write(float value)
        {
            fixed (byte* pBuffer = _buffer) *((float*)pBuffer) = value;
            _stream.Write(_buffer, 0, 4);
        }

        public unsafe void Write(double value)
        {
            fixed (byte* pBuffer = _buffer) *((double*)pBuffer) = value;
            _stream.Write(_buffer, 0, 8);
        }

        public unsafe void Write(Version value)
        {
            fixed (byte* pBuffer = _buffer)
            {
                *((int*)pBuffer) = value.Major;
                *((int*)(pBuffer + 4)) = value.Minor;
                *((int*)(pBuffer + 8)) = value.Build;
                *((int*)(pBuffer + 12)) = value.Revision;
            }

            _stream.Write(_buffer, 0, 16);
        }

        public unsafe void Write(DateTime value)
        {
            fixed (byte* pBuffer = _buffer) *((double*)pBuffer) = value.ToOADate();
            _stream.Write(_buffer, 0, 8);
        }

        public unsafe void Write(TimeSpan value)
        {
            fixed (byte* pBuffer = _buffer) *((double*)pBuffer) = value.TotalMilliseconds;
            _stream.Write(_buffer, 0, 8);
        }

        public void Write(Guid value)
        {
            Write(value.ToByteArray(), 0, 16);
        }

        public unsafe void Write(TwoOctetUnion value)
        {
            fixed (byte* pBuffer = _buffer) *((TwoOctetUnion*)pBuffer) = value;
            _stream.Write(_buffer, 0, TwoOctetUnion.SizeInBytes);
        }

        public unsafe void Write(FourOctetUnion value)
        {
            fixed (byte* pBuffer = _buffer) *((FourOctetUnion*)pBuffer) = value;
            _stream.Write(_buffer, 0, FourOctetUnion.SizeInBytes);
        }

        public unsafe void Write(EightOctetUnion value)
        {
            fixed (byte* pBuffer = _buffer) *((EightOctetUnion*)pBuffer) = value;
            _stream.Write(_buffer, 0, EightOctetUnion.SizeInBytes);
        }
        #endregion

        #region Disposal Handling
        public void Dispose()
        {
            if (_closeStream)
                _stream.Close();
        }
        #endregion
    }
}
