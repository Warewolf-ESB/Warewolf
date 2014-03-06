using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections;

namespace Infragistics.Math
{
	/// <summary>
	/// A ComplexMatrix is a list of <see cref="Complex"/> numbers with an additional dimensionality property that 
	/// specifies its spatial orientation.
	/// </summary>
    public class ComplexMatrix : MatrixBase
			, IEnumerable<Complex>

, ICloneable

	{
		#region Member Variables

		private IList<Complex> elements;

		#endregion //Member Variables

		#region Static Variables

		#region Empty

		/// <summary>
		/// Returns the empty <see cref="ComplexMatrix"/>.
		/// </summary>
		public static ComplexMatrix Empty = new ComplexMatrix();

		#endregion //Empty

		#endregion //Static Variables

		#region Constructors

		/// <summary>
		/// Initializes an empty <see cref="ComplexMatrix"/> instance.
		/// </summary>
		/// <seealso cref="MatrixBase.IsEmpty"/>
		public ComplexMatrix()
			: base()
		{
		}

		/// <summary>
		/// Initializes a one-dimensional <see cref="ComplexMatrix"/> of a specified <paramref name="length"/>. 
		/// </summary>
		/// <param name="length">The length of the constructed ComplexMatrix.</param>
		public ComplexMatrix(int length)
			: base(length)
		{
			this.elements = new Complex[length];
		}

		/// <summary>
		/// Initializes a zero <see cref="ComplexMatrix"/> with the specified <paramref name="dimensions"/>. 
		/// </summary>
		/// <param name="dimensions">The dimensions of the constructed ComplexMatrix.</param>
		public ComplexMatrix(int[] dimensions)
			: base(dimensions)
		{
			this.elements = new Complex[this.Length];
		}

		/// <summary>
		/// Initializes a constant <see cref="ComplexMatrix"/> with the specified <paramref name="dimensions"/>. 
		/// </summary>
		/// <param name="value">The constant value assigned to each element of the ComplexMatrix.</param>
		/// <param name="dimensions">The dimensions of the constructed ComplexMatrix.</param>
		public ComplexMatrix(Complex value, int[] dimensions)
			: base(dimensions)
		{
			this.elements = new Complex[this.Length];
			for (int i = 0; i < this.Length; i++)
			{
				this.elements[i] = value;
			}
		}

		/// <summary>
		/// Initializes a one-dimensional <see cref="ComplexMatrix"/> of the specified <paramref name="length"/> and the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value assigned to each element of the constructed ComplexMatrix.</param>
		/// <param name="length">The length of the constructed ComplexMatrix.</param>
		public ComplexMatrix(Complex value, int length)
			: base(length)
		{
			this.elements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				this.elements[i] = value;
			}
		}

		/// <summary>
		/// Initializes a <see cref="ComplexMatrix"/> with the specified <paramref name="elements"/> and <paramref name="dimensions"/>.
		/// </summary>
		/// <param name="elements">The elements of the constructed ComplexMatrix.</param>
		/// <param name="dimensions">The dimensions of the constructed ComplexMatrix.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the dimensions specify a size that is inconsistent with the length of the elements.
		/// </exception>
		public ComplexMatrix(IList<Complex> elements, int[] dimensions)
			: base(dimensions)
		{
			if (elements.Count != this.Length)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_2"));

			this.elements = elements;
		}

		/// <summary>
		/// Initializes a <see cref="ComplexMatrix"/> by copying the elements and dimensions of a multi-dimensional 
		/// <see cref="Complex"/> array.
		/// </summary>
		/// <param name="x">A multi-dimensional Complex array.</param>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not a Complex array.
		/// </exception>
		public ComplexMatrix(Array x)
			: base(Utilities.GetArrayDimensions(x))
		{
			if (x.GetType().GetElementType() != typeof(Complex))
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_11"), "x");

			// MD 4/7/11 - TFS71039
			// The way the .NET Framework iterates multidimensional arrays is not the same as how we index into the elements 
			// collection. Use the IndexToSubscript method to make sure the indexing is consistent with how the element 
			// collection is accessed.
			//int i = 0;
			//this.elements = new Complex[x.Length];
			//foreach (Complex xi in x)
			//{
			//    this.elements[i++] = xi;
			//}
			this.elements = new Complex[x.Length];
			Vector size = this.Size;
			for (int i = 0; i < x.Length; i++)
			{
				int[] subscript = Compute.IndexToSubscript(i, size);

				if (subscript.Length == 2 && x.Rank == 1)
					this.elements[i] = (Complex)x.GetValue(subscript[1]);
				else
					this.elements[i] = (Complex)x.GetValue(subscript);
			}
		}

		#endregion //Constructors

		#region Base Class Overrides

		#region CopyTo

		/// <summary>
		/// Copies the base <see cref="ComplexMatrix"/> to an <paramref name="array"/> starting at a 
		/// particular <paramref name="index"/>.
		/// </summary>
		/// <param name="array">The destination array.</param>
		/// <param name="index">An index into the destination array where the copying begins.</param>
		protected override void CopyTo(Array array, int index)
		{
			if (this.IsEmpty())
				return;

			((ICollection)this.elements).CopyTo(array, index);
		}

		/// <summary>
		/// Copies the base <see cref="ComplexMatrix"/> to an <paramref name="array"/> starting at a 
		/// particular <paramref name="index"/>.
		/// </summary>
		/// <param name="array">The destination array.</param>
		/// <param name="index">An index into the destination array where the copying begins.</param>
		public void CopyTo(Complex[] array, int index)
		{
			this.CopyTo((Array)array, index);
		}

		#endregion //CopyTo

		#region Equals

		/// <summary>
		/// Compares the <see cref="ComplexMatrix"/> with <paramref name="x"/> for equality.
		/// </summary>
		/// <param name="x">An object.</param>
		/// <returns>Returns True if the ComplexMatrix is equal to x; False otherwise.</returns>
		public override bool Equals(object x)
		{
			ComplexMatrix xCast = x as ComplexMatrix;
			if (object.Equals(x, null))
				return false;

			return this == xCast;
		}

		#endregion Equals

		#region GetHashCode

		/// <summary>
		/// Returns hash code for the <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <returns>A hash code.</returns>
		public override int GetHashCode()
		{
			int length = this.Length;
			if (length == 0)
				return 0;

			int middleIndex = length / 2;

			return length
				^ this.elements[0].GetHashCode()
				^ this.elements[middleIndex].GetHashCode()
				^ this.elements[length - 1].GetHashCode()
				^ this.Dimensions.GetHashCode();
		}

		#endregion GetHashCode

		#region ToString

		/// <summary>
		/// Returns the string representation of a <see cref="ComplexMatrix"/>.
		/// </summary>
		public override string ToString()
		{
			if (this.IsEmpty())
				return "\n( )\n";

			if (this.IsUnitary())
				return "\n( " + this.elements[0].ToString() + " )\n";

			if (this.IsTwoDimensional())
				return this.ToString_Helper(new int[0]) + "\n";

			int curr;
			int length = 1;
			int subscriptLength = this.Rank - 2;
			int[] subscriptDimensions = new int[subscriptLength];
			for (int i = 0; i < subscriptLength; i++)
			{
				curr = this.Dimensions[i + 2];
				subscriptDimensions[i] = curr;
				length *= curr;
			}

			int[] subscript = new int[subscriptLength];
			StringBuilder result = new StringBuilder(4 * this.Length);
			for (int i = 0; i < length; i++)
			{
				result.Append(this.ToString_Helper(subscript));
				result.Append(ComplexMatrix.ToString_HelperSubscript(subscript));

				Utilities.IncrementSubscript(ref subscript, subscriptDimensions);
			}

			result.Append("\n\n");
			return result.ToString();
		}




#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private string ToString_Helper(params int[] subscript)
		{
			int[] fullSubscript = new int[this.Rank];
			if (this.Rank > 2)
				subscript.CopyTo(fullSubscript, 2);

			StringBuilder result = new StringBuilder(4 * this.Dimensions[0] * this.Dimensions[1]);
			result.Append("\n\n( ");

			for (int i = 0, iLength = this.Dimensions[0], jLength = this.Dimensions[1]; i < iLength; i++)
			{
				if (i > 0)
					result.Append("  ");

				fullSubscript[0] = i;
				for (int j = 0; j < jLength; j++)
				{
					fullSubscript[1] = j;
					result.Append(this[fullSubscript]);
					if (j < jLength - 1)
						result.Append(", ");
					else
						result.Append(" ");
				}
				if (i < iLength - 1)
					result.Append("\n");
			}

			result.Append(")\n");
			return result.ToString();
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static string ToString_HelperSubscript(int[] subscript)
		{
			StringBuilder result = new StringBuilder(subscript.Length);
			result.Append("[:,:,");
			for (int i = 0, length = subscript.Length; i < length; i++)
			{
				result.Append(subscript[i].ToString());

				if (i < length - 1)
					result.Append(",");
			}

			result.Append("]");
			return result.ToString();
		}

		#endregion //ToString

		#endregion //Base Class Overrides

		#region Interfaces

		#region IEnumerable<double> Members

		/// <summary>
		/// Returns an double enumerator for the <see cref="ComplexMatrix"/>.
		/// </summary>
		IEnumerator<Complex> IEnumerable<Complex>.GetEnumerator()
		{
			return ((IEnumerable<Complex>)this.elements).GetEnumerator();
		}

		#endregion //IEnumerable<double> Members

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the <see cref="ComplexMatrix"/>.
		/// </summary>
		public override IEnumerator GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		#endregion //IEnumerable Members

		#region ICloneable Members


		/// <summary>
		/// Returns a copy of the <see cref="ComplexMatrix"/>.
		/// </summary>
		object ICloneable.Clone()
		{
			return this.Clone();
		}


		#endregion //ICloneable Members

		#endregion //Interfaces

		#region Operators

		#region +

		/// <summary>
		/// Adds two <see cref="ComplexMatrix"/> instances pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] + x2[i].</returns>
		public static ComplexMatrix operator +(ComplexMatrix x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] + x2[i].</returns>
		public static ComplexMatrix operator +(ComplexMatrix x1, Matrix x2)
		{
			return ComplexMatrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] + x2[i].</returns>
		public static ComplexMatrix operator +(Matrix x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="ComplexMatrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] + x2.</returns>
		public static ComplexMatrix operator +(ComplexMatrix x1, double x2)
		{
			return ComplexMatrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a double and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1 + x2[i].</returns>
		public static ComplexMatrix operator +(double x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] + x2.</returns>
		public static ComplexMatrix operator +(ComplexMatrix x1, Complex x2)
		{
			return ComplexMatrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="Complex"/> number and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1 + x2[i].</returns>
		public static ComplexMatrix operator +(Complex x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Add(x1, x2);
		}

		#endregion // +

		#region -

		/// <summary>
		/// Subtracts two <see cref="ComplexMatrix"/> instances pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] - x2[i].</returns>
		public static ComplexMatrix operator -(ComplexMatrix x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] - x2[i].</returns>
		public static ComplexMatrix operator -(ComplexMatrix x1, Matrix x2)
		{
			return ComplexMatrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] - x2[i].</returns>
		public static ComplexMatrix operator -(Matrix x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="ComplexMatrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] - x2.</returns>
		public static ComplexMatrix operator -(ComplexMatrix x1, double x2)
		{
			return ComplexMatrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a double and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1 - x2[i].</returns>
		public static ComplexMatrix operator -(double x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] - x2.</returns>
		public static ComplexMatrix operator -(ComplexMatrix x1, Complex x2)
		{
			return ComplexMatrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1 - x2[i].</returns>
		public static ComplexMatrix operator -(Complex x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Subtract(x1, x2);
		}

		#endregion // -

		#region *

		/// <summary>
		/// Multiplies two <see cref="ComplexMatrix"/> instances pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] * x2[i].</returns>
		public static ComplexMatrix operator *(ComplexMatrix x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] * x2[i].</returns>
		public static ComplexMatrix operator *(ComplexMatrix x1, Matrix x2)
		{
			return ComplexMatrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] * x2[i].</returns>
		public static ComplexMatrix operator *(Matrix x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="ComplexMatrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] * x2.</returns>
		public static ComplexMatrix operator *(ComplexMatrix x1, double x2)
		{
			return ComplexMatrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a double and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1 * x2[i].</returns>
		public static ComplexMatrix operator *(double x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] * x2.</returns>
		public static ComplexMatrix operator *(ComplexMatrix x1, Complex x2)
		{
			return ComplexMatrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1 * x2[i].</returns>
		public static ComplexMatrix operator *(Complex x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Multiply(x1, x2);
		}

		#endregion // *

		#region /

		/// <summary>
		/// Divides two <see cref="ComplexMatrix"/> instances pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] / x2[i].</returns>
		public static ComplexMatrix operator /(ComplexMatrix x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] / x2[i].</returns>
		public static ComplexMatrix operator /(ComplexMatrix x1, Matrix x2)
		{
			return ComplexMatrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] / x2[i].</returns>
		public static ComplexMatrix operator /(Matrix x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="ComplexMatrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] / x2.</returns>
		public static ComplexMatrix operator /(ComplexMatrix x1, double x2)
		{
			return ComplexMatrix.Divide(x1, x2);
		}

		/// <summary>
		/// Multiplies a double and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1 / x2[i].</returns>
		public static ComplexMatrix operator /(double x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1[i] / x2.</returns>
		public static ComplexMatrix operator /(ComplexMatrix x1, Complex x2)
		{
			return ComplexMatrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/> pointwise.
		/// </summary>
		/// <returns>A ComplexMatrix y, where y[i] = x1 / x2[i].</returns>
		public static ComplexMatrix operator /(Complex x1, ComplexMatrix x2)
		{
			return ComplexMatrix.Divide(x1, x2);
		}

		#endregion // /

		#region ==

		/// <summary>
		/// Determines whether two <see cref="ComplexMatrix"/> instances have the same dimensions and element values.
		/// </summary>
		/// <param name="x1">The first ComplexMatrix.</param>
		/// <param name="x2">The second ComplexMatrix.</param>
		/// <returns>Returns True if the two ComplexMatrix instances are equal; False otherwise.</returns>
		public static bool operator ==(ComplexMatrix x1, ComplexMatrix x2)
		{
			if (!(x1.Size == x2.Size))
				return false;

			int length = x1.Length;
			for (int i = 0; i < length; i++)
			{
				if (x1.Elements[i] != x2.Elements[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determines whether a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> have the same dimensions 
		/// and element values.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>Returns True if the ComplexMatrix and the Matrix are equal; False otherwise.</returns>
		public static bool operator ==(ComplexMatrix x1, Matrix x2)
		{
			return x1 == (ComplexMatrix)x2;
		}

		/// <summary>
		/// Determines whether a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> have the same dimensions 
		/// and element values.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>Returns True if the Matrix and the ComplexMatrix are equal; False otherwise.</returns>
		public static bool operator ==(Matrix x1, ComplexMatrix x2)
		{
			return (ComplexMatrix)x1 == x2;
		}

		/// <summary>
		/// Returns the pointwise equality operator for a <see cref="ComplexMatrix"/> and a double.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = Equals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator ==(ComplexMatrix x1, double x2)
		public static BooleanMatrix operator ==(ComplexMatrix x1, double x2)
		{
			if (x1.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] == x2;
			}

			// MD 4/19/11 - TFS72396
			//return new BooleanVector(newElements, x1.Dimensions);
			return new BooleanMatrix(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise equality operator for a double and a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = Equals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator ==(double x1, ComplexMatrix x2)
		public static BooleanMatrix operator ==(double x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}

			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 == x2.elements[i];
			}

			// MD 4/19/11 - TFS72396
			//return new BooleanVector(newElements, x2.Dimensions);
			return new BooleanMatrix(newElements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise equality operator for a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = Equals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator ==(ComplexMatrix x1, Complex x2)
		public static BooleanMatrix operator ==(ComplexMatrix x1, Complex x2)
		{
			if (x1.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] == x2;
			}

			// MD 4/19/11 - TFS72396
			//return new BooleanVector(newElements, x1.Dimensions);
			return new BooleanMatrix(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise equality operator for a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = Equals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator ==(Complex x1, ComplexMatrix x2)
		public static BooleanMatrix operator ==(Complex x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}

			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 == x2.elements[i];
			}

			// MD 4/19/11 - TFS72396
			//return new BooleanVector(newElements, x2.Dimensions);
			return new BooleanMatrix(newElements, x2.Dimensions);
		}

		#endregion // ==

		#region !=

		/// <summary>
		/// Determines whether two <see cref="ComplexMatrix"/> instances have different dimensions or element values.
		/// </summary>
		/// <param name="x1">The first ComplexMatrix.</param>
		/// <param name="x2">The second ComplexMatrix.</param>
		/// <returns>Returns True if the two ComplexMatrix instances are unequal; False otherwise.</returns>
		public static bool operator !=(ComplexMatrix x1, ComplexMatrix x2)
		{
			return !(x1 == x2);
		}

		/// <summary>
		/// Determines whether a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> have different dimensions 
		/// and element values.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>Returns True if the ComplexMatrix and the Matrix are unequal; False otherwise.</returns>
		public static bool operator !=(ComplexMatrix x1, Matrix x2)
		{
			return x1 != (ComplexMatrix)x2;
		}

		/// <summary>
		/// Determines whether a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> have different dimensions 
		/// and element values.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>Returns True if the Matrix and the ComplexMatrix are unequal; False otherwise.</returns>
		public static bool operator !=(Matrix x1, ComplexMatrix x2)
		{
			return (ComplexMatrix)x1 != x2;
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a <see cref="ComplexMatrix"/> and a double.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = NotEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator !=(ComplexMatrix x1, double x2)
		public static BooleanMatrix operator !=(ComplexMatrix x1, double x2)
		{
			if (x1.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}
			
			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
			    newElements[i] = x1.elements[i] != x2;
			}

			// MD 4/19/11 - TFS72396
			//return new BooleanVector(newElements, x1.Dimensions);
			return new BooleanMatrix(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a double and a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = NotEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator !=(double x1, ComplexMatrix x2)
		public static BooleanMatrix operator !=(double x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}
			
			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
			    newElements[i] = x1 != x2.elements[i];
			}

			// MD 4/19/11 - TFS72396
			//return new BooleanVector(newElements, x2.Dimensions);
			return new BooleanMatrix(newElements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = NotEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator !=(ComplexMatrix x1, Complex x2)
		public static BooleanMatrix operator !=(ComplexMatrix x1, Complex x2)
		{
			if (x1.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}
			
			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
			    newElements[i] = x1.elements[i] != x2;
			}

			// MD 4/19/11 - TFS72396
			//return new BooleanVector(newElements, x1.Dimensions);
			return new BooleanMatrix(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = NotEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator !=(Complex x1, ComplexMatrix x2)
		public static BooleanMatrix operator !=(Complex x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}
			
			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
			    newElements[i] = x1 != x2.elements[i];
			}

			// MD 4/19/11 - TFS72396
			//return new BooleanVector(newElements, x2.Dimensions);
			return new BooleanMatrix(newElements, x2.Dimensions);
		}

		#endregion // !=

		#region >

		/// <summary>
		/// Compares two <see cref="ComplexMatrix"/> instances using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">The first ComplexMatrix.</param>
		/// <param name="x2">The second ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(ComplexMatrix x1, ComplexMatrix x2)
		public static BooleanMatrix operator >(ComplexMatrix x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] > 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a double using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(ComplexMatrix x1, double x2)
		public static BooleanMatrix operator >(ComplexMatrix x1, double x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] > 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="ComplexMatrix"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(double x1, ComplexMatrix x2)
		public static BooleanMatrix operator >(double x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x2.Clone().CompareTo(x1);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] < 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number using the GreaterThan 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(ComplexMatrix x1, Complex x2)
		public static BooleanMatrix operator >(ComplexMatrix x1, Complex x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] > 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/> using the GreaterThan 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(Complex x1, ComplexMatrix x2)
		public static BooleanMatrix operator >(Complex x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x2.Clone().CompareTo(x1);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] < 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanZero);
		}

		#endregion // >

		#region >=

		/// <summary>
		/// Compares two <see cref="ComplexMatrix"/> instances using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">The first ComplexMatrix.</param>
		/// <param name="x2">The second ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(ComplexMatrix x1, ComplexMatrix x2)
		public static BooleanMatrix operator >=(ComplexMatrix x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] >= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a double using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(ComplexMatrix x1, double x2)
		public static BooleanMatrix operator >=(ComplexMatrix x1, double x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] >= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="ComplexMatrix"/> using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(double x1, ComplexMatrix x2)
		public static BooleanMatrix operator >=(double x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x2.Clone().CompareTo(x1);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] <= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(ComplexMatrix x1, Complex x2)
		public static BooleanMatrix operator >=(ComplexMatrix x1, Complex x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] >= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/> using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(Complex x1, ComplexMatrix x2)
		public static BooleanMatrix operator >=(Complex x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x2.Clone().CompareTo(x1);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] <= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		#endregion // >=

		#region <

		/// <summary>
		/// Compares two <see cref="ComplexMatrix"/> instances using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">The first ComplexMatrix.</param>
		/// <param name="x2">The second ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(ComplexMatrix x1, ComplexMatrix x2)
		public static BooleanMatrix operator <(ComplexMatrix x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] < 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a double using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(ComplexMatrix x1, double x2)
		public static BooleanMatrix operator <(ComplexMatrix x1, double x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] < 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="ComplexMatrix"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(double x1, ComplexMatrix x2)
		public static BooleanMatrix operator <(double x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x2.Clone().CompareTo(x1);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] > 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(ComplexMatrix x1, Complex x2)
		public static BooleanMatrix operator <(ComplexMatrix x1, Complex x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] < 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(Complex x1, ComplexMatrix x2)
		public static BooleanMatrix operator <(Complex x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x2.Clone().CompareTo(x1);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] > 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		#endregion // <

		#region <=

		/// <summary>
		/// Compares two <see cref="ComplexMatrix"/> instances using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">The first ComplexMatrix.</param>
		/// <param name="x2">The second ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(ComplexMatrix x1, ComplexMatrix x2)
		public static BooleanMatrix operator <=(ComplexMatrix x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] <= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a double using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(ComplexMatrix x1, double x2)
		public static BooleanMatrix operator <=(ComplexMatrix x1, double x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] <= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="ComplexMatrix"/> using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(double x1, ComplexMatrix x2)
		public static BooleanMatrix operator <=(double x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x2.Clone().CompareTo(x1);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] >= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(ComplexMatrix x1, Complex x2)
		public static BooleanMatrix operator <=(ComplexMatrix x1, Complex x2)
		{
			ComplexMatrix x = x1.Clone().CompareTo(x2);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] <= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/> using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(Complex x1, ComplexMatrix x2)
		public static BooleanMatrix operator <=(Complex x1, ComplexMatrix x2)
		{
			ComplexMatrix x = x2.Clone().CompareTo(x1);

			// MD 4/19/11 - TFS72396
			// Moved all code to a helper method.
			//int length = x.Length;
			//bool[] newElements = new bool[length];
			//for (int i = 0; i < length; i++)
			//{
			//    if (x.elements[i] >= 0)
			//        newElements[i] = true;
			//    else
			//        newElements[i] = false;
			//}
			//
			//return new BooleanVector(newElements, x.Dimensions);
			return ComplexMatrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		#endregion // <=

		#region Casting

		/// <summary>
		/// Casts a <see cref="ComplexVector"/> to a <see cref="ComplexMatrix"/>. 
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A Complex y, where y[i] = x[i].</returns>
		public static explicit operator ComplexMatrix(ComplexVector x)
		{
			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			return new ComplexMatrix(x.Elements, x.Dimensions);
		}

		/// <summary>
		/// Casts a <see cref="ComplexMatrix"/> to a <see cref="ComplexVector"/>. 
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexVector y, where y[i] = x[i].</returns>
		public static explicit operator ComplexVector(ComplexMatrix x)
		{
			if (!x.IsColumn() && !x.IsRow())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_13"));

			if (x.IsEmpty())
				return ComplexVector.Empty;

			return new ComplexVector(x.Elements, x.Dimensions);
		}

		/// <summary>
		/// Casts a <see cref="Vector"/> to a <see cref="ComplexMatrix"/>. 
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A Complex y, where y[i] = x[i].</returns>
		public static explicit operator ComplexMatrix(Vector x)
		{
			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			ComplexVector x2 = x;
			return new ComplexMatrix(x2.Elements, x2.Dimensions);
		}

		/// <summary>
		/// Casts a <see cref="Complex"/> number to a unitary <see cref="ComplexMatrix"/>. 
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>A unitary ComplexMatrix y, where y[0] = x.</returns>
		public static implicit operator ComplexMatrix(Complex x)
		{
			return new ComplexMatrix(x, 1);
		}

		/// <summary>
		/// Casts a double to a unitary <see cref="ComplexMatrix"/>. 
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>A unitary ComplexMatrix y, where y[0] = x.</returns>
		public static implicit operator ComplexMatrix(double x)
		{
			return new ComplexMatrix(x, 1);
		}

		/// <summary>
		/// Casts a Complex array to a row <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> array.</param>
		/// <returns>A row ComplexMatrix y, where y[i] = x[i].</returns>
		public static implicit operator ComplexMatrix(Complex[] x)
		{
			return new ComplexMatrix(x);
		}

		/// <summary>
		/// Casts a double array to a row <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x">A double array.</param>
		/// <returns>A row ComplexMatrix y, where y[i] = (Complex)x[i].</returns>
		public static implicit operator ComplexMatrix(double[] x)
		{
			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexMatrix(newElements, new int[] { 1, x.Length });
		}

		/// <summary>
		/// Casts a <see cref="Matrix"/> to a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A row ComplexMatrix y, where y[i] = (Complex)x[i].</returns>
		public static implicit operator ComplexMatrix(Matrix x)
		{
			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexMatrix(newElements, x.Dimensions);
		}

		/// <summary>
		/// Casts an int array to a row <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x">An int array.</param>
		/// <returns>A row ComplexMatrix y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexMatrix(int[] x)
		{
			int length = x.Length;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexMatrix(newElements);
		}

		/// <summary>
		/// Casts a short array to a row <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x">A short array.</param>
		/// <returns>A row ComplexMatrix y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexMatrix(short[] x)
		{
			int length = x.Length;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexMatrix(newElements);
		}

		/// <summary>
		/// Casts a long array to a row <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x">A long array.</param>
		/// <returns>A row ComplexMatrix y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexMatrix(long[] x)
		{
			int length = x.Length;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexMatrix(newElements);
		}

		/// <summary>
		/// Casts a float array to a row <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x">A float array.</param>
		/// <returns>A row ComplexMatrix y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexMatrix(float[] x)
		{
			int length = x.Length;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexMatrix(newElements);
		}

		/// <summary>
		/// Casts a decimal array to a row <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">A decimal array.</param>
		/// <returns>A row ComplexMatrix y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexMatrix(decimal[] x)
		{
			int length = x.Length;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = (double)x[i];
			}
			return new ComplexMatrix(newElements);
		}

		#endregion //Casting

		#endregion //Operators

		#region Methods

		#region Basic Math Functions

		#region Acos

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Acos(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Acos(ComplexMatrix)"/>
		public ComplexMatrix Acos()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Acos(this.elements[i]);
			}
			return this;
		}

		#endregion //Acos

		#region Add

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] + x[i].
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix Add(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] + x[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] + x[i].
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix Add(ComplexMatrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] + x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] + x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix Add(double x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] + x;
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] + x.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix Add(Complex x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] + x;
			}
			return this;
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(ComplexMatrix x1, Matrix x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(Matrix x1, ComplexMatrix x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexMatrix();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1[i] + x2.elements[i];
			}
			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(ComplexMatrix x1, ComplexMatrix x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(ComplexMatrix x1, double x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(double x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 + x2.elements[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(ComplexMatrix x1, Complex x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(Complex x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 + x2[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Add

		#region Asin

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Asin(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Asin(ComplexMatrix)"/>
		public ComplexMatrix Asin()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Asin(this.elements[i]);
			}
			return this;
		}

		#endregion //Asin

		#region Atan

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Atan(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Atan(ComplexMatrix)"/>
		public ComplexMatrix Atan()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Atan(this.elements[i]);
			}
			return this;
		}

		#endregion //Atan

		#region Ceiling

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Ceiling(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Ceiling(ComplexMatrix)"/>
		public ComplexMatrix Ceiling()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Ceiling(this.elements[i]);
			}
			return this;
		}

		#endregion //Ceiling

		#region Cis

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Cis(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Cis(ComplexMatrix)"/>
		public ComplexMatrix Cis()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Cis(this.elements[i]);
			}
			return this;
		}

		#endregion //Cis

		#region CompareTo

		/// <summary>
		/// Modifies the <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with CompareTo(y[i],x[i]).
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions.
		/// </exception>
		public ComplexMatrix CompareTo(ComplexMatrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			Complex thisCurr;
			Complex xCurr;
			for (int i = 0, length = this.Length; i < length; i++)
			{
				thisCurr = this.elements[i];
				xCurr = x.elements[i];
				if (thisCurr < xCurr)
					this.elements[i] = -1;
				else if (thisCurr > xCurr)
					this.elements[i] = 1;
				else
					this.elements[i] = 0;
			}
			return this;
		}

		/// <summary>
		/// Modifies the <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with CompareTo(y[i],x[i]).
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix CompareTo(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			Complex thisCurr;
			Complex xCurr;
			for (int i = 0, length = this.Length; i < length; i++)
			{
				thisCurr = this.elements[i];
				xCurr = x.Elements[i];
				if (thisCurr < xCurr)
					this.elements[i] = -1;
				else if (thisCurr > xCurr)
					this.elements[i] = 1;
				else
					this.elements[i] = 0;
			}
			return this;
		}

		/// <summary>
		/// Modifies the <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with CompareTo(y[i],x).
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix CompareTo(double x)
		{
			if (this.IsEmpty())
				return this;

			Complex thisCurr;
			for (int i = 0, length = this.Length; i < length; i++)
			{
				thisCurr = this.elements[i];
				if (thisCurr < x)
					this.elements[i] = -1;
				else if (thisCurr > x)
					this.elements[i] = 1;
				else
					this.elements[i] = 0;
			}
			return this;
		}

		/// <summary>
		/// Modifies the <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with CompareTo(y[i],x).
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix CompareTo(Complex x)
		{
			if (this.IsEmpty())
				return this;

			Complex thisCurr;
			for (int i = 0, length = this.Length; i < length; i++)
			{
				thisCurr = this.elements[i];
				if (thisCurr < x)
					this.elements[i] = -1;
				else if (thisCurr > x)
					this.elements[i] = 1;
				else
					this.elements[i] = 0;
			}
			return this;
		}

		#endregion //CompareTo

		#region Conj

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Conj(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Conj(ComplexMatrix)"/>
		public ComplexMatrix Conj()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Conj(this.elements[i]);
			}
			return this;
		}

		#endregion //Conj

		#region Cos

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Cos(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Cos(ComplexMatrix)"/>
		public ComplexMatrix Cos()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Cos(this.elements[i]);
			}
			return this;
		}

		#endregion //Cos

		#region Cosh

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Cosh(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Cosh(ComplexMatrix)"/>
		public ComplexMatrix Cosh()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Cosh(this.elements[i]);
			}
			return this;
		}

		#endregion //Cosh

		#region Divide

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] / x[i].
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix Divide(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] / x[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] / x[i].
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix Divide(ComplexMatrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] / x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] / x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix Divide(double x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] / x;
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] / x.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix Divide(Complex x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] / x;
			}
			return this;
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(ComplexMatrix x1, Matrix x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(Matrix x1, ComplexMatrix x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexMatrix();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1[i] / x2.elements[i];
			}
			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(ComplexMatrix x1, ComplexMatrix x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(ComplexMatrix x1, double x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(double x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 / x2.elements[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(ComplexMatrix x1, Complex x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(Complex x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 / x2[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Divide

		#region Exp

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Exp(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Exp(ComplexMatrix)"/>
		public ComplexMatrix Exp()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Exp(this.elements[i]);
			}
			return this;
		}

		#endregion //Exp

		#region Floor

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Floor(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Floor(ComplexMatrix)"/>
		public ComplexMatrix Floor()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Floor(this.elements[i]);
			}
			return this;
		}

		#endregion //Floor

		#region Log

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Log(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Log(ComplexMatrix)"/>
		public ComplexMatrix Log()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Log(this.elements[i]);
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Log(y[i],B).
		/// </summary>
		/// <param name="B"></param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Log(ComplexMatrix,Complex)"/>
		public ComplexMatrix Log(Complex B)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Log(this.elements[i], B);
			}
			return this;
		}

		#endregion //Log

		#region Log2

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Log2(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Log2(ComplexMatrix)"/>
		public ComplexMatrix Log2()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Log2(this.elements[i]);
			}
			return this;
		}

		#endregion //Log2

		#region Log10

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Log10(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Log10(ComplexMatrix)"/>
		public ComplexMatrix Log10()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Log10(this.elements[i]);
			}
			return this;
		}

		#endregion //Log10

		#region Multiply

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] * x[i].
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix Multiply(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] * x[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] * x[i].
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix Multiply(ComplexMatrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] * x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] * x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix Multiply(double x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] * x;
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] * x.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix Multiply(Complex x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] * x;
			}
			return this;
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(ComplexMatrix x1, Matrix x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(Matrix x1, ComplexMatrix x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexMatrix();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1[i] * x2.elements[i];
			}
			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(ComplexMatrix x1, ComplexMatrix x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(ComplexMatrix x1, double x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(double x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 * x2.elements[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(ComplexMatrix x1, Complex x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(Complex x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 * x2[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Multiply

		#region Pow

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Pow(y[i],x[i]).
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		/// <seealso cref="Compute.Pow(ComplexMatrix,Matrix)"/>
		public ComplexMatrix Pow(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Pow(this.elements[i], x[i]);
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Pow(y[i],x[i]).
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		/// <seealso cref="Compute.Pow(ComplexMatrix,ComplexMatrix)"/>
		public ComplexMatrix Pow(ComplexMatrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Pow(this.elements[i], x.elements[i]);
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Pow(y[i],x).
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Pow(ComplexMatrix,double)"/>
		public ComplexMatrix Pow(double x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Pow(this.elements[i], x);
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Pow(y[i],x).
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Pow(ComplexMatrix,Complex)"/>
		public ComplexMatrix Pow(Complex x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Pow(this.elements[i], x);
			}
			return this;
		}

		#endregion //Pow

		#region Round

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Round(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Round(ComplexMatrix)"/>
		public ComplexMatrix Round()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Round(this.elements[i]);
			}
			return this;
		}

		#endregion //Round

		#region Sign

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Sign(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Sign(ComplexMatrix)"/>
		public ComplexMatrix Sign()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Sign(this.elements[i]);
			}
			return this;
		}

		#endregion //Sign

		#region Sin

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Sin(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Sin(ComplexMatrix)"/>
		public ComplexMatrix Sin()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Sin(this.elements[i]);
			}
			return this;
		}

		#endregion //Sin

		#region Sinh

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Sinh(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Sinh(ComplexMatrix)"/>
		public ComplexMatrix Sinh()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Sinh(this.elements[i]);
			}
			return this;
		}

		#endregion //Sinh

		#region Sqrt

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Sqrt(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Sqrt(ComplexMatrix)"/>
		public ComplexMatrix Sqrt()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Sqrt(this.elements[i]);
			}
			return this;
		}

		#endregion //Sqrt

		#region Subtract

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] - x[i].
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix Subtract(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] - x[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] - x[i].
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexMatrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexMatrix Subtract(ComplexMatrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] - x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] - x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix Subtract(double x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] - x;
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with y[i] - x.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		public ComplexMatrix Subtract(Complex x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] - x;
			}
			return this;
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(ComplexMatrix x1, Matrix x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(Matrix x1, ComplexMatrix x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexMatrix();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_14"));

			int length = x2.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1[i] - x2.elements[i];
			}
			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(ComplexMatrix x1, ComplexMatrix x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(ComplexMatrix x1, double x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(double x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 - x2.elements[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(ComplexMatrix x1, Complex x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(Complex x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 - x2[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Subtract

		#region Tan

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Tan(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Tan(ComplexMatrix)"/>
		public ComplexMatrix Tan()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Tan(this.elements[i]);
			}
			return this;
		}

		#endregion //Tan

		#region Tanh

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i] with Tanh(y[i]).
		/// </summary>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <seealso cref="Compute.Tanh(ComplexMatrix)"/>
		public ComplexMatrix Tanh()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Tanh(this.elements[i]);
			}
			return this;
		}

		#endregion //Tanh

		#endregion //Basic Math Functions

		#region ComplexMatrix Functions

		#region Clone

		/// <summary>
		/// Returns a copy of the <see cref="ComplexMatrix"/> instance.
		/// </summary>
		public ComplexMatrix Clone()
		{
			if (this.IsEmpty())
				return new ComplexMatrix();

			return new ComplexMatrix(Utilities.Clone<Complex>(this.elements), this.Dimensions);
		}

		#endregion //Clone

		#region CumProduct

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i1,..,iN,..,iM] with 
		/// y[i1,..,0,..,iM]*...*y[i1,..,iN,..,iM].
		/// </summary>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <seealso cref="Compute.CumProduct(ComplexMatrix,int)"/>
		public ComplexMatrix CumProduct(int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_14"));

			if (this.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_16"));

			if (N > this.Rank)
				return this.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(this);

			int[] newDimensions = (int[])this.Dimensions.Clone();
			int lengthN = this.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);

			int index;
			Complex curr;
			int[] subscript = new int[this.Rank];
			for (int i = 0, length = newLength; i < length; i++)
			{
				curr = 1;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					index = Compute.SubscriptToIndex(subscript, size);
					curr *= this.Elements[index];
					this.Elements[index] = curr;
				}

				Utilities.IncrementSubscript(ref subscript, N, this.Dimensions);
			}

			return this;
		}

		#endregion //CumProduct

		#region CumSum

		/// <summary>
		/// Modifies a <see cref="ComplexMatrix"/>, y, by replacing each element y[i1,..,iN,..,iM] with 
		/// y[i1,..,0,..,iM]+...+y[i1,..,iN,..,iM].
		/// </summary>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>The modified ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <seealso cref="Compute.CumSum(Matrix,int)"/>
		public ComplexMatrix CumSum(int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (this.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_16"));

			if (N > this.Rank)
				return this.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(this);

			int[] newDimensions = (int[])this.Dimensions.Clone();
			int lengthN = this.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);

			int index;
			Complex curr;
			int[] subscript = new int[this.Rank];
			for (int i = 0, length = newLength; i < length; i++)
			{
				curr = 0;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					index = Compute.SubscriptToIndex(subscript, size);
					curr += this.Elements[index];
					this.Elements[index] = curr;
				}

				Utilities.IncrementSubscript(ref subscript, N, this.Dimensions);
			}

			return this;
		}

		#endregion //CumSum

		#endregion //ComplexMatrix Functions

		#region Private Methods

		// MD 4/19/11 - TFS72396
		#region FindValues

		private static BooleanMatrix FindValues(ComplexMatrix x, FindValuesType type)
		{
			int length = x.Length;
			bool[] newElements = new bool[length];

			Complex zeroValue = 0;

			switch (type)
			{
				case FindValuesType.GreaterThanZero:
					for (int i = 0; i < length; i++)
						newElements[i] = x.elements[i] > zeroValue;
					break;

				case FindValuesType.GreaterThanOrEqualToZero:
					for (int i = 0; i < length; i++)
						newElements[i] = x.elements[i] >= zeroValue;
					break;

				case FindValuesType.LessThanZero:
					for (int i = 0; i < length; i++)
						newElements[i] = x.elements[i] < zeroValue;
					break;

				case FindValuesType.LessThanOrEqualToZero:
					for (int i = 0; i < length; i++)
						newElements[i] = x.elements[i] <= zeroValue;
					break;

				default:
					for (int i = 0; i < length; i++)
						newElements[i] = false;
					Utilities.DebugFail("Unknown FindValuesType: " + type);
					break;
			}

			return new BooleanMatrix(newElements, x.Dimensions);
		}

		#endregion  // FindValues

		#endregion  // Private Methods

		#endregion //Methods

		#region Properties

		#region Elements






		internal IList<Complex> Elements
		{
			get { return this.elements; }
		}

		#endregion //Elements

		#region Indexers

		/// <summary>
		/// An indexer for the <see cref="ComplexMatrix"/>. The Matrix can be indexed one-dimensionally or multi-dimensionally.
		/// </summary>
		/// <param name="subscript">A sequence of indices that correspond to the dimensions of the ComplexMatrix.</param>
		/// <returns>The element specified by the subscript.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the ComplexMatrix is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="subscript"/> is non-unitary and has a different number of dimensions than 
		/// the ComplexMatrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index is out of the bounds of the ComplexMatrix.
		/// </exception>
		public Complex this[params int[] subscript]
		{
			get
			{
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_7"));

				int subscriptLength = subscript.Length;
				if (subscriptLength != 1 && subscriptLength != this.Rank)
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_17"), "subscript");

				if (subscriptLength == 1)
				{
					int index = subscript[0];
					if (index < 0 || index >= this.Length)
						throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_8"));

					return this.elements[index];
				}
				else
				{
					int index;
					for (int i = 0; i < subscriptLength; i++)
					{
						index = subscript[i];
						if (index < 0 || index >= this.Dimensions[i])
							throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_10"));
					}

					return this.elements[Compute.SubscriptToIndex(subscript, this.Size)];
				}
			}
			set
			{
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_11"));

				int subscriptLength = subscript.Length;
				if (subscriptLength != 1 && subscriptLength != this.Rank)
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_17"), "subscript");

				if (subscriptLength == 1)
				{
					int index = subscript[0];
					if (index < 0 || index >= this.Length)
						throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_8"));

					this.elements[index] = value;
				}
				else
				{
					int index;
					for (int i = 0; i < subscriptLength; i++)
					{
						index = subscript[i];
						if (index < 0 || index >= this.Dimensions[i])
							throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_10"));
					}

					this.elements[Compute.SubscriptToIndex(subscript, this.Size)] = value;
				}
			}
		}

		/// <summary>
		/// An indexer for the <see cref="ComplexMatrix"/> that takes a series of <see cref="Vector"/> subscripts. 
		/// The ComplexMatrix can be indexed one-dimensionally or multi-dimensionally.
		/// </summary>
		/// <param name="subscript">An array of index Vectors.</param>
		/// <returns>A ComplexMatrix containing the elements specified by the <paramref name="subscript"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the ComplexMatrix is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="subscript"/> is non-unitary and has a different number of dimensions than 
		/// the ComplexMatrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index is out of the bounds of the ComplexMatrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index has a non-integer value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the set value does not align with the elements specified by the subscript.
		/// </exception>
		public ComplexMatrix this[params Vector[] subscript]
		{
			get
			{
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_11"));

				int subscriptLength = subscript.Length;
				if (subscriptLength != 1 && subscriptLength != this.Rank)
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_17"), "subscript");

				if (subscriptLength == 1)
				{
					Vector indices = subscript[0];
					if (indices.IsEmpty())
						return ComplexMatrix.Empty;

					int index;
					double indexCheck;
					int length = indices.Length;
					ComplexMatrix result = new ComplexMatrix(length);
					for (int i = 0; i < length; i++)
					{
						indexCheck = indices[i];
						index = (int)Compute.Round(indexCheck);
						if (index != indexCheck)
							throw new ArgumentException(Compute.GetString("LE_ArgumentException_19"), "subscript");

						if (index < 0 || index >= this.Length)
							throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_8"));

						result[i] = this.elements[index];
					}

					return result;
				}
				else
				{
					bool isEmpty = false;
					Vector indices;
					int[] newDimensions = new int[subscriptLength];
					for (int i = 0; i < subscriptLength; i++)
					{
						indices = subscript[i];
						if (indices.IsEmpty())
							isEmpty = true;

						newDimensions[i] = indices.Length;
					}

					if (Utilities.ArrayEquals(newDimensions, new int[subscriptLength]))
						return ComplexMatrix.Empty;

					if (isEmpty)
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_20"), "subscript");

					Vector size = this.Size;
					int length = Utilities.Product(newDimensions);
					Complex[] newElements = new Complex[length];
					ComplexMatrix result = new ComplexMatrix(newElements, newDimensions);

					int index;
					double indexCheck;
					int[] sub = new int[subscriptLength];
					int[] subscriptIndex = new int[subscriptLength];
					for (int i = 0; i < length; i++)
					{
						for (int j = 0; j < subscriptLength; j++)
						{
							indexCheck = subscript[j].Elements[subscriptIndex[j]];
							index = (int)Compute.Round(indexCheck);

							if (index != indexCheck)
								throw new ArgumentException(Compute.GetString("LE_ArgumentException_19"), "subscript");

							if (index < 0 || index >= this.Dimensions[j])
								throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_8"));

							sub[j] = index;
						}

						result[subscriptIndex] = this.elements[Compute.SubscriptToIndex(sub, size)];
						Utilities.IncrementSubscript(ref subscriptIndex, newDimensions);
					}

					return result;
				}
			}
			set
			{
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_11"));

				int subscriptLength = subscript.Length;
				if (subscriptLength != 1 && subscriptLength != this.Rank)
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_17"), "subscript");

				bool valueIsUnitary = value.IsUnitary();
				if (subscriptLength == 1)
				{
					Vector indices = subscript[0];
					if (!Utilities.ArrayEquals(indices.Dimensions, value.Dimensions) && !valueIsUnitary)
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_21"), "subscript");

					if (indices.IsEmpty())
						return;

					int index;
					double indexCheck;
					int length = indices.Length;
					for (int i = 0; i < length; i++)
					{
						indexCheck = indices[i];
						index = (int)Compute.Round(indexCheck);
						if (index != indexCheck)
							throw new ArgumentException(Compute.GetString("LE_ArgumentException_19"), "subscript");

						if (index < 0 || index >= this.Length)
							throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_8"));

						if (valueIsUnitary)
							this.elements[index] = value[0];
						else
							this.elements[index] = value[i];
					}
				}
				else
				{
					int[] newDimensions = new int[subscriptLength];
					for (int i = 0; i < subscriptLength; i++)
					{
						newDimensions[i] = subscript[i].Length;
					}

					if (Utilities.ArrayEquals(newDimensions, new int[subscriptLength]) && value.IsEmpty())
						return;

					if (!Utilities.ArrayEquals(newDimensions, value.Dimensions) && !valueIsUnitary)
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_21"), "subscript");

					Vector size = this.Size;
					int length = Utilities.Product(newDimensions);

					int index;
					double indexCheck;
					int[] sub = new int[subscriptLength];
					int[] subscriptIndex = new int[subscriptLength];
					for (int i = 0; i < length; i++)
					{
						for (int j = 0; j < subscriptLength; j++)
						{
							indexCheck = subscript[j].Elements[subscriptIndex[j]];
							index = (int)Compute.Round(indexCheck);

							if (index != indexCheck)
								throw new ArgumentException(Compute.GetString("LE_ArgumentException_19"), "subscript");

							if (index < 0 || index >= this.Dimensions[j])
								throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_8"));

							sub[j] = index;
						}

						if (valueIsUnitary)
							this.elements[Compute.SubscriptToIndex(sub, size)] = value[0];
						else
							this.elements[Compute.SubscriptToIndex(sub, size)] = value[subscriptIndex];

						Utilities.IncrementSubscript(ref subscriptIndex, newDimensions);
					}
				}
			}
		}

		#endregion //Indexers

		#endregion //Properties
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