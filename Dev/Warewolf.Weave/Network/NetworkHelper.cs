// Decompiled with JetBrains decompiler
// Type: System.Network.NetworkHelper
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace System.Network
{
  public static class NetworkHelper
  {
    private const byte PHFMask = 15;
    private const byte PHCMask = 240;
    public const int MinUsernameLength = 6;
    public const int MaxUsernameLength = 100;
    public const int MinPasswordLength = 3;
    public const int MaxPasswordLength = 16;
    internal static readonly byte[] EmptyBytes = new byte[0];
    internal static readonly Encoding Encoding = Encoding.ASCII;
    internal static readonly Socket[] EmptySockets = new Socket[0];

    public static byte[] BuildPacketHeader(byte channel, ushort id, PacketHeaderFlags flags)
    {
      byte[] numArray;
      if (id <= (ushort) byte.MaxValue)
      {
        numArray = new byte[2]{ (byte) 0, (byte) id };
      }
      else
      {
        numArray = new byte[3];
        flags = PacketHeaderFlags.Identifier16;
        TwoOctetUnion twoOctetUnion = new TwoOctetUnion(id);
        numArray[1] = twoOctetUnion.O1;
        numArray[2] = twoOctetUnion.O2;
      }
      numArray[0] = (byte) ((PacketHeaderFlags) ((int) channel & -241) | flags & (PacketHeaderFlags.Extended | PacketHeaderFlags.Identifier16 | PacketHeaderFlags.Length16 | PacketHeaderFlags.Length32));
      return numArray;
    }

    public static NetworkInterface[] GetEthernetNICS() => NetworkHelper.GetEthernetNICS(true);

    public static NetworkInterface[] GetEthernetNICS(bool includeLoopback)
    {
      List<NetworkInterface> networkInterfaceList = new List<NetworkInterface>();
      if (NetworkInterface.GetIsNetworkAvailable())
      {
        try
        {
          NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
          for (int index = 0; index < networkInterfaces.Length; ++index)
          {
            NetworkInterfaceType networkInterfaceType = networkInterfaces[index].NetworkInterfaceType;
            switch (networkInterfaceType)
            {
              case NetworkInterfaceType.Ethernet:
              case NetworkInterfaceType.GigabitEthernet:
                networkInterfaceList.Add(networkInterfaces[index]);
                break;
              default:
                if (includeLoopback && networkInterfaceType == NetworkInterfaceType.Loopback)
                {
                  networkInterfaceList.Add(networkInterfaces[index]);
                  break;
                }
                break;
            }
          }
        }
        catch
        {
        }
      }
      return networkInterfaceList.ToArray();
    }

    public static NetworkInterface GetNICFromID(string id)
    {
      Guid empty = Guid.Empty;
      Guid id1;
      try
      {
        id1 = new Guid(id);
      }
      catch
      {
        id1 = Guid.Empty;
      }
      return NetworkHelper.GetNICFromID(id1);
    }

    public static NetworkInterface GetNICFromID(Guid id)
    {
      if (NetworkInterface.GetIsNetworkAvailable())
      {
        try
        {
          NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
          for (int index = 0; index < networkInterfaces.Length; ++index)
          {
            if (new Guid(networkInterfaces[index].Id) == id)
              return networkInterfaces[index];
          }
        }
        catch
        {
        }
      }
      return (NetworkInterface) null;
    }

    public static IPAddress GetIPAddress(NetworkInterface nic, AddressFamily family)
    {
      if (nic == null)
        return IPAddress.None;
      UnicastIPAddressInformationCollection unicastAddresses = nic.GetIPProperties().UnicastAddresses;
      for (int index = 0; index < unicastAddresses.Count; ++index)
      {
        IPAddress address = unicastAddresses[index].Address;
        if (address.AddressFamily == family)
          return address;
      }
      return IPAddress.None;
    }

    public static bool IPMatchCIDR(string cidr, IPAddress ip)
    {
      if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        return false;
      byte[] bytes = new byte[4];
      string[] strArray = cidr.Split('.');
      bool flag = false;
      int cidrLength = 0;
      for (int index = 0; index < 4; ++index)
      {
        int num1 = 0;
        int num2 = 10;
        foreach (char ch in strArray[index])
        {
          if (ch == 'x' || ch == 'X')
            num2 = 16;
          else if (ch >= '0' && ch <= '9')
          {
            int num3 = (int) ch - 48;
            if (flag)
              cidrLength = cidrLength * num2 + num3;
            else
              num1 = num1 * num2 + num3;
          }
          else if (ch >= 'a' && ch <= 'f')
          {
            int num4 = 10 + ((int) ch - 97);
            if (flag)
              cidrLength = cidrLength * num2 + num4;
            else
              num1 = num1 * num2 + num4;
          }
          else if (ch >= 'A' && ch <= 'F')
          {
            int num5 = 10 + ((int) ch - 65);
            if (flag)
              cidrLength = cidrLength * num2 + num5;
            else
              num1 = num1 * num2 + num5;
          }
          else
          {
            if (ch != '/' || flag || index != 3)
              return false;
            num2 = 10;
            flag = true;
          }
        }
        bytes[index] = (byte) num1;
      }
      return NetworkHelper.IPMatchCIDR(NetworkHelper.OrderedAddressValue(bytes), ip, cidrLength);
    }

    public static bool IPMatchCIDR(IPAddress cidrPrefix, IPAddress ip, int cidrLength) => cidrPrefix != null && ip != null && cidrPrefix.AddressFamily != AddressFamily.InterNetworkV6 && NetworkHelper.IPMatchCIDR(NetworkHelper.SwapUnsignedInt((uint) NetworkHelper.GetLongAddressValue(cidrPrefix)), NetworkHelper.SwapUnsignedInt((uint) NetworkHelper.GetLongAddressValue(ip)), cidrLength);

    public static bool IPMatchCIDR(uint cidrPrefixValue, IPAddress ip, int cidrLength)
    {
      if (ip == null || ip.AddressFamily == AddressFamily.InterNetworkV6)
        return false;
      uint ipValue = NetworkHelper.SwapUnsignedInt((uint) NetworkHelper.GetLongAddressValue(ip));
      return NetworkHelper.IPMatchCIDR(cidrPrefixValue, ipValue, cidrLength);
    }

    public static bool IPMatchCIDR(uint cidrPrefixValue, uint ipValue, int cidrLength)
    {
      if (cidrLength <= 0 || cidrLength >= 32)
        return (int) cidrPrefixValue == (int) ipValue;
      uint num = (uint) (-1 << 32 - cidrLength);
      return ((int) cidrPrefixValue & (int) num) == ((int) ipValue & (int) num);
    }

    public static bool IPMatch(string val, IPAddress ip, ref bool valid)
    {
      valid = true;
      string[] strArray = val.Split('.');
      for (int index = 0; index < 4; ++index)
      {
        int num1;
        int num2;
        if (index >= strArray.Length)
        {
          num1 = 0;
          num2 = (int) byte.MaxValue;
        }
        else
        {
          string str = strArray[index];
          if (str == "*")
          {
            num1 = 0;
            num2 = (int) byte.MaxValue;
          }
          else
          {
            num1 = 0;
            num2 = 0;
            bool flag = false;
            int num3 = 10;
            int num4 = 10;
            foreach (char ch in str)
            {
              switch (ch)
              {
                case '-':
                  flag = true;
                  num2 = 0;
                  break;
                case '?':
                  if (!flag)
                    num1 *= num3;
                  num2 = num2 * num4 + (num4 - 1);
                  break;
                default:
                  if (ch != 'x' && ch != 'X')
                  {
                    if (ch >= '0' && ch <= '9')
                    {
                      int num5 = (int) ch - 48;
                      if (!flag)
                        num1 = num1 * num3 + num5;
                      num2 = num2 * num4 + num5;
                      break;
                    }
                    if (ch >= 'a' && ch <= 'f')
                    {
                      int num6 = 10 + ((int) ch - 97);
                      if (!flag)
                        num1 = num1 * num3 + num6;
                      num2 = num2 * num4 + num6;
                      break;
                    }
                    if (ch >= 'A' && ch <= 'F')
                    {
                      int num7 = 10 + ((int) ch - 65);
                      if (!flag)
                        num1 = num1 * num3 + num7;
                      num2 = num2 * num4 + num7;
                      break;
                    }
                    valid = false;
                    break;
                  }
                  num3 = 16;
                  num4 = 16;
                  break;
              }
            }
          }
        }
        int num8 = (int) (byte) (NetworkHelper.GetAddressValue(ip) >> index * 8);
        if (num8 < num1 || num8 > num2)
          return false;
      }
      return true;
    }

    public static bool IPMatchClassC(IPAddress ip1, IPAddress ip2) => (NetworkHelper.GetAddressValue(ip1) & 16777215) == (NetworkHelper.GetAddressValue(ip2) & 16777215);

    public static int GetAddressValue(IPAddress address) => (int) address.Address;

    public static long GetLongAddressValue(IPAddress address) => address.Address;

    private static uint OrderedAddressValue(byte[] bytes) => bytes.Length != 4 ? 0U : (uint) (((int) bytes[0] << 24 | (int) bytes[1] << 16 | (int) bytes[2] << 8 | (int) bytes[3]) & -1);

    private static uint SwapUnsignedInt(uint source) => (uint) (((int) source & (int) byte.MaxValue) << 24 | ((int) source & 65280) << 8) | (source & 16711680U) >> 8 | (source & 4278190080U) >> 24;

    public static bool IsValidUsername(string username)
    {
      if (string.IsNullOrEmpty(username))
        return false;
      int length = username.Length;
      return length >= 6 && length < 100;
    }

    public static bool IsValidPassword(string password)
    {
      if (string.IsNullOrEmpty(password))
        return false;
      int length = password.Length;
      return length >= 3 && length < 16;
    }

    public static void ReleaseSocket(ref Socket socket)
    {
      if (socket == null)
        return;
      try
      {
        socket.Shutdown(SocketShutdown.Receive);
      }
      catch (ObjectDisposedException ex)
      {
      }
      catch (SocketException ex)
      {
      }
      socket.Close();
      socket = (Socket) null;
    }
  }
}
