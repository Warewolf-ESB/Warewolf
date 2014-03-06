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
using Infragistics.Controls.Grids.Primitives;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A class that defines the SummaryRow settings for all <see cref="ColumnLayout"/> objects.
	/// </summary>
	public class SummaryRowSettings : SettingsBase, IDisposable
	{
		#region Members

		SummaryDefinitionCollection _xamlParsingCollection;

		#endregion // Members

		#region Properties

		#region AllowSummaryRow

		/// <summary>
		/// Identifies the <see cref="AllowSummaryRow"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowSummaryRowProperty = DependencyProperty.Register("AllowSummaryRow", typeof(SummaryRowLocation), typeof(SummaryRowSettings), new PropertyMetadata(SummaryRowLocation.None, new PropertyChangedCallback(AllowSummaryRowChanged)));

		/// <summary>
		/// Gets/Sets the location of the SummaryRow for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public SummaryRowLocation AllowSummaryRow
		{
			get { return (SummaryRowLocation)this.GetValue(AllowSummaryRowProperty); }
			set { this.SetValue(AllowSummaryRowProperty, value); }
		}

		private static void AllowSummaryRowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettings settings = (SummaryRowSettings)obj;
			settings.OnPropertyChanged("AllowSummaryRow");
		}

		#endregion // AllowSummaryRow

		#region Style

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(SummaryRowSettings), new PropertyMetadata(null, new PropertyChangedCallback(StyleChanged)));

		/// <summary>
		/// Gets/Sets the style that will be applied to every <see cref="Cell"/> in the <see cref="SummaryRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style Style
		{
			get { return (Style)this.GetValue(StyleProperty); }
			set { this.SetValue(StyleProperty, value); }
		}

		private static void StyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettings settings = (SummaryRowSettings)obj;
			settings.OnPropertyChanged("Style");
		}

		#endregion // Style

		#region ExpansionIndicatorStyle

		/// <summary>
		/// Identifies the <see cref="ExpansionIndicatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorStyleProperty = DependencyProperty.Register("ExpansionIndicatorStyle", typeof(Style), typeof(SummaryRowSettings), new PropertyMetadata(new PropertyChangedCallback(ExpansionIndicatorStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used on the <see cref="SummaryRowExpansionIndicatorCellControl"/> objects of the <see cref="ColumnLayout"/>.
		/// </summary>
		public Style ExpansionIndicatorStyle
		{
			get { return (Style)this.GetValue(ExpansionIndicatorStyleProperty); }
			set { this.SetValue(ExpansionIndicatorStyleProperty, value); }
		}

		private static void ExpansionIndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettings settings = (SummaryRowSettings)obj;
			if (settings.Grid != null)
				settings.Grid.ResetPanelRows();
			settings.OnPropertyChanged("ExpansionIndicatorStyle");
		}

		#endregion // ExpansionIndicatorStyle

		#region RowSelectorStyle

		/// <summary>
		/// Identifies the <see cref="RowSelectorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowSelectorStyleProperty = DependencyProperty.Register("RowSelectorStyle", typeof(Style), typeof(SummaryRowSettings), new PropertyMetadata(null, new PropertyChangedCallback(RowSelectorStyleChanged)));

		/// <summary>
		/// Gets/Sets the style that will be applied to the RowSelector of an <see cref="SummaryRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style RowSelectorStyle
		{
			get { return (Style)this.GetValue(RowSelectorStyleProperty); }
			set { this.SetValue(RowSelectorStyleProperty, value); }
		}

		private static void RowSelectorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettings settings = (SummaryRowSettings)obj;
			settings.OnPropertyChanged("RowSelectorStyle");
		}

		#endregion // RowSelectorStyle

		#region SummaryScope

		/// <summary>
		/// Identifies the <see cref="SummaryScope"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SummaryScopeProperty = DependencyProperty.Register("SummaryScope", typeof(SummaryScope), typeof(SummaryRowSettings), new PropertyMetadata(SummaryScope.ColumnLayout, new PropertyChangedCallback(SummaryScopeChanged)));

		/// <summary>
		/// Gets / sets SummaryScope for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public SummaryScope SummaryScope
		{
			get { return (SummaryScope)this.GetValue(SummaryScopeProperty); }
			set { this.SetValue(SummaryScopeProperty, value); }
		}

		private static void SummaryScopeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettings settings = (SummaryRowSettings)obj;
			settings.OnPropertyChanged("SummaryScope");
		}

		#endregion // SummaryScope

		#region SummaryExecution

		/// <summary>
		/// Identifies the <see cref="SummaryExecution"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SummaryExecutionProperty = DependencyProperty.Register("SummaryExecution",
				typeof(SummaryExecution),
				typeof(SummaryRowSettings),
				new PropertyMetadata(Infragistics.SummaryExecution.PriorToFilteringAndPaging,
				new PropertyChangedCallback(SummaryExecutionChanged)));

		/// <summary>
		/// Gets/Sets SummaryExecution for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public SummaryExecution SummaryExecution
		{
			get { return (SummaryExecution)this.GetValue(SummaryExecutionProperty); }
			set { this.SetValue(SummaryExecutionProperty, value); }
		}

		private static void SummaryExecutionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettings settings = (SummaryRowSettings)obj;
			settings.OnPropertyChanged("SummaryExecution");
		}

		#endregion // SummaryExecution

		#region SummaryDefinitionCollection

		/// <summary>
		/// Gets the <see cref="SummaryDefinitionCollection"/> which will be applied to the top level of rows.
		/// </summary>
        [Browsable(false)]
		public SummaryDefinitionCollection SummaryDefinitionCollection
		{
			get
			{
				if (this.Grid == null)
				{
					if (this._xamlParsingCollection == null)
						this._xamlParsingCollection = new SummaryDefinitionCollection();

					return this._xamlParsingCollection;
				}
				return this.Grid.RowsManager.SummaryDefinitionCollectionResolved;
			}
		}

		#endregion // SummaryDefinitionCollection

		#region SummaryResultCollection

		/// <summary>
		/// Gets the <see cref="SummaryResultCollection"/> object that contains the results of the summaries being applied to this <see cref="ChildBand"/>
		/// </summary>
        [Browsable(false)]
		public ReadOnlyCollection<SummaryResult> SummaryResultCollection
		{
			get
			{
				if (this.Grid != null)
				{
					return this.Grid.RowsManager.SummaryResultCollection;
				}
				return null;
			}
		}

		#endregion // SummaryResultCollection

		#endregion // Properties

		#region Overrides

		#region OnGridSet

		/// <summary>
		/// Method called when the grid is set on the settings object to allow for catch up code to be processed regarding 
		/// values that may have been set in Xaml.
		/// </summary>
		protected override void OnGridSet()
		{
			if (_xamlParsingCollection != null)
			{
				SummaryDefinitionCollection sdc = this.SummaryDefinitionCollection;

				for (int i = _xamlParsingCollection.Count - 1; i >= 0; i--)
				{
					SummaryDefinition sd = this._xamlParsingCollection[i];

					this._xamlParsingCollection.Remove(sd);

					sdc.Insert(0, sd);
				}
				this._xamlParsingCollection.Clear();
			}

			base.OnGridSet();
		}

		#endregion // OnGridSet

		#endregion // Overrides

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="SummaryRowSettings"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="SummaryRowSettings"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param name="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._xamlParsingCollection != null)
			{
				this._xamlParsingCollection.Dispose();
				this._xamlParsingCollection = null;
			}
		}

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