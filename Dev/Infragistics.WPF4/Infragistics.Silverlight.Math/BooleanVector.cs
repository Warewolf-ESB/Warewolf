using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections;

namespace Infragistics.Math
{
	/// <summary>
	/// A BooleanVector is a list of boolean values with an additional dimensionality property that specifies 
	/// its orientation.
	/// </summary>
    public class BooleanVector : MatrixBase
		, IEnumerable<bool>

, ICloneable

	{
		#region Member Variables

		private IList<bool> elements;

		#endregion //Member Variables

		#region Static Variables

		#region Empty

		/// <summary>
		/// Returns the empty <see cref="BooleanVector"/>.
		/// </summary>
		public static BooleanVector Empty = new BooleanVector();

		#endregion //Empty

		#endregion //Static Variables

		#region Constructors
		
		/// <summary>
		/// Initializes an empty <see cref="BooleanVector"/> instance.
		/// </summary>
		/// <seealso cref="MatrixBase.IsEmpty"/>
		public BooleanVector()
			: base()
		{
		}

		/// <summary>
		/// Initializes a row zero <see cref="BooleanVector"/> of a specified <paramref name="length"/>. 
		/// </summary>
		/// <param name="length">The length of the constructed BooleanVector.</param>
		public BooleanVector(int length)
			: base(length)
		{
			this.elements = new bool[length];
		}

		/// <summary>
		/// Initializes a row <see cref="BooleanVector"/> of the specified <paramref name="length"/> and the 
		/// specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value assigned to each element of the constructed BooleanVector.</param>
		/// <param name="length">The length of the constructed BooleanVector.</param>
		public BooleanVector(bool value, int length)
			: base(length)
		{
			this.elements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				this.elements[i] = value;
			}
		}

		/// <summary>
		/// Initializes a row <see cref="BooleanVector"/> with the specified <paramref name="elements"/>.
		/// </summary>
		/// <param name="elements">The elements of the constructed BooleanVector.</param>
		public BooleanVector(IList<bool> elements)
			:base(elements.Count)
		{
			this.elements = elements;
		}

		/// <summary>
		/// Initializes a <see cref="BooleanVector"/> with the specified <paramref name="elements"/> and 
		/// <paramref name="dimensions"/>.
		/// </summary>
		/// <param name="elements">The elements of the constructed BooleanVector.</param>
		/// <param name="dimensions">The dimensions of the constructed BooleanVector.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the dimensions specify a size that is inconsistent with the length of the elements.
		/// </exception>
		public BooleanVector(IList<bool> elements, int[] dimensions)
			: base(dimensions)
		{
			if (!this.IsRow() && !this.IsColumn())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_1"), "dimensions");

			if (elements.Count != this.Length)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_2"));

			this.elements = elements;
		}

		/// <summary>
		/// Initializes a <see cref="BooleanVector"/> with the specified elements <paramref name="x"/>.
		/// </summary>
		/// <param name="type">An enum that specifies the orientation of the BooleanVector.</param>
		/// <param name="x">A bool array of elements.</param>
		public BooleanVector(VectorType type, params bool[] x)
			: base(x.Length, type)
		{
			this.elements = x;
		}

		#endregion //Constructors

		#region Base Class Overrides

		#region CopyTo

		/// <summary>
		/// Copies the base <see cref="BooleanVector"/> to an <paramref name="array"/> starting at a 
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
		/// Copies the base <see cref="BooleanVector"/> to an <paramref name="array"/> starting at a 
		/// particular <paramref name="index"/>.
		/// </summary>
		/// <param name="array">The destination array.</param>
		/// <param name="index">An index into the destination array where the copying begins.</param>
		public void CopyTo(bool[] array, int index)
		{
			this.CopyTo((Array)array, index);
		}

		#endregion //CopyTo

		#region Equals

		/// <summary>
		/// Compares the <see cref="BooleanVector"/> with <paramref name="x"/> for equality.
		/// </summary>
		/// <param name="x">An object.</param>
		/// <returns>Returns True if the BooleanVector is equal to x; False otherwise.</returns>
		public override bool Equals(object x)
		{
			BooleanVector xCast = x as BooleanVector;
			if (object.Equals(x, null))
				return false;

			return this == xCast;
		}

		#endregion Equals

		#region GetHashCode

		/// <summary>
		/// Returns hash code for the <see cref="BooleanVector"/>.
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
		/// Returns the string representation of a <see cref="BooleanVector"/>.
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

		#region IEnumerable<bool> Members

		/// <summary>
		/// Returns an double enumerator for the <see cref="BooleanVector"/>.
		/// </summary>
		IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
		{
			return ((IEnumerable<bool>)this.elements).GetEnumerator();
		}

		#endregion //IEnumerable<bool> Members

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the <see cref="BooleanVector"/>.
		/// </summary>
		public override IEnumerator GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		#endregion //IEnumerable Members

		#region ICloneable Members


		/// <summary>
		/// Returns a copy of the <see cref="BooleanVector"/>.
		/// </summary>
		object ICloneable.Clone()
		{
			return this.Clone();
		}


		#endregion //ICloneable Members

		#endregion //Interfaces

		#region Operators

		#region !

		/// <summary>
		/// Returns the pointwise NOT of a <see cref="BooleanVector"/>.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = NOT(x[i]).</returns>
		public static BooleanVector operator !(BooleanVector x)
		{
			if(x.IsEmpty())
				return BooleanVector.Empty;

			int length = x.Length;
			bool[] newElements = new bool[length];
			for(int i = 0; i < length; i++)
			{
				newElements[i] = !x.elements[i];
			}

			return new BooleanVector(newElements,x.Dimensions); 
		}

		/// <summary>
		/// Returns the pointwise NOT of the <see cref="BooleanVector"/>.
		/// </summary>
		/// <returns>A BooleanVector y, where y[i] = NOT(this[i]).</returns>
		public BooleanVector LogicalNot()
		{
			return !this;
		}

		#endregion // !

		#region &

		/// <summary>
		/// Returns the pointwise logical AND of two <see cref="BooleanVector"/> instances.
		/// </summary>
		/// <param name="x1">The first BooleanVector.</param>
		/// <param name="x2">The second BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = AND(x1[i],x2[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the <paramref name="x1"/> and <paramref name="x2"/> have different dimensions.
		/// </exception>
		public static BooleanVector operator &(BooleanVector x1, BooleanVector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return BooleanVector.Empty;

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_3"));

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for(int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] && x2.elements[i];
			}

			return new BooleanVector(newElements,x1.Dimensions); 
		}

		/// <summary>
		/// Returns the pointwise logical AND of a <see cref="BooleanVector"/> and a bool.
		/// </summary>
		/// <param name="x1">A BooleanVector.</param>
		/// <param name="x2">A bool.</param>
		/// <returns>A BooleanVector y, where y[i] = AND(x1[i],x2).</returns>
		public static BooleanVector operator &(BooleanVector x1, bool x2)
		{
			if (x1.IsEmpty())
				return BooleanVector.Empty;

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] && x2;
			}

			return new BooleanVector(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical AND of a bool and a <see cref="BooleanVector"/>.
		/// </summary>
		/// <param name="x1">A bool.</param>
		/// <param name="x2">A BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = AND(x1,x2[i]).</returns>
		public static BooleanVector operator &(bool x1, BooleanVector x2)
		{
			if (x2.IsEmpty())
				return BooleanVector.Empty;

			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 && x2.elements[i];
			}

			return new BooleanVector(newElements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical AND of two <see cref="BooleanVector"/> instances.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = AND(this[i],x[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the BooleanVector and <paramref name="x"/> have different dimensions.
		/// </exception>
		public BooleanVector BitwiseAnd(BooleanVector x)
		{
			return this & x;
		}

		/// <summary>
		/// Returns the pointwise logical AND of two <see cref="BooleanVector"/> instances.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = AND(this[i],x[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the BooleanVector and <paramref name="x"/> have different dimensions.
		/// </exception>
		public BooleanVector And(BooleanVector x)
		{
			return this & x;
		}

		#endregion // &

		#region |

		/// <summary>
		/// Returns the pointwise logical OR of two <see cref="BooleanVector"/> instances.
		/// </summary>
		/// <param name="x1">The first BooleanVector.</param>
		/// <param name="x2">The second BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = OR(x1[i],x2[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the x1 and x2 have different dimensions.
		/// </exception>
		public static BooleanVector operator |(BooleanVector x1, BooleanVector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return BooleanVector.Empty;

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_4"));

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] || x2.elements[i];
			}

			return new BooleanVector(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical OR of a <see cref="BooleanVector"/> and a bool.
		/// </summary>
		/// <param name="x1">A BooleanVector.</param>
		/// <param name="x2">A bool.</param>
		/// <returns>A BooleanVector y, where y[i] = OR(x1[i],x2).</returns>
		public static BooleanVector operator |(BooleanVector x1, bool x2)
		{
			if (x1.IsEmpty())
				return BooleanVector.Empty;

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] || x2;
			}

			return new BooleanVector(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical OR of a bool and a <see cref="BooleanVector"/>.
		/// </summary>
		/// <param name="x1">A bool.</param>
		/// <param name="x2">A BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = OR(x1,x2[i]).</returns>
		public static BooleanVector operator |(bool x1, BooleanVector x2)
		{
			if (x2.IsEmpty())
				return BooleanVector.Empty;

			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 || x2.elements[i];
			}

			return new BooleanVector(newElements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical OR of two <see cref="BooleanVector"/> instances.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = OR(this[i],x[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the BooleanVector and <paramref name="x"/> have different dimensions.
		/// </exception>
		public BooleanVector BitwiseOr(BooleanVector x)
		{
			return this | x;
		}

		/// <summary>
		/// Returns the pointwise logical OR of two <see cref="BooleanVector"/> instances.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = OR(this[i],x[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the BooleanVector and <paramref name="x"/> have different dimensions.
		/// </exception>
		public BooleanVector Or(BooleanVector x)
		{
			return this | x;
		}

		#endregion // |

		#region ==

		/// <summary>
		/// Determines whether two <see cref="BooleanVector"/> instances have the same dimensions and element values.
		/// </summary>
		/// <returns>Returns True if the two BooleanVector instances are equal; False otherwise.</returns>
		public static bool operator ==(BooleanVector x1, BooleanVector x2)
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
		/// Returns the pointwise equality operator for a <see cref="BooleanVector"/> and a bool.
		/// </summary>
		/// <param name="x1">A BooleanVector.</param>
		/// <param name="x2">A bool.</param>
		/// <returns>A new BooleanVector y, where y[i] = Equals(x1[i],x2).</returns>
		public static BooleanVector operator ==(BooleanVector x1, bool x2)
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
		/// Returns the pointwise equality operator for a bool and a <see cref="BooleanVector"/>.
		/// </summary>
		/// <param name="x1">A bool.</param>
		/// <param name="x2">A BooleanVector.</param>
		/// <returns>A new BooleanVector y, where y[i] = Equals(x1,x2[i]).</returns>
		public static BooleanVector operator ==(bool x1, BooleanVector x2)
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
		/// Returns the pointwise equality operator for the <see cref="BooleanVector"/> and a bool.
		/// </summary>
		/// <param name="x">A bool.</param>
		/// <returns>A new BooleanVector y, where y[i] = Equals(this[i],x).</returns>
		public BooleanVector Equals(bool x)
		{
			return this == x;
		}

		#endregion // ==

		#region !=

		/// <summary>
		/// Determines whether two <see cref="BooleanVector"/> instances have different dimensions and element values.
		/// </summary>
		/// <returns>Returns True if the two Vector instances are unequal; False otherwise.</returns>
		public static bool operator !=(BooleanVector x1, BooleanVector x2)
		{
			return !(x1 == x2);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a <see cref="BooleanVector"/> and a bool.
		/// </summary>
		/// <param name="x1">A BooleanVector.</param>
		/// <param name="x2">A bool.</param>
		/// <returns>A new BooleanVector y, where y[i] = NotEquals(x1[i],x2).</returns>
		public static BooleanVector operator !=(bool x1, BooleanVector x2)
		{
			return !(x1 == x2);
		}

		/// <summary>
		/// Returns the pointwise inequality operator for a bool and a <see cref="BooleanVector"/>.
		/// </summary>
		/// <param name="x1">A bool.</param>
		/// <param name="x2">A BooleanVector.</param>
		/// <returns>A new BooleanVector y, where y[i] = NotEquals(x1,x2[i]).</returns>
		public static BooleanVector operator !=(BooleanVector x1, bool x2)
		{
			return !(x1 == x2);
		}

		#endregion // !=

		#region Casting

		/// <summary>
		/// Casts a bool to a unitary <see cref="BooleanVector"/>. 
		/// </summary>
		/// <param name="x">A bool.</param>
		/// <returns>A unitary BooleanVector y, where y[0] = x.</returns>
		public static implicit operator BooleanVector(bool x)
		{
			return new BooleanVector(x, 1);
		}

		/// <summary>
		/// Casts a <see cref="BooleanVector"/> to a bool array. 
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A bool array y, where y[i] = x[i].</returns>
		public static explicit operator bool[](BooleanVector x)
		{
			bool[] newElements = new bool[x.Length];
			Utilities.Copy(x.elements, newElements);
			return newElements;
		}

		/// <summary>
		/// Casts a bool array to a row <see cref="BooleanVector"/>.
		/// </summary>
		/// <param name="x">A bool array.</param>
		/// <returns>A row BooleanVector y, where y[i] = x[i].</returns>
		public static implicit operator BooleanVector(bool[] x)
		{
			return new BooleanVector(x);
		}

		#endregion //Casting

		#endregion //Operators

		#region Methods

		#region Clone

		/// <summary>
		/// Returns a copy of the <see cref="BooleanVector"/> instance.
		/// </summary>
		public BooleanVector Clone()
		{
			if (this.IsEmpty())
				return BooleanVector.Empty;

			return new BooleanVector(Utilities.Clone<bool>(this.elements), this.Dimensions);
		}

		#endregion //Clone

		#region Reverse

		/// <summary>
		/// Modifies a <see cref="BooleanVector"/> by reversing the order of its elements.
		/// </summary>
		/// <returns>The modified BooleanVector.</returns>
		/// <seealso cref="Compute.Reverse(BooleanVector)"/>
		public BooleanVector Reverse()
		{
			if (this.IsEmpty())
				return this;

			bool[] newElements = new bool[this.Length];
			this.elements.CopyTo(newElements, 0);
			Array.Reverse(newElements);
			this.elements = newElements;

			return this;
		}

		#endregion //Reverse

		#region Sort

		/// <summary>
		/// Modifies the <see cref="BooleanVector"/> by sorting the elements by value in ascending order.
		/// </summary>
		/// <returns>The modified BooleanVector.</returns>
		/// <seealso cref="Compute.Sort(BooleanVector)"/>
		public BooleanVector Sort()
		{
			if (this.IsEmpty())
				return this;

			bool[] elementsArray = this.elements as bool[];
			if (elementsArray != null)
			{
				Array.Sort(elementsArray);
				return this;
			}

			List<bool> elementsList = this.elements as List<bool>;
			if (elementsList != null)
			{
				elementsList.Sort();
				return this;
			}

			bool[] newElements = new bool[this.elements.Count];
			this.elements.CopyTo(newElements, 0);
			Array.Sort(newElements);
			this.elements = newElements;

			return this;
		}

		#endregion //Sort

		#region Transpose

		/// <summary>
		/// Modifies a <see cref="BooleanVector"/> by switching its orientation. A row BooleanVector is converted to a 
		/// column BooleanVector and vice versa.
		/// </summary>
		/// <returns>The modified BooleanVector.</returns>
		/// <seealso cref="Compute.Transpose(BooleanVector)"/>
		public BooleanVector Transpose()
		{
			int temp = this.Dimensions[0];
			this.Dimensions[0] = this.Dimensions[1];
			this.Dimensions[1] = temp;
			return this;
		}

		#endregion //Transpose

		#endregion //Methods

		#region Properties

		#region Elements






		internal IList<bool> Elements
		{
			get { return this.elements; }
		}

		#endregion //Elements

		#region Indexers

		/// <summary>
		/// An indexer that gets and sets a single element of a <see cref="BooleanVector"/>.
		/// </summary>
		/// <param name="indices">An index that specifies an element of the BooleanVector.</param>
		/// <returns>The specified element of the BooleanVector.</returns>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the BooleanVector is indexed with more than two indices.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the BooleanVector is indexed below 0 or above its length.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if a row BooleanVector is indexed with two indices and the first index is greater than 0.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if a column BooleanVector is indexed with two indices and the second index is greater than 0.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the empty BooleanVector is indexed.
		/// </exception>
		public bool this[params int[] indices]
		{
			get
			{
				//Exception for the Empty Vector
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_1"));

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
				if (this.IsUnitary())
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
				else if (this.IsRow())
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
				if (this.IsUnitary())
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
				else if (this.IsRow())
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
		/// An indexer that gets or sets a set of elements in a <see cref="BooleanVector"/>.
		/// </summary>
		/// <param name="indices">A Vector of integer indices.</param>
		/// <returns>A BooleanVector of elements specified by the <paramref name="indices"/></returns>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the BooleanVector is indexed below 0 or above its length.
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// Occurs if the empty BooleanVector is indexed.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if the indices are not integers.
		/// </exception>
		public BooleanVector this[Vector indices]
		{
			get
			{
				//Exception for the empty BooleanVector.
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_6"));

				//Creating the new dimensions of the BooleanVector.
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

				//Creating the new elements of the BooleanVector.
				double currindex;
				int length = this.Length;
				IList<bool> newElements = new bool[indices.Length];
				for (int i = 0, count = newElements.Count; i < count; i++)
				{
					currindex = indices.Elements[i];

					//Exception for out of range indices.
					if (currindex < 0 || currindex >= length)
						throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

					//Exception for non-integer indices.
					if (currindex != Compute.Round(currindex))
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_5"), "indices");

					newElements[i] = this.elements[(int)currindex];
				}

				//Return a new vector of the same type.
				return new BooleanVector(newElements, newDimensions);
			}

			set
			{
				//Exception for the empty BooleanVector.
				if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_6"));

				//Creating the new dimensions of the BooleanVector.
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

				//Creating the new elements of the BooleanVector.
				double currindex;
				int currindexint;
				int length = indices.Length;
				int thisLength = this.Length;
				for (int i = 0; i < length; i++)
				{
					currindex = indices.Elements[i];
					currindexint = (int)Compute.Round(currindex);

					//Exception for out of range indices.
					if (currindex < 0 || currindex >= thisLength)
						throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

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