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

namespace QueueWorker
{
    public interface IArgs
    {
        string TriggerId { get; }
        bool ShowConsole { get; }
    }
    public class Args : IArgs
    {
        [Option('c', "config", Required = true, HelpText = "The id of the queue trigger")]
        public string TriggerId { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Show the console window")]
        public bool ShowConsole { get; set; } = false;
    }
}