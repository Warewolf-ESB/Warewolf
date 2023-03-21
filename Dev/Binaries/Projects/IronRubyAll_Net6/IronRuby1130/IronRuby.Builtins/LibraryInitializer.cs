using System;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public abstract class LibraryInitializer
	{
		private RubyContext _context;

		private bool _builtin;

		internal static int MaxOverloads = 4;

		protected RubyContext Context
		{
			get
			{
				return _context;
			}
		}

		internal void LoadModules(RubyContext context, bool builtin)
		{
			_context = context;
			_builtin = builtin;
			LoadModules();
		}

		private void PublishModule(string name, RubyModule module)
		{
			_context.ObjectClass.SetConstant(name, module);
			if ((module.Restrictions & ModuleRestrictions.NotPublished) == 0)
			{
				module.Publish(name);
			}
		}

		protected RubyClass DefineGlobalClass(string name, Type type, int attributes, RubyClass super, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] mixins, Delegate factory)
		{
			return DefineGlobalClass(name, type, attributes, super, instanceTrait, classTrait, constantsInitializer, mixins, new Delegate[1] { factory });
		}

		protected RubyClass DefineGlobalClass(string name, Type type, int restrictions, RubyClass super, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] mixins, params Delegate[] factories)
		{
			RubyClass rubyClass = _context.DefineLibraryClass(name, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, (ModuleRestrictions)restrictions, _builtin);
			PublishModule(name, rubyClass);
			return rubyClass;
		}

		protected RubyClass DefineClass(string name, Type type, int restrictions, RubyClass super, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] mixins, params Delegate[] factories)
		{
			return _context.DefineLibraryClass(name, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, (ModuleRestrictions)restrictions, _builtin);
		}

		protected RubyClass ExtendClass(Type type, int restrictions, RubyClass super, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] mixins, params Delegate[] factories)
		{
			return _context.DefineLibraryClass(null, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, (ModuleRestrictions)restrictions, _builtin);
		}

		protected RubyModule DefineGlobalModule(string name, Type type, int restrictions, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] mixins)
		{
			RubyModule rubyModule = _context.DefineLibraryModule(name, type, instanceTrait, classTrait, constantsInitializer, mixins, (ModuleRestrictions)restrictions, _builtin);
			PublishModule(name, rubyModule);
			return rubyModule;
		}

		protected RubyModule DefineModule(string name, Type type, int restrictions, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, params RubyModule[] mixins)
		{
			return _context.DefineLibraryModule(name, type, instanceTrait, classTrait, constantsInitializer, mixins, (ModuleRestrictions)restrictions, _builtin);
		}

		protected RubyModule ExtendModule(Type type, int restrictions, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, params RubyModule[] mixins)
		{
			return _context.DefineLibraryModule(null, type, instanceTrait, classTrait, constantsInitializer, mixins, (ModuleRestrictions)restrictions, _builtin);
		}

		protected object DefineSingleton(Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, params RubyModule[] mixins)
		{
			RubyModule[] expandedMixins;
			using (_context.ClassHierarchyLocker())
			{
				expandedMixins = RubyModule.ExpandMixinsNoLock(_context.ObjectClass, mixins);
			}
			object obj = new RubyObject(_context.ObjectClass);
			_context.GetOrCreateInstanceSingleton(obj, instanceTrait, classTrait, constantsInitializer, expandedMixins);
			return obj;
		}

		private static bool SkipDefinition(RubyModule module, int attributes)
		{
			return attributes >> 16 > (int)module.Context.RubyOptions.Compatibility;
		}

		internal static void DefineLibraryMethod(RubyModule module, string name, int attributes, LibraryOverload[] overloads)
		{
			RubyMemberFlags flags = (RubyMemberFlags)(attributes & 0xF);
			bool noEvent = (attributes & 0x40) != 0;
			SetLibraryMethod(module, name, new RubyLibraryMethodInfo(overloads, flags, module), noEvent);
		}

		[CLSCompliant(false)]
		public static void DefineLibraryMethod(RubyModule module, string name, int attributes, uint[] overloadAttributes, params Delegate[] overloads)
		{
			if (!SkipDefinition(module, attributes))
			{
				LibraryOverload[] array = new LibraryOverload[overloads.Length];
				for (int i = 0; i < overloads.Length; i++)
				{
					array[i] = LibraryOverload.Create(overloads[i], overloadAttributes[i]);
				}
				DefineLibraryMethod(module, name, attributes, array);
			}
		}

		[CLSCompliant(false)]
		public static void DefineLibraryMethod(RubyModule module, string name, int attributes, uint overloadAttributes1, Delegate overload1)
		{
			if (!SkipDefinition(module, attributes))
			{
				DefineLibraryMethod(module, name, attributes, new LibraryOverload[1] { LibraryOverload.Create(overload1, overloadAttributes1) });
			}
		}

		[CLSCompliant(false)]
		public static void DefineLibraryMethod(RubyModule module, string name, int attributes, uint overloadAttributes1, uint overloadAttributes2, Delegate overload1, Delegate overload2)
		{
			if (!SkipDefinition(module, attributes))
			{
				DefineLibraryMethod(module, name, attributes, new LibraryOverload[2]
				{
					LibraryOverload.Create(overload1, overloadAttributes1),
					LibraryOverload.Create(overload2, overloadAttributes2)
				});
			}
		}

		[CLSCompliant(false)]
		public static void DefineLibraryMethod(RubyModule module, string name, int attributes, uint overloadAttributes1, uint overloadAttributes2, uint overloadAttributes3, Delegate overload1, Delegate overload2, Delegate overload3)
		{
			if (!SkipDefinition(module, attributes))
			{
				DefineLibraryMethod(module, name, attributes, new LibraryOverload[3]
				{
					LibraryOverload.Create(overload1, overloadAttributes1),
					LibraryOverload.Create(overload2, overloadAttributes2),
					LibraryOverload.Create(overload3, overloadAttributes3)
				});
			}
		}

		[CLSCompliant(false)]
		public static void DefineLibraryMethod(RubyModule module, string name, int attributes, uint overloadAttributes1, uint overloadAttributes2, uint overloadAttributes3, uint overloadAttributes4, Delegate overload1, Delegate overload2, Delegate overload3, Delegate overload4)
		{
			if (!SkipDefinition(module, attributes))
			{
				DefineLibraryMethod(module, name, attributes, new LibraryOverload[4]
				{
					LibraryOverload.Create(overload1, overloadAttributes1),
					LibraryOverload.Create(overload2, overloadAttributes2),
					LibraryOverload.Create(overload3, overloadAttributes3),
					LibraryOverload.Create(overload4, overloadAttributes4)
				});
			}
		}

		public static void DefineRuleGenerator(RubyModule module, string name, int attributes, RuleGenerator generator)
		{
			RubyMemberFlags flags = (RubyMemberFlags)(attributes & 7);
			bool noEvent = (attributes & 0x40) != 0;
			SetLibraryMethod(module, name, new RubyCustomMethodInfo(generator, flags, module), noEvent);
		}

		private static void SetLibraryMethod(RubyModule module, string name, RubyMemberInfo method, bool noEvent)
		{
			RubyContext context = module.Context;
			if (noEvent)
			{
				using (context.ClassHierarchyLocker())
				{
					module.SetMethodNoMutateNoEventNoLock(context, name, method);
					return;
				}
			}
			module.AddMethod(context, name, method);
		}

		public static void SetBuiltinConstant(RubyModule module, string name, object value)
		{
			using (module.Context.ClassHierarchyLocker())
			{
				module.SetConstantNoMutateNoLock(name, value);
			}
		}

		public static void SetConstant(RubyModule module, string name, object value)
		{
			module.SetConstant(name, value);
		}

		protected RubyClass GetClass(Type type)
		{
			return _context.GetOrCreateClass(type);
		}

		protected RubyModule GetModule(Type type)
		{
			RubyModule result;
			if (!_context.TryGetModule(type, out result))
			{
				throw new NotSupportedException(string.Format("Ruby library that contains type {0} hasn't been loaded yet.", type));
			}
			return result;
		}

		protected virtual void LoadModules()
		{
			throw new NotImplementedException();
		}

		public static string GetTypeName(string libraryNamespace)
		{
			ContractUtils.RequiresNotNull(libraryNamespace, "libraryNamespace");
			return libraryNamespace.Substring(libraryNamespace.LastIndexOf(Type.Delimiter) + 1) + "LibraryInitializer";
		}

		public static string GetFullTypeName(string libraryNamespace)
		{
			return libraryNamespace + Type.Delimiter + GetTypeName(libraryNamespace);
		}

		public static string GetBuiltinsFullTypeName()
		{
			return GetFullTypeName(typeof(RubyClass).Namespace);
		}
	}
}
