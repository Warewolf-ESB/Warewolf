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

        private readonly IList<INameValue> _notEvaluated;
        private List<INameValue> _evaluatedHeaders;

        public WebRequestHeadersHelper(IList<INameValue> notEvaluatedHeaders, IEnumerable<INameValue> evaluatedHeaders)
        {
            _notEvaluated = notEvaluatedHeaders;
            _evaluatedHeaders = evaluatedHeaders.ToList();
        }

        public INameValue ContentType
        {
            get
            {
                return _evaluatedHeaders.FirstOrDefault(o => o.Name == CONTENTTYPE && o.Value.Contains("multipart/form-data"));
            }
        }

        public bool IsFormDataContentTypeExist
        {
            get
            {
                return ContentType != null;
            }
        }

        public bool IsFormDataContentTypeIncomplete
        {
            get
            {
                return IsFormDataContentTypeExist && !ContentType.Value.Contains("boundary=");
            }
        }

        internal void AddFormDataContentType()
        {
            var addItem = new NameValue(CONTENTTYPE, GenerateFormDataContentType());
            _evaluatedHeaders.Add(addItem);
            _notEvaluated.Add(addItem);
        }

        private static string GenerateFormDataContentType()
        {
            return MUTIPARTFORMDATA + "; boundary=" + string.Format("----------{0:N}", Guid.NewGuid());
        }

        private void AppendBoundaryToFormDataContentType()
        {
            var formDataContentType = ContentType;
            _notEvaluated.Remove(new NameValue(formDataContentType.Name, formDataContentType.Value));
            _evaluatedHeaders.Remove(new NameValue(formDataContentType.Name, formDataContentType.Value));

            var addItem = new NameValue(CONTENTTYPE, GenerateFormDataContentType());
            _evaluatedHeaders.Add(addItem);
            _notEvaluated.Add(addItem);
        }

        public IEnumerable<INameValue> CalculateFormDataContentType()
        {
            if (IsFormDataContentTypeExist)
            {
                if (IsFormDataContentTypeIncomplete)
                {
                    AppendBoundaryToFormDataContentType();
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
