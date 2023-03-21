using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.BigDecimal
{
	[RubyModule(Extends = typeof(Kernel))]
	public static class KernelOps
	{
		[RubyMethod("BigDecimal", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("BigDecimal", RubyMethodAttributes.PublicSingleton)]
		public static object CreateBigDecimal(RubyContext context, object self, [DefaultProtocol] MutableString value, [Optional] int n)
		{
			return BigDecimal.Create(BigDecimalOps.GetConfig(context), value.ConvertToString(), n);
		}
	}
}
