using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace Dev2.Runtime.Tests
{
    [TestClass]
    static class InitializeAuditDB
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            string inputFilePath = "Dev2.Tests.Activities.auditDB.db";
            string outputFilePath = Environment.ExpandEnvironmentVariables(@"%programdata%\Warewolf\Audits\auditDB.db");
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            if (!Directory.Exists(Path.GetDirectoryName(outputFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
            }
            Stream inputFile = Assembly.GetExecutingAssembly().GetManifestResourceStream(inputFilePath);
            Assert.IsNotNull(inputFile, inputFilePath + " file not found in " + string.Join(", ", Assembly.GetExecutingAssembly().GetManifestResourceNames()));
            using (Stream input = inputFile)
            {
                using (Stream output = File.Create(outputFilePath))
                {
                    byte[] buffer = new byte[8192];

                    int bytesRead;
                    while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}
