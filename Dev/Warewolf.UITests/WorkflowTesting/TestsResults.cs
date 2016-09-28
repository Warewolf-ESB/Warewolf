using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                    Assert.IsTrue(test2.Pending.Exists, "Test 2 status is not set to Pending");
                    break;
                case 3:
                    var test3 = test as Test3;
                    Assert.IsNotNull(test3);
                    Assert.IsTrue(test3.Pending.Exists, "Test 3 status is not set to Pending");
                    break;
                default:
                    var test1 = test as Test1;
                    Assert.IsNotNull(test1);
                    Assert.IsTrue(test1.Pending.Exists, "Test 1 status is not set to Pending");
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
                    Assert.IsTrue(test2.Invalid.Exists, "Test 2 status is not set to Invalid");
                    break;
                case 3:
                    var test3 = test as Test3;
                    Assert.IsNotNull(test3);
                    Assert.IsTrue(test3.Invalid.Exists, "Test 2 status is not set to Invalid");
                    break;
                default:
                    var test1 = test as Test1;
                    Assert.IsNotNull(test1);
                    Assert.IsTrue(test1.Invalid.Exists, "Test 1 status is not set to Invalid");
                    break;
            }
        }
        public static void GetSelectedTestPassingResult(WpfListItem test, int instance = 1)
        {
            if (instance == 2)
            {
                var test2 = test as Test2;
                Assert.IsNotNull(test2);
                Assert.IsTrue(test2.Passing.Exists, "Test 2 status is not set to Passing");
            }
            if (instance == 3)
            {
                var test3 = test as Test3;
                Assert.IsNotNull(test3);
                Assert.IsTrue(test3.Passing.Exists, "Test 3 status is not set to Passing");
            }
            if (instance == 1)
            {
                var test1 = test as Test1;
                Assert.IsNotNull(test1);
                Assert.IsTrue(test1.Passing.Exists, "Test 1 status is not set to Passing");
            }            
        }

        public static void GetSelectedTestFailingResult(WpfListItem test, int instance)
        {
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    Assert.IsNotNull(test2);
                    Assert.IsTrue(test2.Failing.Exists, "Test 2 status is not set to Failing");
                    break;
                case 3:
                    var test3 = test as Test3;
                    Assert.IsNotNull(test3);
                    Assert.IsTrue(test3.Failing.Exists, "Test 3 status is not set to Failing");
                    break;
                default:
                    var test1 = test as Test1;
                    Assert.IsNotNull(test1);
                    Assert.IsTrue(test1.Failing.Exists, "Test 1 status is not set to Failing");
                    break;
            }
        }
    }
}
