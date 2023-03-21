using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public class RubyEvent : IRubyDynamicMetaObjectProvider, IDynamicMetaObjectProvider
	{
		internal sealed class Meta : RubyMetaObject<RubyEvent>
		{
			public override RubyContext Context
			{
				get
				{
					return base.Value.Info.Context;
				}
			}

			protected override MethodInfo ContextConverter
			{
				get
				{
					return Methods.GetContextFromIRubyObject;
				}
			}

			public Meta(Expression expression, BindingRestrictions restrictions, RubyEvent value)
				: base(expression, restrictions, value)
			{
			}
		}

		private readonly object _target;

		private readonly string _name;

		private readonly RubyEventInfo _info;

		public object Target
		{
			get
			{
				return _target;
			}
		}

		public RubyEventInfo Info
		{
			get
			{
				return _info;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public RubyEvent(object target, RubyEventInfo info, string name)
		{
			ContractUtils.RequiresNotNull(target, "target");
			ContractUtils.RequiresNotNull(info, "info");
			ContractUtils.RequiresNotNull(name, "name");
			_target = target;
			_info = info;
			_name = name;
		}

		public void Add(object handler)
		{
			_info.Tracker.AddHandler(_target, handler, _info.DeclaringModule.Context.DelegateCreator);
		}

		public void Remove(object handler)
		{
			_info.Tracker.RemoveHandler(_target, handler, _info.DeclaringModule.Context.EqualityComparer);
		}

		public DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new Meta(parameter, BindingRestrictions.Empty, this);
		}
	}
}
