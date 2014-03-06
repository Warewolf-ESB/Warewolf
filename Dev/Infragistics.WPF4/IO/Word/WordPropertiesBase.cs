using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;


using System.Windows;
using System.Windows.Media;





namespace Infragistics.Documents.Word
{    
    #region WordPropertiesBase class
    /// <summary>
    /// Base class for objects which represent the properties of a
    /// Word entity such as a Paragraph, Table, TableRow, TableCell, etc.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public abstract class WordPropertiesBase
    {
        #region Member variables

        internal IUnitOfMeasurementProvider     unitOfMeasurementProvider = null;

        #endregion Member variables

        #region Constructor
        internal WordPropertiesBase( IUnitOfMeasurementProvider provider )
        {
            this.unitOfMeasurementProvider = provider;
        }
        #endregion Constructor

        #region Properties

        #region Unit
        internal UnitOfMeasurement Unit { get { return this.unitOfMeasurementProvider != null ? this.unitOfMeasurementProvider.Unit : WordUtilities.DefaultUnitOfMeasurement; } }
        #endregion Unit

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The Reset method restores th object to the state it was
        /// in when it was first instantiated. When used within the context of the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// class, calling the Reset method on an existing instance is recommended over
        /// instantiation of a new object since the current state of this object has no
        /// bearing on content that was produced at an earlier time, as the content
        /// has already been persisted.
        /// </p>
        /// </remarks>
        public abstract void Reset();
        #endregion Reset

        #endregion Methods
    }
    #endregion TableProperties class
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