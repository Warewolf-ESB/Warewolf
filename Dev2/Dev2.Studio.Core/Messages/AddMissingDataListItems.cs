using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Messages
{
    public class AddMissingDataListItems : IMessage
    {
        public AddMissingDataListItems(IList<IDataListVerifyPart> listToAdd, IResourceModel resourceModel)
        {
            ListToAdd = listToAdd;
            ResourceModel = resourceModel;
        }

        public IList<IDataListVerifyPart> ListToAdd { get; set; }

        public IResourceModel ResourceModel { get; set; }
    }
}
