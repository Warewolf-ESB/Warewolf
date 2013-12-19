using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Unlimited.Framework.Converters.Graph
{
    public class DataSourceShapeFactory
    {
        public static IDataSourceShape CreateDataSourceShape()
        {
            return new DataSourceShape();
        }
    }
}
