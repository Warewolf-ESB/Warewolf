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
    // ReSharper disable ClassWithVirtualMembersNeverInherited.Global
    public class OutputsRegion : IOutputsToolRegion
    {
        private readonly ModelItem _modelItem;
        private bool _isVisible;
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
            }
            else
            {
                IsVisible = true;
                var outputs = new ObservableCollection<IServiceOutputMapping>(_modelItem.GetProperty<ICollection<IServiceOutputMapping>>("Outputs"));
                outputs.CollectionChanged += OutputsCollectionChanged;
                Outputs = outputs;
            }
        }

        // ReSharper disable once UnusedMember.Global
        //Needed for Deserialization
        public OutputsRegion()
        {
            ToolRegionName = "OutputsRegion";
        }

        void OutputsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _modelItem.SetProperty("Outputs", _outputs.ToList());
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsOutputsEmptyRows");
            // ReSharper restore ExplicitCallerInfoArgument
        }
        private bool _outputMappingEnabled;
        private string _recordsetName;
        private IOutputDescription _description;
        private bool _isOutputsEmptyRows;
        private bool _outputCountExpandAllowed;

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
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
                IsVisible = region.IsVisible;
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
                    OnPropertyChanged();
                }
            }
        }

        public bool OutputCountExpandAllowed
        {
            get
            {
                return _outputCountExpandAllowed;
            }
            set
            {
                _outputCountExpandAllowed = value;
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
    }
}