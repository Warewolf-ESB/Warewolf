using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common;
using Dev2.Runtime.ServiceModel;

namespace Dev2.Runtime.Hosting
{
    internal class ResourceIterator : IResourceIterator
    {
        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile ResourceIterator _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ResourceIterator Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new ResourceIterator();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Initialization

        // Prevent instantiation
        ResourceIterator()
        {
        }

        #endregion

        #region IterateAll

        public void IterateAll(Guid workspaceID, Func<ResourceIteratorResult, bool> action, params ResourceDelimiter[] delimiters)
        {
            Iterate(Resources.RootFolders.Values.Distinct(), workspaceID, action, delimiters);
        }

        #endregion

        #region Iterate

        public void Iterate(IEnumerable<string> folders, Guid workspaceID, Func<ResourceIteratorResult, bool> action, params ResourceDelimiter[] delimiters)
        {
            if(delimiters == null || delimiters.Length == 0 || action == null || folders == null)
            {
                return;
            }

            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            foreach(var path in folders.Select(folder => Path.Combine(workspacePath, folder)))
            {
                var files = Directory.GetFiles(path, "*.xml");
                foreach(var file in files)
                {
                    // XML parsing will add overhead - so just read file and use string ops instead
                    var content = File.ReadAllText(file);
                    var iteratorResult = new ResourceIteratorResult { Content = content };
                    var delimiterFound = false;
                    foreach(var delimiter in delimiters)
                    {
                        string value;
                        if(delimiter.TryGetValue(content, out value))
                        {
                            delimiterFound = true;
                            iteratorResult.Values.Add(delimiter.ID, value);
                        }
                    }
                    if(delimiterFound)
                    {
                        if(!action(iteratorResult))
                        {
                            return;
                        }
                    }
                }
            }
        }

        #endregion

    }
}