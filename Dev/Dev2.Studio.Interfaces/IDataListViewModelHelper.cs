using System.Text;
using System.Xml;
using Dev2.Data;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Interfaces
{
    public interface IDataListViewModelHelper
    {
        OptomizedObservableCollection<IDataListItemModel> CreateFullDataList();
        bool IsJsonAttribute(XmlNode child);
        void AddItemToBuilder(StringBuilder result, IDataListItemModel item);
    }
}