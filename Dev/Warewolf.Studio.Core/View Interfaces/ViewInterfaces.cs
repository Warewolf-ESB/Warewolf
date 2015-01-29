using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Core.View_Interfaces
{
    public interface IExplorerView : IView
    {
        IEnvironmentViewModel GetEnvironmentNode(string nodeName);
        List<IExplorerItemViewModel> GetFoldersVisible();
    }
    public interface IToolboxView : IView { }
    public interface IMenuView : IView { }
    public interface IVariableListView : IView { }
}
