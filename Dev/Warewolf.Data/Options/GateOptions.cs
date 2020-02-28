/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Data.Decisions.Operations;
using Warewolf.Data.Options.Enums;
using Warewolf.Options;

namespace Warewolf.Data.Options
{
    public class GateOptions
    {
        public GateOptions()
        { }


        [DataValue(nameof(OnResumeBase.Resume))]
        [MultiDataProvider(typeof(Continue), typeof(EndWorkflow))]
        [OptionUX(nameof(OptionRadioButtons))]
        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionGateResumeHelpText))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionGateResumeToolTip))]
        [Orientation(Orientation.Horizontal)]
        public OnResumeBase GateOpts { get; set; } = new Continue();
    }

    public class OnResumeBase
    {
        public GateResumeAction Resume { get; protected set; } = GateResumeAction.Continue;
    }

    public class EndWorkflow : OnResumeBase
    {
        public EndWorkflow()
        {
            Resume = GateResumeAction.EndWorkflow;
        }
    }

    public class Continue : OnResumeBase
    {
        public Continue()
        {
            Resume = GateResumeAction.Continue;
        }

        [DataValue(nameof(RetryAlgorithmBase.RetryAlgorithm))]
        [MultiDataProvider(typeof(NoBackoff), typeof(ConstantBackoff), typeof(LinearBackoff), typeof(FibonacciBackoff))]
        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionGateStrategyHelpText))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionGateStrategyToolTip))]
        public RetryAlgorithmBase Strategy { get; set; } = new NoBackoff();
    }

    public enum GateResumeAction
    {
        [Description("End this workflow")]
        EndWorkflow = 0,
        [Description("Continue")]
        Continue = 1,
    }

    public abstract class RetryAlgorithmBase
    {
        public RetryAlgorithm RetryAlgorithm { get; set; }

        public abstract IEnumerable<bool> Create();
    }

    public class NoBackoff : RetryAlgorithmBase
    {
        public NoBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.NoBackoff;
        }

        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionGateMaxRetriesHelpText))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionGateMaxRetriesToolTip))]
        public int MaxRetries { get; set; } = 3;
        public override IEnumerable<bool> Create()
        {
            for (var i = 0; i < MaxRetries; i++)
            {
                yield return true;
            }

            yield return false;
        }
    }

    public class ConstantBackoff : RetryAlgorithmBase
    {
        public ConstantBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.ConstantBackoff;
        }

        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionGateIncrementHelpText))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionGateIncrementToolTip))]
        public int Increment { get; set; } = 100;

        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionGateMaxRetriesHelpText))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionGateMaxRetriesToolTip))]
        public int MaxRetries { get; set; } = 2;

        public override IEnumerable<bool> Create()
        {
            for (var i = 0; i < MaxRetries; i++)
            {
                Task.Delay(Increment).Wait();
                yield return true;
            }

            yield return false;
        }
    }

    public class LinearBackoff : RetryAlgorithmBase
    {
        public LinearBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.LinearBackoff;
        }

        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionGateIncrementHelpText))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionGateIncrementToolTip))]
        public int Increment { get; set; } = 50;

        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionGateMaxRetriesHelpText))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionGateMaxRetriesToolTip))]
        public int MaxRetries { get; set; } = 2;

        public override IEnumerable<bool> Create()
        {
            for (var i = 0; i < MaxRetries; i++)
            {
                Task.Delay(i * Increment).Wait();
                yield return true;
            }

            yield return false;
        }
    }

    public class FibonacciBackoff : RetryAlgorithmBase
    {
        public FibonacciBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.FibonacciBackoff;
        }

        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionGateMaxRetriesHelpText))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionGateMaxRetriesToolTip))]
        public int MaxRetries { get; set; } = 2;
        public override IEnumerable<bool> Create()
        {
            var increment = 0;
            for (var i = 0; i < MaxRetries; i++)
            {
                Task.Delay(i * increment).Wait();
                yield return true;
            }

            yield return false;
        }
    }

    public class ConditionExpression : IOptionConvertable
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionConditionLeftHelpText))]
        public string Left { get; set; }

        [DataValue(nameof(Condition.MatchType))]
        [MultiDataProvider(typeof(ConditionMatch), typeof(ConditionBetween))]
        public Condition Cond { get; set; }

        public IOption[] ToOptions()
        {
            var option = new OptionConditionExpression
            {
                Left = Left
            };
            Cond?.SetOptions(option);
            SetSelectedMatchType(option);
            return new[] {
                option
            };
        }

        private void SetSelectedMatchType(OptionConditionExpression option)
        {
            var sb = new StringBuilder();
            Cond.MatchType.RenderDescription(sb);
            var item = NamedInt.GetAll(Cond.MatchType.GetType()).First(o => o.Name == sb.ToString());
            option.SelectedMatchType = new NamedInt { Name = item.Name, Value = item.Value };
        }

        public void FromOption(IOption option)
        {
            if (option is OptionConditionExpression optionConditionExpression)
            {
                this.Left = optionConditionExpression.Left;
                if (optionConditionExpression.IsBetween)
                {
                    this.Cond = ConditionBetween.FromOption(optionConditionExpression);
                }
                else
                {
                    this.Cond = ConditionMatch.FromOption(optionConditionExpression);
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
            sb.Append(Left);
            Cond?.RenderDescription(sb);
        }
    }

    public abstract class Condition
    {
        public enDecisionType MatchType { get; set; }

        internal abstract bool Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError);
        public abstract void SetOptions(OptionConditionExpression option);

        public abstract void RenderDescription(StringBuilder sb);

    }
    public class ConditionMatch : Condition
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionConditionRightHelpText))]
        public string Right { get; set; }
        public override void SetOptions(OptionConditionExpression option)
        {
            option.MatchType = MatchType;
            option.Right = Right;
        }

        internal static Condition FromOption(OptionConditionExpression optionConditionExpression)
        {
            return new ConditionMatch
            {
                MatchType = optionConditionExpression.MatchType,
                Right = optionConditionExpression.Right,
            };
        }

        internal override bool Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var factory = Dev2DecisionFactory.Instance();

            if (MatchType == enDecisionType.IsError)
            {
                return hasError;
            }
            if (MatchType == enDecisionType.IsNotError)
            {
                return !hasError;
            }
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

    public class ConditionBetween : Condition
    {
        public string From { get; set; }
        public string To { get; set; }
        public override void SetOptions(OptionConditionExpression option)
        {
            option.MatchType = MatchType;
            option.From = From;
            option.To = To;
        }

        internal static Condition FromOption(OptionConditionExpression optionConditionExpression)
        {
            return new ConditionBetween
            {
                MatchType = optionConditionExpression.MatchType,
                From = optionConditionExpression.From,
                To = optionConditionExpression.To,
            };
        }


        internal override bool Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var factory = Dev2DecisionFactory.Instance();

            if (MatchType == enDecisionType.IsError)
            {
                return hasError;
            }
            if (MatchType == enDecisionType.IsNotError)
            {
                return !hasError;
            }
            IList<bool> ret = new List<bool>();

            var items = getArgumentsFunc(left, From, To);
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
            if (MatchType == enDecisionType.IsBetween)
            {
                sb.Append(" is greater than ");
                sb.Append(From);
                sb.Append(" and less than ");
                sb.Append(To);
            }
            else if (MatchType == enDecisionType.NotBetween)
            {
                sb.Append(" is less than ");
                sb.Append(From);
                sb.Append(" and more than ");
                sb.Append(To);
            }
            else
            {
                sb.Append(" ");
                sb.Append(From);
                MatchType.RenderDescription(sb);
                sb.Append(" ");
                sb.Append(To);
            }
        }
    }

}
