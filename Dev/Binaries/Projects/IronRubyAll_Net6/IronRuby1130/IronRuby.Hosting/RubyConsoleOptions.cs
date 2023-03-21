using Microsoft.Scripting.Hosting.Shell;

namespace IronRuby.Hosting
{
	public sealed class RubyConsoleOptions : ConsoleOptions
	{
		public string ChangeDirectory;

		public bool DisplayVersion;
	}
}
