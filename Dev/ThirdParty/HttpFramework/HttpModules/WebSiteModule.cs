
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Net;
using HttpFramework;
using HttpFramework.Sessions;

namespace HttpFramework.HttpModules
{
    /// <summary>
    /// The website module let's you handle multiple websites in the same server.
    /// It uses the "Host" header to check which site you want.
    /// </summary>
    /// <remarks>It's recommended that you do not
    /// add any other modules to HttpServer if you are using the website module. Instead,
    /// add all wanted modules to each website.</remarks>
    class WebSiteModule : HttpModule
    {
        private readonly string _host;
        readonly List<HttpModule> _modules = new List<HttpModule>();
        private readonly string _siteName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host">domain name that should be handled.</param>
        /// <param name="name"></param>
        public WebSiteModule(string host, string name)
        {
            _host = host;
            _siteName = name;
        }

        /// <summary>
        /// Name of site.
        /// </summary>
        public string SiteName
        {
            get { return _siteName; }
        }

        public bool CanHandle(Uri uri)
        {
            return string.Compare(uri.Host, _host, true) == 0;
        }

        /// <summary>
        /// Method that process the url
        /// </summary>
        /// <param name="request">Information sent by the browser about the request</param>
        /// <param name="response">Information that is being sent back to the client.</param>
        /// <param name="session">Session used to </param>
        public override bool Process(IHttpRequest request, IHttpResponse response, IHttpSession session)
        {
            if (!CanHandle(request.Uri))
                return false;

            bool handled = false;
            foreach (HttpModule module in _modules)
            {
                if (module.Process(request, response, session))
                    handled = true;
            }

            if (!handled)
                response.Status = HttpStatusCode.NotFound;

            return true;
        }
    }
}
