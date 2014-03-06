using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// The <see cref="FilterSelectionControl"/> is displayed when when FilterMenu filtering is used. 
    /// </summary>
    [TemplatePart(Name = "SelectAllCheckBox", Type = typeof(CheckBox))]    
    [TemplateVisualState(GroupName = "SelectionBoxWaterMark", Name = "HideSelectionBoxWaterMark")]
    [TemplateVisualState(GroupName = "SelectionBoxWaterMark", Name = "ShowSelectionBoxWaterMark")]
    [TemplateVisualState(GroupName = "AppendButtonVisibility", Name = "ShowAppendFilterButton")]
    [TemplateVisualState(GroupName = "AppendButtonVisibility", Name = "HideAppendFilterButton")]
    [TemplateVisualState(GroupName = "BusyStatus", Name = "Busy")]
    [TemplateVisualState(GroupName = "BusyStatus", Name = "Idle")]
    [TemplateVisualState(GroupName = "LimitItemsStatus", Name = "Limited")]
    [TemplateVisualState(GroupName = "LimitItemsStatus", Name = "NotLimited")]
    public class FilterSelectionControl : SelectionControl
    {
        #region Members

        internal const int Limit = 10000;
        private bool _boolBuildCachedList = true;
        private bool _itemSourceSet;
        private bool _hasFilters;
        private bool _hasCheckedItems;
        private bool _isMassEdit;
        private bool _changeDetected;
        private bool _isBusy;
        private bool _isInDesignMode;
        private bool _allowFiltering;
        private string _formatString;
        private string _nullString, _emptyString;
        private string _filterText;
        private FilterItemCollection _uniqueValues;
        private FilterItemCollection _cachedUniqueValues;
        private CancellationTokenSource _phase1TokenSource;
        private List<CancellationTokenSource> _phase2TokenSourceQueue = new List<CancellationTokenSource>();
        private DispatcherTimer _waitSearchStringTimer;
        private DispatcherTimer _busyIndicatorStartTimer;

        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="FilterSelectionControl"/> class.
        /// </summary>
        static FilterSelectionControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterSelectionControl), new FrameworkPropertyMetadata(typeof(FilterSelectionControl)));
            FocusableProperty.OverrideMetadata(typeof(FilterSelectionControl), new FrameworkPropertyMetadata(false));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FilterSelectionControl"/> class.
        /// </summary>
        public FilterSelectionControl()
        {




            this._isInDesignMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode(this);
            this.Loaded += new RoutedEventHandler(FilterSelectionControl_Loaded);
            this.Unloaded += new RoutedEventHandler(FilterSelectionControl_Unloaded);

            this.OKButtonText = SRGrid.GetString("FilterSelectionControl_OKButtonText");
            this.CancelButtonText = SRGrid.GetString("FilterSelectionControl_CancelButtonText");
            this.AppendFilterText = SRGrid.GetString("AppendFilterText");

            this.NotAllItemsShowingText = SRGrid.GetString("FilterBoxNotAllItemsShowing");
            this.NotAllItemsShowingTooltipText = string.Format(SRGrid.GetString("FilterBoxNotAllItemsShowingTooltip"), Limit);
            this._busyIndicatorStartTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 250) };
        }

        #endregion // Constructor

        #region Static
        /// <summary>
        /// Gets the underlying type of a Nullable Type.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        protected internal static Type GetUnderlyingNonNullableType(Type dataType)
        {
            if (dataType != null)
            {
                if (dataType.IsGenericType && dataType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    Type newType = Nullable.GetUnderlyingType(dataType);

                    return newType;
                }
            }
            return dataType;
        }
        #endregion // Static

        #region Properties

        #region IsLoaded

        internal bool IsLoaded { get; private set; }

        #endregion // IsLoaded

        #region IsLoadingData

        private bool IsLoadingData { get; set; }

        #endregion // IsLoadingData

        #region DelayIsBusy

        private bool DelayIsBusy { get; set; }

        #endregion // DelayIsBusy

        #region IsBusy

        protected bool IsBusy
        {
            get
            {
                return this._isBusy;
            }

            set
            {
                this._isBusy = value;

                if (value == false)
                {
                    this.DelayIsBusy = false;
                }
            }
        }

        #endregion // IsBusy

        #region IsLimited

        protected bool IsLimited { get; set; }

        #endregion // IsLimited

        #region ItemSourceSet
        
        /// <summary>
        /// Gets / sets if the <see cref="FilterSelectionControl"/> is attached to a <see cref="XamGrid"/> that has it's ItemSource set.
        /// </summary>
        public bool ItemSourceSet
        {
            get
            {
                return this._itemSourceSet;
            }
            set
            {
                if (this._itemSourceSet != value)
                {
                    this._itemSourceSet = value;
                    this.OnPropertyChanged("ItemSourceSet");
                }
            }
        }

        #endregion // ItemSourceSet

        #region ClearFiltersText

        /// <summary>
        /// Identifies the <see cref="ClearFiltersText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ClearFiltersTextProperty = DependencyProperty.Register("ClearFiltersText", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata(new PropertyChangedCallback(ClearFiltersTextChanged)));

        /// <summary>
        /// Gets / sets the text that will be used for the ClearFilters button.
        /// </summary>
        public string ClearFiltersText
        {
            get { return (string)this.GetValue(ClearFiltersTextProperty); }
            set { this.SetValue(ClearFiltersTextProperty, value); }
        }

        private static void ClearFiltersTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl fsc = (FilterSelectionControl)obj;
            fsc.OnPropertyChanged("ClearFiltersText");
        }

        #endregion // ClearFiltersText

        #region TypeSpecificFiltersText

        /// <summary>
        /// Identifies the <see cref="TypeSpecificFiltersText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty TypeSpecificFiltersTextProperty = DependencyProperty.Register("TypeSpecificFiltersText", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata(new PropertyChangedCallback(TypeSpecificFiltersTextChanged)));

        /// <summary>
        /// Gets / sets the text that will be use for the type specific filter button.
        /// </summary>
        public string TypeSpecificFiltersText
        {
            get { return (string)this.GetValue(TypeSpecificFiltersTextProperty); }
            set { this.SetValue(TypeSpecificFiltersTextProperty, value); }
        }

        private static void TypeSpecificFiltersTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl fsc = (FilterSelectionControl)obj;
            fsc.OnPropertyChanged("TypeSpecificFiltersText");
        }

        #endregion // TypeSpecificFiltersText

        #region CancelButtonText

        /// <summary>
        /// Identifies the <see cref="CancelButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register("CancelButtonText", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata( new PropertyChangedCallback(CancelButtonTextChanged)));

        /// <summary>
        /// Gets/Sets the text that will be displayed in the Cancel button of the <see cref="FilterSelectionControl"/>
        /// </summary>
        public string CancelButtonText
        {
            get { return (string)this.GetValue(CancelButtonTextProperty); }
            set { this.SetValue(CancelButtonTextProperty, value); }
        }

        private static void CancelButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl fsc = (FilterSelectionControl)obj;
            fsc.OnPropertyChanged("CancelButtonText");
        }

        #endregion // CancelButtonText

        #region OKButtonText

        /// <summary>
        /// Identifies the <see cref="OKButtonText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty OKButtonTextProperty = DependencyProperty.Register("OKButtonText", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata( new PropertyChangedCallback(OKButtonTextChanged)));

        /// <summary>
        /// Gets/Sets the text that will be displayed in the OK button of the <see cref="FilterSelectionControl"/>
        /// </summary>
        public string OKButtonText
        {
            get { return (string)this.GetValue(OKButtonTextProperty); }
            set { this.SetValue(OKButtonTextProperty, value); }
        }

        private static void OKButtonTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl fsc = (FilterSelectionControl)obj;
            fsc.OnPropertyChanged("OKButtonText");
        }

        #endregion // OKButtonText

        #region BusyText

        /// <summary>
        /// Gets the text that will be displayed when <see cref="FilterSelectionControl"/> is loading its items.
        /// </summary>
        public string BusyText
        {
            get { return SRGrid.GetString("FilterBoxLoading"); }
        }

        #endregion // BusyText

        #region NotAllItemsShowingText

        /// <summary>
        /// Identifies the <see cref="NotAllItemsShowingText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NotAllItemsShowingTextProperty = DependencyProperty.Register("NotAllItemsShowingText", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata(new PropertyChangedCallback(NotAllItemsShowingTextChanged)));

        /// <summary>
        /// Gets or sets the text that will be dipsplayed when the items in the <see cref="FilterSelectionControl"/> are limited.
        /// </summary>
        public string NotAllItemsShowingText
        {
            get { return (string) this.GetValue(NotAllItemsShowingTextProperty); }
            set { this.SetValue(NotAllItemsShowingTextProperty, value); }
        }

        private static void NotAllItemsShowingTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl fsc = (FilterSelectionControl)obj;
            fsc.OnPropertyChanged("NotAllItemsShowingText");
        }

        #endregion // NotAllItemsShowingText 

        #region NotAllItemsShowingTooltipText

        /// <summary>
        /// Identifies the <see cref="NotAllItemsShowingTooltipText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NotAllItemsShowingTooltipTextProperty = DependencyProperty.Register("NotAllItemsShowingTooltipText", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata(new PropertyChangedCallback(NotAllItemsShowingTooltipTextChanged)));

        /// <summary>
        /// Gets or sets the tooltip text describing why the items are limited.
        /// </summary>
        public string NotAllItemsShowingTooltipText
        {
            get { return (string) this.GetValue(NotAllItemsShowingTooltipTextProperty); }
            set { this.SetValue(NotAllItemsShowingTooltipTextProperty, value); }
        }

        private static void NotAllItemsShowingTooltipTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl fsc = (FilterSelectionControl)obj;
            fsc.OnPropertyChanged("NotAllItemsShowingTooltipText");
        }

        #endregion // NotAllItemsShowingTooltipText 

        #region SelectAllCheckBox

        /// <summary>
        /// Gets / sets the select all checkbox in the <see cref="FilterSelectionControl"/>.
        /// </summary>
        protected internal CheckBox SelectAllCheckBox { get; set; }

        #endregion // SelectAllCheckBox

        #region UniqueValues

        /// <summary>
        /// Gets a list of UniqueValues for the FilterSelectionControl.
        /// </summary>
        public virtual FilterItemCollection UniqueValues
        {
            get
            {
                if (this._uniqueValues == null)
                {
                    this._uniqueValues = this.GetCollection();
                    
                    if (this._uniqueValues != null)
                    {
                        this._uniqueValues.CollectionChanged += new NotifyCollectionChangedEventHandler(UniqueValues_CollectionChanged);    
                    }
                }
                return this._uniqueValues;
            }
        }

        #endregion // UniqueValues

        #region FilterableUniqueValues

        /// <summary>
        /// Gets a list of unique values which are filterable.
        /// </summary>
        public virtual IEnumerable FilterableUniqueValues
        {
            get
            {
                if (this.UniqueValues == null || this.UniqueValues.Count == 0)
                {
                    return null;
                }

                return this.UniqueValues;
            }
        }
        
        #endregion // FilterableUniqueValues

        #region ClearFiltersTextResolved

        /// <summary>
        /// Identifies the <see cref="ClearFiltersTextResolved"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ClearFiltersTextResolvedProperty = DependencyProperty.Register("ClearFiltersTextResolved", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata(new PropertyChangedCallback(ClearFiltersTextResolvedChanged)));

        /// <summary>
        /// Gets/Sets the text for the <see cref="ClearFilters"/> in the <see cref="FilterSelectionControl"/>
        /// </summary>
        public string ClearFiltersTextResolved
        {
            get { return (string)this.GetValue(ClearFiltersTextResolvedProperty); }
            set { this.SetValue(ClearFiltersTextResolvedProperty, value); }
        }

        private static void ClearFiltersTextResolvedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl ctrl = (FilterSelectionControl)obj;
            ctrl.OnPropertyChanged("ClearFiltersTextResolved");
        }

        #endregion // ClearFiltersTextResolved

        #region TypeSpecificFiltersTextResolved

        /// <summary>
        /// Identifies the <see cref="TypeSpecificFiltersTextResolved"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty TypeSpecificFiltersTextResolvedProperty = DependencyProperty.Register("TypeSpecificFiltersTextResolved", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata(new PropertyChangedCallback(TypeSpecificFiltersTextResolvedChanged)));

        /// <summary>
        /// Gets/Sets the Text used for types in the <see cref="FilterSelectionControl"/>
        /// </summary>
        public string TypeSpecificFiltersTextResolved
        {
            get { return (string)this.GetValue(TypeSpecificFiltersTextResolvedProperty); }
            set { this.SetValue(TypeSpecificFiltersTextResolvedProperty, value); }
        }

        private static void TypeSpecificFiltersTextResolvedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl ctrl = (FilterSelectionControl)obj;
            ctrl.OnPropertyChanged("TypeSpecificFiltersTextResolved");
        }

        #endregion // TypeSpecificFiltersTextResolved

        #region CheckedCount

        /// <summary>
        /// Gets / sets the number of elements which were checked.
        /// </summary>
        protected int CheckedCount { get; set; }

        #endregion // CheckedCount

        #region UncheckedCount

        /// <summary>
        /// Gets / sets the number of elements which were unchecked.
        /// </summary>
        protected int UncheckedCount { get; set; }

        #endregion // UncheckedCount

        #region RawValues

        /// <summary>
        /// Gets / sets the list of values retrieved from the InformationContext.
        /// </summary>
        protected IList RawValues { get; set; }

        #endregion // RawValues

        #region FilterText

        /// <summary>
        /// Gets / sets the text that will be used to filter the selections.
        /// </summary>
        public string FilterText
        {
            get { return this._filterText; }
            set
            {
                if (this._filterText != value)
                {
                    this._filterText = value;
                    this.OnPropertyChanged("FilterText");
                }
            }
        }

        #endregion // FilterText 			

        #region FilterBoxNoDataAvailable
        /// <summary>
        /// Gets the string that will display in the selection box when there is no data due to a search.
        /// </summary>
        public string FilterBoxNoDataAvailable
        {
            get
            {
                return SRGrid.GetString("FilterBoxNoDataAvailable");
            }
        }
        #endregion // FilterBoxNoDataAvailable

        #region ShowAppendFiltersCheckBox
        /// <summary>
        /// Gets / sets if the checkbox that allows you to append filters should be shown or not.
        /// </summary>
        protected bool ShowAppendFiltersCheckBox { get; set; }
        #endregion // ShowAppendFiltersCheckBox

        #region AppendFilterText

        /// <summary>
        /// Identifies the <see cref="AppendFilterText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AppendFilterTextProperty = DependencyProperty.Register("AppendFilterText", typeof(string), typeof(FilterSelectionControl), new PropertyMetadata( new PropertyChangedCallback(AppendFilterTextChanged)));

        /// <summary>
        /// Gets / sets the text that will show for the append filters item.
        /// </summary>
        public string AppendFilterText
        {
            get { return (string)this.GetValue(AppendFilterTextProperty); }
            set { this.SetValue(AppendFilterTextProperty, value); }
        }

        private static void AppendFilterTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl ctrl = (FilterSelectionControl)obj;
            ctrl.OnPropertyChanged("AppendFilterText");
        }

        #endregion // AppendFilterText 

        #region AppendFilters

        /// <summary>
        /// Identifies the <see cref="AppendFilters"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AppendFiltersProperty = DependencyProperty.Register("AppendFilters", typeof(bool), typeof(FilterSelectionControl), new PropertyMetadata(false, new PropertyChangedCallback(AppendFiltersChanged)));

        /// <summary>
        /// Gets / sets when the AcceptChanges is called if the filters will be appended or if they will be added clean to the collection.  
        /// </summary>
        public bool AppendFilters
        {
            get { return (bool)this.GetValue(AppendFiltersProperty); }
            set { this.SetValue(AppendFiltersProperty, value); }
        }

        private static void AppendFiltersChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectionControl ctrl = (FilterSelectionControl)obj;
            ctrl.OnPropertyChanged("AppendFilters");
            ctrl.OnPropertyChanged("CanFilter");
        }

        #endregion // AppendFilters 

        #region CachedUniqueValues

        internal FilterItemCollection CachedUniqueValues
        {
            get
            {
                if (this._cachedUniqueValues == null)
                {
                    this._cachedUniqueValues = this.GetCollection();
                }
                return this._cachedUniqueValues;
            }
        }
        #endregion // CachedUniqueValues

        #region CachedCheckedCount
        private int CachedCheckedCount { get; set; }
        #endregion // CachedCheckedCount

        #region CachedUncheckedCount
        private int CachedUncheckedCount { get; set; }
        #endregion // CachedCheckedCount        

        #region HasFilters
        /// <summary>
        /// Gets if the FilterMenu has any checked filters.
        /// </summary>
        public bool HasFilters
        {
            get
            {
                return this._hasFilters;
            }
            protected set
            {
                if (value != this._hasFilters)
                {
                    this._hasFilters = value;
                    this.OnPropertyChanged("HasFilters");
                    this.OnPropertyChanged("HasCheckedItems");
                }
            }
        }
        #endregion //HasFilters

        #region HasCheckedItems
        /// <summary>
        /// Gets if the checkbox selection area has items checked and unchecked.
        /// </summary>
        public virtual bool HasCheckedItems
        {
            get
            {
                return this._hasCheckedItems;
            }
            protected set
            {
                if (value != this._hasCheckedItems)
                {
                    this._hasCheckedItems = value;
                    this.OnPropertyChanged("HasCheckedItems");
                }
            }
        }

        #endregion // HasCheckedItems

        #region Phase2TokenSourceQueue

        internal List<CancellationTokenSource> Phase2TokenSourceQueue
        {
            get { return _phase2TokenSourceQueue; }
        }

        #endregion // Phase2TokenSourceQueue

        #region AllowFiltering

        /// <summary>
        /// Gets or sets a value indicating whether the selected filter can be applied.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the selected filter can be applied; otherwise, <c>false</c>.
        /// </value>
        protected bool AllowFiltering
        {
            get
            {
                return this._allowFiltering;
            }
            set
            {
                if (value != this._allowFiltering)
                {
                    this._allowFiltering = value;
                    this.OnPropertyChanged("CanFilter");
                }
            }
        }

        #endregion // AllowFiltering

        #region CanFilter

        /// <summary>
        /// Gets value indicating whether the selected filter can be applied.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the selected filter can be applied; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanFilter
        {
            get
            {
                return this.AllowFiltering || this.AppendFilters;
            }
        }

        #endregion // CanFilter

        #endregion // Properties

        #region Overrides

        #region OnApplyTemplate
        
        /// <summary>
        /// Builds the visual tree for the <see cref="FilterSelectionControl"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.SelectAllCheckBox = base.GetTemplateChild("SelectAllCheckBox") as CheckBox;

            FilterColumnSettings fcs = null;
            string selectAllText = "";
            if (Cell != null && Cell.Column != null)
            {
                fcs = this.Cell.Column.FilterColumnSettings;
                if (Cell.Column.ColumnLayout != null && Cell.Column.ColumnLayout.Grid != null)
                    selectAllText = Cell.Column.ColumnLayout.Grid.FilteringSettings.SelectAllText;
            }

            if (this.SelectAllCheckBox != null)
            {
                this.SelectAllCheckBox.Style = this.CheckBoxStyle;

                if (!string.IsNullOrEmpty(selectAllText))
                    this.SelectAllCheckBox.Content = selectAllText;
                else
                    this.SelectAllCheckBox.Content = SRGrid.GetString("SelectAllCheckBox");
            }

            string clearFilters = fcs == null ? string.Empty : fcs.FilterMenuClearFiltersStringResolved;

            if (string.IsNullOrEmpty(clearFilters) && this.Cell != null)
            {
                string headerText = !string.IsNullOrEmpty(this.Cell.Column.HeaderText)
                                        ? this.Cell.Column.HeaderText
                                        : this.Cell.Column.Key;

                if (string.IsNullOrEmpty(this.ClearFiltersText))
                    this.ClearFiltersTextResolved = string.Format(SRGrid.GetString("ClearFiltersForColumnKey"), headerText);
                else
                    this.ClearFiltersTextResolved = string.Format(this.ClearFiltersText, headerText);
            }
            else
            {
                this.ClearFiltersTextResolved = clearFilters;
            }

            string s = fcs == null ? string.Empty : fcs.FilterMenuTypeSpecificFiltersString;

            if (string.IsNullOrEmpty(s) && this.Cell != null)
            {
                Type t = GetUnderlyingNonNullableType(this.Cell.Column.DataType);

                if (string.IsNullOrEmpty(this.TypeSpecificFiltersText))
                {
                    if (t != null)
                    {
                        this.TypeSpecificFiltersTextResolved = string.Format(SRGrid.GetString("CustomFiltersWithType"), t.Name);
                    }
                    else
                    {
                        this.TypeSpecificFiltersTextResolved = SRGrid.GetString("CustomFiltersWithoutType");
                    }
                }
                else
                {
                    this.TypeSpecificFiltersTextResolved = this.TypeSpecificFiltersText;
                }
            }
            else
                this.TypeSpecificFiltersTextResolved = s;

            this.EnsureCurrentState();


            


            this.Dispatcher.BeginInvoke(new Action(this.InvalidateArrange));

        }

        #endregion // OnApplyTemplate

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected override bool SupportsCommand(ICommand command)
        {
            return command is ClearSelectedItemsCommand ||
                    command is ShowCustomFilterDialogCommand ||
                    base.SupportsCommand(command);
        }

        #endregion // SupportsCommand

        #region AcceptChanges

        /// <summary>
        /// Processes the elements in the selected filters.
        /// </summary>
        protected internal override void AcceptChanges()
        {
            if (!this._changeDetected)
            {
                return;
            }

            CellBase cell = this.Cell;
            string columnKey = cell.Column.Key;
            RowsManager rm = cell.Row.Manager as RowsManager;

            if (rm == null)
            {
                return;
            }

            RowFiltersCollection rowFiltersCollectionResolved = rm.RowFiltersCollectionResolved;
            RowsFilter rf = rowFiltersCollectionResolved[columnKey];

            if (rf == null)
            {
                return;
            }

            Column col = this.Cell.Column;
            ColumnLayout colLayout = col.ColumnLayout;
            XamGrid grid = col.ColumnLayout.Grid;

            bool shouldClearFilters = !this.AppendFilters;

            if (colLayout.Grid.FilteringSettings.FilterMenuSelectionListGeneration == FilterMenuCumulativeSelectionList.ExcelStyle)
            {
                foreach (RowsFilter rfTemp in rowFiltersCollectionResolved)
                {
                    if (rf != rfTemp)
                    {
                        if (rfTemp.Conditions.Count > 0)
                        {
                            shouldClearFilters = false;
                            break;
                        }
                    }
                }
            }

            if (shouldClearFilters)
            {
                rf.Conditions.LogicalOperator = LogicalOperator.Or;

                rf.Conditions.ClearSilently();
            }

            if (this.AppendFilters)
            {                    
                rf.Conditions.LogicalOperator = LogicalOperator.Or;

                rf.Conditions.ClearSilently();

                



                foreach (FilterValueProxy fvp in this.FilterableUniqueValues)
                {
                    foreach (FilterValueProxy cachedValue in this.CachedUniqueValues)
                    {
                        bool valueEquals = false;

                        if (fvp.Value == null)
                        {
                            valueEquals = fvp.Value == cachedValue.Value;
                        }
                        else
                        {
                            valueEquals = fvp.Value.Equals(cachedValue.Value);
                        }
                        if (valueEquals)
                        {
                            if (fvp.IsChecked != cachedValue.IsChecked)
                            {
                                if (cachedValue.IsChecked)
                                    this.CachedCheckedCount--;
                                else
                                    this.CachedUncheckedCount--;

                                cachedValue.IsChecked = fvp.IsChecked;

                                if (cachedValue.IsChecked)
                                    this.CachedCheckedCount++;
                                else
                                    this.CachedUncheckedCount++;
                            }
                            break;
                        }
                    }
                }

                if (this.CachedCheckedCount > this.CachedUncheckedCount || this.CachedCheckedCount == 0 )
                {
                    foreach (FilterValueProxy uniqueVal in this.CachedUniqueValues)
                    {
                        FilterSelectionControl.ApplyFilterForItem(uniqueVal.Value, uniqueVal.IsChecked, col, colLayout, rm, rf, true);
                    }
                }
                else
                {
                    foreach (FilterValueProxy uniqueVal in this.CachedUniqueValues)
                    {
                        this.ApplyNegativeFilterForItem(uniqueVal.Value, uniqueVal.IsChecked, col, colLayout, rm, rf, true);
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(this.FilterText) || this.FilterableUniqueValues == null)
                    this.ProcessChanges(this.CheckedCount, this.UncheckedCount, ((FilterItemCollection<FilterValueProxy>)this.UniqueValues).ToList());
                else
                {
                    this.CachedCheckedCount = this.RawValues.Count;
                    this.CachedUncheckedCount = 0;

                    foreach (FilterValueProxy cachedValue in this.CachedUniqueValues)
                    {
                        cachedValue.IsChecked = false;
                        this.CachedUncheckedCount++;
                        this.CachedCheckedCount--;
                        if (this.FilterableUniqueValues != null)
                        {
                            foreach (FilterValueProxy fvp in this.FilterableUniqueValues)
                            {
                                bool valueEquals = false;
                                if (fvp.Value == null)
                                {
                                    valueEquals = fvp.Value == cachedValue.Value;
                                }
                                else
                                {
                                    valueEquals = fvp.Value.Equals(cachedValue.Value);
                                }

                                if (valueEquals)
                                {
                                    if (fvp.IsChecked != cachedValue.IsChecked)
                                    {
                                        if (cachedValue.IsChecked)
                                            this.CachedCheckedCount--;
                                        else
                                            this.CachedUncheckedCount--;

                                        cachedValue.IsChecked = fvp.IsChecked;

                                        if (cachedValue.IsChecked)
                                            this.CachedCheckedCount++;
                                        else
                                            this.CachedUncheckedCount++;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    this.ProcessChanges(this.CachedCheckedCount, this.CachedUncheckedCount, ((FilterItemCollection<FilterValueProxy>)this.CachedUniqueValues).ToList());
                }
            }

            cell.Row.ColumnLayout.InvalidateFiltering();
            grid.ResetPanelRows();
            grid.OnFiltered(rm.RowFiltersCollectionResolved);
        }

        #endregion // AcceptChanges

        #region OnCellAssigned
        
        /// <summary>
        /// Raised when a Cell is assigned to the control.
        /// </summary>
        protected override void OnCellAssigned()
        {
            if (this.Cell != null)
            {
                Style style = this.Cell.Column.ColumnLayout.FilteringSettings.FilterSelectionControlStyleResolved;

                ColumnContentProviderBase.SetControlStyle(this, style);
            }
            else
            {
                this._formatString = string.Empty;
                this.ClearValue(FilterSelectionControl.StyleProperty);
            }
        }

        #endregion // OnCellAssigned

        #region OnMouseLeftButtonDown

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = true;
        }


        #endregion // OnMouseLeftButtonDown

        #endregion // Overrides

        #region Methods

        #region Protected

        #region OnRawValuesSet
        
        /// <summary>
        /// Method called after raw data is pulled from the data manager so that any manipulation that needs to take place can.
        /// </summary>
        protected virtual void OnRawValuesSet()
        {
        }

        #endregion // OnRawValuesSet

        #region EnsureState
        /// <summary>
        /// Ensures that <see cref="FilterSelectionControl"/> is in the correct state.
        /// </summary>
        protected internal virtual void EnsureCurrentState()
        {
            if (this.Cell != null && this.Cell.Column is UnboundColumn)
            {
                VisualStateManager.GoToState(this, "HideSelectionBox", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "ShowSelectionBox", false);
            }

            if (!this._isInDesignMode && this.IsLoaded && !(this.DelayIsBusy || this.IsBusy) && (this.UniqueValues == null || this.UniqueValues.Count == 0))
            {
                VisualStateManager.GoToState(this, "ShowSelectionBoxWaterMark", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "HideSelectionBoxWaterMark", false);
            }

            if (this.ShowAppendFiltersCheckBox)
            {
                VisualStateManager.GoToState(this, "ShowAppendFilterButton", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "HideAppendFilterButton", false);
            }

            if (this.IsBusy)
            {
                VisualStateManager.GoToState(this, "Busy", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Idle", false);
            }

            if (this.IsLimited)
            {
                VisualStateManager.GoToState(this, "Limited", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "NotLimited", false);
            }
        }
        #endregion // EnsureState

        #region SetupSelectionBox

        /// <summary>
        /// Used to set up the the controls which will go into the <see cref="Panel"/>.
        /// </summary>
        protected override void SetupSelectionBox()
        {
            CommandSourceManager.NotifyCanExecuteChanged(typeof(ClearSelectedItemsCommand));

            if (this.SelectAllCheckBox != null && this.Cell != null && this.Cell.Column != null && this.Cell.Column.IsFilterable && !(this.Cell.Column is UnboundColumn))
            {
                this.CheckedCount = this.UncheckedCount = 0;

                RowsManager rm = this.Cell.Row.Manager as RowsManager;

                if (rm == null)
                {
                    return;
                }

                if (rm.DataManager == null || rm.DataManager.CachedType == null)
                {
                    this.SelectAllCheckBox.IsEnabled = false;
                    this.ItemSourceSet = false;
                    return;
                }
                else
                {
                    this.ItemSourceSet = true;
                    this.SelectAllCheckBox.IsEnabled = true;
                }

                RowFiltersCollection rowFiltersCollectionResolved = rm.RowFiltersCollectionResolved;
                RowsFilter rf = rowFiltersCollectionResolved[this.Cell.Column.Key];
                if (rf == null)
                {
                    rf = new RowsFilter(rm.DataManager.CachedType, this.Cell.Column);
                    rm.RowFiltersCollectionResolved.Add(rf);
                }

                bool shouldRespectOtherFilters = false;
                if (rm.ColumnLayout.Grid.FilteringSettings.FilterMenuSelectionListGeneration == FilterMenuCumulativeSelectionList.ExcelStyle)
                {
                    foreach (RowsFilter rfTemp in rowFiltersCollectionResolved)
                    {
                        if (rf != rfTemp)
                        {
                            if (rfTemp.Conditions.Count > 0)
                            {
                                shouldRespectOtherFilters = true;
                                break;
                            }
                        }
                    }
                }

                if (!this.IsLoadingData)
                {
                    this.IsLoadingData = true;
                    _phase1TokenSource = new CancellationTokenSource();
                    CancellationToken token = _phase1TokenSource.Token;

                    var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
                    Task<IList> phase1Task = rm.GetDistinctItemsForColumn(this.Cell, shouldRespectOtherFilters, token);

                    if (this._busyIndicatorStartTimer.IsEnabled)
                    {
                        this._busyIndicatorStartTimer.Tick -= new EventHandler(BusyIndicatorStartTimer_Tick);
                        this._busyIndicatorStartTimer.Stop();
                    }

                    
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                    this.DelayIsBusy = true;
                    this.EnsureCurrentState();

                    this._busyIndicatorStartTimer.Tick += new EventHandler(BusyIndicatorStartTimer_Tick);
                    this._busyIndicatorStartTimer.Start();


                    phase1Task.ContinueWith(
                        result =>
                        {
                            if (_phase1TokenSource == null || _phase1TokenSource.IsCancellationRequested)
                            {
                                this.IsBusy = false;
                                this.IsLoadingData = false;
                                this.EnsureCurrentState();
                                _phase1TokenSource = null;
                                return;
                            }

                            if (result.Exception != null || result.Status == TaskStatus.Faulted || result.Result == null)
                            {
                                this.IsBusy = false;
                                this.IsLoadingData = false;
                                this.EnsureCurrentState();
                                _phase1TokenSource = null;
                                return;
                            }

                            this.RawValues = result.Result;
                            this.OnRawValuesSet();

                            int checkedCounter = 0;
                            int count = this.RawValues.Count;
                            int conditionCount = rf.Conditions.Count;

                            if (conditionCount > 1 && rf.Conditions.LogicalOperator == LogicalOperator.Or)
                            {
                                


                                bool areAllConditionsEqual = true;

                                List<object> values = new List<object>();

                                foreach (IFilterCondition ifc in rf.Conditions)
                                {
                                    ComparisonCondition cc = ifc as ComparisonCondition;
                                    if (cc != null)
                                    {
                                        if (cc.Operator != ComparisonOperator.Equals)
                                        {
                                            areAllConditionsEqual = false;
                                            values.Clear();
                                            break;
                                        }
                                        values.Add(cc.FilterValue);
                                    }
                                    else
                                    {
                                        areAllConditionsEqual = false;
                                        values.Clear();
                                        break;
                                    }
                                }

                                if (areAllConditionsEqual)
                                {
                                    checkedCounter = CreateEqualsBasedCheckboxTree(this.RawValues, values);
                                    this.HasCheckedItems = true;
                                    this.HasFilters = false;
                                }
                                else
                                {
                                    this.CreateAllCheckBoxes(this.RawValues, false);
                                    this.HasCheckedItems = false;
                                }
                            }
                            else if (conditionCount > 1 && rf.Conditions.LogicalOperator == LogicalOperator.And)
                            {
                                
                                bool areAllConditionsNotEqual = true;

                                List<object> values = new List<object>();

                                ConditionCollection conditions = rf.Conditions;
                                int internalCount = conditions.Count;
                                for (int i = 0; i < internalCount; i++)
                                {
                                    ComparisonCondition cc = conditions[i] as ComparisonCondition;

                                    if (cc == null)
                                    {
                                        areAllConditionsNotEqual = false;
                                        values.Clear();
                                        break;
                                    }

                                    if (cc.Operator != ComparisonOperator.NotEquals)
                                    {
                                        areAllConditionsNotEqual = false;
                                        values.Clear();
                                        break;
                                    }
                                    values.Add(cc.FilterValue);
                                }

                                if (areAllConditionsNotEqual)
                                {
                                    checkedCounter = CreateNotEqualsBasedCheckboxTree(this.RawValues, values);
                                    this.HasCheckedItems = true;
                                    this.HasFilters = false;
                                }
                                else
                                {
                                    this.CreateAllCheckBoxes(this.RawValues, false);
                                    this.HasCheckedItems = false;
                                }
                            }
                            else if (conditionCount == 1)
                            {
                                ComparisonCondition cc = rf.Conditions[0] as ComparisonCondition;

                                if (cc != null)
                                {
                                    bool isEquals = cc.Operator == ComparisonOperator.Equals;
                                    bool isNotEquals = cc.Operator == ComparisonOperator.NotEquals;

                                    List<object> values = new List<object>();
                                    values.Add(cc.FilterValue);
                                    if (isEquals)
                                    {
                                        checkedCounter = this.CreateEqualsBasedCheckboxTree(this.RawValues, values);
                                        this.HasCheckedItems = true;
                                        this.HasFilters = false;
                                    }
                                    else if (isNotEquals)
                                    {
                                        checkedCounter = this.CreateNotEqualsBasedCheckboxTree(this.RawValues, values);
                                        this.HasCheckedItems = true;
                                        this.HasFilters = false;
                                    }
                                    else
                                    {
                                        this.CreateAllCheckBoxes(this.RawValues, false);
                                        this.HasCheckedItems = false;
                                    }
                                }
                                else
                                {
                                    this.CreateAllCheckBoxes(this.RawValues, false);
                                    this.HasCheckedItems = false;
                                }
                            }
                            else
                            {
                                bool isChecked = conditionCount == 0;
                                if (isChecked)
                                {
                                    checkedCounter = count;
                                }
                                CreateAllCheckBoxes(this.RawValues, isChecked);
                                this.HasCheckedItems = false;
                            }

                            bool? isCheckedValue;

                            if (checkedCounter == 0)
                            {
                                isCheckedValue = false;
                            }
                            else if (checkedCounter == count)
                            {
                                isCheckedValue = true;
                            }
                            else
                            {
                                isCheckedValue = null;
                            }

                            this.SetSelectAllCheckBox(isCheckedValue);

                            this.IsBusy = false;
                            this.IsLoadingData = false;
                            this.EnsureCurrentState();
                        },
                        uiContext);
                }
            }
        }

        #endregion // SetupSelectionBox

        #region OnSelectAllCheckBoxChecked

        /// <summary>
        /// Called when the <see cref="SelectAllCheckBox"/> is checked.
        /// </summary>
        /// <param name="checkBoxSender"></param>
        protected virtual void OnSelectAllCheckBoxChecked(CheckBox checkBoxSender)
        {
            bool isChecked = (checkBoxSender.IsChecked == true);

            foreach (FilterValueProxy uniqueVal in this.UniqueValues)
            {
                uniqueVal.PropertyChanged -= UniqueValue_PropertyChanged;
                uniqueVal.IsChecked = isChecked;
                uniqueVal.PropertyChanged += UniqueValue_PropertyChanged;
            }

            if (isChecked)
            {
                this.CheckedCount = this.UniqueValues.Count;
                this.UncheckedCount = 0;
            }
            else
            {
                this.UncheckedCount = this.UniqueValues.Count;
                this.CheckedCount = 0;
            }

            this.AllowFiltering = checkBoxSender.IsChecked != false;
        }

        #endregion // OnSelectAllCheckBoxChecked

        #region GetCollection

        /// <summary>
        /// Creates the <see cref="FilterItemCollection"/> which will be used to store the unique values.
        /// </summary>
        /// <returns></returns>
        protected virtual FilterItemCollection GetCollection()
        {
            return new FilterValueProxyCollection();
        }

        #endregion // GetCollection

        #region ClearExistingFilters

        /// <summary>
        /// Resets the existing filter
        /// </summary>
        protected internal virtual void ClearExistingFilters()
        {
            this.AllowFiltering = true;

            if (this.SelectAllCheckBox != null)
            {
                this.SelectAllCheckBox.IsChecked = true;
            }

            CellBase cell = this.Cell;
            string columnKey = cell.Column.Key;
            RowsManager rm = cell.Row.Manager as RowsManager;

            if (rm == null)
            {
                return;
            }

            RowFiltersCollection rowFiltersCollectionResolved = rm.RowFiltersCollectionResolved;
            RowsFilter rf = rowFiltersCollectionResolved[columnKey];

            if (rf == null)
            {
                return;
            }

            rf.Conditions.Clear();
        }

        #endregion // ClearExistingFilters

        #region ControlLoaded
        /// <summary>
        /// Method called when the control is loaded.
        /// </summary>
        protected virtual void ControlLoaded()
        {
            if (this.Cell != null)
            {
                FilterColumnSettings fcs = this.Cell.Column.FilterColumnSettings;
                fcs.ValidateResolvedProperties();

                if (this.Cell.Column.ColumnLayout.Grid != null)
                {
                    FilteringSettings fs = this.Cell.Column.ColumnLayout.Grid.FilteringSettings;
                    this._nullString = fs.NullValueString;
                    this._emptyString = fs.EmptyValueString;

                    this._formatString = fcs.FilterMenuFormatStringResolved;

                    if (this.SelectAllCheckBox != null)
                        this.SelectAllCheckBox.Content = fs.SelectAllText;
                }
            }

            this.CachedUniqueValues.Clear();
            this.ShowAppendFiltersCheckBox = false;
            this.IsBusy = false;
            this.IsLimited = false;



            this.FilterText = string.Empty;
            this.Dispatcher.BeginInvoke(new Action(() => { this.OnPropertyChanged("FilterText"); }));

            this.SetupSelectionBox();
            this.EnsureCurrentState();
            this.EnsureMenuCheckedState();
            this._changeDetected = false;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        }
        #endregion // ControlLoaded

        #region EnsureMenuCheckedState
        
        /// <summary>
        /// This method is called to ensure that the menu options of the <see cref="FilterSelectionControl"/> are correctly checked.
        /// </summary>
        protected virtual void EnsureMenuCheckedState()
        {
            this.HasFilters = this.EnsureMenuCheckedStateHelper(this.Cell.Column.FilterColumnSettings.FilterMenuOperands);
        }

        #endregion // EnsureMenuCheckedState

        #region SetSelectAllCheckBox
        /// <summary>
        /// Sets the select all checkbox value to the given value without doing the action of selecting all the other checkboxes.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SetSelectAllCheckBox(bool? value)
        {
            this.DetachSelectAllCheckBoxEvents();

            if (this.SelectAllCheckBox != null)
            {
                this.SelectAllCheckBox.IsChecked = value;
            }

            this.AttachSelectAllCheckBoxEvents();
            this._changeDetected = true;
            this.AllowFiltering = value != false;
        }
        #endregion // SetSelectAllCheckBox

        #region GenerateUniqueList

        /// <summary>
        /// Given a list of objects builds the UniqueValue list of the control.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="values"></param>
        /// <param name="shouldCheck"></param>
        /// <returns></returns>
        protected virtual int GenerateUniqueList(IList items, List<object> values, bool shouldCheck)
        {
            this.UniqueValues.Clear();

            int checkedCounter = 0;

            Type type = this.Cell.Column.DataType;

            
            this.IsLimited = items.Count > Limit;

            for (int i = 0; i < items.Count; i++)
            {
                object o = items[i];
                


                bool bFound = false;
                foreach (object obj in values)
                {
                    if (o == obj)
                    {
                        bFound = true;
                    }
                    else if (o != null)
                    {
                        if (type == typeof(string))
                        {
                            if (Cell.Column.FilterColumnSettings.FilterCaseSensitive)
                                bFound = ((string)o).Equals(obj as string);
                            else
                                bFound = ((string)o).Equals(obj as string, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            bFound = o.Equals(obj);
                        }
                    }

                    if (!bFound && type == typeof(string))
                    {
                        if (string.IsNullOrEmpty((string)o) && string.IsNullOrEmpty((string)obj))
                        {
                            bFound = true;
                        }
                    }

                    if (bFound)
                    {
                        break;
                    }
                }

                bool updateCheckedCounter = (shouldCheck) ? bFound : !bFound;
                if (updateCheckedCounter)
                {
                    checkedCounter++;
                }

                bool isChecked = (shouldCheck) ? bFound : !bFound;

                this.AddUniqueValue(o, isChecked, type, (i + 1) > Limit);
            }

            this.UniqueValues.RaiseCollectionChanged();
            this.OnPropertyChanged("FilterableUniqueValues");

            return checkedCounter;
        }

        #endregion // GenerateUniqueList

        #region CreateAllCheckBoxes

        /// <summary>
        /// Creates the list that will be used to populate the selection control.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="shouldCheck"></param>
        protected virtual void CreateAllCheckBoxes(IList items, bool shouldCheck)
        {
            this.UniqueValuesClear();

            Type type = this.Cell.Column.DataType;

            _isMassEdit = true;

            this.IsLimited = items.Count > Limit;

            for (int i = 0; i < items.Count; i++)
            {
                object o = items[i];
                this.AddUniqueValue(o, shouldCheck, type, (i + 1) > Limit);
            }

            _isMassEdit = false;

            this.UniqueValues.RaiseCollectionChanged();
            this.OnPropertyChanged("FilterableUniqueValues");
        }

        #endregion // CreateAllCheckBoxes

        #region AttachSelectAllCheckBoxEvents

        /// <summary>
        /// Attaches the events to the <see cref="SelectAllCheckBox"/>.
        /// </summary>
        protected void AttachSelectAllCheckBoxEvents()
        {
            if (this.SelectAllCheckBox != null)
            {
                this.SelectAllCheckBox.Checked += SelectAllCheckBox_Checked;
                this.SelectAllCheckBox.Unchecked += SelectAllCheckBox_Checked;
                this.SelectAllCheckBox.Indeterminate += SelectAllCheckBox_Checked;
            }
        }

        #endregion // AttachSelectAllCheckBoxEvents

        #region DetachSelectAllCheckBoxEvents

        /// <summary>
        /// Detaches the events to the <see cref="SelectAllCheckBox"/>.
        /// </summary>
        protected void DetachSelectAllCheckBoxEvents()
        {
            if (this.SelectAllCheckBox != null)
            {
                this.SelectAllCheckBox.Checked -= SelectAllCheckBox_Checked;
                this.SelectAllCheckBox.Unchecked -= SelectAllCheckBox_Checked;
                this.SelectAllCheckBox.Indeterminate -= SelectAllCheckBox_Checked;
            }
        }

        #endregion // DetachSelectAllCheckBoxEvents

        #region AddUniqueValue

        /// <summary>
        /// Creates an object for UniqueList and adds it to the list.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isChecked"></param>
        /// <param name="type"></param>
        protected virtual void AddUniqueValue(object value, bool isChecked, Type type)
        {
            this.AddUniqueValue(value, isChecked, type, false);
        }

        protected virtual void AddUniqueValue(object value, bool isChecked, Type type, bool limitUniqueValues)
        {
            if(!limitUniqueValues)
            {
                FilterValueProxy fvp = new FilterValueProxy(value, isChecked)
                {
                    FormatString = this._formatString,
                    DataType = type,
                    EmptyString = this._emptyString,
                    NullString = this._nullString
                };

                if (isChecked)
                    this.CheckedCount++;
                else
                    this.UncheckedCount++;

                fvp.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(UniqueValue_PropertyChanged);

                this.UniqueValues.AddSilently(fvp);
            }

            if (_boolBuildCachedList)
            {
                FilterValueProxy cachedFvp = new FilterValueProxy(value, isChecked)
                {
                    FormatString = this._formatString,
                    DataType = type,
                    EmptyString = this._emptyString,
                    NullString = this._nullString
                };

                this.CachedUniqueValues.AddSilently(cachedFvp);
                if (isChecked)
                    this.CachedCheckedCount++;
                else
                    this.CachedUncheckedCount++;
            }
        }

        #endregion // AddUniqueValue

        #region InvalidateShowAppendFilterButton

        /// <summary>
        /// Determines if the <see cref="ShowAppendFiltersCheckBox"/> should be set.
        /// </summary>
        /// <param name="filterValue"></param>
        protected virtual void InvalidateShowAppendFilterButton(string filterValue)
        {
            if (string.IsNullOrEmpty(filterValue))
            {
                this.ShowAppendFiltersCheckBox = false;
                this.AppendFilters = false;
            }
            
            else if (this.UniqueValues != null && this.UniqueValues.Count == 0)
            {
                this.ShowAppendFiltersCheckBox = false;
                this.AppendFilters = false;
            }
            else if (this.FilterableUniqueValues == null)
            {
                this.ShowAppendFiltersCheckBox = false;
                this.AppendFilters = false;
            }
            else
            {
                this.ShowAppendFiltersCheckBox = true;
            }
            this.EnsureCurrentState();
        }

        #endregion // InvalidateShowAppendFilterButton

        #endregion // Protected

        #region Internal

        #region GenerateContentString

        internal string GenerateContentString(object o)
        {
            string formatString = this._formatString;

            if (o == null)
            {
                return this._nullString;
            }
            else if (this.Cell.Column.DataType == typeof(String))
            {
                string s = o as string;
                if (string.IsNullOrEmpty(s))
                    return this._emptyString;
            }
            else if (this.Cell.Column.DataType == typeof(String))
            {
                string s = o as string;
                if (string.IsNullOrEmpty(s))
                    return SRGrid.GetString("EmptyContentString");
            }

            return string.IsNullOrEmpty(formatString) ? o.ToString() : string.Format(formatString, o);
        }

        #endregion // GenerateContentString

        #region FilterUniqueValue
        
        /// <summary>
        /// This method is exposed to allow for doing a filtering based on an inputted string.
        /// </summary>
        /// <param name="filterValue"></param>
        internal protected virtual void FilterUniqueValue(string filterValue)
        {
            this.FilterText = filterValue;

            var copy = new List<CancellationTokenSource>(this.Phase2TokenSourceQueue);
            foreach (var cancellationTokenSource in copy)
            {
                cancellationTokenSource.Cancel();
                Phase2TokenSourceQueue.Remove(cancellationTokenSource);
            }

            if (this._waitSearchStringTimer.IsEnabled)
            {
                this._waitSearchStringTimer.Stop();
            }

            this._waitSearchStringTimer.Start();
        }

        internal virtual void FilterUniqueValueInternal(string filterValue)
        {
            if (this.CachedUniqueValues != null && this.CachedUniqueValues.Count > 0)
            {
                if (string.IsNullOrEmpty(filterValue))
                {
                    // this._filterableFlatUniqueValuesDataManager.Filters[0].Conditions.Clear();
                    this._boolBuildCachedList = false;
                    this.RefreshUniqueValues(this.CachedUniqueValues);
                    this._boolBuildCachedList = true;

                    var copy = new List<CancellationTokenSource>(this.Phase2TokenSourceQueue);

                    foreach (var cancellationTokenSource in copy)
                    {
                        cancellationTokenSource.Cancel();
                        Phase2TokenSourceQueue.Remove(cancellationTokenSource);
                    }

                    this.IsBusy = false;

                    this.InvalidateShowAppendFilterButton(filterValue);
                    this.EnsureCurrentState();
                }
                else
                {
                    TaskScheduler uiContext = TaskScheduler.FromCurrentSynchronizationContext();
                    this.IsBusy = true;
                    this.EnsureCurrentState();

                    CancellationTokenSource source = new CancellationTokenSource();
                    Phase2TokenSourceQueue.Add(source);

                    Task<Phase2Payload>.Factory.StartNew(
                        () =>
                            {
                                var manager = DataManagerBase.CreateDataManager(this.CachedUniqueValues);
                                manager.Filters = new RecordFilterCollection();
                                manager.Filters.Add(new FilterValueProxyRowsFilter(typeof(FilterValueProxy), manager.CachedTypedInfo));

                                manager.Filters[0].Conditions.Clear();
                                manager.Filters[0].Conditions.Add(new FilterValueProxyStringFilter { FilterValue = filterValue });
                                manager.UpdateData();

                                return new Phase2Payload { CancellationTokenSource = source, FilteredList = manager.SortedFilteredDataSource };
                            },
                            source.Token)
                            .ContinueWith(result =>
                                              {
                                                  if (result.IsCanceled || result.Exception != null || result.Result.CancellationTokenSource.IsCancellationRequested)
                                                  {
                                                      return;
                                                  }

                                                  this._boolBuildCachedList = false;
                                                  this.RefreshUniqueValues(result.Result.FilteredList);

                                                  if (!string.IsNullOrEmpty(filterValue))
                                                  {
                                                      this.SetSelectAllCheckBox(true);
                                                  }

                                                  this.InvalidateShowAppendFilterButton(filterValue);
                                                  this._boolBuildCachedList = true;


                                                  Phase2TokenSourceQueue.Remove(result.Result.CancellationTokenSource);
                                                  if(Phase2TokenSourceQueue.Count == 0)
                                                  {
                                                      this.IsBusy = false;
                                                  }
                                                  this.EnsureCurrentState();
                                              }, 
                                              uiContext);
                }
            }
        }

        #endregion // FilterUniqueValue

        #region UniqueValuesClear

        internal virtual void UniqueValuesClear()
        {
            if (this.UniqueValues != null)
            {
                IEnumerable list = this.UniqueValues.Cast<FilterValueProxy>();

                UniqueValuesClear(list);

                this.UniqueValues.Clear();
            }
        }

        internal virtual void UniqueValuesClear(IEnumerable list)
        {
            foreach (FilterValueProxy obj in list)
            {
                obj.PropertyChanged -= UniqueValue_PropertyChanged;
            }
        }

        #endregion // UniqueValuesClear

        #endregion // Internal

        #region Private

        #region RefreshUniqueValues

        private void RefreshUniqueValues(IEnumerable values)
        {
            this.UniqueValuesClear();
            this.IsLimited = false;

            Type type = this.Cell.Column.DataType;

            if (string.IsNullOrEmpty(this.FilterText))
            {
                int count = 0;
                foreach (FilterValueProxy val in values)
                {
                    if (++count > Limit)
                    {
                        // Break. We are not building cache when RefreshUniqueValues is called. 
                        this.IsLimited = true;
                        break;
                    }

                    this.AddUniqueValue(val.Value, val.IsChecked, type);
                }
            }
            else
            {
                int count = 0;
                foreach (FilterValueProxy val in values)
                {
                    if (++count > Limit)
                    {
                        // Break. We are not building cache when RefreshUniqueValues is called. 
                        this.IsLimited = true;
                        break;
                    }

                    this.AddUniqueValue(val.Value, true, type);
                }
            }

            this.UniqueValues.RaiseCollectionChanged();
            this.OnPropertyChanged("FilterableUniqueValues");
        }

        #endregion // RefreshUniqueValues

        #region CreateEqualsBasedCheckboxTree

        private int CreateEqualsBasedCheckboxTree(IList items, List<object> values)
        {
            return this.GenerateUniqueList(items, values, true);
        }

        #endregion // CreateEqualsBasedCheckboxTree

        #region CreateNotEqualsBasedCheckboxTree

        private int CreateNotEqualsBasedCheckboxTree(IList items, List<object> values)
        {
            return this.GenerateUniqueList(items, values, false);
        }

        #endregion // CreateNotEqualsBasedCheckboxTree

        #region ApplyFilterForItem

        private static void ApplyFilterForItem(object value, bool isChecked, Column col, ColumnLayout colLayout, RowsManager rm, RowsFilter rf, bool raiseEvents)
        {
            if (!isChecked)
            {
                bool found = false;

                foreach (ComparisonCondition cc in rf.Conditions)
                {
                    if (value == cc.FilterValue)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (
                        value == null ||
                        (col.DataType == typeof(string) && string.IsNullOrEmpty((string)value))
                        )
                        colLayout.BuildNullableFilter(rm.RowFiltersCollectionResolved, col, new NotEqualsOperand(), true, false, false);
                    else
                        colLayout.BuildFilters(rm.RowFiltersCollectionResolved, value, col, new NotEqualsOperand(), true, false, raiseEvents);
                    rf.Conditions.LogicalOperator = LogicalOperator.And;
                }
            }
        }

        #endregion // ApplyFilterForItem

        #region ApplyNegativeFilterForItem

        private void ApplyNegativeFilterForItem(object value, bool isChecked, Column col, ColumnLayout colLayout, RowsManager rm, RowsFilter rf, bool raiseEvents)
        {
            if (isChecked)
            {
                bool found = false;

                foreach (ComparisonCondition cc in rf.Conditions)
                {
                    if (value == cc.FilterValue)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (value == null)
                        colLayout.BuildNullableFilter(rm.RowFiltersCollectionResolved, col, new EqualsOperand(), true, false, raiseEvents);
                    else
                        colLayout.BuildFilters(rm.RowFiltersCollectionResolved, value, col, new EqualsOperand(), true, false, raiseEvents);
                    rf.Conditions.LogicalOperator = LogicalOperator.Or;
                }
            }
        }

        #endregion // ApplyNegativeFilterForItem

        #region ClearAllMenuChecks

        private void ClearAllMenuChecks(List<FilterMenuTrackingObject> list)
        {
            if (list == null)
                return;

            if (list.Count == 0)
                return;

            foreach (FilterMenuTrackingObject obj in list)
            {
                
                if (obj.IsSeparator)
                    continue;

                obj.IsChecked = false;

                ClearAllMenuChecks(obj.Children);
            }
        }

        #endregion // ClearAllMenuChecks

        #region EnsureMenuCheckedStateHelper

        private bool EnsureMenuCheckedStateHelper(List<FilterMenuTrackingObject> list)
        {
            if (list == null)
            {
                return true;
            }

            if (list.Count == 0)
            {
                return false;
            }

            CellBase cell = this.Cell;
            string columnKey = cell.Column.Key;
            RowsManager rm = cell.Row.Manager as RowsManager;

            if (rm == null)
            {
                return false;
            }

            RowFiltersCollection rowFiltersCollectionResolved = rm.RowFiltersCollectionResolved;
            RowsFilter rf = rowFiltersCollectionResolved[columnKey];

            if (rf == null)
            {
                return false;
            }

            this.ClearAllMenuChecks(list);

            if (rf.Conditions.Count == 0)
            {
                return false;
            }

            FilterMenuTrackingObject customTrackingObject = null;
            


            foreach (FilterMenuTrackingObject obj in list)
            {
                
                if (obj.IsSeparator)
                    continue;

                if (obj.IsCustomOption)
                    customTrackingObject = obj;

                
                if (obj.FilterOperands.Count == rf.Conditions.Count)
                {
                    bool foundAll = false; // rf.Conditions.Count > 0;
                    for (int i = 0; i < rf.Conditions.Count; i++)
                    {
                        ComparisonCondition cc = rf.Conditions[i] as ComparisonCondition;

                        if (cc != null)
                        {
                            if (!foundAll)
                            {
                                foreach (FilterOperand foSub in obj.FilterOperands)
                                {
                                    if (foSub.ComparisonOperatorValue == cc.Operator)
                                    {
                                        foundAll = true;
                                        break;
                                    }
                                    foundAll = false;
                                }
                            }

                            continue;
                        }

                        CustomComparisonCondition customCond = rf.Conditions[i] as CustomComparisonCondition;

                        if (customCond != null)
                        {
                            Type customCondType = customCond.FilterOperand;
                            if (!foundAll)
                            {
                                foreach (FilterOperand foSub in obj.FilterOperands)
                                {
                                    Type foSubType = foSub.GetType();
                                    if (foSubType == customCondType)
                                    {
                                        foundAll = true;
                                        break;
                                    }
                                    foundAll = false;
                                }
                            }

                            continue;
                        }
                    }

                    if (foundAll)
                    {
                        obj.IsChecked = true;
                        return true;
                    }
                    else
                    {
                        obj.IsChecked = false;
                    }
                }

                if (EnsureMenuCheckedStateHelper(obj.Children))
                {
                    obj.IsChecked = true;
                    return true;
                }
                else if (!this._hasCheckedItems && customTrackingObject != null)
                {
                    customTrackingObject.IsChecked = true;
                    return true;
                }
                else
                {
                    obj.IsChecked = false;
                }
            }

            return false;
        }
        #endregion // EnsureMenuCheckedStateHelper

        #region ProcessChanges

        private void ProcessChanges(int checkedCount, int uncheckedCount, IList list)
        {
            CellBase cell = this.Cell;
            string columnKey = cell.Column.Key;
            RowsManager rm = cell.Row.Manager as RowsManager;

            if (rm == null)
            {
                return;
            }

            RowFiltersCollection rowFiltersCollectionResolved = rm.RowFiltersCollectionResolved;
            RowsFilter rf = rowFiltersCollectionResolved[columnKey];

            if (rf == null)
            {
                return;
            }

            Column col = this.Cell.Column;
            ColumnLayout colLayout = col.ColumnLayout;

            if (checkedCount > uncheckedCount || checkedCount == 0) 
            {
                foreach (FilterValueProxy uniqueVal in list)
                {                    
                    FilterSelectionControl.ApplyFilterForItem(uniqueVal.Value, uniqueVal.IsChecked, col, colLayout, rm, rf, true);
                }
            }
            else
            {
                foreach (FilterValueProxy uniqueVal in list)
                {                    
                    this.ApplyNegativeFilterForItem(uniqueVal.Value, uniqueVal.IsChecked, col, colLayout, rm, rf, true);
                }
            }
        }

        #endregion // ProcessChanges

        #endregion // Private

        #endregion // Methods

        #region EventHandlers

        #region WaitSearchStringTimer_Tick

        private void WaitSearchStringTimer_Tick(object sender, EventArgs e)
        {
            this._waitSearchStringTimer.Stop();
            this.FilterUniqueValueInternal(this.FilterText);
        }

        #endregion // WaitSearchStringTimer_Tick

        #region BusyIndicatorStartTimer_Tick

        private void BusyIndicatorStartTimer_Tick(object sender, EventArgs e)
        {
            this._busyIndicatorStartTimer.Tick -= new EventHandler(BusyIndicatorStartTimer_Tick);
            this._busyIndicatorStartTimer.Stop();

            if (this.DelayIsBusy)
            {
                this.DelayIsBusy = false;
                this.IsBusy = true;
                this.EnsureCurrentState();
            }
        }

        #endregion // BusyIndicatorStartTimer_Tick

        #region FilterSelectionControl_Loaded

        private void FilterSelectionControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsLoaded = true;
            this.ControlLoaded();

            if (this._waitSearchStringTimer == null)
            {
                this._waitSearchStringTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 400) };
                this._waitSearchStringTimer.Tick += new EventHandler(WaitSearchStringTimer_Tick);
            }
        }

        #endregion // FilterSelectionControl_Loaded

        #region FilterSelectionControl_Unloaded

        private void FilterSelectionControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.IsLoaded = false;

            if(this._phase1TokenSource != null)
            {
                this._phase1TokenSource.Cancel();
            }

            var copy = new List<CancellationTokenSource>(this.Phase2TokenSourceQueue);

            foreach (var cancellationTokenSource in copy)
            {
                cancellationTokenSource.Cancel();
                Phase2TokenSourceQueue.Remove(cancellationTokenSource);
            }
            
            this.IsBusy = false;
            this.EnsureCurrentState();

            this.RawValues = null;
            this.UniqueValuesClear();
            this._filterText = string.Empty;

            if (this._waitSearchStringTimer != null)
            {
                this._waitSearchStringTimer.Tick -= new EventHandler(WaitSearchStringTimer_Tick);
                
                if (this._waitSearchStringTimer.IsEnabled)
                {
                    this._waitSearchStringTimer.Stop();
                }

                this._waitSearchStringTimer = null;
            }
        }

        #endregion // FilterSelectionControl_Unloaded

        #region SelectAllCheckBox_Checked

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cbSender = (CheckBox)sender;
            this.OnSelectAllCheckBoxChecked(cbSender);
            this._changeDetected = true;
        }

        #endregion // SelectAllCheckBox_Checked

        #region UniqueValue_PropertyChanged

        private void UniqueValue_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FilterValueProxy uniquevalue = (FilterValueProxy)sender;

            if (e.PropertyName == "IsChecked")
            {
                bool trueFound = uniquevalue.IsChecked;
                bool falseFound = !uniquevalue.IsChecked;

                foreach (FilterValueProxy val in this.UniqueValues)
                {
                    if (val != uniquevalue)
                    {
                        if (val.IsChecked)
                        {
                            trueFound = true;
                        }
                        else
                        {
                            falseFound = true;
                        }

                        if (trueFound && falseFound)
                            break;
                    }
                }

                if (uniquevalue.IsChecked)
                {
                    this.UncheckedCount--;
                    this.CheckedCount++;
                }
                else
                {
                    this.UncheckedCount++;
                    this.CheckedCount--;
                }

                bool? isCheckedValue;

                if (trueFound && falseFound)
                {
                    isCheckedValue = null;
                }
                else if (trueFound)
                {
                    isCheckedValue = true;
                }
                else
                {
                    isCheckedValue = false;
                }

                this.SetSelectAllCheckBox(isCheckedValue);

                this._changeDetected = true;
            }
        }
        #endregion // UniqueValue_PropertyChanged

        #region UniqueValues_CollectionChanged

        private void UniqueValues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnUniqueValuesCollectionChanged(sender, e);
        }

        protected virtual void OnUniqueValuesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!this._isMassEdit)
            {
                this.EnsureCurrentState();
            }
        }

        #endregion // UniqueValues_CollectionChanged

        #endregion // EventHandlers

        #region Classes

        internal class Phase2Payload
        {
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public IList FilteredList { get; set; }
        }

        #endregion // Classes
    }
}

namespace Infragistics.Controls.Grids
{
    #region FilterSelectionControlCommandSource
    /// <summary>
    /// The command source object for <see cref="FilterSelectionControl"/>.
    /// </summary>
    public class FilterSelectionControlCommandSource : CommandSource
    {
        #region Properties

        #region CommandType

        /// <summary>
        /// Gets / sets the <see cref="FilterSelectionControlCommand"/> which is to be executed by the command.
        /// </summary>
        public FilterSelectionControlCommand CommandType
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
                case FilterSelectionControlCommand.AcceptChanges:
                    {
                        command = new AcceptChangesCommand();
                        break;
                    }
                case FilterSelectionControlCommand.ClearSelectedItems:
                    {
                        command = new ClearSelectedItemsCommand();
                        break;
                    }
            }
            return command;
        }
        #endregion // ResolveCommand

        #endregion // Overrides
    }
    #endregion // FilterSelectionControlCommandSource

    #region FilterSelectionControlCommand

    /// <summary>
    /// An enum describing the commands which can be executed on the <see cref="FilterSelectionControlCommandSource"/>
    /// </summary>
    public enum FilterSelectionControlCommand
    {
        /// <summary>
        /// Accepts the changes from the control.
        /// </summary>
        AcceptChanges,

        /// <summary>
        /// Clears the items selected in the filterable checkbox list. 
        /// </summary>
        ClearSelectedItems
    }
    #endregion // SummarySelectionControlCommand

    #region ClearSelectedItemsCommand
    /// <summary>
    /// A command which will unselect all the selected options in FilterSelectionControl.
    /// </summary>
    public class ClearSelectedItemsCommand : CommandBase
    {
        #region Overrides

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            SelectionControl fsc = parameter as SelectionControl;
            if (fsc != null)
            {
                if (fsc.Cell != null)
                {
                    RowsManager rm = (RowsManager)fsc.Cell.Row.Manager;
                    RowsFilter rf = rm.RowFiltersCollectionResolved[fsc.Cell.Column.Key];
                    if (rf == null)
                    {
                        return false;
                    }
                    return rf.Conditions.Count > 0;
                }
            }
            return false;
        }
        #endregion // CanExecute

        #region Execute

        /// <summary>
        /// Runs the command with the given parameter.
        /// </summary>
        /// <param name="parameter">An object containing any parameters for the command.</param>
        public override void Execute(object parameter)
        {
            FilterSelectionControl fsc = parameter as FilterSelectionControl;
            
            if (fsc != null)
            {
                fsc.ClearExistingFilters();    
            }
        }

        #endregion // Execute

        #endregion // Overrides
    }
    #endregion // ClearSelectedItemsCommand
}

namespace Infragistics.Controls.Grids
{
    #region CustomFilteringDialogCommand
    /// <summary>
    /// An enum describing the commands which can be executed on the <see cref="CustomFilteringDialogCommandSource"/>
    /// </summary>
    public enum CustomFilteringDialogCommand
    {
        /// <summary>
        /// Shows the custome filter dialog.
        /// </summary>
        ShowCustomFilterDialog
    }
    #endregion // CustomFilteringDialogCommand

    #region CustomFilteringDialogCommandSource

    /// <summary>
    /// The command source object for <see cref="Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl"/>.
    /// </summary>
    public class CustomFilteringDialogCommandSource : CommandSource
    {
        #region CommandType
        /// <summary>
        /// Gets / sets the <see cref="CustomFilteringDialogCommand"/> which is to be executed by the command.
        /// </summary>
        public CustomFilteringDialogCommand CommandType
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
                case CustomFilteringDialogCommand.ShowCustomFilterDialog:
                    {
                        command = new ShowCustomFilterDialogCommand();
                        break;
                    }
            }
            return command;
        }
        #endregion // ResolveCommand

        #endregion // Overrides
    }

    #endregion // CustomFilteringDialogCommandSource

    #region AcceptCustomFilterDialogChangesCommand

    /// <summary>
    /// A command which will be used for the Accept action on the <see cref="Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl"/>.
    /// </summary>
    public class AcceptCustomFilterDialogChangesCommand : CommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
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
            ColumnFilterDialogControl control = parameter as ColumnFilterDialogControl;
            if (control != null)
            {
                control.AcceptChanges();
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }
    #endregion // AcceptCustomFilterDialogChangesCommand

    #region CloseCustomFilterDialogCommand
    /// <summary>
    /// A command that will close the <see cref="Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl"/>.
    /// </summary>
    public class CloseCustomFilterDialogCommand : CommandBase
    {
        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
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
            ColumnFilterDialogControl control = parameter as ColumnFilterDialogControl;
            if (control != null)
            {
                control.Hide();
                VisualStateManager.GoToState(control.Cell.Row.ColumnLayout.Grid, "Active", false);
                control.Cell = null;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }
    #endregion // CloseCustomFilterDialogCommand
}

namespace Infragistics.Controls.Grids.Primitives
{
    #region CustomFilterDialogControlCommand

    /// <summary>
    /// An enum describing the commands which can be executed on the <see cref="CustomFilteringDialogControlCommandSource"/>
    /// </summary>
    public enum CustomFilterDialogControlCommand
    {
        /// <summary>
        /// Accepts the changes made by the dialog.
        /// </summary>
        Accept,

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        Close
    }

    #endregion // CustomFilterDialogControlCommand

    #region CustomFilteringDialogControlCommandSource
    /// <summary>
    /// The command source object for <see cref="Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl"/>.
    /// </summary>
    public class CustomFilteringDialogControlCommandSource : CommandSource
    {
        #region Properties

        #region CommandType
        /// <summary>
        /// Gets / sets the <see cref="CustomFilterDialogControlCommand"/> which is to be executed by the command.
        /// </summary>
        public CustomFilterDialogControlCommand CommandType
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
                case CustomFilterDialogControlCommand.Accept:
                    {
                        command = new AcceptCustomFilterDialogChangesCommand();
                        break;
                    }
                case CustomFilterDialogControlCommand.Close:
                    {
                        command = new CloseCustomFilterDialogCommand();
                        break;
                    }
            }
            return command;
        }

        #endregion // ResolveCommand

        #endregion // Overrides
    }
    #endregion // CustomFilteringDialogControlCommandSource

    #region CustomFilterDialogCommand

    /// <summary>
    /// A command that is used with the <see cref="Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl"/>.
    /// </summary>
    public class CustomFilterDialogCommand : CommandBase
    {
        #region Methods

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion // CanExecute

        #endregion // Methods
    }

    #endregion // CustomFilterDialogCommand

    #region ShowCustomFilterDialogCommand

    /// <summary>
    /// A command which will show the <see cref="Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl"/>.
    /// </summary>
    public class ShowCustomFilterDialogCommand : CellBaseCommandBase
    {
        #region Methods

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
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
        /// <param name="cell"></param>
        protected override void ExecuteCommand(CellBase cell)
        {
            XamGrid grid = cell.Row.ColumnLayout.Grid;
            grid.Panel.CustomFilterDialogControl.Cell = cell;
            grid.Panel.CustomFilterDialogControl.Show();
            this.CommandSource.Handled = false;
        }
        #endregion // ExecuteCommand

        #endregion // Methods
    }
    #endregion // ShowCustomFilterDialogCommand
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