/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Serializers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json;
using Warewolf;

namespace Dev2.Runtime
{
    public interface ITestCoverageCatalog
    {
        IServiceTestCoverageModelTo GenerateSingleTestCoverage(Guid resourceID, IServiceTestModelTO test);
        IServiceTestCoverageModelTo GenerateAllTestsCoverage(string resourceName, Guid resourceId, List<IServiceTestModelTO> serviceTestModelTos);
        IServiceTestCoverageModelTo FetchReport(Guid workflowId, string reportName);
        void ReloadAllReports();
        void DeleteAllCoverageReports(Guid workflowId);
        List<IServiceTestCoverageModelTo> Fetch(Guid coverageResourceId);
        // void SaveCoverage(Guid resourceId, IServiceTestCoverageModelTo serviceTestCoverageModelTo);
    }

    public class TestCoverageCatalog : ITestCoverageCatalog
    {
        static readonly Lazy<ITestCoverageCatalog> LazyCat = new Lazy<ITestCoverageCatalog>(() =>
        {
            var catalog = new TestCoverageCatalog();
            return catalog;
        }, LazyThreadSafetyMode.PublicationOnly);


        private readonly DirectoryWrapper _directoryWrapper;
        private readonly FileWrapper _fileWrapper;

        public TestCoverageCatalog()
        {
            _directoryWrapper = new DirectoryWrapper();
            _fileWrapper = new FileWrapper();
            _directoryWrapper.CreateIfNotExists(EnvironmentVariables.TestCoveragePath);
            _serializer = new Dev2JsonSerializer();
            _serviceAllTestsCoverageModelToFactory = CustomContainer.Get<IServiceTestCoverageModelToFactory>() ?? new ServiceTestCoverageModelToFactory();
            TestCoverageReports = new ConcurrentDictionary<Guid, List<IServiceTestCoverageModelTo>>();
        }

        public static ITestCoverageCatalog Instance => LazyCat.Value;

        private readonly Dev2JsonSerializer _serializer;
        private readonly IServiceTestCoverageModelToFactory _serviceAllTestsCoverageModelToFactory;

        public ConcurrentDictionary<Guid, List<IServiceTestCoverageModelTo>> TestCoverageReports { get; private set; }

        public IServiceTestCoverageModelTo GenerateSingleTestCoverage(Guid resourceID, IServiceTestModelTO test)
        {
            return SaveSingleTestCoverageToDisk(resourceID, test);
        }

        private void UpdateTestCoverageReports(Guid resourceID, IServiceTestCoverageModelTo test)
        {
            var existingTestCoverageReports = TestCoverageReports.GetOrAdd(resourceID, new List<IServiceTestCoverageModelTo>());
            var record = existingTestCoverageReports.FirstOrDefault(to => to.ReportName.Equals(test.ReportName, StringComparison.CurrentCultureIgnoreCase));
            if (record == null)
            {
                existingTestCoverageReports.Add(test);
            }
            else
            {
                existingTestCoverageReports.Remove(record);
                existingTestCoverageReports.Add(record);
            }
        }

        IServiceTestCoverageModelTo SaveSingleTestCoverageToDisk(Guid resourceId, IServiceTestModelTO serviceTestModelTo)
        {
            var serviceTestModelTos = new List<IServiceTestModelTO>
            {
                serviceTestModelTo
            };
            var coverageArgs = new CoverageArgs { OldReportName = serviceTestModelTo.OldTestName, ReportName = serviceTestModelTo.TestName };
            var serviceTestCoverageModelTo = _serviceAllTestsCoverageModelToFactory.New(resourceId, coverageArgs, serviceTestModelTos);

            SaveCoverageReport(resourceId, serviceTestCoverageModelTo);
            
            UpdateTestCoverageReports(resourceId, serviceTestCoverageModelTo);
            return serviceTestCoverageModelTo;
        }

        public void DeleteCoverageReport(Guid resourceID, string reportName)
        {
            var dirPath = GetTestCoveragePathForResourceId(resourceID);
            var testFilePath = Path.Combine(dirPath, $"{reportName}.coverage");
            if (_fileWrapper.Exists(testFilePath))
            {
                _fileWrapper.Delete(testFilePath);
                if (TestCoverageReports.TryGetValue(resourceID, out List<IServiceTestCoverageModelTo> coverageReports))
                {
                    var foundReportToDelete = coverageReports.FirstOrDefault(to => to.ReportName.Equals(reportName, StringComparison.InvariantCultureIgnoreCase));
                    if (foundReportToDelete != null)
                    {
                        Dev2Logger.Debug("Removing Report: " + reportName + Environment.NewLine + Environment.StackTrace, GlobalConstants.WarewolfDebug);
                        coverageReports.Remove(foundReportToDelete);
                    }
                }
            }
        }

        public void DeleteAllCoverageReports(Guid resourceId)
        {
            var dirPath = GetTestCoveragePathForResourceId(resourceId);
            if (_directoryWrapper.Exists(dirPath))
            {
                _directoryWrapper.Delete(dirPath, true);
                TestCoverageReports.TryRemove(resourceId, out _);
            }
        }

        static string GetTestCoveragePathForResourceId(Guid resourceId) => Path.Combine(EnvironmentVariables.TestCoveragePath, resourceId.ToString());

        public IServiceTestCoverageModelTo GenerateAllTestsCoverage(string resourceName, Guid resourceId, List<IServiceTestModelTO> serviceTestModelTos)
        {
            var coverageArgs = new CoverageArgs { OldReportName = "", ReportName = resourceName };
            var serviceTestCoverageModelTo = _serviceAllTestsCoverageModelToFactory.New(resourceId, coverageArgs, serviceTestModelTos);


            UpdateTestCoverageReports(resourceId, serviceTestCoverageModelTo);

            return serviceTestCoverageModelTo;
        }


        private void SaveCoverageReport(Guid resourceId, IServiceTestCoverageModelTo serviceTestCoverageModelTo)
        {
            var dirPath = GetTestCoveragePathForResourceId(resourceId);
            _directoryWrapper.CreateIfNotExists(dirPath);

            if (!string.Equals(serviceTestCoverageModelTo.OldReportName, serviceTestCoverageModelTo.ReportName, StringComparison.InvariantCultureIgnoreCase))
            {
                var oldFilePath = Path.Combine(dirPath, $"{serviceTestCoverageModelTo.OldReportName}.coverage");
                _fileWrapper.Delete(oldFilePath);
            }
            var filePath = Path.Combine(dirPath, $"{serviceTestCoverageModelTo.ReportName}.coverage");
            var sw = new StreamWriter(filePath, false);

            _serializer.Serialize(sw, serviceTestCoverageModelTo);
        }

        public IServiceTestCoverageModelTo FetchReport(Guid workflowId, string reportName)
        {
            if (TestCoverageReports.TryGetValue(workflowId, out List<IServiceTestCoverageModelTo> testList))
            {
                var result = testList?.FirstOrDefault(to => to.ReportName.Equals(reportName, StringComparison.InvariantCultureIgnoreCase));
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public void ReloadAllReports()
        {
            TestCoverageReports.Clear();
            Load();
        }

        private void Load()
        {
            TestCoverageReports = new ConcurrentDictionary<Guid, List<IServiceTestCoverageModelTo>>();
            var resourceTestCoverageDirectories = _directoryWrapper.GetDirectories(EnvironmentVariables.TestCoveragePath);
            foreach (var resourceTestCoverageDirectory in resourceTestCoverageDirectories)
            {
                var resIdString = _directoryWrapper.GetDirectoryName(resourceTestCoverageDirectory);
                if (Guid.TryParse(resIdString, out Guid resId))
                {
                    TestCoverageReports.AddOrUpdate(resId, GetReportList(resourceTestCoverageDirectory), (id, list) => GetReportList(resourceTestCoverageDirectory));
                }

            }
        }

        private List<IServiceTestCoverageModelTo> GetReportList(string resourceTestDirectory)
        {
            var serviceTestCoverageModelTos = new List<IServiceTestCoverageModelTo>();
            var files = _directoryWrapper.GetFiles(resourceTestDirectory);
            foreach (var file in files)
            {
                try
                {
                    var reader = new StreamReader(file);
                    var testModel = _serializer.Deserialize<ServiceTestCoverageModelTo>(reader);
                    serviceTestCoverageModelTos.Add(testModel);
                }
                catch (Exception e)
                {
                    Dev2Logger.Warn($"failed loading test: {file} {e.GetType().Name}: " + e.Message, GlobalConstants.WarewolfWarn);
                }
            }
            return serviceTestCoverageModelTos;
        }

        public List<IServiceTestCoverageModelTo> Fetch(Guid coverageResourceId)
        {
            var result = TestCoverageReports.GetOrAdd(coverageResourceId, guid =>
            {
                var dir = Path.Combine(EnvironmentVariables.TestCoveragePath, guid.ToString());
                return GetReportList(dir);
            });
            // note: list is duplicated in order to avoid concurrent modifications of the list during test runs
            return result.ToList();
        }

        // public void SaveCoverage(Guid resourceId, IServiceTestCoverageModelTo serviceTestCoverageModelTo)
        // {
        //     SaveCoverageReport(resourceId, serviceTestCoverageModelTo);
        // }
    }

    public interface IServiceTestCoverageModelToFactory
    {
        IServiceTestCoverageModelTo New(Guid workflowId, CoverageArgs args, List<IServiceTestModelTO> serviceTestModelTos);
    }

    public class CoverageArgs
    {
        public string OldReportName { get; set; } 
        public string ReportName { get; set; }
    }

    internal class ServiceTestCoverageModelToFactory : IServiceTestCoverageModelToFactory
    {
        public IServiceTestCoverageModelTo New(Guid workflowId, CoverageArgs args, List<IServiceTestModelTO> serviceTestModelTos)
        {
            return new ServiceTestCoverageModelTo(workflowId, args, serviceTestModelTos);
        }
    }

    public interface IWorkflowNode 
    {
        Guid ActivityID { get; }
        Guid UniqueID { get; }
        string StepDescription { get; }
        bool MockSelected { get; }
        List<IWorkflowNode> NextNodes { get; }

    }

    public class WorkflowNode : IWorkflowNode
    {
        public Guid ActivityID { get; set; }
        public Guid UniqueID { get; set; }
        public string StepDescription { get; set; }
        public bool MockSelected { get; set; }
        public List<IWorkflowNode> NextNodes { get; set; }
    }

    public interface IWorkflowWrapper
    {
        string Name { get; }
        Guid WorkflowId { get; }
        List<IWorkflowNode> GetAllWorkflowNodes();
    }

    //Just think this class should implement the IResourceModel or be similarly structured as IResourceModel
    //this is so that this class can now hold all that is used and needed be a workflow
    public class WorkflowWrapper : IWorkflowWrapper
    {
        private readonly IWorkflow _workflow;

        public WorkflowWrapper(Guid workflowId)
        {
            var resourceCatalog = CustomContainer.Get<IResourceCatalog>() ?? ResourceCatalog.Instance;
            var workflowFactory = CustomContainer.Get<IWorkflowFactory>() ?? new WorkflowFactory();

            var workflowContents = resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, workflowId);
            _workflow = workflowFactory.New(workflowContents.ToXElement());

            WorkflowId = workflowId;
            Name = _workflow.Name;
        }

        public string Name { get; }

        public Guid WorkflowId { get; }

        public List<IWorkflowNode> GetAllWorkflowNodes()
        {
            return _workflow?.WorkflowNodes;
        }

        public List<IWorkflowNode> GetHTMLWorkflowNodes()
        {
            return _workflow?.WorkflowNodesForHtml;
        }
    }

    public class WorkflowFactory : IWorkflowFactory
    {
        public IWorkflow New(XElement xElement)
        {
            return new Workflow(xElement);
        }
    }

    public interface IWorkflowFactory
    {
        IWorkflow New(XElement xElement);
    }

    // internal interface IServiceSingleTestCoverageModelTo
    // {
    //     // Guid WorkflowId { get; }
    //     // string OldTestName { get; }
    //     // string TestName { get; }
    //     // string Password { get; }
    //     // ISingleTestNodesCovered SingleTestNodesCovered { get; }
    // }

    public interface IServiceTestCoverageModelTo
    {
        SingleTestNodesCovered[] AllTestNodesCovered { get; }
        string OldReportName { get; }
        string ReportName { get; }
        Guid WorkflowId { get; }
        double CoveragePercentage { get; }
        DateTime LastRunDate { get; }
        double TotalCoverage { get; }
    }

    public class ServiceTestCoverageModelTo : IServiceTestCoverageModelTo
    {
        public ServiceTestCoverageModelTo()
        {
            // Used during json deserialization
        }

        public ServiceTestCoverageModelTo(Guid workflowId, CoverageArgs args, List<IServiceTestModelTO> tests)
        {
            OldReportName = args?.OldReportName;
            ReportName = args?.ReportName;
            LastRunDate = DateTime.Now;


            AllTestNodesCovered = tests.Select(test => new SingleTestNodesCovered(test.TestName, test.TestSteps)).ToArray();
            var coveredNodes = AllTestNodesCovered.SelectMany(o => o.TestNodesCovered);

            WorkflowId = workflowId;

            var workflow = new WorkflowWrapper(workflowId);
            var workflowNodes = workflow.GetAllWorkflowNodes();
            var testedNodes = coveredNodes.GroupBy(i => i.ActivityID).Select(o => o.First())
                .Where(o => o.MockSelected is false)
                .Select(u => u.ActivityID);
            var n = testedNodes.Intersect(workflowNodes.Select(o => o.UniqueID)).Count();
            double totalNodes = workflowNodes.Count;
            CoveragePercentage = n / totalNodes;
            TotalCoverage = n / totalNodes;
        }

        public SingleTestNodesCovered[] AllTestNodesCovered { get; set; }

        public string OldReportName { get; set; }

        public string ReportName { get; set; }

        public Guid WorkflowId { get; set; }

        public DateTime LastRunDate { get; set; }

        public double CoveragePercentage { get; set; }
        public double TotalCoverage { get; set; }
}

    public interface ISingleTestNodesCovered
    {
        string TestName { get; }
        List<IWorkflowNode> TestNodesCovered { get; }
    }

    public class SingleTestNodesCovered : ISingleTestNodesCovered
    {
        public SingleTestNodesCovered(string testName, IEnumerable<IServiceTestStep> testSteps)
        {
            TestName = testName;
            TestNodesCovered = testSteps?.Select(step => new WorkflowNode
            {
                ActivityID = step.ActivityID,
                UniqueID = step.UniqueID,
                StepDescription = step.StepDescription,
                MockSelected = step.MockSelected
            }).ToList<IWorkflowNode>() ?? new List<IWorkflowNode>();
        }

        public string TestName { get; }
        public List<IWorkflowNode> TestNodesCovered { get; }
    }
}
