using System;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.ParseTree
{
	public sealed class ParseTreeLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyModule module = DefineGlobalModule("IronRuby", typeof(Ruby), 0, null, null, null, RubyModule.EmptyArray);
			RubyModule value = DefineModule("IronRuby::ParseTree", typeof(IronRubyOps.ParseTreeOps), 8, LoadIronRuby__ParseTree_Instance, null, null, RubyModule.EmptyArray);
			LibraryInitializer.SetConstant(module, "ParseTree", value);
		}

		private static void LoadIronRuby__ParseTree_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "parse_tree_for_meth", 17, 131078u, new Func<object, RubyModule, string, bool, RubyArray>(IronRubyOps.ParseTreeOps.CreateParseTreeForMethod));
			LibraryInitializer.DefineLibraryMethod(module, "parse_tree_for_str", 17, 12u, new Func<RubyScope, object, MutableString, MutableString, int, RubyArray>(IronRubyOps.ParseTreeOps.CreateParseTreeForString));
		}
	}
}
