/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Dev2.Activities.Designers2.Core.Adorners;
using Dev2.Activities.Designers2.Core.Errors;
using Dev2.Activities.Designers2.Sequence;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Utilities;
using FontAwesome.WPF;

namespace Dev2.Activities.Designers2.Core
{
    [ActivityDesignerOptions(AllowDrillIn = false, AlwaysCollapseChildren = true)]
    public class ActivityDesigner<TViewModel> : ActivityDesigner, IDisposable, IUpdatesHelp, IErrorsSource
        where TViewModel : ActivityDesignerViewModel
    {
        bool _isInitialFocusDone;
        readonly AdornerControl _errorsAdorner;
        IDesignerManagementService _designerManagementService;
        bool _isDisposed;
        DependencyPropertyDescriptor _zIndexProperty;
        // ReSharper disable InconsistentNaming
        protected TViewModel _dataContext;
        // ReSharper restore InconsistentNaming
        bool _isSetFocusActionSet;
        MenuItem _showCollapseLargeView;

        public ActivityDesigner()
        {
            //This line is bad it causes the overall designer to not get focus when clicking on it
            //Please be very careful about putting this line in.
            //FocusManager.SetIsFocusScope(this , true);
            _errorsAdorner = new ErrorsAdorner(this);
            Loaded += OnRoutedEventHandler;
            Unloaded += ActivityDesignerUnloaded;
            AllowDrop = true;
            
        }

        #region Overrides of WorkflowViewElement

        /// <summary>
        /// Invoked when the context menu is loaded. Implement this method in a derived class to handle this event.
        /// </summary>
        /// <param name="menu">The <see cref="T:System.Windows.Controls.ContextMenu"/> that is loaded.</param>
        protected override void OnContextMenuLoaded(ContextMenu menu)
        {
            int indexOfOpenItem = -1;
            foreach(var menuItem in menu.Items.Cast<object>().OfType<MenuItem>().Where(menuItem => (string)menuItem.Header == "_Open"))
            {
                indexOfOpenItem = menu.Items.IndexOf(menuItem);
                break;
            }
            if (indexOfOpenItem != -1)
            {
                menu.Items.RemoveAt(indexOfOpenItem);
            }
            base.OnContextMenuLoaded(menu);
        }

        #endregion

        public TViewModel ViewModel => DataContext as TViewModel;

        public ActivityDesignerTemplate ContentDesignerTemplate => (ActivityDesignerTemplate)Content;

        //don't TAKE OUT... This is used to block the test view workflow
        public bool IsServiceTestView
        {
            get
            {
                if (UpdateContentEnabled())
                    return false;
                return true;
            }
        }

        private bool UpdateContentEnabled()
        {
            DesignerView parentContentPane = FindDependencyParent.FindParent<DesignerView>(this);
            var dataContext = parentContentPane?.DataContext;
            if (dataContext != null)
            {
                if (dataContext.GetType().Name == "ServiceTestViewModel")
                {
                    if (ContentDesignerTemplate != null)
                    {
                        if (ContentDesignerTemplate.Parent.GetType().Name != "ForeachDesigner" &&
                            ContentDesignerTemplate.Parent.GetType().Name != "SequenceDesigner" &&
                            ContentDesignerTemplate.Parent.GetType().Name != "SelectAndApplyDesigner")
                        {
                            ContentDesignerTemplate.IsEnabled = false;
                        }
                        ContentDesignerTemplate.RightButtons.Clear();
                        ContentDesignerTemplate.LeftButtons.Clear();
                    }

                }
                return true;
            }
            return false;
        }

        //don't TAKE OUT... This has been done so that the drill down doesnt happen when you double click.
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            ToggleView(e);
            if (!(e.OriginalSource is IScrollInfo))
            {
                e.Handled = true;
            }
            base.OnPreviewMouseDoubleClick(e);
        }

        #region Overrides of UIElement

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyDown"/> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (ViewModel != null)
            {
                UpdateHelpDescriptor(ViewModel.HelpText);
            }
        }

        #endregion

        void ToggleView(MouseButtonEventArgs eventArgs)
        {
            var originalSource = eventArgs.OriginalSource;
            var fe = originalSource as FrameworkElement;
            if(fe != null && (fe.TemplatedParent is ToggleButton || fe.TemplatedParent is ActivityDesignerButton))
            {
                return;
            }

            if(originalSource is Panel || originalSource is Shape || originalSource is Decorator ||
               originalSource is ScrollViewer)
            {
                if(eventArgs.Source is Large)
                {
                    return;
                }
                if (ViewModel!=null && ViewModel.IsSelected)
                {
                    ShowCollapseLargeView();
                    eventArgs.Handled = true;
                }
            }
            UpdateContentEnabled();
        }

        void ShowCollapseLargeView()
        {
            if(ViewModel != null && ViewModel.HasLargeView)
            {
                if(ViewModel.ShowSmall)
                {
                    ViewModel.Expand();
                }
                else if(ViewModel.ShowLarge)
                {
                    ViewModel.Collapse();
                }
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if(!_isSetFocusActionSet)
            {
                var vm = DataContext as ActivityDesignerViewModel;
                if(vm != null)
                {
                    vm.SetIntialFocusAction(SetInitialiFocus);
                    _isSetFocusActionSet = true;
                    return;
                }
            }
            base.OnMouseEnter(e);
        }

        private void SetInitialiFocus()
        {
            if(!_isInitialFocusDone)
            {
                ContentDesignerTemplate.SetInitialFocus();
                _isInitialFocusDone = true;
            }
        }

        protected virtual void OnLoaded()
        {
            _dataContext = CreateViewModel();
            DataContext = _dataContext;
            BuildInitialContextMenu();
            ApplyBindings(_dataContext);
            ApplyEventHandlers(_dataContext);
            UpdateContentEnabled();
        }

        protected virtual TViewModel CreateViewModel()
        {
            return (TViewModel)Activator.CreateInstance(typeof(TViewModel), ModelItem);
        }

        void ApplyBindings(TViewModel viewModel)
        {
            BindingOperations.SetBinding(viewModel, ActivityDesignerViewModel.IsMouseOverProperty, new Binding(IsMouseOverProperty.Name)
            {
                Source = this,
                Mode = BindingMode.OneWay
            });

            BindingOperations.SetBinding(_errorsAdorner, AdornerControl.IsAdornerVisibleProperty, new Binding(ActivityDesignerViewModel.ShowErrorsProperty.Name)
            {
                Source = viewModel,
                Mode = BindingMode.OneWay
            });
        }

        void ApplyEventHandlers(TViewModel viewModel)
        {
            _zIndexProperty = DependencyPropertyDescriptor.FromProperty(ActivityDesignerViewModel.ZIndexPositionProperty, typeof(TViewModel));
            _zIndexProperty.AddValueChanged(viewModel, OnZIndexPositionChanged);

            if (Context != null)
            {
                Context.Items.Subscribe<Selection>(OnSelectionChanged);
                Context.Services.Subscribe<IDesignerManagementService>(OnDesignerManagementServiceChanged);
            }
        }

        void OnZIndexPositionChanged(object sender, EventArgs args)
        {
            var viewModel = (TViewModel)sender;

            var element = Parent as FrameworkElement;
            element?.SetZIndex(viewModel.ZIndexPosition);
        }

        void OnSelectionChanged(Selection item)
        {
            ViewModel.IsSelected = item.SelectedObjects.Any(modelItem => modelItem == ModelItem);
             //item.PrimarySelection == ModelItem;
        }

        void OnDesignerManagementServiceChanged(IDesignerManagementService designerManagementService)
        {
            if(_designerManagementService != null)
            {
                _designerManagementService.CollapseAllRequested -= OnDesignerManagementServiceCollapseAllRequested;
                _designerManagementService.ExpandAllRequested -= OnDesignerManagementServiceExpandAllRequested;
                _designerManagementService.RestoreAllRequested -= OnDesignerManagementServiceRestoreAllRequested;
                _designerManagementService = null;
            }

            if(designerManagementService != null)
            {
                _designerManagementService = designerManagementService;
                
                _designerManagementService.CollapseAllRequested += OnDesignerManagementServiceCollapseAllRequested;
                _designerManagementService.ExpandAllRequested += OnDesignerManagementServiceExpandAllRequested;
                _designerManagementService.RestoreAllRequested += OnDesignerManagementServiceRestoreAllRequested;
            }
        }



        protected void OnDesignerManagementServiceRestoreAllRequested(object sender, EventArgs e)
        {
            ViewModel.Restore();
        }

        protected void OnDesignerManagementServiceExpandAllRequested(object sender, EventArgs e)
        {
            ViewModel.Expand();
        }

        protected void OnDesignerManagementServiceCollapseAllRequested(object sender, EventArgs e)
        {
            ViewModel.Collapse();
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            ActivityHelper.HandleDragEnter(e);
        }

        #region IDisposable Members


        ~ActivityDesigner()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            ViewModel.UpdateHelpDescriptor(helpText);
        }

        void OnRoutedEventHandler(object sender, RoutedEventArgs args)
        {
            OnLoaded();
        }

        void ActivityDesignerUnloaded(object sender, RoutedEventArgs e)
        {
            OnUnloaded();
            Dispose();
        }

        protected virtual void OnUnloaded()
        {
  
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                    //OnDispose();
//                    if(_dataContext != null)
//                    {
//                        _dataContext.Dispose();
//                    }
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion

        protected void BuildInitialContextMenu()
        {
            ContextMenu = new ContextMenu();
            
            if(ViewModel != null && ViewModel.HasLargeView)
            {
                _showCollapseLargeView = new MenuItem { Header = "Show Large View" };
                _showCollapseLargeView.Click += ShowCollapseFromContextMenu;
                _showCollapseLargeView.SetValue(AutomationProperties.AutomationIdProperty, "UI_ShowLargeViewMenuItem_AutoID");
                ContextMenu.Items.Add(_showCollapseLargeView);
            }

        }

        void ShowCollapseFromContextMenu(object sender, RoutedEventArgs e)
        {
            ShowCollapseLargeView();
        }


        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            DesignerView parentContentPane = FindDependencyParent.FindParent<DesignerView>(this);
            var dataContext = parentContentPane?.DataContext;
            if (dataContext != null)
            {
                if (dataContext.GetType().Name == "ServiceTestViewModel")
                {
                    e.Handled = true;
                }
                else
                {
                    base.OnContextMenuOpening(e);

                    if (ViewModel != null && ViewModel.HasLargeView)
                    {
                        if (ViewModel.ShowLarge)
                        {
                            var imageSource = ImageAwesome.CreateImageSource(FontAwesomeIcon.Compress, Brushes.Black);
                            var icon = new Image {Source = imageSource, Height = 14, Width = 14};
                            _showCollapseLargeView.Header = "Collapse Large View";
                            _showCollapseLargeView.Icon = icon;
                        }
                        else if (ViewModel.ShowSmall)
                        {
                            var imageSource = ImageAwesome.CreateImageSource(FontAwesomeIcon.Expand, Brushes.Black);
                            var icon = new Image {Source = imageSource, Height = 14, Width = 14};
                            _showCollapseLargeView.Header = "Show Large View";
                            _showCollapseLargeView.Icon = icon;
                        }
                    }
                }
            }
        }

        #region Implementation of IErrorsSource

        public List<IActionableErrorInfo> Errors
        {
            get
            {
                return ViewModel.Errors;
            }
            set
            {
                ViewModel.Errors = value;
            }
        }

        #endregion
    }
}
