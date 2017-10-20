using System;
using System.Collections.Generic;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.DsfDecision
{
    [TestClass]
    public class DsfDecisionEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyAssigns_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId };
            var decision1 = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDSame_EmptyAssigns_IsEqual()
        {
            //---------------Set up test pack-------------------
            DsfFlowDecisionActivity decisionActivity = new DsfFlowDecisionActivity();
            var decision = new Dev2.Activities.DsfDecision(decisionActivity);
            var decision1 = new Dev2.Activities.DsfDecision(decisionActivity);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);//This Id is meaningless
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyAssigns_IsEqual()
        {
            //---------------Set up test pack-------------------
            DsfFlowDecisionActivity decisionActivity = new DsfFlowDecisionActivity();
            DsfFlowDecisionActivity decisionActivity1 = new DsfFlowDecisionActivity();
            var decision = new Dev2.Activities.DsfDecision(decisionActivity);
            var decision1 = new Dev2.Activities.DsfDecision(decisionActivity1);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);//This Id is meaningless
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, DisplayName = "a" };
            var decision1 = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TrueArm_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                TrueArm = trueArms

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                TrueArm = trueArms1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TrueArm_Different_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity()
            {
                NextNodes = new List<IDev2Activity>()
                {
                    dsfBaseConvertActivity
                }
            };
            ;
            var dsfCaseConvertActivity = new DsfCaseConvertActivity()
            {
                NextNodes = new List<IDev2Activity>() { dsfCalculateActivity }
            };

            var trueArms = new List<IDev2Activity>
            {
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfCalculateActivity,
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a"
                ,
                TrueArm = trueArms

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a"
                ,
                TrueArm = trueArms1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FalseArm_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity()
            {
                NextNodes = new List<IDev2Activity>() { dsfCaseConvertActivity }
            };
            var dsfBaseConvertActivity = new DsfBaseConvertActivity()
            {
                NextNodes = new List<IDev2Activity>()
                {
                    dsfCalculateActivity
                }
            };
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Conditions_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                TheStack = new List<Dev2Decision>()
                {
                    new Dev2Decision()
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>()
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision()
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>()
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }

                },
                DisplayText = "a"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dev2DecisionStack1 = new Dev2DecisionStack()
            {
                TheStack = new List<Dev2Decision>()
                {
                    new Dev2Decision()
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>()
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision()
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>()
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }

                },
                DisplayText = "a"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = dev2DecisionStack

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = dev2DecisionStack1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayText_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "aa"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dev2DecisionStack1 = new Dev2DecisionStack()
            {
                DisplayText = "aa"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = dev2DecisionStack

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = dev2DecisionStack1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayText_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "1aa"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dev2DecisionStack1 = new Dev2DecisionStack()
            {
                DisplayText = "aa"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = dev2DecisionStack

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = dev2DecisionStack1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FalseArmText_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "aa"
                ,
                FalseArmText = "ErrorArm1"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dev2DecisionStack1 = new Dev2DecisionStack()
            {
                DisplayText = "aa"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = dev2DecisionStack

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = dev2DecisionStack1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FalseArmText_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "aa"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dev2DecisionStack1 = new Dev2DecisionStack()
            {
                DisplayText = "aa"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = dev2DecisionStack

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = dev2DecisionStack1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Mode_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                Mode = Dev2DecisionMode.AND

            };
            var dev2DecisionStack1 = new Dev2DecisionStack()
            {
                Mode = Dev2DecisionMode.AND

            };
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = dev2DecisionStack

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = dev2DecisionStack1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Mode_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                Mode = Dev2DecisionMode.AND

            };
            var dev2DecisionStack1 = new Dev2DecisionStack()
            {
                Mode = Dev2DecisionMode.OR

            };
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = dev2DecisionStack

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = dev2DecisionStack1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Conditions_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                TheStack = new List<Dev2Decision>()
                {
                    new Dev2Decision()
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>()
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision()
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>()
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }

                },
                DisplayText = "a"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dev2DecisionStack1 = new Dev2DecisionStack()
            {
                TheStack = new List<Dev2Decision>()
                {
                    new Dev2Decision()
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>()
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        },
                        Col2 = "Col1"
                    },
                    new Dev2Decision()
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>()
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }

                },
                DisplayText = "a"
                ,
                FalseArmText = "ErrorArm"
                ,
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND

            };
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = dev2DecisionStack

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = dev2DecisionStack1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Conditions_Null_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity();
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var trueArms = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfBaseConvertActivity,
                dsfCalculateActivity,
                dsfCaseConvertActivity
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms,
                Conditions = null

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a",
                FalseArm = trueArms1,
                Conditions = null
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FalseArm_Different_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            var dsfCalculateActivity = new DsfCalculateActivity()
            {
                NextNodes = new List<IDev2Activity>()
                {
                    dsfBaseConvertActivity
                }
            };
            ;
            var dsfCaseConvertActivity = new DsfCaseConvertActivity()
            {
                NextNodes = new List<IDev2Activity>() { dsfCalculateActivity }
            };

            var trueArms = new List<IDev2Activity>
            {
                dsfCaseConvertActivity
            };

            var trueArms1 = new List<IDev2Activity>
            {
                dsfCalculateActivity,
            };
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a"
                ,
                FalseArm = trueArms

            };
            var decision1 = new Dev2.Activities.DsfDecision()
            {
                UniqueID = uniqueId
                ,
                DisplayName = "a"
                ,
                FalseArm = trueArms1
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, DisplayName = "A" };
            var decision1 = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, DisplayName = "AAA" };
            var decision1 = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, Result = "A" };
            var decision1 = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var decision = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, Result = "AAA" };
            var decision1 = new Dev2.Activities.DsfDecision() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(decision);
            //---------------Execute Test ----------------------
            var equals = decision.Equals(decision1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
    }
}
