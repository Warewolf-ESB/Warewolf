
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Concurrent;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Browsers
{
    /// <summary>
    /// Handler for posting via a GET method.
    /// </summary>
    public class Dev2RequestHandler : IDisposable
    {
        public const string PostQuery = "method=post";

        ConcurrentDictionary<int, string> _postData = new ConcurrentDictionary<int, string>();
        bool _disposed;

        #region SetData

        /// <summary>
        /// Sets the data prior to invoking <see cref="GetData(int, Uri, string)"/>.
        /// </summary>
        /// <param name="requestID">The request ID - usually the process ID of the request.</param>
        /// <param name="postData">The post data for the GET request.</param>
        public void SetData(int requestID, string postData)
        {
            _postData.TryAdd(requestID, postData);
        }

        #endregion

        #region GetData

        /// <summary>
        /// Gets the post data for the request ID.
        /// Post data must be set previously by invoking <see cref="SetData"/>.
        /// </summary>
        /// <param name="requestID">The request ID - usually the process ID of the request.</param>
        /// <returns>The post data if It was found; <code>null</code> otherwise.</returns>
        public string GetData(int requestID)
        {
            string postData;
            return _postData.TryRemove(requestID, out postData) ? postData : null;
        }

        /// <summary>
        /// Gets the post data for <paramref name="requestUri"/> that have a query containing <b>method=post</b>.
        /// Post data must be set previously by invoking <see cref="SetData"/>.
        /// </summary>
        /// <param name="requestID">The request ID - usually the process ID of the request.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="requestMethod">The request method - usually POST or GET.</param>
        /// <returns>The post data if the query contains <b>method=post</b> and it was found; <code>null</code> otherwise.</returns>
        public string GetData(int requestID, Uri requestUri, string requestMethod)
        {
            //
            // NOTE: This method MUST be as efficient as possible; hence no NULL parameter checks, etc.
            // 
            if(requestMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
            {
                // Check if we're trying to POST with a GET
                if(requestUri.OriginalString.LastIndexOf(PostQuery, StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    string postData;
                    if(_postData.TryRemove(requestID, out postData))
                    {
                        return postData;
                    }
                }
            }
            return null;
        }

        #endregion

        #region IDisposable

        ~Dev2RequestHandler()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_disposed)
            {
                if(disposing)
                {
                    // Dispose managed resources.
                    if(_postData != null)
                    {
                        _postData.Clear();
                        _postData = null;
                    }
                }

                // Clean up unmanaged resources here.

                _disposed = true;

            }
        }

        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
