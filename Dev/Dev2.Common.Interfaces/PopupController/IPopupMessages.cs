namespace Dev2.Common.Interfaces.PopupController
{
    public interface IPopupMessages
    {
        IPopupMessage GetNotConnected();
        IPopupMessage GetDeleteConfirmation(string nameOfItemBeingDeleted);
        IPopupMessage GetNameChangedConflict(string oldName, string newName);
        IPopupMessage GetSettingsCloseConfirmation();
        IPopupMessage GetSchedulerCloseConfirmation();
        IPopupMessage GetNoInputsSelectedWhenClickLink();
        IPopupMessage GetSaveErrorDialog(string errorMessage);
        IPopupMessage GetConnectionTimeoutConfirmation(string serverName);
        IPopupMessage GetDeleteVersionMessage(string displayName);
        IPopupMessage GetRollbackVersionMessage(string displayName);
        IPopupMessage GetInvalidCharacterMessage(string invalidText);
    }
}