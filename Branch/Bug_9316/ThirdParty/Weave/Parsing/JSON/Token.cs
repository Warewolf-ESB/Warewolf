using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Parsing.Tokenization;

namespace System.Parsing.JSON
{
    public sealed class Token : Token<Token, TokenKind>
    {
        public override string ToString()
        {
            if (Definition != null) return Definition.ToString() + " (( " + Content;
            return Content;
        }
    }
}
