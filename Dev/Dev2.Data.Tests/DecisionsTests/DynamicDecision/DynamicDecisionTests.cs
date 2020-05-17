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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Data.Decisions;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using OperandType = Dev2.Data.Decisions.OperandType;

namespace Dev2.Data.Tests.DecisionsTests.DynamicDecision
{
  [TestClass]
  public class DynamicDecisionTests
  {
    [TestMethod]
    [Owner("Hagashen Naidu")]
    [TestCategory("Dynamic Decision")]
    public void Simple_Decision_WithEquals_No_Chain_ReturnTrue()
    {
      //------------Setup for test--------------------------
      var decisionExpression = new DecisionExpression();
      decisionExpression.Expression = new Dev2Decision
                                      {
                                        Col1 = "10",
                                        EvaluationFn = enDecisionType.IsEqual,
                                        Col2 = "10"
                                      };
      var dynamicDecisionExecutor = new DynamicDecisionExecutor(decisionExpression);
      //------------Execute Test---------------------------
      var result = dynamicDecisionExecutor.Evaluate();
      //------------Assert Results-------------------------
      Assert.IsTrue(result);
    }

    [TestMethod]
    [Owner("Hagashen Naidu")]
    [TestCategory("Dynamic Decision")]
    public void Simple_Decisions_WithEquals_AND_Chain_ReturnTrue()
    {
      //------------Setup for test--------------------------
      var decisionExpression = new DecisionExpression
                               {
                                 Expression = new Dev2Decision
                                              {
                                                Col1 = "10", EvaluationFn = enDecisionType.IsEqual, Col2 = "10"
                                              },
                                 LogicalOperandType = OperandType.And,
                                 Chain = new DecisionExpression
                                         {
                                           Expression = new Dev2Decision
                                                        {
                                                          Col1 = "12", EvaluationFn = enDecisionType.IsEqual,
                                                          Col2 = "12"
                                                        }
                                         }
                               };
      var dynamicDecisionExecutor = new DynamicDecisionExecutor(decisionExpression);
      //------------Execute Test---------------------------
      var result = dynamicDecisionExecutor.Evaluate();
      //------------Assert Results-------------------------
      Assert.IsTrue(result);
    }

    [TestMethod]
    [Owner("Hagashen Naidu")]
    [TestCategory("Dynamic Decision")]
    public void Simple_Decisions_WithEquals_AND_Chain_Return_False()
    {
      //------------Setup for test--------------------------
      var decisionExpression = new DecisionExpression
                               {
                                 Expression = new Dev2Decision
                                              {
                                                Col1 = "10",
                                                EvaluationFn = enDecisionType.IsEqual,
                                                Col2 = "11"
                                              },
                                 LogicalOperandType = OperandType.And,
                                 Chain = new DecisionExpression
                                         {
                                           Expression = new Dev2Decision
                                                        {
                                                          Col1 = "12",
                                                          EvaluationFn = enDecisionType.IsEqual,
                                                          Col2 = "12"
                                                        }
                                         }
                               };
      var dynamicDecisionExecutor = new DynamicDecisionExecutor(decisionExpression);
      //------------Execute Test---------------------------
      var result = dynamicDecisionExecutor.Evaluate();
      //------------Assert Results-------------------------
      Assert.IsFalse(result);
    }

    [TestMethod]
    [Owner("Hagashen Naidu")]
    [TestCategory("Dynamic Decision")]
    public void Simple_Decisions_WithEquals_OR_Chain_Return_True()
    {
      //------------Setup for test--------------------------
      var decisionExpression = new DecisionExpression
                               {
                                 Expression = new Dev2Decision
                                              {
                                                Col1 = "10",
                                                EvaluationFn = enDecisionType.IsEqual,
                                                Col2 = "11"
                                              },
                                 LogicalOperandType = OperandType.Or,
                                 Chain = new DecisionExpression
                                         {
                                           Expression = new Dev2Decision
                                                        {
                                                          Col1 = "12",
                                                          EvaluationFn = enDecisionType.IsEqual,
                                                          Col2 = "12"
                                                        }
                                         }
                               };
      var dynamicDecisionExecutor = new DynamicDecisionExecutor(decisionExpression);
      //------------Execute Test---------------------------
      var result = dynamicDecisionExecutor.Evaluate();
      //------------Assert Results-------------------------
      Assert.IsTrue(result);
    }

    [TestMethod]
    [Owner("Hagashen Naidu")]
    [TestCategory("Dynamic Decision")]
    public void Simple_Decisions_WithEquals_OR_Chain_Return_False()
    {
      //------------Setup for test--------------------------
      var decisionExpression = new DecisionExpression
                               {
                                 Expression = new Dev2Decision
                                              {
                                                Col1 = "10",
                                                EvaluationFn = enDecisionType.IsEqual,
                                                Col2 = "11"
                                              },
                                 LogicalOperandType = OperandType.Or,
                                 Chain = new DecisionExpression
                                         {
                                           Expression = new Dev2Decision
                                                        {
                                                          Col1 = "12",
                                                          EvaluationFn = enDecisionType.IsEqual,
                                                          Col2 = "13"
                                                        }
                                         }
                               };
      var dynamicDecisionExecutor = new DynamicDecisionExecutor(decisionExpression);
      //------------Execute Test---------------------------
      var result = dynamicDecisionExecutor.Evaluate();
      //------------Assert Results-------------------------
      Assert.IsFalse(result);
    }

    [TestMethod]
    [Owner("Hagashen Naidu")]
    [TestCategory("Dynamic Decision")]
    public void Simple_Decisions_WithEquals_Mixed_Chain_Return_False()
    {
      //------------Setup for test--------------------------
      var decisionExpression = new DecisionExpression
                               {
                                 Expression = new Dev2Decision
                                              {
                                                Col1 = "10",
                                                EvaluationFn = enDecisionType.IsEqual,
                                                Col2 = "10"
                                              },
                                 LogicalOperandType = OperandType.Or,
                                 Chain = new DecisionExpression
                                         {
                                           Expression = new Dev2Decision
                                                        {
                                                          Col1 = "12",
                                                          EvaluationFn = enDecisionType.IsEqual,
                                                          Col2 = "13"
                                                        },
                                           LogicalOperandType = OperandType.And,
                                           Chain = new DecisionExpression
                                                   {
                                                     Expression = new Dev2Decision
                                                                  {
                                                                    Col1 = "12",
                                                                    EvaluationFn = enDecisionType.IsEqual,
                                                                    Col2 = "12"
                                                                  },
                                                     LogicalOperandType = OperandType.And,
                                                     Chain = new DecisionExpression
                                                             {
                                                               Expression = new Dev2Decision
                                                                            {
                                                                              Col1 = "100",
                                                                              EvaluationFn = enDecisionType.IsEqual,
                                                                              Col2 = "200"
                                                                            }
                                                             }
                                                   }
                                         }
                               };
      var dynamicDecisionExecutor = new DynamicDecisionExecutor(decisionExpression);
      //------------Execute Test---------------------------
      var result = dynamicDecisionExecutor.Evaluate();
      //------------Assert Results-------------------------
      Assert.IsFalse(result);
    }

    [TestMethod]
    [Owner("Hagashen Naidu")]
    [TestCategory("Dynamic Decision")]
    public void Simple_Decisions_WithEquals_Mixed_Chain_Return_True()
    {
      //------------Setup for test--------------------------
      var decisionExpression = new DecisionExpression
                               {
                                 Expression = new Dev2Decision
                                              {
                                                Col1 = "10",
                                                EvaluationFn = enDecisionType.IsEqual,
                                                Col2 = "10"
                                              },
                                 LogicalOperandType = OperandType.Or,
                                 Chain = new DecisionExpression
                                         {
                                           Expression = new Dev2Decision
                                                        {
                                                          Col1 = "12",
                                                          EvaluationFn = enDecisionType.IsEqual,
                                                          Col2 = "13"
                                                        },
                                           LogicalOperandType = OperandType.And,
                                           Chain = new DecisionExpression
                                                   {
                                                     Expression = new Dev2Decision
                                                                  {
                                                                    Col1 = "12",
                                                                    EvaluationFn = enDecisionType.IsEqual,
                                                                    Col2 = "12"
                                                                  },
                                                     LogicalOperandType = OperandType.And,
                                                     Chain = new DecisionExpression
                                                             {
                                                               Expression = new Dev2Decision
                                                                            {
                                                                              Col1 = "100",
                                                                              EvaluationFn = enDecisionType.IsEqual,
                                                                              Col2 = "100"
                                                                            }
                                                             }
                                                   }
                                         }
                               };
      var dynamicDecisionExecutor = new DynamicDecisionExecutor(decisionExpression);
      //------------Execute Test---------------------------
      var result = dynamicDecisionExecutor.Evaluate();
      //------------Assert Results-------------------------
      Assert.IsTrue(result);
    }

    [TestMethod]
    [Owner("Hagashen Naidu")]
    [TestCategory("Dynamic Decision")]
    public void Simple_Decisions_WithSerializedString_ShouldEvaluate_True()
    {
      //------------Setup for test--------------------------
      var serializedVersion = @"{
                                 ""Expression"": {
                                 ""Col1"": ""10"",
                                 ""Col2"": ""10"",
                                 ""EvaluationFn"": ""IsEqual""
                                  },
                                  ""LogicalOperandType"": ""Or"",
                                  ""Chain"": {
                                    ""Expression"": {
                                      ""Col1"": ""12"",
                                      ""Col2"": ""13"",
                                      ""EvaluationFn"": ""IsEqual""
                                    },
                                    ""LogicalOperandType"": ""And"",
                                    ""Chain"": {
                                      ""Expression"": {
                                        ""Col1"": ""12"",
                                        ""Col2"": ""12"",
                                        ""EvaluationFn"": ""IsEqual""
                                      },
                                      ""LogicalOperandType"": ""And"",
                                      ""Chain"": {
                                        ""Expression"": {
                                          ""Col1"": ""100"",
                                          ""Col2"": ""100"",
                                          ""EvaluationFn"": ""IsEqual""
                                        }
                                      }
                                    }
                                  }
                                }";
      var dynamicDecisionExecutor = new DynamicDecisionExecutor(serializedVersion);
      //------------Execute Test---------------------------
      var result = dynamicDecisionExecutor.Evaluate();
      //------------Assert Results-------------------------
      Assert.IsTrue(result);
    }
  }
}
