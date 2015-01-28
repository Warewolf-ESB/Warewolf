namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{
    public interface IInnerDialogueTemplate
    {
        /// <summary>
        /// called by outer when validating
        /// </summary>
        /// <returns></returns>
        string Validate();
        /// <summary>
        /// Is valid 
        /// </summary>
        bool IsValid { get; set; }
    }
}