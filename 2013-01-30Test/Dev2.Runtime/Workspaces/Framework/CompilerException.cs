using System;
using Dev2.DynamicServices;

// ReSharper disable CheckNamespace
namespace Dev2
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Represents a compiler exception.
    /// </summary>
    public class CompilerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerException" /> class.
        /// </summary>
        /// <param name="dso">The service object that threw the error.</param>
        public CompilerException(IDynamicServiceObject dso)
            : base(string.Format("Service '{0}' failed compilation", dso.Name))
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
