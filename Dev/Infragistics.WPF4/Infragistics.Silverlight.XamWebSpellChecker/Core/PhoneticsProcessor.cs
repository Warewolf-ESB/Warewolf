using System;
using System.Text;

namespace Infragistics.SpellChecker
{
	///////
	/// Generates the phonetic (sounds like) code for a word.
	////
	internal class PhoneticsProcessor {
		static int maxCodeLen = 6;
		static String vowels = "AEIOU";
		static String frontv = "EIY";
		static String varson = "CSPTG";
		
		///////
		/// @param txt the word to find the phonetic code for
		/// @return String the phonetic code for the txt
		public static String MetaPhone(String txt) 
		{
			int mtsz = 0  ;
			bool hard = false ;
			if(( txt == null ) || ( txt.Length == 0 )) return "" ;
			// single character is itself
			if( txt.Length == 1 ) return txt.ToUpper() ;
			//
			char[] inwd = txt.ToUpper().ToCharArray() ;
			//
			String tmpS ;
			StringBuilder local = new StringBuilder( 40 ); // manipulate
			StringBuilder code = new StringBuilder( 10 ) ; //   output




			// handle initial 2 characters exceptions
			switch( inwd[0] ){
				case 'K': case 'G' : case 'P' : 
				  if( inwd[1] == 'N')local.Append(inwd, 1, inwd.Length - 1 );
				  else local.Append( inwd );
				  break;
				case 'A': 
				  if( inwd[1] == 'E' )local.Append(inwd, 1, inwd.Length - 1 );
				  else local.Append( inwd );
				  break;
				case 'W' : 
				  if( inwd[1] == 'R' ){   // WR -> R
					local.Append(inwd, 1, inwd.Length - 1 ); break ;
				  }
				  if( inwd[1] == 'H'){
					local.Append(inwd, 1, inwd.Length - 1 );
					local[0] = 'W'; // WH -> W
				  }
				  else local.Append( inwd );
				  break;
				case 'X' : 
				  inwd[0] = 'S' ;local.Append( inwd );
				  break ;
				default :
				  local.Append( inwd );
				  break;
			} // now local has working string with initials fixed






			int wdsz = local.Length;
			int n = 0 ;
			while((mtsz < maxCodeLen ) && (n < wdsz ) ){// max code size of 4 works well

				char symb = local[n] ;

				// remove duplicate letters except C
				if(( symb != 'C' ) &&
				   (n > 0 ) && ( local[n - 1] == symb )) n++ ;
				else{ // not dup


				  //very long switch
				  switch( symb ){

					case 'A' : case 'E' : case 'I' : case 'O' : case 'U' :
					  if( n == 0 ) { code.Append(symb );mtsz++;
					  }
					  break ; // only use vowel if leading char


					case 'B' :
					  if( (n > 0 ) &&
						  !(n + 1 == wdsz ) && // not MB at end of word
						  ( local[n - 1] == 'M')) {
							code.Append(symb);
						  }
					  else code.Append(symb);
					  mtsz++ ;
					  break ;


					case 'C' : // lots of C special cases
					  
					  if( ( n > 0 ) &&
						  ( local[n-1] == 'S' ) &&
						  ( n + 1 < wdsz ) &&
						  ( frontv.IndexOf( local[n + 1]) >= 0 )){ break ;}
					  tmpS = local.ToString();
					  if( tmpS.IndexOf("CIA", n ) == n ) { // "CIA" -> X
						 code.Append('X' ); mtsz++; break ;
					  }
					  if( ( n + 1 < wdsz ) &&
						  (frontv.IndexOf( local[n+1] )>= 0 )){
						 code.Append('S');mtsz++; break ; // CI,CE,CY -> S
					  }
					  if(( n > 0) &&
						 ( tmpS.IndexOf("SCH",n-1 )== n-1 )){ // SCH->sk
						 code.Append('K') ; mtsz++;break ;
					  }
					  if( tmpS.IndexOf("CH", n ) == n ){ // detect CH
						if((n == 0 ) &&
						   (wdsz >= 3 ) &&    // CH consonant -> K consonant
						   (vowels.IndexOf( local[2] ) < 0 )){
							 code.Append('K');
						}
						else { code.Append('X'); // CHvowel -> X
						}
						mtsz++;
					  }
					  else { code.Append('K' );mtsz++;
					  }
					  break ;


					case 'D' :
					  if(( n + 2 < wdsz )&&  // DGE DGI DGY -> J
						 ( local[n+1] == 'G' )&&
						 (frontv.IndexOf( local[n+2] )>= 0)){
							code.Append('J' ); n += 2 ;
					  }
					  else { code.Append( 'T' );
					  }
					  mtsz++;
					  break ;


					case 'G' : // GH silent at end or before consonant
					  if(( n + 2 == wdsz )&&
						 (local[n+1] == 'H' )) break ;
					  if(( n + 2 < wdsz ) &&
						 (local[n+1] == 'H' )&&
						 (vowels.IndexOf( local[n+2]) < 0 )) break ;
					  tmpS = local.ToString();
					  if((n > 0) &&
						 ( tmpS.IndexOf("GN", n ) == n)||
						 ( tmpS.IndexOf("GNED",n) == n )) break ; // silent G
					  if(( n > 0 ) &&
						 (local[n-1] == 'G')) hard = true ;
					  else hard = false ;
					  if((n+1 < wdsz) &&
						 (frontv.IndexOf( local[n+1] ) >= 0 )&&
						 (!hard) ) code.Append( 'J' );
					  else code.Append('K');
					  mtsz++;
					  break ;


					case 'H':
					  if( n + 1 == wdsz ) break ; // terminal H
					  if((n > 0) &&
						 (varson.IndexOf( local[n-1]) >= 0)) break ;
					  if( vowels.IndexOf( local[n+1]) >=0 ){
						  code.Append('H') ; mtsz++;// Hvowel
					  }
					  break;


					case 'F': case 'J' : case 'L' :
					case 'M': case 'N' : case 'R' :
					  code.Append( symb ); mtsz++; break ;


					case 'K' :
					  if( n > 0 ){ // not initial
						if( local[ n -1] != 'C' ) {
							 code.Append(symb );
						}
					  }
					  else   code.Append( symb ); // initial K
					  mtsz++ ;
					  break ;


					case 'P' :
					  if((n + 1 < wdsz) &&  // PH -> F
						 (local[ n+1] == 'H'))code.Append('F');
					  else code.Append( symb );
					  mtsz++;
					  break ;


					case 'Q' :
					  code.Append('K' );mtsz++; break ;


					case 'S' :
					  tmpS = local.ToString();
					  if((tmpS.IndexOf("SH", n )== n) ||
						 (tmpS.IndexOf("SIO",n )== n) ||
						 (tmpS.IndexOf("SIA",n )== n)) code.Append('X');
					  else code.Append( 'S' );
					  mtsz++ ;
					  break ;


					case 'T' :
					  tmpS = local.ToString(); // TIA TIO -> X
					  if((tmpS.IndexOf("TIA",n )== n)||
						 (tmpS.IndexOf("TIO",n )== n) ){
							code.Append('X'); mtsz++; break;
					  }
					  if( tmpS.IndexOf("TCH",n )==n) break;
					  // substitute numeral 0 for TH (resembles theta after all)
					  if( tmpS.IndexOf("TH", n )==n) code.Append('0');
					  else code.Append( 'T' );
					  mtsz++ ;
					  break ;


					case 'V' :
					  code.Append('F'); mtsz++;break ;


					case 'W' : case 'Y' : // silent if not followed by vowel
					  if((n+1 < wdsz) &&
						 (vowels.IndexOf( local[n+1])>=0)){
							code.Append( symb );mtsz++;
					  }
					  break ;


					case 'X' :
					  code.Append('K'); code.Append('S');mtsz += 2;
					  break ;


					case 'Z' :
					  code.Append('S'); mtsz++; break ;


				  } // end switch




				  n++ ;


				} // end else from symb != 'C'

//				if( mtsz > maxCodeLen )code.setLength( maxCodeLen);
				if( mtsz > maxCodeLen )code.Length = maxCodeLen;

			}
			return code.ToString();
		}
		
		/////// is more efficient than finding the metaphone of word and then testing if strings	are equal.
		/// @param txt word to check if has same metaphone as
		/// @param metaphone the meta phone to compare with
		/// @return boolean true if txt has same metaphone
		public static bool HasSameMetaPhone(String txt, String metaphone) 
		{
			int mtsz = 0  ;
			bool hard = false ;
			if(( txt == null ) || ( txt.Length == 0 )) return false ;
			// single character is itself
			if( txt.Length == 1 && txt.ToUpper().Equals(metaphone)) return true ; 
			else if( txt.Length == 1) 			return false ;
			//
			char[] inwd = txt.ToUpper().ToCharArray() ;
			//
			String tmpS ;
			StringBuilder local = new StringBuilder( 40 ); // manipulate
			StringBuilder code = new StringBuilder( 10 ) ; //   output




			// handle initial 1 or 2 character exceptions
			// eg word like, knife, gnaw, wright...
			switch( inwd[0] ){
				case 'K': case 'G' : case 'P' : 
				  if( inwd[1] == 'N')local.Append(inwd, 1, inwd.Length - 1 );
				  else local.Append( inwd );
				  break;

				case 'A': 
				  if( inwd[1] == 'E' )local.Append(inwd, 1, inwd.Length - 1 );
				  else local.Append( inwd );
				  break;

				case 'W' : 
				  if( inwd[1] == 'R' ){   // WR -> R
					local.Append(inwd, 1, inwd.Length - 1 ); break ;
				  }
				  if( inwd[1] == 'H'){
					local.Append(inwd, 1, inwd.Length - 1 );
					local[0]='W'; // WH -> W
				  }
				  else local.Append( inwd );
				  break;

				case 'X' : 
				  inwd[0] = 'S' ;local.Append( inwd );
				  break ;
				default :
				  local.Append( inwd );
				  break;
			} // now local has working string with initials fixed






			int wdsz = local.Length;
			int n = 0 ;
			while((mtsz < maxCodeLen ) && (n < wdsz ) ){// max code size of 4 works well
			
				if(code.Length > metaphone.Length){
					//can't be it
					return false;
				}

				//dont bother doing any more if last (so far) letter of code isn't the same as one in (argument) metaphone
				if(code.Length > 0){
					if (code[code.Length-1] != metaphone[code.Length-1]){
						return false;
					}
				}

				//look at each char in local
				char symb = local[n] ;

				// remove duplicate letters except C
				if(( symb != 'C' ) && (n > 0 ) && ( local[n - 1] == symb )) {
					n++ ;
				}
				else{ // not dup


				  //very long switch
				  switch( symb ){

					case 'A' : case 'E' : case 'I' : case 'O' : case 'U' :
					  if( n == 0 ) { code.Append(symb );mtsz++;
					  }
					  break ; // only use vowel if leading char


					case 'B' :
					  if( (n > 0 ) &&
						  !(n + 1 == wdsz ) && // not MB at end of word
						  ( local[n - 1] == 'M')) {
							code.Append(symb);
						  }
					  else code.Append(symb);
					  mtsz++ ;
					  break ;


					case 'C' : // lots of C special cases
					  
					  if( ( n > 0 ) &&
						  ( local[n-1] == 'S' ) &&
						  ( n + 1 < wdsz ) &&
						  ( frontv.IndexOf( local[n + 1]) >= 0 )){ break ;}
					  tmpS = local.ToString();
					  if( tmpS.IndexOf("CIA", n ) == n ) { // "CIA" -> X
						 code.Append('X' ); mtsz++; break ;
					  }
					  if( ( n + 1 < wdsz ) &&
						  (frontv.IndexOf( local[n+1] )>= 0 )){
						 code.Append('S');mtsz++; break ; // CI,CE,CY -> S
					  }
					  if(( n > 0) &&
						 ( tmpS.IndexOf("SCH",n-1 )== n-1 )){ // SCH->sk
						 code.Append('K') ; mtsz++;break ;
					  }
					  if( tmpS.IndexOf("CH", n ) == n ){ // detect CH
						if((n == 0 ) &&
						   (wdsz >= 3 ) &&    // CH consonant -> K consonant
						   (vowels.IndexOf( local[ 2] ) < 0 )){
							 code.Append('K');
						}
						else { code.Append('X'); // CHvowel -> X
						}
						mtsz++;
					  }
					  else { code.Append('K' );mtsz++;
					  }
					  break ;


					case 'D' :
					  if(( n + 2 < wdsz )&&  // DGE DGI DGY -> J
						 ( local[n+1] == 'G' )&&
						 (frontv.IndexOf( local[n+2] )>= 0)){
							code.Append('J' ); n += 2 ;
					  }
					  else { code.Append( 'T' );
					  }
					  mtsz++;
					  break ;


					case 'G' : // GH silent at end or before consonant
					  if(( n + 2 == wdsz )&&
						 (local[n+1] == 'H' )) break ;
					  if(( n + 2 < wdsz ) &&
						 (local[n+1] == 'H' )&&
						 (vowels.IndexOf( local[n+2]) < 0 )) break ;
					  tmpS = local.ToString();
					  if((n > 0) &&
						 ( tmpS.IndexOf("GN", n ) == n)||
						 ( tmpS.IndexOf("GNED",n) == n )) break ; // silent G
					  if(( n > 0 ) &&
						 (local[n-1] == 'G')) hard = true ;
					  else hard = false ;
					  if((n+1 < wdsz) &&
						 (frontv.IndexOf( local[n+1] ) >= 0 )&&
						 (!hard) ) code.Append( 'J' );
					  else code.Append('K');
					  mtsz++;
					  break ;


					case 'H':
					  if( n + 1 == wdsz ) break ; // terminal H
					  if((n > 0) &&
						 (varson.IndexOf( local[n-1]) >= 0)) break ;
					  if( vowels.IndexOf( local[n+1]) >=0 ){
						  code.Append('H') ; mtsz++;// Hvowel
					  }
					  break;


					case 'F': case 'J' : case 'L' :
					case 'M': case 'N' : case 'R' :
					  code.Append( symb ); mtsz++; break ;


					case 'K' :
					  if( n > 0 ){ // not initial
						if( local[ n -1] != 'C' ) {
							 code.Append(symb );
						}
					  }
					  else   code.Append( symb ); // initial K
					  mtsz++ ;
					  break ;


					case 'P' :
					  if((n + 1 < wdsz) &&  // PH -> F
						 (local[ n+1] == 'H'))code.Append('F');
					  else code.Append( symb );
					  mtsz++;
					  break ;


					case 'Q' :
					  code.Append('K' );mtsz++; break ;


					case 'S' :
					  tmpS = local.ToString();
					  if((tmpS.IndexOf("SH", n )== n) ||
						 (tmpS.IndexOf("SIO",n )== n) ||
						 (tmpS.IndexOf("SIA",n )== n)) code.Append('X');
					  else code.Append( 'S' );
					  mtsz++ ;
					  break ;


					case 'T' :
					  tmpS = local.ToString(); // TIA TIO -> X
					  if((tmpS.IndexOf("TIA",n )== n)||
						 (tmpS.IndexOf("TIO",n )== n) ){
							code.Append('X'); mtsz++; break;
					  }
					  if( tmpS.IndexOf("TCH",n )==n) break;
					  // substitute numeral 0 for TH (resembles theta after all)
					  if( tmpS.IndexOf("TH", n )==n) code.Append('0');
					  else code.Append( 'T' );
					  mtsz++ ;
					  break ;


					case 'V' :
					  code.Append('F'); mtsz++;break ;


					case 'W' : case 'Y' : // silent if not followed by vowel
					  if((n+1 < wdsz) &&
						 (vowels.IndexOf( local[n+1])>=0)){
							code.Append( symb );mtsz++;
					  }
					  break ;


					case 'X' :
					  code.Append('K'); code.Append('S');mtsz += 2;
					  break ;


					case 'Z' :
					  code.Append('S'); mtsz++; break ;


				  } // end switch




				  n++ ;


				} // end else from symb != 'C'

				if ( mtsz > maxCodeLen ) code.Length = maxCodeLen;

			}

			if(code.ToString().Equals(metaphone)){
				return true;
			} else {
				return false;
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