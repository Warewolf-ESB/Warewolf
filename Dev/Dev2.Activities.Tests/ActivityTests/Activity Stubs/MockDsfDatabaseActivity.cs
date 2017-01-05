/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Services.Execution;
// ReSharper disable CheckNamespace
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

        public void MockExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors)
        {
            ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors,0);
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
