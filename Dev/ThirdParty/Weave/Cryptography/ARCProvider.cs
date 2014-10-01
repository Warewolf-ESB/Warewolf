
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
using System.Security;
using System.Security.Cryptography;

namespace System.Cryptography
{
    public sealed class ARCProvider : ICryptProvider
    {
        #region Static Members
        private static byte[] ExecuteHMAC(byte[] seed, byte[] key)
        {
            HMACSHA1 signingKey = new HMACSHA1(seed);
            return signingKey.ComputeHash(key);
        }
        #endregion

        #region Instance Fields
        private Random _ksrn;
        private int _ksrnSeed;
        private byte[] _sessionKey;
        private ARC4 _inboundRC4;
        private ARC4 _outboundRC4;
        #endregion

        #region Public Properties
        public byte[] Key { get { return _sessionKey; } }
        #endregion

        #region Constructor
        public ARCProvider(int ksrnSeed, byte[] sessionKey, byte[] inboundSeed, byte[] outboundSeed)
        {
            _ksrn = new Random(_ksrnSeed = ksrnSeed);
            _sessionKey = sessionKey;
            _inboundRC4 = new ARC4(ExecuteHMAC(inboundSeed, _sessionKey), 1024);
            _outboundRC4 = new ARC4(ExecuteHMAC(outboundSeed, _sessionKey), 1024);
        }
        #endregion

        #region [Advance/Retreat] Handling
        public void Advance(bool outbound, int count)
        {
            (outbound ? _outboundRC4 : _inboundRC4).Advance(count);
        }

        public void Retreat(bool outbound, int count)
        {
            (outbound ? _outboundRC4 : _inboundRC4).Retreat(count);
        }
        #endregion

        #region KSRN Handling
        public int NextKSRN()
        {
            return _ksrn.Next(256);
        }
        #endregion

        #region Encryption Handling
        public byte Encrypt(byte decrypted)
        {
            return (byte)(decrypted ^ _outboundRC4.Advance());
        }

        public byte[] Encrypt(byte[] decrypted)
        {
            int length = decrypted.Length;
            byte[] encrypted = new byte[length];
            Encrypt(decrypted, 0, encrypted, 0, length);
            return encrypted;
        }

        public void Encrypt(byte[] data, int offset, int length)
        {
            Encrypt(data, offset, data, offset, length);
        }

        public void Encrypt(byte[] decrypted, byte[] encrypted)
        {
            Encrypt(decrypted, 0, encrypted, 0, decrypted.Length);
        }

        public void Encrypt(byte[] decrypted, int decryptOffset, byte[] encrypted, int encryptOffset, int count)
        {
            for (int i = 0; i < count; i++)
                encrypted[i + encryptOffset] = (byte)(decrypted[i + decryptOffset] ^ _outboundRC4.Advance());
        }
        #endregion

        #region Decryption Handling
        public byte Decrypt(byte encrypted)
        {
            return (byte)(encrypted ^ _inboundRC4.Advance());
        }

        public byte[] Decrypt(byte[] encrypted)
        {
            int length = encrypted.Length;
            byte[] decrypted = new byte[length];
            Decrypt(encrypted, 0, decrypted, 0, length);
            return decrypted;
        }

        public void Decrypt(byte[] data, int offset, int length)
        {
            Decrypt(data, offset, data, offset, length);
        }

        public void Decrypt(byte[] encrypted, byte[] decrypted)
        {
            Decrypt(encrypted, 0, decrypted, 0, encrypted.Length);
        }

        public void Decrypt(byte[] encrypted, int encryptOffset, byte[] decrypted, int decryptOffset, int count)
        {
            for (int i = 0; i < count; i++)
                decrypted[i + decryptOffset] = (byte)(encrypted[i + encryptOffset] ^ _inboundRC4.Advance());
        }
        #endregion

        #region ARC4
        private sealed class ARC4
        {
            private int _prgaIndex;
            private byte[] _prgaBuffer;
            private byte[] _s;
            private byte _i;
            private byte _j;

            public ARC4(byte[] key, int disgard)
            {
                int kLength = key.Length;
                _s = new byte[256];
                for (int i = 0; i < _s.Length; i++) _s[i] = (byte)i;

                byte swap = 0;

                for (int i = 0, j = 0; i < _s.Length; i++)
                {
                    j = (byte)((j + key[i % kLength] + _s[i]) % 256);
                    swap = _s[i];
                    _s[i] = _s[j];
                    _s[j] = swap;
                }

                _i = _j = 0;

                for (int i = 0; i < disgard; i++) GeneratePRGA();

                _prgaBuffer = new byte[1024];
                for (int i = 512; i < _prgaBuffer.Length; i++) _prgaBuffer[i] = GeneratePRGA();
                _prgaIndex = 512;
            }

            public byte Advance()
            {
                if (_prgaIndex == 1024)
                {
                    Buffer.BlockCopy(_prgaBuffer, 512, _prgaBuffer, 0, 512);
                    for (int i = 512; i < _prgaBuffer.Length; i++) _prgaBuffer[i] = GeneratePRGA();
                    _prgaIndex = 512;
                }

                return _prgaBuffer[_prgaIndex++];
            }

            public void Advance(int count)
            {
                _prgaIndex += count;
            }

            public void Retreat(int count)
            {
                _prgaIndex -= count;
            }

            private byte GeneratePRGA()
            {
                _i = (byte)((_i + 1) % 256);
                _j = (byte)((_j + _s[_i]) % 256);

                byte swap = _s[_i];
                _s[_i] = _s[_j];
                _s[_j] = swap;

                return _s[(_s[_i] + _s[_j]) % 256];
            }
        }
        #endregion
    }
}
