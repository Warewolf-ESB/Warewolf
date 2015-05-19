namespace Dev2.Common.Interfaces.Studio
{
    public interface IDialogViewModelFactory
    {
        IDialogueViewModel CreateAboutDialog();
         IDialogueViewModel CreateServerAboutDialog(string serverVersion);
    } 
}

