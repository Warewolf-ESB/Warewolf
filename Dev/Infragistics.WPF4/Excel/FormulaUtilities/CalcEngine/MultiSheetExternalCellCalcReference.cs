using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 6/18/12 - TFS102878
	internal sealed class MultiSheetExternalCellCalcReference : ExcelRefBase
	{
		#region Member Variables

		private WorksheetCellAddress address;
		private WorksheetReferenceExternal firstWorksheetReference;
		private WorksheetReferenceExternal lastWorksheetReference;
		private ExcelCalcValue value;

		#endregion // Member Variables

		#region Constructor

		public MultiSheetExternalCellCalcReference(WorksheetReferenceExternal firstWorksheetReference, WorksheetReferenceExternal lastWorksheetReference, WorksheetCellAddress address)
		{
			Debug.Assert(firstWorksheetReference.FirstWorksheetIndex < lastWorksheetReference.FirstWorksheetIndex, "The first worksheet should be before the last worksheet.");
			Debug.Assert(firstWorksheetReference.WorkbookReference == lastWorksheetReference.WorkbookReference, "The first worksheet should be from the same workbook as the last worksheet.");

			this.firstWorksheetReference = firstWorksheetReference;
			this.lastWorksheetReference = lastWorksheetReference;
			this.address = address;

			WorkbookReferenceBase workbookReference = this.firstWorksheetReference.WorkbookReference;

			ArrayProxy[] values = new ArrayProxy[this.lastWorksheetReference.FirstWorksheetIndex - this.firstWorksheetReference.FirstWorksheetIndex + 1];
			for (int i = 0; i < values.Length; i++)
			{
				WorksheetReference worksheetReference = workbookReference.GetWorksheetReference(i + this.firstWorksheetReference.FirstWorksheetIndex);
				IExcelCalcReference calcReference = worksheetReference.GetCalcReference(this.address);
				values[i] = calcReference.Value.ToArrayProxy();
			}

			this.value = new ExcelCalcValue(values);
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region ColumnIndex

		public override short ColumnIndex
		{
			get { return this.address.ColumnIndex; }
		}

		#endregion // ColumnIndex

		#region ContainsReference

		public override bool ContainsReference(IExcelCalcReference inReference)
		{
			IExcelCalcReference reference = ExcelCalcEngine.GetResolvedReference(inReference);

			ExternalCellCalcReference externalCellReference = reference as ExternalCellCalcReference;
			if (externalCellReference != null)
			{
				return
					this.FirstWorksheetReference.WorkbookReference == externalCellReference.WorksheetReference.WorkbookReference &&
					this.FirstWorksheetReference.FirstWorksheetIndex <= externalCellReference.WorksheetReference.FirstWorksheetIndex &&
					externalCellReference.WorksheetReference.FirstWorksheetIndex <= this.LastWorksheetReference.FirstWorksheetIndex &&
					this.address == externalCellReference.Address;
			}

			ExternalRegionCalcReference externalRegionReference = reference as ExternalRegionCalcReference;
			if (externalRegionReference != null)
			{
				return
					this.FirstWorksheetReference.WorkbookReference == externalRegionReference.WorksheetReference.WorkbookReference &&
					this.FirstWorksheetReference.FirstWorksheetIndex <= externalRegionReference.WorksheetReference.FirstWorksheetIndex &&
					externalRegionReference.WorksheetReference.FirstWorksheetIndex <= this.LastWorksheetReference.FirstWorksheetIndex &&
					externalRegionReference.Address.Contains(this.address);
			}

			MultiSheetExternalCellCalcReference multiSheetExternalCellReference = reference as MultiSheetExternalCellCalcReference;
			if (multiSheetExternalCellReference != null)
			{
				return
					this.firstWorksheetReference.WorkbookReference == multiSheetExternalCellReference.FirstWorksheetReference.WorkbookReference &&
					multiSheetExternalCellReference.FirstWorksheetReference.FirstWorksheetIndex <= this.lastWorksheetReference.FirstWorksheetIndex &&
					this.firstWorksheetReference.FirstWorksheetIndex <= multiSheetExternalCellReference.LastWorksheetReference.FirstWorksheetIndex &&
					this.address == multiSheetExternalCellReference.Address;
			}

			MultiSheetExternalRegionCalcReference multiSheetExternalRegionReference = reference as MultiSheetExternalRegionCalcReference;
			if (multiSheetExternalRegionReference != null)
			{
				return
					this.firstWorksheetReference.WorkbookReference == multiSheetExternalRegionReference.FirstWorksheetReference.WorkbookReference &&
					multiSheetExternalRegionReference.FirstWorksheetReference.FirstWorksheetIndex <= this.lastWorksheetReference.FirstWorksheetIndex &&
					this.firstWorksheetReference.FirstWorksheetIndex <= multiSheetExternalRegionReference.LastWorksheetReference.FirstWorksheetIndex &&
					multiSheetExternalRegionReference.Address.Contains(this.address);
			}

			return false;
		}

		#endregion // ContainsReference

		#region ElementName

		public override string ElementName
		{
			get
			{
				return
					Utilities.CreateReferenceString(this.firstWorksheetReference.WorkbookReference.FileName, this.firstWorksheetReference.Name, this.lastWorksheetReference.Name) +
					this.address.ToString(false, false, this.Workbook.CurrentFormat, CellReferenceMode.A1);
			}
		}

		#endregion // ElementName

		#region Equals

		public override bool Equals(object obj)
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;
			if (reference == null)
				return false;

			MultiSheetExternalCellCalcReference other = ExcelCalcEngine.GetResolvedReference(reference) as MultiSheetExternalCellCalcReference;
			if (other == null)
				return false;

			return
				this.firstWorksheetReference == other.firstWorksheetReference &&
				this.lastWorksheetReference == other.lastWorksheetReference &&
				this.address == other.address;
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return
				this.firstWorksheetReference.GetHashCode() ^
				this.lastWorksheetReference.GetHashCode() << 1 ^
				this.address.GetHashCode() << 2;
		}

		#endregion // GetHashCode

		#region IsSubsetReference

		public override bool IsSubsetReference(IExcelCalcReference inReference)
		{
			Utilities.DebugFail("This seems to only be called on formula owners and an instance of MultiSheetExternalCellCalcReference cannot own a formula.");
			return false;
		}

		#endregion // IsSubsetReference

		#region Row

		public override WorksheetRow Row
		{
			get { return null; }
		}

		#endregion // Row

		#region Value

		public override ExcelCalcValue Value
		{
			get { return this.value; }
			set
			{
				Utilities.DebugFail("Cannot set the value of this reference.");
			}
		}

		#endregion // Value

		#region Workbook

		public override Workbook Workbook
		{
			get { return this.firstWorksheetReference.WorkbookReference.TargetWorkbook; }
		}

		#endregion // Workbook

		#endregion // Base Class Overrides

		#region Properties

		#region Address

		public WorksheetCellAddress Address
		{
			get { return this.address; }
		}

		#endregion // Address

		#region FirstWorksheetReference

		public WorksheetReference FirstWorksheetReference
		{
			get { return this.firstWorksheetReference; }
		}

		#endregion // FirstWorksheetReference

		#region LastWorksheetReference

		public WorksheetReference LastWorksheetReference
		{
			get { return this.lastWorksheetReference; }
		}

		#endregion // LastWorksheetReference

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