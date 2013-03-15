namespace Dev2.Studio.Core.Messages
{
    public class HasWizardMessage:IMessage
    {
        public bool HasWizard { get; set; }

        public HasWizardMessage(bool hasWizard)
        {
            HasWizard = hasWizard;
        }
    }
}