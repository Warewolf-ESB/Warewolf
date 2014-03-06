using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Infragistics.Shared;
using Infragistics.Windows.OutlookBar;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design.Services;


namespace Infragistics.Windows.Design.OutlookBar
{
    /// <summary>
    /// The following class implements an adorner provider for the OutlookBarGroup. 
    /// The adorner is a edit control, which changes some properties of the OutlookBarGroup.
    /// </summary>
    public class OutlookBarGroupAdornerProvider : PrimarySelectionAdornerProvider
    {
        #region Member Variables

        private OutlookBarGroupAdorner _genericAdorner;
        private SmartButton _smartButtonAdorner;

        private AdornerPanel _adornerPanel;
        private ModelItem _adornedControlModel;

        #endregion //Member Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="OutlookBarGroupAdornerProvider"/>
        /// </summary>
        public OutlookBarGroupAdornerProvider()
        {
            _genericAdorner = new OutlookBarGroupAdorner();
            _smartButtonAdorner = new SmartButton();
            _adornerPanel = new AdornerPanel();
            _adornerPanel.IsContentFocusable = true;

            //Workaround for a known Microsoft's bug about Delete button in design time
            CommandBinding DeleteCmdBinding = new CommandBinding(ApplicationCommands.Delete, OpenCmdExecuted, OpenCmdCanExecute);
            _adornerPanel.CommandBindings.Add(DeleteCmdBinding);

            _smartButtonAdorner.btnSmart.Click += new RoutedEventHandler(btnSmart_Click);
            _genericAdorner.ClosePopup += new RoutedEventHandler(outlookBarGroupAdorner_ClosePopup);

            // Initialize the slider when it is loaded.
            _genericAdorner.Loaded += new RoutedEventHandler(genericAdorner_Loaded);
            _genericAdorner.tbHeader.TextChanged += new TextChangedEventHandler(tbHeader_TextChanged);
            _genericAdorner.cbIsSelected.Click += new RoutedEventHandler(cbIsSelected_Click);

            _genericAdorner.lbAddContentTree.Click += new RoutedEventHandler(lbAddContentTree_Click);
            _genericAdorner.lbAddContentRadio.Click += new RoutedEventHandler(lbAddContentRadio_Click);
            _genericAdorner.lbAddContentCalendar.Click += new RoutedEventHandler(lbAddContentCalendar_Click);
        }

        #endregion //Constructor

        #region Base Class Overrides

        #region Activate

        /// <summary>
        /// Called when adorners are requested for the first time by the designer.
        /// </summary>

		protected override void Activate(ModelItem item)



		{
            // The following method is called when the adorner is activated.
            // It creates the adorner control, sets up the adorner panel,
            // and attaches a ModelItem to the adorned control.
            #region Positioning

            AdornerPlacementCollection smartBtnPlacement = new AdornerPlacementCollection();
            AdornerPlacementCollection animatedProgressBarPlacement = new AdornerPlacementCollection();

            //The adorner has the same size as its content
            smartBtnPlacement.SizeRelativeToAdornerDesiredHeight(1.0, 0);
            smartBtnPlacement.SizeRelativeToAdornerDesiredWidth(1.0, 0);

            //Set x = panel's width
            smartBtnPlacement.PositionRelativeToContentHeight(0.0, 0);
            smartBtnPlacement.PositionRelativeToContentWidth(1.0, 0);

            //Set y = adorner's height, x = adorner's width
            smartBtnPlacement.PositionRelativeToAdornerHeight(-0.2, -2);
            smartBtnPlacement.PositionRelativeToAdornerWidth(-1.0, -2);

            AdornerPanel.SetPlacements(_smartButtonAdorner, smartBtnPlacement);

            animatedProgressBarPlacement.SizeRelativeToAdornerDesiredHeight(1.0, 0);
            animatedProgressBarPlacement.SizeRelativeToAdornerDesiredWidth(1.0, 0);

            animatedProgressBarPlacement.PositionRelativeToContentHeight(0, -(2 + _smartButtonAdorner.Height));
            animatedProgressBarPlacement.PositionRelativeToContentWidth(1.0, 4 - 5);

            AdornerProperties.SetOrder(_adornerPanel, AdornerOrder.Foreground);
            AdornerPanel.SetPlacements(_genericAdorner, animatedProgressBarPlacement);

            #endregion //Positioning

            #region Intialization

            // Save the ModelItem and hook into when it changes.            
            _adornedControlModel = item;
            _adornedControlModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(AdornedControlModel_PropertyChanged);

            #endregion //Intialization

            _adornerPanel.Children.Add(_smartButtonAdorner);
            this.Adorners.Add(_adornerPanel);


			base.Activate(item);



		}

        #endregion //Activate

        #region Deactivate

        /// <summary>
        /// Called when an adorner provider is about to be discarded by the designer.
        /// </summary>
        protected override void Deactivate()
        {
            _adornerPanel.Children.Remove(_smartButtonAdorner);
            this.Adorners.Clear();

            _adornedControlModel.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(AdornedControlModel_PropertyChanged);
            base.Deactivate();
        }

        #endregion //Deactivate

        #endregion //Base Class Overrides

		#region Methods

		#region Private Methods

		#region GetCurrentHeader

		private string GetCurrentHeader()
        {   //Value returns the value requested by the user 
            //ComputedValue returns the real value, some controls have value ranges 

            object header = _adornedControlModel.Properties["Header"].ComputedValue;
            header = null == header ? string.Empty : header;
            return header.ToString();
        }

        #endregion //GetCurrentHeader

        #region GetCurrentIsSelected

        private bool GetCurrentIsSelected()
        {
            bool isSelected = (bool)_adornedControlModel.Properties["IsSelected"].ComputedValue;
            return isSelected;
        }

        #endregion //GetCurrentIsSelected

        #region AddContentWithPredefinedContent

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Windows.Markup.XamlReader.#Parse(System.String)")]
		private void AddContentWithPredefinedContent(string xamlContent)
        {
			ModelItem obarContent = null;

			object o = System.Windows.Markup.XamlReader.Parse(xamlContent);
			obarContent = ModelFactory.CreateItem(this.Context, o);





			if ((null == _adornedControlModel.Content.Value || (0 == _adornedControlModel.Content.Value.Content.Collection.Count
                && obarContent.ItemType == _adornedControlModel.Content.Value.ItemType))
                && MessageBox.Show(SR.GetString("LST_OutlookBar_OutlookBarGroupAdorner_MessageBoxText"), SR.GetString("LST_OutlookBar_OutlookBarGroupAdorner_MessageBoxTitle"),
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
				_adornedControlModel.Content.SetValue(obarContent);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, SR.GetString("LE_OutlookBar_XamOutlookBarAdorner_AddContentWithPredefinedContent"));
            }
        }

        #endregion //AddContentWithPredefinedContent

		// JM 04-03-09 TFS15467
		#region GetNormalizedXamlStringResource

		internal static string GetNormalizedXamlStringResource(string xamlResourceName)
		{
			string rawXaml = SR.GetString(xamlResourceName);

			// JM 08-25-09 TFS20727
			rawXaml = rawXaml.Replace("Items", SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_PredefinedContent_Items"));

			rawXaml = rawXaml.Replace("Item 1", SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_PredefinedContent_Item1"));
			rawXaml = rawXaml.Replace("Item 2", SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_PredefinedContent_Item2"));
			rawXaml = rawXaml.Replace("Item 3", SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_PredefinedContent_Item3"));
			rawXaml = rawXaml.Replace("Radio 1", SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_PredefinedContent_Radio1"));
			rawXaml = rawXaml.Replace("Radio 2", SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_PredefinedContent_Radio2"));
			rawXaml = rawXaml.Replace("Radio 3", SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_PredefinedContent_Radio3"));

			return rawXaml;
		}

		#endregion //GetNormalizedXamlStringResource

		#endregion //Private Methods

		#endregion //Methods

		#region Events

		#region ExecutedCommand



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
        }

        #endregion //ExecutedCommand

        #endregion //Events

        #region Event Handlers

        #region btnSmart_Click

        private void btnSmart_Click(object sender, RoutedEventArgs e)
        {
            if (_smartButtonAdorner.IsRightArrow)
            {
                _adornerPanel.Children.Add(_genericAdorner);
            }
            else
            {
                _adornerPanel.Children.Remove(_genericAdorner);
            }
        }

        #endregion //btnSmart_Click

        #region outlookBarGroupAdorner_ClosePopup

        private void outlookBarGroupAdorner_ClosePopup(object sender, RoutedEventArgs e)
        {
            _adornerPanel.Children.Remove(_genericAdorner);
            _smartButtonAdorner.IsRightArrow = false;
        }

        #endregion //outlookBarGroupAdorner_ClosePopup

        #region AdornedControlModel_PropertyChanged

        private void AdornedControlModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
			if (e.PropertyName == "Header")
			{
				_genericAdorner.tbHeader.Text = GetCurrentHeader().ToString();
			}

			if (e.PropertyName == "IsSelected")
			{
				_genericAdorner.cbIsSelected.IsChecked = GetCurrentIsSelected();
			}
        }

        #endregion //AdornedControlModel_PropertyChanged

        #region genericAdorner_Loaded

        private void genericAdorner_Loaded(object sender, RoutedEventArgs e)
        {
            _genericAdorner.tbHeader.Text = GetCurrentHeader();
            _genericAdorner.cbIsSelected.IsChecked = GetCurrentIsSelected();
        }

        #endregion //genericAdorner_Loaded

        #region tbHeader_TextChanged

        private void tbHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_genericAdorner.tbHeader.Text == GetCurrentHeader())
            {
                return;
            }

            ModelProperty headerProperty = _adornedControlModel.Properties["Header"];

			// JM 07-27-09 TFS 19859 - Don't trim the entered text.
            //headerProperty.SetValue(_genericAdorner.tbHeader.Text.Trim());
			//headerProperty.SetValue(_genericAdorner.tbHeader.Text);
			headerProperty.ComputedValue = _genericAdorner.tbHeader.Text;
		}

        #endregion //tbHeader_TextChanged

        #region cbIsSelected_Click

        private void cbIsSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_genericAdorner.cbIsSelected.IsChecked == GetCurrentIsSelected())
            {
                return;
            }

            ModelProperty isSelectedProperty = _adornedControlModel.Properties["IsSelected"];
            //isSelectedProperty.SetValue(_genericAdorner.cbIsSelected.IsChecked);
			isSelectedProperty.ComputedValue = _genericAdorner.cbIsSelected.IsChecked;
		}

        #endregion //cbIsSelected_Click

        #region lbAddContentTree_Click

        private void lbAddContentTree_Click(object sender, RoutedEventArgs e)
        {
			// JM 04-03-09 TFS15467
            //string xamlContent = SR.GetString("LST_OutlookBar_GroupTreeContent");
			string xamlContent = OutlookBarGroupAdornerProvider.GetNormalizedXamlStringResource("LST_OutlookBar_GroupTreeContent");
			AddContentWithPredefinedContent(xamlContent);
        }

        #endregion //lbAddContentTree_Click

        #region lbAddContentRadio_Click

        private void lbAddContentRadio_Click(object sender, RoutedEventArgs e)
        {
			// JM 04-03-09 TFS15467
			//string xamlContent = SR.GetString("LST_OutlookBar_GroupRadioContent");
			string xamlContent = OutlookBarGroupAdornerProvider.GetNormalizedXamlStringResource("LST_OutlookBar_GroupRadioContent");
			AddContentWithPredefinedContent(xamlContent);
        }

        #endregion //lbAddContentRadio_Click

        #region lbAddContentCalendar_Click

        private void lbAddContentCalendar_Click(object sender, RoutedEventArgs e)
        {
			// JM 04-03-09 TFS15467
			//string xamlContent = SR.GetString("LST_OutlookBar_GroupCalendarContent");
			string xamlContent = OutlookBarGroupAdornerProvider.GetNormalizedXamlStringResource("LST_OutlookBar_GroupCalendarContent");
			AddContentWithPredefinedContent(xamlContent);
        }

        #endregion //lbAddContentCalendar_Click

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