
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
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dev2.Activities.Designers2.Core.Adorners;
using Dev2.Activities.Designers2.Core.Errors;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Activities.Designers2.Sequence;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Utilities;

namespace Dev2.Activities.Designers2.Core
{
    [ActivityDesignerOptions(AllowDrillIn = false, AlwaysCollapseChildren = true)]
    public class ActivityDesigner<TViewModel> : ActivityDesigner, IDisposable
        where TViewModel : ActivityDesignerViewModel
    {
        bool _isInitialFocusDone;
        readonly AdornerControl _helpAdorner;
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
            _helpAdorner = new HelpAdorner(this);
            _errorsAdorner = new ErrorsAdorner(this);

            Loaded += OnRoutedEventHandler;
            Unloaded += ActivityDesignerUnloaded;
            AllowDrop = true;
        }

        public TViewModel ViewModel { get { return DataContext as TViewModel; } }

        public ActivityDesignerTemplate ContentDesignerTemplate { get { return (ActivityDesignerTemplate)Content; } }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen when you double click.
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            ToggleView(e);
            if(!(e.OriginalSource is IScrollInfo))
            {
                e.Handled = true;
            }
            base.OnPreviewMouseDoubleClick(e);
        }

        void ToggleView(MouseButtonEventArgs eventArgs)
        {
            var originalSource = eventArgs.OriginalSource;
            var fe = originalSource as FrameworkElement;
            if(fe != null && (fe.TemplatedParent is ToggleButton || fe.TemplatedParent is ActivityDesignerButton))
            {
                return;
            }

            if((originalSource is Panel) || (originalSource is Shape) || (originalSource is Decorator) ||
               (originalSource is ScrollViewer))
            {
                if(eventArgs.Source is Large)
                {
                    return;
                }
                ShowCollapseLargeView();
                eventArgs.Handled = true;
            }
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

            BindingOperations.SetBinding(_helpAdorner, AdornerControl.IsAdornerVisibleProperty, new Binding(ActivityDesignerViewModel.ShowHelpProperty.Name)
            {
                Source = viewModel,
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
            if(element != null)
            {
                element.SetZIndex(viewModel.ZIndexPosition);
            }
        }

        void OnSelectionChanged(Selection item)
        {
            ViewModel.IsSelected = item.PrimarySelection == ModelItem;
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

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
            if(_designerManagementService != null)
            {
                _designerManagementService.CollapseAllRequested -= OnDesignerManagementServiceCollapseAllRequested;
                _designerManagementService.ExpandAllRequested -= OnDesignerManagementServiceExpandAllRequested;
                _designerManagementService.RestoreAllRequested -= OnDesignerManagementServiceRestoreAllRequested;
            }
            if(_showCollapseLargeView != null)
            {
                _showCollapseLargeView.Click -= ShowCollapseFromContextMenu;
            }
            if(Context != null)
            {
                Context.Items.Unsubscribe<Selection>(OnSelectionChanged);
                Context.Services.Unsubscribe<IDesignerManagementService>(OnDesignerManagementServiceChanged);
            }

            if(_zIndexProperty != null)
            {
                _zIndexProperty.RemoveValueChanged(_dataContext, OnZIndexPositionChanged);
            }
            if(ViewModel != null)
            {
                ViewModel.Dispose();
            }

            Loaded -= OnRoutedEventHandler;

            Unloaded -= ActivityDesignerUnloaded;
            CEventHelper.RemoveAllEventHandlers(this);
            CEventHelper.RemoveAllEventHandlers(this);
           GC.SuppressFinalize(this);
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
                    OnDispose();
                    if(_dataContext != null)
                    {
                        _dataContext.Dispose();
                    }
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
            base.OnContextMenuOpening(e);

            if(ViewModel != null && ViewModel.HasLargeView)
            {
                if(ViewModel.ShowLarge)
                {
                    var icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceCollapseMapping-32.png")), Height = 16, Width = 16 };
                    _showCollapseLargeView.Header = "Collapse Large View";
                    _showCollapseLargeView.Icon = icon;
                }
                else if(ViewModel.ShowSmall)
                {
                    var icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceExpandMapping-32.png")), Height = 16, Width = 16 };
                    _showCollapseLargeView.Header = "Show Large View";
                    _showCollapseLargeView.Icon = icon;
                }
            }
        }
    }
}
