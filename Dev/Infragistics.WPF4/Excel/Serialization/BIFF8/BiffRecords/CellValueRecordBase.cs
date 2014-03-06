using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal abstract class CellValueRecordBase : Biff8RecordBase
	{
		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected abstract object LoadCellValue( BIFF8WorkbookSerializationManager manager );
		//protected abstract void SaveCellValue( BIFF8WorkbookSerializationManager manager, object value );
		protected abstract void LoadCellValue( BIFF8WorkbookSerializationManager manager, ref byte[] data, ref int dataIndex, out object cellValue, out object lastCalculatedCellValue );

		// MD 4/18/11 - TFS62026
		// Since the FORMULA record is the only one which needs the lastCalculatedCellValue, it is no longer passed in here. Instead, pass along the cell
		// context so the FORMULA record can ask for the calculated value directly.
		// Also, we will write the cell value records in one block if possible, so pass in the memory stream containing the initial data from the record.
		//protected abstract void SaveCellValue( BIFF8WorkbookSerializationManager manager, object cellValue, object lastCalculatedCellValue );
		protected abstract void SaveCellValue(BIFF8WorkbookSerializationManager manager, CellContext cellContext, MemoryStream initialData);

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
			//ushort columnIndex = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			short columnIndex = manager.CurrentRecordStream.ReadInt16FromBuffer(ref data, ref dataIndex);

			// MD 9/23/09 - TFS19150
			// Getting the row every time is slow, so call a method on the manager, which does some caching and speeds things up.
			//WorksheetCell cell = worksheet.Rows[ rowIndex ].Cells[ columnIndex ];
			WorksheetRow row = manager.GetRow( worksheet, rowIndex );

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell = row.Cells[ columnIndex ];

			ushort formatIndex = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			// MD 7/12/12 - TFS116934
			// If the format index is out of range, Excel will use the default format, so we should as well.
			//WorksheetCellFormatData format = manager.Formats[ formatIndex ];
			WorksheetCellFormatData format;
			if (formatIndex < manager.Formats.Count)
				format = manager.Formats[formatIndex];
			else
				format = manager.Workbook.CellFormats.DefaultElement;

			// MD 1/1/12 - 12.1 - Cell Format Updates
			// The default element is a cell format and not a style format, so we can now check for equality.
			//if ( format.HasSameData( manager.Workbook.CellFormats.DefaultElement ) == false )
			if (format.EqualsInternal(manager.Workbook.CellFormats.DefaultElement) == false)
			{
				// MD 9/23/09 - TFS19150
				// Getting the CellFormat and then setting the formatted creates the format proxy with the default format and then 
				// changes it to hold the new format. This is unnecessary and slow, so create the proxy with the correct format
				// with the new SetCellFormat method.
				//cell.CellFormat.SetFormatting( format );
				// MD 10/27/10 - TFS56976
				// Renamed for clarity because this should only be called during loading.
				//cell.SetCellFormat( format );
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//cell.SetCellFormatWhileLoading(format);
				row.SetCellFormatWhileLoading(columnIndex, format);
			}

			// MD 9/2/08 - Excel formula solving
			//object value = this.LoadCellValue( manager );
			object cellValue;
			object lastCalculatedCellValue;
			this.LoadCellValue( manager, ref data, ref dataIndex, out cellValue, out lastCalculatedCellValue );

// MD 7/20/2007 - BR25039
// Surrounded in #if


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


			// MD 9/2/08 - Excel formula solving
			//Formula formula = value as Formula;
			Formula formula = cellValue as Formula;

			if ( formula != null )
			{
				if ( formula.IsSpecialFormula == false )
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//formula.ApplyTo( cell );
					//
					//// MD 9/2/08 - Excel formula solving
					//// Apply the loaded cached value
					//cell.CalcReference.Value = CalcUtilities.CreateExcelCalcValue( lastCalculatedCellValue );
					formula.ApplyTo(row, columnIndex);
					row.GetCellCalcReference(columnIndex).Value = CalcUtilities.CreateExcelCalcValue(lastCalculatedCellValue);
				}
				// MD 3/30/10 - TFS30253
				// The shared formulas don't necessarily have to be in contiguous blocks, so we need to apply the shared formula to the cell when we get the 
				// special FORMULA record for the target cell.
				else
				{
					ExpToken expToken = formula.PostfixTokenList[0] as ExpToken;

					// The EXP token indicates an array or shared formula.
					if (expToken != null)
					{
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//WorksheetCell sourceCell = expToken.CellAddress.GetTargetCell(cell, false);
						//
						//if (sourceCell != cell)
						//{
						//    // If the cell pointed to by the EXP token is in the SharedFormulas collection, we have encountered a SHRFMLA record already so we 
						//    // know it is a shared formula and not an array formula.
						//    Formula sharedFormula;
						//    if (manager.SharedFormulas.TryGetValue(sourceCell, out sharedFormula))
						//        sharedFormula.ApplyTo(cell);
						//}
						short expTokenColumnIndex = expToken.CellAddress.Column;
						int expTokenRowIndex = expToken.CellAddress.Row;

						// MD 3/21/12 - TFS104630
						// We may have already loaded the SHRFMLA record for the root cell, so look through the SharedFormulas collection
						// always first and then add the cell to the PendingSharedFormulaRoots collection if it is the root cell and we haven't
						// loaded the shared formula yet.
						//if (expTokenRowIndex != row.Index || expTokenColumnIndex != columnIndex)
						//{
						//    WorksheetCellAddress sourceCellAddress = new WorksheetCellAddress(worksheet.Rows[expTokenRowIndex], expTokenColumnIndex);
						//
						//    // If the cell pointed to by the EXP token is in the SharedFormulas collection, we have encountered a SHRFMLA record already so we 
						//    // know it is a shared formula and not an array formula.
						//    Formula sharedFormula;
						//    if (manager.SharedFormulas.TryGetValue(sourceCellAddress, out sharedFormula))
						//        sharedFormula.ApplyTo(row, columnIndex);
						//}
						//// MD 5/26/11 - TFS76587
						//else
						//{
						//    manager.PendingSharedFormulaRoots[new WorksheetCellAddress(row, columnIndex)] = true;
						//}
						// MD 3/27/12 - 12.1 - Table Support
						//WorksheetCellAddress sourceCellAddress = new WorksheetCellAddress(worksheet.Rows[expTokenRowIndex], expTokenColumnIndex);
						WorksheetCellAddress sourceCellAddress = new WorksheetCellAddress(expTokenRowIndex, expTokenColumnIndex);

						// If the cell pointed to by the EXP token is in the SharedFormulas collection, we have encountered a SHRFMLA record already so we 
						// know it is a shared formula and not an array formula.
						Formula sharedFormula;
						if (manager.SharedFormulas.TryGetValue(sourceCellAddress, out sharedFormula))
						{
							sharedFormula.ApplyTo(row, columnIndex);
						}
						else if (expTokenRowIndex == row.Index && expTokenColumnIndex == columnIndex)
						{
							// MD 3/27/12 - 12.1 - Table Support
							//manager.PendingSharedFormulaRoots[new WorksheetCellAddress(row, columnIndex)] = true;
							manager.PendingSharedFormulaRoots[sourceCellAddress] = true;
						}
					}
				}
			}
			else
			{
				// MD 9/2/08 - Excel formula solving
				//cell.Value = value;
                // MD 9/23/09 - TFS19150
				// The Value setter may get the row again, which will be slow, so pass in the row we already have.
				//cell.Value = cellValue;
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//cell.InternalSetValue( cellValue, row );
				row.SetCellValueRaw(columnIndex, cellValue);
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell = (WorksheetCell)manager.ContextStack[ typeof( WorksheetCell ) ];
			//
			//if ( cell == null )
			//{
			//    Utilities.DebugFail("There is no cell in the context stack.");
			//    return;
			//}
			// MD 4/18/11 - TFS62026
			// We now just have one CellContext object for the cell on the context stack as opposed to multiple values that 
			// we have to get separately.
			//WorksheetRow row = (WorksheetRow)manager.ContextStack[typeof(WorksheetRow)];
			//object columnIndexValue = manager.ContextStack[typeof(ColumnIndex)];
			//
			//if (row == null || columnIndexValue == null)
			//{
			//    Utilities.DebugFail("There is no cell in the context stack.");
			//    return;
			//}
			//
			//ColumnIndex columnIndex = (ColumnIndex)columnIndexValue;
			//
			//object cellValue = manager.ContextStack[typeof(object)];
			
			CellContext cellContext = (CellContext)manager.ContextStack[typeof(CellContext)];
			if (cellContext == null)
			{
				Utilities.DebugFail("There is no cell in the context stack.");
				return;
			}

			// MD 1/10/12 - 12.1 - Cell Format Updates
			CellDataContext cellDataContext = (CellDataContext)manager.ContextStack[typeof(CellDataContext)];
			if (cellDataContext == null)
			{
				Utilities.DebugFail("There is no CellDataContext in the context stack.");
				return;
			}

			// MD 6/11/07 - BR23706
			// For cells with no value but some formatting, the value will be null
			//if ( cellValue == null )
			//{
            //    Utilities.DebugFail( "There is no cell value in the context stack." );
			//    return;
			//}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//manager.CurrentRecordStream.Write( (ushort)cell.RowIndex );
			//manager.CurrentRecordStream.Write( (ushort)cell.ColumnIndex );
			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//manager.CurrentRecordStream.Write((ushort)cellContext.row.Index);
			//manager.CurrentRecordStream.Write(cellContext.columnIndex);
			MemoryStream initialData = new MemoryStream(32);
			initialData.Write(BitConverter.GetBytes((ushort)cellContext.Row.Index), 0, 2);
			initialData.Write(BitConverter.GetBytes(cellContext.ColumnIndex), 0, 2);
			
			// MD 7/26/10 - TFS34398
			// Now the resolved formats are stored on the manager instead of the cell.
			//int formatIndex = cell.ResolvedFormat.IndexInFormatCollection;
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//int formatIndex = manager.ResolvedCellFormatsByCell[cell].IndexInFormatCollection;
			// MD 4/18/11 - TFS62026
			// The ResolvedCellFormatsByCell collection used too much memory.
			//int formatIndex = manager.ResolvedCellFormatsByCell[new WorksheetCellAddress(row, columnIndex)].IndexInFormatCollection;
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// We no longer cache format indexes because we can easily get them at save time.
			//int formatIndex = cellContext.RowCache.cellFormatIndexValues[cellContext.RowCache.nextCellFormatIndex++];
			int formatIndex = manager.GetCellFormatIndex(cellDataContext.CellFormatData);

			if ( formatIndex < 0 )
			{
                Utilities.DebugFail("Unknown format index");
				formatIndex = 0;
			}

			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//manager.CurrentRecordStream.Write( (ushort)formatIndex );
			initialData.Write(BitConverter.GetBytes((ushort)formatIndex), 0, 2);

			// MD 9/2/08 - Excel formula solving
			//this.SaveCellValue( manager, cellValue );
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//this.SaveCellValue( manager, cellValue, WorkbookSerializationManager.GetSerializableCellValue( cell, true ) );
			// MD 4/18/11 - TFS62026
			// Since the FORMULA record is the only one which needs the lastCalculatedCellValue, it is no longer passed in here. Instead, pass along the cell
			// context so the FORMULA record can ask for the calculated value directly.
			// Also, we will write the cell value records in one block if possible, so pass in the memory stream containing the initial data from the record.
			//this.SaveCellValue(manager, cellValue, WorkbookSerializationManager.GetSerializableCellValue(row, columnIndex, true));
			this.SaveCellValue(manager, cellContext, initialData);
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