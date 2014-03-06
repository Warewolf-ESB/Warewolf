using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections;
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
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Editors
{
	#region DisplayCharBase Class

	/// <summary>
	/// Base type for all DisplayCharacter classes.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// When XamMaskedEditor parses a mask (specified via XamMaskedEditor's <see cref="XamMaskedEditor.Mask"/> property),
	/// the result is a collection of <see cref="SectionBase"/>
	/// derived classes. Each Section in turn is a collection of display characters. Section's
	/// <see cref="SectionBase.DisplayChars"/> property returns the display characters of a section.
	/// XamMaskedEditor returns the collection of sections via its <see cref="XamMaskedEditor.Sections"/>
	/// property. It also exposes <see cref="XamMaskedEditor.DisplayChars"/> property that returns 
	/// a collection of display characters that contains the aggregate display characters from all sections.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// <seealso cref="XamMaskedEditor.Sections"/>
	/// <seealso cref="XamMaskedEditor.DisplayChars"/>
	/// </remarks>
	public abstract class DisplayCharBase : PropertyChangeNotifier
	{
		private FilterType	filterType;
		private DisplayCharIncludeMethod includeMethod = DisplayCharIncludeMethod.Default;
		private SectionBase section;
		private bool required; // = false;
		private int index = -1;

		internal static string PROPERTY_CHAR = "Char";
		internal static string PROPERTY_DRAWSTRING = "DrawString";
		internal static string PROPERTY_VISIBILITY = "Visibility";
		internal static string PROPERTY_DRAWSELECTED = "DrawSelected";







		internal DisplayCharBase( )
		{
		}







		internal DisplayCharBase( SectionBase section )
		{
			this.Initialize( section );
		}


		internal void Initialize( SectionBase section )
		{
			this.section = section;
		}






		internal FilterType FilterType
		{
			get
			{
				return this.filterType;
			}
			set
			{
				this.filterType = value;
			}
		}


		internal SectionBase Section
		{
			get
			{
				return this.section;
			}
		}







		internal int Index
		{
			get
			{
				DisplayCharsCollection parentCollection = this.Section.DisplayChars;
				int count = parentCollection.Count;

				if ( this.index >= 0 && this.index < count )
				{
					if ( this == parentCollection[ this.index ] )
						return this.index;
				}

				this.index = parentCollection.IndexOf( this );

				return this.index;
			}
		}







		internal int OverallIndexInEdit
		{
			get
			{
				if ( this.Section.Index < 0 )
					return -1;

				// if first section
				if ( 0 == this.Section.Index )
				{
					return this.Index;
				}
				else // if not the first section
				{
					// If this is not the first section, then how could this not
					// have a previous section ?
					//
					Debug.Assert( null != this.Section.PreviousSection, "If this is not the first section, then how could this not have a previous section ?" );
					if ( null == this.Section.PreviousSection )
						return this.Index;

					DisplayCharBase dc = this.Section.PreviousSection.LastDisplayChar;
					return ( null != dc ? ( dc.OverallIndexInEdit + 1 ) : 0 ) + this.Index;
				}
			}
		}







		internal virtual bool IsEmpty
		{
			get
			{
				return (int)0 == this.Char;
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool IsSelected
		{
			get
			{
				XamMaskedEditor editor = this.MaskedEdit;
				EditInfo editInfo = null != editor ? editor.EditInfo : null;

				if ( null != editInfo )
				{
					int i = this.OverallIndexInEdit;
					return i >= editInfo.SelectionStart &&
						i < editInfo.SelectionStart + editInfo.SelectionLength;
				}

				return false;
			}
		}


		/// <summary>
		/// This is used to determine whether to draw the display character selected or not.
		/// </summary>
		internal bool DrawSelected
		{
			get
			{
				return this.IsSelected && this.MaskInfo.IsBeingEditedAndFocused;
			}
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal char Filter( char c )
		{
			switch ( this.filterType )
			{
				case FilterType.Unchanged:
					break;
				case FilterType.UpperCase:
					// JJD 12/10/02 - FxCop
					// Pass the culture info in explicitly
					//c = char.ToUpper( c );
					c = char.ToUpper( c, System.Globalization.CultureInfo.CurrentCulture);
					break;
				case FilterType.LowerCase:
					// JJD 12/10/02 - FxCop
					// Pass the culture info in explicitly
					//c = char.ToLower( c );
					c = char.ToLower( c, System.Globalization.CultureInfo.CurrentCulture);
					break;
			}

			return c;
		}

		internal DisplayCharBase PrevDisplayChar
		{
			get
			{
				Debug.Assert( null != this.Section, "no section associated with this display char" );
				if ( null == this.Section )
					return null;

				DisplayCharsCollection displayChars = this.Section.DisplayChars;

				Debug.Assert( null != displayChars, "no DisplayChars associated with the section" );
				if ( null == displayChars )
					return null;

				if ( this.Index - 1 >= 0 )
					return displayChars[ this.Index - 1 ];
				else
					return null != this.Section.PreviousSection ? this.Section.PreviousSection.LastDisplayChar : null;
			}
		}

		internal DisplayCharBase NextDisplayChar
		{
			get
			{
				Debug.Assert( null != this.Section, "no section associated with this display char" );
				if ( null == this.Section )
					return null;

				DisplayCharsCollection displayChars = this.Section.DisplayChars;

				Debug.Assert( null != displayChars, "no DisplayChars associated with the section" );
				if ( null == displayChars )
					return null;

				int index = this.Index;

				if ( 1 + index < displayChars.Count )
				{
					return displayChars[ 1 + index ];
				}
				else
				{
					SectionBase nextSection = this.Section.NextSection;
					return null != nextSection ? nextSection.FirstDisplayChar : null;
				}
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal DisplayCharBase NextEditableDisplayChar
		{
			get
			{
			
				Debug.Assert( null != this.Section, "no section associated with this display char" );
				if ( null == this.Section )
					return null;

				DisplayCharsCollection displayChars = this.Section.DisplayChars;

				Debug.Assert( null != displayChars, "no DisplayChars associated with the section" );
				if ( null == displayChars )
					return null;

				
				// if we are going to have literal display chars in non-literal display chars section
				// then this loop should be used
				for ( int i = 1+this.Index; i < displayChars.Count; i++ )
				{
					DisplayCharBase dc = displayChars[ i ];

					Debug.Assert( null != dc, "null display char in display chars collection" );

					if ( null != dc && dc.IsEditable )
					{
						return dc;
					}
				}
				
				
				// at this point, this display char is the last char in this sections display collection,
				// so get the next edit section and get the first character of that edit section
				if ( null != this.Section.NextEditSection )
				{
					if ( this.Section.NextEditSection.FirstDisplayChar.IsEditable )
						return this.Section.NextEditSection.FirstDisplayChar;
					else
						return this.Section.NextEditSection.FirstDisplayChar.NextEditableDisplayChar;
				}

				return null;
			}
		}

		// SSP 12/2/04 BR00890
		// Added NextFilledEditableDisplayChar property.
		//


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal DisplayCharBase NextFilledEditableDisplayChar
		{
			get
			{
				DisplayCharBase dc = this;
				
				do
				{
					dc = dc.NextEditableDisplayChar;
				}
				while ( null != dc && dc.IsEmpty );

				return dc;				
			}
		}

		/// <summary>
		/// Returns or sets a value that specifies how MaskMode is used in deciding whether or not to include the character. 
		/// </summary>
		/// <remarks>
		/// <p class="body">When <b>IncludeMethod</b> is set to Always, the display character will always be included except when it is empty, in which case normal processing takes effect. (The mask is examined to determine whether a prompt character ot a pad character should be used.) If this property is set to Never, it will never be included in the text. The default setting (DisplayCharIncludeMethod.Default) is to look at the MaskMode that's being applied to the text and determine which character to use accordingly.</p>
		/// </remarks>
		public DisplayCharIncludeMethod IncludeMethod
		{
			get
			{
				return this.includeMethod;
			}
			set
			{
				// test that the value is in range
				//
				Utils.ValidateEnum( "IncludeMethod", typeof( DisplayCharIncludeMethod ), value );

				this.includeMethod = value;
			}
		}		



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal char GetChar( MaskMode maskMode, char promptChar, char padChar )
		{
			char charValue = this.Char;

			switch ( this.IncludeMethod )
			{
				case DisplayCharIncludeMethod.Never:
					return (char)0;					
				case DisplayCharIncludeMethod.Always:
					// Only return it if it's not 0. If it's 0, then do default
					// processing so that if any padding or prompt char is to
					// be included, it will be done so in the default processing.

					// SSP 10/28/02 UWM154
					// Only return a character for the decimal seperator if there
					// is something in the fraction portion.
					//
					// ---------------------------------------------------------------------------
					DisplayCharBase nextDisplayChar = this.NextDisplayChar;
					if ( null != nextDisplayChar && nextDisplayChar.Section is FractionPart
						// SSP 9/15/11 TFS87304
						// I noticed this while debugging this bug.
						// 
						&& this.Section != nextDisplayChar.Section 
						)
					{
						FractionPart fp = (FractionPart)nextDisplayChar.Section;

						// SSP 12/20/02
						// Optimizations.
						//
						
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

						bool isFractionPartEmpty = true;
						DisplayCharsCollection dcColl = fp.DisplayChars;
						for ( int i = 0, count = dcColl.Count; i < count; i++ )
						{
							DisplayCharBase dc = dcColl[i];

							if ( null != dc && 0 != dc.GetChar( maskMode, promptChar, padChar ) )
							{
								isFractionPartEmpty = false;
								break;
							}
						}
						if ( isFractionPartEmpty )
							return (char)0;
					}
					// ---------------------------------------------------------------------------

					if ( 0 != (int)charValue )
						return charValue;
					break;
			}


			char c = (char)0;

			switch ( maskMode )
			{
				case MaskMode.Raw:
					// (Default) Raw Data Mode. Only significant characters will be
					// returned. Any prompt characters or literals will be excluded 
					// from the text.
					if ( this.IsEditable )
						c = charValue;

					break;
				case MaskMode.IncludeLiterals:
					// Include Literal Characters. Data and literal characters will 
					// be returned. Prompt characters will be omitted.
					c = charValue;

					break;
				case MaskMode.IncludePromptChars:
					// Include Prompt Characters. Data and prompt characters will be 
					// returned. Literals will be omitted.
					if ( this.IsEditable )
					{
						c = charValue;
							
						// if the position is not filled, then use prompt character
						if ( 0 == c )							
							c = promptChar;							
					}

					break;
				case MaskMode.IncludeBoth:
					// Include both Prompt Characters and Literals. Text will be 
					// returned exactly as it appears in the object when a cell is
					// in edit mode. Data, prompt character and literals will all be 
					// included.
					c = charValue;
							
					// if the position is not filled, then use prompt character
					if ( 0 == c )							
						c = promptChar;
						
					break;
				case MaskMode.IncludeLiteralsWithPadding:
					// Include Literals With Padding. Prompt characters will be 
					// converted into pad characters (by default they are spaces,
					// which are then included with literals and data when text 
					// is returned.
					c = charValue;
							
					// if the position is not filled, then use padding character
					if ( 0 == c )							
						c = padChar;

					break;
			}

			return c;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal char GetChar( MaskMode maskMode )
		{   
			MaskInfo maskInfo = this.MaskInfo;

			Debug.Assert( null != maskInfo, "null MaskInfo" );
			if ( null == maskInfo )
				return this.Char;

			return this.GetChar( maskMode, maskInfo.PromptChar, maskInfo.PadChar );
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal virtual char GetDrawChar( )
		{	
			MaskInfo maskInfo = this.MaskInfo;

			Debug.Assert( null != maskInfo , "null MaskInfo" );
			if ( null == maskInfo )
				return this.Char;

			if ( maskInfo.IsBeingEditedAndFocused ) // EDIT MODE
			{
				// when in edit mode, everything is going to be displayed
				return this.GetChar( MaskMode.IncludeBoth );
			}
			else // DISPLAY MODE
			{
				// when not in edit mode, use display mask mode
				return this.GetChar( maskInfo.DisplayMode );
			}
		}

		/// <summary>
		/// Returns the string that will be drawn by this display character.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// DrawString property returns text that is to be drawn in place
		/// of this dislay character. This is the property that's used by the DisplayCharacterPresenter
		/// control template to display the display character. 
		/// It takes into account the focused state of the control as well
		/// as whether the display character is empty and if so it returns the prompt character.
		/// </para>
		/// <para class="body">
		/// As any state (for example focused) changes that affects this display character, the DrawString
		/// is updated to reflect the change.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> Typically there is no need for you to use this property. You would need to use
		/// this for example if you are writing a control template for an element that will display this
		/// display character.
		/// </para>
		/// </remarks>
		public string DrawString
		{
			get
			{
				string s = this.GetDrawChar( ).ToString( );
				if ( this.DrawComma && null != this.MaskInfo )
					s += this.MaskInfo.CommaChar;

				return s;
			}
		}

		/// <summary>
		/// Returns whether this display character should be displayed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property is used by the control template of the <see cref="DisplayCharacterPresenter"/>
		/// to hide the element. This property takes into account the state of the control (for example
		/// focused state) and gets updated to reflect changes in the states.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> Typically you don't need to use this property unless for example you are creating 
		/// a control template for a ui element that will display this display character.
		/// </para>
		/// </remarks>
		public Visibility Visibility
		{
			get
			{
				char c = this.GetDrawChar( );
				if ( (char)0 == c )
					return Visibility.Collapsed;

				return Visibility.Visible;
			}
		}

		internal void NotifyPropertyChangedEvent( string propName )
		{
			this.RaisePropertyChangedEvent( propName );

			if ( PROPERTY_CHAR == propName )
			{
				this.RaisePropertyChangedEvent( PROPERTY_VISIBILITY );
				this.RaisePropertyChangedEvent( PROPERTY_DRAWSTRING );

				// SSP 10/23/08 BR35204
				// "." decimal separator character's visibility is dependent on whether the fraction part
				// is empty or not in display mode. Therefore when the fraction part's text is modified,
				// raise appropriate property change notifications on the decimal separator so that the
				// associated elements in the template reflect any potentially new state in the decimal
				// separator's visibility.
				// 
				if ( this.section is FractionPart )
				{
					LiteralSection prevLiteralSection = this.section.PreviousLiteralSection;
					if ( null != prevLiteralSection )
						XamMaskedEditor.NotifyPropertyOnDisplayCharacters( prevLiteralSection, PROPERTY_CHAR );
				}
			}
		}

		// SSP 10/5/01
		// Added this function for drawing commas in number secions.
		//


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool DrawComma
		{
			get
			{
				return false;
			}
		}







		internal XamMaskedEditor MaskedEdit
		{
			get
			{
				SectionBase section = this.section;
				Debug.Assert( null != section, "no section associated with this display char" );
				if ( null == section )
					return null;

				return section.MaskedEdit;
			}
		}







		internal MaskInfo MaskInfo
		{
			get
			{
				return this.Section.MaskInfo;
			}
		}

		/// <summary>
		/// Returns true if the specified character <paramref name="c"/> matches the mask
		/// </summary>
		/// <param name="c">Character to match</param>
		/// <remarks>
		/// <para class="body">
		/// MatchChar method is an abstract method that derived display character classes implement.
		/// This method returns a value indicating whether the specified character matches
		/// this display character. If this method returns False for a character, the user can not enter 
		/// that character in the place of this display character.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> Typically you don't need to use this method directly as the XamMaskedEditor will
		/// automatically validate the user input against the specified mask.
		/// </para>
		/// </remarks>
		public abstract bool MatchChar( char c );






		internal bool Required
		{
			get
			{
				return this.required;
			}
			set
			{
				this.required = value;
			}
		}

		/// <summary>
		/// Indicates if the character position is an editable position, one that
		/// user can input a character into. The literal characters will return False.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Literal display characters return False from this property where as the display 
		/// characters that are place holder for user input return True from this property.
		/// </para>
		/// </remarks>
		public abstract bool IsEditable
		{
			get;			
		}

		// SSP 8/22/03 - Ink Provider Related changes
		// Made Char property public.
		//		
		/// <summary>
		/// Returns the char associated with this character position.
		/// If it's an InputPositionBase derivative (character placeholder),
		/// it will return the character that the user has input, or 0 if it's empty.
		/// 
		/// For LiteralPosition and derivatives, it will return the associated
		/// literal character.
		/// 
		/// Set of this property will only work if IsEditable returns true, otherwise it 
		/// will throw an exception.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Gets or sets the character associated with this display character. When the user
		/// enters text into the editor, this property is updated with the entered character.
		/// As characters are entered, the caret moves so the next character will capture the
		/// next character user enters.
		/// </para>
		/// </remarks>
		public abstract char Char
		{
			get;
			set;
		}

		// SSP 12/2/04 BR00890
		// Added EraseChar helper method.
		//





		internal void EraseChar( )
		{
			if ( this.IsEditable )
				this.Char = (char)0;
		}







		internal virtual void InternalCopy( DisplayCharBase dest )
		{
			if ( this.GetType( ) != dest.GetType( ) )
				throw new ArgumentException( XamMaskedEditor.GetString( "LE_ArgumentException_2" ), "dest" );

			dest.filterType = this.filterType;
			dest.includeMethod = this.includeMethod;
			dest.required = this.required;
		}







		internal abstract DisplayCharBase InternalClone( SectionBase section );
	}

	#endregion // DisplayCharBase Class

	#region InputCharBase Class

	/// <summary>
	/// Class that represents an editable position in the mask edit control
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// This DisplayCharBase derived class is an abstract base class for representing 
	/// character positions in the mask that are editable. See <see cref="DisplayCharBase"/> for
	/// more information.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public abstract class InputCharBase : DisplayCharBase
	{
		char inputChar; // = (char)0;






		internal InputCharBase( )
			: base( )
		{
		}






		internal InputCharBase( SectionBase section )
			: base( section )
		{
		}

		/// <summary>
		/// Overridden, returns true to indicate that InputPositionBase derivatives
		/// are editable character positions.
		/// </summary>
		public override bool IsEditable
		{
			get
			{
				return true;
			}
		}

		// SSP 8/22/03 - Ink Provider Related changes
		// Made Char property public.
		//
		/// <summary>
		/// returns the char associated with this character position.
		/// If it's an InputPositionBase derivative (character placeholder),
		/// it will return the character that the user has input, or 0 if it's empty
		/// 
		/// For LiteralPosition and derivatives, it will return the 
		/// literal character that will be used in storing the text (if one
		/// of the DataMaskModes is to include literals)
		/// 
		/// set will only work if IsEditable returns true, otherwise it will
		/// throw an exception
		/// </summary>
		public override char Char
		{
			get
			{
				return this.inputChar;
			}
			set
			{
				// if an attempt to enter a value other than 0 is done, then we have to
				// see if that value matches using MatchChar.
				if ( (char)0 != value && !this.MatchChar( value ) )
				{
					throw new ArgumentException(XamMaskedEditor.GetString("LE_ArgumentException_3"));
				}

				if ( value != this.inputChar )
				{
					// SSP 8/7/01. We nned to apply filter before assigning.
					//this.inputChar = value;
					this.inputChar = this.Filter( value );

					this.NotifyPropertyChangedEvent( PROPERTY_CHAR );
				}
			}
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal override char GetDrawChar( )
		{	
			char c = base.GetDrawChar( );

			if ( (char)0 == this.Char )
			{
				NumberSection numberSection = this.Section as NumberSection;
			
				// If sign type is to show always, then show the + sign if
				// the number is positive.
				//
				if ( null != numberSection && SignDisplayType.ShowAlways == numberSection.SignType )
				{	
					if ( !numberSection.IsNegative )
					{
						DisplayCharBase dc = numberSection.FirstFilledChar;
					
						DisplayCharBase prevDc = null != dc ? dc.PrevDisplayChar : null;

						if ( this == prevDc )
							c = numberSection.PlusSignChar;
					}
				}
			}

			return c;
		}







		internal override void InternalCopy( DisplayCharBase dest )
		{
			base.InternalCopy( dest );

			InputCharBase destChar = (InputCharBase)dest;
			destChar.inputChar = this.inputChar;
		}
	}

	#endregion // InputCharBase Class

	#region LiteralChar Class

	/// <summary>
	/// Class for matching literal characters.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// LiteralChar represents literals in the mask. Literal characters are the characters that the user
	/// can not modify. For example, in the "###-##-####" mask the two occurrences of '-' are literal characters.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	internal class LiteralChar : InputCharBase
	{
		protected char literal;







		internal LiteralChar( char literal )
			: base( )
		{
			// SSP 1/11/02
			// Use the literal as passed int. Any Culture lookups will be
			// done by the parser.
			//
			//this.literal = this.MaskedEdit.GetCultureChar( literal );
			this.literal = literal;
		}




#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal LiteralChar( char literal, SectionBase section )
			: base( section )
		{
			this.literal = literal;
		}

		// SSP 8/22/03 - Ink Provider Related changes
		// Made MatchChar public.
		//
		/// <summary>
		/// returns true if the character passed in as parameter matches the 
		/// the literal this instance represents
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public override bool MatchChar( char c )
		{
			return this.Filter( c ) == this.Filter( this.literal );
		}







		// JM 08-22-03 - Make this public.  (Initially done for the UltraPenInputPanel)
		//internal override bool IsEditable
		public override bool IsEditable
		{
			get
			{
				return false;
			}
		}

		// SSP 8/22/03 - Ink Provider Related changes
		// Made Char property public.
		//
		/// <summary>
		/// returns the char associated with this character position.
		/// If it's an InputPositionBase derivative (character placeholder),
		/// it will return the character that the user has input, or 0 if it's empty
		/// 
		/// For LiteralPosition and derivatives, it will return the 
		/// literal character that will be used in storing the text (if one
		/// of the DataMaskModes is to include literals)
		/// 
		/// set will only work if IsEditable returns true, otherwise it will
		/// throw an exception
		/// </summary>
		public override char Char
		{
			get
			{
				return this.literal;
			}
			set
			{
				throw new InvalidOperationException(XamMaskedEditor.GetString("LE_InvalidOperationException_3"));
			}
		}







		internal override void InternalCopy( DisplayCharBase dest )
		{
			base.InternalCopy( dest );

			LiteralChar destChar = (LiteralChar)dest;
			destChar.literal = this.literal;
		}







		internal override DisplayCharBase InternalClone( SectionBase section )
		{
			LiteralChar clone = new LiteralChar( this.literal, section );

			this.InternalCopy( clone );

			return clone;
		}

	}

	#endregion // LiteralChar Class

	#region DecimalSeperatorChar Class

	/// <summary>
	/// Class for representing the decimal seperators ('.') in number and currency
	/// sections.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// DecimalSeperatorChar is a literal character that separates integer and fraction parts of
	/// a numeric mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	internal sealed class DecimalSeperatorChar : LiteralChar
	{

		internal DecimalSeperatorChar( char decimalSeperatorChar )
			: base( decimalSeperatorChar )
		{
		}

		// SSP 8/22/03 - Ink Provider Related changes
		// Made Char property public.
		//
		/// <summary>
		/// returns the char associated with this character position.
		/// If it's an InputPositionBase derivative (character placeholder),
		/// it will return the character that the user has input, or 0 if it's empty
		/// 
		/// For LiteralPosition and derivatives, it will return the 
		/// literal character that will be used in storing the text (if one
		/// of the DataMaskModes is to include literals)
		/// 
		/// set will only work if IsEditable returns true, otherwise it will
		/// throw an exception
		/// </summary>
		public override char Char
		{
			get
			{
				if ( null != this.Section )
					return this.Section.DecimalSeperatorChar;

				return base.Char;
			}
			set
			{
				throw new InvalidOperationException(XamMaskedEditor.GetString("LE_InvalidOperationException_3"));
			}
		}

		// SSP 9/30/03 UWE503
		// Overrode MatchChar method.
		//


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public override bool MatchChar( char c )
		{
			if ( c == this.Section.DecimalSeperatorChar )
				return true;
			
			return base.MatchChar( c );
		}







		internal override DisplayCharBase InternalClone( SectionBase section )
		{
			DecimalSeperatorChar clone = new DecimalSeperatorChar( this.literal );

			this.InternalCopy( clone );

			clone.Initialize( section );

			return clone;
		}

		// SSP 9/15/11 TFS87304
		// Overrode GetDrawChar.
		// 
		internal override char GetDrawChar( )
		{
			char c = base.GetDrawChar( );

			if ( 0 == c )
			{
				MaskInfo maskInfo = this.MaskInfo;
				if ( null != maskInfo && maskInfo.IsBeingEditedAndFocused )
					c = this.Char;
			}

			return c;
		}
	}

	#endregion // DecimalSeperatorChar Class

	#region DigitChar Class

	/// <summary>
	/// Class for matching digits.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// DigitChar is a dispaly character that accepts a numeric character (0-9).
	/// Char.IsDigit is used to check if a character is numeric.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public class DigitChar : InputCharBase
	{
		private bool hasComma; // = false;






		internal DigitChar( )
			: base( )
		{
		}






		internal DigitChar( SectionBase section )
			: base( section )
		{
		}

		// SSP 8/22/03 - Ink Provider Related changes
		// Made MatchChar public.
		//
		/// <summary>
		/// checks to see if specified character c mathces a digit
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public override bool MatchChar( char c )
		{
			char filteredChar = this.Filter( c );

			// SSP 5/8/02
			// Added code to support plus-minus in number sections.
			//
			NumberSection numberSection = this.Section as NumberSection;

			if ( null != numberSection &&
				// SSP 3/10/06 BR10576
				// Allow negative sign even if the number section's MinValue is 0 as long
				// as the overall min value is negative. For example if the editor's
				// MinValue is set to -0.5 then the number section's min value is going 
				// to be 0 however it should still allow negative sign.
				// 
				//numberSection.MinValue < 0 
				( numberSection.MinValue < 0 || numberSection.lastConvertedMinValWithFractionPart < 0 )
				)
			{
				char plusChar = numberSection.PlusSignChar;
				char minusChar = numberSection.MinusSignChar;

				if ( plusChar == c || minusChar == c )
				{
					// If the number section is performing a special operation
					// where following requirement needs to be ommited, then return
					// skip checking for that requirement and return true.
					//
					if ( !numberSection.IgnoreStrictSignChecking )
					{
						// All the characters before this character in the number section
						// have to be empty in order for sign to be considered valid for
						// this display char.
						//
						for ( int i = 0; i < numberSection.DisplayChars.Count; i++ )
						{
							DisplayCharBase dc = numberSection.DisplayChars[i];

							if ( !dc.IsEmpty &&	plusChar != dc.Char && minusChar != dc.Char )
								return false;
						
							if ( this == dc )
								break;
						}
					}

					return true;
				}
				else if ( Char.IsDigit( c ) )
				{
					// We should not allow inserting a digit before - or + sign.
					// Digits always go after the - or + sign.
					//
					if ( !numberSection.IgnoreStrictSignChecking )
					{
						int index = this.Index;
						Debug.Assert( index >= 0, "Invalid index !" );						
						
						if ( index >= 0 )
						{
							int count = numberSection.DisplayChars.Count;
							for ( int i = 1 + index; i < count; i++ )
							{
								DisplayCharBase dc = numberSection.DisplayChars[i];

								// If we encounter a + or - sign after the character they are
								// trying to assign a digit character, then return false.
								if ( null != dc && !dc.IsEmpty && ( plusChar == dc.Char || minusChar == dc.Char ) )
									return false;
							}
						}
					}

					// SSP 11/19/03 - Optimizations
					// Since we've already check for the character being a digit character, simply
					// return true here.
					//
					return true;
				}
			}
			
			return Char.IsDigit( filteredChar );
		}

	
		internal virtual bool ShouldIncludeComma( MaskMode maskMode )
		{
			if ( !this.HasComma )
				return false;

			NumberSection section = this.Section as NumberSection;

			// If not a number section, return false.
			//
			if ( null == section )
				return false;

			// If no chars in the number section before this char (including
			// this char ) are filled, then comma shouln't be shown so
			// return false
			//
			if ( 0 == this.Char )
				return false;		
		
			// SSP 5/30/02
			// Don't draw the comma right after + or - signs.
			//
			if ( section.MinusSignChar == this.Char ||
				section.PlusSignChar == this.Char )
				return false;

			// when not in edit mode, use display mask mode to determine
			// whether to include the comma or not
			//
			switch ( maskMode )
			{
				case MaskMode.IncludeBoth:
				case MaskMode.IncludeLiterals:
				case MaskMode.IncludeLiteralsWithPadding:
					return true;
			}							
													

			return false;
		}

		// SSP 10/5/01
		// Added this function for drawing commas in number sections.
		//
		//


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool DrawComma
		{
			get
			{
				Debug.Assert( null != this.MaskInfo, "null MaskInfo" );

				if ( null == this.MaskInfo )
					return false;

				return this.ShouldIncludeComma( 
					this.MaskInfo.IsBeingEditedAndFocused ? MaskMode.IncludeBoth :
					this.MaskInfo.DisplayMode );
			}
		}

	


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool HasComma
		{
			get
			{
				return this.hasComma;
			}
			set
			{
				this.hasComma = value;
			}
		}







		internal override void InternalCopy( DisplayCharBase dest )
		{
			base.InternalCopy( dest );

			DigitChar destChar = (DigitChar)dest;
			destChar.hasComma = this.hasComma;
		}







		internal override DisplayCharBase InternalClone( SectionBase section )
		{
			DigitChar clone = new DigitChar( section );

			this.InternalCopy( clone );

			return clone;
		}
	}

	#endregion // DigitChar Class

	#region AlphaChar Class

	/// <summary>
	/// Class for matching alpha characters [A-Za-z]
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// AlphaChar is a dispaly character that accepts a alphabetic characters (A-Z and a-z).
	/// Char.IsLetter is used to check if a character is alphabetic character.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class AlphaChar : InputCharBase
	{






		internal AlphaChar( )
			: base( )
		{
		}






		internal AlphaChar( SectionBase section )
			: base( section )
		{
		}

		// SSP 8/22/03 - Ink Provider Related changes
		// Made MatchChar public.
		//
		/// <summary>
		/// checks to see if specified character c matches
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public override bool MatchChar( char c )
		{			
			char filteredChar = this.Filter( c );

			return char.IsLetter( filteredChar );
		}








		internal override DisplayCharBase InternalClone( SectionBase section )
		{
			AlphaChar clone = new AlphaChar( section );

			this.InternalCopy( clone );

			AlphaChar destChar = (AlphaChar)clone;

			return clone;
		}
	}

	#endregion // AlphaChar Class

	#region AlphanumericChar Class

	/// <summary>
	/// Class for matching alpha and digits [A-Za-z0-9].
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// AlphanumericChar is a dispaly character that accepts a alpha-numeric characters (A-Z, a-z and 0-9).
	/// Char.IsLetterOrDigit is used to check if a character is alpha-numeric.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class AlphanumericChar : InputCharBase
	{






		internal AlphanumericChar( )
			: base( )
		{
		}






		internal AlphanumericChar( SectionBase section )
			: base( section )
		{
		}

		/// <summary>
		/// checks to see if specified character c matches
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public override bool MatchChar( char c )
		{
			char filteredChar = this.Filter( c );

			return char.IsLetterOrDigit( filteredChar );
		}







		internal override DisplayCharBase InternalClone( SectionBase section )
		{
			AlphanumericChar clone = new AlphanumericChar( section );

			this.InternalCopy( clone );

			AlphanumericChar destChar = (AlphanumericChar)clone;

			return clone;
		}
	}

	#endregion // AlphanumericChar Class

	#region CharacterSet Class

	/// <summary>
	/// Class for matching an arbitrary set of characters.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// CharacterSet is a dispaly character that accepts any character that's part of
	/// an arbitrarily defined set. You can specify a character set in the mask using
	/// {char:n:set} mask token where n is number of display characters to create and
	/// set specifies the set of characters that are acceptable. Here is an example
	/// that creates two display characters that accept a, b, c, d, e, f and 0 to 9
	/// digit characters:<br/>
	/// "{char:2:abcdef0-9}"<br/>
	/// The set can include list of arbitrary characters as well as ranges. In above
	/// example, 'abcdef' is arbitrary character list where as 0-9 is a range. Ranges
	/// are inclusize.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> The character sets are case sensitive. If you want to accept both
	/// the upper and lower case of a character, include both the upper and lower case
	/// character in the list.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class CharacterSet : InputCharBase
	{
		#region Nested Classes/Interfaces

		#region ICharSet Interface

		internal interface ICharSet
		{
			bool Contains( char c );
		}

		#endregion // ICharSet Interface

		#region RangeSet Class

		internal class RangeSet : ICharSet
		{
			char _start, _end;

			public RangeSet( char start, char end )
			{
				// Make sure start is less than end.
				// 
				if ( start > end )
				{
					char tmp = start;
					start = end;
					end = tmp;
				}

				_start = start;
				_end = end;
			}

			public bool Contains( char c )
			{
				return c >= _start && c <= _end;
			}
		}

		#endregion // RangeSet Class

		#region CharSet Class

		internal class CharSet : ICharSet
		{
			private char[] _set;

			public CharSet( char[] set )
			{
				_set = set;
			}

			public bool Contains( char c )
			{
				return Array.IndexOf<char>( _set, c ) >= 0;
			}
		}

		#endregion // CharSet Class

		#region AggregateSet Class

		internal class AggregateSet : ICharSet
		{
			private ICharSet[] _sets;

			public AggregateSet( ICharSet[] sets )
			{
				_sets = sets;
			}

			public bool Contains( char c )
			{
				for ( int i = 0; i < _sets.Length; i++ )
				{
					if ( _sets[i].Contains( c ) )
						return true;
				}

				return false;
			}
		}

		#endregion // AggregateSet Class

		#endregion // Nested Classes/Interfaces

		#region Private Vars

		private CharacterSet.ICharSet _set = null;

		#endregion // Private Vars

		#region Constructor






		internal CharacterSet( ) : base( )
		{
		}






		internal CharacterSet( SectionBase section ) : base( section )
		{
		}

		#endregion // Constructor

		#region InitializeSet

		internal void InitializeSet( ICharSet set )
		{
			_set = set;
		}

		#endregion // InitializeSet

		#region ParseSet

		internal static ICharSet ParseSet( string str )
		{
			const char RANGE_SEPARATOR = '-';

			ArrayList list = new ArrayList( );
			char[] arbitraryChars = new char[str.Length];
			int arbitraryCharsCounter = 0;

			char prevChar = (char)0;

            // SSP 5/6/10 TFS31789
            // 
            // ------------------------------------------------------------------------------
            bool escapeState = false;

			for ( int i = 0; i < str.Length; i++ )
			{
				char c = str[i];

                if ( MaskParser.ESCAPE_CHAR == c && !escapeState )
                {
                    escapeState = true;
                    continue;
                }
                else if ( RANGE_SEPARATOR == c && i > 0 && 1 + i < str.Length && !escapeState )
				{
					arbitraryCharsCounter--;
					i++;

                    // If the next character is escape character, then go the escaped character.
                    // 
                    if ( str[i] == MaskParser.ESCAPE_CHAR && 1 + i < str.Length )
                        i++;

					list.Add( new RangeSet( prevChar, str[i] ) );
				}
				else
				{
                    escapeState = false;
					arbitraryChars[ arbitraryCharsCounter++ ] = c;
				}

				prevChar = c;
			}

            
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

            // ------------------------------------------------------------------------------

			if ( arbitraryCharsCounter > 0 && arbitraryCharsCounter < arbitraryChars.Length )
			{
				char[] tmp = new char[arbitraryCharsCounter];
				Array.Copy( arbitraryChars, tmp, arbitraryCharsCounter );
				arbitraryChars = tmp;
			}

			if ( arbitraryChars.Length > 0 )
				list.Add( new CharSet( arbitraryChars ) );

			if ( 1 == list.Count )
				return (ICharSet)list[0];
			else if ( list.Count > 0 )
				return new AggregateSet( (ICharSet[])list.ToArray( typeof( ICharSet ) ) );

			Debug.Assert( false );
			return null;
		}

		#endregion // ParseSet

		/// <summary>
		/// checks to see if specified character c matches
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public override bool MatchChar( char c )
		{
			char filteredChar = this.Filter( c );

			return _set.Contains( filteredChar );
		}







		internal override DisplayCharBase InternalClone( SectionBase section )
		{
			CharacterSet clone = new CharacterSet( section );

			this.InternalCopy( clone );

			clone._set = this._set;

			return clone;
		}
	}

	#endregion // CharacterSet Class

	#region HexDigitChar Class

	/// <summary>
	/// Class for matching hexadecimal digit.
	/// </summary>
	/// <para class="body">
	/// HexDigitChar is a dispaly character that accepts an hexadecimal digit (0-9, A-F and a-f).
	/// to include a hexadecimal character in the mask, use 'h' or 'H' mask character.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use display characters.
	/// XamMaskedEditor will automatically create and manage these objects based on the supplied mask.
	/// </para>
	public sealed class HexDigitChar : InputCharBase
	{
		internal const string HEX_DIGITS = "0123456789ABCDEFabcdef";






		internal HexDigitChar( ) : base( )
		{
		}






		internal HexDigitChar( SectionBase section ) : base( section )
		{
		}

		/// <summary>
		/// checks to see if specified character c matches
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public override bool MatchChar( char c )
		{
			char filteredChar = this.Filter( c );

			return HEX_DIGITS.IndexOf( filteredChar ) >= 0;
		}







		internal override DisplayCharBase InternalClone( SectionBase section )
		{
			HexDigitChar clone = new HexDigitChar( section );

			this.InternalCopy( clone );

			return clone;
		}
	}

	#endregion // HexDigitChar Class

	#region KeyboardAndForeignChar Class



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal sealed class KeyboardAndForeignChar : InputCharBase
	{







		internal KeyboardAndForeignChar( )
			: base( )
		{
		}






		internal KeyboardAndForeignChar( SectionBase section )
			: base( section )
		{
		}

		// SSP 8/22/03 - Ink Provider Related changes
		// Made MatchChar public.
		//
		/// <summary>
		/// checks to see if specified character c matches
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public override bool MatchChar( char c )
		{
			char filteredChar = this.Filter( c );

			int ordValue = ( int )filteredChar;

			return !( ordValue <= 31 || ordValue == 127 );
		}







		internal override DisplayCharBase InternalClone( SectionBase section )
		{
			KeyboardAndForeignChar clone = new KeyboardAndForeignChar( section );

			this.InternalCopy( clone );

			return clone;
		}
	}

	#endregion // KeyboardAndForeignChar Class

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