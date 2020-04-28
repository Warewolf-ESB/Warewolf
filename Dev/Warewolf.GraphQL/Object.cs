using GraphQL.Types;

namespace Warewolf.GraphQL
{
  public class Object
  {
    public Object()
    {
    }

    public string Name { get; set; }
    public string Value { get; set; }
  }


  public class ObjectType : ObjectGraphType<Object>
  {
    public ObjectType()
    {
      Field(s => s.Name);
      Field(s => s.Value);
    }
  }
}