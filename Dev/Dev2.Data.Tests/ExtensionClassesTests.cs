using System.Collections.Generic;
using Dev2.Data.Binary_Objects;
using Dev2.Data.MathOperations;
using Dev2.Data.SystemTemplates;
using Dev2.DataList.Contract;
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
        public void GivenResourceName_ResourceForTree_ToString_ShouldReturtnResourceName()
        {
            var resourceForTree = new ResourceForTree();
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
