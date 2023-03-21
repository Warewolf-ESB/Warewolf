using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math.Extensions;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyModule("Math")]
	public static class RubyMath
	{
		[RubyConstant]
		public const double E = Math.E;

		[RubyConstant]
		public const double PI = Math.PI;

		private static double DomainCheck(double result, string functionName)
		{
			if (double.IsNaN(result))
			{
				throw new Errno.DomainError("Domain error - " + functionName);
			}
			return result;
		}

		private static ushort Exponent(byte[] v)
		{
			return (ushort)(((ushort)(v[7] & 0x7F) << 4) | ((ushort)(v[6] & 0xF0) >> 4));
		}

		private static ulong Mantissa(byte[] v)
		{
			uint num = (uint)(v[0] | (v[1] << 8) | (v[2] << 16) | (v[3] << 24));
			uint num2 = (uint)(v[4] | (v[5] << 8) | ((v[6] & 0xF) << 16));
			return num | ((ulong)num2 << 32);
		}

		[RubyMethod("acos", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("acos", RubyMethodAttributes.PrivateInstance)]
		public static double Acos(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Math.Acos(x), "acos");
		}

		[RubyMethod("acosh", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("acosh", RubyMethodAttributes.PublicSingleton)]
		public static double Acosh(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Math.Log(x + Math.Sqrt(x * x - 1.0)), "acosh");
		}

		[RubyMethod("asin", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("asin", RubyMethodAttributes.PrivateInstance)]
		public static double Asin(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Math.Asin(x), "asin");
		}

		[RubyMethod("asinh", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("asinh", RubyMethodAttributes.PrivateInstance)]
		public static double Asinh(object self, [DefaultProtocol] double x)
		{
			return Math.Log(x + Math.Sqrt(x * x + 1.0));
		}

		[RubyMethod("atan", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("atan", RubyMethodAttributes.PrivateInstance)]
		public static double Atan(object self, [DefaultProtocol] double x)
		{
			return Math.Atan(x);
		}

		[RubyMethod("atan2", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("atan2", RubyMethodAttributes.PublicSingleton)]
		public static double Atan2(object self, [DefaultProtocol] double y, [DefaultProtocol] double x)
		{
			return Math.Atan2(y, x);
		}

		[RubyMethod("atanh", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("atanh", RubyMethodAttributes.PrivateInstance)]
		public static double Atanh(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(0.5 * Math.Log((1.0 + x) / (1.0 - x)), "atanh");
		}

		[RubyMethod("cos", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("cos", RubyMethodAttributes.PublicSingleton)]
		public static double Cos(object self, [DefaultProtocol] double x)
		{
			return Math.Cos(x);
		}

		[RubyMethod("cosh", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("cosh", RubyMethodAttributes.PrivateInstance)]
		public static double Cosh(object self, [DefaultProtocol] double x)
		{
			return Math.Cosh(x);
		}

		[RubyMethod("cbrt", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("cbrt", RubyMethodAttributes.PublicSingleton)]
		public static double CubeRoot(object self, [DefaultProtocol] double x)
		{
			return Math.Pow(x, 1.0 / 3.0);
		}

		[RubyMethod("erf", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("erf", RubyMethodAttributes.PrivateInstance)]
		public static double Erf(object self, [DefaultProtocol] double x)
		{
			return Microsoft.Scripting.Utils.MathUtils.Erf(x);
		}

		[RubyMethod("erfc", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("erfc", RubyMethodAttributes.PublicSingleton)]
		public static double Erfc(object self, [DefaultProtocol] double x)
		{
			return Microsoft.Scripting.Utils.MathUtils.ErfComplement(x);
		}

		[RubyMethod("exp", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("exp", RubyMethodAttributes.PublicSingleton)]
		public static double Exp(object self, [DefaultProtocol] double x)
		{
			return Math.Exp(x);
		}

		[RubyMethod("gamma", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("gamma", RubyMethodAttributes.PrivateInstance)]
		public static double Gamma(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Microsoft.Scripting.Utils.MathUtils.Gamma(x), "gamma");
		}

		[RubyMethod("hypot", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("hypot", RubyMethodAttributes.PrivateInstance)]
		public static double Hypot(object self, [DefaultProtocol] double x, [DefaultProtocol] double y)
		{
			return DomainCheck(Math.Sqrt(x * x + y * y), "hypot");
		}

		[RubyMethod("frexp", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("frexp", RubyMethodAttributes.PrivateInstance)]
		public static RubyArray Frexp(object self, [DefaultProtocol] double x)
		{
			RubyArray rubyArray = new RubyArray(2);
			byte[] bytes = BitConverter.GetBytes(x);
			double num = ((double)Mantissa(bytes) * Math.Pow(2.0, -52.0) + 1.0) / 2.0;
			int num2 = Exponent(bytes) - 1022;
			rubyArray.Add(num);
			rubyArray.Add(num2);
			return rubyArray;
		}

		[RubyMethod("ldexp", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("ldexp", RubyMethodAttributes.PublicSingleton)]
		public static double Ldexp(object self, [DefaultProtocol] double x, [DefaultProtocol] IntegerValue y)
		{
			return x * Math.Pow(2.0, y.IsFixnum ? ((double)y.Fixnum) : y.Bignum.ToFloat64());
		}

		[RubyMethod("lgamma", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("lgamma", RubyMethodAttributes.PublicSingleton)]
		public static double LogGamma(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Microsoft.Scripting.Utils.MathUtils.LogGamma(x), "lgamma");
		}

		[RubyMethod("log", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("log", RubyMethodAttributes.PrivateInstance)]
		public static double Log(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Math.Log(x), "log");
		}

		[RubyMethod("log10", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("log10", RubyMethodAttributes.PublicSingleton)]
		public static double Log10(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Math.Log10(x), "log10");
		}

		[RubyMethod("log2", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("log2", RubyMethodAttributes.PrivateInstance)]
		public static double Log2(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Math.Log(x) / Math.Log(2.0), "log2");
		}

		[RubyMethod("sin", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("sin", RubyMethodAttributes.PrivateInstance)]
		public static double Sin(object self, [DefaultProtocol] double x)
		{
			return Math.Sin(x);
		}

		[RubyMethod("sinh", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("sinh", RubyMethodAttributes.PrivateInstance)]
		public static double Sinh(object self, [DefaultProtocol] double x)
		{
			return Math.Sinh(x);
		}

		[RubyMethod("sqrt", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("sqrt", RubyMethodAttributes.PublicSingleton)]
		public static double Sqrt(object self, [DefaultProtocol] double x)
		{
			return DomainCheck(Math.Sqrt(x), "sqrt");
		}

		[RubyMethod("tan", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("tan", RubyMethodAttributes.PublicSingleton)]
		public static double Tan(object self, [DefaultProtocol] double x)
		{
			return Math.Tan(x);
		}

		[RubyMethod("tanh", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("tanh", RubyMethodAttributes.PublicSingleton)]
		public static double Tanh(object self, [DefaultProtocol] double x)
		{
			return Math.Tanh(x);
		}
	}
}
