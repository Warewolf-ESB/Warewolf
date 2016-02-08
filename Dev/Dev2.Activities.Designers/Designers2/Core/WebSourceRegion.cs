using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Activities.Designers2.Core
{
    public class WebSourceRegion:ISourceToolRegion<IWebServiceSource>
    {
        private double _minHeight;
        private double _currentHeight;
        private bool _isVisible;
        private double _maxHeight;
        private IWebServiceSource _selectedSource;
        private ICollection<IWebServiceSource> _sources;
        private readonly ModelItem _modelItem;

        public WebSourceRegion(IWebServiceModel model, ModelItem modelItem)
        {
            MinHeight = 20;
            MaxHeight = 20;
            CurrentHeight = 20;
            IsVisible = true;
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => model.EditSource(SelectedSource), CanEditSource);
            var sources = model.RetrieveSources().OrderBy(source => source.Name);
            Sources = sources.ToObservableCollection();
            IsVisible = true;
            _modelItem = modelItem;
            if (SourceId != Guid.Empty)
            {
                SelectedSource = Sources.FirstOrDefault(source => source.Id == SourceId);
            }
        }


        Guid SourceId
        {
            get
            {
                return _modelItem.GetProperty<Guid>("SourceId");
            }
            set
            {
               _modelItem.SetProperty("SourceId",value);
            }
        }




        public bool CanEditSource()
        {
            return SelectedSource != null;
        }

        public ICommand EditSourceCommand { get; set; }

        public ICommand NewSourceCommand { get; set; }
        public event SomethingChanged SomethingChanged;

        #region Implementation of IToolRegion

        public double MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                _minHeight = value;
                OnPropertyChanged();
            }
        }
        public double CurrentHeight
        {
            get
            {
                return _currentHeight;
            }
            set
            {
                _currentHeight = value;
                OnPropertyChanged();
            }
        }
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        public double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                _maxHeight = value;
                OnPropertyChanged();
            }
        }
        public event HeightChanged HeightChanged;
        public IList<IToolRegion> Dependants { get; set; }

        #endregion

        #region Implementation of ISourceToolRegion<IWebServiceSource>

        public IWebServiceSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                _selectedSource = value;
                SavedSource = value;
                SourceId = value.Id;
                OnSomethingChanged(this);
                OnHeightChanged(this);
                OnPropertyChanged();
            }
        }
        public ICollection<IWebServiceSource> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
                OnPropertyChanged();
            }
        }

        public IList<string> Errors
        {
            get
            {
                return SelectedSource == null ? new List<string> { "Invalid Source Selected" } : new List<string>();
            }
        }

        public IWebServiceSource SavedSource {
            get
            {
               return _modelItem.GetProperty<IWebServiceSource>("SavedSource");
            }
            set {
                _modelItem.SetProperty("SavedSource", value);

            } }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            if(handler != null)
            {
                handler(this, args);
            }
        }
    }

    public class ErrorRegion:IToolRegion
    {
        private double _minHeight;
        private double _currentHeight;
        private bool _isVisible;
        private double _maxHeight;

        public ErrorRegion()
        {
            IsVisible = true;
            MaxHeight = 250;
            MinHeight = 250;
            CurrentHeight = 250;
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IToolRegion

        public double MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                _minHeight = value;
            }
        }
        public double CurrentHeight
        {
            get
            {
                return _currentHeight;
            }
            set
            {
                _currentHeight = value;
            }
        }
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
            }
        }
        public double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                _maxHeight = value;
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

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
