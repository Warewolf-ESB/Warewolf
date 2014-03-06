using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class EOFRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Debug.Assert( manager.ContextStack.Current is Workbook || manager.ContextStack.Current is Worksheet );

			object currentContext = manager.ContextStack.Pop();

			if ( currentContext is Workbook )
			{
				// MD 9/9/08 - Excel 2007 Format
				// Moved to Workbook.OnAfterLoadGlobalSettings because it is used in multiple places now.
				#region Moved

				//manager.Workbook.WindowOptions.SelectedWorksheet = manager.Workbook.Worksheets[ manager.Workbook.WindowOptions.SelectedWorksheetIndex ];
				//
				//if ( manager.Workbook.HasCustomViews )
				//{
				//    foreach ( CustomView customView in manager.Workbook.CustomViews )
				//    {
				//        if ( manager.WorksheetIndices.ContainsKey( customView.WindowOptions.SelectedWorksheetTabId ) )
				//        {
				//            int worksheetIndex = manager.WorksheetIndices[ customView.WindowOptions.SelectedWorksheetTabId ];
				//            customView.WindowOptions.SelectedWorksheet = manager.Workbook.Worksheets[ worksheetIndex ];
				//        }
				//    }
				//}
				//
				//// MD 8/20/07 - BR25818
				//// After the workbook global section has been deserialized, resolve all 
				//// named reference names in other named reference formulas.
				//manager.ResolveNamedReferences(); 

				#endregion Moved
				manager.Workbook.OnAfterLoadGlobalSettings( manager );
			}
			else
			{
				// MD 7/20/2007 - BR25039
				// We need to do some work with the worksheet object now
				//Debug.Assert( currentContext is Worksheet );
				Worksheet worksheet = currentContext as Worksheet;
				Debug.Assert( worksheet != null );

				// Clear out all shapes which shouldn't be in the shapes collection
				if ( worksheet != null )
					worksheet.Shapes.RemoveInvalidShapes();

				manager.ContextStack.Pop(); // Print settings
				manager.ContextStack.Pop(); // Display settings

				manager.NextWorksheetIndex++;

				// MD 3/30/10 - TFS30253
				// Shared formulas can only be shared in the worksheet, so after loading the current worksheet, clear the shared formulas.
				manager.SharedFormulas.Clear();

				// MD 9/27/11 - TFS88499
				// We should also clear the pending roots because shared formula can only be shared in the worksheet.
				manager.PendingSharedFormulaRoots.Clear();
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager ) { }

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.EOF; }
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