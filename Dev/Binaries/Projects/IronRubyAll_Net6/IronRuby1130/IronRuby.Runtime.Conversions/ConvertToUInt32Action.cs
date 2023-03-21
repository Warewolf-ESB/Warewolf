using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToUInt32Action : ConvertToIntegerAction<ConvertToUInt32Action>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "System::UInt32";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToUInt32Validator;
			}
		}
	}
}
