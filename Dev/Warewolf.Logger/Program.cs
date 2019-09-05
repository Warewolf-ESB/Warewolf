using System;
using System.Linq;

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
                Console.WriteLine($"Starting listening: Warewolf.Logger");
            }
        }
    }
}
