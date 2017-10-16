using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Dev2.SignalR.Wrappers
{
    public interface IHubProxyWrapper
    {


        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        Task Invoke(string method, params object[] args);

        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of result returned from the hub.</typeparam>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        Task<T> Invoke<T>(string method, params object[] args);

        object Object();

        IDisposable On<T>(string eventName, Action<T> onData);
        
        ISubscriptionWrapper Subscribe(string sendmemo);
    }

    public interface ISubscriptionWrapper
    {
        event Action<IList<JToken>> Received;
    }


}
