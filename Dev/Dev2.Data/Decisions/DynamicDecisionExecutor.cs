#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Serializers;
using Dev2.Data.Decisions.Operations;

namespace Dev2.Data.Decisions
{
  public class DynamicDecisionExecutor
  {
    private readonly DecisionExpression _expression;
    private readonly Dev2DecisionFactory _decisionFactory;

    public DynamicDecisionExecutor(DecisionExpression expression)
    {
      _expression = expression;
      _decisionFactory = Dev2DecisionFactory.Instance();
    }

    public DynamicDecisionExecutor(string expression):this(new Dev2JsonSerializer().Deserialize<DecisionExpression>(expression))
    {
    }

    public bool Evaluate()
    {
      var currentExpression = _expression;
      bool? result = null;
      DecisionExpression previousExpression = null;
      while (currentExpression!=null)
      {
        var currentResult = _decisionFactory.FetchDecisionFunction(currentExpression.Expression.EvaluationFn)
                                            .Invoke(new[]
                                                    {
                                                      currentExpression.Expression.Col1,
                                                      currentExpression.Expression.Col2,
                                                      currentExpression.Expression.Col3
                                                    });
        if (!result.HasValue)
        {
          result = currentResult;
        }
        else
        {
          switch (previousExpression.LogicalOperandType)
          {
            case OperandType.And:
              result = result.Value && currentResult;
              break;
            case OperandType.Or:
              result = result.Value || currentResult;
              break;
            default:
              continue;
          }
        }
        previousExpression = currentExpression;
        currentExpression = currentExpression.Chain;  
      }
      return result.HasValue && result.Value;
    }
  }
}