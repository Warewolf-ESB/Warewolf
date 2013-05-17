using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Parsing.Tokenization;

namespace System.Parsing.JSON
{
    public class JSONTokenizer : Tokenizer<Token, TokenKind>
    {
        public JSONTokenizer()
            : base(new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF), new TestParseEventLog())
        {

        }

        public JSONTokenizer(ParseEventLog eventLog)
            : base(new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF), eventLog)
        {

        }
    }
}
