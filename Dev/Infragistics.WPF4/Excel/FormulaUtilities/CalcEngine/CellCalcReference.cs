using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	internal sealed class CellCalcReference : ExcelRefBase
	{
		#region Member Variables

		private ExcelCalcValue cachedStaticCellValue;

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private WorksheetCell cell;
		private WorksheetRow row;
		private short columnIndex;

		// MD 1/12/12 - TFS99279
		private bool isInRecalcChain;

		#endregion Member Variables

		#region Constructor

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public CellCalcReference( WorksheetCell cell )
		//{
		//    this.cell = cell;
		//}
		public CellCalcReference(WorksheetRow row, short columnIndex)
		{
			this.row = row;
			this.columnIndex = columnIndex;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region CanOwnFormula

		public override bool CanOwnFormula
		{
			get { return true; }
		} 

		#endregion CanOwnFormula

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//#region Cell

		//public override WorksheetCell Cell
		//{
		//    get { return this.cell; }
		//}

		//#endregion Cell

		// MD 4/12/11 - TFS67084
		#region ColumnIndex

		public override short ColumnIndex
		{
			get { return this.columnIndex; }
		} 

		#endregion  // ColumnIndex

		#region ContainsReference

		public override bool ContainsReference( IExcelCalcReference inReference )
		{
			// MD 2/24/12 - 12.1 - Table Support
			// Rewrote this code using utility methods.
			#region Old Code

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////WorksheetCell otherCell = inReference.Context as WorksheetCell;
			////if ( otherCell != null )
			////    return this.cell == otherCell;
			////
			////WorksheetRegion otherRegion = inReference.Context as WorksheetRegion;
			////if ( otherRegion != null )
			////    return otherRegion.Contains( this.cell );
			////
			////List<WorksheetRegion> otherRegionGroup = inReference.Context as List<WorksheetRegion>;
			////if ( otherRegionGroup != null )
			////{
			////    foreach ( WorksheetRegion otherRegion2 in otherRegionGroup )
			////    {
			////        if ( otherRegion2.Contains( this.cell ) )
			////            return true;
			////    }
			////
			////    return false;
			////}
			////
			////return false;
			//CellCalcReference otherCellReference = inReference.Context as CellCalcReference;
			//if (otherCellReference != null)
			//    return this.Equals(otherCellReference);

			//WorksheetRegion otherRegion = inReference.Context as WorksheetRegion;
			//if (otherRegion != null)
			//    return otherRegion.Contains(this.row, this.columnIndex);

			//List<WorksheetRegion> otherRegionGroup = inReference.Context as List<WorksheetRegion>;
			//if (otherRegionGroup != null)
			//{
			//    foreach (WorksheetRegion otherRegion2 in otherRegionGroup)
			//    {
			//        if (otherRegion2.Contains(this.row, this.columnIndex))
			//            return true;
			//    }

			//    return false;
			//}

			//return false;

			#endregion // Old Code
			IList<WorksheetRegion> regionGroup = CalcUtilities.GetRegionGroup(inReference);
			if (regionGroup == null)
				return false;

			for (int i = 0; i < regionGroup.Count; i++)
			{
				if (regionGroup[i].Contains(this.row, this.columnIndex))
					return true;
			}

			return false;
		} 

		#endregion ContainsReference

		#region Context

		public override object Context
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//get { return this.cell; }
			get { return this; }
		}

		#endregion Context

		#region ElementName

		public override string ElementName
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//get { return this.cell.ToString( CellReferenceMode.A1, true ); }
			get { return this.row.GetCellAddressString(this.columnIndex, CellReferenceMode.A1, true); }
		}

		#endregion ElementName

		#region Equals

		public override bool Equals( object obj )
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;

			if ( reference == null )
				return false;

			CellCalcReference cellReference = ExcelCalcEngine.GetResolvedReference( reference ) as CellCalcReference;

			if ( cellReference == null )
				return false;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//return this.cell == cellReference.Cell;
			return this.row == cellReference.row && this.columnIndex == cellReference.columnIndex;
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
		    //return this.Cell.GetHashCode();
			return this.row.GetHashCode() ^ this.columnIndex.GetHashCode();
		} 

		#endregion GetHashCode

		// MD 2/24/12 - 12.1 - Table Support
		#region GetRegionGroup

		public override IList<WorksheetRegion> GetRegionGroup()
		{
			return new WorksheetRegion[] { 
				row.Worksheet.GetCachedRegion(this.row.Index, this.columnIndex, this.row.Index, this.columnIndex) 
			};
		}

		#endregion // GetRegionGroup

		#region IsSubsetReference

		public override bool IsSubsetReference( IExcelCalcReference inReference )
		{
			// MD 2/24/12 - 12.1 - Table Support
			//IExcelCalcReference resolvedReference = UltraCalcEngine.GetResolvedReference( inReference );
			//
			//if ( resolvedReference is UCReference )
			//    return false;
			//
			//WorksheetRegion[] regionGroup = CalcUtilities.GetRegionGroup( resolvedReference.Context );
			//
			//if ( regionGroup.Length == 0 )
			//    return false;
			IList<WorksheetRegion> regionGroup = CalcUtilities.GetRegionGroup(inReference);
			if (regionGroup == null)
				return false;

			foreach ( WorksheetRegion region in regionGroup )
			{
				if ( region.IsSingleCell == false )
					return false;

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//if ( region.TopLeftCell != this.cell )
				//    return false;
				if (region.FirstRow != this.row.Index || region.FirstColumnInternal != this.columnIndex)
					return false;
			}

			return true;
		} 

		#endregion IsSubsetReference

		// MD 4/12/11 - TFS67084
		#region Row

		public override WorksheetRow Row
		{
			get { return this.row; }
		}

		#endregion  // Row

		#region ValueInternal

		protected override ExcelCalcValue ValueInternal
		{
			get
			{
				// If the cell's worksheet has been removed from the workbook, return a #REF! error.
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//if ( this.cell.Worksheet.Workbook == null )
				if (this.row.Worksheet.Workbook == null)
					return ExcelReferenceError.Instance.Value;

				if ( this.Formula != null )
					return base.ValueInternal;

				if ( this.cachedStaticCellValue == null )
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//this.cachedStaticCellValue = CalcUtilities.CreateExcelCalcValue( this.cell.Value );
					this.cachedStaticCellValue = CalcUtilities.CreateExcelCalcValue(this.row.GetCellValueInternal(this.columnIndex));
				}

				return this.cachedStaticCellValue;
			}
			set
			{
				base.ValueInternal = value;

				// The calc value should have been unwrapped and dereferenced at this point, so just get the Value directly from the value.
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//this.cell.CalculatedValue = CalcUtilities.GetValueFormExcelCalcValue( value.Value );
				WorksheetCellBlock cellBlock;
				if (row.TryGetCellBlock(columnIndex, out cellBlock) == false)
				{
					// If the cache isn't even created, it can't have a formula or data table applied, so it can't have a calculated value set.
					Utilities.DebugFail("A cell without a formula or data table should not have its calculated value set.");
					return;
				}

				cellBlock.SetCalculatedValue(row, columnIndex, CalcUtilities.GetValueFormExcelCalcValue(value.Value));
			}
		}

		#endregion ValueInternal

		#endregion Base Class Overrides

		#region Methods

		#region ClearCachedStaticValue

		internal void ClearCachedStaticValue()
		{
			this.cachedStaticCellValue = null;
		} 

		#endregion ClearCachedStaticValue

		#region GetRelativeAddressesInArrayFormula

		internal void GetRelativeAddressesInArrayFormula( out int relativeColumn, out int relativeRow )
		{
			ArrayFormula arrayFormula = this.ExcelFormula as ArrayFormula;

			if ( arrayFormula == null )
			{
				Utilities.DebugFail( "The array formula should not be null if this was called." );
				relativeRow = 0;
				relativeColumn = 0;
				return;
			}

			WorksheetRegion cellsRange = arrayFormula.CellRange;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//relativeRow = this.cell.RowIndex - cellsRange.FirstRow;
			//relativeColumn = this.cell.ColumnIndex - cellsRange.FirstColumn;
			relativeRow = this.row.Index - cellsRange.FirstRow;
			relativeColumn = this.columnIndex - cellsRange.FirstColumn;
		}  

		#endregion GetRelativeAddressesInArrayFormula

		// MD 12/2/11 - TFS97046
		#region IsHidden

		public bool IsHidden
		{
			get
			{
				if (this.row.Hidden)
					return true;

				// MD 3/15/12 - TFS104581
				//WorksheetColumn column = this.row.Worksheet.Columns.GetIfCreated(this.columnIndex);
				//if (column != null && column.Hidden)
				//    return true;
				//
				//return false;
				return this.row.Worksheet.IsColumnHidden(this.columnIndex);
			}
		}

		#endregion // IsHidden

		#endregion Methods

		#region Properties

		#region HasArrayFormula

		internal bool HasArrayFormula
		{
			get { return this.ExcelFormula is ArrayFormula; }
		} 

		#endregion HasArrayFormula

		// MD 1/12/12 - TFS99279
		#region IsInRecalcChain

		public bool IsInRecalcChain
		{
			get { return this.isInRecalcChain; }
			set { this.isInRecalcChain = value; }
		}

		#endregion  // IsInRecalcChain

		#endregion Properties
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