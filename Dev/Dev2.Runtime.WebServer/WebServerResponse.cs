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
using System.Linq;
using System.Net.Http;

namespace Dev2.Runtime.WebServer
{
    public class WebServerResponse : ICommunicationResponse
    {
        public WebServerResponse(HttpResponseMessage response)
        {
            VerifyArgument.IsNotNull("response", response);
            IEnumerable<string> origins;
            if (response.RequestMessage != null && response.RequestMessage.Headers != null &&
                response.RequestMessage.Headers.TryGetValues("Origin", out origins))
            {
                string origin = origins.FirstOrDefault();
                if (!string.IsNullOrEmpty(origin))
                {
                    response.Headers.Add("Access-Control-Allow-Origin", origin);
                }
            }
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            Response = response;
        }

        public HttpResponseMessage Response { get; private set; }
    }
}