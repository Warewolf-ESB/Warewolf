using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToByteAction : ConvertToIntegerAction<ConvertToByteAction>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "System::Byte";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToByteValidator;
			}
		}
	}
}
