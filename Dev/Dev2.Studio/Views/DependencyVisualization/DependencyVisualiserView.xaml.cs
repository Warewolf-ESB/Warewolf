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
using Dev2.Services.Events;
using Dev2.Studio.Interfaces;
using Infragistics.Controls.Maps;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;


namespace Dev2.Studio.Views.DependencyVisualization
{
    public partial class DependencyVisualiserView : IView
    {
        readonly IEventAggregator _eventPublisher;

        private bool _isMoveInEffect;
        private NetworkNodeNodeControl _currentElement;
        private Point _currentPosition;

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
            _currentElement = element;
            element.CaptureMouse();
            _isMoveInEffect = true;
            _currentPosition = e.GetPosition(element.Parent as UIElement);
        }

        private void ElementMouseMove(object sender, MouseEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            if (_currentElement == null || !Equals(element, _currentElement))
            {
                _isMoveInEffect = false;
            }
            else
            {
                if (_isMoveInEffect)
                {
                    if (e.GetPosition(Nodes).X > Nodes.ActualWidth || e.GetPosition(Nodes).Y > Nodes.ActualHeight || e.GetPosition(Nodes).Y < 0.0)
                    {
                        element.ReleaseMouseCapture();
                        _isMoveInEffect = false;
                    }
                    else
                    {
                        var currentPosition = e.GetPosition(element.Parent as UIElement);

                        element.Node.Location = new Point(
                            element.Node.Location.X + (currentPosition.X - _currentPosition.X) / Nodes.ZoomLevel,
                            element.Node.Location.Y + (currentPosition.Y - _currentPosition.Y) / Nodes.ZoomLevel);

                        _currentPosition = currentPosition;
                    }
                }
            }
        }

        private void ElementMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            element.ReleaseMouseCapture();
            _isMoveInEffect = false;
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
