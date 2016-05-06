
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.WorkSurface;
using Infragistics.Windows.DataPresenter.Events;
using Microsoft.Practices.Prism.Mvvm;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.DataList
{
    /// <summary>
    /// Interaction logic for DataListView.xaml
    /// </summary>
    public partial class DataListView : IView,ICheckControlEnabledView
    {
        readonly IEventAggregator _eventPublisher;

        public DataListView()
            : this(EventPublishers.Aggregator)
        {
        }

        public DataListView(IEventAggregator eventPublisher)
        {
            InitializeComponent();

            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
            DataContextChanged += OnDataContextChanged;
            //KeyboardNavigation.SetTabNavigation(ScalarExplorer, KeyboardNavigationMode.Cycle);
        }


        #region Events

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                //vm.AddRecordsetNamesIfMissing();
            }
        }

        private void NametxtTextChanged(object sender, RoutedEventArgs e)
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {

                TextBox txtbox = sender as TextBox;
                if(txtbox != null)
                {
                    IDataListItemModel itemThatChanged = txtbox.DataContext as IDataListItemModel;
                    if (itemThatChanged != null) // && itemThatChanged.IsRecordset
                    {
                        itemThatChanged.IsExpanded = true;
                    }
                    if(itemThatChanged != null)
                    {
                        vm.AddBlankRow(itemThatChanged);
                        //vm.ValidateNames(itemThatChanged);
                    }
                }
            }
        }

        private void NametxtFocusLost(object sender, RoutedEventArgs e)
        {
            DoDataListValidation(sender);
        }

        void DoDataListValidation(object sender)
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                TextBox txtbox = sender as TextBox;
                if(txtbox != null)
                {
                    IDataListItemModel itemThatChanged = txtbox.DataContext as IDataListItemModel;
                    vm.RemoveBlankRows(itemThatChanged);
                    //vm.AddRecordsetNamesIfMissing();
                    vm.ValidateNames(itemThatChanged);
                }
            }
        }

        private void UserControlLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //var keyboardFocus = Keyboard.FocusedElement as UIElement;
            //if (e.KeyboardDevice.IsKeyDown(Key.Tab))
            //{
            //    if (keyboardFocus != null)
            //    {
            //        keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            //        e.Handled = true;
            //    }
            //}
            WriteToResourceModel();
        }

        private void Inputcbx_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if(checkBox == null || !checkBox.IsEnabled)
            {
                return;
            }
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if (vm != null && !vm.IsSorting)
            {
                WriteToResourceModel();
            }
        }

        private void Outputcbx_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if(checkBox == null || !checkBox.IsEnabled)
            {
                return;
            }

            IDataListViewModel vm = DataContext as IDataListViewModel;
            if (vm != null && !vm.IsSorting)
            {
                WriteToResourceModel();
            }
        }

        #endregion Events

        #region Private Methods

        private void WriteToResourceModel()
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                vm.WriteToResourceModel();
                _eventPublisher.Publish(new UpdateIntellisenseMessage());
            }
        }

        #endregion Private Methods

        void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {

                Button btn = sender as Button;
                if(btn != null)
                {
                    IDataListItemModel itemThatChanged = btn.DataContext as IDataListItemModel;
                    vm.RemoveDataListItem(itemThatChanged);
                    WriteToResourceModel();
                }
            }
        }

        private void UIElement_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                var model = vm.Parent as WorkSurfaceContextViewModel;
                if(model != null)
                {
                    model.FindMissing();
                }

            }
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
                   return Xtg.IsEnabled;
            }
            
            return false;
        }

        #endregion

        public void ExecuteCommand(string command)
        {
            if (command.Equals("Delete Variables", StringComparison.OrdinalIgnoreCase))
            {
                DeleteButton.Command.Execute(null);
            }
            if (command.Equals("lr().a", StringComparison.OrdinalIgnoreCase))
            {
                DeleteButton.Command.Execute(null);
            }
            if (command.Equals("[[a]]", StringComparison.OrdinalIgnoreCase))
            {
                DeleteButton.Command.Execute(null);
            }
            if (command.Equals("mr()", StringComparison.OrdinalIgnoreCase))
            {
                DeleteButton.Command.Execute(null);
            }
        }

        private void Xtg_OnAssigningFieldLayoutToItem(object sender, AssigningFieldLayoutToItemEventArgs e)
        {
            if(e.Item != null)
            {
                if (e.Item.GetType() == typeof(RecordSetItemModel))
                {
                    var fieldLayouts = Xtg.FieldLayouts;
                    var fieldLayout = fieldLayouts[2];
                    if (fieldLayout != null)
                    {
                        e.FieldLayout = fieldLayout;
                    }
                }
               
                if (e.Item.GetType() == typeof(RecordSetFieldItemModel))
                {
                    var fieldLayouts = Xtg.FieldLayouts;
                    var fieldLayout = fieldLayouts[1];
                    if (fieldLayout != null)
                    {
                        e.FieldLayout = fieldLayout;
                    }
                }
            }
        }
    }
}
