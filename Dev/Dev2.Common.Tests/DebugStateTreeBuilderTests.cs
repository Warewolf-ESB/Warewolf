using System.Collections.Generic;
using System.Linq;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class DebugStateTreeBuilderTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenIsNew()
        {
            //---------------Set up test pack-------------------
            var builder = new DebugStateTreeBuilder();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(builder);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildTree_GivenStates_ShouldReturnStates()
        {
            //---------------Set up test pack-------------------
            var fetch = JsonResource.Fetch("FlatStates");
            var debugStates = fetch.DeserializeToObject<List<IDebugState>>();
            //---------------Execute Test ----------------------
            var buildTree = DebugStateTreeBuilder.BuildTree(debugStates);
            //---------------Test Result -----------------------
            Assert.IsNotNull(buildTree);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildTree_GivenStates_ShouldTreeWithChildren()
        {
            //---------------Set up test pack-------------------
            var fetch = JsonResource.Fetch("FlatStates");
            var debugStates = fetch.DeserializeToObject<List<IDebugState>>();
            Assert.AreEqual(6, debugStates.Count());
            var buildTree = DebugStateTreeBuilder.BuildTree(debugStates).ToList();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(buildTree);
            //---------------Execute Test ----------------------
            var states = buildTree.ToList();
            Assert.AreEqual(3, states.Count);
            //---------------Test Result -----------------------
            Assert.IsTrue(buildTree.Any(state => state.Children.Any()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildTree_GivenStates_ShouldAddChildrenCorrectly()
        {
            //---------------Set up test pack-------------------
            var fetch = JsonResource.Fetch("FlatStates");
            var debugStates = fetch.DeserializeToObject<List<IDebugState>>();
            Assert.AreEqual(6, debugStates.Count());
            var buildTree = DebugStateTreeBuilder.BuildTree(debugStates).ToList();
            //---------------Assert Precondition----------------
            Assert.IsTrue(buildTree.Any(state => state.Children.Any()));
            //---------------Execute Test ----------------------
            var children = buildTree[1].Children;
            Assert.AreEqual(2, children.Count);
            //---------------Test Result -----------------------
            var debugState = children[0];
            var debugState1 = children[1];
            Assert.AreEqual("DsfMultiAssignActivity", debugState.ActualType);
            
            Assert.AreEqual("DsfMultiAssignActivity", debugState1.ActualType);
            Assert.AreEqual("Assign (1)", debugState.DisplayName);
            Assert.AreEqual("Assign (1)", debugState1.DisplayName);
            Assert.IsTrue(children.Any(state => !state.Children.Any()));
        }
    }
}
