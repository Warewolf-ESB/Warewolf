using System;
using System.Reflection;
using IronRuby.Builtins;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyLibraryMethodInfo : RubyMethodGroupBase
	{
		private readonly LibraryOverload[] _overloads;

		internal LibraryOverload[] Overloads
		{
			get
			{
				return _overloads;
			}
		}

		internal override SelfCallConvention CallConvention
		{
			get
			{
				return SelfCallConvention.SelfIsParameter;
			}
		}

		internal override bool ImplicitProtocolConversions
		{
			get
			{
				return false;
			}
		}

		protected internal override OverloadInfo[] MethodBases
		{
			get
			{
				return base.MethodBases ?? SetMethodBasesNoLock(_overloads);
			}
		}

		internal RubyLibraryMethodInfo(LibraryOverload[] overloads, RubyMemberFlags flags, RubyModule declaringModule)
			: base(null, flags, declaringModule)
		{
			_overloads = overloads;
		}

		public RubyLibraryMethodInfo(LibraryOverload[] overloads, RubyMethodVisibility visibility, RubyModule declaringModule)
			: this(overloads, (RubyMemberFlags)(visibility & (RubyMethodVisibility)7), declaringModule)
		{
			ContractUtils.RequiresNotNull(declaringModule, "declaringModule");
			ContractUtils.RequiresNotNullItems(overloads, "overloads");
		}

		private RubyLibraryMethodInfo(RubyLibraryMethodInfo info, OverloadInfo[] methods)
			: base(methods, info.Flags, info.DeclaringModule)
		{
		}

		public override MemberInfo[] GetMembers()
		{
			return ArrayUtils.ConvertAll(MethodBases, (OverloadInfo o) => o.ReflectionInfo);
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyLibraryMethodInfo(_overloads, flags, module);
		}

		protected override RubyMemberInfo Copy(OverloadInfo[] methods)
		{
			return new RubyLibraryMethodInfo(this, methods);
		}

		internal override MemberDispatcher GetDispatcher(Type delegateType, RubyCallSignature signature, object target, int version)
		{
			if (!(target is IRubyObject))
			{
				return null;
			}
			int arity;
			if (!base.IsEmpty || (arity = GetArity()) != 1)
			{
				return null;
			}
			return MethodDispatcher.CreateRubyObjectDispatcher(delegateType, new Func<object, Proc, object, object>(EmptyRubyMethodStub1), arity, signature.HasScope, signature.HasBlock, version);
		}

		public static object EmptyRubyMethodStub1(object self, Proc block, object arg0)
		{
			return null;
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			RubyMethodGroupBase.BuildCallNoFlow(metaBuilder, args, name, MethodBases, CallConvention, ImplicitProtocolConversions);
		}
	}
}
