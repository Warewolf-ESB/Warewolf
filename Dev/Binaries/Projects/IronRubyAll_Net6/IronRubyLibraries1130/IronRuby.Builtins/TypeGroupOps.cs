using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyClass(Extends = typeof(TypeGroup), Restrictions = ModuleRestrictions.None)]
	[Includes(new Type[] { typeof(Enumerable) })]
	public static class TypeGroupOps
	{
		[RubyMethod("of")]
		[RubyMethod("[]")]
		public static RubyModule Of(RubyContext context, TypeGroup self, [NotNullItems] params object[] typeArgs)
		{
			TypeTracker typeForArity = self.GetTypeForArity(typeArgs.Length);
			if (typeForArity == null)
			{
				throw RubyExceptions.CreateArgumentError("Invalid number of type arguments for `{0}'", self.Name);
			}
			Type type = ((typeArgs.Length <= 0) ? typeForArity.Type : typeForArity.Type.MakeGenericType(Protocols.ToTypes(context, typeArgs)));
			return context.GetModule(type);
		}

		[RubyMethod("of")]
		[RubyMethod("[]")]
		public static RubyModule Of(RubyContext context, TypeGroup self, int genericArity)
		{
			TypeTracker typeForArity = self.GetTypeForArity(genericArity);
			if (typeForArity == null)
			{
				throw RubyExceptions.CreateArgumentError("Type group `{0}' does not contain a type of generic arity {1}", self.Name, genericArity);
			}
			return context.GetModule(typeForArity.Type);
		}

		[RubyMethod("each")]
		public static object EachType(RubyContext context, BlockParam block, TypeGroup self)
		{
			if (block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			foreach (Type type in self.Types)
			{
				RubyModule module = context.GetModule(type);
				object blockResult;
				if (block.Yield(module, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("name")]
		[RubyMethod("to_s")]
		public static MutableString GetName(RubyContext context, TypeGroup self)
		{
			return MutableString.Create(self.Name, context.GetIdentifierEncoding());
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, TypeGroup self)
		{
			MutableString mutableString = MutableString.CreateMutable(context.GetIdentifierEncoding());
			mutableString.Append("#<TypeGroup: ");
			bool flag = true;
			foreach (KeyValuePair<int, Type> item in self.TypesByArity.ToSortedList((KeyValuePair<int, Type> x, KeyValuePair<int, Type> y) => x.Key - y.Key))
			{
				Type value = item.Value;
				if (!flag)
				{
					mutableString.Append(", ");
				}
				else
				{
					flag = false;
				}
				mutableString.Append(context.GetTypeName(value, true));
			}
			mutableString.Append('>');
			return mutableString;
		}

		private static Type GetNonGenericType(TypeGroup self)
		{
			TypeTracker typeForArity = self.GetTypeForArity(0);
			if (typeForArity == null)
			{
				throw RubyExceptions.CreateTypeError("type group doesn't include non-generic type");
			}
			return typeForArity.Type;
		}

		public static RubyClass GetNonGenericClass(RubyContext context, TypeGroup typeGroup)
		{
			Type nonGenericType = GetNonGenericType(typeGroup);
			if (nonGenericType.IsInterface)
			{
				throw RubyExceptions.CreateTypeError("cannot instantiate an interface");
			}
			return context.GetClass(nonGenericType);
		}

		private static object New(string methodName, CallSiteStorage<Func<CallSite, object, object, object>> storage, TypeGroup self, params object[] args)
		{
			RubyClass nonGenericClass = GetNonGenericClass(storage.Context, self);
			CallSite<Func<CallSite, object, object, object>> callSite = storage.GetCallSite(methodName, new RubyCallSignature(1, (RubyCallFlags)18));
			return callSite.Target(callSite, nonGenericClass, RubyOps.MakeArrayN(args));
		}

		private static object New(string methodName, CallSiteStorage<Func<CallSite, object, object, object, object>> storage, BlockParam block, TypeGroup self, params object[] args)
		{
			RubyClass nonGenericClass = GetNonGenericClass(storage.Context, self);
			CallSite<Func<CallSite, object, object, object, object>> callSite = storage.GetCallSite(methodName, new RubyCallSignature(1, (RubyCallFlags)26));
			return callSite.Target(callSite, nonGenericClass, (block != null) ? block.Proc : null, RubyOps.MakeArrayN(args));
		}

		[RubyMethod("new")]
		public static object New(CallSiteStorage<Func<CallSite, object, object, object>> storage, TypeGroup self, params object[] args)
		{
			return New("new", storage, self, args);
		}

		[RubyMethod("new")]
		public static object New(CallSiteStorage<Func<CallSite, object, object, object, object>> storage, BlockParam block, TypeGroup self, params object[] args)
		{
			return New("new", storage, block, self, args);
		}

		[RubyMethod("superclass")]
		public static RubyClass GetSuperclass(RubyContext context, TypeGroup self)
		{
			Type nonGenericType = GetNonGenericType(self);
			if (!nonGenericType.IsInterface)
			{
				return context.GetClass(nonGenericType).SuperClass;
			}
			return null;
		}

		[RubyMethod("clr_new")]
		public static object ClrNew(CallSiteStorage<Func<CallSite, object, object, object>> storage, TypeGroup self, params object[] args)
		{
			return New("clr_new", storage, self, args);
		}

		[RubyMethod("clr_new")]
		public static object ClrNew(CallSiteStorage<Func<CallSite, object, object, object, object>> storage, BlockParam block, TypeGroup self, params object[] args)
		{
			return New("clr_new", storage, block, self, args);
		}

		[RubyMethod("clr_constructor")]
		[RubyMethod("clr_ctor")]
		public static RubyMethod GetClrConstructor(RubyContext context, TypeGroup self)
		{
			return ClassOps.GetClrConstructor(GetNonGenericClass(context, self));
		}
	}
}
