using System;
using System.Dynamic;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime
{
	internal sealed class MissingBlockParam
	{
		internal sealed class Meta : DynamicMetaObject, IRestrictedMetaObject
		{
			internal static readonly DynamicMetaObject Instance = new Meta();

			private Meta()
				: base(Microsoft.Scripting.Ast.Utils.Constant(null, typeof(MissingBlockParam)), BindingRestrictions.Empty)
			{
			}

			public DynamicMetaObject Restrict(Type type)
			{
				return this;
			}
		}
	}
}
