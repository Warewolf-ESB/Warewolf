using Dev2.Common.Interfaces.Core.Graph;
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
