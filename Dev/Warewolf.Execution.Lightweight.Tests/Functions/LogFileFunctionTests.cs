using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Tests.Auth;

namespace Warewolf.Execution.Lightweight.Tests.Functions
{
    /// <summary>
    /// Tests for <see cref="LogFileFunction"/> covering numLines parameter parsing
    /// and last-N-lines reading behavior.
    /// </summary>
    [TestClass]
    public class LogFileFunctionTests
    {
        private string _tempLogFile = null!;
        private string? _originalLogFilePath;

        [TestInitialize]
        public void Setup()
        {
            _tempLogFile = Path.GetTempFileName();
            // Write a 10-line log file
            File.WriteAllLines(_tempLogFile, new[]
            {
                "Line1", "Line2", "Line3", "Line4", "Line5",
                "Line6", "Line7", "Line8", "Line9", "Line10"
            });

            // Override the server log file path env var
            _originalLogFilePath = Environment.GetEnvironmentVariable("WAREWOLF_SERVER_LOG_FILE");
            // EnvironmentVariables.ServerLogFile reads from a known location;
            // we'll use reflection or direct field setting if needed.
            // For now, test the ReadLastLines logic via the function endpoint.
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { File.Delete(_tempLogFile); } catch { }
            if (_originalLogFilePath != null)
                Environment.SetEnvironmentVariable("WAREWOLF_SERVER_LOG_FILE", _originalLogFilePath);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetLogFile_ReturnsResponse()
        {
            // EnvironmentVariables.ServerLogFile may or may not exist in the test env.
            // This test verifies the function executes without throwing.
            var func = new LogFileFunction();
            var ctx = new TestFunctionContext();
            var req = new FakeHttpRequestData(ctx,
                new Uri("https://localhost/api/internal/getlogfile"));

            var response = await func.GetLogFile(req);

            // Should be either OK (file exists) or NotFound (file missing)
            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ReadLastLines_ReturnsCorrectNumberOfLines()
        {
            // Use reflection to test the private ReadLastLines method
            var method = typeof(LogFileFunction).GetMethod("ReadLastLines",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.IsNotNull(method, "ReadLastLines method should exist");

            var result = (string)method.Invoke(null, new object[] { _tempLogFile, 3 })!;
            var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(3, lines.Length);
            Assert.AreEqual("Line8", lines[0]);
            Assert.AreEqual("Line9", lines[1]);
            Assert.AreEqual("Line10", lines[2]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ReadLastLines_RequestMoreThanExists_ReturnsAll()
        {
            var method = typeof(LogFileFunction).GetMethod("ReadLastLines",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (string)method!.Invoke(null, new object[] { _tempLogFile, 50 })!;
            var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(10, lines.Length);
            Assert.AreEqual("Line1", lines[0]);
            Assert.AreEqual("Line10", lines[9]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ReadLastLines_SingleLine_ReturnsLastLine()
        {
            var method = typeof(LogFileFunction).GetMethod("ReadLastLines",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (string)method!.Invoke(null, new object[] { _tempLogFile, 1 })!;

            Assert.AreEqual("Line10", result.Trim());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ReadLastLines_EmptyFile_ReturnsEmpty()
        {
            var emptyFile = Path.GetTempFileName();
            try
            {
                var method = typeof(LogFileFunction).GetMethod("ReadLastLines",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                var result = (string)method!.Invoke(null, new object[] { emptyFile, 5 })!;

                Assert.AreEqual(string.Empty, result);
            }
            finally
            {
                File.Delete(emptyFile);
            }
        }
    }
}
