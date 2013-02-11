using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    internal class DataBrokerMock : DataBroker
    {
        public override void GetStoredProcedures(IDbConnection connection, Func<IDbCommand, IList<IDataParameter>, string, bool> procedureProcessor, Func<IDbCommand, IList<IDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false)
        {
            throw new NotImplementedException();
        }

        public override DataSet ExecuteSelect(IDbCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
