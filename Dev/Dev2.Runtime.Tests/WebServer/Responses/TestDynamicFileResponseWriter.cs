
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    public class TestDynamicFileResponseWriter : DynamicFileResponseWriter
    {
        readonly string _layoutFileContent;

        public TestDynamicFileResponseWriter(string layoutFileContent, string contentPathToken, string contentPath)
            : base("layout.htm", contentPathToken, contentPath)
        {
            _layoutFileContent = layoutFileContent;
        }

        protected override string ReadLayoutFile()
        {
            return _layoutFileContent;
        }
    }
}
