using System;
using System.Linq;
using Warewolf.Driver.Serilog;

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
            private readonly ILoggerContext _config;
            public Implementation(ILoggerContext config)
            {
                _config = config;
            }
            public void Run()
            {
                var logger = _config.Source;
                var connection = logger.NewConnection(_config.LoggerConfig);
                connection.StartConsuming(_config.LoggerConfig, new SeriLogConsumer(""));
            }
        }
    }
}
