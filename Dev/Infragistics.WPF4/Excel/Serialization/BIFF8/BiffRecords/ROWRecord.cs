using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class ROWRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			byte[] data = new byte[ 0 ];
			int dataIndex = 0;
			ushort rowNumber = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			WorksheetRow row = worksheet.Rows[ rowNumber ];

			manager.OnRowLoaded( row );

			dataIndex += 4; // first defined column & first column of unused tail of row
			ushort heightStruct = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			bool rowHasDefualtHeight =	( heightStruct & 0x8000 ) == 0x8000;
			if ( rowHasDefualtHeight == false )
				row.Height =	(ushort)( heightStruct & 0x7FFF );
			else
				row.Height = -1;

			dataIndex += 4; // Not used
			uint optionFlags = manager.CurrentRecordStream.ReadUInt32FromBuffer( ref data, ref dataIndex );

			row.OutlineLevel =	  (byte)( optionFlags & 0x00000007 );
			//bool hasCollapseIndicator=( optionFlags & 0x00000010 ) == 0x00000010;
			row.Hidden =				( optionFlags & 0x00000020 ) == 0x00000020;
			//bool nonDefaultHeight =	( optionFlags & 0x00000040 ) == 0x00000040;
			bool hasFormat =			( optionFlags & 0x00000080 ) == 0x00000080;
			// MD 9/5/07 - BR26240
			// This condition doesn't seem to be true always
			//Debug.Assert(				( optionFlags & 0x00000100 ) == 0x00000100 );
			int formatIndex = (ushort)( ( optionFlags & 0x0FFF0000 ) >> 16 );
			// Next 2 bits are additional space flags, not sure if we need to save these

			// MD 12/20/08 - TFS11419
			// If hasFormat (fGhostDirty) is False, the formatIndex (ixfe) is acutally undefined, 
			// so we shouldn't try to do anything else with the format.
			if ( hasFormat == false )
				return;

			WorksheetCellFormatData format = manager.Formats[ formatIndex ];

			// MD 1/1/12 - 12.1 - Cell Format Updates
			// The default element is a cell format and not a style format, so we can now check for equality.
			//if ( format.HasSameData( manager.Workbook.CellFormats.DefaultElement ) == false )
			if (format.EqualsInternal(manager.Workbook.CellFormats.DefaultElement) == false)
			{
                
                // One situation we don't want to have this assert fail is if we don't have a fill
                // pattern but the foreground fill color is a non-default, but this doesn't matter
                // since we aren't using it anyway.
                //
				//Debug.Assert( hasFormat );

				row.CellFormat.SetFormatting( format );
			}
			// MD 12/20/08 - TFS11419
			// This code is no longer needed because we will bail out above if hasFormat (fGhostDirty) is False.
			//else
			//{
			//    Debug.Assert( hasFormat == false );
			//}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			WorksheetRow row = (WorksheetRow)manager.ContextStack[ typeof( WorksheetRow ) ];

			// MD 7/26/10 - TFS34398
			// Get the cache from the context stack as well.
			WorksheetRowSerializationCache rowCache = (WorksheetRowSerializationCache)manager.ContextStack[typeof(WorksheetRowSerializationCache)];

			if ( row == null )
			{
                Utilities.DebugFail("There is no row in the context stack.");
				return;
			}

			// MD 7/26/10 - TFS34398
			if (rowCache == null)
			{
				Utilities.DebugFail("There is no WorksheetRowSerializationCachein the context stack.");
				return;
			}

			MemoryStream memoryStream = new MemoryStream(16);

			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//manager.CurrentRecordStream.Write( (ushort)row.Index );
			memoryStream.Write(BitConverter.GetBytes((ushort)row.Index), 0, 2);

			// MD 7/26/10 - TFS34398
			// The values are not cached on the row anymore.
			//manager.CurrentRecordStream.Write( (ushort)row.FirstCell );
			//manager.CurrentRecordStream.Write( (ushort)row.FirstCellInUndefinedTail );
			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//manager.CurrentRecordStream.Write((ushort)rowCache.firstCell);
			//manager.CurrentRecordStream.Write((ushort)rowCache.firstCellInUndefinedTail);
			memoryStream.Write(BitConverter.GetBytes(rowCache.firstCell), 0, 2);
			memoryStream.Write(BitConverter.GetBytes(rowCache.firstCellInUndefinedTail), 0, 2);

			// MD 10/7/10 - TFS34476
			// Resolve the row height and then write it out, because we will need it later.
			//if ( row.Height < 0 )
			//    manager.CurrentRecordStream.Write( (ushort)row.Worksheet.DefaultRowHeight );
			//else
			//    manager.CurrentRecordStream.Write( (ushort)row.Height );
			int rowHeight = row.Height < 0
				? rowHeight = row.Worksheet.DefaultRowHeight
				: rowHeight = row.Height;

			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//manager.CurrentRecordStream.Write((ushort)rowHeight);
			//
			//manager.CurrentRecordStream.Write( (uint)0 );
			memoryStream.Write(BitConverter.GetBytes((ushort)rowHeight), 0, 2);
			memoryStream.Write(BitConverter.GetBytes((uint)0), 0, 4);

			uint optionFlags = 0x00000100;

			optionFlags |= (uint)row.OutlineLevel;

			// If the row before this one is collapsed but this one isn't, mark the fifth bit
			// MD 7/26/10 - TFS34398
			// The values are not cached on the row anymore.
			//if ( row.HasCollapseIndicator )
			if (rowCache.hasCollapseIndicator)
				optionFlags |= 0x00000010;

			if ( row.Hidden )
				optionFlags |= 0x00000020;

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Cache the default and row formats because we need them below.
			WorksheetCellFormatData defaultFormat = manager.Workbook.CellFormats.DefaultElement;
			WorksheetCellFormatData rowFormat = row.HasCellFormat ? row.CellFormatInternal.Element : defaultFormat;

			// MD 7/9/10 - TFS34476
			// This was implemented slightly incorrectly. This bit should be set when the height of the row differs from 
			// what the row height should be based on it's current resolved font.
			//if ( row.Height >= 0 && row.Height != row.Worksheet.DefaultRowHeight )
			//    optionFlags |= 0x00000040;
			// MD 10/7/10 - TFS34476
			// Apparently, we always need to do this logic, even if the height of the row is not set, because the 
			// DefaultRowHeight on the worksheet may not be the same as the default row height based on the font and Excel
			// will only honor the height value wirtten in this record is the 0x40 bit is set in the options.
			//if (row.Height >= 0)
			{
				// MD 1/10/12 - 12.1 - Cell Format Updates
				#region Old Code

				//string fontName = Workbook.DefaultFontName;
				//int fontHeight = row.Worksheet.Workbook.DefaultFontHeight;
				//
				//if (row.HasCellFormat)
				//{
				//    string rowFontName = row.CellFormat.Font.Name;
				//    if (rowFontName != null && rowFontName != fontName)
				//        fontName = rowFontName;
				//
				//    int rowFontHeight = row.CellFormat.Font.Height;
				//    if (0 <= rowFontHeight && rowFontHeight != fontHeight)
				//        fontHeight = rowFontHeight;
				//}

				#endregion // Old Code
				// MD 7/30/12 - TFS117846
				//string fontName = rowFormat.FontNameResolved;
				//int fontHeight = rowFormat.FontHeightResolved;
				string fontName;
				int fontHeight;
				if ((rowFormat.FormatOptions & WorksheetCellFormatOptions.ApplyFontFormatting) == 0 &&
					row.Worksheet.DefaultColumnFormat != null)
				{
					fontName = row.Worksheet.DefaultColumnFormat.FontNameResolved;
					fontHeight = row.Worksheet.DefaultColumnFormat.FontHeightResolved;
				}
				else
				{
					fontName = rowFormat.FontNameResolved;
					fontHeight = rowFormat.FontHeightResolved;
				}

				int defaultRowHeight = manager.GetDefaultRowHeight(fontName, fontHeight);

				// MD 10/7/10 - TFS34476
				// Use the resolved row height, because we will now perform this logic even when row.Height is less than 0.
				//if (row.Height != defaultRowHeight)
				if (rowHeight != defaultRowHeight)
					optionFlags |= 0x00000040;
			}

			// MD 1/10/12 - 12.1 - Cell Format Updates
			#region Old Code

			//// This should really only be written out if the format is non-default
			//if ( row.HasCellFormat )
			//{
			//    if ( row.CellFormatInternal.HasDefaultValue == false )
			//        optionFlags |= 0x00000080;
			//
			//    int formatIndex = row.CellFormatInternal.Element.IndexInFormatCollection;
			//
			//    if ( formatIndex < 0 )
			//    {
			//        Utilities.DebugFail("Unknown format index");
			//        formatIndex = 0;
			//    }
			//
			//    optionFlags |= (uint)formatIndex << 16;
			//}

			#endregion // Old Code
			int formatIndex = manager.GetCellFormatIndex(rowFormat);
			if (formatIndex < 0)
			{
				Utilities.DebugFail("Unknown format index");
				formatIndex = 0;
			}

			if (rowFormat.EqualsInternal(defaultFormat) == false)
				optionFlags |= 0x00000080;

			optionFlags |= (uint)formatIndex << 16;

			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//manager.CurrentRecordStream.Write( (uint)optionFlags );
			memoryStream.Write(BitConverter.GetBytes(optionFlags), 0, 4);

			// MD 4/18/11 - TFS62026
			// Write all row data at once for performance.
			manager.CurrentRecordStream.Write(memoryStream);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.ROW; }
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