using System;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyObjectAttributeWriterDispatcherWithScope<T0> : AttributeDispatcher
	{
		internal override void Initialize(string name, int version)
		{
			Name = name;
			Version = version;
		}

		public override object CreateDelegate(bool isUntyped)
		{
			if (!isUntyped)
			{
				return new Func<CallSite, RubyScope, object, T0, object>(Invoke);
			}
			return new Func<CallSite, object, object, T0, object>(Invoke);
		}

		public object Invoke<TScope>(CallSite callSite, TScope scope, object self, T0 arg0)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				object obj = arg0;
				rubyObject.ImmediateClass.Context.SetInstanceVariable(rubyObject, Name, obj);
				return obj;
			}
			return ((CallSite<Func<CallSite, TScope, object, T0, object>>)callSite).Update(callSite, scope, self, arg0);
		}
	}
}
