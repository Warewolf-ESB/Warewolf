#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net.Mail;

namespace Dev2.Common.Interfaces
{
    public interface IEmailSource
    {
        string Host { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        int Port { get; set; }
        bool EnableSsl { get; set; }
        int Timeout { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        string TestFromAddress { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        string TestToAddress { get; set; }
        string DataList { get; set; }
        Version Version { get; set; }
        bool ReloadActions { get; set; }
        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>   
        Guid ResourceID { get; set; }
        /// <summary>
        /// The display name of the resource.
        /// </summary>
        string ResourceName { get; set; }
        void Send(MailMessage mailMessage);
    }
}