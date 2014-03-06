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
using System.Collections.Generic;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A column that uses a <see cref="TextBlock"/> as the content for it's <see cref="CellBase"/>s
	/// </summary>
	public class TextColumn : EditableColumn
	{
		#region Members
		FilterOperand _defaultFilterOperand;
        Style _textBlockStyle;
        string _filterMenuCustomFilterString;
		#endregion

		#region Overrides

		#region Methods

		#region GenerateContentProvider
		/// <summary>
		/// Generates a new <see cref="TextColumnContentProvider"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override ColumnContentProviderBase GenerateContentProvider()
		{
			return new TextColumnContentProvider();
		}
		#endregion // GenerateContentProvider

		#region FillAvailableFilters

		/// <summary>
		/// Fills the <see cref="FilterOperandCollection"/> with the operands that the column expects as filter values.
		/// </summary>
		/// <param name="availableFilters"></param>
		protected internal override void FillAvailableFilters(FilterOperandCollection availableFilters)
		{
			base.FillAvailableFilters(availableFilters);

			if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
			{
				Dictionary<ComparisonOperator, DataTemplate> icons = this.ColumnLayout.Grid.FilterIcons;
				if (icons != null && this.DataType != null)
				{
					if (this.DataType == typeof(string))
					{
						//this.DefaultFilterOperand = new StartsWithOperand();

						availableFilters.Add(new StartsWithOperand() { XamWebGrid = this.ColumnLayout.Grid });
						availableFilters.Add(new ContainsOperand() { XamWebGrid = this.ColumnLayout.Grid });
						availableFilters.Add(new EndsWithOperand() { XamWebGrid = this.ColumnLayout.Grid });
						availableFilters.Add(new DoesNotContainOperand() { XamWebGrid = this.ColumnLayout.Grid });
						availableFilters.Add(new DoesNotStartWithOperand() { XamWebGrid = this.ColumnLayout.Grid });
						availableFilters.Add(new DoesNotEndWithOperand() { XamWebGrid = this.ColumnLayout.Grid });
					}
					else if ((this.DataType.IsValueType || FilterRow.IsNullableValueType(this.DataType))
                         && this.DataType != typeof(bool) && this.DataType != typeof(bool?)
                         && this.DataType != typeof(DateTime) && this.DataType != typeof(DateTime?)
                        )

					{
						
						availableFilters.Add(new GreaterThanOperand() { XamWebGrid = this.ColumnLayout.Grid });
						availableFilters.Add(new GreaterThanOrEqualOperand() { XamWebGrid = this.ColumnLayout.Grid });
						availableFilters.Add(new LessThanOperand() { XamWebGrid = this.ColumnLayout.Grid });
						availableFilters.Add(new LessThanOrEqualOperand() { XamWebGrid = this.ColumnLayout.Grid });
					}
				}
			}
		}
		#endregion // FillAvailableFilters

        #region FillFilterMenuOptions
        /// <summary>
        /// Fills the inputted list with options for the FilterMenu control.
        /// </summary>
        /// <param name="list"></param>
        internal protected override void FillFilterMenuOptions(List<FilterMenuTrackingObject> list)
        {
            base.FillFilterMenuOptions(list);

            if (this.DataType != null && this.ColumnLayout != null && this.ColumnLayout.Grid != null)
            {
                if (list != null && list.Count > 0)
                {
                    FilterMenuTrackingObject fmto = list[0];
                    list = fmto.Children;
                    if (this.DataType == typeof(string))
                    {
                        list.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                        list.Add(new FilterMenuTrackingObject(new StartsWithOperand() { DisplayName = SRGrid.GetString("StartsWithEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new EndsWithOperand() { DisplayName = SRGrid.GetString("EndsWithEllipsis") }));
                        list.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                        list.Add(new FilterMenuTrackingObject(new ContainsOperand() { DisplayName = SRGrid.GetString("ContainsEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new DoesNotContainOperand() { DisplayName = SRGrid.GetString("DoesNotContainEllipsis") }));
                        list.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                        list.Add(new FilterMenuTrackingObject(new DoesNotStartWithOperand() { DisplayName = SRGrid.GetString("DoesNotStartWithEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new DoesNotEndWithOperand() { DisplayName = SRGrid.GetString("DoesNotEndWithEllipsis") }));
                    }
                    else if ((this.DataType.IsPrimitive || FilterRow.IsNullableValueType(this.DataType))
                        && this.DataType != typeof(bool) && this.DataType != typeof(bool?)
                        && this.DataType != typeof(DateTime) && this.DataType != typeof(DateTime?)
                        )
                    {
                        
                        list.Add(new FilterMenuTrackingObject() { IsSeparator = true });
                        list.Add(new FilterMenuTrackingObject(new GreaterThanOperand() { DisplayName = SRGrid.GetString("GreaterThanEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new GreaterThanOrEqualOperand() { DisplayName = SRGrid.GetString("GreaterThanOrEqualEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new LessThanOperand() { DisplayName = SRGrid.GetString("LessThanEllipsis") }));
                        list.Add(new FilterMenuTrackingObject(new LessThanOrEqualOperand() { DisplayName = SRGrid.GetString("LessThanOrEqualEllipsis") }));

                        FilterMenuTrackingObject obj = new FilterMenuTrackingObject();
                        obj.Label = SRGrid.GetString("BetweenEllipsis");
                        obj.FilterOperands.Add(new GreaterThanOrEqualOperand());
                        obj.FilterOperands.Add(new LessThanOrEqualOperand());
                        list.Add(obj);
                    }
                }
            }
        }
        #endregion // FillFilterMenuOptions

        #region OnDataTypeChanged
        /// <summary>
        /// Raised when the DataType of the <see cref="ColumnBase" /> is changed.
        /// </summary>
        protected override void OnDataTypeChanged()
        {
            if (this.DataType == typeof(string))
            {
                this._filterMenuCustomFilterString = "StringFilters";
            }
            else if ( (this.DataType.IsPrimitive || FilterRow.IsNullableValueType(this.DataType) ) && 
                this.DataType != typeof(bool) && this.DataType != typeof(bool?) &&
                this.DataType != typeof(DateTime) && this.DataType != typeof(DateTime?)
                )
            {
                this._filterMenuCustomFilterString = "NumberFilters";
            }
            else if (this.DataType == typeof(bool) || this.DataType == typeof(bool?))
            {
                this._filterMenuCustomFilterString = "BoolFilters";
            }
            base.OnDataTypeChanged();
        }
        #endregion // OnDataTypeChanged

        #endregion // Methods

        #region Properties

        #region DefaultFilterOperand

        /// <summary>
		/// The default <see cref="FilterOperand"/> for this column type;
		/// </summary>
		protected internal override FilterOperand DefaultFilterOperand
		{
			get
			{
				if (this.DataType == typeof(string))
				{
					if (this._defaultFilterOperand == null)
						this._defaultFilterOperand = new StartsWithOperand();

					return this._defaultFilterOperand;
				}
				return base.DefaultFilterOperand;
			}
		}

		#endregion // DefaultFilterOperand

		#region FormatStringResolved

		/// <summary>
		/// A format string for formatting data in this column.
		/// </summary>
		protected internal override string FormatStringResolved
		{
			get
			{
				return this.FormatString;
			}
		}

		#endregion // FormatStringResolved

        #region FilterMenuCustomFilterString
        /// <summary>
        /// Gets the default string for the FilterMenu for the CustomFilter text
        /// </summary>
        protected override string FilterMenuCustomFilterString
        {
            get
            {
                if(string.IsNullOrEmpty(this._filterMenuCustomFilterString))
                    return base.FilterMenuCustomFilterString;

                return SRGrid.GetString(this._filterMenuCustomFilterString);
            }
        }
        #endregion // FilterMenuCustomFilterString

		#endregion // Properties

		#endregion // Overrides

		#region Properties

        #region TextBlockStyle

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be applied to the <see cref="TextBlock"/> that displays data in a <see cref="Cell"/>
        /// when it is not in edit mode.
        /// </summary>
        public Style TextBlockStyle
        {
            get
            {
                return this._textBlockStyle;
            }
            set
            {
                if (this._textBlockStyle != value)
                {
                    this._textBlockStyle = value;
                    this.OnPropertyChanged("TextBlockStyle");
                }
            }
        }

        #endregion // TextBlockStyle

		#region TextWrapping

		/// <summary>
		/// Identifies the <see cref="TextWrapping"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TextColumn), new PropertyMetadata(TextWrapping.NoWrap, new PropertyChangedCallback(TextWrappingChanged)));

		/// <summary>
		/// Gets/Sets whether <see cref="TextWrapping"/> should be applied to the  <see cref="TextBlock"/> and <see cref="TextBox"/> of a <see cref="TextColumn"/>
		/// </summary>
		public TextWrapping TextWrapping
		{
			get { return (TextWrapping)this.GetValue(TextWrappingProperty); }
			set { this.SetValue(TextWrappingProperty, value); }
		}

		private static void TextWrappingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TextColumn col = (TextColumn)obj;
			col.OnPropertyChanged("TextWrapping");
		}

		#endregion // TextWrapping

		#region FormatString

		/// <summary>
		/// Identifies the <see cref="FormatString"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(TextColumn), new PropertyMetadata(new PropertyChangedCallback(FormatStringChanged)));

		/// <summary>
		/// Gets/sets the format string that will be applied to all cells in the column, if applicable. 
		/// </summary>
		/// <remarks>
		/// Note: The <see cref="Column.ValueConverter"/> property has higher precedence. 
		/// <para>In order to set this property in xaml, the value must begin with {}. For example: FormatString="{}{0:C}"</para>
		/// </remarks>
		public string FormatString
		{
			get { return (string)this.GetValue(FormatStringProperty); }
			set { this.SetValue(FormatStringProperty, value); }
		}

		private static void FormatStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TextColumn col = (TextColumn)obj;
			col.OnPropertyChanged("FormatString");
			if (col.ColumnLayout != null && col.ColumnLayout.Grid != null)
			{
				col.ColumnLayout.Grid.ResetPanelRows();
			}
		}

		#endregion // FormatString

		#endregion // Properties       
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