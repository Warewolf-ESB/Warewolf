using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class CustomToolWithRegionBase : ActivityDesignerViewModel, INotifyPropertyChanged, ICustomToolViewModelWithRegionBase
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private IList<IToolRegion> _regions;
        private double _designHeight;
        private double _designMinHeight;
        private double _designMaxHeight;
        private const double BaseHeight = 120;

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        [ExcludeFromCodeCoverage]
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        // ReSharper disable once PublicConstructorInAbstractClass
        public CustomToolWithRegionBase(ModelItem modelItem)
            : base(modelItem)
        {
            // ReSharper disable once VirtualMemberCallInContructor
            //_regions = BuildRegions();
            //SubscribeToRegions();
            //ReCalculateHeight();
        }

        public abstract IList<IToolRegion> BuildRegions();


        // ReSharper disable once PublicConstructorInAbstractClass
        public CustomToolWithRegionBase(ModelItem modelItem, IList<IToolRegion> regions)
            : base(modelItem)
        {
            _regions = regions;
            SubscribeToRegions();
            ReCalculateHeight();
        }

        private void SubscribeToRegions()
        {
            if(_regions != null)
            {
                foreach(var toolRegion in _regions)
                {
                    toolRegion.HeightChanged += ToolRegionHeightChanged;
                }
            }
        }

        void ToolRegionHeightChanged(object sender, IToolRegion args)
        {
            ReCalculateHeight();
        }

        public void ReCalculateHeight()
        {
            if(_regions != null)
            {
                var isInputVisible = _regions[1].IsVisible;

                DesignMinHeight = _regions.Where(a => a.IsVisible).Sum(a => a.MinHeight);
                DesignMaxHeight = _regions.Where(a => a.IsVisible).Sum(a => a.MaxHeight);
                DesignHeight = _regions.Where(a => a.IsVisible).Sum(a => a.CurrentHeight);

                if (isInputVisible)
                {
                    DesignMaxHeight += BaseHeight;
                    DesignHeight += BaseHeight;
                    DesignMinHeight += BaseHeight;
                }
            }
        }

        // ReSharper disable once PublicConstructorInAbstractClass
        public CustomToolWithRegionBase(ModelItem modelItem, Action<Type> showExampleWorkflow, IList<IToolRegion> regions)
            : base(modelItem, showExampleWorkflow)
        {
            _regions = regions;
            SubscribeToRegions();
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
        public double DesignHeight
        {
            get
            {
                return _designHeight;
            }
            set
            {
                _designHeight = value;
                OnPropertyChanged("DesignHeight");
            }
        }
        public double DesignMinHeight
        {
            get
            {
                return _designMinHeight;
            }
            set
            {
                _designMinHeight = value;
                OnPropertyChanged("DesignMinHeight");
            }
        }
        public double DesignMaxHeight
        {
            get
            {
                return _designMaxHeight;
            }
            set
            {
                _designMaxHeight = value;
                OnPropertyChanged("DesignMaxHeight");
            }
        }

        #endregion
    }
}