using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Data;



using Infragistics.Windows.Licensing;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations
{
	#region CustomCalculationFunctionBase Class

	/// <summary>
	/// Abstract base class for <see cref="CustomCalculationFunction"/> class.
	/// </summary>
	public abstract class CustomCalculationFunctionBase : CalculationFunction
	{
		#region Vars

		private string _name;
		private string _description;
		private string _category;
		private string[] _argList;
		private string[] _argDescriptions;

		#endregion // Vars

		#region Properties

		#region Public Properties

		#region ArgDescriptors

		/// <summary>
		/// Gets descriptions of each argument to the function.
		/// </summary>
		public override string[] ArgDescriptors
		{
			get
			{
				return _argDescriptions;
			}
		}

		#endregion // ArgDescriptors

		#region ArgList

		/// <summary>
		/// Gets names of each argument to the function.
		/// </summary>
		public override string[] ArgList
		{
			get
			{
				return _argList;
			}
		}

		#endregion // ArgList

		#region Category

		/// <summary>
		/// Gets the category of the function. Used for formula editor UI to categorize functions.
		/// </summary>
		public override string Category
		{
			get
			{
				return _category;
			}
		}

		#endregion // Category

		#region Description

		/// <summary>
		/// Gets the description of the function. Used for formula editor UI.
		/// </summary>
		public override string Description
		{
			get
			{
				return _description;
			}
		}

		#endregion // Description

		#region Name

		/// <summary>
		/// Function name used to reference the function in a formula.
		/// </summary>
		public override string Name
		{
			get
			{
				return _name;
			}
		}

		#endregion // Name

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Protected Methods

		#region SetName

		/// <summary>
		/// Sets the value of the <see cref="Name"/> property.
		/// </summary>
		/// <param name="name">Name value.</param>
		protected void SetName( string name )
		{
			_name = name;
		}

		#endregion // SetName

		#region SetDescription

		/// <summary>
		/// Sets the value of the <see cref="Description"/> property.
		/// </summary>
		/// <param name="description"></param>
		protected void SetDescription( string description )
		{
			_description = description;
		}

		#endregion // SetDescription

		#region SetCategory

		/// <summary>
		/// Sets the value of the <see cref="Category"/> property.
		/// </summary>
		/// <param name="category"></param>
		protected void SetCategory( string category )
		{
			_category = category;
		}

		#endregion // SetCategory

		#region SetArgList

		/// <summary>
		/// Sets the value of the <see cref="ArgList"/> property.
		/// </summary>
		/// <param name="argList"></param>
		protected void SetArgList( string[] argList )
		{
			_argList = argList;
		}

		#endregion // SetArgList

		#region SetArgDescriptions

		/// <summary>
		/// Sets the value of the <see cref="ArgDescriptors"/> property.
		/// </summary>
		/// <param name="argDescriptions"></param>
		protected void SetArgDescriptions( string[] argDescriptions )
		{
			_argDescriptions = argDescriptions;
		}

		#endregion // SetArgDescriptions

		#endregion // MyRegionProtected Methods 

		#endregion // Methods
	} 

	#endregion // CustomCalculationFunctionBase Class

	#region CustomCalculationFunction Class

	/// <summary>
	/// Used to create a custom calculation function using function delegates.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>CustomCalculationFunction</b> derives from <see cref="CalculationFunction"/> base class. You can derive from
	/// the <i>CalculationFunction</i> class to create a custom calculation function for use in formulas. This class
	/// eliminates the need for deriving by letting you simply supply a delegate that contains the function logic.
	/// </para>
	/// </remarks>
	/// <seealso cref="CalculationFunction"/>
	/// <see cref="XamCalculationManager.RegisterUserDefinedFunction(CustomCalculationFunction)"/>
	public class CustomCalculationFunction : CustomCalculationFunctionBase
	{
		#region Vars

		private Func<double, double> _functionAA;
		private Func<double, double, double> _functionBB;
		private Func<double[], double> _functionCC;
		private int _minArgs;
		private int _maxArgs;

		#endregion // Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Function name that will be used in the formulas to reference this function.</param>
		/// <param name="function">Provides the calculation logic. The function takes a single double parameter and returns the result as a double.</param>
		public CustomCalculationFunction( string name, Func<double, double> function )
		{
			this.Initialize( name );
			_functionAA = function;
			_minArgs = _maxArgs = 1;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Function name that will be used in the formulas to reference this function.</param>
		/// <param name="function">Provides the calculation logic. The function takes a two double parameters and returns the result as a double.</param>
		public CustomCalculationFunction( string name, Func<double, double, double> function )
		{
			this.Initialize( name );
			_functionBB = function;
			_minArgs = _maxArgs = 2;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Function name that will be used in the formulas to reference this function.</param>
		/// <param name="function">Provides the calculation logic. The function takes an array of double parameters and returns the result as a double.</param>
		/// <param name="minArgs">Minimum numer of arguments supported by the function.</param>
		/// <param name="maxArgs">Maximum numer of arguments supported by the function.</param>
		public CustomCalculationFunction( string name, Func<double[], double> function, int minArgs, int maxArgs )
		{
			this.Initialize( name );
			_functionCC = function;
			_minArgs = minArgs;
			_maxArgs = maxArgs;
		} 

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region ArgDescriptors

		/// <summary>
		/// Gets or sets the descriptions of each argument to the function.
		/// </summary>
		public new string[] ArgDescriptors
		{
			get
			{
				return base.ArgDescriptors;
			}
			set
			{
				this.SetArgDescriptions( value );
			}
		}

		#endregion // ArgDescriptors 

		#region ArgList

		/// <summary>
		/// Gets or sets names of each argument to the function.
		/// </summary>
		public new string[] ArgList
		{
			get
			{
				return base.ArgList;
			}
			set
			{
				base.SetArgList( value );
			}
		}

		#endregion // ArgList

		#region Category

		/// <summary>
		/// Gets or sets the category of the function. Used for formula editor UI to categorize functions.
		/// </summary>
		public new string Category
		{
			get
			{
				return base.Category;
			}
			set
			{
				base.SetCategory( value );
			}
		}

		#endregion // Category

		#region Description

		/// <summary>
		/// Gets or sets the description of the function. Used for formula editor UI.
		/// </summary>
		public new string Description
		{
			get
			{
				return base.Description;
			}
			set
			{
				base.SetDescription( value );
			}
		}

		#endregion // Description		

		#region MaxArgs

		/// <summary>
		/// Maximum number of arguments.
		/// </summary>
		public override int MaxArgs
		{
			get
			{
				return _maxArgs;
			}
		}

		#endregion // MaxArgs

		#region MinArgs

		/// <summary>
		/// Minimum number of arguments.
		/// </summary>
		public override int MinArgs
		{
			get
			{
				return _minArgs;
			}
		}

		#endregion // MinArgs

		#endregion // Properties

		#region Methods

		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return _functionCC != null;
		}

		#endregion  // CanParameterBeEnumerable

		#region Evaluate

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="numberStack">Stack from which to pop off <paramref name="argumentCount"/> number of arguments
		/// to be used by the function.</param>
		/// <param name="argumentCount">Number of arguments passed into the function.</param>
		/// <returns></returns>
		protected override CalculationValue Evaluate( CalculationNumberStack numberStack, int argumentCount )
		{
			Debug.Assert( argumentCount >= _minArgs && argumentCount <= _maxArgs );

			CalculationValue error;
			var values = this.GetArgumentsAsDouble( numberStack, argumentCount, out error, false );
			if ( null != error )
				return error;

			try
			{
				double result;
				if ( null != _functionAA )
					result = _functionAA( values[0] );
				else if ( null != _functionBB )
					result = _functionBB( values[0], values[1] );
				else if ( null != _functionCC )
					result = _functionCC( values );
				else
				{
					Debug.Assert( false );
					return Utils.ToCalcValue( CalculationErrorCode.Num );
				}

				return new CalculationValue( result );
			}
			catch ( Exception exception )
			{
				return new CalculationValue( exception );
			}
		}

		#endregion // Evaluate  

		#endregion // Methods

		#endregion // Base Overrides

		#region Private Methods

		#region GetArgumentsAsDouble

		private double[] GetArgumentsAsDouble( CalculationNumberStack numberStack,
			int argumentCount, out CalculationValue error, bool skipNonDoubles )
		{
			var values = this.GetArguments( numberStack, argumentCount, false );

			List<double> list = new List<double>( argumentCount );

			error = null;

			foreach ( var ii in values )
			{
				if ( ii.IsError )
				{
					error = new CalculationValue( ii.ToErrorValue( ) );
				}
				else
				{
					double d;
					if ( ii.ToDouble( out d ) )
					{
						list.Add( d );
					}
					else if ( !skipNonDoubles )
					{
						// MD 7/14/08
						// Found while implementing Excel formula solving
						// The wrong error is being returned here. It should have returned a #VALUE! error, which can be seen in Excel.
						//return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
						error = Utils.ToCalcValue( CalculationErrorCode.Value );
					}
				}

				if ( null != error )
					return null;
			}

			return list.ToArray( );
		}

		#endregion // GetArgumentsAsDouble

		#region Initialize

		private void Initialize( string name )
		{
			CoreUtilities.ValidateNotEmpty( name, "name" );

			base.SetName( name );
		}

		#endregion // Initialize

		#endregion // Private Methods
	}

	#endregion // CustomCalculationFunction Class

	
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