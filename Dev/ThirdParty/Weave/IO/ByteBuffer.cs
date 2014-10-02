
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
using System.IO;

namespace System
{
    public class ByteBuffer : Stream, IByteStreamBase
    {
        #region Instance Fields
        protected ByteKernal _kernal;
        private IByteReaderBase _reader;
        private IByteWriterBase _writer;
        #endregion

        #region Public Properties
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { return _kernal.Length; } }
        public override long Position { get { return _kernal.Position; } set { _kernal.Position = (int)value; } }
        public byte[] Data { get { return _kernal.Buffer; } }
        #endregion

        #region Constructors
        public ByteBuffer()
            : this(32)
        {
        }

        public ByteBuffer(int initialCapacity)
        {
            _kernal = new ByteKernal();
            _kernal.Capacity = Math.Max(initialCapacity, 32);
            _kernal.Buffer = new byte[_kernal.Capacity];
            _reader = CreateByteReader();
            _writer = CreateByteWriter();
        }

        public ByteBuffer(byte[] data)
            : this(data, data.Length)
        {
        }

        public ByteBuffer(byte[] data, int length)
            : this(data, 0, length, length)
        {
        }

        public ByteBuffer(byte[] data, int index, int length)
            : this(data, index, length, length)
        {
        }

        public ByteBuffer(byte[] data, int index, int length, int capacity)
        {
            _kernal = new ByteKernal();
            _kernal.Capacity = capacity;
            _kernal.Length = length;
            _kernal.Position = index;
            _kernal.Buffer = data;
            _reader = CreateByteReader();
            _writer = CreateByteWriter();
        }
        #endregion

        #region [Get/Set] Handling
        public override void SetLength(long value)
        {
            _kernal.SetLength((int)value);
        }
        #endregion

        #region Create(...) Handling
        protected virtual IByteReaderBase CreateByteReader()
        {
            return new ByteReaderBackend(_kernal);
        }

        protected virtual IByteWriterBase CreateByteWriter()
        {
            return new ByteWriterBackend(_kernal);
        }
        #endregion

        #region [Flush/Seek] Handling
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: _kernal.Position = (int)offset; break;
                case SeekOrigin.Current: _kernal.Position += (int)offset; break;
                case SeekOrigin.End: _kernal.Position = _kernal.Length + (int)offset; break;
            }

            return _kernal.Position;
        }
        #endregion

        #region Read Handling
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _reader.ReadBytes(buffer, offset, count);
        }

        public bool ReadBoolean()
        {
            return _reader.ReadBoolean();
        }

        public char ReadChar()
        {
            return _reader.ReadChar();
        }

        public string ReadString()
        {
            return _reader.ReadString();
        }

        public byte[] ReadBytes(int amount)
        {
            return _reader.ReadBytes(amount);
        }

        public int ReadBytes(byte[] buffer, int offset, int count)
        {
            return _reader.ReadBytes(buffer, offset, count);
        }

        public char[] ReadChars(int amount)
        {
            return _reader.ReadChars(amount);
        }

        public int ReadChars(char[] buffer, int offset, int count)
        {
            return _reader.ReadChars(buffer, offset, count);
        }

        public sbyte ReadSByte()
        {
            return _reader.ReadSByte();
        }

        public short ReadInt16()
        {
            return _reader.ReadInt16();
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public long ReadInt64()
        {
            return _reader.ReadInt64();
        }

        public decimal ReadDecimal()
        {
            return _reader.ReadDecimal();
        }

        public new byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public ushort ReadUInt16()
        {
            return _reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return _reader.ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            return _reader.ReadUInt64();
        }

        public float ReadSingle()
        {
            return _reader.ReadSingle();
        }

        public double ReadDouble()
        {
            return _reader.ReadDouble();
        }

        public Version ReadVersion()
        {
            return _reader.ReadVersion();
        }

        public DateTime ReadDateTime()
        {
            return _reader.ReadDateTime();
        }

        public TimeSpan ReadTimeSpan()
        {
            return _reader.ReadTimeSpan();
        }

        public Guid ReadGuid()
        {
            return _reader.ReadGuid();
        }

        public TwoOctetUnion ReadTwoOctet()
        {
            return _reader.ReadTwoOctet();
        }

        public FourOctetUnion ReadFourOctet()
        {
            return _reader.ReadFourOctet();
        }

        public EightOctetUnion ReadEightOctet()
        {
            return _reader.ReadEightOctet();
        }
        #endregion

        #region Write Handling
        public void Write(bool value)
        {
            _writer.Write(value);
        }

        public void Write(char value)
        {
            _writer.Write(value);
        }

        public void Write(string value)
        {
            _writer.Write(value);
        }

        public void Write(byte[] value)
        {
            _writer.Write(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _writer.Write(buffer, offset, count);
        }

        public void Write(char[] value)
        {
            _writer.Write(value);
        }

        public void Write(char[] value, int offset, int count)
        {
            _writer.Write(value, offset, count);
        }

        public void Write(sbyte value)
        {
            _writer.Write(value);
        }

        public void Write(short value)
        {
            _writer.Write(value);
        }

        public void Write(int value)
        {
            _writer.Write(value);
        }

        public void Write(long value)
        {
            _writer.Write(value);
        }

        public void Write(decimal value)
        {
            _writer.Write(value);
        }

        public void Write(byte value)
        {
            _writer.Write(value);
        }

        public void Write(ushort value)
        {
            _writer.Write(value);
        }

        public void Write(uint value)
        {
            _writer.Write(value);
        }

        public void Write(ulong value)
        {
            _writer.Write(value);
        }

        public void Write(float value)
        {
            _writer.Write(value);
        }

        public void Write(double value)
        {
            _writer.Write(value);
        }

        public void Write(Version value)
        {
            _writer.Write(value);
        }

        public void Write(DateTime value)
        {
            _writer.Write(value);
        }

        public void Write(TimeSpan value)
        {
            _writer.Write(value);
        }

        public void Write(Guid value)
        {
            _writer.Write(value);
        }

        public void Write(TwoOctetUnion value)
        {
            _writer.Write(value);
        }

        public void Write(FourOctetUnion value)
        {
            _writer.Write(value);
        }

        public void Write(EightOctetUnion value)
        {
            _writer.Write(value);
        }
        #endregion
    }
}
