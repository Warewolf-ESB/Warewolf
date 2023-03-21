using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Dir", Inherits = typeof(object))]
	[Includes(new Type[] { typeof(Enumerable) })]
	public class RubyDir : RubyObject
	{
		private enum DirectoryOperation
		{
			Delete,
			Create,
			Open,
			ChangeDir
		}

		private MutableString _dirName;

		private string[] _rawEntries;

		private int _pos;

		internal PlatformAdaptationLayer Platform
		{
			get
			{
				return base.ImmediateClass.Context.Platform;
			}
		}

		private bool Closed
		{
			get
			{
				return _dirName == null;
			}
		}

		public RubyDir(RubyClass cls)
			: base(cls)
		{
		}

		public RubyDir(RubyClass cls, MutableString dirname)
			: base(cls)
		{
			Reinitialize(this, dirname);
		}

		[RubyConstructor]
		public static RubyDir Create(RubyClass self, [NotNull] MutableString dirname)
		{
			return new RubyDir(self, dirname);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyDir Reinitialize(RubyDir self, [NotNull] MutableString dirname)
		{
			self.Close();
			string path = self.ImmediateClass.Context.DecodePath(dirname);
			try
			{
				self._rawEntries = self.Platform.GetFileSystemEntries(path, "*");
			}
			catch (Exception ex)
			{
				throw ToRubyException(ex, path, DirectoryOperation.Open);
			}
			self._dirName = dirname.Clone();
			self._pos = -2;
			return self;
		}

		private static void SetCurrentDirectory(PlatformAdaptationLayer pal, string dir)
		{
			try
			{
				pal.CurrentDirectory = dir;
			}
			catch (Exception ex)
			{
				throw ToRubyException(ex, dir, DirectoryOperation.ChangeDir);
			}
		}

		[RubyMethod("chdir", RubyMethodAttributes.PublicSingleton)]
		public static object ChangeDirectory(ConversionStorage<MutableString> toPath, BlockParam block, RubyClass self, object dir)
		{
			MutableString mutableString = Protocols.CastToPath(toPath, dir);
			return ChangeDirectory(self.Context.Platform, self.Context.DecodePath(mutableString), mutableString, block);
		}

		[RubyMethod("chdir", RubyMethodAttributes.PublicSingleton)]
		public static object ChangeDirectory(BlockParam block, RubyClass self)
		{
			string homeDirectory = RubyUtils.GetHomeDirectory(self.Context.Platform);
			if (homeDirectory == null)
			{
				throw RubyExceptions.CreateArgumentError("HOME / USERPROFILE not set");
			}
			return ChangeDirectory(self.Context.Platform, homeDirectory, self.Context.EncodePath(homeDirectory), block);
		}

		private static object ChangeDirectory(PlatformAdaptationLayer pal, string strDir, MutableString dir, BlockParam block)
		{
			if (block == null)
			{
				SetCurrentDirectory(pal, strDir);
				return 0;
			}
			string currentDirectory = pal.CurrentDirectory;
			try
			{
				SetCurrentDirectory(pal, strDir);
				object blockResult;
				block.Yield(dir, out blockResult);
				return blockResult;
			}
			finally
			{
				SetCurrentDirectory(pal, currentDirectory);
			}
		}

		[RubyMethod("chroot", RubyMethodAttributes.PublicSingleton)]
		public static int ChangeRoot(object self)
		{
			throw new InvalidOperationException();
		}

		[RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
		public static bool Exists(ConversionStorage<MutableString> toPath, RubyModule self, object path)
		{
			return FileTest.DirectoryExists(self.Context, Protocols.CastToPath(toPath, path));
		}

		[RubyMethod("rmdir", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
		public static int RemoveDirectory(ConversionStorage<MutableString> toPath, RubyClass self, object dirname)
		{
			string path = self.Context.DecodePath(Protocols.CastToPath(toPath, dirname));
			try
			{
				self.Context.Platform.DeleteDirectory(path, false);
			}
			catch (Exception ex)
			{
				throw ToRubyException(ex, path, DirectoryOperation.Delete);
			}
			return 0;
		}

		[RubyMethod("entries", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetEntries(ConversionStorage<MutableString> toPath, RubyClass self, object dirname, [Optional] IDictionary<object, object> options)
		{
			return new RubyDir(self, Protocols.CastToPath(toPath, dirname)).GetEntries(self.Context);
		}

		[RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
		public static object ForEach(ConversionStorage<MutableString> toPath, BlockParam block, RubyClass self, object dirname)
		{
			return new RubyDir(self, Protocols.CastToPath(toPath, dirname)).EnumerateEntries(self.Context, block, null);
		}

		[RubyMethod("getwd", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("pwd", RubyMethodAttributes.PublicSingleton)]
		public static MutableString GetCurrentDirectory(RubyClass self)
		{
			return self.Context.EncodePath(RubyUtils.CanonicalizePath(self.Context.Platform.CurrentDirectory));
		}

		[RubyMethod("glob", RubyMethodAttributes.PublicSingleton)]
		public static object Glob([NotNull] BlockParam block, RubyClass self, [DefaultProtocol][NotNull] MutableString pattern, [Optional][DefaultProtocol] int flags)
		{
			foreach (MutableString match in IronRuby.Builtins.Glob.GetMatches(self.Context, pattern, flags))
			{
				object blockResult;
				if (block.Yield(match, out blockResult))
				{
					return blockResult;
				}
			}
			return null;
		}

		[RubyMethod("glob", RubyMethodAttributes.PublicSingleton)]
		public static object Glob(RubyClass self, [DefaultProtocol][NotNull] MutableString pattern, [Optional][DefaultProtocol] int flags)
		{
			RubyArray rubyArray = new RubyArray();
			foreach (MutableString match in IronRuby.Builtins.Glob.GetMatches(self.Context, pattern, flags))
			{
				rubyArray.Add(match);
			}
			return rubyArray;
		}

		[RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray Glob(RubyClass self, [DefaultProtocol][NotNullItems] params MutableString[] patterns)
		{
			RubyArray rubyArray = new RubyArray();
			foreach (MutableString pattern in patterns)
			{
				foreach (MutableString match in IronRuby.Builtins.Glob.GetMatches(self.Context, pattern, 0))
				{
					rubyArray.Add(match);
				}
			}
			return rubyArray;
		}

		[RubyMethod("mkdir", RubyMethodAttributes.PublicSingleton)]
		public static int MakeDirectory(ConversionStorage<MutableString> toPath, RubyClass self, object dirname, [Optional] object permissions)
		{
			PlatformAdaptationLayer platform = self.Context.Platform;
			string text = self.Context.DecodePath(Protocols.CastToPath(toPath, dirname));
			if (platform.FileExists(text) || platform.DirectoryExists(text))
			{
				throw RubyExceptions.CreateEEXIST(text);
			}
			string directoryName = platform.GetDirectoryName(text);
			if (!string.IsNullOrEmpty(directoryName) && !platform.DirectoryExists(directoryName))
			{
				throw RubyExceptions.CreateENOENT("No such file or directory - {0}", directoryName);
			}
			try
			{
				platform.CreateDirectory(text);
			}
			catch (Exception ex)
			{
				throw ToRubyException(ex, text, DirectoryOperation.Create);
			}
			return 0;
		}

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		public static object Open(ConversionStorage<MutableString> toPath, BlockParam block, RubyClass self, object dirname)
		{
			RubyDir rubyDir = new RubyDir(self, Protocols.CastToPath(toPath, dirname));
			try
			{
				object blockResult;
				block.Yield(rubyDir, out blockResult);
				return blockResult;
			}
			finally
			{
				Close(rubyDir);
			}
		}

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		public static RubyDir Open(ConversionStorage<MutableString> toPath, RubyClass self, object dirname)
		{
			return new RubyDir(self, Protocols.CastToPath(toPath, dirname));
		}

		[RubyMethod("close")]
		public static void Close(RubyDir self)
		{
			self.ThrowIfClosed();
			self.Close();
		}

		[RubyMethod("each")]
		public static object Each(RubyContext context, BlockParam block, RubyDir self)
		{
			return self.EnumerateEntries(context, block, self);
		}

		[RubyMethod("to_path")]
		[RubyMethod("path")]
		public static MutableString GetPath(RubyContext context, RubyDir self)
		{
			if (context.RubyOptions.Compatibility < RubyCompatibility.Default)
			{
				self.ThrowIfClosed();
			}
			else if (self.Closed)
			{
				return null;
			}
			return self._dirName.Clone();
		}

		[RubyMethod("tell")]
		[RubyMethod("pos")]
		public static int GetCurrentPosition(RubyDir self)
		{
			self.ThrowIfClosed();
			return self._pos + 2;
		}

		[RubyMethod("pos=")]
		public static int SetPosition(RubyDir self, int pos)
		{
			self.ThrowIfClosed();
			self._pos = pos - 2;
			return pos;
		}

		[RubyMethod("read")]
		public static MutableString Read(RubyContext context, RubyDir self)
		{
			self.ThrowIfClosed();
			if (self._pos + 1 > self._rawEntries.Length)
			{
				return null;
			}
			MutableString result = ((self._pos == -2) ? context.EncodePath(".") : ((self._pos != -1) ? context.EncodePath(context.Platform.GetFileName(self._rawEntries[self._pos])) : context.EncodePath("..")));
			self._pos++;
			return result;
		}

		[RubyMethod("rewind")]
		public static RubyDir Rewind(RubyDir self)
		{
			self.ThrowIfClosed();
			self._pos = -2;
			return self;
		}

		[RubyMethod("seek")]
		public static RubyDir Seek(RubyDir self, int pos)
		{
			self.ThrowIfClosed();
			if (pos < 0)
			{
				self._pos = -2;
			}
			else if (pos > self._rawEntries.Length + 2)
			{
				self._pos = self._rawEntries.Length;
			}
			else
			{
				self._pos = pos - 2;
			}
			return self;
		}

		private void Close()
		{
			_dirName = null;
			_rawEntries = null;
		}

		private void ThrowIfClosed()
		{
			if (Closed)
			{
				throw RubyExceptions.CreateIOError("closed directory");
			}
		}

		private RubyArray GetEntries(RubyContext context)
		{
			ThrowIfClosed();
			RubyArray rubyArray = new RubyArray(_rawEntries.Length + 2);
			rubyArray.Add(context.EncodePath("."));
			rubyArray.Add(context.EncodePath(".."));
			string[] rawEntries = _rawEntries;
			foreach (string path in rawEntries)
			{
				MutableString mutableString = context.TryEncodePath(context.Platform.GetFileName(path));
				if (mutableString != null)
				{
					rubyArray.Add(mutableString);
				}
			}
			return rubyArray;
		}

		private object EnumerateEntries(RubyContext context, BlockParam block, object defaultResult)
		{
			ThrowIfClosed();
			_pos = -2;
			foreach (object entry in GetEntries(context))
			{
				if (block == null)
				{
					throw RubyExceptions.NoBlockGiven();
				}
				_pos++;
				object blockResult;
				if (block.Yield(entry, out blockResult))
				{
					return blockResult;
				}
			}
			return defaultResult;
		}

		private static Exception ToRubyException(Exception ex, string path, DirectoryOperation op)
		{
			Type type = ex.GetType();
			switch (op)
			{
			case DirectoryOperation.ChangeDir:
				return RubyExceptions.CreateEINVAL(path);
			case DirectoryOperation.Open:
				return RubyExceptions.CreateENOENT("No such file or directory - {0}", path);
			case DirectoryOperation.Delete:
				if (ex is ArgumentException)
				{
					return RubyExceptions.CreateEINVAL(path);
				}
				if (ex is IOException)
				{
					return Errno.CreateEACCES(path);
				}
				break;
			case DirectoryOperation.Create:
				if (ex is ArgumentException)
				{
					return RubyExceptions.CreateEINVAL(path);
				}
				if (ex is IOException)
				{
					return RubyExceptions.CreateEEXIST(path);
				}
				break;
			}
			if (ex is UnauthorizedAccessException)
			{
				return Errno.CreateEACCES(path, ex);
			}
			return RubyExceptions.CreateSystemCallError("unknown scenario - {0}, {1}, {2}", type, path, op);
		}
	}
}
