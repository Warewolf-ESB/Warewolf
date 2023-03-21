using System.Collections.Generic;
using Microsoft.Scripting;

namespace IronRuby.Compiler
{
	internal sealed class RuntimeLexicalScope : LexicalScope
	{
		protected override bool IsRuntimeScope
		{
			get
			{
				return true;
			}
		}

		internal override bool IsTop
		{
			get
			{
				return true;
			}
		}

		internal override bool IsStaticTop
		{
			get
			{
				return false;
			}
		}

		public RuntimeLexicalScope(List<string> names)
			: base(null, -1)
		{
			for (int i = 0; i < names.Count; i++)
			{
				AddVariable(names[i], SourceSpan.None);
			}
		}
	}
}
