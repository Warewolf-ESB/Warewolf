using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.Services
{
    public class ConnectionsMock : Connections
    {
        public int CanConnectToTcpClientHitCount { get; set; }

        protected override ValidationResult CanConnectToTcpClient(Connection connection)
        {
            CanConnectToTcpClientHitCount++;
            return new ValidationResult { IsValid = true };
        }
    }
}
