using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.AppResources.DependencyVisualization;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Utils;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.DependencyVisualization
{
    public partial class DependencyVisualiserView
    {
        private Point _scrollStartOffset;
        readonly IEventAggregator _eventPublisher;


        public DependencyVisualiserView()
            : this(EventPublishers.Aggregator)
        {
        }

        public DependencyVisualiserView(IEventAggregator eventPublisher)
        {
            InitializeComponent();
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //2012.10.01: massimo.guerrera - Added for the click through on the dependency viewer
            if(e.ClickCount == 2)
            {
                ReleaseMouseCapture();
                FrameworkElement fe = e.OriginalSource as FrameworkElement;
                FrameworkContentElement fce = e.OriginalSource as FrameworkContentElement;
                object dataContext = null;

                if(fe != null)
                {
                    dataContext = fe.DataContext;
                }
                else if(fce != null)
                {
                    dataContext = fce.DataContext;
                }

                string resourceName = dataContext as string;

                if(string.IsNullOrEmpty(resourceName) && dataContext is Node)
                {
                    resourceName = (dataContext as Node).ID;
                }

                if(!string.IsNullOrEmpty(resourceName))
                {
                    var vm = DataContext as DependencyVisualiserViewModel;
                    if(vm != null)
                    {
                        IResourceModel resource = vm.ResourceModel.Environment
                                                                 .ResourceRepository.FindSingle(
                                                                     c => c.ResourceName == resourceName);
                        if(resource != null)
                        {
                            WorkflowDesignerUtils.EditResource(resource, _eventPublisher);
                        }
                    }
                }
            }

            e.GetPosition(this);
            _scrollStartOffset.X = myScrollViewer.HorizontalOffset;
            _scrollStartOffset.Y = myScrollViewer.VerticalOffset;

            // Update the cursor if scrolling is possible 
            Cursor = (myScrollViewer.ExtentWidth > myScrollViewer.ViewportWidth) ||
                (myScrollViewer.ExtentHeight > myScrollViewer.ViewportHeight) ?
                Cursors.ScrollAll : Cursors.Arrow;

            CaptureMouse();
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if(IsMouseCaptured)
            {
                // Get the new mouse position. 
                Point mouseDragCurrentPoint = e.GetPosition(this);

                // Scroll to the new position. 
                myScrollViewer.ScrollToHorizontalOffset(mouseDragCurrentPoint.X);
                myScrollViewer.ScrollToVerticalOffset(mouseDragCurrentPoint.Y);
            }
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if(IsMouseCaptured)
            {
                Cursor = Cursors.Arrow;
                ReleaseMouseCapture();
            }
            base.OnPreviewMouseUp(e);
        }
    }
}