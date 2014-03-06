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
using System.Windows.Shapes;
using Infragistics.Windows.DockManager;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design;

namespace Infragistics.Windows.Design.DockManager
{
	/// <summary>
	/// Interaction logic for SplitPaneOptionsDialog.xaml
	/// </summary>
	public partial class SplitPaneOptionsDialog : Window
	{
		private ModelItem				_splitPane;
		private Type					_paneTypeToAdd;
		private EditingContext			_context;

		internal SplitPaneOptionsDialog(ModelItem splitPane, EditingContext context, Type paneTypeToAdd)
		{
			InitializeComponent();

			this._splitPane		= splitPane;
			this._paneTypeToAdd	= paneTypeToAdd;
			this._context		= context;

			if (paneTypeToAdd == typeof(ContentPane))
				this.txtNumberOfPanes.Text = SR.GetString("SmartTag_DialogItem_NumberContentPanes");
			else
				this.txtNumberOfPanes.Text = SR.GetString("SmartTag_DialogItem_NumberTabGroupPanes");


			// Blend3/4 doesn't populate the properties collection with attached properties but we can get to them using 'Find'. 
			ModelProperty mp = this._splitPane.Properties.Find(new Microsoft.Windows.Design.Metadata.PropertyIdentifier(typeof(XamDockManager), "InitialLocation")); 
			if (mp == null)
				this.borderPaneLocation.Visibility = Visibility.Collapsed;
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			if (false == (this.numTotalPanes.Value is int))
			{
				MessageBox.Show(SR.GetString("SmartTag_M_NumberOfPanes"),
							  SR.GetString("SmartTag_T_InvalidValue"),
							  MessageBoxButton.OK,
							  MessageBoxImage.Exclamation);

				return;
			}

			this._splitPane.Properties["SplitterOrientation"].SetValue((Orientation)this.cmbOrientation.SelectedIndex);

			// Blend3/4 doesn't seem to populate the properties collection with attached properties but we can get to them using 'Find'
			ModelProperty mp = this._splitPane.Properties.Find(new Microsoft.Windows.Design.Metadata.PropertyIdentifier(typeof(XamDockManager), "InitialLocation"));
			if (mp != null)
				mp.SetValue((InitialPaneLocation)(this.cmbPaneLocation.SelectedIndex));

			// Create and add 'n' Panes of the requested type to the specified SplitPane's Panes collection
			ModelProperty	panesModelProperty	= this._splitPane.Properties["Panes"];
			ModelItem		newPaneItem;
			int				totalPanesToAdd		= (int)(this.numTotalPanes.Value);

			for (int i = 0; i < totalPanesToAdd; i++)
			{
				newPaneItem = ModelFactory.CreateItem(this._context, this._paneTypeToAdd, null);

				if (this._paneTypeToAdd == typeof(ContentPane))
				{
					// Set a default header.
					newPaneItem = ModelFactory.CreateItem(this._context, typeof(ContentPane), null);
					newPaneItem.Properties["Header"].SetValue(SR.GetString("SmartTag_Default_ContentPaneHeader"));
				}
				else
				if (this._paneTypeToAdd == typeof(TabGroupPane))
				{
					newPaneItem = DALHelpers.CreateTabGroupPaneWithTwoContentPanes(this._context);
				}
				
				panesModelProperty.Collection.Add(newPaneItem);
			}

			this.Close();
		}

		/// <summary>
		/// 
		/// </summary>
		public static string string_SplitPaneLocation
		{
			get { return SR.GetString("SmartTag_DialogItem_SplitPaneLocation"); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static string string_SplitterOrientation
		{
			get { return SR.GetString("SmartTag_DialogItem_SplitterOrientation"); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static string string_OK
		{
			get { return SR.GetString("SmartTag_DialogItem_OK"); }
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