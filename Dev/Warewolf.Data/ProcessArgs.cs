/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Warewolf.Data
{
    public interface IArgs
    {
        string Filename { get; }
    }
    public class Args : IArgs
    {
        public string Filename { get; set; }
    }
    public static class CommandLine
    {
        public static IArgs ParseArguments(string[] args)
        {
            var result = new Args();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg[0] == '-')
                {
                    i++;
                    result.ParseArgument(arg, args[i]);
                }
            }
            return result;
        }

        static private void ParseArgument(this Args args, string arg, string value)
        {
            arg = arg.Substring(1);

            if (arg == "f")
            {
                args.Filename = value;
            }
        }
    }
}
