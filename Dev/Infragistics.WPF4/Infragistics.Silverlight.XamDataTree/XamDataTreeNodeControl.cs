using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Menus.Primitives;
using Infragistics.DragDrop;
using Infragistics;
using System.Diagnostics;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// An object that acts as the visual representaion of a <see cref="XamDataTreeNode"/> objects.
    /// </summary>
    [TemplateVisualState(GroupName = "ActiveStates", Name = "Inactive")]
    [TemplateVisualState(GroupName = "ActiveStates", Name = "Active")]
    [TemplateVisualState(GroupName = "SelectedStates", Name = "NotSelected")]
    [TemplateVisualState(GroupName = "SelectedStates", Name = "Selected")]
    [TemplateVisualState(GroupName = "EndLines", Name = "None")]
    [TemplateVisualState(GroupName = "EndLines", Name = "TShape")]
    [TemplateVisualState(GroupName = "EndLines", Name = "LShape")]
    [TemplateVisualState(GroupName = "DropStates", Name = "NoDrop")]
    [TemplateVisualState(GroupName = "DropStates", Name = "DropOnto")]
    [TemplateVisualState(GroupName = "DropStates", Name = "DropBefore")]
    [TemplateVisualState(GroupName = "DropStates", Name = "DropAfter")]
    [TemplateVisualState(GroupName = "ExpandedIconStates", Name = "ShowExpandedIcon")]
    [TemplateVisualState(GroupName = "ExpandedIconStates", Name = "HideIcons")]
    [TemplateVisualState(GroupName = "ExpandedIconStates", Name = "ShowCollapsedIcon")]
    [TemplateVisualState(GroupName = "DraggingStates", Name = "NotDragging")]
    [TemplateVisualState(GroupName = "DraggingStates", Name = "Dragging")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplatePart(Name = "CheckBox", Type = typeof(CheckBox))]
    [TemplatePart(Name = "ExpandedIcon", Type = typeof(DataTemplate))]
    [TemplatePart(Name = "CollapsedIcon", Type = typeof(DataTemplate))]
    [TemplatePart(Name = "ContentPresenter", Type = typeof(ContentPresenter))]
    public class XamDataTreeNodeControl : ContentControl, IRecyclableElement, INotifyPropertyChanged
    {
        #region Members

        private static XamDataTree _mouseHolder;
        private XamDataTree MouseHolder
        {
            get
            {
                return _mouseHolder;
            }
            set 
            {
                if (_mouseHolder != value)
                {
                    if (_mouseHolder != null)
                    {
                        this._dragSource.DragOver -= new EventHandler<DragDropMoveEventArgs>(_mouseHolder.DragSource_DragOver);
                    }

                    _mouseHolder = value;

                    if (_mouseHolder != null)
                    {
                        this._dragSource.DragOver += new EventHandler<DragDropMoveEventArgs>(_mouseHolder.DragSource_DragOver);
                    }
                }
            }
        }

        XamDataTreeNode _node;

        NodeLineTerminator _terminator;

        List<BindingDataInfo> _editorBindings;

        DragSource _dragSource;
        DropTarget _dropTarget;

        bool _bindingError;

        Thickness _padding;

        ContentControl _expandedIcon;
        ContentControl _collapsedIcon;

        ActiveNodeIndicator _activeNodeIndicator;

        private TreeDropDestination? _currentDropDestination = null;

        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="NodeLineTerminator"/> class.
        /// </summary>
        static XamDataTreeNodeControl()
        {
            Style style = new Style();
            style.Seal();
            Control.FocusVisualStyleProperty.OverrideMetadata(typeof(XamDataTreeNodeControl), new FrameworkPropertyMetadata(style));

            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamDataTreeNodeControl), new FrameworkPropertyMetadata(typeof(XamDataTreeNodeControl)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="XamDataTreeNodeControl"/> class.
        /// </summary>
        public XamDataTreeNodeControl()
        {





            //this.Focusable = false;
            this.SnapsToDevicePixels = true;
            this.IsTabStop = false;

        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region Node

        /// <summary>
        /// The <see cref="XamDataTreeNode"/> that owns the <see cref="XamDataTreeNodeControl"/>.
        /// </summary>
        public XamDataTreeNode Node
        {
            get
            {
                return this._node;
            }
            set
            {
                if (this._node != value)
                {
                    this._node = value;

                    if (this._node != null)
                    {
                        this._dragSource = new DragSource() { };
                        DragDropManager.SetDragSource(this, this._dragSource);

                        Binding b = new Binding("Node.IsDraggableResolved");
                        b.Mode = BindingMode.OneWay;
                        b.Source = this;
                        BindingOperations.SetBinding(this._dragSource, DragSource.IsDraggableProperty, b);

                        this._dragSource.DragStart += new EventHandler<DragDropStartEventArgs>(DragSource_DragStart);
                        this._dragSource.DragOver += new EventHandler<DragDropMoveEventArgs>(DragSource_DragOver);
                        this._dragSource.DragLeave += new EventHandler<DragDropEventArgs>(DragSource_DragLeave);
                        this._dragSource.DragEnter += new EventHandler<DragDropCancelEventArgs>(DragSource_DragEnter);
                        this._dragSource.DragCancel += new EventHandler<DragDropEventArgs>(DragSource_DragCancel);
                        this._dragSource.DragEnd += new EventHandler<DragDropEventArgs>(DragSource_DragEnd);
                        this._dragSource.Drop += new EventHandler<DropEventArgs>(DragSource_Drop);

                        this._dropTarget = new DropTarget() { };
                        DragDropManager.SetDropTarget(this, this._dropTarget);

                        Binding b2 = new Binding("Node.IsDropTargetResolved");
                        b2.Mode = BindingMode.OneWay;
                        b2.Source = this;
                        BindingOperations.SetBinding(this._dropTarget, DropTarget.IsDropTargetProperty, b2);
                    }
                    else
                    {

                        if (this._dragSource != null)
                            BindingOperations.ClearBinding(this._dragSource, DragSource.IsDraggableProperty);
                        if (this._dropTarget != null)
                            BindingOperations.ClearBinding(this._dropTarget, DropTarget.IsDropTargetProperty);

                        DragDropManager.SetDragSource(this, null);
                        DragDropManager.SetDropTarget(this, null);

                        this._dragSource.DragStart -= DragSource_DragStart;
                        this._dragSource.DragOver -= DragSource_DragOver;
                        this._dragSource.DragLeave -= DragSource_DragLeave;
                        this._dragSource.DragEnter -= DragSource_DragEnter;
                        this._dragSource.DragCancel -= DragSource_DragCancel;
                        this._dragSource.DragEnd -= DragSource_DragEnd;
                        this._dragSource.Drop -= DragSource_Drop;

                        this._dragSource = null;
                        this._dropTarget = null;
                    }

                    this.OnPropertyChanged("Node");
                }
            }
        }
        #endregion // Node

        #region CheckBoxStyle

        /// <summary>
        /// Identifies the <see cref="CheckBoxStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxStyleProperty = DependencyProperty.Register("CheckBoxStyle", typeof(Style), typeof(XamDataTreeNodeControl), new PropertyMetadata(new PropertyChangedCallback(CheckBoxStyleChanged)));

        /// <summary>
        /// Gets / sets a <see cref="Style"/> that will be applied to the checkboxs of the <see cref="XamDataTreeNodeControl"/>.
        /// </summary>
        public Style CheckBoxStyle
        {
            get { return (Style)this.GetValue(CheckBoxStyleProperty); }
            set { this.SetValue(CheckBoxStyleProperty, value); }
        }

        private static void CheckBoxStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDataTreeNodeControl ctrl = (XamDataTreeNodeControl)obj;
            ctrl.OnPropertyChanged("CheckBoxStyle");
        }

        #endregion // CheckBoxStyle

        #endregion // Public

        #region Protected

        #region DelayRecycling
        /// <summary>
        /// Get / set if the object should be recycled.
        /// </summary>
        protected virtual bool DelayRecycling
        {
            get;
            set;
        }
        #endregion // DelayRecycling

        #region OwnerPanel

        /// <summary>
        /// Gets/sets the <see cref="Panel"/> that owns this element. 
        /// </summary>
        protected virtual Panel OwnerPanel
        {
            get;
            set;
        }

        #endregion // OwnerPanel

        #region HasEditingBindings
        /// <summary>
        /// Resolves whether the <see cref="FrameworkElement"/> that is being used for editing has any <see cref="Binding"/> objects
        /// associated with it.
        /// </summary>
        /// <remarks>
        /// Note: this property is only valid when the <see cref="XamDataTreeNode" /> is currently in edit mode.
        /// </remarks>
        protected internal bool HasEditingBindings
        {
            get
            {
                return (this._editorBindings != null && this._editorBindings.Count > 0);
            }
        }
        #endregion // HasEditingBindings

        #region CheckBox

        /// <summary>
        /// Gets the <see cref="CheckBox"/> that should be in the template.
        /// </summary>
        protected internal CheckBox CheckBox
        {
            get;
            private set;
        }

        #endregion // CheckBox

        #endregion // Protected

        #region Internal

        #region ArrangeRaised

        internal bool ArrangeRaised
        {
            get;
            set;
        }

        #endregion // ArrangeRaised

        #region MeasureRaised

        internal bool MeasureRaised
        {
            get;
            set;
        }

        #endregion // MeasureRaised

        #region EditorAlreadyLoaded

        /// <summary>
        /// Gets / sets if the editor control for this <see cref="XamDataTreeNodeControl"/> is loaded.
        /// </summary>
        protected virtual bool EditorAlreadyLoaded
        {
            get;
            set;
        }

        #endregion // EditorAlreadyLoaded

        #region NodeLineControl
        internal NodeLineControl NodeLineControl
        {
            get;
            set;
        }
        #endregion // NodeLineControl

        #region ContentPresenter
        internal ContentPresenter ContentPresenter { get; set; }
        #endregion // ContentPresenter

        #region IsActiveDropTarget
        



        internal bool IsActiveDropTarget { get; private set; }
        #endregion // IsActiveDropTarget

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Protected

        #region OnAttached

        /// <summary>
        /// Called when the <see cref="XamDataTreeNode"/> is attached to the <see cref="XamDataTreeNodeControl"/>.
        /// </summary>
        /// <param propertyName="cell">The <see cref="XamDataTreeNode"/> that is being attached to the <see cref="XamDataTreeNodeControl"/></param>
        protected internal virtual void OnAttached(XamDataTreeNode node)
        {
            this.Node = node;
            XamDataTreeNodeDataContext dataContext = new XamDataTreeNodeDataContext();
            dataContext.Data = node.Data;
            dataContext.Node = node;
            this.DataContext = dataContext;
            this.AttachContent();

            Binding b = new Binding("IsEnabled");
            b.Source = node;
            this.SetBinding(XamDataTreeNodeControl.IsEnabledProperty, b);

            this.Measure(new Size(1, 1));
        }

        #endregion // OnAttached

        #region OnReleased

        /// <summary>
        /// Called when the <see cref="XamDataTreeNode"/> releases the <see cref="XamDataTreeNodeControl"/>.
        /// </summary>
        protected internal virtual void OnReleased(XamDataTreeNode node)
        {
            if (node != null)
            {
                node.EnsureCurrentState();
            }

            VisualStateManager.GoToState(this, "None", false);
            VisualStateManager.GoToState(this, "NoDrop", false);
            this.IsActiveDropTarget = false;

            this.ClearValue(XamDataTreeNodeControl.ContentTemplateProperty);

            this.DataContext = null;
            this.ReleaseContent();
            this.Node = null;

            this.ClearValue(XamDataTreeNodeControl.ContentProperty);
            this.ClearValue(Control.IsEnabledProperty);

            this.Measure(new Size(1, 1));
        }
        #endregion // OnReleased

        #region AttachContent

        /// <summary>
        /// Invoked when content is attached to the Control.
        /// </summary>
        protected virtual void AttachContent()
        {
            this.ResolveDisplayElement();

            this.LoadCollapsedExpandedIcons();

            this.SetActiveIndicatorStyle();

            if (this.Node != null && this.Node.IsActive)
            {
                Control content = this.Content as Control;
                if (content != null && content.IsHitTestVisible)
                    content.Focus();
            }
        }
        #endregion // AttachContent

        #region ReleaseContent

        /// <summary>
        /// Invoked before content is released from the control.
        /// </summary>
        protected virtual void ReleaseContent()
        {
            if (this.Node.IsEditing)
            {
                this.Node.NodeLayout.Tree.ExitEditMode(true);
                this.Padding = this._padding;
            }
        }
        #endregion // ReleaseContent

        #region AddEditorToControl

        /// <summary>
        /// Method called when the <see cref="XamDataTreeNodeControl"/> needs to change it's internal control structure to allow for editing.
        /// </summary>
        protected internal virtual void AddEditorToControl()
        {
            double height = this.ActualHeight;
            double width = this.ActualWidth;

            this.EditorAlreadyLoaded = false;
            XamDataTreeNode node = this.Node;

            if (node != null)
            {
                object dataValue = node.Value;
                object obj = this.ResolveEditorNodeValue(dataValue);

                if (obj != null && !obj.Equals(dataValue))
                    this.EditorAlreadyLoaded = true;

                this._padding = this.Padding;
                this.Padding = new Thickness();

                double availHeight = height - (this.BorderThickness.Top + this.BorderThickness.Bottom + this.Padding.Top + this.Padding.Bottom);
                double availWidth = width - (this.BorderThickness.Left + this.BorderThickness.Right + this.Padding.Left + this.Padding.Right);

                availHeight = this.DesiredSize.Height;

                if (availHeight <= 0)
                    availHeight = double.NaN;

                availWidth += this.DesiredSize.Width;

                if (availWidth <= 0)
                    availWidth = double.NaN;

                FrameworkElement editor = this.Node.ResolveEditor(node, null, obj, availWidth, availHeight, this.Node.ResolveEditorBinding());

                if (editor != null)
                {
                    // Check to see if the focused element is a control, if so, then call ReleaseMouseCapture, 
                    // Just in case it has captured the mouse, otherwise, when we remove it from the visual tree, it could throw an exception.
                    Control ctrl = PlatformProxy.GetFocusedElement(this) as Control;
                    if (ctrl != null)
                        ctrl.ReleaseMouseCapture();

                    this._editorBindings = XamDataTreeNodeControl.ResolveBindingsFromChildren(editor, true);
                    if (this.Content != editor)
                    {
                        this.Content = editor;
                        editor.Loaded += new RoutedEventHandler(Editor_Loaded);
                    }

                    


                    this.ClearValue(XamDataTreeNodeControl.ContentTemplateProperty);

                }
            }
        }

        #endregion // AddEditorToControl

        #region ResolveDisplayElement

        /// <summary>
        /// Method called when the <see cref="XamDataTreeNodeControl"/> needs to change it's internal control structure to display data.
        /// </summary>
        protected internal void ResolveDisplayElement()
        {
            if (!this.Node.IsHeader)
            {
                DataTemplate template = this.Node.NodeLayout.ItemTemplateResolved;
                if (template != null)
                {



                    Binding b = new Binding();
                    b.Path = new PropertyPath(XamDataTreeNodeControl.DataContextProperty);
                    b.Source = this;
                    b.Mode = BindingMode.OneWay;
                    this.SetBinding(XamDataTreeNodeControl.ContentProperty, b);
                    this.ContentTemplate = template;

                }
                else
                {
                    Binding b = new Binding("Data." + this.Node.NodeLayout.DisplayMemberPathResolved);
                    this.SetBinding(XamDataTreeNodeControl.ContentProperty, b);
                }
            }
            else
            {
                object obj = this.Node.NodeLayout.HeaderContentResolved;
                DataTemplate dt = obj as DataTemplate;
                if (dt != null)
                    this.Content = dt.LoadContent();
                else
                    this.Content = obj;
            }
        }

        #endregion // ResolveDisplayElement

        #region ResolveEditorNodeValue

        /// <summary>
        /// Determines the value that will be used as the text for the editor control.
        /// </summary>
        /// <param propertyName="dataValue"></param>
        /// <returns></returns>
        protected virtual object ResolveEditorNodeValue(object dataValue)
        {
            XamDataTreeNode c = this.Node;
            Dictionary<string, object> values = c.NodeLayout.Tree.EditNodeValues;

            object obj = null;
            if (values != null && values.ContainsKey(c.NodeLayout.DisplayMemberPathResolved))
            {
                obj = values[c.NodeLayout.DisplayMemberPathResolved];
            }
            else
                obj = dataValue;

            return obj;
        }

        #endregion // ResolveEditorNodeValue

        #region ResolveEditorBinding

        /// <summary>
        /// Creates a <see cref="Binding"/> that can be applied to an editor.
        /// </summary>
        /// <remarks>This will be called during the ContentProvider ResolveBinding and is not to be called directly otherwise.</remarks>
        /// <returns></returns>
        protected internal virtual Binding ResolveEditorBinding()
        {
            return null;
        }

        #endregion // ResolveEditorBinding

        #region EvaluateEditingBindings
        /// <summary>
        /// Loops through all the <see cref="Binding"/> objects that are associated with an editor, and determines
        /// if there is a binding error. 
        /// </summary>
        /// <returns>False if a binding error is found.</returns>
        protected internal bool EvaluateEditingBindings()
        {
            if (this._editorBindings != null)
            {
                foreach (BindingDataInfo data in this._editorBindings)
                {



                    Validation.AddErrorHandler(data.Element, Element_BindingValidationError);

                    data.Expression.UpdateSource();



                    Validation.RemoveErrorHandler(data.Element, Element_BindingValidationError);

                }
                if (this._bindingError)
                {
                    this._bindingError = false;
                    return false;
                }


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

            }
            return true;
        }
        #endregion // EvaluateEditingBindings

        #region RemoveEditorFromControl

        /// <summary>
        /// Used during inline editing, cleans up the cell control restoring it to display the data of the cell.
        /// </summary>
        protected internal virtual void RemoveEditorFromControl()
        {
            if (this.Node != null)
            {
                FrameworkElement editor = this.Content as FrameworkElement;

                if (editor != null)
                {
                    editor.Loaded -= Editor_Loaded;
                }

                bool wasEditorInNode = XamDataTreeNodeControl.IsFocusedControlInsideEditor(editor as Control);

                this.ResolveDisplayElement();

                bool focus = false;
                // Make sure that the editor no longer has focus, speficially, if the editor is always visible
                // such as a checkbox. 
                if (this.Node == this.Node.NodeLayout.Tree.ActiveNode && wasEditorInNode)
                    focus = this.Focus();

                if (!focus)
                    focus = this.Node.NodeLayout.Tree.Focus();


                this.Padding = this._padding;
            }
        }

        #endregion // RemoveEditorFromControl

        #region OnEditorLoaded

        /// <summary>
        /// This method is raised when an editor has fired it's Loaded event.
        /// And should be used to set focus and intialize anything in the editor that needs to be initialized.
        /// </summary>
        /// <param propertyName="editor"></param>
        protected virtual void OnEditorLoaded(FrameworkElement editor)
        {
            this.FocusOnEditorElement(editor);

            // This line of code ensures that the editor will actually be rendered inside of the cell
            // If you enter and exit edit mode a few times, the editor's ActualHeight and ActualWidth may resolve to 0.0
            // I'm not exactly sure why this occurs, but it appears to be a timing issue. 
            // The following line code solves that problem, by ensuring that everything is rendered properly for this particular cell. 
            //            this.Node.Control.RenderCell(this.Cell);

            // To Continue on the same issue, sometimes RenderCell doesn't event work, in which case we need a way to triggering 
            // The height and width of the editor to be recalculated.  This code appears to do the trick.  Merely calling UpdateLayout
            // has no effect, however, once we touch the Height and Width, it seems to work. 
            if ((editor.ActualHeight == 0 || editor.ActualWidth == 0) && double.IsNaN(editor.Height) && double.IsNaN(editor.Width))
            {
                editor.Height = 1;
                editor.Width = 1;
                editor.UpdateLayout();
                editor.Height = double.NaN;
                editor.Width = double.NaN;
            }
        }

        #endregion // OnEditorLoaded

        #endregion // Protected

        #region Internal

        internal void InternalCellMouseLeave(XamDataTreeNode xamDataTreeNode)
        {
            this.Node.EnsureCurrentState();
        }

        internal void InternalCellMouseEnter(XamDataTreeNode xamDataTreeNode)
        {
            this.Node.EnsureCurrentState();
        }

        #endregion // Internal

        #region Private

        #region ProcessDrop

        private void ProcessDrop(DropEventArgs e)
        {
            if (e.OperationType == OperationType.Move)
            {
                IList sourceList = this.GetIList(this.Node.Manager.ItemsSource);
                XamDataTreeNodesCollection sourceNodesCollection = this.Node.Manager.Nodes;
                XamDataTreeNodesCollection targetNodesCollection = null;

                if (sourceList == null)
                    return;

                bool addAsChild = false;
                bool addBeforeDestination = true;

                IList destinationList = null;
                XamDataTree targetTree = null;
                XamDataTreeNodeControl destNodeControl = e.DropTarget as XamDataTreeNodeControl;

                if (destNodeControl != null && destNodeControl.Node != null)
                {
                    XamDataTreeNode destinationNode = destNodeControl.Node;

                    targetTree = destNodeControl.Node.Manager.NodeLayout.Tree;

                    destinationList = this.GetIList(destinationNode.Manager.ItemsSource);
                    targetNodesCollection = destinationNode.Manager.Nodes;

                    if (destinationList == null)
                    {
                        if (destinationNode.ChildNodesManager != null &&
                            destinationNode.ChildNodesManager.DataManager != null)
                        {
                            destinationList = this.GetIList(destinationNode.ChildNodesManager.ItemsSource);
                            targetNodesCollection = destinationNode.ChildNodesManager.Nodes;
                            if (targetTree == this.Node.NodeLayout.Tree)
                            {
                                destNodeControl.Node.ChildNodesManager.RegisterCachedNode(this.Node);
                            }
                        }
                        else
                        {
                            if (targetTree == this.Node.NodeLayout.Tree)
                            {
                                destNodeControl.Node.Manager.RegisterCachedNode(this.Node);
                            }
                        }
                    }

                    if (destinationList != null)
                    {
                        double y = e.GetPosition(destNodeControl).Y;

                        double height = (destNodeControl).ActualHeight;

                        if (y <= height * 0.33333)
                        {
                            addBeforeDestination = true;
                        }
                        else if (y >= height * 0.66666)
                        {
                            addBeforeDestination = false;
                        }
                        else
                        {
                            addAsChild = true;
                            destinationList = this.GetIList(destinationNode.ChildNodesManager.ItemsSource);
                            targetNodesCollection = destinationNode.ChildNodesManager.Nodes;
                        }
                    }
                }
                else
                {
                    XamDataTree destTree = e.DropTarget as XamDataTree;

                    if (destTree != null)
                    {
                        destinationList = this.GetIList(destTree.ItemsSource);
                        targetNodesCollection = destTree.Nodes;
                    }

                    targetTree = destTree;

                    if (targetTree == this.Node.NodeLayout.Tree)
                    {
                        destTree.NodesManager.RegisterCachedNode(this.Node);
                    }
                }

                if (destNodeControl != null && this.Node == destNodeControl.Node)
                    return;

                if (destinationList == null)
                    return;

                bool sameTree = targetTree == this.Node.NodeLayout.Tree;

                if (addAsChild)
                {
                    if (sameTree)
                    {
                        sourceNodesCollection.Remove(this.Node);
                        targetNodesCollection.Add(this.Node);
                    }
                    else
                    {
                        sourceList.Remove(this.Node.Data);
                        destinationList.Add(this.Node.Data);
                    }
                    this.Node.UpdateLevel();
                }
                else if (addBeforeDestination)
                {                    
                    if (destNodeControl != null)
                    {
                        if (!this.EnsureNodeCanBeAddedToCollection(destNodeControl.Node.Manager.DataManager))
                            return;
                    }
                    else if (!sameTree)
                    {
                        if (!this.EnsureNodeCanBeAddedToCollection(targetTree.NodesManager.DataManager))
                            return;
                    }

                    if (sameTree)
                    {
                        if (destNodeControl == null)
                            return;

                        int sourceIndex = this.Node.Index;
                        int destIndex = destNodeControl.Node.Index;

                        if (sourceNodesCollection == targetNodesCollection)
                        {
                            if (sourceIndex < destIndex)
                                destIndex--;
                        }

                        sourceNodesCollection.Remove(this.Node);
                        int insertionIndex = destIndex;
                        if (insertionIndex < 0)
                            insertionIndex = 0;
                        targetNodesCollection.Insert(insertionIndex, this.Node);
                    }
                    else
                    {
                        object o = this.Node.Data;
                        sourceList.Remove(o);
                        int insertionIndex = destNodeControl != null ? destinationList.IndexOf(destNodeControl.Node.Data) : 0;
                        destinationList.Insert(insertionIndex, o);
                    }
                }
                else
                {
                    if (!this.EnsureNodeCanBeAddedToCollection(destNodeControl.Node.Manager.DataManager))
                        return;

                    if (sameTree)
                    {
                        if (destNodeControl == null)
                            return;

                        int sourceIndex = this.Node.Index;
                        int destIndex = destNodeControl.Node.Index;
                        if (sourceNodesCollection == targetNodesCollection)
                        {
                            if (sourceIndex < destIndex)
                                destIndex--;
                        }
                        sourceNodesCollection.Remove(this.Node);
                        int insertionIndex = destIndex;
                        if (insertionIndex >= targetNodesCollection.Count)
                            targetNodesCollection.Add(this.Node);
                        else
                            targetNodesCollection.Insert(insertionIndex + 1, this.Node);
                    }
                    else
                    {
                        object o = this.Node.Data;
                        sourceList.Remove(o);
                        int insertionIndex = destinationList.IndexOf(destNodeControl.Node.Data);
                        if (insertionIndex >= destinationList.Count)
                            destinationList.Add(o);
                        else
                            destinationList.Insert(insertionIndex + 1, o);
                    }
                }
            }
            if (this.Node != null)
            {
                this.Node.JustMoved = true;
                Size s = this.RenderSize;
                this.Measure(new Size(1, 1));
                this.Measure(s);               
                this.Node.JustMoved = false;

                foreach (XamDataTreeNode child in this.Node.Manager.Nodes)
                {
                    // Just walking through to ensure visible.
                }
            }
        }


        #endregion // ProcessDrop

        #region EnsureNodeNotBeingAssignedToChild

        private bool EnsureNodeNotBeingAssignedToChild(XamDataTreeNodeControl destination)
        {
            bool valid = true;

            if (destination == null)
            {
                valid = false;
            }
            else
            {
                XamDataTreeNode parentNode = destination.Node;

                while (parentNode != null)
                {
                    if (parentNode == this.Node)
                    {
                        valid = false;
                        break;
                    }

                    parentNode = parentNode.Manager.ParentNode;
                }
            }

            return valid;
        }

        #endregion // EnsureNodeNotBeingAssignedToChild

        #region EnsureNodeListsCanHandleMoving

        private bool EnsureNodeListsCanHandleMoving(XamDataTreeNodeControl destinationNodeControl, DragDropMoveEventArgs e)
        {
            bool valid = true;

            Point pt = e.GetPosition(destinationNodeControl);

            double height = destinationNodeControl.ActualHeight;

            IList iList = this.GetIList(this.Node.Manager.ItemsSource);

            if (iList == null || iList.IsReadOnly)
            {
                valid = false;
            }

            if (pt.Y <= height * 0.33333 || pt.Y >= height * 0.66666)
            {
                
                if (destinationNodeControl.Node.Manager.DataManager != null && this.Node.Manager.DataManager != null && destinationNodeControl.Node.Manager.DataManager.CachedType != this.Node.Manager.DataManager.CachedType)
                {
                    valid = false;
                }

                iList = this.GetIList(destinationNodeControl.Node.Manager.ItemsSource);
                if (iList == null || iList.IsReadOnly)
                {
                    valid = false;
                }
            }
            else
            {
                if (destinationNodeControl.Node.ChildNodesManager.DataManager != null && this.Node.Manager.DataManager != null)
                {
                    if (destinationNodeControl.Node.ChildNodesManager.DataManager.CachedType != this.Node.Manager.DataManager.CachedType)
                    {
                        valid = false;
                    }
                    else
                    {
                        iList = this.GetIList(destinationNodeControl.Node.ChildNodesManager.ItemsSource);
                        if (iList == null || iList.IsReadOnly)
                        {
                            valid = false;
                        }
                    }
                }
                else
                {
                    valid = false;
                }
            }

            return valid;
        }

        #endregion // EnsureNodeListsCanHandleMoving

        #region LoadCollapsedExpandedIcons

        private void LoadCollapsedExpandedIcons()
        {
            if (this._collapsedIcon != null && this.Node != null)
            {
                DataTemplate collapsedIconResolved = this.Node.CollapsedIconTemplateResolved;
                if (collapsedIconResolved != null)
                {
                    this._collapsedIcon.Content = collapsedIconResolved.LoadContent();
                }
                else
                {
                    this._collapsedIcon.Content = null;
                }
            }

            if (this._expandedIcon != null && this.Node != null)
            {
                DataTemplate expandedIconResolved = this.Node.ExpandedIconTemplateResolved;
                if (expandedIconResolved != null)
                {
                    this._expandedIcon.Content = expandedIconResolved.LoadContent();
                }
                else
                {
                    this._expandedIcon.Content = null;
                }
            }
        }

        #endregion // LoadCollapsedExpandedIcons

        #region UndragVisualState
        private void UndragVisualState()
        {
            VisualStateManager.GoToState(this, "NotDragging", false);
        }
        #endregion // UndragVisualState

        #region FocusOnEditorElement

        private bool FocusOnEditorElement(FrameworkElement editor)
        {
            Control ctrl = editor as Control;

            if (ctrl != null)
            {
                if (this.Node != null && this.Node.IsEditing && editor.IsHitTestVisible)
                {
                    if (!XamDataTreeNodeControl.IsFocusedControlInsideEditor(ctrl))
                    {
                        ctrl.Focus();
                        return true;
                    }
                }
            }
            else
            {
                Panel panel = editor as Panel;

                if (panel != null)
                {
                    foreach (UIElement child in panel.Children)
                    {
                        if (FocusOnEditorElement(child as FrameworkElement))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion // FocusOnEditorElement

        #region SetActiveIndicatorStyle

        private void SetActiveIndicatorStyle()
        {
            if (this.Node != null && this._activeNodeIndicator != null && this._activeNodeIndicator.Style != this.Node.NodeLayout.Tree.ActiveNodeIndicatorStyle)
            {
                Style s = this.Node.NodeLayout.Tree.ActiveNodeIndicatorStyle;
                if (this._activeNodeIndicator.Style != s)
                {
                    if (s == null)
                        this._activeNodeIndicator.ClearValue(ActiveNodeIndicator.StyleProperty);
                    else
                        this._activeNodeIndicator.Style = s;
                }
            }
        }
        #endregion // SetActiveIndicatorStyle

        #region EnsureNodeCanBeAddedToCollection
        private bool EnsureNodeCanBeAddedToCollection(DataManagerBase destinationDataManager)
        {
            return destinationDataManager.CachedType.IsAssignableFrom(this.Node.Manager.DataManager.CachedType);
        }

        internal static bool EnsureNodeCanBeAddedToCollection(DataManagerBase destinationDataManager, XamDataTreeNode node)
        {
            if (destinationDataManager == null)
                return false;
            return destinationDataManager.CachedType.IsAssignableFrom(node.Manager.DataManager.CachedType);
        }
        #endregion //EnsureNodeCanBeAddedToCollection

        #region GetIList

        private IList GetIList(IEnumerable source)
        {
            IList iList = source as IList;


            if (iList == null && source is IListSource)
            {
                iList = ((IListSource)source).GetList();
            }


            return iList;
        }

        #endregion // GetIList

        #endregion // Private

        #region Static

        #region ResolveBindingsFromChildren
        /// <summary>
        /// Loops through a <see cref="FrameworkElement"/>'s children, and find all <see cref="Binding"/>s that are assoicated with them.
        /// </summary>
        /// <param propertyName="element">The element to recurse through</param>
        /// <param propertyName="forEditing">Whether certain element's should be traversed.</param>
        /// <returns></returns>
        internal static List<BindingDataInfo> ResolveBindingsFromChildren(FrameworkElement element, bool forEditing)
        {
            List<BindingDataInfo> bindings = new List<BindingDataInfo>();
            bindings.AddRange(XamDataTreeNodeControl.ResovingBindingData(element, forEditing));

            int children = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < children; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                if (child != null)
                {
                    bindings.AddRange(XamDataTreeNodeControl.ResolveBindingsFromChildren(child, forEditing));
                }
            }

            return bindings;
        }
        #endregion // ResolveBindingsFromChildren

        #region GetDependencyProperties

        private static List<DependencyProperty> GetDependencyProperties(FrameworkElement element, bool forEditing)
        {
            List<DependencyProperty> list = new List<DependencyProperty>();
            if (!forEditing || ((((!(element is Panel) && !(element is Button)) && (!(element is Image) && !(element is ScrollViewer))) && ((!(element is TextBlock) && !(element is Border)) && !(element is Shape))) && !(element is ContentPresenter)))
            {
                FieldInfo[] fields = element.GetType().GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
                foreach (FieldInfo info in fields)
                {
                    if (info.FieldType == typeof(DependencyProperty))
                    {
                        list.Add((DependencyProperty)info.GetValue(null));
                    }
                }
            }
            return list;
        }

        #endregion // GetDependencyProperties

        #region ResovingBindingData

        private static List<BindingDataInfo> ResovingBindingData(FrameworkElement element, bool forEditing)
        {
            List<BindingDataInfo> data = new List<BindingDataInfo>();
            List<DependencyProperty> properties = XamDataTreeNodeControl.GetDependencyProperties(element, forEditing);

            if (properties.Count > 0)
            {
                foreach (DependencyProperty property in properties)
                {
                    BindingExpression expression = element.GetBindingExpression(property);
                    if (expression != null)
                    {
                        if (expression.ParentBinding != null)
                        {
                            if (expression.ParentBinding.Mode == BindingMode.TwoWay)
                            {
                                data.Add(new BindingDataInfo() { Element = element, Expression = expression });
                            }
                        }
                        else
                        {
                            data.Add(new BindingDataInfo() { Element = element, Expression = expression });
                        }
                    }
                }
            }

            return data;
        }

        #endregion // ResovingBindingData

        #region IsFocusedControlInsideEditor

        internal static bool IsFocusedControlInsideEditor(Control ctrl)
        {
            if (ctrl != null)
            {
                DependencyObject dp = PlatformProxy.GetFocusedElement(ctrl) as DependencyObject;
                FrameworkElement oldParent;
                while (dp != null)
                {
                    if (dp == ctrl)
                    {
                        return true;
                    }

                    oldParent = dp as FrameworkElement;

                    
                    dp = PlatformProxy.GetParent(dp);
                }
            }
            return false;
        }

        #endregion // IsFocusedControlInsideEditor

        #endregion // Static

        #endregion // Methods

        #region Override

        #region ArrangeOverride
        /// <summary>
        /// Arranges the visual representaion of the control.        
        /// </summary>
        /// <param propertyName="finalSize">
        /// The final area within the parent that this object 
        /// should use to arrange itself and its children.
        /// </param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this.ArrangeRaised = true;
            return base.ArrangeOverride(finalSize);
        }
        #endregion // ArrangeOverride

        #region MeasureOverride
        /// <summary>
        /// Measures the visual representation of the control.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            this.MeasureRaised = true;
            return base.MeasureOverride(constraint);
        }
        #endregion // MeasureOverride

        #region OnMouseEnter

        /// <summary>
        /// Called before the <see cref="UIElement.MouseEnter"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            VisualStateManager.GoToState(this, "NodeMouseOver", false);
        }

        #endregion

        #region OnMouseLeave

        /// <summary>
        /// Called before the <see cref="UIElement.MouseLeave"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            
            VisualStateManager.GoToState(this, "NodeMouseGone", false);
            if (this.Node != null)
                this.Node.EnsureCurrentState();
        }

        #endregion
        
        #region OnApplyTemplate

        /// <summary>
        /// Builds the visual tree for the <see cref="XamDataTreeNodeControl"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            _terminator = base.GetTemplateChild("LineTerminator") as NodeLineTerminator;

            this.CheckBox = base.GetTemplateChild("Checkbox") as CheckBox;

            if (this.CheckBox != null && this.Node != null)
            {
                this.Node.ApplyStyle();
            }

            this._collapsedIcon = base.GetTemplateChild("CollapsedIcon") as ContentControl;

            this._expandedIcon = base.GetTemplateChild("ExpandedIcon") as ContentControl;

            this._activeNodeIndicator = base.GetTemplateChild("ActiveBorder") as ActiveNodeIndicator;

            this.SetActiveIndicatorStyle();

            this.LoadCollapsedExpandedIcons();

            this.NodeLineControl = base.GetTemplateChild("lineControl") as NodeLineControl;

            this.ContentPresenter = base.GetTemplateChild("ContentPresenter") as ContentPresenter;
        }

        #endregion // OnApplyTemplate

        #endregion // Override

        #region EventHandlers

        void DragSource_Drop(object sender, DropEventArgs e)
        {
            XamDataTreeNodeControl node = e.DropTarget as XamDataTreeNodeControl;

            if (node != null)
            {
                VisualStateManager.GoToState(node, "NoDrop", false);

                if (!node.IsActiveDropTarget)
                {
                    _currentDropDestination = null;
                    return;
                }
            }

            TreeDropEventArgs args = new TreeDropEventArgs(e);

            if (this._currentDropDestination != null)
                args.DropDestination = (TreeDropDestination)this._currentDropDestination;
            else
                args.DropDestination = TreeDropDestination.DropOnto;

            _currentDropDestination = null;

            this.Node.NodeLayout.Tree.OnNodeDragDrop(args);

            if (!args.Handled)
                this.ProcessDrop(e);
        }

        void DragSource_DragEnd(object sender, DragDropEventArgs e)
        {
            XamDataTree tree = this.Node.NodeLayout.Tree;
            tree.SetupPossibleExpandingNode(null);
            tree.CurrentDraggingNode = null;
            tree.OnNodeDragEnd(e);

            if (this.MouseHolder != null)
            {
                this.MouseHolder.ReleaseMouseCapture();
                this.MouseHolder = null;
            }
        }

        void DragSource_DragCancel(object sender, DragDropEventArgs e)
        {
            this.Node.NodeLayout.Tree.CancelDragScrolling();
            this.Node.NodeLayout.Tree.SetupPossibleExpandingNode(null);
            XamDataTreeNodeControl node = e.DropTarget as XamDataTreeNodeControl;
            if (node != null)
            {
                VisualStateManager.GoToState(node, "NoDrop", false);
                _currentDropDestination = null;
            }
            this.Node.NodeLayout.Tree.OnNodeDragCancel(e);
        }

        void DragSource_DragEnter(object sender, DragDropCancelEventArgs e)
        {
            XamDataTreeNodeControl targetNode = e.DropTarget as XamDataTreeNodeControl;
            XamDataTreeNodeControl sourceNode = e.DragSource as XamDataTreeNodeControl;
            XamDataTree destTree = e.DropTarget as XamDataTree;

            if (targetNode != null)
            {
                VisualStateManager.GoToState(targetNode, "NoDrop", false);
            }
            _currentDropDestination = null;

            if (targetNode != null && sourceNode != null)
            {
                // we have drag-drop from one tree to another
                if (targetNode.Node.NodeLayout.Tree != sourceNode.Node.NodeLayout.Tree)
                {

                    bool isMouseCaptured = sourceNode.IsMouseCaptured;



                    // we can redirect the mouse capture when:
                    // 1. mouse is captured by our drag-and-drop (drag-drop will catch the change automatically)
                    // 2. we already have passed once through this procedure
                    if (isMouseCaptured || this.MouseHolder != null)
                    {
                        if (this.MouseHolder == null)
                        {
                            // first time switching between trees
                            sourceNode.Node.NodeLayout.Tree.CurrentDraggingNode = null;
                        }
                        else
                        {
                            this.MouseHolder.CurrentDraggingNode = null;
                        }

                        // keep the latest tree which has mouse captured
                        // this resolves:
                        // 1. the case when we drag from tree1 to tree2, then to tree3 and etc.
                        // 2. we need to release the mouse when drag-drop ends


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

                        this.MouseHolder = targetNode.Node.NodeLayout.Tree;

                        this.MouseHolder.CaptureMouse();

                        this.MouseHolder.CurrentDraggingNode = sourceNode.Node;
                    }
                }
                else
                {
                    // we are into the original treee
                    if (this.MouseHolder != null)
                    {
                        this.MouseHolder.CurrentDraggingNode = null;





                        this.MouseHolder = targetNode.Node.NodeLayout.Tree;

                        this.MouseHolder.CaptureMouse();

                        this.MouseHolder.CurrentDraggingNode = sourceNode.Node;
                    }
                }
            }
            else if (sourceNode != null && destTree != null)
            {
                // we can redirect the mouse capture when:
                // 1. mouse is captured by our drag-and-drop (drag-drop will catch the change automatically)
                // 2. we already have passed once through this procedure
                if (

sourceNode.IsMouseCaptured ||

 (this.MouseHolder != null

 && this.MouseHolder.IsMouseCaptured

)
                    )
                {
                    if (this.MouseHolder == null)
                    {
                        // first time switching between trees
                        sourceNode.Node.NodeLayout.Tree.CurrentDraggingNode = null;
                    }
                    else
                    {
                        this.MouseHolder.CurrentDraggingNode = null;
                    }

                    // keep the latest tree which has mouse captured
                    // this resolves:
                    // 1. the case when we drag from tree1 to tree2, then to tree3 and etc.
                    // 2. we need to release the mouse when drag-drop ends


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

                    this.MouseHolder = destTree;

                    this.MouseHolder.CaptureMouse();

                    this.MouseHolder.CurrentDraggingNode = sourceNode.Node;
                }
                else
                {
                    // we are into the original treee
                    if (this.MouseHolder != null)
                    {
                        this.MouseHolder.CurrentDraggingNode = null;

                        this.MouseHolder.CurrentDraggingNode = null;





                        this.MouseHolder = destTree;

                        this.MouseHolder.CaptureMouse();

                        this.MouseHolder.CurrentDraggingNode = sourceNode.Node;
                    }
                }
            }

            if (targetNode != null && !targetNode.Node.NodeLayout.Tree.AllowDragDropCopy)
            {
                e.OperationType = OperationType.Move;
            }
        }

        void DragSource_DragLeave(object sender, DragDropEventArgs e)
        {
            XamDataTreeNodeControl node = e.DropTarget as XamDataTreeNodeControl;
            if (node != null)
            {
                VisualStateManager.GoToState(node, "NoDrop", false);
            }
            _currentDropDestination = null;
        }

        void DragSource_DragOver(object sender, DragDropMoveEventArgs e)
        {
            XamDataTreeNodeControl destinationNodeControl = e.DropTarget as XamDataTreeNodeControl;
            e.OperationType = OperationType.DropNotAllowed;

            if (destinationNodeControl != null && destinationNodeControl != this)
            {
                destinationNodeControl.IsActiveDropTarget = true;

                Point pt = e.GetPosition(destinationNodeControl);

                double height = destinationNodeControl.ActualHeight;

                // check that we aren't trying to drag a parent onto a child node
                if (EnsureNodeNotBeingAssignedToChild(destinationNodeControl) && EnsureNodeListsCanHandleMoving(destinationNodeControl, e))
                {
                    if (destinationNodeControl.Node.NodeLayout.Tree.AllowDragDropCopy && (Keyboard.Modifiers & ModifierKeys.Control) > 0)
                        e.OperationType = OperationType.Copy;
                    else
                        e.OperationType = OperationType.Move;

                    if (pt.Y <= height * 0.33333)
                    {
                        VisualStateManager.GoToState(destinationNodeControl, "DropBefore", false);
                        _currentDropDestination = TreeDropDestination.DropBefore;
                        destinationNodeControl.Node.NodeLayout.Tree.SetupPossibleExpandingNode(null);
                    }
                    else if (pt.Y >= height * 0.66666)
                    {
                        VisualStateManager.GoToState(destinationNodeControl, "DropAfter", false);
                        _currentDropDestination = TreeDropDestination.DropAfter;
                        destinationNodeControl.Node.NodeLayout.Tree.SetupPossibleExpandingNode(null);
                    }
                    else
                    {
                        VisualStateManager.GoToState(destinationNodeControl, "DropOnto", false);
                        _currentDropDestination = TreeDropDestination.DropOnto;
                        destinationNodeControl.Node.NodeLayout.Tree.SetupPossibleExpandingNode(destinationNodeControl.Node);
                    }
                }
                else
                {
                    VisualStateManager.GoToState(destinationNodeControl, "NoDrop", false);
                    e.OperationType = OperationType.DropNotAllowed;
                    _currentDropDestination = null;
                    destinationNodeControl.Node.NodeLayout.Tree.SetupPossibleExpandingNode(destinationNodeControl.Node);
                }
            }
            else
            {
                if (destinationNodeControl != null)
                    VisualStateManager.GoToState(destinationNodeControl, "NoDrop", false);
                e.OperationType = OperationType.DropNotAllowed;
                _currentDropDestination = null;
            }
        }

        void DragSource_DragStart(object sender, DragDropStartEventArgs e)
        {
            XamDataTreeNodeControl dragNode = XamDataTree.GetNodeFromSource(e.OriginalDragSource as DependencyObject, this.Node.NodeLayout.Tree);

            if (dragNode == null)
            {
                e.Cancel = true;
                return;
            }

            if (this.Node.IsEditing)
            {
                e.Cancel = true;
                return;
            }

            if (!this.Node.IsEnabled)
            {
                e.Cancel = true;
                return;
            }

            e.Data = this.Node;

            VisualStateManager.GoToState(this, "Normal", false);

            e.DragSnapshotElement = base.GetTemplateChild("ContentPresenter") as ContentPresenter;

            this.Node.NodeLayout.Tree.CurrentDraggingNode = this.Node;

            this.Node.NodeLayout.Tree.OnNodeDraggingStart(e);

            // keep the latest tree which has mouse captured
            // this resolves:
            // 1. the case when we drag from tree1 to tree2, then to tree3 and etc.
            // 2. we need to release the mouse when drag-drop ends
            this.MouseHolder = this.Node.NodeLayout.Tree;
            this.MouseHolder.CaptureMouse();
        }

        void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            this.OnEditorLoaded(sender as FrameworkElement);
        }

        void Element_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (!this.Node.NodeLayout.Tree.OnNodeEditingValidationFailed(this.Node, e))
            {
                if (!e.Handled && e.Action == ValidationErrorEventAction.Added)
                    this._bindingError = true;
            }
        }

        #endregion // EventHandlers

        #region IRecyclableElement Members

        bool IRecyclableElement.DelayRecycling
        {
            get
            {
                return this.DelayRecycling;
            }
            set
            {
                this.DelayRecycling = value;
            }
        }

        Panel IRecyclableElement.OwnerPanel
        {
            get
            {
                return this.OwnerPanel;
            }
            set
            {
                this.OwnerPanel = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event raised when a property on this object changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propName"></param>
        protected virtual void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        #region BindingDataInfo Class

        internal class BindingDataInfo
        {
            public FrameworkElement Element
            {
                get;
                set;
            }

            public BindingExpression Expression
            {
                get;
                set;
            }
        }

        #endregion // BindingDataInfo
    }


    internal static class XamDataTreeUtilities
    {
        #region MethodInvoker delegate

        internal delegate void MethodInvoker();

        #endregion //MethodInvoker delegate
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