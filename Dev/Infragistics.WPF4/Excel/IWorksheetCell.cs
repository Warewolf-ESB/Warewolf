// MD 4/12/11 - TFS67084
// Moved away from using WorksheetCell objects.
#region Removed

//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Infragistics.Documents.Excel
//{
//#if DEBUG
//    /// <summary>
//    /// Represents a cell which contains a value and format (could be a WorksheetCell or WorksheetMergedCellsRegioon).
//    /// </summary> 
//#endif
//    // MD 9/2/08 - Cell Comments
//    // All cells are FormattedString owners
//    //internal interface IWorksheetCell
//    internal interface IWorksheetCell : IFormattedStringOwner
//    {
//        // MD 7/14/08 - Excel formula solving
//#if DEBUG
//        /// <summary>
//        /// Notifies the workbook's calc network that the cell's value has changed.
//        /// </summary> 
//#endif
//        void DirtyReference();

//#if DEBUG
//        /// <summary>
//        /// Applies the value to the cell.
//        /// </summary> 
//#endif
//        void InternalSetValue( object value );

//        // MD 7/14/08 - Excel formula solving
//#if DEBUG
//        /// <summary>
//        /// Applies the formula of the cell on its calc reference if the value is a formula.
//        /// </summary> 
//#endif
//        void SetFormulaOnCalcReference( bool canClearPreviouslyCalculatedValue );


//#if DEBUG
//        /// <summary>
//        /// Gets the cell format for the cell.
//        /// </summary> 
//#endif
//        IWorksheetCellFormat CellFormat { get;}

//#if DEBUG
//        /// <summary>
//        /// Gets the column index of the cell (the column index of the top-left cell for merged cell regions).
//        /// </summary> 
//#endif
//        int ColumnIndex { get;}

//        // MD 7/14/08 - Excel formula solving
//#if DEBUG
//        /// <summary>
//        /// Gets the formula applied to the cell.
//        /// </summary> 
//#endif
//        Formula Formula { get; }

//#if DEBUG
//        /// <summary>
//        /// Gets the value indicating whether the cell's format is non-default.
//        /// </summary> 
//#endif
//        bool HasCellFormat { get;}

//#if DEBUG
//        /// <summary>
//        /// Gets the value indicating whether the cell still exists on a worksheet (only False for removed merged cell regions).
//        /// </summary>  
//#endif
//        bool IsOnWorksheet { get;}

//        // MD 7/14/08 - Excel formula solving
//#if DEBUG
//        /// <summary>
//        /// Gets a region blocking value which is applied to this cell as well as others.
//        /// </summary>  
//#endif
//        IWorksheetRegionBlockingValue RegionBlockingValue { get; }

//#if DEBUG
//        /// <summary>
//        /// Gets the row index of the cell (the row index of the top-left cell for merged cell regions).
//        /// </summary> 
//#endif
//        int RowIndex { get;}

//#if DEBUG
//        /// <summary>
//        /// Gets or sets the value of the cell.
//        /// </summary>  
//#endif
//        object Value { get;set;}

//        // MD 9/2/08 - Cell Comments
//        // This has been moved down to the base interface IFormattedStringOwner
////#if DEBUG
////        /// <summary>
////        /// Gets the worksheet on which the cell reside. This will always be non-null, even when IsOnWorksheet returns False.
////        /// </summary> 
////#endif
////        Worksheet Worksheet { get; }
//    }
//}

#endregion  // Removed
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