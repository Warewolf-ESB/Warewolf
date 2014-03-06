using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Infragistics.Windows.Internal;
using System.Collections;

namespace Infragistics.Windows.Controls
{
    #region ComplementCondition Class

    /// <summary>
    /// Condition that complements the results of a specified condition.
    /// </summary>
	/// <seealso cref="ComparisonCondition"/>
	/// <seealso cref="ConditionGroup"/>
	/// <seealso cref="ICondition"/>
	[ContentProperty( "SourceCondition" )]
	[DefaultProperty( "SourceCondition" )]
    public class ComplementCondition : ICondition
    {
		#region Nested Data Structures

		#region SerializationInfo Class

		/// <summary>
		/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
		/// </summary>
		internal class SerializationInfo : ObjectSerializationInfo
		{
			private PropertySerializationInfo[] _props;

			public override IEnumerable<PropertySerializationInfo> SerializedProperties
			{
				get
				{
					if ( null == _props )
					{
						_props = new PropertySerializationInfo[]
						{
							new PropertySerializationInfo( typeof( ICondition ), SerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY )
						};
					}

					return _props;
				}
			}

			public override Dictionary<string, object> Serialize( object obj )
			{
				ComplementCondition cc = (ComplementCondition)obj;
				Dictionary<string, object> values = new Dictionary<string, object>( );

				if ( null != cc.SourceCondition )
					values[SerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY] = new ICondition[] { cc.SourceCondition };

				return values;
			}

			public override object Deserialize( Dictionary<string, object> values )
			{
				object v;
				if ( values.TryGetValue( SerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY, out v ) )
				{
					foreach ( ICondition c in (IEnumerable)v )
						return new ComplementCondition( c );
				}

				return null;
			}
		}

		#endregion // SerializationInfo Class

		#endregion // Nested Data Structures

        #region Member Vars

        private ICondition _sourceCondition;

        #endregion // Member Vars

        #region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ComplementCondition"/>.
		/// </summary>
		public ComplementCondition( )
		{
		}

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ComplementCondition"/>.
        /// </summary>
		/// <param name="sourceCondition">Result of this condition will be complemented.</param>
        public ComplementCondition( ICondition sourceCondition )
        {
			_sourceCondition = sourceCondition;
        }

        #endregion // Constructor

        #region Base class overrides

		#region Equals
		/// <summary>
		/// Returns true if the passed in object is equal
		/// </summary>
		public override bool Equals(object obj)
		{
			ComplementCondition other = obj as ComplementCondition;

			return null != other && object.Equals(other._sourceCondition, _sourceCondition);
		}
		#endregion //Equals

		#region GetHashCode
		/// <summary>
		/// Caclulates a value used for hashing
		/// </summary>
		public override int GetHashCode()
		{
			return _sourceCondition != null ? _sourceCondition.GetHashCode() : 100;
		}
		#endregion //GetHashCode

		#region ToString

        /// <summary>
        /// Returns a string representation of the condition.
        /// </summary>
        public override string ToString()
        {
            return SR.GetString("ComplementCondition", new object[] { this.SourceCondition });
        }

        #endregion //ToString

        #endregion //Base class overrides	

		#region ICloneable.Clone

		object ICloneable.Clone( )
		{
			return new ComplementCondition( (ICondition)_sourceCondition.Clone( ) );
		}

		#endregion // ICloneable.Clone

        #region IsMatch

        /// <summary>
        /// Returns true if the specified value matches the condition. False otherwise. Value is considered
		/// to match this ComplementCondition if the value doesn't match the underlying <see cref="SourceCondition"/>.
        /// </summary>
		/// <remarks>
		/// <para class="body">
		/// Returns the complement of the result of the <see cref="SourceCondition"/>. If 
		/// <i>SourceCondition</i> is not specified, this method returns true. 
		/// </para>
		/// </remarks>
        /// <param name="value">Value to test.</param>
        /// <param name="context">Context information on where the value came from.</param>
        /// <returns>True if the value passes the condition, false otherwise.</returns>
        public bool IsMatch( object value, ConditionEvaluationContext context )
        {
            return null == _sourceCondition || ! _sourceCondition.IsMatch( value, context );
        }

        #endregion // IsMatch

		#region SourceCondition

		/// <summary>
		/// Specifies the source condition whose result will be complemented by this ComplementCondition.
		/// </summary>
		/// <seealso cref="ComparisonCondition"/>
		/// <seealso cref="ConditionGroup"/>
		/// <seealso cref="ICondition"/>
		public ICondition SourceCondition
		{
			get
			{
				return _sourceCondition;
			}
			set
			{
				_sourceCondition = value;
			}
		}

		#endregion // SourceCondition
	}

    #endregion // ComplementCondition Class
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