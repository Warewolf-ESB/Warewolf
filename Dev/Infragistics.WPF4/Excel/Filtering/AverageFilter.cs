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
using Infragistics.Documents.Excel.CalcEngine;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a filter which can filter data based on whether the data is below or above the average of the entire data range.
	/// </summary>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class AverageFilter : DynamicValuesFilter
	{
		#region Member Variables

		private double _average;
		private AverageFilterType _type;

		#endregion // Member Variables

		#region Constructor

		internal AverageFilter(WorksheetTableColumn owner, AverageFilterType type)
			: base(owner)
		{
			Utilities.VerifyEnumValue(type);
			_type = type;
		}

		internal AverageFilter(WorksheetTableColumn owner, AverageFilterType type, double average)
			: this(owner, type)
		{
			_average = average;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			AverageFilter other = filter as AverageFilter;
			if (other == null)
				return false;

			return
				_average == other._average &&
				_type == other._type;
		}

		#endregion // HasSameData

		#region MeetsCriteria

		internal override bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex)
		{
			// Note: date values are averaged in as well.

			object value = WorksheetRow.GetCellValue(row, columnIndex);

			double numericValue;
			if (Utilities.TryGetNumericValue(worksheet.Workbook, value, out numericValue))
			{
				switch (this.Type)
				{
					case AverageFilterType.AboveAverage:
						return _average < numericValue;

					case AverageFilterType.BelowAverage:
						return numericValue < _average;

					default:
						Utilities.DebugFail("Unknown AverageFilterType: " + this.Type);
						break;
				}
			}

			return false;
		}

		#endregion // MeetsCriteria

		#region OnBeforeFilterColumn

		internal override bool OnBeforeFilterColumn(Worksheet worksheet, int firstRowIndex, int lastRowIndex, short columnIndex)
		{
			// Note: date values are averaged in as well.

			Workbook workbook = worksheet.Workbook;

			double total = 0;
			int count = 0;
			foreach (WorksheetRow row in worksheet.Rows.GetItemsInRange(firstRowIndex, lastRowIndex))
			{
				object value = row.GetCellValueInternal(columnIndex);

				double numericValue;
				if (Utilities.TryGetNumericValue(workbook, value, out numericValue))
				{
					total += numericValue;
					count++;
				}

				// When there is an error in a cell and an average filter is applied, the calculated average is 0.
				if (value is ErrorValue)
				{
					_average = 0;
					return true;
				}
			}

			if (count == 0)
			{
				_average = 0;
				return true;
			}

			_average = total / count;
			return true;
		}

		#endregion // OnBeforeFilterColumn

		#region Type2007

		internal override ST_DynamicFilterType Type2007
		{
			get
			{
				switch (this.Type)
				{
					case AverageFilterType.AboveAverage:
						return ST_DynamicFilterType.aboveAverage;

					case AverageFilterType.BelowAverage:
						return ST_DynamicFilterType.belowAverage;

					default:
						Utilities.DebugFail("Unknown AverageFilterType: " + this.Type);
						return ST_DynamicFilterType._null;
				}
			}
		}

		#endregion // Type2007

		#endregion // Base Class Overrides

		#region Properties

		#region Average

		/// <summary>
		/// Gets the average that was computed the last time the filter was applied or 0 if any errors or all non-numeric values 
		/// were found when applying the filter.
		/// </summary>
		public double Average
		{
			get { return _average; }
		}

		#endregion // Average

		#region Type

		/// <summary>
		/// Gets or sets the value indicating whether to filter in values below or above the average of the data range.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="AverageFilterType"/> enumeration.
		/// </exception>
		/// <value>AboveAverage to show cells above the average of the data range; BelowAverage to show cells below the average.</value>
		public AverageFilterType Type
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