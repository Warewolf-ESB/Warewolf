using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Windows.OutlookBar.Internal
{
    /// <summary>
    /// This class is for internal use and is used as a proxy for <see cref="OutlookBarGroup"/>s in the <see cref="NavigationPaneOptionsControl"/>
    /// </summary>
    public class OutlookBarGroupProxy : DependencyObject
    {
        #region Member Variables

        OutlookBarGroup _group; // orginial source for OutlookBarGroupProxy

        #endregion //Member Variables	
    
        #region Constructors

        /// <summary>
        /// Creates a OutlookBarGroupProxy from the specified  <see cref="OutlookBarGroup"/>
        /// </summary>
        /// <param name="group"></param>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/>. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public OutlookBarGroupProxy(OutlookBarGroup group)
        {
			// JM 02-18-09
			System.Diagnostics.Debug.Assert(group != null, "Group is null!!");


            // creates proxy object from original group in XamOutlookBar.Groups
            _group = group;
            IsSelected = !(_group.Visibility == Visibility.Collapsed);


			// JM 02-18-09 - Set our new GroupHeader property which our default template now binds to (instead of Group.Header).  If the Group's Visibility
			//				 is Collapsed we must temporarily make it Visible to get the value of its Header property.  We need to do this
			//				 to force the framework to find any custom Styles for OutlookBarGroup that may be specified up the resolution chain
			//				 which in turn may be binding the Group's Header property to some property on object in the Group's DataContext (this is 
			//				 the typical case when the XamOutlookBar's GroupsSource property is set to bind to a list of objects).
			object groupHeader = group.Header;

			// JM 03-26-12 TFS105371 - This code was causing a flicker in the groups display when the XamOutlookBar is Minimized
			// and the Add/Remove Buttons menu is displayed.  After commenting this out I tested the scenario described above with 
			// the GroupsSource property and it seems to display the Header property correctly, so this change looks OK.
			//if (group.Visibility == Visibility.Collapsed)
			//{
			//    group.Visibility	= Visibility.Visible;
			//    groupHeader			= group.Header;
			//    group.Visibility	= Visibility.Collapsed;
			//}

			this.SetValue(OutlookBarGroupProxy.GroupHeaderPropertyKey, groupHeader);
        }

        #endregion //Constructors	
    
        #region Properties

        #region Group

        /// <summary>
        /// Returns the source <see cref="OutlookBarGroup"/>.
        /// </summary>
		[Browsable(false)]
		public OutlookBarGroup Group
        {
            get { return _group; }
        }

        #endregion //Group	
  
		// JM 02-18-09 
		#region GroupHeader

		private static readonly DependencyPropertyKey GroupHeaderPropertyKey =
			DependencyProperty.RegisterReadOnly("GroupHeader",
			typeof(object), typeof(OutlookBarGroupProxy), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="GroupHeader"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupHeaderProperty =
			GroupHeaderPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the Header from the associated <see cref="OutlookBarGroup"/>
		/// </summary>
		/// <seealso cref="GroupHeaderProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public object GroupHeader
		{
			get
			{
				return (object)this.GetValue(OutlookBarGroupProxy.GroupHeaderProperty);
			}
		}

		#endregion //GroupHeader
  
        #region IsSelected
        /// <summary>
        /// Identifies the <see cref="IsSelected"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(OutlookBarGroupProxy), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Returns sets whether the underlying <see cref="OutlookBarGroup"/> is selected.
        /// </summary>
        /// <seealso cref="IsSelectedProperty"/>
        [Browsable(false)]
        [Bindable(true)]
        public bool IsSelected
        {
            get
            {
                return (bool)this.GetValue(OutlookBarGroupProxy.IsSelectedProperty);
            }
            set
            {
                this.SetValue(OutlookBarGroupProxy.IsSelectedProperty, value);
            }
        }

        #endregion //IsSelected

        #endregion //Properties	
    }

    #region GroupsOrderComparer class






    internal class GroupsOrderComparer : IComparer<OutlookBarGroupProxy>
    {
        #region IComparer<OutlookBarGroupOptions> Members

        public int Compare(OutlookBarGroupProxy x, OutlookBarGroupProxy y)
        {
            return x.Group.InitialOrder - y.Group.InitialOrder;
        }

        #endregion
    }

    #endregion //GroupsOrderComparer class

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