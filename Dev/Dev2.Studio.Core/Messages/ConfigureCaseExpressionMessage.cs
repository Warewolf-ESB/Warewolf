using System;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class ConfigureCaseExpressionTO
    {
        public ModelItem TheItem { get; set; }

        public string ExpressionText { get; set; }
    }

    public class ConfigureCaseExpressionMessage : IMessage
    {
        public ConfigureCaseExpressionMessage(Tuple<ConfigureCaseExpressionTO, IEnvironmentModel> model)
        {
            Model = model;
        }

        public Tuple<ConfigureCaseExpressionTO, IEnvironmentModel> Model { get; set; }
    }
}