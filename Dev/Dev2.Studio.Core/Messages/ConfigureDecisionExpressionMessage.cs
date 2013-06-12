using System;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class ConfigureDecisionExpressionMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ConfigureDecisionExpressionMessage(Tuple<ModelItem, IEnvironmentModel> model)
        {
            Model = model;
        }

        public Tuple<ModelItem, IEnvironmentModel> Model { get; set; }
    }
}