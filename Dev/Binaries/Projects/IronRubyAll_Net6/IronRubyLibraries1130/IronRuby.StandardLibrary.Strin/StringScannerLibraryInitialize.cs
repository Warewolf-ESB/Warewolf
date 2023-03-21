using System;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.StringScanner
{
	public sealed class StringScannerLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(RubyObject));
			DefineGlobalClass("StringScanner", typeof(StringScanner), 8, @class, LoadStringScanner_Instance, LoadStringScanner_Class, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, object, StringScanner>(StringScanner.Create));
		}

		private static void LoadStringScanner_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 17, 0u, new Func<StringScanner, int, MutableString>(StringScanner.GetMatchSubgroup));
			LibraryInitializer.DefineLibraryMethod(module, "<<", 17, 0u, new Func<StringScanner, MutableString, StringScanner>(StringScanner.Concat));
			LibraryInitializer.DefineLibraryMethod(module, "beginning_of_line?", 17, 0u, new Func<StringScanner, bool>(StringScanner.BeginningOfLine));
			LibraryInitializer.DefineLibraryMethod(module, "bol?", 17, 0u, new Func<StringScanner, bool>(StringScanner.BeginningOfLine));
			LibraryInitializer.DefineLibraryMethod(module, "check", 17, 2u, new Func<StringScanner, RubyRegex, MutableString>(StringScanner.Check));
			LibraryInitializer.DefineLibraryMethod(module, "check_until", 17, 2u, new Func<StringScanner, RubyRegex, MutableString>(StringScanner.CheckUntil));
			LibraryInitializer.DefineLibraryMethod(module, "clear", 17, 0u, new Func<StringScanner, StringScanner>(StringScanner.Clear));
			LibraryInitializer.DefineLibraryMethod(module, "concat", 17, 0u, new Func<StringScanner, MutableString, StringScanner>(StringScanner.Concat));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 17, 0u, new Func<StringScanner, bool>(StringScanner.EndOfLine));
			LibraryInitializer.DefineLibraryMethod(module, "eos?", 17, 0u, new Func<StringScanner, bool>(StringScanner.EndOfLine));
			LibraryInitializer.DefineLibraryMethod(module, "exist?", 17, 2u, new Func<StringScanner, RubyRegex, int?>(StringScanner.Exist));
			LibraryInitializer.DefineLibraryMethod(module, "get_byte", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.GetByte));
			LibraryInitializer.DefineLibraryMethod(module, "getbyte", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.GetByte));
			LibraryInitializer.DefineLibraryMethod(module, "getch", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.GetChar));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 65538u, new Action<StringScanner, MutableString, object>(StringScanner.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 18, 65538u, new Action<StringScanner, StringScanner>(StringScanner.InitializeFrom));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "match?", 17, 2u, new Func<StringScanner, RubyRegex, int?>(StringScanner.Match));
			LibraryInitializer.DefineLibraryMethod(module, "matched", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.Matched));
			LibraryInitializer.DefineLibraryMethod(module, "matched?", 17, 0u, new Func<StringScanner, bool>(StringScanner.WasMatched));
			LibraryInitializer.DefineLibraryMethod(module, "matched_size", 17, 0u, new Func<StringScanner, int?>(StringScanner.MatchedSize));
			LibraryInitializer.DefineLibraryMethod(module, "matchedsize", 17, 0u, new Func<StringScanner, int?>(StringScanner.MatchedSize));
			LibraryInitializer.DefineLibraryMethod(module, "peek", 17, 0u, new Func<StringScanner, int, MutableString>(StringScanner.Peek));
			LibraryInitializer.DefineLibraryMethod(module, "peep", 17, 0u, new Func<StringScanner, int, MutableString>(StringScanner.Peek));
			LibraryInitializer.DefineLibraryMethod(module, "pointer", 17, 0u, new Func<StringScanner, int>(StringScanner.GetCurrentPosition));
			LibraryInitializer.DefineLibraryMethod(module, "pointer=", 17, 0u, new Func<StringScanner, int, int>(StringScanner.SetCurrentPosition));
			LibraryInitializer.DefineLibraryMethod(module, "pos", 17, 0u, new Func<StringScanner, int>(StringScanner.GetCurrentPosition));
			LibraryInitializer.DefineLibraryMethod(module, "pos=", 17, 0u, new Func<StringScanner, int, int>(StringScanner.SetCurrentPosition));
			LibraryInitializer.DefineLibraryMethod(module, "post_match", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.PostMatch));
			LibraryInitializer.DefineLibraryMethod(module, "pre_match", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.PreMatch));
			LibraryInitializer.DefineLibraryMethod(module, "reset", 17, 0u, new Func<StringScanner, StringScanner>(StringScanner.Reset));
			LibraryInitializer.DefineLibraryMethod(module, "rest", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.Rest));
			LibraryInitializer.DefineLibraryMethod(module, "rest?", 17, 0u, new Func<StringScanner, bool>(StringScanner.IsRestLeft));
			LibraryInitializer.DefineLibraryMethod(module, "rest_size", 17, 0u, new Func<StringScanner, int>(StringScanner.RestSize));
			LibraryInitializer.DefineLibraryMethod(module, "restsize", 17, 0u, new Func<StringScanner, int>(StringScanner.RestSize));
			LibraryInitializer.DefineLibraryMethod(module, "scan", 17, 2u, new Func<StringScanner, RubyRegex, object>(StringScanner.Scan));
			LibraryInitializer.DefineLibraryMethod(module, "scan_full", 17, 2u, new Func<StringScanner, RubyRegex, bool, bool, object>(StringScanner.ScanFull));
			LibraryInitializer.DefineLibraryMethod(module, "scan_until", 17, 2u, new Func<StringScanner, RubyRegex, object>(StringScanner.ScanUntil));
			LibraryInitializer.DefineLibraryMethod(module, "search_full", 17, 2u, new Func<StringScanner, RubyRegex, bool, bool, object>(StringScanner.SearchFull));
			LibraryInitializer.DefineLibraryMethod(module, "skip", 17, 2u, new Func<StringScanner, RubyRegex, int?>(StringScanner.Skip));
			LibraryInitializer.DefineLibraryMethod(module, "skip_until", 17, 2u, new Func<StringScanner, RubyRegex, int?>(StringScanner.SkipUntil));
			LibraryInitializer.DefineLibraryMethod(module, "string", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.GetString));
			LibraryInitializer.DefineLibraryMethod(module, "string=", 17, 4u, new Func<RubyContext, StringScanner, MutableString, MutableString>(StringScanner.SetString));
			LibraryInitializer.DefineLibraryMethod(module, "terminate", 17, 0u, new Func<StringScanner, StringScanner>(StringScanner.Clear));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 17, 0u, new Func<StringScanner, MutableString>(StringScanner.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "unscan", 17, 0u, new Func<StringScanner, StringScanner>(StringScanner.Unscan));
		}

		private static void LoadStringScanner_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "must_C_version", 33, 0u, new Func<object, object>(StringScanner.MustCVersion));
		}
	}
}
