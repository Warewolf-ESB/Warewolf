using System;
using Dev2.Common.Interfaces;

namespace Dev2.Activities.DropBox2016
{
    public class DropboxFileNameValidator:IFilenameValidator
    {
        private readonly string _dropboxFile;

        public DropboxFileNameValidator(string dropboxFile)
        {
            _dropboxFile = dropboxFile;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(_dropboxFile))
                throw new ArgumentException("Please specify a correct dropbox file name");
        }
    }
}