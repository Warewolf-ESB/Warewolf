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
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A class that defines the SummaryRow settings for a particular <see cref="ColumnLayout"/>.
	/// </summary>
	public class SummaryRowSettingsOverride : SettingsOverrideBase, IDisposable
	{
		#region Members
		SummaryDefinitionCollection _summaryDefinitionCollection;
		#endregion // Members

		#region Overrides

		#region SettingsObject

		/// <summary>
		/// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsOverrideBase"/>
		/// </summary>
		protected override SettingsBase SettingsObject
		{
			get
			{
				SettingsBase settings = null;
				if (this.ColumnLayout.Grid != null)
					settings = this.ColumnLayout.Grid.SummaryRowSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides

		#region Properties

		#region AllowSummaryRow

		/// <summary>
		/// Identifies the <see cref="AllowSummaryRow"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowSummaryRowProperty = DependencyProperty.Register("AllowSummaryRow", typeof(SummaryRowLocation?), typeof(SummaryRowSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AllowSummaryRowChanged)));

		/// <summary>
		/// Gets/Sets the location of the SummaryRow for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<SummaryRowLocation>))]
		public SummaryRowLocation? AllowSummaryRow
		{
			get { return (SummaryRowLocation?)this.GetValue(AllowSummaryRowProperty); }
			set { this.SetValue(AllowSummaryRowProperty, value); }
		}

		private static void AllowSummaryRowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettingsOverride settings = (SummaryRowSettingsOverride)obj;
			settings.OnPropertyChanged("AllowSummaryRow");
		}

		#endregion // AllowSummaryRow

		#region AllowSummaryRowResolved

		/// <summary>
		/// Resolves the <see cref="SummaryRowSettingsOverride.AllowSummaryRow"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public SummaryRowLocation AllowSummaryRowResolved
		{
            get
            {
                if (this.AllowSummaryRow == null)
                {
                    if (this.SettingsObject != null)
                        return ((SummaryRowSettings)this.SettingsObject).AllowSummaryRow;
                }
                else
                    return (SummaryRowLocation)this.AllowSummaryRow;

                return (SummaryRowLocation)SummaryRowSettings.AllowSummaryRowProperty.GetMetadata(typeof(SummaryRowSettings)).DefaultValue;
            }
		}
		#endregion //AllowSummaryRowResolved

		#region ExpansionIndicatorStyle

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorStyleProperty = DependencyProperty.Register("ExpansionIndicatorStyle", typeof(Style), typeof(SummaryRowSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(ExpansionIndicatorStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style ExpansionIndicatorStyle
		{
			get { return (Style)this.GetValue(ExpansionIndicatorStyleProperty); }
			set { this.SetValue(ExpansionIndicatorStyleProperty, value); }
		}

		private static void ExpansionIndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettingsOverride settings = (SummaryRowSettingsOverride)obj;
			if (settings.ColumnLayout != null && settings.ColumnLayout.Grid != null)
				settings.ColumnLayout.Grid.ResetPanelRows();
			settings.OnPropertyChanged("ExpansionIndicatorStyle");
		}

		#endregion // ExpansionIndicatorStyle

		#region ExpansionIndicatorStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style ExpansionIndicatorStyleResolved
		{
			get
			{
				if (this.ExpansionIndicatorStyle == null && this.SettingsObject != null)
					return ((SummaryRowSettings)this.SettingsObject).ExpansionIndicatorStyle;
				else
					return this.ExpansionIndicatorStyle;
			}
		}

		#endregion // ExpansionIndicatorStyleResolved

		#region Style

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(SummaryRowSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(StyleChanged)));

		/// <summary>
        /// Gets/Sets the style that will be applied to every <see cref="Cell"/> in the <see cref="Infragistics.Controls.Grids.Primitives.SummaryRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style Style
		{
			get { return (Style)this.GetValue(StyleProperty); }
			set { this.SetValue(StyleProperty, value); }
		}

		private static void StyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettingsOverride settings = (SummaryRowSettingsOverride)obj;
			settings.OnPropertyChanged("Style");
		}

		#endregion // Style

		#region StyleResolved

		/// <summary>
		/// Resolves the <see cref="SummaryRowSettingsOverride.Style"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style StyleResolved
		{
			get
			{
				if (this.Style == null && this.SettingsObject != null)
					return ((SummaryRowSettings)this.SettingsObject).Style;
				else
					return (Style)this.Style;
			}
		}

		#endregion //StyleResolved

		#region RowSelectorStyle

		/// <summary>
		/// Identifies the <see cref="RowSelectorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowSelectorStyleProperty = DependencyProperty.Register("RowSelectorStyle", typeof(Style), typeof(SummaryRowSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(RowSelectorStyleChanged)));

		/// <summary>
        /// Gets/Sets the style that will be applied to the RowSelector of an <see cref="Infragistics.Controls.Grids.Primitives.SummaryRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style RowSelectorStyle
		{
			get { return (Style)this.GetValue(RowSelectorStyleProperty); }
			set { this.SetValue(RowSelectorStyleProperty, value); }
		}

		private static void RowSelectorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettingsOverride settings = (SummaryRowSettingsOverride)obj;
			settings.OnPropertyChanged("RowSelectorStyle");
		}

		#endregion // RowSelectorStyle

		#region RowSelectorStyleResolved

		/// <summary>
		/// Resolves the <see cref="SummaryRowSettingsOverride.RowSelectorStyle"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style RowSelectorStyleResolved
		{
			get
			{
				if (this.RowSelectorStyle == null && this.SettingsObject != null)
					return ((SummaryRowSettings)this.SettingsObject).RowSelectorStyle;

				return (Style)this.RowSelectorStyle;
			}
		}

		#endregion //RowSelectorStyleResolved

		#region SummaryDefinitionCollection

		/// <summary>
		/// A collection of <see cref="SummaryDefinition"/> objects that will define which summaries should be calculated.
		/// </summary>
		public SummaryDefinitionCollection SummaryDefinitionCollection
		{
			get
			{
				if (this._summaryDefinitionCollection == null)
				{
					this._summaryDefinitionCollection = new SummaryDefinitionCollection();
					this._summaryDefinitionCollection.CollectionChanged += SummaryDefinitionCollection_CollectionChanged;
				}
				return this._summaryDefinitionCollection;
			}
		}

		#endregion // SummaryDefinitionCollection

		#region SummaryScope

		/// <summary>
		/// Identifies the <see cref="SummaryScope"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SummaryScopeProperty = DependencyProperty.Register("SummaryScope", typeof(SummaryScope?), typeof(SummaryRowSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(SummaryScopeChanged)));

		/// <summary>
		/// Gets/Sets RowSummaryScope for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<SummaryScope>))]
		public SummaryScope? SummaryScope
		{
			get { return (SummaryScope?)this.GetValue(SummaryScopeProperty); }
			set { this.SetValue(SummaryScopeProperty, value); }
		}

		private static void SummaryScopeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettingsOverride settings = (SummaryRowSettingsOverride)obj;
			settings.OnPropertyChanged("SummaryScope");
		}

		#endregion // SummaryScope

		#region SummaryScopeResolved

		/// <summary>
		/// Resolves the <see cref="SummaryScope"/> property for a particular <see cref="ColumnLayout"/>.
		/// </summary>
		public SummaryScope SummaryScopeResolved
		{
            get
            {
                if (this.SummaryScope == null)
                {
                    if (this.SettingsObject != null)
                        return ((SummaryRowSettings)this.SettingsObject).SummaryScope;
                }
                else
                    return (SummaryScope)this.SummaryScope;

                return (SummaryScope)SummaryRowSettings.SummaryScopeProperty.GetMetadata(typeof(SummaryRowSettings)).DefaultValue;
            }
		}

		#endregion //SummaryScopeResolved

		#region SummaryExecution

		/// <summary>
		/// Identifies the <see cref="SummaryExecution"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SummaryExecutionProperty = DependencyProperty.Register("SummaryExecution", typeof(SummaryExecution?), typeof(SummaryRowSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(SummaryExecutionChanged)));

		/// <summary>
		/// Gets/Sets SummaryExecution for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<SummaryExecution>))]
		public SummaryExecution? SummaryExecution
		{
			get { return (SummaryExecution?)this.GetValue(SummaryExecutionProperty); }
			set { this.SetValue(SummaryExecutionProperty, value); }
		}

		private static void SummaryExecutionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowSettingsOverride settings = (SummaryRowSettingsOverride)obj;
			settings.OnPropertyChanged("SummaryExecution");
		}

		#endregion // SummaryExecution

		#region SummaryExecutionResolved

		/// <summary>
		/// Resolves the <see cref="SummaryRowSettingsOverride.SummaryExecution"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public SummaryExecution SummaryExecutionResolved
		{
			get
			{
                if (this.SummaryExecution == null)
                {
                    if (this.SettingsObject != null)
                        return ((SummaryRowSettings)this.SettingsObject).SummaryExecution;
                }
                else
                    return (SummaryExecution)this.SummaryExecution;

                return (SummaryExecution)SummaryRowSettings.SummaryExecutionProperty.GetMetadata(typeof(SummaryRowSettings)).DefaultValue;
			}
		}

		#endregion //SummaryExecutionResolved

		#endregion // Properties

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="SummaryRowSettingsOverride"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="SummaryRowSettingsOverride"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param name="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._summaryDefinitionCollection != null)
			{
				this._summaryDefinitionCollection.CollectionChanged -= SummaryDefinitionCollection_CollectionChanged;
				this._summaryDefinitionCollection.Dispose();
				this._summaryDefinitionCollection = null;
			}
		}

		#endregion

		#region EventHandlers

		void SummaryDefinitionCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.OnPropertyChanged("InvalidateSummaries");
		}

		#endregion // EventHandlers
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