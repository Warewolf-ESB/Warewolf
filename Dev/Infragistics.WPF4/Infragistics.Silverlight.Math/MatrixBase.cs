using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Infragistics.Math
{
	/// <summary>
	/// A matrix is a set of numbers with an additional dimensionality property that specifys its spatial orientation.
	/// MatrixBase handles the logic for manipulating the spatial orientation of a matrix. 
	/// </summary>
    public abstract class MatrixBase : ICollection
	{
		#region Member Variables

		private int[] dimensions;

		private readonly int length;

		#endregion //Member Variables

		#region Constructors

		/// <summary>
		/// Initializes an empty <see cref="MatrixBase"/> instance.
		/// </summary>
		protected MatrixBase()
		{
			this.dimensions = new int[] { 0, 0 };
			this.length = 0;
		}

		/// <summary>
		/// Initializes a row <see cref="MatrixBase"/> of the specified <paramref name="length"/>.
		/// </summary>
		/// <param name="length">The length of the constructed MatrixBase.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when length is less than 0.
		/// </exception>
		protected MatrixBase(int length)
		{
			if (length < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_64"), "length");

			this.dimensions = new int[] { 1, length };
			this.length = length;
		}

		/// <summary>
		/// Initializes a <see cref="MatrixBase"/> with the specified length and the orientation specified by 
		/// <paramref name="type"/>.
		/// </summary>
		/// <param name="length">The length of the constructed MatrixBase.</param>
		/// <param name="type">The orientation of the constructed MatrixBase.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when length is less than 0.
		/// </exception>
		protected MatrixBase(int length, VectorType type)
		{
			if (length < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_64"), "length");

			this.length = length;
			if (type == VectorType.Row)
				this.dimensions = new int[] { 1, length };
			else
				this.dimensions = new int[] { length, 1 };
		}

		/// <summary>
		/// Initializes a <see cref="MatrixBase"/> with the specified <paramref name="dimensions"/>.
		/// </summary>
		/// <param name="dimensions">The dimensions of the Constructed MatrixBase.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when more than two dimensions are specified.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when neither dimension is singular.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when either dimension is less than 1.
		/// </exception>
		protected MatrixBase(int[] dimensions)
		{
			if (dimensions.Length < 2)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_65"), "dimensions");

			for (int i = 0; i < dimensions.Length; i++)
			{
				if (dimensions[i] < 1)
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_67"), "dimensions");
			}

			this.dimensions = (int[])dimensions.Clone();
			this.length = Utilities.Product(dimensions);
		}

		#endregion

		#region Base Class Overrides

		#region ToString

		/// <summary>
		/// Returns the string representation of a <see cref="MatrixBase"/>.
		/// </summary>
		/// <returns>The string representation of a <see cref="MatrixBase"/>.</returns>
		public abstract override string ToString();

		#endregion //ToString

		#region Equals

		/// <summary>
		/// Compares the <see cref="MatrixBase"/> with <paramref name="x"/> for equality.
		/// </summary>
		/// <param name="x">An object.</param>
		/// <returns>Returns True if the MatrixBase is equal to x; False otherwise.</returns>
		public abstract override bool Equals(object x);

		#endregion //Equals

		#region GetHashCode

		/// <summary>
		/// Returns a hash code for the <see cref="MatrixBase"/>.
		/// </summary>
		/// <returns>A hash code.</returns>
		public abstract override int GetHashCode();

		#endregion //GetHashCode

		#endregion //Base Class Overrides

		#region Interfaces

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			this.CopyTo(array, index);
		}

		int ICollection.Count
		{
			get { return this.Length; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return null; }
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the <see cref="MatrixBase"/> instance."
		/// </summary>
		/// <returns>An enumerator.</returns>
		public abstract IEnumerator GetEnumerator();

		#endregion

		#endregion //Interfaces

		#region Methods

		#region CopyTo

		/// <summary>
		/// Copies the base <see cref="ComplexVector"/> to an <paramref name="array"/> starting at a 
		/// particular <paramref name="index"/>.
		/// </summary>
		/// <param name="array">The destination array.</param>
		/// <param name="index">An index into the destination array where the copying begins.</param>
		protected virtual void CopyTo(Array array, int index)
		{
			Utilities.DebugFail("This must be implemented on the derived types.");
		}

		#endregion //CopyTo

		#region IsColumn

		/// <summary>
		/// Determines whether the <see cref="MatrixBase"/> is a column MatrixBase.
		/// </summary>
		/// <returns>Returns True if the second dimension of MatrixBase is equal to 1; False otherwise.</returns>
		public bool IsColumn()
		{
			return this.IsTwoDimensional() && this.dimensions[1] <= 1;
		}

		#endregion //IsColumn

		#region IsEmpty

		/// <summary>
		/// Determines whether the <see cref="MatrixBase"/> is empty.
		/// </summary>
		/// <returns>Returns True if the MatrixBase is empty; False otherwise.</returns>
		public bool IsEmpty()
		{
			return this.Length == 0;
		}

		#endregion //IsEmpty

		#region IsRow

		/// <summary>
		/// Determines whether the <see cref="MatrixBase"/> is a row MatrixBase.
		/// </summary>
		/// <returns>Returns True if the first dimension of MatrixBase is equal to 1; False otherwise.</returns>
		public bool IsRow()
		{
			return this.IsTwoDimensional() && this.dimensions[0] <= 1;
		}

		#endregion //IsRow

		#region IsSquare

		/// <summary>
		/// Determines whether the <see cref="MatrixBase"/> is square.
		/// </summary>
		/// <returns>Returns True if the MatrixBase has exactly two equal dimensions; False otherwise.</returns>
		public bool IsSquare()
		{
			return this.IsTwoDimensional() && this.dimensions[0] == this.dimensions[1];
		}

		#endregion //IsSquare

		#region IsTwoDimensional

		/// <summary>
		/// Determines if the <see cref="MatrixBase"/> has exactly two dimensions.
		/// </summary>
		/// <returns>True if the MatrixBase has exactly two dimensions; False otherwise.</returns>
		public bool IsTwoDimensional()
		{
			return this.dimensions.Length == 2;
		}

		#endregion //IsTwoDimensional

		#region IsUnitary

		/// <summary>
		/// Determines whether the <see cref="MatrixBase"/> is unitary.
		/// </summary>
		/// <returns>Returns True if the MatrixBase has only one element; False otherwise.</returns>
		public bool IsUnitary()
		{
			return Utilities.Product(this.dimensions) == 1;
		}

		#endregion //IsUnitary

		#region Squeeze

		/// <summary>
		/// Removes unitary dimensions from <see cref="MatrixBase"/> instances with greater than two dimensions.
		/// </summary>
		/// <returns>The modified MatrixBase.</returns>
		public MatrixBase Squeeze()
		{
			if (this.IsTwoDimensional())
				return this;

			int unitaryCount = 0;
			int rank = this.dimensions.Length;
			for (int i = 0; i < rank; i++)
			{
				if (this.dimensions[i] == 1)
					unitaryCount++;
			}

			int[] newDimensions;
			int newRank = rank - unitaryCount;
			if (newRank == 0)
			{
				this.dimensions = new int[] { 1, 1 };
				return this;
			}
			else
			{
				int curr;
				int next = 0;
				newDimensions = new int[newRank];
				for (int i = 0; i < rank; i++)
				{
					curr = this.dimensions[i];
					if (curr != 1)
						newDimensions[next++] = curr;
				}

				if (newRank == 1)
				{
					int temp = newDimensions[0];
					newDimensions = new int[2];
					if (this.dimensions[1] == 1)
					{
						newDimensions[0] = temp;
						newDimensions[1] = 1;
					}
					else
					{
						newDimensions[0] = 1;
						newDimensions[1] = temp;
					}
				}
			}

			this.dimensions = newDimensions;
			return this;
		}

		#endregion //Squeeze

		#endregion //Methods

		#region Properties

		#region Dimensions

		/// <summary>
		/// Returns the dimensions of the <see cref="MatrixBase"/>.
		/// </summary>
		/// <seealso cref="Size"/>
		protected internal int[] Dimensions
		{
			get { return this.dimensions; }
		}

		#endregion //Dimensions

		#region Length

		/// <summary>
		/// Returns the length of the <see cref="MatrixBase"/>.
		/// </summary>
		public int Length 
		{
			get { return this.length; }
		}

		#endregion //Length

		#region Rank

		/// <summary>
		/// Returns the number of dimensions in the <see cref="MatrixBase"/>.
		/// </summary>
		public int Rank 
		{ 
			get { return this.dimensions.Length; } 
		}

		#endregion

		#region Size

		/// <summary>
		/// Returns the dimensions of the <see cref="MatrixBase"/> as a <see cref="Vector"/>.
		/// </summary>
		public Vector Size
		{
			get { return (Vector)this.dimensions; }
		}

		#endregion //Size

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