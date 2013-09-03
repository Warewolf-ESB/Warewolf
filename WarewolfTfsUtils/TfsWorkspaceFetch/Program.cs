using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarewolfTfsUtils;

namespace TfsWorkspaceFetch
{
    /// <summary>
    /// Here to work around needing tons of dlls for the server ;)
    /// </summary>
    class Program
    {
        static string Main(string[] args)
        {

            if (args.Length == 4)
            {

                var server = args[0];
                var project = args[1];
                var workspaceName = args[2];
                var workingDirectory = args[3];

                WarewolfWorkspace utils = new WarewolfWorkspace();

                return utils.FetchWorkspace(server, project, workspaceName, workingDirectory);    
            }

            return "Incorrect Number of Arguments";

        }
    }
}
