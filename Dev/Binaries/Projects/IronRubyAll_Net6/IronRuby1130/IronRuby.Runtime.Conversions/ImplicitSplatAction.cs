using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ImplicitSplatAction : SplatAction<ImplicitSplatAction>
	{
		protected sealed override string ToMethodName
		{
			get
			{
				return Symbols.ToAry;
			}
		}

		protected sealed override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToArrayValidator;
			}
		}
	}
}
