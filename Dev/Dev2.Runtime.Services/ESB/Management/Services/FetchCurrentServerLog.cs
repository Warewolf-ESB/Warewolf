#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchCurrentServerLog : DefaultEsbManagementEndpoint
    {
        const int DefaultNumberOfLines = 1000;
        readonly string _serverLogPath;

        public FetchCurrentServerLog()
            : this(EnvironmentVariables.ServerLogFile)
        {
        }

        public FetchCurrentServerLog(string serverLogPath)
        {
            _serverLogPath = serverLogPath;
        }

        public string ServerLogPath => _serverLogPath;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var numberOfLines = DefaultNumberOfLines;
                if (values != null && values.TryGetValue("NumberOfLines", out var numberOfLinesValue))
                {
                    if (int.TryParse(numberOfLinesValue?.ToString(), out var parsedLines) && parsedLines > 0)
                    {
                        numberOfLines = parsedLines;
                    }
                }

                Dev2Logger.Info($"Fetch Server Log Started, requesting {numberOfLines} lines", GlobalConstants.WarewolfInfo);
                var result = new ExecuteMessage { HasError = false };
                if (File.Exists(_serverLogPath))
                {
                    var allLines = ReadLastNLines(_serverLogPath, numberOfLines);
                    result.Message.Append(string.Join("\n", allLines));
                    Dev2Logger.Info($"Fetch Server Log returning {allLines.Count} lines", GlobalConstants.WarewolfInfo);
                }
                var serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(result);
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Fetch Server Log Error", err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        private static List<string> ReadLastNLines(string filePath, int numberOfLines)
        {
            var lines = new List<string>();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream))
            {
                while (!streamReader.EndOfStream)
                {
                    lines.Add(streamReader.ReadLine());
                }
            }

            if (lines.Count > numberOfLines)
            {
                return lines.Skip(lines.Count - numberOfLines).ToList();
            }
            return lines;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><NumberOfLines ColumnIODirection=\"Input\"></NumberOfLines><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchCurrentServerLogService";
    }
}
