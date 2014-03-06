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
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A control which contains the information necessary to display the available filtering options.
	/// </summary>
	public abstract class FilterOperand
	{
		#region Members

		string _displayName;

		#endregion // Members

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="FilterOperand"/> class.
		/// </summary>
		protected FilterOperand()
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region DisplayName

		/// <summary>
		/// Gets the string that will be displayed in the drop down for this <see cref="FilterOperand"/>
		/// </summary>
		public virtual string DisplayName 
		{
			get
			{
				if (this._displayName == null)
					return this.DefaultDisplayName;

				return this._displayName;
			}
			set
			{
				this._displayName = value;
			}
		}

		#endregion // DisplayName

		#region Icon

		/// <summary>
		/// Gets / sets the DataTemplate that will be used to visually indicate the filter option.
		/// </summary>
		public DataTemplate Icon { get; set; }

		#endregion // Icon

		#region IconResolved

		/// <summary>
		/// Gets the <see cref="DataTemplate"/> which will be used as the graphical icon.
		/// </summary>
		public DataTemplate IconResolved
		{
			get
			{
				if (this.Icon != null)
					return this.Icon;

				if (this.XamWebGrid != null)
				{
					if (this.ComparisonOperatorValue != null)
					{
						ComparisonOperator op = (ComparisonOperator)this.ComparisonOperatorValue;
						if (this.XamWebGrid.FilterIcons.ContainsKey(op))
						{
							return this.XamWebGrid.FilterIcons[op];
						}
					}
				}
				return null;
			}
		}

		#endregion // IconResolved

		#region RequiresFilteringInput

		/// <summary>
		/// Gets if the then filter requires input to be applied or is standalone.
		/// </summary>
		public virtual bool RequiresFilteringInput
		{
			get
			{
				return true;
			}
		}

		#endregion // RequiresFilteringInput

		#endregion // Public

		#region Protected

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected virtual string DefaultDisplayName 
		{
			get 
			{ 
				return ""; 
			} 
		}

		#endregion // Protected

		#region Internal

		#region ComparisonOperator

		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>						
		public virtual ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return null;
			}
		}

		#endregion  // ComparisonOperator

		#region XamGrid

        private WeakReference _grid;

		/// <summary>
		/// The <see cref="XamGrid"/> which will provide the icons for this <see cref="FilterOperand"/>.
		/// </summary>
		internal XamGrid XamWebGrid
		{
            get
            {
                if (this._grid != null && this._grid.IsAlive)
                    return (XamGrid)this._grid.Target;

                return null;
            }
            set
            {
                if (this._grid == null || this._grid.Target != value)
                {
                    if (value != null)
                        this._grid = new WeakReference(value);
                    else
                        this._grid = null;
                }
            }
		}

		#endregion // XamGrid

		#endregion // Internal

		#endregion // Properties

		#region Methods

		#region Public

		/// <summary>
		/// Returns a Linq Expression which will be used for filtering.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual System.Linq.Expressions.Expression FilteringExpression(object value)
		{
			return null;
		}

		#endregion // Public

		#endregion // Methods
	}

	/// <summary>
	/// A class with the information for the Equals operand
	/// </summary>
	public class EqualsOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("EqualsString");
			}
		}

		#endregion // DefaultDisplayName

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.Equals;
			}
		}

		#endregion  // ComparisonOperator

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the NotEquals operand
	/// </summary>
	public class NotEqualsOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.NotEquals;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("NotEqualsString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the GreaterThan operand
	/// </summary>
	public class GreaterThanOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("GreaterThanString");
			}
		}

		#endregion // DefaultDisplayName

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.GreaterThan;
			}
		}

		#endregion  // ComparisonOperator

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the GreaterThanOrEquals operand
	/// </summary>
	public class GreaterThanOrEqualOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.GreaterThanOrEqual;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("GreaterThanOrEqualsString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the LessThan operand
	/// </summary>
	public class LessThanOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.LessThan;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("LessThanString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the LessThanOrEquals operand
	/// </summary>
	public class LessThanOrEqualOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.LessThanOrEqual;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("LessThanOrEqualsString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the StartsWith operand
	/// </summary>
	public class StartsWithOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.StartsWith;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("StartsWithString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the EndsWith operand
	/// </summary>
	public class EndsWithOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.EndsWith;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("EndsWithString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the Contains operand
	/// </summary>
	public class ContainsOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.Contains;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("ContainsString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the DoesNotContain operand
	/// </summary>
	public class DoesNotContainOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.DoesNotContain;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("DoesNotContainString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the DoesNotStartWith operand
	/// </summary>
	public class DoesNotStartWithOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.DoesNotStartWith;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("DoesNotStartWithString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

		#endregion // Overrides
	}

	/// <summary>
	/// A class with the information for the DoesNotEndWith operand
	/// </summary>
	public class DoesNotEndWithOperand : FilterOperand
	{
		#region Overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>
		public override ComparisonOperator? ComparisonOperatorValue
		{
			get
			{
				return ComparisonOperator.DoesNotEndWith;
			}
		}

		#endregion  // ComparisonOperator

		#region DefaultDisplayName

		/// <summary>
		/// Gets the string that will be displayed, when the DisplayName is not set. 
		/// </summary>
		protected override string DefaultDisplayName
		{
			get
			{
				return SRGrid.GetString("DoesNotEndWithString");
			}
		}

		#endregion // DefaultDisplayName

		#endregion // Properties

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