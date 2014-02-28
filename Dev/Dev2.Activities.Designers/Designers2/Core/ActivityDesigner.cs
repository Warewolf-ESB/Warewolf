using System;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core.Adorners;
using Dev2.Activities.Designers2.Core.Errors;
using Dev2.Activities.Designers2.Core.Help;
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

        public ActivityDesigner()
        {
            //This line is bad it causes the overall designer to not get focus when clicking on it
            //Please be very careful about putting this line in.
            //FocusManager.SetIsFocusScope(this , true);
            _helpAdorner = new HelpAdorner(this);
            _errorsAdorner = new ErrorsAdorner(this);

            Loaded += (sender, args) => OnLoaded();
            Unloaded += ActivityDesignerUnloaded;
        }

        public TViewModel ViewModel { get { return DataContext as TViewModel; } }

        public ActivityDesignerTemplate ContentDesignerTemplate { get { return (ActivityDesignerTemplate)Content; } }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen when you double click.
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if(!(e.OriginalSource is IScrollInfo))
            {
                e.Handled = true;
            }
            base.OnPreviewMouseDoubleClick(e);
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

            if(Context != null)
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

            if(Context != null)
            {
                Context.Items.Unsubscribe<Selection>(OnSelectionChanged);
                Context.Services.Unsubscribe<IDesignerManagementService>(OnDesignerManagementServiceChanged);
            }

            if(_zIndexProperty != null)
            {
                _zIndexProperty.RemoveValueChanged(_dataContext, OnZIndexPositionChanged);
            }

            // ReSharper disable EventUnsubscriptionViaAnonymousDelegate
            Loaded -= (sender, args) => OnLoaded();
            // ReSharper restore EventUnsubscriptionViaAnonymousDelegate
            Unloaded -= ActivityDesignerUnloaded;
        }


        void ActivityDesignerUnloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
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
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion
    }
}
