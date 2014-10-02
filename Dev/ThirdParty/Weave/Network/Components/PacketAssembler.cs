
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

namespace System.Network
{
    public sealed class PacketAssembler
    {
        #region Constants
        private const int NotSupported = -1;
        private const int NotFixed = -2;
        private const byte PHFMask = 1 + 2 + 4 + 8;
        private const byte PHIMask = 16 + 32 + 64 + 128;
        #endregion

        #region Instance Fields
        private NetworkHost _host;
        private Connection _owner;
        private IDecryptionProvider _decryptor;
        private int _secureStage;

        private int _channel;
        private PacketHeaderFlags _flags;
        private ushort _packetID;
        private PacketTemplate _template;

        private int _dataLength;
        private int _dataIndex;
        private byte[] _dataStorage;

        private PacketData _assembled;
        private bool _extended;
        #endregion

        #region Constructor
        public PacketAssembler(Connection owner, IDecryptionProvider decryptor)
        {
            _host = owner.Host;
            _owner = owner;
            _decryptor = decryptor;
        }
        #endregion

        #region Assembly Handling
        public bool Assemble(byte[] data, int index, int totalLength)
        {
            while (index >= 0 && index != totalLength) index = AssembleSecured(data, index, totalLength);
            if (index == -1) return false;
            return true;
        }

        private int AssembleSecured(byte[] data, int index, int totalLength)
        {
            if (_secureStage < 2) index = AssembleHeader(data, index, totalLength);
            if (_secureStage < 4) index = AssembleData(data, index, totalLength);
            if (_secureStage < 6) index = AssembleExtensionHeader(data, index, totalLength);
            if (_secureStage < 8) index = AssembleExtensionData(data, index, totalLength);

            if (_secureStage == 8 && index != -1)
            {
                if (_extended)
                {
                    _host.Dispatch(_owner, new PacketData(_channel, _dataLength, _dataStorage, _packetID), _assembled, (_flags & PacketHeaderFlags.Extended) != 0);
                    _assembled.Data = null;
                }
                else _host.Dispatch(_owner, _channel, _packetID, _dataLength, _dataStorage, (_flags & PacketHeaderFlags.Extended) != 0);

                _template = null;
                _dataStorage = null;
                _secureStage = 0;
                _extended = false;
            }

            return index;
        }

        private int AssembleHeader(byte[] data, int index, int totalLength)
        {
            if (index == -1) return -1;

            if (_secureStage == 0)
            {
                if (index >= totalLength) return index;
                ++_secureStage;
                byte current = _decryptor.Decrypt(data[index++]);
                _channel = current & ~PHIMask;

                if (!_host.ValidateChannel(_channel))
                {
                    Fail(FailureReason.InvalidChannel);
                    return -1;
                }

                _flags = (PacketHeaderFlags)(current & ~PHFMask);
                _extended = false;
            }

            if (_secureStage == 1)
            {
                if ((_flags & PacketHeaderFlags.Identifier16) != 0)
                {
                    if (index + 1 >= totalLength) return index;
                    _decryptor.Decrypt(data, index, 2);
                    _packetID = new TwoOctetUnion(data[index], data[index + 1]).UInt16;
                    index += 2;
                }
                else
                {
                    if (index >= totalLength) return index;
                    _packetID = _decryptor.Decrypt(data[index++]);
                }

                if ((_template = _host.AcquirePacketTemplate(_channel, _packetID)) == null)
                {
                    Fail(FailureReason.InvalidPacketID);
                    return -1;
                }

                _dataIndex = 0;

                if (_template._dataComponent)
                {
                    if (_template._variableDataLength) ++_secureStage;
                    else
                    {
                        _dataStorage = new byte[_dataLength = _template._dataLength];
                        _secureStage += 2;
                    }
                }
                else _secureStage += 3;
            }

            return index;
        }

        private int AssembleData(byte[] data, int index, int totalLength)
        {
            if (index == -1) return -1;

            if (_secureStage == 2)
            {
                if ((_flags & PacketHeaderFlags.Length32) != 0)
                {
                    if (index + 3 >= totalLength) return index;
                    _decryptor.Decrypt(data, index, 4);
                    _dataLength = new FourOctetUnion(data[index], data[index + 1], data[index + 2], data[index + 3]).Int32;
                    index += 4;
                }
                else if ((_flags & PacketHeaderFlags.Length16) != 0)
                {
                    if (index + 1 >= totalLength) return index;
                    _decryptor.Decrypt(data, index, 2);
                    _dataLength = new TwoOctetUnion(data[index], data[index + 1]).UInt16;
                    index += 2;
                }
                else
                {
                    if (index >= totalLength) return index;
                    _dataLength = _decryptor.Decrypt(data[index++]);
                }

                _dataStorage = new byte[_dataLength];
                ++_secureStage;
            }

            if (_secureStage == 3)
            {
                int remaining = _dataLength - _dataIndex;

                if (index + remaining > totalLength)
                {
                    Buffer.BlockCopy(data, index, _dataStorage, _dataIndex, remaining = totalLength - index);
                    _dataIndex += remaining;
                    return totalLength;
                }
                else
                {
                    if (remaining != 0)
                    {
                        Buffer.BlockCopy(data, index, _dataStorage, _dataIndex, remaining);
                        index += remaining;
                    }

                    ++_secureStage;
                }
            }

            return index;
        }

        private int AssembleExtensionHeader(byte[] data, int index, int totalLength)
        {
            if (index == -1) return -1;

            if (_secureStage == 4)
            {
                if ((_flags & PacketHeaderFlags.Extended) == 0)
                {
                    _extended = false;
                    _secureStage += 4;
                    return index;
                }

                if (index >= totalLength) return index;
                _assembled = new PacketData(_channel, _dataLength, _dataStorage, _packetID);
                _extended = true;
                _template = null;
                _dataStorage = null;
                ++_secureStage;

                byte current = data[index++];
                _channel = current & ~PHIMask;

                if ((_template = _host.AcquireExtensionTemplate(_channel)) == null)
                {
                    Fail(FailureReason.InvalidExtension);
                    return -1;
                }

                _flags = (PacketHeaderFlags)(current & ~PHFMask);
            }

            if (_secureStage == 5)
            {
                if ((_flags & PacketHeaderFlags.Identifier16) != 0)
                {
                    if (index + 1 >= totalLength) return index;
                    _packetID = new TwoOctetUnion(data[index], data[index + 1]).UInt16;
                    index += 2;
                }
                else
                {
                    if (index >= totalLength) return index;
                    _packetID = data[index++];
                }

                _dataIndex = 0;

                if (_template._dataComponent)
                {
                    if (_template._variableDataLength) ++_secureStage;
                    else
                    {
                        _dataStorage = new byte[_dataLength = _template._dataLength];
                        _secureStage += 2;
                    }
                }
                else _secureStage += 3;
            }

            return index;
        }

        private int AssembleExtensionData(byte[] data, int index, int totalLength)
        {
            if (index == -1) return -1;

            if (_secureStage == 6)
            {
                if ((_flags & PacketHeaderFlags.Length32) != 0)
                {
                    if (index + 3 >= totalLength) return index;
                    _dataLength = new FourOctetUnion(data[index], data[index + 1], data[index + 2], data[index + 3]).Int32;
                    index += 4;
                }
                else if ((_flags & PacketHeaderFlags.Length16) != 0)
                {
                    if (index + 1 >= totalLength) return index;
                    _dataLength = new TwoOctetUnion(data[index], data[index + 1]).UInt16;
                    index += 2;
                }
                else
                {
                    if (index >= totalLength) return index;
                    _dataLength = data[index++];
                }

                _dataStorage = new byte[_dataLength];
                ++_secureStage;
            }

            if (_secureStage == 7)
            {
                int remaining = _dataLength - _dataIndex;

                if (index + remaining > totalLength)
                {
                    Buffer.BlockCopy(data, index, _dataStorage, _dataIndex, remaining = totalLength - index);
                    _dataIndex += remaining;
                    return totalLength;
                }
                else
                {
                    if (remaining != 0)
                    {
                        Buffer.BlockCopy(data, index, _dataStorage, _dataIndex, remaining);
                        index += remaining;
                    }

                    ++_secureStage;
                }
            }

            return index;
        }
        #endregion

        #region Failure Handling
        private void Fail(FailureReason reason)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region FailureReason
        private enum FailureReason
        {
            Unknown,
            InvalidChannel,
            InvalidPacketID,
            InvalidExtension,
        }
        #endregion
    }
}
