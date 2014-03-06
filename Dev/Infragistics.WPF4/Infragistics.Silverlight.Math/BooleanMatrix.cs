using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections;

namespace Infragistics.Math
{
	// MD 4/19/11 - TFS72396
	/// <summary>
	/// A BooleanMatrix is a list of boolean values with an additional dimensionality property that 
	/// specifies its spatial orientation.
	/// </summary>
	public class BooleanMatrix : MatrixBase
			, IEnumerable<Boolean>

, ICloneable

	{
		#region Member Variables

		private IList<Boolean> elements;

		#endregion //Member Variables

		#region Static Variables

		#region Empty

		/// <summary>
		/// Returns the empty <see cref="BooleanMatrix"/>.
		/// </summary>
		public static BooleanMatrix Empty = new BooleanMatrix();

		#endregion //Empty

		#endregion //Static Variables

		#region Constructors

		/// <summary>
		/// Initializes an empty <see cref="BooleanMatrix"/> instance.
		/// </summary>
		/// <seealso cref="MatrixBase.IsEmpty"/>
		public BooleanMatrix()
			: base()
		{
		}

		/// <summary>
		/// Initializes a one-dimensional <see cref="BooleanMatrix"/> of a specified <paramref name="length"/>. 
		/// </summary>
		/// <param name="length">The length of the constructed BooleanMatrix.</param>
		public BooleanMatrix(int length)
			: base(length)
		{
			this.elements = new bool[length];
		}

		/// <summary>
		/// Initializes a zero <see cref="BooleanMatrix"/> with the specified <paramref name="dimensions"/>. 
		/// </summary>
		/// <param name="dimensions">The dimensions of the constructed BooleanMatrix.</param>
		public BooleanMatrix(int[] dimensions)
			: base(dimensions)
		{
			this.elements = new bool[this.Length];
		}

		/// <summary>
		/// Initializes a constant <see cref="BooleanMatrix"/> with the specified <paramref name="dimensions"/>. 
		/// </summary>
		/// <param name="value">The constant value assigned to each element of the BooleanMatrix.</param>
		/// <param name="dimensions">The dimensions of the constructed BooleanMatrix.</param>
		public BooleanMatrix(bool value, int[] dimensions)
			: base(dimensions)
		{
			this.elements = new bool[this.Length];
			for (int i = 0; i < this.Length; i++)
			{
				this.elements[i] = value;
			}
		}

		/// <summary>
		/// Initializes a one-dimensional <see cref="BooleanMatrix"/> of the specified <paramref name="length"/> and the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value assigned to each element of the constructed BooleanMatrix.</param>
		/// <param name="length">The length of the constructed BooleanMatrix.</param>
		public BooleanMatrix(bool value, int length)
			: base(length)
		{
			this.elements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				this.elements[i] = value;
			}
		}

		/// <summary>
		/// Initializes a <see cref="BooleanMatrix"/> with the specified <paramref name="elements"/> and <paramref name="dimensions"/>.
		/// </summary>
		/// <param name="elements">The elements of the constructed BooleanMatrix.</param>
		/// <param name="dimensions">The dimensions of the constructed BooleanMatrix.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the dimensions specify a size that is inconsistent with the length of the elements.
		/// </exception>
		public BooleanMatrix(IList<bool> elements, int[] dimensions)
			: base(dimensions)
		{
			if (elements.Count != this.Length)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_2"));

			this.elements = elements;
		}

		/// <summary>
		/// Initializes a <see cref="BooleanMatrix"/> by copying the elements and dimensions of a multi-dimensional 
		/// Boolean array.
		/// </summary>
		/// <param name="x">A multi-dimensional Boolean array.</param>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not a Boolean array.
		/// </exception>
		public BooleanMatrix(Array x)
			: base(Utilities.GetArrayDimensions(x))
		{
			if (x.GetType().GetElementType() != typeof(bool))
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_11"), "x");

			this.elements = new bool[x.Length];
			Vector size = this.Size;
			for (int i = 0; i < x.Length; i++)
			{
				int[] subscript = Compute.IndexToSubscript(i, size);

				if (subscript.Length == 2 && x.Rank == 1)
					this.elements[i] = (bool)x.GetValue(subscript[1]);
				else
					this.elements[i] = (bool)x.GetValue(subscript);
			}
		}

		#endregion //Constructors

		#region Base Class Overrides

		#region CopyTo

		/// <summary>
		/// Copies the base <see cref="BooleanMatrix"/> to an <paramref name="array"/> starting at a 
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
		/// Copies the base <see cref="BooleanMatrix"/> to an <paramref name="array"/> starting at a 
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
		/// Compares the <see cref="BooleanMatrix"/> with <paramref name="x"/> for equality.
		/// </summary>
		/// <param name="x">An object.</param>
		/// <returns>Returns True if the BooleanMatrix is equal to x; False otherwise.</returns>
		public override bool Equals(object x)
		{
			BooleanMatrix xCast = x as BooleanMatrix;
			if (object.Equals(x, null))
				return false;

			return this == xCast;
		}

		#endregion Equals

		#region GetHashCode

		/// <summary>
		/// Returns hash code for the <see cref="BooleanMatrix"/>.
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
		/// Returns the string representation of a <see cref="BooleanMatrix"/>.
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
				result.Append(BooleanMatrix.ToString_HelperSubscript(subscript));

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

		#region IEnumerable<bool> Members

		/// <summary>
		/// Returns an double enumerator for the <see cref="BooleanMatrix"/>.
		/// </summary>
		IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
		{
			return ((IEnumerable<bool>)this.elements).GetEnumerator();
		}

		#endregion //IEnumerable<bool> Members

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the <see cref="BooleanMatrix"/>.
		/// </summary>
		public override IEnumerator GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		#endregion //IEnumerable Members

		#region ICloneable Members


		/// <summary>
		/// Returns a copy of the <see cref="BooleanMatrix"/>.
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
		/// Returns the pointwise NOT of a <see cref="BooleanMatrix"/>.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = NOT(x[i]).</returns>
		public static BooleanMatrix operator !(BooleanMatrix x)
		{
			if (x.IsEmpty())
				return BooleanMatrix.Empty;

			int length = x.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = !x.elements[i];
			}

			return new BooleanMatrix(newElements, x.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise NOT of the <see cref="BooleanMatrix"/>.
		/// </summary>
		/// <returns>A BooleanMatrix y, where y[i] = NOT(this[i]).</returns>
		public BooleanMatrix LogicalNot()
		{
			return !this;
		}

		#endregion // !

		#region &

		/// <summary>
		/// Returns the pointwise logical AND of two <see cref="BooleanMatrix"/> instances.
		/// </summary>
		/// <param name="x1">The first BooleanMatrix.</param>
		/// <param name="x2">The second BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = AND(x1[i],x2[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the <paramref name="x1"/> and <paramref name="x2"/> have different dimensions.
		/// </exception>
		public static BooleanMatrix operator &(BooleanMatrix x1, BooleanMatrix x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return BooleanMatrix.Empty;

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_3"));

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] && x2.elements[i];
			}

			return new BooleanMatrix(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical AND of a <see cref="BooleanMatrix"/> and a bool.
		/// </summary>
		/// <param name="x1">A BooleanMatrix.</param>
		/// <param name="x2">A bool.</param>
		/// <returns>A BooleanMatrix y, where y[i] = AND(x1[i],x2).</returns>
		public static BooleanMatrix operator &(BooleanMatrix x1, bool x2)
		{
			if (x1.IsEmpty())
				return BooleanMatrix.Empty;

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] && x2;
			}

			return new BooleanMatrix(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical AND of a bool and a <see cref="BooleanMatrix"/>.
		/// </summary>
		/// <param name="x1">A bool.</param>
		/// <param name="x2">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = AND(x1,x2[i]).</returns>
		public static BooleanMatrix operator &(bool x1, BooleanMatrix x2)
		{
			if (x2.IsEmpty())
				return BooleanMatrix.Empty;

			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 && x2.elements[i];
			}

			return new BooleanMatrix(newElements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical AND of two <see cref="BooleanMatrix"/> instances.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = AND(this[i],x[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the BooleanMatrix and <paramref name="x"/> have different dimensions.
		/// </exception>
		public BooleanMatrix BitwiseAnd(BooleanMatrix x)
		{
			return this & x;
		}

		/// <summary>
		/// Returns the pointwise logical AND of two <see cref="BooleanMatrix"/> instances.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = AND(this[i],x[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the BooleanMatrix and <paramref name="x"/> have different dimensions.
		/// </exception>
		public BooleanMatrix And(BooleanMatrix x)
		{
			return this & x;
		}

		#endregion // &

		#region |

		/// <summary>
		/// Returns the pointwise logical OR of two <see cref="BooleanMatrix"/> instances.
		/// </summary>
		/// <param name="x1">The first BooleanMatrix.</param>
		/// <param name="x2">The second BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = OR(x1[i],x2[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the x1 and x2 have different dimensions.
		/// </exception>
		public static BooleanMatrix operator |(BooleanMatrix x1, BooleanMatrix x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return BooleanMatrix.Empty;

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_4"));

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] || x2.elements[i];
			}

			return new BooleanMatrix(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical OR of a <see cref="BooleanMatrix"/> and a bool.
		/// </summary>
		/// <param name="x1">A BooleanMatrix.</param>
		/// <param name="x2">A bool.</param>
		/// <returns>A BooleanMatrix y, where y[i] = OR(x1[i],x2).</returns>
		public static BooleanMatrix operator |(BooleanMatrix x1, bool x2)
		{
			if (x1.IsEmpty())
				return BooleanMatrix.Empty;

			int length = x1.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1.elements[i] || x2;
			}

			return new BooleanMatrix(newElements, x1.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical OR of a bool and a <see cref="BooleanMatrix"/>.
		/// </summary>
		/// <param name="x1">A bool.</param>
		/// <param name="x2">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = OR(x1,x2[i]).</returns>
		public static BooleanMatrix operator |(bool x1, BooleanMatrix x2)
		{
			if (x2.IsEmpty())
				return BooleanMatrix.Empty;

			int length = x2.Length;
			bool[] newElements = new bool[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x1 || x2.elements[i];
			}

			return new BooleanMatrix(newElements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the pointwise logical OR of two <see cref="BooleanMatrix"/> instances.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = OR(this[i],x[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the BooleanMatrix and <paramref name="x"/> have different dimensions.
		/// </exception>
		public BooleanMatrix BitwiseOr(BooleanMatrix x)
		{
			return this | x;
		}

		/// <summary>
		/// Returns the pointwise logical OR of two <see cref="BooleanMatrix"/> instances.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix y, where y[i] = OR(this[i],x[i]).</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the BooleanMatrix and <paramref name="x"/> have different dimensions.
		/// </exception>
		public BooleanMatrix Or(BooleanMatrix x)
		{
			return this | x;
		}

		#endregion // |

		#region ==

		/// <summary>
		/// Determines whether two <see cref="BooleanMatrix"/> instances have the same dimensions and element values.
		/// </summary>
		/// <param name="x1">The first BooleanMatrix.</param>
		/// <param name="x2">The second BooleanMatrix.</param>
		/// <returns>Returns True if the two BooleanMatrix instances are equal; False otherwise.</returns>
		public static bool operator ==(BooleanMatrix x1, BooleanMatrix x2)
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

		#endregion // ==

		#region !=

		/// <summary>
		/// Determines whether two <see cref="BooleanMatrix"/> instances have different dimensions or element values.
		/// </summary>
		/// <param name="x1">The first BooleanMatrix.</param>
		/// <param name="x2">The second BooleanMatrix.</param>
		/// <returns>Returns True if the two BooleanMatrix instances are unequal; False otherwise.</returns>
		public static bool operator !=(BooleanMatrix x1, BooleanMatrix x2)
		{
			return !(x1 == x2);
		}

		#endregion // !=

		#region Casting

		/// <summary>
		/// Casts a <see cref="BooleanVector"/> to a <see cref="BooleanMatrix"/>. 
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A Boolean y, where y[i] = x[i].</returns>
		public static explicit operator BooleanMatrix(BooleanVector x)
		{
			if (x.IsEmpty())
				return BooleanMatrix.Empty;

			return new BooleanMatrix(x.Elements, x.Dimensions);
		}

		/// <summary>
		/// Casts a <see cref="BooleanMatrix"/> to a <see cref="BooleanVector"/>. 
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A BooleanVector y, where y[i] = x[i].</returns>
		public static explicit operator BooleanVector(BooleanMatrix x)
		{
			if (!x.IsColumn() && !x.IsRow())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_13"));

			if (x.IsEmpty())
				return BooleanVector.Empty;

			return new BooleanVector(x.Elements, x.Dimensions);
		}

		/// <summary>
		/// Casts a boolean value to a unitary <see cref="BooleanMatrix"/>. 
		/// </summary>
		/// <param name="x">A boolean value.</param>
		/// <returns>A unitary BooleanMatrix y, where y[0] = x.</returns>
		public static implicit operator BooleanMatrix(bool x)
		{
			return new BooleanMatrix(x, 1);
		}

		#endregion //Casting

		#endregion //Operators

		#region Methods

		#region Clone

		/// <summary>
		/// Returns a copy of the <see cref="BooleanMatrix"/> instance.
		/// </summary>
		public BooleanMatrix Clone()
		{
			if (this.IsEmpty())
				return new BooleanMatrix();

			return new BooleanMatrix(Utilities.Clone<bool>(this.elements), this.Dimensions);
		}

		#endregion //Clone

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
		/// An indexer for the <see cref="BooleanMatrix"/>. The Matrix can be indexed one-dimensionally or multi-dimensionally.
		/// </summary>
		/// <param name="subscript">A sequence of indices that correspond to the dimensions of the BooleanMatrix.</param>
		/// <returns>The element specified by the subscript.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the BooleanMatrix is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="subscript"/> is non-unitary and has a different number of dimensions than 
		/// the BooleanMatrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index is out of the bounds of the BooleanMatrix.
		/// </exception>
		public bool this[params int[] subscript]
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
		/// An indexer for the <see cref="BooleanMatrix"/> that takes a series of <see cref="Vector"/> subscripts. 
		/// The BooleanMatrix can be indexed one-dimensionally or multi-dimensionally.
		/// </summary>
		/// <param name="subscript">An array of index Vectors.</param>
		/// <returns>A BooleanMatrix containing the elements specified by the <paramref name="subscript"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the BooleanMatrix is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="subscript"/> is non-unitary and has a different number of dimensions than 
		/// the BooleanMatrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index is out of the bounds of the BooleanMatrix.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript index has a non-integer value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when the set value does not align with the elements specified by the subscript.
		/// </exception>
		public BooleanMatrix this[params Vector[] subscript]
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
						return BooleanMatrix.Empty;

					int index;
					double indexCheck;
					int length = indices.Length;
					BooleanMatrix result = new BooleanMatrix(length);
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
						return BooleanMatrix.Empty;

					if (isEmpty)
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_20"), "subscript");

					Vector size = this.Size;
					int length = Utilities.Product(newDimensions);
					bool[] newElements = new bool[length];
					BooleanMatrix result = new BooleanMatrix(newElements, newDimensions);

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