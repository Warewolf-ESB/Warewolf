using System;
using System.Collections.Generic;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class ConnectionsMock : Connections
    {
        public ConnectionsMock(Func<string> connectToServerResult)
            : this(() => new List<string> { "RSAKLFSVRGENDEV", "RSAKLFSVRTFSBLD" }, connectToServerResult)
        {
        }

        public ConnectionsMock(Func<List<string>> fetchComputersFn, Func<string> connectToServerResult)
            : base(fetchComputersFn)
        {
            ConnectToServerResult = connectToServerResult;
        }

        public Func<string> ConnectToServerResult { get; private set; }
        protected override string ConnectToServer(Connection connection)
        {
            return ConnectToServerResult == null ? null : ConnectToServerResult();
        }
    }
}
