using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Dev2.Common.Interfaces
{
    public interface IRequestServiceNameView : IView
    {
        void ShowView();

        void RequestClose();
    
        void EnterName(string serviceName);
    
        bool IsSaveButtonEnabled();
    
        string GetValidationMessage();

        void Cancel();
    }
}