#pragma warning disable IDE1006, CC0091, S1226, S100, CC0044, CC0045, CC0021, CC0022, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001, IDE0019, CC0105, RECS008, RECS009, RECS003, CA2202
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Workspaces;
using Infragistics;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;


namespace Dev2.Studio.Dock
{
    public class ContentPaneFactory : ContainerFactory
    {
        DependencyObject _target;

        static ContentPaneFactory()
        {
            ContainerTypeProperty.OverrideMetadata(typeof(ContentPaneFactory), new FrameworkPropertyMetadata(typeof(ContentPane)));
        }
        
        protected override void ClearContainerForItem(DependencyObject container, object item)
        {
            if (container is ContentPane pane)
            {
                pane.Closed -= OnPaneClosed;
                pane.Closing -= OnPaneClosing;
            }

            base.ClearContainerForItem(container, item);
        }
        
        protected sealed override void OnItemInserted(DependencyObject container, object item, int index)
        {
            AddPane((ContentPane)container);
        }
        
        protected sealed override void OnItemMoved(DependencyObject container, object item, int oldIndex, int newIndex)
        {
        }
        
        protected sealed override void OnItemRemoved(DependencyObject container, object oldItem)
        {
            RemovePane((ContentPane)container);
        }
        
        protected override void PrepareContainerForItem(DependencyObject container, object item)
        {
             BindingHelper.BindPath(container, item, HeaderPath, HeaderedContentControl.HeaderProperty);
            BindingHelper.BindPath(container, item, ContentPath, ContentControl.ContentProperty);
            BindingHelper.BindPath(container, item, TabHeaderPath, ContentPane.TabHeaderProperty);

            base.PrepareContainerForItem(container, item);

            var pane = container as ContentPane;

            SetTabName(pane, item);
            
            if(pane != null)
            {
                pane.PreviewLostKeyboardFocus += pane_PreviewLostKeyboardFocus;
                pane.PreviewGotKeyboardFocus += pane_PreviewLostKeyboardFocus;
                pane.PreviewMouseDown+=PaneOnPreviewMouseDown;
                pane.Closed += OnPaneClosed;
                pane.Closing += OnPaneClosing;
                
                if (item is WorkSurfaceContextViewModel model)
                {
                    var vm = model;
                    vm.Deactivated += ViewModelDeactivated;
                }


                if (RemoveItemOnClose)
                {
                    var cv = CollectionViewSource.GetDefaultView(ItemsSource) as IEditableCollectionView;

                    pane.CloseAction = PaneCloseAction.RemovePane;

                    if(null == cv || !cv.CanRemove)
                    {
                        pane.AllowClose = false;
                    }
                }
            }
        }

        void PaneOnPreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
            if (mvm?.ActiveItem != null)
            {
                var item = sender as ContentPane;
                var workSurfaceContextViewModel = item?.DataContext as WorkSurfaceContextViewModel;
                if (mvm.ActiveItem != workSurfaceContextViewModel)
                {
                    mvm.ActiveItem = workSurfaceContextViewModel;
                }
            }
        }

        void pane_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

            if (sender is ContentPane contentPane)
            {
                var tabGroupPane = contentPane.Parent as TabGroupPane;
                var splitPane = tabGroupPane?.Parent as SplitPane;
                if (splitPane?.Parent is PaneToolWindow paneToolWindow && string.IsNullOrWhiteSpace(paneToolWindow.Title))
                {
                    var hasMainWindow = Application.Current != null && Application.Current.MainWindow != null;
                    if (hasMainWindow && Application.Current.MainWindow.DataContext != null && Application.Current.MainWindow.DataContext is ShellViewModel mainViewModel)
                    {
                        paneToolWindow.Title = mainViewModel.DisplayName;
                    }
                }
            }
        }

        void ViewModelDeactivated(object sender, DeactivationEventArgs e)
        {
            if (e.WasClosed && _target is TabGroupPane container && sender is WorkSurfaceContextViewModel model)
            {
                var toRemove = container.Items.Cast<ContentPane>().ToList()
                    .FirstOrDefault(p => p.Content != null && p.Content == model.WorkSurfaceViewModel);

                if (toRemove != null)
                {
                    RemovePane(toRemove);
                }
                if (toRemove != null &&
                    Application.Current != null &&
                    !Application.Current.Dispatcher.HasShutdownStarted)
                {
                    container.Items.Remove(toRemove);
                }
            }


        }

        void SetTabName(ContentPane pane, object item)
        {
            if (item is WorkSurfaceContextViewModel model)
            {
                var vm = model;
                pane.Name = vm.WorkSurfaceKey.ToString();
            }
            else
            {
                pane.Name = item.ToString();
            }
        }

        protected sealed override void ValidateContainerType(Type elementType)
        {
            if(!typeof(ContentPane).IsAssignableFrom(elementType))
            {
                throw new ArgumentException("ContainerType must be a ContentPane or a derived class.");
            }
            base.ValidateContainerType(elementType);
        }
        public static readonly DependencyProperty ContentPathProperty = DependencyProperty.Register("ContentPath",
            typeof(string), typeof(ContentPaneFactory), new FrameworkPropertyMetadata(null));
        
        [Description("Returns or sets the path to the property on the underlying item that should be used to provide the Content for the ContentPane.")]
        [Category("Behavior")]
        [Bindable(true)]
        public string ContentPath
        {
            get
            {
                return (string)GetValue(ContentPathProperty);
            }
            set
            {
                SetValue(ContentPathProperty, value);
            }
        }
        
        public static readonly DependencyProperty HeaderPathProperty = DependencyProperty.Register("HeaderPath",
            typeof(string), typeof(ContentPaneFactory), new FrameworkPropertyMetadata(null));
        
        [Description("Returns or sets the path to the property on the underlying item that should be used to provide the Header for the ContentPane.")]
        [Category("Behavior")]
        [Bindable(true)]
        public string HeaderPath
        {
            get
            {
                return (string)GetValue(HeaderPathProperty);
            }
            set
            {
                SetValue(HeaderPathProperty, value);
            }
        }
        
        public static readonly DependencyProperty PaneFactoryProperty =
            DependencyProperty.RegisterAttached("PaneFactory", typeof(ContentPaneFactory), typeof(ContentPaneFactory),
                new FrameworkPropertyMetadata(null,
                    OnPaneFactoryChanged));

        [AttachedPropertyBrowsableForType(typeof(DocumentContentHost))]
        [AttachedPropertyBrowsableForType(typeof(TabGroupPane))]
        [AttachedPropertyBrowsableForType(typeof(SplitPane))]
        public static ContentPaneFactory GetPaneFactory(DependencyObject d) => (ContentPaneFactory)d.GetValue(PaneFactoryProperty);

        public static void SetPaneFactory(DependencyObject d, ContentPaneFactory value)
        {
            d.SetValue(PaneFactoryProperty, value);
        }

        static void OnPaneFactoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DocumentContentHost || d is TabGroupPane || d is SplitPane)
            {
                var oldFactory = (ContentPaneFactory)e.OldValue;
                var newFactory = (ContentPaneFactory)e.NewValue;

                if (oldFactory != null && oldFactory.Equals(newFactory))
                {
                    return;
                }

                if (oldFactory != null)
                {
                    oldFactory._target = null;

                    foreach (var o in oldFactory.GetElements())
                    {
                        var cp = (ContentPane)o;
                        oldFactory.RemovePane(cp);
                    }
                }

                if (newFactory != null)
                {
                    newFactory._target = d;

                    foreach (var o in newFactory.GetElements())
                    {
                        var cp = (ContentPane)o;
                        newFactory.AddPane(cp);
                    }
                }
            }
        }

        public static readonly DependencyProperty RemoveItemOnCloseProperty = DependencyProperty.Register("RemoveItemOnClose",
            typeof(bool), typeof(ContentPaneFactory), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
        
        [Description("Returns or sets a boolean indicating whether to remove the item when the pane was closed.")]
        [Category("Behavior")]
        [Bindable(true)]
        public bool RemoveItemOnClose
        {
            get
            {
                return (bool)GetValue(RemoveItemOnCloseProperty);
            }
            set
            {
                SetValue(RemoveItemOnCloseProperty, value);
            }
        }
        
        public static readonly DependencyProperty TabHeaderPathProperty = DependencyProperty.Register("TabHeaderPath",
            typeof(string), typeof(ContentPaneFactory), new FrameworkPropertyMetadata(null));
        
        [Description("Returns or sets the path to the property on the underlying item that should be used to provide the TabHeader for the ContentPane.")]
        [Category("Behavior")]
        [Bindable(true)]
        public string TabHeaderPath
        {
            get
            {
                return (string)GetValue(TabHeaderPathProperty);
            }
            set
            {
                SetValue(TabHeaderPathProperty, value);
            }
        }
        
        protected virtual void AddPane(ContentPane pane)
        {
            if (_target is DocumentContentHost host)
            {
                var sibling = GetSiblingDocument();
                TabGroupPane tgp = null;

                if (sibling != null)
                {
                    tgp = LogicalTreeHelper.GetParent(sibling) as TabGroupPane;
                    Debug.Assert(null != tgp, "Expected all documents to be within a tab group pane.");
                }

                if (null == tgp)
                {
                    var sp = new SplitPane();
                    tgp = new TabGroupPane { Name = "Z" + Guid.NewGuid().ToString("N") };
                    sp.Panes.Add(tgp);
                    var dch = host;
                    dch.Panes.Add(sp);
                }

                tgp.Items.Add(pane);
                RaiseInitializeContentPane(pane);
            }
            else
            {
                IList targetCollection = null;

                Debug.Assert(_target == null || !string.IsNullOrEmpty((string)_target.GetValue(FrameworkElement.NameProperty)),
                    "The Name should be set so the container will not be removed when all the panes have been moved elsewhere. Otherwise new panes may not be displayed.");

                if (_target is SplitPane splitPane)
                {
                    targetCollection = splitPane.Panes;
                }
                else
                {
                    if (_target is TabGroupPane target)
                    {
                        targetCollection = target.Items;
                    }
                }

                if (null != targetCollection)
                {
                    targetCollection.Add(pane);

                    RaiseInitializeContentPane(pane);
                }
            }
        }

        ContentPane GetSiblingDocument()
        {
            var dch = _target as DocumentContentHost;

            if (dch == null)
            {
                return null;
            }

            if (null != dch.ActiveDocument)
            {
                return dch.ActiveDocument;
            }

            var dm = XamDockManager.GetDockManager(dch);

            if (dm == null)
            {
                return null;
            }

            ContentPane firstDocument = null;

            foreach (ContentPane cp in dm.GetPanes(PaneNavigationOrder.VisibleOrder))
            {
                if (cp.PaneLocation != PaneLocation.Document)
                {
                    continue;
                }

                if (firstDocument == null)
                {
                    firstDocument = cp;
                }

                if (cp.Visibility != Visibility.Visible)
                {
                    continue;
                }

                return cp;
            }

            return firstDocument;
        }

        public void OnPaneClosing(object sender, PaneClosingEventArgs e)
        {
            if (sender is ContentPane contentPane)
            {
                var pane = contentPane;

                if (pane.DataContext is WorkSurfaceContextViewModel model)
                {
                    CloseCurrentWorkSurfaceWorkflowDesignerViewModel(e, model);
                }
            }
        }

        static void CloseCurrentWorkSurfaceWorkflowDesignerViewModel(PaneClosingEventArgs e, WorkSurfaceContextViewModel model)
        {
            var workflowVm = model.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            var resource = workflowVm?.ResourceModel;

            if (resource != null && !resource.IsWorkflowSaved)
            {
                CloseCurrent(e, model);
            }
            else
            {
                if (model.WorkSurfaceViewModel is IStudioTab sourceView)
                {
                    CloseCurrent(e, model);
                }
            }
        }

        static void CloseCurrent(PaneClosingEventArgs e, WorkSurfaceContextViewModel model)
        {
            var vm = model;
            vm.TryClose();
            if (vm.Parent is ShellViewModel mainVm && !mainVm.CloseCurrent)
            {
                e.Cancel = true;
            }

        }


        void OnPaneClosed(object sender, PaneClosedEventArgs e)
        {
            if (sender is ContentPane pane && IsContainerInUse(pane) && pane.CloseAction == PaneCloseAction.RemovePane)
            {
                var cv = CollectionViewSource.GetDefaultView(ItemsSource) as IEditableCollectionView;

                Debug.Assert(cv != null && cv.CanRemove, "The ContentPane is being removed from the XamDockManager but it is still referenced by the source collection and it is not possible to remove it from the source collection.");

                if (cv != null && cv.CanRemove)
                {
                    var dataItem = GetItemForContainer(pane);
                    cv.Remove(dataItem);
                    var item = pane.Content as WorkflowDesignerViewModel;
                    if (item?.ResourceModel != null)
                    {
                        WorkspaceItemRepository.Instance.Remove(item.ResourceModel);
                    }

                    item?.RemoveUnsavedWorkflowName(item.DisplayName);
                }
            }
        }

        void RaiseInitializeContentPane(ContentPane pane)
        {
            if (null == _target)
            {
                return;
            }

            var args = new InitializeContentPaneEventArgs(pane) { RoutedEvent = InitializeContentPaneEvent };
            UiElementHelper.RaiseEvent(_target, args);
        }

        protected virtual void RemovePane(ContentPane cp)
        {
            var closeProp = ContentPane.CloseActionProperty;
            if (cp == null)
            {
                return;
            }

            var oldValue = cp.ReadLocalValue(closeProp);
            BindingExpressionBase oldExpression = cp.GetBindingExpression(closeProp);

            cp.CloseAction = PaneCloseAction.RemovePane;
            
            if(oldExpression != null)
            {
                cp.SetBinding(closeProp, oldExpression.ParentBindingBase);
            }
            else if(oldValue == DependencyProperty.UnsetValue)
            {
                cp.ClearValue(closeProp);
            }
            else
            {
                cp.SetValue(closeProp, oldValue);
            }
            cp.PreviewMouseDown -= PaneOnPreviewMouseDown;
        }
        
        public static readonly RoutedEvent InitializeContentPaneEvent = EventManager.RegisterRoutedEvent("InitializeContentPane",
            RoutingStrategy.Direct, typeof(EventHandler<InitializeContentPaneEventArgs>), typeof(ContentPaneFactory));


    }
    
    public class InitializeContentPaneEventArgs : RoutedEventArgs
    {
        readonly ContentPane _pane;
        
        public InitializeContentPaneEventArgs(ContentPane pane)
        {
            _pane = pane ?? throw new ArgumentNullException(nameof(pane));
        }
                
        public ContentPane Pane => _pane;
    }
}
