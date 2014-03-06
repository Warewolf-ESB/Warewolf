using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd949063(v=office.12).aspx
	internal class XFRecord : Biff8RecordBase
	{
		// MD 1/1/12 - 12.1 - Cell Format Updates
		// Rewrote the save and load methods to function more closely with the way Excel saves and loads XF records.
		#region Old Code

		//public override void Load( BIFF8WorkbookSerializationManager manager )
		//{
		//    WorksheetCellFormatData format = (WorksheetCellFormatData)manager.Workbook.CreateNewWorksheetCellFormat();

		//    ushort fontIndex = manager.CurrentRecordStream.ReadUInt16();

		//    if ( fontIndex < manager.Fonts.Count )
		//        format.Font.SetFontFormatting( manager.Fonts[ fontIndex ] );
		//    else
		//        Utilities.DebugFail("This should never happen");

		//    ushort formatIndex = manager.CurrentRecordStream.ReadUInt16();

		//    // MD 10/19/07
		//    // Found while fixing BR27421
		//    // GetFormatString wasn't needed anymore, get the format diretcly from the Formats collection of the workbook.
		//    //format.FormatString = manager.GetFormatString( formatIndex );
		//    format.FormatString = manager.Workbook.Formats[ formatIndex ];

		//    ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();
		//    format.Locked =			( optionFlags & 0x0001 ) == 0x0001 ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
		//    //bool hidden =			( optionFlags & 0x0002 ) == 0x0002;
		//    format.IsStyle =		(optionFlags & 0x0004) == 0x0004;
		//    //bool f123Prefix =		( optionFlags & 0x0008 ) == 0x0008;
		//    int parentIndex =		( optionFlags & 0xFFF0 ) >> 4;
		//    Debug.Assert(format.IsStyle ^ parentIndex != 0x0FFF);

		//    byte optionFlags2 = (byte)manager.CurrentRecordStream.ReadByte();
		//    format.Alignment = (HorizontalCellAlignment)( optionFlags2 & 0x07 );
		//    format.WrapText = ( optionFlags2 & 0x08 ) == 0x08 ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
		//    format.VerticalAlignment = (VerticalCellAlignment)( ( optionFlags2 & 0x70 ) >> 4 );

		//    format.Rotation = manager.CurrentRecordStream.ReadByte();

		//    optionFlags2 = (byte)manager.CurrentRecordStream.ReadByte();
		//    format.Indent =			( optionFlags2 & 0x0F );
		//    format.ShrinkToFit =	( optionFlags2 & 0x10 ) == 0x10 ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
		//    //bool mergeCells =		( optionFlags2 & 0x20 ) == 0x20;
		//    //int readingOrder =	( optionFlags2 & 0xC0 ) >> 6;

		//    optionFlags2 = (byte)manager.CurrentRecordStream.ReadByte();

		//    // MD 12/31/11 - 12.1 - Table Support
		//    // Set this after loading all other properties (which will modify the style options)
		//    //format.FormatOptions = (StyleCellFormatOptions)( ( optionFlags2 & 0xFC ) >> 2 );
		//    StyleCellFormatOptions formatOptions = (StyleCellFormatOptions)((optionFlags2 & 0xFC) >> 2);

		//    optionFlags = manager.CurrentRecordStream.ReadUInt16();
		//    format.LeftBorderStyle =	(CellBorderLineStyle)( ( optionFlags & 0x000F ) );
		//    format.RightBorderStyle =	(CellBorderLineStyle)( ( optionFlags & 0x00F0 ) >> 4 );
		//    format.TopBorderStyle =		(CellBorderLineStyle)( ( optionFlags & 0x0F00 ) >> 8 );
		//    format.BottomBorderStyle =	(CellBorderLineStyle)( ( optionFlags & 0xF000 ) >> 12 );

		//    optionFlags = manager.CurrentRecordStream.ReadUInt16();
		//    format.LeftBorderColorIndex =	( optionFlags & 0x007F );
		//    format.RightBorderColorIndex =	( optionFlags & 0x3F80 ) >> 7;

		//    // MD 10/26/11 - TFS91546
		//    // MD 12/22/11 - 12.1 - Table Support
		//    // The diagonal border bits are now shifted by one bit to make room for the bit indicating that the diagonal borders are set.
		//    //format.DiagonalBorders = (DiagonalBorders)((optionFlags & 0xC000) >> 14);
		//    format.DiagonalBorders = DiagonalBorders.None | (DiagonalBorders)((optionFlags & 0xC000) >> 13);

		//    uint optionFlags3 = manager.CurrentRecordStream.ReadUInt32();
		//    format.TopBorderColorIndex =		(int)( optionFlags3 & 0x0000007F );
		//    format.BottomBorderColorIndex =		(int)( optionFlags3 & 0x00003F80 ) >> 7;

		//    // MD 10/26/11 - TFS91546
		//    format.DiagonalBorderColorIndex =	(int)( optionFlags3 & 0x001FC000 ) >> 14;
		//    format.DiagonalBorderStyle = (CellBorderLineStyle)( ( optionFlags3 & 0x01E00000 ) >> 21 );

		//    format.FillPattern = (FillPatternStyle)( ( optionFlags3 & 0xFC000000 ) >> 26 );

		//    optionFlags = manager.CurrentRecordStream.ReadUInt16();
		//    format.FillPatternForegroundColorIndex = ( optionFlags & 0x007F );
		//    format.FillPatternBackgroundColorIndex = ( optionFlags & 0x3F80 ) >> 7;

		//    if ( manager.Formats.Count == 0 )
		//    {
		//        Debug.Assert(format.IsStyle);
		//        manager.Workbook.CellFormats.DefaultElement = format;
		//    }

		//    // MD 12/31/11 - 12.1 - Table Support
		//    format.SetStyleOptionsInteral(formatOptions);

		//    manager.Formats.Add( format );
		//}

		//public override void Save( BIFF8WorkbookSerializationManager manager )
		//{
		//    WorksheetCellFormatData format = (WorksheetCellFormatData)manager.ContextStack[ typeof( WorksheetCellFormatData ) ];

		//    if ( format == null )
		//    {
		//        Utilities.DebugFail("There was no format in the context stack.");
		//        return;
		//    }

		//    // MD 11/11/11 - TFS85193
		//    //int fontIndex = format.FontInternal.Element.IndexInFontCollection;
		//    int fontIndex = format.FontInternal.Element.GetIndexInFontCollection(FontResolverType.Normal);

		//    if ( fontIndex < 0 )
		//    {
		//        Utilities.DebugFail("Unknown font index");
		//        fontIndex = 0;
		//    }

		//    manager.CurrentRecordStream.Write( (ushort)fontIndex );

		//    int numberFormatIndex = format.FormatStringIndex;

		//    if ( numberFormatIndex < 0 )
		//        numberFormatIndex = 0;

		//    manager.CurrentRecordStream.Write( (ushort)numberFormatIndex );

		//    ushort optionFlags = 0;

		//    if ( format.Locked == ExcelDefaultableBoolean.True )
		//        optionFlags |= 0x0001;

		//    if (format.IsStyle)
		//    {
		//        optionFlags |= 0x0004;
		//        optionFlags |= 0xFFF0; // Parent style
		//    }

		//    manager.CurrentRecordStream.Write( optionFlags );

		//    byte optionFlags2 = 0;

		//    optionFlags2 |= (byte)format.Alignment;

		//    if ( format.WrapText == ExcelDefaultableBoolean.True )
		//        optionFlags2 |= 0x08;

		//    optionFlags2 |= (byte)( (byte)format.VerticalAlignment << 4 );

		//    manager.CurrentRecordStream.Write( optionFlags2 );

		//    int rotation = format.Rotation % 256;

		//    if ( rotation < 0 )
		//        rotation += 256;

		//    manager.CurrentRecordStream.Write( (byte)rotation );

		//    optionFlags2 = 0;

		//    optionFlags2 |= (byte)format.Indent;

		//    if ( format.ShrinkToFit == ExcelDefaultableBoolean.True )
		//        optionFlags2 |= 0x10;

		//    manager.CurrentRecordStream.Write( optionFlags2 );

		//    optionFlags2 = 0;
		//    optionFlags2 |= (byte)( (byte)format.FormatOptions << 2 );

		//    manager.CurrentRecordStream.Write( optionFlags2 );

		//    optionFlags = 0;

		//    optionFlags |= (ushort)format.LeftBorderStyle;
		//    optionFlags |= (ushort)( (ushort)format.RightBorderStyle << 4 );
		//    optionFlags |= (ushort)( (ushort)format.TopBorderStyle << 8 );
		//    optionFlags |= (ushort)( (ushort)format.BottomBorderStyle << 12 );

		//    manager.CurrentRecordStream.Write( optionFlags );

		//    optionFlags = 0;

		//    optionFlags |= (ushort)format.LeftBorderColorIndex;
		//    optionFlags |= (ushort)( (int)format.RightBorderColorIndex << 7 );

		//    // MD 10/26/11 - TFS91546
		//    // The diagonal border bits are now shifted by one bit to make room for the bit indicating that the diagonal borders are set.
		//    // Also, we need to remove that bit before or-ing into the options flags.
		//    //optionFlags |= (ushort)((int)format.DiagonalBorders << 14);
		//    optionFlags |= (ushort)((int)(format.DiagonalBorders & ~DiagonalBorders.None) << 13);

		//    manager.CurrentRecordStream.Write( optionFlags );

		//    uint optionFlags3 = 0;

		//    optionFlags3 |= (uint)format.TopBorderColorIndex;
		//    optionFlags3 |= (uint)( format.BottomBorderColorIndex << 7 );

		//    // MD 10/26/11 - TFS91546
		//    optionFlags3 |= (uint)(format.DiagonalBorderColorIndex << 14);
		//    optionFlags3 |= (uint)((int)format.DiagonalBorderStyle << 21);

		//    optionFlags3 |= (uint)( (int)format.FillPattern << 26 );

		//    manager.CurrentRecordStream.Write( optionFlags3 );

		//    optionFlags = 0;

		//    optionFlags |= (ushort)format.FillPatternForegroundColorIndex;
		//    optionFlags |= (ushort)( format.FillPatternBackgroundColorIndex << 7 );

		//    manager.CurrentRecordStream.Write( optionFlags );
		//}

		#endregion // Old Code

		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			ushort ifnt = manager.CurrentRecordStream.ReadUInt16();
			ushort ifmt = manager.CurrentRecordStream.ReadUInt16();

			ushort temp16 = manager.CurrentRecordStream.ReadUInt16();
			bool fLocked = Utilities.TestBit(temp16, 0);
			bool fHidden = Utilities.TestBit(temp16, 1);
			bool fStyle = Utilities.TestBit(temp16, 2);
			bool f123Prefix = Utilities.TestBit(temp16, 3);
			int ixfParent = Utilities.GetBits(temp16, 4, 15);
			Debug.Assert(fStyle == false || f123Prefix == false, "If fStyle is True, f123Prefix must be False.");
			Debug.Assert(fStyle ^ (ixfParent != 0x0FFF), "Either fStyle should be True or ixfParent should be 0x0FFF.");

			byte temp8 = (byte)manager.CurrentRecordStream.ReadByte();
			HorizontalCellAlignment alc = (HorizontalCellAlignment)Utilities.GetBits(temp8, 0, 2);
			bool fWrap = Utilities.TestBit(temp8, 3);
			VerticalCellAlignment alcV = (VerticalCellAlignment)Utilities.GetBits(temp8, 4, 6);
			// MD 3/21/12 - TFS104630
			// We need to round-trip the AddIndent value.
			bool fJustLastLine = Utilities.TestBit(temp8, 7);

			int trot = manager.CurrentRecordStream.ReadByte();
			Debug.Assert(0 <= trot && trot < 256, "The trot value of the XF record is out of range.");

			temp16 = manager.CurrentRecordStream.ReadUInt16();
			int cIndent = Utilities.GetBits(temp16, 0, 3);
			bool fShrinkToFit = Utilities.TestBit(temp16, 4);
			bool fMergeCell = Utilities.TestBit(temp16, 5);
			int iReadOrder = Utilities.GetBits(temp16, 6, 7);
			bool fAtrNum = Utilities.TestBit(temp16, 10);
			bool fAtrFnt = Utilities.TestBit(temp16, 11);
			bool fAtrAlc = Utilities.TestBit(temp16, 12);
			bool fAtrBdr = Utilities.TestBit(temp16, 13);
			bool fAtrPat = Utilities.TestBit(temp16, 14);
			bool fAtrProt = Utilities.TestBit(temp16, 15);

			temp16 = manager.CurrentRecordStream.ReadUInt16();
			CellBorderLineStyle dgLeft = (CellBorderLineStyle)Utilities.GetBits(temp16, 0, 3);
			CellBorderLineStyle dgRight = (CellBorderLineStyle)Utilities.GetBits(temp16, 4, 7);
			CellBorderLineStyle dgTop = (CellBorderLineStyle)Utilities.GetBits(temp16, 8, 11);
			CellBorderLineStyle dgBottom = (CellBorderLineStyle)Utilities.GetBits(temp16, 12, 15);

			temp16 = manager.CurrentRecordStream.ReadUInt16();
			int icvLeft = Utilities.GetBits(temp16, 0, 6);
			int icvRight = Utilities.GetBits(temp16, 7, 13);
			int grbitDiag = Utilities.GetBits(temp16, 14, 15);

			uint temp32 = manager.CurrentRecordStream.ReadUInt32();
			int icvTop = Utilities.GetBits(temp32, 0, 6);
			int icvBottom = Utilities.GetBits(temp32, 7, 13);
			int icvDiag = Utilities.GetBits(temp32, 14, 20);
			CellBorderLineStyle dgDiag = (CellBorderLineStyle)Utilities.GetBits(temp32, 21, 24);
			bool fHasXFExt = Utilities.TestBit(temp32, 25);
			FillPatternStyle fls = (FillPatternStyle)Utilities.GetBits(temp32, 26, 31);

			temp16 = manager.CurrentRecordStream.ReadUInt16();
			int icvFore = Utilities.GetBits(temp16, 0, 6);
			int icvBack = Utilities.GetBits(temp16, 7, 13);
			bool fSxButton = Utilities.TestBit(temp16, 14);
			Debug.Assert(fStyle == false || fSxButton == false, "If fStyle is True, fSxButton must be False.");

			WorksheetCellFormatData format = manager.Workbook.CreateNewWorksheetCellFormatInternal(
				fStyle ? WorksheetCellFormatType.StyleFormat : WorksheetCellFormatType.CellFormat);

			WorksheetCellFormatData parentStyleFormat = null;
			if (fStyle)
			{
				// The bits indicating which format options are set are True for cell formats and False for style formats, so reverse them for style formats.
				fAtrNum ^= true;
				fAtrFnt ^= true;
				fAtrAlc ^= true;
				fAtrBdr ^= true;
				fAtrPat ^= true;
				fAtrProt ^= true;
			}
			else if (0 <= ixfParent && ixfParent < manager.Formats.Count)
			{
				parentStyleFormat = manager.Formats[ixfParent];

				List<WorksheetCellFormatData> childFormats;
				if (manager.ParentStylesToChildren.TryGetValue(ixfParent, out childFormats) == false)
				{
					childFormats = new List<WorksheetCellFormatData>();
					manager.ParentStylesToChildren.Add(ixfParent, childFormats);
				}

				childFormats.Add(format);
			}
			else
			{
				Utilities.DebugFail("The ixfParent value of the Xf record is out of range.");
			}

			if (0 <= ifnt && ifnt < manager.Fonts.Count)
				format.Font.SetFontFormatting(manager.Fonts[ifnt]);
			else
				Utilities.DebugFail("The ifnt value of the XF record is out of range.");

			Workbook workbook = manager.Workbook;

			// MD 3/21/12 - TFS104630
			// We need to round-trip the AddIndent value.
			format.AddIndent = fJustLastLine;

			format.Alignment = alc;

			if (icvBottom != 0)
			{
				format.BottomBorderColorInfo = new WorkbookColorInfo(workbook, icvBottom);
				format.BottomBorderStyle = dgBottom;
			}

			if (icvDiag != 0)
			{
				format.DiagonalBorderColorInfo = new WorkbookColorInfo(workbook, icvDiag);
				format.DiagonalBorders = DiagonalBorders.None | (DiagonalBorders)(grbitDiag << 1);
				format.DiagonalBorderStyle = dgDiag;
			}
			else if (grbitDiag != 0)
			{
				format.DiagonalBorders = DiagonalBorders.None | (DiagonalBorders)(grbitDiag << 1);
			}

			format.Fill = new CellFillPattern(icvBack, icvFore, fls, format);
			format.FormatStringIndex = ifmt;
			format.Indent = cIndent;

			if (icvLeft != 0)
			{
				format.LeftBorderColorInfo = new WorkbookColorInfo(workbook, icvLeft);
				format.LeftBorderStyle = dgLeft;
			}

			format.Locked = Utilities.ToDefaultableBoolean(fLocked);

			if (icvRight != 0)
			{
				format.RightBorderColorInfo = new WorkbookColorInfo(workbook, icvRight);
				format.RightBorderStyle = dgRight;
			}

			format.Rotation = trot;
			format.ShrinkToFit = Utilities.ToDefaultableBoolean(fShrinkToFit);

			if (icvTop != 0)
			{
				format.TopBorderColorInfo = new WorkbookColorInfo(workbook, icvTop);
				format.TopBorderStyle = dgTop;
			}

			format.VerticalAlignment = alcV;
			format.WrapText = Utilities.ToDefaultableBoolean(fWrap);

			WorksheetCellFormatOptions formatOptions = WorksheetCellFormatOptions.None;

			if (fAtrNum)
				formatOptions |= WorksheetCellFormatOptions.ApplyNumberFormatting;

			if (fAtrFnt)
				formatOptions |= WorksheetCellFormatOptions.ApplyFontFormatting;

			if (fAtrAlc)
				formatOptions |= WorksheetCellFormatOptions.ApplyAlignmentFormatting;

			if (fAtrBdr)
				formatOptions |= WorksheetCellFormatOptions.ApplyBorderFormatting;

			if (fAtrPat)
				formatOptions |= WorksheetCellFormatOptions.ApplyFillFormatting;

			if (fAtrProt)
				formatOptions |= WorksheetCellFormatOptions.ApplyProtectionFormatting;

			// For cell formats, don't set the FormatOptions property directly because we don't want to reset the properties
			// If any property values differ from the parent style, we will turn on those format options when the style is loaded
			// by the STYLE record.
			if (format.Type == WorksheetCellFormatType.StyleFormat)
				format.FormatOptions = formatOptions;
			else
				format.SetFormatOptionsInternal(formatOptions);

			int xfid = manager.Formats.Count;
			Debug.Assert(xfid >= 0 || format.Type == WorksheetCellFormatType.StyleFormat, "The first XF record should be a style XF.");

			manager.Formats.Add(format);
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			XFContext xfContext = (XFContext)manager.ContextStack[typeof(XFContext)];
			if (xfContext == null)
			{
				Utilities.DebugFail("There was no XFContext in the context stack.");
				return;
			}
			WorksheetCellFormatData format = xfContext.FormatData;
			Workbook workbook = manager.Workbook;

			short ifnt = format.FontInternal.SaveIndex;
			if (ifnt < 0)
			{
				Utilities.DebugFail("The ifnt value for the XF record is invalid");
				ifnt = 0;
			}

			manager.CurrentRecordStream.Write((ushort)ifnt);

			int ifmt = format.FormatStringIndexResolved;
			if (ifmt < 0)
			{
				Utilities.DebugFail("The ifmt value for the XF record is invalid");
				ifmt = 0;
			}

			manager.CurrentRecordStream.Write((ushort)ifmt);

			int ixfParent;
			if (format.Type != WorksheetCellFormatType.StyleFormat)
			{
				ixfParent = manager.GetStyleFormatIndex(format.Style);

				if (ixfParent < 0)
				{
					Utilities.DebugFail("The ixfParent value for the XF record is invalid");
					ixfParent = 0;
				}
			}
			else
			{
				ixfParent = 0x0FFF;
			}

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, format.LockedResolved, 0);
			Utilities.SetBit(ref temp16, format.Type == WorksheetCellFormatType.StyleFormat, 2);
			Utilities.AddBits(ref temp16, ixfParent, 4, 15);
			manager.CurrentRecordStream.Write(temp16);

			byte temp8 = 0;
			Utilities.AddBits(ref temp8, (int)format.AlignmentResolved, 0, 2);
			Utilities.SetBit(ref temp8, format.WrapTextResolved, 3);
			Utilities.AddBits(ref temp8, (int)format.VerticalAlignmentResolved, 4, 6);
			// MD 3/21/12 - TFS104630
			// We need to round-trip the AddIndent value.
			Utilities.SetBit(ref temp8, format.AddIndent, 7);
			manager.CurrentRecordStream.Write(temp8);

			int rotation = format.RotationResolved % 256;

			if (rotation < 0)
				rotation += 256;

			manager.CurrentRecordStream.Write((byte)rotation);

			// The bit indicating which format options are set are True for cell formats and False for style format.
			bool formatOptionsPresetValue = (format.Type != WorksheetCellFormatType.StyleFormat);
			bool fAtrNum = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting) == formatOptionsPresetValue;
			bool fAtrFnt = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyFontFormatting) == formatOptionsPresetValue;
			bool fAtrAlc = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting) == formatOptionsPresetValue;
			bool fAtrBdr = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting) == formatOptionsPresetValue;
			bool fAtrPat = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyFillFormatting) == formatOptionsPresetValue;
			bool fAtrProt = Utilities.TestFlag(format.FormatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting) == formatOptionsPresetValue;

			temp16 = 0;
			Utilities.AddBits(ref temp16, Math.Min(WorksheetCellFormatData.MaxIndent2003, format.IndentResolved), 0, 3);
			Utilities.SetBit(ref temp16, format.ShrinkToFitResolved, 4);
			Utilities.SetBit(ref temp16, fAtrNum, 10);
			Utilities.SetBit(ref temp16, fAtrFnt, 11);
			Utilities.SetBit(ref temp16, fAtrAlc, 12);
			Utilities.SetBit(ref temp16, fAtrBdr, 13);
			Utilities.SetBit(ref temp16, fAtrPat, 14);
			Utilities.SetBit(ref temp16, fAtrProt, 15);
			manager.CurrentRecordStream.Write(temp16);

			CellBorderLineStyle dgLeft;
			CellBorderLineStyle dgRight;
			CellBorderLineStyle dgTop;
			CellBorderLineStyle dgBottom;
			CellBorderLineStyle dgDiag;
			int icvLeft;
			int icvRight;
			int icvTop;
			int icvBottom;
			int icvDiag;

			if (format.BottomBorderStyle != CellBorderLineStyle.Default ||
				format.BottomBorderColorInfo != null)
			{
				dgBottom = format.BottomBorderStyleResolved;
				icvBottom = format.BottomBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}
			else
			{
				dgBottom = CellBorderLineStyle.None;
				icvBottom = 0;
			}

			if (format.DiagonalBorderStyle != CellBorderLineStyle.Default ||
				format.DiagonalBorders != DiagonalBorders.Default ||
				format.DiagonalBorderColorInfo != null)
			{
				dgDiag = format.DiagonalBorderStyleResolved;
				icvDiag = format.DiagonalBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}
			else
			{
				dgDiag = CellBorderLineStyle.None;
				icvDiag = 0;
			}

			if (format.LeftBorderStyle != CellBorderLineStyle.Default ||
				format.LeftBorderColorInfo != null)
			{
				dgLeft = format.LeftBorderStyleResolved;
				icvLeft = format.LeftBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}
			else
			{
				dgLeft = CellBorderLineStyle.None;
				icvLeft = 0;
			}

			if (format.RightBorderStyle != CellBorderLineStyle.Default ||
				format.RightBorderColorInfo != null)
			{
				dgRight = format.RightBorderStyleResolved;
				icvRight = format.RightBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}
			else
			{
				dgRight = CellBorderLineStyle.None;
				icvRight = 0;
			}

			if (format.TopBorderStyle != CellBorderLineStyle.Default ||
				format.TopBorderColorInfo != null)
			{
				dgTop = format.TopBorderStyleResolved;
				icvTop = format.TopBorderColorInfoResolved.GetIndex(workbook, ColorableItem.CellBorder);
			}
			else
			{
				dgTop = CellBorderLineStyle.None;
				icvTop = 0;
			}

			temp16 = 0;
			Utilities.AddBits(ref temp16, (int)dgLeft, 0, 3);
			Utilities.AddBits(ref temp16, (int)dgRight, 4, 7);
			Utilities.AddBits(ref temp16, (int)dgTop, 8, 11);
			Utilities.AddBits(ref temp16, (int)dgBottom, 12, 15);
			manager.CurrentRecordStream.Write(temp16);

			int grbitDiag = (int)format.DiagonalBordersResolved >> 1;

			temp16 = 0;
			Utilities.AddBits(ref temp16, icvLeft, 0, 6);
			Utilities.AddBits(ref temp16, icvRight, 7, 13);
			Utilities.AddBits(ref temp16, grbitDiag, 14, 15);
			manager.CurrentRecordStream.Write(temp16);

			CellFill fillResolved = format.FillResolved;
			FillPatternStyle fillPattern = format.GetFillPattern(fillResolved);
			bool fHasXFExt = xfContext.ExtProps.Count != 0;

			uint temp32 = 0;
			Utilities.AddBits(ref temp32, icvTop, 0, 6);
			Utilities.AddBits(ref temp32, icvBottom, 7, 13);
			Utilities.AddBits(ref temp32, icvDiag, 14, 20);
			Utilities.AddBits(ref temp32, (int)dgDiag, 21, 24);
			Utilities.SetBit(ref temp32, fHasXFExt, 25);
			Utilities.AddBits(ref temp32, (int)fillPattern, 26, 31);
			manager.CurrentRecordStream.Write(temp32);

			int icvFore = format.GetFileFormatFillPatternColor(fillResolved, false, true).GetIndex(workbook, ColorableItem.CellFill);
			int icvBack = format.GetFileFormatFillPatternColor(fillResolved, true, true).GetIndex(workbook, ColorableItem.CellFill);

			temp16 = 0;
			Utilities.AddBits(ref temp16, icvFore, 0, 6);
			Utilities.AddBits(ref temp16, icvBack, 7, 13);
			manager.CurrentRecordStream.Write(temp16);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.XF; }
		}

		// MD 1/10/12 - 12.1 - Cell Format Updates
		public sealed class XFContext
		{
			public readonly WorksheetCellFormatData FormatData;
			public readonly ushort XfId;
			public readonly List<ExtProp> ExtProps;

			public XFContext(WorksheetCellFormatData formatData, ushort xfid, List<ExtProp> extProps)
			{
				this.FormatData = formatData;
				this.XfId = xfid;
				this.ExtProps = extProps;
			}
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