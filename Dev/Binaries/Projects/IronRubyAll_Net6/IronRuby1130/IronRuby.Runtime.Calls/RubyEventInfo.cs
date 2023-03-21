using System;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyEventInfo : RubyMemberInfo
	{
		private readonly EventTracker _tracker;

		private readonly bool _isDetached;

		public EventTracker Tracker
		{
			get
			{
				return _tracker;
			}
		}

		internal override bool IsDataMember
		{
			get
			{
				return true;
			}
		}

		internal override bool IsRubyMember
		{
			get
			{
				return _isDetached;
			}
		}

		public RubyEventInfo(EventTracker tracker, RubyMemberFlags flags, RubyModule declaringModule, bool isDetached)
			: base(flags, declaringModule)
		{
			_tracker = tracker;
			_isDetached = isDetached;
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyEventInfo(_tracker, flags, module, true);
		}

		public override MemberInfo[] GetMembers()
		{
			return new MemberInfo[1] { _tracker.Event };
		}

		public override RubyMemberInfo TrySelectOverload(Type[] parameterTypes)
		{
			if (parameterTypes.Length != 0)
			{
				return null;
			}
			return this;
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			if (args.Signature.ArgumentCount == 0)
			{
				if (args.Signature.HasBlock)
				{
					metaBuilder.Result = Methods.HookupEvent.OpCall(Microsoft.Scripting.Ast.Utils.Constant(this), args.TargetExpression, Expression.Convert(args.GetBlockExpression(), typeof(Proc)));
				}
				else
				{
					metaBuilder.Result = Methods.CreateEvent.OpCall(Microsoft.Scripting.Ast.Utils.Constant(this), args.TargetExpression, Microsoft.Scripting.Ast.Utils.Constant(name));
				}
			}
			else
			{
				metaBuilder.SetError(Methods.MakeWrongNumberOfArgumentsError.OpCall(Expression.Constant(args.Signature.ArgumentCount), Expression.Constant(0)));
			}
		}
	}
}
