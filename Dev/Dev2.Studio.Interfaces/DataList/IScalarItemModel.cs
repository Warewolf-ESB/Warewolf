using System.Diagnostics.CodeAnalysis;

namespace Dev2.Studio.Interfaces.DataList
{
    public interface IScalarItemModel : IDataListItemModel
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        string ValidateName(string name);

        void Filter(string searchText);
    }
}