
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Net;

namespace Dev2.Webs.Callbacks
{
    public interface INetworkHelper
    {
        bool HasConnection(string uri);
    }

    public class NetworkHelper : INetworkHelper
    {
        public bool HasConnection(string uri)
        {
            try
            {
                using(var client = new WebClient())
                using(client.OpenRead(uri))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
