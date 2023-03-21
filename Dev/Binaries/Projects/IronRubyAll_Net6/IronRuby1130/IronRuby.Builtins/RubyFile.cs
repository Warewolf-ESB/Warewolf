using System;
using System.IO;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public class RubyFile : RubyIO
	{
		public string Path { get; set; }

		public RubyFile(RubyContext context)
			: base(context)
		{
			Path = null;
		}

		public RubyFile(RubyContext context, string path, IOMode mode)
			: base(context, OpenFileStream(context, path, mode), mode)
		{
			Path = path;
		}

		public RubyFile(RubyContext context, Stream stream, int descriptor, IOMode mode)
			: base(context, stream, descriptor, mode)
		{
			Path = null;
		}

		public static Stream OpenFileStream(RubyContext context, string path, IOMode mode)
		{
			ContractUtils.RequiresNotNull(path, "path");
			FileAccess fileAccess = mode.ToFileAccess();
			FileMode mode2;
			if ((mode & IOMode.CreateIfNotExists) != 0)
			{
				if ((mode & IOMode.ErrorIfExists) != 0)
				{
					fileAccess |= FileAccess.Write;
					mode2 = FileMode.CreateNew;
				}
				else
				{
					mode2 = FileMode.OpenOrCreate;
				}
			}
			else
			{
				mode2 = FileMode.Open;
			}
			if ((mode & IOMode.Truncate) != 0 && (fileAccess & FileAccess.Write) == 0)
			{
				throw RubyExceptions.CreateEINVAL("cannot truncate a file opened for reading only");
			}
			if ((mode & IOMode.WriteAppends) != 0 && (fileAccess & FileAccess.Write) == 0)
			{
				throw RubyExceptions.CreateEINVAL("cannot append to a file opened for reading only");
			}
			if (string.IsNullOrEmpty(path))
			{
				throw RubyExceptions.CreateEINVAL();
			}
			Stream stream;
			if (path == "NUL")
			{
				stream = Stream.Null;
			}
			else
			{
				try
				{
					stream = context.DomainManager.Platform.OpenInputFileStream(path, mode2, fileAccess, FileShare.ReadWrite);
				}
				catch (FileNotFoundException)
				{
					throw RubyExceptions.CreateENOENT(string.Format("No such file or directory - {0}", path));
				}
				catch (DirectoryNotFoundException ex2)
				{
					throw RubyExceptions.CreateENOENT(ex2.Message, ex2);
				}
				catch (PathTooLongException ex3)
				{
					throw RubyExceptions.CreateENOENT(ex3.Message, ex3);
				}
				catch (IOException)
				{
					if ((mode & IOMode.ErrorIfExists) != 0)
					{
						throw RubyExceptions.CreateEEXIST(path);
					}
					throw;
				}
				catch (ArgumentException ex5)
				{
					throw RubyExceptions.CreateEINVAL(ex5.Message, ex5);
				}
			}
			if ((mode & IOMode.Truncate) != 0)
			{
				stream.SetLength(0L);
			}
			return stream;
		}
	}
}
