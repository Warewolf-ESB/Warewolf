using System;

namespace Infragistics.SpellChecker
{
	///<summary>Gets permutations (stores small n=1,2,3,4,5 for quick access), this is used to find anagrams for suggestions</summary>
	internal class Permuter{

		int  NN;//, i, count=0 ;
		int[]  p;// = new int[100];
		int[]  pi;//= new int[100];	  /* The permutation and its inverse */
		int[]  dir;//=new int[100];          /* The directions of each element  */

		int[][] permutations;		//set of permuations
		int ptr;

		int[][] p1 = new int[1][];
		int[][] p2 = new int[2][];

        /// <summary>Generates all permutations of NN numbers.</summary>
        /// <remarks>E.g. if NN = 3 this will generate 123,132,213,231,312,321 (not in typographic order, but transposition order).</remarks>
		public Permuter(int NN){
			int numPerms = factorial(NN);
			permutations = new int[numPerms][];

			if(NN == 1){
				permutations[0] = new int[]{1};
			} else if(NN==2){
				permutations[0] = new int[]{1,2};
				permutations[1] = new int[]{2,1};
			} else if(NN==3){
				permutations[0] = new int[]{1,2,3};
				permutations[1] = new int[]{1,3,2};
				permutations[2] = new int[]{3,1,2};
				permutations[3] = new int[]{3,2,1};
				permutations[4] = new int[]{2,3,1};
				permutations[5] = new int[]{2,1,3};
			} else if(NN==4){
				permutations[0] = new int[]{1,2,3,4};
				permutations[1] = new int[]{1,2,4,3};
				permutations[2] = new int[]{1,4,2,3};
				permutations[3] = new int[]{4,1,2,3};
				permutations[4] = new int[]{4,1,3,2};
				permutations[5] = new int[]{1,4,3,2};
				permutations[6] = new int[]{1,3,4,2};
				permutations[7] = new int[]{1,3,2,4};
				permutations[8] = new int[]{3,1,2,4};
				permutations[9] = new int[]{3,1,4,2};
				permutations[10] = new int[]{3,4,1,2};
				permutations[11] = new int[]{4,3,1,2};
				permutations[12] = new int[]{4,3,2,1};
				permutations[13] = new int[]{3,4,2,1};
				permutations[14] = new int[]{3,2,4,1};
				permutations[15] = new int[]{3,2,1,4};
				permutations[16] = new int[]{2,3,1,4};
				permutations[17] = new int[]{2,3,4,1};
				permutations[18] = new int[]{2,4,3,1};
				permutations[19] = new int[]{4,2,3,1};
				permutations[20] = new int[]{4,2,1,3};
				permutations[21] = new int[]{2,4,1,3};
				permutations[22] = new int[]{2,1,4,3};
				permutations[23] = new int[]{2,1,3,4};
			} else if(NN==5){
				permutations[0] = new int[]{1,2,3,4,5};
				permutations[1] = new int[]{1,2,3,5,4};
				permutations[2] = new int[]{1,2,5,3,4};
				permutations[3] = new int[]{1,5,2,3,4};
				permutations[4] = new int[]{5,1,2,3,4};
				permutations[5] = new int[]{5,1,2,4,3};
				permutations[6] = new int[]{1,5,2,4,3};
				permutations[7] = new int[]{1,2,5,4,3};
				permutations[8] = new int[]{1,2,4,5,3};
				permutations[9] = new int[]{1,2,4,3,5};
				permutations[10] = new int[]{1,4,2,3,5};
				permutations[11] = new int[]{1,4,2,5,3};
				permutations[12] = new int[]{1,4,5,2,3};
				permutations[13] = new int[]{1,5,4,2,3};
				permutations[14] = new int[]{5,1,4,2,3};
				permutations[15] = new int[]{5,4,1,2,3};
				permutations[16] = new int[]{4,5,1,2,3};
				permutations[17] = new int[]{4,1,5,2,3};
				permutations[18] = new int[]{4,1,2,5,3};
				permutations[19] = new int[]{4,1,2,3,5};
				permutations[20] = new int[]{4,1,3,2,5};
				permutations[21] = new int[]{4,1,3,5,2};
				permutations[22] = new int[]{4,1,5,3,2};
				permutations[23] = new int[]{4,5,1,3,2};
				permutations[24] = new int[]{5,4,1,3,2};
				permutations[25] = new int[]{5,1,4,3,2};
				permutations[26] = new int[]{1,5,4,3,2};
				permutations[27] = new int[]{1,4,5,3,2};
				permutations[28] = new int[]{1,4,3,5,2};
				permutations[29] = new int[]{1,4,3,2,5};
				permutations[30] = new int[]{1,3,4,2,5};
				permutations[31] = new int[]{1,3,4,5,2};
				permutations[32] = new int[]{1,3,5,4,2};
				permutations[33] = new int[]{1,5,3,4,2};
				permutations[34] = new int[]{5,1,3,4,2};
				permutations[35] = new int[]{5,1,3,2,4};
				permutations[36] = new int[]{1,5,3,2,4};
				permutations[37] = new int[]{1,3,5,2,4};
				permutations[38] = new int[]{1,3,2,5,4};
				permutations[39] = new int[]{1,3,2,4,5};
				permutations[40] = new int[]{3,1,2,4,5};
				permutations[41] = new int[]{3,1,2,5,4};
				permutations[42] = new int[]{3,1,5,2,4};
				permutations[43] = new int[]{3,5,1,2,4};
				permutations[44] = new int[]{5,3,1,2,4};
				permutations[45] = new int[]{5,3,1,4,2};
				permutations[46] = new int[]{3,5,1,4,2};
				permutations[47] = new int[]{3,1,5,4,2};
				permutations[48] = new int[]{3,1,4,5,2};
				permutations[49] = new int[]{3,1,4,2,5};
				permutations[50] = new int[]{3,4,1,2,5};
				permutations[51] = new int[]{3,4,1,5,2};
				permutations[52] = new int[]{3,4,5,1,2};
				permutations[53] = new int[]{3,5,4,1,2};
				permutations[54] = new int[]{5,3,4,1,2};
				permutations[55] = new int[]{5,4,3,1,2};
				permutations[56] = new int[]{4,5,3,1,2};
				permutations[57] = new int[]{4,3,5,1,2};
				permutations[58] = new int[]{4,3,1,5,2};
				permutations[59] = new int[]{4,3,1,2,5};
				permutations[60] = new int[]{4,3,2,1,5};
				permutations[61] = new int[]{4,3,2,5,1};
				permutations[62] = new int[]{4,3,5,2,1};
				permutations[63] = new int[]{4,5,3,2,1};
				permutations[64] = new int[]{5,4,3,2,1};
				permutations[65] = new int[]{5,3,4,2,1};
				permutations[66] = new int[]{3,5,4,2,1};
				permutations[67] = new int[]{3,4,5,2,1};
				permutations[68] = new int[]{3,4,2,5,1};
				permutations[69] = new int[]{3,4,2,1,5};
				permutations[70] = new int[]{3,2,4,1,5};
				permutations[71] = new int[]{3,2,4,5,1};
				permutations[72] = new int[]{3,2,5,4,1};
				permutations[73] = new int[]{3,5,2,4,1};
				permutations[74] = new int[]{5,3,2,4,1};
				permutations[75] = new int[]{5,3,2,1,4};
				permutations[76] = new int[]{3,5,2,1,4};
				permutations[77] = new int[]{3,2,5,1,4};
				permutations[78] = new int[]{3,2,1,5,4};
				permutations[79] = new int[]{3,2,1,4,5};
				permutations[80] = new int[]{2,3,1,4,5};
				permutations[81] = new int[]{2,3,1,5,4};
				permutations[82] = new int[]{2,3,5,1,4};
				permutations[83] = new int[]{2,5,3,1,4};
				permutations[84] = new int[]{5,2,3,1,4};
				permutations[85] = new int[]{5,2,3,4,1};
				permutations[86] = new int[]{2,5,3,4,1};
				permutations[87] = new int[]{2,3,5,4,1};
				permutations[88] = new int[]{2,3,4,5,1};
				permutations[89] = new int[]{2,3,4,1,5};
				permutations[90] = new int[]{2,4,3,1,5};
				permutations[91] = new int[]{2,4,3,5,1};
				permutations[92] = new int[]{2,4,5,3,1};
				permutations[93] = new int[]{2,5,4,3,1};
				permutations[94] = new int[]{5,2,4,3,1};
				permutations[95] = new int[]{5,4,2,3,1};
				permutations[96] = new int[]{4,5,2,3,1};
				permutations[97] = new int[]{4,2,5,3,1};
				permutations[98] = new int[]{4,2,3,5,1};
				permutations[99] = new int[]{4,2,3,1,5};
				permutations[100] = new int[]{4,2,1,3,5};
				permutations[101] = new int[]{4,2,1,5,3};
				permutations[102] = new int[]{4,2,5,1,3};
				permutations[103] = new int[]{4,5,2,1,3};
				permutations[104] = new int[]{5,4,2,1,3};
				permutations[105] = new int[]{5,2,4,1,3};
				permutations[106] = new int[]{2,5,4,1,3};
				permutations[107] = new int[]{2,4,5,1,3};
				permutations[108] = new int[]{2,4,1,5,3};
				permutations[109] = new int[]{2,4,1,3,5};
				permutations[110] = new int[]{2,1,4,3,5};
				permutations[111] = new int[]{2,1,4,5,3};
				permutations[112] = new int[]{2,1,5,4,3};
				permutations[113] = new int[]{2,5,1,4,3};
				permutations[114] = new int[]{5,2,1,4,3};
				permutations[115] = new int[]{5,2,1,3,4};
				permutations[116] = new int[]{2,5,1,3,4};
				permutations[117] = new int[]{2,1,5,3,4};
				permutations[118] = new int[]{2,1,3,5,4};
				permutations[119] = new int[]{2,1,3,4,5};

			} else {
				//generate 
				ptr=0;
				p = new int[NN+1];
				pi= new int[NN+1];	  
				dir=new int[NN+1];          

				this.NN = NN;

				for (int i=1; i<=NN; ++i) {
				dir[i] = -1;  
				p[i] = i;  pi[i] = i;
				}
				Perm ( 1 );
			}
		}

        /// <summary>
        /// Returns a permutation of a zero based array at index p.  If p&gt; number of permutations stored this will return the last permutation in the set.
        /// </summary>
		public int[] GetPermutation(int p){
			if(p<permutations.Length)
				return permutations[p];
			else return permutations[permutations.Length-1];
		}

        /// <summary>
        /// Gets total number of permutations.
        /// </summary>
		public int GetNumberOfPermutations(){
			return permutations.Length;
		}


		void PrintPerm() {
		  for (int i=1; i <= NN; ++i) Console.Write(  p[i] );
		  Console.WriteLine();
		}

		void RecordPerm(){
			permutations[ptr] = new int[NN];
			for (int i=0; i < NN; ++i)
				permutations[ptr][i] = p[i+1];
			ptr++;
		}

		void Move( int x, int d ) {
		  int z;
		  z = p[pi[x]+d];
		  p[pi[x]] = z;
		  p[pi[x]+d] = x;
		  pi[z] = pi[x];
		  pi[x] = pi[x]+d;  
		}

		void Perm ( int n ) { 
		  int i;
		  if (n > NN) RecordPerm();
		  else {
		    Perm( n+1 );
		    for (i=1; i<=n-1; ++i) {
		       Move( n, dir[n] );  Perm( n+1 ); 
		    }
		    dir[n] = -dir[n];
		  }
		}

		int factorial(int n){
			int res = 1;
			for(int i=n; i>1; --i)
				res = res * i;
			return res;
		}



		public static void Main(String[] args) {
			Console.WriteLine( "Enter n: " );  
			int n = Int32.Parse(Console.ReadLine());
			Console.WriteLine( "\n" );

			Permuter p = new Permuter(n);
			for(int perm=0; perm<p.GetNumberOfPermutations(); perm++){
				for (int i=0; i < n; ++i) Console.Write(  p.GetPermutation(perm)[i] );
				Console.WriteLine();			
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