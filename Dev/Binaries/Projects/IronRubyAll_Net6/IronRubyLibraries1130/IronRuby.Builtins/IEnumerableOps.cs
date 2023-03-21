using System;
using System.Collections;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule(Extends = typeof(IEnumerable), Restrictions = ModuleRestrictions.None)]
	[Includes(new Type[] { typeof(Enumerable) })]
	public static class IEnumerableOps
	{
		[RubyMethod("each", RubyMethodAttributes.PublicInstance)]
		public static object Each(BlockParam block, IEnumerable self)
		{
			foreach (object item in self)
			{
				if (block == null)
				{
					throw RubyExceptions.NoBlockGiven();
				}
				object blockResult;
				if (block.Yield(item, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}
	}
}
