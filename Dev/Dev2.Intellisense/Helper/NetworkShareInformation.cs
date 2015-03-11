
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace Dev2.Intellisense.Helper
{

    #region Share Type

    /// <summary>
    ///     Type of share
    /// </summary>
    [Flags]
    public enum ShareType
    {
        Disk,
        Device ,
// ReSharper disable InconsistentNaming
        IPC,
// ReSharper restore InconsistentNaming
        Special = -2147483648
    }

    #endregion

    #region Share

    /// <summary>
    ///     Information about a local share
    /// </summary>
    public class Share
    {
        #region Private data

        readonly string _networkName;
        readonly string _shareServer;
        private readonly ShareType _shareType;

        #endregion

        #region Constructor

       
        public Share(string server, string netName, ShareType shareType)
        {
            if(ShareType.Special == shareType && "IPC$" == netName)
            {
                shareType |= ShareType.IPC;
            }

            _shareServer = server;
            _networkName = netName;
            _shareType = shareType;
        }

        #endregion

        #region Properties

        public bool IsFileSystem
        {
            get
            {
                if(0 != (ShareType & ShareType.Device))
                {
                    return false;
                }

                if(0 != (ShareType & ShareType.IPC))
                {
                    return false;
                }

                if(0 == (ShareType & ShareType.Special))
                {
                    return true;
                }

                return ShareType.Special == ShareType && !string.IsNullOrEmpty(_networkName);
            }
        }

        public ShareType ShareType
        {
            get { return _shareType; }
        }

        #endregion

        public override string ToString()
        {
            return string.Format(@"\\{0}\{1}", string.IsNullOrEmpty(_shareServer) 
                    ? Environment.MachineName 
                    : _shareServer, _networkName);
        }
    }

    #endregion

    #region Share Collection

    /// <summary>
    ///     A collection of shares
    /// </summary>
    public class ShareCollection : ReadOnlyCollectionBase
    {
        #region Constants

        const int NoError = 0;
        const int ErrorAccessDenied = 5;

        #endregion

        /// <summary>The name of the server this collection represents</summary>
        readonly string _server;

        #region Constructor

        /// <summary>
        ///     Default constructor - local machine
        /// </summary>
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

        #endregion

        #region Interop

        #region Enumerate shares

        [ExcludeFromCodeCoverage]
// ReSharper disable InconsistentNaming
        static void EnumerateSharesNT(string server, ShareCollection shares)
// ReSharper restore InconsistentNaming
        {
            int level = 2;
            int hResume = 0;
            IntPtr pBuffer = IntPtr.Zero;

            try
            {
                int entriesRead;
                int totalEntries;
                int nRet = NetShareEnum(server, level, out pBuffer, -1,
                    out entriesRead, out totalEntries, ref hResume);

                if(ErrorAccessDenied == nRet)
                {
                    level = 1;
                    nRet = NetShareEnum(server, level, out pBuffer, -1,
                        out entriesRead, out totalEntries, ref hResume);
                }

                if(NoError == nRet && entriesRead > 0)
                {
                    Type t = (2 == level) ? typeof(ShareInfo2) : typeof(ShareInfo1);
                    int offset = Marshal.SizeOf(t);

                    for(int i = 0, lpItem = pBuffer.ToInt32(); i < entriesRead; i++, lpItem += offset)
                    {
                        IntPtr pItem = new IntPtr(lpItem);
                        if(1 == level)
                        {
                            ShareInfo1 si = (ShareInfo1)Marshal.PtrToStructure(pItem, t);
                            shares.Add(si.NetName, string.Empty, si.ShareType, si.Remark);
                        }
                        else
                        {
                            ShareInfo2 si = (ShareInfo2)Marshal.PtrToStructure(pItem, t);
                            shares.Add(si.NetName, si.Path, si.ShareType, si.Remark);
                        }
                    }
                }
            }
            finally
            {
                if(IntPtr.Zero != pBuffer)
                {
                    NetApiBufferFree(pBuffer);
                }
            }
        }

        protected static void EnumerateShares(string server, ShareCollection shares)
        {
            EnumerateSharesNT(server, shares);
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Enumerate shares (NT)</summary>
        [DllImport("netapi32", CharSet = CharSet.Unicode)]
        protected static extern int NetShareEnum(string lpServerName, int dwLevel,
                                                 out IntPtr lpBuffer, int dwPrefMaxLen, out int entriesRead,
                                                 out int totalEntries, ref int hResume);

        /// <summary>Free the buffer (NT)</summary>
        [DllImport("netapi32")]
        protected static extern int NetApiBufferFree(IntPtr lpBuffer);

        #endregion

        #region Add

        protected void Add(string netName, string path, ShareType shareType, string remark)
        {
            InnerList.Add(new Share(_server, netName, shareType));
        }

        #endregion

        #region Structures

        /// <summary>Share information, NT, level 1</summary>
        /// <remarks>
        ///     Fallback when no admin rights.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct ShareInfo1
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NetName;

            public ShareType ShareType;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Remark;
        }

        /// <summary>Share information, NT, level 2</summary>
        /// <remarks>
        ///     Requires admin rights to work.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct ShareInfo2
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NetName;

            public ShareType ShareType;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Remark;

            public int Permissions;
            public int MaxUsers;
            public int CurrentUsers;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Path;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Password;
        }

        #endregion
    }

    #endregion
}
