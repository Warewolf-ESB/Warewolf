using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;

namespace Warewolf.Studio.ViewModels
{
    public class EmailAttachmentModel : IEmailAttachmentModel
    {
        readonly IQueryManager _queryManager;
        private readonly string _filter;
        private readonly IFile _fileWrapper;

        public EmailAttachmentModel(IQueryManager queryManager)
        {
            VerifyArgument.IsNotNull("queryManager", queryManager);
            _queryManager = queryManager;
        }
        
        public EmailAttachmentModel(IQueryManager queryManager, string filter, IFile file)
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
            if (!string.IsNullOrEmpty(_filter))
            {
                var listings = fileListings.ToList();
                var toRemove = fileListings.Where(listing =>
                {
                    var fileAttributes = _fileWrapper.GetAttributes(listing.FullName);
                    var b = ((fileAttributes & FileAttributes.Directory) != FileAttributes.Directory) && !listing.FullName.EndsWith(_filter);
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
            return _queryManager.FetchFiles();
        }

        #endregion
    }
}
