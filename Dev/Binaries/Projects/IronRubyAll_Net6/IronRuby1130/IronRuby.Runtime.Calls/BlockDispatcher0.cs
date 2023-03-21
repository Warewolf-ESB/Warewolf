using System;
using System.Collections;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls
{
	internal sealed class BlockDispatcher0 : BlockDispatcher<Func<BlockParam, object, object>>
	{
		public override int ParameterCount
		{
			get
			{
				return 0;
			}
		}

		public BlockDispatcher0(BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
			: base(attributesAndArity, sourcePath, sourceLine)
		{
		}

		public override object Invoke(BlockParam param, object self, Proc procArg)
		{
			return _block(param, self);
		}

		public override object InvokeNoAutoSplat(BlockParam param, object self, Proc procArg, object arg1)
		{
			return _block(param, self);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1)
		{
			return _block(param, self);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2)
		{
			return _block(param, self);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3)
		{
			return _block(param, self);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4)
		{
			return _block(param, self);
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object[] args)
		{
			return _block(param, self);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, IList splattee)
		{
			return _block(param, self);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, IList splattee)
		{
			return _block(param, self);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, IList splattee)
		{
			return _block(param, self);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, IList splattee)
		{
			return _block(param, self);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList splattee)
		{
			return _block(param, self);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object[] args, IList splattee)
		{
			return _block(param, self);
		}

		public override object InvokeSplatRhs(BlockParam param, object self, Proc procArg, object[] args, IList splattee, object rhs)
		{
			return _block(param, self);
		}
	}
}
