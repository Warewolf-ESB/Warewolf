using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common.State;
using Dev2.Communication;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CaseConvertActivityTests
    /// </summary>
    [TestClass]
    
    public class DecisionActivityTests : BaseActivityUnitTest
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDecision_SerializeDeserialize")]
        public void DsfDecision_SerializeDeserialize_WhenAndSetTrue_ShouldHaveAndAsTrueWhenDeserialized()
        {
            //------------Setup for test--------------------------
            var dsfDecision = new DsfDecision { And = true };
            var serializer = new Dev2JsonSerializer();
            var serDecision = serializer.Serialize(dsfDecision);
            //------------Execute Test---------------------------
            var deSerDecision = serializer.Deserialize<DsfDecision>(serDecision);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deSerDecision);
            Assert.IsTrue(deSerDecision.And);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDecision_SerializeDeserialize")]
        public void DsfDecision_SerializeDeserialize_WhenAndSetFalse_ShouldHaveAndAsFalseWhenDeserialized()
        {
            //------------Setup for test--------------------------
            var dsfDecision = new DsfDecision { And = false };
            var serializer = new Dev2JsonSerializer();
            var serDecision = serializer.Serialize(dsfDecision);
            //------------Execute Test---------------------------
            var deSerDecision = serializer.Deserialize<DsfDecision>(serDecision);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deSerDecision);
            Assert.IsFalse(deSerDecision.And);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDecision_GetState")]
        public void DsfDecision_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var conditions = new Dev2DecisionStack();
            conditions.TheStack = new List<Dev2Decision>();
            conditions.AddModelItem(new Dev2Decision
            {
                Col1 = "[[a]]",
                EvaluationFn = Data.Decisions.Operations.enDecisionType.IsEqual,
                Col2 = "bob"
            });
            var serializer = new Dev2JsonSerializer();
            //------------Setup for test--------------------------
            var act = new DsfDecision { Conditions = conditions, And = true };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(5, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "Conditions",
                    Type = StateVariable.StateType.Input,
                    Value = serializer.Serialize(conditions)
                },
                new StateVariable
                {
                    Name="And",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = null
                },
                new StateVariable
                {
                    Name="TrueArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.TrueArm?.ToList())
                },
                new StateVariable
                {
                    Name="FalseArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.FalseArm?.ToList())
                }
            };

            var iter = act.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}