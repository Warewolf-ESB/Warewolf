using System;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

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
        protected EsbExecuteRequest Request { get; private set; }

        public String InstanceOutputDefinition { get; set; }

        protected EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : this(sa, dataObject, theWorkspace, esbChannel, null)
        {
        }

        protected EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
        {
            ServiceAction = sa;
            DataObject = dataObject;
            TheWorkspace = theWorkspace;
            EsbChannel = esbChannel;
            Request = request;
        }

        protected EsbExecutionContainer()
        {
        }

        public abstract Guid Execute(out ErrorResultTO errors);
    }
}
