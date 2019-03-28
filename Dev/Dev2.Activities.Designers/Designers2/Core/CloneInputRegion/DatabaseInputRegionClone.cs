#pragma warning disable
﻿using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Activities.Designers2.Core.CloneInputRegion
{
    public class DatabaseInputRegionClone
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsEnabled { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
    }
}
