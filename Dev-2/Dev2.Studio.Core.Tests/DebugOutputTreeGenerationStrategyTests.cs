using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Factory;
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
        private List<IDebugState> _emptyExistingContent = null;

        private ObservableCollection<DebugTreeViewItemViewModel> _testRootItems = null;
        private List<IDebugState> _testExistingContent = null;

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
            _emptyExistingContent = new List<IDebugState>();


            _testRootItems = new ObservableCollection<DebugTreeViewItemViewModel>();
            _testExistingContent = new List<IDebugState>();

            DebugState DebugState1 = new DebugState
                {
                    ID = new Guid(new byte[16] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
                    ParentID = new Guid(new byte[16] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0})
                };
            DebugStateTreeViewItemViewModel _testTreeRoot1 = new DebugStateTreeViewItemViewModel(null, DebugState1, null);
            _testRootItems.Add(_testTreeRoot1);
            _testExistingContent.Add(DebugState1);

            DebugState DebugState1_1 = new DebugState
            {
                ID = new Guid(new byte[16] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
                ParentID = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })
            };
            DebugStateTreeViewItemViewModel _testTree1_1 = new DebugStateTreeViewItemViewModel(null, DebugState1_1, null);
            _testTreeRoot1.Children.Add(_testTree1_1);
            _testExistingContent.Add(DebugState1_1);

            DebugState DebugState1_2 = new DebugState
            {
                ID = new Guid(new byte[16] { 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
                ParentID = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })
            };
            DebugStateTreeViewItemViewModel _testTree1_2 = new DebugStateTreeViewItemViewModel(null, DebugState1_2, null);
            _testTreeRoot1.Children.Add(_testTree1_2);
            _testExistingContent.Add(DebugState1_2);

            DebugState DebugState2 = new DebugState
            {
                ID = new Guid(new byte[16] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
                ParentID = new Guid(new byte[16] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })
            };
            DebugStateTreeViewItemViewModel _testTreeRoot2 = new DebugStateTreeViewItemViewModel(null, DebugState2, null);
            _testRootItems.Add(_testTreeRoot2);
            _testExistingContent.Add(DebugState2);

            DebugState DebugState2_1 = new DebugState
            {
                ID = new Guid(new byte[16] { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
                ParentID = new Guid(new byte[16] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })
            };
            DebugStateTreeViewItemViewModel _testTree2_1 = new DebugStateTreeViewItemViewModel(null, DebugState2_1, null);
            _testTreeRoot2.Children.Add(_testTree2_1);
            _testExistingContent.Add(DebugState2_1);

            DebugState DebugState2_2 = new DebugState
            {
                ID = new Guid(new byte[16] { 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
                ParentID = new Guid(new byte[16] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })
            };
            DebugStateTreeViewItemViewModel _testTree2_2 = new DebugStateTreeViewItemViewModel(null, DebugState2_2, null);
            _testTreeRoot2.Children.Add(_testTree2_2);
            _testExistingContent.Add(DebugState2_2);

            //MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
            
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
            var state = DebugStateFactory.Create("cake", null);
            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy
                .PlaceContentInTree(_emptyRootItems, _emptyExistingContent, state, "moo", false, 0); ;

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsString_Expected_ItemAtRoot()
        {
            var state = DebugStateFactory.Create("cake", null);
            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy
                .PlaceContentInTree(_emptyRootItems, _emptyExistingContent, state, "", false, 0); ;

            CollectionAssert.Contains(_emptyRootItems, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_IDIsEmpty_Expected_ItemAtRoot()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            _emptyExistingContent.Add(content);

            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, content, "", false, 0); ;

            CollectionAssert.Contains(_emptyRootItems, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentIDIsEmpty_Expected_ItemAtRoot()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            _emptyExistingContent.Add(content);

            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, content, "", false, 0); ;

            CollectionAssert.Contains(_emptyRootItems, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentIDIsEqualToID_Expected_ItemAtRoot()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            _emptyExistingContent.Add(content);

            DebugTreeViewItemViewModel actual = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _emptyExistingContent, content, "", false, 0); ;

            CollectionAssert.Contains(_emptyRootItems, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentExists_Expected_ItemAddedToParent()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 0);
            var expected = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            var actual = ((IDebugState)((DebugStateTreeViewItemViewModel)newNode.Parent).Content).ID;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentDoesntExists_Expected_ItemAddedToParent()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _testExistingContent, content, "", false, 0);
            var expected = new Guid(new byte[16] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            var actual = ((IDebugState)((DebugStateTreeViewItemViewModel)newNode.Parent).Content).ID;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_And_ParentDoesntExist_Expected_AllNecessaryParentsCreated()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_emptyRootItems, _testExistingContent, content, "", false, 0);
            var expected = "00000201-0000-0000-0000-00000000000000000001-0000-0000-0000-000000000000";
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
            content.ID = new Guid(new byte[16] { 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 1);

            Assert.IsNull(newNode);
        }

        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugState_DepthLimitInPlaceButNotExceeded_Expected_ItemAddedToParent()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 3);

            Assert.IsNotNull(newNode);
        }

        ///
        /// Juries - Bug 8469
        /// 
        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugStated_WithoutError_Expected_NoErrors()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.HasError = false;

            _testExistingContent.Add(content);
            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 3);

            Assert.IsTrue(newNode.HasError == false);
            Assert.IsTrue(newNode.Parent.HasError == false);
        }


        ///
        /// Juries - Bug 8469
        /// 
        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugStated_WithError_Expected_ItemTrueError()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.HasError = true;

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 3);

            Assert.IsTrue(newNode.HasError == true);
        }

        ///
        /// Juries - Bug 8469
        /// 
        [TestMethod]
        public void PlaceContentInTree_Where_ContentIsDebugStated_WithError_Expected_PraentTreeNullError()
        {
            DebugState content = new DebugState();
            content.ID = new Guid(new byte[16] { 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content.ParentID = new Guid(new byte[16] { 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            DebugState content2 = new DebugState();
            content2.ID = new Guid(new byte[16] { 2, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content2.ParentID = new Guid(new byte[16] { 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            content2.HasError = true;

            _testExistingContent.Add(content);

            DebugTreeViewItemViewModel newNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content, "", false, 3);
            DebugTreeViewItemViewModel newestNode = _debugOutputTreeGenerationStrategy.PlaceContentInTree(_testRootItems, _testExistingContent, content2, "", false, 3);

            Assert.IsTrue(newestNode.Parent.HasError == null);
            Assert.IsTrue(newestNode.Parent.Parent.HasError == null);
        }
        #endregion Tests
    }
}
