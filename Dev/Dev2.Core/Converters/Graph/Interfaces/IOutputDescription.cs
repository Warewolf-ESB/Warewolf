using System.Collections.Generic;

namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IOutputDescription
    {
        OutputFormats Format { get; set; }
        List<IDataSourceShape> DataSourceShapes { get; set; }
    }
}
