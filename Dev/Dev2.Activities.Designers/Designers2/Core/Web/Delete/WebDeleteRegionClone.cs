using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;


namespace Dev2.Activities.Designers2.Core.Web.Delete
{
    public class WebDeleteRegionClone
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsEnabled { get; set; }
        public ObservableCollection<INameValue> Headers { get; set; }
        public string QueryString { get; set; }
        public string RequestUrl { get; set; }
    }
}