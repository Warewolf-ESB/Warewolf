using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class GetLogFileServiceHandler : AbstractWebRequestHandler
    {
        public override void ProcessRequest(ICommunicationContext ctx)
        {
            if (ctx.Request.QueryString.HasKeys())
            {
                var numberOfLines = GlobalConstants.LogFileNumberOfLines;
                var numberOfLinesString = ctx.Request.QueryString["numLines"];
                if (numberOfLinesString != null && int.TryParse(numberOfLinesString, out int numLines) && numLines > 0)
                {
                    numberOfLines = numLines;
                }

                string serverLogFile;
                var logFile = EnvironmentVariables.ServerLogFile;
                if (File.Exists(logFile))
                {
                    var buffor = new Queue<string>(numberOfLines);
                    Stream stream = File.Open(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var file = new StreamReader(stream);
                    while (!file.EndOfStream)
                    {
                        var line = file.ReadLine();

                        if (buffor.Count >= numberOfLines)
                        {
                            buffor.Dequeue();
                        }

                        buffor.Enqueue(line);
                    }
                    var lastLines = buffor.ToArray();
                    serverLogFile = string.Join(Environment.NewLine, lastLines);
                }
                else
                {
                    serverLogFile = "Could not locate Warewolf Server log file.";
                }
                var response = new StringResponseWriter(serverLogFile, "text/xml");
                ctx.Send(response);
            }
            else
            {
                var result = GetFileFromPath(EnvironmentVariables.ServerLogFile);
                ctx.Send(result);
            }
        }

        static IResponseWriter GetFileFromPath(string filePath)
        {
            return new FileResponseWriter(filePath);
        }
    }
}