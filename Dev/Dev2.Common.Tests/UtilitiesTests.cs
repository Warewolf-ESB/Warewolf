/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Moq;
using System.Threading;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class UtilitiesTests
    {
        class Example
        {
            public Example(string name)
            {
                Name = name;
            }
            public string Name { get; private set; }
            public IList<Example> Children { get; set; }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]

        public void Utilities_Flatten_GivenNull()
        {
            var result = Utilities.Flatten<int>(null, (item) => { return new List<int> { item }; });

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        public void Utilities_Flatten_GivenAList()
        {
            var list = new List<Example> {
                new Example("e1") { Children = new List<Example> { new Example("e1e1"), new Example("e1e2") } },
                new Example("e2") { Children = new List<Example> { new Example("e2e1"), new Example("e2e2") } },
            };

            var result = Utilities.Flatten<Example>(list, (item) => { return item.Children; });

            Assert.AreEqual(6, result.Count());
            var resultArray = result.ToArray();
            Assert.AreEqual("e1e1", resultArray[0].Name);
            Assert.AreEqual("e1e2", resultArray[1].Name);
            Assert.AreEqual("e2e1", resultArray[2].Name);
            Assert.AreEqual("e2e2", resultArray[3].Name);
            Assert.AreEqual("e1", resultArray[4].Name);
            Assert.AreEqual("e2", resultArray[5].Name);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        public void Utilities_Flatten_GivenAnArray()
        {
            var list = new Example[] {
                new Example("e1") { Children = new List<Example> { new Example("e1e1"), new Example("e1e2") } },
                new Example("e2") { Children = new List<Example> { new Example("e2e1"), new Example("e2e2") } },
            };

            var result = Utilities.Flatten<Example>(list, (item) => { return item.Children; });

            Assert.AreEqual(6, result.Count());
            var resultArray = result.ToArray();
            Assert.AreEqual("e1e1", resultArray[0].Name);
            Assert.AreEqual("e1e2", resultArray[1].Name);
            Assert.AreEqual("e2e1", resultArray[2].Name);
            Assert.AreEqual("e2e2", resultArray[3].Name);
            Assert.AreEqual("e1", resultArray[4].Name);
            Assert.AreEqual("e2", resultArray[5].Name);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        public void Utilities_DistinctBy()
        {
            var list1 = new List<Example> {
                new Example("e1") { Children = new List<Example> { new Example("some"), new Example("e4") } },
                new Example("e2") { Children = new List<Example> { new Example("e2"), new Example("some") } },
            };

            var list = list1.Flatten((item) => item.Children);


            var result = Utilities.DistinctBy<Example, string>(list, (item) => { return item.Name; });

            var resultArray = result.ToArray();
            Assert.AreEqual(4, resultArray.Length);
            Assert.AreEqual(list1[0].Children[0], resultArray[0]);
            Assert.AreEqual(list1[0].Children[1], resultArray[1]);
            Assert.AreEqual(list1[1].Children[0], resultArray[2]);
            Assert.AreEqual(list1[0], resultArray[3]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        [DoNotParallelize]
        public void Utilities_PerformActionInsideImpersonatedContext_GivenNullPrincipal_ShouldExecuteWithoutImpersonation()
        {
            var executed = false;

            Utilities.PerformActionInsideImpersonatedContext(null, () => { executed = true; });

            Assert.IsTrue(executed);
        }

        class MyWindowsIdentity : WindowsIdentity
        {
            public int ImpersonateCallCount { get; private set; }
            protected MyWindowsIdentity(WindowsIdentity identity) : base(identity)
            {
            }
            public static MyWindowsIdentity New(WindowsIdentity identity)
            {
                return new MyWindowsIdentity(identity);
            }

            public override WindowsImpersonationContext Impersonate()
            {
                ImpersonateCallCount++;
                return base.Impersonate();
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        public void Utilities_PerformActionInsideImpersonatedContext_GivenPrincipal_ShouldExecuteWithImpersonation()
        {
            var executed = false;
            var mockPrincipal = new Mock<IPrincipal>();
            var identity = MyWindowsIdentity.New(WindowsIdentity.GetCurrent());
            mockPrincipal.Setup(o => o.Identity).Returns(identity);

            Utilities.OrginalExecutingUser = mockPrincipal.Object;

            Utilities.PerformActionInsideImpersonatedContext(mockPrincipal.Object, () => { executed = true; });

            mockPrincipal.Verify(o => o.Identity, Times.Exactly(1));
            Assert.IsTrue(executed);

            Assert.AreEqual(1, identity.ImpersonateCallCount);
            Assert.AreEqual(mockPrincipal.Object, Utilities.OrginalExecutingUser);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        public void Utilities_PerformActionInsideImpersonatedContext_GivenPrincipalAlreadyImpersonated_ShouldExecuteWithImpersonation()
        {
            var executed = false;
            var mockPrincipal = new Mock<IPrincipal>();
            var identity = MyWindowsIdentity.New(WindowsIdentity.GetCurrent());
            mockPrincipal.Setup(o => o.Identity).Returns(identity);

            Thread.CurrentPrincipal = mockPrincipal.Object;

            Utilities.OrginalExecutingUser = mockPrincipal.Object;

            Utilities.PerformActionInsideImpersonatedContext(mockPrincipal.Object, () => { executed = true; });

            mockPrincipal.Verify(o => o.Identity, Times.Exactly(1));
            Assert.IsTrue(executed);
            Assert.AreEqual(1, identity.ImpersonateCallCount);

            Assert.AreEqual(mockPrincipal.Object, Utilities.OrginalExecutingUser);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        public void Utilities_PerformActionInsideImpersonatedContext_GivenPrincipal_NullActionDoesNotThrow()
        {
            var mockPrincipal = new Mock<IPrincipal>();

            Utilities.OrginalExecutingUser = mockPrincipal.Object;

            Utilities.PerformActionInsideImpersonatedContext(mockPrincipal.Object, null);

            mockPrincipal.Verify(o => o.Identity, Times.Exactly(1));

            Assert.AreEqual(mockPrincipal.Object, Utilities.OrginalExecutingUser);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        [DoNotParallelize]
        public void Utilities_PerformActionInsideImpersonatedContext_GivenAnonymousPrincipal_ShouldTryServerUser()
        {
            var executed = false;
            var mockPrincipal = new Mock<IPrincipal>();
            var mockServerUserPrincipal = new Mock<IPrincipal>();


            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetAnonymous());

            mockServerUserPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetAnonymous());
            Utilities.ServerUser = mockServerUserPrincipal.Object;

            try
            {
                Utilities.PerformActionInsideImpersonatedContext(mockPrincipal.Object, () => { executed = true; });
                Assert.Fail("Expected exception");
            }
            catch (Exception e)
            {
                Assert.AreEqual("An anonymous identity cannot perform an impersonation.", e.Message);
            }

            mockPrincipal.Verify(o => o.Identity, Times.Exactly(1));
            mockServerUserPrincipal.Verify(o => o.Identity, Times.Once);

            Assert.IsFalse(executed);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        [DoNotParallelize]
        public void Utilities_PerformActionInsideImpersonatedContext_TaskFailureTriesAgain()
        {
            var executed = false;
            var mockPrincipal = new Mock<IPrincipal>();
            var mockServerUserPrincipal = new Mock<IPrincipal>();


            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());

            mockServerUserPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());
            Utilities.ServerUser = mockServerUserPrincipal.Object;

            var shouldThrow = true;

            Utilities.PerformActionInsideImpersonatedContext(mockPrincipal.Object, () => {
                if (shouldThrow)
                {
                    shouldThrow = false;
                    throw new Exception("some exception");
                }
                executed = true;
            });

            mockPrincipal.Verify(o => o.Identity, Times.Exactly(1));
            mockServerUserPrincipal.Verify(o => o.Identity, Times.Once);

            Assert.IsFalse(shouldThrow);
            Assert.IsTrue(executed);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Utilities))]
        public void Utilities_PerformActionInsideImpersonatedContext_TaskFailureTriesAgainAndFailsAgain_ShouldThrow()
        {
            var executedCount = 0;
            var mockPrincipal = new Mock<IPrincipal>();
            var mockServerUserPrincipal = new Mock<IPrincipal>();


            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());

            mockServerUserPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());
            Utilities.ServerUser = mockServerUserPrincipal.Object;

            try
            {
                Utilities.PerformActionInsideImpersonatedContext(mockPrincipal.Object, () => {
                    executedCount++;
                    throw new Exception("some exception");
                });
                Assert.Fail("Expected exception");
            }
            catch (Exception e)
            {
                Assert.AreEqual("some exception", e.Message);
            }

            mockPrincipal.Verify(o => o.Identity, Times.Exactly(1));
            mockServerUserPrincipal.Verify(o => o.Identity, Times.Once);

            Assert.AreEqual(2, executedCount);
        }
    }
}
