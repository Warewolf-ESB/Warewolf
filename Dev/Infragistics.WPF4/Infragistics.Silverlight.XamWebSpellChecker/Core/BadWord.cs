using System;
using System.Collections;
using System.Collections.Generic;

namespace Infragistics
	{
	/// <summary>Represents a mis-spelt word in the text.</summary>
	public class BadWord : WordOccurrence{


		/// <summary>BadWord reason, incorrect spelling.</summary>
		internal static int REASON_SPELLING = 2;

		internal int reason;

		/// <summary>Gets the reason that this is a bad word.</summary>
        internal int GetReason() 
		{
			return reason;
		}

		/// <summary>Gets the reason that this is a bad word.</summary>
        internal int Reason { get { return reason; } }

		/// <summary>Constructs a BadWord (REASON_SPELLING) instance.</summary>
		/// <param name="word">the word String that is misspelt</param>
		/// <param name="caretStart">the position in the original text</param>
		/// <param name="caretEnd">the end position in the text</param>
		public BadWord(String word, int caretStart, int caretEnd) : base(word, caretStart, caretEnd)
		{
			reason = REASON_SPELLING;
		}
		
		/// <summary>Constructs a BadWord instance.</summary>
		/// <param name="word">the word String that is misspelt</param>
		/// <param name="caretStart">the position in the original text</param>
		/// <param name="caretEnd">the end position in the text</param>
		/// <param name="reason">the reason this is a bad word,  REASON_SPELLING</param>
        internal BadWord(String word, int caretStart, int caretEnd, int reason)
            : base(word, caretStart, caretEnd)
		{
			this.reason = reason;
		}

        private List<String> suggestions = null;
        /// <summary>
        /// List of potential corrections for the <see cref="BadWord"/>.
        /// </summary>
		public List<String> Suggestions
		{
			get
			{
				if (suggestions == null)
                    suggestions = new List<String>();
				return suggestions;
			}
			set
			{
				suggestions = value;
			}
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