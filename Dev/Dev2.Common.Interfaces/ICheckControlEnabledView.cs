using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common.Interfaces
{
    public interface ICheckControlEnabledView
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        bool GetControlEnabled(string controlName);
    }
}
