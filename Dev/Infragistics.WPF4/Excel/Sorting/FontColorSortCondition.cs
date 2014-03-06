using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.Excel2007;




using Infragistics.Shared;
using System.Drawing;


namespace Infragistics.Documents.Excel.Sorting
{
	// MD 12/14/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a sort condition which will sort cells based on their fonts colors.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// This sort condition specifies a single color. Cells of this color will be moved to the beginning of the data range for the ascending
	/// sort direction and moved to the end of the data range for the descending sort direction. All matching cells will be kept in their same 
	/// relative order to each other. In addition, all non-matching cells will be kept in their same relative order to each other.
	/// </p>
	/// </remarks>
	/// <seealso cref="SortSettings&lt;T&gt;.SortConditions"/>
	/// <seealso cref="SortCondition.SortDirection"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class FontColorSortCondition : SortCondition,
		IColorSortCondition
	{
		#region Member Variables

		private readonly WorkbookColorInfo _fontColorInfo;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="FontColorSortCondition"/> instance.
		/// </summary>
		/// <param name="fontColor">The color by which the cells should be sorted.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fontColor"/> is empty.
		/// </exception>
		public FontColorSortCondition(Color fontColor)
			: this(Utilities.ToColorInfo(fontColor)) { }

		/// <summary>
		/// Creates a new <see cref="FontColorSortCondition"/> instance.
		/// </summary>
		/// <param name="fontColorInfo">The color by which the cells should be sorted.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fontColorInfo"/> is null.
		/// </exception>
		public FontColorSortCondition(WorkbookColorInfo fontColorInfo)
			: this(fontColorInfo, SortDirection.Ascending) { }

		/// <summary>
		/// Creates a new <see cref="FontColorSortCondition"/> instance.
		/// </summary>
		/// <param name="fontColor">The color by which the cells should be sorted.</param>
		/// <param name="sortDirection">The direction by which to sort the cells.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fontColor"/> is empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="sortDirection"/> is not defined in the <see cref="SortDirection"/> enumeration.
		/// </exception>
		public FontColorSortCondition(Color fontColor, SortDirection sortDirection)
			: this(Utilities.ToColorInfo(fontColor), sortDirection) { }

		/// <summary>
		/// Creates a new <see cref="FontColorSortCondition"/> instance.
		/// </summary>
		/// <param name="fontColorInfo">
		/// A <see cref="WorkbookColorInfo"/> which describes the color by which the cells should be sorted.
		/// </param>
		/// <param name="sortDirection">The direction by which to sort the cells.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fontColorInfo"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="sortDirection"/> is not defined in the <see cref="SortDirection"/> enumeration.
		/// </exception>
		public FontColorSortCondition(WorkbookColorInfo fontColorInfo, SortDirection sortDirection)
			: base(sortDirection)
		{
			if (fontColorInfo == null)
				throw new ArgumentNullException("fontColorInfo");

			_fontColorInfo = fontColorInfo;
		}

		#endregion // Constructor

		#region Interfaces

		#region IColorSortCondition Members

		uint IColorSortCondition.AddDxfToManager(WorkbookSerializationManager manager)
		{
			WorksheetCellFormatData format = manager.Workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);
			format.IsForFillOrSortCondition = true;
			format.Font.ColorInfo = this.FontColorInfo;
			return manager.AddDxf(format);
		}

		#endregion

		#endregion // Interfaces

		#region Base Class Overrides

		#region CompareCells

		internal override int CompareCells<T>(SortSettings<T> owner, Worksheet worksheet, int rowIndexX, int rowIndexY, short columnIndex)
		{
			WorksheetCellFormatData cellFormatX = worksheet.GetCellFormatElementReadOnly(worksheet.Rows.GetIfCreated(rowIndexX), columnIndex);
			WorksheetCellFormatData cellFormatY = worksheet.GetCellFormatElementReadOnly(worksheet.Rows.GetIfCreated(rowIndexY), columnIndex);

			bool doesMatchX = (cellFormatX.FontColorInfoResolved == this.FontColorInfo);
			bool doesMatchY = (cellFormatY.FontColorInfoResolved == this.FontColorInfo);

			if (doesMatchX == doesMatchY)
				return 0;

			int result = doesMatchX ? -1 : 1;
			if (this.SortDirection == SortDirection.Descending)
				return -result;

			return result;
		}

		#endregion // CompareCells

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="FontColorSortCondition"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			FontColorSortCondition other = obj as FontColorSortCondition;
			if (other == null)
				return false;

			return
				Object.Equals(_fontColorInfo, other._fontColorInfo) &&
				base.Equals(other);
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="FontColorSortCondition"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			return _fontColorInfo.GetHashCode() ^ base.GetHashCode();
		}

		#endregion // GetHashCode

		#region SortByValue

		internal override ST_SortBy SortByValue
		{
			get { return ST_SortBy.fontColor; }
		}

		#endregion // SortByValue

		#endregion // Base Class Overrides

		#region Methods

		#region CreateFontColorSortCondition

		internal static FontColorSortCondition CreateFontColorSortCondition(WorkbookSerializationManager manager, uint dxfId, SortDirection sortDirection)
		{
			if (manager.Dxfs.Count <= dxfId)
			{
				Utilities.DebugFail("The dxfId value is incorrect.");
				return null;
			}

			WorksheetCellFormatData format = manager.Dxfs[(int)dxfId];
			WorkbookColorInfo colorInfo = format.Font.ColorInfo;

			if (colorInfo == null)
			{
				Utilities.DebugFail("Cannot find the color by which to sort.");
				return null;
			}

			return new FontColorSortCondition(colorInfo, sortDirection);
		}

		#endregion // CreateColorSortCondition

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region FontColorInfo

		/// <summary>
		/// Gets the <see cref="WorkbookColorInfo"/> which describes the color by which the cells should be sorted.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Cells of this color will be moved to the beginning of the data range for the ascending sort direction and moved to the end 
		/// of the data range for the descending sort direction. All matching cells will be kept in their same relative order to each other. 
		/// In addition, all non-matching cells will be kept in their same relative order to each other.
		/// </p>
		/// </remarks>
		/// <value>The WorkbookColorInfo which describes the color by which the cells should be sorted.</value>
		/// <seealso cref="SortCondition.SortDirection"/>
		/// <seealso cref="WorksheetCell.CellFormat"/>
		/// <seealso cref="IWorksheetCellFormat.Font"/>
		/// <seealso cref="IWorkbookFont.ColorInfo"/>
		public WorkbookColorInfo FontColorInfo
		{
			get { return _fontColorInfo; }
		}

		#endregion // ColorInfo

		#endregion // Public Properties

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