/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using CommandLine;

namespace WarewolfStorageActions
{
    internal class Args
    {
        [Option('e', "encrypt", Required = false, HelpText = "Should we encrypt? default is decrypt")]
        public bool ShouldEncrypt { get; set; } = false;

        [Option('f', "file", Required = true, HelpText = "The source file")]
        public string FileName { get; set; }

        [Option('o', "output-file", Required = true, HelpText = "The destination file")]
        public string OutputFileName { get; set; }
    }
}
