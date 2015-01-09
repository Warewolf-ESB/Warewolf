namespace Dev2.Common.Interfaces.PopupController
{
    public interface IPopupMessageBoxFactory
    {
        IDev2MessageBoxViewModel Create(IPopupMessage message);
    }
}