using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;






using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine

{
	internal static class MatrixUtilities
	{
		#region CalculateCofactor

		public static double CalculateCofactor( double[ , ] matrix, int n, int x, int y )
		{
			double[ , ] minorMatrix = MatrixUtilities.CreateMinorMatrix( matrix, n, x, y );
			double minor = MatrixUtilities.CalculateDeterminant( minorMatrix, n - 1 );

			return Math.Pow( -1, y + x ) * minor;
		}

		#endregion CalculateCofactor

		#region CalculateDeterminant

		public static double CalculateDeterminant( double[ , ] matrix, int n )
		{
			if ( n == 1 )
				return matrix[ 0, 0 ];

			double total = 0;

			
			for ( int x = 0; x < n; x++ )
			{
				double cofactor = MatrixUtilities.CalculateCofactor( matrix, n, x, 0 );

				total += matrix[ x, 0 ] * cofactor;
			}

			return total;
		} 

		#endregion CalculateDeterminant

		#region CalculateInverse

		public static double[ , ] CalculateInverse( double[ , ] matrix, int n, out ExcelCalcErrorValue error )
		{
			error = null;

			double determinant = MatrixUtilities.CalculateDeterminant( matrix, n );

			if ( determinant == 0 )
			{
				error = ErrorValue.ValueRangeOverflow.ToCalcErrorValue();
				return null;
			}

			double oneOverDeterminant = 1 / determinant;
			double[ , ] transposedMatrix = MatrixUtilities.TransposeMatrix( matrix );
			double[ , ] inverseMatrix = new double[ n, n ];

			
			for ( int x = 0; x < n; x++ )
			{
				for ( int y = 0; y < n; y++ )
					inverseMatrix[ x, y ] = oneOverDeterminant * MatrixUtilities.CalculateCofactor( transposedMatrix, n, x, y );
			}

			return inverseMatrix;
		}

		#endregion CalculateInverse

		#region ConvertArrayToMatrix

		// MD 12/1/11 - TFS96113
		// Instead of storing arrays for rectangular regions of values we now use an ArrayProxy.
		//public static double[ , ] ConvertArrayToMatrix( ExcelCalcValue[ , ] array, NonNumericElementBehavior nonNumericElementBehavior, out ExcelCalcErrorValue error )
		public static double[,] ConvertArrayToMatrix(ArrayProxy array, NonNumericElementBehavior nonNumericElementBehavior, out ExcelCalcErrorValue error)
		{
			error = null;

			int arrayColumns = array.GetLength( 0 );
			int arrayRows = array.GetLength( 1 );

			double[ , ] convertedArray = new double[ arrayColumns, arrayRows ];
			for ( int row = 0; row < arrayRows; row++ )
			{
				for ( int column = 0; column < arrayColumns; column++ )
				{
					ExcelCalcValue currentValue = array[ column, row ];

					if ( currentValue.IsError )
					{
						error = currentValue.ToErrorValue();
						return null;
					}

					if ( currentValue.IsString || currentValue.IsBoolean )
					{
						switch ( nonNumericElementBehavior )
						{
							case NonNumericElementBehavior.CausesError:
								error = new ExcelCalcErrorValue( ExcelCalcErrorCode.Value );
								return null;

							case NonNumericElementBehavior.TreatAsZero:
								convertedArray[ column, row ] = 0;
								break;

							case NonNumericElementBehavior.Ignore:
								convertedArray[ column, row ] = Double.NaN;
								break;

							default:
								Utilities.DebugFail( "Unknown NonNumericElementBehavior value." );
								break;
						}
					}
					else
					{
						convertedArray[ column, row ] = currentValue.ToDouble();
					}
				}
			}

			return convertedArray;
		}

		#endregion ConvertArrayToMatrix

		#region CreateMinorMatrix

		private static double[ , ] CreateMinorMatrix( double[ , ] matrix, int n, int sourceX, int sourceY )
		{
			double[ , ] minorMatrix = new double[ n - 1, n - 1 ];

			for ( int x = 0, xMinor = 0; x < n; x++, xMinor++ )
			{
				// Skip the j-th column
				if ( x == sourceX )
				{
					xMinor--;
					continue;
				}

				for ( int y = 0, yMinor = 0; y < n; y++, yMinor++ )
				{
					// Skip the i-th row
					if ( y == sourceY )
					{
						yMinor--;
						continue;
					}

					minorMatrix[ xMinor, yMinor ] = matrix[ x, y ];
				}
			}

			return minorMatrix;
		}

		#endregion CreateMinorMatrix

		#region TransposeMatrix

		public static T[ , ] TransposeMatrix<T>( T[ , ] matrix )
		{
			int maxX = matrix.GetLength( 0 );
			int maxY = matrix.GetLength( 1 );

			T[ , ] transposedMatrix = new T[ maxY, maxX ];

			for ( int x = 0; x < maxX; x++ )
			{
				for ( int y = 0; y < maxY; y++ )
					transposedMatrix[ y, x ] = matrix[ x, y ];
			}

			return transposedMatrix;
		}

		// MD 12/1/11 - TFS96113
		public static ExcelCalcValue[,] TransposeMatrix(ArrayProxy matrix)
		{
			int maxX = matrix.GetLength(0);
			int maxY = matrix.GetLength(1);

			ExcelCalcValue[,] transposedMatrix = new ExcelCalcValue[maxY, maxX];

			for (int x = 0; x < maxX; x++)
			{
				for (int y = 0; y < maxY; y++)
					transposedMatrix[y, x] = matrix[x, y];
			}

			return transposedMatrix;
		} 

		#endregion TransposeMatrix


		#region NonNumericElementBehavior enum

		public enum NonNumericElementBehavior
		{
			CausesError,
			TreatAsZero,
			Ignore
		} 

		#endregion NonNumericElementBehavior enum
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