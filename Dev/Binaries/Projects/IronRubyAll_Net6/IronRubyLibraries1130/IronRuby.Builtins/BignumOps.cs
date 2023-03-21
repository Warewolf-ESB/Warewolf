using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;

namespace IronRuby.Builtins
{
	[RubyClass("Bignum", Extends = typeof(BigInteger), Inherits = typeof(Integer))]
	[UndefineMethod("new", IsStatic = true)]
	[HideMethod(">")]
	[Includes(new Type[] { typeof(ClrBigInteger) }, Copy = true)]
	[HideMethod(">=")]
	[HideMethod("<")]
	[HideMethod("<=")]
	public static class BignumOps
	{
		[RubyMethod("size")]
		public static int Size(BigInteger self)
		{
			return self.GetWordCount() * 4;
		}
	}
}
