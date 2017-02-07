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
                try
                {
                    var numberOfLinesString = ctx.Request.QueryString.Get("numLines");
                    int numLines;
                    if (int.TryParse(numberOfLinesString, out numLines))
                    {
                        if (numLines > 0)
                        {
                            numberOfLines = numLines;
                        }
                    }
                }
                catch (Exception)
                {
                    //Bad Query return default number of lines
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
                        string line = file.ReadLine();

                        if (buffor.Count >= numberOfLines)
                            buffor.Dequeue();
                        buffor.Enqueue(line);
                    }
                    string[] lastLines = buffor.ToArray();
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