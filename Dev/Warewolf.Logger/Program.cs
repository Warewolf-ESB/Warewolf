using System;

namespace Warewolf.Logger
{
    public class Program
    {
        static void Main(string[] args)
        {           
            var config = new LoggerContext(args);
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
