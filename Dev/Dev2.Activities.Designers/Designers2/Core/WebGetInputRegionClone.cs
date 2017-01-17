using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Activities.Annotations;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;
// ReSharper disable UnusedMember.Local

namespace Dev2.Activities.Designers2.Core
{
    public sealed class WebGetInputRegionClone : IToolRegion
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
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}