
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

namespace HttpFramework.Sessions
{
    /// <summary>
    /// A session stored in memory.
    /// </summary>
    public class MemorySession : IHttpSession
    {
        private string _id;
        private DateTime _accessed;
        private readonly IDictionary<string, object> _vars = new Dictionary<string, object>();
        private bool _changed = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">A unique id used by the sessions store to identify the session</param>
        public MemorySession(string id)
        {
            _id = id;
        }
        
        /// <summary>
        /// Id
        /// </summary>
        /// <param name="id"></param>
        internal void SetId(string id)
        {
            _id = id;
        }
        #region IHttpSession Members

        /// <summary>
        /// Session id
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Should 
        /// </summary>
        /// <param name="name">Name of the session variable</param>
        /// <returns><c>null</c> if it's not set</returns>
        public object this[string name]
        {
            get
            {
				if (_vars.ContainsKey(name))
				{
					_accessed = DateTime.Now;
					return _vars[name];
				}
				
				return null;
            }
            set
            {
                if (_vars.ContainsKey(name))
                    _vars[name] = value;
                else
                    _vars.Add(name, value);

                _changed = true;
                _accessed = DateTime.Now;
            }
        }

        /// <summary>
        /// when the session was last accessed.
        /// </summary>
        /// <remarks>
        /// Used to determine when the session should be removed.
        /// </remarks>
        public DateTime Accessed
        {
            get { return _accessed; }
            set { _accessed = value; }
        }

        /// <summary>
        /// Number of values in the session
        /// </summary>
        public int Count
        {
            get { return _vars.Count; }
        }

        /// <summary>
        /// Flag to indicate that the session have been changed
        /// and should be saved into the session store.
        /// </summary>
        public bool Changed
        {
            get { return _changed; }
            set { _changed = value; }
        }

        /// <summary>
        /// Remove everything from the session
        /// </summary>
        public void Clear()
        {
			Clear(false);
        }

		/// <summary>
		/// Clears the specified expire.
		/// </summary>
		/// <param name="expires">True if the session is cleared due to expiration</param>
		public void Clear(bool expires)
		{
			BeforeClear(this, new HttpSessionClearedArgs(expires));

			_vars.Clear();
			_accessed = DateTime.Now;
			_changed = true;	
		}

    	///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

    	/// <summary>
    	/// Event triggered upon clearing the session
    	/// </summary>
    	public event HttpSessionClearedHandler BeforeClear = delegate{};

    	/// <summary>
    	/// Gets keys for all values.
    	/// </summary>
    	public IEnumerable<string> Keys
    	{
			get { return _vars.Keys; }
    	}

    	#endregion
    }
}
