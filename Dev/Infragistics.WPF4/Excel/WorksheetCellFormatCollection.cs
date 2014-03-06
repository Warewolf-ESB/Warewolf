using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	// MD 4/18/11 - TFS62026
	// MD 2/2/12 - TFS100573
	//internal class WorksheetCellFormatCollection : GenericCachedCollection<WorksheetCellFormatData>
	internal class WorksheetCellFormatCollection : GenericCachedCollectionEx<WorksheetCellFormatData>
	{
		#region Member Variables

		// MD 1/2/12 - 12.1 - Cell Format Updates
		// The DefaultCellElement is no longer needed because only cell formats are stored in the collection now (no style formats)
		// so the DefaultElement of the collection is the DefaultCellElement.
		//private WorksheetCellFormatData defaultCellElement;

		#endregion  // Member Variables

		#region Constructor

		public WorksheetCellFormatCollection(WorksheetCellFormatData defaultElement, Workbook workbook)
			: base(defaultElement, workbook, workbook.MaxCellFormats) 
		{
			// MD 1/2/12 - 12.1 - Cell Format Updates
			// The DefaultCellElement is no longer needed because only cell formats are stored in the collection now (no style formats)
			// so the DefaultElement of the collection is the DefaultCellElement.
			//this.defaultCellElement = (WorksheetCellFormatData)this.EmptyElement.Clone();
			//this.defaultCellElement.Style = false;
			//GenericCacheElement.FindExistingOrAddToCache(this.defaultCellElement, this);
		}

		#endregion  // Constructor

		#region Methods

		// MD 2/6/12 - 12.1 - Cell Format Updates
		#region AddDirect

		internal void AddDirect(WorksheetCellFormatData cellFormat)
		{
			WorksheetCellFormatData existingFormat;
			bool result = this.cache.AddIfItemDoesntExist(cellFormat, out existingFormat);
			Debug.Assert(result, "The items should have been added to the collection.");
		}

		#endregion // AddDirect

		#endregion // Methods

		// MD 1/2/12 - 12.1 - Cell Format Updates
		// The DefaultCellElement is no longer needed because only cell formats are stored in the collection now (no style formats)
		// so the DefaultElement of the collection is the DefaultCellElement.
		#region Removed

		//#region DefaultCellElement
		//
		//public WorksheetCellFormatData DefaultCellElement
		//{
		//    get { return this.defaultCellElement; }
		//} 
		//
		//#endregion  // DefaultCellElement

		#endregion // Removed
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