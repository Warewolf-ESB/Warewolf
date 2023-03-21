using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToInt16Action : ConvertToIntegerAction<ConvertToInt16Action>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "System::Int16";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToInt16Validator;
			}
		}
	}
}
