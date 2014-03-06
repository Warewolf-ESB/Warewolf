using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.Serialization.Excel2007;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a filter which can filter in cells in the upper or lower portion of the sorted values.
	/// </summary>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	/// <seealso cref="WorksheetTableColumn.ApplyTopOrBottomFilter()"/>
	/// <seealso cref="WorksheetTableColumn.ApplyTopOrBottomFilter(TopOrBottomFilterType,int)"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class TopOrBottomFilter : DynamicValuesFilter
	{
		#region Member Variables

		private double _referenceValue;
		private TopOrBottomFilterType _type;
		private int _value;

		#endregion // Member Variables

		#region Constructor

		internal TopOrBottomFilter(WorksheetTableColumn owner)
			: this(owner, TopOrBottomFilterType.TopValues, 10) { }

		internal TopOrBottomFilter(WorksheetTableColumn owner, TopOrBottomFilterType type, int value)
			: base(owner)
		{
			Utilities.VerifyEnumValue(type);
			TopOrBottomFilter.ValidateValue(value);

			_type = type;
			_value = value;
		}

		internal TopOrBottomFilter(WorksheetTableColumn owner, TopOrBottomFilterType type, int value, double referenceValue)
			: this(owner, type, value)
		{
			_referenceValue = referenceValue;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			TopOrBottomFilter other = filter as TopOrBottomFilter;
			if (other == null)
				return false;

			return
				_referenceValue == other._referenceValue &&
				_type == other._type &&
				_value == other._value;
		}

		#endregion // HasSameData

		#region MeetsCriteria

		internal override bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex)
		{
			object value = WorksheetRow.GetCellValue(row, columnIndex);

			double numericValue;
			if (Utilities.TryGetNumericValue(worksheet.Workbook, value, out numericValue))
			{
				if (this.IsTop)
					return _referenceValue <= numericValue;
				else
					return numericValue <= _referenceValue;
			}

			return false;
		}

		#endregion // MeetsCriteria

		#region OnBeforeFilterColumn

		internal override bool OnBeforeFilterColumn(Worksheet worksheet, int firstRowIndex, int lastRowIndex, short columnIndex)
		{
			List<double> sortedValues = new List<double>();

			Workbook workbook = worksheet.Workbook;

			foreach (WorksheetRow row in worksheet.Rows.GetItemsInRange(firstRowIndex, lastRowIndex))
			{
				object value = row.GetCellValueInternal(columnIndex);

				ErrorValue errorValue = value as ErrorValue;
				if (errorValue != null && errorValue != ErrorValue.Circularity)
					return false;

				double numericValue;
				if (Utilities.TryGetNumericValue(workbook, value, out numericValue))
					sortedValues.Add(numericValue);
			}

			if (sortedValues.Count == 0)
				return true;

			sortedValues.Sort();

			int numberOfValues;
			if (this.IsPercent)
				numberOfValues = (sortedValues.Count * Math.Min(100, this.Value)) / 100;
			else
				numberOfValues = Math.Min(sortedValues.Count, this.Value);

			if (numberOfValues == 0)
			{
				Utilities.DebugFail("This is unexpected.");
				return true;
			}

			if (this.IsTop)
				_referenceValue = sortedValues[sortedValues.Count - numberOfValues];
			else
				_referenceValue = sortedValues[numberOfValues - 1];

			return true;
		}

		#endregion // OnBeforeFilterColumn

		#region ShouldSaveIn2003Formats

		internal override bool ShouldSaveIn2003Formats(out bool needsAUTOFILTER12Record, out IList<string> allowedTextValues)
		{
			needsAUTOFILTER12Record = false;
			allowedTextValues = null;
			return true;
		}

		#endregion // ShouldSaveIn2003Formats

		#region Type2007

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal override ST_DynamicFilterType Type2007
		{
			get
			{
				Utilities.DebugFail("This should never be called.");
				return ST_DynamicFilterType._null;
			}
		}

		#endregion // Type2007

		#endregion // Base Class Overrides

		#region Methods

		#region Internal Methods

		#region GetFilterType

		internal static TopOrBottomFilterType GetFilterType(bool percent, bool top)
		{
			if (percent)
			{
				if (top)
					return TopOrBottomFilterType.TopPercentage;

				return TopOrBottomFilterType.BottomPercentage;
			}

			if (top)
				return TopOrBottomFilterType.TopValues;

			return TopOrBottomFilterType.BottomValues;
		}

		#endregion // GetFilterType

		#endregion // Internal Methods

		#region Private Methods

		#region ValidateValue

		private static void ValidateValue(int value)
		{
			if (value < 1 || 500 < value)
				throw new ArgumentOutOfRangeException("value", value, SR.GetString("LE_ArgumentException_InvalidTopOrBottomFilterValue"));
		}

		#endregion // ValidateValue

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Type

		/// <summary>
		/// Gets or sets the type of the filter.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assign is not defined in the <see cref="TopOrBottomFilterType"/> enumeration.
		/// </exception>
		/// <value>
		/// TopValues or BottomValues to filter in the top or bottom N value in the list of sorted values; TopPercentage or BottomPercentage 
		/// to filter in the top or bottom N percentage of values in the list of sorted values.
		/// </value>
		/// <seealso cref="Value"/>
		public TopOrBottomFilterType Type
		{
			get { return _type; }
			set
			{
				if (this.Type == value)
					return;

				Utilities.VerifyEnumValue(value);
				_type = value;
				this.OnModified();
			}
		}

		#endregion // Type

		#region Value

		/// <summary>
		/// Gets or sets the number or percentage of value of values which should be filtered in.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When <see cref="Type"/> is TopValues or BottomValues, Value indicates the number of value which should be filtered in.
		/// When Type is TopPercentage or BottomPercentage, Value indicates percentage to filter in. For example, a Value of 15 and
		/// a Type of BottomPercentage will filter in the bottom 15 percent of values.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is less than 1 or greater than 500.
		/// </exception>
		/// <value>The number or percentage of items which should be filtered in.</value>
		public int Value
		{
			get { return _value; }
			set
			{
				if (this.Value == value)
					return;

				TopOrBottomFilter.ValidateValue(value);
				_value = value;
				this.OnModified();
			}
		}

		#endregion // Value

		#endregion // Public Properties

		#region Internal Properties

		#region FilterOperator

		internal ST_FilterOperator FilterOperator
		{
			get
			{
				if (this.IsTop)
					return ST_FilterOperator.greaterThanOrEqual;

				return ST_FilterOperator.lessThanOrEqual;
			}
		}

		#endregion // FilterOperator

		#region IsPercent

		internal bool IsPercent
		{
			get
			{
				return
					this.Type == TopOrBottomFilterType.BottomPercentage ||
					this.Type == TopOrBottomFilterType.TopPercentage;
			}
		}

		#endregion // IsPercent

		#region IsTop

		internal bool IsTop
		{
			get
			{
				return
					this.Type == TopOrBottomFilterType.TopValues ||
					this.Type == TopOrBottomFilterType.TopPercentage;
			}
		}

		#endregion // IsTop

		#region ReferenceValue

		internal double ReferenceValue
		{
			get { return _referenceValue; }
		}

		#endregion // ReferenceValue

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