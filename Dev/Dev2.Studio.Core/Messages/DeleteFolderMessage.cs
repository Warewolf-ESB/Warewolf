using System;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class DeleteFolderMessage : IMessage
    {
        public DeleteFolderMessage(string folderName, Action actionToDoOnDelete = null)
        {
            FolderName = folderName;
            ActionToDoOnDelete = actionToDoOnDelete;
        }

        public string FolderName { get; set; }
        public Action ActionToDoOnDelete { get; set; }
    }
}