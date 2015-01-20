using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels.VariableList;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class VariableListViewModelTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VariableListViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void VariableListViewModel_Ctor_NullParams_ExpectErrors()
        {
            //------------Setup for test--------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new List<IDataExpression>(), new Mock<IDatalistViewExpressionConvertor>().Object }, typeof(VariableListViewModel));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VariableListViewModel_Ctor")]
        public void VariableListViewModel_Ctor_ValidParams_ExpectNoErrors_AllPropertiesSet()
        {


            //------------Setup for test--------------------------

            var expressions = new List<IDataExpression>();
            var convertor = new Mock<IDatalistViewExpressionConvertor>().Object;

            //------------Execute Test---------------------------
            var val = new VariableListViewModel(expressions, convertor);

            val.FilterExpression = "bob";
            Assert.AreEqual("bob", val.FilterExpression);

            val.Enabled = false;
            Assert.IsFalse(val.Enabled);

            //------------Assert Results-------------------------
            PrivateObject p = new PrivateObject(val);
            Assert.AreEqual(expressions, p.GetField("_workflowExpressions"));
            Assert.AreEqual(convertor, p.GetField("_convertor"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VariableListViewModel_Ctor")]
        public void VariableListViewModel_Ctor_ExpectThatExpressionsAreConverted()
        {


            //------------Setup for test--------------------------

            var expressions = new List<IDataExpression> { new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object };
            var convertor = new Mock<IDatalistViewExpressionConvertor>();
            var convertedRecset = new Mock<IVariablelistViewRecordSetViewModel>().Object;
            var convertedScalar = new Mock<IVariableListViewScalarViewModel>().Object;
            var convertedcolumn = new Mock<IVariableListViewColumn>();
            var convertedcolumn2 = new Mock<IVariableListViewColumn>();
            convertedcolumn.Setup(a => a.RecordsetName).Returns("bob");
            convertedcolumn2.Setup(a => a.RecordsetName).Returns("bob");
            convertor.Setup(a => a.Create(expressions[0])).Returns(convertedRecset);
            convertor.Setup(a => a.Create(expressions[1])).Returns(convertedScalar);
            convertor.Setup(a => a.Create(expressions[2])).Returns(convertedcolumn.Object);
            convertor.Setup(a => a.Create(expressions[3])).Returns(convertedcolumn2.Object);
            //------------Execute Test---------------------------
            var val = new VariableListViewModel(expressions, convertor.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(val.RecordSets.Count, 2);
            Assert.AreEqual(val.Scalars.Count, 1);
            Assert.AreEqual(val.RecordSets.ToArray()[0], convertedRecset);
            Assert.AreEqual(val.RecordSets.ToArray()[1].Name, "bob");
            Assert.AreEqual(val.RecordSets.ToArray()[1].Columns.ToArray()[0], convertedcolumn.Object);
            Assert.AreEqual(val.RecordSets.ToArray()[1].Columns.ToArray()[1], convertedcolumn2.Object);
            Assert.AreEqual(val.Scalars.ToArray()[0], convertedScalar);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VariableListViewModel_ClearUnused")]
        public void VariableListViewModel_CLearUnused()
        {


            //------------Setup for test--------------------------

            var expressions = new List<IDataExpression> { new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object };

            var ExpressionsToRemove = new List<IDataExpression> { expressions[1], expressions[2] };
            var convertor = new Mock<IDatalistViewExpressionConvertor>();
            var convertedRecset = new Mock<IVariablelistViewRecordSetViewModel>().Object;
            var convertedScalar = new Mock<IVariableListViewScalarViewModel>().Object;
            var convertedcolumn = new Mock<IVariableListViewColumn>();
            convertedcolumn.Setup(a => a.RecordsetName).Returns("bob");
            convertor.Setup(a => a.Create(expressions[1])).Returns(convertedRecset);
            convertor.Setup(a => a.Create(expressions[0])).Returns(convertedScalar);
            convertor.Setup(a => a.Create(expressions[2])).Returns(convertedcolumn.Object);
            //------------Execute Test---------------------------
            var val = new VariableListViewModel(expressions, convertor.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(val.RecordSets.Count, 2);
            Assert.AreEqual(val.Scalars.Count, 1);
            Assert.AreEqual(val.RecordSets.ToArray()[0], convertedRecset);
            Assert.AreEqual(val.RecordSets.ToArray()[1].Name, "bob");
            Assert.AreEqual(val.RecordSets.ToArray()[1].Columns.ToArray()[0], convertedcolumn.Object);
            Assert.AreEqual(val.Scalars.ToArray()[0], convertedScalar);

            val.ClearUnused(ExpressionsToRemove);

            Assert.AreEqual(val.RecordSets.Count, 2);
            Assert.AreEqual(val.Scalars.Count, 0);
            Assert.AreEqual(val.RecordSets.ToArray()[0], convertedRecset);
            Assert.AreEqual(val.RecordSets.ToArray()[1].Name, "bob");
            Assert.AreEqual(val.RecordSets.ToArray()[1].Columns.ToArray()[0], convertedcolumn.Object);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VariableListViewModel_ClearUnused")]
        public void VariableListViewModel_ClearUnusedRecset()
        {


            //------------Setup for test--------------------------

            var expressions = new List<IDataExpression> { new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object };

            var ExpressionsToRemove = new List<IDataExpression> { expressions[0], expressions[2] };
            var convertor = new Mock<IDatalistViewExpressionConvertor>();
            var convertedRecset = new Mock<IVariablelistViewRecordSetViewModel>().Object;
            var convertedScalar = new Mock<IVariableListViewScalarViewModel>().Object;
            var convertedcolumn = new Mock<IVariableListViewColumn>();
            convertedcolumn.Setup(a => a.RecordsetName).Returns("bob");
            convertor.Setup(a => a.Create(expressions[1])).Returns(convertedRecset);
            convertor.Setup(a => a.Create(expressions[0])).Returns(convertedScalar);
            convertor.Setup(a => a.Create(expressions[2])).Returns(convertedcolumn.Object);
            //------------Execute Test---------------------------
            var val = new VariableListViewModel(expressions, convertor.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(val.RecordSets.Count, 2);
            Assert.AreEqual(val.Scalars.Count, 1);
            Assert.AreEqual(val.RecordSets.ToArray()[0], convertedRecset);
            Assert.AreEqual(val.RecordSets.ToArray()[1].Name, "bob");
            Assert.AreEqual(val.RecordSets.ToArray()[1].Columns.ToArray()[0], convertedcolumn.Object);
            Assert.AreEqual(val.Scalars.ToArray()[0], convertedScalar);

            val.ClearUnused(ExpressionsToRemove);

            Assert.AreEqual(val.RecordSets.Count, 1);
            Assert.AreEqual(val.Scalars.Count, 1);

            Assert.AreEqual(val.RecordSets.ToArray()[0].Name, "bob");
            Assert.AreEqual(val.RecordSets.ToArray()[0].Columns.ToArray()[0], convertedcolumn.Object);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VariableListViewModel_Sort")]
        public void VariableListViewModel_Sort_Asc_thenDesc()
        {


            //------------Setup for test--------------------------

            var expressions = new List<IDataExpression> { new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object };

            var convertor = new Mock<IDatalistViewExpressionConvertor>();
            var convertedRecset = new Mock<IVariablelistViewRecordSetViewModel>();
            var convertedScalar = new Mock<IVariableListViewScalarViewModel>();
            var convertedRecset2 = new Mock<IVariablelistViewRecordSetViewModel>();
            var convertedScalar2 = new Mock<IVariableListViewScalarViewModel>();
            convertedRecset.Setup(a => a.Name).Returns("b");
            convertedRecset2.Setup(a => a.Name).Returns("a");
            convertedScalar.Setup(a => a.Name).Returns("c");
            convertedScalar2.Setup(a => a.Name).Returns("d");
            convertor.Setup(a => a.Create(expressions[0])).Returns(convertedRecset.Object);
            convertor.Setup(a => a.Create(expressions[1])).Returns(convertedScalar.Object);
            convertor.Setup(a => a.Create(expressions[2])).Returns(convertedRecset2.Object);
            convertor.Setup(a => a.Create(expressions[3])).Returns(convertedScalar2.Object);
            //------------Execute Test---------------------------
            var val = new VariableListViewModel(expressions, convertor.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(val.RecordSets.Count, 2);
            Assert.AreEqual(val.Scalars.Count, 2);
            Assert.AreEqual(val.RecordSets.ToArray()[0].Name, "b");
            Assert.AreEqual(val.RecordSets.ToArray()[1].Name, "a");
            Assert.AreEqual(val.Scalars.ToArray()[0].Name, "c");
            Assert.AreEqual(val.Scalars.ToArray()[1].Name, "d");
            val.Sort();
            Assert.AreEqual(val.RecordSets.ToArray()[0].Name, "a");
            Assert.AreEqual(val.RecordSets.ToArray()[1].Name, "b");
            Assert.AreEqual(val.Scalars.ToArray()[0].Name, "c");
            Assert.AreEqual(val.Scalars.ToArray()[1].Name, "d");
            val.Sort();
            Assert.AreEqual(val.RecordSets.ToArray()[0].Name, "b");
            Assert.AreEqual(val.RecordSets.ToArray()[1].Name, "a");
            Assert.AreEqual(val.Scalars.ToArray()[0].Name, "d");
            Assert.AreEqual(val.Scalars.ToArray()[1].Name, "c");
            val.SortCommand.Execute();
            Assert.AreEqual(val.RecordSets.ToArray()[0].Name, "a");
            Assert.AreEqual(val.RecordSets.ToArray()[1].Name, "b");
            Assert.AreEqual(val.Scalars.ToArray()[0].Name, "c");
            Assert.AreEqual(val.Scalars.ToArray()[1].Name, "d");

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VariableListViewModel_Filter")]
        public void VariableListViewModel_Filter_SetsSomeFieldsToInvisible()
        {


            //------------Setup for test--------------------------

            var expressions = new List<IDataExpression> { new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object, new Mock<IDataExpression>().Object };

            var convertor = new Mock<IDatalistViewExpressionConvertor>();
            var convertedRecset = new Mock<IVariablelistViewRecordSetViewModel>();
            var convertedScalar = new Mock<IVariableListViewScalarViewModel>();
            var convertedRecset2 = new Mock<IVariablelistViewRecordSetViewModel>();
            var convertedScalar2 = new Mock<IVariableListViewScalarViewModel>();
            convertedRecset.Setup(a => a.Name).Returns("b");
            convertedRecset2.Setup(a => a.Name).Returns("aa");
            convertedScalar.Setup(a => a.Name).Returns("c");
            convertedScalar2.Setup(a => a.Name).Returns("da");
            convertor.Setup(a => a.Create(expressions[0])).Returns(convertedRecset.Object);
            convertor.Setup(a => a.Create(expressions[1])).Returns(convertedScalar.Object);
            convertor.Setup(a => a.Create(expressions[2])).Returns(convertedRecset2.Object);
            convertor.Setup(a => a.Create(expressions[3])).Returns(convertedScalar2.Object);
            //------------Execute Test---------------------------
            var val = new VariableListViewModel(expressions, convertor.Object);
            val.Filter("a");
            convertedRecset.VerifySet(a => a.Visible = false);
            convertedScalar.VerifySet(a => a.Visible = false);
            convertedScalar2.VerifySet(a => a.Visible = false, Times.Never());
            convertedRecset2.VerifySet(a => a.Visible = false, Times.Never());
            val.Filter("");
            convertedRecset.VerifySet(a => a.Visible = true, Times.Exactly(2));
            convertedScalar.VerifySet(a => a.Visible = true, Times.Exactly(2));
            convertedScalar2.VerifySet(a => a.Visible = true, Times.Exactly(2));
            convertedRecset2.VerifySet(a => a.Visible = true, Times.Exactly(2));
            val.FilterExpression = "a";
            val.FilterCommand.Execute();
            convertedRecset.VerifySet(a => a.Visible = false);
            convertedScalar.VerifySet(a => a.Visible = false);
            convertedScalar2.VerifySet(a => a.Visible = false, Times.Never());
            convertedRecset2.VerifySet(a => a.Visible = false, Times.Never());

        }
    }
    // ReSharper restore InconsistentNaming
}
