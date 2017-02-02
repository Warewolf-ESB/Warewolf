using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Activities.Annotations;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core.Web.Put
{
    public class WebPutRegionClone : IToolRegion
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public IList<string> Errors { get; private set; }
        public ObservableCollection<INameValue> Headers { get; set; }
        public string QueryString { get; set; }
        public string RequestUrl { get; set; }
        public IToolRegion CloneRegion()
        {
            return this;
        }
        public void RestoreRegion(IToolRegion toRestore)
        {

        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        [NotifyPropertyChangedInvocator]
        // ReSharper disable once UnusedMember.Local
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}