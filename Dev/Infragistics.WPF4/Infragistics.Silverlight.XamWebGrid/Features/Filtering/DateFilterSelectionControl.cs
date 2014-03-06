using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
    #region DateFilterSelectionControl
    /// <summary>
    /// The <see cref="DateFilterSelectionControl"/> is displayed when when FilterMenu filtering is used. 
    /// </summary>    
    public class DateFilterSelectionControl : FilterSelectionControl
    {
        #region Members

        private bool _addTime;
        private bool _cacheCount = true;
        private bool _boolBuildCachedList = true;
        
        private FilterItemCollection _cachedFlatUniqueValues;
        private ObservableCollection<DateFilterListDisplayObject> _dateFilterTypeList;

        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="DateFilterSelectionControl"/> class.
        /// </summary>
        static DateFilterSelectionControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateFilterSelectionControl), new FrameworkPropertyMetadata(typeof(DateFilterSelectionControl)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DateFilterSelectionControl"/> class.
        /// </summary>
        public DateFilterSelectionControl()
        {



            this.Unloaded += new RoutedEventHandler(DateFilterSelectionControl_Unloaded);
        }

        void DateFilterSelectionControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.CachedCheckedCount = this.CachedUncheckedCount = 0;
            this.CachedUniqueValues.Clear();
            this.CachedFlatUniqueValues.Clear();
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region DateFilterObjectTypeItem

        /// <summary>
        /// Identifies the <see cref="DateFilterObjectTypeItem"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DateFilterObjectTypeItemProperty = DependencyProperty.Register("DateFilterObjectTypeItem", typeof(DateFilterListDisplayObject), typeof(DateFilterSelectionControl), new PropertyMetadata(new PropertyChangedCallback(DateFilterObjectTypeItemChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DateFilterListDisplayObject"/> that will be used by the control when filtering options in the selection box.
        /// </summary>
        public DateFilterListDisplayObject DateFilterObjectTypeItem
        {
            get { return (DateFilterListDisplayObject)this.GetValue(DateFilterObjectTypeItemProperty); }
            set { this.SetValue(DateFilterObjectTypeItemProperty, value); }
        }

        private static void DateFilterObjectTypeItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DateFilterSelectionControl ctrl = (DateFilterSelectionControl)obj;
            ctrl.OnPropertyChanged("DateFilterObjectTypeItem");
            if (!string.IsNullOrEmpty(ctrl.FilterText))
                ctrl.FilterUniqueValue(ctrl.FilterText);
        }

        #endregion // DateFilterObjectTypeItem

        #region DateFilterTypeList

        /// <summary>
        /// Gets / sets the list of <see cref="DateFilterObjectType"/> which can be filtered on.
        /// </summary>
        public ObservableCollection<DateFilterListDisplayObject> DateFilterTypeList
        {
            get
            {
                if (this._dateFilterTypeList == null)
                {
                    this._dateFilterTypeList = new ObservableCollection<DateFilterListDisplayObject>();
                    this.SetupComboBox();
                }
                return this._dateFilterTypeList;
            }
        }

        #endregion // DateFilterTypeList

        #region DateFilterHierarchicalDataTemplate

        /// <summary>
        /// Identifies the <see cref="DateFilterHierarchicalDataTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DateFilterHierarchicalDataTemplateProperty = DependencyProperty.Register("DateFilterHierarchicalDataTemplate", typeof(DateFilterHierarchicalDataTemplate), typeof(DateFilterSelectionControl), new PropertyMetadata(new PropertyChangedCallback(DateFilterHierarchicalDataTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DateFilterHierarchicalDataTemplate"/> that will be assigned to the <see cref="DateFilterTreeView"/> which displays the date information.
        /// </summary>
        public DateFilterHierarchicalDataTemplate DateFilterHierarchicalDataTemplate
        {
            get { return (DateFilterHierarchicalDataTemplate)this.GetValue(DateFilterHierarchicalDataTemplateProperty); }
            set { this.SetValue(DateFilterHierarchicalDataTemplateProperty, value); }
        }

        private static void DateFilterHierarchicalDataTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DateFilterSelectionControl ctrl = (DateFilterSelectionControl)obj;
            ctrl.OnPropertyChanged("DateFilterHierarchicalDataTemplate");
        }

        #endregion // DateFilterHierarchicalDataTemplate

        #region FilterableUniqueValues
        /// <summary>
        /// Gets a list of unique values which are filterable.
        /// </summary>
        public override IEnumerable FilterableUniqueValues
        {
            get
            {
                if (this.UniqueValues == null)
                    return null;

                return this.UniqueValues;
            }
        }

        #endregion // FilterableUniqueValues

        #endregion // Public

        #region Private

        #region CachedFlatUniqueValues

        private FilterItemCollection CachedFlatUniqueValues
        {
            get
            {
                if (this._cachedFlatUniqueValues == null)
                {
                    this._cachedFlatUniqueValues = this.GetCollection();
                }

                return this._cachedFlatUniqueValues;
            }
        }

        #endregion // CachedFlatUniqueValues

        #region CachedCheckedCount

        private int CachedCheckedCount { get; set; }
        
        #endregion // CachedCheckedCount

        #region CachedUncheckedCount

        private int CachedUncheckedCount { get; set; }

        #endregion // CachedCheckedCount

        #endregion // Private

        #endregion // Properties

        #region Overrides

        #region OnRawValuesSet
        
        /// <summary>
        /// Method called after raw data is pulled from the data manager so that any manipulation that needs to take place can.
        /// </summary>
        protected override void OnRawValuesSet()
        {
            this._addTime = false;

            foreach (object o in this.RawValues)
            {
                if (o != null)
                {
                    DateTime date = (DateTime)o;

                    if (date.Hour != 0 || date.Minute != 0 || date.Second != 0)
                    {
                        this._addTime = true;
                    }
                }
            }

            this.DateFilterTypeList.Clear();
            this.SetupComboBox();
        }

        #endregion // OnRawValuesSet

        #region AcceptChanges

        /// <summary>
        /// Processes the elements in the <see cref="Panel"/>.
        /// </summary>
        protected internal override void AcceptChanges()
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

            if (this.CachedUncheckedCount == 0 || this.CachedCheckedCount == 0)
            {
                shouldClearFilters = true;
            }

            if (shouldClearFilters)
            {
                rf.Conditions.LogicalOperator = LogicalOperator.Or;

                rf.Conditions.ClearSilently();
            }

            if (this.AppendFilters)
            {
                


                foreach (XamGridFilterDate year in this.FilterableUniqueValues)
                {
                    if (year.NullDate)
                    {
                        this.SynchornizeCachedValue(year);
                        continue;
                    }
                    foreach (XamGridFilterDate month in year.Children)
                    {
                        foreach (XamGridFilterDate date in month.Children)
                        {
                            if (this._addTime)
                            {
                                foreach (XamGridFilterDate hour in date.Children)
                                {
                                    foreach (XamGridFilterDate minute in hour.Children)
                                    {
                                        foreach (XamGridFilterDate second in minute.Children)
                                        {
                                            this.SynchornizeCachedValue(second);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                this.SynchornizeCachedValue(date);
                            }
                        }
                    }
                }

                this.ProcessChanges(this.CachedCheckedCount, this.CachedUncheckedCount, ((FilterItemCollection<XamGridFilterDate>)this.CachedUniqueValues).ToList());
            }
            else
            {
                if (string.IsNullOrEmpty(this.FilterText))
                {
                    this.ProcessChanges(this.CheckedCount, this.UncheckedCount, this.FilterableUniqueValues);
                }
                else
                {
                    int checkedNodes = 0;
                    foreach (XamGridFilterDate year in this.FilterableUniqueValues)
                    {
                        if (year.NullDate)
                        {
                            checkedNodes++;
                            continue;
                        }
                        foreach (XamGridFilterDate month in year.Children)
                        {
                            foreach (XamGridFilterDate date in month.Children)
                            {
                                if (this._addTime)
                                {
                                    foreach (XamGridFilterDate hour in date.Children)
                                    {
                                        foreach (XamGridFilterDate minute in hour.Children)
                                        {
                                            foreach (XamGridFilterDate second in minute.Children)
                                            {
                                                checkedNodes++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    checkedNodes++;
                                }
                            }
                        }
                    }

                    this.ProcessChanges(checkedNodes, this.RawValues.Count - checkedNodes, this.FilterableUniqueValues);
                }
            }

            cell.Row.ColumnLayout.InvalidateFiltering();
            grid.ResetPanelRows();
            grid.OnFiltered(rm.RowFiltersCollectionResolved);
        }

        #endregion // AcceptChanges

        #region FilterUniqueValueInternal

        internal override void FilterUniqueValueInternal(string filterValue)
        {
            if (this.CachedUniqueValues != null && this.CachedUniqueValues.Count > 0)
            {
                if (string.IsNullOrEmpty(filterValue))
                {
                    this._boolBuildCachedList = false;
                    this._cacheCount = false;
                    this.RefreshUniqueValues(this.CachedFlatUniqueValues);
                    this._cacheCount = true;
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

                    DateFilterObjectType dateFilterObjectType = this.DateFilterObjectTypeItem.DateFilterObjectType;

                    Task<Phase2Payload>.Factory.StartNew(
                        () =>
                        {
                            var manager = DataManagerBase.CreateDataManager(this.CachedFlatUniqueValues);
                            manager.Filters = new RecordFilterCollection();
                            manager.Filters.Add(new FilterValueProxyRowsFilter(typeof(XamGridFilterDate), manager.CachedTypedInfo));
                            manager.Filters[0].Conditions.Add(new DateFilterObjectStringFilter { DateFilterObjectType = dateFilterObjectType, FilterValue = filterValue });
                            // manager.Sort.Add(new SortContext<XamGridFilterDate, DateTime>("Date", true, false, manager.CachedTypedInfo));
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

                            this.FilterText = filterValue;
                            this._boolBuildCachedList = false;
                            this._cacheCount = false;
                            this.RefreshUniqueValues(result.Result.FilteredList);
                            this._cacheCount = true;
                            this._boolBuildCachedList = true;

                            this.InvalidateShowAppendFilterButton(filterValue);


                            Phase2TokenSourceQueue.Remove(result.Result.CancellationTokenSource);
                            if (Phase2TokenSourceQueue.Count == 0)
                            {
                                this.IsBusy = false;
                            }
                            this.EnsureCurrentState();
                        },
                        uiContext);
                }

                
            }
        }

        #endregion // FilterUniqueValueInternal

        #region GetCollection

        /// <summary>
        /// Creates the <see cref="FilterItemCollection"/> which will be used to store the unique values.
        /// </summary>
        /// <returns></returns>
        protected override FilterItemCollection GetCollection()
        {
            return new XamGridFilterYearCollection();
        }

        #endregion // GetCollection

        #region OnSelectAllCheckBoxChecked

        /// <summary>
        /// Called when the <see cref="Infragistics.Controls.Grids.Primitives.FilterSelectionControl.SelectAllCheckBox"/> is checked.
        /// </summary>
        /// <param name="cbSender"></param>
        protected override void OnSelectAllCheckBoxChecked(CheckBox cbSender)
        {
            bool isChecked = (cbSender.IsChecked == true);

            this.SetAllCheckBoxValues(isChecked);

            this.AllowFiltering = cbSender.IsChecked != false;
        }

        #endregion // OnSelectAllCheckBoxChecked

        #region ClearExistingFilters
        /// <summary>
        /// Resets the existing filter
        /// </summary>
        protected internal override void ClearExistingFilters()
        {
            base.ClearExistingFilters();

            SetAllCheckBoxValues(true);
        }

        #endregion // ClearExistingFilters

        #region ControlLoaded
        
        /// <summary>
        /// Method called when the control is loaded.
        /// </summary>
        protected override void ControlLoaded()
        {
            this.CachedCheckedCount = this.CachedUncheckedCount = 0;

            base.ControlLoaded();

            this.OnPropertyChanged("DateFilterObjectType");
        }

        #endregion // ControlLoaded

        #endregion // Overrides

        #region Methods

        #region Private

        #region SetupComboBox

        private void SetupComboBox()
        {
            ObservableCollection<DateFilterListDisplayObject> list = this.DateFilterTypeList;
            DateFilterListDisplayObject item = new DateFilterListDisplayObject { DateFilterObjectType = DateFilterObjectType.All, Name = SRGrid.GetString("DateFilterObjectType_All") };
            list.Add(item);
            this.DateFilterObjectTypeItem = item;
            list.Add(new DateFilterListDisplayObject { DateFilterObjectType = DateFilterObjectType.Year, Name = SRGrid.GetString("DateFilterObjectType_Year") });
            list.Add(new DateFilterListDisplayObject { DateFilterObjectType = DateFilterObjectType.Month, Name = SRGrid.GetString("DateFilterObjectType_Month") });
            list.Add(new DateFilterListDisplayObject { DateFilterObjectType = DateFilterObjectType.Date, Name = SRGrid.GetString("DateFilterObjectType_Date") });
            if (_addTime)
            {
                list.Add(new DateFilterListDisplayObject { DateFilterObjectType = DateFilterObjectType.Hour, Name = SRGrid.GetString("DateFilterObjectType_Hour") });
                list.Add(new DateFilterListDisplayObject { DateFilterObjectType = DateFilterObjectType.Minute, Name = SRGrid.GetString("DateFilterObjectType_Minute") });
                list.Add(new DateFilterListDisplayObject { DateFilterObjectType = DateFilterObjectType.Second, Name = SRGrid.GetString("DateFilterObjectType_Second") });
            }
        }

        #endregion // SetupComboBox

        #region GenerateUniqueList
        /// <summary>
        /// Given a list of objects builds the UniqueValue list of the control.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="values"></param>
        /// <param name="shouldCheck"></param>
        /// <returns></returns>
        protected override int GenerateUniqueList(IList items, List<object> values, bool shouldCheck)
        {
            this.UniqueValuesClear();

            int checkedCounter = 0;

            Type type = this.Cell.Column.DataType;

            
            foreach (object o in items)
            {
                


                bool bFound = false;
                foreach (object obj in values)
                {
                    if (o == obj)
                    {
                        bFound = true;
                    }
                    else if (o != null)
                    {
                        bFound = o.Equals(obj);
                    }

                    if (!bFound && type == typeof(string))
                    {
                        if (string.IsNullOrEmpty((string)o) && string.IsNullOrEmpty((string)obj))
                        {
                            bFound = true;
                        }
                    }

                    if (bFound)
                        break;
                }

                bool updateCheckedCounter = (shouldCheck) ? bFound : !bFound;
                if (updateCheckedCounter)
                {
                    checkedCounter++;
                }

                bool isChecked = (shouldCheck) ? bFound : !bFound;

                this.AddUniqueValue(o, isChecked, type);
            }


            this.UniqueValues.RaiseCollectionChanged();
            this.OnPropertyChanged("FilterableUniqueValues");

            this.SetUpIsExpanded();

            return checkedCounter;
        }

        #endregion // GenerateUniqueList

        #region CreateAllCheckBoxes

        /// <summary>
        /// Creates the list that will be used to populate the selection control.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="shouldCheck"></param>
        protected override void CreateAllCheckBoxes(IList items, bool shouldCheck)
        {
            this.UniqueValuesClear();
            Type type = this.Cell.Column.DataType;            
            foreach (object o in items)
                this.AddUniqueValue(o, shouldCheck, type);

            this.UniqueValues.RaiseCollectionChanged();
            this.OnPropertyChanged("FilterableUniqueValues");

            this.SetUpIsExpanded();
        }

        #endregion // CreateAllCheckBoxes

        #region AddUniqueValue

        /// <summary>
        /// Creates an object for UniqueList and adds it to the list.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isChecked"></param>
        /// <param name="type"></param>
        protected override void AddUniqueValue(object value, bool isChecked, Type type)
        {
            if (value == null)
            {
                XamGridFilterDate nullValue = new XamGridFilterDate();
                nullValue.IsChecked = isChecked;
                nullValue.DateFilterObjectType = DateFilterObjectType.None;
                this.UniqueValues.Add(nullValue);
                nullValue.PropertyChanged -= UniqueValue_PropertyChanged;
                nullValue.PropertyChanged += UniqueValue_PropertyChanged;

                nullValue = new XamGridFilterDate();
                nullValue.IsChecked = isChecked;
                if (_boolBuildCachedList)
                {
                    this.CachedUniqueValues.AddSilently(nullValue);
                    this.CachedFlatUniqueValues.AddSilently(nullValue);
                    if (this._cacheCount)
                    {
                        if (isChecked)
                        {
                            this.CachedCheckedCount++;
                            this.CheckedCount++;
                        }
                        else
                        {
                            this.CachedUncheckedCount++;
                            this.UncheckedCount++;
                        }
                    }
                }
                return;
            }

            string filterCellValue = Regex.Escape(this.FilterText);
            Regex regex = new Regex(filterCellValue, RegexOptions.IgnoreCase);

            DateTime date = (DateTime)value;

            XamGridFilterDate filterYear = ((XamGridFilterYearCollection)this.UniqueValues).GetItemByYear(date);
            XamGridFilterDate filterYearCached = ((XamGridFilterYearCollection)this.CachedUniqueValues).GetItemByYear(date);
            if (filterYear == null)
            {
                if (_boolBuildCachedList)
                {
                    filterYearCached = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Year };
                    this.CachedUniqueValues.AddSilently(filterYearCached);
                    this.CachedFlatUniqueValues.AddSilently(filterYearCached);
                }
                filterYear = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Year };
                this.UniqueValues.AddSilently(filterYear);
            }

            XamGridFilterDate filterMonth = filterYear.GetMonthByDate(date);
            XamGridFilterDate filterMonthCached = filterYearCached.GetMonthByDate(date);
            if (filterMonth == null)
            {
                if (_boolBuildCachedList)
                {
                    filterMonthCached = new XamGridFilterDate { DateFilterObjectType = DateFilterObjectType.Month, Date = date, Parent = filterYearCached };
                    filterYearCached.Children.Add(filterMonthCached);
                    this.CachedFlatUniqueValues.AddSilently(filterMonthCached);
                }
                filterMonth = new XamGridFilterDate { DateFilterObjectType = DateFilterObjectType.Month, Date = date, Parent = filterYear };
                filterYear.Children.Add(filterMonth);

                if (this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.All ||
                    this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.Month)
                {
                    if (!string.IsNullOrEmpty(this.FilterText) && regex.IsMatch(filterMonth.ContentStringMonth))
                    {
                        filterYear.IsExpanded = true;
                    }
                }
            }

            XamGridFilterDate filterDate = filterMonth.GetDayByDate(date);
            XamGridFilterDate filterDateCached = filterMonthCached.GetDayByDate(date);
            if (filterDate == null)
            {
                if (_boolBuildCachedList)
                {
                    filterDateCached = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Date, Parent = filterMonthCached };
                    filterMonthCached.Children.Add(filterDateCached);
                    this.CachedFlatUniqueValues.AddSilently(filterDateCached);
                }
                filterDate = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Date, Parent = filterMonth };
                filterMonth.Children.Add(filterDate);

                if (this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.All ||                    this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.Date)
                {
                    if (!string.IsNullOrEmpty(this.FilterText) && regex.IsMatch(filterDate.ContentString))
                    {
                        filterMonth.IsExpanded = true;
                        filterYear.IsExpanded = true;
                    }
                }

                if (!this._addTime)
                {
                    filterDate.IsChecked = isChecked;
                    if (_boolBuildCachedList)
                    {
                        filterDateCached.IsChecked = isChecked;
                    }
                }
            }

            if (this._addTime)
            {
                XamGridFilterDate filterHour = filterDate.GetHourByDate(date);
                XamGridFilterDate filterHourCached = filterDateCached.GetHourByDate(date);
                if (filterHour == null)
                {
                    if (_boolBuildCachedList)
                    {
                        filterHourCached = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Hour, Parent = filterDateCached };
                        filterDateCached.Children.Add(filterHourCached);
                        this.CachedFlatUniqueValues.AddSilently(filterHourCached);
                    }
                    filterHour = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Hour, Parent = filterDate };
                    filterDate.Children.Add(filterHour);
                    if (this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.All ||                   this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.Hour)
                    {
                        if (!string.IsNullOrEmpty(this.FilterText) && regex.IsMatch(filterHour.ContentString))
                        {
                            filterDate.IsExpanded = true;
                            filterMonth.IsExpanded = true;
                            filterYear.IsExpanded = true;
                        }
                    }
                }

                XamGridFilterDate filterMinute = filterHour.GetMinuteByDate(date);
                XamGridFilterDate filterMinuteCached = filterHourCached.GetMinuteByDate(date);
                if (filterMinute == null)
                {
                    if (_boolBuildCachedList)
                    {
                        filterMinuteCached = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Minute, Parent = filterHourCached };
                        filterHourCached.Children.Add(filterMinuteCached);
                        this.CachedFlatUniqueValues.AddSilently(filterMinuteCached);
                    }
                    filterMinute = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Minute, Parent = filterHour };
                    filterHour.Children.Add(filterMinute);
                    if (this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.All || this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.Minute)
                    {
                        if (!string.IsNullOrEmpty(this.FilterText) && regex.IsMatch(filterMinute.ContentString))
                        {
                            filterHour.IsExpanded = true;
                            filterDate.IsExpanded = true;
                            filterMonth.IsExpanded = true;
                            filterYear.IsExpanded = true;
                        }
                    }
                }

                XamGridFilterDate filterSecond = filterMinute.GetSecondByDate(date);
                XamGridFilterDate filterSecondCached;
                if (filterSecond == null)
                {
                    if (_boolBuildCachedList)
                    {
                        filterSecondCached = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Second, Parent = filterMinuteCached };
                        filterMinuteCached.Children.Add(filterSecondCached);
                        this.CachedFlatUniqueValues.AddSilently(filterSecondCached);
                        filterSecondCached.IsChecked = isChecked;
                    }
                    filterSecond = new XamGridFilterDate { Date = date, DateFilterObjectType = DateFilterObjectType.Second, Parent = filterMinute };
                    filterMinute.Children.Add(filterSecond);

                    filterSecond.IsChecked = isChecked;
                    if (this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.All || this.DateFilterObjectTypeItem.DateFilterObjectType == DateFilterObjectType.Second)
                    {
                        if (!string.IsNullOrEmpty(this.FilterText) && regex.IsMatch(filterSecond.ContentString))
                        {
                            filterMinute.IsExpanded = true;
                            filterHour.IsExpanded = true;
                            filterDate.IsExpanded = true;
                            filterMonth.IsExpanded = true;
                            filterYear.IsExpanded = true;
                        }
                    }
                }

                // hack fix for a second ... we attach an event handler to the filter date( the lowest level )...maybe we don't need to anymore
                filterDate = filterSecond;
            }

            if (this._cacheCount)
            {
                if (isChecked)
                {
                    this.CachedCheckedCount++;
                    this.CheckedCount++;
                }
                else
                {
                    this.CachedUncheckedCount++;
                    this.UncheckedCount++;
                }
            }

            filterDate.PropertyChanged -= UniqueValue_PropertyChanged;
            filterDate.PropertyChanged += UniqueValue_PropertyChanged;
        }

        #endregion // AddUniqueValue

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

        private static void ApplyNegativeFilterForItem(object value, bool isChecked, Column col, ColumnLayout colLayout, RowsManager rm, RowsFilter rf, bool raiseEvents)
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
                        colLayout.BuildNullableFilter(rm.RowFiltersCollectionResolved, col, new EqualsOperand(), true, false, false);
                    else
                        colLayout.BuildFilters(rm.RowFiltersCollectionResolved, value, col, new EqualsOperand(), true, false, raiseEvents);
                    rf.Conditions.LogicalOperator = LogicalOperator.Or;
                }
            }
        }

        #endregion // ApplyNegativeFilterForItem

        #region SetUpIsExpanded

        void SetUpIsExpanded()
        {
            if (this.FilterableUniqueValues != null && string.IsNullOrEmpty(this.FilterText))
            {
                int itemCount = this.UniqueValues.Count;
                if (itemCount == 1 && this.Cell.Column.DataType == typeof(DateTime))
                {
                    SetUpIsExpandedHelper((XamGridFilterDate)this.UniqueValues[0]);
                }
                else if (this.Cell.Column.DataType == typeof(DateTime?) && itemCount > 0 && itemCount <= 2)
                {
                    if (itemCount == 1)
                    {
                        SetUpIsExpandedHelper((XamGridFilterDate)this.UniqueValues[0]);
                    }
                    else
                    {
                        SetUpIsExpandedHelper((XamGridFilterDate)this.UniqueValues[1]);
                    }
                }
            }
        }

        #endregion // SetUpIsExpanded

        #region SetUpIsExpandedHelper
        private void SetUpIsExpandedHelper(XamGridFilterDate date)
        {
            XamGridFilterDate year = date;
            year.IsExpanded = true;

            if (year.Children.Count == 1)
            {
                XamGridFilterDate month = year.Children[0];
                month.IsExpanded = true;
            }
        }
        #endregion // SetUpIsExpandedHelper

        #region RefreshUniqueValues

        private void RefreshUniqueValues(IEnumerable values)
        {
            this.UniqueValuesClear();

            int count = 0;
            foreach (XamGridFilterDate date in values)
            {
                if (++count > Limit)
                {
                    // Break. We are not building cache when RefreshUniqueValues is called. 
                    this.IsLimited = true;
                    break;
                }

                this.AddUniqueValue(date.NullDate ? null : (object)date.Date, true, typeof(DateTime));
            }

            this.UniqueValues.RaiseCollectionChanged();
            this.OnPropertyChanged("FilterableUniqueValues");

            this.SetUpIsExpanded();
        }

        #endregion // RefreshUniqueValues

        #region ResetIsChecked
        
        private void ResetIsChecked(XamGridFilterDate date, bool isChecked)
        {
            date.PropertyChanged -= UniqueValue_PropertyChanged;
            date.ChangeSilent(isChecked);
            date.PropertyChanged += UniqueValue_PropertyChanged;
        }

        #endregion // ResetIsChecked

        #region UniqueValuesClear

        internal override void UniqueValuesClear()
        {
            if (this.UniqueValues != null)
            {
                IEnumerable list = this.UniqueValues.Cast<XamGridFilterDate>();

                UniqueValuesClear(list);

                this.UniqueValues.Clear();
            }
        }

        internal override void UniqueValuesClear(IEnumerable list)
        {
            foreach (XamGridFilterDate obj in list)
            {
                obj.PropertyChanged -= UniqueValue_PropertyChanged;
                UniqueValuesClear(obj.Children);
            }
        }

        #endregion // UniqueValuesClear

        #region SetAllCheckBoxValues

        private void SetAllCheckBoxValues(bool isChecked)
        {
            int count = 0;

            if (this.FilterableUniqueValues != null)
            {
                foreach (XamGridFilterDate year in this.FilterableUniqueValues)
                {
                    foreach (XamGridFilterDate month in year.Children)
                    {
                        foreach (XamGridFilterDate date in month.Children)
                        {
                            if (date.Children.Count > 0)
                            {
                                foreach (XamGridFilterDate hour in date.Children)
                                {
                                    foreach (XamGridFilterDate minute in hour.Children)
                                    {
                                        foreach (XamGridFilterDate second in minute.Children)
                                        {
                                            this.ResetIsChecked(second, isChecked);
                                            count++;
                                        }
                                        this.ResetIsChecked(minute, isChecked);
                                        count++;
                                    }
                                    this.ResetIsChecked(hour, isChecked);
                                    count++;
                                }

                            }
                            this.ResetIsChecked(date, isChecked);
                            count++;
                        }
                        this.ResetIsChecked(month, isChecked);
                        count++;
                    }
                    this.ResetIsChecked(year, isChecked);
                    count++;
                }
            }

            if (isChecked)
            {
                this.CheckedCount = count;
                this.UncheckedCount = 0;
            }
            else
            {
                this.UncheckedCount = count;
                this.CheckedCount = 0;
            }
        }

        #endregion // SetAllCheckBoxValues

        #region SynchornizeCachedValue

        private void SynchornizeCachedValue(XamGridFilterDate date)
        {
            foreach (XamGridFilterDate tempDate in this.CachedFlatUniqueValues)
            {
                if (date.NullDate && tempDate.NullDate)
                {
                    if (tempDate.IsChecked != date.IsChecked)
                    {
                        if (tempDate.IsChecked == true)
                            this.CachedCheckedCount--;
                        else
                            this.CachedUncheckedCount--;

                        tempDate.IsChecked = date.IsChecked;

                        if (tempDate.IsChecked == true)
                            this.CachedCheckedCount++;
                        else
                            this.CachedUncheckedCount++;
                    }
                    break;
                }

                if (date.DateFilterObjectType == tempDate.DateFilterObjectType && date.Date == tempDate.Date)
                {
                    if (tempDate.IsChecked != date.IsChecked)
                    {
                        if (tempDate.IsChecked == true)
                            this.CachedCheckedCount--;
                        else
                            this.CachedUncheckedCount--;

                        tempDate.IsChecked = date.IsChecked;

                        if (tempDate.IsChecked == true)
                            this.CachedCheckedCount++;
                        else
                            this.CachedUncheckedCount++;
                    }
                    break;
                }
            }
        }

        #endregion // SynchornizeCachedValue

        #region ProcessChanges

        private void ProcessChanges(int checkedCount, int uncheckedCount, IEnumerable list)
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

            if (checkedCount > uncheckedCount || checkedCount == 0 )
            {
                foreach (XamGridFilterDate year in list)
                {
                    if (year.NullDate)
                    {
                        DateFilterSelectionControl.ApplyFilterForItem(null, year.IsChecked == true, col, colLayout, rm, rf, true);
                        continue;
                    }

                    foreach (XamGridFilterDate month in year.Children)
                    {
                        foreach (XamGridFilterDate date in month.Children)
                        {
                            if (this._addTime)
                            {
                                foreach (XamGridFilterDate hour in date.Children)
                                {
                                    foreach (XamGridFilterDate minute in hour.Children)
                                    {
                                        foreach (XamGridFilterDate second in minute.Children)
                                        {
                                            if (second != null && second.IsChecked != null)
                                            {
                                                DateFilterSelectionControl.ApplyFilterForItem(second.Date, (bool)second.IsChecked, col, colLayout, rm, rf, true);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (date != null && date.IsChecked != null)
                                {
                                    DateFilterSelectionControl.ApplyFilterForItem(date.Date, (bool)date.IsChecked, col, colLayout, rm, rf, true);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (XamGridFilterDate year in list)
                {
                    if (year.NullDate)
                    {
                        DateFilterSelectionControl.ApplyNegativeFilterForItem(null, year.IsChecked == true, col, colLayout, rm, rf, true);
                        continue;
                    }

                    foreach (XamGridFilterDate month in year.Children)
                    {
                        foreach (XamGridFilterDate date in month.Children)
                        {
                            if (this._addTime)
                            {
                                foreach (XamGridFilterDate hour in date.Children)
                                {
                                    foreach (XamGridFilterDate minute in hour.Children)
                                    {
                                        foreach (XamGridFilterDate second in minute.Children)
                                        {
                                            if (second != null && second.IsChecked != null)
                                            {
                                                DateFilterSelectionControl.ApplyNegativeFilterForItem(second.Date, (bool)second.IsChecked, col, colLayout, rm, rf, true);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (date != null && date.IsChecked != null)
                                {
                                    DateFilterSelectionControl.ApplyNegativeFilterForItem(date.Date, (bool)date.IsChecked, col, colLayout, rm, rf, true);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion // ProcessChanges

        #endregion // Private

        #endregion // Methods

        #region EventHandlers

        #region UniqueValue_PropertyChanged

        private void UniqueValue_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            XamGridFilterDate uniquevalue = (XamGridFilterDate)sender;

            if (e.PropertyName == "IsChecked")
            {
                bool trueFound = false;
                bool falseFound = false;
                bool nullFound = false;

                foreach (XamGridFilterDate val in this.FilterableUniqueValues)
                {
                    if (val.IsChecked == true)
                    {
                        trueFound = true;
                    }
                    else if (val.IsChecked == false)
                    {
                        falseFound = true;
                    }
                    else if (val.IsChecked == null)
                    {
                        nullFound = true;
                    }

                    if (
                        (trueFound && falseFound) ||
                        (trueFound && nullFound) ||
                        (falseFound && nullFound)
                        )
                        break;
                }

                if (uniquevalue.IsChecked == true)
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

                if (
                    (trueFound && falseFound) ||
                    (trueFound && nullFound) ||
                    (falseFound && nullFound)
                    )
                {
                    isCheckedValue = null;
                }
                else if (trueFound)
                {
                    isCheckedValue = true;
                }
                else if (falseFound)
                {
                    isCheckedValue = false;
                }
                else
                {
                    isCheckedValue = null;
                }

                this.SetSelectAllCheckBox(isCheckedValue);
            }
        }

        #endregion // UniqueValue_PropertyChanged

        #region FilterableUniqueValuesDataManager_CollectionChanged

        protected override void OnUniqueValuesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged("FilterableUniqueValues");
            this.EnsureCurrentState();
        }

        #endregion // FilterableUniqueValuesDataManager_CollectionChanged

        #endregion // EventHandlers
    }
    #endregion // DateFilterSelectionControl

    #region DateFilterListDisplayObject
    /// <summary>
    /// A class that will allow by the <see cref="DateFilterSelectionControl"/> for it's <see cref="DateFilterSelectionControl.DateFilterTypeList"/> collection.
    /// </summary>
    public class DateFilterListDisplayObject
    {
        /// <summary>
        /// Gets / sets the <see cref="DateFilterObjectType"/> which should be set for the control.
        /// </summary>
        public DateFilterObjectType DateFilterObjectType { get; set; }

        /// <summary>
        /// Gets / sets the text that will be displayed.
        /// </summary>
        public string Name { get; set; }
    }
    #endregion // DateFilterListDisplayObject
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