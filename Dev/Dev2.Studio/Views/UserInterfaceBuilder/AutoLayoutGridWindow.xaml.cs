using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.TO;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Web;
using Unlimited.Framework;

namespace Dev2.Studio.Views.UserInterfaceBuilder
{
    /// <summary>
    ///     Interaction logic for AutoLayoutGrid.xaml
    /// </summary>
    public partial class AutoLayoutGridWindow : IHandle<TabClosedMessage>, IHandle<UpdateWebpagePreviewMessage>
    {
        #region Class Members

        private bool _contextMenuOpened;
        private DataObject _data;
        private LayoutObjectViewModel _dragStartObject;
        private bool _dragStarted;
        private Point _start;
        private bool _webPreviewLoaded;

        #endregion Class Members

        #region Constructor

        public AutoLayoutGridWindow(ILayoutGridViewModel viewModel)
        {
            InitializeComponent();
            ImportService.SatisfyImports(this);
            webBrowser.Initialize();
            EventPublishers.Aggregator.Subscribe(this);

            // Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateWebpagePreview, UpdateWebpagePreview);
            DataContext = viewModel;
            Loaded += AutoLayoutGridWindow_Loaded;
        }

        #endregion

        #region Private Methods

        void UpdateWebpagePreview(IWebBrowserNavigateRequestTO webBrowserNavigateRequestTo)
        {
            if(webBrowserNavigateRequestTo != null && webBrowserNavigateRequestTo.DataContext == DataContext)
            {
                var layoutGridViewModel = DataContext as ILayoutGridViewModel;
                if(layoutGridViewModel != null && layoutGridViewModel.ResourceModel != null)
                {
                    ErrorResultTO errors;
                    webBrowser.Post(webBrowserNavigateRequestTo.Uri, layoutGridViewModel.ResourceModel.Environment, webBrowserNavigateRequestTo.Payload, out errors);
                }
            }
        }

        #endregion

        #region Event handlers

        void AutoLayoutGridWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var layoutGridViewModel = DataContext as ILayoutGridViewModel;
            if(layoutGridViewModel != null && !_webPreviewLoaded)
            {
                _webPreviewLoaded = true;
                layoutGridViewModel.Navigate();
            }
        }

        protected void AutoLayoutGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _start = e.GetPosition(this);
            if(e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                _contextMenuOpened = e.RightButton == MouseButtonState.Pressed;
                _dragStarted = false;
                _start = e.GetPosition(this);
                var layoutObj = (e.Source as dynamic).DataContext as LayoutObjectViewModel;
                if(layoutObj != null)
                {
                    _dragStartObject = layoutObj;
                    layoutObj.LayoutObjectGrid.LayoutObjects.ToList().ForEach(c => c.IsSelected = false);
                    layoutObj.IsSelected = true;
                }
            }
        }

        protected void AutoLayoutGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(_contextMenuOpened) return;

            if(e.LeftButton != MouseButtonState.Pressed || _dragStarted) return;

            var current = e.GetPosition(this);
            if(!(Math.Abs(current.X - _start.X) >= 15.0) && !(Math.Abs(current.Y - _start.Y) >= 15.0)) return;

            var source = (e.OriginalSource as dynamic).DataContext as LayoutObjectViewModel;
            if(source == null) return;

            if(_dragStartObject == null) return;

            _dragStarted = true;
            _data = new DataObject();
            _data.SetData(_dragStartObject);

            Debug.WriteLine("running");

            if(e.OriginalSource is DependencyObject)
                DragDrop.DoDragDrop(e.OriginalSource as DependencyObject, _data,
                    DragDropEffects.Link);
        }

        void Border_PreviewDrop(object sender, DragEventArgs e)
        {
            _dragStarted = false;
            var dragSource = e.Data.GetData(typeof(AbstractTreeViewModel));

            if(dragSource != null)
            {
                var resourceTreeViewModel = dragSource as ResourceTreeViewModel;
                if(resourceTreeViewModel != null)
                {
                    var draggedObject = resourceTreeViewModel.DataContext as ResourceModel;
                    //This is the drop operation from a toolbox onto the Layout Grid
                    //We want to render the cell that was dropped with the service information of
                    //of the webpart e.g. Name, Icon etc
                    if((draggedObject != null) &&
                       draggedObject.Category.Equals("Webpart", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var cell = (e.OriginalSource as dynamic).DataContext as LayoutObjectViewModel;
                        if(cell != null)
                        {
                            if(cell.HasContent)
                            {
                                cell.PreviousWebpartServiceName = cell.WebpartServiceName;
                                cell.PreviousIconPath = cell.IconPath;
                                cell.PreviousXmlConfig = cell.XmlConfiguration;
                                cell.Delete(true);
                            }
                            cell.LayoutObjectGrid.AddNewUIElement(cell, resourceTreeViewModel.DataContext.ResourceName,
                                resourceTreeViewModel.DataContext.IconPath);
                            //cell.WebpartServiceName = (dragSource as NavigationItemViewModel).ResourceModelItem.ResourceName;
                            //cell.IconPath = (dragSource as NavigationItemViewModel).ResourceModelItem.IconPath;
                            //if (cell.WebpartServiceName.Equals("File")) {
                            //    cell.LayoutObjectGrid.FormEncodingType = "multipart/form-data";
                            //}
                            //cell.LayoutObjectGrid.UpdateModelItem();
                            //cell.OpenPropertyEditor();
                        }
                    }
                }
            }
            else
            {
                dragSource = e.Data.GetData(typeof(LayoutObjectViewModel));
                if((dragSource is LayoutObjectViewModel) && (dragSource as LayoutObjectViewModel).HasContent)
                {
                    //The user has dragged an item from one position on the grid to another
                    var dropTarget = (e.Source as dynamic).DataContext as LayoutObjectViewModel;
                    if(dropTarget != null)
                    {
                        var source = dragSource as LayoutObjectViewModel;
                        //Dont update the cell as it was dropped onto itself
                        if(source == dropTarget)
                        {
                            return;
                        }

                        source.LayoutObjectGrid.Move(source, dropTarget);
                    }
                }
            }
        }

        void Border_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var vm = (e.OriginalSource as dynamic).DataContext as LayoutGridViewModel;
            if(vm == null) return;

            switch(e.Key)
            {
                case Key.Up:
                    vm.MoveUpCommand.Execute(null);
                    break;

                case Key.Down:
                    vm.MoveDownCommand.Execute(null);
                    break;

                case Key.Right:
                    vm.MoveRightCommand.Execute(null);
                    break;

                case Key.Left:
                    vm.MoveLeftCommand.Execute(null);
                    break;

                case Key.Delete:
                    vm.ActiveCell.DeleteCellCommand.Execute(null);
                    break;

                case Key.C:
                    if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        vm.CopyCommand.Execute(null);
                    }
                    break;

                case Key.V:
                    if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        vm.PasteCommand.Execute(null);
                    }
                    break;

                case Key.X:
                    if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        vm.CutCommand.Execute(null);
                    }
                    break;


                case Key.Z:
                    if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        vm.UndoCommand.Execute(null);
                    }

                    break;

                case Key.Y:
                    if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        vm.RedoCommand.Execute(null);
                    }
                    break;
            }
        }

        #endregion

        #region Implementation of IHandle<TabClosedMessage>

        public void Handle(TabClosedMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            if(message.Context.Equals(this))
            {
                Loaded -= AutoLayoutGridWindow_Loaded;
                EventPublishers.Aggregator.Unsubscribe(this);
                webBrowser.Dispose();
            }
        }

        #endregion

        #region Implementation of IHandle<UpdateWebpagePreviewMessage>

        public void Handle(UpdateWebpagePreviewMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            UpdateWebpagePreview(message.WebBrowserNavigateRequestTo);
        }

        #endregion
    }
}
