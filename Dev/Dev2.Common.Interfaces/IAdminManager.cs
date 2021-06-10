/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System.Collections.Generic;
using Warewolf.Licensing;

namespace Dev2.Common.Interfaces
{
    /// <summary>
    /// an admin manager is responsible for all operations that manages the state of a warewolf server
    /// </summary>
    public interface IAdminManager
    {

        /// <summary>
        /// Gets the Warewolf Server version
        /// </summary>
        /// <returns>The version of the Server. Default version text of "less than 0.4.19.1" is returned
        /// if the server is older than that version.</returns>
        string GetServerVersion();

        Dictionary<string,string> GetServerInformation();

        string GetMinSupportedServerVersion();
        ISubscriptionData GetSubscriptionData();
    }
}
