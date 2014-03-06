using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization.BIFF8;







using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 1/8/12 - 12.1 - Cell Format Updates
	// The code we changed too much for the cell format updates, so I just commented out the old code.
	#region Old Code

	//    internal sealed class WorksheetCellFormatData : GenericCacheElement, 
	//        IWorksheetCellFormat
	//    {
	//        #region Private Members

	//        private HorizontalCellAlignment alignment;
	//        private int bottomBorderColorIndex;
	//        private CellBorderLineStyle bottomBorderStyle;

	//        // MD 7/2/09 - TFS18634
	//        private int cachedHashCode;

	//        private FillPatternStyle fillPattern;
	//        private int fillPatternForegroundColorIndex;
	//        private int fillPatternBackgroundColorIndex;
	//        private WorkbookFontProxy font;
	//        private int formatStringIndex;
	//        private int indent;
	//        private int leftBorderColorIndex;
	//        private CellBorderLineStyle leftBorderStyle;
	//        private ExcelDefaultableBoolean locked;
	//        private int rightBorderColorIndex;
	//        private CellBorderLineStyle rightBorderStyle;
	//        private int rotation;
	//        private ExcelDefaultableBoolean shrinkToFit;
	//        private bool style;
	//        private StyleCellFormatOptions formatOptions;
	//        private int topBorderColorIndex;
	//        private CellBorderLineStyle topBorderStyle;
	//        private VerticalCellAlignment verticalAlignment;
	//        private ExcelDefaultableBoolean wrapText;

	//        // MBS 7/15/08 - Excel 2007 Format
	//        internal const int MaxIndent = 15;
	//        internal const int MaxIndent2007 = 250;

	//        // MD 11/24/10 - TFS34598
	//        // We will now store RGB data as an alternative to indexed values when we are using 2007 or later formats.
	//        private Color fillPatternBackgroundColor;
	//        private Color fillPatternForegroundColor;

	//        // MD 10/26/11 - TFS91546
	//        private int diagonalBorderColorIndex;
	//        private DiagonalBorders diagonalBorders;
	//        private DiagonalBorders previousDiagonalBorders;
	//        private CellBorderLineStyle diagonalBorderStyle;

	//        // MD 11/29/11 - TFS96205
	//        private Dictionary<ExtPropType, ExtProp> roundTripProps;

	//        #region Serialization Cache

	//        private int indexInFormatCollection = -1;
	//        private int indexInXfsCollection = -1;

	//        #endregion Serialization Cache

	//        #endregion Private Members

	//        #region Constructors

	//        public WorksheetCellFormatData( Workbook workbook, GenericCachedCollection<WorkbookFontData> fonts )
	//            : base( workbook )
	//        {
	//            // MD 2/15/11 - TFS66333
	//            // Use the EmptyElement for the initial data element. The DefaultElement will be populated with data if 
	//            // the workbook was loaded from a file or stream.
	//            //this.font = new WorkbookFontProxy( workbook.Fonts.DefaultElement, fonts, workbook );
	//            // MD 4/18/11 - TFS62026
	//            // The workbook parameter could be null.
	//            //this.font = new WorkbookFontProxy(workbook.Fonts.EmptyElement, fonts, workbook);
	//            WorkbookFontData emptyFont;
	//            if (workbook != null)
	//                emptyFont = workbook.Fonts.EmptyElement;
	//            else
	//                emptyFont = new WorkbookFontData(null);

	//            this.font = new WorkbookFontProxy(emptyFont, fonts, workbook);

	//            this.formatStringIndex = -1;

	//            this.locked = ExcelDefaultableBoolean.Default;
	//            this.wrapText = ExcelDefaultableBoolean.Default;
	//            this.shrinkToFit = ExcelDefaultableBoolean.Default;

	//            this.alignment = HorizontalCellAlignment.Default;
	//            this.verticalAlignment = VerticalCellAlignment.Default;

	//            this.rotation = -1;
	//            this.indent = -1;

	//            this.leftBorderStyle = CellBorderLineStyle.Default;
	//            this.rightBorderStyle = CellBorderLineStyle.Default;
	//            this.topBorderStyle = CellBorderLineStyle.Default;
	//            this.bottomBorderStyle = CellBorderLineStyle.Default;

	//            this.leftBorderColorIndex = -1;
	//            this.rightBorderColorIndex = -1;
	//            this.topBorderColorIndex = -1;
	//            this.bottomBorderColorIndex = -1;

	//            this.fillPattern = FillPatternStyle.Default;

	//            this.fillPatternForegroundColorIndex = -1;
	//            this.fillPatternBackgroundColorIndex = -1;

	//            this.formatOptions = StyleCellFormatOptions.None;

	//            // MD 10/26/11 - TFS91546
	//            this.diagonalBorderColorIndex = -1;
	//            this.diagonalBorders = DiagonalBorders.None;
	//            this.diagonalBorderStyle = CellBorderLineStyle.Default;
	//        }

	//        // MD 4/18/11 - TFS62026
	//        // Removed: It is faster to use the Clone method.
	//        #region Removed

	//        //private WorksheetCellFormatData( WorksheetCellFormatData source )
	//        //    : base( source.Workbook )
	//        //{
	//        //    this.style = source.style;
	//        //
	//        //    // Don't provide the collection of fonts because this element may just be a temporary clone while a property is being set.s
	//        //    this.font = new WorkbookFontProxy( source.font, null, source.Workbook );
	//        //
	//        //    this.formatStringIndex = source.formatStringIndex;
	//        //
	//        //    this.locked = source.locked;
	//        //    this.wrapText = source.wrapText;
	//        //    this.shrinkToFit = source.shrinkToFit;
	//        //
	//        //    this.alignment = source.alignment;
	//        //    this.verticalAlignment = source.verticalAlignment;
	//        //
	//        //    this.rotation = source.rotation;
	//        //    this.indent = source.indent;
	//        //
	//        //    this.leftBorderStyle = source.leftBorderStyle;
	//        //    this.rightBorderStyle = source.rightBorderStyle;
	//        //    this.topBorderStyle = source.topBorderStyle;
	//        //    this.bottomBorderStyle = source.bottomBorderStyle;
	//        //
	//        //    this.leftBorderColorIndex = source.leftBorderColorIndex;
	//        //    this.rightBorderColorIndex = source.rightBorderColorIndex;
	//        //    this.topBorderColorIndex = source.topBorderColorIndex;
	//        //    this.bottomBorderColorIndex = source.bottomBorderColorIndex;
	//        //
	//        //    this.fillPattern = source.fillPattern;
	//        //    this.fillPatternForegroundColorIndex = source.fillPatternForegroundColorIndex;
	//        //    this.fillPatternBackgroundColorIndex = source.fillPatternBackgroundColorIndex;
	//        //
	//        //    // MD 11/24/10 - TFS34598
	//        //    // Copy over the RGB data as well.
	//        //    this.fillPatternForegroundColor = source.fillPatternForegroundColor;
	//        //    this.fillPatternBackgroundColor = source.fillPatternBackgroundColor;
	//        //
	//        //    this.formatOptions = source.formatOptions;
	//        //
	//        //    this.indexInFormatCollection = source.indexInFormatCollection;
	//        //    this.indexInXfsCollection = source.indexInXfsCollection;
	//        //} 

	//        #endregion // Removed

	//        #endregion Constructors

	//        #region Base Class Overrides

	//        #region Clone

	//        public override object Clone()
	//        {
	//            // MD 4/18/11 - TFS62026
	//            // Call off to the new CloneInternal method
	//            //return new WorksheetCellFormatData( this );
	//            return this.CloneInternal();
	//        }

	//        // MD 4/18/11 - TFS62026
	//        public WorksheetCellFormatData CloneInternal()
	//        {
	//            WorksheetCellFormatData clone = (WorksheetCellFormatData)this.MemberwiseClone();

	//            Workbook workbook = this.Workbook;
	//            GenericCachedCollection<WorkbookFontData> fonts = null;
	//            if (workbook != null)
	//                fonts = workbook.Fonts;

	//            clone.font = new WorkbookFontProxy(this.font.Element, fonts, workbook);
	//            clone.ResetReferenceCount();

	//            // MD 11/29/11 - TFS96205
	//            if (this.roundTripProps != null)
	//                clone.roundTripProps = new Dictionary<ExtPropType, ExtProp>(this.roundTripProps);

	//            return clone;
	//        }

	//        #endregion Clone

	//        #region Equals

	//        public override bool Equals( object obj )
	//        {
	//            if ( this == obj )
	//                return true;

	//            WorksheetCellFormatData otherFormat = obj as WorksheetCellFormatData;

	//            if ( otherFormat == null )
	//                return false;

	//            // Style and style options are not considered in HasSameData, because they do not affect the appearance of the format,
	//            // so check them here
	//            if ( this.style != otherFormat.style )
	//                return false;

	//            if ( this.formatOptions != otherFormat.formatOptions )
	//                return false;

	//            return this.HasSameData( otherFormat );
	//        }

	//        #endregion Equals

	//        #region GetHashCode

	//        public override int GetHashCode()
	//        {
	//            // MD 7/2/09 - TFS18634
	//            if ( this.cachedHashCode != 0 )
	//                return this.cachedHashCode;

	//            int tmp = Convert.ToInt32( this.Style );

	//            if ( this.font.Element != null )
	//                tmp ^= this.font.Element.GetHashCode()		<< 1;

	//            tmp ^= this.formatStringIndex					<< 2;
	//            tmp ^= (int)this.locked							<< 3;
	//            tmp ^= (int)this.wrapText						<< 4;
	//            tmp ^= (int)this.shrinkToFit					<< 5;
	//            tmp ^= (int)this.alignment						<< 6;
	//            tmp ^= (int)this.verticalAlignment				<< 7;
	//            tmp ^= this.rotation							<< 8;
	//            tmp ^= this.indent								<< 9;
	//            tmp ^= (int)this.leftBorderStyle				<< 10;
	//            tmp ^= (int)this.rightBorderStyle				<< 11;
	//            tmp ^= (int)this.topBorderStyle					<< 12;
	//            tmp ^= (int)this.bottomBorderStyle				<< 13;
	//            tmp ^= this.leftBorderColorIndex				<< 14;
	//            tmp ^= this.rightBorderColorIndex				<< 15;
	//            tmp ^= this.topBorderColorIndex					<< 16;
	//            tmp ^= this.bottomBorderColorIndex				<< 17;
	//            tmp ^= (int)this.fillPattern					<< 18;
	//            tmp ^= this.fillPatternForegroundColorIndex		<< 19;
	//            tmp ^= this.fillPatternBackgroundColorIndex		<< 20;
	//            tmp ^= (int)this.formatOptions					<< 21;

	//            // MD 10/26/11 - TFS91546
	//            tmp ^= this.diagonalBorderColorIndex			<< 22;
	//            tmp ^= (int)diagonalBorders						<< 23;
	//            tmp ^= (int)this.diagonalBorderStyle			<< 24;

	//            // MD 11/24/10 - TFS34598
	//            // The RGB data should be included in the hash code.
	//            tmp ^= Utilities.ColorToArgb(this.fillPatternForegroundColor);
	//            tmp ^= Utilities.ColorToArgb(this.fillPatternBackgroundColor);			

	//            // MD 7/2/09 - TFS18634
	//            this.cachedHashCode = tmp;

	//            return tmp;
	//        }

	//        #endregion GetHashCode

	//        #region HasSameData

	//        public override bool HasSameData( GenericCacheElement otherElement )
	//        {
	//            if ( this == otherElement )
	//                return true;

	//            WorksheetCellFormatData otherFormat = (WorksheetCellFormatData)otherElement;

	//            if ( this.formatStringIndex != otherFormat.formatStringIndex )
	//                return false;

	//            if ( this.locked != otherFormat.locked )
	//                return false;

	//            if ( this.wrapText != otherFormat.wrapText )
	//                return false;

	//            if ( this.shrinkToFit != otherFormat.shrinkToFit )
	//                return false;

	//            if ( this.alignment != otherFormat.alignment )
	//                return false;

	//            if ( this.verticalAlignment != otherFormat.verticalAlignment )
	//                return false;

	//            if ( this.rotation != otherFormat.rotation )
	//                return false;

	//            if ( this.indent != otherFormat.indent )
	//                return false;

	//            if ( this.leftBorderStyle != otherFormat.leftBorderStyle )
	//                return false;

	//            if ( this.rightBorderStyle != otherFormat.rightBorderStyle )
	//                return false;

	//            if ( this.topBorderStyle != otherFormat.topBorderStyle )
	//                return false;

	//            if ( this.bottomBorderStyle != otherFormat.bottomBorderStyle )
	//                return false;

	//            if ( this.leftBorderColorIndex != otherFormat.leftBorderColorIndex )
	//                return false;

	//            if ( this.rightBorderColorIndex != otherFormat.rightBorderColorIndex )
	//                return false;

	//            if ( this.topBorderColorIndex != otherFormat.topBorderColorIndex )
	//                return false;

	//            if ( this.bottomBorderColorIndex != otherFormat.bottomBorderColorIndex )
	//                return false;

	//            if ( this.fillPattern != otherFormat.fillPattern )
	//                return false;

	//            if ( this.fillPatternForegroundColorIndex != otherFormat.fillPatternForegroundColorIndex )
	//                return false;

	//            if ( this.fillPatternBackgroundColorIndex != otherFormat.fillPatternBackgroundColorIndex )
	//                return false;

	//            // MD 11/24/10 - TFS34598
	//            // The RGB data should be considered in the equality check
	//            if (this.fillPatternForegroundColor != otherFormat.fillPatternForegroundColor)
	//                return false;

	//            if (this.fillPatternBackgroundColor != otherFormat.fillPatternBackgroundColor)
	//                return false;

	//            // MD 10/26/11 - TFS91546
	//            if (this.diagonalBorderColorIndex != otherFormat.diagonalBorderColorIndex)
	//                return false;

	//            // MD 10/26/11 - TFS91546
	//            if (this.diagonalBorders != otherFormat.diagonalBorders)
	//                return false;

	//            // MD 10/26/11 - TFS91546
	//            if (this.diagonalBorderStyle != otherFormat.diagonalBorderStyle)
	//                return false;

	//            if ( this.font.Element.Equals( otherFormat.font.Element ) == false )
	//                return false;

	//            return true;
	//        }

	//        #endregion HasSameData

	//        #region OnAddedToRootCollection

	//        public override void OnAddedToRootCollection()
	//        {
	//            // When the format has been added to the workbook's collection of formats, its font is rooted
	//            if ( this.font != null )
	//                this.font.OnRooted( this.Workbook.Fonts );
	//        }

	//        #endregion OnAddedToRootCollection

	//        // MD 11/24/10 - TFS34598
	//        #region OnCurrentFormatChanged

	//        public override void OnCurrentFormatChanged()
	//        {
	//            if (Utilities.IsRGBColorFormatSupported(this.Workbook.CurrentFormat) == false)
	//            {
	//                if (Utilities.ColorIsEmpty(this.fillPatternForegroundColor) == false)
	//                    this.FillPatternForegroundColor = this.fillPatternForegroundColor;

	//                if (Utilities.ColorIsEmpty(this.fillPatternBackgroundColor) == false)
	//                    this.FillPatternBackgroundColor = this.fillPatternBackgroundColor;
	//            }
	//        }

	//        #endregion // OnCurrentFormatChanged

	//        #region OnRemovedFromCollection

	//        public override void OnRemovedFromRootCollection()
	//        {
	//            // When the format has been removed from the workbook's collection of formats, its font is not rooted anymore
	//            if ( this.font != null )
	//                this.font.OnUnrooted();
	//        }

	//        #endregion OnRemovedFromCollection

	//        #endregion Base Class Overrides

	//        #region Methods

	//        #region Public Methods

	//        // MD 1/14/08 - BR29635
	//        #region RemoveUsedColorIndicies

	//        public override void RemoveUsedColorIndicies( List<int> unusedIndicies )
	//        {
	//            Utilities.RemoveValueFromSortedList( this.bottomBorderColorIndex, unusedIndicies );
	//            Utilities.RemoveValueFromSortedList( this.fillPatternBackgroundColorIndex, unusedIndicies );
	//            Utilities.RemoveValueFromSortedList( this.fillPatternForegroundColorIndex, unusedIndicies );
	//            Utilities.RemoveValueFromSortedList( this.leftBorderColorIndex, unusedIndicies );
	//            Utilities.RemoveValueFromSortedList( this.rightBorderColorIndex, unusedIndicies );
	//            Utilities.RemoveValueFromSortedList( this.topBorderColorIndex, unusedIndicies );

	//            // MD 10/26/11 - TFS91546
	//            Utilities.RemoveValueFromSortedList(this.diagonalBorderColorIndex, unusedIndicies);
	//        }

	//        #endregion RemoveUsedColorIndicies

	//        #region SetFormatting

	//        public void SetFormatting( IWorksheetCellFormat source )
	//        {
	//            if ( source == null )
	//                throw new ArgumentNullException( "source", SR.GetString( "LE_ArgumentNullException_SourceFormatting" ) );

	//            this.font.SetFontFormatting( source.Font );

	//            // Copy all properties from the source
	//            this.FormatString = source.FormatString;

	//            this.locked = source.Locked;
	//            this.wrapText = source.WrapText;
	//            this.shrinkToFit = source.ShrinkToFit;

	//            this.alignment = source.Alignment;
	//            this.verticalAlignment = source.VerticalAlignment;

	//            this.rotation = source.Rotation;
	//            this.indent = source.Indent;

	//            this.leftBorderStyle = source.LeftBorderStyle;
	//            this.rightBorderStyle = source.RightBorderStyle;
	//            this.topBorderStyle = source.TopBorderStyle;
	//            this.bottomBorderStyle = source.BottomBorderStyle;

	//            this.LeftBorderColor = source.LeftBorderColor;
	//            this.RightBorderColor = source.RightBorderColor;
	//            this.TopBorderColor = source.TopBorderColor;
	//            this.BottomBorderColor = source.BottomBorderColor;

	//            this.fillPattern = source.FillPattern;
	//            this.FillPatternForegroundColor = source.FillPatternForegroundColor;
	//            this.FillPatternBackgroundColor = source.FillPatternBackgroundColor;

	//            // MD 10/26/11 - TFS91546
	//            this.DiagonalBorderColor = source.DiagonalBorderColor;
	//            this.diagonalBorders = source.DiagonalBorders;
	//            this.diagonalBorderStyle = source.DiagonalBorderStyle;

	//            // MD 7/2/09 - TFS18634
	//            this.cachedHashCode = 0;
	//        }

	//        #endregion SetFormatting

	//        #endregion Public Methods

	//        #region Internal Methods

	//        // MD 11/29/11 - TFS96205
	//        #region ClearRoundTripProp

	//        internal void ClearRoundTripProp(ExtPropType propType)
	//        {
	//            if (this.roundTripProps == null)
	//                return;

	//            this.roundTripProps.Remove(propType);
	//        }

	//        #endregion  // ClearRoundTripProp

	//        #region Combine

	//#if DEBUG
	//        /// <summary>
	//        /// Combines two ranked formats to get a single font.
	//        /// </summary>  
	//#endif
	//        internal static WorksheetCellFormatData Combine(  
	//            WorksheetCellFormatData highPriorityFormat, 
	//            WorksheetCellFormatData lowPriorityFormat,
	//            // MD 3/21/11 - TFS65198
	//            // Renamed for clarity
	//            //WorksheetCellFormatData defaultFormat )
	//            WorksheetCellFormatData emptyFormat)
	//        {
	//            // If the high priority format has default data, return the low priority format
	//            if ( highPriorityFormat.HasSameData( emptyFormat ) )
	//                return lowPriorityFormat;

	//            // If the low priority format has default data, return the high priority format
	//            if ( lowPriorityFormat.HasSameData( emptyFormat ) )
	//                return highPriorityFormat;

	//            // Create a new cell format, initializing it with the lower priority format
	//            // MD 4/18/11 - TFS62026
	//            // It is faster to use the CloneInternal method.
	//            //WorksheetCellFormatData combinedFormat = new WorksheetCellFormatData( lowPriorityFormat );
	//            WorksheetCellFormatData combinedFormat = lowPriorityFormat.CloneInternal();

	//            // Selective copy the higher priority format values
	//            combinedFormat.SelectiveCopyValues( highPriorityFormat );

	//            #region Combine the fonts

	//            WorkbookFontProxy lowPriorityFontPxy = lowPriorityFormat.FontInternal;
	//            WorkbookFontProxy highPriorityFontPxy = highPriorityFormat.FontInternal;

	//            // MD 3/21/11 - TFS65198
	//            // We really want to see if the elements have any data set, so check HasEmptyValue instead of HasDefaultValue.
	//            // The default values could be set to non-empty values if the workbook was loaded from a file.
	//            //if ( !lowPriorityFontPxy.HasDefaultValue && !highPriorityFontPxy.HasDefaultValue )
	//            //{
	//            //    combinedFormat.Font.SetFontFormatting( WorkbookFontData.Combine( highPriorityFontPxy.Element, lowPriorityFontPxy.Element ) );
	//            //}
	//            //else
	//            //{
	//            //    if ( !highPriorityFontPxy.HasDefaultValue )
	//            //        combinedFormat.Font.SetFontFormatting( highPriorityFontPxy );
	//            //}
	//            bool highFontHasEmptyValue = highPriorityFontPxy.HasEmptyValue;
	//            if (!lowPriorityFontPxy.HasEmptyValue && !highFontHasEmptyValue)
	//            {
	//                combinedFormat.Font.SetFontFormatting(WorkbookFontData.Combine(highPriorityFontPxy.Element, lowPriorityFontPxy.Element));
	//            }
	//            else
	//            {
	//                if (!highFontHasEmptyValue)
	//                    combinedFormat.Font.SetFontFormatting(highPriorityFontPxy);
	//            }

	//            #endregion Combine the fonts

	//            return combinedFormat;
	//        }

	//        #endregion Combine

	//        // MD 11/29/11 - TFS96205
	//        #region GetRoundTripData

	//        internal ExtProp[] GetRoundTripData()
	//        {
	//            if (this.roundTripProps == null)
	//                return new ExtProp[0];

	//            ExtProp[] props = new ExtProp[this.roundTripProps.Count];
	//            this.roundTripProps.Values.CopyTo(props, 0);
	//            return props;
	//        }

	//        #endregion  // GetRoundTripData

	//        // MD 4/18/11 - TFS62026
	//        // Added a way to get any value with the CellFormatValue enum.
	//        #region GetValue

	//        public object GetValue(CellFormatValue valueToGet)
	//        {
	//            switch (valueToGet)
	//            {
	//                case CellFormatValue.Alignment:
	//                    return this.Alignment;

	//                case CellFormatValue.BottomBorderColor:
	//                    return this.BottomBorderColor;

	//                case CellFormatValue.BottomBorderStyle:
	//                    return this.BottomBorderStyle;

	//                // MD 10/26/11 - TFS91546
	//                case CellFormatValue.DiagonalBorderColor:
	//                    return this.DiagonalBorderColor;

	//                // MD 10/26/11 - TFS91546
	//                case CellFormatValue.DiagonalBorders:
	//                    return this.DiagonalBorders;

	//                // MD 10/26/11 - TFS91546
	//                case CellFormatValue.DiagonalBorderStyle:
	//                    return this.DiagonalBorderStyle;

	//                case CellFormatValue.FillPattern:
	//                    return this.FillPattern;

	//                case CellFormatValue.FillPatternBackgroundColor:
	//                    return this.FillPatternBackgroundColor;

	//                case CellFormatValue.FillPatternForegroundColor:
	//                    return this.FillPatternForegroundColor;

	//                // MD 10/13/10 - TFS43003
	//                case CellFormatValue.FontBold:
	//                    return this.Font.Bold;

	//                case CellFormatValue.FontColor:
	//                    return this.Font.Color;

	//                case CellFormatValue.FontHeight:
	//                    return this.Font.Height;

	//                case CellFormatValue.FontItalic:
	//                    return this.Font.Italic;

	//                case CellFormatValue.FontName:
	//                    return this.Font.Name;

	//                case CellFormatValue.FontStrikeout:
	//                    return this.Font.Strikeout;

	//                case CellFormatValue.FontSuperscriptSubscriptStyle:
	//                    return this.Font.SuperscriptSubscriptStyle;

	//                case CellFormatValue.FontUnderlineStyle:
	//                    return this.Font.UnderlineStyle;
	//                // ***************** End of TFS43003 Fix ********************

	//                case CellFormatValue.FormatString:
	//                    return this.FormatString;

	//                case CellFormatValue.Indent:
	//                    return this.Indent;

	//                case CellFormatValue.LeftBorderColor:
	//                    return this.LeftBorderColor;

	//                case CellFormatValue.LeftBorderStyle:
	//                    return this.LeftBorderStyle;

	//                case CellFormatValue.Locked:
	//                    return this.Locked;

	//                case CellFormatValue.RightBorderColor:
	//                    return this.RightBorderColor;

	//                case CellFormatValue.RightBorderStyle:
	//                    return this.RightBorderStyle;

	//                case CellFormatValue.Rotation:
	//                    return this.Rotation;

	//                case CellFormatValue.ShrinkToFit:
	//                    return this.ShrinkToFit;

	//                case CellFormatValue.Style:
	//                    return this.Style;

	//                case CellFormatValue.TopBorderColor:
	//                    return this.TopBorderColor;

	//                case CellFormatValue.TopBorderStyle:
	//                    return this.TopBorderStyle;

	//                case CellFormatValue.VerticalAlignment:
	//                    return this.VerticalAlignment;

	//                case CellFormatValue.WrapText:
	//                    return this.WrapText;

	//                default:
	//                    Utilities.DebugFail("Unknown format value: " + valueToGet);
	//                    return null;
	//            }
	//        }

	//        #endregion // GetValue

	//        #region ResolvedCellFormatData

	//        internal WorksheetCellFormatData ResolvedCellFormatData()
	//        {
	//            // MD 11/12/07 - BR27987
	//            // Called new overload with default parameter
	//            return this.ResolvedCellFormatData( true );
	//        }

	//        // MD 11/12/07 - BR27987
	//        // Added new overload to allow the caller to prevent the number formatting flag from being resolved
	//        // in the style options.
	//        internal WorksheetCellFormatData ResolvedCellFormatData( bool allowNumberFormattingFlag )
	//        {
	//            // MD 4/18/11 - TFS62026
	//            // It is faster to use the CloneInternal method.
	//            //WorksheetCellFormatData tmp = new WorksheetCellFormatData( this );
	//            WorksheetCellFormatData tmp = this.CloneInternal();

	//            if ( tmp.font.Element.HasSameData( this.Workbook.Fonts.DefaultElement ) == false )
	//                tmp.formatOptions |= StyleCellFormatOptions.UseFontFormatting;

	//            if ( tmp.formatStringIndex == -1 )
	//                tmp.formatStringIndex = 0;
	//            // MD 11/12/07 - BR27987
	//            // Only resovle the number formatting flag if it is allowed.
	//            //else
	//            else if ( allowNumberFormattingFlag )
	//                tmp.formatOptions |= StyleCellFormatOptions.UseNumberFormatting;

	//            if ( tmp.locked == ExcelDefaultableBoolean.Default )
	//                tmp.locked = ExcelDefaultableBoolean.True;
	//            else
	//                tmp.formatOptions |= StyleCellFormatOptions.UseProtectionFormatting;

	//            if ( tmp.wrapText == ExcelDefaultableBoolean.Default )
	//                tmp.wrapText = ExcelDefaultableBoolean.False;
	//            else
	//                tmp.formatOptions |= StyleCellFormatOptions.UseAlignmentFormatting;

	//            if ( tmp.shrinkToFit == ExcelDefaultableBoolean.Default )
	//                tmp.shrinkToFit = ExcelDefaultableBoolean.False;
	//            else
	//                tmp.formatOptions |= StyleCellFormatOptions.UseAlignmentFormatting;

	//            if ( tmp.alignment == HorizontalCellAlignment.Default )
	//                tmp.alignment = HorizontalCellAlignment.General;
	//            else
	//                tmp.formatOptions |= StyleCellFormatOptions.UseAlignmentFormatting;

	//            if ( tmp.verticalAlignment == VerticalCellAlignment.Default )
	//                tmp.verticalAlignment = VerticalCellAlignment.Bottom;
	//            else
	//                tmp.formatOptions |= StyleCellFormatOptions.UseAlignmentFormatting;

	//            if ( tmp.rotation == -1 )
	//                tmp.rotation = 0;
	//            else
	//                tmp.formatOptions |= StyleCellFormatOptions.UseAlignmentFormatting;

	//            if ( tmp.indent == -1 )
	//                tmp.indent = 0;
	//            else
	//                tmp.formatOptions |= StyleCellFormatOptions.UseAlignmentFormatting;

	//            // MD 10/26/11 - TFS91546
	//            // We had duplicate logic here for each border side. I have refactored them into a method called ResolveBorderInfo.
	//            #region Refactored

	//            //if ( tmp.leftBorderColorIndex == -1 || tmp.LeftBorderColor == Utilities.ColorEmpty )
	//            //{
	//            //    // MD 10/23/07 - BR27496
	//            //    // The color should only be set to black if there it is being used in a border, otherwise, 
	//            //    // 0 should be written out as the border color index.
	//            //    //tmp.LeftBorderColor = Color.Black;
	//            //    if ( tmp.leftBorderStyle == CellBorderLineStyle.None ||
	//            //        tmp.leftBorderStyle == CellBorderLineStyle.Default )
	//            //        tmp.LeftBorderColorIndex = 0;
	//            //    else
	//            //    {
	//            //        // MD 4/24/09
	//            //        // Found while fixing TFS16204
	//            //        // The default border color is WindowText, not Black
	//            //        //tmp.LeftBorderColor = Color.Black;
	//            //        tmp.LeftBorderColor = Utilities.SystemColorsWindowText;
	//            //    }
	//            //}
	//            //else
	//            //{
	//            //    tmp.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            //    if ( tmp.leftBorderStyle == CellBorderLineStyle.Default )
	//            //        tmp.leftBorderStyle = CellBorderLineStyle.Thin;
	//            //}

	//            //if (tmp.rightBorderColorIndex == -1 || tmp.RightBorderColor == Utilities.ColorEmpty)
	//            //{
	//            //    // MD 10/23/07 - BR27496
	//            //    // The color should only be set to black if there it is being used in a border, otherwise, 
	//            //    // 0 should be written out as the border color index.
	//            //    //tmp.RightBorderColor = Color.Black;
	//            //    if ( tmp.rightBorderStyle == CellBorderLineStyle.None ||
	//            //        tmp.rightBorderStyle == CellBorderLineStyle.Default )
	//            //        tmp.RightBorderColorIndex = 0;
	//            //    else
	//            //    {
	//            //        // MD 4/24/09
	//            //        // Found while fixing TFS16204
	//            //        // The default border color is WindowText, not Black
	//            //        //tmp.RightBorderColor = Color.Black;
	//            //        tmp.RightBorderColor = Utilities.SystemColorsWindowText;
	//            //    }
	//            //}
	//            //else
	//            //{
	//            //    tmp.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            //    if ( tmp.rightBorderStyle == CellBorderLineStyle.Default )
	//            //        tmp.rightBorderStyle = CellBorderLineStyle.Thin;
	//            //}

	//            //if (tmp.topBorderColorIndex == -1 || tmp.TopBorderColor == Utilities.ColorEmpty)
	//            //{
	//            //    // MD 10/23/07 - BR27496
	//            //    // The color should only be set to black if there it is being used in a border, otherwise, 
	//            //    // 0 should be written out as the border color index.
	//            //    //tmp.TopBorderColor = Color.Black;
	//            //    if ( tmp.topBorderStyle == CellBorderLineStyle.None ||
	//            //        tmp.topBorderStyle == CellBorderLineStyle.Default )
	//            //        tmp.TopBorderColorIndex = 0;
	//            //    else
	//            //    {
	//            //        // MD 4/24/09
	//            //        // Found while fixing TFS16204
	//            //        // The default border color is WindowText, not Black
	//            //        //tmp.TopBorderColor = Color.Black;
	//            //        tmp.TopBorderColor = Utilities.SystemColorsWindowText;
	//            //    }
	//            //}
	//            //else
	//            //{
	//            //    tmp.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            //    if ( tmp.topBorderStyle == CellBorderLineStyle.Default )
	//            //        tmp.topBorderStyle = CellBorderLineStyle.Thin;
	//            //}

	//            //if (tmp.bottomBorderColorIndex == -1 || tmp.BottomBorderColor == Utilities.ColorEmpty)
	//            //{
	//            //    // MD 10/23/07 - BR27496
	//            //    // The color should only be set to black if there it is being used in a border, otherwise, 
	//            //    // 0 should be written out as the border color index.
	//            //    //tmp.BottomBorderColor = Color.Black;
	//            //    if ( tmp.bottomBorderStyle == CellBorderLineStyle.None ||
	//            //        tmp.bottomBorderStyle == CellBorderLineStyle.Default )
	//            //        tmp.BottomBorderColorIndex = 0;
	//            //    else
	//            //    {
	//            //        // MD 4/24/09
	//            //        // Found while fixing TFS16204
	//            //        // The default border color is WindowText, not Black
	//            //        //tmp.BottomBorderColor = Color.Black;
	//            //        tmp.BottomBorderColor = Utilities.SystemColorsWindowText;
	//            //    }
	//            //}
	//            //else
	//            //{
	//            //    tmp.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            //    if ( tmp.bottomBorderStyle == CellBorderLineStyle.Default )
	//            //        tmp.bottomBorderStyle = CellBorderLineStyle.Thin;
	//            //}

	//            //if ( tmp.leftBorderStyle == CellBorderLineStyle.Default )
	//            //    tmp.leftBorderStyle = CellBorderLineStyle.None;
	//            //else
	//            //    tmp.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            //if ( tmp.rightBorderStyle == CellBorderLineStyle.Default )
	//            //    tmp.rightBorderStyle = CellBorderLineStyle.None;
	//            //else
	//            //    tmp.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            //if ( tmp.topBorderStyle == CellBorderLineStyle.Default )
	//            //    tmp.topBorderStyle = CellBorderLineStyle.None;
	//            //else
	//            //    tmp.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            //if ( tmp.bottomBorderStyle == CellBorderLineStyle.Default )
	//            //    tmp.bottomBorderStyle = CellBorderLineStyle.None;
	//            //else
	//            //    tmp.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            //// MD 4/24/09 - TFS16204
	//            //// If this is not a style format, 0 is not valid for border colors and prevents the format cells dialog 
	//            //// from appearing. Since 0 on the style format seems to indicate the default color, use the actual default 
	//            //// color index.
	//            //if ( tmp.style == false )
	//            //{
	//            //    if ( tmp.leftBorderColorIndex == 0 && tmp.leftBorderStyle != CellBorderLineStyle.None )
	//            //        tmp.leftBorderColorIndex = WorkbookColorCollection.AutomaticColor;

	//            //    if ( tmp.rightBorderColorIndex == 0 && tmp.rightBorderStyle != CellBorderLineStyle.None )
	//            //        tmp.rightBorderColorIndex = WorkbookColorCollection.AutomaticColor;

	//            //    if ( tmp.topBorderColorIndex == 0 && tmp.topBorderStyle != CellBorderLineStyle.None )
	//            //        tmp.topBorderColorIndex = WorkbookColorCollection.AutomaticColor;

	//            //    if ( tmp.bottomBorderColorIndex == 0 && tmp.bottomBorderStyle != CellBorderLineStyle.None )
	//            //        tmp.bottomBorderColorIndex = WorkbookColorCollection.AutomaticColor;
	//            //}

	//            #endregion  // Refactored
	//            tmp.ResolveBorderInfo(tmp.LeftBorderColor, CellFormatValue.LeftBorderColor, ref tmp.leftBorderColorIndex, ref tmp.leftBorderStyle, false);
	//            tmp.ResolveBorderInfo(tmp.RightBorderColor, CellFormatValue.RightBorderColor, ref tmp.rightBorderColorIndex, ref tmp.rightBorderStyle, false);
	//            tmp.ResolveBorderInfo(tmp.TopBorderColor, CellFormatValue.TopBorderColor, ref tmp.topBorderColorIndex, ref tmp.topBorderStyle, false);
	//            tmp.ResolveBorderInfo(tmp.BottomBorderColor, CellFormatValue.BottomBorderColor, ref tmp.bottomBorderColorIndex, ref tmp.bottomBorderStyle, false);
	//            tmp.ResolveBorderInfo(tmp.DiagonalBorderColor, CellFormatValue.DiagonalBorderColor, ref tmp.diagonalBorderColorIndex, ref tmp.diagonalBorderStyle, 
	//                tmp.diagonalBorders != DiagonalBorders.None);

	//            // MD 11/24/10 - TFS34598
	//            // This OR is redundant because when the index is -1, the returned color is empty anyway.
	//            // And now that we also store the RGB data, it causes a problem when we have RGB data but no index.
	//            //if (tmp.fillPatternBackgroundColorIndex == -1 || tmp.FillPatternBackgroundColor == Utilities.ColorEmpty)
	//            if (tmp.FillPatternBackgroundColor == Utilities.ColorEmpty)
	//            {
	//                // MD 10/23/07 - BR27496
	//                // The default background fill color is Window, not Black, but when there is no fill pattern, 
	//                // that window color must be written as index 0x41, which is never allowed for fill colors, 
	//                // so save the index manually.
	//                //tmp.FillPatternBackgroundColor = Color.Black;
	//                if ( tmp.fillPattern == FillPatternStyle.None ||
	//                    tmp.fillPattern == FillPatternStyle.Default )
	//                    tmp.FillPatternBackgroundColorIndex = 0x41; //SystemColors.Window
	//                else
	//                    tmp.FillPatternBackgroundColor = Utilities.SystemColorsWindow;
	//            }
	//            else
	//            {
	//                tmp.formatOptions |= StyleCellFormatOptions.UsePatternsFormatting;

	//                if ( tmp.fillPattern == FillPatternStyle.Default )
	//                    tmp.fillPattern = FillPatternStyle.Gray50percent;
	//            }

	//            // MD 11/24/10 - TFS34598
	//            // This OR is redundant because when the index is -1, the returned color is empty anyway.
	//            // And now that we also store the RGB data, it causes a problem when we have RGB data but no index.
	//            //if (tmp.fillPatternForegroundColorIndex == -1 || tmp.FillPatternForegroundColor == Utilities.ColorEmpty)
	//            if (tmp.FillPatternForegroundColor == Utilities.ColorEmpty)
	//            {
	//                // MD 10/23/07 - BR27496
	//                // The default foreground fill color is WindowText, not Black
	//                //tmp.FillPatternForegroundColor = Color.Black;
	//                tmp.FillPatternForegroundColor = Utilities.SystemColorsWindowText;
	//            }
	//            else
	//            {
	//                tmp.formatOptions |= StyleCellFormatOptions.UsePatternsFormatting;

	//                if ( tmp.fillPattern == FillPatternStyle.Default )
	//                    tmp.fillPattern = FillPatternStyle.Solid;
	//            }

	//            if ( tmp.fillPattern == FillPatternStyle.Default )
	//                tmp.fillPattern = 0;
	//            else
	//                tmp.formatOptions |= StyleCellFormatOptions.UsePatternsFormatting;

	//            // MD 10/26/11 - TFS91546
	//            // Due to the refactoring above, we may not clear the cachedHashCode when resolving the border values, so just clear it at 
	//            // the end here.
	//            tmp.cachedHashCode = 0;

	//            return tmp;
	//        }

	//        #endregion ResolvedCellFormatData

	//        // MD 11/29/11 - TFS96205
	//        #region SetRoundTripData

	//        internal void SetRoundTripData(ExtPropType propType, ExtProp propData)
	//        {
	//            if (this.roundTripProps == null)
	//                this.roundTripProps = new Dictionary<ExtPropType, ExtProp>();

	//            Debug.Assert(this.roundTripProps.ContainsKey(propType) == false, "This property should not exist yet.");
	//            this.roundTripProps[propType] = propData;
	//        }

	//        #endregion  // SetRoundTripData

	//        // MD 4/18/11 - TFS62026
	//        // Added a way to set any value with the CellFormatValue enum.
	//        #region SetValue

	//        public void SetValue(CellFormatValue valueToSet, object value)
	//        {
	//            switch (valueToSet)
	//            {
	//                case CellFormatValue.Alignment:
	//                    this.Alignment = (HorizontalCellAlignment)value;
	//                    break;

	//                case CellFormatValue.BottomBorderColor:
	//                    this.BottomBorderColor = (Color)value;
	//                    break;

	//                case CellFormatValue.BottomBorderStyle:
	//                    this.BottomBorderStyle = (CellBorderLineStyle)value;
	//                    break;

	//                // MD 10/26/11 - TFS91546
	//                case CellFormatValue.DiagonalBorderColor:
	//                    this.DiagonalBorderColor = (Color)value;
	//                    break;

	//                // MD 10/26/11 - TFS91546
	//                case CellFormatValue.DiagonalBorders:
	//                    this.DiagonalBorders = (DiagonalBorders)value;
	//                    break;

	//                // MD 10/26/11 - TFS91546
	//                case CellFormatValue.DiagonalBorderStyle:
	//                    this.DiagonalBorderStyle = (CellBorderLineStyle)value;
	//                    break;

	//                case CellFormatValue.FillPattern:
	//                    this.FillPattern = (FillPatternStyle)value;
	//                    break;

	//                case CellFormatValue.FillPatternBackgroundColor:
	//                    this.FillPatternBackgroundColor = (Color)value;
	//                    break;

	//                case CellFormatValue.FillPatternForegroundColor:
	//                    this.FillPatternForegroundColor = (Color)value;
	//                    break;

	//                case CellFormatValue.FontBold:
	//                    this.Font.Bold = (ExcelDefaultableBoolean)value;
	//                    break;

	//                case CellFormatValue.FontColor:
	//                    this.Font.Color = (Color)value;
	//                    break;

	//                case CellFormatValue.FontHeight:
	//                    this.Font.Height = (int)value;
	//                    break;

	//                case CellFormatValue.FontItalic:
	//                    this.Font.Italic = (ExcelDefaultableBoolean)value;
	//                    break;

	//                case CellFormatValue.FontName:
	//                    this.Font.Name = (string)value;
	//                    break;

	//                case CellFormatValue.FontStrikeout:
	//                    this.Font.Strikeout = (ExcelDefaultableBoolean)value;
	//                    break;

	//                case CellFormatValue.FontSuperscriptSubscriptStyle:
	//                    this.Font.SuperscriptSubscriptStyle = (FontSuperscriptSubscriptStyle)value;
	//                    break;

	//                case CellFormatValue.FontUnderlineStyle:
	//                    this.Font.UnderlineStyle = (FontUnderlineStyle)value;
	//                    break;

	//                case CellFormatValue.FormatString:
	//                    this.FormatString = (string)value;
	//                    break;

	//                case CellFormatValue.Indent:
	//                    this.Indent = (int)value;
	//                    break;

	//                case CellFormatValue.LeftBorderColor:
	//                    this.LeftBorderColor = (Color)value;
	//                    break;

	//                case CellFormatValue.LeftBorderStyle:
	//                    this.LeftBorderStyle = (CellBorderLineStyle)value;
	//                    break;

	//                case CellFormatValue.Locked:
	//                    this.Locked = (ExcelDefaultableBoolean)value;
	//                    break;

	//                case CellFormatValue.RightBorderColor:
	//                    this.RightBorderColor = (Color)value;
	//                    break;

	//                case CellFormatValue.RightBorderStyle:
	//                    this.RightBorderStyle = (CellBorderLineStyle)value;
	//                    break;

	//                case CellFormatValue.Rotation:
	//                    this.Rotation = (int)value;
	//                    break;

	//                case CellFormatValue.ShrinkToFit:
	//                    this.ShrinkToFit = (ExcelDefaultableBoolean)value;
	//                    break;

	//                case CellFormatValue.Style:
	//                    this.Style = (bool)value;
	//                    break;

	//                case CellFormatValue.TopBorderColor:
	//                    this.TopBorderColor = (Color)value;
	//                    break;

	//                case CellFormatValue.TopBorderStyle:
	//                    this.TopBorderStyle = (CellBorderLineStyle)value;
	//                    break;

	//                case CellFormatValue.VerticalAlignment:
	//                    this.VerticalAlignment = (VerticalCellAlignment)value;
	//                    break;

	//                case CellFormatValue.WrapText:
	//                    this.WrapText = (ExcelDefaultableBoolean)value;
	//                    break;

	//                default:
	//                    Utilities.DebugFail("Unknown format value: " + value);
	//                    break;
	//            }
	//        }  

	//        #endregion  // SetValue

	//        // MBS 7/15/08 - Excel 2007 Format
	//        #region VerifyFormatLimits

	//        public override void VerifyFormatLimits(FormatLimitErrors errors, WorkbookFormat testFormat)
	//        {
	//            base.VerifyFormatLimits(errors, testFormat);

	//            int maxIndent = WorksheetCellFormatData.GetMaxIndent(testFormat);
	//            if (maxIndent < this.Indent)
	//                errors.AddError(String.Format(SR.GetString("LE_FormatLimitError_Indent"), maxIndent));
	//        }
	//        #endregion //VerifyFormatLimits

	//        #endregion Internal Methods

	//        #region Private Method

	//        // MD 5/14/07 - BR22853
	//        #region ConvertColorForBorder

	//        private static void ConvertColorForBorder( ref Color color )
	//        {
	//            // WindowFrame is not supported for borders
	//            if ( color == Utilities.SystemColorsWindowFrame )
	//                color = Utilities.SystemColorsWindowText;
	//        }

	//        #endregion ConvertColorForBorder

	//        // MBS 7/15/08 - Excel 2007 Format
	//        #region GetMaxIndent

	//        private static int GetMaxIndent(WorkbookFormat format)
	//        {
	//            // MD 2/4/11
	//            // Done while fixing TFS65015
	//            // Use the new Utilities.Is2003Format method so we don't need to switch on the format all over the place.
	//            //switch (format)
	//            //{
	//            //    case WorkbookFormat.Excel97To2003:
	//            //    // MD 5/7/10 - 10.2 - Excel Templates
	//            //    case WorkbookFormat.Excel97To2003Template:
	//            //        return WorksheetCellFormatData.MaxIndent;
	//            //
	//            //    case WorkbookFormat.Excel2007:
	//            //    // MD 10/1/08 - TFS8471
	//            //    case WorkbookFormat.Excel2007MacroEnabled:
	//            //    // MD 5/7/10 - 10.2 - Excel Templates
	//            //	  case WorkbookFormat.Excel2007MacroEnabledTemplate:
	//            //	  case WorkbookFormat.Excel2007Template:
	//            //        return WorksheetCellFormatData.MaxIndent2007;
	//            //
	//            //    default:
	//            //        Utilities.DebugFail("Unknown workbook type: " + format.ToString());
	//            //        goto case WorkbookFormat.Excel97To2003;
	//            //}
	//            if (Utilities.Is2003Format(format))
	//                return WorksheetCellFormatData.MaxIndent;

	//            return WorksheetCellFormatData.MaxIndent2007;
	//        }
	//        #endregion //GetMaxIndent

	//        // MD 10/26/11 - TFS91546
	//        // Refactored some duplicate code from the ResolvedCellFormatData method
	//        #region ResolveBorderInfo

	//        private void ResolveBorderInfo(Color borderColor, CellFormatValue borderColorValue, ref int borderColorIndex, ref CellBorderLineStyle borderStyle, bool forceResolve)
	//        {
	//            if (borderColorIndex == -1 || borderColor == Utilities.ColorEmpty)
	//            {
	//                if (forceResolve == false && (borderStyle == CellBorderLineStyle.None || borderStyle == CellBorderLineStyle.Default))
	//                    borderColorIndex = 0;
	//                else
	//                    this.SetValue(borderColorValue, Utilities.SystemColorsWindowText);
	//            }
	//            else
	//            {
	//                this.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//                if (borderStyle == CellBorderLineStyle.Default)
	//                    borderStyle = CellBorderLineStyle.Thin;
	//            }

	//            if (forceResolve &&
	//                (borderStyle == CellBorderLineStyle.Default || borderStyle == CellBorderLineStyle.None))
	//            {
	//                borderStyle = CellBorderLineStyle.Thin;
	//            }

	//            if (borderStyle == CellBorderLineStyle.Default)
	//                borderStyle = CellBorderLineStyle.None;
	//            else
	//                this.formatOptions |= StyleCellFormatOptions.UseBorderFormatting;

	//            if (this.style == false && borderColorIndex <= 0 && borderStyle != CellBorderLineStyle.None)
	//                borderColorIndex = WorkbookColorCollection.AutomaticColor;
	//        }

	//        #endregion  // ResolveBorderInfo

	//        #region SelectiveCopyValues

	//#if DEBUG
	//        /// <summary>
	//        /// Copies all non-default data from the specified format to this format
	//        /// </summary> 
	//#endif
	//        internal void SelectiveCopyValues( WorksheetCellFormatData source )
	//        {
	//            if ( source.formatStringIndex != -1 )
	//                this.formatStringIndex = source.formatStringIndex;

	//            if ( source.locked != ExcelDefaultableBoolean.Default )
	//                this.locked = source.locked;

	//            if ( source.wrapText != ExcelDefaultableBoolean.Default )
	//                this.wrapText = source.wrapText;

	//            if ( source.shrinkToFit != ExcelDefaultableBoolean.Default )
	//                this.shrinkToFit = source.shrinkToFit;

	//            if ( source.alignment != HorizontalCellAlignment.Default )
	//                this.alignment = source.alignment;

	//            if ( source.verticalAlignment != VerticalCellAlignment.Default )
	//                this.verticalAlignment = source.verticalAlignment;

	//            if ( source.rotation != -1 )
	//                this.rotation = source.rotation;

	//            if ( source.indent != -1 )
	//                this.indent = source.indent;

	//            if ( source.leftBorderStyle != CellBorderLineStyle.Default )
	//                this.leftBorderStyle = source.leftBorderStyle;

	//            if ( source.rightBorderStyle != CellBorderLineStyle.Default )
	//                this.rightBorderStyle = source.rightBorderStyle;

	//            if ( source.topBorderStyle != CellBorderLineStyle.Default )
	//                this.topBorderStyle = source.topBorderStyle;

	//            if ( source.bottomBorderStyle != CellBorderLineStyle.Default )
	//                this.bottomBorderStyle = source.bottomBorderStyle;

	//            if ( source.leftBorderColorIndex != -1 )
	//                this.leftBorderColorIndex = source.leftBorderColorIndex;

	//            if ( source.rightBorderColorIndex != -1 )
	//                this.rightBorderColorIndex = source.rightBorderColorIndex;

	//            if ( source.topBorderColorIndex != -1 )
	//                this.topBorderColorIndex = source.topBorderColorIndex;

	//            if ( source.bottomBorderColorIndex != -1 )
	//                this.bottomBorderColorIndex = source.bottomBorderColorIndex;

	//            if ( source.fillPattern != FillPatternStyle.Default )
	//                this.fillPattern = source.fillPattern;

	//            if ( source.fillPatternForegroundColorIndex != -1 )
	//                this.fillPatternForegroundColorIndex = source.fillPatternForegroundColorIndex;

	//            if ( source.fillPatternBackgroundColorIndex != -1 )
	//                this.fillPatternBackgroundColorIndex = source.fillPatternBackgroundColorIndex;

	//            // MD 11/24/10 - TFS34598
	//            // Also copy the RGB data.
	//            if (Utilities.ColorIsEmpty(source.fillPatternForegroundColor) == false)
	//                this.fillPatternForegroundColor = source.fillPatternForegroundColor;

	//            if (Utilities.ColorIsEmpty(source.fillPatternBackgroundColor) == false)
	//                this.fillPatternBackgroundColor = source.fillPatternBackgroundColor;

	//            // MD 10/26/11 - TFS91546
	//            if (source.diagonalBorderColorIndex != -1)
	//                this.diagonalBorderColorIndex = source.diagonalBorderColorIndex;

	//            // MD 10/26/11 - TFS91546
	//            this.diagonalBorders = source.diagonalBorders;

	//            // MD 10/26/11 - TFS91546
	//            if (source.diagonalBorderStyle != CellBorderLineStyle.Default)
	//                this.diagonalBorderStyle = source.diagonalBorderStyle;

	//            // MD 7/2/09 - TFS18634
	//            this.cachedHashCode = 0;
	//        }

	//        #endregion SelectiveCopyValues

	//        #endregion Private Methods

	//        #endregion Methods

	//        #region Properties

	//        #region Public Properties

	//        #region Alignment

	//        public HorizontalCellAlignment Alignment
	//        {
	//            get { return this.alignment; }
	//            set 
	//            {
	//                if ( this.alignment != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( HorizontalCellAlignment ), value ) == false )
	//                    if (Utilities.IsHorizontalCellAlignmentDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( HorizontalCellAlignment ) );

	//                    this.alignment = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion Alignment

	//        #region BottomBorderColor

	//        public Color BottomBorderColor
	//        {
	//            get
	//            {
	//                return this.Workbook.Palette[ this.bottomBorderColorIndex ];
	//            }
	//            set
	//            {
	//                // MD 5/14/07 - BR22853
	//                // Some colors are not supported by borders
	//                WorksheetCellFormatData.ConvertColorForBorder( ref value );

	//                // MD 8/24/07 - BR25848
	//                // Another parameter is now needed for the Add method
	//                //this.bottomBorderColorIndex = this.Workbook.Palette.Add( value );
	//                // MD 8/30/07 - BR26111
	//                // Changed parameter to provide more info about the item getting a color
	//                //this.bottomBorderColorIndex = this.Workbook.Palette.Add( value, true );
	//#if SILVERLIGHT
	//                // GT 8/25/10 - Comparison of the colors in Silverlight doesn't include the name, so we shouldn't change the index
	//                // PP 6/14/11 - The color index remains to its old value and this cause a dead loop in set/reset color to default value
	//                // if (this.bottomBorderColorIndex < 0)
	//#endif
	//                this.bottomBorderColorIndex = this.Workbook.Palette.Add(value, ColorableItem.CellBorder);

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion BottomBorderColor

	//        #region BottomBorderStyle

	//        public CellBorderLineStyle BottomBorderStyle
	//        {
	//            get { return this.bottomBorderStyle; }
	//            set
	//            {
	//                if ( this.bottomBorderStyle != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( CellBorderLineStyle ), value ) == false )
	//                    if (Utilities.IsCellBorderLineStyleDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( CellBorderLineStyle ) );

	//                    this.bottomBorderStyle = value;
	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion BottomBorderStyle

	//        // MD 10/26/11 - TFS91546
	//        #region DiagonalBorderColor

	//        public Color DiagonalBorderColor
	//        {
	//            get
	//            {
	//                return this.Workbook.Palette[this.diagonalBorderColorIndex];
	//            }
	//            set
	//            {
	//                WorksheetCellFormatData.ConvertColorForBorder(ref value);
	//                this.diagonalBorderColorIndex = this.Workbook.Palette.Add(value, ColorableItem.CellBorder);
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion  // DiagonalBorderColor

	//        // MD 10/26/11 - TFS91546
	//        #region DiagonalBorders

	//        public DiagonalBorders DiagonalBorders
	//        {
	//            get { return this.diagonalBorders; }
	//            set
	//            {
	//                if (this.diagonalBorders != value)
	//                {
	//                    if (Enum.IsDefined(typeof(DiagonalBorders), value) == false)
	//                        throw new InvalidEnumArgumentException("value", (int)value, typeof(DiagonalBorders));

	//                    this.previousDiagonalBorders = this.diagonalBorders;
	//                    this.diagonalBorders = value;
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion  // DiagonalBorders

	//        // MD 10/26/11 - TFS91546
	//        #region DiagonalBorderStyle

	//        public CellBorderLineStyle DiagonalBorderStyle
	//        {
	//            get { return this.diagonalBorderStyle; }
	//            set
	//            {
	//                if (this.diagonalBorderStyle != value)
	//                {
	//                    if (Utilities.IsCellBorderLineStyleDefined(value) == false)
	//                        throw new InvalidEnumArgumentException("value", (int)value, typeof(CellBorderLineStyle));

	//                    this.diagonalBorderStyle = value;
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion  // DiagonalBorderStyle

	//        #region FillPattern

	//        public FillPatternStyle FillPattern
	//        {
	//            get { return this.fillPattern; }
	//            set
	//            {
	//                if ( this.fillPattern != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( FillPatternStyle ), value ) == false )
	//                    if (Utilities.IsFillPatternStyleDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( FillPatternStyle ) );

	//                    this.fillPattern = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion FillPattern

	//        #region FillPatternBackgroundColor

	//        public Color FillPatternBackgroundColor
	//        {
	//            get
	//            {
	//                // MD 11/24/10 - TFS34598
	//                // If we can use the RGB data and it is set, use it.
	//                if (Utilities.IsRGBColorFormatSupported(this.Workbook.CurrentFormat) &&
	//                    Utilities.ColorIsEmpty(this.fillPatternBackgroundColor) == false)
	//                {
	//                    return this.fillPatternBackgroundColor;
	//                }

	//                return this.Workbook.Palette[ this.fillPatternBackgroundColorIndex ];
	//            }
	//            set
	//            {
	//                // MD 11/24/10 - TFS34598
	//                // If we can use the RGB data and the color is not a system color, use that instead of indexed color values.
	//                //// MD 8/24/07 - BR25848
	//                //// Another parameter is now needed for the Add method
	//                ////this.fillPatternBackgroundColorIndex = this.Workbook.Palette.Add( value );
	//                //// MD 8/30/07 - BR26111
	//                //// Changed parameter to provide more info about the item getting a color
	//                ////this.fillPatternBackgroundColorIndex = this.Workbook.Palette.Add( value, false );
	//                //this.fillPatternBackgroundColorIndex = this.Workbook.Palette.Add( value, ColorableItem.CellFill );
	//                if (Utilities.IsRGBColorFormatSupported(this.Workbook.CurrentFormat) &&
	//                    Utilities.ColorIsSystem(value) == false)
	//                {
	//                    this.fillPatternBackgroundColor = value;
	//                    this.fillPatternBackgroundColorIndex = -1;
	//                }
	//                else
	//                {
	//                    this.fillPatternBackgroundColor = Utilities.ColorEmpty;
	//                    this.fillPatternBackgroundColorIndex = this.Workbook.Palette.Add(value, ColorableItem.CellFill);
	//                }

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion FillPatternBackgroundColor

	//        #region FillPatternForegroundColor

	//        public Color FillPatternForegroundColor
	//        {
	//            get
	//            {
	//                // MD 11/24/10 - TFS34598
	//                // If we can use the RGB data and it is set, use it.
	//                if (Utilities.IsRGBColorFormatSupported(this.Workbook.CurrentFormat) &&
	//                    Utilities.ColorIsEmpty(this.fillPatternForegroundColor) == false)
	//                {
	//                    return this.fillPatternForegroundColor;
	//                }

	//                return this.Workbook.Palette[ this.fillPatternForegroundColorIndex ];
	//            }
	//            set
	//            {
	//                // MD 11/24/10 - TFS34598
	//                // If we can use the RGB data and the color is not a system color, use that instead of indexed color values.
	//                //// MD 8/24/07 - BR25848
	//                //// Another parameter is now needed for the Add method
	//                ////this.fillPatternForegroundColorIndex = this.Workbook.Palette.Add( value );
	//                //// MD 8/30/07 - BR26111
	//                //// Changed parameter to provide more info about the item getting a color
	//                ////this.fillPatternForegroundColorIndex = this.Workbook.Palette.Add( value, false );
	//                //this.fillPatternForegroundColorIndex = this.Workbook.Palette.Add( value, ColorableItem.CellFill );
	//                if (Utilities.IsRGBColorFormatSupported(this.Workbook.CurrentFormat) &&
	//                    Utilities.ColorIsSystem(value) == false)
	//                {
	//                    this.fillPatternForegroundColor = value;
	//                    this.fillPatternForegroundColorIndex = -1;
	//                }
	//                else
	//                {
	//                    this.fillPatternForegroundColor = Utilities.ColorEmpty;
	//                    this.fillPatternForegroundColorIndex = this.Workbook.Palette.Add(value, ColorableItem.CellFill);
	//                }

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion FillPatternForegroundColor

	//        #region Font

	//        public IWorkbookFont Font
	//        {
	//            get { return this.FontInternal; }
	//        }

	//        #endregion Font

	//        #region FormatString

	//        public string FormatString
	//        {
	//            get
	//            {
	//                return this.Workbook.Formats[ this.formatStringIndex ];
	//            }
	//            set
	//            {
	//                if ( value != null )
	//                    this.formatStringIndex = this.Workbook.Formats.Add( value );
	//                else
	//                    this.formatStringIndex = -1;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion FormatString

	//        #region Indent

	//        public int Indent
	//        {
	//            get { return this.indent; }
	//            set 
	//            {
	//                if ( this.indent != value )
	//                {
	//                    // MBS 7/15/08 - Excel 2007 Functionality
	//                    //if ( value < 0 || 15 < value )
	//                    int maxIndent = WorksheetCellFormatData.GetMaxIndent( this.Workbook != null ? this.Workbook.CurrentFormat : WorkbookFormat.Excel97To2003 );
	//                    if(value < 0 || maxIndent < value)
	//                        throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Indent" ) );

	//                    this.indent = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion Indent

	//        #region LeftBorderColor

	//        public Color LeftBorderColor
	//        {
	//            get
	//            {
	//                return this.Workbook.Palette[ this.leftBorderColorIndex ];
	//            }
	//            set
	//            {
	//                // MD 5/14/07 - BR22853
	//                // Some colors are not supported by borders
	//                WorksheetCellFormatData.ConvertColorForBorder( ref value );

	//                // MD 8/24/07 - BR25848
	//                // Another parameter is now needed for the Add method
	//                //this.leftBorderColorIndex = this.Workbook.Palette.Add( value );
	//                // MD 8/30/07 - BR26111
	//                // Changed parameter to provide more info about the item getting a color
	//                //this.leftBorderColorIndex = this.Workbook.Palette.Add( value, true );
	//#if SILVERLIGHT
	//                // GT 8/25/10 - Comparison of the colors in Silverlight doesn't include the name, so we shouldn't change the index
	//                // PP 6/14/11 - The color index remains to its old value and this cause a dead loop in set/reset color to default value
	//                // if (this.leftBorderColorIndex < 0)
	//#endif
	//                this.leftBorderColorIndex = this.Workbook.Palette.Add( value, ColorableItem.CellBorder );

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion LeftBorderColor

	//        #region LeftBorderStyle

	//        public CellBorderLineStyle LeftBorderStyle
	//        {
	//            get { return this.leftBorderStyle; }
	//            set
	//            {
	//                if ( this.leftBorderStyle != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( CellBorderLineStyle ), value ) == false )
	//                    if (Utilities.IsCellBorderLineStyleDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( CellBorderLineStyle ) );

	//                    this.leftBorderStyle = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion LeftBorderStyle

	//        #region Locked

	//        public ExcelDefaultableBoolean Locked
	//        {
	//            get { return this.locked; }
	//            set
	//            {
	//                if ( this.locked != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
	//                    if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

	//                    this.locked = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion Locked

	//        #region RightBorderColor

	//        public Color RightBorderColor
	//        {
	//            get
	//            {
	//                return this.Workbook.Palette[ this.rightBorderColorIndex ];
	//            }
	//            set
	//            {
	//                // MD 5/14/07 - BR22853
	//                // Some colors are not supported by borders
	//                WorksheetCellFormatData.ConvertColorForBorder( ref value );

	//                // MD 8/24/07 - BR25848
	//                // Another parameter is now needed for the Add method
	//                //this.rightBorderColorIndex = this.Workbook.Palette.Add( value );
	//                // MD 8/30/07 - BR26111
	//                // Changed parameter to provide more info about the item getting a color
	//                //this.rightBorderColorIndex = this.Workbook.Palette.Add( value, true );
	//#if SILVERLIGHT
	//                // GT 8/25/10 - Comparison of the colors in Silverlight doesn't include the name, so we shouldn't change the index
	//                // PP 6/14/11 - The color index remains to its old value and this cause a dead loop in set/reset color to default value
	//                // if (this.rightBorderColorIndex < 0)
	//#endif
	//                this.rightBorderColorIndex = this.Workbook.Palette.Add( value, ColorableItem.CellBorder );

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion RightBorderColor

	//        #region RightBorderStyle

	//        public CellBorderLineStyle RightBorderStyle
	//        {
	//            get { return this.rightBorderStyle; }
	//            set
	//            {
	//                if ( this.rightBorderStyle != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( CellBorderLineStyle ), value ) == false )
	//                    if (Utilities.IsCellBorderLineStyleDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( CellBorderLineStyle ) );

	//                    this.rightBorderStyle = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion RightBorderStyle

	//        #region Rotation

	//        public int Rotation
	//        {
	//            get { return this.rotation; }
	//            set
	//            {
	//                this.rotation = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion Rotation

	//        #region ShrinkToFit

	//        public ExcelDefaultableBoolean ShrinkToFit
	//        {
	//            get { return this.shrinkToFit; }
	//            set
	//            {
	//                if ( this.shrinkToFit != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
	//                    if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

	//                    this.shrinkToFit = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion ShrinkToFit

	//        #region TopBorderColor

	//        public Color TopBorderColor
	//        {
	//            get
	//            {
	//                return this.Workbook.Palette[ this.topBorderColorIndex ];
	//            }
	//            set
	//            {
	//                // MD 5/14/07 - BR22853
	//                // Some colors are not supported by borders
	//                WorksheetCellFormatData.ConvertColorForBorder( ref value );

	//                // MD 8/24/07 - BR25848
	//                // Another parameter is now needed for the Add method
	//                //this.topBorderColorIndex = this.Workbook.Palette.Add( value );
	//                // MD 8/30/07 - BR26111
	//                // Changed parameter to provide more info about the item getting a color
	//                //this.topBorderColorIndex = this.Workbook.Palette.Add( value, true );
	//#if SILVERLIGHT
	//                // GT 8/25/10 - Comparison of the colors in Silverlight doesn't include the name, so we shouldn't change the index
	//                // PP 6/14/11 - The color index remains to its old value and this cause a dead loop in set/reset color to default value
	//                // if (this.topBorderColorIndex < 0)
	//#endif
	//                this.topBorderColorIndex = this.Workbook.Palette.Add( value, ColorableItem.CellBorder );

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion TopBorderColor

	//        #region TopBorderStyle

	//        public CellBorderLineStyle TopBorderStyle
	//        {
	//            get { return this.topBorderStyle; }
	//            set
	//            {
	//                if ( this.topBorderStyle != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( CellBorderLineStyle ), value ) == false )
	//                    if (Utilities.IsCellBorderLineStyleDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( CellBorderLineStyle ) );

	//                    this.topBorderStyle = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion TopBorderStyle

	//        #region VerticalAlignment

	//        public VerticalCellAlignment VerticalAlignment
	//        {
	//            get { return this.verticalAlignment; }
	//            set
	//            {
	//                if ( this.verticalAlignment != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( VerticalCellAlignment ), value ) == false )
	//                    if (Utilities.IsVerticalCellAlignmentDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( VerticalCellAlignment ) );

	//                    this.verticalAlignment = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion VerticalAlignment

	//        #region WrapText

	//        public ExcelDefaultableBoolean WrapText
	//        {
	//            get { return this.wrapText; }
	//            set
	//            {
	//                if ( this.wrapText != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
	//                    if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

	//                    this.wrapText = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion WrapText

	//        #endregion Public Properties

	//        #region Internal Properties

	//        #region BottomBorderColorIndex

	//        internal int BottomBorderColorIndex
	//        {
	//            get { return this.bottomBorderColorIndex; }
	//            set 
	//            {
	//                this.bottomBorderColorIndex = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion BottomBorderColorIndex

	//        // MD 10/26/11 - TFS91546
	//        #region DiagonalBorderColorIndex

	//        internal int DiagonalBorderColorIndex
	//        {
	//            get { return this.diagonalBorderColorIndex; }
	//            set
	//            {
	//                this.diagonalBorderColorIndex = value;
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion DiagonalBorderColorIndex

	//        #region FillPatternBackgroundColorIndex

	//        internal int FillPatternBackgroundColorIndex
	//        {
	//            get { return this.fillPatternBackgroundColorIndex; }
	//            set 
	//            {
	//                this.fillPatternBackgroundColorIndex = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion FillPatternBackgroundColorIndex

	//        #region FillPatternForegroundColorIndex

	//        internal int FillPatternForegroundColorIndex
	//        {
	//            get { return this.fillPatternForegroundColorIndex; }
	//            set
	//            {
	//                this.fillPatternForegroundColorIndex = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion FillPatternForegroundColorIndex

	//        // MD 11/24/10 - TFS34598
	//        #region FillPatternBackgroundColorInternal

	//        internal Color FillPatternBackgroundColorInternal
	//        {
	//            get { return this.fillPatternBackgroundColor; }
	//        }

	//        #endregion FillPatternBackgroundColorInternal

	//        // MD 11/24/10 - TFS34598
	//        #region FillPatternForegroundColorInternal

	//        internal Color FillPatternForegroundColorInternal
	//        {
	//            get { return this.fillPatternForegroundColor; }
	//        }

	//        #endregion FillPatternForegroundColorInternal

	//        #region FontInternal

	//        internal WorkbookFontProxy FontInternal
	//        {
	//            get { return this.font; }
	//        }

	//        #endregion FontInternal

	//        #region FormatStringIndex

	//        internal int FormatStringIndex
	//        {
	//            get { return this.formatStringIndex; }
	//            set 
	//            {
	//                this.formatStringIndex = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion FormatStringIndex

	//        // MD 11/29/11 - TFS96205
	//        #region HasRoundTripData

	//        internal bool HasRoundTripData
	//        {
	//            get
	//            {
	//                return this.roundTripProps != null && this.roundTripProps.Count != 0;
	//            }
	//        }

	//        #endregion  // HasRoundTripData

	//        #region LeftBorderColorIndex

	//        internal int LeftBorderColorIndex
	//        {
	//            get { return this.leftBorderColorIndex; }
	//            set 
	//            {
	//                this.leftBorderColorIndex = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion LeftBorderColorIndex

	//        // MD 10/26/11 - TFS91546
	//        #region PreviousDiagonalBorders

	//        internal DiagonalBorders PreviousDiagonalBorders
	//        {
	//            get { return this.previousDiagonalBorders; }
	//        }

	//        #endregion  // PreviousDiagonalBorders

	//        #region RightBorderColorIndex

	//        internal int RightBorderColorIndex
	//        {
	//            get { return this.rightBorderColorIndex; }
	//            set
	//            {
	//                this.rightBorderColorIndex = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion RightBorderColorIndex

	//        #region Style

	//        internal bool Style
	//        {
	//            get { return this.style; }
	//            set
	//            {
	//                this.style = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion Style

	//        #region FormatOptions

	//        internal StyleCellFormatOptions FormatOptions
	//        {
	//            get { return this.formatOptions; }
	//            set
	//            {
	//                this.formatOptions = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion FormatOptions

	//        #region IndexInFormatCollection

	//        internal int IndexInFormatCollection
	//        {
	//            get { return this.indexInFormatCollection; }
	//            set
	//            {
	//                this.indexInFormatCollection = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion IndexInFormatCollection

	//        #region IndexInXfsCollection

	//        internal int IndexInXfsCollection
	//        {
	//            get { return this.indexInXfsCollection; }
	//            set 
	//            {
	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;

	//                this.indexInXfsCollection = value; 
	//            }
	//        }

	//        #endregion IndexInXfsCollection

	//        #region TopBorderColorIndex

	//        internal int TopBorderColorIndex
	//        {
	//            get { return this.topBorderColorIndex; }
	//            set 
	//            {
	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;

	//                this.topBorderColorIndex = value; 
	//            }
	//        }

	//        #endregion TopBorderColorIndex

	//        #endregion Internal Properties        

	//        #endregion Properties
	//    }

	#endregion // Old Code
	// MD 2/2/12 - TFS100573
	// The workbook reference has been moved to the GenericCacheElementEx base type.
	//internal sealed class WorksheetCellFormatData : GenericCacheElement,
	internal sealed class WorksheetCellFormatData : GenericCacheElementEx,
		IWorkbookFontDefaultsResolver,
		IWorksheetCellFormat
	{
		#region Constants

		internal const string DefaultFormatString = "General";

		internal const int MaxIndent2003 = 15;
		internal const int MaxIndent2007 = 250;

		#endregion // Constants

		#region Static Members

		[ThreadStatic]
		private static IList<CellFormatValue> _allCellFormatValues;

		private static readonly CellFormatValue[] AlignmentFormattingProperties = new CellFormatValue[] 
		{
			CellFormatValue.Alignment,
			CellFormatValue.Indent,
			CellFormatValue.Rotation,
			CellFormatValue.ShrinkToFit,
			CellFormatValue.VerticalAlignment,
			CellFormatValue.WrapText,
		};

		private static readonly CellFormatValue[] BorderFormattingProperties = new CellFormatValue[] 
		{
			CellFormatValue.BottomBorderColorInfo,
			CellFormatValue.BottomBorderStyle,
			CellFormatValue.DiagonalBorderColorInfo,
			CellFormatValue.DiagonalBorders,
			CellFormatValue.DiagonalBorderStyle,
			CellFormatValue.LeftBorderColorInfo,
			CellFormatValue.LeftBorderStyle,
			CellFormatValue.RightBorderColorInfo,
			CellFormatValue.RightBorderStyle,
			CellFormatValue.TopBorderColorInfo,
			CellFormatValue.TopBorderStyle,
		};

		private static readonly CellFormatValue[] FillFormattingProperties = new CellFormatValue[] 
		{
			CellFormatValue.Fill,
		};

		private static readonly CellFormatValue[] FontFormattingProperties = new CellFormatValue[] 
		{
			CellFormatValue.FontBold,
			CellFormatValue.FontColorInfo,
			CellFormatValue.FontHeight,
			CellFormatValue.FontItalic,
			CellFormatValue.FontName,
			CellFormatValue.FontStrikeout,
			CellFormatValue.FontSuperscriptSubscriptStyle,
			CellFormatValue.FontUnderlineStyle,
		};

		private static readonly CellFormatValue[] NumberFormattingProperties = new CellFormatValue[] 
		{
			CellFormatValue.FormatString,
		};

		private static readonly CellFormatValue[] ProtectionFormattingProperties = new CellFormatValue[] 
		{
			CellFormatValue.Locked,
		};

		#endregion // Static Members

		#region Private Members

		// MD 3/21/12 - TFS104630
		// We need to round-trip the AddIndent value.
		private bool _addIndent;

		private HorizontalCellAlignment _alignment;
		private WorkbookColorInfo _bottomBorderColorInfo;
		private CellBorderLineStyle _bottomBorderStyle;
		private int? _cachedHashCode;
		private WorksheetCellFormatData _cachedStyleValues;
		private WorkbookColorInfo _diagonalBorderColorInfo;
		private DiagonalBorders _diagonalBorders;
		private CellBorderLineStyle _diagonalBorderStyle;
		private CellFill _fill;
		private WorkbookFontProxy _font;
		private WorksheetCellFormatOptions _formatOptions;
		private FontScheme _fontScheme = FontScheme.Nil;
		private int _formatStringIndex;
		private int _indent;
		private bool _isForFillOrSortCondition;
		private WorkbookColorInfo _leftBorderColorInfo;
		private CellBorderLineStyle _leftBorderStyle;
		private ExcelDefaultableBoolean _locked;
		private DiagonalBorders _previousDiagonalBorders;
		private WorkbookColorInfo _rightBorderColorInfo;
		private CellBorderLineStyle _rightBorderStyle;
		private int _rotation;
		private ExcelDefaultableBoolean _shrinkToFit;
		private WorkbookStyle _style;
		private WorkbookColorInfo _topBorderColorInfo;
		private CellBorderLineStyle _topBorderStyle;
		private WorksheetCellFormatType _type;
		private CellFormatValueChangeCallback _valueChangeCallback;
		private VerticalCellAlignment _verticalAlignment;
		private ExcelDefaultableBoolean _wrapText;

		#endregion Private Members

		#region Constructors

		public WorksheetCellFormatData(Workbook workbook, WorksheetCellFormatType type)
			: base(workbook)
		{
			_type = type;
			this.ResetFormatOptions(WorksheetCellFormatOptions.All);
		}

		#endregion Constructors

		#region Interfaces

		#region IWorkbookFontDefaultsResolver Members

		void IWorkbookFontDefaultsResolver.ResolveDefaults(WorkbookFontData font)
		{
			if (font.Bold == ExcelDefaultableBoolean.Default)
				font.Bold = this.FontBoldResolved;

			if (font.ColorInfo == null)
				font.ColorInfo = this.FontColorInfoResolved;

			font.ColorInfo = this.VerifyFontColorInfoForSave(font.ColorInfo);

			if (font.Height < 0)
				font.Height = this.FontHeightResolved;

			if (font.Italic == ExcelDefaultableBoolean.Default)
				font.Italic = this.FontItalicResolved;

			if (font.Name == null)
				font.Name = this.FontNameResolved;

			if (font.Strikeout == ExcelDefaultableBoolean.Default)
				font.Strikeout = this.FontStrikeoutResolved;

			if (font.SuperscriptSubscriptStyle == FontSuperscriptSubscriptStyle.Default)
				font.SuperscriptSubscriptStyle = this.FontSuperscriptSubscriptStyleResolved;

			if (font.UnderlineStyle == FontUnderlineStyle.Default)
				font.UnderlineStyle = this.FontUnderlineStyleResolved;
		}

		#endregion

		#region IWorksheetCellFormat Members

		Color IWorksheetCellFormat.BottomBorderColor
		{
			get
			{
				return this.GetColor(this.BottomBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.BottomBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		Color IWorksheetCellFormat.DiagonalBorderColor
		{
			get
			{
				return this.GetColor(this.DiagonalBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.DiagonalBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		FillPatternStyle IWorksheetCellFormat.FillPattern
		{
			get
			{
				return this.GetFillPattern(this.Fill);
			}
			set
			{
				this.Fill = this.UpdatedFillPattern(this.FillResolved, value);
			}
		}

		Color IWorksheetCellFormat.FillPatternBackgroundColor
		{
			get
			{
				return this.GetColor(this.GetFileFormatFillPatternColor(this.Fill, true, false));
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.Fill = this.UpdatedFillPatternColor(this.FillResolved, value, true);
			}
		}

		Color IWorksheetCellFormat.FillPatternForegroundColor
		{
			get
			{
				return this.GetColor(this.GetFileFormatFillPatternColor(this.Fill, false, false));
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.Fill = this.UpdatedFillPatternColor(this.FillResolved, value, false);
			}
		}

		Color IWorksheetCellFormat.LeftBorderColor
		{
			get
			{
				return this.GetColor(this.LeftBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.LeftBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		Color IWorksheetCellFormat.RightBorderColor
		{
			get
			{
				return this.GetColor(this.RightBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.RightBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		Color IWorksheetCellFormat.TopBorderColor
		{
			get
			{
				return this.GetColor(this.TopBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.TopBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		#endregion // IWorksheetCellFormat Members

		#endregion // Interfaces

		#region Base Class Overrides

		#region Clone

		public override object Clone(Workbook workbook)
		{
			return this.CloneInternal(workbook);
		}

		// MD 4/18/11 - TFS62026
		public WorksheetCellFormatData CloneInternal()
		{
			return this.CloneInternal(this.Workbook);
		}

		public WorksheetCellFormatData CloneInternal(Workbook workbook)
		{
			if (workbook == null)
				workbook = this.Workbook;

			WorksheetCellFormatData clone = (WorksheetCellFormatData)this.MemberwiseClone();

			clone.IsFrozen = false;

			if (_cachedStyleValues != null)
				clone._cachedStyleValues = _cachedStyleValues.CloneInternal(workbook);

			if (workbook != null && _style != null)
			{
				if (workbook != clone._style.Workbook)
					clone._style = workbook.Styles[_style.Name];
			}
			else
			{
				clone._style = null;
			}

			WorkbookFontData fontElement = _font.Element;
			if (fontElement.Workbook != workbook  || fontElement.IsFrozen)
				fontElement = fontElement.CloneInternal(workbook);

			clone._font = new WorkbookFontProxy(fontElement, workbook, clone);

			clone.ResetReferenceCount();

			// Set this last because there are side effects when it is set.
			clone.Collection = null;
			clone.Workbook = workbook;

			return clone;
		}

		#endregion Clone

		#region CopyValues

		public override void CopyValues(GenericCacheElement element)
		{
			WorksheetCellFormatData other = element as WorksheetCellFormatData;
			if (other == null)
			{
				Utilities.DebugFail("The specified element is not of the right type.");
				return;
			}
			this.SetFormatting(other);
		}

		#endregion // CopyValues

		#region Equals

		public override bool Equals(object obj)
		{
			if (this == obj)
				return true;

			WorksheetCellFormatData otherFormat = obj as WorksheetCellFormatData;

			if (otherFormat == null)
				return false;

			return this.EqualsInternal(otherFormat);
		}

		internal bool EqualsInternal(WorksheetCellFormatData otherFormat)
		{
			// MD 1/9/12 - 12.1 - Cell Format Updates
			// This is just a performance optimization.
			if (Object.ReferenceEquals(this, otherFormat))
				return true;

			// IsStyle is not considered in HasSameData, because they do not affect the appearance of the format,
			// so check them here
			if (_type != otherFormat._type)
				return false;

			return this.HasSameData(otherFormat);
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			if (_cachedHashCode.HasValue == false)
			{
				int tmp = Convert.ToInt32(_type);

				if (_font.Element != null)
					tmp ^= _font.Element.GetHashCode() << 1;

				tmp ^= _formatStringIndex << 2;
				tmp ^= (int)_locked << 3;
				tmp ^= (int)_wrapText << 4;
				tmp ^= (int)_shrinkToFit << 5;
				tmp ^= (int)_alignment << 6;
				tmp ^= (int)_verticalAlignment << 7;
				tmp ^= _rotation << 8;
				tmp ^= _indent << 9;
				tmp ^= (int)_leftBorderStyle << 10;
				tmp ^= (int)_rightBorderStyle << 11;
				tmp ^= (int)_topBorderStyle << 12;
				tmp ^= (int)_bottomBorderStyle << 13;

				if (_leftBorderColorInfo != null)
					tmp ^= _leftBorderColorInfo.GetHashCode() << 14;

				if (_rightBorderColorInfo != null)
					tmp ^= _rightBorderColorInfo.GetHashCode() << 15;

				if (_topBorderColorInfo != null)
					tmp ^= _topBorderColorInfo.GetHashCode() << 16;

				if (_bottomBorderColorInfo != null)
					tmp ^= _bottomBorderColorInfo.GetHashCode() << 17;

				if (_fill != null)
					tmp ^= _fill.GetHashCode() << 18;

				tmp ^= (int)_formatOptions << 19;

				if (_diagonalBorderColorInfo != null)
					tmp ^= _diagonalBorderColorInfo.GetHashCode() << 20;

				tmp ^= (int)_diagonalBorders << 21;
				tmp ^= (int)_diagonalBorderStyle << 22;

				// MD 3/21/12 - TFS104630
				if (_addIndent)
					tmp ^= 1 << 23;

				if (_style != null)
					tmp ^= _style.GetHashCode();

				_cachedHashCode = tmp;
			}

			return _cachedHashCode.Value;
		}

		#endregion GetHashCode

		#region HasSameData

		public override bool HasSameData(GenericCacheElement otherElement)
		{
			if (this == otherElement)
				return true;

			WorksheetCellFormatData otherFormat = (WorksheetCellFormatData)otherElement;

			if (_formatStringIndex != otherFormat._formatStringIndex)
				return false;

			if (_locked != otherFormat._locked)
				return false;

			if (_wrapText != otherFormat._wrapText)
				return false;

			if (_shrinkToFit != otherFormat._shrinkToFit)
				return false;

			if (_alignment != otherFormat._alignment)
				return false;

			if (_verticalAlignment != otherFormat._verticalAlignment)
				return false;

			if (_rotation != otherFormat._rotation)
				return false;

			if (_indent != otherFormat._indent)
				return false;

			if (_leftBorderStyle != otherFormat._leftBorderStyle)
				return false;

			if (_rightBorderStyle != otherFormat._rightBorderStyle)
				return false;

			if (_topBorderStyle != otherFormat._topBorderStyle)
				return false;

			if (_bottomBorderStyle != otherFormat._bottomBorderStyle)
				return false;

			if (_leftBorderColorInfo != otherFormat._leftBorderColorInfo)
				return false;

			if (_rightBorderColorInfo != otherFormat._rightBorderColorInfo)
				return false;

			if (_topBorderColorInfo != otherFormat._topBorderColorInfo)
				return false;

			if (_bottomBorderColorInfo != otherFormat._bottomBorderColorInfo)
				return false;

			if (Object.Equals(_fill, otherFormat._fill) == false)
				return false;

			// MD 10/26/11 - TFS91546
			if (_diagonalBorderColorInfo != otherFormat._diagonalBorderColorInfo)
				return false;

			// MD 10/26/11 - TFS91546
			if (_diagonalBorders != otherFormat._diagonalBorders)
				return false;

			// MD 10/26/11 - TFS91546
			if (_diagonalBorderStyle != otherFormat._diagonalBorderStyle)
				return false;

			if (_font.Element.Equals(otherFormat._font.Element) == false)
				return false;

			if (_formatOptions != otherFormat._formatOptions)
				return false;

			// MD 3/21/12 - TFS104630
			if (_addIndent != otherFormat._addIndent)
				return false;

			if (_style != otherFormat._style)
				return false;

			if (Object.Equals(_cachedStyleValues, otherFormat._cachedStyleValues) == false)
				return false;

			return true;
		}

		#endregion HasSameData

		#region OnAddedToRootCollection

		// MD 2/2/12 - TFS100573
		//public override void OnAddedToRootCollection(IGenericCachedCollection collection)
		public override void OnAddedToRootCollection(IGenericCachedCollectionEx collection)
		{
			base.OnAddedToRootCollection(collection);

			// When the format has been added to the workbook's collection of formats, its font is rooted
			if (_font != null && this.Workbook != null)
				_font.OnRooted(this.Workbook.Fonts);

			if (this.Style != null)
				this.Style.OnChildFormatAttached();
		}

		#endregion OnAddedToRootCollection

		#region OnRemovedFromCollection

		public override void OnRemovedFromRootCollection()
		{
			base.OnRemovedFromRootCollection();

			// When the format has been removed from the workbook's collection of formats, its font is not rooted anymore
			if (this.FontInternal != null)
				this.FontInternal.OnUnrooted();

			if (this.Style != null)
				this.Style.OnChildFormatDetached();
		}

		#endregion OnRemovedFromCollection

		#region OnWorkbookChanged

		protected override void OnWorkbookChanged(bool resetStyle)
		{
			base.OnWorkbookChanged(resetStyle);

			if (_cachedStyleValues != null)
				_cachedStyleValues.Workbook = this.Workbook;

			// Reset the style when the workbook changes.
			if (resetStyle || (this.Style != null && this.Style.Workbook != this.Workbook))
				this.Style = null;

			if (_font != null)
			{
				_font.OnUnrooted();
				if (this.Workbook != null)
					_font.OnRooted(this.Workbook.Fonts);
			}
		}

		#endregion // OnWorkbookChanged

		#region VerifyCanSetValue

		protected override void VerifyCanSetValue()
		{
			if (this.IsFrozen)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_ReadOnlyFormat"));
		}

		#endregion // VerifyCanSetValue

		#endregion Base Class Overrides

		#region Methods

		#region Public Methods

		#region SetFormatting

		public void SetFormatting(IWorksheetCellFormat source)
		{
			if (source == null)
				throw new ArgumentNullException("source", SR.GetString("LE_ArgumentNullException_SourceFormatting"));

			this.VerifyCanSetValue();

			WorksheetCellFormatData otherData = source as WorksheetCellFormatData;
			if (otherData != null)
			{
				// MD 4/3/12
				// Found while fixing TFS107866
				// We need to notify ourselves that the format options changed, so cache the old value.
				WorksheetCellFormatOptions oldFormatOptions = _formatOptions;

				_font.SetFontFormatting(otherData.FontInternal.Element);

				_alignment = otherData._alignment;
				_bottomBorderColorInfo = otherData._bottomBorderColorInfo;
				_bottomBorderStyle = otherData._bottomBorderStyle;
				_diagonalBorderColorInfo = otherData._diagonalBorderColorInfo;
				_diagonalBorders = otherData._diagonalBorders;
				_diagonalBorderStyle = otherData._diagonalBorderStyle;
				_fill = otherData._fill;
				_formatOptions = otherData._formatOptions;
				_formatStringIndex = otherData._formatStringIndex;
				_indent = otherData._indent;
				_leftBorderColorInfo = otherData._leftBorderColorInfo;
				_leftBorderStyle = otherData._leftBorderStyle;
				_locked = otherData._locked;
				_rightBorderColorInfo = otherData._rightBorderColorInfo;
				_rightBorderStyle = otherData._rightBorderStyle;
				_rotation = otherData._rotation;
				_shrinkToFit = otherData._shrinkToFit;

				if (this.Type != WorksheetCellFormatType.StyleFormat)
					_style = otherData._style;

				_topBorderColorInfo = otherData._topBorderColorInfo;
				_topBorderStyle = otherData._topBorderStyle;
				_verticalAlignment = otherData._verticalAlignment;
				_wrapText = otherData._wrapText;

				_cachedHashCode = null;

				this.OnValueSet(WorksheetCellFormatOptions.ApplyAlignmentFormatting);
				this.OnValueSet(WorksheetCellFormatOptions.ApplyBorderFormatting);
				this.OnValueSet(WorksheetCellFormatOptions.ApplyFillFormatting);
				this.OnValueSet(WorksheetCellFormatOptions.ApplyFontFormatting);
				this.OnValueSet(WorksheetCellFormatOptions.ApplyNumberFormatting);
				this.OnValueSet(WorksheetCellFormatOptions.ApplyProtectionFormatting);

				// MD 4/3/12
				// Found while fixing TFS107866
				// We need to notify ourselves that the format options changed.
				this.OnFormatOptionsChanged(oldFormatOptions);
			}
			else
			{
				_font.SetFontFormatting(source.Font);

				// Copy all properties from the source
				this.FormatString = source.FormatString;

				this.Locked = source.Locked;
				this.WrapText = source.WrapText;
				this.ShrinkToFit = source.ShrinkToFit;

				this.Alignment = source.Alignment;
				this.VerticalAlignment = source.VerticalAlignment;

				this.Rotation = source.Rotation;
				this.Indent = source.Indent;

				this.LeftBorderStyle = source.LeftBorderStyle;
				this.RightBorderStyle = source.RightBorderStyle;
				this.TopBorderStyle = source.TopBorderStyle;
				this.BottomBorderStyle = source.BottomBorderStyle;

				this.LeftBorderColorInfo = source.LeftBorderColorInfo;
				this.RightBorderColorInfo = source.RightBorderColorInfo;
				this.TopBorderColorInfo = source.TopBorderColorInfo;
				this.BottomBorderColorInfo = source.BottomBorderColorInfo;

				this.Fill = source.Fill;

				// MD 10/26/11 - TFS91546
				this.DiagonalBorderColorInfo = source.DiagonalBorderColorInfo;
				this.DiagonalBorders = source.DiagonalBorders;
				this.DiagonalBorderStyle = source.DiagonalBorderStyle;

				// MD 4/3/12
				// Found while fixing TFS107866
				// Moved below. We should set this after setting the Style so setting the Style doesn't overwrite 
				// the format options from the source format.
				//this.FormatOptions = source.FormatOptions;

				if (this.Type == WorksheetCellFormatType.CellFormat)
					this.Style = source.Style;

				// MD 4/3/12
				// Found while fixing TFS107866
				// Moved from above.
				this.FormatOptions = source.FormatOptions;
			}
		}

		#endregion SetFormatting

		#endregion Public Methods

		#region Internal Methods

		#region CacheResolvedFormatOptionsValues

		internal void CacheResolvedFormatOptionsValues(WorksheetCellFormatOptions formatOptionsToCopy, WorksheetCellFormatData styleFormat)
		{
			if (this.Type == WorksheetCellFormatType.StyleFormat)
			{
				Utilities.DebugFail("We should not be doing this for styles.");
				return;
			}

			if (formatOptionsToCopy == WorksheetCellFormatOptions.None)
				return;

			if (_cachedStyleValues == null)
				_cachedStyleValues = new WorksheetCellFormatData(this.Workbook, WorksheetCellFormatType.StyleFormat);

			// MD 5/6/12 - TFS110650
			// Re-wrote this to not use the resolved values from the style format (because it is unnecessary since unset style 
			// properties resolve to ultimate defaults) and to not use property setters (since we know no one is listening to 
			// notifications, we can just copy field values and manually set the format options on).
			#region Old Code

			//if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyNumberFormatting))
			//{
			//    _cachedStyleValues.FormatString = styleFormat.FormatStringResolved;
			//}

			//if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyAlignmentFormatting))
			//{
			//    _cachedStyleValues.Alignment = styleFormat.AlignmentResolved;
			//    _cachedStyleValues.Indent = styleFormat.IndentResolved;
			//    _cachedStyleValues.Rotation = styleFormat.RotationResolved;
			//    _cachedStyleValues.ShrinkToFit = styleFormat.ShrinkToFitResolved;
			//    _cachedStyleValues.VerticalAlignment = styleFormat.VerticalAlignmentResolved;
			//    _cachedStyleValues.WrapText = styleFormat.WrapTextResolved;
			//}

			//if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyFontFormatting))
			//{
			//    WorkbookFontData cachedFont = new WorkbookFontData(this.Workbook);
			//    cachedFont.Bold = styleFormat.FontBoldResolved;
			//    cachedFont.ColorInfo = styleFormat.FontColorInfoResolved;
			//    cachedFont.Height = styleFormat.FontHeightResolved;
			//    cachedFont.Italic = styleFormat.FontItalicResolved;
			//    cachedFont.Name = styleFormat.FontNameResolved;
			//    cachedFont.Strikeout = styleFormat.FontStrikeoutResolved;
			//    cachedFont.SuperscriptSubscriptStyle = styleFormat.FontSuperscriptSubscriptStyleResolved;
			//    cachedFont.UnderlineStyle = styleFormat.FontUnderlineStyleResolved;

			//    _cachedStyleValues.Font.SetFontFormatting(cachedFont);
			//}

			//if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyBorderFormatting))
			//{
			//    _cachedStyleValues.BottomBorderColorInfo = styleFormat.BottomBorderColorInfoResolved;
			//    _cachedStyleValues.BottomBorderStyle = styleFormat.BottomBorderStyleResolved;
			//    _cachedStyleValues.DiagonalBorderColorInfo = styleFormat.DiagonalBorderColorInfoResolved;
			//    _cachedStyleValues.DiagonalBorders = styleFormat.DiagonalBordersResolved;
			//    _cachedStyleValues.DiagonalBorderStyle = styleFormat.DiagonalBorderStyleResolved;
			//    _cachedStyleValues.LeftBorderColorInfo = styleFormat.LeftBorderColorInfoResolved;
			//    _cachedStyleValues.LeftBorderStyle = styleFormat.LeftBorderStyleResolved;
			//    _cachedStyleValues.RightBorderColorInfo = styleFormat.RightBorderColorInfoResolved;
			//    _cachedStyleValues.RightBorderStyle = styleFormat.RightBorderStyleResolved;
			//    _cachedStyleValues.TopBorderColorInfo = styleFormat.TopBorderColorInfoResolved;
			//    _cachedStyleValues.TopBorderStyle = styleFormat.TopBorderStyleResolved;
			//}

			//if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyFillFormatting))
			//{
			//    _cachedStyleValues.Fill = styleFormat.Fill;
			//}

			//if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyProtectionFormatting))
			//{
			//    _cachedStyleValues.Locked = styleFormat.LockedResolved;
			//}

			#endregion // Old Code
			if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyNumberFormatting))
			{
				_cachedStyleValues._formatStringIndex = styleFormat._formatStringIndex;
				_cachedStyleValues.FormatOptions |= WorksheetCellFormatOptions.ApplyNumberFormatting;
			}

			if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyAlignmentFormatting))
			{
				_cachedStyleValues._alignment = styleFormat._alignment;
				_cachedStyleValues._indent = styleFormat._indent;
				_cachedStyleValues._rotation = styleFormat._rotation;
				_cachedStyleValues._shrinkToFit = styleFormat._shrinkToFit;
				_cachedStyleValues._verticalAlignment = styleFormat._verticalAlignment;
				_cachedStyleValues._wrapText = styleFormat._wrapText;
				_cachedStyleValues.FormatOptions |= WorksheetCellFormatOptions.ApplyAlignmentFormatting;
			}

			if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyFontFormatting))
			{
				WorkbookFontData cachedFont = new WorkbookFontData(this.Workbook);
				WorkbookFontData styleFont = styleFormat.FontInternal.Element;
				cachedFont.Bold = styleFont.Bold;
				cachedFont.ColorInfo = styleFont.ColorInfo;
				cachedFont.Height = styleFont.Height;
				cachedFont.Italic = styleFont.Italic;
				cachedFont.Name = styleFont.Name;
				cachedFont.Strikeout = styleFont.Strikeout;
				cachedFont.SuperscriptSubscriptStyle = styleFont.SuperscriptSubscriptStyle;
				cachedFont.UnderlineStyle = styleFont.UnderlineStyle;

				_cachedStyleValues.Font.SetFontFormatting(cachedFont);
				_cachedStyleValues.FormatOptions |= WorksheetCellFormatOptions.ApplyFontFormatting;
			}

			if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyBorderFormatting))
			{
				_cachedStyleValues._bottomBorderColorInfo = styleFormat._bottomBorderColorInfo;
				_cachedStyleValues._bottomBorderStyle = styleFormat._bottomBorderStyle;
				_cachedStyleValues._diagonalBorderColorInfo = styleFormat._diagonalBorderColorInfo;
				_cachedStyleValues._diagonalBorders = styleFormat._diagonalBorders;
				_cachedStyleValues._diagonalBorderStyle = styleFormat._diagonalBorderStyle;
				_cachedStyleValues._leftBorderColorInfo = styleFormat._leftBorderColorInfo;
				_cachedStyleValues._leftBorderStyle = styleFormat._leftBorderStyle;
				_cachedStyleValues._rightBorderColorInfo = styleFormat._rightBorderColorInfo;
				_cachedStyleValues._rightBorderStyle = styleFormat._rightBorderStyle;
				_cachedStyleValues._topBorderColorInfo = styleFormat._topBorderColorInfo;
				_cachedStyleValues._topBorderStyle = styleFormat._topBorderStyle;
				_cachedStyleValues.FormatOptions |= WorksheetCellFormatOptions.ApplyBorderFormatting;
			}

			if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyFillFormatting))
			{
				_cachedStyleValues._fill = styleFormat._fill;
				_cachedStyleValues.FormatOptions |= WorksheetCellFormatOptions.ApplyFillFormatting;
			}

			if (Utilities.TestFlag(formatOptionsToCopy, WorksheetCellFormatOptions.ApplyProtectionFormatting))
			{
				_cachedStyleValues._locked = styleFormat._locked;
				_cachedStyleValues.FormatOptions |= WorksheetCellFormatOptions.ApplyProtectionFormatting;
			}

			// The format should have all format options the cached style values have.
			this.FormatOptions |= formatOptionsToCopy;
		}

		#endregion // CacheResolvedFormatOptionsValues

		#region Freeze

		internal void Freeze()
		{
			if (this.Collection is GenericCachedCollection<WorksheetCellFormatData>)
			{
				Utilities.DebugFail("Cannot freeze a data element in a shared collection.");
				return;
			}

			this.IsFrozen = true;
			this.FontInternal.Element.Freeze();
		}

		#endregion // Freeze

		#region GetColor

		internal Color GetColor(WorkbookColorInfo colorInfo)
		{
			if (colorInfo == null)
				return Utilities.ColorEmpty;

			return colorInfo.GetResolvedColor(this.Workbook);
		}

		#endregion // GetColor

		// MD 5/12/10 - TFS26732
		#region GetDefaultValue

		public static object GetDefaultValue(CellFormatValue value)
		{
			switch (value)
			{
				case CellFormatValue.Alignment:
					return HorizontalCellAlignment.Default;

				case CellFormatValue.BottomBorderColorInfo:
				case CellFormatValue.DiagonalBorderColorInfo:
				case CellFormatValue.FontColorInfo:
				case CellFormatValue.FontName:
				case CellFormatValue.FormatString:
				case CellFormatValue.LeftBorderColorInfo:
				case CellFormatValue.RightBorderColorInfo:
				case CellFormatValue.TopBorderColorInfo:
					return null;

				case CellFormatValue.BottomBorderStyle:
				case CellFormatValue.DiagonalBorderStyle:
				case CellFormatValue.LeftBorderStyle:
				case CellFormatValue.RightBorderStyle:
				case CellFormatValue.TopBorderStyle:
					return CellBorderLineStyle.Default;

				case CellFormatValue.DiagonalBorders:
					return DiagonalBorders.Default;

				case CellFormatValue.Fill:
					return null;

				case CellFormatValue.FontBold:
				case CellFormatValue.FontItalic:
				case CellFormatValue.FontStrikeout:
				case CellFormatValue.Locked:
				case CellFormatValue.ShrinkToFit:
				case CellFormatValue.WrapText:
					return ExcelDefaultableBoolean.Default;

				case CellFormatValue.FontHeight:
				case CellFormatValue.Indent:
				case CellFormatValue.Rotation:
					return -1;

				case CellFormatValue.FontSuperscriptSubscriptStyle:
					return FontSuperscriptSubscriptStyle.Default;

				case CellFormatValue.FontUnderlineStyle:
					return FontUnderlineStyle.Default;

				// MD 2/27/12 - 12.1 - Table Support
				//case CellFormatValue.IsStyle:
				//    return false;

				case CellFormatValue.VerticalAlignment:
					return VerticalCellAlignment.Default;

				default:
					Utilities.DebugFail("Unknown format value: " + value);
					return true;
			}
		}

		#endregion // GetDefaultValue

		#region GetFillPattern

		internal FillPatternStyle GetFillPattern(CellFill fill)
		{
			if (fill == null)
			{
				if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyFillFormatting))
					return FillPatternStyle.None;

#pragma warning disable 0618
				return FillPatternStyle.Default;
#pragma warning restore 0618
			}

			CellFillPattern cellFillPattern = fill as CellFillPattern;
			if (cellFillPattern != null)
				return cellFillPattern.PatternStyle;

			CellFillGradient cellFillGradient = fill as CellFillGradient;
			if (cellFillGradient != null)
				return FillPatternStyle.Solid;

			Utilities.DebugFail("Unknown Fill type.");
#pragma warning disable 0618
			return FillPatternStyle.Default;
#pragma warning restore 0618
		}

		#endregion // GetFillPattern

		#region GetFileFormatFillPatternColor

		internal WorkbookColorInfo GetFileFormatFillPatternColor(CellFill fill, bool backgroundColor, bool resolveNullValues)
		{
			WorkbookColorInfo defaultColor = backgroundColor
				? new WorkbookColorInfo(Utilities.SystemColorsInternal.WindowColor)
				: WorkbookColorInfo.Automatic;

			WorkbookColorInfo missingValue = resolveNullValues ? defaultColor : null;

			if (fill == null)
			{
				if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyFillFormatting))
					return defaultColor;

				return missingValue;
			}

			CellFillPattern cellFillPattern = fill as CellFillPattern;
			if (cellFillPattern != null)
			{
				if (backgroundColor)
					return cellFillPattern.GetFileFormatBackgroundColorInfo(this);

				return cellFillPattern.GetFileFormatForegroundColorInfo(this);
			}

			CellFillGradient cellFillGradient = fill as CellFillGradient;
			if (cellFillGradient != null)
			{
				if (backgroundColor)
					return WorkbookColorInfo.Automatic;

				return cellFillGradient.Stops[0].ColorInfo;
			}

			Utilities.DebugFail("Unknown Fill type.");
			return missingValue;
		}

		#endregion // GetFileFormatFillPatternColor

		#region GetResolvedValue

		internal object GetResolvedValue(CellFormatValue property, GetAdjacentBorderValueCallback callback)
		{
			bool isFromAdjacentBorder;
			object value = this.GetResolvedValueHelper(property, callback, out isFromAdjacentBorder);

			// MD 4/4/12 - TFS107655
			// Apparently, both sets of overlapping border properties can be set when loading an XLS file and Excel
			// seems to honor the ones which do not have the ultimate defaults.
			if (isFromAdjacentBorder == false &&
				callback != null &&
				Utilities.IsEdgeBorderValue(property) &&
				Object.Equals(value, WorksheetCellFormatData.GetUltimateDefault(property)))
			{
				CellFormatValue associatedProperty = Utilities.GetAssociatedBorderValue(property);
				object associatedValue = this.GetValue(associatedProperty);

				if (WorksheetCellFormatData.IsValueDefault(associatedProperty, associatedValue) ||
					Object.Equals(associatedValue, WorksheetCellFormatData.GetUltimateDefault(associatedProperty)))
				{
					object adjacentValue;
					if (callback(property, out adjacentValue))
						return adjacentValue;
				}
			}

			return value;
		}

		private object GetResolvedValueHelper(CellFormatValue property, GetAdjacentBorderValueCallback callback, out bool isFromAdjacentBorder)
		{
			isFromAdjacentBorder = false;

			object value = this.GetValue(property);
			if (WorksheetCellFormatData.IsValueDefault(property, value) == false)
				return value;

			WorksheetCellFormatOptions associatedStyleOption = WorksheetCellFormatData.StyleOptionForValue(property);

			// If this is a differential format without the associated style option set, allow the default value to be returned.
			if (this.Type == WorksheetCellFormatType.DifferentialFormat &&
				Utilities.TestFlag(this.FormatOptions, associatedStyleOption) == false)
				return value;

			if (callback != null &&
				this.Type == WorksheetCellFormatType.CellFormat &&
				Utilities.IsEdgeBorderValue(property))
			{
				if (callback(property, out value))
				{
					isFromAdjacentBorder = true;
					return value;
				}
			}

			bool resolveThruStyleIfPossible;
			value = this.GetResolvedDefault(property, out resolveThruStyleIfPossible);

			if (resolveThruStyleIfPossible && this.Type != WorksheetCellFormatType.StyleFormat)
			{
				if (Utilities.TestFlag(this.FormatOptions, associatedStyleOption))
				{
					Debug.Assert(_cachedStyleValues != null || this.Type == WorksheetCellFormatType.DifferentialFormat || this.Collection == null, "We should have cached style values here.");
					if (_cachedStyleValues != null)
					{
						Debug.Assert(Utilities.TestFlag(_cachedStyleValues.FormatOptions, associatedStyleOption),
						    "If the format has a style option set, so should the cached style values.");
						value = _cachedStyleValues.GetResolvedValue(property);
					}
				}
				else if (this.Style != null)
				{
					value = this.Style.StyleFormatInternal.GetResolvedValue(property);
				}
			}

			Debug.Assert(WorksheetCellFormatData.IsValueDefault(property, value) == false, "The value should no longer be the default value.");
			return value;
		}

		#endregion // GetResolvedValue

		// MD 4/18/11 - TFS62026
		// Added a way to get any value with the CellFormatValue enum.
		#region GetValue

		public object GetValue(CellFormatValue valueToGet)
		{
			switch (valueToGet)
			{
				case CellFormatValue.Alignment:
					return this.Alignment;

				case CellFormatValue.BottomBorderColorInfo:
					return this.BottomBorderColorInfo;

				case CellFormatValue.BottomBorderStyle:
					return this.BottomBorderStyle;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorderColorInfo:
					return this.DiagonalBorderColorInfo;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorders:
					return this.DiagonalBorders;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorderStyle:
					return this.DiagonalBorderStyle;

				case CellFormatValue.Fill:
					return this.Fill;

				// MD 10/13/10 - TFS43003
				case CellFormatValue.FontBold:
					return this.Font.Bold;

				case CellFormatValue.FontColorInfo:
					return this.Font.ColorInfo;

				case CellFormatValue.FontHeight:
					return this.Font.Height;

				case CellFormatValue.FontItalic:
					return this.Font.Italic;

				case CellFormatValue.FontName:
					return this.Font.Name;

				case CellFormatValue.FontStrikeout:
					return this.Font.Strikeout;

				case CellFormatValue.FontSuperscriptSubscriptStyle:
					return this.Font.SuperscriptSubscriptStyle;

				case CellFormatValue.FontUnderlineStyle:
					return this.Font.UnderlineStyle;
				// ***************** End of TFS43003 Fix ********************

				case CellFormatValue.FormatString:
					return this.FormatString;

				case CellFormatValue.Indent:
					return this.Indent;

				case CellFormatValue.LeftBorderColorInfo:
					return this.LeftBorderColorInfo;

				case CellFormatValue.LeftBorderStyle:
					return this.LeftBorderStyle;

				case CellFormatValue.Locked:
					return this.Locked;

				case CellFormatValue.RightBorderColorInfo:
					return this.RightBorderColorInfo;

				case CellFormatValue.RightBorderStyle:
					return this.RightBorderStyle;

				case CellFormatValue.Rotation:
					return this.Rotation;

				case CellFormatValue.ShrinkToFit:
					return this.ShrinkToFit;

				// MD 2/27/12 - 12.1 - Table Support
				//case CellFormatValue.IsStyle:
				//    return this.IsStyle;

				case CellFormatValue.Style:
					return this.Style;

				case CellFormatValue.FormatOptions:
					return this.FormatOptions;

				case CellFormatValue.TopBorderColorInfo:
					return this.TopBorderColorInfo;

				case CellFormatValue.TopBorderStyle:
					return this.TopBorderStyle;

				case CellFormatValue.VerticalAlignment:
					return this.VerticalAlignment;

				case CellFormatValue.WrapText:
					return this.WrapText;

				default:
					Utilities.DebugFail("Unknown format value: " + valueToGet);
					return null;
			}
		}

		#endregion // GetValue

		#region GetXFEXTProps

		internal List<ExtProp> GetXFEXTProps()
		{
			List<ExtProp> xfextProps = new List<ExtProp>();

			this.TryAddXFEXTColorInfo(this.BottomBorderColorInfoResolved, ExtPropType.BottomBorderColor, xfextProps);
			this.TryAddXFEXTColorInfo(this.DiagonalBorderColorInfoResolved, ExtPropType.DiagonalBorderColor, xfextProps);
			this.TryAddXFEXTColorInfo(this.LeftBorderColorInfoResolved, ExtPropType.LeftBorderColor, xfextProps);
			this.TryAddXFEXTColorInfo(this.RightBorderColorInfoResolved, ExtPropType.RightBorderColor, xfextProps);
			this.TryAddXFEXTColorInfo(this.TopBorderColorInfoResolved, ExtPropType.TopBorderColor, xfextProps);

			WorkbookColorInfo fontColorInfo = this.VerifyFontColorInfoForSave(this.FontColorInfoResolved);
			if (fontColorInfo != null)
				xfextProps.Add(new ExtPropColor(fontColorInfo, ExtPropType.CellTextColor));

			CellFill fill = this.FillResolved;
			if (fill != null)
				fill.PopulateXFEXTProps(this, xfextProps);

			int indent = this.IndentResolved;
			if (WorksheetCellFormatData.MaxIndent2003 < indent)
				xfextProps.Add(new ExtPropTextIndentationLevel((ushort)indent));

			if (_fontScheme != FontScheme.Nil)
				xfextProps.Add(new ExtPropFontScheme(_fontScheme));

			return xfextProps;
		}

		#endregion  // GetXFEXTProps

		#region GetXFProps

		internal List<XFProp> GetXFProps()
		{
			Debug.Assert(this.Workbook != null, "We should have a workbook here.");

			bool includeEverything = (this.Type == WorksheetCellFormatType.DifferentialFormat);

			List<XFProp> xfextProps = new List<XFProp>();
			if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting))
			{
				if (includeEverything)
				{
					if (this.Alignment != HorizontalCellAlignment.Default)
						xfextProps.Add(new XFPropHorizontalAlignment(this.Alignment));

					if (this.Indent != -1)
						xfextProps.Add(new XFPropTextIndentationLevel((ushort)Math.Min(this.Indent, WorksheetCellFormatData.MaxIndent2003)));

					if (this.Rotation != -1)
						xfextProps.Add(new XFPropTextRotation((byte)this.Rotation));

					if (this.ShrinkToFit != ExcelDefaultableBoolean.Default)
						xfextProps.Add(new XFPropBool(XFPropType.ShrinkToFit, this.ShrinkToFit));

					if (this.VerticalAlignment != VerticalCellAlignment.Default)
						xfextProps.Add(new XFPropVerticalAlignment(this.VerticalAlignment));

					if (this.WrapText != ExcelDefaultableBoolean.Default)
						xfextProps.Add(new XFPropBool(XFPropType.WrappedText, this.WrapText));
				}

				if (WorksheetCellFormatData.MaxIndent2003 < this.Indent)
					xfextProps.Add(new XFPropTextIndentationLevelRelative((short)(this.Indent - WorksheetCellFormatData.MaxIndent2003)));
			}

			if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting))
			{
				if (this.BottomBorderColorInfo != null || this.BottomBorderStyle != CellBorderLineStyle.Default)
					xfextProps.Add(new XFPropBorder(XFPropType.BottomBorder, this.BottomBorderColorInfoResolved, this.BottomBorderStyleResolved));

				if (this.DiagonalBorderColorInfo != null || this.TopBorderStyle != CellBorderLineStyle.Default)
					xfextProps.Add(new XFPropBorder(XFPropType.TopBorder, this.TopBorderColorInfoResolved, this.TopBorderStyleResolved));

				if (this.LeftBorderColorInfo != null || this.LeftBorderStyle != CellBorderLineStyle.Default)
					xfextProps.Add(new XFPropBorder(XFPropType.LeftBorder, this.LeftBorderColorInfoResolved, this.LeftBorderStyleResolved));

				if (this.RightBorderColorInfo != null || this.RightBorderStyle != CellBorderLineStyle.Default)
					xfextProps.Add(new XFPropBorder(XFPropType.RightBorder, this.RightBorderColorInfoResolved, this.RightBorderStyleResolved));

				if (this.BottomBorderColorInfo != null || this.TopBorderStyle != CellBorderLineStyle.Default)
					xfextProps.Add(new XFPropBorder(XFPropType.TopBorder, this.TopBorderColorInfoResolved, this.TopBorderStyleResolved));

				if (Utilities.TestFlag(this.DiagonalBorders, DiagonalBorders.DiagonalDown))
					xfextProps.Add(new XFPropBool(XFPropType.DiagonalDownBorder, true));

				if (Utilities.TestFlag(this.DiagonalBorders, DiagonalBorders.DiagonalUp))
					xfextProps.Add(new XFPropBool(XFPropType.DiagonalUpBorder, true));
			}

			if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyFillFormatting))
			{
				CellFill fill = this.FillResolved;
				if (fill != null)
					fill.PopulateXFProps(this, xfextProps);
			}

			if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyFontFormatting))
			{
				if (includeEverything)
				{
					WorkbookFontData font = this.FontInternal.Element;

					if (font.Bold != ExcelDefaultableBoolean.Default)
						xfextProps.Add(new XFPropFontBold(font.Bold));

					if (font.ColorInfo != null)
						xfextProps.Add(new XFPropColor(XFPropType.FontColor, this.VerifyFontColorInfoForSave(font.ColorInfo)));

					if (font.Height != -1)
						xfextProps.Add(new XFPropFontHeight((uint)font.Height));

					if (font.Italic != ExcelDefaultableBoolean.Default)
						xfextProps.Add(new XFPropBool(XFPropType.FontItalic, font.Italic));

					if (font.Name != null)
						xfextProps.Add(new XFPropFontName(font.Name));

					if (font.Strikeout != ExcelDefaultableBoolean.Default)
						xfextProps.Add(new XFPropBool(XFPropType.FontItalic, font.Strikeout));

					if (font.SuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.Default)
						xfextProps.Add(new XFPropFontSubscriptSuperscript(font.SuperscriptSubscriptStyle));

					if (font.UnderlineStyle != FontUnderlineStyle.Default)
						xfextProps.Add(new XFPropFontUnderline(font.UnderlineStyle));
				}

				if (_fontScheme != FontScheme.Nil)
					xfextProps.Add(new XFPropFontScheme(_fontScheme));
			}

			if (includeEverything)
			{
				if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting))
				{
					if (this.FormatStringIndex != -1)
					{
						if (this.Workbook != null && this.Workbook.Formats.IsStandardFormat(this.FormatStringIndex))
							xfextProps.Add(new XFPropNumberFormatId((ushort)this.FormatStringIndex));
						else
							xfextProps.Add(new XFPropNumberFormat((ushort)this.FormatStringIndex, this.FormatString));
					}
				}

				if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting))
				{
					if (this.Locked != ExcelDefaultableBoolean.Default)
						xfextProps.Add(new XFPropBool(XFPropType.Locked, this.Locked));
				}
			}

			return xfextProps;
		}

		#endregion // GetXFProps

		#region IsAnyBorderPropertySet

		internal bool IsAnyBorderPropertySet()
		{
			return
				this.BottomBorderColorInfo != null ||
				this.BottomBorderStyle != CellBorderLineStyle.Default ||
				this.DiagonalBorderColorInfo != null ||
				this.DiagonalBorders != DiagonalBorders.Default ||
				this.DiagonalBorderStyle != CellBorderLineStyle.Default ||
				this.LeftBorderColorInfo != null ||
				this.LeftBorderStyle != CellBorderLineStyle.Default ||
				this.RightBorderColorInfo != null ||
				this.RightBorderStyle != CellBorderLineStyle.Default ||
				this.TopBorderColorInfo != null ||
				this.TopBorderStyle != CellBorderLineStyle.Default;
		}

		#endregion // IsAnyBorderPropertySet

		// MD 5/12/10 - TFS26732
		#region IsValueDefault

		public static bool IsValueDefault(CellFormatValue value, object testValue)
		{
			// There is no default for the Style or FormatOptions properties.
			if (value == CellFormatValue.Style || value == CellFormatValue.FormatOptions)
				return false;

			return Object.Equals(testValue, WorksheetCellFormatData.GetDefaultValue(value));
		}

		#endregion // IsValueDefault

		#region OnFontChanged

		internal void OnFontChanged()
		{
			if (this.FontNameResolved == WorkbookFontData.DefaultMinorFontName)
				_fontScheme = FontScheme.Minor;
			else if (this.FontNameResolved == WorkbookFontData.DefaultMajorFontName)
				_fontScheme = FontScheme.Major;
			else
				_fontScheme = FontScheme.Nil;

			this.OnValueSet(WorksheetCellFormatOptions.ApplyFontFormatting);
		}

		#endregion // OnFontChanged

		#region ResetValue

		internal void ResetValue(CellFormatValue value)
		{
			this.SetValue(value, WorksheetCellFormatData.GetDefaultValue(value));
		}

		#endregion // ResetValue

		// MD 4/18/11 - TFS62026
		// Added a way to set any value with the CellFormatValue enum.
		#region SetValue

		public void SetValue(CellFormatValue valueToSet, object value)
		{
			switch (valueToSet)
			{
				case CellFormatValue.Alignment:
					this.Alignment = (HorizontalCellAlignment)value;
					break;

				case CellFormatValue.BottomBorderColorInfo:
					this.BottomBorderColorInfo = (WorkbookColorInfo)value;
					break;

				case CellFormatValue.BottomBorderStyle:
					this.BottomBorderStyle = (CellBorderLineStyle)value;
					break;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorderColorInfo:
					this.DiagonalBorderColorInfo = (WorkbookColorInfo)value;
					break;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorders:
					this.DiagonalBorders = (DiagonalBorders)value;
					break;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorderStyle:
					this.DiagonalBorderStyle = (CellBorderLineStyle)value;
					break;

				case CellFormatValue.Fill:
					this.Fill = (CellFill)value;
					break;

				case CellFormatValue.FontBold:
					this.Font.Bold = (ExcelDefaultableBoolean)value;
					break;

				case CellFormatValue.FontColorInfo:
					this.Font.ColorInfo = (WorkbookColorInfo)value;
					break;

				case CellFormatValue.FontHeight:
					this.Font.Height = (int)value;
					break;

				case CellFormatValue.FontItalic:
					this.Font.Italic = (ExcelDefaultableBoolean)value;
					break;

				case CellFormatValue.FontName:
					this.Font.Name = (string)value;
					break;

				case CellFormatValue.FontStrikeout:
					this.Font.Strikeout = (ExcelDefaultableBoolean)value;
					break;

				case CellFormatValue.FontSuperscriptSubscriptStyle:
					this.Font.SuperscriptSubscriptStyle = (FontSuperscriptSubscriptStyle)value;
					break;

				case CellFormatValue.FontUnderlineStyle:
					this.Font.UnderlineStyle = (FontUnderlineStyle)value;
					break;

				case CellFormatValue.FormatString:
					this.FormatString = (string)value;
					break;

				case CellFormatValue.Indent:
					this.Indent = (int)value;
					break;

				case CellFormatValue.LeftBorderColorInfo:
					this.LeftBorderColorInfo = (WorkbookColorInfo)value;
					break;

				case CellFormatValue.LeftBorderStyle:
					this.LeftBorderStyle = (CellBorderLineStyle)value;
					break;

				case CellFormatValue.Locked:
					this.Locked = (ExcelDefaultableBoolean)value;
					break;

				case CellFormatValue.RightBorderColorInfo:
					this.RightBorderColorInfo = (WorkbookColorInfo)value;
					break;

				case CellFormatValue.RightBorderStyle:
					this.RightBorderStyle = (CellBorderLineStyle)value;
					break;

				case CellFormatValue.Rotation:
					this.Rotation = (int)value;
					break;

				case CellFormatValue.ShrinkToFit:
					this.ShrinkToFit = (ExcelDefaultableBoolean)value;
					break;

				// MD 2/27/12 - 12.1 - Table Support
				//case CellFormatValue.IsStyle:
				//    this.IsStyle = (bool)value;
				//    break;

				case CellFormatValue.Style:
					this.Style = (WorkbookStyle)value;
					break;

				case CellFormatValue.FormatOptions:
					this.FormatOptions = (WorksheetCellFormatOptions)value;
					break;

				case CellFormatValue.TopBorderColorInfo:
					this.TopBorderColorInfo = (WorkbookColorInfo)value;
					break;

				case CellFormatValue.TopBorderStyle:
					this.TopBorderStyle = (CellBorderLineStyle)value;
					break;

				case CellFormatValue.VerticalAlignment:
					this.VerticalAlignment = (VerticalCellAlignment)value;
					break;

				case CellFormatValue.WrapText:
					this.WrapText = (ExcelDefaultableBoolean)value;
					break;

				default:
					Utilities.DebugFail("Unknown format value: " + value);
					break;
			}
		}

		#endregion  // SetValue

		#region TryAddXFEXTColorInfo

		internal void TryAddXFEXTColorInfo(WorkbookColorInfo colorInfo, ExtPropType propType, List<ExtProp> xfextProps)
		{
			if (colorInfo == null)
				return;

			ColorableItem colorableItem;
			switch (propType)
			{
				case ExtPropType.BottomBorderColor:
				case ExtPropType.DiagonalBorderColor:
				case ExtPropType.LeftBorderColor:
				case ExtPropType.RightBorderColor:
				case ExtPropType.TopBorderColor:
					colorableItem = ColorableItem.CellBorder;
					break;

				case ExtPropType.CellTextColor:
					colorableItem = ColorableItem.CellFont;
					break;

				case ExtPropType.ForegroundColor:
				case ExtPropType.BackgroundColor:
					colorableItem = ColorableItem.CellFill;
					break;

				default:
					Utilities.DebugFail("This is unexpected.");
					colorableItem = ColorableItem.CellBorder;
					break;
			}

			if (colorInfo.IsSupportedIn2003(this.Workbook, colorableItem) == false)
				xfextProps.Add(new ExtPropColor(colorInfo, propType));
		}

		#endregion // TryAddXFEXTColorInfo

		#region TryAddXFPropColorInfo

		internal void TryAddXFPropColorInfo(WorkbookColorInfo colorInfo, XFPropType propType, List<XFProp> xfextProps)
		{
			if (colorInfo == null)
				return;

			ColorableItem colorableItem;
			switch (propType)
			{
				case XFPropType.ForegroundColor:
				case XFPropType.BackgroundColor:
					colorableItem = ColorableItem.CellFill;
					break;

				default:
					Utilities.DebugFail("This is unexpected.");
					colorableItem = ColorableItem.CellBorder;
					break;
			}

			if (colorInfo.IsSupportedIn2003(this.Workbook, colorableItem) == false)
				xfextProps.Add(new XFPropColor(propType, colorInfo));
		}

		#endregion // TryAddXFPropColorInfo

		#region UpdatedFillPattern

		public CellFill UpdatedFillPattern(CellFill resolvedFill, FillPatternStyle pattern)
		{
#pragma warning disable 0618
			if (pattern == FillPatternStyle.Default)
#pragma warning restore 0618
				return null;

			if (pattern == FillPatternStyle.None)
				return CellFill.NoColor;

			CellFillPattern cellFillPattern = resolvedFill as CellFillPattern;
			CellFillGradient cellFillGradient = resolvedFill as CellFillGradient;
			Debug.Assert(cellFillGradient != null || cellFillPattern != null, "One of these must be non-null");

			WorkbookColorInfo backgroundColorInfo = this.GetFileFormatFillPatternColor(resolvedFill, true, true);
			WorkbookColorInfo patternColorInfo = this.GetFileFormatFillPatternColor(resolvedFill, false, true);

			if (pattern == FillPatternStyle.Solid)
				return CellFill.CreatePatternFill(patternColorInfo, backgroundColorInfo, pattern);

			return CellFill.CreatePatternFill(backgroundColorInfo, patternColorInfo, pattern);
		}

		#endregion // UpdatedFillPattern

		#region UpdatedFillPatternColor

		public CellFill UpdatedFillPatternColor(CellFill resolvedFill, Color color, bool backgroundColor)
		{
			CellFillPattern cellFillPattern = resolvedFill as CellFillPattern;
			if (cellFillPattern != null)
			{
				FillPatternStyle pattern = cellFillPattern.PatternStyle;

				if (Utilities.ColorIsEmpty(color))
				{
					color = backgroundColor
						? Utilities.SystemColorsInternal.WindowColor
						: Utilities.SystemColorsInternal.WindowTextColor;
				}
				else
				{
					if (pattern == FillPatternStyle.None)
					{
						if (backgroundColor)
							pattern = FillPatternStyle.Gray50percent;
						else
							pattern = FillPatternStyle.Solid;
					}
				}

				if (backgroundColor)
				{
					return new CellFillPattern(
						new WorkbookColorInfo(color),
						cellFillPattern.GetFileFormatForegroundColorInfo(this),
						pattern,
						this);
				}

				return new CellFillPattern(
					cellFillPattern.GetFileFormatBackgroundColorInfo(this),
					new WorkbookColorInfo(color),
					pattern,
					this);
			}

			if (Utilities.ColorIsEmpty(color))
				return null;

			Debug.Assert(resolvedFill is CellFillGradient, "This is unexpected.");
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_DeprecatedFillColorSetWithGradientFill"));
		}

		#endregion // UpdatedFillPatternColor

		#endregion Internal Methods

		#region Private Method

		// MD 5/14/07 - BR22853
		#region ConvertColorForBorder

		private static void ConvertColorForBorder(ref WorkbookColorInfo color)
		{
			if (color == null)
				return;

			// WindowFrame is not supported for borders
			if (color.Color.HasValue && color.Color.Value == Utilities.SystemColorsInternal.WindowFrameColor)
				color = new WorkbookColorInfo(Utilities.SystemColorsInternal.WindowTextColor, null, color.Tint, false);
		}

		#endregion ConvertColorForBorder

		#region FilterOutOptionsWithAllUltimateDefaults

		private WorksheetCellFormatOptions FilterOutOptionsWithAllUltimateDefaults(WorksheetCellFormatOptions formatOptions)
		{
			WorksheetCellFormatOptions result = WorksheetCellFormatOptions.None;

			this.FilterOutOptionsWithAllUltimateDefaultsHelper(formatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting, ref result);
			this.FilterOutOptionsWithAllUltimateDefaultsHelper(formatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting, ref result);
			this.FilterOutOptionsWithAllUltimateDefaultsHelper(formatOptions, WorksheetCellFormatOptions.ApplyFillFormatting, ref result);
			this.FilterOutOptionsWithAllUltimateDefaultsHelper(formatOptions, WorksheetCellFormatOptions.ApplyFontFormatting, ref result);
			this.FilterOutOptionsWithAllUltimateDefaultsHelper(formatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting, ref result);
			this.FilterOutOptionsWithAllUltimateDefaultsHelper(formatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting, ref result);

			return result;
		}

		private void FilterOutOptionsWithAllUltimateDefaultsHelper(
			WorksheetCellFormatOptions formatOptions,
			WorksheetCellFormatOptions option,
			ref WorksheetCellFormatOptions result)
		{
			if (Utilities.TestFlag(formatOptions, option))
			{
				if (this.HasAllUltimateDefaults(option) == false)
					result |= option;
			}
		}

		#endregion // FilterOutOptionsWithAllUltimateDefaults

		#region GetAssociatedValues

		private static CellFormatValue[] GetAssociatedValues(WorksheetCellFormatOptions styleOption)
		{
			switch (styleOption)
			{
				case WorksheetCellFormatOptions.ApplyNumberFormatting:
					return WorksheetCellFormatData.NumberFormattingProperties;

				case WorksheetCellFormatOptions.ApplyAlignmentFormatting:
					return WorksheetCellFormatData.AlignmentFormattingProperties;

				case WorksheetCellFormatOptions.ApplyFontFormatting:
					return WorksheetCellFormatData.FontFormattingProperties;

				case WorksheetCellFormatOptions.ApplyBorderFormatting:
					return WorksheetCellFormatData.BorderFormattingProperties;

				case WorksheetCellFormatOptions.ApplyFillFormatting:
					return WorksheetCellFormatData.FillFormattingProperties;

				case WorksheetCellFormatOptions.ApplyProtectionFormatting:
					return WorksheetCellFormatData.ProtectionFormattingProperties;

				default:
					Utilities.DebugFail("Unknown WorksheetCellFormatOptions: " + styleOption);
					return new CellFormatValue[0];
			}
		}

		#endregion // GetAssociatedValues

		#region GetChangesRequiredForChangedFormatOption

		private void GetChangesRequiredForChangedFormatOption(
			WorksheetCellFormatOptions oldFormatOptions,
			WorksheetCellFormatOptions formatOptionToTest,
			ref WorksheetCellFormatOptions formatOptionsToReset,
			ref WorksheetCellFormatOptions formatOptionsToCopyFromStyle)
		{
			bool oldApplyNumberFormatting = Utilities.TestFlag(oldFormatOptions, formatOptionToTest);
			bool newApplyNumberFormatting = Utilities.TestFlag(this.FormatOptions, formatOptionToTest);

			if (oldApplyNumberFormatting != newApplyNumberFormatting)
			{
				if (newApplyNumberFormatting == false)
					formatOptionsToReset |= formatOptionToTest;
				else
					formatOptionsToCopyFromStyle |= formatOptionToTest;
			}
		}

		#endregion // GetChangesRequiredForChangedFormatOption

		#region GetResolvedDefault

		private object GetResolvedDefault(CellFormatValue value, out bool resolveThruStyleIfPossible)
		{
			resolveThruStyleIfPossible = true;
			switch (value)
			{
				case CellFormatValue.DiagonalBorderStyle:
					WorkbookColorInfo diagonalBorderColorInfo = this.DiagonalBorderColorInfo;
					DiagonalBorders diagonalBorders = this.DiagonalBorders;
					if (diagonalBorderColorInfo != null ||
						Utilities.TestFlag(diagonalBorders, DiagonalBorders.DiagonalDown) ||
						Utilities.TestFlag(diagonalBorders, DiagonalBorders.DiagonalUp))
					{
						resolveThruStyleIfPossible = false;
						return CellBorderLineStyle.Thin;
					}
					break;

				case CellFormatValue.BottomBorderStyle:
				case CellFormatValue.LeftBorderStyle:
				case CellFormatValue.RightBorderStyle:
				case CellFormatValue.TopBorderStyle:
					WorkbookColorInfo associatedBorderColor = (WorkbookColorInfo)this.GetValue(Utilities.GetAssociatedBorderValue(value));
					if (associatedBorderColor != null)
					{
						resolveThruStyleIfPossible = false;
						return CellBorderLineStyle.Thin;
					}
					break;
			}

			return WorksheetCellFormatData.GetUltimateDefault(value);
		}

		#endregion // GetResolvedDefault

		#region GetResolvedValue

		private object GetResolvedValue(CellFormatValue property)
		{
			return this.GetResolvedValue(property, null);
		}

		#endregion // GetResolvedValue

		#region GetUltimateDefault

		private static object GetUltimateDefault(CellFormatValue value)
		{
			switch (value)
			{
				case CellFormatValue.Alignment:
					return HorizontalCellAlignment.General;

				case CellFormatValue.BottomBorderColorInfo:
				case CellFormatValue.DiagonalBorderColorInfo:
				case CellFormatValue.FontColorInfo:
				case CellFormatValue.LeftBorderColorInfo:
				case CellFormatValue.RightBorderColorInfo:
				case CellFormatValue.TopBorderColorInfo:
					return WorkbookColorInfo.Automatic;

				case CellFormatValue.BottomBorderStyle:
				case CellFormatValue.DiagonalBorderStyle:
				case CellFormatValue.LeftBorderStyle:
				case CellFormatValue.RightBorderStyle:
				case CellFormatValue.TopBorderStyle:
					return CellBorderLineStyle.None;

				case CellFormatValue.DiagonalBorders:
					return DiagonalBorders.None;

				case CellFormatValue.Fill:
					return CellFill.NoColor;

				case CellFormatValue.FontBold:
				case CellFormatValue.FontItalic:
				case CellFormatValue.FontStrikeout:
				case CellFormatValue.ShrinkToFit:
				case CellFormatValue.WrapText:
					return ExcelDefaultableBoolean.False;

				case CellFormatValue.FontHeight:
					return WorkbookFontData.DefaultFontHeight;

				case CellFormatValue.FontName:
					return WorkbookFontData.DefaultFontName;

				case CellFormatValue.FontSuperscriptSubscriptStyle:
					return FontSuperscriptSubscriptStyle.None;

				case CellFormatValue.FontUnderlineStyle:
					return FontUnderlineStyle.None;

				case CellFormatValue.FormatString:
					return "General";

				case CellFormatValue.Indent:
				case CellFormatValue.Rotation:
					return 0;

				case CellFormatValue.Locked:
					return ExcelDefaultableBoolean.True;

				case CellFormatValue.VerticalAlignment:
					return VerticalCellAlignment.Bottom;

				case CellFormatValue.Style:
				case CellFormatValue.FormatOptions:
					Utilities.DebugFail("There is no resolved value for this CellFormatValue: " + value);
					return null;

				default:
					Utilities.DebugFail("Unknown CellFormatValue: " + value);
					return null;
			}
		}

		#endregion // GetUltimateDefault

		#region HasAllUltimateDefaults

		public bool HasAllUltimateDefaults(WorksheetCellFormatOptions option)
		{
			CellFormatValue[] values = WorksheetCellFormatData.GetAssociatedValues(option);

			for (int i = 0; i < values.Length; i++)
			{
				CellFormatValue formatValue = values[i];
				if (Object.Equals(this.GetResolvedValue(formatValue), WorksheetCellFormatData.GetUltimateDefault(formatValue)) == false)
					return false;
			}

			return true;
		}

		#endregion // HasAllUltimateDefaults

		#region OnValueSet

		private void OnValueSet(CellFormatValue property)
		{
			this.OnValueSet(WorksheetCellFormatData.StyleOptionForValue(property));
		}

		private void OnValueSet(WorksheetCellFormatOptions associatedStyleOption)
		{
			_cachedHashCode = null;

			switch (associatedStyleOption)
			{
				case WorksheetCellFormatOptions.ApplyNumberFormatting:
					this.UpdateUseNumberFormattingFlag();
					break;

				case WorksheetCellFormatOptions.ApplyAlignmentFormatting:
					this.UpdateUseAlignmentFormattingFlag();
					break;

				case WorksheetCellFormatOptions.ApplyFontFormatting:
					this.UpdateUseFontFormattingFlag();
					break;

				case WorksheetCellFormatOptions.ApplyBorderFormatting:
					this.UpdateUseBorderFormattingFlag();
					break;

				case WorksheetCellFormatOptions.ApplyFillFormatting:
					this.UpdateUsePatternsFormattingFlag();
					break;

				case WorksheetCellFormatOptions.ApplyProtectionFormatting:
					this.UpdateUseProtectionFormattingFlag();
					break;

				default:
					Utilities.DebugFail("Unknown StyleCellFormatOptions: " + associatedStyleOption);
					break;
			}

			if (_valueChangeCallback != null)
				_valueChangeCallback(associatedStyleOption);
		}

		#endregion // OnValueSet

		#region StyleOptionForValue

		private static WorksheetCellFormatOptions StyleOptionForValue(CellFormatValue value)
		{
			switch (value)
			{
				case CellFormatValue.Alignment:
				case CellFormatValue.Indent:
				case CellFormatValue.Rotation:
				case CellFormatValue.ShrinkToFit:
				case CellFormatValue.VerticalAlignment:
				case CellFormatValue.WrapText:
					return WorksheetCellFormatOptions.ApplyAlignmentFormatting;

				case CellFormatValue.Fill:
					return WorksheetCellFormatOptions.ApplyFillFormatting;

				case CellFormatValue.BottomBorderColorInfo:
				case CellFormatValue.BottomBorderStyle:
				case CellFormatValue.DiagonalBorderColorInfo:
				case CellFormatValue.DiagonalBorders:
				case CellFormatValue.DiagonalBorderStyle:
				case CellFormatValue.LeftBorderColorInfo:
				case CellFormatValue.LeftBorderStyle:
				case CellFormatValue.RightBorderColorInfo:
				case CellFormatValue.RightBorderStyle:
				case CellFormatValue.TopBorderColorInfo:
				case CellFormatValue.TopBorderStyle:
					return WorksheetCellFormatOptions.ApplyBorderFormatting;

				case CellFormatValue.FormatString:
					return WorksheetCellFormatOptions.ApplyNumberFormatting;

				case CellFormatValue.Locked:
					return WorksheetCellFormatOptions.ApplyProtectionFormatting;

				case CellFormatValue.FontBold:
				case CellFormatValue.FontColorInfo:
				case CellFormatValue.FontHeight:
				case CellFormatValue.FontItalic:
				case CellFormatValue.FontName:
				case CellFormatValue.FontStrikeout:
				case CellFormatValue.FontSuperscriptSubscriptStyle:
				case CellFormatValue.FontUnderlineStyle:
					return WorksheetCellFormatOptions.ApplyFontFormatting;

				case CellFormatValue.Style:
				case CellFormatValue.FormatOptions:
					Utilities.DebugFail("This method shouldn't be called for this CellFormatValue: " + value);
					return WorksheetCellFormatOptions.None;

				default:
					Utilities.DebugFail("Unknown CellFormatValue: " + value);
					return WorksheetCellFormatOptions.None;
			}
		}

		#endregion // StyleOptionForValue

		#region ResetFormatOptions

		internal void ResetFormatOptions(WorksheetCellFormatOptions formatOptionsToReset)
		{
			if (formatOptionsToReset == WorksheetCellFormatOptions.None)
				return;

			if (Utilities.TestFlag(formatOptionsToReset, WorksheetCellFormatOptions.ApplyNumberFormatting))
			{
				_formatStringIndex = -1;

				if (_valueChangeCallback != null)
					_valueChangeCallback(WorksheetCellFormatOptions.ApplyNumberFormatting);
			}

			if (Utilities.TestFlag(formatOptionsToReset, WorksheetCellFormatOptions.ApplyAlignmentFormatting))
			{
				_alignment = HorizontalCellAlignment.Default;
				_indent = -1;
				_rotation = -1;
				_shrinkToFit = ExcelDefaultableBoolean.Default;
				_wrapText = ExcelDefaultableBoolean.Default;
				_verticalAlignment = VerticalCellAlignment.Default;

				if (_valueChangeCallback != null)
					_valueChangeCallback(WorksheetCellFormatOptions.ApplyAlignmentFormatting);
			}

			if (Utilities.TestFlag(formatOptionsToReset, WorksheetCellFormatOptions.ApplyFontFormatting))
			{
				if (_font == null)
					_font = new WorkbookFontProxy(new WorkbookFontData(this.Workbook), this.Workbook, this);
				else
					_font.SetFontFormatting(new WorkbookFontData(this.Workbook));

				if (_valueChangeCallback != null)
					_valueChangeCallback(WorksheetCellFormatOptions.ApplyFontFormatting);
			}

			if (Utilities.TestFlag(formatOptionsToReset, WorksheetCellFormatOptions.ApplyBorderFormatting))
			{
				_bottomBorderColorInfo = null;
				_bottomBorderStyle = CellBorderLineStyle.Default;
				_diagonalBorderColorInfo = null;
				_diagonalBorders = DiagonalBorders.Default;
				_diagonalBorderStyle = CellBorderLineStyle.Default;
				_leftBorderColorInfo = null;
				_leftBorderStyle = CellBorderLineStyle.Default;
				_rightBorderColorInfo = null;
				_rightBorderStyle = CellBorderLineStyle.Default;
				_topBorderColorInfo = null;
				_topBorderStyle = CellBorderLineStyle.Default;

				if (_valueChangeCallback != null)
					_valueChangeCallback(WorksheetCellFormatOptions.ApplyBorderFormatting);
			}

			if (Utilities.TestFlag(formatOptionsToReset, WorksheetCellFormatOptions.ApplyFillFormatting))
			{
				_fill = null;

				if (_valueChangeCallback != null)
					_valueChangeCallback(WorksheetCellFormatOptions.ApplyFillFormatting);
			}

			if (Utilities.TestFlag(formatOptionsToReset, WorksheetCellFormatOptions.ApplyProtectionFormatting))
			{
				_locked = ExcelDefaultableBoolean.Default;

				if (_valueChangeCallback != null)
					_valueChangeCallback(WorksheetCellFormatOptions.ApplyProtectionFormatting);
			}

			_cachedHashCode = null;

			Debug.Assert((this.FormatOptions & formatOptionsToReset) == WorksheetCellFormatOptions.None, "Not all format options were reset.");

			// Strip out all format options to reset from the cached style values.
			if (_cachedStyleValues != null)
			{
				_cachedStyleValues.FormatOptions &= ~formatOptionsToReset;

				if (_cachedStyleValues.FormatOptions == WorksheetCellFormatOptions.None)
					_cachedStyleValues = null;
			}
		}

		#endregion // ResetFormatOptions

		#region UpdateUseAlignmentFormattingFlag

		private void UpdateUseAlignmentFormattingFlag()
		{
			bool useAlignmentFormatting =
				this.Alignment != HorizontalCellAlignment.Default ||
				this.Indent != -1 ||
				this.Rotation != -1 ||
				this.ShrinkToFit != ExcelDefaultableBoolean.Default ||
				this.VerticalAlignment != VerticalCellAlignment.Default ||
				this.WrapText != ExcelDefaultableBoolean.Default;

			if (useAlignmentFormatting)
				this.FormatOptions |= WorksheetCellFormatOptions.ApplyAlignmentFormatting;
		}

		#endregion // UpdateUseAlignmentFormattingFlag

		#region UpdateUseBorderFormattingFlag

		private void UpdateUseBorderFormattingFlag()
		{
			if (this.IsAnyBorderPropertySet())
				this.FormatOptions |= WorksheetCellFormatOptions.ApplyBorderFormatting;
		}

		#endregion // UpdateUseBorderFormattingFlag

		#region UpdateUseFontFormattingFlag

		private void UpdateUseFontFormattingFlag()
		{
			bool useFontFormatting = (this.FontInternal.Element.IsEmpty == false);

			if (useFontFormatting)
				this.FormatOptions |= WorksheetCellFormatOptions.ApplyFontFormatting;
		}

		#endregion // UpdateUseFontFormattingFlag

		#region UpdateUseNumberFormattingFlag

		private void UpdateUseNumberFormattingFlag()
		{
			bool useNumberFormatting =
				this.FormatString != null;

			if (useNumberFormatting)
				this.FormatOptions |= WorksheetCellFormatOptions.ApplyNumberFormatting;
		}

		#endregion // UpdateUseNumberFormattingFlag

		#region UpdateUsePatternsFormattingFlag

		private void UpdateUsePatternsFormattingFlag()
		{
			bool usePatternsFormatting =
				this.Fill != null;

			if (usePatternsFormatting)
				this.FormatOptions |= WorksheetCellFormatOptions.ApplyFillFormatting;
		}

		#endregion // UpdateUsePatternsFormattingFlag

		#region UpdateUseProtectionFormattingFlag

		private void UpdateUseProtectionFormattingFlag()
		{
			bool useProtectionFormatting =
				this.Locked != ExcelDefaultableBoolean.Default;

			if (useProtectionFormatting)
				this.FormatOptions |= WorksheetCellFormatOptions.ApplyProtectionFormatting;
		}

		#endregion // UpdateUseProtectionFormattingFlag

		#region VerifyFontColorInfoForSave

		private WorkbookColorInfo VerifyFontColorInfoForSave(WorkbookColorInfo colorInfo)
		{
			// MD 3/15/12
			// Found while fixing TFS104581
			// Certain system colors have indexes which are valid for shapes text fonts, but invalid to cell format fonts,
			// so convert them to normal colors if we have one of those.
			if (colorInfo != null && this.Workbook != null)
			{
				Color color = colorInfo.GetResolvedColor(this.Workbook);
				int index = this.Workbook.Palette.FindIndex(color, ColorableItem.CellFont);
				if (0x42 <= index && index <= 0x51)
				{
					int argb = Utilities.ColorToArgb(color);
					return new WorkbookColorInfo(Utilities.ColorFromArgb(argb));
				}
			}

			return colorInfo;
		}

		#endregion // VerifyFontColorInfoForSave

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Alignment

		public HorizontalCellAlignment Alignment
		{
			get { return _alignment; }
			set
			{
				this.VerifyCanSetValue();

				if (this.Alignment == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( HorizontalCellAlignment ), value ) == false )
				if (Utilities.IsHorizontalCellAlignmentDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(HorizontalCellAlignment));

				_alignment = value;

				// MD 3/21/12 - TFS104630
				// The AddIndent value can only be True when the Alignment is Distributed.
				if (_alignment != HorizontalCellAlignment.Distributed)
					this.AddIndent = false;

				this.OnValueSet(CellFormatValue.Alignment);
			}
		}

		#endregion Alignment

		#region BottomBorderColorInfo

		public WorkbookColorInfo BottomBorderColorInfo
		{
			get
			{
				return _bottomBorderColorInfo;
			}
			set
			{
				this.VerifyCanSetValue();

				if (this.BottomBorderColorInfo == value)
					return;

				WorksheetCellFormatData.ConvertColorForBorder(ref value);
				_bottomBorderColorInfo = value;
				this.OnValueSet(CellFormatValue.BottomBorderColorInfo);
			}
		}

		#endregion BottomBorderColorInfo

		#region BottomBorderStyle

		public CellBorderLineStyle BottomBorderStyle
		{
			get { return _bottomBorderStyle; }
			set
			{
				this.VerifyCanSetValue();

				if (this.BottomBorderStyle == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( CellBorderLineStyle ), value ) == false )
				if (Utilities.IsCellBorderLineStyleDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(CellBorderLineStyle));

				_bottomBorderStyle = value;
				this.OnValueSet(CellFormatValue.BottomBorderStyle);
			}
		}

		#endregion BottomBorderStyle

		#region DiagonalBorderColorInfo

		public WorkbookColorInfo DiagonalBorderColorInfo
		{
			get
			{
				return _diagonalBorderColorInfo;
			}
			set
			{
				this.VerifyCanSetValue();

				if (this.DiagonalBorderColorInfo == value)
					return;

				WorksheetCellFormatData.ConvertColorForBorder(ref value);
				_diagonalBorderColorInfo = value;
				this.OnValueSet(CellFormatValue.DiagonalBorderColorInfo);
			}
		}

		#endregion DiagonalBorderColorInfo

		// MD 10/26/11 - TFS91546
		#region DiagonalBorders

		public DiagonalBorders DiagonalBorders
		{
			get { return _diagonalBorders; }
			set
			{
				this.VerifyCanSetValue();

				if (this.DiagonalBorders == value)
					return;

				if (Enum.IsDefined(typeof(DiagonalBorders), value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(DiagonalBorders));

				_previousDiagonalBorders = _diagonalBorders;
				_diagonalBorders = value;

				this.OnValueSet(CellFormatValue.DiagonalBorders);
			}
		}

		#endregion  // DiagonalBorders

		// MD 10/26/11 - TFS91546
		#region DiagonalBorderStyle

		public CellBorderLineStyle DiagonalBorderStyle
		{
			get { return _diagonalBorderStyle; }
			set
			{
				this.VerifyCanSetValue();

				if (this.DiagonalBorderStyle == value)
					return;

				if (Utilities.IsCellBorderLineStyleDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(CellBorderLineStyle));

				_diagonalBorderStyle = value;
				this.OnValueSet(CellFormatValue.DiagonalBorderStyle);
			}
		}

		#endregion  // DiagonalBorderStyle

		#region Fill

		public CellFill Fill
		{
			get { return _fill; }
			set
			{
				this.VerifyCanSetValue();

				if (this.Fill == value)
					return;

				_fill = value;
				this.OnValueSet(CellFormatValue.Fill);
			}
		}

		#endregion Fill

		#region Font

		public IWorkbookFont Font
		{
			get { return this.FontInternal; }
		}

		#endregion Font

		#region FormatOptions

		public WorksheetCellFormatOptions FormatOptions
		{
			get { return _formatOptions; }
			set
			{
				this.VerifyCanSetValue();

				// Strip out all invalid bits.
				value &= WorksheetCellFormatOptions.All;

				if (_formatOptions == value)
					return;

				WorksheetCellFormatOptions oldFormatOptions = _formatOptions;
				this.SetFormatOptionsInternal(value);

				this.OnFormatOptionsChanged(oldFormatOptions);

				// MD 2/16/12 - TFS101528
				// If the FormatOptions change, we should notify a listener that something changed.
				if (_valueChangeCallback != null)
					_valueChangeCallback(null);
			}
		}

		internal void SetFormatOptionsInternal(WorksheetCellFormatOptions value)
		{
			_formatOptions = value;
			_cachedHashCode = null;

			// MD 2/16/12 - TFS101528
			// If the FormatOptions change, we should notify a listener that something changed.
			if (_valueChangeCallback != null)
				_valueChangeCallback(null);
		}

		private void OnFormatOptionsChanged(WorksheetCellFormatOptions oldFormatOptions)
		{
			WorksheetCellFormatOptions formatOptionsToReset = WorksheetCellFormatOptions.None;
			WorksheetCellFormatOptions formatOptionsToCopyFromStyle = WorksheetCellFormatOptions.None;

			this.GetChangesRequiredForChangedFormatOption(oldFormatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting,
				ref formatOptionsToReset, ref formatOptionsToCopyFromStyle);
			this.GetChangesRequiredForChangedFormatOption(oldFormatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting,
				ref formatOptionsToReset, ref formatOptionsToCopyFromStyle);
			this.GetChangesRequiredForChangedFormatOption(oldFormatOptions, WorksheetCellFormatOptions.ApplyFontFormatting,
				ref formatOptionsToReset, ref formatOptionsToCopyFromStyle);
			this.GetChangesRequiredForChangedFormatOption(oldFormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting,
				ref formatOptionsToReset, ref formatOptionsToCopyFromStyle);
			this.GetChangesRequiredForChangedFormatOption(oldFormatOptions, WorksheetCellFormatOptions.ApplyFillFormatting,
				ref formatOptionsToReset, ref formatOptionsToCopyFromStyle);
			this.GetChangesRequiredForChangedFormatOption(oldFormatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting,
				ref formatOptionsToReset, ref formatOptionsToCopyFromStyle);

			// If any format options are removed, reset those properties
			this.ResetFormatOptions(formatOptionsToReset);

			// When the first property in a format options group is set, we need to copy all other properties in that group from the style to this format.
			// That way, if properties from the group on the style are changed, the cell won't pick them up. For example, if the parent style has italic 
			// text, the cell format is bolded, and then the italic setting is removed from the style, the cell will still have bold, italic text in Excel.
			if (this.Style != null)
				this.CacheResolvedFormatOptionsValues(formatOptionsToCopyFromStyle, this.Style.StyleFormatInternal);
		}

		#endregion FormatOptions

		#region FormatString

		public string FormatString
		{
			get
			{
				// MD 2/29/12 - 12.1 - Table Support
				if (this.Workbook == null)
					return null;

				return this.Workbook.Formats[_formatStringIndex];
			}
			set
			{
				this.VerifyCanSetValue();

				if (this.FormatString == value)
					return;

				if (value != null)
				{
					// Validate the format string if necessary.
					Workbook workbook = this.Workbook;
					if (workbook != null)
					{
						if (workbook.ValidateFormatStrings)
						{
							// MD 4/9/12 - TFS101506
							//ValueFormatter valueFormatter = new ValueFormatter(workbook, value);
							ValueFormatter valueFormatter = new ValueFormatter(workbook, value, workbook.CultureResolved);

							if (valueFormatter.IsValid == false)
							{
								if (workbook.IsLoading)
									Utilities.DebugFail("This is unexpected.");
								else
									throw new ArgumentException(SR.GetString("LE_InvalidOperationException_InvalidFormatString"), "value");
							}
						}

						_formatStringIndex = workbook.Formats.Add(value);
					}
					else
					{
						Utilities.DebugFail("This is unexpected.");
					}
				}
				else
				{
					_formatStringIndex = -1;
				}

				this.OnValueSet(CellFormatValue.FormatString);
			}
		}

		#endregion FormatString

		#region Indent

		public int Indent
		{
			get { return _indent; }
			set
			{
				this.VerifyCanSetValue();

				if (this.Indent == value)
					return;

				if (value != -1 && (value < 0 || WorksheetCellFormatData.MaxIndent2007 < value))
					throw new ArgumentOutOfRangeException("value", value, SR.GetString("LE_ArgumentOutOfRangeException_Indent"));

				_indent = value;
				this.OnValueSet(CellFormatValue.Indent);
			}
		}

		#endregion Indent

		#region LeftBorderColorInfo

		public WorkbookColorInfo LeftBorderColorInfo
		{
			get
			{
				return _leftBorderColorInfo;
			}
			set
			{
				this.VerifyCanSetValue();

				if (this.LeftBorderColorInfo == value)
					return;

				WorksheetCellFormatData.ConvertColorForBorder(ref value);
				_leftBorderColorInfo = value;
				this.OnValueSet(CellFormatValue.LeftBorderColorInfo);
			}
		}

		#endregion LeftBorderColorInfo

		#region LeftBorderStyle

		public CellBorderLineStyle LeftBorderStyle
		{
			get { return _leftBorderStyle; }
			set
			{
				this.VerifyCanSetValue();

				if (this.LeftBorderStyle == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( CellBorderLineStyle ), value ) == false )
				if (Utilities.IsCellBorderLineStyleDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(CellBorderLineStyle));

				_leftBorderStyle = value;
				this.OnValueSet(CellFormatValue.LeftBorderStyle);
			}
		}

		#endregion LeftBorderStyle

		#region Locked

		public ExcelDefaultableBoolean Locked
		{
			get { return _locked; }
			set
			{
				this.VerifyCanSetValue();

				if (this.Locked == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
				if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ExcelDefaultableBoolean));

				_locked = value;
				this.OnValueSet(CellFormatValue.Locked);
			}
		}

		#endregion Locked

		#region RightBorderColorInfo

		public WorkbookColorInfo RightBorderColorInfo
		{
			get
			{
				return _rightBorderColorInfo;
			}
			set
			{
				this.VerifyCanSetValue();

				if (this.RightBorderColorInfo == value)
					return;

				WorksheetCellFormatData.ConvertColorForBorder(ref value);
				_rightBorderColorInfo = value;
				this.OnValueSet(CellFormatValue.RightBorderColorInfo);
			}
		}

		#endregion RightBorderColorInfo

		#region RightBorderStyle

		public CellBorderLineStyle RightBorderStyle
		{
			get { return _rightBorderStyle; }
			set
			{
				this.VerifyCanSetValue();

				if (this.RightBorderStyle == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( CellBorderLineStyle ), value ) == false )
				if (Utilities.IsCellBorderLineStyleDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(CellBorderLineStyle));

				_rightBorderStyle = value;
				this.OnValueSet(CellFormatValue.RightBorderStyle);
			}
		}

		#endregion RightBorderStyle

		#region Rotation

		public int Rotation
		{
			get { return _rotation; }
			set
			{
				this.VerifyCanSetValue();

				if (this.Rotation == value)
					return;

				_rotation = value;
				this.OnValueSet(CellFormatValue.Rotation);
			}
		}

		#endregion Rotation

		#region ShrinkToFit

		public ExcelDefaultableBoolean ShrinkToFit
		{
			get { return _shrinkToFit; }
			set
			{
				this.VerifyCanSetValue();

				if (this.ShrinkToFit == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
				if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ExcelDefaultableBoolean));

				_shrinkToFit = value;
				this.OnValueSet(CellFormatValue.ShrinkToFit);
			}
		}

		#endregion ShrinkToFit

		#region Style

		public WorkbookStyle Style
		{
			get { return _style; }
			set
			{
				this.VerifyCanSetValue();

				// If the specified style is null and this is not a style format, default it to the normal style.
				if (value == null && this.Type != WorksheetCellFormatType.StyleFormat && this.Workbook != null)
					value = this.Workbook.Styles.NormalStyle;

				if (this.Style == value)
					return;

				if (value != null)
				{
					if (this.Type == WorksheetCellFormatType.StyleFormat)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotSetParentStyleOnStyle"));

					if (value.Workbook != this.Workbook)
						throw new ArgumentException(SR.GetString("LE_InvalidOperationException_ParentStyleFromOtherWorkbook"), "value");
				}

				if (this.Style != null && this.Collection != null)
					this.Style.OnChildFormatDetached();

				// When the format options differ between the previous style and the new one, we want to copy the style properties from
				// the previous style which are not set by the new style. But don't no this with the normal style.
				if (this.Style != null && value != null && this.Workbook.IsLoading == false)
				{
					WorksheetCellFormatOptions optionsToCopyToFormat = this.Style.StyleFormatInternal.FormatOptions;

					// Remove any format options applied by the new style
					optionsToCopyToFormat &= ~value.StyleFormatInternal.FormatOptions;

					// Remove any format options which have all ultimate default properties
					optionsToCopyToFormat = this.FilterOutOptionsWithAllUltimateDefaults(optionsToCopyToFormat);

					this.CacheResolvedFormatOptionsValues(optionsToCopyToFormat, this.Style.StyleFormatInternal);
				}

				_style = value;

				if (this.Style != null && this.Collection != null)
					this.Style.OnChildFormatAttached();

				// When the style changes and we are not loading, all format properties which are set on the style are reset here.
				if (this.Style != null &&
					(this.Workbook == null || this.Workbook.IsLoading == false))
				{
					this.FormatOptions &= ~this.Style.StyleFormatInternal.FormatOptions;
				}

				// MD 2/16/12 - TFS101528
				// If the Style changes, we should notify a listener that something changed.
				if (_valueChangeCallback != null)
					_valueChangeCallback(null);
			}
		}

		internal void SetStyleInternal(WorkbookStyle style)
		{
			_style = style;
			_cachedHashCode = null;

			if (_valueChangeCallback != null)
				_valueChangeCallback(null);
		}

		#endregion Style

		#region TopBorderColorInfo

		public WorkbookColorInfo TopBorderColorInfo
		{
			get
			{
				return _topBorderColorInfo;
			}
			set
			{
				this.VerifyCanSetValue();

				if (this.TopBorderColorInfo == value)
					return;

				WorksheetCellFormatData.ConvertColorForBorder(ref value);
				_topBorderColorInfo = value;
				this.OnValueSet(CellFormatValue.TopBorderColorInfo);
			}
		}

		#endregion TopBorderColorInfo

		#region TopBorderStyle

		public CellBorderLineStyle TopBorderStyle
		{
			get { return _topBorderStyle; }
			set
			{
				this.VerifyCanSetValue();

				if (_topBorderStyle == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( CellBorderLineStyle ), value ) == false )
				if (Utilities.IsCellBorderLineStyleDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(CellBorderLineStyle));

				_topBorderStyle = value;
				this.OnValueSet(CellFormatValue.TopBorderStyle);
			}
		}

		#endregion TopBorderStyle

		#region VerticalAlignment

		public VerticalCellAlignment VerticalAlignment
		{
			get { return _verticalAlignment; }
			set
			{
				this.VerifyCanSetValue();

				if (_verticalAlignment == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( VerticalCellAlignment ), value ) == false )
				if (Utilities.IsVerticalCellAlignmentDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(VerticalCellAlignment));

				_verticalAlignment = value;
				this.OnValueSet(CellFormatValue.VerticalAlignment);
			}
		}

		#endregion VerticalAlignment

		#region WrapText

		public ExcelDefaultableBoolean WrapText
		{
			get { return _wrapText; }
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				this.VerifyCanSetValue();

				if (_wrapText == value)
					return;

				// MD 10/21/10 - TFS34398
				// Use the utility function instead of Enum.IsDefined. It is faster.
				//if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
				if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ExcelDefaultableBoolean));

				_wrapText = value;
				this.OnValueSet(CellFormatValue.WrapText);
			}
		}

		#endregion WrapText

		#endregion Public Properties

		#region Resolved Properties

		#region AlignmentResolved

		internal HorizontalCellAlignment AlignmentResolved
		{
			get
			{
				return (HorizontalCellAlignment)this.GetResolvedValue(CellFormatValue.Alignment);
			}
		}

		#endregion // AlignmentResolved

		#region BottomBorderColorInfoResolved

		internal WorkbookColorInfo BottomBorderColorInfoResolved
		{
			get
			{
				return (WorkbookColorInfo)this.GetResolvedValue(CellFormatValue.BottomBorderColorInfo);
			}
		}

		#endregion // BottomBorderColorInfoResolved

		#region BottomBorderStyleResolved

		internal CellBorderLineStyle BottomBorderStyleResolved
		{
			get
			{
				return (CellBorderLineStyle)this.GetResolvedValue(CellFormatValue.BottomBorderStyle);
			}
		}

		#endregion // BottomBorderStyleResolved

		#region DiagonalBorderColorInfoResolved

		internal WorkbookColorInfo DiagonalBorderColorInfoResolved
		{
			get
			{
				return (WorkbookColorInfo)this.GetResolvedValue(CellFormatValue.DiagonalBorderColorInfo);
			}
		}

		#endregion // DiagonalBorderColorInfoResolved

		#region DiagonalBordersResolved

		internal DiagonalBorders DiagonalBordersResolved
		{
			get
			{
				return (DiagonalBorders)this.GetResolvedValue(CellFormatValue.DiagonalBorders);
			}
		}

		#endregion // DiagonalBordersResolved

		#region DiagonalBorderStyleResolved

		internal CellBorderLineStyle DiagonalBorderStyleResolved
		{
			get
			{
				return (CellBorderLineStyle)this.GetResolvedValue(CellFormatValue.DiagonalBorderStyle);
			}
		}

		#endregion // DiagonalBorderStyleResolved

		#region FillResolved

		internal CellFill FillResolved
		{
			get
			{
				return (CellFill)this.GetResolvedValue(CellFormatValue.Fill);
			}
		}

		#endregion // FillPatternResolved

		#region FontBoldResolved

		internal ExcelDefaultableBoolean FontBoldResolved
		{
			get
			{
				return (ExcelDefaultableBoolean)this.GetResolvedValue(CellFormatValue.FontBold);
			}
		}

		#endregion // FontBoldResolved

		#region FontColorInfoResolved

		internal WorkbookColorInfo FontColorInfoResolved
		{
			get
			{
				return (WorkbookColorInfo)this.GetResolvedValue(CellFormatValue.FontColorInfo);
			}
		}

		#endregion // FontColorInfoResolved

		#region FontHeightResolved

		internal int FontHeightResolved
		{
			get
			{
				return (int)this.GetResolvedValue(CellFormatValue.FontHeight);
			}
		}

		#endregion // FontHeightResolved

		#region FontItalicResolved

		internal ExcelDefaultableBoolean FontItalicResolved
		{
			get
			{
				return (ExcelDefaultableBoolean)this.GetResolvedValue(CellFormatValue.FontItalic);
			}
		}

		#endregion // FontItalicResolved

		#region FontNameResolved

		internal string FontNameResolved
		{
			get
			{
				return (string)this.GetResolvedValue(CellFormatValue.FontName);
			}
		}

		#endregion // FontNameResolved

		#region FontStrikeoutResolved

		internal ExcelDefaultableBoolean FontStrikeoutResolved
		{
			get
			{
				return (ExcelDefaultableBoolean)this.GetResolvedValue(CellFormatValue.FontStrikeout);
			}
		}

		#endregion // FontStrikeoutResolved

		#region FontSuperscriptSubscriptStyleResolved

		internal FontSuperscriptSubscriptStyle FontSuperscriptSubscriptStyleResolved
		{
			get
			{
				return (FontSuperscriptSubscriptStyle)this.GetResolvedValue(CellFormatValue.FontSuperscriptSubscriptStyle);
			}
		}

		#endregion // FontSuperscriptSubscriptStyleResolved

		#region FontUnderlineStyleResolved

		internal FontUnderlineStyle FontUnderlineStyleResolved
		{
			get
			{
				return (FontUnderlineStyle)this.GetResolvedValue(CellFormatValue.FontUnderlineStyle);
			}
		}

		#endregion // FontUnderlineStyleResolved

		#region FormatOptionsResolved

		internal WorksheetCellFormatOptions FormatOptionsResolved
		{
			get
			{
				return (WorksheetCellFormatOptions)this.GetResolvedValue(CellFormatValue.FormatOptions);
			}
		}

		#endregion // FormatOptionsResolved

		#region FormatStringIndexResolved

		internal int FormatStringIndexResolved
		{
			get
			{
				if (_formatStringIndex < 0)
				{
					if (Utilities.TestFlag(this.FormatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting))
					{
						if (_cachedStyleValues != null)
							return _cachedStyleValues.FormatStringIndexResolved;
					}
					else
					{
						if (this.Style != null)
							return this.Style.StyleFormatInternal.FormatStringIndexResolved;
					}

					return 0;
				}

				return _formatStringIndex;
			}
		}

		#endregion FormatStringIndexResolved

		#region FormatStringResolved

		internal string FormatStringResolved
		{
			get
			{
				return (string)this.GetResolvedValue(CellFormatValue.FormatString);
			}
		}

		#endregion // FormatStringResolved

		#region IndentResolved

		internal int IndentResolved
		{
			get
			{
				return (int)this.GetResolvedValue(CellFormatValue.Indent);
			}
		}

		#endregion // IndentResolved

		#region LeftBorderColorInfoResolved

		internal WorkbookColorInfo LeftBorderColorInfoResolved
		{
			get
			{
				return (WorkbookColorInfo)this.GetResolvedValue(CellFormatValue.LeftBorderColorInfo);
			}
		}

		#endregion // LeftBorderColorInfoResolved

		#region LeftBorderStyleResolved

		internal CellBorderLineStyle LeftBorderStyleResolved
		{
			get
			{
				return (CellBorderLineStyle)this.GetResolvedValue(CellFormatValue.LeftBorderStyle);
			}
		}

		#endregion // LeftBorderStyleResolved

		#region LockedResolved

		internal ExcelDefaultableBoolean LockedResolved
		{
			get
			{
				return (ExcelDefaultableBoolean)this.GetResolvedValue(CellFormatValue.Locked);
			}
		}

		#endregion // LockedResolved

		#region RightBorderColorInfoResolved

		internal WorkbookColorInfo RightBorderColorInfoResolved
		{
			get
			{
				return (WorkbookColorInfo)this.GetResolvedValue(CellFormatValue.RightBorderColorInfo);
			}
		}

		#endregion // RightBorderColorInfoResolved

		#region RightBorderStyleResolved

		internal CellBorderLineStyle RightBorderStyleResolved
		{
			get
			{
				return (CellBorderLineStyle)this.GetResolvedValue(CellFormatValue.RightBorderStyle);
			}
		}

		#endregion // RightBorderStyleResolved

		#region RotationResolved

		internal int RotationResolved
		{
			get
			{
				return (int)this.GetResolvedValue(CellFormatValue.Rotation);
			}
		}

		#endregion // RotationResolved

		#region ShrinkToFitResolved

		internal ExcelDefaultableBoolean ShrinkToFitResolved
		{
			get
			{
				return (ExcelDefaultableBoolean)this.GetResolvedValue(CellFormatValue.ShrinkToFit);
			}
		}

		#endregion // ShrinkToFitResolved

		#region StyleResolved

		internal WorkbookStyle StyleResolved
		{
			get
			{
				return (WorkbookStyle)this.GetResolvedValue(CellFormatValue.Style);
			}
		}

		#endregion // StyleResolved

		#region TopBorderColorInfoResolved

		internal WorkbookColorInfo TopBorderColorInfoResolved
		{
			get
			{
				return (WorkbookColorInfo)this.GetResolvedValue(CellFormatValue.TopBorderColorInfo);
			}
		}

		#endregion // TopBorderColorInfoResolved

		#region TopBorderStyleResolved

		internal CellBorderLineStyle TopBorderStyleResolved
		{
			get
			{
				return (CellBorderLineStyle)this.GetResolvedValue(CellFormatValue.TopBorderStyle);
			}
		}

		#endregion // TopBorderStyleResolved

		#region VerticalAlignmentResolved

		internal VerticalCellAlignment VerticalAlignmentResolved
		{
			get
			{
				return (VerticalCellAlignment)this.GetResolvedValue(CellFormatValue.VerticalAlignment);
			}
		}

		#endregion // VerticalAlignmentResolved

		#region WrapTextResolved

		internal ExcelDefaultableBoolean WrapTextResolved
		{
			get
			{
				return (ExcelDefaultableBoolean)this.GetResolvedValue(CellFormatValue.WrapText);
			}
		}

		#endregion // WrapTextResolved

		#endregion // Resolved Properties

		#region Internal Properties

		// MD 3/21/12 - TFS104630
		#region AddIndent

		internal bool AddIndent
		{
			get { return _addIndent; }
			set { _addIndent = value; }
		}

		#endregion // AddIndent

		#region AllCellFormatValues

		internal static IList<CellFormatValue> AllCellFormatValues
		{
			get
			{
				if (_allCellFormatValues == null)
				{
					_allCellFormatValues = new ReadOnlyCollection<CellFormatValue>(
						(CellFormatValue[])CalcManagerUtilities.EnumGetValues(typeof(CellFormatValue))
						);
				}

				return _allCellFormatValues;
			}
		}

		#endregion // AllCellFormatValues

		#region DoesReverseColorsForSolidFill

		internal bool DoesReverseColorsForSolidFill
		{
			get
			{
				return
					_isForFillOrSortCondition ||
					_type != WorksheetCellFormatType.DifferentialFormat;
			}
		}

		#endregion // DoesReverseColorsForSolidFill

		#region FontInternal

		internal WorkbookFontProxy FontInternal
		{
			get { return _font; }
		}

		#endregion FontInternal

		#region FontSchemeInternal

		internal FontScheme FontSchemeInternal
		{
			get { return _fontScheme; }
			set { _fontScheme = value; }
		}

		#endregion // FontSchemeInternal

		#region FormatStringIndex

		internal int FormatStringIndex
		{
			get { return _formatStringIndex; }
			set
			{
				_formatStringIndex = value;
				this.OnValueSet(CellFormatValue.FormatString);
			}
		}

		#endregion FormatStringIndex

		#region IsEmpty

		internal bool IsEmpty
		{
			get
			{
				// MD 4/4/12 - TFS107975
				// The default element may have some format options, so we should check for equality wiht the defualt 
				// element when this is a cell format.
				if (this.Type == WorksheetCellFormatType.CellFormat && this.Workbook != null)
					return this.Workbook.CellFormats.DefaultElement.Equals(this);

				if (this.FormatOptions != WorksheetCellFormatOptions.None)
					return false;

				if (this.Style != null && this.Style.IsNormalStyle == false)
					return false;

				return true;
			}
		}

		#endregion // IsEmpty

		#region IsForFillOrSortCondition

		internal bool IsForFillOrSortCondition
		{
			get { return _isForFillOrSortCondition; }
			set { _isForFillOrSortCondition = value; }
		}

		#endregion // IsForFillOrSortCondition

		#region Type

		internal WorksheetCellFormatType Type
		{
			get { return _type; }
			set
			{
				if (_type == value)
					return;

				_type = value;
				_cachedHashCode = null;

				// Reset the Style when IsStyle changes.
				this.Style = null;
			}
		}

		#endregion Type

		// MD 10/26/11 - TFS91546
		#region PreviousDiagonalBorders

		internal DiagonalBorders PreviousDiagonalBorders
		{
			get { return _previousDiagonalBorders; }
		}

		#endregion  // PreviousDiagonalBorders

		#region ValueChangeCallback

		internal CellFormatValueChangeCallback ValueChangeCallback
		{
			get { return _valueChangeCallback; }
			set { _valueChangeCallback = value; }
		}

		#endregion // ValueChangeCallback

		#endregion Internal Properties

		#endregion Properties
	}

	internal enum WorksheetCellFormatType : byte
	{
		CellFormat,
		StyleFormat,
		DifferentialFormat
	}

	internal delegate void CellFormatValueChangeCallback(WorksheetCellFormatOptions? associatedStyleOption);
	internal delegate bool GetAdjacentBorderValueCallback(CellFormatValue borderProperty, out object value);
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