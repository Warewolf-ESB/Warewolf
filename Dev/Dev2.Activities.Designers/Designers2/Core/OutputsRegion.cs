using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Communication;
using Dev2.Studio.Core.Activities.Utils;


namespace Dev2.Activities.Designers2.Core
{
    // ReSharper disable ClassWithVirtualMembersNeverInherited.Global
    public class OutputsRegion : IOutputsToolRegion
    {
        private readonly ModelItem _modelItem;
        private double _minHeight;
        private double _currentHeight;
        private bool _isVisible;
        private double _maxHeight;
        private const double BaseHeight = 60;
        private ICollection<IServiceOutputMapping> _outputs;
        public OutputsRegion(ModelItem modelItem)
        {
            ToolRegionName = "OutputsRegion";
            _modelItem = modelItem;
            if (_modelItem.GetProperty("Outputs") == null||_modelItem.GetProperty<IList<IServiceOutputMapping>>("Outputs").Count ==0)
            {
                var current = _modelItem.GetProperty<ICollection<IServiceOutputMapping>>("Outputs");
                if(current == null)
                {
                    IsVisible = false;
                }
                var outputs = new ObservableCollection<IServiceOutputMapping>(current ?? new List<IServiceOutputMapping>());
                outputs.CollectionChanged += OutputsCollectionChanged;
                Outputs = outputs;
                SetInitialHeight();
            }
            else
            {
                IsVisible = true;
                var outputs = new ObservableCollection<IServiceOutputMapping>(_modelItem.GetProperty<ICollection<IServiceOutputMapping>>("Outputs"));
                outputs.CollectionChanged += OutputsCollectionChanged;
                Outputs = outputs;
                ReCalculateHeight();
            }
        }

        void SetInitialHeight()
        {
            MaxHeight = BaseHeight;
            MinHeight = BaseHeight;
            CurrentHeight = BaseHeight;
        }

        // ReSharper disable once UnusedMember.Global
        //Needed for Deserialization
        public OutputsRegion()
        {
            ToolRegionName = "OutputsRegion";
            SetInitialHeight();
        }

        void OutputsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ReCalculateHeight();
            _modelItem.SetProperty("Outputs", _outputs.ToList());
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsOutputsEmptyRows");
            // ReSharper restore ExplicitCallerInfoArgument
        }
        private bool _outputMappingEnabled;
        private string _recordsetName;
        private IOutputDescription _description;
        private bool _isOutputsEmptyRows;

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
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
                if (value != null)
                {
                    _outputs = value;
                    _modelItem.SetProperty("Outputs", value.ToList());
                    ReCalculateHeight();
                    OnHeightChanged(this);
                    OnPropertyChanged();
                }
            }
        }

        private void ReCalculateHeight()
        {
            // Need to add custom height due to Infragistics XamGrid doing its own thing again
            const double XamGridHeight = 15;
            MaxHeight = (GlobalConstants.RowHeaderHeight + _outputs.Count * GlobalConstants.RowHeight) + XamGridHeight;
            MinHeight = (GlobalConstants.RowHeaderHeight + _outputs.Count * GlobalConstants.RowHeight) + XamGridHeight;
            if(_outputs.Count == 0)
            {
                MaxHeight = BaseHeight;
                MinHeight = BaseHeight;
            }
            if(_outputs.Count >= 3)
            {
                MinHeight = 110;
            }
            IsOutputsEmptyRows = _outputs == null || _outputs.Count == 0;
            CurrentHeight = MinHeight;
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
                return _isOutputsEmptyRows;
            }
            set
            {
                _isOutputsEmptyRows = value;
                OnPropertyChanged();
            }
        }
        public string RecordsetName
        {
            get
            {
                if (string.IsNullOrEmpty(_recordsetName))
                {
                    if(Outputs != null)
                    {
                        var recSet = Outputs.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                        if (recSet != null)
                        {
                            _recordsetName = recSet.RecordSetName;
                        }
                    }
                }
                return _recordsetName;
            }
            set
            {
     
                if (Outputs != null)
                {
                    _recordsetName = value;
                    foreach (var serviceOutputMapping in Outputs)
                    {
                        if (_recordsetName != null)
                        {
                            serviceOutputMapping.RecordSetName = value;
                        }
                    }
                }               
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
                _modelItem.SetProperty("OutputDescription",value);
                OnPropertyChanged();
            }
        }

        public IList<string> Errors
        {
            get
            {
                return Outputs.Where(a => WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate(a.MappedTo).IsComplexExpression || WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate(a.MappedTo).IsWarewolfAtomAtomExpression).Select(a => "Invalid Output Mapping" + a.ToString()).ToList();
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