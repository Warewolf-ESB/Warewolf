using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToUInt16Action : ConvertToIntegerAction<ConvertToUInt16Action>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "System::UInt16";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToUInt16Validator;
			}
		}
	}
}
