﻿using Dev2.Common.Interfaces;
using Dropbox.Api.Files;

namespace Dev2.Activities.DropBox2016.Result
{
    public class DropboxDeleteSuccessResult : IDropboxResult
    {
        readonly FileMetadata _metaData;

        public DropboxDeleteSuccessResult(FileMetadata metadata)
        {
            _metaData = metadata;
        }

        public FileMetadata GerFileMetadata() => _metaData;
    }
}
