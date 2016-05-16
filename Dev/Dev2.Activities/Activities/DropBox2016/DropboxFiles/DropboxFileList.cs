using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.DropboxFiles
{
    //To Use this class to get all dropbox files and folders
    [ExcludeFromCodeCoverage]
    public class DropboxFileList
    {
        private readonly DropboxClient _client;
        private readonly string _pathInDropbox;

        public List<string> Files { get;  protected set; } 
        public List<string> Folders { get;  protected set; } 

        public DropboxFileList(DropboxClient client, string pathInDropbox = "")
        {
            _client = client;
            _pathInDropbox = pathInDropbox;
            FetchDropboxFiles();
        }

        private void FetchDropboxFiles()
        {
            var listFolderResult = _client.Files.ListFolderAsync(new ListFolderArg(_pathInDropbox)).Result;
            Folders = listFolderResult.Entries.Where(metadata => metadata.IsFolder).Select(metadata => metadata.Name).ToList();
            Files = listFolderResult.Entries.Where(metadata => metadata.IsFile).Select(metadata => metadata.Name).ToList();
        }
    }
}
