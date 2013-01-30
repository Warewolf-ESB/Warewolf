using System;
using Dev2.DynamicServices;

// ReSharper disable CheckNamespace
namespace Dev2
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Represents a duplicate exception.
    /// </summary>
    public class DuplicateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateException" /> class.
        /// </summary>
        /// <param name="dso">The service object that threw the error.</param>
        public DuplicateException(IDynamicServiceObject dso)
            : base(string.Format("Service '{0}' already exists", dso.Name))
        {
            ServiceObject = dso;
        }

        /// <summary>
        /// Gets the service object.
        /// </summary>
        public IDynamicServiceObject ServiceObject
        {
            get;
            private set;
        }
    }
}
