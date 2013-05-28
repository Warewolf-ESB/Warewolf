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
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        /*
                    [HelpOption]
                    public string GetUsage()
                    {
                        return HelpText.AutoBuild(this, delegate(HelpText current) {
                            if (this.LastPostParsingState.Errors.Count > 0)
                            {
                                var errors = current.RenderParsingErrorsText(this, 2); // indent with two spaces
                                if (!string.IsNullOrEmpty(errors))
                                {
                                    current.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                                    current.AddPreOptionsLine(errors);
                                }
                            }
                        });
                    }
                    */

        /*
        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText { Heading = Program._headingInfo,
                Copyright = new CopyrightInfo("Giacomo Stelluti Scala", 2005, 2012),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            this.HandleParsingErrorsInHelp(help);
            help.AddPreOptionsLine("This is free software. You may redistribute copies of it under the terms of");
            help.AddPreOptionsLine("the MIT License <http://www.opensource.org/licenses/mit-license.php>.");
            help.AddPreOptionsLine("Usage: SampleApp -rMyData.in -wMyData.out --calculate");
            help.AddPreOptionsLine(string.Format("       SampleApp -rMyData.in -i -j{0} file0.def file1.def", 9.7));
            help.AddPreOptionsLine("       SampleApp -rMath.xml -wReport.bin -o *;/;+;-");
            help.AddOptions(this);

            return help;
        }

        private void HandleParsingErrorsInHelp(HelpText help)
        {
            if (this.LastPostParsingState.Errors.Count > 0)
            {
                var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces
                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                    help.AddPreOptionsLine(errors);
                }
            }
        }
        */
    }
}
