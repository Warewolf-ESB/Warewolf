using System;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class ConfigureSwitchExpressionMessage : IMessage
    {
        public ConfigureSwitchExpressionMessage(Tuple<ModelItem, IEnvironmentModel> model)
        {
            Model = model;
        }

        public Tuple<ModelItem, IEnvironmentModel> Model { get; set; }
    }
}