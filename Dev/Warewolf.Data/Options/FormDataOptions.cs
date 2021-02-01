/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Warewolf.Options;

namespace Warewolf.Data.Options
{
    public class FormDataOptions : IOptionConvertParameter
    {
        public FormDataOptions()
        {

        }

        public void Notify()
        {
            OnChange?.Invoke();
        }
        public delegate void NotifyHandler();
        public event NotifyHandler OnChange;
    }

    public abstract class FormDataCondition
    {
        public enFormDataTableType MatchType { get; set; }

        internal abstract IEnumerable<FormDataParameters> Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError);
        public abstract void SetOptions(FormDataOptionConditionExpression option);

        public abstract void RenderDescription(StringBuilder sb);

    }


    public class FormDataConditionExpression : IOptionConvertable
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.FormDataOptionConditionLeftHelpText))]
        public string Key { get; set; }

        [DataValue(nameof(FormDataCondition.MatchType))]
        [MultiDataProvider(typeof(FormDataConditionMatch), typeof(FormDataConditionBetween))]
        public FormDataCondition Cond { get; set; }

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
        
        public IEnumerable<FormDataParameters> Eval(Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                return new List<FormDataParameters>();
            }
            return Cond.Eval(Key, getArgumentsFunc, hasError);
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
        public override void SetOptions(FormDataOptionConditionExpression option)
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


        internal override IEnumerable<FormDataParameters> Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var ret = new List<FormDataParameters>();

            var items = getArgumentsFunc(left, File, FileName);
            foreach (var arguments in items)
            {
                try
                {
                    ret.Add(new FileParameter(new FormDataConditionExpression
                    {
                        Key = arguments[0],
                        Cond = new FormDataConditionBetween
                        {
                            MatchType = enFormDataTableType.File,
                            File = arguments[1],
                            FileName = arguments[2]
                        }
                    }));
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
        public override void SetOptions(FormDataOptionConditionExpression option)
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


        internal override IEnumerable<FormDataParameters> Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var ret = new List<FormDataParameters>();

            var items = getArgumentsFunc(left, Value, null);
            foreach (var arguments in items)
            {
                try
                {
                    ret.Add(new TextParameter(new FormDataConditionExpression
                    {
                        Key = arguments[0],
                        Cond = new FormDataConditionMatch
                        {
                            Value = arguments[1],
                            MatchType = enFormDataTableType.Text
                        }
                    }));

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
