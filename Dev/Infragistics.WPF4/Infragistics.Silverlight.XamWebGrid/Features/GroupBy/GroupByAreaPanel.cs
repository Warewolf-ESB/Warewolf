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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A panel used to arrange <see cref="GroupByHeaderCellControl"/> objects in the <see cref="XamGrid"/>.
	/// </summary>
	public class GroupByAreaPanel : Panel
	{
		#region Members

		Collection<GroupByChildInfo> _measuredItems;
		GroupByChildInfoComparer _comparer;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupByAreaPanel"/>
		/// </summary>
		public GroupByAreaPanel()
		{
			this._comparer = new GroupByChildInfoComparer();
			this._measuredItems = new Collection<GroupByChildInfo>();
		}

		#endregion // Constructor

		#region Overrides

		#region MeasureOverride
		/// <summary>
		/// Provides the behavior for the "measure" pass of Silverlight layout. Classes can override this method to define their own measure pass behavior.
		/// </summary>
		/// <param propertyName="availableSize">The available size that this object can give to child objects. Infinity can be specified as a value to indicate that the object will size to whatever content is available.</param>
		/// <returns>The size that this object determines it needs during layout, based on its calculations of child object allotted sizes.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			this._measuredItems.Clear();

			List<GroupByKey> keys = new List<GroupByKey>();

			double top = 0, left = 0;
			double height = 0, width = 0;

			foreach (UIElement element in this.Children)
			{
				int index = GroupByAreaPanel.GetIndex(element);
				GroupByKey key = new GroupByKey() { Level = GroupByAreaPanel.GetLevel(element), Key = GroupByAreaPanel.GetLevelKey(element) };

				element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

				int keyIndex = keys.IndexOf(key);
				if (keyIndex == -1)
				{
					keys.Add(key);
					key.Children = new List<GroupByChildInfo>();
				}
				else
					key = keys[keyIndex];

				key.Children.Add(new GroupByChildInfo() { Index = index, Element = element });
			}

			keys.Sort(this._comparer);

			foreach (GroupByKey key in keys)
			{
				List<GroupByChildInfo> list = key.Children;
				list.Sort(this._comparer);

				int count = list.Count;

				double maxHeight = 0;
				for(int i = 0; i < count; i++)
				{
					GroupByChildInfo info = list[i];

					GroupByHeaderCellControl ctrl = info.Element as GroupByHeaderCellControl;
					if (ctrl != null)
					{
						ctrl.Cell.ApplyStyle();
						// Is this the first item?
						ctrl.IsFirst = (i == 0);
						
						// Is this the last item?
						ctrl.IsLast = (i == count - 1);

						ctrl.UpdateState();
					}

					info.Left = left;
					info.Top = top;
					left += info.Element.DesiredSize.Width;
					maxHeight = Math.Max(info.Element.DesiredSize.Height, maxHeight);
					
					this._measuredItems.Add(info);
				}

				foreach (GroupByChildInfo info in list)
				{
					info.MaxHeight = maxHeight;
				}

				width = Math.Max(left, width);

				top += maxHeight + 5;

				height = top - 5;

				left = 0;
			}

			if (double.IsInfinity(availableSize.Height))
				availableSize.Height = height;

			if (double.IsInfinity(availableSize.Width))
				availableSize.Width = width;

			return availableSize;
		}
		#endregion // MeasureOverride

		#region ArrangeOverride
		/// <summary>
		/// Provides the behavior for the "arrange" pass of Silverlight layout. Classes can override this method to define their own arrange pass behavior.
		/// </summary>
		/// <param propertyName="finalSize">The final area within the parent that this object should use to arrange itself and its children.</param>
		/// <returns>The actual size used.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			int zIndex = this._measuredItems.Count;
			foreach (GroupByChildInfo info in this._measuredItems)
			{
				info.Element.Arrange(new Rect(info.Left, info.Top, info.Element.DesiredSize.Width, info.MaxHeight));
				Canvas.SetZIndex(info.Element, zIndex);
				zIndex--;
			}

			return base.ArrangeOverride(finalSize);
		}
		#endregion // ArrangeOverride

		#endregion // Overrides

		#region Properties

		#region LevelKey

		/// <summary>
		/// An attached property that Gets/Sets the LevelKey of the element that is being Grouped.
		/// </summary>
		public static readonly DependencyProperty LevelKeyProperty = DependencyProperty.RegisterAttached("LevelKey", typeof(string), typeof(GroupByAreaPanel), null);

		/// <summary>
		/// Gets the level key that element should be arranged by.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <returns></returns>
		public static string GetLevelKey(UIElement element)
		{
			return (string)element.GetValue(LevelKeyProperty);
		}

		/// <summary>
		/// Sets the level key that an element should be arranged by.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <param propertyName="key"></param>
		public static void SetLevelKey(UIElement element, string key)
		{
			element.SetValue(LevelKeyProperty, key);
		}

		#endregion // LevelKey

		#region Level

		/// <summary>
		/// An attached property that Gets/Sets the Level of the element that is being Grouped.
		/// </summary>
		public static readonly DependencyProperty LevelProperty = DependencyProperty.RegisterAttached("Level", typeof(int), typeof(GroupByAreaPanel), null);

		/// <summary>
		/// Gets the level an element should be arranged at.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <returns></returns>
		public static int GetLevel(UIElement element)
		{
			return (int)element.GetValue(LevelProperty);
		}

		/// <summary>
		/// Sets the level an element should be arranged at.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <param propertyName="level"></param>
		public static void SetLevel(UIElement element, int level)
		{
			element.SetValue(LevelProperty, level);
		}
			
		#endregion // Level

		#region Index

		/// <summary>
		/// An attached property that Gets/Sets the Index of the element that is being Grouped.
		/// </summary>
		public static readonly DependencyProperty IndexProperty = DependencyProperty.RegisterAttached("Index", typeof(int), typeof(GroupByAreaPanel), null);

		/// <summary>
		/// Gets the index an element should be arranged by.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <returns></returns>
		public static int GetIndex(UIElement element)
		{
			return (int)element.GetValue(IndexProperty);
		}

		/// <summary>
		/// Sets the index an element should be arranged by.
		/// </summary>
		/// <param propertyName="element"></param>
		/// <param propertyName="index"></param>
		public static void SetIndex(UIElement element, int index)
		{
			element.SetValue(IndexProperty, index);
		}

		#endregion // Index

		#endregion // Properties

		#region Private Classes

		#region GroupByChildInfoComparer

		/// <summary>
		/// A comparer used to Sort both GroupByChildInfo and GroupByKey objects.
		/// </summary>
		private class GroupByChildInfoComparer : IComparer<GroupByChildInfo>, IComparer<GroupByKey>
		{
			#region IComparer<GroupByChildInfo> Members

			public int Compare(GroupByChildInfo x, GroupByChildInfo y)
			{
				return x.Index.CompareTo(y.Index);
			}

			#endregion

			#region IComparer<GroupByKey> Members

			public int Compare(GroupByKey x, GroupByKey y)
			{
				int val = x.Level.CompareTo(y.Level);
				return (val == 0) ? string.Compare(x.Key, y.Key, StringComparison.CurrentCulture) : val;
			}

			#endregion
		}

		#endregion // GroupByChildInfoComparer

		#region GroupByKey
		
		/// <summary>
		/// An object used to organize data by it's keys.
		/// </summary>
		private class GroupByKey 
		{
			public int Level
			{
				get;
				set;
			}

			public string Key
			{
				get;
				set;
			}

			public List<GroupByChildInfo> Children
			{
				get;
				set;
			}

			public override bool Equals(object obj)
			{
				GroupByKey key = obj as GroupByKey;

				if(key == null)
					return false;

				return (this.Level == key.Level && this.Key == key.Key);
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		#endregion // GroupByKey

		#region GroupByChildInfo
		/// <summary>
		/// An object used store infomration about an element in this panel
		/// </summary>
		private class GroupByChildInfo
		{
			public int Index
			{
				get;
				set;
			}

			public UIElement Element
			{
				get;
				set;
			}

			public double Top
			{
				get;
				set;
			}

			public double Left
			{
				get;
				set;
			}

			public double MaxHeight
			{
				get;
				set;
			}
		}
		#endregion // GroupByChildInfo

		#endregion // Private Classes

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