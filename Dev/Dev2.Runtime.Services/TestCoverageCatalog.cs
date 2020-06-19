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
using Dev2.Common.Interfaces;
using Dev2.Common.Serializers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dev2.Runtime.Hosting;
using Warewolf.Data;

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
    }

    public class TestCoverageCatalog : ITestCoverageCatalog
    {
        static readonly Lazy<ITestCoverageCatalog> LazyCat = new Lazy<ITestCoverageCatalog>(() =>
        {
            var catalog = new TestCoverageCatalog(ResourceCatalog.Instance);
            return catalog;
        }, LazyThreadSafetyMode.PublicationOnly);


        private readonly DirectoryWrapper _directoryWrapper;
        private readonly FileWrapper _fileWrapper;

        public TestCoverageCatalog(IResourceCatalog resourceCatalog)
        {
            _directoryWrapper = new DirectoryWrapper();
            _fileWrapper = new FileWrapper();
            _directoryWrapper.CreateIfNotExists(EnvironmentVariables.TestCoveragePath);
            _serializer = new Dev2JsonSerializer();
            _serviceAllTestsCoverageModelToFactory = CustomContainer.Get<IServiceTestCoverageModelToFactory>() ?? new ServiceTestCoverageModelToFactory(resourceCatalog);
        }

        public static ITestCoverageCatalog Instance => LazyCat.Value;

        private readonly Dev2JsonSerializer _serializer;
        private readonly IServiceTestCoverageModelToFactory _serviceAllTestsCoverageModelToFactory;

        public ConcurrentDictionary<Guid, List<IServiceTestCoverageModelTo>> TestCoverageReports { get; } = new ConcurrentDictionary<Guid, List<IServiceTestCoverageModelTo>>();

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
            if (!TestCoverageReports.TryGetValue(workflowId, out List<IServiceTestCoverageModelTo> testList))
            {
                return null;
            }

            var result = testList?.FirstOrDefault(to => to.ReportName.Equals(reportName, StringComparison.InvariantCultureIgnoreCase));
            return result;
        }

        public void ReloadAllReports()
        {
            TestCoverageReports.Clear();
            Load();
        }

        private void Load()
        {
            TestCoverageReports.Clear();
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
                var exists = _fileWrapper.Exists(file.Replace(EnvironmentVariables.TestCoveragePath, EnvironmentVariables.TestPath).Replace(".coverage", ".test"));
                if (!exists)
                {
                    _fileWrapper.Delete(file);
                    continue;
                }
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
        private readonly IResourceCatalog _resourceCatalog;

        public ServiceTestCoverageModelToFactory(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }
        public IServiceTestCoverageModelTo New(Guid workflowId, CoverageArgs args, List<IServiceTestModelTO> serviceTestModelTos)
        {
            var allTestNodesCovered = serviceTestModelTos.Select(test => new SingleTestNodesCovered(test.TestName, test.TestSteps)).ToArray();

            var result = new ServiceTestCoverageModelTo
            {
                WorkflowId = workflowId,
                OldReportName = args?.OldReportName,
                ReportName = args?.ReportName,
                LastRunDate = DateTime.Now,
                AllTestNodesCovered = allTestNodesCovered,
            };
            var coveredNodes = allTestNodesCovered.SelectMany(o => o.TestNodesCovered);

            var workflow = _resourceCatalog.GetWorkflow(workflowId);
            var workflowNodes = workflow.WorkflowNodes;
            var testedNodes = coveredNodes.GroupBy(i => i.ActivityID).Select(o => o.First())
                .Where(o => o.MockSelected is false)
                .Select(u => u.ActivityID);
            var n = testedNodes.Intersect(workflowNodes.Select(o => o.UniqueID)).Count();
            double totalNodes = workflowNodes.Count;
            result.CoveragePercentage = n / totalNodes;
            result.TotalCoverage = n / totalNodes;

            return result;
        }
    }


    public class WorkflowNode : IWorkflowNode
    {
        public Guid ActivityID { get; set; }
        public Guid UniqueID { get; set; }
        public string StepDescription { get; set; }
        public bool MockSelected { get; set; }
        public List<IWorkflowNode> NextNodes { get; set; } = new List<IWorkflowNode>();

        public void Add(IWorkflowNode node)
        {
            NextNodes.Add(node);
        }
    }

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
