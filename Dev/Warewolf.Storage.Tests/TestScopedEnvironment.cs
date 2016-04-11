using System;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WarewolfParserInterop;

// ReSharper disable InconsistentNaming

namespace Warewolf.Storage.Tests
{
    [TestClass]
    public class TestScopedEnvironment
    {
        private Mock<IExecutionEnvironment> _mockEnv;

        [TestInitialize]
        public void Setup()
        {
            _mockEnv = new Mock<IExecutionEnvironment>();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Setup")]
        public void ScopedEnvironment_Setup_Constructor_ExpectEquals()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "bob", "builder");

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            PrivateObject p = new PrivateObject(scopedEnvironment);
            Assert.AreEqual(_mockEnv.Object, p.GetField("_inner"));
            Assert.AreEqual("bob", p.GetField("_datasource").ToString());
            Assert.AreEqual("builder", p.GetField("_alias").ToString());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Eval")]
        public void ScopedEnvironment_Eval_Basic_ExpectEvalReplaced()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Eval("[[a]]", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Eval("[[Person(*)]]", 0, true));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Eval")]
        public void ScopedEnvironment_Eval_ExpectNoReplacement_IfNoAlias()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Eval("[[b]]", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Eval("[[b]]", 0, true));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalStrict")]
        public void ScopedEnvironment_EvalStrict_Basic_ExpectEvalStrictReplaced()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.EvalStrict("[[a]]", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.EvalStrict("[[Person(*)]]", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalStrict")]
        public void ScopedEnvironment_EvalStrict_ExpectNoReplacement_IfNoAlias()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.EvalStrict("[[b]]", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.EvalStrict("[[b]]", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Assign")]
        public void ScopedEnvironment_Assign_Basic_ExpectAssignReplaced()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Assign("[[a]]", "bob", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Assign("[[Person(*)]]", "bob", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Assign")]
        public void ScopedEnvironment_Assign_HasUpdateValue_ExpectAssignReplaced()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Assign("[[a]]", "bob", 1);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Assign("[[Person(1)]]", "bob", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Assign")]
        public void ScopedEnvironment_Assign_HasUpdateValue_ExpectAssignReplacedOnRight()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Assign("[[a]]", "[[a]]", 1);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Assign("[[Person(1)]]", "[[Person(1)]]", 0));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_Assign")]
        public void ScopedEnvironment_Assign_ExpectNoReplacement_IfNoAlias()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");

            //------------Execute Test---------------------------
            scopedEnvironment.Assign("[[b]]", "bob", 0);
            //------------Assert Results-------------------------
            _mockEnv.Verify(a => a.Assign("[[b]]", "bob", 0));
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignWithFrame")]
        public void ScopedEnvironment_AssignWithFrame_Basic_ExpectAssignWithFrameReplaced()
        {
            //------------Setup for test--------------------------
            bool replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a,  int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            //------------Execute Test---------------------------
            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);
            //------------Assert Results-------------------------
            Assert.IsTrue(replaced);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_AssignWithFrame")]
        public void ScopedEnvironment_AssignWithFrame_ExpectNoReplacement_IfNoAlias()
        {
            //------------Setup for test--------------------------
            bool replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            //------------Execute Test---------------------------
            scopedEnvironment.AssignWithFrame(new AssignValue("[[b]]", "bob"), 0);
            //------------Assert Results-------------------------
            Assert.IsFalse(replaced);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_GetLength")]
        public void ScopedEnvironment_GetLength_ExpectEquals()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.GetLength(It.IsAny<string>())).Returns(1);

            //------------Execute Test---------------------------
            var length = scopedEnvironment.GetLength("[[a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(length, 1);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_GetCount")]
        public void ScopedEnvironment_GetCount_ExpectEquals()
        {
            //------------Setup for test--------------------------
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.GetCount(It.IsAny<string>())).Returns(1);

            //------------Execute Test---------------------------
            var length = scopedEnvironment.GetCount("[[a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(length, 1);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalRecordSetIndexes")]
        public void ScopedEnvironment_EvalRecordSetIndexes_Basic_ExpectAssignWithFrameReplaced()
        {
            //------------Setup for test--------------------------
            bool replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            //------------Execute Test---------------------------
            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);
            //------------Assert Results-------------------------
            Assert.IsTrue(replaced);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ScopedEnvironment_EvalRecordSetIndexes")]
        public void ScopedEnvironment_EvalRecordSetIndexes()
        {
            //------------Setup for test--------------------------
            bool replaced = false;
            var scopedEnvironment = new ScopedEnvironment(_mockEnv.Object, "[[Person(*)]]", "[[a]]");
            _mockEnv.Setup(a => a.AssignWithFrame(It.IsAny<IAssignValue>(), It.IsAny<int>())).Callback((IAssignValue a, int b) =>
            {
                replaced = a.Name.Contains("[[Person(*)]]");
            });

            //------------Execute Test---------------------------
            scopedEnvironment.AssignWithFrame(new AssignValue("[[a]]", "bob"), 0);
            //------------Assert Results-------------------------
            Assert.IsTrue(replaced);
        }

      
    }
}
