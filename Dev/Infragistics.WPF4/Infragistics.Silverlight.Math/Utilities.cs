using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace Infragistics.Math
{





	// AS 3/22/11
	// This class used to be internal (and looks like it was meant to be) but 
    //public static class Utilities
    internal static class Utilities
	{
		#region Methods

		#region ArrayEquals



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public static bool ArrayEquals(Array array1, Array array2)
		{
			if (array1.Length != array2.Length)
				return false;

			for (int i = 0; i < array1.Length; i++)
			{
				if (!object.Equals(array1.GetValue(i), array2.GetValue(i)))
					return false;
			}

			return true;
		}

		#endregion //ArrayEquals

		#region ArrayEqualsExceptLastElement



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public static bool ArrayEqualsExceptLastElement(int[] x1, int[] x2)
		{
			if (x1.Length != x2.Length)
				return false;

			if (x1.Length < 2)
				return true;

			for (int i = 0, length = x1.Length - 1; i < length; i++)
			{
				if (x1[i] != x2[i])
					return false;
			}

			return true;
		}

		#endregion //ArrayEqualsExceptLastElement

		#region Copy



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		public static void Copy<T>(IList<T> source, IList<T> destination)
		{
			Exception exception = null;
			if(!TryCopy<T>(source, destination, out exception))
			{
				throw exception;
			}
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public static bool TryCopy<T>(IList<T> source, IList<T> destination, out Exception exception)
		{
			exception = null;
			try
			{
				Copy<T>(source, 0, destination, 0);
				return true;
			}
			catch (Exception ex)
			{
				exception = ex;
				Utilities.DebugFail("An exception occurred");
				return false;
			}
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		public static void Copy<T>(IList<T> source, int sourceIndex, IList<T> destination, int destinationIndex)
		{
			Exception exception = null;
			if(!TryCopy<T>(source, sourceIndex, destination, destinationIndex, out exception))
			{
				throw exception;
			}
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		public static bool TryCopy<T>(IList<T> source, int sourceIndex, IList<T> destination, int destinationIndex, out Exception exception)
		{
			exception = null;
			try
			{
				if (destination.Count - destinationIndex < source.Count - sourceIndex)
				{
					exception = new ArgumentException(Compute.GetString("LE_ArgumentException_68"));
					return false;
				}

				for (int i = 0; i < source.Count; i++)
				{
					destination[destinationIndex + i] = source[sourceIndex + i];
				}

				return true;
			}
			catch (Exception ex)
			{
				exception = ex;
				Utilities.DebugFail("An exception occurred");
				return false;
			}
		}

		#endregion //Copy

		#region Clone



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public static IList<T> Clone<T>(IList<T> x)
		{
			if (x == null)
				return null;

			IList<T> result = new T[x.Count];
			for (int i = 0; i < result.Count; i++)
			{
				result[i] = x[i];
			}
			return result;
		}

		#endregion //Clone

		#region CumProduct







		public static int[] CumProduct(int[] x)
		{
			if (x == null)
				return null;

			int[] cumproduct = new int[x.Length];
			cumproduct[0] = x[0];

			for (int i = 1; i < x.Length; i++)
			{
				cumproduct[i] = x[i] * cumproduct[i - 1];
			}
			return cumproduct;
		}

		#endregion //CumProduct

		#region DebugFail

		public static void DebugFail(string message)
		{



			Utilities.DebugFail(message); 

		}

		#endregion //DebugFail

		#region GetArrayDimensions

		/// <summary>
		/// Returns the dimensions of an array as an integer array.
		/// </summary>
		/// <param name="x">An array.</param>
		/// <returns>The dimensions of <paramref name="x"/>.</returns>
		public static int[] GetArrayDimensions(Array x)
		{
			int N = x.Rank;
			if (N == 1)
				return new int[] { 1, x.Length };

			int[] newDimensions = new int[N];
			for (int i = 0; i < N; i++)
			{
				newDimensions[i] = x.GetLength(i);
			}

			return newDimensions;
		}

		#endregion //GetArrayDimensions

		#region IncrementSubscript



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		public static void IncrementSubscript(ref int[] subscript, int[] dimensions)
		{
			for (int i = 0; i < dimensions.Length; i++)
			{
				if (subscript[i] < dimensions[i] - 1)
				{
					subscript[i]++;
					break;
				}
				else
				{
					subscript[i] = 0;
				}
			}
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public static void IncrementSubscript(ref int[] subscript, int N, int[] dimensions)
		{
			N--;
			for (int i = 0, length = dimensions.Length; i < length; i++)
			{
				if (i == N)
				{
					i++;
					if (i >= length)
						break;
				}

				if (subscript[i] < dimensions[i] - 1)
				{
					subscript[i]++;
					break;
				}
				else
				{
					subscript[i] = 0;
				}
			}
		}

		#endregion //IncrementMatrixIndex

		#region IsZero







		public static bool InZero(int[] x)
		{
			if (x.Length == 0)
				return false;

			return Utilities.ArrayEquals(x, new int[x.Length]);
		}

		#endregion //IsZero

		#region Max



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static int Max(int[] x)
		{
			if (x == null)
				Utilities.DebugFail("x is null.");

			int curr;
			int max = x[0];
			for (int i = 1; i < x.Length; i++)
			{
				curr = x[i];
				if (x[i] > max)
				{
					max = curr;
				}
			}
			return max;
		}

		#endregion //Max

		#region Product

		/// <summary>
		/// Returns the product of the elements of an integer array.
		/// </summary>
		/// <param name="x">An integer array.</param>
		/// <returns>The product of an integer array.</returns>
		internal static int Product(int[] x)
		{
			int result = 1;
			for (int i = 0; i < x.Length; i++)
			{
				result *= x[i];
			}

			return result;
		}

		#endregion //Product

		#region TrySizeToDimensions



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		public static bool TrySizeToDimensions(Vector size, out int[] newDimensions, out Exception exception)
		{
			int length = size.Length;
			exception = null;
			newDimensions = new int[length];

			if (size.IsEmpty() || size.IsUnitary() || size.IsColumn())
			{
				exception = new ArgumentException("Invalid size Vector.");
				return false;
			}

			int curr;
			double currCheck;
			for (int i = 0; i < length; i++)
			{ 
				currCheck = size.Elements[i];
				curr = (int)Compute.Round(currCheck);
				if (currCheck != curr)
				{
					exception = new ArgumentException("Invalid size Vector.");
					return false;
				}

				newDimensions[i] = curr;
			}

			return true;
		}

		#endregion //TrySizeToDimensions

		#endregion //Methods
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