using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Grids.Primitives;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// The base class for all standard column objects of the <see cref="XamGrid"/>.
	/// </summary>
	public abstract class Column : ColumnBase, ISelectableObject
	{
		#region Members

		double _actualWidth;
		FixedState _isFixed;
		FixedIndicatorDirection? _fixedDirection;
		bool _isFixable, _isSortable, _isSelected, _isGroupBy, _allowCaseSensitiveSort = true, _isFilterable = true, _isSummable = true;
		HorizontalAlignment _hAlign;
		VerticalAlignment _vAlign;
		DataTemplate _footerTemplate;
		object _comparer, _groupByComparer;
		SortDirection _isSorted;
		FilterColumnSettings _filterColumnSettings;
		FilterOperand _defaultFilterOperand;
		SummaryColumnSettings _summaryColumnSettings;
		ConditionalFormatCollection _conditionalFormatCollection;
        bool _isGroupable;
        Column _cachedParentColumn;
        List<Column> _cachedChildren;
        bool _ignoreGroupByCalls = false;
        string _cachedTypeName;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Column"/> class.
		/// </summary>
		protected Column()
		{
			this._isFixable = true;
			this.IsMovable = true;
			this._isSortable = true;
			this.IsResizable = true;
			this.IsGroupable = true;

			this.AddNewRowItemTemplateVerticalContentAlignment =  this.VerticalContentAlignment = VerticalAlignment.Center;
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region Width

		/// <summary>
		/// Identifies the <see cref="Width"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width", typeof(ColumnWidth?), typeof(Column), new PropertyMetadata(null, new PropertyChangedCallback(WidthChanged)));

		/// <summary>
		/// Gets/sets the <see cref="ColumnWidth"/> of the <see cref="ColumnBase"/>
		/// </summary>
        [TypeConverter(typeof(NullableColumnWidthTypeConverter))]
		public virtual ColumnWidth? Width
		{
			get { return (ColumnWidth?)this.GetValue(WidthProperty); }
			set { this.SetValue(WidthProperty, value); }
		}

		private static void WidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			Column col = (Column)obj;
			col.ActualWidth = 0;
			col.OnPropertyChanged("Width");
		}

		#endregion // Width

		#region WidthResolved

		/// <summary>
		/// Resolves the <see cref="ColumnWidth"/> that is being applied to this <see cref="Column"/>
		/// </summary>
		public virtual ColumnWidth WidthResolved
		{
			get
			{
				if (this.Width == null)
					return this.ColumnLayout.ColumnWidthResolved;
				else
					return (ColumnWidth)this.Width;
			}

		}

		#endregion // WidthResolved

		#region ActualWidth
		double _cachedActualWidth;
		/// <summary>
		/// Gets the Actual width of the column. 
		/// </summary>
		/// <remarks>Note: this value is only available when the column is rendered.</remarks>
		public double ActualWidth
		{
			get
			{
				return this._actualWidth;
			}
			internal set
			{
				if (this._actualWidth != value)
				{
					this._actualWidth = value;
					this.OnPropertyChanged("ActualWidth");
				}
			}
		}
		#endregion // ActualWidth

		#region MinimumWidth

		/// <summary>
		/// Identifies the <see cref="MinimumWidth"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MinimumWidthProperty = DependencyProperty.Register("MinimumWidth", typeof(double), typeof(Column), new PropertyMetadata(0.0, new PropertyChangedCallback(MinimumWidthChanged)));

		/// <summary>
		/// Gets/sets the minimum width the column is allowed to be.
		/// </summary>
		public virtual double MinimumWidth
		{
			get { return (double)this.GetValue(MinimumWidthProperty); }
			set { this.SetValue(MinimumWidthProperty, value); }
		}

		private static void MinimumWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			Column cb = (Column)obj;
			cb.OnPropertyChanged("MinimumWidth");
		}

		#endregion // MinimumWidth

		#region MaximumWidth

		/// <summary>
		/// Identifies the <see cref="MaximumWidth"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MaximumWidthProperty = DependencyProperty.Register("MaximumWidth", typeof(double), typeof(Column), new PropertyMetadata(Double.PositiveInfinity, new PropertyChangedCallback(MaximumWidthChanged)));

		/// <summary>
		/// Gets/sets the maximum width the column is allowed to be.
		/// </summary>
		public virtual double MaximumWidth
		{
			get { return (double)this.GetValue(MaximumWidthProperty); }
			set { this.SetValue(MaximumWidthProperty, value); }
		}

		private static void MaximumWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			Column cb = (Column)obj;
			cb.OnPropertyChanged("MaximumWidth");
		}

		#endregion // MaximumWidth

		#region FooterTemplate

		/// <summary>
		/// Gets/Sets the <see cref="DataTemplate"/> used to define the Content of the Footer of the <see cref="Column"/>.
		/// </summary>
		public DataTemplate FooterTemplate
		{
			get { return this._footerTemplate; }
			set
			{
				this._footerTemplate = value;
				this.OnPropertyChanged("FooterTemplate");
			}
		}

		#endregion // FooterTemplate

        #region FooterText

        /// <summary>
        /// Identifies the <see cref="FooterText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FooterTextProperty = DependencyProperty.Register("FooterText", typeof(string), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(FooterTextChanged)));

        /// <summary>
        /// Gets/Sets the text that will be displayed in the Footer.
        /// </summary>
        /// <remarks>If the <see cref="Column.FooterTemplate"/> is set, this property will have no effect.</remarks>
        public string FooterText
        {
            get { return (string)this.GetValue(FooterTextProperty); }
            set { this.SetValue(FooterTextProperty, value); }
        }

        private static void FooterTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("FooterText");
        }

        #endregion // FooterText 

		#region IsFixed

		/// <summary>
		/// Gets/Sets if a <see cref="Column"/> should be unpinned or pinned to the Left or Right side of the <see cref="XamGrid"/>.
		/// </summary>
		public FixedState IsFixed
		{
			get { return this._isFixed; }
			set
			{
				if (this._isFixed != value)
				{
					this.SetFixedColumnState(value);
				}
			}
		}

		#endregion // IsFixed

		#region FixedIndicatorDirection

		/// <summary>
		/// Gets/Sets what side of the <see cref="XamGrid"/> should be locked to if the <see cref="Column.IsFixed"/> property is set to true.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<FixedIndicatorDirection>))]
		public FixedIndicatorDirection? FixedIndicatorDirection
		{
			get { return this._fixedDirection; }
			set
			{
				if (this._fixedDirection != value)
				{
					this._fixedDirection = value;
					this.OnPropertyChanged("FixedIndicatorDirection");
				}
			}
		}

		#endregion // FixedIndicatorDirection

		#region FixedIndicatorDirectionResolved

		/// <summary>
		/// Resolves the actual <see cref="FixedIndicatorDirection"/> of the <see cref="Column"/>
		/// </summary>
		public FixedIndicatorDirection FixedIndicatorDirectionResolved
		{
			get
			{
				if (this.FixedIndicatorDirection == null && this.ColumnLayout != null)
					return this.ColumnLayout.FixedColumnSettings.FixedIndicatorDirectionResolved;
				else
					return (FixedIndicatorDirection)this.FixedIndicatorDirection;
			}

		}

		#endregion // FixedIndicatorDirectionResolved

		#region IsFixable

		/// <summary>
		/// Gets/Sets if a Column can be fixed. 
		/// </summary>
		public virtual bool IsFixable
		{
			get 
            {
                if (this.ParentColumn == null)
                {
                    if (this.IsGroupBy && this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                        return false;

                    return this._isFixable;
                }
                else
                    return false;
            }
			set
			{
				if (this._isFixable != value)
				{
					this._isFixable = value;
					this.OnPropertyChanged("IsFixable");
				}
			}
		}

		#endregion IsFixable

		#region HorizontalContentAlignment

		/// <summary>
		/// Gets/Sets the <see cref="HorizontalAlignment"/> of the content of all cells in this <see cref="Column"/>
		/// </summary>
		public HorizontalAlignment HorizontalContentAlignment
		{
			get { return this._hAlign; }
			set
			{
				this._hAlign = value;
				this.OnPropertyChanged("HorizontalContentAlignment");
			}
		}

		#endregion // HorizontalContentAlignment

		#region VerticalContentAlignment

		/// <summary>
		/// Gets/Sets the <see cref="VerticalAlignment"/> of the content of all cells in this <see cref="Column"/>
		/// </summary>
		public VerticalAlignment VerticalContentAlignment
		{
			get { return this._vAlign; }
			set
			{
				this._vAlign = value;
				this.OnPropertyChanged("VerticalContentAlignment");
			}
		}

		#endregion // VerticalContentAlignment

		#region IsSortable
		/// <summary>
		/// Gets/Sets if a Column can be sorted. 
		/// </summary>
		public virtual bool IsSortable
		{
			get
			{
				return this._isSortable;
			}
			set
			{
				if (this._isSortable != value)
				{
					this._isSortable = value;
					this.OnPropertyChanged("IsSortable");
				}
			}
		}
		#endregion // IsSortable

		#region AllowCaseSensitiveSort
		/// <summary>
		/// Gets/sets if a column should be sorted case sensitive.  This is only used for string columns.
		/// </summary>
		public bool AllowCaseSensitiveSort
		{
			get
			{
				return this._allowCaseSensitiveSort;
			}
			set
			{
				if (this._allowCaseSensitiveSort != value)
				{
					this._allowCaseSensitiveSort = value;
					this.OnPropertyChanged("AllowCaseSensitiveSort");
				}
			}
		}
		#endregion // AllowCaseSensitiveSort

		#region SortComparer
		/// <summary>
		/// Gets/sets a custom IComparer template object which will be used when sorting data.
		/// </summary>
		public object SortComparer
		{
			get
			{
				return this._comparer;
			}
			set
			{
				if (this._comparer != value)
				{
					this._comparer = value;

					ValidateSortComparer();

					this.OnPropertyChanged("SortComparer");
				}
			}
		}
		#endregion // SortComparer

		#region IsMovable

		/// <summary>
		/// Gets/Sets if a Column can be moved via the UI.
		/// </summary>
		public virtual bool IsMovable
		{
			get;
			set;
		}

		#endregion // IsMovable

		#region IsResizable

		/// <summary>
		/// Gets/Sets if a Column can be resized via the UI.
		/// </summary>
		public virtual bool IsResizable
		{
			get;
			set;
		}

		#endregion // IsMovable

		#region IsFilterable

		/// <summary>
		/// Gets/sets if a column can be filtered via the UI.
		/// </summary>
		public virtual bool IsFilterable
		{
			get
			{
				return _isFilterable;
			}
			set
			{
				if (this._isFilterable != value)
				{
					this._isFilterable = value;
					if (this.ColumnLayout != null)
					{
						this.ColumnLayout.Grid.ExitEditModeInternal(true);
						this.ColumnLayout.Grid.InvalidateScrollPanel(false);
					}
				}
			}
		}

		#endregion // IsFilterable

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
                if (this.SupportsActivationAndSelection)
                {
                    if (this.ColumnLayout != null)
                    {
                        if (value)
                            this.ColumnLayout.Grid.SelectColumn(this, InvokeAction.Code);
                        else
                            this.ColumnLayout.Grid.UnselectColumn(this);
                    }
                    else
                        this._isSelected = value;
                }
			}

		}
		#endregion // IsSelected

		#region IsSorted
		/// <summary>
		/// Gets / sets how the data in this column are sorted.
		/// </summary>
		public virtual SortDirection IsSorted
		{
			get
			{
				return this._isSorted;
			}
			set
			{
				if (this._isSorted != value)
				{
					this.SetSortedColumnState(value);
				}
			}
		}

		#endregion // IsSorted

		#region IsSummable

		/// <summary>
		/// Gets / sets if the column will show the UI for SummaryRow.
		/// </summary>
		public virtual bool IsSummable
		{
			get
			{
				return this._isSummable;
			}
			set
			{
				if (_isSummable != value)
				{
					_isSummable = value;
					if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
						this.ColumnLayout.Grid.InvalidateScrollPanel(false);
				}
			}
		}
		#endregion // IsSummable

        #region IsGroupable

        /// <summary>
        /// Gets/sets whether a user can group their data by this <see cref="Column"/>
        /// </summary>
        public virtual bool IsGroupable
        {
            get 
            {
                //if (this.ParentColumn != null && this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                //    return false;

                return this._isGroupable; 
            }
            set
            {
                if (this._isGroupable != value)
                {
                    this._isGroupable = value;                   
                }
            }
        }

        #endregion // IsGroupable

		#region IsGroupBy

		/// <summary>
		/// Gets/Sets whether the data that this <see cref="Column"/> represents should be grouped by this column.
		/// </summary>
		public bool IsGroupBy
		{
			get { return this._isGroupBy; }
			set
			{
                if (this._isGroupBy != value && this.CanBeGroupedBy)
                {
                    if (this.ColumnLayout != null)
                    {
                        if (value)
                            this.ColumnLayout.Grid.GroupBySettings.GroupByColumns.Add(this);
                        else
                        {
                            // if we're ungrouping, we may be in edit mode. 
                            // If we're in edit mode, we should just kill edit mode, b/c we might be in an error state
                            // so just cancel it and move on. 
                            ColumnLayout cl = this.ColumnLayout;
                            if (cl != null && cl.Grid != null)
                            {
                                if (cl.Grid.CurrentEditCell != null || cl.Grid.CurrentEditRow != null)
                                {
                                    cl.Grid.ExitEditMode(true);
                                }
                            }
                            this.ColumnLayout.Grid.GroupBySettings.GroupByColumns.Remove(this);
                        }
                    }
                    else
                        this._isGroupBy = value;
                }
			}
		}
		#endregion // IsGroupBy

		#region ValueConverter

		/// <summary>
		/// Identifies the <see cref="ValueConverter"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ValueConverterProperty = DependencyProperty.Register("ValueConverter", typeof(IValueConverter), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(ValueConverterChanged)));

		/// <summary>
        /// Gets/sets the <see cref="IValueConverter"/> that will be used to display the value in the column.
		/// </summary>
        /// <remarks>
        /// For editing purposes the <see cref="EditableColumn.EditorValueConverter"/> is used.  This <see cref="IValueConverter"/> is only used for display.
        /// </remarks>
		public IValueConverter ValueConverter
		{
			get { return (IValueConverter)this.GetValue(ValueConverterProperty); }
			set { this.SetValue(ValueConverterProperty, value); }
		}

		private static void ValueConverterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			Column col = (Column)obj;
			col.OnPropertyChanged("ValueConverter");
            if (col.ColumnLayout != null && col.ColumnLayout.Grid != null)
            {
                col.ColumnLayout.Grid.ResetPanelRows();
            }
		}

		#endregion // ValueConverter

		#region ValueConverterParameter

		/// <summary>
		/// Identifies the <see cref="ValueConverterParameter"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ValueConverterParameterProperty = DependencyProperty.Register("ValueConverterParameter", typeof(object), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(ValueConverterParameterChanged)));

		/// <summary>
		/// Gets/sets the parameter that will be used with the <see cref="ValueConverter"/> property.
		/// </summary>
		public object ValueConverterParameter
		{
			get { return (object)this.GetValue(ValueConverterParameterProperty); }
			set { this.SetValue(ValueConverterParameterProperty, value); }
		}

		private static void ValueConverterParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			Column col = (Column)obj;
			col.OnPropertyChanged("ValueConverterParameter");
		}

		#endregion // ValueConverterParameter

		#region GroupByItemTemplate

		/// <summary>
		/// Identifies the <see cref="GroupByItemTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByItemTemplateProperty = DependencyProperty.Register("GroupByItemTemplate", typeof(DataTemplate), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(GroupByItemTemplateChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="DataTemplate"/> that will be used in a <see cref="GroupByCell"/> when a column is grouped.
		/// </summary>
		/// <remarks>
		/// The DataContext of the <see cref="GroupByCell"/> will be of type <see cref="GroupByDataContext"/>.
		/// </remarks>
		public DataTemplate GroupByItemTemplate
		{
			get { return (DataTemplate)this.GetValue(GroupByItemTemplateProperty); }
			set { this.SetValue(GroupByItemTemplateProperty, value); }
		}

		private static void GroupByItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			Column col = (Column)obj;

			if (col.ColumnLayout != null && col.ColumnLayout.Grid != null)
				col.ColumnLayout.Grid.ResetPanelRows(true);

			col.OnPropertyChanged("GroupByItemTemplate");
		}

		#endregion // GroupByItemTemplate

		#region GroupByComparer
		/// <summary>
		/// Gets/sets a custom <see cref="IEqualityComparer"/> template object which will be used when grouping data.
		/// </summary>
		public object GroupByComparer
		{
			get
			{
				return this._groupByComparer;
			}
			set
			{
				if (this._groupByComparer != value)
				{
					this._groupByComparer = value;

					this.ValidateGroupByComparer();

					this.OnPropertyChanged("GroupByComparer");

					if (this.ColumnLayout != null)
						this.ColumnLayout.InvalidateGroupByReset();
				}
			}
		}
		#endregion // SortComparer

		#region SummaryColumnSettings

		/// <summary>
		/// Gets / Sets the <see cref="SummaryColumnSettings"/> which will control behavior for this <see cref="Column"/>.
		/// </summary>
		public SummaryColumnSettings SummaryColumnSettings
		{
			get
			{
				if (this._summaryColumnSettings == null)
				{
					this._summaryColumnSettings = new SummaryColumnSettings(this);
					this._summaryColumnSettings.PropertyChanged += SummaryColumnSettings_PropertyChanged;
				}
				return this._summaryColumnSettings;
			}
			set
			{
				if (this._summaryColumnSettings != value)
				{
					if (this._summaryColumnSettings != null)
					{

						this._summaryColumnSettings.PropertyChanged -= SummaryColumnSettings_PropertyChanged;
					}

					this._summaryColumnSettings = value;

					if (value != null)
					{
						this._summaryColumnSettings.Column = this;
						if (value != null)
						{
							this._summaryColumnSettings.PropertyChanged += SummaryColumnSettings_PropertyChanged;
						}
					}
				}
			}
		}



		#endregion // SummaryColumnSettings

		#region FilterColumnSettings

		/// <summary>
		/// Gets / Sets the <see cref="FilterColumnSettings"/> which will control behavior for this <see cref="Column"/>.
		/// </summary>
		public FilterColumnSettings FilterColumnSettings
		{
			get
			{
				if (this._filterColumnSettings == null)
				{
					this._filterColumnSettings = new FilterColumnSettings(this);
				}
				return this._filterColumnSettings;
			}
			set
			{
				if (this._filterColumnSettings != value)
				{
					this._filterColumnSettings = value;

					if (value != null)
					{
						this._filterColumnSettings.Column = this;
					}
				}
			}
		}

		#endregion // FilterColumnSettings

		#region FirstSortDirection

		/// <summary>
		/// Identifies the <see cref="FirstSortDirection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FirstSortDirectionProperty = DependencyProperty.Register("DefaultSort", typeof(SortDirection?), typeof(Column), new PropertyMetadata(null, new PropertyChangedCallback(FirstSortDirectionChanged)));

		/// <summary>
		/// Gets/Sets DefaultSort for a particular <see cref="Column"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<SortDirection>))]
		public SortDirection? FirstSortDirection
		{
			get { return (SortDirection?)this.GetValue(FirstSortDirectionProperty); }
			set { this.SetValue(FirstSortDirectionProperty, value); }
		}

		private static void FirstSortDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			Column settings = (Column)obj;
			settings.OnPropertyChanged("FirstSortDirection");
		}

		#endregion // FirstSortDirection

		#region FirstSortDirectionResolved

		/// <summary>
		/// Resolves the <see cref="SortingSettingsOverride.FirstSortDirection"/> property for a particular <see cref="Column"/>
		/// </summary>
		public SortDirection FirstSortDirectionResolved
		{
			get
			{
				if (this.FirstSortDirection == null)
				{
					if (this.ColumnLayout != null)
						return this.ColumnLayout.SortingSettings.FirstSortDirectionResolved;
				}
				else
					return (SortDirection)this.FirstSortDirection;

				return SortDirection.Ascending;
			}
		}

		#endregion //FirstSortDirectionResolved

        #region HeaderTextHorizontalAlignment

        /// <summary>
        /// Identifies the <see cref="HeaderTextHorizontalAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTextHorizontalAlignmentProperty = DependencyProperty.Register("HeaderTextHorizontalAlignment", typeof(HorizontalAlignment?), typeof(Column), new PropertyMetadata(null, new PropertyChangedCallback(HeaderTextHorizontalAlignmentChanged)));

        /// <summary>
        /// Gets/sets  the <see cref="HorizontalAlignment"/> of the content for this <see cref="Column"/>.
        /// </summary>
        [TypeConverter(typeof(NullableEnumTypeConverter<HorizontalAlignment>))]
        public HorizontalAlignment? HeaderTextHorizontalAlignment
        {
            get { return (HorizontalAlignment?)this.GetValue(HeaderTextHorizontalAlignmentProperty); }
            set { this.SetValue(HeaderTextHorizontalAlignmentProperty, value); }
        }

        private static void HeaderTextHorizontalAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("HeaderTextHorizontalAlignment");
        }

        #endregion // HeaderTextHorizontalAlignment

        #region HeaderTextHorizontalAlignmentResolved

        /// <summary>
        /// Gets the actual <see cref="HorizontalAlignment"/> of the content for this <see cref="Column"/>.
        /// </summary>
        public HorizontalAlignment HeaderTextHorizontalAlignmentResolved
        {
            get
            {
                if (this.HeaderTextHorizontalAlignment == null)
                {
                    if (this.ColumnLayout != null)
                    {
                        return this.ColumnLayout.HeaderTextHorizontalAlignmentResolved;
                    }
                    else
                    {
                        return (HorizontalAlignment)XamGrid.HeaderTextHorizontalAlignmentProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
                    }
                }
                else
                {
                    return (HorizontalAlignment)this.HeaderTextHorizontalAlignment;
                }
            }
        }

        #endregion // HeaderTextHorizontalAlignmentResolved

        #region HeaderTextVerticalAlignment

        /// <summary>
        /// Identifies the <see cref="HeaderTextVerticalAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTextVerticalAlignmentProperty = DependencyProperty.Register("HeaderTextVerticalAlignment", typeof(VerticalAlignment?), typeof(Column), new PropertyMetadata(null, new PropertyChangedCallback(HeaderTextVerticalAlignmentChanged)));

        /// <summary>
        /// Gets/sets  the <see cref="VerticalAlignment"/> of the content for this <see cref="Column"/>.
        /// </summary>
        [TypeConverter(typeof(NullableEnumTypeConverter<VerticalAlignment>))]
        public VerticalAlignment? HeaderTextVerticalAlignment
        {
            get { return (VerticalAlignment?)this.GetValue(HeaderTextVerticalAlignmentProperty); }
            set { this.SetValue(HeaderTextVerticalAlignmentProperty, value); }
        }

        private static void HeaderTextVerticalAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("HeaderTextVerticalAlignment");
        }

        #endregion // HeaderTextVerticalAlignment

        #region HeaderTextVerticalAlignmentResolved

        /// <summary>
        /// Gets the actual <see cref="VerticalAlignment"/> of the content for this <see cref="Column"/>.
        /// </summary>
        public VerticalAlignment HeaderTextVerticalAlignmentResolved
        {
            get
            {
                if (this.HeaderTextVerticalAlignment == null)
                {
                    if (this.ColumnLayout != null)
                    {
                        return this.ColumnLayout.HeaderTextVerticalAlignmentResolved;
                    }
                    else
                    {
                        return (VerticalAlignment)XamGrid.HeaderTextVerticalAlignmentProperty.GetMetadata(typeof(XamGrid)).DefaultValue;
                    }
                }
                else
                {
                    return (VerticalAlignment)this.HeaderTextVerticalAlignment;
                }
            }
        }

        #endregion // HeaderTextVerticalAlignmentResolved

		#region ConditionalFormatCollection
      
		/// <summary>
		/// Get / set the <see cref="ConditionalFormatCollection"/> which contains the formatting rules to be applied to this <see cref="Column"/>.
		/// </summary>
        [Browsable(false)]
        public ConditionalFormatCollection ConditionalFormatCollection
		{
			get
			{
				if (this._conditionalFormatCollection == null)
				{
					this._conditionalFormatCollection = new ConditionalFormatCollection();
					this._conditionalFormatCollection.CollectionChanged += ConditionalFormatCollection_CollectionChanged;
					this._conditionalFormatCollection.CollectionItemChanged += ConditionalFormatCollection_CollectionItemChanged;
					this._conditionalFormatCollection.Column = this;
				}

				return this._conditionalFormatCollection;
			}
			set
			{
				if (this._conditionalFormatCollection != null)
				{
					this._conditionalFormatCollection.CollectionChanged -= ConditionalFormatCollection_CollectionChanged;
					this._conditionalFormatCollection.CollectionItemChanged -= ConditionalFormatCollection_CollectionItemChanged;
					this._conditionalFormatCollection.Column = null;
				}

				this._conditionalFormatCollection = value;

				if (this._conditionalFormatCollection != null)
				{
					this._conditionalFormatCollection.CollectionChanged += ConditionalFormatCollection_CollectionChanged;
					this._conditionalFormatCollection.CollectionItemChanged += ConditionalFormatCollection_CollectionItemChanged;
					this._conditionalFormatCollection.Column = this;
				}
			}
		}

		#endregion // ConditionalFormatCollection

        #region ToolTipStyle

        /// <summary>
        /// Identifies the <see cref="ToolTipStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ToolTipStyleProperty = DependencyProperty.Register("ToolTipStyle", typeof(Style), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(ToolTipStyleChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> of the <see cref="ToolTip"/> object that is displayed for this particular <see cref="Column"/>
        /// </summary>
        public Style ToolTipStyle
        {
            get { return (Style)this.GetValue(ToolTipStyleProperty); }
            set { this.SetValue(ToolTipStyleProperty, value); }
        }

        private static void ToolTipStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("ToolTipStyle");
        }

        #endregion // ToolTipStyle 

        #region TooltipContentTemplate

        /// <summary>
        /// Identifies the <see cref="ToolTipContentTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ToolTipContentTemplateProperty = DependencyProperty.Register("TooltipContentTemplate", typeof(DataTemplate), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(ToolTipContentTemplateChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="DataTemplate"/> that should be used to display the content of a <see cref="Column"/>'s tooltip.
        /// </summary>
        /// <remarks>
        /// The DataContext of the tooltip is the particualr Row's Data.
        /// </remarks>
        public DataTemplate ToolTipContentTemplate
        {
            get { return (DataTemplate)this.GetValue(ToolTipContentTemplateProperty); }
            set { this.SetValue(ToolTipContentTemplateProperty, value); }
        }

        private static void ToolTipContentTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("TooltipContentTemplate");
        }

        #endregion // TooltipContentTemplate 
				
        #region AllowToolTips

        /// <summary>
        /// Identifies the <see cref="AllowToolTips"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowToolTipsProperty = DependencyProperty.Register("AllowToolTips", typeof(AllowToolTips), typeof(Column), new PropertyMetadata(AllowToolTips.Never, new PropertyChangedCallback(AllowToolTipsChanged)));

        /// <summary>
        /// Gets/Sets when a <see cref="ToolTip"/> should be displayed for every <see cref="Cell"/> in this <see cref="Column"/>
        /// </summary>
        public AllowToolTips AllowToolTips
        {
            get { return (AllowToolTips)this.GetValue(AllowToolTipsProperty); }
            set { this.SetValue(AllowToolTipsProperty, value); }
        }

        private static void AllowToolTipsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.CachedAllowToolTips = (AllowToolTips)e.NewValue;
            col.OnPropertyChanged("AllowToolTips");
        }

        #endregion // AllowToolTips 

        #region AllColumns

        /// <summary>
        /// Gets a ReadOnly collection of all <see cref="Column"/> objects that belong to a particular <see cref="Column"/>. 
        /// </summary>
        /// <remarks>
        /// This includes Columns that are children of other Columns. 
        /// </remarks>
        [Browsable(false)]
        public virtual ReadOnlyKeyedColumnBaseCollection<Column> AllColumns
        {
            get
            {
                return new ReadOnlyKeyedColumnBaseCollection<Column>(new List<Column>());
            }
        }

        #endregion // AllColumns

        #region AllVisibleChildColumns

        /// <summary>
        /// Gets a ReadOnly collection of all visible <see cref="Column"/> objects that have no children columns.
        /// </summary>
        /// <remarks>
        /// This includes Columns that are children of other Columns. 
        /// </remarks>
        [Browsable(false)]
        public virtual ReadOnlyKeyedColumnBaseCollection<Column> AllVisibleChildColumns
        {
            get
            {
                return new ReadOnlyKeyedColumnBaseCollection<Column>(new List<Column>());
            }
        }
        #endregion // AllVisibleChildColumns

        #region AddNewItemTemplate

        /// <summary>
        /// Identifies the <see cref="AddNewRowItemTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AddNewRowItemTemplateProperty = DependencyProperty.Register("AddNewRowItemTemplate", typeof(DataTemplate), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(AddNewRowItemTemplateChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> that will be used to generate content for the <see cref="CellBase"/> of the <see cref="EditableColumn"/> in the <see cref="AddNewRowCell"/>.
        /// </summary>
        /// <remarks>
        /// If this property is null then the normal column content is used.
        /// </remarks>
        public DataTemplate AddNewRowItemTemplate
        {
            get { return (DataTemplate)this.GetValue(AddNewRowItemTemplateProperty); }
            set { this.SetValue(AddNewRowItemTemplateProperty, value); }
        }

        private static void AddNewRowItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("AddNewRowItemTemplate");
            if (col.ColumnLayout != null && col.ColumnLayout.Grid != null)
            {
                col.ColumnLayout.Grid.ResetPanelRows(true);
            }
        }

        #endregion // AddNewItemTemplate

        #region AddNewEditorTemplate

        /// <summary>
        /// Identifies the <see cref="AddNewRowEditorTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AddNewRowEditorTemplateProperty = DependencyProperty.Register("AddNewRowEditorTemplate", typeof(DataTemplate), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(AddNewRowEditorTemplateChanged)));

        /// <summary>
        /// Gets / sets the Editor that will be displayed when this <see cref="AddNewRowCell"/> in this <see cref="EditableColumn"/> is in edit mode.
        /// </summary>
        /// <remarks>
        /// If this property is null then the normal editor content is used.        
        /// </remarks>
        public DataTemplate AddNewRowEditorTemplate
        {
            get { return (DataTemplate)this.GetValue(AddNewRowEditorTemplateProperty); }
            set { this.SetValue(AddNewRowEditorTemplateProperty, value); }
        }

        private static void AddNewRowEditorTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("AddNewRowEditorTemplate");
            if (col.ColumnLayout != null && col.ColumnLayout.Grid != null)
            {
                col.ColumnLayout.Grid.ResetPanelRows(true);
            }
        }

        #endregion // AddNewEditorTemplate 

        #region AddNewRowItemTemplateHorizontalContentAlignment

        /// <summary>
        /// Identifies the <see cref="AddNewRowItemTemplateHorizontalContentAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AddNewRowItemTemplateHorizontalContentAlignmentProperty = DependencyProperty.Register("AddNewRowItemTemplateHorizontalContentAlignment", typeof(HorizontalAlignment), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(AddNewRowItemTemplateHorizontalContentAlignmentChanged)));

        /// <summary>
        /// Gets / sets the <see cref="HorizontalAlignment"/> which will be assigned to the content of the <see cref="AddNewRowItemTemplate" />.
        /// </summary>
        public HorizontalAlignment AddNewRowItemTemplateHorizontalContentAlignment
        {
            get { return (HorizontalAlignment)this.GetValue(AddNewRowItemTemplateHorizontalContentAlignmentProperty); }
            set { this.SetValue(AddNewRowItemTemplateHorizontalContentAlignmentProperty, value); }
        }

        private static void AddNewRowItemTemplateHorizontalContentAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("AddNewRowItemTemplateHorizontalContentAlignment");
            if (col.ColumnLayout != null && col.ColumnLayout.Grid != null)
            {
                col.ColumnLayout.Grid.ResetPanelRows(true);
            }
        }

        #endregion // AddNewRowItemTemplateHorizontalContentAlignment 
				
        #region AddNewRowItemTemplateVerticalContentAlignment

        /// <summary>
        /// Identifies the <see cref="AddNewRowItemTemplateVerticalContentAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AddNewRowItemTemplateVerticalContentAlignmentProperty = DependencyProperty.Register("AddNewRowItemTemplateVerticalContentAlignment", typeof(VerticalAlignment), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(AddNewRowItemTemplateVerticalContentAlignmentChanged)));

        /// <summary>
        /// Gets / sets the <see cref="VerticalAlignment"/> which will be assigned to the content of the <see cref="AddNewRowItemTemplate" />.
        /// </summary>
        public VerticalAlignment AddNewRowItemTemplateVerticalContentAlignment
        {
            get { return (VerticalAlignment)this.GetValue(AddNewRowItemTemplateVerticalContentAlignmentProperty); }
            set { this.SetValue(AddNewRowItemTemplateVerticalContentAlignmentProperty, value); }
        }

        private static void AddNewRowItemTemplateVerticalContentAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;
            col.OnPropertyChanged("AddNewRowItemTemplateVerticalContentAlignment");
            if (col.ColumnLayout != null && col.ColumnLayout.Grid != null)
            {
                col.ColumnLayout.Grid.ResetPanelRows(true);
            }
        }

        #endregion // AddNewRowItemTemplateVerticalContentAlignment 					
	
        #region AddNewRowEditorTemplateVerticalContentAlignment

        /// <summary>
        /// Identifies the <see cref="AddNewRowEditorTemplateVerticalContentAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AddNewRowEditorTemplateVerticalContentAlignmentProperty = DependencyProperty.Register("AddNewRowEditorTemplateVerticalContentAlignment", typeof(VerticalAlignment), typeof(Column), new PropertyMetadata(VerticalAlignment.Stretch, new PropertyChangedCallback(AddNewRowEditorTemplateVerticalContentAlignmentChanged)));

        /// <summary>
        /// Gets / sets the <see cref="VerticalAlignment"/> which will be assigned to the content of the <see cref="AddNewRowEditorTemplate" />.
        /// </summary>
        public VerticalAlignment AddNewRowEditorTemplateVerticalContentAlignment
        {
            get { return (VerticalAlignment)this.GetValue(AddNewRowEditorTemplateVerticalContentAlignmentProperty); }
            set { this.SetValue(AddNewRowEditorTemplateVerticalContentAlignmentProperty, value); }
        }

        private static void AddNewRowEditorTemplateVerticalContentAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column ctrl = (Column)obj;
            ctrl.OnPropertyChanged("AddNewRowEditorTemplateVerticalContentAlignment");
        }

        #endregion // AddNewRowEditorTemplateVerticalContentAlignment 
	
        #region AddNewRowEditorTemplateHorizontalContentAlignment

        /// <summary>
        /// Identifies the <see cref="AddNewRowEditorTemplateHorizontalContentAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AddNewRowEditorTemplateHorizontalContentAlignmentProperty = DependencyProperty.Register("AddNewRowEditorTemplateHorizontalContentAlignment", typeof(HorizontalAlignment), typeof(Column), new PropertyMetadata(HorizontalAlignment.Stretch, new PropertyChangedCallback(AddNewRowEditorTemplateHorizontalContentAlignmentChanged)));

        /// <summary>
        /// Gets / sets the <see cref="HorizontalAlignment"/> which will be assigned to the content of the <see cref="AddNewRowEditorTemplate" />.
        /// </summary>
        public HorizontalAlignment AddNewRowEditorTemplateHorizontalContentAlignment
        {
            get { return (HorizontalAlignment)this.GetValue(AddNewRowEditorTemplateHorizontalContentAlignmentProperty); }
            set { this.SetValue(AddNewRowEditorTemplateHorizontalContentAlignmentProperty, value); }
        }

        private static void AddNewRowEditorTemplateHorizontalContentAlignmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column ctrl = (Column)obj;
            ctrl.OnPropertyChanged("AddNewRowEditorTemplateHorizontalContentAlignment");
        }

        #endregion // AddNewRowEditorTemplateHorizontalContentAlignment 	

        #region MergedItemTemplate

        /// <summary>
        /// Identifies the <see cref="MergedItemTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MergedItemTemplateProperty = DependencyProperty.Register("MergedItemTemplate", typeof(DataTemplate), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(MergedItemTemplateChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="DataTemplate"/> that will be used in a <see cref="MergedContentControl"/> when a column is merged.
        /// </summary>
        /// <remarks>
        /// The DataContext of the <see cref="MergedContentControl"/> will be of type <see cref="MergeDataContext"/>.
        /// </remarks>
        public DataTemplate MergedItemTemplate
        {
            get { return (DataTemplate)this.GetValue(MergedItemTemplateProperty); }
            set { this.SetValue(MergedItemTemplateProperty, value); }
        }

        private static void MergedItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column col = (Column)obj;

            if (col.ColumnLayout != null && col.ColumnLayout.Grid != null)
                col.ColumnLayout.Grid.ResetPanelRows();

            col.OnPropertyChanged("MergedItemTemplate");
        }

        #endregion // MergedItemTemplate 

        #region AddNewRowCellStyle

        /// <summary>
        /// Identifies the <see cref="AddNewRowCellStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AddNewRowCellStyleProperty = DependencyProperty.Register("AddNewRowCellStyle", typeof(Style), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(AddNewRowCellStyleChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Style"/> which will be applied to the <see cref="Cell"/> on the <see cref="AddNewRow"/>.
        /// </summary>
        public Style AddNewRowCellStyle
        {
            get { return (Style)this.GetValue(AddNewRowCellStyleProperty); }
            set { this.SetValue(AddNewRowCellStyleProperty, value); }
        }

        private static void AddNewRowCellStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column ctrl = (Column)obj;
            ctrl.OnStyleChanged();
            ctrl.OnPropertyChanged("AddNewRowCellStyle");
        }

        #endregion // AddNewRowCellStyle 

        #region GroupByRowCellStyle

        /// <summary>
        /// Identifies the <see cref="GroupByRowStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty GroupByRowCellStyleProperty = DependencyProperty.Register("GroupByRowCellStyle", typeof(Style), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(GroupByRowCellStyleChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Style"/> which will be applied to the <see cref="Cell"/> on the <see cref="GroupByRow"/>.
        /// </summary>
        /// <remarks>
        /// This style should target a <see cref="GroupByCellControl"/>.
        /// </remarks>
        public Style GroupByRowStyle
        {
            get { return (Style)this.GetValue(GroupByRowCellStyleProperty); }
            set { this.SetValue(GroupByRowCellStyleProperty, value); }
        }

        private static void GroupByRowCellStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column ctrl = (Column)obj;
            ctrl.OnStyleChanged();
            ctrl.OnPropertyChanged("GroupByRowCellStyle");
        }

        #endregion // GroupByRowCellStyle 

        #region GroupByHeaderCellStyle

        /// <summary>
        /// Identifies the <see cref="GroupByHeaderStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty GroupByHeaderCellStyleProperty = DependencyProperty.Register("GroupByHeaderCellStyle", typeof(Style), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(GroupByHeaderCellStyleChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Style"/> which will be applied to the <see cref="Cell"/> on the <see cref="GroupByRow"/>.
        /// </summary>
        /// <remarks>
        /// This style should target a <see cref="GroupByHeaderCellControl"/>.
        /// </remarks>
        public Style GroupByHeaderStyle
        {
            get { return (Style)this.GetValue(GroupByHeaderCellStyleProperty); }
            set { this.SetValue(GroupByHeaderCellStyleProperty, value); }
        }

        private static void GroupByHeaderCellStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column ctrl = (Column)obj;
            ctrl.OnStyleChanged();
            ctrl.OnPropertyChanged("GroupByHeaderCellStyle");
        }

        #endregion // GroupByHeaderCellStyle 

        #region MergeCellStyle

        /// <summary>
        /// Identifies the <see cref="MergeCellStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MergeCellStyleProperty = DependencyProperty.Register("MergeCellStyle", typeof(Style), typeof(Column), new PropertyMetadata(new PropertyChangedCallback(MergeCellStyleChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Style"/> which will be applied to the <see cref="Cell"/> for a merge cell.
        /// </summary>
        public Style MergeCellStyle
        {
            get { return (Style)this.GetValue(MergeCellStyleProperty); }
            set { this.SetValue(MergeCellStyleProperty, value); }
        }

        private static void MergeCellStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Column ctrl = (Column)obj;
            ctrl.OnStyleChanged();
            ctrl.OnPropertyChanged("MergeCellStyle");            
        }

        #endregion // MergeCellStyle 

        #endregion // Public

        #region Internal

        internal double PercentMoved
		{
			get;
			set;
		}

		internal double MovingColumnsWidth
		{
			get;
			set;
		}		

		internal bool ReverseMove
		{
			get;
			set;
		}

		internal bool IsInitialAutoSet
		{
			get;
			set;
		}

        internal string TypeName
        {
            get
            {
                if (this._cachedTypeName == null)
                    this._cachedTypeName = this.GetType().Name;

                return this._cachedTypeName;
            }
        }

        internal bool IsDragging
        {
            get;
            set;
        }

        #region CachedAllowTooltips

        internal AllowToolTips CachedAllowToolTips
	    {
	        get;
	        private set;
	    }

        #endregion // CachedAllowTooltips

        #region CachedPropertyReadOnly

        internal bool? CachedPropertyReadOnly
	    {
	        get;
	        set;
	    }

        #endregion // CachedPropertyReadOnly

        #region CachedClipboardIndex

        internal int CachedClipboardIndex
        {
            get;
            set;
        }

        #endregion // CachedClipboardIndex

	    #region CachedParentColumn

        /// <summary>
        /// CachedParentColumn returns the parent column stored off when a child column is temporary removed/moved
        /// when grouping (with <see cref="GroupBySettings.GroupByOperation"/> <see cref="GroupByOperation.MergeCells"/>)
        /// </summary>
	    internal Column CachedParentColumn
	    {
	        get { return this._cachedParentColumn; }
	    }

	    #endregion // CachedParentColumn

		#endregion // Internal

		#region Protected

		#region SupportsHeaderFooterContent

		/// <summary>
		/// Gets whether a <see cref="HeaderCell"/> or <see cref="FooterCell"/> are allowed to display
		/// content that's not directly set on them, such as the <see cref="XamGrid.ColumnsHeaderTemplate"/> property.
		/// </summary>
		protected internal virtual bool SupportsHeaderFooterContent
		{
			get { return true; }
		}

		#endregion // SupportsHeaderFooterContent

		#region IsEditable

		/// <summary>
		/// Resolves whether this <see cref="Column"/> supports editing.
		/// </summary>
		protected internal virtual bool IsEditable
		{
			get { return false; }
		}

		#endregion // IsEditable

		#region UniqueColumnContent

		/// <summary>
		/// Resolves whether <see cref="Cell"/> objects can be recycled and shared amongst columns of the same type.
		/// </summary>
		/// <remarks>
		/// Note: If false, then the Cells generated by this Column's ContentProvider will be 
		/// shared between other column's of this specific type.
		/// </remarks>
		protected internal virtual bool UniqueColumnContent
		{
			get { return false; }
		}

		#endregion // UniqueColumnContent

		#region DefaultFilterOperand

		/// <summary>
		/// The default <see cref="FilterOperand"/> for this column type;
		/// </summary>
		protected internal virtual FilterOperand DefaultFilterOperand
		{
			get
			{
				if (this._defaultFilterOperand == null)
				{
					this._defaultFilterOperand = new EqualsOperand();
				}
				return this._defaultFilterOperand;
			}
		}

		#endregion // DefaultFilterOperand

        #region AllowCellEditorValueChangedFiltering

        /// <summary>
        /// Gets a value indicating whether filtering will be immediately applied after the value of the cell editor is changed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a filter should be applied immediately after the value of the cell editor is changed; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Designed to be used by <see cref="Column"/>s which need filtering while the editor control is being edited.
        /// </remarks>
        protected internal virtual bool AllowCellEditorValueChangedFiltering
        {
            get
            {
                return this.DataType == typeof(string);
            }
        }

        #endregion // AllowCellEditorValueChangedFiltering

		#region FormatStringResolved

		/// <summary>
		/// A format string for formatting data in this column.
		/// </summary>
		protected internal virtual string FormatStringResolved
		{
			get
			{
				return null;
			}
		}

		#endregion // FormatStringResolved

        #region ParentColumn

        /// <summary>
        /// Gets/Sets the <see cref="Column"/> that owns this particular column.
        /// </summary>
        /// <remarks>If a Column is not a direct child of a Column, then this property will return null.</remarks>
        protected internal Column ParentColumn
        {
            get;
            set;
        }

        #endregion // ParentColumn

        #region ResizeColumnResolved

        /// <summary>
        /// Resolves the <see cref="Column"/> that should be resized when dragging the right edge of this Column.
        /// </summary>
        protected internal virtual Column ResizeColumnResolved
        {
            get
            {
                return this;
            }
        }

        #endregion // ResizeColumnResolved

        #region SupportsActivationAndSelection

        /// <summary>
        /// Determines if a <see cref="Cell"/> in this particular Column can be Selected or Activated. 
        /// </summary>
        protected internal virtual bool SupportsActivationAndSelection
        {
            get
            {
                return true;
            }
        }

        #endregion // SupportsActivationAndSelection

        #region CanBeGroupedBy

        /// <summary>
        /// Determines if a <see cref="Column"/> can be grouped by.
        /// </summary>
        protected internal virtual bool CanBeGroupedBy
        {
            get
            {
                return true;
            }
        }

        #endregion // CanBeGroupedBy

        #region CanBeSorted

        /// <summary>
        /// Determines if a <see cref="Column"/> can be Sorted.
        /// </summary>
        protected internal virtual bool CanBeSorted
        {
            get
            {
                return true;
            }
        }

        #endregion // CanBeSorted

        #region CanBeFiltered

        /// <summary>
        /// Determines if a <see cref="Column"/> can be Filtered.
        /// </summary>
        protected internal virtual bool CanBeFiltered
        {
            get
            {
                return true;
            }
        }

        #endregion // CanBeFiltered

        #region UseReadOnlyFlag

        /// <summary>
        /// Determines if the <see cref="Column"/> should use the ReadOnly flag on a property, to determine if it can enter edit mode.
        /// </summary>
        protected internal virtual bool UseReadOnlyFlag
        {
            get { return true; }
        }

        #endregion // UseReadOnlyFlag

        #region ResetCellValueObjectAfterEditing

        /// <summary>
        /// Gets if the <see cref="Column"/> should reset the CellValueObject when exiting edit mode.
        /// </summary>
        protected internal virtual bool ResetCellValueObjectAfterEditing
        {
            get
            {
                return true;
            }
        }

        #endregion // ResetCellValueObjectAfterEditing

        #region FilterMenuCustomFilterString
        /// <summary>
        /// Gets the default string for the FilterMenu for the CustomFilter text
        /// </summary>
        protected virtual string FilterMenuCustomFilterString
        {
            get
            {
                return SRGrid.GetString("CustomFiltersWithoutType");
            }
        }
        #endregion // FilterMenuCustomFilterString


        #endregion // Protected

        #endregion // Properties

        #region Methods

        #region Protected

        #region SetSelected
        /// <summary>
		/// Sets the selected state of an item. 
		/// </summary>
		/// <param propertyName="isSelected"></param>
		protected internal virtual void SetSelected(bool isSelected)
		{
			this._isSelected = isSelected;
			if (this.ColumnLayout != null)
				this.ColumnLayout.Grid.InvalidateScrollPanel(false);
		}
		#endregion // SetSelected

		#region GenerateCell

		/// <summary>
		/// Based on the type of row that is passed in, this method generates a new <see cref="CellBase"/> object.
		/// </summary>
		/// <param propertyName="row">The row in which the cell should be created for.</param>
		/// <returns></returns>
		protected internal virtual CellBase GenerateCell(RowBase row)
		{
			if (row is HeaderRow)
				return this.GenerateHeaderCell(row);
			else if (row is FooterRow)
				return this.GenerateFooterCell(row);
			else
				return this.GenerateDataCell(row);
		}

		#endregion // GenerateCell

		#region GenerateHeaderCell

		/// <summary>
		/// Returns a new instance of a <see cref="HeaderCell"/>
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected virtual CellBase GenerateHeaderCell(RowBase row)
		{
			return new HeaderCell(row, this);
		}

		#endregion // GenerateHeaderCell

		#region GenerateFooterCell

		/// <summary>
		/// Returns a new instance of a <see cref="FooterCell"/>
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected virtual CellBase GenerateFooterCell(RowBase row)
		{
			return new FooterCell(row, this);
		}

		#endregion // GenerateFooterCell

		#region GenerateDataCell

		/// <summary>
		/// Returns a new instance of a <see cref="Cell"/>
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected virtual CellBase GenerateDataCell(RowBase row)
		{
			if (row.RowType == RowType.AddNewRow)
				return new AddNewRowCell(row, this);

			if (row.RowType == RowType.FilterRow)
				return new FilterRowCell(row, this);

			if (row.RowType == RowType.SummaryRow)
				return new SummaryRowCell(row, this);

            if (row.RowType == RowType.MergedSummaryRow)
                return new MergedSummaryCell(row, this);

			if (this.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
				return new ConditionalFormattingCell(row, this);

			return new Cell(row, this);
		}

		#endregion // GenerateDataCell

		#region GenerateContentProvider

		/// <summary>
		/// Generates a new <see cref="ColumnContentProviderBase"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		protected internal abstract ColumnContentProviderBase GenerateContentProvider();

		#endregion // GenerateContentProvider

		#region FillAvailableFilters

		/// <summary>
		/// Fills the <see cref="FilterOperandCollection"/> with the operands that the column expects as filter values.
		/// </summary>
		/// <param name="availableFilters"></param>
		protected internal virtual void FillAvailableFilters(FilterOperandCollection availableFilters)
		{
			ColumnLayout columnLayout = this.ColumnLayout;
			if (columnLayout != null)
			{
				XamGrid grid = columnLayout.Grid;

				if (grid != null)
				{
					availableFilters.Add(new EqualsOperand() { XamWebGrid = grid });
					availableFilters.Add(new NotEqualsOperand() { XamWebGrid = grid });
				}
			}
		}

		#endregion // FillAvailableFilters

		#region FillAvailableSummaries

		/// <summary>
		/// Fills the <see cref="SummaryOperandCollection"/> with the operands that the column expects as summary values.
		/// </summary>
		/// <param name="availableSummaries"></param>
		protected internal virtual void FillAvailableSummaries(SummaryOperandCollection availableSummaries)
		{
			ColumnLayout columnLayout = this.ColumnLayout;
			if (columnLayout != null)
			{
				XamGrid grid = columnLayout.Grid;

				if (grid != null)
				{
					availableSummaries.Add(new CountSummaryOperand());

					if (this.DataType != typeof(bool) &&
						this.DataType != typeof(bool?))
					{
						availableSummaries.Add(new MaximumSummaryOperand());
						availableSummaries.Add(new MinimumSummaryOperand());
					}

					if (this.DataType == typeof(int) ||
						this.DataType == typeof(int?) ||
						this.DataType == typeof(double) ||
						this.DataType == typeof(double?) ||
						this.DataType == typeof(decimal) ||
						this.DataType == typeof(decimal?) ||
						this.DataType == typeof(long) ||
						this.DataType == typeof(long?) ||
						this.DataType == typeof(float) ||
						this.DataType == typeof(float?)||
                        this.DataType == typeof(byte) ||
						this.DataType == typeof(byte?) ||
						this.DataType == typeof(sbyte) ||
						this.DataType == typeof(sbyte?) ||
						this.DataType == typeof(short) ||
						this.DataType == typeof(short?) ||
						this.DataType == typeof(ushort) ||
						this.DataType == typeof(ushort?) ||
						this.DataType == typeof(ulong) ||
						this.DataType == typeof(ulong?) ||
						this.DataType == typeof(uint) ||
						this.DataType == typeof(uint?) 
                        
                        )
					{
						availableSummaries.Add(new SumSummaryOperand());
						availableSummaries.Add(new AverageSummaryOperand());
					}
				}
			}
		}

		#endregion // FillAvailableSummaries

		#region OnShowAvailableColumnFilterOperands

		/// <summary>
		/// Raises the <see cref="XamGrid"/> event notifying that the filter populating event is being fired.
		/// </summary>
		protected internal void OnShowAvailableColumnFilterOperands()
		{
			if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
			{
				PopulatingFiltersEventArgs args = new PopulatingFiltersEventArgs();

				args.Column = this;

				args.FilterOperands = this.FilterColumnSettings.RowFilterOperands;

				this.ColumnLayout.Grid.OnShowAvailableColumnFilterOperands(args);
			}
		}

		#endregion // OnShowAvailableColumnFilterOperands

		#region ValidateSortComparer

		/// <summary>
		/// Used to validate the <see cref="SortComparer"/> can be used on this <see cref="Column"/>.
		/// </summary>
		protected virtual void ValidateSortComparer()
		{
			if (this._comparer != null && this.DataType != null)
			{
				System.Type typeOfIComp = typeof(IComparer<>).MakeGenericType(new System.Type[] { this.DataType });

				if (!typeOfIComp.IsAssignableFrom(this._comparer.GetType()))
				{
					throw new TypeLoadException(string.Format(System.Threading.Thread.CurrentThread.CurrentCulture, SRGrid.GetString("TypeMismatchException"), this._comparer.GetType().Name, typeOfIComp.Name));
				}
			}
		}

		#endregion // ValidateSortComparer

		#region ValidateGroupByComparer

		/// <summary>
		/// Used to validate the <see cref="GroupByComparer"/> can be used on this <see cref="Column"/>.
		/// </summary>
		protected virtual void ValidateGroupByComparer()
		{
			if (this._groupByComparer != null && this.DataType != null)
			{
				System.Type typeOfIComp = typeof(IEqualityComparer<>).MakeGenericType(new System.Type[] { this.DataType });

				if (!typeOfIComp.IsAssignableFrom(this._groupByComparer.GetType()))
				{
					throw new TypeLoadException(string.Format(System.Threading.Thread.CurrentThread.CurrentCulture, SRGrid.GetString("TypeMismatchException"), this._groupByComparer.GetType().Name, typeOfIComp.Name));
				}
			}
		}

		#endregion // ValidateGroupByComparer

        #region ResolveChildColumns

        /// <summary>
        /// If a <see cref="Column"/> owns child Columns, return the collection of Columns.
        /// </summary>
        /// <returns></returns>
        protected internal virtual CollectionBase<Column> ResolveChildColumns()
        {
            return null;
        }

        #endregion // ResolveChildColumns

        #region InvalidateVisibility
        /// <summary>
        /// Changes the visibility of all subcolumns.
        /// </summary>
        /// <param name="makeFirstVisIfCollapsed"></param>
        protected internal void InvalidateVisibility(bool makeFirstVisIfCollapsed)
        {
            ReadOnlyKeyedColumnBaseCollection<Column> allCols = this.AllColumns;
            bool visible = false;
            foreach (Column childCol in allCols)
            {
                if (childCol.ParentColumn == this && childCol.Visibility == Visibility.Visible)
                {
                    visible = true;
                    break;
                }
            }

            if (!visible)
            {
                this.Visibility = Visibility.Collapsed;

                if (makeFirstVisIfCollapsed)
                {
                    if (allCols.Count > 0)
                        allCols[0].Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
        #endregion // InvalidateVisibility

        #region GenerateFilterSelectionControl

        /// <summary>
        /// Returns a new instance of the <see cref="SelectionControl"/> which will be used by the FilterMenu.
        /// </summary>
        /// <returns></returns>
        protected internal virtual SelectionControl GenerateFilterSelectionControl()
        {
            return new FilterSelectionControl();
        }

        #endregion // GenerateFilterSelectionControl

        #region FillFilterMenuOptions
        /// <summary>
        /// Fills the inputted list with options for the FilterMenu control.
        /// </summary>
        /// <param name="list"></param>
        internal protected virtual void FillFilterMenuOptions(List<FilterMenuTrackingObject> list)
        {
            if (list != null)
            {
                FilterMenuTrackingObject fmto = new FilterMenuTrackingObject();
                fmto.Label = string.IsNullOrEmpty(this.FilterColumnSettings.FilterMenuTypeSpecificFiltersString) ? this.FilterMenuCustomFilterString : this.FilterColumnSettings.FilterMenuTypeSpecificFiltersString;
                list.Add(fmto);
                fmto.Children.Add(new FilterMenuTrackingObject(new EqualsOperand() { DisplayName = SRGrid.GetString("EqualsEllipsis") }));
                fmto.Children.Add(new FilterMenuTrackingObject(new NotEqualsOperand() { DisplayName = SRGrid.GetString("NotEqualsEllipsis") }));
            }
        }
        #endregion // FillFilterMenuOptions

        #endregion // Protected

        #region Internal

        internal void SetGroupBy(bool groupBy)
		{
            if (!this._ignoreGroupByCalls)
            {
                this._isGroupBy = groupBy;

                if (this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                {
                    if (groupBy)
                    {
                        // Merged columns can't be fixed
                        this.IsFixed = FixedState.NotFixed;

                        // if We are part of a group, remove it from the grouping.
                        if (this.ParentColumn != null)
                        {
                            Column parent = this.ParentColumn;
                            // Store off the children (this should only be done once per parent column)
                            if (parent._cachedChildren == null)
                            {
                                parent._cachedChildren = new List<Column>(this.ParentColumn.ResolveChildColumns());
                            }

                            // Cache the parent column so we can add ourselves back afterwards
                            this._cachedParentColumn = this.ParentColumn;

                            ColumnLayout layout = this.ColumnLayout;
                            XamGrid grid = layout.Grid;

                            // Remove ourselves from the parent column and put ourselves into our ColumnLayout's columns collection. 
                            this.IsMoving = false;
                            CollectionBase<Column> children = parent.ResolveChildColumns();
                            children.Remove(this);
                            layout.Columns.Add(this);

                            // Make sure we invalidate, so that we release all of our old cell controls.
                            grid.ResetPanelRows();
                            grid.InvalidateScrollPanel(true);

                            this._ignoreGroupByCalls = true;
                            grid.GroupBySettings.GroupByColumns.Remove(this);
                            this._ignoreGroupByCalls = false;

                            parent.InvalidateVisibility(true);
                        }
                    }
                    else
                    {
                        // Ok , when we were merged, we were removed from our parent column.
                        if (this._cachedParentColumn != null)
                        {
                            ColumnLayout layout = this.ColumnLayout;
                            XamGrid grid = layout.Grid;

                            this._ignoreGroupByCalls = true;

                            CollectionBase<Column> children = this._cachedParentColumn.ResolveChildColumns();
                            List<Column> cachedChildren = this._cachedParentColumn._cachedChildren;

                            // Remove ourselves from the columnlayout
                            this.IsMoving = false;
                            layout.Columns.Remove(this);

                            int index = cachedChildren.IndexOf(this);

                            // Figure out where we needed to get ourselves inserted into in our parent column;
                            bool inserted = false;
                            for (int i = index + 1; i < cachedChildren.Count; i++)
                            {
                                Column currentCol = cachedChildren[i];
                                int currentIndex = children.IndexOf(currentCol);
                                if (currentIndex != -1)
                                {
                                    children.Insert(currentIndex, this);
                                    inserted = true;
                                    break;
                                }
                            }

                            if (!inserted)
                                children.Add(this);

                            if (this._cachedParentColumn.Visibility == System.Windows.Visibility.Collapsed)
                                this._cachedParentColumn.Visibility = System.Windows.Visibility.Visible;

                            // If all the cached children are back, then we don't need to store this anymore.
                            if (children.Count == cachedChildren.Count)
                                this._cachedParentColumn._cachedChildren = null;

                            this._cachedParentColumn = null;

                            this._ignoreGroupByCalls = false;
                        }
                    }

                    this.OnPropertyChanged("IsGroupBy");
                }
            }
		}

		internal bool SetFixedColumnState(FixedState state)
		{
            bool isMerged = false;

            if (this.IsGroupBy && this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                isMerged = true;

            // MergedColumns and Columns in a GroupColumn cannot be fixed.
            if (this.ParentColumn != null || isMerged)
            {
                this._isFixed = FixedState.NotFixed;

                if (state != FixedState.NotFixed)
                    return false;
            }

			bool cancel = false;
			if (this.ColumnLayout != null)
				cancel = this.ColumnLayout.Grid.OnColumnFixedStateChanging(this, state);
			if (!cancel)
			{
				FixedState previous = this._isFixed;
				this._isFixed = state;

				if (this.ColumnLayout != null)
				{
					if (state == FixedState.Left)
					{
						this.ColumnLayout.FixedColumnSettings.FixedColumnsLeft.AddItemSilently(this);
						this.ColumnLayout.FixedColumnSettings.FixedColumnsRight.RemoveItemSilently(this);
					}
					else if (state == FixedState.NotFixed)
					{
						this.ColumnLayout.FixedColumnSettings.FixedColumnsRight.RemoveItemSilently(this);
						this.ColumnLayout.FixedColumnSettings.FixedColumnsLeft.RemoveItemSilently(this);
					}
					else if (state == FixedState.Right)
					{
						this.ColumnLayout.FixedColumnSettings.FixedColumnsRight.AddItemSilently(this);
						this.ColumnLayout.FixedColumnSettings.FixedColumnsLeft.RemoveItemSilently(this);
					}

					this.ColumnLayout.Grid.OnColumnFixedStateChanged(this, previous);
				}

				this.OnPropertyChanged("IsFixed");
			}
			return !cancel;
		}

		internal void SetSortedColumnStateSilent(SortDirection state)
		{
            if (!this.CanBeSorted)
                return;

			this._isSorted = state;

			if (this.ColumnLayout != null)
			{
				if (state == SortDirection.Ascending)
				{
					this.ColumnLayout.SortingSettings.SortedColumns.AddItemSilently(this);
				}
				else if (state == SortDirection.Descending)
				{
					this.ColumnLayout.SortingSettings.SortedColumns.AddItemSilently(this);
				}
				else if (state == SortDirection.None)
				{
					this.ColumnLayout.SortingSettings.SortedColumns.RemoveItemSilently(this);
				}
			}
		}

        internal void SetSortedColumnStateSilent(SortDirection state, int index)
        {
            this._isSorted = state;

            if (this.ColumnLayout != null)
            {
                if (state == SortDirection.Ascending)
                {
                    this.ColumnLayout.SortingSettings.SortedColumns.InsertItemSilently(this, index);
                }
                else if (state == SortDirection.Descending)
                {
                    this.ColumnLayout.SortingSettings.SortedColumns.InsertItemSilently(this, index);
                }
                else if (state == SortDirection.None)
                {
                    this.ColumnLayout.SortingSettings.SortedColumns.RemoveItemSilently(this);
                }
            }
        }

		internal bool SetSortedColumnState(SortDirection state)
		{
			return this.SetSortedColumnStateInternally(state, false);
		}

		internal bool SetSortedColumnStateInternally(SortDirection state, bool silent)
		{
            if (!this.CanBeSorted)
                return true;

			bool cancel = false;

			if (this.ColumnLayout != null)
			{
				cancel = this.ColumnLayout.Grid.OnColumnSorting(this, state);
			}
			if (!cancel)
			{
				SortDirection previous = this._isSorted;

				this.SetSortedColumnStateSilent(state);

				if (!silent)
					this.OnPropertyChanged("IsSorted");

				if (this.ColumnLayout != null)
				{
					this.ColumnLayout.Grid.OnColumnSorted(this, previous);
				}
			}
			return !cancel;
		}

        internal bool SetSortedColumnStateInternally(SortDirection state, bool silent, int index)
        {
            bool cancel = false;

            if (this.ColumnLayout != null)
            {
                cancel = this.ColumnLayout.Grid.OnColumnSorting(this, state);
            }
            if (!cancel)
            {
                SortDirection previous = this._isSorted;

                this.SetSortedColumnStateSilent(state, index);

                if (!silent)
                    this.OnPropertyChanged("IsSorted");

                if (this.ColumnLayout != null)
                {
                    this.ColumnLayout.Grid.OnColumnSorted(this, previous);
                }
            }
            return !cancel;
        }

		internal bool SetSortedColumnState(SortDirection state, bool multipleColumnSort)
		{
            if (!this.CanBeSorted)
                return true;

			bool cancel = false;

			ColumnLayout colLayout = this.ColumnLayout;
			if (colLayout != null)
			{
				cancel = colLayout.Grid.OnColumnSorting(this, state);
			}
			if (!cancel)
			{
				SortDirection previous = this._isSorted;
				SortedColumnsCollection sortedColumns = colLayout.SortingSettings.SortedColumns;
				if (state == SortDirection.None)
				{
					sortedColumns.RemoveItemSilently(this);
				}
				else
				{
					SortingSettingsOverride sortingSettings = colLayout.SortingSettings;
					if (!multipleColumnSort || !sortingSettings.AllowMultipleColumnSortingResolved)
					{
						sortedColumns.RemoveItemSilently(this);
						sortedColumns.ClearSilently();
						sortedColumns.AddItemSilently(this);
					}
				}

				this.SetSortedColumnStateSilent(state);

				this.OnPropertyChanged("IsSorted");

				if (this.ColumnLayout != null)
				{
					this.ColumnLayout.Grid.OnColumnSorted(this, previous);
				}
			}
			return !cancel;
		}

		internal SortDirection NextSortDirection
		{
			get
			{
				SortDirection direction = this.IsSorted;

				if (this.ColumnLayout != null)
				{
					if (this.IsSorted == SortDirection.Ascending)
					{
						direction = SortDirection.Descending;
					}
					else if (this.IsSorted == SortDirection.None)
					{
						direction = this.FirstSortDirectionResolved;
					}
					else
					{
						direction = SortDirection.Ascending;
					}
				}

				return direction;
			}
		}

		#endregion // Internal

		#endregion // Methods

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

		#region Overrides

		#region OnVisibilityChanged
		/// <summary>
		/// Raised when the Visiblity of a <see cref="ColumnBase"/> has changed.
		/// </summary>
		protected override void OnVisibilityChanged()
		{
			if (this.Visibility == Visibility.Collapsed)
			{
				this._cachedActualWidth = this._actualWidth;
				this._actualWidth = 0;
			}
			else
			{
				this._actualWidth = this._cachedActualWidth;
				this._cachedActualWidth = 0;
			}

			base.OnVisibilityChanged();
		}
		#endregion // OnVisibilityChanged

		#region ToString


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		#endregion // ToString

		#region OnDataTypeChanged

		/// <summary>
		/// Raised when the DataType of the <see cref="ColumnBase" /> is changed.
		/// </summary>
		protected override void OnDataTypeChanged()
		{
			this.SummaryColumnSettings.ValidateSummaryOperands();
            FilterColumnSettings fcs = this.FilterColumnSettings;
            fcs.Reset();
            fcs.ValidateFilterMenuOperands();
			fcs.ValidateFilterOperands();
			base.OnDataTypeChanged();
		}

		#endregion // OnDataTypeChanged        

        #region OnColumnLayoutChanged
        /// <summary>
        /// Raised when the a <see cref="ColumnLayout"/> is assigned or removed from this <see cref="ColumnBase"/>
        /// </summary>
        protected override void OnColumnLayoutChanged()
        {
            this.SummaryColumnSettings.ValidateSummaryOperands();
            FilterColumnSettings fcs = this.FilterColumnSettings;
            fcs.Reset();
            fcs.ValidateFilterMenuOperands();
            fcs.ValidateFilterOperands();
            base.OnColumnLayoutChanged();
        }
        #endregion // OnColumnLayoutChanged

        #endregion // Overrides

        #region EventHandlers

        void SummaryColumnSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case ("RowDisplayLabel"):
				case ("FormatString"):
					{
						if (this.ColumnLayout != null)
						{
							this.ColumnLayout.RefreshSummaryRowVisual();
						}
						break;
					}
				default:
					{
						this.OnPropertyChanged("SummaryColumnSettings");
						break;
					}
			}

		}

		void ConditionalFormatCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.OnPropertyChanged("ConditionalFormatCollectionCollectionChanged");
		}

		void ConditionalFormatCollection_CollectionItemChanged(object sender, EventArgs e)
		{
			if (this.ColumnLayout != null)
				this.ColumnLayout.InvalidateConditionalFormatting();
		}

		#endregion // EventHandlers

		#region IProvidePropertyPersistenceSettings Members


		/// <summary>
		/// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
		/// </summary>
		protected override List<string> PropertiesToIgnore
		{
			get
			{
				List<string> properties = base.PropertiesToIgnore;
				properties.AddRange(new List<string>()
					{
                        "IsGroupBy",
                        "IsFixed",
                        "IsSorted",
                        "IsSelected",
					});

				return properties;
			}
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