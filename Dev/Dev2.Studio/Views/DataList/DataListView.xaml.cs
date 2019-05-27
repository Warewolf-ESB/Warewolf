#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Resources.Languages;

namespace Dev2.Studio.Views.DataList
{
    /// <summary>
    /// Interaction logic for DataListView.xaml
    /// </summary>
    public partial class DataListView : IView, ICheckControlEnabledView
    {
        public DataListView()
        {
            InitializeComponent();
            KeyboardNavigation.SetTabNavigation(ScalarExplorer, KeyboardNavigationMode.Cycle);
        }

        #region Events

        void NametxtTextChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is IDataListViewModel vm)
            {
                var txtbox = sender as TextBox;
                if (txtbox?.DataContext is IDataListItemModel itemThatChanged)
                {
                    itemThatChanged.IsExpanded = true;
                }
                vm.AddBlankRow(null);
            }
        }

        void Inputcbx_OnChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null || !checkBox.IsEnabled)
            {
                return;
            }
            WriteToResourceModel();
        }

        void Outputcbx_OnChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null || !checkBox.IsEnabled)
            {
                return;
            }
            WriteToResourceModel();
        }

        void NametxtFocusLost(object sender, RoutedEventArgs e)
        {
            DoDataListValidation(sender);
        }

        void DoDataListValidation(object sender)
        {
            if (DataContext is IDataListViewModel vm && sender is TextBox txtbox)
            {
                var itemThatChanged = txtbox.DataContext as IDataListItemModel;
                vm.RemoveBlankRows(itemThatChanged);
                vm.ValidateVariableNamesForUI(itemThatChanged);

                if (vm.HasErrors && vm.DataListErrorMessage.Length != 0)
                {
                    vm.LogCustomTrackerEvent(TrackEventVariables.EventCategory, TrackEventVariables.IncorrectSyntax, "Variable Textbox input - " + vm.DataListErrorMessage);
                }
            }
        }

        void UserControlLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            WriteToResourceModel();
        }

        #endregion Events

        #region Private Methods

        void WriteToResourceModel()
        {
            if (DataContext is IDataListViewModel vm && !vm.IsSorting && !vm.HasErrors)
            {
                vm.WriteToResourceModel();
            }
        }

        #endregion Private Methods

        void UIElement_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var vm = DataContext as IDataListViewModel;
            var model = vm?.Parent as WorkSurfaceContextViewModel;
            model?.FindMissing();
        }

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Delete Variables":
                    return DeleteButton.Command.CanExecute(null);
                case "Sort Variables":
                    return SortButton.Command.CanExecute(null);
                case "Variables":
                    return ScalarExplorer.IsEnabled;
                default:
                    break;
            }

            return false;
        }

        #endregion

        private void SearchTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBoxSearch)
            {
                var vm = DataContext as IDataListViewModel;
                vm.LogCustomTrackerEvent(TrackEventVariables.EventCategory, TrackEventVariables.VariablesSearch, textBoxSearch.Text);
            }
        }
    }
}
