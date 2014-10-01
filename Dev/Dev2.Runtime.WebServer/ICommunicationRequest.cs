
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
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace Dev2.Runtime.WebServer
{
    public interface ICommunicationRequest
    {
        string Method { get; }
        Uri Uri { get; }
        IPrincipal User { get; set; }
        int ContentLength { get; }
        string ContentType { get; }
        Encoding ContentEncoding { get; }
        Stream InputStream { get; }

        NameValueCollection QueryString { get; }
        NameValueCollection BoundVariables { get; }
    }
}
