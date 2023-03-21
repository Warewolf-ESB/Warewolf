using System;
using System.IO;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("FileTest")]
	public static class FileTest
	{
		[RubyMethod("blockdev?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("blockdev?", RubyMethodAttributes.PrivateInstance)]
		public static bool IsBlockDevice(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsBlockDevice(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("chardev?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("chardev?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsCharDevice(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsCharDevice(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("directory?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("directory?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsDirectory(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return DirectoryExists(self.Context, Protocols.CastToPath(toPath, path));
		}

		[RubyMethod("executable_real?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("executable?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("executable?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("executable_real?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsExecutable(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RunIfFileExists(self.Context, Protocols.CastToPath(toPath, path), (FileSystemInfo fsi) => RubyFileOps.RubyStatOps.IsExecutable(fsi));
		}

		[RubyMethod("exist?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("exists?", RubyMethodAttributes.PrivateInstance)]
		public static bool Exists(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			MutableString path2 = Protocols.CastToPath(toPath, path);
			if (!FileExists(self.Context, path2))
			{
				return DirectoryExists(self.Context, path2);
			}
			return true;
		}

		[RubyMethod("file?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("file?", RubyMethodAttributes.PrivateInstance)]
		public static bool IsFile(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileExists(self.Context, Protocols.CastToPath(toPath, path));
		}

		[RubyMethod("grpowned?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("grpowned?", RubyMethodAttributes.PrivateInstance)]
		public static bool IsGroupOwned(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsGroupOwned(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("identical?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("identical?", RubyMethodAttributes.PublicSingleton)]
		public static bool AreIdentical(ConversionStorage<MutableString> toPath, RubyModule self, object path1, object path2)
		{
			FileSystemInfo result;
			FileSystemInfo result2;
			if (RubyFileOps.RubyStatOps.TryCreate(self.Context, self.Context.DecodePath(Protocols.CastToPath(toPath, path1)), out result) && RubyFileOps.RubyStatOps.TryCreate(self.Context, self.Context.DecodePath(Protocols.CastToPath(toPath, path2)), out result2))
			{
				return RubyFileOps.RubyStatOps.AreIdentical(self.Context, result, result2);
			}
			return false;
		}

		[RubyMethod("owned?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("owned?", RubyMethodAttributes.PrivateInstance)]
		public static bool IsUserOwned(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsUserOwned(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("pipe?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("pipe?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsPipe(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsPipe(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("readable?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("readable_real?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("readable?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("readable_real?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsReadable(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RunIfFileExists(self.Context, Protocols.CastToPath(toPath, path), (FileSystemInfo fsi) => RubyFileOps.RubyStatOps.IsReadable(fsi));
		}

		[RubyMethod("setgid?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("setgid?", RubyMethodAttributes.PrivateInstance)]
		public static bool IsSetGid(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsSetGid(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("setuid?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("setuid?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsSetUid(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsSetUid(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("size", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("size", RubyMethodAttributes.PrivateInstance)]
		public static int Size(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.Size(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("size?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("size?", RubyMethodAttributes.PrivateInstance)]
		public static object NullableSize(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			FileSystemInfo result;
			if (RubyFileOps.RubyStatOps.TryCreate(self.Context, Protocols.CastToPath(toPath, path).ConvertToString(), out result))
			{
				return RubyFileOps.RubyStatOps.NullableSize(result);
			}
			return null;
		}

		[RubyMethod("socket?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("socket?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsSocket(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsSocket(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("sticky?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("sticky?", RubyMethodAttributes.PrivateInstance)]
		public static object IsSticky(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsSticky(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("symlink?", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("symlink?", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static bool IsSymLink(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RubyFileOps.RubyStatOps.IsSymLink(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("writable_real?", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("writable_real?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("writable?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("writable?", RubyMethodAttributes.PrivateInstance)]
		public static bool IsWritable(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return RunIfFileExists(self.Context, Protocols.CastToPath(toPath, path), (FileSystemInfo fsi) => RubyFileOps.RubyStatOps.IsWritable(fsi));
		}

		[RubyMethod("zero?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("zero?", RubyMethodAttributes.PrivateInstance)]
		public static bool IsZeroLength(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			string text = self.Context.DecodePath(Protocols.CastToPath(toPath, path));
			if (text.ToUpperInvariant() == "NUL")
			{
				return RubyFileOps.RubyStatOps.IsZeroLength(RubyFileOps.RubyStatOps.Create(self.Context, text));
			}
			if (self.Context.Platform.DirectoryExists(text) || !self.Context.Platform.FileExists(text))
			{
				return false;
			}
			return RubyFileOps.RubyStatOps.IsZeroLength(RubyFileOps.RubyStatOps.Create(self.Context, text));
		}

		internal static bool FileExists(RubyContext context, MutableString path)
		{
			return context.Platform.FileExists(context.DecodePath(path));
		}

		internal static bool DirectoryExists(RubyContext context, MutableString path)
		{
			return context.Platform.DirectoryExists(context.DecodePath(path));
		}

		internal static bool Exists(RubyContext context, MutableString path)
		{
			string path2 = context.DecodePath(path);
			if (!context.Platform.DirectoryExists(path2))
			{
				return context.Platform.FileExists(path2);
			}
			return true;
		}

		private static bool RunIfFileExists(RubyContext context, MutableString path, Func<FileSystemInfo, bool> del)
		{
			return RunIfFileExists(context, path.ConvertToString(), del);
		}

		private static bool RunIfFileExists(RubyContext context, string path, Func<FileSystemInfo, bool> del)
		{
			FileSystemInfo result;
			if (RubyFileOps.RubyStatOps.TryCreate(context, path, out result))
			{
				return del(result);
			}
			return false;
		}
	}
}
