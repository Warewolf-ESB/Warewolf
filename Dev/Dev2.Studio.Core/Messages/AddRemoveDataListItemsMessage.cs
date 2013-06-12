using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class AddRemoveDataListItemsMessage : IMessage
    {
        public IDataListViewModel DataListViewModel { get; set; }

        public AddRemoveDataListItemsMessage(IDataListViewModel dataListViewModel)
        {
            DataListViewModel = dataListViewModel;
        }
    }
}