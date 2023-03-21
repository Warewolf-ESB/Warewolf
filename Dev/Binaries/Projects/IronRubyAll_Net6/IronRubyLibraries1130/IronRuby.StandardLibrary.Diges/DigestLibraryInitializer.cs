using System;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Digest
{
	public sealed class DigestLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(object));
			RubyModule module = DefineGlobalModule("Digest", typeof(Digest), 8, null, LoadDigest_Class, null, RubyModule.EmptyArray);
			RubyModule rubyModule = DefineModule("Digest::Instance", typeof(Digest.Instance), 8, LoadDigest__Instance_Instance, null, null, RubyModule.EmptyArray);
			RubyClass rubyClass = DefineClass("Digest::Class", typeof(Digest.Class), 8, @class, null, LoadDigest__Class_Class, null, new RubyModule[1] { rubyModule });
			RubyClass rubyClass2 = DefineClass("Digest::Base", typeof(Digest.Base), 8, rubyClass, LoadDigest__Base_Instance, null, null, RubyModule.EmptyArray);
			RubyClass value = DefineClass("Digest::MD5", typeof(Digest.MD5), 8, rubyClass2, null, null, null, RubyModule.EmptyArray);
			RubyClass value2 = DefineClass("Digest::SHA1", typeof(Digest.SHA1), 8, rubyClass2, null, null, null, RubyModule.EmptyArray);
			RubyClass value3 = DefineClass("Digest::SHA256", typeof(Digest.SHA256), 8, rubyClass2, null, null, null, RubyModule.EmptyArray);
			RubyClass value4 = DefineClass("Digest::SHA384", typeof(Digest.SHA384), 8, rubyClass2, null, null, null, RubyModule.EmptyArray);
			RubyClass value5 = DefineClass("Digest::SHA512", typeof(Digest.SHA512), 8, rubyClass2, null, null, null, RubyModule.EmptyArray);
			LibraryInitializer.SetConstant(module, "Instance", rubyModule);
			LibraryInitializer.SetConstant(module, "Class", rubyClass);
			LibraryInitializer.SetConstant(module, "Base", rubyClass2);
			LibraryInitializer.SetConstant(module, "MD5", value);
			LibraryInitializer.SetConstant(module, "SHA1", value2);
			LibraryInitializer.SetConstant(module, "SHA256", value3);
			LibraryInitializer.SetConstant(module, "SHA384", value4);
			LibraryInitializer.SetConstant(module, "SHA512", value5);
		}

		private static void LoadDigest_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "const_missing", 33, 65538u, new Func<RubyModule, string, object>(Digest.ConstantMissing));
			LibraryInitializer.DefineLibraryMethod(module, "hexencode", 33, 2u, new Func<RubyModule, MutableString, MutableString>(Digest.HexEncode));
		}

		private static void LoadDigest__Base_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<<", 17, 0u, new Func<RubyContext, Digest.Base, MutableString, Digest.Base>(Digest.Base.Update));
			LibraryInitializer.DefineLibraryMethod(module, "finish", 18, 0u, new Func<RubyContext, Digest.Base, MutableString>(Digest.Base.Finish));
			LibraryInitializer.DefineLibraryMethod(module, "reset", 17, 0u, new Func<RubyContext, Digest.Base, Digest.Base>(Digest.Base.Reset));
			LibraryInitializer.DefineLibraryMethod(module, "update", 17, 0u, new Func<RubyContext, Digest.Base, MutableString, Digest.Base>(Digest.Base.Update));
		}

		private static void LoadDigest__Class_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "digest", 33, 262152u, 0u, new Func<CallSiteStorage<Func<CallSite, RubyClass, object>>, CallSiteStorage<Func<CallSite, object, MutableString, object>>, RubyClass, MutableString, MutableString>(Digest.Class.Digest), new Func<RubyClass, MutableString>(Digest.Class.Digest));
			LibraryInitializer.DefineLibraryMethod(module, "hexdigest", 33, 131076u, 0u, new Func<CallSiteStorage<Func<CallSite, object, MutableString, object>>, RubyClass, MutableString, MutableString>(Digest.Class.HexDigest), new Func<RubyClass, MutableString>(Digest.Class.HexDigest));
		}

		private static void LoadDigest__Instance_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "digest", 17, 0u, 524304u, new Func<CallSiteStorage<Func<CallSite, object, object, object>>, CallSiteStorage<Func<CallSite, RubyClass, object>>, CallSiteStorage<Func<CallSite, object, object>>, object, MutableString>(Digest.Instance.Digest), new Func<CallSiteStorage<Func<CallSite, object, MutableString, object>>, CallSiteStorage<Func<CallSite, object, object>>, CallSiteStorage<Func<CallSite, object, object>>, object, MutableString, MutableString>(Digest.Instance.Digest));
			LibraryInitializer.DefineLibraryMethod(module, "digest!", 17, 0u, new Func<CallSiteStorage<Func<CallSite, object, object>>, CallSiteStorage<Func<CallSite, object, object>>, object, MutableString>(Digest.Instance.DigestNew));
			LibraryInitializer.DefineLibraryMethod(module, "hexdigest", 17, 0u, 524304u, new Func<CallSiteStorage<Func<CallSite, object, object, object>>, CallSiteStorage<Func<CallSite, RubyClass, object>>, CallSiteStorage<Func<CallSite, object, object>>, object, MutableString>(Digest.Instance.HexDigest), new Func<CallSiteStorage<Func<CallSite, object, MutableString, object>>, CallSiteStorage<Func<CallSite, object, object>>, CallSiteStorage<Func<CallSite, object, object>>, object, MutableString, MutableString>(Digest.Instance.HexDigest));
			LibraryInitializer.DefineLibraryMethod(module, "hexdigest!", 17, 0u, new Func<CallSiteStorage<Func<CallSite, object, object>>, CallSiteStorage<Func<CallSite, object, object>>, object, MutableString>(Digest.Instance.HexDigestNew));
		}
	}
}
