using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.ViewModels.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DebugOutputTreeGenerationStrategyTests
    {
        #region Class Members

        private static DebugOutputTreeGenerationStrategy _debugOutputTreeGenerationStrategy = new DebugOutputTreeGenerationStrategy();
        private static ImportServiceContext _importContext;

        private ObservableCollection<DebugTreeViewItemViewModel> _emptyRootItems = null;
        private List<object> _emptyExistingContent = null;

        private ObservableCollection<DebugTreeViewItemViewModel> _testRootItems = null;
        private List<object> _testExistingContent = null;

        #endregion Class Members

        #region Initialization


        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            _importContext = CompositionInitializer.DefaultInitialize();
        }
        
        [TestInitialize()]
        public void TestInitialize()
        {
            _emptyRootItems = new ObservableCollection<DebugTreeViewItemViewModel>();
            _emptyExistingContent = new List<object>();


            _testRootItems = new ObservableCollection<DebugTreeViewItemViewModel>();
            _testExistingContent = new List<object>();

            DebugState DebugState1 = new DebugState { ID = "1", ParentID = "1" };
            DebugStateTreeViewItemViewModel _testTreeRoot1 = new DebugStateTreeViewItemViewModel(null, DebugState1, null);
            _testRootItems.Add(_testTreeRoot1);
            _testExistingContent.Add(DebugState1);

            DebugState DebugState1_1 = new DebugState { ID = "1_1", ParentID = "1" };
            DebugStateTreeViewItemViewModel _testTree1_1 = new DebugStateTreeViewItemViewModel(null, DebugState1_1, null);
            _testTreeRoot1.Children.Add(_testTree1_1);
            _testExistingContent.Add(DebugState1_1);

            DebugState DebugState1_2 = new DebugState { ID = "1_2", ParentID = "1" };
            DebugStateTreeViewItemViewModel _testTree1_2 = new DebugStateTreeViewItemViewModel(null, DebugState1_2, null);
            _testTreeRoot1.Children.Add(_testTree1_2);
            _testExistingContent.Add(DebugState1_2);

            DebugState DebugState2 = new DebugState { ID = "2", ParentID = "2" };
            DebugStateTreeViewItemViewModel _testTreeRoot2 = new DebugStateTreeViewItemViewModel(null, DebugState2, null);
            _testRootItems.Add(_testTreeRoot2);
            _testExistingContent.Add(DebugState2);

            DebugState DebugState2_1 = new DebugState { ID = "2_1", ParentID = "2" };
            DebugStateTreeViewItemViewModel _testTree2_1 = new DebugStateTreeViewItemViewModel(null, DebugState2_1, null);
            _testTreeRoot2.Children.Add(_testTree2_1);
            _testExistingContent.Add(DebugState2_1);

            DebugState DebugState2_2 = new DebugState { ID = "2_2", ParentID = "2" };
            DebugStateTreeViewItemViewModel _testTree2_2 = new DebugStateTreeViewItemViewModel(null, DebugState2_2, null);
            _testTreeRoot2.Children.Add(_testTree2_2);
            _testExistingContent.Add(DebugState2_2);

            MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
            
        }

        #endregion Initialization

        #region Tests

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsNull_Expected_Null()
        {
            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, null, "", false, 0); ;

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentFailsFilter_Expected_Null()
        {
            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, "cake", "moo", false, 0); ;

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsString_Expected_ItemAtRoot()
        {
            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, "cake", "", false, 0); ;

            CollectionAssert.Contains(_emptyRootItems, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_IDIsEmpty_Expected_ItemAtRoot()
        {
            DebugState content = new DebugState();
            content.ID = "";
            content.ParentID = "";

            _emptyExistingContent.Add(content);

            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, content, "", false, 0); ;

            CollectionAssert.Contains(_emptyRootItems, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentIDIsEmpty_Expected_ItemAtRoot()
        {
            DebugState content = new DebugState();
            content.ID = "1";
            content.ParentID = "";

            _emptyExistingContent.Add(content);

            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, content, "", false, 0); ;

            CollectionAssert.Contains(_emptyRootItems, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentIDIsEqualToID_Expected_ItemAtRoot()
        {
            DebugState content = new DebugState();
            content.ID = "1";
            content.ParentID = "1";

            _emptyExistingContent.Add(content);

            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, content, "", false, 0); ;

            CollectionAssert.Contains(_emptyRootItems, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentExists_Expected_ItemAddedToParent()
        {
            DebugState content = new DebugState();
            content.ID = "1_3";
            content.ParentID = "1";

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 0);
            string expected = "1";
            string actual = ((IDebugState)((DebugStateTreeViewItemViewModel)newNode.Parent).Content).ID;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentDoesntExists_Expected_ItemAddedToParent()
        {
            DebugState content = new DebugState();
            content.ID = "1_3";
            content.ParentID = "1";

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _testExistingContent, content, "", false, 0);
            string expected = "1";
            string actual = ((IDebugState)((DebugStateTreeViewItemViewModel)newNode.Parent).Content).ID;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentDoesntExist_Expected_AllNecessaryParentsCreated()
        {
            DebugState content = new DebugState();
            content.ID = "1_2_1";
            content.ParentID = "1_2";

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _testExistingContent, content, "", false, 0);
            string expected = "1_21";
            string actual = "";
            
            DebugTreeViewItemViewModel current = newNode.Parent;
            while (current != null)
            {
                actual += ((IDebugState)((DebugStateTreeViewItemViewModel)current).Content).ID;
                current = current.Parent;
            }

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_DepthLimitIsExceeded_Expected_ItemAddedToParent()
        {
            DebugState content = new DebugState();
            content.ID = "1_2_1";
            content.ParentID = "1_2";

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 1);

            Assert.IsNull(newNode);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_DepthLimitInPlaceButNotExceeded_Expected_ItemAddedToParent()
        {
            DebugState content = new DebugState();
            content.ID = "1_2_1";
            content.ParentID = "1_2";

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 3);

            Assert.IsNotNull(newNode);
        }

        #endregion Tests
    }
}
