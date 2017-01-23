using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Activities.Annotations;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core.CloneInputRegion
{
    public class DotNetConstructorInputRegionClone : IToolRegion
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public IList<string> Errors { get; private set; }
        public IList<IServiceInput> Inputs { get; set; }
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