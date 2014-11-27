
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.TO
{
    public class WebBrowserNavigateRequestTO : IWebBrowserNavigateRequestTO
    {
        public WebBrowserNavigateRequestTO(object dataContext, string uri, string payload)
        {
            DataContext = dataContext;
            Uri = uri;
            Payload = payload;
        }

        public object DataContext { get; set; }
        public string Uri { get; set; }
        public string Payload { get; set; }
    }
}
