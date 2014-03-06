using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A class which defines the CheckBox settings which will be applied across all <see cref="NodeLayout"/>s of a <see cref="XamDataTree"/>.
    /// </summary>
    public class CheckBoxSettings : SettingsBase
    {
        #region Properties

        #region CheckBoxStyle

        /// <summary>
        /// Identifies the <see cref="CheckBoxStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxStyleProperty = DependencyProperty.Register("CheckBoxStyle", typeof(Style), typeof(CheckBoxSettings), new PropertyMetadata(new PropertyChangedCallback(CheckBoxStyleChanged)));

        /// <summary>
        /// Gets / sets a <see cref="Style"/> which will be applied to the <see cref="CheckBox"/> controls which are available on the <see cref="XamDataTreeNode"/>.
        /// </summary>
        public Style CheckBoxStyle
        {
            get { return (Style)this.GetValue(CheckBoxStyleProperty); }
            set { this.SetValue(CheckBoxStyleProperty, value); }
        }

        private static void CheckBoxStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CheckBoxSettings ctrl = (CheckBoxSettings)obj;
            ctrl.OnPropertyChanged("CheckBoxStyle");
        }

        #endregion // CheckBoxStyle

        #region CheckBoxVisibility

        /// <summary>
        /// Identifies the <see cref="CheckBoxVisibility"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxVisibilityProperty = DependencyProperty.Register("CheckBoxVisibility", typeof(Visibility), typeof(CheckBoxSettings), new PropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(CheckBoxVisibilityChanged)));

        /// <summary>
        /// Gets / sets if a <see cref="CheckBox"/> will be made available on the <see cref="XamDataTreeNode"/> object.
        /// </summary>
        public Visibility CheckBoxVisibility
        {
            get { return (Visibility)this.GetValue(CheckBoxVisibilityProperty); }
            set { this.SetValue(CheckBoxVisibilityProperty, value); }
        }

        private static void CheckBoxVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CheckBoxSettings ctrl = (CheckBoxSettings)obj;
            ctrl.OnPropertyChanged("CheckBoxVisibility");
        }

        #endregion // CheckBoxVisibility

        #region IsCheckBoxThreeState

        /// <summary>
        /// Identifies the <see cref="IsCheckBoxThreeState"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsCheckBoxThreeStateProperty = DependencyProperty.Register("IsCheckBoxThreeState", typeof(bool), typeof(CheckBoxSettings), new PropertyMetadata(true, new PropertyChangedCallback(IsCheckBoxThreeStateChanged)));

        /// <summary>
        /// Gets / sets if the <see cref="CheckBox"/> controls will allow for three state click through when the <see cref="CheckBoxMode"/> is set to <see cref="TreeCheckBoxMode.Manual"/>.  This property will have no effect when set to <see cref="TreeCheckBoxMode.Auto"/>.
        /// </summary>
        public bool IsCheckBoxThreeState
        {
            get { return (bool)this.GetValue(IsCheckBoxThreeStateProperty); }
            set { this.SetValue(IsCheckBoxThreeStateProperty, value); }
        }

        private static void IsCheckBoxThreeStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CheckBoxSettings ctrl = (CheckBoxSettings)obj;
            ctrl.OnPropertyChanged("IsCheckBoxThreeState");
        }

        #endregion // IsCheckBoxThreeState

        #region CheckBoxMode

        /// <summary>
        /// Identifies the <see cref="CheckBoxMode"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxModeProperty = DependencyProperty.Register("CheckBoxMode", typeof(TreeCheckBoxMode), typeof(CheckBoxSettings), new PropertyMetadata(TreeCheckBoxMode.Auto, new PropertyChangedCallback(CheckBoxModeChanged)));

        /// <summary>
        /// Gets / sets the <see cref="TreeCheckBoxMode"/> which will govern what will happen to the <see cref="XamDataTreeNode.IsChecked"/> property of parent and child <see cref="XamDataTreeNode"/> objects when controlling <see cref="XamDataTreeNode"/> is checked.
        /// </summary>
        public TreeCheckBoxMode CheckBoxMode
        {
            get { return (TreeCheckBoxMode)this.GetValue(CheckBoxModeProperty); }
            set { this.SetValue(CheckBoxModeProperty, value); }
        }

        private static void CheckBoxModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CheckBoxSettings ctrl = (CheckBoxSettings)obj;
            ctrl.OnPropertyChanged("CheckBoxMode");
        }

        #endregion // CheckBoxMode

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