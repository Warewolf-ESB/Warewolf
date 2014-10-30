
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    public interface INavigationViewModel:INotifyPropertyChanged
    {
        List<IEnvironmentModel> Environments { get; }
        IEnvironmentRepository EnvironmentRepository { get; }
        ObservableCollection<IExplorerItemModel> ExplorerItemModels { get; set; }
        IExplorerItemModel SelectedItem { get; set; }
        void Dispose();
        bool IsFromActivityDrop { get; set; }
        IEnvironmentModel FilterEnvironment { get; set; }
        void BringItemIntoView(IContextualResourceModel item);
        void Filter(Func<IExplorerItemModel, bool> filter, bool fromFilter = false, bool useDialogFilter=false);
    }
}
