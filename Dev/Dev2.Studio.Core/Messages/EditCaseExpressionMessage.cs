using System;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class EditCaseExpressionMessage : IMessage
    {
        public EditCaseExpressionMessage(Tuple<ModelProperty, IEnvironmentModel> model)
        {
            Model = model;
        }

        public Tuple<ModelProperty, IEnvironmentModel> Model { get; set; }
    }
}