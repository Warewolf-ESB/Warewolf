using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToSByteAction : ConvertToIntegerAction<ConvertToSByteAction>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "System::SByte";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToSByteValidator;
			}
		}
	}
}
