
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
