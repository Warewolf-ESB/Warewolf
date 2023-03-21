using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ExplicitSplatAction : SplatAction<ExplicitSplatAction>
	{
		protected sealed override string ToMethodName
		{
			get
			{
				return Symbols.ToA;
			}
		}

		protected sealed override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToAValidator;
			}
		}
	}
}
