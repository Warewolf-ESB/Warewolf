﻿using System;
using Dev2.Common.Interfaces;

namespace Dev2
{
   

    [Serializable]
    public class DeletedFileMetadata : IDeletedFileMetadata
    {
        public bool IsDeleted { get; set; }
        public Guid ResourceId { get; set; }
    }
}
