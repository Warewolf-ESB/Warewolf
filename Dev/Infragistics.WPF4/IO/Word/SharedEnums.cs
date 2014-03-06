using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;






using System.Windows;
using System.Windows.Media;





namespace Infragistics.Documents.Word
{

    #region EnumConverter class





    internal class EnumConverter
    {
		// AS 2/15/11
		// Changed to threadstatic since this object gets manipulated by the static helper methods.
		//
		[ThreadStatic]
        static StringBuilder forCaseConversion = null;

        #region ParagraphAlignment

        /// <summary>
        /// ParagraphAlignment -> ST_Jc
        /// </summary>
        static internal string FromParagraphAlignment( ParagraphAlignment value )
        {
			return EnumCache<ParagraphAlignment>.ToString(value);
        }

        /// <summary>
        /// ST_Jc -> ParagraphAlignment.
        /// </summary>
        static internal ParagraphAlignment ToParagraphAlignment( string value )
        {
			return EnumCache<ParagraphAlignment>.FromString(value) ?? WordUtilities.DefaultParagraphAlignment;
        }

        #endregion ParagraphAlignment

        #region Underline

        /// <summary>
        /// Underline -> ST_Underline
        /// </summary>
        static internal string FromUnderline( Underline value )
        {
			return EnumCache<Underline>.ToString(value);
        }

        /// <summary>
        /// ST_Underline -> Underline
        /// </summary>
        static internal Underline ToUnderline( string value )
        {
			return EnumCache<Underline>.FromString(value) ?? Underline.None;
		}

        #endregion Underline

        #region RelativeHorizontalPosition

        /// <summary>
        /// RelativeHorizontalPosition -> ST_RelFromH
        /// </summary>
        static internal string FromRelativeHorizontalPosition( AnchorRelativeHorizontalPosition value )
        {
			return EnumCache<AnchorRelativeHorizontalPosition>.ToString(value);
        }

        /// <summary>
        /// ST_RelFromH -> RelativeHorizontalPosition
        /// </summary>
        static internal AnchorRelativeHorizontalPosition ToRelativeHorizontalPosition( string value )
        {
			return EnumCache<AnchorRelativeHorizontalPosition>.FromString(value) ?? AnchorRelativeHorizontalPosition.LeftMargin;
        }

        #endregion RelativeHorizontalPosition

        #region RelativeVerticalPosition

        /// <summary>
        /// RelativeVerticalPosition -> ST_RelFromV
        /// </summary>
        static internal string FromRelativeVerticalPosition( AnchorRelativeVerticalPosition value )
        {
			return EnumCache<AnchorRelativeVerticalPosition>.ToString(value);
        }

        /// <summary>
        /// ST_RelFromV -> RelativeVerticalPosition
        /// </summary>
        static internal AnchorRelativeVerticalPosition ToRelativeVerticalPosition( string value )
        {
			return EnumCache<AnchorRelativeVerticalPosition>.FromString(value) ?? AnchorRelativeVerticalPosition.TopMargin;
        }

        #endregion RelativeVerticalPosition

        #region AnchorHorizontalAlignment

        /// <summary>
        /// AnchorHorizontalAlignment -> ST_AlignH
        /// </summary>
        static internal string FromHorizontalAlignment( AnchorHorizontalAlignment value )
        {
            Debug.Assert(value != AnchorHorizontalAlignment.None, "Shouldn't be in here, 'None' implies it doesn't get used." );

			return EnumCache<AnchorHorizontalAlignment>.ToString(value);
        }

        /// <summary>
        /// ST_AlignH -> AnchorHorizontalAlignment
        /// </summary>
        static internal AnchorHorizontalAlignment ToHorizontalAlignment( string value )
        {
			return EnumCache<AnchorHorizontalAlignment>.FromString(value) ?? AnchorHorizontalAlignment.Left;
        }

        #endregion AnchorHorizontalAlignment

        #region AnchorVerticalAlignment

        /// <summary>
        /// AnchorVerticalAlignment -> ST_AlignV
        /// </summary>
        static internal string FromAnchorVerticalAlignment( AnchorVerticalAlignment value )
        {
            if ( value == AnchorVerticalAlignment.None )
                WordUtilities.DebugFail("Shouldn't be in here, 'None' implies it doesn't get used." );

			return EnumCache<AnchorVerticalAlignment>.ToString(value);
        }

        /// <summary>
        /// ST_AlignV -> VerticalAlignment
        /// </summary>
        static internal AnchorVerticalAlignment ToAnchorVerticalAlignment( string value )
        {
			return EnumCache<AnchorVerticalAlignment>.FromString(value) ?? AnchorVerticalAlignment.Top;
        }

        #endregion AnchorVerticalAlignment

        #region TableCellVerticalAlignment

        /// <summary>
        /// TableCellVerticalAlignment -> ST_VerticalJc
        /// </summary>
        static internal string FromTableCellVerticalAlignment( TableCellVerticalAlignment value )
        {
            Debug.Assert(value != TableCellVerticalAlignment.Default, "Shouldn't be in here, 'AllPages' implies it doesn't get used." );

			return EnumCache<TableCellVerticalAlignment>.ToString(value);
        }

        /// <summary>
        /// ST_VerticalJc -> TableCellVerticalAlignment
        /// </summary>
        static internal TableCellVerticalAlignment ToTableCellVerticalAlignment( string value )
        {
			return EnumCache<TableCellVerticalAlignment>.FromString(value) ?? TableCellVerticalAlignment.Default;
        }

        #endregion VerticalAlignment

        #region TextWrappingSide

        /// <summary>
        /// VerticalAlignment -> ST_WrapText
        /// </summary>
        static internal string FromTextWrappingSide( AnchorTextWrappingSide value )
        {
            switch ( value )
            {
                case AnchorTextWrappingSide.Both:
                    return "bothSides";

                default:
					return EnumCache<AnchorTextWrappingSide>.ToString(value);
            }

        }

        /// <summary>
        /// ST_WrapText -> TextWrappingSide
        /// </summary>
        static internal AnchorTextWrappingSide ToTextWrappingSide( string value )
        {
            switch ( value )
            {
                case "bothSides":
                    return AnchorTextWrappingSide.Both;

                default:
					return EnumCache<AnchorTextWrappingSide>.FromString(value) ?? AnchorTextWrappingSide.Both;
            }
        }

        #endregion VerticalAlignment

        #region FontVerticalAlignment

        /// <summary>
        /// FontVerticalAlignment -> ST_AlignV
        /// </summary>
        static internal string FromFontVerticalAlignment( FontVerticalAlignment value )
        {
			return EnumCache<FontVerticalAlignment>.ToString(value);
        }

        /// <summary>
        /// ST_AlignV -> FontVerticalAlignment
        /// </summary>
        static internal FontVerticalAlignment ToFontVerticalAlignment( string value )
        {
			return EnumCache<FontVerticalAlignment>.FromString(value) ?? FontVerticalAlignment.Baseline;
        }

        #endregion FontVerticalAlignment

        #region TableCellVerticalMerge
        /// <summary>
        /// TableCellVerticalMerge -> ST_Merge
        /// </summary>
        static internal string FromTableCellVerticalMerge( TableCellVerticalMerge value )
        {
            switch ( value )
            {
                case TableCellVerticalMerge.Continue:
                    return "continue";

                case TableCellVerticalMerge.None:
                case TableCellVerticalMerge.Start:
                    return "restart";
                default:
					EnumCache<TableCellVerticalMerge>.ValidateEnum(value);
                    return "restart";
            }
        }

        /// <summary>
        /// ST_Merge -> TableCellVerticalMerge
        /// </summary>
        static internal TableCellVerticalMerge ToTableCellVerticalMerge( string value )
        {
            if ( string.IsNullOrEmpty(value) )
                return TableCellVerticalMerge.None;

            return value.Equals("restart") ? TableCellVerticalMerge.Start : TableCellVerticalMerge.Continue;
        }
        #endregion TableCellVerticalMerge

        #region LineSpacingRule

        /// <summary>
        /// LineSpacingRule -> ST_LineSpacingRule
        /// </summary>
        static internal string FromLineSpacingRule( LineSpacingRule value )
        {
			return EnumCache<LineSpacingRule>.ToString(value);
        }

        /// <summary>
        /// ST_LineSpacingRule -> LineSpacingRule.
        /// </summary>
        static internal LineSpacingRule ToLineSpacingRule( string value )
        {
			return EnumCache<LineSpacingRule>.FromString(value) ?? WordUtilities.DefaultLineSpacingRule;
        }

        #endregion LineSpacingRule

        #region RowHeightRule

        /// <summary>
        /// RowHeightRule -> ST_RowHeightRule
        /// </summary>
        static internal string FromRowHeightRule( RowHeightRule value )
        {
			return EnumCache<RowHeightRule>.ToString(value);
		}

        /// <summary>
        /// ST_RowHeightRule -> RowHeightRule.
        /// </summary>
        static internal RowHeightRule ToRowHeightRule( string value )
        {
			return EnumCache<RowHeightRule>.FromString(value) ?? WordUtilities.DefaultRowHeightRule;
		}

        #endregion RowHeightRule

        #region TableBorderStyle

		internal const string NilBorder = "nil";

        /// <summary>
        /// TableBorderStyle -> ST_Border
        /// </summary>
        static internal string FromTableBorderStyle( TableBorderStyle value )
        {
			return EnumCache<TableBorderStyle>.ToString(value);
        }

        /// <summary>
        /// ST_Border -> TableBorderStyle.
        /// </summary>
        static internal TableBorderStyle ToTableBorderStyle( string value )
        {
			// word also uses nil for none in some situations
			if (value == NilBorder)
				return TableBorderStyle.None;

			return EnumCache<TableBorderStyle>.FromString(value) ?? WordUtilities.DefaultTableBorderStyle;
		}

        #endregion TableBorderStyle

        #region PictureOutlineStyle

        /// <summary>
        /// PictureOutlineStyle -> ST_CompoundLine
        /// (see 5.1.12.15 ST_CompoundLine (Compound Line Type))
        /// </summary>
        static internal string FromPictureOutlineStyle( PictureOutlineStyle value )
        {
            switch ( value )
            {
                case PictureOutlineStyle.None:
                    return string.Empty;

                case PictureOutlineStyle.Single:
                    return "sng";

                case PictureOutlineStyle.Double:
                    return "dbl";

                case PictureOutlineStyle.ThinThick:
                case PictureOutlineStyle.ThickThin:
                    return EnumCache<PictureOutlineStyle>.ToString( value );

                case PictureOutlineStyle.Triple:
                    return "tri";

                default:
                    WordUtilities.DebugFail( "Unrecognized PictureOutlineStyle value." );
					EnumCache<PictureOutlineStyle>.ValidateEnum(value);
                    return string.Empty;
            }
        }

        /// <summary>
        /// ST_CompoundLine -> PictureOutlineStyle.
        /// </summary>
        static internal PictureOutlineStyle ToPictureOutlineStyle( string value )
        {
            switch ( value )
            {
                case "":
                    return PictureOutlineStyle.None;
                case "Double":
                    return PictureOutlineStyle.Double;
                case "Single":
                    return PictureOutlineStyle.Single;
                case "ThickThin":
                    return PictureOutlineStyle.ThickThin;
                case "ThinThick":
                    return PictureOutlineStyle.ThinThick;
                case "Triple":
                    return PictureOutlineStyle.Triple;

                default:
                    WordUtilities.DebugFail( "Unrecognized PictureOutlineStyle value." );
                    return WordUtilities.DefaultPictureOutlineStyle;
            }
        }

        #endregion PictureOutlineStyle

        #region TableCellTextDirection

        /// <summary>
        /// TableCellTextDirection -> ST_TextDirection
        /// (see 2.18.100 ST_TextDirection (Text Flow Direction))
        /// </summary>
        static internal string FromTableCellTextDirection( TableCellTextDirection value )
        {
            switch ( value )
            {
                case TableCellTextDirection.Normal:
                    return string.Empty;

                case TableCellTextDirection.BottomToTopLeftToRight:
                    return "btLr";

                case TableCellTextDirection.LeftToRightTopToBottom:
                    return "lrTb";

                case TableCellTextDirection.LeftToRightTopToBottomRotated:
                    return "lrTbV";

                case TableCellTextDirection.TopToBottomRightToLeft:
                    return "tbRl";

                case TableCellTextDirection.TopToBottomLeftToRightRotated:
                    return "tbLrV";

                case TableCellTextDirection.TopToBottomRightToLeftRotated:
                    return "tbRlV";

                default:
                    WordUtilities.DebugFail( "Unrecognized TableCellTextDirection value." );
					EnumCache<TableCellTextDirection>.ValidateEnum(value);
                    return string.Empty;
            }
        }

        /// <summary>
        /// ST_TextDirection -> TableCellTextDirection.
        /// </summary>
        static internal TableCellTextDirection ToTableCellTextDirection( string value )
        {
            switch ( value )
            {
                case "":
                    return TableCellTextDirection.Normal;

                case "btLr":
                    return TableCellTextDirection.BottomToTopLeftToRight;

                case "lrTb":
                    return TableCellTextDirection.LeftToRightTopToBottom;

                case "lrTbV":
                    return TableCellTextDirection.LeftToRightTopToBottomRotated;

                case "tbRl":
                    return TableCellTextDirection.TopToBottomRightToLeft;

                case "tbLrV":
                    return TableCellTextDirection.TopToBottomLeftToRightRotated;

                case "tbRlV":
                    return TableCellTextDirection.TopToBottomRightToLeftRotated;

                default:
                    WordUtilities.DebugFail( "Unrecognized TableCellTextDirection value." );
                    return WordUtilities.DefaultTableCellTextDirection;
            }
        }

        #endregion TableCellTextDirection

        #region PageNumberFieldFormat
        /// <summary>
        /// See: 2.16.4.3 General formatting
        /// </summary>
        static internal string FromPageNumberFieldFormat( PageNumberFieldFormat value )
        {
            string s = null;

            switch ( value )
            {
                case PageNumberFieldFormat.Decimal:
                    s = "Arabic";
                    break;

                case PageNumberFieldFormat.DecimalCircle:
                    s = "CIRCLENUM";
                    break;

                case PageNumberFieldFormat.DecimalDash:
                    s = "ArabicDash";
                    break;

                case PageNumberFieldFormat.DecimalParenthesis:
                    s = "GB2";
                    break;

                case PageNumberFieldFormat.LatinAlphabeticUppercase:
                    s = "ALPHABETIC";
                    break;

                case PageNumberFieldFormat.LatinAlphabeticLowercase:
                    s = "alphabetic";
                    break;

                case PageNumberFieldFormat.RomanLowercase:
                    s = "roman";
                    break;

                case PageNumberFieldFormat.RomanUppercase:
                    s = "Roman";
                    break;

                case PageNumberFieldFormat.ArabicAbjad:
                    s = "ARABICABJAD";
                    break;

                case PageNumberFieldFormat.ArabicAlphabetic:
                    s = "ARABICALPHA";
                    break;

                case PageNumberFieldFormat.Chinese:
                    s = "CHINESENUM1";
                    break;

                case PageNumberFieldFormat.ChineseLegalSimplified:
                    s = "CHINESENUM2";
                    break;

                case PageNumberFieldFormat.ChineseCountingThousand:
                    s = "CHINESENUM3";
                    break;

                case PageNumberFieldFormat.Hebrew:
                    s = "HEBREW1";
                    break;

                case PageNumberFieldFormat.HebrewAlphabetic:
                    s = "HEBREW2";
                    break;

                case PageNumberFieldFormat.JapaneseKanji:
                    s = "KANJINUM1";
                    break;

                case PageNumberFieldFormat.JapaneseCounting:
                    s = "KANJINUM2";
                    break;

                case PageNumberFieldFormat.JapaneseLegal:
                    s = "KANJINUM3";
                    break;

                case PageNumberFieldFormat.JapaneseIroha:
                    s = "IROHA";
                    break;

                case PageNumberFieldFormat.JapaneseDigitalTenThousand:
                    s = "DBNUM4";
                    break;

                case PageNumberFieldFormat.KoreanChosung:
                    s = "CHOSUNG";
                    break;

                case PageNumberFieldFormat.KoreanCounting:
                    s = "DBNUM2";
                    break;

                case PageNumberFieldFormat.KoreanGanada:
                    s = "GANADA";
                    break;

                case PageNumberFieldFormat.Thai:
                    s = "THAIARABIC";
                    break;

                case PageNumberFieldFormat.ThaiCounting:
                    s = "THAICARDTEXT";
                    break;

                case PageNumberFieldFormat.ThaiLetter:
                    s = "THAILETTER";
                    break;

                case PageNumberFieldFormat.Ordinal:
                    s = "Ordinal";
                    break;

                case PageNumberFieldFormat.TextOrdinal:
                    s = "OrdText";
                    break;

                case PageNumberFieldFormat.TextCardinal:
                    s = "CardText";
                    break;

                default:
                    WordUtilities.DebugFail( string.Format("Unrecognized constant ('{0}') in FromPageNumberFieldFormat.", value) );
                    s = string.Empty;
                    break;

            }

            return s;
        }
        #endregion PageNumberFieldFormat

        #region AnchorRelativeVerticalPosition (VML)

        static internal string FromAnchorRelativeVerticalPosition_Vml( AnchorRelativeVerticalPosition value )
        {
            Type enumType = typeof(AnchorRelativeVerticalPosition);
            if ( Enum.IsDefined(enumType, value) == false )
                throw new ArgumentException( string.Format("Value '{0}' is not a valid value for {1}.", value, enumType) );

            switch ( value )
            {
                case AnchorRelativeVerticalPosition.Line:
                    return "line";

                case AnchorRelativeVerticalPosition.Margin:
                    return "margin";

                case AnchorRelativeVerticalPosition.InsideMargin:
                    return "inner-margin-area";

                case AnchorRelativeVerticalPosition.OutsideMargin:
                    return "outer-margin-area";

                case AnchorRelativeVerticalPosition.TopMargin:
                    return "top-margin-area";

                case AnchorRelativeVerticalPosition.BottomMargin:
                    return "bottom-margin-area";

                case AnchorRelativeVerticalPosition.Paragraph:
                    return "text";

                default:
                    return "page";
            }
        }

        /// <summary>
        /// ST_CompoundLine -> AnchorRelativeVerticalPosition.
        /// </summary>
        static internal AnchorRelativeVerticalPosition ToAnchorRelativeVerticalPosition_Vml( string value )
        {
            switch ( value )
            {
                case "line":
                    return AnchorRelativeVerticalPosition.Line;
                case "margin":
                    return AnchorRelativeVerticalPosition.Margin;
                case "text":
                    return AnchorRelativeVerticalPosition.Paragraph;
                case "inner-margin-area":
                    return AnchorRelativeVerticalPosition.InsideMargin;
                case "outer-margin-area":
                    return AnchorRelativeVerticalPosition.OutsideMargin;
                case "top-margin-area":
                    return AnchorRelativeVerticalPosition.TopMargin;
                case "bottom-margin-area":
                    return AnchorRelativeVerticalPosition.BottomMargin;
                case "page":
                    return AnchorRelativeVerticalPosition.Page;

                default:
                    WordUtilities.DebugFail( "Unrecognized AnchorRelativeVerticalPosition value." );
                    return AnchorRelativeVerticalPosition.Page;
            }
        }

        #endregion AnchorRelativeVerticalPosition (VML)

        #region AnchorRelativeHorizontalPosition (VML)

        static internal string FromAnchorRelativeHorizontalPosition_Vml( AnchorRelativeHorizontalPosition value )
        {
            Type enumType = typeof(AnchorRelativeHorizontalPosition);
            if ( Enum.IsDefined(enumType, value) == false )
                throw new ArgumentException( string.Format("Value '{0}' is not a valid value for {1}.", value, enumType) );

            switch ( value )
            {
                case AnchorRelativeHorizontalPosition.Character:
                    return "char";

                case AnchorRelativeHorizontalPosition.Margin:
                    return "margin";

                case AnchorRelativeHorizontalPosition.InsideMargin:
                    return "inner-margin-area";

                case AnchorRelativeHorizontalPosition.OutsideMargin:
                    return "outer-margin-area";

                case AnchorRelativeHorizontalPosition.LeftMargin:
                    return "left-margin-area";

                case AnchorRelativeHorizontalPosition.RightMargin:
                    return "right-margin-area";

                case AnchorRelativeHorizontalPosition.Column:
                    return "text";

                default:
                    return "page";
            }
        }

        /// <summary>
        /// ST_CompoundLine -> AnchorRelativeHorizontalPosition.
        /// </summary>
        static internal AnchorRelativeHorizontalPosition ToAnchorRelativeHorizontalPosition_Vml( string value )
        {
            switch ( value )
            {
                case "char":
                    return AnchorRelativeHorizontalPosition.Character;
                case "margin":
                    return AnchorRelativeHorizontalPosition.Margin;
                case "text":
                    return AnchorRelativeHorizontalPosition.Column;
                case "inner-margin-area":
                    return AnchorRelativeHorizontalPosition.InsideMargin;
                case "outer-margin-area":
                    return AnchorRelativeHorizontalPosition.OutsideMargin;
                case "left-margin-area":
                    return AnchorRelativeHorizontalPosition.LeftMargin;
                case "right-margin-area":
                    return AnchorRelativeHorizontalPosition.RightMargin;
                case "page":
                    return AnchorRelativeHorizontalPosition.Page;

                default:
                    WordUtilities.DebugFail( "Unrecognized AnchorRelativeHorizontalPosition value." );
                    return AnchorRelativeHorizontalPosition.Page;
            }
        }

        #endregion AnchorRelativeHorizontalPosition (VML)

        #region AnchorHorizontalAlignment (VML)

        static internal string FromAnchorHorizontalAlignment_Vml( AnchorHorizontalAlignment value )
        {
			return EnumCache<AnchorHorizontalAlignment>.ToString(value);
            //Type enumType = typeof(AnchorHorizontalAlignment);
            //if ( Enum.IsDefined(enumType, value) == false )
            //    throw new ArgumentException( string.Format("Value '{0}' is not a valid value for {1}.", value, enumType) );

            //return EnumConverter.FromEnumToCamelCaseHelper( enumType, value );
        }

        static internal AnchorHorizontalAlignment ToAnchorHorizontalAlignment_Vml( string value )
        {
			return EnumCache<AnchorHorizontalAlignment>.FromString(value) ?? AnchorHorizontalAlignment.None;
            //object o = EnumConverter.ToEnumHelper( typeof(AnchorHorizontalAlignment), value );
            //return o != null ? (AnchorHorizontalAlignment)o : AnchorHorizontalAlignment.None;
        }

        #endregion AnchorHorizontalAlignment (VML)

        #region AnchorVerticalAlignment (VML)

        static internal string FromAnchorVerticalAlignment_Vml( AnchorVerticalAlignment value )
        {
			return EnumCache<AnchorVerticalAlignment>.ToString(value);
            //Type enumType = typeof(AnchorVerticalAlignment);
            //if ( Enum.IsDefined(enumType, value) == false )
            //    throw new ArgumentException( string.Format("Value '{0}' is not a valid value for {1}.", value, enumType) );

            //return EnumConverter.FromEnumToCamelCaseHelper( enumType, value );
        }

        static internal AnchorVerticalAlignment ToAnchorVerticalAlignment_Vml( string value )
        {
			return EnumCache<AnchorVerticalAlignment>.FromString(value) ?? AnchorVerticalAlignment.None;
            //object o = EnumConverter.ToEnumHelper( typeof(AnchorVerticalAlignment), value );
            //return o != null ? (AnchorVerticalAlignment)o : AnchorVerticalAlignment.None;
        }

        #endregion AnchorVerticalAlignment (VML)

        #region ShapeLineStyle
        //  <v:stroke dashstyle="solid" /> 
        static internal string FromShapeLineStyle( ShapeLineStyle value )
        {
			return value.ToString().ToLower();
            //Type enumType = typeof(AnchorVerticalAlignment);
            //if ( Enum.IsDefined(enumType, value) == false )
            //    throw new ArgumentException( string.Format("Value '{0}' is not a valid value for {1}.", value, enumType) );

            //return EnumConverter.FromEnumToCamelCaseHelper( enumType, value );
        }

        //static internal ShapeLineStyle ToShapeLineStyle( string value )
        //{
        //}

        #endregion ShapeLineStyle
        
        #region FromEnumToCamelCaseHelper
        static private string FromEnumToCamelCaseHelper( Type enumType, object value )
        {
            if ( value == null )
            {
				WordUtilities.DebugFail("Null or non-string value in FromEnumToCamelCaseHelper");
                return string.Empty;
            }

            string s = value.ToString();
            if ( s.Length < 1 )
            {
				WordUtilities.DebugFail("Empty string in FromEnumToCamelCaseHelper");
                return string.Empty;
            }

            return EnumConverter.FirstCharToLower( s );
        }

        #endregion FromEnumToCamelCaseHelper

        #region ToEnumHelper
		// AS 2/15/11
		// This is no longer being used.
		//
        //static private object ToEnumHelper( Type enumType, string value )
        //{
        //    value = EnumConverter.FirstCharToUpper( value );
		
        //    if ( Enum.IsDefined(enumType, value) )            
        //        return Enum.Parse(enumType, value, false);
        //    else
        //        WordUtilities.DebugFail(string.Format("Value '{0}' is not recognized by the {1} enumeration", value, enumType.Name));
		
        //    return null;
        //}
        #endregion ToEnumHelper

        #region FirstCharToLower/Upper
        static private string FirstCharToLower( string value )
        {
            if ( EnumConverter.forCaseConversion == null )
                EnumConverter.forCaseConversion = new StringBuilder();
            else
                EnumConverter.forCaseConversion.Remove( 0, EnumConverter.forCaseConversion.Length );

            StringBuilder sb = EnumConverter.forCaseConversion;            
            sb.Append( value.Substring(0, 1).ToLower() );
            sb.Append( value.Substring(1, value.Length - 1) );
            return sb.ToString();
        }
        static private string FirstCharToUpper( string value )
        {
            if ( EnumConverter.forCaseConversion == null )
                EnumConverter.forCaseConversion = new StringBuilder();
            else
                EnumConverter.forCaseConversion.Remove( 0, EnumConverter.forCaseConversion.Length );

            StringBuilder sb = EnumConverter.forCaseConversion;           
            sb.Append( value.Substring(0, 1).ToUpper() );
            sb.Append( value.Substring(1, value.Length - 1) );
            return sb.ToString();
        }
        #endregion FirstCharToLower/Upper

		// AS 2/15/11
		// Instead of building a string to convert to a string or parsing a string we can just cache 
		// the results of the enums. Also Enum.IsDefined is an expensive operation and it's not really 
		// needed - if we don't have the enum in our dictionary then we know it's not valid since we 
		// build the table using Enum.GetValues.
		//
		#region EnumCache class
		private class EnumCache<T> where T : struct
		{
			#region Member Variables

			private static Dictionary<T, string> _toStringCache;
			private static Dictionary<string, T?> _fromStringCache;

			#endregion //Member Variables

			#region Constructor
			static EnumCache()
			{
				Type enumType = typeof(T);
				Debug.Assert(enumType.IsEnum, "This is only meant for enums!");
				object[] attribs = enumType.GetCustomAttributes(typeof(FlagsAttribute), true);
				Debug.Assert(attribs == null || attribs.Length == 0, "This may not work well for flagged attributes");

				Dictionary<T, string> toStringTable = new Dictionary<T, string>();
				Dictionary<string, T?> fromStringTable = new Dictionary<string, T?>();

				foreach (T value in GetEnumValues(enumType))
				{
					string toString = EnumConverter.FromEnumToCamelCaseHelper(enumType, value);
					toStringTable[value] = toString;
					fromStringTable[toString] = value;
				}

				_fromStringCache = fromStringTable;
				_toStringCache = toStringTable;
			}
			#endregion //Constructor

			#region Methods

			#region FromString
			internal static T? FromString(string value)
			{
				T? enumValue;
				if (!_fromStringCache.TryGetValue(value, out enumValue))
				{
					WordUtilities.DebugFail(string.Format("Value '{0}' is not recognized by the {1} enumeration", value, typeof(T).Name));
				}

				return enumValue;
			} 
			#endregion //FromString

			#region GetEnumValues
			private static System.Collections.IList GetEnumValues(Type enumType)
			{


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				return Enum.GetValues(enumType);

			}
			#endregion //GetEnumValues

			#region ToString
			internal static string ToString(T value)
			{
				string stringValue;
				if (!_toStringCache.TryGetValue(value, out stringValue))
				{
					throw new ArgumentException(string.Format("Value '{0}' is not a valid value for {1}.", value, typeof(T)));
				}

				return stringValue;
			} 
			#endregion //ToString

			#region ValidateEnum
			internal static void ValidateEnum(T value)
			{
				// just call the to string since that throws an exception
				ToString(value);
			} 
			#endregion //ValidateEnum

			#endregion //Methods
		} 
		#endregion //EnumCache class
    }
    #endregion EnumConverter class


    #region ParagraphAlignment enumeration
    //
    //  2.18.50 ST_Jc (Horizontal Alignment Type)
    //  OpenXML pg. 1749
    //
    /// <summary>
    /// Constants which specify the alignment for a paragraph or table.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum ParagraphAlignment
    {
        /// <summary>
        /// Specifies that the text is justified between both
        /// of the text margins in the document.
        /// </summary>
        Both,

        /// <summary>
        /// Specifies that the text is centered on the line
        /// between both of the text margins in the document.
        /// </summary>
        Center,

        /// <summary>
        /// Specifies that the text is justified between both
        /// of the text margins in the document.
        /// </summary>
        Distribute,

        /// <summary>
        /// Specifies that the kashida length for text in the current
        /// paragraph is extended to its widest possible length.
        /// Note: This setting only affects <i>kashidas</i>, which are special
        /// characters used to extend the joiner between two Arabic characters.
        /// </summary>
        HighKashida,

        /// <summary>
        /// Specifies that the text is aligned on the left
        /// text margin in the document.
        /// </summary>
        Left,

        /// <summary>
        /// Specifies that the kashida length for text in the current
        /// paragraph is extended to a slightly longer length.
        /// Note: This setting only affects <i>kashidas</i>, which are special
        /// characters used to extend the joiner between two Arabic characters.
        /// </summary>
        LowKashida,

        /// <summary>
        /// Specifies that the kashida length for text in the current
        /// paragraph is extended to a medium length determined
        /// by the consumer.
        /// Note: This setting only affects <i>kashidas</i>, which are special
        /// characters used to extend the joiner between two Arabic characters.
        /// </summary>
        MediumKashida,

        /// <summary>
        /// Specifies that the text is aligned to the list tab,
        /// which is the tab stop after the numbering for the current
        /// paragraph.
        /// </summary>
        NumTab,

        /// <summary>
        /// Specifies that the text is aligned on the right
        /// text margin in the document.
        /// </summary>
        Right,

        /// <summary>
        /// Specifies that the text is justified with an
        /// optimization for Thai.
        /// Note: This type of justification affects both the interword
        /// spacing on each line and the inter-character spacing between each
        /// word when justifying its contents, unlike the 'Both' setting.
        /// This difference is created in that the inter-character space
        /// is increased slightly in order to ensure that the additional space
        /// created by the justification is reduced.
        /// </summary>
        ThaiDistribute,
    }
    #endregion ParagraphAlignment enumeration

    #region UnitOfMeasurement
    /// <summary>
    /// Constants which specify the implied unit of measurement
    /// for properties which represent a graphical length.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// The default unit of measurement is a desktop publishing point, which
    /// is equal to 1/72 of an inch.
    /// </p>
    /// </remarks>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum UnitOfMeasurement
    {
        /// <summary>
        /// Units are expressed as DTPs (desktop publishing points), a unit
        /// equal to 1/72 of an inch. One point is equal to 20 twips.
        /// </summary>
        Point,

        /// <summary>
        /// Units are expressed as one-twentieths of a point, or 1/1440 of an inch.
        /// </summary>
        Twip,

        /// <summary>
        /// Units are expressed as inches (U.S. customary units).
        /// One inch is equal to 1,440 twips.
        /// </summary>
        Inch,

        /// <summary>
        /// Units are expressed as centimeters. One centimeter is equal to
        /// approximately 567 twips.
        /// </summary>
        Centimeter,
    }
    #endregion UnitOfMeasurement

    #region Underline enumeration
    //
    //  2.18.107 ST_Underline (Underline Patterns)
    //  OpenXML pg. 1844
    //
    /// <summary>
    /// Constants which define the manner in which text is underlined.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum Underline
    {
        /// <summary>
	    /// The actual value is determined at a higher level of the
        /// property resolution hierarchy.
	    /// </summary>
	    Default,

        /// <summary>
	    /// No underline.
	    /// </summary>
	    None,

	    /// <summary>
	    /// Specifies an underline consisting of a dashed line
        /// beneath all characters.
	    /// </summary>
	    Dash,

	    /// <summary>
	    /// Specifies an underline consisting of a series of thick
        /// dash, dot, dot characters beneath all characters.
	    /// </summary>
	    DashDotDotHeavy,

        /// <summary>
        /// Specifies an underline consisting of a series of thick
        /// dash, dot characters beneath all characters.
        /// </summary>
        DashDotHeavy,

        /// <summary>
        /// Specifies an underline consisting of a series of thick
        /// dashes beneath all characters in this run.
        /// </summary>
        DashedHeavy,

        /// <summary>
        /// Specifies an underline consisting of long dashed
        /// characters beneath all characters.
        /// </summary>
        DashLong,

        /// <summary>
        /// Specifies an underline consisting of thick long dashed
        /// characters beneath all characters.
        /// </summary>
        DashLongHeavy,

        /// <summary>
        /// Specifies an underline consisting of a series of dash,
        /// dot characters beneath all characters.
        /// </summary>
        DotDash,

        /// <summary>
        /// Specifies an underline consisting of a series of dash,
        /// dot, dot characters beneath all characters.
        /// </summary>
        DotDotDash,

        /// <summary>
        /// Specifies an underline consisting of a series of dot
        /// characters beneath all characters.
        /// </summary>
        Dotted,

        /// <summary>
        /// Specifies an underline consisting of a series of thick dot
        /// characters beneath all characters.
        /// </summary>
        DottedHeavy,

        /// <summary>
        /// Specifies an underline consisting of two lines beneath
        /// all characters.
        /// </summary>
        Double,

        /// <summary>
        /// Specifies an underline consisting of a single line beneath
        /// all characters.
        /// </summary>
        Single,

        /// <summary>
        /// Specifies an underline consisting of a single thick line
        /// beneath all characters.
        /// </summary>
        Thick,

        /// <summary>
        /// Specifies an underline consisting of a single wavy line
        /// beneath all characters.
        /// </summary>
        Wave,

        /// <summary>
        /// Specifies an underline consisting of a pair of wavy lines
        /// beneath all characters.
        /// </summary>
        WavyDouble,

        /// <summary>
        /// Specifies an underline consisting of a single thick wavy line
        /// beneath all characters.
        /// </summary>
        WavyHeavy,

        /// <summary>
        /// Specifies an underline consisting of a single line
        /// beneath all non-space characters in the run. No underline
        /// appears beneath any space character (breaking or non-breaking).
        /// </summary>
        Words,
    }
    #endregion Underline enumeration

    #region Strikethrough enumeration
    /// <summary>
    /// Constants which define the manner in which a line
    /// is drawn through the text.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum StrikeThrough
    {
        /// <summary>
	    /// The actual value is determined at a higher level of the
        /// property resolution hierarchy.
	    /// </summary>
	    Default,

        /// <summary>
        /// A single line is drawn through the text.
        /// </summary>
        SingleOn,

        /// <summary>
        /// The 'SingleOn' setting is explicitly disabled for the associated run.
        /// Use this setting to clear the single strikethough effect for this run
        /// when the setting is enabled at a higher level of the
        /// property resolution hierarchy.
        /// </summary>
        SingleOff,

        /// <summary>
        /// A double line is drawn through the text.
        /// </summary>
        DoubleOn,

        /// <summary>
        /// The 'DoubleOn' setting is explicitly disabled for the associated run.
        /// Use this setting to clear the double strikethough effect for this run
        /// when the setting is enabled at a higher level of the
        /// property resolution hierarchy.
        /// </summary>
        DoubleOff,

    }
    #endregion Strikethrough enumeration

    #region Capitalization enumeration
    /// <summary>
    /// Constants which define the manner in which characters
    /// are capitalized.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum Capitalization
    {
        /// <summary>
	    /// The actual value is determined at a higher level of the
        /// property resolution hierarchy.
	    /// </summary>
	    Default,

        /// <summary>
        /// Letters are capitalized.
        /// </summary>
        CapsOn,

        /// <summary>
        /// The 'CapsOn' setting is explicitly disabled for the associated run.
        /// Use this setting to clear the captitalization for this run when the
        /// 'CapsOn' setting is enabled at a higher level of the property resolution hierarchy.
        /// </summary>
        CapsOff,

        /// <summary>
        /// Lower-case characters in this text run are formatted for display as their
        /// upper-case character equivalents in a font size two points smaller
        /// than the resolved font size for the associated run.
        /// </summary>
        SmallCapsOn,

        /// <summary>
        /// The 'SmallCapsOn' setting is explicitly disabled for the associated run.
        /// Use this setting to clear the captitalization for this run when the
        /// 'SmallCapsOn' setting is enabled at a higher level of the property resolution hierarchy.
        /// </summary>
        SmallCapsOff,
    }
    #endregion Capitalization enumeration

    #region FontVerticalAlignment enumeration
    /// <summary>
    /// Constants which define the manner in which characters
    /// are vertically aligned, i.e., when the characters appear
    /// in subscript or superscript.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum FontVerticalAlignment
    {
        /// <summary>
	    /// The actual value is determined at a higher level of the
        /// property resolution hierarchy.
	    /// </summary>
	    Default,

        /// <summary>
        /// Normal; the text is rendered at its normal location
        /// as relative to the baseline for the associated run.
        /// Use this setting to clear the subscript or superscript
        /// effect for this run when either setting is enabled at a
        /// higher level of the property resolution hierarchy.
        /// </summary>
        Baseline,

        /// <summary>
        /// Text is rendered in a smaller size below the default
        /// baseline location for the associated run.
        /// </summary>
        Subscript,

        /// <summary>
        /// Text is rendered in a smaller size above the default
        /// baseline location for the associated run.
        /// </summary>
        Superscript,
    }
    #endregion FontVerticalAlignment enumeration

    #region FontTextEffect
    /// <summary>
    /// Constants which define additional special effects
    /// for the font, i.e., engraved, embossed, outlined,
    /// or shadowed.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum FontTextEffect
    {
        /// <summary>
	    /// The actual value is determined at a higher level of the
        /// property resolution hierarchy.
	    /// </summary>
	    Default,

        /// <summary>
        /// The text appears as if it is raised off the page in relief.
        /// </summary>
        EmbossingOn,

        /// <summary>
        /// The 'EmbossingOn' setting is explicitly disabled for the associated run.
        /// Use this setting to clear the embossing effect for this run when the
        /// setting is enabled at a higher level of the property resolution hierarchy.
        /// </summary>
        EmbossingOff,

        /// <summary>
        /// The text appears as if it is imprinted or pressed into the page.
        /// </summary>
        EngravingOn,

        /// <summary>
        /// The 'EngravingOn' setting is explicitly disabled for the associated run.
        /// Use this setting to clear the engraving effect for this run when the
        /// setting is enabled at a higher level of the property resolution hierarchy.
        /// </summary>
        EngravingOff,

        /// <summary>
        /// A border is drawn around the inner and outer borders of
        /// each character glyph in the run.
        /// </summary>
        OutliningOn,

        /// <summary>
        /// The 'OutliningOn' setting is explicitly disabled for the associated run.
        /// Use this setting to clear the outlining effect for this run when the
        /// setting is enabled at a higher level of the property resolution hierarchy.
        /// </summary>
        OutliningOff,
    }
    #endregion FontTextEffect

    #region AnchorRelativeHorizontalPosition enumeration
    //
    //  5.5.3.4 ST_RelFromH (Horizontal Relative Positioning)
    //  OpenXML pg. 3949
    //
    /// <summary>
    /// Constants which describe a
    /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture's</see>
    /// relative horizontal position.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum AnchorRelativeHorizontalPosition
    {
        /// <summary>
        /// Specifies that the horizontal positioning is
        /// relative to the position of the anchor within its run
        /// content.
        /// </summary>
        Character,

        /// <summary>
        /// Specifies that the horizontal positioning is
        /// relative to the extents of the column which contains its
        /// anchor.
        /// </summary>
        Column,

        /// <summary>
        /// Specifies that the horizontal positioning is
        /// relative to the inside margin of the current page (the
        /// left margin on odd pages, right on even pages).
        /// </summary>
        InsideMargin,

        /// <summary>
        /// Specifies that the horizontal positioning is
        /// relative to the left margin of the page.
        /// </summary>
        LeftMargin,

        /// <summary>
        /// Specifies that the horizontal positioning is
        /// relative to the page margins.
        /// </summary>
        Margin,

        /// <summary>
        /// Specifies that the horizontal positioning is
        /// relative to the outside margin of the current page (the
        /// right margin on odd pages, left on even pages).
        /// </summary>
        OutsideMargin,

        /// <summary>
        /// Specifies that the horizontal positioning is
        /// relative to the edge of the page.
        /// </summary>
        Page,

        /// <summary>
        /// Specifies that the horizontal positioning is
        /// relative to the right margin of the page.
        /// </summary>
        RightMargin,
    }
    #endregion AnchorRelativeHorizontalPosition enumeration

    #region AnchorRelativeVerticalPosition enumeration
    //
    //  5.5.3.5 ST_RelFromV (Vertical Relative Positioning)
    //  OpenXML pg. 3951
    //
    /// <summary>
    /// Constants which describe a
    /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture's</see>
    /// relative vertical position.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum AnchorRelativeVerticalPosition
    {
        /// <summary>
        /// Specifies that the vertical positioning is relative
        /// to the bottom margin of the current page.
        /// </summary>
        BottomMargin,

        /// <summary>
        /// Specifies that the vertical positioning is relative
        /// to the inside margin of the current page.
        /// </summary>
        InsideMargin,

        /// <summary>
        /// Specifies that the vertical positioning is relative
        /// to the line containing the anchor character.
        /// </summary>
        Line,

        /// <summary>
        /// Specifies that the vertical positioning is relative
        /// to the page margins.
        /// </summary>
        Margin,

        /// <summary>
        /// Specifies that the vertical positioning is relative
        /// to the outside margin of the current page.
        /// </summary>
        OutsideMargin,

        /// <summary>
        /// Specifies that the vertical positioning is relative
        /// to the edge of the page.
        /// </summary>
        Page,

        /// <summary>
        /// Specifies that the vertical positioning is relative
        /// to the paragraph which contains the drawing anchor.
        /// Note: This value is only supported when an offset is
        /// explicitly defined.
        /// </summary>
        Paragraph,

        /// <summary>
        /// Specifies that the vertical positioning is relative
        /// to the top margin of the current page.
        /// </summary>
        TopMargin,
    }
    #endregion AnchorRelativeVerticalPosition enumeration

    #region AnchorHorizontalAlignment enumeration
    //
    //  5.5.3.1 ST_AlignH (Relative Horizontal Alignment Positions)
    //  OpenXML pg. 3947
    //
    /// <summary>
    /// Constants which describe a
    /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture's</see>
    /// horizontal alignment.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum AnchorHorizontalAlignment
    {
        /// <summary>
        /// No horizontal alignment; the picture's position is specified as an absolute value.
        /// </summary>
        None,

        /// <summary>
        /// Specifies that the picture is centered as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeHorizontalPosition">RelativeHorizontalPosition</see>.
        /// </summary>
        Center,

        /// <summary>
        /// Specifies that the picture is inside as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeHorizontalPosition">RelativeHorizontalPosition</see>.
        /// </summary>
        Inside,

        /// <summary>
        /// Specifies that the picture is left-aligned as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeHorizontalPosition">RelativeHorizontalPosition</see>.
        /// </summary>
        Left,

        /// <summary>
        /// Specifies that the picture is outside as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeHorizontalPosition">RelativeHorizontalPosition</see>.
        /// </summary>
        Outside,

        /// <summary>
        /// Specifies that the picture is right-aligned as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeHorizontalPosition">RelativeHorizontalPosition</see>.
        /// </summary>
        Right,
    }
    #endregion AnchorHorizontalAlignment enumeration

    #region AnchorVerticalAlignment enumeration
    //
    //  5.5.3.2 ST_AlignV (Vertical Alignment Definition)
    //  OpenXML pg. 3948
    //
    /// <summary>
    /// Constants which describe a
    /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture's</see>
    /// vertical alignment.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum AnchorVerticalAlignment
    {
        /// <summary>
        /// No vertical alignment; the object's position is specified as an absolute value.
        /// </summary>
        None,

        /// <summary>
        /// Specifies that the object is bottom-aligned as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeVerticalPosition">RelativeVerticalPosition</see>.
        /// </summary>
        Bottom,

        /// <summary>
        /// Specifies that the object is centered as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeVerticalPosition">RelativeVerticalPosition</see>.
        /// </summary>
        Center,

        /// <summary>
        /// Specifies that the object is inside as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeVerticalPosition">RelativeVerticalPosition</see>.
        /// </summary>
        Inside,

        /// <summary>
        /// Specifies that the object is outside as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeVerticalPosition">RelativeVerticalPosition</see>.
        /// </summary>
        Outside,

        /// <summary>
        /// Specifies that the object is at the top as relative to the
        /// <see cref="Infragistics.Documents.Word.AnchorRelativeVerticalPosition">RelativeVerticalPosition</see>.
        /// </summary>
        Top,
    }
    #endregion AnchorVerticalAlignment enumeration

    #region TableCellVerticalAlignment enumeration
    //
    //  2.18.111 ST_VerticalJc (Vertical Alignment Type)
    //  OpenXML pg. 1851
    //
    /// <summary>
    /// Constants which describe the vertical alignment for a table cell.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum TableCellVerticalAlignment
    {
        /// <summary>
        /// The actual value is determined at a higher level of the property resolution hierarchy.
        /// </summary>
        Default,

        /// <summary>
        /// Specifies that the text is vertically aligned with the bottom of the cell.
        /// </summary>
        Bottom,

        /// <summary>
        /// Specifies that the text is vertically aligned with the center of the cell.
        /// </summary>
        Center,

        /// <summary>
        /// Specifies that the text is vertically aligned with the top of the cell.
        /// </summary>
        Top,
    }
    #endregion TableCellVerticalAlignment enumeration

    #region AnchorTextWrapping enumeration
    //
    //  5.5.2.17 wrapSquare (Square Wrapping) - OpenXml pg. 3935
    //  5.5.2.18 wrapThrough (Through Wrapping) - OpenXml pg. 3938
    //  5.5.2.20 wrapTopAndBottom (Top and Bottom Wrapping) - OpenXml pg. 3938
    //
    //  
    //  
    //  Note that 'Tight' is currently not supported, since we don't support
    //  wrap points and that setting would is only meaningful when those are defined.
    //
    /// <summary>
    /// Constants which define the layout for text included in the same
    /// paragraph as an
    /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture</see>.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum AnchorTextWrapping
    {
	    /// <summary>
	    /// Text flows around all sides of the picture.
	    /// </summary>
	    Square,

	    /// <summary>
	    /// Text does not wrap, and appears "behind" the picture.
	    /// </summary>
	    TextInBackground,

	    /// <summary>
	    /// Text does not wrap, and appears superimposed over the picture.
	    /// </summary>
	    TextInForeground,

	    /// <summary>
	    /// Text flows along the top and bottom of the picture.
	    /// </summary>
	    TopAndBottom,
    }
    #endregion AnchorTextWrapping enumeration

    #region AnchorTextWrappingSide enumeration
    //
    //  5.5.3.7 ST_WrapText (Text Wrapping Location)
    //  OpenXML pg. 3953
    //
    /// <summary>
    /// Constants which define the sides of an
    /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture</see>
    /// around which adjacent text can be wrapped.
    /// </summary>
    /// <seealso cref="Infragistics.Documents.Word.AnchorTextWrapping">TextWrapping enumeration</seealso>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum AnchorTextWrappingSide
    {
	    /// <summary>
	    /// Text wraps around the left and right sides of the picture.
	    /// </summary>
	    Both,

	    /// <summary>
	    /// Text wraps around the left side of the picture.
	    /// </summary>
	    Left,

	    /// <summary>
	    /// Text wraps around the right side of the picture.
	    /// </summary>
	    Right,

	    /// <summary>
	    /// Text wraps around the side of the picture that is farthest
        /// from the page margin.
	    /// </summary>
	    Largest,

    }
    #endregion AnchorTextWrappingSide enumeration

    #region TableCellVerticalMerge
    /// <summary>
    /// Constants which describe the manner in which a TableCell
    /// is vertically merged with adjacent cells.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum TableCellVerticalMerge
    {
        /// <summary>
        /// No vertical merging. The cell appears to span only the row
        /// to which it belongs.
        /// </summary>
        None,

        /// <summary>
        /// Begins a vertical merging run. If the associated cell follows one
        /// which belongs to a vertical merging run, that run is discontinued.
        /// If no cells 'above' the associated cell belong to a vertical merging
        /// run, a new run begins.
        /// </summary>
        Start,

        /// <summary>
        /// Continues a previously defined vertical merging run.
        /// If no cells 'above' the associated cell belong to a vertical merging
        /// run, this setting has no effect.
        /// </summary>
        Continue,
    }
    #endregion TableCellVerticalMerge

    #region RowHeightRule enumeration
    /// <summary>
    /// Constants which describe the rule applied when calculating the height of a table row.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum RowHeightRule
    {
        /// <summary>
        /// Specifies that the height of the row is automatically
        /// determined by the size of its contents, with no predetermined minimum
        /// or maximum size.
        /// </summary>
        Auto,

        /// <summary>
        /// Specifies that the height of the row is at least
        /// the value specified, but may be expanded to fit its content as needed.
        /// </summary>
        AtLeast,

        /// <summary>
        /// Specifies that the height of the row is exactly the value
        /// specified, regardless of the size of its contents. If the
        /// content is too large for the specified height, it is clipped.
        /// </summary>
        Exact,
    }
    #endregion RowHeightRule enumeration

    #region TableBorderStyle enumeration
    /// <summary>
    /// Constants which describe the style of the borders displayed
    /// around tables and cells.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum TableBorderStyle
    {
        /// <summary>
        /// No borders are displayed.
        /// </summary>
        None,

        /// <summary>
        /// A single-line border is displayed.
        /// </summary>
        Single,

        /// <summary>
        /// A dashed-line border is displayed.
        /// </summary>
        Dashed,

        /// <summary>
        /// A dotted-line border is displayed.
        /// </summary>
        Dotted,

        /// <summary>
        /// A double-line border is displayed.
        /// </summary>
        Double,

        /// <summary>
        /// A inset-line border is displayed.
        /// </summary>
        Inset,
    }
    #endregion TableBorderStyle enumeration

    #region PictureOutlineStyle enumeration

    /// <summary>
    /// Constants which describe the style of the outlines displayed
    /// around anchored and inline pictures.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum PictureOutlineStyle
    {
        /// <summary>
        /// No outline is displayed around the picture.
        /// </summary>
        None,

        /// <summary>
        /// The picture is outlined with a single line.
        /// </summary>
        Single,

        /// <summary>
        /// The picture is outlined with double lines of equal width.
        /// </summary>
        Double,

        /// <summary>
        /// The picture is outlined with double lines, one thin and one thick.
        /// </summary>
        ThinThick,

        /// <summary>
        /// The picture is outlined with double lines, one thick and one thin.
        /// </summary>
        ThickThin,

        /// <summary>
        /// The picture is outlined with three lines - thin, thick, and thin.
        /// </summary>
        Triple,
    }

    #endregion PictureOutlineStyle enumeration

    #region PictureOutlineCornerStyle

    /// <summary>
    /// Constants which describe the manner in which the corners
    /// of a picture outline are joined.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum PictureOutlineCornerStyle
    {
        /// <summary>
        /// The corners of the outline are mitered.
        /// </summary>
        Miter,

        /// <summary>
        /// The corners of the outline are rounded.
        /// </summary>
        Round,

        /// <summary>
        /// The corners of the outline are beveled.
        /// </summary>
        Bevel,
    }

    #endregion PictureOutlineCornerStyle

    #region TableCellTextDirection enumeration
    /// <summary>
    /// Constants which describe the direction of text flow for table cells.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum TableCellTextDirection
    {
        /// <summary>
        /// Text flows normally, from left to right along the x-axis.
        /// </summary>
        Normal,

        /// <summary>
        /// Specifies that text flows from bottom to top vertically,
        /// then from left to right horizontally on the page. Under this setting, vertical lines
        /// are filled before the text expands horizontally.
        /// </summary>
        BottomToTopLeftToRight,

        /// <summary>
        /// Specifies that text flows from left to right horizontally,
        /// then top to bottom vertically on the page. Under this setting, horizontal lines
        /// are filled before the text expands vertically.
        /// </summary>
        LeftToRightTopToBottom,

        /// <summary>
        /// Specifies that text flows from left to right horizontally,
        /// then top to bottom vertically on the page. Under this setting, horizontal lines
        /// are filled before the text expands vertically.
        /// This flow is also rotated such that any East Asian text shall be
        /// rotated 270 degrees when displayed on a page.
        /// </summary>
        LeftToRightTopToBottomRotated,

        /// <summary>
        /// Specifies that text flows from top to bottom vertically,
        /// then left to right horizontally on the page. Under this setting, vertical lines
        /// are filled before the text expands horizontally.
        /// This flow is also rotated such that all text is rotated 90
        /// degrees when displayed on a page.
        /// </summary>
        TopToBottomLeftToRightRotated,

        /// <summary>
        /// Specifies that text flows from right to left horizontally,
        /// then top to bottom vertically on the page. Under this setting,
        /// vertical lines are filled before the text expands horizontally.
        /// </summary>
        TopToBottomRightToLeft,

        /// <summary>
        /// Specifies that text flows from right to left horizontally,
        /// then top to bottom vertically on the page. Under this setting,
        /// vertical lines are filled before the text expands horizontal.
        /// This flow is also rotated such that all text is rotated 90
        /// degrees when displayed on a page.
        /// </summary>
        TopToBottomRightToLeftRotated,

    }
    #endregion TableCellTextDirection enumeration

    #region TableLayout enumeration
    /// <summary>
    /// Constants which define the possible types of layout algorithms
    /// which are applied when determining the size and position of
    /// table cells.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum TableLayout
    {
        /// <summary>
        /// The preferred widths of the table items are used to generate
        /// the final sizing of the table, but the contents of each cell
        /// determines the final column widths.
        /// </summary>
        Auto,

        /// <summary>
        /// The preferred widths of the table items are used to generate
        /// the final sizing of the table, but does not change that size,
        /// regardless of the contents of each cell.
        /// </summary>
        Fixed,
    }
    #endregion TableLayout enumeration

    #region PageOrientation enumeration
    /// <summary>
    /// Constants which define the orientation for a page in the document.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum PageOrientation
    {
        /// <summary>
        /// The actual value is determined by the page size.
        /// If the width of the page size is greater than the height, the orientation
        /// resolves to 'Landscape'; if the width is equal or less than the height,
        /// it resolves to 'Portrait'.
        /// </summary>
        Default,

        /// <summary>
        /// Pages in the associated section are printed in portrait mode, with no rotation.
        /// The page size is adjusted, if necessary, so that the height is greater than or
        /// equal to the width, by swapping the values as defined by the page size.
        /// </summary>
        Portrait,

        /// <summary>
        /// Pages in the associated section are printed at a 90 degree rotation
        /// with respect to the normal page orientation.
        /// The page size is adjusted, if necessary, so that the width is greater than or
        /// equal to the height, by swapping the values as defined by the page size.
        /// </summary>
        Landscape,
    }
    #endregion PageOrientation enumeration

    #region TableBorderSides enumeration
    /// <summary>
    /// Constants which define which border sides are drawn for cells and tables.
    /// </summary>
    [Flags()]

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum TableBorderSides
    {
        /// <summary>
        /// No borders are drawn for the cell or table. This setting is equivalent
        /// to setting the style of all borders to 'None'.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// The top border is drawn for the cell or table.
        /// </summary>
        Top = 0x0001,

        /// <summary>
        /// The left border is drawn for the cell or table.
        /// </summary>
        Left = 0x0002,

        /// <summary>
        /// The bottom border is drawn for the cell or table.
        /// </summary>
        Bottom = 0x0004,

        /// <summary>
        /// The right border is drawn for the cell or table.
        /// </summary>
        Right = 0x0008,

        /// <summary>
        /// The inner horizontal borders.
        /// </summary>        
        InsideH = 0x0010,

        /// <summary>
        /// The inner vertical borders.
        /// </summary>        
        InsideV = 0x0020,

        /// <summary>
        /// Combines the 'Left' and 'Right' settings.
        /// </summary>
        LeftAndRight = Left | Right,

        /// <summary>
        /// Combines the 'Top' and 'Bottom' settings.
        /// </summary>
        TopAndBottom = Top | Bottom,

        /// <summary>
        /// Combines the 'InsideH' and 'InsideV' settings.
        /// </summary>
        AllInner = InsideH | InsideV,

        /// <summary>
        /// Combines the 'Top', 'Left', 'Bottom', and 'Right' settings.
        /// </summary>
        AllOuter =  Top | Left | Bottom | Right,

        /// <summary>
        /// All borders are drawn. This is the default setting.
        /// </summary>
        All = AllInner | AllOuter,
    }
    #endregion TableBorderSides enumeration

    #region WordDocumentWriterState enumeration
    /// <summary>
    /// Bitflags which describe the current state of a
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
    /// </summary>
    [Flags()]

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum WordDocumentWriterState
    {
        /// <summary>
        /// No noteworthy state.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// The writer is currently between calls to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartDocument">StartDocument</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndDocument(bool)">EndDocument</see>
        /// methods.
        /// </summary>
        DocumentOpen = 0x0001,

        /// <summary>
        /// The writer is currently between calls to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndParagraph">EndParagraph</see>
        /// methods.
        /// </summary>
        ParagraphOpen = 0x0002,

        /// <summary>
        /// The writer is currently between calls to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartTable(int)">StartTable</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</see>
        /// methods.
        /// </summary>
        TableOpen = 0x0004,

        /// <summary>
        /// The writer is currently between calls to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartTableRow()">StartTableRow</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndTableRow">EndTableRow</see>
        /// methods.
        /// </summary>
        TableRowOpen = 0x0008,

        /// <summary>
        /// The writer is currently between calls to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartTableCell">StartTableCell</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndTableCell">EndTableCell</see>
        /// methods.
        /// </summary>
        TableCellOpen = 0x0010,
    }
    #endregion WordDocumentWriterState enumeration

    #region SectionHeaderFooterParts
    /// <summary>
    /// Constants which describe the header and footer parts that
    /// will be created in a document section when used by the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts)">AddSectionHeaderFooter</see>
    /// method.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// This enumeration is decorated with the System.Flags attribute, which means that
    /// the constants defined herein can be combined using the logical OR operator.
    /// For example, to specify that a document section should contain both a header
    /// and a footer, on all pages, the values 'HeaderAllPages' (0x0001) and
    /// 'FooterAllPages' (0x0002) would be combined in a bitwise manner to produce
    /// a numerical value of 0x0003, resulting in both a header and a footer on
    /// all pages in the section.
    /// </p>
    /// </remarks>
    [Flags()]

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum SectionHeaderFooterParts
    {
        /// <summary>
        /// No headers or footers should appear in the section.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// A header which appears on all pages of the section.
        /// </summary>
        HeaderAllPages = 0x0001,

        /// <summary>
        /// A footer which appears on all pages of the section.
        /// </summary>
        FooterAllPages = 0x0002,

        /// <summary>
        /// A header which appears only on the first page of the section.
        /// </summary>
        HeaderFirstPageOnly = 0x0004,

        /// <summary>
        /// A footer which appears only on the first page of the section.
        /// </summary>
        FooterFirstPageOnly = 0x0008,

        ///// <summary>
        ///// A header which appears only on even numbered pages of the section.
        ///// Note that numbering begins with the first page in the section, not
        ///// the document.
        ///// </summary>
        //HeaderEven = 0x0010,

        ///// <summary>
        ///// A footer which appears only on even numbered pages of the section.
        ///// Note that numbering begins with the first page in the section, not
        ///// the document.
        ///// </summary>
        //FooterEven = 0x0020,

        /// <summary>
        /// Combines the HeaderAllPages, FooterAllPages, HeaderFirstPageOnly, and FooterFirstPageOnly
        /// settings.
        /// </summary>
        All = HeaderAllPages | FooterAllPages | HeaderFirstPageOnly | FooterFirstPageOnly,
    }
    #endregion SectionHeaderFooterParts

    #region HeaderFooterType
    /// <summary>
    /// Specifies the possible types of headers and footers which
    /// may be specified for a given header or footer reference in
    /// a document section.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum HeaderFooterType
    {
        /// <summary>
        /// Specifies that this header/footer is to appear on
        /// every page in this section which is not overridden with
        /// a specific first page header/footer.
        /// </summary>
        AllPages,

        //  BF 1/20/11
        //
        //  This requires adding a settings.xml part, which until now
        //  we haven't needed...let's see if we really need this yet.
        //
        ///// <summary>
        ///// Specifies that this header/footer is to appear on all
        ///// even numbered pages in this section (counting from the first
        ///// page in the section).
        ///// </summary>
        //Even,

        /// <summary>
        /// Specifies that this header/footer is to appear on only
        /// the first page of the section.
        /// </summary>
        FirstPageOnly,
    }
    #endregion HeaderFooterType

    #region PageNumberFieldFormat
    /// <summary>
    /// Constants which define the format applied to the page numbers
    /// displayed for a header or footer.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum PageNumberFieldFormat
    {
        /// <summary>
        /// Pages are numbered using standard decimal representation.
        /// Example: "1", "2", "3"
        /// </summary>
        Decimal,
        
        /// <summary>
        /// Pages are numbered using standard decimal representation, with the
        /// numbers prefixed and sufixed by dashes.
        /// Example: "-1-", "-2-", "-3-"
        /// </summary>
        DecimalDash,
        
        /// <summary>
        /// Pages are numbered using standard decimal representation, with the
        /// numbers enclosed in a circle.
        /// </summary>
        DecimalCircle,
        
        /// <summary>
        /// Pages are numbered using standard decimal representation, with the
        /// numbers enclosed in parenthesis.
        /// Example: "(1)", "(2)", "(3)"
        /// </summary>
        DecimalParenthesis,

        /// <summary>
        /// Formats a numeric result as one or more occurrences of an
        /// uppercase alphabetic Latin character.
        /// Example: "A", "B", "C"
        /// </summary>
        LatinAlphabeticUppercase,
        
        /// <summary>
        /// Formats a numeric result as one or more occurrences of an
        /// lowercase alphabetic Latin character.
        /// </summary>
        /// Example: "a", "b", "c"
        LatinAlphabeticLowercase,
        
        /// <summary>
        /// Pages are numbered using ascending Abjad numerals.
        /// </summary>
        ArabicAbjad,
        
        /// <summary>
        /// Pages are numbered using characters in the Arabic alphabet.
        /// </summary>
        ArabicAlphabetic,

        /// <summary>
        /// Pages are numbered using ascending numbers from the Chinese counting system.
        /// </summary>
        Chinese,

        /// <summary>
        /// Pages are numbered using sequential numbers from the Chinese simplified legal format.
        /// </summary>
        ChineseLegalSimplified,

        /// <summary>
        /// Pages are numbered using sequential numbers from the Chinese counting thousand system.
        /// </summary>
        ChineseCountingThousand,

        /// <summary>
        /// Pages are numbered using Hebrew numerals.
        /// </summary>
        Hebrew,

        /// <summary>
        /// Pages are numbered using the Hebrew alphabet.
        /// </summary>
        HebrewAlphabetic,

        /// <summary>
        /// Pages are numbered using a Japanese style using sequential digital
        /// ideographs, using the appropriate character.
        /// </summary>
        JapaneseKanji,

        /// <summary>
        /// Pages are numbered using the Japanese counting system.
        /// </summary>
        JapaneseCounting,

        /// <summary>
        /// Pages are numbered using the Japanese iroha.
        /// </summary>
        JapaneseIroha,

        /// <summary>
        /// Pages are numbered using sequential numbers from the Japanese legal counting system.
        /// </summary>
        JapaneseLegal,

        /// <summary>
        /// Pages are numbered using sequential numbers from the Japanese digital ten thousand counting system.
        /// </summary>
        JapaneseDigitalTenThousand,

        /// <summary>
        /// Pages are numbered using sequential numbers from the Korean counting system.
        /// </summary>
        KoreanCounting,

        /// <summary>
        /// Pages are numbered using sequential numbers from the Korean Chosung format.
        /// </summary>
        KoreanChosung,

        /// <summary>
        /// Pages are numbered using sequential numbers from the Korean Ganada format.
        /// </summary>
        KoreanGanada,

        /// <summary>
        /// Pages are numbered using lowercase Roman numerals.
        /// </summary>
        RomanLowercase,

        /// <summary>
        /// Pages are numbered using uppercase Roman numerals.
        /// </summary>
        RomanUppercase,

        /// <summary>
        /// Pages are numbered using Thai numbers.
        /// </summary>
        Thai,

        /// <summary>
        /// Pages are numbered using sequential numbers from the Thai counting system.
        /// </summary>
        ThaiCounting,

        /// <summary>
        /// Pages are numbered using Thai letters.
        /// </summary>
        ThaiLetter,

        /// <summary>
        /// Pages are numbered using lowercase ordinal representation of arabic numerals.
        /// Example: "1st", "2nd", "3rd"
        /// </summary>
        Ordinal,

        /// <summary>
        /// Pages are numbered using lowercase ordinal text.
        /// Example: "first", "second", "third"
        /// </summary>
        TextOrdinal,

        /// <summary>
        /// Pages are numbered using lowercase cardinal text.
        /// Example: "one", "two", "three"
        /// </summary>
        TextCardinal,
    }
    #endregion PageNumberFieldFormat

    #region ShapeType
    /// <summary>
    /// Constants which define the types of shapes that can appear within a document.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// The
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
    /// currently only supports shape rendering using Vector Markup Language (VML); the reason
    /// this rendering mode is used is that it is supported in both MS Word 2007 and MS Word 2010.
    /// </p>
    /// <p class="body">
    /// The following table lists the classnames of the VML shapes associated with
    /// each constant:
    /// </p>
    /// <p class="body">
	/// <table border="1" cellpadding="3" width="100%" class="FilteredItemListTable">
	/// <thead>
	/// <tr>
	/// <th>ShapeType</th>
	/// <th>VML Shape</th>
	/// </tr>
	/// </thead>
	/// <tbody>
	/// <tr><td class="body">Line</td><td class="body"><see cref="Infragistics.Documents.Word.VmlLine">VmlLine</see></td></tr>
	/// <tr><td class="body">Rectangle</td><td class="body"><see cref="Infragistics.Documents.Word.VmlRectangle">VmlRectangle</see></td></tr>
	/// <tr><td class="body">Ellipse</td><td class="body"><see cref="Infragistics.Documents.Word.VmlEllipse">VmlEllipse</see></td></tr>
	/// <tr><td class="body">IsosceleseTriangle</td><td class="body"><see cref="Infragistics.Documents.Word.VmlIsosceleseTriangle">VmlIsosceleseTriangle</see></td></tr>
	/// <tr><td class="body">RightTriangle</td><td class="body"><see cref="Infragistics.Documents.Word.VmlRightTriangle">VmlRightTriangle</see></td></tr>
	/// </tbody>
	/// </table>
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum ShapeType
    {
        /// <summary>
        /// A straight
        /// <see cref="Infragistics.Documents.Word.VmlLine">line</see>.
        /// The origin of the line is implied to be
        /// at the top left corner of the rectangle defined by its
        /// position within the document and its
        /// <see cref="Infragistics.Documents.Word.Shape.Size">size</see>,
        /// and its endpoint is implied to be the bottom right corner of that rectangle.
        /// For lines that run parallel to the x or y
        /// axis, the width or height component of the Size property
        /// is set to zero (pure horizontal lines have a height of zero,
        /// pure vertical lines have a width of zero).
        /// A line can be inverted about the x axis using the
        /// <see cref="Infragistics.Documents.Word.VmlLine.InvertY">InvertY</see>
        /// property. This is used to render a line with a positive slope; since the origin
        /// is implied to be the top left corner of the bounding rect, the origin must be
        /// transformed to the bottom left corner.
        /// </summary>
        Line,        

        /// <summary>
        /// A
        /// <see cref="Infragistics.Documents.Word.VmlRectangle">rectangle</see>.
        /// The bounds of the rectangle are defined by its
        /// position within the document and its
        /// <see cref="Infragistics.Documents.Word.Shape.Size">size</see>.
        /// </summary>
        Rectangle,        

        /// <summary>
        /// An
        /// <see cref="Infragistics.Documents.Word.VmlEllipse">ellipse</see>.
        /// The bounds of the enclosing rectangle are defined by its
        /// position within the document and its
        /// <see cref="Infragistics.Documents.Word.Shape.Size">size</see>.
        /// </summary>
        Ellipse,        

        /// <summary>
        /// A
        /// <see cref="Infragistics.Documents.Word.VmlIsosceleseTriangle">triangle</see>
        /// whose apex is at the center of the rectangle which encloses it.
        /// The bounds of the enclosing rectangle are defined by its
        /// position within the document and its
        /// <see cref="Infragistics.Documents.Word.Shape.Size">size</see>.
        /// </summary>
        IsosceleseTriangle,        

        /// <summary>
        /// A
        /// <see cref="Infragistics.Documents.Word.VmlRightTriangle">triangle</see>
        /// whose apex is at the left edge of the rectangle which encloses it.
        /// The bounds of the enclosing rectangle are defined by its
        /// position within the document and its
        /// <see cref="Infragistics.Documents.Word.Shape.Size">size</see>.
        /// </summary>
        RightTriangle,        
    }
    #endregion ShapeType

    #region NewLineType
    /// <summary>
    /// Constants which specify the way a newline within a
    /// paragraph is represented in the generated output.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// A document consumer may interpret the behavior of line breaking
    /// characters differently depending on the characters used to represent
    /// them. For example, MS Word interprets a carriage return differently
    /// depending on whether the content is enclosed within a table cell.
    /// Paragraph justification also behaves differently depending on whether
    /// a line break or carriage return is used.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum NewLineType
    {
        /// <summary>
        /// The newline is represented as a line break.
        /// For the WordprocessingML format, the '&lt;br&gt;' tag is used to represent the newline in the generated XML.
        /// </summary>
        LineBreak,

        /// <summary>
        /// The newline is represented as a carriage return.
        /// For the WordprocessingML format, the '&lt;cr&gt;' tag is used to represent the newline in the generated XML.
        /// </summary>
        CarriageReturn,
    }
    #endregion NewLineType

    #region ShapeLineStyle
    /// <summary>
    /// Constants which determine the style applied to the line for a shape.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum ShapeLineStyle
    {
        /// <summary>
        /// A solid, unbroken line.
        /// </summary>
        Solid,

        /// <summary>
        /// A line with short dashes.
        /// </summary>
        ShortDash,

        /// <summary>
        /// A line with short dots.
        /// </summary>
        ShortDot,

        /// <summary>
        /// A line with a short dash and a dot.
        /// </summary>
        ShortDashDot,

        /// <summary>
        /// A line with a short dash followed by two dots.
        /// </summary>
        ShortDashDotDot,

        /// <summary>
        /// A dotted line.
        /// </summary>
        Dot,

        /// <summary>
        /// A dashed line.
        /// </summary>
        Dash,

        /// <summary>
        /// A line with long dashes.
        /// </summary>
        LongDash,

        /// <summary>
        /// A line with alternating dashes and dots.
        /// </summary>
        DashDot,

        /// <summary>
        /// A line with alternating long dashes and dots.
        /// </summary>
        LongDashDot,

        /// <summary>
        /// A line with a long dash followed by two dots.
        /// </summary>
        LongDashDotDot,
    }
    #endregion ShapeLineStyle
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