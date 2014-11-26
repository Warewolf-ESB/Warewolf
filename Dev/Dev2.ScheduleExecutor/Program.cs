
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.Communication;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.TaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;

namespace Dev2.ScheduleExecutor
{
    internal class Program
    {
        private const string WarewolfTaskSchedulerPath = "\\warewolf\\";
        private static readonly string OutputPath = string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), GlobalConstants.SchedulerDebugPath);
        private static readonly string SchedulerLogDirectory = OutputPath +  "SchedulerLogs";
        private static readonly Stopwatch Stopwatch = new Stopwatch();
        private static readonly DateTime StartTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 5));

        private static void Main(string[] args)
        {
            try
            {

                SetupForLogging();

                Stopwatch.Start();

                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                Log("Info", "Task Started");
                if(args.Length < 2)
                {
                    Log("Error", "Invalid arguments passed in.");
                    return;
                }
                var paramters = new Dictionary<string, string>();
                for(int i = 0; i < args.Count(); i++)
                {
                    string[] singleParameters = args[i].Split(':');

                    paramters.Add(singleParameters[0],
                                  singleParameters.Skip(1).Aggregate((a, b) => String.Format("{0}:{1}", a, b)));
                }
                Log("Info", string.Format("Start execution of {0}", paramters["Workflow"]));
                try
                {
                    PostDataToWebserverAsRemoteAgent(paramters["Workflow"], paramters["TaskName"], Guid.NewGuid());
                }
                catch
                {
                    CreateDebugState("Warewolf Server Unavailable", paramters["Workflow"], paramters["TaskName"]);
                    throw;
                }
            }
            catch(Exception e)
            {
                Log("Error", string.Format("Error from execution: {0}{1}", e.Message, e.StackTrace));

                Environment.Exit(1);
            }
        }

        public static string PostDataToWebserverAsRemoteAgent(string workflowName, string taskName, Guid requestID)
        {
            string postUrl = string.Format("http://localhost:3142/services/{0}", workflowName);
            Log("Info", string.Format("Executing as {0}", CredentialCache.DefaultNetworkCredentials.UserName));
            int len = postUrl.Split('?').Count();
            if(len == 1)
            {
                string result = string.Empty;

                WebRequest req = WebRequest.Create(postUrl);
                req.Credentials = CredentialCache.DefaultNetworkCredentials;
                req.Method = "GET";

                req.Headers.Add(HttpRequestHeader.From, requestID.ToString()); // Set to remote invoke ID ;)
                req.Headers.Add(HttpRequestHeader.Cookie, "RemoteWarewolfServer");

                try
                {
                    using(var response = req.GetResponse() as HttpWebResponse)
                    {
                        if(response != null)
                        {
                            // ReSharper disable AssignNullToNotNullAttribute
                            using(var reader = new StreamReader(response.GetResponseStream()))
                            // ReSharper restore AssignNullToNotNullAttribute
                            {
                                result = reader.ReadToEnd();
                            }

                            if(response.StatusCode != HttpStatusCode.OK)
                            {
                                Log("Error", string.Format("Error from execution: {0}", result));
                            }
                            else
                            {
                                Log("Info", string.Format("Completed execution. Output: {0}", result));

                                WriteDebugItems(requestID, workflowName, taskName);
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
            return string.Empty;
        }

        private static void CreateDebugState(string result, string workflowName, string taskName)
        {
            string user = Thread.CurrentPrincipal.Identity.Name.Replace("\\", "-");
            var state = new DebugState
                {
                    HasError = true,
                    ID = Guid.NewGuid(),
                    Message = string.Format("{0}", result),
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    ErrorMessage = string.Format("{0}", result),
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
            string correlation = GetCorrelationId(WarewolfTaskSchedulerPath + taskName);
            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);
            File.WriteAllText(
                string.Format("{0}DebugItems_{1}_{2}_{3}_{4}.txt", OutputPath, workflowName.Replace("\\","_"),
                              DateTime.Now.ToString("yyyy-MM-dd"), correlation, user),
                js.SerializeToBuilder(new List<DebugState> { state }).ToString());
        }

        private static string GetCorrelationId(string taskName)
        {
            try
            {
                var factory = new TaskServiceConvertorFactory();
                DateTime time = DateTime.Now;
                ITaskEventLog eventLog = factory.CreateTaskEventLog(taskName);
                ITaskEvent events = (from a in eventLog
                                     where a.TaskCategory == "Task Started" && time > StartTime
                                     orderby a.TimeCreated
                                     select a).LastOrDefault();
                if(null != events)
                {
                    return events.Correlation;
                }
                return "";
            }
            catch(Exception e)
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

        private static void WriteDebugItems(Guid id, string workflowName, string taskName)
        {
            string user = Thread.CurrentPrincipal.Identity.Name.Replace("\\", "-");
            string getDebugItemsUrl = "http://localhost:3142/services/FetchRemoteDebugMessagesService?InvokerID=" +
                                      id.ToString();
            WebRequest req = WebRequest.Create(getDebugItemsUrl);
            req.Credentials = CredentialCache.DefaultCredentials;
            req.Method = "GET";

            using(var response = req.GetResponse() as HttpWebResponse)
            {
                if(response != null)
                {
                    Stream responseStream = response.GetResponseStream();
                    if(responseStream != null)
                    {
                        using(var reader = new StreamReader(responseStream))
                        {
                            string data = reader.ReadToEnd();
                            if(Stopwatch.ElapsedMilliseconds < 5000)
                            {
                                Thread.Sleep(5000);
                            }
                            string correlation = GetCorrelationId(WarewolfTaskSchedulerPath + taskName);
                            File.WriteAllText(
                                string.Format("{0}DebugItems_{1}_{2}_{3}_{4}.txt", OutputPath, workflowName.Replace("\\", "_"),
                                              DateTime.Now.ToString("yyyy-MM-dd"), correlation, user), data);
                        }
                    }
                }
            }
        }

        private static void Log(string logType, string logMessage)
        {
            try
            {


                using(
                    TextWriter tsw =
                        new StreamWriter(new FileStream(SchedulerLogDirectory + "/" + DateTime.Now.ToString("yyyy-MM-dd"),
                                                        FileMode.Append)))
                {
                    tsw.WriteLine();
                    tsw.Write(logType);
                    tsw.Write("----");
                    tsw.WriteLine(logMessage);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {


            }
        }

        private static void SetupForLogging()
        {
            bool hasSchedulerLogDirectory = Directory.Exists(SchedulerLogDirectory);
            if(hasSchedulerLogDirectory)
            {
                var directoryInfo = new DirectoryInfo(SchedulerLogDirectory);
                FileInfo[] logFiles = directoryInfo.GetFiles();
                if(logFiles.Count() > 20)
                {
                    try
                    {



                        FileInfo fileInfo = logFiles.OrderByDescending(f => f.LastWriteTime).First();
                        fileInfo.Delete();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
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
