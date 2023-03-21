using System;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;

namespace IronRuby.StandardLibrary.BigDecimal
{
	public sealed class BigDecimalLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(Numeric));
			DefineGlobalClass("BigDecimal", typeof(BigDecimal), 0, @class, LoadBigDecimal_Instance, LoadBigDecimal_Class, LoadBigDecimal_Constants, RubyModule.EmptyArray, new Func<RubyContext, RubyClass, MutableString, int, BigDecimal>(BigDecimalOps.CreateBigDecimal));
			ExtendModule(typeof(Kernel), 0, LoadIronRuby__Builtins__Kernel_Instance, LoadIronRuby__Builtins__Kernel_Class, null, RubyModule.EmptyArray);
		}

		private static void LoadBigDecimal_Constants(RubyModule module)
		{
			LibraryInitializer.SetConstant(module, "BASE", 1000000000u);
			LibraryInitializer.SetConstant(module, "EXCEPTION_ALL", 255);
			LibraryInitializer.SetConstant(module, "EXCEPTION_INFINITY", 1);
			LibraryInitializer.SetConstant(module, "EXCEPTION_NaN", 2);
			LibraryInitializer.SetConstant(module, "EXCEPTION_OVERFLOW", 1);
			LibraryInitializer.SetConstant(module, "EXCEPTION_UNDERFLOW", 4);
			LibraryInitializer.SetConstant(module, "EXCEPTION_ZERODIVIDE", 1);
			LibraryInitializer.SetConstant(module, "ROUND_CEILING", 5);
			LibraryInitializer.SetConstant(module, "ROUND_DOWN", 2);
			LibraryInitializer.SetConstant(module, "ROUND_FLOOR", 6);
			LibraryInitializer.SetConstant(module, "ROUND_HALF_DOWN", 4);
			LibraryInitializer.SetConstant(module, "ROUND_HALF_EVEN", 7);
			LibraryInitializer.SetConstant(module, "ROUND_HALF_UP", 3);
			LibraryInitializer.SetConstant(module, "ROUND_MODE", 256);
			LibraryInitializer.SetConstant(module, "ROUND_UP", 1);
			LibraryInitializer.SetConstant(module, "SIGN_NaN", 0);
			LibraryInitializer.SetConstant(module, "SIGN_NEGATIVE_FINITE", -2);
			LibraryInitializer.SetConstant(module, "SIGN_NEGATIVE_INFINITE", -3);
			LibraryInitializer.SetConstant(module, "SIGN_NEGATIVE_ZERO", -1);
			LibraryInitializer.SetConstant(module, "SIGN_POSITIVE_FINITE", 2);
			LibraryInitializer.SetConstant(module, "SIGN_POSITIVE_INFINITE", 3);
			LibraryInitializer.SetConstant(module, "SIGN_POSITIVE_ZERO", 1);
		}

		private static void LoadBigDecimal_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "-", 17, 0u, 0u, 4u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Subtract), new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Subtract), new Func<RubyContext, BigDecimal, BigInteger, BigDecimal>(BigDecimalOps.Subtract), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Subtract));
			LibraryInitializer.DefineLibraryMethod(module, "%", 17, 4u, 0u, 4u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Modulo), new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Modulo), new Func<RubyContext, BigDecimal, BigInteger, BigDecimal>(BigDecimalOps.Modulo), new Func<BinaryOpStorage, BinaryOpStorage, RubyContext, BigDecimal, object, object>(BigDecimalOps.ModuloOp));
			LibraryInitializer.DefineLibraryMethod(module, "*", 17, 0u, 0u, 4u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Multiply), new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Multiply), new Func<RubyContext, BigDecimal, BigInteger, BigDecimal>(BigDecimalOps.Multiply), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Multiply));
			LibraryInitializer.DefineLibraryMethod(module, "**", 17, 0u, new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Power));
			LibraryInitializer.DefineLibraryMethod(module, "/", 17, 0u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Divide), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Divide));
			LibraryInitializer.DefineLibraryMethod(module, "-@", 17, 0u, new Func<RubyContext, BigDecimal, BigDecimal>(BigDecimalOps.Negate));
			LibraryInitializer.DefineLibraryMethod(module, "_dump", 17, 0u, new Func<BigDecimal, object, MutableString>(BigDecimalOps.Dump));
			LibraryInitializer.DefineLibraryMethod(module, "+", 17, 4u, 0u, 4u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Add), new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Add), new Func<RubyContext, BigDecimal, BigInteger, BigDecimal>(BigDecimalOps.Add), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Add));
			LibraryInitializer.DefineLibraryMethod(module, "+@", 17, 0u, new Func<BigDecimal, BigDecimal>(BigDecimalOps.Identity));
			LibraryInitializer.DefineLibraryMethod(module, "<", 17, new uint[5] { 2u, 4u, 0u, 0u, 0u }, new Func<BigDecimal, BigDecimal, object>(BigDecimalOps.LessThan), new Func<RubyContext, BigDecimal, BigInteger, object>(BigDecimalOps.LessThan), new Func<RubyContext, BigDecimal, int, object>(BigDecimalOps.LessThan), new Func<RubyContext, BigDecimal, double, object>(BigDecimalOps.LessThan), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.LessThan));
			LibraryInitializer.DefineLibraryMethod(module, "<=", 17, new uint[5] { 2u, 4u, 0u, 0u, 0u }, new Func<BigDecimal, BigDecimal, object>(BigDecimalOps.LessThanOrEqual), new Func<RubyContext, BigDecimal, BigInteger, object>(BigDecimalOps.LessThanOrEqual), new Func<RubyContext, BigDecimal, int, object>(BigDecimalOps.LessThanOrEqual), new Func<RubyContext, BigDecimal, double, object>(BigDecimalOps.LessThanOrEqual), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.LessThanOrEqual));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 17, new uint[5] { 0u, 2u, 4u, 0u, 0u }, new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Compare), new Func<BigDecimal, BigDecimal, object>(BigDecimalOps.Compare), new Func<RubyContext, BigDecimal, BigInteger, object>(BigDecimalOps.Compare), new Func<RubyContext, BigDecimal, int, object>(BigDecimalOps.Compare), new Func<RubyContext, BigDecimal, double, object>(BigDecimalOps.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "==", 17, new uint[5] { 2u, 0u, 4u, 0u, 0u }, new Func<BigDecimal, BigDecimal, object>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, int, bool>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, BigInteger, bool>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, double, bool>(BigDecimalOps.Equal), new Func<BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Equal));
			LibraryInitializer.DefineLibraryMethod(module, "===", 17, new uint[5] { 2u, 0u, 4u, 0u, 0u }, new Func<BigDecimal, BigDecimal, object>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, int, bool>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, BigInteger, bool>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, double, bool>(BigDecimalOps.Equal), new Func<BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Equal));
			LibraryInitializer.DefineLibraryMethod(module, ">", 17, new uint[5] { 2u, 4u, 0u, 0u, 0u }, new Func<BigDecimal, BigDecimal, object>(BigDecimalOps.GreaterThan), new Func<RubyContext, BigDecimal, BigInteger, object>(BigDecimalOps.GreaterThan), new Func<RubyContext, BigDecimal, int, object>(BigDecimalOps.GreaterThan), new Func<RubyContext, BigDecimal, double, object>(BigDecimalOps.GreaterThan), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.GreaterThan));
			LibraryInitializer.DefineLibraryMethod(module, ">=", 17, new uint[5] { 2u, 4u, 0u, 0u, 0u }, new Func<BigDecimal, BigDecimal, object>(BigDecimalOps.GreaterThanOrEqual), new Func<RubyContext, BigDecimal, BigInteger, object>(BigDecimalOps.GreaterThanOrEqual), new Func<RubyContext, BigDecimal, int, object>(BigDecimalOps.GreaterThanOrEqual), new Func<RubyContext, BigDecimal, double, object>(BigDecimalOps.GreaterThanOrEqual), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.GreaterThanOrEqual));
			LibraryInitializer.DefineLibraryMethod(module, "abs", 17, 0u, new Func<RubyContext, BigDecimal, BigDecimal>(BigDecimalOps.Abs));
			LibraryInitializer.DefineLibraryMethod(module, "add", 17, new uint[9] { 4u, 0u, 4u, 0u, 4u, 0u, 4u, 0u, 524288u }, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Add), new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Add), new Func<RubyContext, BigDecimal, BigInteger, BigDecimal>(BigDecimalOps.Add), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Add), new Func<RubyContext, BigDecimal, BigDecimal, int, BigDecimal>(BigDecimalOps.Add), new Func<RubyContext, BigDecimal, int, int, BigDecimal>(BigDecimalOps.Add), new Func<RubyContext, BigDecimal, BigInteger, int, BigDecimal>(BigDecimalOps.Add), new Func<RubyContext, BigDecimal, double, int, BigDecimal>(BigDecimalOps.Add), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, int, object>(BigDecimalOps.Add));
			LibraryInitializer.DefineLibraryMethod(module, "ceil", 17, 0u, new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Ceil));
			LibraryInitializer.DefineLibraryMethod(module, "coerce", 17, 0u, 0u, 0u, 0u, new Func<BigDecimal, BigDecimal, RubyArray>(BigDecimalOps.Coerce), new Func<RubyContext, BigDecimal, double, RubyArray>(BigDecimalOps.Coerce), new Func<RubyContext, BigDecimal, int, RubyArray>(BigDecimalOps.Coerce), new Func<RubyContext, BigDecimal, BigInteger, RubyArray>(BigDecimalOps.Coerce));
			LibraryInitializer.DefineLibraryMethod(module, "div", 17, 0u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Div), new Func<RubyContext, BigDecimal, BigDecimal, int, BigDecimal>(BigDecimalOps.Div));
			LibraryInitializer.DefineLibraryMethod(module, "divmod", 17, 4u, 0u, 4u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, RubyArray>(BigDecimalOps.DivMod), new Func<RubyContext, BigDecimal, int, RubyArray>(BigDecimalOps.DivMod), new Func<RubyContext, BigDecimal, BigInteger, RubyArray>(BigDecimalOps.DivMod), new Func<BinaryOpStorage, BinaryOpStorage, RubyContext, BigDecimal, object, object>(BigDecimalOps.DivMod));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 17, new uint[5] { 2u, 0u, 4u, 0u, 0u }, new Func<BigDecimal, BigDecimal, object>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, int, bool>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, BigInteger, bool>(BigDecimalOps.Equal), new Func<RubyContext, BigDecimal, double, bool>(BigDecimalOps.Equal), new Func<BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Equal));
			LibraryInitializer.DefineLibraryMethod(module, "exponent", 17, 0u, new Func<BigDecimal, int>(BigDecimalOps.Exponent));
			LibraryInitializer.DefineLibraryMethod(module, "finite?", 17, 0u, new Func<BigDecimal, bool>(BigDecimalOps.IsFinite));
			LibraryInitializer.DefineLibraryMethod(module, "fix", 17, 0u, new Func<RubyContext, BigDecimal, BigDecimal>(BigDecimalOps.Fix));
			LibraryInitializer.DefineLibraryMethod(module, "floor", 17, 0u, new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Floor));
			LibraryInitializer.DefineLibraryMethod(module, "frac", 17, 0u, new Func<RubyContext, BigDecimal, BigDecimal>(BigDecimalOps.Fraction));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 17, 0u, new Func<BigDecimal, int>(BigDecimalOps.Hash));
			LibraryInitializer.DefineLibraryMethod(module, "infinite?", 17, 0u, new Func<BigDecimal, object>(BigDecimalOps.IsInfinite));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 17, 0u, new Func<RubyContext, BigDecimal, MutableString>(BigDecimalOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "modulo", 17, new uint[5] { 4u, 0u, 4u, 0u, 0u }, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Modulo), new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Modulo), new Func<RubyContext, BigDecimal, BigInteger, BigDecimal>(BigDecimalOps.Modulo), new Func<BinaryOpStorage, RubyContext, BigDecimal, double, object>(BigDecimalOps.Modulo), new Func<BinaryOpStorage, BinaryOpStorage, RubyContext, BigDecimal, object, object>(BigDecimalOps.Modulo));
			LibraryInitializer.DefineLibraryMethod(module, "mult", 17, new uint[5] { 0u, 0u, 4u, 0u, 524288u }, new Func<RubyContext, BigDecimal, BigDecimal, int, BigDecimal>(BigDecimalOps.Multiply), new Func<RubyContext, BigDecimal, int, int, BigDecimal>(BigDecimalOps.Multiply), new Func<RubyContext, BigDecimal, BigInteger, int, BigDecimal>(BigDecimalOps.Multiply), new Func<RubyContext, BigDecimal, double, int, BigDecimal>(BigDecimalOps.Multiply), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, int, object>(BigDecimalOps.Multiply));
			LibraryInitializer.DefineLibraryMethod(module, "nan?", 17, 0u, new Func<BigDecimal, bool>(BigDecimalOps.IsNaN));
			LibraryInitializer.DefineLibraryMethod(module, "nonzero?", 17, 0u, new Func<BigDecimal, BigDecimal>(BigDecimalOps.IsNonZero));
			LibraryInitializer.DefineLibraryMethod(module, "power", 17, 0u, new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Power));
			LibraryInitializer.DefineLibraryMethod(module, "precs", 17, 0u, new Func<BigDecimal, RubyArray>(BigDecimalOps.Precision));
			LibraryInitializer.DefineLibraryMethod(module, "quo", 17, 0u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Divide), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Quotient));
			LibraryInitializer.DefineLibraryMethod(module, "remainder", 17, 4u, 0u, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Remainder), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Remainder));
			LibraryInitializer.DefineLibraryMethod(module, "round", 17, 0u, 0u, new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Round), new Func<RubyContext, BigDecimal, int, int, BigDecimal>(BigDecimalOps.Round));
			LibraryInitializer.DefineLibraryMethod(module, "sign", 17, 0u, new Func<BigDecimal, int>(BigDecimalOps.Sign));
			LibraryInitializer.DefineLibraryMethod(module, "split", 17, 0u, new Func<BigDecimal, RubyArray>(BigDecimalOps.Split));
			LibraryInitializer.DefineLibraryMethod(module, "sqrt", 17, 0u, 0u, new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.SquareRoot), new Func<RubyContext, BigDecimal, object, object>(BigDecimalOps.SquareRoot));
			LibraryInitializer.DefineLibraryMethod(module, "sub", 17, new uint[9] { 0u, 0u, 4u, 0u, 0u, 0u, 4u, 0u, 524288u }, new Func<RubyContext, BigDecimal, BigDecimal, BigDecimal>(BigDecimalOps.Subtract), new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Subtract), new Func<RubyContext, BigDecimal, BigInteger, BigDecimal>(BigDecimalOps.Subtract), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, object>(BigDecimalOps.Subtract), new Func<RubyContext, BigDecimal, BigDecimal, int, BigDecimal>(BigDecimalOps.Subtract), new Func<RubyContext, BigDecimal, int, int, BigDecimal>(BigDecimalOps.Subtract), new Func<RubyContext, BigDecimal, BigInteger, int, BigDecimal>(BigDecimalOps.Subtract), new Func<RubyContext, BigDecimal, double, int, BigDecimal>(BigDecimalOps.Subtract), new Func<BinaryOpStorage, BinaryOpStorage, BigDecimal, object, int, object>(BigDecimalOps.Subtract));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 17, 0u, new Func<RubyContext, BigDecimal, double>(BigDecimalOps.ToFloat));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 17, 0u, new Func<RubyContext, BigDecimal, object>(BigDecimalOps.ToI));
			LibraryInitializer.DefineLibraryMethod(module, "to_int", 17, 0u, new Func<RubyContext, BigDecimal, object>(BigDecimalOps.ToI));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 17, 0u, 65536u, 65538u, new Func<BigDecimal, MutableString>(BigDecimalOps.ToString), new Func<BigDecimal, int, MutableString>(BigDecimalOps.ToString), new Func<BigDecimal, MutableString, MutableString>(BigDecimalOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "truncate", 17, 0u, new Func<RubyContext, BigDecimal, int, BigDecimal>(BigDecimalOps.Truncate));
			LibraryInitializer.DefineLibraryMethod(module, "zero?", 17, 0u, new Func<BigDecimal, bool>(BigDecimalOps.IsZero));
		}

		private static void LoadBigDecimal_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "_load", 33, 131072u, new Func<RubyContext, RubyClass, MutableString, BigDecimal>(BigDecimalOps.Load));
			LibraryInitializer.DefineLibraryMethod(module, "double_fig", 33, 0u, new Func<RubyClass, int>(BigDecimalOps.DoubleFig));
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 33, 2u, 0u, 4u, 0u, new Func<RubyClass, BigDecimal, BigDecimal>(BigDecimalOps.InducedFrom), new Func<RubyContext, RubyClass, int, BigDecimal>(BigDecimalOps.InducedFrom), new Func<RubyContext, RubyClass, BigInteger, BigDecimal>(BigDecimalOps.InducedFrom), new Func<RubyClass, object, BigDecimal>(BigDecimalOps.InducedFrom));
			LibraryInitializer.DefineLibraryMethod(module, "limit", 33, 0u, 0u, new Func<RubyContext, RubyClass, int, int>(BigDecimalOps.Limit), new Func<RubyContext, RubyClass, object, int>(BigDecimalOps.Limit));
			LibraryInitializer.DefineLibraryMethod(module, "mode", 33, 0u, 0u, new Func<RubyContext, RubyClass, int, int>(BigDecimalOps.Mode), new Func<RubyContext, RubyClass, int, object, int>(BigDecimalOps.Mode));
			LibraryInitializer.DefineLibraryMethod(module, "ver", 33, 0u, new Func<RubyClass, MutableString>(BigDecimalOps.Version));
		}

		private static void LoadIronRuby__Builtins__Kernel_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "BigDecimal", 18, 131072u, new Func<RubyContext, object, MutableString, int, object>(KernelOps.CreateBigDecimal));
		}

		private static void LoadIronRuby__Builtins__Kernel_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "BigDecimal", 33, 131072u, new Func<RubyContext, object, MutableString, int, object>(KernelOps.CreateBigDecimal));
		}
	}
}
