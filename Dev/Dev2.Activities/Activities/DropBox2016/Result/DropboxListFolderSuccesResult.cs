using Dev2.Common.Interfaces;
using Dropbox.Api.Files;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Dev2.Activities.DropBox2016.Result
{
    public class DropboxListFolderSuccesResult : IDropboxResult
    {
        private readonly ListFolderResult _listFolderResult;

        public DropboxListFolderSuccesResult(ListFolderResult listFolderResult)
        {
            _listFolderResult = listFolderResult;
        }

        public virtual ListFolderResult GetListFolderResulResult()
        {
            return _listFolderResult;
        }

    }
}