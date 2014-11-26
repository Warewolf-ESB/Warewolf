/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using CommandLine;
using CommandLine.Text;

namespace Dev2
{
    internal class CommandLineParameters : CommandLineOptionsBase
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