using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Warewolf.UI.Tests.Merge
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class MergeConflictsTests
    {
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        public UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        //ExplorerUIMap ExplorerUIMap
        //{
        //    get
        //    {
        //        if (_ExplorerUIMap == null)
        //        {
        //            _ExplorerUIMap = new ExplorerUIMap();
        //        }

        //        return _ExplorerUIMap;
        //    }
        //}

        //private ExplorerUIMap _ExplorerUIMap;

        //WorkflowTabUIMap WorkflowTabUIMap
        //{
        //    get
        //    {
        //        if (_WorkflowTabUIMap == null)
        //        {
        //            _WorkflowTabUIMap = new WorkflowTabUIMap();
        //        }

        //        return _WorkflowTabUIMap;
        //    }
        //}

        //private WorkflowTabUIMap _WorkflowTabUIMap;

        #endregion
    }
}
