using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using Dev2.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Collections
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ObservableReadOnlyListTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ObservableReadOnlyList_IReadOnlyList")]
        public void ObservableReadOnlyList_IReadOnlyList_Implemented()
        {
            //------------Setup for test--------------------------
            var observableReadOnlyList = new ObservableReadOnlyList<string>();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(observableReadOnlyList, typeof(IReadOnlyList<string>));

            observableReadOnlyList.Insert(0, "Item1");
            observableReadOnlyList.Insert(1, "Item3");
            observableReadOnlyList.Insert(1, "Item2");

            Assert.AreEqual(3, observableReadOnlyList.Count);
            Assert.AreEqual("Item1", observableReadOnlyList[0]);
            Assert.AreEqual("Item2", observableReadOnlyList[1]);
            Assert.AreEqual("Item3", observableReadOnlyList[2]);

            var idx = observableReadOnlyList.IndexOf("Item3");
            Assert.AreEqual(2, idx);

            observableReadOnlyList.RemoveAt(1);
            Assert.AreEqual(2, observableReadOnlyList.Count);
            Assert.AreEqual("Item1", observableReadOnlyList[0]);
            Assert.AreEqual("Item3", observableReadOnlyList[1]);

            observableReadOnlyList[1] = "Item5";
            Assert.AreEqual("Item5", observableReadOnlyList[1]);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ObservableReadOnlyList_IReadOnlyCollection")]
        public void ObservableReadOnlyList_IReadOnlyCollection_Implemented()
        {
            //------------Setup for test--------------------------
            var observableReadOnlyList = new ObservableReadOnlyList<string>();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(observableReadOnlyList, typeof(IReadOnlyCollection<string>));
            Assert.IsTrue(observableReadOnlyList.IsReadOnly);

            observableReadOnlyList.Add("Item1");
            observableReadOnlyList.Add("Item2");
            observableReadOnlyList.Add("Item3");

            Assert.AreEqual(3, observableReadOnlyList.Count);
            Assert.AreEqual("Item1", observableReadOnlyList[0]);
            Assert.AreEqual("Item2", observableReadOnlyList[1]);
            Assert.AreEqual("Item3", observableReadOnlyList[2]);

            var contains = observableReadOnlyList.Contains("Item3");
            var notContains = observableReadOnlyList.Contains("Item4");
            Assert.IsTrue(contains);
            Assert.IsFalse(notContains);

            observableReadOnlyList.Remove("Item2");
            Assert.AreEqual(2, observableReadOnlyList.Count);
            Assert.AreEqual("Item1", observableReadOnlyList[0]);
            Assert.AreEqual("Item3", observableReadOnlyList[1]);

            var array = new string[3];
            observableReadOnlyList.CopyTo(array, 1);

            Assert.IsNull(array[0]);
            Assert.AreEqual("Item1", array[1]);
            Assert.AreEqual("Item3", array[2]);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ObservableReadOnlyList_IEnumerable")]
        public void ObservableReadOnlyList_IEnumerable_Implemented()
        {
            //------------Setup for test--------------------------
            var observableReadOnlyList = new ObservableReadOnlyList<string>(new List<string> { "Item1", "Item2", "Item3" });

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(observableReadOnlyList, typeof(IEnumerable<string>));

            var enumeratorCount = 0;
            var enumerator = observableReadOnlyList.GetEnumerator();
            while(enumerator.MoveNext())
            {
                Assert.AreEqual("Item" + ++enumeratorCount, enumerator.Current);
            }

            Assert.AreEqual(3, enumeratorCount);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ObservableReadOnlyList_INotifyCollectionChanged")]
        public void ObservableReadOnlyList_INotifyCollectionChanged_Implemented()
        {
            //------------Setup for test--------------------------
            var observableReadOnlyList = new ObservableReadOnlyList<string>(new List<string> { "Item1", "Item2", "Item3" });

            var collectionChanged = false;
            observableReadOnlyList.CollectionChanged += (sender, args) => collectionChanged = true;

            //------------Execute Test---------------------------
            observableReadOnlyList.Add("item4");

            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(observableReadOnlyList, typeof(INotifyCollectionChanged));
            Assert.IsTrue(collectionChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ObservableReadOnlyList_CollectionChanged")]
        public void ObservableReadOnlyList_CollectionChanged_BoundToCollectionViewAndModifiedFromNonDispatcherThread_DoesNotThrowException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            VerifyCollectionChangedBoundToCollectionViewDoesNotThrowException(list => list.Clear());
            VerifyCollectionChangedBoundToCollectionViewDoesNotThrowException(list => list.Add("item3"));
            VerifyCollectionChangedBoundToCollectionViewDoesNotThrowException(list => list.Insert(0, "item4"));
            VerifyCollectionChangedBoundToCollectionViewDoesNotThrowException(list => list.RemoveAt(0));
            VerifyCollectionChangedBoundToCollectionViewDoesNotThrowException(list => list.Remove(list[0]));

            //------------Assert Results-------------------------
        }


        void VerifyCollectionChangedBoundToCollectionViewDoesNotThrowException(Action<ObservableReadOnlyList<string>> action)
        {
            //------------Setup for test--------------------------

            var otherDone = new ManualResetEventSlim(false);

            //
            // Create list and view on the Dispatcher.CurrentDispatcher thread
            // MUST bind to CollectionView!!
            //
            var observableReadOnlyList = new ObservableReadOnlyList<string> { "item1", "item2" };
            observableReadOnlyList.TestDispatcherFrame = new DispatcherFrame();
            var collectionView = CollectionViewSource.GetDefaultView(observableReadOnlyList);

            //
            // Modify list from another thread
            //
            string exceptionMessage = null;
            var otherThread = new Thread(() =>
            {
                try
                {
                    action(observableReadOnlyList);

                    exceptionMessage = null;
                }
                catch(Exception ex)
                {
                    exceptionMessage = ex.Message;
                }
                finally
                {
                    otherDone.Set();
                }
            });

            //------------Execute Test---------------------------
            otherThread.Start();

            // Wait for thread to finish
            Dispatcher.PushFrame(observableReadOnlyList.TestDispatcherFrame);

            otherDone.Wait();

            //------------Assert Results-------------------------
            Assert.IsNull(exceptionMessage);
        }
    }


}
