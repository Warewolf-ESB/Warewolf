using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A cell that represents a <see cref="Cell"/> in a <see cref="UnboundColumn"/> when conditional formatting is turned on.
	/// </summary>
	public class ConditionalFormattingUnboundCell : UnboundCell
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionalFormattingUnboundCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="RowBase"/> object that owns the <see cref="Cell"/></param>
		/// <param propertyName="column">The <see cref="Column"/> object that the <see cref="Cell"/> represents.</param>
		public ConditionalFormattingUnboundCell(RowBase row, Column column)
			: base(row, column)
		{
		}

		#endregion // Constructor

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
			return new ConditionalFormattingUnboundCellBindingConverter();
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

		#endregion // Methods

		#endregion // Overrides

		#region Properties

		#region ConditionalStyle
		/// <summary>
		/// Get / set a <see cref="Style"/> that will override existing styles.  
		/// </summary>
		protected internal Style ConditionalStyle
		{
			get;
			set;
		}
		#endregion // ConditionalStyle

		#endregion // Properties

		#region ConditionalFormattingCellBindingConverter

		internal class ConditionalFormattingUnboundCellBindingConverter : IValueConverter
		{
			#region IValueConverter Members

			public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
                ConditionalFormattingCellControl cellControl = (ConditionalFormattingCellControl)parameter;
                ConditionalFormattingUnboundCell cell = (ConditionalFormattingUnboundCell)cellControl.Cell;

				Visibility renderText = Visibility.Visible;
				if (cell.Control != null)
				{
					Column cellColumn = cell.Column;
					ColumnLayout columnLayout = cellColumn.ColumnLayout;
					if (columnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
					{
						bool needApplyStyle = cell.ConditionalStyle != null;
						cell.ConditionalStyle = null;
						Row row = (Row)cell.Row;
						cell.Control.EnsureCurrentState();

						ReadOnlyCollection<IConditionalFormattingRuleProxy> rules = cell.Row.Manager.GetCellScopedConditions(cellColumn.Key);// columnLayout.ConditionalFormats.GetCellScopedConditionsForKey(cellColumn.Key);

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

						if (cellSetters.Count > 0)
						{
							Style generatedStyle = new Style(typeof(ConditionalFormattingCellControl));

							generatedStyle.BasedOn = cell.ResolveStyle;

							for (int i = cellSetters.Count - 1; i >= 0; i--)
							{
								Setter tempSetter = new Setter();
								Setter currentSetter = (Setter)cellSetters[i];
								tempSetter.Property = currentSetter.Property;
								tempSetter.Value = currentSetter.Value;
								generatedStyle.Setters.Add(tempSetter);
							}
							cell.ConditionalStyle = generatedStyle;

							needApplyStyle = true;
						}
						if (needApplyStyle)
							cell.ApplyStyle();
					}
				}

				cell.RaiseCellControlAttachedEvent();

				if (renderText == Visibility.Collapsed)
					return "";

				Column column = cell.ResolveColumn;

				if (column.ValueConverter != null)
                    value = column.ValueConverter.Convert(value, targetType, column.ValueConverterParameter, culture);
                else if (cell.Control != null && cell.Control.ContentProvider != null)
                    value = cell.Control.ContentProvider.ApplyFormatting(value, column, culture);

                if (value == null && !string.IsNullOrEmpty(column.DataField.NullDisplayText))
                    value = column.DataField.NullDisplayText;

				return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				Cell cell = (Cell)parameter;
				Column column = cell.ResolveColumn;

				if (column.ValueConverter != null)
					return column.ValueConverter.ConvertBack(value, targetType, column.ValueConverterParameter, culture);

				return value;
			}

			#endregion
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