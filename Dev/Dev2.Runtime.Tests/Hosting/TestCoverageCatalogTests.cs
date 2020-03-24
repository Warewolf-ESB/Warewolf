/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class TestCoverageCatalogTests
    {
        [TestInitialize]
        public void Setup()
        {
            var workflowNodes = GetWorkflowNodes();

            var mockWorkflowBuilder = new Mock<IWorkflowBuilder>();
            mockWorkflowBuilder.Setup(o => o.WorkflowId).Returns(_wfId);
            mockWorkflowBuilder.Setup(o => o.Name).Returns("wf-send-cached-client-an-email");
            mockWorkflowBuilder.Setup(o => o.GetAllWorkflowNodes()).Returns(workflowNodes);

            CustomContainer.Register<IWorkflowBuilder>(mockWorkflowBuilder.Object);
        }

        private static Guid _wfId = Guid.Parse("99c23a82-aaf8-46a5-8746-4ff2d251daf2");

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("TestCoverageCatalog_Intergration")]
        public void TestCoverageCatalog_GenerateAllTestsCoverage_ExpectFullCoverage()
        {
            var tests = GetTests();

            var sut = new TestCoverageCatalog();

            var coverage = sut.GenerateAllTestsCoverage(_wfId, tests);

            Assert.AreEqual(100, coverage.CoveragePercentage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("TestCoverageCatalog_Intergration")]
        public void TestCoverageCatalog_GenerateAllTestsCoverage_ExpectPartialCoverage()
        {
            var test = GetFalseBranchTest();

            var sut = new TestCoverageCatalog();

            var coverage = sut.GenerateSingleTestCoverage(_wfId, test);

            Assert.AreEqual(50, coverage.CoveragePercentage);
        }

        private IServiceTestModelTO GetFalseBranchTest()
        {
            return _false_branchTest;
        }

        private List<IWorkflowNode> GetWorkflowNodes()
        {
            return new List<IWorkflowNode>
            {
                new WorkflowNode
                {
                    ActivityID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                    UniqueID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                    StepDescription = "Assign(input)",
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                    UniqueID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                    StepDescription = "If [[data_ttl_expired]] Is = false",
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                    UniqueID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                    StepDescription = "Assign(error)",
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                    UniqueID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                    StepDescription = "SQL(get person)"
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("1639df9f-0a29-4481-8295-96466693c5a8"),
                    UniqueID = Guid.Parse("1639df9f-0a29-4481-8295-96466693c5a8"),
                    StepDescription = "Assign(sql person)",
                },

                new WorkflowNode
                {
                    ActivityID = Guid.Parse("e886b1c0-0075-4637-9c4f-dacb366cd6fb"),
                    UniqueID = Guid.Parse("e886b1c0-0075-4637-9c4f-dacb366cd6fb"),
                    StepDescription = "Email(sql person)",
                }
            };
        }


        private IServiceTestModelTO _false_branchTest = new ServiceTestModelTO
        {
            ResourceId = _wfId,
            TestName = "False branch test",
            Password = "p@ssw0rd",

            TestSteps = new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                        UniqueID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                        StepDescription = "Assign(input)",
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        UniqueID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        StepDescription = "If [[data_ttl_expired]] Is = false",
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                        UniqueID = Guid.Parse("75cecc31-3ab3-4f68-8348-c30dfa3c04fc"),
                        StepDescription = "Assign(error)",
                    }
                },
        };

        private IServiceTestModelTO _true_branchtest = new ServiceTestModelTO
        {
            ResourceId = _wfId,
            TestName = "True branch test",
            Password = "p@ssw0rd",
            TestSteps = new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                        UniqueID = Guid.Parse("ef7dba5d-865d-4762-991b-b7451ccff225"),
                        StepDescription = "Assign(input)",
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        UniqueID = Guid.Parse("f8ac128c-6715-428e-8ba9-447cb0ec1fe3"),
                        StepDescription = "If [[data_ttl_expired]] Is = true",
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                        UniqueID = Guid.Parse("d32cb8ef-cc3c-4044-a6e0-df3d16f91281"),
                        StepDescription = "SQL(get person)"
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("1639df9f-0a29-4481-8295-96466693c5a8"),
                        UniqueID = Guid.Parse("1639df9f-0a29-4481-8295-96466693c5a8"),
                        StepDescription = "Assign(sql person)",
                    },

                    new ServiceTestStepTO
                    {
                        ActivityID = Guid.Parse("e886b1c0-0075-4637-9c4f-dacb366cd6fb"),
                        UniqueID = Guid.Parse("e886b1c0-0075-4637-9c4f-dacb366cd6fb"),
                        StepDescription = "Email(sql person)",
                    }
                },
        };

        private List<IServiceTestModelTO> GetTests()
        {
            var tests = new List<IServiceTestModelTO>
            {
                _false_branchTest,
                _true_branchtest
            };

            return tests;
        }
    }
}
