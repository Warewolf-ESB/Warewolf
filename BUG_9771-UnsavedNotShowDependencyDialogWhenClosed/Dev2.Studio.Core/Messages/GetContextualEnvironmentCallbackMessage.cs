using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;

namespace Dev2.Studio.Core.Messages
{
    public class GetContextualEnvironmentCallbackMessage : ICallBackMessage<IEnvironmentModel>
    {


        public Action<IEnvironmentModel> Callback { get; set; }

        public GetContextualEnvironmentCallbackMessage(Action<IEnvironmentModel> callback)
        {
            Callback = callback;
        }


        public interface ICallBackMessage<T> where T : class
        {
            Action<T> Callback { get; set; }
        }
    }

}
