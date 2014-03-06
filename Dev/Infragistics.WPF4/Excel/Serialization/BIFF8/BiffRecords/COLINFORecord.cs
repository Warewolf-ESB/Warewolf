using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class COLINFORecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort firstColumnInRange = manager.CurrentRecordStream.ReadUInt16();
			ushort lastColumnInRange = manager.CurrentRecordStream.ReadUInt16();

			// MD 7/23/12 - TFS117430
			// Moved this below because we need to know if either value is 256 when we get the format.
			//// MD 3/26/12
			//// Found while fixing TFS106075
			//// The value of these indexes could be 256 to indicate the default column format. Since we don't support that yet,
			//// we should just make sure the column indexes are less than 256.
			//firstColumnInRange = Math.Min((ushort)255, firstColumnInRange);
			//lastColumnInRange = Math.Min((ushort)255, lastColumnInRange);

			ushort columnWidth = manager.CurrentRecordStream.ReadUInt16();

			ushort formatIndex = manager.CurrentRecordStream.ReadUInt16();

			if ( formatIndex < 0 )
			{
                Utilities.DebugFail("Unknown format index");
				formatIndex = 0;
			}

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			bool hidden =		( optionFlags & 0x0001 ) == 0x0001;
			int outlineLevel =	( optionFlags & 0x0700 ) >> 8;
			bool collapsed =	( optionFlags & 0x1000 ) == 0x1000;

			WorksheetCellFormatData format = manager.Formats[ formatIndex ];

			// MD 7/23/12 - TFS117430
			// If the lastColumnInRange value is 256, it is the default for all columns, which means the default for everything.
			if (lastColumnInRange == 256)
			{
				// MD 7/30/12 - TFS117846
				// This was incorrect. It is the default for everything in the worksheet, not the workbook.
				//manager.Workbook.CellFormats.DefaultElement = format.CloneInternal();
				worksheet.DefaultColumnFormat = format.CloneInternal();
			}


			// MD 7/23/12 - TFS117430
			// Wrapped in an if statement. If both values were 256, this record just stored the default column format. Otherwise,
			// make sure the lastColumnInRange value is less than 256 and load the column block.
			if (firstColumnInRange != 256)
			{
				lastColumnInRange = Math.Min((ushort)255, lastColumnInRange);

			// MD 3/15/12 - TFS104581
			// We now store the column blocks on the worksheet.
			#region Old Code

			//// MD 1/1/12 - 12.1 - Cell Format Updates
			//// The default element is a cell format and not a style format, so we can now check for equality.
			////bool setFormat = format.HasSameData( manager.Workbook.CellFormats.DefaultElement ) == false;
			//bool setFormat = format.EqualsInternal(manager.Workbook.CellFormats.DefaultElement) == false;
			//
			//// MD 6/31/08 - Excel 2007 Format
			//int maxColumnCount = worksheet.Workbook.MaxColumnCount;
			//
			//for ( int i = firstColumnInRange; i <= lastColumnInRange; i++ )
			//{
			//    // MD 6/31/08 - Excel 2007 Format
			//    //if ( i >= Workbook.MaxExcelColumnCount )
			//    if ( i >= maxColumnCount )
			//        break;
			//
			//    WorksheetColumn column = worksheet.Columns[ i ];
			//
			//    if ( setFormat )
			//        column.CellFormat.SetFormatting( format );
			//
			//    column.Hidden = hidden;
			//    column.OutlineLevel = outlineLevel;
			//    column.Width = columnWidth;
			//
			//    Debug.Assert( column.HasCollapseIndicator == collapsed );
			//}

			#endregion // Old Code
			worksheet.OnColumnBlockLoaded((short)firstColumnInRange, (short)lastColumnInRange,
				(int)columnWidth, hidden, (byte)outlineLevel, format);
			}

			// MD 12/16/09 - TFS24196
			// If this file was not created with Microsoft Excel, this record may not have the correct length. If it
			// is only 11 bytes long instead of 12, trying to read a UInt16 here will throw an EndOfStreamException.
			// Since we don't need the value anyway, just seek instead, which will not cause an exception.
			//manager.CurrentRecordStream.ReadUInt16(); // Not used
			manager.CurrentRecordStream.Seek(2, System.IO.SeekOrigin.Current);
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 3/15/12 - TFS104581
			//BIFF8WorkbookSerializationManager.ColumnBlockInfo columnInfo =
			//    (BIFF8WorkbookSerializationManager.ColumnBlockInfo)manager.ContextStack[ typeof( BIFF8WorkbookSerializationManager.ColumnBlockInfo ) ];
			WorksheetColumnBlock columnInfo = (WorksheetColumnBlock)manager.ContextStack[typeof(WorksheetColumnBlock)];

			if ( columnInfo == null )
			{
                Utilities.DebugFail("There is no column info in the context stack.");
				return;
			}

			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)columnInfo.FirstColumnIndex );

			// MD 7/2/12 - TFS115692
			// We still do no support the default column format, but if we don't write it out, the row height calculations may be incorrect,
			// so for now, just make the last column block the default column block.
			//manager.CurrentRecordStream.Write( (ushort)columnInfo.LastColumnIndex );
			ushort lastColumnIndex = (ushort)columnInfo.LastColumnIndex;
			if (lastColumnIndex == 255)
				lastColumnIndex = 256;

			manager.CurrentRecordStream.Write(lastColumnIndex);

			if ( columnInfo.Width < 0 )
			{
				// MD 1/11/08 - BR29105
				// The default column width is not the actual width of default columns, use the resolved property instead.
				//manager.CurrentRecordStream.Write( (ushort)worksheet.DefaultColumnWidth );
				// MD 2/10/12 - TFS97827
				// The DefaultColumnWidth is now the final value. Nothing needs to be resolved.
				//manager.CurrentRecordStream.Write( (ushort)worksheet.DefaultColumnWidthResolved );
				manager.CurrentRecordStream.Write((ushort)worksheet.DefaultColumnWidth);
			}
			else
				manager.CurrentRecordStream.Write( (ushort)columnInfo.Width );

			// MD 3/15/12 - TFS104581
			//manager.CurrentRecordStream.Write( (ushort)columnInfo.FormatIndex );
			manager.CurrentRecordStream.Write((ushort)manager.GetCellFormatIndex(columnInfo.CellFormat));

			ushort optionFlags = 0x0002;

			if ( columnInfo.Hidden )
				optionFlags |= 0x0001;

			optionFlags |= (ushort)( columnInfo.OutlineLevel << 8 );

			if ( columnInfo.HasCollapseIndicator )
				optionFlags |= 0x1000;

			manager.CurrentRecordStream.Write( (ushort)optionFlags );
			manager.CurrentRecordStream.Write( (ushort)0 );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.COLINFO; }
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