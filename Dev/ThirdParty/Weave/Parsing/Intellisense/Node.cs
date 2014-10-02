
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
    public abstract class Node : ASTNode<Token, TokenKind, Node>
    {
        public string EvaluatedValue;
        internal Node TransientReference;

        public TokenPair Declaration;
        public TokenPair Identifier;

        public sealed override Token Start { get { return Declaration.Start; } }
        public sealed override Token End { get { return Declaration.End; } }

        public Node()
        {
            TransientReference = this;
        }

        public Node(Token token)
        {
            TransientReference = this;
            Declaration = Identifier = token;
        }

        internal virtual Node GetLeftMostNode()
        {
            return this;
        }

        internal virtual Node GetRightMostNode()
        {
            return this;
        }

        public virtual Node GetNodeAt(int sourceIndex)
        {
            return this;
        }

        public virtual void CollectNodes(ICollection<Node> nodes)
        {
            nodes.Add(this);
        }

        public virtual string GetEvaluatedValue()
        {
            return EvaluatedValue;
        }

        public virtual string GetRepresentationForEvaluation()
        {
            return Declaration.Content;
        }

        public virtual void AppendText(StringBuilder text, int indent)
        {
            string index = new string('\t', indent);
            text.AppendLine(index.Substring(1) + GetType().Name);
            text.AppendLine(index + Identifier.Content);

            text.AppendLine();
        }
    }

    public interface INodeValueSource
    {
        string GetEvaluatedValue(Node node);
        string GetRepresentationForEvaluation(Node node);
    }

    public class IterationNode : Node
    {
        public INodeValueSource ValueSource;

        public IterationNode(Token token)
            : base(token)
        {
        }

        public override string GetEvaluatedValue()
        {
            if (ValueSource != null) return ValueSource.GetEvaluatedValue(this);
            return GetRepresentationForEvaluation();
        }

        public override string GetRepresentationForEvaluation()
        {
            if (ValueSource != null) return ValueSource.GetRepresentationForEvaluation(this);
            return Declaration.Content;
        }
    }

    public abstract class CollectionNode : Node
    {
        public Node[] Items;

        public CollectionNode()
        {
        }

        public CollectionNode(Token token)
            : base(token)
        {
        }

        public override Node GetNodeAt(int sourceIndex)
        {
            if (Items != null)
            {
                for (int i = 0; i < Items.Length; i++)
                    if (Items[i].Declaration.Contains(sourceIndex))
                        return Items[i].GetNodeAt(sourceIndex);
            }

            return this;
        }

        public override void CollectNodes(ICollection<Node> nodes)
        {
            base.CollectNodes(nodes);

            if (Items != null)
                for (int i = 0; i < Items.Length; i++)
                    if (Items[0] != null)
                        Items[i].CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            base.OnVisit(purpose, builder);

            if (purpose == VisitPurpose.BuildNodeDeclarations)
            {
                Items = builder.BuildNodes(this, Declaration.Start.Next, Declaration.End.Previous);
            }
        }

        public override void AppendText(StringBuilder text, int indent)
        {
            base.AppendText(text, indent);

            if (Items != null && Items.Length > 0)
            {
                indent++;
                for (int i = 0; i < Items.Length; i++) Items[i].AppendText(text, indent);
            }
        }
    }

    public sealed class LiteralNode : Node
    {
        public TokenClassification Kind { get { return Declaration.Start == null ? TokenClassification.Invalid : Declaration.Start.Definition.Kind; } }

        public override bool IsTerminal { get { return true; } }

        public LiteralNode()
        {
        }

        public LiteralNode(Token token)
            : base(token)
        {
        }

        public override string GetEvaluatedValue()
        {
            return GetRepresentationForEvaluation();
        }

        public override string GetRepresentationForEvaluation()
        {
            if (Declaration.Start.Definition.IsStringLiteral)
            {
                int index = Declaration.Start.SourceIndex + 1;
                int innerLength = Declaration.Start.SourceLength - 2;

                if (Declaration.Start.Definition == TokenKind.VerbatimString)
                {
                    index++;
                    innerLength--;

                    return "\"" + StringUtility.GetUnescapedVerbatimString(Declaration.Start.Source.Substring(index, innerLength)) + "\"";
                }
                

                return "\"" + StringUtility.GetUnescapedRegularString(Declaration.Start.Source.Substring(index, innerLength)) + "\"";
            }

            return Declaration.Content;
        }
    }

    public class DatalistReferenceNode : Node
    {
        public DatalistReferenceNode()
        {
        }

        public DatalistReferenceNode(Token token)
            : base(token)
        {
        }
    }

    public class DatalistNestedReferenceNode : DatalistReferenceNode
    {
        public Node NestedIdentifier;

        public DatalistNestedReferenceNode()
        {
        }

        public DatalistNestedReferenceNode(Token token)
            : base(token)
        {
        }

        public override string GetRepresentationForEvaluation()
        {
            return TokenKind.OpenDL.Identifier + NestedIdentifier.GetEvaluatedValue() + TokenKind.CloseDL.Identifier;
        }

        public override Node GetNodeAt(int sourceIndex)
        {
            if (NestedIdentifier != null)
                if (NestedIdentifier.Declaration.Contains(sourceIndex))
                    return NestedIdentifier.GetNodeAt(sourceIndex);

            return this;
        }

        public override void CollectNodes(ICollection<Node> nodes)
        {
            base.CollectNodes(nodes);
            if (NestedIdentifier != null) NestedIdentifier.CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            base.OnVisit(purpose, builder);

            if (purpose == VisitPurpose.BuildNodeDeclarations)
            {
                if (NestedIdentifier != null) NestedIdentifier.Visit(purpose, builder);
            }
        }

        public override void AppendText(StringBuilder text, int indent)
        {
            base.AppendText(text, indent);

            if (NestedIdentifier != null)
            {
                NestedIdentifier.AppendText(text, ++indent);
            }
        }
    }

    public class DatalistRecordSetNode : DatalistReferenceNode
    {
        public Node NestedIdentifier;
        public ParenthesisGroupNode Parameter;

        public DatalistRecordSetNode()
        {
        }

        public DatalistRecordSetNode(Token token)
            : base(token)
        {
        }



        public override string GetRepresentationForEvaluation()
        {
            if (NestedIdentifier != null)
                return TokenKind.OpenDL.Identifier + NestedIdentifier.GetEvaluatedValue() + Parameter.GetRepresentationForEvaluation() + TokenKind.CloseDL.Identifier;
            else
                return TokenKind.OpenDL.Identifier + Identifier.Content + Parameter.GetRepresentationForEvaluation() + TokenKind.CloseDL.Identifier;
        }

        public override Node GetNodeAt(int sourceIndex)
        {
            if (NestedIdentifier != null)
                if (NestedIdentifier.Declaration.Contains(sourceIndex))
                    return NestedIdentifier.GetNodeAt(sourceIndex);

            if (Parameter != null)
                if (Parameter.Declaration.Contains(sourceIndex))
                    return Parameter.GetNodeAt(sourceIndex);

            return this;
        }

        public override void CollectNodes(ICollection<Node> nodes)
        {
            base.CollectNodes(nodes);
            if (NestedIdentifier != null) NestedIdentifier.CollectNodes(nodes);
            if (Parameter != null) Parameter.CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            base.OnVisit(purpose, builder);

            if (purpose == VisitPurpose.BuildNodeDeclarations)
            {
                if (NestedIdentifier != null) NestedIdentifier.Visit(purpose, builder);
                if (Parameter != null) Parameter.Visit(purpose, builder);
            }
        }

        public override void AppendText(StringBuilder text, int indent)
        {
            base.AppendText(text, indent);

            indent++;

            if (NestedIdentifier != null) NestedIdentifier.AppendText(text, indent);
            if (Parameter != null) Parameter.AppendText(text, indent);
        }
    }

    public class DatalistRecordSetFieldNode : DatalistReferenceNode
    {
        public DatalistRecordSetNode RecordSet;
        public Node Field;


        public DatalistRecordSetFieldNode()
        {
        }

        public DatalistRecordSetFieldNode(Token token)
            : base(token)
        {
        }

        public override string GetRepresentationForEvaluation()
        {
            if (Field != null)
            {
                if (RecordSet.NestedIdentifier != null)
                    return TokenKind.OpenDL.Identifier + RecordSet.NestedIdentifier.GetEvaluatedValue() + RecordSet.Parameter.GetRepresentationForEvaluation() + TokenKind.FullStop.Identifier + Field.GetEvaluatedValue() + TokenKind.CloseDL.Identifier;
                else
                    return TokenKind.OpenDL.Identifier + RecordSet.Identifier.Content + RecordSet.Parameter.GetRepresentationForEvaluation() + TokenKind.FullStop.Identifier + Field.GetEvaluatedValue() + TokenKind.CloseDL.Identifier;
            }
            else
            {
                if (RecordSet.NestedIdentifier != null)
                    return TokenKind.OpenDL.Identifier + RecordSet.NestedIdentifier.GetEvaluatedValue() + RecordSet.Parameter.GetRepresentationForEvaluation() + TokenKind.FullStop.Identifier + Identifier.Content + TokenKind.CloseDL.Identifier;
                else
                    return TokenKind.OpenDL.Identifier + RecordSet.Identifier.Content + RecordSet.Parameter.GetRepresentationForEvaluation() + TokenKind.FullStop.Identifier + Identifier.Content + TokenKind.CloseDL.Identifier;
            }
        }

        public override Node GetNodeAt(int sourceIndex)
        {
            if (RecordSet != null)
                if (RecordSet.Declaration.Contains(sourceIndex))
                    return RecordSet.GetNodeAt(sourceIndex);

            if (Field != null)
                if (Field.Declaration.Contains(sourceIndex))
                    return Field.GetNodeAt(sourceIndex);

            return this;
        }

        public override void CollectNodes(ICollection<Node> nodes)
        {
            base.CollectNodes(nodes);
            if (RecordSet != null) RecordSet.CollectNodes(nodes);
            if (Field != null) Field.CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
        {
            base.OnVisit(purpose, builder);

            if (purpose == VisitPurpose.BuildNodeDeclarations)
            {
                if (RecordSet != null) RecordSet.Visit(purpose, builder);
                if (Field != null) Field.Visit(purpose, builder);
            }
        }

        public override void AppendText(StringBuilder text, int indent)
        {
            base.AppendText(text, indent);

            indent++;

            if (RecordSet != null) RecordSet.AppendText(text, indent);
            if (Field != null) Field.AppendText(text, indent);

        }
    }
}
