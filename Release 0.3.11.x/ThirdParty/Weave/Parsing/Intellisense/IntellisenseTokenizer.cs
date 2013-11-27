using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
    public class IntellisenseTokenizer : Tokenizer<Token, TokenKind>
    {
        public IntellisenseTokenizer()
            : base(new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF), new TestParseEventLog())
        {

        }

        public IntellisenseTokenizer(ParseEventLog eventLog)
            : base(new RequiredTokenDefinitions<TokenKind>(TokenKind.Whitespace, TokenKind.LineBreak, TokenKind.Unknown, TokenKind.EOF), eventLog)
        {

        }
    }
}
