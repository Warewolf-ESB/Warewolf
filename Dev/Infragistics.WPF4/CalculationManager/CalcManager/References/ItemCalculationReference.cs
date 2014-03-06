using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;
using System.Reflection;
using Infragistics.Windows.Internal;
using System.Windows.Data;

namespace Infragistics.Calculations
{
	internal class ItemCalculationReference : ItemPropertyReference
	{
		#region Private Members

		private ItemCalculatorReferenceBase _root;
		private ItemCalculation _calculation;
		private string _elementName;

		#endregion //Private Members	
    
		#region Constructor

		internal ItemCalculationReference(ItemCalculatorReferenceBase root, ItemCalculation calculation, string referenceId, object item, IItemPropertyValueAccessor valueAccessor, bool trackValueChange)
			: base(root, referenceId, item, valueAccessor, trackValueChange)
		{
			CoreUtilities.ValidateNotNull(calculation, "calculation");
			_root = root;
			_calculation = calculation;
		}

		#endregion //Constructor
 
		#region Properties

		#region Calculator

		internal ItemCalculation Calculation { get { return _calculation; } }

		#endregion //Calculator	
    
		#endregion //Properties	

		#region Methods

		#region Internal Methods

		#endregion //Internal Methods

		#endregion //Methods
    
		#region Base class overrides

		#region Properties

		#region BaseParent

		public override Engine.RefBase BaseParent
		{
			get
			{
				return _root;
			}
		}

		#endregion //BaseParent

		#region ElementName

		public override string ElementName
		{
			get
			{
				if (_elementName == null)
					_elementName = RefParser.EscapeString(_calculation.ReferenceIdResolved, false);

				return _elementName;
			}
		}

		#endregion //ElementName	
   
		#endregion // Properties

		#region Methods

		#region CreateReference

		public override ICalculationReference CreateReference(string inReference)
		{
			if (!Utils.IsRootReference(inReference))
			{
				var reference = _root.Calculator.GetReference(inReference);

				if (reference != null)
					return reference;
			}
			return base.CreateReference(inReference);
		}

		#endregion //CreateReference	

		#region GetCalculationValueCore

		protected override CalculationValue GetCalculationValueCore()
		{
			IItemPropertyValueAccessor valueAccessor = this.ValueAccessor;

			object valueToRtn = valueAccessor.GetValue(this.Item);

			IValueConverter converter = _root.Calculator.ValueConverter;
			Type treatAsType = _calculation.TreatAsTypeResolved;

			return Utils.ConvertValueHelper(valueToRtn, null, converter, valueAccessor.Name, treatAsType ?? valueAccessor.PropertyType, treatAsType); ;
		}

		#endregion //GetCalculationValueCore

		#endregion // Methods

		#endregion // Base class overrides
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