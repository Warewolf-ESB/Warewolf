
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;
using System.Text;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    public class TestStaticFileResponseWriter : StaticFileResponseWriter
    {
        readonly string _fileContent;

        public TestStaticFileResponseWriter(string fileContent, string contentType)
            : base("layout.htm", contentType)
        {
            _fileContent = fileContent;
        }

        protected override Stream OpenFileStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_fileContent));
        }
    }
}
