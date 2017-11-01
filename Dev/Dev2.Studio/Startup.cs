using Dev2.Studio;
using System;

namespace Dev2
{
    public class Startup
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var wrapper = new SingleInstanceApplicationWrapper();
            wrapper.Run(args);
        }
    }

    public class SingleInstanceApplicationWrapper : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase
    {
        App _app;

        public SingleInstanceApplicationWrapper()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            _app = new App();
            _app.Run();

            return false;
        }

        protected override void OnStartupNextInstance(Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs eventArgs)
        {
            if (eventArgs.CommandLine.Count > 0)
            {
                _app.OpenBasedOnArguments(new WarwolfStartupEventArgs(eventArgs));
            }
        }
    }
}
