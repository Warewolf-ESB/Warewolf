using Microsoft.Practices.Prism.Mvvm;

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

    
        void Save();
    }
}