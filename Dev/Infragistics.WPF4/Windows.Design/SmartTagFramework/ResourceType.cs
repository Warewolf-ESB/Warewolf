
namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Represents an resource in a project
    /// </summary>
    public class ResourceType
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the ResourceType class. 
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="description">Resource description</param>
        public ResourceType(string extension, string description)
        {
            this.Extension = extension;
            this.Description = description;
        }

        #endregion //Constructor

        #region Properties

        #region Public Properties

        #region Extension

        /// <summary>
        /// File extension
        /// </summary>
        public string Extension
        {
            get;
            set;
        }

        #endregion //Extension

        #region Description

        /// <summary>
        /// Resource description
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        #endregion //Description

        #endregion //Public Properties

        #endregion //Properties

        #region Base Class Overrides

        #region ToString

		/// <summary>
		/// Returns a string representatioj of the ResourceType
		/// </summary>
		/// <returns></returns>
        public override string ToString()
        {
            return this.Extension;
        }

        #endregion //ToString

        #region Equals

		/// <summary>
		/// Returns true if the resource extension equals the specified object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.Extension.Equals(((ResourceType)obj).Extension);
        }

        #endregion //Equals

        #region GetHashCode

		/// <summary>
		/// Returns the instance's hashcode.
		/// </summary>
		/// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion //GetHashCode

        #endregion //Base Class Overrides
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