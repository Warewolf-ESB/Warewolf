using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace Infragistics.Windows.Controls
{
    #region ConditionEvaluationContext Class

    /// <summary>
    /// Provides information regarding the context in which a condition is being evaluated.
    /// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ConditionEvaluationContext</b> provides information regarding the context in 
	/// which a condition is being evaluated. This object is passed into the <see cref="ICondition"/>'s
	/// <see cref="ICondition.IsMatch"/> method along with the value that's being matched against the
	/// condition.
	/// </para>
	/// </remarks>
    public abstract class ConditionEvaluationContext
    {
        #region Member Vars

        private object _userCache;

		// SSP 7/21/11 TFS81583
		// 
		private Dictionary<object, object> _userCacheTable;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ConditionEvaluationContext"/>.
        /// </summary>
        public ConditionEvaluationContext()
        {
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

        #region AllValues

        /// <summary>
        /// Returns the set of values a member of which is currently being evaluated. This is used to
        /// evaluate conditions that require information regarding the associated set of values, for example
        /// a condition that matches values that are 'above average' require the average attribute of
        /// the set of values associated with the current value being evaluated. Such a condition would
        /// calculate the average of 'AllValues' and compare the value being evaluated with that average.
        /// </summary>
        /// <remarks>
        /// <para class="body">
		/// <b>AllValues</b> property returns the set of values a member of which is currently being evaluated. 
		/// This is used to evaluate conditions that require information regarding the associated set of values,
		/// for example a condition that matches values that are 'above average' require the average attribute 
		/// of the set of values associated with the current value being evaluated. Such a condition would
		/// calculate the average of 'AllValues' and compare the value being evaluated with that average.
        /// </para>
        /// <para class="body">
        /// For example, in XamDataGrid when 'AboveAverage' filter condition is selected on a field, <i>AllValues</i>
        /// will be all field values of the record collection associated with the current field value that's
        /// being evaluated. <b>Note</b> that once you calculate the particular attribute of the set of values
        /// (in our example the average), you can cache the value using the <see cref="SetUserCache"/> method
        /// which you can retrieve using <see cref="GetUserCache"/> for reuse in successive evalulation of other 
		/// values in the set. <i>GetUserData</i> will return null initially and once you cache it using the
		/// <see cref="SetUserCache"/>, it will continue returning that value for evaluation of all other values 
		/// in the set. When values in the set changes, the context owner (for example the data presenter) will
		/// make sure to clear the cache.
        /// </para>
		/// <para class="body">
		/// <b>Note</b> that accessing <b>AllValues</b> will cause the re-evaluation of filters on all items
		/// even if data of one item changes. For example, in XamDataGrid, with 'AboveAverage' as the condition,
		/// when a cell value in a record changes, the 'average' of all values is changed and therefore all
		/// records have to be re-evaluated to make sure that they are still above the new average.
		/// </para>
        /// </remarks>
		/// <seealso cref="UserCache"/>
		/// <seealso cref="ValueEntry"/>
        public abstract IEnumerable<ValueEntry> AllValues { get; }

        #endregion // AllValues

		#region CurrentValue

		/// <summary>
		/// Returns further information regarding the value that’s being evaluated currently.
		/// </summary>
		public abstract ValueEntry CurrentValue { get; }

		#endregion // CurrentValue

		#region Comparer

        // SSP 5/3/10 TFS25788
		// Added FilterValueComparer property on the FieldSettings in data presenter.
		// 
		/// <summary>
		/// Returns any custom comparer for comparing values when evaluating filter conditions.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Comparer</b> property returns any custom comparer that may have been specified, for example
		/// the data presenter's FieldSettings' FilterValueComparer property.
		/// </para>
		/// <para class="body">
		/// Default implementation returns null.
		/// </para>
		/// </remarks>
		public virtual IComparer Comparer
		{
			get
			{
				return null;
			}
		}

		#endregion // Comparer

		#region IgnoreCase

		/// <summary>
		/// Indicates whether string comparisons should be done case-insensitive or case-sensitive.
		/// </summary>
		public abstract bool IgnoreCase { get; }

		#endregion // IgnoreCase

		#region PreferredComparisonDataType

		/// <summary>
		/// Indicates the data type that should be used to coerce values before performing 
		/// quantitative comparisons.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>PreferredComparisonDataType</b> indicates the data type that should be used 
		/// to coerce values before performing comparison. This is only applicable to quantitative 
		/// comparison operators, like 
		/// GreaterThan, LessThan etc… and does not apply to string comparison operators like 
		/// StartsWith, Contains etc… This property will typically return the underlying data 
		/// type of the field. For example, in data presenter this will be the field’s 
		/// EditAsTypeResolved which defaults to the Field's DataType.
		/// </para>
		/// </remarks>
		public abstract Type PreferredComparisonDataType { get; }

		#endregion // PreferredComparisonDataType

		#region UserCache

		/// <summary>
        /// Used for caching any value.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// <b>UserCache</b> is used for caching any value that can be used during evaluations of
        /// further values in the set. See <see cref="AllValues"/> for more information.
        /// </para>
        /// <see cref="AllValues"/>
        /// </remarks>
		// SSP 7/21/11 TFS81583
		// Added GetUserCache and SetUserCache methods and marked UserCache property obsolete.
		// The reason for this is that there may be multiple operands or conditions that require
		// caching values within a record filter.
		// 
		[ Obsolete("UserCache is replaced by GetUserCache and SetUserCache methods.", false ) ]
        public object UserCache
        {
            get
            {
                return _userCache;
            }
            set
            {
                _userCache = value;
            }
        }

        #endregion // UserCache

        #endregion // Public Properties

		#region Internal Methods

		#region FilterEvaluator

		// SSP 2/29/12 TFS89053
		// 
		internal virtual IFilterEvaluator FilterEvaluator
		{
			get
			{
				return null;
			}
		}

		#endregion // FilterEvaluator 

		#endregion // Internal Methods

        #endregion // Properties

		#region Methods

		#region Public Methods

		#region GetUserCache

		// SSP 7/21/11 TFS81583
		// 
		/// <summary>
		/// Retrieves the value cached using the <see cref="SetUserCache"/> method.
		/// </summary>
		/// <param name="cacheIdentifier">Used to identify the cached value. You need to pass in the same
		/// identifier that was passed into <see cref="SetUserCache"/> when the value was cached. This can be
		/// any object, including the instance of the condition or the operand for which the value is
		/// being cached.
		/// </param>
		/// <returns></returns>
		/// <remarks>
		/// <para class="body">
		/// <b>GetUserCache</b> and <see cref="SetUserCache"/> methods are used used for caching any value 
		/// that can be used during evaluations of further values in the set. 
		/// See <see cref="AllValues"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="SetUserCache"/>
		/// <seealso cref="AllValues"/>
		public object	GetUserCache( object cacheIdentifier )
		{
			CoreUtilities.ValidateNotNull( cacheIdentifier );

			object val;
			if ( null != _userCacheTable && _userCacheTable.TryGetValue( cacheIdentifier, out val ) )
				return val;

			return null;
		}

		#endregion // GetUserCache

		#region SetUserCache

		// SSP 7/21/11 TFS81583
		// 
		/// <summary>
		/// Used for caching any value.
		/// </summary>
		/// <param name="cacheIdentifier">Used to identify the cached value. You need to pass in the same
		/// identifier to <see cref="GetUserCache"/> to retrieve the cached value. This can be
		/// any object, including the instance of the condition or the operand for which the value is
		/// being cached.
		/// </param>
		/// <param name="cacheValue">The value to cache.</param>
		/// <returns></returns>
		/// <remarks>
		/// <para class="body">
		/// <see cref="GetUserCache"/> and <b>SetUserCache</b> methods are used used for caching any value 
		/// that can be used during evaluations of further values in the set. 
		/// See <see cref="AllValues"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="SetUserCache"/>
		/// <seealso cref="AllValues"/>
		public void SetUserCache( object cacheIdentifier, object cacheValue )
		{
			CoreUtilities.ValidateNotNull( cacheIdentifier );

			if ( null == cacheValue )
			{
				if ( null != _userCacheTable )
					_userCacheTable.Remove( cacheIdentifier );
			}
			else
			{
				if ( null == _userCacheTable )
					_userCacheTable = new Dictionary<object, object>( );

				_userCacheTable[cacheIdentifier] = cacheValue;
			}
		}

		#endregion // SetUserCache

		#endregion // Public Methods 

		#endregion // Methods
    }

    #endregion // ConditionEvaluationContext Class

    #region ValueEntry Class

    /// <summary>
    /// Provides information regarding a value. Used by <see cref="ConditionEvaluationContext.AllValues"/> property.
    /// </summary>
    public abstract class ValueEntry
    {
        #region Vars

		private double? _cachedValueAsDouble;

        #endregion // Vars

        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ValueEntry"/>.
        /// </summary>
        public ValueEntry( )
        {
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

		#region Context

		/// <summary>
		/// Returns the context associated with the value. In the case of data presenter, the context will 
		/// be the Cell whose value this value entry represents.
		/// </summary>
		public abstract object Context { get; }

		#endregion // Context

        #region Culture

		/// <summary>
		/// Returns the culture info.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Culture</b> and <see cref="Format"/> properties indicate any culture and format
		/// associated with the object for which the condition is being evaulated. For example,
		/// in the data presenter, Culture and Format will be the culture and format settings
		/// associated with the cell's field.
		/// </para>
		/// <para class="body">
		/// Typically you would use the <i>Culture</i> and <i>Format</i> for converting values
		/// from one type to another when evaluating the condition.
		/// </para>
		/// </remarks>
		/// <see cref="Format"/>
		public abstract CultureInfo Culture { get; }

        #endregion // Culture

        #region Format

		/// <summary>
		/// Returns the format.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// See <see cref="Culture"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Culture"/>
		public abstract string Format { get; }
        
        #endregion // Format

        #region Value

        /// <summary>
        /// Returns the value.
        /// </summary>
		public abstract object Value { get; }

        #endregion // Value

        #region ValueAsDouble

        /// <summary>
        /// Returns the value as double. If the value is not double, it will try to convert
        /// the value into double and return the converted value. If the conversion fails,
        /// it returns double.NaN.
        /// </summary>
        public double ValueAsDouble
        {
            get
            {
				if ( _cachedValueAsDouble.HasValue )
					return _cachedValueAsDouble.Value;

                object value = this.Value;

				object r = Utilities.ConvertDataValue( value, typeof( double ), this.Culture, this.Format );
				if ( r is double )
				{
					double d = (double)r;
					_cachedValueAsDouble = d;
					return d;
				}
				else
				{
					_cachedValueAsDouble = double.NaN;
					return _cachedValueAsDouble.Value;
				}
            }
        }

        #endregion // ValueAsDouble

        #endregion // Public Properties

        #endregion // Properties
	}

	#endregion // ValueEntry Class
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