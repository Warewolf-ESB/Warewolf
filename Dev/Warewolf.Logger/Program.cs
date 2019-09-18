using Fleck;
using System;
using System.Linq;
using Warewolf.Driver.Serilog;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public class Program
    {


        static void Main(string[] args)
        {           
            var config = new LoggerContext(args);
            if(config.Errors.Count() > 0)
            {
                Environment.Exit(1);
            };
            new Implementation(config).Run();
        }

        private class Implementation
        {
            private WebSocketServer _server;
            private readonly ILoggerContext _config;
            public Implementation(ILoggerContext config)
            {
                _config = config;
            }
            public void Run()
            {
                var loggerConfig = _config as ILoggerConfig;
                _server = new WebSocketServer(loggerConfig.ServerLoggingAddress)
                {
                    RestartAfterListenError = true
                };

                var logger = _config.Source;
                var connection = logger.NewConnection(_config.LoggerConfig);
                var publisher = connection.NewPublisher();

                _server.Start(socket =>
                {
                    //socket.OnOpen = () => Console.WriteLine("Open!");
                    //socket.OnClose = () => Console.WriteLine("Close!");
                    socket.OnMessage = message =>  publisher.Info(message);
                });
                connection.StartConsuming(_config.LoggerConfig, new SeriLogConsumer(_config.LoggerConfig as ISeriLogConfig));
            }
        }
    }
}
