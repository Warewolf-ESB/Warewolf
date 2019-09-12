/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using CommandLine;
using System;
using System.Collections.Generic;
using Warewolf.Common;

namespace Warewolf.Logger
{
    public class LoggerContext : ILoggerContext
    {
        public IEnumerable<Error> Errors { get; private set; }
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
        }

        public bool Verbose { get; set; }
        public LoggerContext(string[] args)
        {
            Errors = new List<Error>();
            var processArgs = CommandLine.Parser.Default.ParseArguments<Options>(args)
                 .WithParsed<Options>(opts => IsVerbose(opts))
                 .WithNotParsed<Options>((errs) => HandleParseError(errs));

        }

        public void IsVerbose(Options opts)
        {
            if (opts.Verbose)
            {
                Verbose = true;
                Console.WriteLine("Warewolf.Logger is in Verbose mode!");
                _ = new ConsoleWindow();
            }
            else
            {
                Verbose = false;
            }
        }

        public void HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine("Command Line parameters provided were not valid!");
            Errors = errs;
        }
    }
}
