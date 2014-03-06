using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Globalization;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A conditional format rule which will display icons in the cell when the condition is met.
    /// </summary>
    public class IconConditionalFormatRule : ConditionalFormattingRuleBase
    {
        #region Members

        ObservableCollection<ConditionalFormatIcon> _rules;

        #endregion // Members

        #region Properties

        #region Rules

        /// <summary>
        /// Get / set a collection of <see cref="ConditionalFormatIcon"/> objects which will be applied by this rule.
        /// </summary>
        public ObservableCollection<ConditionalFormatIcon> Rules
        {
            get
            {
                if (this._rules == null)
                {
                    this._rules = new ObservableCollection<ConditionalFormatIcon>();
                    this._rules.CollectionChanged += Rules_CollectionChanged;
                }
                return this._rules;
            }
            set
            {
                if (this._rules != null)
                {
                    this._rules.CollectionChanged -= Rules_CollectionChanged;

                    foreach (ConditionalFormatIcon icon in this._rules)
                    {
                        icon.PropertyChanged -= Icon_PropertyChanged;
                        icon.Parent = null;
                    }
                }
                this._rules = value;
                if (this._rules != null)
                {
                    foreach (ConditionalFormatIcon icon in this._rules)
                    {
                        icon.Parent = this;
                        icon.PropertyChanged += Icon_PropertyChanged;
                    }

                    this._rules.CollectionChanged += Rules_CollectionChanged;
                }
            }
        }

        #endregion // Rules

        #endregion // Properties

        #region Overrides
        /// <summary>
        /// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override IConditionalFormattingRuleProxy CreateProxy()
        {
            return new IconConditionalFormatRuleProxy();
        }
        #endregion // Overrides

        #region EventHandlers

        #region Rules_CollectionChanged

        void Rules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (ConditionalFormatIcon icon in e.NewItems)
                    {
                        icon.Parent = this;
                        icon.PropertyChanged += Icon_PropertyChanged;
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (ConditionalFormatIcon icon in e.OldItems)
                    {
                        icon.Parent = null;
                        icon.PropertyChanged -= Icon_PropertyChanged;
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                if (e.OldItems != null)
                {
                    foreach (ConditionalFormatIcon icon in e.OldItems)
                    {
                        icon.Parent = null;
                        icon.PropertyChanged -= Icon_PropertyChanged;
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (ConditionalFormatIcon icon in e.OldItems)
                    {
                        icon.Parent = null;
                        icon.PropertyChanged -= Icon_PropertyChanged;
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (ConditionalFormatIcon icon in e.NewItems)
                    {
                        icon.Parent = this;
                        icon.PropertyChanged += Icon_PropertyChanged;
                    }
                }
            }

            this.OnPropertyChanged("Rules");
        }

        #endregion // Rules_CollectionChanged

        #region Icon_PropertyChanged

        void Icon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        #endregion // Icon_PropertyChanged

        #endregion // EventHandlers
    }

    /// <summary>
    /// The execution proxy for the <see cref="IconConditionalFormatRule"/>.
    /// </summary>
    public class IconConditionalFormatRuleProxy : ConditionalFormattingRuleBaseProxy
    {
        #region Members
        PercentileData _percentileData;
        PercentData _percentData;
        #endregion // Members

        #region Methods

        #region GatherData
        /// <summary>
        /// Called at the stage provided by <see cref="IRule.RuleExecution"/> so that values can be derived for formatting purposes.
        /// </summary>
        /// <param name="query"></param>
        protected override void GatherData(IQueryable query)
        {
            IconConditionalFormatRule rule = (IconConditionalFormatRule)this.Parent;
            


            bool needPercentileData = false;
            bool needPercentData = false;

            foreach (ConditionalFormatIcon icon in rule.Rules)
            {
                if (icon.ValueType == IconRuleValueType.Percent)
                    needPercentData = true;
                else if (icon.ValueType == IconRuleValueType.Percentile)
                    needPercentileData = true;
            }

            if (needPercentileData)
            {
                this._percentileData = PercentileData.CreateGenericPercentileData(rule.Column.ColumnLayout.ObjectDataTypeInfo,
                                                                        rule.Column.Key,
                                                                        rule.Column.DataType == typeof(DateTime),
                                                                        query);
            }

            if (needPercentData)
            {
                this._percentData = PercentData.CreatePercentData(rule.Column.ColumnLayout.ObjectDataTypeInfo,
                                                                        rule.Column.Key,
                                                                        query);
            }
        }
        #endregion // GatherData

        #region EvaluateCondition

        /// <summary>
        /// Determines whether the inputted value meets the condition of <see cref="ConditionalFormattingRuleBase"/> and returns the style 
        /// that should be applied.
        /// </summary>
        /// <param name="sourceDataObject"></param>
        /// <param name="sourceDataValue"></param>
        /// <returns></returns>
        protected override Style EvaluateCondition(object sourceDataObject, object sourceDataValue)
        {
            if (sourceDataValue == null)
            {
                return null;
            }

            IconConditionalFormatRule rule = (IconConditionalFormatRule)this.Parent;

            int rulesCount = rule.Rules.Count - 1;

            foreach (ConditionalFormatIcon icon in rule.Rules)
            {
                double? value = null;

                if (sourceDataValue == null)
                    return null;

                switch (icon.ValueType)
                {
                    case IconRuleValueType.Number:
                        {
                            value = Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture);
                            break;
                        }
                    case IconRuleValueType.Percentile:
                        {
                            if (this._percentileData != null)
                            {
                                value = this._percentileData.GetPercentileValue(sourceDataValue);
                            }
                            break;
                        }
                    case IconRuleValueType.Percent:
                        {
                            if (this._percentData != null && this._percentData.Range > 0)
                            {
                                value = this._percentData.GetPercentValue(Convert.ToDouble(sourceDataValue, CultureInfo.InvariantCulture));
                            }
                            break;
                        }
                }

                if (value != null && icon.PassesConditions((double)value) || rule.Rules.IndexOf(icon) == rulesCount)
                {
                    return icon.GenerateStyle();
                }
            }

            return null;
        }

        #endregion // EvaluateCondition

        #endregion // Methods
    }
}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved