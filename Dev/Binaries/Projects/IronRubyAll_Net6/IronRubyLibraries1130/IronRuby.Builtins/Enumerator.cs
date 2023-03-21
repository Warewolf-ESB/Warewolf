using System;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyClass("Enumerator")]
	[Includes(new Type[] { typeof(Enumerable) })]
	public class Enumerator
	{
		internal sealed class Wrapper : IEnumerator
		{
			private readonly object _targetObject;

			private readonly string _targetName;

			private readonly object[] _targetArguments;

			public Wrapper(object targetObject, string targetName, object[] targetArguments)
			{
				_targetObject = targetObject;
				_targetName = targetName ?? "each";
				_targetArguments = targetArguments ?? ArrayUtils.EmptyObjects;
			}

			public object Each(RubyScope scope, BlockParam block)
			{
				return KernelOps.SendMessageOpt(scope, block, _targetObject, _targetName, _targetArguments);
			}
		}

		internal sealed class DelegateWrapper : IEnumerator
		{
			private readonly Func<RubyScope, BlockParam, object> _each;

			public DelegateWrapper(Func<RubyScope, BlockParam, object> each)
			{
				_each = each;
			}

			public object Each(RubyScope scope, BlockParam block)
			{
				return _each(scope, block);
			}
		}

		private IEnumerator _impl;

		public Enumerator()
			: this(null, null, null)
		{
		}

		internal Enumerator(IEnumerator impl)
		{
			_impl = impl;
		}

		internal Enumerator(Func<RubyScope, BlockParam, object> impl)
		{
			_impl = new DelegateWrapper(impl);
		}

		public Enumerator(object targetObject, string targetName, params object[] targetArguments)
		{
			Reinitialize(this, targetObject, targetName, targetArguments);
		}

		[RubyConstructor]
		public static Enumerator Create(RubyClass self, object targetObject, [Optional][DefaultProtocol] string targetName, params object[] targetArguments)
		{
			return Reinitialize(new Enumerator(), targetObject, targetName, targetArguments);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Enumerator Reinitialize(Enumerator self, object targetObject, [Optional][DefaultProtocol] string targetName, params object[] targetArguments)
		{
			self._impl = new Wrapper(targetObject, targetName, targetArguments);
			return self;
		}

		[RubyMethod("each")]
		public static object Each(RubyScope scope, BlockParam block, Enumerator self)
		{
			return self._impl.Each(scope, block);
		}
	}
}
