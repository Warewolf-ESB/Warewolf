using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;

namespace IronRuby.Runtime
{
	public sealed class RubyMethodScope : RubyClosureScope
	{
		internal const int HasBlockFlag = 1;

		internal const int HasUnsplatFlag = 2;

		private readonly RubyModule _declaringModule;

		private readonly string _definitionName;

		private readonly Proc _blockParameter;

		private readonly int _visibleParameterCountAndSignatureFlags;

		internal bool HasUnsplatParameter
		{
			get
			{
				return (_visibleParameterCountAndSignatureFlags & 2) != 0;
			}
		}

		internal bool HasBlockParameter
		{
			get
			{
				return (_visibleParameterCountAndSignatureFlags & 1) != 0;
			}
		}

		internal int VisibleParameterCount
		{
			get
			{
				return _visibleParameterCountAndSignatureFlags >> 2;
			}
		}

		public override ScopeKind Kind
		{
			get
			{
				return ScopeKind.Method;
			}
		}

		public override bool InheritsLocalVariables
		{
			get
			{
				return false;
			}
		}

		internal RubyModule DeclaringModule
		{
			get
			{
				return _declaringModule;
			}
		}

		internal string DefinitionName
		{
			get
			{
				return _definitionName;
			}
		}

		public Proc BlockParameter
		{
			get
			{
				return _blockParameter;
			}
		}

		internal RubyMethodScope(MutableTuple locals, string[] variableNames, int visibleParameterCountAndSignatureFlags, RubyScope parent, RubyModule declaringModule, string definitionName, object selfObject, Proc blockParameter, InterpretedFrame interpretedFrame)
		{
			_activeFlowControlScope = this;
			_parent = parent;
			_top = parent.Top;
			_selfObject = selfObject;
			_methodAttributes = RubyMethodAttributes.PublicInstance;
			_locals = locals;
			_variableNames = variableNames;
			_visibleParameterCountAndSignatureFlags = visibleParameterCountAndSignatureFlags;
			base.InterpretedFrame = interpretedFrame;
			_declaringModule = declaringModule;
			_definitionName = definitionName;
			_blockParameter = blockParameter;
			InitializeRfc(blockParameter);
		}

		public string[] GetVisibleParameterNames()
		{
			int num = (HasBlockParameter ? 1 : 0);
			string[] array = new string[VisibleParameterCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _variableNames[num + i];
			}
			return array;
		}
	}
}
