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
        /// The query-string parameter (e.g. "api_key") or header (e.g. "X-Api-Key") that carries
        /// <see cref="ApiKeyValue"/>. Used by <c>ApiKeyQuery</c> and <c>ApiKeyHeader</c>; unused by <c>Bearer</c>.
        /// </summary>
        string ApiKeyName { get; set; }
        /// <summary>
        /// The credential sent with every request by <c>ApiKeyQuery</c>, <c>ApiKeyHeader</c> and <c>Bearer</c>
        /// sources. For <c>Bearer</c> it is the bare token; "Bearer " is added when it is sent.
        /// </summary>
        string ApiKeyValue { get; set; }
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