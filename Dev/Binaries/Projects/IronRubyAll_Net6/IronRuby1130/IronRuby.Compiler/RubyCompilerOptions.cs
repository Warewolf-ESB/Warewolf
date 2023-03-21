using System;
using System.Collections.Generic;
using IronRuby.Runtime;
using Microsoft.Scripting;

namespace IronRuby.Compiler
{
	[Serializable]
	public sealed class RubyCompilerOptions : CompilerOptions
	{
		private SourceLocation _initialLocation = SourceLocation.MinValue;

		internal bool IsEval { get; set; }

		internal TopScopeFactoryKind FactoryKind { get; set; }

		internal SourceLocation InitialLocation
		{
			get
			{
				return _initialLocation;
			}
			set
			{
				_initialLocation = value;
			}
		}

		internal string TopLevelMethodName { get; set; }

		internal string[] TopLevelParameterNames { get; set; }

		internal bool TopLevelHasUnsplatParameter { get; set; }

		internal List<string> LocalNames { get; set; }

		internal RubyCompatibility Compatibility { get; set; }

		public RubyCompilerOptions()
		{
		}

		public RubyCompilerOptions(RubyOptions runtimeOptions)
		{
			Compatibility = runtimeOptions.Compatibility;
		}
	}
}
