
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

namespace Dev2.Tests.Weave
{
    public class MockByteReaderWriter : IByteWriterBase, IByteReaderBase
    {
        readonly List<dynamic> _values = new List<dynamic>();
        int _current;

        public void Reset()
        {
            _values.Clear();
            _current = 0;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _values.Clear();
        }

        #endregion

        #region Implementation of IByteWriterBase

        public void Write(bool value)
        {
            _values.Add(value);
        }

        public void Write(char value)
        {
            _values.Add(value);
        }

        public void Write(string value)
        {
            _values.Add(value);
        }

        public void Write(byte[] value)
        {
            _values.Add(value);
        }

        public void Write(byte[] value, int offset, int count)
        {
            _values.Add(value);
        }

        public void Write(char[] value)
        {
            _values.Add(value);
        }

        public void Write(char[] value, int offset, int count)
        {
            _values.Add(value);
        }

        public void Write(sbyte value)
        {
            _values.Add(value);
        }

        public void Write(short value)
        {
            _values.Add(value);
        }

        public void Write(int value)
        {
            _values.Add(value);
        }

        public void Write(long value)
        {
        }

        public void Write(decimal value)
        {
            _values.Add(value);
        }

        public void Write(byte value)
        {
            _values.Add(value);
        }

        public void Write(ushort value)
        {
            _values.Add(value);
        }

        public void Write(uint value)
        {
            _values.Add(value);
        }

        public void Write(ulong value)
        {
            _values.Add(value);
        }

        public void Write(float value)
        {
            _values.Add(value);
        }

        public void Write(double value)
        {
            _values.Add(value);
        }

        public void Write(Version value)
        {
            _values.Add(value);
        }

        public void Write(DateTime value)
        {
            _values.Add(value);
        }

        public void Write(TimeSpan value)
        {
            _values.Add(value);
        }

        public void Write(Guid value)
        {
            _values.Add(value);
        }

        public void Write(TwoOctetUnion value)
        {
            _values.Add(value);
        }

        public void Write(FourOctetUnion value)
        {
            _values.Add(value);
        }

        public void Write(EightOctetUnion value)
        {
            _values.Add(value);
        }

        #endregion

        #region Implementation of IByteReaderBase

        public bool ReadBoolean()
        {
            return _values[_current++];
        }

        public char ReadChar()
        {
            return _values[_current++];
        }

        public string ReadString()
        {
            return _values[_current++];
        }

        public byte[] ReadBytes(int amount)
        {
            return _values[_current++];
        }

        public int ReadBytes(byte[] buffer, int offset, int count)
        {
            return _values[_current++];
        }

        public char[] ReadChars(int amount)
        {
            return _values[_current++];
        }

        public int ReadChars(char[] buffer, int offset, int count)
        {
            return _values[_current++];
        }

        public sbyte ReadSByte()
        {
            return _values[_current++];
        }

        public short ReadInt16()
        {
            return _values[_current++];
        }

        public int ReadInt32()
        {
            return _values[_current++];
        }

        public long ReadInt64()
        {
            return _values[_current++];
        }

        public decimal ReadDecimal()
        {
            return _values[_current++];
        }

        public byte ReadByte()
        {
            return _values[_current++];
        }

        public ushort ReadUInt16()
        {
            return _values[_current++];
        }

        public uint ReadUInt32()
        {
            return _values[_current++];
        }

        public ulong ReadUInt64()
        {
            return _values[_current++];
        }

        public float ReadSingle()
        {
            return _values[_current++];
        }

        public double ReadDouble()
        {
            return _values[_current++];
        }

        public Version ReadVersion()
        {
            return _values[_current++];
        }

        public DateTime ReadDateTime()
        {
            return _values[_current++];
        }

        public TimeSpan ReadTimeSpan()
        {
            return _values[_current++];
        }

        public Guid ReadGuid()
        {
            return _values[_current++];
        }

        public TwoOctetUnion ReadTwoOctet()
        {
            return _values[_current++];
        }

        public FourOctetUnion ReadFourOctet()
        {
            return _values[_current++];
        }

        public EightOctetUnion ReadEightOctet()
        {
            return _values[_current++];
        }

        #endregion
    }
}
