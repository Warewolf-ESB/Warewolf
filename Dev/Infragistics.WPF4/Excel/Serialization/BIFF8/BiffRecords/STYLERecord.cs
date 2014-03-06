using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd920702(v=office.12).aspx
	internal class STYLERecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();
			int styleXFIndex =	( optionFlags & 0x0FFF );
			bool isBuiltIn =	( optionFlags & 0x8000 ) == 0x8000;

			WorksheetCellFormatData format = manager.Formats[ styleXFIndex ];
			Debug.Assert(format.Type == WorksheetCellFormatType.StyleFormat, "The cell format type is invalid.");

			// MD 1/1/12 - 12.1 - Cell Format Updates
			// We need to use the style after it is created, so I moved the declaration and the call to Add outside the if block below.
			WorkbookStyle style;

			if ( isBuiltIn )
			{
				BuiltInStyleType type = (BuiltInStyleType)manager.CurrentRecordStream.ReadByte();
				byte outlineLevel = (byte)manager.CurrentRecordStream.ReadByte();

				// MD 1/1/12 - 12.1 - Cell Format Updates
				//WorkbookBuiltInStyle builtInStyle = new WorkbookBuiltInStyle( manager.Workbook, format, type, outlineLevel );
				//
				//manager.Workbook.Styles.Add( builtInStyle );
				style = new WorkbookBuiltInStyle(manager.Workbook, format, type, outlineLevel);
			}
			else
			{
				string name = manager.CurrentRecordStream.ReadFormattedString( LengthType.SixteenBit ).UnformattedString;

				// MD 1/1/12 - 12.1 - Cell Format Updates
				// The newer built in styles in 2007 are written out as user defined styles in the 2003 formats.
				//WorkbookUserDefinedStyle userDefinedStyle = new WorkbookUserDefinedStyle( manager.Workbook, format, name );
				//
				//manager.Workbook.Styles.Add( userDefinedStyle );
				WorkbookStyleCollection.BuiltInStyleInfo info;
				if (WorkbookStyleCollection.StyleTypesByName.TryGetValue(name, out info))
					style = new WorkbookBuiltInStyle(manager.Workbook, format, info.Type, info.OutlineLevel);
				else
					style = new WorkbookUserDefinedStyle(manager.Workbook, format, name);
			}

			// MD 1/1/12 - 12.1 - Cell Format Updates
			style = manager.Workbook.Styles.Add(style);
			manager.LoadedStyles.Add(style);

			List<WorksheetCellFormatData> childFormats;
			if (manager.ParentStylesToChildren.TryGetValue(styleXFIndex, out childFormats))
			{
				// Cache the style format so we can compare it with the child formats.
				WorksheetCellFormatData styleFormat = style.StyleFormatInternal;

				for (int i = 0; i < childFormats.Count; i++)
				{
					WorksheetCellFormatData childFormat = childFormats[i];
					Debug.Assert(childFormat.Style == manager.Workbook.Styles.NormalStyle, "This format's Style should not be set yet.");
					childFormat.Style = style;

					WorksheetCellFormatOptions formatOptions = childFormat.FormatOptions;

					#region Compare formats to see if any FormatOptions are missing

					if (Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting) == false)
					{
						if (childFormat.AlignmentResolved != styleFormat.AlignmentResolved ||
							childFormat.IndentResolved != styleFormat.IndentResolved ||
							childFormat.RotationResolved != styleFormat.RotationResolved ||
							childFormat.ShrinkToFitResolved != styleFormat.ShrinkToFitResolved ||
							childFormat.VerticalAlignmentResolved != styleFormat.VerticalAlignmentResolved ||
							childFormat.WrapTextResolved != styleFormat.WrapTextResolved)
						{
							formatOptions |= WorksheetCellFormatOptions.ApplyAlignmentFormatting;
						}
					}

					if (Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting) == false)
					{
						if (childFormat.BottomBorderColorInfoResolved != styleFormat.BottomBorderColorInfoResolved ||
							childFormat.BottomBorderStyleResolved != styleFormat.BottomBorderStyleResolved ||
							childFormat.DiagonalBorderColorInfoResolved != styleFormat.DiagonalBorderColorInfoResolved ||
							childFormat.DiagonalBordersResolved != styleFormat.DiagonalBordersResolved ||
							childFormat.DiagonalBorderStyleResolved != styleFormat.DiagonalBorderStyleResolved ||
							childFormat.LeftBorderColorInfoResolved != styleFormat.LeftBorderColorInfoResolved ||
							childFormat.LeftBorderStyleResolved != styleFormat.LeftBorderStyleResolved ||
							childFormat.RightBorderColorInfoResolved != styleFormat.RightBorderColorInfoResolved ||
							childFormat.RightBorderStyleResolved != styleFormat.RightBorderStyleResolved ||
							childFormat.TopBorderColorInfoResolved != styleFormat.TopBorderColorInfoResolved ||
							childFormat.TopBorderStyleResolved != styleFormat.TopBorderStyleResolved)
						{
							formatOptions |= WorksheetCellFormatOptions.ApplyBorderFormatting;
						}
					}

					if (Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyFillFormatting) == false)
					{
						if (Object.Equals(childFormat.FillResolved, styleFormat.FillResolved) == false)
						{
							formatOptions |= WorksheetCellFormatOptions.ApplyFillFormatting;
						}
					}

					if (Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyFontFormatting) == false)
					{
						if (childFormat.FontBoldResolved != styleFormat.FontBoldResolved ||
							childFormat.FontColorInfoResolved != styleFormat.FontColorInfoResolved ||
							childFormat.FontHeightResolved != styleFormat.FontHeightResolved ||
							childFormat.FontItalicResolved != styleFormat.FontItalicResolved ||
							childFormat.FontNameResolved != styleFormat.FontNameResolved ||
							childFormat.FontStrikeoutResolved != styleFormat.FontStrikeoutResolved ||
							childFormat.FontSuperscriptSubscriptStyleResolved != styleFormat.FontSuperscriptSubscriptStyleResolved ||
							childFormat.FontUnderlineStyleResolved != styleFormat.FontUnderlineStyleResolved)
						{
							formatOptions |= WorksheetCellFormatOptions.ApplyFontFormatting;
						}
					}

					if (Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting) == false)
					{
						if (childFormat.FormatStringResolved != styleFormat.FormatStringResolved)
						{
							formatOptions |= WorksheetCellFormatOptions.ApplyNumberFormatting;
						}
					}

					if (Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting) == false)
					{
						if (childFormat.LockedResolved != styleFormat.LockedResolved)
						{
							formatOptions |= WorksheetCellFormatOptions.ApplyProtectionFormatting;
						}
					}

					#endregion // Compare formats to see if any FormatOptions are missing

					childFormat.FormatOptions = formatOptions;

					// MD 4/4/12 - TFS107655
					// Make sure the cache the style format values of the cell format.
					childFormat.CacheResolvedFormatOptionsValues(formatOptions, styleFormat);

					// Reset all other format options so all properties have a default value if they are not explicitly set on this cell format.
					childFormat.ResetFormatOptions(WorksheetCellFormatOptions.All & ~formatOptions);
				}
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			WorkbookStyle style = (WorkbookStyle)manager.ContextStack[ typeof( WorkbookStyle ) ];

			if ( style == null )
			{
                Utilities.DebugFail("There was no style in the context stack.");
				return;
			}

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// We no longer cache format indexes because we can easily get them at save time.
			//int formatIndex = style.StyleFormatInternal.IndexInFormatCollection;
			int formatIndex = manager.GetStyleFormatIndex(style);

			if ( formatIndex < 0 )
			{
                Utilities.DebugFail("The format index has not been assigned for this style.");
				formatIndex = 0;
			}

			WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;

			// MD 1/1/12 - 12.1 - Cell Format Updates
			bool isBuiltInStyle = builtInStyle != null && builtInStyle.IsBuiltInStyleIn2003;

			ushort optionFlags = 0;

			optionFlags |= (ushort)formatIndex;

			// MD 1/1/12 - 12.1 - Cell Format Updates
			//if ( builtInStyle != null )
			//    optionFlags |= 0x8000;
			Utilities.SetBit(ref optionFlags, isBuiltInStyle, 15);

			manager.CurrentRecordStream.Write( optionFlags );

			// MD 1/1/12 - 12.1 - Cell Format Updates
			//if ( builtInStyle != null )
			if (isBuiltInStyle)
			{
				manager.CurrentRecordStream.Write( (byte)builtInStyle.Type );
				manager.CurrentRecordStream.Write( (byte)builtInStyle.OutlineLevel );
			}
			else
			{
				// MD 1/1/12 - 12.1 - Cell Format Updates
				// The style may be a built in style here now.
				//WorkbookUserDefinedStyle userDefinedStyle = (WorkbookUserDefinedStyle)style;
				//
				//manager.CurrentRecordStream.Write( userDefinedStyle.Name, LengthType.SixteenBit );
				manager.CurrentRecordStream.Write(style.Name, LengthType.SixteenBit);
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.STYLE; }
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