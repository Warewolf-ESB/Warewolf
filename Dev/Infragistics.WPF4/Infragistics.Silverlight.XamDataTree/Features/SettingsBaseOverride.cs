namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A base object for a settings objects on a <see cref="NodeLayout"/>.
    /// </summary>
    public abstract class SettingsBaseOverride : DependencyObjectNotifier
    {
        #region Public

        #region NodeLayout

        /// <summary>
        /// Gets the <see cref="NodeLayout"/> that owns the <see cref="SettingsBaseOverride"/> object.
        /// </summary>
        protected internal NodeLayout NodeLayout
        {
            get;
            set;
        }

        #endregion // NodeLayout

        #endregion // Public

        #region Protected

        #region SettingsObject

        /// <summary>
        /// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsBaseOverride"/>
        /// </summary>
        protected abstract SettingsBase SettingsObject
        {
            get;
        }

        #endregion // SettingsObject

        #endregion // Protected

        #region Methods

        #region Protected

        #region InvalidateResolvedProperties
        /// <summary>
        /// A method designed to raise the OnPropertyChanged for the resolved properties so that changes from higher levels can be reflected through bindings correctly.
        /// </summary>
        /// <param name="propertyName"></param>
        protected internal virtual void InvalidateResolvedProperties(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                this.OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// A method designed to raise the OnPropertyChanged for the resolved properties so that changes from higher levels can be reflected through bindings correctly.
        /// </summary>
        protected internal virtual void InvalidateResolvedProperties()
        {
            this.InvalidateResolvedProperties(null);
        }

        #endregion // InvalidateResolvedProperties

        #endregion // Protected

        #endregion // Methods
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