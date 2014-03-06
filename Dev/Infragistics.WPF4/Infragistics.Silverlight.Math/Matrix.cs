using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections;

namespace Infragistics.Math
{
	/// <summary>
	/// A Matrix is a list of doubles with an additional dimensionality property that specifies its spatial orientation.
	/// </summary>
    public class Matrix : MatrixBase
			, IEnumerable<double>

, ICloneable

	{
		#region Member Variables

		private IList<double> elements;

		#endregion //Member Variables

		#region Static Variables

        #region Empty

        /// <summary>
        /// Returns the empty <see cref="Matrix"/>.
        /// </summary>
        public static Matrix Empty = new Matrix();

        #endregion //Empty

        #endregion //Static Variables

        #region Constructors
        
        /// <summary>
        /// Initializes an empty <see cref="Matrix"/> instance.
        /// </summary>
        /// <seealso cref="MatrixBase.IsEmpty"/>
        public Matrix()
            : base()
        {
        }

        /// <summary>
        /// Initializes a one-dimensional <see cref="Matrix"/> of a specified <paramref name="length"/>. 
        /// </summary>
        /// <param name="length">The length of the constructed Matrix.</param>
        public Matrix(int length)
            : base(length)
        {
            this.elements = new double[length];
        }

		/// <summary>
		/// Initializes a zero <see cref="Matrix"/> with the specified <paramref name="dimensions"/>. 
		/// </summary>
		/// <param name="dimensions">The dimensions of the constructed Matrix.</param>
		public Matrix(int[] dimensions)
			: base(dimensions)
		{
			this.elements = new double[this.Length];
		}

		/// <summary>
		/// Initializes a constant <see cref="Matrix"/> with the specified <paramref name="dimensions"/>. 
		/// </summary>
		/// <param name="value">The constant value assigned to each element of the Matrix.</param>
		/// <param name="dimensions">The dimensions of the constructed Matrix.</param>
		public Matrix(double value, int[] dimensions)
			: base(dimensions)
		{
			this.elements = new double[this.Length];
			for (int i = 0; i < this.Length; i++)
			{
				this.elements[i] = value;
			}
		}

		/// <summary>
		/// Initializes a one-dimensional <see cref="Matrix"/> of the specified <paramref name="length"/> and the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value assigned to each element of the constructed Matrix.</param>
		/// <param name="length">The length of the constructed Matrix.</param>
		public Matrix(double value, int length)
            : base(length)
        {
            this.elements = new double[length];
            for (int i = 0; i < length; i++)
            {
                this.elements[i] = value;
            }
        }

		/// <summary>
		/// Initializes a <see cref="Matrix"/> with the specified <paramref name="elements"/> and <paramref name="dimensions"/>.
		/// </summary>
		/// <param name="elements">The elements of the constructed Matrix.</param>
		/// <param name="dimensions">The dimensions of the constructed Matrix.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the dimensions specify a size that is inconsistent with the length of the elements.
		/// </exception>
		public Matrix(IList<double> elements, int[] dimensions)
            : base(dimensions)
        {
            if (elements.Count != this.Length)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_56"));

            this.elements = elements;
        }

		/// <summary>
		/// Initializes a <see cref="Matrix"/> by copying the elements and dimensions of a multi-dimensional double array.
		/// </summary>
		/// <param name="x">A multi-dimensional double array.</param>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not a double array.
		/// </exception>
		public Matrix(Array x)
			: base(Utilities.GetArrayDimensions(x))
		{
			if (x.GetType().GetElementType() != typeof(double))
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_57"), "x");

			// MD 4/7/11 - TFS71039
			// The way the .NET Framework iterates multidimensional arrays is not the same as how we index into the elements 
			// collection. Use the IndexToSubscript method to make sure the indexing is consistent with how the element 
			// collection is accessed.
			//int i = 0;
			//this.elements = new double[x.Length];
			//foreach (double xi in x)
			//{
			//    this.elements[i++] = xi;
			//}
			this.elements = new double[x.Length];
			Vector size = this.Size;
			for (int i = 0; i < x.Length; i++)
			{
				int[] subscript = Compute.IndexToSubscript(i, size);

				if (subscript.Length == 2 && x.Rank == 1)
					this.elements[i] = (double)x.GetValue(subscript[1]);
				else
					this.elements[i] = (double)x.GetValue(subscript);
			}
		}

        #endregion //Constructors

		#region Base Class Overrides

		#region CopyTo

		/// <summary>
		/// Copies the base <see cref="Matrix"/> to an <paramref name="array"/> starting at a 
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
		/// Copies the base <see cref="Matrix"/> to an <paramref name="array"/> starting at a 
		/// particular <paramref name="index"/>.
		/// </summary>
		/// <param name="array">The destination array.</param>
		/// <param name="index">An index into the destination array where the copying begins.</param>
		public void CopyTo(double[] array, int index)
		{
			this.CopyTo((Array)array, index);
		}

		#endregion //CopyTo

		#region Equals

		/// <summary>
		/// Compares the <see cref="Matrix"/> with <paramref name="x"/> for equality.
		/// </summary>
		/// <param name="x">An object.</param>
		/// <returns>Returns True if the Matrix is equal to x; False otherwise.</returns>
		public override bool Equals(object x)
		{
			Matrix xCast = x as Matrix;
			if (object.Equals(x, null))
				return false;

			return this == xCast;
		}

		#endregion Equals

		#region GetHashCode

		/// <summary>
		/// Returns hash code for the <see cref="Matrix"/>.
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
		/// Returns the string representation of a <see cref="Matrix"/>.
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
				result.Append(Matrix.ToString_HelperSubscript(subscript));

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
			if(this.Rank > 2)
				subscript.CopyTo(fullSubscript, 2);

			StringBuilder result = new StringBuilder(4 * this.Dimensions[0] * this.Dimensions[1]);
			result.Append("\n\n( ");

			for (int i = 0, iLength = this.Dimensions[0], jLength = this.Dimensions[1]; i < iLength; i++)
			{
				if(i > 0)
					result.Append("  ");

				fullSubscript[0] = i;
				for (int j = 0; j < jLength; j++)
				{
					fullSubscript[1] = j;
					result.Append(this[fullSubscript]);
					if(j < jLength - 1)
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
		/// Returns an double enumerator for the <see cref="Matrix"/>.
		/// </summary>
		IEnumerator<double> IEnumerable<double>.GetEnumerator()
		{
			return ((IEnumerable<double>)this.elements).GetEnumerator();
		}

		#endregion //IEnumerable<double> Members

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the <see cref="Matrix"/>.
		/// </summary>
		public override IEnumerator GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		#endregion //IEnumerable Members

		#region ICloneable Members


		/// <summary>
		/// Returns a copy of the <see cref="Matrix"/>.
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
		/// Adds two <see cref="Matrix"/> instances pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] + x2[i].</returns>
		public static Matrix operator +(Matrix x1, Matrix x2)
		{
			return Matrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="Matrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] + x2.</returns>
		public static Matrix operator +(Matrix x1, double x2)
		{
			return Matrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a double and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1 + x2[i].</returns>
		public static Matrix operator +(double x1, Matrix x2)
		{
			return Matrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="Matrix"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A <see cref="ComplexMatrix"/> y, where y[i] = x1[i] + x2.</returns>
		public static ComplexMatrix operator +(Matrix x1, Complex x2)
		{
			return Matrix.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="Complex"/> number and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A <see cref="ComplexMatrix"/> y, where y[i] = x1 + x2[i].</returns>
		public static ComplexMatrix operator +(Complex x1, Matrix x2)
		{
			return Matrix.Add(x1, x2);
		}

		#endregion // +

		#region -

		/// <summary>
		/// Subtracts two <see cref="Matrix"/> instances pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] - x2[i].</returns>
		public static Matrix operator -(Matrix x1, Matrix x2)
		{
			return Matrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="Matrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] - x2.</returns>
		public static Matrix operator -(Matrix x1, double x2)
		{
			return Matrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a double and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1 - x2[i].</returns>
		public static Matrix operator -(double x1, Matrix x2)
		{
			return Matrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="Matrix"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] - x2.</returns>
		public static ComplexMatrix operator -(Matrix x1, Complex x2)
		{
			return Matrix.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="Complex"/> number and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1 - x2[i].</returns>
		public static ComplexMatrix operator -(Complex x1, Matrix x2)
		{
			return Matrix.Subtract(x1, x2);
		}

		#endregion // -

		#region *

		/// <summary>
		/// Multiplies two <see cref="Matrix"/> instances pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] * x2[i].</returns>
		public static Matrix operator *(Matrix x1, Matrix x2)
		{
			return Matrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="Matrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] * x2.</returns>
		public static Matrix operator *(Matrix x1, double x2)
		{
			return Matrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a double and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1 * x2[i].</returns>
		public static Matrix operator *(double x1, Matrix x2)
		{
			return Matrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="Matrix"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] * x2.</returns>
		public static ComplexMatrix operator *(Matrix x1, Complex x2)
		{
			return Matrix.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="Complex"/> number and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1 * x2[i].</returns>
		public static ComplexMatrix operator *(Complex x1, Matrix x2)
		{
			return Matrix.Multiply(x1, x2);
		}

		#endregion // *

		#region /

		/// <summary>
		/// Divides two <see cref="Matrix"/> instances pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] / x2[i].</returns>
		public static Matrix operator /(Matrix x1, Matrix x2)
		{
			return Matrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="Matrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] / x2.</returns>
		public static Matrix operator /(Matrix x1, double x2)
		{
			return Matrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a double and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1 / x2[i].</returns>
		public static Matrix operator /(double x1, Matrix x2)
		{
			return Matrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="Matrix"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] / x2.</returns>
		public static ComplexMatrix operator /(Matrix x1, Complex x2)
		{
			return Matrix.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="Complex"/> number and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1 / x2[i].</returns>
		public static ComplexMatrix operator /(Complex x1, Matrix x2)
		{
			return Matrix.Divide(x1, x2);
		}

		#endregion // /

		#region %

		/// <summary>
		/// Returns the modulus of two <see cref="Matrix"/> instances pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] % x2[i].</returns>
		public static Matrix operator %(Matrix x1, Matrix x2)
		{
			return Matrix.Mod(x1, x2);
		}

		/// <summary>
		/// Returns the modulus of a <see cref="Matrix"/> and a double pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1[i] % x2.</returns>
		public static Matrix operator %(Matrix x1, double x2)
		{
			return Matrix.Mod(x1, x2);
		}

		/// <summary>
		/// Returns the modulus of a double and a <see cref="Matrix"/> pointwise.
		/// </summary>
		/// <returns>A Matrix y, where y[i] = x1 % x2[i].</returns>
		public static Matrix operator %(double x1, Matrix x2)
		{
			return Matrix.Mod(x1, x2);
		}

		#endregion // %

		#region ==

        /// <summary>
        /// Determines whether two <see cref="Matrix"/> instances have the same dimensions and element values.
        /// </summary>
        /// <param name="x1">The first Matrix.</param>
        /// <param name="x2">The second Matrix.</param>
        /// <returns>Returns True if the two Matrix instances are equal; False otherwise.</returns>
		public static bool operator ==(Matrix x1, Matrix x2)
		{
			if (!(x1.Size == x2.Size))
				return false;

			int length = x1.Length;
			for (int i = 0; i < length; i++)
			{
				if(x1.Elements[i] != x2.Elements[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns the pointwise equality operator for a <see cref="Matrix"/> and a double.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = Equals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator ==(Matrix x1, double x2)
		public static BooleanMatrix operator ==(Matrix x1, double x2)
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
		/// Returns the pointwise equality operator for a double and a <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = Equals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator ==(double x1, Matrix x2)
		public static BooleanMatrix operator ==(double x1, Matrix x2)
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
		/// Returns the pointwise equality operator for a <see cref="Matrix"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = Equals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator ==(Matrix x1, Complex x2)
		public static BooleanMatrix operator ==(Matrix x1, Complex x2)
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
		/// Returns the pointwise equality operator for a <see cref="Complex"/> number and a <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = Equals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator ==(Complex x1, Matrix x2)
		public static BooleanMatrix operator ==(Complex x1, Matrix x2)
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
		/// Determines whether two <see cref="Matrix"/> instances have different dimensions or element values.
		/// </summary>
		/// <param name="x1">The first Matrix.</param>
		/// <param name="x2">The second Matrix.</param>
		/// <returns>Returns True if the two Matrix instances are unequal; False otherwise.</returns>
		public static bool operator !=(Matrix x1, Matrix x2)
		{
			return !(x1 == x2);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a <see cref="Matrix"/> and a double.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = NotEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator !=(Matrix x1, double x2)
		public static BooleanMatrix operator !=(Matrix x1, double x2)
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
		/// Returns the pointwise inequality operator for a double and a <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = NotEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator !=(double x1, Matrix x2)
		public static BooleanMatrix operator !=(double x1, Matrix x2)
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
		/// Returns the pointwise inequality operator for a <see cref="Matrix"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = NotEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator !=(Matrix x1, Complex x2)
		public static BooleanMatrix operator !=(Matrix x1, Complex x2)
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
		/// Returns the pointwise inequality operator for a <see cref="Complex"/> number and a <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A new <see cref="BooleanMatrix"/> y, where y[i] = NotEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator !=(Complex x1, Matrix x2)
		public static BooleanMatrix operator !=(Complex x1, Matrix x2)
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
		/// Compares two <see cref="Matrix"/> instances using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">The first Matrix.</param>
		/// <param name="x2">The second Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(Matrix x1, Matrix x2)
		public static BooleanMatrix operator >(Matrix x1, Matrix x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(Matrix x1, ComplexMatrix x2)
		public static BooleanMatrix operator >(Matrix x1, ComplexMatrix x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(ComplexMatrix x1, Matrix x2)
		public static BooleanMatrix operator >(ComplexMatrix x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a double using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(Matrix x1, double x2)
		public static BooleanMatrix operator >(Matrix x1, double x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="Matrix"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(double x1, Matrix x2)
		public static BooleanMatrix operator >(double x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a <see cref="Complex"/> number using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(Matrix x1, Complex x2)
		public static BooleanMatrix operator >(Matrix x1, Complex x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="Matrix"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThan(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >(Complex x1, Matrix x2)
		public static BooleanMatrix operator >(Complex x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.LessThanZero);
		}

		#endregion // >

		#region >=

		/// <summary>
		/// Compares two <see cref="Matrix"/> instances using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">The first Matrix.</param>
		/// <param name="x2">The second Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(Matrix x1, Matrix x2)
		public static BooleanMatrix operator >=(Matrix x1, Matrix x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(Matrix x1, ComplexMatrix x2)
		public static BooleanMatrix operator >=(Matrix x1, ComplexMatrix x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(ComplexMatrix x1, Matrix x2)
		public static BooleanMatrix operator >=(ComplexMatrix x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a double using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(Matrix x1, double x2)
		public static BooleanMatrix operator >=(Matrix x1, double x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="Matrix"/> using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(double x1, Matrix x2)
		public static BooleanMatrix operator >=(double x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a <see cref="Complex"/> number using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(Matrix x1, Complex x2)
		public static BooleanMatrix operator >=(Matrix x1, Complex x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="Matrix"/> using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = GreaterThanOrEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator >=(Complex x1, Matrix x2)
		public static BooleanMatrix operator >=(Complex x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		#endregion // >=

		#region <

		/// <summary>
		/// Compares two <see cref="Matrix"/> instances using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">The first Matrix.</param>
		/// <param name="x2">The second Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(Matrix x1, Matrix x2)
		public static BooleanMatrix operator <(Matrix x1, Matrix x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(Matrix x1, ComplexMatrix x2)
		public static BooleanMatrix operator <(Matrix x1, ComplexMatrix x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(ComplexMatrix x1, Matrix x2)
		public static BooleanMatrix operator <(ComplexMatrix x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a double using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(Matrix x1, double x2)
		public static BooleanMatrix operator <(Matrix x1, double x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="Matrix"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(double x1, Matrix x2)
		public static BooleanMatrix operator <(double x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a <see cref="Complex"/> number using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(Matrix x1, Complex x2)
		public static BooleanMatrix operator <(Matrix x1, Complex x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="Matrix"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThan(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <(Complex x1, Matrix x2)
		public static BooleanMatrix operator <(Complex x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanZero);
		}

		#endregion // <

		#region <=

		/// <summary>
		/// Compares two <see cref="Matrix"/> instances using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">The first Matrix.</param>
		/// <param name="x2">The second Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(Matrix x1, Matrix x2)
		public static BooleanMatrix operator <=(Matrix x1, Matrix x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a <see cref="ComplexMatrix"/> using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(Matrix x1, ComplexMatrix x2)
		public static BooleanMatrix operator <=(Matrix x1, ComplexMatrix x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexMatrix"/> and a <see cref="Matrix"/> using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1[i],x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(ComplexMatrix x1, Matrix x2)
		public static BooleanMatrix operator <=(ComplexMatrix x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a double using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(Matrix x1, double x2)
		public static BooleanMatrix operator <=(Matrix x1, double x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="Matrix"/> using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(double x1, Matrix x2)
		public static BooleanMatrix operator <=(double x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Matrix"/> and a <see cref="Complex"/> number using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1[i],x2).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(Matrix x1, Complex x2)
		public static BooleanMatrix operator <=(Matrix x1, Complex x2)
		{
			Matrix x = x1.Clone().CompareTo(x2);

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
			return Matrix.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="Matrix"/> using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A <see cref="BooleanMatrix"/> y, where y[i] = LessThanOrEquals(x1,x2[i]).</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector operator <=(Complex x1, Matrix x2)
		public static BooleanMatrix operator <=(Complex x1, Matrix x2)
		{
			Matrix x = x2.Clone().CompareTo(x1);

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
			return Matrix.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		#endregion // <=

		#region Casting

		/// <summary>
		/// Casts a <see cref="Vector"/> to a <see cref="Matrix"/>. 
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Matrix y, where y[i] = x[i].</returns>
		public static explicit operator Matrix(Vector x)
		{
			if (x.IsEmpty())
				return Matrix.Empty;

			return new Matrix(x.Elements, x.Dimensions);
		}

		/// <summary>
		/// Casts a <see cref="Matrix"/> to a <see cref="Vector"/>. 
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Vector y, where y[i] = x[i].</returns>
		public static explicit operator Vector(Matrix x)
		{
			if (!x.IsColumn() && !x.IsRow())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_58"));

			if (x.IsEmpty())
				return Vector.Empty;

			return new Vector(x.Elements, x.Dimensions);
		}

		/// <summary>
		/// Casts a double to a unitary <see cref="Matrix"/>. 
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>A unitary Vector y, where y[0] = x.</returns>
		public static implicit operator Matrix(double x)
		{
			return new Matrix(x, 1);
		}

		/// <summary>
		/// Casts a double array to a row <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">A double array.</param>
		/// <returns>A row Matrix y, where y[i] = x[i].</returns>
		public static implicit operator Matrix(double[] x)
		{
			return new Matrix(x);
		}

		/// <summary>
		/// Casts an int array to a row <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">An int array.</param>
		/// <returns>A row Matrix y, where y[i] = (double)x[i].</returns>
		public static explicit operator Matrix(int[] x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new Matrix(newElements);
		}

		/// <summary>
		/// Casts a short array to a row <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">A short array.</param>
		/// <returns>A row Matrix y, where y[i] = (double)x[i].</returns>
		public static explicit operator Matrix(short[] x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new Matrix(newElements);
		}

		/// <summary>
		/// Casts a long array to a row <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">A uint array.</param>
		/// <returns>A row Matrix y, where y[i] = (double)x[i].</returns>
		public static explicit operator Matrix(long[] x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new Matrix(newElements);
		}

		/// <summary>
		/// Casts a float array to a row <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">A float array.</param>
		/// <returns>A row Matrix y, where y[i] = (double)x[i].</returns>
		public static explicit operator Matrix(float[] x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new Matrix(newElements);
		}

		/// <summary>
		/// Casts a decimal array to a row <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">A decimal array.</param>
		/// <returns>A row Matrix y, where y[i] = (double)x[i].</returns>
		public static explicit operator Matrix(decimal[] x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = (double)x[i];
			}
			return new Matrix(newElements);
		}

		#endregion //Casting

		#endregion //Operators

		#region Methods

		#region Basic Math Functions

		#region Abs

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Abs(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Abs(Matrix)"/>
		public Matrix Abs()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Abs(this.elements[i]);
			}
			return this;
		}

		#endregion //Abs

		#region Acos

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Acos(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Acos(Matrix)"/>
		public Matrix Acos()
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
		/// Modifies the <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] + x[i].
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the Matrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public Matrix Add(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_59"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] + x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] + x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified Matrix.</returns>
		public Matrix Add(double x)
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

		internal static Matrix Add(Matrix x1, Matrix x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Add(Matrix x1, double x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Add(double x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new Matrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<double> newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 + x2.elements[i];
			}

			return new Matrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(Matrix x1, Complex x2)
		{
			if (x1.IsEmpty())
				return new ComplexMatrix();

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] + x2;
			}

			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Add(Complex x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 + x2[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Add

		#region Arg

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Arg(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Arg(Matrix)"/>
		public Matrix Arg()
		{
			if (this.IsEmpty())
				return this;

			int length = this.Length;
			for (int i = 0; i < length; i++)
			{
				this.elements[i] = 0;
			}

			return this;
		}

		#endregion //Arg

		#region Asin

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Asin(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Asin(Matrix)"/>
		public Matrix Asin()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Atan(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Atan(Matrix)"/>
		public Matrix Atan()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Ceiling(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Ceiling(Matrix)"/>
		public Matrix Ceiling()
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

		#region CompareTo

		/// <summary>
		/// Modifies the <see cref="Matrix"/>, y, by replacing each element y[i] with CompareTo(y[i],x[i]).
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the Matrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public Matrix CompareTo(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_59"));

			double thisCurr;
			double xCurr;
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
		/// Modifies the <see cref="Matrix"/>, y, by replacing each element y[i] with CompareTo(y[i],x[i]).
		/// </summary>
		/// <param name="x">A <see cref="ComplexMatrix"/>.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the Matrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public Matrix CompareTo(ComplexMatrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_59"));

			double thisCurr;
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
		/// Modifies the <see cref="Matrix"/>, y, by replacing each element y[i] with CompareTo(y[i],x).
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified Matrix.</returns>
		public Matrix CompareTo(double x)
		{
			if (this.IsEmpty())
				return this;

			double thisCurr;
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
		/// Modifies the <see cref="Matrix"/>, y, by replacing each element y[i] with CompareTo(y[i],x).
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified Matrix.</returns>
		public Matrix CompareTo(Complex x)
		{
			if (this.IsEmpty())
				return this;

			double thisCurr;
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

		#region Cos

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Cos(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Cos(Matrix)"/>
		public Matrix Cos()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Cosh(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Cosh(Matrix)"/>
		public Matrix Cosh()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] / x[i].
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the Matrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public Matrix Divide(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_59"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] / x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] / x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified Matrix.</returns>
		public Matrix Divide(double x)
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

		internal static Matrix Divide(Matrix x1, Matrix x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Divide(Matrix x1, double x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Divide(double x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new Matrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<double> newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 / x2.elements[i];
			}

			return new Matrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(Matrix x1, Complex x2)
		{
			if (x1.IsEmpty())
				return new ComplexMatrix();

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] / x2;
			}

			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Divide(Complex x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 / x2[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Divide

		#region Exp

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Exp(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Exp(Matrix)"/>
		public Matrix Exp()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Floor(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Floor(Matrix)"/>
		public Matrix Floor()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Log(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Log(Matrix)"/>
		public Matrix Log()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Log(y[i],B).
		/// </summary>
		/// <param name="B">The base of the logarithm.</param>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Log(Matrix, double)"/>
		public Matrix Log(double B)
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Log2(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Log2(Matrix)"/>
		public Matrix Log2()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Log10(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Log10(Vector)"/>
		public Matrix Log10()
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

		#region Mod

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] % x[i].
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the Matrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public Matrix Mod(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_59"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] % x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] % x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified Matrix.</returns>
		public Matrix Mod(double x)
		{
			if (this.IsEmpty())
				return this;

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] % x;
			}
			return this;
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Mod(Matrix x1, Matrix x2)
		{
			return x1.Clone().Mod(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Mod(Matrix x1, double x2)
		{
			return x1.Clone().Mod(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Mod(double x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new Matrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<double> newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 % x2.elements[i];
			}

			return new Matrix(newElements, newDimensions);
		}

		#endregion //Mod

		#region Multiply

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] * x[i].
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the Matrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public Matrix Multiply(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_59"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] * x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] * x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified Matrix.</returns>
		public Matrix Multiply(double x)
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

		internal static Matrix Multiply(Matrix x1, Matrix x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Multiply(Matrix x1, double x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Multiply(double x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new Matrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<double> newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 * x2.elements[i];
			}

			return new Matrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(Matrix x1, Complex x2)
		{
			if (x1.IsEmpty())
				return new ComplexMatrix();

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] * x2;
			}

			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Multiply(Complex x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 * x2[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Multiply

		#region Pow

		/// <summary>
		/// Modifies the <see cref="Matrix"/>, y, by replacing each element y[i] with Pow(y[i],x[i]).
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the Matrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		/// <seealso cref="Compute.Pow(Matrix,Matrix)"/>
		public Matrix Pow(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_59"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Pow(this.elements[i], x.elements[i]);
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Pow(y[i],x).
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Pow(Matrix,double)"/>
		public Matrix Pow(double x)
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Round(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Round(Matrix)"/>
		public Matrix Round()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Sign(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Sign(Matrix)"/>
		public Matrix Sign()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Sin(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Sin(Matrix)"/>
		public Matrix Sin()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Sinh(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Sinh(Matrix)"/>
		public Matrix Sinh()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Sqrt(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Sqrt(Matrix)"/>
		public Matrix Sqrt()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] - x[i].
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the Matrix and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public Matrix Subtract(Matrix x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_59"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] - x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with y[i] - x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified Matrix.</returns>
		public Matrix Subtract(double x)
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

		internal static Matrix Subtract(Matrix x1, Matrix x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Subtract(Matrix x1, double x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Matrix Subtract(double x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new Matrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<double> newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 - x2.elements[i];
			}

			return new Matrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(Matrix x1, Complex x2)
		{
			if (x1.IsEmpty())
				return new ComplexMatrix();

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] - x2;
			}

			return new ComplexMatrix(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexMatrix Subtract(Complex x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 - x2[i];
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Subtract

		#region Tan

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Tan(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Tan(Matrix)"/>
		public Matrix Tan()
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i] with Tanh(y[i]).
		/// </summary>
		/// <returns>The modified Matrix.</returns>
		/// <seealso cref="Compute.Tanh(Matrix)"/>
		public Matrix Tanh()
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

		#region Matrix Functions

		#region Bin

		/// <summary>
		/// Modifies the <see cref="Matrix"/> by replacing its element with a bin number.
		/// </summary>
		/// <param name="N">The number of evenly-spaced bins.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than one.
		/// </exception>
		public Matrix Bin(int N)
		{
			if (N < 1)
				throw new ArgumentException("There cannot be less than one bin", "N");

			if (this.IsEmpty())
				return this;

			double min = Compute.Min(this);
			double max = Compute.Max(this);
			double edge;
			if (max == min)
				edge = (double)1 / (double)N;
			else
				edge = (max - min) / (2 * N);

			Vector bins = Compute.Line(min - edge, max + edge, N + 1);
			return this.Bin(bins);
		}

		/// <summary>
		/// Modifies the <see cref="Matrix"/> by replacing each element with a bin number. 
		/// </summary>
		/// <param name="edges">edges[i] and edges[i+1] are the edges of the ith bin.</param>
		/// <returns>The modified Matrix.</returns>
		/// <remarks>
		/// If an element of the Matrix is not in any bin, it is replaced by Constant.NaN.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when there are less than two bin <paramref name="edges"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the bin edges are not in increasing order.
		/// </exception>
		public Matrix Bin(Vector edges)
		{
			int binNumber = edges.Length;
			if (binNumber < 2)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_60"), "edges");

			for (int k = 1; k < binNumber; k++)
			{
				if (edges[k - 1] > edges[k])
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_62"), "bins");
			}

			if (this.IsEmpty())
				return this;

			double current;
			bool done = false;
			int length = this.Length;
			double NaN = Constant.NaN;
			for (int i = 0; i < length; i++)
			{
				current = this.elements[i];

				done = false;
				for (int k = 0, bound = binNumber - 1; k < bound; k++)
				{
					if (current >= edges[k] && current <= edges[k + 1])
					{
						this.elements[i] = k;
						done = true;
						break;
					}
				}

				if (!done)
					this.elements[i] = NaN;
			}

			return this;
		}

		#endregion //Bin

		#region Clone

		/// <summary>
		/// Returns a copy of the <see cref="Matrix"/> instance.
		/// </summary>
		public Matrix Clone()
		{
			if (this.IsEmpty())
				return new Matrix();

			return new Matrix(Utilities.Clone<double>(this.elements), this.Dimensions);
		}

		#endregion //Clone

		#region CumProduct

		/// <summary>
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i1,..,iN,..,iM] with y[i1,..,0,..,iM]*...*y[i1,..,iN,..,iM].
		/// </summary>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <seealso cref="Compute.CumProduct(Matrix,int)"/>
		public Matrix CumProduct(int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (this.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_16"));

			if (N > this.Rank)
				return this.Clone();

			int Nm1 = N-1;
			Vector size = Compute.Size(this);

			int[] newDimensions = (int[])this.Dimensions.Clone();
			int lengthN = this.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);

			int index;
			double curr;
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
		/// Modifies a <see cref="Matrix"/>, y, by replacing each element y[i1,..,iN,..,iM] with y[i1,..,0,..,iM]+...+y[i1,..,iN,..,iM].
		/// </summary>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>The modified Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <seealso cref="Compute.CumSum(Matrix,int)"/>
		public Matrix CumSum(int N)
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
			double curr;
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

		#endregion //Matrix Functions

		#region Private Methods

		// MD 4/19/11 - TFS72396
		#region FindValues

		private static BooleanMatrix FindValues(Matrix x, FindValuesType type)
		{
			int length = x.Length;
			bool[] newElements = new bool[length];

			switch (type)
			{
				case FindValuesType.GreaterThanZero:
					for (int i = 0; i < length; i++)
						newElements[i] = x.elements[i] > 0;
					break;

				case FindValuesType.GreaterThanOrEqualToZero:
					for (int i = 0; i < length; i++)
						newElements[i] = x.elements[i] >= 0;
					break;

				case FindValuesType.LessThanZero:
					for (int i = 0; i < length; i++)
						newElements[i] = x.elements[i] < 0;
					break;

				case FindValuesType.LessThanOrEqualToZero:
					for (int i = 0; i < length; i++)
						newElements[i] = x.elements[i] <= 0;
					break;

				default:
					for (int i = 0; i < length; i++)
						newElements[i] = false;
					Utilities.DebugFail("Unknown FindValuesType: " + type);
					break;
			}

			// MD 4/26/11 - TFS73678
			// The result matrix should have the same dimensions as the original.
			//return new BooleanMatrix(newElements);
			return new BooleanMatrix(newElements, x.Dimensions);
		}

		#endregion  // FindValues

		#endregion  // Private Methods

		#endregion //Methods

		#region Properties

		#region Elements






		internal IList<double> Elements
		{
			get { return this.elements; }
		}

		#endregion //Elements

		#region Indexers

		/// <summary>
		/// An indexer for the <see cref="Matrix"/>. The Matrix can be indexed one-dimensionally or multi-dimensionally.
		/// </summary>
		/// <param name="subscript">A sequence of indices that correspond to the dimensions of the Matrix.</param>
		/// <returns>The element specified by the subscript.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the Matrix is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="subscript"/> is non-unitary and has a different number of dimensions than the Matrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index is out of the bounds of the Matrix.
		/// </exception>
		public double this[params int[] subscript]
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
					int index = subscript[0];
					if(index < 0 || index >= this.Length)
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
		/// An indexer for the <see cref="Matrix"/> that takes a series of <see cref="Vector"/> subscripts. The Matrix can 
		/// be indexed one-dimensionally or multi-dimensionally.
		/// </summary>
		/// <param name="subscript">An array of index Vectors.</param>
		/// <returns>A Matrix containing the elements specified by the <paramref name="subscript"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the Matrix is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="subscript"/> is non-unitary and has a different number of dimensions than the Matrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index is out of the bounds of the Matrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index has a non-integer value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the set value does not align with the elements specified by the subscript.
		/// </exception>
		public Matrix this[params Vector[] subscript]
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
						return Matrix.Empty;

					int index;
					double indexCheck;
					int length = indices.Length;
					Matrix result = new Matrix(length);
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

					if(Utilities.ArrayEquals(newDimensions, new int[subscriptLength]))
						return Matrix.Empty;

					if (isEmpty)
						throw new ArgumentException("A subscript Vector is empty", "subscript");

					Vector size = this.Size;
					int length = Utilities.Product(newDimensions);
					double[] newElements = new double[length];
					Matrix result = new Matrix(newElements, newDimensions);

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

						if(valueIsUnitary)
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