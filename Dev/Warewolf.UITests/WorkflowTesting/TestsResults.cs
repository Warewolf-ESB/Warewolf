using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class TestResults
    {
        public static void GetSelectedTestPendingResult(WpfListItem test, int instance = 1)
        {
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    Assert.IsNotNull(test2);
                    var isPending2 = test2.Pending.Width != 1 && test2.Pending.Width != 1 && test2.Pending.Width != 1 && test2.Pending.Width != 1;
                    Assert.IsTrue(isPending2, "Test 2 status is not set to Pending");
                    break;
                case 3:
                    var test3 = test as Test3;
                    Assert.IsNotNull(test3);
                    var isPending3 = test3.Pending.Width != 1 && test3.Pending.Width != 1 && test3.Pending.Width != 1 && test3.Pending.Width != 1;
                    Assert.IsTrue(isPending3, "Test 3 status is not set to Pending");
                    break;
                case 4:
                    var test4 = test as Test4;
                    Assert.IsNotNull(test4);
                    var isPending4 = test4.Pending.Width != 1 && test4.Pending.Width != 1 && test4.Pending.Width != 1 && test4.Pending.Width != 1;
                    Assert.IsTrue(isPending4, "Test 4 status is not set to Pending");
                    break;
                default:
                    var test1 = test as Test1;
                    Assert.IsNotNull(test1);
                    var isPending = test1.Pending.Width != 1 && test1.Pending.Width != 1 && test1.Pending.Width != 1 && test1.Pending.Width != 1;
                    Assert.IsTrue(isPending, "Test 1 status is not set to Pending");
                    break;
            }
        }

        public static void GetSelectedTestInvalidResult(WpfListItem test, int instance = 1)
        {
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    Assert.IsNotNull(test2);
                    var isInvalid2 = test2.Invalid.Width != 1 && test2.Invalid.Height != 1 && test2.Invalid.Left != 1 && test2.Invalid.Top != 1;
                    Assert.IsTrue(isInvalid2, "Test 2 status is not set to Invalid");
                    break;
                case 3:
                    var test3 = test as Test3;
                    Assert.IsNotNull(test3);
                    var isInvalid3 = test3.Invalid.Width != 1 && test3.Invalid.Height != 1 && test3.Invalid.Left != 1 && test3.Invalid.Top != 1;
                    Assert.IsTrue(isInvalid3, "Test 3 status is not set to Invalid");
                    break;
                case 4:
                    var test4 = test as Test4;
                    Assert.IsNotNull(test4);
                    var isInvalid4 = test4.Invalid.Width != 1 && test4.Invalid.Height != 1 && test4.Invalid.Left != 1 && test4.Invalid.Top != 1;
                    Assert.IsTrue(isInvalid4, "Test 4 status is not set to Invalid");
                    break;
                default:
                    var test1 = test as Test1;
                    Assert.IsNotNull(test1);
                    var isInvalid = test1.Invalid.Width != 1 && test1.Invalid.Height != 1 && test1.Invalid.Left != 1 && test1.Invalid.Top != 1;
                    Assert.IsTrue(isInvalid, "Test 1 status is not set to Invalid");
                    break;
            }
        }
        public static void GetSelectedTestPassingResult(WpfListItem test,  int instance = 1)
        {
            if (instance == 2)
            {
                var test2 = test as Test2;
                Assert.IsNotNull(test2);
                var isPassing = test2.Passing.Width != 1 && test2.Passing.Height != 1 && test2.Passing.Left != 1 && test2.Passing.Top != 1;
                Assert.IsTrue(isPassing, "Test 2 status is not set to Passing");
            }
            if (instance == 3)
            {
                var test3 = test as Test3;
                Assert.IsNotNull(test3);
                var isPassing = test3.Passing.Width != 1 && test3.Passing.Height != 1 && test3.Passing.Left != 1 && test3.Passing.Top != 1;
                Assert.IsTrue(isPassing, "Test 2 status is not set to Passing");
            }
            if (instance == 4)
            {
                var test4 = test as Test4;
                Assert.IsNotNull(test4);
                var isPassing = test4.Passing.Width != 1 && test4.Passing.Height != 1 && test4.Passing.Left != 1 && test4.Passing.Top != 1;
                Assert.IsTrue(isPassing, "Test 2 status is not set to Passing");
            }
            if (instance == 1)
            {
                var test1 = test as Test1;
                Assert.IsNotNull(test1);
                var isPassing = test1.Passing.Width != 1 && test1.Passing.Height != 1 && test1.Passing.Left != 1 && test1.Passing.Top != 1;
                Assert.IsTrue(isPassing, "Test 2 status is not set to Passing");
            }
        }

        public static void GetSelectedTestFailingResult(WpfListItem test, int instance)
        {
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    Assert.IsNotNull(test2);
                    var clickablePoint = test2.Failing.Width != 1 && test2.Failing.Height != 1 && test2.Failing.Left != 1 && test2.Failing.Top != 1;
                    Assert.IsNotNull(clickablePoint, "Test 2 status is not set to Failing");
                    break;
                case 3:
                    var test3 = test as Test3;
                    Assert.IsNotNull(test3);
                    var clickablePoint3 = test3.Failing.Width != 1 && test3.Failing.Height != 1 && test3.Failing.Left != 1 && test3.Failing.Top != 1;
                    Assert.IsNotNull(clickablePoint3, "Test 3 status is not set to Failing");
                    break;
                case 4:
                    var test4 = test as Test4;
                    Assert.IsNotNull(test4);
                    var clickablePoint4 = test4.Failing.Width != 1 && test4.Failing.Height != 1 && test4.Failing.Left != 1 && test4.Failing.Top != 1;
                    Assert.IsNotNull(clickablePoint4, "Test 3 status is not set to Failing");
                    break;
                default:
                    var test1 = test as Test1;
                    Assert.IsNotNull(test1);
                    var isFailing = test1.Failing.Width != 1 && test1.Failing.Height != 1 && test1.Failing.Left != 1 && test1.Failing.Top != 1;
                    Assert.IsNotNull(isFailing, "Test 3 status is not set to Failing");
                    break;
            }
        }
    }
}
