using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 3/15/12 - TFS104581
	internal class WorksheetColumnBlock
	{
		#region Member Variables

		private WorksheetCellFormatData _cellFormat;

		// Since the blocks are keyed by the first column index in the worksheet's ColumnBlocks collection, 
		// we should make it read-only.
		private readonly short _firstColumnIndex;

		private bool _hidden;
		private short _lastColumnIndex;
		private byte _outlineLevel;
		private int _width = -1;

		#region Serialization Cache

		// This is only valid when the column's worksheet is about to be saved
		private bool _hasCollapseIndicator;

		#endregion Serialization Cache

		#endregion // Member Variables

		#region Constructor

		public WorksheetColumnBlock(short firstColumnIndex, short lastColumnIndex, WorksheetCellFormatData cellFormat)
		{
			_firstColumnIndex = firstColumnIndex;
			_lastColumnIndex = lastColumnIndex;
			_cellFormat = cellFormat;
		}

		public WorksheetColumnBlock(short firstColumnIndex, short lastColumnIndex, WorksheetColumnBlock initializeFrom)
			: this(firstColumnIndex, lastColumnIndex, initializeFrom._cellFormat)
		{
			_hidden = initializeFrom._hidden;
			_outlineLevel = initializeFrom._outlineLevel;
			_width = initializeFrom._width;
			_cellFormat.IncrementReferenceCount();
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		public override bool Equals(object obj)
		{
			WorksheetColumnBlock other = obj as WorksheetColumnBlock;
			if (other == null)
				return false;

			return
				_hidden == other._hidden &&
				_outlineLevel == other._outlineLevel &&
				_width == other._width &&
				_cellFormat.Equals(other._cellFormat);
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return
				_hidden.GetHashCode() ^
				_outlineLevel.GetHashCode() ^
				_width.GetHashCode() ^
				_cellFormat.GetHashCode();
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

		#region Methods

		#region GetWidth

		public double GetWidth(Worksheet worksheet, WorksheetColumnWidthUnit units)
		{
			return worksheet.ConvertFromCharacter256thsInt(this.Width, units);
		}

		#endregion // GetWidth

		#region InitSerializationCache

		internal bool InitSerializationCache(Worksheet worksheet,
			WorkbookSerializationManager serializationManager,
			WorksheetColumnBlock previousColumn)
		{
			#region Determine if a collapse indicator is above the column

			// The column with the collapsed indicator may be before or after an outlining group. 
			// If the ShowExpansionIndicatorToRightOfGroupedColumnsResolved value is False, the group 
			// is after the column with the indicator, so check the values of the next column instead 
			// of the previous one.
			int groupOutlineLevel = 0;
			bool groupHidden = false;

			if (worksheet.DisplayOptions.ShowExpansionIndicatorToRightOfGroupedColumnsResolved)
			{
				// If the previous column specified is actually the immediate column before this column,
				// take the hidden state and outline level from that column
				if (previousColumn != null)
				{
					groupOutlineLevel = previousColumn.OutlineLevel;
					groupHidden = previousColumn.Hidden;
				}
			}
			else
			{
				short nextBlockStartIndex = (short)(this.LastColumnIndex + 1);

				if (nextBlockStartIndex < worksheet.Columns.MaxCount)
				{
					WorksheetColumnBlock nextBlock = worksheet.GetColumnBlock(nextBlockStartIndex);
					if (nextBlock != null)
					{
						groupOutlineLevel = nextBlock.OutlineLevel;
						groupHidden = nextBlock.Hidden;
					}
				}
			}

			// This column will only display a collapse indicator if the adjacent column is hidden and the adjacent column has 
			// a higher outline level
			_hasCollapseIndicator = groupHidden && this.OutlineLevel < groupOutlineLevel;

			#endregion Determine if a collapse indicator is above the column

			// Return whether the column should be saved
			return
				this.OutlineLevel != 0 ||
				this.Width >= 0 ||
				_hasCollapseIndicator ||
				this.CellFormat.IsEmpty == false;
		}

		#endregion InitSerializationCache

		#region VerifyFormatLimits

		internal void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		{
			if (this.IsEmpty)
				return;

			int maxColumnIndex = Workbook.GetMaxColumnCount(testFormat) - 1;

			if (maxColumnIndex < this.LastColumnIndex)
			{
				limitErrors.AddError(
					String.Format(SR.GetString("LE_FormatLimitError_MaxColumnIndex"),
					Math.Max(this.FirstColumnIndex, maxColumnIndex + 1),
					maxColumnIndex));
			}
		}

		#endregion VerifyFormatLimits

		#endregion // Methods

		#region Properties

		#region CellFormat

		public WorksheetCellFormatData CellFormat
		{
			get { return _cellFormat; }
			set { _cellFormat = value; }
		}

		#endregion // CellFormat

		#region FirstColumnIndex

		public short FirstColumnIndex
		{
			get { return _firstColumnIndex; }
		}

		#endregion // FirstColumnIndex

		#region HasCollapseIndicator

		internal bool HasCollapseIndicator
		{
			get { return _hasCollapseIndicator; }
		}

		#endregion HasCollapseIndicator

		#region Hidden

		public bool Hidden
		{
			get { return _hidden; }
			set { _hidden = value; }
		}

		#endregion // Hidden

		#region IsEmpty

		public bool IsEmpty
		{
			get
			{
				return
					_hidden == false &&
					_outlineLevel == 0 &&
					_width == -1 &&
					_cellFormat.IsEmpty;
			}
		}

		#endregion // IsEmpty

		#region LastColumnIndex

		public short LastColumnIndex
		{
			get { return _lastColumnIndex; }
			set { _lastColumnIndex = value; }
		}

		#endregion // LastColumnIndex

		#region OutlineLevel

		public byte OutlineLevel
		{
			get { return _outlineLevel; }
			set { _outlineLevel = value; }
		}

		#endregion // OutlineLevel

		#region Width

		public int Width
		{
			get { return _width; }
			set { _width = value; }
		}

		#endregion // Width

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