namespace Dev2.Common
{

    public interface IDuplicateResource
    {
        string ResourceName { get; set; }
        string FilePath { get; set; }
        string FilePath2 { get; set; }
    }
    public class DuplicateResource:IDuplicateResource
    {
        #region Implementation of IDuplicateResource

        public string ResourceName { get; set; }
        public string FilePath { get; set; }
        public string FilePath2 { get; set; }

        #endregion
    }
}