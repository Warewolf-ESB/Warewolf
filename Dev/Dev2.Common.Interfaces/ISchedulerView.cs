using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface ISchedulerView:IView
    {
        string GetSelectedServerName();

        void GetCurrentSchedule();

        void EnterUsername(string userName);

        void EnterPassword(string password);

        void SelectTask(string taskName);

        void CreateNewTask();

        void Save();

        void DeleteTask(string taskName);

        void UpdateTask(string taskName, string status, string workflowName, string edit);

        string GetUsername();

        string GetPassword();

        void GetScheduledTasks();
    }
}