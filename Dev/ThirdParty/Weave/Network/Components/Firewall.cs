
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Net;

namespace System.Network
{
    public class Firewall : SerializableEntity
    {
        #region Instance Fields
        private HashSet<uint> _blockedHardware;
        private List<IFirewallEntry> _blockedAddresses;
        private bool _networkLockdown;
        private Dictionary<IPAddress, AddressMonitor> _activeMonitors;
        private BFPSetup _setup;
        private DateTime _nextMonitorUpdate;
        #endregion

        #region Public Properties
        public HashSet<uint> BlockedHardware { get { return _blockedHardware; } }
        public List<IFirewallEntry> BlockedAddresses { get { return _blockedAddresses; } }
        public bool NetworkLockdown { get { return _networkLockdown; } set { _networkLockdown = value; } }
        #endregion

        #region Constructors
        public Firewall(BFPSetup setup)
        {
            if (setup == null) _setup = new BFPSetup();
            else _setup = setup.Clone();
            _blockedHardware = new HashSet<uint>();
            _blockedAddresses = new List<IFirewallEntry>();
            _activeMonitors = new Dictionary<IPAddress, AddressMonitor>();
            _nextMonitorUpdate = DateTime.MinValue;
        }

        public Firewall()
            : this(new BFPSetup())
        {
        }
        #endregion

        #region [Serialization/Deserialization] Handling
        /// <summary>
        /// Initializes a new instance of the <see cref="Firewall"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public Firewall(IByteReaderBase reader)
            : base(reader)
        {
            _nextMonitorUpdate = DateTime.MinValue;
            _blockedAddresses = new List<IFirewallEntry>();
            _blockedHardware = new HashSet<uint>();
            _activeMonitors = new Dictionary<IPAddress, AddressMonitor>();
            _setup = new BFPSetup(reader);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++) _blockedAddresses.Add(ToFirewallEntry(reader.ReadString()));
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++) _blockedHardware.Add(reader.ReadUInt32());
        }

        /// <summary>
        /// Serializes the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void Serialize(IByteWriterBase writer)
        {
            _setup.Serialize(writer, false);
            writer.Write(_blockedAddresses.Count);
            for (int i = 0; i < _blockedAddresses.Count; i++) writer.Write(_blockedAddresses[i].ToString());
            writer.Write(_blockedHardware.Count);
            foreach (uint identity in _blockedHardware) writer.Write(identity);
        }
        #endregion

        #region [Add/Remove/Blocked] Handling
        /// <summary>
        /// Adds the address.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void AddAddress(IFirewallEntry entry)
        {
            if (!_blockedAddresses.Contains(entry))
                _blockedAddresses.Add(entry);
        }

        /// <summary>
        /// Adds the address.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        public void AddAddress(string pattern)
        {
            IFirewallEntry entry = ToFirewallEntry(pattern);

            if (!_blockedAddresses.Contains(entry))
                _blockedAddresses.Add(entry);
        }

        /// <summary>
        /// Adds the address.
        /// </summary>
        /// <param name="address">The address.</param>
        public void AddAddress(IPAddress address)
        {
            IFirewallEntry entry = new IPFirewallEntry(address);

            if (!_blockedAddresses.Contains(entry))
                _blockedAddresses.Add(entry);
        }

        public void AddHardware(uint identity)
        {
            if (identity == UInt32.MinValue) return;
            _blockedHardware.Add(identity);
        }

        public void RemoveAddress(object obj)
        {
            if (obj == null) return;
            IFirewallEntry entry = ToFirewallEntry(obj);
            if (entry != null) _blockedAddresses.Remove(entry);
        }

        public void RemoveHardware(uint identity)
        {
            if (identity == UInt32.MinValue) return;
            _blockedHardware.Remove(identity);
        }

        public bool IsBlocked(uint identity)
        {
            return _blockedHardware.Contains(identity);
        }

        public bool IsBlocked(IPAddress address)
        {
            for (int i = 0; i < _blockedAddresses.Count; i++)
                if (_blockedAddresses[i].IsBlocked(address))
                    return true;

            return false;
        }
        #endregion

        #region Brute Force Handling
        public bool IsBFPThrottled(Guid accountID, IPAddress address)
        {
            if (_nextMonitorUpdate < DateTime.Now)
                UpdateMonitors();

            AddressMonitor monitor = null;

            if (_activeMonitors.TryGetValue(address, out monitor))
            {
                if (monitor.Expired)
                {
                    _activeMonitors.Remove(address);
                    return false;
                }
                else if (monitor.Throttle) return true;

                return monitor.IsAccountThrottled(accountID);
            }

            return false;
        }

        private void UpdateMonitors()
        {
            _nextMonitorUpdate = DateTime.Now.AddMinutes(1);

            List<IPAddress> expiredMonitors = new List<IPAddress>();

            foreach (AddressMonitor activeMonitor in _activeMonitors.Values)
                if (activeMonitor.Expired)
                    expiredMonitors.Add(activeMonitor.Address);

            for (int i = 0; i < expiredMonitors.Count; i++)
                _activeMonitors.Remove(expiredMonitors[i]);
        }
        #endregion

        #region Notify(...) Handling
        public void NotifyLoginAttempt(IPAddress address, Guid accountID, bool validatedCredentials)
        {
            AddressMonitor monitor = null;

            if (_activeMonitors.TryGetValue(address, out monitor))
            {
                if (monitor.Expired)
                {
                    _activeMonitors.Remove(address);

                    if (!validatedCredentials)
                    {
                        monitor = new AddressMonitor(this, address, accountID);
                        _activeMonitors.Add(address, monitor);
                        return;
                    }
                }
                else monitor.NotifyLoginAttempt(this, accountID, validatedCredentials);
            }
            else if (!validatedCredentials)
                _activeMonitors.Add(address, new AddressMonitor(this, address, accountID));
        }

        public void NotifyAccountCompromised(NetworkAccount account, IPAddress address, AccountCompromisedReason reason)
        {
            
        }
        #endregion

        #region Conversion Handling
        protected virtual IFirewallEntry ToFirewallEntry(string entry)
        {
            IPAddress addr;

            if (IPAddress.TryParse(entry, out addr))
                return new IPFirewallEntry(addr);

            string[] str = entry.Split('/');

            if (str.Length == 2)
            {
                IPAddress cidrPrefix;

                if (IPAddress.TryParse(str[0], out cidrPrefix))
                {
                    int cidrLength;

                    if (int.TryParse(str[1], out cidrLength))
                        return new CIDRFirewallEntry(cidrPrefix, cidrLength);
                }
            }

            return new WildcardIPFirewallEntry(entry);
        }

        protected virtual IFirewallEntry ToFirewallEntry(object entry)
        {
            if (entry == null) return null;

            if (entry is string)
                return ToFirewallEntry((string)entry);
            else if (entry is IFirewallEntry)
                return (IFirewallEntry)entry;
            else if (entry is IPAddress)
                return new IPFirewallEntry((IPAddress)entry);

            return null;
        }
        #endregion

        #region Firewall Entries
        public sealed class IPFirewallEntry : IFirewallEntry
        {
            private IPAddress _address;

            public IPFirewallEntry(IPAddress address)
            {
                _address = address;
            }

            public bool IsBlocked(IPAddress address)
            {
                return _address.Equals(address);
            }

            public override string ToString()
            {
                return _address.ToString();
            }

            public override bool Equals(object obj)
            {
                if (obj is IPAddress)
                {
                    return obj.Equals(_address);
                }
                else if (obj is string)
                {
                    IPAddress otherAddress;

                    if (IPAddress.TryParse((string)obj, out otherAddress))
                        return otherAddress.Equals(_address);
                }
                else if (obj is IPFirewallEntry)
                {
                    return _address.Equals(((IPFirewallEntry)obj)._address);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return _address.GetHashCode();
            }
        }

        public sealed class CIDRFirewallEntry : IFirewallEntry
        {
            private IPAddress _cIDRPrefix;
            private int _cIDRLength;

            public CIDRFirewallEntry(IPAddress cidrPrefix, int cidrLength)
            {
                _cIDRPrefix = cidrPrefix;
                _cIDRLength = cidrLength;
            }

            public bool IsBlocked(IPAddress address)
            {
                return NetworkHelper.IPMatchCIDR(_cIDRPrefix, address, _cIDRLength);
            }

            public override string ToString()
            {
                return String.Format("{0}/{1}", _cIDRPrefix, _cIDRLength);
            }

            public override bool Equals(object obj)
            {
                if (obj is string)
                {
                    string entry = (string)obj;

                    string[] str = entry.Split('/');

                    if (str.Length == 2)
                    {
                        IPAddress cidrPrefix;

                        if (IPAddress.TryParse(str[0], out cidrPrefix))
                        {
                            int cidrLength;

                            if (int.TryParse(str[1], out cidrLength))
                                return _cIDRPrefix.Equals(cidrPrefix) && _cIDRLength.Equals(cidrLength);
                        }
                    }
                }
                else if (obj is CIDRFirewallEntry)
                {
                    CIDRFirewallEntry entry = obj as CIDRFirewallEntry;

                    return _cIDRPrefix.Equals(entry._cIDRPrefix) && _cIDRLength.Equals(entry._cIDRLength);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return _cIDRPrefix.GetHashCode() ^ _cIDRLength.GetHashCode();
            }
        }

        public sealed class WildcardIPFirewallEntry : IFirewallEntry
        {
            private string _entry;
            private bool _valid = true;

            public WildcardIPFirewallEntry(string entry)
            {
                _entry = entry;
            }

            public bool IsBlocked(IPAddress address)
            {
                if (!_valid) return false;
                return NetworkHelper.IPMatch(_entry, address, ref _valid);
            }

            public override string ToString()
            {
                return _entry.ToString();
            }

            public override bool Equals(object obj)
            {
                if (obj is string)
                    return obj.Equals(_entry);
                else if (obj is WildcardIPFirewallEntry)
                    _entry.Equals(((WildcardIPFirewallEntry)obj)._entry);

                return false;
            }

            public override int GetHashCode()
            {
                return _entry.GetHashCode();
            }
        }
        #endregion

        #region AddressMonitor
        private sealed class AddressMonitor
        {
            private IPAddress _address;
            private List<AccountMonitor> _accountMonitors;
            private DateTime _monitorExpiration;
            private bool _throttle;

            public IPAddress Address { get { return _address; } }
            public bool Expired { get { return GetExpired(); } }
            public bool Throttle { get { return _throttle; } }

            public AddressMonitor(Firewall source, IPAddress address, Guid accountID)
            {
                _address = address;
                _accountMonitors = new List<AccountMonitor>();
                _accountMonitors.Add(new AccountMonitor(source, accountID));
            }

            public bool IsAccountThrottled(Guid accountID)
            {
                for (int i = 0; i < _accountMonitors.Count; i++)
                    if (_accountMonitors[i].AccountID == accountID)
                    {
                        if (_accountMonitors[i].Expired)
                        {
                            _accountMonitors.RemoveAt(i);
                            return false;
                        }

                        return _accountMonitors[i].Throttle;
                    }

                return false;
            }

            public void NotifyLoginAttempt(Firewall source, Guid accountID, bool validated)
            {
                bool exists = false;

                for (int i = 0; i < _accountMonitors.Count; i++)
                    if (_accountMonitors[i].AccountID == accountID)
                    {
                        exists = true;

                        if (validated)
                            _accountMonitors.RemoveAt(i);
                        else _accountMonitors[i].NotifyFailedAttempt(source);

                        break;
                    }

                if (!exists && !validated) _accountMonitors.Add(new AccountMonitor(source, accountID));

                if (!_throttle)
                {
                    if (_accountMonitors.Count > source._setup.AddressActivationThreshold)
                    {
                        _throttle = true;
                        _monitorExpiration = DateTime.Now.Add(source._setup.AddressDuration);
                    }
                }
                else _monitorExpiration += source._setup.AddressPenalty;
            }

            private bool GetExpired()
            {
                if (_throttle) return (_monitorExpiration < DateTime.Now);

                for (int i = 0; i < _accountMonitors.Count; )
                    if (_accountMonitors[i].Expired)
                        _accountMonitors.RemoveAt(i);
                    else i++;

                return (_accountMonitors.Count == 0);
            }

            private sealed class AccountMonitor
            {
                private Guid _accountID;
                private DateTime _monitorExpiration;
                private int _failedAttempts;
                private bool _throttle;

                public Guid AccountID { get { return _accountID; } }
                public bool Expired { get { return (_monitorExpiration < DateTime.Now); } }
                public bool Throttle { get { return _throttle; } }

                public AccountMonitor(Firewall source, Guid accountID)
                {
                    _accountID = accountID;
                    _monitorExpiration = DateTime.Now.Add(source._setup.AccountMonitorDuration);
                    _failedAttempts = 1;
                    _throttle = false;
                }

                public void NotifyFailedAttempt(Firewall source)
                {
                    _monitorExpiration += source._setup.AccountPenalty;
                    _failedAttempts++;

                    if (!_throttle)
                    {
                        if (_failedAttempts > source._setup.AccountActivationThreshold)
                        {
                            _throttle = true;
                            _monitorExpiration = DateTime.Now.Add(source._setup.AccountDuration);
                        }
                    }
                    else _monitorExpiration += source._setup.AccountPenalty;
                }
            }
        }
        #endregion
    }

    public interface IFirewallEntry
    {
        bool IsBlocked(IPAddress address);
    }

    public sealed class BFPSetup : SerializableEntity, ICloneable
    {
        #region Instance Fields
        private uint _accountActivationThreshold;
        private uint _addressActivationThreshold;
        private TimeSpan _accountMonitorDuration;
        private TimeSpan _accountDuration;
        private TimeSpan _accountPenalty;
        private TimeSpan _addressMonitorDuration;
        private TimeSpan _addressDuration;
        private TimeSpan _addressPenalty;
        #endregion

        #region Public Properties
        public uint AccountActivationThreshold { get { return _accountActivationThreshold; } set { _accountActivationThreshold = value; } }
        public uint AddressActivationThreshold { get { return _addressActivationThreshold; } set { _addressActivationThreshold = value; } }
        public TimeSpan AccountMonitorDuration { get { return _accountMonitorDuration; } set { _accountMonitorDuration = value; } }
        public TimeSpan AccountDuration { get { return _accountDuration; } set { _accountDuration = value; } }
        public TimeSpan AccountPenalty { get { return _accountPenalty; } set { _accountPenalty = value; } }
        public TimeSpan AddressMonitorDuration { get { return _addressMonitorDuration; } set { _addressMonitorDuration = value; } }
        public TimeSpan AddressDuration { get { return _addressDuration; } set { _addressDuration = value; } }
        public TimeSpan AddressPenalty { get { return _addressPenalty; } set { _addressPenalty = value; } }
        #endregion

        #region Constructors
        private BFPSetup(BFPSetup original)
        {
            _accountActivationThreshold = original._accountActivationThreshold;
            _addressActivationThreshold = original._addressActivationThreshold;
            _accountMonitorDuration = original._accountMonitorDuration;
            _accountDuration = original._accountDuration;
            _accountPenalty = original._accountPenalty;
            _addressMonitorDuration = original._addressMonitorDuration;
            _addressDuration = original._addressDuration;
            _addressPenalty = original._addressPenalty;
        }

        public BFPSetup()
        {
            _accountActivationThreshold = 10;
            _addressActivationThreshold = 6;
            _accountMonitorDuration = TimeSpan.FromMinutes(5);
            _accountDuration = TimeSpan.FromMinutes(10);
            _accountPenalty = TimeSpan.FromMinutes(2);
            _addressMonitorDuration = TimeSpan.FromMinutes(5);
            _addressDuration = TimeSpan.FromMinutes(15);
            _addressPenalty = TimeSpan.FromMinutes(4);
        }
        #endregion

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public BFPSetup Clone()
        {
            return new BFPSetup(this);
        }
        #endregion

        #region [Serialization/Deserialization] Handling
        public BFPSetup(IByteReaderBase reader)
            : base(reader)
        {
            _accountActivationThreshold = reader.ReadUInt32();
            _addressActivationThreshold = reader.ReadUInt32();
            _accountMonitorDuration = reader.ReadTimeSpan();
            _accountDuration = reader.ReadTimeSpan();
            _accountPenalty = reader.ReadTimeSpan();
            _addressMonitorDuration = reader.ReadTimeSpan();
            _addressDuration = reader.ReadTimeSpan();
            _addressPenalty = reader.ReadTimeSpan();
        }

        protected override void Serialize(IByteWriterBase writer)
        {
            writer.Write(_accountActivationThreshold);
            writer.Write(_addressActivationThreshold);
            writer.Write(_accountMonitorDuration);
            writer.Write(_accountDuration);
            writer.Write(_accountPenalty);
            writer.Write(_addressMonitorDuration);
            writer.Write(_addressDuration);
            writer.Write(_addressPenalty);
        }
        #endregion
    }
}
