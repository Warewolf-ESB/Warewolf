using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd947893(v=office.12)
	internal class WINDOW2Record : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			worksheet.DisplayOptions.ShowFormulasInCells =			( optionFlags & 0x0001 ) == 0x0001;
			worksheet.DisplayOptions.ShowGridlines =				( optionFlags & 0x0002 ) == 0x0002;
			worksheet.DisplayOptions.ShowRowAndColumnHeaders =		( optionFlags & 0x0004 ) == 0x0004;
			worksheet.DisplayOptions.PanesAreFrozen =				( optionFlags & 0x0008 ) == 0x0008;
			worksheet.DisplayOptions.ShowZeroValues =				( optionFlags & 0x0010 ) == 0x0010;
			worksheet.DisplayOptions.UseAutomaticGridlineColor =	( optionFlags & 0x0020 ) == 0x0020;
			worksheet.DisplayOptions.OrderColumnsRightToLeft =		( optionFlags & 0x0040 ) == 0x0040;
			worksheet.DisplayOptions.ShowOutlineSymbols =			( optionFlags & 0x0080 ) == 0x0080;
			//bool removeSplitsIfPaneFreezeRemoved =				( optionFlags & 0x0100 ) == 0x0100;
			//bool selected =										( optionFlags & 0x0200 ) == 0x0200;
			bool sheetIsCurrentlyDisplayed =						( optionFlags & 0x0400 ) == 0x0400;
			bool showInPageBreakView =								( optionFlags & 0x0800 ) == 0x0800;

			if ( showInPageBreakView )
				worksheet.DisplayOptions.View = WorksheetView.PageBreakPreview;

			Debug.Assert( sheetIsCurrentlyDisplayed == false || worksheet.Workbook.WindowOptions.SelectedWorksheet == worksheet );

			ushort firstVisibleRow = manager.CurrentRecordStream.ReadUInt16();
			ushort firstVisibleColumn = manager.CurrentRecordStream.ReadUInt16();

			if ( worksheet.DisplayOptions.PanesAreFrozen )
			{
				worksheet.DisplayOptions.FrozenPaneSettings.FirstColumnInRightPane = firstVisibleColumn;
				worksheet.DisplayOptions.FrozenPaneSettings.FirstRowInBottomPane = firstVisibleRow;
			}
			else
			{
				worksheet.DisplayOptions.UnfrozenPaneSettings.FirstColumnInRightPane = firstVisibleColumn;
				worksheet.DisplayOptions.UnfrozenPaneSettings.FirstRowInBottomPane = firstVisibleRow;
			}

			worksheet.DisplayOptions.GridlineColorIndex = manager.CurrentRecordStream.ReadUInt16();

			manager.CurrentRecordStream.ReadUInt16(); // not used

			// MD 11/11/09 - TFS24618
			// The method using this value requires an int parameter.
			//ushort magnificationInPageBreakView = manager.CurrentRecordStream.ReadUInt16();
			int magnificationInPageBreakView = manager.CurrentRecordStream.ReadUInt16();

			if ( magnificationInPageBreakView == 0 )
				magnificationInPageBreakView = DisplayOptions.DefaultMagnificationInPageBreakView;

			// MD 11/11/09 - TFS24618
			// Make sure setting the magnification will not throw an exception.
			Utilities.EnsureMagnificationIsValid( ref magnificationInPageBreakView );

			worksheet.DisplayOptions.MagnificationInPageBreakView = magnificationInPageBreakView;

			// MD 11/11/09 - TFS24618
			// The method using this value requires an int parameter.
			//ushort magnificationInNormalView = manager.CurrentRecordStream.ReadUInt16();
			int magnificationInNormalView = manager.CurrentRecordStream.ReadUInt16();

			if ( magnificationInNormalView == 0 )
				magnificationInNormalView = DisplayOptions.DefaultMagnificationInNormalView;

			// MD 11/11/09 - TFS24618
			// Make sure setting the magnification will not throw an exception.
			Utilities.EnsureMagnificationIsValid( ref magnificationInNormalView );

			worksheet.DisplayOptions.MagnificationInNormalView = magnificationInNormalView;

			manager.CurrentRecordStream.ReadUInt32(); // not used
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort optionFlags = 0;

			if ( worksheet.DisplayOptions.ShowFormulasInCells )
				optionFlags |= 0x0001;

			if ( worksheet.DisplayOptions.ShowGridlines )
				optionFlags |= 0x0002;

			if ( worksheet.DisplayOptions.ShowRowAndColumnHeaders )
				optionFlags |= 0x0004;

			if ( worksheet.DisplayOptions.PanesAreFrozen )
				optionFlags |= 0x0008;

			if ( worksheet.DisplayOptions.ShowZeroValues )
				optionFlags |= 0x0010;

			if ( worksheet.DisplayOptions.UseAutomaticGridlineColor )
				optionFlags |= 0x0020;

			if ( worksheet.DisplayOptions.OrderColumnsRightToLeft )
				optionFlags |= 0x0040;

			if ( worksheet.DisplayOptions.ShowOutlineSymbols )
				optionFlags |= 0x0080;

			if ( worksheet.DisplayOptions.PanesAreFrozen )
				optionFlags |= 0x0100; // remove splits if pane freeze removed

			// MD 9/27/08
			// The selected worksheet will now resolve itself
			//if ( worksheet == worksheet.Workbook.WindowOptions.SelectedWorksheetResolved )
			if ( worksheet == worksheet.Workbook.WindowOptions.SelectedWorksheet )
			{
				optionFlags |= 0x0200; // selected
				optionFlags |= 0x0400; // sheet is currently displayed
			}

			if ( worksheet.DisplayOptions.View == WorksheetView.PageBreakPreview )
				optionFlags |= 0x0800;

			manager.CurrentRecordStream.Write( (ushort)optionFlags );

			if ( worksheet.DisplayOptions.PanesAreFrozen )
			{
				if ( worksheet.DisplayOptions.FrozenPaneSettings.FrozenRows == 0 )
					manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.FrozenPaneSettings.FirstRowInBottomPane );
				else
					manager.CurrentRecordStream.Write( (ushort)0 );

				if ( worksheet.DisplayOptions.FrozenPaneSettings.FrozenColumns == 0 )
					manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.FrozenPaneSettings.FirstColumnInRightPane );
				else
					manager.CurrentRecordStream.Write( (ushort)0 );
			}
			else
			{
				if ( worksheet.DisplayOptions.UnfrozenPaneSettings.TopPaneHeight == 0 )
					manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.FirstRowInBottomPane );
				else
					manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.FirstRowInTopPane );

				if ( worksheet.DisplayOptions.UnfrozenPaneSettings.LeftPaneWidth == 0 )
					manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.FirstColumnInRightPane );
				else
					manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.UnfrozenPaneSettings.FirstColumnInLeftPane );
			}

			manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.GridlineColorIndex );
			manager.CurrentRecordStream.Write( (ushort)0 );

			int magnificationInPageBreakView = worksheet.DisplayOptions.MagnificationInPageBreakView;

			// MD 7/23/12 - TFS117430
			// It appears that the current magnification level is always written out, even when it is the default.
			//if ( magnificationInPageBreakView == DisplayOptions.DefaultMagnificationInPageBreakView )
			if (magnificationInPageBreakView == DisplayOptions.DefaultMagnificationInPageBreakView &&
				worksheet.DisplayOptions.View != WorksheetView.PageBreakPreview)
			{
				magnificationInPageBreakView = 0;
			}

			manager.CurrentRecordStream.Write( (ushort)magnificationInPageBreakView );

			int magnificationInNormalView = worksheet.DisplayOptions.MagnificationInNormalView;

			// MD 7/23/12 - TFS117430
			// It appears that the current magnification level is always written out, even when it is the default.
			//if ( magnificationInNormalView == DisplayOptions.DefaultMagnificationInNormalView )
			if (magnificationInNormalView == DisplayOptions.DefaultMagnificationInNormalView &&
				worksheet.DisplayOptions.View != WorksheetView.Normal)
			{
				magnificationInNormalView = 0;
			}

			manager.CurrentRecordStream.Write( (ushort)magnificationInNormalView );

			manager.CurrentRecordStream.Write( (uint)0 );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.WINDOW2; }
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