using System;
using System.Reflection;
using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public class RubyLambdaMethodInfo : RubyMemberInfo
	{
		private static int _Id = 1;

		private readonly int _id;

		private readonly Proc _lambda;

		private readonly string _definitionName;

		public Proc Lambda
		{
			get
			{
				return _lambda;
			}
		}

		internal int Id
		{
			get
			{
				return _id;
			}
		}

		public string DefinitionName
		{
			get
			{
				return _definitionName;
			}
		}

		internal RubyLambdaMethodInfo(Proc block, string definitionName, RubyMemberFlags flags, RubyModule declaringModule)
			: base(flags, declaringModule)
		{
			_lambda = block.ToLambda(this);
			_definitionName = definitionName;
			_id = Interlocked.Increment(ref _Id);
		}

		public override int GetArity()
		{
			return _lambda.Dispatcher.Arity;
		}

		public override MemberInfo[] GetMembers()
		{
			return new MemberInfo[1] { _lambda.Dispatcher.Method.Method };
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyLambdaMethodInfo(_lambda, _definitionName, flags, module);
		}

		public override RubyMemberInfo TrySelectOverload(Type[] parameterTypes)
		{
			if (parameterTypes.Length != _lambda.Dispatcher.ParameterCount || !CollectionUtils.TrueForAll(parameterTypes, (Type type) => type == typeof(object)))
			{
				return null;
			}
			return this;
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			Proc.BuildCall(metaBuilder, Microsoft.Scripting.Ast.Utils.Constant(_lambda), args.TargetExpression, args);
		}
	}
}
