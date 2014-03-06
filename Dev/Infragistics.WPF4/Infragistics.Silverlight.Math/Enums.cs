using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Math
{
	#region ComparisonType

	/// <summary>
	/// Specifies one of the six comparison operators.
	/// </summary>
    public enum ComparisonType
	{ 
		/// <summary>
		/// Specifies a comparison using the GreaterThan operator.
		/// </summary>
		GreaterThan,

		/// <summary>
		/// Specifies a comparison using the GreaterThanOrEqual operator.
		/// </summary>
		GreaterThanOrEqualTo,

		/// <summary>
		/// Specifies a comparison using the LessThan operator.
		/// </summary>
		LessThan,

		/// <summary>
		/// Specifies a comparison using the LessThanOrEqual operator.
		/// </summary>
		LessThanOrEqualTo,

		/// <summary>
		/// Specifies a comparison using the Equality operator.
		/// </summary>
		EqualTo,

		/// <summary>
		/// Specifies a comparison using the InEquality operator.
		/// </summary>
		NotEqualTo
	}

	#endregion //CompareType

	#region ConvolutionType

	/// <summary>
	/// Specifies the type of convolution to operation to perform.
	/// </summary>
    public enum ConvolutionType
	{
		/// <summary>
		/// For inputs with length N1 and N2, returns an N1 + N2 - 1 length <see cref="Vector"/> representing the 
		/// full convolution operation.
		/// </summary>
		Full,

		/// <summary>
		/// For inputs with length N1 and N2, returns a max(N1,N2) length <see cref="Vector"/> representing the 
		/// center of the convolution operation.
		/// </summary>
		Center
	}

	#endregion //ConvolutionType

	#region FindValuesType

	internal enum FindValuesType
	{
		GreaterThanZero,
		GreaterThanOrEqualToZero,
		LessThanZero,
		LessThanOrEqualToZero,
	}

	#endregion  // FindValuesType

	#region StatisticsType

	/// <summary>
	/// Specifies the type of statistic to calculate for a given dataset.
	/// </summary>
    public enum StatisticsType
	{
		/// <summary>
		/// Specifies the population statistic, which treats the dataset as a complete population of data.
		/// </summary>
		Population,

		/// <summary>
		/// Specifies the sample statistic, which treats the dataset as a sample of a larger population.
		/// </summary>
		Sample
	}

	#endregion //StatisticsType

	#region VectorType

	/// <summary>
	/// Specifies the orientation of a <see cref="Vector"/> instance.
	/// </summary>
    public enum VectorType
	{ 
		/// <summary>
		/// Specifies a row Vector.
		/// </summary>
		Row,

		/// <summary>
		/// Specifies a column Vector.
		/// </summary>
		Column
	}

	#endregion //VectorType
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