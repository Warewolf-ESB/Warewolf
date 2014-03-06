using System.Windows;
using Infragistics.Controls.Grids.Primitives;
using System.Collections;
using Infragistics.Controls.Editors;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A <see cref="Column"/> that generates a <see cref="XamMultiColumnComboEditor"/> as the content for a <see cref="Cell"/>.
    /// </summary>
    public class MultiColumnComboColumn : CustomDisplayEditableColumn
    {
        #region Memebers

        ObservableCollection<ComboColumn> _columns;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiColumnComboColumn"/> class.
        /// </summary>
        public MultiColumnComboColumn()
        {
            this.AddNewRowItemTemplateVerticalContentAlignment = this.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.AddNewRowItemTemplateHorizontalContentAlignment = this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }
        #endregion // Constructor

        #region Overrides

        #region GenerateContentProvider
        /// <summary>
        /// Generates a new <see cref="MultiColumnComboColumnContentProvider"/> that will be used to generate content for <see cref="Cell"/> objects for this <see cref="Column"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override ColumnContentProviderBase GenerateContentProvider()
        {
            return new MultiColumnComboColumnContentProvider();
        }

        #endregion // GenerateContentProvider

        #region RequiresFullRedrawOnEditorStyleUpdate

        /// <summary>
        /// Gets / sets if whether the <see cref="Infragistics.Controls.Grids.XamGrid.ResetPanelRows(bool)"/> should be called to do a full redraw when the <see cref=" Infragistics.Controls.Grids.MultiColumnComboColumn"/>.EditorStyle is changed.
        /// </summary>
        protected internal override bool RequiresFullRedrawOnEditorStyleUpdate
        {
            get
            {
                return true;
            }
        }

        #endregion // RequiresFullRedrawOnEditorStyleUpdate

        #region AllowCellEditorValueChangedFiltering

        /// <summary>
        /// Gets a value indicating whether filtering will be immediately applied after the value of the cell editor is changed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a filter should be applied immediately after the value of the cell editor is changed; otherwise, <c>false</c>.
        /// </value>
        protected internal override bool AllowCellEditorValueChangedFiltering
        {
            get
            {
                return true;
            }
        }

        #endregion // AllowCellEditorValueChangedFiltering

        #endregion // Overrides

        #region Properties

        #region ItemsSource

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiColumnComboColumn), new PropertyMetadata(new PropertyChangedCallback(ItemsSourceChanged)));

        /// <summary>
        /// This is the items to display for the combo box contents of each cell. 
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MultiColumnComboColumn ctrl = (MultiColumnComboColumn)obj;
            ctrl.OnPropertyChanged("ItemsSource");
            ctrl.RedrawGrid();
        }

        #endregion // ItemsSource

        #region DisplayMemberPath

        /// <summary>
        /// Identifies the <see cref="DisplayMemberPath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(MultiColumnComboColumn), new PropertyMetadata(new PropertyChangedCallback(DisplayMemberPathChanged)));

        /// <summary>
        /// This is the key from the ItemSource that will be used as a display value for the combo box.
        /// </summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        private static void DisplayMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MultiColumnComboColumn ctrl = (MultiColumnComboColumn)obj;
            ctrl.OnPropertyChanged("DisplayMemberPath");
            ctrl.RedrawGrid();
        }

        #endregion // DisplayMemberPath

        #region AutoGenerateColumns

        /// <summary>
        /// Identifies the <see cref="AutoGenerateColumns"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoGenerateColumnsProperty = DependencyProperty.Register("AutoGenerateColumns", typeof(bool), typeof(MultiColumnComboColumn), new PropertyMetadata(new PropertyChangedCallback(AutoGenerateColumnsChanged)));

        /// <summary>
        /// If true will cause the Combo Box to auto-generate the columns based on the ItemSource Property.  Otherwise, will use the columns in the Columns Property.
        /// </summary>
        public bool AutoGenerateColumns
        {
            get { return (bool)this.GetValue(AutoGenerateColumnsProperty); }
            set { this.SetValue(AutoGenerateColumnsProperty, value); }
        }

        private static void AutoGenerateColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MultiColumnComboColumn ctrl = (MultiColumnComboColumn)obj;
            ctrl.OnPropertyChanged("AutoGenerateColumns");
            ctrl.RedrawGrid();
        }

        #endregion // DisplayMemberPath

        #region SelectedValuePath

        /// <summary>
        /// Identifies the <see cref="SelectedValuePath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(MultiColumnComboColumn), new PropertyMetadata(new PropertyChangedCallback(SelectedValuePathChanged)));

        /// <summary>
        /// This is the key from the ItemSource that will be used as a value value for the selected item of the combo box.
        /// </summary>
        public string SelectedValuePath
        {
            get { return (string)this.GetValue(SelectedValuePathProperty); }
            set { this.SetValue(SelectedValuePathProperty, value); }
        }

        private static void SelectedValuePathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MultiColumnComboColumn ctrl = (MultiColumnComboColumn)obj;
            ctrl.OnPropertyChanged("SelectedValuePath");
            ctrl.RedrawGrid();
        }

        #endregion // DisplayMemberPath

        #region Columns

        /// <summary>
        /// Allows the user the ability to specify which columns they want to display in the combo box, these are ignored if <see cref="AutoGenerateColumns"/> dependency property is set to true.
        /// </summary>
        public ObservableCollection<ComboColumn> Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new ObservableCollection<ComboColumn>();
                    _columns.CollectionChanged += this.Columns_CollectionChanged;
                }

                return _columns;
            }
        }

        #endregion

        #endregion // Properties

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

        #region EventHandlers

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RedrawGrid();
        }

        #endregion // EventHandlers
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