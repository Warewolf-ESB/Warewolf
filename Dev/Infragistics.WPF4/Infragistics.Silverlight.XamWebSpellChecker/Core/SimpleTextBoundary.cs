using System;

namespace Infragistics.SpellChecker
{
	/// <summary>A simple implementation of Java's word BreakIterator</summary>
	/// <note>does not treat punctuation the same, Java's one treats ! and \s differently
	/// this treats anything that isnt a letter the same.</note>

	internal class SimpleTextBoundary{

		///<summary>The text being parsed.</summary>
		protected String theText;
		int pos;
		///<summary>Whether to simply treat hyphens as breaking chars.</summary>
		protected bool shw = false;
		///<summary>Language rules to use when parsing.</summary>
		protected int lp = ENGLISH;

		///<summary>English text parsing.</summary>
		public static int ENGLISH = 1;
		///<summary>French text parsing.</summary>
		public static int FRENCH = 2;

        /// <summary>
        /// Whether to treat hyphenated (-) words as separate words.  The default is false.
        /// </summary>
        /// <remarks>For e.g. if this is true, text like "cheap-deals" will be treated as two words, "cheap" and "deals", otherwise this will be treated as one word "cheap-deals".</remarks>
		public bool SeparateHyphenWords{
			get{ return shw; }
			set{ shw = value; }
		}



		///<summary>The language parsing to be used.</summary>
		public int LanguageParsing{
			get{ return lp; }
			set{ lp = value;}
		}



        /// <summary>
        /// Return the last boundary.  The iterator's current position is set to the last boundary.
        /// </summary>
		public virtual int Last()
		{
			pos = theText.Length;
			return pos;
		}


		/// <summary>Sets the text to be analysed.</summary>
		public virtual void SetText(String t){
			theText = t;
			pos = 0;
		}




        /// <summary>
        /// Return the first boundary after the specified offset.
        /// </summary>
        /// <param name="offset">the offset to start</param>
        /// <returns>The first boundary after offset as an integer.</returns>
		public virtual int Following(int offset)
		{

			bool lookingForWhiteSpace ;
			if (offset < 0 || offset >= theText.Length) throw new ArgumentOutOfRangeException("Following("+offset+") offset out of bounds");


			pos = offset;
			lookingForWhiteSpace = isAtNonWhiteSpace(pos);

			while( pos<theText.Length && isAtNonWhiteSpace(pos) == lookingForWhiteSpace ){
				pos++;
			}

			return pos;
		}



        /// <summary>
        /// Return the first boundary before the specified offset.
        /// </summary>
        /// <param name="offset">the offset to start</param>
        /// <returns>Returns the first boundary before the offset as an integer.</returns>
		public virtual int Preceding(int offset)
		{
			bool lookingForWhiteSpace ;
			if (offset < 0 || offset > theText.Length) throw new ArgumentOutOfRangeException("Preceding(offset) ["+offset+"] offset out of bounds");
			
			if (offset==0) return -1;

			pos = offset-1;
			lookingForWhiteSpace = isAtNonWhiteSpace(pos);
			
			while( pos>=0 && isAtNonWhiteSpace(pos) == lookingForWhiteSpace  ){
				pos--;
			}
			pos++;


			return pos;
		}

		/// <summary>Whether <c>offset</c> is one place after the last character in a word.</summary>
		public virtual bool IsBoundaryRight(int offset){

			if ( (offset < theText.Length && IsBoundary(offset) && offset > 0)
				|| (offset == theText.Length && offset > 0) ){

				return isAtNonWhiteSpace(offset-1);
			}else return false;
		}

        /// <summary>
        /// Whether <c>offset</c> is a boundary to the left of text.
        /// </summary>
		public virtual bool IsBoundaryLeft(int offset){
			if ( (offset < theText.Length && IsBoundary(offset) && offset > 0) ){
				return !isAtNonWhiteSpace(offset-1) && isAtNonWhiteSpace(offset);
			} else if (offset==0 && theText.Length >0) return Char.IsLetterOrDigit(theText[0]);
			else return false;
		}

        /// <summary>
        /// Determines if a character at the <c>offset</c> is a boundary.
        /// </summary>
		public virtual bool IsBoundary(int offset){
			if (offset < 0 || offset > theText.Length) throw new ArgumentOutOfRangeException("IsBoundary offset out of bounds "+offset+" >= "+theText.Length);
			if (offset == 0 || offset == theText.Length)
			    return true;
			else
			    return Following(offset - 1) == offset;
		}

        /// <summary>
        /// Returns whether the character at the position is a whitespace character.  This is contextually relevant.
        /// </summary>
		protected virtual bool isAtNonWhiteSpace(int position){
			if(position==theText.Length) return false;

			return Char.IsLetterOrDigit(theText[position]) 
					|| ( lp == ENGLISH && theText[position]=='\'' 
							&& (position+1!=theText.Length 
								&& Char.IsLetterOrDigit(theText[position+1]) 
								&& position>0 && Char.IsLetterOrDigit(theText[position-1]) )
						)
					|| ( !shw && theText[position]=='-' 
							&& (position+1!=theText.Length 
								&& Char.IsLetterOrDigit(theText[position+1]) 
								&& position>0 && Char.IsLetterOrDigit(theText[position-1]) )
						);
		
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