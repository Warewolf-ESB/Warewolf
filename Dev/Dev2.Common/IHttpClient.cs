/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dev2.Common
{
    public class HttpClientWrapper : IHttpClient
    {
        readonly HttpClient _httpClient;

        public HttpClientWrapper(HttpClient client)
        {
            _httpClient = client;
        }

        public void Dispose() => _httpClient.Dispose();

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            return _httpClient.GetAsync(url);
        }
    }
}