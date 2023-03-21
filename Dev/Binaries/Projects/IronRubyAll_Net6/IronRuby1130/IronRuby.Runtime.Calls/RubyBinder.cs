using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyBinder : DefaultBinder
	{
		private readonly RubyContext _context;

		public override bool PrivateBinding
		{
			get
			{
				return _context.DomainManager.Configuration.PrivateBinding;
			}
		}

		internal RubyBinder(RubyContext context)
		{
			_context = context;
		}

		public override string GetTypeName(Type t)
		{
			return _context.GetTypeName(t, true);
		}

		public override string GetObjectTypeName(object arg)
		{
			return _context.GetClassDisplayName(arg);
		}

		public override Expression ConvertExpression(Expression expr, Type toType, ConversionResultKind kind, OverloadResolverFactory context)
		{
			throw new InvalidOperationException("OBSOLETE");
		}

		public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level)
		{
			return Converter.CanConvertFrom(null, fromType, toType, toNotNullable, level, false, false).IsConvertible;
		}

		public override Candidate PreferConvert(Type t1, Type t2)
		{
			return Converter.PreferConvert(t1, t2);
		}

		internal static object[] ToValues(DynamicMetaObject[] args, int start)
		{
			object[] array = new object[args.Length - start];
			for (int i = Math.Max(0, -start); i < array.Length; i++)
			{
				array[i] = args[start + i].Value;
			}
			return array;
		}

		[Conditional("DEBUG")]
		internal static void DumpPrecompiledRule(CallSiteBinder binder, MemberDispatcher dispatcher)
		{
		}

		[Conditional("DEBUG")]
		internal static void DumpRule(CallSiteBinder binder, BindingRestrictions restrictions, Expression expr)
		{
		}
	}
}
