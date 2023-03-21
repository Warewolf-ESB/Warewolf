using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToFixnumAction : ConvertToIntegerAction<ConvertToFixnumAction>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "Fixnum";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToFixnumValidator;
			}
		}
	}
}
