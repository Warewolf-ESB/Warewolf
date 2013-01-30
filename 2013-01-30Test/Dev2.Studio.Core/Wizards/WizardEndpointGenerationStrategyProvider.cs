using System;
using System.ComponentModel.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Dev2.TO;

namespace Dev2.Studio.Core.Wizards
{
    [Export(typeof(IServiceEndpointGenerationStrategyProvider))]
    public class WizardEndpointGenerationStrategyProvider : IServiceEndpointGenerationStrategyProvider
    {
        #region Class Members

        private static readonly string _resourceActivityWizardServiceKey = "ResourceActivitySpecificSettingsWizard";
        private static readonly string _codedActivityWizardServiceKey = "CodedActivitySpecificSettingsWizard";
        private static readonly string _activitySettingsWizardServiceKey = "ActivityGeneralSettingsWizard";
        private static readonly string _serviceWithDataListMergeAndExecutionCallBackKey = "ServiceWithDataListMergeAndExecutionCallBacK";
        private static readonly string _serviceWithExecutionCallBackKey = "ServiceWithExecutionCallBacK";

        private static readonly string _queryStringStartChar = "?";
        private static readonly string _queryStringDelimiterChar = "&";
        private static readonly string _queryStringAssignmentOperator = "=";

        private static readonly string _endpointDatalistOutMergeID = "DatalistOutMergeID";
        private static readonly string _endpointDatalistOutMergeType = "DatalistOutMergeType";
        private static readonly string _endpointDatalistOutMergeDepth = "DatalistOutMergeDepth";
        private static readonly string _endpointDatalistOutMergeFrequency = "DatalistOutMergeFrequency";

        private static readonly string _endpointDatalistInMergeID = "DatalistInMergeID";
        private static readonly string _endpointDatalistInMergeType = "DatalistInMergeType";
        private static readonly string _endpointDatalistInMergeDepth = "DatalistInMergeDepth";
        
        private static readonly string _endpointExecutionStatusCallbackID = "ExecutionCallbackID";

        private readonly string _activityGeneralSettingsWizardName = "Dev2ActivityGeneralSettingsWizard";

        #endregion Class Members

        #region Methods

        public void RegisterEndpointGenerationStrategies(IServiceLocator serviceLocator)
        {
            //
            // Check if ServiceLocator is null
            //
            if (serviceLocator == null)
            {
                throw new ArgumentNullException("serviceLocator");
            }

            //
            // Register generation strategies for resource and coded activities
            //
            serviceLocator.RegisterEnpoint<Tuple<IContextualResourceModel, DataListMergeOpsTO, Guid>>(_resourceActivityWizardServiceKey, ResourceActivityWizardEndpointGenerationStrategy);
            serviceLocator.RegisterEnpoint<Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>>(_codedActivityWizardServiceKey, CodedActivityWizardEndpointGenerationStrategy);
            serviceLocator.RegisterEnpoint<Tuple<IEnvironmentModel, DataListMergeOpsTO, Guid>>(_activitySettingsWizardServiceKey, ActivitySettingsWizardEndpointGenerationStrategy);
            serviceLocator.RegisterEnpoint<Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>>(_serviceWithDataListMergeAndExecutionCallBackKey, ServiceWithDataListMergeAndExecutionCallBackEndpointGenerationStrategy);
            serviceLocator.RegisterEnpoint<Tuple<string, IEnvironmentModel, Guid>>(_serviceWithExecutionCallBackKey, ServiceWithExecutionCallBackEndpointGenerationStrategy);
        }

        #endregion Methods

        #region Static Properties

        public static string ResourceActivityWizardServiceKey
        {
            get { return _resourceActivityWizardServiceKey; }
        }

        public static string CodedActivityWizardServiceKey
        {
            get { return _codedActivityWizardServiceKey; }
        }

        public static string ActivitySettingsWizardServiceKey
        {
            get { return _activitySettingsWizardServiceKey; }
        }

        public static string ServiceWithDataListMergeAndExecutionCallBackKey
        {
            get { return _serviceWithDataListMergeAndExecutionCallBackKey; }
        }

        public static string ServiceWithExecutionCallBackKey
        {
            get { return _serviceWithExecutionCallBackKey; }
        }

        #endregion Static Properties

        #region Private Methods

        /// <summary>
        /// The logic responsible for generating the wizard endpoint for a resource activity specific settings.
        /// </summary>
        /// <param name="resource">The workflow resource for which to generate the wizard endpoint.</param>
        private Uri ResourceActivityWizardEndpointGenerationStrategy(Tuple<IContextualResourceModel, DataListMergeOpsTO, Guid> activityInfo)
        {
            DataListMergeOpsTO mergeTO = activityInfo.Item2;
            IContextualResourceModel resource = activityInfo.Item1;
            string executionStatusCallbackID = activityInfo.Item3.ToString();

            string wizardName = WizardEngine.GetResourceWizardName(resource.ResourceName);
            string queryString = _queryStringStartChar +
                _endpointDatalistOutMergeDepth + _queryStringAssignmentOperator + mergeTO.DataListOutMergeDepth + _queryStringDelimiterChar +
                _endpointDatalistOutMergeFrequency + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeFrequency + _queryStringDelimiterChar +
                _endpointDatalistOutMergeID + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeID + _queryStringDelimiterChar +
                _endpointDatalistOutMergeType + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeType + _queryStringDelimiterChar +

                _endpointDatalistInMergeDepth + _queryStringAssignmentOperator + mergeTO.DataListInMergeDepth + _queryStringDelimiterChar +
                _endpointDatalistInMergeID + _queryStringAssignmentOperator + mergeTO.DatalistInMergeID + _queryStringDelimiterChar +
                _endpointDatalistInMergeType + _queryStringAssignmentOperator + mergeTO.DatalistInMergeType + _queryStringDelimiterChar +

                _endpointExecutionStatusCallbackID + _queryStringAssignmentOperator + executionStatusCallbackID;



            string relativeUri = "/services/" + wizardName + queryString;

            Uri endpointUri;
            if (!Uri.TryCreate(resource.Environment.WebServerAddress, relativeUri, out endpointUri))
            {
                endpointUri = new Uri(new Uri(StringResources.Uri_WebServer), relativeUri);
            }

            return endpointUri;
        }

        /// <summary>
        /// The logic responsible for generating the wizard endpoint for a coded activity specific settings.
        /// </summary>
        /// <param name="activityInfo">The activity name and environment against which it is executed.</param>
        private Uri CodedActivityWizardEndpointGenerationStrategy(Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid> activityInfo)
        {
            IEnvironmentModel enviromentModel = activityInfo.Item2;
            DataListMergeOpsTO mergeTO = activityInfo.Item3;
            string executionStatusCallbackID = activityInfo.Item4.ToString();

            string wizardName = WizardEngine.GetCodedActivityWizardName(activityInfo.Item1);
            string queryString = _queryStringStartChar +
                _endpointDatalistOutMergeDepth + _queryStringAssignmentOperator + mergeTO.DataListOutMergeDepth + _queryStringDelimiterChar +
                _endpointDatalistOutMergeFrequency + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeFrequency + _queryStringDelimiterChar +
                _endpointDatalistOutMergeID + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeID + _queryStringDelimiterChar +
                _endpointDatalistOutMergeType + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeType + _queryStringDelimiterChar +

                _endpointDatalistInMergeDepth + _queryStringAssignmentOperator + mergeTO.DataListInMergeDepth + _queryStringDelimiterChar +
                _endpointDatalistInMergeID + _queryStringAssignmentOperator + mergeTO.DatalistInMergeID + _queryStringDelimiterChar +
                _endpointDatalistInMergeType + _queryStringAssignmentOperator + mergeTO.DatalistInMergeType + _queryStringDelimiterChar +

                _endpointExecutionStatusCallbackID + _queryStringAssignmentOperator + executionStatusCallbackID;

            string relativeUri = "/services/" + wizardName + queryString;

            Uri endpointUri;
            if (!Uri.TryCreate(enviromentModel.WebServerAddress, relativeUri, out endpointUri))
            {
                endpointUri = new Uri(new Uri(StringResources.Uri_WebServer), relativeUri);
            }

            return endpointUri;
        }

        /// <summary>
        /// The logic responsible for generating the wizard endpoint for a activity specific settings.
        /// </summary>
        /// <param name="environment">The environment against which the activity will be executed.</param>
        private Uri ActivitySettingsWizardEndpointGenerationStrategy(Tuple<IEnvironmentModel, DataListMergeOpsTO, Guid> activityInfo)
        {
            IEnvironmentModel enviromentModel = activityInfo.Item1;
            DataListMergeOpsTO mergeTO = activityInfo.Item2;
            string executionStatusCallbackID = activityInfo.Item3.ToString();

            string queryString = _queryStringStartChar +
                _endpointDatalistOutMergeDepth + _queryStringAssignmentOperator + mergeTO.DataListOutMergeDepth + _queryStringDelimiterChar +
                _endpointDatalistOutMergeFrequency + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeFrequency + _queryStringDelimiterChar +
                _endpointDatalistOutMergeID + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeID + _queryStringDelimiterChar +
                _endpointDatalistOutMergeType + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeType + _queryStringDelimiterChar +

                _endpointDatalistInMergeDepth + _queryStringAssignmentOperator + mergeTO.DataListInMergeDepth + _queryStringDelimiterChar +
                _endpointDatalistInMergeID + _queryStringAssignmentOperator + mergeTO.DatalistInMergeID + _queryStringDelimiterChar +
                _endpointDatalistInMergeType + _queryStringAssignmentOperator + mergeTO.DatalistInMergeType + _queryStringDelimiterChar +

                _endpointExecutionStatusCallbackID + _queryStringAssignmentOperator + executionStatusCallbackID;

            string relativeUri = "/services/" + _activityGeneralSettingsWizardName + queryString;

            Uri endpointUri;
            if (!Uri.TryCreate(enviromentModel.WebServerAddress, relativeUri, out endpointUri))
            {
                endpointUri = new Uri(new Uri(StringResources.Uri_WebServer), relativeUri);
            }

            return endpointUri;
        }

        /// <summary>
        /// The logic responsible for generating a service endpoint with datalist merge and execution call back parameters
        /// </summary>
        /// <param name="serviceInfo">The service info.</param>
        private Uri ServiceWithDataListMergeAndExecutionCallBackEndpointGenerationStrategy(Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid> serviceInfo)
        {
            IEnvironmentModel enviromentModel = serviceInfo.Item2;
            DataListMergeOpsTO mergeTO = serviceInfo.Item3;
            string executionStatusCallbackID = serviceInfo.Item4.ToString();
            string serviceName = serviceInfo.Item1;

            string queryString = _queryStringStartChar +
                _endpointDatalistOutMergeDepth + _queryStringAssignmentOperator + mergeTO.DataListOutMergeDepth + _queryStringDelimiterChar +
                _endpointDatalistOutMergeFrequency + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeFrequency + _queryStringDelimiterChar +
                _endpointDatalistOutMergeID + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeID + _queryStringDelimiterChar +
                _endpointDatalistOutMergeType + _queryStringAssignmentOperator + mergeTO.DatalistOutMergeType + _queryStringDelimiterChar +

                _endpointDatalistInMergeDepth + _queryStringAssignmentOperator + mergeTO.DataListInMergeDepth + _queryStringDelimiterChar +
                _endpointDatalistInMergeID + _queryStringAssignmentOperator + mergeTO.DatalistInMergeID + _queryStringDelimiterChar +
                _endpointDatalistInMergeType + _queryStringAssignmentOperator + mergeTO.DatalistInMergeType + _queryStringDelimiterChar +

                _endpointExecutionStatusCallbackID + _queryStringAssignmentOperator + executionStatusCallbackID;

            string relativeUri = "/services/" + serviceName + queryString;

            Uri endpointUri;
            if (!Uri.TryCreate(enviromentModel.WebServerAddress, relativeUri, out endpointUri))
            {
                endpointUri = new Uri(new Uri(StringResources.Uri_WebServer), relativeUri);
            }

            return endpointUri;
        }

        /// <summary>
        /// The logic responsible for generating a service endpoint with execution call back parameters
        /// </summary>
        /// <param name="serviceInfo">The service info.</param>
        private Uri ServiceWithExecutionCallBackEndpointGenerationStrategy(Tuple<string, IEnvironmentModel, Guid> serviceInfo)
        {
            IEnvironmentModel enviromentModel = serviceInfo.Item2;
            string executionStatusCallbackID = serviceInfo.Item3.ToString();
            string serviceName = serviceInfo.Item1;

            string queryString = _queryStringStartChar +
                _endpointExecutionStatusCallbackID + _queryStringAssignmentOperator + executionStatusCallbackID;

            string relativeUri = "/services/" + serviceName + queryString;

            Uri endpointUri;
            if (!Uri.TryCreate(enviromentModel.WebServerAddress, relativeUri, out endpointUri))
            {
                endpointUri = new Uri(new Uri(StringResources.Uri_WebServer), relativeUri);
            }

            return endpointUri;
        }

        #endregion Private Methods
    }
}
