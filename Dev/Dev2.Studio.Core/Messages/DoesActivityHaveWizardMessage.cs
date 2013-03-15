using System.Activities.Presentation.Model;

namespace Dev2.Studio.Core.Messages
{
    public class DoesActivityHaveWizardMessage:IMessage
    {
        public ModelItem Model { get; set; }

        public DoesActivityHaveWizardMessage(ModelItem model)
        {
            Model = model;
        }
    }
}