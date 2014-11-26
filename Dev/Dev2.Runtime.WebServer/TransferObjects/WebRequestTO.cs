
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Runtime.WebServer.TransferObjects
{
    public class WebRequestTO
    {
        public string ServiceName { get; set; }
        public string InstanceID { get; set; }
        public string Bookmark { get; set; }
        public string WebServerUrl { get; set; }
        public string Dev2WebServer { get; set; }
        public string RawRequestPayload { get; set; }
    }
}
