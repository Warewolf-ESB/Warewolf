using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IOutputDescription
    {
        OutputFormats Format { get; set; }
        List<IDataSourceShape> DataSourceShapes { get; set; }
    }
}
