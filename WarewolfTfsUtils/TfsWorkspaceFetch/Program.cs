using System;
using WarewolfTfsUtils;

namespace TfsWorkspaceFetch
{
    /// <summary>
    /// Here to work around needing tons of dlls for the server ;)
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 4 || args.Length == 6)
            {

                var server = args[0];
                var project = args[1];
                var workspaceName = args[2];
                var workingDirectory = args[3];
                var user = string.Empty;
                var pass = string.Empty;
                // use user and pass sent in ;)
                if (args.Length == 6)
                {
                    user = args[4];
                    pass = args[5];
                }

                WarewolfWorkspace utils = new WarewolfWorkspace();

                Console.WriteLine(utils.FetchWorkspace(server, project, workspaceName, workingDirectory, user, pass));

            }
            else
            {

                Console.WriteLine("Too many args [ " + args.Length + " ], 4 required.");
            }


        }
    }
}
