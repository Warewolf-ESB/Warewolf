using System.Text;
using System.Xml;
using Dev2.Data;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Interfaces
{
    internal interface IDataListViewModelHelper
    {
        OptomizedObservableCollection<IDataListItemModel> CreateFullDataList();
        bool IsJsonAttribute(XmlNode child);
        void AddItemToBuilder(StringBuilder result, IDataListItemModel item);
    }
}