using Dev2.Common;

namespace Dev2.Data.Storage
{
    /// <summary>
    /// Responsible for handling the storage and retrival of alias keys ;)
    /// PBI : 10440
    /// </summary>
    public class BinaryDataListKeyAliasManager
    {

        // TODO : Create an array of alias files bound the the max number of executors

        private string _fileName;
       
        public BinaryDataListKeyAliasManager(string fileName)
        {
            _fileName = fileName;
        }

    }
}
