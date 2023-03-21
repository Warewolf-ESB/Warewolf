using System.Linq.Expressions;

namespace IronRuby.Runtime.Conversions
{
	public struct Convertibility
	{
		public static readonly Convertibility NotConvertible = new Convertibility(false, null);

		public static readonly Convertibility AlwaysConvertible = new Convertibility(true, null);

		public readonly bool IsConvertible;

		public readonly Expression Assumption;

		public Convertibility(bool isConvertible, Expression assumption)
		{
			IsConvertible = isConvertible;
			Assumption = assumption;
		}
	}
}
