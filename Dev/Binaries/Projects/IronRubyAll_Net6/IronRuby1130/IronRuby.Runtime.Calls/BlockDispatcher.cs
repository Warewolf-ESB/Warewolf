using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	public abstract class BlockDispatcher
	{
		internal const int MaxBlockArity = 4;

		internal const int HiddenParameterCount = 2;

		private readonly string _sourcePath;

		private readonly int _sourceLine;

		private readonly BlockSignatureAttributes _attributesAndArity;

		public bool HasUnsplatParameter
		{
			get
			{
				return (_attributesAndArity & BlockSignatureAttributes.HasUnsplatParameter) != 0;
			}
		}

		public bool HasProcParameter
		{
			get
			{
				return (_attributesAndArity & BlockSignatureAttributes.HasProcParameter) != 0;
			}
		}

		public int Arity
		{
			get
			{
				return (int)_attributesAndArity >> 2;
			}
		}

		public abstract int ParameterCount { get; }

		public abstract Delegate Method { get; }

		public string SourcePath
		{
			get
			{
				return _sourcePath;
			}
		}

		public int SourceLine
		{
			get
			{
				return _sourceLine;
			}
		}

		public static BlockSignatureAttributes MakeAttributes(BlockSignatureAttributes attributes, int arity)
		{
			return (BlockSignatureAttributes)((int)attributes | (arity << 2));
		}

		internal abstract BlockDispatcher SetMethod(object method);

		public abstract object Invoke(BlockParam param, object self, Proc procArg);

		public abstract object InvokeNoAutoSplat(BlockParam param, object self, Proc procArg, object arg1);

		public abstract object Invoke(BlockParam param, object self, Proc procArg, object arg1);

		public abstract object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2);

		public abstract object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3);

		public abstract object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4);

		public abstract object Invoke(BlockParam param, object self, Proc procArg, object[] args);

		public abstract object InvokeSplat(BlockParam param, object self, Proc procArg, IList splattee);

		public abstract object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, IList splattee);

		public abstract object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, IList splattee);

		public abstract object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, IList splattee);

		public abstract object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList splattee);

		public abstract object InvokeSplat(BlockParam param, object self, Proc procArg, object[] args, IList splattee);

		public abstract object InvokeSplatRhs(BlockParam param, object self, Proc procArg, object[] args, IList splattee, object rhs);

		internal BlockDispatcher(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
		{
			_attributesAndArity = attributesAndArity;
			_sourcePath = sourcePath;
			_sourceLine = sourceLine;
		}

		internal static BlockDispatcher Create(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
		{
			if ((attributesAndArity & BlockSignatureAttributes.HasUnsplatParameter) == 0)
			{
				if ((attributesAndArity & BlockSignatureAttributes.HasProcParameter) == 0)
				{
					switch (parameterCount)
					{
					case 0:
						return new BlockDispatcher0(attributesAndArity, sourcePath, sourceLine);
					case 1:
						return new BlockDispatcher1(attributesAndArity, sourcePath, sourceLine);
					case 2:
						return new BlockDispatcher2(attributesAndArity, sourcePath, sourceLine);
					case 3:
						return new BlockDispatcher3(attributesAndArity, sourcePath, sourceLine);
					case 4:
						return new BlockDispatcher4(attributesAndArity, sourcePath, sourceLine);
					default:
						return new BlockDispatcherN(parameterCount, attributesAndArity, sourcePath, sourceLine);
					}
				}
				return new BlockDispatcherProcN(parameterCount, attributesAndArity, sourcePath, sourceLine);
			}
			if ((attributesAndArity & BlockSignatureAttributes.HasProcParameter) == 0)
			{
				return new BlockDispatcherUnsplatN(parameterCount, attributesAndArity, sourcePath, sourceLine);
			}
			return new BlockDispatcherUnsplatProcN(parameterCount, attributesAndArity, sourcePath, sourceLine);
		}

		internal static LambdaExpression CreateLambda(Expression body, string name, ICollection<ParameterExpression> parameters, int parameterCount, BlockSignatureAttributes attributes)
		{
			if ((attributes & BlockSignatureAttributes.HasUnsplatParameter) == 0)
			{
				if ((attributes & BlockSignatureAttributes.HasProcParameter) == 0)
				{
					switch (parameterCount)
					{
					case 0:
						return Expression.Lambda<Func<BlockParam, object, object>>(body, name, parameters);
					case 1:
						return Expression.Lambda<Func<BlockParam, object, object, object>>(body, name, parameters);
					case 2:
						return Expression.Lambda<Func<BlockParam, object, object, object, object>>(body, name, parameters);
					case 3:
						return Expression.Lambda<Func<BlockParam, object, object, object, object, object>>(body, name, parameters);
					case 4:
						return Expression.Lambda<Func<BlockParam, object, object, object, object, object, object>>(body, name, parameters);
					default:
						return Expression.Lambda<Func<BlockParam, object, object[], object>>(body, name, parameters);
					}
				}
				return Expression.Lambda<Func<BlockParam, object, object[], Proc, object>>(body, name, parameters);
			}
			if ((attributes & BlockSignatureAttributes.HasProcParameter) == 0)
			{
				return Expression.Lambda<Func<BlockParam, object, object[], RubyArray, object>>(body, name, parameters);
			}
			return Expression.Lambda<Func<BlockParam, object, object[], RubyArray, Proc, object>>(body, name, parameters);
		}

		private static void CopyArgumentsFromSplattee(object[] args, int initializedArgCount, int parameterCount, out int nextArg, out int nextItem, IList splattee)
		{
			int num = Math.Min(initializedArgCount, parameterCount);
			int num2 = 0;
			while (num < parameterCount && num2 < splattee.Count)
			{
				args[num++] = splattee[num2++];
			}
			nextArg = num;
			nextItem = num2;
		}

		internal static object[] CopyArgumentsFromSplattee(object[] args, int initializedArgCount, IList splattee)
		{
			int nextArg;
			int nextItem;
			CopyArgumentsFromSplattee(args, initializedArgCount, args.Length, out nextArg, out nextItem, splattee);
			return args;
		}

		internal static void CreateArgumentsFromSplattee(int parameterCount, out int nextArg, out int nextItem, ref object[] args, IList splattee)
		{
			int initializedArgCount = args.Length;
			if (args.Length < parameterCount)
			{
				Array.Resize(ref args, parameterCount);
			}
			CopyArgumentsFromSplattee(args, initializedArgCount, parameterCount, out nextArg, out nextItem, splattee);
		}

		internal static object[] CreateArgumentsFromSplatteeAndRhs(int parameterCount, object[] args, IList splattee, object rhs)
		{
			int nextArg;
			int nextItem;
			CreateArgumentsFromSplattee(parameterCount, out nextArg, out nextItem, ref args, splattee);
			if (nextArg < args.Length)
			{
				args[nextArg++] = rhs;
			}
			return args;
		}
	}
	internal abstract class BlockDispatcher<T> : BlockDispatcher where T : class
	{
		protected T _block;

		public override Delegate Method
		{
			get
			{
				return (Delegate)(object)_block;
			}
		}

		public BlockDispatcher(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
			: base(attributesAndArity, sourcePath, sourceLine)
		{
		}

		internal override BlockDispatcher SetMethod(object method)
		{
			_block = (T)method;
			return this;
		}
	}
}
