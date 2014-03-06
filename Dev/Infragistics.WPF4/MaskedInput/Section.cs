using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using Infragistics.Controls.Editors.Primitives;
using System.Globalization;


namespace Infragistics.Controls.Editors
{
	#region SectionBase

	/// <summary>
	/// Abstract base class for all section classes.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// When XamMaskedInput parses a mask (specified via XamMaskedInput's <see cref="XamMaskedInput.Mask"/> property),
	/// the result is a collection of SectionBase derived classes. Each Section in turn is a collection of display 
	/// characters. Section's <see cref="SectionBase.DisplayChars"/> property returns the display characters of a 
	/// section. XamMaskedInput returns the collection of sections via its <see cref="XamMaskedInput.Sections"/>
	/// property. It also exposes <see cref="XamMaskedInput.DisplayChars"/> property that returns 
	/// a collection of display characters that contains the aggregate display characters from all sections.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// <seealso cref="XamMaskedInput.Sections"/>
	/// <seealso cref="XamMaskedInput.DisplayChars"/>
	/// </remarks>
	public abstract class SectionBase : PropertyChangeNotifier
	{
		private SectionsCollection sections;






		private DisplayCharsCollection _displayChars;

		private int index = -1;

		// JAS 12/15/04 Japanese DateTime Separators Implementation
		private bool isDateSeperatorSection; // = false;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal SectionBase( )
			: base( )
		{
			_displayChars = new DisplayCharsCollection( this );
		}

		internal void InitSectionsCollection( SectionsCollection sections )
		{
			if ( null == sections )
				throw new ArgumentNullException( XamMaskedInput.GetString( "LE_ArgumentNullException_69" ) );

			this.sections = sections;
		}

		/// <summary>
		/// Returns the sections collection this section belongs to.
		/// </summary>
		public SectionsCollection Sections
		{
			get
			{
				return this.sections;
			}
		}







		internal virtual void SetFilterToAllChars( FilterType filter )
		{
			for ( int i = 0; i < this.DisplayChars.Count; i++ )
			{
				this.DisplayChars[i].FilterType = filter;
			}
		}

		#region DisplayChars

		/// <summary>
		/// Returns the display characters of this section.
		/// </summary>
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]

		public DisplayCharsCollection DisplayChars
		{
			get
			{
				return _displayChars;
			}
		}

		#endregion // DisplayChars

		/// <summary>
		/// Checks if str is a valid string for this section.
		/// Either returns <paramref name="str"/> itself or returns a modified string that is to be displayed.
		/// Returns null to indicate that validation failed
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public abstract bool ValidateString( ref string str );






		internal int Index 
		{
			get
			{				
				SectionsCollection parentCollection = this.Sections;

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

		/// <summary>
		/// Finds the previous section.
		/// </summary>
		/// <returns></returns>
		public virtual SectionBase PreviousSection
		{
			get
			{
				// if this is the first section, then return null
				if ( this.Index <= 0 )
					return null;

				return this.Sections[ this.Index - 1 ];
			}
		}

		/// <summary>
		/// Returns the previous literal section, skipping any edit sections.
		/// </summary>
		/// <returns></returns>
		public virtual LiteralSection PreviousLiteralSection
		{		
			get
			{
				SectionBase prevSection = this.PreviousSection;

				// no more sections after this, so return null
				if ( null == prevSection )
					return null;

				// if previous section is a literal section, then return that
				if ( prevSection is LiteralSection )
					return (LiteralSection)prevSection;
				else // otherwise, recurse
					return prevSection.PreviousLiteralSection;
			}
		}

		/// <summary>
		/// Returns the next section.
		/// </summary>
		/// <returns></returns>
		public virtual SectionBase NextSection
		{		
			get
			{
				int index = this.Index;

				if ( 1 + index < this.Sections.Count )
					return this.Sections[1 + index];

				return null;
			}
		}


		/// <summary>
		/// Returns the previous edit section.
		/// </summary>
		/// <returns></returns>
		public virtual EditSectionBase PreviousEditSection
		{
			get
			{
				SectionBase prevSection = this.PreviousSection;

				// no more sections before this, so return null
				if ( null == prevSection )
					return null;

				// if prev section is edit section, then return that
				if ( prevSection is EditSectionBase )
					return (EditSectionBase)prevSection;
				else // otherwise, recurse
					return prevSection.PreviousEditSection;
			}
		}








		internal DisplayCharBase FirstDisplayChar
		{
			get
			{
				// we will never have a section that has 0 chars
				Debug.Assert( this.DisplayChars.Count > 0, "no display chars in the collection" );

				if ( this.DisplayChars.Count <= 0 )
					return null;

				return this.DisplayChars[ 0 ];
			}
		}







		internal DisplayCharBase LastDisplayChar
		{
			get
			{
				// we will never have a section that has 0 chars
				Debug.Assert( this.DisplayChars.Count > 0, "no display chars in the collection" );

				if ( this.DisplayChars.Count <= 0 )
					return null;

				return this.DisplayChars[ this.DisplayChars.Count - 1 ];
			}
		}


		/// <summary>
		/// Returns the next edit section, skipping any literal sections.
		/// </summary>
		public virtual EditSectionBase NextEditSection
		{		
			get
			{
				SectionBase nextSection = this.NextSection;

				// no more sections after this, so return null
				if ( null == nextSection )
					return null;

				// if next section is edit section, then return that
				if ( nextSection is EditSectionBase )
					return (EditSectionBase)nextSection;
				else // otherwise, recurse
					return nextSection.NextEditSection;
			}
		}


		/// <summary>
		/// Returns the next literal section, skipping any edit sections.
		/// </summary>
		public virtual LiteralSection NextLiteralSection
		{		
			get
			{
				SectionBase nextSection = this.NextSection;

				// no more sections after this, so return null
				if ( null == nextSection )
					return null;

				// if next section is literal section, then return that
				if ( nextSection is LiteralSection )
					return (LiteralSection)nextSection;
				else // otherwise, recurse
					return nextSection.NextLiteralSection;
			}
		}







		internal XamMaskedInput MaskedEdit
		{
			get
			{
				return this.Sections.MaskedInput;
			}
		}

		internal MaskInfo MaskInfo
		{
			get
			{
				return this.Sections.MaskInfo;
			}
		}

		// SSP 2/26/02
		// Added DecimalSeperatorChar and CommaChar
		//





		internal char DecimalSeperatorChar
		{
			get
			{
				return XamMaskedInput.GetCultureChar( '.', this.MaskInfo );
			}
		}






		internal char PlusSignChar
		{
			get
			{
				return XamMaskedInput.GetCultureChar( '+', this.MaskInfo );
			}
		}






		internal char MinusSignChar
		{
			get
			{
				return XamMaskedInput.GetCultureChar( '-', this.MaskInfo );
			}
		}
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool IsAllSpaces( )
		{
			foreach ( DisplayCharBase dc in this.DisplayChars )
			{
				if ( ' ' != dc.Char )
					return false;
			}

			return true;
		}






		internal char CommaChar
		{
			get
			{
				return XamMaskedInput.GetCultureChar( ',', this.MaskInfo );
			}
		}
		
		/// <summary>
		/// Returns the text for this section.
		/// </summary>
		/// <returns></returns>
		public string GetText( )
		{
			string text = this.GetText( InputMaskMode.IncludeLiterals );

			return null != text ? text : "";
		}
	


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal virtual string GetText( InputMaskMode maskMode )
		{
            
            
            
            DisplayCharsCollection displayChars = this.DisplayChars;

			if ( null == displayChars || displayChars.Count <= 0 )
			{
				Debug.Assert( false, "null or empty collection returned by MaskedEdit.DisplayChars property" );
				return null;
			}

			int count = displayChars.Count;

			// use string builder, instead of a string since it is more efficient
			StringBuilder s = new StringBuilder( 1 + count );

			for ( int i = 0; i < count; i++ )
			{
				DisplayCharBase cp = displayChars[i];

				Debug.Assert( null != cp, "a null DisplayChar instance encountered in MaskedEdit.DisplayChars" );

				if ( null == cp )
					continue; // cp should never be null

				char c = cp.GetChar( maskMode );

				// if the character is to be included, then append it to the string
				if ( 0 != c )
					s.Append( c );
			}
			
			return s.ToString();
		}







		internal virtual void InternalCopy( SectionBase dest )
		{
			if ( this.GetType( ) != dest.GetType( ) )
				throw new ArgumentException(XamMaskedInput.GetString("LE_ArgumentException_2"), "dest");

			// Only create a clone of the display chars collection if the section
			// has not already created one.
			//
			if ( null == dest._displayChars || dest._displayChars.Count <= 0 )
				dest._displayChars = this._displayChars.InternalClone( dest );
		}







		internal abstract SectionBase InternalClone( SectionsCollection sections );






		internal bool IsDateTimeSeperatorSection
		{
			get
			{
				return this.isDateSeperatorSection;
			}
			set
			{
				this.isDateSeperatorSection = value;
			}
		}






		internal bool IsDateSection
		{
			get
			{
				return this is DaySection || this is MonthSection || this is YearSection;
			}
		}






		internal bool IsTimeSection
		{
			get
			{
				return this is SecondSection || this is MinuteSection || this is HourSection || this is AMPMSection;
			}
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


        // AS 8/25/08 Support Calendar
        internal System.Globalization.Calendar Calendar
        {
            get
            {
                return this.Sections.Calendar;
            }
        }
	}

	#endregion //SectionBase
 
	#region LiteralSection

	/// <summary>
	/// LiteralSection class.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// A LiteralSection is created for each continous occurrences of literals in the mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class LiteralSection : SectionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LiteralSection"/> class
		/// </summary>
		public LiteralSection( )
			: base( )
		{
		}

		/// <summary>
		/// overridden method does nothing
		/// </summary>
		/// <param name="filter"></param>
		internal override void SetFilterToAllChars( FilterType filter )
		{
			// filters should not be applied to literal characters
		}

		/// <summary>
		/// Validates the string for this section.
		/// </summary>
		/// This method checks to see if the specified string is a valid string for this section. It may modify the specified string in which case the new string will be close to the specified string, but one that matches the section. This method returns False to indicate that validation failed.
		/// <param name="str">string to validate</param>
		/// <returns><b>True</b> if string is valid, <b>false</b> otherwise</returns>
		public override bool ValidateString( ref string str )
		{
			DisplayCharsCollection displayChars = this.DisplayChars;

			// check for the length of str. we can't have less or more
			// characters in str
			// 
			if ( null == str || displayChars.Count != str.Length )
				return false;

			// all the characters in str has to match the corrsponding
			// display chars			
			//
			for ( int i = 0; i < displayChars.Count; i++ )
			{
				DisplayCharBase dc = displayChars[i];

				if ( !dc.MatchChar( str[i] ) )
					return false;
			}

			return true;
		}








		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			LiteralSection clone = new LiteralSection( );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}
	}


	#endregion //LiteralSection

	#region EditSectionBase

	/// <summary>
	/// Abstract base class for non-literal sections.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// EditSectionBase is a base class for all editable sections.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public abstract class EditSectionBase : SectionBase
	{






		internal EditSectionBase( )
			: base( )
		{
		}

		#region SafeDisplayCharAt

		/// <summary>
		/// Returns character in DisplayChars collection at index.
		/// </summary>
		/// <param name="index">index of char to display</param>
		/// <returns>character at position or null if not found.</returns>
		protected DisplayCharBase SafeDisplayCharAt( int index )
		{
			if ( index >= 0 && index < this.DisplayChars.Count )
				return this.DisplayChars[ index ];

			return null;
		}

		#endregion //SafeDisplayCharAt

		#region CanShift



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private bool CanShift( int from, int positionsToShift, bool left )
		{
			int count = this.DisplayChars.Count;

			// can't have an invalid starting point for shifting the characters
			if ( from < 0 || from >= count )
				return false;

			// can't have negative number for number of characters to shift
			if ( positionsToShift < 0 )
				return false;


			// can't shift more characters from a position than we have
			if ( from + positionsToShift > count )
				return false;

						
			bool canShift = true;

			if ( left )
			{
				// shifting left
				for ( int i = from; i < this.DisplayChars.Count; i++ )
				{
					DisplayCharBase dc1 = this.SafeDisplayCharAt( i );
					DisplayCharBase dc2 = this.SafeDisplayCharAt( i + positionsToShift );

					// in case we encounter a literal, we have to get a non-literal
					// character before it

					if ( null != dc1 && !dc1.IsEditable )
						continue;

					if ( null != dc2 && !dc2.IsEditable )
					{
						int tmp = positionsToShift+1;
						while ( null != dc2 && !dc2.IsEditable )
						{
							dc2 = this.SafeDisplayCharAt( i + tmp );
							tmp++;
						}
					}
			
					char c = (char)0;

					if ( null != dc2 )
						c = dc2.Char;

					if ( null != dc1 && ( c == (int)0 || dc1.MatchChar( c ) ) )
					{
					}
					else if ( (int)c != 0 )
					{
						canShift = false;	
						break;
					}					
				}

			}
			else
			{
				// shifting right
				for ( int i = this.DisplayChars.Count - 1; i >= from; i-- )
				{
					DisplayCharBase dc1 = this.SafeDisplayCharAt( i );
					DisplayCharBase dc2 = null;
					if ( i - positionsToShift >= from )
						dc2 = this.SafeDisplayCharAt( i - positionsToShift );

					// in case we encounter a literal, we have to get a non-literal
					// character before it

					if ( null != dc1 && !dc1.IsEditable )
						continue;

					if ( null != dc2 && !dc2.IsEditable )
					{
						int tmp = positionsToShift+1;
						while ( null != dc2 && !dc2.IsEditable )
						{
							dc2 = this.SafeDisplayCharAt( i - tmp );
							tmp++;
						}
					}

					char c = (char)0;

					if ( null != dc2 )
						c = dc2.Char;

					if ( null != dc1 && ( c == (int)0 || dc1.MatchChar( c ) ) )
					{											
					}
					else if ( (int)c != 0 )
					{
						canShift = false;
						break;
					}								
				}

			}


			return canShift;
		}

		#endregion //CanShift

		#region Shift



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private bool Shift( int from, int positionsToShift, bool left )
		{
			if ( !this.CanShift( from, positionsToShift, left ) )
				return false;

			int count = this.DisplayChars.Count;

			// can't have an invalid starting point for shifting the characters
			if ( from < 0 || from >= count )
				return false;

			// can't have negative number for number of characters to shift
			if ( positionsToShift < 0 )
				return false;


			// can't shift more characters from a position than we have
			if ( from + positionsToShift > count )
				return false;


			bool couldShift = true;

			if ( left )
			{
				// shifting left
				for ( int i = from; i < this.DisplayChars.Count; i++ )
				{
					DisplayCharBase dc1 = this.SafeDisplayCharAt( i );
					DisplayCharBase dc2 = this.SafeDisplayCharAt( i + positionsToShift );

					// in case we encounter a literal, we have to get a non-literal
					// character before it

					if ( null != dc1 && !dc1.IsEditable )
						continue;

					if ( null != dc2 && !dc2.IsEditable )
					{
						int tmp = positionsToShift+1;
						while ( null != dc2 && !dc2.IsEditable )
						{
							dc2 = this.SafeDisplayCharAt( i + tmp );
							tmp++;
						}
					}
			
					char c = (char)0;

					if ( null != dc2 )
						c = dc2.Char;

					if ( null != dc1 && ( c == (int)0 || dc1.MatchChar( c ) ) )
					{
						dc1.Char = c;											
					}
					else
					{
						couldShift = false;	
					}					
				}

			}
			else
			{
				// shifting right
				for ( int i = this.DisplayChars.Count - 1; i >= from; i-- )
				{
					DisplayCharBase dc1 = this.SafeDisplayCharAt( i );
					DisplayCharBase dc2 = null;
					if ( i - positionsToShift >= from )
						dc2 = this.SafeDisplayCharAt( i - positionsToShift );

					// in case we encounter a literal, we have to get a non-literal
					// character before it

					if ( null != dc1 && !dc1.IsEditable )
						continue;

					if ( null != dc2 && !dc2.IsEditable )
					{
						int tmp = positionsToShift+1;
						while ( null != dc2 && !dc2.IsEditable )
						{
							dc2 = this.SafeDisplayCharAt( i - tmp );
							tmp++;
						}
					}

					char c = (char)0;

					if ( null != dc2 )
						c = dc2.Char;

					if ( null != dc1 && ( c == (int)0 || dc1.MatchChar( c ) ) )
					{
						dc1.Char = c;											
					}
					else
					{
						couldShift = false;	
					}								
				}

			}

			return couldShift;
		}

		#endregion //Shift

		#region IsInt



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool IsInt( string str )
		{
			if ( str.Length == 0 )
				return false;

			int i = 0;

			// SSP 5/8/02
			// Added support for signed numbers.
			// Take into account first character being a plus or a minus.
			//
			if ( this.PlusSignChar == str[i] ||
				this.MinusSignChar == str[i] )
			{
				i++;
			}

			for ( ; i < str.Length; i++ )
			{
				if ( !char.IsDigit( str, i ) )
					return false;
			}

			return true;
		}

		#endregion //IsInt

		#region SetText

		// SSP 2/18/04
		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public abstract void SetText( string text );

		#endregion // SetText
		






		internal virtual bool IsInputFull( )
		{
			for ( int i = 0; i < this.DisplayChars.Count; i++ )
			{
				DisplayCharBase dc = this.DisplayChars[ i ];

				if ( dc.IsEditable && dc.IsEmpty )
					return false;
			}

			return true;
		}




#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool CanShiftRight( int from, int positionsToShift )
		{
			return this.CanShift( from, positionsToShift, false );
		}




#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool CanShiftLeft( int from, int positionsToShift )
		{
			return this.CanShift( from, positionsToShift, true );
		}

		internal virtual bool ShiftLeft( int from, int positionsToShift )
		{
			return this.Shift( from, positionsToShift, true );
		}

		internal virtual bool ShiftRight( int from, int positionsToShift )
		{
			return this.Shift( from, positionsToShift, false );
		}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool DeleteCharWithNoShift( int pos )
		{
			Debug.Assert( pos >= 0 && pos < this.DisplayChars.Count, 
				"attempt to delete character at an invalid position" );
			if ( pos < 0 || pos >= this.DisplayChars.Count )
				return false;

			this.DisplayChars[ pos ].Char = (char)0;

			return true;
		}




#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool DeleteCharsAndShift( int pos, int count )
		{
			Debug.Assert( pos >= 0 && pos < this.DisplayChars.Count, 
				"attempt to delete character at an invalid position" );
			if ( pos < 0 || pos >= this.DisplayChars.Count )
				return false;


			if ( this.CanShiftLeft( pos, count ) )
			{
				this.ShiftLeft( pos, count );				
				return true;
			}

			return false;
		}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool DeleteCharAndShift( int pos )
		{
			Debug.Assert( pos >= 0 && pos < this.DisplayChars.Count, 
				"attempt to delete character at an invalid position" );
			if ( pos < 0 || pos >= this.DisplayChars.Count )
				return false;


			if ( this.CanShiftLeft( pos, 1 ) )
			{
				this.ShiftLeft( pos, 1 );				
				return true;
			}

			return false;
		}




#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool ReplaceCharAt( int pos, char c )
		{
			Debug.Assert( pos >= 0 && pos < this.DisplayChars.Count, 
				"attempt to replace character at an invalid position" );
			if ( pos < 0 || pos > this.DisplayChars.Count )
				return false;

			DisplayCharBase dc = this.DisplayChars[ pos ];

			// this should never be the case, since display chars collection always holds
			// non null values
			Debug.Assert( null != dc, "null display char in display chars collection" );
			if ( null == dc )
				return false;

			Debug.Assert( dc.IsEditable, "attempt to insert character at a literal DisplayChar position" );
			if ( !dc.IsEditable )
				return false;

			if ( !dc.MatchChar( c ) )
				return false;		
			
			dc.Char = c;

			return true;
		}


		internal virtual bool CanInsertCharAt( int pos, char c )
		{
			Debug.Assert( pos >= 0 && pos < this.DisplayChars.Count, 
				"attempt to replace character at an invalid position" );
			if ( pos < 0 || pos > this.DisplayChars.Count )
				return false;

			DisplayCharBase dc = this.DisplayChars[ pos ];


			// this should never be the case, since display chars collection always holds
			// non null values
			Debug.Assert( null != dc, "null display char in display chars collection" );
			if ( null == dc )
				return false;

			Debug.Assert( dc.IsEditable, "attempt to insert character at a literal DisplayChar position" );
			if ( !dc.IsEditable )
				return false;

			if ( !dc.MatchChar( c ) )
				return false;

			if ( !this.LastDisplayChar.IsEmpty )
				return false;

			// if we cannot shift to right by 1 character, then we can't insert the character
			if ( !this.CanShiftRight( pos, 1 ) )			
				return false;

			return true;
		}




#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool InsertCharAt( int pos, char c )
		{

			if ( this.CanInsertCharAt( pos, c ) )
			{
			
				this.Shift( pos, 1, false );

				DisplayCharBase dc = this.SafeDisplayCharAt( pos );

				dc.Char = c;			

				return true;
			}

			return false;
		}




#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal virtual void EraseChars( int startIndex, int endIndex )
		{
			if ( startIndex > endIndex )
			{
				int tmp = startIndex;
				startIndex = endIndex;
				endIndex = tmp;
			}

            
            
            
            DisplayCharsCollection displayChars = this.DisplayChars;
            int displayCharsCount = displayChars.Count;

			for ( int i = startIndex; i <= endIndex; i++ )
			{
				if ( i >= displayCharsCount )
					continue;

				DisplayCharBase dc = displayChars[ i ];

				if ( dc.IsEditable )
					dc.Char = (char)0;
			}
		}







		// SSP 3/26/02
		//  
		//internal virtual void EraseAllChars( )
		internal void EraseAllChars( )
		{
			this.EraseChars( 0, this.DisplayChars.Count - 1 );
		}







		internal virtual DisplayCharBase FirstFilledChar
		{
			get
			{
                
                
                
                DisplayCharsCollection dcColl = this.DisplayChars;
                int count = dcColl.Count;

				for ( int i = 0; i < count; i++ )
				{
					DisplayCharBase dc = dcColl[ i ];

					if ( !dc.IsEditable )
						continue;

					if ( (char)0 != dc.Char )
						return dc;
				}

				return null;
			}
		}

		// SSP 4/8/05 BR03077
		//





		internal virtual bool IsEmpty
		{
			get
			{
				return null == this.FirstFilledChar;
			}
		}







		internal virtual DisplayCharBase LastFilledChar
		{
			get
			{
				for ( int i = this.DisplayChars.Count - 1; i >= 0 ; i-- )
				{
					DisplayCharBase dc = this.DisplayChars[ i ];

					if ( !dc.IsEditable )
						continue;

					if ( (char)0 != dc.Char )
						return dc;
				}

				return null;
			}
		}

		/// <summary>
		/// Validates the section
		/// </summary>
		/// This method is usually invoked when the input position is being removed from the section. It returns True if an appropriate value has been input in the section. This function may modify the values of display characters.
		/// <returns></returns>
		public bool ValidateSection(  )
		{
			return this.ValidateSection( true );
		}

		// SSP 3/13/02
		// Added contentModificationsAllowed argument.
		//
		/// <summary>
		/// Validates the section.
		/// </summary>
		/// <remarks>
		/// This method is usually invoked when the input position is being removed from the section. It returns True if an appropriate value has been input in the section. This function may modify the values of display characters.
		/// </remarks>
		/// <param name="contentModificationsAllowed">Whether the implementation should modify the contents.</param>
		/// <returns></returns>
		public abstract bool ValidateSection( bool contentModificationsAllowed );

		/// <summary>
		/// Returns whether editing is left-to-right or right-to-left.
		/// </summary>
		public abstract EditOrientation Orientation { get; }

		// SSP 3/26/02
		// Added these internal virtual methods for spinning
		//





		internal virtual bool SupportsSpinning
		{
			get
			{
				return false;
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool CanSpin( bool up )
		{
			return false;
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal virtual bool Spin( bool up, bool setLimit )
		{
			return false;
		}
	}


	#endregion //EditSectionBase

	#region DisplayCharsEditSection

	/// <summary>
	/// This is an edit section that can contain an arbitrary list of editable display characters.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// DisplayCharsEditSection is created for each group of consecutive editable characters in the mask.
	/// Note that this section is create for editable characters that do not have their own special sections
	/// associated with. For example, "mm", "dd", and "yyyy" have <see cref="MonthSection"/>, 
	/// <see cref="DaySection"/> and <see cref="YearSection"/> associated with them so for these the
	/// DisplayCharsEditSection is not created. However for other mask characters for which there are
	/// no special sections associated with them, this object will be created for a group of continuous
	/// edit mask character in the mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public class DisplayCharsEditSection : EditSectionBase
	{





		internal DisplayCharsEditSection( )
			: base( )
		{
		}


		/// <summary>
		/// indicates whether this section is a right-to-left edit section (number section)
		/// or a left-to-right (regular edit sections)
		/// </summary>
		public override EditOrientation Orientation 
		{ 
			get
			{
				return EditOrientation.LeftToRight;
			}
		}


		// SSP 2/18/04
		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			int textIndex = 0;
			for ( int i = 0; i < this.DisplayChars.Count; i++ )
			{
				DisplayCharBase dc = this.DisplayChars[ i ];

				if ( !dc.IsEditable )
					continue;

				char c = (char)0;
				if ( textIndex < text.Length )
					c = text[ textIndex++ ];

				dc.Char = c;
			}
		}

		/// <summary>
		/// Checks if str is a valid string for this section.
		/// Either returns itself or returns a modified string that is to be displayed.
		/// Returns false to indicate that validation failed
		/// </summary>
		/// <param name="str">string to validate</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise.</returns>
		public override bool ValidateString( ref  string str )
		{
			// check for the length of str. we can't have less or more
			// characters in str
			// 
			if ( null == str || this.DisplayChars.Count != str.Length )
				return false;

			char padChar = this.MaskInfo.PadChar;
			char promptChar = this.MaskInfo.PromptChar;

			// all the characters in str has to match the corrsponding
			// display chars			
			//
			for ( int i = 0; i < this.DisplayChars.Count; i++ )
			{
				DisplayCharBase dc = this.DisplayChars[i];

				if ( !dc.MatchChar( str[i] ) )
				{
					// SSP 8/22/03 - Ink Provider Related changes
					// If the display character is not required and the character is
					// a pad or a prompt character, then pass it.
					//
					if ( ( padChar == str[i] || promptChar == str[i] ) && 
						dc.IsEditable && ! dc.Required )
						continue;

					return false;
				}
			}

			return true;
		}


		
		/// <summary>
		/// checks to see if the so far input chars in the section
		/// satisfy the input requirement for the section
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not to allow content modification</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise.</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{
			int i = 0;

			//StringBuilder sb = new StringBuilder( string.Empty, 10 );

			// traverse throught all the display chars belonging to this
			// edit section
			while ( i < this.DisplayChars.Count  && 
				this == this.DisplayChars[i].Section )
			{
				DisplayCharBase dc = this.DisplayChars[i];

				Debug.Assert( null != dc, "null instance of DisplayChar encountered in DisplayChars collection" );

				if ( null != dc )
				{
					if ( 0 != dc.Char )
					{
						if ( !dc.MatchChar(  dc.Char  ) )						
								return false;
					}
					else
					{
						// if required and empty
						if ( dc.Required )
							return false;
					}
				}

				i++;
			}

			// if we get here, then we have successfully treaversed through 
			// all the display characters associated with this edit section
			// and are found to be valid
			return true;
		}







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			DisplayCharsEditSection clone = new DisplayCharsEditSection( );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}
		
	}
	

	#endregion //DisplayCharsEditSection

	#region NumberSection

	/// <summary>
	/// A number section that will edit from right to left.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// DisplayCharsEditSection is created for each group of consecutive editable characters in the mask.
	/// Note that this section is create for editable characters that do not have their own special sections
	/// associated with. For example, "mm", "dd", and "yyyy" have <see cref="MonthSection"/>, 
	/// <see cref="DaySection"/> and <see cref="YearSection"/> associated with them so for these the
	/// DisplayCharsEditSection is not created. However for other mask characters for which there are
	/// no special sections associated with them, this object will be created for a group of continuous
	/// edit mask characters in the mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public class NumberSection : EditSectionBase
	{
		#region Private Vars

		private int digits;
		private int[] commaPositions;
		
		//private int minValue = int.MinValue;
		private decimal minValue; // = 0;
		private decimal maxValue = int.MaxValue;

		// SSP 5/8/02
		// Added support for signs in number sections.
		//
		private SignDisplayType signType = SignDisplayType.None;

		// A flag to let the DigitChar know not to perform strict checking
		// in MatchChar for assigning plus or minus symbol
		//
		private bool ignoreStrictSignChecking; // = false;

		// SSP 11/19/03 UWE749
		// Added a caching mechanism to cache the converted min and max value.
		//
		// SSP 8/16/10 TFS27897
		// 
		
		
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private object _lastMinValue = DependencyProperty.UnsetValue;
		private object _lastMaxValue;
		private object _lastMinValueExclusive;
		private object _lastMaxValueExclusive;
		private decimal _lastCalculatedMinValue;
		private decimal _lastCalculatedMaxValue;
		

		// SSP 3/10/06 BR10576
		// Allow negative sign even if the number section's MinValue is 0 as long
		// as the overall min value is negative. For example if the editor's
		// MinValue is set to -0.5 then the number section's min value is going 
		// to be 0 however it should still allow negative sign.
		// 
		internal decimal lastConvertedMinValWithFractionPart = decimal.MinValue;

		// SSP 4/6/12 TFS95799
		// 
		internal decimal lastConvertedMaxValWithFractionPart = decimal.MaxValue;

		// MRS 12/12/05 - BR07946
		//
		private bool isMinValueExclusive; // = false;
		private bool isMaxValueExclusive; // = false;

		// SSP 10/3/06 BR16287
		// 
		private decimal defaultMinValue; // = 0;

		#endregion // Private Vars







		internal NumberSection( int digits )
			: this( digits, null, SignDisplayType.None )
		{
		}



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		internal NumberSection( int digits, int[] commaPositions, SignDisplayType signType )
			: base( )
		{
			this.InternalInitialize( digits, commaPositions, signType );
		}

		private void InternalInitialize( int digits, int[] commaPositions, SignDisplayType signType )
		{
			if ( digits <= 0 )
				throw new ArgumentOutOfRangeException("digits", XamMaskedInput.GetString("LE_ArgumentOutOfRangeException_1"));

			this.digits = digits;
			this.commaPositions = commaPositions;
			this.signType = signType;			
			// SSP 7/11/02 UWM105
			// Now we are adding an extra character place if the section is signed so no need
			// to account for sign here.
			//
			//this.minValue = SignDisplayType.None != signType ? ( -(int)Math.Pow( 10, digits - 1 ) + 1 ) : 0;
			// SSP 8/16/02
			// Use decimal instead of int to allow for numeric values larger than what an int can hold.
			//
			// -------------------------------------------------------------------------
			//this.minValue = SignDisplayType.None != signType ? ( -(int)Math.Pow( 10, digits ) + 1 ) : 0;
			//this.maxValue = (int)Math.Pow( 10, digits ) - 1;

			// SSP 10/3/06 BR16287
			// 
			// ----------------------------------------------------------
			// SSP 2/29/12 TFS92791
			// Catch the overflow exception and fallback to min value.
			// 
			//decimal defMinVal = -(decimal)Math.Pow( 10, digits ) + 1;
			decimal defMinVal;
			try
			{
				defMinVal = -(decimal)Math.Pow( 10, digits ) + 1;
			}
			catch ( OverflowException )
			{
				defMinVal = decimal.MinValue;
			}

			if ( defMinVal != decimal.Floor( defMinVal ) )
				defMinVal = decimal.Floor( defMinVal ) + 1;

			this.defaultMinValue = defMinVal;

			decimal min;
			if ( SignDisplayType.None != signType )
			{
				min = defMinVal;
				// SSP 2/29/12 TFS92791
				// 
				//this.lastConvertedMinValWithFractionPart = defMinVal - 1;
				this.lastConvertedMinValWithFractionPart = defMinVal == decimal.MinValue ? defMinVal : defMinVal - 1;
			}
			else
			{
				// If singType doesn't allow for negative numbers, then the min value
				// would be 0.
				//
				min = 0;
				this.lastConvertedMinValWithFractionPart = 0;
			}

			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			// ----------------------------------------------------------

			// SSP 2/29/12 TFS92791
			// Catch the overflow exception and fallback to min value.
			// 
			//decimal max = decimal.Floor( (decimal)Math.Pow( 10, digits ) - 1 );
			decimal max;
			try
			{
				max = decimal.Floor( (decimal)Math.Pow( 10, digits ) - 1 );
			}
			catch ( OverflowException )
			{
				max = decimal.MaxValue;
			}

			this.minValue = min;
			this.maxValue = max;
			// -------------------------------------------------------------------------

			// SSP 4/6/12 TFS95799
			// 
			this.lastConvertedMinValWithFractionPart = this.maxValue;
			
			this.CreateDisplayChars( );
		}







		internal SignDisplayType SignType
		{
			get
			{
				return this.signType;
			}
			set
			{
				Utils.ValidateEnum( "SignType", typeof( SignDisplayType ), value );

				Debug.Assert( typeof( NumberSection ) == this.GetType( ), "SignType should only be assigned to a number section." );

				this.signType = value;
			}
		}






		internal bool IsNegative
		{
			get
			{
				DisplayCharsCollection displayChars = this.DisplayChars;
				int count = displayChars.Count;

				for ( int i = 0; i < count; i++ )
				{
					DisplayCharBase dc = displayChars[i];

					Debug.Assert( null != dc, "Null element !" );

					if ( null == dc )
						continue;

					if ( this.MinusSignChar == dc.Char )
					{
						return true;
					}
					else if ( !dc.IsEmpty )
						return false;
				}

				return false;
			}
		}

		// SSP 3/15/12 TFS98213
		// 
		/// <summary>
		/// Returns the associated fraction part.
		/// </summary>
		internal FractionPart FractionPart
		{
			get
			{
				return this.NextEditSection as FractionPart;
			}
		}

		// SSP 3/15/12 TFS98213
		// 
		/// <summary>
		/// Returns true if there's an associated fraction part and it's not empty.
		/// </summary>
		internal bool IsFractionPartNonEmpty
		{
			get
			{
				FractionPart fractionPart = this.FractionPart;
				return null != fractionPart && ! fractionPart.IsEmpty;
			}
		}







		internal bool SetNumberSign( bool negative )
		{
			// If we are already the same sign, then return.
			//
			if ( this.IsNegative == negative )
				return true;

			DisplayCharsCollection displayChars = this.DisplayChars;
			int count = displayChars.Count;

			DisplayCharBase firstDc = this.FirstFilledChar;

			// If the whole section is empty then just put the sign in the last
			// display character (remember, the number section is right to left).
			//
			if ( null == firstDc )
			{
				if ( negative )
				{					
					DisplayCharBase lastDc = this.LastDisplayChar;

					Debug.Assert( null != lastDc, "LastDisplayChar returned null !" );

					if ( null != lastDc )
						lastDc.Char = this.MinusSignChar;
				}

				// for psotive, just leave the character empty and when drawing a + will
				// be drawn depending on the SignType property.
				//
				return true;
			}
			else
			{
				if ( this.MinusSignChar == firstDc.Char ||
					this.PlusSignChar == firstDc.Char )
				{
					if ( negative )
						firstDc.Char = this.MinusSignChar;
					else 
						// Clearing the negative sign makes the section positive, so no 
						// need to assign plus character to it.
						//
						firstDc.Char = (char)0;

					return true;
				}
			}

			DisplayCharBase prevDc = firstDc.PrevDisplayChar;

			if ( null == prevDc || this != prevDc.Section )
			{
				// Darn it, the section is full. Where shall we put the sign if all the 
				// displayChars are filled ?
				//
				Debug.Assert( false, "All characters in the number section are filled." );

				return false;
			}

			if ( negative )
				prevDc.Char = this.MinusSignChar;

			return true;
		}

		#region VerifyMinMaxCache

		// SSP 8/16/10 TFS27897 - Optimizations
		// 
		internal void VerifyMinMaxCache( bool verifyMin )
		{
			// SSP 10/7/03 UWE727
			// Get the min value from the owner and use that if that's greater than
			// minValue of the section.
			//
			// --------------------------------------------------------------------------------------
			MaskInfo maskInfo = this.MaskInfo;

			if ( DependencyProperty.UnsetValue != _lastMinValue 
				&& ( verifyMin
					? object.Equals( maskInfo.MinValue, _lastMinValue ) && object.Equals( maskInfo.MinExclusive, _lastMinValueExclusive )
					: object.Equals( maskInfo.MaxValue, _lastMaxValue ) && object.Equals( maskInfo.MaxExclusive, _lastMaxValueExclusive ) ) )
			{
				return;
			}

			_lastMinValue = maskInfo.MinValue;
			_lastMaxValue = maskInfo.MaxValue;
			_lastMinValueExclusive = maskInfo.MinExclusive;
			_lastMaxValueExclusive = maskInfo.MaxExclusive;

			// SSP 4/6/12 TFS95799
			// 
			//decimal resolvedMin = this.GetResolvedMinMaxValueHelper( true, _lastMinValue, _lastMinValueExclusive, out this.isMinValueExclusive );
			//decimal resolvedMax = this.GetResolvedMinMaxValueHelper( false, _lastMaxValue, _lastMaxValueExclusive, out this.isMaxValueExclusive );
			bool isMinDefaultBasedOnSection, isMaxDefaultBasedOnSection;
			decimal resolvedMin = this.GetResolvedMinMaxValueHelper( true, _lastMinValue, _lastMinValueExclusive, out this.isMinValueExclusive, out isMinDefaultBasedOnSection );
			decimal resolvedMax = this.GetResolvedMinMaxValueHelper( false, _lastMaxValue, _lastMaxValueExclusive, out this.isMaxValueExclusive, out isMaxDefaultBasedOnSection );

			decimal truncatedMin = Math.Max( decimal.Truncate( resolvedMin ), this.defaultMinValue );
			decimal truncatedMax = Math.Min( decimal.Truncate( resolvedMax ), this.maxValue );

			// If the min and the max integer portions are the same then constrain the fraction part.
			// 
			FractionPart fractionPart = this.NextEditSection as FractionPart;
			if ( null != fractionPart )
			{
				if ( truncatedMin == truncatedMax )
				{
					fractionPart.MinValue = (double)( Math.Abs( resolvedMin ) - Math.Abs( truncatedMin ) );
					fractionPart.MaxValue = (double)( Math.Abs( resolvedMax ) - Math.Abs( truncatedMax ) );
				}
				else
				{
					fractionPart.ResetMinValue( );
					fractionPart.ResetMaxValue( );
				}
			}

			_lastCalculatedMinValue = truncatedMin;
			_lastCalculatedMaxValue = truncatedMax;

			// SSP 4/6/12 TFS95799
			// 
			//this.lastConvertedMinValWithFractionPart = resolvedMin;
			this.lastConvertedMinValWithFractionPart = !isMinDefaultBasedOnSection ? resolvedMin
				: truncatedMin + ( null != fractionPart ? (decimal)fractionPart.MinValue : 0m );

			this.lastConvertedMaxValWithFractionPart = !isMaxDefaultBasedOnSection ? resolvedMax
				: truncatedMax + ( null != fractionPart ? (decimal)fractionPart.MaxValue : 0m );
		} 

		#endregion // VerifyMinMaxCache

		/// <summary>
		/// min value for this section
		/// </summary>
		// SSP 8/16/02
		// Made the number section support values bigger than int
		//
		//public int MinValue
		public decimal MinValue
		{
			get
			{
				// SSP 8/16/10 TFS27897 - Optimizations
				// Refactored into new VerifyMinMaxCache method.
				// 
				// --------------------------------------------------------------------------------------
				this.VerifyMinMaxCache( true );
				return _lastCalculatedMinValue;
				
#region Infragistics Source Cleanup (Region)












































































#endregion // Infragistics Source Cleanup (Region)

				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			}
			set
			{
				// SSP 8/8/01
				// We need to make sure that the user does not assign a
				// negative value
				if ( value < 0 && 
					// We allow negatives for number sections.
					//
					typeof( NumberSection ) != this.GetType( ) )

					throw new ArgumentOutOfRangeException("MinValue", XamMaskedInput.GetString("LE_ArgumentOutOfRangeException_2"));

				this.minValue = value;
			}
		}

		// SSP 11/19/03 UWE749
		// Added EqualsHelper method.
		//
		private static bool EqualsHelper( object o1, object o2 )
		{
			return o1 == o2 || null != o1 && null != o2 && o1.GetType( ) == o2.GetType( ) && o1.Equals( o2 );
		}

		// SSP 11/19/03 UWE749
		// Added ConvertToDecimalHelper method.
		//
		private object ConvertToDecimalHelper( object val )
		{
			if ( null == val || DBNull.Value == val || val is DateTime )
				return null;

			// JAS 4/15/05 BR03404
			// If the number exceeds the bounds of the Decimal data type,
			// set the value to the min/max value of Decimal.
			//
			if( val is double && (double)val < (double)decimal.MinValue ||
				val is float  && (float) val < (float) decimal.MinValue )
			{
				val = decimal.MinValue;
			}
			else if( val is double && (double)val > (double)decimal.MaxValue ||
					 val is float  && (float) val > (float) decimal.MaxValue )
			{
				val = decimal.MaxValue;
			}

			MaskInfo maskInfo = this.MaskInfo;
			return CoreUtilities.ConvertDataValue( val, typeof( decimal ), maskInfo.FormatProvider, maskInfo.Format );
		}

		/// <summary>
		/// max value for this section
		/// </summary>
		// SSP 8/16/02
		// Made the number section support values bigger than int
		//
		//public int MaxValue
		public decimal MaxValue
		{
			get
			{
				// SSP 8/16/10 TFS27897 - Optimizations
				// Refactored into new VerifyMinMaxCache method.
				// 
				// --------------------------------------------------------------------------------------
				this.VerifyMinMaxCache( false );
				return _lastCalculatedMaxValue;
				
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)

				
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			}
			set
			{
				// SSP 8/8/01
				// We need to make sure that the user does not assign a
				// negative value
				if ( value < 0
					// SSP 9/3/09 TFS18219
					// Number sections do support negative values and therefore 
					// the max should be allowed to be negative.
					//
					&& typeof( NumberSection ) != this.GetType( ) )
					throw new ArgumentOutOfRangeException("MaxValue", XamMaskedInput.GetString("LE_ArgumentOutOfRangeException_3"));

				this.maxValue = value;
			}
		}

		#region NumberOfDigits

		internal int NumberOfDigits
		{
			get
			{
				return this.digits;
			}
		}

		#endregion // NumberOfDigits
			
		// SSP 3/26/02
		// Added this internal virtual methods for spinning
		//





		internal override bool SupportsSpinning
		{
			get
			{
				return true;
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanSpin( bool up )
		{
			string s = this.GetText( InputMaskMode.Raw );

			// We can always increment empty number seciton.
			//
			//	BF 10.6.03	UWE720
			//	We should disallow decrementing empty number sections
			//
			//if ( null == s || 0 == s.Length )
			// SSP 8/6/12 TFS118267
			// Either we should disallow both spin up and down operations or we should allow both.
			// 
			//if ( up && (null == s || 0 == s.Length) )
			if ( null == s || 0 == s.Length )
				return true;

			decimal val;
			try
			{
				// SSP 8/16/02 UWM115
				// Use ToDecimal which uses deciaml rather than int.Parse
				//
                // AS 10/8/08 Optimization - TFS8781
				//val = this.ToDecimal( );
                if (false == this.TryToDecimal(out val))
                    return false;
				//val = int.Parse( s );
			}
			catch ( Exception )
			{
				return false;
			}

            // JDN 10/29/04 SpinWrap - allow spin buttons to wrap value
            if ( this.MaskInfo.MaskedInput.SpinWrap )
                return true;

			if ( up )
			{
				// MRS 12/12/05 - BR07946
				// Added a check for isMaxExclusive
				//return ( 1 + val ) <= this.MaxValue; 
				if (!this.isMaxValueExclusive)
					return ( 1 + val ) <= this.MaxValue; 
				else
					return ( 1 + val ) < this.MaxValue; 
			}
			else
			{
				// MRS 12/12/05 - BR07946
				// Added a check for isMinExclusive
				//return ( val - 1 ) >= this.MinValue;
				if (! this.isMinValueExclusive)
					return ( val - 1 ) >= this.MinValue;
				else
					return ( val - 1 ) > this.MinValue;
			}
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool Spin( bool up, bool setLimit )
		{	
            // JDN 10/29/04 - SpinWrap: support for spin button wrapping
            return this.Spin( up, setLimit, this.MinValue, this.MaxValue );

            // JDN 10/29/04 - SpinWrap: moved implementation to overloaded Spin method
            #region Old Implementation
//			decimal val;
//
//			if ( setLimit )
//			{
//				val = up ? this.MaxValue : this.MinValue;
//			}
//			else
//			{
//				string s = this.GetText( InputMaskMode.Raw );
//
//				// SSP 8/16/02 UWM115
//				// To support numeric values larger than int use decimal instead.
//				// Rewrote the below code.
//				//
//				// -------------------------------------------------------------
//				/*
//				//	BF 7.24.02	UWE81
//				//	If the section is unpopulated, an up click should
//				//	set the value to the minimum
//				//
//				if ( string.Empty.Equals( s ) && up )
//					val = this.MinValue;
//				else
//				{
//					if ( null != s && !string.Empty.Equals( s ) && this.IsInt( s ) )				
//						// SSP 8/16/02
//						//
//						//val = int.Parse( s );
//						val = decimal.Parse( s );
//					else
//						val = up ? ( this.MinValue - 1 ) : ( this.MaxValue + 1 );
//
//					val = up ? ( val + 1 ) : ( val - 1 );
//				}
//				*/
//				if ( null != s && s.Length > 0 && this.IsInt( s ) )
//				{
//					val = this.ToDecimal( );
//
//					val = up ? ( val + 1 ) : ( val - 1 );
//				}
//				else
//				{
//					val = up ? this.MinValue : this.MaxValue;
//				}
//				// -------------------------------------------------------------
//			}
//            
//			// SSP 8/19/02
//			// When we are decrementing we only want to check if the new value
//			// is greater than the min value. And if we are incremeneting
//			// we want to check if the new value does not exceed the max
//			// value. We don't want to check for both otherwise it will not
//			// decrement a number that's bigger than the max value.
//			//
//			/*
//			if ( val >= this.MinValue && val <= this.MaxValue )
//				// SSP 8/16/02 UWM115
//				//
//				//this.SetText( val.ToString( ) );
//				this.SetText( NumberSection.DecimalToString( val ) );
//			*/
//			if ( up )
//			{
//                if ( val <= this.MaxValue )
//					this.SetText( NumberSection.DecimalToString( val ) ); 
//                else if ( val > this.MaxValue && this.MaskedEdit.SpinWrap )
//                    this.SetText( NumberSection.DecimalToString( this.MinValue ) );
//			}
//			else
//			{
//				if ( val >= this.MinValue )
//					this.SetText( NumberSection.DecimalToString( val ) );
//                //JDN Test SpinWrap
//                else if ( val < this.MaxValue && this.MaskedEdit.SpinWrap )
//                    this.SetText( NumberSection.DecimalToString( this.MaxValue ) );
//			}
//		
//			return true;
            #endregion // Old Implementation
		}

        // JDN 10/29/04 - added support for spin button wrapping


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        internal bool Spin(bool up, bool setLimit, decimal minValue, decimal maxValue)
        {
            decimal val;

            if (setLimit)
            {
                val = up ? maxValue : minValue;
            }
            else
            {
                string s = this.GetText(InputMaskMode.Raw);

                if (null != s && s.Length > 0 && this.IsInt(s))
                {
                    val = this.ToDecimal();

                    val = up ? (val + 1) : (val - 1);
                }
                else
                {
                    val = up ? minValue : maxValue;

					// SSP 9/15/11 TFS87104
					// If min is negative and max is positive, for example {number:-50-50}, then
					// start at 0.
					// 
					if ( minValue < 0 && 0 < maxValue )
						val = 0;
                }
            }

            // MRS 7/7/06 - BR14098
            // call the new overload.
            #region Old Code
            //if ( up )
            //{
            //    if ( val <= maxValue )
            //        this.SetText( NumberSection.DecimalToString( val ) ); 
            //    else if ( val > maxValue && this.MaskedEdit.SpinWrap )
            //        this.SetText( NumberSection.DecimalToString( minValue ) );
            //}
            //else
            //{
            //    if ( val >= minValue )
            //        this.SetText( NumberSection.DecimalToString( val ) );
            //     else if ( val < minValue && this.MaskedEdit.SpinWrap )
            //        this.SetText( NumberSection.DecimalToString( maxValue ) );
            //}

            //return true;
            #endregion Old Code

            return this.Spin(up, setLimit, val, minValue, maxValue);
        }

        // MRS 7/7/06 - BR14098
        // Added an overload that takes the value. The YearSection needs to call this overload from
        // it's Spin method becuse the year needs to be formatted into a 4-digit year for the purposes of
        // determining if it is within the Min/Max values. 


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        internal bool Spin(bool up, bool setLimit, decimal val, decimal minValue, decimal maxValue)
        {
            if (up)
            {
                if (val <= maxValue)
                    this.SetText(NumberSection.DecimalToString(val));
                else if (val > maxValue && this.MaskedEdit.SpinWrap)
                    this.SetText(NumberSection.DecimalToString(minValue));
            }
            else
            {
                if (val >= minValue)
                    this.SetText(NumberSection.DecimalToString(val));
                else if (val < minValue && this.MaskedEdit.SpinWrap)
                    this.SetText(NumberSection.DecimalToString(maxValue));
            }

            return true;
        }





#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// MRS 9/1/04 - UWE1041
		// This was limiting the MaskEdit to 9 digits after the decimal. Changed
		// it to Int64 to gives us 19 digits. 
		//internal static string PadNumber( int n, int totalWidthOfResultingString )
		internal static string PadNumber( System.Int64 n, int totalWidthOfResultingString )
		{
			string ns = n.ToString( );

			// SSP 2/10/04 UWE764
			// If the string is already big enough then return it.
			//
			// ------------------------------------------------------------------------
			if ( ns.Length >= totalWidthOfResultingString )
				return ns;
			// ------------------------------------------------------------------------

			return new String( '0', totalWidthOfResultingString - ns.Length ) + ns;
		}







		internal override bool IsInputFull( )
		{							
			// SSP 7/11/02 UWM105
			// Always leave the first display character for signs.
			// Added below if statement.
			//
			if ( SignDisplayType.None != this.SignType && this.DisplayChars.Count > 1 )
			{
				DisplayCharBase dc = this.FirstDisplayChar.NextDisplayChar;

				if ( null != dc && !dc.IsEmpty && '0' != dc.Char &&
					 this.PlusSignChar != dc.Char && this.MinusSignChar != dc.Char )
					return true;
			}

			if ( !this.FirstDisplayChar.IsEmpty && '0' != this.FirstDisplayChar.Char )
				return true;
			
			return false;
		}

		// SSP 3/15/12 TFS98213
		// 
		/// <summary>
		/// Returns true if the only character filled in the number section is either '+' or '-';
		/// </summary>
		internal bool HasOnlySignSymbol
		{
			get
			{
				string str = this.GetText( InputMaskMode.Raw );
				if ( 1 == str.Length )
				{
					char c = str[0];
					if ( this.PlusSignChar == c || this.MinusSignChar == c )
						return true;
				}

				return false;
			}
		}

		// SSP 2/18/04
		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			// since this is a number, 
			int textIndex = text.Length - 1;

			// First erase all the characters.
			//
			this.EraseAllChars( );

			char commaChar = this.CommaChar;

            
            
            
            DisplayCharsCollection displayChars = this.DisplayChars;

			for ( int i = displayChars.Count - 1; i >= 0; i-- )
			{
				DisplayCharBase dc = displayChars[ i ];

				if ( !dc.IsEditable )
					continue;

				// SSP 10/5/01				
				// skip commas
				//
				while ( textIndex >= 0 && commaChar == text[ textIndex ] )
					textIndex--;

				char c = (char)0;
				if ( textIndex >= 0 )
					c = text[ textIndex-- ];

				dc.Char = c;
			}
		}

		// SSP 2/29/12 TFS92791
		// 
		internal void SetText( string text, bool autoExpand )
		{
			if ( autoExpand && text.Length > 0 && text.Length > this.DisplayChars.Count - 1 )
			{
				char firstChar = text[0];
				bool isFirstCharPlusOrMinus = !char.IsDigit( firstChar )
					&& ( this.PlusSignChar == firstChar || this.MinusSignChar == firstChar );

				int digitsRequired = text.Length - ( isFirstCharPlusOrMinus ? 1 : 0 );

				DisplayCharsCollection dcColl = this.DisplayChars;

				bool allowNegative = this.MinValue < 0m;
				int placeHolderCount = dcColl.Count - ( allowNegative ? 1 : 0 );

				if ( digitsRequired > placeHolderCount )
				{
					IFormatProvider formatProvider = this.MaskInfo.FormatProvider;

					SectionsCollection sections;
					MaskParser.Parse( XamMaskedInput.CalcDefaultDoubleMask( formatProvider, digitsRequired, 0, '-' ), out sections, formatProvider );
					NumberSection newSection = null != sections && sections.Count > 0 ? sections[0] as NumberSection : null;
					if ( null != newSection )
					{
						if ( null != this.Sections )
							this.Sections._isCacheInvalid = true;

						this.DisplayChars.Clear( );
						this.InternalInitialize( digitsRequired, newSection.commaPositions, this.signType );
					}
				}
			}

			this.SetText( text );
		}

		// SSP 8/16/02 UWM115
		// Changed from int to decimal in order to support numbers larger than
		// what an integer can hold.
		//
		internal int ToInt( )
		{
			return (int)this.ToDecimal( );
		}

		internal decimal ToDecimal( )
		{
			decimal val;

			if ( !this.ParseToInt( out val ) )
			{
				throw new FormatException(XamMaskedInput.GetString("LE_Exception_72"));
			}

			return val;
		}

        // AS 10/8/08 Optimization - TFS8781
        // Avoid throwing exceptions when possible.
        //
        internal bool TryToDecimal(out decimal value)
        {
            return this.ParseToInt(out value);
        }

        internal bool TryToInt(out int value)
        {
            decimal d;

            if (this.ParseToInt(out d))
            {
                value = (int)d;
                return true;
            }

            value = 0;
            return false;
        }

		internal virtual bool ParseToInt( out decimal val )
		{
			val = 0;

			string s = this.GetText( InputMaskMode.Raw );

            // AS 10/8/08 Optimization - TFS8781
			//if ( null == s || !this.IsInt( s ) )
			if ( string.IsNullOrEmpty(s) || !this.IsInt( s ) )
				return false;

            
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

            return decimal.TryParse(s, out val);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static string DecimalToString( decimal val )
		{
			return val.ToString( );
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static decimal StringToDecimal( string val )
		{
			return decimal.Parse( val );
		}

		internal override string GetText( InputMaskMode maskMode )
		{
			if ( null == this.DisplayChars || this.DisplayChars.Count <= 0 )
			{
				Debug.Assert( false, "null or empty collection returned by MaskedEdit.DisplayChars property" );
				return null;
			}

			int count = this.DisplayChars.Count;


			StringBuilder sb = new StringBuilder( string.Empty, 1 + this.digits );

			for ( int i = 0; i < count; i++ )
			{
				DigitChar dc = this.DisplayChars[ i ] as DigitChar;

				Debug.Assert( null != dc, 
					"display chars collection in number section encountered" +
					"a display char instance that is other than a DigitChar" );

				if ( null != dc && (char)0 != dc.Char )
				{
					sb.Append( dc.Char );

					// SSP 10/5/01
					//
					//if ( this.HasCommas && InputMaskMode.IncludeLiterals == maskMode )
					if ( dc.ShouldIncludeComma( maskMode ) )
					{
						//if ( 0 == ( ( count - 1 ) % 3 ) )
						//{
						char comma = this.CommaChar;
					
							//sb.Append( comma );

						if ( 0 != comma )
							sb.Append( comma );

						//}
					}
				}

			}


			return sb.ToString( );
		}




#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanShiftRight( int from, int positionsToShift )
		{
			throw new InvalidOperationException(XamMaskedInput.GetString("LE_InvalidOperationException_4"));
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanShiftLeft( int from, int positionsToShift )
		{
			throw new InvalidOperationException(XamMaskedInput.GetString("LE_InvalidOperationException_5"));
		}
	


		/// <summary>
		///Overridden. Throws an InvalidOperationException exception
		/// since this function is invalid for a number section 
		/// </summary>
		/// <param name="from"></param>
		/// <param name="positionsToShift"></param>
		/// <returns></returns>
		internal override bool ShiftLeft( int from, int positionsToShift )
		{
			throw new InvalidOperationException(XamMaskedInput.GetString("LE_InvalidOperationException_6"));
		}




#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool ShiftRight( int from, int positionsToShift )
		{
			throw new InvalidOperationException( XamMaskedInput.GetString( "LE_InvalidOperationException_7" ) );
		}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool DeleteCharWithNoShift( int pos )
		{
			// can't delete a character without shifting
			throw new InvalidOperationException(XamMaskedInput.GetString("LE_InvalidOperationException_8"));
		}

		internal bool IgnoreStrictSignChecking
		{
			get
			{
				return this.ignoreStrictSignChecking;
			}
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool DeleteCharsAndShift( int pos, int count )
		{
			Debug.Assert( pos >= 0 && pos <= this.DisplayChars.Count, 
				"attempt to delete character at an invalid position" );
			
			if ( pos < 0 || pos >= this.DisplayChars.Count )
				return false;

			this.ignoreStrictSignChecking = true;

			try
			{	
				for ( int i = pos; i >= 0; i-- )
				{            
					DisplayCharBase dc1 = this.SafeDisplayCharAt( i );
					DisplayCharBase dc2 = this.SafeDisplayCharAt( i - count );

					Debug.Assert( null != dc1 );

				

					// in case we encounter a literal, we have to get a non-literal
					// character before it

					if ( null != dc1 && !dc1.IsEditable )
						continue;

					if ( null != dc2 && !dc2.IsEditable )
					{
						int tmp = count + 1;
						while ( null != dc2 && !dc2.IsEditable )
						{
							dc2 = this.SafeDisplayCharAt( i - tmp );
							tmp++;
						}
					}

					if ( null != dc1 )
						dc1.Char = null != dc2 ? dc2.Char : (char)0;
				}
			}
			finally
			{
				this.ignoreStrictSignChecking = false;
			}

			return true;            			
		}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool DeleteCharAndShift( int pos )
		{
			return this.DeleteCharsAndShift( pos, 1 );	
		}
		


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// SSP 5/12/09 TFS17499
		// Specified character could be invalid digit char that causes the decimal.Parse to
		// fail. In which case we need to return a value indicating that the character 
		// can not be inserted.
		// 
		//private decimal GetIntValueIfInserted( int pos, char c )
		private bool GetIntValueIfInserted( int pos, char c, out decimal newVal )
		{
			int count = this.DisplayChars.Count;

			// AS 1/24/06
			//string s = string.Empty;
			StringBuilder sb = new StringBuilder();

			int i = 0;
			for ( ; i < count; i++ )
			{
				DisplayCharBase dc = this.DisplayChars[ i ];

				if ( ! ( dc is DigitChar ) )
					break;

				// SSP 8/29/02 UWM139
				// Moved this from below the below if statement.
				//
				if ( 0 != dc.Char )
					
					//s += dc.Char;
					sb.Append(dc.Char);
				
				if ( i == pos && 0 != c )
					// SSP 1/7/02 
					// I think this was a typo.
					//s += dc.Char;
					
					//s += c;
					sb.Append(c);

				// SSP 8/29/02 UWM139
				// Moved this before above if statement.
				//
				//if ( 0 != dc.Char )
				//	s += dc.Char;
			}

			if ( i == pos && 0 != c )
			{
				// SSP 1/7/01 
				// I think this was a typo.
				//s += 'c';
				
				//s += c;
				sb.Append(c);
			}
			
			// SSP 8/16/02 UWM115
			// Use decimal rather than int to allow for numeric values greater than int.
			//
			//return int.Parse( s );
			// SSP 8/29/02 UWM139
			// Convert the string s to the decimal rather than what's in the section.
			//
			//decimal val;
			//this.ParseToInt( out val );
			
			//decimal val = NumberSection.StringToDecimal( s );
			// SSP 5/12/09 TFS17499
			// 
			// ------------------------------------------------------------------------
			//decimal val = NumberSection.StringToDecimal( sb.ToString() );
			//return val;
			newVal = 0;
			return decimal.TryParse( sb.ToString( ), out newVal );
			// ------------------------------------------------------------------------
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanInsertCharAt( int pos, char c )
		{			
			Debug.Assert( pos >= 0 && pos < this.DisplayChars.Count, 
				"attempt to replace character at an invalid position" );
			// SSP 4/8/05 BR03077
			//
			//if ( pos < 0 || pos > this.DisplayChars.Count )
			if ( pos < 0 || pos >= this.DisplayChars.Count )
				return false;

			DisplayCharBase dc = this.SafeDisplayCharAt( pos );

			Debug.Assert( null != dc, "null dc at valid index" );

			// can't insert a 0
			if ( 0 == (int)c ) 
				return false;
			
			// can only insert a matching char
			if ( !dc.MatchChar( c ) )
				return false;

			if ( this.IsInputFull( ) )
				return false;

			// SSP 8/16/02 UWM115
			// Moved this from below to here because we were prematurely returning
			// true without testing this condition.
			//
			if ( Char.IsDigit( c ) )
			{
				// SSP 5/12/09 TFS17499
				// Even though we are checking IsDigit above, certain unicode characters 
				// are apparently considered digits by it however decimal.Parse throws an 
				// exception when parsing a string containing those.
				// 
				//decimal v = this.GetIntValueIfInserted( pos, c );
				decimal v;
				if ( !this.GetIntValueIfInserted( pos, c, out v ) )
					return false;

				// if that would lead to out of range value in the section
				// SSP 9/23/02 UWM151
				// Adding a digit anywhere in the number section will never
				// make its value decrease. So don't check for value entered
				// being less than min value. The problem is that for example
				// when a section's min value is 1 or more, the user won't be
				// able to enter for example a 0 even though he plans on
				// entering some other digit later on and making the value 
				// bigger. If the MinValue was over 10, then the user
				// won't be able to enter anything in the section at all when
				// the section is empty because no matter what digit is entered, 
				// the the new value of the section will never be greater than
				// 10 and below condition will always be met and this method
				// will always return true.
				//
				//if ( v < this.MinValue || v > this.MaxValue )

				// JAS 1/7/05 BR01514 - If the value is negative then we need to validate against the min value.
//				if ( v > this.MaxValue )
//					return false;				
				if( v >= 0 )
				{
					if ( v > this.MaxValue )
						return false;
				}
				else
				{
					if( v < this.MinValue )
						return false;
				}
			}

			if ( this.FirstDisplayChar.IsEmpty || dc.IsEmpty )
				return true;

			if ( '0' == this.FirstDisplayChar.Char && this.GetType( ) == typeof( NumberSection ) )
				return true;

			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		

			// in a number section, since every DisplayChar is of the same type,
			// it's safe to match one and return true

			return false;
		}




#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool InsertCharAt( int pos, char c )
		{

			Debug.Assert( pos >= 0 && pos < this.DisplayChars.Count, 
				"attempt to replace character at an invalid position" );
			if ( pos < 0 || pos >= this.DisplayChars.Count )
				return false;

			DisplayCharBase dc = this.SafeDisplayCharAt( pos );

			Debug.Assert( null != dc, "null dc at valid index" );

			if ( !this.CanInsertCharAt( pos, c ) )
				return false;


			for ( int i = 0; i < pos; i++ )
			{            
				DisplayCharBase dc1 = this.SafeDisplayCharAt( i );
				DisplayCharBase dc2 = this.SafeDisplayCharAt( i + 1 );

				Debug.Assert( null != dc1 && null != dc2 );


				
				// in case we encounter a literal, we have to get a non-literal
				// character before it

				if ( !dc1.IsEditable )
					continue;

				if ( !dc2.IsEditable )
				{
					int tmp = 2;
					while ( null != dc2 && !dc2.IsEditable )
					{
						dc2 = this.SafeDisplayCharAt( i + tmp );
						tmp++;
					}
				}



				dc1.Char = dc2.Char;
			}
		

			dc.Char = c;

			return true;
		}

		internal static string StripOutPadPromptCharacters( string str, SectionBase section )
		{
			Debug.Assert( null != section.MaskInfo );
			char padChar = null != section.Sections.MaskInfo ? section.Sections.MaskInfo.PadChar : ' ';
			char promptChar = null != section.Sections.MaskInfo ? section.Sections.MaskInfo.PromptChar : '_';

			StringBuilder sb = new StringBuilder( str.Length );

			bool flag = false;

			for ( int i = 0; i < str.Length; i++ )
			{
				if ( str[i] != padChar && str[i] != promptChar )
					sb.Append( str[i] );
				else 
					flag = true;
			}

			return flag ? sb.ToString( ) : str;
		}

		/// <summary>
		/// Returns true if string str matches the mask associated with this section
		/// </summary>
		/// <param name="str">string to validate</param>
		/// <returns><b>true</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateString( ref string str )
		{
			// SSP 10/5/01
			// Make sure commas are not included when validating a string
			// even thought the number section does not have any commas set
			// for it.
			//
			StringBuilder sb = new StringBuilder( str.Length );

			char comma = this.CommaChar;

			// SSP 9/12/03 - Ink Provider Related changes
			// Check for prompt and pad characters.
			//
			// ----------------------------------------------------------------------------------------
			str = NumberSection.StripOutPadPromptCharacters( str, this );
			// ----------------------------------------------------------------------------------------

			for ( int i = 0; i < str.Length; i++ )
			{
				// skip commas
				//
				if ( comma != str[i] )
					sb.Append( str[i] );
			}

			if ( this.IsInt( sb.ToString( ) ) )
			{
				// SSP 8/22/03 - Ink Provider Related changes
				// Also check if the min and max constraints are met.
				//
				// --------------------------------------------------------------------
				try
				{
                    // AS 10/8/08 Optimization - TFS8781
					//decimal val = NumberSection.StringToDecimal( sb.ToString( ) );
                    decimal val;
                    if (false == decimal.TryParse(sb.ToString(), out val))
                        return false;

					if ( val < this.MinValue || val > this.MaxValue )
						return false;
				}
				catch ( Exception )
				{
					return false;
				}
				// --------------------------------------------------------------------
				
				return true;
			}

			return false;
		}

		
		
		/// <summary>
		/// if the input in this section matches the mask.
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not modifications are allowed</param>
		/// <returns><b>true</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{
			string str = null;

			// SSP 5/8/02
			// We have a special fraction section for fractions. So we don't need this 
			// anymore.
			//
			
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)



			// SSP 10/5/01
			// Use InputMaskMode.Raw so that it strips out the commas
			//
			//string str = this.GetText( InputMaskMode.IncludeLiterals );
			str = this.GetText( InputMaskMode.Raw );
		
			if ( this.IsInt( str ) )
			{
				// SSP 9/1/09 TFS18219
				// 
				string tmp;
				if ( ! this.ValidateAgainstMinMaxHelper( out tmp ) )
					return false;

				// SSP 3/13/02
				// Added contentModificationsAllowed parameter
				//						
				if ( contentModificationsAllowed )
					this.SetText( str );

				return true;
			}
			
			return false;
		}

		// SSP 9/1/09 TFS18219
		// Added ValidateAgainstMinMaxHelper method.
		// 
		internal bool ValidateAgainstMinMaxHelper( out string error )
		{
			error = null;

			decimal val;
			if ( !this.ParseToInt( out val ) )
				val = 0;

			if ( val < this.minValue )
			{
				error = XamMaskedInput.GetString("LMSG_ValueConstraint_MinInclusive", this.minValue);
				return false;
			}

			if ( val > this.maxValue )
			{
				error = XamMaskedInput.GetString("LMSG_ValueConstraint_MaxInclusive", this.maxValue);
				return false;
			}

			return true;
		}

		/// <summary>
		/// overridden. returns RightToLeft to indicate that this section
		/// is to be edited right-to-left
		/// </summary>
		public override EditOrientation Orientation 
		{ 
			get
			{
				return EditOrientation.RightToLeft;
			}
		}

		
		/// <summary>
		/// Creates display chars associated with this edit section to the passed
		/// in displayChars collection.
		/// </summary>
		
		
		
		
		
		//protected virtual void CreateDisplayChars( )
		protected void CreateDisplayChars()
		{
			DigitChar dc = null;

			// SSP 7/11/02 UWM105
			// If we are going to have a sign, then reserve an extra place for signs.
			//
			int charPlacesToCreate = this.digits;

			if ( SignDisplayType.None != signType )
				charPlacesToCreate++;			

			for ( int i = 0; i < charPlacesToCreate; i++ )
			{
				dc = new DigitChar( this );

				this.DisplayChars.Add( dc );

				// Tell the display char that it's at a position
				// in the number section that has comma
				//
				// SSP 9/20/02 UWM149
				// If we are incrementing charPlacesToCreate by 1 because we have
				// a sign, then we have to compensate for it here as comma indexes 
				// would need to be shifted 1 right
				// 
				//if ( null != this.commaPositions &&					
				//	-1 != Array.IndexOf( this.commaPositions, i ) )
				if ( null != this.commaPositions &&					
					-1 != Array.IndexOf( this.commaPositions, SignDisplayType.None != signType ? i - 1 : i ) )
				{
					dc.HasComma = true;
				}
			}
		}







		internal override void InternalCopy( SectionBase dest )
		{
			if ( this.GetType( ) != dest.GetType( ) )
				throw new ArgumentException(XamMaskedInput.GetString("LE_ArgumentException_2"), "dest");

			base.InternalCopy( dest );

			// MD 7/11/06 - Fixed assignment to same variable
			// Moved below
			//this.digits = this.digits;

			NumberSection destSection = (NumberSection)dest;
			if ( null != this.commaPositions )
			{
				destSection.commaPositions = new int[this.commaPositions.Length];
				this.commaPositions.CopyTo( destSection.commaPositions, 0 );
			}

			// MD 7/11/06 - Fixed assignment to same variable
			// Moved from above
			destSection.digits = this.digits;

			destSection.maxValue = this.maxValue;
			destSection.minValue = this.minValue;
			destSection.defaultMinValue = this.defaultMinValue;

			// SSP 4/8/05 BR03077 
			//
			destSection.signType = this.signType;
		}







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			NumberSection clone = new NumberSection( this.digits, this.commaPositions, this.signType );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}

		// MRS 12/12/05 - BR07946
		#region GetResolvedMinValueAndSetIsMinExclusive







		// SSP 8/16/10 TFS27897 - Optimizations
		// Replaced GetResolvedMinValueAndSetIsMinExclusive. Original code is commented out.
		// 
		private decimal GetResolvedMinMaxValueHelper( bool calculatingMin, object val, object exclusive, out bool isExclusive
			// SSP 4/6/12 TFS95799
			// 
			, out bool isDefaultBasedOnSection )
		{
			object dVal = ConvertToDecimalHelper( val );
			object dExclusive = ConvertToDecimalHelper( exclusive );

			object retVal = dVal;
			isExclusive = false;

			if ( null != dExclusive )
			{
				if ( null == dVal || ( calculatingMin ? (decimal)dExclusive >= (decimal)dVal : (decimal)dExclusive <= (decimal)dVal ) )
				{
					retVal = dExclusive;
					isExclusive = true;
				}
			}

			// SSP 4/6/12 TFS95799
			// 
			isDefaultBasedOnSection = null == retVal;

			return null != retVal ? (decimal)retVal : ( calculatingMin ? this.minValue : this.maxValue );
		}
		
#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)


		#endregion GetResolvedMinValueAndSetIsMinExclusive

		// MRS 12/12/05 - BR07946
		#region GetResolvedMaxValueAndSetIsMaxExclusive

		// SSP 8/16/10 TFS27897 - Optimizations
		// Replaced GetResolvedMinValueAndSetIsMinExclusive. Original code is commented out.
		// 
		
#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)

		#endregion GetResolvedMaxValueAndSetIsMaxExclusive

	}

	#endregion //NumberSection

	#region MonthSection

	/// <summary>
	/// Month section part of a date mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// MonthSection represents the month portion of a date mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class MonthSection : NumberSection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MonthSection"/> class
		/// </summary>
		public MonthSection( )
			: base( 2 )
		{
			this.MinValue = 1;
			this.MaxValue = 12;
		}

		/// <summary>
		/// ValidateString
		/// </summary>
		/// <param name="str">string to validate</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateString( ref string str )
		{
			// SSP 9/12/03 - Ink Provider Related changes
			// Check for prompt and pad characters.
			//
			// ----------------------------------------------------------------------------------------
			str = NumberSection.StripOutPadPromptCharacters( str, this );
			// ----------------------------------------------------------------------------------------

			int m = -1;
			
			try
			{
                // AS 10/8/08 Optimization - TFS8781
                //m = int.Parse( str, NumberStyles.None );
                if (false == int.TryParse(str, NumberStyles.None, NumberFormatInfo.CurrentInfo, out m))
                    return false;
			}
			catch ( Exception )
			{
				return false;
			}

            // AS 8/25/08 Support Calendar
            //if ( m >= 1 && m <= 12 )
			if ( m >= 1 && m <= this.MaxValue )
			{
				str = m.ToString();
				return true;
			}

			return false;
		}

		/// <summary>
		/// ValidateSection
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{
			if ( !base.ValidateSection( contentModificationsAllowed ) )
				return false;

			decimal val;

			if ( !this.ParseToInt( out val ) )
				return false;

            // AS 8/25/08 Support Calendar
            //if (val < 1 || val > 12)
            if (val < 1 || val > this.MaxValue)
				return false;

			return true;
		}

		// SSP 2/18/04
		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			base.SetText( text.PadLeft( this.NumberOfDigits, '0' ) );
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanInsertCharAt( int pos, char c )
		{

			if ( !base.CanInsertCharAt( pos, c ) )
				return false;

			if ( 0 == pos )
			{
                // AS 8/25/08 Support Calendar
                //if (this.DisplayChars[1].Char > '2' && c > '0')
                if (this.DisplayChars[1].Char > this.MaxTrailingChar && c > '0')
					return false;

				// SSP 1/17/02
				//
				//if ( (char)0 != this.DisplayChars[1].Char && c > '2' )
				if ( (char)0 != this.DisplayChars[1].Char && c >= '2' )
					return false;
			}

			if ( 1 == pos )
			{
				if ( this.DisplayChars[1].Char > '1' )
					return false;

                // AS 8/25/08 Support Calendar
				//if ( '1' == this.DisplayChars[1].Char && c > '2' )
                if ( '1' == this.DisplayChars[1].Char && c > this.MaxTrailingChar )
                    return false;
            }

			return true;
		}

        // AS 8/25/08 Support Calendar
        private char MaxTrailingChar
        {
            get
            {
                Debug.Assert(this.MaxValue == 12 || this.MaxValue == 13);
                if (this.MaxValue == 13)
                    return '3';
                else
                    return '2';
            }
        }

        // JDN 10/29/04 - SpinWrap: support for spin button wrapping



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal override bool Spin( bool up, bool setLimit ) 
        {			
            string s = this.GetText( InputMaskMode.Raw );

            // AS 8/25/08 Support Calendar
            System.Globalization.Calendar calendar = this.Calendar;

            // If the month is not set, set it to the current month.
            if ( null == s || s.Length == 0 || !this.IsInt( s ) )
            {
                try
                {
                    // AS 8/25/08 Support Calendar
                    //this.SetText( DateTime.Now.Month.ToString( ) );
                    this.SetText( calendar.GetMonth(DateTime.Now).ToString( ) );
                    return true;
                }
                catch ( Exception ) 
                {
                }
            }

            // retrieve min and max spin values
            decimal minMonthValue = this.MinValue;
            decimal maxMonthValue = this.MaxValue;
            int minDateYear = 0;
            int maxDateYear = 9999;

            object minValue = this.MaskInfo.MinValue;
            if ( null != minValue )
            {
                    if ( minValue is DateTime )
                    {
                        DateTime minDateTime    = (DateTime)minValue;
                        // AS 8/25/08 Support Calendar
                        //minMonthValue           = minDateTime.Month;
                        //minDateYear             = minDateTime.Year;
                        minMonthValue           = calendar.GetMonth(minDateTime);
                        minDateYear             = calendar.GetYear(minDateTime);
                    }
            }

			object maxValue = this.MaskInfo.MaxValue;
            if ( null != maxValue )
            {
                    if ( maxValue is DateTime )
                    {
                        DateTime maxDateTime    = (DateTime)maxValue;
                        // AS 8/25/08 Support Calendar
                        //maxMonthValue           = maxDateTime.Month;
                        //maxDateYear             = maxDateTime.Year;
                        maxMonthValue           = calendar.GetMonth(maxDateTime);
                        maxDateYear             = calendar.GetYear(maxDateTime);
                    }
            }

            // if the min and max years are the same restrict spinning to valid months
            if ( minDateYear == maxDateYear )
                return base.Spin( up, setLimit, minMonthValue, maxMonthValue );
            else
                return base.Spin( up, setLimit, this.MinValue, this.MaxValue );
        }









		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			MonthSection clone = new MonthSection( );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}

        // AS 8/25/08 Support Calendar
        internal static void InitializeMinMax(MonthSection section, System.Globalization.Calendar calendar)
        {
            if (

				calendar is EastAsianLunisolarCalendar ||

                calendar is HebrewCalendar)
                section.MaxValue = 13;
        }
    }


	#endregion //MonthSection

	#region DaySection

	/// <summary>
	/// Day section part of a date mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// DaySection represents the day portion of a date mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class DaySection : NumberSection
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="DaySection"/> class
		/// </summary>
		public DaySection( )
			: base( 2 )
		{
			this.MinValue = 1;
			this.MaxValue = 31;
		}

		/// <summary>
		/// ValidateString
		/// </summary>
		/// <param name="str">string to validate</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateString( ref string str )
		{			
			// SSP 9/12/03 - Ink Provider Related changes
			// Check for prompt and pad characters.
			//
			// ----------------------------------------------------------------------------------------
			str = NumberSection.StripOutPadPromptCharacters( str, this );
			// ----------------------------------------------------------------------------------------

			int d = -1;
			try
			{
                // AS 10/8/08 Optimization - TFS8781
                //d = int.Parse( str, NumberStyles.None );
                if (false == int.TryParse(str, NumberStyles.None, NumberFormatInfo.CurrentInfo, out d))
                    return false;
			}
			catch ( Exception )
			{
				return false;
			}

			if ( d >= 1 && d <= 31 )
			{
				str = d.ToString();
				return true;
			}

			return false;
		}

		// SSP 2/18/04
		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			base.SetText( text.PadLeft( this.NumberOfDigits, '0' ) );
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanInsertCharAt( int pos, char c )
		{

			if ( !base.CanInsertCharAt( pos, c ) )
				return false;

			if ( 0 == pos )
			{
				if ( (char)0 != this.DisplayChars[1].Char )
				{
					if ( this.DisplayChars[1].Char <= '1' && c > '3' )
						return false;

					if ( this.DisplayChars[1].Char > '1' && c > '2' )
						return false;
				}					
			}

			if ( 1 == pos )
			{
				if ( this.DisplayChars[1].Char > '3' )
					return false;

				if ( this.DisplayChars[1].Char > '2' && c > '1' )
					return false;
			}

			return true;
		}

		

		/// <summary>
		/// ValidateSection
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{	
			if ( !base.ValidateSection( contentModificationsAllowed ) )
				return false;

			decimal val;

			if ( !this.ParseToInt( out val ) )
				return false;

			if ( val < 1 || val > 31 )
				return false;

			return true;
		}

        // JDN 10/29/04 - SpinWrap: support for spin button wrapping


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal override bool Spin( bool up, bool setLimit ) 
        {			
            string s = this.GetText( InputMaskMode.Raw );

            // AS 8/25/08 Support Calendar
            System.Globalization.Calendar calendar = this.Calendar;

            // If the day is not set, set it to the current day.
            if ( null == s || s.Length == 0 || !this.IsInt( s ) )
            {
                try
                {
                    // AS 8/25/08 Support Calendar
                    //this.SetText( DateTime.Now.Day.ToString( ) );
                    this.SetText( calendar.GetDayOfMonth(DateTime.Now).ToString( ) );
                    return true;
                }
                catch ( Exception ) 
                {
                }
            }

            // retrieve min and max spin values
            decimal minDayValue = this.MinValue;
            decimal maxDayValue = this.MaxValue;
            int minDateMonth    = 1;
            int maxDateMonth    = 12;
            int minDateYear     = 0;
            int maxDateYear     = 9999;

            object minValue = this.MaskInfo.MinValue;
            minValue = this.MaskInfo.ValueToDataValue( minValue );
            if ( null != minValue )
            {
                if ( minValue is DateTime )
                {
                    DateTime minDateTime = (DateTime)minValue;
                    // AS 8/25/08 Support Calendar
                    //minDayValue     = minDateTime.Day;
                    //minDateMonth    = minDateTime.Month;
                    //minDateYear     = minDateTime.Year;
                    minDayValue     = calendar.GetDayOfMonth(minDateTime);
                    minDateMonth    = calendar.GetMonth(minDateTime);
                    minDateYear     = calendar.GetYear(minDateTime);
                }
            }

            object maxValue = this.MaskInfo.MaxValue;
            maxValue = this.MaskInfo.ValueToDataValue( maxValue );
            if ( null != maxValue )
            {
                if ( maxValue is DateTime )
                {
                    DateTime maxDateTime = (DateTime)maxValue;
                    // AS 8/25/08 Support Calendar
                    //maxDayValue     = maxDateTime.Day;
                    //maxDateMonth    = maxDateTime.Month;
                    //maxDateYear     = maxDateTime.Year;
                    maxDayValue     = calendar.GetDayOfMonth(maxDateTime);
                    maxDateMonth    = calendar.GetMonth(maxDateTime);
                    maxDateYear     = calendar.GetYear(maxDateTime);
                }
            }

            // if the min and max years and months are the same restrict spinning to valid days
            if ( (minDateYear == maxDateYear) && (minDateMonth ==  maxDateMonth) )
                return base.Spin( up, setLimit, minDayValue, maxDayValue );
            else
                return base.Spin( up, setLimit, this.MinValue, this.MaxValue );
        }







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			DaySection clone = new DaySection( );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}
	}


	#endregion //DaySection

	#region YearSection

	/// <summary>
	/// Year section of a date mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// YearSection represents the year portion of a date mask. The section can be two digits year section
	/// or four digits year section.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class YearSection : NumberSection
	{
		private bool isFourDigits = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="YearSection"/> class
		/// </summary>
		/// <param name="isFourDigits">indicates if year section is 4 digits or 2 digits</param>
		public YearSection( bool isFourDigits )
			: base( isFourDigits ? 4 : 2 )
		{
			this.isFourDigits = isFourDigits;

			// SSP 10/17/01
			// Set the min and max value of the year section
			//
			this.MinValue = 0;
			this.MaxValue = 9999;
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal int GetYear( )
		{
			int year = this.ToInt( );

            
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

            return this.GetYear(year);
		}

        // AS 10/8/08 Optimization - TFS8781
        internal bool TryGetYear(out int year)
        {
            if (false == this.TryToInt(out year))
                return false;

            year = this.GetYear(year);
            return true;
        }

        // AS 10/8/08 Optimization - TFS8781
        private int GetYear(int year)
        {
            // AS 8/25/08 Support Calendar
            System.Globalization.Calendar calendar = this.Calendar;

            if (this.isFourDigits)
            {
                if (0 == this.DisplayChars[0].Char && 0 == this.DisplayChars[1].Char)
                    // AS 8/25/08 Support Calendar
                    //year = XamMaskedInput.ConvertToFourYear(year);
                    year = XamMaskedInput.ConvertToFourYear(year, calendar);
            }
            else
            {
                // AS 8/25/08 Support Calendar
                //year = XamMaskedInput.ConvertToFourYear(year);
                year = XamMaskedInput.ConvertToFourYear(year, calendar);
            }

            return year;
        }

		// SSP 11/13/09 TFS24728
		// 
		internal override bool ParseToInt( out decimal val )
		{
			if ( base.ParseToInt( out val ) )
			{
				DisplayCharsCollection displayChars = this.DisplayChars;
				bool convertToFourYears = !isFourDigits
					|| 0 == displayChars[0].Char && 0 == displayChars[1].Char;

				if ( convertToFourYears )
					val = XamMaskedInput.ConvertToFourYear( (int)val, this.Calendar );

				return true;
			}

			return false;
		}

		/// <summary>
		/// ValidateSection
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{
			// SSP 8/28/02
			// Moved this from below.
			//
			// SSP 10/14/02 UWM81
			// Pass in false for contentModificationsAllowed
			// 
			//if ( !base.ValidateSection( contentModificationsAllowed ) )
			if ( !base.ValidateSection( false ) )
				return false;


			// If the entered year is 2 digit, then convert it to 4 digit
			// year if this year section is a four digit year section instead
			// of just padding it with 0.
			//
			if ( contentModificationsAllowed )
			{
				try
				{
					
					// SSP 8/7/02
					// Check for the year section being four digits.
					//
					// --------------------------------------------------
					// SSP 9/23/02 
					// Like the comment above says, check for the section being 
					// 4 digit before trying to set the text of the section to
					// 4 digit year.
					// Enclosed below code in the if block.
					//
					if ( this.isFourDigits )
					{
                        // AS 10/8/08 Optimization - TFS8781
						//int val = this.GetYear( );
                        int val;
                        if (this.TryGetYear(out val))
                        {
                            this.SetText(NumberSection.PadNumber(val, 4));
                        }
					}

					
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

					// --------------------------------------------------
				}
				catch ( Exception )
				{
				}
			}

			// SSP 3/12/02
			// As long as there is a valid number in this year section
			// then return true.
			//
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			return true;
		}

		// SSP 10/23/01
		// Overrode the Decrement function to start at current year instead of the Max
		// year
		//	
		// SSP 3/26/02
		// Combined Decrement and Increment into one Spin method
		//


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool Spin( bool up, bool setLimit ) 
		{			
			string s = this.GetText( InputMaskMode.Raw );

            // AS 8/25/08 Support Calendar
            System.Globalization.Calendar calendar = this.Calendar;

			// If the year is not set, set it to the current year.
			if ( null == s || s.Length == 0 || !this.IsInt( s ) )
			{
				try
				{
                    // AS 8/25/08 Support Calendar
                    //this.SetText( DateTime.Now.Year.ToString( ) );
					this.SetText( calendar.GetYear(DateTime.Now).ToString( ) );
					return true;
				}
				catch ( Exception ) 
				{
				}
			}

            // JDN 10/29/04 SpinWrap
            //return base.Spin( up, setLimit );

            // retrieve min and max spin values
            decimal minYearValue = this.MinValue;
            decimal maxYearValue = this.MaxValue;

            object minValue = this.MaskInfo.MinValue;
			// JDN 1/24/05 BR01911	
            //minValue = this.MaskInfo.MaskedInput.ValueToDataValue( minValue, this.MaskInfo.Owner, this.MaskInfo.OwnerContext );
            if ( null != minValue )
            {
                try
                {
                    if ( minValue is DateTime )
                    {
                        // AS 8/25/08 Support Calendar
                        //minYearValue = ((DateTime)minValue).Year;
                        minYearValue = calendar.GetYear(((DateTime)minValue));
                    }
                }
                catch ( Exception )
                {
                }
            }

            object maxValue = this.MaskInfo.MaxValue;
			// JDN 1/24/05 BR01911			
            //maxValue = this.MaskInfo.MaskedInput.ValueToDataValue( maxValue, this.MaskInfo.Owner, this.MaskInfo.OwnerContext );
            if ( null != maxValue )
            {
                try
                {
                    if ( maxValue is DateTime )
                    {
                        // AS 8/25/08 Support Calendar
                        //maxYearValue = ((DateTime)maxValue).Year;
                       maxYearValue = calendar.GetYear(((DateTime)maxValue));
                    }
                }
                catch ( Exception )
                {
                }
            }

            // MRS 7/7/06 - BR14098
            // We need to format the year for 4 digits and pass it in.
            //return base.Spin( up, setLimit, minYearValue, maxYearValue );
            decimal val;

            if (setLimit)
                val = up ? minYearValue : maxYearValue;
            else
            {
                try
                {
                    val = this.GetYear();
                    val = up ? (val + 1) : (val - 1);
                }
                catch
                {
                    val = up ? minYearValue : maxYearValue;
                }
            }
            return base.Spin(up, setLimit, val, minYearValue, maxYearValue);          
		}

		// SSP 2/18/04
		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			base.SetText( text.PadLeft( this.NumberOfDigits, '0' ) );
		}







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			YearSection clone = new YearSection( this.isFourDigits );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}

        // AS 8/25/08 Support Calendar
        internal static void InitializeMinMax(YearSection yearSection, System.Globalization.Calendar calendar)
        {
            yearSection.MinValue = calendar.GetYear(calendar.MinSupportedDateTime);
            yearSection.MaxValue = calendar.GetYear(calendar.MaxSupportedDateTime);
        }
    }


	#endregion //YearSection

	#region HourSection

	/// <summary>
	/// Hour section of a time mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// HourSection represents the hour portion of a time mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class HourSection : NumberSection
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="HourSection"/> class
		/// </summary>
		public HourSection( )
			: base( 2 )
		{
			this.MinValue = 0;
			this.MaxValue = 23;
		}

		/// <summary>
		/// ValidateString
		/// </summary>
		/// <param name="str">string to validate</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateString( ref string str )
		{
			// SSP 9/12/03 - Ink Provider Related changes
			// Check for prompt and pad characters.
			//
			// ----------------------------------------------------------------------------------------
			str = NumberSection.StripOutPadPromptCharacters( str, this );
			// ----------------------------------------------------------------------------------------

			int h = -1;
			
			try
			{
                // AS 10/8/08 Optimization - TFS8781
                //h = int.Parse( str, NumberStyles.None );
                if (false == int.TryParse(str, NumberStyles.None, NumberFormatInfo.CurrentInfo, out h))
                    return false;
			}
			catch ( Exception )
			{
				return false;
			}

			// SSP 7/22/02 UWG1418
			// Look at the MaxValue which will be 12 or 23 depending on whether the
			// section is 12 hours or 24 hours.
			//
			//if ( h >= 1 && h <= 12 )
			if ( h >= this.MinValue && h <= this.MaxValue )
			{
				str = h.ToString();
				return true;
			}

			return false;
		}

		// SSP 2/18/04
		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			base.SetText( text.PadLeft( this.NumberOfDigits, '0' ) );
		}


		/// <summary>
		/// ValidateSection
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{
			if ( !base.ValidateSection( contentModificationsAllowed ) )
				return false;

			decimal val;

			if ( !this.ParseToInt( out val ) )
				return false;

			// SSP 7/22/02 UWG1418
			// Look at the MaxValue which will be 12 or 23 depending on whether the
			// section is 12 hours or 24 hours.
			//
			//if ( val < 1 || val > 12 )
			if ( val < this.MinValue || val > this.MaxValue )
				return false;

			return true;
		}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanInsertCharAt( int pos, char c )
		{
			if ( !base.CanInsertCharAt( pos, c ) )
				return false;

			// SSP 1/17/02
			// Look at the max value (whether it's 12 or 24 and validate
			// differently for those two different values.)
			//

			if ( this.MaxValue > 12 )
			{
				// In a 24 hour hour sections, validate differently.
				//
				if ( 0 == pos )
				{
					if ( this.DisplayChars[1].Char > '3' && c > '1' )
						return false;

					if ( (char)0 != this.DisplayChars[1].Char && c > '2' )
						return false;
				}

				if ( 1 == pos )
				{
					if ( this.DisplayChars[1].Char > '2' )
						return false;

					if ( '2' == this.DisplayChars[1].Char && c > '3' )
						return false;
				}
			}
			else
			{
				if ( 0 == pos )
				{
					if ( this.DisplayChars[1].Char > '2' && c > '0' )
						return false;

					if ( (char)0 != this.DisplayChars[1].Char && c > '1' )
						return false;
				}

				if ( 1 == pos )
				{
					if ( this.DisplayChars[1].Char > '1' )
						return false;

					if ( '1' == this.DisplayChars[1].Char && c > '2' )
						return false;
				}
			}

			return true;
		}







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			HourSection clone = new HourSection( );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}
	}

	#endregion // HourSection

	#region AMPMSection

	/// <summary>
	/// Class for AM-PM section of a time mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// AMPMSection represents the AM/PM portion of a time mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class AMPMSection : DisplayCharsEditSection
	{
		private string amValue;
		private string pmValue;

		internal AMPMSection( string amValue, string pmValue )
			: base( )
		{
			Utils.ValidateNull( "amValue", amValue );
			Utils.ValidateNull( "pmValue", pmValue );

			this.amValue = amValue;
			this.pmValue = pmValue;

			this.CreateDisplayChars( );
		}






		internal string AMValue
		{
			get
			{
				return this.amValue;
			}
		}






		internal string PMValue
		{
			get
			{
				return this.pmValue;
			}
		}






		internal override bool SupportsSpinning
		{
			get
			{
				return true;
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanSpin( bool up )
		{
			return true;
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool Spin( bool up, bool setLimit )
		{			
			this.ToggleAMPM( );
			return true;
		}

		internal void ToggleAMPM(  )
		{
			string currText = this.GetText( InputMaskMode.Raw );

			var culture = this.MaskInfo.CultureInfo;
			if ( 0 == Utils.CompareStrings( currText, this.amValue, true, culture ) )
				this.SetText( pmValue );
			else if ( 0 == Utils.CompareStrings( currText, this.pmValue, true, culture ) )
				this.SetText( this.amValue );
			else
				this.SetText( this.amValue );
		}







		internal override bool IsInputFull( )
		{
			string str = this.GetText( InputMaskMode.Raw );

			var culture = this.MaskInfo.CultureInfo;
			if ( 0 == Utils.CompareStrings( this.amValue, str, true, culture ) ||
				 0 == Utils.CompareStrings( this.pmValue, str, true, culture ) )
			{
				return true;
			}

			return false;
		}

		
		
		
		
		private sealed class CustomDisplayChar : InputCharBase
		{
			char inputChar; // = (char)0;







			internal CustomDisplayChar( AMPMSection section ) : base( section )
			{
				this.IncludeMethod = DisplayCharIncludeMethod.Always;
			}






			public override bool IsEditable
			{
				get
				{
					return true;
				}
			}

			/// <summary>
			/// checks to see if specified character c matches
			/// </summary>
			/// <param name="c"></param>
			/// <returns></returns>
			public override bool MatchChar( char c )
			{
				char filteredChar = this.Filter( c );

				AMPMSection section = (AMPMSection)this.Section;
				
				DisplayCharsCollection displayChars = section.DisplayChars;

				int index = this.Index;

				bool amMatched = false;
				bool pmMatched = false;

				char lowercase = char.ToLower( c, this.MaskInfo.CultureInfo );

				if ( section.amValue.Length > index && 
					char.ToLower( section.amValue[index], this.MaskInfo.CultureInfo ) == lowercase )
					amMatched = true;

				if ( section.pmValue.Length > index && 
					char.ToLower( section.pmValue[index], this.MaskInfo.CultureInfo ) == lowercase )
					pmMatched = true;

				if ( !amMatched && !pmMatched )
					return false;

				if ( 0 == index )
					return true;

				if ( amMatched && this.PrevDisplayChar.MatchChar( section.amValue[ index - 1 ] ) )
					return true;

				if ( pmMatched && this.PrevDisplayChar.MatchChar( section.pmValue[ index - 1 ] ) )
					return true;

				return false;
			}

			/// <summary>
			/// Overridden
			/// </summary>
			public override char Char
			{
				get
				{
					if ( !this.MatchChar( this.inputChar ) )
						return (char)0;

					return this.inputChar;
				}
				set
				{
					char c = value;

					// if an attempt to enter a value other than 0 is done, then we have to
					// see if that value matches using MatchChar.
					if ( (char)0 != value && !this.MatchChar( c ) )
						c = (char)0;

					if ( 0 != c )
						c = this.Filter( c );

					this.inputChar = c;

					this.NotifyPropertyChangedEvent( PROPERTY_CHAR );
				}
			}







			internal override void InternalCopy( DisplayCharBase dest )
			{
				base.InternalCopy( dest );

				AMPMSection.CustomDisplayChar destChar = (CustomDisplayChar)dest;
				destChar.inputChar = this.inputChar;
			}







			internal override DisplayCharBase InternalClone( SectionBase section )
			{
				AMPMSection.CustomDisplayChar clone = new AMPMSection.CustomDisplayChar( (AMPMSection)section );

				this.InternalCopy( clone );

				return clone;
			}
		}

		private void CreateDisplayChars( )
		{
			int maxLen = Math.Max( this.amValue.Length, this.pmValue.Length );

			for ( int i = 0; i < maxLen; i++ )
				this.DisplayChars.Add( new CustomDisplayChar( this ) );			
		}







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			AMPMSection clone = new AMPMSection( this.amValue, this.pmValue );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}

		// Overrode ValidateSection. If only "a" or "p" has been typed then make them 
		// "am" and "pm" respectively. This way the user doesn't have to type in the full
		// "am" or "pm".
		//
		/// <summary>
		/// ValidateSection
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{
			if ( ! base.ValidateSection( contentModificationsAllowed ) )
				return false;

			if ( contentModificationsAllowed )
			{
				string prefix = "";
				for ( int i = 0; i < this.DisplayChars.Count; i++ )
				{
					DisplayCharBase dc = this.DisplayChars[i];
					if ( null != dc && ! dc.IsEmpty )
						prefix += dc.Char;
				}

				if ( prefix.Length > 0 )
				{
					prefix = prefix.ToLower( System.Globalization.CultureInfo.CurrentCulture );

					bool matchesAM = this.AMValue.ToLower( System.Globalization.CultureInfo.CurrentCulture ).StartsWith( prefix );
					bool matchesPM = this.PMValue.ToLower( System.Globalization.CultureInfo.CurrentCulture ).StartsWith( prefix );
					if ( matchesAM && ! matchesPM )
						this.SetText( this.AMValue );
					else if ( matchesPM && ! matchesAM )
						this.SetText( this.PMValue );
				}
			}

			return true;
		}
	}


	#endregion //AMPMSection
	
	#region MinuteSection

	/// <summary>
	/// Minute section of a time mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// MinuteSection represents minute portion of a time mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class MinuteSection : NumberSection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MinuteSection"/> class
		/// </summary>
		public MinuteSection( )
			: base( 2 )
		{
			this.MinValue = 0;
			this.MaxValue = 59;
		}

		/// <summary>
		/// ValidateString
		/// </summary>
		/// <param name="str">string to validate</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateString( ref string str )
		{
			// Check for prompt and pad characters.
			//
			str = NumberSection.StripOutPadPromptCharacters( str, this );

			int m = -1;
			try
			{
                // AS 10/8/08 Optimization - TFS8781
                //m = int.Parse( str, NumberStyles.None );
                if (false == int.TryParse(str, NumberStyles.None, NumberFormatInfo.CurrentInfo, out m))
                    return false;
			}
			catch ( Exception )
			{
				return false;
			}

			if ( m >= 0 && m <= 59 )
			{
				str = m.ToString();
				return true;
			}

			return false;
		}

		/// <summary>
		/// ValidateSection
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{	
			if ( !base.ValidateSection( contentModificationsAllowed ) )
				return false;

			decimal val;

			if ( !this.ParseToInt( out val ) )
				return false;

			if ( val < 0 || val >= 60 )
				return false;

			return true;
		}

		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			base.SetText( text.PadLeft( this.NumberOfDigits, '0' ) );
		}







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			MinuteSection clone = new MinuteSection( );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}
	}


	#endregion //MinuteSection

	#region SecondSection

	/// <summary>
	/// Second section of a time mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// SecondSection represents second portion of a time mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class SecondSection : NumberSection
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="SecondSection"/> class
		/// </summary>
		public SecondSection( )
			: base( 2 )
		{
			this.MinValue = 0;

			//	BF 5.26.03	UWE566
			//	MaxValue represents the greatest valid value, not
			//	the first invalid value, so we should return 59 here.
			//this.MaxValue = 60;
			this.MaxValue = 59;
		}


		/// <summary>
		/// ValidateString
		/// </summary>
		/// <param name="str">string to validate</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateString( ref string str )
		{
			// Check for prompt and pad characters.
			//
			str = NumberSection.StripOutPadPromptCharacters( str, this );

			int s = -1;
			try
			{
                // AS 10/8/08 Optimization - TFS8781
				//s = int.Parse( str, NumberStyles.None );
                if (false == int.TryParse(str, NumberStyles.None, NumberFormatInfo.CurrentInfo, out s))
                    return false;
			}
			catch ( Exception )
			{
				return false;
			}

			if ( s >= this.MinValue && s <= this.MaxValue )
			{
				str = s.ToString();
				return true;
			}

			return false;
		}

		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			base.SetText( text.PadLeft( this.NumberOfDigits, '0' ) );
		}

		/// <summary>
		/// ValidateSection
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{	
			if ( !base.ValidateSection( contentModificationsAllowed ) )
				return false;

			decimal val;

			if ( !this.ParseToInt( out val ) )
				return false;

			if ( val < this.MinValue || val > this.MaxValue )
				return false;

			return true;
		}







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			SecondSection clone = new SecondSection( );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}
	}


	#endregion //SecondSection

	#region FractionPart

	/// <summary>
	/// Edit section implementation for a fraction part.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// FractionPart represents the fraction part of a numeric mask.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public class FractionPart : EditSectionBase
	{
		#region Private Vars

		private int digits = 2;
		private double minValue;
		private double maxValue;

		#endregion // Private Vars

		#region Constructor







		internal FractionPart( int digits )
			: base( )
		{
			this.digits = digits;
			
			//this.minValue = 0;
			// SSP 8/16/10 TFS27897
			// Refactored and moved into a new method.
			// 
			//this.maxValue = 1.0 - Math.Pow( 10, -digits );
			this.ResetMaxValue( );

			this.CreateDisplayChars( );
		}

		#endregion // Constructor

		#region NumberOfDigits

		/// <summary>
		/// Number of digits in this fraction part.
		/// </summary>
		public int NumberOfDigits
		{
			get
			{
				return this.digits;
			}
		}

		#endregion // NumberOfDigits

		#region MinValue






		internal double MinValue
		{
			get
			{
				return this.minValue;
			}
            set
            {
                this.minValue = value;
            }
		}

		#endregion //MinValue

		#region MaxValue






		internal double MaxValue
		{
			get
			{
				return this.maxValue;
			}
            set
            {
                this.maxValue = value;
            }
		}

		#endregion //MaxValue

		#region ResetMaxValue

		// SSP 8/16/10 TFS27897
		// 
		internal void ResetMaxValue( )
		{
			this.maxValue = 1.0 - Math.Pow( 10, -digits );
		} 

		#endregion // ResetMaxValue

		#region ResetMinValue

		// SSP 8/16/10 TFS27897
		// 
		internal void ResetMinValue( )
		{
			this.minValue = 0;
		}

		#endregion // ResetMinValue

		#region DeltaValue







		internal double DeltaValue
		{
			get
			{
				double d = Math.Pow( 10, -this.digits );

				return d;
			}
		}

		#endregion //DeltaValue

		#region ContinuousEditing

		// This flag indicates that the input characters are to be shifted across number
		// and fraction sections.
		//






		internal virtual bool ContinuousEditing
		{
			get
			{
				return false;
			}
		}

		#endregion // ContinuousEditing

		#region CanInsertCharAt

		internal override bool CanInsertCharAt( int pos, char c )
		{			
			Debug.Assert( pos >= 0 && pos < this.DisplayChars.Count, 
				"attempt to replace character at an invalid position" );

			if ( pos < 0 || pos >= this.DisplayChars.Count )
				return false;

			DisplayCharBase dc = this.DisplayChars[ pos ];

			// this should never be the case, since display chars collection always holds
			// non null values
			Debug.Assert( null != dc, "null display char in display chars collection" );
			if ( null == dc )
				return false;

			Debug.Assert( dc.IsEditable, "attempt to insert character at a literal DisplayChar position" );
			if ( !dc.IsEditable )
				return false;

			if ( !dc.MatchChar( c ) )
				return false;

			if ( !this.LastDisplayChar.IsEmpty &&  '0' != this.LastDisplayChar.Char )
				return false;

			return true;
		}

		#endregion //CanInsertCharAt

		#region InsertCharAt



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool InsertCharAt( int pos, char c )
		{
			Debug.Assert( (char)0 != c );

			if ( (char)0 == c )
				return false;

			if ( this.CanInsertCharAt( pos, c ) )
			{			
				for ( int i = this.DisplayChars.Count - 1; i > pos; i--)
				{
					this.DisplayChars[i].Char = this.DisplayChars[i-1].Char;
				}

				this.DisplayChars[pos].Char = c;

				return true;
			}

			return false;
		}

		#endregion //InsertCharAt

		#region DeleteCharAndShift



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool DeleteCharAndShift( int pos )
		{
			return this.DeleteCharsAndShift( pos, 1 );
		}
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool DeleteCharsAndShift( int pos, int count )
		{			
			int displayCharCount = this.DisplayChars.Count;

			Debug.Assert( pos >= 0 && pos < displayCharCount, 
				"attempt to delete character at an invalid position" );
			if ( pos < 0 || pos >= displayCharCount )
				return false;

			for ( int i = pos; i < displayCharCount; i++ )
			{
				char c = (char)0;
				if ( count+i < displayCharCount )
					c = this.DisplayChars[count+i].Char;

				this.DisplayChars[i].Char = c;
			}

			for ( int i = 0; i < count; i++ )
			{
				this.DisplayChars[ displayCharCount - i - 1 ].Char = (char)0;
			}

			return true;      			
		}

		#endregion //DeleteCharAndShift

		#region SetText

		// Made the SetText property public. Before it was internal. This will provide greater flexiblility
		// in terms what sort of custom things one can do with editor with mask related editors.
		//
		/// <summary>
		///	Assigns the text to the section. Call to this method with an invalid text will result in an exception.
		/// </summary>
		/// <param name="text"></param>
		public override void SetText( string text )
		{
			// since this is a fraction section, we are going to
			// set the text from left to right
			int textIndex = 0;

            
            
            
            DisplayCharsCollection dcColl = this.DisplayChars;
            int count = dcColl.Count;

			for ( int i = 0; i < count; i++ )
			{
				DisplayCharBase dc = dcColl[ i ];

				if ( !dc.IsEditable )
					continue;

				char c = (char)0;
				if ( textIndex >= 0 && textIndex < text.Length )
					c = text[ textIndex++ ];

				dc.Char = c;
			}
		}

		
		#endregion //SetText

		#region IsInputFull







		internal override bool IsInputFull( )
		{
			if ( base.IsInputFull( ) )
				return true;

			if ( this.LastDisplayChar.IsEmpty || '0' == this.LastDisplayChar.Char )
				return false;

			return true;
		}

		#endregion //IsInputFull

		#region EraseChars



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal override void EraseChars( int startIndex, int endIndex )
		{
			if ( startIndex > endIndex )
			{
				int tmp = startIndex;
				startIndex = endIndex;
				endIndex = tmp;
			}

			int delta = 1 + endIndex - startIndex;

			for ( int i = startIndex; i < this.DisplayChars.Count; i++ )
			{
				if ( i >= this.DisplayChars.Count )
					continue;

				char c = (char)0;

				if ( i + delta < this.DisplayChars.Count )
					c = this.DisplayChars[ i + delta ].Char;

				if ( this.DisplayChars[i].IsEditable )
					this.DisplayChars[i].Char = c;
			}
		}


		#endregion //EraseChars

		#region NumberDecimalSeparator






		private char NumberDecimalSeparator
		{
			get
			{
				return XamMaskedInput.GetCultureChar( '.', this.MaskInfo.FormatProvider, false );
			}
		}

		#endregion // NumberDecimalSeparator

		#region GetFractionValue
			


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal double GetFractionValue( )
		{
			StringBuilder sb = new StringBuilder( "0" + this.DecimalSeperatorChar , 5 + this.DisplayChars.Count );

            
            
            
            DisplayCharsCollection dcColl = this.DisplayChars;
			int count = dcColl.Count;

			for ( int i = 0; i < count; i++ )
			{
				char c = dcColl[i].Char;

				if ( (char)0 == c && this.ContinuousEditing )
                    c = '0';					

				if ( (char)0 != c )
				{
					bool isDigit = char.IsDigit( c );

					Debug.Assert( isDigit, "Not a digit !" );

					if ( isDigit )
						sb.Append( c );
				}
			}

			sb.Append( '0' );

			return double.Parse( sb.ToString( ), this.MaskInfo.FormatProvider );
		}

		#endregion //GetFractionValue

		#region CanShiftRight



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanShiftRight( int from, int positionsToShift )
		{
			return false;
		}

		#endregion // CanShiftRight

		#region CanShiftLeft
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanShiftLeft( int from, int positionsToShift )
		{
			return false;
		}

		#endregion // CanShiftLeft

		#region ShiftLeft
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool ShiftLeft( int from, int positionsToShift )
		{
			return false;
		}

		#endregion // ShiftLeft

		#region ShiftRight
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool ShiftRight( int from, int positionsToShift )
		{
			return false;
		}

		#endregion // ShiftRight

		#region DeleteCharWithNoShift
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool DeleteCharWithNoShift( int pos )
		{
			return false;
		}

		#endregion // DeleteCharWithNoShift

		#region ValidateSection

		/// <summary>
		/// ValidateSection.
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{	
			if ( contentModificationsAllowed )
			{
				int count = this.DisplayChars.Count;

				int i;

				for ( i = 0; i < count; i++ )
				{
					DisplayCharBase dc = this.DisplayChars[i];

					if ( dc.IsEmpty )
					{
						if ( null != this.LastFilledChar && dc.Index < this.LastFilledChar.Index )
						{
							this.DeleteCharAndShift( i );
							i--;
						}
						else
						{
							break;
						}
					}
				}
			}

			return true;
		}

		#endregion // ValidateSection

		#region PadWithZero
		





		internal virtual void PadWithZero( )
		{
			int count = this.DisplayChars.Count;

			// In fraction section, fill the characters after the last filled
			// character with 0. So 10.5_ will become 10.50.
			//
			for ( int i = count - 1; i >= 0; i-- )
			{
				DisplayCharBase dc = this.DisplayChars[i];

				if ( null == dc )
					continue;

				if ( dc.IsEmpty )
					dc.Char = '0';
				else
					break;
			}
		}

		#endregion // PadWithZero

		#region TrimInsignificantZeros

		// SSP 9/14/11 TFS76307
		// 
		/// <summary>
		/// Trims zeros at the end of the input.
		/// </summary>
		internal virtual void TrimInsignificantZeros( )
		{
			DisplayCharsCollection displayChars = this.DisplayChars;
			int count = displayChars.Count;

			bool nonEmptyCharEncountered = false;

			// In fraction section, fill the characters after the last filled
			// character with 0. So 10.5_ will become 10.50.
			//
			for ( int i = count - 1; i >= 0; i-- )
			{
				DisplayCharBase dc = displayChars[i];

				if ( !nonEmptyCharEncountered && dc.IsEmpty )
					continue;

				nonEmptyCharEncountered = true;

				if ( '0' == dc.Char )
					dc.Char = (char)0;
				else
					break;
			}
		} 

		#endregion // TrimInsignificantZeros

		#region Orientation

		/// <summary>
		/// Returns whether editing is left-to-right or right-to-left.
		/// </summary>
		public override EditOrientation Orientation
		{
			get
			{
				return EditOrientation.LeftToRight;
			}
		}

		#endregion // Orientation

		#region ValidateString

		/// <summary>
		/// ValidateString.
		/// </summary>
		/// <param name="str">string to validate</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateString( ref string str )
		{
			// Check for prompt and pad characters.
			//
			str = NumberSection.StripOutPadPromptCharacters( str, this );

			for ( int i = 0; i < str.Length; i++ )
			{
				if ( !char.IsDigit( str, i ) )
					return false;
			}

			if ( str.Length > this.DisplayChars.Count )
				str = str.Substring( 0, this.DisplayChars.Count );

			return true;
		}

		#endregion // ValidateString

		#region CreateDisplayChars

		private void CreateDisplayChars( )
		{
			this.DisplayChars.Clear( );

			for ( int i = 0; i < digits; i++ )
				this.DisplayChars.Add( new DigitChar( this ) );
		}

		#endregion //CreateDisplayChars

		#region SupportsSpinning






		internal override bool SupportsSpinning
		{
			get
			{
				return true;
			}
		}

		#endregion // SupportsSpinning

		#region CanSpin



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal override bool CanSpin( bool up )
		{
			string s = this.GetText( InputMaskMode.Raw );

			// We can always increment empty number seciton.
			//
			//	BF 10.6.03	UWE720
			//	We should disallow decrementing empty number sections
			//
			//if ( null == s || 0 == s.Length )
			if ( up && (null == s || 0 == s.Length) )
				return true;

			double val;
			try
			{
				val = this.GetFractionValue( );
			}
			catch ( Exception )
			{
				return false;
			}

            if ( this.MaskInfo.MaskedInput.SpinWrap )
                return true;

			if ( up )
			{
				return ( this.DeltaValue + val ) <= this.MaxValue;
			}
			else
			{
				return ( val - this.DeltaValue ) >= this.MinValue;
			}
		}

		#endregion // CanSpin

		#region SetFractionValue
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void SetFractionValue( double fractionValue )
		{
			fractionValue = fractionValue - Math.Floor( fractionValue );

			try
			{
				System.Int64 val = (System.Int64)Math.Round( fractionValue * Math.Pow( 10, this.digits ), 0 );

				// SSP 8/30/11 TFS76307
				// Added TrimFractionalZeros property on XamMaskedEditor.
				// 
				// ------------------------------------------------------------------------------
				//this.SetText( NumberSection.PadNumber( val, this.digits ) );

				string text = NumberSection.PadNumber( val, this.digits );

				XamMaskedInput editor = this.MaskedEdit;
				if ( editor.TrimFractionalZeros && !( this is FractionPartContinuous ) )
					text = text.TrimEnd( '0' );

				this.SetText( text );
				// ------------------------------------------------------------------------------
			}
			catch ( Exception e )
			{
				Debug.Assert( false, "Exception thrown with messange:\n" + e.Message );
			}
		}

		#endregion // SetFractionValue

		#region NumberSection






		internal NumberSection NumberSection
		{
			get
			{
				return this.PreviousEditSection as NumberSection;
			}
		}

		#endregion // NumberSection

		#region Spin
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal override bool Spin( bool up, bool setLimit )
		{
			try
			{
				double val;
				XamMaskedInput editor = this.MaskedEdit;

				if ( setLimit )
				{
					val = up ? this.MaxValue : this.MinValue;
				}
				else
				{
					string s = this.GetText( InputMaskMode.Raw );

					// SSP 9/14/11 TFS76307
					// 
					// ----------------------------------------------------------------------
					if ( string.IsNullOrEmpty( s ) || !this.IsInt( s ) )
					{
						val = up ? this.MinValue : this.MaxValue;

						if ( up && 0 == val && editor.TrimFractionalZeros )
							val += this.DeltaValue;
					}
					else
					{
						val = double.Parse( "0" + this.NumberDecimalSeparator + s, this.MaskInfo.FormatProvider );
						val += ( up ? this.DeltaValue : -this.DeltaValue );
					}
					////	BF 7.24.02	UWE81
					////	If the section is unpopulated, an up click should
					////	set the value to the minimum
					////
					//// JJD 12/9/02 - FxCop
					//// Check the length instead of an == check.	
					////if ( string.Empty.Equals( s ) && up )
					//if ( ( s == null || s.Length == 0 ) && up )
					//    val = this.MinValue; 
					//else
					//{
					//    // JJD 12/9/02 - FxCop
					//    // Check the length instead of an == check.	
					//    //if ( null != s && !string.Empty.Equals( s ) && this.IsInt( s ) )
					//    if ( null != s && s.Length > 0 && this.IsInt( s ) )
					//        // SSP 9/30/03 UWE503
					//        // Use the culture dependant decimal separator character rather than ".".
					//        //
					//        //val = double.Parse( "0." + s );
					//        val = double.Parse( "0" + this.NumberDecimalSeparator + s, this.MaskInfo.FormatProvider );
					//    else
					//        val = up ? ( this.MinValue - 1 ) : ( this.MaxValue + 1 );

					//    val = up ? ( val + this.DeltaValue ) : ( val - this.DeltaValue );
					//}
					// ----------------------------------------------------------------------
				}

                // JDN 10/29/04 - allow spin buttons to wrap value
				// if ( val >= this.MinValue && val <= this.MaxValue )
                // if ( (val >= this.MinValue && val <= this.MaxValue) || this.MaskedEdit.SpinWrap )
					// this.SetFractionValue( val ); 

                // JDN 1/10/05 BRO1265
                if ( editor.SpinWrap )
                {
					// SSP 9/14/11 TFS76307
					// Replaced all SetFractionValue calls with SetFractionValue_SpinHelper calls below.
					// 

                    if ( up )
                    {
						if ( val <= this.MaxValue )
							this.SetFractionValue( val ); 
						else if ( val > this.MaxValue )
							this.SetFractionValue( this.MinValue );
                    }
                    else
                    {
                        if ( val >= this.MinValue )
							this.SetFractionValue( val );
                        else if ( val < this.MinValue )
							this.SetFractionValue( this.MaxValue );
                    }
                    return true;
                }
                else
                {
                    if ( val >= this.MinValue && val <= this.MaxValue )
                    {
						this.SetFractionValue( val ); 
                        return true;
                    }
                    else
                        return false;
                }
	
				//return true;
			}
			catch ( Exception e )
			{
				Debug.Assert( false, "Exception thrown with message:\n" + e.Message );
			}

			return false;
		}

		#endregion // Spin

		#region InternalClone







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			FractionPart clone = new FractionPart( this.digits );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}

		#endregion // InternalClone

		#region InternalCopy

		// SSP 4/8/05 BR03077
		//






		internal override void InternalCopy( SectionBase dest )
		{
			base.InternalCopy( dest );
			FractionPart destFP = (FractionPart)dest;
		}

		#endregion // InternalCopy
	}

	#endregion //FractionPart

	#region FractionPartContinuous

	/// <summary>
	/// Edit section implementation for a fraction part.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// FractionPartContinous represents the fraction part of a numeric mask. This differs from
	/// <see cref="FractionPart"/> in that this section will allow digits entered into the fraction
	/// part to flow into the integer part as the fraction part gets filled up. This allows for
	/// convenient entering of values without having to enter the '.' character. You specify
	/// this kind of fraction section in the mask using "{double:-n.m:c}" mask token where 'n'
	/// is the number of digits in integer portion, 'm' the number of digits in the fraction portion
	/// and 'c' for continous fraction part. See the associated entry in the table of masks
	/// in the <a href="xamInputs_Masks.html">Masks</a> topic for more information.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use sections.
	/// XamMaskedInput will automatically create and manage these objects based on the supplied mask.
	/// </para>
	/// </remarks>
	public sealed class FractionPartContinuous : FractionPart
	{
		#region Private Vars

		#endregion // Private Vars

		#region Constructor







		internal FractionPartContinuous( int digits )
			: base( digits )
		{
		}

		#endregion // Constructor

		#region ContinuousEditing







		internal override bool ContinuousEditing
		{
			get
			{
				return true;
			}
		}

		#endregion // ContinuousEditing

		#region CanInsertCharAt

		internal override bool CanInsertCharAt( int pos, char c )
		{			
			Debug.Assert( pos >= 0 && pos <= this.DisplayChars.Count, 
				"attempt to replace character at an invalid position" );
			if ( pos < 0 || pos > this.DisplayChars.Count )
				return false;

			DisplayCharBase dc = this.SafeDisplayCharAt( pos );			

			DisplayCharBase testDc = null != dc ? dc : this.LastDisplayChar;
			if ( ! testDc.MatchChar( c ) )
				return false;

            NumberSection ns = this.NumberSection;

			if ( this.IsInputFull( ) )
			{
				// If this section is full then see if we can carry the first digit to the
				// number section.
				//
				if ( null != ns && ! ns.CanInsertCharAt( ns.LastDisplayChar.Index, this.FirstDisplayChar.Char ) )
					return false;
			}

			return true;
		}

		#endregion // CanInsertCharAt

		#region InsertCharAt



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool InsertCharAt( int pos, char c )
		{
			Debug.Assert( (char)0 != c );

			if ( (char)0 == c )
				return false;

			NumberSection ns = this.NumberSection;

			if ( this.CanInsertCharAt( pos, c ) )
			{			
				if ( this.FirstDisplayChar.IsEmpty
					|| null != ns && ns.InsertCharAt( ns.LastDisplayChar.Index, this.FirstDisplayChar.Char ) )
				{
					for ( int i = 1; i <= pos; i++ )
						this.DisplayChars[ i - 1 ].Char = this.DisplayChars[ i ].Char;

					this.DisplayChars[ pos ].Char = c;

					return true;
				}
			}

			return false;
		}

		#endregion //InsertCharAt

		#region DeleteCharAndShift
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal override bool DeleteCharsAndShift( int pos, int count )
		{			
			for ( int i = 0; i < count; i++ )
			{
				if ( ! this.DeleteCharHelper( pos ) )
					return false;
			}

			return true;      			
		}

		private bool DeleteCharHelper( int pos )
		{
			int displayCharCount = this.DisplayChars.Count;

			Debug.Assert( pos >= 0 && pos < displayCharCount, 
				"attempt to delete character at an invalid position" );
			if ( pos < 0 || pos >= displayCharCount )
				return false;

			NumberSection ns = this.NumberSection;

			for ( int i = pos; i > 0; i-- )
				this.DisplayChars[ i ].Char = this.DisplayChars[ i - 1 ].Char;

			char c = null != ns ? ns.LastDisplayChar.Char : (char)0;
			if ( (char)0 != c && ( ! this.DisplayChars[ 0 ].MatchChar( c )
				|| ! ns.DeleteCharAndShift( ns.LastDisplayChar.Index ) ) )
				c = (char)0;

            this.DisplayChars[ 0 ].Char = c;
			return true;			
		}

		#endregion // DeleteCharAndShift

		#region GetText
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal override string GetText( InputMaskMode maskMode )
		{
			DisplayCharsCollection displayChars = this.DisplayChars;
			int count = displayChars.Count;
			StringBuilder sb = new StringBuilder( count );

			for ( int i = 0; i < count; i++ )
			{
				DisplayCharBase dc = displayChars[i];
				sb.Append( ! dc.IsEmpty ? dc.Char : '0' );
			}

			return sb.ToString( );
		}

		#endregion // GetText

		#region IsInputFull







		internal override bool IsInputFull( )
		{
			if ( base.IsInputFull( ) && ( null == this.NumberSection || this.NumberSection.IsInputFull( ) ) )
				return true;

			return false;
		}

		#endregion //IsInputFull

		#region EraseChars



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal override void EraseChars( int startIndex, int endIndex )
		{
			if ( startIndex > endIndex )
			{
				int tmp = startIndex;
				startIndex = endIndex;
				endIndex = tmp;
			}

			this.DeleteCharsAndShift( startIndex, 1 + endIndex - startIndex );
		}

		#endregion //EraseChars

		#region ValidateSection

		/// <summary>
		/// ValidateSection.
		/// </summary>
		/// <param name="contentModificationsAllowed">Whether or not contents can be modified</param>
		/// <returns><b>True</b> if valid, <b>false</b> otherwise</returns>
		public override bool ValidateSection( bool contentModificationsAllowed )
		{	
			if ( contentModificationsAllowed )
			{
				if ( ! this.IsEmpty || null != this.NumberSection && ! this.NumberSection.IsEmpty )
					this.PadWithZeroHelper( true );
			}

			return true;
		}

		#endregion // ValidateSection

		#region PadWithZero
		





		internal override void PadWithZero( )
		{
			this.PadWithZeroHelper( false );
		}

		#endregion // PadWithZero

		#region PadWithZeroHelper

		private void PadWithZeroHelper( bool force )
		{
			int count = this.DisplayChars.Count;

			for ( int i = count - 1; i >= 0; i-- )
			{
				DisplayCharBase dc = this.DisplayChars[i];
				if ( dc.IsEmpty )
				{
					// If in ContinuousEditing mode then only break if the whole section
					// is empty. Here we are trying to make ._5 become .05 however leave
					// .__ as it is.
					//
					if ( i == count - 1 && ! force )
						break;

					dc.Char = '0';
				}
			}
		}

		#endregion // PadWithZeroHelper

		#region Orientation

		/// <summary>
		/// Returns whether editing is left-to-right or right-to-left.
		/// </summary>
		public override EditOrientation Orientation
		{
			get
			{
				return EditOrientation.RightToLeft;
			}
		}

		#endregion // Orientation

		#region InternalClone







		internal override SectionBase InternalClone( SectionsCollection sections )
		{
			FractionPart clone = new FractionPartContinuous( this.NumberOfDigits );

			this.InternalCopy( clone );
			clone.InitSectionsCollection( sections );

			return clone;
		}

		#endregion // InternalClone
	}

	#endregion // FractionPart

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