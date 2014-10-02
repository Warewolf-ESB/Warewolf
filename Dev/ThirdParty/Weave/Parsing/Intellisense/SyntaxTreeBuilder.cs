
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
using System.Parsing.SyntaxAnalysis;

namespace System.Parsing.Intellisense
{
    public class SyntaxTreeBuilder : AbstractSyntaxTreeBuilder<Token, TokenKind, Node>
    {
        public SyntaxTreeBuilder()
        {
            RegisterGrammer(new InfrigisticNumericLiteralGrammer());
            RegisterGrammer(new BooleanLiteralGrammer());
            RegisterGrammer(new InfrigisticsGrammer());
            RegisterGrammer(new DatalistGrammer());
        }

        protected override Tokenization.Tokenizer<Token, TokenKind> CreateTokenizerInstance()
        {
            return new IntellisenseTokenizer();
        }

        protected override void OnUnhandledTokenEncountered(Node container, Token token)
        {
            if (container != null && container is CompositeStringLiteralNode) return;
            if (token.Definition.IsWhitespace) return;
            EventLog.Log(new ParseEventLogEntry(token.Source, token.ToParseEventLogToken(), null, 5, 0, "SyntaxTreeBuilder", "OnUnhandledTokenEncountered"));
        }
    }
}
