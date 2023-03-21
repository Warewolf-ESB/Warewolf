using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[UndefineMethod("module_function")]
	[RubyClass("Class", Extends = typeof(RubyClass), Inherits = typeof(RubyModule), Restrictions = (ModuleRestrictions.Builtin | ModuleRestrictions.NoUnderlyingType))]
	[UndefineMethod("extend_object")]
	[UndefineMethod("append_features")]
	public sealed class ClassOps
	{
		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static void Reinitialize(BlockParam body, RubyClass self, [Optional] RubyClass superClass)
		{
			throw RubyExceptions.CreateTypeError("already initialized class");
		}

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static void InitializeCopy(RubyClass self, [NotNull] RubyClass other)
		{
			self.InitializeClassCopy(other);
		}

		[RubyMethod("allocate")]
		public static RuleGenerator Allocate()
		{
			return RuleGenerators.InstanceAllocator;
		}

		[RubyMethod("new")]
		public static RuleGenerator New()
		{
			return RuleGenerators.InstanceConstructor;
		}

		[RubyMethod("superclass")]
		public static RubyClass GetSuperclass(RubyClass self)
		{
			if (self.IsSingletonClass)
			{
				RubyClass immediateClass = self.ImmediateClass;
				if (!immediateClass.IsDummySingletonClass)
				{
					return immediateClass;
				}
				return self;
			}
			return self.SuperClass;
		}

		[RubyMethod("inherited", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
		public static void Inherited(object self, object subclass)
		{
		}

		[RubyMethod("clr_new")]
		public static RuleGenerator ClrNew()
		{
			return delegate(MetaObjectBuilder metaBuilder, CallArguments args, string name)
			{
				((RubyClass)args.Target).BuildClrObjectConstruction(metaBuilder, args, name);
			};
		}

		[RubyMethod("clr_ctor")]
		[RubyMethod("clr_constructor")]
		public static RubyMethod GetClrConstructor(RubyClass self)
		{
			if (self.TypeTracker == null)
			{
				throw RubyExceptions.CreateNotClrTypeError(self);
			}
			RubyMemberInfo method;
			if (!self.TryGetClrConstructor(out method))
			{
				throw RubyOps.MakeConstructorUndefinedError(self);
			}
			return new RubyMethod(self, method, ".ctor");
		}
	}
}
