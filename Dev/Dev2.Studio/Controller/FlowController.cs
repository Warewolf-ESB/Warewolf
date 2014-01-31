#region

using Caliburn.Micro;
using Dev2.Common;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Webs;
using Dev2.Studio.Webs.Callbacks;
using Newtonsoft.Json;
using System.Activities.Presentation.Model;
using System.ComponentModel.Composition;
using System.Windows;
using Unlimited.Applications.BusinessDesignStudio.Activities;

#endregion

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Controller
{
    [Export]
    public class FlowController : IHandle<ConfigureDecisionExpressionMessage>, IHandle<ConfigureSwitchExpressionMessage>,
                                  IHandle<ConfigureCaseExpressionMessage>, IHandle<EditCaseExpressionMessage>
    {

        #region Fields

        private readonly IPopupController _popupController;
        private Dev2DecisionCallbackHandler _callBackHandler;

        #endregion Fields

        #region ctor

        [ImportingConstructor]
        public FlowController([Import] IPopupController popupController)
        {
            _popupController = popupController;
            EventPublishers.Aggregator.Subscribe(this);
            _callBackHandler = new Dev2DecisionCallbackHandler();
        }

        #endregion ctor

        #region Public Methods

        /// <summary>
        ///     Configures the decision expression.
        ///     Travis.Frisinger - Developed for new Decision Wizard
        /// </summary>
        void ConfigureDecisionExpression(ConfigureDecisionExpressionMessage args)
        {
            var condition = ConfigureActivity<DsfFlowDecisionActivity>(args.ModelItem, GlobalConstants.ConditionPropertyText, args.IsNew);
            if(condition == null)
            {
                return;
            }

            var expression = condition.Properties[GlobalConstants.ExpressionPropertyText];
            var ds = DataListConstants.DefaultStack;

            if(expression != null && expression.Value != null)
            {
                //we got a model, push it in to the Model region ;)
                // but first, strip and extract the model data ;)

                var eval = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(expression.Value.ToString());

                if(!string.IsNullOrEmpty(eval))
                {
                    ds = JsonConvert.DeserializeObject<Dev2DecisionStack>(eval);
                }
            }

            var displayName = args.ModelItem.Properties[GlobalConstants.DisplayNamePropertyText];
            if(displayName != null && displayName.Value != null)
            {
                ds.DisplayText = displayName.Value.ToString();
            }

            var val = JsonConvert.SerializeObject(ds);

            // Now invoke the Wizard ;)
            _callBackHandler = StartDecisionWizard(args.EnvironmentModel, val);

            // Wizard finished...
            try
            {
                string tmp = WebHelper.CleanModelData(_callBackHandler);
                var dds = JsonConvert.DeserializeObject<Dev2DecisionStack>(tmp);

                if(dds == null)
                {
                    return;
                }

                Utilities.ActivityHelper.SetArmTextDefaults(dds);
                Utilities.ActivityHelper.InjectExpression(dds, expression);
                Utilities.ActivityHelper.SetArmText(args.ModelItem, dds);
                Utilities.ActivityHelper.SetDisplayName(args.ModelItem, dds); // PBI 9220 - 2013.04.29 - TWR
            }
            catch
            {
                _popupController.Show(GlobalConstants.DecisionWizardErrorString,
                                      GlobalConstants.DecisionWizardErrorHeading, MessageBoxButton.OK,
                                      MessageBoxImage.Error);
            }
        }

        public void ConfigureSwitchExpression(ConfigureSwitchExpressionMessage args)
        {
            var expression = ConfigureActivity<DsfFlowSwitchActivity>(args.ModelItem, GlobalConstants.SwitchExpressionPropertyText, args.IsNew);
            if(expression == null)
            {
                return;
            }

            var expressionText = expression.Properties[GlobalConstants.SwitchExpressionTextPropertyText];
            var ds = DataListConstants.DefaultSwitch;


            if(expressionText != null && expressionText.Value != null)
            {
                var val = Utilities.ActivityHelper.ExtractData(expressionText.Value.ToString());
                if(!string.IsNullOrEmpty(val))
                {
                    ds.SwitchVariable = val;
                }
            }

            var displayName = args.ModelItem.Properties[GlobalConstants.DisplayNamePropertyText];
            if(displayName != null && displayName.Value != null)
            {
                ds.DisplayText = displayName.Value.ToString();
            }

            var webModel = JsonConvert.SerializeObject(ds);

            // now invoke the wizard ;)
            _callBackHandler = StartSwitchDropWizard(args.EnvironmentModel, webModel);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var resultSwitch = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);
                Utilities.ActivityHelper.InjectExpression(resultSwitch, expressionText);

                // PBI 9220 - 2013.04.29 - TWR
                Utilities.ActivityHelper.SetDisplayName(args.ModelItem, resultSwitch); // MUST use args.ModelItem otherwise it won't be visible!
            }
            catch
            {
                _popupController.Show(GlobalConstants.SwitchWizardErrorString,
                                      GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                      MessageBoxImage.Error);
            }
        }

        public void ConfigureSwitchCaseExpression(ConfigureCaseExpressionMessage args)
        {
            IEnvironmentModel environment = args.EnvironmentModel;
            ModelItem switchCase = args.ModelItem;

            string modelData =
                JsonConvert.SerializeObject(new Dev2Switch
                    {
                        SwitchVariable = "",
                        SwitchExpression = args.ExpressionText
                    });

            // now invoke the wizard ;)
            _callBackHandler = RootWebSite.ShowSwitchDragDialog(environment, modelData);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var ds = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);
                Utilities.ActivityHelper.SetSwitchKeyProperty(ds, switchCase);
            }
            catch
            {
                _popupController.Show(GlobalConstants.SwitchWizardErrorString,
                                      GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                      MessageBoxImage.Error);
            }
        }

        // 28.01.2013 - Travis.Frisinger : Added for Case Edits
        public void EditSwitchCaseExpression(EditCaseExpressionMessage args)
        {
            IEnvironmentModel environment = args.EnvironmentModel;
            ModelProperty switchCaseValue = args.ModelItem.Properties["Case"];

            string modelData = JsonConvert.SerializeObject(DataListConstants.DefaultCase);

            // Extract existing value ;)
            if(switchCaseValue != null)
            {
                string val = switchCaseValue.ComputedValue.ToString();
                modelData = JsonConvert.SerializeObject(new Dev2Switch { SwitchVariable = val });
            }

            // now invoke the wizard ;)
            _callBackHandler = RootWebSite.ShowSwitchDragDialog(environment, modelData);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var ds = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);

                if(ds != null)
                {
                    if(switchCaseValue != null)
                    {
                        switchCaseValue.SetValue(ds.SwitchVariable);
                    }
                }
            }
            catch
            {
                _popupController.Show(GlobalConstants.SwitchWizardErrorString,
                                      GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                      MessageBoxImage.Error);
            }
        }

        #endregion public methods

        #region Protected Methods

        protected virtual Dev2DecisionCallbackHandler StartDecisionWizard(IEnvironmentModel environmentModel, string val)
        {
            return RootWebSite.ShowDecisionDialog(environmentModel, val);
        }

        protected virtual Dev2DecisionCallbackHandler StartSwitchDropWizard(IEnvironmentModel environmentModel, string val)
        {
            return RootWebSite.ShowSwitchDropDialog(environmentModel, val);
        }

        #endregion

        #region IHandle

        public void Handle(ConfigureDecisionExpressionMessage message)
        {
            ConfigureDecisionExpression(message);
        }

        public void Handle(ConfigureSwitchExpressionMessage message)
        {
            ConfigureSwitchExpression(message);
        }

        public void Handle(ConfigureCaseExpressionMessage message)
        {
            ConfigureSwitchCaseExpression(message);
        }

        public void Handle(EditCaseExpressionMessage message)
        {
            EditSwitchCaseExpression(message);
        }

        #endregion IHandle

        #region ConfigureActivity

        ModelItem ConfigureActivity<T>(ModelItem modelItem, string propertyName, bool isNew) where T : class, IFlowNodeActivity, new()
        {
            var property = modelItem.Properties[propertyName];
            if(property == null)
            {
                return null;
            }

            ModelItem result;
            var activity = property.ComputedValue as T;
            if(activity == null)
            {
                activity = new T();
                result = property.SetValue(activity);
            }
            else
            {
                result = property.Value;

                // BUG 9717 - 2013.06.22 - don't show wizard on copy paste
                var isCopyPaste = isNew && !string.IsNullOrEmpty(activity.ExpressionText);
                if(result == null || isCopyPaste)
                {
                    return null;
                }
            }
            return result;
        }

        #endregion

    }
}