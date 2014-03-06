using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8
{
	// http://msdn.microsoft.com/en-us/library/dd953723(v=office.12).aspx
	internal abstract class XFProp
	{
		public abstract void ApplyTo(WorksheetCellFormatData format);
		public abstract XFPropType Type { get; }
	}

	#region XFPropBool class

	internal class XFPropBool : XFProp
	{
		private bool _value;
		private XFPropType _type;

		public XFPropBool(XFPropType type, bool value)
		{
			_type = type;
			_value = value;
		}

		public XFPropBool(XFPropType type, ExcelDefaultableBoolean value)
			: this(type, value == ExcelDefaultableBoolean.True)
		{
			Debug.Assert(value != ExcelDefaultableBoolean.Default, "We should not be using the XFPropBool class for a default value.");
		}

		public XFPropBool(XFPropType type, byte value)
		{
			Debug.Assert(value == 0 || value == 1, "The Boolean value is incorrect.");
			_type = type;
			_value = value == 0 ? false : true;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			ExcelDefaultableBoolean defaultableValue = Utilities.ToDefaultableBoolean(_value);

			switch (_type)
			{
				case XFPropType.CellMerged:
					break;

				case XFPropType.DiagonalDownBorder:
					if (_value)
						format.DiagonalBorders |= DiagonalBorders.DiagonalDown;
					else
						format.DiagonalBorders &= ~DiagonalBorders.DiagonalDown;
					break;

				case XFPropType.DiagonalUpBorder:
					if (_value)
						format.DiagonalBorders |= DiagonalBorders.DiagonalUp;
					else
						format.DiagonalBorders &= ~DiagonalBorders.DiagonalUp;
					break;

				case XFPropType.FontCondensed:
					break;

				case XFPropType.FontExtended:
					break;

				case XFPropType.FontItalic:
					format.Font.Italic = defaultableValue;
					break;

				case XFPropType.FontOutline:
					break;

				case XFPropType.FontShadow:
					break;

				case XFPropType.FontStrikethrough:
					format.Font.Strikeout = defaultableValue;
					break;

				case XFPropType.Hidden:
					break;

				case XFPropType.Locked:
					format.Locked = defaultableValue;
					break;

				case XFPropType.ShrinkToFit:
					format.ShrinkToFit = defaultableValue;
					break;

				case XFPropType.WrappedText:
					format.WrapText = defaultableValue;
					break;

				default:
					Utilities.DebugFail("Unknown XFPropType: " + _type);
					break;
			}
		}

		public override XFPropType Type
		{
			get { return _type; }
		}

		public bool Value
		{
			get { return _value; }
		}

		public byte ValueByte
		{
			get { return (byte)(_value ? 1 : 0); }
		}
	}

	#endregion // XFPropBool class

	#region XFPropBorder class

	// http://msdn.microsoft.com/en-us/library/dd945965(v=office.12).aspx
	internal class XFPropBorder : XFProp
	{
		private WorkbookColorInfo _borderColor;
		private CellBorderLineStyle _borderStyle;
		private XFPropType _type;

		public XFPropBorder(XFPropType type, WorkbookColorInfo borderColor, CellBorderLineStyle borderStyle)
		{
			Debug.Assert(borderColor != null && borderStyle != CellBorderLineStyle.Default, "There is a problem with these value.");

			_borderColor = borderColor;
			_borderStyle = borderStyle;
			_type = type;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			switch (_type)
			{
				case XFPropType.BottomBorder:
					format.BottomBorderColorInfo = _borderColor;
					format.BottomBorderStyle = _borderStyle;
					break;

				case XFPropType.DiagonalBorder:
					format.DiagonalBorderColorInfo = _borderColor;
					format.DiagonalBorderStyle = _borderStyle;
					break;

				case XFPropType.HorizontalBorder:
					break;

				case XFPropType.LeftBorder:
					format.LeftBorderColorInfo = _borderColor;
					format.LeftBorderStyle = _borderStyle;
					break;

				case XFPropType.RightBorder:
					format.RightBorderColorInfo = _borderColor;
					format.RightBorderStyle = _borderStyle;
					break;

				case XFPropType.TopBorder:
					format.TopBorderColorInfo = _borderColor;
					format.TopBorderStyle = _borderStyle;
					break;

				case XFPropType.VerticalBorder:
					break;

				default:
					Utilities.DebugFail("Unknown XFPropType: " + _type);
					break;
			}
		}

		public override XFPropType Type
		{
			get { return _type; }
		}

		public WorkbookColorInfo BorderColor
		{
			get { return _borderColor; }
		}

		public CellBorderLineStyle BorderStyle
		{
			get { return _borderStyle; }
		}
	}

	#endregion // XFPropBorder class

	#region XFPropByte class

	internal class XFPropByte : XFProp
	{
		private XFPropType _type;
		private byte _value;

		public XFPropByte(XFPropType type, byte value)
		{
			_type = type;
			_value = value;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{

		}

		public override XFPropType Type
		{
			get { return _type; }
		}

		public ushort Value
		{
			get { return _value; }
		}
	}

	#endregion // XFPropByte class

	#region XFPropColor class

	// http://msdn.microsoft.com/en-us/library/dd906791(v=office.12).aspx
	internal class XFPropColor : XFProp
	{
		private WorkbookColorInfo _colorInfo;
		private XFPropType _type;

		public XFPropColor(XFPropType type, WorkbookColorInfo colorInfo)
		{
			_type = type;
			_colorInfo = colorInfo;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			switch (_type)
			{
				case XFPropType.BackgroundColor:
				case XFPropType.ForegroundColor:
					Utilities.DebugFail("We should not have called ApplyTo for the fill color types.");
					break;

				case XFPropType.FontColor:
					format.Font.ColorInfo = _colorInfo;
					break;

				default:
					Utilities.DebugFail("Unknown XFPropType: " + _type);
					break;
			}
		}

		public override XFPropType Type
		{
			get { return _type; }
		}

		public WorkbookColorInfo ColorInfo
		{
			get { return _colorInfo; }
		}
	}

	#endregion // XFPropColor class

	#region XFPropFillPattern class

	// http://msdn.microsoft.com/en-us/library/dd905968(v=office.12).aspx
	internal class XFPropFillPattern : XFProp
	{
		private FillPatternStyle _fillPattern;

		public XFPropFillPattern(FillPatternStyle fillPattern)
		{
			_fillPattern = fillPattern;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			Utilities.DebugFail("We should not have called ApplyTo for the fill pattern.");
		}

		public override XFPropType Type
		{
			get { return XFPropType.PatternFill; }
		}

		public FillPatternStyle FillPattern
		{
			get { return _fillPattern; }
		}
	}

	#endregion // XFPropFillPattern class

	#region XFPropFontBold class

	// http://msdn.microsoft.com/en-us/library/dd905544(v=office.12).aspx
	internal class XFPropFontBold : XFProp
	{
		private bool _bold;

		public XFPropFontBold(ushort fontWeight)
		{
			switch (fontWeight)
			{
				case Biff8RecordStream.FontNormalWeight:
					_bold = false;
					break;

				case Biff8RecordStream.FontBoldWeight:
					_bold = true;
					break;

				default:
					Utilities.DebugFail("Unknown fontWeight value.");

					if (fontWeight < Biff8RecordStream.FontNormalWeight)
						goto case Biff8RecordStream.FontNormalWeight;

					goto case Biff8RecordStream.FontBoldWeight;
			}
		}

		public XFPropFontBold(ExcelDefaultableBoolean bold)
		{
			Debug.Assert(bold != ExcelDefaultableBoolean.Default, "We should not be using the XFPropFontBold class for a default value.");
			_bold = bold == ExcelDefaultableBoolean.True;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Font.Bold = Utilities.ToDefaultableBoolean(_bold);
		}

		public override XFPropType Type
		{
			get { return XFPropType.FontBold; }
		}

		public bool Bold
		{
			get { return _bold; }
		}

		public ushort FontWeight
		{
			get
			{
				return _bold
					? Biff8RecordStream.FontBoldWeight
					: Biff8RecordStream.FontNormalWeight;
			}
		}
	}

	#endregion // XFPropFontBold class

	#region XFPropFontHeight class

	internal class XFPropFontHeight : XFProp
	{
		private uint _fontHeight;

		public XFPropFontHeight(uint fontHeight)
		{
			Debug.Assert(20 <= fontHeight && fontHeight <= 8191, "The font height value is incorrect.");
			_fontHeight = fontHeight;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Font.Height = (int)_fontHeight;
		}

		public override XFPropType Type
		{
			get { return XFPropType.FontHeight; }
		}

		public uint FontHeight
		{
			get { return _fontHeight; }
		}
	}

	#endregion // XFPropFontHeight class

	#region XFPropFontName class

	internal class XFPropFontName : XFProp
	{
		private string _fontName;

		public XFPropFontName(string fontName)
		{
			Debug.Assert(fontName.Length <= 32, "The font name length is incorrect.");
			_fontName = fontName;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Font.Name = _fontName;
		}

		public override XFPropType Type
		{
			get { return XFPropType.FontName; }
		}

		public string FontName
		{
			get { return _fontName; }
		}
	}

	#endregion // XFPropFontName class

	#region XFPropFontSubscriptSuperscript class

	// http://msdn.microsoft.com/en-us/library/dd907891(v=office.12).aspx
	internal class XFPropFontSubscriptSuperscript : XFProp
	{
		private FontSuperscriptSubscriptStyle _style;

		public XFPropFontSubscriptSuperscript(FontSuperscriptSubscriptStyle style)
		{
			_style = style;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Font.SuperscriptSubscriptStyle = _style;
		}

		public override XFPropType Type
		{
			get { return XFPropType.FontSubscriptSuperscript; }
		}

		public FontSuperscriptSubscriptStyle Style
		{
			get { return _style; }
		}
	}

	#endregion // XFPropFontSubscriptSuperscript class

	#region XFPropFontScheme class

	// http://msdn.microsoft.com/en-us/library/dd907891(v=office.12).aspx
	internal class XFPropFontScheme : XFProp
	{
		private FontScheme _fontScheme;

		public XFPropFontScheme(FontScheme fontScheme)
		{
			_fontScheme = fontScheme;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.FontSchemeInternal = _fontScheme;
		}

		public override XFPropType Type
		{
			get { return XFPropType.FontScheme; }
		}

		public FontScheme FontScheme
		{
			get { return _fontScheme; }
		}
	}

	#endregion // XFPropFontScheme class

	#region XFPropFontUnderline class

	// http://msdn.microsoft.com/en-us/library/dd907891(v=office.12).aspx
	internal class XFPropFontUnderline : XFProp
	{
		private FontUnderlineStyle _underlineStyle;

		public XFPropFontUnderline(FontUnderlineStyle underlineStyle)
		{
			_underlineStyle = underlineStyle;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Font.UnderlineStyle = _underlineStyle;
		}

		public override XFPropType Type
		{
			get { return XFPropType.FontUnderline; }
		}

		public FontUnderlineStyle UnderlineStyle
		{
			get { return _underlineStyle; }
		}
	}

	#endregion // XFPropFontUnderline class

	#region XFPropGradientFill class

	internal class XFPropGradientFill : XFProp
	{
		private bool _isRectangular;
		private double _numDegree;
		private double _numFillToLeft;
		private double _numFillToRight;
		private double _numFillToTop;
		private double _numFillToBottom;

		public XFPropGradientFill(bool isRectangular, double numDegree,
			double numFillToLeft, double numFillToRight, double numFillToTop, double numFillToBottom)
		{
			_isRectangular = isRectangular;
			_numDegree = numDegree;
			_numFillToLeft = numFillToLeft;
			_numFillToRight = numFillToRight;
			_numFillToTop = numFillToTop;
			_numFillToBottom = numFillToBottom;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			Utilities.DebugFail("We should not have called ApplyTo for the fills.");
		}

		public override XFPropType Type
		{
			get { return XFPropType.GradientFill; }
		}

		public bool IsRectangular
		{
			get { return _isRectangular; }
		}

		public double NumDegree
		{
			get { return _numDegree; }
		}

		public double NumFillToLeft
		{
			get { return _numFillToLeft; }
		}

		public double NumFillToRight
		{
			get { return _numFillToRight; }
		}

		public double NumFillToTop
		{
			get { return _numFillToTop; }
		}

		public double NumFillToBottom
		{
			get { return _numFillToBottom; }
		}
	}

	#endregion // XFPropGradientFill class

	#region XFPropGradientStop class

	internal class XFPropGradientStop : XFProp
	{
		private WorkbookColorInfo _color;
		private double _numPosition;

		public XFPropGradientStop(WorkbookColorInfo color, double numPosition)
		{
			_color = color;
			_numPosition = numPosition;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			Utilities.DebugFail("We should not have called ApplyTo for the fills.");
		}

		public override XFPropType Type
		{
			get { return XFPropType.GradientStop; }
		}

		public WorkbookColorInfo Color
		{
			get { return _color; }
		}

		public double NumPosition
		{
			get { return _numPosition; }
		}
	}

	#endregion // XFPropGradientStop class

	#region XFPropHorizontalAlignment class

	// http://msdn.microsoft.com/en-us/library/dd908508(v=office.12).aspx
	internal class XFPropHorizontalAlignment : XFProp
	{
		private HorizontalCellAlignment _alignment;

		public XFPropHorizontalAlignment(HorizontalCellAlignment alignment)
		{
			_alignment = alignment;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Alignment = _alignment;
		}

		public override XFPropType Type
		{
			get { return XFPropType.HorizontalAlignment; }
		}

		public HorizontalCellAlignment Alignment
		{
			get { return _alignment; }
		}
	}

	#endregion // XFPropHorizontalAlignment class

	#region XFPropNumberFormat class

	// http://msdn.microsoft.com/en-us/library/dd944946(v=office.12).aspx
	internal class XFPropNumberFormat : XFProp
	{
		private ushort _numFmtId;
		private string _format;

		public XFPropNumberFormat(ushort numFmtId, string format)
		{
			_numFmtId = numFmtId;
			_format = format;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.FormatString = _format;
		}

		public override XFPropType Type
		{
			get { return XFPropType.NumberFormat; }
		}

		public ushort NumFmtId
		{
			get { return _numFmtId; }
		}

		public string Format
		{
			get { return _format; }
		}
	}

	#endregion // XFPropNumberFormat class

	#region XFPropNumberFormatId class

	// http://msdn.microsoft.com/en-us/library/dd948560(v=office.12).aspx
	internal class XFPropNumberFormatId : XFProp
	{
		private ushort _fmtId;

		public XFPropNumberFormatId(ushort fmtId)
		{
			_fmtId = fmtId;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.FormatStringIndex = _fmtId;
		}

		public override XFPropType Type
		{
			get { return XFPropType.NumberFormatId; }
		}

		public ushort FmtId
		{
			get { return _fmtId; }
		}
	}

	#endregion // XFPropNumberFormatId class

	#region XFPropTextIndentationLevel class

	internal class XFPropTextIndentationLevel : XFProp
	{
		private ushort _indent;

		public XFPropTextIndentationLevel(ushort indent)
		{
			_indent = indent;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Indent = _indent;
		}

		public override XFPropType Type
		{
			get { return XFPropType.TextIndentationLevel; }
		}

		public ushort Indent
		{
			get { return _indent; }
		}
	}

	#endregion // XFPropTextIndentationLevel class

	#region XFPropTextIndentationLevelRelative class

	internal class XFPropTextIndentationLevelRelative : XFProp
	{
		private short _indentOffset;

		public XFPropTextIndentationLevelRelative(short indentOffset)
		{
			_indentOffset = indentOffset;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Indent += _indentOffset;
		}

		public override XFPropType Type
		{
			get { return XFPropType.TextIndentationLevelRelative; }
		}

		public short IndentOffset
		{
			get { return _indentOffset; }
		}
	}

	#endregion // XFPropTextIndentationLevelRelative class

	#region XFPropTextRotation class

	// http://msdn.microsoft.com/en-us/library/dd925371(v=office.12).aspx
	internal class XFPropTextRotation : XFProp
	{
		private byte _rotation;

		public XFPropTextRotation(byte rotation)
		{
			_rotation = rotation;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.Rotation = _rotation;
		}

		public override XFPropType Type
		{
			get { return XFPropType.TextRotation; }
		}

		public byte Rotation
		{
			get { return _rotation; }
		}
	}

	#endregion // XFPropTextRotation class

	#region XFPropVerticalAlignment class

	// http://msdn.microsoft.com/en-us/library/dd908508(v=office.12).aspx
	internal class XFPropVerticalAlignment : XFProp
	{
		private VerticalCellAlignment _verticalAlignment;

		public XFPropVerticalAlignment(VerticalCellAlignment verticalAlignment)
		{
			_verticalAlignment = verticalAlignment;
		}

		public override void ApplyTo(WorksheetCellFormatData format)
		{
			format.VerticalAlignment = _verticalAlignment;
		}

		public override XFPropType Type
		{
			get { return XFPropType.VerticalAlignment; }
		}

		public VerticalCellAlignment VerticalAlignment
		{
			get { return _verticalAlignment; }
		}
	}

	#endregion // XFPropVerticalAlignment class


	#region XFPropType enum

	// http://msdn.microsoft.com/en-us/library/dd953723(v=office.12).aspx
	internal enum XFPropType : ushort
	{
		PatternFill = 0x00,
		ForegroundColor = 0x01,
		BackgroundColor = 0x02,
		GradientFill = 0x03,
		GradientStop = 0x04,
		FontColor = 0x05,
		TopBorder = 0x06,
		BottomBorder = 0x07,
		LeftBorder = 0x08,
		RightBorder = 0x09,
		DiagonalBorder = 0x0A,
		VerticalBorder = 0x0B,
		HorizontalBorder = 0x0C,
		DiagonalUpBorder = 0x0D,
		DiagonalDownBorder = 0x0E,
		HorizontalAlignment = 0x0F,
		VerticalAlignment = 0x10,
		TextRotation = 0x11,
		TextIndentationLevel = 0x12,
		ReadingOrder = 0x13,
		WrappedText = 0x14,
		JustifyDistributed = 0x15,
		ShrinkToFit = 0x16,
		CellMerged = 0x17,
		FontName = 0x18,
		FontBold = 0x19,
		FontUnderline = 0x1A,
		FontSubscriptSuperscript = 0x1B,
		FontItalic = 0x1C,
		FontStrikethrough = 0x1D,
		FontOutline = 0x1E,
		FontShadow = 0x1F,
		FontCondensed = 0x20,
		FontExtended = 0x21,
		FontCharacterSet = 0x22,
		FontFamily = 0x23,
		FontHeight = 0x24,
		FontScheme = 0x25,
		NumberFormat = 0x26,
		NumberFormatId = 0x29,
		TextIndentationLevelRelative = 0x2A,
		Locked = 0x2B,
		Hidden = 0x2C,
	}

	#endregion // XFPropType enum
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