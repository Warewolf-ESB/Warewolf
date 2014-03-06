using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;






using System.Drawing;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class WINDOW1Record : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			short leftTwips = manager.CurrentRecordStream.ReadInt16();
			short topTwips = manager.CurrentRecordStream.ReadInt16();
			ushort widthTwips = manager.CurrentRecordStream.ReadUInt16();
			ushort heightTwips = manager.CurrentRecordStream.ReadUInt16();

			manager.Workbook.WindowOptions.BoundsInTwips = new Rectangle( leftTwips, topTwips, widthTwips, heightTwips );

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			//bool hidden =									( optionFlags & 0x0001 ) == 0x0001;
			manager.Workbook.WindowOptions.Minimized =		( optionFlags & 0x0002 ) == 0x0002;
			bool hScrollBarVisible =						( optionFlags & 0x0008 ) == 0x0008;
			bool vScrollBarVisible =						( optionFlags & 0x0010 ) == 0x0010;
			manager.Workbook.WindowOptions.TabBarVisible =	( optionFlags & 0x0020 ) == 0x0020;

			ScrollBars scrollBars = ScrollBars.None;
			scrollBars |= hScrollBarVisible ? ScrollBars.Horizontal : 0;
			scrollBars |= vScrollBarVisible ? ScrollBars.Vertical : 0;

			manager.Workbook.WindowOptions.ScrollBars = scrollBars;

			manager.Workbook.WindowOptions.SelectedWorksheetIndex = manager.CurrentRecordStream.ReadUInt16();

			manager.Workbook.WindowOptions.FirstVisibleTabIndex = manager.CurrentRecordStream.ReadUInt16();
			manager.CurrentRecordStream.ReadUInt16(); // selected worksheets
			manager.Workbook.WindowOptions.TabBarWidth = manager.CurrentRecordStream.ReadUInt16();
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			manager.CurrentRecordStream.Write( (ushort)manager.Workbook.WindowOptions.BoundsInTwips.Left );
			manager.CurrentRecordStream.Write( (ushort)manager.Workbook.WindowOptions.BoundsInTwips.Top );
			manager.CurrentRecordStream.Write( (ushort)manager.Workbook.WindowOptions.BoundsInTwips.Width );
			manager.CurrentRecordStream.Write( (ushort)manager.Workbook.WindowOptions.BoundsInTwips.Height );

			ushort optionFlags = 0;

			if ( manager.Workbook.WindowOptions.Minimized )
				optionFlags |= 0x0002;

			if ( ( manager.Workbook.WindowOptions.ScrollBars & ScrollBars.Horizontal ) != 0 )
				optionFlags |= 0x0008;

			if ( ( manager.Workbook.WindowOptions.ScrollBars & ScrollBars.Vertical ) != 0 )
				optionFlags |= 0x0010;

			if ( manager.Workbook.WindowOptions.TabBarVisible )
				optionFlags |= 0x0020;

			manager.CurrentRecordStream.Write( (ushort)optionFlags );

			// MD 9/27/08
			// The selected worksheet will now resolve itself
			//manager.CurrentRecordStream.Write( (ushort)manager.Workbook.WindowOptions.SelectedWorksheetResolved.Index );
			manager.CurrentRecordStream.Write( (ushort)manager.Workbook.WindowOptions.SelectedWorksheet.Index );

			manager.CurrentRecordStream.Write( (ushort)manager.Workbook.WindowOptions.FirstVisibleTabIndex );
			manager.CurrentRecordStream.Write( (ushort)1 ); // selected worksheets
			manager.CurrentRecordStream.Write( (ushort)manager.Workbook.WindowOptions.TabBarWidth );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.WINDOW1; }
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