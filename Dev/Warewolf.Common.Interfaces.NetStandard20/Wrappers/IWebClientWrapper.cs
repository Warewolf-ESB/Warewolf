
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
using System.Collections.Specialized;
using System.Net;

namespace Warewolf.Common.Interfaces.NetStandard20
{
    public interface IWebClientWrapper : IDisposable
    {
        WebHeaderCollection Headers { get; }
        ICredentials Credentials { get; set; }

        byte[] UploadValues(string wareWolfResumeUrl, string method, NameValueCollection nameValueCollection);
        byte[] DownloadData(string address);
        byte[] UploadData(string address, string method, byte[] data);
    }
}
