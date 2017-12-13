using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Utils;
using Dev2.Data.Util;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities.Designers2.Core
{
    public class OutputsRegion : IOutputsToolRegion
    {
        readonly ModelItem _modelItem;
        bool _isEnabled;
        ICollection<IServiceOutputMapping> _outputs;

        public OutputsRegion(ModelItem modelItem)
            : this(modelItem, false)
        {
        }

        public OutputsRegion(ModelItem modelItem, bool isObjectOutputUsed)
        {
            ToolRegionName = "OutputsRegion";
            Dependants = new List<IToolRegion>();
            _modelItem = modelItem;
            var serviceOutputMappings = _modelItem.GetProperty<ICollection<IServiceOutputMapping>>("Outputs");
            if (_modelItem.GetProperty("Outputs") == null||_modelItem.GetProperty<IList<IServiceOutputMapping>>("Outputs").Count ==0)
            {
                var current = serviceOutputMappings;
                if(current == null)
                {
                    IsEnabled = false;
                }
                var outputMappings = current ?? new List<IServiceOutputMapping>();
                var outputs = new ObservableCollection<IServiceOutputMapping>();
                outputs.CollectionChanged += OutputsCollectionChanged;
                outputs.AddRange(outputMappings);
                Outputs = outputs;
            }
            else
            {
                IsEnabled = true;
                var outputs = new ObservableCollection<IServiceOutputMapping>();
                outputs.CollectionChanged += OutputsCollectionChanged;
                outputs.AddRange(serviceOutputMappings);
                Outputs = outputs;
            }
           
            IsObject = _modelItem.GetProperty<bool>("IsObject");
            ObjectResult = _modelItem.GetProperty<string>("ObjectResult");
            ObjectName = _modelItem.GetProperty<string>("ObjectName");
            IsObjectOutputUsed = isObjectOutputUsed;
            IsOutputsEmptyRows = !IsObject ? Outputs.Count == 0 : !string.IsNullOrWhiteSpace(ObjectResult);
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
          
        }

    
        //Needed for Deserialization
        public OutputsRegion()
        {
            ToolRegionName = "OutputsRegion";
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
        }

        void OutputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
            OnPropertyChanged("IsOutputsEmptyRows");
            AddItemPropertyChangeEvent(e);
            RemoveItemPropertyChangeEvent(e);
            
        }


        void AddItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in args.NewItems)
            {
                if (item != null)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _modelItem.SetProperty("Outputs", _outputs.ToList());
        }

        void RemoveItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems == null)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in args.OldItems)
            {
                if (item != null)
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                }
            }
        }


        bool _outputMappingEnabled;
        string _recordsetName;
        IOutputDescription _description;
        bool _isOutputsEmptyRows;
        bool _outputCountExpandAllowed;
        bool _isObject;
        string _objectName;
        string _objectResult;
        bool _isObjectOutputUsed;
        IShellViewModel _shellViewModel;
        RelayCommand _viewObjectResult;

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
            Mapper.AddMap<OutputsRegion, OutputsRegion>();
            var outputsRegion = new OutputsRegion();
            Mapper.Map(this,outputsRegion);
            return outputsRegion;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is OutputsRegion region)
            {
                Outputs = region.Outputs;
                RecordsetName = region.RecordsetName;
                IsEnabled = toRestore.IsEnabled;
                ObjectResult = region.ObjectResult;
                ObjectName = region.ObjectName;
                IsObject = region.IsObject;

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
                    IsOutputsEmptyRows = !IsObject ? Outputs.Count == 0 : !string.IsNullOrWhiteSpace(ObjectResult);
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
                    var recSet = Outputs?.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                    if (recSet != null)
                    {
                        _recordsetName = recSet.RecordSetName;
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


        public IJsonObjectsView JsonObjectsView => CustomContainer.GetInstancePerRequestType<IJsonObjectsView>();

        public RelayCommand ViewObjectResult
        {
            get
            {
                return _viewObjectResult ?? (_viewObjectResult = new RelayCommand(item =>
                {
                    ViewJsonObjects();
                }, CanRunCommand));
            }
        }

        bool CanRunCommand(object obj)
        {
            return true;
        }

        void ViewJsonObjects()
        {
            JsonObjectsView?.ShowJsonString(JSONUtils.Format(ObjectResult));
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
                                if (_shellViewModel == null)
                                {
                                    _shellViewModel = CustomContainer.Get<IShellViewModel>();
                                }
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
                        var serviceOutputMappings = Outputs.Where(a => !string.IsNullOrEmpty(a.MappedTo) && (FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(a.MappedTo).IsComplexExpression || FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(a.MappedTo).IsWarewolfAtomExpression));
                        errors = serviceOutputMappings.Select(a => "Invalid Output Mapping" + a.MappedTo).ToList();
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
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

      
    }
}