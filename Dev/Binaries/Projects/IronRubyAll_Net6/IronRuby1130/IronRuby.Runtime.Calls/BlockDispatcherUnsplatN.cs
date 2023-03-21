using System;
using System.Collections;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	internal sealed class BlockDispatcherUnsplatN : BlockDispatcherN<Func<BlockParam, object, object[], RubyArray, object>>
	{
		internal BlockDispatcherUnsplatN(int parameterCount, BlockSignatureAttributes attributesAndArity, string sourcePath, int sourceLine)
			: base(parameterCount, attributesAndArity, sourcePath, sourceLine)
		{
		}

		public override object Invoke(BlockParam param, object self, Proc procArg)
		{
			return InvokeInternal(param, self, procArg, ArrayUtils.EmptyObjects);
		}

		public override object InvokeNoAutoSplat(BlockParam param, object self, Proc procArg, object arg1)
		{
			return InvokeInternal(param, self, procArg, new object[1] { arg1 });
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1)
		{
			if (_parameterCount > 0)
			{
				IList splattee = (arg1 as IList) ?? Protocols.ImplicitTrySplat(param.RubyContext, arg1) ?? new object[1] { arg1 };
				return InvokeSplatInternal(param, self, procArg, ArrayUtils.EmptyObjects, splattee);
			}
			return InvokeInternal(param, self, procArg, new object[1] { arg1 });
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2)
		{
			return InvokeInternal(param, self, procArg, new object[2] { arg1, arg2 });
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3)
		{
			return InvokeInternal(param, self, procArg, new object[3] { arg1, arg2, arg3 });
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4)
		{
			return InvokeInternal(param, self, procArg, new object[4] { arg1, arg2, arg3, arg4 });
		}

		public override object Invoke(BlockParam param, object self, Proc procArg, object[] args)
		{
			return InvokeInternal(param, self, procArg, args);
		}

		private object InvokeInternal(BlockParam param, object self, Proc procArg, object[] args)
		{
			if (args.Length < _parameterCount)
			{
				Array.Resize(ref args, _parameterCount);
				return _block(param, self, args, RubyOps.MakeArray0());
			}
			if (args.Length == _parameterCount)
			{
				return _block(param, self, args, RubyOps.MakeArray0());
			}
			if (_parameterCount == 0)
			{
				return _block(param, self, ArrayUtils.EmptyObjects, RubyOps.MakeArrayN(args));
			}
			object[] array = new object[_parameterCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = args[i];
			}
			RubyArray rubyArray = new RubyArray(args.Length - _parameterCount);
			for (int j = _parameterCount; j < args.Length; j++)
			{
				rubyArray.Add(args[j]);
			}
			return _block(param, self, array, rubyArray);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, IList splattee)
		{
			if (splattee.Count == 1)
			{
				return Invoke(param, self, procArg, splattee[0]);
			}
			return InvokeSplatInternal(param, self, procArg, ArrayUtils.EmptyObjects, splattee);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, IList splattee)
		{
			if (splattee.Count == 0)
			{
				return Invoke(param, self, procArg, arg1);
			}
			return InvokeSplatInternal(param, self, procArg, new object[1] { arg1 }, splattee);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, IList splattee)
		{
			return InvokeSplatInternal(param, self, procArg, new object[2] { arg1, arg2 }, splattee);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, IList splattee)
		{
			return InvokeSplatInternal(param, self, procArg, new object[3] { arg1, arg2, arg3 }, splattee);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object arg1, object arg2, object arg3, object arg4, IList splattee)
		{
			return InvokeSplatInternal(param, self, procArg, new object[4] { arg1, arg2, arg3, arg4 }, splattee);
		}

		public override object InvokeSplat(BlockParam param, object self, Proc procArg, object[] args, IList splattee)
		{
			return InvokeSplatInternal(param, self, procArg, args, splattee);
		}

		public override object InvokeSplatRhs(BlockParam param, object self, Proc procArg, object[] args, IList splattee, object rhs)
		{
			RubyArray rubyArray = new RubyArray(splattee.Count + 1);
			rubyArray.AddRange(splattee);
			rubyArray.Add(rhs);
			splattee = rubyArray;
			return InvokeSplatInternal(param, self, procArg, args, rubyArray);
		}

		private object InvokeSplatInternal(BlockParam param, object self, Proc procArg, object[] args, IList splattee)
		{
			int num = args.Length;
			int nextArg;
			int nextItem;
			BlockDispatcher.CreateArgumentsFromSplattee(_parameterCount, out nextArg, out nextItem, ref args, splattee);
			RubyArray rubyArray = new RubyArray();
			while (nextArg < num)
			{
				rubyArray.Add(args[nextArg++]);
			}
			while (nextItem < splattee.Count)
			{
				rubyArray.Add(splattee[nextItem++]);
			}
			return _block(param, self, args, rubyArray);
		}
	}
}
