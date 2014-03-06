using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;






using System.Drawing;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class USERBVIEWRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			uint idForCustomView = manager.CurrentRecordStream.ReadUInt32();
			Debug.Assert( idForCustomView == 2190 );

			int selectedWorksheetTabId = manager.CurrentRecordStream.ReadInt32();
			Guid id = new Guid( manager.CurrentRecordStream.ReadBytes( 16 ) );

			int leftTwips = manager.CurrentRecordStream.ReadInt32();
			int topTwips = manager.CurrentRecordStream.ReadInt32();
			int widthTwips = manager.CurrentRecordStream.ReadInt32();
			int heightTwips = manager.CurrentRecordStream.ReadInt32();

			ushort tabBarWidth = manager.CurrentRecordStream.ReadUInt16();

			ushort optionFlags1 = manager.CurrentRecordStream.ReadUInt16();

			bool showFormulaBar =											( optionFlags1 & 0x0001 ) == 0x0001;
			bool showStatusBar =											( optionFlags1 & 0x0002 ) == 0x0002;
			//byte noteDisplay =									(byte)( ( optionFlags1 & 0x000C ) >> 2 );	 // These don't seem to work when the custom view is applied in Excel
			bool hScrollBarVisible =										( optionFlags1 & 0x0010 ) == 0x0010;
			bool vScrollBarVisible =										( optionFlags1 & 0x0020 ) == 0x0020;
			bool tabBarVisible =											( optionFlags1 & 0x0040 ) == 0x0040;
			bool maximized =												( optionFlags1 & 0x0080 ) == 0x0080;
			ObjectDisplayStyle objectDisplayStyle =	  (ObjectDisplayStyle)( ( optionFlags1 & 0x0300 ) >> 8 );
			bool savePrintOptions =											( optionFlags1 & 0x0400 ) == 0x0400;
			bool saveHiddenRowsAndColumns =									( optionFlags1 & 0x0800 ) == 0x0800;
			//bool sheetDeletedOrHidden =									( optionFlags1 & 0x1000 ) == 0x1000; // What is this used for?
			//bool timedUpdateMode =										( optionFlags1 & 0x2000 ) == 0x2000; // What is this used for?
			//bool sharedLocalSessionChangesOnly =							( optionFlags1 & 0x4000 ) == 0x4000; // What is this used for?
			//bool sharedWorkbookConflictMode =								( optionFlags1 & 0x8000 ) == 0x8000; // What is this used for?

			ScrollBars scrollBars = ScrollBars.None;
			scrollBars |= hScrollBarVisible ? ScrollBars.Horizontal : 0;
			scrollBars |= vScrollBarVisible ? ScrollBars.Vertical : 0;

			manager.CurrentRecordStream.ReadUInt16(); // More options flags, don;t need any info out of these
			manager.CurrentRecordStream.ReadUInt16(); // is shared
			manager.CurrentRecordStream.ReadUInt16(); // merge interval?
			
			string customViewName = manager.CurrentRecordStream.ReadFormattedString( LengthType.SixteenBit ).UnformattedString;

			CustomView customView = manager.Workbook.CustomViews.Add( customViewName, savePrintOptions, saveHiddenRowsAndColumns );

			customView.Id = id;
			customView.WindowOptions.BoundsInPixels = new Rectangle( leftTwips, topTwips, widthTwips, heightTwips );
			customView.WindowOptions.Maximized = maximized;
			customView.WindowOptions.ObjectDisplayStyle = objectDisplayStyle;
			customView.WindowOptions.ScrollBars = scrollBars;
			customView.WindowOptions.SelectedWorksheetTabId = selectedWorksheetTabId;
			customView.WindowOptions.ShowFormulaBar = showFormulaBar;
			customView.WindowOptions.ShowStatusBar = showStatusBar;
			customView.WindowOptions.TabBarVisible = tabBarVisible;
			customView.WindowOptions.TabBarWidth = tabBarWidth;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			CustomView customView = (CustomView)manager.ContextStack[ typeof( CustomView ) ];

			if ( customView == null )
			{
				Utilities.DebugFail( "There is no custom view in the context stack." );
				return;
			}

			manager.CurrentRecordStream.Write( (uint)2190 ); // id (not sure why its 2190)

			// MD 9/27/08
			// The selected worksheet will now resolve itself
			//Worksheet selectedWorksheet = customView.WindowOptions.SelectedWorksheetResolved;
			Worksheet selectedWorksheet = customView.WindowOptions.SelectedWorksheet;

			if ( selectedWorksheet == null )
				manager.CurrentRecordStream.Write( (uint)0 );
			else
			{
				// MD 9/9/08 - Excel 2007 Format
				// Added a SheetID proeprty so we don't have to keep adding one to the index.
				//manager.CurrentRecordStream.Write( (uint)( selectedWorksheet.Index + 1 ) );
				manager.CurrentRecordStream.Write( (uint)( selectedWorksheet.SheetId ) );
			}

			manager.CurrentRecordStream.Write( customView.Id.ToByteArray() );

			manager.CurrentRecordStream.Write( (uint)customView.WindowOptions.BoundsInPixels.Left );
			manager.CurrentRecordStream.Write( (uint)customView.WindowOptions.BoundsInPixels.Top );
			manager.CurrentRecordStream.Write( (uint)customView.WindowOptions.BoundsInPixels.Width );
			manager.CurrentRecordStream.Write( (uint)customView.WindowOptions.BoundsInPixels.Height );

			manager.CurrentRecordStream.Write( (ushort)customView.WindowOptions.TabBarWidth );

			ushort optionFlags1 = 0x0004;

			if ( customView.WindowOptions.ShowFormulaBar )
				optionFlags1 |= 0x0001;

			if ( customView.WindowOptions.ShowStatusBar )
				optionFlags1 |= 0x0002;

			if ( ( customView.WindowOptions.ScrollBars & ScrollBars.Horizontal ) != 0 )
				optionFlags1 |= 0x0010;

			if ( ( customView.WindowOptions.ScrollBars & ScrollBars.Vertical ) != 0 )
				optionFlags1 |= 0x0020;

			if ( customView.WindowOptions.TabBarVisible )
				optionFlags1 |= 0x0040;

			if ( customView.WindowOptions.Maximized )
				optionFlags1 |= 0x0080;

			optionFlags1 |= (ushort)( (ushort)customView.WindowOptions.ObjectDisplayStyle << 8 );

			if ( customView.SavePrintOptions )
				optionFlags1 |= 0x0400;

			if ( customView.SaveHiddenRowsAndColumns )
				optionFlags1 |= 0x0800;

			manager.CurrentRecordStream.Write( (ushort)optionFlags1 );

			ushort optionFlags2 = 0x0064;

			manager.CurrentRecordStream.Write( (ushort)optionFlags2 );

			manager.CurrentRecordStream.Write( Convert.ToUInt16( false ) );
			manager.CurrentRecordStream.Write( (ushort)0xFFFF );

			manager.CurrentRecordStream.Write( customView.Name, LengthType.SixteenBit );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.USERBVIEW; }
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