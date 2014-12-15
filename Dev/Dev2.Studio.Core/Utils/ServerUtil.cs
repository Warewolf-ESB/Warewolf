
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Utils
{
    public static class ServerUtil
    {
        public static IEnvironmentModel GetLocalhostServer()
        {
            var servers = ServerProvider.Instance.Load();
            var localHost = servers.FirstOrDefault(s => s.IsLocalHost);
            if(localHost != null && localHost.IsConnected)
                return localHost;
            return null;
        }
    }
}
