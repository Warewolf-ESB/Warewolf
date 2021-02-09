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
using Newtonsoft.Json;
using System;

namespace Warewolf.Data.Options
{
    public abstract class FormDataParameters : IFormDataParameters
    {
        private readonly IFormDataConditionExpression _conditionExpression;
        protected readonly IFormDataCondition _formDataCondition;
        protected readonly string _key;

        public FormDataParameters()
        {

        }

        protected FormDataParameters(IFormDataConditionExpression conditionExpression)
        {
            _conditionExpression = conditionExpression;
            _formDataCondition = _conditionExpression.Cond;
            _key = _conditionExpression.Key;
        }

        public string Key { get; set; }
    }

    public class TextParameter : IFormDataParameters
    {
        public TextParameter()
        {

        }

        public string Value { get; set; }
        public string Key { get; set; }
    }

    public class FileParameter : IFormDataParameters
    {
        public FileParameter()
        {
        }

        private byte[] GetFileByteArray()
        {
            var fileString = FileBase64;

            try
            {
                if (string.IsNullOrEmpty(fileString))
                {
                    throw new ArgumentNullException("File Content");
                }
                return Convert.FromBase64String(fileString);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string FileName { get; set; }
        public string ContentType { get; set; }

        [JsonIgnore]
        public byte[] FileBytes => GetFileByteArray();

        public string FileBase64 { get; set; }
        public string Key { get; set; }
    }
}
