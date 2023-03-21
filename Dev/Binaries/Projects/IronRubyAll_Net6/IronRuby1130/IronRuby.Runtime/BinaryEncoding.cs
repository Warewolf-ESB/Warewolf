using System;
using System.Text;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public sealed class BinaryEncoding : Encoding
	{
		public static readonly Encoding Instance = new BinaryEncoding();

		public override string EncodingName
		{
			get
			{
				return "ASCII-8BIT";
			}
		}

		public override bool IsSingleByte
		{
			get
			{
				return true;
			}
		}

		public override string WebName
		{
			get
			{
				return "ASCII-8BIT";
			}
		}

		private BinaryEncoding()
			: base(0)
		{
		}

		public override int GetByteCount(char[] chars, int index, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (chars[index + i] > 'ÿ')
				{
					throw new EncoderFallbackException();
				}
			}
			return count;
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return count;
		}

		public override int GetMaxByteCount(int charCount)
		{
			return charCount;
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return byteCount;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			ContractUtils.RequiresArrayRange(chars, charIndex, charCount, "charIndex", "charCount");
			ContractUtils.RequiresArrayRange(bytes, byteIndex, charCount, "byteIndex", "charCount");
			try
			{
				for (int i = 0; i < charCount; i++)
				{
					bytes[byteIndex + i] = checked((byte)chars[charIndex + i]);
				}
				return charCount;
			}
			catch (OverflowException)
			{
				throw new EncoderFallbackException();
			}
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			ContractUtils.RequiresArrayRange(bytes, byteIndex, byteCount, "byteIndex", "byteCount");
			ContractUtils.RequiresArrayRange(chars, charIndex, byteCount, "charIndex", "byteCount");
			for (int i = 0; i < byteCount; i++)
			{
				chars[charIndex + i] = (char)bytes[byteIndex + i];
			}
			return byteCount;
		}
	}
}
