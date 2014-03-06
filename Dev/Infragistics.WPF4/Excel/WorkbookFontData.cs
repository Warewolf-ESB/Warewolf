using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using Infragistics.Documents.Excel.Serialization;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 1/18/12 - 12.1 - Cell Format Updates
	// The code we changed too much for the cell format updates, so I just commented out the old code.
	#region Old Code

	//    internal class WorkbookFontData : GenericCacheElement, 
	//        IWorkbookFont
	//    {
	//        #region Private Members

	//        private ExcelDefaultableBoolean bold;

	//        // MD 7/2/09 - TFS18634
	//        private int cachedHashCode;

	//        private int colorIndex;
	//        private int height;
	//        private ExcelDefaultableBoolean italic;
	//        private string name;
	//        private ExcelDefaultableBoolean strikeout;
	//        private FontSuperscriptSubscriptStyle superscriptSubscriptStyle;
	//        private FontUnderlineStyle underlineStyle;

	//        // MD 1/19/11 - TFS62268
	//        // We will now store RGB data as an alternative to indexed values when we are using 2007 or later formats.
	//        private Color color;

	//        #region Serialization Cache

	//        private int indexInFontCollection = -1;

	//        // MD 11/10/11 - TFS85193
	//        // Because shapes resolve different defaults, the associated item in the saved (resolved) font table may
	//        // be at a different index for the text of shapes versus other entities.
	//        private int indexInFontCollectionForShapesWithText = -1;

	//        #endregion Serialization Cache

	//        #endregion Private Members

	//        #region Constructors

	//        public WorkbookFontData( Workbook workbook )
	//            : base( workbook )
	//        {
	//            this.height = -1;

	//            this.colorIndex = -1;

	//            this.bold = ExcelDefaultableBoolean.Default;
	//            this.italic = ExcelDefaultableBoolean.Default;
	//            this.strikeout = ExcelDefaultableBoolean.Default;

	//            this.superscriptSubscriptStyle = FontSuperscriptSubscriptStyle.Default;
	//            this.underlineStyle = FontUnderlineStyle.Default;
	//        }

	//        private WorkbookFontData( WorkbookFontData source )
	//            : base( source.Workbook )
	//        {
	//            this.name = source.name;
	//            this.height = source.height;
	//            this.colorIndex = source.colorIndex;
	//            this.bold = source.bold;
	//            this.italic = source.italic;
	//            this.strikeout = source.strikeout;
	//            this.superscriptSubscriptStyle = source.superscriptSubscriptStyle;
	//            this.underlineStyle = source.underlineStyle;

	//            this.indexInFontCollection = source.indexInFontCollection;

	//            // MD 1/19/11 - TFS62268
	//            // Copy over the RGB data as well.
	//            this.color = source.color;

	//            // MD 4/18/11 - TFS62026
	//            // If all the members are the same, so is the hash code, so copy that as well.
	//            this.cachedHashCode = source.cachedHashCode;
	//        }

	//        #endregion Constructors

	//        #region Base Class Overrides

	//        #region Clone

	//        public override object Clone()
	//        {
	//            return new WorkbookFontData( this );
	//        }

	//        #endregion Clone

	//        #region Equals

	//        public override bool Equals( object obj )
	//        {
	//            return this.HasSameData( obj as GenericCacheElement );
	//        }

	//        #endregion Equals

	//        #region GetHashCode

	//        public override int GetHashCode()
	//        {
	//            // MD 7/2/09 - TFS18634
	//            if ( this.cachedHashCode != 0 )
	//                return this.cachedHashCode;

	//            int hashCode = 0;

	//            if ( this.name != null )
	//                hashCode ^= this.name.GetHashCode();

	//            hashCode ^= this.height								<< 1;
	//            hashCode ^= this.colorIndex							<< 2;
	//            hashCode ^= (int)this.bold							<< 3;
	//            hashCode ^= (int)this.italic						<< 4;
	//            hashCode ^= (int)this.strikeout						<< 5;
	//            hashCode ^= (int)this.superscriptSubscriptStyle		<< 6;
	//            hashCode ^= (int)this.underlineStyle				<< 7;

	//            // MD 1/19/11 - TFS62268
	//            // The RGB data should be included in the hash code.
	//            hashCode ^= Utilities.ColorToArgb(this.color);

	//            // MD 7/2/09 - TFS18634
	//            this.cachedHashCode = hashCode;

	//            return hashCode;
	//        }

	//        #endregion GetHashCode

	//        #region HasSameData

	//        public override bool HasSameData( GenericCacheElement otherElement )
	//        {
	//            if ( otherElement == null )
	//                return false;

	//            if ( this == otherElement )
	//                return true;

	//            WorkbookFontData otherFont = (WorkbookFontData)otherElement;

	//            if ( this.height != otherFont.height )
	//                return false;

	//            if ( this.colorIndex != otherFont.colorIndex )
	//                return false;

	//            if ( this.bold != otherFont.bold )
	//                return false;

	//            if ( this.italic != otherFont.italic )
	//                return false;

	//            if ( this.strikeout != otherFont.strikeout )
	//                return false;

	//            if ( this.superscriptSubscriptStyle != otherFont.superscriptSubscriptStyle )
	//                return false;

	//            if ( this.underlineStyle != otherFont.underlineStyle )
	//                return false;

	//            if ( this.name != otherFont.name )
	//                return false;

	//            // MD 1/19/11 - TFS62268
	//            // The RGB data should be considered in the equality check
	//            if (this.color != otherFont.color)
	//                return false;

	//            return true;
	//        }

	//        #endregion HasSameData

	//        // MD 1/19/11 - TFS62268
	//        #region OnCurrentFormatChanged

	//        public override void OnCurrentFormatChanged()
	//        {
	//            if (Utilities.IsRGBColorFormatSupported(this.Workbook.CurrentFormat) == false)
	//            {
	//                if (Utilities.ColorIsEmpty(this.color) == false)
	//                    this.Color = this.color;
	//            }
	//        } 

	//        #endregion // OnCurrentFormatChanged

	//        #endregion Base Class Overrides

	//        #region Methods

	//        #region Public Methods

	//        // MD 1/14/08 - BR29635
	//        #region RemoveUsedColorIndicies

	//        public override void RemoveUsedColorIndicies( List<int> unusedIndicies )
	//        {
	//            Utilities.RemoveValueFromSortedList( this.colorIndex, unusedIndicies );
	//        }

	//        #endregion RemoveUsedColorIndicies

	//        #region SetFontFormatting

	//        public void SetFontFormatting( IWorkbookFont source )
	//        {
	//            if ( source == null )
	//                throw new ArgumentNullException( "source", SR.GetString( "LE_ArgumentNullException_SourceFont" ) );

	//            this.name = source.Name;
	//            this.height = source.Height;
	//            this.Color = source.Color;
	//            this.bold = source.Bold;
	//            this.italic = source.Italic;
	//            this.strikeout = source.Strikeout;
	//            this.superscriptSubscriptStyle = source.SuperscriptSubscriptStyle;
	//            this.underlineStyle = source.UnderlineStyle;

	//            // MD 7/2/09 - TFS18634
	//            this.cachedHashCode = 0;
	//        }

	//        #endregion SetFontFormatting

	//        #endregion Public Methods

	//        #region Internal Methods

	//        #region Combine

	//        internal static WorkbookFontData Combine( WorkbookFontData highPriorityFnt, WorkbookFontData lowPriorityFnt )
	//        {
	//            // MD 11/8/11 - TFS85193
	//            // Moved all code to new overload of Combine
	//            return WorkbookFontData.Combine(highPriorityFnt, lowPriorityFnt, null);
	//        }

	//        // MD 11/8/11 - TFS85193
	//        // Added a new overload to take a IWorkbookFontDefaultsResolver parameter
	//        internal static WorkbookFontData Combine(WorkbookFontData highPriorityFnt, WorkbookFontData lowPriorityFnt, IWorkbookFontDefaultsResolver defaultsResolver)
	//        {
	//            // Initialize a new font with the lower priority font
	//            WorkbookFontData resolved = new WorkbookFontData( lowPriorityFnt );

	//            // MD 11/8/11 - TFS85193
	//            // If a defaults resolver is specified, resolve the properties of the high priority font. 
	//            // They will be taken into the combined font.
	//            if (defaultsResolver != null)
	//            {
	//                highPriorityFnt = new WorkbookFontData(highPriorityFnt);
	//                defaultsResolver.ResolveDefaults(highPriorityFnt);
	//            }

	//            // Copy all non-default values from the higher priority font to the resolved font
	//            resolved.SelectiveCopyValues( highPriorityFnt );

	//            return resolved;
	//        }

	//        #endregion Combine

	//        // MD 11/10/11 - TFS85193
	//        #region CopyFontCollectionIndexesFrom

	//        internal void CopyFontCollectionIndexesFrom(WorkbookFontData other)
	//        {
	//            this.indexInFontCollection = other.indexInFontCollection;
	//            this.indexInFontCollectionForShapesWithText = other.indexInFontCollectionForShapesWithText;
	//        }

	//        #endregion  // CopyFontCollectionIndexesFrom

	//        // MD 11/10/11 - TFS85193
	//        #region GetIndexInFontCollection

	//        internal int GetIndexInFontCollection(FontResolverType fontResolverType)
	//        {
	//            switch (fontResolverType)
	//            {
	//                case FontResolverType.Normal:
	//                    return this.indexInFontCollection;

	//                case FontResolverType.ShapeWithText:
	//                    return this.indexInFontCollectionForShapesWithText;

	//                default:
	//                    Utilities.DebugFail("Unknown FontResolverType: " + fontResolverType);
	//                    goto case FontResolverType.Normal;
	//            }
	//        }

	//        #endregion  // GetIndexInFontCollection

	//        #region ResolvedFontData

	//        internal WorkbookFontData ResolvedFontData()
	//        {
	//            // MD 8/23/11 - TFS84306
	//            // Moved all code to the new overload.
	//            return this.ResolvedFontData(null);
	//        }

	//        // MD 8/23/11 - TFS84306
	//        // Added a new overload to take an instance to resolve font property defaults.
	//        internal WorkbookFontData ResolvedFontData(IWorkbookFontDefaultsResolver defaultsResolver)
	//        {
	//            // MD 9/15/08 - Excel 2007 Format
	//            // If this is a cell or formatting run font data item, we want to first initialize it with any data in the default element, 
	//            // especially if this is a loaded workbook, because our defaults may not be the workbook defaults. Then if the things are 
	//            // still default, resolve them with our defaults.
	//            //WorkbookFontData resolvedData = new WorkbookFontData( this );
	//            WorkbookFontData defaultElement = this.Workbook.Fonts.DefaultElement;
	//            WorkbookFontData resolvedData;

	//            if ( this == defaultElement )
	//            {
	//                resolvedData = new WorkbookFontData( this );

	//                // MD 11/8/11 - TFS85193
	//                // If an instance to resolve font property defaults was provided, let it resolve any defaults it wants.
	//                if (defaultsResolver != null)
	//                    defaultsResolver.ResolveDefaults(resolvedData);
	//            }
	//            else
	//            {
	//                // MD 11/8/11 - TFS85193
	//                // We must let the Combine use the defaults resolver because after combining with the default element, we may have nothing 
	//                // set to default anymore.
	//                //resolvedData = WorkbookFontData.Combine( this, defaultElement );
	//                resolvedData = WorkbookFontData.Combine(this, defaultElement, defaultsResolver);
	//            }

	//            // MD 11/8/11 - TFS85193
	//            // We couldn't do this here. It is too late to resolve defaults after combining with the default element, because that element
	//            // is fully populated when the workbook is loaded from a file. So instead, the defaults resolver is now used in the call to
	//            // Combine or when we create the cloned element above.
	//            //// MD 8/23/11 - TFS84306
	//            //// If an instance to resolve font property defaults was provided, let it resolve any defaults it wants.
	//            //if (defaultsResolver != null)
	//            //    defaultsResolver.ResolveDefaults(resolvedData);

	//            if ( resolvedData.name == null )
	//            {
	//                // MD 1/11/08 - BR29105
	//                // This was moved to a constant because it is used in other places now.
	//                //resolvedData.name = "Arial";
	//                resolvedData.name = Workbook.DefaultFontName;
	//            }

	//            if ( resolvedData.height < 0 )
	//                resolvedData.height = this.Workbook.DefaultFontHeight;

	//            // MD 1/19/11 - TFS62268
	//            // This OR is redudant because when the index is -1, the returned color is empty anyway.
	//            // And now that we also store the RGB data, it causes a problem when we have RGB data but no index.
	//            //if (resolvedData.colorIndex == -1 || resolvedData.Color == Utilities.ColorEmpty)
	//            if (resolvedData.Color == Utilities.ColorEmpty)
	//            {
	//                // MD 4/10/08 - BR31785
	//                // The default color is not Black, its WindowText. Use the alternate index for WindowText, because that is what Excel uses
	//                // when saving out. When Black was specified as the color for the first 4 default fonts written out in the BIFF8 format,
	//                // the bold and italic buttons on the toolbar in Excel 2003 no longer affected the format of selected cells with the default
	//                // font.
	//                //resolvedData.Color = Color.Black;
	//                resolvedData.ColorIndex = 0x7FFF;
	//            }

	//            if ( resolvedData.bold == ExcelDefaultableBoolean.Default )
	//                resolvedData.bold = ExcelDefaultableBoolean.False;

	//            if ( resolvedData.italic == ExcelDefaultableBoolean.Default )
	//                resolvedData.italic = ExcelDefaultableBoolean.False;

	//            if ( resolvedData.strikeout == ExcelDefaultableBoolean.Default )
	//                resolvedData.strikeout = ExcelDefaultableBoolean.False;

	//            if ( resolvedData.superscriptSubscriptStyle == FontSuperscriptSubscriptStyle.Default )
	//                resolvedData.superscriptSubscriptStyle = FontSuperscriptSubscriptStyle.None;

	//            if ( resolvedData.underlineStyle == FontUnderlineStyle.Default )
	//                resolvedData.underlineStyle = FontUnderlineStyle.None;

	//            return resolvedData;
	//        }

	//        #endregion ResolvedFontData

	//        // MD 11/10/11 - TFS85193
	//        #region SetIndexInFontCollection

	//        internal void SetIndexInFontCollection(FontResolverType fontResolverType, int value)
	//        {
	//            switch (fontResolverType)
	//            {
	//                case FontResolverType.Normal:
	//                    this.indexInFontCollection = value;
	//                    break;

	//                case FontResolverType.ShapeWithText:
	//                    this.indexInFontCollectionForShapesWithText = value;
	//                    break;

	//                default:
	//                    Utilities.DebugFail("Unknown FontResolverType: " + fontResolverType);
	//                    goto case FontResolverType.Normal;
	//            }
	//        }

	//        #endregion  // SetIndexInFontCollection

	//        #endregion Internal Methods

	//        #region Private Methods

	//        #region SelectiveCopyValues

	//        private void SelectiveCopyValues( WorkbookFontData source )
	//        {
	//            if ( source.name != null )
	//                this.name = source.name;

	//            if ( source.height != -1 )
	//                this.height = source.height;

	//            // MD 8/24/11 - TFS81451
	//            // We need to sync the colorIndex and color together. this has been moved below.
	//            //if (source.colorIndex != -1)
	//            //    this.colorIndex = source.colorIndex;

	//            if ( source.bold != ExcelDefaultableBoolean.Default )
	//                this.bold = source.bold;

	//            if ( source.italic != ExcelDefaultableBoolean.Default )
	//                this.italic = source.italic;

	//            if ( source.strikeout != ExcelDefaultableBoolean.Default )
	//                this.strikeout = source.strikeout;

	//            if ( source.superscriptSubscriptStyle != FontSuperscriptSubscriptStyle.Default )
	//                this.superscriptSubscriptStyle = source.superscriptSubscriptStyle;

	//            if ( source.underlineStyle != FontUnderlineStyle.Default )
	//                this.underlineStyle = source.underlineStyle;

	//            // MD 1/19/11 - TFS62268
	//            // Also copy the RGB data.
	//            // MD 8/24/11 - TFS81451
	//            // We need to sync the colorIndex and color together. 
	//            // If either one is set, we have to copy over both of them because they both describe the same attirbute of the font.
	//            // If we don't do this, we will get into a bad state when one field is set on the source and the other is set on the
	//            // destination. That the destination will get both fields set and they could be describing different colors.
	//            //if (Utilities.ColorIsEmpty(source.color) == false)
	//            //    this.color = source.color;
	//            if (Utilities.ColorIsEmpty(source.color) == false || source.colorIndex != -1)
	//            {
	//                this.color = source.color;
	//                this.colorIndex = source.colorIndex;
	//            }

	//            // MD 7/2/09 - TFS18634
	//            this.cachedHashCode = 0;
	//        }

	//        #endregion SelectiveCopyValues

	//        #endregion Private Methods

	//        #endregion Methods

	//        #region Properties

	//        #region Public Properties

	//        #region Bold

	//        public ExcelDefaultableBoolean Bold
	//        {
	//            get { return this.bold; }
	//            set 
	//            {
	//                if ( this.bold != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
	//                    if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

	//                    this.bold = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion Bold

	//        #region Color

	//        public Color Color
	//        {
	//            get
	//            {
	//                // MD 1/19/11 - TFS62268
	//                // If we can use the RGB data and it is set, use it.
	//                if (Utilities.IsRGBColorFormatSupported(this.Workbook.CurrentFormat) &&
	//                    Utilities.ColorIsEmpty(this.color) == false)
	//                {
	//                    return this.color;
	//                }

	//                return this.Workbook.Palette[ this.colorIndex ];
	//            }
	//            set
	//            {
	//                if (Utilities.ColorIsEmpty(value))
	//                {
	//                    this.colorIndex = -1;

	//                    // MD 1/19/11 - TFS62268
	//                    // Clear the RGB data as well.
	//                    this.color = Utilities.ColorEmpty;

	//                    return;
	//                }

	//                // MD 8/24/07 - BR25848
	//                // Another parameter is now needed for the Add method
	//                //this.colorIndex = this.Workbook.Palette.Add( value );
	//                // MD 8/30/07 - BR26111
	//                // Changed parameter to provide more info about the item getting a color
	//                //this.colorIndex = this.Workbook.Palette.Add( value, false );
	//                // MD 1/19/11 - TFS62268
	//                // If we can use the RGB data and the color is not a system color, use that instead of indexed color values.
	////#if SILVERLIGHT
	////                // GT 8/25/10 - Comparison of the colors in Silverlight doesn't include the name, so we shouldn't change the index
	////                if (this.colorIndex < 0)
	////#endif
	////                    this.colorIndex = this.Workbook.Palette.Add(value, ColorableItem.CellFont);
	//                if (Utilities.IsRGBColorFormatSupported(this.Workbook.CurrentFormat) &&
	//                    Utilities.ColorIsSystem(value) == false)
	//                {
	//                    this.color = value;
	//                    this.colorIndex = -1;
	//                }
	//                else
	//                {
	//                    this.color = Utilities.ColorEmpty;

	//#if SILVERLIGHT
	//                    // GT 8/25/10 - Comparison of the colors in Silverlight doesn't include the name, so we shouldn't change the index
	//                    if (this.colorIndex < 0)
	//#endif
	//                        this.colorIndex = this.Workbook.Palette.Add(value, ColorableItem.CellFill);
	//                }

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion Color

	//        #region Height

	//        public int Height
	//        {
	//            get { return this.height; }
	//            set 
	//            {
	//                if ( this.height != value )
	//                {
	//                    if ( value >= 0 && ( value < 20 || 8180 < value ) )
	//                        throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_FontHeight" ) );

	//                    this.height = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion Height

	//        #region Italic

	//        public ExcelDefaultableBoolean Italic
	//        {
	//            get { return this.italic; }
	//            set
	//            {
	//                if ( this.italic != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
	//                    if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

	//                    this.italic = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion Italic

	//        #region Name

	//        public string Name
	//        {
	//            get { return this.name; }
	//            set 
	//            {
	//                this.name = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion Name

	//        #region Strikeout

	//        public ExcelDefaultableBoolean Strikeout
	//        {
	//            get { return this.strikeout; }
	//            set
	//            {
	//                if ( this.strikeout != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
	//                    if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

	//                    this.strikeout = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion Strikeout

	//        #region SuperscriptSubscriptStyle

	//        public FontSuperscriptSubscriptStyle SuperscriptSubscriptStyle
	//        {
	//            get { return this.superscriptSubscriptStyle; }
	//            set
	//            {
	//                if ( this.superscriptSubscriptStyle != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( FontSuperscriptSubscriptStyle ), value ) == false )
	//                    if (Utilities.IsFontSuperscriptSubscriptStyleDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( FontSuperscriptSubscriptStyle ) );

	//                    this.superscriptSubscriptStyle = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion SuperscriptSubscriptStyle

	//        #region UnderlineStyle

	//        public FontUnderlineStyle UnderlineStyle
	//        {
	//            get { return this.underlineStyle; }
	//            set
	//            {
	//                if ( this.underlineStyle != value )
	//                {
	//                    // MD 10/21/10 - TFS34398
	//                    // Use the utility function instead of Enum.IsDefined. It is faster.
	//                    //if ( Enum.IsDefined( typeof( FontUnderlineStyle ), value ) == false )
	//                    if (Utilities.IsFontUnderlineStyleDefined(value) == false)
	//                        throw new InvalidEnumArgumentException( "value", (int)value, typeof( FontUnderlineStyle ) );

	//                    this.underlineStyle = value;

	//                    // MD 7/2/09 - TFS18634
	//                    this.cachedHashCode = 0;
	//                }
	//            }
	//        }

	//        #endregion UnderlineStyle

	//        #endregion Public Properties

	//        #region Internal Properties

	//        #region ColorIndex

	//        internal int ColorIndex
	//        {
	//            get { return this.colorIndex; }
	//            set
	//            {
	//                this.colorIndex = value;

	//                // MD 7/2/09 - TFS18634
	//                this.cachedHashCode = 0;
	//            }
	//        }

	//        #endregion ColorIndex

	//        // MD 1/19/11 - TFS62268
	//        #region ColorInternal

	//        internal Color ColorInternal
	//        {
	//            get { return this.color; }
	//        }

	//        #endregion ColorInternal

	//        // MD 11/10/11 - TFS85193
	//        #region Removed

	//        //#region IndexInFontCollection

	//        //internal int IndexInFontCollection
	//        //{
	//        //    get { return this.indexInFontCollection; }
	//        //    set { this.indexInFontCollection = value; }
	//        //}

	//        //#endregion IndexInFontCollection

	//        #endregion  // Removed

	//        #endregion Internal Properties

	//        #endregion Properties
	//    } 

	#endregion // Old Code
	// MD 2/2/12 - TFS100573
	// The workbook reference has been moved to the GenericCacheElementEx base type.
	//internal class WorkbookFontData : GenericCacheElement,
	internal class WorkbookFontData : GenericCacheElementEx,
		IWorkbookFont
	{
		#region Constants

		internal const int DefaultFontHeight = 220;
		internal const string DefaultFontName = "Calibri";

		// MD 2/5/12 - TFS100840
		internal const string DefaultMajorFontName = "Cambria";
		internal const string DefaultMinorFontName = "Calibri";

		#endregion // Constants

		#region Private Members

		private ExcelDefaultableBoolean _bold = ExcelDefaultableBoolean.Default;
		private int? _cachedHashCode;
		private WorkbookColorInfo _colorInfo;
		private int _height = -1;
		private ExcelDefaultableBoolean _italic = ExcelDefaultableBoolean.Default;
		private string _name;
		private ExcelDefaultableBoolean _strikeout = ExcelDefaultableBoolean.Default;
		private FontSuperscriptSubscriptStyle _superscriptSubscriptStyle = FontSuperscriptSubscriptStyle.Default;
		private FontUnderlineStyle _underlineStyle = FontUnderlineStyle.Default;

		#endregion Private Members

		#region Constructors

		public WorkbookFontData(Workbook workbook)
			: base(workbook) { }

		private WorkbookFontData(WorkbookFontData source)
			: this(source, source.Workbook) { }

		private WorkbookFontData(WorkbookFontData source, Workbook workbook)
			: this(workbook)
		{
			_bold = source._bold;
			_colorInfo = source._colorInfo;
			_height = source._height;
			_italic = source._italic;
			_name = source._name;
			_strikeout = source._strikeout;
			_superscriptSubscriptStyle = source._superscriptSubscriptStyle;
			_underlineStyle = source._underlineStyle;
		}

		#endregion Constructors

		#region Interfaces

		#region IWorkbookFont Members

		Color IWorkbookFont.Color
		{
			get
			{
				if (_colorInfo == null)
					return Utilities.ColorEmpty;

				return _colorInfo.GetResolvedColor(this.Workbook);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.ColorInfo = Utilities.ToColorInfo(value);
			}
		}

		#endregion // IWorkbookFont Members

		#endregion // Interfaces

		#region Base Class Overrides

		#region Clone

		public override object Clone(Workbook workbook)
		{
			return this.CloneInternal(workbook);
		}

		public WorkbookFontData CloneInternal(Workbook workbook)
		{
			return new WorkbookFontData(this, workbook);
		}

		#endregion Clone

		#region CopyValues

		public override void CopyValues(GenericCacheElement element)
		{
			WorkbookFontData other = element as WorkbookFontData;

			if (other == null)
			{
				Utilities.DebugFail("The specified element is not of the right type.");
				return;
			}

			this.SetFontFormatting(other);
		}

		#endregion // CopyValues

		#region Equals

		public override bool Equals(object obj)
		{
			return this.HasSameData(obj as GenericCacheElement);
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			if (_cachedHashCode.HasValue == false)
			{
				int hashCode = 0;

				if (_name != null)
					hashCode ^= _name.GetHashCode();

				hashCode ^= _height << 1;

				if (_colorInfo != null)
					hashCode ^= _colorInfo.GetHashCode() << 2;

				hashCode ^= (int)_bold << 3;
				hashCode ^= (int)_italic << 4;
				hashCode ^= (int)_strikeout << 5;
				hashCode ^= (int)_superscriptSubscriptStyle << 6;
				hashCode ^= (int)_underlineStyle << 7;

				_cachedHashCode = hashCode;
			}

			return _cachedHashCode.Value;
		}

		#endregion GetHashCode

		#region HasSameData

		public override bool HasSameData(GenericCacheElement otherElement)
		{
			if (otherElement == null)
				return false;

			if (this == otherElement)
				return true;

			WorkbookFontData otherFont = (WorkbookFontData)otherElement;

			if (_height != otherFont._height)
				return false;

			if (_bold != otherFont._bold)
				return false;

			if (_italic != otherFont._italic)
				return false;

			if (_strikeout != otherFont._strikeout)
				return false;

			if (_superscriptSubscriptStyle != otherFont._superscriptSubscriptStyle)
				return false;

			if (_underlineStyle != otherFont._underlineStyle)
				return false;

			if (_name != otherFont._name)
				return false;

			if (_colorInfo != otherFont._colorInfo)
				return false;

			return true;
		}

		#endregion HasSameData

		#region VerifyCanSetValue

		protected override void VerifyCanSetValue()
		{
			if (this.IsFrozen)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_ReadOnlyFont"));
		}

		#endregion // VerifyCanSetValue

		#endregion Base Class Overrides

		#region Methods

		#region Public Methods

		#region SetFontFormatting

		public void SetFontFormatting(IWorkbookFont source)
		{
			if (source == null)
				throw new ArgumentNullException("source", SR.GetString("LE_ArgumentNullException_SourceFont"));

			_name = source.Name;
			_height = source.Height;
			_colorInfo = source.ColorInfo;
			_bold = source.Bold;
			_italic = source.Italic;
			_strikeout = source.Strikeout;
			_superscriptSubscriptStyle = source.SuperscriptSubscriptStyle;
			_underlineStyle = source.UnderlineStyle;

			_cachedHashCode = null;
		}

		#endregion SetFontFormatting

		#endregion Public Methods

		#region Internal Methods

		#region Combine

		internal static WorkbookFontData Combine(WorkbookFontData highPriorityFnt, WorkbookFontData lowPriorityFnt)
		{
			// MD 11/8/11 - TFS85193
			// Moved all code to new overload of Combine
			return WorkbookFontData.Combine(highPriorityFnt, lowPriorityFnt, null);
		}

		// MD 11/8/11 - TFS85193
		// Added a new overload to take a IWorkbookFontDefaultsResolver parameter
		internal static WorkbookFontData Combine(WorkbookFontData highPriorityFnt, WorkbookFontData lowPriorityFnt, IWorkbookFontDefaultsResolver defaultsResolver)
		{
			// Initialize a new font with the lower priority font
			WorkbookFontData resolved = new WorkbookFontData(lowPriorityFnt);

			// MD 11/8/11 - TFS85193
			// If a defaults resolver is specified, resolve the properties of the high priority font. 
			// They will be taken into the combined font.
			if (defaultsResolver != null)
			{
				highPriorityFnt = new WorkbookFontData(highPriorityFnt);
				defaultsResolver.ResolveDefaults(highPriorityFnt);
			}

			// Copy all non-default values from the higher priority font to the resolved font
			resolved.SelectiveCopyValues(highPriorityFnt);

			return resolved;
		}

		#endregion Combine

		// MD 2/14/12 - 12.1 - Table Support
		#region CreateFont



#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

		internal static Font CreateFont(WorksheetCellFormatData format)
		{
			return WorkbookFontData.CreateFont(
				format.FontNameResolved,
				format.FontHeightResolved,
				format.FontBoldResolved,
				format.FontItalicResolved,
				format.FontUnderlineStyleResolved,
				format.FontStrikeoutResolved);
		}

		internal static Font CreateFont(string name, int height, ExcelDefaultableBoolean bold, ExcelDefaultableBoolean italic, FontUnderlineStyle underlineStyle, ExcelDefaultableBoolean strikeout)
		{
			// MD 3/25/12 - TFS104322
			// When Excel can't find a font, it uses Arial instead.
			//FontFamily family = new FontFamily(name);
			FontFamily family;
			try
			{
				family = new FontFamily(name);
			}
			catch (Exception)
			{
				family = new FontFamily("Arial");
			}

			FontStyle style = FontStyle.Regular;
			if (bold == ExcelDefaultableBoolean.True && family.IsStyleAvailable(FontStyle.Bold))
				style |= FontStyle.Bold;

			if (italic == ExcelDefaultableBoolean.True && family.IsStyleAvailable(FontStyle.Italic))
				style |= FontStyle.Italic;

			if (underlineStyle != FontUnderlineStyle.None && family.IsStyleAvailable(FontStyle.Underline))
				style |= FontStyle.Underline;

			if (strikeout == ExcelDefaultableBoolean.True && family.IsStyleAvailable(FontStyle.Strikeout))
				style |= FontStyle.Strikeout;

			return new Font(name, height / 20f, style);
		}


		#endregion // CreateFont

		#region Freeze

		internal void Freeze()
		{
			if (this.Collection != null)
			{
				Utilities.DebugFail("Cannot freeze a data element in a shared collection.");
				return;
			}

			this.IsFrozen = true;
		}

		#endregion // Freeze

		#region ResolvedFontData

		internal WorkbookFontData ResolvedFontData(IWorkbookFontDefaultsResolver defaultsResolver)
		{
			if (defaultsResolver == null)
				defaultsResolver = UltimateFontDefaultsResolver.Instance;

			WorkbookFontData resolvedData = new WorkbookFontData(this);

			if (defaultsResolver != null)
				defaultsResolver.ResolveDefaults(resolvedData);

			return resolvedData;
		}

		#endregion ResolvedFontData

		#endregion Internal Methods

		#region Private Methods

		#region SelectiveCopyValues

		private void SelectiveCopyValues(WorkbookFontData source)
		{
			if (source._name != null)
				_name = source._name;

			if (source._height != -1)
				_height = source._height;

			if (source._bold != ExcelDefaultableBoolean.Default)
				_bold = source._bold;

			if (source._italic != ExcelDefaultableBoolean.Default)
				_italic = source._italic;

			if (source._strikeout != ExcelDefaultableBoolean.Default)
				_strikeout = source._strikeout;

			if (source._superscriptSubscriptStyle != FontSuperscriptSubscriptStyle.Default)
				_superscriptSubscriptStyle = source._superscriptSubscriptStyle;

			if (source._underlineStyle != FontUnderlineStyle.Default)
				_underlineStyle = source._underlineStyle;

			if (source._colorInfo != null)
				_colorInfo = source._colorInfo;

			_cachedHashCode = null;
		}

		#endregion SelectiveCopyValues

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Bold

		public ExcelDefaultableBoolean Bold
		{
			get { return _bold; }
			set
			{
				this.VerifyCanSetValue();

				if (_bold == value)
					return;

				Utilities.VerifyExcelDefaultableBoolean(value, "value");
				_bold = value;
				_cachedHashCode = null;
			}
		}

		#endregion Bold

		#region ColorInfo

		public WorkbookColorInfo ColorInfo
		{
			get { return _colorInfo; }
			set
			{
				this.VerifyCanSetValue();

				if (_colorInfo == value)
					return;

				_colorInfo = value;
				_cachedHashCode = null;
			}
		}

		#endregion ColorInfo

		#region Height

		public int Height
		{
			get { return _height; }
			set
			{
				this.VerifyCanSetValue();

				if (_height == value)
					return;

				if (value >= 0 && (value < 20 || 8180 < value))
					throw new ArgumentOutOfRangeException("value", value, SR.GetString("LE_ArgumentOutOfRangeException_FontHeight"));

				_height = value;
				_cachedHashCode = null;

			}
		}

		#endregion Height

		#region Italic

		public ExcelDefaultableBoolean Italic
		{
			get { return _italic; }
			set
			{
				this.VerifyCanSetValue();

				if (_italic == value)
					return;

				Utilities.VerifyExcelDefaultableBoolean(value, "value");
				_italic = value;
				_cachedHashCode = null;
			}
		}

		#endregion Italic

		#region Name

		public string Name
		{
			get { return _name; }
			set
			{
				this.VerifyCanSetValue();

				if (_name == value)
					return;

				_name = value;
				_cachedHashCode = null;
			}
		}

		#endregion Name

		#region Strikeout

		public ExcelDefaultableBoolean Strikeout
		{
			get { return _strikeout; }
			set
			{
				this.VerifyCanSetValue();

				if (_strikeout == value)
					return;

				Utilities.VerifyExcelDefaultableBoolean(value, "value");
				_strikeout = value;
				_cachedHashCode = null;
			}
		}

		#endregion Strikeout

		#region SuperscriptSubscriptStyle

		public FontSuperscriptSubscriptStyle SuperscriptSubscriptStyle
		{
			get { return _superscriptSubscriptStyle; }
			set
			{
				this.VerifyCanSetValue();

				if (_superscriptSubscriptStyle == value)
					return;

				Utilities.VerifyEnumValue(value, "value");
				_superscriptSubscriptStyle = value;
				_cachedHashCode = null;
			}
		}

		#endregion SuperscriptSubscriptStyle

		#region UnderlineStyle

		public FontUnderlineStyle UnderlineStyle
		{
			get { return _underlineStyle; }
			set
			{
				this.VerifyCanSetValue();

				if (_underlineStyle == value)
					return;

				Utilities.VerifyEnumValue(value, "value");
				_underlineStyle = value;
				_cachedHashCode = null;
			}
		}

		#endregion UnderlineStyle

		#endregion Public Properties

		#region Internal Properties

		#region IsEmpty

		internal bool IsEmpty
		{
			get
			{
				return
					_bold == ExcelDefaultableBoolean.Default &&
					_colorInfo == null &&
					_height < 0 &&
					_italic == ExcelDefaultableBoolean.Default &&
					_name == null &&
					_strikeout == ExcelDefaultableBoolean.Default &&
					_superscriptSubscriptStyle == FontSuperscriptSubscriptStyle.Default &&
					_underlineStyle == FontUnderlineStyle.Default;
			}
		}

		#endregion // IsEmpty

		#endregion Internal Properties

		#endregion Properties
	}

	// MD 8/23/11 - TFS84306
	internal interface IWorkbookFontDefaultsResolver
	{
		void ResolveDefaults(WorkbookFontData font);
	}

	// MD 1/18/12 - 12.1 - Cell Format Updates
	internal class UltimateFontDefaultsResolver : IWorkbookFontDefaultsResolver
	{
		public static readonly UltimateFontDefaultsResolver Instance = new UltimateFontDefaultsResolver();

		private UltimateFontDefaultsResolver() { }

		public void ResolveDefaults(WorkbookFontData font)
		{
			if (font.Bold == ExcelDefaultableBoolean.Default)
				font.Bold = ExcelDefaultableBoolean.False;

			if (font.ColorInfo == null)
				font.ColorInfo = WorkbookColorInfo.Automatic;

			if (font.Height < 0)
				font.Height = WorkbookFontData.DefaultFontHeight;

			if (font.Italic == ExcelDefaultableBoolean.Default)
				font.Italic = ExcelDefaultableBoolean.False;

			if (font.Name == null)
				font.Name = WorkbookFontData.DefaultFontName;

			if (font.Strikeout == ExcelDefaultableBoolean.Default)
				font.Strikeout = ExcelDefaultableBoolean.False;

			if (font.SuperscriptSubscriptStyle == FontSuperscriptSubscriptStyle.Default)
				font.SuperscriptSubscriptStyle = FontSuperscriptSubscriptStyle.None;

			if (font.UnderlineStyle == FontUnderlineStyle.Default)
				font.UnderlineStyle = FontUnderlineStyle.None;
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