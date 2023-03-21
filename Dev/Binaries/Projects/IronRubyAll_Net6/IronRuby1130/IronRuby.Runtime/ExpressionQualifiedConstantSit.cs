using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public sealed class ExpressionQualifiedConstantSiteCache
	{
		internal volatile VersionAndModule Condition = VersionAndModule.Default;

		internal volatile object Value;

		internal void Update(object newValue, int newVersion, RubyModule newModule)
		{
			Value = newValue;
			Condition = new VersionAndModule(newVersion, newModule.Id);
		}
	}
}
