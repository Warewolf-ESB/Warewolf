using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8
{
	// http://msdn.microsoft.com/en-us/library/dd906769(v=office.12).aspx
	internal abstract class ExtProp
	{
		public abstract void ApplyTo(BIFF8WorkbookSerializationManager manager, WorksheetCellFormatData formatData);
		public abstract ExtPropType ExtType { get; }
		public abstract void Save(Biff8RecordStream stream);
	}

	internal class ExtPropColor : ExtProp
	{
		private WorkbookColorInfo _colorInfo;
		private ExtPropType _extType;

		public ExtPropColor(WorkbookColorInfo colorInfo, ExtPropType extType)
		{
			Debug.Assert(colorInfo != null, "The colorInfo should not be null.");
			_colorInfo = colorInfo;
			_extType = extType;
		}

		public override void ApplyTo(BIFF8WorkbookSerializationManager manager, WorksheetCellFormatData formatData)
		{
			switch (_extType)
			{
				case ExtPropType.BackgroundColor:
				case ExtPropType.ForegroundColor:
					{
						CellFillPattern patternFill = formatData.Fill as CellFillPattern;
						if (patternFill != null)
						{
							WorkbookColorInfo backgroundColorInfo = patternFill.GetFileFormatBackgroundColorInfo(formatData);
							WorkbookColorInfo foregroundColorInfo = patternFill.GetFileFormatForegroundColorInfo(formatData);
							
							if (_extType == ExtPropType.BackgroundColor)
								backgroundColorInfo = _colorInfo;
							else
								foregroundColorInfo = _colorInfo;

							formatData.Fill = new CellFillPattern(backgroundColorInfo, foregroundColorInfo, patternFill.PatternStyle, formatData);
						}
					}
					break;

				case ExtPropType.BottomBorderColor:
					formatData.BottomBorderColorInfo = _colorInfo;
					break;
				case ExtPropType.CellTextColor:
					formatData.Font.ColorInfo = _colorInfo;
					break;
				case ExtPropType.DiagonalBorderColor:
					formatData.DiagonalBorderColorInfo = _colorInfo;
					break;
				case ExtPropType.LeftBorderColor:
					formatData.LeftBorderColorInfo = _colorInfo;
					break;
				case ExtPropType.RightBorderColor:
					formatData.RightBorderColorInfo = _colorInfo;
					break;
				case ExtPropType.TopBorderColor:
					formatData.TopBorderColorInfo = _colorInfo;
					break;

				default:
					Utilities.DebugFail("Unknown ExtPropType: " + _extType);
					break;
			}
		}

		public override ExtPropType ExtType
		{
			get { return _extType; }
		}

		public override void Save(Biff8RecordStream stream)
		{
			ColorableItem item;
			switch (_extType)
			{
				case ExtPropType.BackgroundColor:
				case ExtPropType.ForegroundColor:
					item = ColorableItem.CellFill;
					break;

				case ExtPropType.BottomBorderColor:
				case ExtPropType.DiagonalBorderColor:
				case ExtPropType.LeftBorderColor:
				case ExtPropType.RightBorderColor:
				case ExtPropType.TopBorderColor:
					item = ColorableItem.CellBorder;
					break;

				case ExtPropType.CellTextColor:
					item = ColorableItem.CellFont;
					break;

				default:
					Utilities.DebugFail("Unknown ExtPropType: " + _extType);
					item = ColorableItem.CellFill;
					break;
			}

			stream.WriteFullColorExt(_colorInfo, item);
		}
	}

	internal class ExtPropFontScheme : ExtProp
	{
		private FontScheme _fontScheme;

		public ExtPropFontScheme(FontScheme fontScheme)
		{
			_fontScheme = fontScheme;
		}

		public override void ApplyTo(BIFF8WorkbookSerializationManager manager, WorksheetCellFormatData formatData)
		{
			formatData.FontSchemeInternal = _fontScheme;
		}

		public override ExtPropType ExtType
		{
			get { return ExtPropType.FontScheme; }
		}

		public override void Save(Biff8RecordStream stream)
		{
			stream.Write((byte)_fontScheme);
		}
	}

	internal class ExtPropGradientFill : ExtProp
	{
		private CellFillGradient _gradientFill;

		public ExtPropGradientFill(CellFillGradient gradientFill)
		{
			_gradientFill = gradientFill;
		}

		public override void ApplyTo(BIFF8WorkbookSerializationManager manager, WorksheetCellFormatData formatData)
		{
			formatData.Fill = _gradientFill;
		}

		public override ExtPropType ExtType
		{
			get { return ExtPropType.GradientFill; }
		}

		public override void Save(Biff8RecordStream stream)
		{
			stream.WriteXFExtGradient(_gradientFill);
		}
	}

	internal class ExtPropTextIndentationLevel : ExtProp
	{
		private ushort _textIndentationLevel;

		public ExtPropTextIndentationLevel(ushort textIndentationLevel)
		{
			_textIndentationLevel = textIndentationLevel;
		}

		public override void ApplyTo(BIFF8WorkbookSerializationManager manager, WorksheetCellFormatData formatData)
		{
			formatData.Indent = _textIndentationLevel;
		}

		public override ExtPropType ExtType
		{
			get { return ExtPropType.TextIndentationLevel; }
		}

		public override void Save(Biff8RecordStream stream)
		{
			stream.Write((ushort)_textIndentationLevel);
		}
	}

	// http://msdn.microsoft.com/en-us/library/dd906769(v=office.12).aspx
	internal enum ExtPropType : ushort
	{
		ForegroundColor = 0x0004,
		BackgroundColor = 0x0005,
		GradientFill = 0x0006,
		TopBorderColor = 0x0007,
		BottomBorderColor = 0x0008,
		LeftBorderColor = 0x0009,
		RightBorderColor = 0x000A,
		DiagonalBorderColor = 0x000B,
		CellTextColor = 0x000D,
		FontScheme = 0x000E,
		TextIndentationLevel = 0x000F,
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