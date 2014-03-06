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
	/// An object that is used to specify the Width of a <see cref="ColumnBase"/> object.
	/// </summary>	
	[TypeConverter(typeof(Infragistics.Controls.Grids.Primitives.ColumnWidthTypeConverter))]
	public struct ColumnWidth
	{
		#region Static Members

		static ColumnWidth _sizeToCells = new ColumnWidth(1, ColumnWidthType.SizeToCells);
		static ColumnWidth _auto = new ColumnWidth(1, ColumnWidthType.Auto);
		static ColumnWidth _initialAuto = new ColumnWidth(1, ColumnWidthType.InitialAuto);
		static ColumnWidth _sizeToHeader= new ColumnWidth(1, ColumnWidthType.SizeToHeader);
		static ColumnWidth _star = new ColumnWidth(1, ColumnWidthType.Star);

		#region SizeToHeader
		/// <summary>
		/// Gets an instance of a SizeToHeader <see cref="ColumnWidth"/>.
		/// </summary>
		public static ColumnWidth SizeToHeader
		{
			get { return _sizeToHeader; }
		}
		#endregion // SizeToHeader

		#region SizeToCells
		/// <summary>
		/// Gets an instance of a SizeToCells <see cref="ColumnWidth"/>.
		/// </summary>
		public static ColumnWidth SizeToCells
		{
			get { return _sizeToCells; }
		}
		#endregion // SizeToCells

		#region Auto
		/// <summary>
		/// Gets an instance of an Auto <see cref="ColumnWidth"/>.
		/// </summary>
		public static ColumnWidth Auto
		{
			get { return _auto; }
		}
		#endregion // Auto

		#region InitialAuto
		/// <summary>
		/// Gets an instance of an InitialAuto <see cref="ColumnWidth"/>.
		/// </summary>
		public static ColumnWidth InitialAuto
		{
			get { return _initialAuto; }
		}
		#endregion // InitialAuto

		#region Star
		/// <summary>
		/// Gets an instance of a Star <see cref="ColumnWidth"/>.
		/// </summary>
		public static ColumnWidth Star
		{
			get { return _star; }
		}
		#endregion // Star

		#endregion // Static Members

		#region Members

		double _value;
		ColumnWidthType _type;

		#endregion // Members

		#region Constructor

		private ColumnWidth(double value, ColumnWidthType type)
		{
			if (value < 0)
				value = 0; 
			this._value = value;
			this._type = type;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnWidth"/> struct of type ColumnWidthType.Numeric.
		/// </summary>
		/// <param propertyName="value">The width of the column.</param>
		/// <param propertyName="isStar">Whether the ColumnWidth should be of Type Star or Numeric.</param>
		public ColumnWidth(double value, bool isStar): this(value, isStar? ColumnWidthType.Star: ColumnWidthType.Numeric)
		{
		}

		#endregion // Constructor

		#region Properties

		#region Value

		/// <summary>
		/// Gets the value of the ColumnWidth property.
		/// </summary>
		/// <remarks>
		/// <see cref="ColumnWidthType"/>.Auto : 1 is returned.
		/// <see cref="ColumnWidthType"/>.SizeToCells : 1 is returned.
		/// <see cref="ColumnWidthType"/>.SizeToHeader : 1 is returned.
		/// <see cref="ColumnWidthType"/>.Numeric : the width of the column is returned.
		/// <see cref="ColumnWidthType"/>.Star : the percent compared to all other Start columns is returned.
		/// </remarks>
		public double Value
		{
			get { return this._value; }
		}
		#endregion // Value

		#region WidthType

		/// <summary>
		/// Gets the <see cref="ColumnWidthType"/> of the <see cref="ColumnWidth"/>
		/// </summary>
		public ColumnWidthType WidthType
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