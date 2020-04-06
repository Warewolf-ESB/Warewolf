using Dev2.Common;
using Dev2.Runtime.Network;
using Dev2.Workspaces;
using Warewolf.Esb;
using Dev2.Diagnostics.Debug;

namespace Dev2.Runtime.ESB.Management
{
    public class InternalExecutionContext : IInternalExecutionContext
    {
        private IWorkspace _workspace;
        private readonly IEsbHub _esbHub;

        public InternalExecutionContext(IEsbHub esbHub)
        {
            _esbHub = esbHub;
        }
        private InternalExecutionContext(IEsbRequest request, IWorkspace workspace, IEsbHub esbHub)
            :this(esbHub)
        {
            _workspace = workspace;
            Request = request;
            _esbHub = esbHub;
        }

        public void RegisterAsClusterEventListener()
        {
            ClusterDispatcher.Instance.AddListener(_workspace.ID, _esbHub);
        }

        public void RegisterAsDebugEventLister()
        {
            DebugDispatcher.Instance.AddListener(GlobalConstants.ServerWorkspaceID, _esbHub);
        }

        public IInternalExecutionContext CloneForRequest(IEsbRequest request)
        {
            return new InternalExecutionContext(request, _workspace, _esbHub);
        }

        public IEsbRequest Request { get; private set; }

        public IWorkspaceBase Workspace
        {
            get => _workspace;
            set => _workspace = value as IWorkspace;
        }
    }
}