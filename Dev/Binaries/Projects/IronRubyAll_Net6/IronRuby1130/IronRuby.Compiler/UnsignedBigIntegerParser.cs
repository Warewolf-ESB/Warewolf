using System;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler
{
	public abstract class UnsignedBigIntegerParser
	{
		protected abstract int ReadDigit();

		public BigInteger ParseBinary(int digitCount)
		{
			return ParseBinarySuperPowers(digitCount, 1);
		}

		public BigInteger ParseHexadecimal(int digitCount)
		{
			return ParseBinarySuperPowers(digitCount, 4);
		}

		public BigInteger ParseDecimal(int digitCount)
		{
			return ParseAnyBase(digitCount, 10u, 1000000000u, 9);
		}

		public BigInteger ParseOctal(int digitCount)
		{
			ContractUtils.Requires(digitCount > 0, "digitCount");
			int num = digitCount * 3 % 96;
			int num2 = digitCount * 3 / 96;
			uint[] array = new uint[num2 * 3 + (num + 32 - 1) / 32];
			if (num <= 32)
			{
				uint num3 = ReadBinaryWord(num / 3, 3);
				array[array.Length - 1] = num3;
			}
			else if (num <= 64)
			{
				uint num4 = ReadBinaryWord((num - 32) / 3, 3);
				uint num5 = (uint)ReadDigit();
				uint num6 = ReadBinaryWord(10, 3);
				array[array.Length - 1] = (num4 << 1) | (num5 >> 2);
				array[array.Length - 2] = num6 | ((num5 & 3) << 30);
			}
			else
			{
				ReadOctalTriword(array, array.Length - 1, (num - 64) / 3);
			}
			for (int num7 = num2 * 3 - 1; num7 > 0; num7 -= 3)
			{
				ReadOctalTriword(array, num7, 10);
			}
			return new BigInteger(1, array);
		}

		private void ReadOctalTriword(uint[] result, int i, int digits)
		{
			uint num = ReadBinaryWord(digits, 3);
			uint num2 = (uint)ReadDigit();
			uint num3 = ReadBinaryWord(10, 3);
			uint num4 = (uint)ReadDigit();
			uint num5 = ReadBinaryWord(10, 3);
			result[i] = (num << 2) | (num2 >> 1);
			result[i - 1] = (num3 << 1) | ((num2 & 1) << 31) | (num4 >> 2);
			result[i - 2] = num5 | ((num4 & 3) << 30);
		}

		public BigInteger Parse(int digitCount, int @base)
		{
			ContractUtils.Requires(@base > 1, "base");
			switch (@base)
			{
			case 2:
				return ParseBinary(digitCount);
			case 4:
				return ParseBinarySuperPowers(digitCount, 2);
			case 8:
				return ParseOctal(digitCount);
			case 16:
				return ParseHexadecimal(digitCount);
			case 10:
				return ParseDecimal(digitCount);
			default:
				return ParseDefault(digitCount, (uint)@base);
			}
		}

		internal BigInteger ParseDefault(int digitCount, uint @base)
		{
			uint num = 1u;
			int num2 = 0;
			while (true)
			{
				ulong num3 = (ulong)num * (ulong)@base;
				if (num3 > uint.MaxValue)
				{
					break;
				}
				num = (uint)num3;
				num2++;
			}
			return ParseAnyBase(digitCount, @base, num, num2);
		}

		private BigInteger ParseAnyBase(int digitCount, uint @base, uint wordBase, int digitsPerWord)
		{
			ContractUtils.Requires(digitCount > 0, "digitCount");
			int resultSize = GetResultSize(digitCount, @base);
			int digitCount2 = digitCount % digitsPerWord;
			int num = digitCount / digitsPerWord;
			uint[] array = new uint[resultSize];
			array[0] = ReadWord(digitCount2, @base);
			int count = 1;
			for (int i = 0; i < num; i++)
			{
				count = MultiplyAdd(array, count, wordBase, ReadWord(digitsPerWord, @base));
			}
			return new BigInteger(1, array);
		}

		private int GetResultSize(int digitCount, uint @base)
		{
			try
			{
				return (int)Math.Ceiling(Math.Log(@base) * (double)digitCount);
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException("Too many digits", "digitCount");
			}
		}

		private BigInteger ParseBinarySuperPowers(int digitCount, int bitsPerDigit)
		{
			ContractUtils.Requires(digitCount > 0, "digitCount");
			int num = 32 / bitsPerDigit;
			int num2 = digitCount % num;
			int num3 = digitCount / num;
			uint[] array = new uint[num3 + (num2 + num - 1) / num];
			array[array.Length - 1] = ReadBinaryWord(num2, bitsPerDigit);
			for (int num4 = num3 - 1; num4 >= 0; num4--)
			{
				array[num4] = ReadBinaryWord(num, bitsPerDigit);
			}
			return new BigInteger(1, array);
		}

		private int MultiplyAdd(uint[] data, int count, uint x, uint carry)
		{
			ulong num = 0uL;
			for (int i = 0; i < count + 1; i++)
			{
				num = (ulong)((long)data[i] * (long)x + carry);
				data[i] = (uint)(num & 0xFFFFFFFFu);
				carry = (uint)(num >> 32);
			}
			if (num == 0)
			{
				return count;
			}
			return count + 1;
		}

		private uint ReadBinaryWord(int digitCount, int bitsPerDigit)
		{
			uint num = 0u;
			while (digitCount > 0)
			{
				num = (num << bitsPerDigit) | (uint)ReadDigit();
				digitCount--;
			}
			return num;
		}

		private uint ReadWord(int digitCount, uint @base)
		{
			uint num = 0u;
			while (digitCount > 0)
			{
				num = num * @base + (uint)ReadDigit();
				digitCount--;
			}
			return num;
		}
	}
}
