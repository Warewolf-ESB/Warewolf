using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections;

namespace Infragistics.Math
{
	/// <summary>
	/// A ComplexVector is a list of <see cref="Complex"/> numbers with an additional dimensionality property that
	/// specifies its orientation.
	/// </summary>
    public class ComplexVector : MatrixBase
		, IEnumerable<Complex>

, ICloneable

	{
		#region Member Variables

		private IList<Complex> elements;

		#endregion //Member Variables

		#region Static Variables

		#region Empty

		/// <summary>
		/// Returns the empty <see cref="ComplexVector"/>.
		/// </summary>
		public static ComplexVector Empty = new ComplexVector();

		#endregion //Empty

		#endregion //Static Variables

		#region Constructors

		/// <summary>
		/// Initializes an empty <see cref="ComplexVector"/> instance.
		/// </summary>
		/// <seealso cref="MatrixBase.IsEmpty"/>
		public ComplexVector()
			: base()
		{
		}

		/// <summary>
		/// Initializes a row zero <see cref="ComplexVector"/> of a specified <paramref name="length"/>. 
		/// </summary>
		/// <param name="length">The length of the constructed ComplexVector.</param>
		public ComplexVector(int length)
			: base(length)
		{
			this.elements = new Complex[length];
		}

		/// <summary>
		/// Initializes a row <see cref="ComplexVector"/> of the specified <paramref name="length"/> and 
		/// the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value assigned to each element of the constructed ComplexVector.</param>
		/// <param name="length">The length of the constructed ComplexVector.</param>
		public ComplexVector(Complex value, int length)
			: base(length)
		{
			this.elements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				this.elements[i] = value;
			}
		}

		/// <summary>
		/// Initializes a row <see cref="ComplexVector"/> with the specified <paramref name="elements"/>.
		/// </summary>
		/// <param name="elements">The elements of the constructed ComplexVector.</param>
		public ComplexVector(IList<Complex> elements)
			: base(elements.Count)
		{
			this.elements = elements;
		}

		/// <summary>
		/// Initializes a <see cref="ComplexVector"/> with the specified <paramref name="elements"/> and <paramref name="dimensions"/>.
		/// </summary>
		/// <param name="elements">The elements of the constructed ComplexVector.</param>
		/// <param name="dimensions">The dimensions of the constructed ComplexVector.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the dimensions specify a size that is inconsistent with the length of the elements.
		/// </exception>
		public ComplexVector(IList<Complex> elements, int[] dimensions)
			: base(dimensions)
		{
			if (!this.IsRow() && !this.IsColumn())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_1"), "dimensions");

			if (elements.Count != this.Length)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_2"));

			this.elements = elements;
		}

		/// <summary>
		/// Initializes a <see cref="ComplexVector"/> with the specified elements <paramref name="x"/>.
		/// </summary>
		/// <param name="type">An enum that specifies the orientation of the ComplexVector.</param>
		/// <param name="x">A Complex array of elements.</param>
		public ComplexVector(VectorType type, params Complex[] x)
			: base(x.Length, type)
		{
			this.elements = x;
		}

		#endregion //Constructors

		#region Base Class Overrides

		#region CopyTo

		/// <summary>
		/// Copies the base <see cref="ComplexVector"/> to an <paramref name="array"/> starting at a 
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
		/// Copies the base <see cref="ComplexVector"/> to an <paramref name="array"/> starting at a 
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
		/// Compares the <see cref="ComplexVector"/> with <paramref name="x"/> for equality.
		/// </summary>
		/// <param name="x">An object.</param>
		/// <returns>Returns True if the ComplexVector is equal to x; False otherwise.</returns>
		public override bool Equals(object x)
		{
			ComplexVector xCast = x as ComplexVector;
			if (object.Equals(x, null))
				return false;

			return this == xCast;
		}

		#endregion Equals

		#region GetHashCode

		/// <summary>
		/// Returns a hash code for the <see cref="ComplexVector"/>.
		/// </summary>
		/// <returns>A hash code.</returns>
		public override int GetHashCode()
		{
			int length = this.Length;
			if (length == 0)
				return 0;

			int lengthCode;
			if (this.Dimensions[0] == 1)
				lengthCode = length;
			else
				lengthCode = length << 16;

			int middleIndex = length / 2;

			return lengthCode
				^ this.elements[0].GetHashCode()
				^ this.elements[middleIndex].GetHashCode()
				^ this.elements[length - 1].GetHashCode();
		}

		#endregion GetHashCode

		#region ToString

		/// <summary>
		/// Returns the string representation of a <see cref="ComplexVector"/>.
		/// </summary>
		public override string ToString()
		{
			string space;
			int length = this.Length;
			if (length == 0)
			{
				return "( )";
			}
			else if (this.IsUnitary())
			{
				return "( " + this.elements[0].ToString() + " )";
			}
			else if (this.IsColumn())
				space = "\n ";
			else
				space = " ";

			StringBuilder returnString = new StringBuilder(length * 4 + 4);
			returnString.Append("( ");
			for (int i = 0; i < length; i++)
			{
				returnString.Append(this.elements[i].ToString());
				returnString.Append(space);
			}
			returnString.Append(")");

			return returnString.ToString();
		}

		#endregion //ToString

		#endregion //Base Class Overrides

		#region Interfaces

		#region IEnumerable<Complex> Members

		/// <summary>
		/// Returns an <see cref="Complex"/> enumerator for the <see cref="ComplexVector"/>.
		/// </summary>
		IEnumerator<Complex> IEnumerable<Complex>.GetEnumerator()
		{
			return ((IEnumerable<Complex>)this.elements).GetEnumerator();
		}

		#endregion //IEnumerable<Complex> Members

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the <see cref="ComplexVector"/>.
		/// </summary>
		public override IEnumerator GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		#endregion //IEnumerable Members

		#region ICloneable Members


		/// <summary>
		/// Returns a copy of the <see cref="ComplexVector"/>.
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
		/// Adds two <see cref="ComplexVector"/> instances pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] + x2[i].</returns>
		public static ComplexVector operator +(ComplexVector x1, ComplexVector x2)
		{
			return ComplexVector.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="ComplexVector"/> and a <see cref="Vector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] + x2[i].</returns>
		public static ComplexVector operator +(ComplexVector x1, Vector x2)
		{
			return ComplexVector.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="Vector"/> and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] + x2[i].</returns>
		public static ComplexVector operator +(Vector x1, ComplexVector x2)
		{
			return ComplexVector.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="ComplexVector"/> and a double pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] + x2.</returns>
		public static ComplexVector operator +(ComplexVector x1, double x2)
		{
			return ComplexVector.Add(x1, x2);
		}

		/// <summary>
		/// Adds a double and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1 + x2[i].</returns>
		public static ComplexVector operator +(double x1, ComplexVector x2)
		{
			return ComplexVector.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="ComplexVector"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] + x2.</returns>
		public static ComplexVector operator +(ComplexVector x1, Complex x2)
		{
			return ComplexVector.Add(x1, x2);
		}

		/// <summary>
		/// Adds a <see cref="Complex"/> number and a <see cref="Vector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1 + x2[i].</returns>
		public static ComplexVector operator +(Complex x1, ComplexVector x2)
		{
			return ComplexVector.Add(x1, x2);
		}

		#endregion // +

		#region -

		/// <summary>
		/// Subtracts two <see cref="ComplexVector"/> instances pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] - x2[i].</returns>
		public static ComplexVector operator -(ComplexVector x1, ComplexVector x2)
		{
			return ComplexVector.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="ComplexVector"/> and a <see cref="Vector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] - x2[i].</returns>
		public static ComplexVector operator -(ComplexVector x1, Vector x2)
		{
			return ComplexVector.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="Vector"/> and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] - x2[i].</returns>
		public static ComplexVector operator -(Vector x1, ComplexVector x2)
		{
			return ComplexVector.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="ComplexVector"/> and a double pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] - x2.</returns>
		public static ComplexVector operator -(ComplexVector x1, double x2)
		{
			return ComplexVector.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a double and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1 - x2[i].</returns>
		public static ComplexVector operator -(double x1, ComplexVector x2)
		{
			return ComplexVector.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="ComplexVector"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] - x2.</returns>
		public static ComplexVector operator -(ComplexVector x1, Complex x2)
		{
			return ComplexVector.Subtract(x1, x2);
		}

		/// <summary>
		/// Subtracts a <see cref="Complex"/> number and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1 - x2[i].</returns>
		public static ComplexVector operator -(Complex x1, ComplexVector x2)
		{
			return ComplexVector.Subtract(x1, x2);
		}

		#endregion // -

		#region *

		/// <summary>
		/// Multiplies two <see cref="ComplexVector"/> instances pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] * x2[i].</returns>
		public static ComplexVector operator *(ComplexVector x1, ComplexVector x2)
		{
			return ComplexVector.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="ComplexVector"/> and a <see cref="Vector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] * x2[i].</returns>
		public static ComplexVector operator *(ComplexVector x1, Vector x2)
		{
			return ComplexVector.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="Vector"/> and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] * x2[i].</returns>
		public static ComplexVector operator *(Vector x1, ComplexVector x2)
		{
			return ComplexVector.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="ComplexVector"/> and a double pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] * x2.</returns>
		public static ComplexVector operator *(ComplexVector x1, double x2)
		{
			return ComplexVector.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a double and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1 * x2[i].</returns>
		public static ComplexVector operator *(double x1, ComplexVector x2)
		{
			return ComplexVector.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="ComplexVector"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] * x2.</returns>
		public static ComplexVector operator *(ComplexVector x1, Complex x2)
		{
			return ComplexVector.Multiply(x1, x2);
		}

		/// <summary>
		/// Multiplies a <see cref="Complex"/> number and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1 * x2[i].</returns>
		public static ComplexVector operator *(Complex x1, ComplexVector x2)
		{
			return ComplexVector.Multiply(x1, x2);
		}

		#endregion // *

		#region /

		/// <summary>
		/// Divides two <see cref="ComplexVector"/> instances pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] / x2[i].</returns>
		public static ComplexVector operator /(ComplexVector x1, ComplexVector x2)
		{
			return ComplexVector.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="ComplexVector"/> and a <see cref="Vector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] / x2[i].</returns>
		public static ComplexVector operator /(ComplexVector x1, Vector x2)
		{
			return ComplexVector.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="Vector"/> and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] / x2[i].</returns>
		public static ComplexVector operator /(Vector x1, ComplexVector x2)
		{
			return ComplexVector.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="ComplexVector"/> and a double pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] / x2.</returns>
		public static ComplexVector operator /(ComplexVector x1, double x2)
		{
			return ComplexVector.Divide(x1, x2);
		}

		/// <summary>
		/// Multiplies a double and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1 / x2[i].</returns>
		public static ComplexVector operator /(double x1, ComplexVector x2)
		{
			return ComplexVector.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="ComplexVector"/> and a <see cref="Complex"/> number pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1[i] / x2.</returns>
		public static ComplexVector operator /(ComplexVector x1, Complex x2)
		{
			return ComplexVector.Divide(x1, x2);
		}

		/// <summary>
		/// Divides a <see cref="Complex"/> number and a <see cref="ComplexVector"/> pointwise.
		/// </summary>
		/// <returns>A ComplexVector y, where y[i] = x1 / x2[i].</returns>
		public static ComplexVector operator /(Complex x1, ComplexVector x2)
		{
			return ComplexVector.Divide(x1, x2);
		}

		#endregion // /

		#region ==

		/// <summary>
		/// Determines whether two <see cref="ComplexVector"/> instances have the same dimensions and element values.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>Returns True if the two ComplexVector instances are equal; False otherwise.</returns>
		public static bool operator ==(ComplexVector x1, ComplexVector x2)
		{
			//Check for the empty condition
			if (x1.IsEmpty() && x2.IsEmpty())
				return true;

			//Check for equivalent dimensions.
			if (x1.Dimensions[0] != x2.Dimensions[0] || x1.Dimensions[1] != x2.Dimensions[1])
				return false;

			//Check for equivalent elements.
			for (int i = 0, length = x1.Length; i < length; i++)
			{
				if (x1.elements[i] != x2.elements[i])
					return false;
			}

			//If it hasn't returned false yet, the vectors are equal.
			return true;
		}

		/// <summary>
		/// Determines whether a <see cref="ComplexVector"/> and a <see cref="Vector"/> have the same dimensions 
		/// and element values.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>Returns True if the ComplexVector and the Vector are equal; False otherwise.</returns>
		public static bool operator ==(ComplexVector x1, Vector x2)
		{
			return x1 == (ComplexVector)x2;
		}

		/// <summary>
		/// Determines whether a <see cref="Vector"/> and a <see cref="ComplexVector"/> have the same dimensions 
		/// and element values.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>Returns True if the Vector and the ComplexVector are equal; False otherwise.</returns>
		public static bool operator ==(Vector x1, ComplexVector x2)
		{
			return (ComplexVector)x1 == x2;
		}

		/// <summary>
		/// Returns the pointwise equality operator for a <see cref="ComplexVector"/> and a double.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A new <see cref="BooleanVector"/> y, where y[i] = Equals(x1[i],x2).</returns>
		public static BooleanVector operator ==(ComplexVector x1, double x2)
		{
			if (x1.IsEmpty())
				return BooleanVector.Empty;

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] == x2;
			}

			return new BooleanVector(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise equality operator for a double and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A new <see cref="BooleanVector"/> y, where y[i] = Equals(x1,x2[i]).</returns>
		public static BooleanVector operator ==(double x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return BooleanVector.Empty;

			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 == x2.elements[i];
			}

			return new BooleanVector(newElements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise equality operator for a <see cref="ComplexVector"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A new <see cref="BooleanVector"/> y, where y[i] = Equals(x1[i],x2).</returns>
		public static BooleanVector operator ==(ComplexVector x1, Complex x2)
		{
			if (x1.IsEmpty())
				return BooleanVector.Empty;

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] == x2;
			}

			return new BooleanVector(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise equality operator for a <see cref="Complex"/> number and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A new <see cref="BooleanVector"/> y, where y[i] = Equals(x1,x2[i]).</returns>
		public static BooleanVector operator ==(Complex x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return BooleanVector.Empty;

			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 == x2.elements[i];
			}

			return new BooleanVector(newElements, x2.Dimensions);
		}

		#endregion // ==

		#region !=

		/// <summary>
		/// Determines whether two <see cref="ComplexVector"/> instances have different dimensions and element values.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>Returns True if the two ComplexVector instances are unequal; False otherwise.</returns>
		public static bool operator !=(ComplexVector x1, ComplexVector x2)
		{
			return !(x1 == x2);
		}

		/// <summary>
		/// Determines whether a <see cref="ComplexVector"/> and a <see cref="Vector"/> have different dimensions 
		/// and element values.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>Returns True if the ComplexVector and the Vector are unequal; False otherwise.</returns>
		public static bool operator !=(ComplexVector x1, Vector x2)
		{
			return x1 != (ComplexVector)x2;
		}

		/// <summary>
		/// Determines whether a <see cref="Vector"/> and a <see cref="ComplexVector"/> have different dimensions 
		/// and element values.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>Returns True if the Vector and the ComplexVector are unequal; False otherwise.</returns>
		public static bool operator !=(Vector x1, ComplexVector x2)
		{
			return (ComplexVector)x1 != x2;
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a <see cref="ComplexVector"/> and a double.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A new <see cref="BooleanVector"/> y, where y[i] = NotEquals(x1[i],x2).</returns>
		public static BooleanVector operator !=(ComplexVector x1, double x2)
		{
			if (x1.IsEmpty())
			    return BooleanVector.Empty;
			
			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
			    newElements[i] = x1.elements[i] != x2;
			}

			return new BooleanVector(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a double and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A new <see cref="BooleanVector"/> y, where y[i] = NotEquals(x1,x2[i]).</returns>
		public static BooleanVector operator !=(double x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
			    return BooleanVector.Empty;
			
			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
			    newElements[i] = x1 != x2.elements[i];
			}

			return new BooleanVector(newElements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a <see cref="ComplexVector"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A new <see cref="BooleanVector"/> y, where y[i] = NotEquals(x1[i],x2).</returns>
		public static BooleanVector operator !=(ComplexVector x1, Complex x2)
		{
			if (x1.IsEmpty())
			    return BooleanVector.Empty;
			
			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
			    newElements[i] = x1.elements[i] != x2;
			}

			return new BooleanVector(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a <see cref="Complex"/> number and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A new <see cref="BooleanVector"/> y, where y[i] = NotEquals(x1,x2[i]).</returns>
		public static BooleanVector operator !=(Complex x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
			    return BooleanVector.Empty;
			
			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
			    newElements[i] = x1 != x2.elements[i];
			}

			return new BooleanVector(newElements, x2.Dimensions);
		}

		#endregion // !=

		#region >

		/// <summary>
		/// Compares two <see cref="ComplexVector"/> instances using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1[i],x2[i]).</returns>
		public static BooleanVector operator >(ComplexVector x1, ComplexVector x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a double using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1[i],x2).</returns>
		public static BooleanVector operator >(ComplexVector x1, double x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="ComplexVector"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1,x2[i]).</returns>
		public static BooleanVector operator >(double x1, ComplexVector x2)
		{
			ComplexVector x = x2.Clone().CompareTo(x1);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a <see cref="Complex"/> number using the GreaterThan 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1[i],x2).</returns>
		public static BooleanVector operator >(ComplexVector x1, Complex x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="ComplexVector"/> using the GreaterThan 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1,x2[i]).</returns>
		public static BooleanVector operator >(Complex x1, ComplexVector x2)
		{
			ComplexVector x = x2.Clone().CompareTo(x1);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanZero);
		}

		#endregion // >

		#region >=

		/// <summary>
		/// Compares two <see cref="ComplexVector"/> instances using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2[i]).</returns>
		public static BooleanVector operator >=(ComplexVector x1, ComplexVector x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a double using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2).</returns>
		public static BooleanVector operator >=(ComplexVector x1, double x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="ComplexVector"/> using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1,x2[i]).</returns>
		public static BooleanVector operator >=(double x1, ComplexVector x2)
		{
			ComplexVector x = x2.Clone().CompareTo(x1);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a <see cref="Complex"/> number using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2).</returns>
		public static BooleanVector operator >=(ComplexVector x1, Complex x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="ComplexVector"/> using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1,x2[i]).</returns>
		public static BooleanVector operator >=(Complex x1, ComplexVector x2)
		{
			ComplexVector x = x2.Clone().CompareTo(x1);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		#endregion // >=

		#region <

		/// <summary>
		/// Compares two <see cref="ComplexVector"/> instances using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1[i],x2[i]).</returns>
		public static BooleanVector operator <(ComplexVector x1, ComplexVector x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a double using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1[i],x2).</returns>
		public static BooleanVector operator <(ComplexVector x1, double x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="ComplexVector"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1,x2[i]).</returns>
		public static BooleanVector operator <(double x1, ComplexVector x2)
		{
			ComplexVector x = x2.Clone().CompareTo(x1);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a <see cref="Complex"/> number using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1[i],x2).</returns>
		public static BooleanVector operator <(ComplexVector x1, Complex x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="ComplexVector"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1,x2[i]).</returns>
		public static BooleanVector operator <(Complex x1, ComplexVector x2)
		{
			ComplexVector x = x2.Clone().CompareTo(x1);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		#endregion // <

		#region <=

		/// <summary>
		/// Compares two <see cref="ComplexVector"/> instances using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1[i],x2[i]).</returns>
		public static BooleanVector operator <=(ComplexVector x1, ComplexVector x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a double using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1[i],x2).</returns>
		public static BooleanVector operator <=(ComplexVector x1, double x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="ComplexVector"/> using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1,x2[i]).</returns>
		public static BooleanVector operator <=(double x1, ComplexVector x2)
		{
			ComplexVector x = x2.Clone().CompareTo(x1);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a <see cref="Complex"/> number using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1[i],x2).</returns>
		public static BooleanVector operator <=(ComplexVector x1, Complex x2)
		{
			ComplexVector x = x1.Clone().CompareTo(x2);

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
			return ComplexVector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="ComplexVector"/> using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1,x2[i]).</returns>
		public static BooleanVector operator <=(Complex x1, ComplexVector x2)
		{
			ComplexVector x = x2.Clone().CompareTo(x1);

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
			return ComplexVector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		#endregion // <=

		#region Casting

		/// <summary>
		/// Casts a <see cref="Complex"/> number to a unitary <see cref="ComplexVector"/>. 
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>A unitary ComplexVector y, where y[0] = x.</returns>
		public static implicit operator ComplexVector(Complex x)
		{
			return new ComplexVector(x, 1);
		}

		/// <summary>
		/// Casts a double to a unitary <see cref="ComplexVector"/>. 
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>A unitary ComplexVector y, where y[0] = x.</returns>
		public static implicit operator ComplexVector(double x)
		{
			return new ComplexVector(x, 1);
		}

		/// <summary>
		/// Casts a <see cref="ComplexVector"/> to a Complex array. 
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A <see cref="Complex"/> array y, where y[i] = x[i].</returns>
		public static explicit operator Complex[](ComplexVector x)
		{
			int length = x.Length;
			Complex[] result = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				result[i] = x[i];
			}
			return result;
		}

		/// <summary>
		/// Casts a Complex array to a row <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> array.</param>
		/// <returns>A row ComplexVector y, where y[i] = x[i].</returns>
		public static implicit operator ComplexVector(Complex[] x)
		{
			return new ComplexVector(x);
		}

		/// <summary>
		/// Casts a double array to a row <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">A double array.</param>
		/// <returns>A row ComplexVector y, where y[i] = (Complex)x[i].</returns>
		public static implicit operator ComplexVector(double[] x)
		{
			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexVector(newElements, new int[] { 1, x.Length });
		}

		/// <summary>
		/// Casts a <see cref="Vector"/> to a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A row ComplexVector y, where y[i] = (Complex)x[i].</returns>
		public static implicit operator ComplexVector(Vector x)
		{
			if (x.IsEmpty())
				return ComplexVector.Empty;

			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexVector(newElements, x.Dimensions);
		}

		/// <summary>
		/// Casts an int array to a row <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">An int array.</param>
		/// <returns>A row ComplexVector y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexVector(int[] x)
		{
			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexVector(newElements);
		}

		/// <summary>
		/// Casts a short array to a row <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">A short array.</param>
		/// <returns>A row ComplexVector y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexVector(short[] x)
		{
			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexVector(newElements);
		}

		/// <summary>
		/// Casts a long array to a row <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">A long array.</param>
		/// <returns>A row ComplexVector y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexVector(long[] x)
		{
			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexVector(newElements);
		}

		/// <summary>
		/// Casts a float array to a row <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">A float array.</param>
		/// <returns>A row ComplexVector y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexVector(float[] x)
		{
			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x[i];
			}
			return new ComplexVector(newElements);
		}

		/// <summary>
		/// Casts a decimal array to a row <see cref="Vector"/>.
		/// </summary>
		/// <param name="x">A decimal array.</param>
		/// <returns>A row ComplexVector y, where y[i] = (Complex)x[i].</returns>
		public static explicit operator ComplexVector(decimal[] x)
		{
			int length = x.Length;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = (double)x[i];
			}
			return new ComplexVector(newElements);
		}

		#endregion //Casting

		#endregion //Operators

		#region Methods

		#region Basic Math Functions

		#region Acos

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Acos(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Acos(ComplexVector)"/>
		public ComplexVector Acos()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] + x[i].
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector Add(Vector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] + x[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] + x[i].
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector Add(ComplexVector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] + x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] + x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector Add(double x)
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] + x.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector Add(Complex x)
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

		internal static ComplexVector Add(ComplexVector x1, Vector x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Add(Vector x1, ComplexVector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexVector();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1[i] + x2.elements[i];
			}
			return new ComplexVector(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Add(ComplexVector x1, ComplexVector x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Add(ComplexVector x1, double x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Add(double x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 + x2.elements[i];
			}

			return new ComplexVector(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Add(ComplexVector x1, Complex x2)
		{
			return x1.Clone().Add(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Add(Complex x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 + x2[i];
			}

			return new ComplexVector(newElements, newDimensions);
		}

		#endregion //Add

		#region Asin

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Asin(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Asin(ComplexVector)"/>
		public ComplexVector Asin()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Atan(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Atan(ComplexVector)"/>
		public ComplexVector Atan()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Ceiling(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Ceiling(ComplexVector)"/>
		public ComplexVector Ceiling()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Cis(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Ceiling(ComplexVector)"/>
		public ComplexVector Cis()
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
		/// Modifies the <see cref="ComplexVector"/>, y, by replacing each element y[i] with CompareTo(y[i],x[i]).
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions.
		/// </exception>
		public ComplexVector CompareTo(ComplexVector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

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
		/// Modifies the <see cref="ComplexVector"/>, y, by replacing each element y[i] with CompareTo(y[i],x[i]).
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector CompareTo(Vector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

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
		/// Modifies the <see cref="ComplexVector"/>, y, by replacing each element y[i] with CompareTo(y[i],x).
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector CompareTo(double x)
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
		/// Modifies the <see cref="ComplexVector"/>, y, by replacing each element y[i] with CompareTo(y[i],x).
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector CompareTo(Complex x)
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Conj(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Conj(ComplexVector)"/>
		public ComplexVector Conj()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Cos(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Cos(ComplexVector)"/>
		public ComplexVector Cos()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Cosh(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Cosh(ComplexVector)"/>
		public ComplexVector Cosh()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] / x[i].
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector Divide(Vector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] / x[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] / x[i].
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector Divide(ComplexVector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] / x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] / x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector Divide(double x)
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] / x.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector Divide(Complex x)
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

		internal static ComplexVector Divide(ComplexVector x1, Vector x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Divide(Vector x1, ComplexVector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexVector();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1[i] / x2.elements[i];
			}
			return new ComplexVector(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Divide(ComplexVector x1, ComplexVector x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Divide(ComplexVector x1, double x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Divide(double x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 / x2.elements[i];
			}

			return new ComplexVector(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Divide(ComplexVector x1, Complex x2)
		{
			return x1.Clone().Divide(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Divide(Complex x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 / x2[i];
			}

			return new ComplexVector(newElements, newDimensions);
		}

		#endregion //Divide

		#region Exp

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Exp(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Exp(ComplexVector)"/>
		public ComplexVector Exp()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Floor(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Floor(ComplexVector)"/>
		public ComplexVector Floor()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Log(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Log(ComplexVector)"/>
		public ComplexVector Log()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Log(y[i],B).
		/// </summary>
		/// <param name="B"></param>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Log(ComplexVector,Complex)"/>
		public ComplexVector Log(Complex B)
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Log2(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Log2(ComplexVector)"/>
		public ComplexVector Log2()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Log10(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Log10(ComplexVector)"/>
		public ComplexVector Log10()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] * x[i].
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector Multiply(Vector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] * x[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] * x[i].
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector Multiply(ComplexVector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] * x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] * x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector Multiply(double x)
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] * x.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector Multiply(Complex x)
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

		internal static ComplexVector Multiply(ComplexVector x1, Vector x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Multiply(Vector x1, ComplexVector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexVector();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1[i] * x2.elements[i];
			}
			return new ComplexVector(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Multiply(ComplexVector x1, ComplexVector x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Multiply(ComplexVector x1, double x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Multiply(double x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 * x2.elements[i];
			}

			return new ComplexVector(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Multiply(ComplexVector x1, Complex x2)
		{
			return x1.Clone().Multiply(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Multiply(Complex x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 * x2[i];
			}

			return new ComplexVector(newElements, newDimensions);
		}

		#endregion //Multiply

		#region Pow

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Pow(y[i],x[i]).
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		/// <seealso cref="Compute.Pow(ComplexVector,Vector)"/>
		public ComplexVector Pow(Vector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Pow(this.elements[i], x[i]);
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Pow(y[i],x[i]).
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		/// <seealso cref="Compute.Pow(ComplexVector,ComplexVector)"/>
		public ComplexVector Pow(ComplexVector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = Compute.Pow(this.elements[i], x.elements[i]);
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Pow(y[i],x).
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Pow(ComplexVector,double)"/>
		public ComplexVector Pow(double x)
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Pow(y[i],x).
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Pow(ComplexVector,Complex)"/>
		public ComplexVector Pow(Complex x)
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Round(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Round(ComplexVector)"/>
		public ComplexVector Round()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Sign(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Sign(ComplexVector)"/>
		public ComplexVector Sign()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Sin(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Sin(ComplexVector)"/>
		public ComplexVector Sin()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Sinh(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Sinh(ComplexVector)"/>
		public ComplexVector Sinh()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Sqrt(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Sqrt(ComplexVector)"/>
		public ComplexVector Sqrt()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] - x[i].
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector Subtract(Vector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] - x[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] - x[i].
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The modified ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the ComplexVector and <paramref name="x"/> have different dimensions. 
		/// </exception>
		public ComplexVector Subtract(ComplexVector x)
		{
			if (this.IsEmpty() && x.IsEmpty())
				return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			for (int i = 0, length = this.Length; i < length; i++)
			{
				this.elements[i] = this.elements[i] - x.elements[i];
			}
			return this;
		}

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] - x.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector Subtract(double x)
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[i] - x.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The modified ComplexVector.</returns>
		public ComplexVector Subtract(Complex x)
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

		internal static ComplexVector Subtract(ComplexVector x1, Vector x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Subtract(Vector x1, ComplexVector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexVector();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_22"));

			int length = x2.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1[i] - x2.elements[i];
			}
			return new ComplexVector(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Subtract(ComplexVector x1, ComplexVector x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Subtract(ComplexVector x1, double x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Subtract(double x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 - x2.elements[i];
			}

			return new ComplexVector(newElements, newDimensions);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Subtract(ComplexVector x1, Complex x2)
		{
			return x1.Clone().Subtract(x2);
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static ComplexVector Subtract(Complex x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 - x2[i];
			}

			return new ComplexVector(newElements, newDimensions);
		}

		#endregion //Subtract

		#region Tan

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Tan(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Tan(ComplexVector)"/>
		public ComplexVector Tan()
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
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with Tanh(y[i]).
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Tanh(ComplexVector)"/>
		public ComplexVector Tanh()
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

		#region ComplexVector Functions

		#region Clone

		/// <summary>
		/// Returns a copy of the <see cref="ComplexVector"/> instance.
		/// </summary>
		public ComplexVector Clone()
		{
			if (this.IsEmpty())
				return new ComplexVector();

			return new ComplexVector(Utilities.Clone<Complex>(this.elements), this.Dimensions);
		}

		#endregion //Clone

		#region CumProduct

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[0]*...*y[i].
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.CumProduct(ComplexVector)"/>
		public ComplexVector CumProduct()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 1, length = this.Length; i < length; i++)
			{
				this[i] = this[i - 1] * this[i];
			}

			return this;
		}

		#endregion //CumProduct

		#region CumSum

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/>, y, by replacing each element y[i] with y[0]+...+y[i].
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.CumSum(ComplexVector)"/>
		public ComplexVector CumSum()
		{
			if (this.IsEmpty())
				return this;

			for (int i = 1, length = this.Length; i < length; i++)
			{
				this[i] = this[i - 1] + this[i];
			}

			return this;
		}

		#endregion //CumSum

		#region Reverse

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/> by reversing the order of its elements.
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Reverse(ComplexVector)"/>
		internal ComplexVector Reverse()
		{
			if (this.IsEmpty())
				return this;

			Complex[] newElements = new Complex[this.Length];
			this.elements.CopyTo(newElements, 0);
			Array.Reverse(newElements);
			this.elements = newElements;

			return this;
		}

		#endregion //Reverse

		#region Sort

		/// <summary>
		/// Modifies the <see cref="ComplexVector"/> by sorting the elements by value in ascending order.
		/// </summary>
		/// <returns>The modified Vector.</returns>
		/// <seealso cref="Compute.Sort(ComplexVector)"/>
		public ComplexVector Sort()
		{
			if (this.IsEmpty())
				return this;

			Complex[] elementsArray = this.elements as Complex[];
			if (elementsArray != null)
			{
				Array.Sort(elementsArray);
				return this;
			}

			List<Complex> elementsList = this.elements as List<Complex>;
			if (elementsList != null)
			{
				elementsList.Sort();
				return this;
			}

			Complex[] newElements = new Complex[this.elements.Count];
			this.elements.CopyTo(newElements, 0);
			Array.Sort(newElements);
			this.elements = newElements;

			return this;
		}

		#endregion //Sort

		#region Transpose

		/// <summary>
		/// Modifies a <see cref="ComplexVector"/> by switching its orientation. A row ComplexVector is converted to a 
		/// column ComplexVector and vice versa.
		/// </summary>
		/// <returns>The modified ComplexVector.</returns>
		/// <seealso cref="Compute.Transpose(ComplexVector)"/>
		public ComplexVector Transpose()
		{
			int temp = this.Dimensions[0];
			this.Dimensions[0] = this.Dimensions[1];
			this.Dimensions[1] = temp;
			return this;
		}

		#endregion //Transpose

		#endregion //ComplexVector Functions

		#region Private Methods

		// MD 4/19/11 - TFS72396
		#region FindValues

		private static BooleanVector FindValues(ComplexVector x, FindValuesType type)
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

			return new BooleanVector(newElements, x.Dimensions);
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
		/// An indexer that gets and sets a single element of a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="indices">An index that specifies an element of the ComplexVector.</param>
		/// <returns>The specified element of the ComplexVector.</returns>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the ComplexVector is indexed with more than two indices.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the ComplexVector is indexed below 0 or above its length.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if a row ComplexVector is indexed with two indices and the first index is greater than 0.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if a column ComplexVector is indexed with two indices and the second index is greater than 0.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the empty ComplexVector is indexed.
		/// </exception>
		public Complex this[params int[] indices]
		{
			get
			{
				//Exception for the Empty Vector
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_6"));

				//Exception for more than two indices.
				if (indices.Length != 1 && indices.Length != 2)
					throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_2"));

				//Counting the number of indices.
				int numberOfIndices = indices.Length;

				//Exception for improperly specified indices.
				if (numberOfIndices == 2)
					if (indices[0] != 0 && indices[1] != 0)
						throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_3"));

				//Exception for the empty Vector.
				if (this.Length == 0)
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_4"));

				//Condition for the unitary vector.
				if (this.Dimensions[0] == 1 && this.Dimensions[1] == 1)
				{
					if (numberOfIndices == 1)
					{
						if (indices[0] != 0)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						return this.elements[0];
					}
					else
					{
						if (indices[0] != 0 || indices[1] != 0)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						return this.elements[0];
					}
				}

				//Condition for the row vector.
				else if (this.Dimensions[0] == 1)
				{
					if (numberOfIndices == 1)
					{
						if (indices[0] < 0 || indices[0] >= this.Length)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						return this.elements[indices[0]];
					}
					else
					{
						if (indices[0] != 0 || indices[1] < 0 || indices[1] >= this.Length)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						return this.elements[indices[1]];
					}
				}
				//Condition for the column vector.
				else
				{
					if (numberOfIndices == 1)
					{
						if (indices[0] < 0 || indices[0] >= this.Length)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						return this.elements[indices[0]];
					}
					else
					{
						if (indices[1] != 0 || indices[0] < 0 || indices[0] >= this.Length)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						return this.elements[indices[0]];
					}
				}
			}

			set
			{
				//Exception for the Empty Vector
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_6"));

				//Exception for more than two indices.
				if (indices.Length != 1 && indices.Length != 2)
					throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_2"));

				//Counting the number of indices.
				int numberOfIndices = indices.Length;

				//Exception for improperly specified indices.
				if (numberOfIndices == 2)
					if (indices[0] != 0 && indices[1] != 0)
						throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_3"));

				//Exception for the empty Vector.
				if (this.Length == 0)
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_4"));

				//Condition for the unitary vector.
				if (this.Dimensions[0] == 1 && this.Dimensions[1] == 1)
				{
					if (numberOfIndices == 1)
					{
						if (indices[0] != 0)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						this.elements[0] = value;
					}
					else
					{
						if (indices[0] != 0 || indices[1] != 0)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						this.elements[0] = value;
					}
				}

				//Condition for the row vector.
				else if (this.Dimensions[0] == 1)
				{
					if (numberOfIndices == 1)
					{
						if (indices[0] < 0 || indices[0] >= this.Length)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						this.elements[indices[0]] = value;
					}
					else
					{
						if (indices[0] != 0 || indices[1] < 0 || indices[1] >= this.Length)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						this.elements[indices[1]] = value;
					}
				}
				//Condition for the column vector.
				else
				{
					if (numberOfIndices == 1)
					{
						if (indices[0] < 0 || indices[0] >= this.Length)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						this.elements[indices[0]] = value;
					}
					else
					{
						if (indices[1] != 0 || indices[0] < 0 || indices[0] >= this.Length)
							throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

						this.elements[indices[0]] = value;
					}
				}
			}
		}

		/// <summary>
		/// An indexer that gets or sets a set of elements in a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="indices">A <see cref="Vector"/> of integer indices.</param>
		/// <returns>A ComplexVector of elements specified by the <paramref name="indices"/></returns>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the ComplexVector is indexed below 0 or above its length.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the empty ComplexVector is indexed.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if the indices are not integers.
		/// </exception>
		public ComplexVector this[Vector indices]
		{
			get
			{
				//Exception for the Empty Vector
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_6"));

				//Exception for Complex indices.
				Vector ind = indices as Vector;
				if (ind.IsEmpty())
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_23"), "indices");

				//Exception for the empty Vector.
				if (this.Length == 0)
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_4"));

				//Creating the new dimensions of the vector.
				int[] newDimensions = new int[2];
				if (this.Dimensions[0] == 1)
				{
					newDimensions[0] = 1;
					newDimensions[1] = indices.Length;
				}
				else
				{
					newDimensions[1] = 1;
					newDimensions[0] = indices.Length;
				}

				//Creating the new elements of the vector.
				double currindex;
				int length = ind.Length;
				IList<Complex> newElements = new Complex[length];
				for (int i = 0; i < length; i++)
				{
					currindex = ind[i];

					//Exception for out of range indices.
					if (currindex < 0 || currindex >= this.Length)
						throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

					//Exception for non-integer indices.
					if (currindex != Compute.Round(currindex))
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_5"), "indices");

					newElements[i] = this.elements[(int)currindex];
				}

				//Return a new vector of the same type.
				return new ComplexVector(newElements, newDimensions);
			}

			set
			{
				//Exception for the Empty Vector
				if (this.IsEmpty())
					throw new IndexOutOfRangeException(Compute.GetString("LE_ArgumentOutOfRangeException_6"));

				//Creating the new dimensions of the vector.
				int[] newDimensions = new int[2];
				if (this.Dimensions[0] == 1)
				{
					newDimensions[0] = 1;
					newDimensions[1] = indices.Length;
				}
				else
				{
					newDimensions[1] = 1;
					newDimensions[0] = indices.Length;
				}

				//Setting a single number.
				bool valueIsUnitary = value.IsUnitary();

				//Creating the new elements of the vector.
				double currindex;
				int currindexint;
				int length = indices.Length;
				int thisLength = this.Length;
				for (int i = 0; i < length; i++)
				{
					currindex = indices[i];
					currindexint = (int)Compute.Round(currindex);

					//Exception for out of range indices.
					if (currindex < 0 || currindex >= thisLength)
						throw new IndexOutOfRangeException(Compute.GetString("LE_ArgumentOutOfRangeException_5"));

					//Exception for non-integer indices.
					if (currindex != currindexint)
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_5"), "indices");

					//Assigning the value.
					if (valueIsUnitary)
						this.elements[currindexint] = value.elements[0];
					else
						this.elements[currindexint] = value.elements[i];
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