using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.StructuredStorage;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

using System.Drawing;
using Infragistics.Shared;
using Microsoft.VisualBasic;


namespace Infragistics.Documents.Excel
{
    internal static class Utilities
	{
		#region Constants




		public const string RootEmbeddedResourcePath = "Infragistics.Documents.Excel.";


		private const int EMUsPerPoint = 12700;
		internal const int TwipsPerPoint = 20;

		private const uint RKValueIsMultipliedBy100 =	0x00000001;
		private const uint RKValueIsSignedInt =			0x00000002;

		// MD 5/7/12 - TFS106831
		#region Old Code

		//// MBS 8/4/08 - Excel 2007 Format
		//// MD 1/26/12 - 12.1 - Cell Format Updates
		////private const int HLSMAX = 255;
		//
		//private const int RGBMAX = 255;
		//
		//// MD 1/26/12 - 12.1 - Cell Format Updates
		////private const int UNDEFINED = (HLSMAX * 2 / 3);

		#endregion // Old Code
		internal const int HLSMAX = 240;
		private const int RGBMAX = 255;
		private const int UNDEFINED = (HLSMAX * 2 / 3);

		private static readonly int[] Primes = new int[] { 
			2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 
			73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 
			157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 
			239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 
			331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 
			421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 
			509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607, 
			613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 
			709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811, 
			821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 
			919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997 };

		// MD 10/27/10 - TFS56976
		// We don't need these anymore.
        //// MBS 9/9/08 - Excel 2007
        //private static Regex externalFormulaRegex;
        //private static Regex namedReferenceRegex;

		#endregion Constants

        // MBS 8/6/08 - Excel 2007 Format
        #region ApplyTint

		// MD 5/7/12 - TFS106831
		// Rewrote this based on feedback from Microsoft.
		#region Old Code

		//public static Color ApplyTint(Color baseColor, double tint)
		//{
		//    if (tint == 0)
		//        return baseColor;

		//    // MD 1/26/12 - 12.1 - Cell Format Updates
		//    //int h, s, l;
		//    double h, s, l;

		//    Utilities.ConvertRGBToHLS(baseColor, out h, out l, out s);

		//    // Formulas as specified on page 2036
		//    // MD 1/26/12 - 12.1 - Cell Format Updates
		//    //int newL = l;
		//    //if (tint < 0)
		//    //    newL = (int)Math.Round(l * (1 + tint));
		//    //else if (tint > 0)
		//    //    newL = (int)Math.Round(l * (1 - tint) + (HLSMAX - HLSMAX * (1 - tint)));
		//    double newL = l;
		//    if (tint < 0)
		//        newL = l * (1 + tint);
		//    else if (tint > 0)
		//        newL = l * (1 - tint) + tint;

		//    return Utilities.ConvertHLSToRGB(h, newL, s);
		//}

		#endregion // Old Code
		public static Color ApplyTint(Color baseColor, double tint)
		{
			if (tint == 0)
				return baseColor;

			int h, s, l;
			Utilities.ConvertRGBToHLS(baseColor, out h, out l, out s);

			// Formulas as specified on page 2036
			int newL = l;
			if (tint < 0)
				newL = (int)MathUtilities.Truncate(l * (1 + tint));
			else if (tint > 0)
				newL = (int)MathUtilities.Truncate(l * (1 - tint)) + (HLSMAX - (int)MathUtilities.Truncate(HLSMAX * (1 - tint)));

			return Utilities.ConvertHLSToRGB(h, newL, s);
		}

        #endregion //ApplyTint

		// MBS 9/9/08 - Excel 2007
		#region BuildLoadingFormulaReferenceString	

		// MD 10/27/10 - TFS56976
		// This code is incorrect. We don't tell what kind of formula it is by its contents. We tell by what kind of object owns it.
		#region Old Code
		//public static string BuildLoadingFormulaReferenceString(string value, WorkbookSerializationManager manager, out bool isNamedReferenceFormula, out bool isExternalFormula)
		//{
		//    isNamedReferenceFormula = false;
		//    isExternalFormula = false;
		//
		//    if (String.IsNullOrEmpty(value))
		//        return value;
		//
		//    // We need to replace index references with the actual links that we find when
		//    // loading the external references
		//    if (ExternalFormulaRegex.IsMatch(value))
		//    {
		//        Match match = ExternalFormulaRegex.Match(value);
		//        string indexVal = match.Groups["Index"].Value;
		//        if (indexVal.Length > 0)
		//        {
		//            int index = int.Parse(indexVal);
		//            if (manager.WorkbookReferences.Count >= index)
		//            {
		//                string sheetName = match.Groups["Sheet"].Value;
		//                string formulaName = match.Groups["Name"].Value;
		//                if (formulaName.Length > 0)
		//                {
		//                    string fullWorkbookPath = String.Empty;
		//                    if (index > 0)
		//                    {
		//                        isExternalFormula = true;
		//                        fullWorkbookPath = manager.WorkbookReferences[index - 1].FileName;
		//                    }
		//                    else
		//                    {
		//                        // MD 10/27/10 - TFS56976
		//                        // We don't really need to know when it is a named reference formula.
		//                        //isNamedReferenceFormula = true;
		//
		//                        fullWorkbookPath = manager.FilePath ?? String.Empty;
		//                    }
		//
		//                    if (fullWorkbookPath.Length > 0)
		//                    {
		//                        if (sheetName.Length > 0)
		//                        {
		//                            // If a reference has a scope of a particular worksheet, then we'll have the
		//                            // workbook itself in brackets, following by the sheet name
		//                            // (e.g. "='C:\[Book2.xlsx]Sheet1'!MyNamedReference")
		//                            string workbookName = Path.GetFileName(fullWorkbookPath);
		//                            string workbookDir = Path.GetDirectoryName(fullWorkbookPath);
		//                            value = String.Format("='{0}\\[{1}]{2}'!{3}", workbookDir, workbookName, sheetName, formulaName);
		//                        }
		//                        else
		//                            value = String.Format("='{0}'!{1}", fullWorkbookPath, formulaName);
		//                    }
		//                    else if (sheetName.Length > 0)
		//                        value = String.Format("={0}!{1}", sheetName, formulaName);
		//                    else
		//                        value = formulaName;
		//                }
		//                else
		//                    Utilities.DebugFail("The specified reference does not contain a formula");
		//            }
		//            else
		//                Utilities.DebugFail("Encountered an index that doesn't not match a workbook reference");
		//        }
		//    }
		//    else
		//    {
		//        // See if this is at least a named reference
		//        if (NamedReferenceRegex.IsMatch(value))
		//            isNamedReferenceFormula = true;
		//    }
		//
		//    // All formulas need to start with '='
		//    if (value.StartsWith("=") == false)
		//        value = value.Insert(0, "=");
		//
		//    return value;
		//}
		#endregion // Old Code
		public static string BuildLoadingFormulaReferenceString(string value)
		{
			// All formulas need to start with '=' if they have content.
			if (String.IsNullOrEmpty(value) == false && value.StartsWith("=") == false)
				value = value.Insert(0, "=");

			return value;
		}

		#endregion //BuildLoadingFormulaReferenceString 

        #region BuildSavingFormulaReferenceString

        public static string BuildSavingFormulaReferenceString(Formula formula, Excel2007WorkbookSerializationManager manager)
        {
            string formulaString = String.Empty;

			XLSXFormulaStringGenerator generator = new XLSXFormulaStringGenerator(formula, manager.ExternalReferences);
			EvaluationResult<string> result = generator.Evaluate();
			if (result.Completed == false)
			{
				Utilities.DebugFail("Evaluation of external reference failed:\n\n" + result.Result);
				return string.Empty;
			}
			
			formulaString = result.Result;
			if (formulaString.StartsWith("="))
				formulaString = formulaString.Substring(1);

			return formulaString;
        }

        #endregion //BuildSavingFormulaReferenceString

		// MD 11/1/10
		// Found while fixing TFS56976
		// The logic in Worksheet.AddCachedRegion was incorrect because it was just doing a binary search in a list of 
		// weak references without accounting for the fact that the entries could get released, which would mess up the
		// sort order and cause binary searches to fail. This method fixes up the sort order of lists of weak references 
		// while binary searching by removing weak references for released objects.
		#region BinarySearchWeakReferences

		// MD 2/29/12 - 12.1 - Table Support
		//public static int BinarySearchWeakReferences<T>(List<WeakReference> weakReferences, T value, IComparer<T> comparer, out T foundValue)
		public static int BinarySearchWeakReferences<T>(List<WeakReference> weakReferences, T value, IComparer<T> comparer, bool allowDuplicates, out T foundValue)
		{
			int start = 0;
			int end = weakReferences.Count - 1;

			while (start <= end)
			{
				int mid = start + ((end - start) / 2);

				T testValue = (T)Utilities.GetWeakReferenceTarget(weakReferences[mid]);

				if (testValue == null)
				{
					weakReferences.RemoveAt(mid);
					end--;
					continue;
				}

				int compareResult = comparer.Compare(testValue, value);

				if (compareResult == 0)
				{
					foundValue = testValue;

					// MD 2/29/12 - 12.1 - Table Support
					if (allowDuplicates)
						return ~mid;

					return mid;
				}

				if (compareResult < 0)
					start = mid + 1;
				else
					end = mid - 1;
			}

			foundValue = default(T);
			return ~start;
		} 

		#endregion // BinarySearchWeakReferences

        #region ComputeMD4Hash

        public static Guid ComputeMD4Hash( byte[] data )
		{
			// Pad the data to align it by 512 bits
			data = Utilities.AppendDataForMD4( data );

			// Initialize the 4 registers with the appropriate start values
			uint A = 0x67452301;
			uint B = 0xEFCDAB89;
			uint C = 0x98BADCFE;
			uint D = 0x10325476;

			// Data will be encoded in blocks of 16 32-bit values at a time
			uint[] X = new uint[ 16 ];

			for ( int i = 0; i < data.Length; i += 64 )
			{
				// Cache the next 16 32-bit values from the data
				for ( int j = 0; j < 16; j++ )
					X[ j ] = BitConverter.ToUInt32( data, i + ( j * 4 ) );

				// Save the original values of the four registers
				uint oldA = A;
				uint oldB = B;
				uint oldC = C;
				uint oldD = D;

				// Perform the round 1 operations
				Utilities.MD4Round1Operation( ref A, B, C, D, X, 0, 3 );
				Utilities.MD4Round1Operation( ref D, A, B, C, X, 1, 7 );
				Utilities.MD4Round1Operation( ref C, D, A, B, X, 2, 11 );
				Utilities.MD4Round1Operation( ref B, C, D, A, X, 3, 19 );
				Utilities.MD4Round1Operation( ref A, B, C, D, X, 4, 3 );
				Utilities.MD4Round1Operation( ref D, A, B, C, X, 5, 7 );
				Utilities.MD4Round1Operation( ref C, D, A, B, X, 6, 11 );
				Utilities.MD4Round1Operation( ref B, C, D, A, X, 7, 19 );
				Utilities.MD4Round1Operation( ref A, B, C, D, X, 8, 3 );
				Utilities.MD4Round1Operation( ref D, A, B, C, X, 9, 7 );
				Utilities.MD4Round1Operation( ref C, D, A, B, X, 10, 11 );
				Utilities.MD4Round1Operation( ref B, C, D, A, X, 11, 19 );
				Utilities.MD4Round1Operation( ref A, B, C, D, X, 12, 3 );
				Utilities.MD4Round1Operation( ref D, A, B, C, X, 13, 7 );
				Utilities.MD4Round1Operation( ref C, D, A, B, X, 14, 11 );
				Utilities.MD4Round1Operation( ref B, C, D, A, X, 15, 19 );

				// Perform the round 2 operations
				Utilities.MD4Round2Operation( ref A, B, C, D, X, 0, 3 );
				Utilities.MD4Round2Operation( ref D, A, B, C, X, 4, 5 );
				Utilities.MD4Round2Operation( ref C, D, A, B, X, 8, 9 );
				Utilities.MD4Round2Operation( ref B, C, D, A, X, 12, 13 );
				Utilities.MD4Round2Operation( ref A, B, C, D, X, 1, 3 );
				Utilities.MD4Round2Operation( ref D, A, B, C, X, 5, 5 );
				Utilities.MD4Round2Operation( ref C, D, A, B, X, 9, 9 );
				Utilities.MD4Round2Operation( ref B, C, D, A, X, 13, 13 );
				Utilities.MD4Round2Operation( ref A, B, C, D, X, 2, 3 );
				Utilities.MD4Round2Operation( ref D, A, B, C, X, 6, 5 );
				Utilities.MD4Round2Operation( ref C, D, A, B, X, 10, 9 );
				Utilities.MD4Round2Operation( ref B, C, D, A, X, 14, 13 );
				Utilities.MD4Round2Operation( ref A, B, C, D, X, 3, 3 );
				Utilities.MD4Round2Operation( ref D, A, B, C, X, 7, 5 );
				Utilities.MD4Round2Operation( ref C, D, A, B, X, 11, 9 );
				Utilities.MD4Round2Operation( ref B, C, D, A, X, 15, 13 );

				// Perform the round 3 operations
				Utilities.MD4Round3Operation( ref A, B, C, D, X, 0, 3 );
				Utilities.MD4Round3Operation( ref D, A, B, C, X, 8, 9 );
				Utilities.MD4Round3Operation( ref C, D, A, B, X, 4, 11 );
				Utilities.MD4Round3Operation( ref B, C, D, A, X, 12, 15 );
				Utilities.MD4Round3Operation( ref A, B, C, D, X, 2, 3 );
				Utilities.MD4Round3Operation( ref D, A, B, C, X, 10, 9 );
				Utilities.MD4Round3Operation( ref C, D, A, B, X, 6, 11 );
				Utilities.MD4Round3Operation( ref B, C, D, A, X, 14, 15 );
				Utilities.MD4Round3Operation( ref A, B, C, D, X, 1, 3 );
				Utilities.MD4Round3Operation( ref D, A, B, C, X, 9, 9 );
				Utilities.MD4Round3Operation( ref C, D, A, B, X, 5, 11 );
				Utilities.MD4Round3Operation( ref B, C, D, A, X, 13, 15 );
				Utilities.MD4Round3Operation( ref A, B, C, D, X, 3, 3 );
				Utilities.MD4Round3Operation( ref D, A, B, C, X, 11, 9 );
				Utilities.MD4Round3Operation( ref C, D, A, B, X, 7, 11 );
				Utilities.MD4Round3Operation( ref B, C, D, A, X, 15, 15 );

				// Add the original value of the registers in this round to the new value of the registers
				A += oldA;
				B += oldB;
				C += oldC;
				D += oldD;
			}

			// The result will eveually be a GUID, so create a 16 byte array to hold the values
			byte[] result = new byte[ 16 ];

			// Put the data from the registers into the result data
			Buffer.BlockCopy( BitConverter.GetBytes( A ), 0, result, 0, 4 );
			Buffer.BlockCopy( BitConverter.GetBytes( B ), 0, result, 4, 4 );
			Buffer.BlockCopy( BitConverter.GetBytes( C ), 0, result, 8, 4 );
			Buffer.BlockCopy( BitConverter.GetBytes( D ), 0, result, 12, 4 );

			return new Guid( result );
		}

		private static byte[] AppendDataForMD4( byte[] data )
		{
			// Get the number of bits in the data to be MD4 encoded
			ulong dataLength = (ulong)data.Length * 8;

			// Determine the remained when the number of bits is devided by 512
			int modulo = (int)( dataLength % 512 );

			// The total number of bits in the encoded data must be 448 more than a multiple of 512 (congruent to 448, modulo 512).
			// Then a 64-bit length of is appended which indicates the number of bits in the actual data. Bits are always padded on 
			// the data, even if it is already congruent to 448, modulo 512 (in this case, exactly 512 bits are padded).
			int bitsToAppend = modulo < 448
				? bitsToAppend = 448 - modulo
				: bitsToAppend = 960 - modulo;

			// We should be padding an integral number of bytes, so make sure the bits to append is alligned by 8 bits
			Debug.Assert( bitsToAppend % 8 == 0 );

			// Determine the number of bytes to append from the number of bits (we should always be appending bytes)
			int bytesToAppend = bitsToAppend / 8;
			Debug.Assert( bytesToAppend > 0 );

			// Create a block of data to hold the data, padding, and 64-bit data length.
			byte[] appendedData = new byte[ data.Length + bytesToAppend + 8 ];
			Buffer.BlockCopy( data, 0, appendedData, 0, data.Length );

			// All padding bits are 0 (they would have been initialized this way anyway when the byte array was created), 
			// except the first bit, which should be 1 (0x80 is 1000000 in binary)
			appendedData[ data.Length ] = 0x80;

			// Append the 64-bit length to the end of the data
			Buffer.BlockCopy( BitConverter.GetBytes( dataLength ), 0, appendedData, data.Length + bytesToAppend, 8 );

			return appendedData;
		}

		private static void MD4Round1Operation( ref uint A, uint B, uint C, uint D, uint[] X, int i, int s )
		{
			A = Utilities.RotateBitsLeft( ( A + ( ( B & C ) | ( ~B & D ) ) + X[ i ] ), s );
		}

		private static void MD4Round2Operation( ref uint A, uint B, uint C, uint D, uint[] X, int i, int s )
		{
			A = Utilities.RotateBitsLeft( ( A + ( ( B & C ) | ( B & D ) | ( C & D ) ) + X[ i ] + 0x5A827999 ), s );
		}

		private static void MD4Round3Operation( ref uint A, uint B, uint C, uint D, uint[] X, int i, int s )
		{
			A = Utilities.RotateBitsLeft( ( A + ( B ^ C ^ D ) + X[ i ] + 0x6ED9EBA1 ), s );
		}

		#endregion ComputeMD4Hash

        // MBS 8/4/08 - Excel 2007 Format
        #region ConvertHLSToRGB

		// MD 5/7/12 - TFS106831
		// Rewrote this based on feedback from Microsoft.
		#region Old Code

		//// MD 1/26/12 - 12.1 - Cell Format Updates
		//// http://www.easyrgb.com/index.php?X=MATH&H=19#text19
		//#region Old Code

		////public static Color ConvertHLSToRGB(int h, int l, int s)
		////{
		////    int r, g, b;
		////    double magic1, magic2;

		////    if (s == 0)
		////    {
		////        r = g = b = (l * RGBMAX) / HLSMAX;

		////        // MD 10/26/10 - TFS56976
		////        // This seems to be an incorrect assumption. If the saturation is 0, just ignore the hue.
		////        //Debug.Assert(h == -1, "Expected hue to be undefined when saturation is 0");
		////    }
		////    else
		////    {
		////        if (l <= (HLSMAX / 2))
		////            magic2 = (l * (HLSMAX + s) + (HLSMAX / 2)) / HLSMAX;
		////        else
		////            magic2 = l + s - ((l * s) + (HLSMAX / 2)) / (double)HLSMAX;

		////        magic1 = 2 * l - magic2;

		////        r = (int)Math.Min(255, Math.Round((HueToRGB(magic1, magic2, h + (HLSMAX / 3)) * RGBMAX + (HLSMAX / 2)) / HLSMAX));
		////        g = (int)Math.Min(255, Math.Round((HueToRGB(magic1, magic2, h) * RGBMAX + (HLSMAX / 2)) / HLSMAX));
		////        b = (int)Math.Min(255, Math.Round((HueToRGB(magic1, magic2, h - (HLSMAX / 3)) * RGBMAX + (HLSMAX / 2)) / HLSMAX));
		////    }

		////    return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
		////}

		//#endregion // Old Code
		//public static Color ConvertHLSToRGB(double H, double L, double S)
		//{
		//    int r, g, b;

		//    if (S == 0)
		//    {
		//        r = g = b = (int)MathUtilities.MidpointRoundingAwayFromZero(L * RGBMAX);
		//    }
		//    else
		//    {
		//        double temp2;
		//        if (L <= 0.5)
		//            temp2 = L * (1 + S);
		//        else
		//            temp2 = (L + S) - (S * L);

		//        double temp1 = 2 * L - temp2;

		//        r = (int)MathUtilities.MidpointRoundingAwayFromZero(255 * Utilities.HueToRGB(temp1, temp2, H + (1 / 3d)));
		//        g = (int)MathUtilities.MidpointRoundingAwayFromZero(255 * Utilities.HueToRGB(temp1, temp2, H));
		//        b = (int)MathUtilities.MidpointRoundingAwayFromZero(255 * Utilities.HueToRGB(temp1, temp2, H - (1 / 3d)));
		//    }

		//    return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
		//}

		#endregion // Old Code
		public static Color ConvertHLSToRGB(int h, int l, int s)
		{
			int r, g, b;
			int magic1, magic2;

			if (s == 0)
			{
				r = g = b = (l * RGBMAX) / HLSMAX;
			}
			else
			{
				if (l <= (HLSMAX / 2))
					magic2 = (l * (HLSMAX + s) + (HLSMAX / 2)) / HLSMAX;
				else
					magic2 = l + s - ((l * s) + (HLSMAX / 2)) / HLSMAX;

				magic1 = 2 * l - magic2;

				r = (HueToRGB(magic1, magic2, h + (HLSMAX / 3)) * RGBMAX + (HLSMAX / 2)) / HLSMAX;
				g = (HueToRGB(magic1, magic2, h) * RGBMAX + (HLSMAX / 2)) / HLSMAX;
				b = (HueToRGB(magic1, magic2, h - (HLSMAX / 3)) * RGBMAX + (HLSMAX / 2)) / HLSMAX;
			}

			return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
		}

        #endregion //ConvertHLSToRGB
        //
        #region ConvertRGBToHLS

		// MD 5/7/12 - TFS106831
		// Rewrote this based on feedback from Microsoft.
		#region Old Code

		//// MD 1/26/12 - 12.1 - Cell Format Updates
		//// http://www.easyrgb.com/index.php?X=MATH&H=18#text18
		//#region Old Code

		////public static void ConvertRGBToHLS(Color rgb, out int h, out int l, out int s)
		////{
		////    int max = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
		////    int min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);

		////    l = (((max + min) * HLSMAX) + RGBMAX) / (2 * RGBMAX);
		////    if (max == min)
		////    {
		////        s = 0;
		////        h = UNDEFINED;
		////    }
		////    else
		////    {
		////        if (l <= (HLSMAX / 2))
		////            s = (int)Math.Round(((((max - min) * HLSMAX) + ((max + min) / 2.0))) / (max + min));
		////        else
		////            s = (int)Math.Round((((max - min) * HLSMAX) + ((2 * RGBMAX - max - min) / 2.0)) / (2 * RGBMAX - max - min));

		////        double rDelta = (((max - rgb.R) * (HLSMAX / 6.0)) + ((max - min) / 2.0)) / (double)(max - min);
		////        double gDelta = (((max - rgb.G) * (HLSMAX / 6.0)) + ((max - min) / 2.0)) / (double)(max - min);
		////        double bDelta = (((max - rgb.B) * (HLSMAX / 6.0)) + ((max - min) / 2.0)) / (double)(max - min);

		////        if (rgb.R == max)
		////            h = (int)Math.Round(bDelta - gDelta);
		////        else if (rgb.G == max)
		////            h = (int)Math.Round((HLSMAX / 3.0) + rDelta - bDelta);
		////        else
		////            h = (int)Math.Round(((2 * HLSMAX) / 3.0) + gDelta - rDelta);

		////        if (h < 0)
		////            h += HLSMAX;
		////        if (h > HLSMAX)
		////            h -= HLSMAX;
		////    }
		////    h = Math.Min(HLSMAX, h);
		////    s = Math.Min(HLSMAX, s);
		////    l = Math.Min(HLSMAX, l);
		////} 

		//#endregion // Old Code
		//public static void ConvertRGBToHLS(Color rgb, out double h, out double l, out double s)
		//{
		//    double rNorm = rgb.R / 255d;
		//    double gNorm = rgb.G / 255d;
		//    double bNorm = rgb.B / 255d;

		//    double max = Math.Max(Math.Max(rNorm, gNorm), bNorm);
		//    double min = Math.Min(Math.Min(rNorm, gNorm), bNorm);
		//    double delta = max - min;

		//    l = (max + min) / 2;
		//    if (delta == 0)
		//    {
		//        h = 0;
		//        s = 0;
		//    }
		//    else
		//    {
		//        if (l < 0.5)
		//            s = delta / (max + min);
		//        else
		//            s = delta / (2 - max - min);


		//        double rDelta = (((max - rNorm) / 6) + (delta / 2)) / delta;
		//        double gDelta = (((max - gNorm) / 6) + (delta / 2)) / delta;
		//        double bDelta = (((max - bNorm) / 6) + (delta / 2)) / delta;

		//        h = 0;
		//        if (rNorm == max)
		//            h = bDelta - gDelta;
		//        else if (gNorm == max)
		//            h = (1 / 3d) + rDelta - bDelta;
		//        else if (bNorm == max)
		//            h = (2 / 3d) + gDelta - rDelta;
		//        else
		//            Utilities.DebugFail("This shouldn't happen.");

		//        if (h < 0)
		//            h += 1;
		//        else if (h > 1)
		//            h -= 1;
		//    }
		//}

		#endregion // Old Code
		public static void ConvertRGBToHLS(Color rgb, out int h, out int l, out int s)
		{
			int max = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
			int min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);

			l = (((max + min) * HLSMAX) + RGBMAX) / (2 * RGBMAX);
			if (max == min)
			{
				s = 0;
				h = UNDEFINED;
			}
			else
			{
				if (l <= (HLSMAX / 2))
					s = ((((max - min) * HLSMAX) + ((max + min) / 2))) / (max + min);
				else
					s = (((max - min) * HLSMAX) + ((2 * RGBMAX - max - min) / 2)) / (2 * RGBMAX - max - min);

				int rDelta = (((max - rgb.R) * (HLSMAX / 6)) + ((max - min) / 2)) / (max - min);
				int gDelta = (((max - rgb.G) * (HLSMAX / 6)) + ((max - min) / 2)) / (max - min);
				int bDelta = (((max - rgb.B) * (HLSMAX / 6)) + ((max - min) / 2)) / (max - min);

				if (rgb.R == max)
					h = bDelta - gDelta;
				else if (rgb.G == max)
					h = (HLSMAX / 3) + rDelta - bDelta;
				else
					h = ((2 * HLSMAX) / 3) + gDelta - rDelta;

				if (h < 0)
					h += HLSMAX;
				if (h > HLSMAX)
					h -= HLSMAX;
			}
		}

        #endregion //ConvertRGBToHLS

		// MD 5/12/10 - TFS26732
		#region CopyCellFormatValue

		public static void CopyCellFormatValue(WorksheetCellFormatProxy sourceCellFormat, WorksheetCellFormatProxy targetCellFormat, CellFormatValue value)
		{
			// MD 10/21/10 - TFS34398
			// Call off to the new overload.
			Utilities.CopyCellFormatValue(sourceCellFormat, targetCellFormat, value, true, CellFormatValueChangedOptions.DefaultBehavior);
		}

		// MD 10/21/10 - TFS34398
		// Added a new overload that takes a boolean indicating wether to call the BeforeSet and AfterSet methods on the proxy when setting the value.
		public static void CopyCellFormatValue(WorksheetCellFormatProxy sourceCellFormat, WorksheetCellFormatProxy targetCellFormat, CellFormatValue value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 10/21/10 - TFS34398
			// Pass along the new callBeforeAndAfterSet parameter.
			//targetCellFormat.SetValue(value, sourceCellFormat.GetValue(value));
			targetCellFormat.SetValue(value, sourceCellFormat.GetValue(value), callBeforeAndAfterSet, options);
		}

		// MD 4/18/11 - TFS62026
		// Added an overload to copy directly to a WorksheetCellFormatData
		public static void CopyCellFormatValue(WorksheetCellFormatProxy sourceCellFormat, WorksheetCellFormatData targetCellFormat, CellFormatValue value)
		{
			targetCellFormat.SetValue(value, sourceCellFormat.GetValue(value));
		}

		// MD 2/18/12 - 12.1 - Table Support
		public static void CopyCellFormatValue(WorksheetCellFormatData sourceCellFormat, WorksheetCellFormatData targetCellFormat, CellFormatValue value)
		{
			targetCellFormat.SetValue(value, sourceCellFormat.GetValue(value));
		}

		// MD 2/29/12 - 12.1 - Table Support
		public static void CopyCellFormatValue(WorksheetCellFormatData sourceCellFormat, WorksheetCellFormatProxy targetCellFormat, CellFormatValue value)
		{
			targetCellFormat.SetValue(value, sourceCellFormat.GetValue(value));
		}

		#endregion // CopyCellFormatValue

		// MD 3/10/12 - 12.1 - Table Support
		#region CopyCellFormatValues

		public static void CopyCellFormatValues(WorksheetCellFormatProxy sourceFormat, WorksheetCellFormatProxy targetFormat, IList<CellFormatValue> values)
		{
			for (int i = 0; i < values.Count; i++)
				Utilities.CopyCellFormatValue(sourceFormat, targetFormat, values[i]);
		}

		#endregion // CopyCellFormatValues

        #region CreateReferenceString

		public static string CreateReferenceString(string workbookFilePath, string worksheetName)
		{
			return Utilities.CreateReferenceString(workbookFilePath, worksheetName, null);
		}

		public static string CreateReferenceString(string workbookFilePath, string firstWorksheetName, string lastWorksheetName)
		{
			return Utilities.CreateReferenceString(workbookFilePath, false, firstWorksheetName, lastWorksheetName);
		}

		public static string CreateReferenceString(string workbookFilePath, bool isWorkbookFilePathIndexed, string firstWorksheetName, string lastWorksheetName)
		{
			// MD 10/9/07 - BR27172
			// The add-in function pseudo-workbook shouldn't have a reference string
			if (workbookFilePath == AddInFunctionsWorkbookReference.AddInFunctionsWorkbookName)
				return string.Empty;

			bool worksheetReferenceNeedsQuotes = false;
			string worksheetReference = null;
			if (firstWorksheetName != null)
			{
				if (lastWorksheetName == null)
				{
					worksheetReference = firstWorksheetName;
					worksheetReferenceNeedsQuotes = FormulaParser.ShouldWorksheetNameBeQuoted(firstWorksheetName);
				}
				else
				{
					worksheetReference = string.Format("{0}:{1}", firstWorksheetName, lastWorksheetName);
					worksheetReferenceNeedsQuotes =
						FormulaParser.ShouldWorksheetNameBeQuoted(firstWorksheetName) ||
						FormulaParser.ShouldWorksheetNameBeQuoted(lastWorksheetName);
				}
			}

			// If the workbook file name is invalid, it is just worksheet reference
			if (String.IsNullOrEmpty(workbookFilePath))
			{
				if (firstWorksheetName == null)
					return string.Empty;

				if (worksheetReferenceNeedsQuotes)
					return string.Format("'{0}'!", worksheetReference.Replace("'", "''"));

				return string.Format("{0}!", worksheetReference);
			}

			// If the worksheet name is invalid or the same as the workbook file path, it is considered 
			// an external workbook reference
			if (String.IsNullOrEmpty(firstWorksheetName) || workbookFilePath == firstWorksheetName)
			{
				Debug.Assert(lastWorksheetName == null, "We should not have a last worksheet if this is a workbook reference.");

				if (FormulaParser.ShouldWorksheetNameBeQuoted(workbookFilePath))
					return string.Format("'{0}'!", workbookFilePath.Replace("'", "''"));

				return string.Format("{0}!", workbookFilePath);
			}

			// Otherwise, it is an external worksheet reference.
			int lastDirectorySeparator = workbookFilePath.LastIndexOfAny(new char[] { 
				Path.DirectorySeparatorChar, 
				Path.AltDirectorySeparatorChar });

			string fullReference;
			bool shouldBeQuoted = worksheetReferenceNeedsQuotes;
			if (isWorkbookFilePathIndexed)
			{
				fullReference = string.Format("{0}{1}", workbookFilePath, worksheetReference);
			}
			else
			{
				string directoryPath = workbookFilePath.Substring(0, lastDirectorySeparator + 1);
				string fileName = workbookFilePath.Substring(lastDirectorySeparator + 1);
				fullReference = string.Format("{0}[{1}]{2}", directoryPath, fileName, worksheetReference);

				if (String.IsNullOrEmpty(directoryPath) == false || FormulaParser.ShouldWorksheetNameBeQuoted(fileName))
					shouldBeQuoted = true;
			}

			if (shouldBeQuoted)
				return string.Format("'{0}'!", fullReference.Replace("'", "''"));

			return string.Format("{0}!", fullReference);
		}

		#endregion CreateReferenceString

		// 8/8/08 - Excel formula solving
		// This has been moved to ExcelCalcValue
		#region Moved

		//        // MD 10/24/07 - BR27751
		//        #region DateTimeToExcelDate
		//
		//#if DEBUG
		//        /// <summary>
		//        /// Converts a DateTime to Excel's numerical representation of a date.
		//        /// </summary>
		//        /// <param name="dateValue">The DateTime to convert.</param>
		//#endif
		//        public static double DateTimeToExcelDate( DateTime dateValue )
		//        {
		//            // We don't want to do the leap year correction for the min date
		//            if ( dateValue.Ticks == 0 )
		//                return 0;
		//
		//            double result = dateValue.ToOADate();
		//
		//            // MRS 12/12/06 - BR18467
		//            // Excel incorrectly assumes that the year 1900 is a leap year
		//            // http://support.microsoft.com/kb/214326/en-us
		//            if ( dateValue < new DateTime( 1900, 3, 1 ) )
		//                result -= 1;
		//
		//            return result;
		//        }
		//
		//        #endregion DateTimeToExcelDate 

		#endregion Moved

		#region DecodeURL

		public static string DecodeURL( string currentPath, string encodedPath )
		{
			if ( encodedPath.Trim().Length == 0 )
				return currentPath;

			if ( encodedPath[ 0 ] != (char)1 )
				return encodedPath;

			StringBuilder finalPath = new StringBuilder();

			if ( currentPath != null )
			{
				finalPath.Append( Path.GetDirectoryName( currentPath ) );
				finalPath.Append( Path.DirectorySeparatorChar );
			}

			for ( int i = 1; i < encodedPath.Length; i++ )
			{
				switch ( (int)encodedPath[ i ] )
				{
					case 1: // An MS-DOS drive letter will follow, or "@" and the server name of a UNC path
						{
							finalPath.Length = 0;

							char root = encodedPath[ ++i ];

							if ( root == '@' )
							{
								finalPath.Append( Path.DirectorySeparatorChar );
								finalPath.Append( Path.DirectorySeparatorChar );
							}
							else
							{
								finalPath.Append( root );
								finalPath.Append( Path.VolumeSeparatorChar );
								finalPath.Append( Path.DirectorySeparatorChar );
							}

							break;
						}

					case 2: // Start path name on same drive as own document
						finalPath.Length = 0;
						finalPath.Append( Path.GetPathRoot( currentPath ) );
						break;

					case 3: // End of subdirectory name
						finalPath.Append( Path.DirectorySeparatorChar );
						break;

					case 4: // Start path name in parent directory of own document (may occur repeatedly)
						string originalPath = finalPath.ToString();

						finalPath.Length = 0;
						finalPath.Append( Path.GetDirectoryName( originalPath ) );
						finalPath.Append( Path.DirectorySeparatorChar );
						break;

					case 5: // Unencoded URL. Followed by the length of the URL (1 byte), and the URL itself.
						{
                            byte[] charBytes = Workbook.InvariantCompressedTextEncoding.GetBytes(encodedPath[++i].ToString());
							Debug.Assert( charBytes.Length == 1 );

							finalPath.Length = 0;
							finalPath.Append( encodedPath.Substring( ++i, charBytes[ 0 ] ) );
							i += finalPath.Length;

							Debug.Assert( i == encodedPath.Length );
							break;
						}

					case 6: // Start path name in installation directory of Excel
						Utilities.DebugFail( "Don't know how to get the start path name in installation directory of Excel." );
						break;

					case 7: // Macro template directory in installation directory of Excel
						Utilities.DebugFail( "Don't know how to get the macro template directory of Excel." );
						break;

					default:
						finalPath.Append( encodedPath[ i ] );
						break;
				}
			}

			return finalPath.ToString();
		}

		#endregion DecodeURL

		#region DecodeRKValue

		public static double DecodeRKValue( uint rkValue )
		{
			bool valueMultipliedBy100 = ( rkValue & RKValueIsMultipliedBy100 ) == RKValueIsMultipliedBy100;
			bool signedInt =			( rkValue & RKValueIsSignedInt ) == RKValueIsSignedInt;

			uint encodedValue = rkValue & 0xFFFFFFFC;

			if ( signedInt )
			{
				double decodedValue = (int)encodedValue >> 2;

				if ( valueMultipliedBy100 )
					decodedValue /= 100;

				return decodedValue;
			}
			else
			{
				byte[] bytes = new byte[ 8 ];
				byte[] valueBytes = BitConverter.GetBytes( encodedValue );
				valueBytes.CopyTo( bytes, 4 );

				double decodedValue = BitConverter.ToDouble( bytes, 0 );

				if ( valueMultipliedBy100 )
					decodedValue /= 100;

				return (double)decodedValue;
			}
		}

		#endregion DecodeRKValue

		#region EMUToTwips

		public static int EMUToTwips( int emuValue )
		{
			return ( emuValue * Utilities.TwipsPerPoint ) / Utilities.EMUsPerPoint;
		}

		#endregion EMUToTwips

		#region EncodeURL

		public static string EncodeURL( string path )
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( (char)1 );

			if ( Path.IsPathRooted( path ) )
			{
				string root = Path.GetPathRoot( path );

				sb.Append( (char)1 );

				if ( root.StartsWith( String.Concat( Path.DirectorySeparatorChar, Path.DirectorySeparatorChar ) ) )
				{
					int lastSeparator = root.LastIndexOf( Path.DirectorySeparatorChar );

					if ( lastSeparator < 0 )
						lastSeparator = root.LastIndexOf( Path.AltDirectorySeparatorChar );

					if ( lastSeparator < 0 )
						lastSeparator = root.Length;

					sb.Append( "@" + root.Substring( 2, lastSeparator - 2 ) );
					sb.Append( (char)3 );
				}
				else
				{
					sb.Append( root.Substring( 0, 1 ) );
				}

				List<string> directoryNames = new List<string>();

				while ( true )
				{
					string directoryName = Path.GetFileName( path );

					if ( directoryName == null || directoryName.Length == 0 )
						break;

					directoryNames.Insert( 0, directoryName );

					path = Path.GetDirectoryName( path );
				}

				for ( int i = 0; i < directoryNames.Count; i++ )
				{
					string directoryName = directoryNames[ i ];

					sb.Append( directoryName );

					if ( i < directoryNames.Count - 1 )
						sb.Append( (char)3 );
				}

				return sb.ToString();
			}
			else if ( Uri.IsWellFormedUriString( path, UriKind.Absolute ) )
			{
				sb.Append( (char)5 );
                sb.Append(EncodingDefault.GetString(new byte[] { (byte)path.Length }));
				sb.Append( path );

				return sb.ToString();
			}
			else
			{
				return path;
			}
		} 

		#endregion EncodeURL

		// MD 10/27/10 - TFS56976
		// We don't need this anymore. Also, I don't think it was correct. It looks like it was just checking for the existence of a 3D reference with a workbook name, 
		// but regular cells can have 3D references to external workbooks as well.
		//// MBS 9/11/08 - Excel 2007
		//#region ExternalFormulaRegex
		//
		//private static Regex ExternalFormulaRegex
		//{
		//    get
		//    {
		//        if(externalFormulaRegex == null)
		//            externalFormulaRegex = new Regex(@"'*\[(?<Index>\d+)\](?<Sheet>[\w\s]*)'*!(?<Name>[$\w:]+)", RegexOptionsCompiled);
		//
		//        return externalFormulaRegex;
		//    }
		//}
		//#endregion //ExternalFormulaRegex

		// MD 5/2/11 - TFS74130
		#region GetAssociatedBorderValue






		public static CellFormatValue GetAssociatedBorderValue(CellFormatValue value)
		{
			switch (value)
			{
				case CellFormatValue.BottomBorderColorInfo:
					return CellFormatValue.BottomBorderStyle;

				case CellFormatValue.BottomBorderStyle:
					return CellFormatValue.BottomBorderColorInfo;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorderColorInfo:
					return CellFormatValue.DiagonalBorderStyle;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorderStyle:
					return CellFormatValue.DiagonalBorderColorInfo;

				case CellFormatValue.LeftBorderColorInfo:
					return CellFormatValue.LeftBorderStyle;

				case CellFormatValue.LeftBorderStyle:
					return CellFormatValue.LeftBorderColorInfo;

				case CellFormatValue.RightBorderColorInfo:
					return CellFormatValue.RightBorderStyle;

				case CellFormatValue.RightBorderStyle:
					return CellFormatValue.RightBorderColorInfo;

				case CellFormatValue.TopBorderColorInfo:
					return CellFormatValue.TopBorderStyle;

				case CellFormatValue.TopBorderStyle:
					return CellFormatValue.TopBorderColorInfo;


				default:
					Utilities.DebugFail("A non-border property was specified: " + value);
					return value;
			}
		}

		#endregion // GetAssociatedBorderValue

		// MD 5/13/11 - Data Validations / Page Breaks
		#region GetCellAddressRange

		public static CellAddressRange GetCellAddressRange(ReferenceToken referenceToken)
		{
			AreaToken areaToken = referenceToken as AreaToken;
			if (areaToken != null)
				return areaToken.CellAddressRange;

			AreaNToken areaNToken = referenceToken as AreaNToken;
			if (areaNToken != null)
				return areaNToken.CellAddressRange;

			Area3DNToken area3DNToken = referenceToken as Area3DNToken;
			if (area3DNToken != null)
				return area3DNToken.CellAddressRange;

			Area3DToken area3DToken = referenceToken as Area3DToken;
			if (area3DToken != null)
				return area3DToken.CellAddressRange;

			return null;
		} 

		#endregion  // GetCellAddressRange

        // MD 6/30/08 - Excel 2007 Format
		#region GetFileName






		public static string GetFileName( Stream stream )
		{



			FileStream fileStream = stream as FileStream;

			if ( fileStream == null )
				return null;

			return fileStream.Name;

		}

		#endregion GetFileName

		// MD 4/12/11 - TFS67084
		// Removed the FormattedStringProxy type. Elements can be held directly.
		#region Old Code

		// MD 11/3/10 - TFS49093
		//#region GetFormattedStringProxy
		//
		//public static FormattedStringProxy GetFormattedStringProxy(object value)
		//{
		//    FormattedStringProxy proxy = value as FormattedStringProxy;
		//
		//    if (proxy != null)
		//        return proxy;
		//
		//    FormattedString formattedString = value as FormattedString;
		//    if (formattedString != null)
		//        return formattedString.Proxy;
		//
		//    FormattedStringValueReference valueReference = value as FormattedStringValueReference;
		//    if (valueReference != null)
		//        return valueReference.Proxy;
		//
		//    return null;
		//}
		//
		//#endregion // GetFormattedStringProxy 

		#endregion // Old Code
		#region GetFormattedStringElement

		public static StringElement GetFormattedStringElement(object value)
		{
			StringElement element = value as StringElement;
			if (element != null)
				return element;

			FormattedString formattedString = value as FormattedString;
			if (formattedString != null)
				return formattedString.Element;

			FormattedStringValueReference valueReference = value as FormattedStringValueReference;
			if (valueReference != null)
				return valueReference.Element;

			return null;
		}

		#endregion // GetFormattedStringProxy

		// MD 5/12/10 - TFS26732
		#region GetOppositeBorderValue







		public static CellFormatValue GetOppositeBorderValue(CellFormatValue value)
		{
			switch (value)
			{
				case CellFormatValue.BottomBorderColorInfo:
					return CellFormatValue.TopBorderColorInfo;

				case CellFormatValue.BottomBorderStyle:
					return CellFormatValue.TopBorderStyle;

				case CellFormatValue.LeftBorderColorInfo:
					return CellFormatValue.RightBorderColorInfo;

				case CellFormatValue.LeftBorderStyle:
					return CellFormatValue.RightBorderStyle;

				case CellFormatValue.RightBorderColorInfo:
					return CellFormatValue.LeftBorderColorInfo;

				case CellFormatValue.RightBorderStyle:
					return CellFormatValue.LeftBorderStyle;

				case CellFormatValue.TopBorderColorInfo:
					return CellFormatValue.BottomBorderColorInfo;

				case CellFormatValue.TopBorderStyle:
					return CellFormatValue.BottomBorderStyle;

				default:
					Utilities.DebugFail("A non-border property was specified: " + value);
					return value;
			}
		} 

		#endregion // GetOppositeBorderValue

		// MD 5/25/11 - Data Validations / Page Breaks
		#region GetRegionsFromFormula

		public static List<CellAddressRange> GetRangesFromFormula(Formula formula)
		{
			List<CellAddressRange> ranges = new List<CellAddressRange>();

			foreach (FormulaToken token in formula.PostfixTokenList)
			{
				if (token is MemOperatorBase)
					continue;

				if (token is UnionOperator)
					continue;

				Ref3DToken cell = token as Ref3DToken;
				if (cell != null)
				{
					ranges.Add(new CellAddressRange(cell.CellAddress, cell.CellAddress));
					continue;
				}

				Area3DToken area = token as Area3DToken;

				if (area == null)
				{
					Utilities.DebugFail("Invalid formula token.");
					continue;
				}

				ranges.Add(area.CellAddressRange);
			}

			return ranges;
		}

		#endregion  // GetRegionsFromFormula

		// MD 8/21/08 - Excel formula solving
		#region GetWeakReferenceTarget

		internal static object GetWeakReferenceTarget( WeakReference weakRef )
		{
			try
			{
				// AS 10/25/06
				//if ( weakRef.IsAlive == false )
				if ( weakRef == null || weakRef.IsAlive == false )
					return null;

				return weakRef.Target;
			}
			catch ( InvalidOperationException )
			{
				return null;
			}
		}

		#endregion GetWeakReferenceTarget

		// MD 6/30/08 - Excel 2007 Format
		#region GetWorkbookFormat( Stream, string )



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        public static WorkbookFormat GetWorkbookFormat(Stream stream, string parameterName, IPackageFactory packageFactory)
		{
			return Utilities.GetWorkbookFormat( stream, parameterName, true, packageFactory );
		}

		#endregion GetWorkbookFormat( Stream, string )

		// MD 6/30/08 - Excel 2007 Format
		#region GetWorkbookFormat( Stream, string, bool )

		private static WorkbookFormat GetWorkbookFormat( Stream stream, string parameterName, bool throwExcepionOnError, IPackageFactory packageFactory )
		{
            // Bail out here if we don't have a valid stream, since we'll get
            // an exception if we try to open a stream without any data
            if (stream == null || stream.Length == 0)
                return (WorkbookFormat)(-1);

			if ( StructuredStorageManager.IsStructuredStorageStream( stream ) )
				return WorkbookFormat.Excel97To2003;


            if (packageFactory == null)
                packageFactory = new PackageFactory();


            if (packageFactory == null)
                return (WorkbookFormat)(-1);

			try
			{                
				using ( IPackage package = packageFactory.Open( stream, FileMode.Open ) )
				{
					foreach ( IPackageRelationship relationship in package.GetRelationships() )
					{
						//http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument
						if ( relationship.RelationshipType !=
							Serialization.Excel2007.XLSX.ContentTypes.WorkbookPart.RelationshipTypeValue )
							continue;

						Uri absolutePartPath = PackageUtilities.GetTargetPartPath( relationship );

						IPackagePart part = package.GetPart( absolutePartPath );

						switch ( part.ContentType )
						{
							case Serialization.Excel2007.XLSX.ContentTypes.WorkbookPart.ContentTypeValue:
								return WorkbookFormat.Excel2007;

							// MD 10/1/08 - TFS8471
							case Serialization.Excel2007.XLSX.ContentTypes.MacroEnabledWorkbookPart.ContentTypeValue:
								return WorkbookFormat.Excel2007MacroEnabled;

							// MD 5/7/10 - 10.2 - Excel Templates
							case Serialization.Excel2007.XLSX.ContentTypes.MacroEnabledTemplatePart.ContentTypeValue:
								return WorkbookFormat.Excel2007MacroEnabledTemplate;

							// MD 5/7/10 - 10.2 - Excel Templates
							case Serialization.Excel2007.XLSX.ContentTypes.TemplatePart.ContentTypeValue:
								return WorkbookFormat.Excel2007Template;

							//case Infragistics.Documents.Excel.Serialization.Excel2007.XLSB.ContentTypes.WorkbookPart.ContentTypeValue:
							//	return WorkbookFormat.Excel2007Binary;

							default:
								Utilities.DebugFail( "Unknown content type" );
								break;
						}
					}
				}
			}
			catch ( Exception e )
			{
				Utilities.DebugFail( "Error opening package: " + e );
			}

			if ( throwExcepionOnError )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_InvalidFileFormat" ), parameterName );

			return (WorkbookFormat)( -1 );
		} 

		#endregion GetWorkbookFormat( Stream, string, bool )

        // MBS 7/18/08 - Excel 2007 Format
        #region GetWorkbookFormat(string, Stream)

        public static WorkbookFormat GetWorkbookFormat(string fileName, Stream workbookStream, IPackageFactory packageFactory)
        {
            return Utilities.GetWorkbookFormat(fileName, workbookStream, null, false, packageFactory);
        }
        #endregion //GetWorkbookFormat(string, StreaM)

        // MD 6/30/08 - Excel 2007 Format
		#region GetWorkbookFormat( string, string )







        public static WorkbookFormat GetWorkbookFormat(string fileName, string parameterName, IPackageFactory packageFactory)
		{
			return Utilities.GetWorkbookFormat( fileName, null, parameterName, true, packageFactory );
		}

		#endregion GetWorkbookFormat( string, string )

		// MD 6/30/08 - Excel 2007 Format
		#region GetWorkbookFormat( string, string, bool )

        private static WorkbookFormat GetWorkbookFormat(string fileName, Stream workbookStream, string parameterName, bool throwExcepionOnError, IPackageFactory packageFactory)
        {
            WorkbookFormat? workbookFormat = Workbook.GetWorkbookFormat(fileName);
            if (workbookFormat != null)
                return (WorkbookFormat)workbookFormat;

            // MBS 7/17/08 
            // If we have already opened a stream, we shouldn't try to open it again       
            if (workbookStream != null)
                return Utilities.GetWorkbookFormat(workbookStream, parameterName, throwExcepionOnError, packageFactory);

            FileStream stream = null;
            try
            {
                try
                {
                    stream = new FileStream(fileName, FileMode.Open);
                }
                catch (Exception e)
                {
                    Utilities.DebugFail("Error opening file: " + e);
                    throw;
                }

                return Utilities.GetWorkbookFormat(stream, parameterName, throwExcepionOnError, packageFactory);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }    

		#endregion GetWorkbookFormat( string, string, bool )

        public static string GetXmlNodeName(XmlNode node)
        {

            return node.LocalName;



        }

        

        public static XmlAttributeCollection GetXmlNodeAttributes(XmlNode node)
        {
            return node.Attributes;





        }


        public static XmlNodeList GetXmlNodeNodes(XmlNode node)
        {
            return node.ChildNodes;





        }

        public static string GetXmlAttributeName(XmlAttribute attribute)
        {

            return attribute.LocalName;



        }

        // MBS 8/4/08 - Excel 2007 Format
        #region HueToRGB

		// MD 5/7/12 - TFS106831
		// Rewrote this based on feedback from Microsoft.
		#region Old Code

		//// MD 1/26/12 - 12.1 - Cell Format Updates
		//// http://www.easyrgb.com/index.php?X=MATH&H=19#text19
		//#region Old Code

		////private static double HueToRGB(double n1, double n2, double hue)
		////{
		////    if (hue < 0)
		////        hue += HLSMAX;

		////    if (hue > HLSMAX)
		////        hue -= HLSMAX;

		////    if (hue < (HLSMAX / 6))
		////        return n1 + (((n2 - n1) * hue + (HLSMAX / 12)) / (HLSMAX / 6));

		////    if (hue < (HLSMAX / 2))
		////        return n2;

		////    if (hue < ((HLSMAX * 2) / 3))
		////        return n1 + (((n2 - n1) * (((HLSMAX * 2) / 3) - hue) + (HLSMAX / 12)) / (HLSMAX / 6));

		////    return n1;
		////}

		//#endregion // Old Code
		//private static double HueToRGB(double v1, double v2, double vH)
		//{
		//    if (vH < 0)
		//        vH += 1;

		//    if (vH > 1)
		//        vH -= 1;

		//    if ((6 * vH) < 1)
		//        return (v1 + (v2 - v1) * 6 * vH);

		//    if ((2 * vH) < 1)
		//        return (v2);

		//    if ((3 * vH) < 2)
		//        return (v1 + (v2 - v1) * ((2 / 3d) - vH) * 6);

		//    return v1;
		//}

		#endregion // Old Code
		private static int HueToRGB(int n1, int n2, int hue)
		{
			if (hue < 0)
				hue += HLSMAX;

			if (hue > HLSMAX)
				hue -= HLSMAX;

			if (hue < (HLSMAX / 6))
				return n1 + (((n2 - n1) * hue + (HLSMAX / 12)) / (HLSMAX / 6));

			if (hue < (HLSMAX / 2))
				return n2;

			if (hue < ((HLSMAX * 2) / 3))
				return n1 + (((n2 - n1) * (((HLSMAX * 2) / 3) - hue) + (HLSMAX / 12)) / (HLSMAX / 6));

			return n1;
		}

        #endregion //HueToRGB

		// MD 11/1/10 - TFS56976
		#region IntersetLists






		internal static List<T> IntersetLists<T>(List<T> list1, List<T> list2)
		{
			List<T> regionsContainingCell = null;

			Dictionary<T, bool> list1Set = new Dictionary<T, bool>();
			for (int i = 0; i < list1.Count; i++)
				list1Set[list1[i]] = true;

			for (int i = 0; i < list2.Count; i++)
			{
				T item = list2[i];

				// MD 1/23/12
				// Found while fixing TFS99849
				// We should remove the item from the set so we don't get it twice in the intersected list if it was in list2 twice.
				// And the return value from Remove tells us if it was in the list.
				//if (list1Set.ContainsKey(item) == false)
				if (list1Set.Remove(item) == false)
					continue;

				if (regionsContainingCell == null)
					regionsContainingCell = new List<T>();

				regionsContainingCell.Add(item);
			}

			return regionsContainingCell;
		}

		#endregion // IntersetLists

		// MD 2/4/11 - TFS65015
		#region Is2003Format

		public static bool Is2003Format(WorkbookFormat format)
		{
			return Utilities.Is2007Format(format) == false;
		}

		#endregion // Is2003Format

		// MD 5/7/10 - 10.2 - Excel Templates
		#region Is2007Format

		public static bool Is2007Format(WorkbookFormat fileFormat)
		{
			switch (fileFormat)
			{
				case WorkbookFormat.Excel97To2003:
				case WorkbookFormat.Excel97To2003Template:
					return false;

				case WorkbookFormat.Excel2007:
				case WorkbookFormat.Excel2007MacroEnabled:
				case WorkbookFormat.Excel2007MacroEnabledTemplate:
				case WorkbookFormat.Excel2007Template:
					return true;

				default:
					Utilities.DebugFail("Unknown file format: " + fileFormat);
					return false;
			}
		}

		#endregion Is2007Format

		// MD 5/7/10 - 10.2 - Excel Templates
		#region IsMacroEnabledFormat

		public static bool IsMacroEnabledFormat(WorkbookFormat fileFormat)
		{
			switch (fileFormat)
			{
				case WorkbookFormat.Excel97To2003:
				case WorkbookFormat.Excel97To2003Template:
				case WorkbookFormat.Excel2007MacroEnabled:
				case WorkbookFormat.Excel2007MacroEnabledTemplate:
					return true;

				case WorkbookFormat.Excel2007:
				case WorkbookFormat.Excel2007Template:
					return false;

				default:
					Utilities.DebugFail("Unknown file format: " + fileFormat);
					return false;
			}
		}

		#endregion IsMacroEnabledFormat

        // MBS 7/25/08 - Excel 2007 Format
		// MD 10/21/10 - TFS34398
		#region IsCellBorderLineStyleDefined

		public static bool IsCellBorderLineStyleDefined(CellBorderLineStyle value)
		{
			switch (value)
			{
				case CellBorderLineStyle.Default:
				case CellBorderLineStyle.None:
				case CellBorderLineStyle.Thin:
				case CellBorderLineStyle.Medium:
				case CellBorderLineStyle.Dashed:
				case CellBorderLineStyle.Dotted:
				case CellBorderLineStyle.Thick:
				case CellBorderLineStyle.Double:
				case CellBorderLineStyle.Hair:
				case CellBorderLineStyle.MediumDashed:
				case CellBorderLineStyle.DashDot:
				case CellBorderLineStyle.MediumDashDot:
				case CellBorderLineStyle.DashDotDot:
				case CellBorderLineStyle.MediumDashDotDot:
				case CellBorderLineStyle.SlantedDashDot:
					return true;
			}

			return false;
		} 

		#endregion // IsCellBorderLineStyleDefined

		// MD 10/22/10 - TFS36696
		#region IsCellReferenceModeDefined

		public static bool IsCellReferenceModeDefined(CellReferenceMode value)
		{
			switch (value)
			{
				case CellReferenceMode.A1:
				case CellReferenceMode.R1C1:
					return true;
			}

			return false;
		}

		#endregion // IsCellReferenceModeDefined

		// MD 10/21/10 - TFS34398
		#region IsExcelDefaultableBooleanDefined

		public static bool IsExcelDefaultableBooleanDefined(ExcelDefaultableBoolean value)
		{
			switch (value)
			{
				case ExcelDefaultableBoolean.Default:
				case ExcelDefaultableBoolean.False:
				case ExcelDefaultableBoolean.True:
					return true;
			}

			return false;
		}

		#endregion // IsExcelDefaultableBooleanDefined

		// MD 1/19/12 - 12.1 - Cell Format Updates
		// This is not used anymore.
		#region Removed

		//// MD 10/21/10 - TFS34398
		//#region IsFillPatternStyleDefined

		//public static bool IsFillPatternStyleDefined(FillPatternStyle value)
		//{
		//    switch (value)
		//    {
		//        case FillPatternStyle.Default:
		//        case FillPatternStyle.None:
		//        case FillPatternStyle.Solid:
		//        case FillPatternStyle.Gray50percent:
		//        case FillPatternStyle.Gray75percent:
		//        case FillPatternStyle.Gray25percent:
		//        case FillPatternStyle.HorizontalStripe:
		//        case FillPatternStyle.VerticalStripe:
		//        case FillPatternStyle.ReverseDiagonalStripe:
		//        case FillPatternStyle.DiagonalStripe:
		//        case FillPatternStyle.DiagonalCrosshatch:
		//        case FillPatternStyle.ThickDiagonalCrosshatch:
		//        case FillPatternStyle.ThinHorizontalStripe:
		//        case FillPatternStyle.ThinVerticalStripe:
		//        case FillPatternStyle.ThinReverseDiagonalStripe:
		//        case FillPatternStyle.ThinDiagonalStripe:
		//        case FillPatternStyle.ThinHorizontalCrosshatch:
		//        case FillPatternStyle.ThinDiagonalCrosshatch:
		//        case FillPatternStyle.Gray12percent:
		//        case FillPatternStyle.Gray6percent:
		//            return true;
		//    }

		//    return false;
		//}

		//#endregion // IsFillPatternStyleDefined

		#endregion // Removed

		// MD 10/21/10 - TFS34398
		#region IsFontSuperscriptSubscriptStyleDefined

		public static bool IsFontSuperscriptSubscriptStyleDefined(FontSuperscriptSubscriptStyle value)
		{
			switch (value)
			{
				case FontSuperscriptSubscriptStyle.Default:
				case FontSuperscriptSubscriptStyle.None:
				case FontSuperscriptSubscriptStyle.Superscript:
				case FontSuperscriptSubscriptStyle.Subscript:
					return true;
			}

			return false;
		}

		#endregion // IsFontSuperscriptSubscriptStyleDefined

		// MD 10/21/10 - TFS34398
		#region IsFontUnderlineStyleDefined

		public static bool IsFontUnderlineStyleDefined(FontUnderlineStyle value)
		{
			switch (value)
			{
				case FontUnderlineStyle.Default:
				case FontUnderlineStyle.None:
				case FontUnderlineStyle.Single:
				case FontUnderlineStyle.Double:
				case FontUnderlineStyle.SingleAccounting:
				case FontUnderlineStyle.DoubleAccounting:
					return true;
			}

			return false;
		}

		#endregion // IsFontUnderlineStyleDefined

		// MD 10/21/10 - TFS34398
		#region IsHorizontalCellAlignmentDefined

		public static bool IsHorizontalCellAlignmentDefined(HorizontalCellAlignment value)
		{
			switch (value)
			{
				case HorizontalCellAlignment.Default:
				case HorizontalCellAlignment.General:
				case HorizontalCellAlignment.Left:
				case HorizontalCellAlignment.Center:
				case HorizontalCellAlignment.Right:
				case HorizontalCellAlignment.Fill:
				case HorizontalCellAlignment.Justify:
				case HorizontalCellAlignment.CenterAcrossSelection:
				case HorizontalCellAlignment.Distributed:
					return true;
			}

			return false;
		}

		#endregion // IsHorizontalCellAlignmentDefined

		// MBS 7/25/08 - Excel 2007 Format
        // Copied from Infragistics.Win.Utilities
        //
        // JDN 11/16/04 Added IsNumericType method as support for BR00297
        #region IsNumericType



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		// MD 1/19/11 - TFS37369
		// We should now test a value instead of a tpye becasue some doubles shouldn't be saved as numeric types.
        //public static bool IsNumericType(System.Type type)
		public static bool IsNumericType(object value)
        {
			// MD 1/19/11 - TFS37369
			// If doubles can't be expressed with actual digits, we shouldn't consider them numbers because we want 
			// to save them as strings.
			if (value is double)
			{
				double doubleValue = (double)value;

				if (Double.IsInfinity(doubleValue) || Double.IsNaN(doubleValue))
					return false;

				return true;
			}

			// MD 1/19/11 - TFS37369
			System.Type type = value.GetType();

            if (type.IsPrimitive || type == typeof(decimal))
            {
                if (type != typeof(bool) && type != typeof(char))
                    return true;
                else
                    return false;
            }
            return false;
        }

        #endregion // IsNumericType

		// MD 11/24/10 - TFS34598
		#region IsRGBColorFormatSupported

		internal static bool IsRGBColorFormatSupported(WorkbookFormat workbookFormat)
		{
			// MD 2/4/11
			// Done while fixing TFS65015
			// Use the new Utilities.Is2003Format method so we don't need to switch on the format all over the place.
			//switch (workbookFormat)
			//{
			//    case WorkbookFormat.Excel97To2003:
			//    case WorkbookFormat.Excel97To2003Template: 
			//        return false;
			//}
			//
			//return true;
			return Utilities.Is2007Format(workbookFormat);
		} 

		#endregion // IsRGBColorFormatSupported

		#region IsUnicodeString

		public static bool IsUnicodeString( string value )
		{
			// MD 7/25/07 - BR25202
			// When the default language on the machine is set to another language, some of the ascii characters
			// are mapped to common unicode characters used in the language.  The GetBytes and GetString methods used 
			// below can translate with these mappings because they are stored in the default encoding. Excel cannot 
			// make these tranformations. A better test needs to be done to determine if the string needs to be 
			// encoded as a unicode string
			//return value != Encoding.Default.GetString( Encoding.Default.GetBytes( value ) );
			const int MaxBlockSize = 100;

			int startIndex = 0;
			while ( startIndex < value.Length )
			{
				int length = value.Length - startIndex;

				// Only allow 100 characters of the string to be checked at a time so we cut down on the memory used
				if ( length > MaxBlockSize )
					length = MaxBlockSize;

				byte[] unicodeValues = Encoding.Unicode.GetBytes( value.Substring( startIndex, length ) );

				for ( int i = 1; i < unicodeValues.Length; i += 2 )
				{
					// If the second byte of any character is not zero, the entire string needs to be encoded as unicode
					if ( unicodeValues[ i ] != 0 )
						return true;
				}

				startIndex += length;
			}

			return false;
		}

		#endregion IsUnicodeString

		// MD 10/21/10 - TFS34398
		#region IsVerticalCellAlignmentDefined

		public static bool IsVerticalCellAlignmentDefined(VerticalCellAlignment value)
		{
			switch (value)
			{
				case VerticalCellAlignment.Default:
				case VerticalCellAlignment.Top:
				case VerticalCellAlignment.Center:
				case VerticalCellAlignment.Bottom:
				case VerticalCellAlignment.Justify:
				case VerticalCellAlignment.Distributed:
					return true;
			}

			return false;
		}

		#endregion // IsVerticalCellAlignmentDefined

		// MD 10/22/10 - TFS36696
		#region IsWorkbookFormatDefined

		public static bool IsWorkbookFormatDefined(WorkbookFormat value)
		{
			switch (value)
			{
				case WorkbookFormat.Excel97To2003:
				case WorkbookFormat.Excel97To2003Template:
				case WorkbookFormat.Excel2007:
				case WorkbookFormat.Excel2007Template:
				case WorkbookFormat.Excel2007MacroEnabled:
				case WorkbookFormat.Excel2007MacroEnabledTemplate:
					return true;
			}

			return false;
		}

		#endregion // IsWorkbookFormatDefined

		// MD 10/27/10 - TFS56976
		// We don't need this anymore. Also, I don't think it was correct. It looks like it was just checking for the existence of a 3D reference, 
		// but regular cells can have 3D references as well.
		//#region NamedReferenceRegex
		//
		//internal static Regex NamedReferenceRegex
		//{
		//    get
		//    {
		//        if (namedReferenceRegex == null)
		//            namedReferenceRegex = new Regex(@"\w+!\w+", RegexOptionsCompiled);
		//
		//        return namedReferenceRegex;
		//    }
		//}
		//
		//#endregion //NamedReferenceReges

        // MBS 7/22/08 - Excel 2007 Format
        // Moved from CalcUtilities
        #region ParseA1CellAddress

		// MD 4/6/12 - TFS101506
        //public static bool ParseA1CellAddress(string address, WorkbookFormat format, out short columnIndex, out int rowIndex)
		public static bool ParseA1CellAddress(string address, WorkbookFormat format, CultureInfo culture, out short columnIndex, out int rowIndex)
        {
            columnIndex = 0;
            rowIndex = 0;

            bool isColumnRelative = false;
            int columnMatchLength;
			// MD 4/6/12 - TFS101506
            //if (FormulaParser.ParseColumnAddressA1(address, 0, format, ref columnIndex, ref isColumnRelative, out columnMatchLength) == false)
			if (FormulaParser.ParseColumnAddressA1(address, 0, format, culture, ref columnIndex, ref isColumnRelative, out columnMatchLength) == false)
                return false;

            bool isRowRelative = false;
            int rowMatchLength;
            if (FormulaParser.ParseRowAddressA1(address, columnMatchLength, format, ref rowIndex, ref isRowRelative, out rowMatchLength) == false)
                return false;

			return rowMatchLength + columnMatchLength == address.Length;
        }

        #endregion ParseA1CellAddress
        //
		#region ParseA1RegionAddress

		// MD 4/6/12 - TFS101506
		//public static WorksheetRegion ParseA1RegionAddress( string address, Worksheet worksheet )
		public static WorksheetRegion ParseA1RegionAddress(string address, Worksheet worksheet, CultureInfo culture)
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetRegion region;
			//WorksheetCell cell;
			//Utilities.ParseA1RegionAddress( address, worksheet, out region, out cell );
			//
			//if ( region != null )
			//    return region;
			//
			//if ( cell != null )
			//    return cell.CachedRegion;
			int firstRowIndex;
			short firstColumnIndex;
			int lastRowIndex;
			short lastColumnIndex;
			// MD 5/13/11 - Data Validations / Page Breaks
			//Utilities.ParseA1RegionAddress(address, worksheet, out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);
			// MD 4/9/12 - TFS101506
			//Utilities.ParseA1RegionAddress(address, worksheet.CurrentFormat, out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);
			Utilities.ParseA1RegionAddress(address, worksheet.CurrentFormat, culture, out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

			if (0 <= lastRowIndex)
				return worksheet.GetCachedRegion(firstRowIndex, firstColumnIndex, lastRowIndex, lastColumnIndex);

			if (0 <= firstRowIndex)
				return worksheet.GetCachedRegion(firstRowIndex, firstColumnIndex, firstRowIndex, firstColumnIndex);

			return null;
		}

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static void ParseA1RegionAddress( string address, Worksheet worksheet, out WorksheetRegion region, out WorksheetCell cell )
		// MD 5/13/11 - Data Validations / Page Breaks
		// Some callers may not have a worksheet instance, and since we only need it for the format anyway, just take that instead.
		//public static void ParseA1RegionAddress(string address, Worksheet worksheet, 
		// MD 4/6/12 - TFS101506
		//public static void ParseA1RegionAddress(string address, WorkbookFormat format, 
		public static void ParseA1RegionAddress(string address, WorkbookFormat format, CultureInfo culture,
			out int firstRowIndex, 
			out short firstColumnIndex,
			out int lastRowIndex,
			out short lastColumnIndex)
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//region = null;
			//cell = null;
			firstRowIndex = -1;
			firstColumnIndex = -1;
			lastRowIndex = -1;
			lastColumnIndex = -1;

			address = address.TrimEnd();

			// MD 5/13/11 - Data Validations / Page Breaks
			// The format is now passed in.
			//WorkbookFormat format = worksheet.CurrentFormat;

			int rangeOperatorIndex = address.IndexOf( FormulaParser.RangeOperator );
			if ( rangeOperatorIndex >= 0 )
			{
				string rangeStartAddress = address.Substring( 0, rangeOperatorIndex );
				rangeStartAddress = rangeStartAddress.TrimEnd();
				string rangeEndAddress = address.Substring( rangeOperatorIndex + 1 );
				rangeEndAddress = rangeEndAddress.TrimStart();

				bool isValidAddress = false;

				short rangeStartColumnIndex = 0;
				int rangeStartRowIndex = 0;
				short rangeEndColumnIndex = 0;
				int rangeEndRowIndex = 0;

				// MD 4/6/12 - TFS101506
				//if ( Utilities.ParseA1CellAddress( rangeStartAddress, format, out rangeStartColumnIndex, out rangeStartRowIndex ) )
				//{
				//    if ( Utilities.ParseA1CellAddress( rangeEndAddress, format, out rangeEndColumnIndex, out rangeEndRowIndex ) )
				//        isValidAddress = true;
				//}
				if (Utilities.ParseA1CellAddress(rangeStartAddress, format, culture, out rangeStartColumnIndex, out rangeStartRowIndex))
				{
					if (Utilities.ParseA1CellAddress(rangeEndAddress, format, culture, out rangeEndColumnIndex, out rangeEndRowIndex))
						isValidAddress = true;
				}

				if ( isValidAddress == false )
				{
					bool isColumnRelative = false;
					int columnMatchLength;
					// MD 4/6/12 - TFS101506
					//if ( FormulaParser.ParseColumnAddressA1( rangeStartAddress, 0, format, ref rangeStartColumnIndex, ref isColumnRelative, out columnMatchLength ) )
					if (FormulaParser.ParseColumnAddressA1(rangeStartAddress, 0, format, culture, ref rangeStartColumnIndex, ref isColumnRelative, out columnMatchLength))
					{
						// MD 9/17/08
						// Only continue if the entire address matches
						if ( columnMatchLength != rangeStartAddress.Length )
							return;

						// MD 4/6/12 - TFS101506
						//if ( FormulaParser.ParseColumnAddressA1( rangeEndAddress, 0, format, ref rangeEndColumnIndex, ref isColumnRelative, out columnMatchLength ) )
						if (FormulaParser.ParseColumnAddressA1(rangeEndAddress, 0, format, culture, ref rangeEndColumnIndex, ref isColumnRelative, out columnMatchLength))
						{
							// MD 9/17/08
							// Only continue if the entire address matches
							if ( columnMatchLength != rangeEndAddress.Length )
								return;

							rangeStartRowIndex = 0;
							rangeEndRowIndex = Workbook.GetMaxRowCount( format ) - 1;

							isValidAddress = true;
						}
					}
				}

				if ( isValidAddress == false )
				{
					bool isRowRelative = false;
					int rowMatchLength;
					if ( FormulaParser.ParseRowAddressA1( rangeStartAddress, 0, format, ref rangeStartRowIndex, ref isRowRelative, out rowMatchLength ) )
					{
						// MD 9/17/08
						// Only continue if the entire address matches
						if ( rowMatchLength != rangeStartAddress.Length )
							return;

						if ( FormulaParser.ParseRowAddressA1( rangeEndAddress, 0, format, ref rangeEndRowIndex, ref isRowRelative, out rowMatchLength ) )
						{
							// MD 9/17/08
							// Only continue if the entire address matches
							if ( rowMatchLength != rangeEndAddress.Length )
								return;

							rangeStartColumnIndex = 0;
							rangeEndColumnIndex = (short)( Workbook.GetMaxColumnCount( format ) - 1 );

							isValidAddress = true;
						}
					}
				}

				if ( isValidAddress == false )
					return;

				if ( rangeEndColumnIndex < rangeStartColumnIndex )
					Utilities.SwapValues( ref rangeStartColumnIndex, ref rangeEndColumnIndex );

				if ( rangeEndRowIndex < rangeStartRowIndex )
					Utilities.SwapValues( ref rangeStartRowIndex, ref rangeEndRowIndex );

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//region = worksheet.GetCachedRegion( rangeStartRowIndex, rangeStartColumnIndex, rangeEndRowIndex, rangeEndColumnIndex );
				firstRowIndex = rangeStartRowIndex;
				firstColumnIndex = rangeStartColumnIndex;
				lastRowIndex = rangeEndRowIndex;
				lastColumnIndex = rangeEndColumnIndex;

				return;
			}
			else
			{
				short columnIndex;
				int rowIndex;
				// MD 4/6/12 - TFS101506
				//if ( Utilities.ParseA1CellAddress( address, format, out columnIndex, out rowIndex ) )
				if (Utilities.ParseA1CellAddress(address, format, culture, out columnIndex, out rowIndex))
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//cell = worksheet.Rows[ rowIndex ].Cells[ columnIndex ];
					firstRowIndex = rowIndex;
					firstColumnIndex = columnIndex;

					return;
				}
			}
		}

        #endregion //ParseA1RegionAddress

		// MD 8/22/08 - Excel formula solving
		#region ParseR1C1CellAddress

		public static bool ParseR1C1CellAddress( string address, WorkbookFormat format, out short columnNumber, out bool columnNumberIsOffset, out int rowNumber, out bool rowNumberIsOffset )
		{
			columnNumber = 0;
			columnNumberIsOffset = false;
			rowNumber = 0;
			rowNumberIsOffset = false;

			int rowMatchLength;
			if ( FormulaParser.ParseRowAddressR1C1( address, 0, format, ref rowNumber, ref rowNumberIsOffset, out rowMatchLength ) == false )
				return false;

			int columnMatchLength;
			if ( FormulaParser.ParseColumnAddressR1C1( address, rowMatchLength, format, ref columnNumber, ref columnNumberIsOffset, out columnMatchLength ) == false )
				return false;

			return rowMatchLength + columnMatchLength == address.Length;
		}

		#endregion ParseR1C1CellAddress

		#region ParseR1C1RegionAddress

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static void ParseR1C1RegionAddress( string address, Worksheet worksheet, WorksheetCell originCell, out WorksheetRegion region, out WorksheetCell cell )
		public static void ParseR1C1RegionAddress(
			string address,
			// MD 5/13/11 - Data Validations / Page Breaks
			// Some callers may not have a worksheet instance, and since we only need it for the format anyway, just take that instead.
			//Worksheet worksheet, 
			WorkbookFormat format,
			WorksheetRow originCellRow,
			short originCellColumnIndex,
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//out WorksheetRegion region, 
			//out WorksheetCell cell)
			out int firstRowIndex,
			out short firstColumnIndex,
			out int lastRowIndex,
			out short lastColumnIndex)
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//region = null;
			//cell = null;
			firstRowIndex = -1;
			firstColumnIndex = -1;
			lastRowIndex = -1;
			lastColumnIndex = -1;

			address = address.TrimEnd();

			// MD 5/13/11 - Data Validations / Page Breaks
			// The format is now passed in.
			//WorkbookFormat format = worksheet.CurrentFormat;

			int rangeOperatorIndex = address.IndexOf( FormulaParser.RangeOperator );
			if ( rangeOperatorIndex >= 0 )
			{
				string rangeStartAddress = address.Substring( 0, rangeOperatorIndex );
				rangeStartAddress = rangeStartAddress.TrimEnd();
				string rangeEndAddress = address.Substring( rangeOperatorIndex + 1 );
				rangeEndAddress = rangeEndAddress.TrimStart();

				bool isValidAddress = false;

				short rangeStartColumnNumber = 0;
				bool rangeStartColumnNumberIsOffset = false;
				int rangeStartRowNumber = 0;
				bool rangeStartRowNumberIsOffset = false;
				short rangeEndColumnNumber = 0;
				bool rangeEndColumnNumberIsOffset = false;
				int rangeEndRowNumber = 0;
				bool rangeEndRowNumberIsOffset = false;
				if ( Utilities.ParseR1C1CellAddress( rangeStartAddress, format, out rangeStartColumnNumber, out rangeStartColumnNumberIsOffset, out rangeStartRowNumber, out rangeStartRowNumberIsOffset ) )
				{
					if ( Utilities.ParseR1C1CellAddress( rangeEndAddress, format, out rangeEndColumnNumber, out rangeEndColumnNumberIsOffset, out rangeEndRowNumber, out rangeEndRowNumberIsOffset ) )
						isValidAddress = true;
				}

				if ( isValidAddress == false )
				{
					int columnMatchLength;
					if ( FormulaParser.ParseColumnAddressR1C1( rangeStartAddress, 0, format, ref rangeStartColumnNumber, ref rangeStartColumnNumberIsOffset, out columnMatchLength ) )
					{
						// MD 9/17/08
						// Only continue if the entire address matches
						if ( columnMatchLength != rangeStartAddress.Length )
							return;

						if ( FormulaParser.ParseColumnAddressR1C1( rangeEndAddress, 0, format, ref rangeEndColumnNumber, ref rangeEndColumnNumberIsOffset, out columnMatchLength ) )
						{
							// MD 9/17/08
							// Only continue if the entire address matches
							if ( columnMatchLength != rangeEndAddress.Length )
								return;

							rangeStartRowNumber = 0;
							rangeStartRowNumberIsOffset = false;
							rangeEndRowNumber = Workbook.GetMaxRowCount( format ) - 1;
							rangeEndRowNumberIsOffset = false;

							isValidAddress = true;
						}
					}
				}

				if ( isValidAddress == false )
				{
					int rowMatchLength;
					if ( FormulaParser.ParseRowAddressR1C1( rangeStartAddress, 0, format, ref rangeStartRowNumber, ref rangeStartRowNumberIsOffset, out rowMatchLength ) )
					{
						// MD 9/17/08
						// Only continue if the entire address matches
						if ( rowMatchLength != rangeStartAddress.Length )
							return;

						if ( FormulaParser.ParseRowAddressR1C1( rangeEndAddress, 0, format, ref rangeEndRowNumber, ref rangeEndRowNumberIsOffset, out rowMatchLength ) )
						{
							// MD 9/17/08
							// Only continue if the entire address matches
							if ( rowMatchLength != rangeEndAddress.Length )
								return;

							rangeStartColumnNumber = 0;
							rangeStartColumnNumberIsOffset = false;
							rangeEndColumnNumber = (short)( Workbook.GetMaxColumnCount( format ) - 1 );
							rangeEndColumnNumberIsOffset = false;

							isValidAddress = true;
						}
					}
				}

				if ( isValidAddress == false )
					return;

				if ( rangeEndColumnNumberIsOffset || rangeEndRowNumberIsOffset || rangeStartColumnNumberIsOffset || rangeStartRowNumberIsOffset )
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//if ( originCell == null )
					//    return;
					//
					//rangeStartColumnNumber = (short)Utilities.GetColumnNumber( rangeStartColumnNumber, rangeStartColumnNumberIsOffset, originCell );
					//rangeStartRowNumber = Utilities.GetRowNumber( rangeStartRowNumber, rangeStartRowNumberIsOffset, originCell );
					//
					//rangeEndColumnNumber = (short)Utilities.GetColumnNumber( rangeEndColumnNumber, rangeEndColumnNumberIsOffset, originCell );
					//rangeEndRowNumber = Utilities.GetRowNumber( rangeEndRowNumber, rangeEndRowNumberIsOffset, originCell );
					if (originCellRow == null)
						return;

					rangeStartColumnNumber = (short)Utilities.GetColumnNumber(rangeStartColumnNumber, rangeStartColumnNumberIsOffset, originCellRow, originCellColumnIndex);
					rangeStartRowNumber = Utilities.GetRowNumber(rangeStartRowNumber, rangeStartRowNumberIsOffset, originCellRow);

					rangeEndColumnNumber = (short)Utilities.GetColumnNumber(rangeEndColumnNumber, rangeEndColumnNumberIsOffset, originCellRow, originCellColumnIndex);
					rangeEndRowNumber = Utilities.GetRowNumber(rangeEndRowNumber, rangeEndRowNumberIsOffset, originCellRow);
				}


				if ( rangeEndColumnNumber < rangeStartColumnNumber )
					Utilities.SwapValues( ref rangeStartColumnNumber, ref rangeEndColumnNumber );

				if ( rangeEndRowNumber < rangeStartRowNumber )
					Utilities.SwapValues( ref rangeStartRowNumber, ref rangeEndRowNumber );

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//region = worksheet.GetCachedRegion( rangeStartRowNumber, rangeStartColumnNumber, rangeEndRowNumber, rangeEndColumnNumber );
				firstRowIndex = rangeStartRowNumber;
				firstColumnIndex = rangeStartColumnNumber;
				lastRowIndex = rangeEndRowNumber;
				lastColumnIndex = rangeEndColumnNumber;

				return;
			}
			else
			{
				short columnNumber;
				bool columnNumberIsOffset;
				int rowNumber;
				bool rowNumberIsOffset;
				if ( Utilities.ParseR1C1CellAddress( address, format, out columnNumber, out columnNumberIsOffset, out rowNumber, out rowNumberIsOffset ) )
				{
					if ( columnNumberIsOffset || rowNumberIsOffset )
					{
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//if ( originCell == null )
						//    return;
						//
						//columnNumber = (short)Utilities.GetColumnNumber( columnNumber, columnNumberIsOffset, originCell );
						//rowNumber = Utilities.GetRowNumber( rowNumber, rowNumberIsOffset, originCell );
						if (originCellRow == null)
							return;

						columnNumber = (short)Utilities.GetColumnNumber(columnNumber, columnNumberIsOffset, originCellRow, originCellColumnIndex);
						rowNumber = Utilities.GetRowNumber(rowNumber, rowNumberIsOffset, originCellRow);
					}

					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//cell = worksheet.Rows[ rowNumber ].Cells[ columnNumber ];
					firstRowIndex = rowNumber;
					firstColumnIndex = columnNumber;

					return;
				}

				int columnMatchLength;
				if ( FormulaParser.ParseColumnAddressR1C1( address, 0, format, ref columnNumber, ref columnNumberIsOffset, out columnMatchLength ) )
				{
					// MD 9/17/08
					// Only continue if the entire address matches
					if ( columnMatchLength != address.Length )
						return;

					if ( columnNumberIsOffset )
					{
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//if ( originCell == null )
						//    return;
						//
						//columnNumber = (short)Utilities.GetColumnNumber( columnNumber, columnNumberIsOffset, originCell );
						if (originCellRow == null)
							return;

						columnNumber = (short)Utilities.GetColumnNumber(columnNumber, columnNumberIsOffset, originCellRow, originCellColumnIndex);
					}

					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//region = worksheet.GetCachedRegion( 0, columnNumber, Workbook.GetMaxRowCount( format ) - 1, columnNumber );
					firstRowIndex = 0;
					firstColumnIndex = columnNumber;
					lastRowIndex = Workbook.GetMaxRowCount(format) - 1;
					lastColumnIndex = columnNumber;
				}

				int rowMatchLength;
				if ( FormulaParser.ParseRowAddressR1C1( address, 0, format, ref rowNumber, ref rowNumberIsOffset, out rowMatchLength ) )
				{
					// MD 9/17/08
					// Only continue if the entire address matches
					if ( rowMatchLength != address.Length )
						return;

					if ( rowNumberIsOffset )
					{
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//if ( originCell == null )
						//    return;
						//
						//rowNumber = (short)Utilities.GetRowNumber( rowNumber, rowNumberIsOffset, originCell );
						if (originCellRow == null)
							return;

						rowNumber = (short)Utilities.GetRowNumber(rowNumber, rowNumberIsOffset, originCellRow);
					}

					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//region = worksheet.GetCachedRegion( rowNumber, 0, rowNumber, Workbook.GetMaxColumnCount( format ) - 1 );
					firstRowIndex = rowNumber;
					firstColumnIndex = 0;
					lastRowIndex = rowNumber;
					lastColumnIndex = (short)(Workbook.GetMaxColumnCountInternal(format) - 1);
				}
			}
		}

		#endregion //ParseR1C1RegionAddress

		#region ParseRegionAddress

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static void ParseRegionAddress( string address, Worksheet worksheet, CellReferenceMode cellReferenceMode, WorksheetCell originCell, out WorksheetRegion region, out WorksheetCell cell )
		public static void ParseRegionAddress(
			string address,
			// MD 5/13/11 - Data Validations / Page Breaks
			// Some callers may not have a worksheet instance, and since we only need it for the format anyway, just take that instead.
			//Worksheet worksheet, 
			WorkbookFormat format, 
			CellReferenceMode cellReferenceMode, 
			// MD 4/6/12 - TFS101506
			CultureInfo culture,
			WorksheetRow originCellRow,
			short originCellColumnIndex,
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//out WorksheetRegion region, 
			//out WorksheetCell cell)
			out int firstRowIndex,
			out short firstColumnIndex,
			out int lastRowIndex,
			out short lastColumnIndex)
		{
			if ( cellReferenceMode == CellReferenceMode.A1 )
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//Utilities.ParseA1RegionAddress( address, worksheet, out region, out cell );
				// MD 5/13/11 - Data Validations / Page Breaks
				//Utilities.ParseA1RegionAddress(address, worksheet, out firstRowIndex, out firstColumnIndex, out  lastRowIndex, out lastColumnIndex);
				// MD 4/9/12 - TFS101506
				//Utilities.ParseA1RegionAddress(address, format, out firstRowIndex, out firstColumnIndex, out  lastRowIndex, out lastColumnIndex);
				Utilities.ParseA1RegionAddress(address, format, culture, out firstRowIndex, out firstColumnIndex, out  lastRowIndex, out lastColumnIndex);
			}
			else
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//Utilities.ParseR1C1RegionAddress( address, worksheet, originCell, out region, out cell );
				// MD 5/13/11 - Data Validations / Page Breaks
				//Utilities.ParseR1C1RegionAddress(address, worksheet, originCellRow, originCellColumnIndex, out firstRowIndex, out firstColumnIndex, out  lastRowIndex, out lastColumnIndex);
				Utilities.ParseR1C1RegionAddress(address, format, originCellRow, originCellColumnIndex, out firstRowIndex, out firstColumnIndex, out  lastRowIndex, out lastColumnIndex);
			}
		} 

		#endregion ParseRegionAddress

		#region GetRowNumber

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private static int GetRowNumber( int rowNumber, bool rowNumberIsOffset, WorksheetCell originCell )
		private static int GetRowNumber(int rowNumber, bool rowNumberIsOffset, WorksheetRow originCellRow)
		{
			// MD 7/26/10 - TFS34398
			// Now that the cell stores the row instead of the worksheet, we should cache the row and get the index and 
			// worksheet from that.
			//return Utilities.GetAddressNumberHelper(
			//    rowNumber, rowNumberIsOffset, originCell.RowIndex,
			//    Workbook.GetMaxRowCount( originCell.Worksheet.CurrentFormat ) );
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetRow row = originCell.Row;
			//return Utilities.GetAddressNumberHelper(
			//    rowNumber, rowNumberIsOffset, row.Index,
			//    Workbook.GetMaxRowCount(row.Worksheet.CurrentFormat));
			return Utilities.GetAddressNumberHelper(
				rowNumber, rowNumberIsOffset, originCellRow.Index,
				Workbook.GetMaxRowCount(originCellRow.Worksheet.CurrentFormat));
		} 

		#endregion GetRowNumber

		#region GetColumnNumber

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private static int GetColumnNumber( int columnNumber, bool columnNumberIsOffset, WorksheetCell originCell )
		private static int GetColumnNumber(int columnNumber, bool columnNumberIsOffset, WorksheetRow originCellRow, short originCellColumnIndex)
		{
			return Utilities.GetAddressNumberHelper(
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//columnNumber, columnNumberIsOffset, originCell.ColumnIndex,
				//Workbook.GetMaxColumnCount( originCell.Worksheet.CurrentFormat ) );
				columnNumber, columnNumberIsOffset, originCellColumnIndex,
				Workbook.GetMaxColumnCount(originCellRow.Worksheet.CurrentFormat));
		} 

		#endregion GetColumnNumber

		#region GetAddressNumberHelper

		private static int GetAddressNumberHelper( int addressNumber, bool addressNumberIsOffset, int originCellIndex, int maxAddress )
		{
			if ( addressNumberIsOffset == false )
				return addressNumber;

			addressNumber += originCellIndex;

			if ( addressNumber < 0 )
				addressNumber += maxAddress;
			else if ( maxAddress <= addressNumber )
				addressNumber -= maxAddress;

			return addressNumber;
		} 

		#endregion GetAddressNumberHelper
		// -----------------------------------------

		#region ReadEMURect







		public static Rectangle ReadEMURect( BiffRecordStream stream )
		{
			int leftEMU = stream.ReadInt32();
			int topEMU = stream.ReadInt32();
			int rightEMU = stream.ReadInt32();
			int bottomEMU = stream.ReadInt32();

		    int left = EMUToTwips(leftEMU);
		    int top = EMUToTwips(topEMU);
		    int right = EMUToTwips(rightEMU);
		    int bottom = EMUToTwips(bottomEMU);

            return new Rectangle(left, top, right - left, bottom - top);

		}

		#endregion ReadEMURect

		#region ReduceFraction

		public static void ReduceFraction( ref int numerator, ref int denominator )
		{
			// If the numerator and denominator are equal, the fraction is 1/1
			if ( numerator == denominator )
			{
				numerator = 1;
				denominator = 1;
				return;
			}

			// If the numerator is the additive inverse of the denominator, the fraction is 1/-1
			if ( numerator == -denominator )
			{
				numerator = 1;
				denominator = -1;
				return;
			}

			// Otherwise, try to remove all common prime factors of the numerator and denominator
			for ( int i = 0; i < Utilities.Primes.Length; i++ )
			{
				int prime = Utilities.Primes[ i ];

				// If the prime is greater than either the numerator or denominator, stop reducing
				if ( numerator < prime || denominator < prime )
					break;

				// If both the numerator and denominator are divisible by the prime, reduce them both 
				// by the prime as many times as we can
				while ( numerator % prime == 0 && denominator % prime == 0 )
				{
					numerator /= prime;
					denominator /= prime;
				}
			}
		}

		#endregion ReduceFraction

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 1/15/08 - BR29635
		//#region RemoveValueFromSortedList

		//public static void RemoveValueFromSortedList<T>( T value, List<T> list )
		//{
		//    int index = list.BinarySearch( value );

		//    if ( index < 0 )
		//        return;

		//    list.RemoveAt( index );
		//}

		//#endregion RemoveValueFromSortedList

		#endregion // Removed

		// MD 5/12/10 - TFS26732
		#region ResolveCellBorderValue



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static object ResolveCellBorderValue(WorksheetCell cell, WorksheetCell adjacentCell, CellFormatValue value)
		public static object ResolveCellBorderValue(
			Worksheet worksheet,	// MD 3/22/12 - TFS104630
			WorksheetRow row, short columnIndex, 
			WorksheetRow adjacentRow, short adjacentColumnIndex, 
			CellFormatValue value)
		{
			// MD 3/22/12 - TFS104630
			// This code can be simpler now with some of the helper methods available.
			#region Old Code

			//// MD 1/8/12 - 12.1 - Cell Format Updates
			//// Moved GetDefaultValue to the WorksheetCellFormatData.
			////object cellBorderValue = WorksheetCellFormatProxy.GetDefaultValue(value);
			//object cellBorderValue = WorksheetCellFormatData.GetDefaultValue(value);

			//// MD 12/31/11 - 12.1 - Cell Format Updates
			//// This is no longer true. The Style property has a null default value.
			////// MD 10/26/11 - TFS91546
			////// GetDefaultValue may return null for properties which don't have a default value (one that will resolve from owners).
			////if (cellBorderValue == null)
			////{
			////    Utilities.DebugFail("The default value is null here.");
			////    return CellBorderLineStyle.None;
			////}

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////if (cell != null && cell.HasCellFormat)
			////    cellBorderValue = cell.CellFormatInternal.GetValue(value);
			//// MD 4/18/11 - TFS62026
			//// Since we are not setting any properties, we just need the element, not the proxy, so get that instead.
			////WorksheetCellFormatProxy cellFormat;
			//WorksheetCellFormatData cellFormat;
			//// MD 2/25/12 - 12.1 - Table Support
			////if (row != null && row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
			//if (row != null && row.TryGetCellFormat(columnIndex, out cellFormat))
			//    cellBorderValue = cellFormat.GetValue(value);

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////if (WorksheetCellFormatProxy.IsValueDefault(value, cellBorderValue) && adjacentCell != null && adjacentCell.HasCellFormat)
			////    cellBorderValue = adjacentCell.CellFormatInternal.GetValue(Utilities.GetOppositeBorderValue(value));
			//// MD 1/8/12 - 12.1 - Cell Format Updates
			//// Moved IsValueDefault to the WorksheetCellFormatData.
			////if (WorksheetCellFormatProxy.IsValueDefault(value, cellBorderValue) && 
			//if (WorksheetCellFormatData.IsValueDefault(value, cellBorderValue) && 
			//    adjacentRow != null &&
			//    // MD 2/25/12 - 12.1 - Table Support
			//    //adjacentRow.HasCellFormatForCellResolved(adjacentColumnIndex, out cellFormat))
			//    adjacentRow.TryGetCellFormat(adjacentColumnIndex, out cellFormat))
			//{
			//    cellBorderValue = cellFormat.GetValue(Utilities.GetOppositeBorderValue(value));
			//}

			//return cellBorderValue;

			#endregion // Old Code
			object cellBorderValue = worksheet.GetCellFormatElementReadOnly(row, columnIndex, value).GetValue(value);

			if (0 <= adjacentColumnIndex && WorksheetCellFormatData.IsValueDefault(value, cellBorderValue))
				cellBorderValue = worksheet.GetCellFormatElementReadOnly(adjacentRow, adjacentColumnIndex, value).GetValue(Utilities.GetOppositeBorderValue(value));

			return cellBorderValue;
		} 

		#endregion // ResolveCellBorderValue

		// MD 5/12/10 - TFS26732
		#region ResolveCellOwnerBorderValue



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		public static object ResolveCellOwnerBorderValue(RowColumnBase cellOwner, RowColumnBase adjacentCellOwner, CellFormatValue value)
		{
			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved GetDefaultValue to the WorksheetCellFormatData.
			//object interiorRowBorderValue = WorksheetCellFormatProxy.GetDefaultValue(value);
			object interiorRowBorderValue = WorksheetCellFormatData.GetDefaultValue(value);

			// MD 12/31/11 - 12.1 - Cell Format Updates
			// This is no longer true. The Style property has a null default value.
			//// MD 10/26/11 - TFS91546
			//// GetDefaultValue may return null for properties which don't have a default value (one that will resolve from owners).
			//if (interiorRowBorderValue == null)
			//{
			//    Utilities.DebugFail("The default value is null here.");
			//    return CellBorderLineStyle.None;
			//}

			if (cellOwner != null && cellOwner.HasCellFormat)
				interiorRowBorderValue = cellOwner.CellFormatInternal.GetValue(value);

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved IsValueDefault to the WorksheetCellFormatData.
			//if (WorksheetCellFormatProxy.IsValueDefault(value, interiorRowBorderValue) && adjacentCellOwner != null && adjacentCellOwner.HasCellFormat)
			if (WorksheetCellFormatData.IsValueDefault(value, interiorRowBorderValue) && adjacentCellOwner != null && adjacentCellOwner.HasCellFormat)
				interiorRowBorderValue = adjacentCellOwner.CellFormatInternal.GetValue(Utilities.GetOppositeBorderValue(value));

			return interiorRowBorderValue;
		} 

		#endregion // ResolveCellOwnerBorderValue

		#region RotateBitsLeft

		public static uint RotateBitsLeft( uint source, int numberOfBits )
		{
			return
				( source >> ( 32 - numberOfBits ) ) |
				( source << numberOfBits );
		}

		#endregion RotateBitsLeft

		// MD 5/16/07 - BR22962
		#region RoundUpToMultiple



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		public static int RoundUpToMultiple( int value, int multipleBase )
		{
			return value + multipleBase - 1 - ( ( value - 1 ) % multipleBase );
		}

		#endregion RoundUpToMultiple

        // MRS 6/18/2008
        // I copied this whole block from Win.Utilities to here in Excel. 
        //
        // SSP 4/2/03
        // Added SortMerge. SortMerge should be faster than quick-sort when the operation
        // of comparing items is expensive. In the case of the grid, when sorting rows,
        // comparing rows is not a simple operation and in such case, SortMerge can end
        // up being faster.
        //
        #region SortMerge

        /// <summary>
        /// Sorts the passed in array list based on the passed in comparer using a modified merge-sort
        /// algorithm. 
        /// </summary>
        /// <param name="arrayList">The list to be sorted.</param>
        /// <param name="comparer">The comparer (must not be null).</param>
        internal static void SortMerge(List<object> arrayList, IComparer comparer)
        {
            if (arrayList == null)
                throw new ArgumentNullException("arrayList");

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            // get the items as an array
            object[] array = arrayList.ToArray();

            // sort the array
            SortMerge(array, null, comparer);

            // clear the array list
            arrayList.Clear();

            // Add the sorted items back into the array list
            arrayList.AddRange(array);
        }

        /// <summary>
        /// Sorts the passed in array based on the passed in comparer using a modified merge-sort
        /// algorithm. It requires allocation of an array equal in size to the array to be sorted.
        /// Merge sort should be used if the operation of comparing items is expensive.
        /// </summary>
        /// <param name="arr">Array to be sorted.</param>
        /// <param name="comparer">Comparer.</param>
        internal static void SortMerge(object[] arr, IComparer comparer)
        {
            SortMerge((object[])arr, null, comparer);
        }

        /// <summary>
        /// Sorts the passed in array based on the passed in comparer using a modified merge-sort
        /// algorithm. Optionally you can pass in a temporary array equal (or greater) in size to arr. 
        /// The method will make use of that array instead of allocating one. If null is passed in, 
        /// then it will allocate one. Merge sort should be used if the operation of comparing items 
        /// is expensive.
        /// </summary>
        /// <param name="arr">Array to be sorted.</param>
        /// <param name="tmpArr">Null or a temporary array equal (or greater) in size to arr.</param>
        /// <param name="comparer">Comparer.</param>
        internal static void SortMerge(object[] arr, object[] tmpArr, IComparer comparer)
        {
            SortMerge(arr, tmpArr, comparer, 0, arr.Length - 1);
        }

        /// <summary>
        /// Sorts the passed in array based on the passed in comparer using a modified merge-sort
        /// algorithm. Optionally you can pass in a temporary array equal (or greater) in size to arr. 
        /// The method will make use of that array instead of allocating one. If null is passed in, 
        /// then it will allocate one. Merge sort should be used if the operation of comparing items 
        /// is expensive.
        /// </summary>
        /// <param name="arr">Array to be sorted.</param>
        /// <param name="tmpArr">Null or a temporary array equal (or greater) in size to arr.</param>
        /// <param name="comparer">Comparer.</param>
        /// <param name="si">Start index in the array.</param>
        /// <param name="ei">End index in the array.</param>
        internal static void SortMerge(object[] arr, object[] tmpArr, IComparer comparer, int si, int ei)
        {
            if (arr == null)
                throw new ArgumentNullException("arr");

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            if (null == tmpArr)
                tmpArr = (object[])arr.Clone();
            else
                Array.Copy(arr, tmpArr, arr.Length);

            // MD 8/7/07 - 7.3 Performance
            // Use generics
            //SortMergeHelper( arr, tmpArr, comparer, si, ei );
            SortMergeHelper(arr, tmpArr, new ComparerWrapper<object>(comparer), si, ei);
        }

        /// <summary>
        /// Sorts the passed in list based on the passed in comparer using a modified merge-sort
        /// algorithm. 
        /// </summary>
        /// <param name="list">The list to be sorted.</param>
        /// <param name="comparer">The comparer (must not be null).</param>
        internal static void SortMergeGeneric<T>(List<T> list, IComparer<T> comparer)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            // get the items as an array
            T[] array = list.ToArray();

            // sort the array
            SortMergeGeneric<T>(array, null, comparer);

            // clear the list
            list.Clear();

            // Add the sorted items back into the list
            list.AddRange(array);
        }

        /// <summary>
        /// Sorts the passed in array based on the passed in comparer using a modified merge-sort
        /// algorithm. It requires allocation of an array equal in size to the array to be sorted.
        /// Merge sort should be used if the operation of comparing items is expensive.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="comparer"></param>
        internal static void SortMergeGeneric<T>(T[] arr, IComparer<T> comparer)
        {
            SortMergeGeneric<T>(arr, null, comparer);
        }

        /// <summary>
        /// Sorts the passed in array based on the passed in comparer using a modified merge-sort
        /// algorithm. Optionally you can pass in a temporary array equal (or greater) in size to arr. 
        /// The method will make use of that array instead of allocating one. If null is passed in, 
        /// then it will allocate one. Merge sort should be used if the operation of comparing items 
        /// is expensive.
        /// </summary>
        /// <param name="arr">Array to be sorted.</param>
        /// <param name="tmpArr">Null or a temporary array equal (or greater) in size to arr.</param>
        /// <param name="comparer">Comparer.</param>
        internal static void SortMergeGeneric<T>(T[] arr, T[] tmpArr, IComparer<T> comparer)
        {
            SortMergeGeneric<T>(arr, tmpArr, comparer, 0, arr.Length - 1);
        }

        /// <summary>
        /// Sorts the passed in array based on the passed in comparer using a modified merge-sort
        /// algorithm. Optionally you can pass in a temporary array equal (or greater) in size to arr. 
        /// The method will make use of that array instead of allocating one. If null is passed in, 
        /// then it will allocate one. Merge sort should be used if the operation of comparing items 
        /// is expensive.
        /// </summary>
        /// <param name="arr">Array to be sorted.</param>
        /// <param name="tmpArr">Null or a temporary array equal (or greater) in size to arr.</param>
        /// <param name="comparer">Comparer.</param>
        /// <param name="si">Start index in the array.</param>
        /// <param name="ei">End index in the array.</param>
        internal static void SortMergeGeneric<T>(T[] arr, T[] tmpArr, IComparer<T> comparer, int si, int ei)
        {
            if (arr == null)
                throw new ArgumentNullException("arr");

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            if (null == tmpArr)
                tmpArr = (T[])arr.Clone();
            else
                Array.Copy(arr, tmpArr, arr.Length);

            SortMergeHelper<T>(arr, tmpArr, comparer, si, ei);
        }






        // MD 8/7/07 - 7.3 Performance
        // Use generics
        //private static void SortMergeHelper( object[] arr, object[] tmpArr, IComparer comparer, int si, int ei )
        private static void SortMergeHelper<T>(T[] arr, T[] tmpArr, IComparer<T> comparer, int si, int ei)
        {
            int i, j, k, m;

            // MD 8/7/07 - 7.3 Performance
            // Use generics
            //object o1 = null, o2 = null;
            T o1 = default(T), o2 = default(T);

            if (ei - si < 6)
            {
                for (i = 1 + si; i <= ei; i++)
                {
                    o1 = arr[i];

                    for (j = i; j > si; j--)
                    {
                        o2 = arr[j - 1];

                        if (comparer.Compare(o1, o2) < 0)
                            arr[j] = o2;
                        else
                            break;
                    }

                    if (i != j)
                        arr[j] = o1;
                }
                return;
            }

            m = (si + ei) / 2;
            SortMergeHelper(tmpArr, arr, comparer, si, m);
            SortMergeHelper(tmpArr, arr, comparer, 1 + m, ei);

            for (i = si, j = 1 + m, k = si; k <= ei; k++)
            {
                if (i <= m)
                    o1 = tmpArr[i];
                if (j <= ei)
                    o2 = tmpArr[j];

                if (j > ei || i <= m && comparer.Compare(o1, o2) <= 0)
                {
                    arr[k] = o1;
                    i++;
                }
                else
                {
                    arr[k] = o2;
                    j++;
                }
            }
        }

        private class ComparerWrapper<T> : IComparer<T>
        {
            private IComparer comparer;

            public ComparerWrapper(IComparer comparer)
            {
                this.comparer = comparer;
            }

            public int Compare(T x, T y)
            {
                return this.comparer.Compare(x, y);
            }
        }

        #endregion // SortMerge

        #region SwapValues

		// MD 11/13/09 - TFS24818
		// Changed the visibility to public.
		//private static void SwapValues<T>(ref T value1, ref T value2)
		public static void SwapValues<T>( ref T value1, ref T value2 )
        {
            T temp = value1;
            value1 = value2;
            value2 = temp;
        }

        #endregion SwapValues

		#region TryEncodeRKValue

		public static bool TryEncodeRKValue( double value, out uint rkValue )
		{
			rkValue = 0;
			bool valueIsValid = false;
			bool valueIsMultipliedBy100 = false;

			for ( int pass = 0; pass < 2; pass++ )
			{
				byte[] data = BitConverter.GetBytes( value );

				// If the low 34 bits of the number are 0, it is a type 0 or 1 RK value
				if ( data[ 0 ] == 0x00 &&
					data[ 1 ] == 0x00 &&
					data[ 2 ] == 0x00 &&
					data[ 3 ] == 0x00 &&
					( data[ 4 ] & 0x03 ) == 0x00 )
				{
					rkValue = BitConverter.ToUInt32( data, 4 );
					valueIsValid = true;
					break;
				}

				// If the number can be represented by a 30-bit integer, it is a type 2 or 3 RK value
				if ( value % 1 == 0 &&
					-536870912 <= value && value <= 536870911 )
				{
					rkValue = (uint)( (int)value << 2 );
					rkValue |= RKValueIsSignedInt;
					valueIsValid = true;
					break;
				}

				value *= 100;
				valueIsMultipliedBy100 = true;
			}

			if ( valueIsValid )
			{
				if ( valueIsMultipliedBy100 )
					rkValue |= RKValueIsMultipliedBy100;

				return true;
			}

			return false;
		}

		#endregion TryEncodeRKValue

		#region TwipsToEMU
        public static int TwipsToEMU(double twips)
        {
            return ((int)twips * Utilities.EMUsPerPoint) / Utilities.TwipsPerPoint;
        }

		public static int TwipsToEMU( int twips )
		{
			return ( twips * Utilities.EMUsPerPoint ) / Utilities.TwipsPerPoint;
		}

		#endregion TwipsToEMU

		#region VerifyColumnCount

		// MD 6/31/08 - Excel 2007 Format
		//public static void VerifyColumnCount( int count, string paramName )
		// MD 2/24/12 - 12.1 - Table Support
		//public static void VerifyColumnCount( Workbook workbook, int count, string paramName )
		public static void VerifyColumnCount(Worksheet worksheet, int count, string paramName)
		{
			// MD 6/31/08 - Excel 2007 Format
			//if ( 0 <= count && count <= Workbook.MaxExcelColumnCount )
			// MD 2/24/12 - 12.1 - Table Support
			//int maxColumnCount = workbook.MaxRowCount;
			int maxColumnCount = worksheet.Columns.MaxCount;

			if ( 0 <= count && count <= maxColumnCount )
				return;

			throw new ArgumentOutOfRangeException(
				paramName,
				count,
				// MD 6/31/08 - Excel 2007 Format
				//SR.GetString( "LE_ArgumentOutOfRangeException_InvalidColumnCount", count, 0, Workbook.MaxExcelColumnCount ) );
				SR.GetString( "LE_ArgumentOutOfRangeException_InvalidColumnCount", count, 0, maxColumnCount ) );
		}

		#endregion VerifyColumnCount

		#region VerifyColumnIndex

		// MD 6/31/08 - Excel 2007 Format
		//public static void VerifyColumnIndex( int index, string paramName )
		// MD 2/24/12 - 12.1 - Table Support
		//public static void VerifyColumnIndex( Workbook workbook, int index, string paramName )
		public static void VerifyColumnIndex(Worksheet worksheet, int index, string paramName)
		{
			// MD 6/31/08 - Excel 2007 Format
			//if ( 0 <= index && index <= Workbook.MaxExcelColumnCount - 1 )
			//int maxColumnCount = workbook.MaxRowCount;
			// MD 2/24/12 - 12.1 - Table Support
			int maxColumnCount = worksheet.Columns.MaxCount;

			if ( 0 <= index && index <= maxColumnCount - 1 )
				return;

			throw new ArgumentOutOfRangeException(
				paramName,
				index,
				// MD 6/31/08 - Excel 2007 Format
				//SR.GetString( "LE_ArgumentOutOfRangeException_InvalidColumnIndex", index, 0, Workbook.MaxExcelColumnCount - 1 ) );
				SR.GetString( "LE_ArgumentOutOfRangeException_InvalidColumnIndex", index, 0, maxColumnCount - 1 ) );
		}

		// MD 2/24/12 - 12.1 - Table Support
		//// MD 4/12/11 - TFS67084
		//public static void VerifyColumnIndex(Worksheet worksheet, int index, string paramName)
		//{
		//    if (0 <= index)
		//    {
		//        // For performance, check the lower of the column count values. If the index is under that, it is fine no matter what 
		//        // format we are using. It is relatively slower to get the CurrentFormat from the worksheet
		//        if (index <= Workbook.MaxExcelColumnCount - 1)
		//            return;

		//        int maxColumnCount = Workbook.GetMaxColumnCount(worksheet.CurrentFormat);

		//        if (index <= maxColumnCount - 1)
		//            return;
		//    }

		//    throw new ArgumentOutOfRangeException(
		//        paramName,
		//        index,
		//        SR.GetString("LE_ArgumentOutOfRangeException_InvalidColumnIndex", index, 0, Workbook.GetMaxColumnCount(worksheet.CurrentFormat) - 1));
		//}

		#endregion VerifyColumnIndex

		#region VerifyRowCount

		// MD 6/31/08 - Excel 2007 Format
		//public static void VerifyRowCount( int count, string paramName )
		// MD 2/24/12 - 12.1 - Table Support
		//public static void VerifyRowCount( Workbook workbook, int count, string paramName )
		public static void VerifyRowCount(Worksheet worksheet, int count, string paramName)
		{
			// MD 6/31/08 - Excel 2007 Format
			//if ( 0 <= count && count <= Workbook.MaxExcelRowCount )
			//int maxRowCount = workbook.MaxRowCount;
			int maxRowCount = worksheet.Rows.MaxCount;

			if ( 0 <= count && count <= maxRowCount )
				return;

			throw new ArgumentOutOfRangeException(
				paramName,
				count,
				// MD 6/31/08 - Excel 2007 Format
				//SR.GetString( "LE_ArgumentOutOfRangeException_InvalidRowCount", count, 0, Workbook.MaxExcelRowCount ) );
				SR.GetString( "LE_ArgumentOutOfRangeException_InvalidRowCount", count, 0, maxRowCount ) );
		}

		#endregion VerifyRowCount

		#region VerifyRowIndex

		// MD 6/31/08 - Excel 2007 Format
		//public static void VerifyRowIndex( int index, string paramName )
		// MD 2/24/12 - 12.1 - Table Support
		//public static void VerifyRowIndex( Workbook workbook, int index, string paramName )
		public static void VerifyRowIndex(Worksheet worksheet, int index, string paramName)
		{
			// MD 6/31/08 - Excel 2007 Format
			//if ( 0 <= index && index <= Workbook.MaxExcelRowCount - 1 )
			// MD 2/24/12 - 12.1 - Table Support
			//int maxRowCount = workbook.MaxRowCount;
			int maxRowCount = worksheet.Rows.MaxCount;

			if ( 0 <= index && index <= maxRowCount - 1 )
				return;

			throw new ArgumentOutOfRangeException(
				paramName,
				index,
				// MD 6/31/08 - Excel 2007 Format
				//SR.GetString( "LE_ArgumentOutOfRangeException_InvalidRowIndex", index, 0, Workbook.MaxExcelRowCount - 1 ) );
				SR.GetString( "LE_ArgumentOutOfRangeException_InvalidRowIndex", index, 0, maxRowCount - 1 ) );
		}

		#endregion VerifyRowIndex

		#region WriteEMURect

		public static void WriteEMURect( BiffRecordStream stream, Rectangle twipRect )
		{
            stream.Write(Utilities.TwipsToEMU((int)twipRect.Left));
            stream.Write(Utilities.TwipsToEMU((int)twipRect.Top));
            stream.Write(Utilities.TwipsToEMU((int)twipRect.Right));
            stream.Write(Utilities.TwipsToEMU((int)twipRect.Bottom));
		}

		#endregion WriteEMURect

        #region ToInteger





        internal static int ToInteger( object value )
        {
            if ( value is uint )
            {
                uint uintValue = (uint)value;
                return (int)uintValue;
            }
            else
            if ( value is short )
            {
                short shortValue = (short)value;
                return (int)shortValue;
            }
            else
            if ( value is ushort )
            {
                ushort ushortValue = (ushort)value;
                return (int)ushortValue;
            }
            else
            if ( value is int )
                return (int)value;
            else
            if ( value is Color)


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                return ((Color)value).ToArgb();

            else
            {
                Debug.Assert( false, string.Format("The Utilities.ToInteger method does not support converting the value '{0}' to an integer.", value ) );
                return 0;
            }
        }
        #endregion ToInteger

        #region ToDateTime
//#if DEBUG
//        /// <summary>
//        /// Supports conversion of boxed DateTime.
//        /// </summary>
//#endif
//        internal static int ToDateTime( object value )
//        {
//            if ( value is DateTime )
//                return (DateTime)value;
//            else
//            {
//                Debug.Assert( false, string.Format("The Utilities.ToDateTime method does not support converting the value '{0}' to a DateTime.", value ) );
//                return 0;
//            }
//        }
        #endregion ToDateTime
        //  BF 8/6/08   Excel2007 Format
        #region CustomViewWindowOptions/WorkbookWindowOptions/WindowOptions helpers

            #region SetWorkbookWindowOptionsProperties
        internal static void SetWorkbookWindowOptionsProperties(
            bool minimized,
            bool showHScroll,
            bool showVScroll,
            bool showSheetTabs,
            int xWindowTwips,
            int yWindowTwips,
            int windowWidthTwips,
            int windowHeightTwips,
            int tabRatio,
            int firstSheet,
            int activeTab,
            WorkbookWindowOptions workbookWindowOptions )
        {
            workbookWindowOptions.Minimized = minimized;
            workbookWindowOptions.BoundsInTwips = new Rectangle( xWindowTwips, yWindowTwips, windowWidthTwips, windowHeightTwips );
            workbookWindowOptions.FirstVisibleTabIndex = firstSheet;
            workbookWindowOptions.SelectedWorksheetIndex = activeTab;

            SetWindowOptionsProperties(
                showHScroll,
                showVScroll,
                showSheetTabs,
                tabRatio,
                workbookWindowOptions);
        }
            #endregion SetWorkbookWindowOptionsProperties

            #region SetCustomViewWindowOptionsProperties
        internal static void SetCustomViewWindowOptionsProperties(
            bool maximized,
            bool showFormulaBar,
            bool showStatusBar,
            bool showHScroll,
            bool showVScroll,
            bool showSheetTabs,
            int tabRatio,
            int activeSheetId,
            int xWindowPixels,
            int yWindowPixels,
            int windowWidthPixels,
            int windowHeightPixels,
            ObjectDisplayStyle objectDisplayStyle,
            CustomViewWindowOptions customViewWindowOptions )
        {
            customViewWindowOptions.Maximized = maximized;
            customViewWindowOptions.BoundsInPixels = new Rectangle( xWindowPixels, yWindowPixels, windowWidthPixels, windowHeightPixels );
            customViewWindowOptions.SelectedWorksheetTabId = activeSheetId;
            customViewWindowOptions.ShowFormulaBar = showFormulaBar;
            customViewWindowOptions.ShowStatusBar = showStatusBar;
            customViewWindowOptions.ObjectDisplayStyle = objectDisplayStyle;

            SetWindowOptionsProperties(
                showHScroll,
                showVScroll,
                showSheetTabs,
                tabRatio,
                customViewWindowOptions);
        }
            #endregion SetCustomViewWindowOptionsProperties

            #region SetWindowOptionsProperties
        internal static void SetWindowOptionsProperties(
            bool showHScroll,
            bool showVScroll,
            bool showSheetTabs,
            int tabRatio,
            WindowOptions windowOptions )
        {
            windowOptions.ScrollBars =
                showHScroll && showVScroll ?
                ScrollBars.Both :
                showHScroll ?
                ScrollBars.Horizontal :
                showVScroll ?
                ScrollBars.Vertical :
                ScrollBars.None;
           
            windowOptions.TabBarWidth = tabRatio;
            windowOptions.TabBarVisible = showSheetTabs;
        }
            #endregion SetWindowOptionsProperties
        
        #endregion CustomViewWindowOptions/WorkbookWindowOptions/WindowOptions helpers

        //  BF 8/6/08   Excel2007 Format
        #region GetWorksheet
        /// <summary>
        /// Gets a Worksheet off the specified ContextStack by popping
        /// the ChildDataItem off the stack, then returning the value of the Data
        /// property as type Worksheet.
        /// </summary>
        public static Worksheet GetWorksheet( ContextStack contextStack )
        {
            if ( contextStack == null )
            {
                Utilities.DebugFail("Specified ContextStack is null.");
                return null;
            }

            ChildDataItem item = contextStack[typeof(ChildDataItem)] as ChildDataItem;
            if (item == null)
            {
                Utilities.DebugFail("Could not get the ChildDataItem from the ContextStack");
                return null;
            }

            Worksheet worksheet = item.Data as Worksheet;
            if (worksheet == null)
                Utilities.DebugFail("Could not get the worksheet from the ContextStack");

            return worksheet;
        }
        #endregion GetWorksheet

        // CDS 8/12/08 Excel2007 Format
        #region ConvertST_SystemColorValToSystemColor

        /// <summary>
        /// Converts the provided ST_SystemColorVal to a Color using SystemColors
        /// </summary>
        /// <param name="value">The System Color Value Simple-Type value</param>
        /// <returns>Returns a Color based on System.Drawing.SystemColors</returns>
        public static Color ConvertST_SystemColorValToSystemColor(ST_SystemColorVal value)
        {
            switch (value)
            {
                case ST_SystemColorVal.activeBorder:            return SystemColorsInternal.ActiveBorderColor;
                case ST_SystemColorVal.activeCaption:           return SystemColorsInternal.ActiveCaptionColor;
                case ST_SystemColorVal.appWorkspace:            return SystemColorsInternal.AppWorkspaceColor;
                case ST_SystemColorVal.background:              return SystemColorsInternal.DesktopColor;
                case ST_SystemColorVal.btnFace:                 return SystemColorsInternal.ButtonFaceColor;
                case ST_SystemColorVal.btnHighlight:            return SystemColorsInternal.ButtonHighlightColor;
                case ST_SystemColorVal.btnText:                 return SystemColorsInternal.ControlTextColor;
                case ST_SystemColorVal.captionText:             return SystemColorsInternal.ActiveCaptionTextColor;
                case ST_SystemColorVal._3dDarkShadow:           return SystemColorsInternal.ControlDarkColor;
                case ST_SystemColorVal.gradientActiveCaption:   return SystemColorsInternal.GradientActiveCaptionColor;
                case ST_SystemColorVal.gradientInactiveCaption: return SystemColorsInternal.GradientInactiveCaptionColor;
                case ST_SystemColorVal.grayText:                return SystemColorsInternal.GrayTextColor;
                case ST_SystemColorVal.highlight:               return SystemColorsInternal.HighlightColor;
                case ST_SystemColorVal.highlightText:           return SystemColorsInternal.HighlightTextColor;
                case ST_SystemColorVal.hotLight:                return SystemColorsInternal.HotTrackColor;
                case ST_SystemColorVal.inactiveBorder:          return SystemColorsInternal.InactiveBorderColor;
                case ST_SystemColorVal.inactiveCaptionText:     return SystemColorsInternal.InactiveCaptionTextColor;
                case ST_SystemColorVal.infoBk:                  return SystemColorsInternal.InfoColor;
                case ST_SystemColorVal._3dLight:                return SystemColorsInternal.ControlLightColor;
                case ST_SystemColorVal.menu:                    return SystemColorsInternal.MenuColor;
                case ST_SystemColorVal.menuBar:                 return SystemColorsInternal.MenuBarColor;
                case ST_SystemColorVal.menuHighlight:           return SystemColorsInternal.MenuHighlightColor;
                case ST_SystemColorVal.menuText:                return SystemColorsInternal.MenuTextColor;
                case ST_SystemColorVal.scrollBar:               return SystemColorsInternal.ScrollBarColor;
                case ST_SystemColorVal.window:                  return SystemColorsInternal.WindowColor;
                case ST_SystemColorVal.windowFrame:             return SystemColorsInternal.WindowFrameColor;
                case ST_SystemColorVal.windowText:              return SystemColorsInternal.WindowTextColor;
            }

            return Utilities.ColorEmpty;
        }

		// MD 6/21/11 - TFS79214
		// Refactored the SystemColorsInternal class
		#region Refactored

		//        internal class SystemColorsInternal
		//        {            
		//            private static Color activeCaptionColor = Color.FromArgb(0xFF, 0x99, 0xB4, 0xD1);
		//            public static Color ActiveCaptionColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.ActiveCaptionColor;
		//                    return activeCaptionColor;                    
		//#else
		//                return System.Drawing.SystemColors.ActiveCaption;
		//#endif
		//                }
		//            }

		//            private static Color appWorkspaceColor = Color.FromArgb(0xFF, 0xAB, 0xAB, 0xAB);
		//            public static Color AppWorkspaceColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.AppWorkspaceColor;
		//                    return appWorkspaceColor;
		//#else
		//                return System.Drawing.SystemColors.AppWorkspace;
		//#endif
		//                }
		//            }

		//            private static Color controlColor = Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0);
		//            public static Color ControlColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.AppWorkspaceColor;
		//                    return controlColor;
		//#else
		//                return System.Drawing.SystemColors.Control;
		//#endif
		//                }
		//            }

		//            private static Color desktopColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
		//            public static Color DesktopColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.DesktopColor;
		//                    return desktopColor;
		//#else
		//                return System.Drawing.SystemColors.Desktop;
		//#endif
		//                }
		//            }

		//            public static Color ButtonFaceColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    throw new NotImplementedException();
		//#else
		//                return System.Drawing.SystemColors.ButtonFace;
		//#endif
		//                }
		//            }

		//            public static Color ButtonHighlightColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    throw new NotImplementedException();
		//#else
		//                return System.Drawing.SystemColors.ButtonHighlight;
		//#endif
		//                }
		//            }

		//            private static Color controlTextColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
		//            public static Color ControlTextColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.ControlTextColor;
		//                    return controlTextColor;
		//#else
		//                return System.Drawing.SystemColors.ControlText;
		//#endif
		//                }
		//            }

		//            private static Color controlLightLightColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
		//            public static Color ControlLightLightColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.ControlTextColor;
		//                    return controlLightLightColor;
		//#else
		//                return System.Drawing.SystemColors.ControlLightLight;
		//#endif
		//                }
		//            }

		//            private static Color activeCaptionTextColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
		//            public static Color ActiveCaptionTextColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.ActiveCaptionTextColor;
		//                    return activeCaptionTextColor;
		//#else
		//                return System.Drawing.SystemColors.ActiveCaptionText;
		//#endif
		//                }
		//            }

		//            private static Color controlDarkColor = Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
		//            public static Color ControlDarkColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.ControlDarkColor;
		//                    return controlDarkColor;
		//#else
		//                return System.Drawing.SystemColors.ControlDark;
		//#endif
		//                }
		//            }

		//            public static Color GradientActiveCaptionColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    throw new NotImplementedException();
		//#else
		//                return System.Drawing.SystemColors.GradientActiveCaption;
		//#endif
		//                }
		//            }

		//            public static Color GradientInactiveCaptionColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    throw new NotImplementedException();
		//#else
		//                return System.Drawing.SystemColors.GradientInactiveCaption;
		//#endif
		//                }
		//            }

		//            private static Color grayTextColor = Color.FromArgb(0xFF, 0x6D, 0x6D, 0x6D);
		//            public static Color GrayTextColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.GrayTextColor;
		//                    return grayTextColor;
		//#else
		//                return System.Drawing.SystemColors.GrayText;
		//#endif
		//                }
		//            }

		//            private static Color highlightColor = Color.FromArgb(0xFF, 0x33, 0x99, 0xFF);
		//            public static Color HighlightColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.HighlightColor;
		//                    return highlightColor;
		//#else
		//                return System.Drawing.SystemColors.Highlight;
		//#endif
		//                }
		//            }

		//            private static Color highlightTextColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
		//            public static Color HighlightTextColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.HighlightTextColor;
		//                    return highlightTextColor;
		//#else
		//                return System.Drawing.SystemColors.HighlightText;
		//#endif
		//                }
		//            }

		//            public static Color HotTrackColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    throw new NotImplementedException();
		//#else
		//                return System.Drawing.SystemColors.HotTrack;
		//#endif
		//                }
		//            }

		//            private static Color inactiveBorderColor = Color.FromArgb(0xFF, 0xF4, 0xF7, 0xFC);
		//            public static Color InactiveBorderColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.InactiveBorderColor;
		//                    return inactiveBorderColor;
		//#else
		//                return System.Drawing.SystemColors.InactiveBorder;
		//#endif
		//                }
		//            }

		//            private static Color inactiveCaptionTextColor = Color.FromArgb(0xFF, 0x43, 0x4E, 0x54);
		//            public static Color InactiveCaptionTextColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.InactiveCaptionTextColor;
		//                    return inactiveCaptionTextColor;
		//#else
		//                return System.Drawing.SystemColors.InactiveCaptionText;
		//#endif
		//                }
		//            }

		//            private static Color infoColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xE1);
		//            public static Color InfoColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.InfoColor;
		//                    return infoColor;
		//#else
		//                return System.Drawing.SystemColors.Info;
		//#endif
		//                }
		//            }

		//            private static Color infoTextColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
		//            public static Color InfoTextColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.InfoColor;
		//                    return infoTextColor;
		//#else
		//                return System.Drawing.SystemColors.Info;
		//#endif
		//                }
		//            }

		//            private static Color controlLightColor = Color.FromArgb(0xFF, 0xE3, 0xE3, 0xE3);
		//            public static Color ControlLightColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.ControlLightColor;
		//                    return controlLightColor;
		//#else
		//                return System.Drawing.SystemColors.ControlLight;
		//#endif
		//                }
		//            }

		//            private static Color menuColor = Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0);
		//            public static Color MenuColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.MenuColor;
		//                    return menuColor;
		//#else
		//                return System.Drawing.SystemColors.Menu;
		//#endif
		//                }
		//            }

		//            public static Color MenuBarColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    throw new NotImplementedException();
		//#else
		//                return System.Drawing.SystemColors.MenuBar;
		//#endif
		//                }
		//            }

		//            public static Color MenuHighlightColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    throw new NotImplementedException();
		//#else
		//                return System.Drawing.SystemColors.MenuHighlight;
		//#endif
		//                }
		//            }

		//            private static Color menuTextColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
		//            public static Color MenuTextColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.MenuTextColor;
		//                    return menuTextColor;
		//#else
		//                return System.Drawing.SystemColors.MenuText;
		//#endif
		//                }
		//            }

		//            private static Color scrollBarColor = Color.FromArgb(0xFF, 0xC8, 0xC8, 0xC8);
		//            public static Color ScrollBarColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.ScrollBarColor;
		//                    return scrollBarColor;
		//#else
		//                return System.Drawing.SystemColors.ScrollBar;
		//#endif
		//                }
		//            }

		//            private static Color windowColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
		//            public static Color WindowColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.WindowColor;
		//                    return windowColor;
		//#else
		//                return System.Drawing.SystemColors.Window;
		//#endif
		//                }
		//            }

		//            private static Color windowFrameColor = Color.FromArgb(0xFF, 0x64, 0x64, 0x64);
		//            public static Color WindowFrameColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.WindowFrameColor;
		//                    return windowFrameColor;
		//#else
		//                return System.Drawing.SystemColors.WindowFrame;
		//#endif
		//                }
		//            }

		//            private static Color windowTextColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
		//            public static Color WindowTextColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.WindowTextColor;
		//                    return windowTextColor;
		//#else
		//                return System.Drawing.SystemColors.WindowText;
		//#endif
		//                }
		//            }

		//            private static Color activeBorderColor = Color.FromArgb(0xFF, 0xB4, 0xB4, 0xB4);
		//            public static Color ActiveBorderColor
		//            {
		//                get
		//                {
		//#if SILVERLIGHT
		//                    //return SystemColors.ActiveBorderColor;
		//                    return activeBorderColor;
		//#else
		//                return System.Drawing.SystemColors.ActiveBorder;
		//#endif
		//                }
		//            }
		//        } 

		#endregion  // Refactored
		internal static class SystemColorsInternal
		{
			#region Silverlight Specific
      


#region Infragistics Source Cleanup (Region)


































































#endregion // Infragistics Source Cleanup (Region)

    
			#endregion  // Silverlight Specific

			#region Methods

			#region GetColor

			private static Color GetColor(SystemColorValue color)
			{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				switch(color)
				{
					case SystemColorValue.ActiveBorder: return SystemColors.ActiveBorder;
					case SystemColorValue.ActiveCaption: return SystemColors.ActiveCaption;
					case SystemColorValue.ActiveCaptionText: return SystemColors.ActiveCaptionText;
					case SystemColorValue.AppWorkspace: return SystemColors.AppWorkspace;
					case SystemColorValue.ButtonFace: return SystemColors.ButtonFace;
					case SystemColorValue.ButtonHighlight: return SystemColors.ButtonHighlight;
					case SystemColorValue.ButtonShadow: return SystemColors.ButtonShadow;	// MD 7/2/12 - TFS115692
					case SystemColorValue.Control: return SystemColors.Control;
					case SystemColorValue.ControlDark: return SystemColors.ControlDark;
					case SystemColorValue.ControlDarkDark: return SystemColors.ControlDarkDark;
					case SystemColorValue.ControlLight: return SystemColors.ControlLight;
					case SystemColorValue.ControlLightLight: return SystemColors.ControlLightLight;
					case SystemColorValue.ControlText: return SystemColors.ControlText;
					case SystemColorValue.Desktop: return SystemColors.Desktop;
					case SystemColorValue.GradientActiveCaption: return SystemColors.GradientActiveCaption;
					case SystemColorValue.GradientInactiveCaption: return SystemColors.GradientInactiveCaption;
					case SystemColorValue.GrayText: return SystemColors.GrayText;
					case SystemColorValue.Highlight: return SystemColors.Highlight;
					case SystemColorValue.HighlightText: return SystemColors.HighlightText;
					case SystemColorValue.HotTrack: return SystemColors.HotTrack;
					case SystemColorValue.InactiveBorder: return SystemColors.InactiveBorder;
					case SystemColorValue.InactiveCaption: return SystemColors.InactiveCaption;
					case SystemColorValue.InactiveCaptionText: return SystemColors.InactiveCaptionText;
					case SystemColorValue.Info: return SystemColors.Info;
					case SystemColorValue.InfoText: return SystemColors.InfoText;
					case SystemColorValue.Menu: return SystemColors.Menu;
					case SystemColorValue.MenuBar: return SystemColors.MenuBar;
					case SystemColorValue.MenuHighlight: return SystemColors.MenuHighlight;
					case SystemColorValue.MenuText: return SystemColors.MenuText;
					case SystemColorValue.ScrollBar: return SystemColors.ScrollBar;
					case SystemColorValue.Window: return SystemColors.Window;
					case SystemColorValue.WindowFrame: return SystemColors.WindowFrame;
					case SystemColorValue.WindowText: return SystemColors.WindowText;

					default:
						Utilities.DebugFail("Unknown SystemColorValue: " + color);
						return Color.Empty;
				}

			}

			#endregion  // GetColor

			#region IsDefined

			public static bool IsDefined(Color color)
			{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				return color.IsSystemColor;

			}

			#endregion  // IsDefined

			#endregion  // Methods

			#region Properties

			public static Color ActiveBorderColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ActiveBorder); }
			}

			public static Color ActiveCaptionColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ActiveCaption); }
			}

			public static Color ActiveCaptionTextColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ActiveCaptionText); }
			}

			public static Color AppWorkspaceColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.AppWorkspace); }
			}

			public static Color ControlColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.Control); }
			}

			public static Color ButtonFaceColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ButtonFace); }
			}

			public static Color ButtonHighlightColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ButtonHighlight); }
			}

			// MD 7/2/12 - TFS115692
			public static Color ButtonShadowColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ButtonShadow); }
			}

			public static Color ControlLightColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ControlLight); }
			}

			public static Color ControlTextColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ControlText); }
			}

			public static Color ControlLightLightColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ControlLightLight); }
			}

			public static Color ControlDarkColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ControlDark); }
			}

			public static Color DesktopColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.Desktop); }
			}

			public static Color GradientActiveCaptionColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.GradientActiveCaption); }
			}

			public static Color GradientInactiveCaptionColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.GradientInactiveCaption); }
			}

			public static Color GrayTextColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.GrayText); }
			}

			public static Color HighlightColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.Highlight); }
			}

			public static Color HighlightTextColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.HighlightText); }
			}

			public static Color HotTrackColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.HotTrack); }
			}

			public static Color InactiveBorderColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.InactiveBorder); }
			}

			// MD 7/2/12 - TFS115692
			public static Color InactiveCaptionColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.InactiveCaption); }
			}

			public static Color InactiveCaptionTextColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.InactiveCaptionText); }
			}

			public static Color InfoColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.Info); }
			}

			public static Color InfoTextColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.InfoText); }
			}

			public static Color MenuColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.Menu); }
			}

			public static Color MenuBarColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.MenuBar); }
			}

			public static Color MenuHighlightColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.MenuHighlight); }
			}

			public static Color MenuTextColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.MenuText); }
			}

			public static Color ScrollBarColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.ScrollBar); }
			}

			public static Color WindowColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.Window); }
			}

			public static Color WindowFrameColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.WindowFrame); }
			}

			public static Color WindowTextColor
			{
				get { return SystemColorsInternal.GetColor(SystemColorValue.WindowText); }
			} 

			#endregion  // Properties


			#region SystemColorValue enum

			private enum SystemColorValue
			{
				ActiveBorder,
				ActiveCaption,
				ActiveCaptionText,
				AppWorkspace,
				ButtonFace,
				ButtonHighlight,
				ButtonShadow,	// MD 7/2/12 - TFS115692
				Control,
				ControlDark,
				ControlDarkDark,
				ControlLight,
				ControlLightLight,
				ControlText,
				Desktop,
				GradientActiveCaption,
				GradientInactiveCaption,
				GrayText,
				Highlight,
				HighlightText,
				HotTrack,
				InactiveBorder,
				InactiveCaption,
				InactiveCaptionText,
				Info,
				InfoText,
				Menu,
				MenuBar,
				MenuHighlight,
				MenuText,
				ScrollBar,
				Window,
				WindowFrame,
				WindowText
			}

			#endregion  // SystemColorValue enum
		}

        internal class ColorsInternal
        {
            public static Color Black
            {
                get
                {



                    return Color.Black;

                }
            }
        }

        #endregion ConvertST_SystemColorValToSystemColor

        //  BF 8/14/08  Excel2007 Format
        #region FromUnsignedIntHex / ToUnsignedIntHex

        /// <summary>
        /// Returns an Int32 value from the specified string, implied
        /// to be conformant with the pattern expected for the
        /// ST_UnsignedIntHex data type.
        /// </summary>
        public static int FromUnsignedIntHex( string value )
        {
            if ( string.IsNullOrEmpty(value) ||
                 System.Text.RegularExpressions.Regex.IsMatch(value, "[0-9A-F]{8}") == false )
            {
                Utilities.DebugFail( "The specified string is empty, or does not conform to the pattern expected for the ST_UnsignedIntHex data type ('[0-9A-F]{8}')." );
                return int.MinValue;
            }

            int retVal = int.MinValue;
            if ( int.TryParse(value, NumberStyles.HexNumber, null, out retVal) == false )
                Utilities.DebugFail( "Could not parse ST_UnsignedIntHex string to an integer." );

            return retVal;
        }

        /// <summary>
        /// Returns a string that is compatible with the ST_UnsignedIntHex
        /// data type from the specified integer value.
        /// </summary>
        public static string ToUnsignedIntHex( int value )
        {
            return value.ToString( "X8" );
        }

        /// <summary>
        /// Returns an Int32 value from the specified string, implied
        /// to be conformant with the pattern expected for the
        /// ST_HexBinary3 data type.
        /// </summary>
        public static int FromHexBinary3( string value )
        {
            if ( string.IsNullOrEmpty(value) ||
                 System.Text.RegularExpressions.Regex.IsMatch(value, "[0-9A-F]{6}") == false )
            {
                Utilities.DebugFail( "The specified string is empty, or does not conform to the pattern expected for the ST_HexBinary3 data type ('[0-9A-F]{6}')." );
                return int.MinValue;
            }

            int retVal = int.MinValue;
            if ( int.TryParse(value, NumberStyles.HexNumber, null, out retVal) == false )
                Utilities.DebugFail( "Could not parse ST_HexBinary3 string to an integer." );

            return retVal;
        }

        /// <summary>
        /// Returns a string that is compatible with the ST_HexBinary3
        /// data type from the specified color value.
        /// </summary>
        public static string ToHexBinary3(Color value)
        {
            Color color = Color.FromArgb(0, value.R, value.G, value.B);
            int colorValue = Utilities.ColorToArgb(color);
            return colorValue.ToString("X6");
        }

        #endregion FromUnsignedIntHex / ToUnsignedIntHex

        #region FromTextUnderlineType / ToTextUnderlineType

        public static FontUnderlineStyle FromTextUnderlineType( string value )
        {
            FontUnderlineStyle retVal = FontUnderlineStyle.Default;

            if ( string.IsNullOrEmpty(value) )
                return retVal;

            switch ( value )
            {
                case "dbl":
                    retVal = FontUnderlineStyle.Double;
                    break;

                case "none":
                    retVal = FontUnderlineStyle.None;
                    break;

                default:
                    retVal = FontUnderlineStyle.Single;
                    break;

            }

            return retVal;
        }

        public static string ToTextUnderlineType( FontUnderlineStyle value )
        {
            string retVal = string.Empty;

            switch ( value )
            {
                case FontUnderlineStyle.None:
                    retVal = "none";
                    break;                    

                case FontUnderlineStyle.Single:
                case FontUnderlineStyle.SingleAccounting:
                    retVal = "sng";
                    break;                    

                case FontUnderlineStyle.Double:
                case FontUnderlineStyle.DoubleAccounting:
                    retVal = "dbl";
                    break;
                
                default:
                    break;       
            }

            return retVal;
        }

        #endregion FromTextUnderlineType / ToTextUnderlineType

        #region FromTextStrikeType / ToTextStrikeType

        public static ExcelDefaultableBoolean FromTextStrikeType( string value )
        {
            ExcelDefaultableBoolean retVal = ExcelDefaultableBoolean.Default;

            if ( string.IsNullOrEmpty(value) )
                return retVal;

            switch ( value )
            {
                case "noStrike":
                    retVal = ExcelDefaultableBoolean.False;
                    break;

                default:
                    retVal = ExcelDefaultableBoolean.True;
                    break;
            }

            return retVal;
        }

        public static string ToTextStrikeType( ExcelDefaultableBoolean value )
        {
            string retVal = string.Empty;

            switch ( value )
            {
                case ExcelDefaultableBoolean.True:
                    retVal = "sngStrike";
                    break;                    

                case ExcelDefaultableBoolean.False:
                    retVal = "noStrike";
                    break;                    

                default:
                    break;       
            }

            return retVal;
        }

        #endregion FromTextStrikeType / ToTextStrikeType

		// MD 11/11/09 - TFS24618
		#region EnsureMagnificationIsValid

		public static void EnsureMagnificationIsValid( ref int magnification )
		{
			if ( 10 <= magnification && magnification <= 400 )
				return;

			// When an invalid magnification is used, Excel seems to just use 100 instead of the closest constraint.
			magnification = 100;
		}

		#endregion EnsureMagnificationIsValid

        [Conditional("DEBUG")]
        internal static void DebugFail(string message)
        {



            Debug.Fail( message );

        }

        [Conditional("DEBUG")]
        internal static void DebugIndent()
        {



            Debug.Indent( );

        }

        [Conditional("DEBUG")]
        internal static void DebugUnindent()
        {



            Debug.Unindent();

        }

        [Conditional("DEBUG")]
        internal static void DebugWriteLineIf(bool condition, string message)
        {






            Debug.WriteLineIf( condition, message );

        }

        internal static int EncodingGetCodePage(Encoding encoding)
        {



            return encoding.CodePage;

        }

        internal static Encoding EncodingGetEncoding(int codePage)
        {



            return Encoding.GetEncoding(codePage);

        }

        internal static Encoding EncodingDefault
        {
            get
            {



                return Encoding.Default;

            }
        }

        public static CultureInfo GetCultureInfo(int culture)
        {



            return CultureInfo.GetCultureInfo(culture);

        }

        public static int CurrentCultureInfoLCID
        {
            get
            {



                return CultureInfo.CurrentCulture.LCID;

            }
        }

        internal static RegexOptions RegexOptionsCompiled
        {
            get
            {



                return RegexOptions.Compiled;

            }
        }

		// MD 3/18/12 - TFS105148
		// Moved to MathUtilities so it could be shared by both the shared and calc manager assemblies.
		#region Moved

		//        // MD 3/16/12 - TFS105094
		//        // We can't just round to an integer because we could lose precision for large values.
		//        #region Old Code

		//        //        public static int MidpointRoundingAwayFromZero(float value)
		//        //        {
		//        //#if SILVERLIGHT
		//        //            return SilverlightFixes.MidpointRoundingAwayFromZero(value);
		//        //#else
		//        //            return (int) Math.Round(value, MidpointRounding.AwayFromZero);
		//        //#endif
		//        //        }

		//        #endregion // Old Code
		//        public static float MidpointRoundingAwayFromZero(float value)
		//        {
		//            return (float)Utilities.MidpointRoundingAwayFromZero(value, 0);
		//        }

		//        // MD 3/16/12 - TFS105094
		//        // We can't just round to an integer because we could lose precision for large values.
		//        #region Old Code

		//        //        public static int MidpointRoundingAwayFromZero(double value)
		//        //        {
		//        //#if SILVERLIGHT
		//        //            return SilverlightFixes.MidpointRoundingAwayFromZero(value);
		//        //#else
		//        //            return (int) Math.Round(value, MidpointRounding.AwayFromZero);
		//        //#endif
		//        //        }

		//        #endregion // Old Code
		//        public static double MidpointRoundingAwayFromZero(double value)
		//        {
		//            return Utilities.MidpointRoundingAwayFromZero(value, 0);
		//        }

		//#if SILVERLIGHT
		//        // MD 3/16/12 - TFS105094
		//        private static new double[] powersOf10 = new double[] 
		//            { 
		//                1, 
		//                10, 
		//                100, 
		//                1000, 
		//                10000, 
		//                100000, 
		//                1000000, 
		//                10000000, 
		//                100000000, 
		//                1000000000, 
		//                10000000000, 
		//                100000000000, 
		//                1000000000000, 
		//                10000000000000, 
		//                100000000000000, 
		//                1000000000000000 };
		//#endif

		//        // MD 6/7/11 - TFS78166
		//        public static double MidpointRoundingAwayFromZero(double value, int digits)
		//        {
		//            // MD 8/2/11
		//            // Found while fixing TFS81451
		//            // When Math.Round is called with more than 15 digits, it causes an exception, so do the rounding manually.
		//            if (digits > 15)
		//            {
		//                double factor = Math.Pow(10, digits);
		//                int sign = Math.Sign(value);
		//                return Utilities.Truncate(value * factor + 0.5 * sign) / factor;
		//            }

		//#if SILVERLIGHT
		//            // MD 3/16/12 - TFS105094
		//            //return SilverlightFixes.MidpointRoundingAwayFromZero(value, digits);
		//            if (Math.Abs(value) < 1E+16)
		//            {
		//                double powerOf10 = powersOf10[digits];
		//                value *= powerOf10;

		//                double fraction = value % 1;
		//                value = Utilities.Truncate(value);
		//                if (Math.Abs(fraction) >= 0.5)
		//                    value += Math.Sign(fraction);

		//                value /= powerOf10;
		//            }

		//            return value;
		//#else
		//            return Math.Round(value, digits, MidpointRounding.AwayFromZero);
		//#endif
		//        }

		#endregion // Moved

		// MD 2/17/12 - 12.1 - Table Support
		public static Color ColorBlack
		{
			get
			{



				return Color.Black;

			}
		}

		// MD 2/17/12 - 12.1 - Table Support
		public static Color ColorWhite
		{
			get
			{



				return Color.White;

			}
		}

	    /// <summary>
	    /// Replace Color.Empty
	    /// </summary>
	    public static Color ColorEmpty
	    {
	        get
	        {



                return Color.Empty;

	        }
	    }



        public static Color ColorFromHtml(string htmlColor)
        {


#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

            return System.Drawing.ColorTranslator.FromHtml(htmlColor);

        }

        public static Color ColorFromArgb(int argb)
        {

            return Color.FromArgb(argb);



        }

        public static int ColorToArgb(Color color)
        {

            return color.ToArgb();



        }

        public static bool ColorIsEmpty(Color color)
        {



            return color.IsEmpty;

        }

        public static bool ColorIsSystem(Color color)
        {
			// MD 6/21/11 - TFS79214
			// This check is now handled by the SystemColorsInternal.IsDefined method.
//#if SILVERLIGHT
//            foreach (PropertyInfo propertyInfo in typeof(SystemColors).GetProperties())
//            {
//                Color systemColor = (Color)propertyInfo.GetValue(null, null);
//
//                if (Utilities.ColorToArgb(color) == Utilities.ColorToArgb(systemColor))
//                {
//                    return true;
//                }
//            }
//
//            return false;
//
//#else
//            return color.IsSystemColor;
//#endif
			return SystemColorsInternal.IsDefined(color);
        }
        
        public static Point PointEmpty
        {
            get
            {



                return Point.Empty;

            }
        }

        public static PointF PointFEmpty
        {
            get
            {



                return PointF.Empty;

            }
        }


	    #region Extensions methods



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		// MD 1/16/12 - 12.1 - Cell Format Updates
		// These are redundant so they have been removed.
		#region Removed

		//        #region Constants
		//#if SILVERLIGHT
		//        internal static Color SystemColorsWindow
		//        {
		//            get
		//            {
		//                return SystemColorsInternal.WindowColor;
		//            }
		//        }
		//        internal static Color SystemColorsWindowText
		//        {
		//            get
		//            {
		//                return SystemColorsInternal.WindowTextColor;
		//            }
		//        }
		//        internal static Color SystemColorsWindowFrame
		//        {
		//            get
		//            {
		//                return SystemColorsInternal.WindowFrameColor;
		//            }
		//        }
		//#else
		//        internal static Color SystemColorsWindow = SystemColors.Window;
		//        internal static Color SystemColorsWindowText = SystemColors.WindowText;
		//        internal static Color SystemColorsWindowFrame = SystemColors.WindowFrame;
		//#endif

		//        #endregion // Constants

		#endregion // Removed

        #endregion // Extensions methods

        //  BF 9/29/10  2011.1 - Infragistics.Word
        #region GetXmlDeclaration
        internal static string GetXmlDeclaration( string version, string encoding, string standalone )
        {
			StringBuilder builder = new StringBuilder( "version=\"" + version + "\"" );
			if ( encoding.Length > 0 )
			{
				builder.Append( " encoding=\"" );
				builder.Append( encoding );
				builder.Append( "\"" );
			}
			if ( standalone.Length > 0 )
			{
				builder.Append( " standalone=\"" );
				builder.Append( standalone );
				builder.Append( "\"" );
			}

            return builder.ToString();
        }
        #endregion GetXmlDeclaration

		// MD 5/31/11 - TFS75574
		#region Removed

		//// MD 4/18/11 - TFS62026
		//#region FindNextSharedElementKey
		//
		//public static uint FindNextSharedElementKey<T>(SortedList<uint, T> values, ref uint nextKey)
		//{
		//    if (nextKey == UInt32.MaxValue)
		//    {
		//        for (uint i = 1; i < UInt32.MaxValue; i++)
		//        {
		//            if (values.ContainsKey(i) == false)
		//                return i;
		//        }
		//
		//        Utilities.DebugFail("This should never happen.");
		//        return 0;
		//    }
		//    else
		//    {
		//        return nextKey++;
		//    }
		//} 
		//
		//#endregion // FindNextSharedElementKey

		#endregion  // Removed

		// MD 3/18/12 - TFS105148
		// Moved to MathUtilities so it could be shared by both the shared and calc manager assemblies.
		#region Moved

		//        // MD 8/2/11
		//        // Found while fixing TFS81451
		//        // This is needed for the change in the MidpointRoundingAwayFromZero method.
		//        #region Truncate

		//        public static double Truncate(double value)
		//        {
		//#if SILVERLIGHT
		//            if (value < 0)
		//                return Math.Ceiling(value);

		//            return Math.Floor(value);
		//#else
		//            return Math.Truncate(value);
		//#endif
		//        }

		//        #endregion // Truncate

		#endregion // Moved

		// MD 8/23/11 - TFS84306
		#region FromFixedPoint16_16Value

		public static float FromFixedPoint16_16Value(uint value)
		{
			// MD 7/24/12 - TFS115693
			// This conversion is incorrect and doesn't account for negative values.
			//uint integerPortion = value >> 16;
			//double fractionPortion = (value & 0x0000FFFF) / (double)0x00010000;
			//
			//return integerPortion + fractionPortion;
			short integerPortion = (short)(value >> 16);
			ushort fractionalPortion = (ushort)(value & 0xFFFF);
			return integerPortion + (fractionalPortion / (float)0x10000);
		}

		#endregion  // FromFixedPoint16_16Value

		// MD 8/23/11 - TFS84306
		#region ToFixedPoint16_16Value

		public static uint ToFixedPoint16_16Value(double value)
		{
			// MD 7/24/12 - TFS115693
			// This conversion is incorrect and doesn't account for negative values.
			//Debug.Assert(opactiy >= 0, "The Fixed point 16.16 value cannot be negative.");
			//
			//double intPortion = MathUtilities.Truncate(opactiy);
			//Debug.Assert(intPortion < UInt16.MaxValue, "The integer portion should not take more than 16 bits.");
			//
			//uint intPortionAsInt = (uint)intPortion;
			//
			//double fractionPortion = opactiy % 1;
			//double shiftedFractionPortion = fractionPortion * 0x10000;
			//uint fractionPortionAsInt = (uint)MathUtilities.MidpointRoundingAwayFromZero(shiftedFractionPortion);
			//
			//return (intPortionAsInt << 16) | fractionPortionAsInt;
			double temp = MathUtilities.Truncate(value);
			Debug.Assert(Int16.MinValue <= temp && temp <= Int16.MaxValue, "The integer portion should not take more than 16 bits.");
			short integerPortion = (short)temp;

			temp = Math.Abs(value % 1) * 0x10000;
			ushort fractionPortion = (ushort)MathUtilities.MidpointRoundingAwayFromZero(temp);

			return (uint)(integerPortion << 16) + fractionPortion;
		}

		#endregion  // ToFixedPoint16_16Value

		// MD 11/8/11 - TFS85193
		#region GetLines



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		public static List<string> GetLines(string value, out List<int> lineIndexes, out List<string> newlines)
		{
			lineIndexes = new List<int>();
			lineIndexes.Add(0);

			newlines = new List<string>();

			List<string> lines = new List<string>();

			int startOfLastLine = 0;
			for (int i = 0; i < value.Length; i++)
			{
				switch (value[i])
				{
					case '\r':
						lines.Add(value.Substring(startOfLastLine, i - startOfLastLine));

						// If we have a "\r\n", skip past the \n so we don't count two newlines.
						if (i < value.Length - 1 && value[i + 1] == '\n')
						{
							newlines.Add("\r\n");
							i++;
						}
						else
						{
							newlines.Add("\r");
						}

						startOfLastLine = i + 1;
						lineIndexes.Add(startOfLastLine);
						break;

					case '\n':
						newlines.Add("\n");
						lines.Add(value.Substring(startOfLastLine, i - startOfLastLine));
						startOfLastLine = i + 1;
						lineIndexes.Add(startOfLastLine);
						break;
				}
			}

			lines.Add(value.Substring(startOfLastLine));

			return lines;
		}

		#endregion  // GetLines

		// MD 11/10/11 - TFS85193
		#region TrimFormattingRuns

		public static void TrimFormattingRuns(IFormattedRunOwner runOwner)
		{
			// MD 2/2/12 - TFS100573
			// Since we are going to be removing runs, we don't need a workbook reference.
			//Workbook workbook = runOwner.Workbook;
			//List<FormattingRunBase> formattingRuns = runOwner.GetFormattingRuns(workbook);
			List<FormattingRunBase> formattingRuns = runOwner.GetFormattingRuns(null);

			string unformattedString = runOwner.UnformattedString;

			for (int i = formattingRuns.Count - 1; i >= 0; i--)
			{
				if (unformattedString.Length <= formattingRuns[i].FirstFormattedCharInOwner)
				{
					FormattingRunBase run = formattingRuns[i];

					if (run.HasFont)
					{
						// MD 2/2/12 - TFS100573
						// Since we are going to be removing runs, we don't need a workbook reference.
						//run.GetFontInternal(workbook).OnUnrooted();
						run.GetFontInternal(null).OnUnrooted();
					}

					formattingRuns.RemoveAt(i);
				}
				else
				{
					break;
				}
			}
		}

		#endregion  // TrimFormattingRuns

		// MD 11/10/11 - TFS85193
		#region EnumTryParse

		public static bool EnumTryParse<T>(string text, out T value) where T : struct
		{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			return Enum.TryParse<T>(text, out value);

		}

		#endregion  // EnumTryParse

		// MD 11/29/11 - TFS96205
		#region ComputeMsoCrc32

		// http://msdn.microsoft.com/en-us/library/dd922675(v=office.12).aspx
		public static uint ComputeMsoCrc32(uint crcValue, byte[] data)
		{
			Utilities.InitMsoCrc32Cache();
			for (int i = 0; i < data.Length; i++)
			{
				byte b = data[i];

				byte index = (byte)((crcValue >> 24) ^ b);
				crcValue <<= 8;
				crcValue ^= Utilities.msoCrc32Cache[index];
			}

			return crcValue;
		}

		[ThreadStatic]
		private static uint[] msoCrc32Cache;

		private static void InitMsoCrc32Cache()
		{
			if (Utilities.msoCrc32Cache != null)
				return;

			Utilities.msoCrc32Cache = new uint[256];
			for (int index = 0; index < 256; index++)
			{
				uint value = (uint)(index << 24);

				for (int bit = 0; bit < 8; bit++)
				{
					if ((value & 0x80000000) != 0)
					{
						value <<= 1;
						value ^= 0xAF;
					}
					else
					{
						value <<= 1;
					}
				}

				value &= 0xFFFF;
				Utilities.msoCrc32Cache[index] = value;
			}
		}

		#endregion  // ComputeMsoCrc32

		// MD 11/29/11 - TFS96205
		#region DecodeLongRGBA

		// http://msdn.microsoft.com/en-us/library/dd926722(v=office.12).aspx
		public static Color DecodeLongRGBA(uint value)
		{
			byte r = (byte)((value & 0x000000FF));
			byte g = (byte)((value & 0x0000FF00) >> 8);
			byte b = (byte)((value & 0x00FF0000) >> 16);
			byte a = (byte)((value & 0xFF000000) >> 24);

			return Color.FromArgb(a, r, g, b);
		}

		#endregion  // DecodeLongRGBA

		// MD 11/29/11 - TFS96205
		#region EncodeLongRGBA

		// http://msdn.microsoft.com/en-us/library/dd926722(v=office.12).aspx
		public static uint EncodeLongRGBA(Color value)
		{
			return (uint)(
				(value.R) |
				(value.G << 8) |
				(value.B << 16) |
				(value.A << 24));
		}

		#endregion  // EncodeLongRGBA

		// MD 1/18/12 - 12.1 - Cell Format Updates
		#region GetDouble

		public static double GetDouble(byte[] buffer, ref int bufferIndex)
		{
			double retVal = BitConverter.ToDouble(buffer, bufferIndex);
			bufferIndex += 8;
			return retVal;
		}

		#endregion  // GetInt32

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region GetInt16

		public static short GetInt16(byte[] buffer, ref int bufferIndex)
		{
			short retVal = BitConverter.ToInt16(buffer, bufferIndex);
			bufferIndex += 2;
			return retVal;
		}

		#endregion  // GetInt16

		// MD 1/18/12 - 12.1 - Cell Format Updates
		#region GetInt32

		public static int GetInt32(byte[] buffer, ref int bufferIndex)
		{
			int retVal = BitConverter.ToInt32(buffer, bufferIndex);
			bufferIndex += 4;
			return retVal;
		}

		#endregion  // GetInt32

		// MD 11/29/11 - TFS96205
		#region GetUInt16

		public static ushort GetUInt16(byte[] buffer, ref int bufferIndex)
		{
			ushort retVal = BitConverter.ToUInt16(buffer, bufferIndex);
			bufferIndex += 2;
			return retVal;
		}

		#endregion  // GetUInt16

		// MD 11/29/11 - TFS96205
		#region GetUInt32

		public static uint GetUInt32(byte[] buffer, ref int bufferIndex)
		{
			uint retVal = BitConverter.ToUInt32(buffer, bufferIndex);
			bufferIndex += 4;
			return retVal;
		}

		#endregion  // GetUInt32

		// MD 1/19/12 - 12.1 - Cell Format Updates
		#region SetDouble

		public static void SetDouble(double value, byte[] buffer, ref int bufferIndex)
		{
			byte[] valueData = BitConverter.GetBytes(value);
			Buffer.BlockCopy(valueData, 0, buffer, bufferIndex, valueData.Length);
			bufferIndex += valueData.Length;
		}

		#endregion  // SetDouble

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region SetInt16

		public static void SetInt16(short value, byte[] buffer, ref int bufferIndex)
		{
			byte[] valueData = BitConverter.GetBytes(value);
			Buffer.BlockCopy(valueData, 0, buffer, bufferIndex, valueData.Length);
			bufferIndex += valueData.Length;
		}

		#endregion  // SetInt16

		// MD 1/19/12 - 12.1 - Cell Format Updates
		#region SetInt32

		public static void SetInt32(int value, byte[] buffer, ref int bufferIndex)
		{
			byte[] valueData = BitConverter.GetBytes(value);
			Buffer.BlockCopy(valueData, 0, buffer, bufferIndex, valueData.Length);
			bufferIndex += valueData.Length;
		}

		#endregion  // SetInt32

		// MD 11/29/11 - TFS96205
		#region SetUInt16

		public static void SetUInt16(ushort value, byte[] buffer, ref int bufferIndex)
		{
			byte[] valueData = BitConverter.GetBytes(value);
			Buffer.BlockCopy(valueData, 0, buffer, bufferIndex, valueData.Length);
			bufferIndex += valueData.Length;
		}

		#endregion  // SetUInt16

		// MD 11/29/11 - TFS96205
		#region SetUInt32

		public static void SetUInt32(uint value, byte[] buffer, ref int bufferIndex)
		{
			byte[] valueData = BitConverter.GetBytes(value);
			Buffer.BlockCopy(valueData, 0, buffer, bufferIndex, valueData.Length);
			bufferIndex += valueData.Length;
		}

		#endregion  // SetUInt32

		// MD 1/6/12 - TFS98536
		#region FixupCellString

		public static string FixupCellString(string value)
		{
			if(value == null)
				return null;

			return value.Replace("\0", "");
		}

		#endregion  // FixupCellString

		// MD 1/5/12 - TFS98535
		#region RemoveCarriageReturns

		public static string RemoveCarriageReturns(string promptBoxMessage)
		{
			if (promptBoxMessage == null)
				return null;

			return promptBoxMessage.Replace("\r\n", "\n");
		}

		#endregion  // RemoveCarriageReturns

		#region SaveDataToDisk
      


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

    
		#endregion // SaveDataToDisk

		// MD 12/7/11 - 12.1 - Table Support
		public const int DiagonalDownBit = 0x02;
		public const int DiagonalUpBit = 0x04;

		#region AddBits

		internal static void AddBits(ref byte value, int valueToAdd, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 8, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 8, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;
			Debug.Assert(valueToAdd < 1 << valueSize, "The lastBitIndexis is greater than the firstBitIndex.");

			value |= (byte)(valueToAdd << firstBitIndex);
		}

		internal static void AddBits(ref ushort value, int valueToAdd, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 16, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 16, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;
			Debug.Assert(valueToAdd < 1 << valueSize, "The lastBitIndexis is greater than the firstBitIndex.");

			value |= (ushort)(valueToAdd << firstBitIndex);
		}

		internal static void AddBits(ref uint value, int valueToAdd, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 32, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 32, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;
			Debug.Assert(valueToAdd < 1 << valueSize, "The lastBitIndexis is greater than the firstBitIndex.");

			value |= (uint)((uint)valueToAdd << firstBitIndex);
		}

		internal static void AddBits(ref ulong value, int valueToAdd, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 64, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 64, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;
			Debug.Assert(valueToAdd < 1 << valueSize, "The lastBitIndexis is greater than the firstBitIndex.");

			value |= (ulong)((ulong)valueToAdd << firstBitIndex);
		}

		#endregion // AddBits

		#region ClearBits

		internal static void ClearBits(ref byte value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 8, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 8, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;
			int mask = ((1 << valueSize) - 1) << firstBitIndex;
			value &= (byte)~mask;
		}

		internal static void ClearBits(ref ushort value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 16, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 16, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;
			int mask = ((1 << valueSize) - 1) << firstBitIndex;
			value &= (ushort)~mask;
		}

		internal static void ClearBits(ref uint value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 32, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 32, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;
			uint mask = (uint)((1 << valueSize) - 1) << firstBitIndex;
			value &= (uint)~mask;
		}

		internal static void ClearBits(ref ulong value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 64, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 64, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;
			ulong mask = (ulong)((1 << valueSize) - 1) << firstBitIndex;
			value &= (ulong)~mask;
		}

		#endregion // ClearBits

		#region ColorToHtml

		public static string ColorToHtml(Color color)
		{



			return System.Drawing.ColorTranslator.ToHtml(color);

		}

		#endregion // ColorToHtml

		#region CompareFillsForSortOrFilter

		// MD 5/7/12 - TFS106831
		//public static bool CompareFillsForSortOrFilter(WorksheetCellFormatData cellFormat, CellFill expectedFill)
		public static bool CompareFillsForSortOrFilter(Workbook workbook, WorksheetCellFormatData cellFormat, CellFill expectedFill)
		{
			CellFill fill = cellFormat.FillResolved;

			// MD 5/7/12 - TFS106831
			// Resolve both fills before comparing them.
			if (workbook != null)
			{
				fill = fill.ToResolvedColorFill(workbook);
				expectedFill = expectedFill.ToResolvedColorFill(workbook);
			}

			CellFillPattern patternFill = fill as CellFillPattern;
			CellFillPattern expectedPatternFill = expectedFill as CellFillPattern;
			if (patternFill != null && expectedPatternFill != null)
			{
				if (patternFill.PatternStyle == FillPatternStyle.None)
				{
					return expectedPatternFill.PatternStyle == FillPatternStyle.None;
				}
				else if (patternFill.PatternStyle == FillPatternStyle.Solid)
				{
					return
						expectedPatternFill.PatternStyle == FillPatternStyle.Solid &&
						patternFill.BackgroundColorInfo == expectedPatternFill.BackgroundColorInfo;
				}
			}

			return fill.Equals(expectedFill);
		}

		#endregion // CompareFillsForSortOrFilter

		#region CreateCalendar

		public static Calendar CreateCalendar(CalendarType type)
		{
			switch (type)
			{
				case CalendarType.Gregorian:
					return new GregorianCalendar();

				case CalendarType.GregorianArabic:
					return new GregorianCalendar(GregorianCalendarTypes.Arabic);

				case CalendarType.GregorianMeFrench:
					return new GregorianCalendar(GregorianCalendarTypes.MiddleEastFrench);

				case CalendarType.GregorianUs:
					return new GregorianCalendar(GregorianCalendarTypes.USEnglish);

				case CalendarType.GregorianXlitEnglish:
					return new GregorianCalendar(GregorianCalendarTypes.TransliteratedEnglish);

				case CalendarType.GregorianXlitFrench:
					return new GregorianCalendar(GregorianCalendarTypes.TransliteratedFrench);

				case CalendarType.Hebrew:
					return new HebrewCalendar();

				case CalendarType.Hijri:
					return new HijriCalendar();

				case CalendarType.Japan:
					return new JapaneseCalendar();

				case CalendarType.Korea:
					return new KoreanCalendar();

				case CalendarType.None:
					return CultureInfo.CurrentCulture.Calendar;

				case CalendarType.Saka:
					Utilities.DebugFail("Not sure how to create the Saka calendar.");
					return CultureInfo.CurrentCulture.Calendar;

				case CalendarType.Taiwan:
					return new TaiwanCalendar();

				case CalendarType.Thai:
					return new ThaiBuddhistCalendar();

				default:
					Utilities.DebugFail("Unknown CalendarType: " + type);
					return CultureInfo.CurrentCulture.Calendar;
			}
		}

		#endregion // CreateCalendar

		#region CreateWildcardRegex

		public static Regex CreateWildcardRegex(string searchText, out bool hasWildcards)
		{
			hasWildcards = false;

			System.Text.StringBuilder patternBuilder = new System.Text.StringBuilder("\\A" + Regex.Escape(searchText) + "\\z");
			for (int i = 0; i < patternBuilder.Length; i++)
			{
				switch (patternBuilder[i])
				{
					case '~':
						patternBuilder.Remove(i++, 1);

						if (i < patternBuilder.Length - 1 && patternBuilder[i] == '\\')
						{
							switch (patternBuilder[i + 1])
							{
								case '?':
								case '*':
									patternBuilder.Remove(i, 1);
									break;
							}
						}
						break;

					case '\\':
						switch (patternBuilder[i + 1])
						{
							case '?':
								patternBuilder.Remove(i, 2);
								patternBuilder.Insert(i, ".");
								hasWildcards = true;
								break;

							case '*':
								patternBuilder.Remove(i, 2);
								patternBuilder.Insert(i, ".*");
								hasWildcards = true;
								break;
						}
						break;
				}
			}

			return new Regex(patternBuilder.ToString(), RegexOptions.IgnoreCase);
		}

		#endregion // CreateWildcardRegex

		#region EscapeWildcards

		public static string EscapeWildcards(string value)
		{
			System.Text.StringBuilder escapedString = new System.Text.StringBuilder(value);
			for (int i = 0; i < escapedString.Length; i++)
			{
				switch (escapedString[i])
				{
					case '*':
					case '?':
						escapedString.Insert(i++, "~");
						break;
				}
			}

			return escapedString.ToString();
		}

		#endregion // EscapeWildcards

		#region GetBits

		internal static int GetBits(byte value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 8, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 8, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;

			int mask = (1 << valueSize) - 1;
			return (value >> firstBitIndex) & mask;
		}

		internal static int GetBits(ushort value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 16, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 16, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;

			int mask = (1 << valueSize) - 1;
			return (value >> firstBitIndex) & mask;
		}

		internal static int GetBits(uint value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 32, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 32, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;

			int mask = (1 << valueSize) - 1;
			return (int)((value >> firstBitIndex) & mask);
		}

		internal static int GetBits(int value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 32, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 32, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;

			int mask = (1 << valueSize) - 1;
			return (int)((value >> firstBitIndex) & mask);
		}

		internal static int GetBits(ulong value, int firstBitIndex, int lastBitIndex)
		{
			Debug.Assert(0 <= firstBitIndex && firstBitIndex < 64, "The firstBitIndex is out of range.");
			Debug.Assert(0 <= lastBitIndex && lastBitIndex < 64, "The lastBitIndexis out of range.");
			Debug.Assert(firstBitIndex <= lastBitIndex, "The lastBitIndexis is greater than the firstBitIndex.");

			int valueSize = lastBitIndex - firstBitIndex + 1;

			ulong mask = (ulong)(1 << valueSize) - 1;
			return (int)((value >> firstBitIndex) & mask);
		}

		#endregion // GetBits

		#region GetFirstMonthOfQuarter

		public static int GetFirstMonthOfQuarter(int quarter)
		{
			return ((quarter - 1) * 3) + 1;
		}

		#endregion // GetFirstMonthOfQuarter

		#region GetQuarter

		public static int GetQuarter(DateTime value)
		{
			return ((value.Month - 1) / 3) + 1;
		}

		#endregion // GetQuarter

		#region IsCellValueNull

		public static bool IsCellValueNull(object cellValue)
		{
			return cellValue == null || cellValue is DBNull;
		}

		#endregion // IsCellValueNull

		#region IsDiagonalDownSet

		public static bool IsDiagonalDownSet(DiagonalBorders diagonalBorders)
		{
			return (diagonalBorders & DiagonalBorders.DiagonalDown) == DiagonalBorders.DiagonalDown;
		}

		#endregion // IsDiagonalDownSet

		#region IsDiagonalUpSet

		public static bool IsDiagonalUpSet(DiagonalBorders diagonalBorders)
		{
			return (diagonalBorders & DiagonalBorders.DiagonalUp) == DiagonalBorders.DiagonalUp;
		} 

		#endregion // IsDiagonalUpSet

		// MD 3/22/12 - TFS104630
		#region IsEdgeBorderValue

		public static bool IsEdgeBorderValue(CellFormatValue value)
		{
			return
				Utilities.IsHorizontalBorderValue(value) ||
				Utilities.IsVerticalBorderValue(value);
		}

		#endregion // IsEdgeBorderValue

		// MD 3/22/12 - TFS104630
		#region IsHorizontalBorderValue

		internal static bool IsHorizontalBorderValue(CellFormatValue value)
		{
			switch (value)
			{
				case CellFormatValue.BottomBorderColorInfo:
				case CellFormatValue.BottomBorderStyle:
				case CellFormatValue.TopBorderColorInfo:
				case CellFormatValue.TopBorderStyle:
					return true;

				default:
					return false;
			}
		}

		#endregion // IsHorizontalBorderValue

		// MD 4/3/12 - TFS107243
		#region IsNumber

		public static bool IsNumber(object value)
		{
			return
				value is sbyte ||
				value is byte ||
				value is short ||
				value is ushort ||
				value is int ||
				value is uint ||
				value is long ||
				value is ulong ||
				value is float ||
				value is double ||
				value is decimal;
		}

		#endregion // IsNumber

		// MD 3/22/12 - TFS104630
		#region IsVerticalBorderValue

		internal static bool IsVerticalBorderValue(CellFormatValue value)
		{
			switch (value)
			{
				case CellFormatValue.LeftBorderColorInfo:
				case CellFormatValue.LeftBorderStyle:
				case CellFormatValue.RightBorderColorInfo:
				case CellFormatValue.RightBorderStyle:
					return true;

				default:
					return false;
			}
		}

		#endregion // IsVerticalBorderValue

		#region RemoveAlphaChannel

		public static Color RemoveAlphaChannel(Color color)
		{
			if (Utilities.ColorIsEmpty(color) || color.A == 255)
				return color;

			return Color.FromArgb(255, color.R, color.G, color.B);
		}

		#endregion // RemoveAlphaChannel

		#region RemoveDiagonalDownBit

		public static void RemoveDiagonalDownBit(ref DiagonalBorders diagonalBorders)
		{
			diagonalBorders &= ~(DiagonalBorders)Utilities.DiagonalDownBit;
		}

		#endregion // RemoveDiagonalDownBit

		#region RemoveDiagonalUpBit

		public static void RemoveDiagonalUpBit(ref DiagonalBorders diagonalBorders)
		{
			diagonalBorders &= ~(DiagonalBorders)Utilities.DiagonalUpBit;
		}

		#endregion // RemoveDiagonalUpBit

		#region RGBToCIELAB

		// http://www.easyrgb.com/index.php?X=MATH
		// http://www.cs.rit.edu/~ncs/color/t_convert.html
		public static void RGBToCIELAB(Color color, out double l, out double a, out double b)
		{
			double normalizedR = (color.R / 255d);
			double normalizedG = (color.G / 255d);
			double normalizedB = (color.B / 255d);
			double X = normalizedR * 0.412453 + normalizedG * 0.357580 + normalizedB * 0.180423;
			double Y = normalizedR * 0.212671 + normalizedG * 0.715160 + normalizedB * 0.072169;
			double Z = normalizedR * 0.019334 + normalizedG * 0.119193 + normalizedB * 0.950227;

			double tempX = RGBToCIELABHelper(X / 0.95047d);
			double tempY = RGBToCIELABHelper(Y / 1d);
			double tempZ = RGBToCIELABHelper(Z / 1.08883d);

			l = (116 * tempY) - 16;
			a = 500 * (tempX - tempY);
			b = 200 * (tempY - tempZ);
		}

		private static double RGBToCIELABHelper(double value)
		{
			if (value > 0.008856)
				return Math.Pow(value, (1 / 3d));
			else
				return (7.787 * value) + (16 / 116d);
		}

		#endregion // RGBToCIELAB

		#region SetBit

		public static void SetBit(ref byte value, ExcelDefaultableBoolean testValue, int bitIndex)
		{
			Utilities.SetBit(ref  value, testValue == ExcelDefaultableBoolean.True, bitIndex);
		}

		public static void SetBit(ref byte value, bool testValue, int bitIndex)
		{
			Debug.Assert(0 <= bitIndex && bitIndex < 8, "The bitIndex is out of range.");

			byte rawValue = (byte)(1 << bitIndex);
			if (testValue)
				value |= rawValue;
			else
				value &= (byte)~rawValue;
		}

		public static void SetBit(ref ushort value, ExcelDefaultableBoolean testValue, int bitIndex)
		{
			Utilities.SetBit(ref  value, testValue == ExcelDefaultableBoolean.True, bitIndex);
		}

		public static void SetBit(ref ushort value, bool testValue, int bitIndex)
		{
			Debug.Assert(0 <= bitIndex && bitIndex < 16, "The bitIndex is out of range.");

			ushort rawValue = (ushort)(1 << bitIndex);
			if (testValue)
				value |= rawValue;
			else
				value &= (ushort)~rawValue;
		}

		public static void SetBit(ref uint value, bool testValue, int bitIndex)
		{
			Debug.Assert(0 <= bitIndex && bitIndex < 32, "The bitIndex is out of range.");

			uint rawValue = (uint)(1 << bitIndex);
			if (testValue)
				value |= rawValue;
			else
				value &= (uint)~rawValue;
		}

		#endregion // SetBit

		// MD 3/22/12 - TFS104630
		#region SynchronizeOverlappingBorderProperties

		public static void SynchronizeOverlappingBorderProperties(
			WorksheetCellFormatProxy format,
			WorksheetCellFormatProxy adjacentFormat,
			CellFormatValue borderValue,
			object formatValue,
			CellFormatValueChangedOptions options)
		{
			CellFormatValue associatedValue = Utilities.GetAssociatedBorderValue(borderValue);
			CellFormatValue oppositeValue = Utilities.GetOppositeBorderValue(borderValue);
			CellFormatValue associatedOppositeBorderValue = Utilities.GetAssociatedBorderValue(oppositeValue);

			// If we don't have the associated border value set, but that overlapping property is set on the adjacent format,
			// take the value from it before we reset that property below.
			object associatedOppositeBorder = adjacentFormat.GetValue(associatedOppositeBorderValue);
			if (WorksheetCellFormatData.IsValueDefault(associatedOppositeBorderValue, associatedOppositeBorder) == false &&
				format.IsValueDefault(associatedValue))
			{
				format.SetValue(associatedValue, associatedOppositeBorder, true, options);
			}

			// If the overlapping border doesn't have the same value, reset the value and its associated border value as well.
			// MD 5/6/12 - TFS110650
			// For value types, we were always getting in this if block because they were boxed and we were comparing the boxed values,
			// not the values themselves. Object.Equals will return the correct result.
			//if (adjacentFormat.GetValue(oppositeValue) != formatValue)
			if (Object.Equals(adjacentFormat.GetValue(oppositeValue), formatValue) == false)
			{
				adjacentFormat.ResetValue(oppositeValue, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
				adjacentFormat.ResetValue(associatedOppositeBorderValue, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			}
		}

		#endregion // SynchronizeOverlappingBorderProperties

		#region TestBit

		public static bool TestBit(ushort value, int bitIndex)
		{
			Debug.Assert(0 <= bitIndex && bitIndex < 16, "The bitIndex is out of range.");
			ushort bit = (ushort)(1 << bitIndex);
			return (value & bit) == bit;
		}

		public static bool TestBit(uint value, int bitIndex)
		{
			Debug.Assert(0 <= bitIndex && bitIndex < 32, "The bitIndex is out of range.");
			uint bit = (uint)1 << bitIndex;
			return unchecked(value & bit) == bit;
		}

		#endregion // TestBit

		#region TestFlag

		public static bool TestFlag(DiagonalBorders value, DiagonalBorders flag)
		{
			return (value & flag) == flag;
		}

		public static bool TestFlag(PreventTextFormattingTypes value, PreventTextFormattingTypes flag)
		{
			return (value & flag) == flag;
		}

		public static bool TestFlag(WorksheetCellFormatOptions value, WorksheetCellFormatOptions flag)
		{
			return (value & flag) == flag;
		}

		#endregion // TestFlag

		#region ToColorInfo

		public static WorkbookColorInfo ToColorInfo(Color color)
		{
			if (Utilities.ColorIsEmpty(color))
				return null;

			return new WorkbookColorInfo(color);
		}

		#endregion // ToColorInfo

		#region ToDefaultableBoolean

		public static ExcelDefaultableBoolean ToDefaultableBoolean(bool value)
		{
			return value ? ExcelDefaultableBoolean.True : ExcelDefaultableBoolean.False;
		}

		#endregion // ToDefaultableBoolean

		#region TryGetNumericValue

		public static bool TryGetNumericValue(Workbook workbook, object value, out double numericValue)
		{
			if (value is DateTime)
			{
				double? testValue = ExcelCalcValue.DateTimeToExcelDate(workbook, (DateTime)value);

				if (testValue.HasValue)
				{
					numericValue = testValue.Value;
					return true;
				}
			}
			else if (value is double)
			{
				numericValue = (double)value;
				return true;
			}
			else if (value is int)
			{
				numericValue = (double)(int)value;
				return true;
			}
			else if (
				value is float ||
				value is long ||
				value is ulong ||
				value is uint ||
				value is short ||
				value is ushort ||
				value is sbyte ||
				value is byte ||
				value is decimal)
			{
				try
				{
					IConvertible convertible = (IConvertible)value;

					// MD 4/9/12 - TFS101506
					//numericValue = convertible.ToDouble(CultureInfo.CurrentCulture);
					CultureInfo culture = CultureInfo.CurrentCulture;
					if (workbook != null)
						culture = workbook.CultureResolved;

					numericValue = convertible.ToDouble(culture);

					return true;
				}
				catch { }
			}
			else if (value is TimeSpan)
			{
				numericValue = ExcelCalcValue.TimeOfDayToExcelDate((TimeSpan)value);
				return true;
			}
			else if (value == ErrorValue.Circularity)
			{
				numericValue = 0;
				return true;
			}

			numericValue = double.NaN;
			return false;
		}

		#endregion // TryGetNumericValue

		#region UnescapeWildcards

		public static string UnescapeWildcards(string value)
		{
			System.Text.StringBuilder unescapedString = new System.Text.StringBuilder(value);
			for (int i = 0; i < unescapedString.Length; i++)
			{
				switch (unescapedString[i])
				{
					case '~':
						unescapedString.Remove(i, 1);
						break;
				}
			}

			return unescapedString.ToString();
		}

		#endregion // UnescapeWildcards

		#region VerifyCollectionIndex

		public static void VerifyCollectionIndex(int index, int count)
		{
			if (index < 0 || count <= index)
				throw new ArgumentOutOfRangeException(SR.GetString("LE_ArgumentOutOfRangeException_InvalidCollectionIndex"));
		}

		#endregion // VerifyCollectionIndex

		#region VerifyEnumValue

		public static void VerifyEnumValue<T>(T value)
		{
			Utilities.VerifyEnumValue<T>(value, "value");
		}

		public static void VerifyEnumValue<T>(T value, string paramName)
		{
			Debug.Assert(typeof(T).IsEnum, "VerifyEnumValue should only be called for Enums.");

			if (Enum.IsDefined(typeof(T), value) == false)
				throw new InvalidEnumArgumentException(paramName, Convert.ToInt32(value), typeof(T));
		}

		#endregion // VerifyEnumValue

		#region VerifyExcelDefaultableBoolean

		public static void VerifyExcelDefaultableBoolean(ExcelDefaultableBoolean value, string paramName)
		{
			if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
				throw new InvalidEnumArgumentException(paramName, (int)value, typeof(ExcelDefaultableBoolean));
		}

		#endregion // VerifyExcelDefaultableBoolean

		#region Property Accessor Delegates

		public delegate TValue PropertyGetter<T, TValue>(T instance);
		public delegate void PropertySetter<T, TValue>(T instance, TValue value);

		public static readonly PropertyGetter<IWorkbookFont, ExcelDefaultableBoolean> FontBoldGetter = new PropertyGetter<IWorkbookFont, ExcelDefaultableBoolean>(FontBoldGetterHelper);
		private static ExcelDefaultableBoolean FontBoldGetterHelper(IWorkbookFont instance)
		{
			return instance.Bold;
		}

		public static readonly PropertyGetter<IWorkbookFont, WorkbookColorInfo> FontColorInfoGetter = new PropertyGetter<IWorkbookFont, WorkbookColorInfo>(FontColorInfoGetterHelper);
		private static WorkbookColorInfo FontColorInfoGetterHelper(IWorkbookFont instance)
		{
			return instance.ColorInfo;
		}

		public static readonly PropertyGetter<IWorkbookFont, int> FontHeightGetter = new PropertyGetter<IWorkbookFont, int>(FontHeightGetterHelper);
		private static int FontHeightGetterHelper(IWorkbookFont instance)
		{
			return instance.Height;
		}

		public static readonly PropertyGetter<IWorkbookFont, ExcelDefaultableBoolean> FontItalicGetter = new PropertyGetter<IWorkbookFont, ExcelDefaultableBoolean>(FontItalicGetterHelper);
		private static ExcelDefaultableBoolean FontItalicGetterHelper(IWorkbookFont instance)
		{
			return instance.Italic;
		}

		public static readonly PropertyGetter<IWorkbookFont, string> FontNameGetter = new PropertyGetter<IWorkbookFont, string>(FontNameGetterHelper);
		private static string FontNameGetterHelper(IWorkbookFont instance)
		{
			return instance.Name;
		}

		public static readonly PropertyGetter<IWorkbookFont, ExcelDefaultableBoolean> FontStrikeoutGetter = new PropertyGetter<IWorkbookFont, ExcelDefaultableBoolean>(FontStrikeoutGetterHelper);
		private static ExcelDefaultableBoolean FontStrikeoutGetterHelper(IWorkbookFont instance)
		{
			return instance.Strikeout;
		}

		public static readonly PropertyGetter<IWorkbookFont, FontSuperscriptSubscriptStyle> FontSuperscriptSubscriptStyleGetter = new PropertyGetter<IWorkbookFont, FontSuperscriptSubscriptStyle>(FontSuperscriptSubscriptStyleGetterHelper);
		private static FontSuperscriptSubscriptStyle FontSuperscriptSubscriptStyleGetterHelper(IWorkbookFont instance)
		{
			return instance.SuperscriptSubscriptStyle;
		}

		public static readonly PropertyGetter<IWorkbookFont, FontUnderlineStyle> FontUnderlineStyleGetter = new PropertyGetter<IWorkbookFont, FontUnderlineStyle>(FontUnderlineStyleGetterHelper);
		private static FontUnderlineStyle FontUnderlineStyleGetterHelper(IWorkbookFont instance)
		{
			return instance.UnderlineStyle;
		}

		public static readonly PropertySetter<IWorkbookFont, ExcelDefaultableBoolean> FontBoldSetter = new PropertySetter<IWorkbookFont, ExcelDefaultableBoolean>(FontBoldSetterHelper);
		private static void FontBoldSetterHelper(IWorkbookFont instance, ExcelDefaultableBoolean value)
		{
			instance.Bold = value;
		}

		public static readonly PropertySetter<IWorkbookFont, WorkbookColorInfo> FontColorInfoSetter = new PropertySetter<IWorkbookFont, WorkbookColorInfo>(FontColorInfoSetterHelper);
		private static void FontColorInfoSetterHelper(IWorkbookFont instance, WorkbookColorInfo value)
		{
			instance.ColorInfo = value;
		}

		public static readonly PropertySetter<IWorkbookFont, int> FontHeightSetter = new PropertySetter<IWorkbookFont, int>(FontHeightSetterHelper);
		private static void FontHeightSetterHelper(IWorkbookFont instance, int value)
		{
			instance.Height = value;
		}

		public static readonly PropertySetter<IWorkbookFont, ExcelDefaultableBoolean> FontItalicSetter = new PropertySetter<IWorkbookFont, ExcelDefaultableBoolean>(FontItalicSetterHelper);
		private static void FontItalicSetterHelper(IWorkbookFont instance, ExcelDefaultableBoolean value)
		{
			instance.Italic = value;
		}

		public static readonly PropertySetter<IWorkbookFont, string> FontNameSetter = new PropertySetter<IWorkbookFont, string>(FontNameSetterHelper);
		private static void FontNameSetterHelper(IWorkbookFont instance, string value)
		{
			instance.Name = value;
		}

		public static readonly PropertySetter<IWorkbookFont, ExcelDefaultableBoolean> FontStrikeoutSetter = new PropertySetter<IWorkbookFont, ExcelDefaultableBoolean>(FontStrikeoutSetterHelper);
		private static void FontStrikeoutSetterHelper(IWorkbookFont instance, ExcelDefaultableBoolean value)
		{
			instance.Strikeout = value;
		}

		public static readonly PropertySetter<IWorkbookFont, FontSuperscriptSubscriptStyle> FontSuperscriptSubscriptStyleSetter = new PropertySetter<IWorkbookFont, FontSuperscriptSubscriptStyle>(FontSuperscriptSubscriptStyleSetterHelper);
		private static void FontSuperscriptSubscriptStyleSetterHelper(IWorkbookFont instance, FontSuperscriptSubscriptStyle value)
		{
			instance.SuperscriptSubscriptStyle = value;
		}

		public static readonly PropertySetter<IWorkbookFont, FontUnderlineStyle> FontUnderlineStyleSetter = new PropertySetter<IWorkbookFont, FontUnderlineStyle>(FontUnderlineStyleSetterHelper);
		private static void FontUnderlineStyleSetterHelper(IWorkbookFont instance, FontUnderlineStyle value)
		{
			instance.UnderlineStyle = value;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, HorizontalCellAlignment> CellFormatAlignmentGetter = new PropertyGetter<WorksheetCellFormatData, HorizontalCellAlignment>(CellFormatAlignmentGetterHelper);
		private static HorizontalCellAlignment CellFormatAlignmentGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.Alignment;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatBottomBorderColorInfoGetter = new PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatBottomBorderColorInfoGetterHelper);
		private static WorkbookColorInfo CellFormatBottomBorderColorInfoGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.BottomBorderColorInfo;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatBottomBorderStyleGetter = new PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatBottomBorderStyleGetterHelper);
		private static CellBorderLineStyle CellFormatBottomBorderStyleGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.BottomBorderStyle;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatDiagonalBorderColorInfoGetter = new PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatDiagonalBorderColorInfoGetterHelper);
		private static WorkbookColorInfo CellFormatDiagonalBorderColorInfoGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.DiagonalBorderColorInfo;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, DiagonalBorders> CellFormatDiagonalBordersGetter = new PropertyGetter<WorksheetCellFormatData, DiagonalBorders>(CellFormatDiagonalBordersGetterHelper);
		private static DiagonalBorders CellFormatDiagonalBordersGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.DiagonalBorders;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatDiagonalBorderStyleGetter = new PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatDiagonalBorderStyleGetterHelper);
		private static CellBorderLineStyle CellFormatDiagonalBorderStyleGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.DiagonalBorderStyle;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, CellFill> CellFormatFillGetter = new PropertyGetter<WorksheetCellFormatData, CellFill>(CellFormatFillGetterHelper);
		private static CellFill CellFormatFillGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.Fill;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, WorksheetCellFormatOptions> CellFormatFormatOptionsGetter = new PropertyGetter<WorksheetCellFormatData, WorksheetCellFormatOptions>(CellFormatFormatOptionsGetterHelper);
		private static WorksheetCellFormatOptions CellFormatFormatOptionsGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.FormatOptions;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, string> CellFormatFormatStringGetter = new PropertyGetter<WorksheetCellFormatData, string>(CellFormatFormatStringGetterHelper);
		private static string CellFormatFormatStringGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.FormatString;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, int> CellFormatIndentGetter = new PropertyGetter<WorksheetCellFormatData, int>(CellFormatIndentGetterHelper);
		private static int CellFormatIndentGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.Indent;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatLeftBorderColorInfoGetter = new PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatLeftBorderColorInfoGetterHelper);
		private static WorkbookColorInfo CellFormatLeftBorderColorInfoGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.LeftBorderColorInfo;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatLeftBorderStyleGetter = new PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatLeftBorderStyleGetterHelper);
		private static CellBorderLineStyle CellFormatLeftBorderStyleGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.LeftBorderStyle;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, ExcelDefaultableBoolean> CellFormatLockedGetter = new PropertyGetter<WorksheetCellFormatData, ExcelDefaultableBoolean>(CellFormatLockedGetterHelper);
		private static ExcelDefaultableBoolean CellFormatLockedGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.Locked;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatRightBorderColorInfoGetter = new PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatRightBorderColorInfoGetterHelper);
		private static WorkbookColorInfo CellFormatRightBorderColorInfoGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.RightBorderColorInfo;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatRightBorderStyleGetter = new PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatRightBorderStyleGetterHelper);
		private static CellBorderLineStyle CellFormatRightBorderStyleGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.RightBorderStyle;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, int> CellFormatRotationGetter = new PropertyGetter<WorksheetCellFormatData, int>(CellFormatRotationGetterHelper);
		private static int CellFormatRotationGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.Rotation;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, ExcelDefaultableBoolean> CellFormatShrinkToFitGetter = new PropertyGetter<WorksheetCellFormatData, ExcelDefaultableBoolean>(CellFormatShrinkToFitGetterHelper);
		private static ExcelDefaultableBoolean CellFormatShrinkToFitGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.ShrinkToFit;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, WorkbookStyle> CellFormatStyleGetter = new PropertyGetter<WorksheetCellFormatData, WorkbookStyle>(CellFormatStyleGetterHelper);
		private static WorkbookStyle CellFormatStyleGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.Style;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatTopBorderColorInfoGetter = new PropertyGetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatTopBorderColorInfoGetterHelper);
		private static WorkbookColorInfo CellFormatTopBorderColorInfoGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.TopBorderColorInfo;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatTopBorderStyleGetter = new PropertyGetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatTopBorderStyleGetterHelper);
		private static CellBorderLineStyle CellFormatTopBorderStyleGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.TopBorderStyle;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, VerticalCellAlignment> CellFormatVerticalAlignmentGetter = new PropertyGetter<WorksheetCellFormatData, VerticalCellAlignment>(CellFormatVerticalAlignmentGetterHelper);
		private static VerticalCellAlignment CellFormatVerticalAlignmentGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.VerticalAlignment;
		}

		public static readonly PropertyGetter<WorksheetCellFormatData, ExcelDefaultableBoolean> CellFormatWrapTextGetter = new PropertyGetter<WorksheetCellFormatData, ExcelDefaultableBoolean>(CellFormatWrapTextGetterHelper);
		private static ExcelDefaultableBoolean CellFormatWrapTextGetterHelper(WorksheetCellFormatData instance)
		{
			return instance.WrapText;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, HorizontalCellAlignment> CellFormatAlignmentSetter = new PropertySetter<WorksheetCellFormatData, HorizontalCellAlignment>(CellFormatAlignmentSetterHelper);
		private static void CellFormatAlignmentSetterHelper(WorksheetCellFormatData instance, HorizontalCellAlignment value)
		{
			instance.Alignment = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatBottomBorderColorInfoSetter = new PropertySetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatBottomBorderColorInfoSetterHelper);
		private static void CellFormatBottomBorderColorInfoSetterHelper(WorksheetCellFormatData instance, WorkbookColorInfo value)
		{
			instance.BottomBorderColorInfo = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatBottomBorderStyleSetter = new PropertySetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatBottomBorderStyleSetterHelper);
		private static void CellFormatBottomBorderStyleSetterHelper(WorksheetCellFormatData instance, CellBorderLineStyle value)
		{
			instance.BottomBorderStyle = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatDiagonalBorderColorInfoSetter = new PropertySetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatDiagonalBorderColorInfoSetterHelper);
		private static void CellFormatDiagonalBorderColorInfoSetterHelper(WorksheetCellFormatData instance, WorkbookColorInfo value)
		{
			instance.DiagonalBorderColorInfo = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, DiagonalBorders> CellFormatDiagonalBordersSetter = new PropertySetter<WorksheetCellFormatData, DiagonalBorders>(CellFormatDiagonalBordersSetterHelper);
		private static void CellFormatDiagonalBordersSetterHelper(WorksheetCellFormatData instance, DiagonalBorders value)
		{
			instance.DiagonalBorders = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatDiagonalBorderStyleSetter = new PropertySetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatDiagonalBorderStyleSetterHelper);
		private static void CellFormatDiagonalBorderStyleSetterHelper(WorksheetCellFormatData instance, CellBorderLineStyle value)
		{
			instance.DiagonalBorderStyle = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, CellFill> CellFormatFillSetter = new PropertySetter<WorksheetCellFormatData, CellFill>(CellFormatFillSetterHelper);
		private static void CellFormatFillSetterHelper(WorksheetCellFormatData instance, CellFill value)
		{
			instance.Fill = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, WorksheetCellFormatOptions> CellFormatFormatOptionsSetter = new PropertySetter<WorksheetCellFormatData, WorksheetCellFormatOptions>(CellFormatFormatOptionsSetterHelper);
		private static void CellFormatFormatOptionsSetterHelper(WorksheetCellFormatData instance, WorksheetCellFormatOptions value)
		{
			instance.FormatOptions = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, string> CellFormatFormatStringSetter = new PropertySetter<WorksheetCellFormatData, string>(CellFormatFormatStringSetterHelper);
		private static void CellFormatFormatStringSetterHelper(WorksheetCellFormatData instance, string value)
		{
			instance.FormatString = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, int> CellFormatIndentSetter = new PropertySetter<WorksheetCellFormatData, int>(CellFormatIndentSetterHelper);
		private static void CellFormatIndentSetterHelper(WorksheetCellFormatData instance, int value)
		{
			instance.Indent = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatLeftBorderColorInfoSetter = new PropertySetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatLeftBorderColorInfoSetterHelper);
		private static void CellFormatLeftBorderColorInfoSetterHelper(WorksheetCellFormatData instance, WorkbookColorInfo value)
		{
			instance.LeftBorderColorInfo = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatLeftBorderStyleSetter = new PropertySetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatLeftBorderStyleSetterHelper);
		private static void CellFormatLeftBorderStyleSetterHelper(WorksheetCellFormatData instance, CellBorderLineStyle value)
		{
			instance.LeftBorderStyle = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, ExcelDefaultableBoolean> CellFormatLockedSetter = new PropertySetter<WorksheetCellFormatData, ExcelDefaultableBoolean>(CellFormatLockedSetterHelper);
		private static void CellFormatLockedSetterHelper(WorksheetCellFormatData instance, ExcelDefaultableBoolean value)
		{
			instance.Locked = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatRightBorderColorInfoSetter = new PropertySetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatRightBorderColorInfoSetterHelper);
		private static void CellFormatRightBorderColorInfoSetterHelper(WorksheetCellFormatData instance, WorkbookColorInfo value)
		{
			instance.RightBorderColorInfo = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatRightBorderStyleSetter = new PropertySetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatRightBorderStyleSetterHelper);
		private static void CellFormatRightBorderStyleSetterHelper(WorksheetCellFormatData instance, CellBorderLineStyle value)
		{
			instance.RightBorderStyle = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, int> CellFormatRotationSetter = new PropertySetter<WorksheetCellFormatData, int>(CellFormatRotationSetterHelper);
		private static void CellFormatRotationSetterHelper(WorksheetCellFormatData instance, int value)
		{
			instance.Rotation = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, ExcelDefaultableBoolean> CellFormatShrinkToFitSetter = new PropertySetter<WorksheetCellFormatData, ExcelDefaultableBoolean>(CellFormatShrinkToFitSetterHelper);
		private static void CellFormatShrinkToFitSetterHelper(WorksheetCellFormatData instance, ExcelDefaultableBoolean value)
		{
			instance.ShrinkToFit = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, WorkbookStyle> CellFormatStyleSetter = new PropertySetter<WorksheetCellFormatData, WorkbookStyle>(CellFormatStyleSetterHelper);
		private static void CellFormatStyleSetterHelper(WorksheetCellFormatData instance, WorkbookStyle value)
		{
			instance.Style = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, WorkbookColorInfo> CellFormatTopBorderColorInfoSetter = new PropertySetter<WorksheetCellFormatData, WorkbookColorInfo>(CellFormatTopBorderColorInfoSetterHelper);
		private static void CellFormatTopBorderColorInfoSetterHelper(WorksheetCellFormatData instance, WorkbookColorInfo value)
		{
			instance.TopBorderColorInfo = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, CellBorderLineStyle> CellFormatTopBorderStyleSetter = new PropertySetter<WorksheetCellFormatData, CellBorderLineStyle>(CellFormatTopBorderStyleSetterHelper);
		private static void CellFormatTopBorderStyleSetterHelper(WorksheetCellFormatData instance, CellBorderLineStyle value)
		{
			instance.TopBorderStyle = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, VerticalCellAlignment> CellFormatVerticalAlignmentSetter = new PropertySetter<WorksheetCellFormatData, VerticalCellAlignment>(CellFormatVerticalAlignmentSetterHelper);
		private static void CellFormatVerticalAlignmentSetterHelper(WorksheetCellFormatData instance, VerticalCellAlignment value)
		{
			instance.VerticalAlignment = value;
		}

		public static readonly PropertySetter<WorksheetCellFormatData, ExcelDefaultableBoolean> CellFormatWrapTextSetter = new PropertySetter<WorksheetCellFormatData, ExcelDefaultableBoolean>(CellFormatWrapTextSetterHelper);
		private static void CellFormatWrapTextSetterHelper(WorksheetCellFormatData instance, ExcelDefaultableBoolean value)
		{
			instance.WrapText = value;
		}

		#endregion // Property Accessor Delegates

		// MD 1/12/12 - TFS99279
		#region BinarySearch

		public static int BinarySearch<T>(IList<T> list, T value, IComparer<T> comparer)
		{
			int start = 0;
			int end = list.Count - 1;

			while (start <= end)
			{
				int mid = start + ((end - start) / 2);

				T testValue = list[mid];

				int compareResult = comparer.Compare(testValue, value);

				if (compareResult == 0)
					return mid;

				if (compareResult < 0)
					start = mid + 1;
				else
					end = mid - 1;
			}

			return ~start;
		}

		#endregion // BinarySearch

		// MD 7/2/12 - TFS115692
		#region GetAbsoluteShapeBounds

		public static Rectangle GetAbsoluteShapeBounds(Rectangle relativeShapeBounds, Rectangle groupChildrenBounds, Rectangle absoluteGroupBounds, 
			WorkbookFormat format, float childRotation)
		{
			// MD 7/24/12 - TFS115693
			// In the Excel 2007 format, we need to account for rotation.
			#region Old Code

			//double horizontalTransform = absoluteGroupBounds.Width / (double)groupChildrenBounds.Width;
			//double verticalTransform = absoluteGroupBounds.Height / (double)groupChildrenBounds.Height;

			//double xOffsetInGroup = (relativeShapeBounds.X - groupChildrenBounds.X) * horizontalTransform;
			//double yOffsetInGroup = (relativeShapeBounds.Y - groupChildrenBounds.Y) * verticalTransform;

			//int x = (int)MathUtilities.MidpointRoundingAwayFromZero(absoluteGroupBounds.X + xOffsetInGroup);
			//int y = (int)MathUtilities.MidpointRoundingAwayFromZero(absoluteGroupBounds.Y + yOffsetInGroup);
			//int width = (int)MathUtilities.MidpointRoundingAwayFromZero(horizontalTransform * relativeShapeBounds.Width);
			//int height = (int)MathUtilities.MidpointRoundingAwayFromZero(verticalTransform * relativeShapeBounds.Height);

			//return new Rectangle(x, y, width, height);

			#endregion // Old Code
			double horizontalTransform = absoluteGroupBounds.Width / (double)groupChildrenBounds.Width;
			double verticalTransform = absoluteGroupBounds.Height / (double)groupChildrenBounds.Height;

			double xOffsetInGroup = (relativeShapeBounds.X - groupChildrenBounds.X) * horizontalTransform;
			double yOffsetInGroup = (relativeShapeBounds.Y - groupChildrenBounds.Y) * verticalTransform;
			int x = (int)MathUtilities.MidpointRoundingAwayFromZero(absoluteGroupBounds.X + xOffsetInGroup);
			int y = (int)MathUtilities.MidpointRoundingAwayFromZero(absoluteGroupBounds.Y + yOffsetInGroup);
			int width = (int)MathUtilities.MidpointRoundingAwayFromZero(horizontalTransform * relativeShapeBounds.Width);
			int height = (int)MathUtilities.MidpointRoundingAwayFromZero(verticalTransform * relativeShapeBounds.Height);

			childRotation %= 360;
			if (childRotation < 0)
				childRotation += 360;

			if (Utilities.Is2003Format(format) ||
				(childRotation < 45) ||
				(childRotation > 135 && childRotation < 215) ||
				(childRotation > 315))
			{
				return new Rectangle(x, y, width, height);
			}
			else
			{
				// Determine the actual width of the shapes, which will use the opposite scaling orientation. Then adjust the position
				// based on what the size would have been if it scaled normally. Assume the size change occurred in about the center of 
				// the shape.
				int actualWidth = (int)MathUtilities.MidpointRoundingAwayFromZero(verticalTransform * relativeShapeBounds.Width);
				int actualHeight = (int)MathUtilities.MidpointRoundingAwayFromZero(horizontalTransform * relativeShapeBounds.Height);
				int xOffset = (width - actualWidth) / 2;
				int yOffset = (height - actualHeight) / 2;
				return new Rectangle(x + xOffset, y + yOffset, actualWidth, actualHeight);
			}
		}

		#endregion // GetAbsoluteShapeBounds

		// MD 7/23/12 - TFS117429
		#region ApplyShade

		public static Color ApplyShade(Color value, double shade)
		{
			double rLinear = Utilities.RGBToLinearRGB(value.R / 255d);
			double gLinear = Utilities.RGBToLinearRGB(value.G / 255d);
			double bLinear = Utilities.RGBToLinearRGB(value.B / 255d);

			double rLinearShaded = rLinear * shade < 0 ? 0 : (rLinear * shade > 1 ? 1 : rLinear * shade);
			double gLinearShaded = gLinear * shade < 0 ? 0 : (gLinear * shade > 1 ? 1 : gLinear * shade);
			double bLinearShaded = bLinear * shade < 0 ? 0 : (bLinear * shade > 1 ? 1 : bLinear * shade);

			double rShaded = Utilities.LinearRGBToRGB(rLinearShaded);
			double gShaded = Utilities.LinearRGBToRGB(gLinearShaded);
			double bShaded = Utilities.LinearRGBToRGB(bLinearShaded);

			return Color.FromArgb(255,
				(byte)MathUtilities.MidpointRoundingAwayFromZero(rShaded * 255),
				(byte)MathUtilities.MidpointRoundingAwayFromZero(gShaded * 255),
				(byte)MathUtilities.MidpointRoundingAwayFromZero(bShaded * 255));
		}

		private static double RGBToLinearRGB(double value)
		{
			if (value < 0)
				return 0;

			if (value <= 0.04045)
				return value / 12.92;

			if (value <= 1)
				return Math.Pow(((value + 0.055) / 1.055), 2.4);

			return 1;
		}

		private static double LinearRGBToRGB(double value)
		{
			if (value < 0)
				return 0;

			if (value <= 0.0031308)
				return value * 12.92;

			if (value < 1)
				return 1.055 * (Math.Pow(value, (1 / 2.4))) - 0.055;

			return 1;
		}

		#endregion // ApplyShade
	}

	#region ExcelTuple<T1,T2> class

	internal class ExcelTuple<T1,T2>
	{
		private T1 item1;
		private T2 item2;

		public ExcelTuple(T1 item1, T2 item2)
		{
			this.item1 = item1;
			this.item2 = item2;
		}

		public override int GetHashCode()
		{
			return
				this.item1.GetHashCode() ^
				this.item2.GetHashCode() << 1;
		}

		public override bool Equals(object obj)
		{
			ExcelTuple<T1, T2> other = obj as ExcelTuple<T1, T2>;
			if (other == null)
				return false;

			return
				this.item1.Equals(other.item1) &&
				this.item2.Equals(other.item2);
		}

		public T1 Item1
		{
			get { return this.item1; }
		}

		public T2 Item2
		{
			get { return this.item2; }
		}
	}

	#endregion // ExcelTuple<T1,T2> class

	#region ExcelTuple<T1,T2,T3> class

	internal class ExcelTuple<T1, T2, T3>
	{
		private T1 item1;
		private T2 item2;
		private T3 item3;

		public ExcelTuple(T1 item1, T2 item2, T3 item3)
		{
			this.item1 = item1;
			this.item2 = item2;
			this.item3 = item3;
		}

		public override int GetHashCode()
		{
			return
				this.item1.GetHashCode() ^
				this.item2.GetHashCode() << 1 ^
				this.item3.GetHashCode() << 2;
		}

		public override bool Equals(object obj)
		{
			ExcelTuple<T1, T2, T3> other = obj as ExcelTuple<T1, T2, T3>;
			if (other == null)
				return false;

			return
				this.item1.Equals(other.item1) &&
				this.item2.Equals(other.item2) &&
				this.item3.Equals(other.item3);
		}

		public T1 Item1
		{
			get { return this.item1; }
		}

		public T2 Item2
		{
			get { return this.item2; }
		}

		public T3 Item3
		{
			get { return this.item3; }
		}
	}

	#endregion // ExcelTuple<T1,T2> class
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