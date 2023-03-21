using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	internal sealed class LosslessDecoderFallback : DecoderFallback
	{
		internal sealed class Buffer : DecoderFallbackBuffer
		{
			private readonly LosslessDecoderFallback _fallback;

			private int _index;

			public override int Remaining
			{
				get
				{
					return 1 - _index;
				}
			}

			internal Buffer(LosslessDecoderFallback fallback)
			{
				_fallback = fallback;
			}

			public override bool Fallback(byte[] bytesUnknown, int index)
			{
				if (_fallback.Track)
				{
					if (_fallback._invalidCharacters == null)
					{
						_fallback._invalidCharacters = new List<byte[]>();
					}
					_fallback._invalidCharacters.Add(ArrayUtils.Copy(bytesUnknown));
				}
				_index = 0;
				return true;
			}

			public override char GetNextChar()
			{
				if (Remaining > 0)
				{
					_index++;
					return '\uffff';
				}
				return '\0';
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

		internal const char InvalidCharacterPlaceholder = '\uffff';

		private List<byte[]> _invalidCharacters;

		public bool Track { get; set; }

		public List<byte[]> InvalidCharacters
		{
			get
			{
				return _invalidCharacters;
			}
		}

		public override int MaxCharCount
		{
			get
			{
				return 1;
			}
		}

		internal LosslessDecoderFallback()
		{
		}

		public override DecoderFallbackBuffer CreateFallbackBuffer()
		{
			return new Buffer(this);
		}
	}
}
