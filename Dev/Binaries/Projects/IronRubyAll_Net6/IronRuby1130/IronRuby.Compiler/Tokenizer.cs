using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler
{
	public class Tokenizer : TokenizerService
	{
		internal struct NestedTokenSequence : IEquatable<NestedTokenSequence>
		{
			internal static readonly NestedTokenSequence[] EmptyArray = new NestedTokenSequence[0];

			public readonly TokenSequenceState State;

			public readonly int OpenedBracesInEmbeddedCode;

			public NestedTokenSequence(TokenSequenceState state, int openedBracesInEmbeddedCode)
			{
				State = state;
				OpenedBracesInEmbeddedCode = openedBracesInEmbeddedCode;
			}

			public override bool Equals(object other)
			{
				if (other is NestedTokenSequence)
				{
					return Equals((NestedTokenSequence)other);
				}
				return false;
			}

			public bool Equals(NestedTokenSequence other)
			{
				if (!object.ReferenceEquals(State, other.State))
				{
					if (State != null && State.Equals(other.State))
					{
						return OpenedBracesInEmbeddedCode == other.OpenedBracesInEmbeddedCode;
					}
					return false;
				}
				return true;
			}

			public override int GetHashCode()
			{
				return State.GetHashCode() ^ OpenedBracesInEmbeddedCode;
			}
		}

		internal sealed class State : IEquatable<State>
		{
			internal TokenSequenceState _currentSequence;

			internal NestedTokenSequence[] _nestedSequences;

			internal int _nestedSequenceCount;

			internal List<VerbatimHeredocState> VerbatimHeredocQueue;

			internal LexicalState LexicalState;

			internal int OpenedBracesInEmbeddedCode;

			internal byte _commandMode;

			internal byte _whitespaceSeen;

			internal byte _inStringEmbeddedVariable;

			internal int HeredocEndLine;

			internal int HeredocEndLineIndex;

			internal TokenSequenceState CurrentSequence
			{
				get
				{
					return _currentSequence;
				}
				set
				{
					_currentSequence = value;
				}
			}

			internal State(State src)
			{
				if (src != null)
				{
					LexicalState = src.LexicalState;
					OpenedBracesInEmbeddedCode = src.OpenedBracesInEmbeddedCode;
					_commandMode = src._commandMode;
					_whitespaceSeen = src._whitespaceSeen;
					_inStringEmbeddedVariable = src._inStringEmbeddedVariable;
					HeredocEndLine = src.HeredocEndLine;
					HeredocEndLineIndex = src.HeredocEndLineIndex;
					_currentSequence = src._currentSequence;
					_nestedSequenceCount = src._nestedSequenceCount;
					if (_nestedSequenceCount > 0)
					{
						_nestedSequences = new NestedTokenSequence[_nestedSequenceCount];
						Array.Copy(src._nestedSequences, 0, _nestedSequences, 0, _nestedSequenceCount);
					}
					else
					{
						_nestedSequences = NestedTokenSequence.EmptyArray;
					}
				}
				else
				{
					LexicalState = LexicalState.EXPR_BEG;
					_commandMode = 1;
					HeredocEndLineIndex = -1;
					_currentSequence = TokenSequenceState.None;
					_nestedSequences = NestedTokenSequence.EmptyArray;
				}
			}

			public override bool Equals(object other)
			{
				return Equals(other as State);
			}

			public bool Equals(State other)
			{
				if (!object.ReferenceEquals(this, other))
				{
					if (other != null && _currentSequence == other._currentSequence && Utils.ValueEquals(_nestedSequences, _nestedSequenceCount, other._nestedSequences, other._nestedSequenceCount) && LexicalState == other.LexicalState && OpenedBracesInEmbeddedCode == other.OpenedBracesInEmbeddedCode && _commandMode == other._commandMode && _whitespaceSeen == other._whitespaceSeen && _inStringEmbeddedVariable == other._inStringEmbeddedVariable && HeredocEndLine == other.HeredocEndLine)
					{
						return HeredocEndLineIndex == other.HeredocEndLineIndex;
					}
					return false;
				}
				return true;
			}

			public override int GetHashCode()
			{
				return _currentSequence.GetHashCode() ^ _nestedSequences.GetValueHashCode(0, _nestedSequenceCount) ^ _nestedSequenceCount ^ (int)LexicalState ^ OpenedBracesInEmbeddedCode ^ _commandMode ^ _whitespaceSeen ^ _inStringEmbeddedVariable ^ HeredocEndLine ^ HeredocEndLineIndex;
			}

			private void PushNestedSequence(NestedTokenSequence sequence)
			{
				if (_nestedSequences.Length == _nestedSequenceCount)
				{
					Array.Resize(ref _nestedSequences, _nestedSequences.Length * 2 + 1);
				}
				_nestedSequences[_nestedSequenceCount++] = sequence;
			}

			private NestedTokenSequence PopNestedSequence()
			{
				NestedTokenSequence result = _nestedSequences[_nestedSequenceCount - 1];
				_nestedSequences[_nestedSequenceCount - 1] = default(NestedTokenSequence);
				_nestedSequenceCount--;
				return result;
			}

			internal void StartStringEmbeddedCode()
			{
				PushNestedSequence(new NestedTokenSequence(_currentSequence, OpenedBracesInEmbeddedCode));
				OpenedBracesInEmbeddedCode = 0;
				_currentSequence = TokenSequenceState.None;
			}

			internal void EndStringEmbeddedCode()
			{
				NestedTokenSequence nestedTokenSequence = PopNestedSequence();
				_currentSequence = nestedTokenSequence.State;
				OpenedBracesInEmbeddedCode = nestedTokenSequence.OpenedBracesInEmbeddedCode;
			}

			internal void EnqueueVerbatimHeredoc(VerbatimHeredocState heredoc)
			{
				if (VerbatimHeredocQueue == null)
				{
					VerbatimHeredocQueue = new List<VerbatimHeredocState>();
				}
				VerbatimHeredocQueue.Add(heredoc);
			}

			internal void DequeueVerbatimHeredocs()
			{
				if (_currentSequence == TokenSequenceState.None)
				{
					PushNestedSequence(new NestedTokenSequence(new CodeState(LexicalState, _commandMode, _whitespaceSeen), OpenedBracesInEmbeddedCode));
				}
				else
				{
					PushNestedSequence(new NestedTokenSequence(_currentSequence, OpenedBracesInEmbeddedCode));
				}
				for (int num = VerbatimHeredocQueue.Count - 1; num > 0; num--)
				{
					PushNestedSequence(new NestedTokenSequence(VerbatimHeredocQueue[num], OpenedBracesInEmbeddedCode));
				}
				_currentSequence = VerbatimHeredocQueue[0];
				VerbatimHeredocQueue = null;
			}

			internal void FinishVerbatimHeredoc()
			{
				NestedTokenSequence nestedTokenSequence = PopNestedSequence();
				CodeState codeState = nestedTokenSequence.State as CodeState;
				if (codeState != null)
				{
					LexicalState = codeState.LexicalState;
					_commandMode = codeState.CommandMode;
					_whitespaceSeen = codeState.WhitespaceSeen;
					_currentSequence = TokenSequenceState.None;
				}
				else
				{
					_currentSequence = nestedTokenSequence.State;
				}
				OpenedBracesInEmbeddedCode = nestedTokenSequence.OpenedBracesInEmbeddedCode;
			}
		}

		public sealed class BignumParser : UnsignedBigIntegerParser
		{
			private char[] _buffer;

			private int _position;

			public int Position
			{
				get
				{
					return _position;
				}
				set
				{
					_position = value;
				}
			}

			public char[] Buffer
			{
				get
				{
					return _buffer;
				}
				set
				{
					_buffer = value;
				}
			}

			protected override int ReadDigit()
			{
				char c;
				do
				{
					c = _buffer[_position++];
					if (c <= '9')
					{
						return c - 48;
					}
					if (c >= 'a')
					{
						return c - 97 + 10;
					}
				}
				while (c == '_');
				return c - 65 + 10;
			}
		}

		private enum NumericCharKind
		{
			None,
			Digit,
			Underscore
		}

		private const int InitialBufferSize = 80;

		private const int AllowMultiByteIdentifier = 127;

		private const string EncodingHeaderPattern = "^[#].*?coding\\s*[:=]\\s*(?<encoding>[a-z0-9_-]+)";

		private readonly ILexicalVariableResolver _localVariableResolver;

		private BignumParser _bigIntParser;

		private RubyEncoding _encoding;

		private TextReader _input;

		private SourceLocation _initialLocation;

		private RubyCompatibility _compatibility;

		private bool _verbatim;

		private int _multiByteIdentifier = int.MaxValue;

		private SourceUnit _sourceUnit;

		private ErrorSink _errorSink;

		private State _state;

		internal int _openingCount;

		internal int _lambdaOpenings;

		private int _commandArgsStateStack;

		private int _loopConditionStateStack;

		private char[] _lineBuffer;

		private int _lineLength;

		private int _bufferPos;

		private int _currentLine;

		private int _currentLineIndex;

		private bool _unterminatedToken;

		private bool _eofReached;

		private int _dataOffset;

		private TokenValue _tokenValue;

		private SourceLocation _currentTokenStart;

		private SourceLocation _currentTokenEnd;

		private int _currentTokenStartIndex;

		private SourceSpan _tokenSpan;

		public bool ForceBinaryMultiByte { get; set; }

		public bool AllowNonAsciiIdentifiers
		{
			get
			{
				return _multiByteIdentifier < int.MaxValue;
			}
			set
			{
				_multiByteIdentifier = (value ? 127 : int.MaxValue);
			}
		}

		internal RubyEncoding Encoding
		{
			get
			{
				return _encoding;
			}
			set
			{
				_encoding = value;
			}
		}

		public bool Verbatim
		{
			get
			{
				return _verbatim;
			}
			set
			{
				_verbatim = value;
			}
		}

		private bool InStringEmbeddedCode
		{
			get
			{
				return _state._nestedSequenceCount > 0;
			}
		}

		private bool InStringEmbeddedVariable
		{
			get
			{
				return _state._inStringEmbeddedVariable == 1;
			}
			set
			{
				_state._inStringEmbeddedVariable = (byte)(value ? 1 : 0);
			}
		}

		private bool WhitespaceSeen
		{
			get
			{
				return _state._whitespaceSeen == 1;
			}
			set
			{
				_state._whitespaceSeen = (byte)(value ? 1 : 0);
			}
		}

		public bool CommandMode
		{
			get
			{
				return _state._commandMode == 1;
			}
			set
			{
				_state._commandMode = (byte)(value ? 1 : 0);
			}
		}

		internal LexicalState LexicalState
		{
			get
			{
				return _state.LexicalState;
			}
			set
			{
				_state.LexicalState = value;
			}
		}

		private bool IsEndLexicalState
		{
			get
			{
				if (_state.LexicalState != LexicalState.EXPR_END && _state.LexicalState != LexicalState.EXPR_ENDARG)
				{
					return _state.LexicalState == LexicalState.EXPR_ENDFN;
				}
				return true;
			}
		}

		private bool IsBeginLexicalState
		{
			get
			{
				if (_state.LexicalState != 0 && _state.LexicalState != LexicalState.EXPR_MID && _state.LexicalState != LexicalState.EXPR_CLASS)
				{
					return _state.LexicalState == LexicalState.EXPR_VALUE;
				}
				return true;
			}
		}

		private bool InArgs
		{
			get
			{
				if (_state.LexicalState != LexicalState.EXPR_ARG)
				{
					return _state.LexicalState == LexicalState.EXPR_CMDARG;
				}
				return true;
			}
		}

		private bool InCommandArgs
		{
			get
			{
				return BitStackPeek(_commandArgsStateStack);
			}
		}

		private bool InLoopCondition
		{
			get
			{
				return BitStackPeek(_loopConditionStateStack);
			}
		}

		private bool AtEndOfLine
		{
			get
			{
				if (_bufferPos == _lineLength && _bufferPos > 0)
				{
					return _lineBuffer[_bufferPos - 1] == '\n';
				}
				return false;
			}
		}

		public SourceUnit SourceUnit
		{
			get
			{
				return _sourceUnit;
			}
		}

		public int DataOffset
		{
			get
			{
				return _dataOffset;
			}
		}

		public RubyCompatibility Compatibility
		{
			get
			{
				return _compatibility;
			}
			set
			{
				_compatibility = value;
			}
		}

		public SourceSpan TokenSpan
		{
			get
			{
				return _tokenSpan;
			}
		}

		public TokenValue TokenValue
		{
			get
			{
				return _tokenValue;
			}
		}

		public bool EndOfFileReached
		{
			get
			{
				return _eofReached;
			}
		}

		public bool UnterminatedToken
		{
			get
			{
				return _unterminatedToken;
			}
		}

		public override object CurrentState
		{
			get
			{
				return _state;
			}
		}

		public override ErrorSink ErrorSink
		{
			get
			{
				return _errorSink;
			}
			set
			{
				ContractUtils.RequiresNotNull(value, "value");
				_errorSink = value;
			}
		}

		public override bool IsRestartable
		{
			get
			{
				return true;
			}
		}

		public override SourceLocation CurrentPosition
		{
			get
			{
				return _tokenSpan.End;
			}
		}

		public Tokenizer()
			: this(true)
		{
		}

		public Tokenizer(bool verbatim)
			: this(verbatim, DummyVariableResolver.AllMethodNames)
		{
		}

		public Tokenizer(bool verbatim, ILexicalVariableResolver localVariableResolver)
		{
			ContractUtils.RequiresNotNull(localVariableResolver, "localVariableResolver");
			_errorSink = ErrorSink.Null;
			_localVariableResolver = localVariableResolver;
			_verbatim = verbatim;
			_encoding = RubyEncoding.Binary;
		}

		public void Initialize(SourceUnit sourceUnit)
		{
			ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");
			Initialize(null, sourceUnit.GetReader(), sourceUnit, SourceLocation.MinValue);
		}

		public void Initialize(TextReader reader)
		{
			Initialize(null, reader, null, SourceLocation.MinValue);
		}

		public override void Initialize(object state, TextReader reader, SourceUnit sourceUnit, SourceLocation initialLocation)
		{
			ContractUtils.RequiresNotNull(reader, "reader");
			_sourceUnit = sourceUnit;
			_initialLocation = initialLocation;
			_currentLine = _initialLocation.Line;
			_currentLineIndex = _initialLocation.Index;
			_tokenSpan = new SourceSpan(initialLocation, initialLocation);
			_state = new State(state as State);
			_commandArgsStateStack = 0;
			_loopConditionStateStack = 0;
			_openingCount = 0;
			_input = reader;
			_lineBuffer = null;
			_lineLength = 0;
			_bufferPos = 0;
			_currentTokenStart = SourceLocation.Invalid;
			_currentTokenEnd = SourceLocation.Invalid;
			_currentTokenStartIndex = -1;
			_tokenValue = default(TokenValue);
			_eofReached = false;
			_unterminatedToken = false;
			_dataOffset = -1;
		}

		[Conditional("DEBUG")]
		public void EnableLogging(int verbosity, TextWriter output)
		{
		}

		[Conditional("DEBUG")]
		public void DisableLogging()
		{
		}

		[Conditional("DEBUG")]
		private void Log(string format, params object[] args)
		{
		}

		[Conditional("DEBUG")]
		private void DumpBeginningOfUnit()
		{
		}

		[Conditional("DEBUG")]
		private void DumpToken(Tokens token)
		{
		}

		private bool InArgsNoSpace(int c)
		{
			if (InArgs && WhitespaceSeen)
			{
				return !IsWhiteSpace(c);
			}
			return false;
		}

		private void BitStackPush(ref int stack, int n)
		{
			stack = (stack << 1) | (n & 1);
		}

		private int BitStackPop(ref int stack)
		{
			return stack >>= 1;
		}

		private void BitStackOrPop(ref int stack)
		{
			stack = (stack >> 1) | (stack & 1);
		}

		private bool BitStackPeek(int stack)
		{
			return (stack & 1) != 0;
		}

		internal int EnterCommandArguments()
		{
			int commandArgsStateStack = _commandArgsStateStack;
			BitStackPush(ref _commandArgsStateStack, 1);
			return commandArgsStateStack;
		}

		internal void LeaveCommandArguments(int state)
		{
			_commandArgsStateStack = state;
		}

		internal void EnterLoopCondition()
		{
			BitStackPush(ref _loopConditionStateStack, 1);
		}

		internal void LeaveLoopCondition()
		{
			BitStackPop(ref _loopConditionStateStack);
		}

		private void EnterParenthesisedExpression()
		{
			_openingCount++;
			BitStackPush(ref _loopConditionStateStack, 0);
			BitStackPush(ref _commandArgsStateStack, 0);
		}

		internal void LeaveParenthesisedExpression()
		{
			_openingCount--;
			PopParenthesisedExpressionStack();
		}

		internal void PopParenthesisedExpressionStack()
		{
			BitStackOrPop(ref _commandArgsStateStack);
			BitStackPop(ref _loopConditionStateStack);
		}

		private Tokens StringEmbeddedVariableBegin()
		{
			InStringEmbeddedVariable = true;
			LexicalState = LexicalState.EXPR_BEG;
			return Tokens.StringEmbeddedVariableBegin;
		}

		private Tokens StringEmbeddedCodeBegin()
		{
			_state.StartStringEmbeddedCode();
			LexicalState = LexicalState.EXPR_BEG;
			EnterParenthesisedExpression();
			return Tokens.StringEmbeddedCodeBegin;
		}

		internal int EnterLambdaDefinition()
		{
			int lambdaOpenings = _lambdaOpenings;
			_lambdaOpenings = ++_openingCount;
			return lambdaOpenings;
		}

		internal void LeaveLambdaDefinition(int oldLambdaOpenings)
		{
			_lambdaOpenings = oldLambdaOpenings;
		}

		private void Report(string message, int errorCode, SourceSpan location, Severity severity)
		{
			_errorSink.Add(_sourceUnit, message, location, errorCode, severity);
		}

		internal void ReportError(ErrorInfo info)
		{
			Report(info.GetMessage(), info.Code, GetCurrentSpan(), Severity.Error);
		}

		internal void ReportError(ErrorInfo info, params object[] args)
		{
			Report(info.GetMessage(args), info.Code, GetCurrentSpan(), Severity.Error);
		}

		internal void ReportError(ErrorInfo info, SourceSpan location, params object[] args)
		{
			Report(info.GetMessage(args), info.Code, location, Severity.Error);
		}

		internal void ReportWarning(ErrorInfo info)
		{
			Report(info.GetMessage(), info.Code, GetCurrentSpan(), Severity.Warning);
		}

		internal void ReportWarning(ErrorInfo info, params object[] args)
		{
			Report(info.GetMessage(args), info.Code, GetCurrentSpan(), Severity.Warning);
		}

		private void WarnBalanced(string p, string p_2)
		{
		}

		private bool LoadLine()
		{
			int num = 0;
			if (_lineBuffer == null)
			{
				_lineBuffer = new char[80];
			}
			while (true)
			{
				int num2;
				try
				{
					num2 = _input.Read();
				}
				catch (DecoderFallbackException ex)
				{
					ReportError(Errors.InvalidMultibyteCharacter, BitConverter.ToString(ex.BytesUnknown).Replace('-', ' '), _encoding.Name);
					num2 = -1;
				}
				if (num2 == -1)
				{
					if (num > 0)
					{
						if (num < _lineBuffer.Length)
						{
							_lineBuffer[num] = '\0';
						}
						break;
					}
					return false;
				}
				if (num == _lineBuffer.Length)
				{
					Array.Resize(ref _lineBuffer, num * 2);
				}
				_lineBuffer[num++] = (char)num2;
				switch (num2)
				{
				default:
					continue;
				case 13:
					if (_input.Peek() == 10)
					{
						continue;
					}
					break;
				case 10:
					break;
				}
				break;
			}
			_lineLength = num;
			_bufferPos = 0;
			return true;
		}

		private int Read()
		{
			if (!RefillBuffer())
			{
				return -1;
			}
			return _lineBuffer[_bufferPos++];
		}

		private bool Read(int c)
		{
			if (Peek() == c)
			{
				Skip();
				return true;
			}
			return false;
		}

		private void Skip(int c)
		{
			_bufferPos++;
		}

		private void Skip()
		{
			_bufferPos++;
		}

		private void SeekRelative(int disp)
		{
			_bufferPos += disp;
		}

		private void Back(int c)
		{
			if (c != -1)
			{
				_bufferPos--;
			}
		}

		private int Peek()
		{
			return Peek(0);
		}

		private int Peek(int disp)
		{
			if (_lineBuffer == null && !RefillBuffer())
			{
				return -1;
			}
			if (_bufferPos + disp < _lineLength)
			{
				return _lineBuffer[_bufferPos + disp];
			}
			return -1;
		}

		private bool RefillBuffer()
		{
			if (_lineBuffer == null || _bufferPos == _lineLength)
			{
				bool flag = _lineBuffer == null;
				int lineLength = _lineLength;
				if (!LoadLine())
				{
					return false;
				}
				if (_state.HeredocEndLine > 0)
				{
					_currentLine = _state.HeredocEndLine;
					_currentLineIndex = _state.HeredocEndLineIndex;
					_state.HeredocEndLine = 0;
					_state.HeredocEndLineIndex = -1;
				}
				else if (flag)
				{
					_currentLine = _initialLocation.Line;
					_currentLineIndex = _initialLocation.Index;
				}
				else
				{
					_currentLine++;
					_currentLineIndex += lineLength;
				}
			}
			return true;
		}

		private bool is_bol()
		{
			return _bufferPos == 0;
		}

		private bool was_bol()
		{
			return _bufferPos == 1;
		}

		private bool LineContentEquals(string str, bool skipWhitespace)
		{
			int strStart;
			return LineContentEquals(str, skipWhitespace, out strStart);
		}

		private bool LineContentEquals(string str, bool skipWhitespace, out int strStart)
		{
			int i = 0;
			if (skipWhitespace)
			{
				for (; i < _lineLength && IsWhiteSpace(_lineBuffer[i]); i++)
				{
				}
			}
			strStart = i;
			int num = _lineLength - (i + str.Length);
			if (num < 0 || (num > 0 && _lineBuffer[i + str.Length] != '\n' && _lineBuffer[i + str.Length] != '\r'))
			{
				return false;
			}
			return StringEquals(str, _lineBuffer, i, _lineLength);
		}

		private static bool StringEquals(string str, char[] chars, int offset, int count)
		{
			if (str.Length > count - offset)
			{
				return false;
			}
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != chars[offset + i])
				{
					return false;
				}
			}
			return true;
		}

		internal void CaptureTokenSpan()
		{
			_tokenSpan = new SourceSpan(_currentTokenStart, _currentTokenEnd);
		}

		private void MarkTokenEnd(bool isMultiLine)
		{
			if (isMultiLine)
			{
				MarkMultiLineTokenEnd();
			}
			else
			{
				MarkSingleLineTokenEnd();
			}
		}

		private void MarkSingleLineTokenEnd()
		{
			_currentTokenEnd = GetCurrentLocation();
		}

		private void MarkMultiLineTokenEnd()
		{
			_currentTokenEnd = GetCurrentLocation();
		}

		private Tokens MarkSingleLineTokenEnd(Tokens token)
		{
			MarkSingleLineTokenEnd();
			return token;
		}

		private Tokens MarkMultiLineTokenEnd(Tokens token)
		{
			MarkMultiLineTokenEnd();
			return token;
		}

		internal void MarkTokenStart()
		{
			_currentTokenStart = GetCurrentLocation();
			_currentTokenStartIndex = _bufferPos;
		}

		private SourceLocation GetCurrentLocation()
		{
			if (_lineBuffer == null)
			{
				return _initialLocation;
			}
			if (AtEndOfLine)
			{
				return new SourceLocation(_currentLineIndex + _bufferPos, _currentLine + 1, 1);
			}
			return new SourceLocation(_currentLineIndex + _bufferPos, _currentLine, ((_currentLine != _initialLocation.Line) ? 1 : _initialLocation.Column) + _bufferPos);
		}

		private SourceSpan GetCurrentSpan()
		{
			SourceLocation currentLocation = GetCurrentLocation();
			return new SourceSpan(currentLocation, currentLocation);
		}

		public Tokens GetNextToken()
		{
			if (_input == null)
			{
				throw new InvalidOperationException("Uninitialized");
			}
			Tokens tokens;
			do
			{
				RefillBuffer();
				tokens = _state.CurrentSequence.TokenizeAndMark(this);
				if (tokens == Tokens.VerbatimHeredocEnd)
				{
					return tokens;
				}
				WhitespaceSeen = tokens == Tokens.Whitespace;
				switch (tokens)
				{
				case Tokens.SingleLineComment:
				case Tokens.MultiLineComment:
				case Tokens.Whitespace:
				case Tokens.EndOfLine:
					continue;
				case Tokens.EndOfFile:
					_eofReached = true;
					CommandMode = false;
					break;
				case Tokens.NewLine:
				case Tokens.Do:
				case Tokens.Semicolon:
				case Tokens.LeftBlockBrace:
				case Tokens.LeftBlockArgBrace:
					CommandMode = true;
					break;
				default:
					CommandMode = false;
					break;
				}
				break;
			}
			while (!_verbatim);
			if (_verbatim && _state.VerbatimHeredocQueue != null && AtEndOfLine)
			{
				_state.DequeueVerbatimHeredocs();
			}
			return tokens;
		}

		internal Tokens Tokenize()
		{
			MarkTokenStart();
			int num = Read();
			switch (num)
			{
			case 0:
				Back(0);
				MarkSingleLineTokenEnd();
				return Tokens.EndOfFile;
			case -1:
				MarkSingleLineTokenEnd();
				return Tokens.EndOfFile;
			case 9:
			case 12:
			case 32:
				return MarkSingleLineTokenEnd(ReadNonEolnWhiteSpace());
			case 10:
				return MarkMultiLineTokenEnd(GetEndOfLineToken());
			case 13:
				if (Read(10))
				{
					return MarkMultiLineTokenEnd(GetEndOfLineToken());
				}
				return MarkSingleLineTokenEnd(ReadNonEolnWhiteSpace());
			case 92:
				return TokenizeBackslash();
			case 35:
				return MarkSingleLineTokenEnd(ReadSingleLineComment());
			case 42:
				return MarkSingleLineTokenEnd(ReadStar());
			case 33:
				return MarkSingleLineTokenEnd(ReadBang());
			case 61:
				if (was_bol() && PeekMultiLineCommentBegin())
				{
					return TokenizeMultiLineComment(true);
				}
				return MarkSingleLineTokenEnd(ReadEquals());
			case 60:
				return TokenizeLessThan();
			case 62:
				return MarkSingleLineTokenEnd(ReadGreaterThan());
			case 34:
				return MarkSingleLineTokenEnd(ReadDoubleQuote());
			case 39:
				return MarkSingleLineTokenEnd(ReadSingleQuote());
			case 96:
				return MarkSingleLineTokenEnd(ReadBacktick());
			case 63:
				return TokenizeQuestionmark();
			case 38:
				return MarkSingleLineTokenEnd(ReadAmpersand());
			case 124:
				return MarkSingleLineTokenEnd(ReadPipe());
			case 43:
				return MarkSingleLineTokenEnd(ReadPlus());
			case 45:
				return MarkSingleLineTokenEnd(ReadMinus());
			case 46:
				return MarkSingleLineTokenEnd(ReadDot());
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
				return MarkSingleLineTokenEnd(ReadUnsignedNumber(num));
			case 58:
				return MarkSingleLineTokenEnd(ReadColon());
			case 47:
				return MarkSingleLineTokenEnd(ReadSlash());
			case 94:
				return MarkSingleLineTokenEnd(ReadCaret());
			case 59:
				LexicalState = LexicalState.EXPR_BEG;
				MarkSingleLineTokenEnd();
				return Tokens.Semicolon;
			case 44:
				LexicalState = LexicalState.EXPR_BEG;
				MarkSingleLineTokenEnd();
				return Tokens.Comma;
			case 126:
				return MarkSingleLineTokenEnd(ReadTilde());
			case 40:
				return MarkSingleLineTokenEnd(ReadLeftParenthesis());
			case 91:
				return MarkSingleLineTokenEnd(ReadLeftBracket());
			case 123:
				return MarkSingleLineTokenEnd(ReadLeftBrace());
			case 41:
				return TokenizeClosing(Tokens.RightParenthesis);
			case 93:
				return TokenizeClosing(Tokens.RightBracket);
			case 125:
				return TokenizeClosingBrace();
			case 37:
				return TokenizePercent();
			case 36:
				return MarkSingleLineTokenEnd(ReadGlobalVariable());
			case 64:
				return MarkSingleLineTokenEnd(ReadInstanceOrClassVariable());
			case 95:
				if (was_bol() && LineContentEquals("__END__", false))
				{
					Back(95);
					MarkSingleLineTokenEnd();
					_dataOffset = _currentLineIndex + _lineLength;
					return Tokens.EndOfFile;
				}
				return MarkSingleLineTokenEnd(ReadIdentifier(num));
			default:
				if (!IsIdentifierInitial(num, _multiByteIdentifier))
				{
					ReportError(Errors.InvalidCharacterInExpression, (char)num);
					MarkSingleLineTokenEnd();
					return Tokens.InvalidCharacter;
				}
				return MarkSingleLineTokenEnd(ReadIdentifier(num));
			}
		}

		private Tokens ReadNonEolnWhiteSpace()
		{
			while (true)
			{
				int num = Peek();
				switch (num)
				{
				case 9:
				case 12:
				case 32:
					Skip(num);
					continue;
				case 13:
					if (Peek(1) != 10)
					{
						Skip(num);
						continue;
					}
					break;
				}
				break;
			}
			return Tokens.Whitespace;
		}

		private Tokens GetEndOfLineToken()
		{
			if (LexicalState == LexicalState.EXPR_BEG || LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT || LexicalState == LexicalState.EXPR_CLASS || LexicalState == LexicalState.EXPR_VALUE)
			{
				return Tokens.EndOfLine;
			}
			if (!_verbatim || _state.VerbatimHeredocQueue == null)
			{
				RefillBuffer();
				int bufferPos = _bufferPos;
				int num;
				do
				{
					num = Read();
					if (num == 46)
					{
						if (Peek() == 46)
						{
							break;
						}
						_bufferPos = bufferPos;
						return Tokens.EndOfLine;
					}
				}
				while (IsWhiteSpace(num));
				_bufferPos = bufferPos;
			}
			LexicalState = LexicalState.EXPR_BEG;
			return Tokens.NewLine;
		}

		private Tokens TokenizeBackslash()
		{
			if (TryReadEndOfLine())
			{
				MarkMultiLineTokenEnd();
				return Tokens.Whitespace;
			}
			MarkSingleLineTokenEnd();
			return Tokens.Backslash;
		}

		private Tokens ReadSingleLineComment()
		{
			while (true)
			{
				int num = Peek();
				if (num == -1 || num == 10)
				{
					break;
				}
				Skip(num);
			}
			return Tokens.SingleLineComment;
		}

		private bool TryReadEndOfLine()
		{
			int num = Peek();
			switch (num)
			{
			case 10:
				Skip(num);
				return true;
			case 13:
				if (Peek(1) == 10)
				{
					SeekRelative(2);
					return true;
				}
				break;
			}
			return false;
		}

		private int ReadNormalizeEndOfLine()
		{
			int num = Read();
			if (num == 13 && Peek() == 10)
			{
				Skip(10);
				return 10;
			}
			return num;
		}

		private int ReadNormalizeEndOfLine(out int eolnWidth)
		{
			int num = Read();
			if (num == 13 && Peek() == 10)
			{
				Skip(10);
				eolnWidth = 2;
				return 10;
			}
			eolnWidth = 1;
			return num;
		}

		private Tokens ReadIdentifier(int firstCharacter)
		{
			int num = _bufferPos - 1;
			SkipVariableName();
			Tokens result = ReadIdentifierSuffix(firstCharacter);
			string text = new string(_lineBuffer, num, _bufferPos - num);
			if ((InArgs || (LexicalState == LexicalState.EXPR_BEG && !CommandMode)) && Peek(0) == 58 && Peek(1) != 58)
			{
				Skip(58);
				LexicalState = LexicalState.EXPR_BEG;
				SetStringToken(text);
				return Tokens.Label;
			}
			if (LexicalState != LexicalState.EXPR_DOT)
			{
				if (LexicalState == LexicalState.EXPR_FNAME)
				{
					SetStringToken(text);
				}
				Tokens tokens = StringToKeyword(text);
				if (tokens != Tokens.None)
				{
					return tokens;
				}
			}
			if (IsBeginLexicalState || InArgs || LexicalState == LexicalState.EXPR_DOT)
			{
				if (LexicalState != LexicalState.EXPR_DOT && _localVariableResolver.IsLocalVariable(text))
				{
					LexicalState = LexicalState.EXPR_END;
				}
				else if (CommandMode)
				{
					LexicalState = LexicalState.EXPR_CMDARG;
				}
				else
				{
					LexicalState = LexicalState.EXPR_ARG;
				}
			}
			else if (LexicalState == LexicalState.EXPR_FNAME)
			{
				LexicalState = LexicalState.EXPR_ENDFN;
			}
			else
			{
				LexicalState = LexicalState.EXPR_END;
			}
			SetStringToken(text);
			return result;
		}

		private Tokens ReadIdentifierSuffix(int firstCharacter)
		{
			int num = Peek(0);
			int num2 = Peek(1);
			if ((num == 33 || num == 63) && num2 != 61)
			{
				Skip(num);
				return Tokens.FunctionIdentifier;
			}
			if (LexicalState == LexicalState.EXPR_FNAME && num == 61 && num2 != 126 && num2 != 62 && (num2 != 61 || Peek(2) == 62))
			{
				Skip(num);
				return Tokens.Identifier;
			}
			if (!IsUpperLetter(firstCharacter))
			{
				return Tokens.Identifier;
			}
			return Tokens.ConstantIdentifier;
		}

		private Tokens StringToKeyword(string identifier)
		{
			switch (identifier)
			{
			case "if":
				return ReturnKeyword(Tokens.If, Tokens.IfMod, LexicalState.EXPR_BEG);
			case "in":
				return ReturnKeyword(Tokens.In, LexicalState.EXPR_BEG);
			case "do":
				return ReturnDoKeyword();
			case "or":
				return ReturnKeyword(Tokens.Or, LexicalState.EXPR_BEG);
			case "and":
				return ReturnKeyword(Tokens.And, LexicalState.EXPR_BEG);
			case "end":
				return ReturnKeyword(Tokens.End, LexicalState.EXPR_END);
			case "def":
				return ReturnKeyword(Tokens.Def, LexicalState.EXPR_FNAME);
			case "for":
				return ReturnKeyword(Tokens.For, LexicalState.EXPR_BEG);
			case "not":
				return ReturnKeyword(Tokens.Not, LexicalState.EXPR_ARG);
			case "nil":
				return ReturnKeyword(Tokens.Nil, LexicalState.EXPR_END);
			case "END":
				return ReturnKeyword(Tokens.UppercaseEnd, LexicalState.EXPR_END);
			case "else":
				return ReturnKeyword(Tokens.Else, LexicalState.EXPR_BEG);
			case "then":
				return ReturnKeyword(Tokens.Then, LexicalState.EXPR_BEG);
			case "case":
				return ReturnKeyword(Tokens.Case, LexicalState.EXPR_BEG);
			case "self":
				return ReturnKeyword(Tokens.Self, LexicalState.EXPR_END);
			case "true":
				return ReturnKeyword(Tokens.True, LexicalState.EXPR_END);
			case "next":
				return ReturnKeyword(Tokens.Next, LexicalState.EXPR_MID);
			case "when":
				return ReturnKeyword(Tokens.When, LexicalState.EXPR_BEG);
			case "redo":
				return ReturnKeyword(Tokens.Redo, LexicalState.EXPR_END);
			case "alias":
				return ReturnKeyword(Tokens.Alias, LexicalState.EXPR_FNAME);
			case "begin":
				return ReturnKeyword(Tokens.Begin, LexicalState.EXPR_BEG);
			case "break":
				return ReturnKeyword(Tokens.Break, LexicalState.EXPR_MID);
			case "BEGIN":
				return ReturnKeyword(Tokens.UppercaseBegin, LexicalState.EXPR_END);
			case "class":
				return ReturnKeyword(Tokens.Class, LexicalState.EXPR_CLASS);
			case "elsif":
				return ReturnKeyword(Tokens.Elsif, LexicalState.EXPR_BEG);
			case "false":
				return ReturnKeyword(Tokens.False, LexicalState.EXPR_END);
			case "retry":
				return ReturnKeyword(Tokens.Retry, LexicalState.EXPR_END);
			case "super":
				return ReturnKeyword(Tokens.Super, LexicalState.EXPR_ARG);
			case "until":
				return ReturnKeyword(Tokens.Until, Tokens.UntilMod, LexicalState.EXPR_BEG);
			case "undef":
				return ReturnKeyword(Tokens.Undef, LexicalState.EXPR_FNAME);
			case "while":
				return ReturnKeyword(Tokens.While, Tokens.WhileMod, LexicalState.EXPR_BEG);
			case "yield":
				return ReturnKeyword(Tokens.Yield, LexicalState.EXPR_ARG);
			case "ensure":
				return ReturnKeyword(Tokens.Ensure, LexicalState.EXPR_BEG);
			case "module":
				return ReturnKeyword(Tokens.Module, LexicalState.EXPR_BEG);
			case "rescue":
				return ReturnKeyword(Tokens.Rescue, Tokens.RescueMod, LexicalState.EXPR_MID);
			case "return":
				return ReturnKeyword(Tokens.Return, LexicalState.EXPR_MID);
			case "unless":
				return ReturnKeyword(Tokens.Unless, Tokens.UnlessMod, LexicalState.EXPR_BEG);
			case "defined?":
				return ReturnKeyword(Tokens.Defined, LexicalState.EXPR_ARG);
			case "__LINE__":
				return ReturnKeyword(Tokens.Line, LexicalState.EXPR_END);
			case "__FILE__":
				return ReturnKeyword(Tokens.File, LexicalState.EXPR_END);
			case "__ENCODING__":
				return ReturnKeyword(Tokens.Encoding, LexicalState.EXPR_END);
			default:
				return Tokens.None;
			}
		}

		private Tokens ReturnKeyword(Tokens keyword, LexicalState state)
		{
			LexicalState = state;
			return keyword;
		}

		private Tokens ReturnKeyword(Tokens keywordInExpression, Tokens keywordModifier, LexicalState state)
		{
			if (LexicalState == LexicalState.EXPR_BEG || LexicalState == LexicalState.EXPR_VALUE)
			{
				LexicalState = state;
				return keywordInExpression;
			}
			LexicalState = LexicalState.EXPR_BEG;
			return keywordModifier;
		}

		private Tokens ReturnDoKeyword()
		{
			if (_lambdaOpenings > 0 && _lambdaOpenings == _openingCount)
			{
				_lambdaOpenings = 0;
				_openingCount--;
				return Tokens.LambdaDo;
			}
			LexicalState lexicalState = LexicalState;
			LexicalState = LexicalState.EXPR_BEG;
			if (InLoopCondition)
			{
				return Tokens.LoopDo;
			}
			if ((InCommandArgs && lexicalState != LexicalState.EXPR_CMDARG) || lexicalState == LexicalState.EXPR_ENDARG || lexicalState == LexicalState.EXPR_BEG)
			{
				return Tokens.BlockDo;
			}
			return Tokens.Do;
		}

		internal Tokens TokenizeMultiLineComment(bool started)
		{
			while (true)
			{
				if (started)
				{
					_bufferPos = _lineLength;
				}
				else
				{
					started = true;
				}
				switch (Read())
				{
				default:
					continue;
				case -1:
					_unterminatedToken = true;
					if (_verbatim)
					{
						_state.CurrentSequence = MultiLineCommentState.Instance;
						MarkMultiLineTokenEnd();
						if (_currentTokenStart.Index != _currentTokenEnd.Index)
						{
							return Tokens.MultiLineComment;
						}
						return Tokens.EndOfFile;
					}
					ReportError(Errors.UnterminatedEmbeddedDocument);
					break;
				case 61:
					if (!PeekMultiLineCommentEnd())
					{
						continue;
					}
					break;
				}
				break;
			}
			_state.CurrentSequence = TokenSequenceState.None;
			_bufferPos = _lineLength;
			MarkMultiLineTokenEnd();
			return Tokens.MultiLineComment;
		}

		private bool PeekMultiLineCommentBegin()
		{
			int num = _bufferPos + 5;
			if (num <= _lineLength && _lineBuffer[_bufferPos] == 'b' && _lineBuffer[_bufferPos + 1] == 'e' && _lineBuffer[_bufferPos + 2] == 'g' && _lineBuffer[_bufferPos + 3] == 'i' && _lineBuffer[_bufferPos + 4] == 'n')
			{
				if (num != _lineLength)
				{
					return IsWhiteSpace(_lineBuffer[num]);
				}
				return true;
			}
			return false;
		}

		private bool PeekMultiLineCommentEnd()
		{
			int num = _bufferPos + 3;
			if (num <= _lineLength && _lineBuffer[_bufferPos] == 'e' && _lineBuffer[_bufferPos + 1] == 'n' && _lineBuffer[_bufferPos + 2] == 'd')
			{
				if (num != _lineLength)
				{
					return IsWhiteSpace(_lineBuffer[num]);
				}
				return true;
			}
			return false;
		}

		private Tokens ReadEquals()
		{
			switch (LexicalState)
			{
			case LexicalState.EXPR_FNAME:
			case LexicalState.EXPR_DOT:
				LexicalState = LexicalState.EXPR_ARG;
				break;
			default:
				LexicalState = LexicalState.EXPR_BEG;
				break;
			}
			switch (Peek())
			{
			case 61:
				Skip(61);
				if (!Read(61))
				{
					return Tokens.Equal;
				}
				return Tokens.StrictEqual;
			case 126:
				Skip(126);
				return Tokens.Match;
			case 62:
				Skip(62);
				return Tokens.DoubleArrow;
			default:
				return Tokens.Assignment;
			}
		}

		private Tokens ReadPlus()
		{
			int num = Peek();
			if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT)
			{
				LexicalState = LexicalState.EXPR_ARG;
				if (num == 64)
				{
					Skip(64);
					return Tokens.UnaryPlus;
				}
				return Tokens.Plus;
			}
			if (num == 61)
			{
				Skip(61);
				SetAsciiStringToken(Symbols.Plus);
				LexicalState = LexicalState.EXPR_BEG;
				return Tokens.OpAssignment;
			}
			bool flag = false;
			if (IsBeginLexicalState || (flag = InArgsNoSpace(num)))
			{
				if (flag)
				{
					ReportWarning(Errors.AmbiguousFirstArgument);
				}
				LexicalState = LexicalState.EXPR_BEG;
				if (IsDecimalDigit(num))
				{
					Skip(num);
					return ReadUnsignedNumber(num);
				}
				return Tokens.UnaryPlus;
			}
			WarnBalanced("+", "unary operator");
			LexicalState = LexicalState.EXPR_BEG;
			return Tokens.Plus;
		}

		private Tokens ReadLeftParenthesis()
		{
			Tokens result = Tokens.LeftParenthesis;
			if (IsBeginLexicalState)
			{
				result = Tokens.LeftExprParenthesis;
			}
			else if (InArgs && WhitespaceSeen)
			{
				result = Tokens.LeftArgParenthesis;
			}
			EnterParenthesisedExpression();
			LexicalState = LexicalState.EXPR_BEG;
			return result;
		}

		private Tokens ReadInstanceOrClassVariable()
		{
			int num = _bufferPos - 1;
			int num2 = Peek(0);
			Tokens tokens;
			if (num2 == 64)
			{
				num2 = Peek(1);
				tokens = Tokens.ClassVariable;
			}
			else
			{
				tokens = Tokens.InstanceVariable;
			}
			if (IsDecimalDigit(num2))
			{
				ReportError((tokens == Tokens.InstanceVariable) ? Errors.InvalidInstanceVariableName : Errors.InvalidClassVariableName, (char)num2);
			}
			else if (IsIdentifierInitial(num2))
			{
				if (tokens == Tokens.ClassVariable)
				{
					Skip(64);
				}
				Skip(num2);
				SkipVariableName();
				SetStringToken(num, _bufferPos - num);
				LexicalState = LexicalState.EXPR_END;
				return tokens;
			}
			return Tokens.At;
		}

		private Tokens ReadGlobalVariable()
		{
			LexicalState = LexicalState.EXPR_END;
			int bufferPos = _bufferPos;
			int num = Read();
			switch (num)
			{
			case 95:
				if (IsIdentifier(Peek()))
				{
					SkipVariableName();
					SetStringToken(bufferPos, _bufferPos - bufferPos);
					return Tokens.GlobalVariable;
				}
				return GlobalVariableToken(Symbols.LastInputLine);
			case 33:
				return GlobalVariableToken(Symbols.CurrentException);
			case 64:
				return GlobalVariableToken(Symbols.CurrentExceptionBacktrace);
			case 45:
				if (IsIdentifier(Peek()))
				{
					Read();
					SetStringToken(bufferPos, 2);
				}
				else
				{
					SetAsciiStringToken("-");
				}
				return Tokens.GlobalVariable;
			case 44:
				return GlobalVariableToken(Symbols.ItemSeparator);
			case 59:
				return GlobalVariableToken(Symbols.StringSeparator);
			case 47:
				return GlobalVariableToken(Symbols.InputSeparator);
			case 92:
				return GlobalVariableToken(Symbols.OutputSeparator);
			case 42:
				return GlobalVariableToken(Symbols.CommandLineArguments);
			case 36:
				return GlobalVariableToken(Symbols.CurrentProcessId);
			case 63:
				return GlobalVariableToken(Symbols.ChildProcessExitStatus);
			case 61:
				return GlobalVariableToken(Symbols.IgnoreCaseComparator);
			case 58:
				return GlobalVariableToken(Symbols.LoadPath);
			case 34:
				return GlobalVariableToken(Symbols.LoadedFiles);
			case 60:
				return GlobalVariableToken(Symbols.InputContent);
			case 62:
				return GlobalVariableToken(Symbols.OutputStream);
			case 46:
				return GlobalVariableToken(Symbols.LastInputLineNumber);
			case 126:
				return GlobalVariableToken(Symbols.MatchData);
			case 38:
				_tokenValue.SetInteger(0);
				return Tokens.MatchReference;
			case 96:
				_tokenValue.SetInteger(-3);
				return Tokens.MatchReference;
			case 39:
				_tokenValue.SetInteger(-4);
				return Tokens.MatchReference;
			case 43:
				_tokenValue.SetInteger(-2);
				return Tokens.MatchReference;
			case 48:
				if (IsIdentifier(Peek()))
				{
					SkipVariableName();
					ReportError(Errors.InvalidGlobalVariableName, new string(_lineBuffer, bufferPos - 1, _bufferPos - bufferPos));
					SetAsciiStringToken(Symbols.ErrorVariable);
					return Tokens.GlobalVariable;
				}
				return GlobalVariableToken(Symbols.CommandLineProgramPath);
			default:
				if (IsDecimalDigit(num))
				{
					return ReadMatchGroupReferenceVariable(num);
				}
				if (IsIdentifier(num))
				{
					SkipVariableName();
					SetStringToken(bufferPos, _bufferPos - bufferPos);
					return Tokens.GlobalVariable;
				}
				Back(num);
				return Tokens.Dollar;
			}
		}

		private Tokens ReadMatchGroupReferenceVariable(int c)
		{
			int num = _bufferPos - 1;
			int num2 = c - 48;
			bool flag = false;
			while (true)
			{
				c = Peek();
				if (!IsDecimalDigit(c))
				{
					break;
				}
				Skip(c);
				num2 = num2 * 10 + (c - 48);
				flag = flag || num2 < 0;
			}
			if (flag)
			{
				ReportError(Errors.MatchGroupReferenceOverflow, new string(_lineBuffer, num, _bufferPos - num));
			}
			_tokenValue.SetInteger(num2);
			return Tokens.MatchReference;
		}

		private Tokens GlobalVariableToken(string symbol)
		{
			SetAsciiStringToken(symbol);
			return Tokens.GlobalVariable;
		}

		private Tokens TokenizePercent()
		{
			if (IsBeginLexicalState)
			{
				return TokenizeQuotationStart();
			}
			int num = Peek();
			if (num == 61)
			{
				Skip(num);
				SetAsciiStringToken(Symbols.Mod);
				LexicalState = LexicalState.EXPR_BEG;
				MarkSingleLineTokenEnd();
				return Tokens.OpAssignment;
			}
			if (InArgsNoSpace(num))
			{
				return TokenizeQuotationStart();
			}
			switch (LexicalState)
			{
			case LexicalState.EXPR_FNAME:
			case LexicalState.EXPR_DOT:
				LexicalState = LexicalState.EXPR_ARG;
				break;
			default:
				LexicalState = LexicalState.EXPR_BEG;
				break;
			}
			WarnBalanced("%%", "string literal");
			MarkSingleLineTokenEnd();
			return Tokens.Percent;
		}

		private Tokens TokenizeClosing(Tokens token)
		{
			LeaveParenthesisedExpression();
			LexicalState = ((token == Tokens.RightParenthesis) ? LexicalState.EXPR_ENDFN : LexicalState.EXPR_ENDARG);
			MarkSingleLineTokenEnd();
			return token;
		}

		private Tokens TokenizeClosingBrace()
		{
			if (_state.OpenedBracesInEmbeddedCode > 0)
			{
				_state.OpenedBracesInEmbeddedCode--;
				return TokenizeClosing(Tokens.RightBrace);
			}
			if (InStringEmbeddedCode)
			{
				_state.EndStringEmbeddedCode();
				return TokenizeClosing(Tokens.StringEmbeddedCodeEnd);
			}
			return TokenizeClosing(Tokens.RightBrace);
		}

		private Tokens ReadLeftBrace()
		{
			Tokens result;
			if (_lambdaOpenings <= 0 || _lambdaOpenings != _openingCount)
			{
				result = ((InArgs || LexicalState == LexicalState.EXPR_END || LexicalState == LexicalState.EXPR_ENDFN) ? Tokens.LeftBlockBrace : ((LexicalState != LexicalState.EXPR_ENDARG) ? Tokens.LeftBrace : Tokens.LeftBlockArgBrace));
			}
			else
			{
				result = Tokens.LeftLambdaBrace;
				_lambdaOpenings = 0;
				_openingCount--;
			}
			_state.OpenedBracesInEmbeddedCode++;
			EnterParenthesisedExpression();
			LexicalState = LexicalState.EXPR_BEG;
			return result;
		}

		private Tokens ReadLeftBracket()
		{
			if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT)
			{
				LexicalState = LexicalState.EXPR_ARG;
				if (!Read(93))
				{
					return Tokens.LeftIndexingBracket;
				}
				if (!Read(61))
				{
					return Tokens.ItemGetter;
				}
				return Tokens.ItemSetter;
			}
			Tokens result = (IsBeginLexicalState ? Tokens.LeftBracket : ((!InArgs || !WhitespaceSeen) ? Tokens.LeftIndexingBracket : Tokens.LeftBracket));
			LexicalState = LexicalState.EXPR_BEG;
			EnterParenthesisedExpression();
			return result;
		}

		private Tokens ReadTilde()
		{
			if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT)
			{
				Read(64);
				LexicalState = LexicalState.EXPR_ARG;
			}
			else
			{
				LexicalState = LexicalState.EXPR_BEG;
			}
			return Tokens.Tilde;
		}

		private Tokens ReadCaret()
		{
			if (Read(61))
			{
				SetAsciiStringToken(Symbols.Xor);
				LexicalState = LexicalState.EXPR_BEG;
				return Tokens.OpAssignment;
			}
			switch (LexicalState)
			{
			case LexicalState.EXPR_FNAME:
			case LexicalState.EXPR_DOT:
				LexicalState = LexicalState.EXPR_ARG;
				break;
			default:
				LexicalState = LexicalState.EXPR_BEG;
				break;
			}
			return Tokens.Caret;
		}

		private Tokens ReadSlash()
		{
			if (IsBeginLexicalState)
			{
				_state.CurrentSequence = new StringState(StringProperties.ExpandsEmbedded | StringProperties.RegularExpression, '/');
				return Tokens.RegexpBegin;
			}
			int num = Peek();
			if (num == 61)
			{
				Skip(num);
				SetAsciiStringToken(Symbols.Divide);
				LexicalState = LexicalState.EXPR_BEG;
				return Tokens.OpAssignment;
			}
			if (InArgsNoSpace(num))
			{
				ReportWarning(Errors.AmbiguousFirstArgument);
				_state.CurrentSequence = new StringState(StringProperties.ExpandsEmbedded | StringProperties.RegularExpression, '/');
				return Tokens.RegexpBegin;
			}
			switch (LexicalState)
			{
			case LexicalState.EXPR_FNAME:
			case LexicalState.EXPR_DOT:
				LexicalState = LexicalState.EXPR_ARG;
				break;
			default:
				LexicalState = LexicalState.EXPR_BEG;
				break;
			}
			WarnBalanced("/", "regexp literal");
			return Tokens.Slash;
		}

		private Tokens ReadColon()
		{
			int num = Peek();
			if (num == 58)
			{
				Skip(num);
				if (IsBeginLexicalState || (InArgs && WhitespaceSeen))
				{
					LexicalState = LexicalState.EXPR_BEG;
					return Tokens.LeadingDoubleColon;
				}
				LexicalState = LexicalState.EXPR_DOT;
				return Tokens.SeparatingDoubleColon;
			}
			if (IsEndLexicalState || IsWhiteSpace(num))
			{
				WarnBalanced(":", "symbol literal");
				LexicalState = LexicalState.EXPR_BEG;
				return Tokens.Colon;
			}
			switch (num)
			{
			case 39:
				Skip(num);
				_state.CurrentSequence = new StringState(StringProperties.Symbol, '\'');
				break;
			case 34:
				Skip(num);
				_state.CurrentSequence = new StringState(StringProperties.ExpandsEmbedded | StringProperties.Symbol, '"');
				break;
			}
			LexicalState = LexicalState.EXPR_FNAME;
			return Tokens.SymbolBegin;
		}

		private Tokens ReadStar()
		{
			int num = Peek();
			Tokens result;
			switch (num)
			{
			case 42:
				Skip(num);
				if (Read(61))
				{
					SetAsciiStringToken(Symbols.Power);
					LexicalState = LexicalState.EXPR_BEG;
					return Tokens.OpAssignment;
				}
				result = Tokens.Pow;
				break;
			case 61:
				Skip(num);
				SetAsciiStringToken(Symbols.Multiply);
				LexicalState = LexicalState.EXPR_BEG;
				return Tokens.OpAssignment;
			default:
				if (InArgsNoSpace(num))
				{
					ReportWarning(Errors.StarInterpretedAsSplatArgument);
					result = Tokens.Star;
				}
				else if (IsBeginLexicalState)
				{
					result = Tokens.Star;
				}
				else
				{
					WarnBalanced("*", "argument prefix");
					result = Tokens.Asterisk;
				}
				break;
			}
			switch (LexicalState)
			{
			case LexicalState.EXPR_FNAME:
			case LexicalState.EXPR_DOT:
				LexicalState = LexicalState.EXPR_ARG;
				break;
			default:
				LexicalState = LexicalState.EXPR_BEG;
				break;
			}
			return result;
		}

		private Tokens ReadBang()
		{
			int num = Peek();
			if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT)
			{
				LexicalState = LexicalState.EXPR_ARG;
				if (num == 64)
				{
					Skip(num);
					return Tokens.Bang;
				}
			}
			else
			{
				LexicalState = LexicalState.EXPR_BEG;
			}
			switch (num)
			{
			case 61:
				Skip(num);
				return Tokens.NotEqual;
			case 126:
				Skip(num);
				return Tokens.Nmatch;
			default:
				return Tokens.Bang;
			}
		}

		private Tokens TokenizeLessThan()
		{
			int num = Read();
			if (num == 60 && LexicalState != LexicalState.EXPR_DOT && LexicalState != LexicalState.EXPR_CLASS && !IsEndLexicalState && (!InArgs || WhitespaceSeen))
			{
				Tokens tokens = TokenizeHeredocLabel();
				if (tokens != Tokens.None)
				{
					return tokens;
				}
			}
			switch (LexicalState)
			{
			case LexicalState.EXPR_FNAME:
			case LexicalState.EXPR_DOT:
				LexicalState = LexicalState.EXPR_ARG;
				break;
			default:
				LexicalState = LexicalState.EXPR_BEG;
				break;
			}
			switch (num)
			{
			case 61:
				if (Read(62))
				{
					MarkSingleLineTokenEnd();
					return Tokens.Cmp;
				}
				MarkSingleLineTokenEnd();
				return Tokens.LessOrEqual;
			case 60:
				if (Read(61))
				{
					SetAsciiStringToken(Symbols.LeftShift);
					LexicalState = LexicalState.EXPR_BEG;
					MarkSingleLineTokenEnd();
					return Tokens.OpAssignment;
				}
				WarnBalanced("<<", "here document");
				MarkSingleLineTokenEnd();
				return Tokens.Lshft;
			default:
				Back(num);
				MarkSingleLineTokenEnd();
				return Tokens.Less;
			}
		}

		private Tokens ReadGreaterThan()
		{
			switch (LexicalState)
			{
			case LexicalState.EXPR_FNAME:
			case LexicalState.EXPR_DOT:
				LexicalState = LexicalState.EXPR_ARG;
				break;
			default:
				LexicalState = LexicalState.EXPR_BEG;
				break;
			}
			int num = Peek();
			switch (num)
			{
			case 61:
				Skip(num);
				return Tokens.GreaterOrEqual;
			case 62:
				Skip(num);
				if (Read(61))
				{
					SetAsciiStringToken(Symbols.RightShift);
					LexicalState = LexicalState.EXPR_BEG;
					return Tokens.OpAssignment;
				}
				return Tokens.Rshft;
			default:
				return Tokens.Greater;
			}
		}

		private Tokens ReadBacktick()
		{
			if (LexicalState == LexicalState.EXPR_FNAME)
			{
				LexicalState = LexicalState.EXPR_ENDFN;
				return Tokens.Backtick;
			}
			if (LexicalState == LexicalState.EXPR_DOT)
			{
				LexicalState = LexicalState.EXPR_ARG;
				return Tokens.Backtick;
			}
			_state.CurrentSequence = new StringState(StringProperties.ExpandsEmbedded, '`');
			return Tokens.ShellStringBegin;
		}

		private Tokens TokenizeQuestionmark()
		{
			if (IsEndLexicalState)
			{
				LexicalState = LexicalState.EXPR_VALUE;
				MarkSingleLineTokenEnd();
				return Tokens.QuestionMark;
			}
			int num = Peek();
			if (num == -1)
			{
				_unterminatedToken = true;
				MarkSingleLineTokenEnd();
				ReportError(Errors.IncompleteCharacter);
				return Tokens.EndOfFile;
			}
			if (IsWhiteSpace(num))
			{
				if (!InArgs)
				{
					int num2 = 0;
					switch (num)
					{
					case 32:
						num2 = 115;
						break;
					case 10:
						num2 = 110;
						break;
					case 9:
						num2 = 116;
						break;
					case 11:
						num2 = 118;
						break;
					case 13:
						num2 = ((Peek(1) == 10) ? 110 : 114);
						break;
					case 12:
						num2 = 102;
						break;
					}
					if (num2 != 0)
					{
						ReportWarning(Errors.InvalidCharacterSyntax, (char)num2);
					}
				}
				LexicalState = LexicalState.EXPR_VALUE;
				MarkSingleLineTokenEnd();
				return Tokens.QuestionMark;
			}
			if ((IsLetterOrDigit(num) || num == 95) && IsIdentifier(Peek(1)))
			{
				LexicalState = LexicalState.EXPR_BEG;
				MarkSingleLineTokenEnd();
				return Tokens.QuestionMark;
			}
			Skip(num);
			object stringContent;
			RubyEncoding encoding;
			if (num == 92)
			{
				num = Peek();
				if (num == 117)
				{
					Skip(num);
					int num3 = ((Peek() == 123) ? ReadUnicodeEscape6() : ReadUnicodeEscape4());
					stringContent = UnicodeCodePointToString(num3);
					encoding = ((num3 <= 127) ? _encoding : RubyEncoding.UTF8);
					MarkSingleLineTokenEnd();
				}
				else
				{
					num = ReadEscape();
					if (num <= 127)
					{
						stringContent = new string((char)num, 1);
						encoding = _encoding;
					}
					else
					{
						stringContent = new byte[1] { (byte)num };
						encoding = ((_encoding == RubyEncoding.Ascii) ? RubyEncoding.Binary : _encoding);
					}
					MarkMultiLineTokenEnd();
				}
			}
			else
			{
				int num4;
				if (IsHighSurrogate(num) && IsLowSurrogate(num4 = Peek()))
				{
					Skip(num4);
					stringContent = new string(new char[2]
					{
						(char)num,
						(char)num4
					});
				}
				else
				{
					stringContent = new string((char)num, 1);
				}
				encoding = _encoding;
				MarkSingleLineTokenEnd();
			}
			LexicalState = LexicalState.EXPR_END;
			_tokenValue.StringContent = stringContent;
			_tokenValue.Encoding = encoding;
			return Tokens.Character;
		}

		private Tokens ReadAmpersand()
		{
			int num = Peek();
			switch (num)
			{
			case 38:
				Skip(num);
				LexicalState = LexicalState.EXPR_BEG;
				if (Read(61))
				{
					SetAsciiStringToken(Symbols.And);
					return Tokens.OpAssignment;
				}
				return Tokens.LogicalAnd;
			case 61:
				Skip(num);
				LexicalState = LexicalState.EXPR_BEG;
				SetAsciiStringToken(Symbols.BitwiseAnd);
				return Tokens.OpAssignment;
			default:
			{
				Tokens result;
				if (InArgsNoSpace(num))
				{
					ReportWarning(Errors.AmpersandInterpretedAsProcArgument);
					result = Tokens.BlockReference;
				}
				else if (IsBeginLexicalState)
				{
					result = Tokens.BlockReference;
				}
				else
				{
					WarnBalanced("&", "argument prefix");
					result = Tokens.Ampersand;
				}
				switch (LexicalState)
				{
				case LexicalState.EXPR_FNAME:
				case LexicalState.EXPR_DOT:
					LexicalState = LexicalState.EXPR_ARG;
					break;
				default:
					LexicalState = LexicalState.EXPR_BEG;
					break;
				}
				return result;
			}
			}
		}

		private Tokens ReadPipe()
		{
			int num = Peek();
			switch (num)
			{
			case 124:
				Skip(num);
				LexicalState = LexicalState.EXPR_BEG;
				if (Read(61))
				{
					SetAsciiStringToken(Symbols.Or);
					LexicalState = LexicalState.EXPR_BEG;
					return Tokens.OpAssignment;
				}
				return Tokens.LogicalOr;
			case 61:
				Skip(num);
				SetAsciiStringToken(Symbols.BitwiseOr);
				LexicalState = LexicalState.EXPR_BEG;
				return Tokens.OpAssignment;
			default:
				if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT)
				{
					LexicalState = LexicalState.EXPR_ARG;
				}
				else
				{
					LexicalState = LexicalState.EXPR_BEG;
				}
				return Tokens.Pipe;
			}
		}

		private Tokens ReadDot()
		{
			LexicalState = LexicalState.EXPR_BEG;
			int num = Peek();
			if (num == 46)
			{
				Skip(num);
				if (!Read(46))
				{
					return Tokens.DoubleDot;
				}
				return Tokens.TripleDot;
			}
			if (IsDecimalDigit(num))
			{
				ReportError(Errors.NoFloatingLiteral);
			}
			LexicalState = LexicalState.EXPR_DOT;
			return Tokens.Dot;
		}

		private Tokens ReadMinus()
		{
			if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT)
			{
				LexicalState = LexicalState.EXPR_ARG;
				if (!Read(64))
				{
					return Tokens.Minus;
				}
				return Tokens.UnaryMinus;
			}
			int num = Peek();
			switch (num)
			{
			case 61:
				Skip(num);
				SetAsciiStringToken(Symbols.Minus);
				LexicalState = LexicalState.EXPR_BEG;
				return Tokens.OpAssignment;
			case 62:
				Skip(num);
				LexicalState = LexicalState.EXPR_ARG;
				return Tokens.Lambda;
			default:
			{
				bool flag = false;
				if (IsBeginLexicalState || (flag = InArgsNoSpace(num)))
				{
					if (flag)
					{
						ReportWarning(Errors.AmbiguousFirstArgument);
					}
					LexicalState = LexicalState.EXPR_BEG;
					if (!IsDecimalDigit(num))
					{
						return Tokens.UnaryMinus;
					}
					return Tokens.NumberNegation;
				}
				LexicalState = LexicalState.EXPR_BEG;
				WarnBalanced("-", "unary operator");
				return Tokens.Minus;
			}
			}
		}

		private RubyRegexOptions ReadRegexOptions()
		{
			RubyRegexOptions rubyRegexOptions = RubyRegexOptions.NONE;
			RubyRegexOptions rubyRegexOptions2 = RubyRegexOptions.NONE;
			while (true)
			{
				int num = Peek();
				if (!IsLetter(num))
				{
					break;
				}
				Skip(num);
				switch (num)
				{
				case 105:
					rubyRegexOptions2 |= RubyRegexOptions.IgnoreCase;
					break;
				case 120:
					rubyRegexOptions2 |= RubyRegexOptions.Extended;
					break;
				case 109:
					rubyRegexOptions2 |= RubyRegexOptions.Multiline;
					break;
				case 111:
					rubyRegexOptions2 |= RubyRegexOptions.Once;
					break;
				case 110:
					rubyRegexOptions = RubyRegexOptions.FIXED;
					break;
				case 101:
					rubyRegexOptions = RubyRegexOptions.EUC;
					break;
				case 115:
					rubyRegexOptions = RubyRegexOptions.SJIS;
					break;
				case 117:
					rubyRegexOptions = RubyRegexOptions.UTF8;
					break;
				default:
					ReportError(Errors.UnknownRegexOption, (char)num);
					break;
				}
			}
			return rubyRegexOptions2 | rubyRegexOptions;
		}

		private int ReadEscape()
		{
			int num = Read();
			switch (num)
			{
			case 92:
				return 92;
			case 110:
				return 10;
			case 116:
				return 9;
			case 114:
				return 13;
			case 102:
				return 12;
			case 118:
				return 11;
			case 97:
				return 7;
			case 101:
				return 27;
			case 98:
				return 8;
			case 115:
				return 32;
			case 120:
				return ReadHexEscape();
			case 77:
				if (!Read(45))
				{
					return InvalidEscapeCharacter();
				}
				num = ReadNormalizeEndOfLine();
				switch (num)
				{
				case 92:
					return ReadEscape() | 0x80;
				case -1:
					return InvalidEscapeCharacter();
				default:
					return (num & 0xFF) | 0x80;
				}
			case 67:
				if (!Read(45))
				{
					return InvalidEscapeCharacter();
				}
				goto case 99;
			case 99:
				num = ReadNormalizeEndOfLine();
				switch (num)
				{
				case 63:
					return 177;
				case -1:
					return InvalidEscapeCharacter();
				case 92:
					num = ReadEscape();
					break;
				}
				return num & 0x9F;
			case -1:
				return InvalidEscapeCharacter();
			default:
				if (IsOctalDigit(num))
				{
					return ReadOctalEscape(num - 48);
				}
				return num;
			}
		}

		private int InvalidEscapeCharacter()
		{
			ReportError(Errors.InvalidEscapeCharacter);
			return 63;
		}

		private void AppendEscapedRegexEscape(MutableStringBuilder content, int term)
		{
			int num = Read();
			switch (num)
			{
			case 120:
				content.AppendAscii('\\');
				AppendEscapedHexEscape(content);
				return;
			case 77:
				if (!Read(45))
				{
					InvalidEscapeCharacter();
					return;
				}
				content.AppendAscii('\\');
				content.AppendAscii('M');
				content.AppendAscii('-');
				AppendRegularExpressionCompositeEscape(content, term);
				return;
			case 67:
				if (!Read(45))
				{
					InvalidEscapeCharacter();
					return;
				}
				content.AppendAscii('\\');
				content.AppendAscii('C');
				content.AppendAscii('-');
				AppendRegularExpressionCompositeEscape(content, term);
				return;
			case 99:
				content.AppendAscii('\\');
				content.AppendAscii('c');
				AppendRegularExpressionCompositeEscape(content, term);
				return;
			case -1:
				InvalidEscapeCharacter();
				return;
			}
			if (IsOctalDigit(num))
			{
				content.AppendAscii('\\');
				AppendEscapedOctalEscape(content);
				return;
			}
			if (num != 92 || num != term)
			{
				content.AppendAscii('\\');
			}
			AppendCharacter(content, num);
		}

		private void AppendRegularExpressionCompositeEscape(MutableStringBuilder content, int term)
		{
			int num = ReadNormalizeEndOfLine();
			switch (num)
			{
			case 92:
				AppendEscapedRegexEscape(content, term);
				break;
			case -1:
				InvalidEscapeCharacter();
				break;
			default:
				AppendCharacter(content, num);
				break;
			}
		}

		private void AppendEscapedOctalEscape(MutableStringBuilder content)
		{
			int num = _bufferPos - 1;
			ReadOctalEscape(0);
			content.AppendAscii(_lineBuffer, num, _bufferPos - num);
		}

		private void AppendEscapedHexEscape(MutableStringBuilder content)
		{
			int num = _bufferPos - 1;
			ReadHexEscape();
			content.AppendAscii(_lineBuffer, num, _bufferPos - num);
		}

		private bool AppendEscapedUnicode(MutableStringBuilder content)
		{
			bool flag = false;
			int num = _bufferPos - 1;
			flag = ((Peek() != 123) ? (ReadUnicodeEscape4() >= 128) : AppendUnicodeCodePoints(null));
			content.AppendAscii(_lineBuffer, num, _bufferPos - num);
			return flag;
		}

		private int ReadOctalEscape(int value)
		{
			int num;
			if (IsOctalDigit(num = Peek()))
			{
				Skip(num);
				value = (value << 3) | (num - 48);
				if (IsOctalDigit(num = Peek()))
				{
					Skip(num);
					value = (value << 3) | (num - 48);
				}
			}
			return value;
		}

		private int ReadHexEscape()
		{
			int c;
			int num = ToDigit(c = Peek());
			if (num < 16)
			{
				Skip(c);
				int num2 = ToDigit(c = Peek());
				if (num2 < 16)
				{
					Skip(c);
					num = (num << 4) | num2;
				}
				return num;
			}
			return InvalidEscapeCharacter();
		}

		private int ReadUnicodeEscape4()
		{
			int num = ToDigit(Peek(0));
			int num2 = ToDigit(Peek(1));
			int num3 = ToDigit(Peek(2));
			int num4 = ToDigit(Peek(3));
			if (num4 >= 16 || num3 >= 16 || num2 >= 16 || num >= 16)
			{
				return InvalidEscapeCharacter();
			}
			SeekRelative(4);
			return (num << 12) | (num2 << 8) | (num3 << 4) | num4;
		}

		private int ReadUnicodeEscape6()
		{
			Read();
			bool isEmpty;
			int result = ReadUnicodeCodePoint(out isEmpty);
			if (Peek() == 125)
			{
				Skip();
			}
			else
			{
				ReportError(Errors.UntermintedUnicodeEscape);
				isEmpty = false;
			}
			if (isEmpty)
			{
				ReportError(Errors.InvalidUnicodeEscape);
			}
			return result;
		}

		private int ReadUnicodeCodePoint(out bool isEmpty)
		{
			int num = 0;
			int num2 = 0;
			while (true)
			{
				int num3 = ToDigit(Peek());
				if (num3 >= 16)
				{
					break;
				}
				if (num2 < 7)
				{
					num = (num << 4) | num3;
				}
				num2++;
				Skip();
			}
			if (num > 1114111)
			{
				ReportError(Errors.TooLargeUnicodeCodePoint);
				num = 63;
			}
			isEmpty = num2 == 0;
			return num;
		}

		private bool AppendUnicodeCodePoints(MutableStringBuilder content)
		{
			bool flag = true;
			Skip(123);
			bool isEmpty;
			while (true)
			{
				int num = ReadUnicodeCodePoint(out isEmpty);
				if (content != null && !isEmpty)
				{
					AppendUnicodeCodePoint(content, num);
				}
				flag = flag && num <= 127;
				switch (Peek())
				{
				case 125:
					Skip();
					break;
				default:
					ReportError(Errors.UntermintedUnicodeEscape);
					isEmpty = false;
					break;
				case 32:
					goto IL_0054;
				}
				break;
				IL_0054:
				Skip();
			}
			if (isEmpty)
			{
				ReportError(Errors.InvalidUnicodeEscape);
			}
			return !flag;
		}

		private void AppendUnicodeCodePoint(MutableStringBuilder content, int codepoint)
		{
			if (codepoint >= 128)
			{
				SwitchToUtf8(content);
			}
			content.AppendUnicodeCodepoint(codepoint);
		}

		private void SwitchToUtf8(MutableStringBuilder content)
		{
			if (content.IsAscii)
			{
				content.Encoding = RubyEncoding.UTF8;
			}
			else if (content.Encoding != RubyEncoding.UTF8)
			{
				ReportError(Errors.EncodingsMixed, content.Encoding, RubyEncoding.UTF8.Name);
			}
		}

		private void AppendByte(MutableStringBuilder content, int b)
		{
			if (b >= 128 && content.Encoding == RubyEncoding.Ascii)
			{
				content.Encoding = RubyEncoding.Binary;
			}
			content.Append((byte)b);
		}

		private void AppendCharacter(MutableStringBuilder content, int c)
		{
			if (c >= 128 && !content.IsAscii && content.Encoding != _encoding)
			{
				ReportError(Errors.EncodingsMixed, content.Encoding, _encoding);
			}
			content.Append((char)c);
		}

		public static string UnicodeCodePointToString(int codepoint)
		{
			if (codepoint < 65536)
			{
				return new string((char)codepoint, 1);
			}
			codepoint -= 65536;
			return new string(new char[2]
			{
				(char)(codepoint / 1024 + 55296),
				(char)(codepoint % 1024 + 56320)
			});
		}

		public static int ToCodePoint(int highSurrogate, int lowSurrogate)
		{
			return (highSurrogate - 55296) * 1024 + (lowSurrogate - 56320) + 65536;
		}

		public static bool IsHighSurrogate(int c)
		{
			return (uint)(c - 55296) <= 1023u;
		}

		public static bool IsLowSurrogate(int c)
		{
			return (uint)(c - 56320) <= 1023u;
		}

		public static bool IsSurrogate(int c)
		{
			return (uint)(c - 55296) <= 2047u;
		}

		internal void SetStringToken(string value)
		{
			_tokenValue.SetString(value);
		}

		internal void SetAsciiStringToken(string symbol)
		{
			_tokenValue.SetString(symbol);
		}

		internal void SetStringToken(int start, int length)
		{
			SetStringToken(new string(_lineBuffer, start, length));
		}

		private Tokens ReadDoubleQuote()
		{
			_state.CurrentSequence = new StringState(StringProperties.ExpandsEmbedded, '"');
			return Tokens.StringBegin;
		}

		private Tokens ReadSingleQuote()
		{
			_state.CurrentSequence = new StringState(StringProperties.Default, '\'');
			return Tokens.StringBegin;
		}

		private int ReadStringContent(MutableStringBuilder content, StringProperties stringType, int terminator, int openingParenthesis, ref int nestingLevel)
		{
			int num;
			int eolnWidth;
			while (true)
			{
				num = ReadNormalizeEndOfLine(out eolnWidth);
				if (num == -1)
				{
					return -1;
				}
				if (openingParenthesis != 0 && num == openingParenthesis)
				{
					nestingLevel++;
				}
				else if (num == terminator)
				{
					if (nestingLevel == 0)
					{
						SeekRelative(-eolnWidth);
						return num;
					}
					nestingLevel--;
				}
				else if ((stringType & StringProperties.ExpandsEmbedded) != 0 && num == 35 && _bufferPos < _lineLength)
				{
					int num2 = _lineBuffer[_bufferPos];
					if (num2 == 36 || num2 == 64 || num2 == 123)
					{
						SeekRelative(-eolnWidth);
						return num;
					}
				}
				else
				{
					if ((stringType & StringProperties.Words) != 0 && IsWhiteSpace(num))
					{
						break;
					}
					if (num == 92)
					{
						num = ReadNormalizeEndOfLine(out eolnWidth);
						switch (num)
						{
						case 10:
							if ((stringType & StringProperties.Words) == 0)
							{
								if ((stringType & StringProperties.ExpandsEmbedded) != 0)
								{
									continue;
								}
								content.AppendAscii('\\');
							}
							break;
						case 92:
							if ((stringType & StringProperties.RegularExpression) != 0)
							{
								content.AppendAscii('\\');
							}
							break;
						default:
							if ((stringType & StringProperties.RegularExpression) != 0)
							{
								if (num == 117)
								{
									content.AppendAscii('\\');
									if (AppendEscapedUnicode(content))
									{
										SwitchToUtf8(content);
									}
								}
								else
								{
									SeekRelative(-eolnWidth);
									AppendEscapedRegexEscape(content, terminator);
								}
								continue;
							}
							if ((stringType & StringProperties.ExpandsEmbedded) != 0)
							{
								if (num == 117)
								{
									if (Peek() == 123)
									{
										if (AppendUnicodeCodePoints(content))
										{
											SwitchToUtf8(content);
										}
										continue;
									}
									int num3 = ReadUnicodeEscape4();
									AppendUnicodeCodePoint(content, num3);
									if (num3 >= 128)
									{
										SwitchToUtf8(content);
									}
								}
								else
								{
									SeekRelative(-eolnWidth);
									AppendByte(content, ReadEscape());
								}
								continue;
							}
							if (((stringType & StringProperties.Words) == 0 || !IsWhiteSpace(num)) && num != terminator && (openingParenthesis == 0 || num != openingParenthesis))
							{
								content.AppendAscii('\\');
							}
							break;
						}
					}
				}
				AppendCharacter(content, num);
			}
			SeekRelative(-eolnWidth);
			return num;
		}

		internal Tokens TokenizeString(StringState info)
		{
			if (InStringEmbeddedVariable)
			{
				InStringEmbeddedVariable = false;
				return Tokenize();
			}
			StringProperties properties = info.Properties;
			bool flag = false;
			MarkTokenStart();
			int eolnWidth;
			int num = ReadNormalizeEndOfLine(out eolnWidth);
			if (num == -1)
			{
				_unterminatedToken = true;
				MarkSingleLineTokenEnd();
				if (_verbatim)
				{
					return Tokens.EndOfFile;
				}
				ReportError(Errors.UnterminatedString);
				return FinishString(Tokens.StringEnd);
			}
			bool flag2 = num == 10;
			if ((properties & StringProperties.Words) != 0 && IsWhiteSpace(num))
			{
				flag2 |= SkipWhitespace();
				num = Read();
				flag = true;
			}
			if (num == info.TerminatingCharacter && info.NestingLevel == 0)
			{
				if ((properties & StringProperties.Words) != 0)
				{
					MarkTokenEnd(flag2);
					return FinishString(Tokens.StringEnd);
				}
				if ((properties & StringProperties.RegularExpression) != 0)
				{
					_tokenValue.SetRegexOptions(ReadRegexOptions());
					MarkTokenEnd(flag2);
					return FinishString(Tokens.RegexpEnd);
				}
				MarkTokenEnd(flag2);
				return FinishString(Tokens.StringEnd);
			}
			if (flag)
			{
				Back(num);
				MarkTokenEnd(flag2);
				return Tokens.WordSeparator;
			}
			MutableStringBuilder mutableStringBuilder;
			if ((properties & StringProperties.ExpandsEmbedded) != 0 && num == 35)
			{
				switch (Peek())
				{
				case 36:
				case 64:
					MarkSingleLineTokenEnd();
					return StringEmbeddedVariableBegin();
				case 123:
					Skip(123);
					MarkSingleLineTokenEnd();
					return StringEmbeddedCodeBegin();
				}
				mutableStringBuilder = new MutableStringBuilder(_encoding);
				mutableStringBuilder.AppendAscii('#');
			}
			else
			{
				mutableStringBuilder = new MutableStringBuilder(_encoding);
				SeekRelative(-eolnWidth);
			}
			int nestingLevel = info.NestingLevel;
			ReadStringContent(mutableStringBuilder, properties, info.TerminatingCharacter, info.OpeningParenthesis, ref nestingLevel);
			_state.CurrentSequence = info.SetNesting(nestingLevel);
			_tokenValue.SetStringContent(mutableStringBuilder);
			MarkMultiLineTokenEnd();
			return Tokens.StringContent;
		}

		private Tokens FinishString(Tokens endToken)
		{
			_state.CurrentSequence = TokenSequenceState.None;
			LexicalState = LexicalState.EXPR_END;
			return endToken;
		}

		private Tokens TokenizeHeredocLabel()
		{
			StringProperties stringProperties = StringProperties.Default;
			int eolnWidth;
			int num = ReadNormalizeEndOfLine(out eolnWidth);
			if (num == 45)
			{
				num = ReadNormalizeEndOfLine(out eolnWidth);
				eolnWidth++;
				stringProperties = StringProperties.IndentedHeredoc;
			}
			int num2;
			string label;
			if (num == 39 || num == 34 || num == 96)
			{
				if (num != 39)
				{
					stringProperties |= StringProperties.ExpandsEmbedded;
				}
				int bufferPos = _bufferPos;
				num2 = num;
				while (true)
				{
					num = Read();
					if (num == -1)
					{
						_unterminatedToken = true;
						ReportError(Errors.UnterminatedHereDocIdentifier);
						num = num2;
						break;
					}
					if (num == num2)
					{
						break;
					}
					if (num == 10)
					{
						Back(10);
						ReportError(Errors.UnterminatedHereDocIdentifier);
						num = num2;
						break;
					}
				}
				label = new string(_lineBuffer, bufferPos, _bufferPos - bufferPos - 1);
			}
			else
			{
				if (!IsIdentifier(num))
				{
					SeekRelative(-eolnWidth);
					return Tokens.None;
				}
				num2 = 34;
				stringProperties |= StringProperties.ExpandsEmbedded;
				int num3 = _bufferPos - 1;
				SkipVariableName();
				label = new string(_lineBuffer, num3, _bufferPos - num3);
			}
			MarkSingleLineTokenEnd();
			if (_verbatim)
			{
				_state.EnqueueVerbatimHeredoc(new VerbatimHeredocState(stringProperties, label));
				return Tokens.VerbatimHeredocBegin;
			}
			int bufferPos2 = _bufferPos;
			_bufferPos = _lineLength;
			_state.CurrentSequence = new HeredocState(stringProperties, label, bufferPos2, _lineBuffer, _lineLength, _currentLine, _currentLineIndex);
			_lineBuffer = new char[80];
			if (num2 != 96)
			{
				return Tokens.StringBegin;
			}
			return Tokens.ShellStringBegin;
		}

		private void MarkHeredocEnd(HeredocStateBase heredoc, int labelStart)
		{
			if (labelStart < 0)
			{
				MarkTokenStart();
				MarkSingleLineTokenEnd();
				return;
			}
			SeekRelative(labelStart);
			MarkTokenStart();
			SeekRelative(heredoc.Label.Length);
			if (TryReadEndOfLine())
			{
				MarkMultiLineTokenEnd();
			}
			else
			{
				MarkSingleLineTokenEnd();
			}
		}

		internal Tokens FinishVerbatimHeredoc(VerbatimHeredocState heredoc, int labelStart)
		{
			MarkHeredocEnd(heredoc, labelStart);
			_state.FinishVerbatimHeredoc();
			CaptureTokenSpan();
			return Tokens.VerbatimHeredocEnd;
		}

		internal Tokens FinishHeredoc(HeredocState heredoc, int labelStart)
		{
			MarkHeredocEnd(heredoc, labelStart);
			_state.HeredocEndLine = _currentTokenEnd.Line;
			_state.HeredocEndLineIndex = _currentTokenEnd.Index;
			_state.CurrentSequence = TokenSequenceState.None;
			LexicalState = LexicalState.EXPR_END;
			_lineBuffer = heredoc.ResumeLine;
			_lineLength = heredoc.ResumeLineLength;
			_bufferPos = heredoc.ResumePosition;
			_currentLine = heredoc.FirstLine;
			_currentLineIndex = heredoc.FirstLineIndex;
			MarkTokenStart();
			MarkSingleLineTokenEnd();
			CaptureTokenSpan();
			return Tokens.StringEnd;
		}

		internal Tokens TokenizeAndMarkHeredoc(HeredocStateBase heredoc)
		{
			if (InStringEmbeddedVariable)
			{
				InStringEmbeddedVariable = false;
				CaptureTokenSpan();
				return Tokenize();
			}
			StringProperties properties = heredoc.Properties;
			bool skipWhitespace = (properties & StringProperties.IndentedHeredoc) != 0;
			if (Peek() == -1)
			{
				ReportError(Errors.UnterminatedHereDoc, heredoc.Label);
				_unterminatedToken = true;
				return heredoc.Finish(this, -1);
			}
			int strStart;
			if (is_bol() && LineContentEquals(heredoc.Label, skipWhitespace, out strStart))
			{
				return heredoc.Finish(this, strStart);
			}
			MarkTokenStart();
			if ((properties & StringProperties.ExpandsEmbedded) == 0)
			{
				StringBuilder stringBuilder = ReadNonexpandingHeredocContent(heredoc);
				SetStringToken(stringBuilder.ToString());
				MarkMultiLineTokenEnd();
				CaptureTokenSpan();
				return Tokens.StringContent;
			}
			Tokens result = TokenizeExpandingHeredocContent(heredoc);
			CaptureTokenSpan();
			return result;
		}

		private StringBuilder ReadNonexpandingHeredocContent(HeredocStateBase heredoc)
		{
			bool skipWhitespace = (heredoc.Properties & StringProperties.IndentedHeredoc) != 0;
			StringBuilder stringBuilder = new StringBuilder();
			do
			{
				int num = _lineLength;
				if (num > 0)
				{
					switch (_lineBuffer[num - 1])
					{
					case '\n':
						num = ((--num != 0 && _lineBuffer[num - 1] == '\r') ? (num - 1) : (num + 1));
						break;
					case '\r':
						num--;
						break;
					}
				}
				stringBuilder.Append(_lineBuffer, 0, num);
				if (num < _lineLength)
				{
					stringBuilder.Append('\n');
				}
				_bufferPos = _lineLength;
				RefillBuffer();
				if (Peek() == -1)
				{
					return stringBuilder;
				}
			}
			while (!LineContentEquals(heredoc.Label, skipWhitespace));
			_bufferPos = 0;
			return stringBuilder;
		}

		private Tokens TokenizeExpandingHeredocContent(HeredocStateBase heredoc)
		{
			int num = Peek();
			MutableStringBuilder mutableStringBuilder;
			if (num == 35)
			{
				Skip(num);
				switch (Peek())
				{
				case 36:
				case 64:
					MarkSingleLineTokenEnd();
					return StringEmbeddedVariableBegin();
				case 123:
					Skip(123);
					MarkSingleLineTokenEnd();
					return StringEmbeddedCodeBegin();
				}
				mutableStringBuilder = new MutableStringBuilder(_encoding);
				mutableStringBuilder.AppendAscii('#');
			}
			else
			{
				mutableStringBuilder = new MutableStringBuilder(_encoding);
			}
			bool skipWhitespace = (heredoc.Properties & StringProperties.IndentedHeredoc) != 0;
			do
			{
				int nestingLevel = 0;
				num = ReadStringContent(mutableStringBuilder, heredoc.Properties, 10, 0, ref nestingLevel);
				if (num != 10)
				{
					break;
				}
				mutableStringBuilder.AppendAscii((char)ReadNormalizeEndOfLine());
				if (num == 10 && _verbatim && _state.VerbatimHeredocQueue != null)
				{
					break;
				}
				RefillBuffer();
			}
			while (Peek() != -1 && !LineContentEquals(heredoc.Label, skipWhitespace));
			_tokenValue.SetStringContent(mutableStringBuilder);
			MarkMultiLineTokenEnd();
			return Tokens.StringContent;
		}

		private Tokens TokenizeQuotationStart()
		{
			int num = ReadNormalizeEndOfLine();
			StringProperties stringProperties;
			Tokens result;
			int num2;
			switch (num)
			{
			case 81:
				stringProperties = StringProperties.ExpandsEmbedded;
				result = Tokens.StringBegin;
				num2 = ReadNormalizeEndOfLine();
				break;
			case 113:
				stringProperties = StringProperties.Default;
				result = Tokens.StringBegin;
				num2 = ReadNormalizeEndOfLine();
				break;
			case 87:
				stringProperties = StringProperties.ExpandsEmbedded | StringProperties.Words;
				result = Tokens.WordsBegin;
				num2 = ReadNormalizeEndOfLine();
				break;
			case 119:
				stringProperties = StringProperties.Words;
				result = Tokens.VerbatimWordsBegin;
				num2 = ReadNormalizeEndOfLine();
				break;
			case 120:
				stringProperties = StringProperties.ExpandsEmbedded;
				result = Tokens.ShellStringBegin;
				num2 = ReadNormalizeEndOfLine();
				break;
			case 114:
				stringProperties = StringProperties.ExpandsEmbedded | StringProperties.RegularExpression;
				result = Tokens.RegexpBegin;
				num2 = ReadNormalizeEndOfLine();
				break;
			case 115:
				stringProperties = StringProperties.Symbol;
				result = Tokens.SymbolBegin;
				num2 = ReadNormalizeEndOfLine();
				LexicalState = LexicalState.EXPR_FNAME;
				break;
			default:
				stringProperties = StringProperties.ExpandsEmbedded;
				result = Tokens.StringBegin;
				num2 = num;
				break;
			}
			int num3 = num2;
			switch (num2)
			{
			case -1:
				_unterminatedToken = true;
				MarkSingleLineTokenEnd();
				ReportError(Errors.UnterminatedString);
				return Tokens.EndOfFile;
			case 40:
				num2 = 41;
				break;
			case 123:
				num2 = 125;
				break;
			case 91:
				num2 = 93;
				break;
			case 60:
				num2 = 62;
				break;
			default:
				if (IsLetterOrDigit(num2))
				{
					Back(num2);
					MarkSingleLineTokenEnd();
					ReportError(Errors.UnknownQuotedStringType);
					return Tokens.Percent;
				}
				num3 = 0;
				break;
			}
			bool flag = num2 == 10;
			if ((stringProperties & StringProperties.Words) != 0)
			{
				flag |= SkipWhitespace();
			}
			if (flag)
			{
				MarkMultiLineTokenEnd();
			}
			else
			{
				MarkSingleLineTokenEnd();
			}
			_state.CurrentSequence = new StringState(stringProperties, (char)num2, (char)num3, 0);
			return result;
		}

		private Tokens ReadUnsignedNumber(int c)
		{
			LexicalState = LexicalState.EXPR_END;
			if (c == 48)
			{
				switch (Peek())
				{
				case 88:
				case 120:
					Skip();
					return ReadInteger(16, NumericCharKind.None);
				case 66:
				case 98:
					Skip();
					return ReadInteger(2, NumericCharKind.None);
				case 79:
				case 111:
					Skip();
					return ReadInteger(8, NumericCharKind.None);
				case 68:
				case 100:
					Skip();
					return ReadInteger(10, NumericCharKind.None);
				case 69:
				case 101:
				{
					int numberStartIndex = _bufferPos - 1;
					int sign;
					if (TryReadExponentSign(1, out sign))
					{
						return ReadDoubleExponent(numberStartIndex, sign);
					}
					_tokenValue.SetInteger(0);
					return Tokens.Integer;
				}
				case 46:
					if (IsDecimalDigit(Peek(1)))
					{
						Skip(46);
						return ReadDouble(_bufferPos - 2);
					}
					_tokenValue.SetInteger(0);
					return Tokens.Integer;
				case 48:
				case 49:
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
				case 95:
					return ReadInteger(8, NumericCharKind.Digit);
				case 56:
				case 57:
					ReportError(Errors.IllegalOctalDigit);
					return ReadInteger(10, NumericCharKind.Digit);
				default:
					_tokenValue.SetInteger(0);
					return Tokens.Integer;
				}
			}
			return ReadDecimalNumber(c);
		}

		private Tokens ReadInteger(int @base, NumericCharKind prev)
		{
			long num = 0L;
			int bufferPos = _bufferPos;
			int num2 = 0;
			int num3;
			while (true)
			{
				num3 = Peek();
				int num4 = ToDigit(num3);
				if (num4 < @base)
				{
					Skip(num3);
					num = num * @base + num4;
					prev = NumericCharKind.Digit;
					if (num > int.MaxValue)
					{
						return ReadBigNumber(num, @base, bufferPos, num2, false);
					}
					continue;
				}
				switch (prev)
				{
				case NumericCharKind.Underscore:
					ReportError(Errors.TrailingUnderscoreInNumber);
					break;
				default:
					ReportError(Errors.NumericLiteralWithoutDigits);
					break;
				case NumericCharKind.Digit:
					if (num3 == 95)
					{
						Skip(num3);
						prev = NumericCharKind.Underscore;
						num2++;
						continue;
					}
					break;
				}
				break;
			}
			if (num3 == 46 && IsDecimalDigit(Peek(1)))
			{
				ReportWarning(Errors.NoFloatingLiteral);
			}
			_tokenValue.SetInteger((int)num);
			return Tokens.Integer;
		}

		private Tokens ReadDecimalNumber(int c)
		{
			int numberStartIndex = _bufferPos - 1;
			int num = 0;
			NumericCharKind numericCharKind = NumericCharKind.Digit;
			long num2 = c - 48;
			while (true)
			{
				c = Peek();
				if (IsDecimalDigit(c))
				{
					Skip(c);
					numericCharKind = NumericCharKind.Digit;
					num2 = num2 * 10 + (c - 48);
					if (num2 > int.MaxValue)
					{
						return ReadBigNumber(num2, 10, numberStartIndex, num, true);
					}
					continue;
				}
				if (numericCharKind == NumericCharKind.Underscore)
				{
					ReportError(Errors.TrailingUnderscoreInNumber);
					_tokenValue.SetInteger((int)num2);
					return Tokens.Integer;
				}
				int sign;
				if ((c == 101 || c == 69) && TryReadExponentSign(1, out sign))
				{
					return ReadDoubleExponent(numberStartIndex, sign);
				}
				switch (c)
				{
				case 95:
					Skip(c);
					num++;
					numericCharKind = NumericCharKind.Underscore;
					continue;
				case 46:
					if (IsDecimalDigit(Peek(1)))
					{
						Skip(46);
						return ReadDouble(numberStartIndex);
					}
					break;
				}
				break;
			}
			_tokenValue.SetInteger((int)num2);
			return Tokens.Integer;
		}

		private bool TryReadExponentSign(int offset, out int sign)
		{
			int num = Peek(offset);
			switch (num)
			{
			case 45:
				offset++;
				sign = -1;
				break;
			case 43:
				offset++;
				sign = 1;
				break;
			default:
				sign = 1;
				break;
			}
			if (IsDecimalDigit(Peek(offset)))
			{
				SeekRelative(offset);
				return true;
			}
			switch (num)
			{
			case 45:
				ReportError(Errors.TrailingMinusInNumber);
				break;
			case 43:
				ReportError(Errors.TrailingPlusInNumber);
				break;
			default:
				ReportError(Errors.TrailingEInNumber);
				break;
			}
			return false;
		}

		private Tokens ReadBigNumber(long value, int @base, int numberStartIndex, int underscoreCount, bool allowDouble)
		{
			NumericCharKind numericCharKind = NumericCharKind.Digit;
			while (true)
			{
				int num = Peek();
				int num2 = ToDigit(num);
				if (num2 < @base)
				{
					numericCharKind = NumericCharKind.Digit;
					Skip(num);
					continue;
				}
				if (numericCharKind == NumericCharKind.Underscore)
				{
					ReportError(Errors.TrailingUnderscoreInNumber);
					break;
				}
				if (num == 95)
				{
					Skip(num);
					numericCharKind = NumericCharKind.Underscore;
					underscoreCount++;
					continue;
				}
				if (!allowDouble)
				{
					break;
				}
				int sign;
				if ((num == 101 || num == 69) && TryReadExponentSign(1, out sign))
				{
					return ReadDoubleExponent(numberStartIndex, sign);
				}
				if (num != 46 || !IsDecimalDigit(Peek(1)))
				{
					break;
				}
				Skip(46);
				return ReadDouble(numberStartIndex);
			}
			if (_bigIntParser == null)
			{
				_bigIntParser = new BignumParser();
			}
			_bigIntParser.Position = numberStartIndex;
			_bigIntParser.Buffer = _lineBuffer;
			BigInteger bigInteger = _bigIntParser.Parse(_bufferPos - numberStartIndex - underscoreCount, @base);
			_tokenValue.SetBigInteger(bigInteger);
			return Tokens.BigInteger;
		}

		private Tokens ReadDouble(int numberStartIndex)
		{
			NumericCharKind numericCharKind = NumericCharKind.None;
			while (true)
			{
				int num = Peek();
				if (IsDecimalDigit(num))
				{
					numericCharKind = NumericCharKind.Digit;
					Skip(num);
					continue;
				}
				int sign;
				if ((num == 101 || num == 69) && TryReadExponentSign(1, out sign))
				{
					return ReadDoubleExponent(numberStartIndex, sign);
				}
				if (numericCharKind == NumericCharKind.Underscore)
				{
					ReportError(Errors.TrailingUnderscoreInNumber);
					break;
				}
				if (num != 95)
				{
					break;
				}
				Skip(num);
				numericCharKind = NumericCharKind.Underscore;
			}
			return DecodeDouble(numberStartIndex, _bufferPos);
		}

		private Tokens ReadDoubleExponent(int numberStartIndex, int sign)
		{
			int num = 0;
			NumericCharKind numericCharKind = NumericCharKind.None;
			while (true)
			{
				int num2 = Peek();
				if (IsDecimalDigit(num2))
				{
					Skip(num2);
					numericCharKind = NumericCharKind.Digit;
					if (num < 10000)
					{
						num = num * 10 + (num2 - 48);
					}
					continue;
				}
				if (numericCharKind != NumericCharKind.Digit)
				{
					ReportError(Errors.TrailingUnderscoreInNumber);
					break;
				}
				if (num2 != 95)
				{
					break;
				}
				Skip(num2);
				numericCharKind = NumericCharKind.Underscore;
			}
			num *= sign;
			if (num <= -1021 || num >= 1025)
			{
				int num3 = _currentTokenStart.Column - 1;
				ReportWarning(Errors.FloatOutOfRange, new string(_lineBuffer, num3, _bufferPos - num3).Replace("_", ""));
			}
			return DecodeDouble(numberStartIndex, _bufferPos);
		}

		private static bool TryDecodeDouble(char[] str, int first, int end, out double result)
		{
			StringBuilder stringBuilder = new StringBuilder(end - first);
			stringBuilder.Length = end - first;
			int length = 0;
			for (int i = first; i < end; i++)
			{
				if (str[i] != '_')
				{
					stringBuilder[length++] = str[i];
				}
			}
			stringBuilder.Length = length;
			return double.TryParse(stringBuilder.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}

		private static bool TryDecodeDouble(string str, int first, int end, out double result)
		{
			StringBuilder stringBuilder = new StringBuilder(end - first);
			stringBuilder.Length = end - first;
			int length = 0;
			for (int i = first; i < end; i++)
			{
				if (str[i] != '_')
				{
					stringBuilder[length++] = str[i];
				}
			}
			stringBuilder.Length = length;
			return double.TryParse(stringBuilder.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}

		private Tokens DecodeDouble(int first, int end)
		{
			double result;
			if (!TryDecodeDouble(_lineBuffer, first, end, out result))
			{
				result = double.PositiveInfinity;
			}
			_tokenValue.SetDouble(result);
			return Tokens.Float;
		}

		public static bool IsDecimalDigit(int c)
		{
			return (uint)(c - 48) <= 9u;
		}

		public static bool IsOctalDigit(int c)
		{
			return (uint)(c - 48) <= 7u;
		}

		public static bool IsHexadecimalDigit(int c)
		{
			if (!IsDecimalDigit(c) && (uint)(c - 97) > 5u)
			{
				return (uint)(c - 65) <= 5u;
			}
			return true;
		}

		public static int ToDigit(int c)
		{
			if (IsDecimalDigit(c))
			{
				return c - 48;
			}
			if (IsLowerLetter(c))
			{
				return c - 97 + 10;
			}
			if (IsUpperLetter(c))
			{
				return c - 65 + 10;
			}
			return int.MaxValue;
		}

		private bool IsIdentifier(int c)
		{
			return IsIdentifier(c, _multiByteIdentifier);
		}

		public static bool IsIdentifier(int c, int multiByteIdentifier)
		{
			if (!IsIdentifierInitial(c, multiByteIdentifier))
			{
				return IsDecimalDigit(c);
			}
			return true;
		}

		private bool IsIdentifierInitial(int c)
		{
			return IsIdentifierInitial(c, _multiByteIdentifier);
		}

		public static bool IsIdentifierInitial(int c, int multiByteIdentifier)
		{
			if (!IsLetter(c) && c != 95)
			{
				return c > multiByteIdentifier;
			}
			return true;
		}

		public static bool IsLetter(int c)
		{
			if (!IsUpperLetter(c))
			{
				return IsLowerLetter(c);
			}
			return true;
		}

		public static bool IsLetterOrDigit(int c)
		{
			if (!IsLetter(c))
			{
				return IsDecimalDigit(c);
			}
			return true;
		}

		public static bool IsUpperLetter(int c)
		{
			return (uint)(c - 65) <= 25u;
		}

		public static bool IsLowerLetter(int c)
		{
			return (uint)(c - 97) <= 25u;
		}

		public static bool IsWhiteSpace(int c)
		{
			return IsAsciiWhiteSpace(c);
		}

		public static bool IsAsciiWhiteSpace(int c)
		{
			if ((uint)(c - 9) > 4u)
			{
				return c == 32;
			}
			return true;
		}

		private static bool IsMethodNameSuffix(int c, int multiByteIdentifier)
		{
			if (!IsIdentifier(c, multiByteIdentifier) && c != 33 && c != 63)
			{
				return c == 61;
			}
			return true;
		}

		private void SkipVariableName()
		{
			while (true)
			{
				int c = Peek();
				if (IsIdentifier(c))
				{
					Skip();
					continue;
				}
				break;
			}
		}

		private bool SkipWhitespace()
		{
			bool result = false;
			while (true)
			{
				RefillBuffer();
				int num = Peek();
				if (num == 10)
				{
					result = true;
					Skip();
					continue;
				}
				if (!IsWhiteSpace(num))
				{
					break;
				}
				Skip();
			}
			return result;
		}

		private static int NextChar(string str, ref int i)
		{
			if (i != str.Length)
			{
				return str[i++];
			}
			return -1;
		}

		public static IntegerValue ParseInteger(string str, int @base)
		{
			int i = 0;
			return ParseInteger(str, @base, ref i);
		}

		public static IntegerValue ParseInteger(string str, int @base, ref int i)
		{
			ContractUtils.RequiresNotNull(str, "str");
			int num;
			do
			{
				num = NextChar(str, ref i);
			}
			while (IsWhiteSpace(num));
			int num2;
			switch (num)
			{
			case 43:
				num2 = 1;
				num = NextChar(str, ref i);
				break;
			case 45:
				num2 = -1;
				num = NextChar(str, ref i);
				break;
			default:
				num2 = 1;
				break;
			}
			if (num == 48)
			{
				num = NextChar(str, ref i);
				int num3 = 0;
				switch (num)
				{
				case 88:
				case 120:
					num3 = 16;
					break;
				case 66:
				case 98:
					num3 = 2;
					break;
				case 68:
				case 100:
					num3 = 10;
					break;
				case 79:
				case 111:
					num3 = 8;
					break;
				}
				if (num3 != 0)
				{
					if (@base == 0 || num3 == @base)
					{
						@base = num3;
						num = NextChar(str, ref i);
					}
				}
				else if (@base == 0)
				{
					@base = 8;
				}
			}
			else if (@base == 0)
			{
				@base = 10;
			}
			bool flag = false;
			long num4 = 0L;
			int num5 = 0;
			int position = i - 1;
			while (true)
			{
				if (num != 95)
				{
					int num6 = ToDigit(num);
					if (num6 >= @base)
					{
						break;
					}
					if (num4 <= int.MaxValue)
					{
						num4 = num4 * @base + num6;
					}
					num5++;
					flag = true;
				}
				else
				{
					if (!flag)
					{
						break;
					}
					flag = false;
				}
				num = NextChar(str, ref i);
			}
			if (num5 == 0)
			{
				return 0;
			}
			if (num4 <= int.MaxValue)
			{
				num4 *= num2;
				if (num4 >= int.MinValue && num4 <= int.MaxValue)
				{
					return (int)num4;
				}
				return BigInteger.Create(num4);
			}
			BignumParser bignumParser = new BignumParser();
			bignumParser.Position = position;
			bignumParser.Buffer = str.ToCharArray();
			return bignumParser.Parse(num5, @base) * num2;
		}

		private static int Read(string str, ref int i)
		{
			i++;
			if (i >= str.Length)
			{
				return -1;
			}
			return str[i];
		}

		public static bool TryParseDouble(string str, out double result, out bool complete)
		{
			int i = -1;
			int num;
			do
			{
				num = Read(str, ref i);
			}
			while (IsWhiteSpace(num));
			double num2;
			switch (num)
			{
			case 45:
				num = Read(str, ref i);
				if (num == 95)
				{
					result = 0.0;
					complete = false;
					return false;
				}
				num2 = -1.0;
				break;
			case 43:
				num = Read(str, ref i);
				if (num == 95)
				{
					result = 0.0;
					complete = false;
					return false;
				}
				num2 = 1.0;
				break;
			default:
				num2 = 1.0;
				break;
			}
			int first = i;
			while (num == 95 || IsDecimalDigit(num))
			{
				num = Read(str, ref i);
			}
			if (num == 46)
			{
				num = Read(str, ref i);
				while (num == 95 || IsDecimalDigit(num))
				{
					num = Read(str, ref i);
				}
			}
			int num3 = i;
			if (num == 101 || num == 69)
			{
				num = Read(str, ref i);
				if (num == 43 || num == 45)
				{
					num = Read(str, ref i);
				}
				int num4 = num3;
				while (true)
				{
					if (IsDecimalDigit(num))
					{
						num4 = i + 1;
					}
					else if (num != 95)
					{
						break;
					}
					num = Read(str, ref i);
				}
				num3 = num4;
			}
			bool result2 = TryDecodeDouble(str, first, num3, out result);
			result *= num2;
			complete = num3 == str.Length;
			return result2;
		}

		internal static bool TryParseEncodingHeader(TextReader reader, out string encodingName)
		{
			encodingName = null;
			if (reader.Peek() != 35)
			{
				return false;
			}
			string text = reader.ReadLine();
			if (text.Length > 1 && text[1] == '!')
			{
				if (reader.Peek() != 35)
				{
					return false;
				}
				text = reader.ReadLine();
			}
			Regex regex = new Regex("^[#].*?coding\\s*[:=]\\s*(?<encoding>[a-z0-9_-]+)", RegexOptions.IgnoreCase);
			Match match = regex.Match(text);
			if (match.Success)
			{
				encodingName = match.Groups["encoding"].Value;
				return encodingName.Length > 0;
			}
			return false;
		}

		public static bool IsConstantName(string name)
		{
			if (!string.IsNullOrEmpty(name) && IsUpperLetter(name[0]) && IsVariableName(name, 1, 1, 127))
			{
				return IsIdentifier(name[name.Length - 1], 127);
			}
			return false;
		}

		public static bool IsVariableName(string name)
		{
			if (!string.IsNullOrEmpty(name) && IsIdentifierInitial(name[0], 127))
			{
				return IsVariableName(name, 1, 0, 127);
			}
			return false;
		}

		public static bool IsMethodName(string name)
		{
			if (!string.IsNullOrEmpty(name) && IsIdentifierInitial(name[0], 127) && IsVariableName(name, 1, 1, 127))
			{
				return IsMethodNameSuffix(name[name.Length - 1], 127);
			}
			return false;
		}

		public static bool IsInstanceVariableName(string name)
		{
			if (name != null && name.Length >= 2 && name[0] == '@')
			{
				return IsVariableName(name, 1, 0, 127);
			}
			return false;
		}

		public static bool IsClassVariableName(string name)
		{
			if (name != null && name.Length >= 3 && name[0] == '@' && name[1] == '@')
			{
				return IsVariableName(name, 2, 0, 127);
			}
			return false;
		}

		public static bool IsGlobalVariableName(string name)
		{
			if (name != null && name.Length >= 2 && name[0] == '$')
			{
				return IsVariableName(name, 1, 0, 127);
			}
			return false;
		}

		private static bool IsVariableName(string name, int trimStart, int trimEnd, int multiByteIdentifier)
		{
			for (int i = trimStart; i < name.Length - trimEnd; i++)
			{
				if (!IsIdentifier(name[i], multiByteIdentifier))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsOperatorName(string name)
		{
			if (name.Length <= 3)
			{
				switch (name)
				{
				case "|":
				case "^":
				case "&":
				case "<=>":
				case "==":
				case "===":
				case "=~":
				case ">":
				case ">=":
				case "<":
				case "<=":
				case "<<":
				case ">>":
				case "+":
				case "-":
				case "*":
				case "/":
				case "%":
				case "**":
				case "~":
				case "+@":
				case "-@":
				case "[]":
				case "[]=":
				case "`":
					return true;
				}
			}
			return false;
		}

		public override bool SkipToken()
		{
			return GetNextToken() != Tokens.EndOfFile;
		}

		public override TokenInfo ReadToken()
		{
			Tokens nextToken = GetNextToken();
			TokenInfo tokenInfo = GetTokenInfo(nextToken);
			tokenInfo.SourceSpan = TokenSpan;
			return tokenInfo;
		}

		internal static TokenInfo GetTokenInfo(Tokens token)
		{
			TokenInfo result = default(TokenInfo);
			switch (token)
			{
			case Tokens.Undef:
			case Tokens.Rescue:
			case Tokens.Ensure:
			case Tokens.If:
			case Tokens.Unless:
			case Tokens.Then:
			case Tokens.Elsif:
			case Tokens.Else:
			case Tokens.Case:
			case Tokens.When:
			case Tokens.While:
			case Tokens.Until:
			case Tokens.For:
			case Tokens.Break:
			case Tokens.Next:
			case Tokens.Redo:
			case Tokens.Retry:
			case Tokens.In:
			case Tokens.Return:
			case Tokens.Yield:
			case Tokens.Super:
			case Tokens.Self:
			case Tokens.Nil:
			case Tokens.True:
			case Tokens.False:
			case Tokens.And:
			case Tokens.Or:
			case Tokens.Not:
			case Tokens.IfMod:
			case Tokens.UnlessMod:
			case Tokens.WhileMod:
			case Tokens.UntilMod:
			case Tokens.RescueMod:
			case Tokens.Alias:
			case Tokens.Defined:
			case Tokens.Line:
			case Tokens.File:
			case Tokens.Encoding:
				result.Category = TokenCategory.Keyword;
				break;
			case Tokens.Class:
			case Tokens.Module:
			case Tokens.Def:
			case Tokens.Begin:
			case Tokens.End:
			case Tokens.Do:
			case Tokens.LoopDo:
			case Tokens.BlockDo:
			case Tokens.LambdaDo:
			case Tokens.UppercaseBegin:
			case Tokens.UppercaseEnd:
				result.Category = TokenCategory.Keyword;
				result.Trigger = TokenTriggers.MatchBraces;
				break;
			case Tokens.UnaryPlus:
			case Tokens.UnaryMinus:
			case Tokens.Pow:
			case Tokens.Cmp:
			case Tokens.Equal:
			case Tokens.StrictEqual:
			case Tokens.NotEqual:
			case Tokens.GreaterOrEqual:
			case Tokens.LessOrEqual:
			case Tokens.LogicalAnd:
			case Tokens.LogicalOr:
			case Tokens.Match:
			case Tokens.Nmatch:
			case Tokens.DoubleDot:
			case Tokens.TripleDot:
			case Tokens.ItemGetter:
			case Tokens.ItemSetter:
			case Tokens.Lshft:
			case Tokens.Rshft:
			case Tokens.DoubleArrow:
			case Tokens.Star:
			case Tokens.BlockReference:
			case Tokens.Ampersand:
			case Tokens.Backtick:
			case Tokens.Lambda:
			case Tokens.OpAssignment:
			case Tokens.Assignment:
			case Tokens.QuestionMark:
			case Tokens.Colon:
			case Tokens.Greater:
			case Tokens.Less:
			case Tokens.Caret:
			case Tokens.Plus:
			case Tokens.Minus:
			case Tokens.Asterisk:
			case Tokens.Slash:
			case Tokens.Percent:
			case Tokens.NumberNegation:
			case Tokens.Bang:
			case Tokens.Tilde:
				result.Category = TokenCategory.Operator;
				break;
			case Tokens.SeparatingDoubleColon:
			case Tokens.LeadingDoubleColon:
				result.Category = TokenCategory.Delimiter;
				result.Trigger = TokenTriggers.MemberSelect;
				break;
			case Tokens.LeftBrace:
			case Tokens.LeftBlockBrace:
			case Tokens.LeftBlockArgBrace:
			case Tokens.LeftLambdaBrace:
			case Tokens.RightBrace:
			case Tokens.LeftBracket:
			case Tokens.LeftIndexingBracket:
			case Tokens.RightBracket:
			case Tokens.StringEmbeddedCodeBegin:
			case Tokens.StringEmbeddedCodeEnd:
			case Tokens.Pipe:
				result.Category = TokenCategory.Grouping;
				result.Trigger = TokenTriggers.MatchBraces;
				break;
			case Tokens.LeftParenthesis:
			case Tokens.LeftArgParenthesis:
				result.Category = TokenCategory.Grouping;
				result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterStart;
				break;
			case Tokens.LeftExprParenthesis:
				result.Category = TokenCategory.Grouping;
				result.Trigger = TokenTriggers.MatchBraces;
				break;
			case Tokens.RightParenthesis:
				result.Category = TokenCategory.Grouping;
				result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd;
				break;
			case Tokens.Comma:
				result.Category = TokenCategory.Delimiter;
				result.Trigger = TokenTriggers.ParameterNext;
				break;
			case Tokens.Dot:
				result.Category = TokenCategory.Delimiter;
				result.Trigger = TokenTriggers.MemberSelect;
				break;
			case Tokens.VerbatimHeredocBegin:
			case Tokens.VerbatimHeredocEnd:
			case Tokens.StringEnd:
			case Tokens.Character:
				result.Category = TokenCategory.StringLiteral;
				break;
			case Tokens.WordSeparator:
			case Tokens.Semicolon:
			case Tokens.StringEmbeddedVariableBegin:
				result.Category = TokenCategory.Delimiter;
				break;
			case Tokens.SymbolBegin:
			case Tokens.Identifier:
			case Tokens.FunctionIdentifier:
			case Tokens.GlobalVariable:
			case Tokens.InstanceVariable:
			case Tokens.ConstantIdentifier:
			case Tokens.ClassVariable:
			case Tokens.Label:
			case Tokens.MatchReference:
				result.Category = TokenCategory.Identifier;
				break;
			case Tokens.Integer:
			case Tokens.BigInteger:
			case Tokens.Float:
				result.Category = TokenCategory.NumericLiteral;
				break;
			case Tokens.StringBegin:
			case Tokens.RegexpBegin:
			case Tokens.ShellStringBegin:
			case Tokens.WordsBegin:
			case Tokens.VerbatimWordsBegin:
			case Tokens.StringContent:
			case Tokens.RegexpEnd:
				result.Category = TokenCategory.StringLiteral;
				break;
			case Tokens.Pound:
				result.Category = TokenCategory.LineComment;
				break;
			case Tokens.EndOfFile:
				result.Category = TokenCategory.EndOfStream;
				break;
			case Tokens.Whitespace:
			case Tokens.EndOfLine:
			case Tokens.NewLine:
				result.Category = TokenCategory.WhiteSpace;
				break;
			case Tokens.SingleLineComment:
				result.Category = TokenCategory.LineComment;
				break;
			case Tokens.MultiLineComment:
				result.Category = TokenCategory.Comment;
				break;
			case Tokens.Error:
			case Tokens.InvalidCharacter:
			case Tokens.At:
			case Tokens.Dollar:
			case Tokens.Backslash:
				result.Category = TokenCategory.Error;
				break;
			default:
				throw Assert.Unreachable;
			}
			return result;
		}

		internal static string GetTokenDescription(Tokens token)
		{
			switch (token)
			{
			case Tokens.Undef:
				return "`undef'";
			case Tokens.Rescue:
			case Tokens.RescueMod:
				return "`rescue'";
			case Tokens.Ensure:
				return "`ensure'";
			case Tokens.If:
			case Tokens.IfMod:
				return "`if'";
			case Tokens.Unless:
			case Tokens.UnlessMod:
				return "`unless'";
			case Tokens.Then:
				return "`then'";
			case Tokens.Elsif:
				return "`elsif'";
			case Tokens.Else:
				return "`else'";
			case Tokens.Case:
				return "`case'";
			case Tokens.When:
				return "`when'";
			case Tokens.While:
			case Tokens.WhileMod:
				return "`while'";
			case Tokens.Until:
			case Tokens.UntilMod:
				return "`until'";
			case Tokens.For:
				return "`for'";
			case Tokens.Break:
				return "`break'";
			case Tokens.Next:
				return "`next'";
			case Tokens.Redo:
				return "`redo'";
			case Tokens.Retry:
				return "`retry'";
			case Tokens.In:
				return "`in'";
			case Tokens.Return:
				return "`return'";
			case Tokens.Yield:
				return "`yield'";
			case Tokens.Super:
				return "`super'";
			case Tokens.Self:
				return "`self'";
			case Tokens.Nil:
				return "`nil'";
			case Tokens.True:
				return "`true'";
			case Tokens.False:
				return "`false'";
			case Tokens.And:
				return "`and'";
			case Tokens.Or:
				return "`or'";
			case Tokens.Not:
				return "`not'";
			case Tokens.Alias:
				return "`alias'";
			case Tokens.Defined:
				return "`defined'";
			case Tokens.Line:
				return "`__LINE__'";
			case Tokens.File:
				return "`__FILE__'";
			case Tokens.Encoding:
				return "`__ENCODING__'";
			case Tokens.Def:
				return "`def'";
			case Tokens.Class:
				return "`class'";
			case Tokens.Module:
				return "`module'";
			case Tokens.End:
				return "`end'";
			case Tokens.Begin:
				return "`begin'";
			case Tokens.UppercaseBegin:
				return "`BEGIN'";
			case Tokens.UppercaseEnd:
				return "`END'";
			case Tokens.Do:
			case Tokens.LoopDo:
			case Tokens.BlockDo:
			case Tokens.LambdaDo:
				return "`do'";
			case Tokens.Plus:
				return "`+'";
			case Tokens.UnaryPlus:
				return "`+@'";
			case Tokens.Minus:
				return "`-'";
			case Tokens.UnaryMinus:
				return "`-@'";
			case Tokens.Pow:
				return "`**'";
			case Tokens.Cmp:
				return "`<=>'";
			case Tokens.Equal:
				return "`=='";
			case Tokens.StrictEqual:
				return "`==='";
			case Tokens.NotEqual:
				return "`!='";
			case Tokens.Greater:
				return "`>'";
			case Tokens.GreaterOrEqual:
				return "`>='";
			case Tokens.Less:
				return "`<'";
			case Tokens.LessOrEqual:
				return "`<'";
			case Tokens.LogicalAnd:
				return "`&&'";
			case Tokens.LogicalOr:
				return "`||'";
			case Tokens.Match:
				return "`=~'";
			case Tokens.Nmatch:
				return "`!~'";
			case Tokens.DoubleDot:
				return "`..'";
			case Tokens.TripleDot:
				return "`...'";
			case Tokens.ItemGetter:
				return "`[]'";
			case Tokens.ItemSetter:
				return "`[]='";
			case Tokens.Lshft:
				return "`<<'";
			case Tokens.Rshft:
				return "`>>'";
			case Tokens.DoubleArrow:
				return "`=>'";
			case Tokens.Lambda:
				return "`->'";
			case Tokens.Star:
			case Tokens.Asterisk:
				return "`*'";
			case Tokens.BlockReference:
			case Tokens.Ampersand:
				return "`&'";
			case Tokens.Percent:
				return "`%'";
			case Tokens.Assignment:
				return "`='";
			case Tokens.Caret:
				return "`^'";
			case Tokens.Colon:
				return "`:'";
			case Tokens.QuestionMark:
				return "`?'";
			case Tokens.Bang:
				return "`!'";
			case Tokens.Slash:
				return "`/'";
			case Tokens.Tilde:
				return "`~'";
			case Tokens.Backtick:
				return "`";
			case Tokens.SeparatingDoubleColon:
			case Tokens.LeadingDoubleColon:
				return "`::'";
			case Tokens.LeftBracket:
			case Tokens.LeftIndexingBracket:
				return "`['";
			case Tokens.RightBracket:
				return "`]'";
			case Tokens.LeftBrace:
			case Tokens.LeftBlockBrace:
			case Tokens.LeftBlockArgBrace:
			case Tokens.LeftLambdaBrace:
				return "`{'";
			case Tokens.RightBrace:
				return "`}'";
			case Tokens.Pipe:
				return "`|'";
			case Tokens.LeftParenthesis:
			case Tokens.LeftExprParenthesis:
			case Tokens.LeftArgParenthesis:
				return "`('";
			case Tokens.RightParenthesis:
				return "`)'";
			case Tokens.Comma:
				return "`,'";
			case Tokens.Dot:
				return "`.'";
			case Tokens.Semicolon:
				return "`;'";
			case Tokens.Character:
				return "character escape (?...)";
			case Tokens.NumberNegation:
				return "negative number";
			case Tokens.OpAssignment:
				return "assignment with operation";
			case Tokens.StringEmbeddedVariableBegin:
				return "`#@' or `#$'";
			case Tokens.StringEmbeddedCodeBegin:
				return "`#{'";
			case Tokens.StringEmbeddedCodeEnd:
				return "`}'";
			case Tokens.Label:
				return "label";
			case Tokens.Identifier:
				return "identifier";
			case Tokens.FunctionIdentifier:
				return "function name";
			case Tokens.GlobalVariable:
				return "global variable";
			case Tokens.InstanceVariable:
				return "instance variable";
			case Tokens.ConstantIdentifier:
				return "constant";
			case Tokens.ClassVariable:
				return "class variable";
			case Tokens.MatchReference:
				return "$&, $`, $', $+, or $1-9";
			case Tokens.Integer:
				return "integer";
			case Tokens.BigInteger:
				return "big integer";
			case Tokens.Float:
				return "float";
			case Tokens.StringBegin:
				return "quote";
			case Tokens.VerbatimHeredocBegin:
				return "heredoc start";
			case Tokens.StringContent:
				return "string content";
			case Tokens.StringEnd:
				return "string terminator";
			case Tokens.VerbatimHeredocEnd:
				return "heredoc terminator";
			case Tokens.ShellStringBegin:
				return "shell command (`...`)";
			case Tokens.SymbolBegin:
				return "symbol";
			case Tokens.WordsBegin:
				return "`%W'";
			case Tokens.VerbatimWordsBegin:
				return "`%w'";
			case Tokens.WordSeparator:
				return "word separator";
			case Tokens.RegexpBegin:
				return "regex start";
			case Tokens.RegexpEnd:
				return "regex end";
			case Tokens.Pound:
				return "`#'";
			case Tokens.EndOfFile:
				return "end of file";
			case Tokens.EndOfLine:
			case Tokens.NewLine:
				return "end of line";
			case Tokens.Whitespace:
				return "space";
			case Tokens.SingleLineComment:
				return "# comment";
			case Tokens.MultiLineComment:
				return "=begin ... =end";
			case Tokens.Error:
				return "error";
			case Tokens.Backslash:
				return "`\\'";
			case Tokens.At:
				return "`@'";
			case Tokens.Dollar:
				return "`$'";
			case Tokens.InvalidCharacter:
				return "invalid character";
			default:
				throw Assert.Unreachable;
			}
		}
	}
}
