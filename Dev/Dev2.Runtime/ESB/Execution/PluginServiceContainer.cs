using System;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Execution;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    // BUG 9619 - 2013.06.05 - TWR - Refactored
    public class PluginServiceContainer : EsbExecutionContainer
    {
        readonly IServiceExecution _pluginServiceExecution;

        public PluginServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _pluginServiceExecution = new PluginServiceExecution(dataObj, true);
        }

        public PluginServiceContainer(IServiceExecution pluginServiceExecution)
            : base(pluginServiceExecution)
        {
            _pluginServiceExecution = pluginServiceExecution;
        }
        public override Guid Execute(out ErrorResultTO errors)
        {
            // set the output definition ;)
            _pluginServiceExecution.InstanceOutputDefintions = InstanceOutputDefinition;

            var result = _pluginServiceExecution.Execute(out errors);
            return result;
        }
    }
}
