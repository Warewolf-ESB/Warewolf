
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.DataList.Contract;
using Dev2.Services.Execution;
// ReSharper disable CheckNamespace
namespace Dev2.Activities
{
    public class MockDsfPluginActivity : DsfPluginActivity
    {

        public PluginServiceExecution MockGetNewPluginServiceExecution(IDSFDataObject context)
        {
            return GetNewPluginServiceExecution(context);
        }

        public void MockExecutePluginService(PluginServiceExecution container)
        {
            ExecutePluginService(container, 0);
        }
    }

    public class FaultyMockDsfPluginActivity : DsfPluginActivity
    {
        public void MockExecutionImpl(out ErrorResultTO tmpErrors)
        {
            
            tmpErrors = new ErrorResultTO();
            tmpErrors.AddError("Something bad happened");
        }
    }
}
