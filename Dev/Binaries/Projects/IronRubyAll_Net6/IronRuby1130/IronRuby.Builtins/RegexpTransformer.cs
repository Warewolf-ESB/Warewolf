using System;
using System.Collections.Generic;
using System.Text;
using IronRuby.Compiler;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	internal sealed class RegexpTransformer
	{
		private sealed class CharacterSet
		{
			public static readonly CharacterSet Empty = new CharacterSet();

			private readonly bool _negated;

			private readonly string _include;

			private readonly CharacterSet _exclude;

			private readonly bool _isSingleCharacter;

			public string Include
			{
				get
				{
					return _include;
				}
			}

			public bool IsEmpty
			{
				get
				{
					if (_include.Length == 0)
					{
						return !_negated;
					}
					return false;
				}
			}

			public bool IsSingleCharacter
			{
				get
				{
					return _isSingleCharacter;
				}
			}

			public CharacterSet()
			{
				_include = "";
				_exclude = this;
				_isSingleCharacter = false;
			}

			public CharacterSet(string include)
				: this(false, include, Empty)
			{
			}

			public CharacterSet(string include, bool isSingleCharacter)
				: this(false, include, Empty)
			{
				_isSingleCharacter = isSingleCharacter;
			}

			public CharacterSet(bool negate, string include)
				: this(negate, include, Empty)
			{
			}

			public CharacterSet(string include, CharacterSet exclude)
				: this(false, include, exclude)
			{
			}

			public CharacterSet(bool negate, string include, CharacterSet exclude)
			{
				_negated = negate;
				_include = include;
				_exclude = exclude;
			}

			internal CharacterSet GetIncludedSet()
			{
				return new CharacterSet(_include, _isSingleCharacter);
			}

			internal CharacterSet Complement()
			{
				return new CharacterSet(!_negated, _include, _exclude);
			}

			internal CharacterSet Subtract(CharacterSet set)
			{
				if (IsEmpty || set.IsEmpty)
				{
					return this;
				}
				if (_negated)
				{
					if (set._negated)
					{
						return set.Complement().Subtract(Complement());
					}
					return Complement().Union(set).Complement();
				}
				if (set._negated)
				{
					return Intersect(set.Complement());
				}
				return new CharacterSet(_include, _exclude.Union(set));
			}

			internal CharacterSet Union(CharacterSet set)
			{
				if (IsEmpty)
				{
					return set;
				}
				if (set.IsEmpty)
				{
					return this;
				}
				if (_negated)
				{
					if (set._negated)
					{
						return Complement().Intersect(set.Complement()).Complement();
					}
					return Complement().Subtract(set).Complement();
				}
				if (set._negated)
				{
					return set.Complement().Subtract(this).Complement();
				}
				return new CharacterSet(_include + set._include, set._exclude.Subtract(GetIncludedSet()).Union(_exclude.Subtract(set.GetIncludedSet())).Union(_exclude.Intersect(set._exclude)));
			}

			internal CharacterSet Intersect(CharacterSet set)
			{
				if (IsEmpty || set.IsEmpty)
				{
					return Empty;
				}
				if (_negated)
				{
					if (set._negated)
					{
						return Complement().Union(set.Complement()).Complement();
					}
					return set.Subtract(Complement());
				}
				if (set._negated)
				{
					return Subtract(set.Complement());
				}
				return new CharacterSet(_include, new CharacterSet(true, set._include, _exclude.Union(set._exclude)));
			}

			public StringBuilder AppendTo(StringBuilder sb, bool parenthesize)
			{
				if (IsEmpty)
				{
					if (_negated)
					{
						sb.Append("[\0-\uffff]");
					}
					else
					{
						sb.Append("[a-[a]]");
					}
				}
				else if (IsSingleCharacter && !parenthesize)
				{
					sb.Append(_include);
				}
				else
				{
					if (_negated)
					{
						sb.Append("[\0-\uffff-");
					}
					sb.Append('[');
					sb.Append(_include);
					if (!_exclude.IsEmpty)
					{
						sb.Append('-');
						_exclude.AppendTo(sb, true);
					}
					sb.Append(']');
					if (_negated)
					{
						sb.Append(']');
					}
				}
				return sb;
			}

			public override string ToString()
			{
				if (!IsEmpty)
				{
					return AppendTo(new StringBuilder(), false).ToString();
				}
				return string.Empty;
			}
		}

		private enum PosixCharacterClass
		{
			Alnum,
			Alpha,
			Ascii,
			Blank,
			Cntrl,
			Digit,
			Graph,
			Lower,
			Print,
			Punct,
			Space,
			Upper,
			XDigit,
			Word
		}

		private readonly string _rubyPattern;

		private int _index;

		private StringBuilder _sb;

		private bool _hasGAnchor;

		internal static string Transform(string rubyPattern, RubyRegexOptions options, out bool hasGAnchor)
		{
			if (rubyPattern == "^[\t\n\r -\ud7ff\ue000-\ufffd\ud800\udc00-\udbff\udfff]*$")
			{
				hasGAnchor = false;
				return "^(?:[\t\n\r -\ud7ff\ue000-\ufffd]|[\ud800-\udbff][\udc00-\udfff])*$";
			}
			RegexpTransformer regexpTransformer = new RegexpTransformer(rubyPattern);
			string result = regexpTransformer.Transform();
			hasGAnchor = regexpTransformer._hasGAnchor;
			return result;
		}

		private RegexpTransformer(string rubyPattern)
		{
			_rubyPattern = rubyPattern;
		}

		private int Peek()
		{
			if (_index >= _rubyPattern.Length)
			{
				return -1;
			}
			return _rubyPattern[_index];
		}

		private int Peek(int disp)
		{
			int num = _index + disp;
			if (num >= _rubyPattern.Length)
			{
				return -1;
			}
			return _rubyPattern[num];
		}

		private int Read()
		{
			if (_index >= _rubyPattern.Length)
			{
				return -1;
			}
			return _rubyPattern[_index++];
		}

		private void Back()
		{
			_index--;
		}

		private void Skip(int n)
		{
			_index += n;
		}

		private void Skip()
		{
			Skip(1);
		}

		private void Skip(char c)
		{
			Skip();
		}

		private bool Read(int c)
		{
			if (Peek() == c)
			{
				Skip();
				return true;
			}
			return false;
		}

		private void Append(char c)
		{
			_sb.Append(c);
		}

		private void AppendEscaped(int c)
		{
			AppendEscaped(_sb, c);
		}

		private static StringBuilder AppendEscaped(StringBuilder builder, string str)
		{
			for (int i = 0; i < str.Length; i++)
			{
				AppendEscaped(builder, str[i]);
			}
			return builder;
		}

		private static StringBuilder AppendEscaped(StringBuilder builder, int c)
		{
			if (IsMetaCharacter(c))
			{
				builder.Append('\\');
			}
			builder.Append((char)c);
			return builder;
		}

		private static string Escape(int c)
		{
			if (!IsMetaCharacter(c))
			{
				return ((char)c).ToString();
			}
			return "\\" + (char)c;
		}

		private static bool IsMetaCharacter(int c)
		{
			switch (c)
			{
				case 9:
				case 10:
				case 13:
				case 32:
				case 35:
				case 36:
				case 40:
				case 41:
				case 42:
				case 43:
				case 45:
				case 46:
				case 63:
				case 91:
				case 92:
				case 93:
				case 94:
				case 123:
				case 124:
				case 125:
					return true;
				default:
					return false;
			}
		}

		private Exception MakeError(string message)
		{
			return new RegexpError(message + ": " + _rubyPattern);
		}

		private string Transform()
		{
			_sb = new StringBuilder(_rubyPattern.Length);
			Parse(false);
			string result = _sb.ToString();
			_sb = null;
			return result;
		}

		private void Parse(bool isSubexpression)
		{
			int lastEntityIndex = 0;
			while (true)
			{
				int num;
				switch (num = Read())
				{
					case -1:
						if (isSubexpression)
						{
							throw MakeError("end pattern in group");
						}
						return;
					case 92:
						lastEntityIndex = _sb.Length;
						ParseEscape();
						continue;
					case 42:
					case 43:
					case 63:
						Append((char)num);
						ParsePostQuantifier(lastEntityIndex, true);
						continue;
					case 123:
						if (ParseConstrainedQuantifier())
						{
							ParsePostQuantifier(lastEntityIndex, false);
							continue;
						}
						break;
					case 40:
						lastEntityIndex = _sb.Length;
						ParseGroup();
						continue;
					case 41:
						if (isSubexpression)
						{
							return;
						}
						throw MakeError("unmatched close parenthesis");
					case 91:
						lastEntityIndex = _sb.Length;
						ParseCharacterGroup(false).AppendTo(_sb, true);
						continue;
					case 124:
						Append('|');
						lastEntityIndex = _sb.Length;
						continue;
				}
				lastEntityIndex = _sb.Length;
				Append((char)num);
			}
		}

		private bool ParseConstrainedQuantifier()
		{
			int num = -1;
			int num2 = 0;
			while (true)
			{
				int num3 = Peek(num2++);
				switch (num3)
				{
					case 44:
						if (num != -1)
						{
							return false;
						}
						num = num2;
						break;
					default:
						if (!Tokenizer.IsDecimalDigit(num3))
						{
							return false;
						}
						break;
					case 125:
						_sb.Append(_rubyPattern, _index - 1, num2 + 1);
						_index += num2;
						return true;
				}
			}
		}

		private void ParsePostQuantifier(int lastEntityIndex, bool possessive)
		{
			switch (Peek())
			{
				case 43:
					Skip();
					_sb.Insert(lastEntityIndex, possessive ? "(?>" : "(?:");
					Append(')');
					if (!possessive)
					{
						Append('+');
					}
					break;
				case 63:
					Skip();
					Append('?');
					break;
			}
		}

		private void ParseGroup()
		{
			if (Read(63))
			{
				int num = Read();
				if (num == 35)
				{
					while (true)
					{
						switch (Read())
						{
							case -1:
								throw MakeError("end pattern in group");
							case 41:
								return;
						}
					}
				}
				Append('(');
				Append('?');
				switch (num)
				{
					case 45:
					case 105:
					case 109:
					case 120:
						while (true)
						{
							switch (num)
							{
								case 109:
									Append('s');
									goto IL_00fa;
								case 45:
								case 105:
								case 120:
									Append((char)num);
									goto IL_00fa;
								case 58:
									Append(':');
									break;
								case -1:
								case 41:
									Back();
									break;
								default:
									throw MakeError("undefined group option");
							}
							break;
						IL_00fa:
							num = Read();
						}
						break;
					case 58:
						Append(':');
						break;
					case 61:
						Append('=');
						break;
					case 33:
						Append('!');
						break;
					case 62:
						Append('>');
						break;
					case 60:
						Append('<');
						num = Read();
						if (num == 61 || num == 33)
						{
							Append((char)num);
						}
						else
						{
							ParseGroupName(num, 62);
						}
						break;
					case 39:
						Append('\'');
						ParseGroupName(Read(), 39);
						break;
					default:
						throw MakeError("undefined group option");
				}
			}
			else
			{
				Append('(');
			}
			Parse(true);
			Append(')');
		}

		private void ParseGroupName(int c, int terminator)
		{
			if (c == terminator || c == -1)
			{
				throw MakeError("group name is empty");
			}
			do
			{
				Append((char)c);
				c = Read();
				if (c == terminator || c == 41)
				{
					Append((char)c);
					return;
				}
			}
			while (c != -1);
			throw MakeError("unterminated group name");
		}

		private void ParseEscape()
		{
			int num = Read();
			if (num == -1)
			{
				throw MakeError("too short escape sequence");
			}
			ParseEscape(num);
		}

		private void ParseEscape(int escape)
		{
			switch (escape)
			{
				case 65:
				case 66:
				case 90:
				case 98:
				case 122:
					Append('\\');
					Append((char)escape);
					break;
				case 103:
					throw MakeError("\\g not supported");
				case 107:
					ParseBackreference();
					break;
				case 71:
					_hasGAnchor = true;
					Append('\\');
					Append((char)escape);
					break;
				case 117:
					if (Peek() == 123)
					{
						foreach (int item in ParseUnicodeEscapeList())
						{
							AppendEscaped(item);
						}
						break;
					}
					AppendEscaped(ParseUnicodeEscape());
					break;
				default:
					if (Tokenizer.IsDecimalDigit(escape))
					{
						Append('\\');
						Append((char)escape);
					}
					else
					{
						ParseCharacterEscape(escape).AppendTo(_sb, false);
					}
					break;
			}
		}

		private void ParseBackreference()
		{
			int num = Read();
			int num2;
			switch (num)
			{
				case 60:
					num2 = 62;
					break;
				case 39:
					num2 = 39;
					break;
				default:
					throw MakeError("invalid back reference");
			}
			Append('\\');
			Append('k');
			Append((char)num);
			num = Read();
			if (num == num2 || num == -1)
			{
				throw MakeError("group name is empty");
			}
			do
			{
				Append((char)num);
				num = Read();
				if (num == num2)
				{
					Append((char)num);
					return;
				}
			}
			while (num != -1);
			throw MakeError("invalid group name");
		}

		private int ParseSingleByteCharacterEscape(int escape)
		{
			bool hasControlModifier = false;
			bool hasMetaModifier = false;
			return ParseSingleByteCharacterEscape(escape, ref hasControlModifier, ref hasMetaModifier);
		}

		private int ParseSingleByteCharacterEscape(int escape, ref bool hasControlModifier, ref bool hasMetaModifier)
		{
			switch (escape)
			{
				case -1:
					throw MakeError("too short escape sequence");
				case 92:
					return 92;
				case 110:
					return 10;
				case 116:
					return 9;
				case 114:
					return 13;
				case 102:
					return 12;
				case 118:
					return 11;
				case 97:
					return 7;
				case 101:
					return 27;
				case 98:
					return 8;
				case 77:
					{
						if (!Read(45))
						{
							throw MakeError("too short meta escape");
						}
						if (hasMetaModifier)
						{
							throw MakeError("duplicate meta escape");
						}
						hasMetaModifier = true;
						int c = Read();
						switch (c)
						{
							case -1:
								throw MakeError("too short escape sequence");
							case 92:
								c = ParseSingleByteCharacterEscape(Read(), ref hasControlModifier, ref hasMetaModifier);
								break;
						}
						return (c & 0xFF) | 0x80;
					}
				case 67:
					if (!Read(45))
					{
						throw MakeError("too short control escape");
					}
					goto case 99;
				case 99:
					{
						int c = Read();
						if (c == -1)
						{
							throw MakeError("too short escape sequence");
						}
						if (hasControlModifier)
						{
							throw MakeError("duplicate control escape");
						}
						hasControlModifier = true;
						if (c == 92)
						{
							c = ParseSingleByteCharacterEscape(Read(), ref hasControlModifier, ref hasMetaModifier);
						}
						return c & 0x9F;
					}
				case 120:
					{
						int c = Peek();
						int num4 = Tokenizer.ToDigit(c);
						if (num4 > 15)
						{
							throw MakeError("invalid hex escape");
						}
						Skip();
						c = Peek();
						int num5 = Tokenizer.ToDigit(c);
						if (num5 > 15)
						{
							return num4;
						}
						Skip();
						return (num4 << 4) | num5;
					}
				default:
					{
						int num = Tokenizer.ToDigit(escape);
						if (num > 7)
						{
							return -1;
						}
						int num2 = Tokenizer.ToDigit(Peek());
						if (num2 > 7)
						{
							return num;
						}
						Skip();
						int num3 = Tokenizer.ToDigit(Peek());
						if (num3 > 7)
						{
							return (num << 3) | num2;
						}
						Skip();
						return (num << 6) | (num2 << 3) | num3;
					}
			}
		}

		private CharacterSet ParseCharacterEscape(int escape)
		{
			int num = ParseSingleByteCharacterEscape(escape);
			if (num != -1)
			{
				return new CharacterSet(Escape(num), true);
			}
			switch (escape)
			{
				case 72:
				case 104:
					return MakePosixCharacterClass(PosixCharacterClass.XDigit, escape == 104);
				case 80:
				case 112:
					return ParseCharacterCategoryName(escape);
				case 115:
					return new CharacterSet("\\s");
				case 83:
					return new CharacterSet("\\S");
				case 100:
					return new CharacterSet("\\d");
				case 68:
					return new CharacterSet("\\D");
				case 119:
					return new CharacterSet("\\w");
				case 87:
					return new CharacterSet("\\W");
				default:
					return new CharacterSet(Escape(escape), true);
			}
		}

		private int ParseUnicodeEscape()
		{
			int num = Tokenizer.ToDigit(Read());
			int num2 = Tokenizer.ToDigit(Read());
			int num3 = Tokenizer.ToDigit(Read());
			int num4 = Tokenizer.ToDigit(Read());
			if (num4 >= 16 || num3 >= 16 || num2 >= 16 || num >= 16)
			{
				throw MakeError("invalid Unicode escape");
			}
			int num5 = (num << 12) | (num2 << 8) | (num3 << 4) | num4;
			if (num5 >= 55296 && num5 <= 57343)
			{
				throw MakeError("invalid Unicode range");
			}
			return num5;
		}

		private IEnumerable<int> ParseUnicodeEscapeList()
		{
			Skip('{');
			while (true)
			{
				int codepoint = ParseUnicodeCodePoint();
				int c = Read();
				yield return codepoint;
				switch (c)
				{
					case 32:
						continue;
					case 125:
						yield break;
				}
				throw MakeError("invalid Unicode list");
			}
		}

		private int ParseUnicodeCodePoint()
		{
			int num = 0;
			int num2 = 0;
			while (true)
			{
				int num3 = Tokenizer.ToDigit(Peek());
				if (num3 >= 16)
				{
					break;
				}
				if (num2 < 7)
				{
					num = (num << 4) | num3;
				}
				num2++;
				Skip();
			}
			if (num2 == 0)
			{
				throw MakeError("invalid Unicode list");
			}
			return num;
		}

		private string UnicodeCodePointToString(int codepoint)
		{
			StringBuilder stringBuilder = new StringBuilder(2);
			AppendUnicodeCodePoint(stringBuilder, codepoint);
			return stringBuilder.ToString();
		}

		private void AppendUnicodeCodePoint(StringBuilder builder, int codepoint)
		{
			if ((codepoint >= 55296 && codepoint <= 57343) || codepoint > 1114111)
			{
				throw MakeError("invalid Unicode range");
			}
			if (codepoint < 65536)
			{
				AppendEscaped(builder, codepoint);
				return;
			}
			codepoint -= 65536;
			Append((char)(codepoint / 1024 + 55296));
			Append((char)(codepoint % 1024 + 56320));
		}

		private CharacterSet ParseCharacterGroup(bool nested)
		{
			bool flag = !Read(94);
			if (nested)
			{
				CharacterSet characterSet = ParsePosixCharacterClass(flag);
				if (characterSet != null)
				{
					return characterSet;
				}
			}
			CharacterSet characterSet2 = ParseCharacterGroupIntersections();
			if (!flag)
			{
				characterSet2 = characterSet2.Complement();
			}
			Read(93);
			return characterSet2;
		}

		private CharacterSet ParseCharacterGroupIntersections()
		{
			CharacterSet characterSet = null;
			int num;
			while ((num = Peek()) != -1 && num != 93)
			{
				CharacterSet characterSet2 = ParseCharacterGroupUnion();
				characterSet = ((characterSet != null) ? characterSet.Intersect(characterSet2) : characterSet2);
			}
			if (characterSet == null)
			{
				throw MakeError((num == -1) ? "premature end of char-class" : "empty char-class");
			}
			return characterSet;
		}

		private CharacterSet ParseCharacterGroupUnion()
		{
			CharacterSet characterSet = CharacterSet.Empty;
			IEnumerator<int> codepoints = null;
			while (true)
			{
				bool mayStartRange;
				CharacterSet characterSet2 = ParseCharacter(ref codepoints, out mayStartRange);
				if (characterSet2 == null)
				{
					break;
				}
				if (codepoints == null && Read(45))
				{
					bool mayStartRange2;
					CharacterSet characterSet3 = ParseCharacter(ref codepoints, out mayStartRange2);
					if (characterSet3 == null)
					{
						characterSet = characterSet.Union(characterSet2).Union(new CharacterSet("\\-", true));
						break;
					}
					if (!mayStartRange || !characterSet2.IsSingleCharacter)
					{
						throw MakeError("char-class value at start of range");
					}
					if (!mayStartRange2 || !characterSet3.IsSingleCharacter)
					{
						throw MakeError("char-class value at end of range");
					}
					characterSet2 = new CharacterSet(characterSet2.Include + "-" + characterSet3.Include);
				}
				characterSet = characterSet.Union(characterSet2);
			}
			return characterSet;
		}

		private CharacterSet ParseCharacter(ref IEnumerator<int> codepoints, out bool mayStartRange)
		{
			if (codepoints != null)
			{
				mayStartRange = true;
				int current = codepoints.Current;
				if (!codepoints.MoveNext())
				{
					codepoints = null;
				}
				return new CharacterSet(UnicodeCodePointToString(current), true);
			}
			int num;
			switch (num = Read())
			{
				case -1:
					throw MakeError("premature end of char-class");
				case 93:
					Back();
					mayStartRange = false;
					return null;
				case 38:
					if (Read(38))
					{
						mayStartRange = false;
						return null;
					}
					break;
				case 92:
					{
						int num2 = Read();
						if (num2 == 117)
						{
							int codepoint;
							if (Peek() == 123)
							{
								codepoints = ParseUnicodeEscapeList().GetEnumerator();
								if (!codepoints.MoveNext())
								{
									throw MakeError("invalid Unicode list");
								}
								codepoint = codepoints.Current;
								if (!codepoints.MoveNext())
								{
									codepoints = null;
								}
							}
							else
							{
								codepoint = ParseUnicodeEscape();
							}
							mayStartRange = true;
							return new CharacterSet(UnicodeCodePointToString(codepoint), true);
						}
						mayStartRange = true;
						return ParseCharacterEscape(num2);
					}
				case 91:
					mayStartRange = false;
					return ParseCharacterGroup(true);
				case 45:
					mayStartRange = true;
					return new CharacterSet("\\-", true);
			}
			mayStartRange = true;
			return new CharacterSet(((char)num).ToString(), true);
		}

		private CharacterSet ParseCharacterCategoryName(int escape)
		{
			bool flag = escape == 112;
			int num = Peek();
			if (num != 123)
			{
				throw MakeError("invalid Unicode property");
			}
			Skip();
			if (Peek() == 94)
			{
				flag = !flag;
				Skip();
			}
			int index = _index;
			while ((num = Peek()) != 125 && num != -1)
			{
				Skip();
			}
			if (num == -1)
			{
				throw MakeError("invalid Unicode property");
			}
			string text = _rubyPattern.Substring(index, _index - index);
			Skip();
			switch (text)
			{
				case "Alnum":
					return MakePosixCharacterClass(PosixCharacterClass.Alnum, flag);
				case "Alpha":
					return MakePosixCharacterClass(PosixCharacterClass.Alpha, flag);
				case "Blank":
					return MakePosixCharacterClass(PosixCharacterClass.Blank, flag);
				case "Cntrl":
					return MakePosixCharacterClass(PosixCharacterClass.Cntrl, flag);
				case "Digit":
					return MakePosixCharacterClass(PosixCharacterClass.Digit, flag);
				case "Graph":
					return MakePosixCharacterClass(PosixCharacterClass.Graph, flag);
				case "Lower":
					return MakePosixCharacterClass(PosixCharacterClass.Lower, flag);
				case "Print":
					return MakePosixCharacterClass(PosixCharacterClass.Print, flag);
				case "Punct":
					return MakePosixCharacterClass(PosixCharacterClass.Punct, flag);
				case "Space":
					return MakePosixCharacterClass(PosixCharacterClass.Space, flag);
				case "Upper":
					return MakePosixCharacterClass(PosixCharacterClass.Upper, flag);
				case "XDigit":
					return MakePosixCharacterClass(PosixCharacterClass.XDigit, flag);
				case "ASCII":
					return MakePosixCharacterClass(PosixCharacterClass.Ascii, flag);
				case "Word":
					return MakePosixCharacterClass(PosixCharacterClass.Word, flag);
				case "Any":
					if (flag)
					{
						return new CharacterSet("\\P{L}\\P{N}");
					}
					return new CharacterSet("\\p{L}", new CharacterSet("\\p{L}"));
				case "Assigned":
					flag = !flag;
					text = "Cn";
					break;
				case "Arabic":
				case "Armenian":
				case "Bengali":
				case "Bopomofo":
				case "Braille":
				case "Buginese":
				case "Buhid":
				case "Cherokee":
				case "Common":
				case "Coptic":
				case "Cypriot":
				case "Cyrillic":
				case "Deseret":
				case "Devanagari":
				case "Ethiopic":
				case "Georgian":
				case "Glagolitic":
				case "Gothic":
				case "Greek":
				case "Gujarati":
				case "Gurmukhi":
				case "Han":
				case "Hangul":
				case "Hanunoo":
				case "Hebrew":
				case "Hiragana":
				case "Inherited":
				case "Kannada":
				case "Katakana":
				case "Kharoshthi":
				case "Khmer":
				case "Lao":
				case "Latin":
				case "Limbu":
				case "Linear_B":
				case "Malayalam":
				case "Mongolian":
				case "Myanmar":
				case "New_Tai_Lue":
				case "Ogham":
				case "Old_Italic":
				case "Old_Persian":
				case "Oriya":
				case "Osmanya":
				case "Runic":
				case "Shavian":
				case "Sinhala":
				case "Syloti_Nagri":
				case "Syriac":
				case "Tagalog":
				case "Tagbanwa":
				case "TaiLe":
				case "Tamil":
				case "Telugu":
				case "Thaana":
				case "Thai":
				case "Tibetan":
				case "Tifinagh":
				case "Ugaritic":
				case "Yi":
					text = "Is" + text;
					break;
				case "Canadian_Aboriginal":
					text = "IsUnifiedCanadianAboriginalSyllabics";
					break;
			}
			return new CharacterSet("\\" + (flag ? 'p' : 'P') + "{" + text + "}");
		}

		private CharacterSet ParsePosixCharacterClass(bool positive)
		{
			int num = 0;
			if (Peek(num) == 58)
			{
				num++;
				int num2 = _index + num;
				int num3;
				while ((num3 = Peek(num)) != 58 && num3 != -1)
				{
					num++;
				}
				if (num3 == -1 || Peek(num + 1) != 93)
				{
					return null;
				}
				string name = _rubyPattern.Substring(num2, _index + num - num2);
				_index += num + 2;
				return MakePosixCharacterClass(ParsePosixClass(name), positive);
			}
			return null;
		}

		private PosixCharacterClass ParsePosixClass(string name)
		{
			switch (name)
			{
				case "alnum":
					return PosixCharacterClass.Alnum;
				case "alpha":
					return PosixCharacterClass.Alpha;
				case "ascii":
					return PosixCharacterClass.Ascii;
				case "blank":
					return PosixCharacterClass.Blank;
				case "cntrl":
					return PosixCharacterClass.Cntrl;
				case "digit":
					return PosixCharacterClass.Digit;
				case "graph":
					return PosixCharacterClass.Graph;
				case "lower":
					return PosixCharacterClass.Lower;
				case "print":
					return PosixCharacterClass.Print;
				case "punct":
					return PosixCharacterClass.Punct;
				case "space":
					return PosixCharacterClass.Space;
				case "upper":
					return PosixCharacterClass.Upper;
				case "xdigit":
					return PosixCharacterClass.XDigit;
				case "word":
					return PosixCharacterClass.Word;
				default:
					throw MakeError("invalid POSIX bracket type");
			}
		}

		private CharacterSet MakePosixCharacterClass(PosixCharacterClass charClass, bool positive)
		{
			switch (charClass)
			{
				case PosixCharacterClass.Alnum:
					if (positive)
					{
						return new CharacterSet("\\p{L}\\p{N}\\p{M}");
					}
					return new CharacterSet("\\P{L}", new CharacterSet("\\p{N}\\p{M}"));
				case PosixCharacterClass.Alpha:
					if (positive)
					{
						return new CharacterSet("\\p{L}\\p{M}");
					}
					return new CharacterSet("\\P{L}", new CharacterSet("\\p{M}"));
				case PosixCharacterClass.Ascii:
					if (positive)
					{
						return new CharacterSet("\\p{IsBasicLatin}");
					}
					return new CharacterSet("\\P{IsBasicLatin}");
				case PosixCharacterClass.Blank:
					if (positive)
					{
						return new CharacterSet("\\p{Zs}\t");
					}
					return new CharacterSet("\\P{Zs}", new CharacterSet("\t"));
				case PosixCharacterClass.Cntrl:
					if (positive)
					{
						return new CharacterSet("\\p{Cc}");
					}
					return new CharacterSet("\\P{Cc}");
				case PosixCharacterClass.Digit:
					if (positive)
					{
						return new CharacterSet("\\p{Nd}");
					}
					return new CharacterSet("\\P{Nd}");
				case PosixCharacterClass.Graph:
					if (positive)
					{
						return new CharacterSet("\\P{Z}", new CharacterSet("\\p{C}"));
					}
					return new CharacterSet("\\p{Z}\\p{C}");
				case PosixCharacterClass.Lower:
					if (positive)
					{
						return new CharacterSet("\\p{Ll}");
					}
					return new CharacterSet("\\P{Ll}");
				case PosixCharacterClass.Print:
					if (positive)
					{
						return new CharacterSet("\\P{C}");
					}
					return new CharacterSet("\\p{C}");
				case PosixCharacterClass.Punct:
					if (positive)
					{
						return new CharacterSet("\\p{P}");
					}
					return new CharacterSet("\\P{P}");
				case PosixCharacterClass.Space:
					if (positive)
					{
						return new CharacterSet("\\p{Z}\u0085\t-\r");
					}
					return new CharacterSet("\\P{Z}", new CharacterSet("\u0085\t-\r"));
				case PosixCharacterClass.Upper:
					if (positive)
					{
						return new CharacterSet("\\p{Lu}");
					}
					return new CharacterSet("\\P{Lu}");
				case PosixCharacterClass.XDigit:
					if (positive)
					{
						return new CharacterSet("a-fA-F0-9");
					}
					return new CharacterSet(true, "a-fA-F0-9");
				case PosixCharacterClass.Word:
					if (positive)
					{
						return new CharacterSet("\\p{L}\\p{Nd}\\p{Pc}\\p{M}");
					}
					return new CharacterSet("\\P{L}", new CharacterSet("\\p{Nd}\\p{Pc}\\p{M}"));
				default:
					throw Assert.Unreachable;
			}
		}
	}
}
