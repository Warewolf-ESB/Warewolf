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

	/// <summary>
	/// Used to specify formula and other calculation related settings on a control, like TextBox.
	/// This is also used to identify a control that will provide a source value for a formula.
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note:</b> this object can be used to specify calculation related settings for any <see cref="DependencyObject"/>, not just objects derived from <see cref="System.Windows.Controls.Control"/>.</para>
	/// </remarks>
	/// <see cref="XamCalculationManager.ControlSettingsProperty"/>
	public class ControlCalculationSettings : PropertyChangeNotifierExtended
	{
		#region Member Vars

		private string _property;
		private Binding _binding;
		private string _referenceId;
		private string _treatAsTypeName;
		private Type _treatAsType;
		private Type _treatAsTypeResolved;
		private string _formula;
		private IValueConverter _valueConverter; 

		#endregion // Member Vars

		#region Properties

		#region Public Properties

		#region Binding

		/// <summary>
		/// Specifies the binding that will be used to retrieve value from the control or set result of formula calculation on the control.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If the underlying control is a source of value for formula calculations, this binding will be used to retrieve the value of the control.
		/// If there's a formula associated with this control, this binding will be used to set the formula calculation result on the control.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that <b>Binding</b> and <see cref="Property"/> properties are mutually exclusive. You need to specify only one.
		/// </para>
		/// </remarks>
		/// <seealso cref="Property"/>
		public Binding Binding
		{
			get
			{
				return _binding;
			}
			set
			{
				if ( _binding != value )
				{
					_binding = value;
					this.RaisePropertyChangedEvent( "Binding" );
				}
			}
		}

		#endregion // Binding

		#region Formula

		/// <summary>
		/// Specifies the formula.
		/// </summary>
		public string Formula
		{
			get
			{
				return _formula;
			}
			set
			{
				if (_formula != value)
				{
					_formula = value;
					this.RaisePropertyChangedEvent("Formula");
				}
			}
		}

		#endregion // Formula

		#region Property

		/// <summary>
		/// Identifies the property of the control that will provide the source value for calculations when
		/// a formula references this control. If the control is a target of a formula, this property will 
		/// be set to the result of the formula calculation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If the underlying control is a source of value for formula calculations, this property will be used to retrieve the value of the control.
		/// If there's a formula associated with this control, the formula calculation result will be set on this property of the control.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that <b>Binding</b> and <see cref="Property"/> properties are mutually exclusive. You need to specify only one.
		/// </para>
		/// </remarks>
		public string Property
		{
			get
			{
				return _property;
			}
			set
			{
				if ( _property != value )
				{
					_property = value;
					this.RaisePropertyChangedEvent( "Property" );
				}
			}
		}

		#endregion // Property  
		
		#region ReferenceId

		/// <summary>
		/// Identifies the object. ReferenceId is used to refer to the value of the associated object in other formulas.
		/// </summary>
		public string ReferenceId
		{
			get
			{
				return _referenceId;
			}
			set
			{
				if (_referenceId != value)
				{
					_referenceId = value;
					this.RaisePropertyChangedEvent("ReferenceId");
				}
			}
		}

		#endregion // ReferenceId

		#region TreatAsType

		/// <summary>
		/// Specifies what type to treat the values of the associated object as. For example in the case of a <see cref="ControlCalculationSettings"/>
		/// associated to a TextBox, the Text property is string type however you may want to treat it as double when providing
		/// the value to the calc engine for formula calculations.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if this property is specified then the <see cref="TreatAsTypeName"/> setting will be ignored.</para>
		/// </remarks>
		///<seealso cref="TreatAsTypeName"/>
		///<seealso cref="TreatAsTypeResolved"/>
		[DefaultValue(null)]
		public Type TreatAsType
		{
			get
			{
				return _treatAsType;
			}
			set
			{
				if (_treatAsType != value)
				{
					_treatAsType = value;

					this.TreatAsTypeResolved = TypeResolverUtilities.ResolveType(this, _treatAsType, _treatAsTypeName, null, "UnknownTypeName");
					
					this.RaisePropertyChangedEvent("TreatAsType");
				}
			}
		}

		#endregion // TreatAsType

		#region TreatAsTypeName

		/// <summary>
		/// Specifies the name of the type to treat the values of the associated object as. For example in the case of a <see cref="ControlCalculationSettings"/>
		/// associated to a TextBox, the Text property is string type however you may want to treat it as double when providing
		/// the value to the calc engine for formula calculations.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="TreatAsType"/> property is specified then this setting will be ignored.</para>
		/// </remarks>
		///<seealso cref="TreatAsType"/>
		///<seealso cref="TreatAsTypeResolved"/>
		[DefaultValue(null)]
		public string TreatAsTypeName
		{
			get
			{
				return _treatAsTypeName;
			}
			set
			{
				if (_treatAsTypeName != value)
				{
					_treatAsTypeName = value;

					this.TreatAsTypeResolved = TypeResolverUtilities.ResolveType(this, _treatAsType, _treatAsTypeName, null, "UnknownTypeName");
					
					this.RaisePropertyChangedEvent("TreatAsTypeName");
				}
			}
		}

		#endregion // TreatAsTypeName

		#region TreatAsTypeResolved

		/// <summary>
		/// Determines what type to treat the values of the associated object as (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="TreatAsType"/> property is specified then it will be used, otherwise the <see cref="TreatAsTypeName"/> will be used to resolve the type.</para>
		/// </remarks>
		///<seealso cref="TreatAsType"/>
		///<seealso cref="TreatAsTypeName"/>
		[Browsable(false)]
		public Type TreatAsTypeResolved
		{
			get
			{
				return _treatAsTypeResolved;
			}
			private set
			{
				if (_treatAsTypeResolved != value)
				{
					_treatAsTypeResolved = value;
					this.RaisePropertyChangedEvent("TreatAsTypeResolved");
				}
			}
		}

		#endregion // TreatAsTypeResolved

		#region ValueConverter

		/// <summary>
		/// Specifies the converter to use to convert between the underlying value of the source object to the value
		/// that's used in calculations.
		/// </summary>
		public IValueConverter ValueConverter
		{
			get
			{
				return _valueConverter;
			}
			set
			{
				if (_valueConverter != value)
				{
					_valueConverter = value;
					this.RaisePropertyChangedEvent("ValueConverter");
				}
			}
		}

		#endregion // ValueConverter

		#endregion // Public Properties

		#endregion // Properties
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