using System.Collections.ObjectModel;
using System.ComponentModel;
using Dev2.Common.Interfaces;


namespace Dev2.Activities.Designers2.Core
{
    public sealed class WebGetInputRegionClone
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsEnabled { get; set; }
        public ObservableCollection<INameValue> Headers { get; set; }
        public string QueryString { get; set; }
        public string RequestUrl { get; set; }
    }
}