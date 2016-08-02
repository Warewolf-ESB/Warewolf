using System;
using Dev2.Common.Interfaces;
using Warewolf.Resource.Errors;

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
                throw new ArgumentException(ErrorResource.DropboxCorrectFileName);
        }
    }
}