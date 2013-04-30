using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dev2.Workspaces;

namespace Dev2.Studio.Core.Workspaces
{
    public static class WorkspaceItemRepository
    {
        #region RepositoryPath

        static string _repositoryPath;
        static string RepositoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_repositoryPath))
                {
                    _repositoryPath = Path.Combine(new[]
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        StringResources.App_Data_Directory,
                        StringResources.User_Interface_Layouts_Directory,
                        "WorkspaceItems.xml"
                    });
                }
                return _repositoryPath;
            }
        }

        #endregion

        #region Read

        public static IList<IWorkspaceItem> Read()
        {
            var result = new List<IWorkspaceItem>();
            if (File.Exists(RepositoryPath))
            {
                try
                {
                    var xml = XElement.Parse(File.ReadAllText(RepositoryPath));
                    result.AddRange(xml.Elements().Select(x => new WorkspaceItem(x)));
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // corrupt so ignore
                }
            }
            return result;
        }

        #endregion

        #region Write

        public static void Write(IList<IWorkspaceItem> items)
        {
            var root = new XElement("WorkspaceItems");
            foreach (var workspaceItem in items)
            {
                var itemXml = workspaceItem.ToXml();
                root.Add(itemXml);
            }

            if (!File.Exists(RepositoryPath))
            {
                FileInfo fileInfo = new FileInfo(RepositoryPath);
                string finalDirectoryPath = fileInfo.Directory.FullName;
                
                if (!Directory.Exists(finalDirectoryPath))
                {
                    Directory.CreateDirectory(finalDirectoryPath);
                }
            }
            File.WriteAllText(RepositoryPath, root.ToString());
        }

        #endregion

    }
}
