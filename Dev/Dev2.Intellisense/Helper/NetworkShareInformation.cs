/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Dev2.Intellisense.Helper
{
    [Flags]
    public enum ShareType
    {
        None,
        Device,
        IPC,
        Special = -2147483648
    }

    public class Share
    {
        readonly string _networkName;
        readonly string _shareServer;
        readonly ShareType _shareType;

        public Share(string server, string netName, ShareType shareType)
        {
            if (ShareType.Special == shareType && "IPC$" == netName)
            {
                shareType |= ShareType.IPC;
            }

            _shareServer = server;
            _networkName = netName;
            _shareType = shareType;
        }

        public bool IsFileSystem
        {
            get
            {
                if (0 != (ShareType & ShareType.Device))
                {
                    return false;
                }

                if (0 != (ShareType & ShareType.IPC))
                {
                    return false;
                }

                if (0 == (ShareType & ShareType.Special))
                {
                    return true;
                }

                return ShareType.Special == ShareType && !string.IsNullOrEmpty(_networkName);
            }
        }

        public ShareType ShareType => _shareType;

        public override string ToString() => $@"\\{(string.IsNullOrEmpty(_shareServer) ? Environment.MachineName : _shareServer)}\{_networkName}";
    }

    public class ShareCollection : ReadOnlyCollectionBase
    {
        const int NoError = 0;
        const int ErrorAccessDenied = 5;
        readonly string _server;
        
        internal ShareCollection()
        {
            _server = string.Empty;
            EnumerateShares(_server, this);
        }

        internal ShareCollection(string server)
        {
            _server = server;
            EnumerateShares(_server, this);
        }

        public ShareCollection(IEnumerable<Share> shares)
        {
            InnerList.AddRange(shares.ToArray());
        }

        static void TryEnumerateSharesNT(string server, ShareCollection shares)
        {
            var level = 2;
            var hResume = 0;
            var pBuffer = IntPtr.Zero;

            try
            {
                pBuffer = EnumerateSharesNT(server, shares, ref level, ref hResume);
            }
            finally
            {
                if (IntPtr.Zero != pBuffer)
                {
                    NetApiBufferFree(pBuffer);
                }
            }
        }

        private static IntPtr EnumerateSharesNT(string server, ShareCollection shares, ref int level, ref int hResume)
        {
            var nRet = NetShareEnum(server, level, out IntPtr pBuffer, -1, out int entriesRead, out int totalEntries, ref hResume);

            if (ErrorAccessDenied == nRet)
            {
                level = 1;
                nRet = NetShareEnum(server, level, out pBuffer, -1, out entriesRead, out totalEntries, ref hResume);
            }

            if (NoError == nRet && entriesRead > 0)
            {
                var t = 2 == level ? typeof(ShareInfo2) : typeof(ShareInfo1);
                var offset = Marshal.SizeOf(t);

                for (int i = 0, lpItem = pBuffer.ToInt32(); i < entriesRead; i++, lpItem += offset)
                {
                    var pItem = new IntPtr(lpItem);
                    if (1 == level)
                    {
                        var si = (ShareInfo1)Marshal.PtrToStructure(pItem, t);
                        shares.Add(si.NetName, si.ShareType);
                    }
                    else
                    {
                        var si = (ShareInfo2)Marshal.PtrToStructure(pItem, t);
                        shares.Add(si.NetName, si.ShareType);
                    }
                }
            }

            return pBuffer;
        }

        protected static void EnumerateShares(string server, ShareCollection shares) => TryEnumerateSharesNT(server, shares);

        [DllImport("netapi32", CharSet = CharSet.Unicode)]
        protected static extern int NetShareEnum(string lpServerName, int dwLevel,
                                                 out IntPtr lpBuffer, int dwPrefMaxLen, out int entriesRead,
                                                 out int totalEntries, ref int hResume);

        [DllImport("netapi32")]
        protected static extern int NetApiBufferFree(IntPtr lpBuffer);

        protected void Add(string netName, ShareType shareType) => InnerList.Add(new Share(_server, netName, shareType));

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct ShareInfo1
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public readonly string NetName;

            public readonly ShareType ShareType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ShareInfo2
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public readonly string NetName;
            public readonly ShareType ShareType;

            [MarshalAs(UnmanagedType.LPWStr)]
            public readonly string Remark;
            public int Permissions;
            public int MaxUsers;
            public int CurrentUsers;

            [MarshalAs(UnmanagedType.LPWStr)]
            public readonly string Path;

            [MarshalAs(UnmanagedType.LPWStr)]
            public readonly string Password;
        }
    }
}