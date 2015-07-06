
using Dev2.Data.Binary_Objects;

namespace Dev2.Data
{
    public interface IScalar
    {
        string Name { get; set; }
        enDev2ColumnArgumentDirection IODirection { get; set; }
        string Description { get; set; }
        bool IsEditable { get; set; }
        string Value { get; set; }
    }

    public class Scalar : IScalar
    {
        public string Name { get; set; }
        public enDev2ColumnArgumentDirection IODirection { get; set; }
        public string Description { get; set; }
        public bool IsEditable { get; set; }
        public string Value { get; set; }
    }
}