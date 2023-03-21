using System;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyObjectAttributeReaderDispatcherWithScope : AttributeDispatcher
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
				return new Func<CallSite, RubyScope, object, object>(Invoke);
			}
			return new Func<CallSite, object, object, object>(Invoke);
		}

		public object Invoke<TScope>(CallSite callSite, TScope scope, object self)
		{
			IRubyObject rubyObject = self as IRubyObject;
			if (rubyObject != null && rubyObject.ImmediateClass.Version.Method == Version)
			{
				RubyInstanceData rubyInstanceData = rubyObject.TryGetInstanceData();
				if (rubyInstanceData == null)
				{
					return null;
				}
				return rubyInstanceData.GetInstanceVariable(Name);
			}
			return ((CallSite<Func<CallSite, TScope, object, object>>)callSite).Update(callSite, scope, self);
		}
	}
}
