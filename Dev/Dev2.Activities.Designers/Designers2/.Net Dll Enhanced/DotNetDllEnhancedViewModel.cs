using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.ConstructorRegion;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Activities.Designers2.Core.InputRegion;
using Dev2.Activities.Designers2.Core.NamespaceRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Activities.Designers2.Net_Dll_Enhanced
{
    public class DotNetDllEnhancedViewModel : CustomToolWithRegionBase, IDotNetEnhancedViewModel
    {
        private IOutputsToolRegion _outputsRegion;
        private IDotNetConstructorInputRegion _inputArea;
        private ISourceToolRegion<IPluginSource> _sourceRegion;
        private INamespaceToolRegion<INamespaceItem> _namespaceRegion;
        private IConstructorRegion<IPluginConstructor> _constructorRegion;
        private IMethodToolRegion<IPluginAction> _methodRegion;

        private IErrorInfo _worstDesignError;

        const string DoneText = "Done";
        const string FixText = "Fix";

        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;

        [ExcludeFromCodeCoverage]
        public DotNetDllEnhancedViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<IPluginServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;

            SetupCommonProperties();
            this.RunViewSetup();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Resources_Dot_net_DLL;
        }


        Guid UniqueID => GetProperty<Guid>();

        private void SetupCommonProperties()
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel();
            NoError = new ErrorInfo
            {
                ErrorType = ErrorType.None,
                Message = "Service Working Normally"
            };
            if (SourceRegion.SelectedSource == null)
            {
                UpdateLastValidationMemoWithSourceNotFoundError();
            }
            UpdateWorstError();
        }

        private void InitialiseViewModel()
        {
            BuildRegions();

            LabelWidth = 46;
            ButtonDisplayValue = DoneText;

            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            ShowExampleWorkflowLink = Visibility.Collapsed;

            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(o =>
            {
                FixErrors();
                IsWorstErrorReadOnly = true;
            });

            DeleteActionCommand = new DelegateCommand<DotNetMethodRegion>(DeleteAction);

            SetDisplayName("");
            OutputsRegion.OutputMappingEnabled = true;

            InitializeProperties();

        }

        private void DeleteAction(DotNetMethodRegion method)
        {
            if (method != null)
            {
                var newList = new ObservableCollection<IMethodToolRegion<IPluginAction>>();
                var methodToolRegions = newList.AddRange(MethodsToRunList);
                methodToolRegions.Remove(method);
                MethodsToRunList = methodToolRegions.ToObservableCollection();
            }
        }

        void UpdateLastValidationMemoWithSourceNotFoundError()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = UniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = UniqueID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.None,
                Message = _sourceNotFoundMessage
            });
            UpdateDesignValidationErrors(memo.Errors);
        }

        public void ClearValidationMemoWithNoFoundError()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = UniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = UniqueID,
                ErrorType = ErrorType.None,
                FixType = FixType.None,
                Message = ""
            });
            UpdateDesignValidationErrors(memo.Errors);
        }

        void UpdateDesignValidationErrors(IEnumerable<IErrorInfo> errors)
        {
            DesignValidationErrors.Clear();
            foreach (var error in errors)
            {
                DesignValidationErrors.Add(error);
            }
            UpdateWorstError();
        }

        public DotNetDllEnhancedViewModel(ModelItem modelItem, IPluginServiceModel model)
            : base(modelItem)
        {
            Model = model;
            SetupCommonProperties();
        }

        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
            if (Errors == null)
            {
                Errors = new List<IActionableErrorInfo>();
            }
            Errors.Clear();
            if (SourceRegion.Errors.Count > 0)
            {
                foreach (var designValidationError in SourceRegion.Errors)
                {
                    DesignValidationErrors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = designValidationError });
                }
            }
            if (Errors.Count <= 0)
            {
                ClearValidationMemoWithNoFoundError();
            }
            UpdateWorstError();
            InitializeProperties();
        }

        void UpdateWorstError()
        {
            if (DesignValidationErrors.Count == 0)
            {
                DesignValidationErrors.Add(NoError);
            }

            IErrorInfo[] worstError = { DesignValidationErrors[0] };

            foreach (var error in DesignValidationErrors.Where(error => error.ErrorType > worstError[0].ErrorType))
            {
                worstError[0] = error;
                if (error.ErrorType == ErrorType.Critical)
                {
                    break;
                }
            }
            WorstDesignError = worstError[0];
        }

        IErrorInfo WorstDesignError
        {
            // ReSharper disable once UnusedMember.Local
            get { return _worstDesignError; }
            set
            {
                if (_worstDesignError != value)
                {
                    _worstDesignError = value;
                    IsWorstErrorReadOnly = value == null || value.ErrorType == ErrorType.None || value.FixType == FixType.None || value.FixType == FixType.Delete;
                    WorstError = value?.ErrorType ?? ErrorType.None;
                }
            }
        }

        public int LabelWidth { get; set; }

        public List<KeyValuePair<string, string>> Properties { get; private set; }
        void InitializeProperties()
        {
            Properties = new List<KeyValuePair<string, string>>();
            AddProperty("Source :", SourceRegion.SelectedSource == null ? "" : SourceRegion.SelectedSource.Name);
            AddProperty("Type :", Type);
            AddProperty("Constructor :", ConstructorRegion.SelectedConstructor == null ? "" : ConstructorRegion.SelectedConstructor.ConstructorName);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        private IErrorInfo NoError { get; set; }

        public bool IsWorstErrorReadOnly
        {
            get { return (bool)GetValue(IsWorstErrorReadOnlyProperty); }
            private set
            {
                ButtonDisplayValue = value ? DoneText : FixText;
                SetValue(IsWorstErrorReadOnlyProperty, value);
            }
        }
        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(DotNetDllEnhancedViewModel), new PropertyMetadata(false));

        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }
        public static readonly DependencyProperty WorstErrorProperty =
            DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(DotNetDllEnhancedViewModel), new PropertyMetadata(ErrorType.None));

        private ServiceInputBuilder _builder;
        private ObservableCollection<IMethodToolRegion<IPluginAction>> _methodsToRunList;

        private string Type => GetProperty<string>();
        // ReSharper disable InconsistentNaming

        private void FixErrors()
        {
        }

        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }

        public Runtime.Configuration.ViewModels.Base.DelegateCommand FixErrorsCommand { get; set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }

        public string ButtonDisplayValue { get; set; }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion

        #region Overrides of CustomToolWithRegionBase

        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();
            if (SourceRegion == null)
            {
                SourceRegion = new DotNetSourceRegion(Model, ModelItem)
                {
                    SourceChangedAction = () =>
                    {
                        if (OutputsRegion != null)
                        {
                            OutputsRegion.IsEnabled = false;
                            OutputsRegion.IsObject = true;
                            OutputsRegion.IsOutputsEmptyRows = true;
                        }
                        ClearToolRegionErrors();
                    }
                };
                regions.Add(SourceRegion);
                NamespaceRegion = new DotNetNamespaceRegion(Model, ModelItem, SourceRegion)
                {
                    SourceChangedNamespace = () =>
                    {
                        if (OutputsRegion != null)
                        {
                            OutputsRegion.IsEnabled = true;
                            OutputsRegion.IsObject = true;
                            OutputsRegion.IsOutputsEmptyRows = true;
                        }
                        ClearToolRegionErrors();
                    },
                    IsNewPluginNamespace = true
                };
                NamespaceRegion.SomethingChanged += (sender, args) =>
                {
                    if (args.Errors != null)
                        Errors = args.Errors.Select(e => new ActionableErrorInfo
                        {
                            ErrorType = ErrorType.Critical,
                            Message = e
                        } as IActionableErrorInfo).ToList();
                    var dotNetNamespaceRegion = sender as DotNetNamespaceRegion;
                    var outputsRegion = dotNetNamespaceRegion?.Dependants.Single(region => region is OutputsRegion) as OutputsRegion;
                    if (outputsRegion != null)
                    {
                        if (dotNetNamespaceRegion.SelectedNamespace != null)
                        {
                            outputsRegion.ObjectResult = dotNetNamespaceRegion.SelectedNamespace.JsonObject;
                        }
                    }
                    OnPropertyChanged("IsActionsVisible");
                };
                regions.Add(NamespaceRegion);
                ConstructorRegion = new DotNetConstructorRegion(Model, ModelItem, SourceRegion, NamespaceRegion)
                {
                    SourceChangedAction = () =>
                    {
                        if (OutputsRegion != null)
                        {
                            OutputsRegion.IsEnabled = true;
                            OutputsRegion.IsObject = true;
                            OutputsRegion.IsOutputsEmptyRows = !string.IsNullOrWhiteSpace(OutputsRegion.ObjectResult);
                        }

                        //ClearToolRegionErrors();
                    }
                };
                ConstructorRegion.SomethingChanged += (sender, args) =>
                {
                    OnPropertyChanged("IsConstructorVisible");
                };


                ConstructorRegion.ErrorsHandler += (sender, list) =>
                {
                    List<ActionableErrorInfo> errorInfos = list.Select(error => new ActionableErrorInfo(new ErrorInfo { ErrorType = ErrorType.Critical, Message = error }, () => { })).ToList();
                    UpdateDesignValidationErrors(errorInfos);
                    Errors = new List<IActionableErrorInfo>(errorInfos);
                };
                regions.Add(ConstructorRegion);

                InputArea = new DotNetConstructorInputRegion(ModelItem, ConstructorRegion);
                regions.Add(InputArea);
                OutputsRegion = new OutputsRegion(ModelItem, true)
                {
                    IsObject = true,
                    IsEnabled = false
                };
                regions.Add(OutputsRegion);

                var pluginActions = ModelItem.GetProperty<List<IPluginAction>>("MethodsToRun");
                var regionCollections = BuildRegionsFromActions(pluginActions);
                if (regionCollections.Any())
                {
                    MethodsToRunList = regionCollections;
                    CreateMethodRegion();
                }
                else
                {
                    CreateMethodRegion();
                }

                ErrorRegion = new ErrorRegion();
                regions.Add(ErrorRegion);
                SourceRegion.Dependants.Add(NamespaceRegion);
                NamespaceRegion.Dependants.Add(ConstructorRegion);
                NamespaceRegion.Dependants.Add(OutputsRegion);
                ConstructorRegion.Dependants.Add(InputArea);
                ConstructorRegion.Dependants.Add(OutputsRegion);
                OutputsRegion.Dependants.Add(ConstructorRegion);
            }
            Regions = regions;

            return regions;
        }

        private void ClearToolRegionErrors()
        {
            if (Regions != null)
            {
                foreach (var toolRegion in Regions)
                {
                    toolRegion.Errors?.Clear();
                }
            }
            MethodsToRunList = null;
            CreateMethodRegion();
        }

        private void CreateMethodRegion()
        {
            var methodRegion = new DotNetMethodRegion(Model, ModelItem, SourceRegion, NamespaceRegion)
            {
                SelectedMethod = null
            };
            methodRegion.SourceChangedAction = () =>
            {
                if (methodRegion.SelectedMethod != null)
                {
                    if (methodRegion.SelectedMethod.ID == Guid.Empty)
                    {
                        methodRegion.SelectedMethod.ID = Guid.NewGuid();
                    }
                    bool hasUnselectedValue = MethodsToRunList.Any(methodToolRegion => methodToolRegion.SelectedMethod == null);
                    if (!hasUnselectedValue)
                    {
                        CreateMethodRegion();
                    }
                }
            };
            methodRegion.PropertyChanged += DotNetMethodRegionOnPropertyChanged;
            methodRegion.ErrorsHandler += (sender, list) =>
            {
                List<ActionableErrorInfo> errorInfos =
                    list.Select(error => new ActionableErrorInfo(new ErrorInfo
                    {
                        ErrorType = ErrorType.Critical,
                        Message = error
                    }, () => { })).ToList();
                UpdateDesignValidationErrors(errorInfos);
                Errors = new List<IActionableErrorInfo>(errorInfos);
            };
            AddToMethodsList(methodRegion);
        }

        private void AddToMethodsList(DotNetMethodRegion methodRegion)
        {
            if (MethodsToRunList == null)
            {
                var pluginActions = ModelItem.GetProperty<ICollection<IPluginAction>>("MethodsToRun");
                if (pluginActions == null || pluginActions.Count == 0)
                {
                    var current = pluginActions;
                    var actions = current ?? new List<IPluginAction>();
                    var collection = new ObservableCollection<IPluginAction>();
                    collection.CollectionChanged += MethodsToRunListOnCollectionChanged;
                    collection.AddRange(actions);
                    MethodsToRunList = new ObservableCollection<IMethodToolRegion<IPluginAction>>();
                }
                else
                {
                    var actions = new ObservableCollection<IPluginAction>();
                    actions.CollectionChanged += MethodsToRunListOnCollectionChanged;
                    actions.AddRange(pluginActions);
                    var regionCollections = BuildRegionsFromActions(pluginActions);
                    MethodsToRunList = regionCollections;
                }
            }
            var methodRegions = new ObservableCollection<IMethodToolRegion<IPluginAction>>();
            methodRegions.AddRange(MethodsToRunList);
            methodRegions.Add(methodRegion);
            MethodsToRunList = methodRegions;
            NamespaceRegion.Dependants.Add(methodRegion);
        }

        public DelegateCommand<DotNetMethodRegion> DeleteActionCommand { get; set; }

        private ObservableCollection<IMethodToolRegion<IPluginAction>> BuildRegionsFromActions(IEnumerable<IPluginAction> pluginActions)
        {
            var regionCollections = new ObservableCollection<IMethodToolRegion<IPluginAction>>();
            foreach (var pluginAction in pluginActions)
            {
                if (pluginAction == null)
                {
                    continue;
                }
                var dotNetMethodRegion = new DotNetMethodRegion(Model, ModelItem, _sourceRegion, _namespaceRegion) { SelectedMethod = pluginAction };
                dotNetMethodRegion.PropertyChanged += DotNetMethodRegionOnPropertyChanged;
                regionCollections.Add(dotNetMethodRegion);
            }
            return regionCollections;
        }

        private void DotNetMethodRegionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ObjectName")
            {
                ModelItem?.SetProperty("MethodsToRun", GetActionsToRun());
            }
        }

        private void MethodsToRunListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AddItemPropertyChangeEvent(e);
            RemoveItemPropertyChangeEvent(e);
        }
        private void AddItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null) return;
            foreach (INotifyPropertyChanged item in args.NewItems)
            {
                if (item != null)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var actionsToRun = GetActionsToRun();
            ModelItem.SetProperty("MethodsToRun", actionsToRun);
        }

        private void RemoveItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems == null) return;
            foreach (INotifyPropertyChanged item in args.OldItems)
            {
                if (item != null)
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        public bool IsConstructorVisible
        {
            get
            {
                return ConstructorRegion?.Constructors?.Count > 0;
            }
        }
        public bool IsActionsVisible => NamespaceRegion?.SelectedNamespace != null;

        public ObservableCollection<IMethodToolRegion<IPluginAction>> MethodsToRunList
        {
            get
            {
                return _methodsToRunList;
            }
            set
            {
                if (value != null)
                {
                    _methodsToRunList = value;
                    var pluginActions = GetActionsToRun();
                    ModelItem.SetProperty("MethodsToRun", pluginActions);
                }
                else
                {
                    _methodsToRunList.Clear();
                    var pluginActions = _methodsToRunList.Where(p => p.SelectedMethod != null).Select(region => region.SelectedMethod).ToList();
                    ModelItem.SetProperty("MethodsToRun", pluginActions.ToList());
                    OnPropertyChanged("MethodsToRunList");
                }
                OnPropertyChanged("MethodsToRunList");
            }
        }

        public ErrorRegion ErrorRegion { get; private set; }

        #endregion

        #region Implementation of IDatabaseServiceViewModel

        public IConstructorRegion<IPluginConstructor> ConstructorRegion
        {
            get
            {
                return _constructorRegion;
            }
            set
            {
                _constructorRegion = value;
                OnPropertyChanged();
            }
        }
        public IMethodToolRegion<IPluginAction> MethodRegion
        {
            get
            {
                return _methodRegion;
            }
            set
            {
                _methodRegion = value;
                OnPropertyChanged();
            }
        }
        public ISourceToolRegion<IPluginSource> SourceRegion
        {
            get
            {
                return _sourceRegion;
            }
            set
            {
                _sourceRegion = value;
                OnPropertyChanged();
            }
        }
        public INamespaceToolRegion<INamespaceItem> NamespaceRegion
        {
            get
            {
                return _namespaceRegion;
            }
            set
            {
                _namespaceRegion = value;
                OnPropertyChanged();
            }
        }
        public IDotNetConstructorInputRegion InputArea
        {
            get
            {
                return _inputArea;
            }
            set
            {
                _inputArea = value;
                OnPropertyChanged();
            }
        }
        public IOutputsToolRegion OutputsRegion
        {
            get
            {
                return _outputsRegion;
            }
            set
            {
                _outputsRegion = value;
                OnPropertyChanged();
            }
        }

        public IPluginService ToModel()
        {
            var pluginServiceDefinition = new PluginServiceDefinition
            {
                Source = SourceRegion.SelectedSource,
                Constructor = ConstructorRegion.SelectedConstructor,
                Namespace = NamespaceRegion.SelectedNamespace,
                Actions = GetActionsToRun()
            };
            var dt = new List<IServiceInput>();
            foreach (var serviceInput in InputArea.Inputs)
            {
                _builder = _builder ?? new ServiceInputBuilder();
                _builder.GetValue(serviceInput.Value, dt);
            }
            return pluginServiceDefinition;
        }

        private List<IPluginAction> GetActionsToRun()
        {
            var pluginActions = MethodsToRunList.Where(region => region.SelectedMethod != null).Select(region => region.SelectedMethod);
            return pluginActions.ToList();
        }

        public void ErrorMessage(Exception exception, bool hasError)
        {
            Errors = new List<IActionableErrorInfo>();
            if (hasError)
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo(new ErrorInfo
                    {
                        ErrorType = ErrorType.Critical,
                        FixData = "",
                        FixType = FixType.None,
                        Message = exception.Message,
                        StackTrace = exception.StackTrace
                    }, () => { })
                };
        }

        public void SetDisplayName(string outputFieldName)
        {
            var index = DisplayName.IndexOf(" -", StringComparison.Ordinal);

            if (index > 0)
            {
                DisplayName = DisplayName.Remove(index);
            }

            var displayName = DisplayName;

            if (!string.IsNullOrEmpty(displayName) && displayName.Contains("Dsf"))
            {
                DisplayName = displayName;
            }
            if (!string.IsNullOrWhiteSpace(outputFieldName))
            {
                DisplayName = displayName + outputFieldName;
            }
        }

        private IPluginServiceModel Model { get; set; }

        #endregion

        public void UpdateMethodInputs()
        {
            ModelItem.SetProperty("MethodsToRun", GetActionsToRun());
        }
    }
}