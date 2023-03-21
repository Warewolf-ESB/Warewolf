using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToIntAction : ConvertToIntegerValueAction<ConvertToIntAction>
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToInt;
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
