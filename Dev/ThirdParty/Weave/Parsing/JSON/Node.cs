
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

namespace System.Parsing.JSON
{
    public abstract class JSONNode : ASTNode<Token, TokenKind, JSONNode>
    {
        public string EvaluatedValue;
        internal JSONNode TransientReference;

        public TokenPair Declaration;
        public TokenPair Identifier;

        public sealed override Token Start { get { return Declaration.Start; } }
        public sealed override Token End { get { return Declaration.End; } }

        public JSONNode()
        {
            TransientReference = this;
        }

        public JSONNode(Token token)
        {
            TransientReference = this;
            Declaration = Identifier = token;
        }

        internal virtual JSONNode GetLeftMostNode()
        {
            return this;
        }

        internal virtual JSONNode GetRightMostNode()
        {
            return this;
        }

        public virtual JSONNode GetNodeAt(int sourceIndex)
        {
            return this;
        }

        public virtual void CollectNodes(ICollection<JSONNode> nodes)
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

    public abstract class CollectionNode : JSONNode
    {
        public JSONNode[] Items;

        public CollectionNode()
        {
        }

        public CollectionNode(Token token)
            : base(token)
        {
        }

        public override JSONNode GetNodeAt(int sourceIndex)
        {
            if (Items != null)
            {
                for (int i = 0; i < Items.Length; i++)
                    if (Items[i].Declaration.Contains(sourceIndex))
                        return Items[i].GetNodeAt(sourceIndex);
            }

            return this;
        }

        public override void CollectNodes(ICollection<JSONNode> nodes)
        {
            base.CollectNodes(nodes);

            if (Items != null)
                for (int i = 0; i < Items.Length; i++)
                    if (Items[0] != null)
                        Items[i].CollectNodes(nodes);
        }

        protected override void OnVisit(VisitPurpose purpose, AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder)
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

    public sealed class LiteralNode : JSONNode
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
}
