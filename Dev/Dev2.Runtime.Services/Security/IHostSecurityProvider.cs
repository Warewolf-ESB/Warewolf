/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System;
using System.Net;
using System.Text;

namespace Dev2.Runtime.Security
{
    public interface IHostSecurityProvider
    {
        Guid ServerID { get; }
        string ConfigKey { get; }
        string ConfigSitename { get; }
        string CustomerId { get; }
        string PlanId { get; }
        string SubscriptionId { get; }
        bool VerifyXml(StringBuilder xml);

        StringBuilder SignXml(StringBuilder xml);

        bool EnsureSsl(IFile fileWrapper, string certPath, IPEndPoint endPoint);
    }
}