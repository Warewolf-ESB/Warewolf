using System.Collections.Generic;
using GraphQL.Types;

namespace Warewolf.GraphQL
{
    public class ScalarList
    {
        public ScalarList()
        {
            Value = new List<string>();
        }

        public List<string> Value { get; set; }
        public string Name { get; set; }
    }

    public class ScalarListType : ObjectGraphType<ScalarList>
    {
        public ScalarListType()
        {
            Field(list => list.Name);
            Field(list => list.Value, type: typeof(ListGraphType<StringGraphType>));
        }
    }
}