/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.WebServices
{
    public interface IWebService:IEquatable<IWebService>
    {
        string Name { get; set; }
        string Path { get; set; }
        IWebServiceSource Source { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        IList<IServiceOutputMapping> OutputMappings { get; set; }
        string QueryString { get; set; }
        string RequestUrl { get; set; }
        Guid Id { get; set; }
        List<NameValue> Headers { get; set; }
        string PostData { get; set; }
        string SourceUrl { get; set; }
        string Response { get; set; }
        WebRequestMethod Method { get;set; }
    }
}
