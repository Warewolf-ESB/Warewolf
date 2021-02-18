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
        public enFormDataTableType TableType { get; set; }

        public abstract IEnumerable<IFormDataParameters> Eval(string key, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError);
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

        [DataValue(nameof(FormDataCondition.TableType))]
        [MultiDataProvider(typeof(FormDataConditionText), typeof(FormDataConditionFile))]
        public IFormDataCondition Cond { get; set; }

        public IOption[] ToOptions()
        {
            var option = new FormDataOptionConditionExpression
            {
                Key = Key
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
            Cond.TableType.RenderDescription(sb);
            var item = NamedInt.GetAll(Cond.TableType.GetType()).First(o => o.Name == sb.ToString());
            option.SelectedTableType = new NamedInt { Name = item.Name, Value = item.Value };
        }

        public void FromOption(IOption option)
        {
            if (option is FormDataOptionConditionExpression optionConditionExpression)
            {
                this.Key = optionConditionExpression.Key;
                if (optionConditionExpression.IsTripleOperand)
                {
                    this.Cond = FormDataConditionFile.FromOption(optionConditionExpression);
                }
                else
                {
                    this.Cond = FormDataConditionText.FromOption(optionConditionExpression);
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
            if (cond is FormDataConditionText)
            {
                var conditionMatch = cond as FormDataConditionText;
                return new TextParameter
                {
                    Key = this.Key,
                    Value = conditionMatch.Value
                };
            }
            if (cond is FormDataConditionFile)
            {
                var conditionBetween = cond as FormDataConditionFile;
                return new FileParameter
                {
                    Key = this.Key,
                    FileBase64 = conditionBetween.FileBase64,
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

    public class FormDataConditionFile : FormDataCondition
    {
        public string FileBase64 { get; set; }
        public string FileName { get; set; }

        public override void SetOptions(IFormDataOptionConditionExpression option)
        {
            option.TableType = TableType;
            option.FileBase64 = FileBase64;
            option.FileName = FileName;
        }

        internal static FormDataCondition FromOption(FormDataOptionConditionExpression optionConditionExpression)
        {
            return new FormDataConditionFile
            {
                TableType = optionConditionExpression.TableType,
                FileBase64 = optionConditionExpression.FileBase64,
                FileName = optionConditionExpression.FileName,
            };
        }


        public override IEnumerable<IFormDataParameters> Eval(string key, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var ret = new List<IFormDataParameters>();

            var items = getArgumentsFunc(key, FileBase64, FileName);
            foreach (var arguments in items)
            {
                try
                {
                    ret.Add(new FormDataConditionExpression
                    {
                        Key = arguments[0],
                        Cond = new FormDataConditionFile
                        {
                            TableType = enFormDataTableType.File,
                            FileBase64 = arguments[1],
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
            sb.Append(" File Content: ");
            sb.Append(FileBase64);
            sb.Append(" File Name: ");
            sb.Append(FileName);
        }
    }


    public class FormDataConditionText : FormDataCondition
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionConditionRightHelpText))]
        public string Value { get; set; }
        public override void SetOptions(IFormDataOptionConditionExpression option)
        {
            option.TableType = TableType;
            option.Value = Value;
        }

        internal static FormDataCondition FromOption(FormDataOptionConditionExpression optionConditionExpression)
        {
            return new FormDataConditionText
            {
                TableType = optionConditionExpression.TableType,
                Value = optionConditionExpression.Value,
            };
        }


        public override IEnumerable<IFormDataParameters> Eval(string key, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError)
        {
            var ret = new List<IFormDataParameters>();

            var items = getArgumentsFunc(key, Value, null);
            foreach (var arguments in items)
            {
                try
                {
                    ret.Add(new FormDataConditionExpression
                    {
                        Key = arguments[0],
                        Cond = new FormDataConditionText
                        {
                            Value = arguments[1],
                            TableType = enFormDataTableType.Text
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
            TableType.RenderDescription(sb);
            sb.Append(": ");
            sb.Append(Value);
        }
    }
}
