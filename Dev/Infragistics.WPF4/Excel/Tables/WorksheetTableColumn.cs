using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Sorting;




using Infragistics.Shared;
using System.Drawing;


namespace Infragistics.Documents.Excel
{
	// MD 12/21/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a column in a <see cref="WorksheetTable"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Each column contains various settings for controlling the contents, formatting, sorting, and filtering within it.
	/// </p>
	/// </remarks>
	/// <see cref="WorksheetTable.Columns"/>
	[DebuggerDisplay("WorksheetTableColumn: {NameInternal}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class WorksheetTableColumn :
		IAreaFormatsOwner<WorksheetTableColumnArea>,
		IFilterable,
		ISortable
	{
		#region Member Variables

		private short _absoluteColumnIndex;
		private WorksheetTableAreaFormatsCollection<WorksheetTableColumnArea> _areaFormats;
		private List<Formula> _appliedColumnFormulas;
		private int _cachedIndex = -1;
		private Formula _columnFormula;
		private Filter _filter;
		private uint _id;
		private string _name;
		private uint _sortConditionDxfIdDuringSave;
		private WorksheetTable _table;
		private Formula _totalFormula;
		private string _totalLabel;

		#endregion // Member Variables

		#region Constructor

		internal WorksheetTableColumn(WorksheetTable owner, uint id, short absoluteColumnIndex)
		{
			_absoluteColumnIndex = absoluteColumnIndex;
			_id = id;
			_table = owner;
		}

		#endregion // Constructor

		#region Interfaces

		#region IAreaFormatsOwner Members

		bool IAreaFormatsOwner<WorksheetTableColumnArea>.IsReadOnly
		{
			get { return false; }
		}

		void IAreaFormatsOwner<WorksheetTableColumnArea>.OnAreaFormatAdded(WorksheetTableColumnArea area, WorksheetCellFormatData format)
		{
			switch (area)
			{
				case WorksheetTableColumnArea.DataArea:
					break;

				case WorksheetTableColumnArea.HeaderCell:
					WorksheetCell headerCell = this.HeaderCell;
					if (headerCell != null)
					{
						WorksheetCellFormatData cellFormat;
						if (headerCell.Row.TryGetCellFormat(headerCell.ColumnIndexInternal, out cellFormat))
						{
							cellFormat = cellFormat.CloneInternal();
							cellFormat.ResetValue(CellFormatValue.TopBorderColorInfo);
							cellFormat.ResetValue(CellFormatValue.TopBorderStyle);
							format.SetFormatting(cellFormat);
						}
					}
					break;

				case WorksheetTableColumnArea.TotalCell:
					WorksheetCell totalCell = this.TotalCell;
					if (totalCell != null)
					{
						WorksheetCellFormatData cellFormat;
						if (totalCell.Row.TryGetCellFormat(totalCell.ColumnIndexInternal, out cellFormat))
						{
							cellFormat = cellFormat.CloneInternal();
							cellFormat.ResetValue(CellFormatValue.BottomBorderColorInfo);
							cellFormat.ResetValue(CellFormatValue.BottomBorderStyle);
							format.SetFormatting(cellFormat);
						}
					}
					break;

				default:
					Utilities.DebugFail("Unknown WorksheetTableColumnArea: " + area);
					break;
			}
		}

		void IAreaFormatsOwner<WorksheetTableColumnArea>.VerifyCanBeModified() { }

		#endregion

		#region IFilterable Members

		short IFilterable.ColumnIndex
		{
			get { return this.WorksheetColumnIndex; }
		}

		#endregion

		#region IGenericCachedCollectionEx Members

		Workbook IGenericCachedCollectionEx.Workbook
		{
			get { return this.Workbook; }
		}

		#endregion

		#region IWorksheetCellFormatProxyOwner Members

		WorksheetCellFormatData IWorksheetCellFormatProxyOwner.GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue)
		{
			return null;
		}

		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options)
		{
			WorksheetTableAreaFormatProxy<WorksheetTableColumnArea> areaFormatProxy = (WorksheetTableAreaFormatProxy<WorksheetTableColumnArea>)sender;
			this.SynchronizeAreaFormatWithCells(areaFormatProxy, values);
		}

		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanging(WorksheetCellFormatProxy sender, IList<CellFormatValue> values)
		{
			WorksheetTableAreaFormatProxy<WorksheetTableColumnArea> areaFormatProxy = (WorksheetTableAreaFormatProxy<WorksheetTableColumnArea>)sender;
			for (int i = 0; i < values.Count; i++)
				WorksheetTableColumn.VerifyAreaFormatValueCanBeSet(areaFormatProxy.Area, values[i]);
		}

		void IWorksheetCellFormatProxyOwner.VerifyFormatOptions(WorksheetCellFormatProxy sender, WorksheetCellFormatOptions formatOptions) { }

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region ApplyAverageFilter

		/// <summary>
		/// Applies an <see cref="AverageFilter"/> to the column.
		/// </summary>
		/// <param name="type">The value indicating whether to filter in values below or above the average of the data range.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="type"/> is not defined in the <see cref="AverageFilterType"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="AverageFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public AverageFilter ApplyAverageFilter(AverageFilterType type)
		{
			AverageFilter filter = new AverageFilter(this, type);
			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyAverageFilter

		#region ApplyCustomFilter

		/// <summary>
		/// Applies a <see cref="CustomFilter"/> to the column.
		/// </summary>
		/// <param name="condition">The condition which must pass for the data to be filtered in.</param>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> If the filter condition value is longer than 255 characters in length and the workbook is saved in one of 
		/// the 2003 formats, the correct rows will be hidden in the saved file, but the filter will be missing from the column.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="condition"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="CustomFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public CustomFilter ApplyCustomFilter(CustomFilterCondition condition)
		{
			return this.ApplyCustomFilter(condition, null, ConditionalOperator.And);
		}

		/// <summary>
		/// Applies a <see cref="CustomFilter"/> to the column.
		/// </summary>
		/// <param name="condition1">The first condition used to filter the data.</param>
		/// <param name="condition2">The second condition used to filter the data.</param>
		/// <param name="conditionalOperator">
		/// The operator which defines how to logically combine <paramref name="condition1"/> and <paramref name="condition2"/>.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// If <paramref name="condition2"/> is null, the <paramref name="conditionalOperator"/> value is irrelevant.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> If one of the filter condition values is longer than 255 characters in length and the workbook is saved in one of 
		/// the 2003 formats, the correct rows will be hidden in the saved file, but the filter will be missing from the column.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="condition1"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="conditionalOperator"/> is not defined in the <see cref="ConditionalOperator"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="CustomFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public CustomFilter ApplyCustomFilter(CustomFilterCondition condition1, CustomFilterCondition condition2, ConditionalOperator conditionalOperator)
		{
			CustomFilter filter = new CustomFilter(this, condition1, condition2, conditionalOperator);
			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyCustomFilter

		#region ApplyDatePeriodFilter

		/// <summary>
		/// Applies an <see cref="DatePeriodFilter"/> to the column.
		/// </summary>
		/// <param name="type">The type of date period to filter in.</param>
		/// <param name="value">The 1-based value of the month or quarter to filter in.</param>
		/// <remarks>
		/// <p class="body">
		/// If the <paramref name="type"/> is Month, a <paramref name="value"/> of 1 indicates January, 2 indicates February, and so on. 
		/// If type is Quarter, a value of 1 indicates Quarter 1, and so on.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="type"/> is not defined in the <see cref="DatePeriodFilterType"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="type"/> is Quarter and <paramref name="value"/> is less than 1 or greater than 4 or
		/// type is Month and value is less than 1 or greater than 12.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="DatePeriodFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public DatePeriodFilter ApplyDatePeriodFilter(DatePeriodFilterType type, int value)
		{
			DatePeriodFilter filter = new DatePeriodFilter(this, type, value);
			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyDatePeriodFilter

		#region ApplyFillFilter

		/// <summary>
		/// Applies a <see cref="FillFilter"/> to the column.
		/// </summary>
		/// <param name="fill">A <see cref="CellFill"/> by which the cells should be filtered.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fill"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="FillFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public FillFilter ApplyFillFilter(CellFill fill)
		{
			FillFilter filter = new FillFilter(this, fill);
			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyFillFilter

		#region ApplyFixedValuesFilter

		/// <summary>
		/// Applies a <see cref="FixedValuesFilter"/> to the column.
		/// </summary>
		/// <param name="includeBlanks">The value which indicates whether blank cells should be filtered in.</param>
		/// <param name="displayValues">The collection of case-insensitively unique cell text values which should be filtered in.</param>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> If any text values are longer than 255 characters in length and the workbook is saved in one of the 2003 formats,
		/// the correct rows will be hidden in the saved file, but the filter may be missing from the column or reapplying the filter
		/// may hide some of the matching cells.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="displayValues"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// A value in the <paramref name="displayValues"/> collection is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Multiple values from the <paramref name="displayValues"/> collection are case-insensitively equal.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="includeBlanks"/> is False and <paramref name="displayValues"/> has no items. At least one value must be allowed.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="FixedValuesFilter"/>
		/// <seealso cref="WorksheetCell.GetText(TextFormatMode)"/>
		/// <seealso cref="WorksheetRow.GetCellText(int,TextFormatMode)"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public FixedValuesFilter ApplyFixedValuesFilter(bool includeBlanks, params string[] displayValues)
		{
			return this.ApplyFixedValuesFilter(includeBlanks, (IEnumerable<string>)displayValues);
		}

		/// <summary>
		/// Applies a <see cref="FixedValuesFilter"/> to the column.
		/// </summary>
		/// <param name="includeBlanks">The value which indicates whether blank cells should be filtered in.</param>
		/// <param name="displayValues">The collection of case-insensitively unique cell text values which should be filtered in.</param>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> If any text values are longer than 255 characters in length and the workbook is saved in one of the 2003 formats,
		/// the correct rows will be hidden in the saved file, but the filter may be missing from the column or reapplying the filter
		/// may hide some of the matching cells.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="displayValues"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// A value in the <paramref name="displayValues"/> collection is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Multiple values from the <paramref name="displayValues"/> collection are case-insensitively equal.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="includeBlanks"/> is False and <paramref name="displayValues"/> has no items. At least one value must be allowed.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="FixedValuesFilter"/>
		/// <seealso cref="WorksheetCell.GetText(TextFormatMode)"/>
		/// <seealso cref="WorksheetRow.GetCellText(int,TextFormatMode)"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public FixedValuesFilter ApplyFixedValuesFilter(bool includeBlanks, IEnumerable<string> displayValues)
		{
			if (displayValues == null)
				throw new ArgumentNullException("displayValues");

			FixedValuesFilter filter = new FixedValuesFilter(this);
			filter.IncludeBlanks = includeBlanks;
			foreach (string displayValue in displayValues)
				filter.DisplayValues.Add(displayValue);

			if (filter.IsEmpty)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FixedValuesFilterMustAcceptAValue"));

			this.Filter = filter;
			return filter;
		}

		/// <summary>
		/// Applies a <see cref="FixedValuesFilter"/> to the column.
		/// </summary>
		/// <param name="includeBlanks">The value which indicates whether blank cells should be filtered in.</param>
		/// <param name="dateGroups">The collection of fixed date groups which should be filtered in.</param>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> If any text values are longer than 255 characters in length and the workbook is saved in one of the 2003 formats,
		/// the correct rows will be hidden in the saved file, but the filter may be missing from the column or reapplying the filter
		/// may hide some of the matching cells.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dateGroups"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// A FixedDateGroup in the <paramref name="dateGroups"/> collection is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Multiple items in <paramref name="dateGroups"/> are equal to each other.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="includeBlanks"/> is False and <paramref name="dateGroups"/> has no items. At least one value must be allowed.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="FixedValuesFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public FixedValuesFilter ApplyFixedValuesFilter(bool includeBlanks, IEnumerable<FixedDateGroup> dateGroups)
		{
			return this.ApplyFixedValuesFilter(includeBlanks, CalendarType.Gregorian, dateGroups);
		}

		/// <summary>
		/// Applies a <see cref="FixedValuesFilter"/> to the column.
		/// </summary>
		/// <param name="includeBlanks">The value which indicates whether blank cells should be filtered in.</param>
		/// <param name="calendarType">The calendar type used to interpret values in the <paramref name="dateGroups"/> collection.</param>
		/// <param name="dateGroups">The collection of fixed date groups which should be filtered in.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dateGroups"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="calendarType"/> is not defined in the <see cref="CalendarType"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// A FixedDateGroup in the <paramref name="dateGroups"/> collection is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Multiple items in <paramref name="dateGroups"/> are equal to each other.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="includeBlanks"/> is False and <paramref name="dateGroups"/> has no items. At least one value must be allowed.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="FixedValuesFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public FixedValuesFilter ApplyFixedValuesFilter(bool includeBlanks, CalendarType calendarType, params FixedDateGroup[] dateGroups)
		{
			return this.ApplyFixedValuesFilter(includeBlanks, calendarType, (IEnumerable<FixedDateGroup>)dateGroups);
		}

		/// <summary>
		/// Applies a <see cref="FixedValuesFilter"/> to the column.
		/// </summary>
		/// <param name="includeBlanks">The value which indicates whether blank cells should be filtered in.</param>
		/// <param name="calendarType">The calendar type used to interpret values in the <paramref name="dateGroups"/> collection.</param>
		/// <param name="dateGroups">The collection of fixed date groups which should be filtered in.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dateGroups"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="calendarType"/> is not defined in the <see cref="CalendarType"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// A FixedDateGroup in the <paramref name="dateGroups"/> collection is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Multiple items in <paramref name="dateGroups"/> are equal to each other.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="includeBlanks"/> is False and <paramref name="dateGroups"/> has no items. At least one value must be allowed.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="FixedValuesFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public FixedValuesFilter ApplyFixedValuesFilter(bool includeBlanks, CalendarType calendarType, IEnumerable<FixedDateGroup> dateGroups)
		{
			if (dateGroups == null)
				throw new ArgumentNullException("dateGroups");

			FixedValuesFilter filter = new FixedValuesFilter(this);
			filter.CalendarType = calendarType;
			filter.IncludeBlanks = includeBlanks;
			foreach (FixedDateGroup dateGroup in dateGroups)
				filter.DateGroups.Add(dateGroup);

			if (filter.IsEmpty)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FixedValuesFilterMustAcceptAValue"));

			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyFixedValuesFilter

		#region ApplyFontColorFilter

		/// <summary>
		/// Applies a <see cref="FontColorFilter"/> to the column.
		/// </summary>
		/// <param name="fontColor">The font color by which the cells should be filtered.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fontColor"/> is empty.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="FontColorFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public FontColorFilter ApplyFontColorFilter(Color fontColor)
		{
			return this.ApplyFontColorFilter(Utilities.ToColorInfo(fontColor));
		}

		/// <summary>
		/// Applies a <see cref="FontColorFilter"/> to the column.
		/// </summary>
		/// <param name="fontColorInfo">
		/// A <see cref="WorkbookColorInfo"/> which describes the font color by which the cells should be filtered.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fontColorInfo"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="FontColorFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public FontColorFilter ApplyFontColorFilter(WorkbookColorInfo fontColorInfo)
		{
			FontColorFilter filter = new FontColorFilter(this, fontColorInfo);
			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyFontColorFilter

		#region ApplyRelativeDateRangeFilter

		/// <summary>
		/// Applies a <see cref="RelativeDateRangeFilter"/> to the column.
		/// </summary>
		/// <param name="offset">
		/// The offset of relative filter. This combined with the <paramref name="duration"/> determines the full range of accepted dates.
		/// </param>
		/// <param name="duration">The duration of the full range of accepted dates.</param>
		/// <remarks>
		/// <p class="body">
		/// The RelativeDateRangeFilter allows you to filter in dates which are in the previous, current, or next time period 
		/// relative to the date when the filter was applied. The time periods available are day, week, month, quarter, year.
		/// So when using the previous filter type with a day duration, a 'yesterday' filter is created. Or when using a current 
		/// filter type with a year duration, a 'this year' filter is created. However, these filters compare the data against 
		/// the date when the filter was created. So a 'this year' filter created in 1999 will filter in all cells containing 
		/// dates in 1999, even if the workbook is opened in 2012.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="offset"/> is not defined in the <see cref="RelativeDateRangeOffset"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="duration"/> is not defined in the <see cref="RelativeDateRangeDuration"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="RelativeDateRangeFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public RelativeDateRangeFilter ApplyRelativeDateRangeFilter(RelativeDateRangeOffset offset, RelativeDateRangeDuration duration)
		{
			RelativeDateRangeFilter filter = new RelativeDateRangeFilter(this, offset, duration);
			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyRelativeDateRangeFilter

		#region ApplyTopOrBottomFilter

		/// <summary>
		/// Applies a <see cref="TopOrBottomFilter"/> to the column which will filter in the top 10 values in the list of sorted values.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="TopOrBottomFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public TopOrBottomFilter ApplyTopOrBottomFilter()
		{
			return this.ApplyTopOrBottomFilter(TopOrBottomFilterType.TopValues, 10);
		}

		/// <summary>
		/// Applies a <see cref="TopOrBottomFilter"/> to the column.
		/// </summary>
		/// <param name="type">The type of the filter.</param>
		/// <param name="value">The number or percentage of value of values which should be filtered in.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="type"/> is not defined in the <see cref="TopOrBottomFilterType"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than 1 or greater than 500.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="TopOrBottomFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public TopOrBottomFilter ApplyTopOrBottomFilter(TopOrBottomFilterType type, int value)
		{
			TopOrBottomFilter filter = new TopOrBottomFilter(this, type, value);
			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyTopOrBottomFilter

		#region ApplyYearToDateFilter

		/// <summary>
		/// Applies a <see cref="YearToDateFilter"/> to the column.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The <see cref="WorksheetTable.IsFilterUIVisible"/> value of the owning table is False. 
		/// Filters cannot be applied when the header row or filter button is hidden.
		/// </exception>
		/// <seealso cref="YearToDateFilter"/>
		/// <seealso cref="Filter"/>
		/// <seealso cref="ClearFilter"/>
		public YearToDateFilter ApplyYearToDateFilter()
		{
			YearToDateFilter filter =  new YearToDateFilter(this);
			this.Filter = filter;
			return filter;
		}

		#endregion // ApplyYearToDateFilter

		#region ClearFilter

		/// <summary>
		/// Removes the filter from the column if one is applied.
		/// </summary>
		/// <seealso cref="ApplyAverageFilter"/>
		/// <seealso cref="ApplyCustomFilter(CustomFilterCondition)"/>
		/// <seealso cref="ApplyCustomFilter(CustomFilterCondition,CustomFilterCondition,ConditionalOperator)"/>
		/// <seealso cref="ApplyDatePeriodFilter"/>
		/// <seealso cref="ApplyFillFilter"/>
		/// <seealso cref="ApplyFixedValuesFilter(bool,string[])"/>
		/// <seealso cref="ApplyFixedValuesFilter(bool,IEnumerable&lt;string&gt;)"/>
		/// <seealso cref="ApplyFixedValuesFilter(bool,IEnumerable&lt;FixedDateGroup&gt;)"/>
		/// <seealso cref="ApplyFixedValuesFilter(bool,CalendarType,IEnumerable&lt;FixedDateGroup&gt;)"/>
		/// <seealso cref="ApplyFontColorFilter(Color)"/>
		/// <seealso cref="ApplyFontColorFilter(WorkbookColorInfo)"/>
		/// <seealso cref="ApplyRelativeDateRangeFilter"/>
		/// <seealso cref="ApplyTopOrBottomFilter()"/>
		/// <seealso cref="ApplyTopOrBottomFilter(TopOrBottomFilterType,int)"/>
		/// <seealso cref="ApplyYearToDateFilter"/>
		/// <seealso cref="Filter"/>
		public void ClearFilter()
		{
			this.Filter = null;
		}

		#endregion // ClearFilter

		#region SetColumnFormula

		/// <summary>
		/// Sets the formula to use in the data cells in the column.
		/// </summary>
		/// <param name="formula">The formula for the data cells of the column or null to remove the current column formula.</param>
		/// <param name="overwriteExistingValues">
		/// True to overwrite the existing cells values and apply the formula to all data cells in the column. 
		/// False to only apply the formula to the cells with no value set.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// If any relative cell or region references are in the specified formula, it will be assumed that the actual formula is being applied to 
		/// the first data cell in the column. When the formula is applied to other cells in the column, the relative references will be offset by
		/// the appropriate amount.
		/// </p>
		/// <p class="body">
		/// When the column formula is set and the table is resized to give it more rows, the new cells in the column will have the column formula 
		/// applied to them.
		/// </p>
		/// <p class="body">
		/// If there was a different column formula applied previously and it was applied to any of the cells in the column, setting it to a 
		/// different formula will overwrite the formulas on those cells, regardless of the value of <paramref name="overwriteExistingValues"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// <paramref name="formula"/> is already applied to something else, such as a cell or table column.
		/// </exception>
		/// <seealso cref="ColumnFormula"/>
		public void SetColumnFormula(Formula formula, bool overwriteExistingValues)
		{
			if (formula != null && formula.OwningCellRow != null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_ColumnFormulaAlreadyAppliedToCell"));

			if (_columnFormula == formula)
				return;

			// Remove the previous column formula from all cells if one was applied previously.
			this.ClearAppliedColumnFormulas();

			WorksheetRow rowOfTopDataCell = null;
			short columnIndex = -1;

			WorksheetRegion dataAreaRegion = this.DataAreaRegion;
			if (dataAreaRegion != null)
			{
				if (dataAreaRegion.Worksheet == null)
				{
					Utilities.DebugFail("This is unexpected");
					return;
				}

				rowOfTopDataCell = dataAreaRegion.TopRow;
				columnIndex = dataAreaRegion.FirstColumnInternal;

				// Apply the formula to all cell in the column if possible.
				for (int rowIndex = dataAreaRegion.FirstRow; rowIndex <= dataAreaRegion.LastRow; rowIndex++)
				{
					WorksheetRow row = dataAreaRegion.Worksheet.Rows[rowIndex];

					if (overwriteExistingValues == false &&
						Utilities.IsCellValueNull(row.GetCellValueRaw(columnIndex)) == false)
						continue;

					if (formula == null)
					{
						// If the passed in formula is null, clear the value on the cell.
						row.SetCellValueRaw(columnIndex, null);
					}
					else
					{
						// All formulas should be offset as if the original formula was applied to the top cell of the data area,
						// So pass in the top row of the data area as the firstCellAppliedToRow parameter.
						Formula appliedFormula = formula.ApplyTo(rowOfTopDataCell, columnIndex, row, columnIndex);

						if (_appliedColumnFormulas == null)
							_appliedColumnFormulas = new List<Formula>();

						_appliedColumnFormulas.Add(appliedFormula);
					}
				}
			}

			if (_columnFormula != null)
				_columnFormula.SetOwningCell(null, -1);

			_columnFormula = formula;

			if (_columnFormula != null)
				_columnFormula.SetOwningCell(rowOfTopDataCell, columnIndex);
		}

		#endregion // SetColumnFormula

		#endregion // Public Methods

		#region Internal Methods

		#region CanAreaFormatValueBeSet

		internal static bool CanAreaFormatValueBeSet(WorksheetTableColumnArea area, CellFormatValue value)
		{
			string message;
			return WorksheetTableColumn.VerifyAreaFormatValueCanBeSetHelper(area, value, out message);
		}

		#endregion // CanAreaFormatValueBeSet

		#region GetColumnAreaOfRow

		internal WorksheetTableColumnArea GetColumnAreaOfRow(WorksheetRow row)
		{
			if (this.Table == null)
			{
				Utilities.DebugFail("This is unexpected.");
				return WorksheetTableColumnArea.DataArea;
			}

			WorksheetRegionAddress tableAddress = this.Table.WholeTableAddress;

			if (this.Table.IsHeaderRowVisible && row.Index == tableAddress.FirstRowIndex)
				return WorksheetTableColumnArea.HeaderCell;

			if (this.Table.IsTotalsRowVisible && row.Index == tableAddress.LastRowIndex)
				return WorksheetTableColumnArea.TotalCell;

			Debug.Assert(tableAddress.FirstRowIndex <= row.Index && row.Index <= tableAddress.LastRowIndex, "This is unexpected.");
			return WorksheetTableColumnArea.DataArea;
		}

		#endregion // GetColumnAreaOfRow

		#region GetColumnPortionOfTableRegion

		internal WorksheetRegion GetColumnPortionOfTableRegion(WorksheetRegion tableRegion)
		{
			if (tableRegion == null)
				return null;

			if (tableRegion.Worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return null;
			}

			short columnIndex = (short)(tableRegion.FirstColumnInternal + this.Index);
			Debug.Assert(columnIndex <= tableRegion.LastColumnInternal, "The column index of the table column is outside the bounds of the table.");

			return tableRegion.Worksheet.GetCachedRegion(tableRegion.FirstRow, columnIndex, tableRegion.LastRow, columnIndex);
		}

		#endregion // GetColumnPortionOfTableRegion

		#region InitializeAreaFormats

		internal void InitializeAreaFormats()
		{
			Debug.Assert(this.Table.IsTotalsRowVisible == false, "There should be no total cell at this point, so we shouldn't have to initialize anything.");

			// Just requesting these area formats will sync them with the associated cells.
			IWorksheetCellFormat format = this.AreaFormats[WorksheetTableColumnArea.HeaderCell];

			WorksheetRegion dataRegion = this.DataAreaRegion;
			if (dataRegion == null || dataRegion.Worksheet == null)
				return;

			Dictionary<WorksheetCellFormatData, int> formatCounts = new Dictionary<WorksheetCellFormatData, int>();

			WorksheetCellFormatData dataAreaFormat = null;
			WorksheetRow row = dataRegion.Worksheet.Rows.GetIfCreated(dataRegion.FirstRow);
			if (row != null && row.TryGetCellFormat(dataRegion.FirstColumnInternal, out dataAreaFormat))
				formatCounts.Add(dataAreaFormat, 1);

			for (int rowIndex = dataRegion.FirstRow + 1; rowIndex <= dataRegion.LastRow; rowIndex++)
			{
				WorksheetCellFormatData nextDataAreaFormat;

				row = dataRegion.Worksheet.Rows.GetIfCreated(rowIndex);
				if (row == null || row.TryGetCellFormat(dataRegion.FirstColumnInternal, out nextDataAreaFormat) == false)
				{
					dataAreaFormat = null;
					continue;
				}

				if (dataAreaFormat != null && dataAreaFormat.Equals(nextDataAreaFormat) == false)
					dataAreaFormat = null;

				int formatCount;
				if (formatCounts.TryGetValue(nextDataAreaFormat, out formatCount) == false)
					formatCounts.Add(nextDataAreaFormat, 1);
				else
					formatCounts[nextDataAreaFormat] = formatCount + 1;
			}

			// When there are 3 or fewer rows, all cells must have matching formatting for the total cell to be initialize from the format.
			// Otherwise, we just need more than half of the formats to match.
			int requiredNumberOfMatchingFormats;
			if (dataRegion.Height < 4)
				requiredNumberOfMatchingFormats = dataRegion.Height;
			else
				requiredNumberOfMatchingFormats = (dataRegion.Height / 2) + 1;

			foreach (KeyValuePair<WorksheetCellFormatData, int> pair in formatCounts)
			{
				if (requiredNumberOfMatchingFormats <= pair.Value)
				{
					WorksheetCellFormatData totalCellFormat = pair.Key.CloneInternal();
					totalCellFormat.ResetValue(CellFormatValue.BottomBorderColorInfo);
					totalCellFormat.ResetValue(CellFormatValue.BottomBorderStyle);
					this.AreaFormats[WorksheetTableColumnArea.TotalCell].SetFormatting(totalCellFormat);
					break;
				}
			}

			if (dataAreaFormat != null)
				this.AreaFormats[WorksheetTableColumnArea.DataArea].SetFormatting(dataAreaFormat);
		}

		#endregion // InitializeAreaFormats

		#region InitializeSerializationManager

		internal void InitializeSerializationManager(WorkbookSerializationManager manager)
		{
			IColorSortCondition colorSortCondition = this.SortCondition as IColorSortCondition;
			if (colorSortCondition == null)
				_sortConditionDxfIdDuringSave = 0;
			else
				_sortConditionDxfIdDuringSave = colorSortCondition.AddDxfToManager(manager);
		}

		#endregion // InitializeSerializationManager

		#region OnFilterModified

		internal void OnFilterModified()
		{
			if (this.Table != null)
				this.Table.ReapplyFilters();
		}

		#endregion // OnFilterModified

		#region OnHeaderRowHiding

		internal void OnHeaderRowHiding()
		{
			this.VerifyNameCache();
		}

		#endregion // OnHeaderRowHiding

		#region OnHeaderRowShown

		internal void OnHeaderRowShown()
		{
			if (_areaFormats != null)
			{
				this.SynchronizeAreaFormatWithCells(
					_areaFormats.GetFormatProxy(this.Table.Workbook, WorksheetTableColumnArea.HeaderCell),
					WorksheetCellFormatData.AllCellFormatValues);
			}
		}

		#endregion // OnHeaderRowShown

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnRemovedFromTable

		internal void OnRemovedFromTable()
		{
			_table = null;
			_absoluteColumnIndex = -1;
			this.ResetCache();
		}

		#endregion // OnRemovedFromTable

		#region OnRooted

		internal void OnRooted(Workbook workbook)
		{
			if (_areaFormats != null)
				_areaFormats.OnRooted(workbook);

			this.SetInitialColumnFormula(_columnFormula);
		}

		#endregion // OnRooted

		#region OnTableRemovedFromCollection

		internal void OnTableRemovedFromCollection()
		{
			if (_columnFormula != null)
			{
				Debug.Assert(_columnFormula.OwningCellRow == null, "This formula should be owned at this time.");
				_columnFormula.SetOwningCell(null, -1);
			}

			_appliedColumnFormulas = null;
		}

		#endregion // OnTableRemovedFromCollection

		#region OnTotalsRowHiding

		internal void OnTotalsRowHiding()
		{
			this.VerifyTotalCache();
		}

		#endregion // OnTotalsRowHiding

		#region OnTotalsRowShown

		internal void OnTotalsRowShown()
		{
			if (_areaFormats != null)
			{
				this.SynchronizeAreaFormatWithCells(
					_areaFormats.GetFormatProxy(this.Table.Workbook, WorksheetTableColumnArea.TotalCell),
					WorksheetCellFormatData.AllCellFormatValues);
			}
		}

		#endregion // OnTotalsRowShown

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region ResetCache

		internal void ResetCache()
		{
			_cachedIndex = -1;
		}

		#endregion // ResetCache

		#region SetInitialColumnFormula

		internal void SetInitialColumnFormula(Formula columnFormula)
		{
			_columnFormula = columnFormula;

			if (_columnFormula != null)
			{
				Debug.Assert(_columnFormula.OwningCellRow == null, "This formula should not be owned at this time.");

				WorksheetRegion dataAreaRegion = this.Table.DataAreaRegion;

				if (dataAreaRegion == null || dataAreaRegion.Worksheet == null)
				{
					Utilities.DebugFail("This is unexpected");
					return;
				}

				WorksheetRow rowOfTopDataCell = dataAreaRegion.TopRow;
				_columnFormula.SetOwningCell(rowOfTopDataCell, this.WorksheetColumnIndex);

				for (int rowIndex = dataAreaRegion.FirstRow; rowIndex <= dataAreaRegion.LastRow; rowIndex++)
				{
					WorksheetRow row = dataAreaRegion.Worksheet.Rows[rowIndex];
					Formula cellFormula = row.GetCellFormulaInternal(this.WorksheetColumnIndex);

					if (cellFormula != null && _columnFormula.IsEquivalentTo(cellFormula))
					{
						if (_appliedColumnFormulas == null)
							_appliedColumnFormulas = new List<Formula>();

						_appliedColumnFormulas.Add(cellFormula);
					}
				}
			}
		}

		#endregion // SetInitialColumnFormula

		#endregion // Internal Methods

		#region Private Methods

		#region ClearAppliedColumnFormulas

		private void ClearAppliedColumnFormulas()
		{
			if (_appliedColumnFormulas == null)
				return;

			for (int i = 0; i < _appliedColumnFormulas.Count; i++)
			{
				Formula appliedFormula = _appliedColumnFormulas[i];

				WorksheetRow row = appliedFormula.OwningCellRow;
				short columnIndex = appliedFormula.OwningCellColumnIndex;

				if (row == null)
				{
					Utilities.DebugFail("This formula is in the _appliedColumnFormulas list, but is not applied anywhere.");
					continue;
				}

				// Remove the formula from the cell.
				row.SetCellValueRaw(columnIndex, null);
			}

			_appliedColumnFormulas = null;
		}

		#endregion // ClearAppliedColumnFormulas

		#region SynchronizeAreaFormatWithCells

		private void SynchronizeAreaFormatWithCells(WorksheetTableAreaFormatProxy<WorksheetTableColumnArea> areaFormatProxy, IList<CellFormatValue> values)
		{
			if (areaFormatProxy == null || areaFormatProxy.IsEmpty)
				return;

			if (this.Table == null)
				return;

			Workbook workbook = this.Table.Workbook;
			if (workbook != null && workbook.IsLoading)
				return;

			WorksheetTableColumnArea area = areaFormatProxy.Area;

			WorksheetRegion areaRegion = null;
			WorksheetCell areaCell = null;

			switch (area)
			{
				case WorksheetTableColumnArea.DataArea:
					areaRegion = this.DataAreaRegion;
					break;

				case WorksheetTableColumnArea.HeaderCell:
					areaCell = this.HeaderCell;
					break;

				case WorksheetTableColumnArea.TotalCell:
					areaCell = this.TotalCell;
					break;

				default:
					Utilities.DebugFail("Unknown WorksheetTableColumnArea: " + area);
					return;
			}

			if ((areaRegion == null || areaRegion.Worksheet == null) &&
				(areaCell == null || areaCell.Row == null))
			{
				return;
			}

			if (areaRegion != null)
			{
				List<CellFormatValue> nonDefaultValues = new List<CellFormatValue>();
				for (int i = 0; i < values.Count; i++)
				{
					CellFormatValue value = values[i];
					if (WorksheetTableColumn.CanAreaFormatValueBeSet(area, value) && areaFormatProxy.IsValueDefault(value) == false)
						nonDefaultValues.Add(value);
				}

				if (nonDefaultValues.Count == 0)
					return;

				for (int rowIndex = areaRegion.FirstRow; rowIndex <= areaRegion.LastRow; rowIndex++)
				{
					WorksheetRow row = areaRegion.Worksheet.Rows[rowIndex];
					for (short columnIndex = areaRegion.FirstColumnInternal;
						columnIndex <= areaRegion.LastColumnInternal;
						columnIndex++)
					{
						Utilities.CopyCellFormatValues(areaFormatProxy, row.GetCellFormatInternal(columnIndex), nonDefaultValues);
					}
				}
			}
			else if (areaCell != null)
			{
				Utilities.CopyCellFormatValues(areaFormatProxy, areaCell.CellFormatInternal, values);
			}
		}

		#endregion // SynchronizeAreaFormatWithCells

		#region VerifyAreaFormatValueCanBeSet

		private static void VerifyAreaFormatValueCanBeSet(WorksheetTableColumnArea area, CellFormatValue value)
		{
			string message;
			if (WorksheetTableColumn.VerifyAreaFormatValueCanBeSetHelper(area, value, out message) == false)
				throw new InvalidOperationException(message);
		}

		private static bool VerifyAreaFormatValueCanBeSetHelper(WorksheetTableColumnArea area, CellFormatValue value, out string message)
		{
			switch (area)
			{
				case WorksheetTableColumnArea.DataArea:
					message = null;
					return true;

				case WorksheetTableColumnArea.HeaderCell:
					switch (value)
					{
						case CellFormatValue.TopBorderColorInfo:
						case CellFormatValue.TopBorderStyle:
							message = SR.GetString("LE_InvalidOperationException_InvalidHeaderCellColumnAreaFormatProperty");
							return false;
					}

					message = null;
					return true;

				case WorksheetTableColumnArea.TotalCell:
					switch (value)
					{
						case CellFormatValue.BottomBorderColorInfo:
						case CellFormatValue.BottomBorderStyle:
							message = SR.GetString("LE_InvalidOperationException_InvalidTotalsCellColumnAreaFormatProperty");
							return false;
					}

					message = null;
					return true;

				default:
					Utilities.DebugFail("Unknown WorksheetTableColumnArea: " + area);
					goto case WorksheetTableColumnArea.DataArea;
			}
		}

		#endregion // VerifyAreaFormatValueCanBeSet

		#region VerifyNameCache

		private void VerifyNameCache()
		{
			WorksheetCell headerCell = this.HeaderCell;
			if (headerCell != null)
			{
				GetCellTextParameters parameters = new GetCellTextParameters(headerCell.ColumnIndexInternal);
				parameters.TextFormatMode = TextFormatMode.IgnoreCellWidth;
				parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.String;
				_name = headerCell.Row.GetCellTextInternal(parameters);
			}
		}

		#endregion // VerifyNameCache

		#region VerifyTotalCache

		private void VerifyTotalCache()
		{
			WorksheetCell totalCell = this.TotalCell;

			if (totalCell != null)
			{
				_totalFormula = totalCell.Formula;
				if (_totalFormula == null && totalCell.Value != null)
				{
					// The total label will return the non-formatted text for strings, but the formatted text for anything else.
					GetCellTextParameters parameters = new GetCellTextParameters(totalCell.ColumnIndexInternal);
					parameters.TextFormatMode = TextFormatMode.IgnoreCellWidth;
					parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.String;
					_totalLabel = totalCell.Row.GetCellTextInternal(parameters);
				}
				else
				{
					_totalLabel = null;
				}
			}
		}

		#endregion // VerifyTotalCache

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region AreaFormats

		/// <summary>
		/// Gets the collection of formats used for each area of the column.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The available areas of the column which can have a format set are the header, data, and totals areas.
		/// </p>
		/// <p class="body">
		/// Applying a format to an area will apply the format to all cells in that area.
		/// </p>
		/// <p class="body">
		/// If any area formats on the columns are set when the table is resized to give it more rows, the new cells in the column will get the
		/// new format applied.
		/// </p>
		/// </remarks>
		/// <seealso cref="Filter"/>
		/// <seealso cref="WorksheetTable.AreaFormats"/>
		/// <seealso cref="WorksheetTableStyle.AreaFormats"/>
		
		///// <seealso cref="WorksheetTable.Resize"/>
		public WorksheetTableAreaFormatsCollection<WorksheetTableColumnArea> AreaFormats
		{
			get
			{
				if (_areaFormats == null)
					_areaFormats = new WorksheetTableAreaFormatsCollection<WorksheetTableColumnArea>(this);

				return _areaFormats;
			}
		}

		#endregion // AreaFormats

		#region ColumnFormula

		/// <summary>
		/// Gets the formula associated with the data area of the column.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When the column formula is set and the table is resized to give it more rows, the new cells in the column will have the column formula 
		/// applied to them.
		/// </p>
		/// </remarks>
		/// <value>A <see cref="Formula"/> instance representing the formula for the data area of the column or null if no formula is applied.</value>
		/// <seealso cref="SetColumnFormula"/>
		public Formula ColumnFormula
		{
			get { return _columnFormula; }
		}

		#endregion // ColumnFormula

		#region DataAreaRegion

		/// <summary>
		/// Gets the <see cref="WorksheetRegion"/> which represents the region of cells in the data area of the column.
		/// </summary>
		public WorksheetRegion DataAreaRegion
		{
			get
			{
				if (_table == null)
					return null;

				return this.GetColumnPortionOfTableRegion(_table.DataAreaRegion);
			}
		}

		#endregion // DataAreaRegion

		#region Filter

		/// <summary>
		/// Gets the filter applied to the column.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Filters are not constantly evaluated as data within the table changes. Filters are applied to the table only when they are 
		/// added or removed on a column in the table or when the <see cref="WorksheetTable.ReapplyFilters"/> method is called.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> When the filters are reevaluated, the rows of any cells which don't meet the filter criteria of their column will 
		/// be hidden. When a row is filtered out, the entire row is hidden from the worksheet, so any data outside the table but in the 
		/// same row will also be hidden.
		/// </p>
		/// </remarks>
		/// <value>
		/// A <see cref="Filter"/>-derived instance if a filter is applied or null if the column is not filtered.
		/// </value>
		/// <seealso cref="ApplyAverageFilter"/>
		/// <seealso cref="ApplyCustomFilter(CustomFilterCondition)"/>
		/// <seealso cref="ApplyCustomFilter(CustomFilterCondition,CustomFilterCondition,ConditionalOperator)"/>
		/// <seealso cref="ApplyDatePeriodFilter"/>
		/// <seealso cref="ApplyFontColorFilter(Color)"/>
		/// <seealso cref="ApplyFontColorFilter(WorkbookColorInfo)"/>
		/// <seealso cref="ApplyFixedValuesFilter(bool,string[])"/>
		/// <seealso cref="ApplyFixedValuesFilter(bool,IEnumerable&lt;string&gt;)"/>
		/// <seealso cref="ApplyFixedValuesFilter(bool,IEnumerable&lt;FixedDateGroup&gt;)"/>
		/// <seealso cref="ApplyFixedValuesFilter(bool,CalendarType,IEnumerable&lt;FixedDateGroup&gt;)"/>
		/// <seealso cref="ApplyRelativeDateRangeFilter"/>
		/// <seealso cref="ApplyTopOrBottomFilter()"/>
		/// <seealso cref="ApplyTopOrBottomFilter(TopOrBottomFilterType,int)"/>
		/// <seealso cref="ApplyYearToDateFilter"/>
		/// <seealso cref="ClearFilter"/>
		/// <seealso cref="WorksheetTable.ClearFilters"/>
		/// <seealso cref="WorksheetTable.ReapplyFilters"/>
		public Filter Filter
		{
			get { return _filter; }
			internal set
			{
				if (this.Filter == value)
					return;

				if (value != null &&
					this.Table != null &&
					this.Table.IsFilterUIVisible == false)
				{
					Workbook workbook = this.Table.Workbook;
					if (workbook != null && workbook.IsLoading == false)
					{
						if (this.Table.IsHeaderRowVisible)
							throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotApplyFilterWhileUIIsHidden"));
						else
							throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotApplyFilterWhileHeaderRowIsHidden"));
					}
				}

				_filter = value;
				this.OnFilterModified();
			}
		}

		#endregion // Filter

		#region HeaderCell

		/// <summary>
		/// Gets the <see cref="WorksheetCell"/> which represents the header cell for the column.
		/// </summary>
		/// <value>
		/// A WorksheetCell which represents the header cell for the column or null if the header row is not visible in the table.
		/// </value>
		/// <seealso cref="WorksheetTable.IsHeaderRowVisible"/>
		public WorksheetCell HeaderCell
		{
			get
			{
				WorksheetTable table = this.Table;
				if (table == null)
					return null;

				WorksheetRow headerRow = table.HeaderRow;
				if (headerRow == null)
					return null;

				return headerRow.Cells[this.WorksheetColumnIndex];
			}
		}

		#endregion // HeaderCell

		#region Index

		/// <summary>
		/// Gets the 0-based index of the column in the owning <see cref="WorksheetTable.Columns"/> collection.
		/// </summary>
		/// <value>
		/// The 0-based index of the column in its collection or -1 if the column has been removed from the table.
		/// </value>
		/// <seealso cref="WorksheetTable.Columns"/>
		public int Index
		{
			get
			{
				if (_table == null)
					return -1;

				if (_cachedIndex == -1)
					_cachedIndex = _table.Columns.IndexOf(this);

				return _cachedIndex;
			}
		}

		#endregion // Index

		#region Name

		/// <summary>
		/// Gets or sets the name of the column.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the header row is visible in the <see cref="WorksheetTable"/>, the name of the column will be displayed in the cell of the 
		/// column in the header row.
		/// </p>
		/// <p class="body">
		/// When the WorksheetTable is created, the column names will be taken from the cells in the header row. If the table does not 
		/// contain a header row, the column names will be generated.
		/// </p>
		/// <p class="body">
		/// The column names are unique within the owning WorksheetTable. If, when the table is created, there are two or more columns with 
		/// the same name, the second and subsequent duplicate column names will have a number appended to make them unique. If any cells in 
		/// the header row have a non-string value, their value will be changed to a string (the current display text of the cell). If any 
		/// cells in the header row have no value, they will be given a generated column name.
		/// </p>
		/// <p class="body">
		/// If the Name property is set to a null or empty string, a column name will be generated. If the value is set to a column name which
		/// already exists in the table, the column with the higher index will have a number appended to its name so all column names can stay 
		/// unique.
		/// </p>
		/// </remarks>
		/// <value>The unique name of the column within the owning WorksheetTable.</value>
		/// <seealso cref="WorksheetTable.IsHeaderRowVisible"/>



		public string Name
		{
			get
			{
				// Always update the cached value when the header row is being displayed because the cell value can change any any time.
				this.VerifyNameCache();
				return _name;
			}
			set
			{
				if (this.Name == value)
					return;

				_name = value;

				WorksheetCell headerCell = this.HeaderCell;
				if (headerCell != null)
					headerCell.Value = value;
			}
		}

		#endregion // Name

		#region SortCondition

		/// <summary>
		/// Gets or sets the sort condition used to sort the column in the table.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When a sort condition is set on the column, the SortConditions collection on the <see cref="WorksheetTable.SortSettings"/> will be cleared 
		/// and the new sort condition will be added. To sort by multiple columns, the sort conditions must be added to the SortConditions collection 
		/// instead of set on the column. However, if a sort condition is cleared with this property, just the sort condition for the column will be 
		/// removed from the SortConditions collection. All other SortConditions will remain in the collection.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> Sort conditions are not constantly evaluated as data within the table changes. Sort conditions are applied to the table 
		/// only when they are are added or removed on a column in the table or when the <see cref="WorksheetTable.ReapplySortConditions"/> method 
		/// is called.
		/// </p>
		/// </remarks>
		/// <value>The <see cref="SortCondition"/>-derived instance used to sort the column or null of the column is not sorted.</value>
		/// <seealso cref="WorksheetTable.SortSettings"/>
		/// <seealso cref="SortSettings&lt;T&gt;.SortConditions"/>
		/// <seealso cref="SortConditionCollection&lt;T&gt;"/>
		/// <seealso cref="WorksheetTable.ClearSortConditions"/>
		/// <seealso cref="WorksheetTable.ReapplySortConditions"/>
		public SortCondition SortCondition
		{
			get
			{
				if (_table == null)
					return null;

				return _table.SortSettings.SortConditions[this];
			}
			set
			{
				if (this.SortCondition == value)
					return;

				if (_table != null)
				{
					if (value == null)
					{
						_table.SortSettings.SortConditions[this] = null;
					}
					else
					{
						_table.SortSettings.SortConditions.Clear();
						_table.SortSettings.SortConditions.Add(this, value);
					}
				}
			}
		}

		#endregion // SortCondition

		#region Table

		/// <summary>
		/// Gets the <see cref="WorksheetTable"/> to which the column belongs.
		/// </summary>
		/// <value>
		/// The WorksheetTable to which the column belongs or null if the column has been removed from the table.
		/// </value>
		public WorksheetTable Table
		{
			get { return _table; }
		}

		#endregion // Table

		#region TotalCell

		/// <summary>
		/// Gets the <see cref="WorksheetCell"/> which represents the total cell for the column.
		/// </summary>
		/// <value>
		/// A WorksheetCell which represents the total cell for the column or null if the totals row is not visible in the table.
		/// </value>
		/// <seealso cref="WorksheetTable.IsTotalsRowVisible"/>
		public WorksheetCell TotalCell
		{
			get
			{
				WorksheetTable table = this.Table;
				if (table == null)
					return null;

				WorksheetRow totalsRow = table.TotalsRow;
				if (totalsRow == null)
					return null;

				return totalsRow.Cells[this.WorksheetColumnIndex];
			}
		}

		#endregion // TotalCell

		#region TotalFormula

		/// <summary>
		/// Gets or sets the formula to use in the total cell of the column.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The total formula can be set regardless of whether or not the totals row is visible. If the totals row is hidden, the
		/// formula will not be applied anywhere. When the totals row is visible, it will be applied to the total cell of the column.
		/// </p>
		/// <p class="body">
		/// Setting the TotalFormula to a non-null value will clear the <see cref="TotalLabel"/>, and vice versa.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The value is already applied to something else, such as a cell or table column.
		/// </exception>
		/// <seealso cref="TotalLabel"/>
		/// <seealso cref="WorksheetTable.IsTotalsRowVisible"/>



		public Formula TotalFormula
		{
			get
			{
				this.VerifyTotalCache();
				return _totalFormula;
			}
			set
			{
				Formula currentTotalFormula = this.TotalFormula;
				if (currentTotalFormula == value)
					return;

				_totalFormula = value;

				WorksheetCell totalCell = this.TotalCell;
				if (totalCell != null)
				{
					if (_totalFormula == null)
					{
						if (totalCell.Formula != null)
						{
							Debug.Assert(
								totalCell.Formula == currentTotalFormula,
								"The current cell's formula should be the same as the total formula.");

							totalCell.Value = null;
						}
					}
					else
					{
						_totalFormula.ApplyTo(totalCell);
					}
				}

				if (_totalFormula != null)
					this.TotalLabel = null;
			}
		}

		#endregion // TotalFormula

		#region TotalLabel

		/// <summary>
		/// Gets or sets the text label to use in the total cell of the column.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The total label can be set regardless of whether or not the totals row is visible. If the totals row is hidden, the
		/// label will not be displayed anywhere. When the totals row is visible, it will be set as the value of the total cell of the column.
		/// </p>
		/// <p class="body">
		/// Setting the <see cref="TotalFormula"/> to a non-null value will clear the TotalLabel, and vice versa.
		/// </p>
		/// </remarks>
		/// <seealso cref="TotalFormula"/>
		/// <seealso cref="WorksheetTable.IsTotalsRowVisible"/>



		public string TotalLabel
		{
			get
			{
				this.VerifyTotalCache();
				return _totalLabel;
			}
			set
			{
				string currentTotalLabel = this.TotalLabel;
				if (currentTotalLabel == value)
					return;

				_totalLabel = value;

				WorksheetCell totalCell = this.TotalCell;
				if (totalCell != null && _totalLabel != null)
					totalCell.Value = _totalLabel;

				if (_totalLabel != null)
					this.TotalFormula = null;
			}
		}

		#endregion // TotalLabel

		#region WholeColumnRegion

		/// <summary>
		/// Gets the <see cref="WorksheetRegion"/> which represents the region of cells in the whole column, including the header and total cells, 
		/// if visible.
		/// </summary>
		public WorksheetRegion WholeColumnRegion
		{
			get
			{
				if (_table == null)
					return null;

				return this.GetColumnPortionOfTableRegion(_table.WholeTableRegion);
			}
		}

		#endregion // WholeColumnRegion

		#endregion // Public Properties

		#region Internal Properties

		#region Id

		internal uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		#endregion // Id

		#region NameInternal

		internal string NameInternal
		{
			get { return _name; }
			set { _name = value; }
		}

		#endregion // NameInternal

		#region SortConditionDxfIdDuringSave

		internal uint SortConditionDxfIdDuringSave
		{
			get { return _sortConditionDxfIdDuringSave; }
		}

		#endregion // SortConditionDxfIdDuringSave

		#region SortRegion

		internal WorksheetRegion SortRegion
		{
			get
			{
				if (_table == null)
					return null;

				return this.GetColumnPortionOfTableRegion(_table.SortAndHeadersRegion);
			}
		}

		#endregion // SortRegion

		#region TotalLabelInternal

		internal string TotalLabelInternal
		{
			get { return _totalLabel; }
		}

		#endregion // TotalLabelInternal

		#region TotalFormulaInternal

		internal Formula TotalFormulaInternal
		{
			get { return _totalFormula; }
		}

		#endregion // TotalFormulaInternal

		#region Workbook

		internal Workbook Workbook
		{
			get
			{
				if (_table == null)
					return null;

				return _table.Workbook;
			}
		}

		#endregion // Workbook

		#region Worksheet

		internal Worksheet Worksheet
		{
			get
			{
				if (_table == null)
					return null;

				return _table.Worksheet;
			}
		}

		#endregion // Worksheet

		#region WorksheetColumnIndex

		internal short WorksheetColumnIndex
		{
			get { return _absoluteColumnIndex; }
		}

		#endregion // WorksheetColumnIndex

		#endregion // Internal Properties

		#endregion // Properties
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