using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToBignumAction : ConvertToIntegerAction<ConvertToBignumAction>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "Bignum";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToBignumValidator;
			}
		}
	}
}
