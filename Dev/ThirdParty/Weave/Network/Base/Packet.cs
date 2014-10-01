
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;

namespace System.Network
{
    public sealed class Packet : IByteWriterBase
    {
        #region Constants
        private const byte PHFMask = 1 + 2 + 4 + 8;
        private const byte PHCMask = 16 + 32 + 64 + 128;
        #endregion

        #region Instance Fields
        private PacketTemplate _template;
        private ByteKernal _kernal;
        private IByteWriterBase _writer;

        internal PacketFlags _flags;
        internal byte[] _header;

        internal byte _headerLength;
        internal int _dataLength;
        #endregion

        #region Public Properties
        public int Position { get { return _kernal.Position; } set { _kernal.Position = value; } }
        public int Length { get { return _kernal.Length; } }
        public PacketFlags Flags { get { return _flags; } }
        public byte[] Buffer { get { return _kernal.Buffer; } }
        #endregion

        #region Constructor
        public Packet(PacketTemplate template)
        {
            _template = template;

            if(_template._dataComponent)
            {
                _kernal = new ByteKernal();
                _kernal.Capacity = _template._variableDataLength ? 32 : _template._dataLength;
                _kernal.Buffer = new byte[_kernal.Capacity];
                _writer = new ByteWriterBackend(_kernal);
            }
        }
        #endregion

        #region Compilation Handling
        public void Compile()
        {
            if((_flags & PacketFlags.Compiled) != PacketFlags.None) return;
            _flags |= PacketFlags.Compiled;
            PacketHeaderFlags flags = 0;
            _dataLength = _kernal == null ? 0 : _kernal.Length;

#if DEBUG
            if(!_template._variableDataLength && _dataLength != _template._dataLength) throw new ArgumentOutOfRangeException("Compiled length not equal to data length in a fixed length packet.");
#endif
            if(_template._extendedID)
            {
                flags = InstantiateHeader(3) | PacketHeaderFlags.Identifier16;
                TwoOctetUnion id = _template._ushortID;
                _header[1] = id.O1;
                _header[2] = id.O2;
            }
            else
            {
                flags = InstantiateHeader(2);
                _header[1] = _template._byteID;
            }

            _header[0] = (byte)((_template._channel & ~PHCMask) | ((byte)flags & ~PHFMask));
        }

        internal PacketHeaderFlags InstantiateHeader(byte length)
        {
            _headerLength = length;
            PacketHeaderFlags flags = 0;

            if(_template._dataComponent && _template._variableDataLength)
            {
                if(_dataLength > Byte.MaxValue)
                {
                    if(_dataLength > UInt16.MaxValue)
                    {
                        flags |= PacketHeaderFlags.Length32;
                        _header = new byte[_headerLength += 4];
                        FourOctetUnion union = new FourOctetUnion(_dataLength);
                        _header[length] = union.O1;
                        _header[length + 1] = union.O2;
                        _header[length + 2] = union.O3;
                        _header[length + 3] = union.O4;
                    }
                    else
                    {
                        flags |= PacketHeaderFlags.Length16;
                        _header = new byte[_headerLength += 2];
                        TwoOctetUnion union = new TwoOctetUnion((ushort)_dataLength);
                        _header[length] = union.O1;
                        _header[length + 1] = union.O2;
                    }
                }
                else
                {
                    _header = new byte[_headerLength += 1];
                    _header[length] = (byte)_dataLength;
                }
            }
            else _header = new byte[_headerLength];

            return flags;
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

        public void Write(byte[] buffer, int offset, int count)
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

        #region Disposal Handling
        void IDisposable.Dispose()
        {
        }
        #endregion
    }

    public enum PacketFlags : byte
    {
        None = 0,
        Compiled = 1
    }

    public sealed class PacketTemplate
    {
        internal byte _channel;
        internal bool _extendedID;
        internal TwoOctetUnion _ushortID;
        internal byte _byteID;

        internal bool _dataComponent;
        internal bool _variableDataLength;
        internal int _dataLength;

        public PacketTemplate(int channel, int id)
            : this(channel, id, -1)
        {
        }

        public PacketTemplate(int channel, int id, bool dataComponent)
            : this(channel, id, dataComponent ? 0 : -1)
        {
        }

        public PacketTemplate(int channel, int id, int dataLength)
        {
            _channel = (byte)channel;

            if(_extendedID = id > Byte.MaxValue) _ushortID = new TwoOctetUnion((ushort)id);
            else _byteID = (byte)id;

            if(_dataComponent = dataLength >= 0) _variableDataLength = (_dataLength = dataLength) == 0;
        }
    }

    public static class InternalTemplates
    {
        public static readonly PacketTemplate Server_LogoutReceived = new PacketTemplate(15, 0, false);
        public static readonly PacketTemplate Server_OnExecuteStringCommandReceived = new PacketTemplate(15, 1, true);
        public static readonly PacketTemplate Server_OnExecuteBinaryCommandReceived = new PacketTemplate(15, 2, true);
        public static readonly PacketTemplate Server_SendClientDetails = new PacketTemplate(15, 3, true);

        public static readonly PacketTemplate Client_LogoutReceived = new PacketTemplate(15, 0, 1);
        public static readonly PacketTemplate Client_OnExecuteStringCommandReceived = new PacketTemplate(15, 1, true);
        public static readonly PacketTemplate Client_OnExecuteBinaryCommandReceived = new PacketTemplate(15, 2, true);
        public static readonly PacketTemplate Client_OnClientDetailsReceived = new PacketTemplate(15, 3, true);
    }
}
