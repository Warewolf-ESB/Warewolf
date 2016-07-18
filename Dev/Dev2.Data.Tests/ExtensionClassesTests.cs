using System;
using System.Collections.Generic;
using Dev2.Data.Binary_Objects;
using Dev2.Data.MathOperations;
using Dev2.Data.SystemTemplates;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.EqualityComparers;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class ExtensionClassesTests
    {
        [TestMethod]
        public void DataListConstants_ShouldHave_AllConstants()
        {
            Assert.IsNotNull(DataListConstants.DefaultCase);
            Assert.IsNotNull(DataListConstants.DefaultDecision);
            Assert.IsNotNull(DataListConstants.DefaultStack);
            Assert.IsNotNull(DataListConstants.DefaultSwitch);
            Assert.IsNotNull(DataListConstants.EmptyRowStartIdx);
            Assert.IsNotNull(DataListConstants.MinRowSize);
            Assert.IsNotNull(DataListConstants.RowGrowthFactor);
        }

        [TestMethod]
        public void Dev2ColumnComparer_ShouldHave()
        {
            Dev2ColumnComparer comparer = Dev2ColumnComparer.Instance;
            Assert.IsNotNull(comparer);
        }

        [TestMethod]
        public void Dev2ColumnComparer_GetHashCode_ShouldHaveHashCode()
        {
            Dev2ColumnComparer comparer = Dev2ColumnComparer.Instance;
            Assert.IsNotNull(comparer);
            var dev2Column = DataListFactory.CreateDev2Column("", "", false, enDev2ColumnArgumentDirection.None);
            var hashCode = comparer.GetHashCode(dev2Column);
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        public void Dev2ColumnComparer_Equals_Should()
        {
            Dev2ColumnComparer comparer = Dev2ColumnComparer.Instance;
            Assert.IsNotNull(comparer);
            var dev2Column = DataListFactory.CreateDev2Column("", "Value", false, enDev2ColumnArgumentDirection.None);
            var dev2Column2 = DataListFactory.CreateDev2Column("", "SomeValue", false,
                enDev2ColumnArgumentDirection.Input);
            var equals = comparer.Equals(dev2Column, dev2Column2);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        public void GivenResourceName_ResourceForTree_ToString_ShouldReturtnResourceName()
        {
            ResourceForTree resourceForTree = new ResourceForTree();
            Assert.IsNotNull(resourceForTree);
            resourceForTree.ResourceName = "SomeName";
            var res = resourceForTree.ToString();
            Assert.AreEqual("SomeName", res);
        }

        [TestMethod]
        public void GivenTagName_SystemTag_ShouldSurroundNameWithTags()
        {
            var tag = new SystemTag("SomeName");
            Assert.IsNotNull(tag);
            Assert.AreEqual("<SomeName>", tag.StartTag);
            Assert.AreEqual("</SomeName>", tag.EndTag);
        }

        [TestMethod]
        public void GivenName_InputDefinition_ShouldNameWithTags()
        {
            var inputDefinition = new InputDefinition("SomeName", "MapsToSomething", false);
            Assert.IsNotNull(inputDefinition);
            Assert.AreEqual("SomeName", inputDefinition.Name);
            Assert.AreEqual("MapsToSomething", inputDefinition.MapsTo);
            Assert.AreEqual("<MapsToSomething>", inputDefinition.StartTagSearch);
            Assert.AreEqual("</MapsToSomething>", inputDefinition.EndTagSearch);
            Assert.AreEqual("<SomeName>", inputDefinition.StartTagReplace);
            Assert.AreEqual("</SomeName>", inputDefinition.EndTagReplace);
            Assert.IsFalse(inputDefinition.IsEvaluated);
        }

        [TestMethod]
        public void RecordsetNotFoundException_ShouldHaveConstructor()
        {
            var definition = new RecordsetNotFoundException();
            Assert.IsNotNull(definition);
            var foundException = new RecordsetNotFoundException("Error Message");
            Assert.IsNotNull(foundException);
            var notFoundException = new RecordsetNotFoundException("Error Message", new Exception());
            Assert.IsNotNull(notFoundException);
        }

        [TestMethod]
        public void SearchTO_ShouldHaveConstructor()
        {
            var searchTo = new SearchTO("searchField", "seacrchType", "searchCriteria", "result");
            Assert.IsNotNull(searchTo);
            Assert.AreEqual("searchField", searchTo.FieldsToSearch);
            Assert.AreEqual("seacrchType", searchTo.SearchType);
            Assert.AreEqual("searchCriteria", searchTo.SearchCriteria);
            Assert.AreEqual("result", searchTo.Result);
        }

        [TestMethod]
        public void CreateEvaluationFunctionTO_ShouldHaveConstructor()
        {
            var evaluationFunctionTo = MathOpsFactory.CreateEvaluationFunctionTO("someFunction");
            Assert.IsNotNull(evaluationFunctionTo);
            Assert.AreEqual("someFunction", evaluationFunctionTo.Function);
        }

        [TestMethod]
        public void LoopedIndexIterator_ShouldHaveConstructor()
        {
            var indexIterator = new LoopedIndexIterator(2,1);
            Assert.IsNotNull(indexIterator);
            Assert.IsTrue(indexIterator.HasMore());
        }

        [TestMethod]
        public void LoopedIndexIterator_MaxIndex_ShouldReturn2()
        {
            var indexIterator = new LoopedIndexIterator(2,1);
            Assert.IsNotNull(indexIterator);
            Assert.AreEqual(2, indexIterator.MaxIndex());
        }
        [TestMethod]
        public void ListOfIndex_ShouldHaveConstructor()
        {
            var indexes = new List<int> {1,2};
            var listOfIndex = new ListOfIndex(indexes);
            Assert.IsNotNull(listOfIndex);
            var maxIndex = listOfIndex.GetMaxIndex();
            Assert.AreEqual(2, maxIndex);
        }

        [TestMethod]
        public void ListOfIndex_MaxIndex_ShouldReturn2()
        {
            var indexes = new List<int> { 1, 2 };
            var listOfIndex = new ListOfIndex(indexes);
            Assert.IsNotNull(listOfIndex);
            var count = listOfIndex.Count();
            Assert.AreEqual(2, count);
        }
    }
}
