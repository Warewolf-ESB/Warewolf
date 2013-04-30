#region

using System;
using System.Activities.Presentation.Model;
using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Webs;
using Dev2.Utilities;
using Newtonsoft.Json;

#endregion

namespace Dev2.Studio.Controller
{
    [Export]
    public class FlowController : IHandle<ConfigureDecisionExpressionMessage>, IHandle<ConfigureSwitchExpressionMessage>,
                                  IHandle<ConfigureCaseExpressionMessage>, IHandle<EditCaseExpressionMessage>
    {

        #region Fields

        private readonly IEventAggregator _eventAggregator;
        private readonly IPopupController _popupController;
        private Dev2DecisionCallbackHandler _callBackHandler;

        #endregion Fields

        #region ctor

        [ImportingConstructor]
        public FlowController([Import] IEventAggregator eventAggregator,
                              [Import] IPopupController popupController)
        {
            _popupController = popupController;
            _eventAggregator = eventAggregator;
            if (_eventAggregator != null)
                _eventAggregator.Subscribe(this);

            _callBackHandler = new Dev2DecisionCallbackHandler();
        }

        #endregion ctor

        #region Public Methods

        /// <summary>
        ///     Configures the decision expression.
        ///     Travis.Frisinger - Developed for new Decision Wizard
        /// </summary>
        /// <param name="wrapper">The wrapper.</param>
        public void ConfigureDecisionExpression(Tuple<ModelItem, IEnvironmentModel> wrapper)
        {
            IEnvironmentModel environment = wrapper.Item2;

            var activity = ActivityHelper.GetActivityFromWrapper(wrapper, GlobalConstants.ConditionPropertyText);
            if (activity == null)
            {
                return;
            }

            string val = JsonConvert.SerializeObject(DataListConstants.DefaultStack);

            ModelProperty activityExpression = activity.Properties[GlobalConstants.ExpressionPropertyText];

            if (activityExpression != null && activityExpression.Value != null)
            {
                //we got a model, push it in to the Model region ;)
                // but first, strip and extract the model data ;)

                val = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(activityExpression.Value.ToString());

                if (string.IsNullOrEmpty(val))
                {
                    val = JsonConvert.SerializeObject(DataListConstants.DefaultStack);
                }
            }

            // Now invoke the Wizard ;)
            Uri requestUri;
            if (!Uri.TryCreate((environment.Connection.WebServerUri + GlobalConstants.DecisionWizardLocation)
                               , UriKind.Absolute, out requestUri)) return;

            _callBackHandler = WebHelper.ShowWebpage(requestUri, val, 824, 510);

            // Wizard finished...
            try
            {
                string tmp = WebHelper.CleanModelData(_callBackHandler);
                var dds = JsonConvert.DeserializeObject<Dev2DecisionStack>(tmp);

                if (dds == null)
                {
                    return;
                }

                ActivityHelper.SetArmTextDefaults(dds);
                ActivityHelper.InjectExpression(dds, activityExpression);
                ActivityHelper.SetArmText(activity, dds);
            }
            catch
            {
                _popupController.Show(GlobalConstants.DecisionWizardErrorString,
                                      GlobalConstants.DecisionWizardErrorHeading, MessageBoxButton.OK,
                                      MessageBoxImage.Error);
            }
        }

        public void ConfigureSwitchExpression(Tuple<ModelItem, IEnvironmentModel> wrapper)
        {
            IEnvironmentModel environment = wrapper.Item2;
            ModelItem activity = ActivityHelper.GetActivityFromWrapper(wrapper,
                                                                       GlobalConstants.SwitchExpressionPropertyText);

            if (activity == null) return;

            ModelProperty activityExpression =
                activity.Properties[GlobalConstants.SwitchExpressionTextPropertyText];

            string webModel = JsonConvert.SerializeObject(DataListConstants.DefaultSwitch);

            if (activityExpression != null && activityExpression.Value != null)
            {
                string val = ActivityHelper.ExtractData(activityExpression.Value.ToString());
                if (!string.IsNullOrEmpty(val))
                {
                    var ds = new Dev2Switch {SwitchVariable = val};
                    webModel = JsonConvert.SerializeObject(ds);
                }
            }

            // now invoke the wizard ;)
            Uri requestUri;
            if (!Uri.TryCreate((environment.Connection.WebServerUri + GlobalConstants.SwitchDropWizardLocation),
                               UriKind.Absolute, out requestUri)) return;

            _callBackHandler = WebHelper.ShowWebpage(requestUri, webModel, 770, 175);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var ds = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);
                ActivityHelper.InjectExpression(ds, activityExpression);
            }
            catch
            {
                _popupController.Show(GlobalConstants.SwitchWizardErrorString,
                                      GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                      MessageBoxImage.Error);
            }
        }

        public void ConfigureSwitchCaseExpression(Tuple<ConfigureCaseExpressionTO, IEnvironmentModel> payload)
        {
            IEnvironmentModel environment = payload.Item2;
            ModelItem switchCase = payload.Item1.TheItem;

            string modelData =
                JsonConvert.SerializeObject(new Dev2Switch
                    {
                        SwitchVariable = "",
                        SwitchExpression = payload.Item1.ExpressionText
                    });

            // now invoke the wizard ;)
            Uri requestUri;
            if (!Uri.TryCreate((environment.Connection.WebServerUri + GlobalConstants.SwitchDragWizardLocation),
                               UriKind.Absolute, out requestUri)) return;

            _callBackHandler = WebHelper.ShowWebpage(requestUri, modelData, 770, 185);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var ds = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);
                ActivityHelper.SetSwitchKeyProperty(ds, switchCase);
            }
            catch
            {
                _popupController.Show(GlobalConstants.SwitchWizardErrorString,
                                      GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                      MessageBoxImage.Error);
            }
        }

        // 28.01.2013 - Travis.Frisinger : Added for Case Edits
        public void EditSwitchCaseExpression(Tuple<ModelProperty, IEnvironmentModel> payload)
        {
            IEnvironmentModel environment = payload.Item2;
            ModelProperty switchCaseValue = payload.Item1;

            string modelData = JsonConvert.SerializeObject(DataListConstants.DefaultCase);

            // Extract existing value ;)
            if (switchCaseValue != null)
            {
                string val = switchCaseValue.ComputedValue.ToString();
                modelData = JsonConvert.SerializeObject(new Dev2Switch {SwitchVariable = val});
            }

            // now invoke the wizard ;)
            Uri requestUri;
            if (!Uri.TryCreate((environment.Connection.WebServerUri + GlobalConstants.SwitchDragWizardLocation),
                               UriKind.Absolute, out requestUri)) return;

            _callBackHandler = WebHelper.ShowWebpage(requestUri, modelData, 770, 185);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var ds = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);

                if (ds != null)
                {
                    if (switchCaseValue != null)
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

        #region IHandle

        public void Handle(ConfigureCaseExpressionMessage message)
        {
            ConfigureSwitchCaseExpression(message.Model);
        }

        public void Handle(ConfigureDecisionExpressionMessage message)
        {
            ConfigureDecisionExpression(message.Model);
        }

        public void Handle(ConfigureSwitchExpressionMessage message)
        {
            ConfigureSwitchExpression(message.Model);
        }

        public void Handle(EditCaseExpressionMessage message)
        {
            EditSwitchCaseExpression(message.Model);
        }

        #endregion IHandle
    }
}