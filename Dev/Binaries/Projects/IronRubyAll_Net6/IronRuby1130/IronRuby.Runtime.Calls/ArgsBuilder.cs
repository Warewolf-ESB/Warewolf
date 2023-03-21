using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using IronRuby.Compiler;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Calls
{
	public sealed class ArgsBuilder
	{
		private readonly Expression[] _arguments;

		private readonly int _implicitParamCount;

		private readonly int _mandatoryParamCount;

		private readonly int _leadingMandatoryParamCount;

		private readonly int _optionalParamCount;

		private readonly bool _hasUnsplatParameter;

		private int _actualArgumentCount;

		private CallArguments _callArguments;

		private int _listLength;

		private ParameterExpression _listVariable;

		public int ActualArgumentCount
		{
			get
			{
				return _actualArgumentCount;
			}
		}

		public bool HasTooFewArguments
		{
			get
			{
				return _actualArgumentCount < _mandatoryParamCount;
			}
		}

		public bool HasTooManyArguments
		{
			get
			{
				if (!_hasUnsplatParameter)
				{
					return _actualArgumentCount > _mandatoryParamCount + _optionalParamCount;
				}
				return false;
			}
		}

		public int LeadingMandatoryIndex
		{
			get
			{
				return _implicitParamCount;
			}
		}

		public int TrailingMandatoryIndex
		{
			get
			{
				return _implicitParamCount + _leadingMandatoryParamCount;
			}
		}

		public int OptionalParameterIndex
		{
			get
			{
				return _implicitParamCount + _mandatoryParamCount;
			}
		}

		public int UnsplatParameterIndex
		{
			get
			{
				return OptionalParameterIndex + _optionalParamCount;
			}
		}

		public int TrailingMandatoryCount
		{
			get
			{
				return _mandatoryParamCount - _leadingMandatoryParamCount;
			}
		}

		public Expression this[int index]
		{
			get
			{
				return _arguments[index];
			}
		}

		public ArgsBuilder(int implicitParamCount, int mandatoryParamCount, int leadingMandatoryParamCount, int optionalParamCount, bool hasUnsplatParameter)
		{
			_arguments = new Expression[implicitParamCount + mandatoryParamCount + optionalParamCount + (hasUnsplatParameter ? 1 : 0)];
			_implicitParamCount = implicitParamCount;
			_mandatoryParamCount = mandatoryParamCount;
			_leadingMandatoryParamCount = leadingMandatoryParamCount;
			_optionalParamCount = optionalParamCount;
			_hasUnsplatParameter = hasUnsplatParameter;
			_actualArgumentCount = -1;
		}

		internal Expression[] GetArguments()
		{
			return _arguments;
		}

		public void SetImplicit(int index, Expression arg)
		{
			_arguments[index] = arg;
		}

		private Expression GetArgument(int argIndex, out bool isSplatted)
		{
			if (argIndex < _callArguments.SimpleArgumentCount)
			{
				isSplatted = false;
				return _callArguments.GetSimpleArgumentExpression(argIndex);
			}
			int num = argIndex - _callArguments.SimpleArgumentCount;
			if (num < _listLength)
			{
				isSplatted = true;
				return Expression.Call(_listVariable, Methods.IList_get_Item, Microsoft.Scripting.Ast.Utils.Constant(num));
			}
			if (num == _listLength && _callArguments.Signature.HasRhsArgument)
			{
				isSplatted = false;
				return _callArguments.GetRhsArgumentExpression();
			}
			isSplatted = false;
			return null;
		}

		public void AddCallArguments(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			_callArguments = args;
			_actualArgumentCount = args.SimpleArgumentCount;
			if (args.Signature.HasSplattedArgument)
			{
				DynamicMetaObject splattedMetaArgument = args.GetSplattedMetaArgument();
				metaBuilder.AddSplattedArgumentTest((IList)splattedMetaArgument.Value, splattedMetaArgument.Expression, out _listLength, out _listVariable);
				_actualArgumentCount += _listLength;
			}
			if (args.Signature.HasRhsArgument)
			{
				_actualArgumentCount++;
			}
			if (HasTooFewArguments)
			{
				metaBuilder.SetWrongNumberOfArgumentsError(_actualArgumentCount, _mandatoryParamCount);
				return;
			}
			if (HasTooManyArguments)
			{
				metaBuilder.SetWrongNumberOfArgumentsError(_actualArgumentCount, _mandatoryParamCount);
				return;
			}
			bool isSplatted;
			for (int i = 0; i < _leadingMandatoryParamCount; i++)
			{
				_arguments[LeadingMandatoryIndex + i] = GetArgument(i, out isSplatted);
			}
			for (int j = 0; j < TrailingMandatoryCount; j++)
			{
				_arguments[TrailingMandatoryIndex + j] = GetArgument(_actualArgumentCount - TrailingMandatoryCount + j, out isSplatted);
			}
			int k = _leadingMandatoryParamCount;
			int num = _actualArgumentCount - TrailingMandatoryCount;
			for (int l = 0; l < _optionalParamCount; l++)
			{
				_arguments[OptionalParameterIndex + l] = ((k < num) ? GetArgument(k++, out isSplatted) : Expression.Field(null, Fields.DefaultArgument));
			}
			if (_hasUnsplatParameter)
			{
				Expression expression;
				if (args.Signature.HasSplattedArgument)
				{
					List<Expression> list = new List<Expression>();
					for (; k < num; k++)
					{
						Expression argument = GetArgument(k, out isSplatted);
						if (isSplatted)
						{
							break;
						}
						list.Add(Microsoft.Scripting.Ast.Utils.Box(argument));
					}
					expression = Methods.MakeArrayOpCall(list);
					int num2 = k - args.SimpleArgumentCount;
					int num3 = Math.Min(num - k, _listLength - num2);
					if (num3 > 0)
					{
						expression = Methods.AddSubRange.OpCall(expression, _listVariable, Expression.Constant(num2), Expression.Constant(num3));
						k += num3;
					}
					for (; k < num; k++)
					{
						expression = Methods.AddItem.OpCall(expression, Microsoft.Scripting.Ast.Utils.Box(GetArgument(k, out isSplatted)));
					}
				}
				else
				{
					List<Expression> list2 = new List<Expression>(num - k);
					while (k < num)
					{
						list2.Add(Microsoft.Scripting.Ast.Utils.Box(GetArgument(k++, out isSplatted)));
					}
					expression = Methods.MakeArrayOpCall(list2);
				}
				_arguments[UnsplatParameterIndex] = expression;
			}
			_callArguments = null;
			_listVariable = null;
		}
	}
}
