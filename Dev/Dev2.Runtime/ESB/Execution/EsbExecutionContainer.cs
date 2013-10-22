using System;
using Dev2.DataList.Contract;
using Dev2.Services.Execution;
using Dev2.Workspaces;
using Dev2.DynamicServices;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Wrapper class for all executable types in our ESB
    /// </summary>
    public abstract class EsbExecutionContainer
    {

        protected ServiceAction ServiceAction { get; private set; }
        protected IDSFDataObject DataObject { get; private set; }
        protected IWorkspace TheWorkspace { get; private set; }
        protected IEsbChannel EsbChannel { get; private set; }

        public String InstanceOutputDefinition { get; set; }

        public EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel)
        {
            ServiceAction = sa;
            DataObject = dataObject;
            TheWorkspace = theWorkspace;
            EsbChannel = esbChannel;
        }

        protected EsbExecutionContainer(IServiceExecution serviceExecution)
        {
        }

        public abstract Guid Execute(out ErrorResultTO errors);
    }
}
