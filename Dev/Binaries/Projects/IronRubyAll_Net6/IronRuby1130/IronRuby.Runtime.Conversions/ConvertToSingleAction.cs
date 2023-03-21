using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToSingleAction : ConvertToFloatingPointAction<ConvertToSingleAction>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "System::Single";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToSingleValidator;
			}
		}
	}
}
