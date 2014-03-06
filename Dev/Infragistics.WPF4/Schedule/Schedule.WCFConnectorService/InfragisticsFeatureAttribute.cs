using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics
{
	/// <summary>
	/// Indicates an exposed type or member which was added as part of a new feature.
	/// </summary>
	[AttributeUsage(
		AttributeTargets.Class |
		AttributeTargets.Constructor |
		AttributeTargets.Delegate |
		AttributeTargets.Enum |
		AttributeTargets.Event |
		AttributeTargets.Field |
		AttributeTargets.Interface |
		AttributeTargets.Method |
		AttributeTargets.Property |
		AttributeTargets.Struct |
		AttributeTargets.Assembly,	// MD 12/15/09 - Allow the attribute on assemblies.
		Inherited = false )
	]
	[Conditional( "DEBUG" )]

	[Serializable] 

	// MD 12/15/09
	// Prevent this type from being included in the CMHs
	[InfragisticsFeature(Exclude = true)]



	public 

		class InfragisticsFeatureAttribute : Attribute
	{
		private bool exclude;
		private string featureName;
		private string subfeatureName;
		private string version;

		/// <summary>
		/// Creates a new <see cref="InfragisticsFeatureAttribute"/> instance.
		/// </summary>
		public InfragisticsFeatureAttribute() { }

		/// <summary>
		/// Gets or sets the value indicating whether member or type should be excluded from the internal feature documentation.
		/// </summary>
		public bool Exclude
		{
			get { return this.exclude; }
			set { this.exclude = value; }
		}

		/// <summary>
		/// Gets or sets a short name description of the feature.
		/// </summary>
		public string FeatureName
		{
			get { return this.featureName; }
			set { this.featureName = value; }
		}

		/// <summary>
		/// Gets or sets a more granular sub-feature name description of the feature.
		/// </summary>
		public string SubFeatureName
		{
			get { return this.subfeatureName; }
			set { this.subfeatureName = value; }
		}

		/// <summary>
		/// Gets or sets the version in which the feature was added.
		/// </summary>
		public string Version
		{
			get { return this.version; }
			set { this.version = value; }
		}
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