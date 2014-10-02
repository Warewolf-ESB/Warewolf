
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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HttpFramework.Sessions
{
    /// <summary>
    /// Interface for sessions
    /// </summary>
    public interface IHttpSession : IDisposable
    {
        /// <summary>
        /// Session id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Should 
        /// </summary>
        /// <param name="name">Name of the session variable</param>
        /// <returns><c>null</c> if it's not set</returns>
        /// <exception cref="SerializationException">If the object cant be serialized.</exception>
        object this[string name] { get; set; }

        /// <summary>
        /// When the session was last accessed.
        /// This property is touched by the HTTP server each time the
        /// session is requested.
        /// </summary>
        DateTime Accessed { get; set; }

        /// <summary>
        /// Number of session variables.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Remove everything from the session
        /// </summary>
        void Clear();

        /// <summary>
        /// Remove everything from the session
        /// </summary>
        /// <param name="expires">True if the session is cleared due to expiration</param>
        void Clear(bool expires);

        /// <summary>
        /// Event triggered upon clearing the session
        /// </summary>
        event HttpSessionClearedHandler BeforeClear;

		/// <summary>
		/// Gets keys for all values.
		/// </summary>
		IEnumerable<string> Keys { get; }
    }

    /// <summary>
    /// Arguments sent when a <see cref="IHttpSession" /> is cleared
    /// </summary>
    public class HttpSessionClearedArgs
    {
        private readonly bool _expired;

        /// <summary>
        /// Instantiates the arguments for the event
        /// </summary>
        /// <param name="expired">True if the session is cleared due to expiration</param>
        public HttpSessionClearedArgs(bool expired)
        {
            _expired = expired;
        }

        /// <summary>
        /// Gets if the session have expired.
        /// </summary>
        public bool Expired
        {
            get { return _expired; }
        }
    }

    /// <summary>
    /// Delegate for when a <see cref="IHttpSession"/> is cleared
    /// </summary>
    /// <param name="session"><see cref="IHttpSession"/> this is being cleared.</param>
    /// <param name="args">Arguments for the clearing</param>
    public delegate void HttpSessionClearedHandler(object session, HttpSessionClearedArgs args);
}
