
// JJD 8/28/02 - Added support for runtime string customizations
// This class needs to be outside the namespace
using System.Globalization;
using Infragistics.Shared;
internal static class ResourceCustomizerLocator
{
	internal static readonly Infragistics.Shared.ResourceCustomizer Customizer = new Infragistics.Shared.ResourceCustomizer();
}

//-----------------------------------------------------------------
// Note: The only code that needs to be changed is the namespace
//       below, which should be the name of the primary namespace
//       of the assembly. However, no 2 assemblies should have an
//       instance of this file with the same namespace specified 
//       below.
//-----------------------------------------------------------------
namespace Infragistics.Windows.DataPresenter.ExcelExporter // change this line only to the unigue namespace of this assembly
{
	/// <summary>
	/// Exposes a <see cref="Infragistics.Shared.ResourceCustomizer"/> instance for this assembly. 
	/// </summary>
	/// <seealso cref="Customizer"/> 
	/// <seealso cref="Infragistics.Shared.ResourceCustomizer"/> 
	public static class Resources
	{
		/// <summary>
		/// Returns the <see cref="Infragistics.Shared.ResourceCustomizer"/> for this assembly.
		/// </summary>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer"/> 
#pragma warning disable 436
		public static Infragistics.Shared.ResourceCustomizer Customizer { get { return ResourceCustomizerLocator.Customizer; } } 
#pragma warning restore 436

		// AS 7/15/05
		// Provide a way for the customer to get the resolved resource
		// string/object that the assembly has access to.
		//
		#region GetString
		/// <summary>
		/// Returns the resource string using the specified name and default culture.
		/// </summary>
		/// <param name="name">Name of the string resource to return.</param>
		/// <param name="args">Arguments supplied to the string.Format method when formatting the string.</param>
		public static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args); 
#pragma warning restore 436
		}

		/// <summary>
		/// Returns the resource string using the specified resource name and default culture. The string is then formatted using the arguments specified.
		/// </summary>
		/// <param name="name">Name of the string resource to return.</param>
		public static string GetString(string name)
		{
#pragma warning disable 436
			return SR.GetString(name); 
#pragma warning restore 436
		}
		#endregion //GetString


		#region GetDynamicResourceString

		// JJD 3/07/07
		// Added support for DynamicResourceStrings

		/// <summary>
		/// Returns a DynamicResourceString object that tracks customization changes to a resource and raises an event when the resource value is changed.
		/// </summary>
		/// <param name="name">Name of the string resource to return.</param>
		/// <returns>A <see cref="Infragistics.Shared.DynamicResourceString"/> object to track changes in the resource customizations.</returns>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.SetCustomizedString"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.ResetCustomizedString"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.ResetAllCustomizedStrings"/>
		public static Infragistics.Shared.DynamicResourceString GetDynamicResourceString(string name)
		{
#pragma warning disable 436
			return SR.GetDynamicResourceString(name, null, null); 
#pragma warning restore 436
		}

		/// <summary>
		/// Returns a DynamicResourceString object that tracks customization changes to a resource and raises an event when the resource value is changed.
		/// </summary>
		/// <param name="name">Name of the string resource to return.</param>
		/// <param name="args">Arguments supplied to the string.Format method when formatting the string.</param>
		/// <returns>A <see cref="Infragistics.Shared.DynamicResourceString"/> object to track changes in the resource customizations.</returns>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.SetCustomizedString"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.ResetCustomizedString"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.ResetAllCustomizedStrings"/>
		public static Infragistics.Shared.DynamicResourceString GetDynamicResourceString(string name, object[] args)
		{
#pragma warning disable 436
			return SR.GetDynamicResourceString(name, args, null); 
#pragma warning restore 436
		}

		/// <summary>
		/// Returns a DynamicResourceString object that tracks customization changes to a resource and raises an event when the resource value is changed.
		/// </summary>
		/// <param name="name">Name of the string resource to return.</param>
		/// <param name="args">Arguments supplied to the string.Format method when formatting the string.</param>
		/// <param name="culture">Culture whose resource should loaded. The resource will go through a fallback process to locate the appropriate resources.</param>
		/// <returns>A <see cref="Infragistics.Shared.DynamicResourceString"/> object to track changes in the resource customizations.</returns>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.SetCustomizedString"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.ResetCustomizedString"/>
		/// <seealso cref="Infragistics.Shared.ResourceCustomizer.ResetAllCustomizedStrings"/>
		public static Infragistics.Shared.DynamicResourceString GetDynamicResourceString(string name, object[] args, CultureInfo culture)
		{
#pragma warning disable 436
			return SR.GetDynamicResourceString(name, args, null); 
#pragma warning restore 436
		}

		#endregion //GetDynamicResourceString


		#region GetObject
		/// <summary>
		/// Returns the resource object using the specified name.
		/// </summary>
		/// <param name="name">Name of the resource item</param>
		/// <returns>An object containing the specified resource</returns>
		public static object GetObject(string name)
		{
#pragma warning disable 436
			return SR.GetObject(name); 
#pragma warning restore 436
		}
		#endregion //GetObject

        #region DynamicResourceString properties

        /// <summary>
        /// Returns a <see cref="DynamicResourceString"/> instance that contains the localized resource used in the <see cref="FixedFieldButton"/> as the default for its <see cref="FixedFieldButton.FixToFarEdgePrompt"/> property./>
        /// </summary>
        public static DynamicResourceString FieldFixToFarEdgePrompt
        {
            get { return GetDynamicResourceString("FixedFieldButton_FixToFarEdgePromptProperty"); }
        }

        /// <summary>
        /// Returns a <see cref="DynamicResourceString"/> instance that contains the localized resource used in the <see cref="FixedFieldButton"/> as the default for its <see cref="FixedFieldButton.FixToNearEdgePrompt"/> property./>
        /// </summary>
        public static DynamicResourceString FieldFixToNearEdgePrompt
        {
            get { return GetDynamicResourceString("FixedFieldButton_FixToNearEdgePromptProperty"); }
        }

        /// <summary>
        /// Returns a <see cref="DynamicResourceString"/> instance that contains the localized resource used in the <see cref="FixedFieldButton"/> as the default for its <see cref="FixedFieldButton.UnfixPrompt"/> property./>
        /// </summary>
        public static DynamicResourceString FieldUnfixPrompt
        {
            get { return GetDynamicResourceString("FixedFieldButton_UnfixPromptProperty"); }
        }

        /// <summary>
        /// Returns a <see cref="DynamicResourceString"/> instance that contains the localized resource used in the <see cref="GroupByAreaBase"/> as the default for its <see cref="GroupByAreaBase.Prompt1"/> property./>
        /// </summary>
        public static DynamicResourceString GroupByAreaPrompt1
        {
            get { return GetDynamicResourceString("GroupByArea_Prompt1"); }
        }

        /// <summary>
        /// Returns a <see cref="DynamicResourceString"/> instance that contains the localized resource used in the <see cref="GroupByAreaBase"/> as the default for its <see cref="GroupByAreaBase.Prompt2"/> property./>
        /// </summary>
        public static DynamicResourceString GroupByAreaPrompt2
        {
            get { return GetDynamicResourceString("GroupByArea_Prompt2"); }
        }

        /// <summary>
        /// Returns a <see cref="DynamicResourceString"/> instance that contains the localized resource used in the <see cref="FixedRecordButton"/> as the default for its <see cref="FixedRecordButton.FixToBottomPrompt"/> property./>
        /// </summary>
        public static DynamicResourceString RecordFixToBottomPrompt
        {
            get { return GetDynamicResourceString("FixedRecordButton_FixToBottomPromptProperty"); }
        }

        /// <summary>
        /// Returns a <see cref="DynamicResourceString"/> instance that contains the localized resource used in the <see cref="FixedRecordButton"/> as the default for its <see cref="FixedRecordButton.FixToTopPrompt"/> property./>
        /// </summary>
        public static DynamicResourceString RecordFixToTopPrompt
        {
            get { return GetDynamicResourceString("FixedRecordButton_FixToTopPromptProperty"); }
        }

        /// <summary>
        /// Returns a <see cref="DynamicResourceString"/> instance that contains the localized resource used in the <see cref="FixedRecordButton"/> as the default for its <see cref="FixedRecordButton.UnfixPrompt"/> property./>
        /// </summary>
        public static DynamicResourceString RecordUnfixPrompt
        {
            get { return GetDynamicResourceString("FixedRecordButton_UnfixPromptProperty"); }
        }

        #endregion //DynamicResourceString properties

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