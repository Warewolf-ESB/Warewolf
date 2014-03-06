using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A class that can be used as a base class for conditional formatting rules which provides 
	/// implementations of base properties.
	/// </summary>
    public abstract class ConditionalFormattingRuleBase : DependencyObjectNotifier, IConditionalFormattingRule, IProvidePropertyPersistenceSettings
	{
		#region Static

		#region CreateStyle
		/// <summary>
		/// Creates a <see cref="CellControl"/> style object with setters for the BackgroundProperty and AltBackgroundProperty
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		protected internal static Style CreateStyle(Color color)
		{
            Style newBackgroundColorStyle = new Style(typeof(ConditionalFormattingCellControl));
			Brush newBrush = new SolidColorBrush(color);
			newBackgroundColorStyle.Setters.Add(new Setter(CellControl.BackgroundProperty, newBrush));
			newBackgroundColorStyle.Setters.Add(new Setter(ConditionalFormattingCellControl.AltBackgroundProperty, new SolidColorBrush(color)));
			return newBackgroundColorStyle;
		}
		#endregion // CreateStyle

		#region CalculateColorSlope

		/// <summary>
		/// Calculates the slope of a line.
		/// </summary>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <param name="minColor"></param>
		/// <param name="maxColor"></param>
		/// <returns></returns>
		protected internal static double CalculateColorSlope(double minValue, double maxValue, Byte minColor, Byte maxColor)
		{
			double denominator = (maxValue - minValue);
			if (denominator == 0)
				return 0;
			return (maxColor - minColor) / denominator;
		}

		#endregion // CalculateColorSlope

		#region CalculateYIntercept

		/// <summary>
		/// Calculates the Y intercept of a line
		/// </summary>
		/// <param name="slope"></param>
		/// <param name="value"></param>
		/// <param name="colorValue"></param>
		/// <returns></returns>
		protected internal static double CalculateYIntercept(double slope, double value, byte colorValue)
		{
			return colorValue - slope * value;
		}

		#endregion // CalculateYIntercept

		#endregion // Static

		#region Properties

		#region Column
		/// <summary>
		/// Gets the Column that this <see cref="ConditionalFormattingRuleBase"/> is tied to.
		/// </summary>
		public Column Column
		{
			get;
			protected internal set;
		}
		#endregion // Column

		#region IsTerminalRule

		/// <summary>
		/// Identifies the <see cref="IsTerminalRule"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsTerminalRuleProperty = DependencyProperty.Register("IsTerminalRule", typeof(bool), typeof(ConditionalFormattingRuleBase), new PropertyMetadata(new PropertyChangedCallback(IsTerminalRuleChanged)));

		/// <summary>
		/// Get / set whether to stop evaluating rules if this rule will apply a style.
		/// </summary>
		public bool IsTerminalRule
		{
			get { return (bool)this.GetValue(IsTerminalRuleProperty); }
			set { this.SetValue(IsTerminalRuleProperty, value); }
		}

		private static void IsTerminalRuleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingRuleBase ruleBase = (ConditionalFormattingRuleBase)obj;
			ruleBase.OnPropertyChanged("IsTerminalRule");
		}

		#endregion // IsTerminalRule

		#region StyleScope

		/// <summary>
		/// Identifies the <see cref="StyleScope"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty StyleScopeProperty = DependencyProperty.Register("StyleScope", typeof(StyleScope), typeof(ConditionalFormattingRuleBase), new PropertyMetadata(new PropertyChangedCallback(StyleScopeChanged)));

		/// <summary>
		/// Get / set whether the style applied by this rule should effect only a single cell or all cells in the row.
		/// </summary>
		public StyleScope StyleScope
		{
			get { return (StyleScope)this.GetValue(StyleScopeProperty); }
			set { this.SetValue(StyleScopeProperty, value); }
		}

		private static void StyleScopeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingRuleBase ruleBase = (ConditionalFormattingRuleBase)obj;
			ruleBase.OnPropertyChanged("StyleScope");
		}

		#endregion // StyleScope

		#region ShouldRefreshOnDataChange

		/// <summary>
		/// Identifies the <see cref="ShouldRefreshOnDataChange"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ShouldRefreshOnDataChangeProperty = DependencyProperty.Register("ShouldRefreshOnDataChange", typeof(bool), typeof(ConditionalFormattingRuleBase), new PropertyMetadata(new PropertyChangedCallback(ShouldRefreshOnDataChangeChanged)));

		/// <summary>
		/// Get / set whether this rule needs to reaquire data due to cell value being changed.
		/// </summary>
		public bool ShouldRefreshOnDataChange
		{
			get { return (bool)this.GetValue(ShouldRefreshOnDataChangeProperty); }
			set { this.SetValue(ShouldRefreshOnDataChangeProperty, value); }
		}

		private static void ShouldRefreshOnDataChangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingRuleBase ruleBase = (ConditionalFormattingRuleBase)obj;
			ruleBase.OnPropertyChanged("ShouldRefreshOnDataChange");
		}

		#endregion // ShouldRefreshOnDataChange

		#region CellValueVisibility

		/// <summary>
		/// Identifies the <see cref="CellValueVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty CellValueVisibilityProperty = DependencyProperty.Register("CellValueVisibility", typeof(Visibility), typeof(ConditionalFormattingRuleBase), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(CellValueVisibilityChanged)));

		/// <summary>
		/// Get / set if the text data should be displayed in the cell along with the style change.
		/// </summary>
		public Visibility CellValueVisibility
		{
			get { return (Visibility)this.GetValue(CellValueVisibilityProperty); }
			set { this.SetValue(CellValueVisibilityProperty, value); }
		}

		private static void CellValueVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingRuleBase ruleBase = (ConditionalFormattingRuleBase)obj;
			ruleBase.OnPropertyChanged("CellValueVisibility");
		}

		#endregion // CellValueVisibility

		#region RuleExecution

		/// <summary>
		/// Identifies the <see cref="RuleExecution"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RuleExecutionProperty = DependencyProperty.Register("RuleExecution", typeof(EvaluationStage), typeof(ConditionalFormattingRuleBase), new PropertyMetadata(EvaluationStage.PriorToFilteringAndPaging, new PropertyChangedCallback(RuleExecutionChanged)));

		/// <summary>
		/// Get / set at what stage to gather data for conditional formatting.
		/// </summary>
		public EvaluationStage RuleExecution
		{
			get { return (EvaluationStage)this.GetValue(RuleExecutionProperty); }
			set { this.SetValue(RuleExecutionProperty, value); }
		}

		private static void RuleExecutionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingRuleBase ruleBase = (ConditionalFormattingRuleBase)obj;
			ruleBase.OnPropertyChanged("RuleExecution");
		}

		#endregion // RuleExecution

		#endregion // Properties

		#region Methods

		#region CreateProxy
		/// <summary>
		/// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
		/// </summary>
		/// <returns></returns>
		protected internal abstract IConditionalFormattingRuleProxy CreateProxy();

		#endregion // CreateProxy

		#endregion // Methods

		#region IConditionalFormattingRule Members

		bool IConditionalFormattingRule.IsTerminalRule
		{
			get { return this.IsTerminalRule; }
		}

		Column IConditionalFormattingRule.Column
		{
			get
			{
				return this.Column;
			}
			set
			{
				this.Column = value;
			}
		}

		StyleScope IConditionalFormattingRule.StyleScope
		{
			get { return this.StyleScope; }
		}

		bool IConditionalFormattingRule.ShouldRefreshOnDataChange
		{
			get { return this.ShouldRefreshOnDataChange; }
		}

		Visibility IConditionalFormattingRule.CellValueVisibility
		{
			get { return this.CellValueVisibility; }
		}

		IConditionalFormattingRuleProxy IConditionalFormattingRule.CreateProxy()
		{
			return this.CreateProxy();
		}

		EvaluationStage IConditionalFormattingRule.RuleExecution
		{
			get { return this.RuleExecution; }
		}

		#endregion

        #region IProvidePropertyPersistenceSettings Members

        #region Members

        List<string> _propertiesThatShouldntBePersisted;

        #endregion // Members

        #region PropertiesToIgnore

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                if (this._propertiesThatShouldntBePersisted == null)
                {
                    this._propertiesThatShouldntBePersisted = new List<string>()
					{
						"Column"
					};
                }

                return this._propertiesThatShouldntBePersisted;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected virtual List<String> PriorityProperties
        {
            get
            {
                return null;
            }
        }

        #endregion // PriorityProperties

        #region FinishedLoadingPersistence

        /// <summary>
        /// Allows an object to perform an operation, after it's been loaded.
        /// </summary>
        protected virtual void FinishedLoadingPersistence()
        {
        }

        #endregion // FinishedLoadingPersistence

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
        {
            get
            {
                return this.PropertiesToIgnore;
            }
        }

        List<string> IProvidePropertyPersistenceSettings.PriorityProperties
        {
            get
            {
                return this.PriorityProperties;
            }
        }

        void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
        {
            this.FinishedLoadingPersistence();
        }

        #endregion
	}

	/// <summary>
	/// The execution proxy for the <see cref="ConditionalFormattingRuleBase"/>.
	/// </summary>
	public abstract class ConditionalFormattingRuleBaseProxy : IConditionalFormattingRuleProxy
	{
		#region Properties

		#region RuleExecution

		/// <summary>
		/// Get / set at what stage to gather data for conditional formatting.
		/// </summary>
		public EvaluationStage RuleExecution
		{
			get
			{
				EvaluationStage retValue = EvaluationStage.None;
				if (this.Parent != null)
					retValue = this.Parent.RuleExecution;
				return retValue;
			}
		}

		#endregion // RuleExecution

		#region Parent

		/// <summary>
		/// Gets / sets the <see cref="IConditionalFormattingRule"/> which this proxy is associated with.
		/// </summary>
		protected internal IConditionalFormattingRule Parent { get; set; }

		#endregion // Parent

		#endregion // Properties

		#region Methods

		#region EvaluateCondition

		/// <summary>
		/// Determines whether the inputted value meets the condition of <see cref="ConditionalFormattingRuleBase"/> and returns the style 
		/// that should be applied.
		/// </summary>
		/// <param name="sourceDataObject"></param>
		/// <param name="sourceDataValue"></param>
		/// <returns></returns>
		protected abstract Style EvaluateCondition(object sourceDataObject, object sourceDataValue);

		#endregion // EvaluateCondition

		#region GatherData

		/// <summary>
		/// Called at the stage provided by <see cref="RuleExecution"/> so that values can be derived for formatting purposes.
		/// </summary>
		/// <param name="query"></param>
		protected virtual void GatherData(IQueryable query) { }

		#endregion // GatherData

		#endregion // Methods

		#region IRule Members

		#region RuleExecution

		/// <summary>
		/// Designates at what stage during the data binding the GatherData needs to be evaluated.
		/// </summary>
		EvaluationStage IRule.RuleExecution
		{
			get { return this.RuleExecution; }
		}

		#endregion //RuleExecution

		#region GatherData

		/// <summary>
		/// Allows access to the query at the time so that values can be derived off it for the condition.
		/// </summary>
		void IRule.GatherData(IQueryable query)
		{
			this.GatherData(query);
		}

		#endregion // GatherData

		#endregion // IRule Members

		#region IConditionalFormattingRuleProxy Members

		#region Parent
		/// <summary>
		/// Gets / sets the <see cref="IConditionalFormattingRule"/> which this proxy is associated with.
		/// </summary>
		IConditionalFormattingRule IConditionalFormattingRuleProxy.Parent
		{
			get { return this.Parent; }
			set { this.Parent = value; }
		}

		#endregion // Parent

		#region EvaluateCondition

		/// <summary>
		/// Evaluates the rule against the inputted cell and returns true if the rule was applied
		/// </summary>
		/// <param name="sourceDataObject"></param>
		/// <param name="sourceDataValue"></param>
		/// <returns></returns>
		Style IConditionalFormattingRuleProxy.EvaluateCondition(object sourceDataObject, object sourceDataValue)
		{
			return this.EvaluateCondition(sourceDataObject, sourceDataValue);
		}

		#endregion // EvaluateCondition

		#endregion
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