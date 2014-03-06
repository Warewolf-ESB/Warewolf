using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Infragistics.Controls.Editors
{
	/// <summary>
	/// The base class for all cell objects in the <see cref="XamMultiColumnComboEditor"/>.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public abstract class ComboCellBase :  RecyclingContainer<ComboCellControlBase>
	{
        #region Members

        Style _style;

        #endregion // Members

        #region Constructor

        /// <summary>
		/// Initializes a new instance of the <see cref="ComboCellBase"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="ComboRowBase"/> object that owns the <see cref="ComboCellBase"/></param>
		/// <param propertyName="column">The <see cref="ComboColumn"/> object that the <see cref="ComboCellBase"/> represents.</param>
        public ComboCellBase(ComboRowBase row, ComboColumn column)
		{
			this.Row = row;
			this.Column = column;
		}

		#endregion // Constructor

        #region Properties

        #region Public

        #region Row
        /// <summary>
        /// The <see cref="ComboRowBase"/> that owns the <see cref="ComboCellBase"/>
        /// </summary>
        public ComboRowBase Row
        {
            get;
            internal set;
        }
        #endregion // Row

        #region Column

        /// <summary>
        /// The <see cref="ComboColumn"/> that the <see cref="ComboCellBase"/> represents.
        /// </summary>
        public ComboColumn Column
        {
            get;
            private set;
        }
        #endregion // Column

        #region Control

        /// <summary>
        /// Gets the <see cref="ComboCellControlBase"/> that is attached to the <see cref="ComboCellBase"/>
        /// </summary>
        /// <remarks>A Control is only assoicated with a Cell when it's in the viewport of the <see cref="Infragistics.Controls.Editors.Primitives.MultiColumnComboItemsPanel"/></remarks>
        public ComboCellControlBase Control
        {
            get;
            protected internal set;
        }
        #endregion // Control

        #region Style

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="ComboCellControlBase"/> objects.
        /// </summary>
        public Style Style
        {
            get
            {
                return this._style;
            }
            set
            {
                if (this._style != value)
                {
                    this._style = value;

                    this.ApplyStyle();
                }
            }
        }

        #endregion // Style

        #endregion // Public

        #region Protected

        #region ResolveStyle

        /// <summary>
        /// Gets the Style that should be applied to the <see cref="ComboCellControlBase"/> when it's attached.
        /// </summary>
        protected virtual Style ResolveStyle
        {
            get
            {
                return this.Style;
            }
        }

        #endregion // ResolveStyle

        #region BindingMode

        /// <summary>
        /// Gets the <see cref="BindingMode"/> that will be applied when binding a <see cref="ComboCellBase"/> to data.
        /// </summary>
        protected internal virtual BindingMode BindingMode
        {
            get
            {
                return BindingMode.OneWay;
            }
        }

        #endregion // BindingMode

        #endregion // Protected

        #region Internal

        /// <summary>
        /// Used for storing the size of a cell, if it needs to be re-measured.
        /// </summary>
        internal Size MeasuringSize
        {
            get;
            set;
        }

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Protected

        #region ApplyStyle
        /// <summary>
        /// Applies the resolved style of a Cell to it's <see cref="ComboCellControlBase"/>
        /// </summary>
        protected internal void ApplyStyle()
        {
            if (this.Control != null)
            {
                Style s = this.ResolveStyle;
                if (this.Control.Style != s)
                {
                    this.EnsureCurrentState();

                    if (s != null)
                        this.Control.Style = s;
                    else
                        this.Control.ClearValue(ComboCellControlBase.StyleProperty);
                }
            }
        }
        #endregion // ApplyStyle

		#region CreateCellBindingConverter

		/// <summary>
		/// Creates the <see cref="IValueConverter"/> which will be attached to this <see cref="ComboCellBase"/>.
		/// </summary>
		/// <returns></returns>
		protected internal virtual IValueConverter CreateCellBindingConverter()
		{
			return null;
		}

		#endregion // CreateCellBindingConverte

        #region EnsureCurrentState

        /// <summary>
        /// Ensures that <see cref="ComboCellBase"/> is in the correct state.
        /// </summary>
        protected internal virtual void EnsureCurrentState()
        {
			if (this.Control != null)
			{
                this.Control.EnsureVisualStates();
			}
		}

        #endregion // EnsureCurrentState

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region OnElementAttached
        /// <summary>
        /// Called when the <see cref="ComboCellControlBase"/> is attached to the <see cref="ComboCellBase"/>
        /// </summary>
        /// <param propertyName="element">A <see cref="ComboCellControlBase"/></param>
        protected override void OnElementAttached(ComboCellControlBase element)
        {
            this.Control = element;
            element.OnAttached(this);
        }
        #endregion // OnElementAttached

        #region OnElementReleased
        /// <summary>
        /// Called when the <see cref="ComboCellControlBase"/> is removed from the <see cref="ComboCellBase"/>
        /// </summary>
        /// <param propertyName="element">A <see cref="ComboCellControlBase"/></param>
        protected override void OnElementReleased(ComboCellControlBase element)
        {
            this.Control = null;
            element.OnReleased(this);
        }
        #endregion // OnElementReleased

        #region RecyclingElementType

        /// <summary>
        /// Gets the Type of control that should be created for the <see cref="ComboCellBase"/>.
        /// </summary>
        protected override Type RecyclingElementType
        {
            get
            {
                return null;
            }
        }
        #endregion // RecyclingElementType

        #region RecyclingIdentifier

        /// <summary>
        /// If a <see cref="RecyclingElementType"/> isn't specified, this property can be used to offer another way of identifying 
        /// a reyclable element.
        /// </summary>
        protected override string RecyclingIdentifier
        {
            get
            {
                return this.Row.ToString() + "_" + this.Column.Key;
            }
        }
        #endregion // RecyclingIdentifier

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