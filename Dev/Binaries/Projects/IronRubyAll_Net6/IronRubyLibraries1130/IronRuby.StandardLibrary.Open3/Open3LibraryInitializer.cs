using System;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Open3
{
	public sealed class Open3LibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			DefineGlobalModule("Open3", typeof(Open3), 8, null, LoadOpen3_Class, null, RubyModule.EmptyArray);
		}

		private static void LoadOpen3_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "popen3", 33, 131076u, new Func<RubyContext, object, MutableString, RubyArray>(Open3.OpenPipe));
		}
	}
}
