using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public static class Glob
	{
		private static class Constants
		{
			public static readonly int FNM_CASEFOLD = 8;

			public static readonly int FNM_DOTMATCH = 4;

			public static readonly int FNM_NOESCAPE = 1;

			public static readonly int FNM_PATHNAME = 2;

			public static readonly int FNM_SYSCASE = 8;
		}

		private class CharClass
		{
			private readonly StringBuilder _chars = new StringBuilder();

			internal void Add(char c)
			{
				if (c == ']' || c == '\\')
				{
					_chars.Append('\\');
				}
				_chars.Append(c);
			}

			internal string MakeString()
			{
				if (_chars.Length == 0)
				{
					return null;
				}
				if (_chars.Length == 1 && _chars[0] == '^')
				{
					_chars.Insert(0, "\\");
				}
				_chars.Insert(0, "[");
				_chars.Append(']');
				return _chars.ToString();
			}
		}

		private class GlobUngrouper
		{
			internal abstract class GlobNode
			{
				internal readonly GlobNode _parent;

				protected GlobNode(GlobNode parentNode)
				{
					_parent = parentNode ?? this;
				}

				internal abstract GlobNode AddChar(char c);

				internal abstract GlobNode StartLevel();

				internal abstract GlobNode AddGroup();

				internal abstract GlobNode FinishLevel();

				internal abstract List<StringBuilder> Flatten();
			}

			internal class TextNode : GlobNode
			{
				private readonly StringBuilder _builder;

				internal TextNode(GlobNode parentNode)
					: base(parentNode)
				{
					_builder = new StringBuilder();
				}

				internal override GlobNode AddChar(char c)
				{
					if (c != 0)
					{
						_builder.Append(c);
					}
					return this;
				}

				internal override GlobNode StartLevel()
				{
					return _parent.StartLevel();
				}

				internal override GlobNode AddGroup()
				{
					return _parent.AddGroup();
				}

				internal override GlobNode FinishLevel()
				{
					return _parent.FinishLevel();
				}

				internal override List<StringBuilder> Flatten()
				{
					List<StringBuilder> list = new List<StringBuilder>(1);
					list.Add(_builder);
					return list;
				}
			}

			internal class ChoiceNode : GlobNode
			{
				private readonly List<SequenceNode> _nodes;

				internal ChoiceNode(GlobNode parentNode)
					: base(parentNode)
				{
					_nodes = new List<SequenceNode>();
				}

				internal override GlobNode AddChar(char c)
				{
					SequenceNode sequenceNode = new SequenceNode(this);
					_nodes.Add(sequenceNode);
					return sequenceNode.AddChar(c);
				}

				internal override GlobNode StartLevel()
				{
					SequenceNode sequenceNode = new SequenceNode(this);
					_nodes.Add(sequenceNode);
					return sequenceNode.StartLevel();
				}

				internal override GlobNode AddGroup()
				{
					AddChar('\0');
					return this;
				}

				internal override GlobNode FinishLevel()
				{
					AddChar('\0');
					return _parent;
				}

				internal override List<StringBuilder> Flatten()
				{
					List<StringBuilder> list = new List<StringBuilder>();
					foreach (SequenceNode node in _nodes)
					{
						foreach (StringBuilder item in node.Flatten())
						{
							list.Add(item);
						}
					}
					return list;
				}
			}

			internal class SequenceNode : GlobNode
			{
				private readonly List<GlobNode> _nodes;

				internal SequenceNode(GlobNode parentNode)
					: base(parentNode)
				{
					_nodes = new List<GlobNode>();
				}

				internal override GlobNode AddChar(char c)
				{
					TextNode textNode = new TextNode(this);
					_nodes.Add(textNode);
					return textNode.AddChar(c);
				}

				internal override GlobNode StartLevel()
				{
					ChoiceNode choiceNode = new ChoiceNode(this);
					_nodes.Add(choiceNode);
					return choiceNode;
				}

				internal override GlobNode AddGroup()
				{
					return _parent;
				}

				internal override GlobNode FinishLevel()
				{
					return _parent._parent;
				}

				internal override List<StringBuilder> Flatten()
				{
					List<StringBuilder> list = new List<StringBuilder>();
					list.Add(new StringBuilder());
					foreach (GlobNode node in _nodes)
					{
						List<StringBuilder> list2 = new List<StringBuilder>();
						foreach (StringBuilder item in node.Flatten())
						{
							foreach (StringBuilder item2 in list)
							{
								StringBuilder stringBuilder = new StringBuilder(item2.ToString());
								stringBuilder.Append(item.ToString());
								list2.Add(stringBuilder);
							}
						}
						list = list2;
					}
					return list;
				}
			}

			private readonly SequenceNode _rootNode;

			private GlobNode _currentNode;

			private int _level;

			internal int Level
			{
				get
				{
					return _level;
				}
			}

			internal GlobUngrouper(int patternLength)
			{
				_rootNode = new SequenceNode(null);
				_currentNode = _rootNode;
				_level = 0;
			}

			internal void AddChar(char c)
			{
				_currentNode = _currentNode.AddChar(c);
			}

			internal void StartLevel()
			{
				_currentNode = _currentNode.StartLevel();
				_level++;
			}

			internal void AddGroup()
			{
				_currentNode = _currentNode.AddGroup();
			}

			internal void FinishLevel()
			{
				_currentNode = _currentNode.FinishLevel();
				_level--;
			}

			internal string[] Flatten()
			{
				if (_level != 0)
				{
					return ArrayUtils.EmptyStrings;
				}
				List<StringBuilder> list = _rootNode.Flatten();
				string[] array = new string[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					array[i] = list[i].ToString();
				}
				return array;
			}
		}

		private sealed class GlobMatcher
		{
			private readonly PlatformAdaptationLayer _pal;

			private readonly string _pattern;

			private readonly int _flags;

			private readonly bool _dirOnly;

			private readonly List<string> _result;

			private bool _stripTwo;

			private bool NoEscapes
			{
				get
				{
					return (_flags & Constants.FNM_NOESCAPE) != 0;
				}
			}

			internal GlobMatcher(PlatformAdaptationLayer pal, string pattern, int flags)
			{
				_pal = pal;
				_pattern = ((pattern == "**") ? "*" : pattern);
				_flags = flags | Constants.FNM_CASEFOLD;
				_result = new List<string>();
				_dirOnly = _pattern.LastCharacter() == 47;
				_stripTwo = false;
			}

			internal int FindNextSeparator(int position, bool allowWildcard, out bool containsWildcard)
			{
				int num = -1;
				bool flag = false;
				containsWildcard = false;
				for (int i = position; i < _pattern.Length; i++)
				{
					if (flag)
					{
						flag = false;
						continue;
					}
					switch (_pattern[i])
					{
						case '\\':
							flag = true;
							break;
						case '*':
						case '?':
						case '[':
							if (!allowWildcard)
							{
								return num + 1;
							}
							if (num >= 0)
							{
								return num;
							}
							containsWildcard = true;
							break;
						case '/':
						case ':':
							if (containsWildcard)
							{
								return i;
							}
							num = i;
							break;
					}
				}
				return _pattern.Length;
			}

			private void TestPath(string path, int patternEnd, bool isLastPathSegment)
			{
				if (!isLastPathSegment)
				{
					DoGlob(path, patternEnd, false);
					return;
				}
				if (!NoEscapes)
				{
					path = Unescape(path, _stripTwo ? 2 : 0);
				}
				else if (_stripTwo)
				{
					path = path.Substring(2);
				}
				if (_pal.DirectoryExists(path))
				{
					_result.Add(path);
				}
				else if (!_dirOnly && _pal.FileExists(path))
				{
					_result.Add(path);
				}
			}

			private static string Unescape(string path, int start)
			{
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = false;
				for (int i = start; i < path.Length; i++)
				{
					char c = path[i];
					if (flag)
					{
						flag = false;
					}
					else if (c == '\\')
					{
						flag = true;
						continue;
					}
					stringBuilder.Append(c);
				}
				if (flag)
				{
					stringBuilder.Append('\\');
				}
				return stringBuilder.ToString();
			}

			internal IList<string> DoGlob()
			{
				if (_pattern.Length == 0)
				{
					return ArrayUtils.EmptyStrings;
				}
				int num = 0;
				string text = ".";
				if (_pattern[0] == '/' || _pattern.IndexOf(':') >= 0)
				{
					bool containsWildcard;
					num = FindNextSeparator(0, false, out containsWildcard);
					if (num == _pattern.Length)
					{
						TestPath(_pattern, num, true);
						return _result;
					}
					if (num > 0 || _pattern[0] == '/')
					{
						text = _pattern.Substring(0, num);
					}
				}
				_stripTwo = text == ".";
				DoGlob(text, num, false);
				return _result;
			}

			internal void DoGlob(string baseDirectory, int position, bool isPreviousDoubleStar)
			{
				if (!_pal.DirectoryExists(baseDirectory))
				{
					return;
				}
				bool containsWildcard;
				int num = FindNextSeparator(position, true, out containsWildcard);
				bool flag = num == _pattern.Length;
				string text = _pattern.Substring(position, num - position);
				if (!flag)
				{
					num++;
				}
				if (!containsWildcard)
				{
					string path = baseDirectory + "/" + text;
					TestPath(path, num, flag);
					return;
				}
				bool flag2 = text.Equals("**");
				if (flag2 && !isPreviousDoubleStar)
				{
					DoGlob(baseDirectory, num, true);
				}
				string[] fileSystemEntries = _pal.GetFileSystemEntries(baseDirectory, "*");
				foreach (string path2 in fileSystemEntries)
				{
					string fileName = Path.GetFileName(path2);
					if (FnMatch(text, fileName, _flags))
					{
						string text2 = RubyUtils.CanonicalizePath(path2);
						TestPath(text2, num, flag);
						if (flag2)
						{
							DoGlob(text2, position, true);
						}
					}
				}
				if ((!flag || (_flags & Constants.FNM_DOTMATCH) == 0) && text[0] != '.')
				{
					return;
				}
				if (FnMatch(text, ".", _flags))
				{
					string text3 = baseDirectory + "/.";
					if (_dirOnly)
					{
						text3 += '/';
					}
					TestPath(text3, num, true);
				}
				if (FnMatch(text, "..", _flags))
				{
					string text4 = baseDirectory + "/..";
					if (_dirOnly)
					{
						text4 += '/';
					}
					TestPath(text4, num, true);
				}
			}
		}

		private static void AppendExplicitRegexChar(StringBuilder builder, char c)
		{
			builder.Append('[');
			if (c == '^' || c == '\\')
			{
				builder.Append('\\');
			}
			builder.Append(c);
			builder.Append(']');
		}

		internal static string PatternToRegex(string pattern, bool pathName, bool noEscape)
		{
			StringBuilder stringBuilder = new StringBuilder(pattern.Length);
			stringBuilder.Append("\\G");
			bool flag = false;
			CharClass charClass = null;
			foreach (char c in pattern)
			{
				if (flag)
				{
					if (charClass != null)
					{
						charClass.Add(c);
					}
					else
					{
						AppendExplicitRegexChar(stringBuilder, c);
					}
					flag = false;
					continue;
				}
				if (c == '\\' && !noEscape)
				{
					flag = true;
					continue;
				}
				if (charClass != null)
				{
					if (c == ']')
					{
						string text = charClass.MakeString();
						if (text == null)
						{
							return string.Empty;
						}
						stringBuilder.Append(text);
						charClass = null;
					}
					else
					{
						charClass.Add(c);
					}
					continue;
				}
				switch (c)
				{
					case '*':
						stringBuilder.Append(pathName ? "[^/]*" : ".*");
						break;
					case '?':
						stringBuilder.Append('.');
						break;
					case '[':
						charClass = new CharClass();
						break;
					default:
						AppendExplicitRegexChar(stringBuilder, c);
						break;
				}
			}
			if (charClass != null)
			{
				return string.Empty;
			}
			return stringBuilder.ToString();
		}

		public static bool FnMatch(string pattern, string path, int flags)
		{
			if (pattern.Length == 0)
			{
				return path.Length == 0;
			}
			bool pathName = (flags & Constants.FNM_PATHNAME) != 0;
			bool noEscape = (flags & Constants.FNM_NOESCAPE) != 0;
			string text = PatternToRegex(pattern, pathName, noEscape);
			if (text.Length == 0)
			{
				return false;
			}
			if ((flags & Constants.FNM_DOTMATCH) == 0 && path.Length > 0 && path[0] == '.' && (text.Length < 4 || text[2] != '[' || text[3] != '.'))
			{
				return false;
			}
			RegexOptions regexOptions = RegexOptions.None;
			if ((flags & Constants.FNM_CASEFOLD) != 0)
			{
				regexOptions |= RegexOptions.IgnoreCase;
			}
			Match match = Regex.Match(path, text, regexOptions);
			if (match != null && match.Success)
			{
				return match.Length == path.Length;
			}
			return false;
		}

		private static string[] UngroupGlobs(string pattern, bool noEscape)
		{
			GlobUngrouper globUngrouper = new GlobUngrouper(pattern.Length);
			bool flag = false;
			foreach (char c in pattern)
			{
				if (flag)
				{
					if (c != ',' && c != '{' && c != '}')
					{
						globUngrouper.AddChar('\\');
					}
					globUngrouper.AddChar(c);
					flag = false;
					continue;
				}
				if (c == '\\' && !noEscape)
				{
					flag = true;
					continue;
				}
				switch (c)
				{
					case '{':
						globUngrouper.StartLevel();
						break;
					case ',':
						if (globUngrouper.Level < 1)
						{
							globUngrouper.AddChar(c);
						}
						else
						{
							globUngrouper.AddGroup();
						}
						break;
					case '}':
						if (globUngrouper.Level < 1)
						{
							return ArrayUtils.EmptyStrings;
						}
						globUngrouper.FinishLevel();
						break;
					default:
						globUngrouper.AddChar(c);
						break;
				}
			}
			return globUngrouper.Flatten();
		}

		public static IEnumerable<string> GetMatches(PlatformAdaptationLayer pal, string pattern, int flags)
		{
			if (pattern.Length == 0)
			{
				yield break;
			}
			bool noEscape = (flags & Constants.FNM_NOESCAPE) != 0;
			string[] groups = UngroupGlobs(pattern, noEscape);
			if (groups.Length == 0)
			{
				yield break;
			}
			try
			{
				string[] array = groups;
				foreach (string group in array)
				{
					GlobMatcher matcher = new GlobMatcher(pal, group, flags);
					foreach (string item in matcher.DoGlob())
					{
						yield return item;
					}
				}
			}
			finally
			{
			}
		}

		public static IEnumerable<MutableString> GetMatches(RubyContext context, MutableString pattern, int flags)
		{
			foreach (string strFileName in GetMatches(pattern: context.DecodePath(pattern), pal: context.Platform, flags: flags))
			{
				yield return context.EncodePath(strFileName).TaintBy(pattern);
			}
		}
	}
}
