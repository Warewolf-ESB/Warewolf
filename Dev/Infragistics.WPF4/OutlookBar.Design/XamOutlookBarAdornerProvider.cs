using System;
using System.Windows;
using Infragistics.Shared;
using Infragistics.Windows.OutlookBar;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design.Services;
using System.Windows.Markup;


namespace Infragistics.Windows.Design.OutlookBar
{

    /// <summary>
    /// The following class implements an adorner provider for the XamOutlookBar. 
    /// The adorner is a edit control, which changes some properties of the XamOutlookBar.
    /// </summary>
	public class XamOutlookBarAdornerProvider : PrimarySelectionAdornerProvider
    {
        #region Member Variables

        private XamOutlookBarAdorner _genericAdorner;
        private SmartButton _smartButtonAdorner;

        private AdornerPanel _adornerPanel;
        private ModelItem _adornedControlModel;

        #endregion //Member variables

        #region Constructor

        /// <summary>
        /// Creates a XamOutlookBarAdornerProvider
        /// </summary>
        public XamOutlookBarAdornerProvider()
        {
            _genericAdorner = new XamOutlookBarAdorner();
            _smartButtonAdorner = new SmartButton();
            _adornerPanel = new AdornerPanel();
            _adornerPanel.IsContentFocusable = true;

            _smartButtonAdorner.btnSmart.Click += new RoutedEventHandler(btnSmart_Click);
            _genericAdorner.ClosePopup += new RoutedEventHandler(xamOutlookBarAdorner_ClosePopup);

            // Initialize the slider when it is loaded.
            _genericAdorner.Loaded += new RoutedEventHandler(genericAdorner_Loaded);

            _genericAdorner.lbAddGroupGrid.Click += new RoutedEventHandler(lbAddGroupGrid_Click);
            _genericAdorner.lbAddGroupTree.Click += new RoutedEventHandler(lbAddGroupTree_Click);
            _genericAdorner.lbAddGroupRadio.Click += new RoutedEventHandler(lbAddGroupRadio_Click);
            _genericAdorner.lbAddGroupCalendar.Click += new RoutedEventHandler(lbAddGroupCalendar_Click);

            _genericAdorner.lbShowMoreButtons.Click += new RoutedEventHandler(lbShowMoreButtons_Click);
            _genericAdorner.lbShowFewerButtons.Click += new RoutedEventHandler(lbShowFewerButtons_Click);
            _genericAdorner.lbAutoNumberButtons.Click += new RoutedEventHandler(lbAutoNumberButtons_Click);
        }

        #endregion //Constructor

        #region Base Class Overrides

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

            #endregion

            #region Intialization

            // Save the ModelItem and hook into when it changes.
            // This enables updating the slider position when 
            // a new Background value is set.
            _adornedControlModel = item;

            #endregion

            _adornerPanel.Children.Add(_smartButtonAdorner);

            this.Adorners.Add(_adornerPanel);


            base.Activate(item);



		}

        /// <summary>
        /// Called when an adorner provider is about to be discarded by the designer.
        /// </summary>
        protected override void Deactivate()
        {
            _adornerPanel.Children.Remove(_smartButtonAdorner);
            this.Adorners.Clear();

            base.Deactivate();
        }

        #endregion //Base Class Overrides

        #region Methods

        #region Private Methods

        #region AddGroupWithPredefinedContent

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Windows.Markup.XamlReader.#Parse(System.String)")]
		private void AddGroupWithPredefinedContent(string xamlContent)
        {
            try
            {
                int newGroupNumber = _adornedControlModel.Properties["Groups"].Collection.Count + 1;
                string grHeader = SR.GetString("LST_OutlookBar_XamOutlookBarAdorner_AddGroupWithPredefinedContent", newGroupNumber);
				
                ModelItem mi = _adornedControlModel.Properties["Groups"].Collection.Add(new OutlookBarGroup());

				ModelItem obarContent = null;

				object o = XamlReader.Parse(xamlContent);
				obarContent = ModelFactory.CreateItem(this.Context, o);




				mi.Content.SetValue(obarContent);

                mi.Properties["Header"].SetValue(grHeader);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, SR.GetString("LE_OutlookBar_XamOutlookBarAdorner_AddGroupWithPredefinedContent"));
            }
        }

        #endregion //AddGroupWithPredefinedContent	
    
        #region InitializeAdorner

        private void InitializeAdorner()
        {

            XamOutlookBar outlookBar = _adornedControlModel.View.PlatformObject as XamOutlookBar;



			int computedGroupCount = outlookBar.NavigationAreaGroups.Count;

            //check if there are any groups
            bool isNoGroups = outlookBar.Groups.Count > 0;
            _genericAdorner.lbShowMoreButtons.IsEnabled = isNoGroups;
            _genericAdorner.lbShowFewerButtons.IsEnabled = isNoGroups;
            _genericAdorner.lbAutoNumberButtons.IsEnabled = isNoGroups;

            //Intialize "Show More Buttons" button
            if (outlookBar.Groups.Count == computedGroupCount || outlookBar.NavigationAreaMaxGroups == -1)
            {
                _genericAdorner.lbShowMoreButtons.IsEnabled = false;
            }

            //Initialize "Show Fewer Buttons" button
            if (0 == outlookBar.NavigationAreaGroups.Count)
            {
                _genericAdorner.lbShowFewerButtons.IsEnabled = false;
            }

            //Intialize "Show Max # of Buttons" button
            _genericAdorner.lbAutoNumberButtons.IsEnabled = outlookBar.NavigationAreaMaxGroups != -1;
        }

        #endregion //InitializeAdorner	

        #endregion //Private Methods

        #endregion //Methods

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
        
        #region genericAdorner_Loaded

        private void genericAdorner_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeAdorner();
        }

        #endregion //genericAdorner_Loaded	    

        #region lbAddGroupGrid_Click

        private void lbAddGroupGrid_Click(object sender, RoutedEventArgs e)
        {
            string xamlContent = SR.GetString("LST_OutlookBar_GroupGridContent");
            AddGroupWithPredefinedContent(xamlContent);
            InitializeAdorner();
        }

        #endregion //lbAddGroupGrid_Click

        #region lbAddGroupTree_Click

        private void lbAddGroupTree_Click(object sender, RoutedEventArgs e)
        {
			// JM 04-03-09 TFS15467
            //string xamlContent = SR.GetString("LST_OutlookBar_GroupTreeContent");
			string xamlContent = OutlookBarGroupAdornerProvider.GetNormalizedXamlStringResource("LST_OutlookBar_GroupTreeContent");
			AddGroupWithPredefinedContent(xamlContent);
            InitializeAdorner();
        }

        #endregion //lbAddGroupTree_Click

        #region lbAddGroupRadio_Click

        private void lbAddGroupRadio_Click(object sender, RoutedEventArgs e)
        {
			// JM 04-03-09 TFS15467
			//string xamlContent = SR.GetString("LST_OutlookBar_GroupRadioContent");
			string xamlContent = OutlookBarGroupAdornerProvider.GetNormalizedXamlStringResource("LST_OutlookBar_GroupRadioContent");
			AddGroupWithPredefinedContent(xamlContent);
            InitializeAdorner();
        }

        #endregion //lbAddGroupRadio_Click

        #region lbAddGroupCalendar_Click

        private void lbAddGroupCalendar_Click(object sender, RoutedEventArgs e)
        {
			// JM 04-03-09 TFS15467
			//string xamlContent = SR.GetString("LST_OutlookBar_GroupCalendarContent");
			string xamlContent = OutlookBarGroupAdornerProvider.GetNormalizedXamlStringResource("LST_OutlookBar_GroupCalendarContent");
			AddGroupWithPredefinedContent(xamlContent);
            InitializeAdorner();
        }

        #endregion //lbAddGroupCalendar_Click

        #region lbShowMoreButtons_Click

        private void lbShowMoreButtons_Click(object sender, RoutedEventArgs e)
        {
            ModelProperty navigationAreaMaxGroups = _adornedControlModel.Properties["NavigationAreaMaxGroups"];
            int groupCount = (int)navigationAreaMaxGroups.ComputedValue;
            groupCount++;
            navigationAreaMaxGroups.SetValue(groupCount);

            //check if the new group number is accepted by the grid

			XamOutlookBar outlookBar = _adornedControlModel.View.PlatformObject as XamOutlookBar;



			int computedGroupCount = outlookBar.NavigationAreaGroups.Count;

            //if the new group number is not accepted, we return the previous value
            if (groupCount > computedGroupCount)
            {
                groupCount--;
                navigationAreaMaxGroups.SetValue(groupCount);
                return;
            }

            if (outlookBar.Groups.Count == (int)navigationAreaMaxGroups.ComputedValue)
            {
                _genericAdorner.lbShowMoreButtons.IsEnabled = false;
            }

            _genericAdorner.lbShowFewerButtons.IsEnabled = true;
            _genericAdorner.lbAutoNumberButtons.IsEnabled = true;
        }

        #endregion //lbShowMoreButtons_Click

        #region lbShowFewerButtons_Click

        private void lbShowFewerButtons_Click(object sender, RoutedEventArgs e)
        {
            ModelProperty navigationAreaMaxGroups = _adornedControlModel.Properties["NavigationAreaMaxGroups"];
            int groupCount = (int)navigationAreaMaxGroups.ComputedValue;


			XamOutlookBar outlookBar = _adornedControlModel.View.PlatformObject as XamOutlookBar;




			int computedGroupCount = outlookBar.NavigationAreaGroups.Count;

            if (groupCount == -1)
            {
                groupCount = computedGroupCount;
            }

            groupCount--;

            if (groupCount >= 0)
            {
                navigationAreaMaxGroups.SetValue(groupCount);
                _genericAdorner.lbShowMoreButtons.IsEnabled = true;
                _genericAdorner.lbAutoNumberButtons.IsEnabled = true;
            }

            if (0 == outlookBar.NavigationAreaGroups.Count)
            {
                _genericAdorner.lbShowFewerButtons.IsEnabled = false;
            }
        }

        #endregion //lbShowFewerButtons_Click

        #region lbAutoNumberButtons_Click

        private void lbAutoNumberButtons_Click(object sender, RoutedEventArgs e)
        {
            ModelProperty navigationAreaMaxGroups = _adornedControlModel.Properties["NavigationAreaMaxGroups"];
            navigationAreaMaxGroups.SetValue(-1);

            _genericAdorner.lbAutoNumberButtons.IsEnabled = false;
            _genericAdorner.lbShowMoreButtons.IsEnabled = false;


			XamOutlookBar outlookBar = _adornedControlModel.View.PlatformObject as XamOutlookBar;




			int groupsCount = outlookBar.Groups.Count;
            if (groupsCount > 0)
            {
                _genericAdorner.lbShowFewerButtons.IsEnabled = true;
            }
        }

        #endregion //lbAutoNumberButtons_Click

        #region xamOutlookBarAdorner_ClosePopup

        private void xamOutlookBarAdorner_ClosePopup(object sender, RoutedEventArgs e)
        {
            _adornerPanel.Children.Remove(_genericAdorner);
            _smartButtonAdorner.IsRightArrow = false;
        }

        #endregion //xamOutlookBarAdorner_ClosePopup	    

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