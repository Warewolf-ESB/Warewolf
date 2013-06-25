
namespace Dev2.Studio.Core.AppResources.Browsers
{
    public interface IBrowserPopupController
    {
        void ConfigurePopup();
        bool ShowPopup(string url); // BUG 9798 - 2013.06.25 - TWR : added
    }
}
