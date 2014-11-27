
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
using System.Linq.Expressions;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Expression
{
    public class ExpressionVisitorDsfImpl : ExpressionVisitor
    {
        StringBuilder _sb;

        public string Translate(System.Linq.Expressions.Expression expression, string serviceName)
        {
            _sb = new StringBuilder();
            _sb.Append("<xml>");
            _sb.Append("<Service>");
            _sb.Append(serviceName);
            _sb.Append("</Service>");
            expression = ExpressionEvaluator.PartialEval(expression);
            Visit(expression);
            _sb.Append("/></xml>");
            return _sb.ToString();
        }

        protected override System.Linq.Expressions.Expression VisitConstant(ConstantExpression c)
        {
            switch(Type.GetTypeCode(c.Value.GetType()))
            {
                case TypeCode.Boolean:
                    _sb.Append(((bool)c.Value) ? "Value=\"True\"" : " Value=\"False\"");
                    break;
                case TypeCode.String:
                    _sb.Append(" Value=\"");
                    _sb.Append(c.Value);
                    _sb.Append("\"");
                    break;
                case TypeCode.Object:
                    throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                default:
                    _sb.Append(c.Value);
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

        protected override System.Linq.Expressions.Expression VisitBinary(BinaryExpression b)
        {
            Visit(b.Left);
            switch(b.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    _sb.Append(" NextOperand=\"And\"/>");
                    break;
                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    _sb.Append(" NextOperand=\"Or\"/>");
                    break;
                case ExpressionType.Equal:
                    _sb.Append(" Operand=\"Equal\"");
                    break;
                case ExpressionType.NotEqual:
                    _sb.Append(" Operand=\"NotEqual\"");
                    break;
                case ExpressionType.LessThan:
                    _sb.Append(" Operand=\"LessThan\"");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _sb.Append(" Operand=\"LessThanOrEqualTo\"");
                    break;
                case ExpressionType.GreaterThan:
                    _sb.Append(" Operand=\"\"GreaterThan\"");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _sb.Append(" Operand=\"GreaterThanOrEqualTo\"");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            Visit(b.Right);
            return b;
        }

        protected override System.Linq.Expressions.Expression VisitMemberAccess(MemberExpression m)
        {
            if(m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _sb.Append("<SearchCondition PropertyName=\"");
                _sb.Append(m.Member.Name);
                _sb.Append("\"");
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}
