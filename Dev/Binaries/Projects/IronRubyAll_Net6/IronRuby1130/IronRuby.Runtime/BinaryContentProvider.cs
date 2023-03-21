using System.IO;
using Microsoft.Scripting;

namespace IronRuby.Runtime
{
	internal sealed class BinaryContentProvider : StreamContentProvider
	{
		private readonly byte[] _bytes;

		public BinaryContentProvider(byte[] bytes)
		{
			_bytes = bytes;
		}

		public override Stream GetStream()
		{
			return new MemoryStream(_bytes);
		}
	}
}
