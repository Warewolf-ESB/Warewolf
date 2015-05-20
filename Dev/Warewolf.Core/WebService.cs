using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Warewolf.Core
{
   public class WebService:IWebService, IEquatable<WebService>
   {
       /// <summary>
       /// Initializes a new instance of the <see cref="T:System.Object"/> class.
       /// </summary>
       public WebService()
       {
       }

       string _name;
        string _path;
        IWebServiceSource _source;
        IList<IWebserviceInputs> _inputs;
        IList<IWebserviceOutputs> _outputMappings;
        string _queryString;
        Guid _id;

        #region Implementation of IWebService

       /// <summary>
       /// Initializes a new instance of the <see cref="T:System.Object"/> class.
       /// </summary>
       // ReSharper disable TooManyDependencies
       public WebService(string name, string path, IWebServiceSource source, IList<IWebserviceInputs> inputs, IList<IWebserviceOutputs> outputMappings, string queryString, Guid id)
           // ReSharper restore TooManyDependencies
       {
           _name = name;
           _path = path;
           _source = source;
           _inputs = inputs;
           _outputMappings = outputMappings;
           _queryString = queryString;
           _id = id;
       }

       public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
            }
        }
        public IWebServiceSource Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }
        public IList<IWebserviceInputs> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                _inputs = value;
            }
        }
        public IList<IWebserviceOutputs> OutputMappings
        {
            get
            {
                return _outputMappings;
            }
            set
            {
                _outputMappings = value;
            }
        }
        public string QueryString
        {
            get
            {
                return _queryString;
            }
            set
            {
                _queryString = value;
            }
        }
        public Guid Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        #endregion
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(WebService other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(_source, other._source) && string.Equals(_path, other._path) && string.Equals(_name, other._name) && Equals(_inputs, other._inputs) && Equals(_outputMappings, other._outputMappings) && string.Equals(_queryString, other._queryString);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((WebService)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_source != null ? _source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_path != null ? _path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_name != null ? _name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_inputs != null ? _inputs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_outputMappings != null ? _outputMappings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_queryString != null ? _queryString.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(WebService left, WebService right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WebService left, WebService right)
        {
            return !Equals(left, right);
        }

        #endregion

    }
}
