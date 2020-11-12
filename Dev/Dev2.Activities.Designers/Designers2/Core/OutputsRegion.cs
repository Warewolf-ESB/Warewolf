/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
using IFieldAndPropertyMapper = Dev2.Common.IFieldAndPropertyMapper;

namespace Dev2.Activities.Designers2.Core
{
    public class OutputsRegion : IOutputsToolRegion
    {
        private readonly ModelItem _modelItem;
        private bool _isEnabled;
        private ICollection<IServiceOutputMapping> _outputs;
        private readonly IFieldAndPropertyMapper _mapper;

        public OutputsRegion(ModelItem modelItem)
            : this(modelItem, false, CustomContainer.Get<IFieldAndPropertyMapper>())
        {
        }

        public OutputsRegion(ModelItem modelItem, bool isObjectOutputUsed)
            : this(modelItem, isObjectOutputUsed, CustomContainer.Get<IFieldAndPropertyMapper>())
        {
        }

        public OutputsRegion(ModelItem modelItem, bool isObjectOutputUsed, IFieldAndPropertyMapper mapper)
            : this(mapper)
        {
            Dependants = new List<IToolRegion>();
            _modelItem = modelItem;
            var serviceOutputMappings = _modelItem.GetProperty<ICollection<IServiceOutputMapping>>("Outputs");
            var outputs = new ObservableCollection<IServiceOutputMapping>();
            IsEnabled = serviceOutputMappings != null;
            if (serviceOutputMappings != null)
            {
                outputs.AddRange(serviceOutputMappings);
            }
            outputs.CollectionChanged += OutputsCollectionChanged;
            Outputs = outputs;

            IsObject = _modelItem.GetProperty<bool>("IsObject");
            IsResponseBase64 = _modelItem.GetProperty<bool>("IsResponseBase64");
            ObjectResult = _modelItem.GetProperty<string>("ObjectResult");
            ObjectName = _modelItem.GetProperty<string>("ObjectName");
            IsObjectOutputUsed = isObjectOutputUsed;
            IsOutputsEmptyRows = !IsObject ? Outputs.Count == 0 : !string.IsNullOrWhiteSpace(ObjectResult);
        }

        //Needed for Deserialization
        public OutputsRegion()
            : this(CustomContainer.Get<IFieldAndPropertyMapper>())
        {
        }

        public OutputsRegion(IFieldAndPropertyMapper mapper)
        {
            _mapper = mapper;
            ToolRegionName = "OutputsRegion";
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
        }

        public void ResetOutputs(ICollection<IServiceOutputMapping> outputs)
        {
            var newOutputs = new ObservableCollection<IServiceOutputMapping>();
            newOutputs.CollectionChanged += OutputsCollectionChanged;
            newOutputs.AddRange(outputs);
            Outputs = newOutputs;
        }

        private void OutputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsOutputsEmptyRows));
            AddItemPropertyChangeEvent(e);
            RemoveItemPropertyChangeEvent(e);
        }

        private void AddItemPropertyChangeEvent(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in e.NewItems)
            {
                if (item != null)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Outputs.Equals(_outputs))
            {
                _modelItem.SetProperty("Outputs", _outputs.ToList());
            }
        }

        private void RemoveItemPropertyChangeEvent(NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems == null)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in e.OldItems)
            {
                if (item != null)
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        private bool _outputMappingEnabled;
        private string _recordsetName;
        private IOutputDescription _description;
        private bool _isOutputsEmptyRows;
        private bool _outputCountExpandAllowed;
        private bool _isObject;
        private bool _isResponseBase64;
        private string _objectName;
        private string _objectResult;
        private bool _isObjectOutputUsed;
        private IShellViewModel _shellViewModel;
        private RelayCommand _viewObjectResult;

        public string ToolRegionName { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsObjectOutputUsed
        {
            get => _isObjectOutputUsed;
            set
            {
                _isObjectOutputUsed = value;
                OnPropertyChanged();
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            _mapper.AddMap<OutputsRegion, OutputsRegion>();
            var outputsRegion = new OutputsRegion();
            _mapper.Map(this, outputsRegion);
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

                OnPropertyChanged(nameof(IsOutputsEmptyRows));
            }
        }

        public EventHandler<List<string>> ErrorsHandler { get; set; }

        public ICollection<IServiceOutputMapping> Outputs
        {
            get => _outputs;
            set
            {
                if (value != null)
                {
                    _outputs = value;
                    IsOutputsEmptyRows = !IsObject ? Outputs.Count == 0 : !string.IsNullOrWhiteSpace(ObjectResult);
                }
                else
                {
                    _outputs.Clear();
                }
                _modelItem.SetProperty("Outputs", _outputs.ToList());
                OnPropertyChanged();
            }
        }

        public bool OutputCountExpandAllowed
        {
            get => _outputCountExpandAllowed;
            set
            {
                _outputCountExpandAllowed = value;
                OnPropertyChanged();
            }
        }

        public bool OutputMappingEnabled
        {
            get => _outputMappingEnabled;
            set
            {
                _outputMappingEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsOutputsEmptyRows
        {
            get => _isOutputsEmptyRows;
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
            get => _isObject;
            set
            {
                _isObject = value;
                _modelItem.SetProperty("IsObject", value);
                OnPropertyChanged();
            }
        }

        public bool IsResponseBase64
        {
            get => _isResponseBase64;
            set
            {
                _isResponseBase64 = value;
                _modelItem.SetProperty("IsResponseBase64", value);
                OnPropertyChanged();
            }
        }


        public IJsonObjectsView JsonObjectsView => CustomContainer.GetInstancePerRequestType<IJsonObjectsView>();

        public RelayCommand ViewObjectResult => _viewObjectResult ??
                                                (_viewObjectResult = new RelayCommand(item => { ViewJsonObjects(); },
                                                    CanRunCommand));

        private static bool CanRunCommand(object obj) => true;

        private void ViewJsonObjects()
        {
            JsonObjectsView?.ShowJsonString(JsonUtils.Format(ObjectResult));
        }


        public string ObjectName
        {
            get => _objectName;
            set
            {
                if (IsObject && !string.IsNullOrEmpty(ObjectResult))
                {
                    TrySetObjectName(value);
                }
            }
        }

        private void TrySetObjectName(string value)
        {
            try
            {
                if (value != null)
                {
                    SetObjectName(value);
                }
                else
                {
                    _objectName = string.Empty;
                    _modelItem.SetProperty("ObjectName", _objectName);
                    OnPropertyChanged();
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message, GlobalConstants.WarewolfError);
            }
        }

        private void SetObjectName(string value)
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

                _shellViewModel.UpdateCurrentDataListWithObjectFromJson(DataListUtil.RemoveLanguageBrackets(value),
                    ObjectResult);
            }

            _modelItem.SetProperty("ObjectName", value);
        }

        public string ObjectResult
        {
            get => _objectResult;
            set
            {
                if (value != null)
                {
                    _objectResult = JsonUtils.Format(value);
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
            get => _description;
            set
            {
                _description = value;
                _modelItem.SetProperty("OutputDescription", value);
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
                        var serviceOutputMappings = Outputs.Where(a =>
                            !string.IsNullOrEmpty(a.MappedTo) &&
                            (FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(a.MappedTo).IsComplexExpression ||
                             FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(a.MappedTo)
                                 .IsWarewolfAtomExpression));
                        errors = serviceOutputMappings.Select(a => "Invalid Output Mapping: " + a.MappedTo).ToList();
                    }
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                }

                if (IsObject && string.IsNullOrEmpty(ObjectName))
                {
                    errors.Add(ErrorResource.NoObjectName);
                }

                return errors;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}