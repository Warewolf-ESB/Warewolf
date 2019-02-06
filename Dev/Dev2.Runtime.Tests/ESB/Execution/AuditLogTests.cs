using System;
using Dev2.Common.Container;
using Dev2.Runtime.ESB.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class AuditLogTests
    {
        [TestMethod]
        public void AuditLog_Equals_ByField()
        {
            var expected = new AuditLog();
            var actual = new AuditLog();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AuditLog_GetHashCode()
        {
            var expected = new AuditLog();
            var actual = new AuditLog();
            Assert.AreEqual(expected, actual);
            actual.AuditType = "AuditType";
            Assert.AreNotEqual(expected, actual);
            expected.AuditType = "AuditType";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AuditLog_Can_Pass_Through_WarewolfQueue()
        {
            using (var queue = new WarewolfQueue())
            {
                using (var session = queue.OpenSession())
                {
                    var expected = new AuditLog();
                    session.Enqueue(expected);
                    session.Flush();
                    var actual = session.Dequeue<AuditLog>();
                    session.Flush();
                    Assert.AreEqual(expected, actual);
                }
            }
        }
    }
}
