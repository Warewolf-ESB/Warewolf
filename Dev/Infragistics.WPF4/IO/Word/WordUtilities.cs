using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Infragistics.Documents.Core;


using System.Windows;
using System.Windows.Media;
using Infragistics.Documents.Shared;
using SizeF = System.Windows.Size;

//  BF 3/23/11
using Image = System.Windows.Media.Imaging.BitmapSource;








using SR = Infragistics.Shared.SR;


namespace Infragistics.Documents.Word
{
    #region WordUtilities class
    internal class WordUtilities
    {
        #region Constants

        /// <summary>
        /// Returns the number of English Metric Units (EMU) in one desktop publishing point (12,700).
        /// </summary>
		public const int EMUsPerPoint = 12700;
		
        /// <summary>
        /// Returns the number of desktop publishing points (DTPs) in one inch (72).
        /// </summary>
        public const int PointsPerInch = 72;

        /// <summary>
        /// Returns the number of TWIPs (twentieths of a point) in one desktop publishing point (20).
        /// </summary>
        public const int TwipsPerPoint = 20;

        /// <summary>
        /// Returns the number of TWIPs (twentieths of a point) in one inch (72 x 20 = 1440).
        /// </summary>
        public const int TwipsPerInch = TwipsPerPoint * PointsPerInch;

        /// <summary>
        /// Returns the number of centimeters in one inch (2.54).
        /// </summary>
        public const float CentimetersPerInch = 2.54f;

        /// <summary>
        /// Returns the number of TWIPs (twentieths of a point) in one centimeter (1440 / 2.54 = ~567).
        /// </summary>
        public const float TwipsPerCentimeter = 566.93f;

        /// <summary>
        /// Returns the number of decimal places to which converted floating-point values are rounded (2).
        /// </summary>
        public const int Precision = 2;

        /// <summary>
        /// Returns the default resolution for images, in dots per inch (96 x 96)
        /// </summary>
        public static readonly SizeF DefaultImageResolution = new SizeF( 96f, 96f );

        /// <summary>
        /// Returns the default unit of measurement (Point)
        /// </summary>
        internal const UnitOfMeasurement DefaultUnitOfMeasurement = UnitOfMeasurement.Point;

        /// <summary>
        /// Returns the default line spacing rule (auto)
        /// </summary>
        internal const LineSpacingRule DefaultLineSpacingRule = LineSpacingRule.Auto;

        /// <summary>
        /// Returns the default row height rule (auto)
        /// </summary>
        internal const RowHeightRule DefaultRowHeightRule = RowHeightRule.Auto;

        /// <summary>
        /// Returns the default ParagraphAlignment (Left)
        /// </summary>
        internal const ParagraphAlignment DefaultParagraphAlignment = ParagraphAlignment.Left;

        /// <summary>
        /// Returns the default width for a table column in twips (1440 = 1")
        /// </summary>
        internal const float DefaultTableColumnWidthInTwips = WordUtilities.TwipsPerInch;

        /// <summary>
        /// Returns the default border style for rows and cells (single)
        /// </summary>
        internal const TableBorderStyle DefaultTableBorderStyle = TableBorderStyle.Single;

        /// <summary>
        /// Returns the default outline style for picture outlines (none)
        /// </summary>
        internal const PictureOutlineStyle DefaultPictureOutlineStyle = PictureOutlineStyle.None;

        /// <summary>
        /// Returns the default corner style for picture outlines (round)
        /// </summary>
        internal const PictureOutlineCornerStyle DefaultPictureOutlineCornerStyle = PictureOutlineCornerStyle.Round;

        /// <summary>
        /// Returns the default text direction for cells (normal)
        /// </summary>
        internal const TableCellTextDirection DefaultTableCellTextDirection = TableCellTextDirection.Normal;

        /// <summary>
        /// Returns the maximum page width in inches (22).
        /// </summary>
        internal const int MaxPageWidthInInches = 22;

        /// <summary>
        /// Returns the maximum page width (22") in twentieths of a point.
        /// </summary>
        internal const int MaxPageWidthInTwips = (WordUtilities.TwipsPerInch * WordUtilities.MaxPageWidthInInches);

        /// <summary>
        /// Returns the default value of the NewLineType property (LineBreak)
        /// </summary>
        internal const NewLineType DefaultNewLineType = NewLineType.LineBreak;

        /// <summary>
        /// Returns the maximum number of columns that can appear in a table (63)
        /// </summary>
        internal const int MaxTableColumns = 63;

        /// <summary>
        /// Returns the default value for a page number field's type (Decimal).
        /// </summary>
        internal const PageNumberFieldFormat DefaultPageNumberFieldFormat = PageNumberFieldFormat.Decimal;

        #endregion Constants

        #region DebugFail  
        [Conditional("DEBUG")]
        internal static void DebugFail(string message)
        {
            CommonUtilities.DebugFail( message );
        }
        #endregion DebugFail

        #region RegexOptionsCompiled
        internal static RegexOptions RegexOptionsCompiled
        {
            get
            {



                return RegexOptions.Compiled;

            }
        }
        #endregion RegexOptionsCompiled

        #region GetXmlDeclaration
        /// <summary>
        /// Duplicated:
        /// Infragistics.Documents.Excel.Utilities.GetXmlDeclaration
        /// </summary>
        internal static string GetXmlDeclaration( string version, string encoding, string standalone )
        {
			StringBuilder builder = new StringBuilder( "version=\"" + version + "\"" );
			if ( encoding.Length > 0 )
			{
				builder.Append( " encoding=\"" );
				builder.Append( encoding );
				builder.Append( "\"" );
			}
			if ( standalone.Length > 0 )
			{
				builder.Append( " standalone=\"" );
				builder.Append( standalone );
				builder.Append( "\"" );
			}

            return builder.ToString();
        }
        #endregion GetXmlDeclaration

		#region GetFileName






		public static string GetFileName( Stream stream )
		{



			FileStream fileStream = stream as FileStream;

			if ( fileStream == null )
				return null;

			return fileStream.Name;

		}

		#endregion GetFileName

        #region ColorEmpty
        /// <summary>
	    /// Replace Color.Empty
	    /// </summary>
	    public static Color ColorEmpty
	    {
	        get
	        {

	            return SilverlightFixes.ColorEmpty;



	        }
	    }
        #endregion ColorEmpty

        #region ColorIsEmpty
        public static bool ColorIsEmpty(Color color)
        {

            return SilverlightFixes.ColorIsEmpty(color);



        }
        #endregion ColorIsEmpty

        #region StringPropertyGetHelper
        /// <summary>
        /// Returns the specified <paramref name="value"/> if it is not null,
        /// otherwise returns string.Empty.
        /// </summary>
        static public string StringPropertyGetHelper( string value )
        {
            //return string.IsNullOrEmpty( value ) ? string.Empty : value;
            return CommonUtilities.StringPropertyGetHelper( value );
        }
        #endregion StringPropertyGetHelper

        #region Convert
        /// <summary>
        /// Converts the specified <paramref name="value"/> from the UnitOfMeasurement
        /// as specified by <paramref name="from"/> to the unit as specified by <paramref name="to"/>.
        /// </summary>
        /// <returns>The converted value, expressed as specified by <paramref name="to"/>.</returns>
        static public float Convert( float value, UnitOfMeasurement from, UnitOfMeasurement to )
        {
            float twips = WordUtilities.ConvertToTwips( from, value );
            return WordUtilities.ConvertFromTwips( to, twips );
        }
        #endregion Convert

        #region ConvertToTwips

        /// <summary>
        /// Converts the specified <paramref name="value"/> to twentieths of a point,
        /// using the specified <paramref name="unit"/> of measure.
        /// </summary>
        /// <returns>The converted value, expressed in TWIPs</returns>
        static public float ConvertToTwips( UnitOfMeasurement unit, float value )
        {
            //  BF 12/7/10
            //  Don't round here, do the conversion first, then round
            //  the result at the end, otherwise we could be dropping
            //  significant digits.
            #region Obsolete
            //value = WordUtilities.RoundFloat( value );
            //if ( value == 0.00f )
            //    return value;
            #endregion Obsolete

            float retVal = value;

            switch ( unit )
            {
                case UnitOfMeasurement.Twip:
                    break;

                case UnitOfMeasurement.Point:
                    retVal = value * WordUtilities.TwipsPerPoint;
                    break;

                case UnitOfMeasurement.Inch:
                    retVal = value * WordUtilities.TwipsPerInch;
                    break;

                case UnitOfMeasurement.Centimeter:
                    retVal = value * WordUtilities.TwipsPerCentimeter;
                    break;

                default:
					SerializationUtilities.DebugFail(string.Format("The UnitOfMeasurement constant '{0}' is not handled.", unit.ToString()));
                    break;
            }

            return WordUtilities.RoundFloat(retVal);
        }
        #endregion ConvertToTwips

        #region ConvertFromTwips
        /// <summary>
        /// Converts the specified <paramref name="value"/> from twentieths of a point,
        /// using the specified <paramref name="unit"/> of measure.
        /// </summary>
        /// <returns>The converted value, expressed in units as defined by the value of the <paramref name="unit"/> property.</returns>
        static public float ConvertFromTwips( UnitOfMeasurement unit, float value )
        {
            value = WordUtilities.RoundFloat( value );
            if ( value == 0.00f )
                return value;

            float retVal = value;

            switch ( unit )
            {
                case UnitOfMeasurement.Twip:
                    break;

                case UnitOfMeasurement.Point:
                    retVal = value / WordUtilities.TwipsPerPoint;
                    break;

                case UnitOfMeasurement.Inch:
                    retVal = value / WordUtilities.TwipsPerInch;
                    break;

                case UnitOfMeasurement.Centimeter:
                    retVal = value / WordUtilities.TwipsPerCentimeter;
                    break;

                default:
					SerializationUtilities.DebugFail(string.Format("The UnitOfMeasurement constant '{0}' is not handled.", unit.ToString()));
                    break;
            }

            return WordUtilities.RoundFloat( retVal );
        }
        #endregion ConvertFromTwips

        #region ConvertSizeToTwips

        /// <summary>
        /// Converts the specified <paramref name="value"/> to twentieths of a point,
        /// using the specified <paramref name="unit"/> of measure.
        /// </summary>
        /// <returns>The converted value, expressed in TWIPs</returns>
        static public SizeF ConvertSizeToTwips( UnitOfMeasurement unit, SizeF value )
        {
            float width = WordUtilities.ConvertToTwips( unit, (float)value.Width );
            float height = WordUtilities.ConvertToTwips( unit, (float)value.Height );
            return new SizeF( width, height ); 
        }
        #endregion ConvertSizeToTwips

        #region ConvertSizeFromTwips

        /// <summary>
        /// Converts the specified <paramref name="value"/> from twentieths of a point,
        /// using the specified <paramref name="unit"/> of measure.
        /// </summary>
        /// <returns>The converted value, expressed in units as defined by the value of the <paramref name="unit"/> property.</returns>
        static public SizeF ConvertSizeFromTwips( UnitOfMeasurement unit, SizeF value )
        {




            float width = WordUtilities.ConvertFromTwips( unit, (float)value.Width );
			float height = WordUtilities.ConvertFromTwips(unit, (float)value.Height);

            return new SizeF( width, height ); 
        }
        #endregion ConvertSizeFromTwips

        #region ConvertPaddingToTwips

        /// <summary>
        /// Converts the specified <paramref name="value"/> to twentieths of a point,
        /// using the specified <paramref name="unit"/> of measure.
        /// </summary>
        /// <returns>The converted value, expressed in TWIPs</returns>
        static public Padding ConvertPaddingToTwips( UnitOfMeasurement unit, Padding value )
        {
            float top = WordUtilities.ConvertToTwips( unit, value.Top );
            float left = WordUtilities.ConvertToTwips( unit, value.Left );
            float bottom = WordUtilities.ConvertToTwips( unit, value.Bottom );
            float right = WordUtilities.ConvertToTwips( unit, value.Right );
            return new Padding( left, top, right, bottom ); 
        }
        #endregion ConvertPaddingToTwips

        #region ConvertPaddingFromTwips

        /// <summary>
        /// Converts the specified <paramref name="value"/> from twentieths of a point,
        /// using the specified <paramref name="unit"/> of measure.
        /// </summary>
        /// <returns>The converted value, expressed in units as defined by the value of the <paramref name="unit"/> property.</returns>
        static public Padding ConvertPaddingFromTwips( UnitOfMeasurement unit, Padding value )
        {
            float top = WordUtilities.ConvertFromTwips( unit, value.Top );
            float left = WordUtilities.ConvertFromTwips( unit, value.Left );
            float bottom = WordUtilities.ConvertFromTwips( unit, value.Bottom );
            float right = WordUtilities.ConvertFromTwips( unit, value.Right );
            return new Padding( left, top, right, bottom ); 
        }
        #endregion ConvertPaddingFromTwips

        #region RoundFloat
        static private float RoundFloat( float value )
        {
            return (float)Math.Round(value, WordUtilities.Precision);
        }
        #endregion RoundFloat

        #region SplitString
        /// <summary>
        /// Splits the specified string at the specified index and returns
        /// the two resulting substrings. Throws an IndexOutOfRangeException
        /// if the specified index is greater than text.Length. Always returns
        /// a string array with two non-null elements otherwise.
        /// </summary>
        static public string[] SplitString( int index, string text )
        {
            int originalLength = text.Length;
            if ( index < 0 || index > originalLength )
                throw new IndexOutOfRangeException();

            if ( originalLength == 0 )
                return new string[]{ string.Empty, string.Empty };
            if ( index == 0 )
                return new string[]{ string.Empty, text };
            else
            if ( index == originalLength )
                return new string[]{ text, string.Empty };

            string first = text.Substring(0, index);
            string last = text.Substring(index, originalLength - index);
            return new string[]{ first, last };
        }
        #endregion SplitString

        #region Combine
        /// <summary>
        /// Combines the two strings by inserting the specified <paramref name="source"/>
        /// string into the specified <paramref name="destination"/> string at the specified
        /// <paramref name="index"/>.
        /// </summary>
        static public string Combine( string destination, string source,  int index )
        {
            if ( string.IsNullOrEmpty(destination) && string.IsNullOrEmpty(source) )
                return string.Empty;

            if ( string.IsNullOrEmpty(destination) )
                return source;

            if ( string.IsNullOrEmpty(source) )
                return destination;

            string[] split = WordUtilities.SplitString(index, destination);
            StringBuilder sb = new StringBuilder(split[0].Length + split[1].Length);
            sb.Append( split[0] );
            sb.Append( source );
            sb.Append( split[1] );

            return sb.ToString();
        }
        #endregion Combine

		#region GetResolution

		/// <summary>
		/// Returns the graphical resolution for the specified image,
		/// expressed in dots per inch.
		/// </summary>
		/// <param name="image">The Image for which to return the resolution.</param>
		/// <returns>A SizeF struct containing the horizontal and vertical components of the resolution.</returns>
		public static SizeF GetResolution(Image image)
		{
            //  BF 3/23/11
            //  No way to get resolution on SL so assume 96dpi



			if (image == null)
				return WordUtilities.DefaultImageResolution;


			SizeF retVal = new SizeF(image.DpiX, image.DpiY);




			//  Handle the cases where the image's value aren't usable
			if (retVal.Width == 0f && retVal.Height == 0f)
				retVal = WordUtilities.DefaultImageResolution;
			else
				if (retVal.Width <= 0f && retVal.Height > 0f)
					retVal.Width = retVal.Height;
				else
					if (retVal.Height <= 0f && retVal.Width > 0f)
						retVal.Height = retVal.Width;

			return retVal;

		}

		#endregion GetResolution

		#region GetSize
		/// <summary>
		/// Returns the size of the specified image, expressed in the specified unit of measure.
		/// </summary>
		/// <param name="image">The Image for which to return the resolution.</param>
		/// <param name="unit">The unit of measurement to use.</param>
		/// <returns>A SizeF struct containing the horizontal and vertical components of the resolution.</returns>
		public static SizeF GetSize(Image image, UnitOfMeasurement unit)
		{
			if (image == null)
				return SizeF.Empty;

			//  Get the size in inches
			SizeF resolution = WordUtilities.GetResolution(image);
			SizeF sizeInInches =
				new SizeF(

					(float)image.PixelWidth / resolution.Width,
					(float)image.PixelHeight / resolution.Height);





			//  Convert as specified by the caller
			float width = WordUtilities.Convert((float)sizeInInches.Width, UnitOfMeasurement.Inch, unit);
			float height = WordUtilities.Convert((float)sizeInInches.Height, UnitOfMeasurement.Inch, unit);

			return new SizeF(width, height);
		}
		#endregion GetResolution

        #region ConcatenateContentRuns/ConcatenateTextRuns
        //static public string ConcatenateContentRuns( IEnumerable<ContentRun> runs, bool includePlaceholders )
        //{
        //    return WordUtilities.ConcatenateContentRunsHelper( runs, includePlaceholders );
        //}

        //static public string ConcatenateTextRuns( IEnumerable<TextRun> runs )
        //{
        //    return WordUtilities.ConcatenateContentRunsHelper( runs, false );
        //}

        //static private string ConcatenateContentRunsHelper( System.Collections.IEnumerable runs, bool includePlaceholders )
        //{
        //    if ( runs == null )
        //        return string.Empty;

        //    StringBuilder sb = new StringBuilder();
        //    foreach( object o in runs )
        //    {
        //        ContentRun run = o as ContentRun;

        //        if ( WordUtilities.ContentTypeDisplaysText(run.ContentType) == false )
        //        {
        //            if ( includePlaceholders == false )
        //                continue;
        //            else
        //                sb.Append( string.Format("{0}{1}{2}", "{", run.GetType().Name, "}") );

        //            continue;
        //        }

        //        sb.Append( run.ToString() );
        //    }

        //    return sb.ToString();
        //}
        #endregion ConcatenateContentRuns/ConcatenateTextRuns

        #region FontPropertyGetHelper / FontPropertySetHelper
        //static internal Font FontPropertyGetHelper( IDocumentEntity documentEntity, Font font )
        //{
        //    //  Lazily create
        //    if ( font == null )
        //        font = new Font( documentEntity );

        //    //  Verify so that it becomes associated with this instance
        //    //  if it is not associated with any IDocumentEntity
        //    font.VerifyDocumentEntity( documentEntity );

        //    return font;
        //}

        //static internal Font FontPropertySetHelper( IDocumentEntity documentEntity, Font value, bool verify )
        //{
        //    if ( value != null )
        //        value.InitDocumentEntity( documentEntity, verify );
        //    else
        //        value.DereferenceDocumentEntity( documentEntity );

        //    return value;
        //}
        #endregion FontPropertyGetHelper / FontPropertySetHelper

        #region VerifyFloatPropertySetting






        static internal void VerifyFloatPropertySetting(
            UnitOfMeasurement unit,
            string propertyName,
            float? value,
            float minValue,
            float maxValue )
        {
            if ( value.HasValue == false )
                return;

            if ( value.Value < minValue || value.Value > maxValue )
            {
                float val = WordUtilities.ConvertFromTwips( unit, value.Value );
                float min = WordUtilities.ConvertFromTwips( unit, minValue );
                float max = WordUtilities.ConvertFromTwips( unit, maxValue );

                string format = SR.GetString("Exception_PropertyValueOutOfRange");
                throw new ArgumentOutOfRangeException(
                    string.Format(
                        format,
                        val,
                        propertyName,
                        min,
                        max,
                        unit) );
            }
        }

        static internal void VerifyFloatPropertySetting(
            string unit,
            string propertyName,
            float? value,
            float minValue,
            float maxValue )
        {
            if ( value.HasValue == false )
                return;

            if ( value.Value < minValue || value.Value > maxValue )
            {
                string format = SR.GetString("Exception_PropertyValueOutOfRange");
                throw new ArgumentOutOfRangeException(
                    string.Format(
                        format,
                        value.Value,
                        propertyName,
                        minValue,
                        maxValue,
                        unit) );
            }
        }
        #endregion VerifyFloatPropertySetting

        #region GetResolvedFontListHelper
        
        //private delegate void CreateListDelegate( ref List<Font> listToCreate );
        
        ///// <summary>
        ///// Creates and populates the specified List(Of Font) with the fonts
        ///// to be used in a resolution pass.
        ///// </summary>
        ///// <param name="provider">The IFontProvider implementor. Could be a TextRun, Paragraph, TableCell, etc.</param>
        ///// <param name="list">A reference to the List(Of Font) to be populated. Can be initially null, in which case it is created as necessary. Can also be null upon return in which case nobody had anything to contribute.</param>
        ///// <returns>True if anything was added.</returns>
        //static internal bool GetResolvedFontListHelper( IFontProvider provider, ref List<Font> list )
        //{
        //    if ( provider == null )
        //        return false;

        //    //  Creates the list if necessary
        //    CreateListDelegate createList = delegate(ref List<Font> fontList)
        //    {
        //        if ( fontList == null )
        //            fontList = new List<Font>();
        //    };

        //    int originalListCount = list == null ? 0 : list.Count;

        //    //  If the specified provider has something to contribute,
        //    //  add it into the list.
        //    Font font = provider.Font;
        //    if ( font != null )
        //    {
        //        createList( ref list );
        //        list.Add( font );
        //    }

        //    //  Walk up the ancestor chain and find the closest one
        //    //  that also implements IFontProvider. If it finds one,
        //    //  call this method recursively on that implementor, who
        //    //  will do the same if it has a parent that is an
        //    //  IFontProvider, etc.
        //    IDocumentEntity item = provider as IDocumentEntity;
        //    IDocumentEntity parent = item != null ? item.Parent : null;
        //    while ( parent != null )
        //    {
        //        IFontProvider parentProvider = parent as IFontProvider;
        //        if ( parentProvider != null )
        //            return WordUtilities.GetResolvedFontListHelper( parentProvider, ref list );

        //        parent = parent.Parent;
        //    }

        //    //  Base the return value on whether anything was added to the list.
        //    int newListCount = list == null ? 0 : list.Count;
        //    return newListCount > originalListCount;
        //}
        #endregion GetResolvedFontListHelper

        #region SplitOnNewLine






        static internal string[] SplitOnNewLine( string text )
        {
            if ( string.IsNullOrEmpty(text) )
                return new string[]{ string.Empty };

            string newline = Environment.NewLine;

            int indexOf = text.IndexOf( newline );
            if ( indexOf < 0 )
                return new string[]{ text };

            int newlineLen = newline.Length;
            List<string> retVal = new List<string>(1);

            while ( text.Length > 0 )
            {
                //  If the text begins with a newline, grab it and pull it out
                if ( indexOf == 0 )
                {
                    retVal.Add( newline );
                    text = text.Remove(0, newlineLen);
                }
                else
                //  If the text has no newline, we're done so add the
                //  remaining text and bail
                if ( indexOf < 0 )
                {
                    retVal.Add( text );
                    break;
                }
                //  If the newline comes after some text, grab the text
                //  and the newline, and pull them both out
                else
                {
                    retVal.Add( text.Substring(0, indexOf) );
                    text = text.Remove(0, indexOf);

                    retVal.Add( newline );
                    text = text.Remove(0, newlineLen);
                }
                
                //  Get the index of the next newline
                indexOf = text.IndexOf( newline );
            }

            return retVal.ToArray();
        }

        #endregion SplitOnNewLine

        #region SizeEmpty
        internal static Size SizeEmpty
        {
            get
            {



                return Size.Empty;

            }
        }
        #endregion SizeEmpty

        #region ToSize
        internal static Size ToSize( SizeF value )
        {

            int width = (int)Math.Ceiling( value.Width );
            int height = (int)Math.Ceiling( value.Height );
            return new Size( width, height );



        }
        #endregion ToSize

        #region ToHexBinary3
        /// <summary>
        /// Returns a string that is compatible with the ST_HexBinary3
        /// data type from the specified color value.
        /// </summary>
        public static string ToHexBinary3(Color value)
        {
            Color color = Color.FromArgb(0, value.R, value.G, value.B);
            int colorValue = WordUtilities.ColorToArgb(color);
            return colorValue.ToString("X6");
        }
        #endregion ToHexBinary3

        #region ColorToArgb
        public static int ColorToArgb(Color color)
        {



            return SilverlightFixes.ColorToArgb(color);

        }
        #endregion ColorToArgb

		#region ColorFromArgb
		public static Color ColorFromArgb(byte red, byte green, byte blue)
		{



            return Color.FromArgb(255, red, green, blue);

		}
		#endregion ColorFromArgb

		#region TwipsToEMU
		public static int TwipsToEMU( double twips )
        {
            return ((int)twips * WordUtilities.EMUsPerPoint) / WordUtilities.TwipsPerPoint;
        }

		public static int TwipsToEMU( int twips )
		{
			return ( twips * WordUtilities.EMUsPerPoint ) / WordUtilities.TwipsPerPoint;
		}
		#endregion TwipsToEMU

        #region GetImageSizeInEMUs

        public static Size GetImageSizeInEMUs( Image image, SizeF? size, UnitOfMeasurement? unit )
        {
            UnitOfMeasurement unitOfMeasure = unit.HasValue ? unit.Value : WordUtilities.DefaultUnitOfMeasurement;

            //  Get the effective size in twips
            Size sizeInTwips =
                size.HasValue ?
                ToSize( WordUtilities.ConvertSizeToTwips(unitOfMeasure, size.Value) ) :
                ToSize( WordUtilities.GetSize(image, UnitOfMeasurement.Twip) );
            
            //  Convert to EMUs
            Size sizeInEMUs =
                new Size(
                    WordUtilities.TwipsToEMU( sizeInTwips.Width ),
                    WordUtilities.TwipsToEMU( sizeInTwips.Height ) );

            return sizeInEMUs;
        }

        #endregion GetImageSizeInEMUs

        #region FromPixels / ToPixels

        /// <summary>
        /// Converts the specified <paramref name="value"/> into the specified unit of measure.
        /// </summary>
        /// <param name="value">A value which represents a number of pixels.</param>
        /// <param name="dpi">The resolution, expressed in dots per inch.</param>
        /// <param name="unit">
        /// A
        /// <see cref="Infragistics.Documents.Word.UnitOfMeasurement">UnitOfMeasurement</see>
        /// constant which defines the units into which the value is to be converted.
        /// </param>
        static public float FromPixels( int value, float dpi, UnitOfMeasurement unit )
        {
            if ( value < 0 )
                throw new ArgumentOutOfRangeException( "value" );

            if ( dpi < 0f )
                throw new ArgumentOutOfRangeException( "dpi" );

            //  Get the value in inches
            float inches = value / dpi;

            //  Convert inches to units and return.
            return WordUtilities.Convert( inches, UnitOfMeasurement.Inch, unit );
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> into a pixel
        /// value at the specified resolution.
        /// </summary>
        /// <param name="value">A value which represents a linear graphical quantity.</param>
        /// <param name="dpi">The resolution, expressed in dots per inch.</param>
        /// <param name="unit">
        /// A
        /// <see cref="Infragistics.Documents.Word.UnitOfMeasurement">UnitOfMeasurement</see>
        /// constant which defines the unit of measure in which the <paramref name="value"/>
        /// is expressed.
        /// </param>
        static public int ToPixels( float value, float dpi, UnitOfMeasurement unit )
        {
            if ( value < 0f )
                throw new ArgumentOutOfRangeException( "value" );

            if ( dpi < 0f )
                throw new ArgumentOutOfRangeException( "dpi" );

            //  Convert inches to units
            float inches = WordUtilities.Convert( value, unit, UnitOfMeasurement.Inch );

            float retVal = (inches * dpi);
            return (int)(Math.Ceiling(retVal));
        }

        #endregion FromPixels / ToPixels

        #region UnitToString
        /// <summary>
        /// Returns a human readable string for the specified unit.
        /// </summary>
        static internal string UnitToString( UnitOfMeasurement unit )
        {
            switch ( unit )
            {
                case UnitOfMeasurement.Centimeter:
                    return "cm";
                case UnitOfMeasurement.Inch:
                    return "\"";
                case UnitOfMeasurement.Point:
                    return "pt";
                case UnitOfMeasurement.Twip:
                    return "twp";
            }

            return unit.ToString();
        }
        #endregion UnitToString
    }
    #endregion WordUtilities class
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