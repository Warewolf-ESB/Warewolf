using System.Collections.Generic;
using System.Linq;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Serialization;

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
            Assert.AreEqual("DsfMultiAssignActivity", debugState.ActualType);
            Assert.AreEqual("Assign (1)", debugState.DisplayName);
            debugState = children[1];
            Assert.AreEqual("DsfMultiAssignActivity", debugState.ActualType);
            Assert.AreEqual("Assign (1)", debugState.DisplayName);
            Assert.IsTrue(children.Any(state => !state.Children.Any()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildTree_GivenStatesWithForEach_ShouldAddChildrenCorrectly()
        {
            //---------------Set up test pack-------------------
            var fetch = JsonResource.Fetch("ForEachWorkFlow");
            var debugStates = fetch.DeserializeToObject<List<IDebugState>>();
            Assert.AreEqual(5, debugStates.Count);
            var buildTree = DebugStateTreeBuilder.BuildTree(debugStates).ToList();
            //---------------Assert Precondition----------------
            Assert.IsTrue(buildTree.Any(state => state.Children.Any()));
            //---------------Execute Test ----------------------
            var children = buildTree[1].Children;
            Assert.AreEqual(1, children.Count);
            //---------------Test Result -----------------------
            var debugState = children[0];
            Assert.AreEqual("DsfMultiAssignActivity", debugState.ActualType);

            Assert.AreEqual("Assign (1)", debugState.DisplayName);
            Assert.IsTrue(children.Any(state => !state.Children.Any()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildTree_GivenStatesWithNestesForEach_ShouldAddChildrenCorrectly()
        {
            //---------------Set up test pack-------------------
            var fetch = JsonResource.Fetch("NestedForEachWorkFlow");
            var debugStates = fetch.DeserializeToObject<List<IDebugState>>();
            Assert.AreEqual(7, debugStates.Count);
            var buildTree = DebugStateTreeBuilder.BuildTree(debugStates).ToList();
            //---------------Assert Precondition----------------
            Assert.IsTrue(buildTree.Any(state => state.Children.Any()));
            
            //---------------Execute Test ----------------------
            var children = buildTree[1].Children;
            Assert.AreEqual(2, children.Count);
            var count = children.Count(state => state.DisplayName == "For Each");
            //---------------Test Result -----------------------
            Assert.AreEqual(2, count);

            var firstGrandChild = children[0].Children;
            var secondGrandChild = children[1].Children;
            Assert.AreEqual("Assign (1)", firstGrandChild.Single().DisplayName);
            Assert.AreEqual("Assign (1)", secondGrandChild.Single().DisplayName);

        }

    }
}
