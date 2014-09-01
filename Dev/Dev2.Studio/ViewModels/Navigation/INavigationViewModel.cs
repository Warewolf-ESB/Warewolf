using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    public interface INavigationViewModel
    {
        List<IEnvironmentModel> Environments { get; }
        IEnvironmentRepository EnvironmentRepository { get; }
        ObservableCollection<IExplorerItemModel> ExplorerItemModels { get; set; }
        IExplorerItemModel SelectedItem { get; set; }
        void Dispose();
        bool IsFromActivityDrop { get; set; }
        IEnvironmentModel FilterEnvironment { get; set; }
        void BringItemIntoView(IContextualResourceModel item);
        void Filter(Func<IExplorerItemModel, bool> filter, bool fromFilter = false);
    }
}