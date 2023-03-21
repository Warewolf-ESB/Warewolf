using System;
using System.Collections;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Win32API
{
	public sealed class Win32APILibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(RubyObject));
			DefineGlobalClass("Win32API", typeof(Win32API), 8, @class, LoadWin32API_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, MutableString, MutableString, MutableString, Win32API>(Win32API.Create), new Func<RubyClass, MutableString, MutableString, MutableString, MutableString, RubySymbol, Win32API>(Win32API.Create), new Func<ConversionStorage<MutableString>, RubyClass, MutableString, MutableString, IList, MutableString, Win32API>(Win32API.Create));
		}

		private static void LoadWin32API_Instance(RubyModule module)
		{
			LibraryInitializer.DefineRuleGenerator(module, "call", 17, Win32API.Call());
			LibraryInitializer.DefineRuleGenerator(module, "Call", 17, Win32API.Call());
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 983070u, 1441852u, new Func<Win32API, MutableString, MutableString, MutableString, MutableString, Win32API>(Win32API.Reinitialize), new Func<ConversionStorage<MutableString>, Win32API, MutableString, MutableString, IList, MutableString, Win32API>(Win32API.Reinitialize));
		}
	}
}
