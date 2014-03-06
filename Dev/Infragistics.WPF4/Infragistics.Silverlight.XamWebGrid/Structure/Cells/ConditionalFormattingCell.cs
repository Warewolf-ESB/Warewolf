using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using Infragistics.Controls.Grids.Primitives;
using System.Windows.Controls;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A derived <see cref="Cell"/> object in the <see cref="XamGrid"/> which is used for Conditional Formatting.
	/// </summary>
	public class ConditionalFormattingCell : Cell
    {
        #region Members
        Style _conditionalStyle;
        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalFormattingCell"/> class.
        /// </summary>
        /// <param propertyName="row">The <see cref="RowBase"/> object that owns the <see cref="Cell"/></param>
        /// <param propertyName="column">The <see cref="Column"/> object that the <see cref="Cell"/> represents.</param>
        public ConditionalFormattingCell(RowBase row, Column column)
            : base(row, column)
        {
        }

        #endregion // Constructor

        #region Members

        private bool _displayContentPriorToEnterEditMode;
        private bool _displayContent = true;

        #endregion // Members

        #region Overrides

        #region Properties

        #region ResolveStyle

        /// <summary>
        /// Gets the Style that should be applied to the <see cref="CellControl"/> when it's attached.
        /// </summary>
        protected override Style ResolveStyle
        {
            get
            {
                if (this.ConditionalStyle != null && this.Row.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
                    return this.ConditionalStyle;

                if (this.Style != null)
                    return this.Style;
                else
                {
                    Row r = this.Row as Row;

                    if (r != null)
                    {
                        if (r.ConditionalCellStyle != null && this.Row.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
                            return r.ConditionalCellStyle;
                        else if (r.CellStyle != null)
                            return r.CellStyle;
                    }
                    return this.Column.CellStyleResolved;
                }
            }
        }

        #endregion // ResolveStyle

        #region Style
        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="CellControlBase"/> objects.
        /// </summary>
        public override Style Style
        {
            get
            {
                return base.Style;
            }
            set
            {
                if (value != base.Style)
                {
                    base.Style = value;

                    ControlTemplate ct = null;
                    XamGrid.CloneStyleWithoutControlTemplate(value, out ct);
                    this.ControlTemplateForConditionalFormatting = ct;
                    if (this.Control != null && this.Control.IsCellLoaded)
                        this.Refresh();
                }                
            }
        }
        #endregion // Style

        #region ControlTemplateForConditionalFormatting

        internal ControlTemplate ControlTemplateForConditionalFormatting
        {
            get;
            set;
        }

        #endregion // ControlTemplateForConditionalFormatting   

        #endregion // Properties

        #region Methods

        #region CreateInstanceOfRecyclingElement

        /// <summary>
		/// Creates a new instance of a <see cref="CellControl"/> for the <see cref="Cell"/>.
		/// </summary>
		/// <returns>A new <see cref="CellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new ConditionalFormattingCellControl();
		}

        #endregion // CreateInstanceOfRecyclingElement

        #region CreateCellBindingConverter

        /// <summary>
        /// Creates the <see cref="IValueConverter"/> which will be attached to this <see cref="Cell"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override IValueConverter CreateCellBindingConverter()
        {
            return new ConditionalFormattingCellBindingConverter();
        }

        #endregion // CreateCellBindingConverter

        #region EnsureCurrentState
        /// <summary>
        /// Ensures that <see cref="Cell"/> is in the correct state.
        /// </summary>
        protected internal override void EnsureCurrentState()
        {
            base.EnsureCurrentState();

            if (this.Control != null)
                this.Control.EnsureCurrentState();
        }
        #endregion // EnsureCurrentState

        #region EnterEditMode

        /// <summary>
        /// Places the specified <see cref="Cell"/> into edit mode.
        /// </summary>
        protected internal override void EnterEditMode(bool cellIsEditing)
        {
            this._displayContentPriorToEnterEditMode = this.DisplayContent;

            base.EnterEditMode(cellIsEditing);

            this.DisplayContent = true;
        }
        #endregion // EnterEditMode

        #region ExitEditMode

        /// <summary>
        /// Takes the specified <see cref="Cell"/> out of edit mode.
        /// </summary>
        /// <param propertyName="newValue">The value that should be entered in the <see cref="Cell"/></param>
        /// <param propertyName="editingCanceled">Whether or not we're exiting edit mode, because it was cancelled.</param>
        /// <param propertyName="evaluateBindings">Whether or not we should evaluate the cell's bindings.</param>
        protected internal override bool ExitEditMode(object newValue, bool editingCanceled, bool evaluateBindings)
        {
            bool retValue = base.ExitEditMode(newValue, editingCanceled, evaluateBindings);

            if (retValue)
                this.DisplayContent = this._displayContentPriorToEnterEditMode;

            return retValue;
        }

        #endregion // ExitEditMode

        #region ApplyStyle
        /// <summary>
        /// Applies the resolved style of a Cell to it's <see cref="CellControlBase"/>
        /// </summary>
        protected internal override void ApplyStyle()
        {                       
            base.ApplyStyle();

            ConditionalFormattingCellControl cellControl = this.Control as ConditionalFormattingCellControl;
            if (cellControl != null)
            {
                ControlTemplate ct = this.ControlTemplateForConditionalFormattingResolved;
                if (ct != this.Control.Template)
                {
                    if (ct == null)
                    {
                        if (cellControl.CustomTemplateSet)
                        {
                            cellControl.ClearValue(ContentControl.TemplateProperty);
                            cellControl.CustomTemplateSet = false;
                        }
                    }
                    else
                    {
                        cellControl.Template = ct;
                        cellControl.CustomTemplateSet = true;
                    }
                }
            }
        }
        #endregion // ApplyStyle

        #region Refresh
        /// <summary>
        /// Refreshes the content of the cell.
        /// </summary>
        public override void Refresh()
        {
            if (this.Control != null && !this.InCellConverter)
            {
                bool oldValue = ((Row)this.Row).IgnoreCellControlAttached;
                ((Row)this.Row).IgnoreCellControlAttached = true;
                this.Control.DataContext = null;
                this.Control.ClearValue(CellControl.DataContextProperty);
                ((Row)this.Row).IgnoreCellControlAttached = oldValue;
            }
            base.Refresh();
        }
        #endregion // Refresh

        #endregion // Methods

        #endregion // Overrides

        #region Properties

        #region ConditionalStyle

        /// <summary>
		/// Get / set a <see cref="Style"/> that will override existing styles.  
		/// </summary>
		protected internal Style ConditionalStyle
		{
            get
            {
                return this._conditionalStyle;
            }
            set
            {
                if (value != null)
                {
                    Row r = this.Row as Row;
                    if (r != null)
                    {
                        r.ConditionalStyleDirty = true;
                    }
                }
                this._conditionalStyle = value;
            }
		}

        #endregion // ConditionalStyle

        #region DisplayContent

        /// <summary>
        /// Gets / sets if the Content of the cell will be displayed.
        /// </summary>
        protected internal bool DisplayContent
        {
            get
            {
                return this._displayContent;
            }
            set
            {
                if (this._displayContent != value)
                {
                    this._displayContent = value;
                    this.EnsureCurrentState();
                }
            }
        }

        #endregion // DisplayContent

        #region StrippedCellStyleForConditionalFormatting

        internal Style StrippedCellStyleForConditionalFormattingResolved
        {
            get
            {
                if (this.Style != null)
                    return this.Style;

                Row r = this.Row as Row;

                if (r != null)
                {
                    if (r.StrippedCellStyleForConditionalFormatting != null)
                        return r.StrippedCellStyleForConditionalFormatting;
                }
                return this.Column.StrippedCellStyleForConditionalFormattingResolved;
            }
        }

        #endregion // StrippedCellStyleForConditionalFormatting

        #region ControlTemplateForConditionalFormattingResolved

        internal virtual ControlTemplate ControlTemplateForConditionalFormattingResolved
        {
            get
            {

                if (this.ConditionalStyleControlTemplate != null)
                    return this.ConditionalStyleControlTemplate;

                Row row = this.Row as Row;

                if (row != null && row.ConditionalStyleControlTemplate != null)
                    return row.ConditionalStyleControlTemplate;

                if (this.ControlTemplateForConditionalFormatting != null)
                    return this.ControlTemplateForConditionalFormatting;

                if (row != null && row.ControlTemplateForConditionalFormatting != null)
                    return row.ControlTemplateForConditionalFormatting;

                if (this.Column != null)
                    return this.Column.ControlTemplateForConditionalFormattingResolved;

                return null;
            }
        }

        #endregion // ControlTemplateForConditionalFormattingResolved

        #region ConditionalStyleControlTemplate
        /// <summary>
        /// This property holds the ControlTemplate that would have been ripped out by the generated style in conditional formatting.
        /// </summary>
        internal ControlTemplate ConditionalStyleControlTemplate
        {
            get;
            set;
        }
        #endregion // ConditionalStyleControlTemplate

        #region InCellConverter

        internal bool InCellConverter { get; set; }

        #endregion // InCellConverter

        #endregion // Properties

        #region ConditionalFormattingCellBindingConverter

        internal class ConditionalFormattingCellBindingConverter : IValueConverter
        {
            internal static Style MergeSettersIntoSingleStyle(List<SetterBase> setters, Style basedOnStyle, out bool controlTemplateSet, out bool needApplyStyle,out ControlTemplate controlTemplate)
            {
                controlTemplateSet = false;
                controlTemplate = null;
                needApplyStyle = false;

                Style generatedStyle = new Style(typeof(ConditionalFormattingCellControl));
                generatedStyle.BasedOn = basedOnStyle;
                if (setters.Count > 0)
                {
                    for (int i = setters.Count - 1; i >= 0; i--)
                    {
                        Setter tempSetter = new Setter();
                        Setter currentSetter = (Setter)setters[i];
                        tempSetter.Property = currentSetter.Property;
                        object setterValue = currentSetter.Value;
                        
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

                        if (setterValue is uint)
                        {
                            setterValue = setterValue.ToString();
                        }
                        if (setterValue is ControlTemplate)
                        {
                            controlTemplateSet = true;
                            controlTemplate = (ControlTemplate)setterValue;
                            continue;
                        }
                        tempSetter.Value = setterValue;
                        generatedStyle.Setters.Add(tempSetter);
                    }

                    needApplyStyle = true;
                }

                return generatedStyle;
            }

            #region IValueConverter Members

			public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				CellControlBase ctrl = (CellControlBase)parameter;
				ConditionalFormattingCell cell = (ConditionalFormattingCell)ctrl.Cell;

                if (cell == null) return value;

				if (cell != null)
				{
                    bool noCntrl = (cell.Control == null);
                    if (noCntrl)
                        cell.Control = ctrl;

                    cell.InCellConverter = true;
                    cell.Row.InCellConverter = true;

                    if (!((Row)cell.Row).IgnoreCellControlAttached)
                        cell.RaiseCellControlAttachedEvent();

					Visibility renderText = Visibility.Visible;
					Column cellColumn = cell.Column;
					ColumnLayout columnLayout = cellColumn.ColumnLayout;
                    if (columnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting && !cell.IsEditing)
                    {
                        bool needApplyStyle = cell.ConditionalStyle != null;
                        cell.ConditionalStyle = null;
                        Row row = (Row)cell.Row;
                        ctrl.EnsureCurrentState();

                        ReadOnlyCollection<IConditionalFormattingRuleProxy> rules = cell.Row.Manager.GetCellScopedConditions(cellColumn.Key);

                        List<SetterBase> cellSetters = new List<SetterBase>();

                        foreach (IConditionalFormattingRuleProxy rule in rules)
                        {
                            Style s = rule.EvaluateCondition(row.Data, value);
                            if (s != null)
                            {
                                if (rule.Parent.StyleScope == StyleScope.Cell)
                                {
                                    cellSetters.AddRange(s.Setters);
                                }

                                renderText = rule.Parent.CellValueVisibility;

                                if (rule.Parent.IsTerminalRule)
                                    break;
                            }
                        }

                        bool controlTemplateSet = false;

                        ControlTemplate controlTemplate = null;

                        Style baseCellStyle = null;

                        if (cell.Style != null)
                            baseCellStyle = cell.StrippedCellStyleForConditionalFormattingResolved;
                        else if (row.ConditionalCellStyle != null)
                            baseCellStyle = row.ConditionalCellStyle;
                        else
                            baseCellStyle = cell.StrippedCellStyleForConditionalFormattingResolved;
 
                        Style generatedStyle = ConditionalFormattingCellBindingConverter.MergeSettersIntoSingleStyle(cellSetters, baseCellStyle, out controlTemplateSet, out needApplyStyle, out controlTemplate);

                        cell.ConditionalStyleControlTemplate = controlTemplate;

                        if (controlTemplateSet && cell.Control != null)
                        {
                            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

                            cell.Control.Dispatcher.BeginInvoke(new Action<ConditionalFormattingCell, Style>(this.SetAndApplyConditionalStyle), cell, generatedStyle);
                        }
                        else
                        {
                            cell.ConditionalStyle = generatedStyle;
                            cell.ApplyStyle();
                        }
                    }

                    cell.DisplayContent = (renderText == Visibility.Visible);

                    if (noCntrl)
                        cell.Control = null;

                    Column column = cell.ResolveColumn;

                    if (column.ValueConverter != null)
                        value = column.ValueConverter.Convert(value, targetType, column.ValueConverterParameter, culture);
                    else if (ctrl.ContentProvider != null)
                        value = ctrl.ContentProvider.ApplyFormatting(value, column, culture);

                    if (value == null && !string.IsNullOrEmpty(column.DataField.NullDisplayText))
                        value = column.DataField.NullDisplayText;
                }

                cell.InCellConverter = false;
                cell.Row.InCellConverter = false;

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                CellControlBase ctrl = (CellControlBase)parameter;
                Cell cell = (Cell)ctrl.Cell;
                if (cell != null)
                {
                    Column column = cell.ResolveColumn;

                    if (column.ValueConverter != null)
                        return column.ValueConverter.ConvertBack(value, targetType, column.ValueConverterParameter, culture);
                }

                return value;
            }

            #endregion

            #region Methods

            #region Private

            private void SetAndApplyConditionalStyle(ConditionalFormattingCell cell, Style generatedStyle)
            {
                cell.ConditionalStyle = generatedStyle;
                cell.ApplyStyle();
            }

            #endregion // Private

            #endregion // Methods
        }
        #endregion // ConditionalFormattingCellBindingConverter
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