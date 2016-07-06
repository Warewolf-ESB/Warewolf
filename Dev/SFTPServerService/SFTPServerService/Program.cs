using System.ServiceProcess;

namespace SFTPServerService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SftpServerService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
