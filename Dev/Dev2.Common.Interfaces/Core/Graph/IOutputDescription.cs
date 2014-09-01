using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Core.Graph
{
    public interface IOutputDescription
    {
        OutputFormats Format { get; set; }
        List<IDataSourceShape> DataSourceShapes { get; set; }
    }
}
