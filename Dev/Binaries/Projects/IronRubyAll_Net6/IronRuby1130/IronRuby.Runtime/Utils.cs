using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public static class Utils
	{
		private sealed class CheckDecoderFallback : DecoderFallback
		{
			internal sealed class Buffer : DecoderFallbackBuffer
			{
				private readonly CheckDecoderFallback _fallback;

				public override int Remaining
				{
					get
					{
						return 0;
					}
				}

				public Buffer(CheckDecoderFallback fallback)
				{
					_fallback = fallback;
				}

				public override bool Fallback(byte[] bytesUnknown, int index)
				{
					_fallback.HasInvalidCharacters = true;
					return true;
				}

				public override char GetNextChar()
				{
					return '\0';
				}

				public override bool MovePrevious()
				{
					return false;
				}
			}

			public bool HasInvalidCharacters { get; private set; }

			public override int MaxCharCount
			{
				get
				{
					return 1;
				}
			}

			public override DecoderFallbackBuffer CreateFallbackBuffer()
			{
				return new Buffer(this);
			}
		}

		private sealed class CheckEncoderFallback : EncoderFallback
		{
			internal sealed class Buffer : EncoderFallbackBuffer
			{
				private readonly CheckEncoderFallback _fallback;

				public override int Remaining
				{
					get
					{
						return 0;
					}
				}

				public Buffer(CheckEncoderFallback fallback)
				{
					_fallback = fallback;
				}

				public override bool Fallback(char charUnknown, int index)
				{
					_fallback.HasInvalidCharacters = true;
					return true;
				}

				public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
				{
					_fallback.HasInvalidCharacters = true;
					return true;
				}

				public override char GetNextChar()
				{
					return '\0';
				}

				public override bool MovePrevious()
				{
					return false;
				}
			}

			public bool HasInvalidCharacters { get; private set; }

			public override int MaxCharCount
			{
				get
				{
					return 1;
				}
			}

			public override EncoderFallbackBuffer CreateFallbackBuffer()
			{
				return new Buffer(this);
			}
		}

		internal const int MinListSize = 4;

		internal const int MinBufferSize = 16;

		public static readonly byte[] EmptyBytes = new byte[0];

		public static readonly char[] EmptyChars = new char[0];

		public static readonly MemberInfo[] EmptyMemberInfos = new MemberInfo[0];

		public static readonly Delegate[] EmptyDelegates = new Delegate[0];

		public static int IndexOf(this string[] array, string value, StringComparer comparer)
		{
			ContractUtils.RequiresNotNull(array, "array");
			ContractUtils.RequiresNotNull(value, "value");
			ContractUtils.RequiresNotNull(comparer, "comparer");
			for (int i = 0; i < array.Length; i++)
			{
				if (comparer.Equals(array[i], value))
				{
					return i;
				}
			}
			return -1;
		}

		public static bool IsAscii(this string str)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] > '\u007f')
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsAscii(this byte[] bytes, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (bytes[i] > 127)
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsAscii(this char[] str, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (str[i] > '\u007f')
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsAscii(this char[] str, int start, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (str[start + i] > '\u007f')
				{
					return false;
				}
			}
			return true;
		}

		internal static bool IsBinary(this string str)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] > 'ÿ')
				{
					return false;
				}
			}
			return true;
		}

		internal static int GetCharacterCount(this string str)
		{
			int num = 0;
			bool flag = false;
			foreach (char c in str)
			{
				if (c >= '\ud800')
				{
					if (c <= '\udbff')
					{
						flag = true;
					}
					else if (flag && c <= '\udfff')
					{
						num++;
						flag = false;
					}
				}
			}
			return str.Length - num;
		}

		internal static int GetCharacterCount(this char[] str, int count)
		{
			int num = 0;
			bool flag = false;
			if (count < str.Length)
			{
				str[count] = '\uffff';
			}
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				if (c >= '\ud800')
				{
					if (i >= count)
					{
						break;
					}
					if (c <= '\udbff')
					{
						flag = true;
					}
					else if (flag && c <= '\udfff')
					{
						num++;
						flag = false;
					}
				}
			}
			return str.Length - num;
		}

		public static string ToAsciiString(this string str)
		{
			return MutableString.AppendUnicodeRepresentation(new StringBuilder(), str, MutableString.Escape.NonAscii, -1, -1).ToString();
		}

		public static int LastCharacter(this string str)
		{
			if (str.Length != 0)
			{
				return str[str.Length - 1];
			}
			return -1;
		}

		internal static IEnumerable<byte> EnumerateAsBytes(char[] data, int count)
		{
			for (int i = 0; i < count; i++)
			{
				yield return (byte)data[i];
			}
		}

		internal static IEnumerable<byte> EnumerateAsBytes(string data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				yield return (byte)data[i];
			}
		}

		internal static IEnumerable<T> Enumerate<T>(T[] data, int count)
		{
			for (int i = 0; i < count; i++)
			{
				yield return data[i];
			}
		}

		internal static int GetExpandedSize<T>(T[] array, int minLength)
		{
			return Math.Max(minLength, Math.Max(1 + (array.Length << 1), (typeof(T) == typeof(object)) ? 4 : 16));
		}

		internal static void Resize<T>(ref T[] array, int minLength)
		{
			if (array.Length < minLength)
			{
				Array.Resize(ref array, GetExpandedSize(array, minLength));
			}
		}

		internal static void TrimExcess<T>(ref T[] data, int count)
		{
			if (IsSparse(count, data.Length))
			{
				Array.Resize(ref data, count);
			}
		}

		internal static bool IsSparse(int portionSize, int totalSize)
		{
			return (long)portionSize * 10L < (long)totalSize * 9L;
		}

		internal static void ResizeForInsertion<T>(ref T[] array, int itemCount, int index, int count)
		{
			int num = itemCount + count;
			T[] array2;
			if (array.Length < num)
			{
				array2 = new T[GetExpandedSize(array, num)];
				Array.Copy(array, 0, array2, 0, index);
			}
			else
			{
				array2 = array;
			}
			Array.Copy(array, index, array2, index + count, itemCount - index);
			array = array2;
		}

		internal static void Fill<T>(T[] array, int index, T item, int repeatCount)
		{
			for (int i = index; i < index + repeatCount; i++)
			{
				array[i] = item;
			}
		}

		private static void Fill(byte[] src, int srcStart, byte[] dst, int dstStart, int count, int repeatCount)
		{
			if (count == 1)
			{
				Fill(dst, dstStart, src[srcStart], repeatCount);
				return;
			}
			for (int i = 0; i < repeatCount; i++)
			{
				for (int j = 0; j < count; j++)
				{
					dst[dstStart++] = src[srcStart + j];
				}
			}
		}

		private static int GetByteCount(string str, int start, int count, Encoding encoding, out char[] chars)
		{
			chars = new char[count];
			str.CopyTo(start, chars, 0, count);
			return encoding.GetByteCount(chars, 0, chars.Length);
		}

		internal static T[] Concatenate<T>(T[] array1, T[] array2)
		{
			return Concatenate(array1, array1.Length, array2, array2.Length);
		}

		internal static T[] Concatenate<T>(params T[][] arrays)
		{
			int num = 0;
			foreach (T[] array in arrays)
			{
				num += array.Length;
			}
			T[] array2 = new T[num];
			num = 0;
			foreach (T[] array3 in arrays)
			{
				Array.Copy(array3, 0, array2, num, array3.Length);
				num += array3.Length;
			}
			return array2;
		}

		internal static T[] Concatenate<T>(T[] array1, int itemCount1, T[] array2, int itemCount2)
		{
			T[] array3 = new T[itemCount1 + itemCount2];
			Array.Copy(array1, 0, array3, 0, itemCount1);
			Array.Copy(array2, 0, array3, itemCount1, itemCount2);
			return array3;
		}

		internal static int Append<T>(ref T[] array, int itemCount, T item, int repeatCount)
		{
			Resize(ref array, itemCount + repeatCount);
			Fill(array, itemCount, item, repeatCount);
			return itemCount + repeatCount;
		}

		internal static int Append(ref char[] array, int itemCount, string other, int start, int count)
		{
			int num = itemCount + count;
			Resize(ref array, num);
			other.CopyTo(start, array, itemCount, count);
			return num;
		}

		internal static int Append<T>(ref T[] array, int itemCount, T[] other, int start, int count)
		{
			int num = itemCount + count;
			Resize(ref array, num);
			Array.Copy(other, start, array, itemCount, count);
			return num;
		}

		internal static int Append(ref byte[] array, int itemCount, string other, int start, int count, Encoding encoding)
		{
			char[] chars;
			int num = itemCount + GetByteCount(other, start, count, encoding, out chars);
			Resize(ref array, num);
			encoding.GetBytes(chars, 0, chars.Length, array, itemCount);
			return num;
		}

		internal static int Append(ref byte[] array, int itemCount, char[] other, int start, int count, Encoding encoding)
		{
			int num = itemCount + encoding.GetByteCount(other, start, count);
			Resize(ref array, num);
			encoding.GetBytes(other, start, count, array, itemCount);
			return num;
		}

		internal static int Append(ref byte[] array, int itemCount, char other, int repeatCount, Encoding encoding)
		{
			if (repeatCount == 0)
			{
				return itemCount;
			}
			char[] chars = new char[1] { other };
			int byteCount = encoding.GetByteCount(chars, 0, 1);
			int num = itemCount + byteCount * repeatCount;
			Resize(ref array, num);
			encoding.GetBytes(chars, 0, 1, array, itemCount);
			Fill(array, itemCount, array, itemCount + byteCount, byteCount, repeatCount - 1);
			return num;
		}

		internal static int InsertAt<T>(ref T[] array, int itemCount, int index, T other, int repeatCount)
		{
			ResizeForInsertion(ref array, itemCount, index, repeatCount);
			Fill(array, index, other, repeatCount);
			return itemCount + repeatCount;
		}

		internal static int InsertAt(ref char[] array, int itemCount, int index, string other, int start, int count)
		{
			ResizeForInsertion(ref array, itemCount, index, count);
			other.CopyTo(start, array, index, count);
			return itemCount + count;
		}

		internal static int InsertAt<T>(ref T[] array, int itemCount, int index, T[] other, int start, int count)
		{
			ResizeForInsertion(ref array, itemCount, index, count);
			Array.Copy(other, start, array, index, count);
			return itemCount + count;
		}

		internal static int InsertAt(ref byte[] array, int itemCount, int index, string other, int start, int count, Encoding encoding)
		{
			char[] chars;
			int byteCount = GetByteCount(other, start, count, encoding, out chars);
			ResizeForInsertion(ref array, itemCount, index, byteCount);
			encoding.GetBytes(chars, 0, chars.Length, array, itemCount);
			return itemCount + byteCount;
		}

		internal static int InsertAt(ref byte[] array, int itemCount, int index, char[] other, int start, int count, Encoding encoding)
		{
			int byteCount = encoding.GetByteCount(other, start, count);
			ResizeForInsertion(ref array, itemCount, index, byteCount);
			encoding.GetBytes(other, start, count, array, itemCount);
			return itemCount + byteCount;
		}

		internal static int InsertAt(ref byte[] array, int itemCount, int index, char other, int repeatCount, Encoding encoding)
		{
			if (repeatCount == 0)
			{
				return itemCount;
			}
			char[] chars = new char[1] { other };
			int byteCount = encoding.GetByteCount(chars, 0, 1);
			int num = byteCount * repeatCount;
			ResizeForInsertion(ref array, itemCount, index, num);
			encoding.GetBytes(chars, 0, 1, array, itemCount);
			Fill(array, itemCount, array, itemCount + byteCount, byteCount, repeatCount - 1);
			return itemCount + num;
		}

		internal static int Remove<T>(ref T[] array, int itemCount, int start, int count)
		{
			int num = itemCount - count;
			T[] array2;
			if (num > 16 && num < itemCount / 2)
			{
				array2 = new T[num];
				Array.Copy(array, 0, array2, 0, start);
			}
			else
			{
				array2 = array;
			}
			Array.Copy(array, start + count, array2, start, num - start);
			array = array2;
			return num;
		}

		internal static T[] GetSlice<T>(this T[] array, int start, int count)
		{
			T[] array2 = new T[count];
			Array.Copy(array, start, array2, 0, count);
			return array2;
		}

		internal static T[] GetSlice<T>(this T[] array, int arrayLength, int start, int count)
		{
			count = NormalizeCount(arrayLength, start, count);
			T[] array2 = new T[count];
			if (count > 0)
			{
				Array.Copy(array, start, array2, 0, count);
			}
			return array2;
		}

		internal static string GetSlice(this string str, int start, int count)
		{
			count = NormalizeCount(str.Length, start, count);
			if (count <= 0)
			{
				return string.Empty;
			}
			return str.Substring(start, count);
		}

		internal static string GetStringSlice(this char[] chars, int arrayLength, int start, int count)
		{
			count = NormalizeCount(arrayLength, start, count);
			if (count <= 0)
			{
				return string.Empty;
			}
			return new string(chars, start, count);
		}

		internal static int NormalizeCount(int arrayLength, int start, int count)
		{
			if (count > arrayLength - start)
			{
				if (start < arrayLength)
				{
					return arrayLength - start;
				}
				return 0;
			}
			return count;
		}

		internal static void NormalizeLastIndexOfIndices(int arrayLength, ref int start, ref int count)
		{
			if (start >= arrayLength)
			{
				count = arrayLength - (start - count + 1);
				start = arrayLength - 1;
			}
		}

		internal static int IndexOf(byte[] array, int arrayLength, char value, int start, int count)
		{
			int num = start + NormalizeCount(arrayLength, start, count);
			for (int i = start; i < num; i++)
			{
				if (array[i] == value)
				{
					return i;
				}
			}
			return -1;
		}

		internal static int IndexOf(char[] array, int arrayLength, byte value, int start, int count)
		{
			int num = start + NormalizeCount(arrayLength, start, count);
			for (int i = start; i < num; i++)
			{
				if (array[i] == value)
				{
					return i;
				}
			}
			return -1;
		}

		internal static int IndexOf(string str, byte value, int start, int count)
		{
			int num = start + NormalizeCount(str.Length, start, count);
			for (int i = start; i < num; i++)
			{
				if (str[i] == value)
				{
					return i;
				}
			}
			return -1;
		}

		internal static int IndexOf(byte[] array, int arrayLength, string str, int start, int count)
		{
			count = NormalizeCount(arrayLength, start, count);
			int num = start + count - str.Length;
			for (int i = start; i <= num; i++)
			{
				bool flag = true;
				for (int j = 0; j < str.Length; j++)
				{
					if (str[j] != array[i + j])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return i;
				}
			}
			return -1;
		}

		internal static int IndexOf(char[] array, int arrayLength, string str, int start, int count)
		{
			count = NormalizeCount(arrayLength, start, count);
			int num = start + count - str.Length;
			for (int i = start; i <= num; i++)
			{
				bool flag = true;
				for (int j = 0; j < str.Length; j++)
				{
					if (str[j] != array[i + j])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return i;
				}
			}
			return -1;
		}

		internal static int IndexOf(byte[] array, int arrayLength, byte[] bytes, int start, int count)
		{
			count = NormalizeCount(arrayLength, start, count);
			int num = start + count - bytes.Length;
			for (int i = start; i <= num; i++)
			{
				bool flag = true;
				for (int j = 0; j < bytes.Length; j++)
				{
					if (bytes[j] != array[i + j])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return i;
				}
			}
			return -1;
		}

		internal static int IndexOf(char[] array, int arrayLength, byte[] bytes, int start, int count)
		{
			count = NormalizeCount(arrayLength, start, count);
			int num = start + count - bytes.Length;
			for (int i = start; i <= num; i++)
			{
				bool flag = true;
				for (int j = 0; j < bytes.Length; j++)
				{
					if (bytes[j] != array[i + j])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return i;
				}
			}
			return -1;
		}

		internal static int IndexOf(string str, byte[] bytes, int start, int count)
		{
			count = NormalizeCount(str.Length, start, count);
			int num = start + count - bytes.Length;
			for (int i = start; i <= num; i++)
			{
				bool flag = true;
				for (int j = 0; j < bytes.Length; j++)
				{
					if (bytes[j] != str[i + j])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return i;
				}
			}
			return -1;
		}

		internal static int LastIndexOf(char[] array, int arrayLength, byte value, int start, int count)
		{
			NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
			int num = start - count;
			for (int num2 = start; num2 > num; num2--)
			{
				if (array[num2] == value)
				{
					return num2;
				}
			}
			return -1;
		}

		internal static int LastIndexOf(string str, byte value, int start, int count)
		{
			NormalizeLastIndexOfIndices(str.Length, ref start, ref count);
			int num = start - count;
			for (int num2 = start; num2 > num; num2--)
			{
				if (str[num2] == value)
				{
					return num2;
				}
			}
			return -1;
		}

		internal static int LastIndexOf(byte[] array, int arrayLength, string value, int start, int count)
		{
			NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
			int num = start - count + 1;
			if (value.Length == 0)
			{
				return start;
			}
			for (int num2 = start - value.Length + 1; num2 >= num; num2--)
			{
				bool flag = true;
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i] != array[num2 + i])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return num2;
				}
			}
			return -1;
		}

		internal static int LastIndexOf(char[] array, int arrayLength, string value, int start, int count)
		{
			NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
			int num = start - count + 1;
			if (value.Length == 0)
			{
				return start;
			}
			for (int num2 = start - value.Length + 1; num2 >= num; num2--)
			{
				bool flag = true;
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i] != array[num2 + i])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return num2;
				}
			}
			return -1;
		}

		internal static int LastIndexOf(byte[] array, int arrayLength, byte[] value, int start, int count)
		{
			NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
			int num = start - count + 1;
			if (value.Length == 0)
			{
				return start;
			}
			for (int num2 = start - value.Length + 1; num2 >= num; num2--)
			{
				bool flag = true;
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i] != array[num2 + i])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return num2;
				}
			}
			return -1;
		}

		internal static int LastIndexOf(char[] array, int arrayLength, byte[] value, int start, int count)
		{
			NormalizeLastIndexOfIndices(arrayLength, ref start, ref count);
			int num = start - count + 1;
			if (value.Length == 0)
			{
				return start;
			}
			for (int num2 = start - value.Length + 1; num2 >= num; num2--)
			{
				bool flag = true;
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i] != array[num2 + i])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return num2;
				}
			}
			return -1;
		}

		internal static int LastIndexOf(string str, byte[] value, int start, int count)
		{
			NormalizeLastIndexOfIndices(str.Length, ref start, ref count);
			int num = start - count + 1;
			if (value.Length == 0)
			{
				return start;
			}
			for (int num2 = start - value.Length + 1; num2 >= num; num2--)
			{
				bool flag = true;
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i] != str[num2 + i])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return num2;
				}
			}
			return -1;
		}

		internal static int ValueCompareTo(this byte[] array, int itemCount, byte[] other)
		{
			return array.ValueCompareTo(itemCount, other, other.Length);
		}

		internal static int ValueCompareTo(this byte[] array, int itemCount, byte[] other, int otherCount)
		{
			int num = itemCount;
			int result;
			if (num < otherCount)
			{
				result = -1;
			}
			else if (num > otherCount)
			{
				num = otherCount;
				result = 1;
			}
			else
			{
				result = 0;
			}
			for (int i = 0; i < num; i++)
			{
				if (array[i] != other[i])
				{
					return array[i] - other[i];
				}
			}
			return result;
		}

		internal static int ValueCompareTo(this char[] array, int itemCount, char[] other, int otherCount)
		{
			int num = itemCount;
			int result;
			if (num < otherCount)
			{
				result = -1;
			}
			else if (num > otherCount)
			{
				num = otherCount;
				result = 1;
			}
			else
			{
				result = 0;
			}
			for (int i = 0; i < num; i++)
			{
				if (array[i] != other[i])
				{
					return array[i] - other[i];
				}
			}
			return result;
		}

		internal static int ValueCompareTo(this char[] array, int itemCount, string other)
		{
			int num = itemCount;
			int result;
			if (num < other.Length)
			{
				result = -1;
			}
			else if (num > other.Length)
			{
				num = other.Length;
				result = 1;
			}
			else
			{
				result = 0;
			}
			for (int i = 0; i < num; i++)
			{
				if (array[i] != other[i])
				{
					return array[i] - other[i];
				}
			}
			return result;
		}

		internal static int ValueCompareTo(this byte[] array, int itemCount, string other)
		{
			int num = itemCount;
			int result;
			if (num < other.Length)
			{
				result = -1;
			}
			else if (num > other.Length)
			{
				num = other.Length;
				result = 1;
			}
			else
			{
				result = 0;
			}
			for (int i = 0; i < num; i++)
			{
				if (array[i] != other[i])
				{
					return array[i] - other[i];
				}
			}
			return result;
		}

		internal static int ValueCompareTo(this string str, string other)
		{
			int length = str.Length;
			int result;
			if (length < other.Length)
			{
				result = -1;
			}
			else if (length > other.Length)
			{
				length = other.Length;
				result = 1;
			}
			else
			{
				result = 0;
			}
			for (int i = 0; i < length; i++)
			{
				if (str[i] != other[i])
				{
					return str[i] - other[i];
				}
			}
			return result;
		}

		internal static bool SubstringEquals(string name, int start, int count, string other)
		{
			if (count != other.Length)
			{
				return false;
			}
			for (int i = 0; i < count; i++)
			{
				if (name[start + i] != other[i])
				{
					return false;
				}
			}
			return true;
		}

		internal static bool ValueEquals<T>(T[] array, int arrayCount, T[] other, int otherCount)
		{
			if (arrayCount != otherCount)
			{
				return false;
			}
			for (int i = 0; i < arrayCount; i++)
			{
				if (!object.Equals(array[i], other[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] array, Converter<TInput, TOutput> converter)
		{
			TOutput[] array2 = new TOutput[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = converter(array[i]);
			}
			return array2;
		}

		internal static void AddRange(IList list, IList range)
		{
			RubyArray rubyArray;
			if ((rubyArray = list as RubyArray) != null)
			{
				rubyArray.AddRange(range);
				return;
			}
			List<object> list2;
			IEnumerable<object> collection;
			if ((list2 = list as List<object>) != null && (collection = range as IEnumerable<object>) != null)
			{
				list2.AddRange(collection);
				return;
			}
			foreach (object item in range)
			{
				list.Add(item);
			}
		}

		[Conditional("DEBUG")]
		public static void Log(string message, string category)
		{
		}

		public static long DateTimeTicksFromStopwatch(long elapsedStopwatchTicks)
		{
			if (Stopwatch.IsHighResolution)
			{
				return (long)((double)elapsedStopwatchTicks * 10000000.0 / (double)Stopwatch.Frequency);
			}
			return elapsedStopwatchTicks;
		}

		public static char ToLowerHexDigit(this int digit)
		{
			return (char)((digit < 10) ? (48 + digit) : (97 + digit - 10));
		}

		public static char ToUpperHexDigit(this int digit)
		{
			return (char)((digit < 10) ? (48 + digit) : (65 + digit - 10));
		}

		public static char ToUpperInvariant(this char c)
		{
			return char.ToUpper(c, CultureInfo.InvariantCulture);
		}

		public static char ToLowerInvariant(this char c)
		{
			return char.ToLower(c, CultureInfo.InvariantCulture);
		}

		internal static IEnumerable<Expression> ToExpressions(this IEnumerable<DynamicMetaObject> metaObjects)
		{
			foreach (DynamicMetaObject metaObject in metaObjects)
			{
				yield return (metaObject != null) ? metaObject.Expression : null;
			}
		}

		internal static Action<RubyModule> CloneInvocationChain(Action<RubyModule> chain)
		{
			if (chain == null)
			{
				return null;
			}
			Delegate[] invocationList = chain.GetInvocationList();
			int i = 1;
			Action<RubyModule> action = (Action<RubyModule>)invocationList[0].Clone();
			for (; i < invocationList.Length; i++)
			{
				action = (Action<RubyModule>)Delegate.Combine(action, (Action<RubyModule>)invocationList[i]);
			}
			return action;
		}

		internal static void CopyTupleFields(MutableTuple src, MutableTuple dst)
		{
			for (int i = 0; i < src.Capacity; i++)
			{
				dst.SetValue(i, src.GetValue(i));
			}
		}

		internal static bool ContainsInvalidCharacters(byte[] bytes, int start, int count, Encoding encoding)
		{
			Decoder decoder = encoding.GetDecoder();
			CheckDecoderFallback checkDecoderFallback = (CheckDecoderFallback)(decoder.Fallback = new CheckDecoderFallback());
			decoder.GetCharCount(bytes, start, count, true);
			return checkDecoderFallback.HasInvalidCharacters;
		}

		internal static bool ContainsInvalidCharacters(char[] chars, int start, int count, Encoding encoding)
		{
			Encoder encoder = encoding.GetEncoder();
			CheckEncoderFallback checkEncoderFallback = (CheckEncoderFallback)(encoder.Fallback = new CheckEncoderFallback());
			encoder.GetByteCount(chars, start, count, true);
			return checkEncoderFallback.HasInvalidCharacters;
		}
	}
}
