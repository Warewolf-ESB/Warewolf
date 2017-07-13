using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class LogDataServiceBase
    {
        private string _serverLogFilePath;
        public string ServerLogFilePath
        {
            get => _serverLogFilePath ?? EnvironmentVariables.ServerLogFile;
            set => _serverLogFilePath = value;
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Administrator;
        }

        public IEnumerable<dynamic> BuildTempObjects()
        {
            var tmpObjects = new List<object>();
            var buffor = new Queue<string>();
            Stream stream = File.Open(ServerLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var file = new StreamReader(stream);
            while (!file.EndOfStream)
            {
                var line = file.ReadLine();

                buffor.Enqueue(line);
            }
            var logData = buffor.AsQueryable();

            foreach (var singleEntry in logData)
            {
                var matches = GetLogEntryValues(singleEntry);
                if (matches.Length > 1)
                {
                    var match = matches;
                    var isUrl = match[4].StartsWith("Request URL", StringComparison.Ordinal);
                    var cleanUrl = match[4].Replace("Request URL [ ", "").Replace(" ]", "");
                    var tmpObj = new
                    {
                        ExecutionId = match[3],
                        LogType = match[2],
                        DateTime = match[1],
                        Message = isUrl ? "" : match[4],
                        Url = isUrl ? cleanUrl : "",
                    };
                    tmpObjects.Add(tmpObj);
                }
            }
            return tmpObjects;
        }

        private string[] GetLogEntryValues(string singleEntry)
        {
            return Regex.Split(singleEntry, GlobalConstants.LogFileRegex);
        }
    }
}