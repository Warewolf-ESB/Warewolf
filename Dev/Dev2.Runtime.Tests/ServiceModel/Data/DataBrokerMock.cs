using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    internal class DataBrokerMock : DataBroker
    {
        public override void GetStoredProcedures(Func<IDbCommand, IList<IDataParameter>, bool> resultProcessor, bool continueOnProcessorException = false)
        {
            //This code should never be run in the DataBrokerTests
            throw new NotImplementedException();
        }
    }
}
