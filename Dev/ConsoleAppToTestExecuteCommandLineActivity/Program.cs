/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace ConsoleAppToTestExecuteCommandLineActivity
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }
            string switchArg = args[0];
            switch (switchArg)
            {
                case "user":
                    Console.Read();
                    return;
                case "output":
                    Console.WriteLine("This is output from the user");
                    return;
                case "differentoutput":
                    Console.WriteLine("This is a different output from the user");
                    return;
                case "error":
                    Console.WriteLine("This is error");
                    Console.Error.Write("The console errored.");
                    return;
            }
        }
    }
}