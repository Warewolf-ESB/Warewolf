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

namespace Infragistics.Controls.Editors
{
   /// <summary>
	/// An object that is used to specify the Width of a <see cref="ComboColumn"/> object.
	/// </summary>	
	public struct ComboColumnWidth
	{
		#region Static Members

		static ComboColumnWidth _sizeToCells = new ComboColumnWidth(1, ComboColumnWidthType.SizeToCells);
		static ComboColumnWidth _auto = new ComboColumnWidth(1, ComboColumnWidthType.Auto);
		static ComboColumnWidth _initialAuto = new ComboColumnWidth(1, ComboColumnWidthType.InitialAuto);
		static ComboColumnWidth _sizeToHeader= new ComboColumnWidth(1, ComboColumnWidthType.SizeToHeader);
		static ComboColumnWidth _star = new ComboColumnWidth(1, ComboColumnWidthType.Star);

		#region SizeToHeader
		/// <summary>
		/// Gets an instance of a SizeToHeader <see cref="ComboColumnWidth"/>.
		/// </summary>
		public static ComboColumnWidth SizeToHeader
		{
			get { return _sizeToHeader; }
		}
		#endregion // SizeToHeader

		#region SizeToCells
		/// <summary>
		/// Gets an instance of a SizeToCells <see cref="ComboColumnWidth"/>.
		/// </summary>
		public static ComboColumnWidth SizeToCells
		{
			get { return _sizeToCells; }
		}
		#endregion // SizeToCells

		#region Auto
		/// <summary>
		/// Gets an instance of an Auto <see cref="ComboColumnWidth"/>.
		/// </summary>
		public static ComboColumnWidth Auto
		{
			get { return _auto; }
		}
		#endregion // Auto

		#region InitialAuto
		/// <summary>
		/// Gets an instance of an InitialAuto <see cref="ComboColumnWidth"/>.
		/// </summary>
		public static ComboColumnWidth InitialAuto
		{
			get { return _initialAuto; }
		}
		#endregion // InitialAuto

		#region Star
		/// <summary>
		/// Gets an instance of a Star <see cref="ComboColumnWidth"/>.
		/// </summary>
		public static ComboColumnWidth Star
		{
			get { return _star; }
		}
		#endregion // Star

		#endregion // Static Members

		#region Members

		double _value;
		ComboColumnWidthType _type;

		#endregion // Members

		#region Constructor

		private ComboColumnWidth(double value, ComboColumnWidthType type)
		{
			if (value < 0)
				value = 0; 
			this._value = value;
			this._type = type;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ComboColumnWidth"/> struct of type ComboColumnWidthType.Numeric.
		/// </summary>
		/// <param propertyName="value">The width of the column.</param>
		/// <param propertyName="isStar">Whether the ComboColumnWidth should be of Type Star or Numeric.</param>
		public ComboColumnWidth(double value, bool isStar): this(value, isStar? ComboColumnWidthType.Star: ComboColumnWidthType.Numeric)
		{
		}

		#endregion // Constructor

		#region Properties

		#region Value

		/// <summary>
		/// Gets the value of the ComboColumnWidth property.
		/// </summary>
		/// <remarks>
		/// <see cref="ComboColumnWidthType"/>.Auto : 1 is returned.
		/// <see cref="ComboColumnWidthType"/>.SizeToCells : 1 is returned.
		/// <see cref="ComboColumnWidthType"/>.SizeToHeader : 1 is returned.
		/// <see cref="ComboColumnWidthType"/>.Numeric : the width of the column is returned.
		/// <see cref="ComboColumnWidthType"/>.Star : the percent compared to all other Start columns is returned.
		/// </remarks>
		public double Value
		{
			get { return this._value; }
		}
		#endregion // Value

		#region WidthType

		/// <summary>
		/// Gets the <see cref="ComboColumnWidthType"/> of the <see cref="ComboColumnWidth"/>
		/// </summary>
		public ComboColumnWidthType WidthType
		{
			get { return this._type; }
		}
		#endregion // WidthType

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