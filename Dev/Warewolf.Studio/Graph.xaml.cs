using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Infragistics.Controls.Maps;
using Moq;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : Window
    {
        private bool _isMoveInEffect; // is the movement in effect?
        private NetworkNodeNodeControl _currentElement; // the element that we are moving
        private Point _currentPosition; // the current position of that element
        ExplorerItemNodeViewModel _root;
        public Graph()
        {
            InitializeComponent();
            SetupNodes(Visibility.Visible);
        
            Nodes.NodeControlAttachedEvent += (sender, e) =>
            {
                e.NodeControl.MouseLeftButtonUp += Element_MouseLeftButtonUp;
                e.NodeControl.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                e.NodeControl.MouseMove += Element_MouseMove;
            };

            Nodes.NodeControlDetachedEvent += (sender, e) =>
            {
                e.NodeControl.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
                e.NodeControl.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                e.NodeControl.MouseMove -= Element_MouseMove;
            };
        }

        double ZoomLevel { get; set; }

        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            _currentElement = element; // keep track of which node this is
            element.CaptureMouse();
            _isMoveInEffect = true; // initiate the movement effect
            _currentPosition = e.GetPosition(element.Parent as UIElement); // keep track of position
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            if (_currentElement == null || element != _currentElement)
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

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            element.ReleaseMouseCapture();
            _isMoveInEffect = false; // terminate the movement effect
        }


        private void Redraw_Button_Click(object sender, RoutedEventArgs e)
        {
            // re-draw nodes
            Nodes.UpdateNodeArrangement();
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        void SetupNodes(Visibility visibility)
        {
            var shell = new Mock<IShellViewModel>().Object;
            var server = new Mock<IServer>().Object;
            var helper = new Mock<IExplorerHelpDescriptorBuilder>().Object;

           _root = new ExplorerItemNodeViewModel(shell, server, helper, null)
            {
                ResourceName = "bob",
                TextVisibility = visibility,
                ResourceType = ResourceType.WorkflowService,
                IsMainNode = Visibility.Visible,
                Children = new ObservableCollection<IExplorerItemViewModel>()
                {
                    new ExplorerItemNodeViewModel(shell,server,helper,null)
                    {
                        ResourceName = "child 1",
                         ResourceType = ResourceType.PluginService,
                         TextVisibility = visibility,
                        Children = new ObservableCollection<IExplorerItemViewModel>()
                        {
                          new ExplorerItemNodeViewModel(shell,server,helper,null)
                            {
                                ResourceName = "granchild 1",
                                ResourceType = ResourceType.WorkflowService,
                                TextVisibility = visibility,
                                Children = new ObservableCollection<IExplorerItemViewModel>()
                                {
                    
                                }   
                             },
                           new ExplorerItemNodeViewModel(shell,server,helper,null)
                            {

                                ResourceName = "granchild 2",
                                ResourceType = ResourceType.WorkflowService,
                                TextVisibility = visibility,
                                Children = new ObservableCollection<IExplorerItemViewModel>()
                                {
                    
                                }   
                             }
                         }
                      },
                    new ExplorerItemNodeViewModel(shell,server,helper,null)
                    {
                        ResourceName = "child 2",
                         ResourceType = ResourceType.DbService,
                         TextVisibility = visibility,
                        Children = new ObservableCollection<IExplorerItemViewModel>()
                        {
                          new ExplorerItemNodeViewModel(shell,server,helper,null)
                            {
                                ResourceName = "granchild 1",
                                TextVisibility = visibility,
                                Children = new ObservableCollection<IExplorerItemViewModel>()
                                {
                    
                                }   
                             }
                         }
                      }
                }
            };
            if(Nodes != null)
            {
                Nodes.ItemsSource = new ObservableCollection<ExplorerItemNodeViewModel>() { _root, _root.NodeChildren.First(), _root.NodeChildren.Last(), _root.NodeChildren.First().NodeChildren.First(), _root.NodeChildren.First().NodeChildren.Last() };
              
            }
        }

        void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            SetupNodes(LabeBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed);
        }

        void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Nodes.UpdateNodeArrangement();
        }
    }
}
