using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.FileControl
{
	[RubyModule("Fcntl")]
	public class Fcntl
	{
		[RubyConstant]
		public const int F_SETFL = 1;

		[RubyConstant]
		public const int O_CREAT = 256;

		[RubyConstant]
		public const int O_EXCL = 1024;

		[RubyConstant]
		public const int O_TRUNC = 512;

		[RubyConstant]
		public const int O_APPEND = 8;

		[RubyConstant]
		public const int O_NONBLOCK = 1;

		[RubyConstant]
		public const int O_RDONLY = 0;

		[RubyConstant]
		public const int O_RDWR = 2;

		[RubyConstant]
		public const int O_WRONLY = 1;

		[RubyConstant]
		public const int O_ACCMODE = 3;
	}
}
