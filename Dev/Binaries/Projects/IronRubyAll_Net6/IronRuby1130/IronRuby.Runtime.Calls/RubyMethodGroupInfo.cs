using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public class RubyMethodGroupInfo : RubyMethodGroupBase
	{
		private readonly bool _isStatic;

		internal override SelfCallConvention CallConvention
		{
			get
			{
				if (!_isStatic)
				{
					return SelfCallConvention.SelfIsInstance;
				}
				return SelfCallConvention.NoSelf;
			}
		}

		internal bool IsStatic
		{
			get
			{
				return _isStatic;
			}
		}

		internal override bool ImplicitProtocolConversions
		{
			get
			{
				return true;
			}
		}

		internal RubyMethodGroupInfo(OverloadInfo[] methods, RubyModule declaringModule, bool isStatic)
			: base(methods, RubyMemberFlags.Public, declaringModule)
		{
			_isStatic = isStatic;
		}

		private RubyMethodGroupInfo(RubyMethodGroupInfo info, RubyMemberFlags flags, RubyModule module)
			: base(info.MethodBases, flags, module)
		{
			_isStatic = info._isStatic;
		}

		private RubyMethodGroupInfo(RubyMethodGroupInfo info, OverloadInfo[] methods)
			: base(methods, info.Flags, info.DeclaringModule)
		{
			_isStatic = info._isStatic;
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyMethodGroupInfo(this, flags, module);
		}

		protected override RubyMemberInfo Copy(OverloadInfo[] methods)
		{
			return new RubyMethodGroupInfo(this, methods);
		}

		public override MemberInfo[] GetMembers()
		{
			return ArrayUtils.ConvertAll(MethodBases, (OverloadInfo o) => o.ReflectionInfo);
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			IList<OverloadInfo> visibleOverloads = RubyMethodGroupBase.GetVisibleOverloads(args, MethodBases, false);
			if (visibleOverloads.Count == 0)
			{
				metaBuilder.SetError(Methods.MakeClrProtectedMethodCalledError.OpCall(args.MetaContext.Expression, args.MetaTarget.Expression, Expression.Constant(name)));
			}
			else
			{
				RubyMethodGroupBase.BuildCallNoFlow(metaBuilder, args, name, visibleOverloads, CallConvention, ImplicitProtocolConversions);
			}
		}
	}
}
