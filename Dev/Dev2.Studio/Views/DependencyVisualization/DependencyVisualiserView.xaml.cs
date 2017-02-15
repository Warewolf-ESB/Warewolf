/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Services.Events;
using Infragistics.Controls.Maps;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.DependencyVisualization
{
    public partial class DependencyVisualiserView : IView
    {
        //private Point _scrollStartOffset;
        readonly IEventAggregator _eventPublisher;
        //ExplorerItemNodeViewModel _root;

        private bool _isMoveInEffect; // is the movement in effect?
        private NetworkNodeNodeControl _currentElement; // the element that we are 
        private Point _currentPosition; // the current position of that element

        public DependencyVisualiserView()
            : this(EventPublishers.Aggregator)
        {
        }

        public DependencyVisualiserView(IEventAggregator eventPublisher)
        {
            InitializeComponent();
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;

            Nodes.NodeControlAttachedEvent += (sender, e) =>
            {
                e.NodeControl.MouseLeftButtonUp += ElementMouseLeftButtonUp;
                e.NodeControl.MouseLeftButtonDown += ElementMouseLeftButtonDown;
                e.NodeControl.MouseMove += ElementMouseMove;
            };

            Nodes.NodeControlDetachedEvent += (sender, e) =>
            {
                e.NodeControl.MouseLeftButtonUp -= ElementMouseLeftButtonUp;
                e.NodeControl.MouseLeftButtonDown -= ElementMouseLeftButtonDown;
                e.NodeControl.MouseMove -= ElementMouseMove;                
            };
        }

        private void ElementMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            _currentElement = element; // keep track of which node this is
            element.CaptureMouse();
            _isMoveInEffect = true; // initiate the movement effect
            _currentPosition = e.GetPosition(element.Parent as UIElement); // keep track of position
        }

        private void ElementMouseMove(object sender, MouseEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            if (_currentElement == null || !Equals(element, _currentElement))
            {
                // this might happen if a node is released outside of the view area.
                // terminate the movement effect.
                _isMoveInEffect = false;
            }
            else if (_isMoveInEffect) // is the movement effect active?
            {
                if (e.GetPosition(Nodes).X > Nodes.ActualWidth || e.GetPosition(Nodes).Y > Nodes.ActualHeight || e.GetPosition(Nodes).Y < 0.0)
                {
                    // drag is outside of the allowable area, so release the element
                    element.ReleaseMouseCapture();
                    _isMoveInEffect = false;
                }
                else
                {
                    // drag is within the allowable area, so update the element's position
                    var currentPosition = e.GetPosition(element.Parent as UIElement);

                    element.Node.Location = new Point(
                        element.Node.Location.X + (currentPosition.X - _currentPosition.X) / Nodes.ZoomLevel,
                        element.Node.Location.Y + (currentPosition.Y - _currentPosition.Y) / Nodes.ZoomLevel);

                    _currentPosition = currentPosition;
                }
            }
        }

        private void ElementMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            element.ReleaseMouseCapture();
            _isMoveInEffect = false; // terminate the movement effect
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            ////2012.10.01: massimo.guerrera - Added for the click through on the dependency viewer
            //if (e.ClickCount == 2)
            //{
            //    ReleaseMouseCapture();
            //    FrameworkElement fe = e.OriginalSource as FrameworkElement;
            //    FrameworkContentElement fce = e.OriginalSource as FrameworkContentElement;
            //    object dataContext = null;

            //    if (fe != null)
            //    {
            //        dataContext = fe.DataContext;
            //    }
            //    else if (fce != null)
            //    {
            //        dataContext = fce.DataContext;
            //    }

            //    string resourceName = dataContext as string;

            //    if (string.IsNullOrEmpty(resourceName) && dataContext is Node)
            //    {
            //        resourceName = (dataContext as Node).ID;
            //    }

            //    if (!string.IsNullOrEmpty(resourceName))
            //    {
            //        var vm = DataContext as DependencyVisualiserViewModel;
            //        if (vm != null)
            //        {
            //            IResourceModel resource = vm.ResourceModel.Environment.ResourceRepository.FindSingle(c => c.ResourceName == resourceName);
            //            if (resource != null)
            //            {
            //                WorkflowDesignerUtils.EditResource(resource, _eventPublisher);
            //            }
            //        }
            //    }
            //}

            //e.GetPosition(this);
            //_scrollStartOffset.X = MyScrollViewer.HorizontalOffset;
            //_scrollStartOffset.Y = MyScrollViewer.VerticalOffset;

            //// UpdateMode the cursor if scrolling is possible 
            //Cursor = (MyScrollViewer.ExtentWidth > MyScrollViewer.ViewportWidth) ||
            //    (MyScrollViewer.ExtentHeight > MyScrollViewer.ViewportHeight) ?
            //    Cursors.ScrollAll : Cursors.Arrow;

            //CaptureMouse();
            //base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            //if (IsMouseCaptured)
            //{
            //    // Get the new mouse position. 
            //    Point mouseDragCurrentPoint = e.GetPosition(this);

            //    // Scroll to the new position. 
            //    MyScrollViewer.ScrollToHorizontalOffset(mouseDragCurrentPoint.X);
            //    MyScrollViewer.ScrollToVerticalOffset(mouseDragCurrentPoint.Y);
            //}
            //base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            //if (IsMouseCaptured)
            //{
            //    Cursor = Cursors.Arrow;
            //    ReleaseMouseCapture();
            //}
            //base.OnPreviewMouseUp(e);
        }

        void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Nodes.UpdateNodeArrangement();
        }

        void Nodes_OnNodeMouseDoubleClick(object sender, NetworkNodeClickEventArgs e)
        {
            var id = ((ExplorerItemNodeViewModel)e.NodeControl.Node.Data).ResourceId;
            var activeServer = CustomContainer.Get<IShellViewModel>().ActiveServer;
            CustomContainer.Get<IShellViewModel>().OpenResource(id,activeServer.EnvironmentID, activeServer);
            
        }
    }
}
