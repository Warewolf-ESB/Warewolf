using System.Collections.Generic;
using GraphQL.Types;

namespace Warewolf.GraphQL
{
    public class RecordSet
    {
        public RecordSet()
        {
            Columns = new List<ScalarList>();
        }

        public string Name { get; set; }
        public List<ScalarList> Columns { get; set; }
    }

    public class RecordsetType : ObjectGraphType<RecordSet>
    {
        public RecordsetType()
        {
            Field(s => s.Name);
            Field(s => s.Columns, type: typeof(ListGraphType<ScalarListType>));
        }
    }
}