using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Windows.Controls
{
    #region SpecialFilterOperandBase Class

	/// <summary>
	/// Abstract base class for special filter operands.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SpecialFilterOperandBase</b> is the abstract base class for various special filter operands. Built-in special 
	/// operands are exposed by the <see cref="SpecialFilterOperands"/> class as static properties, like 
	/// <see cref="SpecialFilterOperands.Blanks"/>, <see cref="SpecialFilterOperands.Quarter1"/>, 
	/// <see cref="SpecialFilterOperands.AboveAverage"/>
	/// etc... which can be used to specify the operand value on <see cref="ComparisonCondition"/>.
	/// </para>
	/// <para class="body">
	/// You can define a custom operand by deriving a class from this class and registering an instance with the 
	/// <see cref="SpecialFilterOperands"/>' <see cref="SpecialFilterOperands.Register"/> method. This will automatically 
	/// integrate your custom operand with controls/features that are aware of the special filter operands (like the 
	/// data presenter's record filtering functionality). Also you can replace built-in special operands with your own
	/// custom operand by unregistering the built-in operand and registering your instance of the operand with the same
	/// name.
	/// </para>
	/// </remarks>
	/// <seealso cref="SpecialFilterOperands"/>
	/// <seealso cref="SpecialFilterOperands.Register"/>
	/// <seealso cref="SpecialFilterOperands.Unregister"/>
	/// <seealso cref="SpecialFilterOperands.Blanks"/>
	/// <seealso cref="SpecialFilterOperands.NonBlanks"/>
	/// <seealso cref="SpecialFilterOperands.AboveAverage"/>
    public abstract class SpecialFilterOperandBase
    {
        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="SpecialFilterOperandBase"/>.
        /// </summary>
        public SpecialFilterOperandBase( )
        {
        }

        #endregion // Constructor

        #region Base class overrides

		#region Equals

		// SSP 1/5/10 TFS25670
		// We need to override Equals on the operands so the combo editor used for drop-down
		// finds a matching entry even when the instance being searched is a different instance
		// from the one in the drop-down list. This can happen after deserialization for example.
		// 
		/// <summary>
		/// Overridden. Checks to see if the specified object is an operand and is equal to this operand.
		/// </summary>
		/// <param name="obj">Operand object to test for equality.</param>
		/// <returns>True if the specified object is equal to this object, false otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> If you need to provide your own implementation for Equals for a derived class, please
		/// use the <see cref="EqualsOverride"/> method.
		/// </para>
		/// </remarks>
		public override bool Equals( object obj )
		{
			SpecialFilterOperandBase operand = obj as SpecialFilterOperandBase;
			if ( null == operand )
				return false;

			if ( operand is SpecialFilterOperands.OperandWrapper 
				&& ! ( this is SpecialFilterOperands.OperandWrapper ) )
				return operand.EqualsOverride( this );

			return this.EqualsOverride( operand );
		}

		#endregion // Equals

		#region GetHashCode

		// SSP 1/5/10 TFS25670
		// We need to override Equals on the operands so the combo editor used for drop-down
		// finds a matching entry even when the instance being searched is a different instance
		// from the one in the drop-down list. This can happen after deserialization for example.
		// 
		/// <summary>
		/// Overridden. Returns the hash code.
		/// </summary>
		/// <returns>Hash code of the object.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
		/// and return an appropriate hash code.
		/// </para>
		/// </remarks>
		public override int GetHashCode( )
		{
			return 0;
		}

		#endregion // GetHashCode

            #region ToString

        /// <summary>
        /// Returns a string representation of the operand.
        /// </summary>
        public override string ToString()
        {
			// SSP 1/5/10 TFS25670
			// Use the DisplayContent first and then the Description. The combo editor in the 
			// filter cell will do a ToString on the value if there's no corresponding entry
			// in the drop-down. In which case we need to display the same text that would be
			// displayed if there was a matching entry. And because the entries in the 
			// drop-down make use of DisplayContent as the display text, we should use the 
			// same DisplayContent for the ToString as well so we show the same text whether
			// the operand has a corresponding entry in the drop-down or not. Also note that
			// the drop-down in the filter cell gets populated lazily when the drop-down 
			// button is clicked so the above situation is more likely then one would think.
			// 
			// ------------------------------------------------------------------------------

			// JJD 02/17/12 - TFS102170
			// Rolled back the fix for FormattedSRValue from 11.2
			// AS 5/17/11 NA 11.2 Excel Style Filtering
			// With the change for TFS29800, the DisplayContent of our special operands 
			// will return a FormattedSRValue and not a string so we need to try and 
			// use that if possible.
			//
			//string str = this.DisplayContent as string;
			object content = this.DisplayContent;
			string str = GetString(content);

			if (str == null)
			{
				str = this.Description as string;

				// JJD 02/17/12 - TFS102170
				// If Description wasn't supplied then call ToString() on the DisplayContent
				if (str == null && content != null)
					str = content.ToString();
			}
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------------

            return str != null ? str : base.ToString();
        }

            #endregion //ToString

        #endregion //Base class overrides	

        #region Properties

        #region Public Properties

        #region Description

		/// <summary>
		/// Description of the operand. By default this will be displayed in the tooltip of the operand.
		/// </summary>
        public abstract object Description { get; }

        #endregion // Description

        #region DisplayContent

		/// <summary>
		/// This value (can be text) that will be used in the UI to represent this operand.
		/// </summary>
        public abstract object DisplayContent { get; }

        #endregion // DisplayContent

        #region Name

		/// <summary>
		/// Name of the operand. Used to uniquely identify the operand.
		/// </summary>
        public abstract string Name { get; }

        #endregion // Name

		#region UsesAllValues

		/// <summary>
		/// Indicates whether this operand uses context’s AllValues property to evaluate the condition. 
		/// In effect, it indicates whether condition evaluation of a value depends on other values in 
		/// the set of values being evaluated.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>UsesAllValues</b> property indicates whether this operand uses context’s 
		/// <see cref="ConditionEvaluationContext.AllValues"/> property to evaluate the condition. 
		/// In effect, it indicates whether condition evaluation of a value depends on other values in 
		/// the set of values being evaluated. For example, in data presenter, ‘AboveAverage’ condition 
		/// would be a condition where to determine whether a value is above the average, all values of 
		/// the field need to be taken into account. This also indicates to the data presenter that when a 
		/// field’s value is changed in any record, all records need to reevaluate the condition in order 
		/// to reflect correct results based on the new average of the field values.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that for such operands that use <see cref="ConditionEvaluationContext.AllValues"/>
		/// to calculate an attribute of the set of values (for example, average), you can cache the 
		/// calculated value using the <see cref="ConditionEvaluationContext.UserCache"/> property. This
		/// property will return the value that it's set to for evaluation of the rest of the values in
		/// the set of values (for example, all field values in a record collection in data presenter).
		/// This way you can avoid re-calculating the same value again. Note that the caller (data presenter)
		/// will clear out the cached value once all the values are evaluated. Also note that when the
		/// data changes, the caller (data presenter) will clear the user cache and therefore you don't
		/// need to keep track of data changes.
		/// </para>
		/// </remarks>
		/// <seealso cref="ConditionEvaluationContext.AllValues"/>
		public abstract bool UsesAllValues { get; }

		#endregion // UsesAllValues

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Protected Methods

		#region EqualsOverride

		// SSP 1/5/10 TFS25670
		// We need to override Equals on the operands so the combo editor used for drop-down
		// finds a matching entry even when the instance being searched is a different instance
		// from the one in the drop-down list. This can happen after deserialization for example.
		// NOTE: The reason for not making this method abstract is for backward compatibility in
		// case someone had derived a class previously and now they would have a compile time
		// breakage. Since this is only being done for the above mentioned purposes, it's not
		// absolutely essential and therefore we'll devine this method as virtual instead of
		// abstract.
		// 
		/// <summary>
		/// Checks to see if the specified operand is equal to this operand. This method is called
		/// by the Equals method.
		/// </summary>
		/// <param name="operand">Operand to check for equality.</param>
		/// <returns>True if the specified operand is equal to this, false otherwise.</returns>
		protected virtual bool EqualsOverride( SpecialFilterOperandBase operand )
		{
			Debug.Assert( false, "Derived classes need to override this method." );
			return this == operand;
		}

		#endregion // EqualsOverride

		#endregion // Protected Methods

		#region Public Methods

		#region IsMatch

		/// <summary>
		/// Returns true if the specified value matches the condition. False otherwise.
		/// </summary>
		/// <param name="comparisonOperator">The operand is being evaluated against this comparison operator.</param>
		/// <param name="value">Value to test.</param>
		/// <param name="context">Context information associated with the the 'value' being tested for match.</param>
		/// <returns>True if the value passes the condition, false otherwise.</returns>
        public abstract bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context );

        #endregion // IsMatch

		#region SupportsOperator

		/// <summary>
		/// Indicates whether this operand supports specified comparison operator.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator to check for support.</param>
		/// <returns>Returns True if the specified operator is supported by this operand, False otherwise.</returns>
        public abstract bool SupportsOperator( ComparisonOperator comparisonOperator );

		#endregion // SupportsOperator

		#region SupportsDataType

		/// <summary>
		/// Indicates whether this operand supports data values of specified type.
		/// </summary>
		/// <param name="type">Type of data values to check for support.</param>
		/// <returns>Returns True if the data values of the specified type are supported, False otherwise.</returns>
		public abstract bool SupportsDataType( Type type );

        #endregion // SupportsDataType

        #endregion // Public Methods

		#region Private Methods

		// AS 5/17/11 NA 11.2 Excel Style Filtering
		#region GetString
		private static string GetString(object value)
		{
			string str;
			var formattedSR = value as SpecialFilterOperandFactory.IGOperand.FormattedSRValue;

			if (null != formattedSR)
				str = formattedSR.ToString();
			else
				str = value as string;

			return str;
		}
		#endregion //GetString

		#endregion //Private Methods

        #endregion // Methods
    }

    #endregion // SpecialFilterOperandBase Class
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