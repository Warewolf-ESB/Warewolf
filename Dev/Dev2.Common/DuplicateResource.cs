using System;
using System.Collections.Generic;

namespace Dev2.Common
{

    public interface IDuplicateResource
    {
        Guid ResourceId { get; set; }
        string ResourceName { get; set; }
        List<string> ResourcePath { get; set; }
    }
    public class DuplicateResource:IDuplicateResource
    {
        #region Implementation of IDuplicateResource

        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; }
        public List<string> ResourcePath { get; set; }

        #endregion
    }
}