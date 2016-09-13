using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Warewolf.Security.Encryption;

// ReSharper disable LoopCanBeConvertedToQuery

namespace Dev2.Runtime
{
    public class TestCatalog : ITestCatalog
    {
        private readonly DirectoryWrapper _directoryWrapper;
        private readonly Dev2JsonSerializer _serializer;
        private readonly FileWrapper _fileWrapper;

        private static readonly Lazy<TestCatalog> LazyCat = new Lazy<TestCatalog>(() =>
        {
            var c = new TestCatalog();
            return c;
        }, LazyThreadSafetyMode.PublicationOnly);

        public static ITestCatalog Instance => LazyCat.Value;

        public TestCatalog()
        {
            _directoryWrapper = new DirectoryWrapper();
            _fileWrapper = new FileWrapper();
            _directoryWrapper.CreateIfNotExists(EnvironmentVariables.TestPath);
            Tests = new ConcurrentDictionary<Guid,List<IServiceTestModelTO>>();
            _serializer = new Dev2JsonSerializer();

        }

        public ConcurrentDictionary<Guid, List<IServiceTestModelTO>> Tests { get; }

        public void SaveTests(Guid resourceID, List<IServiceTestModelTO> serviceTestModelTos)
        {
            if (serviceTestModelTos != null && serviceTestModelTos.Count>0)
            {
                var dirPath = GetTestPathForResourceId(resourceID);
                _directoryWrapper.CreateIfNotExists(dirPath);

                foreach (var serviceTestModelTo in serviceTestModelTos)
                {
                    if (!string.Equals(serviceTestModelTo.OldTestName, serviceTestModelTo.TestName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var oldFilePath = Path.Combine(dirPath, $"{serviceTestModelTo.OldTestName}.test");
                        _fileWrapper.Delete(oldFilePath);
                    }
                    var filePath = Path.Combine(dirPath, $"{serviceTestModelTo.TestName}.test");
                    serviceTestModelTo.Password = DpapiWrapper.EncryptIfDecrypted(serviceTestModelTo.Password);
                    var sw = new StreamWriter(filePath, false);
                    _serializer.Serialize(sw, serviceTestModelTo);
                }
                var dir = Path.Combine(EnvironmentVariables.TestPath, resourceID.ToString());
                Tests.AddOrUpdate(resourceID, GetTestList(dir), (id, list) => GetTestList(dir));
            }
        }

        public void Load()
        {
            var resourceTestDirectories = _directoryWrapper.GetDirectories(EnvironmentVariables.TestPath);
            foreach(var resourceTestDirectory in resourceTestDirectories)
            {
                var resIdString = _directoryWrapper.GetDirectoryName(resourceTestDirectory);
                Guid resId;
                if(Guid.TryParse(resIdString,out resId))
                {
                    Tests.AddOrUpdate(resId, GetTestList(resourceTestDirectory),(id,list)=> GetTestList(resourceTestDirectory));
                }
                
            }
        }

        private List<IServiceTestModelTO> GetTestList(string resourceTestDirectory)
        {
            var serviceTestModelTos = new List<IServiceTestModelTO>();
            var files = _directoryWrapper.GetFiles(resourceTestDirectory);
            foreach(var file in files)
            {
                var reader = new StreamReader(file);
                var testModel = _serializer.Deserialize<IServiceTestModelTO>(reader);
                serviceTestModelTos.Add(testModel);
            }
            return serviceTestModelTos;
        }

        public List<IServiceTestModelTO> Fetch(Guid resourceId)
        {
            return Tests.GetOrAdd(resourceId, guid =>
             {
                 var dir = Path.Combine(EnvironmentVariables.TestPath, guid.ToString());
                 return GetTestList(dir);
             });
        }

        public void DeleteTest(Guid resourceID, string testName)
        {
            var dirPath = GetTestPathForResourceId(resourceID);
            var testFilePath = Path.Combine(dirPath, $"{testName}.test");
            if (_fileWrapper.Exists(testFilePath))
            {
                _fileWrapper.Delete(testFilePath);
                List<IServiceTestModelTO> testList;
                if(Tests.TryGetValue(resourceID, out testList))
                {
                    var foundTestToDelete = testList.FirstOrDefault(to => to.TestName.Equals(testName, StringComparison.InvariantCultureIgnoreCase));
                    if (foundTestToDelete!=null)
                    {
                        testList.Remove(foundTestToDelete);
                    }
                }
            }
        }

        public void DeleteAllTests(Guid resourceId)
        {
            var dirPath = GetTestPathForResourceId(resourceId);
            if (_directoryWrapper.Exists(dirPath))
            {
                _directoryWrapper.Delete(dirPath,true);
                List<IServiceTestModelTO> removedTests;
                Tests.TryRemove(resourceId, out removedTests);
            }
        }

        private static string GetTestPathForResourceId(Guid resourceId)
        {
            var testPath = EnvironmentVariables.TestPath;
            var dirPath = Path.Combine(testPath, resourceId.ToString());
            return dirPath;
        }
    }
}