/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Common.Interfaces.NetStandard20;

namespace Dev2.Common.Interfaces
{
    public interface IWebSource : IResource
    {
        string Address { get; set; }
        string DefaultQuery { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        string Response { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here so that we only instantiate once!
        /// </summary>
        IWebClientWrapper Client { get; set; }

        void DisposeClient();
    }
}