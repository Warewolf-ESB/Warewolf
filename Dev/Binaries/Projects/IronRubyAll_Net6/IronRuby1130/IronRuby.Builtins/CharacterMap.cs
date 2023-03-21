using System.Collections;
using System.Collections.Generic;

namespace IronRuby.Builtins
{
	public sealed class CharacterMap
	{
		private const char Unmapped = '\0';

		private readonly BitArray _map;

		private readonly char[] _image;

		private readonly int _complement;

		private readonly bool _isComplemental;

		private readonly int _min;

		private readonly uint _imageWidth;

		public int Complement
		{
			get
			{
				return _complement;
			}
		}

		public bool IsComplemental
		{
			get
			{
				return _isComplemental;
			}
		}

		public bool HasBitmap
		{
			get
			{
				return _map != null;
			}
		}

		public bool HasFullMap
		{
			get
			{
				if (_complement < 0)
				{
					return _image != null;
				}
				return true;
			}
		}

		private CharacterMap(BitArray map, char[] image, int complement, bool isComplemental, int min, int max)
		{
			_map = map;
			_min = min;
			_imageWidth = (uint)(max - min);
			_image = image;
			_complement = complement;
			_isComplemental = isComplemental;
		}

		public int TryMap(char c)
		{
			int num = c - _min;
			int num2 = _complement;
			if ((uint)num <= _imageWidth)
			{
				int num3;
				if (num2 < 0)
				{
					num3 = _image[num];
					if (num3 != 0)
					{
						return num3;
					}
					if (_map == null)
					{
						return -1;
					}
					num2 = -1;
				}
				else
				{
					num3 = -1;
				}
				if (!_map[num])
				{
					return num2;
				}
				return num3;
			}
			return num2;
		}

		public bool IsMapped(char c)
		{
			int num = c - _min;
			if ((uint)num > _imageWidth)
			{
				return false;
			}
			return _map[num];
		}

		public static CharacterMap Create(MutableString from, MutableString to)
		{
			int charCount = from.GetCharCount();
			bool flag = from.StartsWith('^') && charCount > 1;
			int num;
			int num2;
			if (from.DetectByteCharacters())
			{
				num = 0;
				num2 = 255;
			}
			else
			{
				num = int.MaxValue;
				num2 = -1;
				for (int i = (flag ? 1 : 0); i < charCount; i++)
				{
					int @char = from.GetChar(i);
					if (@char < num)
					{
						num = @char;
					}
					if (@char > num2)
					{
						num2 = @char;
					}
				}
			}
			char[] array;
			BitArray map;
			if (flag || to.IsEmpty)
			{
				array = null;
				map = MakeBitmap(from, charCount, flag, num, num2);
			}
			else
			{
				map = null;
				array = new char[num2 - num + 1];
				bool flag2 = false;
				IEnumerator<char> enumerator = ExpandRanges(to, 0, to.GetCharCount(), true).GetEnumerator();
				foreach (char item in ExpandRanges(from, 0, charCount, false))
				{
					enumerator.MoveNext();
					bool num3 = flag2;
					char current2;
					array[item - num] = (current2 = enumerator.Current);
					flag2 = num3 || current2 == '\0';
				}
				if (flag2)
				{
					map = MakeBitmap(from, charCount, false, num, num2);
				}
			}
			return new CharacterMap(map, array, flag ? to.GetLastChar() : (-1), flag, num, num2);
		}

		private static BitArray MakeBitmap(MutableString from, int fromLength, bool complemental, int min, int max)
		{
			BitArray bitArray = new BitArray(max - min + 1);
			foreach (char item in ExpandRanges(from, complemental ? 1 : 0, fromLength, false))
			{
				bitArray.Set(item - min, true);
			}
			return bitArray;
		}

		internal static IEnumerable<char> ExpandRanges(MutableString str, int start, int end, bool infinite)
		{
			int rangeMax = -1;
			char c = '\0';
			int i = start;
			char lookahead = str.GetChar(start);
			while (true)
			{
				if (c < rangeMax)
				{
					c = (char)(c + 1);
				}
				else
				{
					if (i >= end)
					{
						break;
					}
					c = lookahead;
					i++;
					lookahead = ((i < end) ? str.GetChar(i) : '\0');
					if (lookahead == '-' && i + 1 < end)
					{
						rangeMax = str.GetChar(i + 1);
						i += 2;
						lookahead = ((i < end) ? str.GetChar(i) : '\0');
						if (c > rangeMax)
						{
							continue;
						}
					}
					else
					{
						rangeMax = -1;
					}
				}
				yield return c;
			}
			if (infinite)
			{
				while (true)
				{
					yield return c;
				}
			}
		}
	}
}
