using System;
using System.Collections;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(IList) }, Copy = true)]
	[RubyClass("Array", Extends = typeof(RubyArray), Inherits = typeof(object))]
	public static class ArrayOps
	{
		private sealed class BreakException : Exception
		{
		}

		[RubyConstructor]
		public static RubyArray CreateArray(RubyClass self)
		{
			return new RubyArray();
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyArray Reinitialize(RubyContext context, RubyArray self)
		{
			self.Clear();
			return self;
		}

		[RubyConstructor]
		public static object CreateArray(ConversionStorage<Union<IList, int>> toAryToInt, BlockParam block, RubyClass self, [NotNull] object arrayOrSize)
		{
			CallSite<Func<CallSite, object, Union<IList, int>>> site = toAryToInt.GetSite(CompositeConversionAction.Make(toAryToInt.Context, CompositeConversion.ToAryToInt));
			Union<IList, int> union = site.Target(site, arrayOrSize);
			if (union.First != null)
			{
				return new RubyArray(union.First);
			}
			if (block != null)
			{
				return CreateArray(block, union.Second);
			}
			return CreateArray(self, union.Second, null);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static object Reinitialize(ConversionStorage<Union<IList, int>> toAryToInt, BlockParam block, RubyArray self, [NotNull] object arrayOrSize)
		{
			RubyContext context = toAryToInt.Context;
			CallSite<Func<CallSite, object, Union<IList, int>>> site = toAryToInt.GetSite(CompositeConversionAction.Make(context, CompositeConversion.ToAryToInt));
			Union<IList, int> union = site.Target(site, arrayOrSize);
			if (union.First != null)
			{
				return Reinitialize(self, union.First);
			}
			if (block != null)
			{
				return Reinitialize(block, self, union.Second);
			}
			return ReinitializeByRepeatedValue(context, self, union.Second, null);
		}

		private static RubyArray Reinitialize(RubyArray self, IList other)
		{
			if (other != self)
			{
				self.Clear();
				IListOps.AddRange(self, other);
			}
			return self;
		}

		private static object CreateArray(BlockParam block, int size)
		{
			return Reinitialize(block, new RubyArray(), size);
		}

		[RubyConstructor]
		public static RubyArray CreateArray(BlockParam block, RubyClass self, [DefaultProtocol] int size, object value)
		{
			return Reinitialize(block, new RubyArray(), size, value);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyArray Reinitialize(BlockParam block, RubyArray self, int size, object value)
		{
			block.RubyContext.ReportWarning("block supersedes default value argument");
			Reinitialize(block, self, size);
			return self;
		}

		private static object Reinitialize(BlockParam block, RubyArray self, int size)
		{
			CheckArraySize(size);
			self.Clear();
			for (int i = 0; i < size; i++)
			{
				object blockResult;
				if (block.Yield(i, out blockResult))
				{
					return blockResult;
				}
				self.Add(blockResult);
			}
			return self;
		}

		[RubyConstructor]
		public static RubyArray CreateArray(RubyClass self, [DefaultProtocol] int size, object value)
		{
			CheckArraySize(size);
			return new RubyArray().AddMultiple(size, value);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyArray ReinitializeByRepeatedValue(RubyContext context, RubyArray self, [DefaultProtocol] int size, object value)
		{
			CheckArraySize(size);
			self.Clear();
			self.AddMultiple(size, value);
			return self;
		}

		private static void CheckArraySize(int size)
		{
			if (size < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative array size");
			}
			if (IntPtr.Size == 4 && size > 536870911)
			{
				throw RubyExceptions.CreateArgumentError("array size too big");
			}
		}

		[RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray MakeArray(RubyClass self, params object[] args)
		{
			RubyArray rubyArray = RubyArray.CreateInstance(self);
			foreach (object item in args)
			{
				rubyArray.Add(item);
			}
			return rubyArray;
		}

		[RubyMethod("to_a")]
		public static RubyArray ToArray(RubyArray self)
		{
			if (!(self is RubyArray.Subclass))
			{
				return self;
			}
			return new RubyArray(self);
		}

		[RubyMethod("to_ary")]
		public static RubyArray ToExplicitArray(RubyArray self)
		{
			return self;
		}

		[RubyMethod("try_convert", RubyMethodAttributes.PublicSingleton)]
		public static IList TryConvert(ConversionStorage<IList> toAry, RubyClass self, object obj)
		{
			CallSite<Func<CallSite, object, IList>> site = toAry.GetSite(ProtocolConversionAction<TryConvertToArrayAction>.Make(toAry.Context));
			return site.Target(site, obj);
		}

		[RubyMethod("pack")]
		public static MutableString Pack(ConversionStorage<IntegerValue> integerConversion, ConversionStorage<double> floatConversion, ConversionStorage<MutableString> stringCast, ConversionStorage<MutableString> tosConversion, RubyArray self, [DefaultProtocol][NotNull] MutableString format)
		{
			return RubyEncoder.Pack(integerConversion, floatConversion, stringCast, tosConversion, self, format);
		}

		[RubyMethod("sort")]
		public static object Sort(ComparisonStorage comparisonStorage, BlockParam block, RubyArray self)
		{
			RubyArray self2 = self.CreateInstance();
			IListOps.Replace(self2, self);
			return SortInPlace(comparisonStorage, block, self2);
		}

		[RubyMethod("sort!")]
		public static object SortInPlace(ComparisonStorage comparisonStorage, BlockParam block, RubyArray self)
		{
			StrongBox<object> breakResult;
			RubyArray result = SortInPlace(comparisonStorage, block, self, out breakResult);
			if (breakResult != null)
			{
				return breakResult.Value;
			}
			return result;
		}

		public static RubyArray SortInPlace(ComparisonStorage comparisonStorage, RubyArray self)
		{
			StrongBox<object> breakResult;
			return SortInPlace(comparisonStorage, null, self, out breakResult);
		}

		internal static RubyArray SortInPlace(ComparisonStorage comparisonStorage, BlockParam block, RubyArray self, out StrongBox<object> breakResult)
		{
			breakResult = null;
			RubyContext context = comparisonStorage.Context;
			if (block == null)
			{
				self.Sort((object x, object y) => Protocols.Compare(comparisonStorage, x, y));
				return self;
			}
			object nonRefBreakResult = null;
			try
			{
				self.Sort(delegate(object x, object y)
				{
					object blockResult = null;
					if (block.Yield(x, y, out blockResult))
					{
						nonRefBreakResult = blockResult;
						throw new BreakException();
					}
					if (blockResult == null)
					{
						throw RubyExceptions.MakeComparisonError(context, x, y);
					}
					return Protocols.ConvertCompareResult(comparisonStorage, blockResult);
				});
				return self;
			}
			catch (InvalidOperationException ex)
			{
				if (ex.InnerException == null)
				{
					throw;
				}
				if (ex.InnerException is BreakException)
				{
					breakResult = new StrongBox<object>(nonRefBreakResult);
					return null;
				}
				throw ex.InnerException;
			}
		}

		[RubyMethod("reverse!")]
		public static RubyArray InPlaceReverse(RubyContext context, RubyArray self)
		{
			self.Reverse();
			return self;
		}
	}
}
