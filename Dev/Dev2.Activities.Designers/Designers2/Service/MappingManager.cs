using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Studio.Interfaces;


namespace Dev2.Activities.Designers2.Service
{
    public class MappingManager
    {
        private readonly ServiceDesignerViewModel _serviceDesignerViewModel;
        private IWebActivityFactory _activityFactory;
        private IDataMappingViewModelFactory _mappingFactory;
        bool _resourcesUpdated;

        internal MappingManager(ServiceDesignerViewModel serviceDesignerViewModel)
        {
            _serviceDesignerViewModel = serviceDesignerViewModel;
        }

        public IDataMappingViewModel DataMappingViewModel { get; private set; }

        

        public void UpdateMappings()
        {
            if (!_resourcesUpdated)
            {
                SetInputs();
                SetOuputs();
            }
        }


        public IWebActivityFactory ActivityFactory
        {
            private get { return _activityFactory ?? new InstanceWebActivityFactory(); }
            set { _activityFactory = value; }
        }

        public IDataMappingViewModelFactory MappingFactory
        {
            private get { return _mappingFactory ?? new DataMappingViewModelFactory(); }
            set { _mappingFactory = value; }
        }


        public void SetInputs()
        {
            if (DataMappingViewModel != null)
            {
                _serviceDesignerViewModel.InputMapping = DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs);
            }
        }

        public void SetOuputs()
        {
            if (DataMappingViewModel != null)
            {
                _serviceDesignerViewModel.OutputMapping = DataMappingViewModel.GetOutputString(DataMappingViewModel.Outputs);
            }
        }

        public void InitializeMappings()
        {
            var webAct = ActivityFactory.CreateWebActivity(_serviceDesignerViewModel.ModelItem, _serviceDesignerViewModel.ResourceModel, _serviceDesignerViewModel.ServiceName);
            DataMappingViewModel = new DataMappingViewModel(webAct, OnMappingCollectionChanged);
        }

        private void OnMappingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IInputOutputViewModel mapping in e.NewItems)
                {
                    mapping.PropertyChanged += OnMappingPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (IInputOutputViewModel mapping in e.OldItems)
                {
                    mapping.PropertyChanged -= OnMappingPropertyChanged;
                }
            }
        }

        private void OnMappingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MapsTo":
                    SetInputs();
                    break;

                case "Value":
                    SetOuputs();
                    break;
            }
        }

        public void UpdateLastValidationMemo(DesignValidationMemo memo, bool checkSource = true)
        {
            _serviceDesignerViewModel.ValidationMemoManager.LastValidationMemo = memo;

            CheckRequiredMappingChangedErrors(memo);
            CheckIsDeleted(memo);

            _serviceDesignerViewModel.ValidationMemoManager.UpdateDesignValidationErrors(memo.Errors.Where(info => info.InstanceID == _serviceDesignerViewModel.UniqueID && info.ErrorType != ErrorType.None));
            if (_serviceDesignerViewModel.SourceId == Guid.Empty)
            {
                if (checkSource && _serviceDesignerViewModel.CheckSourceMissing())
                {
                    InitializeMappings();
                    UpdateMappings();
                }
            }
        }

        private void CheckRequiredMappingChangedErrors(DesignValidationMemo memo)
        {
            var keepError = false;
            var reqiredMappingChanged = memo.Errors.FirstOrDefault(c => c.FixType == FixType.IsRequiredChanged);
            if (reqiredMappingChanged != null)
            {
                if (reqiredMappingChanged.FixData != null)
                {
                    var xElement = XElement.Parse(reqiredMappingChanged.FixData);
                    var inputOutputViewModels = DeserializeMappings(true, xElement);

                    foreach (var input in inputOutputViewModels)
                    {
                        IInputOutputViewModel currentInputViewModel = input;
                        var inputOutputViewModel = DataMappingViewModel?.Inputs.FirstOrDefault(c => c.Name == currentInputViewModel.Name);
                        if (inputOutputViewModel != null)
                        {
                            inputOutputViewModel.Required = input.Required;
                            if (inputOutputViewModel.MapsTo == string.Empty && inputOutputViewModel.Required)
                            {
                                keepError = true;
                            }
                        }
                    }
                }

                if (!keepError)
                {
                    var worstErrors = memo.Errors.Where(c => c.FixType == FixType.IsRequiredChanged && c.InstanceID == _serviceDesignerViewModel.UniqueID).ToList();
                    memo.Errors.RemoveAll(c => c.FixType == FixType.IsRequiredChanged && c.InstanceID == _serviceDesignerViewModel.UniqueID);
                    _serviceDesignerViewModel.ValidationMemoManager.RemoveErrors(worstErrors);
                }
            }
        }

        private void CheckIsDeleted(DesignValidationMemo memo)
        {
            var error = memo.Errors.FirstOrDefault(c => c.FixType == FixType.Delete);
            _serviceDesignerViewModel.IsDeleted = error != null;
            _serviceDesignerViewModel.IsEditable = !_serviceDesignerViewModel.IsDeleted;
            if (_serviceDesignerViewModel.IsDeleted)
            {
                while (memo.Errors.Count > 1)
                {
                    error = memo.Errors.FirstOrDefault(c => c.FixType != FixType.Delete);
                    if (error != null)
                    {
                        memo.Errors.Remove(error);
                    }
                }
            }
        }

        public void CheckForRequiredMapping()
        {
            if (DataMappingViewModel != null && DataMappingViewModel.Inputs.Any(c => c.Required && String.IsNullOrEmpty(c.MapsTo)))
            {
                if (_serviceDesignerViewModel.ValidationMemoManager.DesignValidationErrors.All(c => c.FixType != FixType.IsRequiredChanged))
                {
                    var listToRemove = _serviceDesignerViewModel.ValidationMemoManager.DesignValidationErrors.Where(c => c.FixType == FixType.None && c.ErrorType == ErrorType.None).ToList();

                    foreach (var errorInfo in listToRemove)
                    {
                        _serviceDesignerViewModel.ValidationMemoManager.DesignValidationErrors.Remove(errorInfo);
                    }

                    var mappingIsRequiredMessage = CreateMappingIsRequiredMessage();
                    _serviceDesignerViewModel.ValidationMemoManager.DesignValidationErrors.Add(mappingIsRequiredMessage);
                    _serviceDesignerViewModel.RootModel.AddError(mappingIsRequiredMessage);
                }
                _serviceDesignerViewModel.ValidationMemoManager.UpdateWorstError();
                return;
            }

            if (_serviceDesignerViewModel.ValidationMemoManager.DesignValidationErrors.Any(c => c.FixType == FixType.IsRequiredChanged))
            {
                var listToRemove = _serviceDesignerViewModel.ValidationMemoManager.DesignValidationErrors.Where(c => c.FixType == FixType.IsRequiredChanged).ToList();

                foreach (var errorInfo in listToRemove)
                {
                    _serviceDesignerViewModel.ValidationMemoManager.DesignValidationErrors.Remove(errorInfo);
                    _serviceDesignerViewModel.RootModel.RemoveError(errorInfo);
                }
                _serviceDesignerViewModel.ValidationMemoManager.UpdateWorstError();
            }
        }

        private ErrorInfo CreateMappingIsRequiredMessage()
        {
            return new ErrorInfo { ErrorType = ErrorType.Critical, FixData = CreateFixedData(), FixType = FixType.IsRequiredChanged, InstanceID = _serviceDesignerViewModel.UniqueID };
        }

        private string CreateFixedData()
        {
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Serialize(DataMappingListFactory.CreateListInputMapping(DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs)));
            return string.Concat("<Input>", result, "</Input>");
        }

        public XElement FetchXElementFromFixData()
        {
            if (!string.IsNullOrEmpty(_serviceDesignerViewModel.ValidationMemoManager.WorstDesignError?.FixData))
            {
                try
                {
                    return XElement.Parse(_serviceDesignerViewModel.ValidationMemoManager.WorstDesignError.FixData);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e);
                }
            }

            return XElement.Parse("<x/>");
        }

        public IEnumerable<IInputOutputViewModel> GetMapping(XContainer xml, bool isInput, ObservableCollection<IInputOutputViewModel> oldMappings)
        {
            var result = new ObservableCollection<IInputOutputViewModel>();

            var input = xml.Descendants(isInput ? "Input" : "Output").FirstOrDefault();
            if (input != null)
            {
                var newMappings = DeserializeMappings(isInput, input);

                foreach (var newMapping in newMappings)
                {
                    IInputOutputViewModel mapping = newMapping;
                    var oldMapping = oldMappings.FirstOrDefault(m => m.Name.Equals(mapping.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (oldMapping != null)
                    {
                        newMapping.MapsTo = oldMapping.MapsTo;
                        newMapping.Value = oldMapping.Value;
                    }
                    else
                    {
                        newMapping.IsNew = true;
                    }
                    result.Add(newMapping);
                }
            }
            return result;
        }

        public IEnumerable<IInputOutputViewModel> DeserializeMappings(bool isInput, XElement input)
        {
            try
            {
                var serializer = new Dev2JsonSerializer();
                var defs = serializer.Deserialize<List<Dev2Definition>>(input.Value);
                IList<IDev2Definition> idefs = new List<IDev2Definition>(defs);
                var newMappings = isInput
                    ? InputOutputViewModelFactory.CreateListToDisplayInputs(idefs)
                    : InputOutputViewModelFactory.CreateListToDisplayOutputs(idefs);
                return newMappings;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
            }

            return new List<IInputOutputViewModel>();
        }

        public void CheckVersions(ServiceDesignerViewModel serviceDesignerViewModel)
        {
            if (serviceDesignerViewModel.ValidationMemoManager.LastValidationMemo != null && serviceDesignerViewModel.ValidationMemoManager.LastValidationMemo.Errors.Any(a => a.Message.Contains("This service will only execute when the server is online.")))
            {
                serviceDesignerViewModel.ValidationMemoManager.RemoveErrors(serviceDesignerViewModel.ValidationMemoManager.LastValidationMemo.Errors.Where(
                    a => a.Message.Contains("This service will only execute when the server is online.")).ToList());
                serviceDesignerViewModel.ValidationMemoManager.UpdateWorstError();
            }
            var webAct = ActivityFactory.CreateWebActivity(serviceDesignerViewModel.NewModel, serviceDesignerViewModel.NewModel, serviceDesignerViewModel.ServiceName);
            var newMapping = MappingFactory.CreateModel(webAct, OnMappingCollectionChanged);
            if (newMapping.GetInputString(DataMappingViewModel.Inputs) != DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs) ||
                newMapping.GetOutputString(DataMappingViewModel.Outputs) != DataMappingViewModel.GetOutputString(DataMappingViewModel.Outputs))
            {
                serviceDesignerViewModel.ValidationMemoManager.UpdateLastValidationMemoWithVersionChanged();
                _resourcesUpdated = true;
                serviceDesignerViewModel.ValidationMemoManager.VersionsDifferent = true;
            }
        }
    }
}