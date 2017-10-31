using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces.ToolBase;




namespace Dev2.Activities.Designers2.Core
{
    public abstract class CustomToolWithRegionBase : ActivityDesignerViewModel, INotifyPropertyChanged, ICustomToolViewModelWithRegionBase
    {
        
        protected IList<IToolRegion> _regions;

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        protected CustomToolWithRegionBase(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public abstract IList<IToolRegion> BuildRegions();


        protected CustomToolWithRegionBase(ModelItem modelItem, IList<IToolRegion> regions)
            : base(modelItem)
        {
            _regions = regions;
        }


        #region Implementation of ICustomToolViewModelWithRegionBase

        public IList<IToolRegion> Regions
        {
            get
            {
                return _regions;
            }

            set
            {
                _regions = value;
            }
        }
    }

    #endregion
}

