using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToUInt64Action : ConvertToIntegerAction<ConvertToUInt64Action>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "System::UInt64";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToUInt64Validator;
			}
		}
	}
}
