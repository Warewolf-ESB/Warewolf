using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Interfaces
{
    public class Dev2ExecAction : Dev2Action, IExecAction
    {
        public Dev2ExecAction(ITaskServiceConvertorFactory taskServiceConvertorFactory, ExecAction nativeTnstance)
            : base(nativeTnstance)
        {
        }

        public new ExecAction Instance
        {
            get { return (ExecAction) base.Instance; }
        }

        public string Path
        {
            get { return Instance.Path; }
            set { Instance.Path = value; }
        }

        public string Arguments
        {
            get { return Instance.Arguments; }
            set { Instance.Arguments = value; }
        }

        public string WorkingDirectory
        {
            get { return Instance.WorkingDirectory; }
            set { Instance.WorkingDirectory = value; }
        }
    }
}