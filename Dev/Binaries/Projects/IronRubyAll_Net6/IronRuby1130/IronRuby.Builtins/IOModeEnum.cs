using System;
using System.IO;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	public static class IOModeEnum
	{
		public static bool IsClosed(this IOMode mode)
		{
			return (mode & IOMode.Closed) == IOMode.Closed;
		}

		public static bool CanRead(this IOMode mode)
		{
			return (mode & IOMode.WriteOnly) == 0;
		}

		public static bool CanWrite(this IOMode mode)
		{
			return (mode & IOMode.WriteOnly) != (IOMode)(((int)mode >> 1) & 1);
		}

		public static IOMode Close(this IOMode mode)
		{
			return (mode & ~IOMode.Closed) | IOMode.Closed;
		}

		public static IOMode CloseRead(this IOMode mode)
		{
			return (mode & ~IOMode.Closed) | (mode.CanWrite() ? IOMode.WriteOnly : IOMode.Closed);
		}

		public static IOMode CloseWrite(this IOMode mode)
		{
			return (mode & ~IOMode.Closed) | ((!mode.CanRead()) ? IOMode.Closed : IOMode.ReadOnly);
		}

		public static FileAccess ToFileAccess(this IOMode mode)
		{
			switch (mode & IOMode.Closed)
			{
			case IOMode.WriteOnly:
				return FileAccess.Write;
			case IOMode.ReadOnly:
				return FileAccess.Read;
			case IOMode.ReadWrite:
				return FileAccess.ReadWrite;
			default:
				throw RubyExceptions.CreateEINVAL("illegal access mode {0}", mode);
			}
		}

		public static IOMode Parse(MutableString mode)
		{
			return Parse(mode, IOMode.ReadOnly);
		}

		public static IOMode Parse(MutableString mode, IOMode defaultMode)
		{
			if (mode == null)
			{
				return defaultMode;
			}
			return Parse(mode.ToString());
		}

		public static IOMode Parse(string mode)
		{
			if (string.IsNullOrEmpty(mode))
			{
				throw IllegalMode(mode);
			}
			IOMode iOMode = IOMode.ReadOnly;
			int num = mode.Length - 1;
			bool flag = mode[num] == '+';
			if (flag)
			{
				num--;
			}
			if (num < 0)
			{
				throw IllegalMode(mode);
			}
			if (mode[num] == 'b')
			{
				iOMode |= IOMode.PreserveEndOfLines;
				num--;
			}
			if (num != 0)
			{
				throw IllegalMode(mode);
			}
			switch (mode[0])
			{
			case 'r':
				return iOMode | (flag ? IOMode.ReadWrite : IOMode.ReadOnly);
			case 'w':
				return iOMode | ((!flag) ? IOMode.WriteOnly : IOMode.ReadWrite) | IOMode.Truncate | IOMode.CreateIfNotExists;
			case 'a':
				return iOMode | ((!flag) ? IOMode.WriteOnly : IOMode.ReadWrite) | IOMode.WriteAppends | IOMode.CreateIfNotExists;
			default:
				throw IllegalMode(mode);
			}
		}

		internal static Exception IllegalMode(string modeString)
		{
			return RubyExceptions.CreateArgumentError("illegal access mode {0}", modeString);
		}
	}
}
