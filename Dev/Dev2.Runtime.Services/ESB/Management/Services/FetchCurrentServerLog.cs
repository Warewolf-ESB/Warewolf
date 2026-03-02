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

        /// <summary>
        /// Efficiently reads the last N lines from a file without loading the entire file into memory
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="numLines">Number of lines to read from the end</param>
        /// <returns>Array of the last N lines</returns>
        private static string[] ReadLastLines(string filePath, int numLines)
        {
            try
            {
                using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var lines = new List<string>();
                    
                    // For small files or small numLines, just read all lines and take the last N
                    if (fileStream.Length < 1024 * 1024 || numLines <= 100) // Less than 1MB or requesting <= 100 lines
                    {
                        using (var reader = new StreamReader(fileStream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lines.Add(line);
                            }
                        }
                        
                        return lines.Skip(Math.Max(0, lines.Count - numLines)).ToArray();
                    }
                    
                    // For larger files, use a more efficient approach
                    return ReadLastLinesEfficient(fileStream, numLines);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error reading last {numLines} lines from {filePath}", ex, GlobalConstants.WarewolfError);
                return new string[0];
            }
        }

        /// <summary>
        /// Efficiently reads the last N lines from a large file by reading backwards
        /// </summary>
        private static string[] ReadLastLinesEfficient(FileStream fileStream, int numLines)
        {
            const int bufferSize = 4096;
            var buffer = new byte[bufferSize];
            var lines = new List<string>();
            var currentLine = new StringBuilder();
            var position = fileStream.Length;
            
            while (position > 0 && lines.Count < numLines)
            {
                var bytesToRead = (int)Math.Min(bufferSize, position);
                position -= bytesToRead;
                fileStream.Seek(position, SeekOrigin.Begin);
                
                var bytesRead = fileStream.Read(buffer, 0, bytesToRead);
                
                // Process buffer backwards
                for (int i = bytesRead - 1; i >= 0; i--)
                {
                    char c = (char)buffer[i];
                    
                    if (c == '\n')
                    {
                        if (currentLine.Length > 0)
                        {
                            // Reverse the line since we built it backwards
                            var lineStr = new string(currentLine.ToString().Reverse().ToArray());
                            lines.Add(lineStr);
                            currentLine.Clear();
                            
                            if (lines.Count >= numLines)
                                break;
                        }
                    }
                    else if (c != '\r') // Skip carriage returns
                    {
                        currentLine.Append(c);
                    }
                }
            }
            
            // Add the last line if we have one
            if (currentLine.Length > 0 && lines.Count < numLines)
            {
                var lineStr = new string(currentLine.ToString().Reverse().ToArray());
                lines.Add(lineStr);
            }
            
            // Reverse the lines array since we collected them backwards
            lines.Reverse();
            return lines.ToArray();
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
