using Dev2.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Exposes the Warewolf server log file for diagnostics.
    /// Mirrors <c>GetLogFileServiceHandler</c> on the full server.
    /// Supports an optional <c>numLines</c> query parameter:
    ///   - positive value → return last N lines
    ///   - zero or negative → return entire file
    ///   - absent → return last 10 lines (default)
    /// </summary>
    public sealed class LogFileFunction
    {
        private const int DefaultLines = 10;

        [Function("GetLogFile")]
        public async Task<HttpResponseData> GetLogFile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "internal/getlogfile")] HttpRequestData request)
        {
            var logFilePath = EnvironmentVariables.ServerLogFile;

            if (string.IsNullOrWhiteSpace(logFilePath) || !File.Exists(logFilePath))
            {
                var notFound = request.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Log file not found.");
                return notFound;
            }

            int numLines = DefaultLines;
            var numLinesParam = request.Url.Query
                ?.Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.TrimStart('?').Split('=', 2))
                .FirstOrDefault(p => p[0].Equals("numLines", StringComparison.OrdinalIgnoreCase));

            if (numLinesParam is { Length: 2 } && int.TryParse(numLinesParam[1], out var parsed))
                numLines = parsed;

            string content;
            if (numLines > 0)
                content = ReadLastLines(logFilePath, numLines);
            else
                content = await File.ReadAllTextAsync(logFilePath);

            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync(content);
            return response;
        }

        /// <summary>
        /// Reads the last <paramref name="count"/> lines from the file efficiently
        /// by scanning backwards from the end of the file.
        /// </summary>
        private static string ReadLastLines(string filePath, int count)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (fs.Length == 0)
                    return string.Empty;

                var lines = new string[count];
                int lineIndex = count - 1;
                long position = fs.Length - 1;
                var buffer = new StringBuilder();

                fs.Seek(position, SeekOrigin.Begin);
                // Skip trailing newline
                if (fs.ReadByte() == '\n')
                    position--;

                while (position >= 0 && lineIndex >= 0)
                {
                    fs.Seek(position, SeekOrigin.Begin);
                    var ch = (char)fs.ReadByte();
                    if (ch == '\n')
                    {
                        lines[lineIndex--] = Reverse(buffer);
                        buffer.Clear();
                    }
                    else if (ch != '\r')
                    {
                        buffer.Append(ch);
                    }
                    position--;
                }

                if (buffer.Length > 0 && lineIndex >= 0)
                    lines[lineIndex--] = Reverse(buffer);

                return string.Join(Environment.NewLine, lines.Skip(lineIndex + 1));
            }
            catch
            {
                // Fallback: read all and take last N
                var allLines = File.ReadAllLines(filePath);
                return string.Join(Environment.NewLine, allLines.TakeLast(count));
            }
        }

        private static string Reverse(StringBuilder sb)
        {
            var chars = new char[sb.Length];
            for (int i = 0; i < sb.Length; i++)
                chars[i] = sb[sb.Length - 1 - i];
            return new string(chars);
        }
    }
}
