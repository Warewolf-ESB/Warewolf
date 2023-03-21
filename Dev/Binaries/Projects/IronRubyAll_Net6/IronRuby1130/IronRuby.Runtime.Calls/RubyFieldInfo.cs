using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Calls
{
	internal class RubyFieldInfo : RubyMemberInfo
	{
		private readonly FieldInfo _fieldInfo;

		private readonly bool _isSetter;

		private readonly bool _isDetached;

		internal override bool IsRubyMember
		{
			get
			{
				return _isDetached;
			}
		}

		internal override bool IsDataMember
		{
			get
			{
				return true;
			}
		}

		public RubyFieldInfo(FieldInfo fieldInfo, RubyMemberFlags flags, RubyModule declaringModule, bool isSetter, bool isDetached)
			: base(flags, declaringModule)
		{
			_fieldInfo = fieldInfo;
			_isSetter = isSetter;
			_isDetached = isDetached;
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyFieldInfo(_fieldInfo, flags, module, _isSetter, true);
		}

		public override MemberInfo[] GetMembers()
		{
			return new MemberInfo[1] { _fieldInfo };
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
			Expression expression = (_fieldInfo.IsStatic ? null : Expression.Convert(args.TargetExpression, _fieldInfo.DeclaringType));
			if (_isSetter)
			{
				IList<DynamicMetaObject> list = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
				if (!metaBuilder.Error)
				{
					metaBuilder.Result = Expression.Assign(Expression.Field(expression, _fieldInfo), Converter.ConvertExpression(list[0].Expression, _fieldInfo.FieldType, args.RubyContext, args.MetaContext.Expression, true));
				}
				return;
			}
			RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
			if (!metaBuilder.Error)
			{
				if (_fieldInfo.IsLiteral)
				{
					metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Constant(_fieldInfo.GetValue(null));
				}
				else
				{
					metaBuilder.Result = Expression.Field(expression, _fieldInfo);
				}
			}
		}
	}
}
