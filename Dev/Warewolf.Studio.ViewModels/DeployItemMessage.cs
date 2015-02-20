using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Warewolf.Studio.ViewModels
{
    // ReSharper disable once UnusedMember.Global
    public class DeployItemMessage : IDeployItemMessage 
    {
        readonly IExplorerItemViewModel _item;
        readonly IExplorerItemViewModel _sourceServer;

        public DeployItemMessage(IExplorerItemViewModel item, IExplorerItemViewModel sourceServer)
        {
            _item = item;
            _sourceServer = sourceServer;
        }

        #region Implementation of IDeployItemMessage

        public IExplorerItemViewModel Item
        {
            get
            {
                return _item;
            }
        }
        public IExplorerItemViewModel SourceServer
        {
            get
            {
                return _sourceServer;
            }
        }

        #endregion
    }
}