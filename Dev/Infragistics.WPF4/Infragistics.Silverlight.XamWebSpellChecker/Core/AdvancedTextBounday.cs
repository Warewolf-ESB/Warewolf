using System;

namespace Infragistics.SpellChecker
{
	/// <summary>Extends SimpleTextBoundary to included more advanced parsing.</summary>

	internal class AdvancedTextBoundary : SimpleTextBoundary {

		bool allowXML = false;

        /// <summary>
        /// Whether to ignore XML/HTML tags.  This should be set to true for RichTextBox support, but is false by default.
        /// </summary>
		public bool AllowXML{
			get{ return allowXML; }
			set{ allowXML = value; }
		}

		// finds XML closer for opener at offset, expectedCloserOffset is position of a closer
		int findPosOfCloser(int offset, int expectedCloserOffset)
        {
			int closerPos = expectedCloserOffset;
			if (closerPos > -1)
            {
				int numberOfOpenersBetweenOffsetAndCloser = 0;
				for (int i = offset; i <= closerPos; i++){
					if (theText[i] == '<') numberOfOpenersBetweenOffsetAndCloser++;
					if (theText[i] == '>') numberOfOpenersBetweenOffsetAndCloser--;
				}

				// adjust closerPos incase that unresolved openers exist
				if (numberOfOpenersBetweenOffsetAndCloser > 0)
					closerPos = findPosOfCloser(offset, theText.IndexOf('>', closerPos + 1));				
			} 
			return closerPos;
		}

		int findPosOfCloser(int offset)
        {
			return findPosOfCloser(offset, theText.IndexOf('>', offset));
		}

        /// <summary>
        /// This ignores xml tags butted between text.
        /// </summary>
		public override int Following(int offset) 
        {
			bool finished = false;
			int[] res;

            // MD 6/23/08 - BR34114
            // The if an end XML tag or an '&' escaped non-whitespace character is encountered by FollowingProxy it will return 
            // that it is not finished and to start at the next character. However, if it was encountered at the end of the text,
            // going to the next character would cause an exception, so only continue if the offset is valid in the string.
            //while(!finished){
            while (!finished && offset < theText.Length)
            {
                res = FollowingProxy(offset);
				 finished = res[0] == 1;
				 offset = res[1];
			}
			return offset;
		}


		// Need to use this proxy because although recursion was nice it can cause stackoverflows with lots of X
		private int[] FollowingProxy(int offset) 
        {	
			if (allowXML)
            {

				int posOfCloser;
				int followingWouldBe = base.Following(offset);
				int posOfOpener;
				if ((posOfOpener = theText.IndexOf('<', offset, followingWouldBe - offset)) > -1)
                {
					// there is an opening XML tag in between offset and supposed following.
					if ((posOfCloser = findPosOfCloser(posOfOpener)) > -1)
                    {
						// ignore the tag
						if (isAtNonWhiteSpace(offset)){
							// started at a letter, so may want to end at whitespace
							if (posOfCloser < theText.Length - 1 && isAtNonWhiteSpace(posOfCloser + 1))
                            {
								// started on a letter and ended on a letter, if the tag was
								// text style (<font><b><u><i></b></font></u></i>) we should forward to end of letters
								// because text style tags shouldnt break words
								if (theText.ToLower().IndexOf("font", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("strong", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("basefont", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<big>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</big>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<small>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</small>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<tt>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</tt>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<em>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</em>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<pre>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</pre>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<b>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("</b>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("<s>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("</s>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("<i>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("</i>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("<u>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("</u>", posOfOpener, posOfCloser - posOfOpener) > -1 ){
									return new int[]{0, posOfCloser + 1};
								} else if (posOfCloser < theText.Length)
                                {
									return new int[]{1, posOfCloser + 1};
								} else {
									return new int[]{1, posOfCloser};
								}
							} else if (posOfCloser < theText.Length)
                            {
								return new int[]{1, posOfCloser + 1};
							} else 
                            {
								return new int[]{1, posOfCloser};
							}
						} else 
                        {
							return new int[]{0, posOfCloser};
						}
					} else {
						return new int[]{1, followingWouldBe};
					}

				}
                else if ((posOfOpener = theText.IndexOf('&', offset, followingWouldBe - offset)) > -1) 
                {//& was moved over

					// there is an opening XML tag in between offset and supposed following and isnt an erroneous &.
					if ((posOfCloser = theText.IndexOf(';', posOfOpener) ) > -1 && (posOfOpener + 1 < theText.Length && Char.IsLetter(theText[posOfOpener + 1])))
                    {
						// _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _
						


						// _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _

						// ignore the tag
						if (isAtNonWhiteSpace(offset)){

							// started at a letter, so may want to end at whitespace
							if (posOfCloser < theText.Length - 1 && isAtNonWhiteSpace(posOfCloser + 1)){
								// started on a letter and ended on a letter, if the tag was
								// text style (<font><b><u><i></b></font></u></i>) we should forward to end of letters
								if (theText.ToLower().IndexOf("nbsp", posOfOpener, posOfCloser - posOfOpener) == -1  
									 
									){
									return new int[]{0, (posOfCloser + 1)};
								} else if (posOfCloser < theText.Length){
									return new int[]{1, posOfCloser + 1};
								} else {
									return new int[]{1, posOfCloser};
								}
							} else if (posOfCloser < theText.Length){
								return new int[]{1, posOfCloser + 1};
							} else {
								return new int[]{1, posOfCloser};
							}
						} else {
							return new int[]{0, posOfCloser};
						}
					} else {
						return new int[]{1, followingWouldBe};
					}

				} else {	// didnt move over any tags, but, might have ended next to one, so look into that
					// dont want to end next to an xml tag if we started on nonwhitespace
					if (followingWouldBe < theText.Length && isAtNonWhiteSpace(offset) && theText[followingWouldBe] == '<'){
						posOfOpener = followingWouldBe;
						if (posOfOpener > -1) posOfCloser = theText.IndexOf('>', posOfOpener);
						else posOfCloser = -1;

						// got to see if text after closer is white space, if it is, don't include tag
						if (posOfCloser > -1){
							if (posOfCloser >= theText.Length - 1 || !isAtNonWhiteSpace(posOfCloser + 1)){
								// closer is at end of text
								return new int[]{1, followingWouldBe};
							}
						}

						if(posOfOpener>-1 && posOfCloser>-1 && (
							theText.ToLower().IndexOf("font", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("strong", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("basefont", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<big>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</big>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<small>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</small>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<tt>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</tt>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<em>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</em>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<pre>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</pre>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<b>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("</b>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("<s>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("</s>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("<i>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("</i>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("<u>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("</u>", posOfOpener, posOfCloser - posOfOpener+1)>-1) ){
							if(posOfCloser < theText.Length-1 && isAtNonWhiteSpace(posOfCloser+1)){

								return new int[]{0,(posOfCloser+1)};
							}  else if (posOfCloser < theText.Length){
								return new int[]{1,posOfCloser+1};
							} else {
								return new int[]{1,posOfCloser};
							}
						}
					} else if(followingWouldBe < theText.Length && isAtNonWhiteSpace(offset) && theText[followingWouldBe]=='&' && (followingWouldBe+1<theText.Length && Char.IsLetter(theText[followingWouldBe+1]))){
						posOfOpener=followingWouldBe;
						if (posOfOpener > -1) posOfCloser = theText.IndexOf(';', posOfOpener);
						else posOfCloser = -1;

						



						int whiteSpaceIndex = theText.IndexOf(' ', posOfOpener);
						if(posOfCloser != -1 && whiteSpaceIndex < posOfCloser )
								return new int[]{1,whiteSpaceIndex};

						if(posOfOpener>-1 && posOfCloser>-1 && (
							theText.ToLower().IndexOf("nbsp", posOfOpener, posOfCloser - posOfOpener+1)==-1 	
								 
						)){

								return new int[]{0,(posOfCloser+1)};
						}  else if (posOfCloser < theText.Length){
								return new int[]{1,posOfOpener};
						} else {
								return new int[]{1,posOfCloser};
						}
					} else {
						return new int[]{1,followingWouldBe};						
					}
					return new int[]{1,followingWouldBe};

				} 
				
			} else {
				return new int[]{1,base.Following(offset)};
			}
		}



        /// <summary>
        /// This ignores xml tags butted between text.
        /// </summary>
		public override int Preceding(int offset) {
			bool finished = false;
			int[] res;
			while(!finished){
				 res = PrecedingProxy(offset);
				 finished = res[0]==1;
				 offset = res[1];
			}
			return offset;
		}


		private int[] PrecedingProxy(int offset){	//needed to avoid recursion

			if(allowXML){

				int posOfOpener;
				int precedingWouldBe = base.Preceding(offset);
				int posOfCloser;

				if(precedingWouldBe>0 && (posOfCloser=theText.IndexOf('>', precedingWouldBe, offset - precedingWouldBe)) > -1){
					//there is an opening XML tag in between offset and supposed following.
					if( ( posOfOpener = theText.LastIndexOf('<', posOfCloser) ) > -1){
						//ignore the tag
						if(isAtNonWhiteSpace(offset)){
							//started at a letter, so may want to end at whitespace
							if(posOfOpener < theText.Length-1 && isAtNonWhiteSpace(posOfOpener+1)){
								//started on a letter and ended on a letter, if the tag was
								//text style (<font><b><u><i></b></font></u></i>) we should backward to end of letters
								if(theText.ToLower().IndexOf("font", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("strong", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("basefont", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<big>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</big>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<small>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</small>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<tt>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</tt>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<em>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</em>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<pre>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("</pre>", posOfOpener, posOfCloser - posOfOpener) > -1 || 
									theText.ToLower().IndexOf("<b>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("</b>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("<s>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("</s>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("<i>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("</i>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("<u>", posOfOpener, posOfCloser - posOfOpener) > -1 ||
									theText.ToLower().IndexOf("</u>", posOfOpener, posOfCloser - posOfOpener) > -1 ){
									return new int[]{0,(posOfOpener)};
								} else if (posOfOpener > 0){
									return new int[]{1,posOfOpener-1};
								} else {
									return new int[]{1,posOfOpener};
								}
							} else if (posOfOpener >0){
								return new int[]{1,posOfOpener-1};
							} else {
								return new int[]{1,posOfOpener};
							}
						} else {
							return new int[]{0,(posOfOpener)};
						}
					} else {
						return new int[]{1,precedingWouldBe};
					}

				} else if (isInsideXMLTag(offset)){
					return new int[]{1,base.Preceding(theText.LastIndexOf('<', offset))};
				} else {
					//dont want to end next to an xml tag if we started on nonwhitespace
					if(precedingWouldBe > 0 && isAtNonWhiteSpace(offset) && theText[precedingWouldBe-1]=='>'){
						posOfCloser=precedingWouldBe;
						if (posOfCloser > -1) posOfOpener = theText.LastIndexOf('<', posOfCloser);
						else posOfOpener = -1;
						if(posOfOpener>-1 && posOfCloser>-1 && (
							theText.ToLower().IndexOf("font", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("strong", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("basefont", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<big>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</big>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<small>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</small>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<tt>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</tt>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<em>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</em>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<pre>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("</pre>", posOfOpener, posOfCloser - posOfOpener+1)>-1 || 
							theText.ToLower().IndexOf("<b>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("</b>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("<s>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("</s>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("<i>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("</i>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("<u>", posOfOpener, posOfCloser - posOfOpener+1)>-1 ||
							theText.ToLower().IndexOf("</u>", posOfOpener, posOfCloser - posOfOpener+1)>-1) ){
							if(posOfOpener > 0 && isAtNonWhiteSpace(posOfOpener-1)){
								return new int[]{0,(posOfOpener)};
							}  else if (posOfOpener > 0){

								return new int[]{1,posOfOpener -1};
							} else {

								return new int[]{1,posOfOpener};
							}
						} else {

							return new int[]{1,precedingWouldBe};						
						}
					} else {

						return new int[]{1,precedingWouldBe};
					}
				}

				
			} else {
				return new int[]{1,base.Preceding(offset)};
			}
		}

		bool isInsideXMLTag(int pos){
			if(pos<=0 || pos >= theText.Length) return false;
			//if there is a < before and a > after current pos, without opposites first, return true
			//Eg, <tag> this position HERE not in <tag>
			//Eg, <tag> this position <HERE> is in <tag>
			//return (  theText.LastIndexOf('<', pos)>-1 && theText.IndexOf('>', pos) >-1  );
			int openTagBeforePos = theText.LastIndexOf('<', pos),
			    closeTagBeforePos = theText.LastIndexOf('>', pos),
			    openTagAfterPos = theText.IndexOf('<', pos),
			    closeTagAfterPos = theText.IndexOf('>', pos);
			return (  
				openTagBeforePos  > -1			&& closeTagAfterPos >-1  && 
				closeTagBeforePos < openTagBeforePos	&& openTagAfterPos  > closeTagAfterPos );
		}

        /// <summary>
        /// Whether the character at the position is between a non whitespace character.
        /// </summary>
		protected override bool isAtNonWhiteSpace(int position){
			if(position==theText.Length)
				return false;
			//treat & as white space in XML
			return ((theText[position] != '&' && allowXML) || !allowXML) && base.isAtNonWhiteSpace(position);
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