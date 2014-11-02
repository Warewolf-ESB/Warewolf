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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Dev2.Runtime.WebServer.Responses
{
    public class DynamicFileResponseWriter : IResponseWriter
    {
        private static readonly MediaTypeHeaderValue ContentType = ContentTypes.Html;

        private readonly string _contentPath;
        private readonly string _contentPathToken;
        private readonly string _layoutFile;

        public DynamicFileResponseWriter(string layoutFile, string contentPathToken, string contentPath)
        {
            VerifyArgument.IsNotNull("layoutFile", layoutFile);
            VerifyArgument.IsNotNull("contentPathToken", contentPathToken);
            VerifyArgument.IsNotNull("contentPath", contentPath);
            _layoutFile = layoutFile;
            _contentPathToken = contentPathToken;
            _contentPath = contentPath;
        }

        public void Write(WebServerContext context)
        {
            string content = GetContent();
            context.ResponseMessage.Content = new StringContent(content);
            context.ResponseMessage.Content.Headers.ContentType = ContentType;
        }

        private string GetContent()
        {
            string layoutContent = ReadLayoutFile();
            var builder = new StringBuilder(layoutContent);
            string content = builder.Replace(_contentPathToken, _contentPath).ToString();
            return content;
        }

        protected virtual string ReadLayoutFile()
        {
            return File.ReadAllText(_layoutFile);
        }
    }
}