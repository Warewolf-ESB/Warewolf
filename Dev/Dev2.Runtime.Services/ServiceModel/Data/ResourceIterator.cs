using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common;

namespace Dev2.Runtime.ServiceModel.Data
{
    internal static class ResourceIterator
    {
        public static void Iterate(IEnumerable<string> folders, Guid workspaceID, Func<ResourceIteratorResult, bool> action, params Delimiter[] delimiters)
        {
            if(delimiters == null || delimiters.Length == 0 || action == null || folders == null)
            {
                return;
            }

            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
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
                        var startTokenLength = delimiter.Start.Length;
                        var startIdx = content.IndexOf(delimiter.Start, StringComparison.InvariantCultureIgnoreCase);
                        if(startIdx == -1)
                        {
                            continue;
                        }
                        startIdx += startTokenLength;
                        var endIdx = content.IndexOf(delimiter.End, startIdx, StringComparison.InvariantCultureIgnoreCase);
                        var length = endIdx - startIdx;
                        if(length > 0)
                        {
                            delimiterFound = true;
                            var tokenValue = content.Substring(startIdx, length);
                            iteratorResult.Values.Add(delimiter.ID, tokenValue);
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
    }
}