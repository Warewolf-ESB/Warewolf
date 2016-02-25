using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Dev2.Activities.Annotations;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core
{
    public class WebPostInputRegionClone : IToolRegion
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolRegionName { get; set; }
        public double MinHeight { get; set; }
        public double CurrentHeight { get; set; }
        public bool IsVisible { get; set; }
        public double MaxHeight { get; set; }
        public event HeightChanged HeightChanged;
        public IList<IToolRegion> Dependants { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public IList<string> Errors { get; private set; }
        public ObservableCollection<INameValue> Headers { get; set; }
        public string BodyString { get; set; }
        public string QueryString { get; set; }
        public string RequestUrl { get; set; }
        [ExcludeFromCodeCoverage]
        public IToolRegion CloneRegion()
        {
            return this;
        }
        [ExcludeFromCodeCoverage]
        public void RestoreRegion(IToolRegion toRestore)
        {

        }
        [ExcludeFromCodeCoverage]
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        [ExcludeFromCodeCoverage]
        private void OnHeightChanged(IToolRegion args)
        {
            var handler = HeightChanged;
            if (handler != null) handler(this, args);
        }
    }
}
