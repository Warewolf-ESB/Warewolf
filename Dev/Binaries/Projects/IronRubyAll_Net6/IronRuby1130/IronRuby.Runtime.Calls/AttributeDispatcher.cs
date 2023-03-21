using System;

namespace IronRuby.Runtime.Calls
{
	public abstract class AttributeDispatcher : MemberDispatcher
	{
		internal string Name;

		internal static AttributeDispatcher CreateRubyObjectWriterDispatcher(Type delegateType, string name, int version)
		{
			AttributeDispatcher attributeDispatcher = (AttributeDispatcher)MemberDispatcher.CreateDispatcher(delegateType, 1, true, false, version, null, MemberDispatcher.RubyObjectAttributeWriterDispatchersWithScope);
			if (attributeDispatcher != null)
			{
				attributeDispatcher.Initialize(name, version);
			}
			return attributeDispatcher;
		}

		internal abstract void Initialize(string name, int version);
	}
}
