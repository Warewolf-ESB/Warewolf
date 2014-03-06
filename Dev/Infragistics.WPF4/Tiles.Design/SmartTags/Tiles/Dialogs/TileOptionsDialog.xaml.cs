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
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design;
using Infragistics.Windows.Tiles;

namespace Infragistics.Windows.Design.Tiles
{
	/// <summary>
	/// Interaction logic for TileOptionsDialog.xaml
	/// </summary>
	public partial class TileOptionsDialog : Window
	{
		private ModelItem					_tilesControl;
		private EditingContext				_context;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tilesControl"></param>
		/// <param name="context"></param>
		public TileOptionsDialog(ModelItem tilesControl, EditingContext context)
		{
			InitializeComponent();

			this._tilesControl	= tilesControl;
			this._context		= context;
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			if (false == (this.numTotalTiles.Value is int))
			{
				MessageBox.Show(SR.GetString("SmartTag_M_NumberOfTiles"),
							  SR.GetString("SmartTag_T_InvalidValue"),
							  MessageBoxButton.OK,
							  MessageBoxImage.Exclamation);

				return;
			}

			int				totalTilesToAdd					= (int)(this.numTotalTiles.Value);
			ModelProperty	tilesControlItemsModelProperty	= this._tilesControl.Properties["Items"];

			for (int i = 0; i < totalTilesToAdd; i++)
			{
				ModelItem newTileItem = ModelFactory.CreateItem(this._context, typeof(Tile), null);
				newTileItem.Properties["Header"].SetValue(SR.GetString("SmartTag_Default_TileHeader"));

				tilesControlItemsModelProperty.Collection.Add(newTileItem);
			}

			this.Close();
		}

		/// <summary>
		/// 
		/// </summary>
		public static string string_NumberTiles
		{
			get { return SR.GetString("SmartTag_DialogItem_NumberTiles"); }
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