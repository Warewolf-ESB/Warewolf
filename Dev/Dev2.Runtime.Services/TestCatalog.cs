using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Wrappers;
using Dev2.Communication;

namespace Dev2.Runtime
{
    public class TestCatalog : ITestCatalog
    {
        private readonly DirectoryWrapper _directoryWrapper;
        private Dev2JsonSerializer _serializer;

        public TestCatalog()
        {
            _directoryWrapper = new DirectoryWrapper();
            _directoryWrapper.CreateIfNotExists(EnvironmentVariables.TestPath);
            Tests = new ConcurrentDictionary<Guid,List<IServiceTestModelTO>>();
            _serializer = new Dev2JsonSerializer();

        }

        public ConcurrentDictionary<Guid, List<IServiceTestModelTO>> Tests { get; }

        public void SaveTests(Guid resourceID, List<IServiceTestModelTO> serviceTestModelTos)
        {
            var testPath = EnvironmentVariables.TestPath;
            if (serviceTestModelTos != null && serviceTestModelTos.Count>0)
            {
                var dirPath = Path.Combine(testPath, resourceID.ToString());
                _directoryWrapper.CreateIfNotExists(dirPath);

                foreach (var serviceTestModelTo in serviceTestModelTos)
                {
                    var filePath = Path.Combine(dirPath, $"{serviceTestModelTo.TestName}.test");                    
                    
                    var sw = new StreamWriter(filePath, false);
                    _serializer.Serialize(sw, serviceTestModelTo);
                }
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
    }
}