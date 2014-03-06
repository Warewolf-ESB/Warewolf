using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Services;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Provides functionalities for ImageSource editing
    /// </summary>
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_ListBoxTreeItems", Type = typeof(ListBox))]
	[TemplatePart(Name = "PART_ListBoxTreeItemsScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "PART_SelectedImage", Type = typeof(Image))]
    [TemplatePart(Name = "PART_StackPanelBreadCrumb", Type = typeof(StackPanel))]
    public partial class ImageSourceEditor : UserControl
    {
        #region Member Variables

        private EditingContext _context;
        private Popup _popUp;
        private ListBox _lbTreeItems;
		private ScrollViewer _lbTreeItemsScrollViewer;
        private Image _selectedImage;
        private StackPanel _spBreadCrumb;

		private const string _nullString = "{x:Null}";

		private static bool _externalResourceServiceAvailable;

        #endregion //Member Variables

        #region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
        public ImageSourceEditor()
        {
            InitializeComponent();
        }

		static ImageSourceEditor()
		{
			try
			{
				ImageSourceEditor.TryGetExternalResourceService();
				_externalResourceServiceAvailable = true;
			}
			catch
			{
				_externalResourceServiceAvailable = false;
			}
		}

        #endregion //Constructor	
    
        #region Methods

        #region Private Methods

        #region AddBreadCrumbItem

        private void AddBreadCrumbItem(string itemName, string path)
        {
            LinkButton lb = new LinkButton();
            lb.Text = itemName;
            lb.HorizontalAlignment = HorizontalAlignment.Center;
            lb.Padding = new Thickness(2, 3, 2, 0);
            lb.Tag = path;
			lb.IsEnabled = false;	// the last breadcrumb cannot be clicked on

            Label lbl = new Label();
            lbl.Content = @"/";
            lbl.Margin = new Thickness(0, 0, 0, 0);
            lbl.HorizontalAlignment = HorizontalAlignment.Center;

            lb.Click += new RoutedEventHandler(lb_Click);

            _spBreadCrumb.Children.Add(lb);
            _spBreadCrumb.Children.Add(lbl);

			// Enable all previous breadcrumbs
			for (int i = 0; i < _spBreadCrumb.Children.Count - 2; i += 2)
			{
				_spBreadCrumb.Children[i].IsEnabled = true;
			}
		}

        #endregion //AddBreadCrumbItem

        #region CreateListItems

        private void CreateListItems(string selectedDirectoryPath)
        {
            List<ResourceType> imageTypes = Utils.GetImageTypes();
            List<TreeItem> resourceList = new List<TreeItem>();
            ExternalResourceService ers = _context.Services.GetService<ExternalResourceService>();
            if (ers != null)
            {
                foreach (Uri uri in ers.ResourceUris)
                {
                    //check if the resource is an image
                    ResourceType resourceType = Utils.FindResourceType(uri, imageTypes);
                    if (null == resourceType)
                    {
                        continue;
                    }

                    if (uri.OriginalString.Contains(string.Format("component{0}", selectedDirectoryPath)))
                    {
                        TreeItem treeItem = new TreeItem(uri.OriginalString, selectedDirectoryPath);
                        if (!resourceList.Contains(treeItem))
                        {
                            treeItem.ResourceType = resourceType;
                            resourceList.Add(treeItem);
                        }
                    }
                }
            }
            _lbTreeItems.ItemsSource = resourceList;

			// If there are no items in the list, hide the scrollviewer which will reveal a textblock element that displays
			// a string informing the user that there are no entries in the list.
			_lbTreeItemsScrollViewer.Visibility = _lbTreeItems.Items.Count < 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion //CreateListItems

		#region TryGetExternalResourceService

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool TryGetExternalResourceService()
		{
			Type t = typeof(ExternalResourceService);
			if (false == string.IsNullOrEmpty(t.FullName))
				return true;

			return false;
		}

		#endregion //TryGetExternalResourceService

		#endregion //Private Methods

		#endregion //Methods

		#region Event Handlers

		#region btnPopUp_Click

		private void btnPopUp_Click(object sender, RoutedEventArgs e)
        {
            _context = (EditingContext)this.Tag;

			// JM 05-28-10 TFS33048 - If we are not able to get the image resources (as is the case in Blend), display a Message and exit.
			if (false == ImageSourceEditor._externalResourceServiceAvailable)
			{
				MessageBox.Show(SR.GetString("LST_SmartTagFramework_ImageSourceEditor_M_ImageResourcesNotAvailable"), SR.GetString("LST_SmartTagFramework_ImageSourceEditor_T_ImageResourcesNotAvailable"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}


            _popUp = btnPopUp.Template.FindName("PART_Popup", btnPopUp) as Popup;
            _lbTreeItems = btnPopUp.Template.FindName("PART_ListBoxTreeItems", btnPopUp) as ListBox;
			_lbTreeItemsScrollViewer = btnPopUp.Template.FindName("PART_ListBoxTreeItemsScrollViewer", btnPopUp) as ScrollViewer;
			_selectedImage = btnPopUp.Template.FindName("PART_SelectedImage", btnPopUp) as Image;
            _spBreadCrumb = btnPopUp.Template.FindName("PART_StackPanelBreadCrumb", btnPopUp) as StackPanel;

            _popUp.IsOpen = (bool)btnPopUp.IsChecked;
            if (_popUp.IsOpen)
            {
                //Initialize 
				if (_spBreadCrumb.Children.Count <= 0)
					AddBreadCrumbItem("(root)", string.Empty);

				if (_lbTreeItems.Items.Count <= 0 && ImageSourceEditor._externalResourceServiceAvailable)
					CreateListItems(string.Empty);
            }
        }

        #endregion //btnPopUp_Click	
    
        #region btnClose_Click

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _popUp.IsOpen = false;
            btnPopUp.IsChecked = false;
        }

        #endregion //btnClose_Click	
    
        #region lb_Click

        private void lb_Click(object sender, RoutedEventArgs e)
        {
			if (false == ImageSourceEditor._externalResourceServiceAvailable)
				return;

            LinkButton linkButton = sender as LinkButton;
            CreateListItems(linkButton.Tag.ToString());

            bool isForDelete = false;
            for (int i = 0; i < _spBreadCrumb.Children.Count; i++)
            {
                UIElement element = _spBreadCrumb.Children[i];
                LinkButton lb = element as LinkButton;
                if (!isForDelete)
                {
                    if (null == lb || string.IsNullOrEmpty(lb.Tag.ToString()) || linkButton.Text == lb.Text)
                    {
                        continue;
                    }
                }

                if (isForDelete || !linkButton.Tag.ToString().Contains(lb.Tag.ToString()))
                {
                    _spBreadCrumb.Children.Remove(element);
                    i--;
                    isForDelete = true;
                }
            }
        }

        #endregion //lb_Click	
    
        #region lbTreeItems_SelectionChanged

        private void lbTreeItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (false == ImageSourceEditor._externalResourceServiceAvailable)
				return;
			
			if (e.AddedItems.Count <= 0 || null == e.AddedItems[0])
            {
                return;
            }

            TreeItem treeItem = (TreeItem)e.AddedItems[0];
            if (treeItem.IsFile)
            {
                tbImageSource.Text = treeItem.OriginalPath;
                btnClose_Click(null, null);
            }

            if (treeItem.IsDirectory)
            {
                AddBreadCrumbItem(treeItem.Name, treeItem.Path);
                CreateListItems(treeItem.Path);
            }
        }

        #endregion //lbTreeItems_SelectionChanged	
    
        #region popUp_Closed

        private void popUp_Closed(object sender, EventArgs e)
        {
            btnPopUp.IsChecked = false;
        }

        #endregion //popUp_Closed	

		#region btnReset_Click

		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			tbImageSource.Text = _nullString;
		}

		#endregion //btnReset_Click

		#region tbImageSource_TextChanged

		private void tbImageSource_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (tbImageSource.Text == _nullString || tbImageSource.Text == string.Empty)
				btnReset.IsEnabled = false;
			else
				btnReset.IsEnabled = true;
		}

		#endregion //tbImageSource_TextChanged

		#endregion //Event Handlers
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