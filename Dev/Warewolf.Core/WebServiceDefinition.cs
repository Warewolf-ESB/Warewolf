using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Dev2.Common.Interfaces.WebServices;

namespace Warewolf.Core
{
    // ReSharper disable UnusedMember.Global
    public class WebServiceDefinition : IWebService, IEquatable<WebServiceDefinition>
        // ReSharper restore UnusedMember.Global
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public WebServiceDefinition()
        {
        }

        string _name;
        string _path;
        IWebServiceSource _source;
        IList<IServiceOutputMapping> _outputMappings;
        string _queryString;
        Guid _id;

        IList<INameValue> _headers;
        string _postData;
        string _requestUrl;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        // ReSharper disable TooManyDependencies
        public WebServiceDefinition(string name, string path, IWebServiceSource source, IList<IServiceInput> inputs, IList<IServiceOutputMapping> outputMappings, string queryString, Guid id)
            // ReSharper restore TooManyDependencies
        {
            _name = name;
            _path = path;
            _source = source;
            Inputs = inputs;
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
        public IList<IServiceInput> Inputs { get; set; }
        public IList<IServiceOutputMapping> OutputMappings
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
        public string RequestUrl
        {
            get
            {
                return _requestUrl;
            }
            set
            {
                _requestUrl = value;
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
        public IList<INameValue> Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
            }
        }
        public string PostData
        {
            get
            {
                return _postData;
            }
            set
            {
                _postData = value;
            }
        }

        #region Equality members

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(WebServiceDefinition other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_name, other._name) && string.Equals(_path, other._path) && Equals(_source, other._source) && Equals(_outputMappings, other._outputMappings) && string.Equals(_queryString, other._queryString) && Equals(_headers, other._headers) && string.Equals(_postData, other._postData);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((WebServiceDefinition)obj);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyFieldInGetHashCode
                int hashCode = (_name != null ? _name.GetHashCode() : 0);
              
                hashCode = (hashCode * 397) ^ (_path != null ? _path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_source != null ? _source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_outputMappings != null ? _outputMappings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_queryString != null ? _queryString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_headers != null ? _headers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_postData != null ? _postData.GetHashCode() : 0);
                return hashCode;
                // ReSharper restore NonReadonlyFieldInGetHashCode
            }
        }

        public static bool operator ==(WebServiceDefinition left, WebServiceDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WebServiceDefinition left, WebServiceDefinition right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
