using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Controls.Menus.Primitives;
using Infragistics.Controls.Primitives;
using Infragistics.DragDrop;
using Infragistics;
using System.Diagnostics;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A databound tree control 
    /// </summary>
    [StyleTypedProperty(Property = "ActiveNodeIndicatorStyle", StyleTargetType = typeof(ActiveNodeIndicator))]
    [TemplatePart(Name="Bd",Type=typeof(Border))]

    
    

    [ComplexBindingProperties("ItemsSource")]
    public class XamDataTree : Control, INotifyPropertyChanged, IProvideScrollInfo, ISupportScrollHelper
    {
        #region Members

        ScrollBar _verticalScrollbar, _horizontalScrollbar;
        NodesPanel _panel;
        XamDataTreeNodesManager _nodesManager;
        bool _mouseIsOver, _enteringEditMode = false, _isLoaded, _skipDefaultActivation;
        InternalNodesCollection _internalNodes;

        GlobalNodeLayoutCollection _globalNodeLayouts;
        TreeEditingSettings _editingSettings;
        TreeSelectionSettings _selectionSettings;
        XamDataTreeNode _activeNode;
        TreeDragSelectType _dragSelectType;
        Point _mousePosition;

        CheckBoxSettings _checkBoxSettings;
        XamDataTreeNode _mouseOverNode;

        Dictionary<string, object> _editCellValues, _originalNodeValues;
        long _doubleClickTimeStamp = 0;
        XamDataTreeNode _doubleClickNode;
        DispatcherTimer _selectNodesTimer;

        FrameworkElement _editorRootVisual;
        UIElement _parentUIElement = null;
        DispatcherTimer _scrollNodesTimer;
        DispatcherTimer _nodeExpanderTimer;
        XamDataTreeNode _draggingExpansionNode;

        DateTime _lastCancelledExitEditEvent;

        DropTarget _dropTarget;
        DragSource _dragSource;

        TouchScrollHelper _scrollHelper;





        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="XamDataTree"/> class.
        /// </summary>
        static XamDataTree()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamDataTree), new FrameworkPropertyMetadata(typeof(XamDataTree)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="XamDataTree"/> class.
        /// </summary>
        public XamDataTree()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DragObject dragObject = new DragObject();
            }




            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(XamDataTree_IsEnabledChanged);

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamDataTree), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            this._nodesManager = new XamDataTreeNodesManager(this);
            this.Loaded += new RoutedEventHandler(XamDataTree_Loaded);
            this.Unloaded += new RoutedEventHandler(XamDataTree_Unloaded);

            this._editCellValues = new Dictionary<string, object>();
            this._originalNodeValues = new Dictionary<string, object>();

            this.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.XamDataTree_KeyDown), true);
            this.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.XamDataTree_MouseLeftButtonDown), true);

            this._nodeExpanderTimer = new DispatcherTimer();
            this._nodeExpanderTimer.Tick += new EventHandler(NodeExpanderTimer_Tick);
            this._nodeExpanderTimer.Interval = TimeSpan.FromMilliseconds(500);

            this._dropTarget = new DropTarget() { };
            DragDropManager.SetDropTarget(this, this._dropTarget);
            this._dropTarget.IsDropTarget = true;
        }
        
        #endregion // Constructor

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Builds the visual tree for the <see cref="XamDataTree"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this._panel != null)
            {
                this._panel.ResetNodes();
                this._panel.Children.Clear();
            }

            this._panel = base.GetTemplateChild("NodesPanel") as NodesPanel;
            if (this._panel != null)
            {
                this._panel.Tree = this;
            }

            if (this._verticalScrollbar != null)
                this._verticalScrollbar.Scroll -= VerticalScrollbar_Scroll;

            this._verticalScrollbar = base.GetTemplateChild("VerticalScrollBar") as ScrollBar;
            if (this._verticalScrollbar != null)
            {
                this._verticalScrollbar.Scroll += new ScrollEventHandler(VerticalScrollbar_Scroll);
                this._verticalScrollbar.Orientation = Orientation.Vertical;
            }

            if (this._horizontalScrollbar != null)
                this._horizontalScrollbar.Scroll -= HorizontalScrollbar_Scroll;

            this._horizontalScrollbar = base.GetTemplateChild("HorizontalScrollBar") as ScrollBar;
            if (this._horizontalScrollbar != null)
            {
                this._horizontalScrollbar.Orientation = Orientation.Horizontal;
                this._horizontalScrollbar.Scroll += new ScrollEventHandler(HorizontalScrollbar_Scroll);
            }

            this._scrollHelper = new TouchScrollHelper(this._panel, this);
            this._scrollHelper.IsEnabled = this.IsTouchSupportEnabled;


            this._panel.SetCurrentValue(FrameworkElement.IsManipulationEnabledProperty, this.IsTouchSupportEnabled);

        }

        #endregion // OnApplyTemplate

        #region OnMouseEnter

        /// <summary>
        /// Called before the <see cref="UIElement.MouseEnter"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            this._mouseIsOver = true;
            base.OnMouseEnter(e);
            if (this._selectNodesTimer != null)
                this._selectNodesTimer.Stop();
        }

        #endregion // OnMouseEnter

        #region OnMouseLeave

        /// <summary>
        /// Called before the <see cref="UIElement.MouseLeave"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this._mouseIsOver = false;

            if (this._dragSelectType != TreeDragSelectType.None)
            {
                if (this.SelectionSettings.NodeSelection == TreeSelectionType.Multiple)
                {
                    if (this._selectNodesTimer == null)
                    {
                        this._selectNodesTimer = new DispatcherTimer();
                        this._selectNodesTimer.Tick += new EventHandler(SelectRowsCellsTimer_Tick);
                        this._selectNodesTimer.Interval = TimeSpan.FromMilliseconds(0);
                    }
                    this._selectNodesTimer.Start();
                }
            }
            base.OnMouseLeave(e);
        }

        #endregion // OnMouseLeave

        #region OnMouseWheel

        /// <summary>
        /// Called when the MouseWheel is raised on the <see cref="XamDataTree"/>
        /// </summary>
        /// <param propertyName="e">The data for the event.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            bool shouldScroll = this._mouseIsOver;



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


            if (shouldScroll)
            {
                bool handled = false;
                int multiplier = (e.Delta < 0) ? 1 : -1;

                if (this._verticalScrollbar != null && this._verticalScrollbar.Visibility == Visibility.Visible)
                {
                    handled = true;
                    this._verticalScrollbar.Value += this._verticalScrollbar.SmallChange * multiplier;
                }
                else if (this._horizontalScrollbar != null &&
                         this._horizontalScrollbar.Visibility == Visibility.Visible)
                {
                    handled = true;
                    this._horizontalScrollbar.Value += this._horizontalScrollbar.SmallChange * multiplier;
                }

                if (handled)
                {
                    this.InvalidateScrollPanel(false);
                    e.Handled = true;
                }
            }
        }

        #endregion // OnMouseWheel

        #region OnLostMouseCapture

        /// <summary>
        /// Called before the LostMouseCapture event is raised.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            if (this._dragSelectType == TreeDragSelectType.Node)
            {
                bool forceEndSelection = true;

                Point currentMousePoint = e.GetPosition(null);

                if (this._mouseDownPoint != null)
                {
                    Point p = (Point)this._mouseDownPoint;
                    if (currentMousePoint.X == p.X && currentMousePoint.Y == p.Y)
                    {
                        forceEndSelection = false;
                    }
                }

                this.EndSelectionDrag(forceEndSelection);
            }

            base.OnLostMouseCapture(e);
        }

        #endregion // OnLostMouseCapture
        
        #region OnMouseMove

        /// <summary>
        /// Called before the <see cref="UIElement.MouseMove"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);


            Point currentMousePoint = e.GetPosition(null);

            if (this._mouseDownPoint != null)
            {
                Point p = (Point)this._mouseDownPoint;
                if (currentMousePoint.X != p.X && currentMousePoint.Y != p.Y)
                {
                    this._mouseDownNode = null;
                }
            }




            if (!this._mouseIsOver && this._dragSelectType != TreeDragSelectType.None)
            {

                this._mousePosition = e.GetPosition(this);



            }

            XamDataTreeNodeControl node = this.GetNodeFromSource(e.OriginalSource as DependencyObject);

            // WPF only returns the control itself as the e.OriginalSource when the mouse is captured...
            // In this case, e.OrginalSource will always return the XamDataTree
            // So that means we need to look the element we're currently over, and try to resolve the node that way.
            bool hitTest = this.IsMouseCaptured;

            // If the mouse is captured by the nodeControl even clicking on the nodeline will cause e.OriginalSource to
            // be the whole XamDataTreeNodeControl. In this case we need more precise hitTesting.
            if (!hitTest && node != null && node.IsMouseCaptured)
            {
                node = null;
                hitTest = true;
            }

            if (hitTest)
            {
                HitTestResult result = VisualTreeHelper.HitTest(this, e.GetPosition(this));

                if (node == null)
                {
                    if (result != null && result.VisualHit != null)
                    {
                        node = this.GetNodeFromSource(result.VisualHit);
                    }
                }

                // WPF does not raise the MouseEnter and MouseLeave event when the mouse is capture... another awesome difference between the 2 
                // platforms. so we have to do the logic for DragScrolling in MouseMove instead.
                if (result == null)
                {
                    this._mouseIsOver = false;

                    // MouseLeave
                    if (this._dragSelectType != TreeDragSelectType.None)
                    {
                        if (this.SelectionSettings.NodeSelection == TreeSelectionType.Multiple)
                        {
                            if (this._selectNodesTimer == null)
                            {
                                this._selectNodesTimer = new DispatcherTimer();
                                this._selectNodesTimer.Tick += new EventHandler(SelectRowsCellsTimer_Tick);
                                this._selectNodesTimer.Interval = TimeSpan.FromMilliseconds(0);
                            }
                            this._selectNodesTimer.Start();
                        }
                    }
                }
                else
                {
                    this._mouseIsOver = true;

                    // MouseEnter
                    if (this._selectNodesTimer != null)
                        this._selectNodesTimer.Stop();
                }              
            }


            if (node != null)
            {
                if (node.Node != this._mouseOverNode)
                {
                    if (this._mouseOverNode != null)
                    {
                        this._mouseOverNode.IsMouseOver = false;
                        if (this._mouseOverNode.Control != null)
                            this._mouseOverNode.Control.InternalCellMouseLeave(this._mouseOverNode);
                    }
                    if (node.Node != null)
                    {
                        this._mouseOverNode = node.Node;

                        this._mouseOverNode.IsMouseOver = true;
                        if (this._mouseOverNode.Control != null)
                            this._mouseOverNode.Control.InternalCellMouseEnter(this._mouseOverNode);
                    }
                }
                if (this._dragSelectType != TreeDragSelectType.None && node.Node != null && node.Node.IsDraggableResolved && !this.IsNodeDragging)
                    node.Node.OnNodeDragging(this._dragSelectType);

                if (node.Node != null)
                    node.Node.OnNodeMouseMove(e);
            }
            else
            {
                if (this._mouseOverNode != null)
                {
                    this._mouseOverNode.IsMouseOver = false;
                    if (this._mouseOverNode.Control != null)
                        this._mouseOverNode.Control.InternalCellMouseLeave(this._mouseOverNode);
                    this._mouseOverNode = null;
                }
            }
            
            if (this.IsNodeDragging)
            {

                this._mousePosition = e.GetPosition(this);                



                bool insideTree = false;


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

                insideTree = this.IsMouseCaptureWithin && !this.IsMouseOver;

                if (!insideTree && DragDropManager.IsDragging)
                {
                    if (this._scrollNodesTimer == null)
                    {
                        this._scrollNodesTimer = new DispatcherTimer();
                        this._scrollNodesTimer.Tick += new EventHandler(ScrollNodesTimer_Tick);
                        this._scrollNodesTimer.Interval = TimeSpan.FromMilliseconds(0);
                    }
                    this._scrollNodesTimer.Start();
                }
                else
                {
                    if (this._scrollNodesTimer != null)
                    {
                        this._scrollNodesTimer.Stop();
                    }
                }
            }
            else
            {
                if (this._scrollNodesTimer != null)
                {
                    this._scrollNodesTimer.Stop();
                }
            }
        }        
        
        #endregion // OnMouseMove

        #region OnGotFocus
        /// <summary>
        /// Called before the <see cref="UIElement.GotFocus"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (!this._skipDefaultActivation)
                this.SetDefaultActiveNode();
        }

        #endregion // OnGotFocus

        #region OnMouseLeftButtonDown
        /// <summary>
        /// Called before the System.Windows.UIElement.MouseLeftButtonDown event occurs.
        /// </summary>
        /// <param name="e">The data for the event. </param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Border bd = e.OriginalSource as Border;
            if (bd != null && bd.Name == "Bd")
            {
                if (this.CurrentEditNode != null)
                {                    
                    if (!this.ExitEditMode(false))
                    {
                        _lastCancelledExitEditEvent = DateTime.Now;
                    }
                }
            }           
            base.OnMouseLeftButtonDown(e);
        }
        #endregion // OnMouseLeftButtonDown

        #endregion // Overrides

        #region Properties

        #region Public

        #region ItemsSource

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(XamDataTree), new PropertyMetadata(new PropertyChangedCallback(ItemsSourceChanged)));

        /// <summary>
        /// Gets/sets the <see cref="IEnumerable"/> for the <see cref="XamDataTree"/>.
        /// </summary>
        /// <remarks>
        /// This property won't take effect until the XamDataTree has Loaded.
        /// </remarks>
        public IEnumerable ItemsSource
        {
            get { return this._nodesManager.ItemsSource; }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree tree = (XamDataTree)obj;

            if (tree.IsLoaded)
            {
                tree.ApplyItemSource((IEnumerable)e.NewValue);
            }
        }

        #endregion // ItemsSource

        #region MaxDepth

        /// <summary>
        /// Identifies the <see cref="MaxDepth"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MaxDepthProperty = DependencyProperty.Register("MaxDepth", typeof(int), typeof(XamDataTree), new PropertyMetadata(int.MaxValue, new PropertyChangedCallback(MaxDepthChanged)));

        /// <summary>
        /// Gets/sets the maximum allowed depth of the hierarchy of the <see cref="XamDataTree"/>.
        /// This property is useful for when the data source provided as an infinite recursion.
        /// </summary>
        /// <remarks>
        /// This property is essentially zero based, as the root level would be considered a MaxDepth of 0. 
        /// For example: setting this property to 1, would mean that you would only have 1 level other than the root level. 
        /// </remarks>
        public int MaxDepth
        {
            get { return (int)this.GetValue(MaxDepthProperty); }
            set { this.SetValue(MaxDepthProperty, value); }
        }

        private static void MaxDepthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // MaxDepth

        #region GlobalNodeLayouts

        /// <summary>
        /// Gets a Collection of <see cref="NodeLayout"/> objects that will be used throughout all Levels of the <see cref="XamDataTree"/>
        /// </summary>
        public GlobalNodeLayoutCollection GlobalNodeLayouts
        {
            get
            {
                if (this._globalNodeLayouts == null)
                {
                    this._globalNodeLayouts = new GlobalNodeLayoutCollection(this);
                    this._globalNodeLayouts.CollectionChanged += NodeLayouts_CollectionChanged;
                }
                return this._globalNodeLayouts;
            }
        }

        #endregion // GlobalNodeLayouts

        #region NodeLayouts

        /// <summary>
        /// Gets a Collection of <see cref="NodeLayout"/> objects that will be used only for the root level of the <see cref="XamDataTree"/>
        /// </summary>
        public NodeLayoutCollection NodeLayouts
        {
            get
            {
                return this._nodesManager.NodeLayout.NodeLayouts;
            }
        }

        #endregion // NodeLayouts

        #region DisplayMemberPath

        /// <summary>
        /// Identifies the <see cref="DisplayMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(XamDataTree), new PropertyMetadata("", new PropertyChangedCallback(DisplayMemberPathChanged)));

        /// <summary>
        /// Gets / sets the path to the property on the <see cref="XamDataTreeNode.Data"/> object to populate the text of the <see cref="XamDataTreeNode"/>.
        /// </summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        private static void DisplayMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree tree = (XamDataTree)obj;
            tree.InvalidateScrollPanel(true, false, true);
        }

        #endregion // DisplayMemberPath

        #region CheckBoxMemberPath

        /// <summary>
        /// Identifies the <see cref="CheckBoxMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxMemberPathProperty = DependencyProperty.Register("CheckBoxMemberPath", typeof(string), typeof(XamDataTree), new PropertyMetadata("", new PropertyChangedCallback(CheckBoxMemberPathChanged)));

        /// <summary>
        /// Gets / sets the path to the property on the <see cref="XamDataTreeNode.Data"/> object to populate the <see cref="System.Windows.Controls.CheckBox"/> of the <see cref="XamDataTreeNode"/>.
        /// </summary>
        public string CheckBoxMemberPath
        {
            get { return (string)this.GetValue(CheckBoxMemberPathProperty); }
            set { this.SetValue(CheckBoxMemberPathProperty, value); }
        }

        private static void CheckBoxMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("CheckBoxMemberPath");
            ctrl.OnPropertyChanged("CheckBoxMemberPathResolved");
        }

        #endregion // CheckBoxMemberPath

		// MD 8/11/11 - XamFormulaEditor
		#region IsEnabledMemberPath

		/// <summary>
		/// Identifies the <see cref="IsEnabledMemberPath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsEnabledMemberPathProperty = DependencyPropertyUtilities.Register("IsEnabledMemberPath",
			typeof(string), typeof(XamDataTree),
			DependencyPropertyUtilities.CreateMetadata("", new PropertyChangedCallback(OnIsEnabledMemberPathChanged))
			);

		private static void OnIsEnabledMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDataTree ctrl = (XamDataTree)d;
			ctrl.OnPropertyChanged("IsEnabledMemberPath");
			ctrl.OnPropertyChanged("IsEnabledMemberPathResolved");
		}

		/// <summary>
		/// Gets / sets the path to the property on the <see cref="XamDataTreeNode.Data"/> object to populate the <see cref="XamDataTreeNode.IsEnabled"/> of the <see cref="XamDataTreeNode"/>.
		/// </summary>
		/// <seealso cref="IsEnabledMemberPathProperty"/>
		public string IsEnabledMemberPath
		{
			get { return (string)this.GetValue(XamDataTree.IsEnabledMemberPathProperty); }
			set { this.SetValue(XamDataTree.IsEnabledMemberPathProperty, value); }
		}

		#endregion //IsEnabledMemberPath

        #region IsExpandedMemberPath

        /// <summary>
        /// Identifies the <see cref="IsExpandedMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsExpandedMemberPathProperty = DependencyProperty.Register("IsExpandedMemberPath", typeof(string), typeof(XamDataTree), new PropertyMetadata("", new PropertyChangedCallback(IsExpandedMemberPathChanged)));

        /// <summary>
        /// Gets / sets the path to the property on the <see cref="XamDataTreeNode.Data"/> object to populate the <see cref="XamDataTreeNode.IsExpanded"/> of the <see cref="XamDataTreeNode"/>.
        /// </summary>
        public string IsExpandedMemberPath
        {
            get { return (string)this.GetValue(IsExpandedMemberPathProperty); }
            set { this.SetValue(IsExpandedMemberPathProperty, value); }
        }

        private static void IsExpandedMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("IsExpandedMemberPath");
            ctrl.OnPropertyChanged("IsExpandedMemberPathResolved");
        }

        #endregion // IsExpandedMemberPath

        #region Nodes

        /// <summary>
        /// Gets the nodes on the root level of the <see cref="XamDataTree"/>.
        /// </summary>
        [Browsable(false)]
        public XamDataTreeNodesCollection Nodes
        {
            get
            {
                return ((XamDataTreeNodesCollection)this._nodesManager.Nodes);
            }
        }

        #endregion // Nodes

        #region Indentation

        /// <summary>
        /// Identifies the <see cref="Indentation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation", typeof(double), typeof(XamDataTree), new PropertyMetadata(21.0, new PropertyChangedCallback(IndentationChanged)));

        /// <summary>
        /// Gets / sets how much each <see cref="XamDataTreeNode"/> is indented from it's parent.
        /// </summary>
        public double Indentation
        {
            get { return (double)this.GetValue(IndentationProperty); }
            set { this.SetValue(IndentationProperty, value); }
        }

        private static void IndentationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree tree = (XamDataTree)obj;
            tree.OnPropertyChanged("Indentation");
        }

        #endregion // Indentation

        #region ItemTemplate

        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(XamDataTree), new PropertyMetadata(new PropertyChangedCallback(ItemTemplateChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="DataTemplate"/> that will be used to create the VisualTree for every <see cref="XamDataTreeNode"/> for every <see cref="NodeLayout"/> in the <see cref="XamDataTree"/>
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }

        private static void ItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree tree = (XamDataTree)obj;
            tree.OnPropertyChanged("ItemTemplate");
        }

        #endregion // ItemTemplate

        #region ActiveNode

        /// <summary>
        /// Gets / sets the <see cref="XamDataTreeNode"/> which is considered active by the <see cref="XamDataTree"/>.  This
        /// node will respond to keyboard input.
        /// </summary>
        [Browsable(false)]
        public XamDataTreeNode ActiveNode
        {
            get
            {
                return this._activeNode;
            }
            set
            {
                this.SetActiveNode(value, TreeInvokeAction.Code, true);
            }
        }

        #endregion // ActiveNode

        #region CurrentEditNode
        /// <summary>
        /// Gets/sets the <see cref="XamDataTreeNode"/> that is currently in edit mode.
        /// </summary>
        protected internal XamDataTreeNode CurrentEditNode
        {
            get;
            set;
        }
        #endregion // CurrentEditNode

        #region ShowActiveNodeIndicator

        /// <summary>
        /// Identifies the <see cref="ShowActiveNodeIndicator"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ShowActiveNodeIndicatorProperty = DependencyProperty.Register("ShowActiveNodeIndicator", typeof(bool), typeof(XamDataTree), new PropertyMetadata(true, new PropertyChangedCallback(ShowActiveNodeIndicatorChanged)));

        /// <summary>
        /// Gets / sets if the <see cref="XamDataTree"/> will show an indictator around the currently active <see cref="XamDataTreeNode"/>.
        /// </summary>
        public bool ShowActiveNodeIndicator
        {
            get { return (bool)this.GetValue(ShowActiveNodeIndicatorProperty); }
            set { this.SetValue(ShowActiveNodeIndicatorProperty, value); }
        }

        private static void ShowActiveNodeIndicatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree tree = (XamDataTree)obj;
            tree.OnPropertyChanged("ShowActiveNodeIndicator");
        }

        #endregion // ShowActiveNodeIndicator

        #region SelectionSettings

        /// <summary>
        /// Gets a reference to the <see cref="TreeSelectionSettings"/> object that controls all the properties for selection on all <see cref="NodeLayout"/>s of a <see cref="XamDataTree"/>.
        /// </summary>
        public TreeSelectionSettings SelectionSettings
        {
            get
            {
                if (this._selectionSettings == null)
                    this._selectionSettings = new TreeSelectionSettings();

                this._selectionSettings.Tree = this;

                return this._selectionSettings;
            }
            set
            {
                if (value != this._selectionSettings)
                {
                    this._selectionSettings = value;
                    this.OnPropertyChanged("SelectionSettings");
                }
            }
        }

        #endregion // Public

        #region ActiveNodeIndicatorStyle

        /// <summary>
        /// Identifies the <see cref="ActiveNodeIndicatorStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ActiveNodeIndicatorStyleProperty = DependencyProperty.Register("ActiveNodeIndicatorStyle", typeof(Style), typeof(XamDataTree), new PropertyMetadata(new PropertyChangedCallback(ActiveNodeIndicatorStyleChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Style"/> that will be applied to the <see cref="ActiveNodeIndicator"/> which will be visible on the <see cref="XamDataTree.ActiveNode"/>.
        /// </summary>
        public Style ActiveNodeIndicatorStyle
        {
            get { return (Style)this.GetValue(ActiveNodeIndicatorStyleProperty); }
            set { this.SetValue(ActiveNodeIndicatorStyleProperty, value); }
        }

        private static void ActiveNodeIndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree tree = (XamDataTree)obj;
            tree.OnPropertyChanged("ActiveNodeIndicatorStyle");
            tree.ResetPanelNodes(true);
            tree.InvalidateScrollPanel(false);
        }

        #endregion // ActiveNodeIndicatorStyle

        #region NodeStyle

        /// <summary>
        /// Identifies the <see cref="NodeStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NodeStyleProperty = DependencyProperty.Register("NodeStyle", typeof(Style), typeof(XamDataTree), new PropertyMetadata(new PropertyChangedCallback(NodeStyleChanged)));

        /// <summary>
        /// Get / set the <see cref="Style"/> which will be applied to all <see cref="XamDataTreeNode"/>s of the <see cref="XamDataTree"/>.
        /// </summary>
        public Style NodeStyle
        {
            get { return (Style)this.GetValue(NodeStyleProperty); }
            set { this.SetValue(NodeStyleProperty, value); }
        }

        private static void NodeStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("NodeStyle");
        }

        #endregion // NodeStyle

        #region EditingSettings

        /// <summary>
        /// Gets a reference to the <see cref="TreeEditingSettings"/> object that controls all the properties for editing on all <see cref="NodeLayout"/>s of a <see cref="XamDataTree"/>.
        /// </summary>
        public TreeEditingSettings EditingSettings
        {
            get
            {
                if (this._editingSettings == null)
                {
                    this._editingSettings = new TreeEditingSettings();
                }

                this._editingSettings.Tree = this;

                return this._editingSettings;
            }
            set
            {
                if (value != this._editingSettings)
                {
                    this._editingSettings = value;
                    this.OnPropertyChanged("EditingSettings");
                }
            }
        }

        #endregion // EditingSettings

        #region CheckBoxSettings

        /// <summary>
        /// Gets a reference to the <see cref="CheckBoxSettings"/> object that controls all the properties for <see cref="CheckBox"/>s on all <see cref="NodeLayout"/>s of a <see cref="XamDataTree"/>.
        /// </summary>
        public CheckBoxSettings CheckBoxSettings
        {
            get
            {
                if (this._checkBoxSettings == null)
                {
                    this._checkBoxSettings = new CheckBoxSettings();
                    this._checkBoxSettings.PropertyChanged += CheckBoxSettings_PropertyChanged;
                }

                this._checkBoxSettings.Tree = this;

                return this._checkBoxSettings;
            }
            set
            {
                if (value != this._checkBoxSettings)
                {
                    if (this._checkBoxSettings != null)
                    {
                        this._checkBoxSettings.PropertyChanged -= CheckBoxSettings_PropertyChanged;
                    }

                    this._checkBoxSettings = value;

                    if (this._checkBoxSettings != null)
                    {
                        this._checkBoxSettings.PropertyChanged += CheckBoxSettings_PropertyChanged;
                    }

                    this.OnPropertyChanged("CheckBoxSettings");
                }
            }
        }

        private void CheckBoxSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        #endregion // CheckBoxSettings

        #region IsDraggable

        /// <summary>
        /// Identifies the <see cref="IsDraggable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.Register("IsDraggable", typeof(bool), typeof(XamDataTree), new PropertyMetadata(false, new PropertyChangedCallback(IsDraggableChanged)));

        /// <summary>
        /// Gets / sets if the <see cref="XamDataTree"/> can participate in drag and drop.  This property controls if the <see cref="XamDataTree"/> will allow for dragging to begin on it.
        /// </summary>
        public bool IsDraggable
        {
            get { return (bool)this.GetValue(IsDraggableProperty); }
            set { this.SetValue(IsDraggableProperty, value); }
        }

        private static void IsDraggableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("IsDraggable");
        }

        #endregion // IsDraggable

        #region IsDropTarget

        /// <summary>
        /// Identifies the <see cref="IsDropTarget"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.Register("IsDropTarget", typeof(bool), typeof(XamDataTree), new PropertyMetadata(false, new PropertyChangedCallback(IsDropTargetChanged)));

        /// <summary>
        /// Gets / sets if the <see cref="XamDataTree"/> can participate in drag and drop.  This property controls if the <see cref="XamDataTree"/> will allow for dragging to end on it.
        /// </summary>
        public bool IsDropTarget
        {
            get { return (bool)this.GetValue(IsDropTargetProperty); }
            set { this.SetValue(IsDropTargetProperty, value); }
        }

        private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("IsDropTarget");
        }

        #endregion // IsDropTarget

        #region NodeLineVisibility

        /// <summary>
        /// Identifies the <see cref="NodeLineVisibility"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NodeLineVisibilityProperty = DependencyProperty.Register("NodeLineVisibility", typeof(Visibility), typeof(XamDataTree), new PropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(NodeLineVisibilityChanged)));

        /// <summary>
        /// Gets / sets if NodeLines will be visible on the <see cref="XamDataTree"/>.
        /// </summary>
        public Visibility NodeLineVisibility
        {
            get { return (Visibility)this.GetValue(NodeLineVisibilityProperty); }
            set { this.SetValue(NodeLineVisibilityProperty, value); }
        }

        private static void NodeLineVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("NodeLineVisibility");
            if (ctrl.IsLoaded)
                ctrl.InvalidateScrollPanel(true, true);
        }

        #endregion // NodeLineVisibility

        #region ExpandedIconTemplate

        /// <summary>
        /// Identifies the <see cref="ExpandedIconTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ExpandedIconTemplateProperty = DependencyProperty.Register("ExpandedIconTemplate", typeof(DataTemplate), typeof(XamDataTree), new PropertyMetadata(new PropertyChangedCallback(ExpandedIconTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> that will be displayed on nodes that are currently expanded on all <see cref="NodeLayout"/>s of the <see cref="XamDataTree"/>.
        /// </summary>
        public DataTemplate ExpandedIconTemplate
        {
            get { return (DataTemplate)this.GetValue(ExpandedIconTemplateProperty); }
            set { this.SetValue(ExpandedIconTemplateProperty, value); }
        }

        private static void ExpandedIconTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("ExpandedIconTemplate");
        }

        #endregion // ExpandedIconTemplate

        #region CollapsedIconTemplate

        /// <summary>
        /// Identifies the <see cref="CollapsedIconTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CollapsedIconTemplateProperty = DependencyProperty.Register("CollapsedIconTemplate", typeof(DataTemplate), typeof(XamDataTree), new PropertyMetadata(new PropertyChangedCallback(CollapsedIconTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> that will be displayed on nodes that are currently collapsed on all <see cref="NodeLayout"/>s of the <see cref="XamDataTree"/>.
        /// </summary>
        public DataTemplate CollapsedIconTemplate
        {
            get { return (DataTemplate)this.GetValue(CollapsedIconTemplateProperty); }
            set { this.SetValue(CollapsedIconTemplateProperty, value); }
        }

        private static void CollapsedIconTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("CollapsedIconTemplate");
        }

        #endregion // CollapsedIconTemplate

        #region EnsureNodeExpansion

        /// <summary>
        /// Identifies the <see cref="EnsureNodeExpansion"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EnsureNodeExpansionProperty = DependencyProperty.Register("EnsureNodeExpansion", typeof(bool), typeof(XamDataTree), new PropertyMetadata(false, new PropertyChangedCallback(EnsureNodeExpansionChanged)));

        /// <summary>
        /// Gets / sets if the <see cref="XamDataTree"/> should process all nodes in the tree and register child nodes accordingly.
        /// </summary>
        /// <remarks>
        /// This flag is designed to be used via Xaml.
        /// </remarks>
        public bool EnsureNodeExpansion
        {
            get { return (bool)this.GetValue(EnsureNodeExpansionProperty); }
            set { this.SetValue(EnsureNodeExpansionProperty, value); }
        }

        private static void EnsureNodeExpansionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree ctrl = (XamDataTree)obj;
            ctrl.OnPropertyChanged("EnsureNodeExpansion");
            if ((bool)e.NewValue)
            {
                ctrl.NodesManager.EnsureExpandedChildNodesAreRegistered();
            }
        }

        #endregion // EnsureNodeExpansion 
			
        #region AllowDragDropCopy

        /// <summary>
        /// Identifies the <see cref="AllowDragDropCopy"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowDragDropCopyProperty = DependencyProperty.Register("AllowDragDropCopy", typeof(bool), typeof(XamDataTree), new PropertyMetadata(true));

        /// <summary>
        /// Gets / sets if holding the control key will designate the drag action as a copy rather then a move action.  
        /// </summary>
        /// <remarks>
        /// This controls only the visual cue that the control will display when dragging and dropping.  If true and the Control key is held, then the cue will
        /// be for a copy action and the developer will need to implement the copy action.  If false then holding the control will not designate this as a copy action but 
        /// rather a move action.
        /// </remarks>
        public bool AllowDragDropCopy
        {
            get { return (bool)this.GetValue(AllowDragDropCopyProperty); }
            set { this.SetValue(AllowDragDropCopyProperty, value); }
        }

        #endregion // AllowDragDropCopy 
          
        #region IsTouchSupportEnabled

        /// <summary>
        /// Identifies the <see cref="IsTouchSupportEnabled"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsTouchSupportEnabledProperty = DependencyPropertyUtilities.Register("IsTouchSupportEnabled",
               typeof(bool), typeof(XamDataTree),
               DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsTouchSupportEnabledChanged))
               );

        private static void OnIsTouchSupportEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamDataTree instance = (XamDataTree)d;

            if (instance._scrollHelper != null)
                instance._scrollHelper.IsEnabled = (bool)e.NewValue;


			if ( instance._panel != null )
				instance._panel.SetCurrentValue(FrameworkElement.IsManipulationEnabledProperty, KnownBoxes.FromValue((bool)e.NewValue));

        }

        /// <summary>
        /// Returns or sets whether touch support is enabled for this control
        /// </summary>
        /// <seealso cref="IsTouchSupportEnabledProperty"/>
        public bool IsTouchSupportEnabled
        {
            get
            {
                return (bool)this.GetValue(XamDataTree.IsTouchSupportEnabledProperty);
            }
            set
            {
                this.SetValue(XamDataTree.IsTouchSupportEnabledProperty, value);
            }
        }

        #endregion //IsTouchSupportEnabled

        #endregion // Public

        #region Protected

        #region VerticalScrollBar

        /// <summary>
        /// Gets a reference to the vertical scrollbar that is attached to the <see cref="XamDataTree"/>.
        /// If a vertical scrollbar was not specified, this property will return null.
        /// </summary>
        protected internal ScrollBar VerticalScrollBar
        {
            get { return this._verticalScrollbar; }
        }
        #endregion // VerticalScrollBar

        #region HorizontalScrollBar

        /// <summary>
        /// Gets a reference to the horizontal scrollbar that is attached to the <see cref="XamDataTree"/>.
        /// If a horizontal scrollbar was not specified, this property will return null.
        /// </summary>
        protected internal ScrollBar HorizontalScrollBar
        {
            get { return this._horizontalScrollbar; }
        }
        #endregion // HorizontalScrollBar

        #region NodesManager
        /// <summary>
        /// Gets a reference to the NodesManager that is used by the <see cref="XamDataTree"/>.
        /// </summary>
        protected internal XamDataTreeNodesManager NodesManager
        {
            get
            {
                return this._nodesManager;
            }
        }
        #endregion // NodesManager

        #region Panel
        /// <summary>
        /// The visual container that holds the nodes.
        /// </summary>
        protected internal NodesPanel Panel
        {
            get
            {
                return this._panel;
            }
        }
        #endregion // Panel

        #region IsNodeDragging

        /// <summary>
        /// Gets / sets if a <see cref="XamDataTreeNode"/> is currently being dragged in <see cref="XamDataTree"/>.
        /// </summary>
        protected internal bool IsNodeDragging
        {
            get
            {
                return this.CurrentDraggingNode != null;
            }
        }

        #endregion // IsNodeDragging

        #endregion // Protected

        #region Internal

        internal InternalNodesCollection InternalNodes
        {
            get
            {
                if (this._internalNodes == null)
                    this._internalNodes = new InternalNodesCollection() { RootNodesManager = this._nodesManager };
                return this._internalNodes;
            }
        }

        internal bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }
            set
            {

                _isLoaded = value;
            }
        }

        internal Dictionary<string, object> EditNodeValues
        {
            get { return this._editCellValues; }
        }

        #region CurrentDraggingNode
        /// <summary>
        /// Gets / sets the <see cref="XamDataTreeNode"/> which is currently being dragged.
        /// </summary>
        internal XamDataTreeNode CurrentDraggingNode { get; set; }
        #endregion // CurrentDraggingNode

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Public

        #region EnterEditMode

        /// <summary>
        /// Method which puts the <see cref="XamDataTree"/> into edit mode.
        /// </summary>
        /// <param name="node"></param>        
        public void EnterEditMode(XamDataTreeNode node)
        {
            this.EnterEditModeInternal(node);
        }

        #endregion // EnterEditMode

        #region ExitEditMode

        /// <summary>
        /// Method which ends editing on the <see cref="XamDataTree"/>
        /// </summary>
        /// <param name="cancel">true if editing should be cancelled</param>
        /// <returns>False if the <see cref="NodeExitingEditMode"/> event is cancelled, stopping the exiting of edit mode.</returns>
        public bool ExitEditMode(bool cancel)
        {
            return this.ExitEditModeInternal(cancel);
        }

        #endregion // ExitEditMode

        #endregion // Public

        #region Protected

        #region InvalidateScrollPanel

        /// <summary>
        /// Invalidates the content of the <see cref="NodesPanel"/>.
        /// </summary>
        /// <param propertyName="reset">True if the internal scroll information should be invalidated.</param>
        protected internal void InvalidateScrollPanel(bool reset)
        {
            this.InvalidateScrollPanel(reset, false, false);
        }

        /// <summary>
        /// Invalidates the content of the <see cref="NodesPanel"/>.
        /// </summary>
        /// <param propertyName="reset">True if the internal scroll information should be invalidated.</param>
        protected internal void InvalidateScrollPanel(bool reset, bool resetVisibleNodes)
        {
            this.InvalidateScrollPanel(reset, false, resetVisibleNodes);
        }

        /// <summary>
        /// Invalidates the content of the <see cref="NodesPanel"/>.
        /// </summary>
        /// <param name="reset">True if the internal scroll information should be invalidated.</param>
        /// <param name="resetScrollPosition">True if the grid should return to a zero horizontal and zero vertical position.</param>
        /// <param name="resetVisibleNodes">True if the visible nodes should be reset.</param>        
        protected internal void InvalidateScrollPanel(bool reset, bool resetScrollPosition, bool resetVisibleNodes)
        {
            if (this._panel != null)
            {
                this._panel.InvalidateMeasure();

                if (reset)
                    this._panel.ResetCachedScrollInfo(resetVisibleNodes);

                if (resetScrollPosition)
                {
                    if (this._horizontalScrollbar != null)
                    {
                        this._horizontalScrollbar.Value = 0;
                    }
                    if (this._verticalScrollbar != null)
                    {
                        this._verticalScrollbar.Value = 0;
                    }
                }
            }
        }

        #endregion // InvalidateScrollPanel

        #region ResetPanelNodes

        /// <summary>
        /// Tells the panel that it should recycle all of it's nodes, so that i can be completely reloaded. 
        /// </summary>
        protected internal void ResetPanelNodes()
        {
            this.ResetPanelNodes(false);
        }

        /// <summary>
        /// Tells the panel that it should recycle all of it's nodes, so that i can be completely reloaded. 
        /// </summary>
        /// <param name="releaseNodesPanels">True if the NodesPanel should be released by the RecyclingManager.</param>
        protected internal void ResetPanelNodes(bool releaseNodesPanels)
        {
            if (this.Panel != null)
                this.Panel.ResetNodes(releaseNodesPanels);
        }

        #endregion // ResetPanelNodes

        #region SetActiveNode

        /// <summary>
        /// Method that will change the <see cref="XamDataTreeNode"/> which is currently active in the <see cref="XamDataTree"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="action"></param>
        protected internal virtual void SetActiveNode(XamDataTreeNode node, TreeInvokeAction action)
        {
            this.SetActiveNode(node, action, false);
        }

        /// <summary>
        /// Method that will change the <see cref="XamDataTreeNode"/> which is currently active in the <see cref="XamDataTree"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="action"></param>
        /// <param name="scrollToView"></param>
        protected internal virtual void SetActiveNode(XamDataTreeNode node, TreeInvokeAction action, bool scrollToView)
        {
            if (node == null && this.ActiveNode == null)
                return;

            if (node != null && !node.IsEnabled)
                return;

            

            if (this._activeNode != node)
            {
                // so first we need to deactivate the current active node if it exists       
                ActiveNodeChangingEventArgs args = new ActiveNodeChangingEventArgs() { OriginalActiveTreeNode = this.ActiveNode, NewActiveTreeNode = node };
                
                ActiveNodeChangedEventArgs afterArgs = new ActiveNodeChangedEventArgs() { OriginalActiveTreeNode = this.ActiveNode, NewActiveTreeNode = node };
                
                this.OnActiveNodeChanging(args);

                if (!args.Cancel)
                {
                    XamDataTreeNode prevNode = this._activeNode;

                    this._activeNode = node;

                    if (prevNode != null)
                    {
                        if (prevNode.IsEditing)
                            prevNode.NodeLayout.Tree.ExitEditMode(false);

                        prevNode.EnsureCurrentState();

                        prevNode.RaiseIsActiveChanged();
                    }

                    if (this._activeNode != null)
                    {
                        this._activeNode.EnsureCurrentState();

                        if (scrollToView)
                        {
                            this.ScrollNodeIntoView(node);
                        }

                        TreeEditingSettingsOverride settings = this._activeNode.NodeLayout.EditingSettings;
                        if (settings.AllowEditingResolved && settings.IsOnNodeActiveEditingEnabledResolved)
                        {
                            this.EnterEditModeInternal(this._activeNode);
                        }

                        this._activeNode.EnsureCurrentState();

                        this._activeNode.RaiseIsActiveChanged();
                    }

                    this.OnActiveNodeChanged(afterArgs);
                }
            }

            bool focus = false;
            if (node != null && node.Control != null)
            {

                DependencyObject currentFocusElem = PlatformProxy.GetFocusedElement(this) as DependencyObject;
                if (currentFocusElem != null)
                {
                    while (focus == false && currentFocusElem != null)
                    {
                        if (currentFocusElem == node.Control)
                            focus = true;
                        else
                            currentFocusElem = PlatformProxy.GetParent(currentFocusElem);
                    }
                }

                if (!focus)
                {
                    Control elem = node.Control.Content as Control;
                    if (elem != null && elem.IsHitTestVisible)
                        focus = elem.Focus();
                }

                if (!focus)
                {






                                this.Dispatcher.BeginInvoke(new XamDataTreeUtilities.MethodInvoker(this.FocusActiveNode), null);

                }
            }
        }

        #endregion // SetActiveNode

        #region EnterEditModeInternal

        /// <summary>
        /// Method which puts the <see cref="XamDataTree"/> into edit mode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected internal virtual bool EnterEditModeInternal(XamDataTreeNode node)
        {
            if (node != null && !this._enteringEditMode)
            {
                if (node.IsHeader)
                    return false;

                if (!node.IsEditing)
                {
                    // If the DataManager says it doesn't support editing, then don't go into edit mode. 
                    NodesManager manager = node.Manager as NodesManager;
                    if (manager != null)
                    {
                        if (!manager.DataManager.SupportsEditing)
                            return false;
                    }

                    this._enteringEditMode = true;

                    if (this.ExitEditModeInternal(false))
                    {
                        if (string.IsNullOrEmpty(node.NodeLayout.DisplayMemberPathResolved) && node.NodeLayout.EditorTemplate == null)
                        {
                            this._enteringEditMode = false;
                            return false;
                        }

                        if (!this.OnNodeEnteringEditMode(node))
                        {
                            IEditableObject obj = node.Data as IEditableObject;
                            if (obj != null)
                            {
                                obj.BeginEdit();
                            }

                            this.EditNodeValues.Clear();

                            this._originalNodeValues.Clear();

                            this.ActiveNode = node;

                            if (node.Control != null && !node.Control.IsEnabled)
                            {
                                this._enteringEditMode = false;
                                return false;
                            }
                            object val = node.Value;

                            this.EditNodeValues.Add(node.NodeLayout.DisplayMemberPathResolved, val);

                            this._originalNodeValues.Add(node.NodeLayout.DisplayMemberPathResolved, val);

                            if (node.Control == null)
                            {
                                this.Panel.RenderNode(node, double.PositiveInfinity);
                                this.Panel.ScrollNodeIntoView(node);
                            }

                            node.EnterEditMode(true);

                            this.OnNodeEnteredEditMode(node, node.Control.Content as FrameworkElement);

                            this.AttachMouseDownToRootVis();

                            this.InvalidateScrollPanel(false);

                            this._enteringEditMode = false;

                            return true;
                        }
                    }
                    this._enteringEditMode = false;
                }
            }
            return false;
        }

        #endregion // EnterEditModeInternal

        #region ExitEditModeInternal
        /// <summary>
        /// Method which takes the <see cref="XamDataTree"/> out of edit mode.
        /// </summary>
        /// <param name="cancel"></param>
        /// <returns></returns>
        protected internal virtual bool ExitEditModeInternal(bool cancel)
        {
            if (this.CurrentEditNode != null)
            {
                XamDataTreeNode node = this.CurrentEditNode;

                FrameworkElement editor = null;

                if (node.Control != null)
                {
                    editor = node.Control.Content as FrameworkElement;
                }
                else
                {
                    cancel = true;
                }

                this.EditNodeValues[node.NodeLayout.DisplayMemberPathResolved] = node.ResolveValueFromEditor();

                object newValue = this.EditNodeValues[node.NodeLayout.DisplayMemberPathResolved];

                if (cancel)
                {
                    newValue = this._originalNodeValues[node.NodeLayout.DisplayMemberPathResolved];
                }

                TreeExitEditingEventArgs eventArgs = new TreeExitEditingEventArgs() { Node = node, NewValue = newValue, EditingCanceled = cancel, Editor = editor };

                this.OnNodeExitEditingMode(eventArgs);

                if (!eventArgs.Cancel)
                {
                    this.CurrentEditNode = null;

                    if (!node.ExitEditMode(eventArgs.NewValue, cancel))
                    {
                        FrameworkElement rootVis2 = this._editorRootVisual;
                        if (rootVis2 == null)
                        {
                            rootVis2 = this._editorRootVisual = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                        }
                        if (rootVis2 != null)
                        {
                            rootVis2.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_NodeEditing_MouseLeftButtonDown));
                            rootVis2.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_NodeEditing_MouseLeftButtonDown), true);
                        }

                        this.CurrentEditNode = node;
                        this.ScrollNodeIntoView(node);
                        return false;
                    }

                    IEditableObject obj = node.Data as IEditableObject;
                    if (obj != null)
                    {
                        if (cancel)
                            obj.CancelEdit();
                        else
                            obj.EndEdit();
                    }

                    NodesManager manager = node.Manager as NodesManager;
                    if (manager != null)
                    {
                        if (cancel)
                            manager.DataManager.CancelEdit();
                        else
                            manager.DataManager.CommitEdit();
                    }

                    this.OnExitedEditMode(node);

                    FrameworkElement rootVis = this._editorRootVisual;
                    if (rootVis != null)
                    {
                        rootVis.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_NodeEditing_MouseLeftButtonDown));
                        this._editorRootVisual = null;
                    }

                    this.InvalidateScrollPanel(false);
                }
                else
                {
                    return false;
                }

            }
            return true;
        }

        #endregion // ExitEditModeInternal

        #region UnselectNode

        /// <summary>
        /// Internal method which will unselect a <see cref="XamDataTreeNode"/>
        /// </summary>
        /// <param name="xamDataTreeNode"></param>
        protected internal void UnselectNode(XamDataTreeNode xamDataTreeNode)
        {
            SelectedNodesCollection nodes = this.SelectionSettings.SelectedNodes;
            if (nodes.Contains(xamDataTreeNode))
            {
                nodes.Remove(xamDataTreeNode);
            }
        }

        #endregion // UnselectNode

        #region SelectNode

        /// <summary>
        /// Internal method which will select a <see cref="XamDataTreeNode"/>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="node"></param>
        protected internal bool SelectNode(XamDataTreeNode node, TreeInvokeAction action)
        {
            if (node == null || !node.IsEnabled)
                return true;

            SelectedNodesCollection previouslySelectedNodes = new SelectedNodesCollection();

            bool interrupt = false;

            TreeSelectionType selectionType = this.SelectionSettings.NodeSelection;

            previouslySelectedNodes.InternalAddRangeSilently(this.SelectionSettings.SelectedNodes);

            bool shiftKey = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) || (action == TreeInvokeAction.MouseMove);
            bool ctrlKey = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

            if (selectionType == TreeSelectionType.None && action == TreeInvokeAction.Code)
                selectionType = TreeSelectionType.Single;

            if (selectionType != TreeSelectionType.None)
            {
                SelectedNodesCollection selectedNodes = this.SelectionSettings.SelectedNodes;
                if (selectionType == TreeSelectionType.Single)
                {
                    selectedNodes.InternalResetItemsSilently();
                    selectedNodes.SelectItem(node, false);
                }
                else
                {
                    if (shiftKey && selectionType == TreeSelectionType.Multiple)
                    {
                        if (shiftKey)
                        {
                            if (selectedNodes.Count > 0)
                            {
                                XamDataTreeNode lastSelectedNode = selectedNodes.PivotItem;
                                if (lastSelectedNode == null)
                                    lastSelectedNode = selectedNodes[selectedNodes.Count - 1];

                                int indexOfLastSelectedRow = this.InternalNodes.IndexOf(lastSelectedNode);
                                int indexOfNewSelectedRow = this.InternalNodes.IndexOf(node);

                                if (selectedNodes.PivotItem != null)
                                {
                                    int count = selectedNodes.ShiftSelectedItems.Count;
                                    for (int i = count - 1; i >= 0; i--)
                                    {
                                        int index = selectedNodes.IndexOf(selectedNodes.ShiftSelectedItems[i]);
                                        selectedNodes.InternalRemoveItemSilently(index);
                                    }
                                }

                                if (indexOfLastSelectedRow < indexOfNewSelectedRow)
                                {
                                    for (int i = indexOfLastSelectedRow; i <= indexOfNewSelectedRow; i++)
                                    {
                                        XamDataTreeNode r = this.InternalNodes[i] as XamDataTreeNode;
                                        if (r != null)
                                            selectedNodes.SelectItem(r, true);
                                    }
                                }
                                else
                                {
                                    for (int i = indexOfLastSelectedRow; i >= indexOfNewSelectedRow; i--)
                                    {
                                        XamDataTreeNode r = this.InternalNodes[i] as XamDataTreeNode;
                                        if (r != null)
                                            selectedNodes.SelectItem(r, true);
                                    }
                                }
                            }
                            else
                                selectedNodes.SelectItem(node, true);
                        }
                        else
                        {
                            if (ctrlKey)
                            {
                                if (action == TreeInvokeAction.Click)
                                {
                                    if (selectedNodes.Contains(node))
                                    {
                                        int index = selectedNodes.IndexOf(node);
                                        selectedNodes.InternalRemoveItemSilently(index);
                                        interrupt = true;
                                    }
                                    else
                                        selectedNodes.SelectItem(node, false);
                                }
                            }
                            else
                            {
                                selectedNodes.InternalResetItemsSilently();
                                selectedNodes.SelectItem(node, false);
                            }
                        }
                    }
                    else
                    {
                        if (ctrlKey)
                        {

                            if (selectedNodes.Contains(node))
                            {
                                int index = selectedNodes.IndexOf(node);
                                selectedNodes.InternalRemoveItemSilently(index);
                                interrupt = true;
                            }
                            else
                                selectedNodes.SelectItem(node, false);
                        }
                        else
                        {
                            selectedNodes.InternalResetItemsSilently();
                            selectedNodes.SelectItem(node, false);
                        }
                    }
                }
                this.OnSelectedNodesCollectionChanged(previouslySelectedNodes, this.SelectionSettings.SelectedNodes);
                this.InvalidateScrollPanel(false);
            }

            return interrupt;
        }

        #endregion // SelectNode

        #endregion // Protected

        #region Private

        #region ApplyItemSource

        private void ApplyItemSource(IEnumerable itemSource)
        {
            if (this._nodesManager.ItemsSource != itemSource)
            {
                this._nodesManager.ItemsSource = itemSource;

                if (this.HorizontalScrollBar != null)
                {
                    this.HorizontalScrollBar.Value = 0;
                    this.HorizontalScrollBar.Visibility = Visibility.Collapsed;
                }

                if (this.VerticalScrollBar != null)
                {
                    this.VerticalScrollBar.Value = 0;
                    this.VerticalScrollBar.Visibility = Visibility.Collapsed;
                }
            }

            this.InvalidateScrollPanel(true);
        }
        #endregion // ApplyItemSource

        #region GetNodeFromSource

        internal XamDataTreeNodeControl GetNodeFromSource(DependencyObject obj)
        {
            return XamDataTree.GetNodeFromSource(obj, this);
        }

        internal static XamDataTreeNodeControl GetNodeFromSource(DependencyObject obj, XamDataTree sourceTree)
        {
            XamDataTreeNodeControl ccb = obj as XamDataTreeNodeControl;

            bool clickOnContentPresenter = ccb != null;

            while (obj != null && ccb == null)
            {
                DependencyObject parent = PlatformProxy.GetParent(obj);

                obj = parent;

                ccb = obj as XamDataTreeNodeControl;

                if (ccb != null && ccb.Node != null && ccb.Node.NodeLayout != null && ccb.Node.NodeLayout.Tree == sourceTree)
                {
                    break;
                }
                else
                {
                    ccb = null;
                    
                    ContentPresenter cp = obj as ContentPresenter;
                    if (cp != null)
                    {
                        clickOnContentPresenter = true;
                    }

                    if (obj is ExpansionIndicator)
                        break;

                    if (obj is NodeLineControl)
                        break;

                    if (obj is NodeLineTerminator)
                        break;

                    if (obj is NodeLinePanel)
                        break;

                    if (obj is ContentControl)
                    {
                        ContentControl cc = (ContentControl)obj;
                        if (cc.Name == "ExpandedIcon" || cc.Name == "CollapsedIcon")
                            break;
                    }
                }
            }

            return clickOnContentPresenter ? ccb : null;
        }

        #endregion // GetNodeFromSource

        #region EndSelectionDrag
        private void EndSelectionDrag(bool resetMouseDownCell)
        {
            this._dragSelectType = TreeDragSelectType.None;
            this.ReleaseMouseCapture();

            if (this._selectNodesTimer != null)
                this._selectNodesTimer.Stop();

            if (resetMouseDownCell)
                this._mouseDownNode = null;
        }
        #endregion // EndSelectionDrag

        #region FocusActiveNode

        private void FocusActiveNode()
        {
            if (this.ActiveNode != null && this.ActiveNode.Control != null)
            {
                this.ActiveNode.Control.Focus();

                //FocusManager.SetFocusedElement(this.ActiveNode.NodeLayout.Tree, this.ActiveNode.Control);

            }
        }
        #endregion // FocusActiveNode

        #region ScrollNodeIntoView
        /// <summary>
        /// Scrolls a <see cref="XamDataTreeNode"></see> into the viewable area.
        /// </summary>
        /// <param name="node">The node to be made visable.</param>
        public void ScrollNodeIntoView(XamDataTreeNode node)
        {
            if (node != null && this.Panel != null)
            {
                this.Panel.ScrollNodeIntoView(node);
            }
        }
        #endregion // ScrollNodeIntoView

        #region SetDefaultActiveNode

        private void SetDefaultActiveNode()
        {
            if (this.CurrentEditNode == null && this.ActiveNode == null && this.Nodes.Count > 0)
            {
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    XamDataTreeNode possibleActiveNode = this.Nodes[i];
                    if (possibleActiveNode.IsEnabled)
                    {
                        this.SetActiveNode(possibleActiveNode, TreeInvokeAction.Code);
                        break;
                    }
                }
            }
        }

        #endregion // SetDefaultActiveNode

        #region AttachMouseDownToRootVis

        private void AttachMouseDownToRootVis()
        {

            this.Dispatcher.BeginInvoke(new Action(this.AttachMouseDownToRootVis_Helper));



        }

        private void AttachMouseDownToRootVis_Helper()
        {
            if (this.CurrentEditNode != null)
            {
                FrameworkElement rootVis = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                if (rootVis != null)
                {
                    rootVis.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVis_NodeEditing_MouseLeftButtonDown), true);
                    this._editorRootVisual = rootVis;
                }
            }
        }

        #endregion // AttachMouseDownToRootVis



        #endregion // Private

        #region Internal

        #region SetupPossibleExpandingNode
        internal void SetupPossibleExpandingNode(XamDataTreeNode node)
        {
            if (node == null)
            {
                this._nodeExpanderTimer.Stop();
                this._draggingExpansionNode = null;
                return;
            }

            if (node != this._draggingExpansionNode && !node.IsExpanded && node.HasChildren)
            {
                this._nodeExpanderTimer.Stop();
                this._draggingExpansionNode = node;
                this._nodeExpanderTimer.Start();
            }            
        }
        #endregion // SetupPossibleExpandingNode

        #region CancelDragScrolling

        internal void CancelDragScrolling()
        {
            if (this._scrollNodesTimer != null)
                this._scrollNodesTimer.Stop();
        }

        #endregion CancelDragScrolling
        #endregion // Internal

        #region Static

        #region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

        #endregion // RegisterResources

        #region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

        #endregion // UnregisterResources

        #endregion // Static

        #endregion // Methods

        #region Events

        #region InitializeNode

        /// <summary>
        /// This event is raised when a <see cref="XamDataTreeNode"/> is created.
        /// </summary>
        public event EventHandler<InitializeNodeEventArgs> InitializeNode;

        /// <summary>
        /// Raised when a Node is created.
        /// </summary>
        /// <param propertyName="node">The node that was just created.</param>
        protected internal virtual void OnInitializeNode(XamDataTreeNode node)
        {
            if (this.InitializeNode != null)
                this.InitializeNode(this, new InitializeNodeEventArgs() { Node = node });
        }

        #endregion // InitializeNode

        #region NodeExpansionChanging

        /// <summary>
        /// This event is raised when a <see cref="XamDataTreeNode"/> is expanding or collapsing.
        /// </summary>
        public event EventHandler<CancellableNodeExpansionChangedEventArgs> NodeExpansionChanging;

        /// <summary>
        /// Raises the <see cref="NodeExpansionChanging"/> event.
        /// </summary>
        /// <param propertyName="node">The <see cref="XamDataTreeNode"/> that is being expanded or collapsed</param>
        protected internal virtual bool OnNodeExpansionChanging(XamDataTreeNode node)
        {
            if (this.NodeExpansionChanging != null)
            {
                CancellableNodeExpansionChangedEventArgs args = new CancellableNodeExpansionChangedEventArgs() { Node = node };
                this.NodeExpansionChanging(this, args);
                return args.Cancel;
            }
            return false;
        }

        #endregion // NodeExpansionChanging

        #region NodeExpansionChanged

        /// <summary>
        /// This event is raised when a <see cref="XamDataTreeNode"/> is expanded or collapsed.
        /// </summary>
        public event EventHandler<NodeExpansionChangedEventArgs> NodeExpansionChanged;

        /// <summary>
        /// Raises the <see cref="NodeExpansionChanged"/> event.
        /// </summary>
        /// <param propertyName="node">The <see cref="XamDataTreeNode"/> that was just expanded or collapsed</param>
        protected internal virtual void OnNodeExpansionChanged(XamDataTreeNode node)
        {
            if (this.NodeExpansionChanged != null)
                this.NodeExpansionChanged(this, new NodeExpansionChangedEventArgs() { Node = node });
        }

        #endregion // NodeExpansionChanged

        #region NodeCheckedChanged

        /// <summary>
        /// Event raised when a <see cref="XamDataTreeNode"/> is checked.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeCheckedChanged;

        /// <summary>
        /// Raises the <see cref="NodeCheckedChanged"/> event.
        /// </summary>
        /// <param name="node"></param>
        protected internal virtual void OnNodeCheckChanged(XamDataTreeNode node)
        {
            if (this.NodeCheckedChanged != null)
                this.NodeCheckedChanged(this, new NodeEventArgs() { Node = node });
        }

        #endregion // NodeCheckedChanged

        #region NodeLayoutAssigned

        /// <summary>
        /// Event raised when a <see cref="NodeLayout"/> is assigned to a <see cref="XamDataTreeNode"/>.
        /// </summary>
        public event EventHandler<NodeLayoutAssignedEventArgs> NodeLayoutAssigned;

        /// <summary>
        /// Raises the <see cref="NodeLayoutAssigned"/> event.
        /// </summary>
        /// <param name="args"></param>
        protected internal virtual void OnNodeLayoutAssigned(NodeLayoutAssignedEventArgs args)
        {
            if (this.NodeLayoutAssigned != null)
            {
                this.NodeLayoutAssigned(this, args);
            }
        }

        #endregion // NodeLayoutAssigned

        #region ActiveNodeChanged

        /// <summary>
        /// Event raised when the <see cref="XamDataTree.ActiveNode"/> is changed.
        /// </summary>
        public event EventHandler<ActiveNodeChangedEventArgs> ActiveNodeChanged;

        /// <summary>
        /// Raises the <see cref="ActiveNodeChanged"/> event.
        /// </summary>
        /// <param name="args"></param>
        protected internal virtual void OnActiveNodeChanged(ActiveNodeChangedEventArgs args)
        {
            if (this.ActiveNodeChanged != null)
            {
                this.ActiveNodeChanged(this, args);
            }
        }

        #endregion // ActiveNodeChanged

        #region ActiveNodeChanging

        /// <summary>
        /// Event raised when the <see cref="XamDataTree.ActiveNode"/> is changing.
        /// </summary>
        public event EventHandler<ActiveNodeChangingEventArgs> ActiveNodeChanging;

        /// <summary>
        /// Raises the <see cref="ActiveNodeChanging"/> event.
        /// </summary>
        /// <param name="args"></param>
        protected internal virtual void OnActiveNodeChanging(ActiveNodeChangingEventArgs args)
        {
            if (this.ActiveNodeChanging != null)
            {
                this.ActiveNodeChanging(this, args);
            }
        }

        #endregion // ActiveNodeChanging

        #region NodeEnteringEditMode

        /// <summary>
        /// Event raised when a <see cref="XamDataTreeNode"/> is entering edit mode.
        /// </summary>
        public event EventHandler<BeginEditingNodeEventArgs> NodeEnteringEditMode;

        /// <summary>
        /// Raises the <see cref="NodeEnteringEditMode"/> event.
        /// </summary>
        /// <param name="node"></param>
        protected internal virtual bool OnNodeEnteringEditMode(XamDataTreeNode node)
        {
            if (this.NodeEnteringEditMode != null)
            {
                BeginEditingNodeEventArgs eventArgs = new BeginEditingNodeEventArgs() { Node = node };
                this.NodeEnteringEditMode(this, eventArgs);
                return eventArgs.Cancel;
            }
            return false;
        }

        #endregion // NodeEnteringEditMode

        #region NodeEnteredEditMode

        /// <summary>
        /// Event raised when a <see cref="XamDataTreeNode"/> is in edit mode.
        /// </summary>
        public event EventHandler<TreeEditingNodeEventArgs> NodeEnteredEditMode;

        /// <summary>
        /// Raises the <see cref="NodeEnteredEditMode"/> event.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="editor"></param>
        protected internal virtual void OnNodeEnteredEditMode(XamDataTreeNode node, FrameworkElement editor)
        {
            if (this.NodeEnteredEditMode != null)
            {
                this.NodeEnteredEditMode(this, new TreeEditingNodeEventArgs() { Node = node, Editor = editor });
            }
        }

        #endregion // NodeEnteredEditMode

        #region NodeExitingEditMode

        /// <summary>
        /// Event raised when a <see cref="XamDataTreeNode"/> is exiting edit mode.
        /// </summary>
        public event EventHandler<TreeExitEditingEventArgs> NodeExitingEditMode;

        /// <summary>
        ///  Raises the <see cref="NodeExitingEditMode"/> event.
        /// </summary>
        /// <param name="args"></param>
        protected internal virtual void OnNodeExitEditingMode(TreeExitEditingEventArgs args)
        {
            if (this.NodeExitingEditMode != null)
            {
                this.NodeExitingEditMode(this, args);
            }
        }

        #endregion // NodeExitingEditMode

        #region NodeEditingValidationFailed

        /// <summary>
        /// This event is raised when validation fails while editing a <see cref="XamDataTreeNode"/>.
        /// </summary>
        /// <remarks>When the <see cref="XamDataTree"/> is exiting edit mode, the <see cref="XamDataTree"/> will attach to the BindingValidationError and forward the error, if raised, through this event.
        /// However it will not raise this event if secondary errors are raised on the data object via the <see cref="INotifyDataErrorInfo"/> interface.
        /// </remarks>
        public event EventHandler<NodeValidationErrorEventArgs> NodeEditingValidationFailed;

        /// <summary>
        /// Raises the <see cref="NodeEditingValidationFailed"/> event.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="args"></param>
        protected internal virtual bool OnNodeEditingValidationFailed(XamDataTreeNode node, ValidationErrorEventArgs args)
        {
            if (this.NodeEditingValidationFailed != null)
            {
                NodeValidationErrorEventArgs cve = new NodeValidationErrorEventArgs() { ValidationErrorEventArgs = args, Node = node };
                this.NodeEditingValidationFailed(this, cve);
                return cve.Handled;
            }

            return false;
        }

        #endregion // CellEditingValidationFailed

        #region NodeExitedEditMode

        /// <summary>
        /// Event raised after a <see cref="XamDataTreeNode"/> leaves edit mode.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeExitedEditMode;

        /// <summary>
        /// Raises the <see cref="NodeExitedEditMode"/> event.
        /// </summary>
        /// <param name="node"></param>
        protected internal virtual void OnExitedEditMode(XamDataTreeNode node)
        {
            if (this.NodeExitedEditMode != null)
            {
                this.NodeExitedEditMode(this, new NodeEventArgs() { Node = node });
            }
        }

        #endregion // NodeExitedEditMode

        #region NodeDraggingStart

        /// <summary>
        /// Event raised when a drag / drop operation begins on a <see cref="XamDataTree"/>.
        /// </summary>
        public event EventHandler<DragDropStartEventArgs> NodeDraggingStart;

        /// <summary>
        /// Raises the <see cref="NodeDraggingStart"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnNodeDraggingStart(DragDropStartEventArgs e)
        {
            if (NodeDraggingStart != null)
            {
                this.NodeDraggingStart(this, e);
            }
        }

        #endregion // NodeDraggingStart

        #region NodeDragCancel

        /// <summary>
        /// Event raised when a drag / drop opertation ends by being cancelled.
        /// </summary>
        public event EventHandler<DragDropEventArgs> NodeDragCancel;

        /// <summary>
        ///  Raises the <see cref="NodeDragCancel"/> event.
        /// </summary>
        /// <param name="args"></param>
        protected internal virtual void OnNodeDragCancel(DragDropEventArgs args)
        {
            if (this.NodeDragCancel != null)
            {
                this.NodeDragCancel(this, args);
            }
        }

        #endregion // NodeDragCancel

        #region NodeDragEnd

        /// <summary>
        /// Event raised with a drag / drop operation ends on a <see cref="XamDataTree"/>.
        /// </summary>
        public event EventHandler<DragDropEventArgs> NodeDragEnd;

        /// <summary>
        ///  Raises the <see cref="NodeDragEnd"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnNodeDragEnd(DragDropEventArgs e)
        {
            if (NodeDragEnd != null)
            {
                this.NodeDragEnd(this, e);
            }
        }

        #endregion // NodeDragEnd

        #region NodeDragDrop

        /// <summary>
        /// Event raised when a drag / drop operation ends with a drop action.
        /// </summary>
        public event EventHandler<TreeDropEventArgs> NodeDragDrop;

        /// <summary>
        /// Raises the <see cref="NodeDragDrop"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnNodeDragDrop(TreeDropEventArgs e)
        {
            if (NodeDragDrop != null)
            {
                this.NodeDragDrop(this, e);
            }
        }

        #endregion

        #region OnSelectedNodesCollectionChanged

        /// <summary>
        /// Event raised when the SelectedNodes collection is modified.
        /// </summary>
        public event EventHandler<NodeSelectionEventArgs> SelectedNodesCollectionChanged;

        internal void OnSelectedNodesCollectionChanged(SelectedNodesCollection selectedNodesCollection, SelectedNodesCollection selectedNodesCollection_2)
        {
            if (SelectedNodesCollectionChanged != null)
            {
                NodeSelectionEventArgs args = new NodeSelectionEventArgs();
                args.OriginalSelectedNodes = selectedNodesCollection;
                args.CurrentSelectedNodes = selectedNodesCollection_2;
                this.SelectedNodesCollectionChanged(this, args);
            }
        }

        #endregion // OnSelectedNodesCollectionChanged

        #region DataObjectRequested

        /// <summary>
        /// Event raised when the <see cref="XamDataTree"/> is requesting a new dataobject.
        /// </summary>
        public event EventHandler<TreeDataObjectCreationEventArgs> DataObjectRequested;

        /// <summary>
        /// Raises the DataObjectRequested event.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="nodeLayout"></param>
        /// <param name="parentNode"></param>        
        protected internal virtual void OnDataObjectRequested(HandleableObjectGenerationEventArgs args, NodeLayout nodeLayout, XamDataTreeNode parentNode)
        {
            TreeDataObjectCreationEventArgs newArgs = new TreeDataObjectCreationEventArgs();
            newArgs.ObjectType = args.ObjectType;
            newArgs.NodeLayout = nodeLayout;
            newArgs.ParentNode = parentNode;
            newArgs.CollectionType = args.CollectionType;

            if (this.DataObjectRequested != null)
            {
                this.DataObjectRequested(this, newArgs);
            }

            args.NewObject = newArgs.NewObject;
            if (args.NewObject != null)
                args.Handled = true;
        }

        #endregion // DataObjectRequested

        #region NodeDeleting

        /// <summary>
        /// Event rasied when a <see cref="XamDataTreeNode"/> is being deleted.
        /// </summary>
        protected internal event EventHandler<CancellableNodeDeletionEventArgs> NodeDeleting;

        /// <summary>
        /// Raises the <see cref="NodeDeleting"/> event.
        /// </summary>
        /// <param name="args"></param>
        protected internal virtual void OnNodeDeleting(CancellableNodeDeletionEventArgs args)
        {
            if (this.NodeDeleting != null)
            {
                this.NodeDeleting(this, args);
            }
        }

        #endregion // NodeDeleting

        #region NodeDeleted

        /// <summary>
        /// Event rasied when a <see cref="XamDataTreeNode"/> is deleted.
        /// </summary>
        protected internal event EventHandler<NodeDeletedEventArgs> NodeDeleted;

        /// <summary>
        /// Raises the <see cref="NodeDeleting"/> event.
        /// </summary>
        /// <param name="node"></param>
        protected internal virtual void OnNodeDeleted(XamDataTreeNode node)
        {
            if (this.NodeDeleted != null)
            {
                NodeDeletedEventArgs args = new NodeDeletedEventArgs();
                args.Node = node;
                this.NodeDeleted(this, args);
            }
        }

        #endregion // NodeDeleted        

        #endregion // Events

        #region EventHandlers

        #region XamDataTree_Loaded
        void XamDataTree_Loaded(object sender, RoutedEventArgs e)
        {
            IEnumerable itemSource = (IEnumerable)this.GetValue(ItemsSourceProperty);

            if (itemSource != null)
            {
                this.ApplyItemSource(itemSource);






            }

            this.IsLoaded = true;

            this._parentUIElement = PlatformProxy.GetRootVisual(this);

            if (_parentUIElement != null)
                _parentUIElement.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.XamDataTree_MouseLeftButtonUp), true);
        }
        #endregion // XamDataTree_Loaded

        #region XamDataTree_Unloaded

        void XamDataTree_Unloaded(object sender, RoutedEventArgs e)
        {
            this._isLoaded = false;

            if (this.CurrentEditNode != null && this.CurrentEditNode.IsEditing)
            {
                this.ExitEditMode(true);
            }

            if (_parentUIElement != null)
            {
                _parentUIElement.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.XamDataTree_MouseLeftButtonUp));
                _parentUIElement = null;
            }

            if (this._selectNodesTimer != null)
                this._selectNodesTimer.Stop();

            if (this._scrollNodesTimer != null)
                this._scrollNodesTimer.Stop();

            if (this._nodeExpanderTimer != null)
                this._nodeExpanderTimer.Stop();

            this.ResetPanelNodes(true);
        }

        #endregion // XamDataTree_Unloaded

        #region HorizontalScrollBar Scroll

        private void HorizontalScrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            this.InvalidateScrollPanel(true, false);
        }

        #endregion // HorizontalScrollBar Scroll

        #region VerticalScrollBar Scroll

        private void VerticalScrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            this.InvalidateScrollPanel(false);
        }

        #endregion // VerticalScrollBar Scroll

        #region NodeLayouts_CollectionChanged

        void NodeLayouts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (NodeLayout c in e.NewItems)
                {
                    c.Tree = this;
                }
            }
            else if (e.OldItems != null)
            {
                foreach (NodeLayout c in e.OldItems)
                {
                    c.Tree = null;
                }
            }
        }
        #endregion // NodeLayouts_CollectionChanged

        #region XamDataTree_MouseLeftButtonUp

        XamDataTreeNode _mouseDownNode;
        void XamDataTree_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            _mouseDownPoint = null;


            this.EndSelectionDrag(false);

            if (this._mouseDownNode != null)
            {
                XamDataTreeNodeControl nodeControl = this.GetNodeFromSource(e.OriginalSource as DependencyObject);
                if (nodeControl != null && nodeControl.Node == this._mouseDownNode)
                    this._mouseDownNode.OnNodeClick();

                this._mouseDownNode = null;
            }
        }
        #endregion // XamDataTree_MouseLeftButtonUp

        #region XamDataTree_KeyDown

        void XamDataTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (this.CurrentEditNode != null)
            {
                e.Handled = this.CurrentEditNode.HandleKeyDown(e);
            }
            else if (this.ActiveNode != null)
            {
                e.Handled = this.ActiveNode.HandleKeyDown(e);
            }
        }

        #endregion // XamDataTree_KeyDown

        #region RootVis_CellEditing_MouseLeftButtonDown
        void RootVis_NodeEditing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dependencyObject = e.OriginalSource as DependencyObject;
            XamDataTreeNodeControl nodeControl = this.GetNodeFromSource(dependencyObject);

            



            if (nodeControl == null && dependencyObject != null)
            {
                if (PlatformProxy.GetParent(dependencyObject) == null)
                {
                    return;
                }
            }

            if (this.CurrentEditNode != null && nodeControl == null)
            {
         
            }



            if (nodeControl == null || nodeControl.Node == null || this.CurrentEditNode != nodeControl.Node)
            {
                
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                if ((DateTime.Now - this._lastCancelledExitEditEvent) > TimeSpan.FromSeconds(1))
                    this.ExitEditMode(false);
            }
        }
        #endregion // RootVis_CellEditing_MouseLeftButtonDown

        #region XamDataTree_MouseLeftButtonDown


        Point? _mouseDownPoint;


        void XamDataTree_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {






            bool doubleClick = false;
            long currentTicks = DateTime.Now.Ticks;

            _mouseDownPoint = e.GetPosition(null);

            if (this.CurrentEditNode != null)
            {
                bool internalFind = false;
                DependencyObject parent = PlatformProxy.GetParent(e.OriginalSource as DependencyObject);

                while (parent != null)
                {
                    if (parent == this.CurrentEditNode.Control)
                    {
                        internalFind = true;
                        break;
                    }
                    parent = PlatformProxy.GetParent(parent);
                }

                if (internalFind)
                    return;
            }

            _skipDefaultActivation = true;

            if (!e.Handled)
                this.Focus();
            else
            {
                if (!XamDataTreeNodeControl.IsFocusedControlInsideEditor(this))
                {
                    this.Focus();
                }
            }

            _skipDefaultActivation = false;

            XamDataTreeNodeControl nodeControl = this.GetNodeFromSource(e.OriginalSource as DependencyObject);

            if (nodeControl != null && nodeControl.Node != null)
            {
                if ((currentTicks - this._doubleClickTimeStamp) <= 4000000 && nodeControl.Node == this._doubleClickNode)
                    doubleClick = true;

                this._doubleClickTimeStamp = DateTime.Now.Ticks;

                XamDataTreeNode node = nodeControl.Node;

                if (doubleClick)
                {
                    if (node == null || !node.IsEditing)
                        node.OnNodeDoubleClick();
                    this._doubleClickNode = this._mouseDownNode = null;
                    return;
                }

                if (!node.IsEditing)
                {

                    this._dragSelectType = node.OnNodeMouseDown(e);






                    this._doubleClickNode = this._mouseDownNode = node;
                }
            }
            else
            {
                this._doubleClickNode = this._mouseDownNode = null;
            }

        }
        #endregion // XamDataTree_MouseLeftButtonDown

        #region SelectRowsCellsTimer_Tick
        private void SelectRowsCellsTimer_Tick(object sender, EventArgs e)
        {
            // Get Bounds of the RowsPanel
            Rect r = Rect.Empty;
            try
            {

                GeneralTransform gt = this._panel.TransformToVisual(this);
                Rect bounds = LayoutInformation.GetLayoutSlot(this._panel);
                r = gt.TransformBounds(bounds);





            }
            catch (ArgumentException) { };

            if (r.IsEmpty)
            {
                this._selectNodesTimer.Stop();
                return;
            }

            double top = r.Y;
            double bottom = top + this._panel.ActualHeight;
            double left = r.X;
            double right = left + this._panel.ActualWidth;

            // Calculate the Y position we should pretend we're on inside of the RowsPanel
            double y = this._mousePosition.Y;
            if (y < top)
            {
                y = top;
            }
            else if (y > bottom)
            {
                y = bottom - 1;
            }

            // To Calculate X properly we need to figure out which row we're representing.
            IEnumerable<UIElement> elements = PlatformProxy.GetElementsFromPoint(new Point(left, y), this._panel);

            XamDataTreeNodeControl nodesPanel = null;
            foreach (UIElement elem in elements)
            {
                nodesPanel = elem as XamDataTreeNodeControl;
                if (nodesPanel != null)
                    break;
            }

            // Calculate the X position that we're representing inside of the RowsPanel.
            double x = this._mousePosition.X;
            if (x < left)
                x = left;
            else if (x > right)
                x = right - 1;

            // Cell and Row Selection are allowed to scroll vertically. 
            if (this._verticalScrollbar != null)
            {
                if (this._mousePosition.Y < top)
                    this._verticalScrollbar.Value--;
                else if (this._mousePosition.Y > bottom)
                    this._verticalScrollbar.Value++;
            }

            // Now that we have a proper X and Y coordinate, lets figure out what cell we'll need to select or unselect.
            elements = PlatformProxy.GetElementsFromPoint(new Point(x, y), this._panel);
            XamDataTreeNodeControl nodeControl = null;
            foreach (UIElement elem in elements)
            {
                nodeControl = elem as XamDataTreeNodeControl;
                if (nodeControl != null)
                    break;
            }

            // If We have a cell, lets select or unselect it or its row.
            if (nodeControl != null && nodeControl.Node != null && !this.IsNodeDragging)
            {
                if (this._dragSelectType == TreeDragSelectType.Node)
                    this.SelectNode(nodeControl.Node, TreeInvokeAction.MouseMove);

            }

            // Tell the Grid to redraw.
            this.InvalidateScrollPanel(false);

        }
        #endregion // SelectRowsCellsTimer_Tick

        #region XamDataTree_IsEnabledChanged
        void XamDataTree_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.InvalidateScrollPanel(false, true);
        }
        #endregion // XamDataTree_IsEnabledChanged

        #region ScrollNodesTimer_Tick
        void ScrollNodesTimer_Tick(object sender, EventArgs e)
        {
            // Get Bounds of the RowsPanel
            Rect r = Rect.Empty;
            try
            {

                GeneralTransform gt = this._panel.TransformToVisual(this);
                Rect bounds = LayoutInformation.GetLayoutSlot(this._panel);
                r = gt.TransformBounds(bounds);





            }
            catch (ArgumentException) { };

            if (r.IsEmpty)
            {
                this._scrollNodesTimer.Stop();
                return;
            }

            if (!DragDropManager.IsDragging)
            {
                this._scrollNodesTimer.Stop();
                return;
            }

            double top = r.Y;
            double bottom = top + this._panel.ActualHeight;
            double left = r.X;
            double right = left + this._panel.ActualWidth;

            // Calculate the Y position we should pretend we're on inside of the RowsPanel
            double y = this._mousePosition.Y;
            if (y < top)
            {
                y = top;
            }
            else if (y > bottom)
            {
                y = bottom - 1;
            }

            // To Calculate X properly we need to figure out which row we're representing.
            IEnumerable<UIElement> elements = PlatformProxy.GetElementsFromPoint(new Point(left, y), this._panel);

            XamDataTreeNodeControl nodesPanel = null;
            foreach (UIElement elem in elements)
            {
                nodesPanel = elem as XamDataTreeNodeControl;
                if (nodesPanel != null)
                    break;
            }

            // Calculate the X position that we're representing inside of the RowsPanel.
            double x = this._mousePosition.X;

            if (x > left && x < right)
            {
                // Cell and Row Selection are allowed to scroll vertically. 
                if (this._verticalScrollbar != null)
                {
                    if (this._mousePosition.Y < top)
                        this._verticalScrollbar.Value -= .1;
                    else if (this._mousePosition.Y > bottom)
                        this._verticalScrollbar.Value += .1;
                }
            }

            // Tell the Grid to redraw.
            this.InvalidateScrollPanel(false);
        }
        #endregion // ScrollNodesTimer_Tick

        #region NodeExpanderTimer_Tick
        void NodeExpanderTimer_Tick(object sender, EventArgs e)
        {
            this._nodeExpanderTimer.Stop();
            if (this._draggingExpansionNode != null)
                this._draggingExpansionNode.IsExpanded = true;
        }
        #endregion // NodeExpanderTimer_Tick

        #endregion // EventHandlers

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="XamDataTree"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="XamDataTree"/> object.
        /// </summary>
        /// <param propertyName="propertyName">The propertyName of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IProvideScrollInfo Members

        ScrollBar IProvideScrollInfo.VerticalScrollBar
        {
            get { return this.VerticalScrollBar; }
        }

        ScrollBar IProvideScrollInfo.HorizontalScrollBar
        {
            get { return this.HorizontalScrollBar; }
        }

        #endregion        

        internal void DragSource_DragOver(object sender, DragDropMoveEventArgs e)
         {
             if (this.Nodes.Count == 0)
             {
                 // need to add a validation check here, to ensure that the type of the node being added can be added to 
                 // this tree.
                 XamDataTreeNodeControl nodeControl = e.DragSource as XamDataTreeNodeControl;

                 if (nodeControl != null)
                 {
                     if (XamDataTreeNodeControl.EnsureNodeCanBeAddedToCollection(this.NodesManager.DataManager, nodeControl.Node))
                     {
                         e.OperationType = OperationType.Move;
                         VisualStateManager.GoToState(nodeControl, "DropOnto", false);
                     }
                     else
                     {
                         e.OperationType = OperationType.DropNotAllowed;
                         VisualStateManager.GoToState(nodeControl, "NoDrop", false);
                     }
                 }               
             }             
         }

        #region ISupportScrollHelper Members

        ScrollType ISupportScrollHelper.VerticalScrollType
        {
            get { return ScrollType.Item; }
        }

        ScrollType ISupportScrollHelper.HorizontalScrollType
        {
            get { return ScrollType.Pixel; }
        }

        double ISupportScrollHelper.GetFirstItemHeight()
        {
            if (this._panel != null && this._panel.VisibleNodes.Count > 0)
                return this._panel.VisibleNodes[0].Control.DesiredSize.Height;
            else
                return 0;
        }

        double ISupportScrollHelper.GetFirstItemWidth()
        {
            return 0;
        }

        void ISupportScrollHelper.InvalidateScrollLayout()
        {
            this.InvalidateScrollPanel(false);
        }

        double ISupportScrollHelper.VerticalValue
        {
            get
            {
                if (this.VerticalScrollBar != null)
                    return this.VerticalScrollBar.Value;

                return 0;
            }
            set
            {
                if (this.VerticalScrollBar != null)
                    this.VerticalScrollBar.Value = value;
            }
        }

        double ISupportScrollHelper.VerticalMax
        {
            get
            {
                if (this.VerticalScrollBar != null)
                    return this.VerticalScrollBar.Maximum;

                return 0;
            }
        }

        double ISupportScrollHelper.HorizontalValue
        {
            get
            {
                if (this.HorizontalScrollBar != null)
                    return this.HorizontalScrollBar.Value;

                return 0;
            }
            set
            {
                if (this.HorizontalScrollBar != null)
                    this.HorizontalScrollBar.Value = value;
            }
        }

        double ISupportScrollHelper.HorizontalMax
        {
            get
            {
                if (this.HorizontalScrollBar != null)
                    return this.HorizontalScrollBar.Maximum;

                return 0;
            }
        }

		private bool CanScroll(ScrollBar sb)
		{
			return sb.IsEnabled && sb.ViewportSize > 0 && sb.Maximum > sb.Minimum;
		}

		TouchScrollMode ISupportScrollHelper.GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver)
        {
			if (!IsTouchSupportEnabled)
				return TouchScrollMode.None;

			bool canScrollHorizontally	= this.CanScroll(this.HorizontalScrollBar);
			bool canScrollVertically = this.CanScroll( this.VerticalScrollBar);

			if (canScrollHorizontally)
				return canScrollVertically ? TouchScrollMode.Both		: TouchScrollMode.Horizontal;
			else
				return canScrollVertically ? TouchScrollMode.Vertical	: TouchScrollMode.None;
        }

        void ISupportScrollHelper.OnPanComplete()
        {
        }

        void ISupportScrollHelper.OnStateChanged(TouchState newState, TouchState oldState)
        {


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved