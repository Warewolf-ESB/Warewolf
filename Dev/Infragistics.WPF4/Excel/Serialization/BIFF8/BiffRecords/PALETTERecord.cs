using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class PALETTERecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 1/16/12 - 12.1 - Cell Format Updates
			//manager.Workbook.Palette.PaletteMode = WorkbookPaletteMode.CustomPalette;
			//
			//WorkbookColorCollection palette = manager.Workbook.Palette;
			WorkbookColorPalette palette = manager.Workbook.Palette;

			int count = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(count == 0 || count == WorkbookColorPalette.UserPaletteSize, "This is unexpected");
			count = Math.Min(count, WorkbookColorPalette.UserPaletteSize);

			for ( int i = 0; i < count; i++ )
			{
				int r = manager.CurrentRecordStream.ReadByte();
				int g = manager.CurrentRecordStream.ReadByte();
				int b = manager.CurrentRecordStream.ReadByte();
				manager.CurrentRecordStream.ReadByte();

				// MD 1/16/12 - 12.1 - Cell Format Updates
                //palette.AddInternal(i, Color.FromArgb(255, (byte)r, (byte)g, (byte)b));
				palette[i] = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 1/16/12 - 12.1 - Cell Format Updates
			//WorkbookColorCollection palette = manager.Workbook.Palette;
			//
			//manager.CurrentRecordStream.Write( (ushort)palette.Count );
			WorkbookColorPalette palette = manager.Workbook.Palette;
			manager.CurrentRecordStream.Write((ushort)WorkbookColorPalette.UserPaletteSize);

			// Write each color
			// MD 1/16/12 - 12.1 - Cell Format Updates
			//foreach ( Color color in palette )
			//{
			for (int i = 0; i < WorkbookColorPalette.UserPaletteSize; i++)
			{
				Color color = palette[i];

				manager.CurrentRecordStream.Write( (byte)color.R );
				manager.CurrentRecordStream.Write( (byte)color.G );
				manager.CurrentRecordStream.Write( (byte)color.B );
				manager.CurrentRecordStream.Write( (byte)0 );
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.PALETTE; }
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