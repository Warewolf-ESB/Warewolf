namespace IronRuby.Runtime
{
	internal sealed class VersionAndModule
	{
		internal static readonly VersionAndModule Default = new VersionAndModule(0, 0);

		internal readonly int Version;

		internal readonly int ModuleId;

		internal VersionAndModule(int version, int moduleId)
		{
			Version = version;
			ModuleId = moduleId;
		}
	}
}
