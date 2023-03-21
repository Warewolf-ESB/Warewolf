using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToInt64Action : ConvertToIntegerAction<ConvertToInt64Action>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "System::Int64";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToInt64Validator;
			}
		}
	}
}
