using System;
using System.Collections;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	internal sealed class BlockDispatcherProcN : BlockDispatcherN<Func<BlockParam, object, object[], Proc, object>>
	{
		internal BlockDispatcherProcN(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
			: base(parameterCount, attributesAndArity, sourcePath, sourceLine)
		{
		}

		public override object Invoke(BlockParam param, object self, Proc procArg)
		{
			return _block(param, self, new object[_parameterCount], procArg);
		}

		public override object InvokeNoAutoSplat(BlockParam param, object self, Proc procArg, object arg1)
		{
			return _block(param, self, MakeArray(arg1), procArg);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1)
		{
			if (_parameterCount == 1)
			{
				return _block(param, self, MakeArray(arg1), procArg);
			}
			IList splattee = (arg1 as IList) ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1) ?? new object[1] { arg1 };
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(new object[_parameterCount], 0, splattee), procArg);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2)
		{
			return _block(param, self, MakeArray(arg1, arg2), procArg);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3)
		{
			return _block(param, self, MakeArray(arg1, arg2, arg3), procArg);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4)
		{
			return _block(param, self, MakeArray(arg1, arg2, arg3, arg4), procArg);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object[] args)
		{
			if (args.Length < _parameterCount)
			{
				Array.Resize(ref args, _parameterCount);
			}
			return _block(param, self, args, procArg);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, IList splattee)
		{
			if (splattee.Count == 1)
			{
				return Invoke(param, self, procArg, splattee[0]);
			}
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(new object[_parameterCount], 0, splattee), procArg);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, IList splattee)
		{
			if (splattee.Count == 0)
			{
				return Invoke(param, self, procArg, arg1);
			}
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(MakeArray(arg1), 1, splattee), procArg);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, IList splattee)
		{
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(MakeArray(arg1, arg2), 2, splattee), procArg);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, IList splattee)
		{
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3), 3, splattee), procArg);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList splattee)
		{
			return _block(param, self, BlockDispatcher.CopyArgumentsFromSplattee(MakeArray(arg1, arg2, arg3, arg4), 4, splattee), procArg);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object[] args, IList splattee)
		{
			int nextArg;
			int nextItem;
			BlockDispatcher.CreateArgumentsFromSplattee(_parameterCount, out nextArg, out nextItem, ref args, splattee);
			return _block(param, self, args, procArg);
		}

		public override object InvokeSplatRhs(BlockParam param, object self, Proc procArg, object[] args, IList splattee, object rhs)
		{
			return _block(param, self, BlockDispatcher.CreateArgumentsFromSplatteeAndRhs(_parameterCount, args, splattee, rhs), procArg);
		}
	}
}
