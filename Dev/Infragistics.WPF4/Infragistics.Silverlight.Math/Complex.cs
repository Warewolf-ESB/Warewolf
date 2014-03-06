using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace Infragistics.Math
{
	/// <summary>
	/// A Complex number is a type of mathematical object with real and imaginary parts. 
	/// All of the standard mathematical operations and functions are defined for Complex
	/// numbers.
	/// </summary>
    public struct Complex : IComparable<Complex>, IComparable
	{
		#region Member Variables

		private readonly double re;
		private readonly double im;

		#endregion //Member Variables

		#region Constructors

		/// <summary>
		/// Initializes a new <see cref="Complex"/> number with real and imaginary components.
		/// </summary>
		/// <param name="re">A double representing the real part of the constructed number.</param>
		/// <param name="im">A double representing the imaginary part of the constructed number.</param>
		public Complex(double re, double im)
		{
			this.re = re;
			this.im = im;

			if (Compute.IsNaN(this))
			{
				this.re = Constant.NaN;
				this.im = Constant.NaN;
			}

			if (Compute.IsInf(this))
			{
				if (Compute.IsInf(this.Re))
				{
					this.im = Constant.NaN;
					return;
				}
				if (Compute.IsInf(this.Im))
				{
					this.re = Constant.NaN;
					return;
				}
			}

			if (Compute.IsNegInf(this))
			{
				if (Compute.IsNegInf(this.Re))
				{
					this.im = Constant.NaN;
					return;
				}
				if (Compute.IsNegInf(this.Im))
				{
					this.re = Constant.NaN;
					return;
				}
			}
		}

		#endregion //Constructors

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Compares the <see cref="Complex"/> number to <paramref name="x"/> for equality. 
		/// </summary>
		/// <param name="x">An object.</param>
		/// <returns>True if the Complex number is equal to x; False otherwise.</returns>
		public override bool Equals(object x)
		{
			if (x is Complex)
				return this == (Complex)x;
			else
				return false;
		}

		#endregion //Equals

		#region GetHashCode

		/// <summary>
		/// Returns a hash code for the <see cref="Complex"/> number.
		/// </summary>
		/// <returns>A hash code.</returns>
		public override int GetHashCode()
		{
			int reHash = this.re.GetHashCode();
			int imHash = this.im.GetHashCode();
			return reHash ^ imHash;
		}

		#endregion //GetHashCode

		#region ToString

		/// <summary>
		/// Returns the string representation of a <see cref="Complex"/> number.
		/// </summary>
		/// <returns>The string representation of a Complex number.</returns>
		public override string ToString()
		{
			return this.ToString(System.Globalization.CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Returns the string representation of a <see cref="Complex"/> number in a specified format.
		/// </summary>
		/// <param name="formatProvider">A format for a particular culture.</param>
		/// <returns>The string representation of a Complex number.</returns>
		public string ToString(IFormatProvider formatProvider)
		{
			if (this.im >= 0)
				return (String.Format("{0} + {1}i", this.re.ToString(formatProvider), this.im.ToString(formatProvider)));
			else
				return (String.Format("{0} - {1}i", this.re.ToString(formatProvider), (-this.im).ToString(formatProvider)));
		}

		#endregion //ToString

		#endregion //Base Class Overrides

		#region Interfaces

		#region IComparable Members

		/// <summary>
		/// Compares a <see cref="Complex"/> number to another object.
		/// </summary>
		/// <returns>
		/// -1 if obj is not <see cref="Complex"/>.
		/// -1 if this is less than obj.
		///  0 if this is equal to obj.
		///  1 if this is greater than obj.
		/// </returns>
		int IComparable.CompareTo(object obj)
		{
			if ((obj is Complex) == false)
				return -1;

			Complex other = (Complex)obj;
			return ((IComparable<Complex>)this).CompareTo(other);
		}

		#endregion

		#region IComparable<Complex> Members

		/// <summary>
		/// Compares two <see cref="Complex"/> numbers to each other.
		/// </summary>
		/// <param name="z">A Complex number.</param>
		/// <returns>
		/// Returns -1 if this is less than z.
		/// Returns 0 if this is equal to z.
		/// Returns 1 if this is greater than z.
		/// </returns>
		int IComparable<Complex>.CompareTo(Complex z)
		{
			if (this < z)
				return -1;

			if (this > z)
				return 1;

			return 0;
		}

		#endregion //IComparable<Complex> Members

		#endregion //Interfaces

		#region Operators

		#region +

		/// <summary>
		/// Returns the sum of two <see cref="Complex"/> numbers.
		/// </summary>
		/// <param name="z1">A Complex number of the form: a+bi.</param>
		/// <param name="z2">A Complex number of the form: c+di</param>
		/// <returns>A Complex number of the form: (a+c) + (b+d)i</returns>
		public static Complex operator +(Complex z1, Complex z2)
		{
			return new Complex(z1.Re + z2.Re, z1.Im + z2.Im);
		}

		/// <summary>
		/// Adds a <see cref="Complex"/> number <paramref name="z"/> to the <see cref="Complex"/> number.
		/// </summary>
		/// <param name="z">A Complex number.</param>
		/// <seealso cref="op_Addition(Complex,Complex)"/>
		public Complex Add(Complex z)
		{
			return this + z;
		}

		#endregion //+

		#region -

		/// <summary>
		/// Returns the difference of two <see cref="Complex"/> numbers.
		/// </summary>
		/// <param name="z1">A Complex number of the form: a+bi</param>
		/// <param name="z2">A Complex number of the form: c+di</param>
		/// <returns>A Complex number of the form: (a-c) + (b-d)i</returns>
		public static Complex operator -(Complex z1, Complex z2)
		{
			return new Complex(z1.Re - z2.Re, z1.Im - z2.Im);
		}

		/// <summary>
		/// Subtracts a <see cref="Complex"/> number <paramref name="z"/> from the <see cref="Complex"/> number.
		/// </summary>
		/// <param name="z">A Complex number.</param>
		/// <seealso cref="op_Subtraction(Complex,Complex)"/>
		public Complex Subtract(Complex z)
		{
			return this - z;
		}

		#endregion //-

		#region *

		/// <summary>
		/// Returns the product of two <see cref="Complex"/> numbers.
		/// </summary>
		/// <param name="z1">A Complex number of the form: a + bi</param>
		/// <param name="z2">A Complex number of the form: c + di</param>
		/// <returns>A Complex number of the form: (ac-bd) + (bc + ad)i</returns>
		public static Complex operator *(Complex z1, Complex z2)
		{
			return new Complex(z1.Re * z2.Re - z1.Im * z2.Im, z1.Im * z2.Re + z1.Re * z2.Im);
		}

		/// <summary>
		/// Returns the product of a <see cref="Complex"/> number and a double.
		/// </summary>
		/// <param name="x">A double of the form: c</param>
		/// <param name="z">A Complex number of the form: a + bi</param>
		/// <returns>A Complex number of the form: ac + ibc</returns>
		public static Complex operator *(Double x, Complex z)
		{
			return new Complex(x * z.Re, x * z.Im);
		}
		/// <summary>
		/// Returns the product of a <see cref="Complex"/> number and a double.
		/// </summary>
		/// <param name="z">A Complex number of the form: a + bi</param>
		/// <param name="x">A double of the form: c</param>
		/// <returns>A Complex number of the form: ac + bci</returns>
		public static Complex operator *(Complex z, Double x)
		{
			return new Complex(x * z.Re, x * z.Im);
		}

		/// <summary>
		/// Multiplys a <see cref="Complex"/> number <paramref name="z"/> to the <see cref="Complex"/> number.
		/// </summary>
		/// <param name="z">A Complex number.</param>
		/// <seealso cref="op_Multiply(Complex,Complex)"/>
		public Complex Multiply(Complex z)
		{
			return this * z;
		}

		#endregion //*

		#region /

		/// <summary>
		/// Returns the division of two <see cref="Complex"/> numbers.
		/// </summary>
		/// <param name="z1">A Complex number of the form: a + bi</param>
		/// <param name="z2">A Complex number of the form: c + di</param>
		/// <returns>A Complex number of the form: ((ac + bd)/(c^2 + d^2)) + ((bc - ad)/(c^2 + d^2))*i</returns>
		public static Complex operator /(Complex z1, Complex z2)
		{
			if (z2 == 0)
			{
				if(z1.Re >= 0)
					return new Complex(Constant.Inf, 0);

				return new Complex(-Constant.Inf, 0);
			}

			double a, b, c;
			c = z2.Re * z2.Re + z2.Im * z2.Im;
			a = z1.Re * z2.Re + z1.Im * z2.Im;
			b = z1.Im * z2.Re - z1.Re * z2.Im;
			return new Complex(a / c, b / c);
		}

		/// <summary>
		/// Divides a <see cref="Complex"/> number <paramref name="z"/> from the <see cref="Complex"/> number.
		/// </summary>
		/// <param name="z">A Complex number.</param>
		/// <seealso cref="op_Division(Complex,Complex)"/>
		public Complex Divide(Complex z)
		{
			return this / z;
		}

		#endregion // /

		#region <

		/// <summary>
		/// Returns True if <paramref name="z1"/> is less than <paramref name="z2"/>; False otherwise.
		/// </summary>
		/// <param name="z1">The first <see cref="Complex"/> number.</param>
		/// <param name="z2">The second Complex number.</param>
		/// <returns>
		/// False if z1 and z2 are equal; 
		/// True if z1 and z2 are unequal and z1 = Min(z1,z2);
		/// False otherwise.
		/// </returns>
		public static bool operator <(Complex z1, Complex z2)
		{
			if (z1 == z2)
				return false;

			if (z1 == Compute.Min(z1, z2))
				return true;

			return false;
		}

		#endregion // <

		#region <=

		/// <summary>
		/// Returns True if <paramref name="z1"/> is less than or equal to <paramref name="z2"/>; False otherwise.
		/// </summary>
		/// <param name="z1">The first <see cref="Complex"/> number.</param>
		/// <param name="z2">The second Complex number.</param>
		/// <returns>
		/// True if z1 = Min(z1,z2);
		/// False otherwise.
		/// </returns>
		public static bool operator <=(Complex z1, Complex z2)
		{
			if (z1 == Compute.Min(z1, z2))
				return true;

			return false;
		}

		#endregion // <=

		#region >

		/// <summary>
		/// Returns True if <paramref name="z1"/> is greater than <paramref name="z2"/>; False otherwise.
		/// </summary>
		/// <param name="z1">The first <see cref="Complex"/> number.</param>
		/// <param name="z2">The second Complex number.</param>
		/// <returns>
		/// False if z1 and z2 are equal; 
		/// True if z1 and z2 are unequal and z1 = Max(z1,z2);
		/// False otherwise.
		/// </returns>
		public static bool operator >(Complex z1, Complex z2)
		{
			if (z1 == z2)
				return false;

			if (z1 == Compute.Max(z1, z2))
				return true;

			return false;
		}

		#endregion // >

		#region >=

		/// <summary>
		/// Returns True if <paramref name="z1"/> is greater than or equal to <paramref name="z2"/>; False otherwise.
		/// </summary>
		/// <param name="z1">The first <see cref="Complex"/> number.</param>
		/// <param name="z2">The second Complex number.</param>
		/// <returns>
		/// True if z1 = Max(z1,z2);
		/// False otherwise.
		/// </returns>
		public static bool operator >=(Complex z1, Complex z2)
		{
			if (z1 == Compute.Max(z1, z2))
				return true;

			return false;
		}

		#endregion // >=

		#region ==

		/// <summary>
		/// Compares two <see cref="Complex"/> numbers for equality.
		/// </summary>
		/// <param name="z1">The first Complex number</param>
		/// <param name="z2">The second Complex number</param>
		/// <returns>True <paramref name="z1"/> is equal to <paramref name="z2"/>; they are equal</returns>
		public static bool operator ==(Complex z1, Complex z2)
		{
			if (z1.re == z2.re && z1.im == z2.im)
				return true;
			else
				return false;
		}

		#endregion //==

		#region !=

		/// <summary>
		/// Compares two <see cref="Complex"/> numbers for inequality.
		/// </summary>
		/// <param name="z1">The first Complex number</param>
		/// <param name="z2">The second Complex number</param>
		/// <returns>True if <paramref name="z1"/> is not equal to <paramref name="z1"/>; False otherwise.</returns>
		public static bool operator !=(Complex z1, Complex z2)
		{
			return !(z1 == z2);
		}

		#endregion //!=

		#region Casting

		/// <summary>
		/// Casts a double to a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The equivalent Complex number: x + 0i.</returns>
		public static implicit operator Complex(double x)
		{
			return new Complex(x, 0);
		}

		#endregion //Casting

		#endregion //Operators

		#region Methods

		#region Compare

		/// <summary>
		/// Compares the <see cref="Complex"/> number to another Complex number <paramref name="z"/>.
		/// </summary>
		/// <param name="z">A Complex number.</param>
		/// <returns>
		/// Returns -1 if this is less than z.
		/// Returns 0 if this is equal to z.
		/// Returns 1 if this is greater than z.
		/// </returns>
		public int Compare(Complex z)
		{
			if (this > z)
				return 1;

			if (this < z)
				return -1;

			return 0;
		} 

		#endregion //Compare

		#region Parse

		/// <summary>
		/// Takes a string representation of a <see cref="Complex"/> number and returns the specified Complex number.
		/// </summary>
		/// <param name="s">A string representation of a Complex number.</param>
		/// <returns>The Complex number represented by <paramref name="s"/>.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when s is formatted incorrectly.
		/// </exception>
		public static Complex Parse(string s)
		{
			Complex z;
			Exception error;
			bool successful = TryParse(s, out z, out error);

			if (!successful)
				throw error;

			return z;
		}

		#endregion //Parse

		#region TryParse

		/// <summary>
		/// Returns True if <paramref name="s"/> can be parsed into a <see cref="Complex"/> number; False otherwise. 
		/// </summary>
		/// <param name="s">A string representation of a Complex number.</param>
		/// <param name="z">The Complex number represented by s; null otherwise.</param>
		/// <returns>True if the parse was successful; False otherwise.</returns>
		public static bool TryParse(string s, out Complex z)
		{
			Exception error;
			return TryParse(s, out z, out error);
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static bool TryParse(string s, out Complex z, out Exception error)
		{
			//Initialization
			string[] split;
			error = null;
			z = 0;

			if (!TryParseHelper_Factor(s, out split, out error))
			{
				return false;
			}

			if (split[1].EndsWith("i") || split[1].EndsWith("j"))
			{
				split[1] = split[1].Remove(split[1].Length - 1, 1);
			}
			else if (split[1].StartsWith("i") || split[1].StartsWith("j"))
			{
				split[1] = split[1].Remove(0, 1);
			}
			else 
			{
				error = new ArgumentException(Compute.GetString("LE_ArgumentException_7"), "s");
			}

			//Constructing the Complex number.
			double x;
			double y;
			if (double.TryParse(split[0], out x) && double.TryParse(split[1], out y)) 
			{
				z = new Complex(x, y);
			}
			else
			{
				error = new ArgumentException(Compute.GetString("LE_ArgumentException_9"), "s");
				return false;
			}

			//The parse was successful.
			return true;		
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static bool TryParseHelper_Factor(string s, out string[] split, out Exception error)
		{
			split = null;
			error = null;

			s = s.Replace(" ","");

			bool startMinus = false;
			if (s.StartsWith("-"))
			{
				startMinus = true;
				s = s.Remove(0,1);
			}

			bool hasPlus = s.Contains("+");
			bool hasMinus = s.Contains("-");

			if (hasPlus)
			{
				split = s.Split('+');
				if (startMinus)
					split[0] = "-" + split[0];
				if (split.Length > 2)
				{
					error = new ArgumentException("", "s");
					return false;
				}
			}
			else if (hasMinus)
			{
				split = s.Split('-');
				if (startMinus)
					split[0] = "-" + split[0];
				if (split[1].StartsWith("i") || split[1].StartsWith("j"))
					split[1] = "i-" + split[1].Remove(0, 1);
				else
					split[1] = "-" + split[1];
				if (split.Length > 2)
				{
					error = new ArgumentException(Compute.GetString("LE_ArgumentException_10"), "s");
					return false;
				}
			}
			else
			{ 
				split = new string[2];
				if (s.Contains("i") || s.Contains("j"))
				{
					if(startMinus && (s.StartsWith("i") | s.StartsWith("j")))
						s = "i-" + s.Remove(0,1);
					else
						s = "-" + s;
					split[0] = "0";
					split[1] = s;
				}
				else
				{
					if (startMinus)
						s = "-" + s;
					split[0] = s;
					split[1] = "0i";
				}
			}

			return true;
		}

		#endregion //TryParse

		#endregion //Methods

		#region Properties

		/// <summary>
		/// Returns the real part of the <see cref="Complex"/> number.
		/// </summary>
		public double Re
		{
			get { return re; }
		}
		/// <summary>
		/// Returns the imaginary part of the <see cref="Complex"/> number.
		/// </summary>
		public double Im
		{
			get { return im; }
		}

		/// <summary>
		/// Returns the magnitude of the <see cref="Complex"/> number.
		/// </summary>
		/// <seealso cref="Compute.Abs(Complex)"/>
		public double Mag
		{
			get { return Compute.Abs(this); }
		}

		/// <summary>
		/// Returns the phase of the <see cref="Complex"/> number.
		/// </summary>
		/// <seealso cref="Compute.Arg(Complex)"/>
		public double Phase
		{
			get { return Compute.Arg(this); }
		}

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