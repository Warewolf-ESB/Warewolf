using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToIAction : ConvertToIntegerValueAction<ConvertToIAction>
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToI;
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToIntegerValidator;
			}
		}
	}
}
