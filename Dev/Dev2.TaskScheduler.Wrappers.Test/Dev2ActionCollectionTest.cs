
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;

namespace Dev2.TaskScheduler.Wrappers.Test
{

    /// <summary>
    /// Test thatthe wrappers are passing through the correct values
    /// </summary>
    [TestClass]
    public class Dev2ActionCollectionTest
    {
        private  ActionCollection _nativeInstance;
        private TaskDefinition _nativeTask;
        private TaskService _nativeService;
        private  Mock<ITaskServiceConvertorFactory> _taskServiceConvertorFactory;

        [TestInitialize]
        public void Init()
        {
            _taskServiceConvertorFactory = new Mock<ITaskServiceConvertorFactory>();
            _nativeService = new TaskService();//localhost
            _nativeTask = _nativeService.NewTask();//actually a definition , not an actual task
            _nativeInstance = _nativeTask.Actions;

        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2ActionCollectionTest_Mutate")]
        public void Dev2ActionCollection_Insert_Add()
        {
            var collection = CreateCollection();
            Assert.AreEqual(collection.Count,1);

        }






        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2ActionCollectionTest_utilitymethods")]
        public void Dev2ActionCollection_ContainsType()
        {
            var collection = CreateCollection();
            Assert.AreEqual(collection.Count, 1);
            
            Assert.IsTrue(collection.ContainsType(typeof(ExecAction)));
            Assert.IsFalse(collection.ContainsType(typeof(EmailAction)));
        }


  








        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2ActionCollectionEnumerate")]
        public void Dev2ActionCollection_Enumerate()
        {
            var collection = new Dev2ActionCollection(_taskServiceConvertorFactory.Object, _nativeInstance);
            var nativeAction = new ExecAction("a", "b", "c");
            var actionToAdd = new Dev2ExecAction(_taskServiceConvertorFactory.Object, nativeAction);
            _taskServiceConvertorFactory.Setup(a => a.CreateAction(nativeAction)).Returns(actionToAdd);
            collection.Add(actionToAdd);
            var nativeAction1 = new ExecAction("1", "2", "3");
            var nativeAction2 = new ExecAction("4", "6", "4");
            var actionToAdd1 = new Dev2ExecAction(_taskServiceConvertorFactory.Object, nativeAction1);
            var actionToAdd2 = new Dev2ExecAction(_taskServiceConvertorFactory.Object, nativeAction2);
            _taskServiceConvertorFactory.Setup(a => a.CreateAction(It.IsAny<ExecAction>())).Returns(actionToAdd);
        
            
            collection.Add( actionToAdd1);
            collection.Add( actionToAdd2);
            var e =collection.GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(((ExecAction)e.Current.Instance).Path,"a");
     
            _taskServiceConvertorFactory.Setup(a => a.CreateAction(It.IsAny<ExecAction>())).Returns(actionToAdd1);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(((ExecAction)e.Current.Instance).Path, "1");
          
            _taskServiceConvertorFactory.Setup(a => a.CreateAction(It.IsAny<ExecAction>())).Returns(actionToAdd2);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(((ExecAction)e.Current.Instance).Path, "4");

            Assert.IsFalse(e.MoveNext());
        }

        #region Helpers
        private Dev2ActionCollection CreateCollection()
        {
            var collection = new Dev2ActionCollection(_taskServiceConvertorFactory.Object, _nativeInstance);
            var nativeAction = new ExecAction("a", "b", "c");
            var actionToAdd = new Dev2ExecAction(_taskServiceConvertorFactory.Object, nativeAction);
            _taskServiceConvertorFactory.Setup(a => a.CreateAction(nativeAction)).Returns(actionToAdd);
            collection.Add(actionToAdd);
            return collection;
        }
    #endregion
    }

}
