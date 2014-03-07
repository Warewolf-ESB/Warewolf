using System;
using System.Threading.Tasks;

namespace Dev2.Threading
{
    public interface IAsyncWorker
    {
        /// <summary>
        /// Starts the specified background action and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundAction">The background action.</param>
        /// <param name="uiAction">The UI action.</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/08/08</date>
        Task Start(Action backgroundAction, Action uiAction);

        /// <summary>
        /// Starts the specified background action and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundAction">The background action.</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/08/08</date>
        Task Start(Action backgroundAction);

        /// <summary>
        /// Starts the specified background function and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundFunc">The background function - returns the result to be processed on the UI thread.</param>
        /// <param name="uiAction">The UI action to be taken on the given background result.</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/10/12</date>
        Task Start<TBackgroundResult>(Func<TBackgroundResult> backgroundFunc, Action<TBackgroundResult> uiAction);
    }
}
