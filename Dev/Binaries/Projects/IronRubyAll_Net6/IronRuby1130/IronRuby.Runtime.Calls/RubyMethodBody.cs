using System;
using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyMethodBody
	{
		private readonly MethodDefinition _ast;

		private readonly SymbolDocumentInfo _document;

		private readonly RubyEncoding _encoding;

		private Delegate _delegate;

		public MethodDefinition Ast
		{
			get
			{
				return _ast;
			}
		}

		public SymbolDocumentInfo Document
		{
			get
			{
				return _document;
			}
		}

		public bool HasTarget
		{
			get
			{
				return _ast.Target != null;
			}
		}

		public string Name
		{
			get
			{
				return _ast.Name;
			}
		}

		internal RubyMethodBody(MethodDefinition ast, SymbolDocumentInfo document, RubyEncoding encoding)
		{
			_ast = ast;
			_document = document;
			_encoding = encoding;
		}

		internal Delegate GetDelegate(RubyScope declaringScope, RubyModule declaringModule)
		{
			if ((object)_delegate == null)
			{
				lock (this)
				{
					if ((object)_delegate == null)
					{
						AstGenerator gen = new AstGenerator(declaringScope.RubyContext, new RubyCompilerOptions(), _document, _encoding, false);
						LambdaExpression lambda = _ast.TransformBody(gen, declaringScope, declaringModule);
						_delegate = RubyScriptCode.CompileLambda(lambda, declaringScope.RubyContext);
					}
				}
			}
			return _delegate;
		}
	}
}
