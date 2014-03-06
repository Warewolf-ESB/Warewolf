using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Infragistics.Windows.DataPresenter;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter.Calculations
{
	/// <summary>
	/// Abstract base class for <see cref="FieldCalculationSettings"/> and <see cref="SummaryCalculationSettings"/>
	/// </summary>
	[InfragisticsFeature(FeatureName = "XamCalculationManager", Version = "11.2")]
	public abstract class DataPresenterCalculationSettingsBase : PropertyChangeNotifierExtended
	{
		#region Member Vars

		private string _referenceId;
		private string _treatAsTypeName;
		private Type _treatAsType;
		private Type _treatAsTypeResolved;
		private string _formula;
		private IValueConverter _valueConverter;

		#endregion // Member Vars

		#region Formula

		/// <summary>
		/// Specifies the formula to use to calculate the target's value.
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

		#region ReferenceId

		/// <summary>
		/// Identifies the object. Alias is used to refer to the value of the associated object in other formulas.
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
		/// Specifies what type to treat the values of the associated object as. For example in the case of a <see cref="FieldCalculationSettings"/>
		/// associated with a Field, if the DataType is string you may want to treat it as double when providing
		/// the value to the calculation engine for formula calculations.
		/// </summary>
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
		/// Specifies the name of the type to treat the values of the associated object as. For example in the case of a <see cref="FieldCalculationSettings"/>
		/// associated with a Field, if the DataType is string you may want to treat it as double when providing
		/// the value to the calculation engine for formula calculations.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="TreatAsType"/> property is specified then this setting will be ignored.</para>
		/// </remarks>
		///<seealso cref="TreatAsType"/>
		///<seealso cref="TreatAsTypeResolved"/>
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
		/// Determines what type to treat the values of the associated cell as (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="TreatAsType"/> property is specified then it will be used, otherwise the <see cref="TreatAsTypeName"/> will be used to resolve the type.</para>
		/// </remarks>
		///<seealso cref="TreatAsType"/>
		///<seealso cref="TreatAsTypeName"/>
		[Browsable(false)]

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

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
		/// Specifies the converter to use to convert between the underlying value of the cell to the value
		/// that is used in calculations.
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