using System.Text;

namespace IronRuby.Runtime
{
	internal sealed class BinaryDecoderFallback : DecoderFallback
	{
		internal sealed class Buffer : DecoderFallbackBuffer
		{
			private int _index;

			private byte[] _bytes;

			public override int Remaining
			{
				get
				{
					return _bytes.Length - _index;
				}
			}

			internal Buffer()
			{
			}

			public override bool Fallback(byte[] bytesUnknown, int index)
			{
				_bytes = bytesUnknown;
				_index = 0;
				return true;
			}

			public override char GetNextChar()
			{
				if (Remaining <= 0)
				{
					return '\0';
				}
				return (char)_bytes[_index++];
			}

			public override bool MovePrevious()
			{
				if (_index == 0)
				{
					return false;
				}
				_index--;
				return true;
			}

			public override void Reset()
			{
				_index = 0;
			}
		}

		internal static readonly BinaryDecoderFallback Instance = new BinaryDecoderFallback();

		public override int MaxCharCount
		{
			get
			{
				return 1;
			}
		}

		private BinaryDecoderFallback()
		{
		}

		public override DecoderFallbackBuffer CreateFallbackBuffer()
		{
			return new Buffer();
		}
	}
}
