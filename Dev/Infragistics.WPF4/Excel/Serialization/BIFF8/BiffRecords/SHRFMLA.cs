using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class SHRFMLARecord : Biff8RecordBase
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
			ushort firstRow = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			ushort lastRow = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			byte firstColumn = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );
			byte lastColumn = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );

			manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex ); // not used
			manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex ); // number of formula records for this shared formula

			Formula formula = Formula.Load( manager.CurrentRecordStream, FormulaType.Formula, ref data, ref dataIndex );

			// MD 8/21/08 - Excel formula solving
			//formula.ApplyTo( new WorksheetRegion( worksheet, firstRow, firstColumn, lastRow, lastColumn ) );
			// MD 3/30/10 - TFS30253
			// The shared formulas don't necessarily have to be in contiguous blocks, so just apply the formula to the top left cell for now 
			// and let the other target cells indicate when they want to use the shared formula. This will happen when they have a FORMULA record
			// which has a single EXP token pointing to the top left cell of the shared range.
			//formula.ApplyTo( worksheet.GetCachedRegion( firstRow, firstColumn, lastRow, lastColumn ) );
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell sourceCell = worksheet.Rows[firstRow].Cells[firstColumn];
			//formula.ApplyTo(sourceCell);
			//manager.SharedFormulas.Add(sourceCell, formula);
			WorksheetRow sourceCellRow = worksheet.Rows[firstRow];

			// MD 9/27/11 - TFS88499
			// Now that we are making the dependant cells the roots in the loop below, we need to add the specified root cell in the 
			// SharedFormulas collection outside the loop. This is the root cell the other dependents should be pointing to.
			// MD 3/27/12 - 12.1 - Table Support
			//manager.SharedFormulas[new WorksheetCellAddress(sourceCellRow, firstColumn)] = formula;
			manager.SharedFormulas[new WorksheetCellAddress(firstRow, firstColumn)] = formula;

			// MD 5/26/11 - TFS76587
			// Apparently, the shared formula is not always applied to the top-left cell of the shared range. I have only seen one shared root
			// but just in case, we are setup here to handle multiple shared roots.
			//formula.ApplyTo(sourceCellRow, firstColumn);
			//manager.SharedFormulas.Add(new WorksheetCellAddress(sourceCellRow, firstColumn), formula);
			List<WorksheetCellAddress> pendingSharedFormulaRoots = new List<WorksheetCellAddress>(manager.PendingSharedFormulaRoots.Keys);
			for (int i = 0; i < pendingSharedFormulaRoots.Count; i++)
			{
				WorksheetCellAddress cellAddress = pendingSharedFormulaRoots[i];

				// MD 3/27/12 - 12.1 - Table Support
				// The cell address now just stored the row index, so we can't check to see if its from the same worksheet.
				#region Removed

				//                // MD 9/27/11 - TFS88499
				//                // This shouldn't happen since we now clear the PendingSharedFormulaRoots collection when we move onto the next worksheet,
				//                // but we should check for it anyway.
				//#if DEBUG
				//                if (cellAddress.Row.Worksheet != sourceCellRow.Worksheet)
				//                {
				//                    Utilities.DebugFail("The PendingSharedFormulaRoots collection should be cleared after loading each worksheet.");
				//                    continue;
				//                }
				//#endif

				#endregion // Removed

				// MD 3/27/12 - 12.1 - Table Support
				//int rowIndex = cellAddress.Row.Index;
				int rowIndex = cellAddress.RowIndex;

				short columnIndex = cellAddress.ColumnIndex;

				if (firstRow <= rowIndex && rowIndex <= lastRow &&
					firstColumn <= columnIndex && columnIndex <= lastColumn)
				{
					manager.PendingSharedFormulaRoots.Remove(cellAddress);

					// MD 3/27/12 - 12.1 - Table Support
					//WorksheetRow row = cellAddress.Row;
					WorksheetRow row = worksheet.Rows[rowIndex];

					formula.ApplyTo(row, columnIndex);

					Formula appliedFormula = row.GetCellFormulaInternal(columnIndex);

					// MD 9/27/11 - TFS88499
					// Use the setter instead of the add method so we don't get an exception in the case where we try to add this 
					// for the same cell. Also, we should make the current cell the root in this case, just in case one of the 
					// dependent cells points to this one instead of the root specified (in an incorrect file).
					//manager.SharedFormulas.Add(new WorksheetCellAddress(sourceCellRow, firstColumn), appliedFormula);
					// MD 3/27/12 - 12.1 - Table Support
					//manager.SharedFormulas[new WorksheetCellAddress(row, columnIndex)] = appliedFormula;
					manager.SharedFormulas[new WorksheetCellAddress(rowIndex, columnIndex)] = appliedFormula;
				}
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
            Utilities.DebugFail("We should never use the shared formula optimization.");
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.SHRFMLA; }
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