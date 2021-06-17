/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Common
{
    public class WebRequestHeadersHelper
    {
        const string CONTENTTYPE = "Content-Type";
        const string MUTIPARTFORMDATA = "multipart/form-data";
        const string FORMURLENCODED = "application/x-www-form-urlencoded";

        private readonly IList<INameValue> _notEvaluated;
        private readonly List<INameValue> _evaluatedHeaders;

        public WebRequestHeadersHelper(IList<INameValue> notEvaluatedHeaders, IEnumerable<INameValue> evaluatedHeaders)
        {
            _notEvaluated = notEvaluatedHeaders;
            _evaluatedHeaders = evaluatedHeaders.ToList();
        }
        
        public INameValue FormUrlEncodedContentType
        {
            get
            {
                return _evaluatedHeaders.FirstOrDefault(o => o.Name == CONTENTTYPE && o.Value.Contains(FORMURLENCODED));
            }
        }

        public bool IsFormUrlEncodedContentTypeExist => FormUrlEncodedContentType != null;

        public bool IsFormUrlEncodedContentTypeIncomplete => IsFormUrlEncodedContentTypeExist && !FormUrlEncodedContentType.Value.Contains("boundary=");

        public INameValue FormDataContentType
        {
            get
            {
                return _evaluatedHeaders.FirstOrDefault(o => o.Name == CONTENTTYPE && o.Value.Contains(MUTIPARTFORMDATA));
            }
        }

        public bool IsFormDataContentTypeExist => FormDataContentType != null;

        public bool IsFormDataContentTypeIncomplete => IsFormDataContentTypeExist && !FormDataContentType.Value.Contains("boundary=");

        internal void AddFormDataContentType()
        {
            var addItem = new NameValue(CONTENTTYPE, GenerateFormDataContentType());
            _evaluatedHeaders.Add(addItem);
            _notEvaluated.Add(addItem);
        }

        internal void AddFormUrlEncodedContentType()
        {
            var addItem = new NameValue(CONTENTTYPE, GenerateFormUrlEncodedContentType());
            _evaluatedHeaders.Add(addItem);
            _notEvaluated.Add(addItem);
        }

        private static string GenerateFormDataContentType()
        {
            return MUTIPARTFORMDATA + "; boundary=" + $"----------{Guid.NewGuid():N}";
        }

        private static string GenerateFormUrlEncodedContentType()
        {
            return FORMURLENCODED + "; boundary=" + $"----------{Guid.NewGuid():N}";
        }

        private void AppendBoundaryToFormDataContentType()
        {
            var formDataContentType = FormDataContentType;
            _notEvaluated.Remove(new NameValue(formDataContentType.Name, formDataContentType.Value));
            _evaluatedHeaders.Remove(new NameValue(formDataContentType.Name, formDataContentType.Value));

            var addItem = new NameValue(CONTENTTYPE, GenerateFormDataContentType());
            _evaluatedHeaders.Add(addItem);
            _notEvaluated.Add(addItem);
        }

        private void AppendBoundaryToFormUrlEncodedContentType()
        {
            var formDataContentType = FormUrlEncodedContentType;
            _notEvaluated.Remove(new NameValue(formDataContentType.Name, formDataContentType.Value));
            _evaluatedHeaders.Remove(new NameValue(formDataContentType.Name, formDataContentType.Value));

            var addItem = new NameValue(CONTENTTYPE, GenerateFormUrlEncodedContentType());
            _evaluatedHeaders.Add(addItem);
            _notEvaluated.Add(addItem);
        }

        public IEnumerable<INameValue> CalculateFormDataContentType()
        {
            if(IsFormDataContentTypeExist)
            {
                if(IsFormDataContentTypeIncomplete)
                {
                    AppendBoundaryToFormDataContentType();
                }
            }
            else if(IsFormUrlEncodedContentTypeExist)
            {
                if(IsFormUrlEncodedContentTypeIncomplete)
                {
                    AppendBoundaryToFormUrlEncodedContentType();
                }
            }
            else
            {
                AddFormDataContentType();
            }

            return _evaluatedHeaders;
        }
    }
}