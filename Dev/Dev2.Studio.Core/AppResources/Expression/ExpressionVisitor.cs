
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
using System.Collections.ObjectModel;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Expression
{
    public abstract class ExpressionVisitor
    {
        public virtual System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression exp)
        {
            if(exp == null)
                return null;
            switch(exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch(binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<System.Linq.Expressions.Expression> arguments = VisitExpressionList(initializer.Arguments);
            if(arguments != initializer.Arguments)
            {
                return System.Linq.Expressions.Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        protected virtual System.Linq.Expressions.Expression VisitUnary(UnaryExpression u)
        {
            System.Linq.Expressions.Expression operand = Visit(u.Operand);
            if(operand != u.Operand)
            {
                return System.Linq.Expressions.Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }

        protected virtual System.Linq.Expressions.Expression VisitBinary(BinaryExpression b)
        {
            System.Linq.Expressions.Expression left = Visit(b.Left);
            System.Linq.Expressions.Expression right = Visit(b.Right);
            System.Linq.Expressions.Expression conversion = Visit(b.Conversion);
            if(left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if(b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    return System.Linq.Expressions.Expression.Coalesce(left, right, conversion as LambdaExpression);
                return System.Linq.Expressions.Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }
            return b;
        }

        protected virtual System.Linq.Expressions.Expression VisitTypeIs(TypeBinaryExpression b)
        {
            System.Linq.Expressions.Expression expr = Visit(b.Expression);
            if(expr != b.Expression)
            {
                return System.Linq.Expressions.Expression.TypeIs(expr, b.TypeOperand);
            }
            return b;
        }

        protected virtual System.Linq.Expressions.Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        protected virtual System.Linq.Expressions.Expression VisitConditional(ConditionalExpression c)
        {
            System.Linq.Expressions.Expression test = Visit(c.Test);
            System.Linq.Expressions.Expression ifTrue = Visit(c.IfTrue);
            System.Linq.Expressions.Expression ifFalse = Visit(c.IfFalse);
            if(test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return System.Linq.Expressions.Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }

        protected virtual System.Linq.Expressions.Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        protected virtual System.Linq.Expressions.Expression VisitMemberAccess(MemberExpression m)
        {
            System.Linq.Expressions.Expression exp = Visit(m.Expression);
            if(exp != m.Expression)
            {
                return System.Linq.Expressions.Expression.MakeMemberAccess(exp, m.Member);
            }
            return m;
        }

        protected virtual System.Linq.Expressions.Expression VisitMethodCall(MethodCallExpression m)
        {
            System.Linq.Expressions.Expression obj = Visit(m.Object);
            IEnumerable<System.Linq.Expressions.Expression> args = VisitExpressionList(m.Arguments);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(obj != m.Object || args != m.Arguments)
            {
                return System.Linq.Expressions.Expression.Call(obj, m.Method, args);
            }
            return m;
        }

        protected virtual ReadOnlyCollection<System.Linq.Expressions.Expression> VisitExpressionList(ReadOnlyCollection<System.Linq.Expressions.Expression> original)
        {
            List<System.Linq.Expressions.Expression> list = null;
            for(int i = 0, n = original.Count; i < n; i++)
            {
                System.Linq.Expressions.Expression p = Visit(original[i]);
                if(list != null)
                {
                    list.Add(p);
                }
                else if(p != original[i])
                {
                    list = new List<System.Linq.Expressions.Expression>(n);
                    for(int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            if(list != null)
            {
                return list.AsReadOnly();
            }
            return original;
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            System.Linq.Expressions.Expression e = Visit(assignment.Expression);
            if(e != assignment.Expression)
            {
                return System.Linq.Expressions.Expression.Bind(assignment.Member, e);
            }
            return assignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(bindings != binding.Bindings)
            {
                return System.Linq.Expressions.Expression.MemberBind(binding.Member, bindings);
            }
            return binding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(initializers != binding.Initializers)
            {
                return System.Linq.Expressions.Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }

        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for(int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(original[i]);
                if(list != null)
                {
                    list.Add(b);
                }
                else if(b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for(int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if(list != null)
                return list;
            return original;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for(int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(original[i]);
                if(list != null)
                {
                    list.Add(init);
                }
                else if(init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for(int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if(list != null)
                return list;
            return original;
        }

        protected virtual System.Linq.Expressions.Expression VisitLambda(LambdaExpression lambda)
        {
            System.Linq.Expressions.Expression body = Visit(lambda.Body);
            if(body != lambda.Body)
            {
                return System.Linq.Expressions.Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }

        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<System.Linq.Expressions.Expression> args = VisitExpressionList(nex.Arguments);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(args != nex.Arguments)
            {
                return System.Linq.Expressions.Expression.New(nex.Constructor, args, nex.Members);
            }
            return nex;
        }

        protected virtual System.Linq.Expressions.Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(n != init.NewExpression || bindings != init.Bindings)
            {
                return System.Linq.Expressions.Expression.MemberInit(n, bindings);
            }
            return init;
        }

        protected virtual System.Linq.Expressions.Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(init.Initializers);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(n != init.NewExpression || initializers != init.Initializers)
            {
                return System.Linq.Expressions.Expression.ListInit(n, initializers);
            }
            return init;
        }

        protected virtual System.Linq.Expressions.Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<System.Linq.Expressions.Expression> exprs = VisitExpressionList(na.Expressions);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(exprs != na.Expressions)
            {
                if(na.NodeType == ExpressionType.NewArrayInit)
                {
                    return System.Linq.Expressions.Expression.NewArrayInit(na.Type.GetElementType(), exprs);
                }
                return System.Linq.Expressions.Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
            }
            return na;
        }

        protected virtual System.Linq.Expressions.Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<System.Linq.Expressions.Expression> args = VisitExpressionList(iv.Arguments);
            System.Linq.Expressions.Expression expr = Visit(iv.Expression);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(args != iv.Arguments || expr != iv.Expression)
            {
                return System.Linq.Expressions.Expression.Invoke(expr, args);
            }
            return iv;
        }
    }

}
