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


namespace Infragistics.Documents.Excel.Sorting
{
	// MD 12/14/11 - 12.1 - Table Support



	/// <summary>
	/// Represents an ordered sort condition, which can sort data in either an ascending or descending manner.
	/// </summary>
	/// <seealso cref="SortSettings&lt;T&gt;.SortConditions"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class OrderedSortCondition : SortCondition
	{
		#region Constructor

		/// <summary>
		/// Creates a new <see cref="OrderedSortCondition"/> instance with the ascending sort direction.
		/// </summary>
		public OrderedSortCondition() { }

		/// <summary>
		/// Creates a new <see cref="OrderedSortCondition"/> instance.
		/// </summary>
		/// <param name="sortDirection">The direction with which to sort the data.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="sortDirection"/> is not defined in the <see cref="SortDirection"/> enumeration.
		/// </exception>
		public OrderedSortCondition(SortDirection sortDirection)
			: base(sortDirection) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region CompareCells

		internal override int CompareCells<T>(SortSettings<T> owner, Worksheet worksheet, int rowIndexX, int rowIndexY, short columnIndex)
		{
			WorksheetRow rowX = worksheet.Rows.GetIfCreated(rowIndexX);
			WorksheetRow rowY = worksheet.Rows.GetIfCreated(rowIndexY);

			object valueX = WorksheetRow.GetCellValue(rowX, columnIndex);
			object valueY = WorksheetRow.GetCellValue(rowY, columnIndex);

			// Blanks always go to the bottom, regardless of sort direction.
			if (valueX == null)
			{
				if (valueY == null)
					return 0;

				return 1;
			}
			else if (valueY == null)
			{
				return -1;
			}

			int result = OrderedSortCondition.CompareValues(owner, worksheet, valueX, valueY);
			if (this.SortDirection == SortDirection.Descending)
				return -result;

			return result;
		}

		#endregion // CompareCells

		#region SortByValue

		internal override ST_SortBy SortByValue
		{
			get { return ST_SortBy.value; }
		}

		#endregion // SortByValue

		#endregion // Base Class Overrides

		#region Methods

		#region CompareValues

		internal static int CompareValues<T>(SortSettings<T> owner, Worksheet worksheet, object valueX, object valueY) where T : ISortable
		{
			// Sort order for types: Number, String, Booleans, ErrorValue

			double numericValueX;
			bool isNumberX = Utilities.TryGetNumericValue(worksheet.Workbook, valueX, out numericValueX);

			double numericValueY;
			bool isNumberY = Utilities.TryGetNumericValue(worksheet.Workbook, valueY, out numericValueY);

			if (isNumberX)
			{
				if (isNumberY)
					return numericValueX.CompareTo(numericValueY);

				return -1;
			}
			else if (isNumberY)
			{
				return 1;
			}

			bool isBooleanX = valueX is bool;
			bool isBooleanY = valueY is bool;

			string stringValueX = null;
			if (valueX is ErrorValue == false && isBooleanX == false)
				stringValueX = valueX.ToString();

			string stringValueY = null;
			if (valueY is ErrorValue == false && isBooleanY == false)
				stringValueY = valueY.ToString();

			if (stringValueX != null)
			{
				if (stringValueY != null)
					return String.Compare(stringValueX, stringValueY, owner.SortCulture, owner.CaseSensitive ? CompareOptions.None : CompareOptions.IgnoreCase);

				return -1;
			}
			else if (stringValueY != null)
			{
				return 1;
			}

			if (isBooleanX)
			{
				if (isBooleanY)
					return ((bool)valueX).CompareTo((bool)valueY);

				return -1;
			}
			else if (isBooleanY)
			{
				return 1;
			}

			// Error values do not have any order to them.
			return 0;
		}

		#endregion // CompareValues

		#endregion // Methods
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