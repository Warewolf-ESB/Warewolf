// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.TokenKind
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public sealed class TokenKind : TokenDefinition
  {
    public static readonly List<TokenKind> Operators = new List<TokenKind>();
    public static readonly List<TokenKind> TypeLiterals = new List<TokenKind>();
    public static readonly TokenKind Unknown = new TokenKind(nameof (Unknown), "", TokenClassification.Unknown);
    public static readonly TokenKind Whitespace = new TokenKind(nameof (Whitespace), " ", TokenClassification.Whitespace);
    public static readonly TokenKind LineBreak = new TokenKind("Line Break", "\n", TokenClassification.Whitespace);
    public static readonly TokenKind EOF = new TokenKind("End Of File", "\n", TokenClassification.EndOfFile);
    public static readonly TokenKind OpenCData = TokenKind.CreateOperator("<![CDATA[", "Open CData Region");
    public static readonly TokenKind FullStop = TokenKind.CreateOperator(".", "Full Stop");
    public static readonly TokenKind Comma = TokenKind.CreateOperator(",", nameof (Comma));
    public static readonly TokenKind OpenDL = TokenKind.CreateOperator("[[", "Open Datalist Region", false);
    public static readonly TokenKind CloseDL = TokenKind.CreateOperator("]]", "Close Datalist Region", false);
    public static readonly TokenKind Plus = TokenKind.CreateOperator("+", nameof (Plus));
    public static readonly TokenKind Minus = TokenKind.CreateOperator("-", nameof (Minus));
    public static readonly TokenKind Mod = TokenKind.CreateOperator("%", nameof (Mod));
    public static readonly TokenKind Iteration = TokenKind.CreateOperator("", nameof (Iteration), false);
    public static readonly TokenKind Asterisk = TokenKind.CreateOperator("*", nameof (Asterisk));
    public static readonly TokenKind ForwardSlash = TokenKind.CreateOperator("/", "Forward Slash");
    public static readonly TokenKind LessThan = TokenKind.CreateOperator("<", "Less Than");
    public static readonly TokenKind GreaterThan = TokenKind.CreateOperator(">", "Greater Than");
    public static readonly TokenKind LessThanOrEqual = TokenKind.CreateOperator("<=", "Less Than Or Equal");
    public static readonly TokenKind GreaterThanOrEqual = TokenKind.CreateOperator(">=", "Greater Than Or Equal");
    public static readonly TokenKind Equality = TokenKind.CreateOperator("=", nameof (Equality));
    public static readonly TokenKind Inequality = TokenKind.CreateOperator("<>", nameof (Inequality));
    public static readonly TokenKind Ampersand = TokenKind.CreateOperator("&", nameof (Ampersand));
    public static readonly TokenKind Circumflex = TokenKind.CreateOperator("^", nameof (Circumflex));
    public static readonly TokenKind Colon = TokenKind.CreateOperator(":", nameof (Colon));
    public static readonly TokenKind LeftParenthesis = TokenKind.CreateOperator("(", "Left Parenthesis");
    public static readonly TokenKind RightParenthesis = TokenKind.CreateOperator(")", "Right Parenthesis");
    public static readonly TokenKind LeftCurlyBracket = TokenKind.CreateOperator("{", "Left Curly Bracket");
    public static readonly TokenKind RightCurlyBracket = TokenKind.CreateOperator("}", "Right Curly Bracket");
    public static readonly TokenKind RegularChar = TokenKind.CreateCharLiteral("", "Regular Character");
    public static readonly TokenKind HexadecimalChar = TokenKind.CreateCharLiteral("", "Hexadecimal Character");
    public static readonly TokenKind UnicodeChar = TokenKind.CreateCharLiteral("", "Unicode Character");
    public static readonly TokenKind EscapedChar = TokenKind.CreateCharLiteral("", "Escaped Character");
    public static readonly TokenKind CompositeStringClosure = TokenKind.CreateStringLiteral("", "Composite String Closure");
    public static readonly TokenKind CompositeRegularString = TokenKind.CreateStringLiteral("", "Composite Regular String");
    public static readonly TokenKind CompositeVerbatimString = TokenKind.CreateStringLiteral("", "Composite Verbatim String");
    public static readonly TokenKind RegularString = TokenKind.CreateStringLiteral("", "Regular String");
    public static readonly TokenKind VerbatimString = TokenKind.CreateStringLiteral("", "Verbatim String");
    public static readonly TokenKind IntegerNoSuffix = TokenKind.CreateIntegerLiteral("", "Integer No Suffix");
    public static readonly TokenKind IntegerHexadecimal = TokenKind.CreateIntegerLiteral("", "Integer Hexadecimal");
    public static readonly TokenKind IntegerSuffixU = TokenKind.CreateIntegerLiteral("", "Integer Unsigned");
    public static readonly TokenKind IntegerSuffixL = TokenKind.CreateIntegerLiteral("", "Integer Long");
    public static readonly TokenKind IntegerSuffixUL = TokenKind.CreateIntegerLiteral("", "Integer ULong");
    public static readonly TokenKind RealNoSuffix = TokenKind.CreateRealLiteral("", "Real No Suffix");
    public static readonly TokenKind RealSuffixF = TokenKind.CreateRealLiteral("", "Real Float");
    public static readonly TokenKind RealSuffixD = TokenKind.CreateRealLiteral("", "Real Double");
    public static readonly TokenKind RealSuffixPercent = TokenKind.CreateRealLiteral("", "Real Double Percent");
    public static readonly TokenKind BooleanFalse = TokenKind.CreateBooleanLiteral("false", "Boolean False");
    public static readonly TokenKind BooleanTrue = TokenKind.CreateBooleanLiteral("true", "Boolean True");
    public static readonly TokenKind Null = new TokenKind("Null Literal", "null", TokenClassification.NullLiteral);
    public static readonly HashSet<TokenKind> OperandTerminators = TokenKind.CreateOperandTerminators();
    public static readonly HashSet<TokenKind> BinaryOperators = TokenKind.CreateBinaryOperators();
    public static readonly HashSet<TokenKind> RelationalOperators = TokenKind.CreateRelationalOperators();
    public static readonly TokenKind[] Pairs = TokenKind.CreateTokenPairs();
    private TokenClassification _kind;
    private bool _isKeyword;

    public bool IsInvalid => this._kind == TokenClassification.Invalid;

    public override bool IsUnknown => this._kind == TokenClassification.Unknown;

    public override bool IsWhitespace => this._kind == TokenClassification.Whitespace;

    public override bool IsEndOfFile => this._kind == TokenClassification.EndOfFile;

    public override bool IsKeyword => this._isKeyword;

    public bool IsOperator => this._kind == TokenClassification.Operator;

    public bool IsStringLiteral => this._kind == TokenClassification.StringLiteral;

    public bool IsCharLiteral => this._kind == TokenClassification.CharLiteral;

    public bool IsIntegerLiteral => this._kind == TokenClassification.IntegerLiteral;

    public bool IsRealLiteral => this._kind == TokenClassification.RealLiteral;

    public bool IsBooleanLiteral => this._kind == TokenClassification.BooleanLiteral;

    public bool IsNullLiteral => this._kind == TokenClassification.NullLiteral;

    public bool IsTypeLiteral => this._kind == TokenClassification.TypeLiteral;

    public TokenClassification Kind => this._kind;

    private static HashSet<TokenKind> CreateOperandTerminators() => new HashSet<TokenKind>();

    private static HashSet<TokenKind> CreateBinaryOperators() => new HashSet<TokenKind>()
    {
      TokenKind.Plus,
      TokenKind.Minus,
      TokenKind.Asterisk,
      TokenKind.ForwardSlash,
      TokenKind.Ampersand,
      TokenKind.Circumflex,
      TokenKind.Equality,
      TokenKind.Inequality,
      TokenKind.GreaterThan,
      TokenKind.LessThan,
      TokenKind.GreaterThanOrEqual,
      TokenKind.LessThanOrEqual
    };

    private static HashSet<TokenKind> CreateRelationalOperators() => new HashSet<TokenKind>()
    {
      TokenKind.Equality,
      TokenKind.Inequality,
      TokenKind.GreaterThan,
      TokenKind.LessThan,
      TokenKind.GreaterThanOrEqual,
      TokenKind.LessThanOrEqual
    };

    private static TokenKind[] CreateTokenPairs()
    {
      TokenKind[] tokenPairs = new TokenKind[TokenDefinition.GetTotalDefinitionsOfType(typeof (TokenKind))];
      tokenPairs[TokenKind.LeftCurlyBracket._serial] = TokenKind.RightCurlyBracket;
      tokenPairs[TokenKind.LeftParenthesis._serial] = TokenKind.RightParenthesis;
      tokenPairs[TokenKind.RightCurlyBracket._serial] = TokenKind.LeftCurlyBracket;
      tokenPairs[TokenKind.RightParenthesis._serial] = TokenKind.LeftParenthesis;
      return tokenPairs;
    }

    private static TokenKind CreateOperator(string identifier, string name) => TokenKind.CreateOperator(identifier, name, true);

    private static TokenKind CreateOperator(string identifier, string name, bool add)
    {
      TokenKind tokenKind = new TokenKind(name, identifier, TokenClassification.Operator);
      if (add)
        TokenKind.Operators.Add(tokenKind);
      return tokenKind;
    }

    private static TokenKind CreateCharLiteral(string identifier, string name) => new TokenKind(name, identifier, TokenClassification.CharLiteral);

    private static TokenKind CreateStringLiteral(string identifier, string name) => new TokenKind(name, identifier, TokenClassification.StringLiteral);

    private static TokenKind CreateIntegerLiteral(string identifier, string name) => new TokenKind(name, identifier, TokenClassification.IntegerLiteral);

    private static TokenKind CreateRealLiteral(string identifier, string name) => new TokenKind(name, identifier, TokenClassification.RealLiteral);

    private static TokenKind CreateBooleanLiteral(string identifier, string name) => new TokenKind(name, identifier, TokenClassification.BooleanLiteral);

    private TokenKind(string name, string identifier, TokenClassification classification)
      : base(name, identifier)
    {
      this._kind = classification;
      if (this._kind != TokenClassification.Unknown)
        return;
      this._isKeyword = true;
    }

    private TokenKind(
      string name,
      string identifier,
      TokenClassification classification,
      bool isKeyword)
      : base(name, identifier)
    {
      this._kind = classification;
      this._isKeyword = isKeyword;
    }
  }
}
