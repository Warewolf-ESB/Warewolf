using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A class which defines the CheckBox settings which will be applied a single <see cref="NodeLayout"/> of a <see cref="XamDataTree"/>.
	/// </summary>
    public class CheckBoxSettingsOverride : SettingsBaseOverride
    {
        #region Overrides

        #region SettingsObject

        /// <summary>
        /// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsBaseOverride"/>
        /// </summary>
        protected override SettingsBase SettingsObject
        {
            get
            {
                SettingsBase settings = null;
                if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                    settings = this.NodeLayout.Tree.CheckBoxSettings;
                return settings;
            }
        }

        #endregion // SettingsObject

        /// <summary>
        /// A method designed to raise the OnPropertyChanged for the resolved properties so that changes from higher levels can be reflected through bindings correctly.
        /// </summary>
        protected internal override void InvalidateResolvedProperties()
        {
            base.InvalidateResolvedProperties();

            this.OnPropertyChanged("CheckBoxVisibilityResolved");
            this.OnPropertyChanged("CheckBoxModeResolved");
            this.OnPropertyChanged("IsCheckBoxThreeStateResolved");
        }
        
        #endregion // Overrides

        #region Properties

        #region CheckBoxVisiblity

        /// <summary>
        /// Identifies the <see cref="CheckBoxVisibility"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxVisibilityProperty = DependencyProperty.Register("CheckBoxVisiblity", typeof(Visibility?), typeof(CheckBoxSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(CheckBoxVisibilityChanged)));

        /// <summary>
        /// Gets / sets if a <see cref="CheckBox"/> will be made available on the <see cref="XamDataTreeNode"/> object.
        /// </summary>
        [TypeConverter(typeof(NullableEnumTypeConverter<Visibility>))]
        public Visibility? CheckBoxVisibility
        {
            get { return (Visibility?)this.GetValue(CheckBoxVisibilityProperty); }
            set { this.SetValue(CheckBoxVisibilityProperty, value); }
        }

        private static void CheckBoxVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CheckBoxSettingsOverride ctrl = (CheckBoxSettingsOverride)obj;
            ctrl.OnPropertyChanged("CheckBoxVisibility");
            ctrl.OnPropertyChanged("CheckBoxVisibilityResolved");
        }

        #endregion // CheckBoxVisiblity 

        #region CheckBoxVisibilityResolved

        /// <summary>
        /// Resolves the <see cref="CheckBoxVisibility"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public Visibility CheckBoxVisibilityResolved
        {
            get
            {
                Visibility retVal = Visibility.Collapsed;

                if (this.CheckBoxVisibility != null)
                {
                    retVal = (Visibility)this.CheckBoxVisibility;
                }
                else if (this.SettingsObject != null)
                {
                    retVal = ((CheckBoxSettings)this.SettingsObject).CheckBoxVisibility;
                }
                return retVal;
            }
        }

        #endregion // CheckBoxVisibilityResolved

        #region CheckBoxStyle

        /// <summary>
        /// Identifies the <see cref="CheckBoxStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxStyleProperty = DependencyProperty.Register("CheckBoxStyle", typeof(Style), typeof(CheckBoxSettingsOverride), new PropertyMetadata(null,new PropertyChangedCallback(CheckBoxStyleChanged)));

        /// <summary>
        /// Gets / sets a <see cref="Style"/> which will be applied to the <see cref="CheckBox"/> controls which are available on the <see cref="NodeLayout"/>.
        /// </summary>
        public Style CheckBoxStyle
        {
            get { return (Style)this.GetValue(CheckBoxStyleProperty); }
            set { this.SetValue(CheckBoxStyleProperty, value); }
        }

        private static void CheckBoxStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CheckBoxSettingsOverride ctrl = (CheckBoxSettingsOverride)obj;
            ctrl.OnPropertyChanged("CheckBoxStyle");
            ctrl.OnPropertyChanged("CheckBoxStyleResolved");
            if (ctrl.NodeLayout != null && ctrl.NodeLayout.Tree != null)
                ctrl.NodeLayout.Tree.InvalidateScrollPanel(false);
        }

        #endregion // CheckBoxStyle 
			
        #region CheckBoxStyleResolved

        /// <summary>
        /// Resolves the <see cref="CheckBoxStyle"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public Style CheckBoxStyleResolved
        {
            get
            {
                Style retValue = null;
                if (this.CheckBoxStyle != null)
                {
                    retValue = this.CheckBoxStyle;
                }
                else if (this.SettingsObject != null)
                {
                    retValue = ((CheckBoxSettings)this.SettingsObject).CheckBoxStyle;
                }
                return retValue;
            }
        }

        #endregion // CheckBoxStyleResolved

        #region CheckBoxModeResolved

        /// <summary>
        /// Resolves the <see cref="CheckBoxSettings.CheckBoxMode"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        protected internal TreeCheckBoxMode CheckBoxModeResolved
        {
            get
            {
                TreeCheckBoxMode retVal = TreeCheckBoxMode.Auto;
                if (this.SettingsObject != null)
                    retVal = ((CheckBoxSettings)this.SettingsObject).CheckBoxMode;
                return retVal ;
            }
        }

        #endregion // CheckBoxModeResolved

        #region IsCheckBoxThreeStateResolved

        /// <summary>
        /// Gets if the <see cref="XamDataTreeNode"/>'s <see cref="CheckBox"/>es are set to three state.
        /// </summary>
        public bool IsCheckBoxThreeStateResolved
        {
            get
            {                 
                CheckBoxSettings cbs = this.SettingsObject as CheckBoxSettings;
                
                return (cbs != null && cbs.CheckBoxMode == TreeCheckBoxMode.Manual && cbs.IsCheckBoxThreeState);
            }
        }

        #endregion // IsCheckBoxThreeStateResolved

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