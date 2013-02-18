using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    internal class DataBrokerMock : AbstractDatabaseBroker
    {
        protected override void GetStoredProcedures(IDbConnection connection, Func<IDbCommand, IList<IDataParameter>, string, bool> procedureProcessor, Func<IDbCommand, IList<IDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false)
        {
            
        }

        protected override DataSet ExecuteSelect(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        protected override IDbConnection CreateConnection(string connectionString)
        {
            Mock<IDbConnection> connection = new Mock<IDbConnection>();
            return connection.Object;
        }
    }
}
