/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebServices;

namespace Warewolf.Core
{
    public class WebServiceDefinition : IWebService
    // ReSharper restore UnusedMember.Global
    {
        WebRequestMethod _method;

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
        public WebRequestMethod Method
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value;
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
            return Equals((Object)other);

        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IWebService other)
        {
            return Equals(other as WebServiceDefinition);
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
            if (obj.GetHashCode() != GetHashCode())
                return false;
            bool eq = true;
            var other = obj as WebServiceDefinition;
            var headers = Headers;
            if (other != null)
            {
                var otherHeaders = other.Headers;
                eq = otherHeaders.EnumerableEquals(headers);
            }
            return other != null && (string.Equals(Name, other.Name) && string.Equals(Path, other.Path) && string.Equals(Method, other.Method) && string.Equals(PostData, other.PostData)) && string.Equals(RequestUrl, other.RequestUrl) && Equals(Source, other.Source) && eq && string.Equals(PostData, other.PostData);
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
                int hashCode = Name?.GetHashCode() ?? 0;

                hashCode = (hashCode * 397) ^ (Path?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Source?.GetHashCode() ?? 0);

                if (OutputMappings != null)
                {
                    hashCode = OutputMappings.Aggregate(hashCode, (a, b) => a * 397 ^ (b?.GetHashCode() ?? 0));
                }
                hashCode = (hashCode * 397) ^ (QueryString?.GetHashCode() ?? 0);
                if (Headers != null)
                {
                    hashCode = Headers.Aggregate(hashCode, (current, nameValue) => (current * 397) ^ (nameValue != null ? nameValue.GetHashCode() : 0));
                }
                hashCode = (hashCode * 397) ^ (PostData?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Method.GetHashCode();
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

    public static class EnumerableEquality
    {

        public static bool EnumerableEquals<T>(this IList<T> otherHeaders, IList<T> headers)
        {
            bool eq = true;
            if (otherHeaders == null && headers == null)
                return true;
            if (otherHeaders != null && headers != null)
            {
                if (otherHeaders.Count == headers.Count)
                {
                    for (int i = 0; i < headers.Count; i++)
                    {
                        if (!headers[i].Equals(otherHeaders[i]))
                        {
                            eq = false;
                        }
                    }
                }
                else
                {
                    eq = false;
                }
            }
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            else if (otherHeaders == null && headers != null)
            {
                return false;
            }
            return eq;
        }
    }
}
