using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// Custom structure that represents a position (offset and extent). Note the End is exclusive
	/// </summary>
	internal struct ItemPosition : IEquatable<ItemPosition>
	{
		#region Member Variables

		public double Start;
		public double Extent;

		#endregion // Member Variables

		#region Constructor
		public ItemPosition(double offset, double extent)
		{
			this.Start = offset;
			this.Extent = extent;
		}
		#endregion // Constructor

		#region Base class overrides
		public override int GetHashCode()
		{
			return Start.GetHashCode() ^ Extent.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is ItemPosition)
				return this.Equals((ItemPosition)obj);

			return false;
		}

		public override string ToString()
		{
			return string.Format("{0}x{1}", this.Start, this.Extent);
		}
		#endregion // Base class overrides

		#region Properties
		/// <summary>
		/// The exclusive end point for the structure
		/// </summary>
		public double End
		{
			get { return this.Start + this.Extent; }
		}
		#endregion // Properties

		#region Methods
		public void Union(ItemPosition position)
		{
			double end = Math.Max(position.Start + position.Extent, this.Start + this.Extent);

			if (position.Start < this.Start)
				this.Start = position.Start;

			this.Extent = Math.Max(end - this.Start, 0);
		}

		public bool IntersectsWith(ItemPosition position)
		{
			// if this is a 0 pixel position then we don't want to exclude the end
			if (CoreUtilities.AreClose(position.Start, position.End))
				return !(CoreUtilities.GreaterThanOrClose(position.Start, this.End) || CoreUtilities.LessThan(position.End, this.Start));
				
			// the end is exclusive
			return !(CoreUtilities.GreaterThanOrClose(position.Start, this.End) || CoreUtilities.LessThanOrClose(position.End, this.Start));
		}
		#endregion // Methods

		#region IEquatable<ItemPosition> Members

		public bool Equals(ItemPosition other)
		{
			return this.Start == other.Start &&
				this.Extent == other.Extent;
		}

		#endregion //IEquatable<ItemPosition> Members
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