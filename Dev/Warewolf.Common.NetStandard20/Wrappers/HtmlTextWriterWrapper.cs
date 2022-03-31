/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;
using System.Web.UI;
using Warewolf.Common.Interfaces.NetStandard20.Wrappers;

namespace Warewolf.Common.NetStandard20.Wrappers
{
    public class HtmlTextWriterWrapper : IHtmlTextWriterWrapper
    {
        private readonly HtmlTextWriter _htmlTextWriter;

        public HtmlTextWriterWrapper(TextWriter textWriter)
        {
            _htmlTextWriter = new HtmlTextWriter(textWriter);
        }

        public void AddAttribute(string key, string value)
        {
            _htmlTextWriter.AddAttribute(key, value);
        }

        public void AddStyleAttribute(string key, string value)
        {
            _htmlTextWriter.AddStyleAttribute(key, value);
        }

        public void RenderBeginTag(string tagKey)
        {
            _htmlTextWriter.RenderBeginTag(tagKey);
        }

        public void RenderEndTag()
        {
            _htmlTextWriter.RenderEndTag();
        }

        public void Write(string text)
        {
            _htmlTextWriter.Write(text);
        }

        public void Dispose()
        {
            _htmlTextWriter.Dispose();
        }
    }
}
