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
using Dev2.Data;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Interfaces.Communication;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Runtime
{

    public class TestCoverageCatalog : ITestCoverageCatalog
    {
        static readonly Lazy<ITestCoverageCatalog> LazyCat = new Lazy<ITestCoverageCatalog>(() =>
        {
            var catalog = new TestCoverageCatalog(ResourceCatalog.Instance);
            return catalog;
        }, LazyThreadSafetyMode.PublicationOnly);


        private readonly IFile _fileWrapper;
        private readonly IFilePath _filePathWapper;
        private readonly IDirectory _directoryWrapper;
        public static ITestCoverageCatalog Instance => LazyCat.Value;

        private readonly ISerializer _serializer;
        private readonly IServiceTestCoverageModelToFactory _serviceAllTestsCoverageModelToFactory;
        private readonly IStreamWriterFactory _streamWriterFactory;
        private readonly IStreamReaderFactory _streamReaderFactory;

        public TestCoverageCatalog(IResourceCatalog resourceCatalog)
        {
            _directoryWrapper = new DirectoryWrapper();
            _fileWrapper = new FileWrapper();
            _filePathWapper = new FilePathWrapper();
            _directoryWrapper.CreateIfNotExists(EnvironmentVariables.TestCoveragePath);
            _serializer = new Dev2JsonSerializer();
            _streamWriterFactory = new StreamWriterFactory();
            _streamReaderFactory = new StreamReaderFactory();
            _serviceAllTestsCoverageModelToFactory = CustomContainer.Get<IServiceTestCoverageModelToFactory>() ?? new ServiceTestCoverageModelToFactory(resourceCatalog);
        }

        [ExcludeFromCodeCoverage] //This CTOR is used by tests
        public TestCoverageCatalog(IServiceTestCoverageModelToFactory serviceTestCoverageModelToFactory, IFilePath filePath, IFile fileWrapper, IDirectory directory, IStreamWriterFactory streamWriterFactory, IStreamReaderFactory streamReaderFactory, ISerializer serializer)
        {
            _serviceAllTestsCoverageModelToFactory = serviceTestCoverageModelToFactory;
            _filePathWapper = filePath;
            _fileWrapper = fileWrapper;
            _directoryWrapper = directory;
            _streamWriterFactory = streamWriterFactory;
            _streamReaderFactory = streamReaderFactory;
            _serializer = serializer;
        }

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

        private string GetTestCoveragePathForResourceId(Guid resourceId) => _filePathWapper.Combine(EnvironmentVariables.TestCoveragePath, resourceId.ToString());

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
                var oldFilePath = _filePathWapper.Combine(dirPath, $"{serviceTestCoverageModelTo.OldReportName}.coverage");
                _fileWrapper.Delete(oldFilePath);
            }
            var filePath = _filePathWapper.Combine(dirPath, $"{serviceTestCoverageModelTo.ReportName}.coverage");
            var sw = _streamWriterFactory.New(filePath, false);

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
                    var reader = _streamReaderFactory.New(file);
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
}
