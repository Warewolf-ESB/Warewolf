using System;
using System.Collections;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	internal abstract class BlockDispatcherN<T> : BlockDispatcher<T> where T : class
	{
		protected readonly int _parameterCount;

		public override int ParameterCount
		{
			get
			{
				return _parameterCount;
			}
		}

		public BlockDispatcherN(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
			: base(attributesAndArity, sourcePath, sourceLine)
		{
			_parameterCount = parameterCount;
		}

		protected object[] MakeArray(object arg1)
		{
			object[] array = new object[_parameterCount];
			array[0] = arg1;
			return array;
		}

		protected object[] MakeArray(object arg1, object arg2)
		{
			object[] array = new object[_parameterCount];
			array[0] = arg1;
			array[1] = arg2;
			return array;
		}

		protected object[] MakeArray(object arg1, object arg2, object arg3)
		{
			object[] array = new object[_parameterCount];
			array[0] = arg1;
			array[1] = arg2;
			array[2] = arg3;
			return array;
		}

		protected object[] MakeArray(object arg1, object arg2, object arg3, object arg4)
		{
			object[] array = new object[_parameterCount];
			array[0] = arg1;
			array[1] = arg2;
			array[2] = arg3;
			array[3] = arg4;
			return array;
		}
	}
	internal sealed class BlockDispatcherN : BlockDispatcherN<Func<BlockParam, object, object[], object>>
	{
		internal BlockDispatcherN(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
			: base(parameterCount, attributesAndArity, sourcePath, sourceLine)
		{
		}

		public override object Invoke(BlockParam param, object self, Proc procArg)
		{
			return _block(param, self, new object[_parameterCount]);
		}

		public override object InvokeNoAutoSplat(BlockParam param, object self, Proc procArg, object arg1)
		{
			return _block(param, self, MakeArray(arg1));
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1)
		{
			IList splattee = (arg1 as IList) ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1) ?? new object[1] { arg1 };
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(new object[_parameterCount], 0, splattee));
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2)
		{
			return _block(param, self, MakeArray(arg1, arg2));
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3)
		{
			return _block(param, self, MakeArray(arg1, arg2, arg3));
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4)
		{
			return _block(param, self, MakeArray(arg1, arg2, arg3, arg4));
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object[] args)
		{
			if (args.Length < _parameterCount)
			{
				Array.Resize(ref args, _parameterCount);
			}
			return _block(param, self, args);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, IList splattee)
		{
			if (splattee.Count == 1)
			{
				return Invoke(param, self, procArg, splattee[0]);
			}
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(new object[_parameterCount], 0, splattee));
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, IList splattee)
		{
			if (splattee.Count == 0)
			{
				return Invoke(param, self, procArg, arg1);
			}
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(MakeArray(arg1), 1, splattee));
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, IList splattee)
		{
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(MakeArray(arg1, arg2), 2, splattee));
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, IList splattee)
		{
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3), 3, splattee));
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList splattee)
		{
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3, arg4), 4, splattee));
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object[] args, IList splattee)
		{
			int nextArg;
			int nextItem;
			BlockDispatcher.CreateArgumentsFromSplattee(_parameterCount, out nextArg, out nextItem, ref args, splattee);
			return _block(param, self, args);
		}

		public override object InvokeSplatRhs(BlockParam param, object self, Proc procArg, object[] args, IList splattee, object rhs)
		{
			return _block(param, self, BlockDispatcher.CreateArgumentsFromSplatteeAndRhs(_parameterCount, args, splattee, rhs));
		}
	}
}
