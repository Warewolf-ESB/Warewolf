using System.Collections.Generic;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class AddStringListToDataListMessage : IMessage
    {
        public AddStringListToDataListMessage(List<string> listToAdd)
        {
            ListToAdd = listToAdd;
        }

        public List<string> ListToAdd { get; set; }

    }
}
