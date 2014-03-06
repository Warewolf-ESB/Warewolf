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
using System.Globalization;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Visual object for the <see cref="GroupByCell"/> object.
	/// </summary>
	public class GroupByCellControl : CellControlBase
	{
		#region Members
		ColumnContentProviderBase _content;
		Column _prevColumn;
		FrameworkElement _contentElement;
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupByCellControl"/> class.
		/// </summary>
		public GroupByCellControl()
		{
			base.DefaultStyleKey = typeof(GroupByCellControl);
		}

		#endregion // Constructor

        #region Overrides

        #region EnsureContent

        /// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>		
		protected internal override void EnsureContent()
		{
			FrameworkElement originalFe = this.Content as FrameworkElement;
			double originalContentHeight = 0;
			double originalContentWidth = 0;

			if (originalFe != null)
			{
				originalContentHeight = originalFe.ActualHeight;
				originalContentWidth = originalFe.ActualWidth;
			}

			if (this._content != null)
			{
                Binding binding = this.ResolveBinding(true);
				FrameworkElement fe = this._content.GenerateGroupByCellContent((GroupByCell)this.Cell, binding);

				if (fe != null)
				{
					this.Content = fe;

					if (fe.Width != originalContentWidth || fe.Height != originalContentHeight)
					{
						this.Cell.Row.ActualHeight = 0;
					}
				}
			}

			base.EnsureContent();
		}

		#endregion // EnsureContent

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected override void AttachContent()
		{
			GroupByRow row = (GroupByRow)this.Cell.Row;
			Column column = ((RowsManager)row.Manager).GroupedColumn;

            

            if (column == null)
            {
                return;
            }

			DataTemplate template = column.GroupByItemTemplate;

			if (template == null)
			{
				this._contentElement = null;
			}

			// Cache the information for a specific column.  
			// This will allow us to have MUCH better performance. when scrolling groupby rows
			if (this._prevColumn != column)
			{
				this._content = column.GenerateContentProvider();

				if (template != null)
				{
					this._contentElement = template.LoadContent() as FrameworkElement;
					this.Content = this._contentElement;
				}
				else
				{
					this._contentElement = null;
					this.Content = null;
				}

				this._prevColumn = column;
			}

			if (this._contentElement != null)
			{
				this._contentElement.DataContext = row.GroupByData;				
			}
			else
			{
				Binding binding = this.ResolveBinding(true);

				FrameworkElement fe = this._content.GenerateGroupByCellContent((GroupByCell)this.Cell, binding);

				if (fe != null)
				{
					this.Content = fe;
				}
				else
				{
                    binding = this.ResolveBinding(false);
					this.SetBinding(GroupByAreaCellControl.ContentProperty, binding);
				}
			}
		}
		#endregion // AttachContent

		#region ContentProvider
		/// <summary>
		/// Resolves the <see cref="ColumnContentProviderBase"/> for this <see cref="CellControl"/>.
		/// </summary>
		public override ColumnContentProviderBase ContentProvider
		{
			get
			{
				return this._content;
			}
		}
		#endregion // ContentProvider

		#region ResolveBinding
		/// <summary>
		/// Builds the binding that will be used for a <see cref="GroupByCell"/>
		/// </summary>
		/// <returns>If a binding cannot be created, null will be returned.</returns>
		protected internal override Binding ResolveBinding()
		{
            return this.ResolveBinding(false);
		}

        private Binding ResolveBinding(bool overrideUseValue)
        {
            GroupByRow row = (GroupByRow)this.Cell.Row;

            Binding binding = null;
            GroupByDataContext data = row.GroupByData;

            if (data != null)
            {
                IValueConverter wrappedConverter = null;
                
                if (!overrideUseValue && row.ColumnLayout.Grid.GroupBySettings.DisplayCountOnGroupedRow)
                {
                    Column column = ((Cell)this.Cell).ResolveColumn;

                    if (column != null && !(column is UnboundColumn) && column.ValueConverter != null)
                    {
                        wrappedConverter = column.ValueConverter;
                        binding = new Binding("Value");
                    }
                    else
                    {
                        binding = new Binding("DisplayValue");
                    }
                }
                else
                {
                    binding = new Binding("Value");
                }

                binding.Source = data;
                binding.Mode = BindingMode.OneTime;
                binding.ConverterCulture = CultureInfo.CurrentCulture;

                

                binding.ConverterParameter = this.Cell;
                binding.Converter = new GroupByCellBindingConverter(wrappedConverter, data.DisplayValueStringFormat, data.Count);
            }
            return binding;
        }

		#endregion // ResolveBinding

		#region ReleaseContent

		/// <summary>
		/// Invoked before content is released from the control.
		/// </summary>
		protected override void ReleaseContent()
		{
			base.ReleaseContent();

			if (this._contentElement != null)
				this._contentElement.DataContext = null;
		}

		#endregion // ReleaseContent

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Called before the <see cref="UIElement.MouseLeftButtonDown"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			if (!(this.Cell.Column is FillerColumn))
				this.Cell.Row.ColumnLayout.Grid.ActiveCell = this.Cell;
		}
		#endregion // OnMouseLeftButtonDown
        
        #endregion // Overrides
        
        #region Classes

        internal class GroupByCellBindingConverter : Infragistics.Controls.Grids.Cell.CellBindingConverter
		{
		    #region Members

		    private readonly IValueConverter _valueConverter;
		    private readonly string _stringFormat;
		    private readonly int _count;

		    #endregion // Members

		    #region Constructor

		    public GroupByCellBindingConverter(IValueConverter valueConverter, string stringFormat, int count)
		    {
		        this._valueConverter = valueConverter;
		        this._stringFormat = stringFormat;
		        this._count = count;
		    }

		    #endregion // Constructor

			#region IValueConverter Members

			public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				Cell cell = (Cell)parameter;

				Column column = cell.ResolveColumn;

                if (this._valueConverter != null)
                {
                    var convertedValue = _valueConverter.Convert(value, targetType, column.ValueConverterParameter, culture);

                    return string.Format(this._stringFormat, convertedValue ?? string.Empty, this._count);
                }

				object val = value;

				if (cell.Control != null && cell.Control.ContentProvider != null)
					val = cell.Control.ContentProvider.ApplyFormatting(value, column, culture);

				if (val is IConvertible)
					return System.Convert.ChangeType(val, typeof(string), System.Globalization.CultureInfo.CurrentCulture);
				
				return val;
			}

			#endregion
        }

        #endregion // Classes
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