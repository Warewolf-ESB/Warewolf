
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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System.Cryptography
{
    public sealed class SecureRemotePassword
    {
        #region Constants
        public const int MinPassLength = 3;
        public const int MaxPassLength = 16;
        public const int KeyLength = 32;
        #endregion

        #region Readonly Fields
        private static readonly BigInteger _modulus = new BigInteger("B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89", MaxPassLength);
        private static readonly BigInteger _generator = new BigInteger(7);
        private static readonly HashAlgorithm _hash = new SHA1Managed();
        #endregion

        #region Static Members
        private static RandomNumberGenerator _randomGenerator = new RNGCryptoServiceProvider();

        private static BigInteger RandomNumber(int size)
        {
            byte[] buffer = new byte[size];
            _randomGenerator.GetBytes(buffer);
            if (buffer[0] == 0) buffer[0] = 1;
            return new BigInteger(buffer);
        }

        private static BigInteger RandomNumber()
        {
            return RandomNumber(KeyLength);
        }

        private static BigInteger Hash(params HashDataBroker[] brokers)
        {
            return CryptUtility.HashToBigInteger(_hash, brokers);
        }

        public static byte[] GenerateCredentialsHash(string username, string password)
        {
            byte[] buf = _hash.ComputeHash(Encoding.ASCII.GetBytes(String.Format("{0}:{1}", username.ToUpper(), password)));

            if (buf.Length > 20)
            {
                byte[] newBuff = new byte[20];
                Array.Copy(buf, newBuff, 20);
                return newBuff;
            }

            return buf;
        }

        public static byte[] GenerateCredentialsHash(byte[] source)
        {
            byte[] buf = _hash.ComputeHash(source);

            if (buf.Length > 20)
            {
                byte[] newBuff = new byte[20];
                Array.Copy(buf, newBuff, 20);
                return newBuff;
            }

            return buf;
        }
        #endregion

        #region Instance Fields
        private string _username;
        private bool _isServer;
        private BigInteger _credentialsHash;
        private BigInteger _credentials;
        private BigInteger _salt;
        private BigInteger _rawSessionKey;
        private BigInteger _publicEphemeralValueA;
        private BigInteger _secretEphemeralValueA = RandomNumber();
        private BigInteger _publicEphemeralValueB;
        private BigInteger _secretEphemeralValueB;
        private BigInteger _verifier;
        #endregion

        #region Public Properties
        public string Username { get { return _username; } }
        public bool IsServer { get { return _isServer; } }
        public BigInteger ClientSessionKeyProof { get { return Hash(Hash(_modulus) ^ Hash(_generator), Hash(_username), Salt, PublicEphemeralValueA, PublicEphemeralValueB, SessionKey); } }
        public BigInteger ServerSessionKeyProof { get { return Hash(PublicEphemeralValueA, ClientSessionKeyProof, SessionKey); } }
        public BigInteger Modulus { get { return _modulus; } }
        public BigInteger Generator { get { return _generator; } }
        public BigInteger Multiplier { get { return (BigInteger)MinPassLength; } }
        public BigInteger CredentialsHash { get { return _credentialsHash ?? (_credentialsHash = Hash(Salt, _credentials)); } }
        public BigInteger Credentials { get { return _credentials; } set { _credentials = value; } }
        public BigInteger Salt { get { return _salt ?? (_salt = RandomNumber()); } set { _salt = value; } }
        public BigInteger ScramblingParameter { get { return Hash(PublicEphemeralValueA, PublicEphemeralValueB); } }
        public BigInteger PublicEphemeralValueA { get { return _publicEphemeralValueA == null ? (!_isServer ? (_publicEphemeralValueA = _generator.ModPow(_secretEphemeralValueA, _modulus)) : _publicEphemeralValueA) : _publicEphemeralValueA; } set { SetPublicEphemeralValueA(value); } }
        public BigInteger PublicEphemeralValueB { get { return GetPublicEphemeralValueB(); } set { SetPublicEphemeralValueB(value); } }
        public BigInteger SessionKey { get { return GetSessionKey(); } }
        public BigInteger Verifier { get { return _verifier ?? (_verifier = (_verifier = _generator.ModPow(CredentialsHash, _modulus)) < 0 ? _verifier + _modulus : _verifier); } set { _verifier = value; } }
        #endregion

        #region Constructors
        public SecureRemotePassword(string username, BigInteger credentials, bool isServer)
        {
            _username = username.ToUpper();
            _isServer = isServer;
            _credentials = credentials;
        }

        public SecureRemotePassword(string username, BigInteger verifier, BigInteger salt)
        {
            _username = username.ToUpper();
            _isServer = true;
            _verifier = verifier;
            _salt = salt;
        }
        #endregion

        #region [Get/Set] Handling
        private BigInteger GetPublicEphemeralValueB()
        {
            if (_isServer && _publicEphemeralValueB == null)
            {
                _secretEphemeralValueB = RandomNumber();
                _publicEphemeralValueB = Multiplier * Verifier + _generator.ModPow(_secretEphemeralValueB, _modulus);
                _publicEphemeralValueB %= _modulus;
                if (_publicEphemeralValueB < 0) _publicEphemeralValueB += _modulus;
            }

            return _publicEphemeralValueB;
        }

        private BigInteger GetSessionKey()
        {
            if (_rawSessionKey == null)
            {
                BigInteger s;

                if (_isServer)
                {
                    if (_publicEphemeralValueA == null) return null;
                    s = Verifier.ModPow(ScramblingParameter, _modulus);
                    s = (s * PublicEphemeralValueA) % _modulus;
                    s = s.ModPow(_secretEphemeralValueB, _modulus);
                }
                else
                {
                    s = PublicEphemeralValueB - (Multiplier * _generator.ModPow(CredentialsHash, _modulus));
                    s = s.ModPow(_secretEphemeralValueA + ScramblingParameter * CredentialsHash, _modulus);
                    if (s < 0) s += _modulus;
                }

                _rawSessionKey = s;
            }

            byte[] data = _rawSessionKey.GetBytes(KeyLength);
            byte[] temp = new byte[MaxPassLength];
            for (int i = 0; i < temp.Length; i++) temp[i] = data[2 * i];
            byte[] hash1 = Hash(temp).GetBytes(20);
            for (int i = 0; i < temp.Length; i++) temp[i] = data[2 * i + 1];
            byte[] hash2 = Hash(temp).GetBytes(20);

            data = new byte[40];
            for (int i = 0; i < data.Length; i++) data[i] = i % 2 == 0 ? hash1[i / 2] : hash2[i / 2];
            return new BigInteger(data);
        }

        private void SetPublicEphemeralValueA(BigInteger value)
        {
            _publicEphemeralValueA = value;
            _publicEphemeralValueA %= _modulus;
            if (_publicEphemeralValueA < 0) _publicEphemeralValueA += _modulus;
            if (_publicEphemeralValueA == 0) throw new InvalidDataException("A cannot be 0 mod N!");
        }

        private void SetPublicEphemeralValueB(BigInteger value)
        {
            _publicEphemeralValueB = value;
            _publicEphemeralValueB %= _modulus;
            if (_publicEphemeralValueB < 0) _publicEphemeralValueB += _modulus;
            if (_publicEphemeralValueB == 0) throw new InvalidDataException("B cannot be 0 mod N!");
        }
        #endregion

        #region Validation Handling
        public bool IsClientProofValid(BigInteger clientProof)
        {
            return ClientSessionKeyProof == clientProof;
        }

        public bool IsServerProofValid(BigInteger serverProof)
        {
            return serverProof == ServerSessionKeyProof;
        }
        #endregion
    }
}
