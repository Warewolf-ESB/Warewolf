/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Common.Interfaces.NetStandard20;

namespace Warewolf.Common.NetStandard20
{

    public class WebRequestWrapper : IWebRequest
    {
        private WebRequest _request;
        public WebRequestWrapper(string escapeUriString)
        {
            _request = WebRequest.Create(escapeUriString);
        }
        public string Method
        {
            get => _request.Method;
            set => _request.Method = value;
        }
        public string ContentType
        {
            get => _request.ContentType;
            set => _request.ContentType = value;
        }
        public long ContentLength
        {
            get => _request.ContentLength;
            set => _request.ContentLength = value;
        }
        public bool UseDefaultCredentials
        {
            get => _request.UseDefaultCredentials;
            set => _request.UseDefaultCredentials = value;
        }
        public WebHeaderCollection Headers
        {
            get => _request.Headers;
            set => _request.Headers = value;
        }

        public ICredentials Credentials
        {
            get => _request.Credentials;
            set => _request.Credentials = value;
        }

        public Uri RequestUri => _request.RequestUri;

        public Stream GetRequestStream()
        {
            return _request.GetRequestStream();
        }

        public WebResponse GetResponse()
        {
            return _request.GetResponse();
        }

        public Task<WebResponse> GetResponseAsync()
        {
            return _request.GetResponseAsync();
        }
    }
}
