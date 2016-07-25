using System.Diagnostics.CodeAnalysis;

namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IScalarItemModel : IDataListItemModel
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        string ValidateName(string name);
    }
}