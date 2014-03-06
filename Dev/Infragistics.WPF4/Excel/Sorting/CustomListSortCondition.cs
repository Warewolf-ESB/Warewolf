using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization.Excel2007;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Sorting
{
	// MD 12/14/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a sort condition which will sort cells based on a custom, ordered list of values.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// When the sort direction is ascending, the data range is sorted with the cells in the custom list appearing first, in the order they 
	/// appear in the list, followed by the other cells in the same relative order they had to each other before the sort. When the sort direction 
	/// is descending, the cells not in the list will appear first in the data region and they will appear in the same relative order they 
	/// had before the sort. They will be followed by the other cells in the reverse order of the list.
	/// </p>
	/// <p class="body">
	/// The list of values specified on this sort condition are string values. If a cell being sorted has a string value, that value is used to
	/// sort the cell. Otherwise, the cell text is used. For example, if the cell's value is 0.01, but it is formatted as a percentage cell,
	/// the text used to sort it with this sort condition will be "1%" and not "0.01". When using the cell text, if the format string for the 
	/// cell includes padding characters which are repeated across the cells, they will not be included in the cell text used for comparison.
	/// </p>
	/// <p class="body">
	/// When matching values from a cell to values in the custom list, strings are compared case-sensitively or case-insensitively based 
	/// on the <see cref="SortSettings&lt;T&gt;.CaseSensitive"/> setting.
	/// </p>
	/// <p class="body">
	/// If the cell text contains any repeated padding characters, they are ignored when comparing strings.
	/// </p>
	/// </remarks>
	/// <seealso cref="SortSettings&lt;T&gt;.CaseSensitive"/>
	/// <seealso cref="SortSettings&lt;T&gt;.SortConditions"/>
	/// <seealso cref="SortCondition.SortDirection"/>
	/// <seealso cref="WorksheetCell.GetText(TextFormatMode)"/>
	/// <seealso cref="WorksheetRow.GetCellText(int,TextFormatMode)"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

		class CustomListSortCondition : SortCondition
	{
		#region Member Variables

		private readonly ReadOnlyCollection<string> _list;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="CustomListSortCondition"/> instance.
		/// </summary>
		/// <param name="list">The list of values in the order in which they should be sorted.</param>
		/// <remarks>
		/// <p class="body">
		/// When the sort direction is ascending, the data range is sorted with the cells in the custom list appearing first, in the order they 
		/// appear in the list, followed by the other cells in the same relative order they had to each other before the sort. When the sort direction 
		/// is descending, the cells not in the list will appear first in the data region and they will appear in the same relative order they 
		/// had before the sort. They will be followed by the other cells in the reverse order of the list.
		/// </p>
		/// </remarks>
		public CustomListSortCondition(params string[] list)
			: this(SortDirection.Ascending, (IEnumerable<string>)list) { }

		/// <summary>
		/// Creates a new <see cref="CustomListSortCondition"/> instance.
		/// </summary>
		/// <param name="list">The list of values in the order in which they should be sorted.</param>
		/// <remarks>
		/// <p class="body">
		/// When the sort direction is ascending, the data range is sorted with the cells in the custom list appearing first, in the order they 
		/// appear in the list, followed by the other cells in the same relative order they had to each other before the sort. When the sort direction 
		/// is descending, the cells not in the list will appear first in the data region and they will appear in the same relative order they 
		/// had before the sort. They will be followed by the other cells in the reverse order of the list.
		/// </p>
		/// </remarks>
		public CustomListSortCondition(IEnumerable<string> list)
			: this(SortDirection.Ascending, list) { }

		/// <summary>
		/// Creates a new <see cref="CustomListSortCondition"/> instance.
		/// </summary>
		/// <param name="sortDirection">The direction with which to sort the data.</param>
		/// <param name="list">The list of values in the order in which they should be sorted.</param>
		/// <remarks>
		/// <p class="body">
		/// When the sort direction is ascending, the data range is sorted with the cells in the custom list appearing first, in the order they 
		/// appear in the list, followed by the other cells in the same relative order they had to each other before the sort. When the sort direction 
		/// is descending, the cells not in the list will appear first in the data region and they will appear in the same relative order they 
		/// had before the sort. They will be followed by the other cells in the reverse order of the list.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="sortDirection"/> is not defined in the <see cref="SortDirection"/> enumeration.
		/// </exception>
		public CustomListSortCondition(SortDirection sortDirection, params string[] list)
			: this(sortDirection, (IEnumerable<string>)list) { }

		/// <summary>
		/// Creates a new <see cref="CustomListSortCondition"/> instance.
		/// </summary>
		/// <param name="sortDirection">The direction with which to sort the data.</param>
		/// <param name="list">The list of values in the order in which they should be sorted.</param>
		/// <remarks>
		/// <p class="body">
		/// When the sort direction is ascending, the data range is sorted with the cells in the custom list appearing first, in the order they 
		/// appear in the list, followed by the other cells in the same relative order they had to each other before the sort. When the sort direction 
		/// is descending, the cells not in the list will appear first in the data region and they will appear in the same relative order they 
		/// had before the sort. They will be followed by the other cells in the reverse order of the list.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="list"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="list"/> contains no items.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="sortDirection"/> is not defined in the <see cref="SortDirection"/> enumeration.
		/// </exception>
		public CustomListSortCondition(SortDirection sortDirection, IEnumerable<string> list)
			: base(sortDirection)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			// Copy the list so they don't pass us a mutable list and then change it afterwards.
			List<string> copiedList = new List<string>();
			foreach (string item in list)
			{
				if (item == null)
					continue;

				string trimmedValue = item.Trim();
				if (trimmedValue.Length == 0)
					continue;

				copiedList.Add(trimmedValue);
			}

			if (copiedList.Count == 0)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_EmptyCustomList"), "list");

			_list = new ReadOnlyCollection<string>(copiedList);
		}

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

			// Sort order for types: non-Boolean values, Booleans values

			int result;
			if (valueX is bool)
			{
				if (valueY is bool)
					result = ((bool)valueX).CompareTo((bool)valueY);
				else
					result = 1;
			}
			else if (valueY is bool)
			{
				result = -1;
			}
			else
			{
				// Strings compare as themselves and other types compare with their 
				GetCellTextParameters parameters = new GetCellTextParameters(columnIndex);
				parameters.TextFormatMode = TextFormatMode.IgnoreCellWidth;
				parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.String;
				string cellTextX = rowX.GetCellTextInternal(parameters);
				string cellTextY = rowY.GetCellTextInternal(parameters);

				// MD 4/9/12 - TFS101506
				//int indexX = this.IndexOf(cellTextX, owner.CaseSensitive);
				//int indexY = this.IndexOf(cellTextY, owner.CaseSensitive);
				int indexX = this.IndexOf(worksheet, cellTextX, owner.CaseSensitive);
				int indexY = this.IndexOf(worksheet, cellTextY, owner.CaseSensitive);

				if (indexX == -1)
				{
					if (indexY == -1)
						result = OrderedSortCondition.CompareValues(owner, worksheet, valueX, valueY);
					else
						result = 1;
				}
				else if (indexY == -1)
				{
					result = -1;
				}
				else
				{
					result = indexX.CompareTo(indexY);
				}
			}

			if (this.SortDirection == SortDirection.Descending)
				return -result;

			return result;
		}

		#endregion // CompareCells

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="CustomListSortCondition"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			CustomListSortCondition other = obj as CustomListSortCondition;
			if (other == null || base.Equals(other) == false)
				return false;

			if (_list.Count != other._list.Count)
				return false;

			for (int i = 0; i < _list.Count; i++)
			{
				if (_list[i] != other._list[i])
					return false;
			}

			return true;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="CustomListSortCondition"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			int hashCode = _list.Count;
			if (_list.Count != 0)
				hashCode ^= _list[0].GetHashCode() ^ _list[_list.Count - 1].GetHashCode();

			return hashCode ^ base.GetHashCode();
		}

		#endregion // GetHashCode

		#region SortByValue

		internal override ST_SortBy SortByValue
		{
			get { return ST_SortBy.value; }
		}

		#endregion // SortByValue

		#endregion // Base Class Overrides

		#region Methods

		#region GetListString

		internal string GetListString()
		{
			StringBuilder sb = new StringBuilder(_list.Count * 2);
			for (int i = 0; i < _list.Count; i++)
			{
				sb.Append(_list[i]);
				sb.Append(",");
			}

			sb.Length--;
			return sb.ToString();
		}

		#endregion // GetListString

		#region IndexOf

		// MD 4/9/12 - TFS101506
		//private int IndexOf(string value, bool caseSensitive)
		//{
		//    StringComparison comparison = caseSensitive
		//        ? StringComparison.CurrentCulture
		//        : StringComparison.CurrentCultureIgnoreCase;
		//
		//    for (int i = 0; i < _list.Count; i++)
		//    {
		//        if (String.Equals(_list[i], value, comparison))
		//            return i;
		//    }
		//
		//    return -1;
		//}
		private int IndexOf(Worksheet worksheet, string value, bool caseSensitive)
		{
			CompareOptions options = caseSensitive
				? CompareOptions.None
				: CompareOptions.IgnoreCase;

			CultureInfo culture = worksheet.Culture;
			for (int i = 0; i < _list.Count; i++)
			{
				if (String.Compare(_list[i], value, culture, options) == 0)
					return i;
			}

			return -1;
		}

		#endregion // IndexOf

		#endregion // Methods

		#region Properties

		#region List

		/// <summary>
		/// Gets the ordered list of values by which to sort.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The list of values specified on this sort condition are string values. If a cell being sorted has a string value, that value is used to
		/// sort the cell. Otherwise, the cell text is used. For example, if the cell's value is 0.01, but it is formatted as a percentage cell,
		/// the text used to sort it with this sort condition will be "1%" and not "0.01". When using the cell text, if the format string for the 
		/// cell includes padding characters which are repeated across the cells, they will not be included in the cell text used for comparison.
		/// </p>
		/// <p class="body">
		/// When matching values from a cell to values in the custom list, strings are compared case-sensitively or case-insensitively based 
		/// on the <see cref="SortSettings&lt;T&gt;.CaseSensitive"/> setting.
		/// </p>
		/// <p class="body">
		/// If the cell text contains any repeated padding characters, they are ignored when comparing strings.
		/// </p>
		/// </remarks>
		/// <seealso cref="SortSettings&lt;T&gt;.CaseSensitive"/>
		/// <seealso cref="WorksheetCell.GetText(TextFormatMode)"/>
		/// <seealso cref="WorksheetRow.GetCellText(int,TextFormatMode)"/>
		public IEnumerable<string> List
		{
			get { return _list; }
		}

		#endregion // List

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