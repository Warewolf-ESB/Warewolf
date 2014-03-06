using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Internal;
using System.Collections;
using Infragistics.Shared;

namespace Infragistics.Windows.Controls
{
    #region ConditionGroup Class

	/// <summary>
	/// Class used for grouping multiple conditions.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ConditionGroup</b> class is used for grouping multiple conditions. ConditionGroup itself implements 
	/// <see cref="ICondition"/> interface and therefore you can create arbitrarily nested groups of conditions.
	/// </para>
	/// <para class="body">
	/// Conditions contained in the condition group are combined using the logical operator specified by the
	/// <see cref="ConditionGroup.LogicalOperator"/> property.
	/// </para>
	/// </remarks>
	/// <seealso cref="ICondition"/>
	/// <seealso cref="ComparisonCondition"/>
	/// <seealso cref="ComplementCondition"/>
    public class ConditionGroup : ObservableCollection<ICondition>, ICondition
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
							new PropertySerializationInfo( typeof( LogicalOperator ), "LogicalOperator" ),
							new PropertySerializationInfo( typeof( ICondition ), SerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY ),
						};
					}

					return _props;
				}
			}

			public override Dictionary<string, object> Serialize( object obj )
			{
				ConditionGroup cg = (ConditionGroup)obj;
				Dictionary<string, object> values = new Dictionary<string, object>( );

				values["LogicalOperator"] = cg.LogicalOperator;
				values[SerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY] = cg;

				return values;
			}

			public override object Deserialize( Dictionary<string, object> values )
			{
				object v;

				ConditionGroup cg = new ConditionGroup( );

				if ( values.TryGetValue( "LogicalOperator", out v ) )
					cg.LogicalOperator = (LogicalOperator)v;

				if ( values.TryGetValue( SerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY, out v ) )
				{
					foreach ( ICondition c in (IEnumerable)v )
						cg.Add( c );
				}

				return cg;
			}
		}

		#endregion // SerializationInfo Class

		#endregion // Nested Data Structures

		#region Member Vars

		private LogicalOperator _logicalOperator = LogicalOperator.And;
        private object _tooltip;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ConditionGroup"/>.
        /// </summary>
        public ConditionGroup( )
        {
        }

        #endregion // Constructor

        #region Base class overrides

		#region Equals
		/// <summary>
		/// Returns true if the passed in object is equal
		/// </summary>
		public override bool Equals(object obj)
		{
			ConditionGroup other = obj as ConditionGroup;

			if (null == other ||
				other._logicalOperator != _logicalOperator ||
				// AS 5/29/09
				// Removing tooltip from calculation. We don't clone it
				// and its getter returns a manipulated version of the member 
				// possibly.
				//
				//!object.Equals(other._tooltip, _tooltip) ||
				other.Count != this.Count)
				return false;

			for (int i = 0, count = this.Count; i < count; i++)
			{
				if (!object.Equals(this[i], other[i]))
					return false;
			}

			return true;
		}
		#endregion //Equals

		#region GetHashCode
		/// <summary>
		/// Caclulates a value used for hashing
		/// </summary>
		public override int GetHashCode()
		{
			int hash = _logicalOperator.GetHashCode();

			// AS 5/29/09
			// Removing tooltip from calculation. We don't clone it
			// and its getter returns a manipulated version of the member 
			// possibly.
			//
			//if (null != _tooltip)
			//	hash |= _tooltip.GetHashCode();

			for (int i = 0, count = this.Count; i < count; i++)
			{
				ICondition child = this[i];

				if (null != child)
					hash |= child.GetHashCode();
			}

			return hash;
		}
		#endregion //GetHashCode

		#region ToString

        /// <summary>
        /// Returns a string representation of the group.
        /// </summary>
        public override string ToString()
        {
            return SR.GetString("ConditionGroup_Description");
        }

        #endregion //ToString

        #endregion //Base class overrides	
        
        #region Properties

        #region Public Properties

        #region ToolTip

        /// <summary>
        /// Gets/sets a tooltip to be used for this group.
        /// </summary>
        /// <remarks>
        /// <para class="body">If not set this will a formatted string. </para>
        /// </remarks>
        public object ToolTip
        {
            get
            {
                if (this._tooltip != null)
                    return _tooltip;

                return this.FormatToolTipText();
            }
            set
            {
                this._tooltip = value;
                this.RaisePropertyChanged("ToolTip");
            }
        }

        #endregion //ToolTip	
    
        #region LogicalOperator

        /// <summary>
        /// Specifies how to combine results of conditions in the collection.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// <b>LogicalOperator</b> property specifies whether the conditions in this condition group should
        /// be combined using boolean function 'or' or 'and'.
        /// </para>
        /// </remarks>
        public LogicalOperator LogicalOperator
        {
            get
            {
                return _logicalOperator;
            }
            set
            {
                if ( _logicalOperator != value )
                {
					Utilities.ThrowIfInvalidEnum( value, "LogicalOperator" );

                    _logicalOperator = value;

                    this.RaisePropertyChanged( "LogicalOperator" );
                }
            }
        }

        #endregion // LogicalOperator

        #endregion // Public Properties

        #endregion // Properties

        #region Methods

        #region Public Methods

		#region ICloneable.Clone

		object ICloneable.Clone( )
		{
			ConditionGroup g = new ConditionGroup( );
			g._logicalOperator = _logicalOperator;

			foreach ( ICondition c in this )
				g.Add( (ICondition)c.Clone( ) );

			return g;
		}

		#endregion // ICloneable.Clone

        #region IsMatch

        /// <summary>
        /// Returns true if the specified value matches any one or all conditions in this condition group depending
        /// on the value of the <see cref="LogicalOperator"/> property. False otherwise.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <param name="context">Context information on where the value came from.</param>
        /// <returns>True if the value matches, false otherwise.</returns>
        public bool IsMatch( object value, ConditionEvaluationContext context )
        {
            LogicalOperator logicalOp = this.LogicalOperator;

            int count = this.Count;
            for ( int i = 0; i < count; i++ )
            {
                ICondition c = this[i];
                if ( c.IsMatch( value, context ) )
                {
                    if ( LogicalOperator.Or == logicalOp )
                        return true;
                }
                else
                {
                    if ( LogicalOperator.And == logicalOp )
                        return false;
                }
            }

            return 0 == count || LogicalOperator.And == logicalOp;
        }

        #endregion // IsMatch

        #endregion // Public Methods

		#region Private Methods

        #region FormatToolTipText

        private string FormatToolTipText()
        {
            int count = this.Count;

            if (count == 0)
                return string.Empty;

            if (count == 1)
                return this[0].ToString();

			// AS 5/17/11 NA 11.2 Excel Style Filtering
			// Added special logic for when we have multiple equals values.
			//
			if (count > 2 && _logicalOperator == Controls.LogicalOperator.Or)
			{
				ComparisonCondition comparisonCondition = this[0] as ComparisonCondition;

				if (comparisonCondition != null)
				{
					ComparisonOperator? comparisonOperator = comparisonCondition.Operator;

					if (comparisonOperator == ComparisonOperator.Equals)
					{
						for (int i = 1; i < count; i++)
						{
							comparisonCondition = this[i] as ComparisonCondition;

							if (comparisonCondition == null || comparisonCondition.Operator != comparisonOperator)
							{
								comparisonOperator = null;
								break;
							}
						}

						if (null != comparisonOperator)
						{
							StringBuilder sb = new StringBuilder();
							string separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

							if (string.IsNullOrEmpty(separator))
								separator = " ";
							else if (separator.Length == 1 && !char.IsWhiteSpace(separator[0]))
								separator += " ";

							for (int i = 0; i < count; i++)
							{
								if (i > 0)
									sb.Append(separator);

								sb.Append( ((ComparisonCondition)this[i]).Value );
							}

							return SR.GetString("ConditionGroup_Equals", sb.ToString());
						}
					}
				}
			}

            string str = null;
            string logicalConnectorString;

            if (this.LogicalOperator == LogicalOperator.And)
                logicalConnectorString = SR.GetString("ConditionGroup_Logical_AND");
            else
                logicalConnectorString = SR.GetString("ConditionGroup_Logical_OR");

            for (int i = 0; i < count; i++)
            {
                string temp;

                ConditionGroup group = this[i] as ConditionGroup;

				if (group != null)
				{
					temp = group.FormatToolTipText();

					// AS 5/17/11 NA 11.2 Excel Style Filtering
					// Just put the () around the nested groups.
					//
					temp = SR.GetString("ConditionGroup_Group", temp);
				}
				else
					temp = this[i].ToString();

                if (str == null)
                    str = temp;
                else
                {
                    try
                    {
                        str = string.Format(logicalConnectorString, new object[] { str, temp });
                    }
                    catch
                    {
                    }
                }
            }

			// AS 5/17/11 NA 11.2 Excel Style Filtering
			// Instead of putting the ()'s around every group we'll just 
			// put it around groups within a group.
			//
			//return SR.GetString("ConditionGroup_Group", str);
            return str;
        }

        #endregion //FormatToolTipText	
    
		#region RaisePropertyChanged

		private void RaisePropertyChanged( string propName )
		{
			this.OnPropertyChanged( new PropertyChangedEventArgs( "LogicalOperator" ) );
		}

		#endregion // RaisePropertyChanged

		#endregion // Private Methods

		#endregion // Methods
	}

    #endregion // ConditionGroup Class
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