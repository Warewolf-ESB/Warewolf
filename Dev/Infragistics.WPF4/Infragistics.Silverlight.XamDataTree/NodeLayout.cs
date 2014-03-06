using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A class that how a group of <see cref="XamDataTreeNode"/>s will behave.
    /// </summary>
    public class NodeLayout : NodeLayoutBase
    {
        #region Const

        /// <summary>
        /// The string that will applied to the the key for the <see cref="NodeLayout"/> at the root level.
        /// </summary>
        protected internal const string ROOT_LAYOUT_KEY = "XamDataTree_Root_XamDataTree";

        #endregion // Const

        #region Members

        XamDataTree _tree;
        PropertyInfo _propertyInfo;
        NodeLayoutCollection _nodeLayouts;
        TreeEditingSettingsOverride _editingSettings;
        CheckBoxSettingsOverride _checkboxSettings;        

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeLayout"/> class.
        /// </summary>
        public NodeLayout()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeLayout"/> class.
        /// </summary>
        /// <param propertyName="grid">The <see cref="XamDataTree"/> that owns the <see cref="NodeLayout"/>.</param>
        public NodeLayout(XamDataTree tree)
        {
            this.Tree = tree;
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region Tree

        /// <summary>
        /// Gets the <see cref="XamDataTree"/> that the <see cref="NodeLayout"/> belongs to.
        /// </summary>
        public XamDataTree Tree
        {
            get
            {
                return this._tree;
            }
            internal set
            {
                if (this._tree != null)
                {
                    if (this._tree == value)
                        return;
                    else
                        this._tree.PropertyChanged -= Tree_PropertyChanged;
                }

                this._tree = value;
                if (this._tree != null)
                {
                    this._tree.PropertyChanged += Tree_PropertyChanged;

                    foreach (NodeLayout layout in this.NodeLayouts)
                        layout.Tree = value;
                }
            }
        }

        #endregion // Tree

        #region TargetTypeName

        /// <summary>
        /// Identifies the <see cref="TargetTypeName"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty TargetTypeNameProperty = DependencyProperty.Register("TargetTypeName", typeof(string), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(TargetTypeNameChanged)));

        /// <summary>
        /// Get/Sets the System.Type.Name or <see cref="Type.FullName"/> that this <see cref="NodeLayout"/> object should represent.
        /// </summary>
        /// <remarks>
        /// The property is only used if the <see cref="NodeLayout"/> is defined in the <see cref="XamDataTree.GlobalNodeLayouts"/>
        /// </remarks>
        public string TargetTypeName
        {
            get { return (string)this.GetValue(TargetTypeNameProperty); }
            set { this.SetValue(TargetTypeNameProperty, value); }
        }

        private static void TargetTypeNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // TargetTypeName

        #region Visibility

        /// <summary>
        /// Identifies the <see cref="Visibility"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(VisibilityChanged)));

        /// <summary>
        /// Gets/Sets the Visibility of the <see cref="NodeLayout"/>
        /// </summary>
        public Visibility Visibility
        {
            get { return (Visibility)this.GetValue(VisibilityProperty); }
            set { this.SetValue(VisibilityProperty, value); }
        }

        private static void VisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout col = (NodeLayout)obj;
            col.OnVisibilityChanged();
        }

        /// <summary>
        /// Raised when the Visiblity of a <see cref="NodeLayout"/> has changed.
        /// </summary>
        protected virtual void OnVisibilityChanged()
        {
            this.OnPropertyChanged("Visibility");
        }

        #endregion // Visibility

        #region DisplayMemberPath

        /// <summary>
        /// Identifies the <see cref="DisplayMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(NodeLayout), new PropertyMetadata("", new PropertyChangedCallback(DisplayMemberPathChanged)));

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
            NodeLayout layout = (NodeLayout)obj;
            if (layout.Tree != null)
                layout.Tree.InvalidateScrollPanel(true, false, true);
        }

        #endregion // DisplayMemberPath

        #region DisplayMemberPathResolved

        /// <summary>
        /// Resolves the <see cref="DisplayMemberPath"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public string DisplayMemberPathResolved
        {
            get
            {
                string retVal = "";
                if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                    retVal = this.DisplayMemberPath;
                else if (this.Tree != null)
                    retVal = this.Tree.DisplayMemberPath;
                return retVal;
            }
        }

        #endregion // DisplayMemberPathResolved

        #region CheckBoxMemberPath

        /// <summary>
        /// Identifies the <see cref="CheckBoxMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxMemberPathProperty = DependencyProperty.Register("CheckBoxMemberPath", typeof(string), typeof(NodeLayout), new PropertyMetadata("", new PropertyChangedCallback(CheckBoxMemberPathChanged)));

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
            NodeLayout ctrl = (NodeLayout)obj;
            ctrl.OnPropertyChanged("CheckBoxMemberPath");
            ctrl.OnPropertyChanged("CheckBoxMemberPathResolved");

            if (ctrl.Tree != null)
                ctrl.Tree.InvalidateScrollPanel(true, false, true);
        }

        #endregion // CheckBoxMemberPath

        #region CheckBoxMemberPathResolved

        /// <summary>
        /// Resolves the <see cref="CheckBoxMemberPath"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public string CheckBoxMemberPathResolved
        {
            get
            {
                string retVal = "";
                if (!string.IsNullOrEmpty(this.CheckBoxMemberPath))
                    retVal = this.CheckBoxMemberPath;
                else if (this.Tree != null)
                    retVal = this.Tree.CheckBoxMemberPath;
                return retVal;
            }
        }

        #endregion // CheckBoxMemberPathResolved

		// MD 8/11/11 - XamFormulaEditor
		#region IsEnabledMemberPath

		/// <summary>
		/// Identifies the <see cref="IsEnabledMemberPath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsEnabledMemberPathProperty = DependencyPropertyUtilities.Register("IsEnabledMemberPath",
			typeof(string), typeof(NodeLayout),
			DependencyPropertyUtilities.CreateMetadata("", new PropertyChangedCallback(OnIsEnabledMemberPathChanged))
			);

		private static void OnIsEnabledMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			NodeLayout ctrl = (NodeLayout)d;
			ctrl.OnPropertyChanged("IsEnabledMemberPath");
			ctrl.OnPropertyChanged("IsEnabledMemberPathResolved");

			if (ctrl.Tree != null)
				ctrl.Tree.InvalidateScrollPanel(true, false, true);
		}

		/// <summary>
		/// Gets / sets the path to the property on the <see cref="XamDataTreeNode.Data"/> object to populate the <see cref="XamDataTreeNode.IsEnabled"/> of the <see cref="XamDataTreeNode"/>.
		/// </summary>
		/// <seealso cref="IsEnabledMemberPathProperty"/>
		public string IsEnabledMemberPath
		{
			get { return (string)this.GetValue(NodeLayout.IsEnabledMemberPathProperty); }
			set { this.SetValue(NodeLayout.IsEnabledMemberPathProperty, value); }
		}

		#endregion //IsEnabledMemberPath

		// MD 8/11/11 - XamFormulaEditor
		#region IsEnabledMemberPathResolved

		/// <summary>
		/// Resolves the <see cref="IsEnabledMemberPath"/> property for a particular <see cref="NodeLayout"/>.
		/// </summary>
		public string IsEnabledMemberPathResolved
		{
			get
			{
				if (!string.IsNullOrEmpty(this.IsEnabledMemberPath))
					return this.IsEnabledMemberPath;
				else if (this.Tree != null)
					return this.Tree.IsEnabledMemberPath;
				else
					return "";
			}
		}

		#endregion // IsEnabledMemberPathResolved

		#region IsExpandedMemberPath

		/// <summary>
        /// Identifies the <see cref="IsExpandedMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsExpandedMemberPathProperty = DependencyProperty.Register("IsExpandedMemberPath", typeof(string), typeof(NodeLayout), new PropertyMetadata("", new PropertyChangedCallback(IsExpandedMemberPathChanged)));

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
            NodeLayout ctrl = (NodeLayout)obj;
            ctrl.OnPropertyChanged("IsExpandedMemberPath");
            ctrl.OnPropertyChanged("IsExpandedMemberPathResolved");

            if (ctrl.Tree != null)
                ctrl.Tree.InvalidateScrollPanel(true, false, true);
        }

        #endregion // IsExpandedMemberPath

        #region IsExpandedMemberPathResolved

        /// <summary>
        /// Resolves the <see cref="IsExpandedMemberPath"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public string IsExpandedMemberPathResolved
        {
            get
            {
                if (!string.IsNullOrEmpty(this.IsExpandedMemberPath))
                    return this.IsExpandedMemberPath;
                else if (this.Tree != null)
                    return this.Tree.IsExpandedMemberPath;
                else
                    return "";
            }
        }

        #endregion // IsExpandedMemberPathResolved

        #region NodeLayouts

        /// <summary>
        /// Gets a Collection of <see cref="NodeLayout"/> objects that will be used only for the root level of the <see cref="XamDataTree"/>
        /// </summary>
        public NodeLayoutCollection NodeLayouts
        {
            get
            {
                if (this._nodeLayouts == null)
                {
                    this._nodeLayouts = new NodeLayoutCollection();
                    this._nodeLayouts.CollectionChanged += NodeLayouts_CollectionChanged;
                }
                return this._nodeLayouts;
            }
        }

        #endregion // NodeLayouts

        #region Indentation

        /// <summary>
        /// Identifies the <see cref="Indentation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation", typeof(double), typeof(NodeLayout), new PropertyMetadata(double.NaN, new PropertyChangedCallback(IndentationChanged)));

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
            NodeLayout layout = (NodeLayout)obj;
            layout.OnPropertyChanged("Indentation");
        }

        #endregion // Indentation

        #region IndentationResolved

        /// <summary>
        /// Resolves the <see cref="Indentation"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public double IndentationResolved
        {
            get
            {
                if (double.IsNaN(this.Indentation))
                {
                    if (this.Tree != null)
                        return this.Tree.Indentation;
                    else
                        return (double)XamDataTree.IndentationProperty.GetMetadata(typeof(XamDataTree)).DefaultValue;
                }
                else
                {
                    return this.Indentation;
                }
            }
        }

        #endregion // IndentationResolved

        #region ItemTemplate

        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(ItemTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> that will be used to create the VisualTree for every <see cref="XamDataTreeNode"/> for this particular <see cref="NodeLayout"/> in the <see cref="XamDataTree"/>
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }

        private static void ItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout layout = (NodeLayout)obj;
            layout.OnPropertyChanged("ItemTemplate");
        }

        #endregion // ItemTemplate

        #region ItemTemplateResolved

        /// <summary>
        /// Gets the actual <see cref="DataTemplate"/> that will be used to create the VisualTree for every <see cref="XamDataTreeNode"/> for this particular <see cref="NodeLayout"/>.
        /// </summary>
        public DataTemplate ItemTemplateResolved
        {
            get
            {
                if (this.ItemTemplate == null && this.Tree != null)
                    return this.Tree.ItemTemplate;
                return this.ItemTemplate;
            }
        }

        #endregion // ItemTemplateResolved

        #region HeaderText

        /// <summary>
        /// Identifies the <see cref="HeaderText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(HeaderTextChanged)));

        /// <summary>
        /// Gets / sets the text that will appear for <see cref="XamDataTreeNode"/> objects acting as header nodes.
        /// </summary>
        public string HeaderText
        {
            get { return (string)this.GetValue(HeaderTextProperty); }
            set { this.SetValue(HeaderTextProperty, value); }
        }

        private static void HeaderTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout layout = (NodeLayout)obj;
            layout.OnPropertyChanged("HeaderText");
        }

        #endregion // HeaderText

        #region HeaderTemplate

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(HeaderTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> which will appear on header nodes.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)this.GetValue(HeaderTemplateProperty); }
            set { this.SetValue(HeaderTemplateProperty, value); }
        }

        private static void HeaderTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout layout = (NodeLayout)obj;
            layout.OnPropertyChanged("HeaderTemplate");            
        }

        #endregion // HeaderTemplate

        #region HeaderContentResolved

        /// <summary>
        /// Resolves the <see cref="HeaderTemplate"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public object HeaderContentResolved
        {
            get
            {
                if (this.HeaderTemplate == null)
                {
                    if (string.IsNullOrEmpty(this.HeaderText))
                    {
                        return this.Key;
                    }

                    return this.HeaderText;
                }
                return this.HeaderTemplate;
            }
        }

        #endregion // HeaderContentResolved

        #region EditorTemplate

        /// <summary>
        /// Identifies the <see cref="EditorTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EditorTemplateProperty = DependencyProperty.Register("EditorTemplate", typeof(DataTemplate), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(EditorTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> that will be displayed when the <see cref="XamDataTreeNode"/> object goes into edit mode.
        /// </summary>
        public DataTemplate EditorTemplate
        {
            get { return (DataTemplate)this.GetValue(EditorTemplateProperty); }
            set { this.SetValue(EditorTemplateProperty, value); }
        }

        private static void EditorTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout layout = (NodeLayout)obj;
            layout.OnPropertyChanged("EditorTemplate");
        }

        #endregion // EditorTemplate

        #region EditingSettings

        /// <summary>
        /// Gets a reference to the <see cref="TreeEditingSettingsOverride"/> object that controls all the properties for editing on this <see cref="NodeLayout"/>.
        /// </summary>
        public TreeEditingSettingsOverride EditingSettings
        {
            get
            {
                if (this._editingSettings == null)
                {
                    this._editingSettings = new TreeEditingSettingsOverride();
                    this._editingSettings.PropertyChanged += new PropertyChangedEventHandler(EditingSettings_PropertyChanged);
                }

                this._editingSettings.NodeLayout = this;

                return this._editingSettings;
            }
            set
            {
                if (this._editingSettings != value)
                {
                    if (this._editingSettings != null)
                        this._editingSettings.PropertyChanged -= new PropertyChangedEventHandler(EditingSettings_PropertyChanged);

                    this._editingSettings = value;

                    if (this._editingSettings != null)
                        this._editingSettings.PropertyChanged += new PropertyChangedEventHandler(EditingSettings_PropertyChanged);
                }
            }
        }

        #region EditingSettings_PropertyChanged
        private void EditingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
        #endregion //EditingSettings_PropertyChanged

        #endregion // EditingSettings

        #region CheckBoxSettings

        /// <summary>
        /// Gets a reference to the <see cref="CheckBoxSettingsOverride"/> object that controls all the properties for <see cref="System.Windows.Controls.CheckBox"/>es on this <see cref="NodeLayout"/>.
        /// </summary>
        public CheckBoxSettingsOverride CheckBoxSettings
        {
            get
            {
                if (this._checkboxSettings == null)
                {
                    this._checkboxSettings = new CheckBoxSettingsOverride();
                    this._checkboxSettings.PropertyChanged += new PropertyChangedEventHandler(CheckboxSettings_PropertyChanged);
                }
                this._checkboxSettings.NodeLayout = this;
                return this._checkboxSettings;
            }

            set
            {
                if (this._checkboxSettings != value)
                {
                    if (this._checkboxSettings != null)
                        this._checkboxSettings.PropertyChanged -= new PropertyChangedEventHandler(CheckboxSettings_PropertyChanged);

                    this._checkboxSettings = value;

                    if (this._checkboxSettings != null)
                        this._checkboxSettings.PropertyChanged += new PropertyChangedEventHandler(CheckboxSettings_PropertyChanged);
                }
            }
        }

        void CheckboxSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        #endregion // CheckBoxSettings

        #region IsDraggable

        /// <summary>
        /// Identifies the <see cref="IsDraggable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.Register("IsDraggable", typeof(bool?), typeof(NodeLayout), new PropertyMetadata(null, new PropertyChangedCallback(IsDraggableChanged)));

        /// <summary>
        /// Gets / sets whether or not <see cref="XamDataTreeNode"/> objects associated with the <see cref="NodeLayout"/> are draggable.
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsDraggable
        {
            get { return (bool?)this.GetValue(IsDraggableProperty); }
            set { this.SetValue(IsDraggableProperty, value); }
        }

        private static void IsDraggableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout ctrl = (NodeLayout)obj;
            ctrl.OnPropertyChanged("IsDraggable");
        }

        #endregion // IsDraggable

        #region IsDraggableResolved

        /// <summary>
        /// Resolves the <see cref="IsDraggable"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public bool IsDraggableResolved
        {
            get
            {
                bool retValue = false;
                if (this.IsDraggable != null)
                    retValue = (bool)this.IsDraggable;
                else if (this.Tree != null)
                    retValue = this.Tree.IsDraggable;
                return retValue;
            }
        }

        #endregion // IsDraggableResolved

        #region IsDropTarget

        /// <summary>
        /// Identifies the <see cref="IsDropTarget"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.Register("IsDropTarget", typeof(bool?), typeof(NodeLayout), new PropertyMetadata(null, new PropertyChangedCallback(IsDropTargetChanged)));

        /// <summary>
        /// Gets / sets whether or not <see cref="XamDataTreeNode"/> objects can be dropped on this <see cref="NodeLayout"/>.
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsDropTarget
        {
            get { return (bool?)this.GetValue(IsDropTargetProperty); }
            set { this.SetValue(IsDropTargetProperty, value); }
        }

        private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout ctrl = (NodeLayout)obj;
            ctrl.OnPropertyChanged("IsDropTarget");
        }

        #endregion // IsDropTarget

        #region IsDropTargetResolved
        /// <summary>
        /// Resolves the <see cref="IsDropTarget"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public bool IsDropTargetResolved
        {
            get
            {
                bool retValue = false;
                if (this.IsDropTarget != null)
                    retValue = (bool)this.IsDropTarget;
                else if (this.Tree != null)
                    retValue = this.Tree.IsDropTarget;
                return retValue;
            }
        }
        #endregion // IsDropTargetResolved

        #region NodeStyleResolved

        /// <summary>
        /// Resolves the <see cref="NodeStyle"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public Style NodeStyleResolved
        {
            get
            {
                if (this.NodeStyle == null && this.Tree != null)
                {
                    return this.Tree.NodeStyle;
                }

                return this.NodeStyle;
            }
        }

        #endregion // NodeStyleResolved

        #region NodeStyle

        /// <summary>
        /// Identifies the <see cref="NodeStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NodeStyleProperty = DependencyProperty.Register("NodeStyle", typeof(Style), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(NodeStyleChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Style"/> which will be assigned to <see cref="XamDataTreeNode"/> objects associcated with this <see cref="NodeLayout"/>.
        /// </summary>
        public Style NodeStyle
        {
            get { return (Style)this.GetValue(NodeStyleProperty); }
            set { this.SetValue(NodeStyleProperty, value); }
        }

        private static void NodeStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout layout = (NodeLayout)obj;
            layout.OnPropertyChanged("NodeStyle");            
            if (layout.Tree != null)
                layout.Tree.InvalidateScrollPanel(false);
        }

        #endregion // NodeStyle

        #region ExpandedIconTemplate

        /// <summary>
        /// Identifies the <see cref="ExpandedIconTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ExpandedIconTemplateProperty = DependencyProperty.Register("ExpandedIconTemplate", typeof(DataTemplate), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(ExpandedIconTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> that will be displayed on nodes that are currently expanded.
        /// </summary>
        public DataTemplate ExpandedIconTemplate
        {
            get { return (DataTemplate)this.GetValue(ExpandedIconTemplateProperty); }
            set { this.SetValue(ExpandedIconTemplateProperty, value); }
        }

        private static void ExpandedIconTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout ctrl = (NodeLayout)obj;
            ctrl.OnPropertyChanged("ExpandedIconTemplate");
        }

        #endregion // ExpandedIconTemplate

        #region ExpandedIconTemplateResolved

        /// <summary>
        /// Get the <see cref="DataTemplate"/> which will be used for the <see cref="XamDataTreeNode"/>'s on this <see cref="NodeLayout"/> when they have children and they are expanded.
        /// </summary>
        public DataTemplate ExpandedIconTemplateResolved
        {
            get
            {
                DataTemplate retVal = this.ExpandedIconTemplate;
                if (retVal == null && this.Tree != null)
                    retVal = this.Tree.ExpandedIconTemplate;
                return retVal;
            }
        }

        #endregion // ExpandedIconTemplateResolved

        #region CollapsedIconTemplate

        /// <summary>
        /// Identifies the <see cref="CollapsedIconTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CollapsedIconTemplateProperty = DependencyProperty.Register("CollapsedIconTemplate", typeof(DataTemplate), typeof(NodeLayout), new PropertyMetadata(new PropertyChangedCallback(CollapsedIconTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> that will be displayed on nodes that are currently collapsed on the .
        /// </summary>
        public DataTemplate CollapsedIconTemplate
        {
            get { return (DataTemplate)this.GetValue(CollapsedIconTemplateProperty); }
            set { this.SetValue(CollapsedIconTemplateProperty, value); }
        }

        private static void CollapsedIconTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLayout ctrl = (NodeLayout)obj;
            ctrl.OnPropertyChanged("CollapsedIconTemplate");
        }

        #endregion // CollapsedIconTemplate

        #region CollapsedIconTemplateResolved

        /// <summary>
        /// Get the <see cref="DataTemplate"/> which will be used for the <see cref="XamDataTreeNode"/>'s on this <see cref="NodeLayout"/> when they have children and they are collapsed.
        /// </summary>
        public DataTemplate CollapsedIconTemplateResolved
        {
            get
            {
                DataTemplate retVal = this.CollapsedIconTemplate;
                if (retVal == null && this.Tree != null)
                    retVal = this.Tree.CollapsedIconTemplate;
                return retVal;
            }
        }

        #endregion // CollapsedIconTemplateResolved

        #endregion // Public

        #region Internal

        internal bool IsDefinedGlobally
        {
            get;
            set;
        }

        internal bool IsInitialized
        {
            get;
            set;
        }

        internal IEnumerable<DataField> DataFields
        {
            get;
            set;
        }

        #endregion // Internal

        #endregion // Tree

        #region Methods

        #region Protected

        /// <summary>
        /// Method called when the <see cref="NodeLayoutBase.Key"/> property is altered.
        /// </summary>
        protected override void OnKeyChanged()
        {
            base.OnKeyChanged();

            if (this.Tree != null && this.Tree.GlobalNodeLayouts.Contains(this))
                this.IsDefinedGlobally = true;
        }

        #endregion // Protected

        #region Internal

        internal PropertyInfo ResolvePropertyInfo(object data)
        {
            if (this._propertyInfo != null && data != null && this._propertyInfo.DeclaringType != data.GetType())
            {
                this._propertyInfo = null;
            }

            if (this._propertyInfo == null && data != null && !string.IsNullOrEmpty(this.Key))
            {
                if (this.IsDefinedGlobally && !string.IsNullOrEmpty(this.TargetTypeName))
                {
                    Type type = data.GetType();
                    PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    
                    foreach (PropertyInfo prop in props)
                    {


                        



                        if (type == typeof(System.Data.DataRowView) && prop.Name =="DataView")
                        {
                            continue;
                        }


                        object obj = null;
                        try
                        {
                            obj = prop.GetValue(data, null);
                        }
                        catch (TargetParameterCountException)
                        {
                            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                        }
                        if (obj != null)
                        {
                            IEnumerable list = obj as IEnumerable;
                            if (list != null)
                            {
                                Type t = DataManagerBase.ResolveItemType(list);
                                if (t != null)
                                {
                                    if (this == this.Tree.GlobalNodeLayouts.FromType(t))
                                    {
                                        this._propertyInfo = prop;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    this._propertyInfo = data.GetType().GetProperty(this.Key, BindingFlags.Public);
            }

            return this._propertyInfo;
        }

        #endregion // Internal

        #endregion // Methods

        #region Events

        internal event EventHandler<NodeLayoutEventArgs> ChildNodeLayoutRemoved;

        internal void OnChildNodeLayoutRemoved()
        {
            if (this.ChildNodeLayoutRemoved != null)
            {
                this.ChildNodeLayoutRemoved(this, new NodeLayoutEventArgs() { NodeLayout = this });
            }
        }

        internal event EventHandler<NodeLayoutEventArgs> ChildNodeLayoutAdded;

        internal void OnChildNodeLayoutAdded()
        {
            if (this.ChildNodeLayoutAdded != null)
            {
                this.ChildNodeLayoutAdded(this, new NodeLayoutEventArgs() { NodeLayout = this });
            }
        }

        internal event EventHandler<NodeLayoutEventArgs> ChildNodeLayoutVisibilityChanged;

        internal void OnChildNodeLayoutVisibilityChanged()
        {
            if (this.ChildNodeLayoutVisibilityChanged != null)
            {
                this.ChildNodeLayoutVisibilityChanged(this, new NodeLayoutEventArgs() { NodeLayout = this });
            }
        }

        internal event EventHandler<EventArgs> NodeLayoutDisposed;

        internal void OnNodeLayoutDisposed()
        {
            if (this.NodeLayoutDisposed != null)
                this.NodeLayoutDisposed(this, EventArgs.Empty);
        }

        #endregion // Events

        #region EventHandlers

        #region Tree_PropertyChanged

        void Tree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
            this.Tree.InvalidateScrollPanel(true);

            this.CheckBoxSettings.InvalidateResolvedProperties();

            switch (e.PropertyName)
            {
                case ("CollapsedIconTemplate"):
                    {
                        this.OnPropertyChanged(e.PropertyName + "Resolved");

                        break;
                    }
                case ("ExpandedIconTemplate"):
                    {
                        this.OnPropertyChanged(e.PropertyName + "Resolved");
                        break;
                    }
            }
        }

        #endregion // Tree_PropertyChanged

        #region NodeLayouts_CollectionChanged

        void NodeLayouts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (NodeLayout c in e.NewItems)
                {
                    c.Tree = this.Tree;
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

        #endregion // EventHandlers
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