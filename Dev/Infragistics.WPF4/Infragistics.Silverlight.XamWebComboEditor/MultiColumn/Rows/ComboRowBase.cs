using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Editors.Primitives;
using System.Windows;
using System.Windows.Input;

namespace Infragistics.Controls.Editors
{
	/// <summary>
	/// The base class for an object that represents an item in the ItemsSource of the <see cref="XamMultiColumnComboEditor"/>
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public abstract class ComboRowBase : ComboEditorItemBase<ComboCellsPanel>
	{
	    #region Members

        ComboCellsCollection _cells;
		double _actualHeight;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboRowBase"/> class.
        /// </summary>
        /// <param name="data">The business object.</param>
		/// <param name="xamWebComboEditor">A reference to the <see cref="XamMultiColumnComboEditor"/>.</param>
        internal ComboRowBase(object data, XamMultiColumnComboEditor xamWebComboEditor)
            : base(data)
        {
            this.ComboEditor = xamWebComboEditor;
        }

        #endregion //Constructor

        #region Properties

        #region Public

		#region ActualHeight
		/// <summary>
		/// Gets the Actual height of the row. 
		/// </summary>
		/// <remarks>Note: this value is only available when the row is rendered.</remarks>
		public double ActualHeight
		{
			get
			{
				return this._actualHeight;
			}
			internal set
			{
				if (this._actualHeight != value)
				{
					this._actualHeight = value;
					this.OnPropertyChanged("ActualHeight");
				}
			}
		}
		#endregion // ActualHeight

        #region Cells

        /// <summary>
        /// Gets the <see cref="ComboCellsCollection"/> that belongs to the <see cref="ComboRow"/>.
        /// </summary>
        public virtual ComboCellsCollection Cells
        {
            get
            {
                if (this._cells == null)
                    this._cells = new ComboCellsCollection(this.ComboEditor.Columns, this);
                return this._cells;
            }
        }

        #endregion // Cells

        #region ComboEditor

        /// <summary>
        /// Gets the <see cref="XamMultiColumnComboEditor"/> that holds this <see cref="ComboRowBase"/>
        /// </summary>
        public XamMultiColumnComboEditor ComboEditor
        {
            get;
            private set;
        }

        #endregion //ComboEditor

        #region CellStyle

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="ComboCellControlBase"/> objects on this <see cref="ComboRowBase"/>.
        /// </summary>
        public Style CellStyle
        {
            get;
            set;
        }

        #endregion // CellStyle

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="ComboRowBase"/>
		/// </summary>
		public abstract RowType RowType
		{
			get;
		}

		#endregion // RowType

        #region IsMouseOver

        /// <summary>
        /// Gets whether the mouse is currently over the row.
        /// </summary>
        public bool IsMouseOver
        {
            get;
            protected internal set;
        }

        #endregion // IsMouseOver

        #endregion // Public

        #region Protected

        #region CanScrollHorizontally
        /// <summary>
        /// Gets whether or not a row will ever need to scroll horizontally. 
        /// </summary>
        protected internal virtual bool CanScrollHorizontally
        {
            get { return true; }
        }

        #endregion // CanScrollHorizontally

        #endregion // Protected

        #endregion // Properties

        #region Methods

        #region Protected

        #region ResolveCell

        /// <summary>
        /// Returns the <see cref="ComboCellBase"/> for the specified <see cref="ComboColumn"/>
        /// </summary>
        /// <param propertyName="column">The Column in which to resolve the cell.</param>
        /// <returns>The cell at the given column location.</returns>
        protected internal virtual ComboCellBase ResolveCell(ComboColumn column)
        {
            return this.Cells[column];
        }
        #endregion // ResolveCell

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region CreateInstanceOfRecyclingElement

        /// <summary>
        /// Creates a new instance of the <see cref="ComboCellsPanel"/> that represents the object.
        /// </summary>
        /// <returns></returns>
        protected override ComboCellsPanel CreateInstanceOfRecyclingElement()
        {
            return new ComboCellsPanel();
        }
        #endregion // CreateInstanceOfRecyclingElement

        #region OnElementAttached

        /// <summary>
        /// Called when the <see cref="ComboCellsPanel"/> is attached to the <see cref="ComboRowBase"/>
        /// </summary>
        /// <param name="element">A <see cref="ComboCellsPanel"/></param>
        protected override void OnElementAttached(ComboCellsPanel element)
        {
            base.OnElementAttached(element);
            element.OnAttached(this);
        }

        #endregion // OnElementAttached

        #region OnElementReleased

        /// <summary>
        /// Called when the <see cref="ComboCellsPanel"/> is removed from the <see cref="ComboRowBase"/>
        /// </summary>
        /// <param name="element">A <see cref="ComboCellsPanel"/></param>
        protected override void OnElementReleased(ComboCellsPanel element)
        {
            base.OnElementReleased(element);
            element.OnReleased(this);

			// JM 11-17-11 TFS96079
			this.IsMouseOver = false;
        }

        #endregion // OnElementReleased

        #region MeasureRaised

        /// <summary>
        /// Gets/Sets whether the measure method was raised when Measure was called. 
        /// </summary>
        protected internal override bool MeasureRaised
        {
            get
            {
                if (this.Control != null)
                    return this.Control.MeasureRaised;

                return true;
            }
            set
            {
                if (this.Control != null)
                    this.Control.MeasureRaised = value;
            }
        }

        #endregion // MeasureRaised

		#region OnSelectionChanged

		/// <summary>
		/// Invoked when the IsSelected proprety changes.
		/// </summary>
		protected override void OnSelectionChanged()
		{
			if (this.IsSelected)
			{
				bool ctrlKeyPressed			= ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) || this.ComboEditor.CheckBoxVisibility == Visibility.Visible;
				bool clearExistingSelection	= (false == this.ComboEditor.AllowMultipleSelection) || ctrlKeyPressed;

				this.ComboEditor.SelectItem(this.Data, clearExistingSelection);
			}
			else
				this.ComboEditor.UnselectItem(this.Data);
		}

		#endregion // OnSelectionChanged

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