using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Infragistics.Windows.Editors
{

	#region State Enum

	internal enum State
	{
		Initial,
		LiteralEscape,
		Date,
		Time,
		Number,
		FractionPart
	}

	#endregion // State Enum

	#region MaskParser Class






	internal class MaskParser
	{
		// {LOC} prefixed masks are localized where we will translate certain characters to 
		// underlying locale characters.
		//
		internal static string LOCALIZED_ESCAPE_SEQUENCE = "{LOC}";

		private State state = State.Initial;

		private SectionsCollection sections;

		private IFormatProvider formatProvider;

		// This flag is set when we encounter certain masks that specify that numeric input will
		// be continous across integer and fraction part. That is the caret will start at the 
		// right of the fraction and digits input there will spill over to number section once
		// the fraction part is finished. This mask is spefici using the 'c' symbol in masks like
		// the {currency:8.2:c}.
		//
		private bool? hasContinuousNumericMask = null;

        // SSP 5/6/10 TFS31789
        // Take into account escaped characters.
        // 
        internal const char ESCAPE_CHAR = '\\';
        internal const string ESCAPE_CHAR_STRING = "\\";






		private MaskParser( IFormatProvider formatProvider )
		{			
			this.formatProvider = formatProvider;
		}

		private IFormatProvider FormatProvider
		{
			get
			{
				return this.formatProvider;
			}
		}

		private char[] DateSeperators
		{
			get
			{
				char c = XamMaskedEditor.GetCultureChar( '/', this.FormatProvider );
				
				// SSP 8/16/10 TFS32627
				// Also allow for '.' as a date separator.
				
				
				// 
				//string seps = "-/";
				string seps = "-/.";

				if ( 0 != c )
					seps = c + seps;

				return seps.ToCharArray();
			}
		}

		private char[] TimeSeperators
		{
			get
			{
				char c = XamMaskedEditor.GetCultureChar( ':', this.FormatProvider );

				// SSP 8/16/10 TFS32627
				// Also allow for '.' as a date separator.
				
				
				
				
				string seps = ":.";

				if ( 0 != c )
					seps = c + seps;

				return seps.ToCharArray();
			}
		}

		private char CultureDecimalSeperator
		{
			get
			{
				return XamMaskedEditor.GetCultureChar( '.', this.FormatProvider );
			}
		}

		private SectionsCollection Sections
		{
			get
			{
				if ( null == this.sections )
					this.sections = new SectionsCollection( );

				return this.sections;
			}
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private string SafeSubstring( string str, int startIndex, int length )
		{			
			if ( startIndex < 0 )
				startIndex = 0;

			if ( startIndex >= str.Length )
				return string.Empty;

			if ( length <= 0 )
				return string.Empty;

			if ( startIndex + length - 1 >= str.Length )			
				length = str.Length - startIndex;

			if ( length <= 0 )
				return string.Empty;
			
			return str.Substring( startIndex, length );			
		}




#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private int SafeIndexOfAny( string str, char[] array, int startIndex, int count )
		{			
			if ( startIndex < 0 )
				startIndex = 0;

			if ( startIndex >= str.Length )
				return -1;

			if ( 1 + startIndex + count >= str.Length )			
				count = str.Length - startIndex;

			if ( count <= 0 )
				return -1;
			
			return str.IndexOfAny( array, startIndex, count );			
		}

		// JAS 12/14/04 Japanese DateTime Separators Implementation
		#region GetOffset



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		private int GetOffset( string mask, string currentSection, string targetSection, int index, bool moveForward )
		{
			int offset = moveForward ? currentSection.Length : targetSection.Length;

			if( moveForward )
			{
				if( index + offset >= mask.Length )
					return 0;

                
                
                
                
                if ( mask[index + offset] == ESCAPE_CHAR )
					offset += 2;
				else
					offset += 1;
			}
			else
			{
				if( index - 2 < 0 )
					return 0;

                
                
                
                
                if ( mask[index - 2] == ESCAPE_CHAR )
					offset += 2;
				else
					offset += 1;
			}

			return offset;
		}

		#endregion // GetOffset

		#region AdjustNonstandardForeignDateMask

		/// <summary>
		/// If the current culture requires a non-standard mask for dates (such as in Japan) then
		/// this method inserts a '/' character after the last date section in the mask.  This
		/// allows the analyze method to successfully insert the special postfix character after that
		/// last date section.
		/// </summary>
		/// <param name="mask">A reference to the mask to be analyzed and modified, if appropriate.</param>
		private void AdjustNonstandardForeignDateMask( ref string mask )
		{
			string[] dateStrings = 
					{
						"dd/mm/yyyy",
						"dd/yyyy/mm",
						"mm/dd/yyyy",
						"mm/yyyy/dd",
						"yyyy/mm/dd",
						"yyyy/dd/mm",

						"dd/mm/yy",
						"dd/yy/mm",
						"mm/dd/yy",
						"mm/yy/dd",
						"yy/mm/dd",
						"yy/dd/mm",
					
						"dd/mm",
						"dd/yyyy",
						"mm/dd",
						"mm/yyyy",
						"yyyy/dd",
						"yyyy/mm",

						"dd/mm",
						"dd/yy",
						"mm/dd",
						"mm/yy",
						"yy/dd",
						"yy/mm"
					};

			int idxDateString = -1;
			foreach( string dateString in dateStrings )
			{
				idxDateString = mask.IndexOf( dateString );
				if( -1 < idxDateString )
				{
					int idxTarget = idxDateString + dateString.Length;

					if( idxTarget < mask.Length )
						mask = mask.Insert( idxTarget, "/" );
					else
						mask += "/";

					break;
				}
			}
		}

		internal int GetNumberOfDigitsRequired( double val )
		{
			if ( val < 0 )
				val = -val;

			double log = Math.Log10( val );
			return 1 + (int)Math.Floor( log );
		}

		#endregion // AdjustNonstandardForeignDateMask

        #region IndexOf

        // SSP 5/6/10 TFS31789
        // 
        private static int IndexOf( string str, int startIndex, int endIndex, char c, bool skipEscapedChars, char escapeChar )
        {
            for ( int i = startIndex; i <= endIndex; i++ )
            {
                char ii = str[i];

                if ( skipEscapedChars && escapeChar == ii )
                {
                    i++;
                }
                else if ( ii == c )
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion // IndexOf







		private void Analyze( string mask )
		{
			// SSP 4/9/05 BR03077
			// Moved the call to PreProcessMask from the static Parse method to here
			// because we made PreProcessMask non-static.
			//
			// SSP 1/18/05 - {date}, {time} and {longtime} mask tokens
			// Replace above tokens with the actual mask.
			//
			this.PreProcessMask( ref mask );

			int index = 0;
			SectionBase currentSection = null;
			FilterType filter = FilterType.Unchanged;

			State prevState = state;


			// SSP 1/11/02
			// Now since we are using a CultureInfo structure to get the
			// time and date seperators, instead of having to call 
			// TimeSeperators and DateSeperators every time through the 
			// below loop, just store it in an array and use that reducing
			// the overhead.
			//
			char[] timeSeperators = this.TimeSeperators;
			char[] dateSeperators = this.DateSeperators;

			// SSP 12/18/02 UWE342
			// Allow for localized masks. Added {LOC} prefix to the mask specification. When a mask is
			// preceded by {LOC} string, the certain characters in the mask will be localized. Escaped
			// characters will still not be localized.
			//
			// ---------------------------------------------------------------------------------------
			bool isLocalized = false;

			if ( mask.StartsWith( XamMaskedEditor.LOCALIZED_ESCAPE_SEQUENCE ) )
			{
				isLocalized = true;
				index = XamMaskedEditor.LOCALIZED_ESCAPE_SEQUENCE.Length;
			}
			// ---------------------------------------------------------------------------------------

			// JAS 12/15/04 Japanese Date Separators Implementation
			Hashtable foreignDateSymbols = null;
			XamMaskedEditor.GetNonstandardForeignDateMaskAndPostfixSymbols( this.FormatProvider, ref foreignDateSymbols );
			bool usingNonstandardForeignDateMask = ( foreignDateSymbols != null && isLocalized );
			SectionBase prevSection = null;

			if ( usingNonstandardForeignDateMask )
				this.AdjustNonstandardForeignDateMask( ref mask );

			while ( index < mask.Length )
			{
				DisplayCharBase dc = null;

				// SSP 2/9/04 UWE728
				// A single character in the mask may map to multiple characters.
				// So allow for that.
				//
				DisplayCharBase[] dcArr = null;

				// SSP 8/13/02 UWM96
				// Skip the new line characters and carriage returns as they are
				// not supported by the masked edit.
				//
				if ( '\n' == mask[index] || '\r' == mask[index] )
				{
					index++;
					continue;
				}

				// SSP 6/14/02 UWG1177
				// Only take the '\' character as an escape character if there
				// are any characters after it. If it's the last one, then 
				// just treat it as a literal.
				//
				//if ( "\\".Equals( this.SafeSubstring( mask,  index, 1 ) ) )
				// SSP 4/8/03 UWG2110
				// Don't go into LiteralEscape state if we are already in it. This way
				// a slash can be escaped by itself.
				//
				//if ( "\\".Equals( this.SafeSubstring( mask,  index, 1 ) ) &&
                
                
                
                
                if ( State.LiteralEscape != state && ESCAPE_CHAR_STRING.Equals( this.SafeSubstring( mask, index, 1 ) ) &&
                    mask.Length > ( 1 + index ) )
				{
					state = State.LiteralEscape;
					index++;
				}
				else if ( State.LiteralEscape == state )
				{
					dc = new LiteralChar( mask[index] );
					dc.Required = false;
					index++;
					state = State.Initial;
				}
				else if ( State.Initial == state && ( '{' == mask[index] || '[' == mask[index] ) )
				{
					char c = mask[index];
                    // SSP 5/6/10 TFS31789
                    // Take into account escaped characters.
                    // 
                    //int endIndex = mask.IndexOf( '{' == c ? '}' : ']', 1 + index );
                    int endIndex = IndexOf( mask, 1 + index, mask.Length - 1, '{' == c ? '}' : ']', true, ESCAPE_CHAR );

					bool matched = false;
					if ( endIndex > 0 )
					{
						string str = this.SafeSubstring( mask, 1 + index, endIndex - index - 1 );

						Match match = Regex.Match( str, @"^(number)(?:\: *(-?\d+) *- *(-?\d+))?$" );
						if ( match.Success )
						{
							string token = match.Groups[1].Value;
							string startRange = match.Groups[2].Value;
							string endRange = match.Groups[3].Value;

							decimal minVal = int.MinValue;
							decimal maxVal = int.MaxValue;
							if ( null != startRange && null != endRange && startRange.Length > 0 && endRange.Length > 0 )
							{
								minVal = decimal.Parse( startRange );
								maxVal = decimal.Parse( endRange );
							}

							int minValDigits = this.GetNumberOfDigitsRequired( (double)minVal );
							int maxValDigits = this.GetNumberOfDigitsRequired( (double)maxVal );
							int digits = Math.Max( minValDigits, maxValDigits );

							NumberSection numberSection = new NumberSection( digits, null, 
								minVal < 0 ? SignDisplayType.ShowWhenNegative : SignDisplayType.None );
							numberSection.MinValue = minVal;
							numberSection.MaxValue = maxVal;

							currentSection = numberSection;
							matched = true;
						}

						if ( !match.Success )
						{
							match = Regex.Match( str, @"^(char)\:(\d+)\:(.+)$" );
							if ( match.Success )
							{
								string token = match.Groups[1].Value;
								string numberStr = match.Groups[2].Value;
								string charSetSpecification = match.Groups[3].Value;

								int number = int.Parse( numberStr );
								CharacterSet.ICharSet set = CharacterSet.ParseSet( charSetSpecification );
								Debug.Assert( null != set && number > 0 );
								if ( null != set && number > 0 )
								{
									dcArr = new DisplayCharBase[number];

									for ( int i = 0; i < dcArr.Length; i++ )
									{
										CharacterSet setDc = new CharacterSet( );
										setDc.InitializeSet( set );
										setDc.FilterType = filter;

										dcArr[i] = setDc;
									}

									matched = true;
								}
							}
						}
					}

					if ( matched )
					{
						index = 1 + endIndex;
					}
					else
					{
						// If we encountered a '{' character that did not enclose a valid token then
						// treat the character as literal.
						// 

						dc = new LiteralChar( c );
						index++;
					}
				}
				else if (
					// SSP 2/21/06 BR09295 
					// Changed the condition fron Initial == state to Date != state. Regardless of the state,
					// if we encounter mm followed by dd or yy section then mm is meant for month.
					// 
					//State.Initial == state && 
					State.Date != state
					// SSP 6/29/10 TFS32569
					// If the state is Time and we haven't encountered a minute section yet then "mm" is for minute section.
					// This bug occurs when the mask is "hh:mm dd/mm/yyyy" where the "mm" after the "hh" is interpreted
					// as a month section even though it's a minute section. Added the following line.
					// 
					&& ( State.Time != state || null != XamMaskedEditor.GetSection( this.sections, typeof( MinuteSection ) ) )
					&& "mm".Equals( this.SafeSubstring( mask, index, 2 ) )
					 // JAS 12/14/04 Japanese DateTime Separators Implementation
					&& (
					"dd".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "mm", "dd", index, true ), 2 ) ) ||
					"yy".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "mm", "yy", index, true ), 2 ) ) ) )
				{
					state = State.Date;
				}
				// JAS 12/14/04 Japanese DateTime Separators Implementation
				else if ( State.Initial == state && "mm".Equals( this.SafeSubstring( mask, index, 2 ) )
					&& (
					"dd".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "mm", "dd", index, false ), 2 ) ) ||
					"yy".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "mm", "yy", index, false ), 2 ) ) ) )
				{
					state = State.Date;
				}
				// JAS 12/14/04 Japanese DateTime Separators Implementation
				else if ( 
					// SSP 6/29/10 TFS32569
					// If preceded by time mask then allow for date mask to follow.
					// 
					//State.Initial == state 
					( State.Initial == state || State.Time == state )
					&& "d".Equals( this.SafeSubstring( mask, index, 1 ) )
					&& (
					"mm".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "d", "mm", index, true ), 2 ) ) ||
					"yy".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "d", "yy", index, true ), 2 ) ) ) )
				{
					state = State.Date;
				}
				// JAS 12/14/04 Japanese DateTime Separators Implementation
				else if ( State.Initial == state && "d".Equals( this.SafeSubstring( mask, index, 1 ) )
					&& (
					"mm".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "d", "mm", index, false ), 2 ) ) ||
					"yy".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "d", "yy", index, false ), 2 ) ) ) )
				{
					state = State.Date;
				}

				else if (
					// SSP 6/29/10 TFS32569
					// If preceded by time mask then allow for date mask to follow.
					// 
					//State.Initial == state 
					( State.Initial == state || State.Time == state )
					&& "dd".Equals( this.SafeSubstring( mask, index, 2 ) )
					 // JAS 12/14/04 Japanese DateTime Separators Implementation
					&& (
					"mm".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "dd", "mm", index, true ), 2 ) ) ||
					"yy".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "dd", "yy", index, true ), 2 ) ) ) )
				{
					state = State.Date;
				}
				// JAS 12/14/04 Japanese DateTime Separators Implementation
				else if ( State.Initial == state && "dd".Equals( this.SafeSubstring( mask, index, 2 ) )
					&& (
					"mm".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "dd", "mm", index, false ), 2 ) ) ||
					"yy".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "dd", "yy", index, false ), 2 ) ) ) )
				{
					state = State.Date;
				}
				else if (
					// SSP 6/29/10 TFS32569
					// If preceded by time mask then allow for date mask to follow.
					// 
					//State.Initial == state 
					( State.Initial == state || State.Time == state )
					&& "yy".Equals( this.SafeSubstring( mask, index, 2 ) )
					 // JAS 12/14/04 Japanese DateTime Separators Implementation
					&& (
					"mm".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "yy", "mm", index, true ), 2 ) ) ||
					"dd".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "yy", "dd", index, true ), 2 ) ) ) )
				{
					state = State.Date;
				}
				// SSP 5/22/02 UWG1233
				// Added the case where yyyy/mm/dd did not parse it correctly.
				// Added below else if block.
				//
				else if (
					// SSP 6/29/10 TFS32569
					// If preceded by time mask then allow for date mask to follow.
					// 
					//State.Initial == state 
					( State.Initial == state || State.Time == state )
					&& "yyyy".Equals( this.SafeSubstring( mask, index, 4 ) )
					 // JAS 12/14/04 Japanese DateTime Separators Implementation
					&& (
					"mm".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "yyyy", "mm", index, true ), 2 ) ) ||
					"dd".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "yyyy", "dd", index, true ), 2 ) ) ) )
				{
					state = State.Date;
				}

					// JAS 12/14/04 Japanese DateTime Separators Implementation - Need to explicitly check for 
				// a four digit year at the end of the date because the user can specify arbitrary seperators.
				else if ( State.Initial == state && "yyyy".Equals( this.SafeSubstring( mask, index, 4 ) )
					&& (
					"mm".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "yyyy", "mm", index, false ), 2 ) ) ||
					"dd".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "yyyy", "dd", index, false ), 2 ) ) ) )
				{
					state = State.Date;
				}


				else if ( State.Date == state && "mm".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					currentSection = new MonthSection( );

                    // AS 8/25/08 Support Calendar
                    MonthSection.InitializeMinMax((MonthSection)currentSection, XamMaskedEditor.GetCultureCalendar(this.FormatProvider));

					index += 2;

				}
				else if ( State.Date == state && "yyyy".Equals( this.SafeSubstring( mask, index, 4 ) ) )
				{
					currentSection = new YearSection( true );

                    // AS 8/25/08 Support Calendar
                    YearSection.InitializeMinMax((YearSection)currentSection, XamMaskedEditor.GetCultureCalendar(this.FormatProvider));

					index += 4;
				}
				else if ( State.Date == state && "yy".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					currentSection = new YearSection( false );

                    // AS 8/25/08 Support Calendar
                    YearSection.InitializeMinMax((YearSection)currentSection, XamMaskedEditor.GetCultureCalendar(this.FormatProvider));

                    index += 2;
				}
				else if ( State.Date == state && "dd".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					currentSection = new DaySection( );
					index += 2;
				}
				else if ( State.Initial == state && "hh".Equals( this.SafeSubstring( mask, index, 2 ) )
					 // JAS 12/14/04 Japanese DateTime Separators Implementation
					&& ( "mm".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "hh", "mm", index, true ), 2 ) ) ) )
				{
					state = State.Time;
				}
				//	BF 6.18.03	UWE567
				//	Added case for a single 'h' character
				else if ( State.Initial == state && "h".Equals( this.SafeSubstring( mask, index, 1 ) )
					 // JAS 12/14/04 Japanese DateTime Separators Implementation
					&& ( "mm".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "h", "mm", index, true ), 2 ) ) ) )
				{
					state = State.Time;
				}
				//				else if ( State.Initial == state && "mm".Equals( this.SafeSubstring( mask, index, 2 ) )
				//					&& this.SafeIndexOfAny( mask, timeSeperators, index+2, 1 ) >= 0
				//					&& ( "ss".Equals( this.SafeSubstring( mask, index + 3, 2 ) )  ) )
				//				{
				//					state = State.Time;
				//				}	
				// JAS 12/14/04 Japanese DateTime Separators Implementation - Added check for "hh" and "h"
				else if ( State.Initial == state && "mm".Equals( this.SafeSubstring( mask, index, 2 ) )
					 // JAS 12/14/04 Japanese DateTime Separators Implementation
					&& ( "ss".Equals( this.SafeSubstring( mask, index + this.GetOffset( mask, "mm", "ss", index, true ), 2 ) ) ||
					"hh".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "mm", "hh", index, false ), 2 ) ) ||
					"h".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "mm", "h", index, false ), 1 ) ) ) )
				{
					state = State.Time;
				}
				// JAS 12/14/04 Japanese DateTime Separators Implementation - Added check for "ss"
				else if ( State.Initial == state && "ss".Equals( this.SafeSubstring( mask, index, 2 ) )
					&& "mm".Equals( this.SafeSubstring( mask, index - this.GetOffset( mask, "ss", "mm", index, false ), 2 ) ) )
				{
					state = State.Time;
				}
				else if ( State.Time == state && "mm".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					currentSection = new MinuteSection( );
					index += 2;
				}
				else if ( State.Time == state && "hh".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					currentSection = new HourSection( );
					index += 2;
				}
				//	BF 6.18.03	UWE567
				//	Added case for a single 'h' character
				else if ( State.Time == state && "h".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					currentSection = new HourSection( );
					index += 1;
				}
				else if ( State.Time == state && "ss".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					currentSection = new SecondSection( );
					index += 2;
				}
				// SSP 1/11/01 
				// Added AM-PM section code
				//
				// When in time section, take into account the empty space characters 
				// (one between the minute section and the ampm section in hh:mm tt)
				//
				else if ( State.Time == state && " ".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					dc = new LiteralChar( ' ' );
					index++;
				}
				// SSP 3/30/12 TFS105384
				// If only "hh" (without corresponding minute section) follows date sections, interpret 
				// it as an hour section.
				// 
				else if ( State.Date == state && " hh".Equals( this.SafeSubstring( mask, index, 3 ) )
					&& !"h".Equals( this.SafeSubstring( mask, 3 + index, 1 ) ) )
				{
					state = State.Time;
					dc = new LiteralChar( ' ' );
					index++;
				}
				else if ( State.Time == state && "tt".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					// SSP 12/18/02 UWG342
					// Optimizations. 
					//
					
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

					string amValue = XamMaskedEditor.GetCultureAMPMDesignator( true, this.FormatProvider );
					string pmValue = XamMaskedEditor.GetCultureAMPMDesignator( false, this.FormatProvider );

					// JDN 12/6/04 BR00645
					// Certain cultures don't have AM/PM designators so we need to test if the strings are empty or null

					index += 2;

					if ( amValue == null || amValue.Length == 0 || pmValue == null || pmValue.Length == 0 )
						continue;

					currentSection = new AMPMSection( amValue, pmValue );

					//index += 2;

				}
				// SSP 1/11/02 
				// Commented these two else if's and added modified versions of
				// them below because we should be looking at other date and time
				// seperators (possibley based on any cutlure settings) and not just
				// ':' and '/'.
				//
				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				else if ( State.Time == state && this.SafeIndexOfAny( mask, timeSeperators, index, 1 ) >= 0 )
				{
					// SSP 12/18/02 UWE342
					// Allow for localized mask.
					//
					// account for the time seperators when parsing a time
					//dc = new LiteralChar( mask[index] );
					char timeSep = isLocalized && ':' == mask[index] ? XamMaskedEditor.GetCultureChar( mask[index], this.FormatProvider ) : mask[index];
					dc = new LiteralChar( timeSep );

					index++;
				}
				else if ( State.Date == state && this.SafeIndexOfAny( mask, dateSeperators, index, 1 ) >= 0 )
				{
					// SSP 12/18/02 UWE342
					// Allow for localized mask.
					//
					// account for the date seperators when parsing a date
					//dc = new LiteralChar( mask[index] );

					//
					// JAS 12/14/04 If we are using the special postfix symbols for dates, use the appropriate
					// symbol for the type of section that was just previously created.
					//
					char dateSep = (char)0;
					if ( usingNonstandardForeignDateMask )
					{
						if ( prevSection != null )
						{
							object symbol = foreignDateSymbols[prevSection.GetType( )];
							if ( symbol != null )
							{
								try
								{
									dateSep = Convert.ToChar( symbol );
								}
								catch ( Exception ex )
								{
									Debug.Fail( "Could not convert foreign date symbol to a char.", ex.Message );
									dateSep = '/';
								}
							}
						}
					}
					else
					{
						dateSep = isLocalized && '/' == mask[index] ? XamMaskedEditor.GetCultureChar( mask[index], this.FormatProvider ) : mask[index];
					}

					dc = new LiteralChar( dateSep );
					index++;

					//					char dateSep = isLocalized && '/' == mask[index] ? EditorWithMask.GetCultureChar( mask[ index ], this.FormatProvider ) : mask[index];
					//					dc = new LiteralChar( dateSep );
					//					index++;
				}
				else if ( "#".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					dc = new DigitChar( );
					dc.FilterType = filter;
					dc.Required = true;
					index++;
					state = State.Initial;
				}
				else if ( "9".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					dc = new DigitChar( );
					dc.FilterType = filter;
					dc.Required = false;
					index++;
					state = State.Initial;
				}
				else if ( index < mask.Length && ( 'h' == mask[index] || 'H' == mask[index] ) )
				{
					dc = new HexDigitChar( );
					dc.FilterType = 'h' == mask[index] ? FilterType.LowerCase : FilterType.UpperCase;
					dc.Required = true;
					index++;
					state = State.Initial;
				}
				else if ( State.Number == state && ".".Equals( this.SafeSubstring( mask, index, 1 ) )
					&& "n".Equals( this.SafeSubstring( mask, 1 + index, 1 ) ) )
				{
					dc = new DecimalSeperatorChar( this.CultureDecimalSeperator );
					index++;
					state = State.FractionPart;

					// JAS 1/6/05 BR00050 - In case there already is a section, we need to 
					// get rid of it so that the decimal point does not get appended to it's display characters.
					// For example, if the mask is "$.nn" then this code prevents the first section
					// from being "$."
					currentSection = null;
				}
				// SSP 3/12/02
				// Added code for creating a fraction part section, namely the
				// following else if statement.
				//
				else if ( State.FractionPart == state && "n".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					int i = index;

					while ( i < mask.Length && 'n' == mask[i] )
					{
						i++;
					}

					// SSP 4/8/05 BR03077
					// This flag indicates that the input characters are to be shifted across number
					// and fraction sections.
					//
					//FractionPart fractionPart = new FractionPart( i - index );
					FractionPart fractionPart = true == this.hasContinuousNumericMask
						? new FractionPartContinuous( i - index )
						: new FractionPart( i - index );

					currentSection = fractionPart;
					index = i;

					state = State.Initial;
				}
				// SSP 6/20/03 UWE606
				// Go into number mode if we have a mask like ".nnnn" where there is no integer part but
				// but there is a fraction part. We want to treat the only number section as fraction
				// and not as a integer section which is what we were doing before this fix.
				// Added below else-if block.
				//
				else if ( State.Initial == state && ".n".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					state = State.Number;
				}
				else if ( "n".Equals( this.SafeSubstring( mask, index, 1 ) ) ||
					// SSP 5/7/02
					// Added support for negative numbers.
					//
					"-n".Equals( this.SafeSubstring( mask, index, 2 ) ) ||
					"+n".Equals( this.SafeSubstring( mask, index, 2 ) ) )
				{
					// Take care of displaying signs in number sections.
					//
					SignDisplayType signType = SignDisplayType.None;

					if ( '-' == mask[index] )
					{
						signType = SignDisplayType.ShowWhenNegative;
						index++;
					}
					else if ( '+' == mask[index] )
					{
						signType = SignDisplayType.ShowAlways;
						index++;
					}

					// SSP 10/5/01
					// 
					int i = index;

					ArrayList commaPositions = new ArrayList( 5 );

					while ( i < mask.Length && ( 'n' == mask[i] || ',' == mask[i] ) )
					{
						if ( ',' == mask[i] && 1 + i < mask.Length )
						{
							// Don't include the comma that is after the last n in the
							// number section. For example, if string specified is
							// "nn,nnn,d"   don't include the second comma in the number
							// section as it is not within the number section and is 
							// actually the comma used for seperating number section
							// from the 'd'.
							//
							if ( 1 + i >= mask.Length || 'n' != mask[1 + i] )
								break;

							commaPositions.Add( i - index - commaPositions.Count - 1 );
						}

						i++;
					}

					NumberSection numberSection = new NumberSection( i - index - commaPositions.Count, (int[])commaPositions.ToArray( typeof( int ) ), signType );
					currentSection = numberSection;
					index = i;

					// SSP 1/14/01
					//
					//state = State.Initial;
					state = State.Number;
				}
				// SSP 12/18/02 UWE342
				// Added support for localized mask.
				// Added following else-if block.
				//
				// ------------------------------------------------------------------------------
				else if ( isLocalized &&
					( '$' == mask[index] || '/' == mask[index] || ':' == mask[index] || ',' == mask[index]
					// SSP 8/16/10 TFS32627
					// In a date or time mask, '.' should not be converted to decimal separator.
					// 
					//|| '.' == mask[index] 
					|| State.Date != state && State.Time != state && '.' == mask[index] 
					|| '+' == mask[index] || '-' == mask[index] ) )
				{
					// SSP 2/9/04 UWE728
					// Handle currency symbol specially because it can have multiple characters.
					// Added the if block and enclosed the existing code into the else block.
					//
					if ( '$' == mask[index] )
					{
						NumberFormatInfo numberFormatInfo = XamMaskedEditor.GetNumberFormatInfo( this.FormatProvider );
						string currencySymbol = null != numberFormatInfo ? numberFormatInfo.CurrencySymbol : null;
						if ( null != currencySymbol )
						{
							dcArr = new DisplayCharBase[currencySymbol.Length];
							for ( int i = 0; i < currencySymbol.Length; i++ )
							{
								dcArr[i] = new LiteralChar( currencySymbol[i] );
								dcArr[i].FilterType = filter;
							}
						}
					}
					else
					{
						char mappedChar = XamMaskedEditor.GetCultureChar( mask[index], this.FormatProvider );
						Debug.Assert( 0 != mappedChar );

						dc = new LiteralChar( mappedChar );
						dc.FilterType = filter;
					}

					index++;
					state = State.Initial;
				}
				// ------------------------------------------------------------------------------
				else if ( "A".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					dc = new AlphanumericChar( );
					dc.FilterType = filter;
					dc.Required = true;
					index++;
					state = State.Initial;
				}
				else if ( "a".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					dc = new AlphanumericChar( );
					dc.FilterType = filter;
					dc.Required = false;
					index++;
					state = State.Initial;
				}
				else if ( "?".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					dc = new AlphaChar( );
					dc.FilterType = filter;
					dc.Required = false;
					index++;
					state = State.Initial;
				}
				else if ( "&".Equals( this.SafeSubstring( mask, index, 1 ) ) ||
					"C".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					dc = new KeyboardAndForeignChar( );
					dc.FilterType = filter;
					dc.Required = false;
					index++;
					state = State.Initial;
				}
				else if ( "<".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					// SSP 11/30/01 UWM49
					// Oops I had it reversed.
					//
					//filter = FilterType.UpperCase;
					filter = FilterType.LowerCase;
					index++;
				}
				else if ( ">".Equals( this.SafeSubstring( mask, index, 1 ) ) )
				{
					// SSP 11/30/01 UWM49
					// Oops I had it reversed.
					//
					//filter = FilterType.LowerCase;
					filter = FilterType.UpperCase;
					index++;
				}
				else
				{
					dc = new LiteralChar( mask[index] );
					dc.Required = false;
					state = State.Initial;
					index++;
				}

				// since display chars and literal chars sections can contain
				// multiple characters and span multiple iterations of this loop
				// we do not want to create a new section every time through the
				// loop, but other wise
				if ( null != currentSection &&
					( ( !( currentSection is DisplayCharsEditSection ) &&
					!( currentSection is LiteralSection ) )

					// SSP 1/11/02
					// Added AMPMSection
					//
					|| ( currentSection is AMPMSection ) ) )
				{
					// a new section other that a LiteralSection of DisplayCharsEditSection was created
					// so add it to collection
					this.Sections.Add( currentSection );
					currentSection.SetFilterToAllChars( filter );

					// JAS 12/15/04 Japanese Date Separators Implementation
					prevSection = currentSection;

					currentSection = null;
					continue;
				}

				// if we get here, then either state transited, or a dc should have been created
				//Debug.Assert( prevState != state || null != dc, "a display character instance should have been created" );
				prevState = state;

				// SSP 2/9/04 UWE728
				// A single character in the mask may map to multiple characters.
				// So allow for that. Added dcArr local var and enclosed the following
				// block of code in the for loop to process this array.
				//
				//if ( null == dc )
				//	continue;
				if ( null == dc && ( null == dcArr || 0 == dcArr.Length ) )
					continue;

				if ( null != dc )
					dcArr = new DisplayCharBase[] { dc };

				for ( int i = 0; null != dcArr && i < dcArr.Length; i++ )
				{
					dc = dcArr[i];

					if ( dc is LiteralChar )
					{
						if ( null == currentSection || !( currentSection is LiteralSection ) )
						{
							currentSection = new LiteralSection( );
							this.Sections.Add( currentSection );
						}
					}
					else
					{
						if ( null == currentSection || !( currentSection is DisplayCharsEditSection ) )
						{
							currentSection = new DisplayCharsEditSection( );
							this.Sections.Add( currentSection );
						}
					}

					Debug.Assert( null != currentSection, "section should have been created by now" );

					if ( null != currentSection )
					{
						// now add the display character to the section and initialize
						// the display character with the section
						currentSection.DisplayChars.Add( dc );
						dc.Initialize( currentSection );
					}
				}
			}


			// SSP 8/16/01 UWG37
			// If it's a '.' literal character and the surrounding display chars
			// are digits, then set the IncludeMethod of the '.' literal character
			// to Always, so that it get's included all the time regardless
			// of the MaskMode


			// SSP 1/17/02
			// If we have AM-PM section, then set the Min and Max values
			// of hour section 1 and 12
			//
			bool hasAMPMSection = false;
			HourSection hourSection = null;

			// SSP 9/12/03 UWG162
			// If there are multiple decimal separators in the mask for some reason then
			// don't set their IncludeMethod to Always like we do when they are surrounded
			// by digit chars or number sections. The reason for this is that when they
			// have a mask like "#.#.#" we will end up including the dot chars even when
			// the MaskeMode passed in is Raw.
			// 
			DisplayCharBase decimalSeparatorCharacter = null;

			for ( int i = 0; i < this.Sections.Count; i++ )
			{
				SectionBase section = this.Sections[i];

				if ( typeof( AMPMSection ) == section.GetType( ) )
					hasAMPMSection = true;

				if ( typeof( HourSection ) == section.GetType( ) )
					hourSection = section as HourSection;

				for ( int j = 0; j < section.DisplayChars.Count; j++ )
				{
					DisplayCharBase dc = section.DisplayChars[j];

					if ( dc is LiteralChar )
					{
						// SSP 1/11/02
						// Use new CultureDecimalSeperator which looks at the culture info
						// settings on the masked edit to determine the decimal seperator
						// and not just the hard coded '.' char.
						//
						if ( dc.Char == this.CultureDecimalSeperator || '.' == dc.Char )
						{
							// JAS 1/6/05 BR00050 - You no longer need to have an 'n' preceding a '.'
							// in order to get a fractional section.  This allows you to have a mask
							// of ".nnnn" without needing an 'n' before the '.'
							
							DisplayCharBase nextDc = dc.NextDisplayChar;

							// If the '.' is surrounded by digits or number sections <--- OBSOLETE COMMENT 
							//
							// JAS 1/6/05 BR00050 - Commented out check of prevDc
							if ( null != nextDc  )
							{
								// JAS 1/6/05 BR00050 - Commented out checks of prevDc
								if ( ( ( nextDc is DigitChar )			    ) ||
									( ( nextDc.Section is NumberSection )  ) )
								{
									//	BF 7.21.03	UWE664
									//
									//	If the current culture's decimal separator happens
									//	to be the same character as its date separator, this
									//	conditional will cause the IncludeMethod to be set to
									//	Always for the DateTime data type, which we don't want.
									//
									//dc.IncludeMethod = DisplayCharIncludeMethod.Always;
									if ( this.state != State.Date )
									{
										// SSP 9/12/03 UWG162
										// If there are multiple decimal separators in the mask for some reason then
										// don't set their IncludeMethod to Always like we do when they are surrounded
										// by digit chars or number sections. The reason for this is that when they
										// have a mask like "#.#.#" we will end up including the dot chars even when
										// the MaskeMode passed in is Raw.
										// 
										// --------------------------------------------------------------------------
										//dc.IncludeMethod = DisplayCharIncludeMethod.Always;
										if ( null == decimalSeparatorCharacter )
										{
											decimalSeparatorCharacter = dc;
											dc.IncludeMethod = DisplayCharIncludeMethod.Always;
										}
										// MBS 10/13/06 BR15874
										// Moved below
										//
										//else
										//{
										//    // If we encounter the "." a second time then reset it's IncludeMethod
										//    // to Default and leave decimalSeparatorCharacter non-null.
										//    //
										//    decimalSeparatorCharacter.IncludeMethod = DisplayCharIncludeMethod.Default;
										//}
										// --------------------------------------------------------------------------
									}
								}
							}
							// MBS 10/13/06 BR15874
							// If we encounter a second decimal at any point, regardless of whether the next character is
							// a digit or not, we shouldn't assume that we're dealing with a number anymore and use the default
							// include method
							if ( null != decimalSeparatorCharacter && decimalSeparatorCharacter != dc )
							{
								decimalSeparatorCharacter.IncludeMethod = DisplayCharIncludeMethod.Default;
								break;
							}
							else if ( null == decimalSeparatorCharacter )
							{
								decimalSeparatorCharacter = dc;
							}
						}
					}
				}
			}


			// SSP 1/17/02
			// If we have AM-PM section, then set the Min and Max values
			// of hour section 1 and 12
			//
			if ( hasAMPMSection && null != hourSection )
			{
				hourSection.MinValue = 1;
				hourSection.MaxValue = 12;
			}

			// JAS 12/15/04 Japanese DateTime Separators Implementation
			// Every section after a date or time section should be considered a "DateTimeSeperator",
			// even the section after the last date/time section.
			int numSections = this.Sections.Count;
			for ( int i = 0; i < numSections; ++i )
				if ( this.Sections[i].IsDateSection || this.Sections[i].IsTimeSection )
					if ( ( i + 1 ) < numSections )
						this.Sections[i + 1].IsDateTimeSeperatorSection = true;
		}

		// This method is for pre-processing {date}, {time} and {longtime} mask tokens.
		//


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void PreProcessMask( ref string mask )
		{
			const string DATE_TOKEN = "{date}";
			// SSP 2/6/09 TFS13259
			// Added DATE_TOKEN_S.
			// 
			const string DATE_TOKEN_S = "{date:s}";

			const string TIME_TOKEN = "{time}";
			const string LONGTIME_TOKEN = "{longtime}";

			IFormatProvider formatProvider = this.FormatProvider;

			if ( mask.IndexOf( DATE_TOKEN ) >= 0 )
			{
				string dateMask = XamMaskedEditor.CalcDefaultDateMask( formatProvider );

				mask = mask.Replace( DATE_TOKEN, dateMask );
			}

			// SSP 2/6/09 TFS13259
			// Added 's' flag to the {date} pattern to prevent usage of non-standard foreign culture
			// date separator symbols from long date pattern, as in japanese culture.
			// 
			if ( mask.IndexOf( DATE_TOKEN_S ) >= 0 )
			{
				string dateMask = XamMaskedEditor.CalcDefaultDateMask( formatProvider, false );

				mask = mask.Replace( DATE_TOKEN_S, dateMask );
			}

			if ( mask.IndexOf( TIME_TOKEN ) >= 0 )
			{
				string timeMask = XamMaskedEditor.CalcDefaultTimeMask( formatProvider );

				mask = mask.Replace( TIME_TOKEN, timeMask );
			}

			if ( mask.IndexOf( LONGTIME_TOKEN ) >= 0 )
			{
				string timeMask = XamMaskedEditor.CalcDefaultLongTimeMask( formatProvider );

				mask = mask.Replace( LONGTIME_TOKEN, timeMask );
			}

			// The following code is for parsing tokens for double and currency with info on number if 
			// integer and fraction digits as well as whether the number input is continuous (from right 
			// to left the numbers are shifted across integer-fraction section boundaries).
			//
			// ------------------------------------------------------------------------------------------
			const string numericRE = @"\{(double|currency)(?:\:(\-|\+)?(\d+)(?:\.(\d+))?)?(?:\:(c))?(?:\:(no_symbol))?\}";
			System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match( mask, numericRE );
			if ( null != match && match.Success && match.Groups.Count > 4 )
			{
				string str;

				string numberType = match.Groups[1].Value;				

				str = match.Groups[2].Value;
				char allowNegativeChar = null != str && str.Length > 0 ? str[0] : (char)0;
				
				str = match.Groups[3].Value;
				int integerDigits = null != str && str.Length > 0 ? int.Parse( str ) : -1;

				str = match.Groups[4].Value;
				int fractionDigits = null != str && str.Length > 0 ? int.Parse( str ) : -1;

				this.hasContinuousNumericMask = "c" == match.Groups[5].Value
					&& integerDigits > 0 && fractionDigits > 0;

				bool noCurrencySymbol = "no_symbol" == match.Groups[6].Value;

				// SSP 2/27/07
				// We decided to change the behavior in wpf to allow negative values by default.
				// If number of digits and fraction part is skipped then allow negatives. So
				// for example, {currency} or {double} mask will allow negative by default.
				// 
				if ( (char)0 == allowNegativeChar && -1 == integerDigits )
					allowNegativeChar = '-';

				string replaceMask = "double" == numberType
					? XamMaskedEditor.CalcDefaultDoubleMask( formatProvider, integerDigits, fractionDigits, allowNegativeChar )
					: XamMaskedEditor.CalcDefaultCurrencyMask( formatProvider, integerDigits, fractionDigits, allowNegativeChar, !noCurrencySymbol );

				mask = mask.Replace( match.Value, replaceMask );
			}
			// ------------------------------------------------------------------------------------------
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static void Parse( string mask, out SectionsCollection sections, IFormatProvider formatProvider )
		{			
			MaskParser parser = new MaskParser( formatProvider );

			parser.Analyze( mask );

			sections = parser.sections;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal static void EscapeLiteralsInString(string baseString, out string escapedString)
        {
            if (baseString == null || baseString.Length == 0)
            {
                Debug.Fail("Null or empty string provided");
                escapedString = String.Empty;
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder(baseString.Length * 2);
            for (int i = 0; i < baseString.Length; i++)
            {
                sb.AppendFormat("\\{0}", baseString[i]);
            }

            escapedString = sb.ToString();
        }
	}

	#endregion // MaskParser Class

	#region ParsedMask Class

	// SSP 10/12/07
	// Added ParsedMask class for applying a mask to text.
	// 
	/// <summary>
	/// A class for applying a mask to data.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ParsedMask</b> parses and stores the parsed mask. It exposes <see cref="ParsedMask.ApplyMask"/>
	/// method for applying the associated mask to the specified data.
	/// </para>
	/// </remarks>
	public class ParsedMask
	{
		private string _mask;
		private SectionsCollection _sections;
		private char _promptChar, _padChar;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mask">The mask</param>
		public ParsedMask( string mask ) : this( mask, null )
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mask">The mask</param>
		/// <param name="formatProvider">The format provider to use to get culture sensitive symbols</param>
		public ParsedMask( string mask, IFormatProvider formatProvider )
			: this( mask, formatProvider, XamMaskedEditor.DEFAULT_PROMPT_CHAR, XamMaskedEditor.DEFAULT_PAD_CHAR )
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mask">The mask</param>
		/// <param name="formatProvider">The format provider to use to get culture sensitive symbols</param>
		/// <param name="promptCharacter">The prompt character - only applicable when applying mask with certain mask modes.</param>
		/// <param name="padCharacter">The prompt character - only applicable when applying mask with certain mask modes.</param>
		public ParsedMask( string mask, IFormatProvider formatProvider, char promptCharacter, char padCharacter )
		{
			if ( string.IsNullOrEmpty( mask ) )
				throw new ArgumentException( );

			_mask = mask;
			_promptChar = promptCharacter;
			_padChar = padCharacter;

			MaskParser.Parse( _mask, out _sections, formatProvider );
		}

		/// <summary>
		/// Applies the mask to the specified data.
		/// </summary>
		/// <param name="data">Data to apply the mask to.</param>
		/// <param name="maskMode">Mask mode to use.</param>
		/// <returns>String that results from applying mask to the specified data.</returns>
		public string ApplyMask( string data, MaskMode maskMode )
		{
			XamMaskedEditor.SetText( _sections, data, _promptChar, _padChar );

			return XamMaskedEditor.GetText( _sections, maskMode, _promptChar, _padChar );
		}

		/// <summary>
		/// Returns the associated mask.
		/// </summary>
		public string Mask
		{
			get
			{
				return _mask;
			}
		}

		/// <summary>
		/// Returns the associated prompt character. Used only when applying mask with certain mask modes.
		/// </summary>
		public char PromptCharacter
		{
			get
			{
				return _promptChar;
			}
		}

		/// <summary>
		/// Returns the associated pad character. Used only when applying mask with certain mask modes.
		/// </summary>
		public char PadCharacter
		{
			get
			{
				return _padChar;
			}
		}
	}

	#endregion // ParsedMask Class

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