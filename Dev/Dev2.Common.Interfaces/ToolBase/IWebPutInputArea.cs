﻿using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebPutInputArea : IToolRegion
    {
        string QueryString { get; set; }
        string RequestUrl { get; set; }
        string PutData { get; set; }

        ObservableCollection<INameValue> Headers { get; set; }
    }
}