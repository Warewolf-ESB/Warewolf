using System.Collections.Generic;
using Dev2;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class EmailAttachmentModel:IEmailAttachmentModel
    {
        readonly IQueryManager _queryManager;

        public EmailAttachmentModel(IQueryManager queryManager)
        {
            VerifyArgument.IsNotNull("queryManager",queryManager);
            _queryManager = queryManager;
        }

        #region Implementation of IEmailAttachmentModel


        public IList<IFileListing> FetchFiles(IFileListing file)
        {
            return _queryManager.FetchFiles(file);
        }

        public IList<IFileListing> FetchDrives()
        {
            return _queryManager.FetchFiles();
        }

        #endregion
    }
}
