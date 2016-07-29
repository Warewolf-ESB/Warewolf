/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.InterfaceImplementors
{

    #region Share Type

    /// <summary>
    ///     Type of share
    /// </summary>
    [Flags]
    public enum ShareType
    {
        Disk,
        Device,
        Ipc,
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
        readonly ShareType _sharpType;

        #endregion

        #region Constructor


        public Share(string server, string netName, ShareType shareType)
        {
            if(ShareType.Special == shareType && "IPC$" == netName)
            {
                shareType |= ShareType.Ipc;
            }

            _shareServer = server;
            _networkName = netName;
            _sharpType = shareType;
        }

        #endregion

        #region Properties

        public bool IsFileSystem
        {
            get
            {
                if(0 != (_sharpType & ShareType.Device))
                {
                    return false;
                }

                if(0 != (_sharpType & ShareType.Ipc))
                {
                    return false;
                }

                if(0 == (_sharpType & ShareType.Special))
                {
                    return true;
                }

                return ShareType.Special == _sharpType && !string.IsNullOrEmpty(_networkName);
            }
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

    #endregion
}
