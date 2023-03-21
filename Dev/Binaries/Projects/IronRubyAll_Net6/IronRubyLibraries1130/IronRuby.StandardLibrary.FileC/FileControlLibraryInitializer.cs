using IronRuby.Builtins;

namespace IronRuby.StandardLibrary.FileControl
{
	public sealed class FileControlLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			DefineGlobalModule("Fcntl", typeof(Fcntl), 8, null, null, LoadFcntl_Constants, RubyModule.EmptyArray);
		}

		private static void LoadFcntl_Constants(RubyModule module)
		{
			LibraryInitializer.SetConstant(module, "F_SETFL", 1);
			LibraryInitializer.SetConstant(module, "O_ACCMODE", 3);
			LibraryInitializer.SetConstant(module, "O_APPEND", 8);
			LibraryInitializer.SetConstant(module, "O_CREAT", 256);
			LibraryInitializer.SetConstant(module, "O_EXCL", 1024);
			LibraryInitializer.SetConstant(module, "O_NONBLOCK", 1);
			LibraryInitializer.SetConstant(module, "O_RDONLY", 0);
			LibraryInitializer.SetConstant(module, "O_RDWR", 2);
			LibraryInitializer.SetConstant(module, "O_TRUNC", 512);
			LibraryInitializer.SetConstant(module, "O_WRONLY", 1);
		}
	}
}
