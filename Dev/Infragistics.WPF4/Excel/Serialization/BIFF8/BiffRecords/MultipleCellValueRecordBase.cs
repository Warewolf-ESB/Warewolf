using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal abstract class MultipleCellValueRecordBase : Biff8RecordBase
	{
		// MD 9/23/09 - TFS19150
		// Every read operation is relatively slow, so the buffer is now cached and passed into this method so we can get values from it.
		//protected abstract CellDefinition[] LoadCellValues( BIFF8WorkbookSerializationManager manager, int numberOfCells );
		protected abstract CellDefinition[] LoadCellValues( BIFF8WorkbookSerializationManager manager, int numberOfCells, ref byte[] data, ref int dataIndex );

		protected abstract void SaveCellValues( BIFF8WorkbookSerializationManager manager, CellDefinition[] value );

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
			ushort rowIndex = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			// MD 4/12/11 - TFS67084
			// Just use a regular short here so we don't have to cast the number.
			//ushort firstColumnIndex = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			short firstColumnIndex = manager.CurrentRecordStream.ReadInt16FromBuffer(ref data, ref dataIndex);

			// MD 9/23/09 - TFS19150
			// Every read operation is relatively slow, so read once and then get the values we need as we need them from the buffer.
			//manager.CurrentRecordStream.Position = manager.CurrentRecordStream.Length - 2;
			//ushort lastColumnIndex = manager.CurrentRecordStream.ReadUInt16();
			//
			//manager.CurrentRecordStream.Position = 4;
			ushort lastColumnIndex ;
			if ( data.Length == manager.CurrentRecordStream.Length )
			{
				lastColumnIndex = BitConverter.ToUInt16( data, data.Length - 2 );
			}
			else
			{
				manager.CurrentRecordStream.Position = manager.CurrentRecordStream.Length - 2;
				lastColumnIndex = manager.CurrentRecordStream.ReadUInt16();
			}

			CellDefinition[] definitions = this.LoadCellValues( manager, lastColumnIndex - firstColumnIndex + 1, ref data, ref dataIndex );

			// MD 9/23/09 - TFS19150
			// Getting the row every time is slow, so call a method on the manager, which does some caching and speeds things up.
			//WorksheetRow row = worksheet.Rows[ rowIndex ];
			WorksheetRow row = manager.GetRow( worksheet, rowIndex );

			// MD 4/12/11 - TFS67084
			// Use a short for the column index values so we don't have to cast it.
			//for ( int i = 0, columnIndex = firstColumnIndex; columnIndex <= lastColumnIndex; i++, columnIndex++ )
			for (short i = 0, columnIndex = firstColumnIndex; columnIndex <= lastColumnIndex; i++, columnIndex++)
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetCell cell = row.Cells[ columnIndex ];

				// MD 9/23/09 - TFS19150
				// Getting the CellFormat and then setting the formatted creates the format proxy with the default format and then 
				// changes it to hold the new format. This is unnecessary and slow, so create the proxy with the correct format
				// with the new SetCellFormat method. Also, if the format matches the default element, there's no reason to store it
				// on the cell anyway.
				//cell.CellFormat.SetFormatting( manager.Formats[ definitions[ i ].FormatIndex ] );
				WorksheetCellFormatData format = manager.Formats[ definitions[ i ].FormatIndex ];

				// MD 1/1/12 - 12.1 - Cell Format Updates
				// The default element is a cell format and not a style format, so we can now check for equality.
				//if ( format.HasSameData( manager.Workbook.CellFormats.DefaultElement ) == false )
				if (format.EqualsInternal(manager.Workbook.CellFormats.DefaultElement) == false)
				{
					// MD 10/27/10 - TFS56976
					// Renamed for clarity because this should only be called during loading.
					//cell.SetCellFormat( format );
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//cell.SetCellFormatWhileLoading(format);
					row.SetCellFormatWhileLoading(columnIndex, format);
				}

				// MD 9/22/09 - TFS19150
				// We should use the cached cell instead of getting it again.
				// Also, the Value setter may get the row again, which will be slow, so pass in the row we already have.
				//row.Cells[ columnIndex ].Value = definitions[ i ].Value;
				//cell.Value = definitions[ i ].Value;
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//cell.InternalSetValue( definitions[ i ].Value, row );
				row.SetCellValueRaw(columnIndex, definitions[i].Value);
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 10/19/07
			// Found while fixing BR27421
			// Added support for multiple cell value records
			MultipleCellValueInfo mulCellInfo = (MultipleCellValueInfo)manager.ContextStack[ typeof( MultipleCellValueInfo ) ];

			if ( mulCellInfo == null )
			{
                Utilities.DebugFail("There is no MULCellInfo in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)mulCellInfo.RowIndex );
			manager.CurrentRecordStream.Write( (ushort)mulCellInfo.FirstColumnIndex );

			CellDefinition[] definitions = new CellDefinition[ mulCellInfo.NumberOfCells ];
	
			for ( int i = 0; i < mulCellInfo.NumberOfCells; i++ )
			{
				// MD 7/26/10 - TFS34398
				// Now the resolved formats are stored on the manager instead of the cell.
				//definitions[ i ].FormatIndex = (ushort)mulCellInfo.GetCell( i ).ResolvedFormat.IndexInFormatCollection;
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//definitions[i].FormatIndex = (ushort)manager.ResolvedCellFormatsByCell[mulCellInfo.GetCell(i)].IndexInFormatCollection;
				// MD 4/18/11 - TFS62026
				// The ResolvedCellFormatsByCell collection used too much memory.
				//definitions[i].FormatIndex = (ushort)manager.ResolvedCellFormatsByCell[mulCellInfo.GetCellAddress(i)].IndexInFormatCollection;
				// MD 1/10/12 - 12.1 - Cell Format Updates
				// We no longer cache format indexes because we can easily get them at save time.
				//definitions[i].FormatIndex = (ushort)mulCellInfo.RowCache.cellFormatIndexValues[mulCellInfo.RowCache.nextCellFormatIndex++];
				definitions[i].FormatIndex = manager.GetCellFormatIndex(mulCellInfo.GetCellFormat(i));

				definitions[ i ].Value = mulCellInfo.GetValue( i );
			}

			this.SaveCellValues( manager, definitions );

			manager.CurrentRecordStream.Write( (ushort)mulCellInfo.LastColumnIndex );
		}
	}

	internal struct CellDefinition
	{
		public object Value;
		public ushort FormatIndex;
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