using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization.Excel2007;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a filter which can filter dates in a specific period.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// This filter allows dates to be filtered in if they are in a specific month or quarter of any year.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	/// <seealso cref="WorksheetTableColumn.ApplyDatePeriodFilter"/>
	/// <seealso cref="WorksheetTableColumn.ApplyDatePeriodFilter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class DatePeriodFilter : DynamicValuesFilter
	{
		#region Member Variables

		private DatePeriodFilterType _type;
		private int _value;

		#endregion // Member Variables

		#region Constructor

		internal DatePeriodFilter(WorksheetTableColumn owner, DatePeriodFilterType type, int value)
			: base(owner)
		{
			Utilities.VerifyEnumValue(type);
			DatePeriodFilter.ValidatedValue(type, value);

			_type = type;
			_value = value;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			DatePeriodFilter other = filter as DatePeriodFilter;
			if (other == null)
				return false;

			return
				_value == other._value &&
				_type == other._type;
		}

		#endregion // HasSameData

		#region MeetsCriteria

		internal override bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex)
		{
			DateTime value;
			if (worksheet.TryGetDateTimeFromCell(row, columnIndex, out value))
			{
				switch (this.Type)
				{
					case DatePeriodFilterType.Month:
						return value.Month == this.Value;

					case DatePeriodFilterType.Quarter:
						return Utilities.GetQuarter(value) == this.Value;

					default:
						Utilities.DebugFail("Unknown DatePeriodFilterType: " + this.Type);
						break;
				}
			}

			return false;
		}

		#endregion // MeetsCriteria

		#region Type2007

		internal override ST_DynamicFilterType Type2007
		{
			get
			{
				switch (this.Type)
				{
					case DatePeriodFilterType.Month:
						return ST_DynamicFilterType.M1 + (this.Value - 1);

					case DatePeriodFilterType.Quarter:
						return ST_DynamicFilterType.Q1 + (this.Value - 1);

					default:
						Utilities.DebugFail("Unknown DatePeriodFilterType: " + this.Type);
						return ST_DynamicFilterType._null;
				}
			}
		}

		#endregion // Type2007

		#endregion // Base Class Overrides

		#region Methods

		#region ValidatedValue

		private static void ValidatedValue(DatePeriodFilterType type, int value)
		{
			bool isValueInRange = true;
			switch (type)
			{
				case DatePeriodFilterType.Month:
					if (value < 1 || 12 < value)
						isValueInRange = false;
					break;

				case DatePeriodFilterType.Quarter:
					if (value < 1 || 4 < value)
						isValueInRange = false;
					break;

				default:
					Utilities.DebugFail("Unknown DatePeriodFilterType: " + type);
					break;
			}

			if (isValueInRange == false)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidDatePeriodFilterValue"), "value");
		}

		#endregion // ValidatedValue

		#endregion // Methods

		#region Properties

		#region Type

		/// <summary>
		/// Gets or sets the type of date period to filter in.
		/// </summary>
		/// <value>
		/// Month to filter in dates in a specific month of any year; Quarter to filter in dates in a specific quarter of any year.
		/// </value>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="DatePeriodFilterType"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is Quarter and the <see cref="Value"/> is less than 1 or greater than 4 or
		/// the value assigned is Month and the Value is less than 1 or greater than 12.
		/// </exception>
		/// <seealso cref="Value"/>
		public DatePeriodFilterType Type
		{
			get { return _type; }
			set
			{
				if (this.Type == value)
					return;

				Utilities.VerifyEnumValue(value);
				DatePeriodFilter.ValidatedValue(value, this.Value);
				_type = value;
				this.OnModified();
			}
		}

		#endregion // Type

		#region Value

		/// <summary>
		/// Gets or sets the 1-based value of the month or quarter to filter in.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the <see cref="Type"/> is Month, a Value of 1 indicates January, 2 indicates February, and so on. If Type is Quarter,
		/// a Value of 1 indicates Quarter 1, and so on.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// <see cref="Type"/> is Quarter and the value assigned is less than 1 or greater than 4 or
		/// Type is Month and the value assigned is less than 1 or greater than 12.
		/// </exception>
		/// <value>The 1-based value of the month or quarter to filter in.</value>
		/// <seealso cref="Type"/>
		public int Value
		{
			get { return _value; }
			set
			{
				if (this.Value == value)
					return;

				DatePeriodFilter.ValidatedValue(this.Type, value);
				_value = value;
				this.OnModified();
			}
		}

		#endregion // Value

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