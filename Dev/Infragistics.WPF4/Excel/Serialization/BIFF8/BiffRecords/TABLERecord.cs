using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class TABLERecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort firstRow = manager.CurrentRecordStream.ReadUInt16();
			ushort lastRow = manager.CurrentRecordStream.ReadUInt16();
			byte firstColumn = (byte)manager.CurrentRecordStream.ReadByte();
			byte lastColumn = (byte)manager.CurrentRecordStream.ReadByte();

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			//bool alwaysCalculateFormula =	( optionFlags & 0x0001 ) == 0x0001;
			//bool calculateOnOpen =			( optionFlags & 0x0002 ) == 0x0002;
			bool inputCellInRowInputCell =	( optionFlags & 0x0004 ) == 0x0004;
			bool twoInputDataTable =		( optionFlags & 0x0008 ) == 0x0008;

			ushort primaryInputRow = manager.CurrentRecordStream.ReadUInt16();
			ushort primaryInputColumn = manager.CurrentRecordStream.ReadUInt16();
			ushort secondaryInputRow = manager.CurrentRecordStream.ReadUInt16();
			ushort secondaryInputColumn = manager.CurrentRecordStream.ReadUInt16();

			WorksheetCell rowInputCell = null;
			WorksheetCell columnInputCell = null;

			if ( twoInputDataTable )
			{
				rowInputCell = worksheet.Rows[ primaryInputRow ].Cells[ primaryInputColumn ];
				columnInputCell = worksheet.Rows[ secondaryInputRow ].Cells[ secondaryInputColumn ];
			}
			else if ( inputCellInRowInputCell )
			{
				rowInputCell = worksheet.Rows[ primaryInputRow ].Cells[ primaryInputColumn ];
			}
			else
			{
				columnInputCell = worksheet.Rows[ primaryInputRow ].Cells[ primaryInputColumn ];
			}

			worksheet.DataTables.Add(
				// MD 8/21/08 - Excel formula solving
				//new WorksheetRegion( worksheet, firstRow - 1, firstColumn - 1, lastRow, lastColumn ),
				worksheet.GetCachedRegion( firstRow - 1, firstColumn - 1, lastRow, lastColumn ),
				columnInputCell, 
				rowInputCell );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 4/18/11 - TFS62026
			// The cell value is not on the context stack anymore. Instead, there is a CellContext which holds 
			// multiple pieces of information about the cell.
			//WorksheetDataTable dataTable = (WorksheetDataTable)manager.ContextStack[ typeof( WorksheetDataTable ) ];
			CellContext cellContext = (CellContext)manager.ContextStack[typeof(CellContext)];

			if (cellContext == null)
			{
				Utilities.DebugFail("There was no cell context in the context stack.");
				return;
			}

			WorksheetDataTable dataTable = cellContext.Value as WorksheetDataTable;

			if ( dataTable == null )
			{
                Utilities.DebugFail("There is no data table in the context stack.");
				return;
			}

			// MD 3/12/12 - 12.1 - Table Support
			//manager.CurrentRecordStream.Write( (ushort)( dataTable.CellsInTable.FirstRow + 1 ) );
			//manager.CurrentRecordStream.Write( (ushort)dataTable.CellsInTable.LastRow );
			//manager.CurrentRecordStream.Write( (byte)( dataTable.CellsInTable.FirstColumn + 1 ) );
			//manager.CurrentRecordStream.Write( (byte)dataTable.CellsInTable.LastColumn );
			WorksheetRegion cellsInTable = dataTable.CellsInTable;
			manager.CurrentRecordStream.Write((ushort)(cellsInTable.FirstRow + 1));
			manager.CurrentRecordStream.Write((ushort)cellsInTable.LastRow);
			manager.CurrentRecordStream.Write((byte)(cellsInTable.FirstColumn + 1));
			manager.CurrentRecordStream.Write((byte)cellsInTable.LastColumn);

			ushort optionFlags = 0;
			ushort primaryInputRow = 0;
			ushort primaryInputColumn = 0;
			ushort secondaryInputRow = 0;
			ushort secondaryInputColumn = 0;

			// MD 3/12/12 - 12.1 - Table Support
			//if ( dataTable.ColumnInputCell != null && dataTable.RowInputCell != null )
			//{
			//    optionFlags |= 0x0008;
			//
			//    primaryInputRow = (ushort)dataTable.RowInputCell.RowIndex;
			//    primaryInputColumn = (ushort)dataTable.RowInputCell.ColumnIndex;
			//    secondaryInputRow = (ushort)dataTable.ColumnInputCell.RowIndex;
			//    secondaryInputColumn = (ushort)dataTable.ColumnInputCell.ColumnIndex;
			//}
			//else if ( dataTable.RowInputCell != null )
			//{
			//    optionFlags |= 0x0004;
			//
			//    primaryInputRow = (ushort)dataTable.RowInputCell.RowIndex;
			//    primaryInputColumn = (ushort)dataTable.RowInputCell.ColumnIndex;
			//}
			//else
			//{
			//    primaryInputRow = (ushort)dataTable.ColumnInputCell.RowIndex;
			//    primaryInputColumn = (ushort)dataTable.ColumnInputCell.ColumnIndex;
			//}
			WorksheetCell rowInputCell = dataTable.RowInputCell;
			WorksheetCell columnInputCell = dataTable.ColumnInputCell;
			if (columnInputCell != null && rowInputCell != null)
			{
				optionFlags |= 0x0008;

				primaryInputRow = (ushort)rowInputCell.RowIndex;
				primaryInputColumn = (ushort)rowInputCell.ColumnIndex;
				secondaryInputRow = (ushort)columnInputCell.RowIndex;
				secondaryInputColumn = (ushort)columnInputCell.ColumnIndex;
			}
			else if (rowInputCell != null)
			{
				optionFlags |= 0x0004;

				primaryInputRow = (ushort)rowInputCell.RowIndex;
				primaryInputColumn = (ushort)rowInputCell.ColumnIndex;
			}
			else
			{
				primaryInputRow = (ushort)columnInputCell.RowIndex;
				primaryInputColumn = (ushort)columnInputCell.ColumnIndex;
			}

			manager.CurrentRecordStream.Write( optionFlags );
			manager.CurrentRecordStream.Write( primaryInputRow );
			manager.CurrentRecordStream.Write( primaryInputColumn );
			manager.CurrentRecordStream.Write( secondaryInputRow );
			manager.CurrentRecordStream.Write( secondaryInputColumn );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.TABLE; }
		}
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