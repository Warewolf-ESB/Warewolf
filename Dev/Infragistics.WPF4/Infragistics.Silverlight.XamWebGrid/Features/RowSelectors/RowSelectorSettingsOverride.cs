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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using RowSelectors on a particular  <see cref="ColumnLayout"/>
	/// </summary>
	[StyleTypedProperty(Property = "Style", StyleTargetType = typeof(RowSelectorCellControl))]
	[StyleTypedProperty(Property = "HeaderStyle", StyleTargetType = typeof(RowSelectorHeaderCellControl))]
	[StyleTypedProperty(Property = "FooterStyle", StyleTargetType = typeof(RowSelectorFooterCellControl))]
	public class RowSelectorSettingsOverride : VisualSettingsOverrideBase
	{
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
				if (this.ColumnLayout!=null&& this.ColumnLayout.Grid != null)
					settings = this.ColumnLayout.Grid.RowSelectorSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides

		#region Properties

		#region HeaderStyle

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(RowSelectorSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(HeaderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style HeaderStyle
		{
			get { return (Style)this.GetValue(HeaderStyleProperty); }
			set { this.SetValue(HeaderStyleProperty, value); }
		}

		private static void HeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			RowSelectorSettingsOverride settings = (RowSelectorSettingsOverride)obj;
			if (settings.ColumnLayout != null && settings.ColumnLayout.Grid != null)
				settings.ColumnLayout.Grid.ResetPanelRows();
			settings.OnPropertyChanged("HeaderStyle");
		}

		#endregion // HeaderStyle

		#region HeaderStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style HeaderStyleResolved
		{
			get
			{
				if (this.HeaderStyle == null && this.SettingsObject != null)
					return ((RowSelectorSettings)this.SettingsObject).HeaderStyle;
				else
					return this.HeaderStyle;
			}
		}

		#endregion // HeaderStyleResolved

		#region FooterStyle

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FooterStyleProperty = DependencyProperty.Register("FooterStyle", typeof(Style), typeof(RowSelectorSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FooterStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style FooterStyle
		{
			get { return (Style)this.GetValue(FooterStyleProperty); }
			set { this.SetValue(FooterStyleProperty, value); }
		}

		private static void FooterStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			RowSelectorSettingsOverride settings = (RowSelectorSettingsOverride)obj;
			if (settings.ColumnLayout != null && settings.ColumnLayout.Grid != null)
				settings.ColumnLayout.Grid.ResetPanelRows();
			settings.OnPropertyChanged("FooterStyle");
		}

		#endregion // FooterStyle

		#region FooterStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style FooterStyleResolved
		{
			get
			{
				if (this.FooterStyle == null && this.SettingsObject != null)
					return ((RowSelectorSettings)this.SettingsObject).FooterStyle;
				else
					return this.FooterStyle;
			}
		}

		#endregion // FooterStyleResolved

		#region RowNumberingSeed

		/// <summary>
		/// Identifies the <see cref="RowNumberingSeed"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowNumberingSeedProperty = DependencyProperty.Register("RowNumberingSeed", typeof(int?), typeof(RowSelectorSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(RowNumberingSeedChanged)));

		/// <summary>
		/// Gets / sets the value that will be used to start the row numbering on this <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableIntConverter))]
		public int? RowNumberingSeed
		{
			get { return (int?)this.GetValue(RowNumberingSeedProperty); }
			set { this.SetValue(RowNumberingSeedProperty, value); }
		}

		private static void RowNumberingSeedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			RowSelectorSettingsOverride settings = (RowSelectorSettingsOverride)obj;
			if (settings.ColumnLayout != null && settings.ColumnLayout.Grid != null)
				settings.ColumnLayout.Grid.ResetPanelRows();
			settings.OnPropertyChanged("RowNumberingSeed");
		}

		#endregion // RowNumberingSeed

		#region RowNumberingSeedResolved
		/// <summary>
		/// Gets what value for the <see cref="RowNumberingSeed"/> will be used for the <see cref="ColumnLayout"/>.
		/// </summary>
        public int RowNumberingSeedResolved
        {
            get
            {
                if (this.RowNumberingSeed == null)
                {
                    if (this.SettingsObject != null)
                        return ((RowSelectorSettings)this.SettingsObject).RowNumberingSeed;
                }
                else
                    return (int)this.RowNumberingSeed;

                return (int)RowSelectorSettings.RowNumberingSeedProperty.GetMetadata(typeof(RowSelectorSettings)).DefaultValue;
            }
        }
		#endregion // RowNumberingSeedResolved

		#region EnableRowNumbering

		/// <summary>
		/// Identifies the <see cref="EnableRowNumbering"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty EnableRowNumberingProperty = DependencyProperty.Register("EnableRowNumbering", typeof(bool?), typeof(RowSelectorSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(EnableRowNumberingChanged)));

		/// <summary>
		/// Gets / sets if the RowSelectors should show row numbers.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? EnableRowNumbering
		{
			get { return (bool?)this.GetValue(EnableRowNumberingProperty); }
			set { this.SetValue(EnableRowNumberingProperty, value); }
		}

		private static void EnableRowNumberingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			RowSelectorSettingsOverride settingsOverride = (RowSelectorSettingsOverride)obj;
			settingsOverride.OnPropertyChanged("EnableRowNumbering");
		}

		#endregion // EnableRowNumbering

		#region EnableRowNumberingResolved

		/// <summary>
		/// Resolves if row numbering is enabled for this particular <see cref="RowSelectorSettings"/>
		/// </summary>
        public bool EnableRowNumberingResolved
        {
            get
            {
                if (this.EnableRowNumbering == null)
                {
                    if (this.SettingsObject != null)
                        return ((RowSelectorSettings)this.SettingsObject).EnableRowNumbering;
                }
                else
                    return (bool)this.EnableRowNumbering;

                return (bool)RowSelectorSettings.EnableRowNumberingProperty.GetMetadata(typeof(RowSelectorSettings)).DefaultValue;
            }
        }
		#endregion //IsEnabledResolved
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