using System.Collections.Generic;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting;

namespace IronRuby.Compiler
{
	public abstract class LexicalScope : HybridStringDictionary<LocalVariable>
	{
		private readonly LexicalScope _outerScope;

		private readonly int _depth;

		public int Depth
		{
			get
			{
				return _depth;
			}
		}

		public LexicalScope OuterScope
		{
			get
			{
				return _outerScope;
			}
		}

		protected virtual bool IsRuntimeScope
		{
			get
			{
				return false;
			}
		}

		protected virtual bool AllowsVariableDefinitions
		{
			get
			{
				return true;
			}
		}

		internal virtual bool IsTop
		{
			get
			{
				return true;
			}
		}

		internal virtual bool IsStaticTop
		{
			get
			{
				return true;
			}
		}

		internal LexicalScope(LexicalScope outerScope)
		{
			_outerScope = outerScope;
			_depth = ((!IsTop) ? (outerScope.IsRuntimeScope ? (-1) : (outerScope._depth + 1)) : 0);
		}

		protected LexicalScope(LexicalScope outerScope, int depth)
		{
			_outerScope = outerScope;
			_depth = depth;
		}

		public LocalVariable AddVariable(string name, SourceSpan location)
		{
			LocalVariable localVariable = new LocalVariable(name, location, _depth);
			Add(name, localVariable);
			return localVariable;
		}

		public LocalVariable ResolveOrAddVariable(string name, SourceSpan location)
		{
			LocalVariable localVariable = ResolveVariable(name);
			if (localVariable != null)
			{
				return localVariable;
			}
			LexicalScope lexicalScope = this;
			while (!lexicalScope.AllowsVariableDefinitions)
			{
				lexicalScope = lexicalScope.OuterScope;
			}
			return lexicalScope.AddVariable(name, location);
		}

		public LocalVariable ResolveVariable(string name)
		{
			LexicalScope lexicalScope = this;
			while (true)
			{
				LocalVariable value;
				if (lexicalScope.TryGetValue(name, out value))
				{
					return value;
				}
				if (lexicalScope.IsTop)
				{
					break;
				}
				lexicalScope = lexicalScope.OuterScope;
			}
			return null;
		}

		internal LexicalScope GetInnermostStaticTopScope()
		{
			LexicalScope lexicalScope = this;
			while (!lexicalScope.IsStaticTop)
			{
				lexicalScope = lexicalScope.OuterScope;
			}
			return lexicalScope;
		}

		internal int AllocateClosureSlotsForLocals(int closureIndex)
		{
			int num = 0;
			foreach (KeyValuePair<string, LocalVariable> item in (IEnumerable<KeyValuePair<string, LocalVariable>>)this)
			{
				if (item.Value.ClosureIndex == -1)
				{
					item.Value.SetClosureIndex(closureIndex++);
					num++;
				}
			}
			return num;
		}
	}
}
