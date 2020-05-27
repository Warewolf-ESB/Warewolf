using System;
using System.Activities;
using System.Linq;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.State;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.ForEach
{
    [TestClass]
    public class DsfForEachActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySelectAndApply_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySelectAndApply_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity();
            var forEach = new DsfForEachActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachElementName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachElementName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachElementName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachElementName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NumOfExections_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NumOfExections_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void NumOfExections_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, NumOfExections = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachType_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachType = enForEachType.InCSV };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachType = enForEachType.InRange };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ForEachType_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, ForEachType = enForEachType.InCSV };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, ForEachType = enForEachType.InCSV };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void From_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, From = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, From = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void From_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, From = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, From = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void From_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, From = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, From = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void To_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, To = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, To = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void To_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, To = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, To = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void To_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, To = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, To = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Recordset_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Recordset_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Recordset_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, Recordset = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CsvIndexes_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, CsvIndexes = "a" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, CsvIndexes = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CsvIndexes_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, CsvIndexes = "A" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, CsvIndexes = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var forEach = new DsfForEachActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        DsfMultiAssignActivity CommonAssign(Guid? uniqueId = null)
        {
            return uniqueId.HasValue ? new DsfMultiAssignActivity { UniqueID = uniqueId.Value.ToString() } : new DsfMultiAssignActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DataFunc_SameAssigns_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var commonAssign = CommonAssign();
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };
            var forEach = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DataFunc_Equalsssigns_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var commonAssign = CommonAssign(newGuid);
            var commonAssign1 = CommonAssign(newGuid);
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };
            var forEach = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign1
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DataFunc_DifferentAssigns_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var commonAssign = CommonAssign(newGuid);
            var commonAssign1 = CommonAssign(Guid.NewGuid());
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };
            var forEach = new DsfForEachActivity
            {
                UniqueID = uniqueId,
                DisplayName = "AAA",
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign1
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfForEachActivity.Equals(forEach);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfForEachActivity_GetState")]
        public void DsfForEachActivity_GetState_InRange_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var dsfForEachActivity = new DsfForEachActivity
            {
                ForEachType = enForEachType.InRange,
                From = "[[From]]",
                To = "[[To]]",
                CsvIndexes = "",
                NumOfExections = "",
                Recordset = "",
                ForEachElementName = ""
            };
            //------------Execute Test---------------------------
            var stateItems = dsfForEachActivity.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(7, stateItems.Count());

            var expectedResults = new[]
            {new StateVariable
                {
                    Name = "ForEachElementName",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "ForEachType",
                    Type = StateVariable.StateType.Input,
                    Value = "InRange"
                },
                new StateVariable
                {
                    Name = "From",
                    Type = StateVariable.StateType.Input,
                    Value = "[[From]]"
                },
                new StateVariable
                {
                    Name = "To",
                    Type = StateVariable.StateType.Input,
                    Value = "[[To]]"
                },
                new StateVariable
                {
                    Name = "CsvIndexes",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "NumOfExections",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "Recordset",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                }
            };

            var iter = dsfForEachActivity.GetState().Select(
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfForEachActivity_GetState")]
        public void DsfForEachActivity_GetState_InCSV_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var dsfForEachActivity = new DsfForEachActivity
            {
                ForEachType = enForEachType.InCSV,
                From = "",
                To = "",
                CsvIndexes = "[[CsvIndexes]]",
                NumOfExections = "",
                ForEachElementName = "AA",
                Recordset = ""
            };
            //------------Execute Test---------------------------
            var stateItems = dsfForEachActivity.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(7, stateItems.Count());

            var expectedResults = new[]
            {new StateVariable
                {
                    Name = "ForEachElementName",
                    Type = StateVariable.StateType.Input,
                    Value = "AA"
                },
                new StateVariable
                {
                    Name = "ForEachType",
                    Type = StateVariable.StateType.Input,
                    Value = "InCSV"
                },
                new StateVariable
                {
                    Name = "From",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "To",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "CsvIndexes",
                    Type = StateVariable.StateType.Input,
                    Value = "[[CsvIndexes]]"
                },
                new StateVariable
                {
                    Name = "NumOfExections",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "Recordset",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                }
            };

            var iter = dsfForEachActivity.GetState().Select(
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfForEachActivity_GetState")]
        public void DsfForEachActivity_GetState_NumOfExecution_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var dsfForEachActivity = new DsfForEachActivity
            {
                ForEachType = enForEachType.NumOfExecution,
                From = "",
                To = "",
                CsvIndexes = "",
                NumOfExections = "[[NumOfExection]]",
                ForEachElementName = "",
                Recordset = ""
            };
            //------------Execute Test---------------------------
            var stateItems = dsfForEachActivity.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(7, stateItems.Count());

            var expectedResults = new[]
            {new StateVariable
                {
                    Name = "ForEachElementName",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "ForEachType",
                    Type = StateVariable.StateType.Input,
                    Value = "NumOfExecution"
                },
                new StateVariable
                {
                    Name = "From",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "To",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "CsvIndexes",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "NumOfExections",
                    Type = StateVariable.StateType.Input,
                    Value = "[[NumOfExection]]"
                },
                new StateVariable
                {
                    Name = "Recordset",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                }
            };

            var iter = dsfForEachActivity.GetState().Select(
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfForEachActivity_GetState")]
        public void DsfForEachActivity_GetState_InRecordset_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var dsfForEachActivity = new DsfForEachActivity
            {
                ForEachType = enForEachType.InRecordset,
                From = "",
                To = "",
                CsvIndexes = "",
                NumOfExections = "",
                ForEachElementName = "",
                Recordset = "[[Recordset]]"
            };
            //------------Execute Test---------------------------
            var stateItems = dsfForEachActivity.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(7, stateItems.Count());

            var expectedResults = new[]
            {new StateVariable
                {
                    Name = "ForEachElementName",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "ForEachType",
                    Type = StateVariable.StateType.Input,
                    Value = "InRecordset"
                },
                new StateVariable
                {
                    Name = "From",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "To",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "CsvIndexes",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "NumOfExections",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "Recordset",
                    Type = StateVariable.StateType.Input,
                    Value = "[[Recordset]]"
                }
            };

            var iter = dsfForEachActivity.GetState().Select(
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
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        public void DsfForEachActivity_GetState_ForEachElementName_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity
            {
                ForEachType = enForEachType.InRecordset,
                From = "",
                To = "",
                CsvIndexes = "",
                NumOfExections = "",
                Recordset = "[[Recordset]]",
                ForEachElementName = "AA"
            };

            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfForEachActivity);
            //---------------Execute Test ----------------------
            //------------Execute Test---------------------------
            var stateItems = dsfForEachActivity.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(7, stateItems.Count());

            var expectedResults = new[]
            { new StateVariable
                {
                    Name = "ForEachElementName",
                    Type = StateVariable.StateType.Input,
                    Value = "AA"
                },
                new StateVariable
                {
                    Name = "ForEachType",
                    Type = StateVariable.StateType.Input,
                    Value = "InRecordset"
                },
                new StateVariable
                {
                    Name = "From",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "To",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "CsvIndexes",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "NumOfExections",
                    Type = StateVariable.StateType.Input,
                    Value = ""
                },
                new StateVariable
                {
                    Name = "Recordset",
                    Type = StateVariable.StateType.Input,
                    Value = "[[Recordset]]"
                }
            };

            var iter = dsfForEachActivity.GetState().Select(
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