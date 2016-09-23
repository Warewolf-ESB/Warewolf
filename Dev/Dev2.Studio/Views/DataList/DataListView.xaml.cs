/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.WorkSurface;
using Infragistics.Windows.DataPresenter;
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
        //readonly IEventAggregator _eventPublisher;
        //private readonly FieldLayout _recordSetItemFieldLayout;
        //private FieldLayout _recordSetFieldFieldLayout;
        //private FieldLayout _complexObjectFieldLayout;

        public DataListView()
            : this(EventPublishers.Aggregator)
        {
        }

        public DataListView(IEventAggregator eventPublisher)
        {
            InitializeComponent();

            //VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            //_eventPublisher = eventPublisher;
            //DataContextChanged += OnDataContextChanged;
            //Xtg.DataContextChanged+=OnDataContextChanged;
            //Xtg.DataSourceChanged+=XtgOnDataSourceChanged;
            //KeyboardNavigation.SetTabNavigation(Xtg, KeyboardNavigationMode.Cycle);
            //var fieldLayouts = Xtg.FieldLayouts;
            //_recordSetFieldFieldLayout = fieldLayouts[1];
            //_recordSetItemFieldLayout = fieldLayouts[2];
            //_complexObjectFieldLayout = fieldLayouts[4];
        }

        private void XtgOnDataSourceChanged(object sender, RoutedPropertyChangedEventArgs<IEnumerable> routedPropertyChangedEventArgs)
        {
            ExpandAll();
        }

        private void ExpandAll()
        {
            //var recordCollectionBase = Xtg.Records;
            //if(recordCollectionBase != null)
            //{
            //    recordCollectionBase.ExpandAll(true);
            //}
        }

        #region Events

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ExpandAll();
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
                    if (itemThatChanged != null)
                    {
                        itemThatChanged.IsExpanded = true;
                    }
                    if(itemThatChanged != null)
                    {
                        vm.AddBlankRow(itemThatChanged);
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
                    vm.ValidateNames(itemThatChanged);
                }
            }
        }

        private void UserControlLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            WriteToResourceModel();
        }

        #endregion Events

        #region Private Methods

        private void WriteToResourceModel()
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                vm.WriteToResourceModel();
            }
        }

        #endregion Private Methods

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
                //case "Variables":
                //   return Xtg.IsEnabled;
            }
            
            return false;
        }

        #endregion

        private void Xtg_OnAssigningFieldLayoutToItem(object sender, AssigningFieldLayoutToItemEventArgs e)
        {
            //if (e.Item != null)
            //{
            //    var type = e.Item.GetType();

            //    if (type == typeof(RecordSetItemModel))
            //    {
            //        var fieldLayout = _recordSetItemFieldLayout;
            //        if (fieldLayout != null)
            //        {
            //            e.FieldLayout = fieldLayout;
            //        }
            //    }
            //    else if (type == typeof(RecordSetFieldItemModel))
            //    {
            //        var fieldLayout = _recordSetFieldFieldLayout;
            //        if (fieldLayout != null)
            //        {
            //            e.FieldLayout = fieldLayout;
            //        }
            //    }
            //    else if (type == typeof(ComplexObjectItemModel))
            //    {
            //        var fieldLayout = _complexObjectFieldLayout;
            //        if (fieldLayout != null)
            //        {
            //            e.FieldLayout = fieldLayout;
            //        }
            //    }
            //}
        }

        private void Xtg_OnLoaded(object sender, RoutedEventArgs e)
        {
           // Xtg.Records.ExpandAll(true);
        }

        private void DataListView_OnMouseEnter(object sender, MouseEventArgs e)
        {
            //WriteToResourceModel();
        }
    }
}
