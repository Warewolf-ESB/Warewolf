
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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.WorkSurface;
using Infragistics;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Dock
{
    /// <summary>
    /// Class used to generate ContentPane instances based on a given source collection of items (<see cref="ContainerFactoryBase.ItemsSource"/>).
    /// </summary>
    public class ContentPaneFactory : ContainerFactory
    {
        #region Member Variables

        private DependencyObject _target;

        #endregion //Member Variables

        #region Constructor
        static ContentPaneFactory()
        {
            ContainerTypeProperty.OverrideMetadata(typeof(ContentPaneFactory), new FrameworkPropertyMetadata(typeof(ContentPane)));
        }

        #endregion

        #region Base class overrides

        #region ClearContainerForItem
        /// <summary>
        /// Used to clear any settings applied to a container in the <see cref="PrepareContainerForItem"/>
        /// </summary>
        /// <param name="container">The container element </param>
        /// <param name="item">The item from the source collection</param>
        protected override void ClearContainerForItem(DependencyObject container, object item)
        {
            var pane = container as ContentPane;
            if(pane != null)
            {
                pane.Closed -= OnPaneClosed;
                pane.Closing -= OnPaneClosing;
            }

            base.ClearContainerForItem(container, item);
        }

        #endregion //ClearContainerForItem

        #region OnItemInserted
        /// <summary>
        /// Invoked when an element for an item has been generated.
        /// </summary>
        /// <param name="item">The underlying item for which the element has been generated</param>
        /// <param name="container">The element that was generated</param>
        /// <param name="index">The index at which the item existed</param>
        protected sealed override void OnItemInserted(DependencyObject container, object item, int index)
        {
            AddPane((ContentPane)container);
        }
        #endregion //OnItemInserted

        #region OnItemMoved
        /// <summary>
        /// Invoked when an item has been moved in the source collection.
        /// </summary>
        /// <param name="item">The item that was moved</param>
        /// <param name="container">The associated element</param>
        /// <param name="oldIndex">The old index</param>
        /// <param name="newIndex">The new index</param>
        protected sealed override void OnItemMoved(DependencyObject container, object item, int oldIndex, int newIndex)
        {
        }

        #endregion //OnItemMoved

        #region OnItemRemoved
        /// <summary>
        /// Invoked when an element created for an item has been removed
        /// </summary>
        /// <param name="oldItem">The item associated with the element that was removed</param>
        /// <param name="container">The element that has been removed</param>
        protected sealed override void OnItemRemoved(DependencyObject container, object oldItem)
        {
            RemovePane((ContentPane)container);
        }
        #endregion //OnItemRemoved

        #region PrepareContainerForItem
        /// <summary>
        /// Used to initialize a container for a given item.
        /// </summary>
        /// <param name="container">The container element </param>
        /// <param name="item">The item from the source collection</param>
        protected override void PrepareContainerForItem(DependencyObject container, object item)
        {
             BindingHelper.BindPath(container, item, HeaderPath, HeaderedContentControl.HeaderProperty);
            BindingHelper.BindPath(container, item, ContentPath, ContentControl.ContentProperty);
            BindingHelper.BindPath(container, item, TabHeaderPath, ContentPane.TabHeaderProperty);

            base.PrepareContainerForItem(container, item);

            ContentPane pane = container as ContentPane;

            SetTabName(pane, item);

            //Aded to prevent tab from stealing focus from adorners
            //FocusManager.SetIsFocusScope(pane, false);
            if(pane != null)
            {
                pane.PreviewLostKeyboardFocus += pane_PreviewLostKeyboardFocus;
                pane.PreviewGotKeyboardFocus += pane_PreviewLostKeyboardFocus;

                // always hook the closed
                pane.Closed += OnPaneClosed;
                pane.Closing += OnPaneClosing;

                //Juries attach to events when viewmodel is closed/deactivated to close view.
                WorkSurfaceContextViewModel model = item as WorkSurfaceContextViewModel;
                if(model != null)
                {
                    var vm = model;
                    vm.Deactivated += ViewModelDeactivated;
                }


                if(RemoveItemOnClose)
                {
                    IEditableCollectionView cv = CollectionViewSource.GetDefaultView(ItemsSource) as IEditableCollectionView;

                    // set the pane to be removed from the dockmanager
                    pane.CloseAction = PaneCloseAction.RemovePane;

                    if(null == cv || !cv.CanRemove)
                    {
                        pane.AllowClose = false;
                    }
                }
            }
        }

        void pane_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
        }

        //Juries TODO improve (remove typing tied to contentfactory)
        private void ViewModelDeactivated(object sender, Caliburn.Micro.DeactivationEventArgs e)
        {
            if(e.WasClosed)
            {
                var container = _target as TabGroupPane;
                if(container != null)
                {
                    WorkSurfaceContextViewModel model = sender as WorkSurfaceContextViewModel;
                    if(model != null)
                    {
                        var vm = model.WorkSurfaceViewModel;
                        var toRemove = container.Items.Cast<ContentPane>().ToList()
                            .FirstOrDefault(p => p.Content != null && p.Content == vm);
                        RemovePane(toRemove);
                        if(toRemove != null &&
                            Application.Current != null &&
                            !Application.Current.Dispatcher.HasShutdownStarted)
                        {
                            container.Items.Remove(toRemove);
                        }
                    }
                }
            }
        }

        //Juries TODO improve (remove typing tied to contentfactory)
        private void SetTabName(ContentPane pane, object item)
        {
            WorkSurfaceContextViewModel model = item as WorkSurfaceContextViewModel;
            if(model != null)
            {
                var vm = model;
                pane.Name = vm.WorkSurfaceKey.ToString();
            }
            else pane.Name = item.ToString();
        }

        #endregion //PrepareContainerForItem

        #region ValidateContainerType
        /// <summary>
        /// Invoked when the <see cref="ContainerFactory.ContainerType"/> is about to be changed to determine if the specified type is allowed.
        /// </summary>
        /// <param name="elementType">The new element type</param>
        protected sealed override void ValidateContainerType(Type elementType)
        {
            if(!typeof(ContentPane).IsAssignableFrom(elementType))
            {
                throw new ArgumentException("ContainerType must be a ContentPane or a derived class.");
            }
            base.ValidateContainerType(elementType);
        }
        #endregion //ValidateContainerType

        #endregion //Base class overrides

        #region Properties

        #region ContentPath

        /// <summary>
        /// Identifies the <see cref="ContentPath"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ContentPathProperty = DependencyProperty.Register("ContentPath",
            typeof(string), typeof(ContentPaneFactory), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Returns or sets the path to the property on the underlying item that should be used to provide the Content for the ContentPane.
        /// </summary>
        /// <seealso cref="ContentPathProperty"/>
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

        #endregion //ContentPath

        #region HeaderPath

        /// <summary>
        /// Identifies the <see cref="HeaderPath"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HeaderPathProperty = DependencyProperty.Register("HeaderPath",
            typeof(string), typeof(ContentPaneFactory), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Returns or sets the path to the property on the underlying item that should be used to provide the Header for the ContentPane.
        /// </summary>
        /// <seealso cref="HeaderPathProperty"/>
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

        #endregion //HeaderPath

        #region PaneFactory

        /// <summary>
        /// PaneFactory Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty PaneFactoryProperty =
            DependencyProperty.RegisterAttached("PaneFactory", typeof(ContentPaneFactory), typeof(ContentPaneFactory),
                new FrameworkPropertyMetadata(null,
                    OnPaneFactoryChanged));

        /// <summary>
        /// Returns the object that creates ContentPane instances based on the items in the associated <see cref="ContainerFactoryBase.ItemsSource"/>
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(DocumentContentHost))]
        [AttachedPropertyBrowsableForType(typeof(TabGroupPane))]
        [AttachedPropertyBrowsableForType(typeof(SplitPane))]
        public static ContentPaneFactory GetPaneFactory(DependencyObject d)
        {
            return (ContentPaneFactory)d.GetValue(PaneFactoryProperty);
        }

        /// <summary>
        /// Sets the object that will create ContentPane instances based on the items in the associate <see cref="ContainerFactoryBase.ItemsSource"/>
        /// </summary>
        public static void SetPaneFactory(DependencyObject d, ContentPaneFactory value)
        {
            d.SetValue(PaneFactoryProperty, value);
        }

        private static void OnPaneFactoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is DocumentContentHost || d is TabGroupPane || d is SplitPane)
            {
                ContentPaneFactory oldFactory = (ContentPaneFactory)e.OldValue;
                ContentPaneFactory newFactory = (ContentPaneFactory)e.NewValue;

                if(oldFactory != null && oldFactory.Equals(newFactory))
                {
                    return;
                }

                if(oldFactory != null)
                {
                    oldFactory._target = null;

                    foreach(var o in oldFactory.GetElements())
                    {
                        var cp = (ContentPane)o;
                        oldFactory.RemovePane(cp);
                    }
                }

                if(newFactory != null)
                {
                    newFactory._target = d;

                    foreach(var o in newFactory.GetElements())
                    {
                        var cp = (ContentPane)o;
                        newFactory.AddPane(cp);
                    }
                }
            }
        }

        #endregion //PaneFactory

        #region RemoveItemOnClose

        /// <summary>
        /// Identifies the <see cref="RemoveItemOnClose"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RemoveItemOnCloseProperty = DependencyProperty.Register("RemoveItemOnClose",
            typeof(bool), typeof(ContentPaneFactory), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

        /// <summary>
        /// Returns or sets a boolean indicating whether to remove the item when the pane was closed.
        /// </summary>
        /// <seealso cref="RemoveItemOnCloseProperty"/>
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

        #endregion //RemoveItemOnClose

        #region TabHeaderPath

        /// <summary>
        /// Identifies the <see cref="TabHeaderPath"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TabHeaderPathProperty = DependencyProperty.Register("TabHeaderPath",
            typeof(string), typeof(ContentPaneFactory), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Returns or sets the path to the property on the underlying item that should be used to provide the TabHeader for the ContentPane.
        /// </summary>
        /// <seealso cref="TabHeaderPathProperty"/>
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

        #endregion //TabHeaderPath

        #endregion //Properties

        #region Methods

        #region AddPane
        /// <summary>
        /// Invoked when a new <see cref="ContentPane"/> has been created and needs to be added to the appropriate target collection.
        /// </summary>
        /// <param name="pane">The pane that was created and needs to be added to the appropriate collection</param>
        protected virtual void AddPane(ContentPane pane)
        {
            DocumentContentHost host = _target as DocumentContentHost;
            if(host != null)
            {
                ContentPane sibling = GetSiblingDocument();
                TabGroupPane tgp = null;

                if(sibling != null)
                {
                    tgp = LogicalTreeHelper.GetParent(sibling) as TabGroupPane;
                    Debug.Assert(null != tgp, "Expected all documents to be within a tab group pane.");
                }

                if(null == tgp)
                {
                    SplitPane sp = new SplitPane();
                    tgp = new TabGroupPane { Name = "Z" + Guid.NewGuid().ToString("N") };
                    sp.Panes.Add(tgp);
                    DocumentContentHost dch = host;
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

                SplitPane splitPane = _target as SplitPane;
                if(splitPane != null)
                {
                    targetCollection = splitPane.Panes;
                }
                else
                {
                    TabGroupPane target = _target as TabGroupPane;
                    if(target != null)
                    {
                        targetCollection = target.Items;
                    }
                }

                if(null != targetCollection)
                {
                    targetCollection.Add(pane);

                    RaiseInitializeContentPane(pane);
                }
            }
        }

        #endregion //AddPane

        #region GetSiblingDocument
        private ContentPane GetSiblingDocument()
        {
            DocumentContentHost dch = _target as DocumentContentHost;

            if(dch == null)
            {
                return null;
            }

            if(null != dch.ActiveDocument)
            {
                return dch.ActiveDocument;
            }

            XamDockManager dm = XamDockManager.GetDockManager(dch);

            if(dm == null)
            {
                return null;
            }

            ContentPane firstDocument = null;

            foreach(ContentPane cp in dm.GetPanes(PaneNavigationOrder.VisibleOrder))
            {
                if(cp.PaneLocation != PaneLocation.Document)
                {
                    continue;
                }

                if(firstDocument == null)
                {
                    firstDocument = cp;
                }

                if(cp.Visibility != Visibility.Visible)
                {
                    continue;
                }

                return cp;
            }

            return firstDocument;
        }
        #endregion //GetSiblingDocument

        #region OnPaneClosing
        public void OnPaneClosing(object sender, PaneClosingEventArgs e)
        {
            ContentPane contentPane = sender as ContentPane;
            if(contentPane != null)
            {
                var pane = contentPane;
                WorkSurfaceContextViewModel model = pane.DataContext as WorkSurfaceContextViewModel;
                if(model != null)
                {
                    var vm = model;
                    vm.TryClose();
                    var mainVm = vm.Parent as MainViewModel;
                    if(mainVm != null)
                    {
                        if(mainVm.CloseCurrent)
                        {
                            vm.Dispose();
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
        }

        #endregion

        #region OnPaneClosed
        private void OnPaneClosed(object sender, PaneClosedEventArgs e)
        {
            var pane = sender as ContentPane;

            // if the pane was closed because it was removed from the source collection
            // then we don't want to do anything. however if it is remove pane then 
            // we want to try and remove it from the source collection
            if(pane != null && (IsContainerInUse(pane) && pane.CloseAction == PaneCloseAction.RemovePane))
            {
                var cv = CollectionViewSource.GetDefaultView(ItemsSource) as IEditableCollectionView;

                Debug.Assert(null != cv && cv.CanRemove, "The ContentPane is being removed from the XamDockManager but it is still referenced by the source collection and it is not possible to remove it from the source collection.");

                if(cv.CanRemove)
                {
                    var dataItem = GetItemForContainer(pane);
                    cv.Remove(dataItem);
                }
            }
        }
        #endregion //OnPaneClosed

        #region RaiseInitializeContentPane
        private void RaiseInitializeContentPane(ContentPane pane)
        {
            if(null == _target)
            {
                return;
            }

            var args = new InitializeContentPaneEventArgs(pane) { RoutedEvent = InitializeContentPaneEvent };
            UiElementHelper.RaiseEvent(_target, args);
        }
        #endregion //RaiseInitializeContentPane

        #region RemovePane
        /// <summary>
        /// Invoked when a ContentPane for a given item is being removed.
        /// </summary>
        /// <param name="cp">The pane being removed</param>
        protected virtual void RemovePane(ContentPane cp)
        {
            // we need to temporarily change the close action while we close it
            DependencyProperty closeProp = ContentPane.CloseActionProperty;
            if(cp == null)
            {
                return;
            }

            object oldValue = cp.ReadLocalValue(closeProp);
            BindingExpressionBase oldExpression = cp.GetBindingExpression(closeProp);

            cp.CloseAction = PaneCloseAction.RemovePane;

            // restore the original close action
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
        }
        #endregion //RemovePane

        #endregion //Methods

        #region Events

        #region InitializeContentPane

        /// <summary>
        /// InitializeContentPane Attached Routed Event
        /// </summary>
        public static readonly RoutedEvent InitializeContentPaneEvent = EventManager.RegisterRoutedEvent("InitializeContentPane",
            RoutingStrategy.Direct, typeof(EventHandler<InitializeContentPaneEventArgs>), typeof(ContentPaneFactory));

        /// <summary>
        /// Adds a handler for the InitializeContentPane attached event
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="handler">Event handler to be added</param>
        public static void AddInitializeContentPaneHandler(DependencyObject element, EventHandler<InitializeContentPaneEventArgs> handler)
        {
            UiElementHelper.AddHandler(element, InitializeContentPaneEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the InitializeContentPane attached event
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="handler">Event handler to be removed</param>
        public static void RemoveInitializeContentPaneHandler(DependencyObject element, EventHandler<InitializeContentPaneEventArgs> handler)
        {
            UiElementHelper.RemoveHandler(element, InitializeContentPaneEvent, handler);
        }

        #endregion //InitializeContentPane

        #endregion //Events
    }

    /// <summary>
    /// Event arguments for the <see cref="ContentPaneFactory.InitializeContentPaneEvent"/>
    /// </summary>
    public class InitializeContentPaneEventArgs : RoutedEventArgs
    {
        #region Member Variables

        readonly ContentPane _pane;

        #endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="InitializeContentPaneEventArgs"/>
        /// </summary>
        /// <param name="pane">The pane being initialized</param>
        public InitializeContentPaneEventArgs(ContentPane pane)
        {
            if (pane == null)
            {
                throw new ArgumentNullException("pane");
            }

            _pane = pane;
        }
        #endregion //Constructor

        #region Properties
        /// <summary>
        /// Returns the pane being initialized
        /// </summary>
        public ContentPane Pane
        {
            get { return _pane; }
        }
        #endregion //Properties
    }
}
