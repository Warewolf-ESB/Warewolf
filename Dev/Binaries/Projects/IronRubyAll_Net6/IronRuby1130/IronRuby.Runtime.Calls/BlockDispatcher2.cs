using System;
using System.Collections;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	internal sealed class BlockDispatcher2 : BlockDispatcher<Func<BlockParam, object, object, object, object>>
	{
		public override int ParameterCount
		{
			get
			{
				return 2;
			}
		}

		public BlockDispatcher2(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
			: base(attributesAndArity, sourcePath, sourceLine)
		{
		}

		public override object Invoke(BlockParam param, object self, Proc procArg)
		{
			return _block(param, self, null, null);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1)
		{
			IList list = (arg1 as IList) ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1);
			if (list != null)
			{
				switch (list.Count)
				{
				case 0:
					return _block(param, self, null, null);
				case 1:
					return _block(param, self, list[0], null);
				default:
					return _block(param, self, list[0], list[1]);
				}
			}
			return _block(param, self, arg1, null);
		}

		public override object InvokeNoAutoSplat(BlockParam param, object self, Proc procArg, object arg1)
		{
			return _block(param, self, arg1, null);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2)
		{
			return _block(param, self, arg1, arg2);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3)
		{
			return _block(param, self, arg1, arg2);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4)
		{
			return _block(param, self, arg1, arg2);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object[] args)
		{
			return _block(param, self, args[0], args[1]);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, IList splattee)
		{
			switch (splattee.Count)
			{
			case 0:
				return Invoke(param, self, procArg);
			case 1:
				return Invoke(param, self, procArg, splattee[0]);
			default:
				return Invoke(param, self, procArg, splattee[0], splattee[1]);
			}
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, IList splattee)
		{
			if (splattee.Count == 0)
			{
				return Invoke(param, self, procArg, arg1);
			}
			return Invoke(param, self, procArg, arg1, splattee[0]);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, IList splattee)
		{
			return _block(param, self, arg1, arg2);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, IList splattee)
		{
			return _block(param, self, arg1, arg2);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList splattee)
		{
			return _block(param, self, arg1, arg2);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object[] args, IList splattee)
		{
			return _block(param, self, args[0], args[1]);
		}

		public override object InvokeSplatRhs(BlockParam param, object self, Proc procArg, object[] args, IList splattee, object rhs)
		{
			args = BlockDispatcher.CreateArgumentsFromSplatteeAndRhs(2, args, splattee, rhs);
			return _block(param, self, args[0], args[1]);
		}
	}
}
