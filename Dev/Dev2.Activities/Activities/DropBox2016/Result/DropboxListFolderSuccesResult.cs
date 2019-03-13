/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces;
using Dropbox.Api.Files;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Activities.DropBox2016.Result
{
    public interface IListFolderResult
    {
        IEnumerable<IDataNode> Entries { get; }
    }
    
    internal class DropBoxListFolderResultWrapper : IListFolderResult
    {
        private readonly ListFolderResult listFolderResult;

        public DropBoxListFolderResultWrapper(ListFolderResult listFolderResult)
        {
            this.listFolderResult = listFolderResult;
        }

        public IEnumerable<IDataNode> Entries => listFolderResult.Entries.Select(item 
            => new DataNode
            {
                IsFile = item.IsFile,
                IsDeleted = item.IsDeleted,
                IsFolder = item.IsFolder,
                PathLower = item.PathLower,
            });
    }

    public class DropboxListFolderSuccesResult : IDropboxResult
    {
        readonly IListFolderResult _listFolderResult;

        public DropboxListFolderSuccesResult(IListFolderResult listFolderResult)
        {
            _listFolderResult = listFolderResult;
        }

        public DropboxListFolderSuccesResult(ListFolderResult listFolderResult)
        {
            _listFolderResult = new DropBoxListFolderResultWrapper(listFolderResult);
        }

        public virtual IListFolderResult GetListFolderResult() => _listFolderResult;
    }
}