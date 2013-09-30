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

namespace Dev2.Activities.Designers2.Core
{
    [ActivityDesignerOptions(AllowDrillIn = false, AlwaysCollapseChildren = true)]
    public class ActivityDesigner<TViewModel> : ActivityDesigner
        where TViewModel : ActivityDesignerViewModel
    {
        readonly AdornerControl _helpAdorner;
        readonly AdornerControl _errorsAdorner;
        IDesignerManagementService _designerManagementService;

        public ActivityDesigner()
        {
            _helpAdorner = new HelpAdorner(this);
            _errorsAdorner = new ErrorsAdorner(this);

            Loaded += (sender, args) => OnLoaded();
        }

        public TViewModel ViewModel { get { return DataContext as TViewModel; } }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen when you double click.
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if(!(e.OriginalSource is IScrollInfo))
            {
                e.Handled = true;
            }
            base.OnPreviewMouseDoubleClick(e);
        }

        protected virtual void OnLoaded()
        {
            var viewModel = (TViewModel)Activator.CreateInstance(typeof(TViewModel), ModelItem);
            DataContext = viewModel;

            ApplyBindings(viewModel);
            ApplyEventHandlers(viewModel);
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
            var zIndexProperty = DependencyPropertyDescriptor.FromProperty(ActivityDesignerViewModel.ZIndexPositionProperty, typeof(TViewModel));
            zIndexProperty.AddValueChanged(viewModel, OnZIndexPositionChanged);

            if(Context != null)
            {
                Context.Items.Subscribe<Selection>(OnSelectionChanged);
                Context.Services.Subscribe<IDesignerManagementService>(OnDesignerManagementServiceChanged);
            }
        }

        void OnZIndexPositionChanged(object sender, EventArgs eventArgs)
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
    }
}
