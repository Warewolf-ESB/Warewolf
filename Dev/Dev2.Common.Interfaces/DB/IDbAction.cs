﻿using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DB
{
    public interface IDbAction
    {
        IList<IServiceInput> Inputs { get; set; }
        string Name { get; set; }
        Guid SourceId { get; set; }
        string ExecuteAction { get; set; }

        string GetIdentifier();
    }
}
