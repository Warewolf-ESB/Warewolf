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

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// An object that contains settings for using the <see cref="Infragistics.Controls.Grids.Primitives.ColumnChooserDialog"/> on a particular <see cref="ColumnLayout"/>
    /// </summary>
    public class ColumnChooserSettingsOverride : StyleSettingsOverrideBase
    {
        #region AllowHiddenColumnIndicator

        /// <summary>   
        /// Identifies the <see cref="AllowHiddenColumnIndicator"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowHiddenColumnIndicatorProperty = DependencyProperty.Register("AllowHiddenColumnIndicator", typeof(bool?), typeof(ColumnChooserSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AllowHiddenColumnIndicatorChanged)));

        /// <summary>
        /// Gets / sets if an indicator will be placed in a Column's Header, that indicates that a neighbor column is hidden for a particular ColumnLayout.
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? AllowHiddenColumnIndicator
        {
            get { return (bool?)this.GetValue(AllowHiddenColumnIndicatorProperty); }
            set { this.SetValue(AllowHiddenColumnIndicatorProperty, value); }
        }

        private static void AllowHiddenColumnIndicatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserSettingsOverride settings = (ColumnChooserSettingsOverride)obj;
            settings.OnPropertyChanged("AllowHiddenColumnIndicator");
        }

        #endregion // AllowHiddenColumnIndicator

        #region AllowHiddenColumnIndicatorResolved

        /// <summary>
        /// Gets if an indicator will be placed in a Column's Header, that indicates that a neighbor column is hidden for a particular ColumnLayout.
        /// </summary>
        public bool AllowHiddenColumnIndicatorResolved
        {
            get
            {
                if (this.AllowHiddenColumnIndicator == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnChooserSettings)this.SettingsObject).AllowHiddenColumnIndicator;
                }
                else
                    return (bool)this.AllowHiddenColumnIndicator;

                return (bool)ColumnChooserSettings.AllowHiddenColumnIndicatorProperty.GetMetadata(typeof(ColumnChooserSettings)).DefaultValue;
            }
        }
        #endregion //AllowHiddenColumnIndicatorResolved

        #region AllowHideColumnIcon

        /// <summary>   
        /// Identifies the <see cref="AllowHideColumnIcon"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowHideColumnIconProperty = DependencyProperty.Register("AllowHideColumnIcon", typeof(bool?), typeof(ColumnChooserSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AllowHideColumnIconChanged)));

        /// <summary>
        /// Gets / sets if a button will be placed in the header that will allow users to hide visible Columns for a particular ColumnLayout.
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? AllowHideColumnIcon
        {
            get { return (bool?)this.GetValue(AllowHideColumnIconProperty); }
            set { this.SetValue(AllowHideColumnIconProperty, value); }
        }

        private static void AllowHideColumnIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserSettingsOverride settings = (ColumnChooserSettingsOverride)obj;
            settings.OnPropertyChanged("AllowHideColumnIcon");
        }

        #endregion // AllowHideColumnIcon

        #region AllowHideColumnIconResolved

        /// <summary>
        /// Resolves if a button will be placed in the header that will allow users to hide visible Columns for a particular ColumnLayout.
        /// </summary>
        public bool AllowHideColumnIconResolved
        {
            get
            {
                if (this.AllowHideColumnIcon == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnChooserSettings)this.SettingsObject).AllowHideColumnIcon;
                }
                else
                    return (bool)this.AllowHideColumnIcon;
                return (bool)ColumnChooserSettings.AllowHideColumnIconProperty.GetMetadata(typeof(ColumnChooserSettings)).DefaultValue;
            }
        }
        #endregion //AllowHideColumnIconResolved

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
                if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                    settings = this.ColumnLayout.Grid.ColumnChooserSettings;
                return settings;
            }
        }

        #endregion // SettingsObject

        #endregion // Overrides
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