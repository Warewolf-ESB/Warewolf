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
using System.Text;
using Warewolf.Options;

namespace Warewolf.Data.Options
{
    public abstract class FormDataCondition : IFormDataCondition
    {
        public enFormDataTableType MatchType { get; set; }

        public abstract IEnumerable<IFormDataParameters> Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError);
        public abstract void SetOptions(IFormDataOptionConditionExpression option);

        public abstract void RenderDescription(StringBuilder sb);
    }


    public class FormDataConditionExpression : IOptionConvertable, IFormDataConditionExpression
    {
        private string _key;

        [HelpText(nameof(Studio.Resources.Languages.HelpText.FormDataOptionConditionLeftHelpText))]
        public string Key
        {
            get => _key;
            set => _key = value;
        }

        [DataValue(nameof(FormDataCondition.MatchType))]
        [MultiDataProvider(typeof(FormDataConditionMatch), typeof(FormDataConditionBetween))]
        public IFormDataCondition Cond { get; set; }

        public IOption[] ToOptions()
        {
            var option = new FormDataOptionConditionExpression
            {
                Left = Key
            };
            Cond?.SetOptions(option);
            SetSelectedMatchType(option);
            return new[] {
                option
            };
        }

        private void SetSelectedMatchType(FormDataOptionConditionExpression option)
        {
            var sb = new StringBuilder();
            Cond.MatchType.RenderDescription(sb);
            var item = NamedInt.GetAll(Cond.MatchType.GetType()).First(o => o.Name == sb.ToString());
            option.SelectedMatchType = new NamedInt { Name = item.Name, Value = item.Value };
        }

        public void FromOption(IOption option)
        {
            if (option is FormDataOptionConditionExpression optionConditionExpression)
            {
                this.Key = optionConditionExpression.Left;
                if (optionConditionExpression.IsBetween)
                {
                    this.Cond = FormDataConditionBetween.FromOption(optionConditionExpression);
                }
                else
                {
                    this.Cond = FormDataConditionMatch.FromOption(optionConditionExpression);
                }
            }
        }
        
        public IEnumerable<IFormDataParameters> Eval(Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                return new List<IFormDataParameters>();
            }
            return Cond.Eval(Key, getArgumentsFunc, hasError);
        }

        public IFormDataParameters ToFormDataParameter()
        {
            var cond = this.Cond;
            if (cond is FormDataConditionMatch)
            {
                var conditionMatch = cond as FormDataConditionMatch;
                return new TextParameter
                {
                    Key = this.Key,
                    Value = conditionMatch.Value
                };
            }
            if (cond is FormDataConditionBetween)
            {
                var conditionBetween = cond as FormDataConditionBetween;
                return new FileParameter
                {
                    Key = this.Key,
                    FileBase64 = conditionBetween.File,
                    FileName = conditionBetween.FileName
                };
            }
            return null;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            RenderDescription(sb);
            return sb.ToString();
        }

        public void RenderDescription(StringBuilder sb)
        {
            sb.Append("Key: "+Key);
            Cond?.RenderDescription(sb);
        }
    }

    public class FormDataConditionBetween : FormDataCondition
    {
        public string File { get; set; }
        public string FileName { get; set; }
        public override void SetOptions(IFormDataOptionConditionExpression option)
        {
            option.MatchType = MatchType;
            option.File = File;
            option.FileName = FileName;
        }

        internal static FormDataCondition FromOption(FormDataOptionConditionExpression optionConditionExpression)
        {
            return new FormDataConditionBetween
            {
                MatchType = optionConditionExpression.MatchType,
                File = optionConditionExpression.File,
                FileName = optionConditionExpression.FileName,
            };
        }


        public override IEnumerable<IFormDataParameters> Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var ret = new List<IFormDataParameters>();

            var items = getArgumentsFunc(left, File, FileName);
            foreach (var arguments in items)
            {
                try
                {
                    ret.Add(new FormDataConditionExpression
                    {
                        Key = arguments[0],
                        Cond = new FormDataConditionBetween
                        {
                            MatchType = enFormDataTableType.File,
                            File = arguments[1],
                            FileName = arguments[2]
                        }
                    }.ToFormDataParameter());
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return ret.Select(o => o).ToList();
        }

        public override void RenderDescription(StringBuilder sb)
        {
            if (MatchType == enFormDataTableType.File)
            {
                sb.Append(" File Content: ");
                sb.Append(File);
                sb.Append(" File Name: ");
                sb.Append(FileName);
            }
            else
            {
                sb.Append(" ");
                sb.Append(File);
                MatchType.RenderDescription(sb);
                sb.Append(" ");
                sb.Append(FileName);
            }
        }
    }


    public class FormDataConditionMatch : FormDataCondition
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionConditionRightHelpText))]
        public string Value { get; set; }
        public override void SetOptions(IFormDataOptionConditionExpression option)
        {
            option.MatchType = MatchType;
            option.Right = Value;
        }

        internal static FormDataCondition FromOption(FormDataOptionConditionExpression optionConditionExpression)
        {
            return new FormDataConditionMatch
            {
                MatchType = optionConditionExpression.MatchType,
                Value = optionConditionExpression.Right,
            };
        }


        public override IEnumerable<IFormDataParameters> Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var ret = new List<IFormDataParameters>();

            var items = getArgumentsFunc(left, Value, null);
            foreach (var arguments in items)
            {
                try
                {
                    ret.Add(new FormDataConditionExpression
                    {
                        Key = arguments[0],
                        Cond = new FormDataConditionMatch
                        {
                            Value = arguments[1],
                            MatchType = enFormDataTableType.Text
                        }
                    }.ToFormDataParameter());

                }
                catch (Exception)
                {
                    throw;
                }
            }
            return ret.Select(o => o).ToList();
        }

        public override void RenderDescription(StringBuilder sb)
        {
            sb.Append(" ");
            MatchType.RenderDescription(sb);
            sb.Append(" ");
            sb.Append(Value);
        }
    }
}
