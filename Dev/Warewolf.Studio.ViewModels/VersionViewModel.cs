using System;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class VersionViewModel : ExplorerItemViewModel
    {
        public VersionViewModel(IServer server, IExplorerTreeItem parent, Action<IExplorerItemViewModel> selectAction, IShellViewModel shellViewModel, IPopupController popupController)
            : base(server, parent, selectAction, shellViewModel, popupController)
        {
        }        
    }
}