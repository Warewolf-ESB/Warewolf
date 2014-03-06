using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class PANERecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort horizontalSplitPosition = manager.CurrentRecordStream.ReadUInt16();
			ushort verticalSplitPosition = manager.CurrentRecordStream.ReadUInt16();

			ushort topRowVisibleInBottom = manager.CurrentRecordStream.ReadUInt16();
			ushort leftColumnVisibleInRight = manager.CurrentRecordStream.ReadUInt16();

			// MD 5/24/07
			// Property removed: uncomment when property is re-added (remove read call below comment)
			//PaneLocation activePane = (PaneLocation)manager.CurrentRecordStream.ReadUInt16();
			manager.CurrentRecordStream.ReadUInt16();

			if ( worksheet.DisplayOptions.PanesAreFrozen )
			{
				if ( horizontalSplitPosition > 0 )
					worksheet.DisplayOptions.FrozenPaneSettings.FirstColumnInRightPane = leftColumnVisibleInRight;

				if ( verticalSplitPosition > 0 )
					worksheet.DisplayOptions.FrozenPaneSettings.FirstRowInBottomPane = topRowVisibleInBottom;

				worksheet.DisplayOptions.FrozenPaneSettings.FrozenColumns = horizontalSplitPosition;
				worksheet.DisplayOptions.FrozenPaneSettings.FrozenRows = verticalSplitPosition;

				// MD 5/24/07
				// Property removed: uncomment when property is re-added
				//worksheet.DisplayOptions.FrozenPaneSettings.PaneWithActiveCell = activePane;
			}
			else
			{
				if ( horizontalSplitPosition > 0 )
				{
					worksheet.DisplayOptions.UnfrozenPaneSettings.FirstColumnInLeftPane = worksheet.DisplayOptions.UnfrozenPaneSettings.FirstColumnInRightPane;
					worksheet.DisplayOptions.UnfrozenPaneSettings.FirstColumnInRightPane = leftColumnVisibleInRight;
				}

				if ( verticalSplitPosition > 0 )
				{
					worksheet.DisplayOptions.UnfrozenPaneSettings.FirstRowInTopPane = worksheet.DisplayOptions.UnfrozenPaneSettings.FirstRowInBottomPane;
					worksheet.DisplayOptions.UnfrozenPaneSettings.FirstRowInBottomPane = topRowVisibleInBottom;
				}

				worksheet.DisplayOptions.UnfrozenPaneSettings.LeftPaneWidth = horizontalSplitPosition;
				worksheet.DisplayOptions.UnfrozenPaneSettings.TopPaneHeight = verticalSplitPosition;

				// MD 5/24/07
				// Property removed: uncomment when property is re-added
				//worksheet.DisplayOptions.UnfrozenPaneSettings.PaneWithActiveCell = activePane;
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			if ( worksheet.DisplayOptions.PanesAreFrozen )
			{
				ushort frozenRows = (ushort)worksheet.DisplayOptions.FrozenPaneSettings.FrozenRows;
				ushort frozenColumns = (ushort)worksheet.DisplayOptions.FrozenPaneSettings.FrozenColumns;

				manager.CurrentRecordStream.Write( frozenColumns );
				manager.CurrentRecordStream.Write( frozenRows );

				if ( worksheet.DisplayOptions.FrozenPaneSettings.FrozenRows > 0 )
				{
					// The first row visible in the bottom pane cannot be one of the frozen rows
					ushort firstRowInBottomPane = Math.Max( frozenRows, (ushort)worksheet.DisplayOptions.FrozenPaneSettings.FirstRowInBottomPane );
					manager.CurrentRecordStream.Write( firstRowInBottomPane );
				}
				else
					manager.CurrentRecordStream.Write( (ushort)0 );

				if ( worksheet.DisplayOptions.FrozenPaneSettings.FrozenColumns > 0 )
				{
					// The first column visible in the right pane cannot be one of the frozen column
					ushort firstColumnInRightPane = Math.Max( frozenColumns, (ushort)worksheet.DisplayOptions.FrozenPaneSettings.FirstColumnInRightPane );
					manager.CurrentRecordStream.Write( firstColumnInRightPane );
				}
				else
					manager.CurrentRecordStream.Write( (ushort)0 );

				// MD 5/24/07
				// Property removed: uncomment when property is re-added (remove write call below comment)
				//manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.FrozenPaneSettings.PaneWithActiveCell );
				// 8/27/07 - BR25847
				// The top-right (1) pane must be active for scrolling to work correctly
				//manager.CurrentRecordStream.Write( (ushort)3 );
				// MD 10/19/07 - BR27421
				// Apparently excel doesn't like having a 1 in there, but a 2 seems to work fine and doens't break BR25847
				//manager.CurrentRecordStream.Write( (ushort)1 );
				// MD 12/13/07 - BR25847
				// I don't know how I missed this before, but apparently using 2 does break BR25847. It looks like saving a file in Excel 
				// writes out 0 for this field and fixes both bugs (I should have examined an Excel saved file from the beginning).
				//manager.CurrentRecordStream.Write( (ushort)2 );
				// MD 1/2/08 - BR28895
				// Ok, I think I might have it now: apparently the active pane varies based on what is frozen and must always point to the 
				// unfrozen pane of the view.
				//manager.CurrentRecordStream.Write( (ushort)0 );
				PaneLocation unfrozenPane;

				if ( frozenRows != 0 && frozenColumns != 0 )
					unfrozenPane = PaneLocation.BottomRight;
				else if ( frozenRows != 0 )
					unfrozenPane = PaneLocation.BottomLeft;
				else if ( frozenColumns != 0 )
					unfrozenPane = PaneLocation.TopRight;
				else
					unfrozenPane = PaneLocation.TopLeft;

				manager.CurrentRecordStream.Write( (ushort)unfrozenPane );
			}
			else
			{
				// MD 8/1/08 - BR35180
				// Cache these values so we don't get them multiple times below. All references to the properties have been replaced below.
				ushort leftPaneWidth = (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.LeftPaneWidth;
				ushort topPaneHeight = (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.TopPaneHeight;

				manager.CurrentRecordStream.Write( leftPaneWidth );
				manager.CurrentRecordStream.Write( topPaneHeight );

				if ( topPaneHeight > 0 )
					manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.FirstRowInBottomPane );
				else
					manager.CurrentRecordStream.Write( (ushort)0 );

				if ( leftPaneWidth > 0 )
					manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.FirstColumnInRightPane );
				else
					manager.CurrentRecordStream.Write( (ushort)0 );

				// MD 5/24/07
				// Property removed: uncomment when property is re-added (remove write call below comment)
				//manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.PaneWithActiveCell );
				// 8/27/07 - BR25847
				// The top-right (1) pane must be active for scrolling to work correctly
				//manager.CurrentRecordStream.Write( (ushort)3 );
				// MD 8/1/08 - BR35180
				// It looks like the same rules for frozen panes apply to unfrozen panes: the active pane varies based on what is unfrozen 
				// and must always point to the bottom-right most pane.
				//manager.CurrentRecordStream.Write( (ushort)1 );
				PaneLocation unfrozenPane;

				if ( topPaneHeight > 0 && leftPaneWidth > 0 )
					unfrozenPane = PaneLocation.BottomRight;
				else if ( topPaneHeight > 0 )
					unfrozenPane = PaneLocation.BottomLeft;
				else if ( leftPaneWidth > 0 )
					unfrozenPane = PaneLocation.TopRight;
				else
					unfrozenPane = PaneLocation.TopLeft;

				manager.CurrentRecordStream.Write( (ushort)unfrozenPane );
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.PANE; }
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