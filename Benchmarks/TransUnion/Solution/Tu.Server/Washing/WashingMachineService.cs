using System;
using System.Configuration;
using System.ServiceProcess;
using Tu.Imports;
using Tu.Servers;
using Tu.Simulation;

namespace Tu.Washing
{
    //
    // Right click on the project in Visual Studio, choose Properties and select the Output type as:
    //
    // - "Console application" to run as console applicaton
    // - "Windows application" to run as windows service
    //
    public partial class WashingMachineService : ServiceBase
    {
        readonly IWashingMachine _washingMachine;
        readonly ITransunionProcess _transunionProcess;

        #region Main

        public static void Main(string[] args)
        {
            var service = new WashingMachineService(CreateWashingMachine(), new TransunionProcess());

            if(Environment.UserInteractive)
            {
                service.OnStart(args);
                Console.WriteLine("Press any key to stop program");
                Console.Read();
                service.OnStop();
                service.Dispose();
            }
            else
            {
                Run(service);
            }
        }

        #endregion

        public WashingMachineService(IWashingMachine washingMachine, ITransunionProcess transunionProcess)
        {
            if(washingMachine == null)
            {
                throw new ArgumentNullException("washingMachine");
            }
            _washingMachine = washingMachine;
            _transunionProcess = transunionProcess;
            InitializeComponent();
        }

        public void Start()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            _washingMachine.Export();

            if(_transunionProcess != null)
            {
                _transunionProcess.Run();
            }

            _washingMachine.Import(DateTime.Today);
        }

        public static IWashingMachine CreateWashingMachine()
        {
            var sqlServer = new SqlServer(ConfigurationManager.ConnectionStrings["Benchmark"].ConnectionString);
            var emailServer = new EmailServer();
            var ftpServer = new FtpServer(ConfigurationManager.AppSettings["FtpServerUri"]);
            var errorsFileServer = new FileServer(ConfigurationManager.AppSettings["ImportErrorsPath"]);
            var localFileServer = new FileServer();

            var importProcessor = new ImportProcessor(new WashingOutputColumns(), new WashingOutputColumnMapping());

            return new WashingMachine(importProcessor, sqlServer, emailServer, ftpServer, errorsFileServer, localFileServer);
        }
    }
}
