using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public sealed class ConvertToSAction : RubyConversionAction
	{
		public override Type ReturnType
		{
			get
			{
				return typeof(MutableString);
			}
		}

		public static ConvertToSAction Make(RubyContext context)
		{
			return context.MetaBinderFactory.Conversion<ConvertToSAction>();
		}

		public static ConvertToSAction MakeShared()
		{
			return RubyMetaBinderFactory.Shared.Conversion<ConvertToSAction>();
		}

		public override Expression CreateExpression()
		{
			return Methods.GetMethod(GetType(), "MakeShared").OpCall();
		}

		protected override DynamicMetaObjectBinder GetInteropBinder(RubyContext context, IList<DynamicMetaObject> args, out MethodInfo postConverter)
		{
			postConverter = Methods.StringToMutableString;
			return context.MetaBinderFactory.InteropInvokeMember("ToString", new CallInfo(0));
		}

		protected override bool Build(MetaObjectBuilder metaBuilder, CallArguments args, bool defaultFallback)
		{
			BuildConversion(metaBuilder, args);
			return true;
		}

		internal static void BuildConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			if (TryImplicitConversion(metaBuilder, args))
			{
				metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);
				return;
			}
			RubyMemberInfo methodMissing = null;
			RubyClass immediateClassOf = args.RubyContext.GetImmediateClassOf(args.Target);
			RubyMemberInfo info;
			using (immediateClassOf.Context.ClassHierarchyLocker())
			{
				metaBuilder.AddTargetTypeTest(args.Target, immediateClassOf, args.TargetExpression, args.MetaContext, new string[2]
				{
					"to_s",
					Symbols.MethodMissing
				});
				info = immediateClassOf.ResolveMethodForSiteNoLock("to_s", VisibilityContext.AllVisible).Info;
				if (info == null)
				{
					methodMissing = immediateClassOf.ResolveMethodMissingForSite("to_s", RubyMethodVisibility.None);
				}
			}
			if (info != null)
			{
				info.BuildCall(metaBuilder, args, "to_s");
			}
			else
			{
				RubyCallAction.BuildMethodMissingCall(metaBuilder, args, "to_s", methodMissing, RubyMethodVisibility.None, false, true);
			}
			if (!metaBuilder.Error)
			{
				metaBuilder.Result = Methods.ToSDefaultConversion.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.MetaContext.Expression, typeof(RubyContext)), Microsoft.Scripting.Ast.Utils.Box(args.TargetExpression), Microsoft.Scripting.Ast.Utils.Box(metaBuilder.Result));
			}
		}

		private static bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			MutableString mutableString = args.Target as MutableString;
			if (mutableString != null)
			{
				metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(MutableString));
				return true;
			}
			return false;
		}
	}
}
