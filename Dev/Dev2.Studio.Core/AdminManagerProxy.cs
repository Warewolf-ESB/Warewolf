#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Controller;
using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core
{
    public class AdminManagerProxy : ProxyBase,IAdminManager
    {
        #region Implementation of IAdminManager


        public AdminManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection) : base(communicationControllerFactory, connection) { }

        /// <summary>
        /// Gets the Warewolf Server version
        /// </summary>
        /// <returns>The version of the Server. Default version text of "Not Available." is returned
        /// if the server is older than that version.</returns>
        public string GetServerVersion()
        {
            var controller = CommunicationControllerFactory.CreateController("GetServerVersion");
            var version = controller.ExecuteCommand<string>(Connection, Guid.Empty, 10000);
            return string.IsNullOrEmpty(version) ? Warewolf.Studio.Resources.Languages.Core.ServerVersionUnavailable : version;
        }

        #endregion

        public string GetMinSupportedServerVersion()
        {
            var controller = CommunicationControllerFactory.CreateController("GetMinSupportedVersion");
            var version = controller.ExecuteCommand<string>(Connection, Guid.Empty);
            return version;
        }

        public Dictionary<string,string> GetServerInformation()
        {
            var controller = CommunicationControllerFactory.CreateController("GetServerInformation");
            var information = controller.ExecuteCommand<Dictionary<string, string>>(Connection, Guid.Empty);
            return information;
        }
    }
}
