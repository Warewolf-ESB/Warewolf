
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Messages
{
    public class ShowUnusedDataListVariablesMessage : IMessage
    {

        public ShowUnusedDataListVariablesMessage(IList<IDataListVerifyPart> listOfUnused, IResourceModel resourceModel)
        {
            ListOfUnused = listOfUnused;
            ResourceModel = resourceModel;
        }

        public IList<IDataListVerifyPart> ListOfUnused { get; set; }

        public IResourceModel ResourceModel { get; set; }
    }
}
