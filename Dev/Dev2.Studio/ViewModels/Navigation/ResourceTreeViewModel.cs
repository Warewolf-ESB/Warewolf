#region

using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.ExtMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;

#endregion

namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// A treenode representing a resource (either normal or wizard)
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public class ResourceTreeViewModel : AbstractTreeViewModel<IContextualResourceModel>
    {
        #region private fields

        private string _activityFullName;
        private RelayCommand _buildCommand;
        private RelayCommand _createWizardCommand;
        private IContextualResourceModel _dataContext;
        private RelayCommand _debugCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _editCommand;
        private RelayCommand _editWizardCommand;
        private RelayCommand _helpCommand;
        private RelayCommand _manualEditCommand;
        private RelayCommand _runCommand;
        private RelayCommand _showDependenciesCommand;
        private RelayCommand _showPropertiesCommand;
        private RelayCommand _duplicateCommand;
        private RelayCommand _moveRenameCommand;

        #endregion private fields

        #region ctors + init
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceTreeViewModel" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="dataContext">The data context.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ResourceTreeViewModel(ITreeNode parent,
                                     IContextualResourceModel dataContext)
            : this(parent, dataContext, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceTreeViewModel" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="dataContext">The data context.</param>
        /// <param name="activityFullName">Full name of the activity.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ResourceTreeViewModel(ITreeNode parent,
                                     IContextualResourceModel dataContext,
                                     string activityFullName)
            : base(null)
        {
            DataContext = dataContext;
            ActivityFullName = activityFullName;
            if (parent != null)
            {
                parent.Add(this);
            }
        }

        #endregion ctors + init

        #region public properties
        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <value>
        /// The icon path.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string IconPath
        {
            get
            {
                return string.IsNullOrEmpty(DataContext.IconPath)
                           ? DataContext.ResourceType.GetIconLocation()
                           : DataContext.IconPath;
            }
        }

        public override int ChildrenCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override string DisplayName
        {
            get
            {
                return DataContext != null ? DataContext.ResourceName : String.Empty;
            }
        }

        public override void VerifyCheckState()
        {
        }

        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        /// <value>
        /// The data context.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override sealed IContextualResourceModel DataContext
        {
            get { return _dataContext; }
            set
            {
                _dataContext = value;
                NotifyOfPropertyChange(() => DataContext);
                NotifyOfPropertyChange(() => IconPath);
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        /// <summary>
        /// Gets or sets the full name of the activity.
        /// </summary>
        /// <value>
        /// The full name of the activity.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public string ActivityFullName
        {
            get { return _activityFullName; }
            set
            {
                if (_activityFullName == value) return;

                _activityFullName = value;
                NotifyOfPropertyChange(() => ActivityFullName);
            }
        }

        /// <summary>
        /// Gets the environment model by walking the tree till it finds it
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        /// <exception cref="System.InvalidOperationException">Cant set it directly</exception>
        public override IEnvironmentModel EnvironmentModel
        {
            get { return TreeParent.EnvironmentModel; }
            protected set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Getsa value indicating whether thus instance can select dependencies
        /// </summary>
        public override bool CanSelectDependencies
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has executable commands.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has executable commands; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool HasExecutableCommands
        {
            get
            {
                return base.HasExecutableCommands ||
                       CanBuild || CanDebug || CanEdit ||
                       CanManualEdit || CanRun || CanDelete ||
                       CanEditWizard || CanHelp || CanShowDependencies ||
                       CanShowProperties || CanCreateWizard;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can build.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can build; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanBuild
        {
            //get { return DataContext != null; }
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can debug.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can debug; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanDebug
        {
            //get
            //{
            //    return DataContext != null
            //           && DataContext.ResourceType == ResourceType.WorkflowService;
            //}
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be edited.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanEdit
        {
            get
            {
                return DataContext != null &&
                       (DataContext.ResourceType == ResourceType.WorkflowService ||
                        DataContext.ResourceType == ResourceType.Service ||
                        DataContext.ResourceType == ResourceType.Source);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be manually edited.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can manual edit; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanManualEdit
        {
            get
            {
                return DataContext != null &&
                       (DataContext.ResourceType == ResourceType.WorkflowService ||
                        DataContext.ResourceType == ResourceType.Service ||
                        DataContext.ResourceType == ResourceType.Source) &&
                       WizardEngine.IsResourceWizard(DataContext);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can ran.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can run; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanRun
        {
            //get
            //{
            //    return DataContext != null && (
            //           DataContext.ResourceType == ResourceType.WorkflowService ||
            //           DataContext.ResourceType == ResourceType.Service);
            //}
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be deleted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can delete; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanDelete
        {
            get
            {
                return DataContext != null &&
                       (DataContext.ResourceType == ResourceType.WorkflowService ||
                        DataContext.ResourceType == ResourceType.Service ||
                        DataContext.ResourceType == ResourceType.Source);
            }
        }

        /// <summary>
        /// Gets a value indicating whether tehre is help available for this instance
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can help; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanHelp
        {
            //get { return DataContext != null; }
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can show dependencies.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can show dependencies; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanShowDependencies
        {
            get { return DataContext != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can show properties.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can show properties; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanShowProperties
        {
            //get { return DataContext != null; }
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a qizard can be created for this instance
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can create wizard; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanCreateWizard
        {
            //get
            //{
            //    return DataContext != null && WizardEngine != null &&
            //           !WizardEngine.IsWizard(DataContext) &&
            //           WizardEngine.GetWizard(DataContext) == null &&
            //           (DataContext.ResourceType == ResourceType.Service ||
            //            DataContext.ResourceType == ResourceType.WorkflowService);
            //}
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a wizard can be edited for this instance
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can edit wizard; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override bool CanEditWizard
        {
            //get
            //{
            //    return DataContext != null && WizardEngine != null &&
            //           !WizardEngine.IsWizard(DataContext) &&
            //           WizardEngine.GetWizard(DataContext) != null &&
            //           (DataContext.ResourceType == ResourceType.Service ||
            //            DataContext.ResourceType == ResourceType.WorkflowService);
            //}
            //2013.05.20: Ashley Lewis for PBI 8858 context menu cleanup
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be duplicated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can duplicate; otherwise, <c>false</c>.
        /// </value>
        /// <author>Ashley.lewis</author>
        /// <date>2013/05/20</date>
        public override bool CanDuplicate
        {
            get
            {
                return DataContext != null &&
                       (DataContext.ResourceType == ResourceType.WorkflowService ||
                        DataContext.ResourceType == ResourceType.Service ||
                        DataContext.ResourceType == ResourceType.Source);
            }
        }

        public override bool HasNewWorkflowMenu
        {
            get
            {
                //return DataContext != null &&
                //       (DataContext.ResourceType == ResourceType.WorkflowService);
                return false;
            }
        }

        public override bool HasNewServiceMenu
        {
            get
            {
                return false; //DataContext != null &&
                        //(DataContext.ResourceType == ResourceType.Service);
            }
        }

        public override bool HasNewSourceMenu
        {
            get
            {
                return false;// DataContext != null &&
                        //(DataContext.ResourceType == ResourceType.Source);
            }
        }

        public override bool CanMoveRename
        {
            get
            {
                return DataContext != null &&
                       (DataContext.ResourceType == ResourceType.WorkflowService ||
                       DataContext.ResourceType == ResourceType.Service ||
                       DataContext.ResourceType == ResourceType.Source);
            }
        }

        public override string NewWorkflowTitle
        {
            get
            {              
                return "New Workflow in " + DataContext.Category.ToUpper() +"   (Ctrl+W)";
            }
        }

        public override string NewServiceTitle
        {
            get
            {
                return "New Service in " + DataContext.Category.ToUpper();
            }
        }

        public override string AddSourceTitle
        {
            get
            {
                return "Add Source to " + DataContext.Category.ToUpper();
            }
        }

        public override bool CanDeploy
        {
            get
            {
                return DataContext.ResourceType == ResourceType.WorkflowService || DataContext.ResourceType == ResourceType.Source || DataContext.ResourceType == ResourceType.Service;
            }
        }

        #endregion public properties

        #region commands

        /// <summary>
        /// Gets the debug command.
        /// </summary>
        /// <value>
        /// The debug command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand DebugCommand
        {
            get
            {
                return _debugCommand ?? (_debugCommand =
                    new RelayCommand(param => Debug(), param => CanDebug));
            }
        }

        /// <summary>
        /// Gets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand =
                    new RelayCommand(p => Delete(), o => CanDelete));
            }
        }

        /// <summary>
        /// Gets the build command.
        /// </summary>
        /// <value>
        /// The build command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand BuildCommand
        {
            get
            {
                return _buildCommand ?? (_buildCommand = 
                    new RelayCommand(param => Build(), param => CanBuild));
            }
        }

        /// <summary>
        /// Gets the edit command.
        /// </summary>
        /// <value>
        /// The edit command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = 
                    new RelayCommand(param => Edit(), param => CanEdit));
            }
        }

        /// <summary>
        /// Gets the manual edit command.
        /// </summary>
        /// <value>
        /// The manual edit command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand ManualEditCommand
        {
            get
            {
                return _manualEditCommand ?? (_manualEditCommand = 
                    new RelayCommand(param => ManualEdit(), param => CanManualEdit));
            }
        }

        /// <summary>
        /// Gets the run command.
        /// </summary>
        /// <value>
        /// The run command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand RunCommand
        {
            get
            {
                return _runCommand ?? (_runCommand = 
                    new RelayCommand(param => Run(), param => CanRun));
            }
        }

        /// <summary>
        /// Gets the help command.
        /// </summary>
        /// <value>
        /// The help command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand HelpCommand
        {
            get
            {
                return _helpCommand ?? (_helpCommand = 
                    new RelayCommand(param => ShowHelp(), param => CanHelp));
            }
        }

        /// <summary>
        /// Gets the show dependencies command.
        /// </summary>
        /// <value>
        /// The show dependencies command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand ShowDependenciesCommand
        {
            get
            {
                return _showDependenciesCommand ?? (_showDependenciesCommand = 
                    new RelayCommand(param => ShowDependencies(), param => CanShowDependencies));
            }
        }

        /// <summary>
        /// Gets the show properties command.
        /// </summary>
        /// <value>
        /// The show properties command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand ShowPropertiesCommand
        {
            get
            {
                return _showPropertiesCommand ?? (_showPropertiesCommand = 
                    new RelayCommand(param => ShowProperties(), param => CanShowProperties));
            }
        }

        /// <summary>
        /// Gets the create wizard command.
        /// </summary>
        /// <value>
        /// The create wizard command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand CreateWizardCommand
        {
            get
            {
                return _createWizardCommand ?? (_createWizardCommand = 
                    new RelayCommand(param => CreateWizard(), param => CanCreateWizard));
            }
        }


        /// <summary>
        /// Gets the edit wizard command.
        /// </summary>
        /// <value>
        /// The edit wizard command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ICommand EditWizardCommand
        {
            get
            {
                return _editWizardCommand ?? (_editWizardCommand = 
                    new RelayCommand(param => EditWizard(), param => CanEditWizard));
            }
        }

        /// <summary>
        /// Gets the duplicate command.
        /// </summary>
        /// <value>
        /// The duplicate command.
        /// </value>
        /// <author>Ashley.Lewis</author>
        /// <date>2013/05/20</date>
        public override ICommand DuplicateCommand
        {
            get
            {
                return _duplicateCommand ?? (_duplicateCommand =
                    new RelayCommand(Duplicate));
            }
        }

        public override ICommand NewResourceCommand
        {
            get
            {
                return new RelayCommand(ShowNewResourceWizard);
            }
        }

        void ShowNewResourceWizard(object obj)
        {
            //TODO: Implement in PBI 9501
            var resourceModel = ResourceModelFactory.CreateResourceModel(EnvironmentModel, DataContext.ResourceType, string.Empty, obj.ToString());
            resourceModel.Category = TreeParent.DisplayName;
            EventAggregator.Publish(new ShowEditResourceWizardMessage(resourceModel));
        }

        /// <summary>
        /// Gets the duplicate command.
        /// </summary>
        /// <value>
        /// The duplicate command.
        /// </value>
        /// <author>Ashley.Lewis</author>
        /// <date>2013/05/20</date>
        public override ICommand MoveRenameCommand
        {
            get
            {
                return _moveRenameCommand ?? (_moveRenameCommand =
                    new RelayCommand(MoveRename));
            }
        }

        #endregion commands

        #region public methods
        public override void SetFilter(string filterText, bool updateChildren)
        {
            IsFiltered = GetIsFiltered(filterText);
        }

        /// <summary>
        /// Finds the child containing a specific resource.
        /// This specific node finds types of IContextResourceMode
        /// </summary>
        /// <typeparam name="T">Type of the resource to find</typeparam>
        /// <param name="resourceToFind">The resource to find.</param>
        /// <returns></returns>
        /// <date>2013/01/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public override ITreeNode FindChild<T>(T resourceToFind)
        {
            if (resourceToFind is IContextualResourceModel)
            {
                var toFind = resourceToFind as IContextualResourceModel;
                return ContexttualResourceModelEqualityComparer.Current
                                                               .Equals(DataContext, toFind)
                           ? this
                           : null;
            }
            if (resourceToFind is string)
            {
                var name = resourceToFind as string;
                if (DisplayName == name)
                    return this;
            }
            return base.FindChild(resourceToFind);
        }

        /// <summary>
        ///  Executes the edit command and send a message to debug
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Debug()
        {
            EditCommand.Execute(null);
            EventAggregator.Publish(new DebugResourceMessage(DataContext));
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Sends a message to delete this specific instance
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Delete()
        {
            if (DataContext == null) return;
            EventAggregator.Publish(new DeleteResourceMessage(DataContext, true));
            RaisePropertyChangedForCommands();
        }

        public override void NotifyOfFilterPropertyChanged(bool updateParent = false)
        {
            NotifyOfPropertyChange("ChildrenCount");

            if (TreeParent == null || !updateParent)
            {
                return;
            }

            TreeParent.NotifyOfFilterPropertyChanged(true);
        }

        /// <summary>
        /// Asks the wizard engine to create a qizard for this resource
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void CreateWizard()
        {
            if (DataContext == null) return;

            WizardEngine.CreateResourceWizard(DataContext);
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Sends a message to manually edit this resource
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void ManualEdit()
        {
            if (DataContext == null) return;
            SendManualEditMessage(DataContext);
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Edits this instance after checking whether its a wiazrd or resource
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Edit()
        {
            if (DataContext == null) return;

            //TODO Change to only show for resource wizards not system wizards
            if (WizardEngine.IsResourceWizard(DataContext))
                WizardEngine.EditWizard(DataContext);
            else
                SendEditMessage(DataContext);

            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Sends a message to edit the wizard fo this resource
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void EditWizard()
        {
            if (DataContext == null) return;

            WizardEngine.EditResourceWizard(DataContext);
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Sends a message to show the properties.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void ShowProperties()
        {
            EventAggregator.Publish(new ShowEditResourceWizardMessage(DataContext));
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        ///  Sends a message to show the dependencies.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void ShowDependencies()
        {
            EventAggregator.Publish(new ShowDependenciesMessage(DataContext, true));
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        ///  Sends a message to run this instance.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Run()
        {
            EventAggregator.Publish(new ExecuteResourceMessage(DataContext));
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        ///  Sends a message to shows the help for this resource.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void ShowHelp()
        {
            if (DataContext != null && !String.IsNullOrEmpty(DataContext.HelpLink))
                EventAggregator.Publish(new ShowHelpTabMessage(DataContext.HelpLink));
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Opens and then sends a message to save the resource
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Build()
        {
            EditCommand.Execute(null);
            EventAggregator.Publish(new SaveResourceMessage(DataContext, false));
            RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Sends a message to edit the wizard for this resource with a blank ID
        /// </summary>
        /// <author>Ashley.lewis</author>
        /// <date>2013/05/20</date>
        public void Duplicate(object obj)
        {
            //if (DataContext == null) return;

            //// TODO : Properly send message depending upon the DataContext type ;)
            //var myType = DataContext.ServerResourceType;
            //ResourceType rType;
            //ResourceType.TryParse(myType, out rType);

            //if (rType.Equals(ResourceType.WorkflowService))
            //{
            //    EventAggregator.Publish(new ShowEditResourceWizardMessage(DataContext));
            //}
            //else if (rType.Equals(ResourceType.DbService))
            //{
            //    DataContext.IsDatabaseService = true;
            //    EventAggregator.Publish(new ShowEditResourceWizardMessage(DataContext));
            //    // db service ;)
            //    // TODO : Handle this type ;)

            //    // VIA ServiceCallbackHandler
            //}
            //else if (rType.Equals(ResourceType.PluginService))
            //{
            //    DataContext.IsPluginService = true;
            //    EventAggregator.Publish(new ShowEditResourceWizardMessage(DataContext, false));
            //    // plugin service ;)
            //    // TODO : Handle this type ;)

            //    // VIA ServiceCallbackHandler
            //}
            //else if (rType.Equals(ResourceType.DbSource))
            //{
            //    DataContext.DisplayName = "DbSource";
            //    EventAggregator.Publish(new ShowEditResourceWizardMessage(DataContext, false));
            //    // db source ;)
            //    // TODO : Handle this type ;)

            //    // HANDLED BY WEB, WILL NOT WORK!!!
            //}
            //else if (rType.Equals(ResourceType.PluginSource))
            //{
            //    DataContext.DisplayName = "PluginSource";
            //    EventAggregator.Publish(new ShowEditResourceWizardMessage(DataContext, false));
            //    // plugin source ;)
            //    // TODO : Handle this type ;)

            //    // HANDLED BY WEB, WILL NOT WORK!!!
            //}
            ////else if (rType.Equals(ResourceType.Server))
            ////{
            ////    // server source ;)
            ////    // TODO : Handle this type ;)

            ////    // HANDLED BY WEB, WILL NOT WORK!!!
            ////}
            
            //RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Sends a message to edit the wizard for this resource with a blank ID
        /// </summary>
        /// <author>Ashley.lewis</author>
        /// <date>2013/05/20</date>
        public void MoveRename(object obj)
        {
            if (DataContext == null) return;
            if (DataContext.ID == Guid.Empty)
            {
                //update old resource
                DataContext.ID = Guid.NewGuid();
            }

            // TODO : Handle via resource type?!

            EventAggregator.Publish(new ShowEditResourceWizardMessage(DataContext));
            RaisePropertyChangedForCommands();
        }

        #endregion public methods

        #region private satic methods

        /// <summary>
        /// Sends the manual edit message.
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void SendManualEditMessage(IResourceModel resourceModel)
        {
            SendEditMessage(resourceModel);
        }

        /// <summary>
        /// Sends the edit message.
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void SendEditMessage(IResourceModel resourceModel)
        {
            switch (resourceModel.ResourceType)
            {
                case ResourceType.WorkflowService:
                    EventAggregator.Publish(new AddWorkSurfaceMessage(resourceModel));
                    break;
                case ResourceType.Source:
                    EventAggregator.Publish(new ShowEditResourceWizardMessage(resourceModel));
                    break;
                case ResourceType.Service:
                    EventAggregator.Publish(new ShowEditResourceWizardMessage(resourceModel));
                    break;
            }
        }

        #endregion

        #region IComparable Implementation
        public override int CompareTo(object obj)
        {
            var model = obj as ResourceTreeViewModel;
            if (model != null)
            {
                var other = model;
                return String.Compare(DisplayName, other.DisplayName, StringComparison.Ordinal);
            }
            return base.CompareTo(obj);
        }
        #endregion
    }
}