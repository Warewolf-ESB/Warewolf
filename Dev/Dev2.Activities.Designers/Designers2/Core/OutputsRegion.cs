using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Communication;
using Dev2.Studio.Core.Activities.Utils;


namespace Dev2.Activities.Designers2.Core
{
    public class OutputsRegion : IOutputsToolRegion
    {
        private readonly ModelItem _modelItem;
        private double _minHeight;
        private double _currentHeight;
        private bool _isVisible;
        private double _maxHeight;
        private ICollection<IServiceOutputMapping> _outputs;
        public OutputsRegion(ModelItem modelItem)
        {
            _modelItem = modelItem;
            if (Outputs == null)
            {
                var current = _modelItem.GetProperty<ICollection<IServiceOutputMapping>>("Outputs");
                var outputs = new ObservableCollection<IServiceOutputMapping>(current ?? new List<IServiceOutputMapping>());
                outputs.CollectionChanged += outputs_CollectionChanged;
                Outputs = outputs;
            }
            SetInitialHeight();
        }

        void SetInitialHeight()
        {
            MaxHeight = 60;
            MinHeight = 60;
            CurrentHeight = 60;
        }

        public OutputsRegion()
        {
            SetInitialHeight();
        }

        void outputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MaxHeight = e.NewItems.Count * 15;
            MinHeight = 45;
            _modelItem.SetProperty("Outputs", _outputs.ToList());
        }
        private bool _outputMappingEnabled;
        private string _recordsetName;
        private IOutputDescription _description;

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
                OnHeightChanged(this);
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

        public IToolRegion CloneRegion()
        {
            var ser = new Dev2JsonSerializer();
            return ser.Deserialize<IToolRegion>(ser.SerializeToBuilder(this));
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as OutputsRegion;
            if (region != null)
            {
                MaxHeight = region.MaxHeight;
                MinHeight = region.MinHeight;
                IsVisible = region.IsVisible;
                CurrentHeight = region.CurrentHeight;
                Outputs = region.Outputs;
                RecordsetName = region.RecordsetName;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("IsOutputsEmptyRows");
            }
        }

        #endregion

        #region Implementation of IOutputsToolRegion

        public ICollection<IServiceOutputMapping> Outputs
        {
            get
            {
                return _outputs;
            }
            set
            {
                _outputs = value;
                _modelItem.SetProperty("Outputs", value.ToList());
                MaxHeight = 250 + _outputs.Count * 45;
                MinHeight = 250;
                CurrentHeight = 250;
                OnHeightChanged(this);
                OnPropertyChanged();
            }
        }
        public bool OutputMappingEnabled
        {
            get
            {
                return _outputMappingEnabled;
            }
            set
            {
                _outputMappingEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool IsOutputsEmptyRows
        {
            get
            {
                return _outputs == null || _outputs.Count > 0;
            }
        }
        public string RecordsetName
        {
            get
            {
                return _recordsetName;
            }
            set
            {
                _recordsetName = value;
                OnPropertyChanged();
            }
        }
        public IOutputDescription Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public IList<string> Errors
        {
            get
            {
                return Outputs.Where(a => WarewolfDataEvaluationCommon.ParseLanguageExpressionWithoutUpdate(a.MappedTo).IsComplexExpression || WarewolfDataEvaluationCommon.ParseLanguageExpressionWithoutUpdate(a.MappedTo).IsWarewolfAtomAtomExpression).Select(a => "Invalid Output Mapping" + a.ToString()).ToList();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnHeightChanged(IToolRegion args)
        {
            var handler = HeightChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }
    }
}