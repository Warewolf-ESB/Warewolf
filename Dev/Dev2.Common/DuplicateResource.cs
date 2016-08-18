using System;

namespace Dev2.Common
{

    public interface IDuplicateResource
    {
        Guid ResourceId { get; set; }
        string ResourceName { get; set; }
        string FilePath { get; set; }
        string FilePath2 { get; set; }
    }
    public class DuplicateResource:IDuplicateResource
    {
        #region Implementation of IDuplicateResource

        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string FilePath { get; set; }
        public string FilePath2 { get; set; }

        #endregion
    }
}