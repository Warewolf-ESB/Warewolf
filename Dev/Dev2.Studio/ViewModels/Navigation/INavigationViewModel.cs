using System;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2.Studio.ViewModels.Navigation
{
    public interface INavigationViewModel
    {
        List<IEnvironmentModel> Environments { get; }
        IEnvironmentRepository EnvironmentRepository { get; }
        ObservableCollection<ExplorerItemModel> ExplorerItemModels { get; set; }
        ExplorerItemModel SelectedItem { get; set; }
        void Dispose();
        bool IsFromActivityDrop { get; set; }
        void BringItemIntoView(IContextualResourceModel item);

        void Filter(Func<ExplorerItemModel, bool> filter,bool fromFilter=false);
    }
}