using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Dev2 {
    
    public interface IFrameworkDuplexCallbackChannel {
        [OperationContract(IsOneWay=true)]
        void CallbackNotification(string message);

    }
}
