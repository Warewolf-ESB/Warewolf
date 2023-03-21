using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Scripting.Generation2
{
    internal class CompilerHelpers
    {
        private sealed class DebuggableCodeRewriter : ExpressionVisitor
        {
            private readonly Dictionary<object, FieldBuilder> _fields = new Dictionary<object, FieldBuilder>(ReferenceEqualityComparer<object>.Instance);

            private readonly TypeBuilder _type;

            private readonly HashSet<string> _methodNames = new HashSet<string>();

            internal DebuggableCodeRewriter(TypeBuilder type)
            {
                _type = type;
            }

            internal void InitializeFields(Type type)
            {
                foreach (KeyValuePair<object, FieldBuilder> field in _fields)
                {
                    type.GetField(field.Value.Name).SetValue(null, field.Key);
                }
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                if (_methodNames.Contains(node.Name))
                {
                    int count = _methodNames.Count;
                    string text;
                    do
                    {
                        text = node.Name + "$" + count++;
                    }
                    while (_methodNames.Contains(text));
                    _methodNames.Add(text);
                    return Expression.Lambda<T>(base.Visit(node.Body), text, node.TailCall, node.Parameters);
                }
                _methodNames.Add(node.Name);
                return base.VisitLambda(node);
            }

            protected override Expression VisitExtension(Expression node)
            {
                if (node is LightDynamicExpression lightDynamicExpression)
                {
                    return Visit(lightDynamicExpression.Reduce());
                }
                return Visit(node.Reduce());
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (Microsoft.Scripting.Generation.CompilerHelpers.CanEmitConstant(node.Value, node.Type))
                {
                    return node;
                }
                if (!_fields.TryGetValue(node.Value, out var value))
                {
                    value = _type.DefineField("$constant" + _fields.Count, Microsoft.Scripting.Generation.CompilerHelpers.GetVisibleType(node.Value.GetType()), FieldAttributes.Public | FieldAttributes.Static);
                    _fields.Add(node.Value, value);
                }
                Expression expression = Expression.Field(null, value);
                if (expression.Type != node.Type)
                {
                    expression = Expression.Convert(expression, node.Type);
                }
                return expression;
            }

            protected override Expression VisitDynamic(DynamicExpression node)
            {
                return Visit(Microsoft.Scripting.Generation.CompilerHelpers.Reduce(node));
            }
        }

        public static Delegate CompileToMethod(LambdaExpression lambda, bool emitDebugSymbols)
        {
            string name = (string.IsNullOrEmpty(lambda.Name) ? Microsoft.Scripting.Generation.CompilerHelpers.GetUniqueMethodName() : lambda.Name);
            TypeBuilder typeBuilder = Snippets.Shared.DefineType(name, typeof(object), preserveName: false, emitDebugSymbols).TypeBuilder;
            DebuggableCodeRewriter debuggableCodeRewriter = new DebuggableCodeRewriter(typeBuilder);
            lambda = (LambdaExpression)debuggableCodeRewriter.Visit(lambda);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(name, Microsoft.Scripting.Generation.CompilerHelpers.PublicStatic);
            //lambda.CompileToMethod(methodBuilder, debugInfoGenerator);
            return lambda.Compile();
            //Type type = typeBuilder.CreateType();
            //debuggableCodeRewriter.InitializeFields(type);
            //return Delegate.CreateDelegate(lambda.Type, type.GetMethod(methodBuilder.Name));
        }

    }
}
