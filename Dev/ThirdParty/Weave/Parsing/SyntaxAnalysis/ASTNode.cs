
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

namespace System.Parsing.SyntaxAnalysis
{
    public abstract class ASTNode<Token, TokenKind, ConcreteNode>
        where Token : Token<Token, TokenKind>, new()
        where TokenKind : TokenDefinition
        where ConcreteNode : ASTNode<Token, TokenKind, ConcreteNode>
    {
        public abstract Token Start { get; }
        public abstract Token End { get; }
        public virtual bool IsTerminal { get { return true; } }

        public void Visit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, ConcreteNode> builder)
        {
            //if (_visited) System.Diagnostics.Debugger.Break();
            OnVisit(purpose, builder);
        }

        protected virtual void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, ConcreteNode> builder)
        {
        }
    }
}
