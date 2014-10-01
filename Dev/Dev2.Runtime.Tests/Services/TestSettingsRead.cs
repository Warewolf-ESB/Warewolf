
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
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;

namespace Dev2.Tests.Runtime.Services
{
    public class TestSettingsRead : SettingsRead
    {
        readonly Func<IEsbManagementEndpoint> _securityRead;

        public TestSettingsRead(Func<IEsbManagementEndpoint> securityRead)
        {
            VerifyArgument.IsNotNull("securityRead", securityRead);
            _securityRead = securityRead;
        }

        protected override IEsbManagementEndpoint CreateSecurityReadEndPoint()
        {
            return _securityRead();
        }

        public IEsbManagementEndpoint TestCreateSecurityReadEndPoint()
        {
            return base.CreateSecurityReadEndPoint();
        }
    }
}
