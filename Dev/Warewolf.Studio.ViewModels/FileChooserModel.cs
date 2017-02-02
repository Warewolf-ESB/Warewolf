/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;

namespace Warewolf.Studio.ViewModels
{
    public class FileChooserModel : IFileChooserModel
    {
        readonly IQueryManager _queryManager;
        private readonly string _filter;
        private readonly IFile _fileWrapper;

        public FileChooserModel(IQueryManager queryManager)
        {
            VerifyArgument.IsNotNull("queryManager", queryManager);
            _queryManager = queryManager;
        }

        public FileChooserModel(IQueryManager queryManager, string filter, IFile file)
        {
            VerifyArgument.IsNotNull("queryManager", queryManager);
            _queryManager = queryManager;
            VerifyArgument.IsNotNull("filter", filter);
            _filter = filter;
            _fileWrapper = file;
        }

        #region Implementation of IEmailAttachmentModel


        public IList<IFileListing> FetchFiles(IFileListing file)
        {
            var fileListings = _queryManager.FetchFiles(file);
            return FilterFileListings(fileListings);
        }

        private IList<IFileListing> FilterFileListings(IList<IFileListing> fileListings)
        {
            if (!string.IsNullOrEmpty(_filter))
            {
                var listings = fileListings.ToList();
                var toRemove = fileListings.Where(listing =>
                {
                    var fileAttributes = _fileWrapper.GetAttributes(listing.FullName);
                    var b = ((fileAttributes & FileAttributes.Directory) != FileAttributes.Directory) &&
                            !listing.FullName.EndsWith(_filter);
                    return b;
                }).ToList();
                foreach (var toKill in toRemove)
                {
                    listings.Remove(toKill);
                }

                return listings;
            }

            return fileListings;
        }


        public IList<IFileListing> FetchDrives()
        {
            var fileListings = _queryManager.FetchFiles();
            foreach (var fileListing in fileListings)
            {
                if (fileListing.Children != null && fileListing.Children.Count>0)
                {
                    fileListing.Children = FilterFileListings(fileListing.Children.ToList());
                }
            }
            return fileListings;
        }

        #endregion
    }
}
