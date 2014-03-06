using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.Excel2007;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 5/18/07 - BR23022
	// Created a based class for all cell reference tokens
	// MD 8/18/08 - Excel formula solving
	//internal abstract class CellReferenceToken : FormulaToken
	// MD 3/30/11 - TFS69969
	// Added a common base class for all CellReferenceTokens and NameTokens
	//internal abstract class CellReferenceToken : OperandToken
	internal abstract class CellReferenceToken : ReferenceToken
	{
		// MD 10/22/10 - TFS36696
		// We don't need to store the formula on the token anymore.
		//public CellReferenceToken( Formula formula, TokenClass tokenClass )
		//    : base( formula, tokenClass ) { }
		public CellReferenceToken(TokenClass tokenClass)
			: base(tokenClass) { }

		#region Base Class Overrides

		#region OnWorksheetMoved

		public sealed override bool OnWorksheetMoved(Worksheet worksheet, int oldIndex)
		{
			WorksheetReferenceMulti multiSheetReference = this.WorksheetReference as WorksheetReferenceMulti;
			if (multiSheetReference == null ||
				multiSheetReference.IsExternal ||
				multiSheetReference.IsConnected == false)
			{
				return false;
			}

			int firstIndex = multiSheetReference.FirstWorksheetIndex;
			int lastIndex = multiSheetReference.LastWorksheetIndex;

			// If the first worksheet has moved after the last worksheet, or the last worksheet has moved before the first,
			// A new reference needs to be created. The range is no longer valid.
			if (lastIndex < firstIndex)
			{
				// Determine which worksheet endpoint was not moved.
				Worksheet worksheetNotMoved;
				if (worksheet.Index == firstIndex)
				{
					worksheetNotMoved = worksheet.Workbook.Worksheets[lastIndex];
				}
				else
				{
					Debug.Assert(worksheet.Index == lastIndex, "The moved worksheet should have been the last one in the range for the indexes to cross.");
					worksheetNotMoved = worksheet.Workbook.Worksheets[firstIndex];
				}

				CurrentWorkbookReference workbookReference = worksheet.Workbook.CurrentWorkbookReference;

				int firstWorksheetIndex = Math.Min(oldIndex, worksheetNotMoved.Index);
				int lastWorksheetIndex = Math.Max(oldIndex, worksheetNotMoved.Index);
				string firstWorksheetName = workbookReference.GetWorksheetName(firstWorksheetIndex);

				string lastWorksheetName;
				if (firstWorksheetIndex == lastWorksheetIndex)
				{
					// If the worksheet which was not moved is now at the old index of the moved worksheet, that means the two endpoint worksheet
					// were right next to each other. In this case, the multi-sheet reference is converted to a single sheet reference of the sheet
					// which was not moved.
					lastWorksheetName = null;
				}
				else
				{
					// Otherwise, the range needs to be recreated exactly as it was without the moved worksheet, which will be the from worksheet
					// now at the oldIndex to the worksheet which was not moved.
					lastWorksheetName = workbookReference.GetWorksheetName(lastWorksheetIndex);
				}

				this.WorksheetReference = workbookReference.GetWorksheetReference(firstWorksheetName, lastWorksheetName);
				return true;
			}

			// If the worksheet endpoints are in the same relative order for the range, we just need to determine whether the middle worksheets
			// may have changed, and if so, return true so the formula can be recompiled and the ancestor map updated with the new cells/region
			// referenced by this token.
			if (firstIndex <= worksheet.Index && worksheet.Index <= lastIndex)
				return true;

			if (firstIndex <= oldIndex && oldIndex <= lastIndex)
				return true;

			return false;
		}

		#endregion // OnWorksheetMoved

		#region OnWorksheetRemoved

		public sealed override bool OnWorksheetRemoved(Worksheet worksheet, int oldIndex, out FormulaToken replacementToken)
		{
			replacementToken = null;

			WorksheetReference worksheetReference = this.WorksheetReference;
			if (worksheetReference == null ||
				worksheetReference.IsExternal ||
				worksheetReference.IsConnected == false)
			{
				return false;
			}

			WorksheetReferenceLocal localReference = worksheetReference as WorksheetReferenceLocal;
			if (localReference != null && localReference.Worksheet == worksheet)
			{
				replacementToken = this.CreateEquivalentRefErrorToken();
				return true;
			}

			WorksheetReferenceMulti multiSheetReference = worksheetReference as WorksheetReferenceMulti;
			if (multiSheetReference != null)
			{
				WorkbookReferenceBase workbookReference = multiSheetReference.WorkbookReference;

				string firstWorksheetName;
				string lastWorksheetName;
				if (multiSheetReference.FirstWorksheetIndex == -1)
				{
					Debug.Assert(multiSheetReference.LastWorksheetIndex != -1, "Both worksheets shouldn't have been removed.");
					firstWorksheetName = workbookReference.GetWorksheetName(oldIndex);
					lastWorksheetName = multiSheetReference.LastWorksheetReference.Name;
				}
				else if (multiSheetReference.LastWorksheetIndex == -1)
				{
					firstWorksheetName = multiSheetReference.FirstWorksheetReference.Name;
					lastWorksheetName = workbookReference.GetWorksheetName(oldIndex - 1);
				}
				else
				{
					return multiSheetReference.FirstWorksheetIndex <= oldIndex && oldIndex <= multiSheetReference.LastWorksheetIndex;
				}

				if (firstWorksheetName == lastWorksheetName)
					lastWorksheetName = null;

				this.WorksheetReference = workbookReference.GetWorksheetReference(firstWorksheetName, lastWorksheetName);
				return true;
			}

			return false;
		}

		#endregion // OnWorksheetRemoved

		#endregion // Base Class Overrides

		// MD 5/21/07 - BR23050
		// We need a way to tell if the relative address in a cell address is an offset from the origin
		// cell or an absolute address
		public abstract bool AreRelativeAddressesOffsets { get;}

		protected abstract CellReferenceToken CreateEquivalentRefErrorToken();

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region GetCellAddressOffset

		protected bool GetCellAddressOffset(CellAddress reference,
			WorksheetCellAddress originalOwningCellAddress, WorksheetCellAddress newOwningCellAddress,
			int referencedRowIndexShiftAmount,
			ReferenceShiftType shiftType,
			out int rowOffset)
		{
			rowOffset = 0;

			bool isUsingOffsetRowReference = this.AreRelativeAddressesOffsets && reference.RowAddressIsRelative;
			int formulaRowIndexShiftAmount = newOwningCellAddress.RowIndex - originalOwningCellAddress.RowIndex;

			int newReferencedRowIndex = reference.Row;
			switch (shiftType)
			{
				case ReferenceShiftType.MaintainReference:
					if (isUsingOffsetRowReference)
					{
						// Update the relative row offset by the opposite of the difference of the shift amounts.
						// For example, if the formula shifted down 4 (a shift of 4) cells and the reference shifted up by 2 cells 
						// (a shift of -2), their difference is 6 and therefore the offset from the formula to the reference must
						// decrease by 6.
						newReferencedRowIndex -= (formulaRowIndexShiftAmount - referencedRowIndexShiftAmount);
					}
					else
					{
						// If the both the formula and reference were shifted and the source formula is using a direct reference, 
						// update the reference by the amount the reference has shifted.
						newReferencedRowIndex += referencedRowIndexShiftAmount;
					}
					break;

				case ReferenceShiftType.MaintainRelativeReferenceOffset:
					// If reference is absolute, treat it as we would if we were trying to maintain the reference, because the formula
					// should point to the same cell.
					if (reference.RowAddressIsRelative == false)
						goto case ReferenceShiftType.MaintainReference;

					// If the formula hasn't shifted nothing needs to be changed.
					if (formulaRowIndexShiftAmount == 0)
						return false;

					if (isUsingOffsetRowReference)
					{
						// If the formula shifted, we want to maintain the same relative offset, so we don't have to do anything 
						// to the offset, but return True because this token now points to a different absolute cell.
						return true;
					}
					else
					{
						// If the formula shifted, a direct referenced index must be shifted by the same amount the formula was 
						// shifted.
						newReferencedRowIndex += formulaRowIndexShiftAmount;
					}
					break;

				default:
					Utilities.DebugFail("Unknown ReferenceShiftType: " + shiftType);
					break;
			}

			rowOffset = newReferencedRowIndex - reference.Row;
			return rowOffset != 0;
		}

		#endregion // GetCellAddressOffset

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region GetWorksheetContainingReference

		protected Worksheet GetWorksheetContainingReference(FormulaContext context)
		{
			if (this.Is3DReference == false)
				return context.Worksheet;

			if (this.WorksheetReference.IsExternal || this.WorksheetReference.IsMultiSheet)
				return null;

			WorksheetReferenceLocal localSheetReference = this.WorksheetReference as WorksheetReferenceLocal;
			if (localSheetReference == null)
			{
				Utilities.DebugFail("Only a WorksheetReferenceLocal was expected here.");
				return null;
			}

			return localSheetReference.Worksheet;
		}

		#endregion // GetWorksheetContainingReference

		// MD 2/24/12 - 12.1 - Table Support
		public virtual bool HasRelativeAddresses
		{
			get { return false; }
		}

		// MD 3/30/11 - TFS69969
		// Moved this down to the new ReferenceToken class
		//public abstract bool Is3DReference { get; }

		// MD 9/17/08
		public virtual bool IsReferenceError 
		{
			get { return false; } 
		}

		// MD 3/29/11 - TFS63971
		public virtual void UpdateIndexedWorkbookReference(Excel2007WorkbookSerializationManager manager)
		{
			Debug.Assert(this.Is3DReference == false, "UpdateIndexedWorkbookReference should be overridden on types which are 3D references.");
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