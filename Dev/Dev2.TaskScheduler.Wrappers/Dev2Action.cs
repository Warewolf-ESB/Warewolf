using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2Action : IAction
    {
        private readonly Action _nativeTnstance;

        public Dev2Action(Action nativeTnstance)
        {
            _nativeTnstance = nativeTnstance;
        }

        public TaskActionType ActionType
        {
            get { return _nativeTnstance.ActionType; }
        }

        public string Id
        {
            get { return Instance.Id; }
            set { Instance.Id = value; }
        }

        public void Dispose()
        {
            Instance.Dispose();
        }


        public Action Instance
        {
            get { return _nativeTnstance; }
        }
    }
}