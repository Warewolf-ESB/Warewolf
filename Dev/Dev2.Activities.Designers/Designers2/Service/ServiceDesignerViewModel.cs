/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Utils;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Utils;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Views;
using Dev2.Threading;
using Warewolf.Resource.Errors;
// ReSharper disable NonLocalizedString
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace Dev2.Activities.Designers2.Service
{
	public class ServiceDesignerViewModel : ActivityDesignerViewModel, IHandle<UpdateResourceMessage>, INotifyPropertyChanged
	{
		private readonly IEventAggregator _eventPublisher;

		private bool _isDisposed;
		private const string DoneText = "Done";
		private const string FixText = "Fix";

		[ExcludeFromCodeCoverage]
		public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel)
			: this(modelItem, rootModel, EnvironmentRepository.Instance, EventPublishers.Aggregator, new AsyncWorker())
		{
		}

		public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel,
										IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher)
			: this(modelItem, rootModel, environmentRepository, eventPublisher, new AsyncWorker())
		{
		}

		public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker)
			: base(modelItem)
		{
			ValidationMemoManager = new ValidationMemoManager(this);
			MappingManager = new MappingManager(this);
			if (modelItem.ItemType != typeof(DsfDatabaseActivity) && modelItem.ItemType != typeof(DsfPluginActivity) && modelItem.ItemType != typeof(DsfWebserviceActivity))
			{
				AddTitleBarEditToggle();
			}
			AddTitleBarMappingToggle();

			VerifyArgument.IsNotNull("rootModel", rootModel);
			VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
			VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
			VerifyArgument.IsNotNull("asyncWorker", asyncWorker);

			_worker = asyncWorker;
			_eventPublisher = eventPublisher;
			eventPublisher.Subscribe(this);
			ButtonDisplayValue = DoneText;

			ShowExampleWorkflowLink = Visibility.Collapsed;
			RootModel = rootModel;
			ValidationMemoManager.DesignValidationErrors = new ObservableCollection<IErrorInfo>();
			FixErrorsCommand = new DelegateCommand(o =>
			{
				ValidationMemoManager.FixErrors();
				IsFixed = IsWorstErrorReadOnly;
			});
			DoneCommand = new DelegateCommand(o => Done());
			DoneCompletedCommand = new DelegateCommand(o => DoneCompleted());

			InitializeDisplayName();

			InitializeImageSource();

			IsAsyncVisible = ActivityTypeToActionTypeConverter.ConvertToActionType(Type) == Common.Interfaces.Core.DynamicServices.enActionType.Workflow;
			OutputMappingEnabled = !RunWorkflowAsync;

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

			ValidationMemoManager.InitializeValidationService(_environment);
			IsLoading = true;
			_worker.Start(() => InitializeResourceModel(_environment), b =>
			  {
				  if (b)
				  {
					  UpdateDesignerAfterResourceLoad(environmentRepository);
				  }
			  });

			ViewComplexObjectsCommand = new RelayCommand(item =>
			{
				ViewJsonObjects(item as IComplexObjectItemModel);
			}, CanViewComplexObjects);
		}

		private void UpdateDesignerAfterResourceLoad(IEnvironmentRepository environmentRepository)
		{
			
			if (!IsDeleted)
			{
				MappingManager.InitializeMappings();
				ValidationMemoManager.InitializeLastValidationMemo(_environment);
				if(IsItemDragged.Instance.IsDragged)
				{
					Expand();
					IsItemDragged.Instance.IsDragged = false;
				}
			}
			var environmentModel = environmentRepository.Get(EnvironmentID);
			if (EnvironmentID == Guid.Empty)
			{
				environmentModel = environmentRepository.ActiveEnvironment;
			}
			if(environmentModel?.Connection?.WebServerUri != null)
			{
				var servUri = new Uri(environmentModel.Connection.WebServerUri.ToString());
				var host = servUri.Host;
				if(!host.Equals(FriendlySourceName, StringComparison.InvariantCultureIgnoreCase))
					FriendlySourceName = host;
			}

			InitializeProperties();
			if(_environment != null)
			{
				_environment.AuthorizationServiceSet += OnEnvironmentOnAuthorizationServiceSet;
				AuthorizationServiceOnPermissionsChanged(null, null);
			}
			IsLoading = false;
		}

		public bool IsLoading
		{
			get
			{
				return _isLoading;
			}
			set
			{
				_isLoading = value;
				OnPropertyChanged("IsLoading");
			}
		}

		private static bool CanViewComplexObjects(Object itemx)
		{
			var item = itemx as IDataListItemModel;
			return item != null && !item.IsComplexObject;
		}

		private static void ViewJsonObjects(IComplexObjectItemModel item)
		{
			if (item != null)
			{
				var window = new JsonObjectsView { Height = 280 };
				var contentPresenter = window.FindChild<TextBox>();
				if (contentPresenter != null)
				{
					var json = item.GetJson();
					contentPresenter.Text = JSONUtils.Format(json);
				}

				window.ShowDialog();
			}
		}

		private void OnEnvironmentOnAuthorizationServiceSet(object sender, EventArgs args)
		{
			if (_environment?.AuthorizationService != null)
			{
				_environment.AuthorizationService.PermissionsChanged += AuthorizationServiceOnPermissionsChanged;
			}
		}

		private void AuthorizationServiceOnPermissionsChanged(object sender, EventArgs eventArgs)
		{
			ValidationMemoManager.RemovePermissionsError();

			var hasNoPermission = HasNoPermission();
			if (hasNoPermission)
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
					FixType = FixType.InvalidPermissions,
					Message = ErrorResource.NoPermissionToExecuteTool
				});
				MappingManager.UpdateLastValidationMemo(memo);
			}
		}

		private bool HasNoPermission()
		{
			var hasNoPermission = ResourceModel != null && ResourceModel.UserPermissions == Permissions.None;
			return hasNoPermission;
		}

		private void DoneCompleted()
		{
			IsFixed = true;
		}

		private void Done()
		{
			if (!IsWorstErrorReadOnly)
			{
				ValidationMemoManager.FixErrors();
			}
		}

		public bool IsFixed
		{
			get { return (bool)GetValue(IsFixedProperty); }
			set { SetValue(IsFixedProperty, value); }
		}

		public static readonly DependencyProperty IsFixedProperty = DependencyProperty.Register("IsFixed", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));


		public ICommand FixErrorsCommand { get; private set; }

		public ICommand DoneCommand { get; private set; }

		private RelayCommand ViewComplexObjectsCommand { get; set; }

		public ICommand DoneCompletedCommand { get; private set; }

		public List<KeyValuePair<string, string>> Properties
		{
			get
			{
				return _properties;
			}
			private set
			{
				_properties = value;
				OnPropertyChanged("Properties");
			}
		}

		public IContextualResourceModel ResourceModel { get; set; }

		public IContextualResourceModel RootModel { get; set; }

		public static readonly DependencyProperty WorstErrorProperty =
			DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(ServiceDesignerViewModel), new PropertyMetadata(ErrorType.None));

		public bool IsWorstErrorReadOnly
		{
			get { return (bool)GetValue(IsWorstErrorReadOnlyProperty); }
			set
			{
				if (value)
				{
					ButtonDisplayValue = DoneText;
				}
				else
				{
					ButtonDisplayValue = FixText;
					IsFixed = false;
				}
				SetValue(IsWorstErrorReadOnlyProperty, value);
			}
		}

		protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
		{
			base.OnToggleCheckedChanged(propertyName, isChecked);

			if (propertyName == ShowLargeProperty.Name)
			{
				if (!isChecked)
				{
					MappingManager.UpdateMappings();
					MappingManager.CheckForRequiredMapping();
				}
			}
		}

		public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
			DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

		public bool IsDeleted
		{
			get { return (bool)GetValue(IsDeletedProperty); }
			set { if (!(bool)GetValue(IsDeletedProperty)) SetValue(IsDeletedProperty, value); }
		}

		public static readonly DependencyProperty IsDeletedProperty =
			DependencyProperty.Register("IsDeleted", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

		public bool IsEditable
		{
			get { return (bool)GetValue(IsEditableProperty); }
			set { SetValue(IsEditableProperty, value); }
		}

		public bool IsAsyncVisible
		{
			get { return (bool)GetValue(IsAsyncVisibleProperty); }
			private set { SetValue(IsAsyncVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsAsyncVisibleProperty =
			DependencyProperty.Register("IsAsyncVisible", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));

		public bool RunWorkflowAsync
		{
			get
			{
				return GetProperty<bool>();
			}
			set
			{
				_runWorkflowAsync = value;
				OutputMappingEnabled = !_runWorkflowAsync;
				SetProperty(value);
			}
		}

		public bool OutputMappingEnabled
		{
			get { return (bool)GetValue(OutputMappingEnabledProperty); }
			private set { SetValue(OutputMappingEnabledProperty, value); }
		}

		public static readonly DependencyProperty OutputMappingEnabledProperty =
			DependencyProperty.Register("OutputMappingEnabled", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));

		public static readonly DependencyProperty IsEditableProperty =
			DependencyProperty.Register("IsEditable", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

		public string ImageSource
		{
			get { return (string)GetValue(ImageSourceProperty); }
			private set { SetValue(ImageSourceProperty, value); }
		}

		public static readonly DependencyProperty ImageSourceProperty =
			DependencyProperty.Register("ImageSource", typeof(string), typeof(ServiceDesignerViewModel), new PropertyMetadata(null));

		public bool ShowParent
		{
			get { return (bool)GetValue(ShowParentProperty); }
			set { SetValue(ShowParentProperty, value); }
		}

		public static readonly DependencyProperty ShowParentProperty =
			DependencyProperty.Register("ShowParent", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false, OnShowParentChanged));


		static void OnShowParentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var viewModel = (ServiceDesignerViewModel)d;
			var showParent = (bool)e.NewValue;
			if (showParent)
			{
				viewModel.DoShowParent();
			}
		}

		string ServiceUri => GetProperty<string>();
		public string ServiceName => GetProperty<string>();
		string ActionName => GetProperty<string>();


		private string FriendlySourceName
		{
			get
			{
				var friendlySourceName = GetProperty<string>();

				return friendlySourceName;
			}
			set
			{
				SetProperty(value);
				OnPropertyChanged("FriendlySourceName");

			}
		}

		public IDataMappingViewModel DataMappingViewModel => MappingManager.DataMappingViewModel;

		public string Type => GetProperty<string>();
		// ReSharper disable InconsistentNaming
		Guid EnvironmentID => GetProperty<Guid>();

		public Guid ResourceID => GetProperty<Guid>();
		public Guid UniqueID => GetProperty<Guid>();
		public string OutputMapping { get { return GetProperty<string>(); } set { SetProperty(value); } }
		public string InputMapping { get { return GetProperty<string>(); } set { SetProperty(value); } }

		public string ButtonDisplayValue
		{
			get { return (string)GetValue(ButtonDisplayValueProperty); }
			set { SetValue(ButtonDisplayValueProperty, value); }
		}

		public static readonly DependencyProperty ButtonDisplayValueProperty = DependencyProperty.Register("ButtonDisplayValue", typeof(string), typeof(ServiceDesignerViewModel), new PropertyMetadata(default(string)));
		readonly IEnvironmentModel _environment;
		bool _runWorkflowAsync;
		private readonly IAsyncWorker _worker;
		private bool _isLoading;
		private List<KeyValuePair<string, string>> _properties;

		public override void Validate()
		{
			Errors = new List<IActionableErrorInfo>();
			if (HasNoPermission())
			{
				var errorInfos = ValidationMemoManager.DesignValidationErrors.Where(info => info.FixType == FixType.InvalidPermissions);
				Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(errorInfos.ToList()[0], () => { }) };
			}
			else
			{
				ValidationMemoManager.RemovePermissionsError();
			}
		}

		void InitializeDisplayName()
		{
			var serviceName = ServiceName;
			if (!string.IsNullOrEmpty(serviceName))
			{
				var displayName = DisplayName;
				if (string.IsNullOrEmpty(displayName))
				{
					DisplayName = serviceName;
				}
			}
		}

		bool InitializeResourceModel(IEnvironmentModel environmentModel)
		{
			if (environmentModel != null)
			{
				if (!environmentModel.IsLocalHost && environmentModel.IsConnected)
				{
					var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(ResourceID);
					if (contextualResourceModel != null)
					{
						ResourceModel = contextualResourceModel;
					}
					else
					{
						ResourceModel = ResourceModelFactory.CreateResourceModel(environmentModel);
						ResourceModel.Inputs = InputMapping;
						ResourceModel.Outputs = OutputMapping;
						environmentModel.Connection.Verify(ValidationMemoManager.UpdateLastValidationMemoWithOfflineError, false);
						environmentModel.ResourcesLoaded += OnEnvironmentModel_ResourcesLoaded;
					}
					return true;
				}
				var init = InitializeResourceModelFromRemoteServer(environmentModel);
				return init;

			}
			return true;
		}

		// ReSharper disable InconsistentNaming
		void OnEnvironmentModel_ResourcesLoaded(object sender, ResourcesLoadedEventArgs e)
		// ReSharper restore InconsistentNaming
		{
			_worker.Start(() => GetResourceModel(e.Model), () => MappingManager.CheckVersions(this));
			e.Model.ResourcesLoaded -= OnEnvironmentModel_ResourcesLoaded;
		}

		private void GetResourceModel(IEnvironmentModel environmentModel)
		{
			var resourceId = ResourceID;

			if (resourceId != Guid.Empty)
			{
				NewModel = environmentModel.ResourceRepository.FindSingle(c => c.ID == resourceId, true) as IContextualResourceModel;

			}
		}

		public IContextualResourceModel NewModel { get; set; }

		private bool InitializeResourceModelFromRemoteServer(IEnvironmentModel environmentModel)
		{
			var resourceId = ResourceID;
			if (!environmentModel.IsConnected)
			{
				environmentModel.Connection.Verify(ValidationMemoManager.UpdateLastValidationMemoWithOfflineError);
			}
			if (environmentModel.IsConnected)
			{
				if (resourceId != Guid.Empty)
				{
					ResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);

				}
			}
			if (!CheckSourceMissing())
			{
				return false;
			}
			return true;
		}

		public bool CheckSourceMissing()
		{
			if (ResourceModel != null && _environment != null)
			{
				var resourceModel = ResourceModel;
				string srcId;
				var workflowXml = resourceModel?.WorkflowXaml;
				try
				{
					var xe = workflowXml?.Replace("&", "&amp;").ToXElement();
					srcId = xe?.AttributeSafe("SourceID");
				}
				catch (XmlException xe)
				{
					Dev2Logger.Error(xe);
					srcId = workflowXml.ExtractXmlAttributeFromUnsafeXml("SourceID=\"");
				}

				Guid sourceId;
				if (Guid.TryParse(srcId, out sourceId))
				{
					SourceId = sourceId;
					var sourceResource = _environment.ResourceRepository.LoadContextualResourceModel(sourceId);
					if (sourceResource == null)
					{
						ValidationMemoManager.UpdateLastValidationMemoWithSourceNotFoundError();
						return false;
					}
				}
			}

			return true;
		}

		public Guid SourceId { get; set; }

		void InitializeProperties()
		{
			_properties = new List<KeyValuePair<string, string>>();
			AddProperty("Source :", FriendlySourceName);
			AddProperty("Type :", Type);
			AddProperty("Procedure :", ActionName);
			Properties = _properties;
		}

		void AddProperty(string key, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				_properties.Add(new KeyValuePair<string, string>(key, value));
			}
		}

		void InitializeImageSource()
		{
			Common.Interfaces.Core.DynamicServices.enActionType actionType = ActivityTypeToActionTypeConverter.ConvertToActionType(Type);
			ImageSource = GetIconPath(actionType);
		}

		public string ResourceType
		{
			get;
			set;
		}
		public MappingManager MappingManager { get; }
		public ValidationMemoManager ValidationMemoManager { get; }

		string GetIconPath(Common.Interfaces.Core.DynamicServices.enActionType actionType)
		{
			switch (actionType)
			{
				case Common.Interfaces.Core.DynamicServices.enActionType.Workflow:
					if (string.IsNullOrEmpty(ServiceUri))
					{
						ResourceType = "WorkflowService";
						return "Workflow-32";
					}
					ResourceType = "Server";
					return "RemoteWarewolf-32";

				case Common.Interfaces.Core.DynamicServices.enActionType.RemoteService:
					ResourceType = "Server";
					return "RemoteWarewolf-32";

			}
			return "ToolService-32";
		}

		void AddTitleBarEditToggle()
		{
			// ReSharper disable RedundantArgumentName
			var toggle = ActivityDesignerToggle.Create("ServicePropertyEdit", "Edit", "ServicePropertyEdit", "Edit", "ShowParentToggle",
				autoReset: true,
				target: this,
				dp: ShowParentProperty
				);
			// ReSharper restore RedundantArgumentName
			TitleBarToggles.Add(toggle);
		}

		void AddTitleBarMappingToggle()
		{
			HasLargeView = true;
		}

		void DoShowParent()
		{
			if (!IsDeleted)
			{
				_eventPublisher.Publish(new EditActivityMessage(ModelItem, EnvironmentID));
			}
		}

		public void Handle(UpdateResourceMessage message)
		{
			if (message?.ResourceModel != null)
			{
				if (SourceId != Guid.Empty && SourceId == message.ResourceModel.ID)
				{
					IErrorInfo sourceNotAvailableMessage = ValidationMemoManager.DesignValidationErrors.FirstOrDefault(info => info.Message == ValidationMemoManager.SourceNotFoundMessage);
					if (sourceNotAvailableMessage != null)
					{
						ValidationMemoManager.RemoveError(sourceNotAvailableMessage);
						ValidationMemoManager.UpdateWorstError();
						MappingManager.InitializeMappings();
						MappingManager.UpdateMappings();
					}
				}
			}
		}

		~ServiceDesignerViewModel()
		{
			Dispose(false);
		}

		protected override void OnDispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
			base.OnDispose();
		}

		void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					ValidationMemoManager.ValidationService?.Dispose();
					if (_environment != null)
					{
						_environment.AuthorizationServiceSet -= OnEnvironmentOnAuthorizationServiceSet;
					}
				}
				_isDisposed = true;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName = null)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public override void UpdateHelpDescriptor(string helpText)
		{
			var mainViewModel = CustomContainer.Get<IMainViewModel>();
			mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
		}
	}
}





