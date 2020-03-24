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

namespace Dev2.Runtime
{
    public interface ITestCoverageCatalog
    {
        ConcurrentDictionary<Guid, List<IServiceTestModelTO>> TestCoverageReports { get; }

        IServiceAllTestsCoverageModelTo GenerateSingleTestCoverage(Guid resourceID, IServiceTestModelTO test);
        IServiceAllTestsCoverageModelTo GenerateAllTestsCoverage(Guid resourceID, List<IServiceTestModelTO> serviceTestModelTos);
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
            _directoryWrapper.CreateIfNotExists(EnvironmentVariables.TestPath);
            _serializer = new Dev2JsonSerializer();
            _serviceAllTestsCoverageModelToFactory = CustomContainer.Get<IServiceAllTestsCoverageModelToFactory>() ?? new ServiceAllTestsCoverageModelToFactory();
            TestCoverageReports = new ConcurrentDictionary<Guid, List<IServiceTestModelTO>>();
        }

        public static ITestCoverageCatalog Instance => LazyCat.Value;

        private readonly Dev2JsonSerializer _serializer;
        private readonly IServiceAllTestsCoverageModelToFactory _serviceAllTestsCoverageModelToFactory;

        public ConcurrentDictionary<Guid, List<IServiceTestModelTO>> TestCoverageReports { get; private set; }

        public IServiceAllTestsCoverageModelTo GenerateSingleTestCoverage(Guid resourceID, IServiceTestModelTO test)
        {
            var existingTestCoverageReports = TestCoverageReports.GetOrAdd(resourceID, new List<IServiceTestModelTO>());
            var record = existingTestCoverageReports.FirstOrDefault(to => to.TestName.Equals(test.TestName, StringComparison.CurrentCultureIgnoreCase));
            if (record == null)
            {
                existingTestCoverageReports.Add(test);
            }
            else
            {
                existingTestCoverageReports.Remove(record);
                existingTestCoverageReports.Add(test);
            }
            return SaveSingleTestCoverageToDisk(resourceID, test);
        }

        IServiceAllTestsCoverageModelTo SaveSingleTestCoverageToDisk(Guid resourceId, IServiceTestModelTO serviceTestModelTo)
        {
            var dirPath = GetTestCoveragePathForResourceId(resourceId);
            _directoryWrapper.CreateIfNotExists(dirPath);

            var serviceTestModelTos = new List<IServiceTestModelTO>
            {
                serviceTestModelTo
            };
            var serviceTestCoverageModelTo = _serviceAllTestsCoverageModelToFactory.New(resourceId, serviceTestModelTos);
            if (!string.Equals(serviceTestModelTo.OldTestName, serviceTestModelTo.TestName, StringComparison.InvariantCultureIgnoreCase))
            {
                var oldFilePath = Path.Combine(dirPath, $"{serviceTestModelTo.OldTestName}.coverage");
                _fileWrapper.Delete(oldFilePath);
            }
            var filePath = Path.Combine(dirPath, $"{serviceTestModelTo.TestName}.coverage");
            var sw = new StreamWriter(filePath, false);

            _serializer.Serialize(sw, serviceTestCoverageModelTo);
            return serviceTestCoverageModelTo;
            
        }

        static string GetTestCoveragePathForResourceId(Guid resourceId) => Path.Combine(EnvironmentVariables.TestCoveragePath, resourceId.ToString());

        public IServiceAllTestsCoverageModelTo GenerateAllTestsCoverage(Guid resourceID, List<IServiceTestModelTO> serviceTestModelTos)
        {
            var dirPath = GetTestCoveragePathForResourceId(resourceID);
            TestCoverageReports.AddOrUpdate(resourceID, GetTestCoverageReports(dirPath), (id, list) => GetTestCoverageReports(dirPath));
            return SaveAllTestCoverageToDisk(resourceID, serviceTestModelTos);
        }

        IServiceAllTestsCoverageModelTo SaveAllTestCoverageToDisk(Guid resourceId, List<IServiceTestModelTO> serviceTestModelTos)
        {
            var dirPath = GetTestCoveragePathForResourceId(resourceId);
            _directoryWrapper.CreateIfNotExists(dirPath);

            var serviceTestCoverageModelTo = _serviceAllTestsCoverageModelToFactory.New(resourceId, serviceTestModelTos); //new ServiceAllTestsCoverageModelTo(serviceTestModelTos);
            if (!string.Equals(serviceTestCoverageModelTo.OldReportName, serviceTestCoverageModelTo.ReportName, StringComparison.InvariantCultureIgnoreCase))
            {
                var oldFilePath = Path.Combine(dirPath, $"{serviceTestCoverageModelTo.OldReportName}.coverage");
                _fileWrapper.Delete(oldFilePath);
            }
            var filePath = Path.Combine(dirPath, $"{serviceTestCoverageModelTo.ReportName}.coverage");
            var sw = new StreamWriter(filePath, false);

            _serializer.Serialize(sw, serviceTestCoverageModelTo);

            return serviceTestCoverageModelTo;
        }

        List<IServiceTestModelTO> GetTestCoverageReports(string resourceTestDirectory)
        {
            var serviceTestModelTos = new List<IServiceTestModelTO>();
            var files = _directoryWrapper.GetFiles(resourceTestDirectory);
            foreach (var file in files)
            {
                try
                {
                    var reader = new StreamReader(file);
                    var testModel = _serializer.Deserialize<IServiceTestModelTO>(reader);
                    serviceTestModelTos.Add(testModel);
                }
                catch (Exception e)
                {
                    Dev2Logger.Warn($"failed loading test: {file} {e.GetType().Name}: " + e.Message, GlobalConstants.WarewolfWarn);
                }
            }
            return serviceTestModelTos;
        }
    }

    public interface IServiceAllTestsCoverageModelToFactory
    {
        IServiceAllTestsCoverageModelTo New(Guid workflowId, List<IServiceTestModelTO> serviceTestModelTos);
    }

    internal class ServiceAllTestsCoverageModelToFactory : IServiceAllTestsCoverageModelToFactory
    {
        public IServiceAllTestsCoverageModelTo New(Guid workflowId, List<IServiceTestModelTO> serviceTestModelTos)
        {
            return new ServiceAllTestsCoverageModelTo(workflowId, serviceTestModelTos);
        }
    }

    public interface IWorkflowNode 
    {
        Guid ActivityID { get; set; }
        Guid UniqueID { get; set; }
        string StepDescription { get; set; }
    }

    public class WorkflowNode : IWorkflowNode
    {
        public WorkflowNode()
        {
        }

        public Guid ActivityID { get; set; }
        public Guid UniqueID { get; set; }
        public string StepDescription { get; set; }
    }

    public interface IWorkflowNodesCovered
    {
    }

    public interface IWorkflowBuilder
    {
        string Name { get; }
        Guid WorkflowId { get; }
        List<IWorkflowNode> GetAllWorkflowNodes();
    }

    //Just think this class should implement the IResourceModel or be similarly structured as IResourceModel
    //this is so that this class can now hold all that is used and needed be a workflow
    public class WorkflowBuilder : IWorkflowBuilder
    {
        private readonly IResourceCatalog _resourceCatalog;
        private readonly IWorkflowFactory _workflowFactory;
        private Guid _workflowId;

        public WorkflowBuilder(Guid worflowId)
        {
            _resourceCatalog = CustomContainer.Get<IResourceCatalog>() ?? ResourceCatalog.Instance;
            _workflowFactory = CustomContainer.Get<IWorkflowFactory>() ?? new WorkflowFactory();

            _workflowId = worflowId;
        }

        public string Name { get; private set; }

        public Guid WorkflowId 
        {
            get { return _workflowId; }
            set { _workflowId = value; } 
        }

        public List<IWorkflowNode> GetAllWorkflowNodes()
        {
            var workflow = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, WorkflowId);
            return _workflowFactory.New(workflow.ToXElement()).WorkflowNodes; 
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

    internal interface IServiceSingleTestCoverageModelTo
    {
        Guid WorkflowId { get; }
        string OldTestName { get; }
        string TestName { get; }
        string Password { get; }
        List<ISingleTestNodesCovered> SingleTestNodesCovered { get; }
    }

    internal class ServiceSingleTestCoverageModelTo : IServiceSingleTestCoverageModelTo
    {
        private readonly IServiceTestModelTO _test;
        private List<ISingleTestNodesCovered> _singleTestNodesCovered;

        internal ServiceSingleTestCoverageModelTo(IServiceTestModelTO test)
        {
            _test = test;

            _singleTestNodesCovered = GetSingleTestNodesCovered();
        }

        public List<ISingleTestNodesCovered> SingleTestNodesCovered
        {
            get { return _singleTestNodesCovered; }
            set { _singleTestNodesCovered = value; }
        }

        public string TestName => _test.TestName;

        public string OldTestName => _test.OldTestName;

        public string Password => _test.Password;

        public Guid WorkflowId => _test.ResourceId;

        private List<ISingleTestNodesCovered> GetSingleTestNodesCovered()
        {
            var SingleTestNodesCovered = new List<ISingleTestNodesCovered>
            {
                new SingleTestNodesCovered
                {
                    TestName = _test.TestName,
                    TestNodesCovered = GetNodesCovered()
                }
            };

            return SingleTestNodesCovered;
        }

        private List<IWorkflowNode> GetNodesCovered()
        {
            var nodesCovered = new List<IWorkflowNode>();

            var testSteps = _test.TestSteps;
            if (testSteps is null)
            {
                return new List<IWorkflowNode>();
            }
            {
                foreach (var step in testSteps)
                {
                    nodesCovered.Add(new WorkflowNode
                    {
                        ActivityID = step.ActivityID,
                        UniqueID = step.UniqueID,
                        StepDescription = step.StepDescription
                    });
                }
                return nodesCovered;
            }
        }
    }


    public interface IServiceAllTestsCoverageModelTo
    {
        List<List<ISingleTestNodesCovered>> AllTestNodesCovered { get; }
        string OldReportName { get; set; }
        string ReportName { get; }
        Guid WorkflowId { get; }
        double CoveragePercentage { get; }
    }

    public class ServiceAllTestsCoverageModelTo : IServiceAllTestsCoverageModelTo
    {
        private readonly List<IServiceTestModelTO> _tests;
        private readonly IWorkflowBuilder _workflow;
        private List<IWorkflowNode> _coveredNodes = new List<IWorkflowNode>();
        private double _coveragePercentage;
        private List<List<ISingleTestNodesCovered>> _allTestNodesCovered;

        public ServiceAllTestsCoverageModelTo(Guid workflowId, List<IServiceTestModelTO> tests)
        {
            _tests = tests;
            _workflow = CustomContainer.Get<IWorkflowBuilder>() ?? new WorkflowBuilder(workflowId);

            _allTestNodesCovered = GetAllTestNodesCovered();
            _coveragePercentage = GetCoveragePercentage();
        }

        public List<List<ISingleTestNodesCovered>> AllTestNodesCovered
        {
            get { return _allTestNodesCovered; }
            set { _allTestNodesCovered = value; }
        }

        private List<List<ISingleTestNodesCovered>> GetAllTestNodesCovered()
        {
            var tempAllTestNodes = new List<List<ISingleTestNodesCovered>>();
            _tests.ForEach(test => tempAllTestNodes.Add(GetSingleTestNodesCovered(test)));
            return tempAllTestNodes;
        }

        private List<ISingleTestNodesCovered> GetSingleTestNodesCovered(IServiceTestModelTO test)
        {
            var testNodesCovered = new ServiceSingleTestCoverageModelTo(test).SingleTestNodesCovered.ToList();
            testNodesCovered.ForEach(o => _coveredNodes.AddRange(o.TestNodesCovered));
            return testNodesCovered;
        }

        public string OldReportName { get; set; }

        public string ReportName => _workflow.Name;

        public Guid WorkflowId => _workflow.WorkflowId;

        public double CoveragePercentage
        {
            get 
            { 
                return _coveragePercentage; 
            }
            set
            {
                _coveragePercentage = value;
            }
        }

        private double GetCoveragePercentage()
        {
            var workflowNodes = _workflow.GetAllWorkflowNodes();

            var groupedByUniqueId = _coveredNodes.GroupBy(i => i.UniqueID).Select(o => o.FirstOrDefault());
            double coveredNodes = groupedByUniqueId.Select(u => u.UniqueID).Intersect(workflowNodes.Select(o => o.UniqueID)).Count();

            double totalNodes = workflowNodes.Count;

            return coveredNodes / totalNodes * 100;
        }
    }

    public interface ISingleTestNodesCovered
    {
        string TestName { get; }
        List<IWorkflowNode> TestNodesCovered { get; }
    }

    internal class SingleTestNodesCovered : ISingleTestNodesCovered
    {
        public SingleTestNodesCovered()
        {
        }

        public string TestName { get; internal set; }
        public List<IWorkflowNode> TestNodesCovered { get; internal set; }
    }
}
