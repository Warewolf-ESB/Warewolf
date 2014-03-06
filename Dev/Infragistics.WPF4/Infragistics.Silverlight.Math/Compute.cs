using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;





namespace Infragistics.Math
{
	/// <summary>
	/// Compute is an all-purpose class of mathematical functions designed to act on a variety of 
	/// mathematical objects.
	/// </summary>
    public static class Compute
	{
		#region Constants

		private const double log2 = 0.69314718055994529;

		private static readonly Complex log2c = System.Math.Log(2);

		private static readonly Complex log10c = System.Math.Log(10);

		#endregion //Constants

		#region Static Variables

		[ThreadStatic]
		private static Random rnd;

		#endregion //Static Variables

		#region Methods

		#region Basic Math Functions

		#region Abs

		/// <summary>
		/// Returns the absolute value of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The absolute value of x.</returns>
		public static double Abs(double x)
		{
			return System.Math.Abs(x);
		}

		/// <summary>
		/// Returns the absolute value of a <see cref="Complex"/> number <paramref name="x"/>. The absolute value of 
		/// a Complex number is its magnitude.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The absolute value of x.</returns>
		/// <see cref="Complex.Mag"/>
		/// <seealso cref="Arg(Complex)"/>
		public static double Abs(Complex x)
		{
			return System.Math.Sqrt(x.Re * x.Re + x.Im * x.Im);
		}
		
		/// <summary>
		/// Returns a <see cref="Vector"/> with the absolute value of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Abs(x[i]).</returns>
		/// <seealso cref="Abs(double)"/>
		/// <seealso cref="Vector.Abs()"/>
		public static Vector Abs(Vector x)
		{
			return x.Clone().Abs();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the absolute value of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Abs(x[i]).</returns>
		/// <seealso cref="Abs(Complex)"/>
		public static Vector Abs(ComplexVector x)
		{
			if (x.IsEmpty())
				return Vector.Empty;

			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Abs(x.Elements[i]);
			}
			return new Vector(newElements,x.Dimensions);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the absolute value of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Abs(x[i]).</returns>
		/// <seealso cref="Abs(double)"/>
		/// <seealso cref="Matrix.Abs()"/>
		public static Matrix Abs(Matrix x)
		{
			return x.Clone().Abs();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the absolute value of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Abs(x[i]).</returns>
		/// <seealso cref="Abs(Complex)"/>
		public static Matrix Abs(ComplexMatrix x)
		{
			if (x.IsEmpty())
				return Matrix.Empty;

			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Abs(x.Elements[i]);
			}
			return new Matrix(newElements, x.Dimensions);
		}

		#endregion //Abs

		#region Acos

		/// <summary>
		/// Returns the arccosine of <paramref name="x"/>. Arccosine is the inverse of Cos(x) on the range [0 pi]. 
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The arccosine of x.</returns>
		/// <seealso cref="Cos(double)"/>
		public static double Acos(double x)
		{
			return System.Math.Acos(x);
		}

		/// <summary>
		/// Returns the arccosine of a <see cref="Complex"/> number <paramref name="x"/>. Arccosine is the 
		/// inverse of Cos(x) on the Complex plane.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The arccosine of x.</returns>
		/// <seealso cref="Cos(Complex)"/>
		public static Complex Acos(Complex x)
		{
			Complex i = Constant.I;
			return -1 * i * Compute.Log(x + i * Compute.Sqrt(1 - x * x));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the arccosine of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Acos(x[i]).</returns>
		/// <seealso cref="Acos(double)"/>
		/// <seealso cref="Vector.Acos()"/>
		internal static Vector Acos(Vector x)
		{
			return x.Clone().Acos();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the arccosine of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Acos(x[i]).</returns>
		/// <seealso cref="Acos(Complex)"/>
		/// <seealso cref="ComplexVector.Acos()"/>
		internal static ComplexVector Acos(ComplexVector x)
		{
			return x.Clone().Acos();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the arccosine of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Acos(x[i]).</returns>
		/// <seealso cref="Acos(double)"/>
		/// <seealso cref="Matrix.Acos()"/>
		internal static Matrix Acos(Matrix x)
		{
			return x.Clone().Acos();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the arccosine of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Acos(x[i]).</returns>
		/// <seealso cref="Acos(Complex)"/>
		/// <seealso cref="ComplexMatrix.Acos()"/>
		internal static ComplexMatrix Acos(ComplexMatrix x)
		{
			return x.Clone().Acos();
		}

		#endregion //Acos

		#region Arg

		/// <summary>
		/// The argument of a <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The argument of x.</returns>
		/// <seealso cref="Abs(Complex)"/>
		public static double Arg(double x)
		{
			return 0;
		}

		/// <summary>
		/// The argument of a <see cref="Complex"/> number <paramref name="x"/>. The argument of the complex number is 
		/// equivalent to the phase angle of a Complex number.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The argument of x.</returns>
		/// <seealso cref="Complex.Phase"/>
		/// <seealso cref="Abs(Complex)"/>
		public static double Arg(Complex x)
		{
			return System.Math.Atan2(x.Im, x.Re);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the argument of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Arg(x[i]).</returns>
		/// <seealso cref="Arg(double)"/>
		/// <seealso cref="Vector.Arg()"/>
		public static Vector Arg(Vector x)
		{
			if (x.IsEmpty())
				return new Vector();

			return new Vector(0, x.Length);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the argument of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Arg(x[i]).</returns>
		/// <seealso cref="Arg(Complex)"/>
		public static Vector Arg(ComplexVector x)
		{
			if (x.IsEmpty())
				return new Vector();

			int length = x.Length;
			Vector result = new Vector(new double[length], x.Dimensions);
			for (int i = 0; i < length; i++)
			{
				result[i] = Compute.Arg(x.Elements[i]);
			}
			return result;
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the argument of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Arg(x[i]).</returns>
		/// <seealso cref="Arg(double)"/>
		/// <seealso cref="Matrix.Arg()"/>
		public static Matrix Arg(Matrix x)
		{
			if (x.IsEmpty())
				return new Matrix();

			return new Matrix(0, x.Length);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the argument of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Arg(x[i]).</returns>
		/// <seealso cref="Arg(Complex)"/>
		public static Matrix Arg(ComplexMatrix x)
		{
			if (x.IsEmpty())
				return new Matrix();

			int length = x.Length;
			Matrix result = new Matrix(new double[length], x.Dimensions);
			for (int i = 0; i < length; i++)
			{
				result[i] = Compute.Arg(x.Elements[i]);
			}
			return result;
		}

		#endregion //Arg

		#region Asin

		/// <summary>
		/// Returns the arcsine of <paramref name="x"/>. Arcsine is the inverse of sin(x) on the range [-pi/2 pi/2]. 
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The arcsine of x.</returns>
		/// <seealso cref="Sin(double)"/>
		public static double Asin(double x)
		{
			return System.Math.Asin(x);
		}

		/// <summary>
		/// Returns the arcsine of a <see cref="Complex"/> number <paramref name="x"/>. Arcsine is the inverse of Sin(x) on the Complex plane.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The arcsine of a x.</returns>
		/// <seealso cref="Sin(Complex)"/>
		public static Complex Asin(Complex x)
		{
			Complex i = Constant.I;
			return -1 * i * Compute.Log((i * x) + Compute.Sqrt(1 - x * x));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the arcsine of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Asin(x[i]).</returns>
		/// <seealso cref="Asin(double)"/>
		/// <seealso cref="Vector.Asin()"/>
		public static Vector Asin(Vector x)
		{
			return x.Clone().Asin();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the arcsine of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Asin(x[i]).</returns>
		/// <seealso cref="Asin(Complex)"/>
		/// <seealso cref="ComplexVector.Asin()"/>
		public static ComplexVector Asin(ComplexVector x)
		{
			return x.Clone().Asin();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the arcsine of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Asin(x[i]).</returns>
		/// <seealso cref="Asin(double)"/>
		/// <seealso cref="Matrix.Asin()"/>
		public static Matrix Asin(Matrix x)
		{
			return x.Clone().Asin();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the arcsine of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Asin(x[i]).</returns>
		/// <seealso cref="Asin(Complex)"/>
		/// <seealso cref="ComplexMatrix.Asin()"/>
		public static ComplexMatrix Asin(ComplexMatrix x)
		{
			return x.Clone().Asin();
		}

		#endregion //Asin

		#region Atan

		/// <summary>
		/// Returns the arctangent of <paramref name="x"/>. Arctangent is the inverse of Tan(x) on [-pi/2 pi/2]. 
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>the arctangent of x.</returns>
		/// <seealso cref="Tan(double)"/>
		/// <seealso cref="Atan2(double,double)"/>
		public static double Atan(double x)
		{
			return System.Math.Atan(x);
		}

		/// <summary>
		/// Returns the arctangent of a <see cref="Complex"/> number <paramref name="x"/>. Arctangent is the 
		/// inverse of Tan(x) on the Complex plane.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>the arctangent of x.</returns>
		/// <seealso cref="Tan(Complex)"/>
		/// <seealso cref="Atan2(Complex,Complex)"/>
		public static Complex Atan(Complex x)
		{
			Complex i = Constant.I;
			return 0.5 * i * Compute.Log((i + x) / (i - x));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the arctangent of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Atan(x[i]).</returns>
		/// <seealso cref="Atan(double)"/>
		/// <seealso cref="Vector.Atan()"/>
		public static Vector Atan(Vector x)
		{
			return x.Clone().Atan();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the arctangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Atan(x[i]).</returns>
		/// <seealso cref="Atan(Complex)"/>
		/// <seealso cref="ComplexVector.Atan()"/>
		public static ComplexVector Atan(ComplexVector x)
		{
			return x.Clone().Atan();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the arctangent of each element of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Atan(x[i]).</returns>
		/// <seealso cref="Atan(double)"/>
		/// <seealso cref="Matrix.Atan()"/>
		public static Matrix Atan(Matrix x)
		{
			return x.Clone().Atan();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the arctangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Atan(x[i]).</returns>
		/// <seealso cref="Atan(Complex)"/>
		/// <seealso cref="ComplexMatrix.Atan()"/>
		public static ComplexMatrix Atan(ComplexMatrix x)
		{
			return x.Clone().Atan();
		}

		#endregion //Atan

		#region Atan2

		/// <summary>
		/// Returns the four-quadrant arctangent of <paramref name="y"/>/<paramref name="x"/>. Atan2 is the Arg{x + yi}. 
		/// </summary>
		/// <param name="x">The first double.</param>
		/// <param name="y">The second double.</param>
		/// <returns>The four-quadrant arctangent of y/x.</returns>
		/// <see cref="Tan(double)"/>
		/// <seealso cref="Atan(double)"/>
		/// <seealso cref="Arg(Complex)"/>
		public static double Atan2(double x, double y)
		{
			return System.Math.Atan2(x,y);
		}

		/// <summary>
		/// Returns the arctangent of <paramref name="y"/>/<paramref name="x"/> on the <see cref="Complex"/> plane.
		/// </summary>
		/// <param name="x">The first Complex number.</param>
		/// <param name="y">The second Complex number.</param>
		/// <returns>Returns the arctangent of y/x.</returns>
		/// <see cref="Tan(Complex)"/>
		/// <seealso cref="Atan(Complex)"/>
		public static Complex Atan2(Complex x, Complex y)
		{
			return Compute.Atan(y / x);
		}

		#endregion //Atan2

		#region Ceiling

		/// <summary>
		/// Returns the smallest integer greater than or equal to <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The ceiling of x.</returns>
		/// <seealso cref="Floor(double)"/>
		/// <seealso cref="Round(double)"/>
		public static double Ceiling(double x)
		{
			return System.Math.Ceiling(x);
		}

		/// <summary>
		/// Returns a <see cref="Complex"/> number composed of the smallest integers greater than or equal to the real and
		/// imaginary parts of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The ceiling of x: Ceiling(Re{x}) + i*Ceiling(Im{x}).</returns>
		/// <seealso cref="Floor(Complex)"/>
		/// <seealso cref="Round(Complex)"/>
		public static Complex Ceiling(Complex x)
		{
			return new Complex(System.Math.Ceiling(x.Re),System.Math.Ceiling(x.Im));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the ceiling of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Ceiling(x[i]).</returns>
		/// <seealso cref="Ceiling(double)"/>
		/// <seealso cref="Vector.Ceiling()"/>
		public static Vector Ceiling(Vector x)
		{
			return x.Clone().Ceiling();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the ceiling of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Ceiling(x[i]).</returns>
		/// <seealso cref="Ceiling(Complex)"/>
		/// <seealso cref="ComplexVector.Ceiling()"/>
		public static ComplexVector Ceiling(ComplexVector x)
		{
			return x.Clone().Ceiling();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the ceiling of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Ceiling(x[i]).</returns>
		/// <seealso cref="Ceiling(double)"/>
		/// <seealso cref="Matrix.Ceiling()"/>
		public static Matrix Ceiling(Matrix x)
		{
			return x.Clone().Ceiling();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the ceiling of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Ceiling(x[i]).</returns>
		/// <seealso cref="Ceiling(Complex)"/>
		/// <seealso cref="ComplexMatrix.Ceiling()"/>
		public static ComplexMatrix Ceiling(ComplexMatrix x)
		{
			return x.Clone().Ceiling();
		}

		#endregion //Ceiling

		#region Cis

		/// <summary>
		/// Returns the exponential function of a purely imaginary number.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>A <see cref="Complex"/> number of the form: Exp(i*x) = Cos(x) + i*Sin(x).</returns>
		/// <seealso cref="Cos(double)"/>
		/// <seealso cref="Sin(double)"/>
		/// <seealso cref="Exp(Complex)"/>
		public static Complex Cis(double x)
		{
			return new Complex(System.Math.Cos(x), System.Math.Sin(x));
		}

		/// <summary>
		/// Returns the Cis function for a <see cref="Complex"/> number <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>A Complex number of the form: Exp(i*x) = Cos(x) + i*Sin(x).</returns>
		/// <seealso cref="Cos(Complex)"/>
		/// <seealso cref="Sin(Complex)"/>
		public static Complex Cis(Complex x)
		{
			return Compute.Cos(x) + (new Complex(0,1))*Compute.Sin(x);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the Cis of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>A Vector y, where y[i] = Cis(x[i]).</returns>
		/// <see cref="Cis(double)"/>
		public static ComplexVector Cis(Vector x)
		{
			if (x.IsEmpty())
				return new ComplexVector();

			int length = x.Length;
			ComplexVector result = new ComplexVector(new Complex[length], x.Dimensions);
			for (int i = 0; i < length; i++)
			{
				result[i] = Compute.Cis(x.Elements[i]);
			}
			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the Cis of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Cis(x[i]).</returns>
		/// <see cref="Cis(Complex)"/>
		/// <seealso cref="ComplexVector.Cis()"/>
		public static ComplexVector Cis(ComplexVector x)
		{
			return x.Clone().Cis();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the Cis of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>A Matrix y, where y[i] = Cis(x[i]).</returns>
		/// <see cref="Cis(double)"/>
		public static ComplexMatrix Cis(Matrix x)
		{
			if (x.IsEmpty())
				return new ComplexMatrix();

			int length = x.Length;
			ComplexMatrix result = new ComplexMatrix(new Complex[length], x.Dimensions);
			for (int i = 0; i < length; i++)
			{
				result[i] = Compute.Cis(x.Elements[i]);
			}
			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the Cis of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Cis(x[i]).</returns>
		/// <see cref="Cis(Complex)"/>
		/// <seealso cref="ComplexMatrix.Cis()"/>
		public static ComplexMatrix Cis(ComplexMatrix x)
		{
			return x.Clone().Cis();
		}

		#endregion //Cis

		#region Conj

		/// <summary>
		/// Returns the Complex conjugate of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number of the form: a + bi.</param>
		/// <returns>A Complex number of the form: a - bi.</returns>
		public static Complex Conj(double x)
		{
			return x;
		}

		/// <summary>
		/// Returns the Complex conjugate of a <see cref="Complex"/> number <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number of the form: a + bi.</param>
		/// <returns>A Complex number of the form: a - bi.</returns>
		public static Complex Conj(Complex x)
		{
			return new Complex(x.Re, -1 * x.Im);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the Complex conjugate of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A ComplexVector y, where y[i] = Conj(x[i]).</returns>
		/// <seealso cref="Conj(double)"/>
		public static ComplexVector Conj(Vector x)
		{
			return x;
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the Complex conjugate of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Conj(x[i]).</returns>
		/// <seealso cref="Conj(Complex)"/>
		/// <seealso cref="ComplexVector.Conj()"/>
		public static ComplexVector Conj(ComplexVector x)
		{
			return x.Clone().Conj();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the Complex conjugate of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Conj(x[i]).</returns>
		/// <seealso cref="Conj(double)"/>
		public static ComplexMatrix Conj(Matrix x)
		{
			return x;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the Complex conjugate of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Conj(x[i]).</returns>
		/// <seealso cref="Conj(Complex)"/>
		/// <seealso cref="ComplexMatrix.Conj()"/>
		public static ComplexMatrix Conj(ComplexMatrix x)
		{
			return x.Clone().Conj();
		}

		#endregion //Conj

		#region Cos

		/// <summary>
		/// Returns the cosine of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The cosine of x.</returns>
		/// <seealso cref="Sin(double)"/>
		/// <seealso cref="Tan(double)"/>
		/// <seealso cref="Acos(double)"/>
		/// <seealso cref="Asin(double)"/>
		/// <seealso cref="Atan(double)"/>
		/// <seealso cref="Cosh(double)"/>
		/// <seealso cref="Sinh(double)"/>
		/// <seealso cref="Tanh(double)"/>
		public static double Cos(double x)
		{
			return System.Math.Cos(x);
		}

		/// <summary>
		/// Returns the cosine of a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The cosine of <paramref name="x"/>.</returns>
		/// <seealso cref="Sin(Complex)"/>
		/// <seealso cref="Tan(Complex)"/>
		/// <seealso cref="Acos(Complex)"/>
		/// <seealso cref="Asin(Complex)"/>
		/// <seealso cref="Atan(Complex)"/>
		/// <seealso cref="Cosh(Complex)"/>
		/// <seealso cref="Sinh(Complex)"/>
		/// <seealso cref="Tanh(Complex)"/>
		public static Complex Cos(Complex x)
		{
			Complex i = Constant.I;
			return 0.5 * (Compute.Exp(i * x) + Compute.Exp(-1 * i * x));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the cosine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Cos(x[i]).</returns>
		/// <seealso cref="Cos(double)"/>
		/// <seealso cref="Vector.Cos()"/>
		public static Vector Cos(Vector x)
		{
			return x.Clone().Cos();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the cosine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector</param>
		/// <returns>A ComplexVector y, where y[i] = Cos(x[i]).</returns>
		/// <seealso cref="Cos(Complex)"/>
		/// <seealso cref="ComplexVector.Cos()"/>
		public static ComplexVector Cos(ComplexVector x)
		{
			return x.Clone().Cos();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the cosine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Cos(x[i]).</returns>
		/// <seealso cref="Cos(double)"/>
		/// <seealso cref="Matrix.Cos()"/>
		public static Matrix Cos(Matrix x)
		{
			return x.Clone().Cos();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the cosine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix</param>
		/// <returns>A ComplexMatrix y, where y[i] = Cos(x[i]).</returns>
		/// <seealso cref="Cos(Complex)"/>
		/// <seealso cref="ComplexMatrix.Cos()"/>
		public static ComplexMatrix Cos(ComplexMatrix x)
		{
			return x.Clone().Cos();
		}

		#endregion //Cos

		#region Cosh

		/// <summary>
		/// Returns the hyperbolic cosine of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The hyperbolic cosine of x.</returns>
		/// <seealso cref="Cos(double)"/>
		public static double Cosh(double x)
		{
			return System.Math.Cosh(x);
		}

		/// <summary>
		/// Returns the hyperbolic cosine of a <see cref="Complex"/> number <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The hyperbolic cosine of x.</returns>
		/// <seealso cref="Cos(Complex)"/>
		public static Complex Cosh(Complex x)
		{
			return 0.5 * (Compute.Exp(x) + Compute.Exp(-1 * x));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the hyperbolic cosine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Cosh(x[i]).</returns>
		/// <seealso cref="Cosh(double)"/>
		/// <seealso cref="Vector.Cosh()"/>
		public static Vector Cosh(Vector x)
		{
			return x.Clone().Cosh();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the hyperbolic cosine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Cosh(x[i]).</returns>
		/// <seealso cref="Cosh(Complex)"/>
		/// <seealso cref="ComplexVector.Cosh()"/>
		public static ComplexVector Cosh(ComplexVector x)
		{
			return x.Clone().Cosh();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the hyperbolic cosine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Cosh(x[i]).</returns>
		/// <seealso cref="Cosh(double)"/>
		/// <seealso cref="Matrix.Cosh()"/>
		public static Matrix Cosh(Matrix x)
		{
			return x.Clone().Cosh();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the hyperbolic cosine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Cosh(x[i]).</returns>
		/// <seealso cref="Cosh(Complex)"/>
		/// <seealso cref="ComplexMatrix.Cosh()"/>
		public static ComplexMatrix Cosh(ComplexMatrix x)
		{
			return x.Clone().Cosh();
		}

		#endregion //Cosh

		#region Exp

		/// <summary>
		/// Returns the number e raised to the power <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The exponential function of x.</returns>
		/// <seealso cref="Log(double)"/>
		public static double Exp(double x)
		{
			return System.Math.Exp(x);
		}

		/// <summary>
		/// Returns the number e raised to the power <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The exponential function of x.</returns>
		/// <seealso cref="Log(Complex)"/>
		public static Complex Exp(Complex x)
		{
			return System.Math.Exp(x.Re) * (new Complex(System.Math.Cos(x.Im), System.Math.Sin(x.Im)));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the exponential function of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Exp(x[i]).</returns>
		/// <seealso cref="Exp(double)"/>
		/// <seealso cref="Vector.Exp()"/>
		public static Vector Exp(Vector x)
		{
			return x.Clone().Exp();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the exponential function of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Exp(x[i]).</returns>
		/// <seealso cref="Exp(Complex)"/>
		/// <seealso cref="ComplexVector.Exp()"/>
		public static ComplexVector Exp(ComplexVector x)
		{
			return x.Clone().Exp();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the exponential function of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Exp(x[i]).</returns>
		/// <seealso cref="Exp(double)"/>
		/// <seealso cref="Matrix.Exp()"/>
		public static Matrix Exp(Matrix x)
		{
			return x.Clone().Exp();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the exponential function of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Exp(x[i]).</returns>
		/// <seealso cref="Exp(Complex)"/>
		/// <seealso cref="ComplexMatrix.Exp()"/>
		public static ComplexMatrix Exp(ComplexMatrix x)
		{
			return x.Clone().Exp();
		}

		#endregion //Exp

		#region Floor

		/// <summary>
		/// Returns the largest integer less than or equal to <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The floor of x.</returns>
		/// <seealso cref="Ceiling(double)"/>
		/// <seealso cref="Round(double)"/>
		public static double Floor(double x)
		{
			return System.Math.Floor(x);
		}

		/// <summary>
		/// Returns a <see cref="Complex"/> number composed of the largest integers less than or equal to the real and
		/// imaginary parts of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The floor of x: Floor(Re{x}) + i*Floor(Im{x}).</returns>
		/// <seealso cref="Ceiling(Complex)"/>
		/// <seealso cref="Round(Complex)"/>
		public static Complex Floor(Complex x)
		{
			return new Complex(System.Math.Floor(x.Re), System.Math.Floor(x.Im));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the Floor of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Floor(x[i]).</returns>
		/// <seealso cref="Floor(double)"/>
		/// <seealso cref="Vector.Floor()"/>
		public static Vector Floor(Vector x)
		{
			return x.Clone().Floor();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the Floor of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Floor(x[i]).</returns>
		/// <seealso cref="Floor(Complex)"/>
		/// <seealso cref="ComplexVector.Floor()"/>
		public static ComplexVector Floor(ComplexVector x)
		{
			return x.Clone().Floor();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the Floor of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Floor(x[i]).</returns>
		/// <seealso cref="Floor(double)"/>
		/// <seealso cref="Matrix.Floor()"/>
		public static Matrix Floor(Matrix x)
		{
			return x.Clone().Floor();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the Floor of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Floor(x[i]).</returns>
		/// <seealso cref="Floor(Complex)"/>
		/// <seealso cref="ComplexMatrix.Floor()"/>
		public static ComplexMatrix Floor(ComplexMatrix x)
		{
			return x.Clone().Floor();
		}

		#endregion //Floor

		#region Imaginary

		/// <summary>
		/// Returns the imaginary part of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The imaginary part of x.</returns>
		public static double Imaginary(double x)
		{
			return 0;
		}

		/// <summary>
		/// Returns the imaginary part of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The imaginary part of x.</returns>
		public static double Imaginary(Complex x)
		{
			return x.Im;
		}

		/// <summary>
		/// Returns the imaginary part of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>A Vector y, where y[i] = Imaginary(x[i]).</returns>
		public static Vector Imaginary(Vector x)
		{
			return new Vector(x.Length);
		}

		/// <summary>
		/// Returns the imaginary part of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexVector"/>.</param>
		/// <returns>A ComplexVector y, where y[i] = Imaginary(x[i]).</returns>
		public static Vector Imaginary(ComplexVector x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x.Elements[i].Im;
			}

			return new Vector(newElements, x.Dimensions);
		}

		/// <summary>
		/// Returns the imaginary part of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>A Matrix y, where y[i] = Imaginary(x[i]).</returns>
		public static Matrix Imaginary(Matrix x)
		{
			return new Matrix(x.Length);
		}

		/// <summary>
		/// Returns the imaginary part of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexMatrix"/>.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Imaginary(x[i]).</returns>
		public static Matrix Imaginary(ComplexMatrix x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x.Elements[i].Im;
			}

			return new Matrix(newElements, x.Dimensions);
		}

		#endregion //Imaginary

		#region IsEven

		/// <summary>
		/// Returns True if <paramref name="x"/> is an even number; False otherwise.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>True if x is an even number; false otherwise.</returns>
		public static bool IsEven(double x)
		{
			if (x % 2 == 0)
				return true;
			else
				return false;
		}

		#endregion //IsEven

		#region IsInf

		/// <summary>
		/// Returns True if <paramref name="x"/> is Inf; False otherwise.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>True if x is Inf; False otherwise.</returns>
		public static bool IsInf(double x)
		{
			if (x == Constant.Inf)
				return true;

			return false;
		}

		/// <summary>
		/// Returns True if <paramref name="x"/> is Inf; False otherwise.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>True if x is Inf; False otherwise.</returns>
		public static bool IsInf(Complex x)
		{
			if (x.Re == Constant.Inf || x.Im == Constant.Inf)
				return true;

			return false;
		}

		#endregion //IsInf

		#region IsNegInf

		/// <summary>
		/// Returns True if <paramref name="x"/> is -Inf; False otherwise.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>True if x is -Inf; False otherwise.</returns>
		public static bool IsNegInf(double x)
		{
			if (x == -Constant.Inf)
				return true;

			return false;
		}

		/// <summary>
		/// Returns True if <paramref name="x"/> is -Inf; False otherwise.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>True if x is -Inf; False otherwise.</returns>
		public static bool IsNegInf(Complex x)
		{
			if (x.Re == -Constant.Inf || x.Im == -Constant.Inf)
				return true;

			return false;
		}

		#endregion //IsNegInf

		#region IsNaN

		/// <summary>
		/// Returns True if <paramref name="x"/> is NaN; False otherwise.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>True if x is NaN; False otherwise.</returns>
		public static bool IsNaN(double x)
		{
			if (x.Equals(Constant.NaN))
				return true;

			return false;
		}

		/// <summary>
		/// Returns True if <paramref name="x"/> is NaN; False otherwise.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>True if x is NaN; False otherwise.</returns>
		public static bool IsNaN(Complex x)
		{
			if ((x.Re.Equals(Constant.NaN) && !Compute.IsInf(Compute.Abs(x.Im))) || (x.Im.Equals(Constant.NaN) && !Compute.IsInf(Compute.Abs(x.Re))))
				return true;

			return false;
		}

		#endregion //IsNaN

		#region IsOdd

		/// <summary>
		/// Returns True if <paramref name="x"/> is an odd number; False otherwise.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>True if x is an odd number; false otherwise.</returns>
		public static bool IsOdd(double x)
		{
			if (x % 2 == 1)
				return true;
			else
				return false;
		}

		#endregion //IsOdd

		#region Log

		/// <summary>
		/// Returns the natural (base e) logarithm of <paramref name="x"/>. Log(x) is the inverse of Exp(x).
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The natural logarithm of x.</returns>
		/// <seealso cref="Log2(double)"/>
		/// <seealso cref="Log10(double)"/>
		/// <seealso cref="Exp(double)"/>
		public static double Log(double x)
		{
			return System.Math.Log(x);
		}

		/// <summary>
		/// Returns the base <paramref name="B"/> logarithm of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <param name="B">The base of the logarithm.</param>
		/// <returns>The base B logarithm of x.</returns>
		/// <seealso cref="Log(double)"/>
		/// <seealso cref="Log2(double)"/>
		/// <seealso cref="Log10(double)"/>
		public static double Log(double x, double B)
		{
			return System.Math.Log10(x) / System.Math.Log10(B);
		}

		/// <summary>
		/// Returns the principal value of the <see cref="Complex"/> logarithm of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The natural logarithm of x.</returns>
		/// <seealso cref="Log2(Complex)"/>
		/// <seealso cref="Log10(Complex)"/>
		/// <seealso cref="Exp(Complex)"/>
		public static Complex Log(Complex x)
		{
			return new Complex(System.Math.Log(x.Mag), x.Phase);
		}

		/// <summary>
		/// Returns the principal value of the base <paramref name="B"/> <see cref="Complex"/> logarithm 
		/// of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <param name="B">The base of the logarithm.</param>
		/// <returns></returns>
		/// <seealso cref="Log(Complex)"/>
		/// <seealso cref="Log2(Complex)"/>
		/// <seealso cref="Log10(Complex)"/>
		public static Complex Log(Complex x, Complex B)
		{
			return Compute.Log(x) / Compute.Log(B);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the natural logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Log(x[i]).</returns>
		/// <seealso cref="Log(double)"/>
		/// <seealso cref="Vector.Log()"/>
		public static Vector Log(Vector x)
		{
			return x.Clone().Log();
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the base <paramref name="B"/> logarithm of each element 
		/// of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="B">The base of the logarithm.</param>
		/// <returns>A Vector y, where y[i] = Log(x[i],B).</returns>
		/// <seealso cref="Log(double,double)"/>
		/// <seealso cref="Vector.Log(double)"/>
		public static Vector Log(Vector x, double B)
		{
			return x.Clone().Log(B);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the natural logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Log(x[i]).</returns>
		/// <seealso cref="Log(Complex)"/>
		/// <seealso cref="ComplexVector.Log()"/>
		public static ComplexVector Log(ComplexVector x)
		{
			return x.Clone().Log();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the base <paramref name="B"/> logarithm of each element 
		/// of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <param name="B">The base of the logarithm.</param>
		/// <returns>A ComplexVector y, where y[i] = Log(x[i],B).</returns>
		/// <seealso cref="Log(Complex,Complex)"/>
		/// <seealso cref="ComplexVector.Log(Complex)"/>
		public static ComplexVector Log(ComplexVector x, Complex B)
		{
			return x.Clone().Log(B);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the natural logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Log(x[i]).</returns>
		/// <seealso cref="Log(double)"/>
		/// <seealso cref="Matrix.Log()"/>
		public static Matrix Log(Matrix x)
		{
			return x.Clone().Log();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the base <paramref name="B"/> logarithm of each element 
		/// of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="B">The base of the logarithm.</param>
		/// <returns>A Matrix y, where y[i] = Log(x[i],B).</returns>
		/// <seealso cref="Log(double,double)"/>
		/// <seealso cref="Matrix.Log(double)"/>
		public static Matrix Log(Matrix x, double B)
		{
			return x.Clone().Log(B);
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the natural logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Log(x[i]).</returns>
		/// <seealso cref="Log(Complex)"/>
		/// <seealso cref="ComplexMatrix.Log()"/>
		public static ComplexMatrix Log(ComplexMatrix x)
		{
			return x.Clone().Log();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the base <paramref name="B"/> logarithm of each element 
		/// of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="B">The base of the logarithm.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Log(x[i],B).</returns>
		/// <seealso cref="Log(Complex,Complex)"/>
		/// <seealso cref="ComplexMatrix.Log(Complex)"/>
		public static ComplexMatrix Log(ComplexMatrix x, Complex B)
		{
			return x.Clone().Log(B);
		}

		#endregion //Log

		#region Log2

		/// <summary>
		/// Returns the base 2 logarithm of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The base 2 logarithm of x.</returns>
		/// <seealso cref="Log(double)"/>
		/// <seealso cref="Log10(double)"/>
		public static double Log2(double x)
		{
			return System.Math.Log(x) / log2;
		}

		/// <summary>
		/// Returns the principal value of the base 2 <see cref="Complex"/> logarithm of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The base 2 logarithm of x.</returns>
		/// <seealso cref="Log(Complex)"/>
		/// <seealso cref="Log10(Complex)"/>
		public static Complex Log2(Complex x)
		{
			return Compute.Log(x) / log2c;
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the base 2 logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Log2(x[i]).</returns>
		/// <seealso cref="Log2(double)"/>
		/// <seealso cref="Vector.Log2()"/>
		public static Vector Log2(Vector x)
		{
			return x.Clone().Log2();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the base 2 logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Log2(x[i]).</returns>
		/// <seealso cref="Log2(Complex)"/>
		/// <seealso cref="ComplexVector.Log2()"/>
		public static ComplexVector Log2(ComplexVector x)
		{
			return x.Clone().Log2();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the base 2 logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Log2(x[i]).</returns>
		/// <seealso cref="Log2(double)"/>
		/// <seealso cref="Matrix.Log2()"/>
		public static Matrix Log2(Matrix x)
		{
			return x.Clone().Log2();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the base 2 logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Log2(x[i]).</returns>
		/// <seealso cref="Log2(Complex)"/>
		/// <seealso cref="ComplexMatrix.Log2()"/>
		public static ComplexMatrix Log2(ComplexMatrix x)
		{
			return x.Clone().Log2();
		}

		#endregion //Log2

		#region Log10

		/// <summary>
		/// Returns the base 10 logarithm of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The base 10 logarithm of x.</returns>
		/// <seealso cref="Log(double)"/>
		/// <seealso cref="Log(double,double)"/>
		public static double Log10(double x)
		{
			return System.Math.Log10(x);
		}

		/// <summary>
		/// Returns the principal value of the base 10 <see cref="Complex"/> logarithm of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The base 10 logarithm of x.</returns>
		/// <seealso cref="Log(Complex)"/>
		/// <seealso cref="Log(Complex,Complex)"/>
		public static Complex Log10(Complex x)
		{
			return Compute.Log(x) / log10c;
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the base 10 logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Log10(x[i]).</returns>
		/// <seealso cref="Log10(double)"/>
		/// <seealso cref="Vector.Log10()"/>
		public static Vector Log10(Vector x)
		{
			return x.Clone().Log10();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the base 10 logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Log10(x[i]).</returns>
		/// <seealso cref="Log10(Complex)"/>
		/// <seealso cref="ComplexVector.Log10()"/>
		public static ComplexVector Log10(ComplexVector x)
		{
			return x.Clone().Log10();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the base 10 logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Log10(x[i]).</returns>
		/// <seealso cref="Log10(double)"/>
		/// <seealso cref="Matrix.Log10()"/>
		public static Matrix Log10(Matrix x)
		{
			return x.Clone().Log10();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the base 10 logarithm of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Log10(x[i]).</returns>
		/// <seealso cref="Log10(Complex)"/>
		/// <seealso cref="ComplexMatrix.Log10()"/>
		public static ComplexMatrix Log10(ComplexMatrix x)
		{
			return x.Clone().Log10();
		}

		#endregion //Log10

		#region Max

		/// <summary>
		/// Compares <paramref name="x1"/> to <paramref name="x2"/> and returns the largest one.
		/// </summary>
		/// <param name="x1">The first double.</param>
		/// <param name="x2">The second double.</param>
		/// <returns>x1 if x1 is greater than x2; x2 otherwise.</returns>
		/// <seealso cref="Min(double,double)"/>
		public static double Max(double x1, double x2)
		{
			return System.Math.Max(x1,x2);
		}

		/// <summary>
		/// Compares <paramref name="x1"/> to <paramref name="x2"/> and returns the largest one.
		/// </summary>
		/// <param name="x1">The first <see cref="Complex"/> number.</param>
		/// <param name="x2">The second Complex number.</param>
		/// <returns>
		/// x1 if the real part of x1 is greater than the real part of x2.
		/// x2 if the real part of x2 is greater than the real part of x1.
		/// x1 if x1 and x2 have the same real part, and the imaginary part of x1 is greater than the imaginary part of x2.
		/// x2 otherwise.
		/// </returns>
		/// <remarks>
		/// Complex numbers have no intrinsic ordering. Therefore the Max function is not unique.
		/// </remarks>
		/// <seealso cref="Min(Complex,Complex)"/>
		/// <seealso cref="Abs(Complex)"/>
		/// <seealso cref="Arg(Complex)"/>
		public static Complex Max(Complex x1, Complex x2)
		{
			double x1Mag = x1.Mag;
			double x2Mag = x2.Mag;

			if (x1.Re > x2.Re)
				return x1;

			if (x1.Re < x2.Re)
				return x2;

			// MD 4/19/11 - TFS72530
			// This check was backwards
			//if (x1.Im < x2.Im)
			if (x1.Im > x2.Im)
				return x1;

			return x2;
		}

		/// <summary>
		/// Returns the largest element of the <see cref="Vector"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>The largest element of x.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <seealso cref="Max(double,double)"/>
		public static double Max(Vector x)
		{
			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_1"));

			double currentValue;
			double result = x.Elements[0];
			for (int i = 1, length = x.Length; i < length; i++)
			{
				currentValue = x.Elements[i];
				if (currentValue > result)
					result = currentValue;
			}
			return result;
		}

		/// <summary>
		/// Returns the largest element of the <see cref="ComplexVector"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The largest element of x.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <seealso cref="Max(Complex,Complex)"/>
		public static Complex Max(ComplexVector x)
		{
			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_2"));

			Complex currentValue;
			Complex result = x.Elements[0];
			for (int i = 1, length = x.Length; i < length; i++)
			{
				currentValue = x.Elements[i];
				if (currentValue > result)
					result = currentValue;
			}
			return result;
		}

		/// <summary>
		/// Returns the largest element of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The largest element of x.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <seealso cref="Max(double,double)"/>
		public static double Max(Matrix x)
		{
			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_3"));

			double currentValue;
			double result = x.Elements[0];
			for (int i = 1, length = x.Length; i < length; i++)
			{
				currentValue = x.Elements[i];
				if (currentValue > result)
					result = currentValue;
			}
			return result;
		}

		/// <summary>
		/// Returns the largest element of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The largest element of x.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <seealso cref="Max(Complex,Complex)"/>
		public static Complex Max(ComplexMatrix x)
		{
			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_2"));

			Complex currentValue;
			Complex result = x.Elements[0];
			for (int i = 1, length = x.Length; i < length; i++)
			{
				currentValue = x.Elements[i];
				if (currentValue > result)
					result = currentValue;
			}
			return result;
		}

		/// <summary>
		/// Returns the largest elements of the <see cref="Matrix"/> <paramref name="x"/> along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix with the largest elements of x along the Nth dimension.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <seealso cref="Max(double,double)"/>
		public static Matrix Max(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_2"));

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N-1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] = 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;

			double curr;
			double max;
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				max = -Constant.Inf;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					if (curr > max)
						max = curr;
				}

				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = max; 
				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns the largest elements of the <see cref="ComplexMatrix"/> <paramref name="x"/> along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix with the largest elements of x along the Nth dimension.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <seealso cref="Max(double,double)"/>
		public static ComplexMatrix Max(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_2"));

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] = 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;

			Complex curr;
			Complex max;
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				max = -Constant.Inf;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					if (curr > max)
						max = curr;
				}

				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = max;
				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Max

		#region Min

		/// <summary>
		/// Compares <paramref name="x1"/> to <paramref name="x2"/> and returns the smallest one.
		/// </summary>
		/// <param name="x1">The first double.</param>
		/// <param name="x2">The second double.</param>
		/// <returns>x1 if x1 is smaller than x2; x2 otherwise.</returns>
		/// <seealso cref="Max(double,double)"/>
		public static double Min(double x1, double x2)
		{
			return System.Math.Min(x1, x2);
		}

		/// <summary>
		/// Compares <paramref name="x1"/> to <paramref name="x2"/> and returns the smallest one.
		/// </summary>
		/// <param name="x1">The first <see cref="Complex"/> number.</param>
		/// <param name="x2">The second Complex number.</param>
		/// <returns>
		/// x1 if the magnitude of x1 is smaller than the magnitude of x2.
		/// y if the magnitude of x2 is smaller than the magntiude of x1.
		/// x1 if x1 and x2 have the same magnitude, and the phase of x1 is greater than the phase of x2.
		/// x2 otherwise.
		/// </returns>
		/// <remarks>
		/// Complex numbers have no intrinsic ordering. Therefore the Min function is not unique.
		/// </remarks>
		/// <seealso cref="Max(Complex,Complex)"/>
		/// <seealso cref="Abs(Complex)"/>
		/// <seealso cref="Arg(Complex)"/>
		public static Complex Min(Complex x1, Complex x2)
		{
			Complex max = Compute.Max(x1, x2);
			if (max == x1)
				return x2;

			return x1;
		}

		/// <summary>
		/// Returns the smallest element of the <see cref="Vector"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>The smallest element of x.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <seealso cref="Min(double,double)"/>
		public static double Min(Vector x)
		{
			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_5"));

			double currentValue;
			double result = x.Elements[0];
			for (int i = 1, length = x.Length; i < length; i++)
			{
				currentValue = x.Elements[i];
				if (currentValue < result)
					result = currentValue;
			}
			return result;
		}

		/// <summary>
		/// Returns the smallest element of the <see cref="ComplexVector"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The smallest element of x.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <seealso cref="Min(Complex,Complex)"/>
		public static Complex Min(ComplexVector x)
		{
			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_6"));

			Complex currentValue;
			Complex result = x.Elements[0];
			for (int i = 1, length = x.Length; i < length; i++)
			{
				currentValue = x.Elements[i];
				if (currentValue < result)
					result = currentValue;
			}
			return result;
		}

		/// <summary>
		/// Returns the smallest element of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The smallest element of x.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <seealso cref="Min(double,double)"/>
		public static double Min(Matrix x)
		{
			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_7"));

			double currentValue;
			double result = x.Elements[0];
			for (int i = 1, length = x.Length; i < length; i++)
			{
				currentValue = x.Elements[i];
				if (currentValue < result)
					result = currentValue;
			}
			return result;
		}

		/// <summary>
		/// Returns the smallest element of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The smallest element of x.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <seealso cref="Min(Complex,Complex)"/>
		public static Complex Min(ComplexMatrix x)
		{
			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_6"));

			Complex currentValue;
			Complex result = x.Elements[0];
			for (int i = 1, length = x.Length; i < length; i++)
			{
				currentValue = x.Elements[i];
				if (currentValue < result)
					result = currentValue;
			}
			return result;
		}

		/// <summary>
		/// Returns the smallest elements of the <see cref="Matrix"/> <paramref name="x"/> along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix with the smallest elements of x along the Nth dimension.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <seealso cref="Min(double,double)"/>
		public static Matrix Min(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_6"));

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] = 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;

			double curr;
			double min;
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				min = Constant.Inf;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					if (curr < min)
						min = curr;
				}

				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = min;
				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns the smallest elements of the <see cref="ComplexMatrix"/> <paramref name="x"/> along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix with the smallest elements of x along the Nth dimension.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <seealso cref="Min(double,double)"/>
		public static ComplexMatrix Min(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (x.IsEmpty())
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_8"));

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] = 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;

			Complex curr;
			Complex min;
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				min = Constant.Inf;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					if (curr < min)
						min = curr;
				}

				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = min;
				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Min

		#region Pow

		/// <summary>
		/// Returns <paramref name="x1"/> raised to the power <paramref name="x2"/>.
		/// </summary>
		/// <param name="x1">The first double.</param>
		/// <param name="x2">The second double.</param>
		/// <returns>x1 raised to the power x2.</returns>
		public static double Pow(double x1, double x2)
		{
			return System.Math.Pow(x1, x2);
		}

		/// <summary>
		/// Returns the principal value of <paramref name="x1"/> raised to the power <paramref name="x2"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A <see cref="Complex"/> number.</param>
		/// <returns>x1 raised to the power x2.</returns>
		public static Complex Pow(double x1, Complex x2)
		{
			return Compute.Pow((Complex)x1, x2);
		}

		/// <summary>
		/// Returns the principal value of <paramref name="x1"/> raised to the power <paramref name="x2"/>.
		/// </summary>
		/// <param name="x1">A <see cref="Complex"/> number.</param>
		/// <param name="x2">A double.</param>
		/// <returns>x1 raised to the power x2.</returns>
		public static Complex Pow(Complex x1, double x2)
		{
			return Compute.Pow(x1, (Complex)x2);
		}

		/// <summary>
		/// Returns the principal value of <paramref name="x1"/> raised to the power <paramref name="x2"/>.
		/// </summary>
		/// <param name="x1">The first <see cref="Complex"/> number.</param>
		/// <param name="x2">The second Complex number.</param>
		/// <returns>x1 raised to the power x2.</returns>
		public static Complex Pow(Complex x1, Complex x2)
		{
			if (x2 == 0 && x1 == 0)
				return 1;

			if (x1 == 0)
				return 0;

			return Compute.Exp(x2 * (new Complex(System.Math.Log(x1.Mag), x1.Phase)));
		}

		/// <summary>
		/// Returns the pointwise power operation on two <see cref="Vector"/> instances.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>A Vector y, where y[i] = Pow(x1[i],x2[i]).</returns>
		/// <seealso cref="Pow(double,double)"/>
		/// <seealso cref="Vector.Pow(Vector)"/>
		public static Vector Pow(Vector x1, Vector x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Vector"/> and a double.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A Vector y, where y[i] = Pow(x1[i],x2).</returns>
		/// <seealso cref="Pow(double,double)"/>
		/// <seealso cref="Vector.Pow(double)"/>
		public static Vector Pow(Vector x1, double x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a double and a <see cref="Vector"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Pow(x1,x2[i]).</returns>
		/// <seealso cref="Pow(double,double)"/>
		public static Vector Pow(double x1, Vector x2)
		{
			if (x2.IsEmpty())
				return new Vector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<double> newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1, x2.Elements[i]);
			}

			return new Vector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Vector"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1[i],x2).</returns>
		/// <seealso cref="Pow(double,Complex)"/>
		public static ComplexVector Pow(Vector x1, Complex x2)
		{
			if (x1.IsEmpty())
				return new ComplexVector();

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1.Elements[i], x2);
			}

			return new ComplexVector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Complex"/> number and a <see cref="Vector"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1,x2[i]).</returns>
		/// <seealso cref="Pow(Complex,double)"/>
		public static ComplexVector Pow(Complex x1, Vector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1, x2[i]);
			}

			return new ComplexVector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on two <see cref="ComplexVector"/> instances.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1[i],x2[i]).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		/// <seealso cref="ComplexVector.Pow(ComplexVector)"/>
		public static ComplexVector Pow(ComplexVector x1, ComplexVector x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="ComplexVector"/> and a double.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1[i],x2).</returns>
		/// <seealso cref="Pow(Complex,double)"/>
		/// <seealso cref="ComplexVector.Pow(double)"/>
		public static ComplexVector Pow(ComplexVector x1, double x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a double and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1,x2[i]).</returns>
		/// <seealso cref="Pow(double,Complex)"/>
		public static ComplexVector Pow(double x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1, x2.Elements[i]);
			}

			return new ComplexVector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="ComplexVector"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1[i],x2).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		/// <seealso cref="ComplexVector.Pow(Complex)"/>
		public static ComplexVector Pow(ComplexVector x1, Complex x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Complex"/> number and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1,x2[i]).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		public static ComplexVector Pow(Complex x1, ComplexVector x2)
		{
			if (x2.IsEmpty())
				return new ComplexVector();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1, x2[i]);
			}

			return new ComplexVector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="ComplexVector"/> number and a <see cref="Vector"/>.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1[i],x2[i]).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		/// <seealso cref="ComplexVector.Pow(Vector)"/>
		public static ComplexVector Pow(ComplexVector x1, Vector x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Vector"/> number and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Pow(x1[i],x2[i]).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		public static ComplexVector Pow(Vector x1, ComplexVector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexVector();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

			int length = x2.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1[i], x2.Elements[i]);
			}
			return new ComplexVector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on two <see cref="Matrix"/> instances.
		/// </summary>
		/// <param name="x1">The first Matrix.</param>
		/// <param name="x2">The second Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Pow(x1[i],x2[i]).</returns>
		/// <seealso cref="Pow(double,double)"/>
		/// <seealso cref="Matrix.Pow(Matrix)"/>
		public static Matrix Pow(Matrix x1, Matrix x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Matrix"/> and a double.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A Matrix y, where y[i] = Pow(x1[i],x2).</returns>
		/// <seealso cref="Pow(double,double)"/>
		/// <seealso cref="Matrix.Pow(double)"/>
		public static Matrix Pow(Matrix x1, double x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a double and a <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Pow(x1,x2[i]).</returns>
		/// <seealso cref="Pow(double,double)"/>
		public static Matrix Pow(double x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new Matrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<double> newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1, x2.Elements[i]);
			}

			return new Matrix(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Matrix"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1[i],x2).</returns>
		/// <seealso cref="Pow(double,Complex)"/>
		public static ComplexMatrix Pow(Matrix x1, Complex x2)
		{
			if (x1.IsEmpty())
				return new ComplexMatrix();

			int length = x1.Length;
			int[] newDimensions = x1.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1.Elements[i], x2);
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Complex"/> number and a <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1,x2[i]).</returns>
		/// <seealso cref="Pow(Complex,double)"/>
		public static ComplexMatrix Pow(Complex x1, Matrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1, x2[i]);
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on two <see cref="ComplexMatrix"/> instances.
		/// </summary>
		/// <param name="x1">The first ComplexMatrix.</param>
		/// <param name="x2">The second ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1[i],x2[i]).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		/// <seealso cref="ComplexMatrix.Pow(ComplexMatrix)"/>
		public static ComplexMatrix Pow(ComplexMatrix x1, ComplexMatrix x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="ComplexMatrix"/> and a double.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A double.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1[i],x2).</returns>
		/// <seealso cref="Pow(Complex,double)"/>
		/// <seealso cref="ComplexMatrix.Pow(double)"/>
		public static ComplexMatrix Pow(ComplexMatrix x1, double x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a double and a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x1">A double.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1,x2[i]).</returns>
		/// <seealso cref="Pow(double,Complex)"/>
		public static ComplexMatrix Pow(double x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1, x2.Elements[i]);
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="ComplexMatrix"/> and a <see cref="Complex"/> number.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Complex number.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1[i],x2).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		/// <seealso cref="ComplexMatrix.Pow(Complex)"/>
		public static ComplexMatrix Pow(ComplexMatrix x1, Complex x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Complex"/> number and a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x1">A Complex number.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1,x2[i]).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		public static ComplexMatrix Pow(Complex x1, ComplexMatrix x2)
		{
			if (x2.IsEmpty())
				return new ComplexMatrix();

			int length = x2.Length;
			int[] newDimensions = x2.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1, x2[i]);
			}

			return new ComplexMatrix(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="ComplexMatrix"/> number and a <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x1">A ComplexMatrix.</param>
		/// <param name="x2">A Matrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1[i],x2[i]).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		/// <seealso cref="ComplexMatrix.Pow(Matrix)"/>
		public static ComplexMatrix Pow(ComplexMatrix x1, Matrix x2)
		{
			return x1.Clone().Pow(x2);
		}

		/// <summary>
		/// Returns the pointwise power operation on a <see cref="Matrix"/> number and a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x1">A Matrix.</param>
		/// <param name="x2">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Pow(x1[i],x2[i]).</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		public static ComplexMatrix Pow(Matrix x1, ComplexMatrix x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return new ComplexMatrix();

			if (!Utilities.ArrayEquals(x1.Dimensions, x2.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_9"));

			int length = x2.Length;
			int[] newDimensions = x1.Dimensions;
			IList<Complex> newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = Compute.Pow(x1[i], x2.Elements[i]);
			}
			return new ComplexMatrix(newElements, newDimensions);
		}

		#endregion //Pow

		#region Random

		/// <summary>
		/// Returns a double drawn from the uniform distribution between 0 and 1.
		/// </summary>
		/// <returns>A random double.</returns>
		public static double Random()
		{
			return Compute.Rnd.NextDouble();
		}

		/// <summary>
		/// Returns a double drawn from the uniform distribution between the specified <paramref name="lowerBound"/> and 
		/// the specified <paramref name="upperBound"/>.
		/// </summary>
		/// <param name="lowerBound">A lower bound on the return value.</param>
		/// <param name="upperBound">An upper bound on the return value.</param>
		/// <returns>A random double.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when lowerBound is greater than upperBound.
		/// </exception>
		public static double Random(double lowerBound, double upperBound)
		{
			if (lowerBound > upperBound)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_24"));

			
			return RandomHelper(Compute.Rnd.NextDouble(), lowerBound, upperBound);
		}

		/// <summary>
		/// Returns a <see cref="Complex"/> number drawn from the uniform distribution on the circle given by the
		/// specified <paramref name="origin"/> and <paramref name="radius"/>. 
		/// </summary>
		/// <param name="origin">The origin of a circle on the Complex plane.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <returns>A random Complex number.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if the radius is less than zero.
		/// </exception>
		public static Complex Random(Complex origin, double radius)
		{
			if (radius < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_25"));

			
			return RandomHelper(Compute.Rnd.NextDouble(), Compute.Rnd.NextDouble(), origin, radius);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with <paramref name="N"/> numbers drawn from the uniform distrubition 
		/// between 0 and 1.
		/// </summary>
		/// <param name="N">The length of the Vector.</param>
		/// <returns>A Vector with N random numbers.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than zero.
		/// </exception>
		public static Vector Random(int N)
		{
			if (N == 0)
				return Vector.Empty;

			Vector x = new Vector(N);
			for(int i = 0; i < N; i++)
			{
				x.Elements[i] = Compute.Random();
			}

			return x;
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with <paramref name="N"/> numbers drawn from the uniform distrubition 
		/// between the <paramref name="lowerBound"/> and the <paramref name="upperBound"/>.
		/// </summary>
		/// <param name="N">The length of the Vector.</param>
		/// <param name="lowerBound">A lower bound on the elements of the return value.</param>
		/// <param name="upperBound">An upper bound on the elements of the return value.</param>
		/// <returns>A Vector with N random numbers.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than zero.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when lowerBound is greater than upperBound.
		/// </exception>
		public static Vector Random(int N, double lowerBound, double upperBound)
		{
			if (N == 0)
				return Vector.Empty;

			Vector x = new Vector(N);
			for (int i = 0; i < N; i++)
			{
				x.Elements[i] = Compute.Random(lowerBound,upperBound);
			}

			return x;
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with <paramref name="N"/> <see cref="Complex"/> numbers drawn from 
		/// the uniform distrubition on the circle specified by <paramref name="origin"/> and <paramref name="radius"/> 
		/// on the Complex plane.
		/// </summary>
		/// <param name="N">The length of the Vector.</param>
		/// <param name="origin">The origin of the specified circle.</param>
		/// <param name="radius">The radius of the specified circle.</param>
		/// <returns>A ComplexVector with N random numbers.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than zero.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if the radius is less than zero.
		/// </exception>
		public static ComplexVector Random(int N, Complex origin, double radius)
		{
			if (N == 0)
				return Vector.Empty;

			ComplexVector x = new ComplexVector(N);
			for (int i = 0; i < N; i++)
			{
				x.Elements[i] = Compute.Random(origin, radius);
			}

			return x;
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> of values drawn from the uniform distribution between 0 and 1.
		/// </summary>
		/// <param name="size">A size <see cref="Vector"/>.</param>
		/// <returns>A Matrix of random values.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs for invalid size Vectors.
		/// </exception>
		public static Matrix Random(Vector size)
		{
			int[] dimensions;
			Exception exception;
			if (!Utilities.TrySizeToDimensions(size, out dimensions, out exception))
				throw exception;

			Matrix x = Compute.Zeros(dimensions);
			for (int i = 0, length = x.Length; i < length; i++)
			{
				x.Elements[i] = Compute.Random();
			}

			return x;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> of values drawn from the uniform distribution between 
		/// <paramref name="lowerBound"/> and <paramref name="upperBound"/>.
		/// </summary>
		/// <param name="size">A size <see cref="Vector"/>.</param>
		/// <param name="lowerBound">A lower bound on the elements of the random Matrix.</param>
		/// <param name="upperBound">An upper bound on the elements of the random Matrix.</param>
		/// <returns>A Matrix of random values.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs for invalid size Vectors.
		/// </exception>
		public static Matrix Random(Vector size, double lowerBound, double upperBound)
		{
			int[] dimensions;
			Exception exception;
			if (!Utilities.TrySizeToDimensions(size, out dimensions, out exception))
				throw exception;

			Matrix x = Compute.Zeros(dimensions);
			for (int i = 0, length = x.Length; i < length; i++)
			{
				x.Elements[i] = Compute.Random(lowerBound, upperBound);
			}

			return x;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with <see cref="Complex"/> numbers drawn from the uniform distrubition 
		/// on the circle specified by <paramref name="origin"/> and <paramref name="radius"/> on the Complex plane.
		/// </summary>
		/// <param name="size">A size <see cref="Vector"/>.</param>
		/// <param name="origin">The origin of the specified circle.</param>
		/// <param name="radius">The radius of the specified circle.</param>
		/// <returns>A Matrix of random values.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs for invalid size Vectors.
		/// </exception>
		public static ComplexMatrix Random(Vector size, Complex origin, double radius)
		{
			int[] dimensions;
			Exception exception;
			if (!Utilities.TrySizeToDimensions(size, out dimensions, out exception))
				throw exception;

			ComplexMatrix x = new ComplexMatrix(new Complex[(int)Compute.Product(size)], dimensions);
			for (int i = 0, length = x.Length; i < length; i++)
			{
				x.Elements[i] = Compute.Random(origin, radius);
			}

			return x;
		}

		#endregion //Random

		#region Real

		/// <summary>
		/// Returns the real part of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The real part of x.</returns>
		public static double Real(double x)
		{
			return x;
		}

		/// <summary>
		/// Returns the real part of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>The real part of x.</returns>
		public static double Real(Complex x)
		{
			return x.Re;
		}

		/// <summary>
		/// Returns the real part of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>A Vector y, where y[i] = Real(x[i]).</returns>
		public static Vector Real(Vector x)
		{
			return x.Clone();
		}

		/// <summary>
		/// Returns the real part of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexVector"/>.</param>
		/// <returns>A ComplexVector y, where y[i] = Real(x[i]).</returns>
		public static Vector Real(ComplexVector x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x.Elements[i].Re;
			}

			return new Vector(newElements, x.Dimensions); 
		}

		/// <summary>
		/// Returns the real part of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>A Matrix y, where y[i] = Real(x[i]).</returns>
		public static Matrix Real(Matrix x)
		{
			return x.Clone();
		}

		/// <summary>
		/// Returns the real part of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexMatrix"/>.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Real(x[i]).</returns>
		public static Matrix Real(ComplexMatrix x)
		{
			int length = x.Length;
			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				newElements[i] = x.Elements[i].Re;
			}

			return new Matrix(newElements, x.Dimensions);
		}

		#endregion //Real

		#region Round

		/// <summary>
		/// Returns <paramref name="x"/> rounded to the nearest integer.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>Floor(x) if the decimal portion of x is less than 1/2; Ceiling(x) otherwise.</returns>
		/// <seealso cref="Floor(double)"/>
		/// <seealso cref="Ceiling(double)"/>
		public static double Round(double x)
		{
			return System.Math.Round(x

					, MidpointRounding.AwayFromZero 

				);
		}

		/// <summary>
		/// Returns a <see cref="Complex"/> number composed of the rounded values of the real and imaginary 
		/// parts of <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>A Complex number of the form: Round(Re{x}) + i*Round(Im{x}).</returns>
		/// <seealso cref="Floor(Complex)"/>
		/// <seealso cref="Ceiling(Complex)"/>
		public static Complex Round(Complex x)
		{
			return new Complex(Compute.Round(x.Re), Compute.Round(x.Im));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with each element of <paramref name="x"/> rounded to the nearest integer.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Round(x[i]).</returns>
		/// <see cref="Round(double)"/>
		/// <seealso cref="Vector.Round()"/>
		public static Vector Round(Vector x)
		{
			return x.Clone().Round();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> composed of the rounded values of the real and imaginary 
		/// parts of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Round(x[i]).</returns>
		/// <see cref="Round(Complex)"/>
		/// <seealso cref="ComplexVector.Round()"/>
		public static ComplexVector Round(ComplexVector x)
		{
			return x.Clone().Round();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with each element of <paramref name="x"/> rounded to the nearest integer.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Round(x[i]).</returns>
		/// <see cref="Round(double)"/>
		/// <seealso cref="Matrix.Round()"/>
		public static Matrix Round(Matrix x)
		{
			return x.Clone().Round();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> composed of the rounded values of the real and imaginary 
		/// parts of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Round(x[i]).</returns>
		/// <see cref="Round(Complex)"/>
		/// <seealso cref="ComplexMatrix.Round()"/>
		public static ComplexMatrix Round(ComplexMatrix x)
		{
			return x.Clone().Round();
		}

		#endregion //Round

		#region Sign

		/// <summary>
		/// Returns the sign of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>
		///  1 if x is greater than 0.
		///  0 if x equals 0.
		/// -1 if x is less than 0.
		/// </returns>
		public static double Sign(double x)
		{
			return System.Math.Sign(x);
		}

		/// <summary>
		/// Returns the point on the <see cref="Complex"/> unit circle nearest to x.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>
		/// 0 if x is equal to 0.
		/// Exp(i*Arg{x}) otherwise.
		/// </returns>
		public static Complex Sign(Complex x)
		{
			Complex i = new Complex(0,1);
			if(x == 0)
				return 0;

			return Compute.Exp(i * x.Phase);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the sign of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Sign(x[i]).</returns>
		/// <seealso cref="Sign(double)"/>
		/// <seealso cref="Vector.Sign()"/>
		public static Vector Sign(Vector x)
		{
			return x.Clone().Sign();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the sign of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Sign(x[i]).</returns>
		/// <seealso cref="Sign(Complex)"/>
		/// <seealso cref="ComplexVector.Sign()"/>
		public static ComplexVector Sign(ComplexVector x)
		{
			return x.Clone().Sign();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the sign of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Sign(x[i]).</returns>
		/// <seealso cref="Sign(double)"/>
		/// <seealso cref="Matrix.Sign()"/>
		public static Matrix Sign(Matrix x)
		{
			return x.Clone().Sign();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the sign of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Sign(x[i]).</returns>
		/// <seealso cref="Sign(Complex)"/>
		/// <seealso cref="ComplexMatrix.Sign()"/>
		public static ComplexMatrix Sign(ComplexMatrix x)
		{
			return x.Clone().Sign();
		}

		#endregion //Sign

		#region Sin

		/// <summary>
		/// Returns the sine of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The sine of x.</returns>
		/// <returns></returns>
		/// <seealso cref="Cos(double)"/>
		/// <seealso cref="Tan(double)"/>
		/// <seealso cref="Asin(double)"/>
		/// <seealso cref="Acos(double)"/>
		/// <seealso cref="Atan(double)"/>
		/// <seealso cref="Sinh(double)"/>
		/// <seealso cref="Cosh(double)"/>
		/// <seealso cref="Tanh(double)"/>
		public static double Sin(double x)
		{
			return System.Math.Sin(x);
		}

		/// <summary>
		/// Returns the sine of a <see cref="Complex"/> number <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The sine of x.</returns>
		/// <seealso cref="Cos(Complex)"/>
		/// <seealso cref="Tan(Complex)"/>
		/// <seealso cref="Asin(Complex)"/>
		/// <seealso cref="Acos(Complex)"/>
		/// <seealso cref="Atan(Complex)"/>
		/// <seealso cref="Sinh(Complex)"/>
		/// <seealso cref="Cosh(Complex)"/>
		/// <seealso cref="Tanh(Complex)"/>
		public static Complex Sin(Complex x)
		{
			Complex i = Constant.I;
			return (Compute.Exp(i * x) - Compute.Exp(-1 * i * x)) / (2 * i);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the sine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Sin(x[i]).</returns>
		/// <seealso cref="Sin(double)"/>
		/// <seealso cref="Vector.Sin()"/>
		public static Vector Sin(Vector x)
		{
			return x.Clone().Sin();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the sine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Sin(x[i]).</returns>
		/// <seealso cref="Sin(Complex)"/>
		/// <seealso cref="ComplexVector.Sin()"/>
		public static ComplexVector Sin(ComplexVector x)
		{
			return x.Clone().Sin();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the sine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Sin(x[i]).</returns>
		/// <seealso cref="Sin(double)"/>
		/// <seealso cref="Matrix.Sin()"/>
		public static Matrix Sin(Matrix x)
		{
			return x.Clone().Sin();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the sine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Sin(x[i]).</returns>
		/// <seealso cref="Sin(Complex)"/>
		/// <seealso cref="ComplexMatrix.Sin()"/>
		public static ComplexMatrix Sin(ComplexMatrix x)
		{
			return x.Clone().Sin();
		}

		#endregion //Sin

		#region Sinh

		/// <summary>
		/// Returns the hyperbolic sine of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The hyperbolic sine of x.</returns>
		/// <seealso cref="Sin(double)"/>
		public static double Sinh(double x)
		{
			return System.Math.Sinh(x);
		}

		/// <summary>
		/// Returns the hyperbolic sine of a <see cref="Complex"/> number <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The hyperbolic sine of x.</returns>
		/// <seealso cref="Sin(Complex)"/>
		public static Complex Sinh(Complex x)
		{
			return 0.5 * (Compute.Exp(x) - Compute.Exp(-1 * x));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the hyperbolic sine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Sinh(x[i]).</returns>
		/// <seealso cref="Sinh(double)"/>
		/// <seealso cref="Vector.Sinh()"/>
		public static Vector Sinh(Vector x)
		{
			return x.Clone().Sinh();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the hyperbolic sine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Sinh(x[i]).</returns>
		/// <seealso cref="Sinh(Complex)"/>
		/// <seealso cref="ComplexVector.Sinh()"/>
		public static ComplexVector Sinh(ComplexVector x)
		{
			return x.Clone().Sinh();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the hyperbolic sine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Sinh(x[i]).</returns>
		/// <seealso cref="Sinh(double)"/>
		/// <seealso cref="Matrix.Sinh()"/>
		public static Matrix Sinh(Matrix x)
		{
			return x.Clone().Sinh();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the hyperbolic sine of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Sinh(x[i]).</returns>
		/// <seealso cref="Sinh(Complex)"/>
		/// <seealso cref="ComplexMatrix.Sinh()"/>
		public static ComplexMatrix Sinh(ComplexMatrix x)
		{
			return x.Clone().Sinh();
		}

		#endregion //Sinh

		#region Size

		/// <summary>
		/// Returns the size of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>A <see cref="Vector"/> containing the dimensions of x.</returns>
		public static Vector Size(double x)
		{
			return new Vector(1, 1);
		}

		/// <summary>
		/// Returns the size of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Complex"/> number.</param>
		/// <returns>A <see cref="Vector"/> containing the dimensions of x.</returns>
		public static Vector Size(Complex x)
		{
			return new Vector(1, 1);
		}

		/// <summary>
		/// Returns the size of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>A Vector containing the dimensions of x.</returns>
		public static Vector Size(Vector x)
		{
			return (Vector)x.Dimensions;
		}

		/// <summary>
		/// Returns the size of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexVector"/>.</param>
		/// <returns>A <see cref="Vector"/> containing the dimensions of x.</returns>
		public static Vector Size(ComplexVector x)
		{
			return (Vector)x.Dimensions;
		}

		/// <summary>
		/// Returns the size of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="BooleanVector"/>.</param>
		/// <returns>A <see cref="Vector"/> containing the dimensions of x.</returns>
		public static Vector Size(BooleanVector x)
		{
			return (Vector)x.Dimensions;
		}

		/// <summary>
		/// Returns the size of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>A Vector containing the dimensions of x.</returns>
		public static Vector Size(Matrix x)
		{
			return (Vector)x.Dimensions;
		}

		/// <summary>
		/// Returns the size of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexMatrix"/>.</param>
		/// <returns>A <see cref="Vector"/> containing the dimensions of x.</returns>
		public static Vector Size(ComplexMatrix x)
		{
			return (Vector)x.Dimensions;
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Returns the size of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="BooleanMatrix"/>.</param>
		/// <returns>A <see cref="Vector"/> containing the dimensions of x.</returns>
		public static Vector Size(BooleanMatrix x)
		{
			return (Vector)x.Dimensions;
		}

		#endregion //Size

		#region Sqrt

		/// <summary>
		/// Returns the square-root of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The square-root of x.</returns>
		/// <seealso cref="Pow(double,double)"/>
		public static double Sqrt(double x)
		{
			return System.Math.Sqrt(x);
		}

		/// <summary>
		/// Returns the square-root of a <see cref="Complex"/> number <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The square-root of x.</returns>
		/// <seealso cref="Pow(Complex,Complex)"/>
		public static Complex Sqrt(Complex x)
		{
			return Compute.Pow(x, 0.5);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the square-root of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Sqrt(x[i]).</returns>
		/// <seealso cref="Sqrt(double)"/>
		/// <seealso cref="Vector.Sqrt()"/>
		public static Vector Sqrt(Vector x)
		{
			return x.Clone().Sqrt();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the square-root of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Sqrt(x[i]).</returns>
		/// <seealso cref="Sqrt(Complex)"/>
		/// <seealso cref="ComplexVector.Sqrt()"/>
		public static ComplexVector Sqrt(ComplexVector x)
		{
			return x.Clone().Sqrt();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the square-root of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Sqrt(x[i]).</returns>
		/// <seealso cref="Sqrt(double)"/>
		/// <seealso cref="Matrix.Sqrt()"/>
		public static Matrix Sqrt(Matrix x)
		{
			return x.Clone().Sqrt();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the square-root of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Sqrt(x[i]).</returns>
		/// <seealso cref="Sqrt(Complex)"/>
		/// <seealso cref="ComplexMatrix.Sqrt()"/>
		public static ComplexMatrix Sqrt(ComplexMatrix x)
		{
			return x.Clone().Sqrt();
		}

		#endregion //Sqrt

		#region Tan

		/// <summary>
		/// Returns the tangent of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The tangent of x.</returns>
		/// <seealso cref="Sin(double)"/>
		/// <seealso cref="Cos(double)"/>
		/// <seealso cref="Atan(double)"/>
		/// <seealso cref="Asin(double)"/>
		/// <seealso cref="Acos(double)"/>
		/// <seealso cref="Tanh(double)"/>
		/// <seealso cref="Sinh(double)"/>
		/// <seealso cref="Cosh(double)"/>
		public static double Tan(double x)
		{
			return System.Math.Tan(x);
		}

		/// <summary>
		/// Returns the tangent of a <see cref="Complex"/> number <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The tangent of x.</returns>
		/// <seealso cref="Sin(Complex)"/>
		/// <seealso cref="Cos(Complex)"/>
		/// <seealso cref="Atan(Complex)"/>
		/// <seealso cref="Asin(Complex)"/>
		/// <seealso cref="Acos(Complex)"/>
		/// <seealso cref="Tanh(Complex)"/>
		/// <seealso cref="Sinh(Complex)"/>
		/// <seealso cref="Cosh(Complex)"/>
		public static Complex Tan(Complex x)
		{
			return Compute.Sin(x) / Compute.Cos(x);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the tangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Tan(x[i]).</returns>
		/// <seealso cref="Tan(double)"/>
		/// <seealso cref="Vector.Tan()"/>
		public static Vector Tan(Vector x)
		{
			return x.Clone().Tan();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the tangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Tan(x[i]).</returns>
		/// <seealso cref="Tan(Complex)"/>
		/// <seealso cref="ComplexVector.Tan()"/>
		public static ComplexVector Tan(ComplexVector x)
		{
			return x.Clone().Tan();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the tangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Tan(x[i]).</returns>
		/// <seealso cref="Tan(double)"/>
		/// <seealso cref="Matrix.Tan()"/>
		public static Matrix Tan(Matrix x)
		{
			return x.Clone().Tan();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the tangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Tan(x[i]).</returns>
		/// <seealso cref="Tan(Complex)"/>
		/// <seealso cref="ComplexMatrix.Tan()"/>
		public static ComplexMatrix Tan(ComplexMatrix x)
		{
			return x.Clone().Tan();
		}

		#endregion //Tan

		#region Tanh

		/// <summary>
		/// Returns the hyperbolic tangent of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <returns>The hyperbolic tangent of x.</returns>
		/// <seealso cref="Tan(double)"/>
		public static double Tanh(double x)
		{
			return System.Math.Tanh(x);
		}

		/// <summary>
		/// Returns the hyperbolic tangent of a <see cref="Complex"/> number <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Complex number.</param>
		/// <returns>The hyperbolic tangent of x.</returns>
		/// <seealso cref="Tan(Complex)"/>
		public static Complex Tanh(Complex x)
		{
			return Compute.Sinh(x) / Compute.Cosh(x);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the hyperbolic tangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Tanh(x[i]).</returns>
		/// <seealso cref="Tanh(double)"/>
		/// <seealso cref="Vector.Tanh()"/>
		public static Vector Tanh(Vector x)
		{
			return x.Clone().Tanh();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the hyperbolic tangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = Tanh(x[i]).</returns>
		/// <seealso cref="Tanh(Complex)"/>
		/// <seealso cref="ComplexVector.Tanh()"/>
		public static ComplexVector Tanh(ComplexVector x)
		{
			return x.Clone().Tanh();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the hyperbolic tangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Tanh(x[i]).</returns>
		/// <seealso cref="Tanh(double)"/>
		/// <seealso cref="Matrix.Tanh()"/>
		public static Matrix Tanh(Matrix x)
		{
			return x.Clone().Tanh();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the hyperbolic tangent of each element of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix y, where y[i] = Tanh(x[i]).</returns>
		/// <seealso cref="Tanh(Complex)"/>
		/// <seealso cref="ComplexMatrix.Tanh()"/>
		public static ComplexMatrix Tanh(ComplexMatrix x)
		{
			return x.Clone().Tanh();
		}

		#endregion //Tanh

		#endregion //Basic Math Functions

		#region Discrete Math Functions

		#region Factor

		/// <summary>
		/// Returns a <see cref="Vector"/> containing the prime factors of <paramref name="n"/>.
		/// </summary>
		/// <param name="n">A double.</param>
		/// <returns>The prime factors of n.</returns>
		/// <remarks>
		/// If n is not an integer, NaN is returned as a unitary Vector. If n is negative, the first element of the result
		/// is -1. 
		/// 
		/// For integer-valued n: 
		/// 
		///		n == Product(Factors(n));
		/// </remarks>
		public static Vector Factor(double n)
		{
			if (!Compute.IsInt(n))
				return Constant.NaN;

			if (n > -2 &&  n < 4)
				return n;

			if (n == -2 || n == -3)
				return new double[] { -1, -n };

			List<double> result = new List<double>();
			if (n < 0)
			{
				result.Add(-1);
				n = -n;
			}

			double curr;
			Vector primes = Compute.Append(Compute.Primes(n / 2), n);
			int length = primes.Length;
			for (int i = 0; i < length; i++)
			{ 
				curr = primes.Elements[i];
				if (n % curr == 0)
				{
					result.Add(curr);
					n = n / curr;
					i--;

					if (n == 1)
						break;
				}
				
			}

			return new Vector(result);
		}

		#endregion //Factor

		#region Factorial

		/// <summary>
		/// Returns <paramref name="n"/> factorial.
		/// </summary>
		/// <param name="n">A double.</param>
		/// <returns>n factorial.</returns>
		/// <remarks>
		/// If n is not a natural number, NaN is returned.
		/// </remarks>
		public static double Factorial(double n)
		{
			if (!Compute.IsNatural(n))
				return Constant.NaN;

			if (n < 2)
				return 1;

			double result = 2;
			for (int i = 3, length = (int)n + 1; i < length; i++)
			{
				result *= i;
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> y, where y[i] = Factorial(x[i]).
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = Factorial(x[i]).</returns>
		public static Vector Factorial(Vector x)
		{
			if (x.IsEmpty())
				return Vector.Empty;

			int length = x.Length;
			Vector result = x.Clone();
			for (int i = 0; i < length; i++)
			{
				result.Elements[i] = Compute.Factorial(result.Elements[i]);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> y, where y[i] = Factorial(x[i]).
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix y, where y[i] = Factorial(x[i]).</returns>
		public static Matrix Factorial(Matrix x)
		{
			if (x.IsEmpty())
				return Matrix.Empty;

			int length = x.Length;
			Matrix result = x.Clone();
			for (int i = 0; i < length; i++)
			{
				result.Elements[i] = Compute.Factorial(result.Elements[i]);
			}

			return result;
		}

		#endregion //Factorial

		#region Gcd

		/// <summary>
		/// Returns the greatest common divisor of <paramref name="m"/> and <paramref name="n"/> using Euclid's algorithm.
		/// </summary>
		/// <param name="m">The first double.</param>
		/// <param name="n">The second double.</param>
		/// <returns>The greatest common divisor of m and n.</returns>
		public static double Gcd(double m, double n)
		{
			if (!Compute.IsInt(m) || !Compute.IsInt(n))
				return Constant.NaN;

			if (m == 0 && n == 0)
				return 0;

			if (m == 0)
				return n;

			if (n == 0)
				return m;

			m = Compute.Abs(m);
			n = Compute.Abs(n);

			double temp;
			if (m < n)
			{
				temp = m;
				m = n;
				n = temp;
			}

			bool done = false;
			while (!done)
			{
				temp = n;
				n = m % n;
				m = temp;

				if(n == 0)
					done = true;
			}

			return m;
		}

		#endregion //Gcd

		#region IsInt

		/// <summary>
		/// Returns True if <paramref name="n"/> is integer valued; False otherwise.
		/// </summary>
		/// <param name="n">A double.</param>
		/// <returns>True if x is integer valued; False otherwise.</returns>
		public static bool IsInt(double n)
		{
			return n == Compute.Round(n) && !Compute.IsNaN(n) && !Compute.IsInf(n);
		}

		/// <summary>
		/// Returns a <see cref="BooleanVector"/> y, where y[i] = True if x[i] is integer valued; False otherwise.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>a BooleanVector y, where y[i] = True if x[i] is integer valued; False otherwise.</returns>
		public static BooleanVector IsInt(Vector x)
		{
			if (x.IsEmpty())
				return BooleanVector.Empty;

			int length = x.Length;
			BooleanVector result = new BooleanVector(new bool[length], (int[])x.Dimensions.Clone());
			for (int i = 0; i < length; i++)
			{
				if (Compute.IsInt(x.Elements[i]))
					result.Elements[i] = true;
			}	

			return result;
		}

		/// <summary>
		/// Returns a <see cref="BooleanMatrix"/> y, where y[i] = True if x[i] is integer valued; False otherwise.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>a BooleanMatrix y, where y[i] = True if x[i] is integer valued; False otherwise.</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector IsInt(Matrix x)
		public static BooleanMatrix IsInt(Matrix x)
		{
			if (x.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}

			int length = x.Length;

			// MD 4/19/11 - TFS72396
			//BooleanVector result = new BooleanVector(length);
			BooleanMatrix result = new BooleanMatrix(x.Dimensions);

			for (int i = 0; i < length; i++)
			{
				if (Compute.IsInt(x.Elements[i]))
					result.Elements[i] = true;
			}

			return result;
		}

		#endregion //IsInt

		#region IsNatural

		/// <summary>
		/// Returns True if <paramref name="n"/> is a natrual number; False otherwise.
		/// </summary>
		/// <param name="n">A double.</param>
		/// <returns>True if x is a natural number; False otherwise.</returns>
		public static bool IsNatural(double n)
		{
			return Compute.IsInt(n) && n >= 0;
		}

		/// <summary>
		/// Returns a <see cref="BooleanVector"/> y, where y[i] = True if x[i] is a natural number; False otherwise.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>a BooleanVector y, where y[i] = True if x[i] is a natural number; False otherwise.</returns>
		public static BooleanVector IsNatural(Vector x)
		{
			if (x.IsEmpty())
				return BooleanVector.Empty;

			int length = x.Length;
			BooleanVector result = new BooleanVector(new bool[length], (int[])x.Dimensions.Clone());
			for (int i = 0; i < length; i++)
			{
				if (Compute.IsNatural(x.Elements[i]))
					result.Elements[i] = true;
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="BooleanMatrix"/> y, where y[i] = True if x[i] is a natural number; False otherwise.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>a BooleanMatrix y, where y[i] = True iff x[i] is a natural number; False otherwise.</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector IsNatural(Matrix x)
		public static BooleanMatrix IsNatural(Matrix x)
		{
			if (x.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}

			int length = x.Length;

			// MD 4/19/11 - TFS72396
			//BooleanVector result = new BooleanVector(length);
			BooleanMatrix result = new BooleanMatrix(x.Dimensions);

			for (int i = 0; i < length; i++)
			{
				if (Compute.IsNatural(x.Elements[i]))
					result.Elements[i] = true;
			}

			return result;
		}

		#endregion //IsInt

		#region IsPrime

		/// <summary>
		/// Returns True if <paramref name="n"/> is a prime number; False otherwise.
		/// </summary>
		/// <param name="n">A double.</param>
		/// <returns>True if x is a prime number; False otherwise.</returns>
		public static bool IsPrime(double n)
		{
			if (!Compute.IsNatural(n) || n < 2)
				return false;

			if (n == 2 || n == 3)
				return true;

			// MD 4/19/11
			// Found while fixing TFS72396
			// This is slow.
			#region Old Code

			//double curr;
			//int currInt;
			//Vector numbers = Compute.Index(2, (int)(n/2));
			//int length = numbers.Length;
			//BooleanVector check = new BooleanVector(true, length);
			//for (int i = 0; i < length; i++)
			//{
			//    if (check.Elements[i])
			//    {
			//        curr = numbers.Elements[i];
			//        if (n % curr == 0)
			//            return false;
			//
			//        currInt = (int)curr;
			//        for (int j = i; j < length; j += currInt)
			//        {
			//            check.Elements[j] = false;
			//        }
			//    }
			//}
			//
			//return true; 

			#endregion  // Old Code

			
			
			
			
			// Check for even numbers.
			if ((n % 2) == 0)
				return false;

			// Test all odds from 3 to the square root of the number.
			// If it has any factors, at least one of them has to be less than the square root.
			int maxValue = System.Math.Max(3, (int)System.Math.Ceiling(System.Math.Sqrt(n)));
			for (int testValue = 3; testValue <= maxValue; testValue += 2)
			{
				if ((n % testValue) == 0)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns a <see cref="BooleanVector"/> y, where y[i] = True iff x[i] is a prime number.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>a BooleanVector y, where y[i] = True if x[i] is a prime number; False otherwise.</returns>
		public static BooleanVector IsPrime(Vector x)
		{
			if (x.IsEmpty())
				return BooleanVector.Empty;

			int length = x.Length;
			BooleanVector result = new BooleanVector(new bool[length], (int[])x.Dimensions.Clone());
			for (int i = 0; i < length; i++)
			{
				if (Compute.IsPrime(x.Elements[i]))
					result.Elements[i] = true;
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="BooleanMatrix"/> y, where y[i] = True iff x[i] is integer valued.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <returns>a BooleanMatrix y, where y[i] = True if x[i] is integer valued</returns>
		// MD 4/19/11 - TFS72396
		// Matrix comparisons need to return BooleanMatrix values in case they are multidimensional.
		//public static BooleanVector IsPrime(Matrix x)
		public static BooleanMatrix IsPrime(Matrix x)
		{
			if (x.IsEmpty())
			{
				// MD 4/19/11 - TFS72396
				//return BooleanVector.Empty;
				return BooleanMatrix.Empty;
			}

			int length = x.Length;

			// MD 4/19/11 - TFS72396
			//BooleanVector result = new BooleanVector(length);
			BooleanMatrix result = new BooleanMatrix(x.Dimensions);

			for (int i = 0; i < length; i++)
			{
				if (Compute.IsPrime(x.Elements[i]))
					result.Elements[i] = true;
			}

			return result;
		}

		#endregion //IsPrime

		#region Lcm

		/// <summary>
		/// Returns the least common multiple of <paramref name="m"/> and <paramref name="n"/>.
		/// </summary>
		/// <param name="m">The first double.</param>
		/// <param name="n">The second double.</param>
		/// <returns>The least common multiple of m and n.</returns>
		public static double Lcm(double m, double n)
		{
			if (!Compute.IsInt(m) || !Compute.IsInt(n))
				return Constant.NaN;

			if (m == 0 || n == 0)
				return 0;

			return Compute.Abs(m * n) / Compute.Gcd(m, n);
		}

		#endregion //Lcm

		#region NChooseK

		/// <summary>
		/// Returns <paramref name="n"/> choose <paramref name="k"/>.
		/// </summary>
		/// <param name="n">The first double.</param>
		/// <param name="k">The second double. </param>
		/// <returns>n choose k.</returns>
		/// <remarks>
		/// If either n or k is not a natural number, or if k is greater than n, NaN is returned.
		/// </remarks>
		public static double NChooseK(double n, double k)
		{
			if (!Compute.IsNatural(n) || !Compute.IsNatural(k) || k > n)
				return Constant.NaN;

			return Compute.Factorial(n) / (Compute.Factorial(k) * Compute.Factorial(n - k));
		}

		#endregion //NChooseK

		#region Primes

		/// <summary>
		/// Returns a <see cref="Vector"/> containing the primes before <paramref name="n"/>.
		/// </summary>
		/// <param name="n">A double.</param>
		/// <returns>The primes before n.</returns>
		public static Vector Primes(double n)
		{
			if (n < 2)
				return Vector.Empty;

			if (n < 3)
				return 2;

			double curr;
			int currInt;
			Vector numbers = Compute.Index(2, (int)(n - 1));
			int length = numbers.Length;
			BooleanVector check = new BooleanVector(true, length);
			List<double> result = new List<double>();

			for (int i = 0; i < length; i++)
			{
				if (check.Elements[i])
				{
					curr = numbers.Elements[i];
					if (Compute.IsPrime(curr))
						result.Add(curr);

					currInt = (int)curr;
					for (int j = i; j < length; j += currInt)
					{
						check.Elements[j] = false;
					}
				}
			}

			return new Vector(result);
		}

		#endregion //Primes

		#endregion //Discrete Math Functions

		#region Vector and Matrix Functions

		#region Append

		/// <summary>
		/// Appends a series of <see cref="Vector"/> instances together.
		/// </summary>
		/// <param name="x">An array of Vectors with the same orientation.</param>
		/// <returns>
		/// A Vector which consists of the elements of each Vector in <paramref name="x"/> stacked in sequence.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Occurs when the orientations of the Vectors in x do not align.
		/// </exception>
		public static Vector Append(params Vector[] x)
		{
			bool isRow = false;
			bool isColumn = false;
			Vector xi;
			int length = 0;
			int currLength;
			int xLength = x.Length;

			for (int i = 0; i < xLength; i++)
			{
				xi = x[i];
				currLength = xi.Length;
				length += currLength;

				if(!xi.IsUnitary() && !xi.IsEmpty())
				{
					if (xi.IsRow())
						isRow = true;
					else
						isColumn = true;
				}
			}

			if (isRow && isColumn)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_26"));

			if (!isRow && !isColumn)
				isRow = true;

			if (length == 0)
				return Vector.Empty;

			int[] newDimensions;
			if (length == 0)
				newDimensions = new int[2];
			else if (isRow)
				newDimensions = new int[] { 1, length };
			else
				newDimensions = new int[] { length, 1 };

			int start = 0;
			double[] newElements = new double[length];
			for (int i = 0; i < xLength; i++)
			{
				x[i].CopyTo(newElements, start);
				start += x[i].Length;
			}

			return new Vector(newElements, newDimensions);
		}

		/// <summary>
		/// Appends a series of <see cref="ComplexVector"/> instances together.
		/// </summary>
		/// <param name="x">An array of ComplexVectors with the same orientation.</param>
		/// <returns>
		/// A ComplexVector which consists of the elements of each ComplexVector in <paramref name="x"/> stacked in sequence.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Occurs when the orientations of the ComplexVectors in x do not align.
		/// </exception>
		public static ComplexVector Append(params ComplexVector[] x)
		{
			bool isRow = false;
			bool isColumn = false;
			ComplexVector xi;
			int length = 0;
			int currLength;
			int xLength = x.Length;

			for (int i = 0; i < xLength; i++)
			{
				xi = x[i];
				currLength = xi.Length;
				length += currLength;

				if (!xi.IsUnitary() && !xi.IsEmpty())
				{
					if (xi.IsRow())
						isRow = true;
					else
						isColumn = true;
				}
			}

			if (isRow && isColumn)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_26"));

			if (!isRow && !isColumn)
				isRow = true;

			if (length == 0)
				return ComplexVector.Empty;

			int[] newDimensions;
			if (length == 0)
				newDimensions = new int[2];
			else if (isRow)
				newDimensions = new int[] { 1, length };
			else
				newDimensions = new int[] { length, 1 };

			int start = 0;
			Complex[] newElements = new Complex[length];
			for (int i = 0; i < xLength; i++)
			{
				x[i].CopyTo(newElements, start);
				start += x[i].Length;
			}

			return new ComplexVector(newElements, newDimensions);
		}

		/// <summary>
		/// Appends a series of <see cref="BooleanVector"/> instances together.
		/// </summary>
		/// <param name="x">An array of BooleanVectors with the same orientation.</param>
		/// <returns>
		/// A BooleanVector which consists of the elements of each BooleanVector in <paramref name="x"/> stacked in sequence.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Occurs when the orientations of the BooleanVectors in x do not align.
		/// </exception>
		public static BooleanVector Append(params BooleanVector[] x)
		{
			bool isRow = false;
			bool isColumn = false;
			BooleanVector xi;
			int length = 0;
			int currLength;
			int xLength = x.Length;

			for (int i = 0; i < xLength; i++)
			{
				xi = x[i];
				currLength = xi.Length;
				length += currLength;

				if (!xi.IsUnitary() && !xi.IsEmpty())
				{
					if (xi.IsRow())
						isRow = true;
					else
						isColumn = true;
				}
			}

			if (isRow && isColumn)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_26"));

			if (!isRow && !isColumn)
				isRow = true;

			if (length == 0)
				return BooleanVector.Empty;

			int[] newDimensions;
			if (length == 0)
				newDimensions = new int[2];
			else if (isRow)
				newDimensions = new int[] { 1, length };
			else
				newDimensions = new int[] { length, 1 };

			int start = 0;
			Boolean[] newElements = new Boolean[length];
			for (int i = 0; i < xLength; i++)
			{
				x[i].CopyTo(newElements, start);
				start += x[i].Length;
			}

			return new BooleanVector(newElements, newDimensions);
		}

		/// <summary>
		/// Appends a series of <see cref="Matrix"/> instances together.
		/// </summary>
		/// <param name="x">An array of Matrices all dimensions except the last one identical.</param>
		/// <returns>A Matrix.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when the Matrices do not align.
		/// </exception>
		public static Matrix Append(params Matrix[] x)
		{
			if (x.Length == 0)
				return Matrix.Empty;

			if (x.Length == 1)
				return x[0].Clone();

			Matrix curr;
			int[] dimensions = null;
			for (int i = 0, length = x.Length; i < length; i++)
			{
				curr = x[i];
				if (!curr.IsEmpty())
				{
					dimensions = curr.Dimensions;
					break;
				}
			}
			if(dimensions == null)
				return Matrix.Empty;

			int totalLength = 0;
			int lastDimension = 0;
			int N = dimensions.Length;
			Vector[] vectorArray = new Vector[x.Length]; 
			for (int i = 0, length = x.Length; i < length; i++)
			{
				curr = x[i];
				if (!curr.IsEmpty())
				{
					if (!Utilities.ArrayEqualsExceptLastElement(dimensions, curr.Dimensions))
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_27"));

					totalLength += curr.Length;
					lastDimension += curr.Dimensions[N-1];
					vectorArray[i] = new Vector(curr.Elements, new int[] { 1, curr.Length });
				}
				else
				{
					vectorArray[i] = Vector.Empty;
				}
			}

			int[] newDimensions = new int[N];
			dimensions.CopyTo(newDimensions, 0);
			newDimensions[N-1] = lastDimension;

			Vector appendedVector = Compute.Append(vectorArray);
			return new Matrix(appendedVector.Elements, newDimensions);
		}

		/// <summary>
		/// Appends a series of <see cref="ComplexMatrix"/> instances together.
		/// </summary>
		/// <param name="x">An array of ComplexMatrices all dimensions except the last one identical.</param>
		/// <returns>A ComplexMatrix.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when the ComplexMatrices do not align.
		/// </exception>
		public static ComplexMatrix Append(params ComplexMatrix[] x)
		{
			if (x.Length == 0)
				return ComplexMatrix.Empty;

			if (x.Length == 1)
				return x[0].Clone();

			ComplexMatrix curr;
			int[] dimensions = null;
			for (int i = 0, length = x.Length; i < length; i++)
			{
				curr = x[i];
				if (!curr.IsEmpty())
				{
					dimensions = curr.Dimensions;
					break;
				}
			}
			if (dimensions == null)
				return ComplexMatrix.Empty;

			int totalLength = 0;
			int lastDimension = 0;
			int N = dimensions.Length;
			ComplexVector[] vectorArray = new ComplexVector[x.Length];
			for (int i = 0, length = x.Length; i < length; i++)
			{
				curr = x[i];
				if (!curr.IsEmpty())
				{
					if (!Utilities.ArrayEqualsExceptLastElement(dimensions, curr.Dimensions))
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_27"));

					totalLength += curr.Length;
					lastDimension += curr.Dimensions[N - 1];
					vectorArray[i] = new ComplexVector(curr.Elements, new int[] { 1, curr.Length });
				}
				else
				{
					vectorArray[i] = ComplexVector.Empty;
				}
			}

			int[] newDimensions = new int[N];
			dimensions.CopyTo(newDimensions, 0);
			newDimensions[N - 1] = lastDimension;

			ComplexVector appendedVector = Compute.Append(vectorArray);
			return new ComplexMatrix(appendedVector.Elements, newDimensions);
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Appends a series of <see cref="BooleanMatrix"/> instances together.
		/// </summary>
		/// <param name="x">An array of BooleanMatrices all dimensions except the last one identical.</param>
		/// <returns>A BooleanMatrix.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when the BooleanMatrix do not align.
		/// </exception>
		public static BooleanMatrix Append(params BooleanMatrix[] x)
		{
			if (x.Length == 0)
				return BooleanMatrix.Empty;

			if (x.Length == 1)
				return x[0].Clone();

			BooleanMatrix curr;
			int[] dimensions = null;
			for (int i = 0, length = x.Length; i < length; i++)
			{
				curr = x[i];
				if (!curr.IsEmpty())
				{
					dimensions = curr.Dimensions;
					break;
				}
			}
			if (dimensions == null)
				return BooleanMatrix.Empty;

			int totalLength = 0;
			int lastDimension = 0;
			int N = dimensions.Length;
			BooleanVector[] vectorArray = new BooleanVector[x.Length];
			for (int i = 0, length = x.Length; i < length; i++)
			{
				curr = x[i];
				if (!curr.IsEmpty())
				{
					if (!Utilities.ArrayEqualsExceptLastElement(dimensions, curr.Dimensions))
						throw new ArgumentException(Compute.GetString("LE_ArgumentException_27"));

					totalLength += curr.Length;
					lastDimension += curr.Dimensions[N - 1];
					vectorArray[i] = new BooleanVector(curr.Elements, new int[] { 1, curr.Length });
				}
				else
				{
					vectorArray[i] = BooleanVector.Empty;
				}
			}

			int[] newDimensions = new int[N];
			dimensions.CopyTo(newDimensions, 0);
			newDimensions[N - 1] = lastDimension;

			BooleanVector appendedVector = Compute.Append(vectorArray);
			return new BooleanMatrix(appendedVector.Elements, newDimensions);
		}

		#endregion //Append

		#region Bin

		/// <summary>
		/// Returns a new <see cref="Vector"/> y, where y[i] = n if x[i] is in the nth bin.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="N">The number of evenly-spaced bins.</param>
		/// <returns>A Vector of bin numbers.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than one.
		/// </exception>
		public static Vector Bin(Vector x, int N)
		{
			return x.Clone().Bin(N);
		}

		/// <summary>
		/// Returns a new <see cref="Vector"/> y, where y[i] = n if x[i] is in the nth bin.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="edges">edges[n] and edges[n+1] are the edges of the nth bin.</param>
		/// <returns>A Vector of bin numbers.</returns>
		/// <remarks>
		/// If x[i] is not in any bin, y[i] = Constant.NaN.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when there are less than two bin <paramref name="edges"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the bin edges are not in increasing order.
		/// </exception>
		/// <seealso cref="Vector.Bin(Vector)"/>
		public static Vector Bin(Vector x, Vector edges)
		{
			return x.Clone().Bin(edges);
		}

		/// <summary>
		/// Returns a new <see cref="Matrix"/> y, where y[i1,..,iN] = n if x[i1,...,iN] is in the nth bin.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The number of evenly-spaced bins.</param>
		/// <returns>A Vector of bin numbers.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than one.
		/// </exception>
		public static Matrix Bin(Matrix x, int N)
		{
			return x.Clone().Bin(N);
		}

		/// <summary>
		/// Returns a new <see cref="Matrix"/> y, where y[i] = n if x[i] is in the nth bin.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="edges">edges[n] and edges[n+1] are the edges of the nth bin.</param>
		/// <returns>A Vector of bin numbers.</returns>
		/// <remarks>
		/// If x[i] is not in any bin, y[i] = Constant.NaN.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when there are less than two bin <paramref name="edges"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the bin edges are not in increasing order.
		/// </exception>
		/// <seealso cref="Vector.Bin(Vector)"/>
		public static Matrix Bin(Matrix x, Vector edges)
		{
			return x.Clone().Bin(edges);
		}

		#endregion //Bin

		#region Cofactor

		/// <summary>
		/// Returns the (i,j) cofactor of <paramref name="x"/>. If i+j is even, the (i,j) cofactor is equal to the 
		/// determinant of the (i,j) minor <see cref="Matrix"/> of x. If i+j is odd, the (i,j) cofactor is the negative 
		/// determinant of the (i,j) minor Matrix of x.
		/// </summary>
		/// <param name="x">A square Matrix.</param>
		/// <param name="i">A row index.</param>
		/// <param name="j">A column index.</param>
		/// <returns>The (i,j) cofactor of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not square.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if <paramref name="i"/> or <paramref name="j"/> are out of bounds.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		/// <seealso cref="Compute.MinorMatrix(Matrix,int,int)"/>
		public static double CofactorMatrix(Matrix x, int i, int j)
		{
			if (!x.IsSquare())
				throw new ArgumentException("Cofactor takes square Matrices as input.");

			double result = Compute.Determinant(Compute.MinorMatrix(x, i, j));
			if (Compute.IsEven(i + j))
				return result;

			return -1 * result;
		}

		/// <summary>
		/// Returns the (i,j) cofactor of <paramref name="x"/>. If i+j is even, the (i,j) cofactor is equal to the 
		/// determinant of the (i,j) minor <see cref="ComplexMatrix"/> of x. If i+j is odd, the (i,j) cofactor is the 
		/// negative determinant of the (i,j) minor ComplexMatrix of x.
		/// </summary>
		/// <param name="x">A square ComplexMatrix.</param>
		/// <param name="i">A row index.</param>
		/// <param name="j">A column index.</param>
		/// <returns>The (i,j) cofactor of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not square.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if <paramref name="i"/> or <paramref name="j"/> are out of bounds.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		/// <seealso cref="Compute.MinorMatrix(Matrix,int,int)"/>
		public static Complex CofactorMatrix(ComplexMatrix x, int i, int j)
		{
			if (!x.IsSquare())
				throw new ArgumentException("Cofactor takes square ComplexMatrices as input.");

			Complex result = Compute.Determinant(Compute.MinorMatrix(x, i, j));
			if (Compute.IsEven(i + j))
				return result;

			return -1 * result;
		}

		#endregion //Cofactor

		#region Convolve

		/// <summary>
		/// Returns the center convolution of two <see cref="Vector"/> instances.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>A Vector y, where y[i] = DotProduct(x1[j],x2[i-j]).</returns>
		public static Vector Convolve(Vector x1, Vector x2)
		{
			return Compute.Convolve(x1, x2, ConvolutionType.Center);
		}

		/// <summary>
		/// Returns the center convolution of a <see cref="Vector"/> and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A new ComplexVector y, where y[i] = DotProduct(x1[j],x2[i-j]).</returns>
		public static ComplexVector Convolve(Vector x1, ComplexVector x2)
		{
			return Compute.Convolve(x1, x2, ConvolutionType.Center);
		}

		/// <summary>
		/// Returns the center convolution of a <see cref="ComplexVector"/> and a <see cref="Vector"/>.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = DotProduct(x1[j],x2[i-j]).</returns>
		public static ComplexVector Convolve(ComplexVector x1, Vector x2)
		{
			return Compute.Convolve(x1, x2, ConvolutionType.Center);
		}

		/// <summary>
		/// Returns the center convolution of two <see cref="ComplexVector"/> instances.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = DotProduct(x1[j],x2[i-j]).</returns>
		public static ComplexVector Convolve(ComplexVector x1, ComplexVector x2)
		{
			return Compute.Convolve(x1, x2, ConvolutionType.Center);
		}

		/// <summary>
		/// Returns the convolution of two <see cref="Vector"/> instances.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <param name="type">
		/// Specified whether to return the full convolution or to return the center points and exclude some of 
		/// the edge effects.
		/// </param>
		/// <returns>A Vector y, where y[i] = DotProduct(x1[j],x2[i-j]).</returns>
		public static Vector Convolve(Vector x1, Vector x2, ConvolutionType type)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return Vector.Empty;

			if (x1.IsUnitary())
				return x1.Elements[0] * x2;

			if (x2.IsUnitary())
				return x1 * x2.Elements[0];

			bool x1IsRow = x1.IsRow();
			bool x2IsRow = x2.IsRow();
			if (x1IsRow != x2IsRow)
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_28"));

			int x1Length = x1.Length;
			int x2Length = x2.Length;

			int length;
			int shift;
			if (type == ConvolutionType.Center)
			{
				length = System.Math.Max(x1Length, x2Length);
				shift = System.Math.Min(x1Length, x2Length) / 2;
			}
			else 
			{
				length = x1Length + x2Length - 1;
				shift = 0;
			}

			int[] newDimensions;
			if (x1IsRow)
				newDimensions = new int[] { 1, length };
			else
				newDimensions = new int[] { length, 1 };

			double sum;
			int x1Start = 0;
			int x1End = 0;
			int x2Start = 0;
			int x1VirtualStart = 1 - x2Length + shift;
			int x2VirtualStart = 1 - x1Length + shift;
			int x1VirtualEnd = shift;

			double[] newElements = new double[length];
			for (int i = 0; i < length; i++)
			{
				x1Start = System.Math.Max(x1VirtualStart, 0);
				x1End = System.Math.Min(x1VirtualEnd, x1Length - 1);
				x2Start = System.Math.Max(x2VirtualStart, 0);

				sum = 0;
				for (int j1 = x1Start, j2 = x2Start + x1End - x1Start; j1 <= x1End; j1++, j2--)
 				{
					sum += x1.Elements[j1] * x2.Elements[j2];
				}

				newElements[i] = sum;
				x1VirtualStart++;
				x2VirtualStart++;
				x1VirtualEnd++;
			}

			return new Vector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns the convolution of a <see cref="Vector"/> and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <param name="type">
		/// Specified whether to return the full convolution or to return the center points and exclude some of 
		/// the edge effects.
		/// </param>
		/// <returns>A ComplexVector y, where y[i] = DotProduct(x1[j],x2[i-j]).</returns>
		public static ComplexVector Convolve(Vector x1, ComplexVector x2, ConvolutionType type)
		{
			return Compute.Convolve((ComplexVector)x1, x2,type);
		}

		/// <summary>
		/// Returns the convolution of a <see cref="ComplexVector"/> and a <see cref="Vector"/>.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <param name="type">
		/// Specified whether to return the full convolution or to return the center points and exclude some of 
		/// the edge effects.
		/// </param>
		/// <returns>A ComplexVector y, where y[i] = DotProduct(x1[j],x2[i-j]).</returns>
		public static ComplexVector Convolve(ComplexVector x1, Vector x2, ConvolutionType type)
		{
			return Compute.Convolve(x1, (ComplexVector)x2, type);
		}

		/// <summary>
		/// Returns the convolution of two <see cref="ComplexVector"/> instances.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <param name="type">
		/// Specified whether to return the full convolution or to return the center points and exclude some of 
		/// the edge effects.
		/// </param>
		/// <returns>A ComplexVector y, where y[i] = DotProduct(x1[j],x2[i-j]).</returns>
		public static ComplexVector Convolve(ComplexVector x1, ComplexVector x2, ConvolutionType type)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return Vector.Empty;

			if (x1.IsUnitary())
				return x1.Elements[0] * x2;

			if (x2.IsUnitary())
				return x1 * x2.Elements[0];

			bool x1IsRow = x1.IsRow();
			bool x2IsRow = x2.IsRow();
			if (x1IsRow != x2IsRow)
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_28"));

			int x1Length = x1.Length;
			int x2Length = x2.Length;

			int length;
			int shift;
			if (type == ConvolutionType.Center)
			{
				length = System.Math.Max(x1Length, x2Length);
				shift = System.Math.Min(x1Length, x2Length) / 2;
			}
			else
			{
				length = x1Length + x2Length - 1;
				shift = 0;
			}

			int[] newDimensions;
			if (x1IsRow)
				newDimensions = new int[] { 1, length };
			else
				newDimensions = new int[] { length, 1 };

			Complex sum;
			int x1Start = 0;
			int x1End = 0;
			int x2Start = 0;
			int x1VirtualStart = 1 - x2Length + shift;
			int x2VirtualStart = 1 - x1Length + shift;
			int x1VirtualEnd = shift;

			Complex[] newElements = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				x1Start = System.Math.Max(x1VirtualStart, 0);
				x1End = System.Math.Min(x1VirtualEnd, x1Length - 1);
				x2Start = System.Math.Max(x2VirtualStart, 0);

				sum = 0;
				for (int j1 = x1Start, j2 = x2Start + x1End - x1Start; j1 <= x1End; j1++, j2--)
				{
					sum += x1.Elements[j1] * x2.Elements[j2];
				}

				newElements[i] = sum;
				x1VirtualStart++;
				x2VirtualStart++;
				x1VirtualEnd++;
			}

			return new ComplexVector(newElements, newDimensions);
		}

		#endregion //Convolve

		#region CumProduct

		/// <summary>
		/// Returns the cumulative product of the elements of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>A Vector y, where y[i] = x[0]*...*x[i].</returns>
		/// <seealso cref="Vector.CumProduct()"/>
		/// <seealso cref="CumSum(Vector)"/>
		public static Vector CumProduct(Vector x)
		{
			return x.Clone().CumProduct();
		}

		/// <summary>
		/// Returns the cumulative product of the elements of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexVector"/>.</param>
		/// <returns>A ComplexVector y, where y[i] = x[0]*...*x[i].</returns>
		/// <seealso cref="ComplexVector.CumProduct()"/>
		/// <seealso cref="CumSum(ComplexVector)"/>
		public static ComplexVector CumProduct(ComplexVector x)
		{
			return x.Clone().CumProduct();
		}

		/// <summary>
		/// Returns the cumulative product of the elements of <paramref name="x"/> along the Nth dimension.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y[i1,...,iN,...,iM] = x[i1,...,0,...,iM]*...*x[i1,...,iN,...,iM].</returns>
		/// <seealso cref="Matrix.CumProduct(int)"/>
		/// <seealso cref="CumSum(Matrix,int)"/>
		public static Matrix CumProduct(Matrix x, int N)
		{
			return x.Clone().CumProduct(N);
		}

		/// <summary>
		/// Returns the cumulative product of the elements of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexMatrix"/>.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y[i1,...,iN,...,iM] = x[i1,...,0,...,iM]*...*x[i1,...,iN,...,iM].</returns>
		/// <seealso cref="ComplexMatrix.CumProduct(int)"/>
		/// <seealso cref="CumSum(ComplexMatrix,int)"/>
		public static ComplexMatrix CumProduct(ComplexMatrix x, int N)
		{
			return x.Clone().CumProduct(N);
		}

		#endregion //CumProduct

		#region CumSum

		/// <summary>
		/// Returns the cumulative sum of the elements of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="Vector"/>.</param>
		/// <returns>A Vector y, where y[i] = x[0]+...+x[i].</returns>
		/// <seealso cref="Vector.CumSum()"/>
		/// <seealso cref="CumProduct(Vector)"/>
		public static Vector CumSum(Vector x)
		{
			return x.Clone().CumSum();
		}

		/// <summary>
		/// Returns the cumulative sum of the elements of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexVector"/>.</param>
		/// <returns>A Vector y, where y[i] = x[0]+...+x[i].</returns>
		/// <seealso cref="ComplexVector.CumSum()"/>
		/// <seealso cref="CumProduct(ComplexVector)"/>
		public static ComplexVector CumSum(ComplexVector x)
		{
			return x.Clone().CumSum();
		}

		/// <summary>
		/// Returns the cumulative sum of the elements of <paramref name="x"/> along the Nth dimension.
		/// </summary>
		/// <param name="x">A <see cref="Matrix"/>.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y[i1,...,iN,...,iM] = x[i1,...,0,...,iM]+...+x[i1,...,iN,...,iM].</returns>
		/// <seealso cref="Matrix.CumProduct(int)"/>
		/// <seealso cref="CumSum(Matrix,int)"/>
		public static Matrix CumSum(Matrix x, int N)
		{
			return x.Clone().CumSum(N);
		}

		/// <summary>
		/// Returns the cumulative sum of the elements of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A <see cref="ComplexMatrix"/>.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y[i1,...,iN,...,iM] = x[i1,...,0,...,iM]+...+x[i1,...,iN,...,iM].</returns>
		/// <seealso cref="ComplexMatrix.CumProduct(int)"/>
		/// <seealso cref="CumSum(ComplexMatrix,int)"/>
		public static ComplexMatrix CumSum(ComplexMatrix x, int N)
		{
			return x.Clone().CumSum(N);
		}

		#endregion //CumSum

		#region Diagonal

		/// <summary>
		/// Returns the diagonal of an N-dimensional <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A [1 N] Matrix containing the diagonal of x.</returns>
		public static Matrix Diagonal(Matrix x)
		{
			if (x.IsEmpty())
				return Matrix.Empty;

			if (x.IsUnitary())
				return x.Clone();

			int curr;
			int minLength = int.MaxValue;
			for (int i = 0, length = x.Dimensions.Length; i < length; i++)
			{
				curr = x.Dimensions[i];
				if (curr < minLength)
					minLength = curr;
			}

			int rank = x.Rank;
			Vector size = x.Size;
			int[] subscript = new int[rank];
			Matrix result = new Matrix(0, minLength);
			for (int i = 0; i < minLength; i++)
			{ 
				if(i > 0)
				{
					for(int j = 0; j < rank; j++)
					{
						subscript[j]++;
					}
				}

				result.Elements[i] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
			}

			return result;
		}

		/// <summary>
		/// Returns the diagonal of an N-dimensional <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A [1 N] ComplexMatrix containing the diagonal of x.</returns>
		public static ComplexMatrix Diagonal(ComplexMatrix x)
		{
			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			if (x.IsUnitary())
				return x.Clone();

			int curr;
			int minLength = int.MaxValue;
			for (int i = 0, length = x.Dimensions.Length; i < length; i++)
			{
				curr = x.Dimensions[i];
				if (curr < minLength)
					minLength = curr;
			}

			int rank = x.Rank;
			Vector size = x.Size;
			int[] subscript = new int[rank];
			ComplexMatrix result = new ComplexMatrix(0, minLength);
			for (int i = 0; i < minLength; i++)
			{
				if (i > 0)
				{
					for (int j = 0; j < rank; j++)
					{
						subscript[j]++;
					}
				}

				result.Elements[i] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
			}

			return result;
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Returns the diagonal of an N-dimensional <see cref="BooleanMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A [1 N] BooleanMatrix containing the diagonal of x.</returns>
		public static BooleanMatrix Diagonal(BooleanMatrix x)
		{
			if (x.IsEmpty())
				return BooleanMatrix.Empty;

			if (x.IsUnitary())
				return x.Clone();

			int curr;
			int minLength = int.MaxValue;
			for (int i = 0, length = x.Dimensions.Length; i < length; i++)
			{
				curr = x.Dimensions[i];
				if (curr < minLength)
					minLength = curr;
			}

			int rank = x.Rank;
			Vector size = x.Size;
			int[] subscript = new int[rank];
			BooleanMatrix result = new BooleanMatrix(false, minLength);
			for (int i = 0; i < minLength; i++)
			{
				if (i > 0)
				{
					for (int j = 0; j < rank; j++)
					{
						subscript[j]++;
					}
				}

				result.Elements[i] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
			}

			return result;
		}

		#endregion //Diagonal

		#region Difference

		/// <summary>
		/// Returns a <see cref="Vector"/> with the difference of successive values of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = x[i+1] - x[i].</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when the length of x is less than 2.
		/// </exception>
		public static Vector Difference(Vector x)
		{
			int length = x.Length;
			if (length < 2)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_29"), "x");

			int[] newDimensions = x.Dimensions;
			if (newDimensions[0] > 1)
				newDimensions[0] = newDimensions[0] - 1;
			else
				newDimensions[1] = newDimensions[1] - 1;

			Vector result = new Vector(new double[length - 1], newDimensions);
			for (int i = 0, length2 = length - 1; i < length2; i++)
			{
				result[i] = x.Elements[i + 1] - x.Elements[i];
			}
			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the difference of successive values of <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector y, where y[i] = x[i+1] - x[i].</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when the length of x is less than 2.
		/// </exception>
		public static ComplexVector Difference(ComplexVector x)
		{
			int length = x.Length;
			if (length < 2)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_29"), "x");

			int[] newDimensions = x.Dimensions;
			if (newDimensions[0] > 1)
				newDimensions[0] = newDimensions[0] - 1;
			else
				newDimensions[1] = newDimensions[1] - 1;

			ComplexVector result = new ComplexVector(new Complex[length - 1], newDimensions);
			for (int i = 0, length2 = length - 1; i < length2; i++)
			{
				result[i] = x.Elements[i + 1] - x.Elements[i];
			}
			return result;
		}

		/// <summary>
		/// Returns the difference of successive values of the <see cref="Matrix"/> <paramref name="x"/> 
		/// along the Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix with the difference of successive values of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the Nth dimension has less than two elements.
		/// </exception>
		public static Matrix Difference(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (N > x.Rank || x.Dimensions[N-1] < 2)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_30"));

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] -= 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1]--;

			double prev;
			double curr;
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				subscript[Nm1] = 0;
				prev = x.Elements[Compute.SubscriptToIndex(subscript, size)];

				for (int j = 1; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					subscript[Nm1] = j-1;
					result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr - prev;
					prev = curr;
				}

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns the difference of successive values of the <see cref="ComplexMatrix"/> <paramref name="x"/> 
		/// along the Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix with the difference of successive values of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the Nth dimension has less than two elements.
		/// </exception>
		public static ComplexMatrix Difference(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (N > x.Rank || x.Dimensions[N - 1] < 2)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_30"));

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] -= 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1]--;

			Complex prev;
			Complex curr;
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				subscript[Nm1] = 0;
				prev = x.Elements[Compute.SubscriptToIndex(subscript, size)];

				for (int j = 1; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					subscript[Nm1] = j - 1;
					result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr - prev;
					prev = curr;
				}

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Difference

		#region Derivative

		/// <summary>
		/// Returns the partial derivative of the <see cref="Matrix"/> f along the Nth dimension.
		/// </summary>
		/// <param name="f">A Matrix representing a multi-dimensional function.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>The partial derivative of f with respect to the Nth dimension.</returns>
		/// <remarks>The spacing between the values of f is assumed to be 1.</remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Derivative(Matrix f, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (f.IsEmpty())
				return Matrix.Empty;

			int Nm1 = N - 1;
			if (N > f.Rank || f.Dimensions[Nm1] == 1)
				return Compute.Zeros(f.Dimensions);

			int[] newDimensions = (int[])f.Dimensions.Clone();
			int lengthN = f.Dimensions[Nm1];
			int lengthNm1 = lengthN - 1;
			int newLength = f.Length;

			double curr = 0;
			double prev1 = 0;
			double prev2 = 0;
			int[] subscript = new int[f.Rank];
			Matrix result = new Matrix(new double[newLength], newDimensions);

			Vector size = f.Size;
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					if (j == 0)
					{
						subscript[Nm1] = 1;
						curr = f.Elements[Compute.SubscriptToIndex(subscript, size)];

						subscript[Nm1] = 0;
						prev1 = f.Elements[Compute.SubscriptToIndex(subscript, size)];

						result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr - prev1;

						prev2 = prev1;
						prev1 = curr;
					}
					else if (j < lengthNm1)
					{
						subscript[Nm1] = j + 1;
						curr = f.Elements[Compute.SubscriptToIndex(subscript, size)];

						subscript[Nm1] = j;
						result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = 0.5 * (curr - prev2);

						prev2 = prev1;
						prev1 = curr;
					}
					else 
					{
						subscript[Nm1] = j;
						result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = prev1 - prev2;
					}
				}

				Utilities.IncrementSubscript(ref subscript, N, f.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns the partial derivative of the <see cref="ComplexMatrix"/> f along the Nth dimension.
		/// </summary>
		/// <param name="f">A ComplexMatrix representing a multi-dimensional function.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>The partial derivative of f with respect to the Nth dimension.</returns>
		/// <remarks>The spacing between the values of f is assumed to be 1.</remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Derivative(ComplexMatrix f, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (f.IsEmpty())
				return ComplexMatrix.Empty;

			int Nm1 = N - 1;
			if (N > f.Rank || f.Dimensions[Nm1] == 1)
				return Compute.Zeros(f.Dimensions);

			int[] newDimensions = (int[])f.Dimensions.Clone();
			int lengthN = f.Dimensions[Nm1];
			int lengthNm1 = lengthN - 1;
			int newLength = f.Length;

			Complex curr = 0;
			Complex prev1 = 0;
			Complex prev2 = 0;
			int[] subscript = new int[f.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[newLength], newDimensions);

			Vector size = f.Size;
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					if (j == 0)
					{
						subscript[Nm1] = 1;
						curr = f.Elements[Compute.SubscriptToIndex(subscript, size)];

						subscript[Nm1] = 0;
						prev1 = f.Elements[Compute.SubscriptToIndex(subscript, size)];

						result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr - prev1;

						prev2 = prev1;
						prev1 = curr;
					}
					else if (j < lengthNm1)
					{
						subscript[Nm1] = j + 1;
						curr = f.Elements[Compute.SubscriptToIndex(subscript, size)];

						subscript[Nm1] = j;
						result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = 0.5 * (curr - prev2);

						prev2 = prev1;
						prev1 = curr;
					}
					else
					{
						subscript[Nm1] = j;
						result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = prev1 - prev2;
					}
				}

				Utilities.IncrementSubscript(ref subscript, N, f.Dimensions);
			}

			return result;
		}

		#endregion //Derivative

		#region Determinant

		/// <summary>
		/// Returns the determinant of a square <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A square Matrix.</param>
		/// <returns>The determinant of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not a square Matrix.
		/// </exception>
		/// <seealso cref="Compute.MinorMatrix(Matrix,int,int)"/>
		public static double Determinant(Matrix x)
		{
			if (!x.IsSquare())
				throw new ArgumentException("Cannot compute the determinant of a non-square Matrix.");

			if (x.IsEmpty())
				return 0;

			if (x.IsUnitary())
				return x[0];

			return Compute.Determinant_Helper(x);
		}

		/// <summary>
		/// Returns the determinant of a square <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A square ComplexMatrix.</param>
		/// <returns>The determinant of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not a square ComplexMatrix.
		/// </exception>
		/// <seealso cref="Compute.MinorMatrix(ComplexMatrix,int,int)"/>
		public static Complex Determinant(ComplexMatrix x)
		{
			if (!x.IsSquare())
				throw new ArgumentException("Cannot compute the determinant of a non-square ComplexMatrix.");

			if (x.IsEmpty())
				return 0;

			if (x.IsUnitary())
				return x[0];

			return Compute.Determinant_Helper(x);
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static double Determinant_Helper(Matrix x)
		{
			int N = x.Dimensions[0];
			if (N == 2)
				return x.Elements[0] * x.Elements[3] - x.Elements[2] * x.Elements[1];

			double result = 0;
			for (int i = 0; i < N; i++)
			{
				if(Compute.IsEven(i))
					result += x[0, i] * Compute.Determinant_Helper(Compute.MinorMatrix(x, 0, i));
				else
					result -= x[0, i] * Compute.Determinant_Helper(Compute.MinorMatrix(x, 0, i));
			}
			return result;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static Complex Determinant_Helper(ComplexMatrix x)
		{
			int N = x.Dimensions[0];
			if (N == 2)
				return x.Elements[0] * x.Elements[3] - x.Elements[2] * x.Elements[1];

			Complex result = 0;
			for (int i = 0; i < N; i++)
			{
				if (Compute.IsEven(i))
					result += x[0, i] * Compute.Determinant_Helper(Compute.MinorMatrix(x, 0, i));
				else
					result -= x[0, i] * Compute.Determinant_Helper(Compute.MinorMatrix(x, 0, i));
			}
			return result;
		}

		#endregion //Determinant

		#region Find

		/// <summary>
		/// Find takes a <see cref="BooleanVector"/> <paramref name="x"/> and returns the indices where x is True.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A Vector y, where y[j] = i when x[i] is the jth occurrence of True in x.</returns>
		/// <remarks>
		/// Find is meant to be used with the Compare operators on Vector and ComplexVector, which return
		/// BooleanVector instances. The following notation is convenient:
		/// 
		/// int length = 1000;
		/// Vector x1 = Compute.Line(0,2,length);
		/// Vector x2 = new Vector(length);
		/// 
		/// x2[Compute.Find(x1 > 1)] = 1;
		/// </remarks>
		public static Vector Find(BooleanVector x)
		{ 
			if(x.IsEmpty())
				return Vector.Empty;

			List<double> indices = new List<double>();
			for(int i = 0, length = x.Length; i < length; i++)
			{
				if(x.Elements[i])
					indices.Add(i);
			}

			if(indices.Count == 0)
				return Vector.Empty;

			return new Vector(indices);
		}

		/// <summary>
		/// Find takes a <see cref="BooleanMatrix"/> <paramref name="x"/> and returns the indices where x is True.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A Vector y, where y[j] = i when x[i] is the jth occurrence of True in x.</returns>
		/// <remarks>
		/// Find is meant to be used with the Compare operators on Matrix and ComplexMatrix, which return
		/// BooleanMatrix instances. The following notation is convenient:
		/// 
		/// int length = 1000;
		/// Matrix x1 = (Matrix)Compute.Line(0,2,length);
		/// Vector x2 = new Vector(length);
		/// 
		/// x2[Compute.Find(x1 > 1)] = 1;
		/// </remarks>
		public static Vector Find(BooleanMatrix x)
		{
			if (x.IsEmpty())
				return Vector.Empty;

			List<double> indices = new List<double>();
			for (int i = 0, length = x.Length; i < length; i++)
			{
				if (x.Elements[i])
					indices.Add(i);
			}

			if (indices.Count == 0)
				return Vector.Empty;

			return new Vector(indices);
		}

		#endregion //Find

		#region IdentityMatrix

		/// <summary>
		/// Returns an N-dimensional identity <see cref="Matrix"/>.
		/// </summary>
		/// <param name="N">The dimensions of the identity Matrix.</param>
		/// <returns>An N-dimensional identity Matrix.</returns>
		/// <remarks>
		/// If only one dimension is passed in, a two-dimensional square identity Matrix is returned.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when N[i] is less than 1.
		/// </exception>
		public static Matrix IdentityMatrix(params int[] N)
		{
			int curr;
			int minLength = int.MaxValue;
			int NLength = N.Length;
			for (int i = 0; i < N.Length; i++)
			{
				curr = N[i];
				if (curr < 1)
					throw new ArgumentException("Dimension numbers must be greater than 0");

				if (curr < minLength)
					minLength = curr;
			}

			if (NLength == 1)
			{
				int temp = N[0];
				N = new int[] { temp, temp };
				NLength = 2;
			}

			Matrix result = Compute.Zeros(N);
			Vector size = result.Size;
			int[] subscript = new int[N.Length];

			for (int i = 0; i < minLength; i++)
			{
				if (i > 0)
				{
					for (int j = 0; j < NLength; j++)
					{
						subscript[j]++;
					}
				}

				result.Elements[Compute.SubscriptToIndex(subscript, size)] = 1;
			}

			return result;
		}

		#endregion //IdentityMatrix

		#region Index

		/// <summary>
		/// Returns a <see cref="Vector"/> of successive indices from 0 to <paramref name="end"/>.
		/// </summary>
		/// <param name="end">The last element of the Vector.</param>
		/// <returns>A Vector of successive indices.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when end is less than 0.
		/// </exception>
		/// <seealso cref="Line(double,double,int)"/>
		/// <seealso cref="Line(Complex,Complex,int)"/>
		public static Vector Index(int end)
		{
			if (end < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_31"));

			int length = end + 1;
			double[] index = new double[length];
			for (int i = 0; i < length; i++)
			{
				index[i] = i;
			}
			return new Vector(index);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> of successive indices from <paramref name="start"/> to <paramref name="end"/>.
		/// </summary>
		/// <param name="start">The first index of the Vector.</param>
		/// <param name="end">The last index of the Vector.</param>
		/// <returns>A Vector of successive indices.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when either start or end is less than 0.
		/// </exception>
		/// <seealso cref="Line(double,double,int)"/>
		/// <seealso cref="Line(Complex,Complex,int)"/>
		public static Vector Index(int start, int end)
		{
			if (start < 0 || end < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_31"));

			int sign = 1;
			if (start > end)
			{
				sign = -1;
			}

			int length = (int)Compute.Abs(start - end) + 1;
			double[] index = new double[length];
			for (int i = 0; i < length; i++)
			{
				index[i] = start + sign*i;
			}
			return new Vector(index);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> of evenly spaced indices from <paramref name="start"/> 
		/// to <paramref name="end"/>.
		/// </summary>
		/// <param name="start">The first element of the Vector.</param>
		/// <param name="space">The spacing between the elements.</param>
		/// <param name="end">An upperbound on the last element of the Vector.</param>
		/// <returns>A Vector of evenly spaced indices.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when either start or end is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="space"/> is equal to 0 and start does not equal end.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when space is positive but start is greater than end.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when space is negative but start is less than end.
		/// </exception>
		/// <seealso cref="Line(double,double,int)"/>
		/// <seealso cref="Line(Complex,Complex,int)"/>
		public static Vector Index(int start, int space, int end)
		{
			if (start < 0 || end < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_31"));
			
			if(space == 0 && start != end)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_33"), "space");

			if(start < end && space < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_32"));

			if (start > end && space > 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_32"));

			int length = 1;
			if (System.Math.Abs(space) > 0)
				length = (int)Compute.Abs((start - end)/space) + 1;

			double[] index = new double[length];
			for (int i = 0, value = start; i < length; i++, value += space)
			{
				index[i] = value;
			}

			return new Vector(index);
		}

		#endregion //Index

		#region IndexToSubscript

		/// <summary>
		/// Returns the <see cref="Matrix"/> subscript that corresponds with the one-dimensional <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The one-dimensional Matrix index.</param>
		/// <param name="size">The dimensions of a Matrix.</param>
		/// <returns>The multi-dimensional subscript that corresponds to the one-dimensional index.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when size is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the index it out of the bounds specified by the size <see cref="Vector"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when a dimensions is less than 1.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the size Vector has non-integer elements.
		/// </exception>
		public static int[] IndexToSubscript(int index, Vector size)
		{
			//Pre-processing for the dimension calculations.
			int N = size.Length;
			int[] subscript = new int[N];
			Vector dimProduct = Compute.CumProduct(size);
			dimProduct.Reverse();

			if (size.IsEmpty())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_35"));

			//Array indices must be between 0 and Length - 1.
			if (index < 0 || index >= dimProduct[0])
			{
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_36"));
			}

			//Exception handling.
			int currDimension;
			double currDimensionCheck;
			for (int i = 0; i < N; i++)
			{
				currDimensionCheck = size.Elements[i];
				currDimension = (int)Compute.Round(currDimensionCheck);

				if (currDimensionCheck != currDimension)
					throw new ArgumentOutOfRangeException( "size", Compute.GetString("LE_ArgumentOutOfRangeException_12"));

				if (currDimension < 1)
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_37"), "size");
			}

			//Loop to calculate each dimension.
			int remainder = index;
			int totalDimensionLength;
			for (int i = 1; i < N; i++)
			{
				totalDimensionLength = (int)Compute.Round(dimProduct.Elements[i]);
				remainder = index % totalDimensionLength;
				subscript[N - i] = ((index - remainder) / totalDimensionLength);
				index = remainder;
			}
			subscript[0] = remainder;

			//Return the subscript.
			return subscript;
		}

		#endregion //IndexToSubscript

		#region Line

		/// <summary>
		/// Returns a <see cref="Vector"/> with evenly spaced elements.
		/// </summary>
		/// <param name="start">The first element of the Vector.</param>
		/// <param name="end">The last element of the Vector.</param>
		/// <param name="length">The length of the Vector.</param>
		/// <returns>A Vector with evenly spaced elements.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="length"/> is less than 1.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="start"/> equals <paramref name="end"/> but length does not equal 1.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="start"/> does not equal <paramref name="end"/> but length equals 1.
		/// </exception>
		/// <seealso cref="Index(int)"/>
		/// <seealso cref="Index(int,int)"/>
		/// <seealso cref="Index(int,int,int)"/>
		public static Vector Line(double start, double end, int length)
		{
			if (length < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_39"), "length");

			if(start == end && length != 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_38"));

			if (length == 1)
			{
				if (start == end)
					return new Vector(start, 1);
				else
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_38"));
			}

			double space = 0;
			if(length > 1)
				space = (end - start) / (length - 1);

			double value = start;
			double[] index = new double[length];
			for (int i = 0; i < length-1; i++)
			{
				index[i] = value;
				value += space;
			}
			index[length - 1] = end;
			return new Vector(index);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with evenly spaced <see cref="Complex"/> elements.
		/// </summary>
		/// <param name="start">The first element of the ComplexVector.</param>
		/// <param name="end">The last element of the ComplexVector.</param>
		/// <param name="length">The length of the ComplexVector.</param>
		/// <returns>A ComplexVector with evenly spaced elements.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="length"/> is less than 1.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="start"/> equals <paramref name="end"/> but length does not equal 1.
		/// </exception>
		/// <seealso cref="Index(int)"/>
		/// <seealso cref="Index(int,int)"/>
		/// <seealso cref="Index(int,int,int)"/>
		public static ComplexVector Line(Complex start, Complex end, int length)
		{
			if (length < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_39"), "length");

			if (start == end && length != 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_38"));

			if (length == 1)
			{
				if (start == end)
					return new ComplexVector(start, 1);
				else
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_38"));
			}

			Complex space = 0;
			if(length > 1)
				space = (end - start) / (length - 1);

			Complex value = start;
			Complex[] index = new Complex[length];
			for (int i = 0; i < length - 1; i++)
			{
				index[i] = value;
				value += space;
			}
			index[length - 1] = end;
			return new ComplexVector(index);
		}

		#endregion //Line

		#region MatrixProduct

		/// <summary>
		/// Returns the product of two <see cref="Matrix"/> instances.
		/// </summary>
		/// <param name="x1">The first two-dimensional Matrix.</param>
		/// <param name="x2">The second two-dimensional Matrix.</param>
		/// <returns>The product of two Matrices.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when either <paramref name="x1"/> or <paramref name="x2"/> is not two-dimensional.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs when the second dimension of x1 is not the same size as the first dimension of x2.
		/// </exception>
		public static Matrix MatrixProduct(Matrix x1, Matrix x2)
		{
			return Compute.MatrixProductHelper(
				x1.Elements, x1.Dimensions, 
				x2.Elements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the product of a <see cref="Vector"/> and a <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x1">The Vector.</param>
		/// <param name="x2">The two-dimensional Matrix.</param>
		/// <returns>The product of the Vector and Matrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when <paramref name="x2"/> is not two-dimensional.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs when the second dimension of x1 is not the same size as the first dimension of x2.
		/// </exception>
		public static Matrix MatrixProduct(Vector x1, Matrix x2)
		{
			return Compute.MatrixProductHelper(
				x1.Elements, x1.Dimensions,
				x2.Elements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the product of a <see cref="Matrix"/> and a <see cref="Vector"/>.
		/// </summary>
		/// <param name="x1">The two-dimensional Matrix.</param>
		/// <param name="x2">The Vector.</param>
		/// <returns>The product of the Matrix and Vector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when <paramref name="x1"/> is not two-dimensional.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs when the second dimension of x1 is not the same size as the first dimension of x2.
		/// </exception>
		public static Matrix MatrixProduct(Matrix x1, Vector x2)
		{
			return Compute.MatrixProductHelper(
				x1.Elements, x1.Dimensions,
				x2.Elements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the product of two <see cref="Vector"/> instances.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>The product of two Vectors.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the second dimension of x1 is not the same size as the first dimension of x2.
		/// </exception>
		public static Matrix MatrixProduct(Vector x1, Vector x2)
		{
			return Compute.MatrixProductHelper(
				x1.Elements, x1.Dimensions,
				x2.Elements, x2.Dimensions);
		}

		private static Matrix MatrixProductHelper(IList<double> x1Elements, int[] x1Dimensions, IList<double> x2Elements, int[] x2Dimensions)
		{
			if (x1Dimensions.Length != 2 || x2Dimensions.Length != 2)
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_10"));

			if (x1Dimensions[1] != x2Dimensions[0])
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_11"));

			bool x1IsEmpty = (x1Elements == null || x1Elements.Count == 0);
			bool x2IsEmpty = (x2Elements == null || x2Elements.Count == 0);
			
			if (x1IsEmpty && x2IsEmpty)
				return Matrix.Empty;

			double dotProduct;
			int N = x1Dimensions[1];
			int N1 = x1Dimensions[0];
			int N2 = x2Dimensions[1];
			Matrix result = Compute.Zeros(N1, N2);
			for (int i = 0; i < N1; i++)
			{
				for (int j = 0; j < N2; j++)
				{
					dotProduct = 0;
					for (int n = 0; n < N; n++)
					{
						dotProduct +=
							x1Elements[Compute.SubscriptToIndex(x1Dimensions, i, n)] *
							x2Elements[Compute.SubscriptToIndex(x2Dimensions, n, j)];
					}
					result[i, j] = dotProduct;
				}
			}

			return result;
		}

		/// <summary>
		/// Returns the product of two <see cref="ComplexMatrix"/> instances.
		/// </summary>
		/// <param name="x1">The first two-dimensional ComplexMatrix.</param>
		/// <param name="x2">The second two-dimensional ComplexMatrix.</param>
		/// <returns>The product of two ComplexMatrices.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when either <paramref name="x1"/> or <paramref name="x2"/> is not two-dimensional.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs when the second dimension of x1 is not the same size as the first dimension of x2.
		/// </exception>
		public static ComplexMatrix MatrixProduct(ComplexMatrix x1, ComplexMatrix x2)
		{
			return Compute.MatrixProductHelper(
				x1.Elements, x1.Dimensions,
				x2.Elements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the product of a <see cref="ComplexVector"/> and a <see cref="ComplexMatrix"/>.
		/// </summary>
		/// <param name="x1">The ComplexVector.</param>
		/// <param name="x2">The two-dimensional ComplexMatrix.</param>
		/// <returns>The product of the ComplexVector and ComplexMatrix.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when <paramref name="x2"/> is not two-dimensional.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs when the second dimension of x1 is not the same size as the first dimension of x2.
		/// </exception>
		public static ComplexMatrix MatrixProduct(ComplexVector x1, ComplexMatrix x2)
		{
			return Compute.MatrixProductHelper(
				x1.Elements, x1.Dimensions,
				x2.Elements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the product of a <see cref="ComplexMatrix"/> and a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x1">The two-dimensional ComplexMatrix.</param>
		/// <param name="x2">The ComplexVector.</param>
		/// <returns>The product of the ComplexMatrix and ComplexVector.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when <paramref name="x1"/> is not two-dimensional.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs when the second dimension of x1 is not the same size as the first dimension of x2.
		/// </exception>
		public static ComplexMatrix MatrixProduct(ComplexMatrix x1, ComplexVector x2)
		{
			return Compute.MatrixProductHelper(
				x1.Elements, x1.Dimensions,
				x2.Elements, x2.Dimensions);
		}

		/// <summary>
		/// Returns the product of two <see cref="ComplexVector"/> instances.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>The product of two ComplexVectors.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the second dimension of x1 is not the same size as the first dimension of x2.
		/// </exception>
		public static ComplexMatrix MatrixProduct(ComplexVector x1, ComplexVector x2)
		{
			return Compute.MatrixProductHelper(
				x1.Elements, x1.Dimensions,
				x2.Elements, x2.Dimensions);
		}

		private static ComplexMatrix MatrixProductHelper(IList<Complex> x1Elements, int[] x1Dimensions, IList<Complex> x2Elements, int[] x2Dimensions)
		{
			if (x1Dimensions.Length != 2 || x2Dimensions.Length != 2)
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_12"));

			if (x1Dimensions[1] != x2Dimensions[0])
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_11"));

			bool x1IsEmpty = (x1Elements == null || x1Elements.Count == 0);
			bool x2IsEmpty = (x2Elements == null || x2Elements.Count == 0);

			if (x1IsEmpty && x2IsEmpty)
				return ComplexMatrix.Empty;

			Complex dotProduct;
			int N = x1Dimensions[1];
			int N1 = x1Dimensions[0];
			int N2 = x2Dimensions[1];
			ComplexMatrix result = new ComplexMatrix(new int[] { N1, N2 });
			for (int i = 0; i < N1; i++)
			{
				for (int j = 0; j < N2; j++)
				{
					dotProduct = 0;
					for (int n = 0; n < N; n++)
					{
						dotProduct +=
							x1Elements[Compute.SubscriptToIndex(x1Dimensions, i, n)] *
							x2Elements[Compute.SubscriptToIndex(x2Dimensions, n, j)];
					}
					result[i, j] = dotProduct;
				}
			}

			return result;
		}

		#endregion //MatrixProduct

		#region MinorMatrix

		/// <summary>
		/// Returns the (i,j) minor <see cref="Matrix"/> of <paramref name="x"/>. The (i,j) minor Matrix of x is the 
		/// result of removing the ith row and jth column of x.
		/// </summary>
		/// <param name="x">A two-dimensional Matrix.</param>
		/// <param name="i">A row index.</param>
		/// <param name="j">A column index.</param>
		/// <returns>The (i,j) minor Matrix</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not two-dimensional.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if <paramref name="i"/> or <paramref name="j"/> are out of bounds.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		public static Matrix MinorMatrix(Matrix x, int i, int j)
		{
			if (!x.IsTwoDimensional())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_41"));

			if (i < 0 || j < 0 || i >= x.Dimensions[0] || j >= x.Dimensions[1])
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_42"));

			if (x.IsEmpty())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_43"));

			if (x.IsUnitary())
				return Matrix.Empty;

			Matrix result = new Matrix(0, new int[] { x.Dimensions[0] - 1, x.Dimensions[1] - 1 });
			for (int m = 0, m2 = 0, lengthm = result.Dimensions[0]; m < lengthm; m++, m2++)
			{
				if (m == i)
					m2++;

				for (int n = 0, n2 = 0, lengthn = result.Dimensions[1]; n < lengthn; n++, n2++)
				{
					if (n == j)
						n2++;

					result[m, n] = x[m2, n2];
				}
			}

			return result;
		}

		/// <summary>
		/// Returns the (i,j) minor <see cref="ComplexMatrix"/> of <paramref name="x"/>. The (i,j) minor ComplexMatrix 
		/// of x is the result of removing the ith row and jth column of x.
		/// </summary>
		/// <param name="x">A two-dimensional ComplexMatrix.</param>
		/// <param name="i">A row index.</param>
		/// <param name="j">A column index.</param>
		/// <returns>The (i,j) minor ComplexMatrix</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not two-dimensional.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if <paramref name="i"/> or <paramref name="j"/> are out of bounds.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		public static ComplexMatrix MinorMatrix(ComplexMatrix x, int i, int j)
		{
			if (!x.IsTwoDimensional())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_41"));

			if (i < 0 || j < 0 || i >= x.Dimensions[0] || j >= x.Dimensions[1])
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_42"));

			if (x.IsEmpty())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_43"));

			if (x.IsUnitary())
				return ComplexMatrix.Empty;

			ComplexMatrix result = new ComplexMatrix(0, new int[] { x.Dimensions[0] - 1, x.Dimensions[1] - 1 });
			for (int m = 0, m2 = 0, lengthm = result.Dimensions[0]; m < lengthm; m++, m2++)
			{
				if (m == i)
					m2++;

				for (int n = 0, n2 = 0, lengthn = result.Dimensions[1]; n < lengthn; n++, n2++)
				{
					if (n == j)
						n2++;

					result[m, n] = x[m2, n2];
				}
			}

			return result;
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Returns the (i,j) minor <see cref="BooleanMatrix"/> of <paramref name="x"/>. The (i,j) minor BooleanMatrix 
		/// of x is the result of removing the ith row and jth column of x.
		/// </summary>
		/// <param name="x">A two-dimensional BooleanMatrix.</param>
		/// <param name="i">A row index.</param>
		/// <param name="j">A column index.</param>
		/// <returns>The (i,j) minor BooleanMatrix</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if x is not two-dimensional.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if <paramref name="i"/> or <paramref name="j"/> are out of bounds.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		public static BooleanMatrix MinorMatrix(BooleanMatrix x, int i, int j)
		{
			if (!x.IsTwoDimensional())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_41"));

			if (i < 0 || j < 0 || i >= x.Dimensions[0] || j >= x.Dimensions[1])
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_42"));

			if (x.IsEmpty())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_43"));

			if (x.IsUnitary())
				return BooleanMatrix.Empty;

			BooleanMatrix result = new BooleanMatrix(false, new int[] { x.Dimensions[0] - 1, x.Dimensions[1] - 1 });
			for (int m = 0, m2 = 0, lengthm = result.Dimensions[0]; m < lengthm; m++, m2++)
			{
				if (m == i)
					m2++;

				for (int n = 0, n2 = 0, lengthn = result.Dimensions[1]; n < lengthn; n++, n2++)
				{
					if (n == j)
						n2++;

					result[m, n] = x[m2, n2];
				}
			}

			return result;
		}

		#endregion //MinorMatrix

		#region Product

		/// <summary>
		/// Returns the product of the elements of a <see cref="Vector"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A double y, where y = x[0]*...*x[N].</returns>
		/// <seealso cref="Sum(Vector)"/>
		public static double Product(Vector x)
		{
			if (x.IsEmpty())
				return 1;

			double value = 1;
			for (int i = 0, length = x.Length; i < length; i++)
			{
				value *= x.Elements[i];
			}
			return value;
		}

		/// <summary>
		/// Returns the product of the elements of a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A <see cref="Complex"/> number y, where y = x[0]*...*x[N].</returns>
		/// <seealso cref="Sum(ComplexVector)"/>
		public static Complex Product(ComplexVector x)
		{
			if (x.IsEmpty())
				return 1;

			Complex value = 1;
			for (int i = 0, length = x.Length; i < length; i++)
			{
				value *= x.Elements[i];
			}
			return value;
		}

		/// <summary>
		/// Returns the product of the elements of the <see cref="Matrix"/> <paramref name="x"/> along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix with the product of the elements of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Product(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] = 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;

			double curr;
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				curr = 1;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr *= x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr;
				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns the product of the elements of the <see cref="ComplexMatrix"/> <paramref name="x"/> along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix with the product of the elements of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Product(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] = 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;

			Complex curr;
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				curr = 1;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr *= x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr;
				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Product

		#region Repeat

		/// <summary>
		/// Appends a <see cref="Vector"/> to itself <paramref name="N"/> times.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="N">The number of repeats.</param>
		/// <returns>A Vector composed of <paramref name="x"/> appended to itself N times.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than 0.
		/// </exception>
		public static Vector Repeat(Vector x, int N)
		{
			if (N < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_44"));

			if (N == 0 || x.IsEmpty())
				return Vector.Empty;

			int length;
			int[] newDimensions;
			double[] newElements;
			if (x.IsUnitary())
			{
				length = N;
				newDimensions = new int[] { 1, length };
				double x0 = x.Elements[0];
				newElements = new double[length];
				for (int i = 0; i < length; i++)
				{
					newElements[i] = x0;
				}
			}
			else
			{
				int start = 0;
				int xLength = x.Length;
				length = xLength * N;

				if(x.IsRow())
					newDimensions = new int[] { 1, length };
				else
					newDimensions = new int[] { length, 1 };

				newElements = new double[length];
				for (int i = 0; i < N; i++)
				{
					x.CopyTo(newElements, start);
					start += xLength;
				}
			}

			return new Vector(newElements, newDimensions);
		}

		/// <summary>
		/// Appends a <see cref="ComplexVector"/> to itself <paramref name="N"/> times.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <param name="N">The number of repeats.</param>
		/// <returns>A ComplexVector composed of <paramref name="x"/> appended to itself N times.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than 0.
		/// </exception>
		public static ComplexVector Repeat(ComplexVector x, int N)
		{
			if (N < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_44"));

			if (N == 0 || x.IsEmpty())
				return ComplexVector.Empty;

			int length;
			int[] newDimensions;
			Complex[] newElements;
			if (x.IsUnitary())
			{
				length = N;
				newDimensions = new int[] { 1, length };
				Complex x0 = x.Elements[0];
				newElements = new Complex[length];
				for (int i = 0; i < length; i++)
				{
					newElements[i] = x0;
				}
			}
			else
			{
				int start = 0;
				int xLength = x.Length;
				length = xLength * N;

				if (x.IsRow())
					newDimensions = new int[] { 1, length };
				else
					newDimensions = new int[] { length, 1 };

				newElements = new Complex[length];
				for (int i = 0; i < N; i++)
				{
					x.CopyTo(newElements, start);
					start += xLength;
				}
			}

			return new ComplexVector(newElements, newDimensions);
		}

		/// <summary>
		/// Appends a <see cref="BooleanVector"/> to itself <paramref name="N"/> times.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <param name="N">The number of repeats.</param>
		/// <returns>A BooleanVector composed of <paramref name="x"/> appended to itself N times.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than 0.
		/// </exception>
		public static BooleanVector Repeat(BooleanVector x, int N)
		{
			if (N < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_44"));

			if (N == 0 || x.IsEmpty())
				return BooleanVector.Empty;

			int length;
			int[] newDimensions;
			bool[] newElements;
			if (x.IsUnitary())
			{
				length = N;
				newDimensions = new int[] { 1, length };
				bool x0 = x.Elements[0];
				newElements = new bool[length];
				for (int i = 0; i < length; i++)
				{
					newElements[i] = x0;
				}
			}
			else
			{
				int start = 0;
				int xLength = x.Length;
				length = xLength * N;

				if (x.IsRow())
					newDimensions = new int[] { 1, length };
				else
					newDimensions = new int[] { length, 1 };

				newElements = new bool[length];
				for (int i = 0; i < N; i++)
				{
					x.CopyTo(newElements, start);
					start += xLength;
				}
			}

			return new BooleanVector(newElements, newDimensions);
		}

		/// <summary>
		/// Appends a <see cref="Matrix"/> to itself <paramref name="N"/> times along a new dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The number of repeats.</param>
		/// <returns>A Matrix composed of <paramref name="x"/> appended to itself N times.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than 0.
		/// </exception>
		public static Matrix Repeat(Matrix x, int N)
		{
			if (N < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_44"));

			if (N == 0 || x.IsEmpty())
				return Matrix.Empty;

			if (N == 1)
				return x.Clone();

			int newRank = x.Rank + 1;
			int[] newDimensions = new int[newRank];
			x.Dimensions.CopyTo(newDimensions, 0);
			newDimensions[newRank-1] = N;

			int[] newXDimensions = new int[newRank];
			x.Dimensions.CopyTo(newXDimensions, 0);
			newXDimensions[newRank - 1] = 1;
			Matrix newX = new Matrix(x.Elements, newXDimensions);

			Vector[] index = new Vector[newRank];
			for (int i = 0; i < x.Rank; i++)
			{
				index[i] = Compute.Index(x.Dimensions[i]-1);
			}


			Matrix result = new Matrix(new double[Utilities.Product(newDimensions)], newDimensions);
			for(int i = 0; i < N; i++)
			{
				index[newRank-1] = i;
				result[index] = newX;
			}

			return result;
		}

		/// <summary>
		/// Appends a <see cref="ComplexMatrix"/> to itself <paramref name="N"/> times along a new dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The number of repeats.</param>
		/// <returns>A ComplexMatrix composed of <paramref name="x"/> appended to itself N times.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than 0.
		/// </exception>
		public static ComplexMatrix Repeat(ComplexMatrix x, int N)
		{
			if (N < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_44"));

			if (N == 0 || x.IsEmpty())
				return ComplexMatrix.Empty;

			if (N == 1)
				return x.Clone();

			int newRank = x.Rank + 1;
			int[] newDimensions = new int[newRank];
			x.Dimensions.CopyTo(newDimensions, 0);
			newDimensions[newRank - 1] = N;

			int[] newXDimensions = new int[newRank];
			x.Dimensions.CopyTo(newXDimensions, 0);
			newXDimensions[newRank - 1] = 1;
			ComplexMatrix newX = new ComplexMatrix(x.Elements, newXDimensions);

			Vector[] index = new Vector[newRank];
			for (int i = 0; i < x.Rank; i++)
			{
				index[i] = Compute.Index(x.Dimensions[i] - 1);
			}


			ComplexMatrix result = new ComplexMatrix(new Complex[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0; i < N; i++)
			{
				index[newRank - 1] = i;
				result[index] = newX;
			}

			return result;
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Appends a <see cref="BooleanMatrix"/> to itself <paramref name="N"/> times along a new dimension.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <param name="N">The number of repeats.</param>
		/// <returns>A BooleanMatrix composed of <paramref name="x"/> appended to itself N times.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than 0.
		/// </exception>
		public static BooleanMatrix Repeat(BooleanMatrix x, int N)
		{
			if (N < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_44"));

			if (N == 0 || x.IsEmpty())
				return BooleanMatrix.Empty;

			if (N == 1)
				return x.Clone();

			int newRank = x.Rank + 1;
			int[] newDimensions = new int[newRank];
			x.Dimensions.CopyTo(newDimensions, 0);
			newDimensions[newRank - 1] = N;

			int[] newXDimensions = new int[newRank];
			x.Dimensions.CopyTo(newXDimensions, 0);
			newXDimensions[newRank - 1] = 1;
			BooleanMatrix newX = new BooleanMatrix(x.Elements, newXDimensions);

			Vector[] index = new Vector[newRank];
			for (int i = 0; i < x.Rank; i++)
			{
				index[i] = Compute.Index(x.Dimensions[i] - 1);
			}


			BooleanMatrix result = new BooleanMatrix(new bool[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0; i < N; i++)
			{
				index[newRank - 1] = i;
				result[index] = newX;
			}

			return result;
		}

		#endregion //Repeat

		#region Reverse

		/// <summary>
		/// Returns a <see cref="Vector"/> with the elements of <paramref name="x"/> in reverse order.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = x[N-i-1].</returns>
		/// <seealso cref="Vector.Reverse()"/>
		public static Vector Reverse(Vector x)
		{
			if (x.IsEmpty())
				return Vector.Empty;

			double[] newElements = new double[x.Length];
			x.Elements.CopyTo(newElements, 0);
			Array.Reverse(newElements);

			return new Vector(newElements, x.Dimensions);
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the elements of <paramref name="x"/> in reverse order.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector y, where y[i] = x[N-i-1].</returns>
		public static ComplexVector Reverse(ComplexVector x)
		{
			if (x.IsEmpty())
				return Vector.Empty;

			Complex[] newElements = new Complex[x.Length];
			x.Elements.CopyTo(newElements, 0);
			Array.Reverse(newElements);

			return new ComplexVector(newElements, x.Dimensions);
		}

		/// <summary>
		/// Returns a <see cref="BooleanVector"/> with the elements of <paramref name="x"/> in reverse order.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A BooleanVector y, where y[i] = x[N-i-1].</returns>
		/// <seealso cref="BooleanVector.Reverse()"/>
		public static BooleanVector Reverse(BooleanVector x)
		{
			if (x.IsEmpty())
				return BooleanVector.Empty;

			bool[] newElements = new bool[x.Length];
			x.Elements.CopyTo(newElements, 0);
			Array.Reverse(newElements);

			return new BooleanVector(newElements, x.Dimensions);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the elements of <paramref name="x"/> in reverse order along the
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y[0,...iN,...,M] = x[0,...,length-iN-1,...,M].</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Reverse(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			int newLength = x.Length;

			double curr;
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[newLength], newDimensions);
			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					subscript[Nm1] = lengthN - j - 1;
					result.Elements[Compute.SubscriptToIndex(subscript, size)] = curr;
				}

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the elements of <paramref name="x"/> in reverse order along the
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y[0,...iN,...,M] = x[0,...,length-iN-1,...,M].</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Reverse(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			int newLength = x.Length;

			Complex curr;
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[newLength], newDimensions);
			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					subscript[Nm1] = lengthN - j - 1;
					result.Elements[Compute.SubscriptToIndex(subscript, size)] = curr;
				}

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Returns a <see cref="BooleanMatrix"/> with the elements of <paramref name="x"/> in reverse order along the
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A BooleanMatrix y, where y[0,...iN,...,M] = x[0,...,length-iN-1,...,M].</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static BooleanMatrix Reverse(BooleanMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return BooleanMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			int newLength = x.Length;

			bool curr;
			int[] subscript = new int[x.Rank];
			BooleanMatrix result = new BooleanMatrix(new bool[newLength], newDimensions);
			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
					subscript[Nm1] = lengthN - j - 1;
					result.Elements[Compute.SubscriptToIndex(subscript, size)] = curr;
				}

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Reverse

		#region Switch

		/// <summary>
		/// Returns a <see cref="Vector"/> equal to <paramref name="x"/> with the dimensions switched.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector equal to x with the dimensions switched.</returns>
		/// <seealso cref="Transpose(Vector)"/>
		public static Vector Switch(Vector x)
		{
			return Compute.Transpose(x);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> equal to <paramref name="x"/> with the dimensions switched.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector equal to x with the dimensions switched.</returns>
		/// <seealso cref="Transpose(ComplexVector)"/>
		public static ComplexVector Switch(ComplexVector x)
		{
			return Compute.Transpose(x);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> equal to <paramref name="x"/> with the dimensions N1 and N2 switched.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N1">The first dimension.</param>
		/// <param name="N2">The second dimension.</param>
		/// <returns>A Matrix equal to x with the dimensions N1 and N2 switched.</returns>
		public static Matrix Switch(Matrix x, int N1, int N2)
		{
			int xRank = x.Rank;
			if (N1 < 1 || N2 < 1 || N1 > xRank || N2 > xRank)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_45"));

			if (x.IsEmpty())
				return Matrix.Empty;

			int N1m1 = N1 - 1;
			int N2m1 = N2 - 1;

			Vector size = Compute.Size(x);
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int dimTemp = newDimensions[N1m1];
			newDimensions[N1m1] = newDimensions[N2m1];
			newDimensions[N2m1] = dimTemp;

			double curr;
			int subTemp;
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(newDimensions);
			int lengthN = result.Length;
			Vector newSize = result.Size;

			for (int i = 0, length = result.Length; i < length; i++)
			{
				curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				subTemp = subscript[N1m1];
				subscript[N1m1] = subscript[N2m1];
				subscript[N2m1] = subTemp;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr;
				subTemp = subscript[N1m1];
				subscript[N1m1] = subscript[N2m1];
				subscript[N2m1] = subTemp;

				Utilities.IncrementSubscript(ref subscript, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> equal to <paramref name="x"/> with the dimensions N1 and N2 switched.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N1">The first dimension.</param>
		/// <param name="N2">The second dimension.</param>
		/// <returns>A ComplexMatrix equal to x with the dimensions N1 and N2 switched.</returns>
		public static ComplexMatrix Switch(ComplexMatrix x, int N1, int N2)
		{
			int xRank = x.Rank;
			if (N1 < 1 || N2 < 1 || N1 > xRank || N2 > xRank)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_45"));

			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			int N1m1 = N1 - 1;
			int N2m1 = N2 - 1;

			Vector size = Compute.Size(x);
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int dimTemp = newDimensions[N1m1];
			newDimensions[N1m1] = newDimensions[N2m1];
			newDimensions[N2m1] = dimTemp;

			Complex curr;
			int subTemp;
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(newDimensions);
			int lengthN = result.Length;
			Vector newSize = result.Size;

			for (int i = 0, length = result.Length; i < length; i++)
			{
				curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				subTemp = subscript[N1m1];
				subscript[N1m1] = subscript[N2m1];
				subscript[N2m1] = subTemp;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr;
				subTemp = subscript[N1m1];
				subscript[N1m1] = subscript[N2m1];
				subscript[N2m1] = subTemp;

				Utilities.IncrementSubscript(ref subscript, x.Dimensions);
			}

			return result;
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Returns a <see cref="BooleanMatrix"/> equal to <paramref name="x"/> with the dimensions N1 and N2 switched.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <param name="N1">The first dimension.</param>
		/// <param name="N2">The second dimension.</param>
		/// <returns>A BooleanMatrix equal to x with the dimensions N1 and N2 switched.</returns>
		public static BooleanMatrix Switch(BooleanMatrix x, int N1, int N2)
		{
			int xRank = x.Rank;
			if (N1 < 1 || N2 < 1 || N1 > xRank || N2 > xRank)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_45"));

			if (x.IsEmpty())
				return BooleanMatrix.Empty;

			int N1m1 = N1 - 1;
			int N2m1 = N2 - 1;

			Vector size = Compute.Size(x);
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int dimTemp = newDimensions[N1m1];
			newDimensions[N1m1] = newDimensions[N2m1];
			newDimensions[N2m1] = dimTemp;

			bool curr;
			int subTemp;
			int[] subscript = new int[x.Rank];
			BooleanMatrix result = new BooleanMatrix(newDimensions);
			int lengthN = result.Length;
			Vector newSize = result.Size;

			for (int i = 0, length = result.Length; i < length; i++)
			{
				curr = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				subTemp = subscript[N1m1];
				subscript[N1m1] = subscript[N2m1];
				subscript[N2m1] = subTemp;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr;
				subTemp = subscript[N1m1];
				subscript[N1m1] = subscript[N2m1];
				subscript[N2m1] = subTemp;

				Utilities.IncrementSubscript(ref subscript, x.Dimensions);
			}

			return result;
		}

		#endregion //Switch

		#region SetDifference

		/// <summary>
		/// Returns a <see cref="Vector"/> with the elements contained in <paramref name="x1"/> that 
		/// are not contained in <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>The elements in x1 that are not in x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetIntersection(Vector,Vector)"/>
		/// <seealso cref="SetUnion(Vector,Vector)"/>
		public static Vector SetDifference(Vector x1, Vector x2)
		{
			if (!((x1.IsRow() & x2.IsRow()) | (x1.IsColumn() & x2.IsColumn())))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_13"));

			if (x1.IsEmpty())
				return new Vector();

			if (x2.IsEmpty())
				return Compute.Unique(x1);

			Vector a = Compute.Unique(x1);
			Vector b = Compute.SetIntersection(a, x2);

			if (b.IsEmpty())
				return a;

			int length = a.Length;
			int length2 = b.Length;

			int i = 0;
			int i2 = 0;
			int next = 0;
			bool done = false;
			while (!done)
			{
				if (a.Elements[i] < b.Elements[i2])
				{
					a.Elements[next] = a.Elements[i];
					next++;
					i++;
				}
				else if (a.Elements[i] == b.Elements[i2])
				{
					i++;
					i2++;
				}

				if (i2 == length2)
					done = true;
			}

			if (next == 0)
				return new Vector();

			int[] newDimensions = new int[2];
			if (a.IsRow())
			{
				newDimensions[0] = 1;
				newDimensions[1] = next;
			}
			else
			{
				newDimensions[0] = next;
				newDimensions[1] = 1;
			}

			length = next;
			IList<double> newElements = new double[length];
			for (int k = 0; k < length; k++)
			{
				newElements[k] = a.Elements[k];
			}

			return new Vector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the elements contained in <paramref name="x1"/> that 
		/// are not contained in <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>The elements in x1 that are not in x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetIntersection(Vector,ComplexVector)"/>
		/// <seealso cref="SetUnion(Vector,ComplexVector)"/>
		public static ComplexVector SetDifference(Vector x1, ComplexVector x2)
		{
			ComplexVector x1Complex = x1;
			return Compute.SetDifference(x1Complex, x2);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the elements contained in <paramref name="x1"/> that 
		/// are not contained in <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>The elements in x1 that are not in x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetIntersection(ComplexVector,Vector)"/>
		/// <seealso cref="SetUnion(ComplexVector,Vector)"/>
		public static ComplexVector SetDifference(ComplexVector x1, Vector x2)
		{
			ComplexVector x2Complex = x2;
			return Compute.SetDifference(x1, x2Complex);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the elements contained in <paramref name="x1"/> that 
		/// are not contained in <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>The elements in x1 that are not in x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetIntersection(ComplexVector,ComplexVector)"/>
		/// <seealso cref="SetUnion(ComplexVector,ComplexVector)"/>
		public static ComplexVector SetDifference(ComplexVector x1, ComplexVector x2)
		{
			if (!((x1.IsRow() && x2.IsRow()) || (x1.IsColumn() && x2.IsColumn())))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_13"));

			if (x1.IsEmpty())
				return new ComplexVector();

			if (x2.IsEmpty())
				return Compute.Unique(x1);

			ComplexVector a = Compute.Unique(x1);
			ComplexVector b = Compute.SetIntersection(a, x2);

			if (b.IsEmpty())
				return a;

			int length = a.Length;
			int length2 = b.Length;

			int i = 0;
			int i2 = 0;
			int next = 0;
			bool done = false;
			while (!done)
			{
				if (a.Elements[i] < b.Elements[i2])
				{
					a.Elements[next] = a.Elements[i];
					next++;
					i++;
				}
				else if (a.Elements[i] == b.Elements[i2])
				{
					i++;
					i2++;
				}

				if (i2 == length2)
					done = true;
			}

			if (next == 0)
				return new ComplexVector();

			int[] newDimensions = new int[2];
			if (a.IsRow())
			{
				newDimensions[0] = 1;
				newDimensions[1] = next;
			}
			else
			{
				newDimensions[0] = next;
				newDimensions[1] = 1;
			}

			length = next;
			IList<Complex> newElements = new Complex[length];
			for (int k = 0; k < length; k++)
			{
				newElements[k] = a.Elements[k];
			}

			return new ComplexVector(newElements, newDimensions);
		}

		#endregion //SetDifference

		#region SetIntersection

		/// <summary>
		/// Returns a <see cref="Vector"/> with the elements common to both <paramref name="x1"/> and
		/// <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>The elements common to x1 and x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetDifference(Vector,Vector)"/>
		/// <seealso cref="SetUnion(Vector,Vector)"/>
		public static Vector SetIntersection(Vector x1, Vector x2)
		{
			if (!((x1.IsRow() & x2.IsRow()) | (x1.IsColumn() & x2.IsColumn())))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_13"));

			if (x1.IsEmpty() || x2.IsEmpty())
				return new Vector();

			Vector a = Compute.Unique(x1);
			Vector b = Compute.Unique(x2);
			if (a.Length < b.Length)
			{
				Vector temp = a.Clone();
				a = b;
				b = temp;
			}

			int length = a.Length;
			int length2 = b.Length;

			int i = 0;
			int i2 = 0;
			int next = 0;
			bool done = false;
			while (!done)
			{
				if (a.Elements[i] < b.Elements[i2])
				{
					i++;
				}
				else if (a.Elements[i] > b.Elements[i2])
				{
					i2++;
				}
				else if (a.Elements[i] == b.Elements[i2])
				{
					a.Elements[next] = a.Elements[i];
					next++;
					i++;
					i2++;
				}

				if (i == length || i2 == length2)
					done = true;
			}

			if (next == 0)
				return new Vector();

			int[] newDimensions = new int[2];
			if (a.IsRow())
			{
				newDimensions[0] = 1;
				newDimensions[1] = next;
			}
			else
			{
				newDimensions[0] = next;
				newDimensions[1] = 1;
			}

			length = next;
			IList<double> newElements = new double[length];
			for (int k = 0; k < length; k++)
			{
				newElements[k] = a.Elements[k];
			}

			return new Vector(newElements, newDimensions);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the elements common to both <paramref name="x1"/> and
		/// <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>The elements common to x1 and x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetDifference(Vector,ComplexVector)"/>
		/// <seealso cref="SetUnion(Vector,ComplexVector)"/>
		public static ComplexVector SetIntersection(Vector x1, ComplexVector x2)
		{
			ComplexVector x1Complex = x1;
			return Compute.SetIntersection(x1Complex, x2);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the elements common to both <paramref name="x1"/> and
		/// <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>The elements common to x1 and x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetDifference(ComplexVector,Vector)"/>
		/// <seealso cref="SetUnion(ComplexVector,Vector)"/>
		public static ComplexVector SetIntersection(ComplexVector x1, Vector x2)
		{
			ComplexVector x2Complex = x2;
			return Compute.SetIntersection(x1, x2Complex);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the elements common to both <paramref name="x1"/> and
		/// <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>The elements common to x1 and x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetDifference(ComplexVector,ComplexVector)"/>
		/// <seealso cref="SetUnion(ComplexVector,ComplexVector)"/>
		public static ComplexVector SetIntersection(ComplexVector x1, ComplexVector x2)
		{
			if (!((x1.IsRow() & x2.IsRow()) | (x1.IsColumn() & x2.IsColumn())))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_13"));

			if (x1.IsEmpty() || x2.IsEmpty())
				return new ComplexVector();

			ComplexVector a = Compute.Unique(x1);
			ComplexVector b = Compute.Unique(x2);
			if (a.Length < b.Length)
			{
				ComplexVector temp = a.Clone();
				a = b;
				b = temp;
			}

			int length = a.Length;
			int length2 = b.Length;

			int i = 0;
			int i2 = 0;
			int next = 0;
			bool done = false;
			while (!done)
			{
				if (a.Elements[i] < b.Elements[i2])
				{
					i++;
				}
				else if (a.Elements[i] > b.Elements[i2])
				{
					i2++;
				}
				else if (a.Elements[i] == b.Elements[i2])
				{
					a.Elements[next] = a.Elements[i];
					next++;
					i++;
					i2++;
				}

				if (i == length || i2 == length2)
					done = true;
			}

			if (next == 0)
				return new ComplexVector();

			int[] newDimensions = new int[2];
			if (a.IsRow())
			{
				newDimensions[0] = 1;
				newDimensions[1] = next;
			}
			else
			{
				newDimensions[0] = next;
				newDimensions[1] = 1;
			}

			length = next;
			IList<Complex> newElements = new Complex[length];
			for (int k = 0; k < length; k++)
			{
				newElements[k] = a.Elements[k];
			}

			return new ComplexVector(newElements, newDimensions);
		}

		#endregion //SetIntersection

		#region SetUnion

		/// <summary>
		/// Returns a <see cref="Vector"/> with all elements contained in either <paramref name="x1"/> or
		/// <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>The elements in either x1 or x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetDifference(Vector,Vector)"/>
		/// <seealso cref="SetIntersection(Vector,Vector)"/>
		public static Vector SetUnion(Vector x1, Vector x2)
		{
			return Compute.Unique(Compute.Append(x1, x2));
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with all elements contained in either <paramref name="x1"/> or
		/// <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">A Vector.</param>
		/// <param name="x2">A ComplexVector.</param>
		/// <returns>The elements in either x1 or x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetDifference(Vector,ComplexVector)"/>
		/// <seealso cref="SetIntersection(Vector,ComplexVector)"/>
		public static ComplexVector SetUnion(Vector x1, ComplexVector x2)
		{
			return Compute.Unique(Compute.Append(x1, x2));
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with all elements contained in either <paramref name="x1"/> or
		/// <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">A ComplexVector.</param>
		/// <param name="x2">A Vector.</param>
		/// <returns>The elements in either x1 or x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetDifference(ComplexVector,Vector)"/>
		/// <seealso cref="SetIntersection(ComplexVector,Vector)"/>
		public static ComplexVector SetUnion(ComplexVector x1, Vector x2)
		{
			return Compute.Unique(Compute.Append(x1, x2));
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with all elements contained in either <paramref name="x1"/> or
		/// <paramref name="x2"/>. The result is sorted and unique.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>The elements in either x1 or x2.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when x1 and x2 do not have the same orientation.
		/// </exception>
		/// <seealso cref="SetDifference(ComplexVector,ComplexVector)"/>
		/// <seealso cref="SetIntersection(ComplexVector,ComplexVector)"/>
		public static ComplexVector SetUnion(ComplexVector x1, ComplexVector x2)
		{
			return Compute.Unique(Compute.Append(x1, x2));
		}

		#endregion //SetUnion

		#region Sort

		/// <summary>
		/// Returns a <see cref="Vector"/> with the elements of <paramref name="x"/> sorted by value in ascending order.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector with the sorted values of x.</returns>
		/// <seealso cref="Vector.Sort()"/>
		/// <seealso cref="Unique(Vector)"/>
		public static Vector Sort(Vector x)
		{
			if (x.IsEmpty())
				return Vector.Empty;

			double[] newElements = new double[x.Elements.Count];
			x.Elements.CopyTo(newElements, 0);
			Array.Sort(newElements);

			return new Vector(newElements, x.Dimensions);
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> with the elements of <paramref name="x"/> sorted by value 
		/// in ascending order.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector with the sorted values of x.</returns>
		/// <seealso cref="ComplexVector.Sort()"/>
		/// <seealso cref="Unique(ComplexVector)"/>
		public static ComplexVector Sort(ComplexVector x)
		{
			if (x.IsEmpty())
				return ComplexVector.Empty;

			Complex[] newElements = new Complex[x.Elements.Count];
			x.Elements.CopyTo(newElements, 0);
			Array.Sort(newElements);

			return new ComplexVector(newElements, x.Dimensions);
		}

		/// <summary>
		/// Returns a <see cref="BooleanVector"/> with the elements of <paramref name="x"/> sorted by value 
		/// in ascending order.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A BooleanVector with the sorted values of x.</returns>
		/// <seealso cref="BooleanVector.Sort()"/>
		/// <seealso cref="Unique(BooleanVector)"/>
		public static BooleanVector Sort(BooleanVector x)
		{
			if (x.IsEmpty())
				return BooleanVector.Empty;

			bool[] newElements = new bool[x.Elements.Count];
			x.Elements.CopyTo(newElements, 0);
			Array.Sort(newElements);

			return new BooleanVector(newElements, x.Dimensions);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the elements of <paramref name="x"/> in sorted in increasing order along the
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y equals x sorted along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Sort(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			int newLength = x.Length;

			double[] temp = new double[lengthN];
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[newLength], newDimensions);
			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				Array.Sort(temp);
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					result.Elements[Compute.SubscriptToIndex(subscript, size)] = temp[j];
				}

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the elements of <paramref name="x"/> in sorted in increasing 
		/// order along the Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y equals x sorted along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Sort(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			int newLength = x.Length;

			Complex[] temp = new Complex[lengthN];
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[newLength], newDimensions);
			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				Array.Sort(temp);
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					result.Elements[Compute.SubscriptToIndex(subscript, size)] = temp[j];
				}

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Returns a <see cref="BooleanMatrix"/> with the elements of <paramref name="x"/> in sorted in increasing 
		/// order along the Nth dimension.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A BooleanMatrix y, where y equals x sorted along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static BooleanMatrix Sort(BooleanMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return BooleanMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			int newLength = x.Length;

			bool[] temp = new bool[lengthN];
			int[] subscript = new int[x.Rank];
			BooleanMatrix result = new BooleanMatrix(new bool[newLength], newDimensions);
			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				Array.Sort(temp);
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					result.Elements[Compute.SubscriptToIndex(subscript, size)] = temp[j];
				}

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Sort

		#region Squeeze

		/// <summary>
		/// Returns a <see cref="Matrix"/> equivalent to <paramref name="x"/> with unitary dimensions removed.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix equivalent to x with unitary dimensions removed.</returns>
		/// <seealso cref="MatrixBase.Squeeze()"/>
		public static Matrix Squeeze(Matrix x)
		{
			return (Matrix)x.Clone().Squeeze();
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> equivalent to <paramref name="x"/> with unitary dimensions removed.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix equivalent to x with unitary dimensions removed.</returns>
		/// <seealso cref="MatrixBase.Squeeze()"/>
		public static ComplexMatrix Squeeze(ComplexMatrix x)
		{
			return (ComplexMatrix)x.Clone().Squeeze();
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Returns a <see cref="BooleanMatrix"/> equivalent to <paramref name="x"/> with unitary dimensions removed.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix equivalent to x with unitary dimensions removed.</returns>
		/// <seealso cref="MatrixBase.Squeeze()"/>
		public static BooleanMatrix Squeeze(BooleanMatrix x)
		{
			return (BooleanMatrix)x.Clone().Squeeze();
		}

		#endregion //Squeeze

		#region SubscriptToIndex

		/// <summary>
		/// Returns the one-dimensional index that corresponds with the <see cref="Matrix"/> <paramref name="subscript"/>.
		/// </summary>
		/// <param name="subscript">The Matrix subscript.</param>
		/// <param name="size">The dimensions of a Matrix.</param>
		/// <returns>The one-dimensional index represented by the subscript.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when size is empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the number of <paramref name="subscript"/> elements does not match the specified <paramref name="size"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when a subscript is out of the range specified by the dimensions.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when a dimensions is less than 1.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the size Vector has non-integer elements.
		/// </exception>
		public static int SubscriptToIndex(int[] subscript, Vector size)
		{
			if (size.IsEmpty())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_35"));

			//Exception Handling.
			if (subscript.Length != size.Length)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_46"));

			//Exception handling.
			int currDimension;
			double currDimensionCheck;
			int currIndex;
			for (int i = 0; i < subscript.Length; i++)
			{
				currIndex = subscript[i];
				currDimensionCheck = size.Elements[i];
				currDimension = (int)Compute.Round(currDimensionCheck);

				if (currDimensionCheck != currDimension)
					throw new ArgumentOutOfRangeException("size", Compute.GetString("LE_ArgumentOutOfRangeException_12"));

				if (currDimension < 1)
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_37"), "size");

				if (currIndex < 0 || currIndex >= currDimension)
					throw new ArgumentOutOfRangeException("subscript", Compute.GetString("LE_ArgumentOutOfRangeException_9"));
			}

			//Calculate the array index.
			int index = subscript[0];
			Vector dimensionProduct = Compute.CumProduct(size);
			for (int i = 1, length = size.Length; i < length; i++)
			{
				index += subscript[i] * (int)Compute.Round(dimensionProduct.Elements[i - 1]);
			}
			//Return the array index.
			return index;
		}

		internal static int SubscriptToIndex(int[] dimensions, params int[] subscript)
		{
			if (dimensions.Length == 0)
			{
				Utilities.DebugFail("Invalid dimensions.");
				return 0;
			}

			if (subscript.Length != dimensions.Length)
			{
				Utilities.DebugFail("Invalid dimensions.");
				return 0;
			}

			int currDimension;
			int currIndex;
			for (int i = 0; i < subscript.Length; i++)
			{
				currIndex = subscript[i];
				currDimension = dimensions[i];

				if (currDimension < 1)
				{
					Utilities.DebugFail("Invalid dimensions.");
					return 0;
				}

				if (currIndex < 0 || currIndex >= currDimension)
				{
					Utilities.DebugFail("Invalid dimensions.");
					return 0;
				}
			}

			int index = subscript[0];

			int[] cumProduct = new int[dimensions.Length];
			int product = 1;
			for (int i = 0; i < dimensions.Length; i++)
			{
				product *= dimensions[i];
				cumProduct[i] = product;
			}

			for (int i = 1, length = dimensions.Length; i < length; i++)
				index += subscript[i] * cumProduct[i - 1];

			return index;
		}

		#endregion //SubscriptToIndex

		#region Sum

		/// <summary>
		/// Returns the sum of the elements of a <see cref="Vector"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A double y, where y = x[0]+...+x[N].</returns>
		/// <seealso cref="Product(Vector)"/>
		public static double Sum(Vector x)
		{
			if (x.IsEmpty())
				return 0;

			double value = 0;
			for (int i = 0, length = x.Length; i < length; i++)
			{
				value += x.Elements[i];
			}
			return value;
		}

		/// <summary>
		/// Returns the sum of the elements of a <see cref="ComplexVector"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A <see cref="Complex"/> number y, where y = x[0]+...+x[N].</returns>
		/// <seealso cref="Product(ComplexVector)"/>
		public static Complex Sum(ComplexVector x)
		{
			if (x.IsEmpty())
				return 0;

			Complex value = 0;
			for (int i = 0, length = x.Length; i < length; i++)
			{
				value += x.Elements[i];
			}
			return value;
		}

		/// <summary>
		/// Returns the sum of the elements of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The sum of the elements of x.</returns>
		public static double Sum(Matrix x)
		{
			return Compute.Sum(new Vector(x.Elements));
		}

		/// <summary>
		/// Returns the sum of the elements of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The sum of the elements of x.</returns>
		public static Complex Sum(ComplexMatrix x)
		{
			return Compute.Sum(new ComplexVector(x.Elements));
		}

		/// <summary>
		/// Returns the sum of the elements of the <see cref="Matrix"/> <paramref name="x"/> along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix with the sum of the elements of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Sum(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] = 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;

			double curr;
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				curr = 0;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr += x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr;
				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns the sum of the elements of the <see cref="ComplexMatrix"/> <paramref name="x"/> along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix with the sum of the elements of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Sum(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArithmeticException_4"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			Vector size = Compute.Size(x);
			Vector newSize = size.Clone();
			newSize.Elements[Nm1] = 1;

			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;

			Complex curr;
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[Utilities.Product(newDimensions)], newDimensions);
			for (int i = 0, length = result.Length; i < length; i++)
			{
				curr = 0;
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					curr += x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = curr;
				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Sum

		#region Trace

		/// <summary>
		/// Returns the trace of the <see cref="Matrix"/> <paramref name="x"/>. The trace is the sum of the elements of x
		/// along the main diagonal.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The trace of x.</returns>
		/// <seealso cref="Compute.Diagonal(Matrix)"/>
		public static double Trace(Matrix x)
		{
			if (x.IsEmpty())
				return 0;

			return Compute.Sum(Compute.Diagonal(x));
		}

		/// <summary>
		/// Returns the trace of the <see cref="ComplexMatrix"/> <paramref name="x"/>. The trace is the sum of the 
		/// elements of x along the main diagonal.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The trace of x.</returns>
		/// <seealso cref="Compute.Diagonal(ComplexMatrix)"/>
		public static Complex Trace(ComplexMatrix x)
		{
			if (x.IsEmpty())
				return 0;

			return Compute.Sum(Compute.Diagonal(x));
		}

		#endregion //Trace

		#region Transpose

		/// <summary>
		/// Returns a <see cref="Vector"/> identical to <paramref name="x"/> but with the opposite orientation.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>
		/// Returns the equivalent row Vector if x is a column Vector.
		/// Returns the equivalent column Vector if x is a row Vector.
		/// </returns>
		/// <seealso cref="Vector.Transpose()"/>
		public static Vector Transpose(Vector x)
		{
			return x.Clone().Transpose();
		}

		/// <summary>
		/// Returns a <see cref="ComplexVector"/> identical to <paramref name="x"/> but with the opposite orientation.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>
		/// Returns the equivalent row ComplexVector if x is a column ComplexVector.
		/// Returns the equivalent column ComplexVector if x is a row ComplexVector.
		/// </returns>
		/// <seealso cref="ComplexVector.Transpose()"/>
		public static ComplexVector Transpose(ComplexVector x)
		{
			return x.Clone().Transpose();
		}

		/// <summary>
		/// Returns a <see cref="BooleanVector"/> identical to <paramref name="x"/> but with the opposite orientation.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>
		/// Returns the equivalent row BooleanVector if x is a column BooleanVector.
		/// Returns the equivalent column BooleanVector if x is a row BooleanVector.
		/// </returns>
		/// <seealso cref="BooleanVector.Transpose()"/>
		public static BooleanVector Transpose(BooleanVector x)
		{
			return x.Clone().Transpose();
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> equal to <paramref name="x"/> with the first and second dimensions switched.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>A Matrix equal to x with the first and second dimensions switched.</returns>
		public static Matrix Transpose(Matrix x)
		{
			return Compute.Switch(x, 1, 2);
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> equal to <paramref name="x"/> with the first and second dimensions switched.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>A ComplexMatrix equal to x with the first and second dimensions switched.</returns>
		public static ComplexMatrix Transpose(ComplexMatrix x)
		{
			return Compute.Switch(x, 1, 2);
		}

		// MD 4/19/11 - TFS72396
		/// <summary>
		/// Returns a <see cref="BooleanMatrix"/> equal to <paramref name="x"/> with the first and second dimensions switched.
		/// </summary>
		/// <param name="x">A BooleanMatrix.</param>
		/// <returns>A BooleanMatrix equal to x with the first and second dimensions switched.</returns>
		public static BooleanMatrix Transpose(BooleanMatrix x)
		{
			return Compute.Switch(x, 1, 2);
		}

		#endregion //Transpose

		#region Unique

		/// <summary>
		/// Returns a new <see cref="Vector"/> with the unique elements of <paramref name="x"/>, sorted in ascending order.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>A Vector with the unique elements of x.</returns>
		/// <seealso cref="Sort(Vector)"/>
		public static Vector Unique(Vector x)
		{
			if (x.IsEmpty())
				return new Vector();

			int length = x.Length;
			if (length == 1)
				return x;

			int next = 1;
			Vector result = Compute.Sort(x);
			for (int i = 1; i < length; i++)
			{
				if (result.Elements[i - 1] != result.Elements[i])
				{
					if (next != i)
						result.Elements[next] = result.Elements[i];

					next++;
				}
			}

			int length2 = next;
			if (length == length2)
			{
				return result;
			}
			else
			{
				length = length2;
				int[] newDimensions;
				if (result.IsRow())
					newDimensions = new int[] { 1, length };
				else
					newDimensions = new int[] { length, 1 };

				IList<double> newElements = new double[length];
				for (int i = 0; i < length; i++)
				{
					newElements[i] = result.Elements[i];
				}

				return new Vector(newElements, newDimensions);
			}
		}

		/// <summary>
		/// Returns a new <see cref="ComplexVector"/> with the unique elements of <paramref name="x"/>, sorted 
		/// in ascending order.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>A ComplexVector with the unique elements of x.</returns>
		/// <seealso cref="Sort(ComplexVector)"/>
		public static ComplexVector Unique(ComplexVector x)
		{
			if (x.IsEmpty())
				return new ComplexVector();

			int length = x.Length;
			if (length == 1)
				return x;

			int next = 1;
			ComplexVector result = Compute.Sort(x);
			for (int i = 1; i < length; i++)
			{
				if (result.Elements[i - 1] != result.Elements[i])
				{
					if (next != i)
						result.Elements[next] = result.Elements[i];

					next++;
				}
			}

			int length2 = next;
			if (length == length2)
			{
				return result;
			}
			else
			{
				length = length2;
				int[] newDimensions;
				if (result.IsRow())
					newDimensions = new int[] { 1, length };
				else
					newDimensions = new int[] { length, 1 };

				IList<Complex> newElements = new Complex[length];
				for (int i = 0; i < length; i++)
				{
					newElements[i] = result.Elements[i];
				}

				return new ComplexVector(newElements, newDimensions);
			}
		}

		/// <summary>
		/// Returns a new <see cref="BooleanVector"/> with the unique elements of <paramref name="x"/>, sorted 
		/// in ascending order.
		/// </summary>
		/// <param name="x">A BooleanVector.</param>
		/// <returns>A BooleanVector with the unique elements of x.</returns>
		/// <seealso cref="Sort(BooleanVector)"/>
		public static BooleanVector Unique(BooleanVector x)
		{
			if (x.IsEmpty())
				return new BooleanVector();

			int length = x.Length;
			if (length == 1)
				return x;

			int next = 1;
			BooleanVector result = Compute.Sort(x);
			for (int i = 1; i < length; i++)
			{
				if (result.Elements[i - 1] != result.Elements[i])
				{
					if (next != i)
						result.Elements[next] = result.Elements[i];

					next++;
				}
			}

			int length2 = next;
			if (length == length2)
			{
				return result;
			}
			else
			{
				length = length2;
				int[] newDimensions;
				if (result.IsRow())
					newDimensions = new int[] { 1, length };
				else
					newDimensions = new int[] { length, 1 };

				IList<bool> newElements = new bool[length];
				for (int i = 0; i < length; i++)
				{
					newElements[i] = result.Elements[i];
				}

				return new BooleanVector(newElements, newDimensions);
			}
		}

		#endregion //Unique

		#region VectorProduct

		/// <summary>
		/// Returns the vector product, also known as the dot product or inner product, of two <see cref="Vector"/> instances.
		/// </summary>
		/// <param name="x1">The first Vector.</param>
		/// <param name="x2">The second Vector.</param>
		/// <returns>The sum of the products of corresponding entries in <paramref name="x1"/> and <paramref name="x2"/>.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the length of x1 does not equal the length of x2.
		/// </exception>
		public static double VectorProduct(Vector x1, Vector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return 0;

			if (x1.Length != x2.Length)
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_14"));

			double result = 0;
			for (int i = 0, length = x1.Length; i < length; i++)
				result += x1.Elements[i] * x2.Elements[i];

			return result;
		}

		/// <summary>
		/// Returns the vector product, also known as the dot product or inner product, of two <see cref="ComplexVector"/> instances.
		/// </summary>
		/// <param name="x1">The first ComplexVector.</param>
		/// <param name="x2">The second ComplexVector.</param>
		/// <returns>The sum of the products of corresponding entries in <paramref name="x1"/> and <paramref name="x2"/>.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs when the length of x1 does not equal the length of x2.
		/// </exception>
		public static Complex VectorProduct(ComplexVector x1, ComplexVector x2)
		{
			if (x1.IsEmpty() && x2.IsEmpty())
				return 0;

			if (x1.Length != x2.Length)
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_14"));

			Complex result = 0;
			for (int i = 0, length = x1.Length; i < length; i++)
				result += x1.Elements[i] * x2.Elements[i];

			return result;
		}

		#endregion //VectorProduct

		#region Zeros

		/// <summary>
		/// Returns a row <see cref="Vector"/> with <paramref name="N"/> zeros.
		/// </summary>
		/// <param name="N">The length of the Vector.</param>
		/// <returns>A row zero Vector.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than zero.
		/// </exception>
		public static Vector Zeros(int N)
		{
			if (N < 0)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_47"), "N");

			if (N == 0)
				return Vector.Empty;

			return new Vector(N);
		}

		/// <summary>
		/// Returns a zero <see cref="Matrix"/> the specified <paramref name="size"/>.
		/// </summary>
		/// <param name="size">The size of the constructed Matrix.</param>
		/// <returns>A zero Matrix.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when size has elements that are less than or equal to zero.
		/// </exception>
		public static Matrix Zeros(params int[] size)
		{
			int curr;
			int length = 1;
			int zeroSize = 0;
			for (int i = 0; i < size.Length; i++)
			{ 
				curr = size[i];
				if (curr > 0)
					length *= curr;
				else if (curr == 0)
					zeroSize++;
				else
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_49"));
			}

			if (zeroSize > 0)
			{
				if (zeroSize == size.Length)
					return Matrix.Empty;
				else
					throw new ArgumentException(Compute.GetString("LE_ArgumentException_49"));
			}

			int[] newDimensions;
			double[] newElements = new double[length];
			if (size.Length > 1)
			{
				newDimensions = new int[size.Length];
				size.CopyTo(newDimensions, 0);
			}
			else
			{
				newDimensions = new int[] { 1, size.Length };
			}

			return new Matrix(newElements, newDimensions);
		}

		#endregion //Zeros

		#endregion //Vector and Matrix Functions

		#region Statistics

		#region Correlation

		/// <summary>
		/// Returns the Pearson's correlation coefficient for two <see cref="Vector"/> instances.
		/// </summary>
		/// <param name="x1">The first Vector of observations.</param>
		/// <param name="x2">The second Vector of observations.</param>
		/// <returns>The correlation of <paramref name="x1"/> and <paramref name="x2"/>.</returns>
		/// <seealso cref="Covariance(Vector,Vector)"/>
		public static double Correlation(Vector x1, Vector x2)
		{
			return Compute.Covariance(x1, x2) / (Compute.StandardDeviation(x1) * Compute.StandardDeviation(x2));
		}

		/// <summary>
		/// Returns the N x N correlation <see cref="Matrix"/> of an N x M Matrix <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A two-dimensional Matrix.</param>
		/// <returns>The correlation Matrix of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when x is not two-dimensional.
		/// </exception>
		public static Matrix Correlation(Matrix x)
		{
			if (!x.IsTwoDimensional())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_49"));

			if (x.IsEmpty())
				return Matrix.Empty;

			int length1 = x.Dimensions[0];
			int length2 = x.Dimensions[1];
			Vector curr;
			Vector index = Compute.Index(length2 - 1);
			Matrix result = new Matrix(0, new int[] { length1, length1 });
			for (int i = 0; i < length1; i++)
			{
				curr = (Vector)x[i, index];
				for (int j = 0; j < length1; j++)
				{
					result[i, j] = Compute.Correlation(curr, (Vector)x[j, index]);
				}
			}

			return result;
		}

		#endregion //Correlation

		#region Covariance

		/// <summary>
		/// Returns the sample covariance of two <see cref="Vector"/> instances.
		/// </summary>
		/// <param name="x1">The first Vector of observations.</param>
		/// <param name="x2">The second Vector of observations.</param>
		/// <returns>The covariance of <paramref name="x1"/> and <paramref name="x2"/>.</returns>
		/// <seealso cref="Correlation(Vector,Vector)"/>
		public static double Covariance(Vector x1, Vector x2)
		{
			return Covariance(x1,x2,StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the covariance of two <see cref="Vector"/> instances.
		/// </summary>
		/// <param name="x1">The first Vector of observations.</param>
		/// <param name="x2">The second Vector of observations.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The covariance of <paramref name="x1"/> and <paramref name="x2"/>.</returns>
		public static double Covariance(Vector x1, Vector x2, StatisticsType stat)
		{
			if(stat == StatisticsType.Population)
				return (Compute.VectorProduct(x1-Compute.Mean(x1), x2-Compute.Mean(x2)) / x1.Length);

			return (Compute.VectorProduct(x1-Compute.Mean(x1), x2-Compute.Mean(x2)) / (x1.Length - 1));
		}

		/// <summary>
		/// Returns the N x N covariance <see cref="Matrix"/> of an N x M Matrix <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A two-dimensional Matrix.</param>
		/// <returns>The covariance Matrix of x.</returns>
		public static Matrix Covariance(Matrix x)
		{
			return Compute.Covariance(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the N x N covariance <see cref="Matrix"/> of an N x M Matrix <paramref name="x"/>. 
		/// </summary>
		/// <param name="x">A two-dimensional Matrix.</param>
		/// <param name="stat">The type of statistic.</param>
		/// <returns>The covariance Matrix of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when x is not two-dimensional.
		/// </exception>
		public static Matrix Covariance(Matrix x, StatisticsType stat)
		{
			if (!x.IsTwoDimensional())
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_51"));

			if (x.IsEmpty())
				return Matrix.Empty;

			int length1 = x.Dimensions[0];
			int length2 = x.Dimensions[1];
			Vector curr;
			Vector index = Compute.Index(length2-1);
			Matrix result = new Matrix(0, new int[]{ length1, length1 });
			for (int i = 0; i < length1; i++)
			{
				curr = (Vector)x[i, index];
				for (int j = 0; j < length1; j++)
				{
					result[i, j] = Compute.Covariance(curr, (Vector)x[j, index], stat);
				}
			}

			return result;
		}

		#endregion //Covariance

		#region Histogram

		/// <summary>
		/// Returns a histogram of the data in the <see cref="Vector"/> <paramref name="x"/> using <paramref name="N"/>
		/// evenly-spaced bins.
		/// </summary>
		/// <param name="x">A Vector of data.</param>
		/// <param name="N">The number of evenly-spaced bins.</param>
		/// <returns>A Vector containing the histogram of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than one.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		/// <seealso cref="Compute.Bin(Vector,int)"/>
		public static Vector Histogram(Vector x, int N)
		{
			if (x.IsEmpty())
				throw new ArgumentException("Cannot compute the histogram of the empty Vector.", "x");

			double index;
			Vector xBinned = Compute.Bin(x, N);
			Vector hist = new Vector(N);
			for (int i = 0, length = x.Length; i < length; i++)
			{ 
				index = xBinned.Elements[i];
				if (!Compute.IsNaN(index))
					hist.Elements[(int)Compute.Round(index)]++;
			}

			return hist;
		}

		/// <summary>
		/// Returns a histogram of the data in the <see cref="Vector"/> <paramref name="x"/> using <paramref name="N"/>
		/// evenly-spaced bins.
		/// </summary>
		/// <param name="x">A Vector of data.</param>
		/// <param name="N">The number of evenly-spaced bins.</param>
		/// <param name="axis">The bin centers that correspond to the values of the histogram.</param>
		/// <returns>A Vector containing the histogram of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when N is less than one.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		/// <seealso cref="Compute.Bin(Vector,int)"/>
		public static Vector Histogram(Vector x, int N, out Vector axis)
		{
			if (x.IsEmpty())
				throw new ArgumentException("Cannot compute the histogram of the empty Vector.", "x");

			double min = Compute.Min(x);
			double max = Compute.Max(x);
			if (min == max)
				axis = Compute.Line(min - 1, max + 1, N);
			else
				axis = Compute.Line(min, max, N);
			return Compute.Histogram(x,N);
		}

		/// <summary>
		/// Returns a histogram of the data in the <see cref="Vector"/> <paramref name="x"/> by binning the data using
		/// a sequence of bin <paramref name="edges"/>.
		/// </summary>
		/// <param name="x">A Vector of data.</param>
		/// <param name="edges">edges[n] and edges[n+1] are the edges of the nth bin.</param>
		/// <returns>A Vector containing the histogram of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when there are less than two edges.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		/// <seealso cref="Compute.Bin(Vector,Vector)"/>
		public static Vector Histogram(Vector x, Vector edges)
		{
			if (x.IsEmpty())
				throw new ArgumentException("Cannot compute the histogram of the empty Vector.", "x");

			int N = edges.Length;
			if (N < 2)
				throw new ArgumentException("Bins cannot be specified with less than two elements.", "edges");

			double index;
			Vector xBinned = Compute.Bin(x, edges);
			Vector hist = new Vector(N-1);
			for (int i = 0, length = x.Length; i < length; i++)
			{
				index = xBinned.Elements[i];
				if (!Compute.IsNaN(index))
					hist.Elements[(int)Compute.Round(index)]++;
			}

			return hist;
		}

		/// <summary>
		/// Returns a histogram of the data in the <see cref="Vector"/> <paramref name="x"/> by binning the data using
		/// a sequence of bin <paramref name="edges"/>.
		/// </summary>
		/// <param name="x">A Vector of data.</param>
		/// <param name="edges">edges[n] and edges[n+1] are the edges of the nth bin.</param>
		/// <param name="axis">The bin centers that correspond to the values of the histogram.</param>
		/// <returns>A Vector containing the histogram of x.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when there are less than two edges.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs if x is empty.
		/// </exception>
		/// <seealso cref="Compute.Bin(Vector,Vector)"/>
		public static Vector Histogram(Vector x, Vector edges, out Vector axis)
		{
			if (x.IsEmpty())
				throw new ArgumentException("Cannot compute the histogram of the empty Vector.", "x");

			int N = edges.Length;
			if (N < 2)
				throw new ArgumentException("Bins cannot be specified with less than two elements.", "edges");

			axis = new Vector(N - 1);
			for (int i = 1; i < N; i++)
			{
				axis[i - 1] = (edges[i - 1] + edges[i]) / 2;
			}

			return Compute.Histogram(x, edges);
		}

		#endregion //Histogram

		#region Mean

		/// <summary>
		/// Returns the mean value of a <see cref="Vector"/> of observations.
		/// </summary>
		/// <param name="x">A Vector of observations.</param>
		/// <returns>The mean of <paramref name="x"/>.</returns>
		/// <seealso cref="Median(Vector)"/>
		/// <seealso cref="Variance(Vector)"/>
		/// <seealso cref="StandardDeviation(Vector)"/>
		/// <seealso cref="StandardError(Vector)"/>
		public static double Mean(Vector x)
		{
			if (x.IsEmpty())
				return 0;

			return Compute.Sum(x) / x.Length;
		}

		/// <summary>
		/// Returns the mean value of a <see cref="ComplexVector"/> of observations.
		/// </summary>
		/// <param name="x">A ComplexVector of observations.</param>
		/// <returns>The mean of <paramref name="x"/>.</returns>
		/// <seealso cref="Median(ComplexVector)"/>
		/// <seealso cref="Variance(ComplexVector)"/>
		/// <seealso cref="StandardDeviation(ComplexVector)"/>
		/// <seealso cref="StandardError(ComplexVector)"/>
		public static Complex Mean(ComplexVector x)
		{
			if (x.IsEmpty())
				return 0;

			return Compute.Sum(x) / x.Length;
		}

		/// <summary>
		/// Returns the mean value of a <see cref="Matrix"/> of observations.
		/// </summary>
		/// <param name="x">A Matrix of observations.</param>
		/// <returns>The mean of <paramref name="x"/>.</returns>
		public static double Mean(Matrix x)
		{
			if (x.IsEmpty())
				return 0;

			return Compute.Sum(x) / x.Length;
		}

		/// <summary>
		/// Returns the mean value of a <see cref="ComplexMatrix"/> of observations.
		/// </summary>
		/// <param name="x">A ComplexMatrix of observations.</param>
		/// <returns>The mean of <paramref name="x"/>.</returns>
		public static Complex Mean(ComplexMatrix x)
		{
			if (x.IsEmpty())
				return 0;

			return Compute.Sum(x) / x.Length;
		}

		/// <summary>
		/// Returns the mean value of a <see cref="Matrix"/> of observations along the Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix of observations.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>The mean of <paramref name="x"/> along the Nth dimension.</returns>
		public static Matrix Mean(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException("The calculation dimension number must be greater than 0.");

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			return Compute.Sum(x,N) / x.Dimensions[N - 1];
		}

		/// <summary>
		/// Returns the mean value of a <see cref="ComplexMatrix"/> of observations along the Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix of observations.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>The mean of <paramref name="x"/> along the Nth dimension.</returns>
		public static ComplexMatrix Mean(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException("The calculation dimension number must be greater than 0.");

			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			return Compute.Sum(x, N) / x.Dimensions[N - 1];
		}

		#endregion //Mean

		#region Median

		/// <summary>
		/// Returns the median value of a <see cref="Vector"/> of observations.
		/// </summary>
		/// <param name="x">A Vector of observations.</param>
		/// <returns>The median of <paramref name="x"/>.</returns>
		/// <seealso cref="Sort(Vector)"/>
		/// <seealso cref="Mean(Vector)"/>
		/// <seealso cref="Variance(Vector)"/>
		/// <seealso cref="StandardDeviation(Vector)"/>
		/// <seealso cref="StandardError(Vector)"/>
		public static double Median(Vector x)
		{
			if (x.IsEmpty())
				return Constant.NaN;

			int length = x.Length;
			int ind = x.Length / 2;
			Vector s = Compute.Sort(x);

			if (length % 2 == 1)
			{
				return s[ind];
			}
			
			return (s[ind - 1] + s[ind]) / 2;
		}

		/// <summary>
		/// Returns the median value of a <see cref="ComplexVector"/> of observations.
		/// </summary>
		/// <param name="x">A ComplexVector of observations.</param>
		/// <returns>The median of <paramref name="x"/>.</returns>
		/// <seealso cref="Sort(ComplexVector)"/>
		/// <seealso cref="Mean(ComplexVector)"/>
		/// <seealso cref="Variance(ComplexVector)"/>
		/// <seealso cref="StandardDeviation(ComplexVector)"/>
		/// <seealso cref="StandardError(ComplexVector)"/>
		public static Complex Median(ComplexVector x)
		{
			if (x.IsEmpty())
				return Constant.NaN;

			int length = x.Length;
			int ind = x.Length / 2;
			ComplexVector s = Compute.Sort(x);

			if (length % 2 == 1)
			{
				return s[ind];
			}

			return (s[ind - 1] + s[ind]) / 2;
		}

		/// <summary>
		/// Returns the median value of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The median value of x.</returns>
		public static Matrix Median(Matrix x)
		{
			return Compute.Median(new Vector(x.Elements, x.Dimensions));
		}

		/// <summary>
		/// Returns the median value of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The median value of x.</returns>
		public static ComplexMatrix Median(ComplexMatrix x)
		{
			return Compute.Median(new ComplexVector(x.Elements, x.Dimensions));
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the median of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y equals the median of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Median(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			double median;
			Vector temp = new Vector(new double[lengthN]);
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				median = Compute.Median(temp);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = median;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the median of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y equals the median of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Median(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			Complex median;
			ComplexVector temp = new ComplexVector(new Complex[lengthN]);
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				median = Compute.Median(temp);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = median;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Median

		#region Mode

		/// <summary>
		/// Returns the mode of the elements of a <see cref="Vector"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <returns>The element that occurs most frequently in x.</returns>
		/// <remarks>
		/// In the event of a tie, the greatest mode is returned.
		/// </remarks>
		public static double Mode(Vector x)
		{
			if (x.IsEmpty())
				return Constant.NaN;

			int length = x.Length;
			Vector s = Compute.Sort(x);
			double result = s.Elements[0];
			double last = result;
			double curr;
			int count = 1;
			int maxCount = 1;

			for (int i = 1; i < length; i++)
			{ 
				curr = s.Elements[i];
				if (curr == last)
				{
					count++;
				}
				else 
				{
					if (count >= maxCount)
					{
						maxCount = count;
						result = last;
					}
					count = 1;
				}

				last = curr;
			}
			if (count >= maxCount)
			{
				result = last;
			}

			return result;
		}

		/// <summary>
		/// Returns the mode of the elements of a <see cref="ComplexVector"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <returns>The element that occurs most frequently in x.</returns>
		/// <remarks>
		/// In the event of a tie, the greatest mode is returned.
		/// </remarks>
		public static Complex Mode(ComplexVector x)
		{
			if (x.IsEmpty())
				return Constant.NaN;

			int length = x.Length;
			ComplexVector s = Compute.Sort(x);
			Complex result = s.Elements[0];
			Complex last = result;
			Complex curr;
			int count = 1;
			int maxCount = 1;

			for (int i = 1; i < length; i++)
			{
				curr = s.Elements[i];
				if (curr == last)
				{
					count++;
				}
				else
				{
					if (count >= maxCount)
					{
						maxCount = count;
						result = last;
					}
					count = 1;
				}

				last = curr;
			}
			if (count >= maxCount)
			{
				result = last;
			}

			return result;
		}

		/// <summary>
		/// Returns the mode of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The mode of x.</returns>
		/// <remarks>
		/// In the event of a tie, the greatest mode is returned.
		/// </remarks>
		public static Matrix Mode(Matrix x)
		{
			return Compute.Mode(new Vector(x.Elements, x.Dimensions));
		}

		/// <summary>
		/// Returns the mode of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The mode of x.</returns>
		/// <remarks>
		/// In the event of a tie, the greatest mode is returned.
		/// </remarks>
		public static ComplexMatrix Mode(ComplexMatrix x)
		{
			return Compute.Mode(new ComplexVector(x.Elements, x.Dimensions));
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the mode of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y equals the mode of x along the Nth dimension.</returns>
		/// <remarks>
		/// In the event of a tie, the greatest mode is used.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Mode(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			double mode;
			Vector temp = new Vector(new double[lengthN]);
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				mode = Compute.Mode(temp);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = mode;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the mode of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y equals the mode of x along the Nth dimension.</returns>
		/// <remarks>
		/// In the event of a tie, the greatest mode is used.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Mode(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			Complex mode;
			ComplexVector temp = new ComplexVector(new Complex[lengthN]);
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				mode = Compute.Mode(temp);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = mode;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Mode

		#region NormalDistribution

		/// <summary>
		/// Returns the normal distribution of <paramref name="x"/> with mean <paramref name="mu"/> and 
		/// standard deviation <paramref name="sigma"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <param name="mu">The mean of the distribution.</param>
		/// <param name="sigma">The standard deviation of the distribution.</param>
		/// <returns>The normal distrubition x.</returns>
		public static double NormalDistribution(double x, double mu, double sigma)
		{
			if (sigma < 0)
				throw new ArithmeticException(Compute.GetString("LE_ArgumentException_52"));

			if (sigma == 0)
				return mu;

			return (1 / Compute.Sqrt(2 * System.Math.PI * sigma)) * Compute.Exp((-1 * (Compute.Pow(x - mu, 2))) / (2 * Compute.Pow(sigma, 2)));
		}

		/// <summary>
		/// Returns a <see cref="Vector"/> with the value of the normal distribuion at each element of 
		/// <paramref name="x"/>. The distribution has mean <paramref name="mu"/> and standard deviation 
		/// <paramref name="sigma"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="mu">The mean of the distribution.</param>
		/// <param name="sigma">The standard deviation of the distribution.</param>
		/// <returns>The normal distrubition x.</returns>
		/// <seealso cref="NormalDistribution(double,double,double)"/>
		public static Vector NormalDistribution(Vector x, double mu, double sigma)
		{
			if (x.IsEmpty())
				return Vector.Empty;

			Vector result = x.Clone();
			for (int i = 0; i < result.Length; i++)
			{
				result.Elements[i] = Compute.NormalDistribution(result.Elements[i], mu, sigma);
			}
			return result;
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the value of the normal distribuion at each element of 
		/// <paramref name="x"/>. The distribution has mean <paramref name="mu"/> and standard deviation 
		/// <paramref name="sigma"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="mu">The mean of the distribution.</param>
		/// <param name="sigma">The standard deviation of the distribution.</param>
		/// <returns>The normal distrubition x.</returns>
		/// <seealso cref="NormalDistribution(double,double,double)"/>
		public static Matrix NormalDistribution(Matrix x, double mu, double sigma)
		{
			if (x.IsEmpty())
				return Matrix.Empty;

			Matrix result = x.Clone();
			for (int i = 0; i < result.Length; i++)
			{
				result.Elements[i] = Compute.NormalDistribution(result.Elements[i], mu, sigma);
			}
			return result;
		}

		#endregion //NormalDistribution

		#region StandardDeviation

		/// <summary>
		/// Returns the standard deviation of a <see cref="Vector"/> of observations.
		/// </summary>
		/// <param name="x">A Vector of observations.</param>
		/// <returns>The standard deviation of <paramref name="x"/>.</returns>
		/// <seealso cref="Mean(Vector)"/>
		/// <seealso cref="Median(Vector)"/>
		/// <seealso cref="Variance(Vector)"/>
		/// <seealso cref="StandardError(Vector)"/>
		public static double StandardDeviation(Vector x)
		{
			return StandardDeviation(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the standard deviation of a <see cref="ComplexVector"/> of observations.
		/// </summary>
		/// <param name="x">A ComplexVector of observations.</param>
		/// <returns>The standard deviation of <paramref name="x"/>.</returns>
		/// <seealso cref="Mean(ComplexVector)"/>
		/// <seealso cref="Median(ComplexVector)"/>
		/// <seealso cref="Variance(ComplexVector)"/>
		/// <seealso cref="StandardError(Vector)"/>
		public static Complex StandardDeviation(ComplexVector x)
		{
			return StandardDeviation(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the standard deviation of a <see cref="Vector"/> of observations.
		/// </summary>
		/// <param name="x">A Vector of observations.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The standard deviation of <paramref name="x"/>.</returns>
		/// <seealso cref="Variance(Vector,StatisticsType)"/>
		public static double StandardDeviation(Vector x, StatisticsType stat)
		{
			return Compute.Sqrt(Compute.Variance(x, stat));
		}

		/// <summary>
		/// Returns the standard deviation of a <see cref="ComplexVector"/> of observations.
		/// </summary>
		/// <param name="x">A ComplexVector of observations.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The standard deviation of <paramref name="x"/>.</returns>
		/// <seealso cref="Variance(ComplexVector,StatisticsType)"/>
		public static Complex StandardDeviation(ComplexVector x, StatisticsType stat)
		{
			return Compute.Sqrt(Compute.Variance(x, stat));
		}

		/// <summary>
		/// Returns the sample standard deviation of all the elements of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The sample standard deviation of x.</returns>
		public static double StandardDeviation(Matrix x)
		{
			return Compute.StandardDeviation(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the sample standard deviation of all the elements of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The sample standard deviation of x.</returns>
		public static Complex StandardDeviation(ComplexMatrix x)
		{
			return Compute.StandardDeviation(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the standard deviation of all the elements of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The standard deviation of x.</returns>
		public static double StandardDeviation(Matrix x, StatisticsType stat)
		{
			return Compute.StandardDeviation(new Vector(x.Elements, x.Dimensions), stat);
		}

		/// <summary>
		/// Returns the standard deviation of all the elements of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The standard deviation of x.</returns>
		public static Complex StandardDeviation(ComplexMatrix x, StatisticsType stat)
		{
			return Compute.StandardDeviation(new ComplexVector(x.Elements, x.Dimensions), stat);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the sample standard deviation of <paramref name="x"/>  along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y equals the standard deviation of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix StandardDeviation(Matrix x, int N)
		{
			return Compute.StandardDeviation(x, N, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the sample standard deviation of <paramref name="x"/>  along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y equals the standard deviation of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix StandardDeviation(ComplexMatrix x, int N)
		{
			return Compute.StandardDeviation(x, N, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the sample standard deviation of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>A Matrix y, where y equals the standard deviation of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix StandardDeviation(Matrix x, int N, StatisticsType stat)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			double std;
			Vector temp = new Vector(new double[lengthN]);
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				std = Compute.StandardDeviation(temp, stat);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = std;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the standard deviation of <paramref name="x"/>  along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>A ComplexMatrix y, where y equals the standard deviation of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix StandardDeviation(ComplexMatrix x, int N, StatisticsType stat)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			Complex std;
			ComplexVector temp = new ComplexVector(new Complex[lengthN]);
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				std = Compute.StandardDeviation(temp, stat);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = std;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //StandardDeviation

		#region StandardError

		/// <summary>
		/// Returns the standard error of the sample mean for a <see cref="Vector"/> of observations.
		/// </summary>
		/// <param name="x">A Vector of observations.</param>
		/// <returns>The standard error of <paramref name="x"/>.</returns>
		/// <seealso cref="Mean(Vector)"/>
		/// <seealso cref="Median(Vector)"/>
		/// <seealso cref="Variance(Vector)"/>
		/// <seealso cref="StandardDeviation(Vector)"/>
		public static double StandardError(Vector x)
		{
			return Compute.StandardDeviation(x) / Compute.Sqrt(x.Length);
		}

		/// <summary>
		/// Returns the standard error of the sample mean for a <see cref="ComplexVector"/> of observations.
		/// </summary>
		/// <param name="x">A ComplexVector of observations.</param>
		/// <returns>The standard error of <paramref name="x"/>.</returns>
		/// <seealso cref="Mean(ComplexVector)"/>
		/// <seealso cref="Median(ComplexVector)"/>
		/// <seealso cref="Variance(ComplexVector)"/>
		/// <seealso cref="StandardDeviation(ComplexVector)"/>
		public static Complex StandardError(ComplexVector x)
		{
			return Compute.StandardDeviation(x) / Compute.Sqrt(x.Length);
		}

		/// <summary>
		/// Returns the standard error of the mean of all the elements of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The standard error of the mean of x.</returns>
		public static double StandardError(Matrix x)
		{
			return Compute.StandardError(new Vector(x.Elements, x.Dimensions));
		}

		/// <summary>
		/// Returns the standard error of the mean of all the elements of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The standard error of the mean of x.</returns>
		public static Complex StandardError(ComplexMatrix x)
		{
			return Compute.StandardError(new ComplexVector(x.Elements, x.Dimensions));
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the standard error of the mean of <paramref name="x"/>  along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y equals the standard error of the mean of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix StandardError(Matrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			double stErr;
			Vector temp = new Vector(new double[lengthN]);
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				stErr = Compute.StandardError(temp);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = stErr;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the standard error of the mean of <paramref name="x"/>  along the 
		/// Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y equals the standard error of the mean of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix StandardError(ComplexMatrix x, int N)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			Complex stErr;
			ComplexVector temp = new ComplexVector(new Complex[lengthN]);
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				stErr = Compute.StandardError(temp);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = stErr;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //StandardError

		#region Variance

		/// <summary>
		/// Returns the sample variance of a <see cref="Vector"/> of observations.
		/// </summary>
		/// <param name="x">A Vector of observations.</param>
		/// <returns>The variance of <paramref name="x"/>.</returns>
		/// <seealso cref="Mean(Vector)"/>
		/// <seealso cref="Median(Vector)"/>
		/// <seealso cref="StandardDeviation(Vector)"/>
		/// <seealso cref="StandardError(Vector)"/>
		public static double Variance(Vector x)
		{
			return Variance(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the sample variance of a <see cref="ComplexVector"/> of observations.
		/// </summary>
		/// <param name="x">A ComplexVector of observations.</param>
		/// <returns>The variance of <paramref name="x"/>.</returns>
		/// <seealso cref="Mean(ComplexVector)"/>
		/// <seealso cref="Median(ComplexVector)"/>
		/// <seealso cref="StandardDeviation(ComplexVector)"/>
		/// <seealso cref="StandardError(ComplexVector)"/>
		public static Complex Variance(ComplexVector x)
		{
			return Variance(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the variance of a <see cref="Vector"/> of observations.
		/// </summary>
		/// <param name="x">A Vector of observations.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The variance of <paramref name="x"/>.</returns>
		/// <seealso cref="StandardDeviation(Vector,StatisticsType)"/>
		public static double Variance(Vector x, StatisticsType stat)
		{ 
			if (x.Length <= 1)
				return 0;

			if(stat == StatisticsType.Population)
				return Compute.Sum(Compute.Pow(x - Compute.Mean(x), 2)) / x.Length;

			return Compute.Sum(Compute.Pow(x - Compute.Mean(x), 2)) / (x.Length - 1);
		}

		/// <summary>
		/// Returns the variance of a <see cref="ComplexVector"/> of observations.
		/// </summary>
		/// <param name="x">A ComplexVector of observations.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The variance of <paramref name="x"/>.</returns>
		/// <seealso cref="StandardDeviation(ComplexVector,StatisticsType)"/>
		public static Complex Variance(ComplexVector x, StatisticsType stat)
		{
			if (x.Length <= 1)
				return 0;

			if (stat == StatisticsType.Population)
				return Compute.Sum(Compute.Pow(x - Compute.Mean(x), 2)) / x.Length;

			return Compute.Sum(Compute.Pow(x - Compute.Mean(x), 2)) / (x.Length - 1);
		}

		/// <summary>
		/// Returns the sample variance of all the elements of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <returns>The sample variance of x.</returns>
		public static double Variance(Matrix x)
		{
			return Compute.Variance(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the sample variance of all the elements of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <returns>The sample variance of x.</returns>
		public static Complex Variance(ComplexMatrix x)
		{
			return Compute.Variance(x, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns the variance of all the elements of the <see cref="Matrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The variance of x.</returns>
		public static double Variance(Matrix x, StatisticsType stat)
		{
			return Compute.Variance(new Vector(x.Elements, x.Dimensions), stat);
		}

		/// <summary>
		/// Returns the variance of all the elements of the <see cref="ComplexMatrix"/> <paramref name="x"/>.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>The variance of x.</returns>
		public static Complex Variance(ComplexMatrix x, StatisticsType stat)
		{
			return Compute.Variance(new ComplexVector(x.Elements, x.Dimensions), stat);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the sample variance of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A Matrix y, where y equals the variance of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Variance(Matrix x, int N)
		{
			return Compute.Variance(x, N, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the sample variance of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <returns>A ComplexMatrix y, where y equals the variance of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Variance(ComplexMatrix x, int N)
		{
			return Compute.Variance(x, N, StatisticsType.Sample);
		}

		/// <summary>
		/// Returns a <see cref="Matrix"/> with the variance of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A Matrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>A Matrix y, where y equals the variance of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static Matrix Variance(Matrix x, int N, StatisticsType stat)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return Matrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			double var;
			Vector temp = new Vector(new double[lengthN]);
			int[] subscript = new int[x.Rank];
			Matrix result = new Matrix(new double[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				var = Compute.Variance(temp, stat);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = var;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="ComplexMatrix"/> with the variance of <paramref name="x"/>  along the Nth dimension.
		/// </summary>
		/// <param name="x">A ComplexMatrix.</param>
		/// <param name="N">The calculation dimension.</param>
		/// <param name="stat">The type of statistic desired.</param>
		/// <returns>A ComplexMatrix y, where y equals the variance of x along the Nth dimension.</returns>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="N"/> is less than 1.
		/// </exception>
		public static ComplexMatrix Variance(ComplexMatrix x, int N, StatisticsType stat)
		{
			if (N < 1)
				throw new ArgumentException(Compute.GetString("LE_ArgumentException_15"));

			if (x.IsEmpty())
				return ComplexMatrix.Empty;

			if (N > x.Rank)
				return x.Clone();

			int Nm1 = N - 1;
			int[] newDimensions = (int[])x.Dimensions.Clone();
			int lengthN = x.Dimensions[Nm1];
			newDimensions[Nm1] = 1;
			int newLength = Utilities.Product(newDimensions);
			Vector size = x.Size;

			Complex var;
			ComplexVector temp = new ComplexVector(new Complex[lengthN]);
			int[] subscript = new int[x.Rank];
			ComplexMatrix result = new ComplexMatrix(new Complex[newLength], newDimensions);
			Vector newSize = result.Size;

			for (int i = 0, length = newLength; i < length; i++)
			{
				for (int j = 0; j < lengthN; j++)
				{
					subscript[Nm1] = j;
					temp.Elements[j] = x.Elements[Compute.SubscriptToIndex(subscript, size)];
				}

				var = Compute.Variance(temp, stat);
				subscript[Nm1] = 0;
				result.Elements[Compute.SubscriptToIndex(subscript, newSize)] = var;

				Utilities.IncrementSubscript(ref subscript, N, x.Dimensions);
			}

			return result;
		}

		#endregion //Variance

		#endregion //Staticstics

		#region Excel

		#region AverageDeviation

		/// <summary>
		/// Returns the average absolute deviation from the mean of a <see cref="Vector"/> of data.
		/// </summary>
		/// <param name="x">A Vector of data.</param>
		/// <returns>The average deviation of <paramref name="x"/>.</returns>
		public static double AverageDeviation(Vector x)
		{
			if (x.IsEmpty())
				throw new DivideByZeroException(Compute.GetString("LE_ArgumentException_53"));

			return Compute.Mean((x - Compute.Mean(x)).Abs());
		}

		/// <summary>
		/// Returns the average absolute deviation from the mean of a <see cref="ComplexVector"/> of data.
		/// </summary>
		/// <param name="x">A ComplexVector of data.</param>
		/// <returns>The average deviation of <paramref name="x"/>.</returns>
		public static Complex AverageDeviation(ComplexVector x)
		{
			if (x.IsEmpty())
				throw new DivideByZeroException(Compute.GetString("LE_ArgumentException_53"));

			return Compute.Mean(Compute.Abs(x - Compute.Mean(x)));
		}

		#endregion //AverageDeviation

		#region AverageIf

		/// <summary>
		/// Returns the average value of the <see cref="Vector"/> <paramref name="x"/> where the corresponding 
		/// <see cref="BooleanVector"/> is True.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="comparison">A BooleanVector with the same length and orientation as x.</param>
		/// <returns>The average value of the elements of x where <paramref name="comparison"/> is true.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs if the comparison has a different length than x.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs if the comparison has a different orientation than x.
		/// </exception>
		/// <remarks>
		/// The BooleanVector comparison can be easily constructed using the compare operators on Vector. The following
		/// notation is convenient:
		/// 
		/// Vector x1 = Compute.Line(-1, 1, 1000);
		/// double sumif = Compute.AverageIf(x, x > 0);
		/// </remarks>
		/// <seealso cref="Compute.Mean(Vector)"/>
		public static double AverageIf(Vector x, BooleanVector comparison)
		{
			double result;
			Exception exception;
			if(!TryAverageIf(x, comparison, out result, out exception))
			{
				throw exception;
			}

			return result;
		}

		/// <summary>
		/// Returns the average value of the <see cref="ComplexVector"/> <paramref name="x"/> where the corresponding 
		/// <see cref="BooleanVector"/> is True.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <param name="comparison">A BooleanVector with the same length and orientation as x.</param>
		/// <returns>The average value of the elements of x where <paramref name="comparison"/> is true.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs if the comparison has a different length than x.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs if the comparison has a different orientation than x.
		/// </exception>
		/// <remarks>
		/// The BooleanVector comparison can be easily constructed using the compare operators on ComplexVector. The 
		/// following notation is convenient:
		/// 
		/// Complex i = Constant.I;
		/// ComplexVector x1 = Compute.Line(-1, 1, 1000) + i*Compute.Line(1, -1, 1000);
		/// Complex sum = Compute.AverageIf(x, x > 0);
		/// </remarks>
		/// <seealso cref="Compute.Mean(ComplexVector)"/>
		public static Complex AverageIf(ComplexVector x, BooleanVector comparison)
		{
			Complex result;
			Exception exception;
			if (!TryAverageIf(x, comparison, out result, out exception))
			{
				throw exception;
			}

			return result;
		}

		/// <summary>
		/// Returns the average value of the elements of the <see cref="Vector"/> <paramref name="x"/> which satisfy the condition 
		/// specified by the <see cref="ComparisonType"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="op">A ComparisonType.</param>
		/// <param name="c">A double.</param>
		/// <returns>The conditional average of x.</returns>
		/// <seealso cref="Compute.Mean(Vector)"/>
		public static double AverageIf(Vector x, ComparisonType op, double c)
		{
			double result;
			Exception exception;
			if (!TryAverageIf(x, op, c, out result, out exception))
			{
				throw exception;
			}

			return result;
		}

		/// <summary>
		/// Returns the average value of the elements of the <see cref="ComplexVector"/> <paramref name="x"/> which satisfy the 
		/// condition specified by the <see cref="ComparisonType"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <param name="op">A ComparisonType.</param>
		/// <param name="c">A <see cref="Complex"/> number.</param>
		/// <returns>The conditional average of x.</returns>
		/// <seealso cref="Compute.Mean(ComplexVector)"/>
		public static Complex AverageIf(ComplexVector x, ComparisonType op, Complex c)
		{
			Complex result;
			Exception exception;
			if (!TryAverageIf(x, op, c, out result, out exception))
			{
				throw exception;
			}

			return result;
		}

		/// <summary>
		/// Returns the average value of the <see cref="Vector"/> <paramref name="x"/> where the corresponding 
		/// <see cref="BooleanVector"/> is True as the result.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="comparison">A BooleanVector with the same length and orientation as x.</param>
		/// <param name="result">The average value of the elements of x where <paramref name="comparison"/> is true.</param>
		/// <param name="exception">The excepton if one should be thrown; false otherwise.</param>
		/// <returns>True if the computation is successful; false otherwise.</returns>
		public static bool TryAverageIf(Vector x, BooleanVector comparison, out double result, out Exception exception)
		{
			result = 0;
			exception = null;

			int xLength = x.Length;
			int cLength = comparison.Length;
			if (xLength != cLength)
			{
				exception = new ArithmeticException(Compute.GetString("LE_ArithmeticException_15"));
				return false;
			}

			if (!Utilities.ArrayEquals(x.Dimensions, comparison.Dimensions))
			{
				exception = new ArithmeticException(Compute.GetString("LE_ArithmeticException_16"));
				return false;
			}

			if (x.IsEmpty())
			{
				exception = new DivideByZeroException(Compute.GetString("LE_ArgumentException_53"));
				return false;
			}

			int N = 0;
			for (int i = 0; i < xLength; i++)
			{
				if (comparison.Elements[i])
				{
					result += x.Elements[i];
					N++;
				}
			}

			if (N == 0)
			{
				exception = new DivideByZeroException(Compute.GetString("LE_ArgumentException_53"));
				return false;
			}

			result = result / (double)N;
			return true;
		}

		/// <summary>
		/// Returns the average value of the <see cref="ComplexVector"/> <paramref name="x"/> where the corresponding 
		/// <see cref="BooleanVector"/> is True as the result.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <param name="comparison">A BooleanVector with the same length and orientation as x.</param>
		/// <param name="result">The average value of the elements of x where <paramref name="comparison"/> is true.</param>
		/// <param name="exception">The excepton if one should be thrown; false otherwise.</param>
		/// <returns>True if the computation is successful; false otherwise.</returns>
		public static bool TryAverageIf(ComplexVector x, BooleanVector comparison, out Complex result, out Exception exception)
		{
			result = 0;
			exception = null;

			int xLength = x.Length;
			int cLength = comparison.Length;
			if (xLength != cLength)
			{
				exception = new ArithmeticException(Compute.GetString("LE_ArithmeticException_15"));
				return false;
			}

			if (!Utilities.ArrayEquals(x.Dimensions, comparison.Dimensions))
			{
				exception = new ArithmeticException(Compute.GetString("LE_ArithmeticException_16"));
				return false;
			}

			if (x.IsEmpty())
			{
				exception = new DivideByZeroException(Compute.GetString("LE_ArgumentException_54"));
				return false;
			}

			int N = 0;
			for (int i = 0; i < xLength; i++)
			{
				if (comparison.Elements[i])
				{
					result += x.Elements[i];
					N++;
				}
			}

			if (N == 0)
			{
				exception = new DivideByZeroException(Compute.GetString("LE_ArgumentException_54"));
				return false;
			}

			result = result / (Complex)N;
			return true;
		}

		/// <summary>
		/// Returns the average value of the elements of the <see cref="Vector"/> <paramref name="x"/> which satisfy the condition 
		/// specified by the <see cref="ComparisonType"/> as the <paramref name="result"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="op">A ComparisonType.</param>
		/// <param name="c">A double.</param>
		/// <param name="result">The conditional average of x.</param>
		/// <param name="exception">The exception, if one should be thrown; Null otherwise.</param>
		/// <returns>True if the computation is successful; False otherwise.</returns>
		public static bool TryAverageIf(Vector x, ComparisonType op, double c, out double result, out Exception exception)
		{
			result = 0;
			exception = null;

			if (x.IsEmpty())
			{
				exception = new DivideByZeroException(Compute.GetString("LE_ArgumentException_53"));
				return false;
			}

			BooleanVector comparison;
			switch (op)
			{
				case ComparisonType.GreaterThan:
					comparison = x > c;
					break;

				case ComparisonType.GreaterThanOrEqualTo:
					comparison = x >= c;
					break;

				case ComparisonType.LessThan:
					comparison = x < c;
					break;

				case ComparisonType.LessThanOrEqualTo:
					comparison = x <= c;
					break;

				case ComparisonType.EqualTo:
					comparison = x == c;
					break;

				case ComparisonType.NotEqualTo:
					comparison = x != c;
					break;

				default:
					exception = new IndexOutOfRangeException(Compute.GetString("LE_IndexOutOfRangeException_1"));
					return false;
			}

			if (!TryAverageIf(x, comparison, out result, out exception))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns the average value of the elements of the <see cref="ComplexVector"/> <paramref name="x"/> which satisfy the condition 
		/// specified by the <see cref="ComparisonType"/> as the <paramref name="result"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <param name="op">A ComparisonType.</param>
		/// <param name="c">A <see cref="Complex"/> number.</param>
		/// <param name="result">The conditional average of x.</param>
		/// <param name="exception">The exception, if one should be thrown; Null otherwise.</param>
		/// <returns>True if the computation is successful; False otherwise.</returns>
		public static bool TryAverageIf(ComplexVector x, ComparisonType op, Complex c, out Complex result, out Exception exception)
		{
			result = 0;
			exception = null;

			if (x.IsEmpty())
			{
				exception = new DivideByZeroException(Compute.GetString("LE_ArgumentException_53"));
				return false;
			}

			BooleanVector comparison;
			switch (op)
			{
				case ComparisonType.GreaterThan:
					comparison = x > c;
					break;

				case ComparisonType.GreaterThanOrEqualTo:
					comparison = x >= c;
					break;

				case ComparisonType.LessThan:
					comparison = x < c;
					break;

				case ComparisonType.LessThanOrEqualTo:
					comparison = x <= c;
					break;

				case ComparisonType.EqualTo:
					comparison = x == c;
					break;

				case ComparisonType.NotEqualTo:
					comparison = x != c;
					break;

				default:
					exception = new IndexOutOfRangeException(Compute.GetString("LE_IndexOutOfRangeException_1"));
					return false;
			}

			if (!TryAverageIf(x, comparison, out result, out exception))
			{
				return false;
			}

			return true;
		}

		#endregion //AverageIf

		#region Standardize

		/// <summary>
		/// Returns the z-score of <paramref name="x"/> with respect a distribution of mean <paramref name="mu"/> and 
		/// standard deviation <paramref name="sigma"/>.
		/// </summary>
		/// <param name="x">A double.</param>
		/// <param name="mu">The mean of a distribution.</param>
		/// <param name="sigma">The standard deviation of a distribution.</param>
		/// <returns>The z-score of x.</returns>
		public static double Standardize(double x, double mu, double sigma)
		{
			if (sigma <= 0)
				return Constant.NaN;

			return (x - mu) / sigma;
		}

		/// <summary>
		/// Returns the z-score of the elements of <paramref name="x"/> with respect to their mean and standard deviation.
		/// </summary>
		/// <param name="x">A Vector of data.</param>
		/// <returns>The z-score of the elements of x.</returns>
		public static Vector Standardize(Vector x)
		{
			if (x.IsEmpty())
				return Vector.Empty;

			return (x - Compute.Mean(x)) / Compute.StandardDeviation(x);
		}

		#endregion //Standardize

		#region SumIf

		/// <summary>
		/// Returns the sum of the elements of the <see cref="Vector"/> <paramref name="x"/> where the corresponding 
		/// <see cref="BooleanVector"/> is True.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="comparison">A BooleanVector with the same length and orientation as x.</param>
		/// <returns>The sum of the elements of x where <paramref name="comparison"/> is true.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs if the comparison has a different length than x.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs if the comparison has a different orientation than x.
		/// </exception>
		/// <remarks>
		/// The BooleanVector comparison can be easily constructed using the compare operators on Vector. The following
		/// notation is convenient:
		/// 
		/// Vector x1 = Compute.Line(-1, 1, 1000);
		/// double sumif = Compute.SumIf(x, x > 0);
		/// </remarks>
		/// <seealso cref="Compute.Sum(Vector)"/>
		public static double SumIf(Vector x, BooleanVector comparison)
		{
			int xLength = x.Length;
			int cLength = comparison.Length;
			if (xLength != cLength)
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_15"));

			if (!Utilities.ArrayEquals(x.Dimensions, comparison.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_16"));

			if (x.IsEmpty())
				return 0;

			double result = 0;
			for (int i = 0; i < xLength; i++)
			{
				if (comparison.Elements[i])
					result += x.Elements[i];
			}

			return result;
		}

		/// <summary>
		/// Returns the sum of the elements of the <see cref="ComplexVector"/> <paramref name="x"/> where the corresponding 
		/// <see cref="BooleanVector"/> is True.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <param name="comparison">A BooleanVector with the same length and orientation as x.</param>
		/// <returns>The sum of the elements of x where <paramref name="comparison"/> is true.</returns>
		/// <exception cref="ArithmeticException">
		/// Occurs if the comparison has a different length than x.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Occurs if the comparison has a different orientation than x.
		/// </exception>
		/// <remarks>
		/// The BooleanVector comparison can be easily constructed using the compare operators on ComplexVector. The 
		/// following notation is convenient:
		/// 
		/// Complex i = Constant.I;
		/// ComplexVector x1 = Compute.Line(-1, 1, 1000) + i*Compute.Line(1, -1, 1000);
		/// Complex sum = Compute.SumIf(x, x > 0);
		/// </remarks>
		/// <seealso cref="Compute.Sum(ComplexVector)"/>
		public static Complex SumIf(ComplexVector x, BooleanVector comparison)
		{
			int xLength = x.Length;
			int cLength = comparison.Length;
			if (xLength != cLength)
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_15"));

			if (!Utilities.ArrayEquals(x.Dimensions, comparison.Dimensions))
				throw new ArithmeticException(Compute.GetString("LE_ArithmeticException_16"));

			if (x.IsEmpty())
				return 0;

			Complex result = 0;
			for (int i = 0; i < xLength; i++)
			{
				if (comparison.Elements[i])
					result += x.Elements[i];
			}

			return result;
		}

		/// <summary>
		/// Returns the sum of the elements of the <see cref="Vector"/> <paramref name="x"/> that satisfy the condition 
		/// specified by the <see cref="ComparisonType"/>.
		/// </summary>
		/// <param name="x">A Vector.</param>
		/// <param name="op">A ComparisonType.</param>
		/// <param name="c">A double.</param>
		/// <returns>The conditional sum of x.</returns>
		/// <seealso cref="Compute.Sum(Vector)"/>
		public static double SumIf(Vector x, ComparisonType op, double c)
		{
			if (x.IsEmpty())
				return 0;

			switch (op)
			{ 
				case ComparisonType.GreaterThan:
					return Compute.SumIf(x, x > c);

				case ComparisonType.GreaterThanOrEqualTo:
					return Compute.SumIf(x, x >= c);

				case ComparisonType.LessThan:
					return Compute.SumIf(x, x < c);

				case ComparisonType.LessThanOrEqualTo:
					return Compute.SumIf(x, x <= c);

				case ComparisonType.EqualTo:
					return Compute.SumIf(x, x == c);

				case ComparisonType.NotEqualTo:
					return Compute.SumIf(x, x != c);

				default:
					throw new IndexOutOfRangeException(Compute.GetString("LE_IndexOutOfRangeException_1"));
			}
		}

		/// <summary>
		/// Returns the sum of the elements of the <see cref="ComplexVector"/> <paramref name="x"/> that satisfy the 
		/// condition specified by the <see cref="ComparisonType"/>.
		/// </summary>
		/// <param name="x">A ComplexVector.</param>
		/// <param name="op">A ComparisonType.</param>
		/// <param name="c">A <see cref="Complex"/> number.</param>
		/// <returns>The conditional sum of x.</returns>
		/// <seealso cref="Compute.Sum(Vector)"/>
		public static Complex SumIf(ComplexVector x, ComparisonType op, Complex c)
		{
			if (x.IsEmpty())
				return 0;

			switch (op)
			{
				case ComparisonType.GreaterThan:
					return Compute.SumIf(x, x > c);

				case ComparisonType.GreaterThanOrEqualTo:
					return Compute.SumIf(x, x >= c);

				case ComparisonType.LessThan:
					return Compute.SumIf(x, x < c);

				case ComparisonType.LessThanOrEqualTo:
					return Compute.SumIf(x, x <= c);

				case ComparisonType.EqualTo:
					return Compute.SumIf(x, x == c);

				case ComparisonType.NotEqualTo:
					return Compute.SumIf(x, x != c);

				default:
					throw new IndexOutOfRangeException(Compute.GetString("LE_IndexOutOfRangeException_1"));
			}
		}

		#endregion //SumIf

		#region SumProduct

		/// <summary>
		/// Returns the sum of the products of the corresponding elements of each <see cref="Vector"/> 
		/// in <paramref name="x"/>.
		/// </summary>
		/// <param name="x">An array of Vectors with the same dimensions.</param>
		/// <returns>A double y, where y = (x[0][0]*...*x[M][0]) + ... + (x[0][N]*...*x[M][N]).</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if the dimensions of the Vectors in x do not have the same dimensions.
		/// </exception>
		public static double SumProduct(params Vector[] x)
		{
			double result;
			Exception exception;
			if (!TrySumProduct(out result, out exception, x))
			{
				throw exception;
			}
			return result;
		}

		/// <summary>
		/// Returns the sum of the products of the corresponding elements of each <see cref="Vector"/> 
		/// in <paramref name="x"/> as the result.
		/// </summary>
		/// <param name="exception">The exception, if one is thrown; Null otherwise.</param>
		/// <param name="result">A double y, where y = (x[0][0]*...*x[M][0]) + ... + (x[0][N]*...*x[M][N]).</param>
		/// <param name="x">An array of Vectors with the same dimensions.</param>
		/// <returns>True if the result is computed; false otherwise.</returns>
		public static bool TrySumProduct(out double result, out Exception exception, params Vector[] x)
		{
			result = 0;
			exception = null;

			int xLength = x.Length;
			if (xLength == 0)
				return true;

			int length = x[0].Length;
			int[] dimensions = x[0].Dimensions;
			for (int i = 0; i < xLength; i++)
			{ 
				if(!Utilities.ArrayEquals(dimensions,x[i].Dimensions))
				{
					exception = new ArgumentException(Compute.GetString("LE_ArgumentException_55"));
					return false;
				}
			}

			double product;
			for (int i = 0; i < length; i++)
			{
				product = 1;
				for (int j = 0; j < xLength; j++)
				{
					product *= x[j].Elements[i];
				}
				result += product;
			}

			return true;
		}

		/// <summary>
		/// Returns the sum of the products of the corresponding elements of each <see cref="ComplexVector"/> 
		/// in <paramref name="x"/>.
		/// </summary>
		/// <param name="x">An array of ComplexVectors with the same dimensions.</param>
		/// <returns>A <see cref="Complex"/> number y, where y = (x[0][0]*...*x[M][0]) + ... + (x[0][N]*...*x[M][N]).</returns>
		/// <exception cref="ArgumentException">
		/// Occurs if the dimensions of the ComplexVectors in x do not have the same dimensions.
		/// </exception>
		public static Complex SumProduct(params ComplexVector[] x)
		{
			Complex result;
			Exception exception;
			if (!TrySumProduct(out result, out exception, x))
			{
				throw exception;
			}
			return result;
		}

		/// <summary>
		/// Returns the sum of the products of the corresponding elements of each <see cref="ComplexVector"/> 
		/// in <paramref name="x"/> as the result.
		/// </summary>
		/// <param name="exception">The exception, if one is thrown; Null otherwise.</param>
		/// <param name="result">A <see cref="Complex"/> number y, where y = (x[0][0]*...*x[M][0]) + ... + (x[0][N]*...*x[M][N]).</param>
		/// <param name="x">An array of ComplexVectors with the same dimensions.</param>
		/// <returns>True if the result is computed; false otherwise.</returns>
		public static bool TrySumProduct(out Complex result, out Exception exception, params ComplexVector[] x)
		{
			result = 0;
			exception = null;

			int xLength = x.Length;
			if (xLength == 0)
				return true;

			int length = x[0].Length;
			int[] dimensions = x[0].Dimensions;
			for (int i = 0; i < xLength; i++)
			{
				if (!Utilities.ArrayEquals(dimensions, x[i].Dimensions))
				{
					exception = new ArgumentException(Compute.GetString("LE_ArgumentException_55"));
					return false;
				}
			}

			Complex product;
			for (int i = 0; i < length; i++)
			{
				product = 1;
				for (int j = 0; j < xLength; j++)
				{
					product *= x[j].Elements[i];
				}
				result += product;
			}

			return true;
		}

		#endregion //SumProduct

		#endregion //Excel

		#endregion //Methods

		#region Utilities

		#region GetString
		internal static string GetString(string resourceName)
		{
			return SR.GetString(resourceName);
		} 
		#endregion //GetString

		#region RandomHelper



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static double RandomHelper(double x, double lowerBound, double upperBound)
		{
			return (upperBound - lowerBound) * (x - 0.5) + (lowerBound + upperBound) / 2;
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static Complex RandomHelper(double x1, double x2, Complex origin, double radius)
		{
			return (radius * x1 * Compute.Exp(Constant.I * 2 * Constant.Pi * x2)) + origin;
		}

		#endregion //RandomHelper

		#endregion //Utilities

		#region Properties






		private static Random Rnd
		{
			get
			{
				if (Compute.rnd == null)
					Compute.rnd = new Random();

				return Compute.rnd;
			}
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