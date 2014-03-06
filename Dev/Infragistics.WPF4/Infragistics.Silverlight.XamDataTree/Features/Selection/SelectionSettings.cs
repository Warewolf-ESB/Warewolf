using System;
using System.ComponentModel;
using System.Windows;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A class which defines the Selection settings which will be applied across all <see cref="NodeLayout"/>s of a <see cref="XamDataTree"/>.
    /// </summary>
    public class TreeSelectionSettings : SettingsBase, IDisposable
    {
        #region Members

        SelectedNodesCollection _selectedNodes;

        #endregion // Members

        #region Properties

        #region NodeSelection

        /// <summary>
        /// Identifies the <see cref="NodeSelection"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NodeSelectionProperty = DependencyProperty.Register("NodeSelection", typeof(TreeSelectionType), typeof(TreeSelectionSettings), new PropertyMetadata(TreeSelectionType.Single, new PropertyChangedCallback(NodeSelectionChanged)));

        /// <summary>
        /// Gets / sets the <see cref="TreeSelectionType"/> which will be applied to all <see cref="NodeLayout"/>s.
        /// </summary>
        public TreeSelectionType NodeSelection
        {
            get { return (TreeSelectionType)this.GetValue(NodeSelectionProperty); }
            set { this.SetValue(NodeSelectionProperty, value); }
        }

        private static void NodeSelectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeSelectionSettings settings = (TreeSelectionSettings)obj;
            settings.OnPropertyChanged("NodeSelection");

            if (settings.Tree != null && settings.NodeSelection == TreeSelectionType.None)
                settings.Tree.SelectionSettings.SelectedNodes.Clear();
        }

        #endregion // NodeSelection

        #region SelectedNodes

        /// <summary>
        /// A collection of <see cref="XamDataTreeNode"/> objects which are currently selected.
        /// </summary>
        [Browsable(false)]
        public SelectedNodesCollection SelectedNodes
        {
            get
            {
                if (this._selectedNodes == null)
                {
                    this._selectedNodes = new SelectedNodesCollection();
                    this._selectedNodes.Tree = this.Tree;
                }
                return this._selectedNodes;
            }
        }

        #endregion // SelectedNodes

        #endregion // Properties

        #region IDisposable Members

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TreeSelectionSettings"/> and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param propertyName="disposing">
        /// true to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._selectedNodes != null)
                this._selectedNodes.Dispose();
        }

        /// <summary>
        /// Releases the unmanaged and managed resources used by the <see cref="TreeSelectionSettings"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
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