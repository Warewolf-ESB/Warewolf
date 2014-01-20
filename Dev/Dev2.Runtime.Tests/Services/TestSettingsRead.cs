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