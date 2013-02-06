
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;
using enActionType = Dev2.DynamicServices.enActionType;

namespace Dev2.Runtime.Services.Data
{
    public class ServiceActionWrapper
    {
        #region Fields

        private ServiceAction _serviceAction;
        private static IOutputDescriptionSerializationService outputDescriptionSerializationService = 
            OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

        #endregion Fields

        public ServiceActionWrapper()
        {
            _serviceAction = new ServiceAction();
        }

        #region Properties

        public string Name
        {
            get
            {
                return _serviceAction.Name;
            }
            set
            {
                _serviceAction.Name = value;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enActionType ActionType 
        {
            get
            {
                return _serviceAction.ActionType;
            }
            set
            {
                _serviceAction.ActionType = value;
            }
        }

        public string SourceName
        {
            get
            {
                return _serviceAction.SourceName;
            }
            set
            {
                _serviceAction.SourceName = value;
            }
        }

        public string SourceMethod
        {
            get
            {
                return _serviceAction.SourceMethod;
            }
            set
            {
                _serviceAction.SourceMethod = value;
            }
        }

        public IOutputDescription OutputDescription
        {
            get
            {
                return outputDescriptionSerializationService.Deserialize(_serviceAction.OutputDescription);
            }
            set
            {
                _serviceAction.OutputDescription = outputDescriptionSerializationService.Serialize(value);
            }
        }

        public List<ServiceActionInput> ServiceActionInputs
        {
            get
            {
                return _serviceAction.ServiceActionInputs;
            }
            set
            {
                _serviceAction.ServiceActionInputs = value;
            }
        }

        public IList<IDev2Definition> ServiceActionOutputs
        {
            get
            {
                return _serviceAction.ServiceActionOutputs;
            }
            set
            {
                _serviceAction.ServiceActionOutputs = value;
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion
    }
}
