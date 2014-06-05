
using Microsoft.Win32.TaskScheduler;

namespace Gui.Utility
{
    public class ScheduledTaskDeleteOp
    {
        private const string Folder = "Warewolf";

        public bool RemoveWarewolfScheduledTask()
        {
            if(!DoesWarewolfFolderExist())
            {
                return true;
            }

            using(TaskService ts = new TaskService())
            {
                var theFolder = ts.GetFolder(Folder);
                var taskList = theFolder.GetTasks();
                foreach(var task in taskList)
                {
                    task.Stop();
                    theFolder.DeleteTask(task.Name);
                }

                // now remove the root folder ;)
                theFolder.Dispose();
                ts.RootFolder.DeleteFolder("Warewolf");
            }

            return true;
        }

        public bool DoesWarewolfFolderExist()
        {
            using(TaskService ts = new TaskService())
            {
                try
                {
                    ts.GetFolder(Folder).Dispose();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
