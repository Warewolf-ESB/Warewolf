#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
        readonly ServiceDesignerViewModel _serviceDesignerViewModel;
        IWebActivityFactory _activityFactory;
        IDataMappingViewModelFactory _mappingFactory;
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

        void OnMappingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        void OnMappingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MapsTo":
                    SetInputs();
                    break;
                case "Value":
                    SetOuputs();
                    break;
                default:
                    break;
            }
        }

        public void UpdateLastValidationMemo(DesignValidationMemo memo) => UpdateLastValidationMemo(memo, true);

        public void UpdateLastValidationMemo(DesignValidationMemo memo, bool checkSource)
        {
            _serviceDesignerViewModel.ValidationMemoManager.LastValidationMemo = memo;

            CheckRequiredMappingChangedErrors(memo);
            CheckIsDeleted(memo);

            _serviceDesignerViewModel.ValidationMemoManager.UpdateDesignValidationErrors(memo.Errors.Where(info => info.InstanceID == _serviceDesignerViewModel.UniqueID && info.ErrorType != ErrorType.None));
            if (_serviceDesignerViewModel.SourceId == Guid.Empty && checkSource && _serviceDesignerViewModel.CheckSourceMissing())
            {
                InitializeMappings();
                UpdateMappings();
            }

        }

        void CheckRequiredMappingChangedErrors(DesignValidationMemo memo)
        {
            var keepError = false;
            var reqiredMappingChanged = memo.Errors.FirstOrDefault(c => c.FixType == FixType.IsRequiredChanged);
            if (reqiredMappingChanged != null)
            {
                keepError = KeepError(reqiredMappingChanged);

                if (!keepError)
                {
                    var worstErrors = memo.Errors.Where(c => c.FixType == FixType.IsRequiredChanged && c.InstanceID == _serviceDesignerViewModel.UniqueID).ToList();
                    memo.Errors.RemoveAll(c => c.FixType == FixType.IsRequiredChanged && c.InstanceID == _serviceDesignerViewModel.UniqueID);
                    _serviceDesignerViewModel.ValidationMemoManager.RemoveErrors(worstErrors);
                }
            }
        }

        bool KeepError(IErrorInfo reqiredMappingChanged)
        {
            bool keepError = false;
            if (reqiredMappingChanged.FixData != null)
            {
                keepError = KeepFixDataError(reqiredMappingChanged);
            }

            return keepError;
        }

        bool KeepFixDataError(IErrorInfo reqiredMappingChanged)
        {
            bool keepError = false;
            var xElement = XElement.Parse(reqiredMappingChanged.FixData);
            var inputOutputViewModels = DeserializeMappings(true, xElement);

            foreach (var input in inputOutputViewModels)
            {
                keepError = KeepInputOutputViewModelError(input);
            }

            return keepError;
        }

        bool KeepInputOutputViewModelError(IInputOutputViewModel input)
        {
            bool keepError = false;
            var currentInputViewModel = input;
            var inputOutputViewModel = DataMappingViewModel?.Inputs.FirstOrDefault(c => c.Name == currentInputViewModel.Name);
            if (inputOutputViewModel != null)
            {
                inputOutputViewModel.Required = input.Required;
                if (inputOutputViewModel.MapsTo == string.Empty && inputOutputViewModel.Required)
                {
                    keepError = true;
                }
            }

            return keepError;
        }

        void CheckIsDeleted(DesignValidationMemo memo)
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

        ErrorInfo CreateMappingIsRequiredMessage() => new ErrorInfo { ErrorType = ErrorType.Critical, FixData = CreateFixedData(), FixType = FixType.IsRequiredChanged, InstanceID = _serviceDesignerViewModel.UniqueID };

        string CreateFixedData()
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
                    Dev2Logger.Error(e, "Warewolf Error");
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
                    var mapping = newMapping;
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
                Dev2Logger.Error(e, "Warewolf Error");
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
                serviceDesignerViewModel.ValidationMemoManager.SetVersionsDifferent(true);
            }
        }
    }
}