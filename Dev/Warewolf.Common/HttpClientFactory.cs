/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Common.Interfaces;
using System;
using System.Net.Http;

namespace Warewolf.Common
{
    public interface IHttpClientFactory
    {
        IHttpClient New(Uri uri);
        IHttpClient New(string url);
    }

    public class HttpClientFactory : IHttpClientFactory
    {
        public IHttpClient New(Uri uri)
        {
            var baseAddress = uri.GetLeftPart(UriPartial.Authority);
            var client = new HttpClient { BaseAddress = new Uri(baseAddress) };

            return new HttpClientWrapper(client);
        }

        public IHttpClient New(string url)
        {
            return New(new Uri(url));
        }
    }
}
