using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd906117(v=office.12).aspx
	internal class FONTRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			WorkbookFontData font = (WorkbookFontData)manager.Workbook.CreateNewWorkbookFont();

			// MD 12/13/11 - TFS97715
			// According to the documentation, the dyHeight field can be between 20 and 8191, or 0.
			// In practice, when a value of 0 is written out, the font height in the workbook is 1 point, or 20 twips.
			//font.Height = manager.CurrentRecordStream.ReadUInt16();
			ushort dyHeight = manager.CurrentRecordStream.ReadUInt16();
			if (dyHeight < 20)
			{
				Debug.Assert(dyHeight == 0, "We only expect the font height to be 0 in this case.");
				dyHeight = 20;
			}
			font.Height = dyHeight;

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			font.Italic =		( optionFlags & 0x0002 ) == 0x0002 ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
			font.Strikeout =	( optionFlags & 0x0008 ) == 0x0008 ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;

			// MD 1/17/12 - 12.1 - Cell Format Updates
			//font.ColorIndex = manager.CurrentRecordStream.ReadUInt16();
			ushort icvFont = manager.CurrentRecordStream.ReadUInt16();
			font.ColorInfo = new WorkbookColorInfo(manager.Workbook, icvFont);

			int fontWeight = manager.CurrentRecordStream.ReadUInt16();
			font.Bold = fontWeight < 0x2BC ? ExcelDefaultableBoolean.False : ExcelDefaultableBoolean.True;

			font.SuperscriptSubscriptStyle = (FontSuperscriptSubscriptStyle)manager.CurrentRecordStream.ReadUInt16();
			font.UnderlineStyle = (FontUnderlineStyle)manager.CurrentRecordStream.ReadByte();

			
			manager.CurrentRecordStream.ReadByte(); // fontFamily
			manager.CurrentRecordStream.ReadByte(); // characterSet

			// Reserved; must be 0
			manager.CurrentRecordStream.ReadByte();

			font.Name = manager.CurrentRecordStream.ReadFormattedString( LengthType.EightBit ).UnformattedString;

			// Font indexes skip 4, so insert a null in the list so index translations dont need to be done
			if ( manager.Fonts.Count == 4 )
				manager.Fonts.Add( null );

			if (manager.Fonts.Count == 0)
				manager.Workbook.Fonts.DefaultElement = font;

			manager.Fonts.Add( font );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			WorkbookFontData font = (WorkbookFontData)manager.ContextStack[ typeof( WorkbookFontData ) ];

			if ( font == null )
			{
                Utilities.DebugFail("There was no font in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)font.Height );

			ushort optionFlags = 0;

			if ( font.Italic == ExcelDefaultableBoolean.True )
				optionFlags |= 0x0002;
			if ( font.Strikeout == ExcelDefaultableBoolean.True )
				optionFlags |= 0x0008;

			manager.CurrentRecordStream.Write( (ushort)optionFlags );

			// MD 1/17/12 - 12.1 - Cell Format Updates
			//manager.CurrentRecordStream.Write( (ushort)font.ColorIndex );
			manager.CurrentRecordStream.Write((ushort)font.ColorInfo.GetIndex(manager.Workbook, ColorableItem.CellFont));

			ushort fontWieght = (ushort)( font.Bold == ExcelDefaultableBoolean.True ? 0x02BC : 0x0190 );
			manager.CurrentRecordStream.Write( (ushort)fontWieght );
			manager.CurrentRecordStream.Write( (ushort)font.SuperscriptSubscriptStyle );
			manager.CurrentRecordStream.Write( (byte)font.UnderlineStyle );
			manager.CurrentRecordStream.Write( (byte)0 ); // font family
			manager.CurrentRecordStream.Write( (byte)0 ); // character set
			manager.CurrentRecordStream.Write( (byte)0 ); // Reserved, must be 0
			manager.CurrentRecordStream.Write( font.Name, LengthType.EightBit, false );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.FONT; }
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