using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections;

namespace Infragistics.Math
{
    /// <summary>
    /// A Vector is a list of doubles with an additional dimensionality property that specifies its orientation.
    /// </summary>
    public class Vector : MatrixBase
        , IEnumerable<double>

        , ICloneable 

    {
        #region Member Variables

        private IList<double> elements;

        #endregion //Member Variables

        #region Static Variables

        #region Empty

        /// <summary>
        /// Returns the empty <see cref="Vector"/>.
        /// </summary>
        public static Vector Empty = new Vector();

        #endregion //Empty

        #endregion //Static Variables

        #region Constructors
        
        /// <summary>
        /// Initializes an empty <see cref="Vector"/> instance.
        /// </summary>
        /// <seealso cref="MatrixBase.IsEmpty"/>
        public Vector()
            : base()
        {
        }

        /// <summary>
        /// Initializes a row zero <see cref="Vector"/> of a specified <paramref name="length"/>. 
        /// </summary>
        /// <param name="length">The length of the constructed Vector.</param>
        public Vector(int length)
            : base(length)
        {
            this.elements = new double[length];
        }

        /// <summary>
        /// Initializes a row <see cref="Vector"/> of the specified <paramref name="length"/> and the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value assigned to each element of the constructed Vector.</param>
        /// <param name="length">The length of the constructed Vector.</param>
        public Vector(double value, int length)
            : base(length)
        {
            this.elements = new double[length];
            for (int i = 0; i < length; i++)
            {
                this.elements[i] = value;
            }
        }

        /// <summary>
        /// Initializes a row <see cref="Vector"/> with the specified <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The elements of the constructed Vector.</param>
        public Vector(IList<double> elements)
            :base(elements.Count)
        {
            this.elements = elements;
        }

        /// <summary>
        /// Initializes a <see cref="Vector"/> with the specified <paramref name="elements"/> and <paramref name="dimensions"/>.
        /// </summary>
        /// <param name="elements">The elements of the constructed Vector.</param>
        /// <param name="dimensions">The dimensions of the constructed Vector.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when the dimensions specify a size that is inconsistent with the length of the elements.
        /// </exception>
        public Vector(IList<double> elements, int[] dimensions)
            : base(dimensions)
        {
			if (!this.IsRow() && !this.IsColumn())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_1"), "dimensions");

            if (elements.Count != this.Length)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_69"));

            this.elements = elements;
        }

		/// <summary>
		/// Initializes a <see cref="Vector"/> with the specified elements <paramref name="x"/>.
		/// </summary>
		/// <param name="type">An enum that specifies the orientation of the Vector.</param>
		/// <param name="x">A double array of elements.</param>
		public Vector(VectorType type, params double[] x)
			: base(x.Length, type)
		{
			this.elements = x;
		}

        #endregion //Constructors

        #region Base Class Overrides

        #region CopyTo

        /// <summary>
        /// Copies the base <see cref="Vector"/> to an <paramref name="array"/> starting at a 
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
        /// Copies the base <see cref="Vector"/> to an <paramref name="array"/> starting at a 
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
        /// Compares the <see cref="Vector"/> with <paramref name="x"/> for equality.
        /// </summary>
        /// <param name="x">An object.</param>
        /// <returns>Returns True if the Vector is equal to x; False otherwise.</returns>
        public override bool Equals(object x)
        {
            Vector xCast = x as Vector;
            if (object.Equals(x,null))
                return false;

            return this == xCast;
        }

        #endregion Equals

        #region GetHashCode

        /// <summary>
        /// Returns hash code for the <see cref="Vector"/>.
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
        /// Returns the string representation of a <see cref="Vector"/>.
        /// </summary>
        public override string ToString()
        {
            string space;
            int length = this.Length;
            if (this.IsEmpty())
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

        #region IEnumerable<double> Members

        /// <summary>
        /// Returns an double enumerator for the <see cref="Vector"/>.
        /// </summary>
        IEnumerator<double> IEnumerable<double>.GetEnumerator()
        {
            return ((IEnumerable<double>)this.elements).GetEnumerator();
        }

        #endregion //IEnumerable<double> Members

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator for the <see cref="Vector"/>.
        /// </summary>
        public override IEnumerator GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        #endregion //IEnumerable Members

        #region ICloneable Members


        /// <summary>
        /// Returns a copy of the <see cref="Vector"/>.
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
        /// Adds two <see cref="Vector"/> instances pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] + x2[i].</returns>
        public static Vector operator +(Vector x1, Vector x2)
        {
            return Vector.Add(x1, x2);
        }

        /// <summary>
        /// Adds a <see cref="Vector"/> and a double pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] + x2.</returns>
        public static Vector operator +(Vector x1, double x2)
        {
            return Vector.Add(x1, x2);
        }

        /// <summary>
        /// Adds a double and a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1 + x2[i].</returns>
        public static Vector operator +(double x1, Vector x2)
        {
            return Vector.Add(x1, x2);
        }

        /// <summary>
        /// Adds a <see cref="Vector"/> and a <see cref="Complex"/> number pointwise.
        /// </summary>
        /// <returns>A <see cref="ComplexVector"/> y, where y[i] = x1[i] + x2.</returns>
        public static ComplexVector operator +(Vector x1, Complex x2)
        {
            return Vector.Add(x1, x2);
        }

        /// <summary>
        /// Adds a <see cref="Complex"/> number and a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A <see cref="ComplexVector"/> y, where y[i] = x1 + x2[i].</returns>
        public static ComplexVector operator +(Complex x1, Vector x2)
        {
            return Vector.Add(x1, x2);
        }

        #endregion // +

        #region -

        /// <summary>
        /// Subtracts two <see cref="Vector"/> instances pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] - x2[i].</returns>
        public static Vector operator -(Vector x1, Vector x2)
        {
            return Vector.Subtract(x1, x2);
        }

        /// <summary>
        /// Subtracts a <see cref="Vector"/> and a double pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] - x2.</returns>
        public static Vector operator -(Vector x1, double x2)
        {
            return Vector.Subtract(x1, x2);
        }

        /// <summary>
        /// Subtracts a double and a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1 - x2[i].</returns>
        public static Vector operator -(double x1, Vector x2)
        {
            return Vector.Subtract(x1, x2);
        }

        /// <summary>
        /// Subtracts a <see cref="Vector"/> and a <see cref="Complex"/> number pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] - x2.</returns>
        public static ComplexVector operator -(Vector x1, Complex x2)
        {
            return Vector.Subtract(x1, x2);
        }

        /// <summary>
        /// Subtracts a <see cref="Complex"/> number and a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1 - x2[i].</returns>
        public static ComplexVector operator -(Complex x1, Vector x2)
        {
            return Vector.Subtract(x1, x2);
        }

        #endregion // -

        #region *

        /// <summary>
        /// Multiplies two <see cref="Vector"/> instances pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] * x2[i].</returns>
        public static Vector operator *(Vector x1, Vector x2)
        {
            return Vector.Multiply(x1, x2);
        }

        /// <summary>
        /// Multiplies a <see cref="Vector"/> and a double pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] * x2.</returns>
        public static Vector operator *(Vector x1, double x2)
        {
            return Vector.Multiply(x1, x2);
        }

        /// <summary>
        /// Multiplies a double and a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1 * x2[i].</returns>
        public static Vector operator *(double x1, Vector x2)
        {
            return Vector.Multiply(x1, x2);
        }

        /// <summary>
        /// Multiplies a <see cref="Vector"/> and a <see cref="Complex"/> number pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] * x2.</returns>
        public static ComplexVector operator *(Vector x1, Complex x2)
        {
            return Vector.Multiply(x1, x2);
        }

        /// <summary>
        /// Multiplies a <see cref="Complex"/> number and a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1 * x2[i].</returns>
        public static ComplexVector operator *(Complex x1, Vector x2)
        {
            return Vector.Multiply(x1, x2);
        }

        #endregion // *

        #region /

        /// <summary>
        /// Divides two <see cref="Vector"/> instances pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] / x2[i].</returns>
        public static Vector operator /(Vector x1, Vector x2)
        {
            return Vector.Divide(x1, x2);
        }

        /// <summary>
        /// Divides a <see cref="Vector"/> and a double pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] / x2.</returns>
        public static Vector operator /(Vector x1, double x2)
        {
            return Vector.Divide(x1, x2);
        }

        /// <summary>
        /// Multiplies a double and a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1 / x2[i].</returns>
        public static Vector operator /(double x1, Vector x2)
        {
            return Vector.Divide(x1, x2);
        }

        /// <summary>
        /// Divides a <see cref="Vector"/> and a <see cref="Complex"/> number pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] / x2.</returns>
        public static ComplexVector operator /(Vector x1, Complex x2)
        {
            return Vector.Divide(x1, x2);
        }

        /// <summary>
        /// Divides a <see cref="Complex"/> number and a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1 / x2[i].</returns>
        public static ComplexVector operator /(Complex x1, Vector x2)
        {
            return Vector.Divide(x1, x2);
        }

        #endregion // /

        #region %

        /// <summary>
        /// Returns the modulus of two <see cref="Vector"/> instances pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] % x2[i].</returns>
        public static Vector operator %(Vector x1, Vector x2)
        {
            return Vector.Mod(x1, x2);
        }

        /// <summary>
        /// Returns the modulus of a <see cref="Vector"/> and a double pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1[i] % x2.</returns>
        public static Vector operator %(Vector x1, double x2)
        {
            return Vector.Mod(x1, x2);
        }

        /// <summary>
        /// Returns the modulus of a <see cref="Vector"/> pointwise.
        /// </summary>
        /// <returns>A Vector y, where y[i] = x1 % x2[i].</returns>
        public static Vector operator %(double x1, Vector x2)
        {
            return Vector.Mod(x1, x2);
        }

        #endregion // %

        #region ==

        /// <summary>
        /// Determines whether two <see cref="Vector"/> instances have the same dimensions and element values.
        /// </summary>
        /// <param name="x1">The first Vector.</param>
        /// <param name="x2">The second Vector.</param>
        /// <returns>Returns True if the two Vector instances are equal; False otherwise.</returns>
        public static bool operator ==(Vector x1, Vector x2)
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
        /// Returns the pointwise equality operator for a <see cref="Vector"/> and a double.
        /// </summary>
        /// <param name="x1">A Vector.</param>
        /// <param name="x2">A double.</param>
        /// <returns>A new <see cref="BooleanVector"/> y, where y[i] = Equals(x1[i],x2).</returns>
        public static BooleanVector operator ==(Vector x1, double x2)
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
        /// Returns the pointwise equality operator for a double and a <see cref="Vector"/>.
        /// </summary>
        /// <param name="x1">A double.</param>
        /// <param name="x2">A Vector.</param>
        /// <returns>A new <see cref="BooleanVector"/> y, where y[i] = Equals(x1,x2[i]).</returns>
        public static BooleanVector operator ==(double x1, Vector x2)
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
        /// Returns the pointwise equality operator for a <see cref="Vector"/> and a <see cref="Complex"/> number.
        /// </summary>
        /// <param name="x1">A Vector.</param>
        /// <param name="x2">A Complex number.</param>
        /// <returns>A new <see cref="BooleanVector"/> y, where y[i] = Equals(x1[i],x2).</returns>
        public static BooleanVector operator ==(Vector x1, Complex x2)
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
        /// Returns the pointwise equality operator for a <see cref="Complex"/> number and a <see cref="Vector"/>.
        /// </summary>
        /// <param name="x1">A Complex number.</param>
        /// <param name="x2">A Vector.</param>
        /// <returns>A new <see cref="BooleanVector"/> y, where y[i] = Equals(x1,x2[i]).</returns>
        public static BooleanVector operator ==(Complex x1, Vector x2)
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
        /// Determines whether two <see cref="Vector"/> instances have different dimensions and element values.
        /// </summary>
        /// <param name="x1">The first Vector.</param>
        /// <param name="x2">The second Vector.</param>
        /// <returns>Returns True if the two Vector instances are unequal; False otherwise.</returns>
        public static bool operator !=(Vector x1, Vector x2)
        {
            return !(x1 == x2);
        }

        /// <summary>
        /// Returns the pointwise inequality operator for a <see cref="Vector"/> and a double.
        /// </summary>
        /// <param name="x1">A Vector.</param>
        /// <param name="x2">A double.</param>
        /// <returns>A new <see cref="BooleanVector"/> y, where y[i] = NotEquals(x1[i],x2).</returns>
        public static BooleanVector operator !=(Vector x1, double x2)
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
        /// Returns the pointwise inequality operator for a double and a <see cref="Vector"/>.
        /// </summary>
        /// <param name="x1">A double.</param>
        /// <param name="x2">A Vector.</param>
        /// <returns>A new <see cref="BooleanVector"/> y, where y[i] = NotEquals(x1,x2[i]).</returns>
        public static BooleanVector operator !=(double x1, Vector x2)
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
        /// Returns the pointwise inequality operator for a <see cref="Vector"/> and a <see cref="Complex"/> number.
        /// </summary>
        /// <param name="x1">A Vector.</param>
        /// <param name="x2">A Complex number.</param>
        /// <returns>A new <see cref="BooleanVector"/> y, where y[i] = NotEquals(x1[i],x2).</returns>
        public static BooleanVector operator !=(Vector x1, Complex x2)
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
        /// Returns the pointwise inequality operator for a <see cref="Complex"/> number and a <see cref="Vector"/>.
        /// </summary>
        /// <param name="x1">A Complex number.</param>
        /// <param name="x2">A Vector.</param>
        /// <returns>A new <see cref="BooleanVector"/> y, where y[i] = NotEquals(x1,x2[i]).</returns>
        public static BooleanVector operator !=(Complex x1, Vector x2)
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
		/// Compares two <see cref="Vector"/> instances using the GreaterThan operator pointwise.
        /// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
        /// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1[i],x2[i]).</returns>
        public static BooleanVector operator >(Vector x1, Vector x2)
        {
            Vector x = x1.Clone().CompareTo(x2);

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
			//return new BooleanVector(newElements,x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanZero);
        }

		/// <summary>
		/// Compares a <see cref="Vector"/> and a <see cref="ComplexVector"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1[i],x2[i]).</returns>
		public static BooleanVector operator >(Vector x1, ComplexVector x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a <see cref="Vector"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1[i],x2[i]).</returns>
		public static BooleanVector operator >(ComplexVector x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a double using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1[i],x2).</returns>
        public static BooleanVector operator >(Vector x1, double x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="Vector"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1,x2[i]).</returns>
		public static BooleanVector operator >(double x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a <see cref="Complex"/> number using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1[i],x2).</returns>
		public static BooleanVector operator >(Vector x1, Complex x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="Vector"/> using the GreaterThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThan(x1,x2[i]).</returns>
		public static BooleanVector operator >(Complex x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanZero);
		}

        #endregion // >

		#region >=

		/// <summary>
		/// Compares two <see cref="Vector"/> instances using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2[i]).</returns>
		public static BooleanVector operator >=(Vector x1, Vector x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a <see cref="ComplexVector"/> using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2[i]).</returns>
		public static BooleanVector operator >=(Vector x1, ComplexVector x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a <see cref="Vector"/> using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2[i]).</returns>
		public static BooleanVector operator >=(ComplexVector x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a double using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2).</returns>
		public static BooleanVector operator >=(Vector x1, double x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="Vector"/> using the GreaterThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1,x2[i]).</returns>
		public static BooleanVector operator >=(double x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a <see cref="Complex"/> number using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1[i],x2).</returns>
		public static BooleanVector operator >=(Vector x1, Complex x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="Vector"/> using the GreaterThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = GreaterThanOrEquals(x1,x2[i]).</returns>
		public static BooleanVector operator >=(Complex x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		#endregion // >=

		#region <

		/// <summary>
		/// Compares two <see cref="Vector"/> instances using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1[i],x2[i]).</returns>
		public static BooleanVector operator <(Vector x1, Vector x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a <see cref="ComplexVector"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1[i],x2[i]).</returns>
		public static BooleanVector operator <(Vector x1, ComplexVector x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a <see cref="Vector"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1[i],x2[i]).</returns>
		public static BooleanVector operator <(ComplexVector x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a double using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1[i],x2).</returns>
		public static BooleanVector operator <(Vector x1, double x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="Vector"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1,x2[i]).</returns>
		public static BooleanVector operator <(double x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a <see cref="Complex"/> number using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1[i],x2).</returns>
		public static BooleanVector operator <(Vector x1, Complex x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="Vector"/> using the LessThan operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThan(x1,x2[i]).</returns>
		public static BooleanVector operator <(Complex x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanZero);
		}

		#endregion // <

		#region <=

		/// <summary>
		/// Compares two <see cref="Vector"/> instances using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1[i],x2[i]).</returns>
		public static BooleanVector operator <=(Vector x1, Vector x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a <see cref="ComplexVector"/> using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1[i],x2[i]).</returns>
		public static BooleanVector operator <=(Vector x1, ComplexVector x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="ComplexVector"/> and a <see cref="Vector"/> using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1[i],x2[i]).</returns>
		public static BooleanVector operator <=(ComplexVector x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a double using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1[i],x2).</returns>
		public static BooleanVector operator <=(Vector x1, double x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a double and a <see cref="Vector"/> using the LessThanOrEquals operator pointwise.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1,x2[i]).</returns>
		public static BooleanVector operator <=(double x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Vector"/> and a <see cref="Complex"/> number using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1[i],x2).</returns>
		public static BooleanVector operator <=(Vector x1, Complex x2)
		{
			Vector x = x1.Clone().CompareTo(x2);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.LessThanOrEqualToZero);
		}

		/// <summary>
		/// Compares a <see cref="Complex"/> number and a <see cref="Vector"/> using the LessThanOrEquals 
		/// operator pointwise.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A <see cref="BooleanVector"/> y, where y[i] = LessThanOrEquals(x1,x2[i]).</returns>
		public static BooleanVector operator <=(Complex x1, Vector x2)
		{
			Vector x = x2.Clone().CompareTo(x1);

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

			//return new BooleanVector(newElements, x.Dimensions);
			return Vector.FindValues(x, FindValuesType.GreaterThanOrEqualToZero);
		}

		#endregion // <=

        #region Casting

        /// <summary>
        /// Casts a double to a unitary <see cref="Vector"/>. 
        /// </summary>
        /// <param name="x">A double.</param>
        /// <returns>A unitary Vector y, where y[0] = x.</returns>
        public static implicit operator Vector(double x)
        {
            return new Vector(x, 1);
        }

        /// <summary>
        /// Casts a <see cref="Vector"/> to a double array. 
        /// </summary>
        /// <param name="x">A Vector.</param>
        /// <returns>A double array y, where y[i] = x[i].</returns>
        public static explicit operator double[](Vector x)
        {
            double[] newElements = new double[x.Length];
            Utilities.Copy(x.elements, newElements);
            return newElements;
        }

        /// <summary>
        /// Casts a double array to a row <see cref="Vector"/>.
        /// </summary>
        /// <param name="x">A double array.</param>
        /// <returns>A row Vector y, where y[i] = x[i].</returns>
        public static implicit operator Vector(double[] x)
        {
            return new Vector(x);
        }

        /// <summary>
        /// Casts an int array to a row <see cref="Vector"/>.
        /// </summary>
        /// <param name="x">An int array.</param>
        /// <returns>A row Vector y, where y[i] = (double)x[i].</returns>
        public static explicit operator Vector(int[] x)
        {
            int length = x.Length;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x[i];
            }
            return new Vector(newElements);
        }

        /// <summary>
        /// Casts a short array to a row <see cref="Vector"/>.
        /// </summary>
        /// <param name="x">A short array.</param>
        /// <returns>A row Vector y, where y[i] = (double)x[i].</returns>
        public static explicit operator Vector(short[] x)
        {
            int length = x.Length;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x[i];
            }
            return new Vector(newElements);
        }

        /// <summary>
        /// Casts a long array to a row <see cref="Vector"/>.
        /// </summary>
        /// <param name="x">A uint array.</param>
        /// <returns>A row Vector y, where y[i] = (double)x[i].</returns>
        public static explicit operator Vector(long[] x)
        {
            int length = x.Length;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x[i];
            }
            return new Vector(newElements);
        }

        /// <summary>
        /// Casts a float array to a row <see cref="Vector"/>.
        /// </summary>
        /// <param name="x">A float array.</param>
        /// <returns>A row Vector y, where y[i] = (double)x[i].</returns>
        public static explicit operator Vector(float[] x)
        {
            int length = x.Length;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x[i];
            }
            return new Vector(newElements);
        }

        /// <summary>
        /// Casts a decimal array to a row <see cref="Vector"/>.
        /// </summary>
        /// <param name="x">A decimal array.</param>
        /// <returns>A row Vector y, where y[i] = (double)x[i].</returns>
        public static explicit operator Vector(decimal[] x)
        {
            int length = x.Length;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = (double)x[i];
            }
            return new Vector(newElements);
        }

        #endregion //Casting

        #endregion //Operators

        #region Methods

        #region Basic Math Functions

        #region Abs

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Abs(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Abs(Vector)"/>
        public Vector Abs()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Acos(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Acos(Vector)"/>
        public Vector Acos()
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
        /// Modifies the <see cref="Vector"/>, y, by replacing each element y[i] with y[i] + x[i].
        /// </summary>
        /// <param name="x">A Vector.</param>
        /// <returns>The modified Vector.</returns>
        /// <exception cref="ArithmeticException">
        /// Occurs when the Vector and <paramref name="x"/> have different dimensions. 
        /// </exception>
        public Vector Add(Vector x)
        {
            if (this.IsEmpty() && x.IsEmpty())
                return this;

            if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
                throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

            for (int i = 0, length = this.Length; i < length; i++)
            {
                this.elements[i] = this.elements[i] + x.elements[i];
            }
            return this;
        }

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] + x.
        /// </summary>
        /// <param name="x">A double.</param>
        /// <returns>The modified Vector.</returns>
        public Vector Add(double x)
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

        internal static Vector Add(Vector x1, Vector x2)
        {
            return x1.Clone().Add(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Add(Vector x1, double x2)
        {
            return x1.Clone().Add(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Add(double x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new Vector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 + x2.elements[i];
            }

            return new Vector(newElements, newDimensions);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static ComplexVector Add(Vector x1, Complex x2)
        {
            if (x1.IsEmpty())
                return new ComplexVector();

            int length = x1.Length;
            int[] newDimensions = x1.Dimensions;
            Complex[] newElements = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1.elements[i] + x2;
            }

            return new ComplexVector(newElements, newDimensions);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static ComplexVector Add(Complex x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new ComplexVector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            Complex[] newElements = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 + x2[i];
            }

            return new ComplexVector(newElements, newDimensions);
        }

        #endregion //Add

        #region Arg

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Arg(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Arg(Vector)"/>
        public Vector Arg()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Asin(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Asin(Vector)"/>
        public Vector Asin()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Atan(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Atan(Vector)"/>
        public Vector Atan()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Ceiling(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Ceiling(Vector)"/>
        public Vector Ceiling()
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
        /// Modifies the <see cref="Vector"/>, y, by replacing each element y[i] with CompareTo(y[i],x[i]).
        /// </summary>
        /// <param name="x">A Vector.</param>
        /// <returns>The modified Vector.</returns>
        /// <exception cref="ArithmeticException">
        /// Occurs when the Vector and <paramref name="x"/> have different dimensions. 
        /// </exception>
        public Vector CompareTo(Vector x)
        {
            if (this.IsEmpty() && x.IsEmpty())
                return this;

            if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

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
        /// Modifies the <see cref="Vector"/>, y, by replacing each element y[i] with CompareTo(y[i],x[i]).
        /// </summary>
        /// <param name="x">A <see cref="ComplexVector"/>.</param>
        /// <returns>The modified Vector.</returns>
        /// <exception cref="ArithmeticException">
        /// Occurs when the Vector and <paramref name="x"/> have different dimensions. 
        /// </exception>
        public Vector CompareTo(ComplexVector x)
        {
            if (this.IsEmpty() && x.IsEmpty())
                return this;

            if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
                throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

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
        /// Modifies the <see cref="Vector"/>, y, by replacing each element y[i] with CompareTo(y[i],x).
        /// </summary>
        /// <param name="x">A double.</param>
        /// <returns>The modified Vector.</returns>
        public Vector CompareTo(double x)
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
        /// Modifies the <see cref="Vector"/>, y, by replacing each element y[i] with CompareTo(y[i],x).
        /// </summary>
        /// <param name="x">A <see cref="Complex"/> number.</param>
        /// <returns>The modified Vector.</returns>
        public Vector CompareTo(Complex x)
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Cos(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Cos(Vector)"/>
        public Vector Cos()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Cosh(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Cosh(Vector)"/>
        public Vector Cosh()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] / x[i].
        /// </summary>
        /// <param name="x">A Vector.</param>
        /// <returns>The modified Vector.</returns>
        /// <exception cref="ArithmeticException">
        /// Occurs when the Vector and <paramref name="x"/> have different dimensions. 
        /// </exception>
        public Vector Divide(Vector x)
        {
            if (this.IsEmpty() && x.IsEmpty())
                return this;

			if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

            for (int i = 0, length = this.Length; i < length; i++)
            {
                this.elements[i] = this.elements[i] / x.elements[i];
            }
            return this;
        }

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] / x.
        /// </summary>
        /// <param name="x">A double.</param>
        /// <returns>The modified Vector.</returns>
        public Vector Divide(double x)
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

        internal static Vector Divide(Vector x1, Vector x2)
        {
            return x1.Clone().Divide(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Divide(Vector x1, double x2)
        {
            return x1.Clone().Divide(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Divide(double x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new Vector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 / x2.elements[i];
            }

            return new Vector(newElements, newDimensions);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static ComplexVector Divide(Vector x1, Complex x2)
        {
            if (x1.IsEmpty())
                return new ComplexVector();

            int length = x1.Length;
            int[] newDimensions = x1.Dimensions;
            Complex[] newElements = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1.elements[i] / x2;
            }

            return new ComplexVector(newElements, newDimensions);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static ComplexVector Divide(Complex x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new ComplexVector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            Complex[] newElements = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 / x2[i];
            }

            return new ComplexVector(newElements, newDimensions);
        }

        #endregion //Divide

        #region Exp

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Exp(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Exp(Vector)"/>
        public Vector Exp()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Floor(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Floor(Vector)"/>
        public Vector Floor()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Log(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Log(Vector)"/>
        public Vector Log()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Log(y[i],B).
        /// </summary>
        /// <param name="B">The base of the logarithm.</param>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Log(Vector, double)"/>
        public Vector Log(double B)
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Log2(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Log2(Vector)"/>
        public Vector Log2()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Log10(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Log10(Vector)"/>
        public Vector Log10()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] % x[i].
        /// </summary>
        /// <param name="x">A Vector.</param>
        /// <returns>The modified Vector.</returns>
        /// <exception cref="ArithmeticException">
        /// Occurs when the Vector and <paramref name="x"/> have different dimensions. 
        /// </exception>
        public Vector Mod(Vector x)
        {
            if (this.IsEmpty() && x.IsEmpty())
                return this;

            if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

            for (int i = 0, length = this.Length; i < length; i++)
            {
                this.elements[i] = this.elements[i] % x.elements[i];
            }
            return this;
        }

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] % x.
        /// </summary>
        /// <param name="x">A double.</param>
        /// <returns>The modified Vector.</returns>
        public Vector Mod(double x)
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

        internal static Vector Mod(Vector x1, Vector x2)
        {
            return x1.Clone().Mod(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Mod(Vector x1, double x2)
        {
            return x1.Clone().Mod(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Mod(double x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new Vector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 % x2.elements[i];
            }

            return new Vector(newElements, newDimensions);
        }

        #endregion //Mod

        #region Multiply

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] * x[i].
        /// </summary>
        /// <param name="x">A Vector.</param>
        /// <returns>The modified Vector.</returns>
        /// <exception cref="ArithmeticException">
        /// Occurs when the Vector and <paramref name="x"/> have different dimensions. 
        /// </exception>
        public Vector Multiply(Vector x)
        {
            if (this.IsEmpty() && x.IsEmpty())
                return this;

            if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

            for (int i = 0, length = this.Length; i < length; i++)
            {
                this.elements[i] = this.elements[i] * x.elements[i];
            }
            return this;
        }

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] * x.
        /// </summary>
        /// <param name="x">A double.</param>
        /// <returns>The modified Vector.</returns>
        public Vector Multiply(double x)
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

        internal static Vector Multiply(Vector x1, Vector x2)
        {
            return x1.Clone().Multiply(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Multiply(Vector x1, double x2)
        {
            return x1.Clone().Multiply(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Multiply(double x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new Vector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 * x2.elements[i];
            }

            return new Vector(newElements, newDimensions);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static ComplexVector Multiply(Vector x1, Complex x2)
        {
            if (x1.IsEmpty())
                return new ComplexVector();

            int length = x1.Length;
            int[] newDimensions = x1.Dimensions;
            Complex[] newElements = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1.elements[i] * x2;
            }

            return new ComplexVector(newElements, newDimensions);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static ComplexVector Multiply(Complex x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new ComplexVector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            Complex[] newElements = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 * x2[i];
            }

            return new ComplexVector(newElements, newDimensions);
        }

        #endregion //Multiply

        #region Pow

        /// <summary>
        /// Modifies the <see cref="Vector"/>, y, by replacing each element y[i] with Pow(y[i],x[i]).
        /// </summary>
        /// <param name="x">A Vector.</param>
        /// <returns>The modified Vector.</returns>
        /// <exception cref="ArithmeticException">
        /// Occurs when the Vector and <paramref name="x"/> have different dimensions. 
        /// </exception>
        /// <seealso cref="Compute.Pow(Vector,Vector)"/>
        public Vector Pow(Vector x)
        {
            if (this.IsEmpty() && x.IsEmpty())
                return this;

            if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

            for (int i = 0, length = this.Length; i < length; i++)
            {
                this.elements[i] = Compute.Pow(this.elements[i],x.elements[i]);
            }
            return this;
        }

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Pow(y[i],x).
        /// </summary>
        /// <param name="x">A double.</param>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Pow(Vector,double)"/>
        public Vector Pow(double x)
        {
            if (this.IsEmpty())
                return this;

            for (int i = 0, length = this.Length; i < length; i++)
            {
                this.elements[i] = Compute.Pow(this.elements[i],x);
            }
            return this;
        }

        #endregion //Pow

        #region Round

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Round(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Round(Vector)"/>
        public Vector Round()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Sign(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Sign(Vector)"/>
        public Vector Sign()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Sin(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Sin(Vector)"/>
        public Vector Sin()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Sinh(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Sinh(Vector)"/>
        public Vector Sinh()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Sqrt(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Sqrt(Vector)"/>
        public Vector Sqrt()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] - x[i].
        /// </summary>
        /// <param name="x">A Vector.</param>
        /// <returns>The modified Vector.</returns>
        /// <exception cref="ArithmeticException">
        /// Occurs when the Vector and <paramref name="x"/> have different dimensions. 
        /// </exception>
        public Vector Subtract(Vector x)
        {
            if (this.IsEmpty() && x.IsEmpty())
                return this;

            if (!Utilities.ArrayEquals(this.Dimensions, x.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

            for (int i = 0, length = this.Length; i < length; i++)
            {
                this.elements[i] = this.elements[i] - x.elements[i];
            }
            return this;
        }

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[i] - x.
        /// </summary>
        /// <param name="x">A double.</param>
        /// <returns>The modified Vector.</returns>
        public Vector Subtract(double x)
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

        internal static Vector Subtract(Vector x1, Vector x2)
        {
            return x1.Clone().Subtract(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Subtract(Vector x1, double x2)
        {
            return x1.Clone().Subtract(x2);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static Vector Subtract(double x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new Vector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            IList<double> newElements = new double[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 - x2.elements[i];
            }

            return new Vector(newElements, newDimensions);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static ComplexVector Subtract(Vector x1, Complex x2)
        {
            if (x1.IsEmpty())
                return new ComplexVector();

            int length = x1.Length;
            int[] newDimensions = x1.Dimensions;
            Complex[] newElements = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1.elements[i] - x2;
            }

            return new ComplexVector(newElements, newDimensions);
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static ComplexVector Subtract(Complex x1, Vector x2)
        {
            if (x2.IsEmpty())
                return new ComplexVector();

            int length = x2.Length;
            int[] newDimensions = x2.Dimensions;
            Complex[] newElements = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                newElements[i] = x1 - x2[i];
            }

            return new ComplexVector(newElements, newDimensions);
        }

        #endregion //Subtract

        #region Tan

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Tan(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Tan(Vector)"/>
        public Vector Tan()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with Tanh(y[i]).
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Tanh(Vector)"/>
        public Vector Tanh()
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

        #region Vector Functions

		#region Bin

		/// <summary>
		/// Modifies the <see cref="Vector"/> by replacing its element with a bin number.
		/// </summary>
		/// <param name="N">The number of evenly-spaced bins.</param>
		/// <returns>The modified Vector.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than one.
		/// </exception>
		public Vector Bin(int N)
		{
			if (N < 1)
				throw new ArgumentException("There cannot be less than one bin", "N");

			if (this.IsEmpty())
				return this;

			double min = Compute.Min(this);
			double max = Compute.Max(this);
			double edge;
			if(max == min)
				edge = (double) 1 / (double) N;
			else
				edge = (max - min) / (2 * N);

			Vector bins = Compute.Line(min - edge, max + edge, N + 1);
			return this.Bin(bins);
		}

		/// <summary>
		/// Modifies the <see cref="Vector"/> by replacing each element with a bin number. 
		/// </summary>
		/// <param name="edges">edges[i] and edges[i+1] are the edges of the ith bin.</param>
		/// <returns>The modified Vector.</returns>
		/// <remarks>
		/// If an element of the Vector is not in any bin, it is replaced by Constant.NaN.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when there are less than two bin <paramref name="edges"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the bin edges are not in increasing order.
		/// </exception>
		public Vector Bin(Vector edges)
		{
			int binNumber = edges.Length;
			if (binNumber < 2)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_60"), "edges");

			for (int k = 1; k < binNumber; k++)
			{
				if (edges[k-1] > edges[k])
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
				for (int k = 0, bound = binNumber-1; k < bound; k++)
				{
					if (current >= edges[k] && current <= edges[k+1])
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
		/// Returns a copy of the <see cref="Vector"/> instance.
		/// </summary>
		public Vector Clone()
        {
            if (this.IsEmpty())
                return new Vector();

            return new Vector(Utilities.Clone<double>(this.elements), this.Dimensions);
        }

        #endregion //Clone

        #region CumProduct

        /// <summary>
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[0]*...*y[i].
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.CumProduct(Vector)"/>
        public Vector CumProduct()
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
        /// Modifies a <see cref="Vector"/>, y, by replacing each element y[i] with y[0]+...+y[i].
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.CumSum(Vector)"/>
        public Vector CumSum()
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
        /// Modifies a <see cref="Vector"/> by reversing the order of its elements.
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Reverse(Vector)"/>
        public Vector Reverse()
        {
            if (this.IsEmpty())
                return this;

            double[] newElements = new double[this.Length];
            this.elements.CopyTo(newElements, 0);
            Array.Reverse(newElements);
            this.elements = newElements;

            return this;
        }

        #endregion //Reverse

        #region Sort

        /// <summary>
        /// Modifies the <see cref="Vector"/> by sorting the elements by value in ascending order.
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Sort(Vector)"/>
        public Vector Sort()
        {
            if (this.IsEmpty())
                return this;

            double[] elementsArray = this.elements as double[];
            if (elementsArray != null)
            {
                Array.Sort(elementsArray);
                return this;
            }

            List<double> elementsList = this.elements as List<double>;
            if (elementsList != null)
            {
                elementsList.Sort();
                return this;
            }

            double[] newElements = new double[this.elements.Count];
            this.elements.CopyTo(newElements, 0);
            Array.Sort(newElements);
            this.elements = newElements;

            return this;
        }

        #endregion //Sort

        #region Transpose

        /// <summary>
        /// Modifies a <see cref="Vector"/> by switching its orientation. A row Vector is converted to a column 
        /// Vector and vice versa.
        /// </summary>
        /// <returns>The modified Vector.</returns>
        /// <seealso cref="Compute.Transpose(Vector)"/>
        public Vector Transpose()
        {
			int temp = this.Dimensions[0];
			this.Dimensions[0] = this.Dimensions[1];
			this.Dimensions[1] = temp;
			return this;
        }

        #endregion //Transpose

        #endregion //Vector Functions

		#region Private Methods

		// MD 4/19/11 - TFS72396
		#region FindValues

		private static BooleanVector FindValues(Vector x, FindValuesType type)
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

			return new BooleanVector(newElements, x.Dimensions);
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
        /// An indexer that gets and sets a single element of a <see cref="Vector"/>.
        /// </summary>
        /// <param name="indices">An index that specifies an element of the Vector.</param>
        /// <returns>The specified element of the Vector.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs if the Vector is indexed with more than two indices.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs if the Vector is indexed below 0 or above its length.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs if a row Vector is indexed with two indices and the first index is greater than 0.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs if a column Vector is indexed with two indices and the second index is greater than 0.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs if the empty Vector is indexed.
        /// </exception>
        public double this[params int[] indices]
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
        /// An indexer that gets or sets a set of elements in a <see cref="Vector"/>.
        /// </summary>
        /// <param name="indices">A Vector of integer indices.</param>
        /// <returns>A Vector of elements specified by the <paramref name="indices"/></returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs if the Vector is indexed below 0 or above its length.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs if the empty Vector is indexed.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the indices are not integers.
        /// </exception>
        public Vector this[Vector indices]
        {
            get 
            {
                //Exception for the empty Vector.
                if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_6"));

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
                int length = this.Length;
                IList<double> newElements = new double[indices.Length];
                for(int i = 0, count = newElements.Count; i < count; i++)
                {
                    currindex = indices.elements[i];

                    //Exception for out of range indices.
                    if (currindex < 0 || currindex >= length)
						throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

                    //Exception for non-integer indices.
                    if (currindex != Compute.Round(currindex))
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_5"), "indices");
 
                    newElements[i] = this.elements[(int)currindex];
                }

                //Return a new vector of the same type.
                return new Vector(newElements, newDimensions);
            }
            
            set 
            {
                //Exception for the empty Vector.
                if (this.IsEmpty())
					throw new ArgumentOutOfRangeException("this", Compute.GetString("LE_ArgumentOutOfRangeException_1"));

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
                for(int i = 0; i < length; i++)
                {
                    currindex = indices.elements[i];
                    currindexint = (int)Compute.Round(currindex);

                    //Exception for out of range indices.
                    if (currindex < 0 || currindex >= thisLength)
						throw new ArgumentOutOfRangeException("indices", Compute.GetString("LE_ArgumentOutOfRangeException_5"));

                    //Exception for non-integer indices.
                    if (currindex != currindexint)
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_5"), "indices");
					
					//Assigning the value.
					if(valueIsUnitary)
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