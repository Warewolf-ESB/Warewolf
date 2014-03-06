// MD 1/16/12 - 12.1 - Cell Format Updates
// Removed the WorkbookColorCollection. This has been replaced by the WorkbookColorPalette class.
#region Removed

//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Collections;
//using System.Diagnostics;
//using System.Globalization;

//#if SILVERLIGHT
//using System.Windows.Media;
//using System.Windows;
//using Infragistics.Collections;
//#else
//using System.Drawing;
//using Infragistics.Shared;
//#endif

//namespace Infragistics.Documents.Excel
//{
//#if DEBUG
//    /// <summary>
//    /// Represents the color palette which contains all colors used by a workbook.
//    /// </summary> 
//#endif
//#if SILVERLIGHT
//    internal class WorkbookColorCollection : CollectionBase<Color>
//#else
//    internal class WorkbookColorCollection : CollectionBase
//#endif 
//    {
//        #region Constants

//        internal const int AutomaticColor = 64;
//        internal const int UserPaletteStart = 8;		

//        // MRS 6/28/05 - BR04756
//        // These are the standard Excel Palette colors
//        #region excelStandardColors
//        private static Color[] excelStandardColors = new Color[]
//            {
//                Color.FromArgb(255, 0, 0, 0),			// Black
//                Color.FromArgb(255, 255, 255, 255),		// White
//                Color.FromArgb(255, 255, 0, 0),			// Red
//                Color.FromArgb(255, 0, 255, 0),			// Green
//                Color.FromArgb(255, 0, 0, 255),			// Blue
//                Color.FromArgb(255, 255, 255, 0),		// Yellow
//                Color.FromArgb(255, 255, 0, 255),		// Magenta
//                Color.FromArgb(255, 0, 255, 255),		// Cyan
//                Color.FromArgb(255, 128, 0, 0),
//                Color.FromArgb(255, 0, 128, 0),
//                Color.FromArgb(255, 0, 0, 128),
//                Color.FromArgb(255, 128, 128, 0),
//                Color.FromArgb(255, 128, 0, 128),
//                Color.FromArgb(255, 0, 128, 128),
//                Color.FromArgb(255, 192, 192, 192),
//                Color.FromArgb(255, 128, 128, 128),
//                Color.FromArgb(255, 153, 153, 255),
//                Color.FromArgb(255, 153, 51, 102),
//                Color.FromArgb(255, 255, 255, 204),
//                Color.FromArgb(255, 204, 255, 255),
//                Color.FromArgb(255, 102, 0, 102),
//                Color.FromArgb(255, 255, 128, 128),
//                Color.FromArgb(255, 0, 102, 204),
//                Color.FromArgb(255, 204, 204, 255),
//                Color.FromArgb(255, 0, 0, 128),
//                Color.FromArgb(255, 255, 0, 255),
//                Color.FromArgb(255, 255, 255, 0),
//                Color.FromArgb(255, 0, 255, 255),
//                Color.FromArgb(255, 128, 0, 128),
//                Color.FromArgb(255, 128, 0, 0),
//                Color.FromArgb(255, 0, 128, 128),
//                Color.FromArgb(255, 0, 0, 255),
//                Color.FromArgb(255, 0, 204, 255),
//                Color.FromArgb(255, 204, 255, 255),
//                Color.FromArgb(255, 204, 255, 204),
//                Color.FromArgb(255, 255, 255, 153),
//                Color.FromArgb(255, 153, 204, 255),
//                Color.FromArgb(255, 255, 153, 204),
//                Color.FromArgb(255, 204, 153, 255),
//                Color.FromArgb(255, 255, 204, 153),
//                Color.FromArgb(255, 51, 102, 255),
//                Color.FromArgb(255, 51, 204, 204),
//                Color.FromArgb(255, 153, 204, 0),
//                Color.FromArgb(255, 255, 204, 0),
//                Color.FromArgb(255, 255, 153, 0),
//                Color.FromArgb(255, 255, 102, 0),
//                Color.FromArgb(255, 102, 102, 153),
//                Color.FromArgb(255, 150, 150, 150),
//                Color.FromArgb(255, 0, 51, 102),
//                Color.FromArgb(255, 51, 153, 102),
//                Color.FromArgb(255, 0, 51, 0),
//                Color.FromArgb(255, 51, 51, 0),
//                Color.FromArgb(255, 153, 51, 0),
//                Color.FromArgb(255, 153, 51, 102),
//                Color.FromArgb(255, 51, 51, 153),
//                Color.FromArgb(255, 51, 51, 51),
//        };

//        // This is the Excel 2007 color palette accoriding to the docs. We may need it later
//        //
//        //Color.FromArgb( 0x00, 0x00, 0x00 ),	// Black
//        //Color.FromArgb( 0xFF, 0xFF, 0xFF ),	// White
//        //Color.FromArgb( 0xFF, 0x00, 0x00 ),	// Red
//        //Color.FromArgb( 0x00, 0xFF, 0x00 ),	// Green
//        //Color.FromArgb( 0x00, 0x00, 0xFF ),	// Blue
//        //Color.FromArgb( 0xFF, 0xFF, 0x00 ),	// Yellow
//        //Color.FromArgb( 0xFF, 0x00, 0xFF ),	// Magenta
//        //Color.FromArgb( 0x00, 0xFF, 0xFF ),	// Cyan
//        //Color.FromArgb( 0x00, 0x00, 0x00 ),	// 8
//        //Color.FromArgb( 0xFF, 0xFF, 0xFF ),	// 9
//        //Color.FromArgb( 0xFF, 0x00, 0x00 ),	// 10
//        //Color.FromArgb( 0x00, 0xFF, 0x00 ),	// 11
//        //Color.FromArgb( 0x00, 0x00, 0xFF ),	// 12
//        //Color.FromArgb( 0xFF, 0xFF, 0x00 ),	// 13
//        //Color.FromArgb( 0xFF, 0x00, 0xFF ),	// 14
//        //Color.FromArgb( 0x00, 0xFF, 0xFF ),	// 15
//        //Color.FromArgb( 0x80, 0x00, 0x00 ),	// 16
//        //Color.FromArgb( 0x00, 0x80, 0x00 ),	// 17
//        //Color.FromArgb( 0x00, 0x00, 0x80 ),	// 18
//        //Color.FromArgb( 0x80, 0x80, 0x00 ),	// 19
//        //Color.FromArgb( 0x80, 0x00, 0x80 ),	// 20
//        //Color.FromArgb( 0x00, 0x80, 0x80 ),	// 21
//        //Color.FromArgb( 0xC0, 0xC0, 0xC0 ),	// 22
//        //Color.FromArgb( 0x80, 0x80, 0x80 ),	// 23
//        //Color.FromArgb( 0x99, 0x99, 0xFF ),	// 24
//        //Color.FromArgb( 0x99, 0x33, 0x66 ),	// 25
//        //Color.FromArgb( 0xFF, 0xFF, 0xCC ),	// 26
//        //Color.FromArgb( 0xCC, 0xFF, 0xFF ),	// 27
//        //Color.FromArgb( 0x66, 0x00, 0x66 ),	// 28
//        //Color.FromArgb( 0xFF, 0x80, 0x80 ),	// 29
//        //Color.FromArgb( 0x00, 0x66, 0xCC ),	// 30
//        //Color.FromArgb( 0xCC, 0xCC, 0xFF ),	// 31
//        //Color.FromArgb( 0x00, 0x00, 0x80 ),	// 32
//        //Color.FromArgb( 0xFF, 0x00, 0xFF ),	// 33
//        //Color.FromArgb( 0xFF, 0xFF, 0x00 ),	// 34
//        //Color.FromArgb( 0x00, 0xFF, 0xFF ),	// 35
//        //Color.FromArgb( 0x80, 0x00, 0x80 ),	// 36
//        //Color.FromArgb( 0x80, 0x00, 0x00 ),	// 37
//        //Color.FromArgb( 0x00, 0x80, 0x80 ),	// 38
//        //Color.FromArgb( 0x00, 0x00, 0xFF ),	// 39
//        //Color.FromArgb( 0x00, 0xCC, 0xFF ),	// 40
//        //Color.FromArgb( 0xCC, 0xFF, 0xFF ),	// 41
//        //Color.FromArgb( 0xCC, 0xFF, 0xCC ),	// 42
//        //Color.FromArgb( 0xFF, 0xFF, 0x99 ),	// 43
//        //Color.FromArgb( 0x99, 0xCC, 0xFF ),	// 44
//        //Color.FromArgb( 0xFF, 0x99, 0xCC ),	// 45
//        //Color.FromArgb( 0xCC, 0x99, 0xFF ),	// 46
//        //Color.FromArgb( 0xFF, 0xCC, 0x99 ),	// 47
//        //Color.FromArgb( 0x33, 0x66, 0xFF ),	// 48
//        //Color.FromArgb( 0x33, 0xCC, 0xCC ),	// 49
//        //Color.FromArgb( 0x99, 0xCC, 0x00 ),	// 50
//        //Color.FromArgb( 0xFF, 0xCC, 0x00 ),	// 51
//        //Color.FromArgb( 0xFF, 0x99, 0x00 ),	// 52
//        //Color.FromArgb( 0xFF, 0x66, 0x00 ),	// 53
//        //Color.FromArgb( 0x66, 0x66, 0x99 ),	// 54
//        //Color.FromArgb( 0x96, 0x96, 0x96 ),	// 55
//        //Color.FromArgb( 0x00, 0x33, 0x66 ),	// 56
//        //Color.FromArgb( 0x33, 0x99, 0x66 ),	// 57
//        //Color.FromArgb( 0x00, 0x33, 0x00 ),	// 58
//        //Color.FromArgb( 0x33, 0x33, 0x00 ),	// 59
//        //Color.FromArgb( 0x99, 0x33, 0x00 ),	// 60
//        //Color.FromArgb( 0x99, 0x33, 0x66 ),	// 61
//        //Color.FromArgb( 0x33, 0x33, 0x99 ),	// 62
//        //Color.FromArgb( 0x33, 0x33, 0x33 ),	// 63

//        #endregion excelStandardColors

//        #endregion Constants

//        #region Member Variables

//        private Dictionary<Color, int> colorHash;

//        // MRS 6/28/05 - BR04756
//        private WorkbookPaletteMode paletteMode;

//        // MD 1/15/08 - BR29635
//        private Workbook workbook;

//        #endregion Member Variables

//        #region Constructor

//        // MRS 6/28/05 - BR04756
//        //internal WorkbookColorCollection()
//        // MD 1/15/08 - BR29635
//        // Added a parameter to the constructor
//        //internal WorkbookColorCollection( WorkbookPaletteMode paletteMode )
//        internal WorkbookColorCollection( Workbook workbook, WorkbookPaletteMode paletteMode )
//        {
//            this.colorHash = new Dictionary<Color, int>();

//            // MRS 6/28/05 - BR04756
//            this.paletteMode = paletteMode;

//            // MD 1/15/08 - BR29635
//            this.workbook = workbook;
//        }

//        #endregion Constructor

//        #region Methods

//        #region Internal Methods

//        #region Add

//#if DEBUG
//        /// <summary>
//        /// Adds a color to the palette if it doesn't exist and returns the palette index for the color.
//        /// </summary>  
//#endif
//        // MD 8/24/07 - BR25848
//        // We need to know whether the color is needed for a border or not
//        //internal int Add( Color color )
//        // MD 8/30/07 - BR26111
//        // Changed parameter to provide more info about the item getting a color
//        //internal int Add( Color color, bool isForBorder )
//        internal int Add( Color color, ColorableItem item )
//        {
//            // MD 1/4/11 - TFS60250
//            // Moved all code to a helper method.
//            try
//            {
//                return this.AddHelper(color, item);
//            }
//            catch (UnauthorizedAccessException exc)
//            {
//                throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_SystemColorsAccessedFromWrongThread"), exc);
//            }
//        }

//        // MD 1/4/11 - TFS60250
//        // Moved this code from Add so it could be wrapped in a try...catch.
//        internal int AddHelper(Color color, ColorableItem item)
//        {
//            // MD 5/12/10 - TFS26732
//            // If an empty color is passed in, we don't have to add it to the palette. Just return -1.
//            if (Utilities.ColorIsEmpty(color))
//                return -1;

//            // MD 10/23/07 - BR27496
//            // Only consult the hash after we check for standard colors. Now that we restrict the indices which can be used
//            // based on item type, an index might be stored in the user palette section (in the color hash) for a previous 
//            // item type which had restricted indices, and the current item type could be allowed ot use those indices, but
//            // instead it would get the index in the user palette section because it would find it here in the hash. 
//            // We don't want that to happen.
//            //int index;
//            //if ( this.colorHash.TryGetValue( color, out index ) )
//            //    return index;

//            // MD 8/24/07 - BR25848
//            // Wrapped in an if statement: these first 8 color indicies cannot be used for borders
//            // Wrapped in an if statement: these first 8 color indices cannot be used for borders
//            // MD 8/30/07 - BR26111
//            // The isForBorder parameter was changed to something that provides more info about the item getting a color
//            //if ( isForBorder == false )
//            // MD 10/23/07 - BR27496/BR26111/BR25848
//            // Fill colors can't use these indices either or the cell format dialog will not appear
//            //if ( item != ColorableItem.CellBorder )
//            // MD 8/5/08 - BR34592
//            // These first index values also cannot be used for cell fonts.
//            //if ( item != ColorableItem.CellBorder &&
//            //    item != ColorableItem.CellFill )
//            // MD 9/5/08 - TFS6408
//            // These first index values also cannot be used for the grid line color index or the options dialog cannot be opened.
//            //if ( item != ColorableItem.CellBorder &&
//            //    item != ColorableItem.CellFill &&
//            //    item != ColorableItem.CellFont )
//#if SILVERLIGHT
//            if (item != ColorableItem.CellBorder &&
//                item != ColorableItem.CellFill &&
//                item != ColorableItem.CellFont &&
//                item != ColorableItem.WorksheetGrid)
//            {
//                if (color == Colors.Black)
//                    return 0x00;

//                if (color == Colors.White)
//                    return 0x01;

//                if (color == Colors.Red)
//                    return 0x02;

//                // 09/02/08 CDS #00FF00 is Color.Lime, not Color.Green
//                //if (color == Color.Green)
//                if (color == SilverlightFixes.ColorFromArgb(-16711936))//Color.Lime
//                    return 0x03;

//                if (color == Colors.Blue)
//                    return 0x04;

//                if (color == Colors.Yellow)
//                    return 0x05;

//                if (color == Colors.Magenta)
//                    return 0x06;

//                if (color == Colors.Cyan)
//                    return 0x07;
//            }

//            if (color == Utilities.SystemColorsInternal.WindowTextColor)
//                return 0x40;

//            // MD 8/24/07 - BR25848/BR26111
//            // Wrapped in an if statement: these color indices cannot be used for fill colors
//            if (item != ColorableItem.CellFill)
//            {
//                if (color == Utilities.SystemColorsInternal.WindowColor)
//                    return 0x41;

//                if (color == Utilities.SystemColorsInternal.WindowFrameColor)
//                    return 0x42;

//                // MD 4/9/08 - BR28279
//                // Wrapped in an if statement:these color indices cannot be used for border colors
//                if (item != ColorableItem.CellBorder)
//                {
//                    if (color == Utilities.SystemColorsInternal.ControlColor)
//                        return 0x43;

//                    if (color == Utilities.SystemColorsInternal.ControlTextColor)
//                        return 0x44;

//                    if (color == Utilities.SystemColorsInternal.ControlLightLightColor)
//                        return 0x45;

//                    if (color == Utilities.SystemColorsInternal.ControlDarkColor)
//                        return 0x46;

//                    if (color == Utilities.SystemColorsInternal.HighlightColor)
//                        return 0x47;

//                    // This will never be hit
//                    //if ( color == SystemColors.WindowText )
//                    //    return 0x48;

//                    if (color == Utilities.SystemColorsInternal.ScrollBarColor)
//                        return 0x49;

//                    if (color == InvertColor(Utilities.SystemColorsInternal.ScrollBarColor))
//                        return 0x4A;

//                    // This will never be hit
//                    //if ( color == SystemColors.Window )
//                    //    return 0x4B;

//                    // This will never be hit
//                    //if ( color == SystemColors.WindowFrame )
//                    //	return 0x4C;

//                    // This will never be hit
//                    //if ( color == SystemColors.WindowText )
//                    //    return 0x4D;

//                    // This will never be hit
//                    //if ( color == SystemColors.Window )
//                    //    return 0x4E;

//                    // This will never be hit
//                    //if ( color == Color.Black )
//                    //    return 0x4F;

//                    if (color == Utilities.SystemColorsInternal.InfoColor)
//                        return 0x50;

//                    if (color == Utilities.SystemColorsInternal.InfoTextColor)
//                        return 0x51;

//                    // This will never be hit
//                    //if ( color == SystemColors.WindowText )
//                    //    return 0x7FFF;
//                }
//            }
//#else
//            if ( item != ColorableItem.CellBorder &&
//                item != ColorableItem.CellFill &&
//                item != ColorableItem.CellFont &&
//                item != ColorableItem.WorksheetGrid )
//            {
//                if ( color == Color.Black )
//                    return 0x00;

//                if ( color == Color.White )
//                    return 0x01;

//                if ( color == Color.Red )
//                    return 0x02;

//                // 09/02/08 CDS #00FF00 is Color.Lime, not Color.Green
//                //if (color == Color.Green)
//                if (color == Color.Lime)
//                    return 0x03;

//                if ( color == Color.Blue )
//                    return 0x04;

//                if ( color == Color.Yellow )
//                    return 0x05;

//                if ( color == Color.Magenta )
//                    return 0x06;

//                if ( color == Color.Cyan )
//                    return 0x07;
//            }

//            if ( color == SystemColors.WindowText )
//                return 0x40;

//            // MD 1/8/12 - 12.1 - Cell Format Updates
//            // This is valid for cell fills, so moved it out of the if block below.
//            if (color == SystemColors.Window)
//                return 0x41;

//            // MD 8/24/07 - BR25848/BR26111
//            // Wrapped in an if statement: these color indices cannot be used for fill colors
//            if ( item != ColorableItem.CellFill )
//            {
//                // MD 1/8/12 - 12.1 - Cell Format Updates
//                // This is valid for cell fills, so moved it out of the if block.
//                //if ( color == SystemColors.Window )
//                //    return 0x41;

//                if ( color == SystemColors.WindowFrame )
//                    return 0x42;

//                // MD 4/9/08 - BR28279
//                // Wrapped in an if statement:these color indices cannot be used for border colors
//                if ( item != ColorableItem.CellBorder )
//                {
//                    if ( color == SystemColors.Control )
//                        return 0x43;

//                    if ( color == SystemColors.ControlText )
//                        return 0x44;

//                    if ( color == SystemColors.ControlLightLight )
//                        return 0x45;

//                    if ( color == SystemColors.ControlDark )
//                        return 0x46;

//                    if ( color == SystemColors.Highlight )
//                        return 0x47;

//                    // This will never be hit
//                    //if ( color == SystemColors.WindowText )
//                    //    return 0x48;

//                    if ( color == SystemColors.ScrollBar )
//                        return 0x49;

//                    if ( color == InvertColor( SystemColors.ScrollBar ) )
//                        return 0x4A;

//                    // This will never be hit
//                    //if ( color == SystemColors.Window )
//                    //    return 0x4B;

//                    // This will never be hit
//                    //if ( color == SystemColors.WindowFrame )
//                    //	return 0x4C;

//                    // This will never be hit
//                    //if ( color == SystemColors.WindowText )
//                    //    return 0x4D;

//                    // This will never be hit
//                    //if ( color == SystemColors.Window )
//                    //    return 0x4E;

//                    // This will never be hit
//                    //if ( color == Color.Black )
//                    //    return 0x4F;

//                    if ( color == SystemColors.Info )
//                        return 0x50;

//                    if ( color == SystemColors.InfoText )
//                        return 0x51;

//                    // This will never be hit
//                    //if ( color == SystemColors.WindowText )
//                    //    return 0x7FFF;
//                }
//            }
//#endif // SILVERLIGHT

//            // MD 10/23/07 - BR27496
//            // Moved from above. We only want to consult the hash after trying the standard colors.
//            // See comment above for more info.
//            int index;
//            if ( this.colorHash.TryGetValue( color, out index ) )
//                return index;

//            // MRS 6/28/05 - BR04756
//            if ( this.paletteMode == WorkbookPaletteMode.StandardPalette )
//            {
//                index = WorkbookColorCollection.GetIndexOfNearestExcelColor( color ) + UserPaletteStart;
//                this.colorHash.Add( color, index );
//                return index;
//            }

//            // MD 1/15/08 - BR29635
//            // We might need to search for a color that isn't being used if there are too many colors in the collection.
//            //return this.AddInternal( ( (IList)this ).Count, color );
//            int indexIntoUserPalette = ( (IList)this ).Count;

//            // MBS 7/11/08 - Excel 2007 Format
//            // We don't have the palette restriction in the 2007 format
//            //
//            if ( indexIntoUserPalette >= Workbook.MaxExcelColorCount )
//            {
//                if ( this.TryGetUnusedUserColorIndex( out indexIntoUserPalette ) )
//                {
//                    this.colorHash.Remove( (Color)( (IList)this )[ indexIntoUserPalette ] );
//                    ( (IList)this ).RemoveAt( indexIntoUserPalette );
//                }
//                else
//                {
//                    indexIntoUserPalette = Workbook.MaxExcelColorCount;
//                }
//            }

//            return this.AddInternal( indexIntoUserPalette, color );
//        }

//        #endregion Add

//        #region AddInternal

//        internal int AddInternal( int indexIntoUserPalette, Color color )
//        {
//            // MD 9/10/08 - Excel 2007 Format
//            // Added an assert here because this should really never happen.
//#if !SILVERLIGHT
//            Debug.Assert( color.IsEmpty == false, "Empty colors should not be added to the color palette." );
//#endif

//            // MD 1/15/08 - BR29635
//            // Check the index now, it could be midway in the collection
//            //if ( this.colorHash.Count == Workbook.MaxExcelColorCount )
//            //
//            if ( indexIntoUserPalette >= Workbook.MaxExcelColorCount )
//                throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MaxColors", Workbook.MaxExcelColorCount ) );

//            ( (IList)this ).Insert( indexIntoUserPalette, color );

//            int colorIndex = indexIntoUserPalette + UserPaletteStart;

//            if ( this.colorHash.ContainsKey( color ) )
//                return this.colorHash[ color ];

//            // MD 1/17/11 - TFS62014
//            // Do a reverse lookup to see if the color index is already present. If so, remove it from the dictionary.
//            foreach (KeyValuePair<Color, int> pair in this.colorHash)
//            {
//                if (pair.Value == colorIndex)
//                {
//                    this.colorHash.Remove(pair.Key);
//                    break;
//                }
//            }

//            this.colorHash.Add( color, colorIndex );
//            return colorIndex;
//        }

//        #endregion AddInternal

//        #endregion Internal Methods

//        #region Private Methods

//        // MRS 6/28/05 - BR04756
//        #region GetIndexOfNearestExcelColor

//#if DEBUG
//        /// <summary>
//        /// Gets the index of the closest matching color in the standard Excel Palette
//        /// </summary>
//        /// <param name="color">The color to match.</param>
//        /// <returns></returns> 
//#endif
//        private static int GetIndexOfNearestExcelColor( Color color )
//        {
//            int index = 0;
//            double shortestDistance = Math.Pow( 256, 3 );

//            int r = color.R;
//            int g = color.G;
//            int b = color.B;

//            for ( int i = 0; i < excelStandardColors.Length; i++ )
//            {
//                Color excelColor = excelStandardColors[ i ];

//                int r2 = excelColor.R;
//                int g2 = excelColor.G;
//                int b2 = excelColor.B;

//                double distance = ( Math.Pow( r - r2, 2 ) ) + ( Math.Pow( g - g2, 2 ) ) + ( Math.Pow( b - b2, 2 ) );
//                if ( distance < shortestDistance )
//                {
//                    index = i;
//                    shortestDistance = distance;
//                }
//            }

//            return index;
//        }

//        #endregion GetIndexOfNearestExcelColor

//        #region InvertColor

//        private static Color InvertColor( Color color )
//        {
//            return Color.FromArgb( color.A, (byte)~color.R, (byte)~color.G, (byte)~color.B );
//        }

//        #endregion InvertColor

//        // MD 1/15/08 - BR29635
//        #region TryGetUnusedUserColorIndex

//        private bool TryGetUnusedUserColorIndex( out int indexIntoUserPalette )
//        {
//            indexIntoUserPalette = -1;

//            List<int> unusedIndicies = new List<int>( this.colorHash.Count );

//            foreach ( int index in this.colorHash.Values )
//                unusedIndicies.Add( index );

//            unusedIndicies.Sort();

//            this.workbook.RemoveUsedColorIndicies( unusedIndicies );

//            if ( unusedIndicies.Count == 0 )
//                return false;

//            // MD 6/8/09 - TFS18283
//            // I'm not really sure where the index of 21 came from, but this looks like a mistake. We should be
//            // taking the first unused color, not the 22nd.
//            //indexIntoUserPalette = unusedIndicies[ 21 ] - WorkbookColorCollection.UserPaletteStart;
//            indexIntoUserPalette = unusedIndicies[ 0 ] - WorkbookColorCollection.UserPaletteStart;

//            return true;
//        } 

//        #endregion TryGetUnusedUserColorIndex

//        #endregion Private Methods

//        #endregion Methods

//        #region Properties

//        #region PaletteMode

//        internal WorkbookPaletteMode PaletteMode
//        {
//            get { return this.paletteMode; }
//            set { this.paletteMode = value; }
//        }

//        #endregion PaletteMode

//        #region Indexer[ int ]

//#if SILVERLIGHT
//        internal new Color this[ int index ]
//#else
//        internal Color this[ int index ]
//#endif
//        {
//            get
//            {
//                // MD 1/4/11 - TFS60250
//                // Moved all code to a helper method.
//                try
//                {
//                    return this.IndexerHelper(index);
//                }
//                catch (UnauthorizedAccessException exc)
//                {
//                    throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_SystemColorsAccessedFromWrongThread"), exc);
//                }
//            }
//        }

//        // MD 1/4/11 - TFS60250
//        // Moved this code from the int indexer so it could be wrapped in a try...catch.
//        internal Color IndexerHelper(int index)
//        {
//                switch ( index )
//                {
//#if SILVERLIGHT
//                        case 0x00: return Colors.Black;
//                    case 0x01: return Colors.White;
//                    case 0x02: return Colors.Red;
//                    // 09/02/08 CDS #00FF00 is Color.Lime, not Color.Green
//                    //case 0x03: return Color.Green;
//                    case 0x03: return SilverlightFixes.ColorFromArgb(-16711936);//Color.Lime
//                    case 0x04: return Colors.Blue;
//                    case 0x05: return Colors.Yellow;
//                    case 0x06: return Colors.Magenta;
//                    case 0x07: return Colors.Cyan;

//                    case 0x40: return Utilities.SystemColorsInternal.WindowTextColor;
//                    case 0x41: return Utilities.SystemColorsInternal.WindowColor;
//                    case 0x42: return Utilities.SystemColorsInternal.WindowFrameColor;
//                    case 0x43: return Utilities.SystemColorsInternal.ControlColor;
//                    case 0x44: return Utilities.SystemColorsInternal.ControlTextColor;
//                    case 0x45: return Utilities.SystemColorsInternal.ControlLightLightColor;
//                    case 0x46: return Utilities.SystemColorsInternal.ControlDarkColor;
//                    case 0x47: return Utilities.SystemColorsInternal.HighlightColor;
//                    case 0x48: return Utilities.SystemColorsInternal.WindowTextColor;
//                    case 0x49: return Utilities.SystemColorsInternal.ScrollBarColor;
//                    case 0x4A: return InvertColor(Utilities.SystemColorsInternal.ScrollBarColor);
//                    case 0x4B: return Utilities.SystemColorsInternal.WindowColor;
//                    case 0x4C: return Utilities.SystemColorsInternal.WindowFrameColor;
//                    case 0x4D: return Utilities.SystemColorsInternal.WindowTextColor;
//                    case 0x4E: return Utilities.SystemColorsInternal.WindowColor;
//                    case 0x4F: return Colors.Black;
//                    case 0x50: return Utilities.SystemColorsInternal.InfoColor;
//                    case 0x51: return Utilities.SystemColorsInternal.InfoTextColor;

//                    case 0x7FFF: return Utilities.SystemColorsInternal.WindowTextColor;

//                    default:
//                        if ( 0x51 < index )
//                            return Utilities.SystemColorsInternal.WindowColor;

//                        break;
//                }

//                int userPaletteIndex = index - WorkbookColorCollection.UserPaletteStart;

//                // MD 10/23/07 - BR27496
//                // Moved from below. Negative indices in standard palette mode should also return an empty color.
//                if (userPaletteIndex < 0)
//                    return SilverlightFixes.ColorEmpty;

//#else
//                    case 0x00: return Color.Black;
//                    case 0x01: return Color.White;
//                    case 0x02: return Color.Red;
//                    // 09/02/08 CDS #00FF00 is Color.Lime, not Color.Green
//                    //case 0x03: return Color.Green;
//                    case 0x03: return Color.Lime;
//                    case 0x04: return Color.Blue;
//                    case 0x05: return Color.Yellow;
//                    case 0x06: return Color.Magenta;
//                    case 0x07: return Color.Cyan;

//                    case 0x40: return SystemColors.WindowText;
//                    case 0x41: return SystemColors.Window;
//                    case 0x42: return SystemColors.WindowFrame;
//                    case 0x43: return SystemColors.Control;
//                    case 0x44: return SystemColors.ControlText;
//                    case 0x45: return SystemColors.ControlLightLight;
//                    case 0x46: return SystemColors.ControlDark;
//                    case 0x47: return SystemColors.Highlight;
//                    case 0x48: return SystemColors.WindowText;
//                    case 0x49: return SystemColors.ScrollBar;
//                    case 0x4A: return InvertColor( SystemColors.ScrollBar );
//                    case 0x4B: return SystemColors.Window;
//                    case 0x4C: return SystemColors.WindowFrame;
//                    case 0x4D: return SystemColors.WindowText;
//                    case 0x4E: return SystemColors.Window;
//                    case 0x4F: return Color.Black;
//                    case 0x50: return SystemColors.Info;
//                    case 0x51: return SystemColors.InfoText;

//                    case 0x7FFF: return SystemColors.WindowText;

//                    default:
//                        // MD 9/9/08 - Excel 2007 Format
//                        // These shouldn't be returned here. They will be returned below when we access the Excel standard colors.
//                        //if (this.workbook.CurrentFormat == WorkbookFormat.Excel2007)
//                        //    switch (index)
//                        //    {
//                        //        case 0x08: return Color.Black;
//                        //        case 0x09: return Color.White;
//                        //        case 0x0A: return Color.Red;
//                        //        case 0x0B: return Color.Lime;
//                        //        case 0x0C: return Color.Blue;
//                        //        case 0x0D: return Color.Yellow;
//                        //        case 0x0E: return Color.Magenta;
//                        //        case 0x0F: return Color.Cyan;
//                        //        case 0x10: return Color.Maroon;                          //00800000
//                        //        case 0x11: return Color.Green;                           //00008000
//                        //        case 0x12: return Color.Navy;                            //00000080
//                        //        case 0x13: return Color.Olive;                           //00808000
//                        //        case 0x14: return Color.Purple;                          //00800080
//                        //        case 0x15: return Color.Teal;                            //00008080
//                        //        case 0x16: return Color.Silver;                          //00C0C0C0
//                        //        case 0x17: return Color.Gray;                            //00808080
//                        //        case 0x18: return Color.FromArgb(255, 153, 153, 255);    //009999FF
//                        //        case 0x19: return Color.FromArgb(255, 153, 51, 102);     //00993366
//                        //        case 0x1A: return Color.FromArgb(255, 255, 255, 204);    //00FFFFCC
//                        //        case 0x1B: return Color.FromArgb(255, 204, 255, 255);    //00CCFFFF
//                        //        case 0x1C: return Color.FromArgb(255, 102, 0, 102);      //00660066
//                        //        case 0x1D: return Color.FromArgb(255, 255, 128, 128);    //00FF8080
//                        //        case 0x1E: return Color.FromArgb(255, 0, 102, 204);      //000066CC
//                        //        case 0x1F: return Color.FromArgb(255, 204, 204, 255);    //00CCCCFF
//                        //        case 0x20: return Color.Navy;                            //00000080
//                        //        case 0x21: return Color.Fuchsia;                         //00FF00FF
//                        //        case 0x22: return Color.Yellow;                          //00FFFF00
//                        //        case 0x23: return Color.Aqua;                            //0000FFFF
//                        //        case 0x24: return Color.Purple;                          //00800080
//                        //        case 0x25: return Color.Maroon;                          //00800000
//                        //        case 0x26: return Color.Teal;                            //00008080
//                        //        case 0x27: return Color.Blue;                            //000000FF
//                        //        case 0x28: return Color.FromArgb(255, 0, 204, 255);      //0000CCFF
//                        //        case 0x29: return Color.FromArgb(255, 204, 255, 255);    //00CCFFFF
//                        //        case 0x2A: return Color.FromArgb(255, 204, 255, 204);    //00CCFFCC
//                        //        case 0x2B: return Color.FromArgb(255, 255, 255, 153);    //00FFFF99
//                        //        case 0x2C: return Color.FromArgb(255, 153, 204, 255);    //0099CCFF
//                        //        case 0x2D: return Color.FromArgb(255, 255, 153, 204);    //00FF99CC
//                        //        case 0x2E: return Color.FromArgb(255, 204, 153, 255);    //00CC99FF
//                        //        case 0x2F: return Color.FromArgb(255, 255, 204, 153);    //00FFCC99
//                        //        case 0x30: return Color.FromArgb(255, 51, 102, 255);     //003366FF
//                        //        case 0x31: return Color.FromArgb(255, 51, 204, 204);     //0033CCCC
//                        //        case 0x32: return Color.FromArgb(255, 153, 204, 0);      //0099CC00
//                        //        case 0x33: return Color.FromArgb(255, 255, 204, 0);      //00FFCC00
//                        //        case 0x34: return Color.FromArgb(255, 255, 153, 0);      //00FF9900
//                        //        case 0x35: return Color.FromArgb(255, 255, 102, 0);      //00FF6600
//                        //        case 0x36: return Color.FromArgb(255, 102, 102, 153);    //00666699
//                        //        case 0x37: return Color.FromArgb(255, 150, 150, 150);    //00969696
//                        //        case 0x38: return Color.FromArgb(255, 0, 51, 102);       //00003366
//                        //        case 0x39: return Color.FromArgb(255, 51, 153, 102);     //00339966
//                        //        case 0x3A: return Color.FromArgb(255, 0, 51, 0);         //00003300
//                        //        case 0x3B: return Color.FromArgb(255, 51, 51, 0);        //00333300
//                        //        case 0x3C: return Color.FromArgb(255, 153, 51, 0);       //00993300
//                        //        case 0x3D: return Color.FromArgb(255, 153, 51, 102);     //00993366
//                        //        case 0x3E: return Color.FromArgb(255, 51, 51, 153);      //00333399
//                        //        case 0x3F: return Color.FromArgb(255, 51, 51, 51);       //00333333
//                        //    }
//                        if ( 0x51 < index )
//                            return SystemColors.Window;

//                        break;
//                        }

//                int userPaletteIndex = index - WorkbookColorCollection.UserPaletteStart;

//                // MD 10/23/07 - BR27496
//                // Moved from below. Negative indices in standard palette mode should also return an empty color.
//                if ( userPaletteIndex < 0 )
//                    return Color.Empty;

//#endif

//                // MRS 6/28/05 - BR04756
//                // MD 9/9/08 - Excel 2007 Format
//                // If the custom color at the user index hasn't been defined yet, use the standard color at the index.
//                //if ( this.paletteMode == WorkbookPaletteMode.StandardPalette )
//                if ( this.paletteMode == WorkbookPaletteMode.StandardPalette || userPaletteIndex >= ( (IList)this ).Count )
//                {
//                    return excelStandardColors[userPaletteIndex];
//                }

//                // MD 10/23/07 - BR27496
//                // Moved above. Negative indices in standard palette mode should also return an empty color.
//                //if ( userPaletteIndex < 0 )
//                //    return Color.Empty;

//                return (Color)( ( (IList)this )[ userPaletteIndex ] );
//            //}
//        }

//        #endregion Indexer[ int ]

//        #endregion Properties
//    }
//}

#endregion // Removed
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