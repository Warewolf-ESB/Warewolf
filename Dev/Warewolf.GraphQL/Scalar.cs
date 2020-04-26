using GraphQL.Types;

namespace Warewolf.GraphQL
{
    public class Scalar
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class ScalarType : ObjectGraphType<Scalar>
    {
        public ScalarType()
        {
            Field(s => s.Name);
            Field(s => s.Value);
        }
    }
}