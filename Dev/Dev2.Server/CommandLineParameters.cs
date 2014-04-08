using CommandLine;
using CommandLine.Text;

namespace Dev2
{
    class CommandLineParameters : CommandLineOptionsBase
    { 
        [Option("s", "startservice", HelpText = "Starts the windows service.", DefaultValue = false)]
        public bool StartService { get; set; }

        [Option("x", "stopservice", HelpText = "Stops the windows service.", DefaultValue = false)]
        public bool StopService { get; set; }

        [Option("i", "install", HelpText = "Installs the app as a windows service.", DefaultValue = false)]
        public bool Install { get; set; }

        [Option("u", "uninstall", HelpText = "Uninstalls the windows service for this app.", DefaultValue = false)]
        public bool Uninstall { get; set; }

        [Option("t", "standalone", HelpText = "Standalone Mode", DefaultValue = false)]
        public bool IntegrationTestMode { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}
