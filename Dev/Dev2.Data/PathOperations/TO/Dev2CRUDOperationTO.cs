
namespace Dev2.PathOperations
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide overwrite value to CRUD path operations
    /// </summary>
    public class Dev2CRUDOperationTO : IPathOverwrite
    {

        public Dev2CRUDOperationTO(bool overwrite,bool doRecursiveCopy = true)
        {
            Overwrite = overwrite;
            DoRecursiveCopy = doRecursiveCopy;
        }

        public bool DoRecursiveCopy { get; set; }

        public bool Overwrite
        {
            get;
            set;
        }
    }
}
