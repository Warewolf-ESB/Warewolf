using System;
using System.Diagnostics;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public class RubyMemberInfo
	{
		internal static readonly RubyMemberInfo UndefinedMethod = new RubyMemberInfo(RubyMemberFlags.Empty);

		internal static readonly RubyMemberInfo HiddenMethod = new RubyMemberInfo(RubyMemberFlags.Empty);

		internal static readonly RubyMemberInfo InteropMember = new RubyMemberInfo(RubyMemberFlags.Public);

		private readonly RubyMemberFlags _flags;

		private readonly RubyModule _declaringModule;

		private bool _invalidateSitesOnOverride;

		private bool _invalidateGroupsOnRemoval;

		public RubyMethodVisibility Visibility
		{
			get
			{
				return (RubyMethodVisibility)(_flags & RubyMemberFlags.VisibilityMask);
			}
		}

		public bool IsProtected
		{
			get
			{
				return (_flags & RubyMemberFlags.Protected) != 0;
			}
		}

		public bool IsPrivate
		{
			get
			{
				return (_flags & RubyMemberFlags.Private) != 0;
			}
		}

		public bool IsPublic
		{
			get
			{
				return (_flags & RubyMemberFlags.Public) != 0;
			}
		}

		internal bool IsEmpty
		{
			get
			{
				return (_flags & RubyMemberFlags.Empty) != 0;
			}
		}

		internal virtual bool IsSuperForwarder
		{
			get
			{
				return false;
			}
		}

		internal virtual bool IsRubyMember
		{
			get
			{
				return true;
			}
		}

		internal virtual bool IsDataMember
		{
			get
			{
				return false;
			}
		}

		internal bool IsRemovable
		{
			get
			{
				if (IsRubyMember && !IsHidden && !IsUndefined)
				{
					return !IsInteropMember;
				}
				return false;
			}
		}

		internal RubyMemberFlags Flags
		{
			get
			{
				return _flags;
			}
		}

		[DebuggerDisplay("{_invalidateGroupsOnRemoval}")]
		internal bool InvalidateGroupsOnRemoval
		{
			get
			{
				return _invalidateGroupsOnRemoval;
			}
			set
			{
				_invalidateGroupsOnRemoval = value;
			}
		}

		[DebuggerDisplay("{_invalidateSitesOnOverride}")]
		internal bool InvalidateSitesOnOverride
		{
			get
			{
				return _invalidateSitesOnOverride;
			}
		}

		public RubyModule DeclaringModule
		{
			get
			{
				return _declaringModule;
			}
		}

		public RubyContext Context
		{
			get
			{
				return _declaringModule.Context;
			}
		}

		public bool IsUndefined
		{
			get
			{
				return object.ReferenceEquals(this, UndefinedMethod);
			}
		}

		public bool IsHidden
		{
			get
			{
				return object.ReferenceEquals(this, HiddenMethod);
			}
		}

		public bool IsInteropMember
		{
			get
			{
				return object.ReferenceEquals(this, InteropMember);
			}
		}

		internal virtual void SetInvalidateSitesOnOverride()
		{
			_invalidateSitesOnOverride = true;
		}

		internal static void SetInvalidateSitesOnOverride(RubyMemberInfo member)
		{
			member._invalidateSitesOnOverride = true;
		}

		public virtual int GetArity()
		{
			return 0;
		}

		private RubyMemberInfo(RubyMemberFlags flags)
		{
			_flags = flags;
		}

		internal RubyMemberInfo(RubyMemberFlags flags, RubyModule declaringModule)
		{
			_flags = flags;
			_declaringModule = declaringModule;
		}

		protected internal virtual RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			throw Assert.Unreachable;
		}

		public override string ToString()
		{
			if (!IsHidden)
			{
				if (!IsUndefined)
				{
					return GetType().Name + " " + _flags.ToString() + " (" + _declaringModule.Name + ")";
				}
				return "<undefined>";
			}
			return "<hidden>";
		}

		public virtual MemberInfo[] GetMembers()
		{
			throw Assert.Unreachable;
		}

		public virtual RubyArray GetRubyParameterArray()
		{
			if (_declaringModule == null)
			{
				return new RubyArray();
			}
			RubyContext context = _declaringModule.Context;
			RubyArray rubyArray = new RubyArray();
			int arity = GetArity();
			int num = ((arity < 0) ? (-arity - 1) : arity);
			RubySymbol item = context.CreateAsciiSymbol("req");
			for (int i = 0; i < num; i++)
			{
				rubyArray.Add(new RubyArray { item });
			}
			if (arity < 0)
			{
				rubyArray.Add(new RubyArray { context.CreateAsciiSymbol("rest") });
			}
			return rubyArray;
		}

		public virtual RubyMemberInfo TryBindGenericParameters(Type[] typeArguments)
		{
			return null;
		}

		public virtual RubyMemberInfo TrySelectOverload(Type[] parameterTypes)
		{
			throw Assert.Unreachable;
		}

		internal virtual MemberDispatcher GetDispatcher(Type delegateType, RubyCallSignature signature, object target, int version)
		{
			return null;
		}

		internal virtual void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			throw Assert.Unreachable;
		}

		internal virtual void BuildMethodMissingCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			args.InsertMethodName(name);
			BuildCallNoFlow(metaBuilder, args, Symbols.MethodMissing);
		}

		internal void BuildCall(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			BuildCallNoFlow(metaBuilder, args, name);
			metaBuilder.BuildControlFlow(args);
		}

		internal void BuildMethodMissingCall(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			BuildMethodMissingCallNoFlow(metaBuilder, args, name);
			metaBuilder.BuildControlFlow(args);
		}

		internal virtual void BuildSuperCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name, RubyModule declaringModule)
		{
			BuildCallNoFlow(metaBuilder, args, name);
		}

		internal void BuildSuperCall(MetaObjectBuilder metaBuilder, CallArguments args, string name, RubyModule declaringModule)
		{
			BuildSuperCallNoFlow(metaBuilder, args, name, declaringModule);
			metaBuilder.BuildControlFlow(args);
		}
	}
}
