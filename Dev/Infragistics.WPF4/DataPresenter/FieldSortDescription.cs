using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
//using Infragistics.Windows.Input;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// Specifies how a field is sorted and optionally grouped
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
	/// <seealso cref="Field"/>
	/// <seealso cref="FieldLayout"/>
	/// <seealso cref="FieldLayout.SortedFields"/>
	/// <seealso cref="FieldSortDescriptionCollection"/>
	public class FieldSortDescription
    {
        #region Private Members

        private string              _fieldName;
        private Field               _field;
        private ListSortDirection   _direction;
        private bool                _isGroupBy;
        private bool                _isSealed;
        private int                 _preSortIndex;

        #endregion //Private Members

        #region Constructors

        /// <summary>
		/// Initializes a new instance of the <see cref="FieldSortDescription"/> class
        /// </summary>
        public FieldSortDescription()
        {
        }

		
		
		/// <summary>
		/// Initializes a new instance of the <see cref="FieldSortDescription"/> class
		/// </summary>
		/// <param name="fieldName">Field that's to be sorted.</param>
		/// <param name="direction">Sort direction.</param>
		/// <param name="isGroupBy">Whether to group records by this field</param>
		public FieldSortDescription( string fieldName, ListSortDirection direction, bool isGroupBy )
		{
			_fieldName = fieldName;
			_direction = direction;
			_isGroupBy = isGroupBy;
		}

        #endregion //Constructors
    
        #region Properties

            #region Public Properties

                #region Direction

        /// <summary>
        /// The direction of the sort
        /// </summary>
        public ListSortDirection Direction 
        { 
            get 
            { 
                return this._direction; 
            }
            set
            {
                this.VerifyNotSealed();
                this._direction = value;
            }
        }

                #endregion //Direction

                #region Field

        /// <summary>
        /// The Field  to sort
        /// </summary>
        public Field Field 
        { 
            get 
            { 
                return this._field; 
            } 
            set
            {
                this.VerifyNotSealed();
                this._field = value;
            }
        }

                #endregion //Field	

                #region FieldName

        /// <summary>
        /// The name of the Field to sort
        /// </summary>
        public string FieldName 
        { 
            get 
            {
                if (this._field != null)
                    return this.Field.Name;

                return this._fieldName; 
            } 
            set
            {
                this.VerifyNotSealed();
                this._fieldName = value;
            }
        }

                #endregion //FieldName	

                #region IsGroupBy

        /// <summary>
        /// Determines whether this is also a groupby field
        /// </summary>
        public bool IsGroupBy 
        { 
            get 
            { 
                return this._isGroupBy; 
            }
            set
            {
                this.VerifyNotSealed();
                this._isGroupBy = value;
            }
        }

                #endregion //IsGroupBy

                #region IsSealed

        /// <summary>
        /// Returns true if the Seal method has been called (read-only)
        /// </summary>
        public bool IsSealed 
        { 
            get 
            { 
                return this._isSealed; 
            }
        }

                #endregion //IsSealed
    
            #endregion // Public Properties

            #region Internal Properties

                #region PreSortIndex

        internal int PreSortIndex
        {
            get { return this._preSortIndex; }
            set { this._preSortIndex = value; }
        }

                #endregion //PreSortIndex	
    
            #endregion //Internal Properties

        #endregion //Properties

        #region Methods

            #region Internal Methods

				#region AreEqual

		// SSP 8/25/10 TFS30982
		// 
		internal static bool AreEqual( FieldSortDescription x, FieldSortDescription y )
		{
			return x._direction == y._direction
				&& x._field == y._field
				&& x._fieldName == y._fieldName
				&& x._isGroupBy == y._isGroupBy;
		} 

				#endregion // AreEqual

				// AS 6/1/09 NA 2009.2 Undo/Redo
				#region Clone
		internal FieldSortDescription Clone()
		{
			FieldSortDescription fsd = (FieldSortDescription)this.MemberwiseClone();
			fsd._isSealed = false;
			fsd._preSortIndex = 0;
			return fsd;
		} 
				#endregion //Clone

				// AS 6/1/09 NA 2009.2 Undo/Redo
				#region GetToggleDirection
		internal ListSortDirection GetToggleDirection()
		{
			return _direction == ListSortDirection.Ascending
				? ListSortDirection.Descending
				: ListSortDirection.Ascending;
		} 
				#endregion //GetToggleDirection
	
                #region ToggleDirection

        internal void ToggleDirection()
        {
            if (this._direction == ListSortDirection.Ascending)
            {
                this._direction = ListSortDirection.Descending;
                this._field.SetSortingStatus(SortStatus.Descending, this._isGroupBy);
            }
            else
            {
                this._direction = ListSortDirection.Ascending;
                this._field.SetSortingStatus(SortStatus.Ascending, this._isGroupBy);
            }
        }

                #endregion //ToggleDirection	
    
                #region Seal

        internal void Seal()
        {
            this._isSealed = true;
        }

                #endregion //Seal	
    
            #endregion //Internal Methods	
    
            #region Private Methods

                #region VerifyNotSealed

        private void VerifyNotSealed()
        {
            if (this._isSealed)
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_ArgumentException_22" ) );
        }

                #endregion //VerifyNotSealed	
    
            #endregion //Private Methods	
    
        #endregion //Methods
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