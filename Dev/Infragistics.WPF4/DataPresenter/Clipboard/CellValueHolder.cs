using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// A class that contains information representing a cell value.
    /// </summary>
	/// <seealso cref="CellValueHolderCollection"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.ClipboardOperationEventArgs.Values"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public class CellValueHolder
    {
        #region Member Variables

        private object _value;
        private bool _isDisplayText;
		private bool _ignore;

        #endregion //Member Variables

        #region Constructor
		internal CellValueHolder(CellValueHolder source)
		{
			_value = source._value;
			_isDisplayText = source._isDisplayText;
			_ignore = source._ignore;
		}

        /// <summary>
        /// Initializes a new <see cref="CellValueHolder"/>.
        /// </summary>
        /// <param name="value">The cell value.</param>
        /// <param name="isDisplayText">Specifies whether the cell value is display text or the raw 
        /// cell value. See <see cref="Value"/> property for more info.</param>
        public CellValueHolder(object value, bool isDisplayText)
        {
            this._value = value;
            this._isDisplayText = isDisplayText;
        }
        #endregion //Constructor

        #region Properties

        #region Public Properties

        #region IsDisplayText
        /// <summary>
        /// Specifies whether the value being set is display text.
        /// </summary>
        /// <seealso cref="Value"/>
        public bool IsDisplayText
        {
            get
            {
                return this._isDisplayText;
            }
            set
            {
                this._isDisplayText = value;
            }
        }
        #endregion //IsDisplayText

        #region Value
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <remarks>
        /// <p class="body">This value is treated as either the cell value or display text depending on 
        /// <see cref="IsDisplayText"/> property value.</p>
        /// </remarks>
        /// <seealso cref="IsDisplayText"/>
        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }
        #endregion //Value

        #endregion //Public Properties

		#region Internal Properties

		#region Ignore
		internal bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
		#endregion //Ignore

		#endregion //Internal Properties

        #endregion //Properties

        #region Methods

		#region Clone
		internal virtual CellValueHolder Clone()
		{
			return new CellValueHolder(this);
		} 
		#endregion //Clone

        internal void Initialize(object value, bool isDisplayText)
        {
            this._value = value;
            this._isDisplayText = isDisplayText;
        }
        #endregion //Methods
	}

	/// <summary>
	/// Custom <see cref="CellValueHolder"/> used by the <see cref="ClipboardData"/> class that maintains 
	/// the original value and display text.
	/// </summary>
	internal class ClipboardCellValueHolder : CellValueHolder
	{
		#region Member Variables

		private object _originalValue;
		private string _originalDisplayText; 
		
		#endregion //Member Variables

		#region Constructor
		internal ClipboardCellValueHolder(ClipboardCellValueHolder source)
			: base(source)
		{
			_originalDisplayText = source._originalDisplayText;
			_originalValue = source._originalValue;
		}

		internal ClipboardCellValueHolder(object value, bool isDisplayText, object originalValue, string originalDisplayText)
			: base(value, isDisplayText)
		{
			_originalValue = originalValue;
			_originalDisplayText = originalDisplayText;
		} 
		#endregion //Constructor

		#region Properties
		internal object OriginalValue
		{
			get { return _originalValue; }
		}

		internal string OriginalDisplayText
		{
			get { return _originalDisplayText; }
		} 
		#endregion //Properties

		#region Base class overrides

		#region Clone
		internal override CellValueHolder Clone()
		{
			return new ClipboardCellValueHolder(this);
		}
		#endregion //Clone

		#endregion //Base class overrides
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