// Decompiled with JetBrains decompiler
// Type: System.Network.Firewall
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace System.Network
{
  public class Firewall : SerializableEntity
  {
    private HashSet<uint> _blockedHardware;
    private List<IFirewallEntry> _blockedAddresses;
    private bool _networkLockdown;
    private Dictionary<IPAddress, Firewall.AddressMonitor> _activeMonitors;
    private BFPSetup _setup;
    private DateTime _nextMonitorUpdate;

    public HashSet<uint> BlockedHardware => this._blockedHardware;

    public List<IFirewallEntry> BlockedAddresses => this._blockedAddresses;

    public bool NetworkLockdown
    {
      get => this._networkLockdown;
      set => this._networkLockdown = value;
    }

    public Firewall(BFPSetup setup)
    {
      this._setup = setup != null ? setup.Clone() : new BFPSetup();
      this._blockedHardware = new HashSet<uint>();
      this._blockedAddresses = new List<IFirewallEntry>();
      this._activeMonitors = new Dictionary<IPAddress, Firewall.AddressMonitor>();
      this._nextMonitorUpdate = DateTime.MinValue;
    }

    public Firewall()
      : this(new BFPSetup())
    {
    }

    public Firewall(IByteReaderBase reader)
      : base(reader)
    {
      this._nextMonitorUpdate = DateTime.MinValue;
      this._blockedAddresses = new List<IFirewallEntry>();
      this._blockedHardware = new HashSet<uint>();
      this._activeMonitors = new Dictionary<IPAddress, Firewall.AddressMonitor>();
      this._setup = new BFPSetup(reader);
      int num1 = reader.ReadInt32();
      for (int index = 0; index < num1; ++index)
        this._blockedAddresses.Add(this.ToFirewallEntry(reader.ReadString()));
      int num2 = reader.ReadInt32();
      for (int index = 0; index < num2; ++index)
        this._blockedHardware.Add(reader.ReadUInt32());
    }

    protected override void Serialize(IByteWriterBase writer)
    {
      this._setup.Serialize(writer, false);
      writer.Write(this._blockedAddresses.Count);
      for (int index = 0; index < this._blockedAddresses.Count; ++index)
        writer.Write(this._blockedAddresses[index].ToString());
      writer.Write(this._blockedHardware.Count);
      foreach (uint num in this._blockedHardware)
        writer.Write(num);
    }

    public void AddAddress(IFirewallEntry entry)
    {
      if (this._blockedAddresses.Contains(entry))
        return;
      this._blockedAddresses.Add(entry);
    }

    public void AddAddress(string pattern)
    {
      IFirewallEntry firewallEntry = this.ToFirewallEntry(pattern);
      if (this._blockedAddresses.Contains(firewallEntry))
        return;
      this._blockedAddresses.Add(firewallEntry);
    }

    public void AddAddress(IPAddress address)
    {
      IFirewallEntry firewallEntry = (IFirewallEntry) new Firewall.IPFirewallEntry(address);
      if (this._blockedAddresses.Contains(firewallEntry))
        return;
      this._blockedAddresses.Add(firewallEntry);
    }

    public void AddHardware(uint identity)
    {
      if (identity == 0U)
        return;
      this._blockedHardware.Add(identity);
    }

    public void RemoveAddress(object obj)
    {
      if (obj == null)
        return;
      IFirewallEntry firewallEntry = this.ToFirewallEntry(obj);
      if (firewallEntry == null)
        return;
      this._blockedAddresses.Remove(firewallEntry);
    }

    public void RemoveHardware(uint identity)
    {
      if (identity == 0U)
        return;
      this._blockedHardware.Remove(identity);
    }

    public bool IsBlocked(uint identity) => this._blockedHardware.Contains(identity);

    public bool IsBlocked(IPAddress address)
    {
      for (int index = 0; index < this._blockedAddresses.Count; ++index)
      {
        if (this._blockedAddresses[index].IsBlocked(address))
          return true;
      }
      return false;
    }

    public bool IsBFPThrottled(Guid accountID, IPAddress address)
    {
      if (this._nextMonitorUpdate < DateTime.Now)
        this.UpdateMonitors();
      Firewall.AddressMonitor addressMonitor = (Firewall.AddressMonitor) null;
      if (!this._activeMonitors.TryGetValue(address, out addressMonitor))
        return false;
      if (addressMonitor.Expired)
      {
        this._activeMonitors.Remove(address);
        return false;
      }
      return addressMonitor.Throttle || addressMonitor.IsAccountThrottled(accountID);
    }

    private void UpdateMonitors()
    {
      this._nextMonitorUpdate = DateTime.Now.AddMinutes(1.0);
      List<IPAddress> ipAddressList = new List<IPAddress>();
      foreach (Firewall.AddressMonitor addressMonitor in this._activeMonitors.Values)
      {
        if (addressMonitor.Expired)
          ipAddressList.Add(addressMonitor.Address);
      }
      for (int index = 0; index < ipAddressList.Count; ++index)
        this._activeMonitors.Remove(ipAddressList[index]);
    }

    public void NotifyLoginAttempt(IPAddress address, Guid accountID, bool validatedCredentials)
    {
      Firewall.AddressMonitor addressMonitor1 = (Firewall.AddressMonitor) null;
      if (this._activeMonitors.TryGetValue(address, out addressMonitor1))
      {
        if (addressMonitor1.Expired)
        {
          this._activeMonitors.Remove(address);
          if (validatedCredentials)
            return;
          Firewall.AddressMonitor addressMonitor2 = new Firewall.AddressMonitor(this, address, accountID);
          this._activeMonitors.Add(address, addressMonitor2);
        }
        else
          addressMonitor1.NotifyLoginAttempt(this, accountID, validatedCredentials);
      }
      else
      {
        if (validatedCredentials)
          return;
        this._activeMonitors.Add(address, new Firewall.AddressMonitor(this, address, accountID));
      }
    }

    public void NotifyAccountCompromised(
      NetworkAccount account,
      IPAddress address,
      AccountCompromisedReason reason)
    {
    }

    protected virtual IFirewallEntry ToFirewallEntry(string entry)
    {
      IPAddress address1;
      if (IPAddress.TryParse(entry, out address1))
        return (IFirewallEntry) new Firewall.IPFirewallEntry(address1);
      string[] strArray = entry.Split('/');
      IPAddress address2;
      int result;
      return strArray.Length == 2 && IPAddress.TryParse(strArray[0], out address2) && int.TryParse(strArray[1], out result) ? (IFirewallEntry) new Firewall.CIDRFirewallEntry(address2, result) : (IFirewallEntry) new Firewall.WildcardIPFirewallEntry(entry);
    }

    protected virtual IFirewallEntry ToFirewallEntry(object entry)
    {
      switch (entry)
      {
        case null:
          return (IFirewallEntry) null;
        case string _:
          return this.ToFirewallEntry((string) entry);
        case IFirewallEntry _:
          return (IFirewallEntry) entry;
        case IPAddress _:
          return (IFirewallEntry) new Firewall.IPFirewallEntry((IPAddress) entry);
        default:
          return (IFirewallEntry) null;
      }
    }

    public sealed class IPFirewallEntry : IFirewallEntry
    {
      private IPAddress _address;

      public IPFirewallEntry(IPAddress address) => this._address = address;

      public bool IsBlocked(IPAddress address) => this._address.Equals((object) address);

      public override string ToString() => this._address.ToString();

      public override bool Equals(object obj)
      {
        switch (obj)
        {
          case IPAddress _:
            return obj.Equals((object) this._address);
          case string _:
            IPAddress address;
            if (IPAddress.TryParse((string) obj, out address))
              return address.Equals((object) this._address);
            break;
          case Firewall.IPFirewallEntry _:
            return this._address.Equals((object) ((Firewall.IPFirewallEntry) obj)._address);
        }
        return false;
      }

      public override int GetHashCode() => this._address.GetHashCode();
    }

    public sealed class CIDRFirewallEntry : IFirewallEntry
    {
      private IPAddress _cIDRPrefix;
      private int _cIDRLength;

      public CIDRFirewallEntry(IPAddress cidrPrefix, int cidrLength)
      {
        this._cIDRPrefix = cidrPrefix;
        this._cIDRLength = cidrLength;
      }

      public bool IsBlocked(IPAddress address) => NetworkHelper.IPMatchCIDR(this._cIDRPrefix, address, this._cIDRLength);

			public override string ToString()
			{
				return $"{this._cIDRPrefix}/{this._cIDRLength}";
			}

      public override bool Equals(object obj)
      {
        switch (obj)
        {
          case string _:
            string[] strArray = ((string) obj).Split('/');
            IPAddress address;
            int result;
            if (strArray.Length == 2 && IPAddress.TryParse(strArray[0], out address) && int.TryParse(strArray[1], out result) && this._cIDRPrefix.Equals((object) address))
              return this._cIDRLength.Equals(result);
            break;
          case Firewall.CIDRFirewallEntry _:
            Firewall.CIDRFirewallEntry cidrFirewallEntry = obj as Firewall.CIDRFirewallEntry;
            return this._cIDRPrefix.Equals((object) cidrFirewallEntry._cIDRPrefix) && this._cIDRLength.Equals(cidrFirewallEntry._cIDRLength);
        }
        return false;
      }

      public override int GetHashCode() => this._cIDRPrefix.GetHashCode() ^ this._cIDRLength.GetHashCode();
    }

    public sealed class WildcardIPFirewallEntry : IFirewallEntry
    {
      private string _entry;
      private bool _valid = true;

      public WildcardIPFirewallEntry(string entry) => this._entry = entry;

      public bool IsBlocked(IPAddress address) => this._valid && NetworkHelper.IPMatch(this._entry, address, ref this._valid);

      public override string ToString() => this._entry.ToString();

      public override bool Equals(object obj)
      {
        switch (obj)
        {
          case string _:
            return obj.Equals((object) this._entry);
          case Firewall.WildcardIPFirewallEntry _:
            this._entry.Equals(((Firewall.WildcardIPFirewallEntry) obj)._entry);
            break;
        }
        return false;
      }

      public override int GetHashCode() => this._entry.GetHashCode();
    }

    private sealed class AddressMonitor
    {
      private IPAddress _address;
      private List<Firewall.AddressMonitor.AccountMonitor> _accountMonitors;
      private DateTime _monitorExpiration;
      private bool _throttle;

      public IPAddress Address => this._address;

      public bool Expired => this.GetExpired();

      public bool Throttle => this._throttle;

      public AddressMonitor(Firewall source, IPAddress address, Guid accountID)
      {
        this._address = address;
        this._accountMonitors = new List<Firewall.AddressMonitor.AccountMonitor>();
        this._accountMonitors.Add(new Firewall.AddressMonitor.AccountMonitor(source, accountID));
      }

      public bool IsAccountThrottled(Guid accountID)
      {
        for (int index = 0; index < this._accountMonitors.Count; ++index)
        {
          if (this._accountMonitors[index].AccountID == accountID)
          {
            if (!this._accountMonitors[index].Expired)
              return this._accountMonitors[index].Throttle;
            this._accountMonitors.RemoveAt(index);
            return false;
          }
        }
        return false;
      }

      public void NotifyLoginAttempt(Firewall source, Guid accountID, bool validated)
      {
        bool flag = false;
        for (int index = 0; index < this._accountMonitors.Count; ++index)
        {
          if (this._accountMonitors[index].AccountID == accountID)
          {
            flag = true;
            if (validated)
            {
              this._accountMonitors.RemoveAt(index);
              break;
            }
            this._accountMonitors[index].NotifyFailedAttempt(source);
            break;
          }
        }
        if (!flag && !validated)
          this._accountMonitors.Add(new Firewall.AddressMonitor.AccountMonitor(source, accountID));
        if (!this._throttle)
        {
          if ((long) this._accountMonitors.Count <= (long) source._setup.AddressActivationThreshold)
            return;
          this._throttle = true;
          this._monitorExpiration = DateTime.Now.Add(source._setup.AddressDuration);
        }
        else
          this._monitorExpiration += source._setup.AddressPenalty;
      }

      private bool GetExpired()
      {
        if (this._throttle)
          return this._monitorExpiration < DateTime.Now;
        int index = 0;
        while (index < this._accountMonitors.Count)
        {
          if (this._accountMonitors[index].Expired)
            this._accountMonitors.RemoveAt(index);
          else
            ++index;
        }
        return this._accountMonitors.Count == 0;
      }

      private sealed class AccountMonitor
      {
        private Guid _accountID;
        private DateTime _monitorExpiration;
        private int _failedAttempts;
        private bool _throttle;

        public Guid AccountID => this._accountID;

        public bool Expired => this._monitorExpiration < DateTime.Now;

        public bool Throttle => this._throttle;

        public AccountMonitor(Firewall source, Guid accountID)
        {
          this._accountID = accountID;
          this._monitorExpiration = DateTime.Now.Add(source._setup.AccountMonitorDuration);
          this._failedAttempts = 1;
          this._throttle = false;
        }

        public void NotifyFailedAttempt(Firewall source)
        {
          this._monitorExpiration += source._setup.AccountPenalty;
          ++this._failedAttempts;
          if (!this._throttle)
          {
            if ((long) this._failedAttempts <= (long) source._setup.AccountActivationThreshold)
              return;
            this._throttle = true;
            this._monitorExpiration = DateTime.Now.Add(source._setup.AccountDuration);
          }
          else
            this._monitorExpiration += source._setup.AccountPenalty;
        }
      }
    }
  }
}
