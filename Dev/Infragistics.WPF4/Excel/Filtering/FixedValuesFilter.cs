using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.CalcEngine;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a filter which can filter cells based on specific, fixed values, which are allowed to display.
	/// </summary>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	/// <seealso cref="WorksheetTableColumn.ApplyFixedValuesFilter(bool,string[])"/>
	/// <seealso cref="WorksheetTableColumn.ApplyFixedValuesFilter(bool,IEnumerable&lt;string&gt;)"/>
	/// <seealso cref="WorksheetTableColumn.ApplyFixedValuesFilter(bool,IEnumerable&lt;FixedDateGroup&gt;)"/>
	/// <seealso cref="WorksheetTableColumn.ApplyFixedValuesFilter(bool,CalendarType,IEnumerable&lt;FixedDateGroup&gt;)"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class FixedValuesFilter : Filter
	{
		#region Member Variables

		private Calendar _calendar;
		private CalendarType _calendarType;
		private FixedDateGroupCollection _dateGroups;
		private DisplayValueCollection _displayValues;
		private bool _includeBlanks;

		#endregion // Member Variables

		#region Constructor

		internal FixedValuesFilter(WorksheetTableColumn owner)
			: base(owner) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			FixedValuesFilter other = filter as FixedValuesFilter;
			if (other == null)
				return false;

			if (_includeBlanks != other._includeBlanks ||
				_calendarType != other._calendarType ||
				this.DisplayValues.Count != other.DisplayValues.Count ||
				this.DateGroups.Count != other.DateGroups.Count)
				return false;

			for (int i = 0; i < this.DisplayValues.Count; i++)
			{
				if (this.DisplayValues[i] != other.DisplayValues[i])
					return false;
			}

			for (int i = 0; i < this.DateGroups.Count; i++)
			{
				if (Object.Equals(this.DateGroups[i], other.DateGroups[i]) == false)
					return false;
			}

			return true;
		}

		#endregion // HasSameData

		#region MeetsCriteria

		internal override bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex)
		{
			double? numericValue;
			ValueFormatter.SectionType formattedAs;
			string text = Filter.GetCellText(row, columnIndex, out numericValue, out formattedAs);
			if (String.IsNullOrEmpty(text))
				return this.IncludeBlanks;

			if (_displayValues != null && _displayValues.Contains(text))
				return true;

			if (numericValue.HasValue &&
				formattedAs == ValueFormatter.SectionType.Date &&
				_dateGroups != null &&
				_dateGroups.Count != 0)
			{
				DateTime? testDateTime = ExcelCalcValue.ExcelDateToDateTime(worksheet.Workbook, numericValue.Value);
				if (testDateTime.HasValue)
				{
					DateTime dateTime = testDateTime.Value;

					for (int i = 0; i < _dateGroups.Count; i++)
					{
						FixedDateGroup dateGroup = _dateGroups[i];

						FixedDateGroup.DateRange range;
						dateGroup.GetRange(this.Calendar, this.CalendarType, out range);
						if (range.Start <= dateTime && dateTime < range.End)
							return true;
					}
				}
			}

			return false;
		}

		#endregion // MeetsCriteria

		#region ShouldSaveIn2003Formats

		internal override bool ShouldSaveIn2003Formats(out bool needsAUTOFILTER12Record, out IList<string> allowedTextValues)
		{
			int itemCount = this.GetAllowedItemCount(true, out allowedTextValues);
			needsAUTOFILTER12Record = this.DateGroups.Count != 0 || itemCount > 2;
			return itemCount != 0;
		}

		#endregion // ShouldSaveIn2003Formats

		#endregion // Base Class Overrides

		#region Methods

		#region GetAllowedItemCount

		internal int GetAllowedItemCount()
		{
			IList<string> allowedTextValues;
			return this.GetAllowedItemCount(false, out allowedTextValues);
		}

		internal int GetAllowedItemCount(bool filterItemsOver255Characters, out IList<string> allowedTextValues)
		{
			allowedTextValues = null;
			int count = 0;
			if (this.IncludeBlanks)
				count++;

			if (_dateGroups != null)
				count += _dateGroups.Count;

			if (_displayValues != null)
			{
				allowedTextValues = new List<string>();

				if (filterItemsOver255Characters)
				{
					for (int i = 0; i < _displayValues.Count; i++)
					{
						string displayValue = _displayValues[i];
						if (displayValue.Length <= 255)
						{
							allowedTextValues.Add(displayValue);
							count++;
						}
					}
				}
				else
				{
					count += _displayValues.Count;
					allowedTextValues = _displayValues;
				}
			}

			return count;
		}

		#endregion // GetAllowedItemCount

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region CalendarType

		/// <summary>
		/// Gets or sets the calendar type used to interpret values in the <see cref="DateGroups"/> collection.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="CalendarType"/> enumeration.
		/// </exception>
		public CalendarType CalendarType
		{
			get { return _calendarType; }
			set
			{
				if (this.CalendarType == value)
					return;

				Utilities.VerifyEnumValue(value);
				_calendarType = value;

				_calendar = null;
				this.OnModified();
			}
		}

		#endregion // CalendarType

		#region DateGroups

		/// <summary>
		/// Gets the collection of fixed date groups which should be filtered in.
		/// </summary>
		/// <seealso cref="DisplayValues"/>
		public FixedDateGroupCollection DateGroups
		{
			get
			{
				if (_dateGroups == null)
					_dateGroups = new FixedDateGroupCollection(this);

				return _dateGroups;
			}
		}

		#endregion // DateGroups

		#region DisplayValues

		/// <summary>
		/// Gets the collection of cell text values which should be filtered in.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Text values are compared case-insensitively.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> If any text values are longer than 255 characters in length and the workbook is saved in one of the 2003 formats,
		/// the correct rows will be hidden in the saved file, but the filter may be missing from the column or reapplying the filter
		/// may hide some of the matching cells.
		/// </p>
		/// </remarks>
		/// <seealso cref="DateGroups"/>
		/// <seealso cref="WorksheetCell.GetText(TextFormatMode)"/>
		/// <seealso cref="WorksheetRow.GetCellText(int,TextFormatMode)"/>
		public DisplayValueCollection DisplayValues
		{
			get
			{
				if (_displayValues == null)
					_displayValues = new DisplayValueCollection(this);

				return _displayValues;
			}
		}

		#endregion // DisplayValues

		#region IncludeBlanks

		/// <summary>
		/// Gets or sets the value which indicates whether blank cells should be filtered in.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The value is set to False and both <see cref="DateGroups"/> and <see cref="DisplayValues"/> contain no values. 
		/// This would prevent the filter from including any values, which is not allowed for a <see cref="FixedValuesFilter"/>.
		/// </exception>
		public bool IncludeBlanks
		{
			get { return _includeBlanks; }
			set
			{
				if (this.IncludeBlanks == value)
					return;

				if (value == false && this.GetAllowedItemCount() == 1)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FixedValuesFilterMustAcceptAValue"));

				_includeBlanks = value;
				this.OnModified();
			}
		}

		#endregion // IncludeBlanks

		#endregion // Public Properties

		#region Internal Properties

		#region Calendar

		internal Calendar Calendar
		{
			get
			{
				if (_calendar == null)
					_calendar = Utilities.CreateCalendar(this.CalendarType);

				return _calendar;
			}
		}

		#endregion // Calendar

		#region IsEmpty

		internal bool IsEmpty
		{
			get { return this.GetAllowedItemCount() == 0; }
		}

		#endregion // IsEmpty

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