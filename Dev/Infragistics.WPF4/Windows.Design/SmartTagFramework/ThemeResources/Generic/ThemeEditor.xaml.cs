using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Design.Model;
using System.Diagnostics;
using System.Reflection;
using Infragistics.Windows.Themes;

namespace Infragistics.Windows.Design.SmartTagFramework
{
	/// <summary>
	/// Interaction logic for ThemeEditor.xaml
	/// </summary>
	public partial class ThemeEditor : UserControl
	{
		private bool			_isInitializing;

		/// <summary>
		/// ThemeEditor control.
		/// </summary>
		public ThemeEditor()
		{
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(ThemeEditor_Loaded);
		}

		void ThemeEditor_Loaded(object sender, RoutedEventArgs e)
		{
			// Load up the combobox with the Theme names from the ThemeGrouping for the assembly that contains the owning type for this property.
			DesignerActionPropertyItem propertyItem = this.DataContext as DesignerActionPropertyItem;

			if (propertyItem != null)
			{
				Assembly	assembly	= propertyItem.OwningType.Assembly;
				object []	attribs		= assembly.GetCustomAttributes(typeof(AssemblyThemeGroupingNameAttribute), false);
				if (attribs != null && attribs.Length > 0)
				{
					AssemblyThemeGroupingNameAttribute themeGroupingAttrib = attribs[0] as AssemblyThemeGroupingNameAttribute;
					if (themeGroupingAttrib != null)
						this.cmbThemeNames.ItemsSource = ThemeManager.GetThemes(false, ((AssemblyThemeGroupingNameAttribute)attribs[0]).Name);
				}
			}

			// Wire up the selected event and KeyUp events so we can update the property value when the user makes changes.
			this.cmbThemeNames.SelectionChanged += new SelectionChangedEventHandler(cmbThemeNames_SelectionChanged);
			this.cmbThemeNames.KeyUp			+= new KeyEventHandler(cmbThemeNames_KeyUp);

			// Set the combo's text property to the current property value.
			this._isInitializing	= true;
			this.cmbThemeNames.Text	= (string)propertyItem.Value;
			this._isInitializing	= false;
		}

		void cmbThemeNames_KeyUp(object sender, KeyEventArgs e)
		{
			if (this._isInitializing == false)
			{
				DesignerActionPropertyItem propertyItem = this.DataContext as DesignerActionPropertyItem;
				propertyItem.Value						= ((ComboBox)sender).Text;
			}
		}

		void cmbThemeNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this._isInitializing == false)
			{
				if (((ComboBox)sender).SelectedIndex != -1)
				{
					DesignerActionPropertyItem propertyItem = this.DataContext as DesignerActionPropertyItem;
					propertyItem.Value						= ((ComboBox)sender).SelectedItem;
				}
			}
		}
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