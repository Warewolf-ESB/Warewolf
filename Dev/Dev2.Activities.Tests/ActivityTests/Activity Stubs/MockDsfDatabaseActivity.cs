
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
using Dev2.DataList.Contract;
using Dev2.Services.Execution;

namespace Dev2.Activities
{
    public class MockDsfDatabaseActivity : DsfDatabaseActivity
    {
        public MockDsfDatabaseActivity()
        {
        }

        public MockDsfDatabaseActivity(IServiceExecution exection)
        {
            ServiceExecution = exection;
        }

        public Guid MockExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors)
        {
            return base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors);
        }

        public void MockBeforeExecutionStart(IDSFDataObject dsfDataObject)
        {
            var tmpErrors = new ErrorResultTO();
            BeforeExecutionStart(dsfDataObject, tmpErrors);
        }

        public void MockAfterExecutionCompleted()
        {
            var tmpErrors = new ErrorResultTO();
            AfterExecutionCompleted(tmpErrors);
        }

    }
}
