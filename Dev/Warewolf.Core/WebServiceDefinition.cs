using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        // ReSharper disable TooManyDependencies
        public WebServiceDefinition(string name, string path, IWebServiceSource source, IList<IServiceInput> inputs, IList<IServiceOutputMapping> outputMappings, string queryString, Guid id)
            // ReSharper restore TooManyDependencies
        {
            Name = name;
            Path = path;
            Source = source;
            Inputs = inputs;
            OutputMappings = outputMappings;
            QueryString = queryString;
            Id = id;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public IWebServiceSource Source { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public IList<IServiceOutputMapping> OutputMappings { get; set; }
        public string QueryString { get; set; }
        public string RequestUrl { get; set; }
        public Guid Id { get; set; }
        public List<NameValue> Headers { get; set; }
        public string PostData { get; set; }
        public string SourceUrl { get; set; }
        public string Response { get; set; }

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
            return string.Equals(Name, other.Name) && string.Equals(Path, other.Path) && Equals(Source, other.Source) && Equals(OutputMappings, other.OutputMappings) && string.Equals(QueryString, other.QueryString) && Equals(Headers, other.Headers) && string.Equals(PostData, other.PostData);
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
                int hashCode = (Name != null ? Name.GetHashCode() : 0);
              
                hashCode = (hashCode * 397) ^ (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputMappings != null ? OutputMappings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (QueryString != null ? QueryString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Headers != null ? Headers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PostData != null ? PostData.GetHashCode() : 0);
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
