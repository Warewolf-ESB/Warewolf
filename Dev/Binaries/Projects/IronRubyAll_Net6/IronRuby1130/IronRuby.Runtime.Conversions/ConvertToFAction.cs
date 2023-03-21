using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToFAction : ConvertToFloatingPointAction<ConvertToFAction>
	{
		protected override string TargetTypeName
		{
			get
			{
				return "Float";
			}
		}

		protected override MethodInfo ConversionResultValidator
		{
			get
			{
				return Methods.ToDoubleValidator;
			}
		}
	}
}
