using System.Collections.Generic;
using System.Data;

namespace Tu.Imports
{
    public interface IImportProcessor
    {
        DataTable OutputData { get; }
        DataTable Errors { get; }
        IList<DataColumn> Columns { get; }

        DataRow[] Run(string csvInput);
    }
}