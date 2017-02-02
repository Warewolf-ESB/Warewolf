using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IRequestServiceNameView : IView
    {
        void ShowView();

        void RequestClose();


        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void EnterName(string serviceName);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        bool IsSaveButtonEnabled();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        string GetValidationMessage();


        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void Cancel();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void Save();
    }
}