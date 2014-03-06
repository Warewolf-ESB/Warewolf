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

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// An object that contains settings for using the <see cref="Infragistics.Controls.Grids.Primitives.ColumnChooserDialog"/> on the <see cref="XamGrid"/>
    /// </summary>
    public class ColumnChooserSettings : StyleSettingsBase
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnChooserSettings"/> class.
        /// </summary>
        public ColumnChooserSettings()
        {
            this.HiddenColumnIndicatorToolTipText = SRGrid.GetString("ColumnChooserToolTipText");
            this.ColumnChooserDisplayText = SRGrid.GetString("ColumnChooserDisplayText");
        }

        #endregion // Constructor

        #region AllowHiddenColumnIndicator

        /// <summary>
        /// Identifies the <see cref="AllowHiddenColumnIndicator"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowHiddenColumnIndicatorProperty = DependencyProperty.Register("AllowHiddenColumnIndicator", typeof(bool), typeof(ColumnChooserSettings), new PropertyMetadata(false, new PropertyChangedCallback(AllowHiddenColumnIndicatorChanged)));

        /// <summary>
        /// Gets / sets if an indicator will be placed in a Column's Header, that indicates that a neighbor column is hidden.
        /// </summary>
        public bool AllowHiddenColumnIndicator
        {
            get { return (bool)this.GetValue(AllowHiddenColumnIndicatorProperty); }
            set { this.SetValue(AllowHiddenColumnIndicatorProperty, value); }
        }

        private static void AllowHiddenColumnIndicatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserSettings settings = (ColumnChooserSettings)obj;
            settings.OnPropertyChanged("AllowHiddenColumnIndicator");
        }

        #endregion // AllowHiddenColumnIndicator

        #region AllowHideColumnIcon

        /// <summary>
        /// Identifies the <see cref="AllowHideColumnIcon"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowHideColumnIconProperty = DependencyProperty.Register("AllowHideColumnIcon", typeof(bool), typeof(ColumnChooserSettings), new PropertyMetadata(false, new PropertyChangedCallback(AllowHideColumnIconChanged)));

        /// <summary>
        /// Gets / sets if a button will be placed in the header that will allow users to hide visible Columns.
        /// </summary>
        public bool AllowHideColumnIcon
        {
            get { return (bool)this.GetValue(AllowHideColumnIconProperty); }
            set { this.SetValue(AllowHideColumnIconProperty, value); }
        }

        private static void AllowHideColumnIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserSettings settings = (ColumnChooserSettings)obj;
            settings.OnPropertyChanged("AllowHideColumnIcon");
        }

        #endregion // AllowHideColumnIcon

        #region HiddenColumnIndicatorToolTipText

        /// <summary>
        /// Identifies the <see cref="HiddenColumnIndicatorToolTipText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HiddenColumnIndicatorToolTipTextProperty = DependencyProperty.Register("HiddenColumnIndicatorToolTipText", typeof(string), typeof(ColumnChooserSettings), new PropertyMetadata(new PropertyChangedCallback(HiddenColumnIndicatorToolTipTextChanged)));

        /// <summary>
        /// Gets/Sets the text that should be used over the indicator in the header of a column that indicates that there are adjacent columns which are hidden.
        /// </summary>
        public string HiddenColumnIndicatorToolTipText
        {
            get { return (string)this.GetValue(HiddenColumnIndicatorToolTipTextProperty); }
            set { this.SetValue(HiddenColumnIndicatorToolTipTextProperty, value); }
        }

        private static void HiddenColumnIndicatorToolTipTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // HiddenColumnIndicatorToolTipText 

        #region ColumnChooserDisplayText

        /// <summary>
        /// Identifies the <see cref="ColumnChooserDisplayText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ColumnChooserDisplayTextProperty = DependencyProperty.Register("ColumnChooserDisplayText", typeof(string), typeof(ColumnChooserSettings), new PropertyMetadata(new PropertyChangedCallback(ColumnChooserDisplayTextChanged)));

        /// <summary>
        /// Gets/Sets the text that's used inside the ColumnChooserDialog's header and in the dropdown menu of the Indicator, to launch the ColumnChooserDialog.
        /// </summary>
        public string ColumnChooserDisplayText
        {
            get { return (string)this.GetValue(ColumnChooserDisplayTextProperty); }
            set { this.SetValue(ColumnChooserDisplayTextProperty, value); }
        }

        private static void ColumnChooserDisplayTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserSettings settings = (ColumnChooserSettings)obj;

            if (settings.Grid != null)
                settings.Grid.ColumnChooserDialog.Invalidate();
        }

        #endregion // ColumnChooserDisplayText 

        #region InitialLocation

        /// <summary>
        /// Identifies the <see cref="InitialLocation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty InitialLocationProperty = DependencyProperty.Register("InitialLocation", typeof(Point?), typeof(ColumnChooserSettings), new PropertyMetadata(null, new PropertyChangedCallback(InitialLocationChanged)));

        /// <summary>
        /// The Initial location that the <see cref="Infragistics.Controls.Grids.Primitives.ColumnChooserDialog"/> will appear when its open. 
        /// </summary>
        /// <remarks>
        /// If the value is null, it will appear in the center of the <see cref="XamGrid"/>
        /// The point is relative to the <see cref="XamGrid"/>
        /// </remarks>
        public Point? InitialLocation
        {
            get { return (Point?)this.GetValue(InitialLocationProperty); }
            set { this.SetValue(InitialLocationProperty, value); }
        }

        private static void InitialLocationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // InitialLocation 				

        #region OnStyleChanged
        /// <summary>
        /// Invoked when the Style property changes.
        /// </summary>
        protected override void OnStyleChanged()
        {
            if (this.Grid != null)
            {
                this.Grid.ColumnChooserDialog.Style = this.Style;
                this.Grid.ColumnChooserDialog.Invalidate();
            }
        }
        #endregion // OnStyleChanged

        #region AllowColumnMoving

        /// <summary>
        /// Identifies the <see cref="AllowColumnMoving"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowColumnMovingProperty = DependencyProperty.Register("AllowColumnMoving", typeof(bool), typeof(ColumnChooserSettings), new PropertyMetadata(true, new PropertyChangedCallback(AllowColumnMovingChanged)));

        /// <summary>
        /// Gets/Sets whether <see cref="Column"/>s can be moved around within the <see cref="Infragistics.Controls.Grids.Primitives.ColumnChooserDialog"/>
        /// </summary>
        public bool AllowColumnMoving
        {
            get { return (bool)this.GetValue(AllowColumnMovingProperty); }
            set { this.SetValue(AllowColumnMovingProperty, value); }
        }

        private static void AllowColumnMovingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserSettings settings = (ColumnChooserSettings)obj;
            settings.OnPropertyChanged("AllowColumnMoving");
        }

        #endregion // AllowColumnMoving 
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