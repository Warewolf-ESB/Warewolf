
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
    public sealed class BinaryFileReader : IByteReaderBase
    {
        #region Constants
        private const byte True = 1;
        private const byte False = 0;
        private const int MaxCharBytesSize = 0x80;
        #endregion

        #region Instance Fields
        private bool _twoBytesPerChar;
        private byte[] _buffer;
        private char[] _charBuffer;
        private byte[] _charBytes;
        private Decoder _decoder;
        private int _maxCharsSize;
        private char[] _singleChar;
        private Stream _stream;

        private bool _closeStream;
        private int _bufferLength;
        #endregion

        #region Public Properties
        public Stream BaseStream { get { return _stream; } }
        #endregion

        #region Constructors
        public BinaryFileReader(string path)
            : this(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None), Encoding.ASCII, true)
        {
        }

        public BinaryFileReader(Stream input, bool closeStream = true)
            : this(input, Encoding.ASCII, closeStream)
        {
        }

        public BinaryFileReader(Stream input, Encoding encoding, bool closeStream)
            : this(encoding, closeStream)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (!input.CanRead) throw new ArgumentException("Stream is not readable.");
            _stream = input;
        }

        public BinaryFileReader(Encoding encoding, bool closeStream)
        {
            if (encoding == null) throw new ArgumentNullException("encoding");

            _decoder = encoding.GetDecoder();
            _maxCharsSize = encoding.GetMaxCharCount(0x80);

            _bufferLength = encoding.GetMaxByteCount(1);
            if (_bufferLength < 0x10) _bufferLength = 0x10;

            _buffer = new byte[_bufferLength];
            _charBuffer = null;
            _charBytes = null;
            _twoBytesPerChar = encoding is UnicodeEncoding;
            _closeStream = closeStream;
        }
        #endregion

        #region [Reset/Perform] Handling
        public void Reset(Stream input, bool closeStream = true)
        {
            if (_stream != null)
            {
                if (_closeStream) _stream.Close();
                _stream = null;
            }

            _stream = input;

            if (_stream != null)
                if (!_stream.CanRead) 
                    throw new ArgumentException("Stream is not readable.");

            _closeStream = closeStream;
        }

        public Exception Perform(Stream input, bool closeStream, Action<IByteReaderBase> operation)
        {
            if (_stream != null)
            {
                if (_closeStream) _stream.Close();
                _stream = null;
            }

            Exception result = null;
            _closeStream = closeStream;
            _stream = input;

            try { operation(this); }
            catch (Exception e)
            {
                result = e;
            }
            finally
            {
                if (_closeStream)
                {
                    _stream.Close();
                    _stream = null;
                }
            }

            return result;
        }
        #endregion

        #region Buffer Handling
        private void FillBuffer(int numBytes)
        {
            if (_buffer != null && (numBytes < 0 || numBytes > _bufferLength)) throw new ArgumentOutOfRangeException("numBytes", WeaveUtility.GetResourceString("ArgumentOutOfRange_BinaryReaderFillBuffer"));
            if (_stream == null) __Error.FileNotOpen();

            int offset = 0;
            int num2 = 0;

            if (numBytes == 1)
            {
                num2 = _stream.ReadByte();
                if (num2 == -1) __Error.EndOfFile();
                _buffer[0] = (byte)num2;
            }
            else
            {
                do
                {
                    num2 = _stream.Read(_buffer, offset, numBytes - offset);
                    if (num2 == 0) __Error.EndOfFile();
                    offset += num2;
                }
                while (offset < numBytes);
            }
        }

        private unsafe int InternalReadChars(char[] buffer, int index, int count)
        {
            int num = 0;
            int charCount = count;
            if (_charBytes == null) _charBytes = new byte[MaxCharBytesSize];

            while (charCount > 0)
            {
                int num3 = 0;
                num = charCount;

                if (_twoBytesPerChar) num = num << 1;
                if (num > MaxCharBytesSize) num = MaxCharBytesSize;

                int position = 0;
                byte[] charBytes = _charBytes;
                num = _stream.Read(charBytes, 0, num);

                if (num == 0) return count - charCount;

                fixed (byte* numRef = charBytes)
                    fixed (char* chRef = buffer)
                        num3 = _decoder.GetChars(numRef + position, num, chRef + index, charCount, false);

                charCount -= num3;
                index += num3;
            }

            return (count - charCount);
        }

        private int InternalReadOneChar()
        {
            int num = 0;
            int byteCount = 0;
            long position = 0L;

            if (_stream.CanSeek) position = _stream.Position;
            if (_charBytes == null) _charBytes = new byte[MaxCharBytesSize];
            if (_singleChar == null) _singleChar = new char[1];

            while (num == 0)
            {
                byteCount = _twoBytesPerChar ? 2 : 1;
                int num4 = _stream.ReadByte();
                _charBytes[0] = (byte)num4;
                if (num4 == -1) byteCount = 0;

                if (byteCount == 2)
                {
                    num4 = _stream.ReadByte();
                    _charBytes[1] = (byte)num4;
                    if (num4 == -1) byteCount = 1;
                }

                if (byteCount == 0) return -1;

                try
                {
                    num = _decoder.GetChars(_charBytes, 0, byteCount, _singleChar, 0);
                    continue;
                }
                catch
                {
                    if (_stream.CanSeek) _stream.Seek(position - _stream.Position, SeekOrigin.Current);
                    throw;
                }
            }

            if (num == 0) return -1;
            return _singleChar[0];
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
            FillBuffer(1);
            return _buffer[0] != False;
        }

        public char ReadChar()
        {
            if (_stream == null) __Error.FileNotOpen();
            int value = InternalReadOneChar();
            if (value == -1) __Error.EndOfFile();
            return (char)value;
        }

        public string ReadString()
        {
            if (_stream == null) __Error.FileNotOpen();
            
            int capacity = Read7BitEncodedInt();

            if (capacity == 0) return String.Empty;
            if (capacity < 0) throw new IOException(WeaveUtility.GetResourceString("IO.IO_InvalidStringLen_Len", new object[] { capacity }));
            
            if (_charBytes == null) _charBytes = new byte[MaxCharBytesSize];
            if (_charBuffer == null) _charBuffer = new char[_maxCharsSize];

            StringBuilder builder = null;
            int num = 0;

            do
            {
                int count = ((capacity - num) > MaxCharBytesSize) ? MaxCharBytesSize : (capacity - num);
                int byteCount = _stream.Read(_charBytes, 0, count);
                if (byteCount == 0) __Error.EndOfFile();
                int length = _decoder.GetChars(_charBytes, 0, byteCount, _charBuffer, 0);
                if (num == 0 && byteCount == capacity) return new String(_charBuffer, 0, length);
                if (builder == null) builder = new StringBuilder(capacity);
                builder.Append(_charBuffer, 0, length);
                num += byteCount;
            }
            while (num < capacity);

            return builder.ToString();
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count", WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (_stream == null) __Error.FileNotOpen();
            byte[] buffer = new byte[count];
            int offset = 0;

            do
            {
                int read = _stream.Read(buffer, offset, count);
                if (read == 0) break;
                offset += read;
                count -= read;
            }
            while (count > 0);

            if (offset != buffer.Length)
            {
                byte[] dst = new byte[offset];
                Buffer.BlockCopy(buffer, 0, dst, 0, offset);
                buffer = dst;
            }

            return buffer;
        }

        public int ReadBytes(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer", WeaveUtility.GetResourceString("ArgumentNull_Buffer"));
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0) throw new ArgumentOutOfRangeException("count", WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (_stream == null) __Error.FileNotOpen();
            return _stream.Read(buffer, offset, count);
        }

        public char[] ReadChars(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count", WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (_stream == null) __Error.FileNotOpen();
            char[] buffer = new char[count];
            int num = InternalReadChars(buffer, 0, count);

            if (num != count)
            {
                char[] dst = new char[num];
                Buffer.BlockCopy(buffer, 0, dst, 0, 2 * num);
                buffer = dst;
            }

            return buffer;
        }

        public int ReadChars(char[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer", WeaveUtility.GetResourceString("ArgumentNull_Buffer"));
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0) throw new ArgumentOutOfRangeException("count", WeaveUtility.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (_stream == null) __Error.FileNotOpen();
            return InternalReadChars(buffer, offset, count);
        }

        public sbyte ReadSByte()
        {
            FillBuffer(1);
            return (sbyte)_buffer[0];
        }

        public unsafe short ReadInt16()
        {
            FillBuffer(2);
            short toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((short*)pBuffer);
            return toReturn;
        }

        public unsafe int ReadInt32()
        {
            FillBuffer(4);
            int toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((int*)pBuffer);
            return toReturn;
        }

        public unsafe long ReadInt64()
        {
            FillBuffer(8);
            long toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((long*)pBuffer);
            return toReturn;
        }

        public unsafe decimal ReadDecimal()
        {
            FillBuffer(16);
            decimal toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((decimal*)pBuffer);
            return toReturn;
        }

        public byte ReadByte()
        {
            if (_stream == null) __Error.FileNotOpen();
            int num = _stream.ReadByte();
            if (num == -1) __Error.EndOfFile();
            return (byte)num;
        }

        public unsafe ushort ReadUInt16()
        {
            FillBuffer(2);
            ushort toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((ushort*)pBuffer);
            return toReturn;
        }

        public unsafe uint ReadUInt32()
        {
            FillBuffer(4);
            uint toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((uint*)pBuffer);
            return toReturn;
        }

        public unsafe ulong ReadUInt64()
        {
            FillBuffer(8);
            ulong toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((ulong*)pBuffer);
            return toReturn;
        }

        public unsafe float ReadSingle()
        {
            FillBuffer(4);
            float toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((float*)pBuffer);
            return toReturn;
        }

        public unsafe double ReadDouble()
        {
            FillBuffer(8);
            double toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((double*)pBuffer);
            return toReturn;
        }

        public unsafe Version ReadVersion()
        {
            FillBuffer(16);
            Version toReturn = null;

            fixed (byte* pBuffer = _buffer)
            {
                int major = *((int*)pBuffer);
                int minor = *((int*)(pBuffer + 4));
                int build = *((int*)(pBuffer + 8));
                int revision = *((int*)(pBuffer + 12));
                toReturn = new Version(major, minor, build, revision);
            }

            return toReturn;
        }

        public unsafe DateTime ReadDateTime()
        {
            FillBuffer(8);
            double toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((double*)pBuffer);
            return DateTime.FromOADate(toReturn);
        }

        public unsafe TimeSpan ReadTimeSpan()
        {
            FillBuffer(8);
            double toReturn = 0;
            fixed (byte* pBuffer = _buffer) toReturn = *((double*)pBuffer);
            return TimeSpan.FromMilliseconds(toReturn);
        }

        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        public unsafe TwoOctetUnion ReadTwoOctet()
        {
            FillBuffer(2);
            TwoOctetUnion toReturn = TwoOctetUnion.Empty;
            fixed (byte* pBuffer = _buffer) toReturn = *((TwoOctetUnion*)pBuffer);
            return toReturn;
        }

        public unsafe FourOctetUnion ReadFourOctet()
        {
            FillBuffer(4);
            FourOctetUnion toReturn = FourOctetUnion.Empty;
            fixed (byte* pBuffer = _buffer) toReturn = *((FourOctetUnion*)pBuffer);
            return toReturn;
        }

        public unsafe EightOctetUnion ReadEightOctet()
        {
            FillBuffer(8);
            EightOctetUnion toReturn = EightOctetUnion.Empty;
            fixed (byte* pBuffer = _buffer) toReturn = *((EightOctetUnion*)pBuffer);
            return toReturn;
        }
        #endregion

        #region Disposal Handling
        public void Dispose()
        {
            Stream stream = _stream;
            _stream = null;
            if (stream != null && _closeStream) stream.Close();

            _stream = null;
            _buffer = null;
            _decoder = null;
            _charBytes = null;
            _singleChar = null;
            _charBuffer = null;
        }
        #endregion
    }
}
