using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class FormatInfo
    {

        #region Members

        private WorksheetCellFormatOptions formatOptions = WorksheetCellFormatOptions.None;
        private int borderId = -1;
        private int fillId = -1;
        private int fontId = -1;
        private int numFmtId = -1;
        private bool pivotButton = false;
        private bool quotePrefix = false;
        private int xfId = -1;
        private ProtectionInfo protection = null;
        private AlignmentInfo alignment = null;
        private WorksheetCellFormatData formatDataObject = null;

        #endregion Members

        #region Properties

        #region FormatOptions






        public WorksheetCellFormatOptions FormatOptions
        {
            get { return this.formatOptions; }
            set { this.formatOptions = value; }
        }

        #endregion FormatOptions

		// MD 1/1/12 - 12.1 - Cell Format Updates
		// These are no longer needed.
		#region Removed

		//        #region ApplyAlignment

		//#if DEBUG
		//        /// <summary>
		//        /// Gets or sets the StyleCellFormatOptions.UseAlignmentFormatting on the FormatOptions
		//        /// </summary>
		//#endif
		//        public bool ApplyAlignment
		//        {
		//            get
		//            {
		//                return ((this.formatOptions & StyleCellFormatOptions.UseAlignmentFormatting) == StyleCellFormatOptions.UseAlignmentFormatting);
		//            }
		//            set
		//            {
		//                if (value)
		//                    this.formatOptions |= StyleCellFormatOptions.UseAlignmentFormatting;
		//                else
		//                    this.formatOptions &= ~StyleCellFormatOptions.UseAlignmentFormatting;
		//            }
		//        }

		//        #endregion ApplyAlignment

		//        #region ApplyBorder

		//#if DEBUG
		//        /// <summary>
		//        /// Gets or sets the StyleCellFormatOptions.UseBorderFormatting on the FormatOptions
		//        /// </summary>
		//#endif
		//        public bool ApplyBorder
		//        {
		//            get
		//            {
		//                return ((this.formatOptions & StyleCellFormatOptions.UseBorderFormatting) == StyleCellFormatOptions.UseBorderFormatting);
		//            }
		//            set
		//            {
		//                if (value)
		//                    this.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;
		//                else
		//                    this.formatOptions &= ~StyleCellFormatOptions.UseBorderFormatting;
		//            }
		//        }

		//        #endregion ApplyBorder

		//        #region ApplyFill

		//#if DEBUG
		//        /// <summary>
		//        /// Gets or sets the StyleCellFormatOptions.UsePatternsFormatting on the FormatOptions
		//        /// </summary>
		//#endif
		//        public bool ApplyFill
		//        {
		//            get
		//            {
		//                return ((this.formatOptions & StyleCellFormatOptions.UsePatternsFormatting) == StyleCellFormatOptions.UsePatternsFormatting);
		//            }
		//            set
		//            {
		//                if (value)
		//                    this.formatOptions |= StyleCellFormatOptions.UsePatternsFormatting;
		//                else
		//                    this.formatOptions &= ~StyleCellFormatOptions.UsePatternsFormatting;
		//            }
		//        }

		//        #endregion ApplyFill

		//        #region ApplyFont

		//#if DEBUG
		//        /// <summary>
		//        /// Gets or sets the StyleCellFormatOptions.UseFontFormatting on the FormatOptions
		//        /// </summary>
		//#endif
		//        public bool ApplyFont
		//        {
		//            get
		//            {
		//                return ((this.formatOptions & StyleCellFormatOptions.UseFontFormatting) == StyleCellFormatOptions.UseFontFormatting);
		//            }
		//            set
		//            {
		//                if (value)
		//                    this.formatOptions |= StyleCellFormatOptions.UseFontFormatting;
		//                else
		//                    this.formatOptions &= ~StyleCellFormatOptions.UseFontFormatting;
		//            }
		//        }

		//        #endregion ApplyFont

		//        #region ApplyNumberFormat

		//#if DEBUG
		//        /// <summary>
		//        /// Gets or sets the StyleCellFormatOptions.UseNumberFormatting on the FormatOptions
		//        /// </summary>
		//#endif
		//        public bool ApplyNumberFormat
		//        {
		//            get
		//            {
		//                return ((this.formatOptions & StyleCellFormatOptions.UseNumberFormatting) == StyleCellFormatOptions.UseNumberFormatting);
		//            }
		//            set
		//            {
		//                if (value)
		//                    this.formatOptions |= StyleCellFormatOptions.UseNumberFormatting;
		//                else
		//                    this.formatOptions &= ~StyleCellFormatOptions.UseNumberFormatting;
		//            }
		//        }

		//        #endregion ApplyNumberFormat

		//        #region ApplyProtection

		//#if DEBUG
		//        /// <summary>
		//        /// Gets or sets the StyleCellFormatOptions.UseProtectionFormatting on the FormatOptions
		//        /// </summary>
		//#endif
		//        public bool ApplyProtection
		//        {
		//            get
		//            {
		//                return ((this.formatOptions & StyleCellFormatOptions.UseProtectionFormatting) == StyleCellFormatOptions.UseProtectionFormatting);
		//            }
		//            set
		//            {
		//                if (value)
		//                    this.formatOptions |= StyleCellFormatOptions.UseProtectionFormatting;
		//                else
		//                    this.formatOptions &= ~StyleCellFormatOptions.UseProtectionFormatting;
		//            }
		//        }

		//        #endregion ApplyProtection

		#endregion // Removed

        #region BorderId






        public int BorderId
        {
            get { return this.borderId; }
            set { this.borderId = value; }
        }

        #endregion BorderId

        #region FillId






        public int FillId
        {
            get { return this.fillId; }
            set { this.fillId = value; }
        }

        #endregion FillId

        #region FontId






        public int FontId
        {
            get { return this.fontId; }
            set { this.fontId = value; }
        }

        #endregion FontId

        #region NumFmtId






        public int NumFmtId
        {
            get { return this.numFmtId; }
            set { this.numFmtId = value; }
        }

        #endregion NumFmtId

        #region PivotButton






        public bool PivotButton
        {
            get { return this.pivotButton; }
            set { this.pivotButton = value; }
        }

        #endregion PivotButton

        #region QuotePrefix






        public bool QuotePrefix
        {
            get { return this.quotePrefix; }
            set { this.quotePrefix = value; }
        }

        #endregion QuotePrefix

        #region CellStyleXfId






        public int CellStyleXfId
        {
            get { return this.xfId; }
            set { this.xfId = value; }
        }

        #endregion CellStyleXfId

        #region Protection






        public ProtectionInfo Protection
        {
            get { return this.protection; }
            set { this.protection = value; }
        }

        #endregion Protection

        #region Alignment






        public AlignmentInfo Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        #endregion Alignment

        #region FormatDataObject

        public WorksheetCellFormatData FormatDataObject
        {
            get { return this.formatDataObject; }
            set { this.formatDataObject = value; }
        }

        #endregion FormatDataObject

        #endregion Properties

        #region Methods

        #region CreateWorksheetCellFormatData

		// MD 1/8/12 - 12.1 - Cell Format Updates
		// Added a parameter which indicates whether a cell or style format is getting created.
        //internal WorksheetCellFormatData CreateWorksheetCellFormatData(Excel2007WorkbookSerializationManager manager)
		internal WorksheetCellFormatData CreateWorksheetCellFormatData(Excel2007WorkbookSerializationManager manager, bool isStyleFormat)
        {
            if (manager == null)
                return null;

			// MD 2/27/12 - 12.1 - Table Support
            //WorksheetCellFormatData format = (WorksheetCellFormatData)manager.Workbook.CreateNewWorksheetCellFormat();
			WorksheetCellFormatData format = manager.Workbook.CreateNewWorksheetCellFormatInternal(
				isStyleFormat ? WorksheetCellFormatType.StyleFormat : WorksheetCellFormatType.CellFormat);

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// This is set when the format object is created.
			//format.IsStyle = isStyleFormat;

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Cache the format options and all apply flags
			WorksheetCellFormatOptions formatOptions = this.FormatOptions;
			bool applyAlignmentFormatting = Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting);
			bool applyBorderFormatting = Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting);
			bool applyFillFormatting = Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyFillFormatting);
			bool applyFontFormatting = Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyFontFormatting);
			bool applyNumberFormatting = Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting);
			bool applyProtectionFormatting = Utilities.TestFlag(formatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting);

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// If this is a cell style, set the Style and resolve any apply flags to True if the saved values differ from the style.
			if (format.Type != WorksheetCellFormatType.StyleFormat && this.CellStyleXfId >= 0)
			{
				FormatInfo styleFormatInfo = manager.CellStyleXfs[this.CellStyleXfId];

				WorkbookStyle style;
				if (manager.StylesByCellStyleXfId.TryGetValue(this.CellStyleXfId, out style) == false)
				{
					// If there was no style using the specified style format, we need to generate a custom one (Excel does not do this, 
					// they maintain an internal reference to the style format, but their Style property for the cell reteurns null).
					string styleName;
					int index = 1;
					do
					{
						styleName = string.Format("Style {0}", index++);
					}
					while (manager.Workbook.Styles[styleName] != null);

					style = manager.Workbook.Styles.AddUserDefinedStyle(styleFormatInfo.FormatDataObject, styleName);
					manager.StylesByCellStyleXfId.Add(this.CellStyleXfId, style);
				}

				format.Style = style;

				if (applyAlignmentFormatting == false && (Object.Equals(this.Alignment, styleFormatInfo.Alignment) == false))
				{
					applyAlignmentFormatting = true;
					formatOptions |= WorksheetCellFormatOptions.ApplyAlignmentFormatting;
				}

				if (applyBorderFormatting == false && this.BorderId != styleFormatInfo.BorderId)
				{
					applyBorderFormatting = true;
					formatOptions |= WorksheetCellFormatOptions.ApplyBorderFormatting;
				}

				if (applyFillFormatting == false && this.FillId != styleFormatInfo.FillId)
				{
					applyFillFormatting = true;
					formatOptions |= WorksheetCellFormatOptions.ApplyFillFormatting;
				}

				if (applyFontFormatting == false && this.FontId != styleFormatInfo.FontId)
				{
					applyFontFormatting = true;
					formatOptions |= WorksheetCellFormatOptions.ApplyFontFormatting;
				}

				if (applyNumberFormatting == false && this.numFmtId != styleFormatInfo.numFmtId)
				{
					applyNumberFormatting = true;
					formatOptions |= WorksheetCellFormatOptions.ApplyNumberFormatting;
				}

				if (applyProtectionFormatting == false && (Object.Equals(this.Protection, styleFormatInfo.Protection) == false))
				{
					applyProtectionFormatting = true;
					formatOptions |= WorksheetCellFormatOptions.ApplyProtectionFormatting;
				}
			}

			format.FormatOptions = formatOptions;

			// MD 12/31/11 - 12.1 - Table Support
			// Only copy the properties if the format options bit is set. Otherwise, they are just inherited from the style.
			//if (this.FontId < manager.Fonts.Count &&
			//    this.FontId > -1)
			if (applyFontFormatting &&
				this.FontId < manager.Fonts.Count &&
				this.FontId > -1)
			{
				// MD 1/18/12 - 12.1 - Cell Format Updates
				//format.Font.SetFontFormatting((IWorkbookFont)manager.Fonts[this.FontId]);
				WorkbookFontData resolvedFont = manager.Fonts[this.FontId].ResolvedFontData(format);
				format.Font.SetFontFormatting(resolvedFont);
			}

			// MD 12/31/11 - 12.1 - Table Support
			// Only copy the properties if the format options bit is set. Otherwise, they are just inherited from the style.
			//format.FormatString = manager.Workbook.Formats[this.NumFmtId];
			if (applyNumberFormatting)
			{
				format.FormatString = manager.Workbook.Formats[this.NumFmtId];
			}

			// MD 12/31/11 - 12.1 - Table Support
			// Set this after loading all other properties (which will modify the format options)
            //format.FormatOptions = formatInfo.FormatOptions;

			// MD 12/31/11 - 12.1 - Table Support
			// Only copy the properties if the format options bit is set. Otherwise, they are just inherited from the style.
			//if (this.Protection != null)
			if (applyProtectionFormatting && this.Protection != null)
            {
				// MD 12/21/11 - 12.1 - Table Support
				// Moved this code to a method so it could be used in other places.
				#region Moved

				//format.Locked = (formatInfo.Protection.Locked) ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
				////format.Hidden = formatInfo.Protection.Hidden;

				#endregion // Moved
				this.Protection.ApplyTo(format);
            }

			// MD 12/31/11 - 12.1 - Table Support
			// Only copy the properties if the format options bit is set. Otherwise, they are just inherited from the style.
			//if (this.Alignment != null)
			if (applyAlignmentFormatting && this.Alignment != null)
            {
				// MD 12/21/11 - 12.1 - Table Support
				// Moved this code to a method so it could be used in other places.
				#region Moved

				//format.Alignment = formatInfo.Alignment.Horizontal;
				//format.Rotation = formatInfo.Alignment.TextRotation;
				//format.WrapText = (formatInfo.Alignment.WrapText) ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
				//format.VerticalAlignment = formatInfo.Alignment.Vertical;
				//format.Indent = formatInfo.Alignment.Indent;
				//format.ShrinkToFit = (formatInfo.Alignment.ShrinkToFit) ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;

				#endregion // Moved
				this.Alignment.ApplyTo(format);
            }

			// MD 12/31/11 - 12.1 - Table Support
			// Only copy the properties if the format options bit is set. Otherwise, they are just inherited from the style.
			//if (this.BorderId < manager.Borders.Count &&
			//    this.BorderId > -1)
			if (applyBorderFormatting && 
				this.BorderId < manager.Borders.Count &&
				this.BorderId > -1)
            {
				BorderInfo borderInfo = manager.Borders[this.BorderId];

				// MD 12/21/11 - 12.1 - Table Support
				// Moved this code to a method so it could be used in other places.
				#region Moved

				//if (borderInfo.Bottom != null)
				//{
				//    format.BottomBorderColor = borderInfo.Bottom.ColorInfo.ResolveColor(manager);
				//    if (borderInfo.Bottom.ColorInfo.Indexed != null &&
				//        borderInfo.Bottom.ColorInfo.Indexed != 0x7FFF)
				//        format.BottomBorderColorIndex = Convert.ToInt32(borderInfo.Bottom.ColorInfo.Indexed);
				//    format.BottomBorderStyle = borderInfo.Bottom.BorderStyle;
				//}

				//if (borderInfo.Top != null)
				//{
				//    format.TopBorderColor = borderInfo.Top.ColorInfo.ResolveColor(manager);
				//    if (borderInfo.Top.ColorInfo.Indexed != null &&
				//        borderInfo.Top.ColorInfo.Indexed != 0x7FFF)
				//        format.TopBorderColorIndex = Convert.ToInt32(borderInfo.Top.ColorInfo.Indexed);
				//    format.TopBorderStyle = borderInfo.Top.BorderStyle;
				//}

				//if (borderInfo.Left != null)
				//{
				//    format.LeftBorderColor = borderInfo.Left.ColorInfo.ResolveColor(manager);
				//    if (borderInfo.Left.ColorInfo.Indexed != null &&
				//        borderInfo.Left.ColorInfo.Indexed != 0x7FFF)
				//        format.LeftBorderColorIndex = Convert.ToInt32(borderInfo.Left.ColorInfo.Indexed);
				//    format.LeftBorderStyle = borderInfo.Left.BorderStyle;
				//}

				//if (borderInfo.Right != null)
				//{
				//    format.RightBorderColor = borderInfo.Right.ColorInfo.ResolveColor(manager);
				//    if (borderInfo.Right.ColorInfo.Indexed != null &&
				//        borderInfo.Right.ColorInfo.Indexed != 0x7FFF)
				//    //GT 8/26/10 Fixed Copy-Paste error. It was setting LeftBorderColorIndex
				//        format.RightBorderColorIndex = Convert.ToInt32(borderInfo.Right.ColorInfo.Indexed);
				//    format.RightBorderStyle = borderInfo.Right.BorderStyle;
				//}

				//// MD 10/26/11 - TFS91546
				//if (borderInfo.Diagonal != null)
				//{
				//    format.DiagonalBorderColor = borderInfo.Diagonal.ColorInfo.ResolveColor(manager);
				//    if (borderInfo.Diagonal.ColorInfo.Indexed != null &&
				//        borderInfo.Diagonal.ColorInfo.Indexed != 0x7FFF)
				//    {
				//        format.DiagonalBorderColorIndex = Convert.ToInt32(borderInfo.Diagonal.ColorInfo.Indexed);
				//    }
				//    format.DiagonalBorderStyle = borderInfo.Diagonal.BorderStyle;
				//}

				//DiagonalBorders diagonalBorders = DiagonalBorders.None;

				//if (borderInfo.DiagonalDown)
				//    diagonalBorders |= DiagonalBorders.DiagonalDown;

				//if (borderInfo.DiagonalUp)
				//    diagonalBorders |= DiagonalBorders.DiagonalUp;

				//format.DiagonalBorders = diagonalBorders;
				// ----------------End of TFS91546------------

				#endregion // Moved
				borderInfo.ApplyTo(format, manager);
            }

			// MD 12/31/11 - 12.1 - Table Support
			// Only copy the properties if the format options bit is set. Otherwise, they are just inherited from the style.
			//if (this.FillId < manager.Fills.Count &&
			//    this.FillId > -1)
			if (applyFillFormatting &&
				this.FillId < manager.Fills.Count &&
				this.FillId > -1)
            {
				// MD 12/21/11 - 12.1 - Table Support
				// Moved this code to a method so it could be used in other places.
				#region Moved

				//PatternFillInfo fillInfo = manager.Fills[formatInfo.FillId].PatternFill;
				//if (fillInfo != null)
				//{
				//    format.FillPattern = fillInfo.PatternStyle;
				//    if (fillInfo.BackgroundColor != null)
				//    {
				//        format.FillPatternBackgroundColor = fillInfo.BackgroundColor.ResolveColor(manager);
				//        if (fillInfo.BackgroundColor.Indexed != null &&
				//            fillInfo.BackgroundColor.Indexed != 0x7FFF)
				//            format.FillPatternBackgroundColorIndex = Convert.ToInt32(fillInfo.BackgroundColor.Indexed);
				//    }
				//    if (fillInfo.ForegroundColor != null)
				//    {
				//        // MD 4/29/11 - TFS73906
				//        // There is no longer an overload which takes an isForFont parameter.
				//        //// MD 11/29/10 - TFS37177
				//        //// The foreground color should be treated as if it were a font color, in that certain system colors should be resolved to their opposite.
				//        ////format.FillPatternForegroundColor = fillInfo.ForegroundColor.ResolveColor(manager);
				//        //format.FillPatternForegroundColor = fillInfo.ForegroundColor.ResolveColor(manager, true);
				//        format.FillPatternForegroundColor = fillInfo.ForegroundColor.ResolveColor(manager);

				//        if (fillInfo.ForegroundColor.Indexed != null &&
				//            fillInfo.ForegroundColor.Indexed != 0x7FFF)
				//            format.FillPatternForegroundColorIndex = Convert.ToInt32(fillInfo.ForegroundColor.Indexed);
				//    }
				//}

				#endregion // Moved
				FillInfo fillInfo = manager.Fills[this.FillId];
				fillInfo.ApplyTo(format, manager);
            }

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// The IsStyle value is being set above now.
			//format.IsStyle = (this.CellStyleXfId == -1);

            return format;
        }

        #endregion CreateWorksheetCellFormatData

        #region HasSameData

        internal static bool HasSameData(FormatInfo format1, FormatInfo format2)
        {
            if (ReferenceEquals(format1, null) &&
                ReferenceEquals(format2, null))
                return true;
            if (ReferenceEquals(format1, null) ||
                ReferenceEquals(format2, null))
                return false;
            return (AlignmentInfo.HasSameData(format1.alignment, format2.alignment) &&
                format1.formatOptions == format2.formatOptions &&
                format1.borderId == format2.borderId &&
                format1.xfId == format2.xfId &&
                format1.fillId == format2.fillId &&
                format1.fontId == format2.fontId &&
                format1.numFmtId == format2.numFmtId &&
                format1.pivotButton == format2.pivotButton &&
                ProtectionInfo.HasSameData(format1.protection, format2.protection) &&
                format1.quotePrefix == format2.quotePrefix);
        }

        #endregion HasSameData

        #endregion Methods
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