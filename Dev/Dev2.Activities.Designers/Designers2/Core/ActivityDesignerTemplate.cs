
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
using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Activities.Designers2.Core.Controls;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityDesignerTemplate : UserControl
    {
        Dev2DataGrid _dataGrid;

        protected ActivityDesignerTemplate()
        {
            LeftButtons = new ObservableCollection<Button>();
            RightButtons = new ObservableCollection<Button>();
        }

        public ObservableCollection<Button> LeftButtons { get; set; }
        public ObservableCollection<Button> RightButtons { get; set; }

        public Dev2DataGrid DataGrid
        {
            get
            {
                return _dataGrid;
            }
            set
            {
                // ReSharper disable PossibleUnintendedReferenceComparison
                if(_dataGrid != value)
                // ReSharper restore PossibleUnintendedReferenceComparison
                {
                    if(_dataGrid != null)
                    {
                        _dataGrid.SelectionChanged -= OnSelectionChanged;
                    }

                    _dataGrid = value;

                    if(_dataGrid != null)
                    {
                        _dataGrid.SelectionChanged += OnSelectionChanged;
                    }
                }
            }
        }

        public void SetInitialFocus()
        {
            // Wait for the UI to be fully rendered BEFORE trying to set the focus
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                var focusElement = GetInitialFocusElement();
                if(focusElement != null)
                {
                    Keyboard.Focus(focusElement);
                }
            }));
        }

        protected abstract IInputElement GetInitialFocusElement();

        void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            var viewModel = DataGrid.DataContext as ActivityCollectionDesignerViewModel;
            if(viewModel != null)
            {
                var oldItem = args.RemovedItems != null && args.RemovedItems.Count > 0 ? args.RemovedItems[0] : null;
                var newItem = args.AddedItems != null && args.AddedItems.Count > 0 ? args.AddedItems[0] : null;

                // basic null checks ppl - 3 days of crap for this  ;) 
                if(newItem != null)
                {
                    viewModel.OnSelectionChanged(oldItem as ModelItem, newItem as ModelItem);
                }
            }
        }
    }
}
