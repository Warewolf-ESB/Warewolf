using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyClass("File", Extends = typeof(RubyFile))]
	public static class RubyFileOps
	{
		[RubyModule("Constants")]
		public static class Constants
		{
			[RubyConstant]
			public static readonly int APPEND = 8;

			[RubyConstant]
			public static readonly int BINARY = 32768;

			[RubyConstant]
			public static readonly int CREAT = 256;

			[RubyConstant]
			public static readonly int EXCL = 1024;

			[RubyConstant]
			public static readonly int FNM_CASEFOLD = 8;

			[RubyConstant]
			public static readonly int FNM_DOTMATCH = 4;

			[RubyConstant]
			public static readonly int FNM_NOESCAPE = 1;

			[RubyConstant]
			public static readonly int FNM_PATHNAME = 2;

			[RubyConstant]
			public static readonly int FNM_SYSCASE = 8;

			[RubyConstant]
			public static readonly int LOCK_EX = 2;

			[RubyConstant]
			public static readonly int LOCK_NB = 4;

			[RubyConstant]
			public static readonly int LOCK_SH = 1;

			[RubyConstant]
			public static readonly int LOCK_UN = 8;

			[RubyConstant]
			public static readonly int NONBLOCK = 1;

			[RubyConstant]
			public static readonly int RDONLY = 0;

			[RubyConstant]
			public static readonly int RDWR = 2;

			[RubyConstant]
			public static readonly int TRUNC = 512;

			[RubyConstant]
			public static readonly int WRONLY = 1;
		}

		[RubyClass("Stat", Extends = typeof(FileSystemInfo), Inherits = typeof(object), BuildConfig = "!SILVERLIGHT")]
		[Includes(new Type[] { typeof(Comparable) })]
		public class RubyStatOps
		{
			internal class DeviceInfo : FileSystemInfo
			{
				private string _name;

				public override bool Exists
				{
					get
					{
						return true;
					}
				}

				public override string Name
				{
					get
					{
						return _name;
					}
				}

				internal DeviceInfo(string name)
				{
					_name = name;
				}

				public override void Delete()
				{
					throw new NotImplementedException();
				}
			}

			internal static FileSystemInfo Create(RubyFile file)
			{
				file.RequireInitialized();
				if (file.Path == null)
				{
					throw new NotSupportedException("TODO: cannot get file info for files without path");
				}
				return Create(file.Context, file.Path);
			}

			internal static FileSystemInfo Create(RubyContext context, MutableString path)
			{
				return Create(context, context.DecodePath(path));
			}

			internal static FileSystemInfo Create(RubyContext context, string path)
			{
				FileSystemInfo result;
				if (TryCreate(context, path, out result))
				{
					return result;
				}
				throw RubyExceptions.CreateENOENT("No such file or directory - {0}", path);
			}

			internal static bool TryCreate(RubyContext context, string path, out FileSystemInfo result)
			{
				PlatformAdaptationLayer platform = context.Platform;
				result = null;
				if (platform.FileExists(path))
				{
					result = new FileInfo(path);
				}
				else if (platform.DirectoryExists(path))
				{
					result = new DirectoryInfo(path);
				}
				else
				{
					if (!path.ToUpperInvariant().Equals("NUL"))
					{
						return false;
					}
					result = new DeviceInfo("NUL");
				}
				return true;
			}

			[RubyConstructor]
			public static FileSystemInfo Create(ConversionStorage<MutableString> toPath, RubyClass self, object path)
			{
				return Create(self.Context, Protocols.CastToPath(toPath, path));
			}

			[RubyMethod("<=>")]
			public static int Compare(FileSystemInfo self, [NotNull] FileSystemInfo other)
			{
				return self.LastWriteTime.CompareTo(other.LastWriteTime);
			}

			[RubyMethod("<=>")]
			public static object Compare(FileSystemInfo self, object other)
			{
				return null;
			}

			[RubyMethod("atime")]
			public static RubyTime AccessTime(FileSystemInfo self)
			{
				return new RubyTime(self.LastAccessTime);
			}

			[RubyMethod("blksize")]
			public static object BlockSize(FileSystemInfo self)
			{
				return null;
			}

			[RubyMethod("blockdev?")]
			public static bool IsBlockDevice(FileSystemInfo self)
			{
				return false;
			}

			[RubyMethod("blocks")]
			public static object Blocks(FileSystemInfo self)
			{
				return null;
			}

			[RubyMethod("chardev?")]
			public static bool IsCharDevice(FileSystemInfo self)
			{
				return false;
			}

			[RubyMethod("ctime")]
			public static RubyTime CreateTime(FileSystemInfo self)
			{
				return new RubyTime(self.CreationTime);
			}

			[RubyMethod("dev")]
			[RubyMethod("rdev")]
			public static object DeviceId(FileSystemInfo self)
			{
				return 3;
			}

			[RubyMethod("dev_major")]
			[RubyMethod("rdev_major")]
			public static object DeviceIdMajor(FileSystemInfo self)
			{
				return null;
			}

			[RubyMethod("rdev_minor")]
			[RubyMethod("dev_minor")]
			public static object DeviceIdMinor(FileSystemInfo self)
			{
				return null;
			}

			[RubyMethod("directory?")]
			public static bool IsDirectory(FileSystemInfo self)
			{
				return self is DirectoryInfo;
			}

			[RubyMethod("executable?")]
			[RubyMethod("executable_real?")]
			public static bool IsExecutable(FileSystemInfo self)
			{
				return self.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase);
			}

			[RubyMethod("identical?")]
			public static bool AreIdentical(RubyContext context, FileSystemInfo self, [NotNull] FileSystemInfo other)
			{
				if (self.Exists && other.Exists)
				{
					return context.Platform.PathComparer.Compare(self.FullName, other.FullName) == 0;
				}
				return false;
			}

			[RubyMethod("file?")]
			public static bool IsFile(FileSystemInfo self)
			{
				return self is FileInfo;
			}

			[RubyMethod("ftype")]
			public static MutableString FileType(FileSystemInfo self)
			{
				return MutableString.CreateAscii(IsFile(self) ? "file" : "directory");
			}

			[RubyMethod("gid")]
			public static int GroupId(FileSystemInfo self)
			{
				return 0;
			}

			[RubyMethod("grpowned?")]
			public static bool IsGroupOwned(FileSystemInfo self)
			{
				return false;
			}

			[RubyMethod("ino")]
			public static int Inode(FileSystemInfo self)
			{
				return 0;
			}

			[RubyMethod("inspect")]
			public static MutableString Inspect(RubyContext context, FileSystemInfo self)
			{
				return MutableString.CreateAscii(string.Format(CultureInfo.InvariantCulture, "#<File::Stat dev={0}, ino={1}, mode={2}, nlink={3}, uid={4}, gid={5}, rdev={6}, size={7}, blksize={8}, blocks={9}, atime={10}, mtime={11}, ctime={12}", context.Inspect(DeviceId(self)), context.Inspect(Inode(self)), context.Inspect(Mode(self)), context.Inspect(NumberOfLinks(self)), context.Inspect(UserId(self)), context.Inspect(GroupId(self)), context.Inspect(DeviceId(self)), context.Inspect(Size(self)), context.Inspect(BlockSize(self)), context.Inspect(Blocks(self)), context.Inspect(AccessTime(self)), context.Inspect(ModifiedTime(self)), context.Inspect(CreateTime(self))));
			}

			[RubyMethod("mode")]
			public static int Mode(FileSystemInfo self)
			{
				int num = ((self is FileInfo) ? 32768 : 16384);
				num |= 0x100;
				if ((self.Attributes & FileAttributes.ReadOnly) == 0)
				{
					num |= 0x80;
				}
				return num;
			}

			[RubyMethod("mtime")]
			public static RubyTime ModifiedTime(FileSystemInfo self)
			{
				return new RubyTime(self.LastWriteTime);
			}

			[RubyMethod("nlink")]
			public static int NumberOfLinks(FileSystemInfo self)
			{
				return 1;
			}

			[RubyMethod("owned?")]
			public static bool IsUserOwned(FileSystemInfo self)
			{
				return true;
			}

			[RubyMethod("pipe?")]
			public static bool IsPipe(FileSystemInfo self)
			{
				return false;
			}

			[RubyMethod("readable?")]
			[RubyMethod("readable_real?")]
			public static bool IsReadable(FileSystemInfo self)
			{
				return true;
			}

			[RubyMethod("setgid?")]
			public static bool IsSetGid(FileSystemInfo self)
			{
				return false;
			}

			[RubyMethod("setuid?")]
			public static bool IsSetUid(FileSystemInfo self)
			{
				return false;
			}

			[RubyMethod("size")]
			public static int Size(FileSystemInfo self)
			{
				if (self is DeviceInfo)
				{
					return 0;
				}
				FileInfo fileInfo = self as FileInfo;
				if (fileInfo != null)
				{
					return (int)fileInfo.Length;
				}
				return 0;
			}

			[RubyMethod("size?")]
			public static object NullableSize(FileSystemInfo self)
			{
				if (self is DeviceInfo)
				{
					return 0;
				}
				FileInfo fileInfo = self as FileInfo;
				if (fileInfo == null)
				{
					return null;
				}
				if (fileInfo.Length != 0)
				{
					return (int)fileInfo.Length;
				}
				return null;
			}

			[RubyMethod("socket?")]
			public static bool IsSocket(FileSystemInfo self)
			{
				return false;
			}

			[RubyMethod("sticky?")]
			public static object IsSticky(FileSystemInfo self)
			{
				return null;
			}

			[RubyMethod("symlink?")]
			public static bool IsSymLink(FileSystemInfo self)
			{
				return false;
			}

			[RubyMethod("uid")]
			public static int UserId(FileSystemInfo self)
			{
				return 0;
			}

			[RubyMethod("writable?")]
			[RubyMethod("writable_real?")]
			public static bool IsWritable(FileSystemInfo self)
			{
				return (self.Attributes & FileAttributes.ReadOnly) == 0;
			}

			[RubyMethod("zero?")]
			public static bool IsZeroLength(FileSystemInfo self)
			{
				if (self is DeviceInfo)
				{
					return true;
				}
				FileInfo fileInfo = self as FileInfo;
				if (fileInfo != null)
				{
					return fileInfo.Length == 0;
				}
				return false;
			}
		}

		private const char AltDirectorySeparatorChar = '\\';

		private const char DirectorySeparatorChar = '/';

		private const char PathSeparatorChar = ';';

		private const string NUL_VALUE = "NUL";

		internal const int WriteModeMask = 128;

		internal const int ReadWriteMode = 438;

		[RubyConstant]
		public static readonly MutableString ALT_SEPARATOR;

		[RubyConstant]
		public static readonly MutableString PATH_SEPARATOR;

		[RubyConstant]
		public static readonly MutableString SEPARATOR;

		[RubyConstant]
		public static readonly MutableString Separator;

		internal static readonly object UmaskKey;

		[RubyConstructor]
		public static RubyFile CreateFile(ConversionStorage<int?> toInt, ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<MutableString> toPath, ConversionStorage<MutableString> toStr, RubyClass self, object descriptorOrPath, [Optional] object optionsOrMode, [Optional] object optionsOrPermissions, [DefaultProtocol] IDictionary<object, object> options)
		{
			return Reinitialize(toInt, toHash, toPath, toStr, new RubyFile(self.Context), descriptorOrPath, optionsOrMode, optionsOrPermissions, options);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyFile Reinitialize(ConversionStorage<int?> toInt, ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<MutableString> toPath, ConversionStorage<MutableString> toStr, RubyFile self, object descriptorOrPath, [Optional] object optionsOrMode, [Optional] object optionsOrPermissions, [DefaultProtocol] IDictionary<object, object> options)
		{
			RubyContext context = self.Context;
			Protocols.TryConvertToOptions(toHash, ref options, ref optionsOrMode, ref optionsOrPermissions);
			CallSite<Func<CallSite, object, int?>> site = toInt.GetSite(ProtocolConversionAction<TryConvertToFixnumAction>.Make(toInt.Context));
			IOInfo info = default(IOInfo);
			if (optionsOrMode != Missing.Value)
			{
				int? num = site.Target(site, optionsOrMode);
				info = (num.HasValue ? new IOInfo((IOMode)num.Value) : IOInfo.Parse(context, Protocols.CastToString(toStr, optionsOrMode)));
			}
			int permission = 0;
			if (optionsOrPermissions != Missing.Value)
			{
				int? num2 = site.Target(site, optionsOrPermissions);
				if (!num2.HasValue)
				{
					throw RubyExceptions.CreateTypeConversionError(context.GetClassName(optionsOrPermissions), "Integer");
				}
				permission = num2.Value;
			}
			if (options != null)
			{
				info = info.AddOptions(toStr, options);
			}
			int? num3 = site.Target(site, descriptorOrPath);
			if (num3.HasValue)
			{
				RubyIOOps.Reinitialize(self, num3.Value, info);
			}
			else
			{
				Reinitialize(self, Protocols.CastToPath(toPath, descriptorOrPath), info, permission);
			}
			return self;
		}

		private static void Reinitialize(RubyFile file, MutableString path, IOInfo info, int permission)
		{
			string path2 = file.Context.DecodePath(path);
			Stream stream = RubyFile.OpenFileStream(file.Context, path2, info.Mode);
			file.Path = path2;
			file.Mode = info.Mode;
			file.SetStream(stream);
			file.SetFileDescriptor(file.Context.AllocateFileDescriptor(stream));
			if (info.HasEncoding)
			{
				file.ExternalEncoding = info.ExternalEncoding;
				file.InternalEncoding = info.InternalEncoding;
			}
		}

		static RubyFileOps()
		{
			Separator = SEPARATOR;
			UmaskKey = new object();
			ALT_SEPARATOR = MutableString.CreateAscii('\\'.ToString()).Freeze();
			SEPARATOR = MutableString.CreateAscii('/'.ToString()).Freeze();
			Separator = SEPARATOR;
			PATH_SEPARATOR = MutableString.CreateAscii(';'.ToString()).Freeze();
		}

		internal static bool IsDirectorySeparator(int c)
		{
			if (c != 47)
			{
				return c == 92;
			}
			return true;
		}

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		public static RuleGenerator Open()
		{
			return RubyIOOps.Open();
		}

		[RubyMethod("chmod")]
		public static int Chmod(RubyFile self, [DefaultProtocol] int permission)
		{
			self.RequireInitialized();
			if (self.Path == null)
			{
				throw new NotSupportedException("TODO: cannot chmod for files without path");
			}
			Chmod(self.Path, permission);
			return 0;
		}

		[RubyMethod("chmod", RubyMethodAttributes.PublicSingleton)]
		public static int Chmod(ConversionStorage<MutableString> toPath, RubyClass self, [DefaultProtocol] int permission, object path)
		{
			Chmod(self.Context.DecodePath(Protocols.CastToPath(toPath, path)), permission);
			return 1;
		}

		internal static void Chmod(string path, int permission)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if ((permission & 0x80) == 0)
			{
				File.SetAttributes(path, attributes | FileAttributes.ReadOnly);
			}
			else
			{
				File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
			}
		}

		[RubyMethod("chown")]
		public static int ChangeOwner(RubyFile self, [DefaultProtocol] int owner, [DefaultProtocol] int group)
		{
			return 0;
		}

		[RubyMethod("chown")]
		public static int ChangeOwner(RubyContext context, RubyFile self, object owner, object group)
		{
			if ((owner == null || owner is int) && (group == null || group is int))
			{
				return 0;
			}
			throw RubyExceptions.CreateUnexpectedTypeError(context, owner, "Fixnum");
		}

		[RubyMethod("chown", RubyMethodAttributes.PublicSingleton)]
		public static int ChangeOwner(RubyClass self, [DefaultProtocol] int owner, [DefaultProtocol] int group, [NotNull][DefaultProtocol] MutableString path)
		{
			return 0;
		}

		[RubyMethod("chown", RubyMethodAttributes.PublicSingleton)]
		public static int ChangeOwner(RubyContext context, RubyClass self, object owner, object group, [NotNull][DefaultProtocol] MutableString path)
		{
			if ((owner == null || owner is int) && (group == null || group is int))
			{
				return 0;
			}
			throw RubyExceptions.CreateUnexpectedTypeError(context, owner, "Fixnum");
		}

		[RubyMethod("umask", RubyMethodAttributes.PublicSingleton)]
		public static int GetUmask(RubyClass self, [DefaultProtocol] int mask)
		{
			int result = (int)self.Context.GetOrCreateLibraryData(UmaskKey, () => 0);
			self.Context.TrySetLibraryData(UmaskKey, CalculateUmask(mask));
			return result;
		}

		[RubyMethod("umask", RubyMethodAttributes.PublicSingleton)]
		public static int GetUmask(RubyClass self)
		{
			return (int)self.Context.GetOrCreateLibraryData(UmaskKey, () => 0);
		}

		private static int CalculateUmask(int mask)
		{
			return mask % 512 / 128 * 128;
		}

		[RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
		public static int Delete(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			string text = self.Context.DecodePath(Protocols.CastToPath(toPath, path));
			if (!self.Context.Platform.FileExists(text))
			{
				throw RubyExceptions.CreateENOENT("No such file or directory - {0}", text);
			}
			Delete(self.Context, text);
			return 1;
		}

		internal static void Delete(RubyContext context, string path)
		{
			try
			{
				context.Platform.DeleteFile(path, true);
			}
			catch (DirectoryNotFoundException)
			{
				throw RubyExceptions.CreateENOENT("No such file or directory - {0}", path);
			}
			catch (IOException ex2)
			{
				throw Errno.CreateEACCES(ex2.Message, ex2);
			}
		}

		[RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
		public static int Delete(ConversionStorage<MutableString> toPath, RubyClass self, params object[] paths)
		{
			for (int i = 0; i < paths.Length; i++)
			{
				MutableString path = (MutableString)paths[i];
				Delete(toPath, self, path);
			}
			return paths.Length;
		}

		[RubyMethod("truncate", BuildConfig = "!SILVERLIGHT")]
		public static int Truncate(RubyFile self, [DefaultProtocol] int size)
		{
			if (size < 0)
			{
				throw new InvalidError();
			}
			self.Length = size;
			return 0;
		}

		[RubyMethod("truncate", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static int Truncate(ConversionStorage<MutableString> toPath, RubyClass self, object path, [DefaultProtocol] int size)
		{
			if (size < 0)
			{
				throw new InvalidError();
			}
			using (RubyFile rubyFile = new RubyFile(self.Context, self.Context.DecodePath(Protocols.CastToPath(toPath, path)), IOMode.ReadWrite))
			{
				rubyFile.Length = size;
			}
			return 0;
		}

		[RubyMethod("rename", RubyMethodAttributes.PublicSingleton)]
		public static int Rename(ConversionStorage<MutableString> toPath, RubyClass self, object oldPath, object newPath)
		{
			RubyContext context = self.Context;
			string text = context.DecodePath(Protocols.CastToPath(toPath, oldPath));
			string text2 = context.DecodePath(Protocols.CastToPath(toPath, newPath));
			if (text.Length == 0 || text2.Length == 0)
			{
				throw RubyExceptions.CreateENOENT();
			}
			if (!context.Platform.FileExists(text) && !context.Platform.DirectoryExists(text))
			{
				throw RubyExceptions.CreateENOENT("No such file or directory - {0}", oldPath);
			}
			if (RubyUtils.ExpandPath(context.Platform, text) == RubyUtils.ExpandPath(context.Platform, text2))
			{
				return 0;
			}
			if (context.Platform.FileExists(text2))
			{
				Delete(context, text2);
			}
			try
			{
				context.Platform.MoveFileSystemEntry(text, text2);
			}
			catch (IOException ex)
			{
				throw Errno.CreateEACCES(ex.Message, ex);
			}
			return 0;
		}

		[RubyMethod("path", RubyMethodAttributes.PublicSingleton)]
		public static MutableString ToPath(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			return Protocols.CastToPath(toPath, path);
		}

		[RubyMethod("basename", RubyMethodAttributes.PublicSingleton)]
		public static MutableString BaseName(ConversionStorage<MutableString> toPath, RubyClass self, object path, [Optional][NotNull][DefaultProtocol] MutableString suffix)
		{
			return BaseName(Protocols.CastToPath(toPath, path), suffix);
		}

		private static MutableString BaseName(MutableString path, MutableString suffix)
		{
			if (path.IsEmpty)
			{
				return path;
			}
			string text = path.ConvertToString();
			string[] array = text.Split(new char[2] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 0)
			{
				return MutableString.CreateMutable(path.Encoding).Append((char)path.GetLastChar()).TaintBy(path);
			}
			if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				string text2 = array[0];
				if (text.Length >= 2 && IsDirectorySeparator(text[0]) && IsDirectorySeparator(text[1]))
				{
					if (array.Length <= 2)
					{
						return MutableString.CreateMutable(path.Encoding).Append('/').TaintBy(path);
					}
				}
				else if (text2.Length == 2 && Tokenizer.IsLetter(text2[0]) && text2[1] == ':' && array.Length <= 1)
				{
					MutableString mutableString = MutableString.CreateMutable(path.Encoding).TaintBy(path);
					if (text.Length > 2)
					{
						mutableString.Append(text[2]);
					}
					return mutableString;
				}
			}
			string text3 = array[array.Length - 1];
			if (MutableString.IsNullOrEmpty(suffix))
			{
				return MutableString.CreateMutable(text3, path.Encoding);
			}
			StringComparison comparisonType = ((Environment.OSVersion.Platform == PlatformID.Unix) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
			int count = text3.Length;
			if (suffix != null)
			{
				string text4 = suffix.ToString();
				if (text4.LastCharacter() == 42 && text4.Length > 1)
				{
					int num = text3.LastIndexOf(text4.Substring(0, text4.Length - 1), comparisonType);
					if (num >= 0 && num + text4.Length <= text3.Length)
					{
						count = num;
					}
				}
				else if (text3.EndsWith(text4, comparisonType))
				{
					count = text3.Length - text4.Length;
				}
			}
			return MutableString.CreateMutable(path.Encoding).Append(text3, 0, count).TaintBy(path);
		}

		[RubyMethod("dirname", RubyMethodAttributes.PublicSingleton)]
		public static MutableString DirName(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			return DirName(Protocols.CastToPath(toPath, path));
		}

		private static MutableString DirName(MutableString path)
		{
			string text = path.ConvertToString();
			string text2 = text;
			if (IsValidPath(text))
			{
				text = StripPathCharacters(text);
				text2 = Path.GetDirectoryName(text);
				if (text2 == null)
				{
					return MutableString.CreateMutable(text, path.Encoding);
				}
				string fileName = Path.GetFileName(text);
				if (!string.IsNullOrEmpty(fileName))
				{
					text2 = StripPathCharacters(text.Substring(0, text.LastIndexOf(fileName, StringComparison.Ordinal)));
				}
			}
			else if (text2.Length > 1)
			{
				text2 = "//";
			}
			text2 = (string.IsNullOrEmpty(text2) ? "." : text2);
			return MutableString.CreateMutable(text2, path.Encoding);
		}

		private static bool IsValidPath(string path)
		{
			foreach (char c in path)
			{
				if (c != '/' && c != '\\')
				{
					return true;
				}
			}
			return false;
		}

		private static string StripPathCharacters(string path)
		{
			int num = 0;
			int num2 = path.Length - 1;
			while (num2 > 0 && (path[num2] == '/' || path[num2] == '\\'))
			{
				num++;
				num2--;
			}
			if (num > 0)
			{
				num--;
				if (path.Length == 3 && path[1] == ':')
				{
					num--;
				}
				return path.Substring(0, path.Length - num - 1);
			}
			return path;
		}

		[RubyMethod("extname", RubyMethodAttributes.PublicSingleton)]
		public static MutableString GetExtension(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			MutableString mutableString = Protocols.CastToPath(toPath, path);
			return MutableString.Create(RubyUtils.GetExtension(mutableString.ConvertToString()), mutableString.Encoding).TaintBy(mutableString);
		}

		[RubyMethod("expand_path", RubyMethodAttributes.PublicSingleton)]
		public static MutableString ExpandPath(ConversionStorage<MutableString> toPath, RubyClass self, object path, object basePath)
		{
			RubyContext context = self.Context;
			string path2 = RubyUtils.ExpandPath(context.Platform, context.DecodePath(Protocols.CastToPath(toPath, path)), (basePath == null) ? context.Platform.CurrentDirectory : context.DecodePath(Protocols.CastToPath(toPath, basePath)), true);
			return self.Context.EncodePath(path2);
		}

		[RubyMethod("absolute_path", RubyMethodAttributes.PublicSingleton)]
		public static MutableString AbsolutePath(ConversionStorage<MutableString> toPath, RubyClass self, object path, object basePath)
		{
			RubyContext context = self.Context;
			string path2 = RubyUtils.ExpandPath(context.Platform, context.DecodePath(Protocols.CastToPath(toPath, path)), (basePath == null) ? context.Platform.CurrentDirectory : context.DecodePath(Protocols.CastToPath(toPath, basePath)), false);
			return self.Context.EncodePath(path2);
		}

		[RubyMethod("fnmatch?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("fnmatch", RubyMethodAttributes.PublicSingleton)]
		public static bool FnMatch(ConversionStorage<MutableString> toPath, object self, [NotNull][DefaultProtocol] MutableString pattern, object path, [Optional] int flags)
		{
			return Glob.FnMatch(pattern.ConvertToString(), Protocols.CastToPath(toPath, path).ConvertToString(), flags);
		}

		[RubyMethod("split", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray Split(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			MutableString path2 = Protocols.CastToPath(toPath, path);
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(DirName(path2));
			rubyArray.Add(BaseName(path2, null));
			return rubyArray;
		}

		[RubyMethod("join", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Join(ConversionStorage<MutableString> toPath, RubyClass self, params object[] parts)
		{
			MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Binary);
			Dictionary<object, bool> dictionary = null;
			Stack<object> stack = new Stack<object>();
			int num = 0;
			Push(stack, parts);
			while (stack.Count > 0)
			{
				object obj = stack.Pop();
				IList list = obj as IList;
				MutableString mutableString2;
				if (list != null)
				{
					if (list.Count == 0)
					{
						mutableString2 = MutableString.FrozenEmpty;
					}
					else
					{
						if (dictionary == null || !dictionary.ContainsKey(list))
						{
							if (dictionary == null)
							{
								dictionary = new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance);
							}
							dictionary.Add(list, true);
							Push(stack, list);
							continue;
						}
						mutableString2 = RubyUtils.InfiniteRecursionMarker;
					}
				}
				else
				{
					if (obj == null)
					{
						throw RubyExceptions.CreateTypeConversionError("NilClass", "String");
					}
					mutableString2 = Protocols.CastToPath(toPath, obj);
				}
				if (num > 0)
				{
					AppendDirectoryName(mutableString, mutableString2);
				}
				else
				{
					mutableString.Append(mutableString2);
				}
				num++;
			}
			return mutableString;
		}

		private static void Push(Stack<object> stack, IList values)
		{
			for (int num = values.Count - 1; num >= 0; num--)
			{
				stack.Push(values[num]);
			}
		}

		private static void AppendDirectoryName(MutableString result, MutableString name)
		{
			int charCount = result.GetCharCount();
			int num = charCount - 1;
			while (num >= 0 && IsDirectorySeparator(result.GetChar(num)))
			{
				num--;
			}
			if (num == charCount - 1)
			{
				if (!IsDirectorySeparator(name.GetFirstChar()))
				{
					result.Append('/');
				}
				result.Append(name);
			}
			else if (IsDirectorySeparator(name.GetFirstChar()))
			{
				result.Replace(num + 1, charCount - num - 1, name);
			}
			else
			{
				result.Append(name);
			}
		}

		[RubyMethod("readlink", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static bool Readlink(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			throw new NotImplementedError("readlink() function is unimplemented on this machine");
		}

		[RubyMethod("link", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static int Link(ConversionStorage<MutableString> toPath, RubyClass self, object oldPath, object newPath)
		{
			Protocols.CastToPath(toPath, oldPath);
			Protocols.CastToPath(toPath, newPath);
			throw new NotImplementedError("link not implemented");
		}

		[RubyMethod("symlink", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static object SymLink(RubyClass self, [NotNull][DefaultProtocol] MutableString path)
		{
			throw new NotImplementedError("symlnk() function is unimplemented on this machine");
		}

		[RubyMethod("atime")]
		public static RubyTime AccessTime(RubyContext context, RubyFile self)
		{
			return RubyStatOps.AccessTime(RubyStatOps.Create(self));
		}

		[RubyMethod("atime", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime AccessTime(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			return RubyStatOps.AccessTime(RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("ctime")]
		public static RubyTime CreateTime(RubyContext context, RubyFile self)
		{
			return RubyStatOps.CreateTime(RubyStatOps.Create(self));
		}

		[RubyMethod("ctime", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime CreateTime(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			return RubyStatOps.CreateTime(RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("mtime")]
		public static RubyTime ModifiedTime(RubyContext context, RubyFile self)
		{
			return RubyStatOps.ModifiedTime(RubyStatOps.Create(self));
		}

		[RubyMethod("mtime", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime ModifiedTime(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			return RubyStatOps.ModifiedTime(RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("utime", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static int UpdateTimes(ConversionStorage<MutableString> toPath, RubyClass self, [NotNull] RubyTime accessTime, [NotNull] RubyTime modifiedTime, object path)
		{
			string text = self.Context.DecodePath(Protocols.CastToPath(toPath, path));
			FileInfo fileInfo = new FileInfo(text);
			if (!fileInfo.Exists)
			{
				throw RubyExceptions.CreateENOENT("No such file or directory - {0}", text);
			}
			fileInfo.LastAccessTimeUtc = accessTime.ToUniversalTime();
			fileInfo.LastWriteTimeUtc = modifiedTime.ToUniversalTime();
			return 1;
		}

		[RubyMethod("utime", RubyMethodAttributes.PublicSingleton)]
		public static int UpdateTimes(ConversionStorage<MutableString> toPath, RubyClass self, object accessTime, object modifiedTime, params object[] paths)
		{
			RubyTime accessTime2 = MakeTime(self.Context, accessTime);
			RubyTime modifiedTime2 = MakeTime(self.Context, modifiedTime);
			for (int i = 0; i < paths.Length; i++)
			{
				MutableString path = (MutableString)paths[i];
				UpdateTimes(toPath, self, accessTime2, modifiedTime2, path);
			}
			return paths.Length;
		}

		private static RubyTime MakeTime(RubyContext context, object obj)
		{
			if (obj == null)
			{
				return new RubyTime(DateTime.Now);
			}
			if (obj is RubyTime)
			{
				return (RubyTime)obj;
			}
			if (obj is int)
			{
				return new RubyTime(RubyTime.Epoch.AddSeconds((int)obj));
			}
			if (obj is double)
			{
				return new RubyTime(RubyTime.Epoch.AddSeconds((double)obj));
			}
			string name = context.GetClassOf(obj).Name;
			throw RubyExceptions.CreateTypeConversionError(name, "time");
		}

		[RubyMethod("ftype", RubyMethodAttributes.PublicSingleton)]
		public static MutableString FileType(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			return RubyStatOps.FileType(RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
		}

		[RubyMethod("lstat", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("stat", RubyMethodAttributes.PublicSingleton)]
		public static FileSystemInfo Stat(ConversionStorage<MutableString> toPath, RubyClass self, object path)
		{
			return RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path));
		}

		[RubyMethod("stat")]
		[RubyMethod("lstat")]
		public static FileSystemInfo Stat(RubyFile self)
		{
			return RubyStatOps.Create(self);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyFile self)
		{
			return MutableString.CreateMutable(self.Context.GetPathEncoding()).Append("#<").Append(self.Context.GetClassOf(self).GetName(self.Context))
				.Append(':')
				.Append(self.Path)
				.Append(self.Closed ? " (closed)" : "")
				.Append('>');
		}

		[RubyMethod("to_path")]
		[RubyMethod("path")]
		public static MutableString GetPath(RubyFile self)
		{
			self.RequireInitialized();
			if (self.Path == null)
			{
				return null;
			}
			return self.Context.EncodePath(self.Path);
		}

		[RubyMethod("blockdev?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsBlockDevice(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsBlockDevice(toPath, self, path);
		}

		[RubyMethod("chardev?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsCharDevice(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsCharDevice(toPath, self, path);
		}

		[RubyMethod("directory?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsDirectory(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsDirectory(toPath, self, path);
		}

		[RubyMethod("executable?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("executable_real?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsExecutable(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsExecutable(toPath, self, path);
		}

		[RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
		public static bool Exists(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.Exists(toPath, self, path);
		}

		[RubyMethod("file?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsFile(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsFile(toPath, self, path);
		}

		[RubyMethod("grpowned?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsGroupOwned(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsGroupOwned(toPath, self, path);
		}

		[RubyMethod("identical?", RubyMethodAttributes.PublicSingleton)]
		public static bool AreIdentical(ConversionStorage<MutableString> toPath, RubyModule self, object path1, object path2)
		{
			return FileTest.AreIdentical(toPath, self, path1, path2);
		}

		[RubyMethod("owned?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsUserOwned(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsUserOwned(toPath, self, path);
		}

		[RubyMethod("pipe?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsPipe(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsPipe(toPath, self, path);
		}

		[RubyMethod("readable_real?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("readable?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsReadable(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsReadable(toPath, self, path);
		}

		[RubyMethod("setgid?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsSetGid(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsSetGid(toPath, self, path);
		}

		[RubyMethod("setuid?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsSetUid(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsSetUid(toPath, self, path);
		}

		[RubyMethod("size", RubyMethodAttributes.PublicSingleton)]
		public static int Size(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.Size(toPath, self, path);
		}

		[RubyMethod("size?", RubyMethodAttributes.PublicSingleton)]
		public static object NullableSize(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.NullableSize(toPath, self, path);
		}

		[RubyMethod("socket?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsSocket(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsSocket(toPath, self, path);
		}

		[RubyMethod("sticky?", RubyMethodAttributes.PublicSingleton)]
		public static object IsSticky(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsSticky(toPath, self, path);
		}

		[RubyMethod("symlink?", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static bool IsSymLink(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsSymLink(toPath, self, path);
		}

		[RubyMethod("writable_real?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("writable?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsWritable(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsWritable(toPath, self, path);
		}

		[RubyMethod("zero?", RubyMethodAttributes.PublicSingleton)]
		public static bool IsZeroLength(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.IsZeroLength(toPath, self, path);
		}
	}
}
