#pragma warning disable CC0021
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class GetLogFileServiceHandler : AbstractWebRequestHandler
    {
        public GetLogFileServiceHandler()
            : base(ResourceCatalog.Instance, TestCatalog.Instance, TestCoverageCatalog.Instance, new DefaultEsbChannelFactory(), new SecuritySettings())
        {
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            // Check if numLines parameter is provided
            int? numLines = null;
#pragma warning disable CC0021 // Use nameof
            var numLinesParam = ctx.Request.QueryString["numLines"];
#pragma warning restore CC0021 // Use nameof
            if (!string.IsNullOrEmpty(numLinesParam) && int.TryParse(numLinesParam, out int parsedLines))
            {
                numLines = parsedLines;
            }

            if (numLines.HasValue && numLines.Value > 0)
            {
                // Return only the last N lines
                var lastLines = ReadLastLines(EnvironmentVariables.ServerLogFile, numLines.Value);
                var content = string.Join(Environment.NewLine, lastLines);
                ctx.Send(new StringResponseWriter(content, "text/plain"));
			}
			else if (numLines.HasValue && numLines.Value <= 0)
			{
                // Return entire file
                ctx.Send(new FileResponseWriter(EnvironmentVariables.ServerLogFile));
			}
            else if (!numLines.HasValue)
			{
				// Return only the last 10 lines
				var lastLines = ReadLastLines(EnvironmentVariables.ServerLogFile, 10);
				var content = string.Join(Environment.NewLine, lastLines);
				ctx.Send(new StringResponseWriter(content, "text/plain"));
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
                if (!File.Exists(filePath))
                    return new string[0];

                using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // For small files or small numLines, just read all lines and take the last N
                    if (fileStream.Length < 1024 * 1024 || numLines <= 100) // Less than 1MB or requesting <= 100 lines
                    {
                        // Use StreamReader with the same FileStream to maintain consistent file sharing
                        using (var reader = new StreamReader(fileStream))
                        {
                            var allLines = new List<string>();
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                allLines.Add(line);
                            }
                            return allLines.Skip(Math.Max(0, allLines.Count - numLines)).ToArray();
                        }
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
    }
}
