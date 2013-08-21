using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dev2.Studio.Core.AppResources.Expression {
    public static class ExpressionEvaluator {
        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static System.Linq.Expressions.Expression PartialEval(System.Linq.Expressions.Expression expression, Func<System.Linq.Expressions.Expression, bool> fnCanBeEvaluated) {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static System.Linq.Expressions.Expression PartialEval(System.Linq.Expressions.Expression expression) {
            return PartialEval(expression, ExpressionEvaluator.CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(System.Linq.Expressions.Expression expression) {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        class SubtreeEvaluator : ExpressionVisitor {
            HashSet<System.Linq.Expressions.Expression> candidates;

            internal SubtreeEvaluator(HashSet<System.Linq.Expressions.Expression> candidates) {
                this.candidates = candidates;
            }

            internal System.Linq.Expressions.Expression Eval(System.Linq.Expressions.Expression exp) {
                return this.Visit(exp);
            }

            public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression exp) {
                if (exp == null) {
                    return null;
                }
                if (this.candidates.Contains(exp)) {
                    return this.Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private System.Linq.Expressions.Expression Evaluate(System.Linq.Expressions.Expression e) {
                if (e.NodeType == ExpressionType.Constant) {
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
        class Nominator : ExpressionVisitor {
            Func<System.Linq.Expressions.Expression, bool> fnCanBeEvaluated;
            HashSet<System.Linq.Expressions.Expression> candidates;
            bool cannotBeEvaluated;

            internal Nominator(Func<System.Linq.Expressions.Expression, bool> fnCanBeEvaluated) {
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal HashSet<System.Linq.Expressions.Expression> Nominate(System.Linq.Expressions.Expression expression) {
                this.candidates = new HashSet<System.Linq.Expressions.Expression>();
                this.Visit(expression);
                return this.candidates;
            }

            public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression expression) {
                if (expression != null) {
                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!this.cannotBeEvaluated) {
                        if (this.fnCanBeEvaluated(expression)) {
                            this.candidates.Add(expression);
                        }
                        else {
                            this.cannotBeEvaluated = true;
                        }
                    }
                    this.cannotBeEvaluated |= saveCannotBeEvaluated;
                }
                return expression;
            }
        }
    }
}
