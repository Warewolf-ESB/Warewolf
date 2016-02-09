using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;
using Warewolf.Core;

namespace Dev2.Activities.Designers2.Web_Service_Get
{
    public class WebServiceGetViewModel :CustomToolWithRegionBase,IWebServiceGetViewModel
    {
        private IOutputsToolRegion _outputs;
        private IWebGetInputArea _inputArea;
        private ISourceToolRegion<IWebServiceSource> _source;
        private IEventAggregator _eventPublisher;
        private string _resourceType;
        private string _imageSource;

#pragma warning disable 169
        private IEnvironmentModel _environment;
#pragma warning restore 169
#pragma warning disable 169
        private IErrorInfo _worstDesignError;
#pragma warning restore 169
        const string DoneText = "Done";
        const string FixText = "Fix";
        // ReSharper disable UnusedMember.Local
        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;
    
        readonly string _sourceNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotSelected;
        readonly string _methodNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.PluginServiceMethodNotSelected;
        readonly string _serviceExecuteOnline = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteOnline;
        readonly string _serviceExecuteLoginPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteLoginPermission;
        readonly string _serviceExecuteViewPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteViewPermission;
        // ReSharper restore UnusedMember.Local
        public WebServiceGetViewModel(ModelItem modelItem, IContextualResourceModel rootModel)
            : base(modelItem)
        {
            LabelWidth = 75;
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var pluginServiceModel = CustomContainer.CreateInstance<IWebServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = pluginServiceModel;
            BuildRegions();
            InitialiseViewModel(rootModel, EnvironmentRepository.Instance, EventPublishers.Aggregator, new AsyncWorker(), new ManageWebServiceInputViewModel(), pluginServiceModel);

        }

        public WebServiceGetViewModel(ModelItem modelItem, IList<IToolRegion> regions)
            : base(modelItem, regions)
        {

        }

        public WebServiceGetViewModel(ModelItem modelItem, Action<Type> showExampleWorkflow, IList<IToolRegion> regions)
            : base(modelItem, showExampleWorkflow, regions)
        {
        }

        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
          //  Regions.SelectMany(a => a.Errors);
        }

        private void InitialiseViewModel(IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IManageWebServiceInputViewModel manageServiceInputViewModel, IWebServiceModel webServiceModel)
        {
            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            ManageServiceInputViewModel = manageServiceInputViewModel;
            _eventPublisher = eventPublisher;
            eventPublisher.Subscribe(this);
            //ButtonDisplayValue = DoneText;

            //LabelWidth = 70;
            ReCalculateHeight();

            TestComplete = false;
            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            ShowExampleWorkflowLink = Visibility.Collapsed;
            RootModel = rootModel;
            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(o =>
            {
                FixErrors();
            });

            InitializeDisplayName();

            InitializeImageSource();

            //Outputs.OutputMappingEnabled = true;

            //// When the active environment is not local, we need to get smart around this piece of logic.
            //// It is very possible we are treating a remote active as local since we cannot logically assign 
            //// an environment id when this is the case as it will fail with source not found since the remote 
            //// does not contain localhost's connections ;)
            var activeEnvironment = environmentRepository.ActiveEnvironment;
            if (EnvironmentID == Guid.Empty && !activeEnvironment.IsLocalHostCheck())
            {
                _environment = activeEnvironment;
            }
            else
            {
                var environment = environmentRepository.FindSingle(c => c.ID == EnvironmentID);
                if (environment == null)
                {
                    IList<IEnvironmentModel> environments = EnvironmentRepository.Instance.LookupEnvironments(activeEnvironment);
                    environment = environments.FirstOrDefault(model => model.ID == EnvironmentID);
                }
                _environment = environment;
            }

            //TestInputCommand = new DelegateCommand(() =>
            //{
            //    TestComplete = true;
            //    Outputs.IsVisible = true;
            //    Outputs.Outputs.Add(new ServiceOutputMapping("a", "b", "c"));
            //}, CanTestProcedure);
            TestInputCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(TestAction, CanTestProcedure);
            //InitializeValidationService(_environment);
            //InitializeLastValidationMemo(_environment);
            //ManageServiceInputViewModel = manageServiceInputViewModel;
            //if (_environment != null)
            //{
            //    _isInitializing = true;


            //    Model = webServiceModel;


             
               

            //}
            //InitializeProperties();
            //if (IsItemDragged.Instance.IsDragged)
            //{
            //    Expand();
            //    IsItemDragged.Instance.IsDragged = false;
            //}

            //if (_environment != null)
            //{
            //    _environment.AuthorizationServiceSet += OnEnvironmentOnAuthorizationServiceSet;
            //    AuthorizationServiceOnPermissionsChanged(null, null);
            //}
            //InitializeResourceModel(_environment);

            //_isInitializing = false;

        }

        //void InitializeResourceModel(IEnvironmentModel environmentModel)
        //{
        //    if (environmentModel != null)
        //    {
        //        if (!environmentModel.IsLocalHost && !environmentModel.HasLoadedResources)
        //        {
        //            environmentModel.Connection.Verify(UpdateLastValidationMemoWithOfflineError, false);
        //            environmentModel.ResourcesLoaded += OnEnvironmentModel_ResourcesLoaded;
        //        }
        //        InitializeResourceModelSync(environmentModel);
        //    }
        //}

        // ReSharper disable InconsistentNaming
        //void OnEnvironmentModel_ResourcesLoaded(object sender, ResourcesLoadedEventArgs e)
        //// ReSharper restore InconsistentNaming
        //{
        //}






       

     



        public List<KeyValuePair<string, string>> Properties { get; private set; }
        void InitializeProperties()
        {
            Properties = new List<KeyValuePair<string, string>>();
            AddProperty("Source :", Source.SelectedSource.Name);
            AddProperty("Type :", Type);
            AddProperty("Url :", Source.SelectedSource.HostName);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public DelegateCommand NewSourceCommand
        {
            get
            {
                return _newSourceCommand;
            }
            set
            {
                _newSourceCommand = value;
            }
        }

        public IManageWebServiceInputViewModel ManageServiceInputViewModel
        {
            get
            {
                return _manageServiceInputViewModel;
            }
            set
            {
                _manageServiceInputViewModel = value;
            }
        }

        private bool CanTestProcedure()
        {
            return Source.SelectedSource != null;
        }

        public IErrorInfo NoError
        {
            get
            {
                return _noError;
            }
        }

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
    DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(WebServiceGetViewModel), new PropertyMetadata(false));


        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }
        public static readonly DependencyProperty WorstErrorProperty =
        DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(WebServiceGetViewModel), new PropertyMetadata(ErrorType.None));
        private IManageWebServiceInputViewModel _manageServiceInputViewModel;
#pragma warning disable 414
#pragma warning disable 169
        private bool _isInitializing;
#pragma warning restore 169
#pragma warning restore 414
        private DelegateCommand _newSourceCommand;
#pragma warning disable 649
        private IErrorInfo _noError;
        private bool _testComplete;
        private bool _testSuccessful;
#pragma warning restore 649

        
        void UpdateLastValidationMemo(DesignValidationMemo memo, bool checkSource = true)
        {
            LastValidationMemo = memo;

           // CheckIsDeleted(memo);

            //UpdateDesignValidationErrors(memo.Errors.Where(info => info.InstanceID == UniqueID && info.ErrorType != ErrorType.None));
            //if (SourceId == Guid.Empty)
            //{
            //    if (checkSource && CheckSourceMissing())
            //    {
            //    }
            //}
        }

        public DesignValidationMemo LastValidationMemo { get; private set; }

        public DelegateCommand TestInputCommand { get; set; }

        public string Type { get { return GetProperty<string>(); } }
        // ReSharper disable InconsistentNaming
        Guid EnvironmentID { get { return GetProperty<Guid>(); } }
        Guid ResourceID { get { return GetProperty<Guid>(); } }
        Guid UniqueID { get { return GetProperty<Guid>(); } }



        private void FixErrors()
        {
        }

        void InitializeImageSource()
        {
            ImageSource = GetIconPath();
        }

        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        string GetIconPath()
        {
            ResourceType = Common.Interfaces.Data.ResourceType.PluginService.ToString();
            return "PluginService-32";
        }

        public string ResourceType
        {
            get
            {
                return _resourceType;
            }
            set
            {
                _resourceType = value;
            }
        }

        void InitializeDisplayName()
        {
            var serviceName = ServiceName;
            if (!string.IsNullOrEmpty(serviceName))
            {
                var displayName = DisplayName;
                if (!string.IsNullOrEmpty(displayName) && displayName.Contains("Dsf"))
                {
                    DisplayName = serviceName;
                }
            }
        }
        public string ServiceName { get { return GetProperty<string>(); } }
        public Runtime.Configuration.ViewModels.Base.DelegateCommand FixErrorsCommand { get; set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }

        public IContextualResourceModel RootModel { get; set; }

        public bool TestComplete
        {
            get
            {
                return _testComplete;
            }
            set
            {
                _testComplete = value;
                OnPropertyChanged();
            }
        }

        public int LabelWidth { get; set; }

        public string ButtonDisplayValue { get; set; }

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion

        #region Overrides of CustomToolWithRegionBase

        protected override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();
            if (Source == null)
            {
                Source = new WebSourceRegion(Model, ModelItem);
                regions.Add(Source);
                InputArea = new WebGetInputRegion(ModelItem,Source);
                regions.Add(InputArea);
                Outputs = new OutputsRegion(ModelItem);
                regions.Add(Outputs);
                regions.Add(new ErrorRegion());
                Source.Dependants.Add(InputArea);
                Source.Dependants.Add(Outputs);
            }
            Regions = regions;
            foreach(var toolRegion in regions)
            {
                toolRegion.HeightChanged += toolRegion_HeightChanged;
            }    
            ReCalculateHeight();
            return regions;
        }

        void toolRegion_HeightChanged(object sender, IToolRegion args)
        {
           ReCalculateHeight();
            TestInputCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Implementation of IWebServiceGetViewModel

        public IOutputsToolRegion Outputs
        {
            get
            {
                return _outputs;
            }
            set
            {
                _outputs = value;
                OnPropertyChanged();
            }
        }
        public IWebGetInputArea InputArea
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
        public ISourceToolRegion<IWebServiceSource> Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                OnPropertyChanged();
            }
        }
        public void TestAction()
        {
            try
            {
                Errors = new List<IActionableErrorInfo>();
                var service = ToModel();
                ManageServiceInputViewModel.Model = service;
                ManageServiceInputViewModel.Inputs = service.Inputs;
                ManageServiceInputViewModel.TestResults = null;
                ManageServiceInputViewModel.TestAction = () =>
                {
                    ManageServiceInputViewModel.IsTesting = true;
                    try
                    {
                        ManageServiceInputViewModel.TestResults = Model.TestService(ManageServiceInputViewModel.Model);
                        var serializer = new Dev2JsonSerializer();

                        var responseService = serializer.Deserialize<RecordsetListWrapper>(ManageServiceInputViewModel.TestResults);
                        if (responseService.RecordsetList.Any(recordset => recordset.HasErrors))
                        {
                            var errorMessage = string.Join(Environment.NewLine, responseService.RecordsetList.Select(recordset => recordset.ErrorMessage));
                            throw new Exception(errorMessage);
                        }

                        ManageServiceInputViewModel.Description = responseService.Description;
                        // ReSharper disable MaximumChainedReferences
                        var outputMapping = responseService.RecordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                        {
                            Outputs.RecordsetName = recordset.Name;
                            var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name);
                            return serviceOutputMapping;
                        }).Cast<IServiceOutputMapping>().ToList();
                        // ReSharper restore MaximumChainedReferences

                        ManageServiceInputViewModel.OutputMappings = outputMapping;
                        if (ManageServiceInputViewModel.TestResults != null)
                        {
                            ManageServiceInputViewModel.TestResultsAvailable = ManageServiceInputViewModel.TestResults != null;
                            TestSuccessful = true;
                            ManageServiceInputViewModel.IsTesting = false;
                        }
                    }
                    catch(JsonSerializationException )
                    {
                        ManageServiceInputViewModel.OutputMappings = new List<IServiceOutputMapping>{new ServiceOutputMapping("Result","[[Result]]","")};
                    }
                    catch (Exception)
                    {

                        TestComplete = false;
                       // ErrorMessage(e);
                        ManageServiceInputViewModel.IsTesting = false;
                        ManageServiceInputViewModel.CloseCommand.Execute(null);
                    }
                };
                ManageServiceInputViewModel.OkAction = () =>
                {
                    Outputs.Outputs = new ObservableCollection<IServiceOutputMapping>(ManageServiceInputViewModel.OutputMappings);
                    Outputs.Description = ManageServiceInputViewModel.Description;
                };
                ManageServiceInputViewModel.ShowView();
                if (ManageServiceInputViewModel.OkSelected)
                {
                   ValidateTestComplete();
                }
                ReCalculateHeight();
     
            }
            catch (Exception)
            {
                //ErrorMessage(e);
            }
        }

        private void ValidateTestComplete()
        {
            TestComplete = true;
        }

        public bool TestSuccessful
        {
            get
            {
                return _testSuccessful;
            }
            set
            {
                _testSuccessful = value;
                OnPropertyChanged();
            }
        }

        private IWebService ToModel()
        {
            return new WebServiceDefinition
            {
                Inputs = InputsFromModel() ,
                OutputMappings = new List<IServiceOutputMapping>(),
                Source = Source.SelectedSource,
                Name = "",
                Path = "",
                Id = Guid.NewGuid(),
                PostData = "",
                Headers = InputArea.Headers.Select(value => new NameValue { Name =value.Name, Value = value.Value }).ToList(),
                QueryString = InputArea.QueryString,
                SourceUrl = "",//Source.SelectedSource.HostName,
                RequestUrl = Source.SelectedSource.HostName,
                Response = ""
            };
        }

        private IList<IServiceInput> InputsFromModel()
        {
            var dt = new List<IServiceInput>();
            string s = InputArea.QueryString;
            GetValue(s, dt);
            foreach(var nameValue in InputArea.Headers)
            {
                GetValue(nameValue.Name, dt);
                GetValue(nameValue.Value, dt);
            }
            return dt;
        }

        private static void GetValue(string s, List<IServiceInput> dt)
        {
            var exp = WarewolfDataEvaluationCommon.ParseLanguageExpressionWithoutUpdate(s);
            if(exp.IsComplexExpression)
            {
                var item = ((LanguageAST.LanguageExpression.ComplexExpression)exp).Item;
                var vals = item.Where(a => a.IsRecordSetExpression || a.IsScalarExpression).Select(WarewolfDataEvaluationCommon.LanguageExpressionToString);
                dt.AddRange(vals.Select(a => new ServiceInput(a, "")));
            }
            if (exp.IsScalarExpression)
            {

                 dt.Add(new ServiceInput(s, ""));
            }
            if (exp.IsRecordSetExpression)
            {

                dt.Add(new ServiceInput(s, ""));
            }
        }

        public IWebServiceModel Model { get; set; }

        #endregion
    }



}
