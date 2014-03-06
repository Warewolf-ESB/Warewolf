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

using Infragistics.Calculations;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations
{

	/// <summary>
	/// Defines a calculation to be performed on a item.
	/// </summary>
	/// <seealso cref="ListCalculator"/>
	/// <seealso cref="ListCalculator.ItemCalculations"/>
	/// <seealso cref="ItemCalculator"/>
	/// <seealso cref="ItemCalculator.Calculations"/>
	public sealed class ItemCalculation : ItemCalculationBase
	{
		#region Private Members

		private string _targetProperty;
		private string _refIdResolved;
		private bool _refIdErrorLogged;
		private bool _hasTargetProperty;
		private string _treatAsTypeName;
		private Type _treatAsType;
		private Type _treatAsTypeResolved;

		#endregion //Private Members	
 
		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="ItemCalculation"/>
		/// </summary>
		public ItemCalculation()
		{
		}

		#endregion //Constructor	

		#region Base class overrides

		#region OnPropertyChanged

		/// <summary>
		/// Called when property has changed value
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		protected override void OnPropertyChanged(string propertyName)
		{
			switch (propertyName)
			{
				case "ReferenceId":
				case "TargetProperty":
					_refIdResolved = null;
					_refIdErrorLogged = false;
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

		#endregion //OnPropertyChanged	
    
		#region ReferenceIdResolved

		internal override string ReferenceIdResolved
		{
			get
			{
				if (_refIdResolved == null)
				{
					_refIdResolved = this.ReferenceId;

					if (string.IsNullOrWhiteSpace(_refIdResolved))
					{
						_refIdResolved = _targetProperty;

						if (!_refIdErrorLogged && string.IsNullOrWhiteSpace(_refIdResolved))
						{
							_refIdErrorLogged = true;

							Utils.LogDebuggerWarning(SRUtil.GetString("ItemCalculation_RefID_Warning", this) + Environment.NewLine);
						}
					}
				}

				return _refIdResolved;
			}
		}

		#endregion //ReferenceIdResolved

		#region ToString

		/// <summary>
		/// Returns a string that represents this object;
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(SRUtil.GetString("ItemCalculation_Desc"), this.ReferenceId, this.Formula, this.TargetProperty);
		}

		#endregion //ToString	
    
		#endregion //Base class overrides	
    
		#region Properties

		#region Public Properties

		#region TargetProperty

		/// <summary>
		/// Target property is optional. If specified the result of the calculation will be set on this property.
		/// </summary>
		public string TargetProperty
		{
			get { return _targetProperty; }
			set
			{
				if (value != _targetProperty)
				{
					_targetProperty = value;

					_hasTargetProperty = !string.IsNullOrWhiteSpace(_targetProperty);

					this.RaisePropertyChangedEvent("TargetProperty");
				}
			}
		}

		#endregion //TargetProperty

		#region TreatAsType

		/// <summary>
		/// Specifies what type to treat the values of the associated object as. For example if the <see cref="TargetProperty"/>
		/// is of type string but it actually contains string representations of date values set this property to <see cref="DateTime"/>.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if this property is specified then the <see cref="TreatAsTypeName"/> setting will be ignored.</para>
		/// </remarks>
		///<seealso cref="TreatAsTypeName"/>
		///<seealso cref="TreatAsTypeResolved"/>
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
		/// Specifies what type to treat the values of the associated object as. For example if the <see cref="TargetProperty"/>
		/// is of type string but it actually contains string representations of date values set this property to "datetime" or "datetime?".
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

		#endregion //Public Properties

		#region Internal Properties

		#region HasTargetProperty

		internal bool HasTargetProperty { get { return _hasTargetProperty; } }

		#endregion //HasTargetProperty

		#endregion //Internal Properties	
        
		#endregion //Properties

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