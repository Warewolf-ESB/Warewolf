using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// A control that allows end users to build complex filters over a single column.
    /// </summary>
    [TemplatePart(Name = "FilterGrid", Type = typeof(XamGrid))]
    public class CompoundFilterDialogControl : Control, ICommandTarget, INotifyPropertyChanged
    {
        #region Members
        bool _columnChooserWasOpen = false;
        Popup _dialogPopup;
        CellBase _cell;
        bool _isOpen;
        UIElement _rootVis;
        List<FilterOperand> _defaultFilterOperand;
        XamGrid _filterGrid;
        ObservableCollection<XamGridConditionInfo> _infoObjects = new ObservableCollection<XamGridConditionInfo>();
        XamGridConditionInfoGroup _masterParentGroup = new XamGridConditionInfoGroup(null);
        bool _needSetupGrid = true;
        int _groupColumnKeyCounter = 0;
        ProxyColumn _proxyColumn;
        string _filterDescription = string.Empty;
        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="CompoundFilterDialogControl"/> class.
        /// </summary>
        static CompoundFilterDialogControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CompoundFilterDialogControl), new FrameworkPropertyMetadata(typeof(CompoundFilterDialogControl)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundFilterDialogControl"/> class.
        /// </summary>
        public CompoundFilterDialogControl()
        {



            this._dialogPopup = new Popup();


            this._dialogPopup.Placement = PlacementMode.Relative;
            this._dialogPopup.AllowsTransparency = true;
            this._dialogPopup.StaysOpen = false;
            this._dialogPopup.Closed += new EventHandler(DialogPopup_Closed);
            this._dialogPopup.Opened += new EventHandler(DialogPopup_Opened);


            this._dialogPopup.SizeChanged += new SizeChangedEventHandler(DialogPopup_SizeChanged);

            this.CancelButtonText = SRGrid.GetString("CustomFilterDialog_CancelButtonText");
            this.OKButtonText = SRGrid.GetString("CustomFilterDialog_OKButtonText");
            this.SelectedGroupText = SRGrid.GetString("CompoundFilterDialog_SelectedGroupText");
            this.AndButtonText = SRGrid.GetString("CompoundFilterDialog_AndButtonText");
            this.OrButtonText = SRGrid.GetString("CompoundFilterDialog_OrButtonText");
            this.AddConditionButtonText = SRGrid.GetString("CompoundFilterDialog_AddConditionButtonText");
            this.RemoveConditionButtonText = SRGrid.GetString("CompoundFilterDialog_RemoveConditionButtonText");
            this.UngroupButtonText = SRGrid.GetString("CompoundFilterDialog_UngroupButtonText");
            this.ToggleButtonText = SRGrid.GetString("CompoundFilterDialog_ToggleButtonText");

            this.AndColorLabelText = SRGrid.GetString("CompoundFilterDialog_AndColorLabelText");
            this.OrColorLabelText = SRGrid.GetString("CompoundFilterDialog_OrColorLabelText");

            this.IsTabStop = false;

            this.Unloaded += new RoutedEventHandler(CompoundFilterDialogControl_Unloaded);
        }        

        #endregion // Constructor

        #region Properties

        #region InfoObjects
        /// <summary>
        /// A list of objects that describes how the filter should be built.
        /// </summary>
        internal ObservableCollection<XamGridConditionInfo> InfoObjects
        {
            get
            {
                return this._infoObjects;
            }
        }
        #endregion // InfoObjects

        #region FilterGrid
        /// <summary>
        /// The <see cref="XamGrid"/> that will display the <see cref="CompoundFilterDialogControl.InfoObjects"/> list.
        /// </summary>
        internal XamGrid FilterGrid
        {
            get
            {
                return this._filterGrid;
            }
        }
        #endregion // FilterGrid

        #region MasterParentGroup
        /// <summary>
        /// The <see cref="XamGridConditionInfoGroup"/> 
        /// </summary>
        internal XamGridConditionInfoGroup MasterParentGroup { get { return this._masterParentGroup; } }
        #endregion // MasterParentGroup

        #region Cell

        /// <summary>
        /// Gets / sets the <see cref="CellBase"/> object which hosts the <see cref="CompoundFilterDialogControl"/>.
        /// </summary>
        public CellBase Cell
        {
            get
            {
                return this._cell;
            }
            set
            {
                if (this._cell != value)
                {
                    this._cell = value;
                    this._defaultFilterOperand = null;
                    if (this._proxyColumn != null)
                        this._proxyColumn.ProxiedSource = value;
                }

                Style s = null;

                if (this._cell != null)
                {
                    s = this.Cell.Column.ColumnLayout.FilteringSettings.CustomFilterDialogStyleResolved;
                }

                ColumnContentProviderBase.SetControlStyle(this, s);
            }
        }

        #endregion // Cell

        #region CancelButtonText

        /// <summary>
        /// Identifies the <see cref="CancelButtonText"/> dependency property. 
        /// </summary>                
        public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register("CancelButtonText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(CancelButtonTextChanged)));


        /// <summary>
        /// Gets / sets the text for the cancel buttom.
        /// </summary>
        public string CancelButtonText
        {
            get { return (string)this.GetValue(CancelButtonTextProperty); }
            set { this.SetValue(CancelButtonTextProperty, value); }
        }

        private static void CancelButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl fsc = (CompoundFilterDialogControl)obj;
            fsc.OnPropertyChanged("CancelButtonText");
        }

        #endregion // CancelButtonText

        #region OKButtonText

        /// <summary>
        /// Identifies the <see cref="OKButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty OKButtonTextProperty = DependencyProperty.Register("OKButtonText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(OKButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the OK Button.
        /// </summary>
        public string OKButtonText
        {
            get { return (string)this.GetValue(OKButtonTextProperty); }
            set { this.SetValue(OKButtonTextProperty, value); }
        }

        private static void OKButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl fsc = (CompoundFilterDialogControl)obj;
            fsc.OnPropertyChanged("OKButtonText");
        }

        #endregion // OKButtonText

        #region SelectedGroupText

        /// <summary>
        /// Identifies the <see cref="SelectedGroupText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectedGroupTextProperty = DependencyProperty.Register("SelectedGroupText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(SelectedGroupTextChanged)));

        /// <summary>
        /// Gets / sets the text for the "Selected Group" string.
        /// </summary>
        public string SelectedGroupText
        {
            get { return (string)this.GetValue(SelectedGroupTextProperty); }
            set { this.SetValue(SelectedGroupTextProperty, value); }
        }

        private static void SelectedGroupTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl ctrl = (CompoundFilterDialogControl)obj;
            ctrl.OnPropertyChanged("SelectedGroupText");
        }

        #endregion // SelectedGroupText

        #region AndButtonText

        /// <summary>
        /// Identifies the <see cref="AndButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AndButtonTextProperty = DependencyProperty.Register("AndButtonText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(AndButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the "And" string.
        /// </summary>
        public string AndButtonText
        {
            get { return (string)this.GetValue(AndButtonTextProperty); }
            set { this.SetValue(AndButtonTextProperty, value); }
        }

        private static void AndButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl ctrl = (CompoundFilterDialogControl)obj;
            ctrl.OnPropertyChanged("AndButtonText");
        }

        #endregion // AndButtonText

        #region OrButtonText

        /// <summary>
        /// Identifies the <see cref="OrButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty OrButtonTextProperty = DependencyProperty.Register("OrButtonText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(OrButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the "Or" string.
        /// </summary>
        public string OrButtonText
        {
            get { return (string)this.GetValue(OrButtonTextProperty); }
            set { this.SetValue(OrButtonTextProperty, value); }
        }

        private static void OrButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl ctrl = (CompoundFilterDialogControl)obj;
            ctrl.OnPropertyChanged("OrButtonText");
        }

        #endregion // OrButtonText

        #region AddConditionButtonText

        /// <summary>
        /// Identifies the <see cref="AddConditionButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AddConditionButtonTextProperty = DependencyProperty.Register("AddConditionButtonText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(AddConditionButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the OK Button.
        /// </summary>
        public string AddConditionButtonText
        {
            get { return (string)this.GetValue(AddConditionButtonTextProperty); }
            set { this.SetValue(AddConditionButtonTextProperty, value); }
        }

        private static void AddConditionButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl fsc = (CompoundFilterDialogControl)obj;
            fsc.OnPropertyChanged("AddConditionButtonText");
        }

        #endregion // AddConditionButtonText

        #region RemoveConditionButtonText

        /// <summary>
        /// Identifies the <see cref="RemoveConditionButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty RemoveConditionButtonTextProperty = DependencyProperty.Register("RemoveConditionButtonText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(RemoveConditionButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the OK Button.
        /// </summary>
        public string RemoveConditionButtonText
        {
            get { return (string)this.GetValue(RemoveConditionButtonTextProperty); }
            set { this.SetValue(RemoveConditionButtonTextProperty, value); }
        }

        private static void RemoveConditionButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl fsc = (CompoundFilterDialogControl)obj;
            fsc.OnPropertyChanged("RemoveConditionButtonText");
        }

        #endregion // RemoveConditionButtonText

        #region ToggleButtonText

        /// <summary>
        /// Identifies the <see cref="ToggleButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ToggleButtonTextProperty = DependencyProperty.Register("ToggleButtonText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(ToggleButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the "Toggle" string.
        /// </summary>
        public string ToggleButtonText
        {
            get { return (string)this.GetValue(ToggleButtonTextProperty); }
            set { this.SetValue(ToggleButtonTextProperty, value); }
        }

        private static void ToggleButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl ctrl = (CompoundFilterDialogControl)obj;
            ctrl.OnPropertyChanged("ToggleButtonText");
        }

        #endregion // ToggleButtonText

        #region UngroupButtonText

        /// <summary>
        /// Identifies the <see cref="UngroupButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty UngroupButtonTextProperty = DependencyProperty.Register("UngroupButtonText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(UngroupButtonTextChanged)));

        /// <summary>
        /// Gets / sets the text for the "Ungroup" string.
        /// </summary>
        public string UngroupButtonText
        {
            get { return (string)this.GetValue(UngroupButtonTextProperty); }
            set { this.SetValue(UngroupButtonTextProperty, value); }
        }

        private static void UngroupButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl ctrl = (CompoundFilterDialogControl)obj;
            ctrl.OnPropertyChanged("UngroupButtonText");
        }

        #endregion // UngroupButtonText

        #region AndColorLabelText

        /// <summary>
        /// Identifies the <see cref="AndColorLabelText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AndColorLabelTextProperty = DependencyProperty.Register("AndColorLabelText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(AndColorLabelTextChanged)));

        /// <summary>
        /// Gets / sets the text for the "And" string.
        /// </summary>
        public string AndColorLabelText
        {
            get { return (string)this.GetValue(AndColorLabelTextProperty); }
            set { this.SetValue(AndColorLabelTextProperty, value); }
        }

        private static void AndColorLabelTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl ctrl = (CompoundFilterDialogControl)obj;
            ctrl.OnPropertyChanged("AndColorLabelText");
        }

        #endregion // AndColorLabelText

        #region OrColorLabelText

        /// <summary>
        /// Identifies the <see cref="OrColorLabelText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty OrColorLabelTextProperty = DependencyProperty.Register("OrColorLabelText", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(OrColorLabelTextChanged)));

        /// <summary>
        /// Gets / sets the text for the "Or" string.
        /// </summary>
        public string OrColorLabelText
        {
            get { return (string)this.GetValue(OrColorLabelTextProperty); }
            set { this.SetValue(OrColorLabelTextProperty, value); }
        }

        private static void OrColorLabelTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl ctrl = (CompoundFilterDialogControl)obj;
            ctrl.OnPropertyChanged("OrColorLabelText");
        }

        #endregion // OrColorLabelText

        #region FilterDescription

        /// <summary>
        /// Identifies the <see cref="FilterDescription"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterDescriptionProperty = DependencyProperty.Register("FilterDescription", typeof(string), typeof(CompoundFilterDialogControl), new PropertyMetadata(new PropertyChangedCallback(FilterDescriptionChanged)));

        public string FilterDescription
        {
            get { return (string)this.GetValue(FilterDescriptionProperty); }
            set { this.SetValue(FilterDescriptionProperty, value); }
        }

        private static void FilterDescriptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CompoundFilterDialogControl ctrl = (CompoundFilterDialogControl)obj;
            ctrl.OnPropertyChanged("FilterDescription");
        }

        #endregion // FilterDescription 

        #endregion // Properties

        #region Event Handlers

        #region Grid_SizeChanged

        void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Hide();
        }

        #endregion // Grid_SizeChanged

        #region DialogPopup_SizeChanged

        void DialogPopup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AlignPopup();
        }

        #endregion // DialogPopup_SizeChanged

        #region CompoundFilterDialogControl_SizeChanged

        void CompoundFilterDialogControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AlignPopup();
        }

        #endregion // CompoundFilterDialogControl_SizeChanged

        #region MouseLeftButtonDown
        void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

        }
        #endregion // MouseLeftButtonDown

        #region KeyDown

        /// <summary>
        /// Called before the <see cref="UIElement.KeyDown"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event.</param>
        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Escape)
                {
                    this.Hide();
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.Enter)
                {
                    this.AcceptChanges();
                    this.Hide();
                    e.Handled = true;
                    return;
                }
            }
        }
        #endregion // KeyDown

        #region CompoundFilterDialogControl_Unloaded
        void CompoundFilterDialogControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.FilterGrid != null)
            {
                this._ignoreCellExitingEditMode = true;
                this.FilterGrid.ItemsSource = null;
                this._ignoreCellExitingEditMode = false;
            }
        }
        #endregion // CompoundFilterDialogControl_Unloaded

        #endregion // EventHandlers

        #region Methods

        #region Private

        private void AlignPopup()
        {
            if (this._dialogPopup.IsOpen)
            {
                XamGrid grid = this.Cell.Row.ColumnLayout.Grid;

                double halfTheGridWidth = grid.ActualWidth / 2;
                double halfTheGridHeight = grid.ActualHeight / 2;

                double halfThePopupWidth = this._dialogPopup.Child.DesiredSize.Width / 2;
                double halfThePopupHeight = this._dialogPopup.Child.DesiredSize.Height / 2;

                double horizontalOffset = halfTheGridWidth - halfThePopupWidth;
                this._dialogPopup.HorizontalOffset = (horizontalOffset < 0 ? 0 : horizontalOffset);
                double verticalOffset = halfTheGridHeight - halfThePopupHeight;
                this._dialogPopup.VerticalOffset = (verticalOffset < 0 ? 0 : verticalOffset);



#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

                UIElement root = PlatformProxy.GetRootVisual(this._dialogPopup);
                Rect screen = new Rect(0, 0, root.RenderSize.Width, root.RenderSize.Height);

                



                if (this._dialogPopup.HorizontalOffset > screen.Right)
                {
                    this._dialogPopup.HorizontalOffset = (screen.Right / 2) - halfThePopupWidth;
                }
                if (this._dialogPopup.VerticalOffset > screen.Bottom)
                {
                    this._dialogPopup.VerticalOffset = (screen.Bottom / 2) - halfThePopupHeight;
                }

            }
        }

        private void SetupCurrentView()
        {
            if (this._cell != null && this._filterGrid != null && this._needSetupGrid)
            {
                RowsManager rowsManager = this.Cell.Row.Manager as RowsManager;

                RowsFilter rowsFilter = null;

                if (rowsManager != null)
                {
                    rowsFilter = rowsManager.RowFiltersCollectionResolved[this.Cell.Column.Key];

                    if (rowsFilter == null && rowsManager.DataManager != null)
                    {
                        rowsFilter = new RowsFilter(rowsManager.DataManager.CachedType, this.Cell.Column);
                        rowsManager.RowFiltersCollectionResolved.Add(rowsFilter);
                    }
                }

                this.BuildInfoObjectTree(rowsFilter);

                ComboBoxColumn comboBoxColumn = this._filterGrid.Columns["FilterOperand"] as ComboBoxColumn;
                
                comboBoxColumn.ItemsSource = this._cell.Column.FilterColumnSettings.RowFilterOperands;
                    
                this.SetupGroupColumns();

                if (this._infoObjects.Count == 0 && this._defaultFilterOperand != null && this._defaultFilterOperand.Count > 0)
                {
                    this._masterParentGroup.Operator = LogicalOperator.And;

                    foreach (FilterOperand fo in this._defaultFilterOperand)
                    {
                        FilterOperand foundFilterOperand = this.FindFilterOperand(fo);
                        if (foundFilterOperand != null)
                        {
                            XamGridConditionInfo info = new XamGridConditionInfo(this._masterParentGroup);
                            info.FilterOperand = foundFilterOperand;
                            this._infoObjects.Add(info);
                        }
                    }
                }                

                this._needSetupGrid = false;

                this.BuildFilterDescription();
            }
        }

        

        private void BuildInfoObjectTree(RowsFilter rowsFilter)
        {
            this._infoObjects = null;

            this._infoObjects = new ObservableCollection<XamGridConditionInfo>();

            this._filterGrid.ItemsSource = this.InfoObjects;
            
            if (rowsFilter != null && rowsFilter.Conditions.Count > 0)
            {
                this.BuildInfoObjectTree(rowsFilter.Conditions, this._masterParentGroup);
            }
        }

        private void BuildInfoObjectTree(ConditionCollection collection, XamGridConditionInfoGroup group)
        {
            foreach (IFilterCondition conditionBase in collection)
            {
                if (conditionBase is ComparisonCondition)
                {
                    ComparisonCondition comparisonCondition = (ComparisonCondition)conditionBase;
                    XamGridConditionInfo info = new XamGridConditionInfo(group);
                    FilterOperand foundFilterOperand = FindFilterOperand(comparisonCondition.Operator);
                    if (foundFilterOperand != null)
                    {
                        info.FilterValue = comparisonCondition.FilterValue;
                        info.FilterOperand = foundFilterOperand;
                        this._infoObjects.Add(info);
                    }
                }
                else if (conditionBase is ConditionGroup)
                {
                    ConditionGroup conditionGroup = (ConditionGroup)conditionBase;

                    XamGridConditionInfoGroup newGroup = new XamGridConditionInfoGroup(group);

                    newGroup.Operator = conditionGroup.Conditions.LogicalOperator;

                    BuildInfoObjectTree(conditionGroup.Conditions, newGroup);
                }
                else if (conditionBase is CustomComparisonCondition)
                {
                    CustomComparisonCondition comparisionCondition = (CustomComparisonCondition)conditionBase;
                    XamGridConditionInfo info = new XamGridConditionInfo(group);

                    FilterOperand foundFilterOperand = this.FindFilterOperand(comparisionCondition.FilterOperand);
                    if (foundFilterOperand != null)
                    {
                        info.FilterValue = comparisionCondition.FilterValue;
                        info.FilterOperand = foundFilterOperand;
                        this._infoObjects.Add(info);
                    }
                }
            }
        }

        private FilterOperand FindFilterOperand(ComparisonOperator op)
        {
            foreach (FilterOperand filterOp in this._cell.Column.FilterColumnSettings.RowFilterOperands)
            {
                if (filterOp.ComparisonOperatorValue == op)
                {
                    return filterOp;
                }
            }
            return null;
        }

        private FilterOperand FindFilterOperand(FilterOperand fo)
        {
            foreach (FilterOperand filterOp in this._cell.Column.FilterColumnSettings.RowFilterOperands)
            {
                if (filterOp.GetType() == fo.GetType())
                {
                    return filterOp;
                }
            }
            return null;
        }

        private FilterOperand FindFilterOperand(Type type)
        {
            foreach (FilterOperand filterOp in this._cell.Column.FilterColumnSettings.RowFilterOperands)
            {
                if (filterOp.GetType() == type)
                {
                    return filterOp;
                }
            }
            return null;
        }

        private void SetupGroupColumns()
        {
            if (this._filterGrid != null)
            {
                int maxDepth = 0;
                // first get the max depth of the groups
                foreach (XamGridConditionInfo info in this._infoObjects)
                {
                    maxDepth = Math.Max(maxDepth, info.Group.Level);
                }

                // so whatever the depth was, add 1 so we have enough columns
                maxDepth++;

                int numberOfGroupIndicatorColumns = 0;
                foreach (Column c in _filterGrid.Columns.DataColumns)
                {
                    if (c is GroupDisplayColumn)
                    {
                        numberOfGroupIndicatorColumns++;
                    }
                }

                if (numberOfGroupIndicatorColumns > maxDepth)
                {
                    for (int i = numberOfGroupIndicatorColumns; i > maxDepth; i--)
                    {
                        GroupDisplayColumn toRemove = null;
                        foreach (Column c in _filterGrid.Columns.DataColumns)
                        {
                            toRemove = c as GroupDisplayColumn;
                            if (toRemove != null)
                                break;
                        }
                        if (toRemove != null)
                        {
                            _filterGrid.Columns.Remove(toRemove);
                        }
                    }
                }
                else if (numberOfGroupIndicatorColumns < maxDepth)
                {
                    for (int i = numberOfGroupIndicatorColumns; i < maxDepth; i++)
                    {
                        _filterGrid.Columns.Insert(0, new GroupDisplayColumn() { HeaderText = " ", Key = "A" + _groupColumnKeyCounter });
                        _groupColumnKeyCounter++;
                    }
                }
            }
        }

        private void RemoveInfoFromCollection(XamGridConditionInfo removedObject)
        {
            // for right now we will just remove this item from the list, but i am thinking that the groups will know about
            // the items that belong to it.  So it will be helpful to have them in a method here.
            this._infoObjects.Remove(removedObject);

            removedObject.Group = null;
        }

        private void CreateDeeperLogicalGroup(LogicalOperator op)
        {
            if (this._isOpen && this._filterGrid != null && this._filterGrid.Rows.Count > 0)
            {
                if (this._filterGrid.SelectionSettings.SelectedRows.Count > 1)
                {
                    // only create a deeper group if all the rows are in the same group
                    XamGridConditionInfoGroup singleGroup = null;
                    bool allowGrouping = true;
                    List<XamGridConditionInfo> objectsToAnd = new List<XamGridConditionInfo>();
                    foreach (Row row in this._filterGrid.SelectionSettings.SelectedRows)
                    {
                        XamGridConditionInfo objectToAnd = (XamGridConditionInfo)row.Data;
                        objectsToAnd.Add(objectToAnd);
                        if (singleGroup == null)
                        {
                            singleGroup = objectToAnd.Group;
                        }
                        else
                        {
                            if (singleGroup != objectToAnd.Group)
                            {
                                allowGrouping = false;
                                break;
                            }
                        }
                    }

                    if (allowGrouping)
                    {
                        // so prior to making a new group, see if all the items in list are the full set of items in
                        // the existing group.  If they are, then don't make a new group.

                        if (singleGroup.InfoObjects.Count + singleGroup.ChildGroups.Count > objectsToAnd.Count)
                        {
                            XamGridConditionInfoGroup newGroup = new XamGridConditionInfoGroup(singleGroup);
                            newGroup.Operator = op;

                            var sortedList = objectsToAnd.Select(a => a).OrderBy(a => a.InfoObjectName);

                            foreach (XamGridConditionInfo info in sortedList)
                            {
                                info.Group = newGroup;
                            }

                            this.SetupGroupColumns();
                        }
                    }
                }

                this.BuildFilterDescription();

                this.FilterGrid.InvalidateData();
            }
        }

        private void BuildFilters(ConditionCollection conditions, XamGridConditionInfoGroup group)
        {
            conditions.LogicalOperator = group.Operator;

            RowsManager rowsManager = this.Cell.Row.Manager as RowsManager;
            XamGrid mainGrid = this.Cell.Column.ColumnLayout.Grid;
            foreach (XamGridConditionInfo info in group.InfoObjects)
            {
                if (info.FilterOperand != null && !info.HasError)
                {
                    if (info.FilterOperand.RequiresFilteringInput)
                    {
                        FilterOperand filterOperand = info.FilterOperand;
                        object newValue = info.FilterValue;

                        if (info.FilterValue != null)
                        {
                            if (filterOperand.ComparisonOperatorValue != null)
                            {
                                if (!mainGrid.OnFiltering
                                    (this._proxyColumn.ProxiedSource.Column,
                                    info.FilterOperand,
                                    rowsManager.RowFiltersCollectionResolved,
                                    newValue))
                                {
                                    conditions.AddItemSilently(new ComparisonCondition() { Operator = (ComparisonOperator)filterOperand.ComparisonOperatorValue, FilterValue = this._proxyColumn.ProxiedSource.Column.ResolveValue(newValue) });
                                }
                            }
                            else
                            {
                                System.Linq.Expressions.Expression expression = info.FilterOperand.FilteringExpression(info.FilterValue);
                                if (expression != null)
                                {
                                    if (!mainGrid.OnFiltering(this._proxyColumn.ProxiedSource.Column, info.FilterOperand, rowsManager.RowFiltersCollectionResolved, newValue))
                                    {
                                        conditions.AddItemSilently(new CustomComparisonCondition() { FilterOperand = filterOperand.GetType(), FilterValue = this._proxyColumn.ProxiedSource.Column.ResolveValue(newValue), Expression = expression });
                                    }
                                }
                            }
                        }

                        else
                        {
                            if (FilterRow.IsNullableValueType(this._proxyColumn.ProxiedSource.Column.DataType))
                            {                          
                                if (filterOperand.ComparisonOperatorValue != null)
                                {
                                    if (!mainGrid.OnFiltering
                                        (this._proxyColumn.ProxiedSource.Column,
                                        info.FilterOperand,
                                        rowsManager.RowFiltersCollectionResolved,
                                        null))
                                    {
                                        conditions.AddItemSilently(new ComparisonCondition() { Operator = (ComparisonOperator)filterOperand.ComparisonOperatorValue, FilterValue = null });
                                    }
                                }
                                else
                                {
                                    System.Linq.Expressions.Expression expression = info.FilterOperand.FilteringExpression(info.FilterValue);
                                    if (expression != null)
                                    {
                                        if (!mainGrid.OnFiltering(this._proxyColumn.ProxiedSource.Column, info.FilterOperand, rowsManager.RowFiltersCollectionResolved, newValue))
                                        {
                                            conditions.AddItemSilently(new CustomComparisonCondition() { FilterOperand = filterOperand.GetType(), FilterValue = null, Expression = expression });
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else if (!info.FilterOperand.RequiresFilteringInput)
                    {
                        FilterOperand filterOperand = info.FilterOperand;

                        if (filterOperand.ComparisonOperatorValue != null)
                        {
                            if (!mainGrid.OnFiltering
                                (this._proxyColumn.ProxiedSource.Column,
                                info.FilterOperand,
                                rowsManager.RowFiltersCollectionResolved,
                                null))
                            {
                                conditions.AddItemSilently(new ComparisonCondition() { Operator = (ComparisonOperator)filterOperand.ComparisonOperatorValue, FilterValue = null });
                            }
                        }
                        else
                        {
                            System.Linq.Expressions.Expression expression = info.FilterOperand.FilteringExpression(info.FilterValue);
                            if (expression != null)
                            {
                                if (!mainGrid.OnFiltering(this._proxyColumn.ProxiedSource.Column, info.FilterOperand, rowsManager.RowFiltersCollectionResolved, null))
                                {
                                    conditions.AddItemSilently(new CustomComparisonCondition() { FilterOperand = filterOperand.GetType(), FilterValue = null, Expression = expression });
                                }
                            }
                        }
                    }
                }
            }

            foreach (XamGridConditionInfoGroup subGroup in group.ChildGroups)
            {
                ConditionGroup gc = new ConditionGroup(conditions.Parent);
                BuildFilters(gc.Conditions, subGroup);
                conditions.AddItemSilently(gc);
            }
        }

        private void BuildFilterDescription()
        {
            if (this._masterParentGroup.ChildGroups.Count == 0 && this._masterParentGroup.InfoObjects.Count == 0)
            {
                this.FilterDescription = string.Empty;
            }
            else
            {
                string logicalANDstring = SRGrid.GetString("LogicalAND");
                string logicalORstring = SRGrid.GetString("LogicalOR");
                string filterString = string.Empty;

                if (this._masterParentGroup.InfoObjects.Count > 0)
                {
                    filterString += this.BuildFilterDescriptionFromInfoObjects(this._masterParentGroup.Operator, this._masterParentGroup.InfoObjects);
                }

                if (this._masterParentGroup.ChildGroups.Count > 0)
                {
                    bool addParens = false;
                    if (filterString.Length > 0)
                    {
                        if (this._masterParentGroup.Operator == LogicalOperator.And)
                        {
                            filterString += (" " + logicalANDstring + " (");
                        }
                        else
                        {
                            filterString += (" " + logicalORstring + " (");
                        }
                        addParens = true;
                    }

                    filterString += this.BuildFilterDescriptionFromInfoGroups(this._masterParentGroup.Operator, this._masterParentGroup.ChildGroups);

                    if (addParens)
                        filterString += " )";
                 }

                this.FilterDescription = filterString;
            }
        }

        private string BuildFilterDescriptionFromInfoObjects(LogicalOperator op, List<XamGridConditionInfo> list)
        {
            string filterString = string.Empty;

            if (this.Cell != null)
            {
                string columnName = this._cell.Column.DisplayNameResolved;

                string logicalANDstring = SRGrid.GetString("LogicalAND");
                string logicalORstring = SRGrid.GetString("LogicalOR");

                foreach (XamGridConditionInfo info in list)
                {
                    if (filterString.Length > 0)
                    {
                        if (op == LogicalOperator.And)
                        {
                            filterString += (" " + logicalANDstring + " ");
                        }
                        else
                        {
                            filterString += (" " + logicalORstring + " ");
                        }
                    }
                    // ColumnName FilterOperand.Name
                    // ColumnName FilterOperand.Name = '  '
                    if (info.FilterOperand != null)
                    {
                        if (info.FilterOperand.RequiresFilteringInput)
                        {
                            filterString += (columnName + " " + info.FilterOperand.DisplayName + " '" + (info.FilterValue == null ? "" : info.FilterValue.ToString()) + "'");
                        }
                        else
                        {
                            filterString += (columnName + " " + info.FilterOperand.DisplayName);
                        }
                    }
                }
            }
            return filterString;
        }

        private string BuildFilterDescriptionFromInfoGroups(LogicalOperator op, List<XamGridConditionInfoGroup> list)
        {
            string filterString = string.Empty;
            string logicalANDstring = SRGrid.GetString("LogicalAND");
            string logicalORstring = SRGrid.GetString("LogicalOR");

            foreach (XamGridConditionInfoGroup group in list)
            {
                if (filterString.Length > 0                   )
                {
                    filterString = "(" + filterString + ")";
                }

                if (filterString.Length > 0)
                {
                    if (op == LogicalOperator.And)
                    {
                        filterString += (" " + logicalANDstring + " ");
                    }
                    else
                    {
                        filterString += (" " + logicalORstring + " ");
                    }
                }

                bool presetParen = filterString.Length>0;

                if (presetParen)
                    filterString += "(";

                if (group.InfoObjects.Count > 0)
                {
                    filterString += this.BuildFilterDescriptionFromInfoObjects(group.Operator, group.InfoObjects);
                }

                if (group.ChildGroups.Count > 0)
                {
                    bool addParens = false;
                    if (filterString.Length > 0)
                    {
                        if (group.Operator == LogicalOperator.And)
                        {
                            filterString += (" " + logicalANDstring + " (");
                        }
                        else
                        {
                            filterString += (" " + logicalORstring + " (");
                        }
                        addParens = true;
                    }

                    filterString += this.BuildFilterDescriptionFromInfoGroups(group.Operator, group.ChildGroups);

                    if (addParens)
                        filterString += ")";
                }

                if (presetParen)
                    filterString += ")";
            }
            return filterString;
        }

        #endregion // Private
        
        #region Public

        #region Show

        /// <summary>
        /// Shows the popup
        /// </summary>
        /// <param name="initialOperands">A List of <see cref="FilterOperand"/> objects which this control will attempt to use to prepopulate
        /// the operand dropdowns with.
        /// </param>
        public virtual void Show(List<FilterOperand> initialOperands)
        {
            this._defaultFilterOperand = initialOperands;
            this.Show();
        }

        /// <summary>
        /// Shows the popup
        /// </summary>
        public virtual void Show()
        {
            this._infoObjects.Clear();
            this._masterParentGroup.ChildGroups.Clear();
            this._masterParentGroup.InfoObjects.Clear();

            if (this._dialogPopup != null && !this._isOpen)
            {
                this._isOpen = true;

                this._dialogPopup.Child = this;

                if (!this.Cell.Row.ColumnLayout.Grid.Panel.Children.Contains(this._dialogPopup))
                {
                    this.Cell.Row.ColumnLayout.Grid.Panel.Children.Add(this._dialogPopup);
                }

                FrameworkElement fe = (FrameworkElement)this._dialogPopup.Child;

                if (fe != null)
                    fe.SizeChanged += CompoundFilterDialogControl_SizeChanged;

                this._dialogPopup.IsOpen = true;

                this.SetupCurrentView();

                EditableColumn ec = this.Cell.Column as EditableColumn;

                XamGrid grid = this.Cell.Row.ColumnLayout.Grid;

                VisualStateManager.GoToState(grid, "ReadOnly", false);

                UIElement rootVis = PlatformProxy.GetRootVisual(grid);
                this._rootVis = rootVis;

                rootVis.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler), true);
                rootVis.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.MouseLeftButtonDownHandler), true);

                grid.SizeChanged += Grid_SizeChanged;

                if (grid.ColumnChooserDialog.IsOpen)
                {
                    grid.ColumnChooserDialog.IsOpen = false;
                    this._columnChooserWasOpen = true;
                }

                this.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler), true);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Focus();
                }));
            }
        }

        #endregion // Show

        #region Hide

        /// <summary>
        /// Hides the popup.
        /// </summary>
        public virtual void Hide()
        {
            this._defaultFilterOperand = null;

            if (this._dialogPopup != null && this._isOpen)
            {
                this._dialogPopup.IsOpen = false;
                this._isOpen = false;

                FrameworkElement fe = (FrameworkElement)this._dialogPopup.Child;
                if (fe != null)
                    fe.SizeChanged -= CompoundFilterDialogControl_SizeChanged;

                XamGrid grid = this.Cell.Row.ColumnLayout.Grid;

                if (this._rootVis != null)
                {
                    this._rootVis.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler));
                    this._rootVis.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.MouseLeftButtonDownHandler));
                }

                this.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler));
                grid.SizeChanged -= Grid_SizeChanged;
                this._dialogPopup.Child = null;

                if (this.Cell.Row.ColumnLayout.Grid.Panel.Children.Contains(this._dialogPopup))
                {
                    this.Cell.Row.ColumnLayout.Grid.Panel.Children.Remove(this._dialogPopup);
                }

                VisualStateManager.GoToState(grid, "Active", false);
            }

            if (this._columnChooserWasOpen)
            {
                if (this.Cell != null && this.Cell.Row != null && this.Cell.Row.ColumnLayout != null)
                    this.Cell.Row.ColumnLayout.Grid.ColumnChooserDialog.IsOpen = true;

                this._columnChooserWasOpen = false;
            }

            this._needSetupGrid = true;
        }

        #endregion // Hide

        #endregion // Public

        #region Protected

        #region AcceptChanges

        internal void AcceptChanges()
        {
            if (this._filterGrid.CurrentEditCell != null)
                this._filterGrid.ExitEditMode(false);

            RowsManager rowsManager = this.Cell.Row.Manager as RowsManager;

            RowsFilter rowsFilter = null;

            if (rowsManager != null)
            {
                rowsFilter = rowsManager.RowFiltersCollectionResolved[this.Cell.Column.Key];

                if (rowsFilter == null && rowsManager.DataManager != null)
                {
                    rowsFilter = new RowsFilter(rowsManager.DataManager.CachedType, this.Cell.Column);
                    rowsManager.RowFiltersCollectionResolved.Add(rowsFilter);
                }
            }

            rowsFilter.Conditions.ClearSilently();

            this.BuildFilters(rowsFilter.Conditions, this._masterParentGroup);

            this.Cell.Column.ColumnLayout.Grid.OnFiltered(rowsManager.RowFiltersCollectionResolved);

            rowsManager.ColumnLayout.InvalidateFiltering();
        }

        #endregion // AcceptChanges

        #region GetParameter

        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {
            return this;
        }

        #endregion // GetParameter

        #endregion // Protected

        #region Internal

        internal void AddNewFilterCondition()
        {            
            if (this._isOpen && this._filterGrid != null)
            {
                this._filterGrid.ExitEditMode(false);

                XamGridConditionInfo newObject = null;
                if (this._filterGrid.ActiveCell == null)
                {
                    newObject = new XamGridConditionInfo(_masterParentGroup);
                }
                else
                {
                    newObject = new XamGridConditionInfo(((XamGridConditionInfo)this._filterGrid.ActiveCell.Row.Data).Group);
                }

                newObject.FilterOperand = this._cell.Column.FilterColumnSettings.RowFilterOperands[0];

                this._infoObjects.Add(newObject);

                this.BuildFilterDescription();

                this.EnsureVisualState();

                this.FilterGrid.InvalidateData();
            }
        }

        internal void RemoveConditions()
        {
            if (this._isOpen && this._filterGrid != null && this._filterGrid.Rows.Count > 0)
            {
                this._filterGrid.ExitEditMode(false);

                if (this._filterGrid.SelectionSettings.SelectedRows.Count > 0)
                {
                    // only delete if all the rows are in the same group
                    XamGridConditionInfoGroup singleGroup = null;
                    bool allowDelete = true;
                    List<XamGridConditionInfo> deadObjects = new List<XamGridConditionInfo>();
                    foreach (Row row in this._filterGrid.SelectionSettings.SelectedRows)
                    {
                        XamGridConditionInfo removedObject = (XamGridConditionInfo)row.Data;
                        deadObjects.Add(removedObject);
                        if (singleGroup == null)
                        {
                            singleGroup = removedObject.Group;
                        }
                        else
                        {
                            if (singleGroup != removedObject.Group)
                            {
                                allowDelete = false;
                                break;
                            }
                        }
                    }

                    if (allowDelete)
                    {
                        List<XamGridConditionInfoGroup> groupsWhoJustLostAMember = new List<XamGridConditionInfoGroup>();

                        foreach (XamGridConditionInfo info in deadObjects)
                        {
                            groupsWhoJustLostAMember.Add(info.Group);
                            this.RemoveInfoFromCollection(info);
                        }

                        List<XamGridConditionInfoGroup> groupsWhoHaveASingleMember = groupsWhoJustLostAMember.Where(s => s.InfoObjects.Count == 1).Distinct().OrderByDescending(s => s.Level).ToList();
                        if (groupsWhoHaveASingleMember.Count > 0)
                        {
                            // now we move any single members up a level.  
                            foreach (XamGridConditionInfoGroup group in groupsWhoHaveASingleMember)
                            {
                                if (group.InfoObjects.Count == 1)
                                {
                                    XamGridConditionInfo info = group.InfoObjects[0];

                                    if (info.Group != this._masterParentGroup)
                                    {
                                        info.Group = info.Group.ParentGroup;
                                    }
                                }
                            }
                        }

                        List<XamGridConditionInfoGroup> groupsWhoHaveAZeroMember = groupsWhoJustLostAMember.Where(s => s.InfoObjects.Count == 0).Distinct().OrderByDescending(s => s.Level).ToList();
                        if (groupsWhoHaveAZeroMember.Count > 0)
                        {
                            // now we move any single members up a level.  
                            foreach (XamGridConditionInfoGroup group in groupsWhoHaveAZeroMember)
                            {
                                group.ParentGroup = null;
                            }
                        }
                    }
                }

                this.BuildFilterDescription();

                this.SetupGroupColumns();

                this.EnsureVisualState();

                this.FilterGrid.InvalidateData();
            }
        }

        internal void CreateOrGroup()
        {
            this.CreateDeeperLogicalGroup(LogicalOperator.Or);
        }

        internal void CreateAndGroup()
        {
            this.CreateDeeperLogicalGroup(LogicalOperator.And);
        }

        internal void Ungroup()
        {
            // for ungrouping, for each selected row we move the item from the group it's in into the next one up until we
            // reach the top level group.

            if (this._isOpen && this._filterGrid != null && this._filterGrid.Rows.Count > 0)
            {
                if (this._filterGrid.SelectionSettings.SelectedRows.Count > 0)
                {
                    List<XamGridConditionInfoGroup> groupsWhoJustLostAMember = new List<XamGridConditionInfoGroup>();

                    foreach (Row r in this._filterGrid.SelectionSettings.SelectedRows)
                    {
                        XamGridConditionInfo info = (XamGridConditionInfo)r.Data;
                        if (info.Group != this._masterParentGroup)
                        {
                            groupsWhoJustLostAMember.Add(info.Group);

                            info.Group = info.Group.ParentGroup;
                        }
                    }

                    List<XamGridConditionInfoGroup> groupsWhoHaveASingleMember = groupsWhoJustLostAMember.Where(s => s.InfoObjects.Count == 1).Distinct().OrderByDescending(s => s.Level).ToList();
                    if (groupsWhoHaveASingleMember.Count > 0)
                    {
                        // now we move any single members up a level.  
                        foreach (XamGridConditionInfoGroup group in groupsWhoHaveASingleMember)
                        {
                            if (group.InfoObjects.Count == 1)
                            {
                                XamGridConditionInfo info = group.InfoObjects[0];

                                if (info.Group != this._masterParentGroup)
                                {
                                    info.Group = info.Group.ParentGroup;
                                }
                            }
                        }
                    }

                    List<XamGridConditionInfoGroup> groupsWhoHaveAZeroMember = groupsWhoJustLostAMember.Where(s => s.InfoObjects.Count == 0 && s.ChildGroups.Count == 0).Distinct().OrderByDescending(s => s.Level).ToList();
                    if (groupsWhoHaveAZeroMember.Count > 0)
                    {
                        // now we move any single members up a level.  
                        foreach (XamGridConditionInfoGroup group in groupsWhoHaveAZeroMember)
                        {
                            group.ParentGroup = null;
                        }
                    }

                    this.SetupGroupColumns();

                    // Hack for now:  I really should set it up so that the 
                    this._filterGrid.InvalidateData();
                }
            }

            this.BuildFilterDescription();
        }

        internal void ToggleGroup()
        {
            if (this._isOpen && this._filterGrid != null && this._filterGrid.Rows.Count > 0)
            {
                this._filterGrid.ExitEditMode(false);

                if (this._filterGrid.SelectionSettings.SelectedRows.Count > 0)
                {
                    // only create a deeper group if all the rows are in the same group
                    XamGridConditionInfoGroup singleGroup = null;
                    bool allowGrouping = true;
                    foreach (Row row in this._filterGrid.SelectionSettings.SelectedRows)
                    {
                        XamGridConditionInfo objectToAnd = (XamGridConditionInfo)row.Data;

                        if (singleGroup == null)
                        {
                            singleGroup = objectToAnd.Group;
                        }
                        else
                        {
                            if (singleGroup != objectToAnd.Group)
                            {
                                allowGrouping = false;
                                break;
                            }
                        }
                    }

                    if (allowGrouping)
                    {
                        singleGroup.Operator = singleGroup.Operator == LogicalOperator.And ? LogicalOperator.Or : LogicalOperator.And;
                        this._filterGrid.InvalidateData();
                    }
                }

                this.BuildFilterDescription();

                this.EnsureVisualState();
            }
        }

        #endregion // Internal

        #endregion // Methods

        #region Overrides

        /// <summary>
        /// Builds the visual tree for the <see cref="CompoundFilterDialogControl"/>
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this._filterGrid != null)
            {
                this._filterGrid.SelectedRowsCollectionChanged -= FilterGrid_SelectedRowsCollectionChanged;
                this._filterGrid.CellEnteringEditMode -= FilterGrid_CellEnteringEditMode;
                this._filterGrid.CellExitingEditMode -= FilterGrid_CellExitingEditMode;
                this._filterGrid.CellExitedEditMode -= FilterGrid_CellExitedEditMode;
            }

            this._filterGrid = base.GetTemplateChild("FilterGrid") as XamGrid;

            if (this._filterGrid != null)
            {
                this._filterGrid.ItemsSource = _infoObjects;

                this._filterGrid.SelectedRowsCollectionChanged += FilterGrid_SelectedRowsCollectionChanged;
                this._filterGrid.CellEnteringEditMode += FilterGrid_CellEnteringEditMode;
                this._filterGrid.CellExitingEditMode += FilterGrid_CellExitingEditMode;
                this._filterGrid.CellExitedEditMode += FilterGrid_CellExitedEditMode;

                this._proxyColumn = this._filterGrid.Columns["FilterValue"] as ProxyColumn;

                this._proxyColumn.ProxiedSource = this.Cell;

                if (this._cell != null)
                {
                    



                    ComboBoxColumn comboBoxColumn = this._filterGrid.Columns["FilterOperand"] as ComboBoxColumn;
                    if (comboBoxColumn != null)
                    {
                        comboBoxColumn.ItemsSource = this._cell.Column.FilterColumnSettings.RowFilterOperands;
                    }
                }
            }

            this.SetupCurrentView();

            this.EnsureVisualState();
        }

        private void EnsureVisualState()
        {
            this.CheckButtonState();
        }

        #region CheckButtonState
        /// <summary>
        /// Calls to the CommandSourceManager to update the UI so that the controls that are going to be doing the commanding are updated appropriately.
        /// </summary>
        protected virtual void CheckButtonState()
        {
            CommandSourceManager.NotifyCanExecuteChanged(typeof(AddToCurrentCompoundFilterDialogCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(RemoveConditionsCompoundFilterDialogCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(CreateAndGroupCompoundFilterDialogCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(CreateOrGroupCompoundFilterDialogCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(UngroupCompoundFilterDialogCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(ToggleGroupCompoundFilterDialogCommand));
        }
        #endregion

        #region EventHandler

        void FilterGrid_SelectedRowsCollectionChanged(object sender, SelectionCollectionChangedEventArgs<SelectedRowsCollection> e)
        {
            this.EnsureVisualState();
        }

        void FilterGrid_CellEnteringEditMode(object sender, BeginEditingCellEventArgs e)
        {
            if (e.Cell.Column.Key == "FilterValue")
            {
                XamGridConditionInfo data = (XamGridConditionInfo)e.Cell.Row.Data;

                if (!data.FilterOperand.RequiresFilteringInput)
                {
                    e.Cancel = true;
                }
            }
        }

        bool _ignoreCellExitingEditMode = false;

        void FilterGrid_CellExitingEditMode(object sender, ExitEditingCellEventArgs e)
        {
            if (!_ignoreCellExitingEditMode)
            {
                if (e.Cell.Column.Key == "FilterValue")
                {
                    XamGridConditionInfo data = (XamGridConditionInfo)e.Cell.Row.Data;
                    try
                    {
                        this._proxyColumn.ProxiedSource.Column.ResolveValue(e.NewValue);
                        data.ErrorMessage = string.Empty;
                    }
                    catch (Exception exception)
                    {
                        data.ErrorMessage = exception.Message;
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        void FilterGrid_CellExitedEditMode(object sender, CellExitedEditingEventArgs e)
        {
            this.BuildFilterDescription();
        }


        #region DialogPopup_Closed

        void DialogPopup_Closed(object sender, EventArgs e)
        {
            this.Hide();
        }

        #endregion // DialogPopup_Closed

        #region DialogPopup_Opened
        void DialogPopup_Opened(object sender, EventArgs e)
        {
            this.AlignPopup();
        }
        #endregion // DialogPopup_Opened


        #endregion // EventHandlers

        #endregion // Overrides

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return command is CompoundFilterDialogCommandBase;
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
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
        /// <param name="name"></param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion        
    }
}

namespace Infragistics.Controls.Grids.Primitives
{
    #region CompoundFilterDialogControlCommand

    /// <summary>
    /// An enum describing the commands which can be executed on the <see cref="CompoundFilteringDialogControlCommandSource"/>
    /// </summary>
    public enum CompoundFilterDialogControlCommand
    {
        /// <summary>
        /// Accepts the changes made by the dialog.
        /// </summary>
        Accept,

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        Close,

        AddToCurrent,

        DeleteCondition,

        CreateAndGroup,

        CreateOrGroup,

        Ungroup,

        ToggleGroup
    }

    #endregion // CompoundFilterDialogControlCommand

    #region CompoundFilteringDialogControlCommandSource
    /// <summary>
    /// The command source object for <see cref="Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl"/>.
    /// </summary>
    public class CompoundFilteringDialogControlCommandSource : CommandSource
    {
        #region Properties

        #region CommandType
        /// <summary>
        /// Gets / sets the <see cref="CompoundFilterDialogControlCommand"/> which is to be executed by the command.
        /// </summary>
        public CompoundFilterDialogControlCommand CommandType
        {
            get;
            set;
        }
        #endregion // CommandType

        #endregion // Properties

        #region Overrides

        #region ResolveCommand
        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            ICommand command = null;
            switch (this.CommandType)
            {
                case CompoundFilterDialogControlCommand.Accept:
                    {
                        command = new AcceptCompoundFilterDialogChangesCommand();
                        break;
                    }
                case CompoundFilterDialogControlCommand.Close:
                    {
                        command = new CloseCompoundFilterDialogCommand();
                        break;
                    }
                case CompoundFilterDialogControlCommand.AddToCurrent:
                    {
                        command = new AddToCurrentCompoundFilterDialogCommand();
                        break;
                    }
                case CompoundFilterDialogControlCommand.DeleteCondition:
                    {
                        command = new RemoveConditionsCompoundFilterDialogCommand();
                        break;
                    }
                case CompoundFilterDialogControlCommand.CreateAndGroup:
                    {
                        command = new CreateAndGroupCompoundFilterDialogCommand();
                        break;
                    }
                case CompoundFilterDialogControlCommand.CreateOrGroup:
                    {
                        command = new CreateOrGroupCompoundFilterDialogCommand();
                        break;
                    }
                case CompoundFilterDialogControlCommand.Ungroup:
                    {
                        command = new UngroupCompoundFilterDialogCommand();
                        break;
                    }
                case CompoundFilterDialogControlCommand.ToggleGroup:
                    {
                        command = new ToggleGroupCompoundFilterDialogCommand();
                        break;
                    }
            }
            return command;
        }

        #endregion // ResolveCommand

        #endregion // Overrides        
    }
    #endregion // CompoundFilteringDialogControlCommandSource

    #region CompoundFilterDialogCommand

    /// <summary>
    /// A command that is used with the <see cref="Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl"/>.
    /// </summary>
    public class CompoundFilterDialogCommand : CommandBase
    {
        #region Methods

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion // CanExecute

        #endregion // Methods
    }

    #endregion // CompoundFilterDialogCommand

    #region ShowCompoundFilterDialogCommand

    /// <summary>
    /// A command which will show the <see cref="Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl"/>.
    /// </summary>
    public class ShowCompoundFilterDialogCommand : CellBaseCommandBase
    {
        #region Methods

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }

        #endregion // CanExecute

        #region ExecuteCommand
        /// <summary>
        /// Executes the specific command on the specified <see cref="CellBase"/>
        /// </summary>
        /// <param propertyName="col"></param>
        protected override void ExecuteCommand(CellBase cell)
        {
            XamGrid grid = cell.Row.ColumnLayout.Grid;
            grid.Panel.CompoundFilterDialogControl.Cell = cell;
            grid.Panel.CompoundFilterDialogControl.Show();
            this.CommandSource.Handled = false;
        }
        #endregion // ExecuteCommand

        #endregion // Methods
    }
    #endregion // ShowCompoundFilterDialogCommand
}

namespace Infragistics.Controls.Grids.Primitives
{
    #region CompoundFilteringDialogCommand
    /// <summary>
    /// An enum describing the commands which can be executed on the <see cref="CompoundFilteringDialogCommandSource"/>
    /// </summary>
    public enum CompoundFilteringDialogCommand
    {
        /// <summary>
        /// Shows the custome filter dialog.
        /// </summary>
        ShowCompoundFilterDialog
    }
    #endregion // CompoundFilteringDialogCommand

    #region CompoundFilteringDialogCommandSource

    /// <summary>
    /// The command source object for <see cref="Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl"/>.
    /// </summary>
    public class CompoundFilteringDialogCommandSource : CommandSource
    {
        #region CommandType
        /// <summary>
        /// Gets / sets the <see cref="CompoundFilteringDialogCommand"/> which is to be executed by the command.
        /// </summary>
        public CompoundFilteringDialogCommand CommandType
        {
            get;
            set;
        }

        #endregion // CommandType

        #region Overrides

        #region ResolveCommand
        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            ICommand command = null;
            switch (this.CommandType)
            {
                case CompoundFilteringDialogCommand.ShowCompoundFilterDialog:
                    {
                        command = new ShowCompoundFilterDialogCommand();
                        break;
                    }
            }
            return command;
        }
        #endregion // ResolveCommand

        #endregion // Overrides
    }

    #endregion // CompoundFilteringDialogCommandSource

    #region AcceptCompoundFilterDialogChangesCommand

    /// <summary>
    /// A command which will be used for the Accept action on the <see cref="Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl"/>.
    /// </summary>
    public class AcceptCompoundFilterDialogChangesCommand : CompoundFilterDialogCommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                control.AcceptChanges();
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }
    #endregion // AcceptCompoundFilterDialogChangesCommand

    #region CloseCompoundFilterDialogCommand
    /// <summary>
    /// A command that will close the <see cref="Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl"/>.
    /// </summary>
    public class CloseCompoundFilterDialogCommand : CompoundFilterDialogCommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                control.Hide(); ;
                VisualStateManager.GoToState(control.Cell.Row.ColumnLayout.Grid, "Active", false);
                control.Cell = null;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }
    #endregion // CloseCompoundFilterDialogCommand

    #region AddToCurrentCompoundFilterDialogCommand

    public class AddToCurrentCompoundFilterDialogCommand : CompoundFilterDialogCommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                control.AddNewFilterCondition(); ;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }

    #endregion // AddToCurrentCompoundFilterDialogCommand

    #region RemoveConditionsCompoundFilterDialogCommand

    public class RemoveConditionsCompoundFilterDialogCommand : CompoundFilterDialogCommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                return control.FilterGrid.SelectionSettings.SelectedRows.Count > 0;
            }

            return true;
        }
        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                control.RemoveConditions(); ;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }

    #endregion // RemoveConditionsCompoundFilterDialogCommand

    #region CompoundFilterDialogCommandBase
    public abstract class CompoundFilterDialogCommandBase : CommandBase
    {
    }
    #endregion // CompoundFilterDialogCommandBase

    #region CreateLogicalOperatorCompoundFilterDialogCommand
    public abstract class CreateLogicalOperatorCompoundFilterDialogCommand : CompoundFilterDialogCommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            bool allowGrouping = true;

            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;

            if (control != null)
            {
                if (control.FilterGrid.SelectionSettings.SelectedRows.Count > 1)
                {
                    XamGridConditionInfoGroup singleGroup = null;

                    foreach (Row row in control.FilterGrid.SelectionSettings.SelectedRows)
                    {
                        XamGridConditionInfo objectToAnd = (XamGridConditionInfo)row.Data;

                        if (singleGroup == null)
                        {
                            singleGroup = objectToAnd.Group;
                        }
                        else
                        {
                            if (singleGroup != objectToAnd.Group)
                            {
                                allowGrouping = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    allowGrouping = false;
                }

            }
            return allowGrouping;
        }
        #endregion // CanExecute
    }
    #endregion // CreateLogicalOperatorCompoundFilterDialogCommand

    #region CreateAndGroupCompoundFilterDialogCommand

    public class CreateAndGroupCompoundFilterDialogCommand : CreateLogicalOperatorCompoundFilterDialogCommand
    {
        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                control.CreateAndGroup(); ;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }

    #endregion // CreateAndGroupCompoundFilterDialogCommand

    #region CreateOrGroupCompoundFilterDialogCommand

    public class CreateOrGroupCompoundFilterDialogCommand : CreateLogicalOperatorCompoundFilterDialogCommand
    {
        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                control.CreateOrGroup(); ;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }

    #endregion // CreateOrGroupCompoundFilterDialogCommand

    #region UngroupCompoundFilterDialogCommand

    public class UngroupCompoundFilterDialogCommand : CompoundFilterDialogCommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                // only delete if all the rows are in the same group

                bool allowUngroup = false;

                foreach (Row row in control.FilterGrid.SelectionSettings.SelectedRows)
                {
                    XamGridConditionInfo ungroupingObject = (XamGridConditionInfo)row.Data;

                    if (control.MasterParentGroup != ungroupingObject.Group)
                    {
                        allowUngroup = true;
                        break;
                    }
                }

                return allowUngroup;
            }

            return false;
        }
        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                control.Ungroup();
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }

    #endregion // UngroupCompoundFilterDialogCommand

    #region ToggleGroupCompoundFilterDialogCommand

    public class ToggleGroupCompoundFilterDialogCommand : CompoundFilterDialogCommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            bool retValue = false;
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                if (control.FilterGrid.SelectionSettings.SelectedRows.Count > 0)
                {
                    XamGridConditionInfoGroup singleGroup = null;

                    bool allowGrouping = true;

                    foreach (Row row in control.FilterGrid.SelectionSettings.SelectedRows)
                    {
                        XamGridConditionInfo objectToAnd = (XamGridConditionInfo)row.Data;

                        if (singleGroup == null)
                        {
                            singleGroup = objectToAnd.Group;
                        }
                        else
                        {
                            if (singleGroup != objectToAnd.Group)
                            {
                                allowGrouping = false;
                                break;
                            }
                        }
                    }

                    retValue = allowGrouping;
                }
            }
            return retValue;
        }
        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            CompoundFilterDialogControl control = parameter as CompoundFilterDialogControl;
            if (control != null)
            {
                control.ToggleGroup(); ;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }

    #endregion // ToggleGroupCompoundFilterDialogCommand
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