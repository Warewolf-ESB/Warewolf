using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 3/30/11 - TFS69969
	internal abstract class ReferenceToken : OperandToken
	{
		public ReferenceToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			if (Object.Equals(this.WorksheetReference, ((ReferenceToken)comparisonToken).WorksheetReference) == false)
				return false;

			return true;
		}

		#region Load3DData

		protected WorksheetReference Load3DData(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			WorkbookSerializationManager manager = stream.Manager;

			if (isForExternalNamedReference)
			{
				int firstSheetIndex = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
				int lastSheetIndex = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);

				WorkbookReferenceBase workbook = manager.WorkbookReferences[manager.WorkbookReferences.Count - 1];

				string firstWorksheetName = workbook.GetWorksheetName(firstSheetIndex);
				string lastWorksheetName = null;
				if (firstSheetIndex != lastSheetIndex)
				{
					Debug.Assert(firstSheetIndex < lastSheetIndex, "This is unexpected.");
					lastWorksheetName = workbook.GetWorksheetName(lastSheetIndex);
				}

				return workbook.GetWorksheetReference(firstWorksheetName, lastWorksheetName);
			}
			else
			{
				int externSheetIndex = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
				return manager.GetWorksheetReference(externSheetIndex);
			}
		}

		#endregion // Load3DData

		#region Save3DData

		protected void Save3DData(BiffRecordStream stream, WorksheetReference worksheetReference, bool isForExternalNamedReference)
		{
			if (isForExternalNamedReference)
			{
				stream.Write((ushort)worksheetReference.FirstWorksheetIndex);
				stream.Write((ushort)worksheetReference.LastWorksheetIndex);
			}
			else
			{
				int index = stream.Manager.WorksheetReferences.IndexOf(worksheetReference);
				Debug.Assert(index >= 0, "The worksheet reference could not be found.");
				stream.Write((ushort)index);
			}
		}

		#endregion // Save3DData

		public virtual void SetWorkbookReference(WorkbookReferenceBase workbookReference)
		{
			WorksheetReference worksheetReference = this.WorksheetReference;
			if (workbookReference != null)
				this.WorksheetReference = worksheetReference.Connect(workbookReference);
		}

		public virtual bool Is3DReference
		{
			get { return this.WorksheetReference != null; }
		}

		public virtual bool IsExternalReference
		{
			get 
			{
				WorksheetReference worksheetReference = this.WorksheetReference;
				if (worksheetReference == null)
					return false;

				return worksheetReference.IsExternal;
			}
		}

		public virtual WorksheetReference WorksheetReference
		{
			get { return null; }
			protected set
			{
				Utilities.DebugFail("This should not be set on the base class.");
			}
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