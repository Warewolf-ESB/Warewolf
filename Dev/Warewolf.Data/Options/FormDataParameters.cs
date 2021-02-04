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

namespace Warewolf.Data.Options
{
    public abstract class FormDataParameters : IFormDataParameters
    {
        private readonly IFormDataConditionExpression _conditionExpression;
        protected readonly IFormDataCondition _formDataCondition;
        protected readonly string _key;


        protected FormDataParameters(IFormDataConditionExpression conditionExpression)
        {
            _conditionExpression = conditionExpression;
            _formDataCondition = _conditionExpression.Cond;
            _key = _conditionExpression.Key;
        }

        public string Key { get; set; }
        public IFormDataCondition Cond { get; set; }

    }

    public class TextParameter : FormDataParameters
    {
        private readonly FormDataConditionMatch _conditionMatch;

        public TextParameter(IFormDataConditionExpression conditionExpression)
            : base(conditionExpression)
        {
            _conditionMatch = _formDataCondition as FormDataConditionMatch;

            Key = _key;
            Value = _conditionMatch.Value;
        }

        public string Value { get; internal set; }

    }

    public class FileParameter : FormDataParameters
    {
        private readonly FormDataConditionBetween _conditionBetween;

        public FileParameter(IFormDataConditionExpression conditionExpression)
            : base(conditionExpression)
        {
            _conditionBetween = _formDataCondition as FormDataConditionBetween;

            Key = _key;
            FileName = _conditionBetween.FileName;
            File = GetFileByteArray();
            ContentType = null;
        }

        private byte[] GetFileByteArray()
        {
            var fileString = _conditionBetween.File;

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

        public string FileName { get; internal set; }
        public string ContentType { get; internal set; }
        public byte[] File { get; internal set; }
    }
}
