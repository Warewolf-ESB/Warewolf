using System;
using System.Collections;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	internal sealed class BlockDispatcher1 : BlockDispatcher<Func<BlockParam, object, object, object>>
	{
		public override int ParameterCount
		{
			get
			{
				return 1;
			}
		}

		public BlockDispatcher1(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
			: base(attributesAndArity, sourcePath, sourceLine)
		{
		}

		public override object Invoke(BlockParam param, object self, Proc procArg)
		{
			return _block(param, self, null);
		}

		public override object InvokeNoAutoSplat(BlockParam param, object self, Proc procArg, object arg1)
		{
			return _block(param, self, arg1);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1)
		{
			return _block(param, self, arg1);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2)
		{
			return _block(param, self, arg1);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3)
		{
			return _block(param, self, arg1);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4)
		{
			return _block(param, self, arg1);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object[] args)
		{
			return _block(param, self, args[0]);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, IList splattee)
		{
			return _block(param, self, (splattee.Count > 0) ? splattee[0] : null);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, IList splattee)
		{
			return _block(param, self, arg1);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, IList splattee)
		{
			return _block(param, self, arg1);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, IList splattee)
		{
			return _block(param, self, arg1);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList splattee)
		{
			return _block(param, self, arg1);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object[] args, IList splattee)
		{
			return _block(param, self, args[0]);
		}

		public override object InvokeSplatRhs(BlockParam param, object self, Proc procArg, object[] args, IList splattee, object rhs)
		{
			return _block(param, self, (args.Length > 0) ? args[0] : ((splattee.Count > 0) ? splattee[0] : rhs));
		}
	}
}
