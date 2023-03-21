using System;
using System.Dynamic;
using System.Linq.Expressions;
using IronRuby.Compiler;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Calls
{
	public struct RubyCallSignature : IEquatable<RubyCallSignature>
	{
		private const int FlagsCount = 8;

		private const int ResolveOnlyArgumentCount = 16777215;

		private const int MaxArgumentCount = 16777214;

		private const uint FlagsMask = 255u;

		private readonly uint _countAndFlags;

		public bool HasImplicitSelf
		{
			get
			{
				return (_countAndFlags & 0x10) != 0;
			}
		}

		public bool HasScope
		{
			get
			{
				return (_countAndFlags & 1) != 0;
			}
		}

		public bool HasBlock
		{
			get
			{
				return (_countAndFlags & 8) != 0;
			}
		}

		public bool HasSplattedArgument
		{
			get
			{
				return (_countAndFlags & 2) != 0;
			}
		}

		public bool HasRhsArgument
		{
			get
			{
				return (_countAndFlags & 4) != 0;
			}
		}

		public bool IsInteropCall
		{
			get
			{
				return (_countAndFlags & 0x20) != 0;
			}
		}

		public bool IsVirtualCall
		{
			get
			{
				return (_countAndFlags & 0x40) != 0;
			}
		}

		public bool HasImplicitArguments
		{
			get
			{
				return (_countAndFlags & 0x80) != 0;
			}
		}

		public bool IsSuperCall
		{
			get
			{
				return (_countAndFlags & 0x80) != 0;
			}
		}

		public bool ResolveOnly
		{
			get
			{
				return ArgumentCount == 16777215;
			}
		}

		public int ArgumentCount
		{
			get
			{
				return (int)(_countAndFlags >> 8);
			}
		}

		internal RubyCallFlags Flags
		{
			get
			{
				return (RubyCallFlags)((int)_countAndFlags & 0xFF);
			}
		}

		public int TotalArgumentCount
		{
			get
			{
				return 1 + ArgumentCount + (HasSplattedArgument ? 1 : 0) + (HasBlock ? 1 : 0) + (HasRhsArgument ? 1 : 0);
			}
		}

		private RubyCallSignature(RubyCallFlags flags)
		{
			_countAndFlags = 0xFFFFFF00u | (uint)flags;
		}

		public RubyCallSignature(int argumentCount, RubyCallFlags flags)
		{
			_countAndFlags = (uint)(argumentCount << 8) | (uint)flags;
		}

		[Obsolete("Do not use from code")]
		[CLSCompliant(false)]
		public RubyCallSignature(uint countAndFlags)
		{
			_countAndFlags = countAndFlags;
		}

		internal static bool TryCreate(CallInfo callInfo, out RubyCallSignature callSignature)
		{
			callSignature = Interop(callInfo.ArgumentCount);
			return callInfo.ArgumentNames.Count != 0;
		}

		public static RubyCallSignature WithImplicitSelf(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, RubyCallFlags.HasImplicitSelf);
		}

		public static RubyCallSignature Simple(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, RubyCallFlags.None);
		}

		public static RubyCallSignature Interop(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, RubyCallFlags.IsInteropCall);
		}

		public static RubyCallSignature WithBlock(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, RubyCallFlags.HasBlock);
		}

		public static RubyCallSignature WithSplat(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, RubyCallFlags.HasSplattedArgument);
		}

		public static RubyCallSignature WithSplatAndBlock(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, (RubyCallFlags)10);
		}

		public static RubyCallSignature WithScope(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, RubyCallFlags.HasScope);
		}

		public static RubyCallSignature WithScopeAndBlock(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, (RubyCallFlags)9);
		}

		public static RubyCallSignature WithScopeAndSplat(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, (RubyCallFlags)3);
		}

		public static RubyCallSignature WithScopeAndSplatAndBlock(int argumentCount)
		{
			return new RubyCallSignature(argumentCount, (RubyCallFlags)11);
		}

		public static RubyCallSignature IsDefined(bool hasImplicitSelf)
		{
			return new RubyCallSignature(RubyCallFlags.HasScope | (hasImplicitSelf ? RubyCallFlags.HasImplicitSelf : RubyCallFlags.None));
		}

		internal Expression CreateExpression()
		{
			return Expression.New(Methods.RubyCallSignatureCtor, Microsoft.Scripting.Ast.Utils.Constant(_countAndFlags));
		}

		public bool Equals(RubyCallSignature other)
		{
			return _countAndFlags == other._countAndFlags;
		}

		public override string ToString()
		{
			return "(" + (HasImplicitSelf ? "." : "") + (HasImplicitArguments ? "^" : "") + (IsVirtualCall ? "V" : "") + (HasScope ? "S" : "C") + (ResolveOnly ? "?" : ("," + ArgumentCount)) + (HasSplattedArgument ? "*" : "") + (HasBlock ? "&" : "") + (HasRhsArgument ? "=" : "") + ")";
		}
	}
}
