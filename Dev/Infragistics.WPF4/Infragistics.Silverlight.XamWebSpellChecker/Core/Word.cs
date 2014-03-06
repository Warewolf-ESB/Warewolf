using System;

namespace Infragistics
{
	/// <summary>Represents a word in the text.</summary>
	public class WordOccurrence{
		private int caretStart;
		private int caretEnd;
		private String word;

		/// <summary>Gets the position of the start of this word in the main text.</summary>
		public int StartPosition{ get{ return caretStart; } set{ caretStart = value;} }
		/// <summary>Gets the position of the end of this word in the main text.</summary>
		public int EndPosition{ get{ return caretEnd; } set{ caretEnd = value;}}
		/// <summary>Gets the string of this bad word.</summary>
		public string Word{get{return word;} set{word = value;}}

        /// <summary>Gets the position of the start of this word in the main text.</summary>
        /// <returns>Integer position in the text where this word starts.</returns>
		public int GetStartPosition() 
		{
			return caretStart;
		}


        /// <summary>Gets the position of the end of this word in the main text.</summary>
        /// <returns>Integer position in the text where this word ends</returns>
		public int GetEndPosition() 
		{
			return caretEnd;
		}
		


		
		/// <summary>Constructs a BadWord instance.</summary>
		/// <param name="word">the word String that is misspelt</param>
		/// <param name="caretStart">the position in the original text</param>
		/// <param name="caretEnd">the end position in the text</param>
		public WordOccurrence(String word, int caretStart, int caretEnd) 
		{
			this.word = word;
			this.caretStart = caretStart;
			this.caretEnd = caretEnd;

		}


        /// <summary>Gets the String of this bad word.</summary>
        /// <returns>String in this BadWord.</returns>
		public String GetWord() 
		{
			return word;
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