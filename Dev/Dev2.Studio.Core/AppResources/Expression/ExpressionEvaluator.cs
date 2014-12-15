
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
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Expression
{
    public static class ExpressionEvaluator
    {
        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static System.Linq.Expressions.Expression PartialEval(System.Linq.Expressions.Expression expression, Func<System.Linq.Expressions.Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static System.Linq.Expressions.Expression PartialEval(System.Linq.Expressions.Expression expression)
        {
            return PartialEval(expression, CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(System.Linq.Expressions.Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        class SubtreeEvaluator : ExpressionVisitor
        {
            readonly HashSet<System.Linq.Expressions.Expression> _candidates;

            internal SubtreeEvaluator(HashSet<System.Linq.Expressions.Expression> candidates)
            {
                _candidates = candidates;
            }

            internal System.Linq.Expressions.Expression Eval(System.Linq.Expressions.Expression exp)
            {
                return Visit(exp);
            }

            public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression exp)
            {
                if(exp == null)
                {
                    return null;
                }
                if(_candidates.Contains(exp))
                {
                    return Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private System.Linq.Expressions.Expression Evaluate(System.Linq.Expressions.Expression e)
            {
                if(e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }
                LambdaExpression lambda = System.Linq.Expressions.Expression.Lambda(e);
                Delegate fn = lambda.Compile();
                return System.Linq.Expressions.Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        class Nominator : ExpressionVisitor
        {
            readonly Func<System.Linq.Expressions.Expression, bool> _fnCanBeEvaluated;
            HashSet<System.Linq.Expressions.Expression> _candidates;
            bool _cannotBeEvaluated;

            internal Nominator(Func<System.Linq.Expressions.Expression, bool> fnCanBeEvaluated)
            {
                _fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal HashSet<System.Linq.Expressions.Expression> Nominate(System.Linq.Expressions.Expression expression)
            {
                _candidates = new HashSet<System.Linq.Expressions.Expression>();
                Visit(expression);
                return _candidates;
            }

            public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression expression)
            {
                if(expression != null)
                {
                    bool saveCannotBeEvaluated = _cannotBeEvaluated;
                    _cannotBeEvaluated = false;
                    base.Visit(expression);
                    if(!_cannotBeEvaluated)
                    {
                        if(_fnCanBeEvaluated(expression))
                        {
                            _candidates.Add(expression);
                        }
                        else
                        {
                            _cannotBeEvaluated = true;
                        }
                    }
                    _cannotBeEvaluated |= saveCannotBeEvaluated;
                }
                return expression;
            }
        }
    }
}
