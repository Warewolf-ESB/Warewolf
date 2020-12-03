/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Warewolf.Common.Interfaces.NetStandard20
{
    public interface IWebRequest
    {
        string Method { get; set; }
        string ContentType { get; set; }
        long ContentLength { get; set; }
        bool UseDefaultCredentials { get; set; }
        WebHeaderCollection Headers { get; set; }
        ICredentials Credentials { get; set; }
        Uri RequestUri { get; }
        Stream GetRequestStream();
        WebResponse GetResponse();
        Task<WebResponse> GetResponseAsync();
    }
}
