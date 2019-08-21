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
    public class ProcessArgs
    {
        public string QueueName { get; private set; }
        public string WorkflowUrl { get; private set; }
        public string ValueKey { get; private set; }
        public string HostName { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }

        public ProcessArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg[0] == '-')
                {
                    i++;
                    ProcessArg(arg, args[i]);
                }
            };
        }

        private void ProcessArg(string arg, string value)
        {
            arg = arg.Substring(1);

            if (arg == "q")
            {
                QueueName = value;
            }

            if (arg == "w")
            {
                WorkflowUrl = value;
            }

            if (arg == "v")
            {
                ValueKey = value;
            }

            if (arg == "h")
            {
                HostName = value;
            }

            if (arg == "u")
            {
                UserName = value;
            }

            if (arg == "p")
            {
                Password = value;
            }
        }

    }
}
