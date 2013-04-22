using System;
using System.Text;
using System.Linq.Expressions;

namespace Dev2.Studio.Core.AppResources.Expression {
    public class ExpressionVisitorDSFImpl : ExpressionVisitor {
        StringBuilder sb;
        string currentMember = string.Empty;

        public ExpressionVisitorDSFImpl() {
        }

        public string Translate(System.Linq.Expressions.Expression expression, string serviceName) {
            sb = new StringBuilder();
            sb.Append("<xml>");
            sb.Append("<Service>");
            sb.Append(serviceName);
            sb.Append("</Service>");
            expression = ExpressionEvaluator.PartialEval(expression);
            this.Visit(expression);
            sb.Append("/></xml>");
            return sb.ToString();
        }

        protected override System.Linq.Expressions.Expression VisitConstant(ConstantExpression c) {
            switch (Type.GetTypeCode(c.Value.GetType())) {
                case TypeCode.Boolean:
                sb.Append(((bool)c.Value) ? "Value=\"True\"" : " Value=\"False\"");
                break;
                case TypeCode.String:
                sb.Append(" Value=\"");
                sb.Append(c.Value);
                sb.Append("\"");
                break;
                case TypeCode.Object:
                throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                default:
                sb.Append(c.Value);
                break;
            }

            

            //if (!string.IsNullOrEmpty(currentMember)) {
            //    sb.Append("</");
            //    sb.Append(currentMember);
            //    sb.Append(">");
            //    currentMember = string.Empty;
            //}               

            return c;
        }

        protected override System.Linq.Expressions.Expression VisitBinary(BinaryExpression b) {
            this.Visit(b.Left);
            switch (b.NodeType) {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    sb.Append(" NextOperand=\"And\"/>");
                break;
                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    sb.Append(" NextOperand=\"Or\"/>");
                break;
                case ExpressionType.Equal:
                    sb.Append(" Operand=\"Equal\"");
                break;
                case ExpressionType.NotEqual:
                    sb.Append(" Operand=\"NotEqual\"");
                break;
                case ExpressionType.LessThan:
                    sb.Append(" Operand=\"LessThan\"");
                break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" Operand=\"LessThanOrEqualTo\"");
                break;
                case ExpressionType.GreaterThan:
                    sb.Append(" Operand=\"\"GreaterThan\"");
                break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" Operand=\"GreaterThanOrEqualTo\"");
                break;
                default:
                throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            return b;
        }

        protected override System.Linq.Expressions.Expression VisitMemberAccess(MemberExpression m) {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter) {
                sb.Append("<SearchCondition PropertyName=\"");
                sb.Append(m.Member.Name);
                sb.Append("\"");
                currentMember = m.Member.Name;
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}
