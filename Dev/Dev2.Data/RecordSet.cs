using System.Collections.Generic;

namespace Dev2.Data
{
    public interface IRecordSet:IScalar
    {
        Dictionary<int, List<IScalar>> Columns { get; set; }
    }

    public class RecordSet:Scalar, IRecordSet
    {
        public Dictionary<int, List<IScalar>> Columns { get; set; }
    }
}