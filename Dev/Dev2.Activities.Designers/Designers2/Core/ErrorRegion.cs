using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core
{
    public class ErrorRegion:IToolRegion
    {
        public ErrorRegion()
        {
            ToolRegionName = "ErrorRegion";
            IsVisible = true;
            MaxHeight = 125;
            MinHeight = 125;
            CurrentHeight = 125;
            Dependants = new List<IToolRegion>();
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public double MinHeight { get; set; }
        public double CurrentHeight { get; set; }
        public bool IsVisible { get; set; }
        public double MaxHeight { get; set; }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            return new ErrorRegion();
        }

        public void RestoreRegion(IToolRegion toRestore)
        {

        }

        public IList<string> Errors
        {
            get
            {
                return new List<string>();
            }
        }

        public event HeightChanged HeightChanged;

        #endregion

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnHeightChanged(IToolRegion args)
        {
            var handler = HeightChanged;
            if(handler != null)
            {
                handler(this, args);
            }
        }
    }
}