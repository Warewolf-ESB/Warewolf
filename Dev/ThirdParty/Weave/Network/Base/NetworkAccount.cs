
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
using System.Net;

namespace System.Network
{
    public class NetworkAccount : SerializableEntity
    {
        #region Instance Fields
        protected string _username;
        protected byte[] _password;
        protected Guid _accountID;
        protected NetworkContext _owner;
        protected DateTime _blockedTill;
        protected AuthenticationResponse _blockedLoginResponse;
        protected DateTime _createdAt;
        protected DateTime _lastLogin;
        protected DateTime _previousLogin;
        protected TimeSpan _totalTimeLoggedIn;
        protected StandardAccountPermissions _permissions;
        protected List<IPAddress> _loginIPs;
        #endregion

        #region Public Properties
        public string Username { get { return _username; } internal set { _username = value; } }
        public byte[] Password { get { return _password; } set { SetPassword(value); } }
        public Guid AccountID { get { return _accountID; } }
        public bool Blocked { get { return GetBlocked(); } }
        public AuthenticationResponse BlockedLoginResponse { get { return _blockedLoginResponse; } }
        public bool InUse { get { return GetInUse(); } }
        public NetworkContext Owner { get { return _owner; } }
        public DateTime CreatedAt { get { return _createdAt; } }
        public DateTime PreviousLoginTime { get { return _previousLogin; } }
        public DateTime CurrentLoginTime { get { return GetInUse() ? _lastLogin : DateTime.MinValue; } }
        public TimeSpan TotalTimeLoggedIn { get { return _totalTimeLoggedIn; } }
        public List<IPAddress> LoginAddresses { get { return _loginIPs; } }
        #endregion

        #region Constructors
        protected NetworkAccount()
        {
        }

        public NetworkAccount(IByteReaderBase reader)
            : base(reader)
        {
            _username = reader.ReadString();
            _password = reader.ReadBytes(reader.ReadInt32());
            _accountID = reader.ReadGuid();
            _blockedTill = DateTime.FromOADate(reader.ReadDouble());
            _blockedLoginResponse = (AuthenticationResponse)reader.ReadByte();
            _createdAt = DateTime.FromOADate(reader.ReadDouble());
            _lastLogin = DateTime.FromOADate(reader.ReadDouble());
            _totalTimeLoggedIn = TimeSpan.FromTicks(reader.ReadInt64());
            _previousLogin = _lastLogin;
            // Travis.Frisinger
            // Removed to allow this library to work with Warewolf's product suite ;)
            //_permissions = (StandardAccountPermissions)reader.ReadByte();
            var ignore = (StandardAccountPermissions)reader.ReadByte(); // chuck it out
          
            int count = reader.ReadInt32();
            _loginIPs = new List<IPAddress>();
            for (int i = 0; i < count; i++)
            {
                IPAddress address;
                string sAddress = reader.ReadString().Trim();
                if (IPAddress.TryParse(sAddress, out address))
                    _loginIPs.Add(address);
            }

            _owner = null;
        }

        public NetworkAccount(string username, byte[] password)
        {
            _username = username;
            Password = password;
            _accountID = Guid.NewGuid();
            _blockedTill = DateTime.MinValue;
            _blockedLoginResponse = AuthenticationResponse.Success;
            _owner = null;
            _createdAt = DateTime.Now;
            _lastLogin = DateTime.MinValue;
            _totalTimeLoggedIn = TimeSpan.Zero;
            _loginIPs = new List<IPAddress>();
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return _username;
        }
        #endregion

        #region [Get/Set] Handling
        protected virtual bool GetBlocked()
        {
            if (_blockedTill < DateTime.Now)
                _blockedTill = DateTime.MinValue;
            return (_blockedTill != DateTime.MinValue);
        }

        protected virtual bool GetInUse()
        {
            if (_owner != null)
            {
                if (!_owner.Attached || _owner.Connection == null || !_owner.Connection.Alive)
                {
                    Unbind();
                    return false;
                }

                return false; // Travis.Frisinger - Fix for Multi-Login
                //return true;
            }

            return false;
        }

        protected virtual void SetPassword(byte[] value)
        {
            _password = value;
        }
        #endregion

        #region Blocking Handling
        public void BlockTill(DateTime blockEnd, AuthenticationResponse blockedLoginResponse)
        {
            _blockedTill = blockEnd;
            _blockedLoginResponse = blockedLoginResponse;
            NotifyBlocked();
        }

        public void BlockFor(TimeSpan blockDuration, AuthenticationResponse blockedLoginResponse)
        {
            _blockedTill = DateTime.Now.Add(blockDuration);
            _blockedLoginResponse = blockedLoginResponse;
            NotifyBlocked();
        }

        public void BlockIndefinatly(AuthenticationResponse blockedLoginResponse)
        {
            _blockedTill = DateTime.MaxValue;
            _blockedLoginResponse = blockedLoginResponse;
            NotifyBlocked();
        }

        public void Unblock()
        {
            _blockedTill = DateTime.MinValue;
            _blockedLoginResponse = AuthenticationResponse.Success;
        }

        protected virtual void NotifyBlocked()
        {
            if (InUse && _owner.Connection != null)
                _owner.Connection.Dispose();
        }
        #endregion

        #region Binding Handling
        public virtual void BindTo(NetworkContext client, IPAddress address)
        {
            _owner = client;
            _previousLogin = _lastLogin;
            _lastLogin = DateTime.Now;

            bool contains = false;
            for (int i = 0; i < _loginIPs.Count; i++)
                if (_loginIPs[i].Equals(address))
                {
                    contains = true;
                    break;
                }

            if (!contains) _loginIPs.Add(address);
        }

        public virtual void Unbind()
        {
            _totalTimeLoggedIn += DateTime.Now - _lastLogin;
            _owner = null;
        }
        #endregion

        #region Permission Handling
        public bool HasAllPermissions(StandardAccountPermissions permissions)
        {
            return ((_permissions & permissions) == permissions);
        }

        public bool HasAnyPermissions(StandardAccountPermissions permissions)
        {
            return ((_permissions & permissions) != StandardAccountPermissions.None);
        }

        public void SetPermissions(StandardAccountPermissions permissions, bool hasPermissions)
        {
            if (hasPermissions)
                _permissions |= permissions;
            else
                _permissions &= ~permissions;
        }
        #endregion

        #region Validation Handling
        public virtual bool CheckCredentials(IPAddress source, string username, byte[] password)
        {
            return ValidateCredentials(username, password);
        }

        public bool AccessedBy(IPAddress address)
        {
            return _loginIPs.Contains(address);
        }

        protected virtual bool ValidateCredentials(string username, byte[] password)
        {
            if (!_username.Equals(username, StringComparison.OrdinalIgnoreCase)) return false;
            if (!WeaveUtility.Equals(_password, password)) return false;
            return true;
        }
        #endregion

        #region Serialization Handling
        protected override void Serialize(IByteWriterBase writer)
        {
            writer.Write((string)_username);
            writer.Write(_password.Length);
            writer.Write(_password);
            writer.Write(_accountID);
            writer.Write((double)_blockedTill.ToOADate());
            writer.Write((byte)_blockedLoginResponse);
            writer.Write((double)_createdAt.ToOADate());
            writer.Write((double)_lastLogin.ToOADate());
            writer.Write((long)_totalTimeLoggedIn.Ticks);
            writer.Write((byte)_permissions);
            writer.Write((int)_loginIPs.Count);
            for (int i = 0; i < _loginIPs.Count; i++) writer.Write((string)_loginIPs[i].ToString());
        }
        #endregion
    }

    public enum StandardAccountPermissions : byte
    {
        None = 0,
        IgnoreNetworkLockdown = 1,
        NoBruteForceProtection = 2,
        ReplaceContext = 4,
        ConcurrentLoginRestriction = 8
    }

    public enum AccountCompromisedReason
    {
        Unknown = 0,
        ConcurrentLogins = 1
    }
}
