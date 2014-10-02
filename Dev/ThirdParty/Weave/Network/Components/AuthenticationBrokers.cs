
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
using System.Cryptography;

namespace System.Network
{
    public abstract class AuthenticationBroker
    {
        #region Instance Fields
        protected Connection _connection;
        protected ICryptProvider _cryptProvider;
        #endregion

        #region Public Properties
        public ICryptProvider CryptProvider { get { return _cryptProvider; } }
        #endregion

        #region Constructor
        protected AuthenticationBroker()
        {
        }
        #endregion

        #region Reset Handling
        protected abstract void Reset();
        #endregion

        #region Data Handling
        internal bool NotifyDataReceived(byte[] data, int length)
        {
            return OnDataReceived(data, length);
        }

        protected virtual void OnAuthenticated(ByteBuffer buffer) { }
        protected abstract bool OnDataReceived(byte[] data, int length);
        #endregion
    }

    public abstract class InboundAuthenticationBroker : AuthenticationBroker
    {
        #region Instance Fields
        protected Firewall _firewall;
        protected int _localIdentifier;
        protected int _targetIdentifier;
        protected Version _localVersion;
        protected Version _targetVersion;
        protected PlatformID _targetPlatform;
        protected string _targetServicePack;
        protected uint _targetFingerprint;
        protected string _targetHostname;
        protected string _targetAccountName;
        protected NetworkAccount _targetAccount;
        #endregion

        #region Public Properties
        public int Identifier { get { return _targetIdentifier; } }
        public Version Version { get { return _targetVersion; } }
        public PlatformID Platform { get { return _targetPlatform; } }
        public string ServicePack { get { return _targetServicePack; } }
        public uint Fingerprint { get { return _targetFingerprint; } }
        public string Hostname { get { return _targetHostname; } }
        #endregion

        #region Instantiation Handling
        internal InboundAuthenticationBroker Instantiate(Firewall firewall, Connection connection)
        {
            InboundAuthenticationBroker toReturn = OnInstantiate();
            toReturn._firewall = firewall;
            toReturn._connection = connection;
            toReturn.OnInstantiated();
            return toReturn;
        }

        protected abstract InboundAuthenticationBroker OnInstantiate();
        protected abstract void OnInstantiated();
        #endregion
    }

    /*
     * Client sends introduction
     * Server sends either auth result or auth result, 4 randoms for KSRN seed, salt and ephemeralB  - DONE
     * Client sends proof
     * Server sends either auth result or auth result and proof - DONE
     * Client sends auth result
     * Server either creates context or disposes connection
     */

    public abstract class InboundSRPAuthenticationBroker : InboundAuthenticationBroker
    {
        #region Static Members
        private static byte[] _outboundSeed = unchecked(new byte[] { (byte)((sbyte)34), (byte)((sbyte)-66), (byte)((sbyte)-27), (byte)((sbyte)-49), (byte)((sbyte)-69), (byte)((sbyte)7), (byte)((sbyte)100), (byte)((sbyte)-39), (byte)((sbyte)0), (byte)((sbyte)69), (byte)((sbyte)27), (byte)((sbyte)-48), (byte)((sbyte)36), (byte)((sbyte)-72), (byte)((sbyte)-43), (byte)((sbyte)69) });
        private static byte[] _inboundSeed = unchecked(new byte[] { (byte)((sbyte)-12), (byte)((sbyte)102), (byte)((sbyte)49), (byte)((sbyte)89), (byte)((sbyte)-4), (byte)((sbyte)-125), (byte)((sbyte)110), (byte)((sbyte)49), (byte)((sbyte)49), (byte)((sbyte)2), (byte)((sbyte)81), (byte)((sbyte)-43), (byte)((sbyte)68), (byte)((sbyte)49), (byte)((sbyte)103), (byte)((sbyte)-104) });
        #endregion

        #region Instance Fields
        private int _stage;
        private int _minLength;

        protected byte[] _ksrnSeed;
        protected SecureRemotePassword _srp;
        #endregion

        #region Reset Handling
        protected override void Reset()
        {
            _stage = 0;
            _targetIdentifier = 0;
            _targetVersion = null;
            _targetPlatform = 0;
            _targetServicePack = null;
            _targetFingerprint = 0;
            _targetHostname = null;
            _targetAccountName = null;
            _targetAccount = null;
            _ksrnSeed = null;
            _srp = null;
        }
        #endregion

        #region Instantiation Handling
        protected override void OnInstantiated()
        {
            _minLength = 39 + NetworkHelper.MinUsernameLength;

            if (_localIdentifier != 0) _minLength += 4;
            if (_localVersion != null) _minLength += 16;
            if (_firewall != null) _minLength += 1;
        }
        #endregion

        #region Data Handling
        protected sealed override bool OnDataReceived(byte[] data, int length)
        {
            if (_stage == -1) return false;
            AuthenticationResponse result = AuthenticationResponse.Unspecified;

            switch (_stage)
            {
                case 0:
                    {
                        result = HandleIntroduction(data, length);

                        if (result == AuthenticationResponse.Success)
                        {
                            byte[] salt = _srp.Salt.GetBytes();
                            ByteBuffer writer = new ByteBuffer(38 + salt.Length);
                            writer.Write((byte)result);
                            _ksrnSeed = new byte[4];
                            for (int i = 0; i < 4; i++) writer.Write(_ksrnSeed[i] = MathHelper.Random((byte)1, Byte.MaxValue));
                            writer.Write((byte)salt.Length);
                            writer.Write(salt);
                            writer.Write(_srp.PublicEphemeralValueB.GetBytes(32));

                            _connection.Send(writer.Data, 0, (int)writer.Length);
                        }
                        else
                        {
                            _connection.Send(new byte[] { (byte)result }, 0, 1);

                            if (result != AuthenticationResponse.InvalidCredentials) return false;
                            else
                            {
                                Reset();
                                return true;
                            }
                        }

                        break;
                    }
                case 1:
                    {
                        result = HandleClientProof(data, length);

                        if (result == AuthenticationResponse.Success)
                        {
                            byte[] response = new byte[21];
                            response[0] = (byte)result;
                            Buffer.BlockCopy(_srp.ServerSessionKeyProof.GetBytes(20), 0, response, 1, 20);
                            _connection.Send(response, 0, 21);
                        }
                        else
                        {
                            if (_firewall != null && !_targetAccount.HasAllPermissions(StandardAccountPermissions.NoBruteForceProtection))
                                _firewall.NotifyLoginAttempt(_connection.Address, _targetAccount.AccountID, false);

                            _connection.Send(new byte[] { (byte)result }, 0, 1);

                            if (result != AuthenticationResponse.InvalidCredentials) return false;
                            else
                            {
                                Reset();
                                return true;
                            }
                        }

                        break;
                    }
                case 2:
                    {
                        result = HandleAuthResult(data, length);

                        if (result == AuthenticationResponse.Success)
                        {
                            ByteBuffer writer = new ByteBuffer(32);
                            writer.Write((byte)result);
                            OnAuthenticated(writer);
                            _connection.Send(writer.Data, 0, (int)writer.Length);
                            _cryptProvider = new ARCProvider((((_ksrnSeed[0] + _ksrnSeed[2]) / _ksrnSeed[1]) * _ksrnSeed[3]) + 50, _srp.SessionKey.GetBytes(40), _inboundSeed, _outboundSeed);
                            _connection.Host.NotifyAuthenticated(_connection, _targetAccount, this);
                        }
                        else
                        {
                            if (result != AuthenticationResponse.InUse && _firewall != null && !_targetAccount.HasAllPermissions(StandardAccountPermissions.NoBruteForceProtection))
                                _firewall.NotifyLoginAttempt(_connection.Address, _targetAccount.AccountID, false);

                            _connection.Send(new byte[] { (byte)result }, 0, 1);

                            if (result != AuthenticationResponse.InUse) return false;
                            else
                            {
                                Reset();
                                return true;
                            }
                        }

                        break;
                    }
            }

            if (result == AuthenticationResponse.Success)
            {
                ++_stage;
                return true;
            }
            else
            {
                _stage = -1;
                return false;
            }
        }
        #endregion

        #region Authentication Handling
        private AuthenticationResponse HandleIntroduction(byte[] data, int length)
        {
            if (length < _minLength) return AuthenticationResponse.Unspecified;

            ByteBuffer buffer = new ByteBuffer(data, length);
            if (_localIdentifier != 0 && (_targetIdentifier = buffer.ReadInt32()) != _localIdentifier) return AuthenticationResponse.Unspecified;
            if (_localVersion != null) _targetVersion = buffer.ReadVersion();

            _targetPlatform = (PlatformID)buffer.ReadByte();
            _targetServicePack = buffer.ReadString();
            _targetFingerprint = buffer.ReadUInt32();

            if (_firewall != null)
            {
                if (_firewall.IsBlocked(_targetFingerprint)) return AuthenticationResponse.MachineBan;
                _targetHostname = buffer.ReadString();
            }

            _targetAccountName = buffer.ReadString().ToUpper();
            if (buffer.Length - buffer.Position != 32) return AuthenticationResponse.Unspecified;
            byte[] credentials = buffer.ReadBytes(32);

            if ((_targetAccount = ResolveAccount(_targetAccountName)) != null)
            {
                if (_firewall != null)
                {
                    if (_firewall.NetworkLockdown && !_targetAccount.HasAllPermissions(StandardAccountPermissions.IgnoreNetworkLockdown)) return AuthenticationResponse.NetworkLockdown;

                    if (!_targetAccount.HasAllPermissions(StandardAccountPermissions.NoBruteForceProtection) && _firewall.IsBFPThrottled(_targetAccount.AccountID, _connection.Address))
                    {
                        _firewall.NotifyLoginAttempt(_connection.Address, _targetAccount.AccountID, false);
                        return AuthenticationResponse.BFProtected;
                    }
                }

                if (_targetAccount.Blocked) return _targetAccount.BlockedLoginResponse;

                _srp = new SecureRemotePassword(_targetAccountName, new BigInteger(_targetAccount.Password), true);
                AuthenticationResponse response = AuthenticationResponse.Success;

                try { _srp.PublicEphemeralValueA = new BigInteger(credentials); }
                catch 
                { 
                    response = AuthenticationResponse.InvalidCredentials;

                    if (_firewall != null && !_targetAccount.HasAllPermissions(StandardAccountPermissions.NoBruteForceProtection))
                        _firewall.NotifyLoginAttempt(_connection.Address, _targetAccount.AccountID, false);
                }

                return response;
            }
            else return AuthenticationResponse.InvalidCredentials;
        }

        private AuthenticationResponse HandleClientProof(byte[] data, int length)
        {
            if (length != 20) return AuthenticationResponse.Unspecified;
            AuthenticationResponse response = AuthenticationResponse.Success;
            byte[] credentials = new byte[20];
            Buffer.BlockCopy(data, 0, credentials, 0, 20);

            try 
            {
                BigInteger proof = new BigInteger(credentials);
                if (!_srp.IsClientProofValid(proof)) response = AuthenticationResponse.InvalidCredentials;
            }
            catch  {  response = AuthenticationResponse.InvalidCredentials; }

            return response;
        }

        private AuthenticationResponse HandleAuthResult(byte[] data, int length)
        {
            if (length != 1) return AuthenticationResponse.Unspecified;

            if ((AuthenticationResponse)data[0] == AuthenticationResponse.Success)
            {
                if (_targetAccount.InUse)
                {
                    if (_targetAccount.Owner.Fingerprint == _targetFingerprint || _targetAccount.HasAllPermissions(StandardAccountPermissions.ReplaceContext))
                    {
                        _connection.Host.NotifyDeauthenticated(_targetAccount.Owner.Connection, this, AuthenticationResponse.InUse);
                        if (_targetAccount.InUse) throw new InvalidOperationException("Host failed to deauthenticate connection context.");
                    }
                    else if (_firewall != null && _targetAccount.HasAllPermissions(StandardAccountPermissions.ConcurrentLoginRestriction))
                    {
                        _firewall.NotifyAccountCompromised(_targetAccount, _connection.Address, AccountCompromisedReason.ConcurrentLogins);
                        return _targetAccount.BlockedLoginResponse;
                    }
                    else return AuthenticationResponse.InUse;
                }

                return AuthenticationResponse.Success;
            }
            else return AuthenticationResponse.Unspecified;
        }

        protected abstract NetworkAccount ResolveAccount(string account);
        #endregion
    }

    public abstract class OutboundAuthenticationBroker : AuthenticationBroker
    {
        public void BeginAuthentication(Connection connection)
        {
            _connection = connection;
            _connection.AuthBroker = this;
            _connection.Crypt = NullCryptProvider.Singleton;
            OnBeginAuthentication();
        }

        protected abstract void OnBeginAuthentication();
    }

    public abstract class OutboundSRPAuthenticationBroker : OutboundAuthenticationBroker
    {
        #region Static Members
        private static byte[] _outboundSeed = unchecked(new byte[] { (byte)((sbyte)-12), (byte)((sbyte)102), (byte)((sbyte)49), (byte)((sbyte)89), (byte)((sbyte)-4), (byte)((sbyte)-125), (byte)((sbyte)110), (byte)((sbyte)49), (byte)((sbyte)49), (byte)((sbyte)2), (byte)((sbyte)81), (byte)((sbyte)-43), (byte)((sbyte)68), (byte)((sbyte)49), (byte)((sbyte)103), (byte)((sbyte)-104) });
        private static byte[] _inboundSeed = unchecked(new byte[] { (byte)((sbyte)34), (byte)((sbyte)-66), (byte)((sbyte)-27), (byte)((sbyte)-49), (byte)((sbyte)-69), (byte)((sbyte)7), (byte)((sbyte)100), (byte)((sbyte)-39), (byte)((sbyte)0), (byte)((sbyte)69), (byte)((sbyte)27), (byte)((sbyte)-48), (byte)((sbyte)36), (byte)((sbyte)-72), (byte)((sbyte)-43), (byte)((sbyte)69) });
        #endregion

        #region Instance Fields
        private int _stage;
        private BigInteger _credentials;

        protected string _username;
        protected bool _remoteFirewall;

        protected int _localIdentifier;
        protected Version _localVersion;
        protected PlatformID _localPlatform;
        protected string _localServicePack;
        protected uint _localFingerprint;

        protected byte[] _ksrnSeed;
        protected SecureRemotePassword _srp;
        #endregion

        #region Constructor
        protected OutboundSRPAuthenticationBroker(string username, string password)
        {
            _stage = -1;
            _username = username;
            _credentials = new BigInteger(SecureRemotePassword.GenerateCredentialsHash(username, password));
            _srp = new SecureRemotePassword(_username, _credentials, false);
        }
        #endregion

        #region Reset Handling
        protected override void Reset()
        {
            _stage = -1;
            _ksrnSeed = null;
            _srp = new SecureRemotePassword(_username, _credentials, false);
        }
        #endregion

        #region Authentication Handling
        protected override void OnBeginAuthentication()
        {
            _stage = 0;

            ByteBuffer writer = new ByteBuffer(128);

            if (_localIdentifier != 0) writer.Write(_localIdentifier);
            if (_localVersion != null) writer.Write(_localVersion);

            writer.Write((byte)_localPlatform);
            writer.Write(_localServicePack);
            writer.Write(_localFingerprint);

            if (_remoteFirewall) writer.Write(_connection.ToString());
            writer.Write(_username);
            writer.Write(_srp.PublicEphemeralValueA.GetBytes(32));

            _connection.Send(writer.Data, 0, (int)writer.Length);
        }
        #endregion

        #region Data Handling
        protected override bool OnDataReceived(byte[] data, int length)
        {
            if (_stage == -1 || length <= 0) return false;

            switch (_stage)
            {
                case 0:
                    {
                        ByteBuffer reader = new ByteBuffer(data);
                        AuthenticationResponse response = (AuthenticationResponse)reader.ReadByte();

                        if (response == AuthenticationResponse.Success)
                        {
                            ++_stage;
                            _ksrnSeed = new byte[4];
                            for (int i = 0; i < _ksrnSeed.Length; i++) _ksrnSeed[i] = reader.ReadByte();
                            _srp.Salt = new BigInteger(reader.ReadBytes(reader.ReadByte()));
                            _srp.PublicEphemeralValueB = new BigInteger(reader.ReadBytes(32));
                            _connection.Send(_srp.ClientSessionKeyProof.GetBytes(20), 0, 20);
                        }
                        else
                        {
                            if (response == AuthenticationResponse.InvalidCredentials)
                            {
                                Reset();
                                _connection.Host.NotifyAuthenticationFailed(_connection, this, response, false);
                                return true;
                            }
                            else
                            {
                                _connection.Host.NotifyAuthenticationFailed(_connection, this, response, true);
                                return false;
                            }
                        }

                        break;
                    }
                case 1:
                    {
                        ByteBuffer reader = new ByteBuffer(data);
                        AuthenticationResponse response = (AuthenticationResponse)reader.ReadByte();

                        if (response == AuthenticationResponse.Success)
                        {
                            ++_stage;
                            try { response = _srp.IsServerProofValid(new BigInteger(reader.ReadBytes(20))) ? AuthenticationResponse.Success : AuthenticationResponse.InvalidCredentials; }
                            catch { response = AuthenticationResponse.InvalidCredentials; }
                            _connection.Send(new byte[] { (byte)response }, 0, 1);
                        }
                        else
                        {
                            if (response == AuthenticationResponse.InvalidCredentials)
                            {
                                Reset();
                                _connection.Host.NotifyAuthenticationFailed(_connection, this, response, false);
                                return true;
                            }
                            else
                            {
                                _connection.Host.NotifyAuthenticationFailed(_connection, this, response, true);
                                return false;
                            }
                        }

                        break;
                    }
                case 2:
                    {
                        ByteBuffer reader = new ByteBuffer(data);
                        AuthenticationResponse response = (AuthenticationResponse)reader.ReadByte();

                        if (response == AuthenticationResponse.Success)
                        {
                            OnAuthenticated(reader);
                            _cryptProvider = new ARCProvider((((_ksrnSeed[0] + _ksrnSeed[2]) / _ksrnSeed[1]) * _ksrnSeed[3]) + 50, _srp.SessionKey.GetBytes(40), _inboundSeed, _outboundSeed);
                            _connection.Host.NotifyAuthenticated(_connection, null, this);
                            return true;
                        }
                        else
                        {
                            if (response == AuthenticationResponse.InUse)
                            {
                                Reset();
                                _connection.Host.NotifyAuthenticationFailed(_connection, this, response, false);
                                return true;
                            }
                            else
                            {
                                _connection.Host.NotifyAuthenticationFailed(_connection, this, response, true);
                                return false;
                            }
                        }
                    }
            }

            return true;
        }
        #endregion
    }

    public enum AuthenticationResponse : byte
    {
        Unspecified = 0,
        InvalidVersion = 1,
        MachineBan = 2,
        InUse = 3,
        InvalidCredentials = 4,
        NetworkLockdown = 5,
        BFProtected = 6,
        GeneralBan = 7,
        Logout = 8,
        Success = 9,
    }
}
