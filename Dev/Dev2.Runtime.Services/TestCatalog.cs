using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Util;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime
{
    public class TestCatalog : ITestCatalog
    {
        readonly DirectoryWrapper _directoryWrapper;
        readonly Dev2JsonSerializer _serializer;
        readonly FileWrapper _fileWrapper;

        static readonly Lazy<TestCatalog> LazyCat = new Lazy<TestCatalog>(() =>
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
            Tests = new ConcurrentDictionary<Guid, List<IServiceTestModelTO>>();
            _serializer = new Dev2JsonSerializer();

        }

        public ConcurrentDictionary<Guid, List<IServiceTestModelTO>> Tests { get; private set; }

        public void SaveTests(Guid resourceID, List<IServiceTestModelTO> serviceTestModelTos)
        {
            if (serviceTestModelTos != null && serviceTestModelTos.Count > 0)
            {
                foreach (var serviceTestModelTo in serviceTestModelTos)
                {
                    SaveTestToDisk(resourceID, serviceTestModelTo);
                }
                var dir = Path.Combine(EnvironmentVariables.TestPath, resourceID.ToString());
                Tests.AddOrUpdate(resourceID, GetTestList(dir), (id, list) => GetTestList(dir));
            }
        }

        public void SaveTest(Guid resourceID, IServiceTestModelTO test)
        {
            SaveTestToDisk(resourceID, test);
            var existingTests = Tests.GetOrAdd(resourceID, new List<IServiceTestModelTO>());
            var found = existingTests.FirstOrDefault(to => to.TestName.Equals(test.TestName, StringComparison.CurrentCultureIgnoreCase));
            if (found == null)
            {
                existingTests.Add(test);
            }
            else
            {
                existingTests.Remove(found);
                existingTests.Add(test);
            }
        }

        static void UpdateTestToInvalid(List<IServiceTestModelTO> testsToUpdate)
        {
            foreach (var serviceTestModelTO in testsToUpdate)
            {
                serviceTestModelTO.TestFailing = false;
                serviceTestModelTO.TestPassed = false;
                serviceTestModelTO.TestPending = false;
                serviceTestModelTO.TestInvalid = true;
                UpdateStepOutputsForTest(serviceTestModelTO);
                if (serviceTestModelTO.Outputs != null)
                {
                    foreach (var serviceTestOutput in serviceTestModelTO.Outputs)
                    {
                        if (serviceTestOutput.Result != null)
                        {
                            serviceTestOutput.Result.RunTestResult = RunResult.TestInvalid;
                        }
                    }
                }

            }
        }

        public void UpdateTestsBasedOnIOChange(Guid resourceID, IList<IDev2Definition> inputDefs, IList<IDev2Definition> outputDefs)
        {
            var testsToUpdate = Fetch(resourceID);
            if (testsToUpdate != null && testsToUpdate.Count > 0)
            {
                foreach (var serviceTestModelTO in testsToUpdate)
                {
                    UpdateTestToInvalid(testsToUpdate);
                    if (inputDefs != null && inputDefs.Count > 0)
                    {
                        UpdateInputsForTest(serviceTestModelTO, inputDefs);
                    }
                    if (outputDefs != null && outputDefs.Count > 0)
                    {
                        UpdateOutputsForTest(serviceTestModelTO, outputDefs);
                    }
                }
                SaveTests(resourceID, testsToUpdate);
            }
        }

        static void UpdateStepOutputsForTest(IServiceTestModelTO serviceTestModelTo)
        {
            if (serviceTestModelTo.TestSteps != null)
            {
                foreach (var serviceTestStep in serviceTestModelTo.TestSteps)
                {
                    if (serviceTestStep.Children != null)
                    {
                        var childs = serviceTestStep.Children.Flatten(step => step.Children);
                        foreach (var child in childs)
                        {
                            child.Result = new TestRunResult { RunTestResult = RunResult.TestInvalid };
                            foreach (var serviceTestOutput in child.StepOutputs)
                            {
                                serviceTestOutput.Result = new TestRunResult { RunTestResult = RunResult.TestInvalid };
                            }
                        }
                    }
                    serviceTestStep.Result = new TestRunResult { RunTestResult = RunResult.TestInvalid };
                    foreach (var serviceTestOutput in serviceTestStep.StepOutputs)
                    {
                        serviceTestOutput.Result = new TestRunResult { RunTestResult = RunResult.TestInvalid };
                    }

                }
            }
        }

        public void ReloadAllTests()
        {
            Tests.Clear();
            Load();
        }

        static void UpdateOutputsForTest(IServiceTestModelTO serviceTestModelTO, IList<IDev2Definition> outputDefs)
        {
            if (outputDefs.Count == 0)
            {
                serviceTestModelTO.Outputs = new List<IServiceTestOutput>();
            }
            else
            {
                if (serviceTestModelTO.Outputs == null)
                {
                    serviceTestModelTO.Outputs = new List<IServiceTestOutput>();
                }
                foreach (var dev2Definition in outputDefs)
                {
                    if (dev2Definition.IsRecordSet)
                    {
                        ProcessRecordsetOutputs(serviceTestModelTO, dev2Definition);
                    }
                    else
                    {
                        if (serviceTestModelTO.Outputs.FirstOrDefault(output => output.Variable == dev2Definition.Name) == null)
                        {
                            serviceTestModelTO.Outputs.Add(new ServiceTestOutputTO
                            {
                                Variable = dev2Definition.Name,
                                AssertOp = "=",
                                Value = ""
                            });
                        }
                    }
                }

                for (int i = serviceTestModelTO.Outputs.Count - 1; i >= 0; i--)
                {
                    var output = serviceTestModelTO.Outputs[i];
                    if (outputDefs.FirstOrDefault(definition =>
                    {
                        if (definition.IsRecordSet)
                        {
                            var rec = DataListUtil.CreateRecordsetDisplayValue(definition.RecordSetName, definition.Name, "");
                            var inRec = DataListUtil.ReplaceRecordsetIndexWithBlank(output.Variable);
                            return rec == inRec;
                        }
                        return definition.Name == output.Variable;
                    }) == null)
                    {
                        serviceTestModelTO.Outputs.Remove(output);
                    }
                }
                foreach (var serviceTestOutput in serviceTestModelTO.Outputs)
                {
                    serviceTestOutput.Result = new TestRunResult { RunTestResult = RunResult.TestInvalid };
                }
                serviceTestModelTO.Outputs.Sort((output, testOutput) => string.Compare(output.Variable, testOutput.Variable, StringComparison.InvariantCultureIgnoreCase));
            }

        }

        static void ProcessRecordsetOutputs(IServiceTestModelTO serviceTestModelTO, IDev2Definition dev2Definition)
        {
            var rec = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, "");
            var indexes = serviceTestModelTO.Outputs.Where(output => DataListUtil.ExtractRecordsetNameFromValue(output.Variable) == dev2Definition.RecordSetName).Select(input => DataListUtil.ExtractIndexRegionFromRecordset(input.Variable)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (serviceTestModelTO.Outputs.FirstOrDefault(output => DataListUtil.ReplaceRecordsetIndexWithBlank(output.Variable) == rec) == null)
            {
                if (indexes.Count == 0)
                {
                    serviceTestModelTO.Outputs.Add(new ServiceTestOutputTO
                    {
                        Variable = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, "1"),
                        AssertOp = "=",
                        Value = ""
                    });
                }
                else
                {
                    foreach (var index in indexes)
                    {
                        serviceTestModelTO.Outputs.Add(new ServiceTestOutputTO
                        {
                            Variable = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, index),
                            AssertOp = "=",
                            Value = ""
                        });
                    }
                }
            }
        }

        static void UpdateInputsForTest(IServiceTestModelTO serviceTestModelTO, IList<IDev2Definition> inputDefs)
        {
            if (inputDefs.Count == 0)
            {
                serviceTestModelTO.Inputs = new List<IServiceTestInput>();
            }
            else
            {
                if (serviceTestModelTO.Inputs == null)
                {
                    serviceTestModelTO.Inputs = new List<IServiceTestInput>();
                }
                foreach (var dev2Definition in inputDefs)
                {
                    if (dev2Definition.IsRecordSet)
                    {
                        ProcessRecordsetInputs(serviceTestModelTO, dev2Definition);
                    }
                    else
                    {
                        if (serviceTestModelTO.Inputs.FirstOrDefault(input => input.Variable == dev2Definition.Name) == null)
                        {
                            serviceTestModelTO.Inputs.Add(new ServiceTestInputTO
                            {
                                Variable = dev2Definition.Name,
                                Value = "",
                                EmptyIsNull = false
                            });
                        }
                    }
                }

                for (int i = serviceTestModelTO.Inputs.Count - 1; i >= 0; i--)
                {
                    var input = serviceTestModelTO.Inputs[i];
                    if (inputDefs.FirstOrDefault(definition =>
                        {
                            if (definition.IsRecordSet)
                            {
                                var rec = DataListUtil.CreateRecordsetDisplayValue(definition.RecordSetName, definition.Name, "");
                                var inRec = DataListUtil.ReplaceRecordsetIndexWithBlank(input.Variable);
                                return rec == inRec;
                            }
                            return definition.Name == input.Variable;
                        }) == null)
                    {
                        serviceTestModelTO.Inputs.Remove(input);
                    }
                }
                serviceTestModelTO.Inputs.Sort((input, testInput) => string.Compare(input.Variable, testInput.Variable, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        static void ProcessRecordsetInputs(IServiceTestModelTO serviceTestModelTO, IDev2Definition dev2Definition)
        {
            var rec = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, "");
            var indexes = serviceTestModelTO.Inputs.Where(input => DataListUtil.ExtractRecordsetNameFromValue(input.Variable) == dev2Definition.RecordSetName).Select(input => DataListUtil.ExtractIndexRegionFromRecordset(input.Variable)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (serviceTestModelTO.Inputs.FirstOrDefault(input => DataListUtil.ReplaceRecordsetIndexWithBlank(input.Variable) == rec) == null)
            {
                if (indexes.Count == 0)
                {
                    serviceTestModelTO.Inputs.Add(new ServiceTestInputTO
                    {
                        Variable = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, "1"),
                        Value = "",
                        EmptyIsNull = false
                    });
                }
                else
                {
                    foreach (var index in indexes)
                    {
                        serviceTestModelTO.Inputs.Add(new ServiceTestInputTO
                        {
                            Variable = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, index),
                            Value = "",
                            EmptyIsNull = false
                        });
                    }
                }
            }
        }

        void SaveTestToDisk(Guid resourceId, IServiceTestModelTO serviceTestModelTo)
        {
            var dirPath = GetTestPathForResourceId(resourceId);
            _directoryWrapper.CreateIfNotExists(dirPath);
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

        public void Load()
        {
            Tests = new ConcurrentDictionary<Guid, List<IServiceTestModelTO>>();
            var resourceTestDirectories = _directoryWrapper.GetDirectories(EnvironmentVariables.TestPath);
            foreach (var resourceTestDirectory in resourceTestDirectories)
            {
                var resIdString = DirectoryWrapper.GetDirectoryName(resourceTestDirectory);
                if (Guid.TryParse(resIdString, out Guid resId))
                {
                    Tests.AddOrUpdate(resId, GetTestList(resourceTestDirectory), (id, list) => GetTestList(resourceTestDirectory));
                }

            }
        }

        List<IServiceTestModelTO> GetTestList(string resourceTestDirectory)
        {
            var serviceTestModelTos = new List<IServiceTestModelTO>();
            var files = _directoryWrapper.GetFiles(resourceTestDirectory);
            foreach (var file in files)
            {
                var reader = new StreamReader(file);
                var testModel = _serializer.Deserialize<IServiceTestModelTO>(reader);
                serviceTestModelTos.Add(testModel);
            }
            return serviceTestModelTos;
        }

        public List<IServiceTestModelTO> FetchAllTests()
        {
            var list = new List<IServiceTestModelTO>();
            if (Tests != null)
            {
                foreach (var test in Tests)
                {
                    list.AddRange(test.Value);
                }
            }

            return list;
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
                if (Tests.TryGetValue(resourceID, out List<IServiceTestModelTO> testList))
                {
                    var foundTestToDelete = testList.FirstOrDefault(to => to.TestName.Equals(testName, StringComparison.InvariantCultureIgnoreCase));
                    if (foundTestToDelete != null)
                    {
                        Dev2Logger.Debug("Removing Test: " + testName + Environment.NewLine + Environment.StackTrace, GlobalConstants.WarewolfDebug);
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
                _directoryWrapper.Delete(dirPath, true);
                Tests.TryRemove(resourceId, out List<IServiceTestModelTO> removedTests);
            }
        }

        public void DeleteAllTests(List<string> testsToList)
        {
            var info = new DirectoryInfo(EnvironmentVariables.TestPath);
            if (!info.Exists)
            {
                return;
            }

            var fileInfos = info.GetDirectories();
            foreach (var fileInfo in fileInfos.Where(fileInfo => !testsToList.Contains(fileInfo.Name.ToUpper())))
            {
                DirectoryHelper.CleanUp(fileInfo.FullName);
            }
            Load();
        }
        static string GetTestPathForResourceId(Guid resourceId)
        {
            var testPath = EnvironmentVariables.TestPath;
            var dirPath = Path.Combine(testPath, resourceId.ToString());
            return dirPath;
        }

        public IServiceTestModelTO FetchTest(Guid resourceID, string testName)
        {
            if (Tests.TryGetValue(resourceID, out List<IServiceTestModelTO> testList))
            {
                var foundTestToDelete = testList.FirstOrDefault(to => to.TestName.Equals(testName, StringComparison.InvariantCultureIgnoreCase));
                if (foundTestToDelete != null)
                {
                    return foundTestToDelete;
                }
            }
            return null;
        }           
    }
}