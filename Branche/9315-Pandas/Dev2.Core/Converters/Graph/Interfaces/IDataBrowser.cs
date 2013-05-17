﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IDataBrowser
    {
        IEnumerable<IPath> Map(object data);
        object SelectScalar(IPath path, object data);
        IEnumerable<object> SelectEnumerable(IPath path, object data);
        Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths, object data);
    }
}
