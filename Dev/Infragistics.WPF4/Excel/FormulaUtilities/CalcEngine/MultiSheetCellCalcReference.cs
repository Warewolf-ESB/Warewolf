using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 6/18/12 - TFS102878
	internal sealed class MultiSheetCellCalcReference : ExcelRefBase
	{
		#region Member Variables

		private WorksheetCellAddress address;
		private Worksheet firstWorksheetReference;
		private Worksheet lastWorksheetReference;
		private List<WorksheetRegion> regionGroup;

		#endregion // Member Variables

		#region Constructor

		public MultiSheetCellCalcReference(Worksheet firstWorksheetReference, Worksheet lastWorksheetReference, WorksheetCellAddress address)
		{
			Debug.Assert(firstWorksheetReference.Index < lastWorksheetReference.Index, "The first worksheet should be before the last worksheet.");
			Debug.Assert(firstWorksheetReference.Workbook == lastWorksheetReference.Workbook, "The first worksheet should be from the same workbook as the last worksheet.");

			this.firstWorksheetReference = firstWorksheetReference;
			this.lastWorksheetReference = lastWorksheetReference;
			this.address = address;

			WorksheetCollection worksheets = this.firstWorksheetReference.Workbook.Worksheets;
			WorksheetRegionAddress regionAddress = new WorksheetRegionAddress(this.address.RowIndex, this.address.RowIndex, this.address.ColumnIndex, this.address.ColumnIndex);

			this.regionGroup = new List<WorksheetRegion>();
			for (int i = this.firstWorksheetReference.Index; i <= this.lastWorksheetReference.Index; i++)
				this.regionGroup.Add(worksheets[i].GetCachedRegion(regionAddress));
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
			IList<WorksheetRegion> regionGroup = CalcUtilities.GetRegionGroup(inReference);
			if (regionGroup == null)
				return false;

			for (int i = 0; i < regionGroup.Count; i++)
			{
				WorksheetRegion region = regionGroup[i];
				if (region.Worksheet.Index < this.firstWorksheetReference.Index || this.lastWorksheetReference.Index < region.Worksheet.Index)
					continue;

				if (region.Address.Contains(this.address))
					return true;
			}

			return false;
		}

		#endregion // ContainsReference

		#region Context

		public override object Context
		{
			get { return this.regionGroup; }
		}

		#endregion // Context

		#region ElementName

		public override string ElementName
		{
			get
			{
				return
					Utilities.CreateReferenceString(null, this.firstWorksheetReference.Name, this.lastWorksheetReference.Name) +
					this.address.ToString(false, false, this.firstWorksheetReference.CurrentFormat, CellReferenceMode.A1);
			}
		}

		#endregion // ElementName

		#region Equals

		public override bool Equals(object obj)
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;
			if (reference == null)
				return false;

			MultiSheetCellCalcReference other = ExcelCalcEngine.GetResolvedReference(reference) as MultiSheetCellCalcReference;
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

		#region GetRegionGroup

		public override IList<WorksheetRegion> GetRegionGroup()
		{
			return this.regionGroup;
		}

		#endregion // GetRegionGroup

		#region IsSubsetReference

		public override bool IsSubsetReference(IExcelCalcReference inReference)
		{
			Utilities.DebugFail("This seems to only be called on formula owners and an instance of MultiSheetCellCalcReference cannot own a formula.");
			return false;
		}

		#endregion // IsSubsetReference

		#region Row

		public override WorksheetRow Row
		{
			get { return this.firstWorksheetReference.Rows[this.address.RowIndex]; }
		}

		#endregion // Row

		#region ValueInternal

		protected override ExcelCalcValue ValueInternal
		{
			get
			{
				ExcelCalcValue value = base.ValueInternal;

				if (value == null)
				{
					ArrayProxy[] arrayProxyGroup = new ArrayProxy[this.regionGroup.Count];
					for (int i = 0; i < this.regionGroup.Count; i++)
						arrayProxyGroup[i] = new RegionCalcReference.RegionArrayProxy(this.regionGroup[i]);

					value = new ExcelCalcValue(arrayProxyGroup);
					this.ValueInternal = value;
				}

				return value;
			}
		}

		#endregion ValueInternal

		#endregion // Base Class Overrides
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