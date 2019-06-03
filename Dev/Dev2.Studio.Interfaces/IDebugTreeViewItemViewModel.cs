/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Dev2.Studio.Interfaces
{
    public interface IDebugTreeViewItemViewModel : INotifyPropertyChanged
    {
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        int Depth { get; }
        ObservableCollection<IDebugTreeViewItemViewModel> Children { get; }
        IDebugTreeViewItemViewModel Parent { get; set; }
        bool? HasError { get; set; }
        bool? HasNoError { get; set; }
        bool? MockSelected { get; set; }
        string ActivityTypeName { get; set; }
        bool IsTestView { get; set; }

        void VerifyErrorState();
        T As<T>() where T : class, IDebugTreeViewItemViewModel;
    }
}