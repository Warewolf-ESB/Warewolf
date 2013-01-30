using System.Windows;
using System.Windows.Input;
using Dev2;
using System.Windows.Controls;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace CircularDependencyTool
{   
    public partial class DependencyVisualiser : UserControl
    {
        private IContextualResourceModel _originResource;
        private Point mouseDragStartPoint;
        private Point scrollStartOffset; 

        public DependencyVisualiser(){
            InitializeComponent();
        }

        public DependencyVisualiser(IFrameworkSecurityContext securityContext, IContextualResourceModel originResource)
            : this()
        {
            var _graphBuidler = new GraphBuilder(securityContext, originResource).BuildGraphs();
            base.DataContext = _graphBuidler;
            _originResource = originResource;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            //2012.10.01: massimo.guerrera - Added for the click through on the dependency viewer
            string resourceName = string.Empty;
            if (e.ClickCount == 2)
            {
                this.ReleaseMouseCapture();
                FrameworkElement fe = e.OriginalSource as FrameworkElement;
                FrameworkContentElement fce = e.OriginalSource as FrameworkContentElement;
                object dataContext = null;

                if (fe != null)
                {
                    dataContext = fe.DataContext;
                }
                else if (fce != null)
                {
                    dataContext = fce.DataContext;
                }

                resourceName = dataContext as string;

                if (string.IsNullOrEmpty(resourceName) && dataContext is Node)
                {
                    resourceName = (dataContext as Node).ID;
                }                
                               
                if (!string.IsNullOrEmpty(resourceName))
                {
                    IResourceModel resource = _originResource.Environment.Resources.FindSingle(c => c.ResourceName == resourceName);
                    if (resource != null)
                    {
                        Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, resource);
                    }
                }                
            }

            mouseDragStartPoint = e.GetPosition(this);
            scrollStartOffset.X = myScrollViewer.HorizontalOffset;
            scrollStartOffset.Y = myScrollViewer.VerticalOffset;

            // Update the cursor if scrolling is possible 
            this.Cursor = (myScrollViewer.ExtentWidth > myScrollViewer.ViewportWidth) ||
                (myScrollViewer.ExtentHeight > myScrollViewer.ViewportHeight) ?
                Cursors.ScrollAll : Cursors.Arrow;

            this.CaptureMouse();
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e) {
            if (this.IsMouseCaptured) {
                // Get the new mouse position. 
                Point mouseDragCurrentPoint = e.GetPosition(this);

                // Determine the new amount to scroll. 
                Point delta = new Point(
                    (mouseDragCurrentPoint.X > this.mouseDragStartPoint.X) ?
                    -(mouseDragCurrentPoint.X - this.mouseDragStartPoint.X) :
                    (this.mouseDragStartPoint.X - mouseDragCurrentPoint.X),
                    (mouseDragCurrentPoint.Y > this.mouseDragStartPoint.Y) ?
                    -(mouseDragCurrentPoint.Y - this.mouseDragStartPoint.Y) :
                    (this.mouseDragStartPoint.Y - mouseDragCurrentPoint.Y));

                // Scroll to the new position. 
                myScrollViewer.ScrollToHorizontalOffset(mouseDragCurrentPoint.X);
                myScrollViewer.ScrollToVerticalOffset(mouseDragCurrentPoint.Y);
            }
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e) {
            if (this.IsMouseCaptured) {
                this.Cursor = Cursors.Arrow;
                this.ReleaseMouseCapture();
            }
            base.OnPreviewMouseUp(e);
        } 
    }
}