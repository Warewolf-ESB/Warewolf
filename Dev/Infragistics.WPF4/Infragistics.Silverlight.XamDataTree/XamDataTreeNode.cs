using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Infragistics.Controls.Menus.Primitives;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A object that represents a single data object in the <see cref="XamDataTree"/>.
    /// </summary>
    public class XamDataTreeNode : DependencyObjectRecyclingContainer<XamDataTreeNodeControl>, IBindableItem, ISelectableObject
    {
        #region Members

        IntermediateNodesManager _headerNodesManager;
        NodesManager _childNodesManager;
        XamDataTreeNodeControl _control;
        bool _expanded, _isHeader, _isSelected, _surpressParentChildNotification = false, _ignoreIsExpandedPropertyNotification, _surpressIsCheckNotification = false;
        string _identifier;
        NodeLayout _layout;

        Style _style;
        Action<object> _valueChangedFunction;
        FrameworkElement _editor;

        Binding _checkboxBinding;
		Binding _isEnabledBinding;
        Binding _isExpandedBinding;

        bool _changingIsEnabledOnControl = false;

        bool? _isDraggable;
        bool? _isDropTarget;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XamDataTreeNode"/> class.
        /// </summary>
        public XamDataTreeNode(int index, NodesManager manager, object data, bool isHeader, NodeLayout layout)
        {
            this.Manager = manager;
            this.Data = data;
            this.Index = index;
            this._isHeader = isHeader;
            this._layout = layout;

            if (layout != null)
                layout.PropertyChanged += new PropertyChangedEventHandler(layout_PropertyChanged);

            if (this.NodeLayout != null)
            {
                this._identifier = this.NodeLayout.Key;

                if (this.IsHeader)
                    this._identifier += "_Header";
            }

            this.SetupCheckboxBinding();
            this.SetupIsExpandedBinding();

			// MD 8/11/11 - XamFormulaEditor
			this.SetupIsEnabledBinding();
        }

        #endregion // Constructor

        #region EventHandlers

        void layout_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case ("CheckBoxMemberPathResolved"):
                    {
                        this.SetupCheckboxBinding();
                        break;
                    }
                case ("ExpandedIconTemplateResolved"):
                    {
                        if (this.ExpandedIconTemplate == null)
                            this.OnPropertyChanged("ExpandedIconTemplateResolved");
                        break;
                    }
                case ("CollapsedIconTemplateResolved"):
                    {
                        if (this.CollapsedIconTemplate == null)
                            this.OnPropertyChanged("CollapsedIconTemplateResolved");
                        break;
                    }

				// MD 8/11/11 - XamFormulaEditor
				case ("IsEnabledMemberPathResolved"):
					{
						this.SetupIsEnabledBinding();
						break;
					}

                case ("IsExpandedMemberPathResolved"):
                    {
                        this.SetupIsExpandedBinding();
                        break;
                    }
            }
        }

        #endregion // EventHandlers

        #region Overrides

        #region OnElementAttached

        /// <summary>
        /// Called when the <see cref="XamDataTreeNodeControl"/> is attached to the <see cref="XamDataTreeNode"/>
        /// </summary>
        /// <param propertyName="element">A <see cref="XamDataTreeNodeControl"/></param>
        protected override void OnElementAttached(XamDataTreeNodeControl element)
        {
            this._control = element;
            this.ApplyStyle();
            this._control.OnAttached(this);
            this.EnsureCurrentState();
        }
        #endregion // OnElementAttached

        #region OnElementReleasing

        /// <summary>
        /// Invoked when a <see cref="FrameworkElement"/> is being released from an object.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>False, if the element shouldn't be released.</returns>
        protected override bool OnElementReleasing(XamDataTreeNodeControl element)
        {
            if (this.NodeLayout == null || this.NodeLayout.Tree == null)
                return true;

            return !this.IsEditing && this.NodeLayout.Tree.CurrentDraggingNode != this;
        }

        #endregion // OnElementReleasing

        #region OnElementReleased

        /// <summary>
        /// Called when the <see cref="XamDataTreeNodeControl"/> is removed from the <see cref="XamDataTreeNode"/>
        /// </summary>
        /// <param propertyName="element">A <see cref="XamDataTreeNodeControl"/></param>
        protected override void OnElementReleased(XamDataTreeNodeControl element)
        {
            this._control = null;

            element.OnReleased(this);

            this.IsMouseOver = false;
        }
        #endregion // OnElementReleased

        #region RecyclingIdentifier

        /// <summary>
        /// If a <see cref="ISupportRecycling.RecyclingElementType"/> isn't specified, this property can be used to offer another way of identifying 
        /// a reyclable element.
        /// </summary>
        protected override string RecyclingIdentifier
        {
            get
            {
                return this._identifier;
            }
        }
        #endregion // RecyclingIdentifier

        #region CreateInstanceOfRecyclingElement

        /// <summary>
        /// Creates a new instance of the XamDataTreeNode. 
        /// Note: this method is only meant to be invoked via the RecyclingManager.
        /// </summary>
        /// <returns>A new <see cref="XamDataTreeNodeControl"/></returns>
        protected override XamDataTreeNodeControl CreateInstanceOfRecyclingElement()
        {
            return new XamDataTreeNodeControl();
        }
        #endregion // CreateInstanceOfRecyclingElement

        #region ToString



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


        #endregion // ToString

        #endregion // Overrides

        #region Properties

        #region Public

        #region IsExpanded

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(XamDataTreeNode), new PropertyMetadata(false, new PropertyChangedCallback(IsExpandedChanged)));

        /// <summary>
        /// Gets/sets whether the <see cref="XamDataTreeNode"/> is expanded or collapsed.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)this.GetValue(IsExpandedProperty); }
            set { this.SetValue(IsExpandedProperty, value); }
        }

        private static void IsExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTreeNode ctrl = (XamDataTreeNode)obj;

            NodeLayout nodeLayout = ctrl.NodeLayout;

            if (nodeLayout != null)
                ctrl.NodeLayout.Tree.ExitEditMode(true);

            if (ctrl._ignoreIsExpandedPropertyNotification)
                return;

            bool value = (bool)e.NewValue;

            if (nodeLayout != null && !nodeLayout.Tree.OnNodeExpansionChanging(ctrl))
            {
                ctrl._expanded = value;

                if (ctrl.Manager.Nodes.Contains(ctrl))
                {
                    if (value)
                        ctrl.Manager.RegisterChildNodesManager(ctrl.ChildNodesManager);
                    else
                        ctrl.Manager.UnregisterChildNodesManager(ctrl.ChildNodesManager);
                }

                nodeLayout.Tree.OnNodeExpansionChanged(ctrl);
            }
            else
            {
                ctrl._ignoreIsExpandedPropertyNotification = true;
                ctrl.IsExpanded = !value;
                ctrl._ignoreIsExpandedPropertyNotification = false;
            }

            ctrl.OnPropertyChanged("IsExpanded");

            if (nodeLayout != null)
                ctrl.NodeLayout.Tree.InvalidateScrollPanel(true);
        }

        #endregion // IsExpanded

        #region Data
        /// <summary>
        /// Gets the underlying data associated with the <see cref="XamDataTreeNode"/>.
        /// </summary>
        public object Data
        {
            get;
            protected internal set;
        }
        #endregion // Data

        #region Index

        /// <summary>
        /// Gets the currentIndex index of the <see cref="XamDataTreeNode"/>
        /// </summary>
        public int Index
        {
            get;
            protected internal set;
        }

        #endregion // Index

        #region Manager

        /// <summary>
        /// The <see cref="NodesManager"/> that owns this particular node. 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public NodesManager Manager
        {
            get;
            protected internal set;
        }
        #endregion // Manager

        #region Control

        /// <summary>
        /// Gets the <see cref="XamDataTreeNodeControl"/> that is attached to the <see cref="XamDataTreeNode"/>
        /// </summary>
        /// <remarks>A Control is only assoicated with a node when it's in the viewport of the <see cref="NodesPanel"/></remarks>
        public XamDataTreeNodeControl Control
        {
            get { return this._control; }
        }

        #endregion // Control

        #region IsMouseOver
        /// <summary>
        /// Gets whether or not the Mouse is currently over the <see cref="XamDataTreeNode"/>
        /// </summary>
        public bool IsMouseOver
        {
            get;
            protected internal set;
        }

        #endregion // IsMouseOver

        #region NodeLayout

        /// <summary>
        /// Gets the <see cref="NodeLayout"/> that is associated with the <see cref="XamDataTreeNode"/>.
        /// </summary>
        public virtual NodeLayout NodeLayout
        {
            get
            {
                if (this._layout == null && this.Manager != null)
                    return this.Manager.NodeLayout;
                else
                    return this._layout;
            }
        }
        #endregion // NodeLayout

        #region HasChildren

        /// <summary>
        /// Gets whether or not <see cref="XamDataTreeNode"/> has any children.
        /// </summary>
        public bool HasChildren
        {
            get
            {
                bool retValue = false;
                if (this.ChildNodesManager != null)
                {
                    retValue = (this.ChildNodesManager.FullNodeCount > 0);
                }
                return retValue;
            }
        }

        #endregion // HasChildren

        #region Nodes

        /// <summary>
        /// Gets child nodes of the current <see cref="XamDataTreeNode"/>
        /// </summary>
        [Browsable(false)]
        public XamDataTreeNodesCollection Nodes
        {
            get
            {
                return ((XamDataTreeNodesCollection)this.ChildNodesManager.Nodes);
            }
        }

        #endregion // Nodes

        #region IsHeader

        /// <summary>
        /// Gets whether or not a particular <see cref="XamDataTreeNode"/> is a an actual Data Node, or a Node representing the property of an IEnumerable.
        /// </summary>
        public bool IsHeader
        {
            get
            {
                return this._isHeader;
            }
        }

        #endregion // IsHeader

        #region IsChecked

        /// <summary>
        /// Identifies the <see cref="IsChecked"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool?), typeof(XamDataTreeNode), new PropertyMetadata(false, new PropertyChangedCallback(IsCheckedChanged)));

        /// <summary>
        /// Gets / sets if the <see cref="XamDataTreeNode"/> is checked.
        /// </summary>
        public bool? IsChecked
        {
            get { return (bool?)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        private static void IsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTreeNode ctrl = (XamDataTreeNode)obj;

            if (ctrl._surpressIsCheckNotification)
                return;

            if (ctrl.NodeLayout != null && ctrl.NodeLayout.CheckBoxSettings.CheckBoxModeResolved == TreeCheckBoxMode.Auto)
            {
                if (!ctrl._surpressParentChildNotification)
                {
                    if (ctrl.Manager.ParentNode != null)

                        ctrl.Manager.ParentNode.ValidateParentCheckedState();
                }

                ctrl.ValidateChildNodesCheckState();
            }

            ctrl.OnPropertyChanged("IsChecked");

            if (ctrl.NodeLayout != null)
            {
                ctrl.NodeLayout.Tree.OnNodeCheckChanged(ctrl);

                ctrl.NodeLayout.Tree.InvalidateScrollPanel(false);
            }
        }

        #endregion // IsChecked

        #region IsSelected

        /// <summary>
        /// Gets/Sets whether an item is currently selected. 
        /// </summary>
        public virtual bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                if (this.NodeLayout != null)
                {
                    bool oldValue = this._isSelected;
                    if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                    {
                        if (value && this.IsEnabled)
                            this.NodeLayout.Tree.SelectNode(this, TreeInvokeAction.Code);
                        else
                            this.NodeLayout.Tree.UnselectNode(this);
                    }
                }
            }
        }
        #endregion // IsSelected

        #region Style

        /// <summary>
        /// Gets / sets the <see cref="Style"/> which should be applied to this <see cref="XamDataTreeNode"/>.
        /// </summary>
        public Style Style
        {
            get
            {
                return this._style;
            }
            set
            {
                if (this._style != value)
                {
                    this._style = value;

                    if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                    {
                        this.NodeLayout.Tree.InvalidateScrollPanel(false);
                    }

                    this.ApplyStyle();
                }
            }
        }

        #endregion // Style

        #region IsActive

        /// <summary>
        /// Gets / set if the <see cref="XamDataTreeNode"/> is currently the <see cref="XamDataTree.ActiveNode"/>.
        /// </summary>
        public bool IsActive
        {
            get
            {
                if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                    return (this.NodeLayout.Tree.ActiveNode == this);

                return false;
            }
            set
            {
                if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                {
                    if (value)
                    {
                        this.NodeLayout.Tree.ActiveNode = this;
                    }
                    else
                    {
                        this.NodeLayout.Tree.ActiveNode = null;
                    }

                    this.RaiseIsActiveChanged();
                }
            }
        }

        #endregion // IsActive

        #region IsEditing

        /// <summary>
        /// Gets if the <see cref="XamDataTreeNode"/> is currently in edit mode.
        /// </summary>
        public bool IsEditing
        {
            get
            {
                return this.NodeLayout != null && this.NodeLayout.Tree != null && this.NodeLayout.Tree.CurrentEditNode == this;
            }
        }

        #endregion // IsEditing

        #region IsDraggable

        /// <summary>
        /// Gets / sets if the <see cref="XamDataTreeNode"/> can be dragged from it's current position.
        /// </summary>
        public bool? IsDraggable
        {
            get
            {
                return this._isDraggable;
            }
            set
            {
                if (this._isDraggable != value)
                {
                    this._isDraggable = value;
                    this.OnPropertyChanged("IsDraggable");
                    this.OnPropertyChanged("IsDraggableResolved");
                }
            }
        }

        #endregion // IsDraggable

        #region IsDropTarget

        /// <summary>
        /// Gets / sets if the <see cref="XamDataTreeNode"/> can act as a drop target for nodes.
        /// </summary>
        public bool? IsDropTarget
        {
            get
            {
                return this._isDropTarget;
            }
            set
            {
                if (this._isDropTarget != value)
                {
                    this._isDropTarget = value;
                    this.OnPropertyChanged("IsDropTarget");
                    this.OnPropertyChanged("IsDropTargetResolved");
                }
            }
        }

        #endregion // IsDropTarget

        #region IsDraggableResolved

        /// <summary>
        /// Gets whether a <see cref="XamDataTreeNode"/> is draggable after taking account values from the <see cref="NodeLayout"/> level.
        /// </summary>
        public bool IsDraggableResolved
        {
            get
            {
                bool retVal = false;

                if (this.IsDraggable != null)
                    retVal = (bool)this.IsDraggable;
                else if (this.NodeLayout != null)
                    retVal = this.NodeLayout.IsDraggableResolved;

                return retVal;
            }
        }

        #endregion // IsDraggableResolved

        #region IsDropTargetResolved

        /// <summary>
        /// Gets whether a <see cref="XamDataTreeNode"/> is a drop target after taking account values from the <see cref="NodeLayout"/> level.
        /// </summary>
        public bool IsDropTargetResolved
        {
            get
            {
                bool retVal = false;

                if (this.IsDropTarget != null)
                    retVal = (bool)this.IsDropTarget;
                else if (this.NodeLayout != null)
                    retVal = this.NodeLayout.IsDropTargetResolved;

                return retVal;
            }
        }

        #endregion // IsDropTargetResolved

        #region ExpandedIconTemplate

        /// <summary>
        /// Identifies the <see cref="ExpandedIconTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ExpandedIconTemplateProperty = DependencyProperty.Register("ExpandedIconTemplate", typeof(DataTemplate), typeof(XamDataTreeNode), new PropertyMetadata(new PropertyChangedCallback(ExpandedIconTemplateChanged)));

        /// <summary>
        /// Gets / sets the expanded <see cref="DataTemplate"/> that will be displayed on this node.
        /// </summary>
        public DataTemplate ExpandedIconTemplate
        {
            get { return (DataTemplate)this.GetValue(ExpandedIconTemplateProperty); }
            set { this.SetValue(ExpandedIconTemplateProperty, value); }
        }

        private static void ExpandedIconTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTreeNode ctrl = (XamDataTreeNode)obj;
            ctrl.OnPropertyChanged("ExpandedIconTemplate");
        }

        #endregion // ExpandedIconTemplate

        #region CollapsedIconTemplate

        /// <summary>
        /// Identifies the <see cref="CollapsedIconTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CollapsedIconTemplateProperty = DependencyProperty.Register("CollapsedIconTemplate", typeof(DataTemplate), typeof(XamDataTreeNode), new PropertyMetadata(new PropertyChangedCallback(CollapsedIconTemplateChanged)));

        /// <summary>
        /// Gets / sets the collapsed <see cref="DataTemplate"/> that will be displayed on this node.
        /// </summary>
        public DataTemplate CollapsedIconTemplate
        {
            get { return (DataTemplate)this.GetValue(CollapsedIconTemplateProperty); }
            set { this.SetValue(CollapsedIconTemplateProperty, value); }
        }

        private static void CollapsedIconTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTreeNode ctrl = (XamDataTreeNode)obj;
            ctrl.OnPropertyChanged("CollapsedIconTemplate");
        }

        #endregion // CollapsedIconTemplate

        #region ExpandedIconTemplateResolved

        /// <summary>
        /// Gets / sets the expanded <see cref="DataTemplate"/> that will be displayed on this node.
        /// </summary>
        public DataTemplate ExpandedIconTemplateResolved
        {
            get
            {
                DataTemplate retValue = this.ExpandedIconTemplate;

                if (retValue == null && this.NodeLayout != null)
                {
                    retValue = this.NodeLayout.ExpandedIconTemplateResolved;
                }
                return retValue;
            }
        }

        #endregion // ExpandedIconTemplateResolved

        #region CollapsedIconTemplateResolved

        /// <summary>
        /// Gets the collapsed <see cref="DataTemplate"/> that will be displayed on this node.
        /// </summary>
        public DataTemplate CollapsedIconTemplateResolved
        {
            get
            {
                DataTemplate retValue = this.CollapsedIconTemplate;

                if (retValue == null && this.NodeLayout != null)
                {
                    retValue = this.NodeLayout.CollapsedIconTemplateResolved;
                }

                return retValue;
            }
        }

        #endregion // CollapsedIconTemplateResolved

        #region IsEnabled

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(XamDataTreeNode), new PropertyMetadata(true, new PropertyChangedCallback(IsEnabledChanged)));

        /// <summary>
        /// Gets / sets if the <see cref="XamDataTreeNodeControl"/> should be enabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return (bool)this.GetValue(IsEnabledProperty); }
            set { this.SetValue(IsEnabledProperty, value); }
        }

        private static void IsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTreeNode node = (XamDataTreeNode)obj;

            bool enabledValue = (bool)e.NewValue;

            if (!node._changingIsEnabledOnControl)
            {
                node._changingIsEnabledOnControl = true;

                node.OnPropertyChanged("IsEnabled");

                node.EnsureCurrentState();

                node._changingIsEnabledOnControl = false;
            }
        }

        #endregion // IsEnabled

        #endregion // Public

        #region Protected

        #region ItemsSource

        /// <summary>
        /// Gets/Sets the children data that the node owns. 
        /// </summary>
        protected internal IEnumerable ItemsSource
        {
            get;
            set;
        }

        #endregion // ItemsSource

        #region ChildNodesManager

        /// <summary>
        /// Gets the <see cref="NodesManager"/> that the <see cref="XamDataTreeNode"/> owns.
        /// </summary>
        protected internal NodesManager ChildNodesManager
        {
            get
            {
                if (this._childNodesManager == null && this._headerNodesManager == null)
                {
                    NodesManager manager = this.Manager;
                    if (manager.NodeLayout != null && manager.NodeLayout.Tree != null && manager.Level < manager.NodeLayout.Tree.MaxDepth)
                    {
                        if (this.IsHeader)
                            this._childNodesManager = new NodesManager(manager.Level + 1, this.NodeLayout, this);
                        else
                            this._headerNodesManager = new IntermediateNodesManager(manager.Level + 1, this.NodeLayout, this);
                    }
                }

                if (this._headerNodesManager != null)
                {
                    int count = this._headerNodesManager.FullNodeCount;
                    if (count == 1)
                    {
                        if (this._childNodesManager == null)
                            this._childNodesManager = new NodesManager(this.Manager.Level + 1, this.NodeLayout.NodeLayouts[0], this);
                    }
                    else
                    {
                        return this._headerNodesManager;
                    }
                }

                return this._childNodesManager;
            }
            set
            {
                this._childNodesManager = value;
                if (value == null)
                {
                    this._headerNodesManager = null;
                }
            }
        }

        #endregion // ChildNodesManager

        #region Value

        /// <summary>
        /// Gets the Value of the <see cref="XamDataTreeNode"/>.  The Value is defined as the value from the field on the data object defined by the <see cref="Infragistics.Controls.Menus.NodeLayout.DisplayMemberPathResolved"/>.
        /// </summary>
        protected internal object Value
        {
            get
            {
                object val = null;
                if (this.Data != null && !string.IsNullOrEmpty(this.NodeLayout.DisplayMemberPathResolved))
                {
                    NodeValueObject cellValueObj = new NodeValueObject();

                    Binding b = new Binding(this.NodeLayout.DisplayMemberPathResolved);

                    b.Mode = BindingMode.OneTime;

                    b.Source = this.Data;
                    cellValueObj.SetBinding(NodeValueObject.ValueProperty, b);
                    val = cellValueObj.Value;

                }
                return val;
            }
        }

        #endregion // Value

        #region ResolveStyle
        /// <summary>
        /// Gets the <see cref="Style"/> which should be applied to this <see cref="XamDataTreeNode"/>.
        /// </summary>
        protected virtual Style ResolveStyle
        {
            get
            {
                if (this.Style != null)
                    return this.Style;

                return this.NodeLayout.NodeStyleResolved;
            }
        }
        #endregion // ResolveStyle

        #endregion // Protected

        #region Internal
        #region JustMoved
        internal bool JustMoved { get; set; }
        #endregion // JustMoved
        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Protected

        #region EnsureCurrentState

        /// <summary>
        /// Ensures that <see cref="XamDataTreeNode"/> is in the correct state.
        /// </summary>
        protected internal virtual void EnsureCurrentState()
        {
            bool retValue = false;
            XamDataTreeNodeControl control = this.Control as XamDataTreeNodeControl;
            if (control != null)
            {
                #region IsEnabled
                if (this.IsEnabled && this.Control.IsEnabled)
                {
                    #region IsMouseOver
                    
                    


                    if (this.IsMouseOver && (this.NodeLayout != null && this.NodeLayout.Tree != null && !this.NodeLayout.Tree.IsNodeDragging))
                        VisualStateManager.GoToState(control, "MouseOver", false);
                    else
                        VisualStateManager.GoToState(control, "Normal", false);

                    #endregion // IsMouseOver
                }
                else
                {
                    retValue = VisualStateManager.GoToState(control, "Disabled", false);
                }
                #endregion // IsEnabled

                #region IsExpanded
                if (this.IsExpanded)
                {
                    if (this.ExpandedIconTemplateResolved != null)
                    {
                        retValue = VisualStateManager.GoToState(control, "ShowExpandedIcon", false);
                    }
                    else
                    {
                        retValue = VisualStateManager.GoToState(control, "HideIcons", false);
                    }
                    control.InvalidateArrange();
                    control.InvalidateMeasure();
                }
                else
                {
                    if (this.CollapsedIconTemplateResolved != null)
                    {
                        retValue = VisualStateManager.GoToState(control, "ShowCollapsedIcon", false);

                    }
                    else
                    {
                        retValue = VisualStateManager.GoToState(control, "HideIcons", false);
                    }
                    control.InvalidateArrange();
                    control.InvalidateMeasure();
                }
                #endregion // IsExpanded

                #region IsSelected
                if (this.IsSelected)
                {
                    retValue = VisualStateManager.GoToState(control, "Selected", false);
                }
                else
                {
                    retValue = VisualStateManager.GoToState(control, "NotSelected", false);
                }
                #endregion // IsSelected

                #region ActiveNode

                if ((this.NodeLayout != null) && (this.NodeLayout.Tree.ShowActiveNodeIndicator && this.NodeLayout.Tree.ActiveNode == this))
                {
                    VisualStateManager.GoToState(control, "Active", false);
                }
                else
                {
                    VisualStateManager.GoToState(control, "Inactive", false);
                }
                #endregion // ActiveNode

                #region NodeLines
                if (this.Manager.Level > 0)
                {
                    Visibility visibility = this.NodeLayout.Tree.NodeLineVisibility;
                    if (visibility == Visibility.Collapsed

                        || visibility == Visibility.Hidden

)
                    {
                        VisualStateManager.GoToState(control, "None", false);
                    }
                    else
                    {
                        int siblingCount = this.Manager.Nodes.Count;

                        int index = this.Index;

                        if (this.Nodes.Count == 0)
                        {
                            if (index == siblingCount - 1)
                            {
                                VisualStateManager.GoToState(control, "LShape", false);
                            }
                            else
                            {
                                VisualStateManager.GoToState(control, "TShape", false);
                            }
                        }
                    }
                }
                else
                {
                    VisualStateManager.GoToState(control, "None", false);
                }
                #endregion // NodeLines
            }
        }

        #endregion // EnsureCurrentState

        #region OnNodeClick

        /// <summary>
        /// Method called when the <see cref="XamDataTreeNode"/> is clicked.
        /// </summary>
        protected internal virtual void OnNodeClick()
        {
            this.EnterEditModeViaMouseClick(TreeMouseEditingAction.SingleClick);
        }

        #endregion // OnNodeClick

        #region OnNodeDoubleClick

        /// <summary>
        /// Method called when the <see cref="XamDataTreeNode"/> is double clicked.
        /// </summary>
        protected internal virtual void OnNodeDoubleClick()
        {
            this.EnterEditModeViaMouseClick(TreeMouseEditingAction.DoubleClick);
        }

        #endregion // OnNodeDoubleClick

        #region OnNodeMouseDown

        /// <summary>
        /// Method called when the <see cref="XamDataTreeNode"/> receives a mouse down.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected internal virtual TreeDragSelectType OnNodeMouseDown(MouseEventArgs e)
        {
            XamDataTree tree = this.NodeLayout.Tree;

            tree.SetActiveNode(this, TreeInvokeAction.Click, true);

            if (tree.SelectNode(this, TreeInvokeAction.Click))
            {
                return TreeDragSelectType.None;
            }

            return TreeDragSelectType.Node;
        }

        #endregion // OnNodeMouseDown

        #region HandleKeyDown

        /// <summary>
        /// Method called when a key action happens while the <see cref="XamDataTreeNode"/> is currently active.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected internal bool HandleKeyDown(KeyEventArgs e)
        {
            bool handled = false;

            Key key = e.Key;
            NodeLayout nodeLayout = this.NodeLayout;
            XamDataTree tree = nodeLayout.Tree;
            int visualIndex = tree.InternalNodes.IndexOf(this);
            int nodeCount = tree.InternalNodes.Count;
            bool ctrlKey = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


            if (tree.FlowDirection == FlowDirection.RightToLeft)
            {
                if (key == Key.Left)
                    key = Key.Right;
                else if (key == Key.Right)
                    key = Key.Left;
            }

            if (!this.IsEnabled)
                return false;

            TreeEditingSettingsOverride settings = nodeLayout.EditingSettings;
            TreeSelectionSettings selectionSettings = tree.SelectionSettings;
            switch (key)
            {
                case Key.Down:
                    {
                        if (!this.IsEditing)
                        {
                            if (visualIndex < nodeCount - 1)
                            {
                                int nextVisualIndex = visualIndex + 1;

                                XamDataTreeNode node = tree.InternalNodes[nextVisualIndex];
                                while (node != null)
                                {
                                    if (node.IsEnabled)
                                    {
                                        tree.SetActiveNode(node, TreeInvokeAction.Keyboard, true);
                                        if (!ctrlKey && selectionSettings.NodeSelection != TreeSelectionType.None)
                                            node.IsSelected = true;
                                        handled = true;
                                        break;
                                    }
                                    else
                                    {
                                        node = null;
                                    }

                                    ++nextVisualIndex;

                                    if (nextVisualIndex < nodeCount)
                                        node = tree.InternalNodes[nextVisualIndex];
                                }
                            }
                            handled = true;
                        }
                        break;
                    }
                case Key.Up:
                    {
                        if (!this.IsEditing)
                        {
                            if (visualIndex > 0)
                            {
                                int nextVisualIndex = visualIndex - 1;
                                XamDataTreeNode node = tree.InternalNodes[nextVisualIndex];
                                while (node != null)
                                {
                                    if (node.IsEnabled)
                                    {
                                        tree.SetActiveNode(node, TreeInvokeAction.Keyboard, true);
                                        if (!ctrlKey && selectionSettings.NodeSelection != TreeSelectionType.None)
                                            node.IsSelected = true;
                                        handled = true;
                                        break;
                                    }
                                    else
                                    {
                                        node = null;
                                    }
                                    --nextVisualIndex;
                                    if (nextVisualIndex >= 0)
                                        node = tree.InternalNodes[nextVisualIndex];
                                }
                            }
                            handled = true;
                        }
                        break;
                    }
                case Key.Left:
                    {
                        if (!this.IsEditing)
                        {
                            if (this.IsExpanded)
                                this.IsExpanded = false;
                            else
                            {
                                if (this.Manager.ParentNode != null)
                                {
                                    tree.SetActiveNode(this.Manager.ParentNode, TreeInvokeAction.Keyboard, true);
                                    if (!ctrlKey && selectionSettings.NodeSelection != TreeSelectionType.None)
                                        this.Manager.ParentNode.IsSelected = true;
                                }
                            }
                            handled = true;
                        }
                        break;
                    }
                case Key.Right:
                    {
                        if (!this.IsEditing)
                        {
                            if (this.HasChildren)
                            {
                                if (this.IsExpanded)
                                {
                                    tree.SetActiveNode(this.Nodes[0], TreeInvokeAction.Keyboard, true);
                                    if (!ctrlKey && selectionSettings.NodeSelection != TreeSelectionType.None)
                                        this.Nodes[0].IsSelected = true;
                                }
                                else
                                {
                                    this.IsExpanded = true;
                                }
                            }
                            handled = true;
                        }
                        break;
                    }
                case Key.Add:
                    {
                        if (this.HasChildren && !this.IsEditing)
                        {
                            this.IsExpanded = true;
                            handled = true;
                        }
                        break;
                    }
                case Key.Subtract:
                    {
                        if (this.HasChildren && !this.IsEditing)
                        {
                            this.IsExpanded = false;
                            handled = true;
                        }
                        break;
                    }
                case Key.Enter:

                    if (!this.IsEditing)
                    {
                        if (settings.AllowEditingResolved && settings.IsEnterKeyEditingEnabledResolved)
                        {
                            tree.EnterEditMode(this);
                            handled = true;
                        }
                        else if (this.HasChildren)
                        {
                            this.IsExpanded = !this.IsExpanded;
                            handled = true;
                        }
                    }
                    else if (!e.Handled)
                    {
                        tree.ExitEditMode(false);
                        handled = true;
                    }
                    break;
                case Key.Space:
                    {
                        if (!this.IsEditing)
                        {
                            if (this.NodeLayout.CheckBoxSettings.CheckBoxVisibilityResolved == Visibility.Visible)
                            {
                                if (this.NodeLayout.CheckBoxSettings.IsCheckBoxThreeStateResolved)
                                {
                                    if (this.IsChecked == true)
                                    {
                                        this.IsChecked = null;
                                    }
                                    else if (this.IsChecked == null)
                                    {
                                        this.IsChecked = false;
                                    }
                                    else
                                    {
                                        this.IsChecked = true;
                                    }
                                }
                                else
                                {
                                    if (this.IsChecked == null)
                                    {
                                        this.IsChecked = false;
                                    }
                                    else
                                    {
                                        this.IsChecked = !(bool)this.IsChecked;
                                    }
                                }
                            }
                            else
                            {
                                if (selectionSettings.NodeSelection != TreeSelectionType.None)
                                {
                                    if (this.IsSelected)
                                    {
                                        tree.UnselectNode(this);
                                    }
                                    else
                                    {
                                        tree.SelectNode(this, TreeInvokeAction.Keyboard);
                                    }
                                }
                            }
                            handled = true;
                        }
                    }
                    break;
                case Key.F2:
                    {
                        if (!this.IsEditing && settings.AllowEditingResolved && settings.IsF2EditingEnabledResolved)
                        {
                            tree.EnterEditMode(this);

                            handled = true;
                        }
                        break;
                    }
                case Key.Escape:
                    {
                        if (this.IsEditing)
                        {
                            tree.ExitEditMode(true);

                            handled = true;
                        }
                        break;
                    }
                case Key.Delete:
                    {
                        if (!this.IsEditing && settings.AllowDeletionResolved && !this.IsHeader && !this.Manager.NodeLayout.Tree.IsNodeDragging)
                        {
                            this.DeleteNode();

                            handled = true;
                        }

                        break;
                    }
            }

            return handled;
        }

        #endregion // HandleKeyDown

        #region SetSelected

        /// <summary>
        /// Sets the selected state of an item. 
        /// </summary>
        /// <param propertyName="isSelected"></param>
        protected internal virtual void SetSelected(bool isSelected)
        {
            if (this._isSelected == isSelected)
                return;

            this._isSelected = isSelected;

            if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                this.NodeLayout.Tree.InvalidateScrollPanel(false);

            this.OnPropertyChanged("IsSelected");
        }

        #endregion // SetSelected

        #region OnNodeDragging

        /// <summary>
        /// Method called when the <see cref="XamDataTreeNode"/> is being dragged.
        /// </summary>
        /// <param name="dragSelectType"></param>
        protected internal void OnNodeDragging(TreeDragSelectType dragSelectType)
        {
            if (!this.NodeLayout.Tree.IsNodeDragging && dragSelectType == TreeDragSelectType.Node)
            {
                this.NodeLayout.Tree.SelectNode(this, TreeInvokeAction.MouseMove);
            }
        }

        #endregion // OnNodeDragging

        #region OnNodeMouseMove

        /// <summary>
        /// Method called when the <see cref="XamDataTreeNode"/> is responding to a <see cref="UIElement.MouseMove"/>.
        /// </summary>
        /// <param name="e"></param>
        protected internal void OnNodeMouseMove(MouseEventArgs e)
        {
        }

        #endregion // OnNodeMouseMove

        #region ApplyStyle

        /// <summary>
        /// Method which evaluates which style to apply to the <see cref="XamDataTreeNode"/>.
        /// </summary>
        protected internal void ApplyStyle()
        {
            if (this.Control != null)
            {
                Style s = this.ResolveStyle;

                if (s != null)
                {

                    if (this.Control.Style != s)
                    {
                        this.EnsureCurrentState();
                        this.Control.Style = s;
                    }

                }
                else
                {
                    if (this.Control.Style != null)
                    {
                        this.Control.ClearValue(XamDataTreeNodeControl.StyleProperty);
                    }
                }

                Style checkBoxStyle = this.NodeLayout.CheckBoxSettings.CheckBoxStyleResolved;

                if (checkBoxStyle == null)
                    checkBoxStyle = this.Control.CheckBoxStyle;

                CheckBox cb = this.Control.CheckBox;
                if (cb != null && cb.Style != checkBoxStyle)
                {
                    if (checkBoxStyle == null)
                    {
                        cb.ClearValue(CheckBox.StyleProperty);
                    }
                    else
                    {
                        cb.Style = checkBoxStyle;
                    }
                }
            }
        }

        #endregion // ApplyStyle

        #region EnterEditMode

        /// <summary>
        /// Places the specified <see cref="XamDataTreeNode" /> into edit mode.
        /// </summary>
        protected internal virtual void EnterEditMode(bool cellIsEditing)
        {
            if (this.IsEditing || this.Control == null)
                return;

            if (this.NodeLayout.Tree.ExitEditMode(false))
            {
                if (cellIsEditing)
                    this.NodeLayout.Tree.CurrentEditNode = this;

                ((XamDataTreeNodeControl)this.Control).AddEditorToControl();
            }
        }

        #endregion // EnterEditMode

        #region ResolveEditor

        /// <summary>
        /// Sets up the edtior control that will be displayed in a <see cref="XamDataTreeNode" /> when the the cell is in edit mode.
        /// </summary>
        /// <param propertyName="cell">The <see cref="XamDataTreeNode" /> entering edit mode.</param>
        /// <param propertyName="valueChangedFunction">The function that should be called to notify when a value in the editor has changed.</param>
        /// <param propertyName="editorValue">The value that should be put in the editor.</param>
        /// <param propertyName="availableWidth">The amount of horizontal space available.</param>
        /// <param propertyName="availableHeight">The amound of vertical space available.</param>
        /// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
        /// <returns></returns>	
        protected internal FrameworkElement ResolveEditor(XamDataTreeNode cell, Action<object> valueChangedFunction, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            this._valueChangedFunction = valueChangedFunction;

            return this.ResolveEditorControl(cell, editorValue, availableWidth, availableHeight, editorBinding);
        }

        #endregion // ResolveEditor

        #region ResolveEditorControl

        /// <summary>
        /// Sets up the edtior control that will be displayed in a <see cref="XamDataTreeNode" /> when the the cell is in edit mode.
        /// </summary>
        /// <param propertyName="cell">The <see cref="XamDataTreeNode" /> entering edit mode.</param>
        /// <param propertyName="editorValue">The value that should be put in the editor.</param>
        /// <param propertyName="availableWidth">The amount of horizontal space available.</param>
        /// <param propertyName="availableHeight">The amound of vertical space available.</param>
        /// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
        /// <returns></returns>
        protected virtual FrameworkElement ResolveEditorControl(XamDataTreeNode cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
        {
            if (this.NodeLayout.EditorTemplate != null)
            {
                this._editor = this.NodeLayout.EditorTemplate.LoadContent() as FrameworkElement;
            }
            else
            {
                if (editorBinding != null)
                {
                    if (this._editor == null)
                    {
                        this._editor = new TextBox();
                    }

                    this._editor.Height = availableHeight;
                    this._editor.Width = availableWidth;

                    this._editor.SetBinding(TextBox.TextProperty, editorBinding);
                }
            }

            return this._editor;
        }

        #endregion // ResolveEditorControl

        #region NotifyEditorValueChanged

        /// <summary>
        /// Used to notify the owning cell when an editor's value has changed.
        /// </summary>
        /// <param propertyName="value"></param>
        protected void NotifyEditorValueChanged(object value)
        {
            if (this._valueChangedFunction != null)
                this._valueChangedFunction(value);
        }

        #endregion // NotifyEditorValueChanged

        #region ResolveEditorBinding

        /// <summary>
        /// Creates a <see cref="Binding"/> that can be applied to an editor.
        /// </summary>
        /// <returns></returns>
        protected internal virtual Binding ResolveEditorBinding()
        {
            Binding binding = null;

            if (this.Control != null)
            {
                binding = this.Control.ResolveEditorBinding();
            }
            if (binding == null)
            {
                object data = this.Data;
                if (!string.IsNullOrEmpty(this.NodeLayout.DisplayMemberPathResolved) && data != null)
                {
                    binding = new Binding(this.NodeLayout.DisplayMemberPathResolved);
                    binding.Source = data;

                    binding.ConverterCulture = CultureInfo.CurrentCulture;

                    binding.Mode = BindingMode.TwoWay;
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

                    binding.NotifyOnValidationError = true;
                    binding.ValidatesOnDataErrors = true;
                    binding.ValidatesOnExceptions = true;



                }
            }
            return binding;
        }

        #endregion // ResolveEditorBinding

        #region ResolveValueFromEditor

        /// <summary>
        /// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
        /// </summary>
        /// <param propertyName="cell">The <see cref="XamDataTreeNode" /> that the editor id being displayed in.</param>
        /// <returns>The value that should be displayed in the cell.</returns>
        protected internal virtual object ResolveValueFromEditor()
        {
            TextBox tb = this._editor as TextBox;

            if (tb != null)
                return tb.Text;

            return this.Value;
        }

        #endregion // ResolveValueFromEditor

        #region ExitEditMode

        /// <summary>
        /// Takes the specified <see cref="XamDataTreeNode" /> out of edit mode.
        /// </summary>
        /// <param propertyName="newValue">The value that should be entered in the <see cref="XamDataTreeNode" /></param>
        /// <param propertyName="editingCanceled">Whether or not we're exiting edit mode, because it was cancelled.</param>
        protected internal virtual bool ExitEditMode(object newValue, bool editingCanceled)
        {
            XamDataTreeNodeControl control = (XamDataTreeNodeControl)this.Control;

            if (editingCanceled || !control.HasEditingBindings)
            {
                NodeValueObject cellValueObj = new NodeValueObject();
                Binding b = CreateNodeValueBinding();
                if (b != null && b.Path != null && !string.IsNullOrEmpty(b.Path.Path))
                {
                    cellValueObj.SetBinding(NodeValueObject.ValueProperty, b);
                    cellValueObj.Value = newValue;
                }
            }
            else
            {
                if (!control.EvaluateEditingBindings())
                    return false;
            }

            if (control != null)
                control.RemoveEditorFromControl();

            if (this.Control != null && this.NodeLayout.Tree.Panel.VisibleNodes.Contains(this))
            {
                this.NodeLayout.Tree.Panel.ReleaseNode(this);
            }
            else if (this.Control != null)
            {
                this.OnElementAttached(this.Control);
                this.EnsureCurrentState();
            }
            return true;
        }

        #endregion // ExitEditMode

        #region CreateCellValueBinding

        /// <summary>
        /// Creates the binding used by the CellValueObject for updating
        /// </summary>
        /// <returns></returns>
        protected virtual Binding CreateNodeValueBinding()
        {
            Binding b = new Binding(this.NodeLayout.DisplayMemberPathResolved);
            b.Source = this.Data;
            b.Mode = BindingMode.TwoWay;
            b.ConverterCulture = CultureInfo.CurrentCulture;
            return b;
        }

        #endregion // CreateCellValueBinding

        #region RaiseIsActiveChanged

        /// <summary>
        /// Raises the notification for the <see cref="IsActive"/> property being changed.
        /// </summary>
        protected internal void RaiseIsActiveChanged()
        {
            this.RaiseNodeNotifyPropertyChanged("IsActive");
        }

        #endregion // RaiseIsActiveChanged

        #endregion // Protected

        #region Private

        #region ValidateChildNodesCheckState

        private void ValidateChildNodesCheckState()
        {
            if (this.HasChildren && this.IsChecked != null)
            {
                foreach (XamDataTreeNode node in this.Nodes)
                {
                    if (this.IsChecked != null)
                        node.SetCheckedSilent((bool)this.IsChecked);
                }
            }
        }

        #endregion // ValidateChildNodesCheckState

        #region SetCheckedSilent

        private void SetCheckedSilent(bool p)
        {
            _surpressParentChildNotification = true;
            this.IsChecked = p;
            _surpressParentChildNotification = false;
        }

        #endregion //SetCheckedSilent

        #region SetupCheckboxBinding

        internal void SetupCheckboxBinding()
        {
            this._surpressParentChildNotification = true;
            this._surpressIsCheckNotification = true;

            if (this.NodeLayout != null && this.Data != null && !string.IsNullOrEmpty(this.NodeLayout.CheckBoxMemberPathResolved))
            {
                this._checkboxBinding = new Binding(this.NodeLayout.CheckBoxMemberPathResolved);

                this._checkboxBinding.Source = this.Data;

                this._checkboxBinding.Mode = BindingMode.TwoWay;

                BindingOperations.SetBinding(this, XamDataTreeNode.IsCheckedProperty, this._checkboxBinding);
            }
            else
            {

                BindingOperations.ClearBinding(this, XamDataTreeNode.IsCheckedProperty);


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            }
            this._surpressParentChildNotification = false;
            this._surpressIsCheckNotification = false;
        }

        #endregion // SetupCheckboxBinding

        #region ValidateParentCheckedState

        internal void ValidateParentCheckedState()
        {
            bool trueFound = false;
            bool falseFound = false;

            foreach (XamDataTreeNode node in this.Nodes)
            {
                if (node.NodeLayout.CheckBoxSettings.CheckBoxVisibilityResolved == Visibility.Visible)
                {
                    if (node.IsChecked == null)
                    {
                        trueFound = true;
                        falseFound = true;
                        break;
                    }
                    else if ((bool)node.IsChecked)
                        trueFound = true;
                    else
                        falseFound = true;
                }
            }

            if (trueFound && falseFound)
            {
                this.IsChecked = null;
            }
            else if (trueFound)
            {
                this.IsChecked = true;
            }
            else
            {
                this.IsChecked = false;
            }

            this.OnPropertyChanged("IsChecked");
        }

        #endregion // ValidateParentCheckedState

		#region SetupIsEnabledBinding

		internal void SetupIsEnabledBinding()
		{
			if (this.NodeLayout != null && this.Data != null && !string.IsNullOrEmpty(this.NodeLayout.IsEnabledMemberPathResolved))
			{
				this._isEnabledBinding = new Binding(this.NodeLayout.IsEnabledMemberPathResolved);

				this._isEnabledBinding.Source = this.Data;

				this._isEnabledBinding.Mode = BindingMode.TwoWay;

				BindingOperations.SetBinding(this, XamDataTreeNode.IsEnabledProperty, this._isEnabledBinding);
			}
			else
			{

				BindingOperations.ClearBinding(this, XamDataTreeNode.IsEnabledProperty);


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			}
		}

		#endregion // SetupIsEnabledBinding

		#region SetupIsExpandedBinding

		internal void SetupIsExpandedBinding()
        {
            if (this.NodeLayout != null && this.Data != null && !string.IsNullOrEmpty(this.NodeLayout.IsExpandedMemberPathResolved))
            {
                this._isExpandedBinding = new Binding(this.NodeLayout.IsExpandedMemberPathResolved);

                this._isExpandedBinding.Source = this.Data;

                this._isExpandedBinding.Mode = BindingMode.TwoWay;

                BindingOperations.SetBinding(this, XamDataTreeNode.IsExpandedProperty, this._isExpandedBinding);
            }
            else
            {

                BindingOperations.ClearBinding(this, XamDataTreeNode.IsExpandedProperty);


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            }
        }

        #endregion // SetupIsExpandedBinding

        #region EnterEditModeViaMouseClick

        private void EnterEditModeViaMouseClick(TreeMouseEditingAction action)
        {
            TreeEditingSettingsOverride settings = this.NodeLayout.EditingSettings;
            if (settings.AllowEditingResolved && settings.IsMouseActionEditingEnabledResolved == action)
            {
                if (!this.IsEditing)
                {
                    this.NodeLayout.Tree.EnterEditMode(this);
                }
            }
        }

        #endregion // EnterEditModeViaMouseClick

        #region DeleteNode

        private void DeleteNode()
        {
            NodeLayout nodeLayout = this.NodeLayout;
            
            #region DeleteCode
            
            XamDataTreeNode newNodeToBeActivated = null;

            int count = this.Manager.Nodes.Count;

            int index = this.Manager.Nodes.IndexOf(this);

            if (count - 1 == 0) 
            {
                newNodeToBeActivated = this.Manager.ParentNode;
            }
            else
            {
                
                if (index == 0)
                {
                    for (int i = 1; i < count; i++)
                    {
                        XamDataTreeNode n = this.Manager.Nodes[i];
                        if (n.IsEnabled)
                        {
                            newNodeToBeActivated = n;
                            break;
                        }
                    }

                    if (newNodeToBeActivated == null)
                    {
                        newNodeToBeActivated = this.Manager.ParentNode;
                    }
                }

                else if (index == count - 1)
                {
                    for (int i = count - 2; i >= 0; i--)
                    {
                        XamDataTreeNode n = this.Manager.Nodes[i];
                        if (n.IsEnabled)
                        {
                            newNodeToBeActivated = n;
                            break;
                        }
                    }

                    if (newNodeToBeActivated == null)
                    {
                        newNodeToBeActivated = this.Manager.ParentNode;
                    }
                }
                else  
                {
                    for (int i = index + 1; i < count; i++)
                    {
                        XamDataTreeNode n = this.Manager.Nodes[i];
                        if (n.IsEnabled)
                        {
                            newNodeToBeActivated = n;
                            break;
                        }
                    }
                    if (newNodeToBeActivated == null)
                    {
                        for (int i = index - 1; i >= 0; i--)
                        {
                            XamDataTreeNode n = this.Manager.Nodes[i];
                            if (n.IsEnabled)
                            {
                                newNodeToBeActivated = n;
                                break;
                            }
                        }
                    }
                    if (newNodeToBeActivated == null)
                    {
                        newNodeToBeActivated = this.Manager.ParentNode;
                    }
                }
            }

            CancellableNodeDeletionEventArgs args = new CancellableNodeDeletionEventArgs() { Node = this };

            this.NodeLayout.Tree.OnNodeDeleting(args);

            if (args.Cancel)
            {
                return;
            }

            if (this.Manager.DataManager.RemoveRecord(this.Data))
            {
                if (this.IsSelected)
                    this.IsSelected = false;

                nodeLayout.Tree.SetActiveNode(newNodeToBeActivated, TreeInvokeAction.Code, true);

                nodeLayout.Tree.OnNodeDeleted(this);
            }

            #endregion // DeleteCode
        }

        #endregion // DeleteNode

        #region RaiseNodeNotifyPropertyChanged

        internal void RaiseNodeNotifyPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(propertyName);
        }

        #endregion // RaiseNodeNotifyPropertyChanged

        #region UpdateLevel
        internal void UpdateLevel()
        {
            if (this._childNodesManager != null && this.Manager != null)
            {
                this._childNodesManager.Level = this.Manager.Level + 1;

                foreach (XamDataTreeNode node in this.ChildNodesManager.Nodes)
                {
                    node.UpdateLevel();
                }
            }
        }
        #endregion // UpdateLevel

        #endregion // Private

        #endregion // Methods

        #region IBindableItem Members

        /// <summary>
        /// Gets/sets whether the <see cref="XamDataTreeNode"/> was generated via the datasource or was entered manually.
        /// </summary>
        protected virtual bool IsDataBound
        {
            get;
            set;
        }

        bool IBindableItem.IsDataBound
        {
            get { return this.IsDataBound; }
            set { this.IsDataBound = value; }
        }

        #endregion

        #region ISelectableItem Members

        bool ISelectableObject.IsSelected
        {
            get
            {
                return this.IsSelected;
            }
            set
            {
                this.IsSelected = value;
            }
        }

        void ISelectableObject.SetSelected(bool isSelected)
        {
            this.SetSelected(isSelected);
        }

        #endregion

        #region CellValueObject Class

        /// <summary>
        /// A Class used to store off the value of a <see cref="XamDataTreeNode"/>.
        /// </summary>
        internal class NodeValueObject : FrameworkElement
        {
            #region Value

            /// <summary>
            /// Identifies the <see cref="Value"/> dependency property. 
            /// </summary>
            public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(NodeValueObject), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

            public object Value
            {
                get { return (object)this.GetValue(ValueProperty); }
                set { this.SetValue(ValueProperty, value); }
            }

            private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
            {

            }

            #endregion // Value
        }

        #endregion // CellValueObject        
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