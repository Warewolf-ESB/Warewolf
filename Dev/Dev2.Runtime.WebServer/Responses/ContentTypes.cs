
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Net.Http.Headers;

namespace Dev2.Runtime.WebServer.Responses
{
    public static class ContentTypes
    {
        public static readonly MediaTypeHeaderValue Html = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
        public static readonly MediaTypeHeaderValue Xml = MediaTypeHeaderValue.Parse("text/xml");
        public static readonly MediaTypeHeaderValue Plain = MediaTypeHeaderValue.Parse("text/plain");
        public static readonly MediaTypeHeaderValue Json = MediaTypeHeaderValue.Parse("application/json");

        public static readonly MediaTypeHeaderValue ForceDownload = MediaTypeHeaderValue.Parse("application/force-download; charset=utf-8");
    }
}
