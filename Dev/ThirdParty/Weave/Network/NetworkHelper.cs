
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
using System.Cryptography;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace System.Network
{
    public static class NetworkHelper
    {
        #region Constants
        private const byte PHFMask = 1 + 2 + 4 + 8;
        private const byte PHCMask = 16 + 32 + 64 + 128;

        public const int MinUsernameLength = 6;
        public const int MaxUsernameLength = 100;
        public const int MinPasswordLength = SecureRemotePassword.MinPassLength;
        public const int MaxPasswordLength = SecureRemotePassword.MaxPassLength;
        #endregion

        #region Readonly Fields
        internal static readonly byte[] EmptyBytes = new byte[0];
        internal static readonly Encoding Encoding = Encoding.ASCII;
        internal static readonly Socket[] EmptySockets = new Socket[0];
        #endregion

        #region Build(...) Handling
        public static byte[] BuildPacketHeader(byte channel, ushort id, PacketHeaderFlags flags)
        {
            byte[] header = null;

            if (id > Byte.MaxValue)
            {
                header = new byte[3];
                flags = PacketHeaderFlags.Identifier16;
                TwoOctetUnion tou = new TwoOctetUnion(id);
                header[1] = tou.O1;
                header[2] = tou.O2;
            }
            else
            {
                header = new byte[2];
                header[1] = (byte)id;
            }

            header[0] = (byte)((channel & ~PHCMask) | ((byte)flags & ~PHFMask));

            return header;
        }
        #endregion

        #region Network Interface Handling
        public static NetworkInterface[] GetEthernetNICS()
        {
            return GetEthernetNICS(true);
        }

        public static NetworkInterface[] GetEthernetNICS(bool includeLoopback)
        {
            List<NetworkInterface> toReturn = new List<NetworkInterface>();

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    NetworkInterface[] allNICS = NetworkInterface.GetAllNetworkInterfaces();

                    for (int i = 0; i < allNICS.Length; i++)
                    {
                        NetworkInterfaceType nicType = allNICS[i].NetworkInterfaceType;
                        if (nicType == NetworkInterfaceType.Ethernet || nicType == NetworkInterfaceType.GigabitEthernet)
                            toReturn.Add(allNICS[i]);
                        else if (includeLoopback && nicType == NetworkInterfaceType.Loopback)
                            toReturn.Add(allNICS[i]);
                    }
                }
                catch { }
            }

            return toReturn.ToArray();
        }

        public static NetworkInterface GetNICFromID(string id)
        {
            Guid guid = Guid.Empty;
            try { guid = new Guid(id); }
            catch { guid = Guid.Empty; }
            return GetNICFromID(guid);
        }

        public static NetworkInterface GetNICFromID(Guid id)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    NetworkInterface[] allNICS = NetworkInterface.GetAllNetworkInterfaces();

                    for (int i = 0; i < allNICS.Length; i++)
                    {
                        Guid currentId = new Guid(allNICS[i].Id);
                        if (currentId == id) return allNICS[i];
                    }
                }
                catch { }
            }

            return null;
        }

        public static IPAddress GetIPAddress(NetworkInterface nic, AddressFamily family)
        {
            if (nic == null) return IPAddress.None;
            UnicastIPAddressInformationCollection addresses = nic.GetIPProperties().UnicastAddresses;

            for (int i = 0; i < addresses.Count; i++)
            {
                IPAddress current = addresses[i].Address;
                if (current.AddressFamily == family)
                    return current;
            }

            return IPAddress.None;
        }
        #endregion

        #region IPMatch(...) Handling
        public static bool IPMatchCIDR(string cidr, IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                return false;

            byte[] bytes = new byte[4];
            string[] split = cidr.Split('.');
            bool cidrBits = false;
            int cidrLength = 0;

            for (int i = 0; i < 4; i++)
            {
                int part = 0;

                int partBase = 10;

                string pattern = split[i];

                for (int j = 0; j < pattern.Length; j++)
                {
                    char c = (char)pattern[j];


                    if (c == 'x' || c == 'X')
                    {
                        partBase = 16;
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        int offset = c - '0';

                        if (cidrBits)
                        {
                            cidrLength *= partBase;
                            cidrLength += offset;
                        }
                        else
                        {
                            part *= partBase;
                            part += offset;
                        }
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        int offset = 10 + (c - 'a');

                        if (cidrBits)
                        {
                            cidrLength *= partBase;
                            cidrLength += offset;
                        }
                        else
                        {
                            part *= partBase;
                            part += offset;
                        }
                    }
                    else if (c >= 'A' && c <= 'F')
                    {
                        int offset = 10 + (c - 'A');

                        if (cidrBits)
                        {
                            cidrLength *= partBase;
                            cidrLength += offset;
                        }
                        else
                        {
                            part *= partBase;
                            part += offset;
                        }
                    }
                    else if (c == '/')
                    {
                        if (cidrBits || i != 3)
                        {
                            return false;
                        }

                        partBase = 10;
                        cidrBits = true;
                    }
                    else
                    {
                        return false;
                    }
                }

                bytes[i] = (byte)part;
            }

            uint cidrPrefix = OrderedAddressValue(bytes);

            return IPMatchCIDR(cidrPrefix, ip, cidrLength);
        }

        public static bool IPMatchCIDR(IPAddress cidrPrefix, IPAddress ip, int cidrLength)
        {
            if (cidrPrefix == null || ip == null || cidrPrefix.AddressFamily == AddressFamily.InterNetworkV6)	//Ignore IPv6 for now
                return false;

            uint cidrValue = SwapUnsignedInt((uint)GetLongAddressValue(cidrPrefix));
            uint ipValue = SwapUnsignedInt((uint)GetLongAddressValue(ip));

            return IPMatchCIDR(cidrValue, ipValue, cidrLength);
        }

        public static bool IPMatchCIDR(uint cidrPrefixValue, IPAddress ip, int cidrLength)
        {
            if (ip == null || ip.AddressFamily == AddressFamily.InterNetworkV6)
                return false;

            uint ipValue = SwapUnsignedInt((uint)GetLongAddressValue(ip));

            return IPMatchCIDR(cidrPrefixValue, ipValue, cidrLength);
        }

        public static bool IPMatchCIDR(uint cidrPrefixValue, uint ipValue, int cidrLength)
        {
            if (cidrLength <= 0 || cidrLength >= 32)
                return cidrPrefixValue == ipValue;

            uint mask = uint.MaxValue << 32 - cidrLength;

            return ((cidrPrefixValue & mask) == (ipValue & mask));
        }

        public static bool IPMatch(string val, IPAddress ip, ref bool valid)
        {
            valid = true;

            string[] split = val.Split('.');

            for (int i = 0; i < 4; ++i)
            {
                int lowPart, highPart;

                if (i >= split.Length)
                {
                    lowPart = 0;
                    highPart = 255;
                }
                else
                {
                    string pattern = split[i];

                    if (pattern == "*")
                    {
                        lowPart = 0;
                        highPart = 255;
                    }
                    else
                    {
                        lowPart = 0;
                        highPart = 0;

                        bool highOnly = false;
                        int lowBase = 10;
                        int highBase = 10;

                        for (int j = 0; j < pattern.Length; ++j)
                        {
                            char c = (char)pattern[j];

                            if (c == '?')
                            {
                                if (!highOnly)
                                {
                                    lowPart *= lowBase;
                                    lowPart += 0;
                                }

                                highPart *= highBase;
                                highPart += highBase - 1;
                            }
                            else if (c == '-')
                            {
                                highOnly = true;
                                highPart = 0;
                            }
                            else if (c == 'x' || c == 'X')
                            {
                                lowBase = 16;
                                highBase = 16;
                            }
                            else if (c >= '0' && c <= '9')
                            {
                                int offset = c - '0';

                                if (!highOnly)
                                {
                                    lowPart *= lowBase;
                                    lowPart += offset;
                                }

                                highPart *= highBase;
                                highPart += offset;
                            }
                            else if (c >= 'a' && c <= 'f')
                            {
                                int offset = 10 + (c - 'a');

                                if (!highOnly)
                                {
                                    lowPart *= lowBase;
                                    lowPart += offset;
                                }

                                highPart *= highBase;
                                highPart += offset;
                            }
                            else if (c >= 'A' && c <= 'F')
                            {
                                int offset = 10 + (c - 'A');

                                if (!highOnly)
                                {
                                    lowPart *= lowBase;
                                    lowPart += offset;
                                }

                                highPart *= highBase;
                                highPart += offset;
                            }
                            else
                            {
                                valid = false;	//high & lowpart would be 0 if it got to here.
                            }
                        }
                    }
                }

                int b = (byte)(GetAddressValue(ip) >> (i * 8));

                if (b < lowPart || b > highPart)
                    return false;
            }

            return true;
        }

        public static bool IPMatchClassC(IPAddress ip1, IPAddress ip2)
        {
            return ((GetAddressValue(ip1) & 0xFFFFFF) == (GetAddressValue(ip2) & 0xFFFFFF));
        }

        public static int GetAddressValue(IPAddress address)
        {
#pragma warning disable 618
            return (int)address.Address;
#pragma warning restore 618
        }

        public static long GetLongAddressValue(IPAddress address)
        {
#pragma warning disable 618
            return address.Address;
#pragma warning restore 618
        }

        private static uint OrderedAddressValue(byte[] bytes)
        {
            if (bytes.Length != 4)
                return 0;

            return (uint)((((bytes[0] << 0x18) | (bytes[1] << 0x10)) | (bytes[2] << 8)) | bytes[3]) & ((uint)0xffffffff);
        }

        private static uint SwapUnsignedInt(uint source)
        {
            return (uint)((((source & 0x000000FF) << 0x18)
            | ((source & 0x0000FF00) << 8)
            | ((source & 0x00FF0000) >> 8)
            | ((source & 0xFF000000) >> 0x18)));
        }
        #endregion

        #region Validation Handling
        public static bool IsValidUsername(string username)
        {
            if (String.IsNullOrEmpty(username)) return false;
            int length = username.Length;
            return length >= MinUsernameLength && length < MaxUsernameLength;
        }

        public static bool IsValidPassword(string password)
        {
            if (String.IsNullOrEmpty(password)) return false;
            int length = password.Length;
            return length >= MinPasswordLength && length < MaxPasswordLength;
        }
        #endregion

        #region Socket Handling
        public static void ReleaseSocket(ref Socket socket)
        {
            if (socket == null) return;

            try { socket.Shutdown(SocketShutdown.Receive); }
            catch (ObjectDisposedException) { }
            catch (SocketException) { }

            socket.Close();
            socket = null;
        }
        #endregion
    }
}
