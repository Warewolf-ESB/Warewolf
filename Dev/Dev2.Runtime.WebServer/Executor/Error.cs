/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Util;
using Newtonsoft.Json;
using Warewolf.Data.Serializers;

namespace Dev2.Runtime.WebServer.Executor
{
    public class Error
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public string ToJSON(Formatting indent = Formatting.Indented)
        {
            return JsonConvert.SerializeObject(new { Error = this }, indent);
        }

        public string ToXML(bool scrub = true)
        {
            var xml = this.SerializeToXml();
            return scrub ? Scrubber.Scrub(xml, ScrubType.Xml) : xml;
        }
    }
}