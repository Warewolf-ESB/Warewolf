using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Utils;
using Dev2.Communication;
using Dev2.Data.Util;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities.Designers2.Core
{
    // ReSharper disable ClassWithVirtualMembersNeverInherited.Global
    public class OutputsRegion : IOutputsToolRegion
    {
        private readonly ModelItem _modelItem;
        private bool _isEnabled;
        private ICollection<IServiceOutputMapping> _outputs;
        public OutputsRegion(ModelItem modelItem, bool isObjectOutputUsed = false)
        {
            ToolRegionName = "OutputsRegion";
            _modelItem = modelItem;
            if (_modelItem.GetProperty("Outputs") == null||_modelItem.GetProperty<IList<IServiceOutputMapping>>("Outputs").Count ==0)
            {
                var current = _modelItem.GetProperty<ICollection<IServiceOutputMapping>>("Outputs");
                if(current == null)
                {
                    IsEnabled = false;
                }
                var outputs = new ObservableCollection<IServiceOutputMapping>(current ?? new List<IServiceOutputMapping>());
                outputs.CollectionChanged += OutputsCollectionChanged;
                Outputs = outputs;
            }
            else
            {
                IsEnabled = true;
                var outputs = new ObservableCollection<IServiceOutputMapping>(_modelItem.GetProperty<ICollection<IServiceOutputMapping>>("Outputs"));
                outputs.CollectionChanged += OutputsCollectionChanged;
                Outputs = outputs;
            }
            IsObject = _modelItem.GetProperty<bool>("IsObject");
            ObjectResult = _modelItem.GetProperty<string>("ObjectResult");
            ObjectName = _modelItem.GetProperty<string>("ObjectName");
            IsObjectOutputUsed = isObjectOutputUsed;
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
            
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        //Needed for Deserialization
        public OutputsRegion()
        {
            ToolRegionName = "OutputsRegion";
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
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
        private bool _isObject;
        private string _objectName;
        private string _objectResult;
        private bool _isObjectOutputUsed;
        private IShellViewModel _shellViewModel;

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsObjectOutputUsed
        {
            get { return _isObjectOutputUsed; }
            set
            {
                _isObjectOutputUsed = value;
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
                Outputs = region.Outputs;
                RecordsetName = region.RecordsetName;
                IsEnabled = toRestore.IsEnabled;
                ObjectResult = region.ObjectResult;
                ObjectName = region.ObjectName;
                IsObject = region.IsObject;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("IsOutputsEmptyRows");
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
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
                else
                {
                    _outputs.Clear();
                    _modelItem.SetProperty("Outputs", _outputs.ToList());
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

        public bool IsObject
        {
            get { return _isObject; }
            set
            {
                _isObject = value;
                _modelItem.SetProperty("IsObject", value);
                OnPropertyChanged();
            }            
        }

        public string ObjectName
        {
            get { return _objectName; }
            set
            {
               
                if (IsObject &&!string.IsNullOrEmpty(ObjectResult))
                {
                    try
                    {
                        if (value != null)
                        {
                            _objectName = value;
                            OnPropertyChanged();
                            var language = FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(value);
                            if (language.IsJsonIdentifierExpression)
                            {
                                _shellViewModel.UpdateCurrentDataListWithObjectFromJson(DataListUtil.RemoveLanguageBrackets(value), ObjectResult);
                            }                            
                            _modelItem.SetProperty("ObjectName", value);                            
                        }
                        else
                        {
                            _objectName = string.Empty;
                            _modelItem.SetProperty("ObjectName", _objectName);
                            OnPropertyChanged();
                        }
                    }
                    catch(Exception)
                    {
                        //Is not an object identifier
                    }
                    
                }
            }
        }

        public string ObjectResult
        {
            get { return _objectResult; }
            set
            {
                if (value != null)
                {
                    _objectResult = JSONUtils.Format(value);
                    _modelItem.SetProperty("ObjectResult", value);
                    OnPropertyChanged();
                }
                else
                {
                    _objectResult = string.Empty;
                    _modelItem.SetProperty("ObjectResult", _objectResult);
                    OnPropertyChanged();
                }
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
                var errors = new List<string>();
                try
                {
                    if (Outputs != null && Outputs.Count > 0 && !IsObject)
                    {
                        errors = Outputs.Where(a => FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(a.MappedTo).IsComplexExpression || FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(a.MappedTo).IsWarewolfAtomExpression).Select(a => "Invalid Output Mapping" + a.ToString()).ToList();
                    }
                }
                catch(Exception e)
                {
                    errors.Add(e.Message);
                }
                if(IsObject && string.IsNullOrEmpty(ObjectName))
                {
                    errors.Add(ErrorResource.NoObjectName);
                }
                return errors;
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