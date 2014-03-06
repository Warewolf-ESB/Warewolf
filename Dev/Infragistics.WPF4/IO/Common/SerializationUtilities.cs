using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;



using System.Windows;
using System.Windows.Media;
using Infragistics.Documents.Shared;
using SizeF = System.Windows.Size;









using Image = System.Windows.Media.Imaging.BitmapSource;


namespace Infragistics.Documents.Core
{
    #region SerializationUtilities class
    internal class SerializationUtilities
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

        #endregion Constants

        #region DebugFail
        
        //  Came from Infragistics.Documents.Excel.Utilities

        [Conditional("DEBUG")]
        internal static void DebugFail(string message)
        {



            Debug.Fail( message );

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


        #region GetResolution
        /// <summary>
        /// Returns the graphical resolution for the specified image,
        /// expressed in dots per inch.
        /// </summary>
        /// <param name="image">The Image for which to return the resolution.</param>
        /// <returns>A SizeF struct containing the horizontal and vertical components of the resolution.</returns>
        public static SizeF GetResolution(Image image)
        {
            if (image == null)
                return SerializationUtilities.DefaultImageResolution;


			SizeF retVal = new SizeF(image.DpiX, image.DpiY);




            //  Handle the cases where the image's value aren't usable
            if (retVal.Width == 0f && retVal.Height == 0f)
                retVal = SerializationUtilities.DefaultImageResolution;
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
        ///// <summary>
        ///// Returns the graphical resolution for the specified image,
        ///// expressed in dots per inch.
        ///// </summary>
        ///// <param name="image">The Image for which to return the resolution.</param>
        ///// <param name="unit">The unit of measurement to use.</param>
        ///// <returns>A SizeF struct containing the horizontal and vertical components of the resolution.</returns>
        //public static SizeF GetSize(Image image, UnitOfMeasurement unit)
        //{
        //    if (image == null)
        //        return SizeF.Empty;

        //    //  Get the size in inches
        //    SizeF resolution = SerializationUtilities.GetResolution(image);
        //    SizeF sizeInInches =
        //        new SizeF(
        //            (float)image.Size.Width / resolution.Width,
        //            (float)image.Size.Height / resolution.Height);

        //    //  Convert as specified by the caller
        //    float width = SerializationUtilities.Convert(sizeInInches.Width, UnitOfMeasurement.Inch, unit);
        //    float height = SerializationUtilities.Convert(sizeInInches.Height, UnitOfMeasurement.Inch, unit);

        //    return new SizeF(width, height);
        //}
        #endregion GetSize


        #region ConcatenateContentRuns/ConcatenateTextRuns
        //static public string ConcatenateContentRuns( IEnumerable<ContentRun> runs, bool includePlaceholders )
        //{
        //    return SerializationUtilities.ConcatenateContentRunsHelper( runs, includePlaceholders );
        //}

        //static public string ConcatenateTextRuns( IEnumerable<TextRun> runs )
        //{
        //    return SerializationUtilities.ConcatenateContentRunsHelper( runs, false );
        //}

        //static private string ConcatenateContentRunsHelper( System.Collections.IEnumerable runs, bool includePlaceholders )
        //{
        //    if ( runs == null )
        //        return string.Empty;

        //    StringBuilder sb = new StringBuilder();
        //    foreach( object o in runs )
        //    {
        //        ContentRun run = o as ContentRun;

        //        if ( SerializationUtilities.ContentTypeDisplaysText(run.ContentType) == false )
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
        //static internal void VerifyFloatPropertySetting(
        //    UnitOfMeasurement unit,
        //    string propertyName,
        //    float? value,
        //    float minValue,
        //    float maxValue )
        //{
        //    if ( value.HasValue == false )
        //        return;

        //    float newValue = SerializationUtilities.ConvertToTwips( unit, value.Value );

        //    if ( newValue < minValue || newValue > maxValue )
        //    {
        //        throw new ArgumentOutOfRangeException(
        //            string.Format(
        //                Exceptions.ExceptionMessages.PropertyValueOutOfRange,
        //                newValue,
        //                propertyName,
        //                minValue,
        //                maxValue,
        //                unit) );
        //    }
        //}
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
        //            return SerializationUtilities.GetResolvedFontListHelper( parentProvider, ref list );

        //        parent = parent.Parent;
        //    }

        //    //  Base the return value on whether anything was added to the list.
        //    int newListCount = list == null ? 0 : list.Count;
        //    return newListCount > originalListCount;
        //}
        #endregion GetResolvedFontListHelper

        #region SplitOnNewLine
//#if DEBUG
//        /// <summary>
//        /// Because string.Split does not provide a way to preserve
//        /// multiple instances of the character on which you split.
//        /// </summary>
//#endif
//        static internal string[] SplitOnNewLine( string text )
//        {
//            if ( string.IsNullOrEmpty(text) )
//                return new string[]{ string.Empty };

//            string newline = Environment.NewLine;

//            int indexOf = text.IndexOf( newline );
//            if ( indexOf < 0 )
//                return new string[]{ text };

//            int newlineLen = newline.Length;
//            List<string> retVal = new List<string>(1);

//            while ( text.Length > 0 )
//            {
//                //  If the text begins with a newline, grab it and pull it out
//                if ( indexOf == 0 )
//                {
//                    retVal.Add( newline );
//                    text = text.Remove(0, newlineLen);
//                }
//                else
//                //  If the text has no newline, we're done so add the
//                //  remaining text and bail
//                if ( indexOf < 0 )
//                {
//                    retVal.Add( text );
//                    break;
//                }
//                //  If the newline comes after some text, grab the text
//                //  and the newline, and pull them both out
//                else
//                {
//                    retVal.Add( text.Substring(0, indexOf) );
//                    text = text.Remove(0, indexOf);

//                    retVal.Add( newline );
//                    text = text.Remove(0, newlineLen);
//                }
                
//                //  Get the index of the next newline
//                indexOf = text.IndexOf( newline );
//            }

//            return retVal.ToArray();
//        }

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
            int colorValue = SerializationUtilities.ColorToArgb(color);
            return colorValue.ToString("X6");
        }
        #endregion ToHexBinary3

        #region ColorToArgb
        public static int ColorToArgb(Color color)
        {



            return SilverlightFixes.ColorToArgb(color);

        }
        #endregion ColorToArgb

    }
    #endregion SerializationUtilities class
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