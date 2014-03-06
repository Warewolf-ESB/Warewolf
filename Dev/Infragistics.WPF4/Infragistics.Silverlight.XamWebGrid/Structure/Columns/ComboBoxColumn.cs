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
using Infragistics.Controls.Grids.Primitives;
using System.Collections;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A <see cref="Column"/> that generates a <see cref="ComboBox"/> as the content for a <see cref="Cell"/>.
    /// </summary>
    public class ComboBoxColumn : CustomDisplayEditableColumn
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ComboBoxColumn"/> class.
        /// </summary>
        public ComboBoxColumn()
        {
            this.AddNewRowItemTemplateVerticalContentAlignment = this.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.AddNewRowItemTemplateHorizontalContentAlignment = this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }
        #endregion // Constructor

        #region Overrides

        #region GenerateContentProvider
        /// <summary>
        /// Generates a new <see cref="ComboBoxColumnContentProvider"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override ColumnContentProviderBase GenerateContentProvider()
        {
            return new ComboBoxColumnContentProvider();
        }
        #endregion // GenerateContentProvider

        #region RequiresFullRedrawOnEditorStyleUpdate

        /// <summary>
        /// Gets / sets if whether the <see cref="Infragistics.Controls.Grids.XamGrid.ResetPanelRows(bool)"/> should be called to do a full redraw when the <see cref=" Infragistics.Controls.Grids.ComboBoxColumn"/>.EditorStyle is changed.
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
        /// <remarks>
        /// See the remarks of <see cref="Column.AllowCellEditorValueChangedFiltering"/> for more information.
        /// </remarks>
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
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ComboBoxColumn), new PropertyMetadata(new PropertyChangedCallback(ItemsSourceChanged)));

        /// <summary>
        /// Gets/sets the <see cref="IEnumerable"/> for the <see cref="ComboBoxColumn"/>.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboBoxColumn ctrl = (ComboBoxColumn)obj;
            ctrl.OnPropertyChanged("ItemsSource");
        }

        #endregion // ItemsSource

        #region DisplayMemberPath

        /// <summary>
        /// Identifies the <see cref="DisplayMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(ComboBoxColumn), new PropertyMetadata(null, new PropertyChangedCallback(DisplayMemberPathChanged)));

        /// <summary>
        /// Gets / sets the value which will be set on the <see cref="System.Windows.Controls.ComboBox"/>.DisplayMemberPath.
        /// </summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        private static void DisplayMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboBoxColumn ctrl = (ComboBoxColumn)obj;
            ctrl.OnPropertyChanged("DisplayMemberPath");
            ctrl.RedrawGrid();
        }

        #endregion // DisplayMemberPath

        #region SelectedValuePath

        /// <summary>
        /// Identifies the <see cref="SelectedValuePath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(ComboBoxColumn), new PropertyMetadata(null, new PropertyChangedCallback(SelectedValuePathChanged)));

        /// <summary>
        /// Gets / sets the value which will be set on the <see cref="System.Windows.Controls.ComboBox"/>.SelectedValuePath.
        /// </summary>
        public string SelectedValuePath
        {
            get { return (string)this.GetValue(SelectedValuePathProperty); }
            set { this.SetValue(SelectedValuePathProperty, value); }
        }

        private static void SelectedValuePathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboBoxColumn ctrl = (ComboBoxColumn)obj;
            ctrl.OnPropertyChanged("SelectedValuePath");
            ctrl.RedrawGrid();
        }

        #endregion // SelectedValuePath

        #region ItemTemplate

        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ComboBoxColumn), new PropertyMetadata(new PropertyChangedCallback(ItemTemplateChanged)));

        /// <summary>
        /// Gets / sets the value which will be set on the <see cref="System.Windows.Controls.ComboBox"/>.ItemTemplate.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }

        private static void ItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboBoxColumn ctrl = (ComboBoxColumn)obj;
            ctrl.OnPropertyChanged("ItemTemplate");
            ctrl.RedrawGrid();
        }

        #endregion // ItemTemplate

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