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
using System.Text;

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

        [JsonIgnore]
        public bool IsEmptyRow { get; }
        
        [JsonIgnore]
        public bool IsIncompleteRow { get; }

        public abstract void RenderDescription(StringBuilder text);
    }

    public class TextParameter : IFormDataParameters
    {
        public string Value { get; set; }
        public string Key { get; set; }

        [JsonIgnore]
        public bool IsEmptyRow 
        {
            get 
            {
                var isEmptyRow = string.IsNullOrEmpty(Key);
                isEmptyRow &= string.IsNullOrEmpty(Value);
                return isEmptyRow;
            }
        }

        [JsonIgnore]
        public bool IsIncompleteRow
        {
            get
            {
                var isEmptyRow = string.IsNullOrEmpty(Key);
                isEmptyRow |= string.IsNullOrEmpty(Value);
                return isEmptyRow;
            }
        }

        public void RenderDescription(StringBuilder text)
        {
            text.Append("Key: " + Key);
            text.Append(" ");
            text.Append("Text:");
            text.Append(" ");
            text.Append(Value);
        }
    }

    public class FileParameter : IFormDataParameters
    {
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

        public void RenderDescription(StringBuilder text)
        {
            text.Append("Key: " + Key);
            text.Append(" File Content: ");
            text.Append(FileBase64);
            text.Append(" File Name: ");
            text.Append(FileName);
        }

        public string FileName { get; set; }
        public string ContentType { get; set; }

        [JsonIgnore]
        public byte[] FileBytes => GetFileByteArray();

        public string FileBase64 { get; set; }
        public string Key { get; set; }

        [JsonIgnore]
        public bool IsEmptyRow
        {
            get
            {
                var isEmptyRow = string.IsNullOrEmpty(Key);
                isEmptyRow &= string.IsNullOrEmpty(FileName);
                isEmptyRow &= string.IsNullOrEmpty(FileBase64);
                return isEmptyRow;
            }
        }

        [JsonIgnore]
        public bool IsIncompleteRow
        {
            get
            {
                var isIncompleteRow = string.IsNullOrEmpty(Key);
                isIncompleteRow |= string.IsNullOrEmpty(FileName);
                isIncompleteRow |= string.IsNullOrEmpty(FileBase64);
                return isIncompleteRow;
            }
        }
    }
}
