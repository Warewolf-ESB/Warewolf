using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(ClrFloat) }, Copy = true)]
	[RubyClass(Extends = typeof(float), Inherits = typeof(Numeric), Restrictions = ModuleRestrictions.None)]
	[Includes(new Type[] { typeof(Precision) })]
	public static class SingleOps
	{
		[RubyConstructor]
		public static float Create(RubyClass self, [DefaultProtocol] double value)
		{
			return (float)value;
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (Single)");
		}
	}
}
