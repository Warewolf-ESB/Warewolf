using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;

namespace Dev2.Runtime
{
    public class TestCatalog : ITestCatalog
    {
        public void SaveTests(Guid resourceID, List<IServiceTestModel> serviceTestModelTos)
        {
            var testPath = EnvironmentVariables.TestPath;
            foreach (var serviceTestModelTo in serviceTestModelTos)
            {
                var filePath = Path.Combine(testPath, resourceID.ToString(), $"{serviceTestModelTo.TestName}.test");
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                var sw = new StreamWriter(filePath, false);
                serializer.Serialize(sw, serviceTestModelTo);
                sw.Flush();
            }

        }
    }
}