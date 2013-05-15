using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Parsing.SyntaxAnalysis;

namespace System.Parsing.JSON
{
    public class SyntaxTreeBuilder : AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode>
    {
        public SyntaxTreeBuilder()
        {
            RegisterGrammer(new CharLiteralGrammer());
            RegisterGrammer(new StringLiteralGrammer());
            RegisterGrammer(new CSharpNumericLiteralGrammer());
            RegisterGrammer(new BooleanLiteralGrammer());
            RegisterGrammer(new JSONGrammer());
        }

        protected override Tokenization.Tokenizer<Token, TokenKind> CreateTokenizerInstance()
        {
            return new JSONTokenizer();
        }

        protected override void OnUnhandledTokenEncountered(JSONNode container, Token token)
        {
            if (token.Definition.IsWhitespace) return;
            //EventLog.Log(new ParseEventLogEntry(token.Source, token.ToParseEventLogToken(), null, 5, 0, "SyntaxTreeBuilder", "OnUnhandledTokenEncountered"));
        }
    }
}
