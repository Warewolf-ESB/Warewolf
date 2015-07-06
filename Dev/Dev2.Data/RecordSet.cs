using System.Collections.Generic;

namespace Dev2.Data
{
    public interface IRecordSet:IScalar
    {
        Dictionary<int, List<IScalar>> Columns { get; set; }

        void AddColumn(string value, string field, int index);
    }

    public class RecordSet:Scalar, IRecordSet
    {
        public Dictionary<int, List<IScalar>> Columns { get; set; }

        public void AddColumn(string value, string field, int index)
        {
            var scalar = new Scalar { Name = field, Value = value };
            List<IScalar> cols;
            if (Columns.TryGetValue(index, out cols))
            {
                cols.Add(scalar);
            }
            else
            {
               Columns.Add(index,new List<IScalar>{scalar}); 
            }
        }
    }
}