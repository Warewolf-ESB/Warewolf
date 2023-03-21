using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public sealed class ExpressionQualifiedIsDefinedConstantSiteCache
	{
		internal volatile VersionAndModule Condition = VersionAndModule.Default;

		internal volatile bool Value;

		internal void Update(bool newValue, int newVersion, RubyModule newModule)
		{
			Value = newValue;
			Condition = new VersionAndModule(newVersion, newModule.Id);
		}
	}
}
