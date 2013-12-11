using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class ConnectionsMock : Connections
    {
        public int CanConnectToWebClientHitCount { get; set; }

        protected override ValidationResult CanConnectToWebClient(Connection connection)
        {
            CanConnectToWebClientHitCount++;
            return new ValidationResult { IsValid = true };
        }
    }
}
