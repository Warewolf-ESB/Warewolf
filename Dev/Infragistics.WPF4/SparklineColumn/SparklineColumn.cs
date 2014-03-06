using System.Windows;
using Infragistics.Controls.Grids.Primitives;
using Infragistics.Controls.Charts;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A <see cref="Column"/> which can be used to show A Sparkline Chart.
    /// </summary>
    public class SparklineColumn: Column
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SparklineColumn"/> class.
        /// </summary>
        public SparklineColumn()
        {
            this.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }
        #endregion

        #region GenerateContentProvider
        /// <summary>
        /// Generates a new <see cref="SparklineColumnContentProvider"/> that will be used to generate content for <see cref="Cell"/> objects for this <see cref="Column"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override ColumnContentProviderBase GenerateContentProvider()
        {
            return new SparklineColumnContentProvider();
        }
        #endregion

        #region Overridden Public Properties

        #region IsSummable

        /// <summary>
        /// Gets / sets if the column will show the UI for SummaryRow.
        /// </summary>
        public override bool IsSummable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsSummable = value;
            }
        }

        #endregion

        #region IsFilterable

        /// <summary>
        /// Gets/sets if a column can be filtered via the UI.
        /// </summary>
        public override bool IsFilterable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsFilterable = value;
            }
        }

        #endregion

        #region IsSortable

        /// <summary>
        /// Gets/Sets if a Column can be sorted. 
        /// </summary>
        public override bool IsSortable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsSortable = value;
            }
        }

        #endregion

        #region IsGroupable

        /// <summary>
        /// Gets/Sets if a Column can be grouped. 
        /// </summary>
        public override bool IsGroupable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsGroupable = value;
            }
        }

        #endregion
        
        #region RequiresBoundDataKey
        /// <summary>
        /// Gets whether an exception should be thrown if the key associated with the <see cref="ColumnBase"/> doesn't 
        /// correspond with a property in the data that this object represents.
        /// </summary>
        protected internal override bool RequiresBoundDataKey
        {
            get
            {
                return false;
            }
        }
        #endregion // RequiresBoundDataKey

        #endregion
                  
        #region Public Properties

        #region ValueMemberPath

        /// <summary>
        /// Identifies the <see cref="ValueMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register("ValueMemberPath", typeof(string), typeof(SparklineColumn), new PropertyMetadata(null, new PropertyChangedCallback(ValueMemberPathChanged)));

        /// <summary>
        /// Gets / sets the string path to the value column that will be assigned to the <see cref="XamSparkline"/> controls of the <see cref="SparklineColumn"/>.
        /// </summary>
        public string ValueMemberPath
        {
            get { return this.GetValue(ValueMemberPathProperty).ToString(); }
            set { this.SetValue(ValueMemberPathProperty, value); }
        }

        private static void ValueMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SparklineColumn ctrl = (SparklineColumn)obj;
            ctrl.RedrawGrid();
            ctrl.OnPropertyChanged("ValueMemberPath");
        }

        #endregion // ValueMemberPath

        #region LabelMemberPath

        /// <summary>
        /// Identifies the <see cref="LabelMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty LabelMemberPathProperty = DependencyProperty.Register("LabelMemberPath", typeof(string), typeof(SparklineColumn), new PropertyMetadata(null, new PropertyChangedCallback(LabelMemberPathChanged)));

        /// <summary>
        /// Gets / sets the string path to the label column that will be assigned to the <see cref="XamSparkline"/> controls of the <see cref="SparklineColumn"/>.
        /// </summary>
        public string LabelMemberPath
        {
            get { return (string)this.GetValue(LabelMemberPathProperty); }
            set { this.SetValue(LabelMemberPathProperty, value); }
        }

        private static void LabelMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SparklineColumn ctrl = (SparklineColumn)obj;
            ctrl.RedrawGrid();
            ctrl.OnPropertyChanged("LabelMemberPath");
        }

        #endregion // LabelMemberPath

        #region Style

        /// <summary>
        /// Identifies the <see cref="Style"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(SparklineColumn), new PropertyMetadata(null, new PropertyChangedCallback(StyleChanged)));

        /// <summary>
        /// Gets / sets the Style that will be applied to the <see cref="XamSparkline"/> altering the appearance and behavior of the control in the <see cref="SparklineColumn"/>.
        /// </summary>
        public Style Style
        {
            get { return (Style)this.GetValue(StyleProperty); }
            set { this.SetValue(StyleProperty, value); }
        }

        private static void StyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SparklineColumn ctrl = (SparklineColumn)obj;
            ctrl.RedrawGrid();
            ctrl.OnPropertyChanged("Style");
        }

        #endregion // LabelMemberPath
        
        #region DisplayType

        /// <summary>
        /// Identifies the <see cref="DisplayType"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DisplayTypeProperty = DependencyProperty.Register("DisplayType", typeof(SparklineDisplayType), typeof(SparklineColumn), new PropertyMetadata(SparklineDisplayType.Line, new PropertyChangedCallback(DisplayTypeChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DisplayType"/> that will be assigned to the <see cref="XamSparkline"/> controls of the <see cref="SparklineColumn"/>.
        /// </summary>
        public SparklineDisplayType DisplayType
        {
            get { return (SparklineDisplayType)this.GetValue(DisplayTypeProperty); }
            set { this.SetValue(DisplayTypeProperty, value); }
        }

        private static void DisplayTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SparklineColumn ctrl = (SparklineColumn)obj;
            ctrl.RedrawGrid();
            ctrl.OnPropertyChanged("DisplayType");
        }

        #endregion // DisplayType

        #region ItemsSourcePath

        /// <summary>
        /// Identifies the <see cref="ItemsSourcePath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemsSourcePathProperty = DependencyProperty.Register("ItemsSourcePath", typeof(string), typeof(SparklineColumn), new PropertyMetadata(null, new PropertyChangedCallback(ItemsSourcePathChanged)));

        /// <summary>
        /// Gets / sets the <see cref="ItemsSourcePath"/> that contains the set of data to be displayed in the <see cref="XamSparkline"/> controls of the <see cref="SparklineColumn"/>.
        /// </summary>
        public string ItemsSourcePath
        {
            get { return (string)this.GetValue(ItemsSourcePathProperty); }
            set { this.SetValue(ItemsSourcePathProperty, value); }
        }

        private static void ItemsSourcePathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SparklineColumn ctrl = (SparklineColumn)obj;
            ctrl.RedrawGrid();
            ctrl.OnPropertyChanged("ItemsSourcePath");
        }

        #endregion // ItemsSourcePath

        #endregion

        #region Methods

        #region RedrawGrid

        private void RedrawGrid()
        {
            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.IsLoaded)
            {
                this.ColumnLayout.Grid.ResetPanelRows(true);
            }
        }

        #endregion // RedrawGrid

        #endregion // Methods
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