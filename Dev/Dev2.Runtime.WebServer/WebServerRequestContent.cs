/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dev2.Runtime.WebServer
{
    internal class WebServerRequestContent : ICommunicationRequestContent
    {
        public WebServerRequestContent(HttpContent httpContent)
        {
            Content = httpContent;
            Headers = new WebServerRequestContentHeaders(httpContent.Headers);
        }

        public HttpContent Content { get; }

        public ICommunicationRequestContentHeaders Headers { get; }

        public bool IsMimeMultipartContent(string subtype)
        {
            return Content.IsMimeMultipartContent(subtype);
        }

        public async Task<Stream> ReadAsStreamAsync()
        {
            return await Content.ReadAsStreamAsync();
        }

    }
}