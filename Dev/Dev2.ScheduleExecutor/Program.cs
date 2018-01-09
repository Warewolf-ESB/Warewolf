/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.Communication;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.TaskScheduler.Wrappers;
using Warewolf.Resource.Errors;

namespace Dev2.ScheduleExecutor
{
    class Program
    {
        const string WarewolfTaskSchedulerPath = "\\warewolf\\";
        static readonly string OutputPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\{GlobalConstants.SchedulerDebugPath}";
        static readonly string SchedulerLogDirectory = OutputPath + "SchedulerLogs";
        static readonly Stopwatch Stopwatch = new Stopwatch();
        static readonly DateTime StartTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 5));

        static void Main(string[] args)
        {

            try
            {

                SetupForLogging();

                Stopwatch.Start();

                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                Log("Info", "Task Started");
                if (args.Length < 2)
                {
                    Log("Error", ErrorResource.InvalidArguments);
                    return;
                }
                var paramters = new Dictionary<string, string>();
                for (int i = 0; i < args.Length; i++)
                {
                    var singleParameters = args[i].Split(':');

                    paramters.Add(singleParameters[0],
                                  singleParameters.Skip(1).Aggregate((a, b) => $"{a}:{b}"));
                }
                Log("Info", $"Start execution of {paramters["Workflow"]}");
                try
                {
                    if (paramters.ContainsKey("ResourceId"))
                    {
                        PostDataToWebserverAsRemoteAgent(paramters["Workflow"], paramters["TaskName"], Guid.NewGuid(), paramters["ResourceId"]);
                    }
                    else
                    {
                        PostDataToWebserverAsRemoteAgent(paramters["Workflow"], paramters["TaskName"], Guid.NewGuid());

                    }
                }
                catch
                {
                    CreateDebugState("Warewolf Server Unavailable", paramters["Workflow"], paramters["TaskName"]);
                    throw;
                }
            }
            catch (Exception e)
            {
                Log("Error", $"Error from execution: {e.Message}{e.StackTrace}");

                Environment.Exit(1);
            }
        }

        public static string PostDataToWebserverAsRemoteAgent(string workflowName, string taskName, Guid requestID)
        {
            var postUrl = $"http://localhost:3142/services/{workflowName}";
            Log("Info", $"Executing as {CredentialCache.DefaultNetworkCredentials.UserName}");
            var len = postUrl.Split('?').Length;
            if (len == 1)
            {
                var result = string.Empty;

                var req = WebRequest.Create(postUrl);
                req.Credentials = CredentialCache.DefaultNetworkCredentials;
                req.Method = "GET";

                try
                {
                    using (var response = req.GetResponse() as HttpWebResponse)
                    {
                        if (response != null)
                        {
                            
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            
                            {
                                result = reader.ReadToEnd();
                            }

                            if (response.StatusCode != HttpStatusCode.OK || result.StartsWith("<FatalError>"))
                            {
                                Log("Error", $"Error from execution: {result}");
								CreateDebugState(result, workflowName, taskName);
                                Environment.Exit(1);
                            }
                            else
                            {
                                Log("Info", $"Completed execution. Output: {result}");

                                WriteDebugItems(workflowName, taskName, result);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    CreateDebugState("Warewolf Server Unavailable", workflowName, taskName);
                    Console.Write(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Log("Error",
                        string.Format(
                            "Error executing request. Exception: {0}" + Environment.NewLine + "StackTrace: {1}",
                            e.Message, e.StackTrace));
                    Environment.Exit(1);
                }
                return result;
            }
            return string.Empty;
        }

        public static string PostDataToWebserverAsRemoteAgent(string workflowName, string taskName, Guid requestID, string resourceId)
        {
            var portNumber = "3142";
            if(File.Exists("userServerSettings.config"))
            {
                var doc = XDocument.Load("userServerSettings.config");
                var appSettingsElement = doc.Element("appSettings");
                var webServerPortElement = appSettingsElement?.Elements()
                    .FirstOrDefault(element => element.Name == "add" && element.Attribute("key")?.Value == "webServerPort");
                if(webServerPortElement != null)
                {
                    portNumber = webServerPortElement.Attribute("value")?.Value;
                }
            }
            else
            {
                Log("Error", $"userServerSettings.config does not exist in {Directory.GetCurrentDirectory()}");
            }

            var postUrl = $"http://localhost:{portNumber}/services/{resourceId}.bite";
            Log("Info", $"Executing as {CredentialCache.DefaultNetworkCredentials.UserName}");
            var result = string.Empty;

            var req = WebRequest.Create(postUrl);
            req.Credentials = CredentialCache.DefaultNetworkCredentials;
            req.Method = "GET";

            try
            {
                using(var response = req.GetResponse() as HttpWebResponse)
                {
                    if(response != null)
                    {
                        
                        using(var reader = new StreamReader(response.GetResponseStream()))
                            
                        {
                            result = reader.ReadToEnd();
                        }

                        if(response.StatusCode != HttpStatusCode.OK || result.StartsWith("<FatalError>"))
                        {
                            Log("Error", $"Error from execution: {result}");
                            CreateDebugState(result, workflowName, taskName);
                            Environment.Exit(1);
                        }
                        else
                        {
                            Log("Info", $"Completed execution. Output: {result}");
                            WriteDebugItems(workflowName, taskName, result);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                CreateDebugState("Warewolf Server Unavailable", workflowName, taskName);
                Console.Write(e.Message);
                Console.WriteLine(e.StackTrace);
                Log("Error",
                    string.Format(
                        "Error executing request. Exception: {0}" + Environment.NewLine + "StackTrace: {1}",
                        e.Message, e.StackTrace));
                Environment.Exit(1);
            }
            return result;
        }

        static void CreateDebugState(string result, string workflowName, string taskName)
        {
            var user = Thread.CurrentPrincipal.Identity.Name.Replace("\\", "-");
            var state = new DebugState
            {
                HasError = true,
                ID = Guid.NewGuid(),
                Message = $"{result}",
                StartTime = StartTime,
                EndTime = DateTime.Now,
                ErrorMessage = $"{result}",
                DisplayName = workflowName
            };
            var debug = new DebugItem();
            debug.Add(new DebugItemResult
            {
                Type = DebugItemResultType.Label,
                Value = "Warewolf Execution Error:",
                Label = "Scheduler Execution Error",
                Variable = result
            });
            var js = new Dev2JsonSerializer();
            Thread.Sleep(5000);
            var correlation = GetCorrelationId(WarewolfTaskSchedulerPath + taskName);
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            File.WriteAllText(
                $"{OutputPath}DebugItems_{workflowName.Replace("\\", "_")}_{DateTime.Now.ToString("yyyy-MM-dd")}_{correlation}_{user}.txt",
                js.SerializeToBuilder(new List<DebugState> { state }).ToString());
        }

        static string GetCorrelationId(string taskName)
        {
            try
            {
                var factory = new TaskServiceConvertorFactory();
                var time = DateTime.Now;
                var eventLog = factory.CreateTaskEventLog(taskName);
                var events = (from a in eventLog
                                     where a.TaskCategory == "Task Started" && time > StartTime
                                     orderby a.TimeCreated
                                     select a).LastOrDefault();
                if (null != events)
                {
                    return events.Correlation;
                }
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Log("Error",
                    string.Format(
                        "Error creating task history. Exception: {0}" + Environment.NewLine + "StackTrace: {1}",
                        e.Message, e.StackTrace));
                Environment.Exit(1);
            }
            return "";
        }

        static void WriteDebugItems(string workflowName, string taskName, string result)
        {
            var user = Thread.CurrentPrincipal.Identity.Name.Replace("\\", "-");

            var state = new DebugState
            {
                HasError = result.Contains("Exception"),
                ID = Guid.NewGuid(),
                StartTime = StartTime,
                EndTime = DateTime.Now,
                ActivityType = ActivityType.Workflow,
                ExecutingUser = user,
                Server = "localhost",
                ServerID = Guid.Empty,
                DisplayName = workflowName
            };
            if (!string.IsNullOrEmpty(result))
            {
                var data = DataListUtil.AdjustForEncodingIssues(result);
                var isXml = DataListUtil.IsXml(data, out bool isFragment);
                if (isXml)
                {
                    var xmlData = XElement.Parse(data);
                    var allChildren = xmlData.Elements();
                    var groupedData = allChildren.GroupBy(element => element.Name);

                    var recSets = groupedData as IGrouping<XName, XElement>[] ?? groupedData.ToArray();
                    foreach (var grouping in recSets)
                    {
                        var debugItem = new DebugItem();
                        foreach (var name in grouping)
                        {
                            if (name.HasElements)
                            {
                                var debugItemResult = ProcessRecordSet(name, name.Elements());
                                debugItem.ResultsList.AddRange(debugItemResult);
                            }
                            else
                            {
                                var debugItemResult = new DebugItemResult
                                {
                                    Variable = DataListUtil.AddBracketsToValueIfNotExist(name.Name.LocalName),
                                    Value = name.Value,
                                    Operator = "=",
                                    Type = DebugItemResultType.Variable
                                };
                                debugItem.ResultsList.Add(debugItemResult);
                            }
                        }
                        state.Outputs.Add(debugItem);
                    }
                }
            }
            var js = new Dev2JsonSerializer();
            Thread.Sleep(5000);
            var correlation = GetCorrelationId(WarewolfTaskSchedulerPath + taskName);
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            File.WriteAllText(
                $"{OutputPath}DebugItems_{workflowName.Replace("\\", "_")}_{DateTime.Now.ToString("yyyy-MM-dd")}_{correlation}_{user}.txt",
                js.SerializeToBuilder(new List<DebugState> { state }).ToString());

        }



        static List<IDebugItemResult> ProcessRecordSet(XElement recordSetElement, IEnumerable<XElement> elements)
        {
            var processRecordSet = new List<IDebugItemResult>();
            var recSetName = recordSetElement.Name.LocalName;
            var xAttribute = recordSetElement.Attribute("Index");
            if (xAttribute != null)
            {
                var index = xAttribute.Value;

                foreach (var xElement in elements)
                {
                    var debugItemResult = new DebugItemResult
                    {
                        GroupName = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(recSetName, true)),
                        Value = xElement.Value,
                        GroupIndex = int.Parse(index),
                        Variable = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(recSetName, xElement.Name.LocalName, index)),
                        Operator = "=",
                        Type = DebugItemResultType.Variable
                    };
                    processRecordSet.Add(debugItemResult);
                }
            }
            return processRecordSet;
        }

        static void Log(string logType, string logMessage)
        {
            try
            {
                using (
                    TextWriter tsw =
                        new StreamWriter(new FileStream(SchedulerLogDirectory + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log",
                                                        FileMode.Append)))
                {
                    tsw.WriteLine();
                    tsw.Write(logType);
                    tsw.Write("----");
                    tsw.WriteLine(logMessage);
                }
            }

            catch

            {
            }
        }

        static void SetupForLogging()
        {
            var hasSchedulerLogDirectory = Directory.Exists(SchedulerLogDirectory);
            if (hasSchedulerLogDirectory)
            {
                var directoryInfo = new DirectoryInfo(SchedulerLogDirectory);
                var logFiles = directoryInfo.GetFiles();
                if (logFiles.Length > 20)
                {
                    try
                    {
                        var fileInfo = logFiles.OrderByDescending(f => f.LastWriteTime).First();
                        fileInfo.Delete();
                    }

                    catch

                    {
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(SchedulerLogDirectory);
            }
        }
    }
}
