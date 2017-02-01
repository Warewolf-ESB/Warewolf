using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Common.Utils;
using Dev2.Data.Util;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Core;
using Warewolf.Storage;
// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class DotNetMethodRegion : IMethodToolRegion<IPluginAction>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IPluginSource> _source;
        private readonly INamespaceToolRegion<INamespaceItem> _namespace;
        private bool _isEnabled;

        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        private Action _sourceChangedAction;
        private RelayCommand _viewObjectResult;
        private RelayCommand _viewObjectForServiceInputResult;
        private IPluginAction _selectedMethod;
        private readonly IPluginServiceModel _model;
        private ICollection<IPluginAction> _methodsToRun;
        private bool _isRefreshing;
        private double _labelWidth;
        private IList<string> _errors;
        private bool _isMethodExpanded;
        private readonly IShellViewModel _shellViewModel;
        private readonly IActionInputDatatalistMapper _datatalistMapper;

        public DotNetMethodRegion()
        {
            ToolRegionName = "DotNetMethodRegion";
        }

        internal DotNetMethodRegion(IShellViewModel shellViewModel, IActionInputDatatalistMapper datatalistMapper)
        {
            VerifyArgument.IsNotNull(nameof(shellViewModel), shellViewModel);
            VerifyArgument.IsNotNull(nameof(datatalistMapper), datatalistMapper);
            _shellViewModel = shellViewModel;
            _datatalistMapper = datatalistMapper;
        }

        public DotNetMethodRegion(IPluginServiceModel model, ModelItem modelItem, ISourceToolRegion<IPluginSource> source, INamespaceToolRegion<INamespaceItem> namespaceItem)
            : this(CustomContainer.Get<IShellViewModel>(), new ActionInputDatatalistMapper())
        {
            try
            {
                Errors = new List<string>();

                LabelWidth = 70;
                ToolRegionName = "DotNetMethodRegion";
                _modelItem = modelItem;
                _model = model;
                _source = source;
                _namespace = namespaceItem;
                _namespace.SomethingChanged += SourceOnSomethingChanged;
                Dependants = new List<IToolRegion>();
                IsRefreshing = false;
                if (_source.SelectedSource != null)
                {
                    MethodsToRun = model.GetActionsWithReturns(_source.SelectedSource, _namespace.SelectedNamespace);
                }
                if (Method != null && MethodsToRun != null)
                {
                    SelectedMethod = Method;
                }
                RefreshMethodsCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() =>
                {
                    IsRefreshing = true;
                    if (_source.SelectedSource != null)
                    {
                        MethodsToRun = model.GetActionsWithReturns(_source.SelectedSource, _namespace.SelectedNamespace);
                    }
                    IsRefreshing = false;
                }, CanRefresh);

                IsMethodExpanded = false;
                IsEnabled = true;
                _modelItem = modelItem;
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
            }
        }

        IPluginAction Method
        {
            get
            {
                return _modelItem.GetProperty<List<IPluginAction>>("MethodsToRun").FirstOrDefault();
            }
            set
            {
                _modelItem.SetProperty("Method", value);
                _modelItem.SetProperty("MethodsToRun", new List<IPluginAction>(new[] { value }));
            }
        }

        public double LabelWidth
        {
            get
            {
                return _labelWidth;
            }
            set
            {
                _labelWidth = value;
                OnPropertyChanged();
            }
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();
                IsRefreshing = true;
                // ReSharper disable once ExplicitCallerInfoArgument
                UpdateBasedOnNamespace();
                IsRefreshing = false;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(@"IsEnabled");
            }
            catch (Exception e)
            {
                IsRefreshing = false;
                Errors.Add(e.Message);
            }
            finally
            {
                OnSomethingChanged(this);
                CallErrorsEventHandler();
            }
        }

        private void CallErrorsEventHandler()
        {
            ErrorsHandler?.Invoke(this, new List<string>(Errors));
        }

        private void UpdateBasedOnNamespace()
        {
            if (_source?.SelectedSource != null)
            {
                MethodsToRun = _model.GetActionsWithReturns(_source.SelectedSource, _namespace.SelectedNamespace);
                SelectedMethod = null;
                IsEnabled = true;
            }
        }

        public bool CanRefresh()
        {
            return IsActionEnabled;
        }

        public IPluginAction SelectedMethod
        {
            get
            {
                return _selectedMethod;
            }
            set
            {

                _datatalistMapper?.MapInputsToDatalist(value?.Inputs);
                RestoreIfPrevious(value);
                OnPropertyChanged();
                OnPropertyChanged("IsVoid");
                OnPropertyChanged("Method");
                OnPropertyChanged("RecordsetName");
                OnPropertyChanged("IsObject");
                OnPropertyChanged("ObjectName");
                OnPropertyChanged("ObjectResult");
                OnPropertyChanged("Inputs");
                OnPropertyChanged("IsInputsEmptyRows");
            }
        }

        public bool IsVoid
        {
            get
            {
                return _selectedMethod == null || _selectedMethod.IsVoid;
            }
            set
            {
                if (_selectedMethod != null)
                {
                    _selectedMethod.IsVoid = value;
                }
                OnPropertyChanged();
            }
        }
        public string RecordsetName
        {
            get
            {
                if (_selectedMethod != null)
                {
                    return _selectedMethod.OutputVariable;
                }
                return string.Empty;
            }
            set
            {
                if (_selectedMethod != null)
                {
                    _selectedMethod.OutputVariable = value;
                }
                OnPropertyChanged();
            }
        }

        public bool IsObject
        {
            get { return _selectedMethod != null && _selectedMethod.IsObject; }
            set
            {
                if (_selectedMethod != null)
                    _selectedMethod.IsObject = value;
                OnPropertyChanged();
            }
        }

        public bool IsObjectEnabled => !IsObject;

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

        public RelayCommand ViewObjectResultForParameterInput
        {
            get
            {
                return _viewObjectForServiceInputResult ?? (_viewObjectForServiceInputResult = new RelayCommand(item =>
                {
                    var serviceInput = item as ServiceInput;
                    ViewObjectsResultForParameterInput(serviceInput);
                }, CanRunCommand));
            }
        }

        private bool CanRunCommand(object obj)
        {
            return true;
        }

        private void ViewJsonObjects()
        {
            JsonObjectsView?.ShowJsonString(JSONUtils.Format(ObjectResult));
        }

        private void ViewObjectsResultForParameterInput(IServiceInput input)
        {
            JsonObjectsView?.ShowJsonString(JSONUtils.Format(input.Dev2ReturnType));
        }

        public string ObjectName
        {
            get { return _selectedMethod?.OutputVariable; }
            set
            {
                if (IsObject && !string.IsNullOrEmpty(ObjectResult))
                {
                    try
                    {
                        if (value != null)
                        {
                            _selectedMethod.OutputVariable = value;
                            OnPropertyChanged();
                            var language = FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(value);
                            if (language.IsJsonIdentifierExpression)
                            {
                                _shellViewModel.UpdateCurrentDataListWithObjectFromJson(DataListUtil.RemoveLanguageBrackets(value), ObjectResult);
                            }
                        }
                        else
                        {
                            _selectedMethod.OutputVariable = string.Empty;
                            OnPropertyChanged();
                        }
                    }
                    catch (Exception)
                    {
                        //Is not an object identifier
                    }
                }
            }
        }

        public bool IsInputsEmptyRows => Inputs == null || Inputs.Count == 0;


        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _selectedMethod?.Inputs;
            }
            set
            {
                if (value != null)
                {
                    if (_selectedMethod != null)
                    {
                        _selectedMethod.Inputs = value.ToList();
                    }
                    OnPropertyChanged();
                    OnPropertyChanged("IsInputsEmptyRows");
                }
                else
                {
                    _selectedMethod?.Inputs.Clear();
                    OnPropertyChanged();
                    OnPropertyChanged("IsInputsEmptyRows");
                }
            }
        }

        public string ObjectResult
        {
            get { return _selectedMethod?.Dev2ReturnType; }
            set
            {
                if (value != null)
                {
                    _selectedMethod.Dev2ReturnType = JSONUtils.Format(value);
                    OnPropertyChanged();
                }
                else
                {
                    _selectedMethod.Dev2ReturnType = string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        private void RestoreIfPrevious(IPluginAction value)
        {
            if (IsAPreviousValue(value) && _selectedMethod != null)
            {
                RestorePreviousValues(value);
                SetSelectedAction(value);
            }
            else
            {
                SetSelectedAction(value);
                SourceChangedAction();
                OnSomethingChanged(this);
            }
            var delegateCommand = RefreshMethodsCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
            delegateCommand?.RaiseCanExecuteChanged();

            _selectedMethod = value;
        }

        public ICollection<IPluginAction> MethodsToRun
        {
            get
            {
                return _methodsToRun;
            }
            set
            {

                _methodsToRun = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshMethodsCommand { get; set; }
        public bool IsActionEnabled => _source.SelectedSource != null && _namespace.SelectedNamespace != null;
        public bool IsDeleteActionEnabled => SelectedMethod != null;

        public bool IsMethodExpanded
        {
            get
            {
                return _isMethodExpanded;
            }
            set
            {
                _isMethodExpanded = value;
                OnPropertyChanged();
            }
        }
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public Action SourceChangedAction
        {
            get
            {
                return _sourceChangedAction ?? (() => { });
            }
            set
            {
                _sourceChangedAction = value;
            }
        }
        public event SomethingChanged SomethingChanged;

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
        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            return new DotNetMethodRegion
            {
                IsEnabled = IsEnabled,
                SelectedMethod = SelectedMethod == null ? null : new PluginAction
                {
                    Inputs = SelectedMethod?.Inputs?.Select(a =>
                    {
                        var serviceInput = new ServiceInput(a.Name, a.Value) as IServiceInput;
                        serviceInput.IntellisenseFilter = a.IntellisenseFilter;
                        return serviceInput;
                    }).ToList(),
                    FullName = SelectedMethod.FullName,
                    Method = SelectedMethod.Method,
                }
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DotNetMethodRegion;
            if (region != null)
            {
                SelectedMethod = region.SelectedMethod;
                RestoreIfPrevious(region.SelectedMethod);
                IsEnabled = region.IsEnabled;
                OnPropertyChanged("SelectedMethod");
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        #region Implementation of IActionToolRegion<IPluginAction>

        private void SetSelectedAction(IPluginAction value)
        {
            _selectedMethod = value;
            if (value != null)
            {
                Method = value;
            }
            OnPropertyChanged("SelectedMethod");
        }

        private void RestorePreviousValues(IPluginAction value)
        {
            var toRestore = _previousRegions[value.GetIdentifier()];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        private bool IsAPreviousValue(IPluginAction value)
        {
            return value != null && _previousRegions.Keys.Any(a => a == value.GetIdentifier());
        }

        public IList<string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            handler?.Invoke(this, args);
        }
    }
}