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
using Warewolf.Data.Decisions.Operations;
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

        internal abstract bool Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError);
        public abstract void SetOptions(FormDataOptionConditionExpression option);

        public abstract void RenderDescription(StringBuilder sb);

    }


    public class FormDataConditionExpression : IOptionConvertable
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.FormDataOptionConditionLeftHelpText))]
        public string Left { get; set; }

        [DataValue(nameof(FormDataCondition.MatchType))]
        [MultiDataProvider(typeof(FormDataConditionMatch), typeof(FormDataConditionBetween))]
        public FormDataCondition Cond { get; set; }

        public IOption[] ToOptions()
        {
            var option = new FormDataOptionConditionExpression
            {
                Left = Left
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
                this.Left = optionConditionExpression.Left;
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
        public bool Eval(Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            if (string.IsNullOrWhiteSpace(Left))
            {
                return true;
            }
            return Cond.Eval(Left, getArgumentsFunc, hasError);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            RenderDescription(sb);
            return sb.ToString();
        }

        public void RenderDescription(StringBuilder sb)
        {
            sb.Append("Key: "+Left);
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


        internal override bool Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var factory = Dev2WebPostFormDataDecisionFactory.Instance();

            IList<bool> ret = new List<bool>();

            var items = getArgumentsFunc(left, File, FileName);
            foreach (var arguments in items)
            {
                try
                {
                    ret.Add(factory.FetchDecisionFunction(MatchType).Invoke(arguments));
                }
                catch (Exception)
                {
                    ret.Add(false);
                }
            }
            return ret.All(o => o);
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
        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionConditionRightHelpText))] //TODO: change this to point on the corrent helptext
        public string Right { get; set; }
        public override void SetOptions(FormDataOptionConditionExpression option)
        {
            option.MatchType = MatchType;
            option.Right = Right;
        }

        internal static FormDataCondition FromOption(FormDataOptionConditionExpression optionConditionExpression)
        {
            return new FormDataConditionMatch
            {
                MatchType = optionConditionExpression.MatchType,
                Right = optionConditionExpression.Right,
            };
        }


        internal override bool Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var factory = Dev2WebPostFormDataDecisionFactory.Instance();

            IList<bool> ret = new List<bool>();

            var items = getArgumentsFunc(left, Right, null);
            foreach (var arguments in items)
            {
                try
                {
                    ret.Add(factory.FetchDecisionFunction(MatchType).Invoke(arguments));

                }
                catch (Exception)
                {
                    ret.Add(false);
                }
            }
            return ret.All(o => o);
        }

        public override void RenderDescription(StringBuilder sb)
        {
            sb.Append(" ");
            MatchType.RenderDescription(sb);
            if (MatchType.IsSingleOperand())
            {
                return;
            }
            sb.Append(" ");
            sb.Append(Right);
        }
    }
}
