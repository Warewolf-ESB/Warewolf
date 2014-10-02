
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
    public class DatalistGrammer : AbstractSyntaxTreeGrammer<Token, TokenKind, Node>
    {
        public static readonly GrammerGroup DatalistGrammerGroup = new GrammerGroup("Datalist");

        public override GrammerGroup GrammerGroup { get { return DatalistGrammerGroup; } }

        protected internal override Node BuildNode(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Node container, Token start, Token last)
        {
            if (start == null) return Fail<Node>(RuntimeFailureReason.ArgumentNullException, "start");
            if (start.Definition == TokenKind.Iteration) return new IterationNode(start);

            if (start.Definition != TokenKind.OpenDL) return Fail<Node>(RuntimeFailureReason.ArgumentException, "start", "DataListGrammer BuildNode requests must always start on a TokenKind.OpenDL ([[) token.");
            TokenPair dStart = new TokenPair(start), identifier = new TokenPair();
            if ((start = start.Next) == null) return Fail<Node>( SyntaxFailureReason.ExpectedIdentifier, dStart.Start, TokenKind.Unknown);
            Node result = null;
            
            if (!start.Definition.IsUnknown)
            {
                if (start.Definition == TokenKind.OpenDL)
                {
                    Node nestedReference = BuildNode(builder, container, start, last);
                    if (nestedReference == null) return null;
                    if ((start = nestedReference.End.Next) == null) return Fail<Node>(SyntaxFailureReason.ExpectedToken, nestedReference.End, TokenKind.CloseDL, TokenKind.LeftParenthesis);
                    identifier = nestedReference.Declaration;
                    

                    if (start.Definition != TokenKind.CloseDL)
                    {
                        if (start.Definition == TokenKind.LeftParenthesis)
                        {
                            result = BuildDataListPostIdentifier(builder, start, last, dStart, identifier, result);
                            if (result == null) return null;
                            

                            if (result is DatalistRecordSetFieldNode)
                            {
                                DatalistRecordSetFieldNode fNode = (DatalistRecordSetFieldNode)result;
                                fNode.RecordSet.NestedIdentifier = nestedReference;
                                nestedReference = fNode;
                            }
                            else if (result is DatalistRecordSetNode)
                            {
                                DatalistRecordSetNode rNode = (DatalistRecordSetNode)result;
                                rNode.NestedIdentifier = nestedReference;
                                nestedReference = rNode;
                            }
                            //else System.Diagnostics.Debugger.Break();

                            start = result.Declaration.End;
                            dStart.End = start;
                        }
                        else return Fail<Node>(SyntaxFailureReason.ExpectedToken, start, TokenKind.CloseDL, TokenKind.LeftParenthesis);
                    }
                    else
                    {
                        dStart.End = start;
                        result = new DatalistNestedReferenceNode() { NestedIdentifier = nestedReference, Identifier = identifier, Declaration = dStart };
                    }
                }
                else return Fail<Node>(SyntaxFailureReason.ExpectedToken, start, TokenKind.Unknown, TokenKind.OpenDL);
            }
            else
            {
                identifier = start;
                if ((start = identifier.End.Next) == null) return Fail<Node>(SyntaxFailureReason.ExpectedExitScope, dStart.Start, TokenKind.CloseDL);
                result = BuildDataListPostIdentifier(builder, start, last, dStart, identifier, result);
            }

            return result;
        }

        private Node BuildDataListPostIdentifier(AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder, Token start, Token last, TokenPair dStart, TokenPair identifier, Node result)
        {
            if (start.Definition == TokenKind.LeftParenthesis)
            {
                TokenPair body = TokenUtility.BuildGroupSimple(start, TokenKind.RightParenthesis);
                if (body.End == null) return Fail<Node>(SyntaxFailureReason.ExpectedExitScope, start, TokenKind.RightParenthesis);
                DatalistRecordSetNode recordSet = new DatalistRecordSetNode();

                ParenthesisGroupNode rsParam = new ParenthesisGroupNode();
                rsParam.Identifier = identifier;
                rsParam.Declaration = body;
                recordSet.Parameter = rsParam;

                if (rsParam.Declaration.Start.NextNWS == rsParam.Declaration.End.PreviousNWS && rsParam.Declaration.Start.NextNWS.Definition == TokenKind.Asterisk)
                    rsParam.Declaration.Start.NextNWS.Definition = TokenKind.Iteration;

                if ((start = body.End.Next) == null) return Fail<Node>(SyntaxFailureReason.ExpectedToken, body.End, TokenKind.FullStop, TokenKind.CloseDL);

                if (start.Definition == TokenKind.FullStop)
                {
                    recordSet.Identifier = identifier;
                    recordSet.Declaration = new TokenPair(dStart.Start, body.End);
                    if (start.Next == null) return Fail<Node>(SyntaxFailureReason.ExpectedIdentifier, start, TokenKind.Unknown, TokenKind.OpenDL);

                    if (!(start = start.Next).Definition.IsUnknown)
                    {
                        if (start.Definition == TokenKind.OpenDL)
                        {
                            Node nestedFieldIdentifier = BuildNode(builder, recordSet, start, last);
                            if (nestedFieldIdentifier == null) return null;
                            DatalistRecordSetFieldNode rsField = new DatalistRecordSetFieldNode();
                            rsField.Identifier = nestedFieldIdentifier.Declaration;
                            rsField.RecordSet = recordSet;
                            rsField.Field = nestedFieldIdentifier;

                            if ((start = nestedFieldIdentifier.Declaration.End.Next) == null || start.Definition != TokenKind.CloseDL) return Fail<Node>(SyntaxFailureReason.ExpectedExitScope, recordSet.Declaration.Start, TokenKind.CloseDL);

                            dStart.End = start;
                            rsField.Declaration = dStart;
                            result = rsField;
                        }
                        else return Fail<Node>(SyntaxFailureReason.ExpectedToken, start, TokenKind.Unknown, TokenKind.OpenDL);
                    }
                    else
                    {
                        DatalistRecordSetFieldNode rsField = new DatalistRecordSetFieldNode();
                        rsField.Identifier = start;
                        rsField.RecordSet = recordSet;

                        if ((start = start.Next) == null || start.Definition != TokenKind.CloseDL) return Fail<Node>(SyntaxFailureReason.ExpectedExitScope, recordSet.Declaration.Start, TokenKind.CloseDL);
                        dStart.End = start;
                        rsField.Declaration = dStart;
                        result = rsField;
                    }
                }
                else if (start.Definition == TokenKind.CloseDL)
                {
                    dStart.End = start;
                    result = recordSet;
                    result.Declaration = dStart;
                    result.Identifier = identifier;
                }
                else return Fail<Node>(SyntaxFailureReason.UnexpectedToken, start, TokenKind.FullStop, TokenKind.CloseDL);
            }
            else if (start.Definition == TokenKind.CloseDL)
            {
                result = new DatalistReferenceNode();
                dStart.End = start;
                result.Declaration = dStart;
                result.Identifier = identifier;
            }
            else return Fail<Node>(SyntaxFailureReason.UnexpectedToken, start, TokenKind.LeftParenthesis, TokenKind.CloseDL);

            return result;
        }

        protected internal override void OnRegisterTriggers(ASTGrammerBehaviourRegistry triggerRegistry)
        {
            triggerRegistry.Register(TokenKind.OpenDL);
            triggerRegistry.Register(TokenKind.Iteration);
        }

        protected internal override void OnRegisterSubGrammers(HashSet<GrammerGroup> existingGrammers, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            if (!existingGrammers.Contains(GrammerGroup.StringLiteralGrammers)) builder.RegisterGrammer(new DatalistStringLiteralGrammer(this));
            if (!existingGrammers.Contains(GrammerGroup.NumericLiteralGrammers)) builder.RegisterGrammer(new DatalistNumericLiteralGrammer());
        }

        protected internal override void OnConfigureTokenizer(Tokenization.Tokenizer<Token, TokenKind> tokenizer)
        {
            base.OnConfigureTokenizer(tokenizer);

            List<TokenKind> unary = new List<TokenKind>();

            unary.Add(TokenKind.CloseDL);
            unary.Add(TokenKind.LeftParenthesis);
            unary.Add(TokenKind.RightParenthesis);
            unary.Add(TokenKind.FullStop);

            tokenizer.Handlers.Add(new Tokenization.UnaryTokenizationHandler<Token, TokenKind>(unary));
        }
    }
}
